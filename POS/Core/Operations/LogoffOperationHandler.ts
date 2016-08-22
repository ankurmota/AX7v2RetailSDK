/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Core.d.ts' />
///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the Logoff operation.
     */
    export interface ILogoffOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the Logoff operation.
     */
    export class LogoffOperationHandler extends OperationHandlerBase {
        /**
         * Executes the Logoff operation.
         *
         * @param {ILogoffOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ILogoffOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            // In case of an ongoing transaction, do not allow the cashier to logoff.
            var cartId: string = Commerce.Session.instance.cart.Id;
            if (!StringExtensions.isNullOrWhitespace(cartId)) {
                var errors: Model.Entities.Error[] = <Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.LOGOFF_ERROR)];
                return VoidAsyncResult.createRejected(errors);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            asyncQueue.enqueue((): IVoidAsyncResult => {
                return Utilities.LogonHelper.logoff();
            }).enqueue((): IVoidAsyncResult => {
                var options: Triggers.IPostLogOffTriggerOptions = { employee: Session.instance.CurrentEmployee };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostLogOff, options);
            });

            return asyncQueue.run()
                .done(() => {
                    Commerce.Session.instance.CurrentEmployee = null;
                    Commerce.Session.instance.isSessionStateValid = false;

                    var setAppLanguageResult: IVoidAsyncResult = ViewModelAdapter.setApplicationLanguageAsync(ApplicationContext.Instance.deviceConfiguration.CultureName);

                    // Display terminal closed message on line display then close device
                    var displayTerminalClosedTextResult: IVoidAsyncResult = Peripherals.HardwareStation.LineDisplayHelper.displayTerminalClosedText()
                        .done(() => {
                            Peripherals.instance.lineDisplay.closeDevice();
                        });

                    var postLogoffActionsResult: IVoidAsyncResult = VoidAsyncResult.join([setAppLanguageResult, displayTerminalClosedTextResult]);

                    postLogoffActionsResult
                        .always(() => {
                            this.onLogoffComplete();
                        }).fail((errors: Model.Entities.Error[]) => {
                            RetailLogger.operationLogOffFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                        });
                });
        }

        private onLogoffComplete(): void {
            RetailLogger.operationLogOffComplete();

            // don't navigate when device activation is in progress
            // this causes a race condition with AAD logon and prevents
            // log off to completely finish when AAD is used
            if (!Commerce.Config.aadEnabled || !Helpers.DeviceActivationHelper.isInDeviceActivationProcess()) {
                Commerce.ViewModelAdapter.navigateToLoginPage();
            }
        }
    }
}
