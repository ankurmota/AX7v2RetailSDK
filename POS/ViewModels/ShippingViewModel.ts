/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    /**
     * Represents the delivery view model.
     */
    export class ShippingViewModel extends ViewModelBase {

        constructor() {
            super();
        }

        /**
         * Gets order delivery modes for cart lines.
         *
         * @param {string} shippingAddress: Shipping address
         * @param {Model.Entities.CartLine[]} cartLines: Cart lines
         * @return {IAsyncResult<Model.Entities.SalesLineDeliveryOption[]>} The async result.
         */
        public getDeliveryModes(
            shippingAddress: Model.Entities.Address,
            cartLines: Model.Entities.CartLine[]): IAsyncResult<Model.Entities.SalesLineDeliveryOption[]> {

            if (!ArrayExtensions.hasElements(cartLines)) {
                return AsyncResult.createResolved([]);
            }

            var cartLineIds: string[] = cartLines.map((line: Model.Entities.CartLine) => line.LineId);

            return this.cartManager.getDeliveryModes(shippingAddress, cartLineIds);
        }

        /**
         * Set pick up in store delivery mode.
         *
         * @param {Model.Entities.Cart} cart The current cart.
         * @param {Model.Entities.CartLine[]} cartLines The selected cart lines. This is used when doing pick up selected products.
         * @param {string} storeNumber The store number identifier for picking up products.
         * @param {Date} pickupDate The date for pick up.
         * @param {Model.Entities.Address} [storeAddress] Optional Store address for pick up.
         * @returns {IVoidAsyncResult} The async result.
         */
        public setPickupInStoreDeliveryAsync(
            cart: Model.Entities.Cart,
            cartLines: Model.Entities.CartLine[],
            storeNumber: string,
            pickupDate: Date,
            storeAddress?: Model.Entities.Address): IVoidAsyncResult {

            var isPickupAll: boolean = CartHelper.areAllCartLinesSelected(cart, cartLines);

            if (isPickupAll) {
                var operationPickupAllOptions: Operations.IPickupAllOperationOptions = {
                    cart: cart,
                    pickupDate: pickupDate,
                    storeAddress: storeAddress,
                    storeNumber: storeNumber,
                };

                return this.operationsManager.runOperation(
                    Operations.RetailOperation.PickupAllProducts, operationPickupAllOptions);
            }

            var operationPickupSelectedOptions: Operations.IPickupSelectedOperationOptions = {
                cart: cart,
                cartLines: cartLines,
                pickupDate: pickupDate,
                storeAddress: storeAddress,
                storeNumber: storeNumber
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.PickupSelectedProducts, operationPickupSelectedOptions);
        }

        /**
         * Set ship to address delivery mode.
         *
         * @param {Model.Entities.Cart} cart The current cart.
         * @param {Model.Entities.CartLine[]} cartLines The selected cart lines. This is used when doing pick up selected products.
         * @param {string} deliveryModeCode The delivery mode code.
         * @param {Date} shipDate The date for ship the products.
         * @param {number} chargeAmount The shipping charge amount.
         * @param {Model.Entities.Address} customerAddress (Optional) The customer address for shipping the product.
         * @returns {IVoidAsyncResult} The async result.
         */
        public setShipDeliveryAddressAsync(
            cart: Model.Entities.Cart,
            cartLines: Model.Entities.CartLine[],
            deliveryModeCode: string,
            shipDate: Date,
            chargeAmount: number,
            customerAddress: Model.Entities.Address): IVoidAsyncResult {

            var isShipAll: boolean = CartHelper.areAllCartLinesSelected(cart, cartLines);

            if (isShipAll) {
                var operationShipAllOptions: Operations.IShipAllOperationOptions = {
                    cart: cart,
                    shipDate: shipDate,
                    customerAddress: customerAddress,
                    deliveryModeCode: deliveryModeCode,
                    chargeAmount: chargeAmount,
                };

                return this.operationsManager.runOperation(
                    Operations.RetailOperation.ShipAllProducts, operationShipAllOptions);
            }

            var operationShipSelectedOptions: Operations.IShipSelectedOperationOptions = {
                cart: cart,
                cartLines: cartLines,
                shipDate: shipDate,
                customerAddress: customerAddress,
                deliveryModeCode: deliveryModeCode,
                chargeAmount: chargeAmount,
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.ShipSelectedProducts, operationShipSelectedOptions);
        }
    }
}