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

    export class SelectDropDownDialog extends ModalDialog<string[], string> {

        public _optionsList: ObservableArray<string>;
        public _selectedOption: Observable<string>;

        constructor() {
            super();

            // Control objects
            this._optionsList = ko.observableArray(<string[]>[]);
            this._selectedOption = ko.observable(StringExtensions.EMPTY);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {string[]} options The list option items.
         */
        public onShowing(options: string[]) {
            if (options) {
                this._optionsList(options);
            }

            this.visible(true);
        }

       /**
        * Button click handler
        *
        * @param {string} buttonId The identifier of the button.
        */
        private buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.OK, this._selectedOption());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}