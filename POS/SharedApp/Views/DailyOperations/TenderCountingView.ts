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

module Commerce.ViewControllers {
    "use strict";

    // enum for transaction detail view mode
    export class TenderCountingViewMode {
        static PaymentMethods: string = "payments";
        static Denominations: string = "denominations";
    }

    /*
     * TenderCountingViewController constructor parameters interface
     */
    export interface TenderCountingViewControllerOptions {
        tenderDropAndDeclareType: Proxy.Entities.TransactionType;
        shift?: Proxy.Entities.Shift;

        /**
         * Reason code lines to be added to the tender counting operation.
         * @remarks Used for tender declaration.
         */
        reasonCodeLines?: Proxy.Entities.ReasonCodeLine[];
    }

    export class TenderCountingViewController extends ViewControllerBase {
        public commonHeaderData;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;
        public tenderCountingType: Proxy.Entities.TransactionType;
        public indeterminateWaitVisible: Observable<boolean>;
        public tenderAmountInput: Observable<string>;
        public tenderCountingLines: ObservableArray<ViewModels.TenderCountingLine>;
        public denominationLines: ObservableArray<ViewModels.DenominationCountingLine>;
        public viewMode: Observable<TenderCountingViewMode>;
        public totalAmountCounted: Computed<number>;
        public totalDenominationCounted: Computed<number>;
        public focusAmountInput: Observable<boolean>;
        public appBarVisible: Observable<boolean>;
        public useDenominationTotal: Observable<boolean>;

        private _selectedTenderLine: ViewModels.TenderCountingLine;
        private _selectedDenominationLine: ViewModels.DenominationCountingLine;
        private _shiftToUse: Model.Entities.Shift;
        private _navigationDestination: string;
        private _bankBagNumber: string;
        private _ignoreTenderLineClick: boolean;
        private _nextTenderLineToSelect: number;
        private _nextDenominationLineToSelect: number;
        private _selectedLineIndexForDenomination: number;
        private _tenderLineListControl: any;
        private _denominationLineListControl: any;
        private _printReceiptDialog: Controls.PrintReceiptDialog;
        private _bankBagInputControl: Controls.TextInputDialog;
        private _processingOperation: boolean;
        private _reasonCodeLines: Model.Entities.ReasonCodeLine[];

