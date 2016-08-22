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

    export interface DateInputDialogState {
        minYear: number;
        maxYear: number;
    }

    export class DateInputDialog extends ModalDialog<DateInputDialogState, Date> {

        public currentDate: Observable<Date>;
        public minimumYear: Observable<number>;
        public maximumYear: Observable<number>;

        private static intervalYear: number = 50;

        /**
         * Initializes a new instance of the DateInputDialog class.
         */
        constructor() {
            super();

            var dateNow: Date = new Date();
            this.currentDate = ko.observable(dateNow);
            this.minimumYear = ko.observable(dateNow.getFullYear() - DateInputDialog.intervalYear);
            this.maximumYear = ko.observable(dateNow.getFullYear() + DateInputDialog.intervalYear);
        }

        /**
         * Shows the dialog.
         *
         * @param {DateInputDialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: DateInputDialogState) {
            this.minimumYear(dialogState.minYear);
            this.maximumYear(dialogState.maxYear);

            this.visible(true);
        }

       /**
        * Button click handler.
        *
        * @param {string} buttonId The identifier of the button.
        */
        public buttonClickHandler(buttonId: string) {
            switch (buttonId) {

                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.OK, this.currentDate());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }

        /**
         * Event handler for when the date is changed.
         *
         * @param {Event} event The event arguments.
         */
        private dateChangeHandler(event: Event): void {
            var datePickerDiv: HTMLDivElement = <HTMLDivElement>event.currentTarget;
            var datePickerControl: any = datePickerDiv.winControl;
            this.currentDate(DateExtensions.getDate(datePickerControl.current));
        }
    }
}