/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='../Entities/DeliveryModeStatusEnum.ts'/>
///<reference path='../Extensions/ArrayExtensions.ts'/>
///<reference path='../Extensions/DateExtensions.ts'/>
///<reference path='../Session.ts'/>

module Commerce {
    "use strict";

    export class DeliveryHelper {

        /**
         * Gets specified delivery mode of the cart (Deliver All or Deliver Selected).
         * @param {Model.Entities.Cart} cart to check.
         * @return {Model.Entities.DeliveryModeStatusEnum} The cart delivery mode.
         */
        public static getCartDeliveryMode(cart: Model.Entities.Cart): Model.Entities.DeliveryModeStatusEnum {

            if (ObjectExtensions.isNullOrUndefined(cart)
                || !ArrayExtensions.hasElements(cart.CartLines)
                || cart.CartTypeValue != Model.Entities.CartType.CustomerOrder) {
                return Model.Entities.DeliveryModeStatusEnum.None;
            }

            var nonVoidedCartLines = CartHelper.GetNonVoidedCartLines(cart.CartLines);
            var firstCartLineOrder: Model.Entities.CartLine = ArrayExtensions.firstOrUndefined(nonVoidedCartLines, (cartLine: Model.Entities.CartLine) => {
                return !StringExtensions.isNullOrWhitespace(cartLine.DeliveryMode);
            });

            if (ObjectExtensions.isNullOrUndefined(firstCartLineOrder)) {
                return Model.Entities.DeliveryModeStatusEnum.None;
            }

            var isLineDeliveryMode: boolean = nonVoidedCartLines.some((cartLine) => {

                if (DeliveryHelper.isCartLineDeliveryOptionsEmpty(cartLine)) {
                    return false;
                }

                var sameShippingAddress: boolean = (ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress)
                        && ObjectExtensions.isNullOrUndefined(firstCartLineOrder.ShippingAddress))
                    || (!ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress) && (!ObjectExtensions.isNullOrUndefined(firstCartLineOrder.ShippingAddress))
                        && cartLine.ShippingAddress.RecordId === firstCartLineOrder.ShippingAddress.RecordId);

                var isDeliverySettingsUnique: boolean = cartLine.DeliveryModeChargeAmount > 0
                    || cartLine.DeliveryMode != firstCartLineOrder.DeliveryMode
                    || !DateExtensions.areEqual(DateExtensions.getDate(cartLine.RequestedDeliveryDate), DateExtensions.getDate(firstCartLineOrder.RequestedDeliveryDate))
                    || !sameShippingAddress;

                return isDeliverySettingsUnique;
            });

