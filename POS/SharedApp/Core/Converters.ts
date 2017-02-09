/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Commerce.ViewModels.d.ts'/>
///<reference path='BindingHandlers.ts'/>

module Commerce {
    "use strict";

    export class Formatters {

        /**
         * Returns formatted address type text.
         *
         * @param {Proxy.Entities.AddressType} addressType The address type.
         * @return {string} Formatted address type text.
         */
        static AddressTypeTextFormatter(addressType: Proxy.Entities.AddressType): string {
            var addressTypeText: string = Proxy.Entities.AddressTypeHelper.getDescription(addressType);

            return addressTypeText;
        }

        /**
         * Returns formatted address header text.
         *
         * @param {string} Header string for the address card.
         * @return {string} Formatted address type text.
         */
        static AddressHeaderFormatter(header: string): string {
            if (StringExtensions.isNullOrWhitespace(header)) {
                return ViewModelAdapter.getResourceString("string_1316");
            }

            return header;
        }

        /**
         * Returns formatted date without time.
         *
         * @param {Date} value The date.
         * @return {string} Formatted date without time.
         */
        static DateWithoutTime(value: Date): string {
            if (ObjectExtensions.isNullOrUndefined(value)) {
                return StringExtensions.EMPTY;
            }

            var formatter = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.SHORT_DATE);

            return formatter.format(value);
        }

