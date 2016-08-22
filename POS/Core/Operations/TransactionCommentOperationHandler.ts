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
     * Options passed to the TransactionComment operation.
     */
    export interface ITransactionCommentOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
        comment: string;
    }

    /**
     * Handler for the TransactionComment operation.
     */
    export class TransactionCommentOperationHandler extends OperationHandlerBase {
        /**
         * Execute the operation.
         * 
         * @param {ITransactionCommentOperationOptions} options The transaction comment operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ITransactionCommentOperationOptions): IAsyncResult<IOperationResult> {
            options = options || { cart: undefined, comment: undefined };
            options.cart = options.cart || Session.instance.cart;
            options.comment = options.comment || undefined;

            // This operation is not supported for customer order pickup, cancellation, or return.
            var cart: Model.Entities.Cart = options.cart;
            if (cart.CartTypeValue === Model.Entities.CartType.CustomerOrder
                && (cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Pickup
                || cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Return)
                || cart.CustomerOrderModeValue === Model.Entities.CustomerOrderMode.Cancellation) {

                return VoidAsyncResult.createRejected([
                    new Model.Entities.Error(ErrorTypeEnum.CUSTOMER_ORDER_OPERATION_PICKUP_CANCEL_RETURN_NOT_SUPPORTED)
                ]);
            }

            var asyncQueue = new AsyncQueue();
            if (StringExtensions.isNullOrWhitespace(options.comment)) {
                asyncQueue.enqueue(() => {
                    var activity = new Activities.GetTransactionCommentActivity({ cart: options.cart });
                    activity.responseHandler = (comment) => {
                        return this.cartManager.addCartCommentAsync(comment);
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            } else {
                asyncQueue.enqueue(() => {
                    return this.cartManager.addCartCommentAsync(options.comment);
                });
            }

            return asyncQueue.run();
        }
    }
}