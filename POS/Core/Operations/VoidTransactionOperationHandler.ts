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
     * Options passed to the VoidTransaction operation.
     */
    export interface IVoidTransactionOperationOptions extends IOperationOptions {
        /**
         * The cart to void.
         */
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the VoidTransaction operation.
     */
    export class VoidTransactionOperationHandler extends OperationHandlerBase {
        /**
         * Execute the operation.
         * @param {IVoidTransactionOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IVoidTransactionOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined };

            var asyncQueue: AsyncQueue = new AsyncQueue();

            // we need to get a transaction to void
            if (ObjectExtensions.isNullOrUndefined(options.cart)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var activity: Activities.GetTransactionActivity = new Activities.GetTransactionActivity();
                    activity.responseHandler = (response: Activities.GetTransactionActivityResponse): IVoidAsyncResult => {
                        if (ObjectExtensions.isNullOrUndefined(response.cart)) {
                            return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.OPERATION_VALIDATION_INVALID_ARGUMENTS)]);
                        }

                        return VoidAsyncResult.createResolved();
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        options.cart = activity.response.cart;
                    });
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var isSessionCart: boolean = Session.instance.cart.Id === options.cart.Id;
                if (isSessionCart && !Session.instance.isCartInProgress) {
                    asyncQueue.cancel();
                    return VoidAsyncResult.createResolved();
                }

                return ViewModelAdapter.displayMessage(ViewModelAdapter.getResourceString("string_187"),
                    MessageType.Info, MessageBoxButtons.YesNo, null, 0).done((result: DialogResult) => {
                        if (result !== DialogResult.Yes) {
                            asyncQueue.cancel();
                        }
                    });
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreVoidTransactionTriggerOptions = { cart: options.cart };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreVoidTransaction, preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            }).enqueue((): IVoidAsyncResult => {
                var reasonCodesResult: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: options.cart },
                    (c: ReasonCodesContext) => { return this.cartManager.voidCartAsync(c.cart.Id, c.cart.ReasonCodeLines); },
                    Proxy.Entities.ReasonCodeSourceType.VoidTransaction).run();

                return asyncQueue.cancelOn(reasonCodesResult);
            }).enqueue((): IVoidAsyncResult => {
                var isSessionCart: boolean = Session.instance.cart.Id === options.cart.Id;
                if (isSessionCart) {
                    Session.instance.clearCart();
                }

                if (Peripherals.instance.lineDisplay) {
                    Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayIdleText();
                }

                return VoidAsyncResult.createResolved();
            }).enqueue((): IVoidAsyncResult => {
                var postTriggerOptions: Triggers.IPostVoidTransactionTriggerOptions = { cart: options.cart };

                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostVoidTransaction, postTriggerOptions);
            });

            return asyncQueue.run();
        }
    }
}