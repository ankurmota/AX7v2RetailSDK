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

    import Entities = Proxy.Entities;

    export enum CustomerProductsSourceType {
        RecentPurchases,
        Wishlist
    }

    export class RecentPurchasesViewController extends ViewControllerBase {

        public customerDetailsViewModel: ViewModels.CustomerDetailsViewModel;
        public productDetailsViewModel: ViewModels.ProductDetailsViewModel;
        public selectedPurchaseHistoryLines: Entities.PurchaseHistory[];
        public selectedWishlistLines: Entities.Product[];
        public compareDisabled: Observable<boolean>;
        public addToSaleDisabled: Observable<boolean>;
        public displayZeroProductsText: Observable<boolean>;
        public indeterminateWaitVisible: Observable<boolean>;
        public toggleShowHideMenu: Observable<any>;
        public commonHeaderData: Controls.CommonHeaderData;
        public customerName: string;
        public wishlistName: string;

        private _sourceType: CustomerProductsSourceType;

        constructor(options: any) {
            super(true);

            this.customerDetailsViewModel = new ViewModels.CustomerDetailsViewModel();
            this.productDetailsViewModel = new ViewModels.ProductDetailsViewModel();
            this.commonHeaderData = new Controls.CommonHeaderData();
            this.selectedPurchaseHistoryLines = [];
            this.selectedWishlistLines = [];
            this.displayZeroProductsText = ko.observable(false);
            this.indeterminateWaitVisible = ko.observable(false);
            this.toggleShowHideMenu = ko.observable(() => { });
            this.compareDisabled = ko.observable(true);
            this.addToSaleDisabled = ko.observable(true);
            this._sourceType = CustomerProductsSourceType.RecentPurchases;

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                if (!ObjectExtensions.isNullOrUndefined(options.sourceType)) {
                    this._sourceType = options.sourceType;
                }

                if (!ObjectExtensions.isNullOrUndefined(options.wishlist)) {
                    if (this._sourceType == CustomerProductsSourceType.Wishlist) {
                        this.displayWishlistProducts(options.wishlist);
                        this.wishlistName = options.wishlist.Name;
                    }
                }

                if (!ObjectExtensions.isNullOrUndefined(options.customerAccountNumber)) {
                    this.customerDetailsViewModel.customerAccountNumber = options.customerAccountNumber;
                }

                if (!ObjectExtensions.isNullOrUndefined(options.customerName)) {
                    this.customerName = options.customerName;
                }
            }

            this.initializeCommonHeader();
        }

        /**
        *   Initialize the header
        */
        private initializeCommonHeader() {
            //Load Common Header
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);
            this.commonHeaderData.sectionTitle(this.customerName);
            if (this._sourceType === CustomerProductsSourceType.RecentPurchases) {
                this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_230"));
            }
            else if (this._sourceType === CustomerProductsSourceType.Wishlist) {
                this.commonHeaderData.categoryName(this.wishlistName);
            }
        }

        private displayWishlistProducts(wishlist: Model.Entities.CommerceList) {
            this.indeterminateWaitVisible(true);
            this.customerDetailsViewModel.getWishListProducts(wishlist)
                .done(() => {
                    this.displayZeroProductsText(this.customerDetailsViewModel.wishListProducts().length == 0);
                }).fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                }).always(() => {
                    this.indeterminateWaitVisible(false);
                });
        }

        /**
         *   Add to sale click event handler.
         */
        private addToSaleClick(eventInfo) {
            this.addProductsToCart(false);
        }

        /**
         *   Sell now click event handler.
         */
        private sellNowClick(eventInfo) {
            this.addProductsToCart(true);
        }

        /**
         * Adds the selected products to the cart.
         * @param {boolean} navigateToCart Value indicating whether or not navigate tothe cart page upon successfully adding the products to cart.
         */
        private addProductsToCart(navigateToCart: boolean): void {
            // gets the identifiers of all the products.
            var productIds: number[] = [];
            if (this._sourceType === CustomerProductsSourceType.RecentPurchases) {
                if (!ArrayExtensions.hasElements(this.selectedPurchaseHistoryLines)) {
                    return;
                }

                productIds = this.selectedPurchaseHistoryLines.map((purchaseHistoryLine) => { return purchaseHistoryLine.ProductId });
            } else if (this._sourceType === CustomerProductsSourceType.Wishlist) {
                productIds = this.selectedWishlistLines.map((wishlistLine) => { return wishlistLine.RecordId });
            }

            if (!ArrayExtensions.hasElements(productIds)) {
                return;
            }

            this.indeterminateWaitVisible(true);

            var options: Operations.IItemSaleOperationOptions = {
                productSaleDetails: productIds.map((productId: number): Entities.ProductSaleReturnDetails => {
                    return { productId: productId, quantity: 0 };
                })
            };

            var operationResult = Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.ItemSale, options);

            operationResult
                .done((result) => {
                    if (!result.canceled && navigateToCart) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors) => {
                    NotificationHandler.displayClientErrors(errors);
                }).always(() => {
                    this.indeterminateWaitVisible(false);
                });
        }

        private recentPurchaseItemInvokedHandler(item: Proxy.Entities.PurchaseHistory) {
            if (!ObjectExtensions.isNullOrUndefined(item.ProductId)) {

                var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                productDetailOptions.recordId = item.ProductId;
                ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
            }
        }

        private wishlistItemInvokedHandler(item: Model.Entities.Product) {
            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = item.RecordId;
            ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        private recentPurchaseSelectionChangedEventHandler(lines: Model.Entities.PurchaseHistory[]) {
            this.selectedPurchaseHistoryLines = lines;
            var numItemsSelected = this.selectedPurchaseHistoryLines.length;

            // Enable or disable available commands that are bound to the following members.
            this.compareDisabled(numItemsSelected < 2 || numItemsSelected > 3);
            this.addToSaleDisabled(numItemsSelected == 0);
        }

        private wishlistProductSelectionChangedEventHandler(items: Model.Entities.Product[]) {
            this.selectedWishlistLines = items;
            var numItemsSelected = this.selectedWishlistLines.length;

            // Enable or disable available commands that are bound to the following members.
            this.compareDisabled(numItemsSelected < 2 || numItemsSelected > 3);
            this.addToSaleDisabled(numItemsSelected == 0);
        }

        private compareClicked(eventInfo) {
            var productIds: number[] = [];

            if (this._sourceType == CustomerProductsSourceType.RecentPurchases) {
                productIds = this.selectedPurchaseHistoryLines.map((purchaseHistoryLine) => { return purchaseHistoryLine.ProductId });
                ViewModelAdapter.navigate("CompareProductsView", { items: productIds });
            }
            else if (this._sourceType == CustomerProductsSourceType.Wishlist) {
                productIds = this.selectedWishlistLines.map((wishlistLine) => { return wishlistLine.RecordId });
                ViewModelAdapter.navigate("CompareProductsView", { items: productIds });
            }
        }

        /* 
        *   Switches view between templates
        *   @param {any} eventInfo The event information.
        */
        private ShowViewMenu(eventInfo: any) {
            this.toggleShowHideMenu()();
        }
    }
}