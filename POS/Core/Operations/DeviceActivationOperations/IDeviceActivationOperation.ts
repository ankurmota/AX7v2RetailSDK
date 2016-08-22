/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    export interface IDeviceActivationOperation {

        /**
         * Evaluate if the state entity has non nullable properties needed for this operation to be executed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        validateState(): () => IVoidAsyncResult;

        /**
         * Notifies user that current device activation operation is changed.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        preOperation(): () => IVoidAsyncResult;

        /**
         * The asynchronous execution of the operation.
         * @returns {() => IVoidAsyncResult} The async result.
         */
        operationProcess(): () => IVoidAsyncResult;

        /**
         * Gets the name of the activation operation.
         * @returns {string} The name of the operation.
         */
        operationName(): string;

        /**
         * Gets the message status for user when the operation is executing asynchronous function.
         * @returns {string} The message status.
         */
        processingStatusName(): string;

        /**
         * Gets the message for user when the operation failed executing asynchronous function.
         * @returns {string} The message error status.
         */
        errorStatusName(): string;
    }
}