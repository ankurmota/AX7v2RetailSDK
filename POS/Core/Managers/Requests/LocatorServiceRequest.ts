/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ODataRequestBase.ts'/>

module Commerce.Proxy.Requests {
    "use strict";

    /**
     * Represents a request to the Locator Service endpoint.
     */
    export class LocatorServiceRequest extends ODataRequestBase {
        private static ACTIVITY_ID_HEADER_NAME: string = "ActivityId";
        private static AUTHENTICATION_HEADER_NAME: string = "Authorization";
        private static AUTHENTICATION_TOKEN_FORMAT: string = "{0} {1}";
        private activityId: string;

        /**
         * Initializes a new instance of the LocatorServiceRequest class.
         * @param {string} serverUri the server uri.
         * @param {IDataServiceQueryInternal} the protocol agnostic query.
         */
        constructor(serverUri: string, query: Common.IDataServiceQueryInternal) {
            var locale: string = "";
            super(serverUri, query, locale);

            this.enableCrossDomainCookies = false;
        }

        /**
         * Execute the request.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(): IAsyncResult<T> {
            this.activityId = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
            RetailLogger.modelManagersLocatorServiceRequestStarted(this.requestUri, this.activityId);

            return super.execute<T>().done(() => {
                RetailLogger.modelManagersLocatorServiceRequestFinished(this.activityId);
            }).fail((errors: Model.Entities.Error[]) => {
                RetailLogger.modelManagersLocatorServiceRequestException(ErrorHelper.getErrorMessages(errors), this.activityId);
            });
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
                headers[LocatorServiceRequest.ACTIVITY_ID_HEADER_NAME] = this.activityId;
                return VoidAsyncResult.createResolved();
            }).enqueue((): IVoidAsyncResult => {
                // add authentication token to the header
                return Authentication.AuthenticationProviderManager.instance.acquireToken(Authentication.AuthenticationProviderResourceType.LOCATOR_SERVICE)
                    .done((authenticationToken: Authentication.IAuthenticationToken): void => {
                        if (!ObjectExtensions.isNullOrUndefined(authenticationToken)) {
                            headers[LocatorServiceRequest.AUTHENTICATION_HEADER_NAME] = StringExtensions.format(
                                LocatorServiceRequest.AUTHENTICATION_TOKEN_FORMAT,
                                authenticationToken.tokenType,
                                authenticationToken.token);
                        }
                    });
            }).run().map((): { [headerName: string]: string } => {
                return headers;
            });
        }
    }
} 