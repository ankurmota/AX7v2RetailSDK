/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Requests {
    "use strict";

    /**
     * Represents a request to the Retail Server Commerce endpoint.
     */
    export class RetailServerRequest extends Requests.ODataRequestBase {
        private static PREFER_HEADER_NAME: string = "Prefer";
        private static REQUEST_ID_HEADER_NAME: string = "RequestId";
        private static APP_SESSION_ID_HEADER_NAME: string = "AppSessionId";
        private static USER_SESSION_ID_HEADER_NAME: string = "UserSessionId";

        private static AUTHENTICATION_TOKEN_NAME: string = "Authorization";
        private static AUTHENTICATION_TOKEN_FORMAT: string = "{0} {1}";

        private static DEVICE_TOKEN_NAME: string = "DeviceToken";
        private static DEVICE_TOKEN_FORMAT: string = "{1}";
        private static COMMERCE_RUNTIME_TOKEN_TYPE: string = "commerceruntime_token";
        private static OFFLINE_SYNC_API_NAME: string = "GetTerminalDataStoreName";

        private requestId: string;

        /**
         * Initializes a new instance of the RetailServerRequest class.
         * @param {string} serverUri the server uri.
         * @param {IDataServiceQueryInternal} the protocol agnostic query.
         * @param {string} locale the locale to be set for the request.
         */
        constructor(serverUri: string, query: Common.IDataServiceQueryInternal, locale: string) {
            super(serverUri, query, locale);
        }

        /**
         * Executes the request.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(): IAsyncResult<T> {

            // disable APIs that are being replaced by commerce authentication endpoint
            if (ArrayExtensions.hasElement(["elevateuser", "reverttoself"], this.query.action.toLowerCase())) {
                return AsyncResult.createResolved(null);
            }

            var asyncResult: AsyncResult<T> = new AsyncResult<T>();
            if (!this.query.isReturnTypeACollection || this.query.resultSettings.Paging.Top >= 0) {
                this.loadPage<T>(asyncResult);
            } else {
                this.getAllPages<T>(asyncResult, 0);
            }

            // handle redirection, if needed
            return asyncResult.recoverOnFailure(this.handleRedirection.bind(this));
        }

        /**
         * Parses the error result.
         * @param {Common.IXmlHttpError} errorResponse the error response.
         * @param {Model.Entities.Error[]} the errors from the response.
         */
        protected parseErrorResponse(errorResponse: Common.IXmlHttpError): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = super.parseErrorResponse(errorResponse);

            // since EndSession is part of the error handling process, we don't want to handle the authentication error
            // for that
            if (this.query.action.toLowerCase() !== "endsession") {
                ErrorHandler.authenticationErrorHandler(errors);
            }

            return errors;
        }

        /**
         * Gets the headers to be added to the request.
         * @returns {IAsyncResult<{ [headerName: string]: string }>} a promise for an object whose properties represent header values.
         */
        protected getRequestHeaders(): IAsyncResult<{ [headerName: string]: string }> {
            var headers: { [headerName: string]: string };

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return super.getRequestHeaders().done((headersResponse: { [headerName: string]: string }): void => {
                    headers = headersResponse;
                });
            }).enqueue((): IVoidAsyncResult => {
                // Add prefered RS headers
                headers[RetailServerRequest.PREFER_HEADER_NAME] = "return=representation";

                // Add instrumentation headers.
                headers[RetailServerRequest.REQUEST_ID_HEADER_NAME] = this.requestId;
                headers[RetailServerRequest.APP_SESSION_ID_HEADER_NAME] = Microsoft.Dynamics.Diagnostics.TypeScriptCore.LoggerBase.getAppSessionId();
                headers[RetailServerRequest.USER_SESSION_ID_HEADER_NAME] = Microsoft.Dynamics.Diagnostics.TypeScriptCore.LoggerBase.getUserSessionId();

                return VoidAsyncResult.createResolved();
            }).enqueue((): IVoidAsyncResult => {
                // Add user token to header
                return this.addAuthenticationTokenToHeader(
                    headers,
                    RetailServerRequest.AUTHENTICATION_TOKEN_NAME,
                    RetailServerRequest.AUTHENTICATION_TOKEN_FORMAT,
                    Authentication.AuthenticationProviderResourceType.USER);
            }).enqueue((): IVoidAsyncResult => {
                // Add device token to header
                return this.addAuthenticationTokenToHeader(
                    headers,
                    RetailServerRequest.DEVICE_TOKEN_NAME,
                    RetailServerRequest.DEVICE_TOKEN_FORMAT,
                    Authentication.AuthenticationProviderResourceType.DEVICE);
            }).run().map((): { [headerName: string]: string } => {
                return headers;
            });
        }

        /**
         * Gets the Request Uri for this request.
         * @returns {string} the request uri.
         */
        protected getRequestUri(): string {
            var requestUri: string = super.getRequestUri();

            if (ArrayExtensions.hasElements(this.query.expands)) {
                requestUri = this.addParameter(requestUri, "$expand", this.query.expands.join());
            }

            if (!StringExtensions.isNullOrWhitespace(Proxy.CommerceContext.ApiVersion)) {
                requestUri = this.addParameter(requestUri, "api-version", Proxy.CommerceContext.ApiVersion);
            }

            return requestUri;
        }

        /**
         * Executes request for one page.
         * @param {AsyncResult<T>} The async result.
         * @param {number?} skip the skip value.
         */
        private loadPage<T>(asyncResult: AsyncResult<T>, skip?: number): void {
            if (!ObjectExtensions.isNullOrUndefined(skip)) {
                this.query.resultSettings.Paging.Skip = skip;
            }

            if (this.query.isReturnTypeACollection) {
                this.query.resultSettings.Paging.Top = this.query.resultSettings.Paging.Top || Config.defaultPageSize;
            }

            this.requestId = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
            RetailLogger.modelManagersRetailServerRequestStarted(this.requestId, this.requestUri);

            // This is the pipe for all OData API calls.
            super.execute<T>().done((result: T) => {
                RetailLogger.modelManagersRetailServerRequestFinished(this.requestId, this.requestUri);
                asyncResult.resolve(result);
            }).fail((errors: Model.Entities.Error[]) => {
                // Do not log error if offline sync API call failed when MPOS is offline.
                if (Commerce.Session.instance.connectionStatus === ConnectionStatusType.Online
                      || this.requestUri.search(RetailServerRequest.OFFLINE_SYNC_API_NAME) === -1) {
                    RetailLogger.modelManagersRetailServerRequestError(this.requestId, this.requestUri, ErrorHelper.getErrorMessages(errors));
                }
                asyncResult.reject(errors);
            });
        }

        /**
         * Executes paginated requests for all entities.
         * @param {AsyncResult{T}} asyncResult The async result.
         * @param {number} skip The offset.
         * @param {Array<T>} The array result.
         */
        private getAllPages<T>(asyncResult: AsyncResult<T>, skip: number, results?: Array<T>): void {
            skip = skip || 0;
            var top: number = Config.defaultPageSize;
            var getPageAsyncResult: AsyncResult<T> = new AsyncResult<T>();
            this.query.resultSettings.Paging.Top = top;
            results = results || [];

            getPageAsyncResult.done((pageResult: T) => {
                if (pageResult) {
                    if (typeof (<any>pageResult).length === "number") { // The pageResult is an array
                        results = results.concat(<any>pageResult);

                        if ((<any>pageResult).length < top) {  // This page contains less items than top
                            if ((<any>pageResult).hasNextPage) {  // This means the pagesize defined on server side is smaller than the top
                                skip += (<any>pageResult).length;
                                this.getAllPages<T>(asyncResult, skip, results);  // Try to fetch the next page
                            } else {  // No more pages need to fetch
                                asyncResult.resolve(<T><any>results);
                            }
                        } else {  // This page contains exact amount of item as the top, should not contain more
                            skip += top;
                            this.getAllPages<T>(asyncResult, skip, results);  // Try to fetch the next page
                        }
                    } else {   // The pageResult is a single item
                        results.push(pageResult);
                        asyncResult.resolve(<T><any>results);
                    }
                }
            }).fail((errors: Model.Entities.Error[]) => {
                asyncResult.reject(errors);
            });

            this.loadPage(getPageAsyncResult, skip);
        }

        /**
         * Adds to the header object an authentication token.
         * @param {{ [headerName: string]: string }} header the header object.
         * @param {string} headerName the header name.
         * @param {string} headerValueFormat the header value format.
         * @param {string} authenticationResourceType the authentication token resource type.
         * @returns {IVoidAsyncResult} a promise indicating completion of the operation.
         */
        private addAuthenticationTokenToHeader(
            header: { [headerName: string]: string },
            headerName: string,
            headerValueFormat: string,
            authenticationResourceType: string): IVoidAsyncResult {
            return Commerce.Authentication.AuthenticationProviderManager.instance.acquireToken(authenticationResourceType)
                .done((token: Authentication.IAuthenticationToken): void => {
                    if (!ObjectExtensions.isNullOrUndefined(token) && !StringExtensions.isNullOrWhitespace(token.token)
                        && token.tokenType !== RetailServerRequest.COMMERCE_RUNTIME_TOKEN_TYPE) {
                        header[headerName] = StringExtensions.format(headerValueFormat, token.tokenType, token.token);
                    }
                });
        }

        /**
         * Redirects to another Retail Server if requested.
         * @param {Entities.Error[]} errors the error collection returned by server.
         * @return {IAsyncResult<T>} The async result if redirection can be handled, null otherwise.
         */
        private handleRedirection<T>(errors: Entities.Error[]): IAsyncResult<T> {
            // try to find redirection error, if any
            if (ArrayExtensions.hasElements(errors) && !StringExtensions.compare(errors[0].ErrorCode, ErrorTypeEnum.RETAIL_SERVER_REDIRECT_ERROR)) {
                var redirectUrl: string = errors[0].extraData;

                RetailLogger.retailServerRequestRedirection(redirectUrl);
                ApplicationContext.updateServerUrl(redirectUrl);

                // reissue the current request against new server
                return new RetailServerRequest(redirectUrl, this.query, this.locale).execute();
            }

            // nothing to do
            return null;
        }
    }
}