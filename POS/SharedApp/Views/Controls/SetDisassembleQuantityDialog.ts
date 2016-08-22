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

    export interface SetDisassembleQuantityDialogState {
        product: Model.Entities.Product;
        originalQuantity: number;
    }

    export class SetDisassembleQuantityDialog extends ModalDialog<SetDisassembleQuantityDialogState, number> {
        // Control objects
        private _fadeToggleVisibleNonVisible: Observable<boolean>;

        // Set Quantity objects

        private _productImageUrl: Observable<string>;
        private _productDescription: Observable<string>;
        private _productNumber: Observable<string>;
        private _itemQuantity: Observable<string>;
        private _OKButtonDisabled: Observable<boolean>;
        private _setFocusDisassembleQuantityNumberInput: Observable<boolean>;

        // Set Quantity objects (public)
        public selectDisassembleQuantityNumberInput: Observable<any>;

        /**
         * Initializes a new instance of the SetDisassembleQuantityDialog class.
         */
        constructor() {
            super();

            // Control objects
            this._fadeToggleVisibleNonVisible = ko.observable(true);

            // Set quantity code objects
            this._productImageUrl = ko.observable("");
            this._productDescription = ko.observable("");
            this._productNumber = ko.observable("");
            this._itemQuantity = ko.observable("1");
            this.selectDisassembleQuantityNumberInput = ko.observable(() => { });
            this._setFocusDisassembleQuantityNumberInput = ko.observable(false);

            this.title(Commerce.ViewModelAdapter.getResourceString("string_5364"));
            this._OKButtonDisabled = ko.observable(false);
        }

        /**
         * Handles the event when the quantity changed
         *
         * @param {any} data The current module.
         * @param {any} event The event.
         */
        public quantityChanged(data: any, event: any): void {
            this._OKButtonDisabled(Commerce.StringExtensions.isNullOrWhitespace(event.currentTarget.value) || isNaN(event.currentTarget.value));
        }

        /**
         * Shows the dialog.
         *
         * @param {SetDisassembleQuantityDialogState} dialogState The dialog input.
         */
        public onShowing(dialogState: SetDisassembleQuantityDialogState) {
            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(dialogState.product)) {
                this.cancelDialog();
                return;
            }

            this._fadeToggleVisibleNonVisible(true);
            this.indeterminateWaitVisible(false);

            // display product info;
            this._productImageUrl(dialogState.product.Image.Items[0].Url);
            this._productDescription(dialogState.product.ProductName);
            this._productNumber(dialogState.product.ItemId);
            this._itemQuantity(Math.abs(dialogState.originalQuantity).toString());
            this._OKButtonDisabled(isNaN(dialogState.originalQuantity) || dialogState.originalQuantity <= 0);
            this._setFocusDisassembleQuantityNumberInput(true);

            this.selectDisassembleQuantityNumberInput()();

            this.visible(true);
        }

        private showControlError(error: Model.Entities.Error) {
            this.dialogResult.reject([error]);
            this.hide();
        }

        private setItemDisassembleQuantityInvokedHandler(operationId: Commerce.Controls.Dialog.OperationIds) {
            if (operationId === Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK) {
                var quantity: number = parseInt(this._itemQuantity());
                // If input is less than zero or is not an integer
                if (quantity <= 0 || (quantity != parseFloat(this._itemQuantity()))) {
                    ViewModelAdapterWinJS.displayMessage(
                        Commerce.ViewModelAdapterWinJS.getResourceString("string_3384"),
                        MessageType.Error,
                        MessageBoxButtons.Default,
                        Commerce.ViewModelAdapterWinJS.getResourceString("string_3383"));
                } else {
                    // Get the quantity
                    this._fadeToggleVisibleNonVisible(false);
                    this._fadeToggleVisibleNonVisible(true);
                    this.dialogResult.resolve(DialogResult.OK, quantity);
                }
            } else {
                this.cancelDialog();
            }
        }

        private cancelDialog() {
            this.dialogResult.resolve(DialogResult.Cancel);
        }
    }
}