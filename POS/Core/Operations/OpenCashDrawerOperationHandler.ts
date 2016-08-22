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
     * Options passed to the OpenCashDrawer operation.
     */
    export interface IOpenCashDrawerOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the OpenCashDrawer operation.
     */
    export class OpenCashDrawerOperationHandler extends OperationHandlerBase {
        /**
         * Executes the OpenCashDrawer operation.
         * @param {IOpenCashDrawerOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IOpenCashDrawerOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            var asyncQueue: AsyncQueue = new AsyncQueue();

            var nonSalesTransaction: Proxy.Entities.NonSalesTransaction = { Id: StringExtensions.EMPTY };
            var storeOperationsManager: Model.Managers.IStoreOperationsManager
                = Model.Managers.Factory.GetManager(Model.Managers.IStoreOperationsManagerName, null);

            // Prompt reason code for open drawer store operation
            asyncQueue.enqueue((): IVoidAsyncResult => {
                nonSalesTransaction.TransactionTypeValue = Proxy.Entities.TransactionType.OpenDrawer;
                nonSalesTransaction.ShiftId = Session.instance.Shift.ShiftId.toString();
                nonSalesTransaction.ShiftTerminalId = Session.instance.Shift.TerminalId;

                var reasonCodesQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { nonSalesTransaction: nonSalesTransaction },
                    (c: ReasonCodesContext) => { return storeOperationsManager.createNonSalesTransaction(c.nonSalesTransaction); },
                    Proxy.Entities.ReasonCodeSourceType.OpenDrawer);

                return asyncQueue.cancelOn(reasonCodesQueue.run());
            }).enqueue((): IVoidAsyncResult => {
                return Peripherals.instance.cashDrawer.openAsync();
            });

            return asyncQueue.run();
        }
    }
}
