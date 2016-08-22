/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/RefinerControl.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.Controls {
    "use strict";

    import Entities = Proxy.Entities;

    /**
     * Specifies the initialization data for the search validator
     */
    export class CommonHeaderSearchValidator {
        public data: CommonHeaderData;
        public validatorField: string;     // Name of the property of the element to search
        public validatorType: string;      // Name of the IEntityValidator containing the search validation method

        public constructor(data?: CommonHeaderData) {
            this.data = data;
            this.validatorField = "SearchText";
            this.validatorType = "SearchValidator";
        }
    }

    export class CommonHeaderData implements IDisposable {
        // Common header content
        public sectionTitle: Observable<string>;
        public sectionInfo: Observable<string>;
        public categoryName: Observable<string>;
        public catalogName: Observable<string>;
        public searchText: Observable<string>;
        public selectVirtualCatalogs: any;
        public navigateToProductsView: any;
        public searchValidator: CommonHeaderSearchValidator;
        public viewSectionInfo: Observable<boolean>;
        public resultCount: Observable<string>;
        public navBarSearchText: Observable<string>;
        public searchLocations: ObservableArray<ViewModels.ISearchLocation>;
        public selectedSearchLocation: Observable<Entities.SearchLocation>;

        // Common header visiblity variables
        public viewCommonHeader: Observable<boolean>;
        public viewCategoryName: Observable<boolean>;
        public viewSearchBox: Observable<boolean>;
        public enableSearchLocationMenu: Observable<boolean>;
        public viewHeader: Observable<boolean>;
        public backButtonVisible: Observable<boolean>;
        public enableFilterButton: Observable<boolean>;
        public lockRegisterMenuItemVisible: Observable<boolean>;
        public connectivityButtonVisible: Observable<boolean>;
        public settingsButtonVisible: Observable<boolean>;
        public userAditionalInformationVisible: Observable<boolean>;
        
        // Navigation bar variables
        public viewNavigationBar: Observable<boolean>;
        public viewNavigationBarExtension: Observable<boolean>;
        public viewCartLineCount: Observable<boolean>;

        public searchClick: () => void;
        public commonHeaderData: CommonHeaderData;
        public applyClearRefiner: any;

        // User Grid Variables
        private menuVisible: Observable<boolean>;

        // Navigation bar extension Variables
        private navBarSearch: Observable<boolean>;
        private navBarCategories: Observable<boolean>;
        private navBarFilter: Observable<boolean>;
        private navBarMiniCart: Observable<boolean>;

        // MiniCart
        public miniCart: Observable<Entities.Cart>;
        public miniCartItemCount: Observable<string>;
        public currentCartItemCount: Observable<string>;
        private static miniCartProductPrimaryImageUriMap: Dictionary<Observable<string>> = null;

        // Refiner Values and handlers
        public refinerProductSearchDetailsChangedHandler: (() => void);
        public resetProductRefinersHandler: Observable<(() => void)>;
        private _isRefinerCriteriaUpdated: boolean = false;

        public applyRefinerHandler: (productRefinerValues: Entities.ProductRefinerValue[]) => void;
        public getRefinersHandler: (() => IAsyncResult<Entities.ProductRefiner[]>);
        public getRefinerValuesHandler: ((productRefiner: Entities.ProductRefiner) => IAsyncResult<Entities.ProductRefinerValue[]>);
        public _productRefinerControl: Controls.RefinerControl;

        private _cartUpdateHandler: any;
        public updateMiniCartCount: any;

        // Connection status Variables
        public connectionStatusIsDisconnected: Observable<boolean>;

        // Category tree structure
        public categoryTree: ObservableArray<any>;

        public controlElement: Observable<HTMLElement>;
        public userInfoMenu: Observable<HTMLElement>;
        public connectionInfoMenu: Observable<HTMLElement>;
        public userInfoMenuAnchor: Observable<HTMLElement>;
        public connectionInfoMenuAnchor: Observable<HTMLElement>;
        public navigationActionsVisible: Computed<boolean>;
        public navigationBarVisible: Observable<boolean>;
        public expandButtonVisible: Observable<boolean>;

        private isUserLoggedOn: Computed<boolean>;
        private indeterminateWaitVisible: Observable<boolean>;

        constructor() {
            // Common header content
            var self = this;
            this.sectionTitle = ko.observable("");
            this.sectionInfo = ko.observable("");
            this.categoryName = ko.observable("");
            this.searchText = ko.observable("");
            this.selectVirtualCatalogs = null;
            this.searchValidator = new CommonHeaderSearchValidator(this);
            this.resultCount = ko.observable("");
            this.searchLocations = ko.observableArray([]);
            this.selectedSearchLocation = ko.observable(null);

            // Common header visiblity variables
            this.viewCommonHeader = ko.observable(false);
            this.viewCategoryName = ko.observable(false);
            this.viewSearchBox = ko.observable(false);
            this.enableSearchLocationMenu = ko.observable(false);
            this.viewHeader = ko.observable(true);
            this.viewSectionInfo = ko.observable(true);
            this.backButtonVisible = ko.observable(true);
            this.viewCartLineCount = ko.observable(true);
            this.categoryTree = ko.observableArray([]);
            this.lockRegisterMenuItemVisible = ko.observable(true);
            this.connectivityButtonVisible = ko.observable(true);
            this.settingsButtonVisible = ko.observable(true);
            this.userAditionalInformationVisible = ko.observable(true);

            this.controlElement = ko.observable(null);
            this.userInfoMenu = ko.observable(null);
            this.connectionInfoMenu = ko.observable(null);
            this.userInfoMenuAnchor = ko.observable(null);
            this.connectionInfoMenuAnchor = ko.observable(null);
            this.resetProductRefinersHandler = ko.observable(null);

            this.enableFilterButton = ko.observable(false);
            this.enableFilterButton.subscribe((value: boolean) => {
                if (value && Commerce.ApplicationContext.Instance.storeInformation.RecordId != Commerce.Session.instance.productCatalogStore.Store.RecordId) {
                    this.enableFilterButton(false);
                }
            });

            if (StringExtensions.isNullOrWhitespace(Session.instance.catalogName)) {
                Session.instance.catalogName = ViewModelAdapter.getResourceString("string_33"); //All store products
            }

            this.catalogName = ko.observable(Session.instance.catalogName);

            this.currentCartItemCount = ko.observable(Commerce.Session.instance.cart.CartLines ? this.GetNonVoidedCartLinesCount().toString() : '0');
            this._cartUpdateHandler = () => {
                this.viewCartLineCount(!this.viewCartLineCount());
            };

            this.controlElement.subscribe((newValue: HTMLElement) => {
                Commerce.Session.instance.AddCartStateUpdateHandler(newValue, this._cartUpdateHandler.bind(this));
            });

            // If the product primary image uri map hasn't been initalized yet, initialize it.
            if (ObjectExtensions.isNullOrUndefined(CommonHeaderData.miniCartProductPrimaryImageUriMap)) {
                CommonHeaderData.miniCartProductPrimaryImageUriMap = new Dictionary<Observable<string>>();
            }

            this.updateMiniCartCount = () => {
                if (!ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.cart) && ArrayExtensions.hasElements(Commerce.Session.instance.cart.CartLines)) {
                    if (this.GetNonVoidedCartLinesCount() > 999) {
                        self.currentCartItemCount('999+');
                    } else {
                        self.currentCartItemCount(this.GetNonVoidedCartLinesCount().toString());
                    }
                } else {
                    self.currentCartItemCount('0');
                    CommonHeaderData.miniCartProductPrimaryImageUriMap.clear();
                }
            };

            this.isUserLoggedOn = ko.computed((): boolean => {
                // session property uses observable so computed works on it
                return Session.instance.isLoggedOn;
            });


            // Common header click handlers
            this.searchClick = () => { };

            this.navBarSearchText = ko.observable("");

            // User grid Variables
            this.menuVisible = ko.observable(false);

            // Extension Variables
            this.viewNavigationBarExtension = ko.observable(false);
            this.navBarSearch = ko.observable(false);
            this.navBarCategories = ko.observable(false);
            this.navBarFilter = ko.observable(false);
            this.navBarMiniCart = ko.observable(false);
            this.expandButtonVisible = ko.observable(false);
            this.navigationBarVisible = ko.observable(true);            

            // Navigation bar variables
            this.viewNavigationBar = ko.observable(true);

            this.indeterminateWaitVisible = ko.observable(false);

            // miniCart
            this.miniCart = ko.observable(null);

            this.miniCartItemCount = ko.observable(Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_45"),
                0)); // "{0} line items "

            this.applyClearRefiner = (productRefinerValues: Entities.ProductRefinerValue[]) => {
                this.hideExtensionBar(true);
                this.applyRefinerHandler(productRefinerValues);
            };

            this.navigateToProductsView = (data: any) => {
                RetailLogger.viewsControlsCommonHeaderCategoryInTreeClicked(data.value.Name);
                Commerce.ViewModelAdapter.navigate("ProductsView", <ViewControllers.IProductsViewOptions>{ category: data.value, activeMode: Commerce.ViewModels.ProductsViewModelActiveMode.Products });
                self.categoryTree([]);
                self.hideExtensionBar(true);
            };

            this.refinerProductSearchDetailsChangedHandler = (() => {
                this._isRefinerCriteriaUpdated = true;
            });

            this.connectionStatusIsDisconnected = ko.observable(Commerce.Session.instance.connectionStatus != Commerce.ConnectionStatusType.Online);
            Commerce.Session.instance.connectionStatusAsString.subscribe((newValue) => {
                this.connectionStatusIsDisconnected(Commerce.Session.instance.connectionStatus != Commerce.ConnectionStatusType.Online);
            }, this);

            this.navigationActionsVisible = ko.computed((): boolean => {
                return this.viewNavigationBar() && Session.instance.isLoggedOn;
            });

        }

        /**
         * Toggles all visible elements on or off in the header.
         * @param {boolean} visibility a value indicating whether elements should be visible or not.
         */
        public toggleAllElementsVisibility(visible: boolean): void {
            this.viewCommonHeader(visible);
            this.viewCategoryName(visible);
            this.viewSearchBox(visible);
            this.viewHeader(visible);
            this.viewSectionInfo(visible);
            this.backButtonVisible(visible);
            this.viewCartLineCount(visible);
            this.lockRegisterMenuItemVisible(visible);
            this.connectivityButtonVisible(visible);
            this.settingsButtonVisible(visible);
            this.userAditionalInformationVisible(visible);
            this.navigationBarVisible(visible);
        }

        /**
         * Click handler for navigating to store details
         *
         */
        public navigateLink(): void {
            Commerce.ViewModelAdapter.navigate("CatalogsView");
        }

        /**
         * Click handler for handling off clicks outside the navigation bar extension 
         * when the navigation extension is shown
         *
         */
        public offScreenClickHandler(): void {
            this.commonHeaderData.hideExtensionBar(true);
        }

        /**
         * Method for computed subtitle used in cartview
         *
         */
        public setTitleAsComputed(computableTitle: Computed<string>) {
            // register subscription
            computableTitle.subscribe((newTitle: string) => { this.sectionTitle(newTitle); });

            // computed might have already been trigered, update title
            this.sectionTitle(computableTitle());
        }

        /**
         * Subtitle formatter for displaying the store and catalog
         *
         */
        public enableVirtualCatalogHeader(): void {
            this.sectionInfo(Commerce.ProductCatalogStoreHelper.getStoreHeaderDetails());
        }

        /**
         * Method for hiding navigation bar
         *
         */
        public showHideNavigationBar(): void {
            if (this.commonHeaderData.viewNavigationBar()) {
                this.commonHeaderData.viewNavigationBar(false);
                this.commonHeaderData.hideExtensionBar(true);
            } else {
                this.commonHeaderData.viewNavigationBar(true);
            }
        }

        /**
         * Click handler for change catalog button in the navigation bar extension
         *
         */
        public navigateToCatalogsView(): void {
            this.commonHeaderData.hideExtensionBar(true);
            Commerce.ViewModelAdapter.navigate("CatalogsView");
        }

        /**
         * Click handler for catagories button in navigation bar
         *
         */
        public navigateToCatagoriesView(): void {
            this.commonHeaderData.hideExtensionBar(true);
            Commerce.ViewModelAdapter.navigate("CategoriesView");
        }

        /**
         * Click handler for home button in navigation bar
         *
         */
        public navigateHome(): void {
            this.commonHeaderData.hideExtensionBar(true);
            Commerce.ViewModelAdapter.navigate("HomeView");

        }

        /**
         * Click handler for search button in navigation bar
         *
         */
        public navSearch(): void {
            this.commonHeaderData.navBarSearch() ? this.commonHeaderData.hideExtensionBar(true) : this.commonHeaderData.hideAllShowthis(this.commonHeaderData.navBarSearch);

        }

        /**
         * Click handler for categories tree view in navigation bar
         *
         */
        public navCategories(): void {
            this.commonHeaderData.navBarCategories() ? this.commonHeaderData.hideExtensionBar(true) : this.commonHeaderData.hideAllShowthis(this.commonHeaderData.navBarCategories);
            if (this.commonHeaderData.categoryTree().length == 0) {
                this.commonHeaderData.categoryTree(Commerce.Session.instance.categoryTree());
            }
        }

        /**
         * Click handler for filter menu in navigation bar
         *
         */
        public navFilter(): void {
            RetailLogger.viewsControlsCommonHeaderFilterIconClick();

            this.commonHeaderData.navBarFilter() ? this.commonHeaderData.hideExtensionBar(true) : this.commonHeaderData.hideAllShowthis(this.commonHeaderData.navBarFilter);

            if (this.commonHeaderData._isRefinerCriteriaUpdated) {
                if (ObjectExtensions.isFunction(this.commonHeaderData.resetProductRefinersHandler())) {
                    this.commonHeaderData.resetProductRefinersHandler()();
                }
                this.commonHeaderData._isRefinerCriteriaUpdated = false;
            }
        }

        /**
         * Click handler for displaying mini cart in navigation bar
         *
         */
        public navMiniCart(): void {
            this.commonHeaderData.miniCart(Commerce.Session.instance.cart);

            if (ArrayExtensions.hasElements(this.commonHeaderData.miniCart().CartLines)) {
                var totalCartLineItems: number = CartHelper.GetNonVoidedCartLines(this.commonHeaderData.miniCart().CartLines).length;
                if (totalCartLineItems == 1) {
                    this.commonHeaderData.miniCartItemCount(Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_45"),
                        totalCartLineItems)); //"{0} line item "
                } else {
                    this.commonHeaderData.miniCartItemCount(Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_46"),
                        totalCartLineItems)); //"{0} line items "
                }
            } else {
                this.commonHeaderData.miniCartItemCount(Commerce.StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_46"),
                    0)); // "{0} line items "
            }
            this.commonHeaderData.navBarMiniCart() ? this.commonHeaderData.hideExtensionBar(true) : this.commonHeaderData.hideAllShowthis(this.commonHeaderData.navBarMiniCart);

        }

        /**
         * Click handler for cart button navigation bar
         *
         */
        public navigateToCart(): void {
            this.commonHeaderData.hideExtensionBar(true);
            Commerce.ViewModelAdapter.navigate("CartView");
        }

        /**
         * Click handler for navigation to settings page
         *
         */
        public navigateToSettings(): void {
            Commerce.ViewModelAdapter.navigate("SettingsView");
        }

        /**
         * Click hander for activating help panel
         *
         */
        public showHelpViewPanel(): void {
            Commerce.TaskRecorder.activateTaskRecorder("Help");
        }

        /**
         * Disable Virtual CatalogHeader in the subtitle
         *
         */
        public disableVirtualCatalogHeader(): void {
            this.sectionInfo("");
        }

        /**
         * Click handler to show user menu
         */
        public showUserMenu() {
            this.userInfoMenu().winControl.show(this.userInfoMenuAnchor());
        }

        /**
         * Click handler to show connection information.
         */
        public showConnectionInfo() {
            this.connectionInfoMenu().winControl.show(this.connectionInfoMenuAnchor());
        }

        /**
         * Click handler for lock register button in the user menu
         *
         */
        public lockRegister() {
            var lockRegisterViewOptions: ViewControllers.ILockRegisterViewControllerOptions = {
                OperatorId: Commerce.Session.instance.CurrentEmployee.StaffId
            };

            // Remove the token due to security issues and redirect user to lock register view.
            Utilities.LogonHelper.logoffAuthenticationProvider().always(() => {
                this.commonHeaderData.menuVisible(false);
                Commerce.ViewModelAdapter.navigate("LockRegister", lockRegisterViewOptions);
            });
        }

        /**
         * Click handler for log off button in the user menu
         *
         */
        public logOff() {
            this.menuVisible(false);
            var loginViewModel: Commerce.ViewModels.LoginViewModel = new Commerce.ViewModels.LoginViewModel();

            this.indeterminateWaitVisible(true);

            loginViewModel.logOff(this)
                .fail((error: Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(error, Commerce.ViewModelAdapter.getResourceString("string_509")); // Logoff error
                }).always((): void => {
                    this.indeterminateWaitVisible(false);
                });
        }

        /**
         * Click handler for search button in the navigation bar extension
         *
         */
        public navBarSearchHandler() {
            RetailLogger.viewsControlsCommonHeaderSearch(this.commonHeaderData.navBarSearchText());
            this.commonHeaderData.hideExtensionBar(true);
            Commerce.ViewModelAdapter.navigate("SearchView", { searchText: this.commonHeaderData.navBarSearchText(), searchEntity: "Products" });
        }

        /**
         * Method for hidden all extension and showing a particular navbar extension
         *
         */
        private hideAllShowthis(sectionVisible: Observable<boolean>) {
            this.hideExtensionBar(false);
            sectionVisible(true);
            this.viewNavigationBarExtension(true);
        }


        /**
         * Click handler to hide the navigation bar
         *
         * @param {boolean} Indicates if extension has to be hidden
         */
        public hideExtensionBar(hideExtension: boolean) {
            this.navBarSearch(false);
            this.navBarCategories(false);
            this.navBarFilter(false);
            this.navBarMiniCart(false);
            if (hideExtension) {
                this.viewNavigationBarExtension(false);
            }
        }

        /**
         * Returns the number of non voided cart lines.
         * @return {number} The number of cart lines that are not voided.
         */
        private GetNonVoidedCartLinesCount(): number {
            return CartHelper.GetNonVoidedCartLines(Commerce.Session.instance.cart.CartLines).length;
        }

        /**
         * Called when the resources need to be disposed.
         */
        public dispose(): void {
            ObjectExtensions.disposeAllProperties(this);
        }

        /**
         * Gets the primary image URI for the product associated with the specified product identifier.
         * @param {number} productId The product identifier.
         * @return {Observable<string>} Observable containing the image location.
         */
        private static getProductPrimaryImageUri(productId: number): Observable<string> {
            var primaryImageUri: Observable<string> = CommonHeaderData.miniCartProductPrimaryImageUriMap.getItem(productId);

            // If the primary image URI was not found, then initialize the observable with the default product image, add it to the map,
            // and make the call to retrieve the media location.
            if (ObjectExtensions.isNullOrUndefined(primaryImageUri)) {
                primaryImageUri = ko.observable(Commerce.DefaultImages.ProductSmall);
                CommonHeaderData.miniCartProductPrimaryImageUriMap.setItem(productId, primaryImageUri);

                if (Session.instance.connectionStatus === ConnectionStatusType.Online) {
                    var productManager: Model.Managers.IProductManager =
                        Model.Managers.Factory.getManager<Model.Managers.IProductManager>(Model.Managers.IProductManagerName);

                    var channelId: number = Commerce.Session.instance.productCatalogStore.Context.ChannelId;
                    var catalogId: number = Commerce.Session.instance.productCatalogStore.Context.CatalogId;
                    productManager.getMediaLocationsAsync(productId, channelId, catalogId, 1, 0).done((mediaLocations: Proxy.Entities.MediaLocation[]): void => {
                        var mediaLocation: Proxy.Entities.MediaLocation = ArrayExtensions.firstOrUndefined(mediaLocations);
                        // If a media location was found and it has a URI, then update the observable value.
                        if (!ObjectExtensions.isNullOrUndefined(mediaLocation) && !StringExtensions.isEmptyOrWhitespace(mediaLocation.Uri)) {
                            primaryImageUri(Commerce.Formatters.ImageUrlFormatter(mediaLocation.Uri));
                        }
                    });
                }
            }

            return primaryImageUri;
        }
    }
}