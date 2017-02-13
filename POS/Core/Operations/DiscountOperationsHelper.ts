/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    type CartLineDiscount = {
        cartLine: Proxy.Entities.CartLine;
        discountValue: number;
    };

    /**
     * Options passed to the TransactionDiscountAmount and TransactionDiscountPercent operations.
     */
    export interface ITransactionDiscountOperationOptions extends IOperationOptions {
        cart: Proxy.Entities.Cart;
        discountValue: number;
    }

    /**
     * Options passed to the LineDiscountAmount and LineDiscountPercent operations.
     */
    export interface ILineDiscountOperationOptions extends IOperationOptions {
        cartLineDiscounts: {
            cartLine: Proxy.Entities.CartLine;
            discountValue: number;
        }[];
    }

    /**
     * Helper for discount operations.
     */
    export class DiscountOperationsHelper {
        /**
         * Creates the queue for getting discounts and/or adding discounts to cart lines.
         * @param {ITransactionDiscountOperationOptions} options The options containing the cart to get discounts for and/or discount to add to the cart.
         * @return {AsyncQueue} The async queue.
         */
        public static createTransactionDiscountQueue(options: ITransactionDiscountOperationOptions, isPercent: boolean): AsyncQueue {
            // sanitize options
            options = options || { cart: Session.instance.cart, discountValue: undefined };
            options.cart = options.cart || Session.instance.cart;

            var asyncQueue: AsyncQueue = new AsyncQueue();

            // checks whether discounts are allowed
            var error: Proxy.Entities.Error = DiscountsHelper.validateCanAddDiscounts(options.cart);
            if (error) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    return VoidAsyncResult.createRejected([error]);
                });

                return asyncQueue;
            }

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreTotalDiscountTriggerOptions = { cart: options.cart };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(
                        isPercent ? Triggers.CancelableTriggerType.PreTotalDiscountPercent : Triggers.CancelableTriggerType.PreTotalDiscountAmount,
                        preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            });

            if (ObjectExtensions.isNullOrUndefined(options.discountValue)) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var activity: Activities.GetTransactionDiscountActivity = new Activities.GetTransactionDiscountActivity(
                        { cart: options.cart, isPercent: isPercent });

                    activity.responseHandler = (response: Activities.GetTransactionDiscountActivityResponse): IVoidAsyncResult => {
                        // validates discount
                        var error: Proxy.Entities.Error = isPercent
                            ? DiscountsHelper.validateMaximumDiscountPercentage(response.discount)
                            : DiscountsHelper.validateMaximumDiscountAmount(response.discount);

                        if (error) {
                            return VoidAsyncResult.createRejected([error]);
                        }

                        // updates discount
                        options.discountValue = response.discount;

                        return VoidAsyncResult.createResolved();
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            } else {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    // validates discount
                    var error: Proxy.Entities.Error = isPercent
                        ? DiscountsHelper.validateMaximumDiscountPercentage(options.discountValue)
                        : DiscountsHelper.validateMaximumDiscountAmount(options.discountValue);

                    if (error) {
                        return VoidAsyncResult.createRejected([error]);
                    }

                    return VoidAsyncResult.createResolved();
                });
            }

            // updates cart with discounts and reason codes
            asyncQueue.enqueue((): IAsyncResult<any> => {
                var updatedCart: Proxy.Entities.Cart = new Proxy.Entities.CartClass({
                    Id: options.cart.Id,
                    TotalManualDiscountAmount: isPercent ? 0 : options.discountValue,
                    TotalManualDiscountPercentage: isPercent ? options.discountValue : 0,
                    ReasonCodeLines: options.cart.ReasonCodeLines
                });

                var cartManager: Model.Managers.ICartManager = Model.Managers.Factory.getManager<Model.Managers.ICartManager>(Model.Managers.ICartManagerName);
                var retryQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: updatedCart },
                    (context: ReasonCodesContext) => { return cartManager.createOrUpdateCartAsync(context.cart); },
                    Proxy.Entities.ReasonCodeSourceType.TotalDiscount);

                return asyncQueue.cancelOn(retryQueue.run());
            });

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var postTriggerOptions: Triggers.IPostTotalDiscountTriggerOptions = { cart: Session.instance.cart };
                return Triggers.TriggerManager.instance.execute(
                    isPercent ? Triggers.NonCancelableTriggerType.PostTotalDiscountPercent : Triggers.NonCancelableTriggerType.PostTotalDiscountAmount,
                    postTriggerOptions);
            });

            return asyncQueue;
        }

        /**
         * Creates the queue for getting discounts and/or adding discounts to cart lines.
         * @param {ILineDiscountOperationOptions} options The options containing the cart lines to get discounts for and/or discounts to add to the cart lines.
         * @return {AsyncQueue} The async queue.
         */
        public static createLineDiscountQueue(options: ILineDiscountOperationOptions, isPercent: boolean): AsyncQueue {
            // sanitize options
            options = options || { cartLineDiscounts: [] };
            options.cartLineDiscounts = options.cartLineDiscounts || [];

            var cartLinesWithoutDiscount: Proxy.Entities.CartLine[] = [];
            var cartLinesToUpdate: Proxy.Entities.CartLine[] = [];

            var asyncQueue: AsyncQueue = new AsyncQueue();

            //DEMO4
            var newcart: Proxy.Entities.Cart = Session.instance.cart;
            //if (newcart.DeliveryMode === ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode) {

                //var cartLines: Proxy.Entities.CartLine[] = options.cartLineDiscounts
                //    .map((cartLineDiscount: CartLineDiscount): Proxy.Entities.CartLine => {
                //        return cartLineDiscount.cartLine;
                //    });
             
                //return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                //   return this.cartManager.updateCartLinesOnCartAsync(cartLines);
                //}).run();

                //asyncQueue.enqueue((): IAsyncResult<any> => {
                //    var cartManager: Model.Managers.ICartManager = Model.Managers.Factory
                //        .getManager<Model.Managers.ICartManager>(Model.Managers.ICartManagerName);
                //    var retryQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                //        { cartLines: cartLines },
                //        (context: ReasonCodesContext) => {
                //            return cartManager.updateCartLinesOnCartAsync(context.cartLines);
                //        },
                //        Proxy.Entities.ReasonCodeSourceType.ItemDiscount);

                //    return asyncQueue.cancelOn(retryQueue.run());
                //});

           // } else {
                //DEMO 4 END

                // checks whether discounts are allowed
                var error: Proxy.Entities.Error = DiscountsHelper.validateCanAddDiscounts(Session.instance.cart);
                if (error) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        return VoidAsyncResult.createRejected([error]);
                    });

                    return asyncQueue;
                }

                // check whether lines are present
                if (!ArrayExtensions.hasElements(options.cartLineDiscounts)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        return VoidAsyncResult
                            .createRejected([
                                new Proxy.Entities.Error(ErrorTypeEnum
                                    .MISSING_CARTLINE_ON_APPLY_DISCOUNT)
                            ]);
                    });

                    return asyncQueue;
                }

                asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    if (options.cartLineDiscounts
                        .some((cld: CartLineDiscount): boolean => cld.cartLine.IsPriceOverridden)) {
                        var error: Proxy.Entities.Error = new Proxy.Entities.Error(
                            ErrorTypeEnum.PERMISSION_DENIED_CANNOT_APPLY_DISCOUNT_TO_LINE_WITH_OVERRIDDEN_PRICE);
                        return VoidAsyncResult.createRejected([error]);
                    }

                    return VoidAsyncResult.createResolved();
                });

                asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    var cartLines: Proxy.Entities.CartLine[] = options.cartLineDiscounts
                        .map((cartLineDiscount: CartLineDiscount): Proxy.Entities.CartLine => {
                            return cartLineDiscount.cartLine;
                        });

                    var preTriggerOptions: Triggers.IPreLineDiscountTriggerOptions =
                        { cart: Session.instance.cart, cartLines: cartLines };
                    var preTriggerResult: IAsyncResult<ICancelableResult> =
                        Triggers.TriggerManager.instance.execute(
                            isPercent
                            ? Triggers.CancelableTriggerType.PreLineDiscountPercent
                            : Triggers.CancelableTriggerType.PreLineDiscountAmount,
                            preTriggerOptions);

                    return asyncQueue.cancelOn(preTriggerResult);
                });

                options.cartLineDiscounts.forEach((cartLineDiscount: CartLineDiscount) => {
                    // discount has to be null or undefined (thus ==). If it is zero, it means: clear discount
                    if (ObjectExtensions.isNullOrUndefined(cartLineDiscount.discountValue)) {
                        cartLinesWithoutDiscount.push(cartLineDiscount.cartLine);
                    } else {
                        cartLinesToUpdate.push(<Proxy.Entities.CartLine>{
                            LineId: cartLineDiscount.cartLine.LineId,
                            ProductId: cartLineDiscount.cartLine.ProductId,
                            Quantity: cartLineDiscount.cartLine.Quantity,
                            LineManualDiscountAmount: isPercent ? 0 : cartLineDiscount.discountValue,
                            LineManualDiscountPercentage: isPercent ? cartLineDiscount.discountValue : 0,
                            ReasonCodeLines: cartLineDiscount.cartLine.ReasonCodeLines
                        });
                    }
                });

                // validate present discounts
                if (ArrayExtensions.hasElements(cartLinesToUpdate)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var errors: Proxy.Entities.Error[] = [];
                        cartLinesToUpdate.forEach((cartLine: Proxy.Entities.CartLine) => {
                            var error: Proxy.Entities.Error = isPercent
                                ? DiscountsHelper
                                .validateMaximumLineDiscountPercentage(cartLine.LineManualDiscountPercentage, cartLine)
                                : DiscountsHelper
                                .validateMaximumLineDiscountAmount(cartLine.LineManualDiscountAmount, cartLine);

                            if (error) {
                                errors.push(error);
                            }
                        });

                        if (ArrayExtensions.hasElements(errors)) {
                            return VoidAsyncResult.createRejected(errors);
                        }

                        return VoidAsyncResult.createResolved();
                    });
                }

                // get discounts for lines
                if (ArrayExtensions.hasElements(cartLinesWithoutDiscount)) {
                    asyncQueue.enqueue((): IAsyncResult<any> => {
                        var activity: Activities.GetCartLineDiscountsActivity = new Activities
                            .GetCartLineDiscountsActivity(
                                { cartLines: cartLinesWithoutDiscount, isPercent: isPercent });

                        activity
                            .responseHandler = (response: Activities.GetCartLineDiscountsActivityResponse):
                            IVoidAsyncResult => {
                                var errors: Proxy.Entities.Error[] = [];

                                response.discounts.forEach((discount: number, discountIndex: number) => {
                                    var cartLine: Proxy.Entities.CartLine = cartLinesWithoutDiscount[discountIndex];
                                    var error: Proxy.Entities.Error = isPercent
                                        ? DiscountsHelper.validateMaximumLineDiscountPercentage(discount, cartLine)
                                        : DiscountsHelper.validateMaximumLineDiscountAmount(discount, cartLine);

                                    if (error) {
                                        errors.push(error);
                                    }
                                });

                                if (ArrayExtensions.hasElements(errors)) {
                                    return VoidAsyncResult.createRejected(errors);
                                }

                                // updates discounts and add to lines to update collection
                                cartLinesWithoutDiscount
                                    .forEach((cartLine: Proxy.Entities.CartLine, cartLineIndex: number) => {
                                        cartLine
                                            .LineManualDiscountPercentage =
                                            isPercent ? response.discounts[cartLineIndex] : 0;
                                        cartLine
                                            .LineManualDiscountAmount =
                                            isPercent ? 0 : response.discounts[cartLineIndex];

                                        cartLinesToUpdate.push(cartLine);
                                    });

                                return VoidAsyncResult.createResolved();
                            };

                        return activity.execute()
                            .done(() => {
                                if (!activity.response) {
                                    asyncQueue.cancel();
                                    return;
                                }
                            });
                    });
                }

                // updates cart lines
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var cartManager: Model.Managers.ICartManager = Model.Managers.Factory
                        .getManager<Model.Managers.ICartManager>(Model.Managers.ICartManagerName);
                    var retryQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cartLines: cartLinesToUpdate },
                        (context: ReasonCodesContext) => {
                            return cartManager.updateCartLinesOnCartAsync(context.cartLines);
                        },
                        Proxy.Entities.ReasonCodeSourceType.ItemDiscount);

                    return asyncQueue.cancelOn(retryQueue.run());
                });

                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var lineIds: string[] = options.cartLineDiscounts
                        .map((cartLineDiscount: CartLineDiscount): string => {
                            return cartLineDiscount.cartLine.LineId;
                        });

                    var updatedCartLines: Proxy.Entities.CartLine[] = CartHelper
                        .getCartLinesByLineIds(Session.instance.cart, lineIds);
                    var postTriggerOptions: Triggers.IPostLineDiscountTriggerOptions =
                        { cart: Session.instance.cart, cartLines: updatedCartLines };
                    return Triggers.TriggerManager.instance.execute(
                        isPercent
                        ? Triggers.NonCancelableTriggerType.PostLineDiscountPercent
                        : Triggers.NonCancelableTriggerType.PostLineDiscountAmount,
                        postTriggerOptions);
                });

                return asyncQueue;
            //}//demo4
        }
    }
}