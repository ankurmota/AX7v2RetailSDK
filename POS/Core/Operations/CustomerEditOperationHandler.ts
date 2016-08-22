/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * The options passed to the CustomerEdit operation.
     */
    export interface ICustomerEditOperationOptions extends IOperationOptions {
        customer: Model.Entities.Customer;
        destination: string;
        destinationOptions: any;
    }

    /**
     * Handler for the CustomerEdit operation.
     */
    export class CustomerEditOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CustomerEdit operation.
         * @param {ICustomerEditOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICustomerEditOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { customer: undefined, destination: undefined, destinationOptions: undefined };

            var iCustomerAddEditViewOptions: any = {
                customer: options.customer,
                address: null,
                destination: options.destination,
                destinationOptions: options.destinationOptions
            };

            ViewModelAdapter.navigate("CustomerAddEditView", iCustomerAddEditViewOptions);
            return VoidAsyncResult.createResolved();
        }
    }
}