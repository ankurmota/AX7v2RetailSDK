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

    export interface AskQuestionDialogState {
        content: string;
    }

    export class AskQuestionDialog extends ModalDialog<AskQuestionDialogState, string> {

        private _textContent: Observable<string>;

        constructor() {
            super();

            // Control objects
            this._textContent = ko.observable(StringExtensions.EMPTY);
        }

        /**
         * Shows the modal dialog.
         *
         * @param {AskQuestionDialogState} dialogState The modal dialog state.
         */
        public onShowing(dialogState: AskQuestionDialogState) {
            if (dialogState) {
                this._textContent(dialogState.content);
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
                    this.dialogResult.resolve(DialogResult.Yes, this._textContent());
                    break;

                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}