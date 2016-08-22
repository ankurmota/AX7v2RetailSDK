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
     * The options passed to the CustomerAdd operation.
     */
    export interface ICustomerAddOperationOptions extends IOperationOptions {
        destination: string;
        destinationOptions: any;
    }

    /**
     * Handler for the CustomerAdd operation.
     */
    export class CustomerAddOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the CustomerAdd operation.
         * @param {ICustomerAddOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: ICustomerAddOperationOptions): IAsyncResult<ICancelableResult> {
            return Triggers.TriggerManager.instance.execute(
                Triggers.CancelableTriggerType.PreCustomerAdd,
                <Triggers.IPreCustomerAddTriggerOptions> { cart: Session.instance.cart });
        }

        /**
         * Executes the CustomerAdd operation.
         * @param {ICustomerAddOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: ICustomerAddOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { destination: undefined, destinationOptions: undefined };

            var iCustomerAddEditViewOptions: any = {
                customer: null,
                address: null,
                destination: options.destination,
                destinationOptions: options.destinationOptions
            };

            ViewModelAdapter.navigate("CustomerAddEditView", iCustomerAddEditViewOptions);
            return VoidAsyncResult.createResolved();
        }
    }
}