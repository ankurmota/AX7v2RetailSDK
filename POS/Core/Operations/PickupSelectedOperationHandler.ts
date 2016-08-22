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
     * Options passed to the PickupSelected operation.
     */
    export interface IPickupSelectedOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart; // Current cart.
        cartLines: Model.Entities.CartLine[]; // Selected cart lines.
        pickupDate: Date; // Pick up date.
        storeAddress: Model.Entities.Address; // Store address for pick up.
        storeNumber: string; // Store number for pick up.
    }

    /**
     * Handler for the PickupSelected operation.
     */
    export class PickupSelectedOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PickupSelected operation.
         *
         * @param {IPickupSelectedOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPickupSelectedOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {
                cart: undefined,
                cartLines: undefined,
                pickupDate: undefined,
                storeAddress: undefined,
                storeNumber: undefined
            };

            options.cart = options.cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });
            options.pickupDate = options.pickupDate || DateExtensions.getDate();
            options.cartLines = options.cartLines || [];
            options.storeNumber = options.storeNumber || StringExtensions.EMPTY;
            options.storeAddress = options.storeAddress || null;

            // Set that we have until the end of the day to pickup products.
            // Please note there is no specific time zone - treat it as a destination's time.
            var posTimeZoneOffset = options.pickupDate.getTimezoneOffset() * 60000;
            options.pickupDate = new Date(options.pickupDate.setHours(23, 59, 59) - posTimeZoneOffset);

            var cart: Model.Entities.Cart = options.cart;
            var cartLines: Model.Entities.CartLine[] = options.cartLines;
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return DeliveryHelper.validateCartForShippingOrPickup(cart);
                }).enqueue(() => {
                    return DeliveryHelper.validateCartLinesForShippingOrPickup(cart, cartLines);
                }).enqueue(() => {
                    return DeliveryHelper.validatePickupProperties(options.storeNumber);
                }).enqueue(() => {
                    return DeliveryHelper.getStorePickupAddress(options.storeNumber, options.storeAddress)
                        .done((address: Model.Entities.Address) => {
                            options.storeAddress = address;
                        });
                }).enqueue(() => {
                    // We need to update both cartLines and cart to update delivery modes.
                    var newCart: Model.Entities.Cart = <Model.Entities.Cart>{
                        Id: cart.Id,
                        DeliveryMode: StringExtensions.EMPTY,
                        RequestedDeliveryDate: null,
                        ShippingAddress: null,
                        DeliveryModeChargeAmount: 0, //Pickup charge is always 0
                    };

                    return this.cartManager.createOrUpdateCartAsync(newCart);
                }).enqueue(() => {
                    // Update cart lines
                    DeliveryHelper.setDeliveryForCartLines(cartLines, 0, //Pickup charge is always 0
                        ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode,
                        options.pickupDate, options.storeAddress, options.storeNumber);

                    return this.cartManager.updateCartLinesOnCartAsync(cartLines);
                });

            return asyncQueue.run();
        }
    }
}