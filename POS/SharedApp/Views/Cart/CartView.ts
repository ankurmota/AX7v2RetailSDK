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

///<reference path='../../Core/Navigator.ts'/>
///<reference path='../../Core/Converters.ts'/>
///<reference path='../../Controls/listView/knockout.listView.ts'/>
﻿﻿///<reference path='../../Commerce.Core.d.ts'/>
///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/AddIssueGiftCardDialog.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/EmailReceiptDialog.ts'/>
///<reference path='../Controls/OrderCheckoutDialog.ts'/>
///<reference path='../Controls/PrintReceiptDialog.ts'/>
///<reference path='../Controls/TextInputDialog.ts'/>
///<reference path='../Controls/ReasonCodeDialog.ts'/>
///<reference path='../Controls/WeighItemDialog.ts'/>
///<reference path='../CustomerOrder/PickUpInStoreView.ts'/>
///<reference path='../Order/PaymentView.ts'/>
///<reference path='../IKeepAliveView.ts'/>
///<reference path='../ViewControllerBase.ts'/>
/* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
///<reference path='../../Custom.Extension.d.ts'/>
   END SDKSAMPLE_CROSSLOYALTY (do not remove this) */


module Commerce.ViewControllers {
    "use strict";

    // enum for transaction detail view mode
    export class CartViewTransactionDetailViewMode {
        static Items: string = "items";
        static Payments: string = "payments";
        static Delivery: string = "delivery";
    }

    /**
     * Options for cart view.
     */
    export interface ICartViewControllerOptions {
        /**
         * The text to use to add or search an item.
         */
        itemToAddOrSearch?: string;

        /**
         * The path of the page that navigated to the CartPage
         */
        navigationSource?: string;

        /**
         * View mode of the transaction details pane.
         */
        transactionDetailViewMode?: string;

        /**
         * Payment card for shipping amount authorization, if hardware station is used.
         */
        paymentCardForRemainingBalance?: Proxy.Entities.CustomerOrderPaymentCard;

        /**
         * Tokenized payment card for shipping amount authorization, if card payment accept page is used.
         */
        tokenizedPaymentCard?: Proxy.Entities.TokenizedPaymentCard;
    }

    export class CartViewController extends ViewControllerBase implements IKeepAliveView<ICartViewControllerOptions> {
        public cartViewModel: Commerce.ViewModels.CartViewModel;
        public customerViewModel: Commerce.ViewModels.CustomersViewModel;

        public viewMode: Observable<string>;
        public paymentDialogVisible: Observable<boolean>;
        public emailDialogVisible: Observable<boolean>;
        public changeDialogVisible: Observable<boolean>;
        public customerDetailsVisible: Computed<boolean>;
        public searchText: Observable<string>;
        public processingPayment: Observable<boolean>;
        public receiptEmailAddressInput: Observable<string>;
        public printerEnabled: Observable<boolean>;
        public indeterminateWaitVisible: Observable<boolean>;
        public salesPaymentDifferenceVisible: Computed<boolean>;
        public changeDueWithTenderTypeString: Computed<string>;

        public discountLabel: Computed<string>;

        private _addIssueGiftCardDialog: Controls.AddIssueGiftCardDialog;
        private _orderCheckoutDialog: Controls.OrderCheckoutDialog;
        private _printReceiptDialog: Controls.PrintReceiptDialog;
        private _selectCustomerLoyaltyCardDialog: Controls.SelectCustomerLoyaltyCardDialog;
        private _customerCardViewModel: ViewModels.CustomerCardViewModel;
        private _emailReceiptDialog: Controls.EmailReceiptDialog;

        // Customer order values
        public showDeliveryUI: Computed<boolean>;
        public disableDeliveryButtons: Computed<boolean>;
        private _addProductVisibility: Computed<string>;

        // Dialog values
        public disableCartView: Observable<boolean>;

        // Payment dialog values
        private _paymentDialogTitle: Computed<string>;
        private _paymentDialogMessage: Computed<string>;
        private _paymentDialogPaymentMethods: ObservableArray<Model.Entities.TenderType>;
        private _paymentOptionsDialogVisible: Observable<boolean>;
        private _paymentOptionsDialogDataSource: ObservableArray<any>;

        //POShackF
        private SC_OrderInfoInputDialogTitle: Observable<string>;
        private SC_OrderInfoInputDialogVisible: Observable<boolean>;
        private SC_ZipCode: Observable<string>;
        public SC_AdSourceList: ObservableArray<string>;
        public SC_AdSourceSelectedOption: Observable<string>;
        public SC_PrimarySalesPerson: Observable<string>;
        public SC_SecondarySalesPerson: Observable<string>;
        public SC_TaxStatus: ObservableArray<string>;
        public SC_TaxStatusSelectedOption: Observable<string>;
        public SC_OrderStatus: ObservableArray<string>;
        public SC_OrderStatusSelectedOption: Observable<string>;
        public SC_DelieveryDateVisible: Observable<boolean>;
        public SC_PickupDateVisiable: Observable<boolean>;
        public SC_ChangeOrderStatusTitle: Observable<string>;
        public SC_ChangeOrderSatusHeaderDialogVisible: Observable<boolean>;
        public SC_ChangeOrderStatusLinesDialogVisible: Observable<boolean>;
        private SC_ChangeOrderStatusOperationId: number;
        public SC_DeliveryInstructionTtile: Observable<string>;
        public SC_DeliveryInstructionDialogVisible: Observable<boolean>;
        public SC_DeliveryInstruction: Observable<string>;

        //POShackF END

        // Controls whether remaining amount authorization should be shown to the user.
        // This variable is static because we need to keep state between navigation.
        // Method handleNavigationFromPaymentView controls its state.
        private static _disableAuthorizeRemainingAmountDialog: boolean = false;
        private _disableAuthorizeRemainingAmountDialog: Observable<boolean>;
        private _tokenizedPaymentCard: Proxy.Entities.TokenizedPaymentCard;

        public navigationSource: Observable<string>;
        public commonHeaderData: Commerce.Controls.CommonHeaderData;

        private _operationId: number;
        private _activeControllerForPeripheralEvents: any;
        private _keyPressHandler: (ev: KeyboardEvent) => void;

        private _options: ICartViewControllerOptions;

        // The value indicating whether or not a customer is picking up products.
        // If the value is true, it means that client needs to print PickUp receipt.
        private _isPickingUpProduct: boolean;

        /**
         * Result indicates whether remaining amount must be authorized after checkout or not.
         */
        private _mustAuthorizeRemainingAmountDialogResult: AsyncResult<boolean>;
        private setOptions: any;

        /**
         * To handle the gift receipt printing feature for print behavior AsRequired
         */
        private _giftReceiptsForChangeDialog: Model.Entities.Receipt[] = [];
        private _displayGiftReceiptPrintOption: Observable<boolean>;
        private _printGiftReceiptToggle: Observable<boolean>;

        /**
         * constructor
         *
         * @param {ICartViewControllerOptions} options Setup data for the view
         *
         * Supported option properties:
         * {string} [NavigationSource] The path of the page that navigated to the CartPage
         * {string} [TransactionDetailViewMode] View mode of  the transaction details pane
         */
        constructor(options?: ICartViewControllerOptions) {
            super(true);

            this.setOptions = (options: ICartViewControllerOptions) => {
                this._options = options = options || <ICartViewControllerOptions>{};
                if (!this.navigationSource) {
                    this.navigationSource = ko.observable('');
                }
                if (!this.viewMode) {
                    this.viewMode = ko.observable(null);
                }
                this.navigationSource(options.navigationSource || "");
                this.viewMode(options.transactionDetailViewMode || Commerce.ViewControllers.CartViewTransactionDetailViewMode.Items);
            };

            this._paymentOptionsDialogDataSource = ko.observableArray([
                {
                    Action: 1,
                    DisplayText: ViewModelAdapter.getResourceString('string_4316')
                }, {
                    Action: 2,
                    DisplayText: ViewModelAdapter.getResourceString('string_4315')
                }
            ]);
            this.setOptions(options);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.cartViewModel = new Commerce.ViewModels.CartViewModel();
            this.customerViewModel = new Commerce.ViewModels.CustomersViewModel();

            this._disableAuthorizeRemainingAmountDialog = ko.observable(CartViewController._disableAuthorizeRemainingAmountDialog);
            this._disableAuthorizeRemainingAmountDialog.subscribe((newValue: boolean) => { CartViewController._disableAuthorizeRemainingAmountDialog = newValue; });
            this._tokenizedPaymentCard = null;

            this.processingPayment = ko.observable(false);
            this.paymentDialogVisible = ko.observable(false);
            this.emailDialogVisible = ko.observable(false);
            this.changeDialogVisible = ko.observable(false);
            this.customerDetailsVisible = ko.computed(() => { return !StringExtensions.isNullOrWhitespace(this.cartViewModel.cart().CustomerId) }, this);
            this.searchText = ko.observable("");
            this.receiptEmailAddressInput = ko.observable("");
            this.indeterminateWaitVisible = ko.observable(false);
            this.printerEnabled = ko.observable(false);

            this.discountLabel = ko.computed(this.getDiscountLabel, this);
            this.salesPaymentDifferenceVisible = ko.computed(() => { return this.cartViewModel.salesPaymentDifference() != 0; }, this);
            this.changeDueWithTenderTypeString = ko.computed(() => {
                if (StringExtensions.isEmptyOrWhitespace(this.cartViewModel.changeTenderTypeName())) {
                    return Commerce.ViewModelAdapter.getResourceString("string_1816");
                } else {
                    return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_1824"), this.cartViewModel.changeTenderTypeName());
                }
            }, this);

            // Dialog values for controls
            this.disableCartView = ko.observable(false);

            // customer card
            this._customerCardViewModel = new ViewModels.CustomerCardViewModel(<ViewModels.ICustomerCardViewModelOptions>{
                isLoyaltyCardDataReadOnly: false,
                parentView: "CartView",
                chooseCardClick: this.selectCustomerLoyaltyCardAndAddToCart.bind(this)
            });

            // creates and adds dialogs to the page
            this.addControl(this._addIssueGiftCardDialog = new Controls.AddIssueGiftCardDialog({ cartViewModel: this.cartViewModel }));
            this.addControl(this._orderCheckoutDialog = new Controls.OrderCheckoutDialog());
            this.addControl(this._printReceiptDialog = new Controls.PrintReceiptDialog());
            this.addControl(this._selectCustomerLoyaltyCardDialog = new Controls.SelectCustomerLoyaltyCardDialog({
                loyaltyCards: this._customerCardViewModel.customerLoyaltyCards, enableSelect: true
            }));
            this.addControl(this._emailReceiptDialog = new Controls.EmailReceiptDialog());

            // PaymentDialog values
            this._paymentDialogTitle = ko.computed(() => this.getPaymentDialogOptions().title);
            this._paymentDialogMessage = ko.computed(() => this.getPaymentDialogOptions().message);
            this._paymentDialogPaymentMethods = ko.observableArray<Model.Entities.TenderType>(null);
            this._paymentOptionsDialogVisible = ko.observable(false);

            //POShackF 
            this.SC_OrderInfoInputDialogTitle = ko.observable("Enter order information");
            this.SC_OrderInfoInputDialogVisible = ko.observable(false);
            this.SC_ZipCode = ko.observable(StringExtensions.EMPTY);
            this.SC_AdSourceList = ko.observableArray(<string[]>[]);
            this.SC_AdSourceSelectedOption = ko.observable(StringExtensions.EMPTY);
            this.SC_PrimarySalesPerson = ko.observable(StringExtensions.EMPTY);
            this.SC_SecondarySalesPerson = ko.observable(StringExtensions.EMPTY);
            this.SC_TaxStatus = ko.observableArray(<string[]>[]);;
            this.SC_TaxStatusSelectedOption = ko.observable(StringExtensions.EMPTY);
            this.SC_OrderStatus = ko.observableArray(<string[]>[]);;
            this.SC_OrderStatusSelectedOption = ko.observable(StringExtensions.EMPTY);
            this.SC_DelieveryDateVisible = ko.observable(false);
            this.SC_PickupDateVisiable = ko.observable(false);
            this.SC_ChangeOrderStatusTitle = ko.observable("Change Line Order status");
            this.SC_ChangeOrderSatusHeaderDialogVisible = ko.observable(false);
            this.SC_ChangeOrderStatusLinesDialogVisible = ko.observable(false);
            this.SC_DeliveryInstructionTtile = ko.observable("Delivery Instruction");
            this.SC_DeliveryInstructionDialogVisible = ko.observable(false);
            this.SC_DeliveryInstruction = ko.observable(StringExtensions.EMPTY);
            //POShackF END

            // Parse keyboard input
            this._keyPressHandler = (e) => { this.handleKeyPress(e); };

            //#region Load Common Header
            this.commonHeaderData.viewCategoryName(false);
            this.commonHeaderData.backButtonVisible(false);
            this.commonHeaderData.viewSectionInfo(false);
            this.commonHeaderData.viewNavigationBar(false);
            this.commonHeaderData.expandButtonVisible(true);
            this.commonHeaderData.setTitleAsComputed(ko.computed(this.getCartTypeHeader, this));
            //#endregion

            this.showDeliveryUI = ko.computed(this.isDeliveryUIVisible, this);
            this.disableDeliveryButtons = ko.computed(this.isDeliveryButtonsDisabled, this);
            this.cartViewModel.cartItemsTotalCountString.subscribe((newValue: string) => this.commonHeaderData.categoryName(newValue), this);
            this.cartViewModel.customer.subscribe((newCustomer: Proxy.Entities.Customer) => {
                if (!ObjectExtensions.isNullOrUndefined(newCustomer)) {
                    // update the customer observable on the customerCardViewModel to render the customerCard template
                    this._customerCardViewModel.customer(newCustomer);

                    this.receiptEmailAddressInput(
                        !StringExtensions.isNullOrWhitespace(newCustomer.ReceiptEmail)
                            ? newCustomer.ReceiptEmail
                            : newCustomer.Email);
                }
                else {
                    this.cartViewModel.customerLoyaltyCards(null);
                    this.receiptEmailAddressInput("");
                }
            }, this);
            this.cartViewModel.customerLoyaltyCards.subscribe((updatedCustomerLoyaltyCards: Proxy.Entities.LoyaltyCard[]) => {
                // update the customerLoyaltyCards observable on the customerCardViewModel to render the customerCard template
                this._customerCardViewModel.customerLoyaltyCards(updatedCustomerLoyaltyCards);
            }, this);
            this._operationId = 0;
            this._addProductVisibility = ko.computed(() => {
                return this.cartViewModel.originalCartLines().length == 0 ? 'inherit' : 'collapse';
            }, this);

            this._displayGiftReceiptPrintOption = ko.observable(false);
            this._printGiftReceiptToggle = ko.observable(false);
        }

