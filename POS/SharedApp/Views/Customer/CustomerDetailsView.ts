/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/AutocompleteComboboxDialog.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/ReasonCodeDialog.ts'/>
///<reference path='../Merchandising/ProductDetailsView.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * CustomerDetailsViewController constructor parameters interface
     */
    export interface ICustomerDetailsViewOptions {
        /**
         * The account number of the customer of which to show the details.
         */
        accountNumber: string;

        /**
         * Destination view.
         */
        destination: string;

        /**
         * Destination view.
         */
        destinationOptions: any;
    }


    export class CustomerDetailsViewController extends ViewControllerBase {
        public customerDetailsViewModel: ViewModels.CustomerDetailsViewModel;
        public productDetailsViewModel: ViewModels.ProductDetailsViewModel;
        public affiliationsViewModel: ViewModels.AffiliationsViewModel;

        public addToCartDisabled: Observable<boolean>;
        public editCustomerDisabled: Observable<boolean>;
        public indeterminateWaitVisible: Observable<boolean>;
        public recentPurchasesIndeterminateWaitVisible: Observable<boolean>;
        public wishListIndeterminateWaitVisible: Observable<boolean>;
        public suggestedProductsIndeterminateWaitVisible: Observable<boolean>;
        public orderHistoryIndeterminateWaitVisible: Observable<boolean>;
        public affiliationsIndeterminateWaitVisible: Observable<boolean>;
        public loyaltyCardsIndeterminateWaitVisible: Observable<boolean>;
        public toggleShowHideMenu: Observable<any>;
        public numberOfAddressesText: Computed<string>;
        public addressFilled: Observable<boolean>;
        public zeroSuggestedProductsVisible: Computed<boolean>;
        public zeroWishListVisible: Computed<boolean>;
        public zeroLoyaltyCardVisible: Computed<boolean>;
        public forceOrderListLayout: Observable<() => void>;
        public forceSuggestedProductsLayout: Observable<() => void>;
        public forceWishListLayout: Observable<() => void>;
        public forceRecentPurchasesLayout: Observable<() => void>;

        public commonHeaderData;
        public customerName: string;
        public selectedWishListProducts: Proxy.Entities.Product[];
        public selectedRecentPurchases: Proxy.Entities.SalesLine[];
        public selectedSuggestedProducts: Proxy.Entities.Product[];
        public customerAccountNumber: string;
        public customerFullAddress: Observable<string>;
        public customerEmail: Observable<string>;
        public customerEmailHref: Observable<string>;
        public customerPhoneHref: Observable<string>;

        public customerPhone: Observable<string>;
        private _destination: string = "CartView";
        private _destinationOptions: any;

        private _autocompleteControl: Controls.AutocompleteComboboxDialog;
        private _reasonCodeDialog: Controls.ReasonCodeDialog;
        private _customerCardViewModel: ViewModels.CustomerCardViewModel;

        constructor(options: ICustomerDetailsViewOptions) {
            super(true);

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                if (!ObjectExtensions.isNullOrUndefined(options.destination) || !StringExtensions.isEmptyOrWhitespace(options.destination)) {
                    this._destination = options.destination;
                }
                if (!ObjectExtensions.isNullOrUndefined(options.destinationOptions) || !StringExtensions.isEmptyOrWhitespace(options.destinationOptions)) {
                    this._destinationOptions = options.destinationOptions;
                }
            }

            // Initialize visibility
            this.addToCartDisabled = ko.observable(true);
            this.editCustomerDisabled = ko.observable(true);
            this.indeterminateWaitVisible = ko.observable(false);
            this.recentPurchasesIndeterminateWaitVisible = ko.observable(true);
            this.wishListIndeterminateWaitVisible = ko.observable(true);
            this.suggestedProductsIndeterminateWaitVisible = ko.observable(true);
            this.orderHistoryIndeterminateWaitVisible = ko.observable(true);
            this.affiliationsIndeterminateWaitVisible = ko.observable(true);
            this.loyaltyCardsIndeterminateWaitVisible = ko.observable(true);
            this.addressFilled = ko.observable(false);
            this.customerAccountNumber = options.accountNumber;
            this.selectedWishListProducts = [];
            this.selectedRecentPurchases = [];
            this.selectedSuggestedProducts = [];
            this.customerEmail = ko.observable("");
            this.customerEmailHref = ko.observable("");
            this.customerPhoneHref = ko.observable("");
            this.customerPhone = ko.observable("");
            this.customerFullAddress = ko.observable("");
            this.toggleShowHideMenu = ko.observable(() => { });
            this.forceOrderListLayout = ko.observable(() => { });
            this.forceRecentPurchasesLayout = ko.observable(() => { });
            this.forceWishListLayout = ko.observable(() => { });
            this.forceSuggestedProductsLayout = ko.observable(() => { });

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.customerDetailsViewModel = new Commerce.ViewModels.CustomerDetailsViewModel();
            this.productDetailsViewModel = new Commerce.ViewModels.ProductDetailsViewModel();
            this.affiliationsViewModel = new Commerce.ViewModels.AffiliationsViewModel();

            this.addControl(this._autocompleteControl = new Controls.AutocompleteComboboxDialog());
            this.addControl(this._reasonCodeDialog = new Controls.ReasonCodeDialog());
            this._customerCardViewModel = new ViewModels.CustomerCardViewModel(<ViewModels.ICustomerCardViewModelOptions>{
                parentView: "CustomerDetailsView",
                passThroughDestination: this._destination,
                passThroughOptions: this._destinationOptions,
                isLoyaltyCardDataReadOnly: true
            });

            this.customerDetailsViewModel.Customer.subscribe((newValue) => {
                if (newValue) {
                    this.addToCartDisabled(false);
                    this.editCustomerDisabled(StringExtensions.isNullOrWhitespace(this.customerDetailsViewModel.Customer().AccountNumber));
                }
            }, this);

            this.customerDetailsViewModel.loadCustomer(this.customerAccountNumber)
                .done(() => {
                    this.affiliationsIndeterminateWaitVisible(false);
                    var customerFound = this.customerDetailsViewModel.Customer();
                    this.customerName = customerFound.Name;

                    // populate the customer in the customer card control to populate the address template
                    this._customerCardViewModel.customer(customerFound);

                    if (!StringExtensions.isNullOrWhitespace(customerFound.Email)) {
                        this.customerEmail(customerFound.Email);
                    } else if (!StringExtensions.isNullOrWhitespace(customerFound.ReceiptEmail)) {
                        this.customerEmail(customerFound.ReceiptEmail);
                    } else {
                        this.customerEmail(StringExtensions.EMPTY);
                    }

                    this.customerPhone(customerFound.Phone);
                    this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_136"));
                    this.commonHeaderData.categoryName(this.customerName);
                    this.customerFullAddress(this.formatCustomerAddress(this.customerDetailsViewModel.customerAddress()));
                    this.customerEmailHref("mailto:" + this.customerEmail());
                    this.customerPhoneHref("tel:" + this.customerPhone());
                    RetailLogger.viewsCustomerDetailsLoaded();
                }).fail((errors) => {
                    this.affiliationsIndeterminateWaitVisible(false);
                    this.errorCallback(errors);
                });

            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(true);

            // Load the recent purchases panels.
            this.customerDetailsViewModel.getRecentPurchases(
                Commerce.ViewModels.CustomerDetailsViewModel.RECENT_PURCHASES_TO_INITIAL_DISPLAY,
                0) // skip 0 on customer details page.
                .done((): void => {
                    this.recentPurchasesIndeterminateWaitVisible(false);
                    this.forceRecentPurchasesLayout()();
                }).fail((errors: Proxy.Entities.Error[]) => {
                    this.recentPurchasesIndeterminateWaitVisible(false);
                    this.errorCallback(errors);
                });

            // Load the order history and suggested products panels.
            this.customerDetailsViewModel.getSalesOrdersByCustomer(options.accountNumber,
                Commerce.ViewModels.CustomerDetailsViewModel.SALES_ORDERS_FOR_INITIAL_DISPLAY,
                Commerce.ViewModels.CustomerDetailsViewModel.RECENT_PURCHASES_TO_INITIAL_DISPLAY)
                .done((): void => {
                    this.orderHistoryIndeterminateWaitVisible(false);
                    this.suggestedProductsIndeterminateWaitVisible(false);
                    // After we hide loader we and show the grid
                    // we have to force the grid to update its items position.
                    this.forceOrderListLayout()();
                    this.forceSuggestedProductsLayout()();
                }).fail((errors: Proxy.Entities.Error[]) => {
                    this.orderHistoryIndeterminateWaitVisible(false);
                    this.suggestedProductsIndeterminateWaitVisible(false);
                    this.errorCallback(errors);
                });

            // Load the wish list panel
            this.customerDetailsViewModel.getWishLists(this.customerAccountNumber)
                .done((): void => {
                    this.wishListIndeterminateWaitVisible(false);
                    this.forceWishListLayout()();
                }).fail((errors: Proxy.Entities.Error[]) => {
                    this.wishListIndeterminateWaitVisible(false);
                    this.errorCallback(errors);
                });

            this.numberOfAddressesText = ko.computed(() => {
                var customer: Proxy.Entities.Customer = this.customerDetailsViewModel.Customer();
                if (!ObjectExtensions.isNullOrUndefined(customer)) {
                    var addresses = customer.Addresses;
                    if (ArrayExtensions.hasElements(addresses)) {
                        var numberOfAddresses = addresses.length;
                        return numberOfAddresses == 1
                            ? ViewModelAdapter.getResourceString("string_4843")
                            : StringExtensions.format(ViewModelAdapter.getResourceString("string_4844"), numberOfAddresses);
                    }
                }
                return "";
            }, this);

            this.numberOfAddressesText.subscribe(
                (newValue) => {
                    this.addressFilled((!StringExtensions.isEmptyOrWhitespace(newValue)))
                }, this);

            this.refreshLoyaltyCards();

            this.zeroSuggestedProductsVisible = ko.computed(() => {
                return !ArrayExtensions.hasElements(this.customerDetailsViewModel.suggestedProducts());
            });

            this.zeroWishListVisible = ko.computed(() => {
                return !ArrayExtensions.hasElements(this.customerDetailsViewModel.wishLists());
            });

            this.zeroLoyaltyCardVisible = ko.computed(() => {
                return !ArrayExtensions.hasElements(this.customerDetailsViewModel.loyaltyCards());
            });
        }

        /**
         * Loads the view controller.
         */
        public load(): void {
            this.affiliationsViewModel.load()
                .fail((errors: Proxy.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
        }

        // to-do [v-dabull]: use localized address
        private formatCustomerAddress(addressObject: any): string {
            if (ObjectExtensions.isNullOrUndefined(addressObject) ||
                StringExtensions.isEmptyOrWhitespace(street) ||
                StringExtensions.isEmptyOrWhitespace(city) ||
                StringExtensions.isEmptyOrWhitespace(state) ||
                StringExtensions.isEmptyOrWhitespace(zip)) {
                return "";
            } else {
                var street = addressObject.Street;
                var city = addressObject.City;
                var state = addressObject.State;
                var zip = addressObject.ZipCode;
                return street + ", " + city + ", " + state + " " + zip;
            }
        }

        /**
        * Adds the selected customer and selected products to the session cart.
        */
        private addToCartClick(): void {
            this.indeterminateWaitVisible(true);

            // adds customer to cart
            var asyncQueue: AsyncQueue = new AsyncQueue();
            asyncQueue.enqueue(() => {
                return asyncQueue.cancelOn(this.customerDetailsViewModel.addCustomerToCart([]));
            });

            // adds products to cart
            asyncQueue.enqueue((): IVoidAsyncResult => {
                return asyncQueue.cancelOn(this.addProductsToCartAsyncQueue().run());
            });

            asyncQueue.run()
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done((result) => {
                    if (!result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewsCustomerDetailsAddCustomerFailed();
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private addProductsToCartAsyncQueue(): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            var productIds: number[] = [];

            if (ArrayExtensions.hasElements(this.selectedWishListProducts)) {
                this.selectedWishListProducts.forEach((p: Proxy.Entities.Product) => { productIds.push(p.RecordId); });
            }

            if (ArrayExtensions.hasElements(this.selectedSuggestedProducts)) {
                this.selectedSuggestedProducts.forEach((p: Proxy.Entities.Product) => { productIds.push(p.RecordId); });
            }

            if (ArrayExtensions.hasElements(this.selectedRecentPurchases)) {
                this.selectedRecentPurchases.forEach((s: Proxy.Entities.SalesLine) => { productIds.push(s.ProductId); });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                if (!ArrayExtensions.hasElements(productIds)) {
                    return VoidAsyncResult.createResolved();
                }

                var options: Operations.IItemSaleOperationOptions = {
                    productSaleDetails: productIds.map((v, variantIdIndex): Proxy.Entities.ProductSaleReturnDetails => {
                        return { productId: productIds[variantIdIndex], quantity: 0 };
                    })
                };

                var operationResult = Operations.OperationsManager.instance.runOperation(
                    Operations.RetailOperation.ItemSale, options);

                return asyncQueue.cancelOn(operationResult);
            });

            return asyncQueue;
        }

        private editCustomerClick(): void {
            var options: Operations.ICustomerEditOperationOptions = {
                customer: ObjectExtensions.unwrapObservableProxyObject(this.customerDetailsViewModel.Customer()),
                destination: "CustomerDetailsView",
                destinationOptions: <ICustomerDetailsViewOptions> {
                    accountNumber: this.customerAccountNumber,
                    destination: this._destination,
                    destinationOptions: this._destinationOptions
                }
            };

            Operations.OperationsManager.instance.runOperation(
                Operations.RetailOperation.CustomerEdit, options).fail((error: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error);
                });
        }

        private placeLastVisitedClick() {
            ViewModelAdapter.navigate("StoreDetailsView", { StoreId: this.customerDetailsViewModel.placeOfLastVisitId() });
        }

        /**
         * Refreshes the customer's loyalty cards list.
         */
        private refreshLoyaltyCards() {
            this.loyaltyCardsIndeterminateWaitVisible(true);
            this.customerDetailsViewModel.getLoyaltyCards()
                .always((): void => { this.loyaltyCardsIndeterminateWaitVisible(false); })
                .fail((errors: Proxy.Entities.Error[]) => {
                    this.errorCallback(errors);
                });
        }

        /**
         * Adds the selected loyalty card to the cart.
         * @param {Proxy.Entities.LoyaltyCard} item The loyalty card to add to the cart.
         */
        private loyaltyCardItemInvokedHandler(item: Proxy.Entities.LoyaltyCard): void {
            this.indeterminateWaitVisible(true);
            this.customerDetailsViewModel.addLoyaltyCardToCart(item.CardNumber)
                .always((): void => { this.indeterminateWaitVisible(false); })
                .done((result: ICancelableResult) => {
                    if (!result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((error: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error);
                });
        }

        /**
        * Navigates to the appropriate product details page
        */
        private wishListItemInvokedHandler(item: Proxy.Entities.CommerceList): void {
            ViewModelAdapter.navigate("RecentPurchasesView", { sourceType: ViewControllers.CustomerProductsSourceType.Wishlist, customerAccountNumber: this.customerAccountNumber, customerName: this.customerName, wishlist: item });
        }

        /**
        * Keep track of selected products and enable appropriate buttons
        */
        private wishListSelectionChangedEventHandler(items: Proxy.Entities.Product[]): void {
            // Update selected products
            this.selectedWishListProducts = items;
            this.enableOrDisableAvailableCommandsHelper();
        }

        /**
        * Keep track of selected suggested products and enable appropriate buttons
        */
        private suggestedProductsSelectionChangedEventHandler(items: Proxy.Entities.Product[]): void {
            this.selectedSuggestedProducts = items;
            this.enableOrDisableAvailableCommandsHelper();
        }

        /**
         * Show all suggested products
         */
        private showAllSuggestedProducts(): void {
            var suggestedProducts = this.customerDetailsViewModel.suggestedProducts();
            ViewModelAdapter.navigate("SearchView", { relatedItems: suggestedProducts, searchEntity: "Products" });
        }

        /**
        * Keep track of selected recent purchases and enable appropriate buttons
        */
        private recentPurchasesSelectionChangedEventHandler(items: Proxy.Entities.SalesLine[]): void {
            // Update selected products
            this.selectedRecentPurchases = items;
            this.enableOrDisableAvailableCommandsHelper();
        }

        /**
        * Enable or disable available commands that are bound to the following members after updating product selections
        */
        private enableOrDisableAvailableCommandsHelper() {
            var numItemsSelected = this.selectedRecentPurchases.length + this.selectedWishListProducts.length + this.selectedSuggestedProducts.length;
            this.editCustomerDisabled(numItemsSelected > 0);
        }

        /**
        *  Navigates to the details view for the current customer's recent purchases
        */
        private viewAllRecentPurchases(): void {
            ViewModelAdapter.navigate("RecentPurchasesView", { sourceType: ViewControllers.CustomerProductsSourceType.RecentPurchases, customerAccountNumber: this.customerAccountNumber, customerName: this.customerName });
        }

        private gotoAddNewAddress(): void {
            var address: Proxy.Entities.Address = null;
            var shouldSaveChanges: boolean = true;
            var destination: string = this._destination;
            var options: IAddressAddEditViewCtorOptions =
                AddressAddEditViewCtorOptions.CreateInstance(
                    this.customerDetailsViewModel.Customer(),
                    address,
                    shouldSaveChanges,
                    "CustomerAddEditView",
                    destination,
                    this._destinationOptions);

            ViewModelAdapter.navigate("AddressAddEditView", options);
        }

        private gotoAddressesView(): void {
            var options = {
                customer: this.customerDetailsViewModel.Customer(),
                selectionMode: false
            };
            ViewModelAdapter.navigate("CustomerAddressesView", options);
        }

        private navigateToProductDetailsView(productId: number): void {
            var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
            productDetailOptions.recordId = productId;
            ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
        }

        private productInvokedHandler(product: Proxy.Entities.Product) {
            if (!ObjectExtensions.isNullOrUndefined(product.RecordId)) {
                this.navigateToProductDetailsView(product.RecordId);
            }
        }

        private recentPurchaseInvokedHandler(product: Proxy.Entities.PurchaseHistory) {
            if (!ObjectExtensions.isNullOrUndefined(product.ProductId)) {
                this.navigateToProductDetailsView(product.ProductId);
            }
        }

        private errorCallback(errors: Proxy.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            RetailLogger.viewsCustomerDetailsError(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
        }

        /**
         * Refreshes order history panel with full list of sales orders
         */
        public showAllOrderHistory(): void {
            if (!Commerce.StringExtensions.isEmptyOrWhitespace(this.customerAccountNumber)) {

                ViewModelAdapter.navigate("ShowJournalView", <IShowJournalViewControllerOptions>{ CustomerAccountNumber: this.customerAccountNumber, IsCustomerSalesOrdersMode: true });
            }
        }

        /**
         * Navigates to the appropriate search receipt page
         */
        public clickReceiptCommand(data: Proxy.Entities.SalesOrder) {

            if (!Commerce.ObjectExtensions.isNullOrUndefined(data) && !Commerce.ObjectExtensions.isNullOrUndefined(data.SalesId)) {
                var salesOrderSearchCriteria: Proxy.Entities.SalesOrderSearchCriteria = new Proxy.Entities.SalesOrderSearchCriteriaClass();
                if (!StringExtensions.isEmptyOrWhitespace(data.SalesId)) {
                    salesOrderSearchCriteria.SalesId = data.SalesId;
                }
                if (!StringExtensions.isEmptyOrWhitespace(data.Id)) {
                    salesOrderSearchCriteria.TransactionIds = [data.Id];
                }
                salesOrderSearchCriteria.SalesTransactionTypeValues = [data.TransactionTypeValue];
                salesOrderSearchCriteria.SearchLocationTypeValue = Proxy.Entities.SearchLocation.All;
                ViewModelAdapter.navigate("ShowJournalView", <IShowJournalViewControllerOptions>{ SearchCriteria: salesOrderSearchCriteria, IsShowJournalMode: false });
            }
        }


        /**
         * Shows order history 'Sort by' menu
         */
        public showSortByPopup(): void {
            this.toggleShowHideMenu()();
        }

        /**
         * Add affiliation to customer.
         */
        public addAffiliationToCustomer() {
            var allAffiliations: Proxy.Entities.Affiliation[] = this.affiliationsViewModel.affiliations();

            if (ArrayExtensions.hasElements(allAffiliations)) {

                var affiliationNamesDictonary: Dictionary<Proxy.Entities.Affiliation> = new Dictionary<Proxy.Entities.Affiliation>();
                allAffiliations.forEach((affiliationItem: Proxy.Entities.Affiliation) => {
                    if (!this.customerDetailsViewModel.isAlreadyInCustomer(affiliationItem.RecordId)) {
                        affiliationNamesDictonary.setItem(affiliationItem.Name, affiliationItem);
                    }
                });

                this._autocompleteControl.title(ViewModelAdapter.getResourceString("string_6306"));
                this._autocompleteControl.subTitle(ViewModelAdapter.getResourceString("string_6305"));

                var affiliationDataSource: Commerce.Controls.AutocompleteDataItem[] = [];
                affiliationNamesDictonary.getItems().forEach((affiliation: Proxy.Entities.Affiliation) => {
                    var dataItem: Commerce.Controls.AutocompleteDataItem = new Commerce.Controls.AutocompleteDataItem();
                    dataItem.value = affiliation.Name;
                    dataItem.description = affiliation.Description;
                    affiliationDataSource.push(dataItem);
                });

                this._autocompleteControl.show({ dataSource: affiliationDataSource })
                    .on(DialogResult.OK, (selectedText: string) => {

                        var selectedAffiliation: Proxy.Entities.Affiliation = affiliationNamesDictonary.getItem(selectedText);

                        if (!ObjectExtensions.isNullOrUndefined(selectedAffiliation)) {
                            var customerAffiliation: Proxy.Entities.CustomerAffiliationClass = new Proxy.Entities.CustomerAffiliationClass();
                            customerAffiliation.RetailAffiliationId = selectedAffiliation.RecordId;

                            // Add the selected affiliation to customer.
                            this.indeterminateWaitVisible(true);
                            this.customerDetailsViewModel.addAffiliationToCustomer(customerAffiliation)
                                .always((): void => {
                                    this.indeterminateWaitVisible(false);
                                }).fail((errors: Proxy.Entities.Error[]) => {
                                    NotificationHandler.displayClientErrors(errors);
                                });
                        }
                    });
            }
        }

        /**
         * Navigate to CustomerAffiliation Detail page to show all the customer's affiliations.
         */
        private showAllCustomerAffiliations(): void {
            var options: any = {
                customerAffiliations: this.customerDetailsViewModel.customerAffiliations(),
                customerProxy: this.customerDetailsViewModel.Customer(),
                addressProxy: this.customerDetailsViewModel.customerAddress()
            }
            ViewModelAdapter.navigate("CustomerAffiliationsView", options);
        }

        private refreshWishListPanel(): void {
            this.wishListIndeterminateWaitVisible(true)

            // Load the wish list panel
            this.customerDetailsViewModel.getWishLists(this.customerAccountNumber)
                .done(() => {
                    this.wishListIndeterminateWaitVisible(false);
                })
                .fail((errors) => {
                    this.wishListIndeterminateWaitVisible(false);
                    this.errorCallback(errors);
                });
        }

         /**
         * Opens email href.
         */
        private openEmailHref(): void {
            window.open(this.customerEmailHref());
        }

         /**
         * Opens phone href.
         */
        private openPhoneHref(): void {
            window.open(this.customerPhoneHref());
        }
    }
}
