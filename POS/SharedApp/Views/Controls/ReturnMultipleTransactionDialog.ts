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

    export interface IReturnMultipleTransactionDialogState {
        storeList: Model.Entities.ISearchReceiptStore[];
    }

    export interface IReturnMultipleTransactionDialogOutput {
        salesOrder: Model.Entities.SalesOrder;
    }

    export class ReturnMultipleTransactionDialog extends ModalDialog<IReturnMultipleTransactionDialogState, IReturnMultipleTransactionDialogOutput> {
        // Multiple transaction objects
        private _multipleTransactionStoreNames: ObservableArray<Model.Entities.ISearchReceiptStore>;
        private _multipleTransactionRegisterNumbers: ObservableArray<Model.Entities.ISearchReceiptRegister>;
        private _multipleTransactionStore: Observable<Model.Entities.ISearchReceiptStore>;
        private _multipleTransactionRegister: Observable<Model.Entities.ISearchReceiptRegister>;
        private _isOKButtonDisabled: Observable<boolean>;
        private _multipleTransactionRegisterNumberDisabled: Observable<boolean>;
        private _multipleTransactionStoreNameFocus: Observable<boolean>;
        private _multipleTransactionRegisterNumberFocus: Observable<boolean>;

        private _selectedSalesOrder: Commerce.Model.Entities.SalesOrder;

        /**
         * Initializes a new instance of the ReturnMultipleTransactionDialog class.
         */
        constructor() {
            super();

            // Multiple transaction objects
            this._multipleTransactionStoreNames = ko.observableArray([]);
            this._multipleTransactionRegisterNumbers = ko.observableArray([]);
            this._multipleTransactionStore = ko.observable(null);
            this._multipleTransactionRegister = ko.observable(null);
            this._isOKButtonDisabled = ko.observable(true);
            this._multipleTransactionRegisterNumberDisabled = ko.observable(true);
            this._multipleTransactionStoreNameFocus = ko.observable(false);
            this._multipleTransactionRegisterNumberFocus = ko.observable(false);

            // Subscribed events
            this._multipleTransactionStore.subscribe((newValue: any) => {
                if (newValue === undefined) {
                    this._multipleTransactionRegisterNumberDisabled(true);
                    this._isOKButtonDisabled(true);
                    this._multipleTransactionRegisterNumbers([]);
                } else {
                    this._multipleTransactionRegisterNumbers(newValue);
                    this._multipleTransactionRegisterNumberDisabled(false);
                    this._multipleTransactionRegisterNumberFocus(true);
                }
            }, this);

            this._multipleTransactionRegister.subscribe((newValue: any) => {
                this._isOKButtonDisabled(newValue === undefined);
                if (newValue === undefined) {
                    this._selectedSalesOrder = null;
                } else {
                    this._selectedSalesOrder = newValue;
                }
            }, this);
        }

        /**
         * Method called when a loyalty card button is clicked.
         * @param {string} operationId The id of the button clicked.
         */
        public returnMultipleTransactionDialogButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.cancelDialog();
                    break;
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.transactionSelected();
                    break;
            }
        }

        /**
         * Shows the dialog.
         * @param {Model.Entities.ISearchReceiptStore[]} storeList The list of stores and registers for the orders to be displayed.
         */
        public onShowing(dialogState: IReturnMultipleTransactionDialogState): void {
            var storeList: Model.Entities.ISearchReceiptStore[] = dialogState.storeList;
            if (ObjectExtensions.isNullOrUndefined(storeList) || (storeList.length === 0)) {
                this.cancelDialog();
                return;
            }

            this._multipleTransactionStoreNames(storeList);
            this._multipleTransactionRegisterNumberDisabled(true);
            this._isOKButtonDisabled(true);
            this._multipleTransactionStoreNameFocus(true);

            this.visible(true);
        }

        /**
         * Cancels the dialog.
         */
        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Resolves the dialog with the selected sales order once a transaction is selected.
         */
        private transactionSelected(): void {
            this.dialogResult.resolve(DialogResult.OK, { salesOrder: this._selectedSalesOrder });
        }
    }
}