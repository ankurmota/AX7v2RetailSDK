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

module Commerce.ViewControllers {
    "use strict";

    export class CompareProductsViewController extends ViewControllerBase {

        public ViewModel: Commerce.ViewModels.CompareProductsViewModel;
        public productDetailsViewModel: Commerce.ViewModels.ProductDetailsViewModel;
        public commonHeaderData;
        public productProperties: ObservableArray<any>;
        public isAddProductsVisible: Computed<boolean>;
        public currentCategory: Observable<string>;
        public sectionInfo: Computed<string>;
        public storeNumber: Observable<string>;
        public categoryName: Observable<string>;

        // Indeterminate wait objects
        public indeterminateWaitVisible: Observable<boolean>;

        constructor(options: any) {
            super(true);

            this.ViewModel = new Commerce.ViewModels.CompareProductsViewModel();
            this.productProperties = ko.observableArray([]);
            this.ViewModel.allProductProperties.subscribe((newValue) => {
                if (!Commerce.ObjectExtensions.isNullOrUndefined(newValue) && Commerce.ArrayExtensions.hasElements(newValue)) {
                    this.productProperties(newValue[0].productProperties);
                }
            });
            this.isAddProductsVisible = ko.computed(() => {
                return this.ViewModel.products().length < 3;
            }, this);
            this.productDetailsViewModel = new Commerce.ViewModels.ProductDetailsViewModel();
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            
            //Load Common Header
            this.storeNumber = ko.observable(Commerce.ApplicationContext.Instance.deviceConfiguration.StoreNumber);
            this.categoryName = ko.observable(options.categoryName || "");
            this.sectionInfo = ko.computed(() => {
                var storeInfoStringFormat = Commerce.ViewModelAdapter.getResourceString("string_707"); // : Store #{0} &gt; {1}
                return StringExtensions.format(storeInfoStringFormat, this.storeNumber(), this.categoryName());
            }, this);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_24")); // CATALOG 
            this.commonHeaderData.sectionInfo(this.sectionInfo()); // : Store #xxxx > category
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_700")); // Compare products 

            this.indeterminateWaitVisible = ko.observable(true);

            // Retrieve product details for the given products
            if (!ObjectExtensions.isNullOrUndefined(options) && ArrayExtensions.hasElements(options.items)) {
                var asyncQueue: AsyncQueue = new AsyncQueue();
                var productsToCompare: Model.Entities.Product[]; 

                asyncQueue.enqueue(() => {
                    return this.productDetailsViewModel.getProductDetails(options.items)
                        .done((products: Model.Entities.Product[]) => {
                            productsToCompare = products;
                        });
                }).enqueue(() => {
                    return ProductPropertiesHelper.getActivePricesUsingCart(options.items)
                        .done((productPrices: Model.Entities.ProductPrice[]) => {
                            ProductPropertiesHelper.updatePricesOnProducts(productsToCompare, productPrices);
                            this.ViewModel.products(productsToCompare);
                        });
                });

                asyncQueue.run().done(() => {
                    this.getProductDetailsSuccess();
                }).fail((error: Model.Entities.Error[]) => {
                    this.showControlError(error);
                });
            }
        }

       /**
         * Adds the given product to the cart.
         *
         * @param {Model.Entities.Product} product The invoked product.
         * @param {Commerce.ViewControllers.CompareProductsViewController} self The context of "this" view controller.
         */
        private addProductToCartHandler(product: Model.Entities.Product, self: Commerce.ViewControllers.CompareProductsViewController) {
            self.indeterminateWaitVisible(true);

            var options: Operations.IItemSaleOperationOptions = { productSaleDetails: [{ productId: product.RecordId, quantity: 0 }] };
            var operationResult = Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ItemSale, options);

            operationResult
                .done((result) => {
                    self.indeterminateWaitVisible(false);
                }).fail((errors) => {
                    self.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
        *  Removes the given product from the comparison screen
        *
        *  @param {Model.Entities.Product} product - invoked product
        *  @param {Commerce.ViewControllers.CompareProductsViewController} self - the context of "this" view controller
        */
        private removeProduct(product: Model.Entities.Product, self: Commerce.ViewControllers.CompareProductsViewController) {
            self.ViewModel.removeProductsFromCompareView(product.RecordId);

            // if all products are removed, empty the product property set
            if (!Commerce.ArrayExtensions.hasElements(self.ViewModel.products())) {
                this.productProperties([]);
            }
        }

        /**
        *  When a single product's image is clicked, takes the given
        *  product and navigates to the designated product details view.
        *
        *  @param {Model.Entities.Product} product - invoked product
        *  @param {Commerce.ViewControllers.CompareProductsViewController} self - the context of "this" view controller
        */
        private viewProductById(product: Model.Entities.Product, self: Commerce.ViewControllers.CompareProductsViewController) {
            var recordId: number = product.RecordId;
            var selectedProduct: Model.Entities.Product = self.ViewModel.products().filter((value) => { return value.RecordId === <number>recordId; })[0];
            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = selectedProduct.RecordId;
            Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        /**
        *  Error call back called when get product details call to server fails
        *
        *  @param {Model.Entities.Error} error - Error from the server
        */
        private showControlError(errors: Model.Entities.Error[]) {
            this.indeterminateWaitVisible(false);
            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        /**
        *  Success callback called when get product details call to server succeeds
        */
        private getProductDetailsSuccess() {
            this.indeterminateWaitVisible(false);

            // prepare the product property data so that a cohesive table of comparable properties
            // can be displayed in the UI below the product images.
            this.ViewModel.getPropertySetForDisplay();
        }

        /**
        *  Click handler for Add product button - navigates back to the previous view for product selection
        */
        private addProductClicked() {
            Commerce.Navigator.navigateBack();
        }
    }
}