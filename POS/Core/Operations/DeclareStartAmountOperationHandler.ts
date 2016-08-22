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
     * Options passed to the DeclareStartAmount operation.
     */
    export interface IDeclareStartAmountOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the DeclareStartAmount operation.
     */
    export class DeclareStartAmountOperationHandler extends OperationHandlerBase {
        /**
         * Executes the DeclareStartAmount operation.
         *
         * @param {IDeclareStartAmountOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IDeclareStartAmountOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            return Peripherals.instance.cashDrawer.openAsync()
                .done(() => {
                    var cashManagementViewOptions: any = { nonSalesTenderType: Commerce.Model.Entities.TransactionType.StartingAmount };
                    Commerce.ViewModelAdapter.navigate("CashManagementView", cashManagementViewOptions);
                });
        }
    }
}