        constructor(options: TenderCountingViewControllerOptions) {
            super(false /* saveInHistory */);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.tenderCountingType = options.tenderDropAndDeclareType;
            this.indeterminateWaitVisible = ko.observable(false);
            this.tenderAmountInput = ko.observable("");
            this.viewMode = ko.observable(TenderCountingViewMode.PaymentMethods);
            this._bankBagNumber = "";
            this._ignoreTenderLineClick = false;
            this._processingOperation = false;
            this._nextTenderLineToSelect = -1;
            this._nextDenominationLineToSelect = -1;
            this.focusAmountInput = ko.observable(false);
            this.appBarVisible = ko.observable(false);
            this.useDenominationTotal = ko.observable(false);

            this.addControl(this._printReceiptDialog = new Controls.PrintReceiptDialog());
            this.addControl(this._bankBagInputControl = new Controls.TextInputDialog());

            this._bankBagInputControl.title(Commerce.ViewModelAdapter.getResourceString("string_4129")); // Bank bag

            this.tenderCountingLines = ko.observableArray<ViewModels.TenderCountingLine>([]);
            this.denominationLines = ko.observableArray<ViewModels.DenominationCountingLine>([]);

            var self = this;
            this.totalAmountCounted = ko.computed(() => {
                var total = 0;
                this.tenderCountingLines().forEach((tenderLine) => {
                    total += tenderLine.totalAmount;
                });
                return total;
            }, self);

            this.totalDenominationCounted = ko.computed(() => {
                var total = 0;
                this.denominationLines().forEach((denomination) => {
                    total += denomination.CountedValue;
                });
                return total;
            }, self);

            if (!ObjectExtensions.isNullOrUndefined(options.shift)) {
                this._shiftToUse = options.shift;
                this._navigationDestination = "BlindCloseView";
            }
            else {
                this._shiftToUse = Session.instance.Shift;
                this._navigationDestination = "HomeView";
            }

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4022"));

            if (this.tenderCountingType == Proxy.Entities.TransactionType.TenderDeclaration) {
                this._reasonCodeLines = options.reasonCodeLines;
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4017"));
            }
            else if (this.tenderCountingType == Proxy.Entities.TransactionType.BankDrop) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4018"));
            }
            else if (this.tenderCountingType == Proxy.Entities.TransactionType.SafeDrop) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4019"));
            }
        }

        private populateAmountInput(amount: string): void {
            this.tenderAmountInput(amount);

            // This is a short-term work around to ensure focus and being able to edit happen.
            // Need to wait for methods underneath to finish, ideally call pattern would be direct.
            setTimeout(() => {
                this.amountInput.select();
                this.amountInput.focus();
            }, 100);
        }

        private get amountInput(): JQuery {
            return $(this.getViewContainer()).find('#amountInput');
        }

        /*
         * Loads the control at page load
         */
        public load(): void {
            this.indeterminateWaitVisible(true);
            this.storeOperationsViewModel.load()
                .done(() => {
                    this.storeOperationsViewModel.tenderCountingLines.forEach((tenderCountingLine: Commerce.ViewModels.TenderCountingLine) => {
                        if (this.tenderCountingType == Proxy.Entities.TransactionType.TenderDeclaration && tenderCountingLine.tenderType.CountingRequired == 1 ||
                            this.tenderCountingType == Proxy.Entities.TransactionType.BankDrop && tenderCountingLine.tenderType.TakenToBank == 1 ||
                            this.tenderCountingType == Proxy.Entities.TransactionType.SafeDrop && tenderCountingLine.tenderType.TakenToSafe == 1) {
                            this.tenderCountingLines.push(tenderCountingLine);
                        }
                    });
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        public onShown() {
            this._tenderLineListControl = $("#paymentMethodList")[0].winControl;
            this._denominationLineListControl = $("#denominationList")[0].winControl;
        }

        /*
        * Called by navigation controller when the navigate back event occurs.
        */
        public onNavigateBack(): boolean {
            if (this.viewMode() == TenderCountingViewMode.Denominations) {
                this.viewMode(TenderCountingViewMode.PaymentMethods);
                this._processingOperation = false;
                return false;
            }
            else {
                return true;
            }
        }

        public clickCountButton(event: any) {
            if (!this._ignoreTenderLineClick) {
                // Find out which line was clicked
                var parent = event.target.parentElement;
                while (!parent.classList.contains('win-item')) {
                    parent = parent.parentElement;
                }
                this._selectedLineIndexForDenomination = parseInt(parent.attributes['aria-posinset'].value) - 1;
                this.denominationLines(this.tenderCountingLines()[this._selectedLineIndexForDenomination].denominations);
                this.viewMode(TenderCountingViewMode.Denominations);
                this.commonHeaderData.categoryName(this.tenderCountingLines()[this._selectedLineIndexForDenomination].tenderName);
            }
            else {
                this._ignoreTenderLineClick = false;
                this.focusAmountInput(true);
            }
        }

        /**
         * Changes the selected column if a user click on a denomination line
         *
         * @param {any} event The event information
         */
        public clickDenominationLine(event: any) {
            var element = event.target;

            while (!element.classList.contains('denominationCount') && !element.classList.contains('denominationTotal')) {
                element = element.parentElement;

                if (element == null) {
                    return;
                }
            }

            this.useDenominationTotal(!element.classList.contains('denominationCount'));
        }

        public changeAmountHandler(numPadResult: Controls.NumPad.INumPadResult) {
            var numberEntered: number = NumberExtensions.parseNumber(this.tenderAmountInput());
            this.tenderAmountInput("");

            if (isNaN(numberEntered)) {
                Commerce.NotificationHandler.displayClientErrors([new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
                return;
            }

            if (!ObjectExtensions.isNullOrUndefined(this._selectedTenderLine) && this.viewMode() == TenderCountingViewMode.PaymentMethods) {
                this._nextTenderLineToSelect = this._tenderLineListControl.selection.getIndices()[0] + 1;

                // If tender lines, the number entered is the value in the currency of the tender line
                this._selectedTenderLine.exchangeRate = this.storeOperationsViewModel.getCurrencyExchangeRate(this._selectedTenderLine.currencyCode);
                this._selectedTenderLine.totalAmount = this.storeOperationsViewModel.convertToStoreCurrency(numberEntered, this._selectedTenderLine.currencyCode, this._selectedTenderLine.exchangeRate);

                // Compute the amount entered in the store currency
                this._selectedTenderLine.totalAmountInCurrency = numberEntered;
                this._selectedTenderLine.totalAmountInCurrencyToDisplay = NumberExtensions.formatCurrency(this._selectedTenderLine.totalAmountInCurrency, this._selectedTenderLine.currencyCode);

                this.tenderCountingLines(this.tenderCountingLines());
            }
            else if (!ObjectExtensions.isNullOrUndefined(this._selectedDenominationLine) && this.viewMode() == TenderCountingViewMode.Denominations) {
                this._nextDenominationLineToSelect = this._denominationLineListControl.selection.getIndices()[0] + 1;

                if (this.useDenominationTotal()) { // The number entered is the total amount for the denomination line
                    var denominationCount: number = numberEntered / this._selectedDenominationLine.Amount,
                        roundedCount: number = Math.round(numberEntered / this._selectedDenominationLine.Amount);

                    if (!NumberExtensions.areEquivalent(denominationCount, roundedCount)) {
                        Commerce.NotificationHandler.displayClientErrors([new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
                        return;
                    }
                    this._selectedDenominationLine.CountedAmount = roundedCount;
                } else { // The number entered is the number of denominations counted for the denomination line
                    if (numberEntered != Math.floor(numberEntered)) {
                        Commerce.NotificationHandler.displayClientErrors([new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
                        return;
                    }
                    this._selectedDenominationLine.CountedAmount = numberEntered;
                }

                // Compute the total values for that denomination
                var amountToRound: number = this._selectedDenominationLine.CountedAmount * this._selectedDenominationLine.Amount;
                var currencyCode: string = this._selectedDenominationLine.Currency;

                this._selectedDenominationLine.CountedValue = NumberExtensions.roundToNDigits(amountToRound, NumberExtensions.getDecimalPrecision(currencyCode));
                this._selectedDenominationLine.countedValueToDisplay = NumberExtensions.formatCurrency(this._selectedDenominationLine.CountedValue, this._selectedDenominationLine.Currency);
                this.denominationLines(this.denominationLines());
            }
        }

        public tenderSelectionChangedHandler(tenderCountingLine: ViewModels.TenderCountingLine[]) {
            this._selectedTenderLine = tenderCountingLine[0];
            this._ignoreTenderLineClick = true;

            if (this._nextTenderLineToSelect >= 0 && ObjectExtensions.isNullOrUndefined(this._selectedTenderLine)) {
                if (this._nextTenderLineToSelect >= this.tenderCountingLines().length) {
                    this._tenderLineListControl.selection.set(this._nextTenderLineToSelect - 1);
                    this._selectedTenderLine = this.tenderCountingLines()[this._nextTenderLineToSelect - 1];
                    this.appBarVisible(true);
                }
                else {
                    this._tenderLineListControl.selection.set(this._nextTenderLineToSelect);
                    this._tenderLineListControl.ensureVisible(this._nextTenderLineToSelect);
                    this._selectedTenderLine = this.tenderCountingLines()[this._nextTenderLineToSelect];
                }
                this._nextTenderLineToSelect = -1;
            }

            if (!ObjectExtensions.isNullOrUndefined(this._selectedTenderLine)) {
                this.populateAmountInput(StringExtensions.format("{0}", this._selectedTenderLine.totalAmountInCurrency));
            }

        }

        public denominationSelectionChangedHandler(denomination: ViewModels.DenominationCountingLine[]) {
            this._selectedDenominationLine = denomination[0];

            if (this._nextDenominationLineToSelect >= 0 && ObjectExtensions.isNullOrUndefined(this._selectedDenominationLine)) {
                if (this._nextDenominationLineToSelect >= this.denominationLines().length) {
                    this._denominationLineListControl.selection.set(this._nextDenominationLineToSelect - 1);
                    this._selectedDenominationLine = this.denominationLines()[this._nextDenominationLineToSelect - 1];
                    this.appBarVisible(true);
                }
                else {
                    this._denominationLineListControl.selection.set(this._nextDenominationLineToSelect);
                    this._denominationLineListControl.ensureVisible(this._nextDenominationLineToSelect);
                    this._selectedDenominationLine = this.denominationLines()[this._nextDenominationLineToSelect];
                }
                this._nextDenominationLineToSelect = -1;
            }

            else if (!ObjectExtensions.isNullOrUndefined(this._selectedDenominationLine)) {
                if (this.useDenominationTotal()) {
                    this.populateAmountInput(this._selectedDenominationLine.Amount.toString());
                } else {
                    this.populateAmountInput(this._selectedDenominationLine.CountedAmount.toString());
                }

            }
        }

        private operationSuccessCallback(receipts: Model.Entities.Receipt[]): void {
            this.indeterminateWaitVisible(false);
            // show the receipts dialog.
            this.showPrintDialog(receipts);
        }

        /**
         * Shows the dialog for printing the receipts.
         * @param {Model.Entities.Receipt[]} receipts The list of receipts to show in the dialog.
         */
        private showPrintDialog(receipts: Model.Entities.Receipt[]): void {
            var dialogState: Controls.IPrintReceiptDialogState = { receipts: receipts, rejectOnHardwareStationErrors: true };
            this.showDialog(this._printReceiptDialog, dialogState).onAny(() => {
                // revert to self to avoid the use of manager's credentials
                Operations.OperationsManager.instance.revertToSelf().run().done(() => {
                    Commerce.ViewModelAdapter.navigate(this._navigationDestination);
                });
            })
        }

        /**
         * Shows a modal dialog and handle default results.
         */
        private showDialog<T, U>(dialog: Controls.ModalDialog<T, U>, input: T): IAsyncDialogResult<U> {
            return dialog.show(input)
                .onError((errors) => { Commerce.NotificationHandler.displayClientErrors(errors); });
        }

        private operationErrorCallback(errors: Model.Entities.Error[]): void {
            Commerce.NotificationHandler.displayClientErrors(errors)
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    this._processingOperation = false;
                }).fail(() => {
                    this.indeterminateWaitVisible(false);
                    this._processingOperation = false;
                });
        }

        private showBagNumberDialog() {
            if ((this.tenderCountingType === Proxy.Entities.TransactionType.BankDrop)
                && (this.viewMode() !== TenderCountingViewMode.Denominations)) {
                this._bankBagInputControl.show({ maxLength: 30, rowsNumber: 1 })
                    .on(DialogResult.OK, (inputValue) => {
                        this._bankBagNumber = inputValue;
                        this.processOperation();
                    }).onError((errors: Model.Entities.Error[]) => {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
            }
            else {
                this.processOperation();
            }
        }

        public processOperation(): void {
            // If an operation is already being processed, don't start processing another.
            if (this._processingOperation) {
                return;
            }

            this._processingOperation = true;

            if (this.viewMode() == TenderCountingViewMode.Denominations) {
                this.viewMode(TenderCountingViewMode.PaymentMethods);

                // If tender lines, the number entered is the value in the currency of the tender line
                var tenderCountingLine: Commerce.ViewModels.TenderCountingLine = this.tenderCountingLines()[this._selectedLineIndexForDenomination];
                tenderCountingLine.totalAmountInCurrency = this.totalDenominationCounted();
                tenderCountingLine.totalAmountInCurrencyToDisplay = NumberExtensions.formatCurrency(tenderCountingLine.totalAmountInCurrency, tenderCountingLine.currencyCode);
                tenderCountingLine.exchangeRate = this.storeOperationsViewModel.getCurrencyExchangeRate(tenderCountingLine.currencyCode);
                tenderCountingLine.totalAmount = this.storeOperationsViewModel.convertToStoreCurrency(tenderCountingLine.totalAmountInCurrency, tenderCountingLine.currencyCode, tenderCountingLine.exchangeRate);

                this.tenderCountingLines(this.tenderCountingLines());
                this._processingOperation = false;
            } else if (this.viewMode() == TenderCountingViewMode.PaymentMethods) {
                this.indeterminateWaitVisible(true);

                var result: IAsyncResult<Model.Entities.Receipt[]>;

                if (this.tenderCountingType == Proxy.Entities.TransactionType.TenderDeclaration) {
                    result = this.storeOperationsViewModel.tenderDeclaration(this.tenderCountingLines(), this._shiftToUse, this._reasonCodeLines);
                } else if (this.tenderCountingType == Proxy.Entities.TransactionType.BankDrop) {
                    result = this.storeOperationsViewModel.bankDrop(this.tenderCountingLines(), this._shiftToUse, this._bankBagNumber);
                } else if (this.tenderCountingType == Proxy.Entities.TransactionType.SafeDrop) {
                    result = this.storeOperationsViewModel.safeDrop(this.tenderCountingLines(), this._shiftToUse);
                }

                if (result) {
                    result.done((receipts) => { this.operationSuccessCallback(receipts); })
                        .fail((errors) => { this.operationErrorCallback(errors); });
                } else {
                    this._processingOperation = false;
                    this.indeterminateWaitVisible(false);
                }
            } else {
                this._processingOperation = false;
            }
        }
    }
}
