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
     * Class to manage user authentication for the device.
     */
    export class DeviceAuthenticationProvider implements IResourceOwnerPasswordGrantAuthenticationProvider {
        /**
         * Authenticates with the provider.
         * @param {string} userId the user identifier.
         * @param {string} userPassword the user password.
         * @param {string[]} scope an array of strings to scope the granted access.
         * @param {{ [ parameterName: string ]: string} )} parameters a key value pair object representing extra parameters.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        public logon(userId: string, userPassword: string, scope: string[], parameters: { [parameterName: string]: string }): IVoidAsyncResult {
            throw new Error("Not supported");
        }

        /**
         * Logs off from the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        public logoff(): IVoidAsyncResult {
            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY, StringExtensions.EMPTY);
            return VoidAsyncResult.createResolved();
        }

        /**
         * Acquires the authentication token with the provider, if authentication requirements.
         * @returns {IAsyncResult<IAuthenticationToken>} a promise for the operation completion containing the authentication token.
         * Token might be null or undefined if authentication with provider is absent or expired.
         */
        public acquireToken(): IAsyncResult<IAuthenticationToken> {
            var tokenValue: string = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY);
            var authenticationToken: IAuthenticationToken = null;

            if (!StringExtensions.isNullOrWhitespace(tokenValue)) {
                authenticationToken = {
                    token: tokenValue,
                    tokenType: "",
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
            throw new Error("Not supported.");
        }
    }
} 