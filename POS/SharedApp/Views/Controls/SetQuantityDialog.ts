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
     * The state passed to the dialog upon showing it.
     */
    export interface ISetQuantityDialogState {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * The array of quantities mapped one to one with the cart lines array passed.
     */
    export interface ISetQuantityDialogOutput {
        quantities: number[];
    }

    export class SetQuantityDialog extends ModalDialog<ISetQuantityDialogState, ISetQuantityDialogOutput> {
        // Select Quantity objects (public)
        public focusOnInput: Observable<() => void>;

        // observables
        private _backButtonVisible: Observable<boolean>;
        private _itemQuantity: Observable<number>;
        private _unitOfMeasureDescription: Observable<string>;
        private _isOKButtonDisabled: Observable<boolean>;
        private _inputHasFocus: Observable<boolean>;
        private _unitOfMeasureVisible: Observable<boolean>;

        private _cartLines: Commerce.Model.Entities.CartLine[];
        private _cartLineIndex: number;
        private _quantities: number[];

        /**
         * Initializes a new instance of the SetQuantityDialog class.
         */
        constructor() {
            super();

            // Set quantity code objects
            this._backButtonVisible = ko.observable(false);
            this._itemQuantity = ko.observable(0);
            this._unitOfMeasureDescription = ko.observable("");
            this.focusOnInput = ko.observable(null);
            this._inputHasFocus = ko.observable(false);
            this._unitOfMeasureVisible = ko.observable(false);
            this._isOKButtonDisabled = ko.observable(false);

            this._cartLines = [];
            this._cartLineIndex = -1;
            this._quantities = [];
        }

        /**
         * Handles the event when the quantity changed.
         * @param {any} data The current module.
         * @param {any} event The event.
         */
        public quantityChanged(data: any, event: any): void {
            this._isOKButtonDisabled(isNaN(event.currentTarget.value));
        }

        /**
         * Shows the dialog.
         * @param {ISetQuantityDialogState} dialogState The initial dialog state containing the cart lines to get quantities for.
         */
        public onShowing(dialogState: ISetQuantityDialogState): void {
            dialogState = dialogState || { cartLines: undefined };

            // Check the parameters
            if (!ArrayExtensions.hasElements(dialogState.cartLines)) {
                this.cancelDialog();
                return;
            }

            this._cartLines = dialogState.cartLines;
            this.showItem(0);

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this.focus();
        }

        /**
         * Sets the focus on the input.
         */
        public focus(): void {
            this.focusOnInput()();
        }

        /**
         * Method called when a quantity card control button is clicked
         * @param {string} operationId The id of the button clicked
         */
        public onButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.cancelDialog();
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.onOkButtonClick();
                    break;
            }
        }

        private getTitleString(): string {
            if (ObjectExtensions.isNullOrUndefined(this._cartLines) || (this._cartLines.length <= 1)) {
                return Commerce.ViewModelAdapter.getResourceString("string_5300");
            }

            var index: number = this._cartLineIndex + 1;
            return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_5301"), index, this._cartLines.length);
        }

        private showItem(itemIndex: number): void {
            this._cartLineIndex = itemIndex;
            var cartLine: Commerce.Model.Entities.CartLine = this._cartLines[itemIndex];

            this.title(this.getTitleString());
            this.subTitle(CartLineHelper.getProductName(cartLine));

            this._itemQuantity(Math.abs(cartLine.Quantity));
            this._isOKButtonDisabled(isNaN(cartLine.Quantity));
            this._unitOfMeasureDescription(Commerce.Formatters.CartLineUnitOfMeasureFormat(cartLine));
            this._backButtonVisible(itemIndex > 0);
            this._inputHasFocus(true);
            this._unitOfMeasureVisible(!StringExtensions.isNullOrWhitespace(cartLine.UnitOfMeasureSymbol));

            this.indeterminateWaitVisible(false);
        }

        
        // This function is called by the HTML
        private itemGoBack(element: any): void {
            if (this._cartLineIndex > 0) {
                this.showItem(this._cartLineIndex - 1);
                this.focusOnInput()();
            }
        }
        

        private onOkButtonClick(): void {
            // Get the amount
            this._quantities[this._cartLineIndex] = Number(this._itemQuantity());

            // Show the next item
            var nextItemIndex: number = this._cartLineIndex + 1;
            if (nextItemIndex >= this._cartLines.length) {
                this.allQuantitiesSet();
                return;
            }

            this.showItem(nextItemIndex);
            this.focusOnInput()();
        }

        private allQuantitiesSet(): void {
            this.dialogResult.resolve(DialogResult.OK, { quantities: this._quantities });
        }

        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }
    }
}