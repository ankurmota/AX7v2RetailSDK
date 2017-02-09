/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Proxy.Entities;

    /**
      * Represents the tender function enum. This enum maps to the AX RetailTenderFunction enum. 
      */
    export class TenderFunctionEnum {
        static Normal: number = 0;
        static Card: number = 1;
        static Check: number = 2;
        static Customer: number = 3;
        static TenderRemoveFloat: number = 4;
    }

    /**
     * Represents the signature mode.
     */
    export class SignatureModeEnum {
        static Init: number = 0;
        static Reset: number = 1;
        static Save: number = 2;
    }

    /**
     * Represents the value caption pair.
     */
    export interface ValueCaptionPair {
        value: number;
        caption: string;
    }

    /**
     * Represents the payment view model.
     */
    export class PaymentViewModel extends ViewModelBase {
        // Common properties
        public tenderTypeId: Observable<string>;
        public tenderTypeName: Computed<string>;
        public operationId: Observable<number>;
        public fullAmount: Observable<number>;
        public paymentAmountText: Observable<string>;
        public paymentAmountTextAsCurrency: Observable<string>;
        public originalAmount: Observable<number>;//TODO:AM
        public originalAmountTextAsCurrency: Observable<string>; //TODO:AM
        public isSignatureRequired: Computed<boolean>;
        public signatureData: Observable<string>;
        public countries: Model.Entities.CountryRegionInfo[] = Commerce.ApplicationContext.Instance.Countries;

        // Cash and currency properties
        public currency: Observable<string>;
        public currencyDenominations: ObservableArray<Commerce.Model.Entities.CashDeclaration>;
        public currencyAmounts: ObservableArray<Commerce.Model.Entities.CurrencyAmount>;
        private _currencyDenominationMap = []; // This is an object "hash table" of the cash declarations by currency

        // Card payment specific properties
        public months: ObservableArray<ValueCaptionPair>;
        public years: ObservableArray<ValueCaptionPair>;
        public paymentCard: any; //observable proxy of Model.Entities.PaymentCard
        public cardTypeId: Computed<string>;
        public cardType: Observable<Commerce.Model.Entities.CardTypeInfo>;
        // Card number for UI usage. Will be masked for swiped case, clear otherwise.
        public cardNumber: Observable<string>;
        public maskedCardNumber: Computed<string>;
        public cashBackAmount: Observable<number>;
        public cashBackAmountText: Computed<string>;
        public isCreditCard: Observable<boolean>;
        public isDebitCard: Observable<boolean>;
        public expirationMonth: Computed<ValueCaptionPair>;
        public cardSource: Observable<Model.Entities.CardSource>; // The source of the card information (keyboard, MSR, PaymentTerminal, etc...)
        public cardInfo: Model.Entities.CardInfo;

        // Gift card specific properties
        public giftCardId: Observable<string>;
        public giftCardBalance: Observable<number>;
        public giftCardBalanceTimestamp: Observable<Date>;

        // Credit memo specific properties
        public creditMemoId: Observable<string>;
        public creditMemoAmount: Observable<number>;
        public creditMemoAmountTimestamp: Observable<Date>;

        // Loyalty card specific properties
        public loyaltyCardId: Observable<string>;

        // Customer account specific properties
        public customerId: Observable<string>;

        // Private members
        private _tenderType: Model.Entities.TenderType;

        /**
         * Instantiates the payment view model.
         *
         * @param {Model.Entities.TenderType} tenderType The tender type.
         * @param {string} fullAmount The full amount/balance due
         * @param {string} currency The currency.
         */
        constructor(tenderType?: Model.Entities.TenderType, fullAmount?: number, currency?: string) {
            super();
            // Initialize cash and currency data
            this.currency = ko.observable(currency ? currency : Commerce.ApplicationContext.Instance.deviceConfiguration.Currency);
            this.currencyDenominations = ko.observableArray([]);
            this.currencyAmounts = ko.observableArray([]);
            this._currencyDenominationMap = [];

            // Initialize common data
            this._tenderType = tenderType;
            this.tenderTypeId = ko.observable(tenderType ? tenderType.TenderTypeId : '');
            this.operationId = ko.observable(tenderType ? tenderType.OperationId : 0);
            this.cardType = ko.observable(null);
            this.tenderTypeName = ko.computed(this.computeTenderTypeName, this);
            this.fullAmount = ko.observable(fullAmount ? fullAmount : 0);
            this.paymentAmountText = ko.observable("");
            this.paymentAmountTextAsCurrency = ko.observable("");
            this.originalAmount = ko.observable(0);//TODO:AM
            this.originalAmountTextAsCurrency = ko.observable("");//TODO:AM
            this.setPaymentAmountText(this.fullAmount());
            this.cardSource = ko.observable(Model.Entities.CardSource.Unknown);

            this.isSignatureRequired = ko.computed(this.computeIsSignatureRequired, this);
            this.signatureData = ko.observable('');

            // Initialize gift card payment data
            this.giftCardId = ko.observable('');
            this.giftCardBalance = ko.observable(0);
            this.giftCardBalanceTimestamp = ko.observable(new Date());

            // Initialize credit memo payment data
            this.creditMemoId = ko.observable('');
            this.creditMemoAmount = ko.observable(0);
            this.creditMemoAmountTimestamp = ko.observable(new Date());

            // Loyalty card specific properties
            this.loyaltyCardId = ko.observable('');

            // Initialize card payment specific data to default value
            this.months = ko.observableArray<ValueCaptionPair>(PaymentViewModel.getMonths());
            this.years = ko.observableArray<ValueCaptionPair>(PaymentViewModel.getYears());
            this.cardNumber = ko.observable('');
            this.maskedCardNumber = ko.computed((): string => { return this.maskCardNumber(this.cardNumber()); }, this);
            this.cashBackAmount = ko.observable(0);
            this.cashBackAmountText = ko.computed((): string => {
                return NumberExtensions.formatNumber(this.cashBackAmount(), NumberExtensions.getDecimalPrecision());
            }, this);
            this.isCreditCard = ko.observable(false);
            this.isDebitCard = ko.observable(false);
            this.cardTypeId = ko.computed(this.computeCardTypeId, this);
            this.expirationMonth = ko.computed(this.computeExpirationMonth, this);
            this.initializePaymentCard();
            this.updateCardInfo(null);

            // Customer account specific properties
            this.customerId = ko.observable('');
        }

        public static getDefaultCountryRegionISOCode(): string {
            var storeInformation: Model.Entities.OrgUnit = ApplicationContext.Instance.storeInformation;
            var storeAddress: Model.Entities.Address = storeInformation.OrgUnitAddress;

            if (ObjectExtensions.isNullOrUndefined(storeAddress)) {
                storeAddress = new Model.Entities.AddressClass();
            }

            if (StringExtensions.isNullOrWhitespace(storeAddress.TwoLetterISORegionName)) {
                storeAddress.TwoLetterISORegionName = "US"; //defaulting country / region to United States
            }

            return storeAddress.TwoLetterISORegionName;
        }

        /**
         * Gets the title for the specified operation id.
         *
         * @param {string} operationId The id of the operation.
         * @return {string} The title for the operation.
         */
        public static getTitleForOperationId(operationId: number): string {
            // The title for currency should be set to the title used by cash if cash is supported
            if (operationId == Commerce.Operations.RetailOperation.PayCurrency) {
                operationId = Commerce.Operations.RetailOperation.PayCash;
            }

            var tenderType: Model.Entities.TenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(operationId);
            var tenderTypeName: string = tenderType ? tenderType.Name : "";

            return tenderTypeName;
        }

        /**
         * Gets whether the payment type is card payment.
         *
         * @return {boolean} True: is card payment; false: otherwise.
         */
        public get isCardPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCard;
        }

        /**
         * Gets whether the payment type is cash payment (including different currency).
         *
         * @return {boolean} True: is cash payment; false: otherwise.
         */
        public get isCashPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCash
                || this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCurrency;
        }

        /**
         * Gets whether the payment type is currency payment (including different currency).
         *
         * @return {boolean} True: is currency payment; false: otherwise.
         */
        public get isCurrencyPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCurrency;
        }

        /**
         * Gets whether the payment type is customer account payment.
         *
         * @return {boolean} True: is customer account payment; false: otherwise.
         */
        public get isCustomerAccountPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCustomerAccount;
        }

        /**
         * Gets whether the payment type is check payment.
         *
         * @return {boolean} True: is check payment; false: otherwise.
         */
        public get isCheckPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCheck;
        }

        /**
         * Gets whether the payment type is credit memo payment.
         *
         * @return {boolean} True: is credit memo payment; false: otherwise.
         */
        public get isCreditMemoPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayCreditMemo;
        }

        /**
         * Gets whether the payment type is gift card payment.
         *
         * @return {boolean} True: is gift card payment; false: otherwise.
         */
        public get isGiftCardPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayGiftCertificate;
        }

        /**
         * Gets whether the payment type is loyalty card payment.
         *
         * @return {boolean} True: is loyalty card payment; False: otherwise.
         */
        public get isLoyaltyCardPayment(): boolean {
            return this._tenderType.OperationId == Commerce.Operations.RetailOperation.PayLoyalty;
        }

        /**
         * Gets whether the card type has been set.
         *
         * @return {boolean} True: if card type has been set; False: otherwise.
         */
        public isCardTypeSet(): boolean {
            return !StringExtensions.isNullOrWhitespace(this.cardTypeId());
        }

        /**
         * Gets whether the amount indicates a return.
         *
         * @return {boolean} True: if the amount indicates a return. False: otherwise.
         */
        public isReturn(): boolean {
            return this.fullAmount() < 0;
        }

        /**
         * Gets boolean value which indicates whether card source is payment terminal.
         *
         * @return {boolean} True: if card source is payment terminal. False: otherwise.
         */
        public isCardSourcePaymentTerminal(): boolean {
            return this.cardSource() === Model.Entities.CardSource.PaymentTerminal;
        }

        /**
         * Sets the cashback amount.
         *
         * @param {number} amount The cashback amount.
         */
        public setCashbackAmount(amount: number): void {
            if (isNaN(amount)) {
                amount = 0;
            }

            this.cashBackAmount(amount);
        }

        /**
         * Gets the maximum cashback amount.
         *
         * @return {number} The maximum cashback amount allowed or 0 if no cashback is allowed.
         */
        public getMaximumCashbackAmount(): number {
            var maximumCashbackAmount: number = (this.cardInfo && this.cardInfo.CashBackAmount) ? this.cardInfo.CashBackAmount : 0;
            return maximumCashbackAmount;
        }

        /**
         * Loads the view model.
         *
         * @param {IVoidAsyncResult} The async result.
         */
        public load(): IVoidAsyncResult {
            this.roundAmountByTenderTypeAndSetFields(this.fullAmount());

            // Initialize tender specific data
            switch (this._tenderType.OperationId) {
                case Operations.RetailOperation.PayCard:
                    {
                        // Initialize expiration date choices list
                        this.months(PaymentViewModel.getMonths());
                        this.years(PaymentViewModel.getYears());
                        break;
                    }

                // Initialize denomination information if this is cash or currency
                case Operations.RetailOperation.PayCash:
                    {
                        return this.setCurrencyInfoForAmountForStoreCurrencyAsync(this.fullAmount()).done(() => {
                            this.setCurrency(this.currency(), true);
                        });
                        //break;
                    }
                case Operations.RetailOperation.PayCurrency:
                    {
                        var result: VoidAsyncResult = new VoidAsyncResult();
                        this.setCurrencyInfoForAmountInAllChannelCurrenciesAsync(this.currency(), this.fullAmount())
                            .done(() => {
                                this.setCurrency(this.currency(), true);
                                result.resolve();
                            }).fail((error: Model.Entities.Error[]) => {
                                this.setCurrencyInfoForAmountForStoreCurrencyAsync(this.fullAmount())
                                    .done(() => {
                                        this.setCurrency(this.currency(), true);
                                        result.resolve();
                                    })
                                    .fail((error: Model.Entities.Error[]) => {
                                        result.reject(error);
                                    });
                            });
                        return result;
                        break;
                    }
            }

            return VoidAsyncResult.createResolved();
        }

        /**
         * Sets the payment method to the one of the specified tender type id.
         *
         * @param {string} tenderTypeId The tender type id.
         */
        public setPaymentMethod(tenderType: Model.Entities.TenderType): void {
            if (ObjectExtensions.isNullOrUndefined(tenderType)) {
                throw 'tenderType is null or undefined.';
            }

            this._tenderType = tenderType;
            this.tenderTypeId(tenderType.TenderTypeId);
            this.operationId(tenderType.OperationId);

            // Initialize the payment specific settings.
            this.load();
        }

        /**
         * Sets the payment amount text.
         *
         * @param {number} paymentAmount The payment amount.
         */
        public setPaymentAmountText(paymentAmount: number): void {
            var decimalPrecision: number = NumberExtensions.getDecimalPrecision(this.currency());
            this.paymentAmountText(NumberExtensions.formatNumber(paymentAmount, decimalPrecision));
            this.paymentAmountTextAsCurrency(NumberExtensions.formatCurrency(paymentAmount, this.currency()));
        }

        //TODO://AM Stage Capital
        public setOriginalAmountText(paymentAmount: number): void {
            var decimalPrecision: number = NumberExtensions.getDecimalPrecision(this.currency());
            this.paymentAmountText(NumberExtensions.formatNumber(paymentAmount, decimalPrecision));
            this.originalAmountTextAsCurrency(NumberExtensions.formatCurrency(paymentAmount, this.currency()));
        }

        /**
         * Gets the payment amount.
         *
         * @return {number} The payment amount.
         */
        public getPaymentAmount(): number {
            return NumberExtensions.parseNumber(this.paymentAmountText());
        }

        /**
         * Sets the currency and denominations for the currency.
         * The method will fail if the currency and denominations have not been loaded
         *
         * @param {string } currency The currency.
         * @param {boolean} setPaymentAmountText Indicates whether to set the payment amount on the screen. Defaults to false.
         */
        public setCurrency(currency: string, setPaymentAmountText?: boolean): void {
            // Get the currency information for the target currency
            var currencyAmount: Commerce.Model.Entities.CurrencyAmount = this.getCurrencyAmount(currency);
            if (currencyAmount == null) {
                throw new Error("The currency '" + currency + "' is not listed for the channel");
            }

            this.currency(currencyAmount.CurrencyCode);
            this.currencyDenominations(this._currencyDenominationMap[currencyAmount.CurrencyCode]);

            if ((setPaymentAmountText != undefined) && setPaymentAmountText) {
                this.setPaymentAmountText(currencyAmount.RoundedConvertedAmount);
            }
        }

        /**
         * Validates whether the payment information is complete for processing.
         *
         * @param {boolean} [skipErrorDialog] If set to true, then error message dialog will not be shown.
         * @param {boolean} [isVoiceAuth] If set to true, the payment is a voice authorization payment.
         * @return {boolean} True: tender line information is complete; false: otherwise.
         */
        public isPaymentInformationComplete(skipErrorDialog?: boolean, isVoiceAuth?: boolean): boolean {
            var isValid = true;

            // Validate payment card
            if (this.isCardPayment) {
                isValid = isValid && this.isPaymentCardInformationComplete(skipErrorDialog, isVoiceAuth);
            }

            if (this.isCreditMemoPayment) {
                isValid = isValid && this.isCreditMemoInformationComplete();
            }

            if (this.isCustomerAccountPayment) {
                isValid = isValid && this.isCustomerAccountInformationComplete();
            }

            if (this.isLoyaltyCardPayment) {
                isValid = isValid && this.isLoyaltyCardInformationComplete();
            }

            if (this.isGiftCardPayment) {
                isValid = isValid && this.isGiftCardInformationComplete();
            }

            return isValid;
        }

        /**
         * Gets the tender line based on the payment inputs.
         *
         * @return {Commerce.Model.Entities.CartTenderLine} the cart tender line object.
         */
        public getTenderLine(): Commerce.Model.Entities.CartTenderLine {
            // If cash payment, submit as cash if the currency equals the store currency, otherwise submit as currency.
            var tenderTypeId: string = this.tenderTypeId();
            var currency: string = this.currency();
            var isCurrencyTenderType: boolean = false;
            if (this.isCurrencyPayment) {
                if (currency === Commerce.ApplicationContext.Instance.deviceConfiguration.Currency) {
                    var cashTenderType: Model.Entities.TenderType = Commerce.ApplicationContext.Instance.tenderTypesMap.getTenderTypeByOperationId(Operations.RetailOperation.PayCash);
                    if (!ObjectExtensions.isNullOrUndefined(cashTenderType)) {
                        tenderTypeId = cashTenderType.TenderTypeId;
                    }
                } else {
                    isCurrencyTenderType = true;
                }
            }

            // Create tender line.
            var tenderLine = <Commerce.Model.Entities.CartTenderLine>{
                TenderLineId: "",
                Currency: currency,
                TenderTypeId: tenderTypeId,
                SignatureData: this.signatureData(),
            };

            // Set the tender line fields.
            var paymentAmount: number = this.getPaymentAmount();
            if (isCurrencyTenderType) {
                var currencyAmount: Commerce.Model.Entities.CurrencyAmount = this.getSelectedCurrencyAmount();
                // If the selected currency amount is found.
                if (!ObjectExtensions.isNullOrUndefined(currencyAmount)) {
                    // If the rounded amount of the full payment in the currency is equal to the payment amount set the tender line using the non-rounded amount.
                    if (NumberExtensions.areEquivalent(currencyAmount.RoundedConvertedAmount, paymentAmount)) {
                        tenderLine.AmountInTenderedCurrency = currencyAmount.ConvertedAmount;
                    } else {
                        tenderLine.AmountInTenderedCurrency = paymentAmount;
                    }
                } else {
                    tenderLine.AmountInTenderedCurrency = paymentAmount;
                }
            } else {
                tenderLine.Amount = paymentAmount;
            }

            // Set payment specific properties.
            if (this.isCardPayment) {
                tenderLine.PaymentCard = ObjectExtensions.unwrapObservableProxyObject(this.paymentCard);
                tenderLine.CardTypeId = this.cardTypeId();
                if (this.cashBackAmount() > 0) {
                    tenderLine.CashBackAmount = this.cashBackAmount();
                }
            }

            if (this.isGiftCardPayment) {
                tenderLine.GiftCardId = this.giftCardId();
            }

            if (this.isCreditMemoPayment) {
                tenderLine.CreditMemoId = this.creditMemoId();
                // Clear tender amount for credit memo payment - must use full amount.
                if (paymentAmount >= 0) {
                    tenderLine.Amount = null;
                    tenderLine.AmountInTenderedCurrency = null;
                }
            }

            if (this.isCustomerAccountPayment) {
                tenderLine.CustomerId = this.customerId();
            }

            if (this.isLoyaltyCardPayment) {
                tenderLine.LoyaltyCardId = this.loyaltyCardId();
            }

            return tenderLine;
        }

        /**
         * Updates the card information for appropriate payment type. 
         *
         * @param {Model.Entities.CardInfo} [cardInfo] The card information object.
         * @param {Model.Entities.CardSource} [cardSource] The source of the card information.
         */
        public updateCardInfo(cardInfo?: Model.Entities.CardInfo, cardSource?: Model.Entities.CardSource): void {
            // Set the card info
            this.cardInfo = cardInfo;

            // Get the card number
            var cardNumber: string = cardInfo ? (cardInfo.CardNumber ? cardInfo.CardNumber : "") : "";

            // Check that an operation type has been defined
            if (ObjectExtensions.isNullOrUndefined(this._tenderType) || ObjectExtensions.isNullOrUndefined(this._tenderType.OperationId)) {
                this.loyaltyCardId("");
                this.giftCardId("");
                this.updatePaymentCard(null, Model.Entities.CardSource.Unknown);
                return;
            }

            // Set the card information for the operation type
            if (this.isGiftCardPayment) {
                this.giftCardId(cardNumber);

                this.loyaltyCardId("");
                this.updatePaymentCard(null, Model.Entities.CardSource.Unknown);
            } else if (this.isLoyaltyCardPayment) {
                this.loyaltyCardId(cardNumber);

                this.giftCardId("");
                this.updatePaymentCard(null, Model.Entities.CardSource.Unknown);
            } else if (this.isCardPayment) {
                this.updatePaymentCard(cardInfo, cardSource);

                this.giftCardId("");
                this.loyaltyCardId("");
            }
            else {
                RetailLogger.viewModelPaymentCardSwipeNotSupported(this._tenderType.OperationId);
            }
        }

        /**
         * Updates the payment card for the card swipe. 
         *
         * @param {Model.Entities.CardInfo} [cardInfo] The card information object.
         * @param {Model.Entities.CardSource} [cardSource] The source of the card information.
         * @return {Model.Entities.PaymentCard} The payment card.
         */
        private updatePaymentCard(cardInfo?: Model.Entities.CardInfo, cardSource?: Model.Entities.CardSource): Model.Entities.PaymentCard {
            var paymentCard = this.paymentCard;
            if (cardInfo) {
                var isManual: boolean = cardSource === Model.Entities.CardSource.Manual || cardSource === Model.Entities.CardSource.Unknown;
                paymentCard.CardNumber(cardInfo.CardNumber);
                paymentCard.ExpirationMonth(cardInfo.ExpirationMonth);
                paymentCard.ExpirationYear(((cardInfo.ExpirationYear < 100) && (cardInfo.ExpirationYear >= 0)) ? cardInfo.ExpirationYear + 2000 : cardInfo.ExpirationYear); // Input from swipe sources can come as either a 2 digit or 4 digit year
                paymentCard.Track1(cardInfo.Track1);
                paymentCard.Track2(cardInfo.Track2);
                paymentCard.Track3(cardInfo.Track3);
                paymentCard.EncryptedPin(cardInfo.EncryptedPin);
                paymentCard.AdditionalSecurityData(cardInfo.AdditionalSecurityData);
                paymentCard.IsSwipe(!isManual);
                this.cardNumber(cardInfo.CardNumber);
                this.setCashbackAmount(cardInfo.CashBackAmount);
            } else {
                paymentCard.CardNumber("");
                var date = new Date();
                paymentCard.ExpirationMonth(date.getMonth() + 1);
                paymentCard.ExpirationYear(date.getFullYear());
                paymentCard.Track1("");
                paymentCard.Track2("");
                paymentCard.Track3("");
                paymentCard.EncryptedPin("");
                paymentCard.AdditionalSecurityData("");
                paymentCard.IsSwipe(false);
                this.cardNumber("");
                this.setCashbackAmount(0);
            }

            this.cardSource(cardSource ? cardSource : Model.Entities.CardSource.Unknown);

            return ObjectExtensions.unwrapObservableProxyObject(paymentCard);
        }

        /**
         * Gets the payment card. 
         *
         * @return {Model.Entities.PaymentCard} The payment card.
         */
        public getPaymentCard(): Model.Entities.PaymentCard {
            return ObjectExtensions.unwrapObservableProxyObject(this.paymentCard);
        }

        /**
          * Updates the cash back amount.
         * @param {number} amount The cash back amount.
          */
        public updateCashBackAmount(amount: number): void {
            this.cashBackAmount(amount);
        }

        /**
         * Validates if the payment card information is complete.
         *
         * @param {boolean} [skipErrorDialog] If set to true, then error message dialog will not be shown.
         * @param {boolean} [isVoiceAuth] If set to true, the payment is a voice authorization payment.
         * @return {boolean} True: payment card information is complete; false: otherwise.
         */
        public isPaymentCardInformationComplete(skipErrorDialog?: boolean, isVoiceAuth?: boolean): boolean {
            var paymentCard = this.paymentCard;
            var result = true;

            if (this._tenderType.OperationId == Operations.RetailOperation.PayCard) {

                // If it's card swipe and card number is already updated then it's MSR through HW station.
                if (paymentCard.IsSwipe() && StringExtensions.isNullOrWhitespace(this.cardNumber())) {
                    if (StringExtensions.isNullOrWhitespace(paymentCard.Track2())) {
                        result = false;
                    }
                }
                else {
                    this.paymentCard.CardNumber(this.cardNumber());
                    if (StringExtensions.isNullOrWhitespace(paymentCard.CardNumber())) {
                        result = false;
                    }

                    // Check if voice approval code is set, if voice authorization code was chosen in drop-down.
                    if (isVoiceAuth) {
                        if (StringExtensions.isNullOrWhitespace(paymentCard.VoiceAuthorizationCode())) {
                            result = false;
                        }
                    }
                }
            }

            if (!result && !skipErrorDialog) {
                NotificationHandler.displayErrorMessage("string_1166"); // "Unable to read card data. Please try again or use a different card.
            }

            return result;
        }

        /**
         * Validates the payment amount.
         *
         * @return {boolean} True: payment amount is valid; false: otherwise. 
         */
        public isPaymentAmountValid(): boolean {
            var paymentAmount: number = this.getPaymentAmountInStoreCurrency();
            
            // Check that the payment amount was successfully calculated.
            if (ObjectExtensions.isNullOrUndefined(paymentAmount)) {
                return false;
            }

            // Check payment amount number valid
            if (isNaN(paymentAmount) || paymentAmount === 0) {
                NotificationHandler.displayErrorMessage("string_1138"); // The number that was entered for the payment amount isn't valid. Enter a valid payment amount.
                return false;
            }

            // Get the full amount in selected currency.
            var currencyAmount = this.getSelectedCurrencyAmount();
            var fullAmount = !ObjectExtensions.isNullOrUndefined(currencyAmount) ? currencyAmount.RoundedConvertedAmount : this.fullAmount();

            // Check that the amount is tendered for the correct sign
            if ((fullAmount > 0) && (paymentAmount < 0)) {
                NotificationHandler.displayErrorMessage("string_1179"); // The payment amount must be greater than 0 when the amount due is greater than 0.
                return false;
            } else if ((fullAmount < 0) && (paymentAmount > 0)) {
                NotificationHandler.displayErrorMessage("string_1178"); // The payment amount must be less than 0 when the amount due is less than 0.
                return false;
            }

            // Check the amount is valid for the currency.
            var currency: string = this.currency();
            var paymentAmountInSelectedCurrencyAmount: number = this.getPaymentAmount();
            if (!Commerce.Helpers.CurrencyHelper.isValidAmount(paymentAmountInSelectedCurrencyAmount, currency)) {
                NotificationHandler.displayErrorMessage(Commerce.ErrorTypeEnum.INVALID_CURRENCY_AMOUNT, currency);
                return false;
            }

            return true;
        }

        /**
         * Sets the payment card type.
         *
         * @param {Model.Entities.CardTypeInfo} cardType The card type.
         */
        public setPaymentCardType(cardType: Model.Entities.CardTypeInfo) {
            this.cardType(cardType);
            if (!ObjectExtensions.isNullOrUndefined(cardType)) {
                this.isCreditCard(cardType.CardTypeValue == Entities.CardType.InternationalCreditCard || cardType.CardTypeValue == Entities.CardType.CorporateCard);
                this.isDebitCard(cardType.CardTypeValue == Entities.CardType.InternationalDebitCard);
            } else {
                this.isCreditCard(false);
                this.isDebitCard(false);
            }
        }

        /**
         * Filter the card types based on card number, credit/debit card entry method and whether the type has been selected by the user.
         *
         * @param {boolean) isSwipe True of the card data was gotten through swipe, false if manually entered on POS.
         * @param {Model.Entities.CardInfo) cardInfo The card type information of the card if known (ex. selected by the user on a Hydra device).
         * @return {IAsyncResult<Commerce.Model.Entities.CardTypeInfo[]>} The result which returns a list of card types that are applicable for the card number, card type and card type id (VISA, DEBIT etc).
         */
        public filterCardTypesAsync(isSwipe: boolean, cardInfo: Model.Entities.CardInfo): IAsyncResult<Commerce.Model.Entities.CardTypeInfo[]> {
            var asyncResult: AsyncResult<Model.Entities.CardTypeInfo> = new AsyncResult<Model.Entities.CardTypeInfo>();
            var cardTypeIdString: string = cardInfo ? cardInfo.CardTypeId : null;
            var cardTypeId: number = parseInt(cardTypeIdString);
            var isCreditCard: boolean = cardTypeId === Entities.CardType.InternationalCreditCard || cardTypeId === Entities.CardType.CorporateCard;
            var isDebitCard: boolean = cardTypeId === Entities.CardType.InternationalDebitCard;
            var filteredCardTypes: Commerce.Model.Entities.CardTypeInfo[] = [];

            // If manual entry check that the type of the card isn't mapped to debit card
            if (!isSwipe) {
                if (isDebitCard) {
                    asyncResult.resolve(filteredCardTypes);
                    return asyncResult;
                }

                // Only credit cards are allowed for manual entry, so force the card type to be credit card
                isCreditCard = true;
            }

            Commerce.ApplicationContext.Instance.cardTypesAsync.done((cardTypes: Model.Entities.CardTypeInfo[]) => {
                for (var i = 0; i < cardTypes.length; i++) {
                    var cardType = cardTypes[i];

                    // If manual entry check that the card type allows manual entry
                    if (!isSwipe && !cardType.AllowManualInput) {
                        continue;
                    }

                    // Check that the card type matches the expected card type value
                    if ((isCreditCard && ((cardType.CardTypeValue !== Entities.CardType.InternationalCreditCard) && (cardType.CardTypeValue !== Entities.CardType.CorporateCard)))
                        || (isDebitCard && (cardType.CardTypeValue !== Entities.CardType.InternationalDebitCard))) {
                        continue;
                    }

                    if (this.isAssociatedCardType(cardType, this.paymentCard.CardNumber())) {
                        filteredCardTypes.push(cardType);
                    }
                }
                asyncResult.resolve(filteredCardTypes);
            })
            .fail((errors: Model.Entities.Error[]) => {
                asyncResult.resolve(errors);
            });

            return asyncResult;
        }

        /**
         * Filter the credit card type based on card prefix.
         *
         * @param {string) cardPrefix The card number prefix.
         * @return {IAsyncResult<Commerce.Model.Entities.CardTypeInfo[]>} The result which returns a list of credit card types that are applicable for the card prefix.
         */
        public filterCreditCardTypesAsync(cardPrefix: string): IAsyncResult<Commerce.Model.Entities.CardTypeInfo[]> {
            var asyncResult: AsyncResult<Commerce.Model.Entities.CardTypeInfo[]> = new AsyncResult<Commerce.Model.Entities.CardTypeInfo[]>();
            var filteredCardTypes: Commerce.Model.Entities.CardTypeInfo[] = [];

            Commerce.ApplicationContext.Instance.cardTypesAsync.done((cardTypes: Model.Entities.CardTypeInfo[]) => {
                for (var i = 0; i < cardTypes.length; i++) {
                    var cardType = cardTypes[i];

                    // Check that the card type is credit card or corporate card
                    if ((cardType.CardTypeValue !== Entities.CardType.InternationalCreditCard) && (cardType.CardTypeValue !== Entities.CardType.CorporateCard)) {
                        continue;
                    }

                    if (this.isAssociatedCardType(cardType, cardPrefix)) {
                        filteredCardTypes.push(cardType);
                    }
                }
                asyncResult.resolve(filteredCardTypes);
            })
            .fail((errors: Model.Entities.Error[]) => {
                asyncResult.resolve(errors);
            });

            return asyncResult;
        }

        /**
         * Gets the CurrencyAmount object for the selected currency
         *
         * @return {CurrencyAmount} The CurrencyAmount object for the selected currency. Null if a currency is not selected or the CurrencyAmount does not exist.
         */
        public getSelectedCurrencyAmount(): Commerce.Model.Entities.CurrencyAmount {
            return this.getCurrencyAmount(this.currency());
        }

        /*
         * Gets the currency amount object associated with the currency string passed in.
         * 
         * @param {string} currency The currency code string.
         * @return {Commerce.Model.Entities.CurrencyAmount} The CurrencyAmount object associated with the string parameter. Null if the currency is not found.
         */
        private getCurrencyAmount(currency: string): Commerce.Model.Entities.CurrencyAmount {
            var currencyAmounts: Commerce.Model.Entities.CurrencyAmount[] = this.currencyAmounts();
            for (var i: number = 0; i < currencyAmounts.length; i++) {
                if (currencyAmounts[i].CurrencyCode == currency) {
                    return currencyAmounts[i];
                }
            }

            return null;
        }

        /**
         * Resets the view model.
         */
        public reset(): void {
            this.resetPaymentAmount();
            this.resetPaymentCard();
            this.signatureData(StringExtensions.EMPTY);
        }

        /**
         * Resets the payment amount to full amount due.
         */
        public resetPaymentAmount(): void {
            if (this.currencyAmounts().length > 0) {
                this.setCurrency(this.currency(), true);
            } else {
                this.setPaymentAmountText(this.fullAmount());
            }
        }

        //TODO:AM Stage Capital
        public resetOriginalAmount(): void {
            this.setOriginalAmountText(this.originalAmount());
        }

        /**
         * Checks the gift card balance.
         * @return {IVoidAsyncResult} The async result.
         */
        public checkGiftCardBalance(): IVoidAsyncResult {

            // NOTE: this does not have to go through the operation pipeline since 
            // we are running the pay by gift card operation here already.

            if (StringExtensions.isNullOrWhitespace(this.giftCardId())) {
                var errors: Model.Entities.Error[] = [new Model.Entities.Error(ErrorTypeEnum.PAYMENT_GIFT_CARD_NUMBER_EMPTY)];
                return VoidAsyncResult.createRejected(errors);
            }

            return this.paymentManager.getGiftCardById(this.giftCardId())
                .done((giftCard: Model.Entities.GiftCard) => {
                    this.giftCardBalance(giftCard.Balance);
                    this.giftCardBalanceTimestamp(new Date());
                });
        }

        /**
         * Checks the credit memo amount.
         * @return {IVoidAsyncResult} The async result.
         */
        public checkCreditMemoAmount(): IVoidAsyncResult {
            return this.paymentManager.getCreditMemoById(this.creditMemoId())
                .done((creditMemo: Model.Entities.CreditMemo) => {
                    this.creditMemoAmount(creditMemo.Balance);
                    this.creditMemoAmountTimestamp(new Date());
                });
        }

        /**
         * Gets the months.
         *
         * @return {ValueCaptionPair[]} the months array.
         */
        private static getMonths(): ValueCaptionPair[] {
            var months = new Array();

            for (var monthIndex = 1; monthIndex <= 12; monthIndex++) {

                var monthCaption = Commerce.StringExtensions.format(
                    "({0}) {1}",
                    Commerce.StringExtensions.padLeft(monthIndex.toString(), "0", 2),
                    Commerce.ViewModelAdapter.getMonthName(monthIndex - 1));

                months[monthIndex - 1] = <ValueCaptionPair> { value: monthIndex, caption: monthCaption };
            }

            return months;
        }

        /**
         * Computed observable that returns the expirationMonth object
         *
         * @return {ValueCaptionPair} the expiration month as a ValueCaptionPair.
         */
        private computeExpirationMonth(): ValueCaptionPair {
            var expirationMonthAsValueCaptionPair: ValueCaptionPair = <ValueCaptionPair> { value: -1, caption: "" };

            var months: ValueCaptionPair[] = this.months();
            if (this.paymentCard && months) {
                var expirationMonth: number = this.paymentCard.ExpirationMonth();

                if (expirationMonth) {
                    expirationMonthAsValueCaptionPair = months[expirationMonth - 1];
                }
            }

            return expirationMonthAsValueCaptionPair;
        }

        /**
         * Gets the years starting this year.
         *
         * @return {ValueCaptionPair[]} the years array.
         */
        private static getYears(): ValueCaptionPair[] {
            var years = new Array();
            var date = new Date();
            for (var yearIndex = 0; yearIndex < 20; yearIndex++) {
                var calculateYear = date.getFullYear() + yearIndex;
                years[yearIndex] = <ValueCaptionPair> { value: calculateYear, caption: calculateYear.toString() };
            }

            return years;
        }

        /**
         * Get the empty payment card.
         * @return The empty payment card.
         */
        private static getEmptyPaymentCard(): Model.Entities.PaymentCard {
            var date = new Date();

            var paymentCard = <Commerce.Model.Entities.PaymentCard>{
                UseShippingAddress: false,
                Address1: StringExtensions.EMPTY,
                Country: PaymentViewModel.getDefaultCountryRegionISOCode(),
                Zip: StringExtensions.EMPTY,
                CardNumber: StringExtensions.EMPTY,
                CCID: StringExtensions.EMPTY,
                ExpirationMonth: date.getMonth() + 1,
                ExpirationYear: date.getFullYear(),
                Track1: StringExtensions.EMPTY,
                Track2: StringExtensions.EMPTY,
                Track3: StringExtensions.EMPTY,
                EncryptedPin: StringExtensions.EMPTY,
                AdditionalSecurityData: StringExtensions.EMPTY,
                IsSwipe: false,
                VoiceAuthorizationCode: StringExtensions.EMPTY
            };

            return paymentCard;
        }

        /**
         * Gets the currency, payment amount, and denominations for the currency and payment amount from RetailServer
         * and sets the currency amounts in the view model
         * If fromCurrency and toCurrency is the same, the method will still get the currency and denomination information
         *
         * @param {string} fromCurrency The currency of the payment amount.
         * @param {number} amount The amount to get.
         * @return {IVoidAsyncResult} The async result.
         */
        private setCurrencyInfoForAmountInAllChannelCurrenciesAsync(fromCurrency: string, amount: number): IVoidAsyncResult {
            var currencyAmounts: Model.Entities.CurrencyAmount[];

            return new AsyncQueue()
                .enqueue(() => {
                    return this.paymentManager.getCurrenciesAmount(fromCurrency, amount)
                        .done((value: Model.Entities.CurrencyAmount[]) => {
                        currencyAmounts = value;
                    });
                })
                .enqueue(() => {
                    // For each currency amount, filter the denominations and get the denominations from highest to lowest
                    this._currencyDenominationMap = [];
                    var self = this;

                    var results: IVoidAsyncResult[] = currencyAmounts
                        .map((currency: Model.Entities.CurrencyAmount) => {
                        return self.getFilteredDenominationsForCurrencyAsync(currency.CurrencyCode, currency.ConvertedAmount)
                            .done((denomination: Model.Entities.CashDeclaration[]) => {
                            self._currencyDenominationMap[currency.CurrencyCode] = denomination;
                        });
                    });

                    return VoidAsyncResult.join(results).done(() => {
                        this.currencyAmounts(currencyAmounts);
                    });
                })
                .run();
        }

        /**
         * Gets the currency, payment amount, and denominations for the company currency
         * and sets the currency amounts in the view model
         *
         * @param {number} amount The amount in the company currency.
         * @return {IVoidAsyncResult>} The async result.
         */
        private setCurrencyInfoForAmountForStoreCurrencyAsync(amount: number): IVoidAsyncResult {
            var currencyAmounts: Model.Entities.CurrencyAmount[] = [];
            var currencyAmount: Model.Entities.CurrencyAmount = {
                CurrencyCode: ApplicationContext.Instance.deviceConfiguration.Currency,
                ConvertedAmount: amount,
                RoundedConvertedAmount: amount
            };

            this._currencyDenominationMap = [];
            return this.getFilteredDenominationsForCurrencyAsync(currencyAmount.CurrencyCode, currencyAmount.ConvertedAmount)
                .done((result: Model.Entities.CashDeclaration[]) => {
                    this._currencyDenominationMap[currencyAmount.CurrencyCode] = result;
                    currencyAmounts.push(currencyAmount);
                    this.currencyAmounts(currencyAmounts);
                });
        }

        /**
         * Gets the denominations for the currency above the specified amount
         *
         * @param {string} currencyCode The currency code of the currency of the denominations to retrieve.
         * @param {number} amount The minimum denomination amount.
         * @return {IAsyncResult<Commerce.Model.Entities.CashDeclaration[]>} The result of denominations in descending order by amount. The array will be empty if there are no denominations for the currency code.
         */
        public getFilteredDenominationsForCurrencyAsync(currencyCode: string, amount: number): IAsyncResult<Commerce.Model.Entities.CashDeclaration[]> {
            // Get the denominations for the currency if the amount is positive
            var result: AsyncResult<Commerce.Model.Entities.CashDeclaration[]> = new AsyncResult<Commerce.Model.Entities.CashDeclaration[]>();

            ApplicationContext.Instance.cashDeclarationsMapAsync.done((map: Dictionary<Model.Entities.CashDeclaration[]>) => {
                var denominations: Commerce.Model.Entities.CashDeclaration[] = [];
                if (map.hasItem(currencyCode) && (amount >= 0)) {
                    denominations = map.getItem(currencyCode);
                }

                // Filter the denominations to only include denominations larger than or equal to the currency amount
                denominations = denominations.filter((value: Commerce.Model.Entities.CashDeclaration, index: number, array: Commerce.Model.Entities.CashDeclaration[]): boolean => { return value.Amount >= amount; });

                // Denominations should be in ascending order from the server, reverse the order to get the denominations
                // in descending order
                denominations.reverse();

                // Filter the denominations to remove duplicate values for when there is the same denomination for
                // different currency types
                denominations = denominations.filter((value: Commerce.Model.Entities.CashDeclaration, index: number, array: Commerce.Model.Entities.CashDeclaration[]): boolean => {
                    if (index != 0) {
                        return value.Amount != array[index - 1].Amount;
                    }

                    return true;
                });
                result.resolve(denominations);
            })
            .fail((errors: Model.Entities.Error[]) => {
                result.reject(errors);
            });

            return result;
        }

        /**
         * Initializes payment card.
         *
         * @param {any} [paymentCard] The payment card object.
         * @param {Model.Entities.CardSource} [cardSource] The source of the card information.
         */
        public initializePaymentCard(paymentCard?: any, cardSource?: Model.Entities.CardSource): void {
            // Initialize the payment card
            this.paymentCard = PaymentViewModel.getEmptyPaymentCard();

            // Convert the payment card to an observable proxy
            ObjectExtensions.convertToObservableProxyObject(this.paymentCard);

            // Set the payment card values
            if (!ObjectExtensions.isNullOrUndefined(paymentCard)) {
                this.updatePaymentCard(paymentCard, cardSource);
            }
        }

        /**
         * Computes if the signature is required.
         *
         * @return {boolean} True: signature is required; false: otherwise. 
         */
        private computeIsSignatureRequired(): boolean {
            var result = false;
            var paymentAmount: number = this.getPaymentAmountInStoreCurrency();

            // Check that the payment amount was successfully calculated.
            if (ObjectExtensions.isNullOrUndefined(paymentAmount)) {
                paymentAmount = this.getPaymentAmount();
            }

            if (!ObjectExtensions.isNullOrUndefined(this._tenderType) && !isNaN(paymentAmount)) {
                // Require signature only if configured to use sigCap, and amount no less than the minimum configured.
                if (this._tenderType.UseSignatureCaptureDevice
                    && Math.abs(paymentAmount) >= this._tenderType.MinimumSignatureCaptureAmount) {
                    result = true;
                }
            }
            return result;
        }

        /*
         * Gets the payment amount in the configured store currency.
         * Fails if the currency conversion fails.
         *
         * @return {number} Successful Operation: The payment amount in the store currency. Unsuccessful operation: null
         */
        private getPaymentAmountInStoreCurrency(): number {
            var paymentAmount: number = this.getPaymentAmount();
            // If the currency is different than the default, then convert it to the default currency.
            if (this.currency() != this.applicationContext.deviceConfiguration.Currency) {
                paymentAmount = this.computeConvertedCurrencyAmount(paymentAmount);
            }

            return paymentAmount;
        }

        /*
         * Computes the value of the currency amount in the default store currency.
         * Fails if the currency information cannot be found or the exchange rate is 0.
         *
         * @param {number} The amount in the foreign currency.
         * @return {number} Successful Conversion: The amount in the default store currency. Unsuccessful conversion: null
         */
        private computeConvertedCurrencyAmount(amount: number): number {
            var currencyAmount: Commerce.Model.Entities.CurrencyAmount = this.getSelectedCurrencyAmount();
            if (ObjectExtensions.isNullOrUndefined(currencyAmount)) {
                NotificationHandler.displayErrorMessage("string_1196", this.currency()); // Unable to find the selected currency.
                return null;
            }

            var exchangeRate: number = currencyAmount.ExchangeRate;
            if (NumberExtensions.areEquivalent(exchangeRate, 0)) { // Should never happen as this is not allowed in AX, but just in case...
                NotificationHandler.displayErrorMessage("string_1197"); // Exchange rate cannot be 0.
                return null;
            }

            var convertedAmount: number = amount / exchangeRate;
            return NumberExtensions.roundToNDigits(convertedAmount, NumberExtensions.getDecimalPrecision());
        }

        /**
          * Resets the payment card.
          */
        public resetPaymentCard(): void {
            this.paymentCard.UseShippingAddress(false);
            this.paymentCard.Address1(StringExtensions.EMPTY);
            this.paymentCard.Zip(StringExtensions.EMPTY);
            this.paymentCard.Country(PaymentViewModel.getDefaultCountryRegionISOCode());
            this.paymentCard.CardNumber(StringExtensions.EMPTY);
            this.paymentCard.CCID(StringExtensions.EMPTY);
            this.paymentCard.Track1(StringExtensions.EMPTY);
            this.paymentCard.Track2(StringExtensions.EMPTY);
            this.paymentCard.IsSwipe(false);
            this.paymentCard.VoiceAuthorizationCode(StringExtensions.EMPTY);

            var date = new Date();
            this.paymentCard.ExpirationMonth(date.getMonth() + 1);
            this.paymentCard.ExpirationYear(date.getFullYear());
            this.cardNumber('');
            this.updateCardInfo(null);
        }

        /**
         * Returns whether the credit memo information is complete.
         * @return True: information is complete; False: Otherwise.
         */
        private isCreditMemoInformationComplete(): boolean {
            if (StringExtensions.isNullOrWhitespace(this.creditMemoId())) {
                NotificationHandler.displayErrorMessage("string_1169");
                return false;
            }
            return true;
        }

        /**
         * Returns whether the customer account information is complete.
         * @return True: information is complete; False: Otherwise.
         */
        private isCustomerAccountInformationComplete(): boolean {
            if (StringExtensions.isNullOrWhitespace(this.customerId())) {
                NotificationHandler.displayErrorMessage("string_1172");
                return false;
            }
            return true;
        }

        /**
         * Returns whether the loyalty card information is complete.
         * @return True: information is complete; False: Otherwise.
         */
        private isLoyaltyCardInformationComplete(): boolean {
            if (StringExtensions.isNullOrWhitespace(this.loyaltyCardId())) {
                NotificationHandler.displayErrorMessage("string_1170");
                return false;
            }
            return true;
        }

        /**
         * Returns whether the gift card information is complete.
         * @return True: information is complete; False: Otherwise.
         */
        private isGiftCardInformationComplete(): boolean {
            if (StringExtensions.isNullOrWhitespace(this.giftCardId())) {
                NotificationHandler.displayErrorMessage("string_1171");
                return false;
            }
            return true;
        }

        /**
         * Computes the tender type name.
         * @return The tender type name.
         */
        private computeTenderTypeName(): string {
            var result = "";

            // Pay card needs special handling, as the title depends on the card types.
            if (this.operationId() == Operations.RetailOperation.PayCard
                && !ObjectExtensions.isNullOrUndefined(this.cardType())) {
                switch (this.cardType().CardTypeValue) {
                    case Entities.CardType.InternationalCreditCard:
                        result = "string_1100";
                        break;
                    case Entities.CardType.InternationalDebitCard:
                        result = "string_1155";
                        break;
                    case Entities.CardType.GiftCard:
                        result = "string_105";
                        break;
                    case Entities.CardType.LoyaltyCard:
                        result = "string_196";
                        break;
                }
            }

            if (StringExtensions.isNullOrWhitespace(result)) {
                result = PaymentViewModel.getTitleForOperationId(this.operationId());
            }

            return Commerce.ViewModelAdapter.getResourceString(result);
        }

        /**
         * Computes the card type id.
         * @return The card type id.
         */
        private computeCardTypeId(): string {
            var result = null;
            if (!ObjectExtensions.isNullOrUndefined(this.cardType())) {
                result = this.cardType().TypeId;
            }
            return result;
        }

        /**
         * Returns the masked card number for display.
         *
         * @param {string} cardNumber The card number.
         * @return {string} Masked card number.
         */
        private maskCardNumber(cardNumber: string): string {
            if (StringExtensions.isNullOrWhitespace(cardNumber)) {
                return cardNumber;
            }

            var result = cardNumber;
            var visibleChars = 4;

            if (cardNumber != null && cardNumber.length > visibleChars) {
                result = (new Array(cardNumber.length - visibleChars).join('X') + cardNumber.substring(cardNumber.length - visibleChars));
            }

            return result;
        }

        /**
         * Round amount by tender type.
         *
         * @param {number} [amount] The amount to be rounded.
         */
        private roundAmountByTenderTypeAndSetFields(amount: number): void {
            if (this._tenderType.OperationId != Operations.RetailOperation.PayCurrency) {
                this.paymentManager.roundAmountByTenderType(amount, this.tenderTypeId())
                    .done((roundedAmount: number) => {
                    this.fullAmount(roundedAmount);
                    this.setCurrencyInfoForAmountForStoreCurrencyAsync(roundedAmount).done(() => {
                        this.setPaymentAmountText(roundedAmount);
                    });
                });
            }
        }

        /**
         * Returns the boolean value indicating whether the card number is associated with the given card type.
         *
         * @param {Model.Entities.CardTypeInfo} cardType The card type.
         * @param {string} cardNumber The card number.
         * @return True: If the card number is associated with the given card type; False: Otherwise.
         */
        private isAssociatedCardType(cardType: Model.Entities.CardTypeInfo, cardNumber: string): boolean {

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
    }
}