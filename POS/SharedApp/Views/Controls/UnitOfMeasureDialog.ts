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

    /**
     * Type representing a cart line paired with the applicable unit of measure options.
     */
    type CartLineWithUnitOfMeasureOptions = {
        cartLine: Entities.CartLine;
        unitOfMeasureOptions: Entities.UnitOfMeasure[];
    };

    /**
     * The state passed to the dialog upon showing it.
     */
    export interface IUnitOfMeasureDialogState {
        cartLinesWithUnitOfMeasureOptions: {
            cartLine: Entities.CartLine;
            unitOfMeasureOptions: Entities.UnitOfMeasure[];
        }[];
    }

    /**
     * The array of unit of measures mapped one to one with the cart lines array passed.
     */
    export interface IUnitOfMeasureDialogOutput {
        selectedUnitsOfMeasure: Entities.UnitOfMeasure[];
    }

    export class UnitOfMeasureDialog extends ModalDialog<IUnitOfMeasureDialogState, IUnitOfMeasureDialogOutput> {
        // observables
        private _backButtonVisible: Observable<boolean>;
        private _unitsOfMeasureForProduct: ObservableArray<Entities.UnitOfMeasure>;

        private _cartLinesWithUnitOfMeasureOptions: CartLineWithUnitOfMeasureOptions[];
        private _cartLineIndex: number;
        private _unitsOfMeasure: Entities.UnitOfMeasure[];

        /**
         * Initializes a new instance of the UnitOfMeasureDialog class.
         */
        constructor() {
            super();

            this._backButtonVisible = ko.observable(false);
            this._unitsOfMeasureForProduct = ko.observableArray([]);

            this._cartLinesWithUnitOfMeasureOptions = [];
            this._cartLineIndex = -1;
            this._unitsOfMeasure = [];
        }

        /**
         * Shows the dialog.
         * @param {IUnitOfMeasureDialogState} dialogState The initial dialog state containing the cart lines to get unit of measures for.
         */
        public onShowing(dialogState: IUnitOfMeasureDialogState): void {
            dialogState = dialogState || { cartLinesWithUnitOfMeasureOptions: undefined };

            // Check the parameters
            if (!ArrayExtensions.hasElements(dialogState.cartLinesWithUnitOfMeasureOptions)) {
                this.cancelDialog();
                return;
            }

            this._cartLinesWithUnitOfMeasureOptions = dialogState.cartLinesWithUnitOfMeasureOptions;
            this.showItem(0);

            this.visible(true);
        }

        /**
         * Method called when a unit of measure control button is clicked.
         * @param {string} operationId The identifier of the button clicked.
         */
        public setUnitOfMeasureDialogButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.cancelDialog();
                    break;
            }
        }

        /**
         * Handles the unit of measure button click.
         * @param {TileList.IItemInvokedArgs} event Tile list event click event object.
         * @return {boolean} Returns true to indicate that the operation is handled by the payment button (used for buttonGrid).
         */
        public unitOfMeasureInvokedHandler(event: TileList.IItemInvokedArgs): boolean {
            this._unitsOfMeasure[this._cartLineIndex] = event.data;

            var nextItemIndex: number = this._cartLineIndex + 1;
            if (nextItemIndex >= this._cartLinesWithUnitOfMeasureOptions.length) {
                this.allUnitOfMeasuresSet();
                return true;
            }

            this.showItem(nextItemIndex);
            return true;
        }

        // This function is called by the HTML
        public itemGoBack(element: any): void {
            if (this._cartLineIndex > 0) {
                this.showItem(this._cartLineIndex - 1);
            }
        }

        private getTitleString(): string {
            if (ObjectExtensions.isNullOrUndefined(this._cartLinesWithUnitOfMeasureOptions) || (this._cartLinesWithUnitOfMeasureOptions.length <= 1)) {
                return Commerce.ViewModelAdapter.getResourceString("string_3200");
            }

            var index: number = this._cartLineIndex + 1;
            return StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_3201"), index, this._cartLinesWithUnitOfMeasureOptions.length);
        }

        private cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        private showItem(itemIndex: number): void {
            this._cartLineIndex = itemIndex;
            var cartLineWithUofMOptions: CartLineWithUnitOfMeasureOptions = this._cartLinesWithUnitOfMeasureOptions[itemIndex];

            this.title(this.getTitleString());
            this.subTitle(CartLineHelper.getProductName(cartLineWithUofMOptions.cartLine));

            var unitsOfMeasure: Entities.UnitOfMeasure[] = cartLineWithUofMOptions.unitOfMeasureOptions;
            this._unitsOfMeasureForProduct(unitsOfMeasure);
            this._backButtonVisible(itemIndex > 0);
            this.title(this.getTitleString());

            this.indeterminateWaitVisible(false);
        }

        private allUnitOfMeasuresSet(): void {
            this.dialogResult.resolve(DialogResult.OK, { selectedUnitsOfMeasure: this._unitsOfMeasure });
        }
    }
}