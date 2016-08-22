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

    /*
     * Parameters send to EmailReceiptDialog
     */
    export interface EmailReceiptDialogParams {
        emailAddress: string;
        shouldPromptToSaveEmail: boolean;
        shouldSaveEmail: boolean;
    }

    /*
     * EmailReceiptDialog output
     */
    export interface EmailReceiptDialogOutput {
        emailAddress: string;
        shouldSaveEmail: boolean;   // Whether the checkbox was selected to save the email address
    }

    export class EmailReceiptDialog extends ModalDialog<EmailReceiptDialogParams, EmailReceiptDialogOutput> {

        private _dialogTitle: Observable<string>;
        private _dialoDescription: Observable<string>;
        private _emailAddress: Observable<string>;
        private _shouldPromptToSaveEmail: Observable<boolean>;
        private _shouldSaveEmail: Observable<boolean>;
        private _okButtonDisabled: Computed<boolean>;
        private _selectEmailAddressText: Observable<any>;

        constructor() {
            super();

            // Control objects
            this._dialogTitle = ko.observable(StringExtensions.EMPTY);
            this._dialoDescription = ko.observable(StringExtensions.EMPTY);
            this._emailAddress = ko.observable(StringExtensions.EMPTY);
            this._shouldPromptToSaveEmail = ko.observable(false);
            this._shouldSaveEmail = ko.observable(false);
            this._okButtonDisabled = ko.computed(() => { return !Commerce.Core.RegularExpressionValidations.validateEmail(this._emailAddress()); }, this);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {EmailReceiptDialogParams} dialogParams The dialog parameters.
         */
        public onShowing(dialogParams: EmailReceiptDialogParams) {
            if (dialogParams) {
                if (StringExtensions.isEmptyOrWhitespace(dialogParams.emailAddress)) {
                    this._dialogTitle(ViewModelAdapter.getResourceString("string_1800")); // Do you want to email the receipt?
                    this._dialoDescription(ViewModelAdapter.getResourceString("string_1801")); // Send a copy of the receipt to an email address.
                } else {
                    this._dialogTitle(ViewModelAdapter.getResourceString("string_1828")); // A receipt will be sent to this email address.
                    this._dialoDescription(ViewModelAdapter.getResourceString("string_1829")); // Confirm the address, or provide a different one.
                }
                this._emailAddress(dialogParams.emailAddress);
                this._shouldPromptToSaveEmail(dialogParams.shouldPromptToSaveEmail);
                this._shouldSaveEmail(dialogParams.shouldSaveEmail);
            } else {
                this._emailAddress(StringExtensions.EMPTY);
                this._shouldPromptToSaveEmail(false);
                this._shouldSaveEmail(false);
            }

            this.visible(true);
        }

       /**
        * Button click handler
        *
        * @param {string} buttonId The identifier of the button.
        */
        private buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var output: EmailReceiptDialogOutput = {
                       emailAddress: this._emailAddress(),
                       shouldSaveEmail: this._shouldSaveEmail()
                    };
                    this.dialogResult.resolve(DialogResult.OK, output);
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}