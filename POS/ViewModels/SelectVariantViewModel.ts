/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Proxy.Entities;

    export interface ISelectVariantViewModelOptions {
        product: Proxy.Entities.SimpleProduct;
    }

    /**
     * Represents the select variant view model.
     */
    export class SelectVariantViewModel extends ViewModelBase {
        // Internal state
        private _productDimensionIndex: number;
        private _product: Proxy.Entities.SimpleProduct;
        private _selectedValues: Proxy.Entities.ProductDimension[];
        private _selectionOptionsByIndex: Dictionary<Proxy.Entities.ProductDimensionValue[]>;

        // Binding Properties
        private _selectionOptions: ObservableArray<Proxy.Entities.ProductDimensionValue>;
        private _selectionType: Observable<string>;
        private _selectedValuesText: Observable<string>;
        private _allowUndoPreviousSelection: Observable<boolean>;

        constructor() {
            super();

            this._productDimensionIndex = 0;
            this._selectedValues = [];
            this._selectionOptionsByIndex = new Dictionary<Proxy.Entities.ProductDimensionValue[]>();
            this._selectionOptions = ko.observableArray([]);
            this._selectionType = ko.observable(StringExtensions.EMPTY);
            this._selectedValuesText = ko.observable(StringExtensions.EMPTY);
            this._allowUndoPreviousSelection = ko.observable(false);
        }

        /**
         * Initializes the view model state.
         * @param {ISelectVariantDimensionsViewModelOptions} options The values used to initialize the state.
         * @return {IVoidAsyncResult} The async result.
         */
        public load(options: ISelectVariantViewModelOptions): IVoidAsyncResult {
            this._product = options.product;
            return this.getDimensionValues();
        }

        /**
         * Gets the dimensions that have been selected.
         */
        public get SelectedDimensions(): Proxy.Entities.ProductDimension[] {
            return this._selectedValues;
        }

        /**
         * Updates the view model based on the selection, and indicates if this was the last selection required.
         * @param {Proxy.Entities.ProductDimensionValue} value The selected value.
         * @return {IAsyncResult<boolean>} The async result indicating if selection is complete.  True if complete, false otherwise.
         */
        public makeSelection(value: Proxy.Entities.ProductDimensionValue): IAsyncResult<boolean> {
            var dimension: Proxy.Entities.ProductDimension = this._product.Dimensions[this._selectedValues.length];
            dimension.DimensionValue = value;
            this._selectedValues.push(dimension);
            ++this._productDimensionIndex;

            var result: IAsyncResult<boolean>;
            if (this.isSelectionComplete()) {
                result = AsyncResult.createResolved<boolean>(true);
            } else {
                result = this.getDimensionValues().map((): boolean => { return false; });
            }

            return result;
        }

        /**
         * Clears the previous selection.
         */
        public undoPreviousSelection(): void {
            if (ArrayExtensions.hasElements(this._selectedValues)) {
                this._selectedValues.pop();
                --this._productDimensionIndex;
                this.update();
            }
        }

        /**
         * Determines if all the required dimensions have a value selected.
         * @return {boolean} True if all the dimensions have a value selected, false otherwise.
         */
        private isSelectionComplete(): boolean {
            return this._selectedValues.length === this._product.Dimensions.length;
        }

        /**
         * Retrieves the dimension value options for the current index and updates the view model state.
         * @return {IVoidAsyncResult} The async result.
         */
        private getDimensionValues(): IVoidAsyncResult {
            var channelId: number = Session.instance.productCatalogStore.Context.ChannelId;
            return this.productManager.getDimensionValuesAsync(
                this._product.RecordId,
                channelId,
                this._product.Dimensions[this._productDimensionIndex].DimensionTypeValue,
                this._selectedValues).done((dimensionValues: Entities.ProductDimensionValue[]): void => {
                    this._selectionOptionsByIndex.setItem(this._productDimensionIndex, dimensionValues);
                    this.update();
                });
        }

        /**
         * Updates the data binding properties based on the state of the view model.
         */
        private update(): void {
            this._selectionType(ProductPropertiesHelper.getDimensionTypeLabel(this._product.Dimensions[this._productDimensionIndex].DimensionTypeValue));
            this._selectionOptions(this._selectionOptionsByIndex.getItem(this._productDimensionIndex));
            this._selectedValuesText(SimpleProductHelper.getProductDimensionsDescription(this._selectedValues));
            this._allowUndoPreviousSelection(ArrayExtensions.hasElements(this._selectedValues));
        }
    }
}