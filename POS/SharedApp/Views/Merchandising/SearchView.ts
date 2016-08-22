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
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    import Entities = Proxy.Entities;

    export class SearchViewController extends ViewControllerBase {
        
        public commonHeaderData: Controls.CommonHeaderData;
        public viewModel: Commerce.ViewModels.SearchViewModel;
        public toggleShowHideProductsMenu: Observable<(() => void)>;
        public toggleShowHideCustomersMenu: Observable<(() => void)>;

        //dialogs
        private _variantDialog: Controls.VariantDialog;

        private _previousPageData: any;
        private _customerDestinationPage: string = "CartView";
        private _customerDestinationPageOptions: any = null;

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
         * {saveInHistory} [bool?] Identifies whether we need to store this page in history. The page is ignored on going back through navigation stack if false.
         *
         * Comment:
         * For some tasks, options is sent as a parameter to the next page in navigation and may contain additional
         * properties not used by this view.
         */
        constructor(options: any) {
            super(ObjectExtensions.isNullOrUndefined(options)
                || !ObjectExtensions.isBoolean(options.saveInHistory) ? true : options.saveInHistory);

            // All code except related to options and navigation should exist in view model.
            this.viewModel = new ViewModels.SearchViewModel(options);

            // Initialize view model handlers
            this.viewModel.onCustomerAddedToCartHandler = this.onCustomerAddedToCart.bind(this);
            this.viewModel.showCustomerDetailsHandler = this.showCustomerDetails.bind(this);
            this.viewModel.sendItemToCallbackHandler = this.sendItemToCallback.bind(this);
            this.viewModel.sendCustomerToPriceCheckHandler = this.sendCustomerToPriceCheck.bind(this);
            this.viewModel.issueLoyaltyCardHandler = this.issueLoyaltyCard.bind(this);
            this.viewModel.onReasonCodeResolvedHandler = this.onReasonCodeResolved.bind(this);
            this.viewModel.showCartHandler = this.showCart.bind(this);
            this.viewModel.changeStoreAndCatalogsHandler = this.changeStoreAndCatalogs.bind(this);
            this.viewModel.showProductDetailsHandler = this.showProductDetails.bind(this);
            this.viewModel.compareItemsHandler = this.compareItems.bind(this);

            this._previousPageData = options || {};
            
            // Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.resultCount("");
            this.commonHeaderData.searchText(this.viewModel.searchText);
            this.commonHeaderData.searchText.subscribe((newValue: string) => this.viewModel.searchText = newValue, this);            
            this.commonHeaderData.selectedSearchLocation.subscribe((newValue: Entities.SearchLocation) => this.viewModel.selectedSearchLocation = newValue, this);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_1000")); // SEARCH RESULTS
            this.commonHeaderData.enableVirtualCatalogHeader();

            // Initialize common header handlers
            this.commonHeaderData.selectVirtualCatalogs = this.viewModel.changeStoreAndCatalogs.bind(this.viewModel);
            this.commonHeaderData.applyRefinerHandler = this.viewModel.applyRefinerValues.bind(this.viewModel);
            this.commonHeaderData.getRefinersHandler = this.viewModel.getRefinersHandler.bind(this.viewModel);
            this.commonHeaderData.getRefinerValuesHandler = this.viewModel.getRefinerValuesHandler.bind(this.viewModel);

            this.viewModel.refinerProductSearchDetailsChangedHandler = this.commonHeaderData.refinerProductSearchDetailsChangedHandler.bind(this.commonHeaderData);
            this.viewModel.canRefineItems.subscribe((newValue: boolean) => this.commonHeaderData.enableFilterButton(newValue), this);
            this.viewModel.isSearchWithLocationAvailable.subscribe((newValue: boolean) => this.commonHeaderData.enableSearchLocationMenu(newValue), this);

            if (options.searchEntity === ViewModels.SearchViewModel.productSearchEntity) {
                if (ObjectExtensions.isFunction(this.commonHeaderData.refinerProductSearchDetailsChangedHandler)) {
                    this.commonHeaderData.refinerProductSearchDetailsChangedHandler();
                }
            } else {
                // Check for customer destination page
                if (!StringExtensions.isNullOrWhitespace(options.destination)) {
                    this._customerDestinationPage = options.destination;
                    this._customerDestinationPageOptions = options.destinationOptions;
                }
            }

            this.commonHeaderData.searchClick = () => {
                RetailLogger.viewsMerchandisingSearchViewSearchClick(this.commonHeaderData.searchText());
                this.viewModel.searchItems(true);
            };

            // This step is necessary to broadcast the initial state of isSearchWithLocationAvailable
            // (The value is set when viewModel is constructed, before any subscribers are added)
            this.viewModel.isSearchWithLocationAvailable.valueHasMutated();

            this.populateSearchLocationsMenu();
            this.toggleShowHideProductsMenu = ko.observable((): void => { return; });
            this.toggleShowHideCustomersMenu = ko.observable((): void => { return; });
            
            // Load controls
            this.addControl(this._variantDialog = new Controls.VariantDialog());
        }

        /**
         * Occurs when the element of the page is created.
         *
         * @param {HTMLElement} element DOM element.
         */
        public onCreated(element: HTMLElement) {
            super.onCreated(element);

            var matchedProductElements = $(element).find("#productsview");
            var productsListView: HTMLElement = matchedProductElements[0];
            this.viewModel.resetProductsHandler = (() => {
                ko.applyBindingsToNode(productsListView, null, this.viewModel);
            });

            var matchedCustomerElements = $(element).find("#customerSearchResultListView");
            var customersListView: HTMLElement = matchedCustomerElements[0];
            this.viewModel.resetCustomersHandler = (() => {
                ko.applyBindingsToNode(customersListView, null, this.viewModel);
            });
        }


        /**
         * Called when view is shown.
         */
        public onShown() {
            //Enable extended logon with barcode
            Commerce.Peripherals.instance.barcodeScanner.enableAsync(
                (barcode) => {
                    if (!StringExtensions.isNullOrWhitespace(barcode)) {
                        this.commonHeaderData.searchText(barcode);
                        this.viewModel.searchItems(true);
                    }
                });
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            // Disable barcode scanner.
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
            this.viewModel.rejectUnresolvedReasonCodeResult();
        }

        /*
         *   Switches products view between templates
         */
        private showProductsViewMenu(): void {
            this.toggleShowHideProductsMenu()();
        }

        /*
         *   Switches customers view between templates
         */
        private showCustomersViewMenu(): void {
            this.toggleShowHideCustomersMenu()();
        }

        /**
         * Adds the available search location options to the search locations menu.
         */
        private populateSearchLocationsMenu(): void {
            this.commonHeaderData.searchLocations.removeAll();

            var searchLocations: ViewModels.ISearchLocation[] = this.viewModel.searchLocations;
            if (ArrayExtensions.hasElements(searchLocations)) {
                searchLocations.forEach((searchLocation: ViewModels.ISearchLocation) => {
                    this.commonHeaderData.searchLocations.push(searchLocation)
                });
            }
        }

        /**
         * Navigates to product details view
         * @param {{ RecordId: number, IsKit?: boolean }} product The product.
         * @param {ViewModels.ProductAddModeEnum} [productAddModeEnum] The product add mode to be sent back to caller.
         * @param {number} [quantity] The quantity to be sent back to caller.
         */
        private showProductDetails(
            product: { RecordId: number, IsKit?: boolean },
            productAddModeEnum?: ViewModels.ProductAddModeEnum,
            quantity?: number): void {

            if (!ObjectExtensions.isNullOrUndefined(product)) {
                var callbackParameters: any = this._previousPageData;
                callbackParameters.RecordId = product.RecordId;
                callbackParameters.IsKit = product.IsKit;

                var productDetailOptions: ProductDetailOptions = new ProductDetailOptions();
                productDetailOptions.recordId = callbackParameters.RecordId;
                productDetailOptions.pageCallback = callbackParameters.pageCallback;
                productDetailOptions.additionalParams = callbackParameters;

                if (!ObjectExtensions.isNullOrUndefined(productAddModeEnum)) {
                    productDetailOptions.productAddModeEnum = productAddModeEnum;
                } else if (!ObjectExtensions.isNullOrUndefined(callbackParameters.addModeEnum)) {
                    productDetailOptions.productAddModeEnum = callbackParameters.addModeEnum;
                }

                if (ObjectExtensions.isNumber(quantity)) {
                    productDetailOptions.quantity = quantity;
                }

                Commerce.ViewModelAdapter.navigate("ProductDetailsView", productDetailOptions);
            }
        }

        /**
         * Navigates to Catalogs View for user to select a different store and Catalog
         * @param {string} searchText The search text to send back to the caller.
         * @param {string} searchEntity The search entity to send back to the caller.
         */
        private changeStoreAndCatalogs(searchText: string, searchEntity: string): void {
            this._previousPageData.searchText = searchText;
            this._previousPageData.searchEntity = searchEntity;

            var parameters: ICatalogsViewControllerOptions = {
                destination: "SearchView",
                destinationOptions: this._previousPageData
            };
            Commerce.ViewModelAdapter.navigate("CatalogsView", parameters);
        }

        /**
         * Compare items from the specified product identifiers.
         * @param {number[]} productIds The product identifiers.
         */
        private compareItems(productIds: number[]): void {
            Commerce.ViewModelAdapter.navigate("CompareProductsView", { items: productIds });
        }

        /**
         * Navigates to issue loyalty card page.
         * @param {Entities.GlobalCustomer} customer @param {Entities.GlobalCustomer} customer The customer to whom the loyalty card to be issued.
         */
        private issueLoyaltyCard(customer: Entities.GlobalCustomer): void {
            this._previousPageData.customer = customer;
            Commerce.ViewModelAdapter.navigate("IssueLoyaltyCardView", this._previousPageData);
        }

        /**
         * Sends the specified item to the callback page.
         * @param {Entities.Product} item The item.
         */
        private sendItemToCallback(item: Entities.Product): void {
            this._previousPageData.Product = item;
            Commerce.ViewModelAdapter.navigate(this._previousPageData.pageCallback, this._previousPageData);
        }

        /**
         * Add new customer.
         */
        private addNewCustomer(): void {
            var options: Operations.ICustomerAddOperationOptions = {
                destination: this._customerDestinationPage,
                destinationOptions: this._customerDestinationPageOptions
            };

            this.viewModel.addNewCustomer(options);
        }

        /**
         * Shows the customer details.
         * @param {string} customerAccountNumber The account number of the customer.
         */
        private showCustomerDetails(customerAccountNumber: string): void {
            var viewOptions: ICustomerDetailsViewOptions = {
                accountNumber: customerAccountNumber,
                destination: this._customerDestinationPage,
                destinationOptions: this._customerDestinationPageOptions
            };

            Commerce.ViewModelAdapter.navigate("CustomerDetailsView", viewOptions);
        }

        /**
         * Sends customer to price check view.
         * @param {Entities.GlobalCustomer} customer The customer.
         */
        private sendCustomerToPriceCheck(customer: Entities.GlobalCustomer): void {
            this._previousPageData.customer = customer;
            Commerce.ViewModelAdapter.navigate("PriceCheckView", this._previousPageData);
        }

        /**
         * Shows to cart page.
         */
        private showCart(): void {
            Commerce.ViewModelAdapter.navigate("CartView");
        }

        /**
         * Navigates back to the caller page.
         */
        private onReasonCodeResolved() {
            Commerce.ViewModelAdapter.navigateBack(this);
        }

        /**
         * Navigates to customer destination page.
         */
        private onCustomerAddedToCart(): void {
            Commerce.ViewModelAdapter.navigate(this._customerDestinationPage, this._customerDestinationPageOptions);
        }
    }
}