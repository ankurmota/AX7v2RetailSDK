/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../RetailLogger.d.ts'/>

module Commerce.Proxy.Requests {
    "use strict";

    /**
     * Defines the type for a token issued by the CommerceRuntime.
     */
    interface ICommerceRuntimeToken {
        // names here must follow proxy case definition
        
        Token: string;
        SchemeName: string;
        
    }

    /**
     * Represents a commerce data service request object.
     */
    export class CommerceRuntimeRequest implements Common.IDataServiceRequest {
        private static AUTHENTICATION_REQUEST_TYPE: string = "Authentication";
        private static SET_AUTHENTICATION_TOKENS_ACTION: string = "SetAuthenticationTokens";
        private static TOKEN_ACTION_NAME: string = "token";
        private static COMMERCE_RUNTIME_REQUEST_ERROR_PREFIX: string = "CommerceRuntimeRequestError_";
        private static COMMERCE_RUNTIME_REQUEST_RESPONSE_PREFIX: string = "CommerceRuntimeRequestResponse_";

        /**
         * The type identifier given to a token issued by the commerce runtime.
         */
        private static COMMERCE_RUNTIME_TOKEN_TYPE: string = "commerceruntime_token";

        private static currentCrtUserToken: string = null;
        private static currentDeviceToken: string = null;

        private _connectionUri: string;
        private _query: Common.IDataServiceQueryInternal;

        private _uri: string;
        private _id: number;
        private _locale: string;

        /**
         * Gets the locale for current request.
         */
        public get locale(): string {
            return this._locale;
        }

        /**
         * Sets the locale for current request.
         */
        public set locale(locale: string) {
            this._locale = locale;
        }

        constructor(connectionUri: string, query: Common.IDataServiceQueryInternal, locale: string) {
            this._connectionUri = connectionUri;
            this._locale = locale;
            this._query = query;

            var formatedParameters: string = this.formatParameters(this._query);

            this._uri = StringExtensions.format("{0}/{1}Manager/{2}{3}",  // baseUri/Manager/Action?Parameters
                                                            this._connectionUri,
                                                            this._query.entityType || "StoreOperations",
                                                            this._query.action, formatedParameters ? "?" + formatedParameters : "");
        }

        private static encodeParameters(parameters: { [parameterName: string]: any }): string[] {
            var formattedParameters: string[] = [];

            ObjectExtensions.forEachKeyValuePair(parameters, (propertyName: string, propertyValue: any): void => {
                    formattedParameters.push(propertyName + "=" +
                        (ObjectExtensions.isUndefined(propertyValue)
                            ? ""
                            : encodeURIComponent(JSON.stringify(propertyValue))));
            });

            return formattedParameters;
        }

        private static acquireToken(tokenType: string): IAsyncResult<Authentication.IAuthenticationToken> {
            return Commerce.Authentication.AuthenticationProviderManager.instance.acquireToken(tokenType);
        }

        /**
         * Checks whether the query is an authentication request.
         * @param {Common.IDataServiceQueryInternal} query the request being checked.
         * @returns {boolean} a value indicating whether the query is an authentication request.
         */
        private static isAuthenticationRequest(query: Common.IDataServiceQueryInternal): boolean {
            return query.entityType === CommerceRuntimeRequest.AUTHENTICATION_REQUEST_TYPE;
        }

        /**
         * Gets the Request Identifier.
         * Auto-generated unique identifier if executed in batch. Used to fetch the response from batch result array.
         */
        public id(): number {
            return this._id;
        }


        /**
         * Execute the request.
         * @return {IAsyncResult<T>} The async result.
         */
        public execute<T>(): IAsyncResult<T> {
            var result: T = null;

            return new AsyncQueue().enqueue(() => {
                return this.populateAuthenticationParameters();
            }).enqueue(() => {
                return this.executeRequest().done((executionResult: T) => {
                    result = executionResult;
                });
            }).enqueue(() => {
                return this.postExecuteRequest(result);
            }).run().map((): T => {
                return result;
            });
        }

        /**
         * Executes the batch requests.
         * @param {CommerceRuntimeRequest[]} requests The collection of requests to execute.
         * @return {IAsyncResult<Array>} The async result.  Responses at index I correlates to request with identifier I.
         */
        public executeBatch(requests: CommerceRuntimeRequest[]): IAsyncResult<any[]> {
            var asyncResult: AsyncResult<any[]> = new AsyncResult<any[]>();
            var requestId: number = 1;
            var responses: any[] = new Array();
            var errors: Model.Entities.Error[] = [];

            ObjectExtensions.forEachAsync(requests,
                (request: CommerceRuntimeRequest, next: any) => {
                    request._id = requestId++;

                    request.execute()
                        .done((result: any) => {
                            responses[request._id] = result;
                            next();
                        })
                        .fail((errorResults: Model.Entities.Error[]) => {
                            errors = errors.concat(errorResults);
                            next();
                        });
                },
                () => {
                    if (errors.length === 0) {
                        asyncResult.resolve(responses);
                    } else {
                        asyncResult.reject(errors);
                    }
                });

            return asyncResult;
        }

