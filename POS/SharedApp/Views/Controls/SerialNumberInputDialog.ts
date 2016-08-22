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

    import Entities = Proxy.Entities;

    export interface SerialNumberInputDialogState {
        product: Entities.SimpleProduct;
    }

    export class SerialNumberInputDialog extends ModalDialog<SerialNumberInputDialogState, string> {

        private _serialNumber: Observable<string>;
        private _okButtonDisabled: Computed<Boolean>;

        constructor() {
            super();

            // Control objects
            this._serialNumber = ko.observable(StringExtensions.EMPTY);
            this.subTitleCssClass("primaryFontColor");
            this._okButtonDisabled = ko.computed(() => {
                return StringExtensions.isNullOrWhitespace(this._serialNumber());
            });

            // "Enter product serial number"
            this.title(Commerce.ViewModelAdapter.getResourceString("string_820"));
        }

        /**
         * Shows the modal dialog.
         *
         * @param {SerialNumberInputDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: SerialNumberInputDialogState) {
            if (dialogState) {
                var product: Entities.SimpleProduct = dialogState.product;

                this.subTitle(StringExtensions.format(ViewModelAdapter.getResourceString("string_929"), product.ItemId, product.Name));
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
                    this.dialogResult.resolve(DialogResult.OK, this._serialNumber());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
} 