/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Core.d.ts'/>

module Commerce {
    "use strict";

    export class CartHelper {

        /**
         * Checks whether the cart is the specified cart type
         * @param {Commerce.Model.Entities.Cart} cart The cart
         * @param {Commerce.CartType} cartTypeValue The possible cart type
         * @return {boolean} True if the cart is the type specified by cartTypeValue, false otherwise
         */
        public static isCartType(cart: Commerce.Model.Entities.Cart, cartTypeValue: Commerce.Model.Entities.CartType): boolean {
            if (!ObjectExtensions.isNullOrUndefined(cart) && !ObjectExtensions.isNullOrUndefined(cartTypeValue)) {
                if (ObjectExtensions.isNullOrUndefined(cart.CartTypeValue)) {
                    return Commerce.Model.Entities.CartType.None === cartTypeValue;
                }
                return cart.CartTypeValue === cartTypeValue;
            }

            return false;
        }

        /**
         * Returns non voided cart lines.
         * @param {T[]} cartLines The array of cart lines.
         * @return {T[]} The array of cart lines that are not voided.
         */
        public static GetNonVoidedCartLines<T extends Model.Entities.CartLine>(cartLines: T[]): T[] {
            return ArrayExtensions.where(cartLines, (cartLine: T) => !cartLine.IsVoided);
        }

        /**
         * Checks whether the customer is on the cart
         * @param {Commerce.Model.Entities.Cart} cart The cart
         * @param {string} customerId The customer id/account number
         * @return {boolean} True if the customer is on the cart, false otherwise
         */
        public static isCustomerOnCart(cart: Commerce.Model.Entities.Cart, customerId: string): boolean {
            return !ObjectExtensions.isNullOrUndefined(cart)
                && !StringExtensions.isNullOrWhitespace(cart.CustomerId)
                && !StringExtensions.isNullOrWhitespace(customerId)
                && (cart.CustomerId === customerId);
        }

        /**
         * Check that all cart lines selected
         * @param {Model.Entities.Cart} cart The current cart to be checked.
         * @param {Model.Entities.CartLine[])} selectedCartLines: selected cart lines
         * @return {boolean} true if all cart lines selected, otherwise false
         */
        public static areAllCartLinesSelected(cart: Model.Entities.Cart, selectedCartLines: Model.Entities.CartLine[]): boolean {
            if (ObjectExtensions.isNullOrUndefined(cart) || !ArrayExtensions.hasElements(selectedCartLines)) {
                return false;
            }

            var nonVoidedCartLines: Model.Entities.CartLine[] = CartHelper.GetNonVoidedCartLines(selectedCartLines);

            return ArrayExtensions.hasElements(selectedCartLines)
                && CartHelper.GetNonVoidedCartLines(cart.CartLines).length === nonVoidedCartLines.length;
        }

        /**
         * Returns all cart lines with line identifiers that match one of those contained in the provided lineIds.
         * @param {Model.Entities.Cart} cart The cart from which to get the cart lines.
         * @param {string[]} lineIds The line identifiers of the cart lines to get.
         * @return {Model.Entities.CartLine[]} The cart lines whose identifiers matched those that were provided.
         */
        public static getCartLinesByLineIds(cart: Model.Entities.Cart, lineIds: string[]): Model.Entities.CartLine[] {
            if (ObjectExtensions.isNullOrUndefined(cart) || !ArrayExtensions.hasElements(lineIds)) {
                return [];
            }

            return cart.CartLines.filter((cartLine: Model.Entities.CartLine): boolean => {
                return ArrayExtensions.hasElement(lineIds, cartLine.LineId);
            });
        }

        /**
         * Returns all tender lines with line identifiers that match one of those contained in the provided tender line identifiers.
         * @param {Proxy.Entities.Cart} cart The cart from which to get the cart lines.
         * @param {string[]} tenderLineIds The tender line identifiers of the tender lines to get.
         * @return {Proxy.Entities.TenderLine[]} The tender lines whose identifiers matched those that were provided.
         */
        public static getTenderLineByTenderLineIds(cart: Proxy.Entities.Cart, tenderLineIds: string[]): Proxy.Entities.TenderLine[] {
            if (ObjectExtensions.isNullOrUndefined(cart) || !ArrayExtensions.hasElements(tenderLineIds)) {
                return [];
            }

            return cart.TenderLines.filter((tenderLine: Proxy.Entities.TenderLine): boolean => {
                return ArrayExtensions.hasElement(tenderLineIds, tenderLine.TenderLineId);
            });
        }

        /**
         *  Gets the amount due from the cart.
         * @return {number} The amount due on the cart.
         */
        public static cartAmountDue(): number {
            if (Session.instance.cart) {
                return Session.instance.cart.AmountDue;
            } else {
                return Number.NaN;
            }
        }

        /**
         *  Gets the estimated shipping amount from the cart.
         * @return {number} The estimated shipping amount on the cart.
         */
        public static cartEstimatedShippingAmount(): number {
            if (Session.instance.cart) {
                return Session.instance.cart.EstimatedShippingAmount;
            } else {
                return Number.NaN;
            }
        }

