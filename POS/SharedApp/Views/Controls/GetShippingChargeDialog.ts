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

    export interface IGetShippingChargeDialogOptions {
        originalShippingCharge?: number;
        deliveryMethodDescription?: string;
    }

    export class GetShippingChargeDialog extends ModalDialog<IGetShippingChargeDialogOptions, Activities.GetShippingChargeActivityResponse>{

        // Edit Shipping Charge objects
        private _shippingCharge: Observable<number>;
        private _deliveryMethodDescription: Observable<string>;
        private _isOKButtonDisabled: Computed<boolean>;

        // Select Shipping Charge objects
        private _selectShippingChargeInput: Observable<() => void>;

        /**
         * Initializes a new instance of the GetShippingChargeModalDialog class.
         */
        constructor() {
            super();

            this._shippingCharge = ko.observable(0);
            this._deliveryMethodDescription = ko.observable(StringExtensions.EMPTY);
            
            this._selectShippingChargeInput = ko.observable(() => { });
            
            this._isOKButtonDisabled = ko.computed(() => {
                var shippingCharge: number = this._shippingCharge();
                return StringExtensions.isNullOrWhitespace(shippingCharge + StringExtensions.EMPTY)
                    || 0 > shippingCharge || !Helpers.CurrencyHelper.isValidAmount(shippingCharge);
            });
        }

        /**
         * Shows the dialog.
         * @param {IGetShippingChargeDialogOptions} dialogOptions Shipping charge dialog options.
         */
        public onShowing(dialogOptions: IGetShippingChargeDialogOptions): void {

            // Sanitize the parameters
            dialogOptions = dialogOptions || { };
            dialogOptions.originalShippingCharge = dialogOptions.originalShippingCharge || 0;
            dialogOptions.deliveryMethodDescription = dialogOptions.deliveryMethodDescription || StringExtensions.EMPTY;
            this._shippingCharge(dialogOptions.originalShippingCharge);
            this._deliveryMethodDescription(dialogOptions.deliveryMethodDescription);
            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this._selectShippingChargeInput()();
        }

        /**
         * Method called when a card control button is clicked.
         * @param {string} operationId The id of the button clicked.
         */
        public getShippingChargeButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.getShippingChargeInvokedHandler();
                    break;
            }
        }

        private getShippingChargeInvokedHandler(): void {
            var response: Activities.GetShippingChargeActivityResponse = {
                shippingChargeAmount: Number(this._shippingCharge())
            };

            this.dialogResult.resolve(DialogResult.OK, response);
        }
    }
}