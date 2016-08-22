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

    /**
     * Represents products view model constructor options.
     */
    export interface IProductsViewModelOptions {
        activeMode: ProductsViewModelActiveMode;
        category: Proxy.Entities.Category;
        resetProductsHandler: (() => void);
        showProductDetailsHandler: ((product: { RecordId: number, IsKit?: boolean }) => void);
        showProductsForCategoryHandler: ((activeMode: ProductsViewModelActiveMode, category: Proxy.Entities.Category) => void);
        compareItemsHandler: ((categoryName: string, productIds: number[]) => void);
        showCartHandler: (() => void);
    }

    /**
     * Represents products view model active mode.
     */
    export enum ProductsViewModelActiveMode {
        None = 0,
        Products = 1,
        Categories = 2
    }

    /**
     * Represents the products view model.
     */
    export class ProductsViewModel extends ViewModelBase {
        public canCompareSelectedItems: Observable<boolean>;
        public canAddSelectedItemsToSale: Observable<boolean>;
        public areSubcategoriesEmpty: Observable<boolean>;
        public canRefineItems: Observable<boolean>;
        public isProductsModeBusy: Observable<boolean>;
        public isCategoryModeBusy: Observable<boolean>;
        public isProductsModeActive: Computed<boolean>;
        public isCategoriesModeActive: Computed<boolean>;

        public productsSubHeader: Observable<string>;
        public clearProductSelection: Observable<boolean>
        public activeMode: Observable<number>;
        public categories: ObservableArray<Entities.Category>;

        private _resetProductsHandler: (() => void);
        private _showProductDetailsHandler: ((product: { RecordId: number, IsKit?: boolean }) => void);
        private _showProductsForCategoryHandler: ((activeMode: ProductsViewModelActiveMode, category: Proxy.Entities.Category) => void);
        private _compareItemsHandler: ((categoryName: string, productIds: number[]) => void);
        private _showCartHandler: (() => void);

        private _selectedProductSearchResults: Entities.ProductSearchResult[];
        private _category: Entities.Category;
        private _productRefinerValues: Entities.ProductRefinerValue[];
        private _hasProductSearchResults: Observable<boolean>;

        constructor(options: IProductsViewModelOptions) {
            super();

            this.validateOptions(options);

            this._category = options.category;
            this._resetProductsHandler = options.resetProductsHandler;
            this._showProductDetailsHandler = options.showProductDetailsHandler;
            this._showProductsForCategoryHandler = options.showProductsForCategoryHandler;
            this._compareItemsHandler = options.compareItemsHandler;
            this._showCartHandler = options.showCartHandler;
            this._hasProductSearchResults = ko.observable(false);
            this._hasProductSearchResults.subscribe((): void => this.resetRefineAvailability());

            this.isProductsModeBusy = ko.observable(false);
            this.isCategoryModeBusy = ko.observable(false);
            this.canRefineItems = ko.observable(false);
            this.areSubcategoriesEmpty = ko.observable(false);
            this.canCompareSelectedItems = ko.observable(false);
            this.canAddSelectedItemsToSale = ko.observable(false);

            this.productsSubHeader = ko.observable(Commerce.ViewModelAdapter.getResourceString("string_919"));
            this.activeMode = ko.observable(options.activeMode);
            this.isProductsModeActive = ko.computed(() => this.activeMode() == Commerce.ViewModels.ProductsViewModelActiveMode.Products);
            this.isCategoriesModeActive = ko.computed(() => this.activeMode() == Commerce.ViewModels.ProductsViewModelActiveMode.Categories);
            this.categories = ko.observableArray(<Commerce.Proxy.Entities.Category[]>[]);
            var clearProductSelectionExtender: ObservableExtender = { notify: "always" };
            this.clearProductSelection = ko.observable(false);
            this.clearProductSelection.extend(clearProductSelectionExtender);

            if (this.activeMode() == Commerce.ViewModels.ProductsViewModelActiveMode.Categories) {
                this.switchToCategoriesMode();
            } else {
                this.isProductsModeBusy(true);
                this.resetRefineAvailability();
            }
        }

        /**
         * Add selected items to cart.
         */
        public addSelectedItemsToCart(): IAsyncResult<Operations.IOperationResult> {
            return this.addProductsToCartAsync(false);
        }

        /**
         * Quick sell selected items.
         */
        public quickSellSelectedItems(): IAsyncResult<Operations.IOperationResult> {
            return this.addProductsToCartAsync(true);
        }

        /**
         * Show product details from the specified product search result.
         * @param {Entities.ProductSearchResult} productSearchResult The product search result.
         */
        public showProductDetails(productSearchResult: Entities.ProductSearchResult): void {
            if (!ObjectExtensions.isNullOrUndefined(productSearchResult)) {
                this._showProductDetailsHandler(productSearchResult);
            }
        }

        /**
         * Show products for the specified category.
         * @param {Entities.Category} category The category.
         */
        public showProductsForCategory(category: Entities.Category): void {
            this._showProductsForCategoryHandler(this.activeMode(), category);
        }

        /**
         * Sets the selected items upon which other actions can be performed.
         * @param {Proxy.Entities.ProductSearchResult[]} items The category.
         */
        public setSelectedItems(items: Proxy.Entities.ProductSearchResult[]): void {
            this._selectedProductSearchResults = ArrayExtensions.hasElements(items) ? items : [];
            var numItemsSelected = this._selectedProductSearchResults.length;

            // Enable or disable available commands that are bound to the following members.
            this.canCompareSelectedItems(numItemsSelected === 2 || numItemsSelected === 3);
            this.canAddSelectedItemsToSale(numItemsSelected > 0);
        }

        /**
         * Compare selected items.
         */
        public compareSelectedItems(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._category)) {
                var selectedProductIds: number[] = [];
                for (var i = 0; i < this._selectedProductSearchResults.length; i++) {
                    selectedProductIds.push(this._selectedProductSearchResults[i].RecordId);
                }

                this._compareItemsHandler(this._category.Name, selectedProductIds);
            }
        }

        /**
         * Switch to products mode.
         */
        public switchToProductsMode(): void {
            this.activeMode(Commerce.ViewModels.ProductsViewModelActiveMode.Products);
            if (ObjectExtensions.isNullOrUndefined(this._category)) {
                this.isProductsModeBusy(true);
                this._resetProductsHandler();
            }
            this.resetRefineAvailability();
        }

        /**
         * Switch to categories mode.
         */
        public switchToCategoriesMode(): void {
            this.activeMode(Commerce.ViewModels.ProductsViewModelActiveMode.Categories);
            if (!ArrayExtensions.hasElements(this.categories())) {
                this.areSubcategoriesEmpty(false);
                this.isCategoryModeBusy(true);
                this.updateCategoriesAsync()
                    .done(() => {
                        this.areSubcategoriesEmpty(!ArrayExtensions.hasElements(this.categories()));
                        this.isCategoryModeBusy(false);
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                        this.isCategoryModeBusy(false);
                    });
            }
            this.resetRefineAvailability();
        }

        /**
         * Searches for products matching category and refiners (is set).
         * @param {number} pageSize The number of products to retrieve.
         * @param {number} skip The number of products to skip.
         * @return {IAsyncResult<Proxy.Entities.Product[]>} The async result containing the products.
         */
        public searchProducts(pageSize: number, skip: number): IAsyncResult<Entities.ProductSearchResult[]> {
            var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
            var catalogId: number = Commerce.Session.instance.productCatalogStore.Context.CatalogId;
            var categoryId: number = this.getCurrentCategoryId();
            var searchResults: Entities.ProductSearchResult[] = [];
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue((): IAsyncResult<Entities.ProductSearchResult[]> => {
                var asyncSearchResults: IAsyncResult<Entities.ProductSearchResult[]>;

                if (!ArrayExtensions.hasElements(this._productRefinerValues)) {
                    asyncSearchResults = this.productManager.searchByCategoryAsync(
                        categoryId,
                        channelId,
                        catalogId,
                        pageSize,
                        skip);
                }
                else {
                    asyncSearchResults = this.productManager.refineSearchByCategoryAsync(
                        categoryId,
                        this._productRefinerValues,
                        channelId,
                        catalogId,
                        pageSize,
                        skip);
                }

                return asyncSearchResults.done((results: Entities.ProductSearchResult[]): void => {
                    // The price for ProductSearchResult is being deprecated and it will be later populated by GetActivePrice.
                    // It's set to null so it won't show at the product search result grid.
                    results.forEach((result: Entities.ProductSearchResult) => { result.Price = null; });
                    searchResults = results;
                });
            });

            return asyncQueue.run()
                .map(() => {
                    // If no skip is sepcified, then it is the first page requested. So, in the first page, if there are no results, then there won't be any results at all.
                    if (NumberExtensions.isNullOrZero(skip)) {
                        this._hasProductSearchResults(ArrayExtensions.hasElements(searchResults) || ArrayExtensions.hasElements(this._productRefinerValues));
                    }
                    return searchResults;
                })
                .fail((errors: Entities.Error[]): void => {
                    RetailLogger.viewsModelProductsViewModelSearchProductsByCategoryFailed(categoryId, JSON.stringify(this._productRefinerValues), JSON.stringify(errors));
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Get product refiners using the selected channel/catalog.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        public getRefiners(): IAsyncResult<Entities.ProductRefiner[]> {
            //  Refiners are available only for current store.
            if ((Commerce.ApplicationContext.Instance.storeInformation.RecordId == Commerce.Session.instance.productCatalogStore.Store.RecordId)) {

                var categoryId: number = this.getCurrentCategoryId();
                return this.productManager.getRefinersByCategoryAsync(categoryId, Session.instance.productCatalogStore.Context.CatalogId)
                    .fail((errors: Proxy.Entities.Error[]): void => {
                        RetailLogger.viewsModelProductsViewModelGetRefinersByCategoryFailed(categoryId, JSON.stringify(errors));
                        NotificationHandler.displayClientErrors(errors);
                    });
            }

            return AsyncResult.createResolved([]);
        }

        /**
         * Get product refiner values for the specified refiner.
         * @param {Entities.ProductRefiner[]} refiner The refiner.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        public getRefinerValues(productRefiner: Entities.ProductRefiner): IAsyncResult<Entities.ProductRefinerValue[]> {
            if (ObjectExtensions.isNullOrUndefined(productRefiner)) {
                return AsyncResult.createResolved([]);
            }

            var categoryId = this.getCurrentCategoryId();
            return this.productManager.getRefinerValuesByCategoryAsync(
                categoryId,
                productRefiner.RecordId,
                productRefiner.SourceValue,
                Session.instance.productCatalogStore.Context.CatalogId)
                .fail((errors: Proxy.Entities.Error[]): void => {
                    RetailLogger.viewsModelProductsViewModelGetRefinerValuesByCategoryFailed(categoryId, productRefiner.RecordId, productRefiner.SourceValue, JSON.stringify(errors));
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Applies the spcified refiner values and searches for products.
         * @param {Entities.ProductRefinerValue[]} productRefinerValues The refiner values.
         */
        public applyRefinerValues(productRefinerValues: Entities.ProductRefinerValue[]): void {
            this._productRefinerValues = productRefinerValues;
            this.isProductsModeBusy(true);
            this._resetProductsHandler();

            if (ArrayExtensions.hasElements(productRefinerValues)) {
                this.productsSubHeader(Commerce.ViewModelAdapter.getResourceString("string_921")); // Filtered products
            } else {
                this.productsSubHeader(Commerce.ViewModelAdapter.getResourceString("string_919")); // Products
            }
        }

        /**
         * Validates options and throws appropriate errors if invalid.
         * @param {IProductsViewModelOptions} options The option.
         */
        private validateOptions(options: IProductsViewModelOptions): void {
            if (ObjectExtensions.isNullOrUndefined(options)) {
                throw Error("'options' is required for ProductsViewModel");
            }

            if (ObjectExtensions.isNullOrUndefined(options.activeMode)) {
                throw Error("'options.activeMode' is required for ProductsViewModel");
            }

            if (ObjectExtensions.isNullOrUndefined(options.category)) {
                throw Error("'options.category' is required for ProductsViewModel");
            }

            if (!ObjectExtensions.isFunction(options.resetProductsHandler)) {
                throw Error("'options.resetProductsHandler' must be a function.");
            }

            if (!ObjectExtensions.isFunction(options.showProductDetailsHandler)) {
                throw Error("'options.showProductDetailsHandler' must be a function.");
            }

            if (!ObjectExtensions.isFunction(options.showProductsForCategoryHandler)) {
                throw Error("'options.showProductsForCategoryHandler' must be a function.");
            }

            if (!ObjectExtensions.isFunction(options.compareItemsHandler)) {
                throw Error("'options.compareItemsHandler' must be a function.");
            }

            if (!ObjectExtensions.isFunction(options.showCartHandler)) {
                throw Error("'options.showCartHandler' must be a function.");
            }
        }

        /**
         * Updates current categories from Session's cache.
         * @return {IVoidAsyncResult} The async void result.
         */
        private updateCategoriesAsync(): IVoidAsyncResult {
            if (Session.instance.productCatalogStore.Context.ChannelId == Commerce.ApplicationContext.Instance.storeInformation.RecordId) {
                var asyncResult = new VoidAsyncResult();
                var cachedCategories: Commerce.Proxy.Entities.Category[] = [];

                // The CurrentCategoryList for the current store is refreshed at each logon and per business specs this is enough.

                if (ArrayExtensions.hasElements(Session.instance.CurrentCategoryList)) {
                    cachedCategories = Commerce.Session.instance.CurrentCategoryList.filter((item) => {
                        return item.ParentCategory === this.getCurrentCategoryId();
                    });
                }

                this.categories(cachedCategories);
                asyncResult.resolve();

                return asyncResult;
            } else {
                var result = this.productManager.getChildCategoriesAsync(
                    Session.instance.productCatalogStore.Context.ChannelId, this.getCurrentCategoryId());

                return result.done((categories) => {
                    this.categories(categories);
                });
            }
        }

        /**
         * Adds the selected products to cart.
         * @param {boolean} isQuickSale Whether this is for quick sale.
         * @return {IAsyncResult<Operations.IOperationResult>} The async operation result.
         */
        private addProductsToCartAsync(isQuickSale: boolean): IAsyncResult<Operations.IOperationResult> {
            if (!ArrayExtensions.hasElements(this._selectedProductSearchResults)) {
                RetailLogger.viewModelProductsViewModelAddItemsToCart(StringExtensions.EMPTY, isQuickSale);
                return VoidAsyncResult.createResolved();
            }

            // Show the spinner and disable the add to sale buttons.
            this.isProductsModeBusy(true);
            this.canAddSelectedItemsToSale(false);

            var asyncQueue: AsyncQueue = new AsyncQueue();
            var selectedProducts: Entities.SimpleProduct[];

            asyncQueue.enqueue((): IAsyncResult<Entities.SimpleProduct[]> => {
                return this.getProductsForSelectedItemsAsync()
                    .done((products: Entities.SimpleProduct[]) => {
                        selectedProducts = products;
                    });
            });

            // If only one item is selected, then we have to determine if it is a kit
            // If it is a kit, then we have to navigate to show details otherwise just add items to cart.
            if (this._selectedProductSearchResults.length === 1) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var product: Entities.SimpleProduct = selectedProducts[0];
                    if (product.ProductTypeValue === Entities.ProductType.KitMaster) {
                        this._selectedProductSearchResults = [];
                        this.clearProductSelection(true);
                        this.isProductsModeBusy(false);
                        this.canAddSelectedItemsToSale(true);
                        this._showProductDetailsHandler({ RecordId: product.RecordId, IsKit: true });
                        asyncQueue.cancel();
                    }

                    return VoidAsyncResult.createResolved();
                });
            }

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var options: Operations.IItemSaleOperationOptions = {
                    productSaleDetails: selectedProducts.map((product) => {
                        return <Proxy.Entities.ProductSaleReturnDetails>{ product: product, quantity: 0 };
                    })
                };

                RetailLogger.viewModelProductsViewModelAddItemsToCart(JSON.stringify(options), isQuickSale);

                return Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.ItemSale, options)
                    .done((result) => {
                        this.isProductsModeBusy(false);

                        if (!result.canceled) {
                            if (isQuickSale) {
                                this._showCartHandler();
                            } else {
                                this._selectedProductSearchResults = [];
                                this.clearProductSelection(true);
                            }
                        } else {
                            this.canAddSelectedItemsToSale(true);
                        }
                    });
            });

            var result: IAsyncResult<ICancelableResult> = asyncQueue.run().fail((errors) => {
                Commerce.NotificationHandler.displayClientErrors(errors)
                    .always((): void => {
                        this.isProductsModeBusy(false);
                        this.canAddSelectedItemsToSale(true);
                    });
            })

            return asyncQueue.cancelOn(result);
        }

        /**
         * Gets product details from the selected items.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        private getProductDetailsForSelectedItemsAsync(): IAsyncResult<Entities.Product[]> {
            var productIds: number[] = this._selectedProductSearchResults.map((productSearchResult: Entities.ProductSearchResult) => productSearchResult.RecordId);
            var productSearchCriteria: Entities.ProductSearchCriteria = new Entities.ProductSearchCriteriaClass({
                Ids: productIds,
                Context: Commerce.Session.instance.productCatalogStore.Context,
                IsOnline: Commerce.Session.instance.connectionStatus === Commerce.ConnectionStatusType.Online,
                SkipVariantExpansion: true,
                DataLevelValue: 1
            });
            return this.productManager.getProductDetailsBySearchCriteriaAsync(productSearchCriteria)
                .fail((errors: Entities.Error[]) => {
                    RetailLogger.viewModelProductsViewModelGetProductDetailsFailed(JSON.stringify(productSearchCriteria), JSON.stringify(errors));
                });
        }

        /**
         * Gets products from the selected items.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result.
         */
        private getProductsForSelectedItemsAsync(): IAsyncResult<Entities.SimpleProduct[]> {
            var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
            var productIds: number[] = this._selectedProductSearchResults.map((productSearchResult: Entities.ProductSearchResult) => productSearchResult.RecordId);

            if (productIds.length > 1) {
                return this.productManager.getByIdsAsync(productIds, channelId);
            } else {
                return this.productManager.getByIdAsync(productIds[0], channelId).map((product: Entities.SimpleProduct): Entities.SimpleProduct[]=> {
                    return [product];
                });
            }
        }

        /**
         * Resets status if refiners are available.
         */
        private resetRefineAvailability(): void {
            if (this.activeMode() === ProductsViewModelActiveMode.Categories) {
                this.canRefineItems(false);
            } else {
                this.canRefineItems(this._hasProductSearchResults());
            }
        }
        
        /**
         * If current category exists, returns its record identifier, else 0.
         * @return {number} The current record identifier if exists, else 0.
         */
        private getCurrentCategoryId(): number {
            return ObjectExtensions.isNullOrUndefined(this._category) ? 0 : this._category.RecordId;
        }
    }
}