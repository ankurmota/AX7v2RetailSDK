/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Diagnostics.TypeScriptCore.d.ts'/>

import TsLogging = Microsoft.Dynamics.Diagnostics.TypeScriptCore;

module Commerce.Proxy {

    /**
     * Attaches the logging sink to the LoggerBase.
     *
     * @method attachLoggingSink
     * @param sink {TsLogging.ILoggingSink} Sink to attach to Retail Logger.
     */
    export function attachLoggingSink(sink: TsLogging.ILoggingSink) {
        Microsoft.Dynamics.Diagnostics.TypeScriptCore.LoggerBase.addLoggingSink(sink);
    }

    /**
     * Class represents proxy events.
     * Event Code Range: 44000 - 44999.
     */
    export class RetailLogger {

        public static LogEvent(eventName: Function, error?: string): void {
            error ? eventName(error) : eventName();
        }

        // Core - Event Range: 44000 - 44099.
        public static genericError(message: string): void {
            TsLogging.LoggerBase.writeEvent("GenericEvent", 44000, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "{0}");
        }

        public static genericWarning(message: string): void {
            TsLogging.LoggerBase.writeEvent("GenericWarning", 44001, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Warning, [], "", "", "{0}");
        }

        public static genericInfo(message: string): void {
            TsLogging.LoggerBase.writeEvent("GenericInfo", 44002, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Informational, [], "", "", "{0}");
        }

        public static modelManagersRetailServerRequestStarted(requestId: string, requestUrl: string): void {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestStarted", 44004, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' started.");
        }