        private setNavigationBarVisibility() {
            var layout = Commerce.ApplicationContext.Instance.tillLayoutProxy.getLayout('transactionScreenLayout');
            if (!ObjectExtensions.isNullOrUndefined(layout)) {
                this.commonHeaderData.viewNavigationBar(layout.IsBrowseBarAlwaysVisible);
            }
        }

        private isCustomerAddedToSale(): boolean {
            var customer = this.cartViewModel.customer();
            if (ObjectExtensions.isNullOrUndefined(customer)) {
                return false;
            }
            return !StringExtensions.isNullOrWhitespace(customer.AccountNumber);
        }

        private addAffiliationsFromList() {
            Commerce.Operations.OperationsManager.instance.runOperation(
                Commerce.Operations.RetailOperation.AddAffiliationFromList, this);
        }

        //#region Payment options dialog
        private showDialogMustAuthorizeBeforeCheckout(): IAsyncResult<boolean> {
            this._mustAuthorizeRemainingAmountDialogResult = new AsyncResult<boolean>(this);
            this._paymentOptionsDialogVisible(true);
            return this._mustAuthorizeRemainingAmountDialogResult;
        }

        private paymentOptionsDialogCancelButtonClick() {
            // hide options dialog on dialog button click
            this._paymentOptionsDialogVisible(false);

            // do not resolve _mustAuthorizeRemainingAmountDialogResult on cancel button - we don't want any action to happen
        }

        private paymentOptionsClick(event: TileList.IItemInvokedArgs): boolean {
            // DepositOnly
            if (event.data.Action === 1) {
                this._paymentOptionsDialogVisible(false);
                this._mustAuthorizeRemainingAmountDialogResult.resolve(false);
            }
            // AuthorizeRemaining
            else if (event.data.Action === 2) {
                this._paymentOptionsDialogVisible(false);
                this._mustAuthorizeRemainingAmountDialogResult.resolve(true);
            }

            return true;
        }

        //#endregion

        private getPaymentDialogOptions(): Commerce.Controls.IModalDialogOptions {
            var dialogOptions: Commerce.Controls.IModalDialogOptions = <Commerce.Controls.IModalDialogOptions>{};

            // on customer orders, we have different messages
            if (CustomerOrderHelper.isCustomerOrderCreationOrEdition(this.cartViewModel.cart())) {
                // if we can authorize shipping amount and we have paid entirely the deposit, use message for shipping amount
                if (this.mustPromptToAuthorizeShippingAmount) {
                    dialogOptions.title = ViewModelAdapter.getResourceString("string_4319"); // Authorize remaining balance
                    dialogOptions.message = ViewModelAdapter.getResourceString("string_4320"); // Select a payment method for the remaining balance.
                } else {
                    // otherwise use message for deposit
                    dialogOptions.title = ViewModelAdapter.getResourceString("string_4317"); // Pay deposit
                    dialogOptions.message = ViewModelAdapter.getResourceString("string_4318"); // Select a payment method for the deposit.
                }
            } else {
                dialogOptions.title = ViewModelAdapter.getResourceString("string_100"); // Payment method
                dialogOptions.message = ViewModelAdapter.getResourceString("string_101"); // Select a payment method
            }

            return dialogOptions;
        }

        private getPaymentDialogPaymentMethods(): Proxy.Entities.TenderType[] {
            if (this.mustPromptToAuthorizeShippingAmount) {
                // when we need to authorize amounts, we can only use card types
                return this.cartViewModel.getTenderTypes().filter((tenderType: Proxy.Entities.TenderType) => tenderType.OperationId === Commerce.Operations.RetailOperation.PayCard);
            } else {
                // in other cases, we use all tender types available in the view model
                return this.cartViewModel.getTenderTypes();
            }
        }

        private isDeliveryUIVisible(): boolean {
            return CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(this.cartViewModel.cart());
        }

        private isDeliveryButtonsDisabled(): boolean {
            return !this.isDeliveryUIVisible() || this.cartViewModel.originalCartLines().length == 0;
        }

        private get isCustomerOrderReturn(): boolean {
            return (CustomerOrderHelper.isCustomerOrder(this.cartViewModel.cart()) && this.cartViewModel.cart().CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.Return);
        }

        /**
         * Gets the label text to displayed for the Discount label in the total panel
         *
         * @return {string} The discount label text.
         */
        private getDiscountLabel(): string {
            var isEstimatedDiscount: boolean;
            if (ObjectExtensions.isNullOrUndefined(this.cartViewModel.cart().IsDiscountFullyCalculated)) {
                isEstimatedDiscount = Commerce.ApplicationContext.Instance.deviceConfiguration.ManuallyCalculateComplexDiscounts;
            } else {
                isEstimatedDiscount = !this.cartViewModel.cart().IsDiscountFullyCalculated;
            }

            if (isEstimatedDiscount) {
                return Commerce.ViewModelAdapter.getResourceString("string_4375"); // ESTIMATED DISCOUNTS
            } else {
                return Commerce.ViewModelAdapter.getResourceString("string_118");  // DISCOUNTS
            }
        }

        private getCartTypeHeader(): string {
            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
            return Formatters.CartTypeName(cart);
        }

        /**
        * Set options during navigaton for IKeepAlive view.
        * @param {ICartViewControllerOptions} options The tender type.
        */
        public keepAliveViewActivated(options: ICartViewControllerOptions): void {
            this.setOptions(options);

            // If the store context is in any other context than the current store context, we need to restore it back to store context
            if (Commerce.Session.instance.productCatalogStore.StoreType != Proxy.Entities.StoreButtonControlType.CurrentStore) {
                var catalogViewModel: Commerce.ViewModels.CatalogViewModel = new Commerce.ViewModels.CatalogViewModel();
                this.indeterminateWaitVisible(true);
                catalogViewModel.setVirtualCatalog(Proxy.Entities.StoreButtonControlType.CurrentStore, null, null)
                    .done(() => {
                        this.indeterminateWaitVisible(false);
                    })
                    .fail((errors: Proxy.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        NotificationHandler.displayClientErrors(errors);
                    });
            }

            this.cartViewModel.cart(Session.instance.cart);

            // Set the customer value to get the customer card to refresh
            this.cartViewModel.updateCustomerLoyaltyCards(ObjectExtensions.isNullOrUndefined(Session.instance.cart) ? null : Session.instance.cart.CustomerId);

            if (!StringExtensions.isNullOrWhitespace(this._options.itemToAddOrSearch) && this._options.itemToAddOrSearch != "0") {
                this.addOrSearchProductsAndCustomers(this._options.itemToAddOrSearch);
            }

            this.setNavigationBarVisibility();

            // After view activated, handle actions to be performed after navigation event
            this.handleNavigationFromPaymentView();
            this._handleNavigationFromPickUpView();
        }

        private _handleNavigationFromPickUpView(): void {
            if (this.navigationSource() == "PickUpView") {
                var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
                this._isPickingUpProduct = true;

                // prompt user to provide deposit override for pickup if order was created with deposit override and
                // currently no deposit override is applied
                if (CustomerOrderHelper.shouldWarnForDepositOverrideActionOnPickup(cart)) {
                    // deposit was overridden during creation, manual deposit needs to be provided for pickup
                    Commerce.ViewModelAdapter.displayMessage(
                        Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.CUSTOMERORDER_MANUAL_DEPOSIT_REQUIRED),
                        MessageType.Info,
                        MessageBoxButtons.Default);
                }
            }
        }

