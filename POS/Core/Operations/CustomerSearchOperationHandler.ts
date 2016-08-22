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
     * The options passed to the CustomerSearch operation.
     */
    export interface ICustomerSearchOperationOptions extends IOperationOptions {
        searchText: string;
        destination: string;
        destinationOptions: any;
    }

    /**
     * Handler for the CustomerSearch operation.
     */
    export class CustomerSearchOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CustomerSearch operation.
         * @param {ICustomerSearchOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICustomerSearchOperationOptions): IAsyncResult<IOperationResult> {
            options = options || { searchText: undefined, destination: undefined, destinationOptions: undefined };

            Commerce.ViewModelAdapter.navigate("SearchView", {
                searchText: options.searchText,
                searchEntity: "Customers",
                destination: options.destination,
                destinationOptions: options.destinationOptions
            });

            return VoidAsyncResult.createResolved();
        }
    }
}