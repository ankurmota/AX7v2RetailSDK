/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='IDeviceActivationOperation.ts' />

module Commerce.Operations {
    "use strict";

    export class DeviceActivationOperationBase implements IDeviceActivationOperation {
        private static STATE_OPERATION_NAME: string = "stateOperation";
        public stateOperation: Model.Entities.IDeviceActivationState;

        constructor(stateOperation: Model.Entities.IDeviceActivationState) {
            this.stateOperation = stateOperation;
        }

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        public operationName(): string {
            return Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_OPERATIONBASE_NAME;
        }

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        public processingStatusName(): string {
            return StringExtensions.EMPTY;
        }

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        public errorStatusName(): string {
            return StringExtensions.EMPTY;
        }

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public operationProcess(): () => IVoidAsyncResult {
            return () => {
                return VoidAsyncResult.createResolved();
            };
        }

        /**
         * Notifies user that current device activation operation is changed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public preOperation(): () => IVoidAsyncResult {
            return () => {
                if (ObjectExtensions.isNullOrUndefined(this.stateOperation)) {
                    return VoidAsyncResult.createRejected(this.createRejectResponseForMissingProperties(DeviceActivationOperationBase.STATE_OPERATION_NAME));
                }

                this.stateOperation.currentOperationStep(this.stateOperation.currentOperationStep() + 1);
                this.stateOperation.currentOperation(this);
                return VoidAsyncResult.createResolved();
            };
        }

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        public validateState(): () => IVoidAsyncResult {
            return () => {
                if (ObjectExtensions.isNullOrUndefined(this.stateOperation)) {
                    return VoidAsyncResult.createRejected(this.createRejectResponseForMissingProperties(DeviceActivationOperationBase.STATE_OPERATION_NAME));
                }

                return VoidAsyncResult.createResolved();
            };
        }

        /**
         * Create rejected response for missing properties needed for current operation.
         * @returns {IAsyncResult<any>} The async result.
         */
        public createRejectResponseForMissingProperties(missingProperty: string): Model.Entities.Error[] {
            RetailLogger.corePropertyMissingInDeviceActivationSequence(missingProperty, this.operationName());
            return [new Model.Entities.Error(ErrorTypeEnum.DEVICE_ACTIVATION_DETAILS_NOT_SPECIFIED)];
        }
   }
}