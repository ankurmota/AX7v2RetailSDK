/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";

    import Entities = Proxy.Entities;

    export class CustomerOrderHelper {

        /**
         * Find out if a serial number needs to be entered for the product.
         * 
         * @param {Entities.SimpleProduct} product The product to be inspected.
         * @param {Entities.CartLine} cartLine The current cart line.
         * @param {Entities.Cart} cart [Optional] The current cart.
         */
        public static isSerializedNumberRequired(product: Entities.SimpleProduct, cartLine: Entities.CartLine, cart?: Entities.Cart): boolean {
            if (ObjectExtensions.isNullOrUndefined(product)
                || ObjectExtensions.isNullOrUndefined(product.Behavior)
                || !product.Behavior.HasSerialNumber) {
                return false;
            }

            if (ObjectExtensions.isNullOrUndefined(cart)) {
                cart = Session.instance.cart;
            }

            if (cart.CartTypeValue !== Entities.CartType.CustomerOrder) {
                if (product.Behavior.HasSerialNumber) {
                    // If product is active in sales process, product is a serialized number.
                    // As long as product has serial number and transaction is not customer order,
                    // prompt the serial number dialog.
                    return true;
                }
            } else {
                //Enter this condition when transaction type is Customer Order.

                if (product.Behavior.MustPromptForSerialNumberOnlyAtSale) {
                    if (cart.CustomerOrderModeValue === Entities.CustomerOrderMode.Pickup && cartLine.Quantity > 0) {
                        // If transaction type customer order and product is active in sales process,
                        // prompt serial number dialog during pickup.
                        return true;
                    } else {
                        // Otherwise do not prompt serial number dialog.
                        return false;
                    }
                } else if (product.Behavior.HasSerialNumber) {
                    if (cart.CustomerOrderModeValue === Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                        || cart.CustomerOrderModeValue === Entities.CustomerOrderMode.QuoteCreateOrEdit
                        || (cart.CustomerOrderModeValue === Entities.CustomerOrderMode.Pickup && cartLine.Quantity > 0)) {

                        // If product is serialized and NOT active in sales process, prompt serial number dialog
                        // when customer / quotation create and edit, or during pickup for selected cart lines.
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Checks whether the cart type is customer order and the specified customer mode type
         *
         * @param {Entities.Cart} cart The cart
         * @param {Commerce.CartTypeValueEnum} customerOrderMode The possible customer order mode.
         * @return {boolean} True if the cart type is customer order and has the specified by customerOrderMode type , false otherwise
         */
        public static isCustomerOrderMode(cart: Entities.Cart, customerOrderMode: Entities.CustomerOrderMode): boolean {
            if (!ObjectExtensions.isNullOrUndefined(customerOrderMode)) {
                return CustomerOrderHelper.isCustomerOrder(cart) && cart.CustomerOrderModeValue === customerOrderMode;
            }

            return false;
        }

        /**
        * Checks whether cart is in customer order mode
        *
        * @return {boolean} Returs true if cart in customer order mode
        */
        public static isCustomerOrder(cart: Entities.Cart): boolean {
            return !ObjectExtensions.isNullOrUndefined(cart) && cart.CartTypeValue === Entities.CartType.CustomerOrder;
        }

        public static isCustomerOrderOrQuoteCreationOrEdition(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrderMode(cart, Entities.CustomerOrderMode.CustomerOrderCreateOrEdit)
                || CustomerOrderHelper.isCustomerOrderMode(cart, Entities.CustomerOrderMode.QuoteCreateOrEdit);
        }

        /**
         * Checks whether cart is in customer order cancellation process    
         *     
         * @return {boolean} Returns true if cart in customer order cancellation process
         */
        public static isCustomerOrderCancellation(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrder(cart) && cart.CustomerOrderModeValue == Entities.CustomerOrderMode.Cancellation;
        }

        /**
        * Checks whether cart is in customer order return process        
        * @return {boolean} Returns true if cart in customer order return process
        */
        public static isCustomerOrderReturnOrPickup(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrder(cart) && (cart.CustomerOrderModeValue === Entities.CustomerOrderMode.Return || cart.CustomerOrderModeValue == Entities.CustomerOrderMode.Pickup);
        }

        /**
         * Checks whether cart is in quote mode
         *
         * @return {boolean} Returs true if cart in quote mode
         */
        public static isQuote(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrder(cart) && cart.CustomerOrderModeValue == Entities.CustomerOrderMode.QuoteCreateOrEdit;
        }

        /**
         * Checks whether cart is in customer order creation process
         *
         * @return {boolean} Returs true if cart in customer order creation process
         */
        public static isCustomerOrderCreationOrEdition(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrder(cart) && cart.CustomerOrderModeValue == Entities.CustomerOrderMode.CustomerOrderCreateOrEdit;
        }

        /**
         * Checks whether cart is in customer order pickup process
         *
         * @return {boolean} Returs true if cart in customer order pickup process
         */
        public static isCustomerOrderPickup(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrder(cart) && cart.CustomerOrderModeValue == Entities.CustomerOrderMode.Pickup;
        }

        /**
         * Checks whether cart is in customer order edition.
         *
         * @returns {boolean} True if cart is customer order edition, false otherwise.
         */
        public static isCustomerOrderEdition(cart: Entities.Cart): boolean {
            return CustomerOrderHelper.isCustomerOrderCreationOrEdition(cart) && !StringExtensions.isNullOrWhitespace(cart.SalesId);
        }

        /**
         * Checks is cart is expired
         *
         * @return {boolean} Returs true if cart is expired
         */
        public static isQuoteExpired(cart: Entities.Cart): boolean {
            var now: Date = new Date();
            return cart.QuotationExpiryDate < now;
        }

        /**
         * Checks if cart has lines configure for shipping (delivery method set different from pick up at store)
         *
         * @return {boolean} Returs true if cart has lines with shipping delivery method
         */
        public static hasLinesForShipping(cartLines: Entities.CartLine[]): boolean {
            return cartLines.some((cartLine: Entities.CartLine) => !StringExtensions.isNullOrWhitespace(cartLine.DeliveryMode) && cartLine.DeliveryMode != ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode);
        }

        /**
         * Gets whether the user must be asked to authorize the shipping amount or not.
         *
         * @return {boolean} Returns whether the user must be asked to authorize the shipping amount or not.
         */
        public static canAuthorizeShippingAmount(cart: Entities.Cart): boolean {

            // how much is not covered by deposit
            var orderAmountNotCoveredByDeposit: number = cart.TotalAmount - cart.RequiredDepositAmount;

            return CustomerOrderHelper.isCustomerOrderCreationOrEdition(cart) // only prompt for customer order creation/edition
                && CustomerOrderHelper.hasLinesForShipping(cart.CartLines)              // only prompt if shipping
                && orderAmountNotCoveredByDeposit > 0;   // only prompt deposit isn't covering all order
        }

        /**
         *  Gets a value indicating whether the deposit was overridden for the cart.
         *  @param {Model.Entitites.Cart} cart the cart to be perform the check against.
         *  @ param {boolean} a value indicating whether the deposit was overridden for the cart.
         */
        public static isDepositOverridden(cart: Entities.Cart): boolean {
            return !ObjectExtensions.isNullOrUndefined(cart.OverriddenDepositAmount);
        }

        /**
         * Calculates the deposit paid by all tender lines.
         *
         * @param {number} Deposit paid 
         */
        public static calculateDepositPaid(cart: Entities.Cart): number {
            var depositPaid: number = (CustomerOrderHelper.isCustomerOrderCreationOrEdition(cart)) ? cart.RequiredDepositAmount - cart.AmountDue : cart.RequiredDepositAmount;
            return depositPaid;
        }

        /**
         * Gets the default quote expiration date.
         *
         * @returns {Date} the default quote expiration date.
         */
        public static getDefaultQuoteExpirationDate(): Date {
            var defaultQuoteExpiryDate: Date = DateExtensions.getDate();
            defaultQuoteExpiryDate.setDate(defaultQuoteExpiryDate.getDate() + ApplicationContext.Instance.channelConfiguration.QuoteExpirationDays);
            DateExtensions.setTimeToLastSecondOfDay(defaultQuoteExpiryDate);

            return defaultQuoteExpiryDate;
        }

        /**
         * Validates whether all cart lines inside cart are valid for customer order operations.
         * Checks executed:
         * 1. The cart lines do not contain gift card transactions
         * 2. The cart lines do not contain return transactions
         *
         * @param {Entities.Cart} cart Cart entity to be validated.
         * @returns {Entities.Error[]} List of errors on all invalid cart lines from cart entity.
         */
        public static validateCartLinesForCustomerOrder(cart: Entities.Cart): Entities.Error[] {

            if (ObjectExtensions.isNullOrUndefined(cart)) {
                RetailLogger.genericError("Null or undefined reference: cart on CustomerOrderHelper.validateCartLinesForCustomerOrder()");
                return [new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)];
            }

            var cartLines: Entities.CartLine[] = cart.CartLines;
            var errors: Entities.Error[] = [];

            // Check each cart line
            if (ArrayExtensions.hasElements(cartLines)) {
                $.each(cartLines, (i: number, cartLine: Entities.CartLine) => {

                    // Cart line is not a gift card
                    if (cartLine.IsGiftCardLine && !cartLine.IsVoided) {
                        errors.push(new Entities.Error(ErrorTypeEnum.ORDERS_CANNOT_INCLUDE_GIFTCARDS));
                        return false; // Skip checking of the remaining lines
                    }

                    // Cart line is not a return
                    if (cartLine.Quantity < 0 && !cartLine.IsVoided) {
                        errors.push(new Entities.Error(ErrorTypeEnum.ORDERS_CANNOT_INCLUDE_RETURNS));
                        return false; // Skip checking of the remaining lines
                    }
                });
            }

            return errors;
        }

        /**
         * Show get quotation expiration date dialog.
         *
         * @param {Date} [originalExpirationDate] The quotation expiration date.
         * @returns {Date} The requested quotation expiration date.
         */
        public static getQuotationExpirationDate(originalExpirationDate?: Date): IAsyncResult<Date> {
            var asyncResult = new AsyncResult<Date>(StringExtensions.EMPTY);

            if (ObjectExtensions.isNullOrUndefined(originalExpirationDate)) {
                originalExpirationDate = CustomerOrderHelper.getDefaultQuoteExpirationDate();
            }

            // show Quote Expiration Date Dialog to get expiration date
            var activityContext: Activities.GetQuotationExpirationDateActivityContext = {
                originalExpirationDate: originalExpirationDate
            };

            var activity = new Activities.GetQuotationExpirationDateActivity(activityContext);
            activity.execute().done(() => {
                if (activity.response) {
                    // set that we have until the last second of the day before the quote expires
                    DateExtensions.setTimeToLastSecondOfDay(activity.response.expirationDate);
                    asyncResult.resolve(activity.response.expirationDate);
                } else {
                    asyncResult.resolve(null);
                }
            });

            return asyncResult;
        }

        /**
         * Sets the quotation expiration date.
         *
         * @param {Entities.Cart} cart The current cart.
         * @param {Date} [requestedExpirationDate] The requested quotation expiration date.
         * @returns {IVoidAsyncResult} The async result.
         */
        public static setQuotationExpirationDate(cart: Entities.Cart, requestedExpirationDate?: Date): IVoidAsyncResult {
            var quoteExpirationDateOperationParameters: Operations.ISetQuotationExpirationDateOperationOptions = {
                cart: cart,
                requestedExpirationDate: requestedExpirationDate
            };

            return Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.SetQuotationExpirationDate, quoteExpirationDateOperationParameters);
        }

        /**
         * Gets a value indicating whether the user should be informed that action is required for deposit override during pickup.
         * 
         * @param {Entities.Cart} The cart object.
         * @returns {boolean} A value indicating whether the user should be informed that action is required for deposit override during pickup.
         */
        public static shouldWarnForDepositOverrideActionOnPickup(cart: Entities.Cart): boolean {
            // prompt user to provide deposit override for pickup if order was created with deposit override and
            // currently no deposit override is applied and there is deposit to be applied
            return CustomerOrderHelper.isCustomerOrderPickup(cart)
                && CustomerOrderHelper.isDepositOverridden(cart)
                && cart.OverriddenDepositAmount == 0
                && cart.AvailableDepositAmount != 0;
        }
    }
}