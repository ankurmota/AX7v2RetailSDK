/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='./ProductDetailsView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * ProductsViewController contructor options.
     */
    export interface IProductsViewOptions {
        category: Proxy.Entities.Category;
        activeMode: ViewModels.ProductsViewModelActiveMode;
    }

    export class ProductsViewController extends ViewControllerBase {

        public viewModel: Commerce.ViewModels.ProductsViewModel;
        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public toggleShowHideMenu: Observable<(() => void)>;

        private _productsListView: HTMLElement;

        /*
         * constructor
         * @param {any} options Setup data for the view
         */
        constructor(options: IProductsViewOptions) {
            super(true);

            options = options || {
                activeMode: ViewModels.ProductsViewModelActiveMode.Categories,
                category: {
                    RecordId: 0
                }
            };

            var viewModelOptions: ViewModels.IProductsViewModelOptions = {
                activeMode: options.activeMode,
                category: options.category,
                showProductDetailsHandler: this.showProductDetails.bind(this),
                showProductsForCategoryHandler: this.showProductsForCategory.bind(this),
                compareItemsHandler: this.compareItems.bind(this),
                showCartHandler: this.showCart.bind(this),
                resetProductsHandler: this.resetProducts.bind(this)
            };

            // All code except related to options and navigation should exist in view model.
            this.viewModel = new ViewModels.ProductsViewModel(viewModelOptions);

            this.viewModel.canRefineItems.subscribe((newValue: boolean) => this.commonHeaderData.enableFilterButton(newValue));
            this.toggleShowHideMenu = ko.observable(() => { });

            //Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_609")); //PRODUCTS
            this.commonHeaderData.enableVirtualCatalogHeader();
            this.commonHeaderData.categoryName(Commerce.Formatters.CategoryNameTranslator(options.category));

            this.commonHeaderData.applyRefinerHandler = this.viewModel.applyRefinerValues.bind(this.viewModel);
            this.commonHeaderData.getRefinersHandler = this.viewModel.getRefiners.bind(this.viewModel);
            this.commonHeaderData.getRefinerValuesHandler = this.viewModel.getRefinerValues.bind(this.viewModel);

            if (options.activeMode === Commerce.ViewModels.ProductsViewModelActiveMode.Products) {
                if (ObjectExtensions.isFunction(this.commonHeaderData.refinerProductSearchDetailsChangedHandler)) {
                    this.commonHeaderData.refinerProductSearchDetailsChangedHandler();
                };
            }
        }

        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement): void {
            super.onCreated(element);

            var matchedElements = $(element).find("#productsview");
            this._productsListView = matchedElements[0];
        }

        /* 
         *  Resets products.
         */
        private resetProducts(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._productsListView)) {
                ko.applyBindingsToNode(this._productsListView, null, this.viewModel);
            }
        }

        /* 
         *   Switches view between templates
         */
        private showViewMenu(): void {
            this.toggleShowHideMenu()();
        }

        /**
         * Show product details from the specified product identifier.
         * @param {{ RecordId: number, IsKit?: boolean }} product The product.
         */
        private showProductDetails(product: { RecordId: number, IsKit?: boolean }): void {
            if (!ObjectExtensions.isNullOrUndefined(product)) {
                var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                productDetailOptions.recordId = product.RecordId;
                productDetailOptions.additionalParams = {
                    IsKit: product.IsKit
                };
                Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
            }
        }

        /**
         * Show products for the specified category.
         * @param {ViewModels.ProductsViewModelActiveMode} activeMode The active mode.
         * @param {Entities.Category} category The category.
         */
        private showProductsForCategory(activeMode: ViewModels.ProductsViewModelActiveMode, category: Proxy.Entities.Category): void {
            Commerce.ViewModelAdapter.navigate(
                "ProductsView",
                <IProductsViewOptions>{ category: category, activeMode: ViewModels.ProductsViewModelActiveMode.Products });
        }

        /**
         * Compare items from the specified product identifiers.
         * @param {string} categoryName The category name.
         * @param {number[]} productIds The product identifiers.
         */
        private compareItems(categoryName: string, productIds: number[]): void {
            Commerce.ViewModelAdapter.navigate("CompareProductsView", { categoryName: categoryName, items: productIds });
        }

        /**
         * Navigates to cart page.
         */
        private showCart(): void {
            Commerce.ViewModelAdapter.navigate("CartView");
        }
    }
}