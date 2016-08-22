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

    type TenderCountingViewControllerOptions = {
        tenderDropAndDeclareType: Proxy.Entities.TransactionType;
        shift: Proxy.Entities.Shift;
    }

    /**
     * Options passed to the TenderDeclaration operation.
     */
    export interface ITenderDeclarationOperationOptions extends IOperationOptions {
        shift: Proxy.Entities.Shift;
    }

    /**
     * Handler for the TenderDeclaration operation.
     */
    export class TenderDeclarationOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the TenderDeclaration operation.
         * @param {IPriceOverrideOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: ITenderDeclarationOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreTenderDeclarationTriggerOptions = { shift: options.shift || Session.instance.Shift };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreTenderDeclaration, preTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {ITenderDeclarationOperationOptions} options The provided options.
         * @return {ITenderDeclarationOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: ITenderDeclarationOperationOptions): ITenderDeclarationOperationOptions {
            options = options || { shift: undefined };
            return options;
        }

        /**
         * Executes the TenderDeclaration operation.
         * @param {ITenderDeclarationOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: ITenderDeclarationOperationOptions): IAsyncResult<IOperationResult> {
            var tenderDeclarationQueue: AsyncQueue = new AsyncQueue();
            var dropAndDeclareTransaction: Proxy.Entities.DropAndDeclareTransaction = { Id: StringExtensions.EMPTY };

            tenderDeclarationQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var reasonCodeQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { dropAndDeclareTransaction: dropAndDeclareTransaction },
                    (c: ReasonCodesContext) => { return VoidAsyncResult.createResolved(); },
                    Proxy.Entities.ReasonCodeSourceType.TenderDeclaration);

                return tenderDeclarationQueue.cancelOn(reasonCodeQueue.run());
            }).enqueue((): IVoidAsyncResult => {
                return Peripherals.instance.cashDrawer.openAsync()
                    .done(() => {
                        var tenderOptions: TenderCountingViewControllerOptions = <any>{
                            tenderDropAndDeclareType: Proxy.Entities.TransactionType.TenderDeclaration,
                            shift: options.shift,
                            reasonCodeLines: dropAndDeclareTransaction.ReasonCodeLines
                        };

                        ViewModelAdapter.navigate("TenderCountingView", tenderOptions);
                    });
            });

            return tenderDeclarationQueue.run();
        }
    }
}