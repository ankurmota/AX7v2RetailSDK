/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Common/Helpers/EcommerceTypes.ts" />
/// <reference path="../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    export class MiniCart {

        private _cartView;
        private _miniCart;
        private _miniCartButton;
        private cart;
        private isShoppingCartEnabled;
        private errorMessages: ObservableArray<string>;
        private errorPanel;
        private isCheckoutCart;
        private isMiniCartVisible: boolean;
        private cartType: CommerceProxy.Entities.CartType;
        private miniCartButtonLocation: any;
        private miniCartButtonWidth : number;
        private miniCartWidth: number;
        private currencyStringTemplate: string;

        constructor(element) {
            this._cartView = $(element);
            this.errorMessages = ko.observableArray<string>([]);
            this.errorPanel = this._cartView.find(" .msax-ErrorPanel");
            this._miniCart = this._cartView.find(" > .msax-MiniCart");

            this.isCheckoutCart = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_IsCheckoutCart) ? false : msaxValues.msax_IsCheckoutCart.toLowerCase() == "true");

            if (!this.isCheckoutCart()) {
                this.cartType = CommerceProxy.Entities.CartType.Shopping;
                this.getShoppingCart();
            }
            else {
                this.cartType = CommerceProxy.Entities.CartType.Checkout;
            }

            var cart = new CommerceProxy.Entities.CartClass(null);
            cart.CartLines = [];
            cart.DiscountCodes = [];
            this.cart = ko.observable<CommerceProxy.Entities.Cart>(cart);

            if (this.isCheckoutCart()) {
                // Subscribing to the UpdateCheckoutCart event.
                CartWebApi.OnUpdateCheckoutCart(this, this.updateCart);
            }
            else {
                // Subscribing to the UpdateShoppingCart event.
                CartWebApi.OnUpdateShoppingCart(this, this.updateCart);
            }

            // Handles the keypress event on the control.
            this._cartView.keypress(function (event) {
                if (event.keyCode == 13 /* enter */ || event.keyCode == 8 /* backspace */ || event.keyCode == 27 /* esc */) {
                    event.preventDefault();
                    return false;
                }

                return true;
            });

            // Computed observables.
            this.isShoppingCartEnabled = ko.computed(() => {
                return !Utils.isNullOrUndefined(this.cart()) && Utils.hasElements(this.cart().CartLines);
            });

            $(window).resize($.proxy(this.repositionMiniCart, this));
        }

        private getResx(key: string) {
            // Gets the resource value.
            return Resources[key];
        }

        private formatCurrencyString(amount: number): any {
            if (isNaN(amount)) {
                return amount;
            }
            var formattedCurrencyString: string = "";

            if (!Utils.isNullOrUndefined(amount)) {
                if (Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                    formattedCurrencyString = amount.toString();
                }
                else {
                    formattedCurrencyString = Utils.format(this.currencyStringTemplate, Utils.formatNumber(amount));
                }
            }

            return formattedCurrencyString;
        }

        private shoppingCartNextClick(viewModel: Checkout, event) {
            // If redirection url is specified redirect to the url on button click.
            if (!Utils.isNullOrWhiteSpace(msaxValues.msax_CheckoutUrl)) {
                window.location.href = msaxValues.msax_CheckoutUrl;
            }
        }

        private disableUserActions() {
            this._cartView.find('*').disabled = true;
        }

        private enableUserActions() {
            this._cartView.find('*').disabled = false;
        }

        private showError(errorMessages: string[], isError: boolean) {
            this.errorMessages(errorMessages);

            if (isError) {
                this.errorPanel.addClass("msax-Error");
            }
            else if (this.errorPanel.hasClass("msax-Error")) {
                this.errorPanel.removeClass("msax-Error");
            }

            this.errorPanel.show();
            $(window).scrollTop(0);
        }

        private viewCartClick() {
            // If redirection url is specified redirect to the url on click.
            if (!Utils.isNullOrWhiteSpace(msaxValues.msax_ShoppingCartUrl)) {
                window.location.href = msaxValues.msax_ShoppingCartUrl;
            }
        }

        private showMiniCart() {
            // display mini cart
            this.toggleCartDisplay(true);
        }

        private hideMiniCart() {
            // hide mini cart
            this.toggleCartDisplay(false);
        }

        private toggleCartDisplay(show: boolean) {
            if (!Utils.isNullOrUndefined(this._miniCartButton)) {
                // Setting a default location from the top of the screen for the mini cart.         
                var locationOffScreen = -1 * this._miniCart.height() - 200; // 200 is for the offset for new product that will be added to cart. Location off screen is adjusted after product is added.
                this.miniCartWidth = this._miniCart.width() + 3; // Handling 3 pixel offset.
                this.miniCartButtonLocation = this._miniCartButton.offset();
                this.miniCartButtonWidth = this._miniCartButton.width();
                var miniCartButtonHeight = this._miniCartButton.height() + 3;

                // toggle the display of mini cart
                if (show) {
                    this.isMiniCartVisible = false;

                    setTimeout($.proxy(function () {
                        if (!this.isMiniCartVisible) {
                            this._miniCart.animate({ top: this.miniCartButtonLocation.top + miniCartButtonHeight, left: this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth }, 300, 'linear');
                        }
                    }, this), 500);
                }
                else {
                    this.isMiniCartVisible = true;

                    setTimeout($.proxy(function () {
                        if (this.isMiniCartVisible) {
                            this._miniCart.animate({ top: locationOffScreen, left: this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth }, 300);
                        }
                    }, this), 500);
                }
            }
        }

        private repositionMiniCart() {
            if (Utils.isNullOrUndefined(this._miniCartButton)) {
                this._miniCartButton = this._cartView.find("#MiniCartButton");
            }
            this.miniCartButtonLocation = this._miniCartButton.offset();
            this.miniCartButtonWidth = this._miniCartButton.width();
            this.miniCartWidth = this._miniCart.width();
            this._miniCart[0].style.left = this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth + "px";
        }

        private updateCart(event, data) {
            // Event handler for UpdateShoppingCart/UpdateCheckoutCart events.
            CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Shopping, false)
                .done((cart) => {
                    this.currencyStringTemplate = Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                    this.cart(cart);
                    this._miniCartButton = this._cartView.find("#MiniCartButton");
                    this.repositionMiniCart();
                    this.hideMiniCart();       
                });
        }

        // Service calls

        private getShoppingCart() {
            CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
            this.disableUserActions();
            CartWebApi.GetCart(CommerceProxy.Entities.CartType.Shopping, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                    } else {
                        this.showError([Resources.String_63], true); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    }

                    this.enableUserActions();
                    this.errorPanel.hide();
                    CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Resources.String_63); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.showError(errorMessages, true);
                    this.enableUserActions();
                });
        }

        private removeFromCartClick(cartLine: CommerceProxy.Entities.CartLine) {
            this.disableUserActions();
            CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();

            CartWebApi.RemoveFromCart(this.cartType, [cartLine.LineId], this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                if (!Utils.isNullOrUndefined(cart)) {
                    CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                } else {
                        this.showError([Resources.String_64], true); // Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.
                    }
                this.enableUserActions();
                this.errorPanel.hide();
                CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartFinished();
            })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartError, errors, Resources.String_64); // Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.showError(errorMessages, true);
                    this.enableUserActions();
                });
        }
    }
}