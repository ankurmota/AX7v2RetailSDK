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
     * Represents product add mode.
     */
    export enum ProductAddModeEnum {
        None = 0,
        AddToCart = 1,
        InventoryLookup = 2,
        StockCount = 3,
        PriceCheck = 4,
        PickingAndReceiving = 5,
        KitDisassembly = 6,
        AddToReasonCode = 8
    }

    /**
     * Represents customer add mode.
     */
    export enum CustomerAddModeEnum {
        None = 0,
        AddToCart = 1,
        AddToPriceCheck = 2,
        IssueLoyaltyCard = 3,
        JournalSearch = 4,
        AddToReasonCode = 5
    }

    /**
     * Interface for search location.
     * 
     */
    export interface ISearchLocation {
        name: string;
        value: Entities.SearchLocation;
    }

    /**
     * Represents the products view model.
     */
    export class SearchViewModel extends ViewModelBase {
        public static productSearchEntity: string = "Products";
        public static customerSearchEntity: string = "Customers";
        public viewRelatedProducts: Observable<boolean>;
        public viewCustomers: Observable<boolean>;
        public searchText: string;
        public selectedSearchLocation: Entities.SearchLocation;
        public clearProductSelection: Observable<boolean>;

        // Busy states
        public isProductsModeBusy: Observable<boolean>;
        public isCustomersModeBusy: Observable<boolean>;
        public isRelatedProductsModeBusy: Observable<boolean>;

        // Action and functional states
        public canCompareSelectedItems: Observable<boolean>;
        public canSelectItem: Observable<boolean>;
        public isAddNewCustomerAvailable: Computed<boolean>;
        public isAddSelectedCustomerToCartAvailable: Computed<boolean>;
        public isSearchWithLocationAvailable: Observable<boolean>;
        public canRefineItems: Observable<boolean>;
        public isProductAddModeAddToCart: Computed<boolean>;
        public areAnyItemsSelected: Computed<boolean>;
        public isItemSelectActionAvailable: Computed<boolean>;
        public isCustomerSelectActionAvailable: Computed<boolean>;
        public isSingleCustomerSelected: Computed<boolean>;

        // Handlers to reset results
        public resetProductsHandler: (() => void);
        public resetCustomersHandler: (() => void);
        public refinerProductSearchDetailsChangedHandler: (() => void);

        public showProductDetailsHandler: (
            (product: { RecordId: number, IsKit?: boolean },
                productAddModeEnum?: ViewModels.ProductAddModeEnum,
                quantity?: number) => void);
        public changeStoreAndCatalogsHandler: ((searchText: string, searchEntity: string) => void);
        public compareItemsHandler: ((productIds: number[]) => void);
        public showCartHandler: (() => void);
        public onCustomerAddedToCartHandler: (() => void);
        public issueLoyaltyCardHandler: ((customer: Entities.GlobalCustomer) => void);
        public showCustomerDetailsHandler: ((customerAccountNumber: string) => void);
        public sendCustomerToPriceCheckHandler: ((customer: Entities.GlobalCustomer) => void);
        public onReasonCodeResolvedHandler: (() => void);
        public sendItemToCallbackHandler: ((item: Entities.Product) => void);

        private productSearchViewModel: ProductSearchViewModel;
        private customerSearchViewModel: CustomerSearchViewModel;
        private _variantsIds: number[];
        private _customerAddModeEnum: ViewModels.CustomerAddModeEnum;
        private _processingAddItemsToCart: boolean;
        private _searchLocations: { name: string; value: Entities.SearchLocation }[];

        // Reason code result handler for add to reason code modes
        private reasonCodeAsyncResult: AsyncResult<ICancelableDataResult<string>>;

        private _quantity: number; // The quantity of the entity/product. If value is undefined or null, the default quantity for the entity is used.
        private productAddModeObservable: Observable<number>;
        private customerAddModeObservable: Observable<number>;
        private productAddMode: number;
        private customerAddMode: number;
        private selectedProducts: ObservableArray<Entities.Product>;
        private selectedCustomers: ObservableArray<Entities.GlobalCustomer>;

        /*
         * constructor
         *
         * @param {any} options Setup data for the view
         *
         * Supported option properties:
         * {ProductAddModeEnum} [addModeEnum] Adding target for the specified operation
         * {CustomerAddModeEnum} [customerAddModeEnum] Adding customer for the specified operation
         * {string} searchEntity The default search to show
         * {string} [searchText] The default text to search
         * {Entities.Product[]} [relatedItems] The related items to display
         * {number} [quantity] The quantity of the entity. If value is undefined or null, the default quantity for the entity is used.
         *
         * Comment:
         * For some tasks, options is sent as a parameter to the next page in navigation and may contain additional
         * properties not used by this view.
         */
        constructor(options: any) {
            super();

            options = options || {};

            // Initialize view model
            this.productSearchViewModel = new ProductSearchViewModel();
            this.customerSearchViewModel = new CustomerSearchViewModel();

            // selected Elements Array
            this.selectedProducts = ko.observableArray([]);
            this.selectedCustomers = ko.observableArray([]);

            // Intialize busy state to false by default
            this.isProductsModeBusy = ko.observable(false);
            this.isCustomersModeBusy = ko.observable(false);
            this.isRelatedProductsModeBusy = ko.observable(false);

            this.productSearchViewModel.hasProductSearchResults.subscribe((): void => this.resetRefineAvailability(), this);

            // Initialize boolean to not be processing add items to sale.
            this._processingAddItemsToCart = false;

            // Check for product add mode
            if (!ObjectExtensions.isNullOrUndefined(options.addModeEnum)) {
                this.productAddMode = options.addModeEnum;
            } else {
                this.productAddMode = ViewModels.ProductAddModeEnum.AddToCart;
            }

            // Check for customer add mode
            if (!ObjectExtensions.isNullOrUndefined(options.customerAddModeEnum)) {
                this.customerAddMode = options.customerAddModeEnum;
            } else {
                this.customerAddMode = ViewModels.CustomerAddModeEnum.AddToCart;
            }

            this.viewRelatedProducts = ko.observable(false);
            this.canRefineItems = ko.observable(false);

            this.searchText = StringExtensions.isNullOrWhitespace(options.searchText) ? "" : options.searchText;
            // check for search Context
            if (options.searchEntity === SearchViewModel.customerSearchEntity) {
                this.productAddModeObservable = ko.observable(ViewModels.ProductAddModeEnum.None);
                this.customerAddModeObservable = ko.observable(this.customerAddMode);
                this.isSearchWithLocationAvailable = ko.observable(ApplicationContext.Instance.deviceConfiguration.EnableAxCustomerSearch);
                this.viewCustomers = ko.observable(true);
                this.customerSearchViewModel.CustomerSearchCriteria = this.searchText ? { Keyword: this.searchText } : null;
                this.productSearchViewModel.clearParameters();
            } else {

                this.productAddModeObservable = ko.observable(this.productAddMode);
                this.customerAddModeObservable = ko.observable(ViewModels.CustomerAddModeEnum.None);
                this.viewCustomers = ko.observable(false);
                this.isSearchWithLocationAvailable = ko.observable(false);

                if (ArrayExtensions.hasElements(options.relatedItems)) {
                    this.viewRelatedProducts(true);
                    this.products(options.relatedItems);
                } else {
                    this.productSearchViewModel.searchTextParameter = this.searchText;
                    this.searchItems(false);
                }
            }
            this.resetRefineAvailability();

            // Check for quantity
            if (!ObjectExtensions.isNullOrUndefined(options.quantity)) {
                this._quantity = options.quantity;
            }

            this._variantsIds = [];
            var clearProductSelectionExtender: ObservableExtender = { notify: "always" };
            this.clearProductSelection = ko.observable(false);
            this.clearProductSelection.extend(clearProductSelectionExtender);
            this.canCompareSelectedItems = ko.observable(false);
            this.canSelectItem = ko.observable(false);

            if (ObjectExtensions.isNullOrUndefined(options.customerAddModeEnum)) {
                options.customerAddModeEnum = ViewModels.CustomerAddModeEnum.AddToCart;
            }
            this._customerAddModeEnum = options.customerAddModeEnum;

            if (!ObjectExtensions.isNullOrUndefined(options.reasonCodeAsyncResult)) {
                this.reasonCodeAsyncResult = options.reasonCodeAsyncResult;
            }

            this.isAddNewCustomerAvailable = ko.computed(this.getIsAddNewCustomerAvailable, this);
            this.isAddSelectedCustomerToCartAvailable = ko.computed(this.getIsAddSelectedCustomerToCartAvailable, this);
            this.isProductAddModeAddToCart = ko.computed(() => this.productAddModeObservable() === ViewModels.ProductAddModeEnum.AddToCart);
            this.areAnyItemsSelected = ko.computed(() => this.selectedProducts().length !== 0);
            this.isItemSelectActionAvailable = ko.computed(() => this.productAddModeObservable() >= ViewModels.ProductAddModeEnum.InventoryLookup);
            this.isCustomerSelectActionAvailable =
            ko.computed(() => this.customerAddModeObservable() >= ViewModels.CustomerAddModeEnum.AddToPriceCheck);
            this.isSingleCustomerSelected = ko.computed(() => this.selectedCustomers().length === 1);
        }

        /**
         * Handler to get product refiners using current product search details.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        public get getRefinersHandler(): (() => IAsyncResult<Entities.ProductRefiner[]>) {
            return this.productSearchViewModel.getRefiners.bind(this.productSearchViewModel);
        }

        /**
         * Handler to get product refiner values for the specified refiner using the current product search details.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        public get getRefinerValuesHandler(): ((productRefiner: Entities.ProductRefiner) => IAsyncResult<Entities.ProductRefinerValue[]>) {
            return this.productSearchViewModel.getRefinerValues.bind(this.productSearchViewModel);
        }

        /**
         * Customer search locations.
         * @return {ViewModels.ISearchLocation[]} The search locations.
         */
        public get searchLocations(): ViewModels.ISearchLocation[] {
            if (ObjectExtensions.isNullOrUndefined(this._searchLocations)) {
                this._searchLocations = [
                    {
                        name: ViewModelAdapter.getResourceString("string_1040"), // "Search this store"
                        value: Entities.SearchLocation.Local
                    },
                    {
                        name: ViewModelAdapter.getResourceString("string_1041"), // "Search everywhere"
                        value: Entities.SearchLocation.Remote
                    }
                ];
            }

            return this._searchLocations;
        }

        /**
         * Gets the products.
         * @return {ObservableArray<Entities.Product>} The products.
         */
        public get products(): ObservableArray<Entities.Product> {
            return this.productSearchViewModel.products;
        }

        /**
         * Searches and returns product search results matching search text and refiners (if set).
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        public searchProducts(pageSize: number, skip: number): IAsyncResult<Entities.ProductSearchResult[]> {
            return this.productSearchViewModel.searchProducts(pageSize, skip);
        }

        /**
         * Searches for customers.
         * @param {number} pageSize The number of records per page.
         * @param {number} skip The number of customers to skip.
         * @return {IAsyncResult<Proxy.Entities.Customer[]>} The async result containing the customers that match the search.
         */
        public searchCustomers(pageSize: number, skip: number): IAsyncResult<Proxy.Entities.Customer[]> {
            return this.customerSearchViewModel.searchCustomers(pageSize, skip);
        }

        /**
         * Applies the spcified refiner values and searches for products.
         * @param {Entities.ProductRefinerValue[]} productRefinerValues The refiner values.
         */
        public applyRefinerValues(productRefinerValues: Entities.ProductRefinerValue[]): void {
            this.isProductsModeBusy(true);
            this.productSearchViewModel.productRefinerValuesParameter = productRefinerValues;
            this.resetProducts();
        }

        /**
         * Shows product details for the specified product.
         * @param {{ RecordId: number, IsKit?: boolean }} product The product.
         */
        /**
         * Shows product details for the specified product.
         * @param {{ RecordId: number, IsKit?: boolean }} product The product.
         * @param {ProductAddModeEnum} productAddMode The product add mode.
         * @param {number} quantity? Quantity.
         */
        public showProductDetails(product: { RecordId: number, IsKit?: boolean }, productAddMode: ProductAddModeEnum, quantity?: number): void {
            if (ObjectExtensions.isFunction(this.showProductDetailsHandler)) {
                this.showProductDetailsHandler(product, productAddMode, quantity);
            }
        }

        /**
         * Sets the selected items upon which other actions can be performed.
         * @param {Entities.Product} items The items.
         */
        public setSelectedItems(items: Entities.Product[]): void {
            this.selectedProducts(items);
            this.canCompareSelectedItems(this.selectedProducts().length >= 2 && this.selectedProducts().length <= 3);
            this.canSelectItem(!(this.selectedProducts().length !== 1 || (
                this.selectedProducts().length === 1 &&
                this.productAddMode === ViewModels.ProductAddModeEnum.KitDisassembly &&
                !this.selectedProducts()[0].IsKit)));
        }

        /**
         * Quick sell selected items.
         */
        public quickSellSelectedItems(): IAsyncResult<Operations.IOperationResult> {
            return this.addItemsToCart(true);
        }

        /**
         * Add selected items to cart.
         */
        public addSelectedItemsToCart(): IAsyncResult<Operations.IOperationResult> {
            return this.addItemsToCart(false);
        }

        /**
         * Set the enviroment needed to search items and the optionally performs search based on the specified update flag.
         * @param {boolean} updateSearchResults The flag whether to update search results.
         */
        public searchItems(updateSearchResults: boolean): void {
            this.viewRelatedProducts(false);
            if (!StringExtensions.isNullOrWhitespace(this.searchText)) {
                if (!this.viewCustomers()) {
                    this.isProductsModeBusy(true);
                    this.isRelatedProductsModeBusy(false);
                    this.isCustomersModeBusy(false);
                    this.productSearchViewModel.searchTextParameter = this.searchText;
                    this.productSearchViewModel.productRefinerValuesParameter = null;

                    this.publishRefinerProductSearchDetailsChanged();
                    this.resetRefineAvailability();

                    if (updateSearchResults) {
                        this.resetProducts();
                    }
                } else if (this.viewCustomers()) {
                    this.isProductsModeBusy(false);
                    this.isRelatedProductsModeBusy(false);
                    this.isCustomersModeBusy(true);
                    this.resetRefineAvailability();
                    this.customerSearchViewModel.CustomerSearchCriteria = {
                        Keyword: this.searchText,
                        SearchLocationValue: this.selectedSearchLocation
                    };

                    if (updateSearchResults) {
                        this.resetCustomers();
                    }
                }
            }
        }

        /**
         * Compare selected items.
         */
        public compareSelectedItems(): void {
            // Get the list of selected products ids
            var productIdsSelected: number[] = this.selectedProducts().map((selectedProduct: Entities.Product) => selectedProduct.RecordId);
            if (ObjectExtensions.isFunction(this.compareItemsHandler)) {
                this.compareItemsHandler(productIdsSelected);
            }
        }

        /**
         * Switch to products mode.
         */
        public switchToProductsMode(): void {
            RetailLogger.viewsMerchandisingSearchViewProductButtonClick(this.searchText);
            this.isSearchWithLocationAvailable(false);
            this.viewCustomers(false);
            this.searchItems(true);
            this.productAddModeObservable(this.productAddMode);
            this.selectedProducts([]);
            this.customerAddModeObservable(ViewModels.CustomerAddModeEnum.None);
            this.resetRefineAvailability();
        }

        /**
         * Switch to customers mode.
         */
        public switchToCustomersMode(): void {
            RetailLogger.viewsMerchandisingSearchViewCustomerButtonClick(this.searchText);
            this.isSearchWithLocationAvailable(ApplicationContext.Instance.deviceConfiguration.EnableAxCustomerSearch);
            this.viewCustomers(true);
            this.searchItems(true);
            this.productAddModeObservable(ViewModels.ProductAddModeEnum.None);
            this.selectedCustomers([]);
            this.customerAddModeObservable(this.customerAddMode);
            this.resetRefineAvailability();
        }

        /**
         * Set selected customers.
         */
        public setSelectedCustomers(customers: Entities.GlobalCustomer[]): void {
            this.selectedCustomers(customers);
        }

        /**
         * Add selected customer to cart.
         */
        public addSelectedCustomerToCart(): void {
            this.processAddCustomerToCart([], (customer: Entities.GlobalCustomer) => {
                this.onCustomerAddedToCart();
            });
        }

        /**
         * Add new customer.
         */
        public addNewCustomer(customerAddOperationOptions: Operations.ICustomerAddOperationOptions): void {
            Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.CustomerAdd, customerAddOperationOptions)
                .fail((errors: Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Shows customer details.
         * @param {Entities.GlobalCustomer} globalCustomer The customer whose details to be shown.
         */
        public showCustomerDetails(globalCustomer: Entities.GlobalCustomer): void {
            if (this.customerAddModeObservable() === ViewModels.CustomerAddModeEnum.AddToReasonCode) {
                return; // we do not show details for reason code mode.
            }

            this.customerSearchViewModel.crossCompanyCustomerTransferAsync(globalCustomer)
                .done((customer: Entities.Customer) => {
                    if (!ObjectExtensions.isNullOrUndefined(customer) && ObjectExtensions.isFunction(this.showCustomerDetailsHandler)) {
                        this.showCustomerDetailsHandler(customer.AccountNumber);
                    }
                }).fail((errors: Entities.Error[]) => {
                    this.isCustomersModeBusy(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Navigates to Catalogs View for user to select a different store and Catalog
         */
        public changeStoreAndCatalogs(): void {
            if (ObjectExtensions.isFunction(this.changeStoreAndCatalogsHandler)) {
                this.changeStoreAndCatalogsHandler(this.searchText, SearchViewModel.productSearchEntity);
            }
        }

        /**
         * Performs item select action, which sends the selected item to the page which requested the search.
         */
        public performItemSelectAction(): void {
            switch (this.productAddMode) {
                case ViewModels.ProductAddModeEnum.InventoryLookup:
                case ViewModels.ProductAddModeEnum.StockCount:
                case ViewModels.ProductAddModeEnum.PriceCheck:
                case ViewModels.ProductAddModeEnum.PickingAndReceiving:
                case ViewModels.ProductAddModeEnum.KitDisassembly:
                    this.addProductToCallback();
                    break;
                case ViewModels.ProductAddModeEnum.AddToReasonCode:
                    if (!ObjectExtensions.isNullOrUndefined(this.reasonCodeAsyncResult)) {
                        this.reasonCodeAsyncResult.resolve({ data: this.selectedProducts()[0].ItemId, canceled: false });
                        this.onReasonCodeResolved();
                    }
                    break;
                default:
                    RetailLogger.viewsMerchandisingSearchViewInvalidProductOperation();
                    break;
            }
        }

        /**
         * Performs customer select action, which sends the selected customer to the page which requested the search.
         */
        public performCustomerSelectAction(): void {
            switch (this.customerAddMode) {
                case ViewModels.CustomerAddModeEnum.AddToCart:
                    this.processAddCustomerToCart([], (customer: Entities.GlobalCustomer) => {
                        this.onCustomerAddedToCart();
                    });
                    break;
                case ViewModels.CustomerAddModeEnum.AddToPriceCheck:
                    this.processAddCustomerToCart([], (customer: Entities.GlobalCustomer) => {
                        if (ObjectExtensions.isFunction(this.sendCustomerToPriceCheckHandler)) {
                            this.sendCustomerToPriceCheckHandler(customer);
                        }
                    });
                    break;
                case ViewModels.CustomerAddModeEnum.AddToReasonCode:
                    if (!ObjectExtensions.isNullOrUndefined(this.reasonCodeAsyncResult)) {
                        this.reasonCodeAsyncResult.resolve({ data: this.selectedCustomers()[0].AccountNumber, canceled: false });
                        this.onReasonCodeResolved();
                    }
                    break;
                case ViewModels.CustomerAddModeEnum.IssueLoyaltyCard:
                    this.isCustomersModeBusy(true);
                    this.customerSearchViewModel.crossCompanyCustomerTransferAsync(this.selectedCustomers()[0])
                        .done((customer: Entities.Customer) => {
                            this.isCustomersModeBusy(false);
                            if (ObjectExtensions.isFunction(this.issueLoyaltyCardHandler)) {
                                this.issueLoyaltyCardHandler(customer);
                            }
                        }).fail((errors: Entities.Error[]) => {
                            this.isCustomersModeBusy(false);
                            Commerce.NotificationHandler.displayClientErrors(errors);
                        });
                    break;
                case ViewModels.CustomerAddModeEnum.JournalSearch:
                    break;
                default:
                    RetailLogger.viewsMerchandisingSearchViewInvalidCustomerOperation();
                    break;
            }
        }

        /**
         * Rejects/Cancels any unresolved reason code result (call this during any cleanup process).
         */
        public rejectUnresolvedReasonCodeResult(): void {
            // Reject a reason code result if it exists and it is not resolved.
            if (!ObjectExtensions.isNullOrUndefined(this.reasonCodeAsyncResult) && this.reasonCodeAsyncResult.state() === AsyncResultStateEnum.Pending) {
                this.reasonCodeAsyncResult.resolve({ canceled: true, data: null });
            }
        }

        /**
         * Adds the selected items to cart.
         * @param {boolean} isQuickSale Whether this is for quick sale.
         * @return {IAsyncResult<Operations.IOperationResult>} The async operation result.
         */
        private addItemsToCart(quickSale: boolean): IAsyncResult<Commerce.Operations.IOperationResult> {
            var numberOfSelectedProducts: number = this.selectedProducts().length;

            if (quickSale) {
                RetailLogger.viewsMerchandisingSearchViewQuickSaleClick(numberOfSelectedProducts);
            } else {
                RetailLogger.viewsMerchandisingSearchViewAddToCartClick(numberOfSelectedProducts);
            }

            var productsIndeterminateWaitVisible: Observable<boolean> = this.getCurrentProductsModeBusyTracker();
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var selectedProducts: Entities.SimpleProduct[];

            asyncQueue.enqueue((): IAsyncResult<Entities.SimpleProduct[]> => {
                productsIndeterminateWaitVisible(true);
                return this.getProductsForSelectedItemsAsync()
                    .done((products: Entities.SimpleProduct[]) => {
                        selectedProducts = products;
                    });
            });

            // If only one item is selected, then we have to determine if it is a kit
            // If it is a kit, then we have to navigate to show details otherwise just add items to cart.
            if (numberOfSelectedProducts === 1) {
                if (!this.viewRelatedProducts()) {
                    asyncQueue.enqueue((): IVoidAsyncResult => {
                        var product: Entities.SimpleProduct = selectedProducts[0];
                        if (product.ProductTypeValue === Proxy.Entities.ProductType.KitMaster) {
                            productsIndeterminateWaitVisible(false);
                            this._processingAddItemsToCart = false;
                            this.selectedProducts([]);
                            this.clearProductSelection(true);
                            this.showProductDetails({ RecordId: product.RecordId, IsKit: true }, ViewModels.ProductAddModeEnum.AddToCart, this._quantity);
                            asyncQueue.cancel();
                        }

                        return VoidAsyncResult.createResolved();
                    });
                }
            }

            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                // If already processing add items to cart, don't start processing again.
                if (this._processingAddItemsToCart) {
                    return VoidAsyncResult.createResolved();
                }

                this._processingAddItemsToCart = true;
                productsIndeterminateWaitVisible(true);

                var options: Operations.IItemSaleOperationOptions = {
                    productSaleDetails: selectedProducts.map((product: Entities.SimpleProduct) => {
                        return <Proxy.Entities.ProductSaleReturnDetails>{ product: product, quantity: 0 };
                    })
                };

                return Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.ItemSale, options)
                    .done((result: Commerce.ICancelableResult) => {
                        productsIndeterminateWaitVisible(false);
                        this._processingAddItemsToCart = false;
                        if (!result.canceled) {
                            if (quickSale) {
                                if (ObjectExtensions.isFunction(this.showCartHandler)) {
                                    this.showCartHandler();
                                }
                            } else {
                                this.selectedProducts([]);
                                this.clearProductSelection(true);
                            }
                        }
                    });
            });

            var result: IAsyncResult<ICancelableResult> = asyncQueue.run().fail((errors: Entities.Error[]) => {
                Commerce.NotificationHandler.displayClientErrors(errors)
                    .always((): void => {
                        productsIndeterminateWaitVisible(false);
                        this._processingAddItemsToCart = false;
                    });
            });

            return asyncQueue.cancelOn(result);
        }

        /**
         * Gets the variable that is used to spcify the products mode busy state (including related products mode) in the current mode.
         *
         * @return {Observable<boolean>} The variable that should be used for the spinner based on the view mode.
         */
        private getCurrentProductsModeBusyTracker(): Observable<boolean> {
            if (this.viewRelatedProducts()) {
                return this.isRelatedProductsModeBusy;
            }

            return this.isProductsModeBusy;
        }

        /**
         * Adds customer to cart.
         * @param {Entities.AffiliationLoyaltyTier[]} cartAffiliations Cart affiliations.
         * @param {customer: Entities.GlobalCustomer) => void} successCallback The callback once customer has been added to cart.
         */
        private processAddCustomerToCart(
            cartAffiliations: Entities.AffiliationLoyaltyTier[],
            successCallback: (customer: Entities.GlobalCustomer) => void): void {

            if (!ArrayExtensions.hasElements(this.selectedCustomers())) {
                return;
            }

            this.isCustomersModeBusy(true);
            this.customerSearchViewModel.addCustomerToSale(this.selectedCustomers()[0], cartAffiliations)
                .done((customer: Entities.Customer) => {
                    this.isCustomersModeBusy(false);
                    successCallback(customer);
                }).fail((errors: Entities.Error[]) => {
                    this.isCustomersModeBusy(false);
                    RetailLogger.viewModelSearchViewModelAddCustomerToCartFailed(this.selectedCustomers()[0].AccountNumber, JSON.stringify(errors));
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Adds product to callback.
         */
        private addProductToCallback(): void {
            var product: Entities.Product = <Entities.Product>this.selectedProducts()[0];
            var asyncQueue: AsyncQueue = new AsyncQueue();
            this.isProductsModeBusy(true);

            if (!this.viewRelatedProducts()) {
                asyncQueue.enqueue((): IAsyncResult<Entities.Product[]> => {
                    return this.getProductDetailsForSelectedItems().done((products: Entities.Product[]) => {
                        product = products[0];
                    });
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                if (product.IsKit) {
                    if (ObjectExtensions.isFunction(this.showCustomerDetailsHandler)) {
                        this.showProductDetailsHandler(product);
                    }
                } else {
                    if (ObjectExtensions.isFunction(this.sendItemToCallbackHandler)) {
                        this.sendItemToCallbackHandler(product);
                    }
                }
                return VoidAsyncResult.createResolved();
            });

            asyncQueue.run()
                .fail((errors: Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                })
                .always(() => {
                    this.isProductsModeBusy(false);
                });
        }

        /**
         * Gets product details from the selected items.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        private getProductDetailsForSelectedItems(): IAsyncResult<Entities.Product[]> {
            var productIds: number[] = this.selectedProducts().map((product: Entities.Product) => product.RecordId);
            var productSearchCriteria: Entities.ProductSearchCriteria = new Entities.ProductSearchCriteriaClass({
                Ids: productIds,
                Context: Commerce.Session.instance.productCatalogStore.Context,
                IsOnline: Commerce.Session.instance.connectionStatus === Commerce.ConnectionStatusType.Online,
                SkipVariantExpansion: true,
                DataLevelValue: 1
            });
            return this.productManager.getProductDetailsBySearchCriteriaAsync(productSearchCriteria)
                .fail((errors: Entities.Error[]) => {
                    RetailLogger.viewModelSearchViewModelGetProductDetailsFailed(JSON.stringify(productSearchCriteria), JSON.stringify(errors));
                });
        }

        /**
         * Gets products from the selected items.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result.
         */
        private getProductsForSelectedItemsAsync(): IAsyncResult<Entities.SimpleProduct[]> {
            var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
            var productIds: number[] = this.selectedProducts().map((product: Entities.Product) => product.RecordId);
            if (!ArrayExtensions.hasElements(productIds)) {
                return VoidAsyncResult.createRejected();
            }

            if (productIds.length > 1) {
                return this.productManager.getByIdsAsync(productIds, channelId);
            } else {
                return this.productManager.getByIdAsync(productIds[0], channelId).map((product: Entities.SimpleProduct): Entities.SimpleProduct[] => {
                    return [product];
                });
            }
        }

        /**
         * Gets whether a specific button on an application bar is visible.
         */
        private getIsAddNewCustomerAvailable(): boolean {
            return this.viewCustomers() && (this.customerAddModeObservable() !== ViewModels.CustomerAddModeEnum.AddToReasonCode);
        }

        /**
         * Gets whether a specific button on an application bar is visible.
         */
        private getIsAddSelectedCustomerToCartAvailable(): boolean {
            var customerMode: ViewModels.CustomerAddModeEnum = this.customerAddModeObservable();
            return (customerMode >= ViewModels.CustomerAddModeEnum.AddToCart) && (customerMode !== ViewModels.CustomerAddModeEnum.AddToReasonCode);
        }

        /**
         * Resets status if refiners are available.
         */
        private resetRefineAvailability(): void {
            if (this.viewCustomers() || this.viewRelatedProducts()) {
                this.canRefineItems(false);
            } else {
                this.canRefineItems(this.productSearchViewModel.hasProductSearchResults());
            }
        }

        /**
         * Resets products.
         */
        private resetProducts(): void {
            if (ObjectExtensions.isFunction(this.resetProductsHandler)) {
                this.resetProductsHandler();
            }
        }

        /**
         * Resets customers.
         */
        private resetCustomers(): void {
            if (ObjectExtensions.isFunction(this.resetCustomersHandler)) {
                this.resetCustomersHandler();
            }
        }

        /**
         * Publishes that the search details have been changed and refiners should be updated.
         */
        private publishRefinerProductSearchDetailsChanged(): void {
            if (ObjectExtensions.isFunction(this.refinerProductSearchDetailsChangedHandler)) {
                this.refinerProductSearchDetailsChangedHandler();
            }
        }

        /**
         * On customer is added to cart.
         */
        private onCustomerAddedToCart(): void {
            if (ObjectExtensions.isFunction(this.onCustomerAddedToCartHandler)) {
                this.onCustomerAddedToCartHandler();
            }
        }

        /**
         * On reason code has been resolved.
         */
        private onReasonCodeResolved(): void {
            if (ObjectExtensions.isFunction(this.onReasonCodeResolvedHandler)) {
                this.onReasonCodeResolvedHandler();
            }
        }
    }
}
