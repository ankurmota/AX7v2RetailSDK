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
     * Options passed to the PrintZ operation.
     */
    export interface IPrintZOperationOptions extends IOperationOptions {
        notifyOnNoPrintableReceipts: boolean;
    }

    /**
     * Handler for the PrintZ operation.
     */
    export class PrintZOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PrintZ operation.
         *
         * @param {IPrintZOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPrintZOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { notifyOnNoPrintableReceipts: false };

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var receipts: Model.Entities.Receipt[] = null;

            asyncQueue.enqueue(() => {
                return this.storeOperationsManager.getZReport(ApplicationContext.Instance.hardwareProfile.ProfileId)
                    .done((zreport: Model.Entities.Receipt) => {
                        receipts = [zreport];
                    });
            });

            asyncQueue.enqueue(() => {
                var activity: Activities.PrintReceiptActivity =
                    new Activities.PrintReceiptActivity({ receipts: receipts, notifyOnNoPrintableReceipts: options.notifyOnNoPrintableReceipts });

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
