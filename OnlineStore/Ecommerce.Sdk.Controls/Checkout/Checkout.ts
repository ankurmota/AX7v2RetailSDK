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

    /**
     * Represents the card payment accept message.
     */
    export interface CardPaymentAcceptMessage {
        type: string;
        value: string;
    }

    /**
     * List of async methods that are called during initialization.
     * There is a limit of 31 methods that can be supported
     */
    export enum InitEntitySet {
        None = 0,
        CheckoutCart = 1,
        DeliveryDescriptions = (1 << 1),
        IsAuthSession = (1 << 2),
        Customer = (1 << 3),
        ChannelConfigurations = (1 << 4),
        TenderTypes = (1 << 5),
        CountryRegion = (1 << 6),
        DeliveryPreferences = (1 << 7),
        CardTypes = (1 << 8),
        All = None | CheckoutCart | DeliveryDescriptions | IsAuthSession | Customer | ChannelConfigurations | TenderTypes | CountryRegion | DeliveryPreferences | CardTypes
    }
    
    /**
     * List of async methods that are called when next is clicked on delivery preferences.
     * There is a limit of 31 methods that can be supported
     */
    export enum InitPaymentEntitySet {
        None = 0,
        LoyaltyCards = 1,
        SetDeliveryPreferences = (1 << 1),
        All = None | LoyaltyCards | SetDeliveryPreferences
    }

    export class Checkout {

        private _checkoutView;
        private _loadingDialog;
        private _loadingText;
        private cart: Observable<CommerceProxy.Entities.Cart>;
        private isShoppingCartEnabled: Computed<boolean>;
        private kitVariantProductType;
        public errorMessages: ObservableArray<string>;
        public errorPanel;
        private nextButton;

        private _checkoutFragments = {
            DeliveryPreferences: "msax-DeliveryPreferences",
            PaymentInformation: "msax-PaymentInformation",
            Review: "msax-Review",
            Confirmation: "msax-Confirmation"
        };

        private dialogOverlay;

        private _initEntitySetCompleted: InitEntitySet;
        private _initEntitySetFailed: InitEntitySet;
        private _initEntityErrors: CommerceProxy.ProxyError[];

        private _initPaymentEntitySetCompleted: InitPaymentEntitySet;
        private _initPaymentEntitySetFailed: InitPaymentEntitySet;
        private _initPaymentEntityErrors: CommerceProxy.ProxyError[];

        // Config
        private allDeliveryOptionDescriptions: CommerceProxy.Entities.DeliveryOption[];
        private pickUpInStoreDeliveryModeCode: string;
        private emailDeliveryModeCode: string;
        private bingMapsToken: string;
        private giftCardItemId: string;
        private channelCurrencyCode: string;
        private countries: ObservableArray<{ CountryCode: string; CountryName: string }>;
        private states: ObservableArray<{}>;
        private hasInventoryCheck: Observable<boolean>;
        private storedCustomerAddresses: ObservableArray<{ Value: CommerceProxy.Entities.Address; Text: string }>;
        private isGiftCardPaymentAllowed: boolean;
        private isCreditCardPaymentAllowed: boolean;
        private isLoyaltyCardPaymentAllowed: boolean;

        // Delivery information 
        private selectedDeliveryOptionByLineIdMap: CommerceProxy.Entities.DeliverySpecification[]; // This is used to store the selections made so far.
        private latestHeaderLevelDeliverySpecification: Observable<CommerceProxy.Entities.DeliverySpecification>;

        private cartDeliveryPreferences: CommerceProxy.Entities.CartDeliveryPreferences;
        private productNameByItemVariantIdMap: string[];

        // Billing information
        private paymentCardAddress: Observable<CommerceProxy.Entities.Address>; // Represents the billing address bound to the UI.

        // Delivery preferences section.
        private allowedHeaderLevelDeliveryPreferences: ObservableArray<{ Value: CommerceProxy.Entities.DeliveryPreferenceType; Text: string }>;
        private selectedOrderDeliveryPreference: Observable<CommerceProxy.Entities.DeliveryPreferenceType>;

        private tempShippingAddress: Observable<CommerceProxy.Entities.Address>;

        // Views
        private _deliveryPreferencesView;
        private _availableStoresView;
        private _location;
        private map;
        private mapStoreLocator;
        private DisableTouchInputOnMap = false;
        private showItemDeliveryPreferenceDialog: Observable<boolean>;

        private orderLevelSelectedAddress: Observable<CommerceProxy.Entities.Address>;
        private lineLevelSelectedAddress: Observable<CommerceProxy.Entities.Address>;
        private availableDeliveryOptions: ObservableArray<CommerceProxy.Entities.DeliveryOption>; // Can apply to order level or line level delivery based on the context

        private sendEmailToMe: Observable<boolean>;
        private isAuthenticated: Observable<boolean>;
        private recepientEmailAddress: string;
        private _emailAddressTextBox;

        private searchLocation;
        private orgUnitLocations: CommerceProxy.Entities.OrgUnitLocation[];
        private availabilityByOrgUnitMap: CommerceProxy.Entities.ItemAvailability[][]; //Item availability indexed by the org unit number
        private availabilityFlagByOrgUnitMap: boolean[]; // Boolean flag to indicate if the specified org unit has all the required products.

        private displayLocations: ObservableArray<CommerceProxy.Entities.OrgUnitLocation>;
        private deliveryPreferenceToValidate: Observable<string>;

        private currentLineLevelSelectedDeliveryPreference: Observable<CommerceProxy.Entities.DeliveryPreferenceType>;

        private _itemLevelDeliveryPreferenceSelection;
        private itemDeliveryPreferenceToValidate: Observable<string>;
        private currentCartLine: Observable<CommerceProxy.Entities.CartLine>;
        private currentLineDeliverySpecification: Observable<CommerceProxy.Entities.LineDeliverySpecification>;

        private _deliveryPreferencesFragments = {
            ShipItemsOrderLevel: "msax-ShipItemsOrderLevel",
            PickUpInStoreOrderLevel: "msax-PickUpInStoreOrderLevel",
            EmailOrderLevel: "msax-EmailOrderLevel",
            ItemLevelPreference: "msax-ItemLevelPreference"
        };

        private _itemDeliveryPreferencesFragments = {
            ShipItemsItemLevel: "msax-ShipItemsItemLevel",
            PickUpInStoreItemLevel: "msax-PickUpInStoreItemLevel",
            EmailItemLevel: "msax-EmailItemLevel"
        };

        // Payment methods section.
        private _paymentView;
        private _addDiscountCodeDialog;
        private useShippingAddressForBilling: Observable<boolean>;
        private isBillingAddressSameAsShippingAddress: Observable<boolean>;
        private paymentCardTypes: ObservableArray<string>;
        private tenderLines: CommerceProxy.Entities.CartTenderLine[] = [];
        private maskedCreditCard: Computed<string>;
        private expirationMonths: ObservableArray<{ key: number; value: any }>;
        private confirmEmailValue: Observable<string>;
        private formattedPaymentTotal: Observable<string>;
        private payCreditCard: Observable<boolean>;
        private payGiftCard: Observable<boolean>;
        private payLoyaltyCard: Observable<boolean>;
        private _creditCardPanel;
        private _giftCardPanel;
        private _loyaltyCardPanel;
        private creditCardAmount: number;
        private paymentCard: Observable<CommerceProxy.Entities.PaymentCard>;
        private giftCardAmount: number;
        private giftCardNumber: Observable<string>;
        private giftCardBalance: Observable<string>;
        private isGiftCardInfoAvailable: Observable<boolean>;
        private checkGiftCardAmountValidity: boolean = false;
        private supportedTenderTypes: CommerceProxy.Entities.TenderType[] = [];
        private cardTypes: CommerceProxy.Entities.CardTypeInfo[] = [];

        private loyaltyCards: ObservableArray<string>;
        private loyaltyCardAmount: number;
        private loyaltyCardNumber: Observable<string>;
        private totalAmount: number;

        private currencyStringTemplate: string;

        private expirationYear: string[] = [
            "2014",
            "2015",
            "2016",
            "2017",
            "2018",
            "2019"
        ];

        // Card payment accept properties.
        private isCardPaymentAcceptPage: Observable<boolean>;
        private cardPaymentAcceptPageUrl: Observable<string>;
        private cardPaymentAcceptPageSubmitUrl: string;
        private cardPaymentAcceptMessageOrigin: string;
        private cardPaymentAcceptMessageHandlerProxied: any;
        private cardPaymentAcceptCardType: string;
        private cardPaymentAcceptCardPrefix: string;
        private tokenizedCartTenderLine: CommerceProxy.Entities.CartTenderLine;
        private static CARDPAYMENTACCEPTPAGEHEIGHT: string = "msax-cc-height";
        private static CARDPAYMENTACCEPTPAGEERROR: string = "msax-cc-error";
        private static CARDPAYMENTACCEPTPAGERESULT: string = "msax-cc-result";
        private static CARDPAYMENTACCEPTPAGESUBMIT: string = "msax-cc-submit";
        private static CARDPAYMENTACCEPTCARDPREFIX: string = "msax-cc-cardprefix";

        // Order review section.
        private _editRewardCardDialog;
        private isPromotionCodesEnabled: Computed<boolean>;
        private formattedCreditCardAmount: Observable<string>;
        private formattedGiftCardAmount: Observable<string>;
        private formattedLoyaltyCardAmount: Observable<string>;
        private isEmailDeliverySet: Observable<boolean>;
        private isOrderLevelDeliverySet: Observable<boolean>;
        private displayPromotionBanner: Observable<boolean>;

        // Order confirmation section.
        private orderNumber;

        constructor(element) {
            this._checkoutView = $(element);
            this.errorMessages = ko.observableArray<string>([]);
            this.errorPanel = this._checkoutView.find(" > .msax-ErrorPanel");
            this.nextButton = this._checkoutView.find('.msax-Next');
            this.kitVariantProductType = ko.observable<CommerceProxy.Entities.ProductType>(CommerceProxy.Entities.ProductType.KitVariant);
            this._loadingDialog = this._checkoutView.find('.msax-Loading');
            this._loadingText = this._loadingDialog.find('.msax-LoadingText');
            LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);

            this.countries = ko.observableArray([{ CountryCode: "NoSelection", CountryName: "Select a country:" }]);
            this.states = ko.observableArray(null);
            var cart = new CommerceProxy.Entities.CartClass(null);
            cart.CartLines = [];
            cart.DiscountCodes = [];
            cart.ShippingAddress = new CommerceProxy.Entities.AddressClass(null);
            this.cart = ko.observable<CommerceProxy.Entities.Cart>(cart);

            var selectedHeaderLevelDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass();
            selectedHeaderLevelDeliveryOption.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
            this.latestHeaderLevelDeliverySpecification = ko.observable<CommerceProxy.Entities.DeliverySpecification>(selectedHeaderLevelDeliveryOption);
            this.isAuthenticated = ko.observable<boolean>(false);

            this._initEntitySetCompleted = 0;
            this._initEntitySetFailed = 0;
            this._initEntityErrors = [];

            LoadingOverlay.ShowLoadingDialog();
            this.isAuthenticatedSession();// Establish if this is Anonymous or Signed in user
            this.commenceCheckout();
            this.getAllDeliveryOptionDescriptions();

            // Subscribing to the UpdateCheckoutCart event.
            CartWebApi.OnUpdateCheckoutCart(this, this.updateCheckoutCart);

            // Delivery preferences section.
            // Initialize delivery preferences with "Please select a delivery preference..."
            this.allowedHeaderLevelDeliveryPreferences = ko.observableArray([{ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Resources.String_159 }]);
            ({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Resources.String_159 });
            this.selectedOrderDeliveryPreference = ko.observable<CommerceProxy.Entities.DeliveryPreferenceType>(null);
            this._deliveryPreferencesView = this._checkoutView.find(" > ." + this._checkoutFragments.DeliveryPreferences);
            this.deliveryPreferenceToValidate = ko.observable<string>(null);
            this.tempShippingAddress = ko.observable<CommerceProxy.Entities.Address>(null);
            this.selectedDeliveryOptionByLineIdMap = [];

            this.storedCustomerAddresses = ko.observableArray<{ Value: CommerceProxy.Entities.Address; Text: string }>(null);
            this.orderLevelSelectedAddress = ko.observable<CommerceProxy.Entities.Address>(null);
            this.lineLevelSelectedAddress = ko.observable<CommerceProxy.Entities.Address>(null);
            this.availableDeliveryOptions = ko.observableArray<CommerceProxy.Entities.DeliveryOption>(null);
            var selectedOrderDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
            selectedOrderDeliveryOption.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);

            this.isBillingAddressSameAsShippingAddress = ko.observable<boolean>(false);
            this.sendEmailToMe = ko.observable<boolean>(false);
            this.displayLocations = ko.observableArray<CommerceProxy.Entities.OrgUnitLocation>(null);
            this.hasInventoryCheck = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_HasInventoryCheck) ? true : msaxValues.msax_HasInventoryCheck.toLowerCase() == "true");

            this.currentCartLine = ko.observable<CommerceProxy.Entities.CartLine>(null);

            var selectedLineDeliveryOption: CommerceProxy.Entities.LineDeliverySpecification = new CommerceProxy.Entities.LineDeliverySpecificationClass(null);
            selectedLineDeliveryOption.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
            selectedLineDeliveryOption.DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
            selectedLineDeliveryOption.DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
            this.currentLineDeliverySpecification = ko.observable<CommerceProxy.Entities.LineDeliverySpecification>(selectedLineDeliveryOption);
            this.currentLineLevelSelectedDeliveryPreference = ko.observable<CommerceProxy.Entities.DeliveryPreferenceType>(CommerceProxy.Entities.DeliveryPreferenceType.None);

            this.itemDeliveryPreferenceToValidate = ko.observable<string>(null);
            this.showItemDeliveryPreferenceDialog = ko.observable<boolean>(false);
            this._itemLevelDeliveryPreferenceSelection = this._deliveryPreferencesView.find('.msax-ItemLevelPreference .msax-ItemLevelPreferenceSelection');

            // Payment methods section.
            this._paymentView = this._checkoutView.find(" > ." + this._checkoutFragments.PaymentInformation);
            this._addDiscountCodeDialog = this._paymentView.find('.msax-PayPromotionCode .msax-AddDiscountCodeDialog');
            this.useShippingAddressForBilling = ko.observable<boolean>(false);
            this.paymentCardTypes = ko.observableArray<string>(null);
            this.confirmEmailValue = ko.observable<string>('');

            this.paymentCard = ko.observable<CommerceProxy.Entities.PaymentCard>(null);
            var paymentCard = new CommerceProxy.Entities.PaymentCardClass(null);
            paymentCard.ExpirationYear = 2016;
            paymentCard.ExpirationMonth = 1;
            this.paymentCard(paymentCard);

            this.paymentCardAddress = ko.observable<CommerceProxy.Entities.Address>(new CommerceProxy.Entities.AddressClass());

            this.formattedCreditCardAmount = ko.observable<string>('');
            this.giftCardNumber = ko.observable<string>('');
            this.formattedGiftCardAmount = ko.observable<string>('');
            this.isGiftCardInfoAvailable = ko.observable<boolean>(false);
            this.giftCardBalance = ko.observable<string>('');
            this._paymentView.find('.msax-GiftCardBalance').hide();
            this.formattedPaymentTotal = ko.observable<string>(Utils.formatNumber(0));
            this._creditCardPanel = this._paymentView.find('.msax-PayCreditCard .msax-CreditCardDetails');
            this._giftCardPanel = this._paymentView.find('.msax-PayGiftCard .msax-GiftCardDetails');
            this._loyaltyCardPanel = this._paymentView.find('.msax-PayLoyaltyCard .msax-LoyaltyCardDetails');
            this.loyaltyCards = ko.observableArray<string>(null);
            this.loyaltyCardNumber = ko.observable<string>('');
            this.formattedLoyaltyCardAmount = ko.observable<string>('');

            this.payCreditCard = ko.observable<boolean>(false);
            this.payGiftCard = ko.observable<boolean>(false);
            this.payLoyaltyCard = ko.observable<boolean>(false);

            this.expirationMonths = ko.observableArray([
                { key: 1, value: Resources.String_192 },
                { key: 2, value: Resources.String_193 },
                { key: 3, value: Resources.String_194 },
                { key: 4, value: Resources.String_195 },
                { key: 5, value: Resources.String_196 },
                { key: 6, value: Resources.String_197 },
                { key: 7, value: Resources.String_198 },
                { key: 8, value: Resources.String_199 },
                { key: 9, value: Resources.String_200 },
                { key: 10, value: Resources.String_201 },
                { key: 11, value: Resources.String_202 },
                { key: 12, value: Resources.String_203 }
            ]);

            // Card payment accept properties.
            this.isCardPaymentAcceptPage = ko.observable<boolean>(false);
            this.cardPaymentAcceptPageUrl = ko.observable<string>('');
            this.cardPaymentAcceptMessageHandlerProxied = $.proxy(this.cardPaymentAcceptMessageHandler, this);
            this.tokenizedCartTenderLine = null;

            //Order review section.
            this.isEmailDeliverySet = ko.observable<boolean>(false);
            this.isOrderLevelDeliverySet = ko.observable<boolean>(false);
            this._editRewardCardDialog = this._checkoutView.find('.msax-EditRewardCard');
            this.displayPromotionBanner = ko.observable<boolean>(Utils.isNullOrUndefined(msaxValues.msax_ReviewDisplayPromotionBanner) ? true : msaxValues.msax_ReviewDisplayPromotionBanner.toLowerCase() == "true");

            // Order confirmation section.
            this.orderNumber = ko.observable<string>(null);

            // function that handles the keypress event on the control.
            this._checkoutView.keypress(function (event) {
                if (event.keyCode == 13 /* enter */ || event.keyCode == 27 /* esc */) {
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

            this.selectedOrderDeliveryPreference.subscribe((newValue: CommerceProxy.Entities.DeliveryPreferenceType) => {
                this.resetSelectedOrderShippingOptions();
                this.hideError();
                this.isEmailDeliverySet(false);
                this.isOrderLevelDeliverySet(true);

                if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {

                    var headerLevelDeliverySpecification = this.latestHeaderLevelDeliverySpecification();
                    if (Utils.isNullOrUndefined(headerLevelDeliverySpecification.DeliveryAddress)) {
                        // DeliveryAddress is retained as AddressClass object.
                        // If these values are not initialized here then if user goes back to Ship items option after selecting other delivery options
                        // then undefined binding will result in DOM elements not being displayed in ship items address form.
                        headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                        this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                    }

                    headerLevelDeliverySpecification = this.latestHeaderLevelDeliverySpecification();
                    if ((msaxValues.msax_IsDemoMode.toLowerCase() == "true")
                        && (Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.City)
                            || Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.Street)
                            || Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.State)
                            || Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.ZipCode)
                            || Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName))) {
                        this.autoFillCheckout();
                    }

                    var tempAddress: CommerceProxy.Entities.Address = Utils.clone(this.latestHeaderLevelDeliverySpecification().DeliveryAddress);
                    this.tempShippingAddress(tempAddress);
                    this.deliveryPreferenceToValidate(' .' + this._deliveryPreferencesFragments.ShipItemsOrderLevel);
                    this.showDeliveryPreferenceFragment(this._deliveryPreferencesFragments.ShipItemsOrderLevel);
                }
                else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                    this.deliveryPreferenceToValidate(' .' + this._deliveryPreferencesFragments.PickUpInStoreOrderLevel);
                    this.showDeliveryPreferenceFragment(this._deliveryPreferencesFragments.PickUpInStoreOrderLevel);
                    this._availableStoresView = this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel .msax-AvailableStores");
                    this._location = this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel input.msax-Location");
                    this._availableStoresView.hide();
                    this.map = this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel .msax-Map");
                    this.getMap();
                }
                else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                    this.deliveryPreferenceToValidate(' .' + this._deliveryPreferencesFragments.EmailOrderLevel);
                    this.showDeliveryPreferenceFragment(this._deliveryPreferencesFragments.EmailOrderLevel);

                    var _sendEmailToMeCheckBox = this._checkoutView.find('.msax-EmailOrderLevel .msax-SendEmailToMe');
                    this._emailAddressTextBox = this._deliveryPreferencesView.find('.msax-EmailOrderLevel .msax-EmailTextBox');
                    if (this._emailAddressTextBox.val() == this.recepientEmailAddress) {
                        this.sendEmailToMe(true);
                    }
                    else {
                        this.sendEmailToMe(false);
                    }

                    var headerLevelDeliverySpecification: CommerceProxy.Entities.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                    headerLevelDeliverySpecification.DeliveryModeId = this.emailDeliveryModeCode;
                    headerLevelDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                    headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                    headerLevelDeliverySpecification.ElectronicDeliveryEmailAddress = this._emailAddressTextBox.val();

                    this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                    this.isEmailDeliverySet(true);
                }
                else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                    this.deliveryPreferenceToValidate(' .' + this._deliveryPreferencesFragments.ItemLevelPreference);
                    this.showDeliveryPreferenceFragment(this._deliveryPreferencesFragments.ItemLevelPreference);
                }
                else {
                    this.deliveryPreferenceToValidate('');
                    this.showDeliveryPreferenceFragment('');
                }
            }, this);

            // Code to execute whenever delivery preference at line level is changed.
            this.currentLineLevelSelectedDeliveryPreference.subscribe((deliveryPreferenceType: CommerceProxy.Entities.DeliveryPreferenceType) => {
                this.resetSelectedOrderShippingOptions();
                this.hideError();
                this.currentLineDeliverySpecification().LineId = this.currentCartLine().LineId;
                this.isEmailDeliverySet(false);
                this.isOrderLevelDeliverySet(false);

                // When the dialog box is opened we trigger the subscription manually, in which case newValue will be undefined.
                // If the shipping option id was set previously newValue is updated to reflect that.
                // If the shipping option id is not set it means the dialog is opened for the first time for the item, so set it to ship items by default.
                if (Utils.isNullOrUndefined(deliveryPreferenceType)) {
                    var currentDeliverySpecification = this.currentLineDeliverySpecification().DeliverySpecification;

                    if (Utils.isNullOrUndefined(currentDeliverySpecification)) {
                        this.currentLineLevelSelectedDeliveryPreference(CommerceProxy.Entities.DeliveryPreferenceType.None);
                    }
                    else {
                        if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                            deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                        }
                        else if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                            deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                        }
                        else if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                            deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                        }
                        else {
                            this.currentLineLevelSelectedDeliveryPreference(CommerceProxy.Entities.DeliveryPreferenceType.None);
                        }
                    }
                }

                if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                    var currentDeliverySpecification: CommerceProxy.Entities.DeliverySpecification = this.currentLineDeliverySpecification().DeliverySpecification;
                    currentDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                    var tempAddress = Utils.clone(currentDeliverySpecification.DeliveryAddress);
                    this.tempShippingAddress(tempAddress);

                    var addressDropDownInitialized = false;
                    if (this.isAuthenticated() && !Utils.isNullOrUndefined(currentDeliverySpecification.DeliveryAddress)) {
                        for (var index in this.storedCustomerAddresses()) {
                            if (currentDeliverySpecification.DeliveryAddress.Name == this.storedCustomerAddresses()[index].Value.Name &&
                                currentDeliverySpecification.DeliveryAddress.Street == this.storedCustomerAddresses()[index].Value.Street &&
                                currentDeliverySpecification.DeliveryAddress.City == this.storedCustomerAddresses()[index].Value.City &&
                                currentDeliverySpecification.DeliveryAddress.State == this.storedCustomerAddresses()[index].Value.State &&
                                currentDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName == this.storedCustomerAddresses()[index].Value.ThreeLetterISORegionName &&
                                currentDeliverySpecification.DeliveryAddress.ZipCode == this.storedCustomerAddresses()[index].Value.ZipCode) {
                                this.lineLevelSelectedAddress(this.storedCustomerAddresses()[index].Value);
                                addressDropDownInitialized = true;
                            }
                        }
                    }

                    if (!addressDropDownInitialized) {
                        this.lineLevelSelectedAddress(null);
                    }

                    this.itemDeliveryPreferenceToValidate(' .' + this._itemDeliveryPreferencesFragments.ShipItemsItemLevel);
                    this.showItemDeliveryPreferenceFragment(this._itemDeliveryPreferencesFragments.ShipItemsItemLevel);
                }
                else if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                    this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                    this.itemDeliveryPreferenceToValidate(' .' + this._itemDeliveryPreferencesFragments.PickUpInStoreItemLevel);
                    this.showItemDeliveryPreferenceFragment(this._itemDeliveryPreferencesFragments.PickUpInStoreItemLevel);
                    this._availableStoresView = this._itemLevelDeliveryPreferenceSelection.find(" .msax-PickUpInStoreItemLevel .msax-AvailableStores");
                    this._location = this._itemLevelDeliveryPreferenceSelection.find(" .msax-PickUpInStoreItemLevel input.msax-Location");
                    this._availableStoresView.hide();
                    this.map = this._itemLevelDeliveryPreferenceSelection.find(" .msax-Map");
                    this.getMap();
                }
                else if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                    this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                    this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                    this.itemDeliveryPreferenceToValidate(' .' + this._itemDeliveryPreferencesFragments.EmailItemLevel);
                    this.showItemDeliveryPreferenceFragment(this._itemDeliveryPreferencesFragments.EmailItemLevel);

                    var _sendEmailToMeCheckBox = this._itemLevelDeliveryPreferenceSelection.find('.msax-SendEmailToMe');
                    this._emailAddressTextBox = this._itemLevelDeliveryPreferenceSelection.find('.msax-EmailItemLevel .msax-EmailTextBox');
                    if (this._emailAddressTextBox.val() == this.recepientEmailAddress) {
                        this.sendEmailToMe(true);
                    }
                    else {
                        this.sendEmailToMe(false);
                    }

                    this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = this.emailDeliveryModeCode;
                }
                else {
                    this.itemDeliveryPreferenceToValidate('');
                    this.showItemDeliveryPreferenceFragment('');
                }
            }, this);

            // Code to execute when the shipping address selected on screen is changed.
            this.orderLevelSelectedAddress.subscribe((newValue: CommerceProxy.Entities.Address) => {
                if (!Utils.isNullOrUndefined(newValue)) {
                    this.tempShippingAddress(newValue);
                    var element: any = {};
                    element.id = "OrderAddressStreet";
                    element.value = newValue.Street;
                    this.resetOrderAvailableDeliveryMethods(element);
                    element.id = "OrderAddressCity";
                    element.value = newValue.City;
                    this.resetOrderAvailableDeliveryMethods(element);
                    element.id = "OrderAddressZipCode";
                    element.value = newValue.ZipCode;
                    this.resetOrderAvailableDeliveryMethods(element);
                    element.id = "OrderAddressState";
                    element.value = newValue.State;
                    this.resetOrderAvailableDeliveryMethods(element);
                    element.id = "OrderAddressCountry";
                    element.value = newValue.ThreeLetterISORegionName;
                    this.resetOrderAvailableDeliveryMethods(element);
                    element.id = "OrderAddressName";
                    element.value = newValue.Name;
                    this.resetOrderAvailableDeliveryMethods(element);
                }
            }, this);

            this.lineLevelSelectedAddress.subscribe((newValue) => {
                if (!Utils.isNullOrUndefined(newValue)) {
                    this.tempShippingAddress(newValue);
                    var element: any = {};
                    element.id = "ItemAddressStreet";
                    element.value = newValue.Street;
                    this.resetItemAvailableDeliveryMethods(element);
                    element.id = "ItemAddressCity";
                    element.value = newValue.City;
                    this.resetItemAvailableDeliveryMethods(element);
                    element.id = "ItemAddressZipCode";
                    element.value = newValue.ZipCode;
                    this.resetItemAvailableDeliveryMethods(element);
                    element.id = "ItemAddressState";
                    element.value = newValue.State;
                    this.resetItemAvailableDeliveryMethods(element);
                    element.id = "ItemAddressCountry";
                    element.value = newValue.ThreeLetterISORegionName;
                    this.resetItemAvailableDeliveryMethods(element);
                    element.id = "ItemAddressName";
                    element.value = newValue.Name;
                    this.resetItemAvailableDeliveryMethods(element);
                }
            }, this);

            this.isBillingAddressSameAsShippingAddress.subscribe((isValueSet: boolean) => {
                var paymentCardAddress = this.paymentCardAddress();
                var email = paymentCardAddress.Email;
                if (isValueSet && !Utils.isNullOrUndefined(this.cart().ShippingAddress)) {
                    paymentCardAddress = this.cart().ShippingAddress;
                    this.getStateProvinceInfoService(paymentCardAddress.ThreeLetterISORegionName);
                }
                else {
                    paymentCardAddress = new CommerceProxy.Entities.AddressClass(null);
                    this.states(null);
                }

                //payment.PaymentAddress.Email = email;
                this.paymentCardAddress(paymentCardAddress);

                return isValueSet;
            }, this);

            this.sendEmailToMe.subscribe((isSendEmailToMeSet: boolean) => {
                if (isSendEmailToMeSet) {
                    if (Utils.isNullOrWhiteSpace(this.recepientEmailAddress)) {
                        this.showError([Resources.String_119], true); // Sorry, something went wrong. An error occurred while trying to get the email address. Please enter the email address in the text box below.
                    }

                    // For line level delivery update currentLineDeliverySpecification.
                    if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                        this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailAddress = this.recepientEmailAddress;
                        this.currentLineDeliverySpecification(this.currentLineDeliverySpecification());
                    }
                    // For order level delivery update latestHeaderLevelDeliverySpecification.
                    else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                        this.latestHeaderLevelDeliverySpecification().ElectronicDeliveryEmailAddress = this.recepientEmailAddress;
                        this.latestHeaderLevelDeliverySpecification(this.latestHeaderLevelDeliverySpecification());
                    }
                }

                return isSendEmailToMeSet;
            }, this);

            this.maskedCreditCard = ko.computed(() => {
                var ccNumber = '';

                if (!Utils.isNullOrUndefined(this.paymentCard())) {
                    var cardNumber = this.paymentCard().CardNumber;

                    if (!Utils.isNullOrUndefined(cardNumber)) {
                        var ccLength = cardNumber.length;
                        if (ccLength > 4) {
                            for (var i = 0; i < ccLength - 4; i++) {
                                ccNumber += '*';
                                if ((i + 1) % 4 == 0) {
                                    ccNumber += '-';
                                }
                            }
                            // Display only the last 4 digits.
                            ccNumber += cardNumber.substring(ccLength - 4, ccLength);
                        }
                    }
                }

                return ccNumber;
            });
        }

        private loadXMLDoc(filename) {
            var xhttp;

            if (XMLHttpRequest) {
                xhttp = new XMLHttpRequest();
            }
            else {
                xhttp = new ActiveXObject("Microsoft.XMLHTTP");
            }
            xhttp.open("GET", filename, false);
            xhttp.send();
            return xhttp.responseXML;
        }

        private checkIfCurrentLineLevelDeliveryMode(selectedLineDeliveryOption: CommerceProxy.Entities.LineDeliverySpecification, valueToCheck: string): boolean {
            var result: boolean = false;
            if (!Utils.isNullOrUndefined(selectedLineDeliveryOption) && !Utils.isNullOrUndefined(selectedLineDeliveryOption.DeliverySpecification)) {
                if (selectedLineDeliveryOption.DeliverySpecification.DeliveryModeId == valueToCheck) {
                    result = true;
                }
            }

            return result;
        }

        private lineLevelDeliveryOptionClick(selectedDeliveryModeId: string): boolean {
            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = selectedDeliveryModeId;
            return true;
        }

        private autoFillCheckout() {
            if (Utils.isNullOrEmpty(msaxValues.msax_DemoDataPath)) {
                return;
            }

            var xmlDoc = this.loadXMLDoc(msaxValues.msax_DemoDataPath);

            var address = xmlDoc.getElementsByTagName("Address");
            var country = address[0].getElementsByTagName("Country");
            var name = address[0].getElementsByTagName("Name");
            var street = address[0].getElementsByTagName("Street");
            var city = address[0].getElementsByTagName("City");
            var state = address[0].getElementsByTagName("State");
            var zipcode = address[0].getElementsByTagName("Zipcode");
            var email = xmlDoc.getElementsByTagName("Email");
            var payment = xmlDoc.getElementsByTagName("Payment");
            var cardNumber = payment[0].getElementsByTagName("CardNumber");
            var ccid = payment[0].getElementsByTagName("CCID");

            var tempAddress = new CommerceProxy.Entities.AddressClass(null);
            tempAddress.Name = name[0].textContent;
            tempAddress.ThreeLetterISORegionName = country[0].textContent;
            tempAddress.Street = street[0].textContent;
            tempAddress.City = city[0].textContent;
            tempAddress.State = state[0].textContent;
            tempAddress.ZipCode = zipcode[0].textContent;
            tempAddress.Email = email[0].textContent;
            this.latestHeaderLevelDeliverySpecification().DeliveryAddress = tempAddress;
            this.latestHeaderLevelDeliverySpecification(this.latestHeaderLevelDeliverySpecification());

            var paymentCard: CommerceProxy.Entities.PaymentCard = this.paymentCard();
            this.confirmEmailValue(email[0].textContent);
            paymentCard.NameOnCard = name[0].textContent;
            paymentCard.CardNumber = cardNumber[0].textContent;
            paymentCard.CCID = ccid[0].textContent;
            this.paymentCard(paymentCard);

            this.paymentCardAddress(tempAddress);
        }

        private showCheckoutFragment(fragmentCssClass) {
            var allFragments = this._checkoutView.find("> div:not(' .msax-ProgressBar, .msax-Loading')");
            allFragments.hide();

            var fragmentToShow = this._checkoutView.find(" > ." + fragmentCssClass);
            fragmentToShow.show();

            var _progressBar = this._checkoutView.find(" > .msax-ProgressBar");
            var _delivery = _progressBar.find(" > .msax-DeliveryProgress");
            var _payment = _progressBar.find(" > .msax-PaymentProgress");
            var _review = _progressBar.find(" > .msax-ReviewProgress");
            var _progressBarEnd = _progressBar.find(" > .msax-ProgressBarEnd");

            switch (fragmentCssClass) {
                case this._checkoutFragments.DeliveryPreferences:
                    _delivery.addClass("msax-Active");
                    if (_payment.hasClass("msax-Active")) {
                        _payment.removeClass("msax-Active");
                    }
                    if (_review.hasClass("msax-Active")) {
                        _review.removeClass("msax-Active");
                    }
                    if (_progressBarEnd.hasClass("msax-Active")) {
                        _progressBarEnd.removeClass("msax-Active");
                    }
                    break;

                case this._checkoutFragments.PaymentInformation:
                    _delivery.addClass("msax-Active");
                    _payment.addClass("msax-Active");
                    if (_review.hasClass("msax-Active")) {
                        _review.removeClass("msax-Active");
                    }
                    if (_progressBarEnd.hasClass("msax-Active")) {
                        _progressBarEnd.removeClass("msax-Active");
                    }
                    break;

                case this._checkoutFragments.Review:
                    _delivery.addClass("msax-Active");
                    _payment.addClass("msax-Active");
                    _review.addClass("msax-Active");

                    if (_progressBarEnd.hasClass("msax-Active")) {
                        _progressBarEnd.removeClass("msax-Active");
                    }
                    break;

                case this._checkoutFragments.Confirmation:
                    _delivery.addClass("msax-Active");
                    _payment.addClass("msax-Active");
                    _review.addClass("msax-Active");
                    _progressBarEnd.addClass("msax-Active");
                    break;
            }
        }

        private showDeliveryPreferenceFragment(fragmentCssClass) {
            var allFragments = this._deliveryPreferencesView.find(" .msax-DeliveryPreferenceOption");
            allFragments.hide();

            if (!Utils.isNullOrWhiteSpace(fragmentCssClass)) {
                var fragmentToShow = this._deliveryPreferencesView.find(" ." + fragmentCssClass);
                fragmentToShow.show();
            }
        }

        private showItemDeliveryPreferenceFragment(fragmentCssClass) {
            var allFragments = this._itemLevelDeliveryPreferenceSelection.find(" .msax-DeliveryPreferenceOption");
            allFragments.hide();

            if (!Utils.isNullOrWhiteSpace(fragmentCssClass)) {
                var fragmentToShow = this._itemLevelDeliveryPreferenceSelection.find(" ." + fragmentCssClass);
                fragmentToShow.show();
            }
        }

        private validateItemDeliveryInformation() {
            if (Utils.isNullOrWhiteSpace(this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId)) {
                if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                    this.showError([Utils.format(Resources.String_114)], false); // Please select a store for pick up.
                }
                else if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                    this.showError([Utils.format(Resources.String_61)], false); // Please select shipping method.
                }
                else if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.None) {
                    this.showError([Resources.String_158], false); // Please select delivery preference.
                }

                return false;
            }

            this.hideError();
            return true;
        }

        private validateDeliveryInformation($shippingOptions: JQuery) {
            if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore && Utils.isNullOrWhiteSpace(this.latestHeaderLevelDeliverySpecification().DeliveryModeId)) {
                this.showError([Resources.String_114], false); // Please select a store for pick up.
                return false;
            }
            else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress && Utils.isNullOrWhiteSpace(this.latestHeaderLevelDeliverySpecification().DeliveryModeId)) {
                this.showError([Resources.String_61], false); // Please select shipping method.
                return false;
            }
            else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {

                for (var i = 0; i < this.cart().CartLines.length; i++) {
                    var cartLine: CommerceProxy.Entities.CartLine = this.cart().CartLines[i];
                    var currentLineDeliverySpecification: CommerceProxy.Entities.DeliverySpecification = this.selectedDeliveryOptionByLineIdMap[cartLine.LineId];

                    if (Utils.isNullOrWhiteSpace(currentLineDeliverySpecification.DeliveryModeId)) {
                        if (currentLineDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) { // Pick up in store
                            this.showError([Utils.format(Resources.String_114 + Resources.String_125, cartLine.ProductId)], false); // Please select a store for pick up for product {0}.
                        }
                        else if (currentLineDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) { // Ship items
                            this.showError([Utils.format(Resources.String_61 + Resources.String_125, cartLine.ProductId)], false); // Please select shipping method for product {0}.
                        }
                        else {
                            this.showError([Utils.format(Resources.String_126, cartLine.ProductId)], false); // Please select delivery preference for product {0}.
                        }

                        return false;
                    }
                }
            }
            else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.None) {
                this.showError([Resources.String_158], false); // Please select delivery preference.
                return false;
            }

            this.hideError();
            return true;
        }

        private deliveryPreferencesNextClick() {
            this.states(null); // reset states value
            LoadingOverlay.ShowLoadingDialog();
            this._initPaymentEntitySetCompleted = 0;
            this._initPaymentEntitySetFailed = 0;
            this._initPaymentEntityErrors = [];

            switch (this.selectedOrderDeliveryPreference()) {
                case CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually:
                    var selectedLineDeliveryOptions: CommerceProxy.Entities.LineDeliverySpecification[];
                    selectedLineDeliveryOptions = this.getLatestLineLevelDeliverySpecifications();
                    this.setLineLevelDeliveryOptions(selectedLineDeliveryOptions);
                    this.useShippingAddressForBilling(false);
                    break;
                case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress:
                    this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                    this.useShippingAddressForBilling(true);
                    break;
                case CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery:
                    this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                    this.useShippingAddressForBilling(false);
                    break;
                default:
                    this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                    this.useShippingAddressForBilling(false);
                    break;
            }

            // If session is authenticated get the loyalty cards for the user else hide the radio button before the custom loyalty number text box.private removeValidation(element) {
            if (this.isAuthenticated()) {
                this.getLoyaltyCards();
                this.paymentCardAddress().Email = this.recepientEmailAddress;
                this.paymentCardAddress(this.paymentCardAddress());
                this.confirmEmailValue(this.recepientEmailAddress);
            }
            else {
                var _customLoyaltyRadio = this._paymentView.find("#CustomLoyaltyRadio");
                _customLoyaltyRadio.hide();
                this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.LoyaltyCards);
            }

            this.updatePaymentValidations();
        }

        private paymentInformationPreviousClick() {
            this.showCheckoutFragment(this._checkoutFragments.DeliveryPreferences);
        }

        private getLatestLineLevelDeliverySpecifications(): CommerceProxy.Entities.LineDeliverySpecification[] {
            var latestLineLevelDeliverySpecifications: CommerceProxy.Entities.LineDeliverySpecification[] = [];
            for (var index in this.selectedDeliveryOptionByLineIdMap) {
                if (!(Utils.isNullOrUndefined(this.selectedDeliveryOptionByLineIdMap[index]))) {
                    var selectedLineDeliveryOption = new CommerceProxy.Entities.LineDeliverySpecificationClass();
                    selectedLineDeliveryOption.LineId = index;
                    selectedLineDeliveryOption.DeliverySpecification = this.selectedDeliveryOptionByLineIdMap[index];
                    latestLineLevelDeliverySpecifications.push(selectedLineDeliveryOption);
                }
            }

            return latestLineLevelDeliverySpecifications;
        }

        private getOrderLevelDeliveryAddressHeaderText(): string {
            var headerText: string = null;
            var selectedOrderLevelDeliveryPreference: CommerceProxy.Entities.DeliveryPreferenceType = this.selectedOrderDeliveryPreference();

            switch (selectedOrderLevelDeliveryPreference) {
                case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress: headerText = Resources.String_18;//Shipping Address
                    break;
                case CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore: headerText = Resources.String_115;//Store Address
                    break;
                default:
            }

            return headerText;
        }

        private validateConfirmEmailTextBox(srcElement: Element) {
            var $element = $(srcElement);
            var value: string = $element.val();

            if (value !== this.paymentCardAddress().Email) {
                this.showError([Resources.String_62], false); // The confirm email address must match the email address.
                return false;
            }

            this.hideError();
            return true;
        }

        private updatePayments() {
            // Displaying the loading dialog when 'next' is clicked on payment page.
            LoadingOverlay.ShowLoadingDialog();
            this.tenderLines = [];
            this.validatePayments();
        }

        private reviewPreviousClick() {
            if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                this.useShippingAddressForBilling(true);
            }
            else {
                this.useShippingAddressForBilling(false);
            }

            this.showCheckoutFragment(this._checkoutFragments.PaymentInformation);
            this.updatePaymentValidations();
        }

        private updatePaymentValidations() {
            if (!this.isCardPaymentAcceptPage() && this.payCreditCard()) {
                this.addValidation(this._creditCardPanel);
            }
            else {
                this.removeValidation(this._creditCardPanel);
            }

            if (this.payGiftCard()) {
                this.addValidation(this._giftCardPanel);
            }
            else {
                this.removeValidation(this._giftCardPanel);
            }

            if (this.payLoyaltyCard()) {
                this.addValidation(this._loyaltyCardPanel);
                if (this.isAuthenticated()) {
                    this.removeValidation(this._paymentView.find('#LoyaltyCustomCard'));
                }
            }
            else {
                this.removeValidation(this._loyaltyCardPanel);
            }
        }

        private quantityMinusClick(cartLine: CommerceProxy.Entities.CartLine) {
            if (cartLine.Quantity == 1) {
                this.removeFromCartClick(cartLine);
            } else {
                cartLine.Quantity = cartLine.Quantity - 1;
                this.updateQuantity([cartLine]);
            }
        }

        private quantityPlusClick(cartLine: CommerceProxy.Entities.CartLine) {
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

        private resetSelectedOrderShippingOptions() {
            this.availableDeliveryOptions(null);
            this.latestHeaderLevelDeliverySpecification().DeliveryModeId = "";
        }

        private initEntitySetCallSuccessful(entity: InitEntitySet): void {
            this.actionOnInitEntitySetCompletion(entity);
        }

        private initEntitySetCallFailed(entity: InitEntitySet, errors?: CommerceProxy.ProxyError[]): void {
            this._initEntitySetFailed = this._initEntitySetFailed | entity;
            this._initEntityErrors = this._initEntityErrors.concat(errors);
            this.actionOnInitEntitySetCompletion(entity);
        }

        private actionOnInitEntitySetCompletion(entity: InitEntitySet): void {
            if (entity === InitEntitySet.None) {
                CommerceProxy.RetailLogger.initEntitySetInvalidError(InitEntitySet[entity]);
            } else if ((this._initEntitySetCompleted & entity) === entity) {
                CommerceProxy.RetailLogger.initEntitySetMultipleTimesError(InitEntitySet[entity]);
            }

            this._initEntitySetCompleted = this._initEntitySetCompleted | entity;

            if (this._initEntitySetCompleted === InitEntitySet.All) {
                if (this._initEntitySetFailed !== InitEntitySet.None) {
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(this._initEntityErrors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                } else {
                    LoadingOverlay.CloseLoadingDialog();
                }
            } else if (this._initEntitySetCompleted > InitEntitySet.All) {
                CommerceProxy.RetailLogger.initEntitySetNoMethodNumberError();
            }
        }

        private initPaymentEntitySetCallSuccessful(entity: InitPaymentEntitySet): void {
            this.actionOnInitPaymentEntitySetCompletion(entity);
        }

        private initPaymentEntitySetCallFailed(entity: InitPaymentEntitySet, errors?: CommerceProxy.ProxyError[]): void {
            this._initPaymentEntitySetFailed = this._initPaymentEntitySetFailed | entity;
            this._initPaymentEntityErrors = this._initPaymentEntityErrors.concat(errors);
            this.actionOnInitPaymentEntitySetCompletion(entity);
        }

        private actionOnInitPaymentEntitySetCompletion(entity: InitPaymentEntitySet): void {
            if (entity === InitPaymentEntitySet.None) {
                CommerceProxy.RetailLogger.initPaymentEntitySetInvalidError(InitPaymentEntitySet[entity]);
            } else if ((this._initPaymentEntitySetCompleted & entity) === entity) {
                CommerceProxy.RetailLogger.initPaymentEntitySetMultipleTimesError(InitPaymentEntitySet[entity]);
            }

            this._initPaymentEntitySetCompleted = this._initPaymentEntitySetCompleted | entity;

            if (this._initPaymentEntitySetCompleted === InitPaymentEntitySet.All) {
                if (this._initPaymentEntitySetFailed !== InitPaymentEntitySet.None) {
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(this._initPaymentEntityErrors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                } else {
                    LoadingOverlay.CloseLoadingDialog();
                }
            } else if (this._initPaymentEntitySetCompleted > InitPaymentEntitySet.All) {
                CommerceProxy.RetailLogger.initPaymentEntitySetNoMethodNumberError();
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

        private hideError() {
            this.errorPanel.hide();
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

        private formatProductAvailabilityString(availableCount: number): string {
            if (Utils.isNullOrUndefined(availableCount) || isNaN(availableCount)) {
                availableCount = 0;
            }
            var formattedProductAvailabilityString: string = '[' + availableCount + ']';
            return formattedProductAvailabilityString;
        }

        private formatDistance(distance: number): string {
            return Utils.formatNumber(distance);
        }

        private getResx(key: string) {
            // Gets the resource value.
            return Resources[key];
        }

        private getDeliveryModeText(deliveryModeId: string): string {
            var deliveryModeText: string = "";
            if (!Utils.isNullOrUndefined(this.allDeliveryOptionDescriptions)) {
                for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                    if (this.allDeliveryOptionDescriptions[i].Code == deliveryModeId) {
                        deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                        break;
                    }
                }
            }

            return deliveryModeText;
        }

        private getDeliverySpecificationForCartLine(cartLine: CommerceProxy.Entities.CartLine): CommerceProxy.Entities.DeliverySpecification {
            var selectedDeliveryOptionForLine: CommerceProxy.Entities.DeliverySpecification = this.selectedDeliveryOptionByLineIdMap[cartLine.LineId];
            if (Utils.isNullOrUndefined(selectedDeliveryOptionForLine)) {
                selectedDeliveryOptionForLine = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                selectedDeliveryOptionForLine.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
                selectedDeliveryOptionForLine.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
            }

            return selectedDeliveryOptionForLine;
        }

        private getLineLevelDeliveryModeDescription(cartLine: CommerceProxy.Entities.CartLine): string {
            var deliveryModeText = null;
            if (!Utils.isNullOrWhiteSpace(cartLine.DeliveryMode)) {
                for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                    if (this.allDeliveryOptionDescriptions[i].Code == cartLine.DeliveryMode) {
                        deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                    }
                }
            }

            return deliveryModeText;
        }

        private getMap() {
            // Initialize the map if it exists from a previous search
            if (this.mapStoreLocator) {
                this.mapStoreLocator.dispose();
            }

            // Call to Bing to return the initial map object we will be working with
            // DisableTouchInputOnMap is a global variable which should be specified for each channel
            this.mapStoreLocator = new Microsoft.Maps.Map(this.map[0], { credentials: this.bingMapsToken, zoom: 1, disableTouchInput: this.DisableTouchInputOnMap });


            // This call sets up the search manager that we will use to geocode the starting location specified by the user
            Microsoft.Maps.loadModule('Microsoft.Maps.Search');
        }

        private getNearbyStoresWithAvailability() {
            if (!Utils.isNullOrUndefined(this._location) && !Utils.isNullOrWhiteSpace(this._location.val())) {

                this.resetSelectedOrderShippingOptions();
                this.getMap();

                var searchManager = new Microsoft.Maps.Search.SearchManager(this.mapStoreLocator);
                // Query the location details provided by the user and attempt to convert to a geocode (normalized lat/lon)
                var geocodeRequest = { where: this._location.val(), count: 1, callback: this.geocodeCallback.bind(this), errorCallback: this.geocodeErrorCallback.bind(this) };
                searchManager.geocode(geocodeRequest);
            }
        }

        private geocodeCallback(geocodeResult, userData) {
            // This function is called when a geocode query has successfully executed.
            // Report an error if the geocoding did not return any results.
            // This will be caused by a poorly formed location input by the user.
            if (!geocodeResult.results[0]) {
                this.showError([Resources.String_109], false); // Sorry, we were not able to decipher the address you gave us.  Please enter a valid Address.
                return;
            }

            this.searchLocation = geocodeResult.results[0].location;

            // Center the map based on the location result returned and a starting (city level) zoom
            // This will trigger the map view change event that will render the store plots
            this.mapStoreLocator.setView({ zoom: 11, center: this.searchLocation });

            //Add a handler for the map change event.  This event is used to render the store location plots each time the user zooms or scrolls to a new viewport
            Microsoft.Maps.Events.addHandler(this.mapStoreLocator, 'viewchanged', this.renderAvailableStores.bind(this));

            // Call the CRT to obtain a list of stores with a radius of the location provided.
            // Note that we request stores for the maximum radius we want to support (200).  The map control
            // is used to determine the "within" scope based on the users zoom settings at runtime.
            if (this.hasInventoryCheck()) {
                this.getNearbyStoresWithAvailabilityService();
            }
            else {
                this.getNearbyStoresService();
            }
        }

        private geocodeErrorCallback(geocodeRequest) {
            // This function handles an error from the geocoding service
            // These errors are thrown due to connectivity or system faults, not poorly formed location inputs.       
            this.showError([Resources.String_110], true); // Sorry, something went wrong. An error has occured while looking up the address you provided. Please refresh the page and try again.
        }

        private renderAvailableStores() {
            // Clear the current plots on the map and reset the map observables.
            this.mapStoreLocator.entities.clear();
            this._availableStoresView.hide();
            this.displayLocations(null);

            // Initialise the label index
            var storeCount = 0;
            var pin;
            var pinInfoBox;

            // Get the current bounding rectangle of the map view.  This will become our geofence for testing locations against
            var mapBounds = this.mapStoreLocator.getBounds();
            var displayLocations: CommerceProxy.Entities.OrgUnitLocation[] = [];

            // Display search location
            if (!Utils.isNullOrUndefined(this.searchLocation) && mapBounds.contains(this.searchLocation)) {
                // Plot the location to the map
                pin = new Microsoft.Maps.Pushpin(this.searchLocation, { draggable: false, text: "X" });
                this.mapStoreLocator.entities.push(pin);
            }

            // If we have stores, plot them on the map
            if (!Utils.isNullOrEmpty(this.orgUnitLocations)) {
                for (var i = 0; i < this.orgUnitLocations.length; i++) {
                    var currentStoreLocation = this.orgUnitLocations[i];
                    var locationObj = { latitude: currentStoreLocation.Latitude, longitude: currentStoreLocation.Longitude };

                    // Test each location to see if it is within the bounding rectangle
                    if (mapBounds.contains(locationObj)) {
                        this._availableStoresView.show();

                        //  Increment the counter used to manage the sequential entity index
                        storeCount++;
                        currentStoreLocation['LocationCount'] = storeCount;
                        displayLocations.push(currentStoreLocation);

                        // This is the html that appears when a push pin is clicked on the map
                        var storeAddressText = '<div style="width:80%;height:100%;"><p style="background-color:gray;color:black;margin-bottom:5px;"><span style="padding-right:45px;">Store</span><span style="font-weight:bold;">Distance</span><p><p style="margin-bottom:0px;margin-top:0px;"><span style="color:black;padding-right:35px;">' + currentStoreLocation.OrgUnitName + '</span><span style="color:black;">' + currentStoreLocation.Distance + ' miles</span></p><p style="margin-bottom:0px;margin-top:0px;">' + currentStoreLocation.Street + ' </p><p style="margin-bottom:0px;margin-top:0px;">' + currentStoreLocation.City + ', ' + currentStoreLocation.State + ' ' + currentStoreLocation.Zip + '</p></div>';

                        // Plot the location to the map	
                        pin = new Microsoft.Maps.Pushpin(locationObj, { draggable: false, text: "" + storeCount + "" });

                        // Populating the Bing map push pin popup with store location data
                        pinInfoBox = new Microsoft.Maps.Infobox(locationObj, { width: 225, offset: new Microsoft.Maps.Point(0, 10), showPointer: true, visible: false, description: storeAddressText });

                        // Registering the event that fires when a pushpin on a Bing map is clicked
                        Microsoft.Maps.Events.addHandler(pin, 'click', (function (pinInfoBox) {
                            return function () {
                                pinInfoBox.setOptions({ visible: true });
                            }
                        })(pinInfoBox));

                        this.mapStoreLocator.entities.push(pin);
                        this.mapStoreLocator.entities.push(pinInfoBox);
                    }
                }
            }

            this.displayLocations(displayLocations);
            if (displayLocations.length > 0) {
                this.selectStore(displayLocations[0]);
            }
        }

        private selectStore(location: CommerceProxy.Entities.OrgUnitLocation) {
            // This function is for selecting a store when user clicks on a store.
            if (this.hasInventoryCheck() && !this.areAllReqProductsAvailableInOrgUnit(location.OrgUnitNumber)) {
                this.resetSelectedOrderShippingOptions();
                this.showError([Resources.String_113], false); // Products are not available in the selected store, Please select a different store.
                this.nextButton.addClass("msax-Grey");
            }
            else {
                this.hideError();

                if (this.nextButton.hasClass("msax-Grey")) {
                    this.nextButton.removeClass("msax-Grey");
                }

                if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                    var headerLevelDeliverySpecification: CommerceProxy.Entities.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass();
                    headerLevelDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                    headerLevelDeliverySpecification.PickUpStoreId = location.OrgUnitNumber;
                    headerLevelDeliverySpecification.DeliveryModeId = this.pickUpInStoreDeliveryModeCode;
                    headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
                    headerLevelDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = location.Country;
                    headerLevelDeliverySpecification.DeliveryAddress.ZipCode = location.Zip;
                    headerLevelDeliverySpecification.DeliveryAddress.State = location.State;
                    headerLevelDeliverySpecification.DeliveryAddress.City = location.City;
                    headerLevelDeliverySpecification.DeliveryAddress.Street = location.Street;
                    this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                }
                else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                    var currentLineDeliverySpecification: CommerceProxy.Entities.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass();
                    currentLineDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                    currentLineDeliverySpecification.PickUpStoreId = location.OrgUnitNumber;
                    currentLineDeliverySpecification.DeliveryModeId = this.pickUpInStoreDeliveryModeCode;
                    currentLineDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
                    currentLineDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = location.Country;
                    currentLineDeliverySpecification.DeliveryAddress.ZipCode = location.Zip;
                    currentLineDeliverySpecification.DeliveryAddress.State = location.State;
                    currentLineDeliverySpecification.DeliveryAddress.City = location.City;
                    currentLineDeliverySpecification.DeliveryAddress.Street = location.Street;

                    this.currentLineDeliverySpecification().DeliverySpecification = currentLineDeliverySpecification;
                    this.currentLineDeliverySpecification(this.currentLineDeliverySpecification());
                }
            }


            // This is to add background color to the selected store block.
            var _stores = this._availableStoresView.find(".msax-AvailableStore");
            var selectedChannelId = location.ChannelId;

            _stores.each(function (index, element) {
                if ($(element).hasClass("msax-Selected")) {
                    $(element).removeClass("msax-Selected");
                }

                if (selectedChannelId == parseInt($(element).attr("channelId"))) {
                    $(element).addClass("msax-Selected");
                }
            });
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

        private discountCodeOverlayClick() {
            this.dialogOverlay = $('.ui-widget-overlay');
            this.dialogOverlay.on('click', $.proxy(this.closeDiscountCodeDialog, this));
        }

        private createDiscountCodeDialog() {
            // Creates the discount code dialog box.
            this._addDiscountCodeDialog.dialog({
                modal: true,
                title: Resources.String_188, // Add discount code
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

        private showDiscountCodeDialog() {
            // Specify the close event handler for the dialog.
            $('.ui-dialog-titlebar-close').on('click', $.proxy(this.closeDiscountCodeDialog, this));

            // Displays the discount code dialog.
            this._addDiscountCodeDialog.dialog('open');
            this.discountCodeOverlayClick();
        }

        private closeDiscountCodeDialog() {
            // Close the dialog.
            this._addDiscountCodeDialog.dialog('close');
        }

        private itemDeliveryPreferenceSelectionOverlayClick() {
            this.dialogOverlay = $('.ui-widget-overlay');
            this.dialogOverlay.on('click', $.proxy(this.closeItemDeliveryPreferenceSelection, this));
        }

        private createItemDeliveryPreferenceDialog() {
            this._itemLevelDeliveryPreferenceSelection.dialog({
                modal: true,
                autoOpen: false,
                draggable: true,
                resizable: false,
                closeOnEscape: true,
                show: { effect: "fadeIn", duration: 500 },
                hide: { effect: "fadeOut", duration: 500 },
                width: 980,
                height: 700,
                dialogClass: 'msax-Control msax-NoTitle'
            });
        }

        private getApplicableDeliveryPreferencesForCartLine(cartLine: CommerceProxy.Entities.CartLine): { Value: CommerceProxy.Entities.DeliveryPreferenceType; Text: string }[] {
            var lineLevelDeliveryPreferences: { Value: CommerceProxy.Entities.DeliveryPreferenceType; Text: string }[] = [];

            lineLevelDeliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Resources.String_159 }); // Please select a delivery preference...

            if (!Utils.isNullOrUndefined(this.cartDeliveryPreferences) && !Utils.isNullOrUndefined(this.cartDeliveryPreferences.CartLineDeliveryPreferences)) {

                var deliveryPreferencesForAllLines = this.cartDeliveryPreferences.CartLineDeliveryPreferences;
                for (var i = 0; i < deliveryPreferencesForAllLines.length; i++) {

                    if (deliveryPreferencesForAllLines[i].LineId == cartLine.LineId) {

                        for (var j = 0; j < deliveryPreferencesForAllLines[i].DeliveryPreferenceTypeValues.length; j++) {

                            var preferenceText: string = "";
                            var currentDeliveryPreference = deliveryPreferencesForAllLines[i].DeliveryPreferenceTypeValues[j];

                            switch (currentDeliveryPreference) {
                                case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress: preferenceText = Resources.String_99; // Ship items
                                    break;
                                case CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore: preferenceText = Resources.String_100; // Pick up in store
                                    break;
                                case CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery: preferenceText = Resources.String_58; // Email
                                    break;
                                default: throw "Not supported delivery preference type.";
                            }

                            lineLevelDeliveryPreferences.push({ Value: currentDeliveryPreference, Text: preferenceText });
                        }
                    }
                }
            }

            return lineLevelDeliveryPreferences;
        }

        private showLineLevelDeliveryPreferenceSelection(cartLine: CommerceProxy.Entities.CartLine) {
            var temp: CommerceProxy.Entities.LineDeliverySpecification = new CommerceProxy.Entities.LineDeliverySpecificationClass(null);
            temp.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
            temp.DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
            temp.DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
            //temp.isDeliveryAvailable = parent.DeliverySpecification.isDeliveryAvailable;

            // If item level shipping option is set already retain it, else discard all previous order level settings.
            if (!(Utils.isNullOrWhiteSpace(cartLine.DeliveryMode))) {
                // If the item has a custom address then retain it.
                if (!Utils.isNullOrUndefined(cartLine.ShippingAddress)) {
                    temp.DeliverySpecification.DeliveryAddress.Name = cartLine.ShippingAddress.Name;
                    temp.DeliverySpecification.DeliveryAddress.Street = cartLine.ShippingAddress.Street;
                    temp.DeliverySpecification.DeliveryAddress.City = cartLine.ShippingAddress.City;
                    temp.DeliverySpecification.DeliveryAddress.State = cartLine.ShippingAddress.State;
                    temp.DeliverySpecification.DeliveryAddress.ZipCode = cartLine.ShippingAddress.ZipCode;
                    temp.DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = cartLine.ShippingAddress.ThreeLetterISORegionName;
                }

                temp.DeliverySpecification.ElectronicDeliveryEmailAddress = cartLine.ElectronicDeliveryEmail;
                temp.DeliverySpecification.ElectronicDeliveryEmailContent = cartLine.ElectronicDeliveryEmailContent;
            }

            this.currentLineDeliverySpecification(temp);
            this.currentCartLine(cartLine);

            this.hideError();
            this.errorPanel = this._itemLevelDeliveryPreferenceSelection.find(" .msax-ErrorPanel");
            this.currentLineLevelSelectedDeliveryPreference(temp.DeliverySpecification.DeliveryPreferenceTypeValue);
            this._itemLevelDeliveryPreferenceSelection.dialog('open');

            this.itemDeliveryPreferenceSelectionOverlayClick();
        }

        private static getDeliveryPreferencesForLine(lineId: string, cartDeliveryPreferences: CommerceProxy.Entities.CartDeliveryPreferences): number[] {
            //Get line preferences for current line
            var lineDeliveryPreferences: CommerceProxy.Entities.CartLineDeliveryPreference[] = cartDeliveryPreferences.CartLineDeliveryPreferences;
            for (var i = 0; i < lineDeliveryPreferences.length; i++) {
                if (lineDeliveryPreferences[i].LineId == lineId) {
                    return lineDeliveryPreferences[i].DeliveryPreferenceTypeValues;
                }
            }

            //If you reach here, then throw error.
            var msg: string = "No delivery preferences were found for line id" + lineId;
            throw new Error(msg);
        }

        private paymentCountryUpdate(srcElement) {
            // Get states when country is updated in payment page.
            if (!Utils.isNullOrUndefined(srcElement)) {
                this.paymentCardAddress().ThreeLetterISORegionName = srcElement.value;
                this.getStateProvinceInfoService(srcElement.value);
            }

            return true;
        }

        private resetOrderAvailableDeliveryMethods(srcElement) {
            if (!Utils.isNullOrUndefined(srcElement)) {
                var id = srcElement.id;
                switch (id) {
                    case "OrderAddressStreet":
                        if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Street != srcElement.value) {
                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Street = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                        }

                        break;
                    case "OrderAddressCity":
                        if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.City != srcElement.value) {
                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.City = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                        }

                        break;
                    case "OrderAddressZipCode":
                        if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ZipCode != srcElement.value) {
                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ZipCode = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                        }

                        break;
                    case "OrderAddressState":
                        if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State != srcElement.value) {
                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                        }

                        break;
                    case "OrderAddressCountry":
                        if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ThreeLetterISORegionName != srcElement.value) {
                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ThreeLetterISORegionName = srcElement.value;
                            this.getStateProvinceInfoService(srcElement.value); // Get states when country is updated.
                            this.resetSelectedOrderShippingOptions();
                        }

                        break;
                    case "OrderAddressName":
                        this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Name = srcElement.value;
                        break;
                }
            }

            return true;
        }

        private resetItemAvailableDeliveryMethods(srcElement) {
            if (!Utils.isNullOrUndefined(srcElement)) {
                var id = srcElement.id;
                switch (id) {
                    case "ItemAddressStreet":
                        if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Street != srcElement.value) {
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Street = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                        }

                        break;
                    case "ItemAddressCity":
                        if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.City != srcElement.value) {
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.City = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                        }

                        break;
                    case "ItemAddressZipCode":
                        if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ZipCode != srcElement.value) {
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ZipCode = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                        }

                        break;
                    case "ItemAddressState":
                        if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State != srcElement.value) {
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State = srcElement.value;
                            this.resetSelectedOrderShippingOptions();
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                        }

                        break;
                    case "ItemAddressCountry":
                        if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName != srcElement.value) {
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = srcElement.value;
                            this.getStateProvinceInfoService(srcElement.value); // Get states when country is updated.
                            this.resetSelectedOrderShippingOptions();
                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                        }

                        break;
                    case "ItemAddressName":
                        this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Name = srcElement.value;
                        break;
                }
            }

            return true;
        }

        private closeItemDeliveryPreferenceSelection() {
            // Restore error panel to the panel in the parent page.
            this.errorPanel = this._checkoutView.find(" > .msax-ErrorPanel");

            // Close the dialog.
            this._itemLevelDeliveryPreferenceSelection.dialog('close');
            this.cart(this.cart());
        }

        private setItemDeliveryPreferenceSelection() {

            // Set the delivery method text on the item shipping options if the shipping option is ship to address.
            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailAddress = null;
                this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailContent = null;
            }
            else if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
            }

            var latestLineDeliveryOption: CommerceProxy.Entities.LineDeliverySpecification = this.currentLineDeliverySpecification();
            var currentDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
            currentDeliveryOption.DeliveryAddress = latestLineDeliveryOption.DeliverySpecification.DeliveryAddress;
            currentDeliveryOption.DeliveryModeId = latestLineDeliveryOption.DeliverySpecification.DeliveryModeId;;
            currentDeliveryOption.DeliveryPreferenceTypeValue = latestLineDeliveryOption.DeliverySpecification.DeliveryPreferenceTypeValue;;
            currentDeliveryOption.ElectronicDeliveryEmailAddress = latestLineDeliveryOption.DeliverySpecification.ElectronicDeliveryEmailAddress;
            currentDeliveryOption.ElectronicDeliveryEmailContent = latestLineDeliveryOption.DeliverySpecification.ElectronicDeliveryEmailContent;
            currentDeliveryOption.PickUpStoreId = latestLineDeliveryOption.DeliverySpecification.PickUpStoreId;

            this.selectedDeliveryOptionByLineIdMap[latestLineDeliveryOption.LineId] = currentDeliveryOption;
            //this.currentCartLine().DeliverySpecification = currentDeliveryOption;

            // Close the dialog.
            this.closeItemDeliveryPreferenceSelection();
        }

        private findLocationKeyPress(data, event) {
            if (event.keyCode == 8 /* backspace */ || event.keyCode == 27 /* esc */) {
                event.preventDefault();
                return false;
            }
            else if (event.keyCode == 13 /* enter */) {
                this.getNearbyStoresWithAvailability();
                return false;
            }

            return true;
        }

        // html5 validation validates the hidden elements also. 
        // In order to avoid this remove the validation 'required' attribute from the elements when we hide them
        private removeValidation(element) {
            $(element).find(":input").each(function (idx, element) {
                $(element).removeAttr('required');
            });
        }

        private addValidation(element) {
            $(element).find(":input").each(function (idx, element) {
                $(element).attr('required', true);
            });
        }

        private updateCustomLoyaltyValidation() {
            if (this._paymentView.find('#CustomLoyaltyRadio').is(':checked')) {
                this.addValidation(this._paymentView.find('#LoyaltyCustomCard'));
            }

            return true;
        }

        private checkForGiftCardInCart(cart: CommerceProxy.Entities.Cart) {
            var isGiftCardPresent: boolean = false; // Is true if there is atleast one gift card in the cart. 
            var cartLines: CommerceProxy.Entities.CartLine[] = cart.CartLines;
            for (var i = 0; i < cartLines.length; i++) {
                if (cartLines[i].ItemId == this.giftCardItemId) {
                    isGiftCardPresent = true;
                }
            }

            return isGiftCardPresent;
        }

        private updateHeaderLevelDeliveryPreferences(deliveryPreferenceTypeValues: number[]) {

            var headerLevelDeliveryPreferenceTypes = this.cartDeliveryPreferences.HeaderDeliveryPreferenceTypeValues;
            var hasShipToAddress: boolean = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress);
            var hasPickUpInStore: boolean = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore);
            var hasEmail: boolean = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery);
            var hasMultiDeliveryPreference: boolean = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually);

            // Updates the avialable delivery preferences at order level.
            var deliveryPreferences: { Value: CommerceProxy.Entities.DeliveryPreferenceType; Text: string }[] = [];

            deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Resources.String_159 }); // Please select a delivery preference...

            // If the channel has ship items option add it to available delivery preferences. 
            if (hasShipToAddress) {
                deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress, Text: Resources.String_99 }); // Ship items
            }

            // If the channel has pick up in store option and cart does not have any gift card add it to available delivery preferences. 
            if (hasPickUpInStore) {
                deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore, Text: Resources.String_100 }); // Pick up in store
            }

            // If the channel has email option and cart has only gift cards add it to available delivery preferences. 
            if (hasEmail) {
                deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery, Text: Resources.String_58 }); // Email
            }

            // If the channel has multi item delivery option and cart has more than one item add it to available delivery preferences. 
            if (hasMultiDeliveryPreference) {
                deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually, Text: Resources.String_101 }); // Select delivery options by item
            }

            this.allowedHeaderLevelDeliveryPreferences(deliveryPreferences);
        }

        private showPaymentPanel(data, valueAccessor) {
            var srcElement = valueAccessor.target;

            if (!Utils.isNullOrUndefined(srcElement)) {
                if ($(srcElement).hasClass('msax-PayCreditCardLink')) {
                    this.payCreditCard(true);

                    // Get the card payment accept url on add credit card.
                    this.getCardPaymentAcceptUrl();
                }
                else if ($(srcElement).hasClass('msax-PayGiftCardLink')) {
                    this._giftCardPanel.show();
                    this.addValidation(this._giftCardPanel);
                    this.payGiftCard(true);
                }
                else if ($(srcElement).hasClass('msax-PayLoyaltyCardLink')) {
                    this._loyaltyCardPanel.show();
                    this.addValidation(this._loyaltyCardPanel);
                    if (this.isAuthenticated()) {
                        this.removeValidation(this._paymentView.find('#LoyaltyCustomCard'));
                    }

                    this.payLoyaltyCard(true);
                }

                $(srcElement).hide();
                this.updatePaymentTotal();
            }
        }

        private hidePaymentPanel(data, valueAccessor) {
            var srcElement = valueAccessor.target;

            if (!Utils.isNullOrUndefined(srcElement)) {
                if ($(srcElement.parentElement).hasClass('msax-CreditCardDetails')) {
                    this._creditCardPanel.hide();
                    this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                    this.removeValidation(this._creditCardPanel);
                    this.payCreditCard(false);

                    // Clear the tokenized cart tender line when user removes the credit card.
                    this.tokenizedCartTenderLine = null;
                }
                else if ($(srcElement.parentElement).hasClass('msax-GiftCard')) {
                    this._giftCardPanel.hide();
                    this._paymentView.find('.msax-PayGiftCard .msax-PayGiftCardLink').show();
                    this.removeValidation(this._giftCardPanel);
                    this.payGiftCard(false);
                }
                else if ($(srcElement.parentElement).hasClass('msax-LoyaltyCard')) {
                    this._loyaltyCardPanel.hide();
                    this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                    this.removeValidation(this._loyaltyCardPanel);
                    this.payLoyaltyCard(false);
                }

                this.updatePaymentTotal();
            }
        }

        private updatePaymentTotal() {
            this.creditCardAmount = 0;
            this.giftCardAmount = 0;
            this.loyaltyCardAmount = 0;

            if (this.payGiftCard()) {
                this.giftCardAmount = Utils.parseNumberFromLocaleString(this.formattedGiftCardAmount());
                this.formattedGiftCardAmount(this.formatCurrencyString(this.giftCardAmount));
            }

            if (this.payLoyaltyCard()) {
                this.loyaltyCardAmount = Utils.parseNumberFromLocaleString(this.formattedLoyaltyCardAmount());
                this.formattedLoyaltyCardAmount(this.formatCurrencyString(this.loyaltyCardAmount));
            }

            if (this.payCreditCard()) {
                this.creditCardAmount = Utils.roundToNDigits(this.cart().TotalAmount - this.giftCardAmount - this.loyaltyCardAmount, 3);
                if (isNaN(this.creditCardAmount) || (this.creditCardAmount < 0)) {
                    this.creditCardAmount = 0;
                }

                this.formattedCreditCardAmount(this.formatCurrencyString(this.creditCardAmount));
            }

            this.totalAmount = Number(this.creditCardAmount + this.giftCardAmount + this.loyaltyCardAmount);
            if (isNaN(this.totalAmount)) {
                this.totalAmount = 0;
            }

            this.formattedPaymentTotal(this.formatCurrencyString(this.totalAmount));

            return true;
        }

        private validatePayments() {
            this.updatePaymentTotal();

            if (!this.payCreditCard() && !this.payGiftCard() && !this.payLoyaltyCard()) {
                this.closeDialogAndDisplayError([Resources.String_139], false); // Please select payment method
                return;
            }

            if (!this.isCardPaymentAcceptPage() && this.payCreditCard()) {
                var selectedYear = this.paymentCard().ExpirationYear;
                var selectedMonth = this.paymentCard().ExpirationMonth;
                var currentTime = new Date();
                var currentMonth = currentTime.getMonth() + 1; // Get month returns values between 0 - 12.
                var currentYear = currentTime.getFullYear();
                if (selectedYear < currentYear || selectedYear == currentYear && selectedMonth < currentMonth) {
                    this.closeDialogAndDisplayError([Resources.String_140], false); // The expiration date is not valid. Please select valid expiration month and year and then try again
                    return;
                }
            }

            if (this.payLoyaltyCard()) {
                if (this.loyaltyCardAmount == 0) {
                    this.closeDialogAndDisplayError([Resources.String_152], false); // Loyalty card payment amount cannot be zero
                    return;
                }

                if (this.loyaltyCardAmount > this.cart().TotalAmount) {
                    this.closeDialogAndDisplayError([Resources.String_153], false); // Loyalty card payment amount is more than order total
                    return;
                }
            }

            if (this.payGiftCard()) {
                if (Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                    this.closeDialogAndDisplayError([Resources.String_144], false); // Please enter a gift card number
                    return;
                }

                if (this.giftCardAmount == 0) {
                    this.closeDialogAndDisplayError([Resources.String_146], false); // Gift card payment amount cannot be zero
                    return;
                }

                if (this.giftCardAmount > this.cart().TotalAmount) {
                    this.closeDialogAndDisplayError([Resources.String_147], false); // Gift card payment amount is more than order total
                    return;
                }

                this.checkGiftCardAmountValidity = true;
                // Get gift card balance
                this.getGiftCardBalance();
            }
            else {
                this.createPaymentCardTenderLine();
            }
        }

        private createPaymentCardTenderLine() {
            this.paymentCard(this.paymentCard()); // This is required to trigger computed values and update UI.

            if (this.totalAmount != this.cart().TotalAmount) {
                this.closeDialogAndDisplayError([Resources.String_149], false); // Payment amount is different from the order total
                return;
            }

            if (this.payCreditCard()) {
                if (this.isCardPaymentAcceptPage()) {
                    if (Utils.isNullOrUndefined(this.tokenizedCartTenderLine)) {
                        this.submitCardPaymentAcceptPayment(this.creditCardAmount, this.cardPaymentAcceptMessageOrigin, this.cardPaymentAcceptPageSubmitUrl);
                    } else {
                        this.tokenizedCartTenderLine.Amount = this.creditCardAmount;
                        this.tenderLines.push(this.tokenizedCartTenderLine);
                    }
                }
                else {
                    var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                    tenderLine.Currency = this.channelCurrencyCode;
                    tenderLine.Amount = this.creditCardAmount;
                    tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayCard);
                    tenderLine.PaymentCard = new CommerceProxy.Entities.PaymentCardClass(this.paymentCard());
                    this.formattedCreditCardAmount(this.formatCurrencyString(this.creditCardAmount));

                    tenderLine.PaymentCard.Address1 = this.paymentCardAddress().Street;
                    tenderLine.PaymentCard.City = this.paymentCardAddress().City;
                    tenderLine.PaymentCard.State = this.paymentCardAddress().State;
                    tenderLine.PaymentCard.Zip = this.paymentCardAddress().ZipCode;
                    tenderLine.PaymentCard.Country = this.paymentCardAddress().ThreeLetterISORegionName;

                    this.tenderLines.push(tenderLine);
                }
            }

            if (this.payLoyaltyCard()) {
                var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);

                if (!this.isAuthenticated() || (this.loyaltyCards().length == 0 || this._paymentView.find('#CustomLoyaltyRadio').is(':checked'))) {
                    this.loyaltyCardNumber(this._paymentView.find('#CustomLoyaltyCardNumber').val());
                }

                tenderLine.LoyaltyCardId = this.loyaltyCardNumber();
                tenderLine.Currency = this.channelCurrencyCode;
                tenderLine.Amount = this.loyaltyCardAmount;
                tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayLoyalty);
                this.formattedLoyaltyCardAmount(this.formatCurrencyString(this.loyaltyCardAmount));
                this.tenderLines.push(tenderLine);
            }

            if (this.payGiftCard()) {
                var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                tenderLine.Currency = this.channelCurrencyCode;
                tenderLine.Amount = this.giftCardAmount;
                tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayGiftCertificate);
                tenderLine.GiftCardId = this.giftCardNumber();
                this.formattedGiftCardAmount(this.formatCurrencyString(this.giftCardAmount));
                this.tenderLines.push(tenderLine);
            }

            // If paying by credit card: if card payment accept page is not used or if tokenized cart tender line is already available then show review section.
            // Otherwise it will be shown after obtaining the tokenized cart tender line.
            if (!this.payCreditCard() || !this.isCardPaymentAcceptPage() || !Utils.isNullOrUndefined(this.tokenizedCartTenderLine)) {
                // Closing the loading dialog when we navigate away from the payment section to review section.
                LoadingOverlay.CloseLoadingDialog();
                this.showCheckoutFragment(this._checkoutFragments.Review);
            }
        }

        private updateCheckoutCart(event, data: CommerceProxy.Entities.Cart) {
            // Handles the UpdateCheckoutCart event.
            CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Checkout, this.displayPromotionBanner())
                .done((cart) => {
                    this.currencyStringTemplate = Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                    this.cart(data);
                    this.productNameByItemVariantIdMap = this.createProductNameByItemVariantMap(this.cart().CartLines);

                    // Update payments when cart is updated in the review section.
                    if (this._checkoutView.find(" ." + this._checkoutFragments.Review).is(":visible")) {
                        this.updatePayments();
                    }
                });
        }

        private static isRequestedDeliveryPreferenceApplicable(deliveryPreferenceTypeValues: number[], reqDeliveryPreferenceType: CommerceProxy.Entities.DeliveryPreferenceType): boolean {
            for (var i = 0; i < deliveryPreferenceTypeValues.length; i++) {
                if (deliveryPreferenceTypeValues[i] == reqDeliveryPreferenceType) {
                    return true;
                }
            }

            return false;
        }

        /**
         * Submits payment to the external card payment accept page.
         * @param {number} paymentAmount The payment amount.
         * @param {string} messageOrigin The card payment accept messaging origin.
         * @param {string} submitURL The card payment accept submit URL.
         */
        private submitCardPaymentAcceptPayment(paymentAmount: number, messageOrigin: string, submitURL: string): void {

            if (!Utils.isNullOrWhiteSpace(submitURL)) {
                // If present, use submit URL to trigger submit
                var d = new Date();
                submitURL = submitURL + "#" + d.getTime();
                this.cardPaymentAcceptPageUrl(submitURL);
                LoadingOverlay.CloseLoadingDialog();
            }
            else if (!Utils.isNullOrWhiteSpace(messageOrigin)) {
                // Send a message to the card page to trigger submit
                var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                var cardPaymentAcceptMessage: CardPaymentAcceptMessage = {
                    type: Checkout.CARDPAYMENTACCEPTPAGESUBMIT,
                    value: "true"
                }

                cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), messageOrigin);
            }
        }

        /**
         * Sets the card payment accept card type.
         *
         * @param {Commerce.Model.Entities.CardTypeInfo[]} filteredCreditCardTypes The filtered list of credit card types.
         */
        private setCardPaymentAcceptCardType(filteredCreditCardTypes: CommerceProxy.Entities.CardTypeInfo[]): boolean {
            if (filteredCreditCardTypes.length === 0) {
                this.errorMessages([Resources.String_309]); // The specified card type is not supported.
                this.showError(this.errorMessages(), true);
                return false;
            } else {
                this.cardPaymentAcceptCardType = filteredCreditCardTypes[0].TypeId;
                return true;
            }
        }

        /**
         * Filter the credit card type based on card prefix.
         *
         * @param {string) cardPrefix The card number prefix.
         * @return {Commerce.Model.Entities.CardTypeInfo[]} The result which returns a list of credit card types that are applicable for the card prefix.
         */
        private filterCreditCardTypes(cardPrefix: string): CommerceProxy.Entities.CardTypeInfo[] {
            var filteredCardTypes: CommerceProxy.Entities.CardTypeInfo[] = [];
            
            for (var i = 0; i < this.cardTypes.length; i++) {
                var cardType = this.cardTypes[i];

                // Check that the card type is credit card
                if (cardType.CardTypeValue !== CommerceProxy.Entities.CardType.InternationalCreditCard &&
                    cardType.CardTypeValue !== CommerceProxy.Entities.CardType.CorporateCard) {
                    continue;
                }

                if (this.isAssociatedCardType(cardType, cardPrefix)) {
                    filteredCardTypes.push(cardType);
                }
            }

            return filteredCardTypes;
        }

        /**
         * Returns the boolean value indicating whether the card number is associated with the given card type.
         *
         * @param {CommerceProxy.Entities.CardTypeInfo} cardType The card type.
         * @param {string} cardNumber The card number.
         * @return True: If the card number is associated with the given card type; False: Otherwise.
         */
        private isAssociatedCardType(cardType: CommerceProxy.Entities.CardTypeInfo, cardNumber: string): boolean {
            if (cardNumber) {
                var maskNumFrom: number = parseInt(cardType.NumberFrom);
                var maskNumTo: number = parseInt(cardType.NumberTo);
                var maskLength: number = cardType.NumberFrom.length;
                var cardSubStr: number;

                cardSubStr = (cardNumber.length > maskLength) ? parseInt(cardNumber.substr(0, maskLength)) : parseInt(cardNumber);
                if ((maskNumFrom <= cardSubStr) && (cardSubStr <= maskNumTo)) {
                    return true;
                }
            }

            return false;
        }

        /**
         * Handles messages from the card payment accept page.
         * @param {any} [eventInfo] The event information.
         */
        private cardPaymentAcceptMessageHandler(eventInfo?: any): void {
            // Validate origin
            if (!(this.cardPaymentAcceptMessageOrigin.indexOf(eventInfo.origin) === 0)) {
                return;
            }

            // Parse messages
            var message = eventInfo.data;
            if (typeof (message) === "string" && message.length > 0) {

                // Handle various messages from the card payment accept page
                var messageObject = JSON.parse(message);
                switch (messageObject.type) {
                    case Checkout.CARDPAYMENTACCEPTPAGEHEIGHT:
						LoadingOverlay.CloseLoadingDialog(); 
                        var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                        cardPaymentAcceptIframe.height = messageObject.value;
                        break;
                    case Checkout.CARDPAYMENTACCEPTCARDPREFIX:
                        this.cardPaymentAcceptCardPrefix = messageObject.value;
                        break;
                    case Checkout.CARDPAYMENTACCEPTPAGEERROR:
                        // Handle retrieve card payment accept result failure.

                        var paymentErrors = messageObject.value;
                        var errors: CommerceProxy.ProxyError[] = [];
                        for (var i = 0; i < paymentErrors.length; i++) {
                            errors.push(new CommerceProxy.ProxyError(paymentErrors[i].Code.toString(), paymentErrors[i].Message));
                        }

                        this.closeDialogAndDisplayError(PaymentErrorHelper.ConvertToClientError(errors), true);
                        break;
                    case Checkout.CARDPAYMENTACCEPTPAGERESULT:
                        // Submit the order
                        var cardPaymentResultAccessCode: string = messageObject.value;
                        this.retrieveCardPaymentAcceptResult(cardPaymentResultAccessCode);
                        break;
                    default:
                    // Ignore all other messages.
                }
            }
        }

        /**
         * Register event to receive and handle message from card payment accept page.
         */
        private addCardPaymentAcceptListener(): void {
            window.addEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
        }

        /**
         * Unregister event that receives and handles message from card payment accept page.
         */
        public removeCardPaymentAcceptListener(): void {
            window.removeEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
        }

        /**
         * Handles get card payment accept url failure.
         * @param {CommerceProxy.ProxyError[]} errors The errors
         */
        private handleGetCardPaymentAcceptUrlFailure(errors?: CommerceProxy.ProxyError[]): void {
            if (!Utils.isNullOrUndefined(errors) && Utils.hasElements(errors)) {
                this.closeDialogAndDisplayError(PaymentErrorHelper.ConvertToClientError(errors), true);
            } else {
                this.closeDialogAndDisplayError([Resources.String_211], true); // Sorry, something went wrong. We were unable to obtain the card payment accept page url. Please refresh the page and try again.
            }

            this.payCreditCard(false);
            this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
        }

        /* Service calls */

        private getShoppingCart() {
            CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
            LoadingOverlay.ShowLoadingDialog();
            CartWebApi.GetCart(CommerceProxy.Entities.CartType.Checkout, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Resources.String_63); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private commenceCheckout() {
            CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutStarted();
            CartWebApi.CommenceCheckout(this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                        this.showCheckoutFragment(this._checkoutFragments.DeliveryPreferences);
                        this.initEntitySetCallSuccessful(InitEntitySet.CheckoutCart);
                        this.getChannelConfigurationAndTenderTypes(cart);
                        this.getDeliveryPreferences();
                        this.getCardTypes();
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.CheckoutCart, [new CommerceProxy.ProxyError(null, null, Resources.String_63)]); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    }

                    this.createEditRewardCardDialog();
                    this.createDiscountCodeDialog();

                    CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutError, errors, Resources.String_63); // Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.CheckoutCart, errors);
                });
        }

        private getAllDeliveryOptionDescriptions() {
            CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsStarted();
            OrgUnitWebApi.GetDeliveryOptionsInfo(this)
                .done((deliveryOptions: CommerceProxy.Entities.DeliveryOption[]) => {
                    if (Utils.hasElements(deliveryOptions)) {
                        this.allDeliveryOptionDescriptions = deliveryOptions;
                        this.initEntitySetCallSuccessful(InitEntitySet.DeliveryDescriptions);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.DeliveryDescriptions, [new CommerceProxy.ProxyError(null, null, Resources.String_160)]); // Sorry, something went wrong. An error occurred while trying to get delivery methods information. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsError, errors, Resources.String_160); // Sorry, something went wrong. An error occurred while trying to get delivery methods information. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.DeliveryDescriptions, errors);
                });;
        }

        private getChannelConfigurationAndTenderTypes(cart: CommerceProxy.Entities.Cart) {
            CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationStarted();
            OrgUnitWebApi.GetChannelConfiguration(this)
                .done((channelConfiguration: CommerceProxy.Entities.ChannelConfiguration) => {
                    if (!Utils.isNullOrUndefined(channelConfiguration)) {
                        this.bingMapsToken = channelConfiguration.BingMapsApiKey;
                        this.pickUpInStoreDeliveryModeCode = channelConfiguration.PickupDeliveryModeCode;
                        this.emailDeliveryModeCode = channelConfiguration.EmailDeliveryModeCode;
                        this.giftCardItemId = channelConfiguration.GiftCardItemId;
                        this.channelCurrencyCode = channelConfiguration.Currency;

                        this.initEntitySetCallSuccessful(InitEntitySet.ChannelConfigurations);
                        this.getCountryRegionInfo(channelConfiguration.DefaultLanguageId);
                        this.getTenderTypes(cart);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.ChannelConfigurations, [new CommerceProxy.ProxyError(null, null, Resources.String_98)]); // Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationError, errors, Resources.String_98); // Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.ChannelConfigurations, errors);
                });
        }

        private getDeliveryPreferences() {
            CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesStarted();
            CartWebApi.GetDeliveryPreferences(this)
                .done((data: CommerceProxy.Entities.CartDeliveryPreferences) => {
                    if (!Utils.isNullOrUndefined(data)) {
                        this.cartDeliveryPreferences = data;
                        this.updateHeaderLevelDeliveryPreferences(this.cartDeliveryPreferences.HeaderDeliveryPreferenceTypeValues);
                        this.createItemDeliveryPreferenceDialog();

                        this.initEntitySetCallSuccessful(InitEntitySet.DeliveryPreferences);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.DeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Resources.String_98)]); // Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesError, errors, Resources.String_98); // Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.DeliveryPreferences, errors);
                });
        }

        private getCardTypes() {
            CommerceProxy.RetailLogger.channelServiceGetCardTypesStarted();
            OrgUnitWebApi.GetCardTypes(this)
                .done((data: CommerceProxy.Entities.CardTypeInfo[]) => {
                    if (!Utils.isNullOrUndefined(data)) {
                        this.cardTypes = data;

                        this.initEntitySetCallSuccessful(InitEntitySet.CardTypes);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.CardTypes, [new CommerceProxy.ProxyError(null, null, Resources.String_68)]); // Sorry, something went wrong. The card types information was not retrieved successfully. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.channelServiceGetCardTypesFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetCardTypesError, errors, Resources.String_68); // Sorry, something went wrong. The card types information was not retrieved successfully. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.CardTypes, errors);
                });
        }

        private removeFromCartClick(cartLine: CommerceProxy.Entities.CartLine) {
            CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...
            CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Checkout, [cartLine.LineId], this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                        this.getDeliveryPreferences()
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
            CartWebApi.UpdateQuantity(CommerceProxy.Entities.CartType.Checkout, cartLines, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
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
            var discountCode = this._addDiscountCodeDialog.find('#DiscountCodeTextBox').val();
            if (!Utils.isNullOrWhiteSpace(discountCode)) {
                CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Checkout, discountCode, true /*isAdd*/, this)
                    .done((cart: CommerceProxy.Entities.Cart) => {
                        if (!Utils.isNullOrUndefined(cart)) {
                            CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                            this.closeDiscountCodeDialog();
                        } else {
                            this.showError([Resources.String_93], true); // Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.
                        }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeError, errors, Resources.String_93);  // Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.
                        this.closeDiscountCodeDialog();
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
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

                CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Checkout, promoCode, false, this)
                    .done((cart: CommerceProxy.Entities.Cart) => {
                        if (!Utils.isNullOrUndefined(cart)) {
                            CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
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

        private getOrderDeliveryOptions() {
            CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsStarted();
            this.resetSelectedOrderShippingOptions();
            LoadingOverlay.ShowLoadingDialog();
            var shipToAddress: CommerceProxy.Entities.Address = this.latestHeaderLevelDeliverySpecification().DeliveryAddress;

            CartWebApi.GetOrderDeliveryOptionsForShipping(shipToAddress, this)
                .done((data: CommerceProxy.Entities.DeliveryOption[]) => {
                    if (!Utils.isNullOrUndefined(data)) {
                        this.availableDeliveryOptions(data);

                        var selectedOrderDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                        selectedOrderDeliveryOption.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                        selectedOrderDeliveryOption.DeliveryAddress = shipToAddress;
                    
                        //Make a default selection if there is only one delivery option available.
                        if (this.availableDeliveryOptions().length == 1) {
                            selectedOrderDeliveryOption.DeliveryModeId = this.availableDeliveryOptions()[0].Code;
                        }

                        this.latestHeaderLevelDeliverySpecification(selectedOrderDeliveryOption);
                        this.hideError();
                    } else {
                        this.showError([Resources.String_66], true); // Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.
                    }

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsError, errors, Resources.String_66); // Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });;
        }

        private getItemDeliveryOptions() {
            CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsStarted();
            LoadingOverlay.ShowLoadingDialog();

            var currentLineDeliveryOption: CommerceProxy.Entities.LineDeliverySpecification = this.currentLineDeliverySpecification();
            var lineShippingAddress: CommerceProxy.Entities.LineShippingAddress = new CommerceProxy.Entities.LineShippingAddressClass();
            lineShippingAddress.LineId = currentLineDeliveryOption.LineId;
            lineShippingAddress.ShippingAddress = currentLineDeliveryOption.DeliverySpecification.DeliveryAddress;
            CartWebApi.GetLineDeliveryOptionsForShipping([lineShippingAddress], this)
                .done((data: CommerceProxy.Entities.SalesLineDeliveryOption[]) => {
                    if (!Utils.isNullOrUndefined(data)) {
                        var salesLineDeliveryOptionsInResponse = data;
                        for (var i = 0; i < salesLineDeliveryOptionsInResponse.length; i++) {
                            if (salesLineDeliveryOptionsInResponse[i].SalesLineId == this.currentLineDeliverySpecification().LineId) {
                                this.availableDeliveryOptions(salesLineDeliveryOptionsInResponse[i].DeliveryOptions);
                            }
                        }
                    }
                    else {
                        this.closeDialogAndDisplayError([Resources.String_66], true); // Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.
                        return;
                    }

                    //Make a default selection if there is only one delivery option available.
                    if (this.availableDeliveryOptions().length == 1) {
                        if (Utils.isNullOrUndefined(this.currentLineDeliverySpecification().DeliverySpecification)) {
                            this.currentLineDeliverySpecification().DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                        }
                        this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = this.availableDeliveryOptions()[0].Code;
                        this.currentLineDeliverySpecification(this.currentLineDeliverySpecification()); // This shows the above update in UI.
                    }

                    this.hideError();

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsError, errors, Resources.String_66); // Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private getItemUnitFromCartLine(cartLine: CommerceProxy.Entities.CartLine): CommerceProxy.Entities.ItemUnit {
            var itemUnit: CommerceProxy.Entities.ItemUnit = new CommerceProxy.Entities.ItemUnitClass();
            itemUnit.ItemId = cartLine.ItemId;
            itemUnit.VariantInventoryDimensionId = cartLine.InventoryDimensionId;
            return itemUnit;
        }

        private getNearbyStoresWithAvailabilityService() {
            CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityStarted();
            LoadingOverlay.ShowLoadingDialog();
            var itemUnits: CommerceProxy.Entities.ItemUnit[];

            if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                var itemUnit: CommerceProxy.Entities.ItemUnit = this.getItemUnitFromCartLine(this.currentCartLine());
                itemUnits = [itemUnit];
            }
            else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                itemUnits = this.cart().CartLines.map((cl) => { return this.getItemUnitFromCartLine(cl); })
            }

            OrgUnitWebApi.GetNearbyStoresWithAvailability(this.searchLocation.latitude, this.searchLocation.longitude, 0, itemUnits, this)
                .done((data: CommerceProxy.Entities.OrgUnitAvailability[]) => {
                    if (!Utils.hasElements(data)) {
                        this.resetSelectedOrderShippingOptions();
                        this.displayLocations(null);
                        this._availableStoresView.hide();
                        this.showError([Resources.String_107], true); // There are no stores around the location you searched. Please update your delivery preferences and try again.
                    }
                    else {
                        this.orgUnitLocations = data.map((oua) => { return oua.OrgUnitLocation });
                        this.availabilityByOrgUnitMap = this.createAvailabilitiesByOrgUnitMap(data);
                        this.availabilityFlagByOrgUnitMap = this.createAvailabilityFlagByOrgUnitMap(data, itemUnits);
                        this.renderAvailableStores();
                        this.hideError();
                    }

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityError, errors, Resources.String_107); // There are no stores around the location you searched. Please update your delivery preferences and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private createProductNameByItemVariantMap(cartLines: CommerceProxy.Entities.CartLine[]): string[] {
            var productNameByItemVariantMap: string[] = [];

            if (!Utils.isNullOrUndefined(cartLines)) {
                for (var i = 0; i < cartLines.length; i++) {
                    var key: string = cartLines[i].ItemId + '|' + cartLines[i].InventoryDimensionId;
                    productNameByItemVariantMap[key] = cartLines[i][Constants.ProductNameProperty];
                }
            }

            return productNameByItemVariantMap;
        }

        private getProductName(itemId: string, inventoryDimensionId: string) {
            var key: string = itemId + '|' + inventoryDimensionId;
            var productName: string = key // Default value.
            if (this.productNameByItemVariantIdMap != null && !Utils.isNullOrUndefined(this.productNameByItemVariantIdMap[key])) {
                productName = this.productNameByItemVariantIdMap[key];
            }

            return productName;
        }

        private createAvailabilitiesByOrgUnitMap(orgUnitAvailabilities: CommerceProxy.Entities.OrgUnitAvailability[]): CommerceProxy.Entities.ItemAvailability[][] {
            var availabilitiesByOrgUnitMap: CommerceProxy.Entities.ItemAvailability[][] = [];

            if (!Utils.isNullOrUndefined(orgUnitAvailabilities)) {
                for (var i = 0; i < orgUnitAvailabilities.length; i++) {
                    var currentOrgUnitAvailablities = orgUnitAvailabilities[i];
                    availabilitiesByOrgUnitMap[currentOrgUnitAvailablities.OrgUnitLocation.OrgUnitNumber] = currentOrgUnitAvailablities.ItemAvailabilities;
                }
            }

            return availabilitiesByOrgUnitMap;
        }

        private createAvailabilityFlagByOrgUnitMap(orgUnitAvailabilities: CommerceProxy.Entities.OrgUnitAvailability[], itemUnits: CommerceProxy.Entities.ItemUnit[]): boolean[] {
            var availabilityFlagByOrgUnitMap: boolean[] = [];

            if (!Utils.isNullOrUndefined(orgUnitAvailabilities)) {
                for (var i = 0; i < orgUnitAvailabilities.length; i++) {
                    var itemAvailablities = orgUnitAvailabilities[i].ItemAvailabilities;
                    var key: string;
                    var availableQuantityIndexByItemVariantId: number[] = [];
                    for (var j = 0; j < itemAvailablities.length; j++) {
                        key = itemAvailablities[j].ItemId + '|' + itemAvailablities[j].VariantInventoryDimensionId;
                        availableQuantityIndexByItemVariantId[key] = itemAvailablities[j].AvailableQuantity;
                    }

                    var areAllItemsAvailableInCurentOrgUnit = true;
                    for (var j = 0; j < itemUnits.length; j++) {
                        key = itemUnits[j].ItemId + '|' + itemUnits[j].VariantInventoryDimensionId;
                        var tempBool = !Utils.isNullOrUndefined(availableQuantityIndexByItemVariantId[key]) && availableQuantityIndexByItemVariantId[key] > 0;
                        areAllItemsAvailableInCurentOrgUnit = areAllItemsAvailableInCurentOrgUnit && tempBool;
                    }

                    availabilityFlagByOrgUnitMap[orgUnitAvailabilities[i].OrgUnitLocation.OrgUnitNumber] = areAllItemsAvailableInCurentOrgUnit;
                }
            }

            return availabilityFlagByOrgUnitMap;
        }

        private areAllReqProductsAvailableInOrgUnit(orgUnitNumber: string) {
            var areAllSpecifiedProductsAvailable = false;

            if (!Utils.isNullOrUndefined(this.availabilityFlagByOrgUnitMap) && !Utils.isNullOrUndefined(this.availabilityFlagByOrgUnitMap[orgUnitNumber])) {
                areAllSpecifiedProductsAvailable = this.availabilityFlagByOrgUnitMap[orgUnitNumber];
            }

            return areAllSpecifiedProductsAvailable;
        }

        private getAvailabilitesforOrgUnitNumber(orgUnitNumber: string) {
            var availabilities: CommerceProxy.Entities.ItemAvailability[] = null;

            if (!Utils.isNullOrWhiteSpace(orgUnitNumber) && !Utils.isNullOrUndefined(this.availabilityByOrgUnitMap)) {
                availabilities = this.availabilityByOrgUnitMap[orgUnitNumber];
            }

            return availabilities;
        }

        private getNearbyStoresService() {
            CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresStarted();
            LoadingOverlay.ShowLoadingDialog();

            OrgUnitWebApi.GetNearbyStores(this.searchLocation.latitude, this.searchLocation.longitude, 0, this)
                .done((data: CommerceProxy.Entities.OrgUnitLocation[]) => {
                    if (!Utils.hasElements(data)) {
                        this.resetSelectedOrderShippingOptions();
                        this.displayLocations(null);
                        this._availableStoresView.hide();
                        this.showError([Resources.String_107], true); // Products are not available for pick up in the stores around the location you searched. Please update your delivery preferences and try again.
                    }
                    else {
                        this.orgUnitLocations = data;
                        this.renderAvailableStores();
                        this.hideError();
                    }

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresError, errors, Resources.String_107); // Products are not available for pick up in the stores around the location you searched. Please update your delivery preferences and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private getCountryRegionInfo(languageId: string) {
            CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoStarted();

            StoreOperationsWebApi.GetCountryRegionInfo(languageId, this)
                .done((data: CommerceProxy.Entities.CountryRegionInfo[]) => {
                    if (Utils.hasElements(data)) {
                        this.countries(this.getSortedCountries(data));
                        this.initEntitySetCallSuccessful(InitEntitySet.CountryRegion);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.CountryRegion, [new CommerceProxy.ProxyError(null, null, Resources.String_165)]); // Sorry, something went wrong. An error occurred while retrieving the country region information. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoError, errors, Resources.String_165); // Sorry, something went wrong. An error occurred while retrieving the country region information. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.CountryRegion, errors);
                });
        }

        private getSortedCountries(countryRegionInfoArray: CommerceProxy.Entities.CountryRegionInfo[]): { CountryCode: string; CountryName: string }[] {
            var sortedCountries: { CountryCode: string; CountryName: string }[] = [];

            if (!Utils.isNullOrUndefined(countryRegionInfoArray)) {
                for (var i = 0; i < countryRegionInfoArray.length; i++) {
                    sortedCountries.push({ CountryCode: countryRegionInfoArray[i].CountryRegionId, CountryName: countryRegionInfoArray[i].ShortName });
                }
            }

            sortedCountries.sort((a, b) => { return a.CountryName.localeCompare(b.CountryName); });

            return sortedCountries;
        }

        private getStateProvinceInfoService(countryCode: string) {
            CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoStarted();
            if (!Utils.isNullOrWhiteSpace(countryCode)) {
                LoadingOverlay.ShowLoadingDialog();
                StoreOperationsWebApi.GetStateProvinceInfo(countryCode, this)
                    .done((data: CommerceProxy.Entities.StateProvinceInfo[]) => {
                        this.states(data); // Initialize the states observable array with the collection returned by the service. 

                        if (this._checkoutView.find(" ." + this._checkoutFragments.DeliveryPreferences).is(":visible")) { // If in delivery preferences section
                            var tempAddress = this.tempShippingAddress();
                            if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                // If chosen order delivery preference is ship to address and a state value exists already, and the state belongs to the selected country use that, 
                                // else initialize with empty value.
                                if (!Utils.isNullOrUndefined(this.latestHeaderLevelDeliverySpecification().DeliveryAddress) &&
                                    !Utils.isNullOrUndefined(this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State) &&
                                    this.countryContainsState(data, this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State)) {
                                    tempAddress.State = this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State;
                                }
                                else {
                                    tempAddress.State = '';
                                }
                            }
                            else if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                // If chosen item delivery preference is ship to address and a state value exists already, and the state belongs to the selected country use that, 
                                // else initialize with empty value.

                                //anandjo seems like a bug
                                if (!Utils.isNullOrUndefined(this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress) &&
                                    !Utils.isNullOrUndefined(this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State) &&
                                    this.countryContainsState(data, this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State)) {
                                    tempAddress.State = this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State;
                                }
                                else {
                                    tempAddress.State = '';
                                }
                            }

                            this.tempShippingAddress(tempAddress);
                        }
                        else if (!Utils.isNullOrUndefined(this.paymentCardAddress())) {
                            var tempPaymentCardAddress = this.paymentCardAddress();
                            if (!Utils.isNullOrUndefined(this.paymentCardAddress().State) &&
                                this.countryContainsState(data, this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State)) {
                                // If in the payment section and a state value does not exist already for payment address initialize with empty value.
                                tempPaymentCardAddress.State = this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State;
                            }
                            else {
                                tempPaymentCardAddress.State = '';
                            }
                            this.paymentCardAddress(tempPaymentCardAddress);
                        }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoError, errors, Resources.String_185); // Sorry, something went wrong. An error occurred while retrieving the state/province information. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
            }
        }

        private countryContainsState(stateProvinces: CommerceProxy.Entities.StateProvinceInfo[], stateId: string) {
            for (var i = 0; i < stateProvinces.length; i++) {
                if (stateId == stateProvinces[i].StateId) {
                    return true;
                }
            }

            return false;
        }

        private isAuthenticatedSession() {
            CommerceProxy.RetailLogger.customerServiceIsAuthenticationSessionStarted();
            CustomerWebApi.IsAuthenticatedSession()
                .done((data: boolean) => {
                    this.isAuthenticated(data);

                    if (this.isAuthenticated()) {
                        this.getUserEmailAndAddresses();
                    } else {
                        // set customer entity state as success
                        this.initEntitySetCallSuccessful(InitEntitySet.Customer);
                    }

                    this.initEntitySetCallSuccessful(InitEntitySet.IsAuthSession);
                    CommerceProxy.RetailLogger.customerServiceIsAuthenticationSessionFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    this.isAuthenticated(false);
                    Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Resources.String_233); // Sorry, something went wrong. An error occurred while trying to get user login information. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.IsAuthSession, errors);
                });
        }

        private getUserEmailAndAddresses() {
            CommerceProxy.RetailLogger.customerServiceGetCustomerStarted();

            CustomerWebApi.GetCustomer(this)
                .done((data: CommerceProxy.Entities.Customer) => {
                    if (Utils.isNullOrUndefined(data)) {
                        this.initEntitySetCallFailed(InitEntitySet.Customer, [new CommerceProxy.ProxyError(null, null, Resources.String_209)]); // Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.
                    }
                    else {
                        var addresses: { Value: CommerceProxy.Entities.Address; Text: string }[] = [];

                        if (data.Addresses) {
                            for (var i = 0; i < data.Addresses.length; i++) {
                                var address = data.Addresses[i];

                                if (Utils.isNullOrWhiteSpace(address.Name) &&
                                    Utils.isNullOrWhiteSpace(address.Street) &&
                                    Utils.isNullOrWhiteSpace(address.City) &&
                                    Utils.isNullOrWhiteSpace(address.State) &&
                                    Utils.isNullOrWhiteSpace(address.ZipCode)) {

                                    // If all of the above properties are empty, do not show address in the select address drop down.
                                    continue;
                                }

                                var delimiter = Utils.isNullOrWhiteSpace(address.State) && Utils.isNullOrWhiteSpace(address.ZipCode) ? "" : ", ";
                                var addressString = Utils.format("({0}) {1} {2}{3}{4} {5}", address.Name, address.Street, address.City, delimiter, address.State, address.ZipCode);
                                addresses.push({ Value: address, Text: addressString });
                            }

                            if (addresses.length > 0) {
                                this.storedCustomerAddresses(addresses);
                            }
                        }

                        this.recepientEmailAddress = data.Email;
                        this.initEntitySetCallSuccessful(InitEntitySet.Customer);
                    }

                    CommerceProxy.RetailLogger.customerServiceGetCustomerFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Resources.String_209); // Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.Customer, errors);
                });
        }

        private setHeaderLevelDeliveryOptions(headerLevelDeliveryOption: CommerceProxy.Entities.DeliverySpecification) {
            CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsStarted();
            CartWebApi.UpdateDeliverySpecification(headerLevelDeliveryOption, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                        this.showCheckoutFragment(this._checkoutFragments.PaymentInformation);
                        this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.SetDeliveryPreferences);
                    } else {
                        this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Resources.String_67)]); // Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsError, errors, Resources.String_67); // Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.
                    this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, errors);
                });
        }

        private setLineLevelDeliveryOptions(selectedLineLevelDeliveryOptions: CommerceProxy.Entities.LineDeliverySpecification[]) {
            CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsStarted();
            CartWebApi.UpdateLineDeliverySpecifications(selectedLineLevelDeliveryOptions, this)
                .done((cart: CommerceProxy.Entities.Cart) => {
                    if (!Utils.isNullOrUndefined(cart)) {
                        CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                        this.showCheckoutFragment(this._checkoutFragments.PaymentInformation);
                        this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.SetDeliveryPreferences);
                    } else {
                        this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Resources.String_67)]); // Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsError, errors, Resources.String_67); // Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.
                    this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, errors);
                });
        }

        private getLoyaltyCards() {
            CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsStarted();

            CustomerWebApi.GetLoyaltyCards(this)
                .done((data: CommerceProxy.Entities.LoyaltyCard[]) => {
                    if (Utils.isNullOrUndefined(data)) {
                        this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.LoyaltyCards, [new CommerceProxy.ProxyError(null, null, Resources.String_150)]); // Sorry, something went wrong. An error occurred while trying to get loyalty card information. Please refresh the page and try again
                    } else {
                        var loyaltyCards: string[] = [];
                        var _customLoyaltyRadio = this._paymentView.find("#CustomLoyaltyRadio");
                        var containsValidLoyaltyCard = false;

                        for (var i = 0; i < data.length; i++) {
                            if (data[i].CardTenderTypeValue == CommerceProxy.Entities.LoyaltyCardTenderType.AsCardTender ||
                                data[i].CardTenderTypeValue == CommerceProxy.Entities.LoyaltyCardTenderType.AsContactTender) {
                                loyaltyCards.push(data[i].CardNumber);
                                containsValidLoyaltyCard = true;
                            }
                        }

                        if (!containsValidLoyaltyCard) {
                            _customLoyaltyRadio.hide();
                        }
                        else {
                            _customLoyaltyRadio.show();

                            // Selecting the first loyalty card by default
                            this.loyaltyCardNumber(loyaltyCards[0]);
                        }

                        this.loyaltyCards(loyaltyCards);
                        this.hideError();
                        this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.LoyaltyCards);
                    }

                    CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsError, errors, Resources.String_150); // Sorry, something went wrong. An error occurred while trying to get loyalty card information. Please refresh the page and try again
                    this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.LoyaltyCards, errors);
                });
        }

        private updateLoyaltyCardId() {
            CommerceProxy.RetailLogger.loyaltyServiceUpdateLoyaltyCardIdStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_179); // Updating shopping cart ...

            var loyaltyCardId = this._editRewardCardDialog.find('#RewardCardTextBox').val();

            if (!Utils.isNullOrWhiteSpace(loyaltyCardId)) {
                CartWebApi.UpdateLoyaltyCardId(CommerceProxy.Entities.CartType.Checkout, loyaltyCardId, this)
                    .done((cart: CommerceProxy.Entities.Cart) => {
                        if (!Utils.isNullOrUndefined(cart)) {
                            CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                            this.closeEditRewardCardDialog();
                        } else {
                            this.showError([Resources.String_232], true); // Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.
                        }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.loyaltyServiceUpdateLoyaltyCardIdFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdError, errors, Resources.String_232); // Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeEditRewardCardDialog();
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
            }
        }

        private getTenderTypes(cart: CommerceProxy.Entities.Cart) {
            CommerceProxy.RetailLogger.channelServiceGetTenderTypesStarted();
            OrgUnitWebApi.GetTenderTypes(this)
                .done((data: CommerceProxy.Entities.TenderType[]) => {
                    if (Utils.hasElements(data)) {
                        this.supportedTenderTypes = data;
                        this.calculateSupportedPaymentTypes(data);
                            
                        // If the channel supports credit card tender type display the credit card payment panel.
                        if (this.isCreditCardPaymentAllowed) {
                            this._paymentView.find('.msax-PayCreditCard').show();
                            this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                            this._creditCardPanel.hide();
                        }
                        else {
                            this._paymentView.find('.msax-PayCreditCard').hide();
                        }
                            
                        // If the cart has any gift card item then payment by gift card is not allowed.
                        // If the channel supports gift card tender type display the gift card payment panel.
                        if (!this.checkForGiftCardInCart(cart) && this.isGiftCardPaymentAllowed) {
                            this._paymentView.find('.msax-PayGiftCard').show();
                            this._giftCardPanel.hide();
                            this._paymentView.find('.msax-PayGiftCard .msax-PayGiftCardLink').show();
                        }
                        else {
                            this._paymentView.find('.msax-PayGiftCard').hide();
                        }
                            
                        // If the channel supports loyalty card tender type display the loyalty card payment panel.
                        if (this.isLoyaltyCardPaymentAllowed) {
                            this._paymentView.find('.msax-PayLoyaltyCard').show();
                            this._loyaltyCardPanel.hide();
                            this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                        }
                        else {
                            this._paymentView.find('.msax-PayLoyaltyCard').hide();
                        }

                        this.removeValidation(this._creditCardPanel);
                        this.removeValidation(this._giftCardPanel);
                        this.removeValidation(this._loyaltyCardPanel);
                        this.initEntitySetCallSuccessful(InitEntitySet.TenderTypes);
                    } else {
                        this.initEntitySetCallFailed(InitEntitySet.TenderTypes, [new CommerceProxy.ProxyError(null, null, Resources.String_138)]); // Sorry, something went wrong. An error occurred while trying to get payment methods supported by the store. Please refresh the page and try again.
                    }

                    CommerceProxy.RetailLogger.channelServiceGetTenderTypesFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetTenderTypesError, errors, Resources.String_138); // Sorry, something went wrong. An error occurred while trying to get payment methods supported by the store. Please refresh the page and try again.
                    this.initEntitySetCallFailed(InitEntitySet.TenderTypes, errors);
                });
        }

        /**
         * Invokes get card payment accept url service and handles response from the service.
         */
        private getCardPaymentAcceptUrl(): void {
            CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlStarted();
            LoadingOverlay.ShowLoadingDialog();

            var hostPageOrigin: string = window.location.protocol + "//" + window.location.host;
            var adaptorPath: string = hostPageOrigin + "/Connectors/";
            var cardPaymentAcceptSettings: CommerceProxy.Entities.CardPaymentAcceptSettings = {
                HostPageOrigin: hostPageOrigin,
                AdaptorPath: adaptorPath,
                CardPaymentEnabled: false,
                CardTokenizationEnabled: true
            };

            CartWebApi.GetCardPaymentAcceptPoint(cardPaymentAcceptSettings, this)
                .done((data: CommerceProxy.Entities.CardPaymentAcceptPoint) => {
                    if (!Utils.isNullOrUndefined(data)
                        && !Utils.isNullOrWhiteSpace(data.AcceptPageUrl)
                        && !Utils.isNullOrWhiteSpace(data.MessageOrigin)) {
                        var cardPaymentAcceptUrl: string = data.AcceptPageUrl;
						
                        this.cardPaymentAcceptPageUrl(cardPaymentAcceptUrl);
                        this.cardPaymentAcceptPageSubmitUrl = data.AcceptPageSubmitUrl;
                        this.cardPaymentAcceptMessageOrigin = data.MessageOrigin;
                        this.isCardPaymentAcceptPage(true);

                        // If card payment accept page is being used, add a listener for the messages from the page.
                        this.removeCardPaymentAcceptListener(); // We remove listener first so that we don't have multiple listeners receiving the message in case a listener was added earlier.
                        this.addCardPaymentAcceptListener();
                        this._creditCardPanel.show();

                        // When the opening URL has a different origin from the messaging origin,
                        // The accepting page probably won't send its height. Set iframe height to a hardcode value to fit the page.
                        // Different payment accepting pages need different height values when they don't support height message.
                        if (!(cardPaymentAcceptUrl.indexOf(data.MessageOrigin) === 0)) {
                            var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                            cardPaymentAcceptIframe.height = "600px";
                        }

                        this.updatePaymentTotal();
                        this.hideError();
                    } else {
                        this.handleGetCardPaymentAcceptUrlFailure();
                    }

                    CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlError, errors, Resources.String_211); // Sorry, something went wrong. We were unable to obtain the card payment accept page url. Please refresh the page and try again.
                    this.handleGetCardPaymentAcceptUrlFailure(errors);
                });
        }

        /**
         * Invokes retrieve card payment accept result service and handles response from the service.
         * @param {string} cardPaymentResultAccessCode The card payment result access code.
         */
        private retrieveCardPaymentAcceptResult(cardPaymentResultAccessCode: string): void {
            CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultStarted();

            StoreOperationsWebApi.RetrieveCardPaymentAcceptResult(cardPaymentResultAccessCode, this)
                .done((cardPaymentAcceptResult: CommerceProxy.Entities.CardPaymentAcceptResult) => {
                    if (!Utils.isNullOrUndefined(cardPaymentAcceptResult) && !Utils.isNullOrUndefined(cardPaymentAcceptResult.TokenizedPaymentCard)) {

                        if (Utils.isNullOrUndefined(this.cardPaymentAcceptCardPrefix)) {
                            // This happens when the accepting page did not send the card prefix.
                            // Try to get the card prefix from the tokenized payment card
                            this.cardPaymentAcceptCardPrefix = cardPaymentAcceptResult.TokenizedPaymentCard.CardTokenInfo.MaskedCardNumber;
                        }

                        var types: CommerceProxy.Entities.CardTypeInfo[] = this.filterCreditCardTypes(this.cardPaymentAcceptCardPrefix);
                        if (this.setCardPaymentAcceptCardType(types)) {
                            cardPaymentAcceptResult.TokenizedPaymentCard.CardTypeId = this.cardPaymentAcceptCardType;
                            var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                            tenderLine.Currency = this.channelCurrencyCode;
                            tenderLine.Amount = this.creditCardAmount;
                            tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayCard);
                            tenderLine.TokenizedPaymentCard = cardPaymentAcceptResult.TokenizedPaymentCard;
                            tenderLine.CardTypeId = this.cardPaymentAcceptCardType;
                            this.formattedCreditCardAmount(this.formatCurrencyString(this.creditCardAmount));

                            //Extract billing address
                            var tokenizedPaymentCardAddress: CommerceProxy.Entities.Address = new CommerceProxy.Entities.AddressClass();
                            tokenizedPaymentCardAddress.Street = tenderLine.TokenizedPaymentCard.Address1;
                            tokenizedPaymentCardAddress.City = tenderLine.TokenizedPaymentCard.City;
                            tokenizedPaymentCardAddress.State = tenderLine.TokenizedPaymentCard.State;
                            tokenizedPaymentCardAddress.ZipCode = tenderLine.TokenizedPaymentCard.Zip;
                            tokenizedPaymentCardAddress.ThreeLetterISORegionName = tenderLine.TokenizedPaymentCard.Country;
                            // Retain the email address that is already on the paymentCardAddress object since it is not provided as part of a tokenized card.
                            tokenizedPaymentCardAddress.Email = this.paymentCardAddress().Email;
                            this.paymentCardAddress(tokenizedPaymentCardAddress);
                    
                            // Setting the tokenized cart tender line object to prevent tokenization every time user clicks 'next' on payment page.
                            this.tokenizedCartTenderLine = tenderLine;
                            this.tenderLines.push(this.tokenizedCartTenderLine);
                            this.hideError();
                            this.showCheckoutFragment(this._checkoutFragments.Review);
                        }
                    } else {
                        // Handle retrieve card payment accept result failure.
                        this.showError([Resources.String_210], true); // Sorry, something went wrong. We were unable to obtain the card payment accept result. Please refresh the page and try again.
                    }

                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultFinished();
                }).fail((errors: CommerceProxy.ProxyError[]) => {
                    // Handle retrieve card payment accept result call failure.
                    this.closeDialogAndDisplayError(PaymentErrorHelper.ConvertToClientError(errors), true);
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultError, errors, Resources.String_210); // Sorry, something went wrong. We were unable to obtain the card payment accept result. Please refresh the page and try again.
                });
        }

        private calculateSupportedPaymentTypes(tenderTypes: CommerceProxy.Entities.TenderType[]) {
            for (var i = 0; i < tenderTypes.length; i++) {
                switch (tenderTypes[i].OperationId) {
                    case CommerceProxy.Entities.RetailOperation.PayCard: this.isCreditCardPaymentAllowed = true;
                        break;
                    case CommerceProxy.Entities.RetailOperation.PayLoyalty: this.isLoyaltyCardPaymentAllowed = true;
                        break;
                    case CommerceProxy.Entities.RetailOperation.PayGiftCertificate:
                        this.isGiftCardPaymentAllowed = this.isAuthenticated();
                        break;
                }
            }
        }

        private getTenderTypeIdForOperationId(tenderTypes: CommerceProxy.Entities.TenderType[], operationId: CommerceProxy.Entities.RetailOperation): string {
            var tenderTypeId: string = ""

            for (var i = 0; i < tenderTypes.length; i++) {
                if (tenderTypes[i].OperationId == operationId) {
                    tenderTypeId = tenderTypes[i].TenderTypeId;
                    break;
                }
            }

            return tenderTypeId;
        }

        private getGiftCardBalance() {
            CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceStarted();
            LoadingOverlay.ShowLoadingDialog();
            this._paymentView.find('.msax-GiftCardBalance').hide();
            if (!Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                StoreOperationsWebApi.GetGiftCardBalance(this.giftCardNumber(), this)
                    .done((data: CommerceProxy.Entities.GiftCard) => {
                        if (!Utils.isNullOrUndefined(data)) {
                            var giftCardInResponse: CommerceProxy.Entities.GiftCard = data;
                            if (Utils.isNullOrEmpty(giftCardInResponse.Id)) {
                                this.isGiftCardInfoAvailable(false);
                            }
                            else {

                                // If user enters amount directly into the gift card amount text box,
                                // then these validations will have to be performed on tender line creation
                                if (this.checkGiftCardAmountValidity) {

                                    if (Number(giftCardInResponse.Balance) < Number(this.giftCardAmount)) {
                                        this.closeDialogAndDisplayError([Resources.String_148], false); // Gift card does not have sufficient balance
                                    }
                                }

                                this.isGiftCardInfoAvailable(true);
                                this.giftCardBalance(giftCardInResponse.BalanceCurrencyCode + giftCardInResponse.Balance);
                            }

                            this._paymentView.find('.msax-GiftCardBalance').show();
                            this.hideError();

                            if (this.isGiftCardInfoAvailable() && this.checkGiftCardAmountValidity) {
                                this.createPaymentCardTenderLine();
                            }
                        } else {
                            this.showError([Resources.String_145], true); // Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.
                        }

                        this.checkGiftCardAmountValidity = false;

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceError, errors, Resources.String_145); // Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeDialogAndDisplayError(errorMessages, true);
                        this.checkGiftCardAmountValidity = false;
                    });
            }
            else {
                this.closeDialogAndDisplayError([Resources.String_144], false); // Please enter a gift card number
                this.checkGiftCardAmountValidity = false;
            }
        }

        private applyFullGiftCardAmount() {
            LoadingOverlay.ShowLoadingDialog();
            if (!Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceStarted();
                StoreOperationsWebApi.GetGiftCardBalance(this.giftCardNumber(), this)
                    .done((data: CommerceProxy.Entities.GiftCard) => {
                        if (!Utils.isNullOrUndefined(data)) {
                            var giftCardInResponse: CommerceProxy.Entities.GiftCard = data;
                            var totalAmount = this.cart().TotalAmount;
                            var giftCardBalance = giftCardInResponse.Balance;
                            var giftCardBalanceWithCurrency = giftCardInResponse.BalanceCurrencyCode + giftCardInResponse.Balance;
                            var _giftCardTextBox = this._paymentView.find('#GiftCardAmount');
                            if (Utils.isNullOrEmpty(giftCardInResponse.Id)) {
                                this.isGiftCardInfoAvailable(false);
                            }
                            else {
                                this.isGiftCardInfoAvailable(true);
                                this.giftCardBalance(giftCardBalance.toString());

                                if (Number(giftCardBalance) <= Number(totalAmount)) {
                                    // Using up gift card balance.
                                    _giftCardTextBox.val(giftCardBalanceWithCurrency);
                                    this.updatePaymentTotal();
                                }
                                else {
                                    // Paying full order total using gift card.
                                    _giftCardTextBox.val(totalAmount);
                                    this._creditCardPanel.hide();
                                    this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                                    this.payCreditCard(false);

                                    this._loyaltyCardPanel.hide();
                                    this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                                    this.payLoyaltyCard(false);
                                }
                            }

                            this._paymentView.find('.msax-GiftCardBalance').show();
                            this.hideError();
                        } else {
                            this.showError([Resources.String_145], true); // Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.
                        }

                        LoadingOverlay.CloseLoadingDialog();
                        CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceFinished();
                    })
                    .fail((errors: CommerceProxy.ProxyError[]) => {
                        Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceError, errors, Resources.String_145); // Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.
                        var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                        this.closeDialogAndDisplayError(errorMessages, true);
                    });
            }
            else {
                this.closeDialogAndDisplayError([Resources.String_144], false); // Please enter a gift card number
            }
        }

        private redirectOnOrderCreation(channelReferenceId: string, cleanUpErrors?: CommerceProxy.ProxyError[]) {
            //TODO vben: Need to define how to handle cleanup errors
            this.orderNumber(channelReferenceId);
            this.hideError();
            if (Utils.isNullOrWhiteSpace(msaxValues.msax_OrderConfirmationUrl)) {
                this.showCheckoutFragment(this._checkoutFragments.Confirmation);
            }
            else {
                window.location.href = msaxValues.msax_OrderConfirmationUrl += '?confirmationId=' + channelReferenceId;
            }
        }

        private submitOrder() {
            CommerceProxy.RetailLogger.checkoutServiceSubmitOrderStarted();
            LoadingOverlay.ShowLoadingDialog(Resources.String_180); // Submitting order ...

            var linesIdsToRemoveFromShoppingCart: string[] = this.cart().CartLines.map((cartLine) => { return cartLine.LineId; });

            CartWebApi.SubmitOrder(this.tenderLines, this.paymentCardAddress().Email, this)
                .done((salesOrder: CommerceProxy.Entities.SalesOrder) => {
                    CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderStarted();
                    CartWebApi.CleanUpAfterSuccessfulOrder(linesIdsToRemoveFromShoppingCart, this)
                        .done(() => {
                            CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderFinished();
                            LoadingOverlay.CloseLoadingDialog();
                            this.redirectOnOrderCreation(salesOrder.ChannelReferenceId);
                        })
                        .fail((errors: CommerceProxy.ProxyError[]) => {
                            Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderError, errors, "There was an error on order cleanup");
                            LoadingOverlay.CloseLoadingDialog();
                            this.redirectOnOrderCreation(salesOrder.ChannelReferenceId, errors);
                        });

                    CommerceProxy.RetailLogger.checkoutServiceSubmitOrderFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceSubmitOrderError, errors, "There was an error submitting the order");
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }
    }
}