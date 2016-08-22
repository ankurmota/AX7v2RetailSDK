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
     * Options passed to the SuspendShift operation.
     */
    export interface ISuspendShiftOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the SuspendShift operation.
     */
    export class SuspendShiftOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SuspendShift operation.
         *
         * @param {ISuspendShiftOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ISuspendShiftOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue((): IAsyncResult<any> => {
                    if (!Session.instance.Shift.IsShared) {
                        return VoidAsyncResult.createResolved();
                    }

                    var message: string = ViewModelAdapter.getResourceString("string_4177");
                    return ViewModelAdapter.displayMessage(message, MessageType.Info, MessageBoxButtons.YesNo)
                        .done((result: DialogResult) => {
                            if (result === DialogResult.No) {
                                asyncQueue.cancel();
                            }
                        });
                }).enqueue(() => {
                    return this.storeOperationsManager.suspendShiftAsync(Session.instance.Shift.TerminalId, Session.instance.Shift.ShiftId);
                }).enqueue(() => {
                    Session.instance.Shift = null;
                    ShiftHelper.saveCashDrawerOnStorage(null);

                    return this.operationsManager.runOperation(RetailOperation.LogOff, <ILogoffOperationOptions>{});
                });

            return asyncQueue.run();
        }
    }
}