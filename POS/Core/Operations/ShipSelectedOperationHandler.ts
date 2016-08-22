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
     * Options passed to the ShipSelected operation.
     */
    export interface IShipSelectedOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
        cartLines: Model.Entities.CartLine[];
        shipDate: Date;
        customerAddress: Model.Entities.Address;
        deliveryModeCode: string;
        chargeAmount: number;
    }

    /**
     * Handler for the ShipSelected operation.
     */
    export class ShipSelectedOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SuspendTransaction operation.
         * @param {IShipSelectedOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IShipSelectedOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {
                cart: undefined,
                cartLines: undefined,
                shipDate: undefined,
                customerAddress: undefined,
                deliveryModeCode: undefined,
                chargeAmount: undefined
            };

            options.cart = options.cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });
            options.cartLines = options.cartLines || [];
            options.customerAddress = options.customerAddress || null;
            options.deliveryModeCode = options.deliveryModeCode || StringExtensions.EMPTY;
            options.shipDate = options.shipDate || DateExtensions.getDate();
            options.chargeAmount = options.chargeAmount || 0;

            // set that we have until the end of the day to deliver products
            DateExtensions.setTimeToLastSecondOfDay(options.shipDate);

            var cart: Model.Entities.Cart = options.cart;
            var cartLines: Model.Entities.CartLine[] = options.cartLines;
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return DeliveryHelper.validateCartForShippingOrPickup(cart);
                }).enqueue(() => {
                    return DeliveryHelper.validateCartLinesForShippingOrPickup(cart, cartLines);
                }).enqueue(() => {
                    return DeliveryHelper.validateShippingProperties(options.customerAddress,
                        options.deliveryModeCode, options.chargeAmount);
                }).enqueue(() => {
                    // We need to update both cartLines and cart to update delivery modes.
                    var newCart: Model.Entities.Cart = <Model.Entities.Cart>{
                        Id: cart.Id,
                        DeliveryMode: StringExtensions.EMPTY,
                        RequestedDeliveryDate: null,
                        ShippingAddress: null,
                        DeliveryModeChargeAmount: 0 // Set charge header 0 since operation is ship selected products
                    };

                    return this.cartManager.createOrUpdateCartAsync(newCart);
                }).enqueue(() => {

                    // Update cart lines
                    DeliveryHelper.setDeliveryForCartLines(cartLines,
                        0, // Set to 0 for now. Splitted delivery charges will be determined later.
                        options.deliveryModeCode,
                        options.shipDate, options.customerAddress,
                        null); // Set store number null when ship products

                    ShipSelectedOperationHandler.splitDeliveryCharges(options.chargeAmount, cartLines);

                    return this.cartManager.updateCartLinesOnCartAsync(cartLines);
                });

            return asyncQueue.run();
        }

        
        /**
         * Splits total delivery charge amount between cart lines
         * @param {Model.Entities.CartLineProduct[]} cartLines Cart lines to update delivery charge
         * @param {number} totalChargeAmount Total amount of charge
         */
        private static splitDeliveryCharges(totalChargeAmount: number, cartLines: Model.Entities.CartLine[]): void {
            var decimalPrecision: number = NumberExtensions.getDecimalPrecision();
            var chargeAmountPerLine: number = NumberExtensions.roundToNDigits(totalChargeAmount / cartLines.length, decimalPrecision);

            var appliedChargeAmount: number = chargeAmountPerLine * cartLines.length;
            var remainingChargeAmount: number = NumberExtensions.roundToNDigits(totalChargeAmount - appliedChargeAmount, decimalPrecision);

            cartLines.forEach((cartLine: Model.Entities.CartLine) => {
                cartLine.DeliveryModeChargeAmount = chargeAmountPerLine;
            });

            // We should add the remaining charge amount to the last cart line
            // Example: We have to split 10$ as a delivery charge for 3 cart lines
            // Result: 3.33, 3.33, 3.34
            ArrayExtensions.lastOrUndefined(cartLines).DeliveryModeChargeAmount += remainingChargeAmount;
        }
        
    }
}