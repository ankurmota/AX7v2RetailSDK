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
     * Contract for all authentication providers that implements the resource owner password grant flow.
     * See {@link https://msdn.microsoft.com/en-us/library/azure/dn645543.aspx} for details.
     */
    export interface IResourceOwnerPasswordGrantAuthenticationProvider extends IAuthenticationProvider {
        /**
         * Authenticates with the provider.
         * @param {string} userId the user identifier.
         * @param {string} userPassword the user password.
         * @param {string[]} scope an array of strings to scope the granted access.
         * @param {{ [ parameterName: string ]: string} )} parameters a key value pair object representing extra parameters.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         */
        logon(userId: string, userPassword: string, scope: string[], parameters: { [ parameterName: string ]: string} ): IVoidAsyncResult;
    }
}