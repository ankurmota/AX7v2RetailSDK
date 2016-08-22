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

    import Entities = Proxy.Entities;

    export interface ISelectVariantDialogState {
        product: Entities.SimpleProduct;
    }

    export interface ISelectVariantDialogOuput {
        selectedDimensions: Entities.ProductDimension[];
    }

    export class SelectVariantDialog extends ModalDialog<ISelectVariantDialogState, ISelectVariantDialogOuput> {

        private _viewModel: ViewModels.SelectVariantViewModel;

        constructor() {
            super();

            this._viewModel = new ViewModels.SelectVariantViewModel();
        }

        /**
         * Shows the modal dialog.
         *
         * @param {VariantDialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: ISelectVariantDialogState): void {
            if (!ObjectExtensions.isNullOrUndefined(dialogState)
                && !ObjectExtensions.isNullOrUndefined(dialogState.product)) {

                var product: Entities.SimpleProduct = dialogState.product;
                this.title(ViewModelAdapter.getResourceString("string_923"));
                this.subTitle(StringExtensions.format(ViewModelAdapter.getResourceString("string_929"), product.ItemId, product.Name));

                this.indeterminateWaitVisible(true);
                this._viewModel.load({ product: product }).done((): void => {
                    this.visible(true);
                }).fail((errors: Entities.Error[]): void => {
                    NotificationHandler.displayClientErrors(errors);
                }).always((): void => {
                    this.indeterminateWaitVisible(false);
                });
            }
        }

        /**
         * Clears the modal dialog async result and the last dimension value selected.
         */
        public clearResult(): void {
            this._viewModel.undoPreviousSelection();
            super.clearResult();
        }

        
        private undoPreviousSelection(): void {
            
            this._viewModel.undoPreviousSelection();
        }

        /**
         * Click handler when any of the product variant is selected
         */
        
        private selectionInvokedHandler(event: Commerce.TileList.IItemInvokedArgs): boolean {
            

            this.indeterminateWaitVisible(true);
            this._viewModel.makeSelection(event.data).done((selectionComplete: boolean): void => {
                if (selectionComplete) {
                    this.dialogResult.resolve(DialogResult.OK, { selectedDimensions: this._viewModel.SelectedDimensions });
                } else {
                    this.indeterminateWaitVisible(false);
                }
            }).fail((errors: Entities.Error[]): void => {
                NotificationHandler.displayClientErrors(errors);
                this.indeterminateWaitVisible(false);
            });

            return true;
        }

        /**
         * Method called upon clicking a button on the dialog.
         */
        
        private dialogButtonClick(operationId: string): void {
            
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                default:
                    throw "Unknown operation Id: " + operationId;
            }
        }
    }
}