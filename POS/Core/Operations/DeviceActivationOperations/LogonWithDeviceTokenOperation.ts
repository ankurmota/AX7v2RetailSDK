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

    export class LogonWithDeviceTokenOperation extends DeviceActivationOperationBase {
        constructor(stateActivity: Model.Entities.IDeviceActivationState) {
            super(stateActivity);
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.LOGON_WITH_DEVICE_TOKEN_OPERATION_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8049");
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return ViewModelAdapter.getResourceString("string_8050");
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                return Utilities.LogonHelper.resourceOwnedPasswordLogon(
                    this.stateOperation.operatorId,
                    this.stateOperation.password);
            };
        }

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed.
         * @returns {IVoidAsyncResult} The async result.
         */
        public validateState(): () => IVoidAsyncResult {
            return () => {
                var asyncResult: VoidAsyncResult = new VoidAsyncResult();

                super.validateState()().done(() => {
                    if (StringExtensions.isNullOrWhitespace(this.stateOperation.operatorId)) {
                        asyncResult.reject(this.createRejectResponseForMissingProperties("operatorId"));
                    }

                    if (StringExtensions.isNullOrWhitespace(this.stateOperation.password)) {
                        asyncResult.reject(this.createRejectResponseForMissingProperties("password"));
                    }

                    if (StringExtensions.isNullOrWhitespace(this.stateOperation.deviceId)) {
                        asyncResult.reject(this.createRejectResponseForMissingProperties("deviceId"));
                    }

                    asyncResult.resolve();
                }).fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

                return asyncResult;
            };
        }
    }
}