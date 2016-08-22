/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/VariantDialog.ts'/>
///<reference path='../CustomerOrder/PickUpInStoreView.ts'/>
///<reference path='../ViewControllerBase.ts'/>
///<reference path='SearchView.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class PriceCheckViewController extends ViewControllerBase {

        private _productDetailsViewModel: ViewModels.ProductDetailsViewModel;
        public cartViewModel: Commerce.ViewModels.CartViewModel;

        public indeterminateWaitVisible: Observable<boolean>;
        public addProductToCartDisabled: Observable<boolean>;
        public priceCheckViewModel: ViewModels.PriceCheckViewModel;
        public customerVisible: Computed<boolean>;
        public searchText: Observable<string>;
        public productVisible: Computed<boolean>;
        public productDescriptionExpanded: Observable<boolean>;
        public variantsVisible: Computed<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;

        private _variantDialog: Controls.VariantDialog;
        private _options: any;

        constructor(options?: any) {
            super(true);

            options = options || {};
            this.priceCheckViewModel = new ViewModels.PriceCheckViewModel(options);
            this._productDetailsViewModel = new ViewModels.ProductDetailsViewModel();
            this.indeterminateWaitVisible = ko.observable(false);

            this.customerVisible = ko.computed(this.computeCustomerVisible, this);
            this.searchText = ko.observable("");
            this.addProductToCartDisabled = ko.observable(true);
            this.productVisible = ko.computed(this.computeProductVisible, this);
            this.variantsVisible = ko.computed(this.computeVariantsVisible, this);

            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_3500"));//PRICE CHECK
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_3501"));//Price check

            this.addControl(this._variantDialog = new Controls.VariantDialog());

            this._options = options;
            this.cartViewModel = new Commerce.ViewModels.CartViewModel();
        }

        /**
         * Called on load of view controller.
         */
        public load() {
            // Parse keyboard input.
            $(document).on("keypress", this.handleKeys);
            this.loadPriceCheckView(this._options);
        }

        private loadPriceCheckView(options: any) {
            this.indeterminateWaitVisible(true);

            if (ObjectExtensions.isNullOrUndefined(options.VariantId)) {
                options.VariantId = -1;
            }

            this.priceCheckViewModel.load()
                .done(() => {
                    var storeType: Model.Entities.StoreButtonControlType = Session.instance.productCatalogStore.StoreType;
                    if (this.priceCheckViewModel.product.RecordId > 0) {
                        this.addProductToCartDisabled(false);
                    }
                    this.indeterminateWaitVisible(false);

                    if (options.Product.RecordId > 0 && ((options.Product.IsMasterProduct && options.VariantId <= 0) || !options.Product.IsMasterProduct)) {
                        this.processSingleProduct(options.Product);
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        private computeVariantsVisible(): boolean {
            return this.priceCheckViewModel.product.RecordId !== this.priceCheckViewModel.variantId();
        }

        private computeProductVisible(): boolean {
            return this.priceCheckViewModel.variantId() > 0;
        }

        private computeCustomerVisible(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this.priceCheckViewModel.customer()) &&
                !StringExtensions.isNullOrWhitespace(this.priceCheckViewModel.customer().AccountNumber);
        }

        private triggerVariantSelection(product: Model.Entities.Product) {
            this._variantDialog.show({ products: [product] })
                .on(DialogResult.OK, (result) => { this.variantSuccessCallback(result); })
                .on(DialogResult.Cancel, (result) => { this.variantCancelCallback(); })
                .onError((errors) => { this.variantErrorCallback(errors); });
        }

        private variantSuccessCallback(variantIds: number[]) {
            this.priceCheckViewModel.variantId(variantIds[0]);
            this.getProductPrice();
        }

        private variantErrorCallback(errors: Model.Entities.Error[]) {
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        private variantCancelCallback(): void {
            this.indeterminateWaitVisible(false);
        }

        public navigateToSearchProducts(products: Model.Entities.Product[]) {
            this.indeterminateWaitVisible(false);

            if (!ArrayExtensions.hasElements(products)) {
                products = [];
            }

            var parameters = {
                addModeEnum: ViewModels.ProductAddModeEnum.PriceCheck,
                relatedItems: products,
                searchText: this.searchText(),
                searchEntity: "Products",
                storeId: this.priceCheckViewModel.storeNumber,
                pageCallback: "PriceCheckView",
                saveInHistory: false
            };
            Commerce.ViewModelAdapter.navigate("SearchView", parameters);
        }

        private getProductPrice(): void {
            this.indeterminateWaitVisible(true);
            this.priceCheckViewModel.getProductPrice()
                .done(() => {
                    this.addProductToCartDisabled(false);
                    this.indeterminateWaitVisible(false);
                    RetailLogger.viewsMerchandisingPriceCheckViewGetPriceFinished();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            );
        }

        private processSingleProduct(product: Model.Entities.Product) {

            this.priceCheckViewModel.product = product;

            if (product.IsMasterProduct) {

                var productVariant: Model.Entities.ProductVariant = ProductPropertiesHelper.getActiveVariant(product);

                        if (!ObjectExtensions.isNullOrUndefined(productVariant)) {
                            // Active variant is available, get the product price.
                            this.priceCheckViewModel.variantId(productVariant.DistinctProductVariantId);
                            this.getProductPrice();
                } else if (product.IsKit) {
                    // If product is kit and there is no active variant selected, directly navigate to product details view to choose kit variants - there is no need to re-read the full product object which includes all variants
                            this.navigateToProductDetails();
                        } else {
                            // This is a regular master product. Trigger variant selection
                            // to get variant that user wants.
                    this._productDetailsViewModel.getProductDetails([product.RecordId])
                        .done((productDetails: Model.Entities.Product[]) => {
                            this.priceCheckViewModel.product = productDetails[0];
                            this.triggerVariantSelection(productDetails[0]);
                        })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            );
        }
            }
            else {
                //product is nonvariant, get the product price.
                this.priceCheckViewModel.variantId(product.RecordId);
                this.getProductPrice();
            }
        }

        /**
         * Search for a customer on customer search page.
         */
        public searchCustomers(): void {
            var parameters = {
                searchEntity: "Customers",
                customerAddModeEnum: ViewModels.CustomerAddModeEnum.AddToPriceCheck,
                relatedItems: null,
                VariantId: 0,
                storeId: this.priceCheckViewModel.storeNumber,
                Product: null,
                saveInHistory: false
            };

            var variantId: number = this.priceCheckViewModel.variantId();
            if (!ObjectExtensions.isNullOrUndefined(variantId) && variantId > 0) {
                parameters.VariantId = variantId;
            }

            if (this.priceCheckViewModel.product.RecordId != 0) {
                parameters.relatedItems = [this.priceCheckViewModel.product];
                parameters.Product = this.priceCheckViewModel.product;
            }

            Commerce.ViewModelAdapter.navigate("SearchView", parameters);
        }

        /**
         * Adds a product to cart.
         */
        public addProductToCart(): void {

            this.indeterminateWaitVisible(true);
            var options: Operations.IItemSaleOperationOptions = {
                productSaleDetails: [{
                    productId: this.priceCheckViewModel.variantId(), quantity: 0
                }]
            };

            var operationResult = Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ItemSale, options);

            operationResult
                .done((result) => {
                    this.indeterminateWaitVisible(false);
                    if (!result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public searchProductNumpad(numPadResult: Controls.NumPad.INumPadResult): void {
            this.searchProduct();
        }

        /**
         * Search for a product.
         */
        public searchProduct(): void {
            this.indeterminateWaitVisible(true);

            // using skipvariant expansion to improve search performance, especially when search result is expected to contain products with large number of variants.
            this.priceCheckViewModel.searchProductsByKeyword(this.searchText(), 4, true)
                .done((products: Model.Entities.Product[]) => {
                    if (products.length > 1) {
                        //navigate to product search page
                        this.navigateToSearchProducts(products);
                    } else if (products.length === 1) {
                        this.processSingleProduct(products[0]);
                    } else {
                        this.indeterminateWaitVisible(false);

                        //show UI no products
                        ViewModelAdapterWinJS.displayMessage(
                            Commerce.ViewModelAdapterWinJS.getResourceString("string_3382"),
                            MessageType.Error,
                            MessageBoxButtons.Default,
                            Commerce.ViewModelAdapterWinJS.getResourceString("string_3381")
                        );
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            );
        }

        /**
         * Navigates to product details.
         */
        public navigateToProductDetails(): void {
            this.indeterminateWaitVisible(false);
            var parameters = {
                storeId: this.priceCheckViewModel.storeNumber
            }

            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = this.priceCheckViewModel.product.RecordId;
            productDetailOptions.pageCallback = "PriceCheckView";
            productDetailOptions.additionalParams = parameters;
            productDetailOptions.product = this.priceCheckViewModel.product;
            productDetailOptions.productAddModeEnum = ViewModels.ProductAddModeEnum.PriceCheck;
            Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        /**
         * Navigates to product details.
         */
        public navigateToCustomerDetails(): void {
            var parameters: ICustomerDetailsViewOptions = {
                accountNumber: Session.instance.cart.CustomerId,
                destination: "PriceCheckView",
                destinationOptions: { VariantId: this.priceCheckViewModel.variantId(), Product: this.priceCheckViewModel.product }
            };

            Commerce.ViewModelAdapter.navigate("CustomerDetailsView", parameters);
        }

        /**
         * Navigates to store details.
         */
        public navigateToStoreDetails(): void {
            if (Session.instance.isStoreCatalogVirtual) {
                return;
            }

            var parameters = {
                StoreId: this.priceCheckViewModel.storeNumber
            };

            Commerce.ViewModelAdapter.navigate("StoreDetailsView", parameters);
        }

       /**
         * Called when view is shown.
         */
        public onShown(): void {
            Peripherals.instance.barcodeScanner.enableAsync(
                (barcode: string) => {
                    if (!StringExtensions.isNullOrWhitespace(barcode)) {
                        this.searchText(barcode);
                        this.searchProduct();
                }
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

        public removeCustomer(): void {
            this.indeterminateWaitVisible(true);

            this.cartViewModel.removeCustomerFromCart().done(() => {
                    //update model to remove a customer 
                    this.priceCheckViewModel.customer(null);
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors)=> {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
            });
        }

        /**
         * Handles Key stroke events
         * @param {JQueryEventObject} event The JQuery event object.
         */
        private handleKeys(event: JQueryEventObject): void {
            if (event.keyCode != 13) {
                $('#priceCheckProductSearch').focus();
            }
        }
    }
}