        /**
         * Execute the request.
         * @return {IAsyncResult<T>} The async result.
         */
        private executeRequest<T>(): IAsyncResult<T> {
            var asyncResult: AsyncResult<T> = new AsyncResult<T>();
            var self: CommerceRuntimeRequest = this;
            var timeoutHandle: number = 0;
            var executionCompleted: boolean = false;
            var requestId: string = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();

            // We currently pass all the request data (including potentially sensitive information)
            // through the request string.  In order to ensure we don't log anything that is sensitive,
            // we only log the base URL of the request.
            var requestUri: string = this._uri.split("?")[0];
            RetailLogger.modelManagersCommerceRuntimeRequestStarted(requestId, requestUri);

            var handleTimeout: () => void = (): void => {
                if (!executionCompleted) {
                    executionCompleted = true;

                    var errors: Model.Entities.Error[] = [
                        new Entities.Error(ErrorTypeEnum.SERVER_TIMEOUT)
                    ];
                    RetailLogger.modelManagersCommerceRuntimeRequestError(requestId, requestUri, requestUri + " timed out");

                    asyncResult.reject(errors);
                }
            };

            var resolveOrRejectResult: (response: any, isSuccess?: boolean) => void =
                (response: any, isSuccess: boolean = false): void => {
                    if (!executionCompleted) {
                        executionCompleted = true;
                        clearTimeout(timeoutHandle);

                        if (isSuccess) {
                            asyncResult.resolve(response ? Requests.ODataRequestBase.parseODataResult(JSON.parse(response), self._query.returnType) : response);
                            RetailLogger.modelManagersCommerceRuntimeRequestFinished(requestId, requestUri);
                        } else {
                            var errors: Entities.Error[] = Context.ErrorParser.parseJSONError(response);
                            asyncResult.reject(errors);

                            if (errors[0].ErrorCode.toUpperCase() !==
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED.serverErrorCode) {
                                RetailLogger.modelManagersCommerceRuntimeRequestError(requestId, requestUri, response);
                            }
                        }
                    }
                };

            try {
                Microsoft.Dynamics.Commerce.ClientBroker.CommerceRuntimeRequest.executeAsync(this._uri, requestId)
                    .done(function (response: string): void {
                        var isSuccess: boolean = response.indexOf(CommerceRuntimeRequest.COMMERCE_RUNTIME_REQUEST_RESPONSE_PREFIX) === 0;
                        if (isSuccess) {
                            response = response.substr(CommerceRuntimeRequest.COMMERCE_RUNTIME_REQUEST_RESPONSE_PREFIX.length);
                        } else if (response.indexOf(CommerceRuntimeRequest.COMMERCE_RUNTIME_REQUEST_ERROR_PREFIX) === 0) {
                            response = response.substr(CommerceRuntimeRequest.COMMERCE_RUNTIME_REQUEST_ERROR_PREFIX.length);
                        }

                        resolveOrRejectResult(response, isSuccess);
                    },
                    (error: any) => {
                        resolveOrRejectResult(error);
                    });
            } catch (error) {
                resolveOrRejectResult(error);
            }

            // Set timeout if configured.
            if (Commerce.Config.connectionTimeout > 0) {
                timeoutHandle = setTimeout(handleTimeout, Commerce.Config.connectionTimeout * 1000); // Configured in seconds.
            }

            return asyncResult;
        }

