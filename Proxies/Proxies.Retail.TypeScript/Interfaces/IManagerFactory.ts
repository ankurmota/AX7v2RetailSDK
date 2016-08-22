/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy {
    "use strict";

    /**
     * Instance of client proxy.
     */
    export var Factory: IManagerFactory;

    export interface IManagerFactory {
        /**
         * Creates an instance of given entity manager.
         *
         * @param {string} entityManagerInterface The interface name.
         */
        getManager<T>(entityManagerInterface: string): T;

        /**
         * Creates an instance of given entity manager.
         *
         * @param {string} entityManagerInterface The interface name.
         * @param {any} [callerContext] The optional reference to caller object.
         */
        GetManager(entityManagerInterface: string, callerContext?: any): any;

        /**
         * Updates the server Uri of the commerce context.
         *
         * @param {string} serverUri The new URI.
         */
        updateServerUriInCommerceContext(retailServerUri: string);
    }
}