        /**
         * Returns formatted short time.
         *
         * @param {Date} value The date.
         * @return {string} Formatted short time.
         */
        static ShortTime(value: Date): string {
            if (ObjectExtensions.isNullOrUndefined(value)) {
                return StringExtensions.EMPTY;
            }

            var formatter = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.SHORT_TIME);
            return formatter.format(value);
        }

        /**
         * Returns formatted price.
         *
         * @param {number} value The size.
         * @return {string} Formatted price.
         */
        static PriceFormatter(value: any): string {
            if (ObjectExtensions.isNullOrUndefined(value)) {
                return StringExtensions.EMPTY;
            } else if (ObjectExtensions.isObject(value)) {
                var currencyAmount: Proxy.Entities.CurrencyAmount = <Proxy.Entities.CurrencyAmount> value;
                return NumberExtensions.formatCurrency(currencyAmount.RoundedConvertedAmount, currencyAmount.CurrencyCode);
            } else if (ObjectExtensions.isString(value)) {
                value = NumberExtensions.parseNumber(value);
                if (isNaN(value)) {
                    return StringExtensions.EMPTY;
                }
            }

            return NumberExtensions.formatCurrency(value);
        }

        /**
         * Returns a string that can uniquely identify an order.
         *
         * @param {Proxy.Entities.SalesOrder} A salesOrder
         * @return {string} A unique identifier
         */
        static OrderIdFormatter(salesOrder: Proxy.Entities.SalesOrder): string {
            var genericOrderId: string = "";

            if (!ObjectExtensions.isNullOrUndefined(salesOrder)) {
                genericOrderId = salesOrder.ReceiptId;

                if (StringExtensions.isNullOrWhitespace(salesOrder.ReceiptId)) {
                    genericOrderId = salesOrder.ChannelReferenceId;

                    if (StringExtensions.isNullOrWhitespace(salesOrder.ChannelReferenceId)) {
                        genericOrderId = salesOrder.SalesId;
                    }
                }
            }

            return genericOrderId;
        }

        /**
         * Returns formatted image url.
         *
         * @param {Proxy.Entities.Customer} customer The customer.
         * @param {boolean} isLarge The size.
         * @return {string} Formatted image url.
         */
        static CustomerImage(customer: Proxy.Entities.Customer, isLarge?: boolean): string {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                isLarge = isLarge ? isLarge : false;
                if (ArrayExtensions.hasElements(customer.Images)
                    && !StringExtensions.isNullOrWhitespace(customer.Images[0].Uri)) {
                    var url = Commerce.Formatters.ImageUrlFormatter(customer.Images[0].Uri);
                    return url;
                } else {
                    return isLarge ? "/Assets/defaultLarge.png" : "/Assets/defaultSmall.png";
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(customer.OfflineImage);
            }
        }

        /**
         * Returns formatted image url.
         *
         * @param {Proxy.Entities.Product} product The product.
         * @param {boolean} isLarge The size.
         * @return {string} Formatted image url.
         */
        static ProductImage(product: Proxy.Entities.Product, isLarge?: boolean): string {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                isLarge = isLarge ? isLarge : false;
                if (!ObjectExtensions.isNullOrUndefined(product.Image)
                    && ArrayExtensions.hasElements(product.Image.Items)
                    && !StringExtensions.isNullOrWhitespace(product.Image.Items[0].Url)) {
                    var url = Commerce.Formatters.ImageUrlFormatter(product.Image.Items[0].Url);
                    return url;
                } else {
                    return isLarge ? Commerce.DefaultImages.ProductLarge : Commerce.DefaultImages.ProductSmall;
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(product.OfflineImage);
            }
        }

        /**
         * Returns formatted image base64 binary string.
         *
         * @param {string} base64 image binary.
         * @return {string} Formatted image base64 binary string.
         */
        static ImageBinaryFormatter(source: string): string {
            if (!ObjectExtensions.isNullOrUndefined(source) && !StringExtensions.isNullOrWhitespace(source)) {
                return "data:image/jpeg;base64," + source;
            }
            else {
                return "/Assets/defaultLarge.png";
            }
        }

        /**
         * Returns formatted image URL.
         * @param {string} url The URL.
         * @param {string} defaultImagepath The default image path.
         * @return {string} Formatted image URL.
         */
        static ImageUrlFormatter(url: string, defaultImagepath?: string): string {
            if (!StringExtensions.isNullOrWhitespace(url)) {

                var imageUrl = ApplicationContext.Instance.channelRichMediaBaseURL;

                if (Commerce.Config.isDemoMode) {
                    return Formatters.DemoModeImageUrlFormatter(url);
                }

                if (Session.instance.connectionStatus == ConnectionStatusType.Online && Core.RegularExpressionValidations.validateUrl(url)) {
                    return url;
                } else {
                    return UrlHelper.formatBaseUrl(imageUrl) + url;
                }
            } else {
                if (!StringExtensions.isNullOrWhitespace(defaultImagepath)) {
                    return defaultImagepath;
                } else {
                    return Commerce.DefaultImages.ProductSmall;
                }
            }
        }

        /**
         * Returns formatted image based on connection status.
         *
         * @param {string} source The image source.
         * @param {string} defaultImagepath The default image path.
         * @return {string} Formatted image base64 binary string/url.
         */
        static ImageFormatter(source: string, defaultImagepath?: string): string {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                return this.ImageUrlFormatter(source, defaultImagepath);
            }
            else {
                return this.ImageBinaryFormatter(source);
            }
        }

        /**
         * Formats the product dimension value for the provided cart line and dimension type.
         * @param {Proxy.Entities.CartLine} cartLine The cart line.
         * @param {Proxy.Entities.ProductDimensionType} dimensionType The dimension type for which to get the value.
         * @return {string} The dimension value.
         */
        static productDimensionValueFormatter(cartLine: Proxy.Entities.CartLine, dimensionType: Proxy.Entities.ProductDimensionType): string {
            if (cartLine.IsGiftCardLine) {
                return StringExtensions.EMPTY;
            }

            var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            return ObjectExtensions.isNullOrUndefined(product) ? StringExtensions.EMPTY : SimpleProductHelper.getDimensionValue(product, dimensionType);
        }

        /**
         * Returns formatted cart line property.
         *
         * @param {Proxy.Entities.CartLine} cartLine The cart line.
         * @param {string} propertyName The property name.
         * @return {string} Formatted cart type.
         */
        static CartLineProperty(cartLine: Proxy.Entities.CartLine, propertyName: string): string {
            var returnValue: string = StringExtensions.EMPTY;

            if (!ObjectExtensions.isNullOrUndefined(cartLine) && !StringExtensions.isNullOrWhitespace(propertyName)) {
                switch (propertyName) {
                    case "RequestedDeliveryDate":
                        var formatter = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.SHORT_DATE);
                        returnValue = formatter.format(cartLine.RequestedDeliveryDate);
                        break;
                    case "ShippingAddressName":
                        var addressName: string = StringExtensions.EMPTY;

                        if (!ObjectExtensions.isNullOrUndefined(cartLine.ShippingAddress)) {
                            addressName = cartLine.ShippingAddress.Name;
                        }

                        returnValue = addressName;
                        break;
                    case "DeliveryDescription":
                        var deliveryDescription: string = StringExtensions.EMPTY;

                        var deliveryOption: Proxy.Entities.DeliveryOption = ApplicationContext.Instance.deliveryOptionsMap.getItem(cartLine.DeliveryMode);

                        if (!ObjectExtensions.isNullOrUndefined(deliveryOption)) {
                            var isPickupInStore: boolean = cartLine.DeliveryMode === ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode;
                            var store: Proxy.Entities.OrgUnit = ApplicationContext.Instance.availableStores.getItem(cartLine.FulfillmentStoreId);

                            deliveryDescription = isPickupInStore && !StringExtensions.isNullOrWhitespace(store.OrgUnitName) ?
                            StringExtensions.format(ViewModelAdapter.getResourceString("string_29224"), store.OrgUnitName) :
                            deliveryOption.Description;
                        }

                        returnValue = deliveryDescription;
                        break;
                    default:
                        throw "CartLineProperty formatter not implemented for property '" + propertyName + "'";
                }
            } else {
                RetailLogger.coreFormattersCartLineWrongInputParameters(propertyName, JSON.stringify(cartLine));
            }

            return returnValue;

        }

        /**
         * Returns formatted cart type.
         *
         * @param {Proxy.Entities.Cart} cart The cart.
         * @return {string} Formatted cart type.
         */
        static CartTypeName(cart: Proxy.Entities.Cart): string {
            var resourceId: string;

            if (ObjectExtensions.isNullOrUndefined(cart)) {
                return StringExtensions.EMPTY;
            }

            switch (cart.CartTypeValue) {
                case Proxy.Entities.CartType.CustomerOrder:
                    switch (cart.CustomerOrderModeValue) {
                        case Proxy.Entities.CustomerOrderMode.Cancellation:
                            resourceId = "string_4360";
                            break;

                        case Proxy.Entities.CustomerOrderMode.Pickup:
                            resourceId = "string_4358";
                            break;

                        case Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit:
                            resourceId = "string_4359";
                            break;

                        case Proxy.Entities.CustomerOrderMode.Return:
                            resourceId = "string_4357";
                            break;

                        default:
                        case Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit:
                            resourceId = "string_4330";
                            break;
                    }
                    break;

                case Model.Entities.CartType.AccountDeposit:
                    resourceId = "string_4384"; // CUSTOMER ACCOUNT DEPOSIT
                    break;

                default:
                    resourceId = "string_108"; // NEW SALE
                    break;
            }

            return Commerce.ViewModelAdapter.getResourceString(resourceId);
        }

        /**
         * Returns transalated category name.
         *
         * @param {Proxy.Entities.Category} Category Object
         * @return {string} Transalate category name.
         */
        static CategoryNameTranslator(category: Proxy.Entities.Category): string {
            var translatedCategoryName: string = "";
            if (!ObjectExtensions.isNullOrUndefined(category) && ArrayExtensions.hasElements(category.NameTranslations)) {
                var categoryNameTranslation: Proxy.Entities.TextValueTranslation[] = category.NameTranslations.filter((value: Proxy.Entities.TextValueTranslation) => {
                    return value.Language == ApplicationContext.Instance.deviceConfiguration.CultureName;
                });
                if (categoryNameTranslation.length != 0) {
                    translatedCategoryName = categoryNameTranslation[0].Text;
                } else {
                    translatedCategoryName = category.Name;
                }
            } else {
                translatedCategoryName = category.Name;
            }

            return translatedCategoryName;
        }


        /**
         * Returns formatted tender type.
         *
         * @param {Proxy.Entities.CartTenderLineTenderType} tenderLine tender cart.
         * @return {string} Formatted tender type.
         */
        static TenderLineTypeNameFormatter(tenderLine: Proxy.Entities.CartTenderLineTenderType): string {
            if (ObjectExtensions.isNullOrUndefined(tenderLine) ||
                (!tenderLine.IsHistorical && ObjectExtensions.isNullOrUndefined(tenderLine.TenderType))) {
                return StringExtensions.EMPTY;
            }

            return tenderLine.IsHistorical ? Commerce.ViewModelAdapter.getResourceString("string_4355") : tenderLine.TenderType.Name;
        }

        /**
         * Returns formatted amount in tendered currency.
         *
         * @param {Proxy.Entities.CartTenderLine | Proxy.Entities.TenderLine} tenderedCurrency The tender line.
         * @return {string} Formatted tender type.
         */
        static AmountInTenderedCurrencyFormatter(tenderedCurrency: Proxy.Entities.TenderLine): string;
        static AmountInTenderedCurrencyFormatter(tenderedCurrency: Proxy.Entities.CartTenderLine): string {
            if (ObjectExtensions.isNullOrUndefined(tenderedCurrency) || ObjectExtensions.isNullOrUndefined(tenderedCurrency.Currency)) {
                return StringExtensions.EMPTY;
            }

            return NumberExtensions.formatCurrency(tenderedCurrency.AmountInTenderedCurrency, tenderedCurrency.Currency);
        }

        /**
         * Returns formatted tender name.
         *
         * @param {Proxy.Entities.TenderLine} tenderLine tender cart.
         * @return {string} Formatted tender type.
         */
        static TenderLineNameFormatter(tenderLine: Proxy.Entities.TenderLine): string {

            if (ObjectExtensions.isNullOrUndefined(tenderLine) ||
                (!tenderLine.IsHistorical && ObjectExtensions.isNullOrUndefined(tenderLine.TenderTypeId))) {
                return StringExtensions.EMPTY;
            }

            var historicalTenderTypeName: string = Commerce.ViewModelAdapter.getResourceString("string_4355");
            var tenderType: Proxy.Entities.TenderType = ApplicationContext.Instance.tenderTypesMap.getTenderByTypeId(tenderLine.TenderTypeId);
            return ObjectExtensions.isNullOrUndefined(tenderType) ? historicalTenderTypeName : tenderType.Name;
        }

        /**
         * Returns formatted transfer order type.
         *
         * @param {Proxy.Entities.PurchaseTransferOrderType} value transfer order type.
         * @return {string} Formatted transfer order type.
         */
        static PurchaseTransferOrderEnumFormatter(value: Proxy.Entities.PurchaseTransferOrderType): string {
            var stringValue: string;
            switch (value) {
                case Proxy.Entities.PurchaseTransferOrderType.PurchaseOrder:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_3862");
                    break;
                case Proxy.Entities.PurchaseTransferOrderType.TransferIn:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_3863");
                    break;
                case Proxy.Entities.PurchaseTransferOrderType.TransferOut:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_3864");
                    break;
                case Proxy.Entities.PurchaseTransferOrderType.PickingList:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_3867");
                    break;
                default:
                    stringValue = StringExtensions.EMPTY;
            }

            return stringValue;
        }

        /**
         * Returns formatted cart line quantity respecting the unit of measure decimal precision.
         * @param {Proxy.Entities.CartLine} cartLine The cart line that will have its quantity formatted.
         * @return {string} Formatted cart line quantity respecting the unit of measure decimal precision.
         */
        static CartLineQuantityFormat(cartLine: Proxy.Entities.CartLine): string {
            if (!ObjectExtensions.isNullOrUndefined(cartLine)) {
                return UnitOfMeasureHelper.roundToDisplay(cartLine.Quantity, cartLine.UnitOfMeasureSymbol);
            }

            return NumberExtensions.formatNumber(0, NumberExtensions.getDecimalPrecision());
        }

        /**
         * Returns formatted unit of measure value.
         *
         * @param {Proxy.Entities.CartLine} cartLine cart line.
         * @return {string} Formatted unit of measure value.
         */
        static CartLineUnitOfMeasureFormat(cartLine: Proxy.Entities.CartLine): string {
            if (!ObjectExtensions.isNullOrUndefined(cartLine)) {
                return Commerce.UnitOfMeasureHelper.getDescriptionForSymbol(cartLine.UnitOfMeasureSymbol);
            }
            return null;
        }

        static CartLineCustomField(cartLine: Proxy.Entities.CartLine): string { /*TODO:AM*/
            if (!ObjectExtensions.isNullOrUndefined(cartLine)) {
                var customTotal = cartLine.Price * cartLine.Quantity;
                return NumberExtensions.formatCurrency(customTotal);
            }
            return "0";
        }

        static CartLineCustomDiscount(cartLine: Proxy.Entities.CartLine): string { /*TODO:AM DEMO4*/
            if (!ObjectExtensions.isNullOrUndefined(cartLine)) {
                if (cartLine.Quantity > 0 && cartLine.ItemId == "0003") {
                    var customDiscount = 52.62;
                    return NumberExtensions.formatCurrency(customDiscount);
                }
                
            }
            return "0";
        }

        /**
         * Returns formatted variant name.
         *
         * @param {Proxy.Entities.CartLine} cartLine cart line.
         * @return {string} Formatted variant name.
         */
        static VariantNameFormatter(cartLine: Proxy.Entities.CartLine): string {
            if (cartLine.IsGiftCardLine) {
                return cartLine.Comment;
            }

            var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            return ObjectExtensions.isNullOrUndefined(product) ? StringExtensions.EMPTY : SimpleProductHelper.getVariantDescription(product);
        }

        /**
         * Returns formatted delivery description.
         *
         * @param {Proxy.Entities.CartLine} cartLine cart line.
         * @return {string} Formatted delivery description.
         */
        static DeliveryDescriptionFormatter(cartLine: Proxy.Entities.CartLine): string {
            return Commerce.Formatters.CartLineProperty(cartLine, "DeliveryDescription");
        }

        /**
         * Returns formatted shipping address name.
         *
         * @param {Proxy.Entities.CartLine} cartLine cart line.
         * @return {string} Formatted shipping address name.
         */
        static ShippingAddressNameFormatter(cartLine: Proxy.Entities.CartLine): string {
            return Commerce.Formatters.CartLineProperty(cartLine, "ShippingAddressName");
        }

        /**
         * Returns html formatted of address on Cart View page with html escape.
         *
         * @param {Proxy.Entities.CartLine} cartLine CartLine entity that contains ShippingAddress property.
         * @return {string} Html formatted from address entity.
         */
        static CartLineAddressFormatterWithHtmlEscape(cartLine: Proxy.Entities.CartLine): string {
            return Commerce.Formatters.AddressFormatterWithHtmlEscape(cartLine.ShippingAddress, "span", StringExtensions.EMPTY, true);
        }

        /**
         * Returns html formatted of common address with html escape.
         *
         * @param {Proxy.Entities.Address} address The address entity to be formatted.
         * @param {number} maxLines The maximum lines desired.
         * @return {string} Html formatted from address entity.
         */
        static CommonAddressFormatterWithHtmlEscape(address: Proxy.Entities.Address, maxLines: number = 0): string {
            return Commerce.Formatters.AddressFormatterWithHtmlEscape(address, "h4", "ellipsis", false, maxLines);
        }

        /**
         * Returns html formatted of address with html escape.
         *
         * @param {Proxy.Entities.Address} address The address entity to be formatted.
         * @param {string} htmlTag The tag being used for html formatter.
         * @param {string} cssClass The css class being used for html formatter.
         * @param {boolean} newLine True if newline html tag needed, false otherwise.
         * @param {number} maxLines The maximum lines desired.
         * @return {string} Html formatted from address entity.
         */
        static AddressFormatterWithHtmlEscape(address: Proxy.Entities.Address, htmlTag: string, cssClass: string, newLine: boolean = false, maxLines: number = 0): string {
            if (ObjectExtensions.isNullOrUndefined(address)) {
                return StringExtensions.EMPTY;
            }

            var addressString = StringExtensions.EMPTY;
            var addressLines: string[] = AddressHelper.getFormattedAddress(address);
            var newLineTag: string = newLine ? "<br />" : StringExtensions.EMPTY;

            if (StringExtensions.isNullOrWhitespace(htmlTag)) {
                htmlTag = "span";
            }

            if (StringExtensions.isNullOrWhitespace(cssClass)) {
                cssClass = StringExtensions.EMPTY;
            }

            if (maxLines === 0) {
                maxLines = Number.MAX_VALUE;
            }

            for (var i = 0; i < addressLines.length && i < maxLines; i++) {
                addressString += StringExtensions.format(
                    "<{0} class='{1}'>{2}</{0}>{3}",
                    EscapingHelper.escapeHtmlAttribute(htmlTag),
                    EscapingHelper.escapeHtmlAttribute(cssClass),
                    EscapingHelper.escapeHtml(addressLines[i]),
                    newLineTag);
            }

            return addressString;
        }

        /**
          * Returns formatted requested delivery date with html escape.
          *
          * @param {Proxy.Entities.CartLine} cartLine cart line.
          * @return {string} Formatted requested delivery date.
          */
        static RequestedDeliveryDateFormatterWithHtmlEscape(cartLine: Proxy.Entities.CartLine): string {
            var requestedDeliveryDate = Commerce.Formatters.CartLineProperty(cartLine, "RequestedDeliveryDate");
            var formattedString = Proxy.Entities.SalesOrderWrapper.getOrderStatusString(cartLine.SalesStatusValue);
            if (!Commerce.StringExtensions.isNullOrWhitespace(requestedDeliveryDate)) {
                formattedString = Commerce.StringExtensions.format("{0}<br />{1}", EscapingHelper.escapeHtml(formattedString), EscapingHelper.escapeHtml(requestedDeliveryDate));
            }

            return formattedString;
        }

        static QuantityFromStoreInventoryFormatter(inventoryAvailabilities: Proxy.Entities.ItemAvailability[]): number {
            var inventoryAvailability: Proxy.Entities.ItemAvailability = ArrayExtensions.firstOrUndefined(inventoryAvailabilities);
            var quantity = (inventoryAvailability && inventoryAvailability.AvailableQuantity) ? inventoryAvailability.AvailableQuantity : 0;

            return quantity;
        }

        static UnitOfMeasureFromStoreInventoryFormatter(inventoryAvailabilities: Proxy.Entities.ItemAvailability[]): string {
            var inventoryAvailability: Proxy.Entities.ItemAvailability = ArrayExtensions.firstOrUndefined(inventoryAvailabilities);
            var unitOfMeasure = (inventoryAvailability && inventoryAvailability.UnitOfMeasure) ? inventoryAvailability.UnitOfMeasure : StringExtensions.EMPTY;

            return unitOfMeasure;
        }

        /**
         * Returns the description for the tax override entity.
         * @param {Proxy.Entities.TaxOverride} taxOverride The tax override entity.
         * @return {string} The description.
         */
        static taxOverrideToDescriptionFormatter(taxOverride: Proxy.Entities.TaxOverride): string {
            return taxOverride.AdditionalDescription || taxOverride.Code;
        }

        /**
         * Returns the URL formatted for Demo Mode.
         * @param {string} url The unformatted URL.
         * @return {string} The URL formatted for Demo Mode.
         */
        private static DemoModeImageUrlFormatter(url: string): string {
            var imageUrl: string = "DemoMode/Images/";
            url = url.replace("%5C", "/").replace("jpg", "png").replace("Images/Products", "Products");
            var combinedUrl = imageUrl + url;
            return combinedUrl.replace("//", "/");
        }
    }

    WinJS.Namespace.define("Commerce.Core.Converter", {
        AddressTypeFormatter: WinJS.Binding.converter((addressType: Proxy.Entities.AddressType) =>
            Commerce.Formatters.AddressTypeTextFormatter(addressType)),

        AddressHeaderFormatter: WinJS.Binding.converter((header: string) =>
            Commerce.Formatters.AddressHeaderFormatter(header)),

        TenderLineTypeNameFormatter: WinJS.Binding.converter((tenderLine: Proxy.Entities.CartTenderLineTenderType) => Commerce.Formatters.TenderLineTypeNameFormatter(tenderLine)),
        AmountInTenderedCurrencyFormatter: WinJS.Binding.converter((tenderedCurrency: { AmountInTenderedCurrency: number; Currency: string }) => Commerce.Formatters.AmountInTenderedCurrencyFormatter(tenderedCurrency)),

        TenderLineNameFormatter: WinJS.Binding.converter((tenderLine: Proxy.Entities.TenderLine) => Commerce.Formatters.TenderLineNameFormatter(tenderLine)),

        ProductNameFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine): string => {
            if (cartLine.IsGiftCardLine) {
                return Commerce.ViewModelAdapter.getResourceString("string_5152"); // 'Gift card'
            }

            var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            return ObjectExtensions.isNullOrUndefined(product) ? StringExtensions.EMPTY : product.Name;
        }),

        CartLineQuantityFormat: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine) => Commerce.Formatters.CartLineQuantityFormat(cartLine)),

        SizeNameFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine): string => {
            return Commerce.Formatters.productDimensionValueFormatter(cartLine, Proxy.Entities.ProductDimensionType.Size);
        }),

        ColorNameFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine): string => {
            return Commerce.Formatters.productDimensionValueFormatter(cartLine, Proxy.Entities.ProductDimensionType.Color);
        }),

        StyleNameFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine): string => {
            return Commerce.Formatters.productDimensionValueFormatter(cartLine, Proxy.Entities.ProductDimensionType.Style);
        }),

        ConfigurationNameFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine): string => {
            return Commerce.Formatters.productDimensionValueFormatter(cartLine, Proxy.Entities.ProductDimensionType.Configuration);
        }),

        CartCommentFormatter: WinJS.Binding.converter((cartLine: Proxy.Entities.CartLine) => {

            if (cartLine.IsGiftCardLine) {
                //If cartline is gift card, no need to display comment since it's already displayed on variant field.
                return "";
            } else {
                return cartLine.Comment;
            }
        }),

        // This is the formatter used to choose ReceiptId or SalesId for recent purchase scenario.
        RecentPurchaseIdFormatter: WinJS.Binding.converter((purchaseHistory: Proxy.Entities.PurchaseHistory): string => {
            var result: string = StringExtensions.EMPTY;
            if (!ObjectExtensions.isNullOrUndefined(purchaseHistory)) {
                if (!StringExtensions.isNullOrWhitespace(purchaseHistory.ReceiptId)) {
                    result = purchaseHistory.ReceiptId;
                } else {
                    result = purchaseHistory.SalesId;
                }
            }

            return result;
        }),

        /**
         * Gets the text to show for payment information on a payment line when the payment is made
         * by a non-store currency
         *
         * @param {Proxy.Entities.CartTenderLineTenderType} cartTenderLineTenderType Tender line of the cart
         * @return {string} The text to show for non-store currency payment information on a payment line
         */
        PaymentCurrencyLineText: WinJS.Binding.converter((cartTenderLineTenderType: Proxy.Entities.CartTenderLineTenderType) => {
            var paymentCurrencyLineText: string = StringExtensions.EMPTY;

            if (!ObjectExtensions.isNullOrUndefined(cartTenderLineTenderType)) {
                var currency: string = cartTenderLineTenderType.Currency;
                var foreignCurrencyAmount: number = cartTenderLineTenderType.AmountInTenderedCurrency;
                var exchangeRate: number = cartTenderLineTenderType.ExchangeRate;
                if (!ObjectExtensions.isNullOrUndefined(currency) && !ObjectExtensions.isNullOrUndefined(foreignCurrencyAmount) && !ObjectExtensions.isNullOrUndefined(exchangeRate)) {
                    var amount: string = NumberExtensions.formatCurrency(foreignCurrencyAmount, currency);
                    return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4342"), amount, currency, exchangeRate.toString());
                }
            }

            return paymentCurrencyLineText;
        }),

        /**
         * Gets the display value to show/hide the text for payment information on a payment line
         * when the payment is made by a non-store currency
         *
         * @param {Proxy.Entities.CartTenderLineTenderType} cartTenderLineTenderType Tender line of the cart
         * @return {string} The text "inline-block" to show the text, "none" otherwise
         */
        IsPaymentCurrencyLineTextVisible: WinJS.Binding.converter((cartTenderLineTenderType: Proxy.Entities.CartTenderLineTenderType) => {
            var isVisible: boolean = false;

            if (!ObjectExtensions.isNullOrUndefined(cartTenderLineTenderType)) {
                var currency: string = cartTenderLineTenderType.Currency;
                var amountInTenderedCurrency: number = cartTenderLineTenderType.AmountInTenderedCurrency;
                var exchangeRate: number = cartTenderLineTenderType.ExchangeRate;
                if (!ObjectExtensions.isNullOrUndefined(currency) && !ObjectExtensions.isNullOrUndefined(amountInTenderedCurrency) && !ObjectExtensions.isNullOrUndefined(exchangeRate) && !ObjectExtensions.isNullOrUndefined(cartTenderLineTenderType.TenderTypeId)) {
                    var storeCurrency = Commerce.ApplicationContext.Instance.storeInformation.Currency;
                    isVisible = storeCurrency !== currency;
                }
            }

            return isVisible ? "inline-block" : "none";
        }),

        DecimalFormatter: WinJS.Binding.converter((value) => {
            if (ObjectExtensions.isObject(value)) {
                var currencyAmount: Proxy.Entities.CurrencyAmount = <Proxy.Entities.CurrencyAmount> value;
                return NumberExtensions.formatCurrency(currencyAmount.ConvertedAmount, currencyAmount.CurrencyCode);
            } else if (ObjectExtensions.isString(value)) {
                value = Number(value);
                if (isNaN(value)) {
                    return "";
                }
            }

            return NumberExtensions.formatNumber(value, NumberExtensions.getDecimalPrecision());
        }),

        /*
         * Formats denominations for display
         * A denomination display is in the form:
         * X for a whole number
         * X.YY for a decimal number where YY is the number of decimal places supported for the currency
         *
         * @param {Proxy.Entities.CashDeclaration} value Cash declaration for the denomination
         * @return {string} The denomination value formatted for display
         */
        DenominationFormatter: WinJS.Binding.converter((value) => {
            var currency: string;
            var amount: number;

            if (ObjectExtensions.isObject(value)) {
                var cashDeclaration: Proxy.Entities.CashDeclaration = <Proxy.Entities.CashDeclaration> value;
                currency = cashDeclaration.Currency;
                amount = cashDeclaration.Amount;
            } else if (ObjectExtensions.isString(value)) {
                amount = Number(value);
            }

            return NumberExtensions.formatNumber(amount, NumberExtensions.getDecimalPrecision(currency));
        }),

        PriceFormatter: WinJS.Binding.converter((value) => {
            return Commerce.Formatters.PriceFormatter(value);
        }),

        ExtendedPriceWithoutDiscountFormatter: WinJS.Binding.converter((value: Proxy.Entities.CartLine) => {
            var price = value.Price || 0;
            var quantity = value.Quantity || 0;

            return NumberExtensions.formatCurrency(price * quantity);
        }),

        TransactionTypeFormatter: WinJS.Binding.converter((value) => {
            var transactionType: number = Number(value);

            if (isNaN(transactionType)) {
                return "";
            }

            switch (transactionType) {
                case Proxy.Entities.TransactionType.CustomerOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4515"); // Customer order
                case Proxy.Entities.TransactionType.IncomeExpense:
                    return Commerce.ViewModelAdapter.getResourceString("string_4516"); // Income expense
                case Proxy.Entities.TransactionType.PendingSalesOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4518"); // Pending sales order
                case Proxy.Entities.TransactionType.Sales:
                    return Commerce.ViewModelAdapter.getResourceString("string_4519"); // Sales
                case Proxy.Entities.TransactionType.BankDrop:
                    return Commerce.ViewModelAdapter.getResourceString("string_4572"); // Bank drop
                case Proxy.Entities.TransactionType.SafeDrop:
                    return Commerce.ViewModelAdapter.getResourceString("string_4573"); // Safe drop
                case Proxy.Entities.TransactionType.Payment:
                    return Commerce.ViewModelAdapter.getResourceString("string_4574"); // Payment
                case Proxy.Entities.TransactionType.SalesOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4575"); // Sales orders
                case Proxy.Entities.TransactionType.SalesInvoice:
                    return Commerce.ViewModelAdapter.getResourceString("string_4576"); // Sales invoices
                case Proxy.Entities.TransactionType.TenderDeclaration:
                    return Commerce.ViewModelAdapter.getResourceString("string_4577"); // Declare tender
                default:
                    return '';
            }
        }),

        GroupHeaderFormatter: WinJS.Binding.converter((value) => {
            return Commerce.Formatters.CategoryNameTranslator(value) + "   >";
        }),

        CategoryNameFormatter: WinJS.Binding.converter((value) => {
            return Commerce.Formatters.CategoryNameTranslator(value);
        }),

        QuantityFormatter: WinJS.Binding.converter((value: number) => {
            return NumberExtensions.formatNumber(ObjectExtensions.isNumber(value) ? value : 0, NumberExtensions.getDecimalPrecision());
        }),

        BooleanFormatter: WinJS.Binding.converter((value) => {
            if (value) {
                return Commerce.ViewModelAdapter.getResourceString("string_77"); // Yes
            } else {
                return Commerce.ViewModelAdapter.getResourceString("string_78"); // No
            }
        }),

        ShouldDisplay: WinJS.Binding.converter((value) => {
            return value ? "inline-block" : "none";
        }),

        AbsoluteImageUrl: WinJS.Binding.converter((url: string) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!StringExtensions.isNullOrWhitespace(url)) {
                    return Formatters.ImageUrlFormatter(url);
                } else {
                    return url;
                }
            } else {
                return Commerce.Formatters.ImageBinaryFormatter(url);
            }

        }),

        LargeProductImage: WinJS.Binding.converter((product: Proxy.Entities.Product) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(product)
                    && !ObjectExtensions.isNullOrUndefined(product.Image)
                    && ArrayExtensions.hasElements(product.Image.Items)
                    && !StringExtensions.isNullOrWhitespace(product.Image.Items[0].Url)) {
                    return Commerce.Formatters.ImageUrlFormatter(product.Image.Items[0].Url);
                }
                else {
                    return Commerce.DefaultImages.ProductLarge;
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(product.OfflineImage);
            }

        }),

        SmallProductImage: WinJS.Binding.converter((product: Proxy.Entities.Product) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(product)
                    && !ObjectExtensions.isNullOrUndefined(product.Image)
                    && ArrayExtensions.hasElements(product.Image.Items)
                    && !StringExtensions.isNullOrWhitespace(product.Image.Items[0].Url)) {
                    return Commerce.Formatters.ImageUrlFormatter(product.Image.Items[0].Url);
                }
                else {
                    return Commerce.DefaultImages.ProductSmall;
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(product.OfflineImage);
            }

        }),

        LargeRecentPurchaseImage: WinJS.Binding.converter((imageUrl: string) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(imageUrl)) {
                    return Commerce.Formatters.ImageUrlFormatter(imageUrl);
                }
                else {
                    return Commerce.DefaultImages.ProductLarge;
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(imageUrl);
            }
        }),

        SmallRecentPurchaseImage: WinJS.Binding.converter((imageUrl: string) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(imageUrl)) {
                    return Commerce.Formatters.ImageUrlFormatter(imageUrl);
                }
                else {
                    return Commerce.DefaultImages.ProductSmall;
                }
            }
            else {
                return Commerce.Formatters.ImageBinaryFormatter(imageUrl);
            }
        }),

        CategoryImage: WinJS.Binding.converter((category: { Images?: Proxy.Entities.MediaLocation[], OfflineImage?: string, DefaultImage?: string }) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(category)
                    && ArrayExtensions.hasElements(category.Images) 
                    && !StringExtensions.isNullOrWhitespace(category.Images[0].Uri)) {
                    if (category.Images[0].Uri === Commerce.DefaultImages.AllProducts) {
                        return Commerce.DefaultImages.AllProducts;
                    } else {
                        return Commerce.Formatters.ImageUrlFormatter(category.Images[0].Uri);
                    }
                }
                else {
                    return Commerce.DefaultImages.ProductLarge;
                }
            }
            else {
                // adding the following check for the sake of Catalog
                // since Catalog and Category share the same converter
                // however, we will revisit this code since we will change
                // the category according to the change in CRT
                if (ObjectExtensions.isNullOrUndefined(category.OfflineImage)) {
                    return Commerce.Formatters.ImageBinaryFormatter(category.DefaultImage);
                }
                return Commerce.Formatters.ImageBinaryFormatter(category.OfflineImage);
            }
        }),

        LargeCustomerImage: WinJS.Binding.converter((customer: Proxy.Entities.Customer) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(customer) &&
                    ArrayExtensions.hasElements(customer.Images) &&
                    !StringExtensions.isNullOrWhitespace(customer.Images[0].Uri)) {
                    return Commerce.Formatters.ImageUrlFormatter(customer.Images[0].Uri);
                }
                else {
                    return Commerce.DefaultImages.CustomerLarge;
                }
            }
            else {
                if (!ObjectExtensions.isNullOrUndefined(customer.OfflineImage)) {
                    return Commerce.Formatters.ImageBinaryFormatter(customer.OfflineImage);
                }
            }
        }),

        /**
        * Retrives image URL for customer image
        *
        * @param {Proxy.Entities.Customer} customer - customer object to retrieve the image url from
        * @return the customer image url if available, otherwise returns the default customer image placeholder (small version)
        */
        SmallCustomerImage: WinJS.Binding.converter((customer: Proxy.Entities.Customer) => {
            if (Commerce.Session.instance.connectionStatus == ConnectionStatusType.Online) {
                if (!ObjectExtensions.isNullOrUndefined(customer) &&
                    ArrayExtensions.hasElements(customer.Images) &&
                    !StringExtensions.isNullOrWhitespace(customer.Images[0].Uri)) {
                    return Commerce.Formatters.ImageUrlFormatter(customer.Images[0].Uri);
                } else {
                    return Commerce.DefaultImages.CustomerSmall;
                }
            }
            else {
                if (!ObjectExtensions.isNullOrUndefined(customer.OfflineImage)) {
                    return Commerce.Formatters.ImageBinaryFormatter(customer.OfflineImage);
                }
            }
        }),

        UnitOfMeasureFromStoreInventory: WinJS.Binding.converter((inventoryAvailabilities: Proxy.Entities.ItemAvailability[]): string => {
            return Commerce.Formatters.UnitOfMeasureFromStoreInventoryFormatter(inventoryAvailabilities);
        }),

        QuantityFromStoreInventory: WinJS.Binding.converter((inventoryAvailabilities: Proxy.Entities.ItemAvailability[]): number => {
            return Commerce.Formatters.QuantityFromStoreInventoryFormatter(inventoryAvailabilities);
        }),

        FullAddressFromCustomerAddresses: WinJS.Binding.converter((customerAddresses: Proxy.Entities.Address[]): string => {
            var customerAddress: Proxy.Entities.Address = ArrayExtensions.firstOrUndefined(customerAddresses);
            return (customerAddress && customerAddress.FullAddress) ? customerAddress.FullAddress : StringExtensions.EMPTY;
        }),

        TotalCollection: WinJS.Binding.converter((value) => {
            var numVal = (value && value.length) ? value.length : 0;
            return numVal.toLocaleString(Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName);
        }),

        Distance: WinJS.Binding.converter((value: number) => {
            return Math.round(value * 10) / 10;
        }),

        DateWithoutTime: WinJS.Binding.converter((value: Date): string => Commerce.Formatters.DateWithoutTime(value)),

        OrderIdFormatter: WinJS.Binding.converter((value: Proxy.Entities.SalesOrder): string => Commerce.Formatters.OrderIdFormatter(value)),

        ShortDateAndTime: WinJS.Binding.converter((value: Date) => {
            var formatter = Commerce.Host.instance.globalization.getDateTimeFormatter(Commerce.Host.Globalization.DateTimeFormat.DATE_TIME);
            return formatter.format(value);
        }),

        ShortTime: WinJS.Binding.converter((value: Date) => Commerce.Formatters.ShortTime(value)),

        PurchaseTransferOrderEnumFormatter: WinJS.Binding.converter((value: Proxy.Entities.PurchaseTransferOrderType): string => {
            return Commerce.Formatters.PurchaseTransferOrderEnumFormatter(value);
        }),

        shiftStatusEnumFormatter: WinJS.Binding.converter(function (value: Proxy.Entities.ShiftStatus): string {
            var stringValue: string;
            switch (value) {
                case Proxy.Entities.ShiftStatus.BlindClosed:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_4136");
                    break;
                case Proxy.Entities.ShiftStatus.Closed:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_4137");
                    break;
                case Proxy.Entities.ShiftStatus.None:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_4138");
                    break;
                case Proxy.Entities.ShiftStatus.Open:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_4139");
                    break;
                case Proxy.Entities.ShiftStatus.Suspended:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_4140");
                    break;
                default:
                    stringValue = "";
            }

            return stringValue;
        }),

        cardTenderTypeValueConverter: WinJS.Binding.converter(function (value: Proxy.Entities.LoyaltyCardTenderType): string {
            var stringValue: string;
            switch (value) {
                case Proxy.Entities.LoyaltyCardTenderType.AsCardTender:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_276"); // Card tender
                    break;
                case Proxy.Entities.LoyaltyCardTenderType.AsContactTender:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_277"); // Contact tender
                    break;
                case Proxy.Entities.LoyaltyCardTenderType.NoTender:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_278"); // No tender
                    break;
                case Proxy.Entities.LoyaltyCardTenderType.Blocked:
                    stringValue = Commerce.ViewModelAdapter.getResourceString("string_279"); // Blocked
                    break;
                default:
                    stringValue = "";
            }

            return stringValue;
        }),

        StockCountVariantFormatter: WinJS.Binding.converter((stockCountLine: Proxy.Entities.StockCountLine) => {
            var variantStringValues: string = StringExtensions.EMPTY;
            var variantValues: string[] = [stockCountLine.colorTranslation, stockCountLine.configurationTranslation, stockCountLine.sizeTranslation, stockCountLine.styleTranslation];

            for (var i = 0; i < variantValues.length; i++) {
                if (!StringExtensions.isNullOrWhitespace(variantValues[i])) {
                    if (variantStringValues == "") {
                        variantStringValues = variantValues[i];
                    } else {
                        variantStringValues += ViewModelAdapter.getResourceString("string_2408") + variantValues[i];
                    }
                }
            }

            return variantStringValues;
        }),

        PickingAndReceivingVariantFormatter: WinJS.Binding.converter((pickingAndReceivingLine: Proxy.Entities.PickingAndReceivingOrderLine) => {
            var variantStringValues: string = StringExtensions.EMPTY;
            var variantValues: string[] = [pickingAndReceivingLine.colorTranslation, pickingAndReceivingLine.configurationTranslation, pickingAndReceivingLine.sizeTranslation, pickingAndReceivingLine.styleTranslation];

            for (var i = 0; i < variantValues.length; i++) {
                if (!StringExtensions.isNullOrWhitespace(variantValues[i])) {
                    if (variantStringValues == StringExtensions.EMPTY) {
                        variantStringValues = variantValues[i];
                    } else {
                        variantStringValues += ViewModelAdapter.getResourceString("string_2408") + variantValues[i];
                    }
                }
            }

            return variantStringValues;
        }),

        IncomeExpenseAccountTypeFormatter: WinJS.Binding.converter((value: number) => {
            var stringValue: string = "";

            if (value == Proxy.Entities.IncomeExpenseAccountType.Income) {
                stringValue = Commerce.ViewModelAdapter.getResourceString("string_4132");
            }
            else {
                stringValue = Commerce.ViewModelAdapter.getResourceString("string_4133");
            }

            return stringValue;
        }),

        WishListTypeFormatter: WinJS.Binding.converter((value: number) => {
            var stringValue: string = "";

            if (value == 1) {
                stringValue = Commerce.ViewModelAdapter.getResourceString("string_257");
            }

            return stringValue;
        }),

        RewardPointsFormatter: WinJS.Binding.converter((value: Proxy.Entities.LoyaltyRewardPoint[]): string => {
            var totalActivePoints: number = 0;
            value.forEach((rewardLine: Proxy.Entities.LoyaltyRewardPoint): void => {
                totalActivePoints += rewardLine.ActivePoints;
            });

            return NumberExtensions.formatNumber(totalActivePoints, 0 /* decimalPrecision */);
        }),

        // checks whether the value passed is null, and if so returns an empty string to prevent "null" from being displayed.
        textContentFormatter: WinJS.Binding.converter((value: string) => {
            var stringValue: string = "";
            if (ObjectExtensions.isNullOrUndefined(value)) {
                return stringValue;
            } else {
                return value;
            }
        }),

        // Common address formatter with html escape.
        commonAddressFormatterWithHtmlEscape: WinJS.Binding.converter((address: Proxy.Entities.Address) => {
            return Commerce.Formatters.CommonAddressFormatterWithHtmlEscape(address);
        }),

        threeLineAddressFormatterWithHtmlEscape: WinJS.Binding.converter((address: Proxy.Entities.Address) => {
            return Commerce.Formatters.CommonAddressFormatterWithHtmlEscape(address, 3);
        }),

        storeDistanceFormatter: WinJS.Binding.converter((storeLocation: Proxy.Entities.OrgUnitLocation) => {
            var distanceMeasurement: string = Commerce.ViewModelAdapter.getResourceString("string_2527"); // miles
            var distance: number = NumberExtensions.roundToNDigits(storeLocation.Distance, 1);

            return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_2529"), distance, distanceMeasurement);
        }),

/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
        DayOfWeek: WinJS.Binding.converter(function (value: number): string {
            // TODO: The strings for the days should come from the resx file, but this is just a sample
            var day: string;
            switch (value) {
                case 1:
                    day = "Monday";
                    break;
                case 2:
                    day = "Tuesday";
                    break;
                case 3:
                    day = "Wednesday";
                    break;
                case 4:
                    day = "Thursday";
                    break;
                case 5:
                    day = "Friday";
                    break;
                case 6:
                    day = "Saturday";
                    break;
                case 7:
                    day = "Sunday";
                    break;
                default:
                    day = "";
            }

            return day;
        }),

        TimeOfDayFromSeconds: WinJS.Binding.converter(function (value: number): string {
            var hours = Math.floor(value / 3600);
            var minutes = Math.floor(value % 3600 / 60);
            return StringExtensions.format("{0}:{1}", hours.toString(), StringExtensions.padLeft(minutes.toString(), "0", 2));
        }),
   END SDKSAMPLE_STOREHOURS (do not remove this) */

    });
}