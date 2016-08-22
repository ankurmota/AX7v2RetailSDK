/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='PrePostTriggerOperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the CustomerClear operation.
     */
    export interface ICustomerClearOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the CustomerClear operation.
     */
    export class CustomerClearOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the CustomerClear operation.
         * @param {ICustomerClearOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: ICustomerClearOperationOptions): IAsyncResult<ICancelableResult> {
            return Triggers.TriggerManager.instance.execute(
                Triggers.CancelableTriggerType.PreCustomerClear,
                <Triggers.ICustomerClearTriggerOptions> { cart: Session.instance.cart });
        }

        /**
         * Executes the post-trigger for the CustomerClear operation.
         * @param {ICustomerClearOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: ICustomerClearOperationOptions, result: IOperationResult): IVoidAsyncResult {
            return Triggers.TriggerManager.instance.execute(
                Triggers.NonCancelableTriggerType.PostCustomerClear,
                <Triggers.ICustomerClearTriggerOptions> { cart: Session.instance.cart });
        }

        /**
         * Executes the CustomerClear operation.
         * @param {ICustomerClearOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: ICustomerClearOperationOptions): IVoidAsyncResult {
            var updateCustomerOptions: Operations.IAddCustomerToSalesOrderOperationOptions = {
                customerId: "",
                cartAffiliations: undefined
            };

            return this.operationsManager.runOperation(RetailOperation.SetCustomer, updateCustomerOptions);
        }
    }
}