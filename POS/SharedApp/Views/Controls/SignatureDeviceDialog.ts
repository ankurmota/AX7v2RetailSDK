/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='ModalDialog.ts'/>

module Commerce.Controls {
    "use strict";

    /*
     * The dialog state to set on showing
     *
     */
    export interface SignatureDeviceDialogState {
        allowSkip: boolean;         // Indicates whether to allow skipping the signature.
        allowGetSignature: boolean; // Indicates whether to show the button to retrieve the signature from a peripheral.
    }

    /*
     * Dialog to capture and/or approve a signature
     */
    export class SignatureDeviceDialog extends ModalDialog<SignatureDeviceDialogState, any> {
        // Signature Device objects
        private allowSkip: Observable<boolean>;
        private allowGetSignature: Observable<boolean>;
        public dialogCloseAction: Commerce.DialogResult; // Used to track how the dialog is closed

        /**
         * Initializes a new instance of the SignatureDeviceDialog class.
         */
        constructor() {
            super();

            this.allowSkip = ko.observable(false);
            this.allowGetSignature = ko.observable(false);
        }

        /**
         * Shows the dialog.
         *
         * @param {SignatureDeviceDialogState} dialogState The dialog state to set on showing.
         */
        public onShowing(dialogState: SignatureDeviceDialogState) {
            if (!dialogState) {
                dialogState = {
                    allowSkip: false,
                    allowGetSignature: false
                };
            }

            // Set the enabled state of the buttons
            this.allowSkip(dialogState.allowSkip);

            // Set the visible state of the buttons
            this.allowGetSignature(dialogState.allowGetSignature);

            // Set the dialog action state
            this.dialogCloseAction = null;

            this.visible(true);
        }

        //
        // Dialog methods
        //

        /**
         * Method called when a card type dialog control button is clicked
         *
         * @param {string} operationId The id of the button clicked
         */
        public signatureDeviceDialogButtonClickHandler(operationId: string): void {
            switch (operationId) {
                case Commerce.Controls.Dialog.OperationIds.NO_BUTTON_CLICK: // Skip
                    this.dialogCloseAction = DialogResult.No;
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK: // Get Signature.
                    this.dialogCloseAction = DialogResult.OK;
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK: // Sign on screen.
                    this.dialogCloseAction = DialogResult.Cancel;
                    break;
            }

            if (this.dialogCloseAction) {
                this.dialogResult.resolve(this.dialogCloseAction);
            }
        }
    }
}