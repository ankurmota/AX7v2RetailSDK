/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * Options to send to the control on showing the control.
     */
    export interface CashbackDialogShowOptions {
        cashbackAmount: number;  // The current cashback amount
        maximumCashbackAmount: number; // The maximum cashback amount. If 0 or negative, then there is no maximum cashback amount.
        denominations: Commerce.Model.Entities.CashDeclaration[]; // The list of denominations for the store currency
    }

    /**
     * The formatted denomination to display.
     */
    export interface FormattedDenomination {
        amountText: string; // The amount as formatted text
        amount: number; // The denomination amount
    }

    export class CashbackDialog extends ModalDialog<CashbackDialogShowOptions, number> {

        // Set Cashback objects
        private _maximumCashbackAmount: number;

        // Set Observables
        private _cashbackAmountText: Observable<string>;
        public denominations: ObservableArray<FormattedDenomination>;
        private _OKButtonDisabled: Observable<boolean>;
        public selectCashbackInput: Observable<any>;
        public showCashbackDenominations: Observable<boolean>;

        /**
         * Initializes a new instance of the CashbackDialog class.
         */
        constructor() {
            super();

            // Set cashback objects
            this._maximumCashbackAmount = 0;

            // Set observables
            this._cashbackAmountText = ko.observable(NumberExtensions.formatNumber(0, NumberExtensions.getDecimalPrecision()));
            this.denominations = ko.observableArray<FormattedDenomination>([]);
            this.selectCashbackInput = ko.observable(() => { });
            this._OKButtonDisabled = ko.observable(false);
            this.showCashbackDenominations = ko.observable(false);
        }

        /**
         * Shows the dialog.
         *
         * @param {CashbackDialogShowOptions} options The data to send to the control on show on showing the control.
         */
        public onShowing(options: CashbackDialogShowOptions) {
            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(options)) {
                RetailLogger.viewsControlsCashbackDialogOnShowingParametersUndefined();
                this.dialogResult.resolve(DialogResult.Cancel);
                return;
            }

            // Set cashback objects
            this._cashbackAmountText(NumberExtensions.formatCurrency(options.cashbackAmount ? options.cashbackAmount : 0));
            this._maximumCashbackAmount = options.maximumCashbackAmount ? options.maximumCashbackAmount : 0;

            // Set the denominations
            var formattedDenominations: FormattedDenomination[] = [];
            if (options.denominations) {
                options.denominations.forEach((denomination: Model.Entities.CashDeclaration) => {
                    if ((this._maximumCashbackAmount <= 0) || (this._maximumCashbackAmount >= denomination.Amount)) {
                        var formattedDenomination: FormattedDenomination = {
                            amount: denomination.Amount,
                            amountText: NumberExtensions.formatCurrency(denomination.Amount, denomination.Currency)
                        };
                        formattedDenominations.push(formattedDenomination);
                    }
                });
            }
            this.denominations(formattedDenominations);
            this.showCashbackDenominations(formattedDenominations.length > 0);

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this.selectCashbackInput()();
        }

        /**
         * Handles the event when the price changed.
         *
         * @param {any} data The current module.
         * @param {any} event The event.
         */
        public amountChanged(data: any, event: any): boolean {
            this._OKButtonDisabled(!this.isCashbackAmountTextValid(event.currentTarget.value));

            return true;
        }

        /**
         * Validates the cashback amount text.
         *
         * @param {string} cashbackAmountText The cashback amount.
         * @return {boolean} True if the cashback amount text is valid, false if the cashback amount text is not valid.
         */
        private isCashbackAmountTextValid(cashbackAmountText: string): boolean {
            var cashbackAmount: number = NumberExtensions.parseNumber(cashbackAmountText);
            var isCashbackAmountValid: boolean = !isNaN(cashbackAmount)
                && Helpers.CurrencyHelper.isValidAmount(cashbackAmount)
                && ((this._maximumCashbackAmount <= 0) || (this._maximumCashbackAmount >= cashbackAmount));

            return isCashbackAmountValid;
        }

        /**
         * Handles selection of a denomination.
         *
         * @param {any} itemClicked The denomination selected.
         */
        public denominationInvokedHandler(itemClicked: any) {
            this.cashbackAmountSubmitted(itemClicked.data.amountText);
        }

        /**
         * Method called when a cashback card button is clicked.
         *
         * @param {string} operationId The id of the button clicked.
         */
        public cashbackButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var cashbackAmountText: string = this._cashbackAmountText();
                    this.cashbackAmountSubmitted(cashbackAmountText);
                    break;
                case Commerce.Controls.Dialog.OperationIds.NO_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.No);
                    break;
            }
        }

        /**
         * Method called when a cashback amount has been submitted to be returned from the dialog
         *
         * @param {string} cashbackAmountText The cashback amount submitted.
         */
        private cashbackAmountSubmitted(cashbackAmountText: string) {
            if (this.isCashbackAmountTextValid(cashbackAmountText)) {
                var cashbackAmount: number = NumberExtensions.parseNumber(cashbackAmountText);
                this.dialogResult.resolve(DialogResult.OK, cashbackAmount);
            } else {
                var errors: Commerce.Model.Entities.Error[] = [];
                errors.push(new Commerce.Model.Entities.Error("string_7004")); // The cash back amount is not valid. Enter a different amount.
                Commerce.NotificationHandler.displayClientErrors(errors).done(() => {
                    this.selectCashbackInput()();
                }).fail(() => {
                        this.selectCashbackInput()();
                    });
            }
        }
    }
}