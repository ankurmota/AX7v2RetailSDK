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

    export interface TextInputDialogState {
        content?: string;
        maxLength?: number;
        rowsNumber?: number;
        enableBarcodeScan?: boolean;
        type?: string;
        labelResx?: string;
        hideScrollbar?: boolean;
        enterKeyDisabled?: boolean;
    }

    export class TextInputDialog extends ModalDialog<TextInputDialogState, string> {

        private static DEFAULT_ROWS_NUMBER: number = 5;
        private _textContent: Observable<string>;
        private _textMaxLength: Observable<number>;
        private _rowsNumber: Observable<number>;
        private _scannerEnabled: boolean;
        private _type: Observable<string>;
        private _isTextType: Computed<boolean>;
        private _labelText: Observable<string>;
        private _hideScrollbar: Observable<boolean>;
        private _enterKeyDisabled: boolean;

        constructor() {
            super();

            // Control objects
            this._textContent = ko.observable("");
            this._textMaxLength = ko.observable(null);
            this._rowsNumber = ko.observable(TextInputDialog.DEFAULT_ROWS_NUMBER);
            this._type = ko.observable(null);
            this._isTextType = ko.computed(() => this.getIsTextType());
            this._labelText = ko.observable(null);
            this._hideScrollbar = ko.observable(false);
            this._enterKeyDisabled = false;
        }

        private onkeydown(data: any, event: any): boolean {
            if (event.keyCode == 13 && this._enterKeyDisabled) {
                event.preventDefault();
                return false;
            }

            return true;
        }

        /**
         * Shows the modal dialog.
         *
         * @param {TextInputDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: TextInputDialogState) {
            if (dialogState) {
                this._textContent(dialogState.content);
                this._textMaxLength(dialogState.maxLength);
                this._type(dialogState.type);
                this._labelText(dialogState.labelResx ? ViewModelAdapter.getResourceString(dialogState.labelResx) : null);

                if (!ObjectExtensions.isNullOrUndefined(dialogState.rowsNumber)) {
                    this._rowsNumber(dialogState.rowsNumber);
                }

                if (!ObjectExtensions.isNullOrUndefined(dialogState.hideScrollbar)) {
                    this._hideScrollbar(dialogState.hideScrollbar);
                }

                if (!ObjectExtensions.isNullOrUndefined(dialogState.enterKeyDisabled)) {
                    this._enterKeyDisabled = dialogState.enterKeyDisabled;
                }

                if (dialogState.enableBarcodeScan) {
                    Commerce.Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                        this._textContent(barcode);
                    }).done(() => { this._scannerEnabled = true; });
                }
            }

            this.visible(true);
        }

        /**
         * This is called when the dialog is completely hidden.
         */
        public onHidden(): void {
            if (this._scannerEnabled) {
                Commerce.Peripherals.instance.barcodeScanner.disableAsync();
            }

            super.onHidden();
        }

       /**
        * Button click handler
        *
        * @param {string} buttonId The identifier of the button.
        */
        private buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.OK, this._textContent());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }

        private getIsTextType(): boolean {
            var type: string = this._type();
            return StringExtensions.isNullOrWhitespace(type) || StringExtensions.compare(type, "text", true) === 0;
        }
    }
}