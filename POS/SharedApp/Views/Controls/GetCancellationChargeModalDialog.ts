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

    
    export interface GetCancellationChargeModalDialogOptions {
        originalCancellationCharge: number;
    }
    

    export class GetCancellationChargeModalDialog extends ModalDialog<GetCancellationChargeModalDialogOptions, number> {
        // Edit Cancellation Charge objects
        private _cancellationCharge: Observable<number>;
        private _isOKButtonDisabled: Computed<boolean>;

        // Select Cancellation Charge objects
        private _selectCancellationChargeInput: Observable<() => void>;

        /**
         * Initializes a new instance of the GetCancellationChargeModalDialog class.
         */
        constructor() {
            super();

            // Set cancellation charge code objects
            this._cancellationCharge = ko.observable(0);
            this._selectCancellationChargeInput = ko.observable(null);
            this._isOKButtonDisabled = ko.computed(() => {
                var cancellationCharge: number = this._cancellationCharge();
                return StringExtensions.isNullOrWhitespace(cancellationCharge + StringExtensions.EMPTY) ||
                    isNaN(cancellationCharge) || 0 > cancellationCharge;
            });
        }

        /**
         * Shows the dialog.
         * @param {EditCancellationModalDialogOptions} dialogOptions Cancellation charge dialog options.
         */
        public onShowing(dialogOptions: GetCancellationChargeModalDialogOptions): void {
            // Check the parameters
            if (ObjectExtensions.isNullOrUndefined(dialogOptions)
                    || ObjectExtensions.isNullOrUndefined(dialogOptions.originalCancellationCharge)) {
                dialogOptions = { originalCancellationCharge: 0 };
            }

            this._cancellationCharge(dialogOptions.originalCancellationCharge);
            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this._selectCancellationChargeInput()();
        }

        /**
         * Method called when a quantity card control button is clicked
         * @param {string} operationId The id of the button clicked
         */
        public getCancellationChargeButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.getCancellationChargeInvokedHandler();
                    break;
            }
        }

        private getCancellationChargeInvokedHandler(): void {
            this.dialogResult.resolve(DialogResult.OK, Number(this._cancellationCharge()));
        }
    }
}