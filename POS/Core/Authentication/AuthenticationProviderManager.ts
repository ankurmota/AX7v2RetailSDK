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
     * Class to manage authentication providers.
     */
    export class AuthenticationProviderManager {
        private static _instance: AuthenticationProviderManager;
        private implicitGrantProviders: { [resourceType: string]: IImplicitGrantAuthenticationProvider };
        private resourceOwnerPasswordGrantProviders: { [resourceType: string]: IResourceOwnerPasswordGrantAuthenticationProvider };

        /**
         * Initializes a new instance of the AuthenticationProviderManager class.
         */
        constructor() {
            this.implicitGrantProviders = {};
            this.resourceOwnerPasswordGrantProviders = {};
        }

        /**
         * Gets the instance of this class.
         */
        public static get instance(): AuthenticationProviderManager {
            if (!AuthenticationProviderManager._instance) {
                AuthenticationProviderManager._instance = new AuthenticationProviderManager();
            }

            return AuthenticationProviderManager._instance;
        }

        /**
         * Registers a provider to a specific resource type.
         * @param {IImplicitGrantAuthenticationProvider} provider the authentication provider.
         * @param {string} resourceType the resource type that this provider will authenticate for.
         */
        public registerImplicitGrantProvider(provider: IImplicitGrantAuthenticationProvider, resourceType: string): void {
            this.implicitGrantProviders[resourceType] = provider;
        }

        /**
         * Registers a provider to a specific resource type.
         * @param {IResourceOwnerPasswordGrantAuthenticationProvider} provider the authentication provider.
         * @param {string} resourceType the resource type that this provider will authenticate for.
         */
        public registerResourceOwnerPasswordGrantProvider(provider: IResourceOwnerPasswordGrantAuthenticationProvider, resourceType: string): void {
            this.resourceOwnerPasswordGrantProviders[resourceType] = provider;
        }

        /**
         * Gets the provider to a specific resource type.
         * @param {string} resourceType the resource type that this provider will authenticate for.
         * @ returns {IAuthenticationProvider} the authentication provider.
         */
        public getImplicitGrantProvider(resourceType: string): IImplicitGrantAuthenticationProvider {
            return this.implicitGrantProviders[resourceType];
        }

        /**
         * Gets the provider to a specific resource type.
         * @param {string} resourceType the resource type that this provider will authenticate for.
         * @ returns {IResourceOwnerPasswordGrantAuthenticationProvider} the authentication provider.
         */
        public getResourceOwnerPasswordGrantProvider(resourceType: string): IResourceOwnerPasswordGrantAuthenticationProvider {
            return this.resourceOwnerPasswordGrantProviders[resourceType];
        }

        /**
         * Returns the first token associated with the resource type.         
         * @param {string} resourceType for which token should be acquired.
         * @return {IAsyncResult<string>} promised with a token as result.
         */
        public acquireToken(resourceType: string): IAsyncResult<IAuthenticationToken> {
            var queue: AsyncQueue = new AsyncQueue();
            var token: IAuthenticationToken = null;

            // gets token for first available provider type for that resource type
            queue.enqueue((): IVoidAsyncResult => {
                // try implicit grant first
                var provider: IAuthenticationProvider = this.getImplicitGrantProvider(resourceType);
                if (!ObjectExtensions.isNullOrUndefined(provider)) {
                    return provider.acquireToken().done((authenticationToken: IAuthenticationToken): void => {
                        token = authenticationToken;
                    });
                }

                return VoidAsyncResult.createResolved();
            }).enqueue((): IVoidAsyncResult => {
                // if token is not found, try resource owner password grant then
                var provider: IAuthenticationProvider = this.getResourceOwnerPasswordGrantProvider(resourceType);
                if (ObjectExtensions.isNullOrUndefined(token) && !ObjectExtensions.isNullOrUndefined(provider)) {
                    return provider.acquireToken().done((authenticationToken: IAuthenticationToken): void => {
                        token = authenticationToken;
                    });
                }

                return VoidAsyncResult.createResolved();
            });

            return queue.run().map((): IAuthenticationToken => {
                return token;
            });
        }

        /**
         * Calls logoff for all providers related to the resource type.
         * @param {string} resourceType for which logoff should be called.
         * @return {IVoidAsyncResult} promised when completing the operation.
         */
        public logoff(resourceType: string): IVoidAsyncResult {
            var implicit: IImplicitGrantAuthenticationProvider = this.getImplicitGrantProvider(resourceType);
            var owner: IResourceOwnerPasswordGrantAuthenticationProvider = this.getResourceOwnerPasswordGrantProvider(resourceType);

            return VoidAsyncResult.join([
                implicit != null ? implicit.logoff() : VoidAsyncResult.createResolved(),
                owner != null ? owner.logoff() : VoidAsyncResult.createResolved(),
            ]);
        }
    }
}