/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path="../../JQuery.d.ts" />
///<reference path="../../KnockoutJS.d.ts" />
///<reference path="../../Libraries.Proxies.Retail.TypeScript.d.ts" />

import CommerceProxy = Commerce.Proxy;

// Initialize page
$(document).ready(() => {
    // Calling selectUICulture() in order to read the UICulture value from the cookie
    // and pick the corresponding resource translations for the string in Checkout control.
    Contoso.Retail.Ecommerce.Sdk.Controls.ResourcesHandler.selectUICulture();
    $('.msax-Control').each((index, element) => {
        var viewModelName = $(element.firstElementChild).attr("data-model");
        var pathNames = viewModelName.split('.');
        var viewModel = window[pathNames[0]]; // Initialize with first level. Example 'Commerce' 

        // Starting from next element drill down to the method. 
        for (var i = 1; i < pathNames.length; i++) {
            viewModel = viewModel[pathNames[i]];
        }

        ko.applyBindings(new viewModel(element.firstElementChild), element);
    });
});

// Show errors
var msaxError = {
    Show: function (level, message, errorCodes?) {
        console.error(message)
    }
};

/* This module is to simulate the existence of the variables that are being registered from the server side */
module msaxValues {
    export var msax_CartWebApiUrl;
    export var msax_OrgUnitWebApiUrl;
    export var msax_RetailOperationsWebApiUrl;
    export var msax_CustomerWebApiUrl;
    export var msax_ProductWebApiUrl;
    export var msax_SalesOrderWebApiUrl;
    export var msax_OrderConfirmationUrl;
    export var msax_OrderDetailsUrl;
    export var msax_ProductDetailsUrlTemplate;
    export var msax_IsDemoMode;
    export var msax_DemoDataPath;
    export var msax_HasInventoryCheck;
    export var msax_CheckoutUrl;
    export var msax_ShoppingCartUrl;
    export var msax_IsCheckoutCart;
    export var msax_ContinueShoppingUrl;
    export var msax_CartDiscountCodes;
    export var msax_CartLoyaltyReward;
    export var msax_CartDisplayPromotionBanner;
    export var msax_ReviewDisplayPromotionBanner;
    export var msax_OrderCount;
    export var msax_ShowPaging;
}

/* This module is to simulate the existence of Bing maps APIs */
module Microsoft.Maps {
    export var Map;
    export var loadModule;
    export var Search;
    export var Events;
    export var Pushpin;
    export var Infobox;
    export var Point;
}

module Contoso.Retail.Ecommerce.Sdk.Controls {

    export class AjaxProxy {
        private relativeUrl;

        constructor(relativeUrl) {
            this.relativeUrl = relativeUrl;
            $(document).ajaxError(this.ajaxErrorHandler);
        }

        private ajaxErrorHandler(e, xhr, settings) {
            var errorMessage =
                'Url:\n' + settings.url +
                '\n\n' +
                'Response code:\n' + xhr.status +
                '\n\n' +
                'Status Text:\n' + xhr.statusText +
                '\n\n' +
                'Response Text: \n' + xhr.responseText;

            msaxError.Show('error', 'The web service call was unsuccessful.  Details: ' + errorMessage);
        }