        /**
         * Populate authentication parameters.
         * @returns {IVoidAsyncResult} the promise for completion.
         */
        private populateAuthenticationParameters(): IVoidAsyncResult {
            // do not try to set token for the SET_AUTHENTICATION_TOKENS_ACTION (to avoid recursion)
            if (this._query.action !== CommerceRuntimeRequest.SET_AUTHENTICATION_TOKENS_ACTION) {
                var deviceToken: string;
                var userToken: string;

                return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                    // get tokens from providers
                    return VoidAsyncResult.join([
                        CommerceRuntimeRequest.acquireToken(Authentication.AuthenticationProviderResourceType.DEVICE)
                            .done((token: Authentication.IAuthenticationToken): void => {
                                deviceToken = ObjectExtensions.isNullOrUndefined(token) ? null : token.token;
                            }),
                        CommerceRuntimeRequest.acquireToken(Authentication.AuthenticationProviderResourceType.USER)
                            .done((token: Authentication.IAuthenticationToken): void => {
                                if (ObjectExtensions.isNullOrUndefined(token) || StringExtensions.isNullOrWhitespace(token.token)) {
                                    // if token is null, it means user is not authenticated
                                    userToken = null;
                                } else if (token.tokenType === CommerceRuntimeRequest.COMMERCE_RUNTIME_TOKEN_TYPE) {
                                    // if token is offline, use it to update underlying proxy; otherwise keep using current offline token
                                    userToken = token.token;
                                } else {
                                    // if it is a valid token, but not an offline one, then keep the current offline token in context
                                    userToken = CommerceRuntimeRequest.currentCrtUserToken;
                                }
                            })
                    ]);
                }).enqueue((): IVoidAsyncResult => {

                    // since IPC calls are expensive, don't call set tokens unless tokens have changed
                    if (CommerceRuntimeRequest.currentDeviceToken !== deviceToken || CommerceRuntimeRequest.currentCrtUserToken !== userToken) {
                        // create a request to set the tokens and execute it
                        return (new CommerceRuntimeRequest(
                            this._connectionUri,
                            {
                                entityType: CommerceRuntimeRequest.AUTHENTICATION_REQUEST_TYPE,
                                action: CommerceRuntimeRequest.SET_AUTHENTICATION_TOKENS_ACTION,
                                data: {
                                    userToken: userToken,
                                    deviceToken: deviceToken
                                }
                            },
                            this._locale)).execute();
                    }

                    return VoidAsyncResult.createResolved();
                }).enqueue((): IVoidAsyncResult => {
                    // update current values
                    CommerceRuntimeRequest.currentDeviceToken = deviceToken;
                    CommerceRuntimeRequest.currentCrtUserToken = userToken;
                    return VoidAsyncResult.createResolved();
                }).run();
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Callback that runs after successfull execution.
         * @param {T} result the result from the execution.
         * @returns {IVoidAsyncResult} the promise for completion.
         */
        private postExecuteRequest<T>(result: T): IVoidAsyncResult {
            if (this._query.action.toLowerCase() === CommerceRuntimeRequest.TOKEN_ACTION_NAME) {
                // for the token API, CRT returns a different type
                // adapt it to the expected type
                var commerceRuntimeToken: ICommerceRuntimeToken = <ICommerceRuntimeToken><any>result;
                var commerceToken: Model.Entities.Authentication.ICommerceToken = <Model.Entities.Authentication.ICommerceToken><any>result;
                commerceToken.id_token = commerceRuntimeToken.Token;
                commerceToken.token_type = commerceRuntimeToken.SchemeName;

                // keep token locally for offline requests
                CommerceRuntimeRequest.currentCrtUserToken = commerceToken.id_token;
            }

            return VoidAsyncResult.createResolved();
        }

        private formatParameters(query: Common.IDataServiceQueryInternal): string {
            var result: string = null;
            var formattedParameters: string[] = [];

            if (query.isReturnTypeACollection) {
                formattedParameters.push("queryResultSettings=" + encodeURIComponent(JSON.stringify(query.resultSettings)));
            }

            if (query.key && query.action !== "Update") {
                ObjectExtensions.forEachKeyValuePair(query.key,
                    (propertyName: string, propertyValue: any): void => {
                        formattedParameters.push(propertyName + "=" + encodeURIComponent(JSON.stringify(propertyValue)));
                    });
            }

            if (query.data) {
                if (query.data instanceof Common.ODataOperationParameters) {
                    formattedParameters = formattedParameters.concat(CommerceRuntimeRequest.encodeParameters(query.data.parameters));
                } else if (CommerceRuntimeRequest.isAuthenticationRequest(query)) {
                    formattedParameters = formattedParameters.concat(CommerceRuntimeRequest.encodeParameters(query.data));
                } else {
                    formattedParameters.push("entity=" + encodeURIComponent(JSON.stringify(query.data)));
                }
            }

            // Add locale
            if (!StringExtensions.isNullOrWhitespace(this._locale)) {
                formattedParameters.push("$locale=" + this._locale);
            }

            if (formattedParameters.length > 0) {
                result = formattedParameters.join("&");
            }

            return result;
        }
    }
}

declare module Microsoft.Dynamics.Commerce.ClientBroker {
    class CommerceRuntimeRequest {
        static tryAddConfiguration(configurationName: string, connectionString: string, isMasterDatabaseConnectionString: boolean): boolean;
        static executeAsync(request: string, requestId: string): any;
    }
}
