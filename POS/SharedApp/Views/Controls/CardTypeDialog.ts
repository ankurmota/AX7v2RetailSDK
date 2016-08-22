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

    /*
     * Dialog to select a card type
     */
    export class CardTypeDialog extends ModalDialog<Commerce.Model.Entities.CardTypeInfo, any> {
        // Card Type objects
        public cardTypes: ObservableArray<Commerce.Model.Entities.CardTypeInfo>;

        /**
         * Initializes a new instance of the CardTypeDialog class.
         */
        constructor() {
            super();

            this.cardTypes = ko.observableArray<Model.Entities.CardTypeInfo>([]);
        }

        /**
         * Shows the dialog.
         *
         * @param {Commerce.Model.Entities.CardTypeInfo[]} cardTypes The array of card types to select from
         */
        public onShowing(cardTypes: Commerce.Model.Entities.CardTypeInfo[]) {
            // Check the parameters
            if (!ArrayExtensions.hasElements(cardTypes)) {
                this.cancelDialog();
                return;
            }

            this.cardTypes(cardTypes);
            this.visible(true);
        }

        /**
         * Handles the card type button click.
         *
         * @param {Commerce.TileList.IItemInvokedArgs} eventArgs The event data
         */
        public cardTypeListButtonClickHandler(eventArgs: Commerce.TileList.IItemInvokedArgs) {
            var cardType: Commerce.Model.Entities.CardTypeInfo = eventArgs.data;
            this.dialogResult.resolve(DialogResult.OK, cardType);
        }

        /**
          * Cancel the dialog.
          */
        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Method called when a card type dialog control button is clicked
         *
         * @param {string} operationId The id of the button clicked
         */
        public cardTypeDialogButtonClickHandler(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.cancelDialog();
                    break;
            }
        }
    }
}