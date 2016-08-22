/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='DeviceActivationOperationBase.ts' />

module Commerce.Operations {
    "use strict";

    export class UpdateServerUrlOperation extends DeviceActivationOperationBase {

        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.UPDATE_SERVER_URL_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8079");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8080");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                // locator service only works with AAD, thus using AAD Adaptor directly here
                // if another auth provider is used instead of AAD, locator service should be disabled
                var aadUserDetails: Host.IUserDetails;
                var queue: AsyncQueue = new AsyncQueue().enqueue((): IVoidAsyncResult => {
                    return Host.instance.azureActiveDirectoryAdapter.login().done((user: Host.IUserDetails): void => {
                        aadUserDetails = user;
                    });
                }).enqueue((): IVoidAsyncResult => {
                        return this.resolveUrlFromLocatorServiceResult(aadUserDetails.tenantId);
                    });

                return queue.run();
            };
        }

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public validateState(): () => IVoidAsyncResult {
            return () => {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();

                super.validateState()().done(() => {
                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                        asyncResult.reject(errors);
                    });

                return asyncResult;
            };
        }

        private resolveUrlFromLocatorServiceResult(tenantId: string): IVoidAsyncResult {
            var authManager: Model.Managers.IAuthenticationManager = Model.Managers.Factory.getManager
                <Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName);

            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            authManager.getRetailServerUrl(Commerce.Config.locatorServiceUrl, tenantId)
                .done((resources: Locator.Model.Entities.ServiceEndpoint[]) => {
                    var retailServerUrl: string = StringExtensions.EMPTY;
                    RetailLogger.operationLocateServerUrl(Config.locatorServiceUrl);

                    resources.forEach((serviceEndpoint: Locator.Model.Entities.ServiceEndpoint) => {
                        if (serviceEndpoint.Name === Locator.Model.Entities.ServerServiceEndPointNames.RetailServer) {
                            retailServerUrl = serviceEndpoint.PublicUriString;
                        }
                    });

                    RetailLogger.operationUpdateServerUrl(retailServerUrl);

                    if (StringExtensions.isNullOrWhitespace(retailServerUrl)) {
                        RetailLogger.viewModelLoginRetailServerDiscoveryFailed(
                            Commerce.Config.locatorServiceUrl,
                            tenantId,
                            "Could not find retail server url from locator service response");

                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.RETAILSERVER_URL_DISCOVERY_FAILED)]);
                    } else {
                        Commerce.Model.Managers.Factory.updateServerUriInCommerceContext(retailServerUrl);
                        ApplicationStorage.setItem(ApplicationStorageIDs.RETAIL_SERVER_URL, retailServerUrl);
                        ApplicationContext.updateServerUrl(retailServerUrl);
                        this.stateOperation.serviceUrl = retailServerUrl;
                    }

                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                    var errorDetails: string = ErrorHelper.getErrorMessages(errors);
                    RetailLogger.viewModelLoginRetailServerDiscoveryFailed(
                        Config.locatorServiceUrl,
                        tenantId,
                        errorDetails);

                    asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.RETAILSERVER_URL_DISCOVERY_FAILED)]);
                });

            return asyncResult;
        }
    }
}