        public SubmitRequest = function (webMethod, data, successCallback, errorCallback) {

            // Example: http://www.contoso.com:40002/sites/retailpublishingportal + /_vti_bin/ShoppingCartService.svc/ + GetShoppingCart
            var webServiceUrl = this.relativeUrl + webMethod;

            var requestDigestHeader = (<HTMLInputElement>($(document).find('#__REQUESTDIGEST'))[0]);
            var retailRequestDigestHeader = (<HTMLInputElement>($(document).find('#__RETAILREQUESTDIGEST'))[0]);
            var requestDigestHeaderValue;
            var retailRequestDigestHeaderValue;

            if (Utils.isNullOrUndefined(requestDigestHeader) || Utils.isNullOrUndefined(retailRequestDigestHeader)) {
                requestDigestHeaderValue = null;
                retailRequestDigestHeaderValue = null;
            }
            else {
                requestDigestHeaderValue = requestDigestHeader.value;
                retailRequestDigestHeaderValue = retailRequestDigestHeader.value;
            }

            // Submit the AJAX call using jQuery.
            $.ajax({
                url: webServiceUrl,
                data: JSON.stringify(data),
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    successCallback(data);
                },
                error: function (jqXHR: JQueryXHR) {
                    if (jqXHR.status == 310) {
                        var redirectUrl: string = jqXHR.getResponseHeader("Location");
                        if (Utils.isNullOrWhiteSpace(redirectUrl)) {
                            throw "The redirect url to sign in page should be provided for HTTP status code 310";
                        }
                        else {
                            window.location.replace(redirectUrl);
                        }
                    }
                    errorCallback(jqXHR);
                },
                headers: {
                    "X-RequestDigest": requestDigestHeaderValue,
                    "X-RetailRequestDigest": retailRequestDigestHeaderValue
                }
            });
        };
    }

    export class LoadingOverlay {

        private static pendingCallsCount = 0;
        private static loadingDialog: any = null;
        private static loadingText: any = null;

        public static CreateLoadingDialog(loadingDialog: any, loadingText: any, width: number, height: number) {
            if (Utils.isNullOrUndefined(LoadingOverlay.loadingDialog) && Utils.isNullOrUndefined(LoadingOverlay.loadingText)) {
                LoadingOverlay.loadingDialog = loadingDialog;
                LoadingOverlay.loadingText = loadingText;

                // Creates the loading overlay dialog box.
                LoadingOverlay.loadingDialog.dialog({
                    modal: true,
                    autoOpen: false,
                    draggable: true,
                    resizable: false,
                    closeOnEscape: true,
                    show: { effect: "fadeIn", duration: 500 },
                    hide: { effect: "fadeOut", duration: 500 },
                    open: function (event, ui) {
                        setTimeout(function () {
                            LoadingOverlay.loadingText.text(Resources.String_221); // The service call is taking longer than expected. Please wait for it to respond or refresh the page and try again.
                        }, 60000); // If the service call does not return (does not fail or succeed) for some reason then we update the message on the dialog to warn the user that the service is still in progress..
                    },
                    width: width,
                    height: height,
                    dialogClass: 'msax-Control msax-LoadingOverlay msax-NoTitle'
                });
            }
        }

        public static ShowLoadingDialog(text?: string) {
            // Displays the loading dialog.
            if (Utils.isNullOrWhiteSpace(text)) {
                LoadingOverlay.loadingText.text(Resources.String_176); // Loading ...
            }
            else {
                LoadingOverlay.loadingText.text(text);
            }

            if (LoadingOverlay.pendingCallsCount == 0) {
                LoadingOverlay.loadingDialog.dialog('open');
                $('.ui-widget-overlay').addClass('msax-LoadingOverlay');
            }
            LoadingOverlay.pendingCallsCount = LoadingOverlay.pendingCallsCount + 1;
        }

        public static CloseLoadingDialog() {
            LoadingOverlay.pendingCallsCount = LoadingOverlay.pendingCallsCount - 1;
            if (LoadingOverlay.pendingCallsCount == 0) {
                if (LoadingOverlay.loadingDialog.dialog('isOpen') == true) {
                    // Close the dialog.
                    LoadingOverlay.loadingDialog.dialog('close');
                    $('.ui-widget-overlay').removeClass('msax-LoadingOverlay');
                }
            }
        }
    }

    export class Core {
        // Builds 50 * 50 image markup.
        public static BuildImageMarkup50x50(imageUrl: string, imageAltText: string): string {
            return this.BuildImageMarkup(imageUrl, imageAltText, 50, 50);
        }

        // Builds 180 * 180 image markup.
        public static BuildImageMarkup180x180(imageUrl: string, imageAltText: string): string {
            return this.BuildImageMarkup(imageUrl, imageAltText, 180, 180);
        }

        // Build image markup with given width and height.
        private static BuildImageMarkup(imageUrl: string, imageAltText: string, width: number, height: number): string {
            var imageClassName = "msax-Image";

            if (!Utils.isNullOrWhiteSpace(imageUrl)) {
                var errorScript = Utils.format('onerror=\"this.parentNode.innerHTML=Contoso.Retail.Ecommerce.Sdk.Controls.CartWebApi.GetNoImageMarkup();\"');
                return Utils.format('<img src=\"{0}\" class=\"{1}\" alt=\"{2}\" width=\"{3}\" height=\"{4}\" {5} />', imageUrl, imageClassName, imageAltText, width, height, errorScript);
            }
            else {
                return this.GetNoImageMarkup();
            }
        }

        // Returns markup to be displayed when image url is null or when image url does not exist.
        private static GetNoImageMarkup(): string {
            return Utils.format('<span class=\"msax-NoImageContainer\"></span>');
        }

        public static GetDimensionValuesStringFromDimensions(dimensionValues: CommerceProxy.Entities.ProductDimension[]): string {
            var color: string = null;
            var size: string = null;
            var style: string = null;
            var configuration: string = null;

            if (Utils.hasElements(dimensionValues)) {
                for (var i = 0; i < dimensionValues.length; i++) {
                    var dimension: CommerceProxy.Entities.ProductDimension = dimensionValues[i];
                    if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Color) {
                        color = dimension.DimensionValue.Value;
                    }
                    else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Size) {
                        size = dimension.DimensionValue.Value;
                    }
                    else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Style) {
                        style = dimension.DimensionValue.Value;
                    }
                    else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Configuration) {
                        configuration = dimension.DimensionValue.Value;
                    }
                }
            }

            var dimensionValuesString = Core.GetDimensionValues(color, size, style, configuration);
            return dimensionValuesString;
        }

        public static GetDimensionValues(color: string, size: string, style: string, configuration: string): string {
            var hasColor = !Utils.isNullOrWhiteSpace(color);
            var hasSize = !Utils.isNullOrWhiteSpace(size);
            var hasStyle = !Utils.isNullOrWhiteSpace(style);
            var hasConfiguration = !Utils.isNullOrWhiteSpace(configuration);

            var dimensionValues = null;
            if (hasColor || hasSize || hasStyle || hasConfiguration) {
                dimensionValues = ''
                + (!hasColor ? '' : color)
                + (hasColor && (hasSize || hasStyle || hasConfiguration) ? ', ' : '')
                + (!hasSize ? '' : size)
                + (hasSize && (hasStyle || hasConfiguration) ? ', ' : '')
                + (!hasStyle ? '' : style)
                + (hasStyle && (hasConfiguration) ? ', ' : '')
                + (!hasConfiguration ? '' : configuration)
                + '';
            }

            return dimensionValues;
        }

        public static getDefaultQueryResultSettings(): CommerceProxy.Entities.QueryResultSettings {
            var queryResultSettings = new CommerceProxy.Entities.QueryResultSettingsClass();
            queryResultSettings.Paging = new CommerceProxy.Entities.PagingInfoClass();
            queryResultSettings.Paging.Skip = 0;
            queryResultSettings.Paging.Top = 1000;
            queryResultSettings.Sorting = {};

            return queryResultSettings;
        }

        public static getQueryResultSettings(skip: number, top: number): CommerceProxy.Entities.QueryResultSettings {
            var queryResultSettings = new CommerceProxy.Entities.QueryResultSettingsClass();
            queryResultSettings.Paging = new CommerceProxy.Entities.PagingInfoClass();
            queryResultSettings.Paging.Skip = skip;
            queryResultSettings.Paging.Top = top;
            queryResultSettings.Sorting = {};

            return queryResultSettings;
        }

        public static getOrderSearchCriteria(channelReferenceId: string, salesId: string, receiptId: string, includeDetails: boolean): CommerceProxy.Entities.SalesOrderSearchCriteria {
            var orderSearchCriteria = new CommerceProxy.Entities.SalesOrderSearchCriteriaClass();
            if (!Utils.isNullOrWhiteSpace(channelReferenceId)) {
                orderSearchCriteria.ChannelReferenceId = channelReferenceId;
            }
            else if (!Utils.isNullOrWhiteSpace(salesId)) {
                orderSearchCriteria.SalesId = salesId;
            }
            else {
                orderSearchCriteria.ReceiptId = receiptId;
            }

            orderSearchCriteria.IncludeDetails = includeDetails;
            return orderSearchCriteria;
        }

        public static getOrderNumber(salesOrder: CommerceProxy.Entities.SalesOrder) {
            if (!Utils.isNullOrWhiteSpace(salesOrder.ChannelReferenceId)) {
                return salesOrder.ChannelReferenceId;
            }
            else if (!Utils.isNullOrWhiteSpace(salesOrder.SalesId)) {
                return salesOrder.SalesId;
            }
            else {
                return salesOrder.ReceiptId;
            }
        }

        public static getProductSearchCriteria(productIds: number[]): CommerceProxy.Entities.ProductSearchCriteria {
            var productSearchCriteria = new CommerceProxy.Entities.ProductSearchCriteriaClass();
            productSearchCriteria.Ids = productIds;
            productSearchCriteria.DataLevelValue = 4;
            productSearchCriteria.SkipVariantExpansion = false;

            return productSearchCriteria;
        }

        // Creates a mapping of product ids to their master product ids given a collection of products
        public static getProductIdLookUpMap(products: CommerceProxy.Entities.Product[]): number[] {
            var productIdLookupMap: number[] = [];

            if (products != null) {

                var variants: CommerceProxy.Entities.ProductVariant[] = [];

                // Iterate through the products and populate the product id to master product id mapping.
                for (var i = 0; i < products.length; i++) {
                    var product: CommerceProxy.Entities.Product = products[i];
                    var tempVariants: CommerceProxy.Entities.ProductVariant[] = this.getProductVariants(product);
                    if (tempVariants.length > 0) {
                        variants = variants.concat(tempVariants);
                    }

                    if (productIdLookupMap[product.RecordId] == null) {
                        productIdLookupMap[product.RecordId] = product.RecordId;
                    }
                }

                // Iterate through all the variant products and add missing entries into the product id to master product id mapping.
                if (variants.length > 0) {
                    for (var j = 0; j < variants.length; j++) {
                        var variant: CommerceProxy.Entities.ProductVariant = variants[j];
                        if (productIdLookupMap[variant.DistinctProductVariantId] == null) {
                            productIdLookupMap[variant.DistinctProductVariantId] = variant.MasterProductId;
                        }
                    }
                }
            }

            return productIdLookupMap;
        }

        // Gets a list of all product variants given a product. 
        private static getProductVariants(product: CommerceProxy.Entities.Product): CommerceProxy.Entities.ProductVariant[] {
            var variants: CommerceProxy.Entities.ProductVariant[] = [];

            if (product.IsMasterProduct && product.CompositionInformation != null && product.CompositionInformation.VariantInformation != null) {
                variants = product.CompositionInformation.VariantInformation.Variants;
            }

            return variants;
        }

        // Gets the string type value of the specified extension property.
        public static getExtensionPropertyValue(commerceProperties: CommerceProxy.Entities.CommerceProperty[], propertyName: string): string {
            var commerceProperty: CommerceProxy.Entities.CommerceProperty = null;
            var value: string = null;
            for (var i = 0; i < commerceProperties.length; i++) {
                if (commerceProperties[i].Key == propertyName) {
                    commerceProperty = commerceProperties[i];
                    break;
                }
            }

            if (commerceProperty != null) {
                value = commerceProperty.Value.StringValue;
            }

            return value;
        }

        public static populateProductDetailsForCartLine(line: CommerceProxy.Entities.CartLine, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            Core.populateProductDetailsForLine(line, simpleProductsByIdMap, currencyStringTemplate);
        }

        public static populateProductDetailsForSalesLine(line: CommerceProxy.Entities.SalesLine, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            Core.populateProductDetailsForLine(line, simpleProductsByIdMap, currencyStringTemplate);
        }

        public static populateProductDetailsForLine(line: any, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            // Dynamically setting the additional product details on the cart line so that they can be bound to the UI.
            var simpleProduct: CommerceProxy.Entities.SimpleProduct = simpleProductsByIdMap[line.ProductId];

            if (Utils.isNullOrUndefined(simpleProduct)) {
                line[Constants.ProductNameProperty] = Utils.format("Product info [{0}] unavailable", line.ProductId);
                line[Constants.ProductDescriptionProperty] = "This product is not available in the current store.";
                line[Constants.ProductDimensionProperty] = '';
                line[Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50('', '');;
                line[Constants.ImageMarkup180pxProperty] = Core.BuildImageMarkup180x180('', '');;
                line[Constants.KitComponentsProperty] = '';
                line[Constants.ProductTypeProperty] = '';
                line[Constants.ProductUrlProperty] = '#';
                line[Constants.KitComponentCountProperty] = '';
                line[Constants.KitComponentPriceProperty] = '';
                return;
            }

            line[Constants.ProductNameProperty] = simpleProduct.Name;
            line[Constants.ProductDescriptionProperty] = simpleProduct.Description;

            line[Constants.ProductDimensionProperty] = Core.GetDimensionValuesStringFromDimensions(simpleProduct.Dimensions);

            line[Constants.ProductUrlProperty] = Utils.format(msaxValues.msax_ProductDetailsUrlTemplate, line.ProductId);

            var imageUrl: string = Constants.ProductUrlString + Core.getExtensionPropertyValue(simpleProduct.ExtensionProperties, "PrimaryImageUri");
            imageUrl = (imageUrl != null) ? imageUrl : '';

            var imageAltText: string = Core.getExtensionPropertyValue(line.ExtensionProperties, "PrimaryImageAltText");
            imageAltText = (imageAltText != null) ? imageAltText : '';

            line[Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50(imageUrl, imageAltText);
            line[Constants.ImageMarkup180pxProperty] = Core.BuildImageMarkup180x180(imageUrl, imageAltText);

            line[Constants.ProductTypeProperty] = '';
            line[Constants.KitComponentsProperty] = [];
            line[Constants.KitComponentCountProperty] = 0;
        }

        public static populateKitItemDetailsForCartLine(line: CommerceProxy.Entities.CartLine, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            Core.populateKitItemDetails(line, simpleProductsByIdMap, currencyStringTemplate);
        }

        public static populateKitItemDetailsForSalesLine(line: CommerceProxy.Entities.SalesLine, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            Core.populateKitItemDetails(line, simpleProductsByIdMap, currencyStringTemplate);
        }

        private static populateKitItemDetails(line: any, simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[], currencyStringTemplate: string) {
            var simpleProduct: CommerceProxy.Entities.SimpleProduct = simpleProductsByIdMap[line.ProductId];

            if (Utils.isNullOrUndefined(simpleProduct) || simpleProduct.ProductTypeValue != CommerceProxy.Entities.ProductType.KitVariant || !Utils.hasElements(simpleProduct.Components)) {
                return;
            }

            var kitComponents: CommerceProxy.Entities.ProductComponent[] = simpleProduct.Components;
            for (var j = 0; j < simpleProduct.Components.length; j++) {
                var kitComponent: CommerceProxy.Entities.ProductComponent = kitComponents[j];

                kitComponent[Constants.ProductDimensionProperty] = Core.GetDimensionValuesStringFromDimensions(kitComponent.Dimensions);
                kitComponent[Constants.ProductUrlProperty] = Utils.format(msaxValues.msax_ProductDetailsUrlTemplate, kitComponent.ProductId);
                kitComponent[Constants.ProductNameProperty] = kitComponent.Name;

                var imageUrl: string = Constants.ProductUrlString + Core.getExtensionPropertyValue(kitComponent.ExtensionProperties, "PrimaryImageUri");
                imageUrl = (imageUrl != null) ? imageUrl : '';

                var imageAltText: string = Core.getExtensionPropertyValue(kitComponent.ExtensionProperties, "PrimaryImageAltText");
                imageAltText = (imageAltText != null) ? imageAltText : '';
                kitComponent[Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50(imageUrl, imageAltText);

                kitComponent[Constants.KitComponentPriceProperty] = Core.getKitComponentPriceCurrencyString(kitComponent.AdditionalChargeForComponent, currencyStringTemplate);
            }

            line[Constants.ProductTypeProperty] = CommerceProxy.Entities.ProductType.KitVariant;
            line[Constants.KitComponentCountProperty] = Utils.format(Resources.String_88 /* {0} PRODUCT(S) */, kitComponents.length);
            line[Constants.KitComponentsProperty] = kitComponents;
        }

        private static getKitComponentPriceCurrencyString(amount: number, currencyStringTemplate: string): string {
            var formattedKitComponentPrice: string = Resources.String_208;

            if (amount != 0) {
                if (Utils.isNullOrUndefined(currencyStringTemplate)) {
                    formattedKitComponentPrice = amount.toString();
                }
                else {
                    formattedKitComponentPrice = Utils.format(currencyStringTemplate, Utils.formatNumber(amount));
                }
            }

            return formattedKitComponentPrice;
        }

        public static getSalesStatusString(statusValue: number): string {
            var salesStatus = "";
            if (statusValue == CommerceProxy.Entities.SalesStatus.Unknown) {
                salesStatus = Resources.String_240; /* Processing */
            }
            else {
                salesStatus = CommerceProxy.Entities.SalesStatus[statusValue];
            }

            return salesStatus;
        }

        public static LogEvent(eventName: Function, errors: CommerceProxy.ProxyError[], alternateMessage: string) {
            var logErrorMessage: string = (Utils.hasElements(errors) && !Utils.isNullOrWhiteSpace(errors[0].LocalizedErrorMessage)) ? errors[0].LocalizedErrorMessage : alternateMessage;
            CommerceProxy.RetailLogger.LogEvent(eventName, logErrorMessage);
        }
    }
}