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
    import CartLine = Entities.CartLine;
    import Error = Entities.Error;
    import GetCartLineQuantitiesActivity = Activities.GetCartLineQuantitiesActivity;
    import GetCartLineQuantitiesActivityResponse = Activities.GetCartLineQuantitiesActivityResponse;
    import GetCartLineWeightActivity = Activities.GetCartLineWeightActivity;
    import GetCartLineWeightActivityResponse = Activities.GetCartLineWeightActivityResponse;

    type CartLineQuantity = { cartLine: CartLine; quantity: number; };
    type CartLineQuantityArray = CartLineQuantity[];

    /**
     * Options passed to the SetQuantity operation.
     */
    export interface ISetQuantityOperationOptions extends IOperationOptions {
        cartLineQuantities: {
            cartLine: CartLine;
            quantity: number;
        }[];
    }

    /**
     * Handler for the SetQuantity operation.
     */
    export class SetQuantityOperationHandler extends PrePostTriggerOperationHandlerBase {
        /**
         * Executes the pre-trigger for the SetQuantity operation.
         * @param {ISetQuantityOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableResult>} The result of the pre-trigger execution.
         */
        protected executePreTrigger(options: ISetQuantityOperationOptions): IAsyncResult<ICancelableResult> {
            var preTriggerOptions: Triggers.IPreSetQuantityTriggerOptions = { cart: Session.instance.cart, operationOptions: options };
            return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreSetQuantity, preTriggerOptions);
        }

        /**
         * Executes the post-trigger for the SetQuantity operation.
         * @param {ISetQuantityOperationOptions} options The operation options.
         * @param {IOperationResult} result The result of the operation.
         * @return {IVoidAsyncResult} The result of the post-trigger execution.
         */
        protected executePostTrigger(options: ISetQuantityOperationOptions, result: IOperationResult): IVoidAsyncResult {
            var cartLineIds: string[] = options.cartLineQuantities.map((cartLineQuantity: CartLineQuantity): string => {
                return cartLineQuantity.cartLine.LineId;
            });

            var updatedCartLines: CartLine[] = CartHelper.getCartLinesByLineIds(Session.instance.cart, cartLineIds);

            var postTriggerOptions: Triggers.IPostSetQuantityTriggerOptions = { cart: Session.instance.cart, cartLines: updatedCartLines };
            return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostSetQuantity, postTriggerOptions);
        }

        /**
         * Sanitizes the options provided to the operation.
         * @param {ISetQuantityOperationOptions} options The provided options.
         * @return {ISetQuantityOperationOptions} The sanitized options.
         */
        protected sanitizeOptions(options: ISetQuantityOperationOptions): ISetQuantityOperationOptions {
            options = options || { cartLineQuantities: undefined };
            options.cartLineQuantities = options.cartLineQuantities || [];
            return options;
        }

        /**
         * Executes the SetQuantity operation.
         * @param {ISetQuantityOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        protected executeInternal(options: ISetQuantityOperationOptions): IAsyncResult<IOperationResult> {
            var errors: Error[] = this.validateOperationOptions(options);
            if (ArrayExtensions.hasElements(errors)) {
                return VoidAsyncResult.createRejected(errors);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var cartLinesForQuantityUpdate: CartLine[] = [];
            var cartLinesForWeightUpdate: CartLine[] = [];
            var cartLinesToUpdate: CartLine[] = [];

            // filters out which cart lines already have the quantity to be set, which ones need a quantity
            // and which ones need a weight
            options.cartLineQuantities.forEach((cq: CartLineQuantity): void => {
                if (ObjectExtensions.isNullOrUndefined(cq.quantity)) {
                    var product: Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cq.cartLine.ProductId);
                    if (!product.Behavior.MustWeighProductAtSale) {
                        cartLinesForQuantityUpdate.push(cq.cartLine);
                    } else {
                        cartLinesForWeightUpdate.push(cq.cartLine);
                    }
                } else {
                    var clonedCartLine: CartLine = ObjectExtensions.clone(cq.cartLine);
                    clonedCartLine.Quantity = cq.quantity;
                    cartLinesToUpdate.push(clonedCartLine);
                }
            });

            // get quantities for the cart lines
            if (ArrayExtensions.hasElements(cartLinesForQuantityUpdate)) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    return this.getCartLineQuantities(cartLinesForQuantityUpdate)
                        .done((cartLines: CartLine[]) => {
                        if (!cartLines) {
                            asyncQueue.cancel();
                            return;
                        }

                        cartLinesToUpdate = cartLinesToUpdate.concat(cartLines);
                    });
                });
            }

            // get weights for the cart lines
            if (ArrayExtensions.hasElements(cartLinesForWeightUpdate)) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    return this.getCartLineWeights(cartLinesForWeightUpdate)
                        .done((cartLines: CartLine[]) => {
                        if (!cartLines) {
                            asyncQueue.cancel();
                            return;
                        }

                        cartLinesToUpdate = cartLinesToUpdate.concat(cartLines);
                    });
                });
            }

            // updates the cart lines and handle missing infocodes
            asyncQueue.enqueue((): IAsyncResult<any> => {
                if (ArrayExtensions.hasElements(cartLinesToUpdate)) {
                    var reasonCodesResult: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cartLines: cartLinesToUpdate },
                        (context: ReasonCodesContext): IVoidAsyncResult => {
                            return this.cartManager.updateCartLinesOnCartAsync(context.cartLines);
                        }).run();

                    return asyncQueue.cancelOn(reasonCodesResult);
                }
            });

            return asyncQueue.run();
        }

        /**
         * Validate the operation options.
         * Checks executed:
         * 1. If the cart is a return customer order, makes sure quantities cannot be changed.
         * 2. The cart is not for an Income/Expense transaction.
         * 3. All cart lines are valid.
         * @param {ISetQuantityOperationOptions} options The SetQuantity operation options.
         * @return {Error[]} The list of errors found, empty if there was no errors.
         */
        private validateOperationOptions(options: ISetQuantityOperationOptions): Error[] {
            var cart: Entities.Cart = Session.instance.cart;

            // Check that the cart is not for an income/expense transaction
            if (CartHelper.isCartType(cart, Entities.CartType.IncomeExpense)) {
                return [new Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION)];
            }

            // if no collection is passed or at least one cart line is invalid, return error
            if (!ArrayExtensions.hasElements(options.cartLineQuantities)
                || options.cartLineQuantities.some((c: CartLineQuantity) => ObjectExtensions.isNullOrUndefined(c))) {
                return [new Error(ErrorTypeEnum.SET_QUANTITY_NOT_VALID_NO_ITEM_SELECTED)];
            }

            var errors: Error[] = [];
            options.cartLineQuantities.forEach((cq: CartLineQuantity) => {
                var cartLineErrors: Error[] = this.preValidateCartLine(cq.cartLine);
                if (!ObjectExtensions.isNullOrUndefined(cq.quantity)) {
                    cartLineErrors = cartLineErrors.concat(this.validateCartLine(cq.cartLine, cq.quantity));
                }

                if (ArrayExtensions.hasElements(cartLineErrors)) {
                    errors = errors.concat(cartLineErrors);
                }
            });

            return errors;
        }

        /**
         * Gets quantities for the given cart lines.
         * @param {CartLine[]} cartLines The cart lines to get quanitities for.
         * @return {IAsyncResult<CartLine[]>} The async result containing the updated cart lines.
         */
        private getCartLineQuantities(cartLines: CartLine[]): IAsyncResult<CartLine[]> {
            var activity: GetCartLineQuantitiesActivity = new GetCartLineQuantitiesActivity({ cartLines: cartLines });

            activity.responseHandler = (response: GetCartLineQuantitiesActivityResponse): IVoidAsyncResult => {
                var errors: Error[] = [];
                var updatedCartLines: CartLine[] = cartLines.map((cartLine: CartLine, index: number) => {
                    var quantity: number = response.quantities[index];

                    // handles rounding for negative quantities
                    if (cartLine.Quantity < 0) {
                        var numberOfDecimals: number = NumberExtensions.getNumberOfDecimals(quantity);
                        quantity = -1 * quantity;

                        if (NumberExtensions.getNumberOfDecimals(quantity) >= numberOfDecimals) {
                            quantity = NumberExtensions.roundToNDigits(quantity, numberOfDecimals);
                        }
                    }

                    errors = errors.concat(this.validateCartLine(cartLine, quantity));

                    var clonedCartLine: CartLine = ObjectExtensions.clone(cartLine);
                    clonedCartLine.Quantity = quantity;
                    return clonedCartLine;
                });

                if (ArrayExtensions.hasElements(errors)) {
                    return VoidAsyncResult.createRejected(errors);
                }

                cartLines = updatedCartLines;
                return VoidAsyncResult.createResolved();
            };

            return activity.execute().map((): CartLine[] => {
                if (!activity.response) {
                    return null;
                }

                return cartLines;
            });
        }

        /**
         * Gets weights for the given cart lines.
         * @param {CartLine[]} cartLines The cart lines to get weights for.
         * @return {IAsyncResult<CartLine[]>} The async result containing the updated cart lines.
         */
        private getCartLineWeights(cartLines: CartLine[]): IAsyncResult<CartLine[]> {
            var updatedCartLines: CartLine[] = [];
            var asyncQueue: AsyncQueue = new AsyncQueue();

            cartLines.forEach((cartLine: CartLine) => {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var activity: GetCartLineWeightActivity = new GetCartLineWeightActivity({ cartLine: cartLine });
                    activity.responseHandler = (response: GetCartLineWeightActivityResponse): IVoidAsyncResult => {
                        var quantity: number = response.weight;

                        // handles rounding for negative quantities
                        if (cartLine.Quantity < 0) {
                            var numberOfDecimals: number = NumberExtensions.getNumberOfDecimals(quantity);
                            quantity = -1 * quantity;

                            if (NumberExtensions.getNumberOfDecimals(quantity) >= numberOfDecimals) {
                                quantity = NumberExtensions.roundToNDigits(quantity, numberOfDecimals);
                            }
                        }

                        var errors: Error[] = this.validateCartLine(cartLine, quantity);
                        if (ArrayExtensions.hasElements(errors)) {
                            return VoidAsyncResult.createRejected(errors);
                        }

                        var clonedCartLine: CartLine = ObjectExtensions.clone(cartLine);
                        clonedCartLine.Quantity = quantity;
                        updatedCartLines.push(clonedCartLine);

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

            return asyncQueue.run().map((result: ICancelableResult): CartLine[] => {
                if (!result.canceled) {
                    return updatedCartLines;
                }

                return null;
            });
        }

        /**
         * Checks that the operation can be performed on a cart line.
         * Checks executed:
         * 1. There are cart lines
         * 2. The cart line is not voided
         * 3. Cart line is not for a gift certificate
         * 4. Serialized items do not support quantity change
         * 5. Key in quantity is allowed for the product
         * @param {CartLine} cartLine The cart line
         * @return {Error[]} The list of errors found, empty if there was no errors.
         */
        private preValidateCartLine(cartLine: CartLine): Error[] {
            var errors: Error[] = [];

            // Check that there are cart lines specified for the operation
            if (ObjectExtensions.isNullOrUndefined(cartLine)) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_VALID_NO_ITEM_SELECTED));
                return errors;
            }

            // Check that the cart line is not voided
            if (cartLine.IsVoided) {
                errors.push(new Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED));
            }

            // Check that the cart line is not part of a gift card line
            if (cartLine.IsGiftCardLine) {
                errors.push(new Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FOR_A_GIFT_CARD));
            }

            var product: Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            var isProduct: boolean = !ObjectExtensions.isNullOrUndefined(product);

            // Check that the product is not a serialized item.
            if ((!ObjectExtensions.isNullOrUndefined(cartLine.SerialNumber)
                && !StringExtensions.isEmptyOrWhitespace(cartLine.SerialNumber))
                || (isProduct && product.Behavior.MustPromptForSerialNumberOnlyAtSale)) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_VALID_FOR_SERIALIZED_ITEM));
            }

            // Check that the item does not allow quantity override
            if (isProduct
                && product.Behavior.KeyInQuantityValue === Entities.KeyInQuantityRestriction.NotAllowed) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_VALID_ONE_OR_MORE_ITEMS));
            }

            return errors;
        }

        /**
         * Checks that the operation can be successfully performed on a cart line when the cart line is ready for submission.
         * The checks do not pre-run the checks in the preOperationValidation method
         * Checks executed:
         * 1. Quantity is a number
         * 2. Quantity is greater than 0 for purchases and less than 0 for returns
         * 3. Quantity does not exceed quantity on the receipt
         * 4. Quantity is not higher than Maximum Quantity for the device
         * 5. Quantity not allowed for the unit of measure
         * 6. Quantity is less or equal than is left to be picked up on customer order pickup
         * @param {CartLine} cartLine The cart line without the updated quantity to validate.
         * @param {number} quantity The quantity to be set for the cart line.
         * @return {Error[]} The list of errors found, empty if there was no errors.
         */
        private validateCartLine(cartLine: CartLine, quantity: number): Error[] {
            var errors: Error[] = [];

            // Check that the amount is a number
            if (isNaN(quantity)) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_A_NUMBER));
                return errors;
            }

            // Check that the quantity is greater than 0 for purchases and less than 0 for returns.
            if ((cartLine.Quantity > 0 && quantity < 0) || (cartLine.Quantity < 0 && quantity > 0) || quantity === 0) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_GREATER_THAN_ZERO));
                return errors;
            }

            // Check whether quantity exceeds maximum quantity
            if (!ObjectExtensions.isNullOrUndefined(this.applicationContext.deviceConfiguration.MaximumQuantity)
                && (this.applicationContext.deviceConfiguration.MaximumQuantity !== 0)
                && (this.applicationContext.deviceConfiguration.MaximumQuantity < Math.abs(quantity))) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_QUANTITY_EXCEEDS_MAXIMUM_DEVICE_QUANTITY));
            }

            var cartLineUnitOfMeasure: Entities.UnitOfMeasure =
                this.applicationContext.unitsOfMeasureMap.getItem(cartLine.UnitOfMeasureSymbol.toLowerCase());

            // Check that the quantity is valid for the unit of measure
            if (!UnitOfMeasureHelper.isQuantityValid(quantity, cartLineUnitOfMeasure)) {
                errors.push(new Error(ErrorTypeEnum.SET_QUANTITY_NOT_VALID_FOR_UNIT_OF_MEASURE));
            }

            // Check that quantity is less or equal than is left to be picked up on customer order pickup
            var cart: Entities.Cart = Session.instance.cart;
            if (cart.CartTypeValue === Entities.CartType.CustomerOrder
                && cart.CustomerOrderModeValue === Entities.CustomerOrderMode.Pickup
                && (quantity + cartLine.QuantityInvoiced > cartLine.QuantityOrdered)) {

                errors.push(new Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOPICKUPMORETHANQTYREMAINING));
            }

            return errors;
        }
    }
}