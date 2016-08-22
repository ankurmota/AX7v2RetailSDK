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

    export class DiscountsHelper {
        /**
         * Validates the discount value against employee limits.
         * @param {number} discountValue The discount value.
         * @return {Proxy.Entities.Error} The error, in case the discount value is invalid.
         */
        public static validateMaximumDiscountAmount(discountValue: number): Proxy.Entities.Error {
            var employee: Proxy.Entities.Employee = Session.instance.CurrentEmployee;
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            var error: Proxy.Entities.Error = null;

            if (!ObjectExtensions.isNullOrUndefined(employee)
                && employee.Permissions.MaximumTotalDiscountAmount < discountValue) {

                if (employee.Permissions.MaximumTotalDiscountAmount === 0) {
                    error = new Proxy.Entities.Error(ErrorTypeEnum.PERMISSION_DENIED_TOTAL_AMOUNT_DISCOUNT);
                } else {
                    var maximumDiscountAmount: string = NumberExtensions.formatCurrency(employee.Permissions.MaximumTotalDiscountAmount);
                    error = new Proxy.Entities.Error(ErrorTypeEnum.MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED, false, null, null, maximumDiscountAmount);
                }
            }

            if (error == null && !ObjectExtensions.isNullOrUndefined(cart) && discountValue > cart.SubtotalAmountWithoutTax) {
                error = new Proxy.Entities.Error(ErrorTypeEnum.MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED_SUBTOTAL);
            }

            return error;
        }

        /**
         * Validates the discount value against employee limits.
         * @param {number} discountValue The discount value.
         * @return {Proxy.Entities.Error} The error, in case the discount value is invalid.
         */
        public static validateMaximumDiscountPercentage(discountValue: number): Proxy.Entities.Error {
            var employee: Proxy.Entities.Employee = Session.instance.CurrentEmployee;
            var error: Proxy.Entities.Error = null;

            if (!ObjectExtensions.isNullOrUndefined(employee)
                && employee.Permissions.MaximumTotalDiscountPercentage < discountValue) {

                if (employee.Permissions.MaximumTotalDiscountPercentage === 0) {
                    error = new Proxy.Entities.Error(ErrorTypeEnum.PERMISSION_DENIED_TOTAL_PERCENT_DISCOUNT);
                } else {
                    error = new Proxy.Entities.Error(
                        ErrorTypeEnum.MAXIMUM_TOTAL_DISCOUNT_PERCENT_EXCEEDED, false, null, null, employee.Permissions.MaximumTotalDiscountPercentage);
                }
            }

            return error;
        }

        /**
         * Validates the discount value against employee limits.
         * @param {number} discountValue The discount value.
         * @param {Proxy.Entities.CartLine} cartLine The cart line to add the discount to.
         * @return {Proxy.Entities.Error} The error, in case the discount value is invalid.
         */
        public static validateMaximumLineDiscountAmount(discountValue: number, cartLine: Proxy.Entities.CartLine): Proxy.Entities.Error {
            var employee: Proxy.Entities.Employee = Session.instance.CurrentEmployee;
            var error: Proxy.Entities.Error = null;

            if (!ObjectExtensions.isNullOrUndefined(employee)
                && employee.Permissions.MaximumLineDiscountAmount < discountValue) {

                if (employee.Permissions.MaximumLineDiscountAmount === 0) {
                    error = new Proxy.Entities.Error(ErrorTypeEnum.PERMISSION_DENIED_LINE_AMOUNT_DISCOUNT);
                } else {
                    var maximumDiscountAmount: string = NumberExtensions.formatCurrency(employee.Permissions.MaximumLineDiscountAmount);
                    error = new Proxy.Entities.Error(ErrorTypeEnum.MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED, false, null, null, maximumDiscountAmount);
                }
            }

            if (error == null && !ObjectExtensions.isNullOrUndefined(cartLine) && discountValue > cartLine.ExtendedPrice) {
                error = new Proxy.Entities.Error(ErrorTypeEnum.MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED_PRICE);
            }

            return error;
        }

        /**
         * Validates the discount value against employee limits.
         * @param {number} discountValue The discount value.
         * @param {Proxy.Entities.CartLine} cartLine The cart line to add the discount to.
         * @return {Proxy.Entities.Error} The error, in case the discount value is invalid.
         */
        public static validateMaximumLineDiscountPercentage(discountValue: number, cartLine: Proxy.Entities.CartLine): Proxy.Entities.Error {
            var employee: Proxy.Entities.Employee = Session.instance.CurrentEmployee;
            var error: Proxy.Entities.Error = null;

            if (!ObjectExtensions.isNullOrUndefined(employee)
                && employee.Permissions.MaximumDiscountPercentage < discountValue) {

                if (employee.Permissions.MaximumDiscountPercentage === 0) {
                    error = new Proxy.Entities.Error(ErrorTypeEnum.PERMISSION_DENIED_LINE_PERCENT_DISCOUNT);
                } else {
                    error = new Proxy.Entities.Error(
                        ErrorTypeEnum.MAXIMUM_LINE_DISCOUNT_PERCENT_EXCEEDED, false, null, null, employee.Permissions.MaximumDiscountPercentage);
                }
            }

            return error;
        }

        /**
         * Validates that discount can be applied to the cart.
         * @param {Proxy.Entities.Cart} cart The cart.
         * @return {Proxy.Entities.Error} The error, in case the cart is in invalid type or mode.
         */
        public static validateCanAddDiscounts(cart: Proxy.Entities.Cart): Proxy.Entities.Error {
            if (ObjectExtensions.isNullOrUndefined(cart)) {
                return new Proxy.Entities.Error(ErrorTypeEnum.CART_IS_EMPTY);
            }

            var canApplyDiscounts: boolean = (cart.CartTypeValue === Proxy.Entities.CartType.Shopping)
                || (cart.CartTypeValue === Proxy.Entities.CartType.CustomerOrder
                    && (cart.CustomerOrderModeValue === Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                        || cart.CustomerOrderModeValue === Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit));

            if (!canApplyDiscounts) {
                return new Proxy.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCOUNTISALLOWEDONLYFORCREATIONANDEDITION);
            }

            return null;
        }

        /**
         * Builds discount information for display
         * @param {Proxy.Entities.DiscountLine} discountLine The discount line.
         * @param {number} price The item price.
         * @param {number} quantity The item quantity.
         * @return {string} Returns the discount information string.
         */
        public static getDiscountTypeName(discountLine: Proxy.Entities.DiscountLine, price: number, quantity: number): string {
            var typeName: string = StringExtensions.EMPTY;
            if (ObjectExtensions.isNullOrUndefined(discountLine)) {
                return typeName;
            }

            var percentage: number = discountLine.Percentage;
            if (percentage === 0 && price > 0 && quantity !== 0) {
                percentage = (discountLine.EffectiveAmount / (price * quantity)) * 100;
                percentage = NumberExtensions.roundToNDigits(percentage, 2);
            }

            var formattedAmount: string = NumberExtensions.formatCurrency(discountLine.EffectiveAmount);
            var formattedPercentage: string = NumberExtensions.formatNumber((percentage / 100), NumberExtensions.getNumberOfDecimals(percentage), "P");

            switch (discountLine.DiscountLineTypeValue) {
                case Proxy.Entities.DiscountLineType.PeriodicDiscount:
                    // "{0}: {1} {2} ({3})"
                    typeName = StringExtensions.format(
                        Commerce.ViewModelAdapter.getResourceString("string_5612"),
                        discountLine.OfferName,
                        Commerce.ViewModelAdapter.getResourceString("string_5615"), // Discount
                        formattedAmount,
                        formattedPercentage);
                    break;

                case Proxy.Entities.DiscountLineType.ManualDiscount:
                    {
                        switch (discountLine.ManualDiscountTypeValue) {
                            case Proxy.Entities.ManualDiscountType.LineDiscountAmount:
                            case Proxy.Entities.ManualDiscountType.LineDiscountPercent:
                                // "{0}: {1} ({2})"
                                typeName = StringExtensions.format(
                                    Commerce.ViewModelAdapter.getResourceString("string_5616"),
                                    Commerce.ViewModelAdapter.getResourceString("string_5611"), // Line Discount
                                    formattedAmount,
                                    formattedPercentage);
                                break;
                            case Proxy.Entities.ManualDiscountType.TotalDiscountAmount:
                            case Proxy.Entities.ManualDiscountType.TotalDiscountPercent:
                                // "{0}: {1} ({2})"
                                typeName = StringExtensions.format(
                                    Commerce.ViewModelAdapter.getResourceString("string_5616"),
                                    Commerce.ViewModelAdapter.getResourceString("string_5610"), // Total Discount
                                    formattedAmount,
                                    formattedPercentage);
                                break;
                            default:
                                RetailLogger.coreHelpersInvalidManualDiscountType(discountLine.ManualDiscountTypeValue);
                                break;
                        }
                    }
                case Proxy.Entities.DiscountLineType.CustomerDiscount:
                    {
                        switch (discountLine.CustomerDiscountTypeValue) {
                            case Proxy.Entities.CustomerDiscountType.LineDiscount:
                            case Proxy.Entities.CustomerDiscountType.MultilineDiscount:
                                typeName = StringExtensions.format(
                                    Commerce.ViewModelAdapter.getResourceString("string_5616"),
                                    Commerce.ViewModelAdapter.getResourceString("string_5611"), // Line Discount
                                    formattedAmount,
                                    formattedPercentage);
                                break;
                            case Proxy.Entities.CustomerDiscountType.TotalDiscount:
                                typeName = StringExtensions.format(
                                    Commerce.ViewModelAdapter.getResourceString("string_5616"),
                                    Commerce.ViewModelAdapter.getResourceString("string_5610"), // Total Discount
                                    formattedAmount,
                                    formattedPercentage);
                                break;
                            default:
                                RetailLogger.coreHelpersInvalidCustomerDiscountType(discountLine.CustomerDiscountTypeValue);
                                break;
                        }
                    }
                default:
                    RetailLogger.coreHelpersInvalidDiscountLineType(discountLine.DiscountLineTypeValue);
                    break;
            }

            return typeName;
        }
    }
}