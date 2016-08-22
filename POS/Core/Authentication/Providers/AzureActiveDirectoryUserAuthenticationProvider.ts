/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Authentication.Providers {
    "use strict";

    /**
     * Class to manage user authentication using AAD (Azure Active Directory) through the implicit grant flow.
     */
    export class AzureActiveDirectoryUserAuthenticationProvider implements IImplicitGrantAuthenticationProvider {
        private mayLogoff: boolean = false;
        private user: Commerce.Host.IUserDetails;

        /**
         * Authenticates with the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         * @remarks As the implicit flow requires full frame navigation, it is up to the application to perform state keeping.
         */
        public logon(): IVoidAsyncResult {
            this.mayLogoff = true;
            return new AsyncQueue()
                .enqueue((): IVoidAsyncResult => {
                    return Host.instance.azureActiveDirectoryAdapter.login().done((userDetails: Host.IUserDetails) => {
                        this.user = userDetails;
                    });
                })
                .enqueue((): IVoidAsyncResult => { return Host.instance.azureActiveDirectoryAdapter.acquireToken(Commerce.Config.aadRetailServerResourceId); })
                .run();
        }

        /**
         * Logs off from the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        public logoff(): IVoidAsyncResult {
            if (!this.mayLogoff) {
                return VoidAsyncResult.createResolved();
            }

            this.user = null;
            this.mayLogoff = false;
            return Host.instance.azureActiveDirectoryAdapter.logout();
        }

        /**
         * Acquires the authentication token with the provider.
         * @returns {IAsyncResult<IAuthenticationToken>} a promise for the operation completion containing the authentication token.
         * Token might be null or undefined if authentication with provider is absent or expired.
         */
        public acquireToken(): IAsyncResult<IAuthenticationToken> {
            var token: string = Host.instance.azureActiveDirectoryAdapter.retrieveAvailableToken(Commerce.Config.aadRetailServerResourceId);
            var authenticationToken: IAuthenticationToken = null;
            var currentUser: Host.IUserDetails = this.user;

            if (!StringExtensions.isNullOrWhitespace(token)) {
                authenticationToken = {
                    token: token,
                    tokenType: "bearer",
                    name: currentUser.fullName
                };
            }

            return AsyncResult.createResolved(authenticationToken);
        }

        /**
         * Restores the authentication provider state from the restored token value.
         * @param {IAuthenticationToken} token the token to be used to restore the authentication provider state.
         * @returns {IVoidAsyncResult} the completion promise.
         */
        public restoreToken(token: IAuthenticationToken): IVoidAsyncResult {
            throw new Error("Not supported.");
        }
    }
}