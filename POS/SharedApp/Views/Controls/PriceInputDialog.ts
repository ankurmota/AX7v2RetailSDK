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

    export interface PriceInputDialogState {
        defaultPrice: number;
        minPrice: number; // The minimum value allowed. If NaN, the minimum value will not be checked.
        maxPrice: number; // The maximum value allowed. If NaN, the maximum value will not be checked.
        minPriceInclusive: boolean; // If true, the minimum value is inclusive. If false, the minimum value is exclusive (value must be greater than).
        maxPriceInclusive: boolean; // If true, the maximum value is inclusive. If false, the maximum value is exclusive (value must be less than).
    }

    export class PriceInputDialog extends ModalDialog<PriceInputDialogState, number> {

        private _priceContent: Observable<string>;
        private _minValue: number; // The minimum value allowed. If NaN, the minimum value will not be checked.
        private _maxValue: number; // The maximum value allowed. If NaN, the maximum value will not be checked.
        private _minPriceInclusive: boolean; // If true, the minimum value is inclusive. If false, the minimum value is exclusive (value must be greater than).
        private _maxPriceInclusive: boolean; // If true, the maximum value is inclusive. If false, the maximum value is exclusive (value must be less than).
        private _shouldMinAmountBeChecked: boolean;
        private _shouldMaxAmountBeChecked: boolean;
        private _minValueDisplayString: string;
        private _maxValueDisplayString: string;

        // Function which selects all text in html input control (instantiated by "ko.bindingHandlers.textInput" binding)
        public selectPriceInput: Observable<any>;

        constructor() {
            super();

            // Control objects
            this._priceContent = ko.observable("");
            this.subTitleCssClass("primaryFontColor");
            this._minValue = Number.NaN;
            this._maxValue = Number.NaN;
            this.selectPriceInput = ko.observable(() => { });
        }

        /**
         * Shows the modal dialog.
         *
         * @param {PriceInputDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: PriceInputDialogState) {
            if (dialogState) {
                this._priceContent(NumberExtensions.formatNumber(dialogState.defaultPrice, NumberExtensions.getDecimalPrecision()));
                this._minValue = dialogState.minPrice;
                this._maxValue = dialogState.maxPrice;
                this._minPriceInclusive = ObjectExtensions.isNullOrUndefined(dialogState.minPriceInclusive) ? true : dialogState.minPriceInclusive;
                this._maxPriceInclusive = ObjectExtensions.isNullOrUndefined(dialogState.maxPriceInclusive) ? true : dialogState.maxPriceInclusive;
            } else {
                this._minValue = Number.NaN;
                this._maxValue = Number.NaN;
                this._minPriceInclusive = true;
                this._maxPriceInclusive = true;
            }

            this._shouldMinAmountBeChecked = !isNaN(this._minValue) && ((this._minValue > Number.MIN_VALUE) || !this._minPriceInclusive);
            this._shouldMaxAmountBeChecked = !isNaN(this._maxValue) && ((this._maxValue < Number.MAX_VALUE) || !this._maxPriceInclusive);
            this._minValueDisplayString = NumberExtensions.formatNumber(this._minValue, NumberExtensions.getDecimalPrecision());
            this._maxValueDisplayString = NumberExtensions.formatNumber(this._maxValue, NumberExtensions.getDecimalPrecision());

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this.selectPriceInput()();
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var price = NumberExtensions.parseNumber(this._priceContent());

                    // Validate that the price is valid for the currency
                    if (!Commerce.Helpers.CurrencyHelper.isValidAmount(price)) {
                        NotificationHandler.displayErrorMessage("string_5725");
                        return;
                    }

                    // Validate that the price is within the expected range
                    var errorResourceId;
                    var firstParam: string = StringExtensions.EMPTY;
                    var secondParam: string = StringExtensions.EMPTY;
                    if (this._shouldMinAmountBeChecked && this._shouldMaxAmountBeChecked) {
                        firstParam = this._minValueDisplayString;
                        secondParam = this._maxValueDisplayString;
                        if (this._minPriceInclusive && this._maxPriceInclusive) {
                            errorResourceId = ((this._minValue <= price) && (this._maxValue >= price)) ? StringExtensions.EMPTY : "string_5707";
                        } else if (this._minPriceInclusive && !this._maxPriceInclusive) {
                            errorResourceId = ((this._minValue <= price) && (this._maxValue > price)) ? StringExtensions.EMPTY : "string_5708";
                        } else if (!this._minPriceInclusive && this._maxPriceInclusive) {
                            errorResourceId = ((this._minValue < price) && (this._maxValue >= price)) ? StringExtensions.EMPTY : "string_5709";
                        } else {
                            errorResourceId = ((this._minValue < price) && (this._maxValue > price)) ? StringExtensions.EMPTY : "string_5706";
                        }
                    } else if (this._shouldMinAmountBeChecked) {
                        firstParam = this._minValueDisplayString;
                        if (this._minPriceInclusive) {
                            errorResourceId = (this._minValue <= price) ? StringExtensions.EMPTY : "string_5710";
                        } else {
                            errorResourceId = (this._minValue < price) ? StringExtensions.EMPTY : "string_5711";
                        }
                    } else if (this._shouldMaxAmountBeChecked) {
                        firstParam = this._maxValueDisplayString;
                        if (this._maxPriceInclusive) {
                            errorResourceId = (this._maxValue >= price) ? StringExtensions.EMPTY : "string_5712";
                        } else {
                            errorResourceId = (this._maxValue > price) ? StringExtensions.EMPTY : "string_5713";
                        }
                    }

                    if (!StringExtensions.isNullOrWhitespace(errorResourceId)) {
                        NotificationHandler.displayErrorMessage(errorResourceId, firstParam, secondParam);
                        return;
                    }

                    this.dialogResult.resolve(DialogResult.OK, price);
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}