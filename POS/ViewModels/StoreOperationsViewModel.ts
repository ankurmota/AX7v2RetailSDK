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

    export class DenominationCountingLine {
        RecordId: number;
        Currency: string;
        Amount: number;        // Amount in specified currency
        CashTypeValue: number;
        CountedAmount: number; // Number of denominations
        CountedValue: number;  // Total amount (amount * count) in store currency
        public denominationValueToDisplay: string;
        public countedValueToDisplay: string; // Total amount (amount * count) in entered currency as a formatted string

        constructor(cashDeclaration: Proxy.Entities.CashDeclaration) {
            this.RecordId = cashDeclaration.RecordId;
            this.Currency = cashDeclaration.Currency;
            this.Amount = cashDeclaration.Amount;
            this.CashTypeValue = cashDeclaration.CashTypeValue;
            this.CountedAmount = 0;
            this.CountedValue = 0;
            this.denominationValueToDisplay = NumberExtensions.formatNumber(this.Amount, NumberExtensions.getDecimalPrecision(this.Currency));
            this.countedValueToDisplay = NumberExtensions.formatNumber(this.CountedValue, NumberExtensions.getDecimalPrecision(this.Currency));
        }
    }

    export class TenderCountingLine {
        public tenderType: Proxy.Entities.TenderType;
        public tenderName: string;
        public isForeignCurrency: boolean;
        public currencyCode: string;
        public exchangeRate: number; // Exchange rate between the currency code and store currency
        public denominations: DenominationCountingLine[];
        public totalAmount: number; // Amount in store currency
        public totalAmountInCurrency: number; // Amount in entered currency
        public totalAmountInCurrencyToDisplay: string; // Amount in entered currency as a formatted string
        public numberOfTenderDeclarationRecount: number;   // Number of tender declaration recounts performed

        private _globalCashDenominationMap: Dictionary<Proxy.Entities.CashDeclaration[]>;

        constructor(currencyCode: string, tenderName: string, tenderType: Proxy.Entities.TenderType, cashDenominationMap: Dictionary<Proxy.Entities.CashDeclaration[]>) {

            this.currencyCode = currencyCode;
            this.exchangeRate = 0;
            this.tenderName = tenderName;
            this.tenderType = tenderType;
            this.totalAmount = 0;
            this.totalAmountInCurrency = 0;
            this.totalAmountInCurrencyToDisplay = NumberExtensions.formatNumber(this.totalAmountInCurrency, NumberExtensions.getDecimalPrecision(this.currencyCode));
            this.numberOfTenderDeclarationRecount = 0;
            this.denominations = [];

            this._globalCashDenominationMap = cashDenominationMap;
            this.addDenominations();
        }

        public get hasDenominations(): boolean {
            return ArrayExtensions.hasElements(this.denominations);
        }

        /**
         * Adds cash denominations to the current counting line from cash declaration stored in Application context
         */
        private addDenominations(): void {

            if (ObjectExtensions.isNullOrUndefined(this._globalCashDenominationMap) || StringExtensions.isNullOrWhitespace(this.currencyCode)
                || ObjectExtensions.isNullOrUndefined(this.tenderType)
                || (this.tenderType.OperationId != Operations.RetailOperation.PayCash && this.tenderType.OperationId != Operations.RetailOperation.PayCurrency)) {
                return;
            }

            if (this._globalCashDenominationMap.hasItem(this.currencyCode)) {
                var cashDeclarations: Proxy.Entities.CashDeclaration[] = this._globalCashDenominationMap.getItem(this.currencyCode);
                var denominations: DenominationCountingLine[] = cashDeclarations
                    .map((cashDec: Proxy.Entities.CashDeclaration) => new DenominationCountingLine(cashDec));
                this.denominations = denominations;
            }
        }
    }

    export class StoreOperationsViewModel extends ViewModelBase {

        private _primaryCurrencyCode: string;
        private _nonSalesTransaction: Proxy.Entities.NonSalesTransaction;
        private _dropAndDeclareTransaction: Proxy.Entities.DropAndDeclareTransaction
        public tenderCountingLines: ViewModels.TenderCountingLine[];
        public currencyAmountMap: Dictionary<Proxy.Entities.CurrencyAmount>;  // List of currency conversions from the store currency to all currencies supported for the store

        constructor() {
            super();
            this._primaryCurrencyCode = ApplicationContext.Instance.deviceConfiguration.Currency;
            this.tenderCountingLines = [];
            this.currencyAmountMap = new Dictionary<Proxy.Entities.CurrencyAmount>();
        }

        /**
         * Loads the view model properties.
         */
        public load(): IVoidAsyncResult {
            return this.getCurrenciesForCurrentStoreAsync()
                .done((countingLines) => { this.tenderCountingLines = countingLines; });
        }

        /**
         * Declare the starting amount for the shift.
         * @param {number} amount The amount to declare.
         * @param {string} comment Any comments the operator added.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public declareStartAmount(amount: number, comment: string, shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Receipt[]> {
            return this.validateAndPerformCashManagementOperation(
                amount, comment, shift, Proxy.Entities.TransactionType.StartingAmount, Proxy.Entities.ReceiptType.StartingAmount);
        }

        /**
         * Perform a float entry for the shift.
         * @param {number} amount The amount to float.
         * @param {string} comment Any comments the operator added.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public floatEntry(amount: number, comment: string, shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Receipt[]> {
            return this.validateAndPerformCashManagementOperation(
                amount, comment, shift, Proxy.Entities.TransactionType.FloatEntry, Proxy.Entities.ReceiptType.FloatEntry);
        }

        /**
         * Perform a tender removal for the shift.
         * @param {number} amount The amount to remove.
         * @param {string} comment Any comments the operator added.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public tenderRemove(amount: number, comment: string, shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Receipt[]> {
            return this.validateAndPerformCashManagementOperation(
                amount, comment, shift, Proxy.Entities.TransactionType.RemoveTender, Proxy.Entities.ReceiptType.RemoveTender);
        }

        /**
         * Perform a bank drop for the shift.
         * @param {TenderCountingLine[]} tenderLines The tenders taken to the bank.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @param {string} bankBagNumber The bank bag identifier.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public bankDrop(
            tenderLines: TenderCountingLine[],
            shift: Proxy.Entities.Shift,
            bankBagNumber: string): IAsyncResult<Proxy.Entities.Receipt[]> {
            return this.validateAndPerformTenderCountingOperation(
                tenderLines, shift, Proxy.Entities.TransactionType.BankDrop, bankBagNumber, Proxy.Entities.ReceiptType.BankDrop);
        }

        /**
         * Perform a safe drop for the shift.
         *
         * @param {TenderCountingLine[]} tenderLines The tenders taken to the safe.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public safeDrop(tenderLines: TenderCountingLine[], shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Receipt[]> {
            return this.validateAndPerformTenderCountingOperation(
                tenderLines, shift, Proxy.Entities.TransactionType.SafeDrop, null, Proxy.Entities.ReceiptType.SafeDrop);
        }

        /**
         * Perform a tender declaration for the shift.
         * @param {TenderCountingLine[]} tenderLines The tenders counted.
         * @param {Proxy.Entities.Shift} shift The shift to use.
         * @param {Proxy.Entities.ReasonCodeLine[]} reason code details to use.
         * @return {IAsyncResult<Proxy.Entities.Receipt[]>} The async result.
         */
        public tenderDeclaration(
            tenderLines: TenderCountingLine[],
            shift: Proxy.Entities.Shift,
            reasonCodeLines: Model.Entities.ReasonCodeLine[]): IAsyncResult<Proxy.Entities.Receipt[]> {

            return this.validateAndPerformTenderCountingOperation(
                tenderLines, shift, Proxy.Entities.TransactionType.TenderDeclaration, null, Proxy.Entities.ReceiptType.TenderDeclaration, reasonCodeLines);
        }

        private validateAndPerformCashManagementOperation(
            amount: number,
            comment: string,
            shift: Proxy.Entities.Shift,
            transactionType: Proxy.Entities.TransactionType,
            receiptTypeToPrint: Proxy.Entities.ReceiptType): IAsyncResult<Proxy.Entities.Receipt[]> {

            if (ObjectExtensions.isNullOrUndefined(comment)) {
                comment = StringExtensions.EMPTY;
            }

            if (ObjectExtensions.isNullOrUndefined(amount) || amount < 0 || amount == NaN) {
                return AsyncResult.createRejected([new Commerce.Proxy.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
            } else if (ObjectExtensions.isNullOrUndefined(shift) || shift.ShiftId < 1) {
                return AsyncResult.createRejected([new Commerce.Proxy.Entities.Error(Commerce.ErrorTypeEnum.SHIFT_IS_NOT_VALID)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this._nonSalesTransaction)) {
                this._nonSalesTransaction = { Id: "" };
            }

            this._nonSalesTransaction.Amount = amount;
            this._nonSalesTransaction.ForeignCurrency = this._primaryCurrencyCode;
            this._nonSalesTransaction.TransactionTypeValue = +transactionType;
            this._nonSalesTransaction.Description = comment;
            this._nonSalesTransaction.ShiftId = shift.ShiftId.toString();
            this._nonSalesTransaction.ShiftTerminalId = shift.TerminalId;

            var transactionId: string;
            var receipts: Proxy.Entities.Receipt[];
            var asyncResult = new AsyncResult<Proxy.Entities.Receipt[]>(null);
            var asyncQueue = new AsyncQueue();

            asyncQueue
                .enqueue(() => {
                    return this.storeOperationsManager.createNonSalesTransaction(this._nonSalesTransaction)
                        .done((data) => { transactionId = data.Id; });
                }).enqueue(() => {
                    return this.salesOrderManager.getReceiptsForPrintAsync(
                        transactionId,
                        false,
                        receiptTypeToPrint,
                        false,
                        shift.ShiftId,
                        shift.TerminalId,
                        null,
                        null,
                        ApplicationContext.Instance.hardwareProfile.ProfileId).done((data) => { receipts = data; });
                });

            asyncQueue.run()
                .done(() => { asyncResult.resolve(receipts); })
                .fail((errors) => { asyncResult.reject(errors); });

            return asyncResult;
        }

        private validateAndPerformTenderCountingOperation(
            tenderLines: TenderCountingLine[],
            shift: Proxy.Entities.Shift,
            transactionType: Proxy.Entities.TransactionType,
            bankBagNumber: string,
            receiptTypeToPrint: Proxy.Entities.ReceiptType,
            reasonCodeLines?: Proxy.Entities.ReasonCodeLine[]): IAsyncResult<Proxy.Entities.Receipt[]> {

            if (ObjectExtensions.isNullOrUndefined(shift) || shift.ShiftId < 1) {
                return AsyncResult.createRejected([new Commerce.Proxy.Entities.Error(ErrorTypeEnum.SHIFT_IS_NOT_VALID)]);
            }

            if (ObjectExtensions.isNullOrUndefined(this._dropAndDeclareTransaction)) {
                this._dropAndDeclareTransaction = { Id: "" };
            }
            var tenderDetailLines: Proxy.Entities.TenderDetail[] = [];

            var tenderAmountSum: number = 0;
            tenderLines.forEach((line: ViewModels.TenderCountingLine) => { tenderAmountSum += line.totalAmount; });

            var lineNumber: number = 1;
            tenderLines.forEach((tenderCountingLine) => {
                var tenderDetailLine: Proxy.Entities.TenderDetail = {};
                if (tenderCountingLine.totalAmount != 0 || tenderAmountSum <= 0) {
                    tenderDetailLine.Amount = tenderCountingLine.totalAmount;
                    tenderDetailLine.BankBagNumber = !ObjectExtensions.isNullOrUndefined(bankBagNumber) ? bankBagNumber : null;
                    tenderDetailLine.ForeignCurrency = tenderCountingLine.currencyCode;
                    tenderDetailLine.ForeignCurrencyExchangeRate = tenderCountingLine.exchangeRate;
                    tenderDetailLine.AmountInForeignCurrency = tenderCountingLine.totalAmountInCurrency;
                    tenderDetailLine.TenderTypeId = tenderCountingLine.tenderType.TenderTypeId;
                    tenderDetailLine.TenderRecount = tenderCountingLine.numberOfTenderDeclarationRecount;
                    tenderDetailLines.push(tenderDetailLine);
                }
            });

            // Assign captured reason code details for [Tender Declaraion] store operation
            this._dropAndDeclareTransaction.ReasonCodeLines = reasonCodeLines;
            this._dropAndDeclareTransaction.TenderDetails = tenderDetailLines;
            this._dropAndDeclareTransaction.TransactionTypeValue = +transactionType;
            this._dropAndDeclareTransaction.OpenDrawer = false;
            this._dropAndDeclareTransaction.ShiftId = shift.ShiftId.toString();
            this._dropAndDeclareTransaction.ShiftTerminalId = shift.TerminalId;

            var receipts: Proxy.Entities.Receipt[];
            var createdTransaction: Proxy.Entities.DropAndDeclareTransaction;
            var asyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                return this.storeOperationsManager.createDropAndDeclareTransaction(this._dropAndDeclareTransaction)
                    .done((data: Proxy.Entities.DropAndDeclareTransaction) => {
                        createdTransaction = data;
                    });
            }).enqueue(() => {
                return this.salesOrderManager.getReceiptsForPrintAsync(
                    createdTransaction.Id,
                    false,
                    receiptTypeToPrint,
                    false,
                    shift.ShiftId,
                    shift.TerminalId,
                    null,
                    null,
                    ApplicationContext.Instance.hardwareProfile.ProfileId).done((data) => { receipts = data; });
            });

            if (this._dropAndDeclareTransaction.TransactionTypeValue === Proxy.Entities.TransactionType.TenderDeclaration) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var postTriggerOptions: Triggers.IPostTenderDeclarationTriggerOptions = { shift: shift, transaction: createdTransaction };
                    return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostTenderDeclaration, postTriggerOptions);
                });
            }

            var asyncResult: AsyncResult<Proxy.Entities.Receipt[]> = new AsyncResult<Proxy.Entities.Receipt[]>(null);

            asyncQueue.run()
                .done(() => {
                    asyncResult.resolve(receipts);
                }).fail((errors) => {
                if (this._dropAndDeclareTransaction.TransactionTypeValue === Proxy.Entities.TransactionType.TenderDeclaration) {
                        this.processTenderDeclarationError(errors);
                    }
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }

        private processTenderDeclarationError(errors: Proxy.Entities.Error[]): void {
            var errorCode = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MAXCOUNTINGDIFFERENCEEXCEEDED";
            var countingErrors: Proxy.Entities.Error[] = errors.filter(e => StringExtensions.compare(e.ErrorCode, errorCode, true) == 0);

            if (Commerce.ArrayExtensions.hasElements(countingErrors)) {
                var error: Proxy.Entities.Error = countingErrors[0];

                // Gets the tender type id that needs to be recounted in tender declaration
                var id: string = ArrayExtensions.firstOrUndefined(error.formatData);
                var tenderTypeId: string = !ObjectExtensions.isNullOrUndefined(id) ? id : StringExtensions.EMPTY;

                this.tenderCountingLines.forEach((tenderCountingLine: Commerce.ViewModels.TenderCountingLine) => {
                    if (tenderCountingLine.tenderType.TenderTypeId === tenderTypeId) {
                        tenderCountingLine.numberOfTenderDeclarationRecount++;
                    }
                });
            }
        }

        /**
         * Gets the sum of all Declare Starting Amount operations performed for the current shift.
         */
        public getDeclaredStartingAmount(shift: Proxy.Entities.Shift): IAsyncResult<number> {
            return this.storeOperationsManager.getNonSalesTenderOperationTransactions(Proxy.Entities.TransactionType.StartingAmount, shift.ShiftId.toString(), shift.TerminalId)
                .map((declarations: Proxy.Entities.NonSalesTransaction[]): number => {
                    var sum: number = 0;
                    declarations.forEach(d => sum += d.Amount);

                    return sum;
                })
        }

        /**
         * Get all tender lines for counting in the form of TenderCountingLine objects
         * @return {IAsyncResult<TenderCountingLine[]>]} The result of tender lines to be counted
         */
        private getTenderLinesForCountingAsync(currencies?: Proxy.Entities.CurrencyAmount[]): IAsyncResult<TenderCountingLine[]> {

            var result: AsyncResult<TenderCountingLine[]> = new AsyncResult<TenderCountingLine[]>();

            ApplicationContext.Instance.cashDeclarationsMapAsync.done((map: Dictionary<Proxy.Entities.CashDeclaration[]>) => {
                var tenderList: TenderCountingLine[] = [];

                var tenderTypesMap: TenderTypeMap = ApplicationContext.Instance.tenderTypesMap;

                tenderTypesMap.forEach((key: string, tenderTypes: Commerce.Proxy.Entities.TenderType[]) => {
                    tenderTypes.forEach((tenderType) => {
                        if (tenderType.OperationId != 0 && tenderType.Function != TenderFunctionEnum.TenderRemoveFloat) {
                            if (tenderType.OperationId == Operations.RetailOperation.PayCurrency && !ObjectExtensions.isNullOrUndefined(currencies)) { // We need to iterate through the currencies
                                currencies.forEach((currency: Proxy.Entities.CurrencyAmount) => {
                                    if (currency.CurrencyCode != this._primaryCurrencyCode) {
                                        var tenderLineName = StringExtensions.format(
                                            Commerce.ViewModelAdapter.getResourceString("string_4037"), currency.CurrencyCode); // Cash - {0}
                                        var newCurrencyTenderLine: TenderCountingLine = new TenderCountingLine(currency.CurrencyCode, tenderLineName, tenderType, map);
                                        tenderList.push(newCurrencyTenderLine);
                                    }
                                });
                            }
                            else {
                                var newTenderLine: TenderCountingLine = new TenderCountingLine(this._primaryCurrencyCode, tenderType.Name, tenderType, map);
                                tenderList.push(newTenderLine);
                            }
                        }
                    });
                });

                result.resolve(tenderList);
            })
                .fail((errors: Proxy.Entities.Error[]) => {
                    result.reject(errors);
                });
            return result;
        }

        /**
         * Gets the time registration status of the current employee asynchronously.
         *
         * @return {IAsyncResult<Proxy.Entities.EmployeeActivity>} The async result.
         */
        public getCurrentTimeRegistrationStatusAsync(): IAsyncResult<Proxy.Entities.EmployeeActivity> {
            return this.storeOperationsManager.getCurrentTimeRegistrationStatus();
        }

        /**
         * Creates a time registration for the current employee.
         *
         * @param {registrationType} The type of registration to perform.
         * @return {IAsyncResult<Date>} The async result.
         */
        public performTimeRegistrationAsync(registrationType: Proxy.Entities.EmployeeActivityType): IAsyncResult<Date> {
            return this.storeOperationsManager.performTimeRegistration(
                Session.instance.CurrentEmployee.StaffId, registrationType);
        }

        /**
         * Gets all time registrations matching a given criteria.
         *
         * @param {searchCriteria} The search criteria to filter on.
         * @return {IAsyncResult<Proxy.Entities.EmployeeActivity[]>} The async result.
         */
        public getTimeRegistrationsAsync(searchCriteria: Proxy.Entities.EmployeeActivitySearchCriteria): IAsyncResult<Proxy.Entities.EmployeeActivity[]> {
            return this.storeOperationsManager.getTimeRegistrations(searchCriteria);
        }

        /**
         * Gets all time registrations matching a given criteria for a store manager.
         *
         * @param {searchCriteria} The search criteria to filter on.
         * @return {IAsyncResult<Proxy.Entities.EmployeeActivity[]>} The async result.
         */
        public getRegistrationsForManagerAsync(searchCriteria: Proxy.Entities.EmployeeActivitySearchCriteria): IAsyncResult<Proxy.Entities.EmployeeActivity[]> {
            return this.storeOperationsManager.getTimeRegistrationsForManager(searchCriteria);
        }

        /**
         * Gets all stores an employee has access to.
         *
         * @return {IAsyncResult<Proxy.Entities.OrgUnit[]>} The async result.
         */
        public getEmployeeStoresAsync(): IAsyncResult<Proxy.Entities.OrgUnit[]> {
            return this.storeOperationsManager.getEmployeeStores();
        }

        /**
         * Gets all income or expense accounts for the store.
         *
         * @param {Proxy.Entities.IncomeExpenseAccountType} accountType The account type (income or expense) to retrieve.
         * @return {IAsyncResult<Proxy.Entities.IncomeExpenseAccount[]>} The async result.
         */
        public getIncomeExpenseAccountsAsync(accountType: Proxy.Entities.IncomeExpenseAccountType): IAsyncResult<Proxy.Entities.IncomeExpenseAccount[]> {
            return this.storeOperationsManager.getIncomeExpenseAccounts(accountType);
        }

        /**
         * Gets an X report for the given shift.
         *
         * @param {number} shiftId The identifier of the shift to use.
         * @param {number} shiftId The terminal identifier of the shift to use.
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        public getXReportAsync(shiftId: number, terminalId: string): IAsyncResult<Proxy.Entities.Receipt> {
            return this.storeOperationsManager.getXReport(shiftId, terminalId, ApplicationContext.Instance.hardwareProfile.ProfileId);
        }

        /**
         * Gets the Z report for the last closed shift.
         *
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        public getZReportAsync(): IAsyncResult<Proxy.Entities.Receipt> {
            return this.storeOperationsManager.getZReport(ApplicationContext.Instance.hardwareProfile.ProfileId);
        }

        /**
         * Gets the currency, payment amount, and denominations for the currency and payment amount from RetailServer
         * and sets the currency amounts in the view model.
         * If fromCurrency and toCurrency is the same, the method will still get the currency and denomination information.
         *
         * @param {string} fromCurrency The currency of the payment payment amount.
         * @return {IAsyncResult<ViewModels.TenderCountingLine[]>} The asyncresult.
         */
        public getCurrenciesForCurrentStoreAsync(): IAsyncResult<ViewModels.TenderCountingLine[]> {
            var primaryCurrencyMainDenominationValue: number = 1;
            var currencyAmounts: Proxy.Entities.CurrencyAmount[];
            var result: AsyncResult<ViewModels.TenderCountingLine[]> = new AsyncResult<ViewModels.TenderCountingLine[]>();

            new AsyncQueue()
                .enqueue(() => {
                    return this.paymentManager.getCurrenciesAmount(this._primaryCurrencyCode, primaryCurrencyMainDenominationValue)
                        .done((value: Proxy.Entities.CurrencyAmount[]) => {
                            // Set the has table of currency codes to currency amounts for the store to have the list of conversion rates
                            currencyAmounts = value;
                            this.currencyAmountMap = new Dictionary<Proxy.Entities.CurrencyAmount>();
                            currencyAmounts.forEach((currencyAmount: Proxy.Entities.CurrencyAmount) => {
                                this.currencyAmountMap.setItem(currencyAmount.CurrencyCode, currencyAmount);
                            });
                        });
                })
                .enqueue(() => {
                    return this.getTenderLinesForCountingAsync(currencyAmounts).done((lines: TenderCountingLine[]) => {
                        result.resolve(lines);
                    });
                })
                .run()
                .fail((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewModelStoreOperationsGetCurrenciesForStoreFailed();
                    result.reject(errors);
                });

            return result;
        }

        /**
         * Converts the amount from one currency to the store currency as defined in the currencyAccountMap.
         *
         * @param {number} amount The amount to convert.
         * @param {string} fromCurrencyCode The currency code of the amount. If not provided, the store currency code is used.
         * @param {number} fromCurrencyExchangeRate The exchange rate from the specified currency code to the store currency.
         * @return {number} The converted amount.
         */
        public convertToStoreCurrency(amount: number, fromCurrencyCode: string, fromCurrencyExchangeRate: number): number {
            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(amount)) {
                return amount;
            }

            if (StringExtensions.isNullOrWhitespace(fromCurrencyCode)) {
                fromCurrencyCode = this.applicationContext.deviceConfiguration.Currency;
            }

            if (!this.currencyAmountMap.hasItem(fromCurrencyCode)) {
                return amount;
            }

            // Convert the currency amount to the store currency amount
            var storeAmount: number = amount * fromCurrencyExchangeRate;
            return NumberExtensions.roundToNDigits(storeAmount, NumberExtensions.getDecimalPrecision());
        }

        /**
         * Gets exchange rate from the specified currency to the store currency.
         *
         * @param {string} currencyCode The currency code of the amount. If not provided, the store currency code is used.
         * @return {number} The exchange rate from the specified currency to the store currency.
         */
        public getCurrencyExchangeRate(currencyCode: string): number {
            // Get the currency code
            if (StringExtensions.isNullOrWhitespace(currencyCode)) {
                currencyCode = this.applicationContext.deviceConfiguration.Currency;
            }

            // Get the currency information
            var exchangeRate: number = 0;

            // Set exchange rate to 1 if store currency is same as the amount's currency
            if (currencyCode == this._primaryCurrencyCode) {
                exchangeRate = 1;
            }
            else if (this.currencyAmountMap.hasItem(currencyCode)) {
                var currency: Proxy.Entities.CurrencyAmount = this.currencyAmountMap.getItem(currencyCode);

                // Compute the exchange rate between store currency and specified currency
                exchangeRate = currency.ExchangeRate == 0 ? 0 : (1 / currency.ExchangeRate);
            }

            return exchangeRate;
        }
    }
}