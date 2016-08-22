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

    export interface VariantDialogState {
        products: Commerce.Model.Entities.Product[];
    }

    export class VariantDialog extends ModalDialog<VariantDialogState, number[]> {
        private _selectedProducts: Model.Entities.Product[];
        private _currentProduct: Model.Entities.Product;
        private _currentVariant: Model.Entities.ProductDimensionValueSet[];
        private _variantsChosen: number[];

        // Variant Indexes
        private _productIndex: number;
        private _variantIndex: Observable<number>;
        private _variantOptions: ObservableArray<Model.Entities.ProductDimensionValueSet>;
        private _dimensionType: Observable<string>;
        private _selectedDimensionValues: Observable<string>;
        private _showBackButton: Computed<boolean>;

        constructor() {
            super();

            // Local variables for tracking the proccessed product variants
            this._productIndex = 0;
            this._currentVariant = [];
            this._variantsChosen = [];

            // Observables for diplaying variant properties on the variant control
            this._variantOptions = ko.observableArray([]);
            this._dimensionType = ko.observable("");
            this._variantIndex = ko.observable(0);
            this._selectedDimensionValues = ko.observable("");
            this._showBackButton = ko.computed(() => { return this._variantIndex() > 0; });
        }

        /**
         * Shows the modal dialog.
         *
         * @param {VariantDialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: VariantDialogState) {
            this._productIndex = 0;
            this._variantIndex(0);
            this._variantsChosen = [];
            this._currentVariant = [];
            this._selectedDimensionValues("");

            if (dialogState && ArrayExtensions.hasElements(dialogState.products)) {
                this._selectedProducts = dialogState.products;
                this.indeterminateWaitVisible(true);
                this._currentProduct = this._selectedProducts[this._productIndex];
                this.handleProductTypes();
            }
        }

        /**
         * Back button handler on variant control
         */
        private reselectPreviousVariant() {
            if (this._variantIndex() > 0) {
                this._currentVariant.pop();
                this._variantIndex(this._variantIndex() - 1);
                this._selectedDimensionValues(this._currentVariant.map(d => d.DimensionValue).join(", "));
                this.showAllVariants(this._variantIndex());
            }
        }

        /**
         * Calculcates and displays the next set of variants to displayed, depending upon
         * the variants already selected
         */
        private showAllVariants(dimensionIndex: any) {
            this.visible(true);

            this.title(Commerce.ViewModelAdapter.getResourceString('string_923'));
            this.subTitle(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_929"), this._currentProduct.ItemId, this._currentProduct.ProductName));

            this.indeterminateWaitVisible(false);
            this._dimensionType(ProductPropertiesHelper.getDimensionTypeString(this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionKey.toUpperCase()));

            if (dimensionIndex == 0) {
                this._variantOptions(this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues);
            } else {
                var dimensionValues = [];
                for (var i = 0; i < this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues.length; i++) {
                    var tempVariantSet: number[] = [];
                    for (var j = 0; j < this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues[i].VariantSet.length; j++) {

                        for (var k = 0; k < this._currentVariant[this._variantIndex() - 1].VariantSet.length; k++) {
                            if (this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues[i].VariantSet[j] == this._currentVariant[this._variantIndex() - 1].VariantSet[k]) {
                                tempVariantSet.push(this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues[i].VariantSet[j]);
                            }
                        }
                    }

                    if (tempVariantSet.length != 0) {
                        dimensionValues.push({
                            DimensionValue: this._currentProduct.CompositionInformation.VariantInformation.Dimensions[dimensionIndex].DimensionValues[i].DimensionValue,
                            VariantSet: tempVariantSet
                        });
                    }
                }

                this._variantOptions(dimensionValues);
            }
        }

        /**
         * Click handler when any of the product variant is selected
         */
        private variantInvokedHandler(event: Commerce.TileList.IItemInvokedArgs): boolean {
            this.indeterminateWaitVisible(true);
            this._currentVariant.push(event.data);
            this._selectedDimensionValues(this._currentVariant.map(d => d.DimensionValue).join(", "));

            if (this._currentProduct.CompositionInformation.VariantInformation.Dimensions.length > this._variantIndex() + 1) {
                this._variantIndex(this._variantIndex() + 1);
                this.showAllVariants(this._variantIndex());
            } else {
                this.localCartHandler(this._currentVariant[this._variantIndex()].VariantSet[0]);
            }

            return true;
        }

        private handleProductTypes() {
            if (this._currentProduct.IsKit) {
                var defaultkitComponentIds: number[] = []
                    this._selectedProducts[this._productIndex].CompositionInformation.KitDefinition.KitLineDefinitions.forEach(
                    function (value) { defaultkitComponentIds.push(value.ComponentProductId) });
                var kitVariant: number[] =
                    ProductPropertiesHelper.getKitVariant(this._currentProduct, defaultkitComponentIds);
                this.localCartHandler(kitVariant[0]);
            } else if (this._currentProduct.IsMasterProduct) {
                this.showAllVariants(this._variantIndex());
            } else {
                this.localCartHandler(this._selectedProducts[this._productIndex].RecordId);
            }
        }

        /**
         * Adds products to local collection listings 
         */
        private localCartHandler(productId: number) {
            this._variantsChosen.push(productId);
            this._productIndex++;

            if (this._productIndex < this._selectedProducts.length) {
                this._variantIndex(0);
                this._currentVariant = [];
                this._selectedDimensionValues("");
                this._currentProduct = this._selectedProducts[this._productIndex];
                this.handleProductTypes();
            } else {
                this.dialogResult.resolve(DialogResult.OK, this._variantsChosen);
            }
        }

        /**
         * Method called upon clicking a button on the dialog.
         */
        private dialogButtonClick(operationId: string) {
            switch (operationId) {
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
                default:
                    throw "Unknown operation Id: " + operationId;
                    break;
            }
        }
    }
}