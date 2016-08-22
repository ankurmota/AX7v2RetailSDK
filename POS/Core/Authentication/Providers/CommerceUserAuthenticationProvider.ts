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
     * Class to manage user authentication using Commerce Authentication endpoint through the resource owner password grant flow.
     */
    export class CommerceUserAuthenticationProvider implements IResourceOwnerPasswordGrantAuthenticationProvider {
        public static EXTENDEDLOGON_BARCODE_GRANT_TYPE: string = "auth://example.auth.contoso.com/barcode";
        public static EXTENDEDLOGON_MSR_GRANT_TYPE: string = "auth://example.auth.contoso.com/msr";
        public static EXTENDEDLOGON_ALL_GRANT_TYPE: string = "*";
        private static PASSWORD_GRANT_TYPE: string = "password";
        private static OPERATION_ID_PARAMETER_NAME: string = "OperationId";
        private static CREDENTIALS_PARAMETER_NAME: string = "Credentials";
        private static GRANT_TYPE_PARAMETER_NAME: string = "GrantType";

        private commerceToken: Model.Entities.Authentication.ICommerceToken;

        /**
         * Initializes a new instance of the CommerceUserAuthenticationProvider class.
         */
        constructor() {
            this.commerceToken = null;
        }

        /**
         * Gets the authentication manager.
         * @returns {Model.Managers.IAuthenticationManager} the authentication manager.
         */
        private get authenticationManager(): Model.Managers.IAuthenticationManager {
            return Model.Managers.Factory.getManager<Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName);
        }

        /**
         * Authenticates with the provider.
         * @param {string} userId the user identifier.
         * @param {string} userPassword the user password.
         * @param {string[]} scope an array of strings to scope the granted access.
         * @param {{ [ parameterName: string ]: string} )} parameters a key value pair object representing extra parameters.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        public logon(userId: string, userPassword: string, scope: string[], parameters: { [parameterName: string]: string }): IVoidAsyncResult {
            var operationId: string = parameters[CommerceUserAuthenticationProvider.OPERATION_ID_PARAMETER_NAME];

            var logonRequest: Model.Entities.Authentication.ILogonRequest = {
                grant_type: parameters[CommerceUserAuthenticationProvider.GRANT_TYPE_PARAMETER_NAME] || CommerceUserAuthenticationProvider.PASSWORD_GRANT_TYPE,
                client_id: Commerce.Config.commerceAuthenticationAudience,
                username: userId,
                password: userPassword,
                credential: parameters[CommerceUserAuthenticationProvider.CREDENTIALS_PARAMETER_NAME]
            };

            if (!StringExtensions.isNullOrWhitespace(operationId)) {
                logonRequest.operation_id = operationId;
            }

            return this.authenticationManager.requestUserToken(logonRequest)
                .done((commerceToken: Model.Entities.Authentication.ICommerceToken) => {

                    // we always use id token instead of access token for commerce issued token
                    // repalace token type to always use id_token
                    if (commerceToken.token_type === "bearer") {
                        commerceToken.token_type = "id_token";
                    }

                    this.commerceToken = commerceToken;
                });
        }

        /**
         * Logs off from the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        public logoff(): IVoidAsyncResult {
            // clear the token
            this.commerceToken = null;
            return VoidAsyncResult.createResolved();
        }

        /**
         * Acquires the authentication token with the provider, if authentication requirements.
         * @returns {IAsyncResult<IAuthenticationToken>} a promise for the operation completion containing the authentication token.
         * Token might be null or undefined if authentication with provider is absent or expired.
         */
        public acquireToken(): IAsyncResult<IAuthenticationToken> {
            var authenticationToken: IAuthenticationToken = null;

            if (!ObjectExtensions.isNullOrUndefined(this.commerceToken)) {
                authenticationToken = {
                    token: this.commerceToken.id_token,
                    tokenType: this.commerceToken.token_type,
                    name: ""
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

            if (ObjectExtensions.isNullOrUndefined(token)) {
                this.commerceToken = null;
            } else {
                this.commerceToken = {
                    id_token: token.token,
                    access_token: "",
                    expires_in: 0,
                    token_type: token.tokenType
                };
            }

            return VoidAsyncResult.createResolved();
        }
    }
}