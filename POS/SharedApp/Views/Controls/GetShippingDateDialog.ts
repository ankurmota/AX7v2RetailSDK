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

    export interface GetShippingDateDialogOptions {
        originalShippingDate?: Date;
        deliveryMethodDescription?: string;
    }

    export class GetShippingDateDialog extends ModalDialog<GetShippingDateDialogOptions, Activities.GetShippingDateActivityResponse> {

        // Edit Shipping Date objects
        private _requestedShipDate: Observable<Date>;
        private _deliveryMethodDescription: Observable<string>;
        private _isOKButtonDisabled: Computed<boolean>;
        private _minYear: Observable<number>;

        // Select Shipping Charge objects
        private _selectShippingDateInput: Observable<() => void>;

        /**
         * Initializes a new instance of the GetShippingDateModalDialog class.
         */
        constructor() {
            super();

            // Set shipping charge code objects
            this._minYear = ko.observable(DateExtensions.now.getFullYear());
            this._requestedShipDate = ko.observable(DateExtensions.getDate());
            this._deliveryMethodDescription = ko.observable(StringExtensions.EMPTY);
            this._selectShippingDateInput = ko.observable(() => { });
            this._isOKButtonDisabled = ko.computed(() => {
                var shippingDate: Date = this._requestedShipDate();
                return !DateExtensions.isTodayOrFutureDate(shippingDate);
            });
        }

        /**
         * Shows the dialog.
         *
         * @param {GetShippingDateDialogOptions} dialogOptions Shipping charge dialog options.
         */
        public onShowing(dialogOptions: GetShippingDateDialogOptions) {

            // Check the parameters
            dialogOptions = dialogOptions || {};
            dialogOptions.originalShippingDate = dialogOptions.originalShippingDate || DateExtensions.getDate();
            dialogOptions.deliveryMethodDescription = dialogOptions.deliveryMethodDescription || StringExtensions.EMPTY;

            this._requestedShipDate(dialogOptions.originalShippingDate);
            this._deliveryMethodDescription(dialogOptions.deliveryMethodDescription);
            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Called after dialog is shown.
         */
        public afterShow(): void {
            this._selectShippingDateInput()();
        }

        /**
         * Method called when a card control button is clicked.
         *
         * @param {string} operationId The id of the button clicked.
         */
        public getShippingDateButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.getShippingDateInvokedHandler();
                    break;
            }
        }

        private getShippingDateInvokedHandler(): void {
            var response: Activities.GetShippingDateActivityResponse = {
                requestedShippingDate: this._requestedShipDate()
            };
            this.dialogResult.resolve(DialogResult.OK, response);
        }

        private dateChangeHandler(event: CustomEvent): void {
            var datePickerDiv: HTMLDivElement = <HTMLDivElement>event.currentTarget;
            var datePickerControl: any = datePickerDiv.winControl;
            this._requestedShipDate(DateExtensions.getDate(datePickerControl.current));
        }
    }
}