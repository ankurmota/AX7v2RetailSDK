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
     * Interface that exposes the platform independent set of common operations that can be performed 
     * agains the Azure Active Directory (AAD).
     */
    export interface IAzureActiveDirectoryAdapter {

        /**
         * Authenticates user through the authentication service.
         * @returns {IAsyncResult<IUserDetails>} the async result containing the authenticated user information.
         */
        login(): IAsyncResult<IUserDetails>;

        /**
         * Acquires the token thought the authentication service.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {IAsyncResult<string>} the async result containing the access token.
         */
        acquireToken(resourceId: string): IAsyncResult<string>;

        /**
         * Retrieves a token if available. If the token is not available, null is returned.
         * @param {string} resourceId the resource identifier of the resource that needs to be accessed.
         * @returns {string} the token for the resource or null if token is not available.
         */
        retrieveAvailableToken(resourceId: string): string;

        /**
         * Clears any access tokens present on the application and logs the user out of the authentication service.
         * @returns {IVoidAsyncResult} the async result indicating when the logout process completes.
         */
        logout(): IVoidAsyncResult;

        /**
         * Clears local cache used to keep user and token data.
         */
        clearCache(): void;
    }
}