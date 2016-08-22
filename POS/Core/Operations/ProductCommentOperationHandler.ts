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
     * Options passed to the ProductComment operation.
     */
    export interface IProductCommentOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;

        // Cart lines to add/update comments. If the cart is of type CustomerAccount, this parameter is ignored.
        cartLineComments: {
            cartLine: Model.Entities.CartLine;
            comment: string;
        }[];

        // Customer account deposit line to add/update comments. If the cart is not of type CustomerAccount, this parameter is ignored.
        customerAccountDepositLineComment: {
            line: Model.Entities.CustomerAccountDepositLine;
            comment: string;
        };
    }

    /**
     * Handler for the ProductComment operation.
     */
    export class ProductCommentOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ProductComment operation.
         *
         * @param {IProductCommentOperationOptions} options The options containing the cart lines to get comments for and/or comments to add to the cart lines.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IProductCommentOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: Session.instance.cart, cartLineComments: [], customerAccountDepositLineComment: null};
            options.cart = options.cart || Session.instance.cart;
            options.cartLineComments = options.cartLineComments || [];
            options.customerAccountDepositLineComment = options.customerAccountDepositLineComment || null;

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

            // Check that the cart is not for an income/expense transaction
            if (CartHelper.isCartType(cart, Model.Entities.CartType.IncomeExpense)) {
                return VoidAsyncResult.createRejected([
                    new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION)
                ]);
            }

            // If this is a customer account deposit, run the code to process a customer account deposit instead.
            if (CartHelper.isCartType(options.cart, Proxy.Entities.CartType.AccountDeposit)) {
                return this.executeForCustomerAccountDeposit(options);
            }

            var cartLinesWithoutComment: Model.Entities.CartLine[] = [];
            var cartLinesToUpdate: Model.Entities.CartLine[] = [];

            options.cartLineComments.forEach((cartLineComment) => {
                if (StringExtensions.isNullOrWhitespace(cartLineComment.comment)) {
                    cartLinesWithoutComment.push(cartLineComment.cartLine);
                } else {
                    cartLinesToUpdate.push(<Model.Entities.CartLine>{
                        LineId: cartLineComment.cartLine.LineId,
                        Quantity: cartLineComment.cartLine.Quantity,
                        Comment: cartLineComment.comment
                    });
                }
            });

            var asyncQueue = new AsyncQueue();
            if (ArrayExtensions.hasElements(cartLinesWithoutComment)) {
                asyncQueue.enqueue(() => {
                    var activity = new Activities.GetCartLineCommentsActivity({ cartLines: cartLinesWithoutComment });

                    activity.responseHandler = (response: Activities.GetCartLineCommentsActivityResponse): IVoidAsyncResult => {
                        var updatedCartLines = cartLinesToUpdate.concat(cartLinesWithoutComment.map((cartLine, index) => {
                            return <Model.Entities.CartLine>{
                                LineId: cartLine.LineId,
                                Quantity: cartLine.Quantity,
                                Comment: response.comments[index],
                            };
                        }));

                        return this.cartManager.updateCartLinesOnCartAsync(updatedCartLines);
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            } else if (ArrayExtensions.hasElements(cartLinesToUpdate)) {
                asyncQueue.enqueue(() => {
                    return this.cartManager.updateCartLinesOnCartAsync(cartLinesToUpdate);
                });
            }

            return asyncQueue.run();
        }

        /**
         * Executes the ProductComment operation for a CustomerAccountDeposit.
         *
         * @param {IProductCommentOperationOptions} options The options containing the cart lines to get comments for and/or comments to add to the cart lines.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        private executeForCustomerAccountDeposit(options: IProductCommentOperationOptions): IAsyncResult<IOperationResult> {

            // Function to set the comment for the customer account deposit line
            var setCustomerAccountDepositLineComment = (comment: string): IVoidAsyncResult => {
                var customerAccountDepositLine: Model.Entities.CustomerAccountDepositLine = {
                    Amount: options.customerAccountDepositLineComment.line.Amount,
                    CustomerAccount: options.customerAccountDepositLineComment.line.CustomerAccount,
                    Comment: comment
                }

                return this.cartManager.addCustomerAccountDepositLinesToCartAsync([customerAccountDepositLine]);
            };

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!ObjectExtensions.isNullOrUndefined(options.customerAccountDepositLineComment)) {
                // Get the comment and then set the comment on the customer account deposit line if the comment was not set in the option 
                if (ObjectExtensions.isNullOrUndefined(options.customerAccountDepositLineComment.comment)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var activity: Activities.GetCustomerAccountDepositLineCommentsActivity = new Activities.GetCustomerAccountDepositLineCommentsActivity(
                            { customerAccountDepositLines: [options.customerAccountDepositLineComment] });

                        activity.responseHandler = (response: Activities.IGetCustomerAccountDepositLineCommentsActivityResponse): IVoidAsyncResult => {
                            return setCustomerAccountDepositLineComment(response.comments[0]);
                        };

                        return activity.execute().done(() => {
                            if (!activity.response) {
                                asyncQueue.cancel();
                                return;
                            }
                        });
                    });
                } else {
                    // Set the comment on the customer account deposit line that was passed in the option
                    return setCustomerAccountDepositLineComment(options.customerAccountDepositLineComment.comment);
                }
            }

            return asyncQueue.run();
        }
    }
}