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

    import Error = Model.Entities.Error;

    /**
     * Options passed to the SuspendTransaction operation.
     */
    export interface ISuspendTransactionOperationOptions extends IOperationOptions {
        /**
         * The cart to suspend.
         */
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the SuspendTransaction operation.
     */
    export class SuspendTransactionOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SuspendTransaction operation.
         * @param {ISuspendTransactionOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ISuspendTransactionOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined };

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var isSessionCart: boolean;

            // we need to get a transaction to suspend
            if (ObjectExtensions.isNullOrUndefined(options.cart)) {
                asyncQueue.enqueue(() => {
                    var activity: Activities.GetTransactionActivity = new Activities.GetTransactionActivity();
                    activity.responseHandler = (response: Activities.GetTransactionActivityResponse): IVoidAsyncResult => {
                        if (ObjectExtensions.isNullOrUndefined(response.cart)) {
                            return VoidAsyncResult.createRejected([new Error(ErrorTypeEnum.OPERATION_VALIDATION_INVALID_ARGUMENTS)]);
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

            asyncQueue.enqueue((): IAsyncResult<any> => {
                isSessionCart = Session.instance.cart.Id === options.cart.Id;
                if (isSessionCart && !Session.instance.isCartInProgress) {
                    asyncQueue.cancel();
                    return VoidAsyncResult.createResolved();
                }

                return VoidAsyncResult.createResolved();
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreSuspendTransactionTriggerOptions = { cart: options.cart };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreSuspendTransaction, preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            }).enqueue((): IVoidAsyncResult => {

                return this.cartManager.suspendCartAsync(options.cart.Id).recoverOnFailure((errors: Proxy.Entities.Error[]) => {
                    if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTACTIVE)) {
                        return VoidAsyncResult.createResolved();
                    }

                    return VoidAsyncResult.createRejected(errors);
                }).done(() => {
                    if (isSessionCart) {
                        Session.instance.clearCart();
                    }
                });
            }).enqueue((): IVoidAsyncResult => {
                var postTriggerOptions: Triggers.IPostSuspendTransactionTriggerOptions = { cart: options.cart };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostSuspendTransaction, postTriggerOptions);
            });

            return asyncQueue.run();
        }
    }
}