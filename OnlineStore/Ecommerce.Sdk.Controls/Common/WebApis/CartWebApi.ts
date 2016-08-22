/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/EcommerceTypes.ts" />
/// <reference path="../../Libraries.Proxies.Retail.TypeScript.d.ts" />

"use strict";

// Web services proxy
// Encapsulates ajax calls to the Ajax services.
module Contoso.Retail.Ecommerce.Sdk.Controls {
    export class Constants {
        public static ProductUrlString: string = '/';
        public static NoOpUrlString: string = '#';
        public static OfferNamesProperty: string = 'OfferNames';
        public static ProductNameProperty: string = 'ProductName';
        public static ProductDescriptionProperty: string = 'ProductDescription';
        public static ProductDimensionProperty: string = 'ProductDimension';
        public static ProductDimensionArrayProperty: string = 'ProductDimensionValues';
        public static ProductUrlProperty: string = 'ProductUrl';
        public static ImageMarkup50pxProperty: string = 'ImageMarkup50px';
        public static ImageMarkup180pxProperty: string = 'ImageMarkup180px';
        public static ProductTypeProperty: string = 'ProductType';
        public static KitComponentsProperty: string = 'KitComponents';
        public static KitComponentCountProperty: string = 'KitComponentCount';
        public static KitComponentPriceProperty: string = 'KitComponentPrice';
    }

    export class CartWebApi {

        private static proxy: AjaxProxy;

        public static GetProxy() {
            this.proxy = new AjaxProxy(msaxValues.msax_CartWebApiUrl + '/');
        }

