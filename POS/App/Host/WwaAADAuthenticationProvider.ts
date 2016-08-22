/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../SharedApp/ActiveDirectory.d.ts' />

module Commerce.Host {
    "use strict";

    /**
     * Provides a way to authenticate users using Azure Active Directory.
     */
    export class WwaAADAuthenticationAdapter implements IAzureActiveDirectoryAdapter {
        private static AAD_EXTRA_QUERY_PARAMETERS: string = "nux=1"; // uses  new login page in aad
        private static AAD_REDIRECT_URL: Windows.Foundation.Uri = new Windows.Foundation.Uri("urn:ietf:wg:-Oauth:2.0:-Oob");
        private static AAD_LOGOUT_URL_PART: string = "oauth2/logout";
        private static WEBAUTHENTICATIONBROKER_TIMEOUT_ERROR_ID: number = -2147023436;
        private _aadContext: Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
        private _addLogoutUri: Windows.Foundation.Uri;

        /*
         * Initializes the authentication provider.
         */
        constructor() {
            if (!StringExtensions.isNullOrWhitespace(Commerce.Config.aadLoginUrl)) {
                this._aadContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(
                    Commerce.Config.aadLoginUrl,
                    true);

                var aadLoginUrl: string = Commerce.Config.aadLoginUrl;

                // make sure the base uri has a trailing slash
                if (aadLoginUrl.charAt(aadLoginUrl.length - 1) !== "/") {
                    aadLoginUrl = aadLoginUrl + "/";
                }

                this._addLogoutUri = new Windows.Foundation.Uri(
                    aadLoginUrl,
                    WwaAADAuthenticationAdapter.AAD_LOGOUT_URL_PART);
            }
        }

        /*
         * Authenticates user through the authentication service.
         * @returns {IAsyncResult<IUserDetails>} the async result containing the authenticated user information.
         */
        public login(): IAsyncResult<IUserDetails> {
            return this._acquireToken(Commerce.Config.aadRetailServerResourceId)
                .map((result: Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult): IUserDetails => {

                var userDetails: IUserDetails = {
                    userName: result.userInfo.displayableId,
                    tenantId: result.tenantId,
                    objectId: result.userInfo.uniqueId,
                    fullName: StringExtensions.format("{0} {1}", result.userInfo.givenName || "", result.userInfo.familyName || "")
                };

                return userDetails;
            });
        }

        /*
         * Acquires the token thought the authentication service.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {IAsyncResult<string>} the async result containing the access token.
         */
        public acquireToken(resouceId: string): IAsyncResult<string> {
            return this._acquireToken(resouceId)
                .map((result: Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult): string => {
                return result.accessToken;
            });
        }

        /*
         * Clears any access tokens present on the application and logs the user out of the authentication service.
         * @returns {IVoidAsyncResult} the async result indicating when the logout process completes.
         */
        public logout(): IVoidAsyncResult {
            this._checkAADContextInitialized();

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            // ADAL library does not provide a way to logout - so we need to do it manually
            // First step: clear local AAD tokens
            this.clearTokenCache();

            // Second step: clear cookies against AAD origin
            // Because cookies are kept by a web view outside the application's
            // We need to use the same web broker as the ADAL uses
            Windows.Security.Authentication.Web.WebAuthenticationBroker.authenticateAsync(
                Windows.Security.Authentication.Web.WebAuthenticationOptions.silentMode,
                this._addLogoutUri).done((result: Windows.Security.Authentication.Web.WebAuthenticationResult) => {
                    asyncResult.resolve();
                },
                (error: any) => {
                    // ignore time out as AAD service usually takes considerable amount of time to complete
                    if (error.number === WwaAADAuthenticationAdapter.WEBAUTHENTICATIONBROKER_TIMEOUT_ERROR_ID) {
                        asyncResult.resolve();
                    } else {
                        RetailLogger.genericError("WWA ADAL: an error was returned when logging out from AAD: " + String(error));
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_AUTHENTICATION_FAILED)]);
                    }
                });

            return asyncResult;
        }

        /*
         * Retrieves a token if available. If the token is not available, null is returned.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {string} the token for the resource or null if token is not available.
         */
        public retrieveAvailableToken(resourceId: string): string {
            var tokenItemSelected: Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheItem;

            // only try to find token if AAD context was initialized
            if (this._aadContext) {
                var tokenItems: Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheItem[] =
                    <Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheItem[]><any>this._aadContext.tokenCache.readItems();

                // get the first tokenItem that matches the resourceId we are looking for
                tokenItemSelected = ArrayExtensions.firstOrUndefined(
                    tokenItems,
                    (tokenItem: Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheItem) => {
                        return tokenItem.resource === resourceId;
                    });
            }

            return tokenItemSelected ? tokenItemSelected.accessToken : null;
        }

        /**
         * Clears local cache used to keep user and token data.
         */
        public clearCache(): void {
            this._checkAADContextInitialized();
            this.clearTokenCache();
        }

        /*
         * Acquires the token thought the authentication service.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {IAsyncResult<string>} the async result containing the access token.
         */
        private _acquireToken(resouceId: string): IAsyncResult<Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult> {
            this._checkAADContextInitialized();

            var result: AsyncResult<Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult> =
                new AsyncResult<Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult>();

            this._aadContext.acquireTokenAsync(
                resouceId,
                Commerce.Config.aadClientId,
                WwaAADAuthenticationAdapter.AAD_REDIRECT_URL,
                Microsoft.IdentityModel.Clients.ActiveDirectory.PromptBehavior.Auto,
                Microsoft.IdentityModel.Clients.ActiveDirectory.UserIdentifier.anyUser,
                WwaAADAuthenticationAdapter.AAD_EXTRA_QUERY_PARAMETERS)
                .done((authenticationResult: Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult): void => {

                if (authenticationResult.status === 0) {
                    result.resolve(authenticationResult);
                } else {
                    var errorMessage: string = StringExtensions.format(
                        "AAD service failed with error: {0} - {1}.",
                        authenticationResult.error,
                        authenticationResult.errorDescription);
                    RetailLogger.genericError(errorMessage);
                    result.reject([new Model.Entities.Error(ErrorTypeEnum.AAD_AUTHENTICATION_FAILED)]);
                }
            });

            return result;
        }

        /**
         * Checks whether the AAD Context was initialized.
         */
        private _checkAADContextInitialized(): void {
            if (ObjectExtensions.isNullOrUndefined(this._aadContext)) {
                throw new Error("The AAD context was not initialized. Please check your AAD configuration settings.");
            }
        }

        /**
         * Clears local token cache.
         */
        private clearTokenCache(): void {
            this._aadContext.tokenCache.clear();
        }
    }
}
