/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    export class Cart {
        private _cartView;
        private _editRewardCardDialog;
        private _loadingDialog;
        private _loadingText;
        private cart;
        private isShoppingCartLoaded;
        private isShoppingCartEnabled;
        private isPromotionCodesEnabled;
        private kitVariantProductType;
        private errorMessages: ObservableArray<string>;
        private errorPanel;
        private dialogOverlay;
        private supportDiscountCodes;
        private supportLoyaltyReward;
        private displayPromotionBanner;
        private currencyStringTemplate: string;

        constructor(element) {
            this._cartView = $(element);
            this.errorMessages = ko.observableArray<string>([]);
            this.errorPanel = this._cartView.find(" > .msax-ErrorPanel");
            this.kitVariantProductType = ko.observable<CommerceProxy.Entities.ProductType>(CommerceProxy.Entities.ProductType.KitVariant);
            this._editRewardCardDialog = this._cartView.find('.msax-EditRewardCard');
            this._loadingDialog = this._cartView.find('.msax-Loading');
            this._loadingText = this._loadingDialog.find('.msax-LoadingText');
            LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);

            this.isShoppingCartLoaded = ko.observable<boolean>(false);
            this.supportDiscountCodes = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_CartDiscountCodes) ? true : msaxValues.msax_CartDiscountCodes.toLowerCase() == "true");
            this.supportLoyaltyReward = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_CartLoyaltyReward) ? true : msaxValues.msax_CartLoyaltyReward.toLowerCase() == "true");
            this.displayPromotionBanner = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_CartDisplayPromotionBanner) ? true : msaxValues.msax_CartDisplayPromotionBanner.toLowerCase() == "true");

            this.getShoppingCart();

            var cart = new CommerceProxy.Entities.CartClass(null);
            cart.CartLines = [];
            cart.DiscountCodes = [];
            this.cart = ko.observable<CommerceProxy.Entities.CartClass>(cart);

            // Subscribing to the UpdateShoppingCart event.
            CartWebApi.OnUpdateShoppingCart(this, this.updateShoppingCart);

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

            this.isPromotionCodesEnabled = ko.computed(() => {
                return !Utils.isNullOrUndefined(this.cart()) && Utils.hasElements(this.cart().DiscountCodes);
            });
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

        private quantityMinusClick(cartLine: CommerceProxy.Entities.CartLine) {
            // Handles quantity minus click.
            if (cartLine.Quantity == 1) {
                this.removeFromCartClick(cartLine);
            } else {
                cartLine.Quantity = cartLine.Quantity - 1;
                this.updateQuantity([cartLine]);
            }
        }

        private quantityPlusClick(cartLine: CommerceProxy.Entities.CartLine) {
            // Handles quantity plus click.
            cartLine.Quantity = cartLine.Quantity + 1;
            this.updateQuantity([cartLine]);
        }

        private quantityTextBoxChanged(cartLine: CommerceProxy.Entities.CartLine, valueAccesor) {
            // Handles quantity text box change event.
            var srcElement = valueAccesor.target;
            if (!Utils.isNullOrUndefined(srcElement)) {
                if (Utils.isNullOrWhiteSpace(srcElement.value)) {
                    srcElement.value = cartLine.Quantity;
                    return;
                }

                var enteredNumber: number = Number(srcElement.value);
                if (isNaN(enteredNumber)) {
                    srcElement.value = cartLine.Quantity;
                    return;
                }

                if (enteredNumber != cartLine.Quantity) {
                    cartLine.Quantity = enteredNumber;
                    if (cartLine.Quantity < 0) {
                        cartLine.Quantity = 1;
                    }

                    if (cartLine.Quantity == 0) {
                        this.removeFromCartClick(cartLine);
                    }
                    else {
                        this.updateQuantity([cartLine]);
                    }
                }
            }
        }

        private closeDialogAndDisplayError(errorMessages: string[], isError: boolean) {
            LoadingOverlay.CloseLoadingDialog();
            this.showError(errorMessages, isError);
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

        private editRewardCardOverlayClick() {
            this.dialogOverlay = $('.ui-widget-overlay');
            this.dialogOverlay.on('click', $.proxy(this.closeEditRewardCardDialog, this));
        }

        private createEditRewardCardDialog() {
            // Creates the edit reward card dialog box.
            this._editRewardCardDialog.dialog({
                modal: true,
                title: Resources.String_186, // Edit reward card
                autoOpen: false,
                draggable: true,
                resizable: false,
                closeOnEscape: true,
                show: { effect: "fadeIn", duration: 500 },
                hide: { effect: "fadeOut", duration: 500 },
                width: 500,
                height: 300,
                dialogClass: 'msax-Control'
            });
        }

        private showEditRewardCardDialog() {
            // Specify the close event handler for the dialog.
            $('.ui-dialog-titlebar-close').on('click', $.proxy(this.closeEditRewardCardDialog, this));

            // Displays the edit reward card dialog.
            this._editRewardCardDialog.dialog('open');
            this.editRewardCardOverlayClick();
        }

        private closeEditRewardCardDialog() {
            // Close the dialog.
            this._editRewardCardDialog.dialog('close');
        }

        private continueShoppingClick() {
            // Handles continue shopping click
            if (!Utils.isNullOrWhiteSpace(msaxValues.msax_ContinueShoppingUrl)) {
                window.location.href = msaxValues.msax_ContinueShoppingUrl;
            }
        }

        private updateShoppingCart(event, data) {
            // Handles the UpdateShoppingCart event.
            CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Shopping, this.displayPromotionBanner())
                .done((cart) => {
                    this.currencyStringTemplate = Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                    this.cart(data);
                    this.errorPanel.hide();
                    this.isShoppingCartLoaded(true);

                    this._cartView.find('.msax-ChargeAmount .msax-FooterValue').tooltip();
                    this._cartView.find('.msax-TaxAmount .msax-FooterValue').tooltip();
                });
        }

        // Service calls

        private getShoppingCart() {
            CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
            LoadingOverlay.ShowLoadingDialog();
            CartWebApi.GetCart(CommerceProxy.Entities.CartType.Shopping, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                if (!Utils.isNullOrUndefined(cart)) {
                    CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                } else {
                        this.showError([Resources.String_63], true); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                }

                this.createEditRewardCardDialog();
                LoadingOverlay.CloseLoadingDialog();
                CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
            })

                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Resources.String_63); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private removeFromCartClick(cartLine: CommerceProxy.Entities.CartLine) {
            CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Shopping, [cartLine.LineId], this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                if (!Utils.isNullOrUndefined(cart)) {
                    CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                } else {
                        this.showError([Resources.String_64], true); // Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.
                    }

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartError, errors, Resources.String_64); // Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private updateQuantity(cartLines: CommerceProxy.Entities.CartLine[]) {
            CommerceProxy.RetailLogger.shoppingCartUpdateQuantityStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            CartWebApi.UpdateQuantity(CommerceProxy.Entities.CartType.Shopping, cartLines, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                if (!Utils.isNullOrUndefined(cart)) {
                    CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                } else {
                        this.showError([Resources.String_65], true); // Sorry, something went wrong. The product quantity couldn't be updated. Please refresh the page and try again.
                }

                LoadingOverlay.CloseLoadingDialog();
                CommerceProxy.RetailLogger.shoppingCartUpdateQuantityFinished();
            })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateQuantityError, errors, Resources.String_65); // Sorry, something went wrong. The product quantity couldn't be updated. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private applyPromotionCode(cart: CommerceProxy.Entities.Cart, valueAccesor) {
            CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            var srcElement = valueAccesor.target;

            if (!Utils.isNullOrUndefined(srcElement)
                && !Utils.isNullOrUndefined(srcElement.parentElement)
                && !Utils.isNullOrUndefined(srcElement.parentElement.firstElementChild)) {

                if (!Utils.isNullOrWhiteSpace(srcElement.parentElement.firstElementChild.value)) {
                    var promoCode = srcElement.parentElement.firstElementChild.value;

                    CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Shopping, promoCode, true, this)
                        .done((cart: CommerceProxy.Entities.Cart) => {
                        if (!Utils.isNullOrUndefined(cart)) {
                            CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                        } else {
                                this.showError([Resources.String_93], true); // Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.
                        }
                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeFinished();
                    })
                        .fail((errors: CommerceProxy.ProxyError[]) => {
                            Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeError, errors, Resources.String_93);  // Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.
                            var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                            this.closeDialogAndDisplayError(errorMessages, true);
                        });
                }
                else {
                    this.showError([Resources.String_97], true); /* Please enter a promotion code */
                    LoadingOverlay.CloseLoadingDialog();
                }
            }
            else {
                LoadingOverlay.CloseLoadingDialog();
            }
        }

        private removePromotionCode(cart: CommerceProxy.Entities.Cart, valueAccesor) {
            CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            var srcElement = valueAccesor.target;

            if (!Utils.isNullOrUndefined(srcElement)
                && !Utils.isNullOrUndefined(srcElement.parentElement)
                && !Utils.isNullOrUndefined(srcElement.parentElement.lastElementChild)
                && !Utils.isNullOrWhiteSpace(srcElement.parentElement.lastElementChild.textContent)) {
                var promoCode = srcElement.parentElement.lastElementChild.textContent;

                CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Shopping, promoCode, false, this)
                    .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                    } else {
                            this.showError([Resources.String_94], true); // Sorry, something went wrong. The promotion code could not be removed successfully. Please refresh the page and try again.
                    }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeError, errors, Resources.String_94); // Sorry, something went wrong. The promotion code could not be removed successfully. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
            }
            else {
                LoadingOverlay.CloseLoadingDialog();
            }
        }

        private updateLoyaltyCardId() {
            CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            var loyaltyCardId = this._editRewardCardDialog.find('#RewardCardTextBox').val();

            if (!Utils.isNullOrWhiteSpace(loyaltyCardId)) {
                CartWebApi.UpdateLoyaltyCardId(CommerceProxy.Entities.CartType.Shopping, loyaltyCardId, this)
                    .done((cart: CommerceProxy.Entities.Cart) => {
                        if (!Utils.isNullOrUndefined(cart)) {
                            this.closeEditRewardCardDialog();
                        } else {
                            this.showError([Resources.String_232], true); // Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.
                        }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdError, errors, Resources.String_232); // Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeEditRewardCardDialog();
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
            }
        }
    }
}