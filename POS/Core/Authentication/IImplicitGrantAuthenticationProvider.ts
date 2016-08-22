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
     * Contract for all authentication providers that implements the implicit grant flow.
     * See {@link http://go.microsoft.com/fwlink/p/?linkid=218843} for details.
     */
    export interface IImplicitGrantAuthenticationProvider extends IAuthenticationProvider {
        /**
         * Authenticates with the provider.
         * @returns {IVoidAsyncResult} a promise for the operation completion.
         * @remarks As the implicit flow requires full frame navigation, it is up to the application to perform state keeping.
         */
        logon(): IVoidAsyncResult;
    }
}