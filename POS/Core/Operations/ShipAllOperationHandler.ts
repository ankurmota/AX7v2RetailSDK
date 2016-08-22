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
     * Options passed to the ShipAll operation.
     */
    export interface IShipAllOperationOptions {
        cart: Model.Entities.Cart;
        shipDate: Date;
        customerAddress: Model.Entities.Address;
        deliveryModeCode: string;
        chargeAmount: number;
    }

    /**
     * Handler for the ShipAll operation.
     */
    export class ShipAllOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ShipAll operation.
         * @param {IShipAllOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IShipAllOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {
                cart: undefined,
                shipDate: undefined,
                customerAddress: undefined,
                deliveryModeCode: undefined,
                chargeAmount: undefined
            };

            options.shipDate = options.shipDate || DateExtensions.getDate();
            options.chargeAmount = options.chargeAmount || 0;
            options.customerAddress = options.customerAddress || null;
            options.deliveryModeCode = options.deliveryModeCode || StringExtensions.EMPTY;

            // set that we have until the end of the day to deliver products
            DateExtensions.setTimeToLastSecondOfDay(options.shipDate);

            var cart: Model.Entities.Cart = options.cart;
            var cartLines: Model.Entities.CartLine[] = [];
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return DeliveryHelper.validateCartForShippingOrPickup(cart);
                }).enqueue(() => {
                    return DeliveryHelper.validateShippingProperties(options.customerAddress,
                        options.deliveryModeCode, options.chargeAmount);
                })
                .enqueue(() => {
                    // We need to update both cartLines and cart to update delivery modes.
                    var newCart: Model.Entities.Cart = <Model.Entities.Cart>{
                        Id: cart.Id,
                        DeliveryMode: options.deliveryModeCode,
                        RequestedDeliveryDate: options.shipDate,
                        ShippingAddress: options.customerAddress,
                        DeliveryModeChargeAmount: options.chargeAmount
                    };

                    return this.cartManager.createOrUpdateCartAsync(newCart);
                }).enqueue(() => {
                    cartLines = CartHelper.GetNonVoidedCartLines(cart.CartLines);

                    // Update cart lines
                    DeliveryHelper.setDeliveryForCartLines(cartLines,
                        0, // Charge on cart lines is 0 since delivery mode header already has the charge
                        options.deliveryModeCode,
                        options.shipDate, options.customerAddress,
                        null); // Set store number null when ship products

                    return this.cartManager.updateCartLinesOnCartAsync(cartLines);
                });

            return asyncQueue.run();
        }
    }
}