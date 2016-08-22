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

    export class GetDeviceConfigurationOperation extends DeviceActivationOperationBase {

        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.GET_DEVICE_CONFIGURATION_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8053");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8054");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                var channelManager: Model.Managers.IChannelManager =
                    Model.Managers.Factory.getManager<Model.Managers.IChannelManager>(Model.Managers.IChannelManagerName);

                var queue: AsyncQueue = new AsyncQueue();

                queue.enqueue(() => {
                    var getDeviceConfigurationResult: VoidAsyncResult = new VoidAsyncResult();
                    channelManager.getDeviceConfigurationAsync().done((deviceConfiguration: Model.Entities.DeviceConfiguration) => {
                            try {
                                ApplicationContext.Instance.deviceConfiguration = deviceConfiguration;

                                // Initialize culture if no culture is set in deviceConfiguration.
                                if (StringExtensions.isNullOrWhitespace(ApplicationContext.Instance.deviceConfiguration.CultureName)) {
                                    ApplicationContext.Instance.deviceConfiguration.CultureName = Commerce.ViewModelAdapter.getDefaultUILanguage();
                                }

                                ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY, JSON.stringify(deviceConfiguration));
                                getDeviceConfigurationResult.resolve();
                            } catch (exception) {
                                var storageError: Proxy.Entities.Error = new
                                    Proxy.Entities.Error(ErrorTypeEnum.APPLICATION_STORE_FAILED_TO_SAVE_DEVICE_CONFIGURATION);
                                getDeviceConfigurationResult.reject([storageError]);
                            }
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        getDeviceConfigurationResult.reject(errors);
                    });

                    return getDeviceConfigurationResult;
                });

                queue.enqueue(() => {

                    // Update application to use the store language.
                    if (!StringExtensions.isNullOrWhitespace(ApplicationContext.Instance.deviceConfiguration.CultureName)) {
                        return ViewModelAdapter.setApplicationLanguageAsync(ApplicationContext.Instance.deviceConfiguration.CultureName);
                    } else {
                        return VoidAsyncResult.createResolved();
                    }
                });

                return queue.run();
            };
        }
    }
}