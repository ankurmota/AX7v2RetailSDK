/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Authentication {
    "use strict";

    /**
     * Contract for all authentication providers.
     */
    export interface IAuthenticationProvider {
        /**
         * Logs off from the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        logoff(): IVoidAsyncResult;

        /**
         * Acquires the authentication token with the provider, if authentication requirements.
         * @returns {IAsyncResult<IAuthenticationToken>} a promise for the operation completion containing the authentication token.
         * Token might be null or undefined if authentication with provider is absent or expired.
         */
        acquireToken(): IAsyncResult<IAuthenticationToken>;

        /**
         * Restores the authentication provider state from the restored token value.
         * @param {IAuthenticationToken} token the token to be used to restore the authentication provider state.
         * @returns {IVoidAsyncResult} the completion promise.
         */
        restoreToken(token: IAuthenticationToken): IVoidAsyncResult;
    }
} 