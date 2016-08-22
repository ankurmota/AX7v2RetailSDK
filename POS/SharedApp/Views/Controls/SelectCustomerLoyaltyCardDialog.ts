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

    export interface ISelectCustomerLoyaltyCardDialogOptions {
        loyaltyCards: ObservableArray<Model.Entities.LoyaltyCard>;
        enableSelect?: boolean;
    }

    /**
     * The options for the SelectCustomerLoyaltyCardDialog on show.
     */
    export interface ISelectCustomerLoyaltyCardDialogState {
        // current loyalty card id on the transaction
        currentLoyaltyCardId: string;
    }

    export interface ISelectCustomerLoyaltyCardDialogOutput {
        selectedLoyaltyCardId: string;
    }

    export class SelectCustomerLoyaltyCardDialog extends ModalDialog<ISelectCustomerLoyaltyCardDialogState, ISelectCustomerLoyaltyCardDialogOutput> {

        private _loyaltyCards: ObservableArray<Model.Entities.LoyaltyCard>;
        public enableSelect: Observable<boolean>;

        constructor(options: ISelectCustomerLoyaltyCardDialogOptions) {
            super();

            // options is a required interface for this class, setting up null check here for good measure
            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this._loyaltyCards = options.loyaltyCards;
                this.enableSelect = (Commerce.ObjectExtensions.isNullOrUndefined(options.enableSelect)) ? ko.observable(false) : ko.observable(options.enableSelect);
            }

            this.title(Commerce.ViewModelAdapter.getResourceString("string_3265"));  // Customer loyalty cards
        }

        /**
         * Method called when a dialog button is clicked
         *
         * @param {string} operationId The id of the button clicked
         */
        public selectCustomerLoyaltyCardButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this._cancelDialog();
                    break;
            }
        }

        /**
         * Dialog cancel handler
         */
        private _cancelDialog() {
            this.dialogResult.resolve(DialogResult.Cancel);
        }
        
        /**
         * Shows the dialog.
         *
         * @param {ISelectCustomerLoyaltyCardDialogState} selectCustomerLoyaltyCardDialogState The dialog options.
         */
        public onShowing(selectCustomerLoyaltyCardDialogState: ISelectCustomerLoyaltyCardDialogState) {
            // if enableSelect is false, make the dialog read-only, and remove message to
            // "select loyalty card to add to the transaction"
            this.subTitle(this.enableSelect() ? Commerce.ViewModelAdapter.getResourceString("string_3266") : "");  // Select a loyalty card to add to the transaction
            
            this.visible(true);
        }

        /**
         * Click handler for selecting between multiple customer loyalty cards
         * 
         * @param {any} self a reference to this view controller
         * @param {Model.Entities.LoyaltyCard} selectedLoyaltyCard the selected (invoked) loyalty card
         */
        public loyaltyCardInvokedHandler(selectedLoyaltyCard: Model.Entities.LoyaltyCard) {
            // check whether this item is disabled (CardTenderType is not none or whatever)
            if (this.enableSelect() && !ObjectExtensions.isNullOrUndefined(selectedLoyaltyCard)) {
                this.dialogResult.resolve(DialogResult.OK, { selectedLoyaltyCardId: selectedLoyaltyCard.CardNumber });
            }
        }
    }
}