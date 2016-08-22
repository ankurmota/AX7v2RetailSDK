/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Model.Entities;

    /**
     * Represents the resume cart view model.
     */
    export class ResumeCartViewModel extends ViewModelBase {

        /**
         * Recall the given transaction
         * @param {Entities.Cart} cart The suspended cart to recall.
         * @return {IVoidAsyncResult} The async result.
         */
        public recallTransaction(cart: Model.Entities.Cart): IAsyncResult<ICancelableResult> {
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                var error: Entities.Error = new Entities.Error(ErrorTypeEnum.TRANSACTION_NOT_SELECTED);
                return VoidAsyncResult.createRejected([error]);
            }

            return Triggers.TriggerHelper.executeTriggerWorkflowAsync<INullResult>(
                (): IAsyncResult<ICancelableResult> => {
                    var preTriggerOptions: Triggers.IPreRecallTransactionTriggerOptions = { cart: cart };
                    return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreRecallTransaction, preTriggerOptions);
                },
                (): IVoidAsyncResult => {
                    var asyncResult: VoidAsyncResult = new VoidAsyncResult(null);

                    this.cartManager.resumeCartAsync(cart.Id, cart.CustomerId).fail((errors: Model.Entities.Error[]) => {
                        if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMDISCONTINUEDFROMCHANNEL)) {
                            this.cartManager.getCartByCartIdAsync(cart.Id).done(() => {
                                asyncResult.reject(errors);
                            }).fail((getCartByIdErrors: Model.Entities.Error[]) => {
                                asyncResult.reject(getCartByIdErrors);
                            });
                        } else {
                            asyncResult.reject(errors);
                        }
                    }).done(() => {
                        asyncResult.resolve();
                    });

                    return asyncResult;
                },
                (): IVoidAsyncResult => {
                    var postTriggerOptions: Triggers.IPostRecallTransactionTriggerOptions = { cart: cart };
                    return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostRecallTransaction, postTriggerOptions);
                });
        }

        /**
         * Gets all suspended transactions
         * @return {IAsyncResult<Model.Entities.Cart[]>} The async result containing the suspended carts.
         */
        public getSuspendedTransactions(): IAsyncResult<Model.Entities.Cart[]> {
            return this.cartManager.getSuspendedCarts();
        }
    }
}