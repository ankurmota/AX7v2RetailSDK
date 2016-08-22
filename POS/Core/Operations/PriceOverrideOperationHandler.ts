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

    import Entities = Proxy.Entities;

    type CartLinePrice = { cartLine: Entities.CartLine; price: number; };

    /**
     * Options passed to the PriceOverride operation.
     */
    export interface IPriceOverrideOperationOptions extends IOperationOptions {
        cartLinePrices: {
            cartLine: Entities.CartLine;
            price: number;
        }[];
    }

    /**
     * Handler for the PriceOverride operation.
     */
    export class PriceOverrideOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the PriceOverride operation.
         * @param {IPriceOverrideOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: IPriceOverrideOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPrePriceOverrideTriggerOptions = { cart: Session.instance.cart, operationOptions: options };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PrePriceOverride, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the PriceOverride operation.
         * @param {IPriceOverrideOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: IPriceOverrideOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var lineIds: string[] = options.cartLinePrices.map((cartLinePrice: CartLinePrice): string => {
                return cartLinePrice.cartLine.LineId;
            });

            var updatedCartLines: Entities.CartLine[] = CartHelper.getCartLinesByLineIds(Session.instance.cart, lineIds);
            var postTriggerOptions: Triggers.IPostPriceOverrideTriggerOptions = { cart: Session.instance.cart, cartLines: updatedCartLines };

            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostPriceOverride, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {IPriceOverrideOperationOptions} options The provided options.
         * @return {IPriceOverrideOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: IPriceOverrideOperationOptions): IPriceOverrideOperationOptions {
            options = options || { cartLinePrices: undefined };
            return options;
        }

        /**
         * Executes the PriceOverride operation.
         * @param {IPriceOverrideOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: IPriceOverrideOperationOptions): IAsyncResult<IOperationResult> {
            if (!ArrayExtensions.hasElements(options.cartLinePrices)) {
                return AsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.OPERATION_VALIDATION_INVALID_ARGUMENTS)]);
            }

            var updatedCartLines: Entities.CartLine[] = [];
            var asyncQueue: AsyncQueue = new AsyncQueue();

            options.cartLinePrices.forEach((cp: CartLinePrice): void => {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var cartLine: Entities.CartLine = new Entities.CartLineClass(cp.cartLine);
                    if (!ObjectExtensions.isNullOrUndefined(cp.price)) {
                        cartLine.Price = cp.price;
                        updatedCartLines.push(cartLine);
                        return VoidAsyncResult.createResolved();
                    }

                    // If no prices passed in, get the prices.
                    var activityContext: Activities.GetPriceOverrideActivityContext = { cartLine: cartLine };
                    var activity: Activities.GetPriceOverrideActivity = new Activities.GetPriceOverrideActivity(activityContext);

                    // Hook up a response handler to the activity.
                    activity.responseHandler = (response: Activities.GetPriceOverrideActivityResponse): IVoidAsyncResult => {
                        var newPrice: number = response.newPrice;
                        if (ObjectExtensions.isNullOrUndefined(newPrice) || isNaN(newPrice)) {
                            return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_NOT_A_NUMBER)]);
                        }

                        // Validate that the amount is valid
                        var errors: Entities.Error[] = PriceOverrideOperationHandler.validate(cartLine, newPrice);

                        if (ArrayExtensions.hasElements(errors)) {
                            return VoidAsyncResult.createRejected(errors);
                        }

                        cartLine.Price = newPrice;
                        updatedCartLines.push(cartLine);
                        return VoidAsyncResult.createResolved();
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            });

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var priceOverrideQueue: AsyncQueue = this.priceOverrideAsyncQueue(updatedCartLines);
                return asyncQueue.cancelOn(priceOverrideQueue.run());
            });

            return asyncQueue.run();
        }

        /**
         * Creates an async queue for overriding prices on given cart lines. It also takes care of required reason codes.
         * @param {Entities.CartLines[]} cartLines The cartLines for which to do price override.
         * @return {AsyncQueue} The async queue.
         */
        private priceOverrideAsyncQueue(cartLines: Entities.CartLine[]): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            cartLines.forEach((cartLine: Entities.CartLine) => {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    // cart line override price
                    var updateCartBeforeOverride: boolean = !StringExtensions.isNullOrWhitespace(
                        this.applicationContext.deviceConfiguration.OverridePrice);
                    var retryQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cartLines: [cartLine] },
                        (context: ReasonCodesContext) => {
                            var updateAsyncQueue: AsyncQueue = new AsyncQueue();
                            if (updateCartBeforeOverride) {
                                updateAsyncQueue.enqueue((): IVoidAsyncResult => {
                                    // updates the cart with the added reason codes
                                    return this.cartManager.updateCartLinesOnCartAsync(context.cartLines);
                                });
                            }

                            updateCartBeforeOverride = true;

                            updateAsyncQueue.enqueue((): IVoidAsyncResult => {
                                return this.cartManager.priceOverrideAsync(context.cartLines[0]);
                            });

                            return updateAsyncQueue.run();
                        },
                        Entities.ReasonCodeSourceType.OverridePrice);

                    return asyncQueue.cancelOn(retryQueue.run());
                });
            });

            return asyncQueue;
        }

        /**
         * Checks that the operation can be successfully performed on a cart line when the cart line is ready for submission.
         * The checks do not pre-run the checks in the preOperationValidation method
         *
         * Checks executed:
         * 1. If price is 0, then check that the product allows the price to be set to 0
         * 2. Employee restrictions on price
         * 3. Price exceeds MaximumPrice
         * 4. Item does not allow zero price
         * 5. Product restrictions on price
         * 
         * @param {Entities.CartLine} cartLine The cart line with the updated price to validate.
         * @param {number} newPrice The new price value to be set on the item.
         * @return {Entities.Error[]} The list of errors found, null if there was no errors.
         */
        
        private static validate(cartLine: Entities.CartLine, newPrice: number): Entities.Error[] {
        
            var errors: Entities.Error[] = [];

            // Check that the amount is a number
            if (isNaN(newPrice)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_NOT_A_NUMBER));
                return errors;
            }

            // Compute whether the product allows price override to zero.
            // The default behavior is that a product does not allow price override to zero.
            var product: Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            var allowPriceOverrideToZero: boolean =
                !ObjectExtensions.isNullOrUndefined(product.Behavior.IsZeroSalePriceAllowed) && product.Behavior.IsZeroSalePriceAllowed;

            // Check that the amount is not negative
            if (newPrice < 0) {
                if (allowPriceOverrideToZero) {
                    errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_CANNOT_BE_NEGATIVE));
                } else {
                    errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_MUST_BE_POSITIVE));
                }
                return errors;
            }

            // Check that the amount is a valid amount for the currency
            if (!Helpers.CurrencyHelper.isValidAmount(newPrice)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_INVALID_PRICE));
                return errors;
            }

            // If the price is 0, check whether item does not allow zero price
            if (newPrice === 0) {
                if (!allowPriceOverrideToZero) {
                    errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_CANNOT_BE_ZERO));
                    return errors;
                }
            }

            // Check employee price override
            if ((newPrice > cartLine.Price) &&
                (OperationsManager.instance.currentOperationEmployee.Permissions.AllowPriceOverride === Entities.EmployeePriceOverrideType.LowerOnly)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_ONLY_LOWER_AMOUNTS_ALLOWED));
                return errors;
            }

            if ((newPrice < cartLine.Price) &&
                (OperationsManager.instance.currentOperationEmployee.Permissions.AllowPriceOverride === Entities.EmployeePriceOverrideType.HigherOnly)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_ONLY_HIGHER_AMOUNTS_ALLOWED));
                return errors;
            }

            if (OperationsManager.instance.currentOperationEmployee.Permissions.AllowPriceOverride === Entities.EmployeePriceOverrideType.NotAllowed) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_NONE_ALLOWED));
                return errors;
            }

            // Check product price override
            if ((newPrice > cartLine.Price) &&
                (product.Behavior.KeyInPriceValue === Entities.KeyInPriceRestriction.LowerOrEqualPrice)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_ONLY_LOWER_OR_EQUAL_AMOUNTS_ALLOWED));
                return errors;
            }

            if ((newPrice < cartLine.Price) &&
                (product.Behavior.KeyInPriceValue === Entities.KeyInPriceRestriction.HigherOrEqualPrice)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_ONLY_HIGHER_OR_EQUAL_AMOUNTS_ALLOWED));
                return errors;
            }

            // Check whether price exceeds maximum price
            if (!ObjectExtensions.isNullOrUndefined(ApplicationContext.Instance.deviceConfiguration.MaximumPrice)
                && (ApplicationContext.Instance.deviceConfiguration.MaximumPrice !== 0)
                && (ApplicationContext.Instance.deviceConfiguration.MaximumPrice < newPrice)) {
                errors.push(new Entities.Error(ErrorTypeEnum.PRICE_OVERRIDE_PRICE_EXCEEDS_MAXIMUM_DEVICE_PRICE));
            }

            return errors.length === 0 ? null : errors;
        }
    }
}