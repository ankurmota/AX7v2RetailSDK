/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path="ModalDialog.ts"/>

module Commerce.Controls {
    "use strict";

    export enum GiftCardMode {
        None = 0,
        IssueNew = 1,
        AddExisting = 2,
        CheckBalance = 3
    }

    export interface IAddIssueGiftCardDialogOptions {
        cartViewModel: ViewModels.CartViewModel;
    }

    export class AddIssueGiftCardDialog extends ModalDialog<GiftCardMode, any> {

        public giftCardNumber: Observable<string>;       // The gift card identifier
        public giftCardAmount: Observable<string>;       // The amount to set or add to a gift card (used in Add/Issue Gift Card)
        public giftCardAmountBalance: Computed<string>;  // The balance on an existing gift card (used in Check Gift Card Balance)
        public amountReadOnly: Observable<string>;
        public dialogMessage: Observable<string>;
        public cardAmountSectionLabel: Observable<string>;
        public okButtonLabel: Observable<string>;
        public cancelButtonLabel: Observable<string>;
        public isInputAmount: Computed<boolean>;
        public isNotInputAmount: Computed<boolean>;
        public selectGiftCardNumberText: Observable<any>;

        private _giftCardMode: Observable<GiftCardMode>;
        private _controlsEnabled: Observable<boolean>;
        private _isOKButtonDisabled: Computed<boolean>;
        private _isCancelButtonDisabled: Computed<boolean>;
        private _cartViewModel: ViewModels.CartViewModel;

        constructor(options: IAddIssueGiftCardDialogOptions) {
            super();

            this._cartViewModel = options.cartViewModel || new ViewModels.CartViewModel();

            this._giftCardMode = ko.observable(GiftCardMode.None);
            this._controlsEnabled = ko.observable(true);
            this.dialogMessage = ko.observable(StringExtensions.EMPTY);
            this.giftCardNumber = ko.observable(StringExtensions.EMPTY);
            this.giftCardAmount = ko.observable(StringExtensions.EMPTY);
            this.amountReadOnly = ko.observable(null);
            this.cardAmountSectionLabel = ko.observable(StringExtensions.EMPTY);

            this._controlsEnabled.subscribe(this.computeOKButtonDisabled, this);

            this.okButtonLabel = ko.observable(StringExtensions.EMPTY);
            this.cancelButtonLabel = ko.observable(StringExtensions.EMPTY);
            this._isOKButtonDisabled = ko.computed(() => { return this.computeOKButtonDisabled(); }, this);
            this._isCancelButtonDisabled = ko.computed(() => { return this.computeCancelButtonDisabled(); }, this);
            this.isInputAmount = ko.computed(() => { return this.computeIsInputAmount(); }, this);
            this.isNotInputAmount = ko.computed(() => { return this.computeIsNotInputAmount(); }, this);
            this.giftCardAmountBalance = ko.computed(() => { return this.computeGiftCardAmountBalance(); }, this);
            this.selectGiftCardNumberText = ko.observable(null);
        }

       /**
        * Shows the dialog.
        * @param {GiftCardMode} giftCardMode The gift card mode.
        */
        public onShowing(giftCardMode: GiftCardMode): void {
            Commerce.Peripherals.instance.magneticStripeReader.enableAsync((cardInfo: Model.Entities.CardInfo) => this.giftCardNumber(cardInfo.CardNumber));
            Commerce.Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => this.giftCardNumber(barcode));

            this.setGiftCardMode(giftCardMode);

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this.selectGiftCardNumberText()();
        }

