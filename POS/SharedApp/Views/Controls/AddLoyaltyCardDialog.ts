/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>
///<reference path='ReasonCodeDialog.ts'/>

module Commerce.Controls {
    "use strict";

    /**
     * The options for the AddLoyaltyCardDialog on show.
     */
    export interface AddLoyaltyCardDialogState {
        defaultLoyaltyCardId: string;  // The default loyalty card id to be displayed on dialog showing
    }

    /**
     * Dialog result containing:
     * The loyaltyCardId for add.
     */
    export interface AddLoyaltyCardDialogResult {
        loyaltyCardId: string; // The loyalty card id for add
    }

    /**
     * The AddLoyaltyCardDialog class.
     * Displays a dialog for entry of a loyalty card id.
     */
    export class AddLoyaltyCardDialog extends ModalDialog<AddLoyaltyCardDialogState, AddLoyaltyCardDialogResult> {
        private _reasonCodeDialog: Controls.ReasonCodeDialog;

        // Add loyalty card objects
        private _cart: Commerce.Model.Entities.Cart;
        private _cardNumberText: Observable<string>;
        private _isOKButtonDisabled: Computed<boolean>;
        private _setFocusToCardNumberText: Observable<boolean>;
        private _selectCardNumberText: Observable<any>;

        constructor() {
            super();

            // Add Loyalty Card objects
            this._cardNumberText = ko.observable("");
            this._setFocusToCardNumberText = ko.observable(false);
            this._selectCardNumberText = ko.observable(() => { });

            // Computed objects
            this._isOKButtonDisabled = ko.computed(() => { return StringExtensions.isEmptyOrWhitespace(this._cardNumberText()); }, this);

            this.addControl(this._reasonCodeDialog = new Controls.ReasonCodeDialog());
        }

        /**
         * Method called when a loyalty card button is clicked
         *
         * @param {string} operationId The id of the button clicked
         */
        public addLoyaltyCardButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    // Return the loyalty card id for add
                    this.dialogResult.resolve(DialogResult.OK, {
                        loyaltyCardId: this._cardNumberText()
                    });
                    break;
            }
        }

        /**
         * Shows the dialog.
         *
         * @param {addLoyaltyCardDialogState} AddLoyaltyCardDialogState The dialog options.
         */
        public onShowing(addLoyaltyCardDialogState: AddLoyaltyCardDialogState) {
            Commerce.Peripherals.instance.magneticStripeReader.enableAsync((cardInfo: Model.Entities.CardInfo) => this._cardNumberText(cardInfo.CardNumber));
            Commerce.Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => this._cardNumberText(barcode));

            var defaultLoyaltyCardId: string = "";

            // Show the current card if it exists
            if (!ObjectExtensions.isNullOrUndefined(addLoyaltyCardDialogState) && !ObjectExtensions.isNullOrUndefined(addLoyaltyCardDialogState.defaultLoyaltyCardId)) {
                defaultLoyaltyCardId = addLoyaltyCardDialogState.defaultLoyaltyCardId;
            }

            this._cardNumberText(defaultLoyaltyCardId);
            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this._setFocusToCardNumberText(true);
            this._selectCardNumberText()();
        }

        public hide(): IVoidAsyncResult {
            this._cardNumberText(StringExtensions.EMPTY);

            return super.hide();
        }

        public onHidden(): void {
            Commerce.Peripherals.instance.magneticStripeReader.disableAsync();
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
            super.onHidden();
        }
    }
}