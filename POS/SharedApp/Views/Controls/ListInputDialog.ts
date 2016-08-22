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

    export class ListInputDialog extends ModalDialog<string[], string> {

        private _options: ObservableArray<string>;

        constructor() {
            super();

            this._options = ko.observableArray(<string[]>[]);
        }

        /**
         * Shows the dialog.
         *
         * @param {string[]} options The list option items.
         */
        public onShowing(options: string[]) {
            this._options(options);
            this.visible(true);
        }

       /**
        * Button click handler.
        *
        * @param {string} buttonId The identifier of the button.
        */
        public buttonClickHandler(buttonId: string) {
            switch (buttonId) {
                case Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }

       /**
        * Option click handler.
        * @param {TileList.IItemInvokedArgs} Tile list click event. 
        * @param {return} True if the event was handled, or false, otherwise.
        */
        public optionButtonClickHandler(event: TileList.IItemInvokedArgs): boolean {
            this.dialogResult.resolve(DialogResult.OK, event.data);

            return true;
        }
    }
}