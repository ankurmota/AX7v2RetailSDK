/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/Converters.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/VariantDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='SearchView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class InventoryLookupViewController extends ViewControllerBase {

        public searchKeyword: Observable<string>;
        public indeterminateWaitVisible: Observable<boolean>;
        public inventoryLookupViewModel: Commerce.ViewModels.InventoryLookupViewModel;
        public priceCheckViewModel: ViewModels.PriceCheckViewModel;
        public inventoriesVisible: Computed<boolean>;
        public variantsVisible: Computed<boolean>;
        public productId: Computed<string>;
        public zeroStoresResult: Computed<boolean>;
        public startScanVisible: Observable<boolean>;
        public showStoreLocationDisabled: Observable<boolean>;
        public addToTransactionDisabled: Computed<boolean>;
        public pickUpInStoreDisabled: Observable<boolean>;
        public storeContentVisible: Computed<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;

        private productDetailsViewModel: Commerce.ViewModels.ProductDetailsViewModel;

        private _variantDialog: Controls.VariantDialog;

        private _selectedStoreAvailability: Model.Entities.OrgUnitAvailability;

        constructor(options?: any) {
            super(true);

            this.indeterminateWaitVisible = ko.observable(false);
            this.searchKeyword = ko.observable("");
            this.inventoryLookupViewModel = new Commerce.ViewModels.InventoryLookupViewModel( options);
            this.productDetailsViewModel = new Commerce.ViewModels.ProductDetailsViewModel();
            this.priceCheckViewModel = new ViewModels.PriceCheckViewModel(this);
            this.inventoriesVisible = ko.computed(this.inventoriesResultVisible, this);
            this.zeroStoresResult = ko.computed(this.computeZeroStores, this);
            this.showStoreLocationDisabled = ko.observable(true);
            this.addToTransactionDisabled = ko.computed(this.computeAddToTransactionDisabled, this);
            this.productId = ko.computed(this.computeProductId, this);
            this.variantsVisible = ko.computed(this.computeVariantsVisible, this);
            this.pickUpInStoreDisabled = ko.observable(true);
            this.storeContentVisible = ko.computed(this.computeStoreContentVisible, this);
            this.startScanVisible = ko.observable(!options || (options && !options.Product));
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            // Load Common Header
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2607")); // INVENTORY LOOKUP
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_2606")); // Inventory by store

            this.addControl(this._variantDialog = new Controls.VariantDialog());

            this._selectedStoreAvailability = null;

            if (options && options.Product) {
                if (!ObjectExtensions.isNullOrUndefined(options.VariantId) && options.VariantId > 0) {
                    this.getInventoriesByProduct([options.VariantId]);
                } else {
                    this.processSingleProduct(options.Product);
                }
            }
        }

        /**
         * Called on load of view controller.
         */
        public load(): void {
            // Parse keyboard input.
            $(document).on("keypress", this.handleKeys);
        }

        public onShown(): void {
            Peripherals.instance.barcodeScanner.enableAsync((barcode: string) => {
                this.searchKeyword(barcode);
                this.findInventoriesByKeyword();
            });
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            Peripherals.instance.barcodeScanner.disableAsync();
            // Removes eventhandler.
            $(document).off("keypress", this.handleKeys);
        }

        public storeSelectionChanged(items: Model.Entities.OrgUnitAvailability[]): void {
            var numStoresSelected: number = items.length;
            this.showStoreLocationDisabled(numStoresSelected !== 1);
            this.pickUpInStoreDisabled(numStoresSelected !== 1);

            if (items.length === 1) {
                this._selectedStoreAvailability = items[0];
            }
        }

        public findInventoriesByKeywordNumpad(numPadResult: Controls.NumPad.INumPadResult): void {
            this.findInventoriesByKeyword();
        }

        public findInventoriesByKeyword(): void {
            if (StringExtensions.isNullOrWhitespace(this.searchKeyword())) {
                return;
            }

            this.startScanVisible(false);
            this.indeterminateWaitVisible(true);
            this.inventoryLookupViewModel.stores([]);
            this.inventoryLookupViewModel.product(new Commerce.Model.Entities.ProductClass());


            // using skipvariant expansion to improve search performance, especially when 
            // search result is expected to contain products with large number of variants.
            this.inventoryLookupViewModel.searchProductsByKeyword(this.searchKeyword(), 4, true)
                .done((products: Model.Entities.Product[]) => {
                if (!ArrayExtensions.hasElements(products)) {
                    // show UI error message if search returned no results.
                    ViewModelAdapterWinJS.displayMessage(
                        Commerce.ViewModelAdapterWinJS.getResourceString("string_3382"),
                        MessageType.Error,
                        MessageBoxButtons.Default,
                        Commerce.ViewModelAdapterWinJS.getResourceString("string_3381")
                        );
                    this.indeterminateWaitVisible(false);
                    return;
                } else if (products.length > 1) {
                        // if products has more than 1 result, navigate to product page so 
                        // we can choose a product that we want to look for inventory availability.
                        this.searchForProduct(products);
                    } else if (products.length === 1) {
                        this.processSingleProduct(products[0]);
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                });
        }

        public showStoreLocation(): void {
            Commerce.ViewModelAdapter.navigate("StoreDetailsView", {
                StoreId: this._selectedStoreAvailability.OrgUnitLocation.OrgUnitNumber
            });
        }

        public pickUpInStore(): void {
            this.indeterminateWaitVisible(true);

            this.inventoryLookupViewModel.pickUpInStore(this._selectedStoreAvailability.OrgUnitLocation)
                .always(() => {
                    this.indeterminateWaitVisible(false);
                }).done((cancelResult: ICancelableResult) => {

                    // Do not navigate to CartView if user clicks 'Cancel' on any of the dialogs provided.
                    if (!cancelResult.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public addToTransaction(): void {
            this.indeterminateWaitVisible(true);

            var options: Operations.IItemSaleOperationOptions = {
                productSaleDetails: [{
                    productId: this.inventoryLookupViewModel.variantId(), quantity: 0
                }]
            };

            var operationResult: IAsyncResult<ICancelableDataResult<{}>> = Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ItemSale, options);

            operationResult
                .done((result: ICancelableDataResult<{}>) => {
                    this.indeterminateWaitVisible(false);

                    if (!result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
            }).fail((errors: Model.Entities.Error[]) => {
                this.indeterminateWaitVisible(false);
                NotificationHandler.displayClientErrors(errors);
            });
        }

        private processSingleProduct(product: Model.Entities.Product): void {

            var richMediaLocation: Model.Entities.RichMediaLocations = product.Image;
            this.inventoryLookupViewModel.product(product);
            if (!ObjectExtensions.isNullOrUndefined(richMediaLocation)
                && ArrayExtensions.hasElements(richMediaLocation.Items)
                && !StringExtensions.isNullOrWhitespace(richMediaLocation.Items[0].Url)) {
                var imageAbsolutePath: string = Commerce.Formatters.ImageUrlFormatter(richMediaLocation.Items[0].Url);
                this.inventoryLookupViewModel.productImageAltText(imageAbsolutePath);
            }

            if (product.IsMasterProduct) {

                var productVariant: Model.Entities.ProductVariant = ProductPropertiesHelper.getActiveVariant(product);

                if (!ObjectExtensions.isNullOrUndefined(productVariant)) {
                    this.getInventoriesByProduct([productVariant.DistinctProductVariantId]);
                } else if (product.IsKit) {
                    // If product is kit and there is no active variant selected then 
                    // we dont need to read the full product object so directly navigate to product details view
                    this.inventoryLookupViewModel.product(product);
                            this.navigateToProductDetails();

                        } else {
                            // This is a regular master product. Trigger variant selection
                            // to get variant that user wants.
                                    this.productDetailsViewModel.getProductDetails([product.RecordId])
                                        .done((productDetails: Model.Entities.Product[]) => {

                                            var richMediaLocation: Model.Entities.RichMediaLocations = productDetails[0].Image;
                                            this.inventoryLookupViewModel.product(productDetails[0]);
                                            if (!ObjectExtensions.isNullOrUndefined(richMediaLocation)
                                                && ArrayExtensions.hasElements(richMediaLocation.Items)
                                                && !StringExtensions.isNullOrWhitespace(richMediaLocation.Items[0].Url)) {
                                                var imageAbsolutePath: string = Commerce.Formatters.ImageUrlFormatter(richMediaLocation.Items[0].Url);
                                                this.inventoryLookupViewModel.productImageAltText(imageAbsolutePath);
                                            }

                                            this.triggerVariantSelection();
                                })
                                .fail((errors: Model.Entities.Error[]) => {
                                    Commerce.NotificationHandler.displayClientErrors(errors);
                                    this.indeterminateWaitVisible(false);
                                });
                        }
            } else {
                // product is nonvariant, trigger get inventories by product.
                this.getInventoriesByProduct(null);
            }
        }

        private computeProductId(): string {
            var product: Model.Entities.Product = this.inventoryLookupViewModel.product();
            return ProductPropertiesHelper.getProperty(product.RecordId, product, ProductPropertyNameEnum.ProductNumber);
        }

        private computeVariantsVisible(): boolean {
            var product: Model.Entities.Product = this.inventoryLookupViewModel.product();

            return !ObjectExtensions.isNullOrUndefined(product.CompositionInformation) &&
                !ObjectExtensions.isNullOrUndefined(product.CompositionInformation.VariantInformation) &&
                ArrayExtensions.hasElements(product.CompositionInformation.VariantInformation.Variants);
        }

        private computeAddToTransactionDisabled(): boolean {
            return this.inventoryLookupViewModel.product().RecordId === 0;
        }

        private inventoriesResultVisible(): boolean {
            return this.inventoryLookupViewModel.product().RecordId > 0;
        }

        private computeZeroStores(): boolean {
            return this.inventoryLookupViewModel.product().RecordId > 0 &&
                !ArrayExtensions.hasElements(this.inventoryLookupViewModel.stores()) &&
                !this.indeterminateWaitVisible();
        }

        private computeStoreContentVisible(): boolean {
            return this.inventoryLookupViewModel.stores().length !== 0;
        }

        private searchForProduct(products?: Model.Entities.Product[]): void {
            this.indeterminateWaitVisible(false);

            var parameters: any = {
                searchText: this.searchKeyword(),
                searchEntity: "Products",
                relatedItems: products,
                addModeEnum: ViewModels.ProductAddModeEnum.InventoryLookup,
                pageCallback: "InventoryLookupView"
            };

            Commerce.ViewModelAdapter.navigate("SearchView", parameters);
        }

        private triggerVariantSelection(): void {
            this._variantDialog.show({ products: [this.inventoryLookupViewModel.product()] })
                .on(DialogResult.OK, (result: number[]) => { this.getInventoriesByProduct(result); })
                .on(DialogResult.Cancel, (result: number[]) => { this.variantCancelCallback(); })
                .onError((errors: Model.Entities.Error[]) => { this.variantErrorCallback(errors); });
        }

        private getInventoriesByProduct(variantIds: number[]): void {
            var variantId: number = (variantIds || [])[0];
            this.indeterminateWaitVisible(true);

            this.priceCheckViewModel.product = this.inventoryLookupViewModel.product();
            this.priceCheckViewModel.store(Session.instance.productCatalogStore.Store);

            if (!this.priceCheckViewModel.product.IsMasterProduct) {
                variantId = this.priceCheckViewModel.product.RecordId;
            }

            this.priceCheckViewModel.variantId(variantId);

            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                // Get the latest product price
                return this.priceCheckViewModel.getProductPrice();
            }).enqueue(() => {
                    // Get remaining inventories from the selected product.
                    return this.inventoryLookupViewModel.getInventoriesByProduct(variantId);
                });

            asyncQueue.run()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);

                    // If the error type is HQResponseParsingError (no data returned) then no store is holding the inventory.
                    // no further action should required if this error happens, as the computed observable 'zeroStoresResult'
                    // should display the message.
                    // Only display the message if other error types is thrown.
                    if (!ErrorHelper.hasError(errors, "Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError") ||
                        (ArrayExtensions.hasElements(errors) && errors.length > 1)) {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    }
                });
        }

        private variantErrorCallback(errors: Model.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        private variantCancelCallback(): void {
            this.searchKeyword("");
            this.indeterminateWaitVisible(false);
        }

        private navigateToProductDetails(): void {
            this.indeterminateWaitVisible(false);

            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = this.inventoryLookupViewModel.product().RecordId;
            productDetailOptions.product = this.inventoryLookupViewModel.product();
            productDetailOptions.pageCallback = "InventoryLookupView";
            productDetailOptions.productAddModeEnum = ViewModels.ProductAddModeEnum.InventoryLookup;

            Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        /**
         * Handles Key stroke events
         * @param {JQueryEventObject} event The JQuery event object.
         */
        private handleKeys(event: JQueryEventObject): void {
            if (event.keyCode !== 13) {
                $("#inventoryLookupProductSearch").focus();
            }
        }
    }
}
