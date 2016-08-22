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

    export interface NumberInputDialogState {
        content: number;
        min: number;
        max: number;
    }

    export class NumberInputDialog extends ModalDialog<NumberInputDialogState, number> {

        private _numberContent: Observable<string>;
        private _minValue: number;
        private _maxValue: number;
        private _label: Observable<string>;

        // Function which selects all text in html input control (instantiated by "ko.bindingHandlers.textInput" binding)
        public selectNumberInput: Observable<any>;

        constructor() {
            super();

            // Control objects
            this._numberContent = ko.observable("0");
            this._minValue = Number.MIN_VALUE;
            this._maxValue = Number.MAX_VALUE;
            this._label = ko.observable('');
            this.selectNumberInput = ko.observable(() => { });
        }

        /**
         * Gets the field label observable.
         *
         * @return {Observable<string>} The field label observable.
         */
        public get label(): Observable<string> {
            return this._label;
        }

        /**
         * Shows the modal dialog.
         *
         * @param {NumberInputDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: NumberInputDialogState) {
            if (dialogState) {
                this._numberContent(dialogState.content.toString());
                if (ObjectExtensions.isNumber(dialogState.min) && !isNaN(dialogState.min)) {
                    this._minValue = dialogState.min;
                }
                if (ObjectExtensions.isNumber(dialogState.max) && !isNaN(dialogState.max)) {
                    this._maxValue = dialogState.max;
                }
            }

            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this.selectNumberInput()();
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(buttonId: string) {

            // Handle the button click
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var numberAsText: string = this._numberContent();
                    var numberAsDecimal: number = Number(numberAsText);
                    var errors: Commerce.Model.Entities.Error[] = [];

                    if (StringExtensions.isNullOrWhitespace(numberAsText) || isNaN(numberAsDecimal)) {
                        // Validate a number was entered.
                        errors.push(new Proxy.Entities.Error(ErrorTypeEnum.SET_QUANTITY_NOT_A_NUMBER));
                    } else if (numberAsDecimal > this._maxValue) {
                        // Validate the number entered is less than the maximum allowed value.
                        errors.push(new Proxy.Entities.Error(ErrorTypeEnum.NUMBER_INPUT_VALUE_GREATER_THAN_MAXIMUM_ALLOWED));
                    } else if (numberAsDecimal < this._minValue) {
                        // Validate the number entered is greater than the minimum allowed value.
                        errors.push(new Proxy.Entities.Error(ErrorTypeEnum.NUMBER_INPUT_VALUE_LESS_THAN_MINIMUM_ALLOWED));
                    }

                    if (ArrayExtensions.hasElements(errors)) {
                        Commerce.NotificationHandler.displayClientErrors(errors)
                            .always(() => {
                                this.selectNumberInput()();
                            });
                    } else {
                        this.dialogResult.resolve(DialogResult.OK, numberAsDecimal);
                    }

                    break;
                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}