        /**
         * Loads the control at page load
         */
        public load(): void {
            if (Commerce.ArrayExtensions.hasElements(this.cartViewModel.selectedCartLines())) {
                this.cartViewModel.selectedCartLines([]);
            }

            if (Commerce.ArrayExtensions.hasElements(this.cartViewModel.selectedTenderLines())) {
                this.cartViewModel.selectedTenderLines([]);
            }

            this.handleVoidAsyncResult(this.cartViewModel.load());
        }

        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement) {
            super.onCreated(element);
            this.setNavigationBarVisibility();
            // Add orientation switch tracker for layout change actions.
            Commerce.ApplicationContext.Instance.tillLayoutProxy.addOrientationChangedHandler(element, (args) => {
                this.setNavigationBarVisibility();
            });
        }

        /**
         * Called when view is shown.
         */
        public onShown() {
            // All Peripheral events are subscribed by main view controller.
            // Peripheral events are either handled by current controller of redirected to the child control on request. (e.g. Gift card dialog)
            this._activeControllerForPeripheralEvents = this;

            

            this.handleVoidAsyncResult(this.cartViewModel.setCartAsync(Session.instance.cart));

            // Notify listeners if the card is reloaded
            if (Session.instance.cartReloadedNotificationPending) {
                Session.instance.cartReloaded();
            }

            var barcodeScannerResult = Commerce.Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                if (this._activeControllerForPeripheralEvents && this._activeControllerForPeripheralEvents.barcodeScannerHandler) {
                    this._activeControllerForPeripheralEvents.barcodeScannerHandler.call(this._activeControllerForPeripheralEvents, barcode);
                }
            });

            var stripeReaderResult = Commerce.Peripherals.instance.magneticStripeReader.enableAsync((cardInfo: Proxy.Entities.CardInfo) => {
                if (this._activeControllerForPeripheralEvents && this._activeControllerForPeripheralEvents.magneticStripeReaderHandler) {
                    this._activeControllerForPeripheralEvents.magneticStripeReaderHandler.call(this._activeControllerForPeripheralEvents, cardInfo);
                }
            });

            //POSHackF
            this.SC_initCustomControls();
            this.SC_PopulateKitPrice();
            this.SC_showOrderInfoInputDialog();
            //POSHackF END

            VoidAsyncResult.join([barcodeScannerResult, stripeReaderResult]).done(() => {
                // Parse keyboard input
                document.addEventListener("keypress", this._keyPressHandler);
            });
        }

        public lineItemListLoadingStateHandler(event: any) {
            var listControl = event.currentTarget.winControl;
            var items: number = listControl.itemDataSource.list.length;

            if (ObjectExtensions.isNullOrUndefined(listControl.previousNumberOfItems)) {
                listControl.previousNumberOfItems = 0;
            }

            // if the items are finished loading, scroll to and select the last item
            if (items != listControl.previousNumberOfItems && listControl.loadingState == "complete") {
                listControl.previousNumberOfItems = items;
                Commerce.Host.instance.timers.setImmediate(() => {
                    listControl.selection.add(items - 1);
                    listControl.ensureVisible(items - 1);
                });
            }
        }

        /**
         * Called when view is hidden.
         */
        public onHidden() {
            document.removeEventListener("keypress", this._keyPressHandler);
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
            Commerce.Peripherals.instance.magneticStripeReader.disableAsync();
        }

        private handleNavigationFromPaymentView() {
            // if we just got back from a payment
            if (this.navigationSource() == "PaymentView") {

                // we keep the tokenized payment card from the last time we came from payment view.
                this._tokenizedPaymentCard = this._options.tokenizedPaymentCard;

                // handle flow when payment is done
                this.paymentSuccessCallback();
            } else {
                // if we navigate from any place but payment view, we default this to false
                // since we always want to give the user the option to authorize the remaining payment if we went off the usual flow (cart view -> payment view -> cart view)
                this._disableAuthorizeRemainingAmountDialog(false);
            }
        }

        // If we type anywhere other than an input, focus to the searchBar
        private handleKeyPress(evt: any): void {
            if (evt.target.nodeName != 'INPUT' && evt.keyCode != 13) {
                if (!this.isDialogVisible()) {
                    $('#searchBar').focus();
                } else if (this.emailDialogVisible()) {
                    $('#emailInput').focus();
                }
            }
        }

        private isDialogVisible(): boolean {
            return (this.paymentDialogVisible() || this.emailDialogVisible() || this.changeDialogVisible() || this._paymentOptionsDialogVisible());
        }

        private get mustPromptToAuthorizeShippingAmount(): boolean {
            // only ask for shipping credit card if...
            return CustomerOrderHelper.canAuthorizeShippingAmount(this.cartViewModel.cart()) // view model let's us do it for current cart state
                && this.cartViewModel.canCheckout // after paying all deposit (canCheckout == true)
                && this._disableAuthorizeRemainingAmountDialog() == false // remaining amount dialog is not disabled
                && ObjectExtensions.isNullOrUndefined(this._tokenizedPaymentCard); // we don't have tokenized payment card yet
        }

        private displayWarningIfUnableToPay(tenderType?: Proxy.Entities.TenderType): boolean {
            var cart = this.cartViewModel.cart();

            if (!ArrayExtensions.hasElements(CartHelper.GetNonVoidedCartLines(this.cartViewModel.originalCartLines()))
                && (!CartHelper.isCartType(cart, Model.Entities.CartType.AccountDeposit))
                && (ObjectExtensions.isNullOrUndefined(cart)
                    || !ArrayExtensions.hasElements(cart.IncomeExpenseLines))) {
                NotificationHandler.displayErrorMessage(ErrorTypeEnum.CART_IS_EMPTY);
                return true;
            }

            // customer order or quote
            if (CustomerOrderHelper.isCustomerOrder(cart) || CustomerOrderHelper.isQuote(cart)) {

                if (!this.cartViewModel.cartHasCustomer) {
                    NotificationHandler.displayErrorMessage('string_4419'); // Customer is not selected!
                    return true;
                }

                if ((cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                    || cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit)
                    && this.isDeliveryModeSetOnAnyCartLine && !this.isDeliveryInfoSetForAllCartLines) {
                    NotificationHandler.displayErrorMessage('string_4418'); // "Not all items have selected delivery mode!"
                    return true;
                }

                if (!ObjectExtensions.isNullOrUndefined(tenderType) && tenderType.OperationId == Operations.RetailOperation.PayCustomerAccount) {
                    if (cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                        || cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit) {
                        NotificationHandler.displayErrorMessage('string_29021'); // The customer account payment method cannot be used while creating or editing a customer order or quotation.
                        return true;

                    } else if (cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.Cancellation) {
                        NotificationHandler.displayErrorMessage('string_29025'); // The customer account payment method cannot be used while cancelling a customer order.
                        return true;
                    }
                }
            }

            return false;
        }

        private get isDeliveryInfoSetForAllCartLines(): boolean {
            var haveItemsWithNoAddressOrDeliveryDate: boolean =
                this.cartViewModel.originalCartLines().some((line: Proxy.Entities.CartLine) => {
                    return !line.IsVoided &&
                        (ObjectExtensions.isNullOrUndefined(line.ShippingAddress) ||
                            ObjectExtensions.isNullOrUndefined(line.ShippingAddress.RecordId) ||
                            ObjectExtensions.isNullOrUndefined(line.RequestedDeliveryDate));
                });

            return !haveItemsWithNoAddressOrDeliveryDate;
        }

        private get isDeliveryModeSetOnAnyCartLine(): boolean {
            return this.cartViewModel.originalCartLines().some((line: Proxy.Entities.CartLine) => {
                return !line.IsVoided && line.DeliveryMode != null;
            });
        }

        /**
          * Gets whether we can add payment to current cart.
          */
        private canAddPayment(): boolean {
            // can add payment if
            return !this.processingPayment() // we are not handling a payment already
                && this.cartViewModel.canAddPayment; // it's still possible to add more payments to the cart
        }

        /**
          * Handles the payment button click.
          *
          * @param {event} event The tile click event.
          * @return {boolean} Returns true to indicate that the operation is handled by the payment button (used for buttonGrid).
          */
        private paymentButtonClickHandler(event: TileList.IItemInvokedArgs): boolean {
            // closes dialog after button click
            this.hidePaymentDialog();
            var tenderType: Model.Entities.TenderType = event.data;

            switch (tenderType.OperationId) {
                case Operations.RetailOperation.PayCash:
                case Operations.RetailOperation.PayCustomerAccount:
                case Operations.RetailOperation.PayCurrency:
                case Operations.RetailOperation.PayCheck:
                case Operations.RetailOperation.PayCreditMemo:
                case Operations.RetailOperation.PayGiftCertificate:
                case Operations.RetailOperation.PayLoyalty:
                    this.executePaymentFlow(tenderType.OperationId, tenderType, null, null, true, true);
                    break;
                case Operations.RetailOperation.PayCard:
                    this.payCard(null, null, true);
                    break;
                default:
                    NotificationHandler.displayErrorMessage('string_1133', tenderType.Name);
                    break;
            }

            return true;
        }

        //POSHackF 
        private SC_PopulateKitPrice() {
            var cart = Session.instance.cart;
            if (!cart || !cart.CartLines || cart.CartLines.length < 6 || cart.CartTypeValue != 1) {
                return;
            }
            var kitLinesCount = 0;

            for (var i = 0; i < cart.CartLines.length; i++) {
                var cartLineProperties = Commerce.CartHelper.SC_getCartLineProperties(cart.CartLines[i].Comment);
                if (cartLineProperties[2] != "true" || cartLineProperties[3].length > 0) {
                    continue;
                }
                var price: string = "";
                switch (cart.CartLines[i].ItemId) {
                    case "0003":
                        price = "79.20"; //TODO: Change this per ENV
                        break;
                    case "0005":
                        price = "5.39";//TODO: Change this per ENV
                        break;
                    case "0006":
                        price = "4.49";//TODO: Change this per ENV
                        break;
                    case "0009":
                        price = "32.40";//TODO: Change this per ENV
                        break;
                    case "0021":
                        price = "359.10";//TODO: Change this per ENV
                        break;
                    case "0004":
                        price = "809.10";//TODO: Change this per ENV
                        break;
                    default:
                        continue;
                }

                //cartLineProperties[2] = "true";
                cartLineProperties[3] = price;
                cart.CartLines[i].Comment = Commerce.CartHelper.SC_CartLinePropertiesToComment(cartLineProperties);
                kitLinesCount++;
            }
            if (kitLinesCount >= 6) {
                var cartProperties = Commerce.CartHelper.SC_getCartProperties(cart.Comment);
                cartProperties[8] = "1,290.00"; //Kit price - header //TODO: Change this per ENV
                cart.Comment = Commerce.CartHelper.SC_CartPropertiesToComment(cartProperties);
                var asyncResult: IVoidAsyncResult = new AsyncQueue().enqueue(() => {
                    return this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                }).enqueue((): IVoidAsyncResult => {
                    return this.cartViewModel.cartManager.updateCartLinesOnCartAsync(cart.CartLines);
                }).run();
                this.SC_refreshCartView();
            }
        }
        //POSHackF  end
        private SC_initCustomControls() {
            var AdSourceOptions = new Array<string>();
            AdSourceOptions.push("Internet");
            AdSourceOptions.push("Commercial");
            AdSourceOptions.push("Radio");
            AdSourceOptions.push("Walk-In");
            this.SC_AdSourceList(AdSourceOptions);

            var TaxStatusOptions = new Array<string>();
            TaxStatusOptions.push("Taxable");
            TaxStatusOptions.push("Exempt");
            this.SC_TaxStatus(TaxStatusOptions);

            var OrderStatusOptions = new Array<string>();
            OrderStatusOptions.push("Pickup");
            OrderStatusOptions.push("Delivery");
            OrderStatusOptions.push("Reserve");
            OrderStatusOptions.push("CWI");
            OrderStatusOptions.push("Layaway");
            this.SC_OrderStatus(OrderStatusOptions);
        }
        private SC_showOrderInfoInputDialog(): void {

            var cart: Model.Entities.Cart = Session.instance.cart;
            if (!cart || cart.CartLines.length < 1 || cart.CartTypeValue != 1 || cart.IsReturnByReceipt == true) {
                return;
            }
            /**
            //var indexOfProperty = cart.ExtensionProperties.indexOf("SC_IsOrderInfoCollected");
            cart.ExtensionProperties[0] = new Proxy.Entities.CommercePropertyClass();
            cart.ExtensionProperties[0].Key = "SC_IsOrderInfoCollected";
            cart.ExtensionProperties[0].Value = "true";
            var indexOfProperty = cart.ExtensionProperties.indexOf("SC_IsOrderInfoCollected");
            var a = new Proxy.Entities.AttributeValueBaseClass();
            */
            /**
            var attributeValue = new Proxy.Entities.AttributeValueBaseClass();
            attributeValue.Name = "SG_Attr";
            var property = new Proxy.Entities.CommercePropertyClass();
            property.Key = "SC_IsOrderInfoCollected";
            property.Value = "true";
            attributeValue.ExtensionProperties = new Array<Proxy.Entities.CommercePropertyClass>();
            attributeValue.ExtensionProperties.push(property);
            cart.AttributeValues = new Array<Proxy.Entities.AttributeValueBaseClass>();
            cart.AttributeValues.push(attributeValue);
            */

            /**
            if (Session.instance.cart.Id) {
                //this.cartViewModel.cartManager.saveCartAsync(Session.instance.cart.Id, cart);
            }
            else {
                //this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
            }
            */
            var cartProperty = Commerce.CartHelper.SC_getCartProperties(cart.Comment);
            if (cartProperty[1] != "true") {
                //this.cartViewModel.SC_IsOrderInfoCollected = true;


                this.indeterminateWaitVisible(true);
                this.SC_OrderInfoInputDialogVisible(true);


            }
            else {
                this.indeterminateWaitVisible(false);
                this.SC_OrderInfoInputDialogVisible(false);

            }
            //var cart: Model.Entities.Cart = this.cartViewModel.cart();
            /**
            if (!cart.SC_IsOrderInfoCollected) {
                this.SC_OrderInfoInputDialogVisible(true);
                cart.SC_IsOrderInfoCollected = true;
            }
            */
            //cart = this.cartViewModel.cart();
            //this.SC_OrderInfoInputDialogVisible(true);
        }
        private SC_refreshCartView() {
            var updatedCart = Session.instance.cart;
            this.cartViewModel.cart(updatedCart);
            this.cartViewModel.originalCartLines(updatedCart.CartLines);
        }
        public SC_OrderInfoInputDialogButtonClick() {
            var cart: Model.Entities.Cart = Session.instance.cart;
            if (cart) {
                var cartProperty = new Array<string>();
                cartProperty = Commerce.CartHelper.SC_getCartProperties(cart.Comment);
                cartProperty[1] = "true";
                cartProperty[2] = this.SC_AdSourceSelectedOption();
                cartProperty[3] = this.SC_ZipCode();
                cartProperty[4] = this.SC_PrimarySalesPerson();
                cartProperty[5] = this.SC_SecondarySalesPerson();
                cartProperty[6] = this.SC_TaxStatusSelectedOption();
                cartProperty[7] = this.SC_OrderStatusSelectedOption();
                cart.Comment = Commerce.CartHelper.SC_CartPropertiesToComment(cartProperty);
                //this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                this.SC_updateCartLinesOrderStatusByHeader(cart, this.SC_OrderStatusSelectedOption());
                var cartLines: Proxy.Entities.CartLine[] = cart.CartLines;
                var asyncResult: IVoidAsyncResult = new AsyncQueue().enqueue(() => {
                    return this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                }).enqueue((): IVoidAsyncResult => {
                    return this.cartViewModel.cartManager.updateCartLinesOnCartAsync(cartLines);
                }).run();
                //this.cartViewModel.cartManager.updateCartLinesOnCartAsync(cartLines);

            }
            if (this.SC_TaxStatusSelectedOption() == "Exempt") {
                this.transactionTaxOverride("Exempt");
            }
            this.SC_refreshCartView();
            this.indeterminateWaitVisible(false);
            this.SC_OrderInfoInputDialogVisible(false);
        }

        public SC_DeliveryInstructionDialogButtonClick(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var cart: Model.Entities.Cart = Session.instance.cart;
                    if (cart) {
                        var cartProperties = Commerce.CartHelper.SC_getCartProperties(cart.Comment);
                        cartProperties[9] = this.SC_DeliveryInstruction();
                        cart.Comment = Commerce.CartHelper.SC_CartPropertiesToComment(cartProperties);
                        var asyncResult: IVoidAsyncResult = new AsyncQueue().enqueue(() => {
                            return this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                        }).run();
                        this.SC_refreshCartView();
                    }
                    break;
                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    break;
                default:
                    break;
            }

            this.indeterminateWaitVisible(false);
            this.SC_DeliveryInstructionDialogVisible(false);
        }

        public SC_ChangeOrderStatusHeaderButtonClick(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    if (this.SC_OrderStatusSelectedOption().length < 1) {
                        break;
                    }
                    var cart: Model.Entities.Cart = Session.instance.cart;
                    if (!cart) {
                        return;
                    }
                    switch (this.SC_ChangeOrderStatusOperationId) {
                        case 9001://Header
                            var cartProperty = Commerce.CartHelper.SC_getCartProperties(cart.Comment);
                            cartProperty[7] = this.SC_OrderStatusSelectedOption();
                            cart.Comment = Commerce.CartHelper.SC_CartPropertiesToComment(cartProperty);
                            this.SC_updateCartLinesOrderStatusByHeader(cart, this.SC_OrderStatusSelectedOption());
                            var cartLines: Proxy.Entities.CartLine[] = cart.CartLines;
                            var asyncResult: IVoidAsyncResult = new AsyncQueue().enqueue(() => {
                                return this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                            }).enqueue((): IVoidAsyncResult => {
                                return this.cartViewModel.cartManager.updateCartLinesOnCartAsync(cart.CartLines);
                            }).run();
                            break;
                        case 9002://Lines
                            var cartLinesSelected = this.SC_updateCartLinesOrderStatusBySelected(this.SC_OrderStatusSelectedOption());
                            //POSHackF2
                            for (var j = 0; j < cartLinesSelected.length; j++) {
                                for (var i = 0; i < cart.CartLines.length; i++) {
                                    if (cart.CartLines[i].LineId == cartLinesSelected[j].LineId) {
                                        cart.CartLines[i].Comment = cartLinesSelected[j].Comment;
                                    }
                                }
                            }
                            var asyncResult: IVoidAsyncResult = new AsyncQueue().enqueue(() => {
                                return this.cartViewModel.cartManager.createOrUpdateCartAsync(cart);
                            }).enqueue((): IVoidAsyncResult => {
                                return this.cartViewModel.cartManager.updateCartLinesOnCartAsync(cart.CartLines);
                            }).run();
                            break;
                        default:
                            break;
                    }
                    this.SC_refreshCartView();
                    break;
                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    break;
                default:
                    break;
            }

            this.indeterminateWaitVisible(false);
            this.SC_ChangeOrderSatusHeaderDialogVisible(false);
        }

        public SC_ChangeOrderStatusOperation(operationId: number) {
            this.SC_ChangeOrderStatusOperationId = operationId;
            if (this.SC_ChangeOrderStatusOperationId == 9002 && !this.verifyOneItemSelected()) {
                return;
            }
            this.indeterminateWaitVisible(true);
            this.SC_ChangeOrderSatusHeaderDialogVisible(true);
        }

        public SC_AddDeliveryInstruction(opertaionId: number) {
            this.indeterminateWaitVisible(true);
            this.SC_DeliveryInstructionDialogVisible(true);

        }

        private SC_updateCartLinesOrderStatusByHeader(cart: Proxy.Entities.Cart, orderStaus: string) {
            for (var i = 0; i < cart.CartLines.length; i++) {
                var cartLineProperties = Commerce.CartHelper.SC_getCartLineProperties(cart.CartLines[i].Comment);
                cartLineProperties[1] = orderStaus;
                cart.CartLines[i].Comment = Commerce.CartHelper.SC_CartLinePropertiesToComment(cartLineProperties);
            }

        }

        private SC_updateCartLinesOrderStatusBySelected(orderStatus: string): Proxy.Entities.CartLine[] {
            var cartLinesSelected = this.cartViewModel.selectedCartLines();
            for (var i = 0; i < cartLinesSelected.length; i++) {
                var cartLineProperties = Commerce.CartHelper.SC_getCartLineProperties(cartLinesSelected[i].Comment);
                cartLineProperties[1] = orderStatus;
                cartLinesSelected[i].Comment = Commerce.CartHelper.SC_CartLinePropertiesToComment(cartLineProperties);
            }
            return cartLinesSelected;
        }

            /**
        private SC_loadCartLineProperties(src: string): Dictionary<string> {
            var result;
            if (src && src.length > 0) {
                result = new Dictionary<string>();
                var lines = src.split(CartViewController.SC_commentDelimiter2);
                for (var line in lines) {
                    if (!line) {
                        continue;
                    }
                    var keyValue = line.split(CartViewController.SC_commentDelimiter3);
                    
                }

            }
            return result;
        }
            */
        //POShackF END

        /**
         *  Shows the payment dialog.
         */
        private showPaymentDialog(): void {
            this.indeterminateWaitVisible(true);

            var cart: Model.Entities.Cart = this.cartViewModel.cart();

            // Get the possible payment methods to display
            var tenderTypes: Model.Entities.TenderType[] = this.getPaymentDialogPaymentMethods();

            // Wait for the tender types to be validated and display to the user
            this.cartViewModel.getExecutableTenderTypesAsync(tenderTypes).done((executableTenderTypes: Model.Entities.TenderType[]) => {

                // If no tender types are executable, allow all to be shown so the user can select one and
                // see the validation error message for that tender type operation
                if (!ArrayExtensions.hasElements(executableTenderTypes)) {
                    executableTenderTypes = tenderTypes;
                }

                // Sort the Tender Types by TenderTypeId
                if (ArrayExtensions.hasElements(executableTenderTypes)) {
                    executableTenderTypes.sort((first: Model.Entities.TenderType, second: Model.Entities.TenderType): number => {
                        var firstNum: number = parseInt(first.TenderTypeId);
                        var secondNum: number = parseInt(second.TenderTypeId);
                        return firstNum - secondNum;
                    });
                }

                this._paymentDialogPaymentMethods(executableTenderTypes);
                this.paymentDialogVisible(true);
            });
        }

        /**
         *  Hides the payment dialog.
         */
        private hidePaymentDialog() {
            this.indeterminateWaitVisible(false);
            this.paymentDialogVisible(false);
        }

        /**
         *  Executes the payment flow (checks if a payment can be added, display error messages, shows payment/email dialog, etc).
         *  Call this method to start payment / checkout flow. If cart can be checked out, payment won't be processed, and email dialog will be shown instead.
         *  If cart cannot be checked out, it will show the popup asking for the payment type.
         *  On customer order scenarios, it will add a popup to ask whether shipping amount authorization should happen or not.
         */
        private executeShowPaymentFlow(): void {
            this.executePaymentFlow();
        }

        /**
         *  Executes the payment flow (checks if a payment can be added, display error messages, shows payment/email dialog, etc).
         *  Call this method to start payment / checkout flow. If cart can be checked out, payment won't be processed, and email dialog will be shown instead.
         *  If you do not provide an operationId for the payment, it will show the popup asking for the payment type.
         *  On customer order scenarios, it will add a popup to ask whether shipping amount authorization should happen or not.
         *
         * @param {Commerce.Operations.RetailOperation} [operationId] The id of the operation that called the method.
         * @param {Proxy.Entities.TenderType} [tenderType] The tender type (ex. credit card).
         * @param {Proxy.Entities.CardInfo} [paymentCardInfo] The card information.
         * @param {Proxy.Entities.CardSource} [paymentCardSource] The source of the card information (ex. MSR, keyboard, etc...).
         * @param {Proxy.Entities.CardPaymentAcceptPoint} [cardPaymentAcceptPoint] The card payment accept point, if available.
         * @param {boolean} [doNotAskForShippingAmountAuthorization] True the shipping amount authorization flow will not be executed, false the flow will be executed.
         */
        private executePaymentFlow(
            operationId?: Commerce.Operations.RetailOperation,
            tenderType?: Proxy.Entities.TenderType,
            paymentCardInfo?: Proxy.Entities.CardInfo,
            paymentCardSource?: Proxy.Entities.CardSource,
            cardPaymentAcceptPoint?: Proxy.Entities.CardPaymentAcceptPoint,
            doNotAskForShippingAmountAuthorization?: boolean): void {

            if (this.displayWarningIfUnableToPay(tenderType)) {
                return;
            }

            var currentCart: Proxy.Entities.Cart = this.cartViewModel.cart();

            // Check whether the state is valid to run the payment operation
            if (operationId) {
                var errors: Proxy.Entities.Error[] = Commerce.Operations.PaymentOperationHelper.preOperationValidation(operationId, currentCart);
                if (errors != null) {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    return;
                }
            }

            if (this.canAddPayment() && !this.cartViewModel.canCheckout) { // first add payments, unless we can checkout

                // if operationId has been selected, shipping amount authorization decision has already been taken
                if (doNotAskForShippingAmountAuthorization !== true && CustomerOrderHelper.canAuthorizeShippingAmount(currentCart)) {
                    // if we can authorize shipping amount, provide the option to choose between just paying the deposit
                    // or paying the deposit and authorizing shipping amount
                    this.showDialogMustAuthorizeBeforeCheckout().done((mustAuthorizeShippingAmount: boolean) => {

                        // user has select to authorize remaining amount after payment
                        this._disableAuthorizeRemainingAmountDialog(mustAuthorizeShippingAmount == false);

                        // continue with payment flow
                        this.executePaymentFlow(operationId, tenderType, paymentCardInfo, paymentCardSource, cardPaymentAcceptPoint, /* doNotAskForShippingAmountAuthorization */true);
                    });

                } else if ((operationId == null) || (operationId === Commerce.Operations.RetailOperation.None)) {
                    // if user did not choose an operation id
                    // show payment methods dialog so user can pick one
                    this.showPaymentDialog();

                } else {
                    // process payment
                    this.executePaymentOperation(operationId, tenderType, paymentCardInfo, paymentCardSource, cardPaymentAcceptPoint);
                }

            } else if (this.mustPromptToAuthorizeShippingAmount) { // then authorize shipping amount if possible

                // if doNotAskForShippingAmountAuthorization === true it means the user already clicked on the authorize remaining amount button
                if (doNotAskForShippingAmountAuthorization === true) {
                    // process the authorization
                    this.executePaymentOperation(operationId, tenderType, paymentCardInfo, paymentCardSource, cardPaymentAcceptPoint);
                } else {
                    // whenever authorizing shipping amount, ask for user to select "cards" options on dialog - or cancel it
                    this.showPaymentDialog();
                }
            } else if (this.cartViewModel.canCheckout) {
                // Conclude the transaction
                this.concludeTransaction(null, this);
            }
        }

        /**
         *  Executes a payment operation with a specific tender type, if provided, otherwise will use application context to find the matching tender type for the current operation.
         *
          * @param {Commerce.Operations.RetailOperation} operationId The id of the operation that called the method.
          * @param {Proxy.Entities.TenderType} [tenderType] The tender type (ex. credit card).
          * @param {Proxy.Entities.CardInfo} [paymentCardInfo] The card information.
          * @param {Proxy.Entities.CardSource} [paymentCardSource] The source of the card information (ex. MSR, keyboard, etc...).
          * @param {Proxy.Entities.CardPaymentAcceptPoint} [cardPaymentAcceptPoint] The card payment accept point, if available.

         */
        private executePaymentOperation(
            operationId: Commerce.Operations.RetailOperation,
            tenderType?: Proxy.Entities.TenderType,
            paymentCardInfo?: Proxy.Entities.CardInfo,
            paymentCardSource?: Proxy.Entities.CardSource,
            cardPaymentAcceptPoint?: Proxy.Entities.CardPaymentAcceptPoint): void {

            var paymentAmount: () => number = CartHelper.cartAmountDue;
            var tokenizeCard: boolean = false;

            if (this.mustPromptToAuthorizeShippingAmount) {
                paymentAmount = CartHelper.cartEstimatedShippingAmount;
                tokenizeCard = true;
            }

            if (tenderType == null) {
                tenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(operationId);
            }

            var paymentControllerOptions: Commerce.Operations.IPaymentOperationOptions = <Commerce.Operations.IPaymentOperationOptions>{
                tenderType: tenderType,
                paymentCardInfo: paymentCardInfo,
                paymentCardSource: paymentCardSource ? paymentCardSource : Proxy.Entities.CardSource.Unknown,
                paymentAmount: paymentAmount,
                tokenizeCard: tokenizeCard,
                loyaltyCardId: this.cartViewModel.cart().LoyaltyCardId,
                cardPaymentAcceptPageUrl: cardPaymentAcceptPoint ? cardPaymentAcceptPoint.AcceptPageUrl : undefined,
                cardPaymentAcceptPageSubmitUrl: cardPaymentAcceptPoint ? cardPaymentAcceptPoint.AcceptPageSubmitUrl : undefined,
                cardPaymentAcceptMessageOrigin: cardPaymentAcceptPoint ? cardPaymentAcceptPoint.MessageOrigin : undefined,
            };

            if (operationId === Commerce.Operations.RetailOperation.PayCashQuick) {
                // quick cash operation is handled in this view, all other operations navigates
                // for this case, we enable spinner, for other cases we don't, since a navigation back click
                // would pop this view back from the cache with the spinner, blocking the use of the view
                this.processingPayment(true);
                this.indeterminateWaitVisible(true);
            }

            Commerce.Operations.OperationsManager.instance.runOperation(operationId, paymentControllerOptions)
                .done((result) => {
                    this.paymentSuccessCallback(!result.canceled);
                }).fail((errors) => {
                    this.paymentErrorCallback(errors);
                });
        }

        /**
         * Handles the pay card button click.
         *
         * @param {Proxy.Entities.CardInfo} [paymentCardInfo] The card information.
         * @param {Proxy.Entities.CardSource} [paymentCardSource] The source of the card information (ex. MSR, keyboard, etc...).
         * @param {boolean} [doNotAskForShippingAmountAuthorization] True the shipping amount authorization flow will not be executed, false the flow will be executed.
         */
        private payCard(paymentCardInfo?: Proxy.Entities.CardInfo, paymentCardSource?: Proxy.Entities.CardSource, doNotAskForShippingAmountAuthorization?: boolean): void {
            // Get card tender type.
            var cardTenderType: Proxy.Entities.TenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCard);

            if (ObjectExtensions.isNullOrUndefined(cardTenderType)) {
                NotificationHandler.displayErrorMessage("string_1158"); // Card is not an accepted form of payment. Use a different payment method, and then try again.
                return;
            }

            if (this.canAddPayment() || this.mustPromptToAuthorizeShippingAmount) {
                if (Peripherals.HardwareStation.HardwareStationContext.instance.isActive()) {
                    // Execute payment using hardware station.
                    this.executePaymentFlow(Operations.RetailOperation.PayCard, cardTenderType, paymentCardInfo, paymentCardSource, null, doNotAskForShippingAmountAuthorization);
                } else {
                    // Execute payment using external card payment accept page.
                    this.indeterminateWaitVisible(true);
                    var paymentAmount: number = this.cartViewModel.cart().AmountDue;
                    if (this.mustPromptToAuthorizeShippingAmount) {
                        paymentAmount = this.cartViewModel.cart().EstimatedShippingAmount;
                    }

                    var cardPaymentEnabled: boolean = !this.mustPromptToAuthorizeShippingAmount && paymentAmount > 0; // Authorize payment amount scenario.
                    var cardTokenizationEnabled: boolean = this.mustPromptToAuthorizeShippingAmount || paymentAmount < 0; // Refund and authorize shipping amount scenarios.

                    this.cartViewModel.cartManager.getCardPaymentAcceptPoint(cardPaymentEnabled, cardTokenizationEnabled)
                        .done((result: Proxy.Entities.CardPaymentAcceptPoint) => {
                            if (!ObjectExtensions.isNullOrUndefined(result)
                                && !StringExtensions.isNullOrWhitespace(result.AcceptPageUrl)
                                && !StringExtensions.isNullOrWhitespace(result.MessageOrigin)) {

                                result.AcceptPageUrl = this.updatePaymentAcceptUrl(result);
                                this.indeterminateWaitVisible(false);
                                this.executePaymentFlow(Operations.RetailOperation.PayCard, cardTenderType, paymentCardInfo, paymentCardSource, result, doNotAskForShippingAmountAuthorization);
                            } else {
                                this.paymentErrorCallback([new Proxy.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGETCARDPAYMENTACCEPTPOINT)]);
                            }
                        }).fail((errors: Proxy.Entities.Error[]) => {
                            // Handle get card payment accept point call failure.
                            if (errors[0].ErrorCode.toUpperCase() ===
                                ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED.serverErrorCode) {
                                this.paymentErrorCallback([new Proxy.Entities.Error(ErrorTypeEnum.NOT_SUPPORTED_IN_OFFLINE_MODE_WHEN_HARDWARE_STATION_NOT_ACTIVE)]);
                            }
                            else {
                                this.paymentErrorCallback(errors);
                            }
                        });
                }
            } else {
                NotificationHandler.displayErrorMessage(ErrorTypeEnum.CANNOT_PAYMENT_TRANSACTION_COMPLETED);
            }
        }

        /**
          * Adds the style query parameters to the payment accept url.
          * @param {Proxy.Entities.CardPaymentAcceptPoint} paymentAcceptPoint The payment accept point.
          * @return {string} The payment accept url with the query parameters.
          */
        private updatePaymentAcceptUrl(paymentAcceptPoint: Proxy.Entities.CardPaymentAcceptPoint): string {

            var paymentAcceptUrl: string;
            if (!StringExtensions.isNullOrWhitespace(paymentAcceptPoint.AcceptPageSubmitUrl)) {
                // When a separate submit URL is provided, do not append anything to the open URL. Most likely, these parameters will be ignored anyway.
                paymentAcceptUrl = paymentAcceptPoint.AcceptPageUrl;
            }
            else {
                var bodyElement: any = $(document).find("body");
                var bodyBackgroundColor: string = this.convertToHexColor(bodyElement.css("background-color"));

                var textElement: any = $(this.getViewContainer()).find("#sampleText");
                var textBackgroundColor: string = this.convertToHexColor(textElement.css("background-color"));
                var textColor: string = this.convertToHexColor(textElement.css("color"));
                var textFontSize: string = textElement.css("font-size");

                var labelElement: any = $(this.getViewContainer()).find("#sampleLabel");
                var labelColor: string = this.convertToHexColor(labelElement.css("color"));
                var textFontFamilyString: string = labelElement.css("font-family").replace(/\"/g, "").replace(/\'/g, "");

                paymentAcceptUrl = paymentAcceptPoint.AcceptPageUrl
                    + '&pagebackgroundcolor=' + encodeURIComponent(bodyBackgroundColor)
                    + '&disabledtextbackgroundcolor=' + encodeURIComponent(bodyBackgroundColor)
                    + '&labelcolor=' + encodeURIComponent(labelColor)
                    + '&textbackgroundcolor=' + encodeURIComponent(textBackgroundColor)
                    + '&textcolor=' + encodeURIComponent(textColor)
                    + '&fontsize=' + encodeURIComponent(textFontSize)
                    + '&fontfamily=' + encodeURIComponent(textFontFamilyString)
                    + '&pagewidth=320px'
                    + '&columnnumber=1';
            }

            return paymentAcceptUrl;
        }

        /**
          * Converts the given color into HEX format.
          * @param {string} color The color.
          * @return {string} The color in HEX format.
          */
        private convertToHexColor(color: string): string {
            // Return if the string is already in HEX.
            if (color.indexOf('#') != -1) return color;

            // Remove all characters except R,G,B
            color = color
                .replace("rgba", "")
                .replace("rgb", "")
                .replace("(", "")
                .replace(")", "");

            // Convert to object ["R","G","B"]
            var colorObj: any = color.split(",");

            // Add leading #, Add leading zero, so we get 0XY or 0X, Append leading zero with parsed out int value of R/G/B
            // converted to HEX string representation, Slice away 2 last chars => we get XY from 0XY and 0X stays the same
            return "#"
                + ('0' + parseInt(colorObj[0]).toString(16)).slice(-2)
                + ('0' + parseInt(colorObj[1]).toString(16)).slice(-2)
                + ('0' + parseInt(colorObj[2]).toString(16)).slice(-2);
        }

        /**
          * Pays cash exact.
          */
        private payCashExact(): void {
            RetailLogger.viewsCartCartViewPayQuickCash();
            // when adding a payment, we don't want to trigger checkout until payment conclusion
            this.executePaymentFlow(Commerce.Operations.RetailOperation.PayCashQuick);
        }

        private browseProducts() {
            Commerce.ViewModelAdapter.navigate("CategoriesView");
        }

        private voidTransaction() {
            this.handleVoidAsyncResult(this.cartViewModel.voidTransaction(), false)
                .done(() => { this.viewMode(CartViewTransactionDetailViewMode.Items); });
        }

        private voidProducts() {
            this.handleVoidAsyncResult(this.cartViewModel.voidProducts(this.cartViewModel.selectedCartLines()), false);
        }

        private voidPayment() {
            this.handleVoidAsyncResult(this.cartViewModel.voidPayment(this.cartViewModel.selectedTenderLines()), false);
        }

        private priceCheck() {
            Commerce.ViewModelAdapter.navigate("PriceCheckView");
        }

        /**
        * Open the sales tax override dialog for the transaction
        */
        private transactionTaxOverrideFromList(): void {
            this.handleVoidAsyncResult(this.cartViewModel.overrideTransactionTaxFromList());
        }

        /**
        * Override the transaction tax with the given code
        *
        * @param {string} overrideCode The override code to use
        */
        private transactionTaxOverride(overrideCode: string) {
            var taxOverride = new Proxy.Entities.TaxOverrideClass({ Code: overrideCode });
            this.handleVoidAsyncResult(this.cartViewModel.overrideTransactionTax(taxOverride));
        }

        /**
         * Open the sales tax override dialog for the selected cart line
         */
        private lineTaxOverrideFromList() {
            if (this.verifyOneItemSelected()) {
                this.handleVoidAsyncResult(this.cartViewModel.overrideLineTaxFromList());
            }
        }

        /**
         * Override the tax for the selected cart line with the given code
         *
         * @param {string} overrideCode The override code to use
         */
        private lineTaxOverride(overrideCode: string) {
            if (this.verifyOneItemSelected()) {
                var taxOverride = new Proxy.Entities.TaxOverrideClass({ Code: overrideCode });
                this.handleVoidAsyncResult(this.cartViewModel.overrideLineTax(taxOverride));
            }
        }

        /**
        * Insures that only one item is selected otherwise shows an error
        * @return {boolean} Returns true if one itm selected.
        */
        private verifyOneItemSelected(): boolean {
            var selectedItems: Proxy.Entities.CartLine[] = this.cartViewModel.selectedCartLines();

            // Check that there were selected cart lines
            if (!ArrayExtensions.hasElements(selectedItems)) {
                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.MISSING_CARTLINE_ON_APPLY_TAX_OVERRDE),
                    MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4341")); // There's no line item selected.

                return false;
            }

            // Currently only one cart line/product can be selected
            if (selectedItems.length > 1) {
                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.OPERATION_NOT_ALLOWED_MULTIPLE_CART_LINES),
                    MessageType.Info, MessageBoxButtons.Default);

                return false;
            }

            return true;
        }

        private transactionComment() {
            //TODO:AM Ankur M
            this.cartViewModel.voidProducts(this.cartViewModel.selectedCartLines());
            //this.handleVoidAsyncResult(this.cartViewModel.voidProducts(this.cartViewModel.selectedCartLines()), false);
            //this.addOrSearchProductsAndCustomers("0002");
            //this.addOrSearchProductsAndCustomers("0002");
            //this.addOrSearchProductsAndCustomers("0003");
            //this.addOrSearchProductsAndCustomers("0004");
            //this.addOrSearchProductsAndCustomers("0005");
            //this.addOrSearchProductsAndCustomers("0006");
            //this.addOrSearchProductsAndCustomers("0021");
            //this.addOrSearchProductsAndCustomers("0147");
            //this.addOrSearchProductsAndCustomers("0147");
            
            this.addOrSearchProductsAndCustomers("0003");
            this.addOrSearchProductsAndCustomers("0004");
            this.addOrSearchProductsAndCustomers("0005");
            this.addOrSearchProductsAndCustomers("0006");
            this.addOrSearchProductsAndCustomers("0021");
            this.addOrSearchProductsAndCustomers("0009");

            this.SC_PopulateKitPrice();


            //this.handleAsyncResult(this.cartViewModel.addTransactionComment());//TODO: Uncomment this line
        }

        /**
        * Set the comment on the line.
        */
        private lineComment() {
            if (CartHelper.isCartType(this.cartViewModel.cart(), Proxy.Entities.CartType.AccountDeposit)) {
                this.handleAsyncResult(this.cartViewModel.addCustomerAccountDepositComment());
            } else if (CartHelper.isCartType(this.cartViewModel.cart(), Proxy.Entities.CartType.IncomeExpense)) {
                this.handleAsyncResult(this.cartViewModel.addIncomeAccountComment());
            } else {
                var selectedProducts: Commerce.Model.Entities.CartLine[] = this.cartViewModel.selectedCartLines();

                // Check that there were selected cart lines
                if (!ArrayExtensions.hasElements(selectedProducts)) {
                    Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString('string_4423'), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4341"));
                    return;
                }

                // view model uses the selected cart lines from its observable
                this.handleAsyncResult(this.cartViewModel.addProductComments());
            }
        }

        private invoiceComment() {
            this.handleAsyncResult(this.cartViewModel.addInvoiceComment());
        }

        private onNumPadEnterEventHandler(result: Controls.NumPad.ICartNumPadResult) {
            if (!StringExtensions.isNullOrWhitespace(result.value)) {
                this.addOrSearchProductsAndCustomers(result.value, result.quantity);
            }

            this.searchText(StringExtensions.EMPTY);
        }

        private addOrSearchProductsAndCustomers(searchText: string, quantity?: number) {
            this.indeterminateWaitVisible(true);

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var processTextResult: ViewModels.IProcessTextResult;

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                return asyncQueue.cancelOn(this.cartViewModel.processText(searchText, quantity))
                    .done((dataResult: ICancelableDataResult<ViewModels.IProcessTextResult>): void => {
                        processTextResult = dataResult.data;
                    });
            }).enqueue((): IVoidAsyncResult => {
                if (processTextResult.cartUpdated) {
                    return VoidAsyncResult.createResolved();
                } else if (!ObjectExtensions.isNullOrUndefined(processTextResult.product)) {
                    var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                    productDetailOptions.recordId = processTextResult.product.RecordId;
                    productDetailOptions.quantity = quantity;
                    productDetailOptions.additionalParams = { IsKit: processTextResult.product.ProductTypeValue === Proxy.Entities.ProductType.KitMaster };
                    ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
                } else if (ArrayExtensions.hasElements(processTextResult.productSearchResults)) {
                    ViewModelAdapter.navigate("SearchView", { searchText: searchText, searchEntity: "Products", quantity: quantity });
                } else if (ArrayExtensions.hasElements(processTextResult.customers)) {
                    this.searchCustomers(searchText);
                } else {
                    // If nothing was found and the cart was not updated navigate to the search view with products in focus.
                    ViewModelAdapter.navigate("SearchView", { searchText: searchText, searchEntity: "Products", quantity: quantity });
                }

                return VoidAsyncResult.createResolved();
            });


            this.handleAsyncResult(asyncQueue.run()).done((result: ICancelableResult): void => {
                RetailLogger.viewModelCartAddProductsToCart();
            });
        }

        private searchCustomers(searchText?: string): IAsyncResult<ICancelableResult> {
            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();

            // We are not allowed to change customer in recalled order
            if (cart.CartTypeValue === Proxy.Entities.CartType.CustomerOrder
                && !StringExtensions.isNullOrWhitespace(cart.SalesId)) {
                NotificationHandler.displayErrorMessage('string_4420'); // You can not change customer on recalled order
                return;
            }

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                searchText = this.searchText();
            }

            var options: Operations.ICustomerSearchOperationOptions = {
                searchText: searchText,
                destination: undefined,
                destinationOptions: undefined,
            };

            return Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.CustomerSearch, options);
        }

        private onAddCustomerClick(sender: any, eventArgs: Event) {
            this.searchCustomers();
        }

        private onDisassembleKIT() {
            
        }

        private itemSelectionHander(items: Proxy.Entities.CartLine[]) {
            this.cartViewModel.selectedCartLines(items);
        }

        private tenderLinesSelectionChanged(tenderLines: Proxy.Entities.CartTenderLineTenderType[]) {
            this.cartViewModel.selectedTenderLines(tenderLines);
        }

        private onLineItemExpand(eventInfo: Commerce.ListView.IItemExpand) {
            ko.applyBindingsToNode(eventInfo.colspanRow, {
                template: {
                    name: "lineItemColspan",
                    data: eventInfo.data,
                    as: "originalCartLine"
                }
            }, this);
        }

        /**
         * Bind the line values to the row to expand the Customer Account Deposit
         * @param {Commerce.ListView.IItemExpand} eventInfo The event infomation that contains the row to expand and the data to bind.
         */
        private onCustomerAccountDepositLineItemExpand(eventInfo: Commerce.ListView.IItemExpand): void {
            ko.applyBindingsToNode(eventInfo.colspanRow, {
                template: {
                    name: "ShowJournalCustomerAccountDepositLineItemColspan",
                    data: eventInfo.data,
                    as: "line"
                }
            }, this);
        }

        /**
         * Checks if cart line has information to display in expandable section.
         *
         * @param {Proxy.Entities.CartLine} cartLine The cart line.
         * @return {boolean} The flag to enable or disable row expand.
         */
        private isTransactionGridRowExpandable(cartLine: Model.Entities.CartLine): boolean {
            var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            var isVariant: boolean = !ObjectExtensions.isNullOrUndefined(product) && product.ProductTypeValue === Proxy.Entities.ProductType.Variant;
            var isKitVariant: boolean = !ObjectExtensions.isNullOrUndefined(product) && product.ProductTypeValue === Proxy.Entities.ProductType.KitVariant;
            var isExpandable = isVariant ||
                isKitVariant ||
                cartLine.IsPriceOverridden ||
                /**POSHackF Hide Comment
                (cartLine.Comment && (cartLine.Comment.length > 0)) ||
                */
                (cartLine.SerialNumber && (cartLine.SerialNumber.length > 0)) ||
                ArrayExtensions.hasElements(cartLine.ReasonCodeLines) ||
                ArrayExtensions.hasElements(cartLine.DiscountLines);

            return isExpandable;
        }

        /**
         * Checks if a customer account deposit line has information to display in expandable section.
         *
         * @param {Model.Entities.CartLine} cartLine The cart line.
         * @return {boolean} The flag to enable or disable row expand.
         */
        private isCustomerAccountDepositGridRowExpandable(customerAccountDepositCartLine: Model.Entities.CustomerAccountDepositLine): boolean {
            var isExpandable: boolean = !ObjectExtensions.isNullOrUndefined(customerAccountDepositCartLine)
                && (customerAccountDepositCartLine.Comment && (customerAccountDepositCartLine.Comment.length > 0));

            return isExpandable;
        }

        /**
         * Gets the price override text for a cart line.
         *
         * @param {Proxy.Entities.CartLine} cartLine The cart line.
         * @return {string} The price override text for the cart line.
         */
        private getPriceOverrideText(cartLine: Proxy.Entities.CartLine): string {
            var priceOverrideText: string = StringExtensions.EMPTY;
            if (cartLine && cartLine.IsPriceOverridden) {
                var originalFormattedPriceText: string = NumberExtensions.formatCurrency(cartLine.OriginalPrice);
                priceOverrideText = StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4368"), originalFormattedPriceText);
            }

            return priceOverrideText;
        }

        /**
         * Change sales person on cart.
         */
        private changeSalesPerson() {
            this.executeCustomerOrderOperation(Operations.RetailOperation.SalesPerson);
        }

        private isPaymentVoided(tenderLine: Proxy.Entities.CartTenderLine): boolean {
            return tenderLine.StatusValue == Proxy.Entities.TenderLineStatus.Voided;
        }

        //#region General control methods

        /*
         * Setup steps to take when showing a control
        */
        private showControlSetup(activeControllerForPeripheralEvents?: any) {
            this.disableCartView(true);

            // Redirect events to sub controller.
            this._activeControllerForPeripheralEvents = activeControllerForPeripheralEvents;
        }

        /*
         * Steps to take when a control is closed
         *
         * @param {boolean} show True to show application bar, false to hide the application bar
        */
        private showControlClose(showAppBar?: boolean) {
            this._activeControllerForPeripheralEvents = this;
            this.disableCartView(false);
        }

        /**
         * Shows a modal dialog and handle default results.
         */
        private showDialog<T, U>(dialog: Controls.ModalDialog<T, U>, input: T): IAsyncDialogResult<U> {
            this.showControlSetup(dialog);
            return dialog.show(input)
                .on(DialogResult.OK, (result) => { this.showControlClose(); })
                .on(DialogResult.Cancel, (result) => { this.showControlClose(); })
                .onError((errors) => { this.controlErrorCallbackShowErrors(errors); });
        }

        /**
         * Handles an async result by setting the progress indicator to true and then to false on done/fail callbacks.
         *
         * @param {IAsyncResult<T>} asyncResult The async result to proccess.
         * @param {boolean} continueProcessingOnDone Whether the done callback still continues processing.
         */
        private handleAsyncResult<T>(asyncResult: IAsyncResult<T>, continueProcessingOnDone: boolean = false): IAsyncResult<T> {
            this.indeterminateWaitVisible(true);

            if (!continueProcessingOnDone) {
                asyncResult.done((result) => {
                    this.indeterminateWaitVisible(false);
                });
            }

            return asyncResult.fail((errors) => { this.errorCallBack(errors); });
        }

        /**
         * Handles an async result by setting the progress indicator to true and then to false on done/fail callbacks.
         *
         * @param {IVoidAsyncResult} asyncResult The async result to proccess.
         * @param {boolean} continueProcessingOnDone Whether the done callback still continues processing.
         * @param {boolean} isErrorHandled Set to true if Error is handled else.
         */
        private handleVoidAsyncResult(asyncResult: IVoidAsyncResult, continueProcessingOnDone: boolean = false, isErrorHandled: boolean = false): IVoidAsyncResult {
            this.indeterminateWaitVisible(true);

            if (!continueProcessingOnDone) {
                asyncResult.done(() => { this.indeterminateWaitVisible(false); });
            }

            return isErrorHandled ? asyncResult : asyncResult.fail((errors) => { this.errorCallBack(errors); });
        }

        /*
         * Runs common steps on control error.
         * Steps are:
         * Display the errors.
         * Close the control.
         *
         * @param {Proxy.Entities.Error[]} errors Errors that are returned from the control.
         */
        private controlErrorCallbackShowErrors(errors: Proxy.Entities.Error[]): void {
            // Show the errors
            Commerce.NotificationHandler.displayClientErrors(errors);

            // Close the control
            this.showControlClose();
        }
        //#endregion

        //#region Add Loyalty Card methods

        /**
         * Calls the operation to add or udpate the loyalty card
         */
        private addLoyaltyCardOperation() {
            this.handleAsyncResult(this.cartViewModel.addLoyaltyCardToCartAsync())
                .done((addLoyaltyCardResult: ICancelableResult): void => {
                    var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
                    if (!addLoyaltyCardResult.canceled && !ObjectExtensions.isNullOrUndefined(cart)) {
                        this._customerCardViewModel.customerLoyaltyCardLabel(cart.LoyaltyCardId);
                    }
                });
        }

        /**
         * Displays the dialog to select which customer loyalty card to use, and adds the selected card to the cart.
         * @param {string} cartLoyaltyCardId The current loyalty card on the transaction (Can be null or undefined).
         */
        public selectCustomerLoyaltyCardAndAddToCart(cartLoyaltyCardId: string): void {
            this._selectCustomerLoyaltyCardDialog.show({ currentLoyaltyCardId: cartLoyaltyCardId }, false /* hideOnResult */);
            this.handleLoyaltyCardDialogResult(this._selectCustomerLoyaltyCardDialog);
        }

        /**
         * Handles the result of the select customer loyalty dialog, allowing retry in case of an error at the operation.
         * @param {Controls.SelectCustomerLoyaltyCardDialog} dialog The select customer loyalty card dialog
         */
        private handleLoyaltyCardDialogResult(dialog: Controls.SelectCustomerLoyaltyCardDialog): void {
            dialog.dialogResult.on(Commerce.DialogResult.OK, (result: Commerce.Controls.ISelectCustomerLoyaltyCardDialogOutput) => {
                this._selectCustomerLoyaltyCardDialog.indeterminateWaitVisible(true);

                // Add the loyalty card to cart
                this.cartViewModel.addLoyaltyCardToCartAsync(result.selectedLoyaltyCardId)
                    .always((): void => { this._selectCustomerLoyaltyCardDialog.indeterminateWaitVisible(false); })
                    .done((addLoyaltyCardResult: ICancelableResult): void => {
                        if (!addLoyaltyCardResult.canceled) {
                            this._customerCardViewModel.customerLoyaltyCardLabel(result.selectedLoyaltyCardId);
                            this._selectCustomerLoyaltyCardDialog.hide();
                        }
                    }).fail((errors: Proxy.Entities.Error[]): void => {
                        this._selectCustomerLoyaltyCardDialog.clearResult();
                        Commerce.NotificationHandler.displayClientErrors(errors).done(() => { this._selectCustomerLoyaltyCardDialog.focus(); })
                        this.handleLoyaltyCardDialogResult(dialog);
                    });
            }).on(DialogResult.Cancel, (result: Controls.ISelectCustomerLoyaltyCardDialogOutput): void => {
                this._selectCustomerLoyaltyCardDialog.hide();
            });
        }
        //#endregion

        //#region Add discount operations

        /**
         * Adds a discount code.
         */
        private addDiscountCode() {
            this.handleAsyncResult(this.cartViewModel.addDiscountCode());
        }

        /**
         * Adds a line discount amount.
         */
        private addLineDiscountAmount(discountValue?: number) {
            this.handleAsyncResult(this.cartViewModel.addLineDiscountAmount([discountValue]));
        }

        /**
         * Adds a line discount percentage.
         */
        private addLineDiscountPercent(discountValue?: number) {
            this.handleAsyncResult(this.cartViewModel.addLineDiscountPercent([discountValue]));
        }

        /**
         * Adds a transaction discount amount.
         */
        private addTotalDiscountAmount(discountValue?: number) {
            this.handleAsyncResult(this.cartViewModel.addTransactionDiscountAmount(discountValue));
        }

        /**
         * Adds a transaction discount percentage.
         */
        private addTotalDiscountPercent(discountValue?: number) {
            this.handleAsyncResult(this.cartViewModel.addTransactionDiscountPercent(discountValue));
        }

        //#endregion

        /**
         * Calls the operation to override the price.
         */
        private priceOverrideOperation() {
            this.handleAsyncResult(this.cartViewModel.priceOverride());
        }

        //#region Set Quantity methods

        /**
         * Calls the operation to set the quantity of selected cart lines.
         */
        private setQuantityOperation() {
            this.handleAsyncResult(this.cartViewModel.setQuantities());
        }

        /**
         * Calls the operation to set the quantity of selected cart lines.
         */
        private clearQuantityOperation(): void {
            this.handleAsyncResult(this.cartViewModel.clearQuantities());
        }

        //#endregion

        //#region Unit of measure methods

        /**
         * Calls the operation to change the unit of measure of selected cart lines.
         */
        private changeUnitOfMeasureOperation() {
            this.handleAsyncResult(this.cartViewModel.changeUnitOfMeasures());
        }

        //#endregion

        //#region Return methods

        /**
         * Calls the operation to return the selected products.
         */
        private returnProductOperation() {
            var selectedCartLines: Proxy.Entities.CartLine[] = this.cartViewModel.selectedCartLines();

            // Check that there were selected cart lines
            if (!ArrayExtensions.hasElements(selectedCartLines)) {
                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.RETURN_NO_ITEM_SELECTED), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4341"));
                return;
            }

            // Currently only one cart line can be returned
            // Remove this check and validate the flow if selection for multiple cart lines is desired
            // for this operation
            if (selectedCartLines.length > 1) {
                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString(ErrorTypeEnum.OPERATION_NOT_ALLOWED_MULTIPLE_CART_LINES), MessageType.Info, MessageBoxButtons.Default);
                return;
            }

            this.cartViewModel.returnCartLines().fail((errors) => { this.errorCallBack(errors); });
        }

        //#endregion

        //#region Email methods

        /*
         * Computes whether to prompt to save email when showing the email dialog on receipts
         *
         * @returns True if the prompt should be displayed, false otherwise.
         */
        private shouldPromptToSaveEmail(): boolean {
            return this.customerDetailsVisible() && !ObjectExtensions.isNullOrUndefined(this.cartViewModel.customer());
        }

        /**
         * Concludes the transaction.
         *
         * @param {number} operationId The id of the operation concluding the transaction. Used for special processing as in IssueCreditMemo.
         * @param {CartViewController} callerContext Handle to the object containing cart view state.
         */
        private concludeTransaction(operationId: number, callerContext: CartViewController) {
            this.indeterminateWaitVisible(true);

            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreEndTransactionTriggerOptions = { cart: cart };
                var preTriggerResult: IAsyncResult<ICancelableResult> = Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreEndTransaction, preTriggerOptions);

                return asyncQueue.cancelOn(preTriggerResult);
            });

            // Set the operation of the caller context
            callerContext._operationId = operationId;

            // Check the current payments for authorized non-captured card payments
            var currentCustomer: Proxy.Entities.Customer = this.cartViewModel.customer();
            var tenderLinesToCommit: Proxy.Entities.TenderLine[] = [];
            if (ArrayExtensions.hasElements(cart.TenderLines)) {
                cart.TenderLines.forEach((tenderLine: Proxy.Entities.TenderLine) => {
                    if ((tenderLine.StatusValue === Proxy.Entities.TenderLineStatus.PendingCommit)
                        && CartHelper.isCreditOrDebitCard(tenderLine)) {
                        tenderLinesToCommit.push(tenderLine);
                    }
                });
            }

            // Capture the authorized non-captured card payments
            if (ArrayExtensions.hasElements(tenderLinesToCommit)) {
                tenderLinesToCommit.forEach((tenderLine: Proxy.Entities.TenderLine) => {
                    asyncQueue.enqueue(() => {
                        var updatedTenderLine: Proxy.Entities.TenderLine = null;
                        var makePaymentQueue: AsyncQueue = new AsyncQueue();

                        var enqueueCapture = (payment: any) => {
                            makePaymentQueue.enqueue(() => {
                                return payment.capturePayment(tenderLine.Amount, tenderLine.Authorization, null, this)
                                    .done((paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                                        if (paymentInfo && paymentInfo.IsApproved) {
                                            // Only update the authorization blob when PaymentSdkData is not empty.
                                            if (!StringExtensions.isNullOrWhitespace(paymentInfo.PaymentSdkData)) {
                                                tenderLine.Authorization = paymentInfo.PaymentSdkData;
                                            }

                                            // Set the updated tender line for updating the tender line in the cart
                                            updatedTenderLine = tenderLine;
                                        }
                                    });
                            });
                        };

                        // Capture the tender line
                        var paymentType: Proxy.Entities.PeripheralPaymentType = CartHelper.getPeripheralPaymentType(tenderLine);
                        switch (paymentType) {
                            case Proxy.Entities.PeripheralPaymentType.PaymentTerminal:
                                // Handle capture for card payment when using payment terminal.
                                enqueueCapture(Peripherals.instance.paymentTerminal);
                                break;
                            case Proxy.Entities.PeripheralPaymentType.CardPaymentController:
                                // Handle capture for card payment when not using payment terminal.
                                enqueueCapture(Peripherals.instance.cardPayment);
                                break;
                            default:
                                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERAL_PAYMENT_UNKNOWN_ERROR)]);
                                //break;
                        }

                        // Add the tender line from make payment to the cart
                        makePaymentQueue.enqueue(() => {
                            // Check whether the updatedTenderLine is valid
                            if (ObjectExtensions.isNullOrUndefined(updatedTenderLine)) {
                                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.PERIPHERAL_PAYMENT_UNKNOWN_ERROR)]);
                            }

                            // Updated tenderline on the cart with capture
                            return this.cartViewModel.updatePreprocessedTenderLineInCart(updatedTenderLine);
                        });

                        return asyncQueue.cancelOn(makePaymentQueue.run());
                    });
                });
            }

            // Set email for receipt
            asyncQueue.enqueue(() => {
                var asyncResult = new VoidAsyncResult(null);

                var emailProfileId: string = ApplicationContext.Instance.storeInformation.EmailReceiptProfileId;
                var displayPrompt: boolean = true;

                if (!ObjectExtensions.isNullOrUndefined(currentCustomer)
                    && currentCustomer.ReceiptSettings == 0 /* Print preference for receipt */) {
                    displayPrompt = false;
                }

                if (!StringExtensions.isNullOrWhitespace(emailProfileId)
                    && cart.CartTypeValue != Proxy.Entities.CartType.IncomeExpense
                    && displayPrompt) {
                    var emailReceiptDialogParams: Commerce.Controls.EmailReceiptDialogParams = {
                        emailAddress: callerContext.receiptEmailAddressInput(),
                        shouldPromptToSaveEmail: this.shouldPromptToSaveEmail(),
                        shouldSaveEmail: false
                    };

                    this.emailDialogVisible(true);

                    this.showDialog(this._emailReceiptDialog, emailReceiptDialogParams)
                        .on(DialogResult.OK, (output: Commerce.Controls.EmailReceiptDialogOutput) => {
                            this.emailDialogVisible(false);
                            this.receiptEmailAddressInput(output.emailAddress);

                            // Check that the email was set
                            if (StringExtensions.isNullOrWhitespace(this.receiptEmailAddressInput())) {
                                var errors: Proxy.Entities.Error[] = [];
                                errors.push(new Proxy.Entities.Error(ErrorTypeEnum.RECEIPT_EMAIL_IS_EMPTY));
                                asyncResult.reject(errors);
                                // Commit the email to the customer
                            } else if (output.shouldSaveEmail) {
                                this.cartViewModel.customer().ReceiptEmail = this.receiptEmailAddressInput();

                                this.handleVoidAsyncResult(this.customerViewModel.updateCustomerAsync(this.cartViewModel.customer()))
                                    .done(() => { asyncResult.resolve(); })
                                    .fail((errors) => { asyncResult.resolve(); });
                            } else {
                                asyncResult.resolve();
                            }
                        }).on(DialogResult.Cancel, () => {
                            this.emailDialogVisible(false);
                            this._emailReceiptDialog.hide().done(() => { asyncResult.resolve(); });
                        });
                } else {
                    asyncResult.resolve();
                }

                return asyncResult;
            });

            var receipts: Proxy.Entities.Receipt[];

            // Checkout the cart
            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                // If issuing a credit memo, issue credit memo for remaining balance (will automatically checkout the cart)
                if ((this._operationId === Commerce.Operations.RetailOperation.IssueCreditMemo) && this.canAddPayment()) {
                    this.disableCartView(true);
                    this.processingPayment(true);
                    this.indeterminateWaitVisible(true);

                    return this.cartViewModel.issueCreditMemoAsync(this.receiptEmailAddressInput()).map((memoReceipts): ICancelableResult => {
                        this.processingPayment(false);
                        this.indeterminateWaitVisible(false);
                        this.disableCartView(false);
                        receipts = memoReceipts
                        this.cartCheckoutSuccessCallBack(memoReceipts);
                        return { canceled: false };
                    });
                }

                // Checkout the cart
                this.indeterminateWaitVisible(true);
                var asyncResult: IAsyncResult<ICancelableResult> = this.cartViewModel.checkOutCart(this.receiptEmailAddressInput(), this._tokenizedPaymentCard, this._isPickingUpProduct)
                    .done((result: ICancelableDataResult<Proxy.Entities.Receipt[]>) => {
                        this.indeterminateWaitVisible(false);
                        this._isPickingUpProduct = false;
                        if (!result.canceled) {
                            receipts = result.data;
                            this.cartCheckoutSuccessCallBack(result.data);
                        }
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        // when checkout fails, clear the card for remaining balance so we give the user
                        // the chance to provide it again, since errors could be related to the payment card
                        this.indeterminateWaitVisible(false);
                        this._tokenizedPaymentCard = null;
                        this._isPickingUpProduct = false;
                    });

                return asyncQueue.cancelOn(asyncResult);
            });

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var postTriggerOptions: Triggers.IPostEndTransactionTriggerOptions = { receipts: receipts };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostEndTransaction, postTriggerOptions);
            });

            return asyncQueue.run()
                .fail((errors: Proxy.Entities.Error[]) => {
                    // Update the cart as it might have had the cart updated on partial success
                    this.cartViewModel.cartManager.getCartByCartIdAsync(Session.instance.cart.Id);
                    this.paymentErrorCallback(errors);
                });
        }

        //#endregion

        /*
         * Shows the dialog for printing the receipts.
         * @param {Proxy.Entities.Receipt[]} receipts The list of receipts to show in the dialog
         */
        private showPrintDialog(receipts: Proxy.Entities.Receipt[]) {
            this.indeterminateWaitVisible(true);
            var dialogState: Controls.IPrintReceiptDialogState = { receipts: receipts, rejectOnHardwareStationErrors: false };
            this.showDialog(this._printReceiptDialog, dialogState)
                .on(DialogResult.OK, (result: Controls.IPrintReceiptDialogOutput) => { this.showChangeDialog(receipts); })
                .on(DialogResult.Cancel, (result: Controls.IPrintReceiptDialogOutput) => { this.showChangeDialog(receipts); })
                .onError((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewsCartCartViewShowPrintDialogFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    this.showChangeDialog(receipts);
                })
                .onAny(() => {
                    this.indeterminateWaitVisible(false);
                });
        }

        /*
         * Shows the dialog with change information and additional gift receipt print prompting ("AsRequired" receipts).
         * @param {Proxy.Entities.Receipt[]} receipts The list of receipts to show in the dialog
         */
        private showChangeDialog(receipts: Model.Entities.Receipt[]) {
            this.receiptEmailAddressInput(null);
            this._giftReceiptsForChangeDialog = this.getGiftReceiptsForChangeDialog(receipts);
            this._displayGiftReceiptPrintOption(ArrayExtensions.hasElements(this._giftReceiptsForChangeDialog));
            this.changeDialogVisible(true);
        }

        /*
         * Retrieves the list of gift receipts to be used at Change dialog (for prompting/printing the "AsRequired" receipt)
         * @param {Proxy.Entities.Receipt[]} receipts The list of all receipts.
         * @return {Model.Entities.Receipt[]} The list of gift receipts.
         */
        private getGiftReceiptsForChangeDialog(receipts: Model.Entities.Receipt[]): Model.Entities.Receipt[] {
            var giftReceiptsWithAsRequired: Model.Entities.Receipt[] = [];

            if (!ObjectExtensions.isNullOrUndefined(receipts)) {
                giftReceiptsWithAsRequired = receipts.filter((receipt: Model.Entities.Receipt) =>
                    receipt.ReceiptTypeValue === Proxy.Entities.ReceiptType.GiftReceipt &&
                    receipt.Printers.some((printer: Proxy.Entities.Printer) =>
                        printer.PrintBehaviorValue == Model.Entities.PrintBehavior.AsRequired));
            }

            return giftReceiptsWithAsRequired;
        }

        /*
         * Hides the change dialog and starts the remaining actions when concluding the transaction.
         * @param {string} operationId The operation ID that triggered to hide change dialog.
         */
        private hideChangeDialog(operationId: string): void {
            if (operationId === Controls.Dialog.OperationIds.CLOSE_BUTTON_CLICK
                || operationId === Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK) {

                this.printGiftReceiptsIfRequired();
                this.changeDialogVisible(false);

                // When change dialog closes, show idle text on line display
                if (Peripherals.instance.lineDisplay) {
                    Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayIdleText();
                }

                this.showOrderCreatedDialog();
            }
        }

        /*
         * Prints the gift receipts marked as "AsRequired", if required.
         */
        private printGiftReceiptsIfRequired(): void {
            if (this._displayGiftReceiptPrintOption && this._printGiftReceiptToggle()) {
                // If the "as required" behavior was set to print, then change the behavior to "always print" and send it to print dialog
                this._giftReceiptsForChangeDialog.forEach((receipt: Proxy.Entities.Receipt) =>
                    receipt.Printers.forEach((printer: Proxy.Entities.Printer) => printer.PrintBehaviorValue = Proxy.Entities.PrintBehavior.Always));

                var dialogState: Controls.IPrintReceiptDialogState = { receipts: this._giftReceiptsForChangeDialog, rejectOnHardwareStationErrors: false };
                this.showDialog(this._printReceiptDialog, dialogState)
                    .onError((errors: Proxy.Entities.Error[]) => {
                        RetailLogger.viewsCartCartViewShowPrintDialogFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    });
            }
        }

        private showOrderCreatedDialog() {
            var salesOrder: Proxy.Entities.SalesOrder = this.cartViewModel.lastSalesTransaction();
            var customerOrderMode: Proxy.Entities.CustomerOrderMode = this.cartViewModel.lastCustomerOrderMode;

            switch (customerOrderMode) {

                case Proxy.Entities.CustomerOrderMode.None:
                    this.hideOrderCreatedDialog();
                    break;

                default:
                    this.handleAsyncResult(this.cartViewModel.getEmployeeNameAsync(salesOrder.StaffId))
                        .done((employeeName: string) => {
                            var dialogState: Controls.OrderCheckoutDialogState = <Controls.OrderCheckoutDialogState>{
                                salesOrder: salesOrder,
                                salesPersonName: employeeName,
                                customerOrderMode: customerOrderMode
                            };

                            this.showDialog(this._orderCheckoutDialog, dialogState)
                                .on(DialogResult.OK, () => { this.hideOrderCreatedDialog(); })
                                .onError((errors: Proxy.Entities.Error[]) => { this.hideOrderCreatedDialog(); });
                        }).fail((errors: Proxy.Entities.Error[]) => { this.hideOrderCreatedDialog(); });

                    break;
            }
        }

        private hideOrderCreatedDialog() {
            this.cartViewModel.lastSalesTransaction(new Proxy.Entities.SalesOrderClass());
            this.showControlClose();
            this.exitAfterEachTransaction();
        }


        private exitAfterEachTransaction(): void {
            if (ApplicationContext.Instance.deviceConfiguration.ExitAfterEachTransaction) {
                Operations.OperationsManager.instance.runOperation(RetailOperation.LogOff, <Operations.ILogoffOperationOptions>{});
            }
        }

        private showLinesGrid() {
            this.viewMode(Commerce.ViewControllers.CartViewTransactionDetailViewMode.Items);
        }

        private showPaymentsGrid() {
            this.viewMode(Commerce.ViewControllers.CartViewTransactionDetailViewMode.Payments);
        }

        private showDeliveryGrid() {
            this.viewMode(Commerce.ViewControllers.CartViewTransactionDetailViewMode.Delivery);
        }

        private recallTransaction() {
            this.handleVoidAsyncResult(Commerce.Operations.OperationsManager.instance.runOperation(
                Commerce.Operations.RetailOperation.RecallTransaction, null));
        }

        private suspendTransaction() {
            this.handleVoidAsyncResult(this.cartViewModel.suspendTransaction());
        }

        private paymentSuccessCallback(showPaymentGrid: boolean = true): void {
            this.cartViewModel.cart(Session.instance.cart);

            // show payment grid (instead of usual cart lines grid)
            if (showPaymentGrid) {
                this.showPaymentsGrid();
            }

            // hide any processing spinners
            this.processingPayment(false);
            this.indeterminateWaitVisible(false);
            this.disableCartView(false);

            // if we are ready for checkout, show email dialog
            if (this.cartViewModel.canCheckout) {
                this.executePaymentFlow();
            }
        }

        /**
         * Error handler for a payment error
         *
         * @param {Proxy.Entities.Error[]} errors Errors that occurred when running the operation
         */
        private paymentErrorCallback(errors: Proxy.Entities.Error[]): void {
            this.processingPayment(false);
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
            this.disableCartView(false);
        }

        /**
         * The steps to take on cart checkout success.
         *
         * @param {receipts: Proxy.Entities.Receipt[]} receipts The list of receipts that can be printed.
         */
        private cartCheckoutSuccessCallBack(receipts: Proxy.Entities.Receipt[]): void {
            this.indeterminateWaitVisible(false);

            // Open drawer if required
            if (this.cartViewModel.shouldOpenDrawer()) {
                this.openDrawer();
            }

            // show the receipts dialog.
            this.showPrintDialog(receipts);

            // Sets the cart view to show items
            this.viewMode(CartViewTransactionDetailViewMode.Items);

            // make sure we dispose of card as soon as possible
            this._tokenizedPaymentCard = null;

            // go back to default option for remaining amount authorization after checkout
            this._disableAuthorizeRemainingAmountDialog(false);
        }

        private errorCallBack(errors: Proxy.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        public paymentDialogCancelButtonClick() {
            // hide payment dialog
            this.hidePaymentDialog();

            // if we are asking for card to authorize shipping amount and user canceled it
            if (this.mustPromptToAuthorizeShippingAmount) {
                // handle cancel as no authorization provided
                this._disableAuthorizeRemainingAmountDialog(true);

                // continue with payment flow
                this.executePaymentFlow();
            }
        }

        private createCustomerOrderClickHandler(): void {
            var cart = this.cartViewModel.cart();
            var errors: Proxy.Entities.Error[];

            var queue: AsyncQueue = new AsyncQueue();
            var proceedWithCustomerOrderCreation: boolean = false;

            if (CustomerOrderHelper.isQuote(cart) && !StringExtensions.isNullOrWhitespace(cart.SalesId) && CustomerOrderHelper.isQuoteExpired(cart)) {
                // if this is a quote, then it is a quote to order conversion operation
                // if quote exists already (salesid is not empty) and is expired, we need to show user a message informing that recalculation will take place

                queue.enqueue(() => {
                    return ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4311"), MessageType.Info,
                        MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4312")).done(() => {
                            proceedWithCustomerOrderCreation = true;
                        });
                });
            } else {
                // no need to prompt user
                proceedWithCustomerOrderCreation = true;
            }

            queue.enqueue(() => {
                if (!proceedWithCustomerOrderCreation) {
                    queue.cancel();
                    return AsyncResult.createResolved();
                }

                return this.cartViewModel.createCustomerOrder();
            });

            this.handleAsyncResult(queue.run());
        }

        /**
         * Handler for create quotation operation.
         */
        private createQuotationClickHandler(): void {
            var asyncQueue = this.cartViewModel.createQuotationAndSetExpirationDateAsyncQueue();
            this.handleVoidAsyncResult(asyncQueue.run());
        }

        /**
         * Handler for set quotation expiration date operation.
         */
        private setQuotationExpirationDateClickHandler(): void {
            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();

            if (cart.CartTypeValue === Proxy.Entities.CartType.Shopping) {
                // If cart type is shopping cart, ask user if they want to convert cart to become quotation.

                var activity: Activities.GetOrderTypeActivity =
                    new Activities.GetOrderTypeActivity({ operationId: Operations.RetailOperation.SetQuotationExpirationDate });
                activity.execute().done(() => {
                    if (!activity.response) {
                        return;
                    } else {
                        this.createQuotationClickHandler();
                    }
                });
            } else {
                this.handleVoidAsyncResult(this.cartViewModel.setQuotationExpirationDate());
            }
        }

        /**
         * Handler for get payments history operation.
         */
        private paymentsHistoryHandler(): void {
            var errors: Proxy.Entities.Error[] = Operations.Validators.paymentsHistoryOperationValidator(
                this.cartViewModel.cart());

            if (ArrayExtensions.hasElements(errors)) {
                NotificationHandler.displayClientErrors(errors);
            } else {
                ViewModelAdapter.navigate("PaymentHistoryView");
            }
        }

        /**
         * Calculates all amounts on cart.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public calculateTotalAsync(): IVoidAsyncResult {
            return this.cartViewModel.calculateTotalAsync()
                .fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors, "string_4374"); // Calculate total
                });
        }

        /**
         * Recalculate all amounts on cart.
         */
        public recalculateOrder(): void {
            this.executeCustomerOrderOperation(Operations.RetailOperation.RecalculateCustomerOrder);
        }

        private executeCustomerOrderOperation(operationEnum: Operations.RetailOperation): void {

            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
            var selectedCartLines: Proxy.Entities.CartLine[] = this.cartViewModel.selectedCartLines();

            if (CartHelper.areAllCartLinesSelected(cart, selectedCartLines)) {
                // set ship operation to ship or pick up all
                // if user selects all product lines on cart.

                if (operationEnum === Operations.RetailOperation.ShipSelectedProducts) {
                    operationEnum = Operations.RetailOperation.ShipAllProducts;
                } else if (operationEnum === Operations.RetailOperation.PickupSelectedProducts) {
                    operationEnum = Operations.RetailOperation.PickupAllProducts;
                }
            }

            if (operationEnum === Operations.RetailOperation.ShipAllProducts
                || operationEnum === Operations.RetailOperation.PickupAllProducts) {

                selectedCartLines = CartHelper.GetNonVoidedCartLines(cart.CartLines);
            }

            var asyncQueue = new AsyncQueue();
            asyncQueue
                .enqueue(() => {
                    var operationResult = this.cartViewModel.customerOrderPreExecuteOperation(
                        selectedCartLines, operationEnum);
                    return asyncQueue.cancelOn(operationResult);
                }).enqueue(() => {
                    this.cartViewModel.cart(Session.instance.cart);

                    var asyncResult: IVoidAsyncResult = VoidAsyncResult.createResolved();

                    switch (operationEnum) {
                        case Operations.RetailOperation.ShipAllProducts:
                        case Operations.RetailOperation.ShipSelectedProducts:
                            this.navigateToShipMethodsView(selectedCartLines);
                            break;
                        case Operations.RetailOperation.PickupAllProducts:
                        case Operations.RetailOperation.PickupSelectedProducts:
                            this.navigateToPickupMethodsView(selectedCartLines);
                            break;
                        case Operations.RetailOperation.RecalculateCustomerOrder:
                            asyncResult = this.cartViewModel.recalculateCustomerOrder();
                            break;
                        case Operations.RetailOperation.SalesPerson:
                            asyncResult = this.cartViewModel.changeSalesPerson();
                            break;
                    }

                    return asyncResult;
                });

            this.handleVoidAsyncResult(asyncQueue.run());
        }

        public shipAll(): void {
            this.executeCustomerOrderOperation(Operations.RetailOperation.ShipAllProducts);
        }

        public shipSelected(): void {
            this.executeCustomerOrderOperation(Operations.RetailOperation.ShipSelectedProducts);
        }

        public pickUpAll(): void {
            this.executeCustomerOrderOperation(Operations.RetailOperation.PickupAllProducts);
        }

        public pickUpSelected(): void {
            this.executeCustomerOrderOperation(Operations.RetailOperation.PickupSelectedProducts);
        }

        private navigateToShipMethodsView(cartLines: Proxy.Entities.CartLine[]): void {
            Commerce.ViewModelAdapter.navigate("ShippingMethodsView", {
                cartLines: cartLines
            });
        }

        private navigateToPickupMethodsView(cartLines: Proxy.Entities.CartLine[]): void {
            Commerce.ViewModelAdapter.navigate(
                "PickUpInStoreView", {
                    isForPickUp: true,
                    cartLines: cartLines
                });
        }

        private barcodeScannerHandler(barcode: string) {
            this.addOrSearchProductsAndCustomers(barcode);
        }

        /**
         * Handles the card swipe.
         *
         * @param {Proxy.Entities.CardInfo} [cardInfo] The card information.
         */
        private magneticStripeReaderHandler(cardInfo?: Proxy.Entities.CardInfo) {
            // show error message if there are any payment issues
            if (!this.displayWarningIfUnableToPay()) {
                var isMasked: boolean = PaymentHelper.isCardNumberMasked(cardInfo.CardNumber);
                if (this.canAddPayment() && isMasked) {
                    // Get payment card.
                    var cardTenderType: Proxy.Entities.TenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCard);
                    var paymentViewModel: ViewModels.PaymentViewModel = new ViewModels.PaymentViewModel(cardTenderType);
                    paymentViewModel.updateCardInfo(cardInfo, Proxy.Entities.CardSource.MSR);
                    this.payCard(cardInfo, Proxy.Entities.CardSource.MSR);
                } 
            }
        }

        private depositOverride(): void {
            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
            if (cart.CartTypeValue == Proxy.Entities.CartType.CustomerOrder
                && (cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                    || cart.CustomerOrderModeValue == Proxy.Entities.CustomerOrderMode.Pickup)) {
                Commerce.ViewModelAdapter.navigate("DepositOverrideView");
            } else {
                NotificationHandler.displayErrorMessage('string_4602'); //Deposit override is only available for new orders, pick-up orders, or when editing an existing order."
            }
        }

        private giftCardAction(giftCardMode: Controls.GiftCardMode): void {
            var cart: Proxy.Entities.Cart = this.cartViewModel.cart();
            if (cart.CartTypeValue === Proxy.Entities.CartType.CustomerOrder) {
                NotificationHandler.displayErrorMessage('string_29023') // Gift cards cannot be included in customer orders or quotations.
            } else {
                this.showDialog(this._addIssueGiftCardDialog, giftCardMode);
            }
        }

        public issueGiftCard(): void {
            this.giftCardAction(Controls.GiftCardMode.IssueNew);
        }

        public addGiftCard(): void {
            this.giftCardAction(Controls.GiftCardMode.AddExisting);
        }

        public giftCardBalance(): void {
            // This needs to go through the operation pipeline.
            var asyncResult: IAsyncResult<ICancelableResult> = Commerce.Operations.OperationsManager.instance.runOperation(
                Commerce.Operations.RetailOperation.GiftCardBalance,
                <Operations.IGiftCardBalanceOperationOptions>{
                    callerContext: this,
                    giftCardFunction: () => { this.giftCardAction(Controls.GiftCardMode.CheckBalance); }
                });

            this.handleAsyncResult(asyncResult);
        }

        public removeCustomer(): void {
            this.handleVoidAsyncResult(this.cartViewModel.removeCustomerFromCart());
        }

        /**
         * Handles customer account deposit validation and execution.
         */
        public customerAccountDeposit(): void {
            this.handleVoidAsyncResult(this.cartViewModel.customerAccountDeposit(), false);
        }

        public operationsButtonGridClick(operationId: number, actionProperty: string): boolean {
            switch (operationId) {
                case Commerce.Operations.RetailOperation.DiscountCodeBarcode:
                    this.addDiscountCode();
                    return true;
                case Commerce.Operations.RetailOperation.ClearQuantity:
                    this.clearQuantityOperation();
                    return true;
                case Commerce.Operations.RetailOperation.SetQuantity:
                    this.setQuantityOperation();
                    return true;
                case Commerce.Operations.RetailOperation.ChangeUnitOfMeasure:
                    this.changeUnitOfMeasureOperation();
                    return true;
                case Commerce.Operations.RetailOperation.VoidItem:
                    RetailLogger.viewModelCartVoidProductsStarted();
                    this.voidProducts();
                    return true;
                case Commerce.Operations.RetailOperation.VoidPayment:
                    this.voidPayment();
                    return true;
                case Commerce.Operations.RetailOperation.ReturnItem:
                    this.returnProductOperation();
                    return true;
                case Commerce.Operations.RetailOperation.TransactionComment:
                    this.transactionComment();
                    return true;
                case Commerce.Operations.RetailOperation.ItemComment:
                    this.lineComment();
                    return true;
                case Commerce.Operations.RetailOperation.LoyaltyRequest:
                    this.addLoyaltyCardOperation();
                    return true;
                case Commerce.Operations.RetailOperation.IssueCreditMemo:
                    // Do the pre-operation checks on IssueCreditMemo
                    var errors: Proxy.Entities.Error[] = Commerce.Operations.IssueCreditMemoOperationHelper.preOperationValidation(this.cartViewModel.cart());
                    if (errors != null) {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    } else {
                        this.concludeTransaction(operationId, this);
                    }

                    return true;
                case Commerce.Operations.RetailOperation.LoyaltyIssueCard:
                    Commerce.Operations.OperationsManager.instance.runOperation(
                        operationId,
                        <Operations.IIssueLoyaltyCardOperationOptions>{ customer: this.cartViewModel.customer() });
                    return true;
                case Commerce.Operations.RetailOperation.IssueGiftCertificate:
                    this.issueGiftCard();
                    return true;
                case Commerce.Operations.RetailOperation.AddToGiftCard:
                    this.addGiftCard();
                    return true;
                case Commerce.Operations.RetailOperation.GiftCardBalance:
                    this.giftCardBalance();
                    return true;
                case Commerce.Operations.RetailOperation.VoidTransaction:
                    this.voidTransaction();
                    return true;
                case Commerce.Operations.RetailOperation.InvoiceComment:
                    this.invoiceComment();
                    return true;
                case Commerce.Operations.RetailOperation.OverrideTaxTransaction:
                    this.transactionTaxOverride(actionProperty);
                    return true;
                case Commerce.Operations.RetailOperation.OverrideTaxTransactionList:
                    this.transactionTaxOverrideFromList();
                    return true;
                case Commerce.Operations.RetailOperation.OverrideTaxLine:
                    this.lineTaxOverride(actionProperty);
                    return true;
                case Commerce.Operations.RetailOperation.OverrideTaxLineList:
                    this.lineTaxOverrideFromList();
                    return true;
                case Commerce.Operations.RetailOperation.SuspendTransaction:
                    this.suspendTransaction();
                    return true;
                case Commerce.Operations.RetailOperation.RecallTransaction:
                    this.recallTransaction();
                    return true;
                case Commerce.Operations.RetailOperation.SalesInvoice:
                    // No implementation found
                    return false;
                case Commerce.Operations.RetailOperation.PriceCheck:
                    this.priceCheck();
                    return true;
                case Commerce.Operations.RetailOperation.CreateCustomerOrder:
                    this.createCustomerOrderClickHandler();
                    return true;
                case Commerce.Operations.RetailOperation.CreateQuotation:
                    this.createQuotationClickHandler();
                    return true;
                case Commerce.Operations.RetailOperation.SetQuotationExpirationDate:
                    this.setQuotationExpirationDateClickHandler();
                    return true;
                case Commerce.Operations.RetailOperation.CalculateFullDiscounts:  // CalculateTotal
                    this.calculateTotalAsync();
                    return true;
                case Commerce.Operations.RetailOperation.RecalculateCustomerOrder:
                    this.recalculateOrder();
                    return true;
                case Commerce.Operations.RetailOperation.SalesPerson:
                    this.changeSalesPerson();
                    return true;
                case Commerce.Operations.RetailOperation.ShipSelectedProducts:
                    this.shipSelected();
                    return true;
                case Commerce.Operations.RetailOperation.ShipAllProducts:
                    this.shipAll();
                    return true;
                case Commerce.Operations.RetailOperation.PickupSelectedProducts:
                    this.pickUpSelected();
                    return true;
                case Commerce.Operations.RetailOperation.PickupAllProducts:
                    this.pickUpAll();
                    return true;
                case Commerce.Operations.RetailOperation.PaymentsHistory:
                    this.paymentsHistoryHandler();
                    return true;
                case Commerce.Operations.RetailOperation.CustomerClear:
                    this.removeCustomer();
                    return true;
                case Commerce.Operations.RetailOperation.DepositOverride:
                    this.depositOverride();
                    return true;
                case Commerce.Operations.RetailOperation.PayCashQuick:
                    this.payCashExact();
                    return true;
                case Commerce.Operations.RetailOperation.PayCash:
                case Commerce.Operations.RetailOperation.PayGiftCertificate:
                case Commerce.Operations.RetailOperation.PayCurrency:
                case Commerce.Operations.RetailOperation.PayCustomerAccount:
                case Commerce.Operations.RetailOperation.PayCheck:
                case Commerce.Operations.RetailOperation.PayLoyalty:
                case Commerce.Operations.RetailOperation.PayCreditMemo:
                    var tenderType = null;
                    if (!StringExtensions.isNullOrWhitespace(actionProperty)) {
                        tenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderByTypeId(actionProperty);
                    }
                    this.executePaymentFlow(operationId, tenderType);

                    return true;
                case Commerce.Operations.RetailOperation.PayCard:
                    this.payCard();
                    return true;
                case Commerce.Operations.RetailOperation.TotalDiscountAmount:
                    {
                        var discountValue: number = NumberExtensions.parseNumber(actionProperty);
                        discountValue = isNaN(discountValue) ? undefined : discountValue;
                        this.addTotalDiscountAmount(discountValue);
                    }
                    return true;
                case Commerce.Operations.RetailOperation.TotalDiscountPercent:
                    {
                        var discountValue = parseFloat(actionProperty);
                        discountValue = isNaN(discountValue) ? undefined : discountValue;
                        this.addTotalDiscountPercent(discountValue);
                    }
                    return true;
                case Commerce.Operations.RetailOperation.LineDiscountAmount:
                    {
                        var discountValue: number = NumberExtensions.parseNumber(actionProperty);
                        discountValue = isNaN(discountValue) ? undefined : discountValue;
                        this.addLineDiscountAmount(discountValue);
                    }
                    return true;
                case Commerce.Operations.RetailOperation.LineDiscountPercent:
                    {
                        var discountValue = parseFloat(actionProperty);
                        discountValue = isNaN(discountValue) ? undefined : discountValue;
                        this.addLineDiscountPercent(discountValue);
                    }
                    return true;
                case Commerce.Operations.RetailOperation.PriceOverride:
                    this.priceOverrideOperation();
                    return true;
                case Commerce.Operations.RetailOperation.CustomerSearch:
                    this.searchCustomers();
                    return true;
                case Commerce.Operations.RetailOperation.TimeRegistration:
                    Commerce.Operations.OperationsManager.instance.runOperation(operationId, this);
                    return true;
                case Commerce.Operations.RetailOperation.ItemSale:
                    this.addOrSearchProductsAndCustomers(actionProperty);
                    return true;
                case Commerce.Operations.RetailOperation.AddAffiliation:
                    if (!StringExtensions.isNullOrWhitespace(actionProperty)) {
                        var affiliationNames: string[] = actionProperty.split(";");
                        var options: Operations.IAddAffiliationOperationOptions = { affiliationNames: affiliationNames, affiliations: [] };
                        var operationResult: IAsyncResult<ICancelableResult> = Operations.OperationsManager.instance.runOperation(operationId, options);

                        this.handleAsyncResult(operationResult);
                    }
                    return true;
                case Commerce.Operations.RetailOperation.CustomerEdit:
                    if (this.isCustomerAddedToSale()) {
                        Commerce.Operations.OperationsManager.instance.runOperation(
                            operationId,
                            <Operations.ICustomerEditOperationOptions>{
                                customer: new Proxy.Entities.CustomerClass({ AccountNumber: this.cartViewModel.customer().AccountNumber }),
                                destination: "CartView",
                                destinationOptions: null
                            });
                    } else {
                        NotificationHandler.displayErrorMessage('string_4371'); // Add a customer to the transaction before performing this operation.
                    }
                    return true;
                case Commerce.Operations.RetailOperation.CustomerAccountDeposit:
                    this.customerAccountDeposit();
                    return true;
                case Commerce.Operations.RetailOperation.PackSlip:
                case Commerce.Operations.RetailOperation.EditCustomerOrder:
                case Commerce.Operations.RetailOperation.EditQuotation:
                    this.redirectOperationProcessing();
                    return true;
/* BEGIN SDKSAMPLE_CROSSLOYALTY (do not remove this)
                case Custom.Entities.RetailOperationEx.AddCrossLoyaltyCard:
                    var addCrossLoyaltyCardOperationParameters: string[] = actionProperty.split(";");
                    var addCrossLoyaltyCardOperationOptions: Custom.Operations.IAddCrossLoyaltyCardOperationOptions = {
                        cardNumber: addCrossLoyaltyCardOperationParameters.shift()
                    };

                    Commerce.Operations.OperationsManager.instance.runOperation(operationId, addCrossLoyaltyCardOperationOptions)
                        .done((result: Operations.IOperationResult) => {
                            if (!result.canceled) {
                                this.cartViewModel.cart(Session.instance.cart);
                            }
                        }).fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
                    return true;
   END SDKSAMPLE_CROSSLOYALTY (do not remove this) */

                //POSHackF
                case 9001: //Change order status  
                    this.SC_ChangeOrderStatusOperation(operationId);
                    return true;
                case 9002:
                    this.SC_ChangeOrderStatusOperation(operationId);
                    return true;
                case 9003:
                    this.SC_AddDeliveryInstruction(operationId);
                    return true;
                //POSHackF END
                default:
                    return Commerce.Operations.DefaultButtonGridHandler.handleOperation(operationId, actionProperty, this.indeterminateWaitVisible);
            }
        }

        /**
         * Inform the user that operation might be run from different location.
         */
        private redirectOperationProcessing(): void {
            var dialogResult: IAsyncResult<DialogResult> = ViewModelAdapter
                .displayMessage(ViewModelAdapter.getResourceString("string_4583"), MessageType.Info, MessageBoxButtons.YesNo, null, 0);

            dialogResult.done((result: DialogResult): void => {
                if (result === DialogResult.Yes) {
                    var criteria: Proxy.Entities.SalesOrderSearchCriteria = null;
                    if (!ObjectExtensions.isNullOrUndefined(this.cartViewModel.customer())
                        && !ObjectExtensions.isNullOrUndefined(this.cartViewModel.customer().AccountNumber)) {
                        criteria = new Proxy.Entities.SalesOrderSearchCriteriaClass();
                        criteria.CustomerAccountNumber = this.cartViewModel.customer().AccountNumber;
                    }
                    ViewModelAdapter.navigate("SearchOrdersView", criteria);
                }
            });
        }

        /**
         * Opens the cash drawer.
         */
        private openDrawer(): void {
            Peripherals.instance.cashDrawer.openAsync()
                .fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }
    }
}
