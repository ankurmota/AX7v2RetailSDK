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
     * Dialog state containing:
     * The discount type.
     * The cart line where the discount should be applied.
     */
    export interface AddDiscountDialogState {
        discountType: Model.Entities.ManualDiscountType;
        cartLine: Commerce.Model.Entities.CartLine;
    }

    /**
     * Dialog result containing:
     * The discount value.
     */
    export interface AddDiscountDialogResult {
        discountValue: number;
    }

    /**
     * Control for adding discount amount/percentage to the transaction/line.
     */
    export class AddDiscountDialog extends ModalDialog<AddDiscountDialogState, AddDiscountDialogResult> {
        // Add discount objects
        private _cartLine: Commerce.Model.Entities.CartLine;
        private _discountType: Model.Entities.ManualDiscountType;
        private _isLineDiscount: boolean;
        private _discountInputLabel: Observable<string>;
        private _discountValue: Observable<string>;
        private _setFocusDiscountValueInput: Observable<boolean>;

        // Select input objects (public)
        public selectDiscountValueInput: Observable<any>;

        /**
         * Initializes a new instance of the AddDiscountDialog class.
         */
        constructor() {
            super();

            // add discount code objects
            this._discountType = 0;
            this._isLineDiscount = false;
            this._discountValue = ko.observable("0");
            this._setFocusDiscountValueInput = ko.observable(false);
            this._discountInputLabel = ko.observable("");
            this.selectDiscountValueInput = ko.observable(() => { });
        }

        /**
         * Gets the title string given the operation type.
         * @return {string} The title string.
         */
        private getTitleString(): string {
            var title: string = null;

            switch (this._discountType) {
                case Model.Entities.ManualDiscountType.LineDiscountAmount:
                case Model.Entities.ManualDiscountType.LineDiscountPercent:
                    title = Commerce.ViewModelAdapter.getResourceString("string_5606");
                    break;
                case Model.Entities.ManualDiscountType.TotalDiscountAmount:
                case Model.Entities.ManualDiscountType.TotalDiscountPercent:
                    title = Commerce.ViewModelAdapter.getResourceString("string_5607");
                    break;
                default:
                    break;
            }

            return title;
        }

        /**
         * Gets the discount input label.
         * @return {string} The discount input label.
         */
        private getDiscountInputLabel(): string {
            var inputLabel: string = null;

            switch (this._discountType) {
                case Model.Entities.ManualDiscountType.LineDiscountAmount:
                case Model.Entities.ManualDiscountType.TotalDiscountAmount:
                    inputLabel = Commerce.ViewModelAdapter.getResourceString("string_5608");
                    break;
                case Model.Entities.ManualDiscountType.TotalDiscountPercent:
                case Model.Entities.ManualDiscountType.LineDiscountPercent:
                    inputLabel = Commerce.ViewModelAdapter.getResourceString("string_5609");
                    break;
                default:
                    break;
            }

            return inputLabel;
        }

        /**
         * Shows the dialog.
         * @param {AddDiscountDialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: AddDiscountDialogState): void {
            // initialize values
            this._discountType = dialogState.discountType;
            this._cartLine = dialogState.cartLine;
            this._discountValue("0");

            var errorCode = this.checkSupportedDiscountType();
            if (!StringExtensions.isNullOrWhitespace(errorCode)) {
                this.dialogResult.reject([new Model.Entities.Error(errorCode)]);
                return;
            }

            this.subTitle(this._isLineDiscount
                ? CartLineHelper.getProductName(this._cartLine)
                : StringExtensions.EMPTY);

            this.title(this.getTitleString());
            this._discountInputLabel(this.getDiscountInputLabel());
            this._setFocusDiscountValueInput(true);

            this.visible(true);
        }

        private afterShow(): void {
            this.selectDiscountValueInput()();
        }

        /**
         * Checks whether the operation is supported and returns the error code, if any.
         * @return {string} The error code if the operation is not supported.
         */
        private checkSupportedDiscountType(): string {
            var errorCode: string = null;
            var missingCartLine = ObjectExtensions.isNullOrUndefined(this._cartLine);

            switch (this._discountType) {
                case Model.Entities.ManualDiscountType.LineDiscountAmount:
                case Model.Entities.ManualDiscountType.LineDiscountPercent:
                    if (missingCartLine) {
                        errorCode = ErrorTypeEnum.MISSING_CARTLINE_ON_APPLY_DISCOUNT;
                    } else {
                        this._isLineDiscount = true;
                    }
                    break;
                case Model.Entities.ManualDiscountType.TotalDiscountAmount:
                case Model.Entities.ManualDiscountType.TotalDiscountPercent:
                    this._isLineDiscount = false;
                    break;
                default:
                    errorCode = ErrorTypeEnum.UNSUPPORTED_APPLY_DISCOUNT_OPERATION;
                    break;
            }

            return errorCode;
        }

        /**
         * Applies the discount.
         */
        public addDiscountButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                case Controls.Dialog.OperationIds.CLOSE_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    var testNum: number = NumberExtensions.parseNumber(this._discountValue());
                    if (isNaN(testNum) || testNum < 0) {
                        Commerce.NotificationHandler.displayErrorMessage("string_29823");
                    } else {
                        this.addDiscountSuccess();
                    }
                    break;
                default:
                    throw "Unknown operation Id: " + operationId;
                    break;
            }
        }

        /**
         * Called when the discount is added successfully.
         */
        private addDiscountSuccess(): void {
            this.dialogResult.resolve(DialogResult.OK, {
                discountValue: NumberExtensions.parseNumber(this._discountValue())
            });
        }
    }
}