        public static modelManagersRetailServerRequestError(requestId: string, requestUrl: string, error: string): void {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestError", 44005, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' failed.  Error: {2}.");
        }

        public static modelManagersRetailServerRequestFinished(requestId: string, requestUrl: string): void {
            TsLogging.LoggerBase.writeEvent("ModelManagersRetailServerRequestFinished", 44006, TsLogging.EventChannel.Debug, 1, TsLogging.EventLevel.Informational, [], "", "", "The Retail Server Request with request id '{0}' and request url '{1}' succeeded.");
        }

        public static initEntitySetInvalidError(entitySetId: string): void {
            TsLogging.LoggerBase.writeEvent("InitEntitySetInvalidError", 44007, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with invalid InitEntitySet id '{0}' was reported.");
        }

        public static initEntitySetMultipleTimesError(entitySetId: string): void {
            TsLogging.LoggerBase.writeEvent("InitEntitySetMultipleTimesError", 44008, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with InitEntitySet ID '{0}' was reported multiple times.");
        }

        public static initEntitySetNoMethodNumberError(): void {
            TsLogging.LoggerBase.writeEvent("InitEntitySetNoMethodNumberError", 44009, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The value InitEntitySet.All does not represent the number of methods that can be run");
        }

        public static initPaymentEntitySetInvalidError(entitySetId: string): void {
            TsLogging.LoggerBase.writeEvent("InitPaymentEntitySetInvalidError", 44010, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with invalid InitPaymentEntitySet id '{0}' was reported.");
        }

        public static initPaymentEntitySetMultipleTimesError(entitySetId: string): void {
            TsLogging.LoggerBase.writeEvent("InitPaymentEntitySetMultipleTimesError", 44011, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "A method with InitPaymentEntitySet ID '{0}' was reported multiple times.");
        }

        public static initPaymentEntitySetNoMethodNumberError(): void {
            TsLogging.LoggerBase.writeEvent("InitPaymentEntitySetNoMethodNumberError", 44012, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "The value InitPaymentEntitySet.All does not represent the number of methods that can be run");
        }

        // Shopping Cart Service - Event Range: 44100 - 44199.
        public static shoppingCartServiceGetShoppingCartStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceGetShoppingCartStarted", 44100, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get shopping cart started.");
        }

        public static shoppingCartServiceGetShoppingCartError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceGetShoppingCartError", 44101, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get shopping cart failed with error {0}.");
        }

        public static shoppingCartServiceGetShoppingCartFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceGetShoppingCartFinished", 44102, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get shopping cart finished.");
        }

        public static shoppingCartServiceRemoveFromCartStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceRemoveFromCartStarted", 44104, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Removing item from cart started.");
        }

        public static shoppingCartServiceRemoveFromCartError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceRemoveFromCartError", 44105, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Removing item from cart failed with error {0}.");
        }

        public static shoppingCartServiceRemoveFromCartFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartServiceRemoveFromCartFinished", 44106, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Removing item from cart finished.");
        }

        public static shoppingCartApplyPromotionCodeStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartApplyPromotionCodeStarted", 44108, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Applying promotion code started.");
        }

        public static shoppingCartApplyPromotionCodeError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartApplyPromotionCodeError", 44109, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Applying promotion code failed with error {0}.");
        }

        public static shoppingCartApplyPromotionCodeFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartApplyPromotionCodeFinished", 44110, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Applying promotion code finished.");
        }

        public static shoppingCartUpdateQuantityStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateQuantityStarted", 44112, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Updating quantity started.");
        }

        public static shoppingCartUpdateQuantityError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateQuantityError", 44113, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Updating quantity failed with error {0}.");
        }

        public static shoppingCartUpdateQuantityFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateQuantityFinished", 44114, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Updating quantity finished.");
        }

        public static shoppingCartRemovePromotionCodeStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartRemovePromotionCodeStarted", 44116, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Removing promotion code started.");
        }

        public static shoppingCartRemovePromotionCodeError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartRemovePromotionCodeError", 44117, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Removing promotion code failed with error {0}.");
        }

        public static shoppingCartRemovePromotionCodeFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartRemovePromotionCodeFinished", 44118, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Removing promotion code finished.");
        }

        public static shoppingCartUpdateLoyaltyCardIdStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateLoyaltyCardIdStarted", 44120, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Updating loyalty card id started.");
        }

        public static shoppingCartUpdateLoyaltyCardIdError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateLoyaltyCardIdError", 44121, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Updating loyalty card id failed with error {0}.");
        }

        public static shoppingCartUpdateLoyaltyCardIdFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartUpdateLoyaltyCardIdFinished", 44122, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Updating loyalty card id finished.");
        }

        public static shoppingCartCommenceCheckoutStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartCommenceCheckoutStarted", 44124, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Commence checkout started.");
        }

        public static shoppingCartCommenceCheckoutError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartCommenceCheckoutError", 44125, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Commence checkout failed with error {0}.");
        }

        public static shoppingCartCommenceCheckoutFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartCommenceCheckoutFinished", 44126, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Commence checkout finished.");
        }

        public static shoppingCartGetPromotionsStarted(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartGetPromotionsStarted", 44128, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting promotions started.");
        }

        public static shoppingCartGetPromotionsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartGetPromotionsError", 44129, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting promotions failed with error {0}.");
        }

        public static shoppingCartGetPromotionsFinished(): void {
            TsLogging.LoggerBase.writeEvent("ShoppingCartGetPromotionsFinished", 44130, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting promotions finished.");
        }

        public static getSimpleProductsByIdStarted(): void {
            TsLogging.LoggerBase.writeEvent("getSimpleProductsByIdStarted", 44132, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting simple products by ids started.");
        }

        public static getSimpleProductsByIdError(error: string): void {
            TsLogging.LoggerBase.writeEvent("getSimpleProductsByIdError", 44133, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting simple products by ids failed with error {0}.");
        }

        public static getSimpleProductsByIdFinished(): void {
            TsLogging.LoggerBase.writeEvent("getSimpleProductsByIdFinished", 44134, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting simple products by ids finished.");
        }

        public static getKitComponentsError(): void {
            TsLogging.LoggerBase.writeEvent("GetKitComponentsError", 44136, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "One of the kit components could not be retrieved.");
        }

        public static searchProductsByProductIdsStarted(): void {
            TsLogging.LoggerBase.writeEvent("SearchProductsByProductIdsStarted", 44140, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Searching products by product ids started.");
        }

        public static searchProductsByProductIdsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("SearchProductsByProductIdsError", 44141, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Searching products by product ids failed with error {0}.");
        }

        public static searchProductsByProductIdsFinished(): void {
            TsLogging.LoggerBase.writeEvent("SearchProductsByProductIdsFinished", 44142, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Searching products by product ids finished.");
        }

        // Checkout Service - Event Range: 44200 - 44299.
        public static checkoutServiceGetAllDeliveryOptionDescriptionsStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetAllDeliveryOptionDescriptionsStarted", 44200, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get all delivery option descriptions started.");
        }

        public static checkoutServiceGetAllDeliveryOptionDescriptionsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetAllDeliveryOptionDescriptionsError", 44201, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get all delivery option descriptions failed with error {0}.");
        }

        public static checkoutServiceGetAllDeliveryOptionDescriptionsFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetAllDeliveryOptionDescriptionsFinished", 44202, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get all delivery option descriptions finished.");
        }

        public static checkoutServiceGetDeliveryPreferencesStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetDeliveryPreferencesStarted", 44204, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting delivery preferences started.");
        }

        public static checkoutServiceGetDeliveryPreferencesError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetDeliveryPreferencesError", 44205, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting delivery preferences failed with error {0}.");
        }

        public static checkoutServiceGetDeliveryPreferencesFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetDeliveryPreferencesFinished", 44206, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting delivery preferences finished.");
        }

        public static checkoutServiceGetOrderDeliveryOptionsStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetOrderDeliveryOptionsStarted", 44208, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting order delivery options started.");
        }

        public static checkoutServiceGetOrderDeliveryOptionsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetOrderDeliveryOptionsError", 44209, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting order delivery options failed with error {0}.");
        }

        public static checkoutServiceGetOrderDeliveryOptionsFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetOrderDeliveryOptionsFinished", 44210, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting order delivery options finished.");
        }

        public static checkoutServiceGetItemDeliveryOptionsStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetItemDeliveryOptionsStarted", 44212, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting item delivery options started.");
        }

        public static checkoutServiceGetItemDeliveryOptionsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetItemDeliveryOptionsError", 44213, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting item delivery options failed with error {0}.");
        }

        public static checkoutServiceGetItemDeliveryOptionsFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetItemDeliveryOptionsFinished", 44214, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting item delivery options finished.");
        }

        public static checkoutServiceUpdateDeliverySpecificationsStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateDeliverySpecificationsStarted", 44216, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update of order delivery specifications started.");
        }

        public static checkoutServiceUpdateDeliverySpecificationsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateDeliverySpecificationsError", 44217, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Update of order delivery specifications failed with error {0}.");
        }

        public static checkoutServiceUpdateDeliverySpecificationsFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateDeliverySpecificationsFinished", 44218, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update of order delivery specifications finished.");
        }

        public static checkoutServiceUpdateLineDeliverySpecificationsStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateLineDeliverySpecificationsStarted", 44220, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update of line delivery specifications started.");
        }

        public static checkoutServiceUpdateLineDeliverySpecificationsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateLineDeliverySpecificationsError", 44221, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Update of line delivery specifications failed with error {0}.");
        }

        public static checkoutServiceUpdateLineDeliverySpecificationsFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceUpdateLineDeliverySpecificationsFinished", 44222, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update of line delivery specifications finished.");
        }

        public static checkoutServiceGetPaymentCardTypesStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetPaymentCardTypesStarted", 44224, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting payment card types started.");
        }

        public static checkoutServiceGetPaymentCardTypesError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetPaymentCardTypesError", 44225, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting payment card types failed with error {0}.");
        }

        public static checkoutServiceGetPaymentCardTypesFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetPaymentCardTypesFinished", 44226, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting payment card types finished.");
        }

        public static checkoutServiceGetGiftCardBalanceStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetGiftCardBalanceStarted", 44228, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting gift card balance started.");
        }

        public static checkoutServiceGetGiftCardBalanceError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetGiftCardBalanceError", 44229, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Getting gift card balance failed with error {0}.");
        }

        public static checkoutServiceGetGiftCardBalanceFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetGiftCardBalanceFinished", 44230, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Getting gift card balance finished.");
        }

        public static checkoutServiceSubmitOrderStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceSubmitOrderStarted", 44232, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Submit order started.");
        }

        public static checkoutServiceSubmitOrderError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceSubmitOrderError", 44233, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Submit order failed with error {0}.");
        }

        public static checkoutServiceSubmitOrderFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceSubmitOrderFinished", 44234, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Submit order finished.");
        }

        public static checkoutServiceGetCardPaymentAcceptUrlStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetCardPaymentAcceptUrlStarted", 44235, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get card payment accept url started.");
        }

        public static checkoutServiceGetCardPaymentAcceptUrlFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetCardPaymentAcceptUrlFinished", 44236, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get card payment accept url finished.");
        }

        public static checkoutServiceGetCardPaymentAcceptUrlError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceGetCardPaymentAcceptUrlError", 44237, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get card payment accept url failed with error {0}.");
        }

        public static checkoutServiceRetrieveCardPaymentAcceptResultStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceRetrieveCardPaymentAcceptResultStarted", 44238, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Retrieve card payment accept result started.");
        }

        public static checkoutServiceRetrieveCardPaymentAcceptResultFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceRetrieveCardPaymentAcceptResultFinished", 44239, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Retrieve card payment accept result finished.");
        }

        public static checkoutServiceRetrieveCardPaymentAcceptResultError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceRetrieveCardPaymentAcceptResultError", 44240, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Retrieve card payment accept result failed with error {0}.");
        }

        public static checkoutServiceCleanUpAfterSuccessfulOrderStarted(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceCleanUpAfterSuccessfulOrderStarted", 44241, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Shopping cart clean failed post successful submit order started.");
        }

        public static checkoutServiceCleanUpAfterSuccessfulOrderError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceCleanUpAfterSuccessfulOrderError", 44242, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Shopping cart clean failed post successful submit order with error {0}.");
        }

        public static checkoutServiceCleanUpAfterSuccessfulOrderFinished(): void {
            TsLogging.LoggerBase.writeEvent("CheckoutServiceCleanUpAfterSuccessfulOrderFinished", 44243, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Shopping cart clean failed post successful submit order finished.");
        }

        // Channel Service - Event Range: 44300 - 44349.
        public static channelServiceGetChannelConfigurationStarted(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetChannelConfigurationStarted", 44300, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get channel Configuration started.");
        }

        public static channelServiceGetChannelConfigurationError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetChannelConfigurationError", 44301, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get channel Configuration failed with error {0}.");
        }

        public static channelServiceGetChannelConfigurationFinished(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetChannelConfigurationFinished", 44302, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get channel Configuration finished.");
        }

        public static channelServiceGetCountryRegionInfoStarted(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCountryRegionInfoStarted", 44304, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get country region info started.");
        }

        public static channelServiceGetCountryRegionInfoError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCountryRegionInfoError", 44305, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get country region info failed with error {0}.");
        }

        public static channelServiceGetCountryRegionInfoFinished(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCountryRegionInfoFinished", 44306, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get country region info finished.");
        }

        public static channelServiceGetStateProvinceInfoStarted(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetStateProvinceInfoStarted", 44308, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get state province info started.");
        }

        public static channelServiceGetStateProvinceInfoError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetStateProvinceInfoError", 44309, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get state province info error {0}.");
        }

        public static channelServiceGetStateProvinceInfoFinished(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetStateProvinceInfoFinished", 44310, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get state province info finished.");
        }

        public static channelServiceGetTenderTypesStarted(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetTenderTypesStarted", 44312, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get tender types started.");
        }

        public static channelServiceGetTenderTypesError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetTenderTypesError", 44313, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get tender types failed with error {0}.");
        }

        public static channelServiceGetTenderTypesFinished(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetTenderTypesFinished", 44314, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get tender types finished.");
        }

        public static channelServiceGetCardTypesStarted(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCardTypesStarted", 44315, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get card types started.");
        }

        public static channelServiceGetCardTypesError(error: string): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCardTypesError", 44316, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get card types failed with error {0}.");
        }

        public static channelServiceGetCardTypesFinished(): void {
            TsLogging.LoggerBase.writeEvent("ChannelServiceGetCardTypesFinished", 44317, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get card types finished.");
        }

        // Item Availability Service - Event Range: 44350 - 44369.
        public static storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityStarted(): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresWithAvailabilityStarted", 44350, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get nearby stores with availability started.");
        }

        public static storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityError(error: string): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresWithAvailabilityError", 44351, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get nearby stores with availability failed with error {0}.");
        }

        public static storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityFinished(): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresWithAvailabilityFinished", 44352, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get nearby stores with availability finished.");
        }

        public static storeProductAvailabilityServiceGetNearbyStoresStarted(): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresStarted", 44354, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get nearby stores started.");
        }

        public static storeProductAvailabilityServiceGetNearbyStoresError(error: string): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresError", 44355, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get nearby stores failed with error {0}.");
        }

        public static storeProductAvailabilityServiceGetNearbyStoresFinished(): void {
            TsLogging.LoggerBase.writeEvent("StoreProductAvailabilityServiceGetNearbyStoresFinished", 44356, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get nearby stores finished.");
        }

        // Loyalty Service - Event Range: 44370 - 44399.
        public static loyaltyServiceGetLoyaltyCardsStarted(): void {
            TsLogging.LoggerBase.writeEvent("LoyaltyServiceGetLoyaltyCardsStarted", 44370, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get loyalty cards started.");
        }

        public static loyaltyServiceGetLoyaltyCardsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("LoyaltyServiceGetLoyaltyCardsError", 44371, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get loyalty cards failed with error {0}.");
        }

        public static loyaltyServiceGetLoyaltyCardsFinished(): void {
            TsLogging.LoggerBase.writeEvent("LoyaltyServiceGetLoyaltyCardsFinished", 44372, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get loyalty cards finished.");
        }

        public static loyaltyServiceUpdateLoyaltyCardIdStarted(): void {
            TsLogging.LoggerBase.writeEvent("loyaltyServiceUpdateLoyaltyCardIdStarted", 44374, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update loyalty card id started.");
        }

        public static loyaltyServiceUpdateLoyaltyCardIdError(error: string): void {
            TsLogging.LoggerBase.writeEvent("LoyaltyServiceUpdateLoyaltyCardIdError", 44375, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Update loyalty card id failed with error {0}.");
        }

        public static loyaltyServiceUpdateLoyaltyCardIdFinished(): void {
            TsLogging.LoggerBase.writeEvent("LoyaltyServiceUpdateLoyaltyCardIdFinished", 44376, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Update loyalty card id finished.");
        }

        // Customer Service - Event Range: 44400 - 44499.
        public static customerServiceGetCustomerStarted(): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceGetCustomerStarted", 44400, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get customer started.");
        }

        public static customerServiceGetCustomerError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceGetCustomerError", 44401, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get customer failed with error {0}.");
        }

        public static customerServiceGetCustomerFinished(): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceGetCustomerFinished", 44402, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get customer finished.");
        }

        public static customerServiceIsAuthenticationSessionStarted(): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceIsAuthenticationSessionStarted", 44403, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Is authentication session started.");
        }

        public static customerServiceIsAuthenticationSessionError(error: string): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceIsAuthenticationSessionError", 44404, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Is authentication session failed with error {0}.");
        }

        public static customerServiceIsAuthenticationSessionFinished(): void {
            TsLogging.LoggerBase.writeEvent("CustomerServiceIsAuthenticationSessionFinished", 44405, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Is authentication session finished.");
        }

        // Order Service - Event Range: 44500 - 44599.
        public static getOrderHistoryStarted(): void {
            TsLogging.LoggerBase.writeEvent("GetOrderHistoryStarted", 44500, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get order history started.");
        }

        public static getOrderHistoryError(error: string): void {
            TsLogging.LoggerBase.writeEvent("GetOrderHistoryError", 44501, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get order history failed with error {0}.");
        }

        public static getOrderHistoryFinished(): void {
            TsLogging.LoggerBase.writeEvent("GetOrderHistoryFinished", 44502, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get order history finished.");
        }

        public static getOrderDetailsStarted(): void {
            TsLogging.LoggerBase.writeEvent("GetOrderDetailsStarted", 44504, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get order details started.");
        }

        public static getOrderDetailsError(error: string): void {
            TsLogging.LoggerBase.writeEvent("GetOrderDetailsError", 44505, 1, TsLogging.EventChannel.Operational, TsLogging.EventLevel.Error, [], "", "", "Get order details failed with error {0}.");
        }

        public static getOrderDetailsFinished(): void {
            TsLogging.LoggerBase.writeEvent("GetOrderDetailsFinished", 44506, 1, TsLogging.EventChannel.Debug, TsLogging.EventLevel.Informational, [], "", "", "Get order details finished.");
        }
    }
}