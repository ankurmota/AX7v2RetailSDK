/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Host {
    "use strict";

    /**
     * Provides a way to authenticate users using Azure Active Directory.
     */
    export class WebAADAuthenticationAdapter implements IAzureActiveDirectoryAdapter {
        private static AAD_EXTRA_QUERY_PARAMETERS: string = "nux=1"; // uses  new login page in aad
        private static AAD_LOCALCACHE: string = "sessionStorage"; // enables adaljs to store token on session storage
        private static USER_ACCOUNT_IDENTIFIER_NOT_PROVIDED_REGEX: RegExp = /AADSTS50058/i;
        private static SESSION_IS_INVALID_REGEX: RegExp = /AADSTS16000/i;
        private _aadContext: AuthenticationContext;

        /*
         * Initializes the authentication provider.
         */
        public initialize(): void {
            // get current url without query string or anchors
            var redirectUrl: string = [location.protocol, "//", location.host, location.pathname].join("");

            if (!StringExtensions.isNullOrWhitespace(Commerce.Config.aadClientId)) {
                var aadLoginUrl: string = Commerce.Config.aadLoginUrl || "";
                if (!StringExtensions.endsWith(aadLoginUrl, "/", false)) {
                    aadLoginUrl += "/";
                }

                this._aadContext = new AuthenticationContext(<any>{
                    clientId: Commerce.Config.aadClientId,
                    redirectUri: redirectUrl,
                    postLogoutRedirectUri: redirectUrl,
                    instance: aadLoginUrl,
                    cacheLocation: WebAADAuthenticationAdapter.AAD_LOCALCACHE,
                    extraQueryParameter: WebAADAuthenticationAdapter.AAD_EXTRA_QUERY_PARAMETERS
                });

                // AAD navigates back to app with auth token
                // this method parsers that token and keep it on
                // session / local storage
                this._aadContext.handleWindowCallback();
            }
        }

        /*
         * Authenticates user through the authentication service.
         * @returns {IAsyncResult<IUserDetails>} the async result containing the authenticated user information.
         */
        public login(): IAsyncResult<IUserDetails> {
            this._checkAADContextInitialized();

            var result: AsyncResult<IUserDetails> = new AsyncResult<IUserDetails>();

            var requestId: string = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();

            this._aadContext.getUser((error: string, user: any): void => {
                if (!user) {
                    // Set flag to Application storage that we are on process of AAD Logon
                    ApplicationStorage.setItem(ApplicationStorageIDs.AAD_LOGON_IN_PROCESS_KEY, "true");
                    RetailLogger.librariesAuthenticationProviderLoginStarted(requestId, "Navigating to AAD Login page to request user credentials.");

                    // this breaks the flow and navigates to AAD webpage
                    // due to that, we cannot resolve the result at this point
                    try {
                        RetailLogger.librariesAuthenticationProviderLoginFinished(requestId, "Navigating away to AAD login page now.");
                        this._aadContext.login();
                    } catch (error) {
                        var errorMessage: string = (error || "").toString();
                        RetailLogger.genericError(errorMessage);
                        result.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_AUTHENTICATION_FAILED)]);
                    }
                } else {
                    RetailLogger.librariesAuthenticationProviderLoginStarted(requestId, "User identification token already present in context.");

                    user.profile = user.profile || {};

                    var userDetails: IUserDetails = {
                        userName: user.userName,
                        tenantId: user.profile.tid,
                        objectId: user.profile.oid,
                        fullName: user.profile.name
                    };

                    RetailLogger.librariesAuthenticationProviderLoginFinished(requestId, StringExtensions.EMPTY);
                    result.resolve(userDetails);
                }
            });

            return result;
        }

        /*
         * Acquires the token thought the authentication service.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {IAsyncResult<string>} the async result containing the access token.
         */
        public acquireToken(resourceId: string): IAsyncResult<string> {
            this._checkAADContextInitialized();

            var result: AsyncResult<string> = new AsyncResult<string>();

            this.login().done((user: IUserDetails): void => {
                var requestId: string = Microsoft.Dynamics.Diagnostics.TypeScriptCore.Utils.generateGuid();
                RetailLogger.librariesAuthenticationProviderAcquireTokenStarted(requestId, resourceId);

                this._aadContext.acquireToken(resourceId, (error: string, token: string): void => {
                    if (!StringExtensions.isNullOrWhitespace(error)) {
                        var errorMessage: string = "AAD service failed with message: " + error;
                        RetailLogger.genericError(errorMessage);
                        if (WebAADAuthenticationAdapter.USER_ACCOUNT_IDENTIFIER_NOT_PROVIDED_REGEX.test(error)) {
                            result.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_USER_ACCOUNT_IDENTIFIER_NOT_PROVIDED)]);
                        } else if (WebAADAuthenticationAdapter.SESSION_IS_INVALID_REGEX.test(error)) {
                            // since the session is invalid we clear the cache and logout
                            this.clearCache();
                            this.logout().always(() => {
                                result.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_AUTHENTICATION_FAILED)]);
                            });
                        } else {
                            result.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_AUTHENTICATION_FAILED)]);
                        }
                    } else {
                        RetailLogger.librariesAuthenticationProviderAcquireTokenFinished(requestId);
                        // success
                        result.resolve(token);
                    }
                });
            }).fail((errors: Model.Entities.Error[]): void => {
                result.reject(errors);
            });

            return result;
        }

        /*
         * Clears any access tokens present on the application and logs the user out of the authentication service.
         * @returns {IVoidAsyncResult} the async result indicating when the logout process completes.
         */
        public logout(): IVoidAsyncResult {
            this._checkAADContextInitialized();

            this._aadContext.logOut();
            return VoidAsyncResult.createResolved();
        }

        /*
         * Retrieves a token if available. If the token is not available, null is returned.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {string} the token for the resource or null if token is not available.
         */
        public retrieveAvailableToken(resourceId: string): string {
            return this._aadContext
                ? (this._aadContext.getCachedToken(resourceId) || null)
                : null;
        }

        /**
         * Clears local cache used to keep user and token data.
         */
        public clearCache(): void {
            this._checkAADContextInitialized();
            this._aadContext.clearCache();
        }

        /**
         * Checks whether the AAD Context was initialized.
         */
        private _checkAADContextInitialized(): void {
            if (ObjectExtensions.isNullOrUndefined(this._aadContext)) {
                throw new Error("The AAD context was not initialized. Please check your AAD configuration settings.");
            }
        }
    }
}
