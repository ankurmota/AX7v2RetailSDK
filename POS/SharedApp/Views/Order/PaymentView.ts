/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    import CartLine = Commerce.Proxy.Entities.CartLine;
    "use strict";

    /**
     * Represents the card payment accept message.
     */
    export interface CardPaymentAcceptMessage {
        type: string;
        value: string;
    }

    /**
     * Represents the payment view controller.
     */
    export class PaymentViewController extends ViewControllerBase {
        // view models
        public paymentViewModel: Commerce.ViewModels.PaymentViewModel;
        public cartViewModel: Commerce.ViewModels.CartViewModel;

        // state properties
        public indeterminateWaitVisible: Observable<boolean>;
        public isEnteringAmount: Observable<boolean>;
        public isSwipeCard: Computed<boolean>;
        public isManualCardEntry: Computed<boolean>;
        public isApprovalCode: Computed<boolean>;
        public isPaymentTerminalDialogVisible: Observable<boolean>;
        public isPinPadDialogVisible: Observable<boolean>;
        public isPinPadCancelledDialogVisible: Observable<boolean>;
        public isCardPaymentAcceptPage: Observable<boolean>;
        public isPaymentAcceptDisclaimerDialogVisible: Observable<boolean>;
        private _cardInputOptions: ObservableArray<ViewModels.ValueCaptionPair>;
        private _cardInputOption: Observable<number>;

        // payment type properties
        public isTypeCashPayment: Computed<boolean>;
        public displayCurrencies: Computed<boolean>;
        public toggleShowHideCurrencyCodeFlyout: Observable<any>;
        public hideCurrencyCodeFlyout: Observable<any>;
        public showCurrencyCodeFlyout: Observable<any>;
        public forceLayoutForeignCurrencyListView: Observable<any>;
        public selectTotalAmountInput: Observable<any>;
        private _setFocusCurrencyList: Observable<boolean>;
        public currencyConversionString: Computed<string>;
        public showDenominations: Computed<boolean>;
        public showCurrencyDenominations: Computed<boolean>;
        public tokenizeCard: Observable<boolean>;

        // Card payment properties.
        public cardPaymentAcceptPageUrl: Observable<string>;
        public cardPaymentAcceptPageSubmitUrl: string;
        public cardPaymentAcceptMessageOrigin: string;
        public cardPaymentAcceptMessageHandlerProxied: any;
        public cardPaymentAcceptCardTypeInfo: Commerce.Model.Entities.CardTypeInfo;
        public cardPaymentAcceptCardPrefix: string;
        public cardPaymentAcceptResultAccessCode: string;
        public isCardPaymentAcceptSwipeSent: boolean;
        public isCardPaymentAcceptHeightReceived: boolean;

        public showPaymentDetails: Computed<boolean>;
        public showGiftCardBalance: Observable<boolean>;
        public showCreditMemoBalance: Observable<boolean>;
        public showAccountDetails: Computed<boolean>;
        public customerFullAddress: Computed<string>;
        public customerType: Computed<string>;
        public isDebitCard: Observable<boolean>;
        public amountDue: Observable<string>;
        private _isViewShown: boolean;
        private _hasForeignCurrencyDropDownBeenDisplayed: boolean;

        // common header properties
        public commonHeaderData: Commerce.Controls.CommonHeaderData;

        // private fields
        private _preloadPaymentCard: boolean;
        private _processingPayment: boolean;

        // Denomination field block size (in pixels)
        public static denominationBlockSize: number = 120;

        private _options: Commerce.Operations.IPaymentOperationOptions;

        private _customerCardViewModel: Commerce.ViewModels.CustomerCardViewModel;

        private _keyPressHandler: (ev: KeyboardEvent) => void;

        // Dialogs
        private _approvePartialAmountDialog: Controls.ApprovePartialAmountDialog;
        private _cardTypeDialog: Controls.CardTypeDialog;
        private _signatureCaptureDialog: Controls.SignatureCaptureDialog;
        private _signatureDeviceDialog: Controls.SignatureDeviceDialog;
        private _cashbackDialog: Controls.CashbackDialog;

        private static CARDPAYMENTACCEPTSWIPE: string = "msax-cc-swipe";
        private static CARDPAYMENTACCEPTPARTIALOK: string = "msax-cc-partialok";
        private static CARDPAYMENTACCEPTAMOUNT: string = "msax-cc-amount";
        private static CARDPAYMENTACCEPTSUBMIT: string = "msax-cc-submit";
        private static CARDPAYMENTACCEPTERROR: string = "msax-cc-error";
        private static CARDPAYMENTACCEPTPARTIALAMOUNT: string = "msax-cc-partialamount";
        private static CARDPAYMENTACCEPTRESULT: string = "msax-cc-result";
        private static CARDPAYMENTACCEPTHEIGHT: string = "msax-cc-height";
        private static CARDPAYMENTACCEPTCARDPREFIX: string = "msax-cc-cardprefix";

        /**
         * Instantiates the payment view controller.
         *
         * @param {IPaymentViewControllerOptions} options - instantiation option.
         */
        constructor(options: Commerce.Operations.IPaymentOperationOptions) {
            super(true);

            if (options == null || options.tenderType == null) {
                throw 'Invalid argument exception';
            }

            this._options = options;

            // if not provided, default value to false
            this._options.tokenizeCard = this._options.tokenizeCard || false;
            this.tokenizeCard = ko.observable(this._options.tokenizeCard);

            // card payment accept page properties.
            this.cardPaymentAcceptPageUrl = ko.observable(this._options.cardPaymentAcceptPageUrl);
            this.cardPaymentAcceptPageSubmitUrl = this._options.cardPaymentAcceptPageSubmitUrl;
            this.cardPaymentAcceptMessageOrigin = this._options.cardPaymentAcceptMessageOrigin;
            this.isCardPaymentAcceptPage = ko.observable(!StringExtensions.isNullOrWhitespace(this._options.cardPaymentAcceptPageUrl));
            this.cardPaymentAcceptMessageHandlerProxied = $.proxy(this.cardPaymentAcceptMessageHandler, this);
            this.cardPaymentAcceptResultAccessCode = null;

            this.cartViewModel = new Commerce.ViewModels.CartViewModel();
            this.paymentViewModel = this.createPaymentViewModel(this._options);

            // customer card data object to use for binding data to the customer card
            this._customerCardViewModel = new Commerce.ViewModels.CustomerCardViewModel(<ViewModels.ICustomerCardViewModelOptions>{
                parentView: "PaymentView",
                passThroughOptions: this._options,
                isLoyaltyCardDataReadOnly: true
            });

            if (!ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.cart)) {
                this.cartViewModel.customer.subscribe((newValue) => { this._customerCardViewModel.customer(newValue); });
                this.cartViewModel.customerLoyaltyCards.subscribe((newValue) => { this._customerCardViewModel.customerLoyaltyCards(newValue); });
                this.paymentViewModel.customerId(Commerce.Session.instance.cart.CustomerId);
            }

            if (!ObjectExtensions.isNullOrUndefined(this._options.paymentCardInfo)) {
                this.paymentViewModel.updateCardInfo(this._options.paymentCardInfo, this._options.paymentCardSource);
                this._preloadPaymentCard = true;
            }

            this.indeterminateWaitVisible = ko.observable(false);
            this._processingPayment = false;

            this.isPaymentTerminalDialogVisible = ko.observable(false);
            this.isPinPadDialogVisible = ko.observable(false);
            this.isPinPadCancelledDialogVisible = ko.observable(false);
            this.isPaymentAcceptDisclaimerDialogVisible = ko.observable(false);

            this._cardInputOptions = ko.observableArray<ViewModels.ValueCaptionPair>(this.getCardInputOptions());
            this._cardInputOption = ko.observable(0);
            this._cardInputOption.subscribe(this.clearCardFields, this);

            this.isSwipeCard = ko.computed(() => { return this._cardInputOption() == 0; }, this);
            this.isManualCardEntry = ko.computed(() => { return this._cardInputOption() == 1; }, this);
            this.isApprovalCode = ko.computed(() => { return this._cardInputOption() == 2; }, this);

            // Default is swipe card
            this._cardInputOption(0);

            // The currency code flyout can't bind to the element until the element is displayed.
            // Set the value of _hasForeignCurrencyDropDownBeenDisplayed to force the element to display.
            // This is done in the constructor as the knockout computation for the displaying currencies happens after
            // the page is rendered/shown.
            this._hasForeignCurrencyDropDownBeenDisplayed = this._options.tenderType.OperationId !== Operations.RetailOperation.PayCurrency;

            this.isTypeCashPayment = ko.computed(this.computeIsTypeCashPayment, this);
            this.displayCurrencies = ko.computed(this.computeShowCurrencies, this);
            this.toggleShowHideCurrencyCodeFlyout = ko.observable(() => { });
            this.hideCurrencyCodeFlyout = ko.observable(() => { });
            this.showCurrencyCodeFlyout = ko.observable(() => { });
            this.forceLayoutForeignCurrencyListView = ko.observable(() => { });
            this.selectTotalAmountInput = ko.observable(() => { });
            this._setFocusCurrencyList = ko.observable(false);
            this.currencyConversionString = ko.computed(this.computeCurrencyConversionString, this);
            this.showDenominations = ko.computed(this.computeShowDenominations, this);
            this.showCurrencyDenominations = ko.computed(this.computeShowCurrencyDenominations, this);
            this.isCardPaymentAcceptSwipeSent = false;
            this.isCardPaymentAcceptHeightReceived = false;

            this.showPaymentDetails = ko.computed(this.computeShowPaymentDetails, this);

            this.showGiftCardBalance = ko.observable(false);
            this.showCreditMemoBalance = ko.observable(false);
            this.showAccountDetails = ko.computed(this.computeShowAccountDetails, this);
            this.customerFullAddress = ko.computed(this.computeCustomerFullAddress, this);
            this.customerType = ko.computed(this.computeCustomerType, this);
            this.isDebitCard = ko.observable(false);
            this._isViewShown = false;

            // Creates and adds dialogs to the page
            this.addControl(this._approvePartialAmountDialog = new Controls.ApprovePartialAmountDialog());
            this.addControl(this._cardTypeDialog = new Controls.CardTypeDialog());
            this.addControl(this._signatureCaptureDialog = new Controls.SignatureCaptureDialog());
            this.addControl(this._signatureDeviceDialog = new Controls.SignatureDeviceDialog());
            this.addControl(this._cashbackDialog = new Controls.CashbackDialog());

            // Parse keyboard input
            this._keyPressHandler = (e) => { this.handleKeyPress(e); };

            // load common header 
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_108"));  // NEW SALE
            this.commonHeaderData.categoryName(this.paymentViewModel.tenderTypeName());                     // Cards
        }

        /**
         * Called on load of view controller.
         */
        public load(): void {
            this._isViewShown = false;
            this.paymentViewModel.load().fail((errors: Model.Entities.Error[]) => { this.failedLoad(errors); });

            // If card payment accept page is being used, add a listener for the messages from the page.
            if (this.isCardPaymentAcceptPage()) {
                this.addCardPaymentAcceptListener();

                // Setting focus inside of the iframe by default.
                var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                cardPaymentAcceptIframe.contentWindow.focus();
            }

            if (this._options.tenderType.OperationId == Operations.RetailOperation.PayCustomerAccount || this._options.tenderType.OperationId == Operations.RetailOperation.PayLoyalty) {
                this.handleVoidAsyncResult(this.cartViewModel.getCustomerDetails(Commerce.Session.instance.cart.CustomerId))
                    .done(() => {
                        var customer = this.cartViewModel.customer();
                        if (customer) {
                            if (!StringExtensions.isNullOrWhitespace(customer.InvoiceAccount)) {
                                this.paymentViewModel.customerId(customer.InvoiceAccount);
                            }
                        }
                    });
            }

            //TODO:AM
            var currentCart1 = this.cartViewModel.cart();
            var originalAmount1 = NumberExtensions.roundToNDigits(currentCart1.DiscountAmount + currentCart1.TotalAmount, 2);
            this.paymentViewModel.originalAmount(originalAmount1);
            this.paymentViewModel.originalAmountTextAsCurrency(NumberExtensions.formatCurrency(originalAmount1,"USD"));
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

            return isErrorHandled ? asyncResult : asyncResult.fail((errors) => { this.paymentErrorCallback(errors); });
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            Commerce.Peripherals.instance.magneticStripeReader.enableAsync(
                (cardInfo: Commerce.Model.Entities.CardInfo) => {
                    var isMasked: boolean = PaymentHelper.isCardNumberMasked(cardInfo.CardNumber);

                    if (this.isCardPaymentAcceptPage()) {
                        var swipe: string = cardInfo.Track1 + cardInfo.Track2;
                        this.sendCardPaymentAcceptSwipeMessage(swipe);
                    } else if (!this.paymentViewModel.isCardPayment || (!this.isManualCardEntry() && isMasked)) {
                        // Update card info for other tender types, e.g. gift card, loyalty card.
                        this.paymentViewModel.updateCardInfo(cardInfo, Model.Entities.CardSource.MSR);
                        this.paymentViewModel.setPaymentCardType(null);
                    } else {
                        NotificationHandler.displayErrorMessage('string_7207'); // Bank card swipe is not allowed. Please swipe your bank card on the payment terminal.
                    }
                }).done(() => {
                    document.addEventListener("keypress", this._keyPressHandler);
                });

            Commerce.Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                this.scannerMessageEventHandler(barcode);
            });

            // Get the preloaded payment card if needed
            if (this._preloadPaymentCard && !this.paymentViewModel.isCardTypeSet()) {
                this.indeterminateWaitVisible(true);
                this.getCardTypeAsync().done(() => {
                    this.indeterminateWaitVisible(false);
                    this._isViewShown = true;
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    this._isViewShown = true;
                    Commerce.NotificationHandler.displayClientErrors(errors)
                        .done(() => {
                            Commerce.ViewModelAdapter.navigateBack();
                        }).fail(() => {
                            Commerce.ViewModelAdapter.navigateBack();
                        });
                });
            } else {
                this._isViewShown = true;
            }
        }

        /**
         * Called after view is bound.
         */
        public afterBind(): void {
            if (!this.computeIsCurrencyPayment()) {
                this.selectTotalAmountInput()();
            } else {
                this.showCurrencyCodeFlyout()();
            }
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            document.removeEventListener("keypress", this._keyPressHandler);
            Commerce.Peripherals.instance.magneticStripeReader.disableAsync();
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();

            // If card payment accept page is used, detach the event handler that listens to messages from the page.
            if (this.isCardPaymentAcceptPage()) {
                this.removeCardPaymentAcceptListener();
            }

            this._isViewShown = false;
        }

        /**
         * Action to take if the load fails.
         *
         * @param {Model.Entities.Error} error The error information
         */
        private failedLoad(errors: Model.Entities.Error[]): void {
            // Only error on load is for getCurrencyInfo that only happens on cash or currency
            // For cash, silently fail as the denominations won't show but the rest of the functionality will work
            // For currency, throw an error message and continue as "cash"
            if (this.computeIsCurrencyPayment()) {
                NotificationHandler.displayErrorMessage("string_1143");
            }
        }

        /**
         * Hides the application bar.
         */
        private static hideAppBar(): void {
            $('#commandAppBar').hide();
        }

        /**
         * Action to take after the foreign currency drop down is displayed.
         * On first display, need to force redraw of the list view to get it the list view to display in the flip view
         *
         * @param {any} eventInfo The event information
         */
        private afterForeignCurrencyDropDownIsDisplayed(eventInfo?: any): void {
            if (!this._hasForeignCurrencyDropDownBeenDisplayed) {
                this._hasForeignCurrencyDropDownBeenDisplayed = true;
            }
        }

        /**
        * Get the minimum/default size for the Denomination ListView element
        *
        * @return {any} The miniminum/default cell size for the grid
        */
        private denominationsLineViewGroupInfo(): any {
            return {
                enableCellSpanning: true,
                cellWidth: PaymentViewController.denominationBlockSize,
                cellHeight: PaymentViewController.denominationBlockSize
            };
        }

        /**
        * Get the minimum/default size for the Denomination ListView element
        *
        * @param {number} itemIndex The index of the item in the grid
        * @return {any} The miniminum/default cell size for the grid
        */
        private denominationsLineViewItemInfo(itemIndex: number): any {
            var denominations: Commerce.Model.Entities.CashDeclaration[] = this.paymentViewModel.currencyDenominations();
            var denomLength: number = denominations[itemIndex].Amount.toString().length;
            var numBlocks: number = Math.ceil(denomLength / 5);
            return {
                width: numBlocks === 0 ? 0 : Math.round(PaymentViewController.denominationBlockSize * numBlocks) + Math.round(10 * (numBlocks - 1)),
                height: PaymentViewController.denominationBlockSize
            };
        }

        /**
         * Action to take when the foreign currency is selected from a list.
         * Will set the foreign currency values on the screen and will set the total amount in the
         * payment grid if the total value in the numpad has not been changed
         *
         * @param {Model.Entities.CurrencyAmount} itemSelected The selected currency
         */
        private foreignCurrencyItemInvokedHandler(itemSelected: Model.Entities.CurrencyAmount): void {
            this.hideCurrencyCodeFlyout()();
            if (StringExtensions.compare(itemSelected.CurrencyCode, this.paymentViewModel.currency()) != 0) {
                this.paymentViewModel.setCurrency(itemSelected.CurrencyCode, true);
            }
            this.selectTotalAmountInput()();
        }

        /**
          * Action to take when the currency denomination is selected from a list.
          *
          * @param {Commerce.Model.Entities.CashDeclaration} itemSelected The selected denomination amount.
          * @return {boolean} Returns true to indicate that the operation is handled (used for buttonGrid).
          */
        private denominationInvokedHandler(itemSelected: Proxy.Entities.CashDeclaration): boolean {
            var paymentAmount: string = NumberExtensions.formatNumber(itemSelected.Amount, NumberExtensions.getDecimalPrecision(itemSelected.Currency));
            this.beginProcessingPayment(paymentAmount);
            return true;
        }

        /**
          * Gets whether to show the payment details.
          *
          * @returns {boolean} True: show payment details; false: otherwise.
          */
        private computeShowPaymentDetails(): boolean {
            return this.paymentViewModel.isCardPayment
                || this.paymentViewModel.isGiftCardPayment
                || this.paymentViewModel.isLoyaltyCardPayment;
        }

        /**
         * Gets whether the payment method is cash payment.
         *
         * @returns {boolean} True: payment method is cash; false: otherwise.
         */
        private computeIsTypeCashPayment(): boolean {
            return this.paymentViewModel.operationId() == Operations.RetailOperation.PayCash;
        }

        /**
         * Gets whether the payment method is currency.
         *
         * @returns {boolean} True: payment method is currency; false: otherwise.
         */
        private computeIsCurrencyPayment(): boolean {
            return this.paymentViewModel.operationId() == Operations.RetailOperation.PayCurrency;
        }

        /**
         * Gets whether the currencies sections should be displayed.
         *
         * @returns {boolean} True: the currencies section should be displayed; false: otherwise.
         */
        private computeShowCurrencies(): boolean {
            return !this._hasForeignCurrencyDropDownBeenDisplayed
                || (this.computeIsCurrencyPayment() && (this.paymentViewModel.currencyAmounts().length > 1));
        }

        /**
         * Gets whether the denominations should be displayed.
         *
         * @returns {boolean} True: the denominations section should be displayed; false: otherwise.
         */
        private computeShowDenominations(): boolean {
            return this.paymentViewModel.isCashPayment
                && ArrayExtensions.hasElements(this.paymentViewModel.currencyDenominations());
        }

        /**
         * Gets whether the denominations for Currency should be displayed.
         *
         * @returns {boolean} True: The denominations for currency should be displayed; false: otherwise.
         */
        private computeShowCurrencyDenominations(): boolean {
            return this.paymentViewModel.isCashPayment
                && this.displayCurrencies()
                && ArrayExtensions.hasElements(this.paymentViewModel.currencyDenominations());
        }

        /**
         * Gets the formatted conversion string to display for the Currency data
         *
         * @returns {string} The formatted conversion string to display for the Currency data. An empty string if the currency amount information is not available.
         */
        private computeCurrencyConversionString(): string {
            var currencyConversionString: string = StringExtensions.EMPTY;

            if (this.paymentViewModel) {
                var currencyAmount: Commerce.Model.Entities.CurrencyAmount = this.paymentViewModel.getSelectedCurrencyAmount();

                if (currencyAmount) {
                    var currencyAmountFrom: number = currencyAmount.ExchangeRate;
                    var currencyCodeFrom: string = currencyAmount.CurrencyCode;
                    var currencyAmountTo: number = 1;
                    var currencyCodeTo: string = Commerce.ApplicationContext.Instance.deviceConfiguration.Currency;

                    if (currencyAmountFrom) {
                        currencyConversionString = Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_1187"), currencyAmountFrom, currencyCodeFrom, currencyAmountTo, currencyCodeTo);
                    }
                }
            }

            return currencyConversionString;
        }

        /**
         * Handles the key press.
         *
         * @param {string} tenderTypeId The id of the tender type.
         */
        private handleKeyPress(event: any): void {
            if (event.target.nodeName != 'INPUT' &&
                event.keyCode != 13) {
                $('#totalAmountInput').focus();
            }
        }

        /**
         * Checks whether the tender amounts are valid for payment for a card payment.
         *
         * @param {number} paymentAmount The payment amount.
         * @return {IVoidAsyncResult} The void async result.
         */
        public areTenderAmountsValidForCardPayment(paymentAmount?: number): IVoidAsyncResult {
            if (isNaN(paymentAmount)) {
                paymentAmount = this.paymentViewModel.getPaymentAmount();
            }
            var tenderLineToValidate: Commerce.Model.Entities.TenderLine = {
                Amount: paymentAmount,
                Currency: this.paymentViewModel.currency(),
                TenderTypeId: this.paymentViewModel.tenderTypeId(),
                CashBackAmount: this.paymentViewModel.cashBackAmount(),
                CardTypeId: this.paymentViewModel.cardTypeId()
            };

            return this.cartViewModel.validateTenderLineForAdd(tenderLineToValidate);
        }

        private onNumPadEnterEventHandler(result: Controls.NumPad.INumPadResult): void {
            this.beginProcessingPayment(result.value);
        }

        /**
         * Begins processing payment.
         * Does validation checks
         * Steps:
         * 1. Checks the payment amount is valid
         * 2. Retrieves the payment information for manual card entry
         * 3. Retrieves and authorizes/refunds the payment amount for swipe card entry
         *
         * @param {string} paymentAmountText The payment amount text.
         */
        private beginProcessingPayment(paymentAmountText?: string) {
            RetailLogger.viewsOrderPaymentViewPaymentInitiated();

            // If a payment is already being processed don't process another payment.
            // If the view is not shown, do not process the payment as MSR swipe might send a delayed "enter" from the transaction page
            if (this._processingPayment || !this._isViewShown) {
                return;
            }

            // Block the UI while the payment is being processed.
            this._processingPayment = true;

            // Update payment amount
            if (!StringExtensions.isNullOrWhitespace(paymentAmountText) && (paymentAmountText != this.paymentViewModel.paymentAmountText())) {
                this.paymentViewModel.paymentAmountText(paymentAmountText);
            }

            // Check payment amount is a number
            var paymentAmount: number = this.paymentViewModel.getPaymentAmount();
            if (isNaN(paymentAmount)) {
                var errors: Model.Entities.Error[] =
                    [new Model.Entities.Error(ErrorTypeEnum.PAYMENT_INVALID_NUMBER)];

                this.paymentErrorCallback(errors);
                return;
            }

            // Check that the payment amount is valid
            // Error messages are displayed from the isPaymentAmountValid method
            if (!this.paymentViewModel.isPaymentAmountValid()) {
                this.paymentCancel();
                return;
            }


            //TODO:AM Add control to block payment if cust balance is less than payment amount
            //if (this._options.tenderType.OperationId === 202) {
            //    var currentCart1 = this.cartViewModel.cart();
            //    var custBalance = this._customerCardViewModel.customer().Balance * -1;

            //    var tempCartLines: Commerce.Proxy.Entities.CartLine[] = new Array(currentCart1.CartLines.length);

            //    var testCnt: number = 0;
            //    //Find if this is the last transaction on the order
            //    for (let cLine of currentCart1.CartLines) {
            //        if (cLine.SalesStatusValue !== 4) {
            //            tempCartLines.push(cLine);
            //            if (cLine.Quantity > 0)
            //                testCnt++;
            //        }
            //    }

            //    if (tempCartLines.length !== testCnt) {

            //        //if (paymentAmount !== custBalance) {
            //            var originalAmount1 = currentCart1.DiscountAmount + currentCart1.TotalAmount;
            //            if (originalAmount1 > (custBalance)) {
            //                Commerce.NotificationHandler
            //                    .displayClientErrors([
            //                        new Commerce.Model.Entities.Error("Not enough balance to tender the payment")
            //                    ]);
            //                this.paymentCancel();
            //                return;
            //            }

            //            //TODO:AM Add control to block payment if cust balance is less than payment amount

            //            if (originalAmount1 > paymentAmount) {
            //                Commerce.NotificationHandler
            //                    .displayClientErrors([
            //                        new Commerce.Model.Entities
            //                        .Error("Amount should be greater than or equal to the unit price of the item")
            //                    ]);
            //                this.paymentCancel();
            //                return;
            //            }
            //        //}
            //    }
            //}
            //AM End

            if (this.paymentViewModel.isCardPayment && this.isCardPaymentAcceptPage()) {
                // If using card payment accept page, submit payment to the page.

                if (StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptResultAccessCode)) {
                    this.cardPaymentAcceptSubmitPayment(paymentAmount);
                } else {
                    this.retrieveCardPaymentAcceptResult();
                }

                return;
            } else {
                // Update the swipe flag in payment card based on current UI setting.
                var isSwipeCard: boolean = this.isSwipeCard() && this.paymentViewModel.isCardPayment;
                var isApprovalCode: boolean = this.isApprovalCode() && this.paymentViewModel.isCardPayment;

                // Check whether already swiped
                var needCreditDebitCardInfo: boolean = this.paymentViewModel.cardSource() === Model.Entities.CardSource.Unknown;
                var isCreditDebitCardInfoFromPaymentTerminal = this.paymentViewModel.isCardSourcePaymentTerminal();

                // Set the data source in the view model
                this.paymentViewModel.paymentCard.IsSwipe(isSwipeCard);

                var isTerminalAndEmptyCardNumber: boolean = this.canUsePaymentTerminal() && StringExtensions.isEmptyOrWhitespace(this.paymentViewModel.cardNumber());
                // If swipe and payment terminal is configured, authorize/refund the card
                // If the existing card information is from payment terminal, authorize the card information again
                if (this._options.tokenizeCard && isTerminalAndEmptyCardNumber) {
                    // Tokenize payment card from payment terminal.
                    this.tokenizePaymentCard();
                } else if (!this._options.tokenizeCard && isTerminalAndEmptyCardNumber
                    && (needCreditDebitCardInfo || isCreditDebitCardInfoFromPaymentTerminal)) {
                    // Authorize or refund the payment from payment terminal.
                    this.paymentTerminalAuthorizeRefund(paymentAmount, this.isManualCardEntry(), this.paymentViewModel.paymentCard.VoiceAuthorizationCode());
                    return;
                } else if (isSwipeCard && needCreditDebitCardInfo) {
                    // If swipe and not payment terminal, then check that the card data exists
                    NotificationHandler.displayErrorMessage("string_7200"); // The payment information has not been provided. Provide the card information and try again.
                    this.paymentErrorCallback(null);
                    return;
                } else if (this.paymentViewModel.isCardPayment && needCreditDebitCardInfo && !isSwipeCard) {
                    // If payment is by manual entry of payment card and card information is not available, build the card information and update it in view model.
                    var cardInfo: Model.Entities.CardInfo = {
                        CardNumber: this.paymentViewModel.cardNumber(),
                        ExpirationMonth: this.paymentViewModel.paymentCard.ExpirationMonth(),
                        ExpirationYear: this.paymentViewModel.paymentCard.ExpirationYear(),
                        CCID: this.paymentViewModel.paymentCard.CCID(),
                        VoiceAuthorizationCode: this.paymentViewModel.paymentCard.VoiceAuthorizationCode(),
                        Address1: this.paymentViewModel.paymentCard.Address1(),
                        Zip: this.paymentViewModel.paymentCard.Zip()
                    };

                    this.paymentViewModel.updateCardInfo(cardInfo, Model.Entities.CardSource.Manual);
                    this.continueProcessingPaymentStep1();
                } else {
                    // If card information is not needed, continue processing payment
                    this.continueProcessingPaymentStep1();
                }
            }
        }

        /**
         * Continues processing payment - Step 1
         * Called after all the payment information is retrieved.
         * Steps:
         * 1. Checks that the payment information is complete
         * 2. Gets additional card information (if necessary)
         * 3. Calls the method to make the payment
         *
         */
        private continueProcessingPaymentStep1() {
            // Check that the payment information in complete
            // Error messages are displayed from the isPaymentInformationComplete method
            if (!this.paymentViewModel.isPaymentInformationComplete(false, this.isApprovalCode())) {
                this.paymentCancel();
                return;
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var tenderLine: Commerce.Model.Entities.CartTenderLine; // The tender line to make the payment. Only set if pinpad is set.

            // Check card type for card payment
            // Always get card type for card payment on credit card manual entry (this can be removed if changing of card information on type is tracked)
            var hasAllCardInfo: boolean = this.paymentViewModel.isCardTypeSet() && !ObjectExtensions.isNullOrUndefined(this.paymentViewModel.cardInfo);
            if (this.paymentViewModel.isCardPayment
                && (!hasAllCardInfo || this.isManualCardEntry())) {
                asyncQueue.enqueue(() => {
                    return this.getCardTypeAsync();
                });
            }

            // Get the cash back if debit card
            asyncQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                var cardInfo: Model.Entities.CardInfo = this.paymentViewModel.cardInfo;

                if (this.getCashBackFromUI() && cardInfo) {

                    this.paymentViewModel.getFilteredDenominationsForCurrencyAsync(this.paymentViewModel.currency(), 1)
                        .done((denominations: Commerce.Model.Entities.CashDeclaration[]) => {
                            var cashbackDialogShowOptions: Commerce.Controls.CashbackDialogShowOptions = {
                                cashbackAmount: NumberExtensions.parseNumber(this.paymentViewModel.cashBackAmountText()),
                                maximumCashbackAmount: this.paymentViewModel.getMaximumCashbackAmount(),
                                denominations: denominations
                            };
                            this.showDialog(this._cashbackDialog, cashbackDialogShowOptions)
                                .on(DialogResult.OK, (cashbackAmount: number) => {
                                    this.paymentViewModel.setCashbackAmount(cashbackAmount);
                                    this.areTenderAmountsValidForCardPayment()
                                        .done(() => {
                                            asyncResult.resolve({ canceled: false });
                                        })
                                        .fail((errors: Commerce.Model.Entities.Error[]) => {
                                            asyncResult.reject(errors);
                                        });
                                }).on(DialogResult.No, (cashbackAmount: number) => {
                                    this.paymentViewModel.setCashbackAmount(0);
                                    asyncResult.resolve({ canceled: false });
                                }).on(DialogResult.Cancel, (cashbackAmount: number) => {
                                    asyncResult.resolve({ canceled: true });
                                }).onError((errors: Commerce.Model.Entities.Error[]) => {
                                    asyncResult.reject(errors);
                                });
                        })
                        .fail((errors: Commerce.Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                } else {
                    asyncResult.resolve({ canceled: false });
                }

                return asyncQueue.cancelOn(asyncResult);
            });

            // For card that is being added as a tender line check that the payment information is valid
            if (this.paymentViewModel.isCardPayment && !this._options.tokenizeCard) {
                asyncQueue.enqueue(() => {
                    var paymentAmount: number = this.paymentViewModel.getPaymentAmount();
                    return this.areTenderAmountsValidForCardPayment(paymentAmount);
                });
            }

            // Get the pin if debit card and pin has not been retrieved
            asyncQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                if (this.paymentViewModel.isDebitCard()
                    && !this.canUsePaymentTerminal()) {
                    // Check whether there is a pin pad device configured
                    if (ObjectExtensions.isNullOrUndefined(Peripherals.instance) || (ObjectExtensions.isNullOrUndefined(Peripherals.instance.pinPad))) {
                        // Throw an error as there is no path to get the pin pad
                        asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PINPAD_ERROR)]);
                        return asyncResult;
                    }

                    // Open the pinpad device if needed
                    var pinPadQueue: AsyncQueue = new AsyncQueue();
                    if (!Peripherals.instance.pinPad.isActive) {
                        pinPadQueue.enqueue(() => {
                            var pinPadQueueAsyncResult = new AsyncResult<ICancelableResult>(null);
                            Peripherals.instance.pinPad.openDevice()
                                .done(() => {
                                    if (Peripherals.instance.pinPad.isActive) {
                                        pinPadQueueAsyncResult.resolve({ canceled: false });
                                    } else {
                                        // Throw an error as the pinPad could not be opened
                                        pinPadQueueAsyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PINPAD_ERROR)]);
                                    }
                                }).fail((errors: Commerce.Model.Entities.Error[]) => {
                                    // Throw an error as there is no path to get the pinPad
                                    pinPadQueueAsyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PINPAD_ERROR)]);
                                });

                            return pinPadQueue.cancelOn(pinPadQueueAsyncResult);
                        });
                    }

                    // Create the actions to get the pin from the device
                    pinPadQueue.enqueue(() => {
                        var pinPadQueueAsyncResult = new AsyncResult<ICancelableResult>(null);

                        // Get the tender line to add the pin data
                        tenderLine = this.paymentViewModel.getTenderLine();

                        // Show the dialog to get the pin
                        this.isPinPadDialogVisible(true);

                        // Get the amount to display on the pin pad
                        var amountToDisplay: number;
                        if (!ObjectExtensions.isNullOrUndefined(tenderLine.CashBackAmount) && (tenderLine.CashBackAmount > 0)) {
                            amountToDisplay = NumberExtensions.roundToNDigits(tenderLine.Amount + tenderLine.CashBackAmount, NumberExtensions.getDecimalPrecision());
                        } else {
                            amountToDisplay = tenderLine.Amount;
                        }

                        // Get the pin
                        Peripherals.instance.pinPad.getPinEntry(tenderLine.PaymentCard.CardNumber, amountToDisplay, this)
                            .done((result) => {
                                if (result) {
                                    this.isPinPadDialogVisible(false);
                                    tenderLine.PaymentCard.EncryptedPin = result.encryptedPin;
                                    tenderLine.PaymentCard.AdditionalSecurityData = result.additionalSecurityData;
                                    pinPadQueueAsyncResult.resolve({ canceled: false });
                                }
                                else {
                                    // Pin entry was cancelled - either from dialog or from device
                                    // If cancelled from dialog, pinpad dialog is already closed - don't show pinpad cancelled dialog
                                    if (this.isPinPadDialogVisible()) {
                                        this.isPinPadDialogVisible(false);
                                        this.isPinPadCancelledDialogVisible(true);
                                        pinPadQueueAsyncResult.resolve({ canceled: true });
                                    }
                                }
                            })
                            .fail((errors) => {
                                this.isPinPadDialogVisible(false);
                                pinPadQueueAsyncResult.reject(errors);
                            });

                        return pinPadQueue.cancelOn(pinPadQueueAsyncResult);
                    });

                    // Run the queue to get the pin from the device
                    pinPadQueue.run()
                        .done((result: Commerce.ICancelableResult) => {
                            if (ObjectExtensions.isNullOrUndefined(result) || !result.canceled) {
                                asyncResult.resolve({ canceled: false });
                            } else {
                                asyncResult.resolve({ canceled: true });
                            }
                        }
                            ).fail((errors: Commerce.Model.Entities.Error[]) => {
                                asyncResult.reject(errors);
                            });
                } else {
                    // The payment is not a debit card or the pin data has been retrieved, continue processing
                    asyncResult.resolve({ canceled: false });
                }

                return asyncQueue.cancelOn(asyncResult);
            });

            // Run the queue to get the card information and debit information (if needed) and then make the payment
            asyncQueue.run()
                .done((result: Commerce.ICancelableResult) => {
                    if (ObjectExtensions.isNullOrUndefined(result) || !result.canceled) {
                        this.makePayment(tenderLine);
                    } else {
                        this.paymentCancel();
                    }
                }).fail((errors: Commerce.Model.Entities.Error[]) => {
                    this.paymentErrorCallback(errors);
                });
        }

        /**
         * Sets the payment card type based on the payment card number.
         *
         * @return {IVoidAsyncResult} The void async result.
         */
        public getCardTypeAsync(): IVoidAsyncResult {
            var filteredCardTypes: Commerce.Model.Entities.CardTypeInfo[];

            return new AsyncQueue()
                .enqueue(() => {
                    // Filter the card types based on card number, card type and payment card drop down setting.
                    return this.paymentViewModel.filterCardTypesAsync(this.paymentViewModel.paymentCard.IsSwipe(), this.paymentViewModel.cardInfo).done((types: Commerce.Model.Entities.CardTypeInfo[]) => {
                        filteredCardTypes = types;
                    });
                })
                .enqueue(() => {
                    var asyncResult: VoidAsyncResult = new VoidAsyncResult(this);

                    this.indeterminateWaitVisible(false);
                    if (filteredCardTypes.length === 0) {
                        // string_1139 - The card type is not an accepted form of payment. Use a different payment card, and then try again.
                        asyncResult.reject([new Commerce.Model.Entities.Error(ErrorTypeEnum.PAYMENT_CARD_NOT_SUPPORTED)]);
                    } else if (filteredCardTypes.length === 1) {
                        // Got a single card type: Set payment card type and make payment.
                        this.paymentViewModel.setPaymentCardType(filteredCardTypes[0]);
                        asyncResult.resolve();
                    } else {
                        // Got multiple card types. If card from payment terminal, then select first card.
                        if (this.paymentViewModel.cardSource() === Commerce.Model.Entities.CardSource.PaymentTerminal) {
                            this.paymentViewModel.setPaymentCardType(filteredCardTypes[0]);
                            asyncResult.resolve();
                        } else {
                            // Got multiple card types: Show select payment card type dialog.
                            this.showDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog, filteredCardTypes)
                                .on(DialogResult.OK, (cardTypeInfo: Commerce.Model.Entities.CardTypeInfo) => {
                                    this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                        .done(() => {
                                            this.paymentViewModel.setPaymentCardType(cardTypeInfo);
                                            asyncResult.resolve();
                                        });
                                }).on(DialogResult.Cancel, () => {
                                    this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                    asyncResult.reject(null);
                                });
                        }
                    }

                    return asyncResult;
                })
                .run();
        }

        /**
         * Cancels the payment terminal dialog.
         */
        private cancelPaymentTerminalDialog(): void {
            Peripherals.instance.paymentTerminal.cancelOperation().done(() => {
                // If cancel works close the dialog
                this.isPaymentTerminalDialogVisible(false);
            });
        }

        /**
         * Cancels the pin pad pin entry and closes the pin pad dialog.
         */
        private cancelPinPadDialog(): void {
            this.isPinPadDialogVisible(false);
            this._processingPayment = false;
            Peripherals.instance.pinPad.cancelPinEntry();
        }

        /**
         * Closes the pin pad cancelled dialog.
         */
        private closePinPadCancelledDialog(): void {
            this.isPinPadCancelledDialogVisible(false);
        }

        /**
         * Displays the payment accept view disclaimer dialog.
         */
        private showPaymentAcceptDisclaimerDialog(): void {
            this.isPaymentAcceptDisclaimerDialogVisible(true);
        }

        /**
         * Closes the payment accept view disclaimer dialog.
         */
        private closePaymentAcceptDisclaimerDialog(): void {
            this.isPaymentAcceptDisclaimerDialogVisible(false);
        }

        /**
         * Tokenizes a payment card.
         *
         */
        private tokenizePaymentCard(): void {
            this.isPaymentTerminalDialogVisible(true);

            var cardTokenizationQueue: AsyncQueue = new AsyncQueue();
            var paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo = null;
            var tokenizedPaymentCard: Proxy.Entities.TokenizedPaymentCard = null;

            // Invoke payment terminal fetch token api.
            cardTokenizationQueue.enqueue(() => {
                return Peripherals.instance.paymentTerminal.fetchToken(this.isManualCardEntry(), null, this)
                    .done((result: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                        paymentInfo = result;
                    });
            });

            // Exam tokenization result
            cardTokenizationQueue.enqueue(() => {
                this.isPaymentTerminalDialogVisible(false);

                var asyncResult = new AsyncResult<ICancelableResult>(null);

                if (paymentInfo && paymentInfo.IsApproved) {

                    // Extract the token id (unique card id) and token string from the payment data returned.
                    var tokenString: string = paymentInfo.PaymentSdkData.substring(36);
                    var tokenId: string = paymentInfo.PaymentSdkData.substring(0, 36);

                    var cardTokenInfo: Proxy.Entities.CardTokenInfo = {
                        CardToken: tokenString,
                        MaskedCardNumber: paymentInfo.CardNumberMasked,
                        ServiceAccountId: Commerce.ApplicationContext.Instance.hardwareProfile.EftCompanyId,
                        UniqueCardId: tokenId
                    }

                    // Get the tender line
                    var tenderLine: Commerce.Model.Entities.CartTenderLine = this.paymentViewModel.getTenderLine();

                    tokenizedPaymentCard = {
                        TenderType: Model.Entities.TenderTypeId.Cards.toString(),
                        CardTokenInfo: cardTokenInfo
                    }

                    asyncResult.resolve({ canceled: false });
                }
                else {
                    asyncResult.resolve({ canceled: true });
                }

                return cardTokenizationQueue.cancelOn(asyncResult);
            });       
            
            cardTokenizationQueue.run()
                .done((result: Commerce.ICancelableResult) => {
                    if (ObjectExtensions.isNullOrUndefined(result) || !result.canceled) {

                        // Forward the card token to cart to view to finish checkout
                        var cartViewOptions: ICartViewControllerOptions = <ICartViewControllerOptions>{
                            navigationSource: 'PaymentView',
                            tokenizedPaymentCard: tokenizedPaymentCard
                        };

                        this._processingPayment = false;
                        this.indeterminateWaitVisible(false);
                        Commerce.ViewModelAdapter.navigate("CartView", cartViewOptions); 
                    } else {
                        this.isPaymentTerminalDialogVisible(false);
                        this.paymentErrorCallback([new Model.Entities.Error(ErrorTypeEnum.HARDWARESTATION_BALANCE_TOKEN_ERROR)]);
                    }
                })
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    this.isPaymentTerminalDialogVisible(false);
                    this.paymentErrorCallback([new Model.Entities.Error(ErrorTypeEnum.HARDWARESTATION_BALANCE_TOKEN_ERROR)]);
                });
        }

        /**
         * Makes the payment.
         *
         * @param {Commerce.Model.Entities.CartTenderLine} tenderLine The tender line to process.
         */
        private makePayment(tenderLine: Commerce.Model.Entities.CartTenderLine): void {
            // Get the tender line
            if (ObjectExtensions.isNullOrUndefined(tenderLine)) {
                tenderLine = this.paymentViewModel.getTenderLine();
            }
            
            this.indeterminateWaitVisible(true);
            var cardInfo: Model.Entities.CardInfo = this.paymentViewModel.cardInfo;

            // If the card info is set (only done for payment types that uses a card) then set the properties from the card info
            if (cardInfo && (!ObjectExtensions.isNullOrUndefined(cardInfo.CashBackAmount)) && (cardInfo.CashBackAmount != 0)) {
                tenderLine.CashBackAmount = cardInfo.CashBackAmount;
            }

            // Handles credit card and debit card payments when not using a payment terminal.
            if (this.paymentViewModel.isCreditCard() || this.paymentViewModel.isDebitCard()) {
                this.cardPaymentAuthorizeRefund(tenderLine);
                return;
            }

            // If not a credit or debit card payment, then add the payment as a tender line and let retail server process the payment
            this.cartViewModel.addTenderLineToCart(tenderLine)
                .done((result: ICancelableResult) => {
                    if (result.canceled) {
                        this.paymentCancel();
                        return;
                    }

                    this.getSignature();
                }).fail((errors) => {
                    this.paymentErrorCallback(errors);
                });
        }

        /**
         * Validate the signature from a peripheral on POS UI
         *
         * @param {string} signatureData The signature to verify.
         * @param {boolean} allowSkip Indicates whether to allow the signature to be skipped from the UI.
         */
        private validateSignature(signatureData: string, allowSkip: boolean = false): void {
            var signatureCaptureDialogState: Commerce.Controls.SignatureCaptureDialogState = {
                verifyOnly: true,
                allowSkip: allowSkip,
                signatureData: signatureData,
                paymentAmount: this.paymentViewModel.getPaymentAmount()
            };
            this.showDialog(this._signatureCaptureDialog, signatureCaptureDialogState)
                .on(DialogResult.OK, (signature: string) => {
                    this.paymentViewModel.signatureData(signature);
                    this.beginUploadingSignature(signature);
                }).on(DialogResult.Cancel, () => {
                    this.paymentViewModel.signatureData(null);
                    this.getSignature(true);
                }).on(DialogResult.No, () => {
                    this.paymentCompleted();
                });
        }

        /**
         * Gets the signature from POS (not the peripheral).
         *
         * @param {boolean} allowSkip Indicates whether to allow the signature to be skipped from the UI.
         */
        private getSignatureFromPOS(allowSkip: boolean = false): void {
            var signatureCaptureDialogState: Commerce.Controls.SignatureCaptureDialogState = {
                verifyOnly: false,
                allowSkip: allowSkip,
                signatureData: null,
                paymentAmount: this.paymentViewModel.getPaymentAmount()
            };
            this.showDialog(this._signatureCaptureDialog, signatureCaptureDialogState)
                .on(DialogResult.OK, (signature: string) => {
                    this.paymentViewModel.signatureData(signature);
                    this.beginUploadingSignature(signature);
                }).on(DialogResult.Cancel, () => {
                    this.paymentViewModel.signatureData(null);
                    this.getSignature(true);
                }).on(DialogResult.No, () => {
                    this.paymentCompleted();
                });
        }

        /**
         * Gets the signature if needed.
         *
         * @param {boolean} allowSkip Indicates whether to allow the signature to be skipped from the UI.
         */
        private getSignature(allowSkip: boolean = false): void {
            this.indeterminateWaitVisible(false);
            if (this.paymentViewModel.isSignatureRequired()) {
                var signature: string = this.paymentViewModel.signatureData();

                if (!Commerce.StringExtensions.isNullOrWhitespace(signature)) {
                    this.validateSignature(signature, allowSkip);
                } else if (Peripherals.instance && Peripherals.instance.signatureCapture) {
                    Peripherals.instance.signatureCapture.openDevice(this)
                        .done(() => {
                            if (Peripherals.instance.signatureCapture.isActive) {
                                // Show the dialog to instruct the customer to enter the signature on the device
                                var signatureDeviceDialogState: Commerce.Controls.SignatureDeviceDialogState = {
                                    allowSkip: allowSkip,
                                    allowGetSignature: true
                                };
                                this.showDialog(this._signatureDeviceDialog, signatureDeviceDialogState)
                                    .on(DialogResult.OK, () => {
                                        Peripherals.instance.signatureCapture.cancelSignature(this)
                                            .fail((errors: Commerce.Model.Entities.Error[]) => {
                                                // Check if the device timed out - if so, retry signature capture
                                                var error = <Model.Entities.Error>ArrayExtensions.firstOrUndefined(errors);
                                                if (error && error.ErrorCode == "Microsoft_Dynamics_Commerce_HardwareStation_PeripheralLockNotAcquired") {
                                                    var message: string = "string_4924"; // There was an error communicating with the signature capture device. Check the device or see your system administrator.
                                                    Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4924"), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4909"))
                                                        .always(() => {
                                                            this.getSignature(true);
                                                        });
                                                }
                                                else {  // Device is still active, but we couldn't get the signature for some reason - close the device and try again
                                                    Peripherals.instance.signatureCapture.closeDevice()
                                                        .always(() => {
                                                            var message: string = "string_4924"; // There was an error communicating with the signature capture device. Check the device or see your system administrator.
                                                            Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4924"), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4909"))
                                                                .always(() => {
                                                                    this.getSignature(allowSkip);
                                                                });
                                                        });
                                                }
                                            });
                                    }).on(DialogResult.Cancel, () => {
                                        Peripherals.instance.signatureCapture.cancelSignature(this)
                                            .always(() => {
                                                Peripherals.instance.signatureCapture.closeDevice()
                                                    .always(() => {
                                                        this.getSignatureFromPOS(allowSkip);
                                                    });
                                            });
                                    }).on(DialogResult.No, () => {
                                        Peripherals.instance.signatureCapture.cancelSignature(this)
                                            .always(() => {
                                                Peripherals.instance.signatureCapture.closeDevice()
                                                    .always(() => {
                                                        this.paymentCompleted();
                                                    });
                                            });
                                    });

                                // Instruct the device to get the signature
                                Peripherals.instance.signatureCapture.getSignature(this)
                                    .done((signature: string) => {
                                        // If the message is not closed or resolved to get the signature from the device, then the return value is from the device
                                        if (ObjectExtensions.isNullOrUndefined(this._signatureDeviceDialog.dialogCloseAction)
                                            || (this._signatureDeviceDialog.dialogCloseAction === Commerce.DialogResult.OK)) {
                                            // Close the device
                                            Peripherals.instance.signatureCapture.closeDevice()
                                            // Handles the signature returned from the device
                                                .always(() => {
                                                    // If the message has a value, validate the message
                                                    if (!StringExtensions.isNullOrWhitespace(signature)) {
                                                        this._signatureDeviceDialog.hide()
                                                            .always(() => {
                                                                this.validateSignature(signature, allowSkip);
                                                            });
                                                        // If the message does not have a value, then the signature capture was cancelled from the device
                                                    } else {
                                                        this._signatureDeviceDialog.hide()
                                                            .always(() => {
                                                                var message: string = "string_4922"; // // The customer cancelled the operation from the device.
                                                                Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4922"), MessageType.Info, MessageBoxButtons.Default)
                                                                    .always(() => {
                                                                        this.getSignature(true);
                                                                    });
                                                            });
                                                    }
                                                });
                                            // The input from the device was cancelled from the UI
                                        } else {
                                            Peripherals.instance.signatureCapture.closeDevice()
                                                .always(() => {
                                                    this.getSignatureFromPOS(true);
                                                });
                                        }
                                    })
                                    .fail((errors: Commerce.Model.Entities.Error[]) => {
                                        // Check if the device timed out - if so, retry signature capture
                                        var error = <Model.Entities.Error>ArrayExtensions.firstOrUndefined(errors);
                                        if (error && error.ErrorCode == "Microsoft_Dynamics_Commerce_HardwareStation_PeripheralLockNotAcquired") {
                                            var message: string = "string_4924"; // There was an error communicating with the signature capture device. Check the device or see your system administrator.
                                            Commerce.ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString("string_4924"), MessageType.Info, MessageBoxButtons.Default, Commerce.ViewModelAdapter.getResourceString("string_4909"))
                                                .always(() => {
                                                    this.getSignature(true);
                                                });
                                        } else {
                                            // Error not related to timeout - get signature from POS instead
                                            Peripherals.instance.signatureCapture.closeDevice()
                                                .always(() => {
                                                    Commerce.NotificationHandler.displayClientErrors(errors)
                                                        .always(() => {
                                                            return this.getSignatureFromPOS(allowSkip);
                                                        });
                                                });
                                        }
                                    });
                            } else {
                                // Device was not opened because no device is configured - show the manual entry dialog
                                this.getSignatureFromPOS(allowSkip);
                            }
                        })
                        .fail((errors) => {
                            Commerce.NotificationHandler.displayClientErrors(errors)
                                .always(() => {
                                    this.getSignatureFromPOS(allowSkip);
                                });
                        });
                } else {
                    // Get the signature from POS UI
                    this.getSignatureFromPOS(allowSkip);
                }
            }
            else {
                this.paymentCompleted();
            }
        }

        /**
         * Method to call on payment success
         */
        private paymentCompleted(tenderLine?: Proxy.Entities.TenderLine): void {
            if (ObjectExtensions.isNullOrUndefined(tenderLine)) {
                tenderLine = CartHelper.getLastTenderLine(Session.instance.cart);
            }

            this._processingPayment = false;

            var triggerOptions: Triggers.IPostPaymentTriggerOptions = { cart: Session.instance.cart, tenderLine: tenderLine };
            Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostPayment, triggerOptions).done((): void => {
                this.indeterminateWaitVisible(false);
                Commerce.ViewModelAdapter.navigate("CartView", <ICartViewControllerOptions>{ navigationSource: "PaymentView" });
            }).fail((errors: Proxy.Entities.Error[]): void => {
                NotificationHandler.displayClientErrors(errors).always((): void => {
                    this.indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CartView", <ICartViewControllerOptions>{ navigationSource: "PaymentView" });
                });
            });
        }

        /**
         * Callback method if payment is canceled.
         */
        private paymentCancel(): void {
            try {
                // Remove the card information if from payment terminal as payment terminal cards have to be re-entered each time
                var isSwipeCard: boolean = this.isSwipeCard() && this.paymentViewModel.isCardPayment;
                if (isSwipeCard && this.canUsePaymentTerminal()) {
                    this.paymentViewModel.updateCardInfo(null);
                }
            } finally {
                this.indeterminateWaitVisible(false);
                this._processingPayment = false;
            }
        }

        /**
         * Callback method for payment error.
         *
         * @param {Model.Entities.Error[]} errors The array of errors.
         * @return {IAsyncResult<IMessageResult>} The async result.
         */
        private paymentErrorCallback(errors: Model.Entities.Error[]): IAsyncResult<IMessageResult> {
            return Commerce.NotificationHandler.displayClientErrors(errors)
                .done(() => { this.paymentCancel(); })
                .fail(() => { this.paymentCancel(); });
        }

        /**
         * Begins uploading signature.
         *
         * @param {string} signatureData The signature to upload.
         */
        private beginUploadingSignature(signatureData: string): void {
            if (StringExtensions.isNullOrWhitespace(signatureData)) {
                return;
            }

            // Get the tender line to update
            var tenderLine = CartHelper.getLastTenderLine(Session.instance.cart);
            if (tenderLine == null) {
                NotificationHandler.displayErrorMessage('string_1136'); // We couldn't find the payment for the signature.
                return;
            } else {
                tenderLine.SignatureData = signatureData;
            }

            // Update tender line
            this.indeterminateWaitVisible(true);
            this.cartViewModel.updateTenderLineSignatureInCart(tenderLine)
                .done(() => {
                    this.paymentCompleted(tenderLine);
                })
                .fail((errors) => {
                    this.paymentErrorCallback(errors);
                });
        }

        /**
         * Checks the gift card balance. 
         */
        private checkGiftCardBalance(): void {
            this.indeterminateWaitVisible(true);

            this.paymentViewModel.checkGiftCardBalance().done(() => {
                this.indeterminateWaitVisible(false);
                this.showGiftCardBalance(true);
            }).fail((errors: Model.Entities.Error[]) => {
                this.paymentErrorCallback(errors);
                this.showGiftCardBalance(false);
            });
        }

        /**
         * Checks the credit memo balance.
         */
        private checkCreditMemoBalance(): void {
            this.indeterminateWaitVisible(true);

            this.paymentViewModel.checkCreditMemoAmount().done(() => {
                this.indeterminateWaitVisible(false);
                this.showCreditMemoBalance(true);
            }).fail((errors: Model.Entities.Error[]) => {
                this.showCreditMemoBalance(false);
                this.paymentErrorCallback(errors);
            });
        }

        /**
         * Change customer account.
         */
        private changeCustomerAccount(): void {
            var options: Operations.ICustomerSearchOperationOptions = {
                searchText: "",
                destination: "PaymentView", // navigate to view when complete add customer to sale
                destinationOptions: this._options
            };

            Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.CustomerSearch, options);
        }

        /**
         * Search customers.
         */
        private searchCustomers(): void {
            var options: Operations.ICustomerSearchOperationOptions = {
                searchText: "",
                destination: "PaymentView", // navigate to view when complete add customer to sale
                destinationOptions: this._options
            };

            Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.CustomerSearch, options);
        }

        /**
         * create new customer.
         */
        private createNewCustomer(): void {
            var options: Operations.ICustomerAddOperationOptions = {
                destination: "PaymentView", // navigate to view when complete add customer to sale
                destinationOptions: this._options
            };

            Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.CustomerAdd, options);
        }

        /**
         * Computes whether to show account details.
         * @return True: show account details; False: otherwise.
         */
        private computeShowAccountDetails(): boolean {
            var result = false;
            if (!ObjectExtensions.isNullOrUndefined(this.cartViewModel)) {
                result = !StringExtensions.isNullOrWhitespace(this.cartViewModel.cart().CustomerId);
            }
            return result;
        }

        /**
         * Computes the customer full address.
         * @return Customer full addresss.
         */
        private computeCustomerFullAddress(): string {
            if (ObjectExtensions.isNullOrUndefined(this.cartViewModel.customerAddress())
                || StringExtensions.isNullOrWhitespace(this.cartViewModel.customerAddress().FullAddress)) {
                return '';
            }

            return StringExtensions.replaceNewLineWithBr(this.cartViewModel.customerAddress().FullAddress);
        }

        /**
         * Computes the customer type.
         *
         * @return {string} Customer type.
         */
        private computeCustomerType(): string {
            return CustomerHelper.computeCustomerType(this.cartViewModel.customer());
        }

        /**
         * Gets whether cash back is allowed.
         *
         * @return True: Show cash back; False: otherwise.
         */
        private isCashBackAllowed(): boolean {
            var result = false;

            // Cashback is allowed when:
            // 1. The card information exists.
            // 2. The card information is for a payment.
            // 3. The payment is not for return
            // 4. The payment uses a debit card
            // 5. Cashback is allowed for the card type
            // 6. The payment is for the amount due (it is the final payment on the transaction)
            result = !ObjectExtensions.isNullOrUndefined(this.paymentViewModel.cardType())
            && !this._preloadPaymentCard
            && !this.paymentViewModel.isReturn()
            && this.paymentViewModel.isDebitCard()
            && this.paymentViewModel.cardType().CashBackLimit > 0
            && (this.paymentViewModel.getPaymentAmount() === this.paymentViewModel.fullAmount());

            return result;
        }

        /**
         * Returns true if the cashback amount can be retrieved from the UI, false otherwise.
         *
         * @return True: The cashback amount should be modified; False: otherwise.
         */
        private getCashBackFromUI(): boolean {
            return this.isCashBackAllowed() && (this.paymentViewModel.cardSource() != Model.Entities.CardSource.PaymentTerminal);
        }

        private get titlePaymentAmount(): string {
            return ViewModelAdapter.getResourceString(this._options.tokenizeCard ? "string_1173" : "string_1107");
        }

        private canUsePaymentTerminal(): boolean {
            return this.paymentViewModel.isCardPayment
                && Peripherals.instance.paymentTerminal
                && Peripherals.instance.paymentTerminal.isActive;
        }

        /**
         * Clear the credit card fields.
         */
        private clearCardFields(): void {
            this.paymentViewModel.resetPaymentCard();
            this.paymentViewModel.setPaymentCardType(null);
            this.paymentViewModel.updateCardInfo(null);
        }

        /**
         * Gets the card input options.
         *
         * @return {ValueCaptionPair[]} The card input options.
         */
        private getCardInputOptions(): ViewModels.ValueCaptionPair[] {

            var swipe = { caption: Commerce.ViewModelAdapter.getResourceString("string_1182"), value: 0 }; // Swipe card
            var manual = { caption: Commerce.ViewModelAdapter.getResourceString("string_1127"), value: 1 }; // Enter manually
            var voice = { caption: Commerce.ViewModelAdapter.getResourceString("string_1183"), value: 2 }; // Approval code

            if (!this._options.tokenizeCard) {
                return [swipe, manual, voice];
            }
            else {
                return [swipe, manual];
            }
        }

        /**
         * Reset amount to full amount due.
         */
        private setFullAmountDue(): void {
            this.paymentViewModel.resetPaymentAmount();
        }

        //TODO:AM Stage Capital
        private setOriginalAmountDue(): void {
            this.paymentViewModel.resetOriginalAmount();
        }

        /**
         * Handles card tokenization flow.
         *
         * @param {Model.Entities.CartTenderLine} tenderLine The cart tender line entity.
         */
        private tokenizeCardHandler(tenderLine: Commerce.Model.Entities.CartTenderLine) {
            var cardType: Model.Entities.CardTypeInfo = this.paymentViewModel.cardType();
            var cardTypeValue: Commerce.Model.Entities.CardType = cardType ? cardType.CardTypeValue : -1;

            var customerOrderPaymentCard: Model.Entities.CustomerOrderPaymentCard = <Model.Entities.CustomerOrderPaymentCard>{
                cardTypeId: tenderLine.CardTypeId,
                paymentCard: tenderLine.PaymentCard,
                tenderTypeId: tenderLine.TenderTypeId,
                cardTypeValue: cardTypeValue
            };

            var cartViewOptions: ICartViewControllerOptions = <ICartViewControllerOptions>{
                navigationSource: 'PaymentView',
                paymentCardForRemainingBalance: customerOrderPaymentCard
            };

            this._processingPayment = false;
            this.indeterminateWaitVisible(false);
            Commerce.ViewModelAdapter.navigate("CartView", cartViewOptions);
        }

        /**
         * Get preprocessed tender line.
         * 
         * @param {number} paymentAmount The payment amount.
         * @param {Commerce.Peripherals.HardwareStation.PaymentInfo} paymentInfo The payment information.
         * @return {Model.Entities.TenderLine} The preprocessed tender line.
         */
        private getPreprocessedTenderLine(paymentAmount: number, paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo): Model.Entities.TenderLine {
            // Get the type of card.
            var cardType: Model.Entities.CardTypeInfo = this.paymentViewModel.cardType();
            var cardTypeInfoId: string = cardType ? cardType.TypeId : "";

            var preProcessedTenderLine: Model.Entities.TenderLine = {
                Authorization: paymentInfo.PaymentSdkData,
                CardTypeId: cardTypeInfoId,
                Currency: Commerce.ApplicationContext.Instance.deviceConfiguration.Currency,
                MaskedCardNumber: paymentInfo.CardNumberMasked,
                TenderTypeId: Model.Entities.TenderTypeId.Cards.toString()
            };
            if (paymentAmount >= 0) {
                preProcessedTenderLine.CashBackAmount = paymentInfo.CashbackAmount,
                preProcessedTenderLine.Amount = paymentInfo.ApprovedAmount,
                preProcessedTenderLine.IsVoidable = true;
                preProcessedTenderLine.StatusValue = Model.Entities.TenderLineStatus.PendingCommit;
            }
            else {
                // Refund cannot be voided and doesn't require capture.
                preProcessedTenderLine.IsVoidable = false;

                // The refund api returns the refunded positive amount value. We convert it to negative value to indicate that it is a refund on the payment view and the tender line.
                preProcessedTenderLine.Amount = paymentInfo.ApprovedAmount * -1,
                preProcessedTenderLine.StatusValue = Model.Entities.TenderLineStatus.Committed;
            }

            // Set signature data that is returned from the authorization. The get signature flow handles this value.
            this.paymentViewModel.signatureData(paymentInfo.SignatureData);

            return preProcessedTenderLine;
        }

        /**
         * Authorize/refund payment when using payment terminal.
         * 
         * @param {number} paymentAmount The payment amount.
         * @param {boolean} isManual Requires manual entry on payment terminal.
         */
        private paymentTerminalAuthorizeRefund(paymentAmount: number, isManual: boolean, voiceApprovalCode: string) {
            this.isPaymentTerminalDialogVisible(true);
            var paymentTerminalAuthorizeRefundQueue: AsyncQueue = new AsyncQueue();
            var paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo = null;
            var preProcessedTenderLine: Model.Entities.TenderLine = null;
            var voidPayment: boolean = false;
            var voidPaymentMessageId: string = null;

            // Enqueue authorize/refund.
            paymentTerminalAuthorizeRefundQueue.enqueue(() => {
                if (paymentAmount >= 0) {
                    // Authorize payment.
                    return Peripherals.instance.paymentTerminal.authorizePayment(paymentAmount, voiceApprovalCode, isManual, null, this)
                        .done((result: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                            paymentInfo = result;
                        });
                }
                else {
                    // Refund payment.
                    // The refund api refunds the given positive amount value. We convert the negative value that is displayed in the payment UI to positive value before we send it for refund.
                    return Peripherals.instance.paymentTerminal.refundPayment(paymentAmount * -1, isManual, null, this)
                        .done((result: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                            paymentInfo = result;
                        });
                }
            });      
            
            // Enqueue check whether the payment was partially authorized
            paymentTerminalAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                this.isPaymentTerminalDialogVisible(false);

                // Check if payment info has been approved.
                if (paymentInfo && paymentInfo.IsApproved) {
                    // Continue the payment
                    asyncResult.resolve({ canceled: false });
                } else {
                    // Handle authorization failure.
                    voidPayment = false;
                    asyncResult.resolve({ canceled: true });
                }

                return paymentTerminalAuthorizeRefundQueue.cancelOn(asyncResult);
            });      

            // Enqueue get card type and get preprocessed tender line.
            paymentTerminalAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
                var cardInfo: Model.Entities.CardInfo = {
                    CardNumber: paymentInfo.CardNumberMasked,
                    CardTypeId: paymentInfo.CardType.toString()
                };

                // Set the payment card info and source.
                this.paymentViewModel.updateCardInfo(cardInfo, Model.Entities.CardSource.PaymentTerminal);
                this.getCardTypeAsync()
                    .done(() => {
                        // Get preprocessed tender line.
                        preProcessedTenderLine = this.getPreprocessedTenderLine(paymentAmount, paymentInfo);
                        voidPayment = false;
                        asyncResult.resolve({ canceled: false });

                    }).fail((errors: Model.Entities.Error[]) => {
                        // Handle set card type failure.
                        voidPayment = true;
                        voidPaymentMessageId = (paymentAmount >= 0) ? ErrorTypeEnum.PAYMENT_AUTHORIZED_VOID_FAILED : ErrorTypeEnum.PAYMENT_CAPTURED_VOID_FAILED;
                        asyncResult.reject(errors);
                    });

                return paymentTerminalAuthorizeRefundQueue.cancelOn(asyncResult);
            }); 
            
            // Enqueue check whether the payment was partially authorized
            paymentTerminalAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);

                // If payment was partially authorized, ask the user whether to continue with the partially authorized amount
                // This dialog does not apply to refund scenario as payment amount cannot be partially refunded and refund cannot be voided
                if (paymentAmount >= 0 && paymentAmount != paymentInfo.ApprovedAmount) {
                    var currency: string = ApplicationContext.Instance.deviceConfiguration.Currency;
                    var approvePartialAmountDialogOptions: Commerce.Controls.ApprovePartialAmountDialogOptions = {
                        amountAuthorized: paymentInfo.ApprovedAmount,
                        amountRequested: paymentAmount,
                        amountAuthorizedCurrencyCode: currency,
                        amountRequestedCurrencyCode: currency
                    };

                    this.showDialog(this._approvePartialAmountDialog, approvePartialAmountDialogOptions)
                        .on(DialogResult.OK, () => {
                            // Set the actual payment amount
                            var paymentAmountText: string = NumberExtensions.formatCurrency(paymentInfo.ApprovedAmount, currency);
                            this.paymentViewModel.paymentAmountText(paymentAmountText);
                            asyncResult.resolve({ canceled: false });
                        }).on(DialogResult.Cancel, () => {
                            // Set the values to void the payment
                            voidPayment = true;
                            voidPaymentMessageId = "string_7205"; // The attempt to void the partial payment failed. Would you like to try and void the partial payment again?
                            asyncResult.reject(null);
                        });
                } else {
                    // Handle payment 
                    asyncResult.resolve({ canceled: false });
                }

                return paymentTerminalAuthorizeRefundQueue.cancelOn(asyncResult);
            });

            // Enqueue add preprocessed tender line to cart.
            paymentTerminalAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);

                // Get the type of card.
                var cardType: Model.Entities.CardTypeInfo = this.paymentViewModel.cardType();
                var cardTypeValue: Commerce.Model.Entities.CardType = cardType ? cardType.CardTypeValue : -1;

                // Get the original values of the amounts before addPreprocessedTenderLine changes them to persist the cashback value
                var amount: number = preProcessedTenderLine.Amount;
                var cashBackAmount: number = preProcessedTenderLine.CashBackAmount;

                // Add a tender line to the cart with the processed payment
                this.cartViewModel.addPreprocessedTenderLineToCart(preProcessedTenderLine)
                    .done(() => {
                        voidPayment = false;
                        asyncResult.resolve({ canceled: false });
                    })
                    .fail((errors) => {
                        // Handle add preprocessed tender line to cart failure.
                        voidPayment = true;
                        var messageAuthorized: string = "string_1189"; // The payment was authorized but could not be added to the transaction. The attempt to void the payment failed. Would you like to try and void the payment again?
                        var messageCaptured: string = "string_1190"; // The payment was captured but could not be added to the transaction. The attempt to void the payment failed. Would you like to try and void the payment again?
                        voidPaymentMessageId = (paymentAmount >= 0) ? messageAuthorized : messageCaptured;
                        asyncResult.reject(errors);
                    });

                return paymentTerminalAuthorizeRefundQueue.cancelOn(asyncResult);
            });

            // Run the queue.
            paymentTerminalAuthorizeRefundQueue.run()
                .done((result: Commerce.ICancelableResult) => {
                    if (ObjectExtensions.isNullOrUndefined(result) || !result.canceled) {
                        // Get signature data if queue runs successfully.
                        this.getSignature();
                    } else {
                        // Handle authorize/refund failure.  
                        this.paymentTerminalAuthRefundFail([new Model.Entities.Error(ErrorTypeEnum.PAYMENT_UNABLE_AUTHORIZE_OR_REFUND)]);
                    }
                }).fail((errors: Commerce.Model.Entities.Error[]) => {
                    if (voidPayment) {
                        // Attempt to void the payment
                        var voidPaymentCall = () => {
                            Peripherals.instance.paymentTerminal.voidPayment(paymentInfo.ApprovedAmount, paymentInfo.PaymentSdkData, null, this)
                                .done((voidInfo: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                                    if (voidInfo && voidInfo.IsApproved) {

                                        // Add voided tender line to cart
                                        preProcessedTenderLine.StatusValue = Model.Entities.TenderLineStatus.Voided;
                                        this.cartViewModel.addPreprocessedTenderLineToCart(preProcessedTenderLine)
                                            .done(() => {
                                                this.paymentErrorCallback(errors);
                                                this.paymentCompleted();
                                            })
                                            .fail((errors) => {
                                                // Handle add voided tender line to cart failure.
                                                this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors));
                                            });
                                    }
                                    else {
                                        this.voidPaymentFailureHandler(voidPaymentCall, voidPaymentMessageId);
                                    }
                                }).fail((errors: Model.Entities.Error[]) => {
                                    this.voidPaymentFailureHandler(voidPaymentCall, voidPaymentMessageId, errors);
                                });
                        };

                        voidPaymentCall();
                    }
                    else {
                        // Handle authorize/refund request failure.
                        this.paymentTerminalAuthRefundFail(errors);
                    }
                });
        }

        /**
         * Authorize/refund card payment when not using a payment terminal.
         * 
         * @param {Model.Entities.CartTenderLine} tenderLine The card tender line entity.
         */
        private cardPaymentAuthorizeRefund(tenderLine: Model.Entities.CartTenderLine) {
            var cardPaymentAuthorizeRefundQueue: AsyncQueue = new AsyncQueue();
            var paymentInfo: Commerce.Peripherals.HardwareStation.PaymentInfo = null;
            var preProcessedTenderLine: Model.Entities.TenderLine = null;
            var voidPayment: boolean = false;
            var paymentAmount: number = this.paymentViewModel.getPaymentAmount();
            var voidPaymentMessageId: string = null;

            // Create tenderInfo object
            var tenderInfo: Commerce.Peripherals.HardwareStation.TenderInfo = {
                TenderId: Model.Entities.TenderTypeId.Cards.toString(),
                CardNumber: tenderLine.PaymentCard.CardNumber,
                CardTypeId: tenderLine.PaymentCard.CardTypeId,
                Track1: tenderLine.PaymentCard.Track1,
                Track2: tenderLine.PaymentCard.Track2,
                Track3: tenderLine.PaymentCard.Track3,
                EncryptedPin: tenderLine.PaymentCard.EncryptedPin,
                AdditionalSecurityData: tenderLine.PaymentCard.AdditionalSecurityData,
                CCID: tenderLine.PaymentCard.CCID,
                VoiceAuthorizationCode: tenderLine.PaymentCard.VoiceAuthorizationCode,
                IsSwipe: tenderLine.PaymentCard.IsSwipe,
                Name: tenderLine.PaymentCard.NameOnCard,
                Country: tenderLine.PaymentCard.Country,
                Address: tenderLine.PaymentCard.Address1,
                Zip: tenderLine.PaymentCard.Zip,
                ExpirationMonth: tenderLine.PaymentCard.ExpirationMonth,
                ExpirationYear: tenderLine.PaymentCard.ExpirationYear,
                CashbackAmount: tenderLine.CashBackAmount
            };

            // Enqueue authorize/refund.
            cardPaymentAuthorizeRefundQueue.enqueue(() => {
                if (paymentAmount >= 0) {
                    // Authorize payment.
                    return Peripherals.instance.cardPayment.authorizePayment(paymentAmount, tenderInfo, null, this)
                        .done((result: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                            paymentInfo = result;
                        });
                }
                else {
                    // Refund payment.
                    // The refund api refunds the given positive amount value. We convert the negative value that is displayed in the payment UI to positive value before we send it for refund.
                    return Peripherals.instance.cardPayment.refundPayment(paymentAmount * -1, tenderInfo, null, this)
                        .done((result: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                            paymentInfo = result;
                        });
                }
            });

            // Enqueue get preprocessed tender line.
            cardPaymentAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);

                // Check if payment info has been approved.
                if (paymentInfo && paymentInfo.IsApproved) {
                    // Get preprocessed tender line.
                    preProcessedTenderLine = this.getPreprocessedTenderLine(paymentAmount, paymentInfo);
                    voidPayment = false;
                    asyncResult.resolve({ canceled: false });
                }
                else {
                    // Handle authorization/refund failure.
                    voidPayment = false;
                    asyncResult.resolve({ canceled: true });
                }

                return cardPaymentAuthorizeRefundQueue.cancelOn(asyncResult);
            });

            // Enqueue check whether the payment was partially authorized
            cardPaymentAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);
 
                // Check if payment info has been approved.
                if (paymentInfo && paymentInfo.IsApproved) {
                    // If payment was partially authorized, ask the user whether to continue with the partially authorized amount
                    // This dialog does not apply to refund scenario as payment amount cannot be partially refunded and refund cannot be voided
                    if (paymentAmount >= 0 && paymentAmount != paymentInfo.ApprovedAmount) {
                        var currency: string = ApplicationContext.Instance.deviceConfiguration.Currency;
                        var approvePartialAmountDialogOptions: Commerce.Controls.ApprovePartialAmountDialogOptions = {
                            amountAuthorized: paymentInfo.ApprovedAmount,
                            amountRequested: paymentAmount,
                            amountAuthorizedCurrencyCode: currency,
                            amountRequestedCurrencyCode: currency
                        };

                        this.showDialog(this._approvePartialAmountDialog, approvePartialAmountDialogOptions)
                            .on(DialogResult.OK, () => {
                                // Set the actual payment amount
                                var paymentAmountText: string = NumberExtensions.formatCurrency(paymentInfo.ApprovedAmount, currency);
                                this.paymentViewModel.paymentAmountText(paymentAmountText);
                                asyncResult.resolve({ canceled: false });
                            }).on(DialogResult.Cancel, () => {
                                // Set the values to void the payment
                                voidPayment = true;
                                voidPaymentMessageId = "string_7205"; // The attempt to void the partial payment failed. Would you like to try and void the partial payment again?
                                asyncResult.reject(null);
                            });
                    } else {
                        // Handle payment 
                        asyncResult.resolve({ canceled: false });
                    }
                } else {
                    // Handle authorization failure.
                    voidPayment = false;
                    asyncResult.resolve({ canceled: true });
                }

                return cardPaymentAuthorizeRefundQueue.cancelOn(asyncResult);
            }); 

            // Enqueue add preprocessed tender line to cart.
            cardPaymentAuthorizeRefundQueue.enqueue(() => {
                var asyncResult = new AsyncResult<ICancelableResult>(null);

                // Get the type of card.
                var cardType: Model.Entities.CardTypeInfo = this.paymentViewModel.cardType();
                var cardTypeValue: Commerce.Model.Entities.CardType = cardType ? cardType.CardTypeValue : -1;

                // Get the original values of the amounts before addPreprocessedTenderLine changes them to persist the cashback value
                var amount: number = preProcessedTenderLine.Amount;
                var cashBackAmount: number = preProcessedTenderLine.CashBackAmount;

                // Add a tender line to the cart with the processed payment
                this.cartViewModel.addPreprocessedTenderLineToCart(preProcessedTenderLine)
                    .done(() => {
                        voidPayment = false;
                        asyncResult.resolve({ canceled: false });
                    })
                    .fail((errors) => {
                        // Handle add preprocessed tender line to cart failure.
                        voidPayment = true;
                        voidPaymentMessageId = (paymentAmount >= 0) ? ErrorTypeEnum.PAYMENT_AUTHORIZED_VOID_FAILED : ErrorTypeEnum.PAYMENT_CAPTURED_VOID_FAILED;
                        asyncResult.reject(errors);
                    });

                return cardPaymentAuthorizeRefundQueue.cancelOn(asyncResult);
            });

            // Run the queue.
            cardPaymentAuthorizeRefundQueue.run()
                .done((result: Commerce.ICancelableResult) => {
                    if (ObjectExtensions.isNullOrUndefined(result) || !result.canceled) {
                        // Get signature data if queue runs successfully.
                        this.getSignature();
                    } else {
                        // Handle authorize/refund failure.  
                        this.paymentErrorCallback([new Model.Entities.Error(ErrorTypeEnum.PAYMENT_UNABLE_AUTHORIZE_OR_REFUND)]);
                    }
                })
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    if (voidPayment) {
                        // Attempt to void the payment
                        var voidPaymentCall = () => {
                            Peripherals.instance.cardPayment.voidPayment(paymentInfo.ApprovedAmount, paymentInfo.PaymentSdkData, null, this)
                                .done((voidInfo: Commerce.Peripherals.HardwareStation.PaymentInfo) => {
                                    if (voidInfo && voidInfo.IsApproved) {
                                        // Add voided tender line to cart
                                        preProcessedTenderLine.StatusValue = Model.Entities.TenderLineStatus.Voided;
                                        this.cartViewModel.addPreprocessedTenderLineToCart(preProcessedTenderLine)
                                            .done(() => {
                                                this.paymentErrorCallback(errors);
                                                this.paymentCompleted();
                                            })
                                            .fail((errors) => {
                                                // Handle add voided tender line to cart failure.
                                                this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors));
                                            });
                                    }
                                    else {
                                        this.voidPaymentFailureHandler(voidPaymentCall, voidPaymentMessageId);
                                    }
                                }).fail((errors: Model.Entities.Error[]) => {
                                    this.voidPaymentFailureHandler(voidPaymentCall, voidPaymentMessageId, errors);
                                });
                        };

                        voidPaymentCall();
                    } else {
                        this.paymentErrorCallback(errors);
                    }
                });
        }

        /**
         * Submit payment to card payment accept page.
         * 
         * @param {number} paymentAmount The payment amount.
         */
        private cardPaymentAcceptSubmitPayment(paymentAmount: number) {
            var asyncQueue = new AsyncQueue();
            var filteredCreditCardTypes: Commerce.Model.Entities.CardTypeInfo[];

            if (!StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptCardPrefix)) {

                asyncQueue.enqueue(() => {
                    // Filter card types based on card prefix.
                    return this.paymentViewModel.filterCreditCardTypesAsync(this.cardPaymentAcceptCardPrefix)
                        .done((types: Commerce.Model.Entities.CardTypeInfo[]) => {
                            filteredCreditCardTypes = types;
                        });
                });

                asyncQueue.enqueue(() => {
                    var asyncResult = new VoidAsyncResult(null);

                    if (filteredCreditCardTypes.length === 0) {
                        // Filtered card types is empty
                        asyncResult.reject([new Commerce.Model.Entities.Error(ErrorTypeEnum.PAYMENT_CARD_NOT_SUPPORTED)]);
                    } else if (filteredCreditCardTypes.length === 1) {
                        // Exactly one filtered card type is available
                        this.cardPaymentAcceptCardTypeInfo = filteredCreditCardTypes[0];
                        asyncResult.resolve();
                    } else {
                        // Got multiple card types: Show select payment card type dialog.
                        this.showDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog, filteredCreditCardTypes)
                            .on(DialogResult.OK, (cardTypeInfo: Commerce.Model.Entities.CardTypeInfo) => {
                                this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                    .done(() => {
                                        this.cardPaymentAcceptCardTypeInfo = cardTypeInfo;
                                        asyncResult.resolve();
                                    });
                            }).on(DialogResult.Cancel, () => {
                                this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                asyncResult.reject(null);
                            });
                    }

                    return asyncResult;
                });

                // Validate tender line for add in authorize scenario.
                if (!this._options.tokenizeCard && paymentAmount >= 0) {

                    asyncQueue.enqueue(() => {
                        var asyncResult = new VoidAsyncResult(null);

                        var tenderLineToValidate: Commerce.Model.Entities.TenderLine = {
                            Amount: paymentAmount,
                            Currency: this.paymentViewModel.currency(),
                            TenderTypeId: Model.Entities.TenderTypeId.Cards.toString(),
                            CardTypeId: this.cardPaymentAcceptCardTypeInfo.TypeId
                        };

                        // Validate tender line
                        this.cartViewModel.validateTenderLineForAdd(tenderLineToValidate)
                            .done(() => {
                                asyncResult.resolve();
                            })
                            .fail((errors: Commerce.Model.Entities.Error[]) => {
                                asyncResult.reject(errors);
                            });

                        return asyncResult;
                    });
                }
            }

            asyncQueue.run()
                .done((result) => {
                    this.submitCardPaymentAcceptPayment(paymentAmount);
                })
                .fail((errors: Commerce.Model.Entities.Error[]) => {
                    this.paymentErrorCallback(errors);
                });
        }

        /**
         * Retrieve card payment accept result.
         */
        private retrieveCardPaymentAcceptResult() {
            var paymentAmount: number = this.paymentViewModel.getPaymentAmount();
            var isAuthorize: boolean = !this._options.tokenizeCard && paymentAmount >= 0;
            var isRefund: boolean = !this._options.tokenizeCard && paymentAmount < 0;

            var asyncQueue = new AsyncQueue();
            var cardPaymentAcceptResult: Model.Entities.CardPaymentAcceptResult;

            asyncQueue.enqueue(() => {
                return this.cartViewModel.cartManager.retrieveCardPaymentAcceptResult(this.cardPaymentAcceptResultAccessCode)
                    .done((result: Model.Entities.CardPaymentAcceptResult) => {
                        if (!ObjectExtensions.isNullOrUndefined(result) &&
                            ((isAuthorize && !ObjectExtensions.isNullOrUndefined(result.TenderLine)) ||
                                ((isRefund || this._options.tokenizeCard) && !ObjectExtensions.isNullOrUndefined(result.TokenizedPaymentCard)))) {

                            cardPaymentAcceptResult = result;
                        } else {
                            // Handle retrieve card payment accept result failure.
                            this.paymentErrorCallback([new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETORETRIEVECARDPAYMENTACCEPTRESULT)]);
                        }
                    }).fail((errors) => {
                        // Handle retrieve card payment accept result call failure.
                        this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors));
                    });
            });

            
            if (ObjectExtensions.isNullOrUndefined(this.cardPaymentAcceptCardTypeInfo)) {
                // This happens when the accepting page did not send the card prefix.
                // Try to get the card prefix from the tokenized payment card
                var filteredCreditCardTypes: Commerce.Model.Entities.CardTypeInfo[];

                asyncQueue.enqueue(() => {
                    this.cardPaymentAcceptCardPrefix = cardPaymentAcceptResult.TokenizedPaymentCard.CardTokenInfo.MaskedCardNumber;

                    // Filter card types based on card prefix.
                    return this.paymentViewModel.filterCreditCardTypesAsync(this.cardPaymentAcceptCardPrefix)
                        .done((types: Commerce.Model.Entities.CardTypeInfo[]) => {
                            filteredCreditCardTypes = types;
                        });
                });

                asyncQueue.enqueue(() => {
                    var asyncResult = new VoidAsyncResult(null);

                    if (filteredCreditCardTypes.length === 0) {
                        // Filtered card types is empty
                        this.indeterminateWaitVisible(false);
                        asyncResult.reject([new Commerce.Model.Entities.Error(ErrorTypeEnum.PAYMENT_CARD_NOT_SUPPORTED)]);
                    } else if (filteredCreditCardTypes.length === 1) {
                        // Exactly one filtered card type is available
                        this.cardPaymentAcceptCardTypeInfo = filteredCreditCardTypes[0];
                        asyncResult.resolve();
                    } else {
                        // Got multiple card types: Show select payment card type dialog.
                        this.showDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog, filteredCreditCardTypes)
                            .on(DialogResult.OK, (cardTypeInfo: Commerce.Model.Entities.CardTypeInfo) => {
                                this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                    .done(() => {
                                        this.cardPaymentAcceptCardTypeInfo = cardTypeInfo;
                                        asyncResult.resolve();
                                    });
                            }).on(DialogResult.Cancel, () => {
                                this.closeDialog<Commerce.Model.Entities.CardTypeInfo, any>(this._cardTypeDialog)
                                this.cardPaymentAcceptCardTypeInfo = filteredCreditCardTypes[0];
                                asyncResult.resolve();
                            });
                    }

                    return asyncResult;
                });
            }

            asyncQueue.enqueue(() => {
                var asyncResult = new VoidAsyncResult(null);

                if (isAuthorize) {
                    // Authorize scenario.
                    cardPaymentAcceptResult.TenderLine.CardTypeId = this.cardPaymentAcceptCardTypeInfo.TypeId;
                    this.handleCardPaymentAcceptAuthorize(cardPaymentAcceptResult);
                } else if (isRefund) {
                    // Refund scenario.
                    cardPaymentAcceptResult.TokenizedPaymentCard.CardTypeId = this.cardPaymentAcceptCardTypeInfo.TypeId;
                    this.handleCardPaymentAcceptRefund(cardPaymentAcceptResult, paymentAmount);
                } else {
                    // Recover card information scenario.
                    // Set the tokenized payment card object and navigate to cart view.
                    cardPaymentAcceptResult.TokenizedPaymentCard.CardTypeId = this.cardPaymentAcceptCardTypeInfo.TypeId;
                    cardPaymentAcceptResult.TokenizedPaymentCard.TenderType = Model.Entities.TenderTypeId.Cards.toString();
                    var cartViewOptions: ICartViewControllerOptions = <ICartViewControllerOptions>{
                        navigationSource: 'PaymentView',
                        tokenizedPaymentCard: cardPaymentAcceptResult.TokenizedPaymentCard
                    };

                    this._processingPayment = false;
                    this.indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CartView", cartViewOptions);
                }

                asyncResult.resolve();
                return asyncResult;
            });

            asyncQueue.run();
        }

        /**
         * Void payment failure handler.
         * 
         * @param {Function} voidPaymentCall The callback function to invoke to retry void payment.
         * @param {Model.Entities.Error[]} [errors] The void payment errors.
         */
        private voidPaymentFailureHandler(voidPaymentCall: Function, voidPaymentMessageId: string, errors?: Model.Entities.Error[]) {
            var title: string = Commerce.ViewModelAdapter.getResourceString("string_4909"); // Peripheral error
            var voidPaymentMessage = Commerce.ViewModelAdapter.getResourceString(voidPaymentMessageId);
            Commerce.ViewModelAdapter.displayMessage(voidPaymentMessage, MessageType.Info, MessageBoxButtons.RetryNo, title)
                .done((result: Commerce.DialogResult) => {
                    if (result === Commerce.DialogResult.OK) {
                        voidPaymentCall();
                    } else {
                        // Set the payment terminal to display the transaction again
                        Commerce.Peripherals.instance.paymentTerminal.displayTransaction(Commerce.Session.instance.cart, this);
                        this.paymentErrorCallback(errors || null);
                    }
                });
        }

        /**
         * Payment terminal authorize/refund request failure handler.
         * 
         * @param {Model.Entities.Error[]} [errors] The void payment errors.
         */
        private paymentTerminalAuthRefundFail(errors: Model.Entities.Error[]) {
            this.isPaymentTerminalDialogVisible(false);
            this.paymentErrorCallback(errors);

            // Set the terminal to display the transaction again.
            Commerce.Peripherals.instance.paymentTerminal.displayTransaction(Commerce.Session.instance.cart, this);
        }

        /**
         * Submits payment to the external card payment accept page.
         * 
         * @param {number} paymentAmount The payment amount.
         */
        private submitCardPaymentAcceptPayment(paymentAmount: number): void {

            if (!StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptPageSubmitUrl)) {
                // If present, use submit URL to trigger submit
                var submitUrl = this.cardPaymentAcceptPageSubmitUrl;
                var d = new Date();
                submitUrl = submitUrl + "#" + d.getTime();
                this.cardPaymentAcceptPageUrl(submitUrl);
            }
            else if (!StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptMessageOrigin)) {
                // Do nothing if the origin of the card page is unavailable
                // Send a message to the card page to trigger submit
                var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                var cardPaymentAcceptMessage: CardPaymentAcceptMessage;
                if (!this._options.tokenizeCard && paymentAmount >= 0) {
                    cardPaymentAcceptMessage = {
                        type: PaymentViewController.CARDPAYMENTACCEPTAMOUNT,
                        value: NumberExtensions.formatNumber(paymentAmount, NumberExtensions.getDecimalPrecision())
                    }
                }
                
                cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), this.cardPaymentAcceptMessageOrigin);
                cardPaymentAcceptMessage = {
                    type: PaymentViewController.CARDPAYMENTACCEPTSUBMIT,
                    value: "true"
                }

                cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), this.cardPaymentAcceptMessageOrigin);

                // Only show the progress bar when there is a message from the accepting page (the page is loaded).
                if (this.isCardPaymentAcceptHeightReceived) {
                    this.indeterminateWaitVisible(true);
                }
            }
        }

        /**
         * Sends card swipe data to the external card payment accept page.
         */
        private sendCardPaymentAcceptSwipe(): void {

            // Only sends the swipe data once
            if (this.isCardPaymentAcceptSwipeSent) {
                return;
            }
            else {
                this.isCardPaymentAcceptSwipeSent = true;
            }

            if (this.paymentViewModel.paymentCard == null) {
                return;
            }

            // Recreate swipe data from track1 and track2.
            var swipe = this.paymentViewModel.paymentCard.Track1() + this.paymentViewModel.paymentCard.Track2();

            this.sendCardPaymentAcceptSwipeMessage(swipe);
        }

        /**
         * Sends a message about card swipe data to the external card payment accept page.
         * 
         * @param {boolean} partialOK The value indicating whether the partial authorization is accepted.
         */
        private sendCardPaymentAcceptSwipeMessage(swipe: string): void {
            if (swipe.length == 0) {
                return;
            }

            // Do nothing if the origin of the card page is unavailable
            if (!StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptMessageOrigin)) {
                // Send a message to the card page about swipe data
                var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                var cardPaymentAcceptMessage: CardPaymentAcceptMessage;
                cardPaymentAcceptMessage = {
                    type: PaymentViewController.CARDPAYMENTACCEPTSWIPE,
                    value: swipe
                }

                cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), this.cardPaymentAcceptMessageOrigin);
            }
        }

        /**
         * Sends the confirmation result (true/false) of partial authorization to the external payment accept page.
         * 
         * @param {boolean} partialOK The value indicating whether the partial authorization is accepted.
         */
        private sendCardPaymentAcceptPartialOK(partialOK: boolean): void {
            // Do nothing if the origin of the card page is unavailable
            if (!StringExtensions.isNullOrWhitespace(this.cardPaymentAcceptMessageOrigin)) {
                // Send a message to the card page to trigger submit
                var cardPaymentAcceptIframe = <HTMLIFrameElement>document.getElementById("cardPaymentAcceptFrame");
                var cardPaymentAcceptMessage: CardPaymentAcceptMessage;
                cardPaymentAcceptMessage = {
                    type: PaymentViewController.CARDPAYMENTACCEPTPARTIALOK,
                    value: partialOK.toString()
                }

                cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), this.cardPaymentAcceptMessageOrigin);
                this.indeterminateWaitVisible(true);
            }
        }

        /**
         * Handles the card payment accept authorize flow.
         *
         * @param {Model.Entities.CardPaymentAcceptResult} result The card payment accept result.
         */
        private handleCardPaymentAcceptAuthorize(result: Model.Entities.CardPaymentAcceptResult): void {
            var preProcessedTenderLine: Model.Entities.TenderLine = result.TenderLine;
            preProcessedTenderLine.TenderTypeId = Model.Entities.TenderTypeId.Cards.toString();

            // Add a tender line to the cart with the processed payment
            this.cartViewModel.addPreprocessedTenderLineToCart(preProcessedTenderLine)
                .done(() => {
                    // Add tender line info to session.
                    var capturePaymentType = (preProcessedTenderLine.StatusValue === Model.Entities.TenderLineStatus.Committed) ? Commerce.Model.Entities.PeripheralPaymentType.CardPaymentAccept : Commerce.Model.Entities.PeripheralPaymentType.None;
                    var cardType: Commerce.Model.Entities.CardType = this.cardPaymentAcceptCardTypeInfo ? this.cardPaymentAcceptCardTypeInfo.CardTypeValue : -1;
                    
                    // The result will contain error if the capture had failed and payment was voided. 
                    // Displaying the error after adding the voided payment line to cart.
                    if (ArrayExtensions.hasElements(result.PaymentSdkErrors)) {
                        var error: Model.Entities.Error = new Model.Entities.Error(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETORETRIEVECARDPAYMENTACCEPTRESULT);
                        var paymentException: Model.Entities.PaymentException = new Model.Entities.PaymentExceptionClass();
                        paymentException.PaymentSdkErrors = result.PaymentSdkErrors;
                        error.commerceException = paymentException;

                        this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors([error])).always(() => {
                            this.paymentCompleted();
                        });
                    }
                    else {
                        this.paymentCompleted();
                    }
                })
                .fail((errors) => {
                    // Handle add preprocessed tender line to cart failure.
                    this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors)).always(() => {
                        Commerce.ViewModelAdapter.navigate("CartView", <ICartViewControllerOptions>{ navigationSource: "PaymentView" });
                    });
                });
        }

        /**
         * Handles the card payment accept refund flow.
         * 
         * @param {Model.Entities.CardPaymentAcceptResult} result The card payment accept result.
         * @param {number} paymentAmount The payment amount.
         */
        private handleCardPaymentAcceptRefund(result: Model.Entities.CardPaymentAcceptResult, paymentAmount: number): void {

            var cartTenderLine: Model.Entities.CartTenderLine = {
                TokenizedPaymentCard: result.TokenizedPaymentCard,
                Amount: paymentAmount,
                TenderTypeId: Model.Entities.TenderTypeId.Cards.toString(),
                Currency: ApplicationContext.Instance.deviceConfiguration.Currency,
                CardTypeId: this.cardPaymentAcceptCardTypeInfo.TypeId
            };

            // Add a tender line to the cart with tokenized payment card for refund
            this.cartViewModel.addCartTenderLineToCart(cartTenderLine)
                .done(() => {
                    this.paymentCompleted();
                })
                .fail((errors) => {
                    // Handle add cart tender line to cart failure.
                    this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors)).always(() => {
                        Commerce.ViewModelAdapter.navigate("CartView", <ICartViewControllerOptions>{ navigationSource: "PaymentView" });
                    });
                });
        }

        /**
         * Handles messages from the card payment accept page.
         * 
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
                    case PaymentViewController.CARDPAYMENTACCEPTHEIGHT:
                        // Send card swipe data
                        this.isCardPaymentAcceptHeightReceived = true;
                        this.sendCardPaymentAcceptSwipe();
                        break;
                    case PaymentViewController.CARDPAYMENTACCEPTCARDPREFIX:
                        this.cardPaymentAcceptCardPrefix = messageObject.value;

                        break;
                    case PaymentViewController.CARDPAYMENTACCEPTERROR:
                        // Show errors
                        var paymentErrors = messageObject.value;
                        var errors: Model.Entities.Error[] = [];
                        for (var i = 0; i < paymentErrors.length; i++) {
                            errors.push(new Model.Entities.Error(paymentErrors[i].Code.toString(), false, paymentErrors[i].Message));
                        }

                        this.paymentErrorCallback(PaymentErrorHelper.ConvertToClientErrors(errors));
                        break;
                    case PaymentViewController.CARDPAYMENTACCEPTPARTIALAMOUNT:
                        // Show partial approval confirmation dialog
                        var paymentAmount: number = this.paymentViewModel.getPaymentAmount();
                        var approvedAmount: number = NumberExtensions.parseNumber(messageObject.value);
                        var currency: string = ApplicationContext.Instance.deviceConfiguration.Currency;
                        var approvePartialAmountDialogOptions: Commerce.Controls.ApprovePartialAmountDialogOptions = {
                            amountAuthorized: approvedAmount,
                            amountRequested: paymentAmount,
                            amountAuthorizedCurrencyCode: currency,
                            amountRequestedCurrencyCode: currency
                        };
                        this.showDialog(this._approvePartialAmountDialog, approvePartialAmountDialogOptions)
                            .on(DialogResult.OK, () => {
                                // Set the actual payment amount
                                var paymentAmountText: string = NumberExtensions.formatCurrency(approvedAmount, currency);
                                this.paymentViewModel.paymentAmountText(paymentAmountText);
                                
                                // Notify payment accepting page the partial authorization is accepted.
                                this.sendCardPaymentAcceptPartialOK(true);
                            }).on(DialogResult.Cancel, () => {
                                // Notify payment accepting page the partial authorization is not accepted.
                                this.sendCardPaymentAcceptPartialOK(false);
                            });
                        break;
                    case PaymentViewController.CARDPAYMENTACCEPTRESULT:
                        // Submit the order
                        this.indeterminateWaitVisible(true);
                        this.cardPaymentAcceptResultAccessCode = messageObject.value; 
                        this.retrieveCardPaymentAcceptResult();
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
            // This is to remove event listener in case it had not been removed already. 
            // This may happen if there was an error and removeCardPaymentAcceptListener call was not executed.
            this.removeCardPaymentAcceptListener();
            window.addEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
        }

        /**
         * Unregister event that receives and handles message from card payment accept page.
         */
        public removeCardPaymentAcceptListener(): void {
            window.removeEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
        }

        //
        // Dialog helper methods
        //

        /**
         * Shows a modal dialog and handle default results.
         *
         * @param  {Controls.ModalDialog<T, U><U>} The dialog to open
         * @param  {T} The input to the dialog
         * @return  {IAsyncDialogResult<U>} The return result from the dialog
         */
        private showDialog<T, U>(dialog: Controls.ModalDialog<T, U>, input: T): IAsyncDialogResult<U> {
            return dialog.show(input);
        }

        /**
         * Closes a modal dialog.
         *
         * @param  {Controls.ModalDialog<T, U><U>} The dialog to close
         * @return  {IVoidAsyncResult} The void return result
         */
        private closeDialog<T, U>(dialog: Controls.ModalDialog<T, U>): IVoidAsyncResult {
            return dialog.hide();
        }

        /**
         * Creates a payment view model instance for the controller.
         *
         * @param  {Commerce.Operations.IPaymentViewOptions} The options
         * @return  {ViewModels.PaymentViewModel} The payment view model
         */
        private createPaymentViewModel(options: Commerce.Operations.IPaymentOperationOptions): ViewModels.PaymentViewModel {
            var viewModel = new Commerce.ViewModels.PaymentViewModel(options.tenderType, options.paymentAmount());
            if (!Commerce.StringExtensions.isNullOrWhitespace(options.currentPaymentAmountText)) {
                viewModel.paymentAmountText(options.currentPaymentAmountText);
            }

            viewModel.paymentAmountText.subscribe((newValue: string) => {
                options.currentPaymentAmountText = newValue;
            });

            if (!Commerce.StringExtensions.isNullOrWhitespace(options.loyaltyCardId)) {
                viewModel.loyaltyCardId(options.loyaltyCardId);
            }

            return viewModel;
        }

        private scannerMessageEventHandler(barcode: string): void {
            var viewModel: ViewModels.PaymentViewModel = this.paymentViewModel;
            if (viewModel.isGiftCardPayment || viewModel.isLoyaltyCardPayment) {
                viewModel.updateCardInfo(<Model.Entities.CardInfo> { CardNumber: barcode });
            } else if (viewModel.isCreditMemoPayment) {
                viewModel.creditMemoId(barcode);
            }
        }
    }
}
