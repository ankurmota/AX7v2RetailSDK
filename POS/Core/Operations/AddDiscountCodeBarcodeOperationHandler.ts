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
     * Options passed to the AddDiscountCodeBarcode operation.
     */
    export interface IAddDiscountCodeBarcodeOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
        discountCode: string;
    }

    /**
     * Handler for the AddDiscountCodeBarcode operation.
     */
    export class AddDiscountCodeBarcodeOperationHandler extends OperationHandlerBase {
        /**
         * Executes the AddDiscountCodeBarcode operation.
         *
         * @param {IAddDiscountCodeBarcodeOperationOptions} options The options containing the discount code to add to the transaction.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddDiscountCodeBarcodeOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined, discountCode: undefined };
            options.cart = options.cart || Session.instance.cart;

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (StringExtensions.isNullOrWhitespace(options.discountCode)) {
                // get discount code
                asyncQueue.enqueue(() => {
                    var activity: Activities.GetDiscountCodeActivity = new Activities.GetDiscountCodeActivity({ cart: options.cart });
                    activity.responseHandler = (discountCode: string): IVoidAsyncResult => {
                        return asyncQueue.cancelOn(this.addDiscountCode(discountCode));
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
                    return asyncQueue.cancelOn(this.addDiscountCode(options.discountCode));
                });
            }

            return asyncQueue.run();
        }

        /**
         * Adds the discount code to the cart.
         * @param {string} discountCode The discount code to add.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        private addDiscountCode(discountCode: string): IAsyncResult<ICancelableResult> {
            var discountCodeQueue: AsyncQueue = new AsyncQueue();
            discountCodeQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var reasonCodeQueue: AsyncQueue = ActivityHelper.getStartOfTransactionReasonCodesAsyncQueue(Session.instance.cart);
                return discountCodeQueue.cancelOn(reasonCodeQueue.run());
            }).enqueue((): IVoidAsyncResult => {
                if (Session.instance.isCartInProgress) {
                    return VoidAsyncResult.createResolved();
                }

                return this.cartManager.createOrUpdateCartAsync(Session.instance.cart);
            }).enqueue((): IVoidAsyncResult => {
                return this.cartManager.addDiscountCodeToCartAsync(discountCode);
            });

            return discountCodeQueue.run();
        }
    }
}