        // Calls the GetCart method.
        public static GetCart(cartType: CommerceProxy.Entities.CartType, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);
            var data = {
                "isCheckoutSession": isCheckoutSession
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetCart",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Calls the RemoveFromCart method.
        public static RemoveFromCart(cartType: CommerceProxy.Entities.CartType, lineIds: string[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);
            var data = {
                "isCheckoutSession": isCheckoutSession,
                "lineIds": lineIds
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "RemoveItems",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Calls the UpdateQuantity method.
        public static UpdateQuantity(cartType: CommerceProxy.Entities.CartType, cartLines: CommerceProxy.Entities.CartLine[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);
            var data = {
                "isCheckoutSession": isCheckoutSession,
                "cartLines": cartLines
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "UpdateItems",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Gets the promotion banners for the cart.
        public static GetPromotions(cartType: CommerceProxy.Entities.CartType, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CartPromotions> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CartPromotions>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);
            var data = {
                "isCheckoutSession": isCheckoutSession
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetPromotions",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Adds or removes discount code from shopping cart.
        public static AddOrRemovePromotion(cartType: CommerceProxy.Entities.CartType, promotionCode: string, isAdd: boolean, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);
            var data = {
                "isCheckoutSession": isCheckoutSession,
                "promotionCode": promotionCode,
                "isAdd": isAdd
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "AddOrRemovePromotionCode",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Initiates the checkout workflow.
        public static CommenceCheckout(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var data = {};

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "CommenceCheckout",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        // Returns the cart after updating its promotions lines based on the values provided.
        public static GetCartUpdatedWithPromotions(cart: CommerceProxy.Entities.Cart, cartPromotions: CommerceProxy.Entities.CartPromotions): CommerceProxy.Entities.Cart {
            if (Utils.hasElements(cartPromotions.HeaderPromotions)) {
                cart.PromotionLines = cartPromotions.HeaderPromotions;
            }

            if (Utils.hasElements(cartPromotions.CartLinePromotions)) {
                for (var i = 0; i < cartPromotions.CartLinePromotions.length; i++) {
                    var currentLinePromotion: CommerceProxy.Entities.CartLinePromotion = cartPromotions.CartLinePromotions[i];
                    for (var j = 0; j < cart.CartLines.length; j++) {
                        var currentCartLine: CommerceProxy.Entities.CartLine = cart.CartLines[j];
                        if (currentLinePromotion.LineId == currentCartLine.LineId) {
                            currentCartLine.PromotionLines = currentLinePromotion.Promotions;
                        }
                    }
                }
            }

            return cart;
        }

        // Function that exposes the UpdateShoppingCart event. 
        // The caller can send the context in which the event should be triggered and irrespective of the triggered context the context given here is available in the event handler.
        public static OnUpdateShoppingCart(callerContext: any, handler: any) {
            $(document).on('UpdateShoppingCart', $.proxy(handler, callerContext));
        }

        // Function that exposes the UpdateCheckoutCart event. 
        // The caller can send the context in which the event should be triggered and irrespective of the triggered context the context given here is available in the event handler.
        public static OnUpdateCheckoutCart(callerContext: any, handler: any) {
            $(document).on('UpdateCheckoutCart', $.proxy(handler, callerContext));
        }

        // Returns markup to be displayed when image url is null or when image url does not exist.
        private static GetNoImageMarkup(): string {
            return Utils.format('<span class=\"msax-NoImageContainer\"></span>');
        }

        // Build image markup with given width and height.
        private static BuildImageMarkup(imageUrl: string, imageAltText: string, width: number, height: number): string {
            var imageClassName = "msax-Image";

            if (!Utils.isNullOrWhiteSpace(imageUrl)) {
                var errorScript = Utils.format('onerror=\"this.parentNode.innerHTML=Contoso.Retail.Ecommerce.Sdk.Controls.CartWebApi.GetNoImageMarkup();\"');
                return Utils.format('<img src=\"{0}\" class=\"{1}\" alt=\"{2}\" width=\"{3}\" height=\"{4}\" {5} />', imageUrl, imageClassName, imageAltText, width, height, errorScript);
            }
            else {
                return CartWebApi.GetNoImageMarkup();
            }
        }

        public static UpdateShoppingCartOnResponse(cart: CommerceProxy.Entities.Cart,
            cartType: CommerceProxy.Entities.CartType,
            fetchPromotions: boolean): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult: CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart> = new CommerceProxy.AsyncResult();
            if (!Utils.isNullOrUndefined(cart) && Utils.hasElements(cart.CartLines)) {
                // Initializing all additional dynamically created properties to empty strings.
                this.initializeDynamicCartLineProperties(cart);
                var currencyStringTemplate: string = Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                // Iterate through the cart lines to populate the product details that are needed to be shown in the UI.

                var productIds: number[] = [];
                for (var j = 0; j < cart.CartLines.length; j++) {
                    productIds.push(cart.CartLines[j].ProductId);
                }

                CommerceProxy.RetailLogger.getSimpleProductsByIdStarted();
                ProductWebApi.GetSimpleProducts(productIds, this)
                    .done((simpleProducts: CommerceProxy.Entities.SimpleProduct[]) => {
                        CommerceProxy.RetailLogger.getSimpleProductsByIdFinished();

                        //Create a dictionary
                        var simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[] = [];
                        for (var i = 0; i < simpleProducts.length; i++) {
                            var key: number = simpleProducts[i].RecordId;
                            simpleProductsByIdMap[key] = simpleProducts[i];
                        }

                        for (var j = 0; j < cart.CartLines.length; j++) {
                            var cartLine: CommerceProxy.Entities.CartLine = cart.CartLines[j];
                            Core.populateProductDetailsForCartLine(cartLine, simpleProductsByIdMap, currencyStringTemplate);

                            // Adding more product details specifically for kit products.
                            Core.populateKitItemDetailsForCartLine(cartLine, simpleProductsByIdMap, currencyStringTemplate);
                        }
                    
                        // Add offer names
                        for (var i = 0; i < cart.CartLines.length; i++) {

                            var cartLine: CommerceProxy.Entities.CartLine = cart.CartLines[i];

                            cartLine[Constants.OfferNamesProperty] = "";
                            if (Utils.hasElements(cartLine.DiscountLines)) {
                                for (var j = 0; j < cartLine.DiscountLines.length; j++) {
                                    cartLine[Constants.OfferNamesProperty] = cartLine[Constants.OfferNamesProperty] + cartLine.DiscountLines[j].OfferName + " ";
                                }
                            }
                        }

                        // Updating the order total to be the same value as sub total in the cart when the cart lines do not have any delivery mode set on them. 
                        // We do not want to show the currently esimated order total because the charges and tax value currently calculated is not the final value. 
                        // It will be recalculated again based on the delivery preferences entered.
                        if (!Utils.isNullOrUndefined(cart)) {
                            var resetOrderTotal: boolean = false;

                            if (Utils.isNullOrUndefined(cart.DeliveryMode) &&
                                !Utils.isNullOrUndefined(cart.CartLines)) {
                                var cartLines = cart.CartLines;

                                for (var i = 0; i < cartLines.length; i++) {
                                    if (Utils.isNullOrUndefined(cartLines[i].DeliveryMode)) {
                                        resetOrderTotal = true;
                                        break;
                                    }
                                }
                            }

                            if (resetOrderTotal) {
                                cart.TotalAmount = cart.SubtotalAmountWithoutTax;
                            }
                        }

                        // Add promotions
                        if (fetchPromotions) {
                            CommerceProxy.RetailLogger.shoppingCartGetPromotionsStarted;
                            this.GetPromotions(cartType, this)
                                .done((cartPromotions: CommerceProxy.Entities.CartPromotions) => {
                                    CommerceProxy.RetailLogger.shoppingCartGetPromotionsFinished();
                                    if (!Utils.isNullOrUndefined(cartPromotions)) {
                                        cart = CartWebApi.GetCartUpdatedWithPromotions(cart, cartPromotions);
                                        asyncResult.resolve(cart);
                                    }
                                })
                                .fail((errors: CommerceProxy.ProxyError[]) => {
                                    CommerceProxy.RetailLogger.shoppingCartGetPromotionsError(errors[0].LocalizedErrorMessage);
                                    asyncResult.resolve(cart);
                                });
                        }
                        else {
                            asyncResult.resolve(cart);
                        }
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        CommerceProxy.RetailLogger.getSimpleProductsByIdError(errors[0].LocalizedErrorMessage);
                        asyncResult.resolve(cart);
                    });
            }
            else {
                asyncResult.resolve(cart);
            }

            return asyncResult;
        }

        public static TriggerCartUpdateEvent(cartType: CommerceProxy.Entities.CartType, updatedCart: CommerceProxy.Entities.Cart) {
            if (cartType == CommerceProxy.Entities.CartType.Checkout) {
                $(document).trigger('UpdateCheckoutCart', [updatedCart]);
            }
            else {
                $(document).trigger('UpdateShoppingCart', [updatedCart]);
            }
        }

        private static prepareCartLinesForServer(cartLines: CommerceProxy.Entities.CartLine[]): CommerceProxy.Entities.CartLine[] {
            if (Utils.isNullOrUndefined(cartLines)) {
                return null;
            }

            for (var i = 0; i < cartLines.length; i++) {
                var cartLine = cartLines[i];

                if (!Utils.isNullOrUndefined(cartLine)) {
                    delete cartLine[Constants.OfferNamesProperty];
                    delete cartLine[Constants.ProductNameProperty];
                    delete cartLine[Constants.ProductDescriptionProperty];
                    delete cartLine[Constants.ProductDimensionProperty];
                    delete cartLine[Constants.ImageMarkup50pxProperty];
                    delete cartLine[Constants.ImageMarkup180pxProperty];
                    delete cartLine[Constants.KitComponentsProperty];
                    delete cartLine[Constants.ProductTypeProperty];
                    delete cartLine[Constants.ProductUrlProperty];
                    delete cartLine[Constants.KitComponentCountProperty];
                    delete cartLine[Constants.KitComponentPriceProperty];
                }
            }

            return cartLines;
        }

        private static initializeDynamicCartLineProperties(cart: CommerceProxy.Entities.Cart) {
            for (var i = 0; i < cart.CartLines.length; i++) {
                var cartLine: CommerceProxy.Entities.CartLine = cart.CartLines[i];
                cartLine[Constants.OfferNamesProperty] = '';
                cartLine[Constants.ProductNameProperty] = '';
                cartLine[Constants.ProductDescriptionProperty] = '';
                cartLine[Constants.ProductDimensionProperty] = '';
                cartLine[Constants.ImageMarkup50pxProperty] = '';
                cartLine[Constants.ImageMarkup180pxProperty] = '';
                cartLine[Constants.KitComponentsProperty] = '';
                cartLine[Constants.ProductTypeProperty] = '';
                cartLine[Constants.ProductUrlProperty] = '';
                cartLine[Constants.KitComponentCountProperty] = '';
                cartLine[Constants.KitComponentPriceProperty] = '';
            }
        }

        public static UpdateLoyaltyCardId(cartType: CommerceProxy.Entities.CartType, loyaltyCardId: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var isCheckoutSession: boolean = (cartType == CommerceProxy.Entities.CartType.Checkout);

            var data = {
                "isCheckoutSession": isCheckoutSession,
                "loyaltyCardId": loyaltyCardId
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "UpdateLoyaltyCardId",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static SubmitOrder(cartTenderLines: CommerceProxy.Entities.CartTenderLine[], emailAddress: string, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.SalesOrder> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.SalesOrder>(callerContext);

            var data = {
                "cartTenderLines": cartTenderLines,
                "emailAddress": emailAddress
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "CreateOrder",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetDeliveryPreferences(callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CartDeliveryPreferences> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CartDeliveryPreferences>(callerContext);

            var data = {};

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetDeliveryPreferences",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetOrderDeliveryOptionsForShipping(shipToAddress: CommerceProxy.Entities.Address, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.DeliveryOption[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.DeliveryOption[]>(callerContext);

            var data = {
                "shipToAddress": shipToAddress,
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetOrderDeliveryOptionsForShipping",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetLineDeliveryOptionsForShipping(lineShippingAddresses: CommerceProxy.Entities.LineShippingAddress[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.SalesLineDeliveryOption[]> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.SalesLineDeliveryOption[]>(callerContext);

            var data = {
                "lineShippingAddresses": lineShippingAddresses,
                "queryResultSettings": Core.getDefaultQueryResultSettings()
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetLineDeliveryOptionsForShipping",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static UpdateDeliverySpecification(headerLevelDeliveryOption: CommerceProxy.Entities.DeliverySpecification, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var data = {
                "deliverySpecification": headerLevelDeliveryOption
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "UpdateDeliverySpecification",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static UpdateLineDeliverySpecifications(lineLevelDeliveryOptions: CommerceProxy.Entities.LineDeliverySpecification[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.Cart>(callerContext);

            var data = {
                "lineDeliverySpecifications": lineLevelDeliveryOptions
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "UpdateLineDeliverySpecifications",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static GetCardPaymentAcceptPoint(cardPaymentAcceptSettings: CommerceProxy.Entities.CardPaymentAcceptSettings, callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.CardPaymentAcceptPoint> {
            var asyncResult = new CommerceProxy.AsyncResult<CommerceProxy.Entities.CardPaymentAcceptPoint>(callerContext);

            var data = {
                "cardPaymentAcceptSettings": cardPaymentAcceptSettings
            };

            if (Utils.isNullOrUndefined(this.proxy)) {
                this.GetProxy();
            }

            this.proxy.SubmitRequest(
                "GetCardPaymentAcceptPoint",
                data,
                (response) => {
                    asyncResult.resolve(response);
                },
                (errors: ErrorResponse) => {
                    asyncResult.reject(errors.responseJSON);
                });

            return asyncResult;
        }

        public static CleanUpAfterSuccessfulOrder(linesIdsToRemoveFromShoppingCart: string[], callerContext: any): CommerceProxy.IAsyncResult<CommerceProxy.Entities.Cart> {
            return CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Shopping, linesIdsToRemoveFromShoppingCart, callerContext);
        }
    }
}