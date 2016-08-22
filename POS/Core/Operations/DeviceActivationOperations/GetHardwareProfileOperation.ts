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

    export class GetHardwareProfileOperation extends DeviceActivationOperationBase {

        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.GET_HARDWARE_PROFILE_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8057");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8058");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                var channelManager: Model.Managers.IChannelManager =
                    Model.Managers.Factory.getManager<Model.Managers.IChannelManager>(Model.Managers.IChannelManagerName);

                return channelManager.getHardwareProfileAsync(ApplicationContext.Instance.deviceConfiguration.HardwareProfile)
                    .done((hardwareProfile: Model.Entities.HardwareProfile) => {
                        this.stateOperation.hardwareProfile = hardwareProfile;
                    }).recoverOnFailure((errors: Model.Entities.Error[]) => {
                        // Try and use the application storage value, before throwing exception
                        var currentHardwareProfile: Model.Entities.HardwareProfile = JSON.parse(
                            ApplicationStorage.getItem(ApplicationStorageIDs.HARDWARE_PROFILE_KEY));
                        if (ObjectExtensions.isNullOrUndefined(currentHardwareProfile)) {
                            return VoidAsyncResult.createRejected(errors);
                        }

                        this.stateOperation.skipEncryptionOperation = true;
                        return VoidAsyncResult.createResolved();
                    });
            };
        }
    }
}