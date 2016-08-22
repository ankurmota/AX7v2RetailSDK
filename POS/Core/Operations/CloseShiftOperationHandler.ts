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
     * Options passed to the CloseShift operation.
     */
    export interface ICloseShiftOperationOptions extends IOperationOptions {
        shift: Model.Entities.Shift;
    }

    /**
     * Handler for the CloseShift operation.
     */
    export class CloseShiftOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CloseShift operation.
         *
         * @param {ICloseShiftOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICloseShiftOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { shift: undefined };
            options.shift = options.shift || Session.instance.Shift;

            var isSessionShift: boolean = options.shift.ShiftId === Session.instance.Shift.ShiftId
                && options.shift.TerminalId === Session.instance.Shift.TerminalId
                && options.shift.StoreRecordId === Session.instance.Shift.StoreRecordId;

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (options.shift.IsShared === true) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var message: string = ViewModelAdapter.getResourceString("string_4178");
                    return ViewModelAdapter.displayMessage(message, MessageType.Info, MessageBoxButtons.YesNo)
                        .done((result: DialogResult) => {
                            if (result === DialogResult.No) {
                                asyncQueue.cancel();
                            }
                        });
                });
            }

            asyncQueue.enqueue(() => {
                    // first we close the shift
                    return this.storeOperationsManager.closeShiftAsync(options.shift.TerminalId, options.shift.ShiftId, false)
                        .done(() => {
                            RetailLogger.operationCloseShift();

                            // if the session shift was the one closed, clear it
                            if (isSessionShift) {
                                Session.instance.Shift = null;
                                ShiftHelper.saveCashDrawerOnStorage(null);
                            }
                        });
                }).enqueue(() => {
                    // then print z report prints the recently closed shift
                    return this.operationsManager.runOperation(RetailOperation.PrintZ, <IPrintZOperationOptions>{});
                }).enqueue(() => {
                    // Logoff if necessary else user can continue closing shifts in nondrawer mode.
                    if (isSessionShift) {
                        return OperationsManager.instance.runOperation(RetailOperation.LogOff, <ILogoffOperationOptions>{});
                    }

                    return VoidAsyncResult.createResolved();
                });

            return asyncQueue.run();
        }
    }
}