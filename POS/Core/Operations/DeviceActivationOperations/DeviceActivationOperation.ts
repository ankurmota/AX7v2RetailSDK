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

    export class DeviceActivationOperation extends DeviceActivationOperationBase {
        public deviceActivationResult: Model.Entities.DeviceActivationResult;

        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8045");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8046");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                var authManager: Model.Managers.IAuthenticationManager = Model.Managers.Factory.getManager
                    <Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName);

                return authManager.activateDeviceAsync(
                    this.stateOperation.deviceId, this.stateOperation.registerId, Config.appHardwareId, this.stateOperation.forceActivate)
                    .done((deviceActivationResult: Model.Entities.DeviceActivationResult) => {
                        try {
                            // Store register id, channel, device token and device Id in persistent storage
                            ApplicationStorage.setItem(ApplicationStorageIDs.REGISTER_ID_KEY, deviceActivationResult.Device.TerminalId);
                            ApplicationStorage.setItem(ApplicationStorageIDs.STORE_ID_KEY, deviceActivationResult.Device.ChannelName);
                            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY, deviceActivationResult.Device.Token);
                            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_ID_KEY, deviceActivationResult.Device.DeviceNumber);
                            TsLogging.LoggerBase.setDeviceInfo(Commerce.Config.appHardwareId,
                                deviceActivationResult.Device.DeviceNumber,
                                deviceActivationResult.Device.TerminalId);
                        } catch (err) {
                            RetailLogger.operationDeviceActivationUnhandledError(err.toString());
                            throw err;
                        }
                });
            };
        }

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public validateState(): () => IVoidAsyncResult {
        return () => {
            var asyncResult: VoidAsyncResult = new VoidAsyncResult();

            super.validateState()()
                .done(() => {
                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

                return asyncResult;
            };
        }
    }
}