        public giftCardButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.giftCardHandler();
                    break;
                default:
                    throw "Unknown operation Id: " + operationId;
            }
        }

        public hide(): IVoidAsyncResult {
            this.giftCardNumber(StringExtensions.EMPTY);
            this.giftCardAmount(StringExtensions.EMPTY);

            return super.hide();
        }

        public onHidden(): void {
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
            Commerce.Peripherals.instance.magneticStripeReader.disableAsync();
        }

        public giftCardHandler(): void {
            var cardNumber: string = this.giftCardNumber();
            var amount: number = NumberExtensions.parseNumber(this.giftCardAmount());
            var currency: string = Commerce.ApplicationContext.Instance.deviceConfiguration.Currency;
            var lineDescription: string = this.title();
            var errors: Array<Model.Entities.Error>;

            var self: AddIssueGiftCardDialog = this;
            switch (this._giftCardMode()) {
                case GiftCardMode.IssueNew:
                    errors = this.preValidateOperation(amount);
                    if (errors.length > 0) {
                        NotificationHandler.displayClientErrors(errors);
                        return;
                    }
                    this.waitForAsyncResult();
                    this._cartViewModel.issueGiftCardAsync(cardNumber, amount, currency, lineDescription)
                        .done(() => {
                            this.showControlSuccess();
                            this.hide();
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            this.giftCardErrorHandle(errors);
                        });
                    break;
                case GiftCardMode.AddExisting:
                    errors = this.preValidateOperation(amount);
                    if (errors.length > 0) {
                        NotificationHandler.displayClientErrors(errors);
                        return;
                    }
                    this.waitForAsyncResult();
                    this._cartViewModel.addToGiftCardAsync(cardNumber, amount, currency, lineDescription)
                        .done(() => {
                            this.showControlSuccess();
                            this.hide();
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            this.giftCardErrorHandle(errors);
                        });
                    break;
                case GiftCardMode.CheckBalance:
                    this.waitForAsyncResult();
                    this._cartViewModel.getGiftCardByIdAsync(cardNumber)
                        .done((card: Model.Entities.GiftCard) => {
                            self.afterAsyncResult();
                            this.indeterminateWaitVisible(false);
                            self.giftCardAmount(NumberExtensions.formatNumber(card.Balance, NumberExtensions.getDecimalPrecision()));
                        }).fail((errors: Model.Entities.Error[]) => {
                            self.giftCardAmount(StringExtensions.EMPTY);
                            this.giftCardErrorHandle(errors);
                        });
                    break;
                default:
                    throw "Unknown operation: " + this._giftCardMode();
            }
        }

        private preValidateOperation(amount: number): Model.Entities.Error[] {
            var errors: Array<Model.Entities.Error> = new Array<Commerce.Model.Entities.Error>();
            if (amount < 0) {
                errors.push(new Commerce.Model.Entities.Error("string_2132"));
            }

            return errors;
        }

        /**
         * Error handler for gift card. By default will close the control.
         * @param {Model.Entities.Error[]} errors List of errors
         */
        private giftCardErrorHandle(errors: Model.Entities.Error[]): void {
            this.visible(true);
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors).done(() => {
                this.afterAsyncResult();
            }).fail(() => {
                this.afterAsyncResult();
            });
        }

        /**
         * Computes whether the OK button should be disabled.
         * @return {boolean} True if the OK button should be disabled, false otherwise.
         */
        private computeOKButtonDisabled(): boolean {
            return StringExtensions.isEmptyOrWhitespace(this.giftCardNumber()) ||
                (this._giftCardMode() !== GiftCardMode.CheckBalance && !this.computeValidCardAmount()) ||
                !this._controlsEnabled();
        }

        /**
         * Computes whether the cancel button should be disabled.
         * @return {boolean} True if the cancel button should be disabled, false otherwise.
         */
        private computeCancelButtonDisabled(): boolean {
            return !this._controlsEnabled();
        }

        private computeIsInputAmount(): boolean {
            return this._giftCardMode() !== GiftCardMode.CheckBalance;
        }

        private computeIsNotInputAmount(): boolean {
            return this._giftCardMode() === GiftCardMode.CheckBalance;
        }

        private computeValidCardAmount(): boolean {
            var amountString: string = this.giftCardAmount();

            if (StringExtensions.isNullOrWhitespace(amountString)) {
                return false;
            }

            var decimalAmount: number = NumberExtensions.parseNumber(this.giftCardAmount());
            return !isNaN(decimalAmount) && decimalAmount !== 0;
        }

        /**
         * Computes the gift card balance as a displayable string
         * @return {string} The gift card balance as a displayable string
         */
        private computeGiftCardAmountBalance(): string {
            var decimalAmount: number = NumberExtensions.parseNumber(this.giftCardAmount());
            var decimalAmountFormatted: string = isNaN(decimalAmount) ? StringExtensions.EMPTY
                : Commerce.Formatters.PriceFormatter(decimalAmount);
            return decimalAmountFormatted;
        }

        private disableControls(): void {
            this._controlsEnabled(false);
        }

        private enableControls(): void {
            this._controlsEnabled(true);
        }

        private waitForAsyncResult(): void {
            this.indeterminateWaitVisible(true);
            this.disableControls();
        }

        private afterAsyncResult(): void {
            this.enableControls();
        }

        private showControlSuccess(): void {
            this.afterAsyncResult();
            this.dialogResult.resolve(DialogResult.OK);
        }

        private setGiftCardMode(mode: GiftCardMode): void {
            switch (mode) {
                case GiftCardMode.IssueNew:
                    this.title(Commerce.ViewModelAdapter.getResourceString("string_5100"));
                    this.dialogMessage(Commerce.ViewModelAdapter.getResourceString("string_5110"));
                    this.cardAmountSectionLabel(Commerce.ViewModelAdapter.getResourceString("string_5151")); // Amount
                    this.okButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_75"));            // OK
                    this.cancelButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_107"));       // Cancel
                    this.amountReadOnly(null);
                    break;
                case GiftCardMode.AddExisting:
                    this.title(Commerce.ViewModelAdapter.getResourceString("string_5101"));
                    this.dialogMessage(Commerce.ViewModelAdapter.getResourceString("string_5111"));
                    this.cardAmountSectionLabel(Commerce.ViewModelAdapter.getResourceString("string_5151")); // Amount
                    this.okButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_75"));            // OK
                    this.cancelButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_107"));       // Cancel
                    this.amountReadOnly(null);
                    break;
                case GiftCardMode.CheckBalance:
                    this.title(Commerce.ViewModelAdapter.getResourceString("string_5103"));
                    this.dialogMessage(Commerce.ViewModelAdapter.getResourceString("string_5112"));
                    this.cardAmountSectionLabel(Commerce.ViewModelAdapter.getResourceString("string_5153")); // Balance
                    this.okButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_5190"));          // Check balance
                    this.cancelButtonLabel(Commerce.ViewModelAdapter.getResourceString("string_80"));        // Close
                    this.amountReadOnly("true");
                    break;
                default:
                    break;
            }

            this._giftCardMode(mode);
        }
    }
}