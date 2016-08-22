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
     * Options passed to the PickupAll operation.
     */
    export interface IPickupAllOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart; // Current cart.
        pickupDate: Date; // The pick up date.
        storeAddress: Model.Entities.Address; // Store pick up address.
        storeNumber: string; // Store number.
    }

    /**
     * Handler for the PickupAll operation.
     */
    export class PickupAllOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PickupAll operation.
         *
         * @param {IPickupAllOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPickupAllOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {
                cart: undefined,
                pickupDate: undefined,
                storeAddress: undefined,
                storeNumber: undefined
            };

            options.cart = options.cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });
            options.pickupDate = options.pickupDate || DateExtensions.getDate();
            options.storeNumber = options.storeNumber || StringExtensions.EMPTY;
            options.storeAddress = options.storeAddress || null;

            // set that we have until the end of the day to pickup products
            // Please note there is no specific time zone - treat it as a destination's time.
            var posTimeZoneOffset = options.pickupDate.getTimezoneOffset() * 60000;
            options.pickupDate = new Date(options.pickupDate.setHours(23, 59, 59) - posTimeZoneOffset);
            
            var cart: Model.Entities.Cart = options.cart;
            var cartLines: Model.Entities.CartLine[] = [];
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return DeliveryHelper.validateCartForShippingOrPickup(cart);
                }).enqueue(() => {
                    return DeliveryHelper.validatePickupProperties(options.storeNumber);
                }).enqueue(() => {
                    return DeliveryHelper.getStorePickupAddress(options.storeNumber, options.storeAddress)
                        .done((address: Model.Entities.Address) => {
                            options.storeAddress = address;
                        });
                })
                .enqueue(() => {
                    // We need to update both cartLines and cart to update delivery modes.
                    var newCart: Model.Entities.Cart = <Model.Entities.Cart>{
                        Id: cart.Id,
                        DeliveryMode: ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode,
                        RequestedDeliveryDate: options.pickupDate,
                        ShippingAddress: options.storeAddress,
                        DeliveryModeChargeAmount: 0, //Pickup charge is always 0
                    };

                    return this.cartManager.createOrUpdateCartAsync(newCart);
                }).enqueue(() => {
                    cartLines = CartHelper.GetNonVoidedCartLines(cart.CartLines);

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