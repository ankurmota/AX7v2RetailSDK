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
    export interface IWeighItemDialogState {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * The array of weights mapped one to one with the cart lines array passed.
     */
    export interface IWeighItemDialogOutput {
        weights: number[];
    }

    export class WeighItemDialog extends ModalDialog<IWeighItemDialogState, IWeighItemDialogOutput> {
        // Select Weight objects (public)
        public focusOnInput: Observable<() => void>;

        // observables
        private _backButtonVisible: Observable<boolean>;
        private _itemWeight: Observable<number>;
        private _unitOfMeasureDescription: Observable<string>;
        private _isOKButtonDisabled: Observable<boolean>;
        private _inputHasFocus: Observable<boolean>;
        private _unitOfMeasureVisible: Observable<boolean>;
        private _scaleSectionVisible: Observable<boolean>;
        private _weightSectionVisible: Observable<boolean>;

        private _cartLines: Commerce.Model.Entities.CartLine[];
        private _cartLineIndex: number;
        private _weights: number[];

        /**
         * Initializes a new instance of the WeighItemDialog class.
         */
        constructor() {
            super();

            // Set quantity code objects
            this._backButtonVisible = ko.observable(false);
            this._itemWeight = ko.observable(0);
            this._unitOfMeasureDescription = ko.observable("");
            this.focusOnInput = ko.observable(null);
            this._inputHasFocus = ko.observable(false);
            this._unitOfMeasureVisible = ko.observable(false);
            this._isOKButtonDisabled = ko.observable(false);

            this._cartLines = [];
            this._cartLineIndex = -1;
            this._weights = [];

            this._scaleSectionVisible = ko.observable(false);
            this._weightSectionVisible = ko.observable(false);
        }

        /**
         * Handles the event when the weight changed
         * @param {any} data The current module.
         * @param {any} event The event.
         */
        public weightChanged(data: any, event: any): void {
            this._isOKButtonDisabled(isNaN(event.currentTarget.value));
        }

        /**
         * Shows the dialog.
         * @param {IWeighItemDialogState} dialogState The WeighItemDialog dialog state.
         */
        public onShowing(dialogState: IWeighItemDialogState): void {
            dialogState = dialogState || { cartLines: undefined };

            // Check the parameters
            if (!ArrayExtensions.hasElements(dialogState.cartLines)) {
                this.cancelDialog();
                return;
            }

            this._cartLines = dialogState.cartLines;
            this.showItem(0);

            this.visible(true);
            this.checkScale();
        }

        /**
         * Sets the focus on the input.
         */
        public focus(): void {
            this.focusOnInput()();
        }

        /**
         * Method called when a weight control button is clicked
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
                return Commerce.ViewModelAdapter.getResourceString("string_5319");
            }

            var index: number = this._cartLineIndex + 1;
            return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_5320"), index, this._cartLines.length);
        }

        private showControlError(error: Model.Entities.Error): void {
            this.dialogResult.reject([error]);
            this.hide();
        }

        /**
         * Sets values specific to the item whose weight is being set and shows the manual input dialog.
         */
        private showItem(itemIndex: number): void {
            this._cartLineIndex = itemIndex;
            var cartLine: Commerce.Model.Entities.CartLine = this._cartLines[itemIndex];

            this.title(this.getTitleString());
            this.subTitle(CartLineHelper.getProductName(cartLine));

            this._itemWeight(Math.abs(cartLine.Quantity));
            this._isOKButtonDisabled(isNaN(cartLine.Quantity));
            this._unitOfMeasureDescription(cartLine.UnitOfMeasureSymbol);
            this._backButtonVisible(itemIndex > 0);
            this._inputHasFocus(true);
            this._unitOfMeasureVisible(!StringExtensions.isNullOrWhitespace(cartLine.UnitOfMeasureSymbol));
            this._weightSectionVisible(true);
            this._scaleSectionVisible(false);

            this.indeterminateWaitVisible(false);
        }

        /**
         * Shows the dialog that prompts the user to place an item on the scale.
         */
        private showScaleDialog(): void {
            this.indeterminateWaitVisible(false);
            this._scaleSectionVisible(true);
            this._weightSectionVisible(false);
            this.visible(true);
        }

        
        // This function is called by the HTML
        private itemGoBack(element: any): void {
            if (this._cartLineIndex > 0) {
                this.showItem(this._cartLineIndex - 1);
                this.focusOnInput()();
            }
        }
        

        /**
         * Checks if the scale is defined and if manual input is allowed, and displays either the prompt to weigh the item,
         * or the manual input dialog as needed. If there are errors with getting the weight, they are shown.
         */
        private checkScale(): void {
            var hardwareProfile: Model.Entities.HardwareProfile = ApplicationContext.Instance.hardwareProfile;
            if (hardwareProfile.ScaleDeviceTypeValue !== Model.Entities.PeripheralDeviceType.None) {
                // Scale device is specified - show scale prompt
                this.showScaleDialog();

                // Get weight from scale
                Commerce.Peripherals.instance.scale.read(this)
                    .done((result: number) => {
                        if (result > 0) {
                            // Set the weight retrieved from the scale as the item's quantity
                            this.showItem(0);
                            this.focus();

                            // Divide the weight from the scale by 1000 to have the correct units - same behavior is established in EPOS
                            this._itemWeight(result / 1000);
                            this.onOkButtonClick();
                        } else if (hardwareProfile.ScaleManualInputAllowed) {
                            // Show manual input dialog
                            this.showItem(0);
                            this.focus();
                        } else {
                            // The weighed item was not added to the transaction because no weight was received from the scale.
                            this.showControlError(
                                new Model.Entities.Error(ErrorTypeEnum.SCALE_RETURNED_ZERO_WITHOUT_MANUAL_ENTRY, true));
                        }
                    }).fail((errors: Model.Entities.Error[]) => {
                        this.dialogResult.reject(errors);
                        this.hide();
                    });
            } else if (hardwareProfile.ScaleManualInputAllowed) {
                // Show manual input dialog
                this.showItem(0);
                this.focus();
            } else {
                // The weighed item was not added to the transaction because a scale is not set up
                // and entering the weight manually is not allowed.
                this.showControlError(new Model.Entities.Error(ErrorTypeEnum.SCALE_UNSPECIFIED_WITHOUT_MANUAL_ENTRY, true));
            }
        }

        /**
         * Sets the weight of the item.
         */
        private onOkButtonClick(): void {
            // Get the amount
            this._weights[this._cartLineIndex] = Number(this._itemWeight());

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
            this.dialogResult.resolve(DialogResult.OK, { weights: this._weights });
        }

        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }
    }
}