        /**
         * Gets the most recently added tender line from the cart.
         * @param {Proxy.Entities.Cart} cart The cart.
         * @return {Proxy.Entities.TenderLine} The most recently added tender line.
         */
        public static getLastTenderLine(cart: Proxy.Entities.Cart): Proxy.Entities.TenderLine {
            var tenderLines: Proxy.Entities.TenderLine[] = cart.TenderLines;
            return tenderLines.length > 0 ? tenderLines[tenderLines.length - 1] : null;
        }

        /**
         * Checks whether the tender line is for a credit card or debit card
         *
         * @param {tenderLine} tenderLine The tender line to check
         * @return True if a credit card or debit card, false otherwise
         */
        public static isCreditOrDebitCard(tenderLine: Commerce.Model.Entities.TenderLine): boolean {
            var isCreditOrDebitCard: boolean = false;
            if (tenderLine) {
                var tenderType: Proxy.Entities.TenderType
                    = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCard);
                isCreditOrDebitCard = ((tenderLine.TenderTypeId === tenderType.TenderTypeId)
                    && (StringExtensions.isNullOrWhitespace(tenderLine.GiftCardId))
                    && (StringExtensions.isNullOrWhitespace(tenderLine.LoyaltyCardId)));
            }

            return isCreditOrDebitCard;
        }

        /**
         * Gets the peripheral type for a payment.
         *
         * @param {tenderLine} tenderLine The tender line.
         * @return peripheral type for payment.
         */
        public static getPeripheralPaymentType(tenderLine: Commerce.Model.Entities.TenderLine): Proxy.Entities.PeripheralPaymentType {
            if (!CartHelper.isCreditOrDebitCard(tenderLine)) {
                return Proxy.Entities.PeripheralPaymentType.RetailServer;
            } else if (!Peripherals.HardwareStation.HardwareStationContext.instance.isActive()) {
                return Proxy.Entities.PeripheralPaymentType.CardPaymentAccept;
            } else if (Peripherals.instance.paymentTerminal && Peripherals.instance.paymentTerminal.isActive) {
                return Proxy.Entities.PeripheralPaymentType.PaymentTerminal;
            } else {
                return Proxy.Entities.PeripheralPaymentType.CardPaymentController;
            }
        }

        //POSHackF
        public static SC_numberOfProperties: number = 9;
        /**
        0 Comment
        1 info has been collected
        2 Ad Source
        3 Zip Code
        4 Sales Person primary
        5 Sales Person secondary (optional)
        6 Tax status
        7 Order status - header
        8 Kit Price - header
        */
        public static SC_commentDelimiter: string = "@";
        //public static SC_commentDelimiter2: string = "[]?|*";
        //public static SC_commentDelimiter3: string = "[===]";
        public static SC_cartLineNumOfProperties: number = 4;
        /**
        0 Comment
        1 order status
        2 Is kit line
        3 Kit Price - line
        */
        public static SC_cartLineCommentDelimiter: string = "@";

        public static SC_prepareCartProperty(comment: string): string {
            if (!comment) {
                comment = "";
            }
            var cartProperties = new Array<string>();
            if (CartHelper.SC_ExistCartproperty(comment)) {
                return comment;
            }
            cartProperties.push(comment.toString());
            for (var i = 1; i < CartHelper.SC_numberOfProperties; i++) {
                cartProperties.push("");

            }
            comment = cartProperties.join(CartHelper.SC_commentDelimiter);
            return comment;
        }
        public static SC_getCartProperties(comment: string): string[] {
            comment = CartHelper.SC_prepareCartProperty(comment);
            return comment.split(CartHelper.SC_commentDelimiter);
        }

        public static SC_CartPropertiesToComment(cartProperties: string[]): string {
            return cartProperties.join(CartHelper.SC_commentDelimiter);
        }


        public static SC_ExistCartproperty(comment: string): boolean {
            if (!comment) {
                return false;
            }
            var cartProperty = comment.split(CartHelper.SC_commentDelimiter);
            if (cartProperty.length >= CartHelper.SC_numberOfProperties) {
                return true;
            } else {
                return false;
            }
        }

        public static SC_prepareCartLineProperty(comment: string): string {
            if (!comment) {
                comment = "";
            }
            var cartLineProperties = new Array<string>();
            if (CartHelper.SC_ExistCartLineproperty(comment)) {
                return comment;
            }
            cartLineProperties.push(comment.toString());
            for (var i = 1; i < CartHelper.SC_cartLineNumOfProperties; i++) {
                cartLineProperties.push("");
            }

            comment = cartLineProperties.join(CartHelper.SC_cartLineCommentDelimiter);
            return comment;
        }

        public static SC_getCartLineProperties(comment: string): string[] {
            comment = CartHelper.SC_prepareCartLineProperty(comment);
            return comment.split(CartHelper.SC_cartLineCommentDelimiter);

        }

        public static SC_CartLinePropertiesToComment(cartLineProperties: string[]): string {
            return cartLineProperties.join(CartHelper.SC_cartLineCommentDelimiter);
        }

        public static SC_ExistCartLineproperty(comment: string): boolean {
            if (!comment) {
                return false;
            }
            var cartLineProperty = comment.split(CartHelper.SC_cartLineCommentDelimiter);
            if (cartLineProperty.length >= CartHelper.SC_cartLineNumOfProperties) {
                return true;
            } else {
                return false;
            }
        }
        //POShackF END
    }
}