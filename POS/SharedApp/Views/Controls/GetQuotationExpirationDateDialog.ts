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

    export interface GetQuotationExpirationDateDialogOptions {
        originalExpirationDate?: Date;
    }

    export class GetQuotationExpirationDateDialog extends ModalDialog<GetQuotationExpirationDateDialogOptions, Activities.GetQuotationExpirationDateActivityResponse> {

        // Edit Quotation Expiration Date objects
        private _requestedExpirationDate: Observable<Date>;
        private _isOKButtonDisabled: Computed<boolean>;
        private _minYear: Observable<number>;

        // Select Shipping Charge objects
        private _selectQuotationExpirationDateInput: Observable<() => void>;

        /**
         * Initializes a new instance of the GetQuotationExpirationDateModalDialog class.
         */
        constructor() {
            super();

            // Set shipping charge code objects
            this._minYear = ko.observable(DateExtensions.now.getFullYear());
            this._requestedExpirationDate = ko.observable(DateExtensions.getDate());
            this._selectQuotationExpirationDateInput = ko.observable(() => { });
            this._isOKButtonDisabled = ko.computed(() => {
                var shippingDate: Date = this._requestedExpirationDate();
                return !DateExtensions.isTodayOrFutureDate(shippingDate);
            });
        }

        /**
         * Shows the dialog.
         *
         * @param {GetQuotationExpirationDateDialogOptions} dialogOptions Shipping charge dialog options.
         */
        public onShowing(dialogOptions: GetQuotationExpirationDateDialogOptions) {

            // Check the parameters
            dialogOptions = dialogOptions || {};
            dialogOptions.originalExpirationDate = dialogOptions.originalExpirationDate || DateExtensions.getDate();

            this._requestedExpirationDate(dialogOptions.originalExpirationDate);
            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this._selectQuotationExpirationDateInput()();
        }

        /**
         * Method called when a card control button is clicked.
         *
         * @param {string} operationId The id of the button clicked.
         */
        public getQuotationExpirationDateButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.getQuotationExpirationDateInvokedHandler();
                    break;
            }
        }

        private getQuotationExpirationDateInvokedHandler(): void {
            var response: Activities.GetQuotationExpirationDateActivityResponse = {
                expirationDate: this._requestedExpirationDate()
            };
            this.dialogResult.resolve(DialogResult.OK, response);
        }

        private dateChangeHandler(event: CustomEvent): void {
            var datePickerDiv: HTMLDivElement = <HTMLDivElement>event.currentTarget;
            var datePickerControl: any = datePickerDiv.winControl;
            this._requestedExpirationDate(DateExtensions.getDate(datePickerControl.current));
        }
    }
}