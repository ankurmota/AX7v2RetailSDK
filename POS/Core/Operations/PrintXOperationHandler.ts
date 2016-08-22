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
     * Options passed to the PrintXOperation operation.
     */
    export interface IPrintXOperationOptions extends IOperationOptions {
        shift: Model.Entities.Shift;
    }

    /**
     * Handler for the PrintXOperation operation.
     */
    export class PrintXOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PrintXOperation operation.
         *
         * @param {IPrintXOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPrintXOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { shift: undefined };
            options.shift = options.shift || Session.instance.Shift;

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var receipts: Model.Entities.Receipt[] = null;

            asyncQueue.enqueue(() => {
                return this.storeOperationsManager.getXReport(
                    options.shift.ShiftId,
                    options.shift.TerminalId,
                    ApplicationContext.Instance.hardwareProfile.ProfileId).done((xreport: Model.Entities.Receipt) => {
                        receipts = [xreport];
                    });
            });

            asyncQueue.enqueue(() => {
                var activity: Activities.PrintReceiptActivity = new Activities.PrintReceiptActivity({ receipts: receipts, notifyOnNoPrintableReceipts: true });

                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }
                });
            });

            return asyncQueue.run();
        }
    }
}