            return isLineDeliveryMode
                ? Model.Entities.DeliveryModeStatusEnum.Selected
                : Model.Entities.DeliveryModeStatusEnum.All;
        }

        /**
         * Check that specified cart line have no delivery information
         * @param {Model.Entities.CartLine} cartLine to check
         * @return {boolean} true if specified cart line does not have delivery options, false otherwise
         */
        public static isCartLineDeliveryOptionsEmpty(cartLine: Model.Entities.CartLine): boolean {

            var isDeliveryModeSet: boolean = !StringExtensions.isNullOrWhitespace(cartLine.DeliveryMode);

            var isShippingAddressSet: boolean = !ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress)
                && !ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress.RecordId)
                && cartLine.ShippingAddress.RecordId != 0;

            var isRequestedDeliveryDateSet: boolean = !ObjectExtensions.isNullOrUndefined(cartLine.RequestedDeliveryDate)
                && cartLine.RequestedDeliveryDate.getTime() > 0;

            return ObjectExtensions.isNullOrUndefined(cartLine) || !isDeliveryModeSet && !isShippingAddressSet && !isRequestedDeliveryDateSet;
        }

        /**
         * Determines whether we need to clear header delivery information.
         *
         * @param {Model.Entities.Cart} cart The current cart.
         * @param {Model.Entities.CartLine[]} selectedCartLines Selected cart lines array.
         * @return {boolean} True if we have to clear delivery info from the header, otherwise false.
         */
        public static mustClearHeaderDeliveryInfo(cart: Model.Entities.Cart, selectedCartLines: Model.Entities.CartLine[]): boolean {
            if (ObjectExtensions.isNullOrUndefined(cart) || !ArrayExtensions.hasElements(selectedCartLines) ||
                !CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {

                // No need to show dialog if cart is not customer order / quotation.
                return false;
            } else if (CartHelper.areAllCartLinesSelected(cart, selectedCartLines)) {
                // No need to show dialog if all cart lines are selected, the pick up selected will be
                // converted to pick up all or ship selected will be converted to ship all.
                return false;
            } else if (cart.DeliveryModeChargeAmount > 0) {
                // Show dialog if header delivery charge amount exists, since this value
                // will be replaced.
                return true;
            }

            var containsEmptyDeliveryMode: boolean = selectedCartLines.some((cartLine: Model.Entities.CartLine) => {
                return DeliveryHelper.isCartLineDeliveryOptionsEmpty(cartLine);
            });

            return !containsEmptyDeliveryMode &&
                DeliveryHelper.getCartDeliveryMode(cart) === Model.Entities.DeliveryModeStatusEnum.All;
        }

        /**
         * Clears delivery information from cart
         *
         * @param {Model.Entities.Cart} cart The cart, which delivery informations should be cleared from the header.
         */
        public static clearHeaderDeliveryInfo(cart: Model.Entities.Cart): void {
            if (!ObjectExtensions.isNullOrUndefined(cart)) {
                cart.ShippingAddress = new Model.Entities.AddressClass();
                cart.DeliveryMode = null;
                cart.DeliveryModeChargeAmount = 0;
                cart.RequestedDeliveryDate = null;
            }
        }

        /**
         * Clears delivery information from cart lines
         *
         * @param {Model.Entities.CartLine[]} cartLines The cart lines, which delivery informations should be cleared.
         * @param {(cartLine: T) => boolean} predicate? An optional predicate function, to filter cart lines.
         */
        public static clearLinesDeliveryInformation(cartLines: Model.Entities.CartLine[],
            predicate?: (cartLine: Model.Entities.CartLine) => boolean): void {

            if (!ArrayExtensions.hasElements(cartLines)) {
                return;
            }

            if (!ObjectExtensions.isNullOrUndefined(predicate)) {
                cartLines = cartLines.filter((cartLine: Model.Entities.CartLine) => predicate(cartLine));
            }

            cartLines.forEach((cartLine: Model.Entities.CartLine) => {
                DeliveryHelper.clearLineDeliveryInformation(cartLine);
            });
        }

        /**
         * Clears delivery information from cartLine
         *
         * @param {T extends Model.Entities.CartLine} cartLine The cart line, which delivery informations should be cleared.
         */
        private static clearLineDeliveryInformation<T extends Model.Entities.CartLine>(cartLine: T): void {
            if (!ObjectExtensions.isNullOrUndefined(cartLine)) {
                cartLine.ShippingAddress = new Model.Entities.AddressClass();
                cartLine.DeliveryMode = null;
                cartLine.DeliveryModeChargeAmount = 0;
                cartLine.RequestedDeliveryDate = null;
            }
        }

        /**
         * Validates cart entity if it's eligible to be picked up or shipped.
         *
         * @param {Model.Entities.Cart} cart The cart to be validated.
         * @param {boolean} validateCustomerOrder True if we also wants to validate cart as customer order, false otherwise.
         * @returns {IVoidAsyncResult} The async result.
         */
        public static validateCartForShippingOrPickup(cart: Model.Entities.Cart, validateCustomerOrder: boolean = true): IVoidAsyncResult {
            cart = cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });

            // Check that the cart is in customer order or quote create or edit mode.
            if (!CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart) && validateCustomerOrder) {
                // The operation can be performed on create or edit customer order or quotation only
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CREATE_OR_EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY)]);
            }

            // Validate if cart has at least one valid cart lines.
            var cartLines: Model.Entities.CartLine[] = CartHelper.GetNonVoidedCartLines(cart.CartLines);

            if (!ArrayExtensions.hasElements(cartLines)) {
                // There is nothing in the cart. Add a product to the cart, and then try again.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.CART_IS_EMPTY)]);
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Validates cart lines entity if it's eligible to be picked up or shipped.
         *
         * @param {Model.Entities.Cart} cart The cart to be validated.
         * @param {Model.Entities.CartLine[]} cartLines Selected cart lines to be validated.
         * @returns {IVoidAsyncResult} The async result.
         */
        public static validateCartLinesForShippingOrPickup(cart: Model.Entities.Cart, cartLines: Model.Entities.CartLine[]): IVoidAsyncResult {
            cart = cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });
            cartLines = cartLines || [];

            if (!ArrayExtensions.hasElements(cartLines)) {
                // The operation can't be performed because a cart line wasn't selected. Select a cart line and then try again.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_NO_CART_LINE_SELECTED)]);
            }

            if (ArrayExtensions.firstOrUndefined(cartLines, c => c.IsVoided)) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.SELECTED_CARTLINES_CONTAINS_VOIDED_PRODUCTS)]);
            }

            // Validate if not all cart lines are selected.
            // If they are, then we should show message to use operation ship all / pick up all instead.
            if (CartHelper.areAllCartLinesSelected(cart, cartLines)) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.ALL_PRODUCTS_SELECTED_PICKUP_OR_SHIP_SELECTED)]);
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Validates ship to address properties when we want to ship a product to address.
         *
         * @param {Model.Entities.Address} customerAddress The customer address to be validated.
         * @param {string} deliveryModeCode The delivery mode code to be validated.
         * @param {number} chargeAmount The charge amount for ship to address.
         * @returns {IVoidAsyncResult} The async result.
         */
        public static validateShippingProperties(
            customerAddress: Model.Entities.Address,
            deliveryModeCode: string,
            chargeAmount: number): IVoidAsyncResult {

            // Validate if customer address is valid
            if (ObjectExtensions.isNullOrUndefined(customerAddress) ||
                ObjectExtensions.isNullOrUndefined(customerAddress.RecordId) ||
                customerAddress.RecordId <= 0) {
                // Cannot continue operation because no addresses have been selected.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NO_ADDRESSES_SELECTED_FOR_SHIP)]);
            }

            // Validate if delivery mode code is valid
            if (StringExtensions.isNullOrWhitespace(deliveryModeCode)) {
                // Cannot continue operation because no shipping methods have been selected.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NO_SHIPPING_METHODS_SELECTED_FOR_SHIP)]);
            }

            // Validate if charges are valid
            if (!Commerce.Helpers.CurrencyHelper.isValidAmount(chargeAmount) || chargeAmount < 0) {
                // The number that was entered for the shipping charge isn't valid.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.INVALID_SHIPPING_CHARGES)]);
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Validates pick up in store properties when we want to pick up product in store.
         * 
         * @param {string} storeNumber The store number to be picked up.
         * @returns {IVoidAsyncResult} The async result.
         */
        public static validatePickupProperties(storeNumber: string): IVoidAsyncResult {

            // Validate if store number is selected (not empty).
            if (StringExtensions.isNullOrWhitespace(storeNumber)) {
                //No stores have been selected for this pickup.
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NO_STORE_SELECTED_FOR_PICKUP)]);
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Gets the store address to be picked up.
         *
         * @param {string} storeNumber Store number to be picked up.
         * @param {Model.Entities.Address} storeAddress (Optional) The address entity to be picked up.
         * @returns {IAsyncResult<Model.Entities.Address>} The async result contains valid store address for pick up products.
         */
        public static getStorePickupAddress(storeNumber: string, storeAddress?: Model.Entities.Address): IAsyncResult<Model.Entities.Address> {

            //Get store address if storeAddress parameter is empty.
            if (ObjectExtensions.isNullOrUndefined(storeAddress)
                || ObjectExtensions.isNullOrUndefined(storeAddress.RecordId)
                || storeAddress.RecordId <= 0) {

                var channelManager: Model.Managers.IChannelManager = Model.Managers.Factory.GetManager(
                    Model.Managers.IChannelManagerName, null);

                return channelManager.getStoreDetailsAsync(storeNumber)
                    .map((storeDetails: Model.Entities.OrgUnit) => {
                        return storeDetails.OrgUnitAddress;
                    });
            }

            return AsyncResult.createResolved(storeAddress);
        }

        /**
         * Set delivery properties for cart lines.
         *
         * @param {Model.Entities.CartLine[]} cartLines Cart lines to be set for delivery.
         * @param {number} chargeAmount The charge amount.
         * @param {string} deliveryMode The delivery mode code.
         * @param {Date} deliveryDate The delivery date.
         * @param {Model.Entities.Address} address Address to be shipped / picked up.
         * @param {string} storeNumber The store number if to be picked up.
         */
        public static setDeliveryForCartLines(cartLines: Model.Entities.CartLine[],
            chargeAmount: number, deliveryMode: string, deliveryDate: Date,
            address: Model.Entities.Address, storeNumber: string) {

            cartLines.forEach((cartLine: Model.Entities.CartLine) => {
                cartLine.DeliveryModeChargeAmount = chargeAmount;
                cartLine.DeliveryMode = deliveryMode;
                cartLine.RequestedDeliveryDate = deliveryDate;
                cartLine.ShippingAddress = address;
                cartLine.FulfillmentStoreId = storeNumber;
            });
        }

        /**
         * Calculates total delivery charges from a list of selected cart lines.
         *
         * @param {Model.Entities.Cart} cart The current cart.
         * @param {Model.Entities.CartLine[]} cartLines The list of selected cart lines to be counted for charge lines.
         * @returns {number} The total delivery charge amount on selected cart lines.
         */
        public static calculateDeliveryCharges(cart: Model.Entities.Cart, cartLines: Model.Entities.CartLine[]): number {
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                return 0;
            }

            if (!ArrayExtensions.hasElements(cartLines) || CartHelper.areAllCartLinesSelected(cart, cartLines)) {
                return cart.DeliveryModeChargeAmount;
            }

            var cartLineCharges: number = 0;
            cartLines.forEach((cartLine: Model.Entities.CartLine) => {
                cartLineCharges += cartLine.DeliveryModeChargeAmount;
            });

            return cartLineCharges;
        }

        /**
         * Get the delivery date from a list of selected cart lines.
         *
         * @param {Model.Entities.Cart} cart The current cart.
         * @param {Model.Entities.CartLine[]} cartLines The list of selected cart lines.
         * @returns {number} The delivery date value on selected cart lines.
         */
        public static getDeliveryDate(cart: Model.Entities.Cart, cartLines: Model.Entities.CartLine[]): Date {
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                return DateExtensions.getDate();
            }

            if (!ArrayExtensions.hasElements(cartLines) || CartHelper.areAllCartLinesSelected(cart, cartLines)) {
                return cart.RequestedDeliveryDate;
            }

            return cartLines[0].RequestedDeliveryDate;
        }
    }
}