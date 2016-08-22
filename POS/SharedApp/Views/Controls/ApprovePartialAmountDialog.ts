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
     * The dialog state to set on showing
     */
    export interface ApprovePartialAmountDialogOptions {
        amountAuthorized: number;
        amountRequested: number;
        amountAuthorizedCurrencyCode: string;
        amountRequestedCurrencyCode: string;
    }

    /**
     * Dialog to capture and/or approve a signature
     */
    export class ApprovePartialAmountDialog extends ModalDialog<ApprovePartialAmountDialogOptions, any> {
        // Signature Device objects
        private amountAuthorizedText: Observable<string>;
        private amountRequestedText: Observable<string>;

        /**
         * Initializes a new instance of the SignatureDeviceDialog class.
         */
        constructor() {
            super();

            this.amountAuthorizedText = ko.observable("");
            this.amountRequestedText = ko.observable("");
        }

        /**
         * Shows the dialog.
         * @param {SignatureDeviceDialogState} dialogState The dialog state to set on showing.
         */
        public onShowing(dialogOptions: ApprovePartialAmountDialogOptions): void {
            // Check that the dialog options are provided
            if (!dialogOptions) {
                // If no dialog options, then automatically close the dialog as "Cancelled"
                this.dialogResult.resolve(DialogResult.Cancel);
                return;
            }

            // Set the amounts to display
            this.amountAuthorizedText(NumberExtensions.formatCurrency(dialogOptions.amountAuthorized, dialogOptions.amountAuthorizedCurrencyCode));
            this.amountRequestedText(NumberExtensions.formatCurrency(dialogOptions.amountRequested, dialogOptions.amountRequestedCurrencyCode));

            this.visible(true);
        }

        /**
         * Method called when a card type dialog control button is clicked
         * @param {string} operationId The id of the button clicked
         */
        private approvePartialAmountClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK: // Yes.
                    this.dialogResult.resolve(DialogResult.OK);
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK: // No.
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}