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

    import Entities = Proxy.Entities;
    import CartLine = Entities.CartLine;
    import GetCartLineUnitOfMeasuresActivity = Activities.GetCartLineUnitOfMeasuresActivity;
    import GetCartLineUnitOfMeasuresActivityResponse = Activities.GetCartLineUnitOfMeasuresActivityResponse;
    import UnitOfMeasure = Entities.UnitOfMeasure;

    type CartLineUnitOfMeasure = { cartLine: CartLine; unitOfMeasure: UnitOfMeasure; };
    type CartLineUnitOfMeasureArray = CartLineUnitOfMeasure[];
    type CartLineWithUnitOfMeasureOptions = { cartLine: CartLine; unitOfMeasureOptions: UnitOfMeasure[] };

    /**
     * Options passed to the ChangeUnitOfMeasure operation.
     */
    export interface IChangeUnitOfMeasureOperationOptions extends IOperationOptions {
        cartLineUnitOfMeasures: {
            cartLine: CartLine;
            unitOfMeasure: UnitOfMeasure;
        }[];
    }

    /**
     * Handler for the ChangeUnitOfMeasure operation.
     */
    export class ChangeUnitOfMeasureOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ChangeUnitOfMeasure operation.
         * @param {IChangeUnitOfMeasureOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IChangeUnitOfMeasureOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cartLineUnitOfMeasures: undefined };
            options.cartLineUnitOfMeasures = options.cartLineUnitOfMeasures || [];

            var errors: Entities.Error[] = this.validateOperationOptions(options);
            if (ArrayExtensions.hasElements(errors)) {
                return VoidAsyncResult.createRejected(errors);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var cartLinesForUnitOfMeasureUpdate: CartLine[] = [];
            var cartLinesToUpdate: CartLine[] = [];

            // filters out which cart lines already have the unit of measure to be set
            // and which ones need a unit of measure
            options.cartLineUnitOfMeasures.forEach((cu: CartLineUnitOfMeasure): void => {
                if (ObjectExtensions.isNullOrUndefined(cu.unitOfMeasure)) {
                    cartLinesForUnitOfMeasureUpdate.push(cu.cartLine);
                } else {
                    var clonedCartLine: CartLine = ObjectExtensions.clone(cu.cartLine);
                    clonedCartLine.UnitOfMeasureSymbol = cu.unitOfMeasure.Symbol;
                    cartLinesToUpdate.push(clonedCartLine);
                }
            });

            // get unit of measures for cart lines
            if (ArrayExtensions.hasElements(cartLinesForUnitOfMeasureUpdate)) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    return this.getCartLineUnitOfMeasures(cartLinesForUnitOfMeasureUpdate)
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
         * @param {IChangeUnitOfMeasureOperationOptions} options The ChangeUnitOfMeasure operation options.
         * @return {Entities.Error[]} The list of errors found, empty if there was no errors.
         */
        private validateOperationOptions(options: IChangeUnitOfMeasureOperationOptions): Entities.Error[] {
            var cart: Model.Entities.Cart = Session.instance.cart;

            // Check that the cart is not for an income/expense transaction
            if (CartHelper.isCartType(cart, Model.Entities.CartType.IncomeExpense)) {
                return [new Entities.Error(
                    ErrorTypeEnum.OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION)];
            }

            // if no collection is passed or at least one cart line is invalid, return error
            if (!ArrayExtensions.hasElements(options.cartLineUnitOfMeasures)
                || options.cartLineUnitOfMeasures.some((c: CartLineUnitOfMeasure) => ObjectExtensions.isNullOrUndefined(c))) {
                return [new Entities.Error(
                    ErrorTypeEnum.UNIT_OF_MEASURE_NOT_VALID_NO_ITEM_SELECTED)];
            }

            var errors: Entities.Error[] = [];
            options.cartLineUnitOfMeasures.forEach((cu: CartLineUnitOfMeasure) => {
                var cartLineErrors: Entities.Error[] = this.preValidateCartLine(cu.cartLine);
                if (ArrayExtensions.hasElements(cartLineErrors)) {
                    errors = errors.concat(cartLineErrors);
                }
            });

            return errors;
        }

        /**
         * Gets units of measure for the given cart lines.
         * @param {CartLine[]} cartLines The cart lines to get quanitities for.
         * @return {IAsyncResult<CartLine[]>} The async result containing the updated cart lines.
         */
        private getCartLineUnitOfMeasures(cartLines: CartLine[]): IAsyncResult<CartLine[]> {
            var unitsOfMeasureQueue: AsyncQueue = new AsyncQueue();
            var cartLinesWithUnitOfMeasureOptions: CartLineWithUnitOfMeasureOptions[] = [];
            var updatedCartLines: CartLine[];

            cartLines.forEach((cartLine: CartLine): void => {
                unitsOfMeasureQueue.enqueue((): IVoidAsyncResult => {
                    var getUnitsOfMeasureResult: VoidAsyncResult = new VoidAsyncResult();
                    this.productManager.getUnitsOfMeasureAsync(cartLine.ProductId).done((unitsOfMeasure: UnitOfMeasure[]): void => {
                        if (ArrayExtensions.hasElements(unitsOfMeasure)) {
                            var cartLineWithUnitOfMeasureOptions: CartLineWithUnitOfMeasureOptions = {
                                cartLine: cartLine,
                                unitOfMeasureOptions: unitsOfMeasure
                            };

                            cartLinesWithUnitOfMeasureOptions.push(cartLineWithUnitOfMeasureOptions);
                            getUnitsOfMeasureResult.resolve();
                        } else {
                            var notFoundError: Entities.Error = new Entities.Error(ErrorTypeEnum.UNIT_OF_MEASURE_CONVERSION_NOT_DEFINED);
                            getUnitsOfMeasureResult.reject([notFoundError]);
                        }
                    }).fail((getUnitsOfMeasureErrors: Proxy.Entities.Error[]): void => {
                        getUnitsOfMeasureResult.reject(getUnitsOfMeasureErrors);
                    });

                    return getUnitsOfMeasureResult;
                });
            });

            unitsOfMeasureQueue.enqueue((): IVoidAsyncResult => {
                var activity: GetCartLineUnitOfMeasuresActivity =
                    new GetCartLineUnitOfMeasuresActivity({ cartLinesWithUnitOfMeasureOptions: cartLinesWithUnitOfMeasureOptions });

                return activity.execute().map((): CartLine[]=> {
                    if (!activity.response) {
                        return null;
                    }

                    updatedCartLines = cartLines.map((cartLine: CartLine, index: number) => {
                        var clonedCartLine: CartLine = ObjectExtensions.clone(cartLine);
                        clonedCartLine.UnitOfMeasureSymbol = activity.response.selectedUnitsOfMeasure[index].Symbol;
                        return clonedCartLine;
                    });

                    return updatedCartLines;
                });
            });

            return unitsOfMeasureQueue.run().map((queueResult: ICancelableResult): CartLine[]=> {
                return updatedCartLines;
            });
        }

        /**
         * Checks that the operation can be performed on a cart line.
         * @param {CartLine} cartLine The cart line
         * @return {Entities.Error[]} The list of errors found, empty if there was no errors.
         */
        private preValidateCartLine(cartLine: CartLine): Entities.Error[] {
            var errors: Entities.Error[] = [];

            // Check that there are cart lines specified for the operation
            if (ObjectExtensions.isNullOrUndefined(cartLine)) {
                errors.push(new Entities.Error(
                    ErrorTypeEnum.UNIT_OF_MEASURE_NOT_VALID_NO_ITEM_SELECTED));
                return errors;
            }

            // Check that the cart line is not voided
            if (cartLine.IsVoided) {
                errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED));
            }

            // Check that the cart line is not part of a gift card line
            if (cartLine.IsGiftCardLine) {
                errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FOR_A_GIFT_CARD));
            }

            // Check that the cart line is not part of a receipt
            if (CartLineHelper.isFromAReceipt(cartLine)) {
                errors.push(new Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PRODUCT_IS_FOR_A_RECEIPT));
            }

            // Check that unit of measure is defined for the cart line
            if (StringExtensions.isNullOrWhitespace(cartLine.UnitOfMeasureSymbol)) {
                errors.push(new Entities.Error(ErrorTypeEnum.UNIT_OF_MEASURE_CANNOT_BE_CHANGED));
            }

            return errors;
        }
    }
}