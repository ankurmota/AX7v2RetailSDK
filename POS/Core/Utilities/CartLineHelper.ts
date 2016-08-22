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
///<reference path='../Extensions/StringExtensions.ts'/>
///<reference path='ProductPropertiesHelper.ts'/>

module Commerce {
    "use strict";

    export class CartLineHelper {
        /**
         * Checks whether the cart line is from a receipt
         * @param {Proxy.Entities.CartLine} cartLine The cart line to check
         * @return {boolean} True if the cart line is from a receipt, false if the cart line is not set or not from a receipt
         */
        public static isFromAReceipt(cartLine: Proxy.Entities.CartLine): boolean {
            var isFromAReceipt: boolean = !ObjectExtensions.isNullOrUndefined(cartLine)
                && !ObjectExtensions.isNullOrUndefined(cartLine.ReturnTransactionId)
                && !StringExtensions.isEmptyOrWhitespace(cartLine.ReturnTransactionId);

            return isFromAReceipt;
        }

        /**
         * Gets the added or updated (if aggregation is allowed) cart lines.
         * @param {Proxy.Entities.CartLine[]} originalLines The original collection of cart lines.
         * @param {Proxy.Entities.CartLine[]} newLines The new collection of cart lines.
         * @return {Proxy.Entities.CartLine[]} The collection containing only the modified cart lines.
         */
        public static getModifiedCartLines(originalLines: Proxy.Entities.CartLine[], newLines: Proxy.Entities.CartLine[]): Proxy.Entities.CartLine[] {
            var deviceConfiguration: Proxy.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
            var allowAggregation: boolean = !ObjectExtensions.isNullOrUndefined(deviceConfiguration) && deviceConfiguration.AllowItemsAggregation;

            var quantitiesByLineId: { [lineId: string]: number } = Object.create(null);
            originalLines.forEach((c: Commerce.Model.Entities.CartLine) => quantitiesByLineId[c.LineId] = c.Quantity);

            // if the line is aggregate, quantity must have changed, if it is not aggregate, line should not be present
            // Similar logic is used in LineDisplayHelper::displayLineItems. If you make a change here please check to see if it is needed there as well.
            var addedCartLines: Commerce.Model.Entities.CartLine[] = newLines.filter((c: Commerce.Model.Entities.CartLine) =>
                (allowAggregation && quantitiesByLineId[c.LineId] !== c.Quantity)
                || (!allowAggregation && quantitiesByLineId[c.LineId] === undefined));

            return addedCartLines;
        }

        /**
         * Get a product name from cart line product.
         * @param {Proxy.Entities.CartLine} The cart line product to get the product name.
         * @return {string} The cart line product name.
         */
        public static getProductName(cartLine: Proxy.Entities.CartLine): string {
            if (cartLine.IsGiftCardLine) {
                return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_1245"), cartLine.Comment);
            } else {
                var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
                return ObjectExtensions.isNullOrUndefined(product) ? StringExtensions.EMPTY : product.Name;
            }
        }

        /**
         * Validate if a CartLine object is a product (not a gift card)
         * @param {Proxy.Entities.CartLine} cartLine The cart line object to be validated.
         * @returns {boolean} True if cart line is a product, false otherwise.
         */
        public static IsProduct(cartLine: Proxy.Entities.CartLine): boolean {
            if (!ObjectExtensions.isNullOrUndefined(cartLine) && !cartLine.IsGiftCardLine) {
                return true;
            }

            return false;
        }
    }
}