/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Controls/Bing.Maps/Bing.Maps.ts'/>
///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path="../INavigationReturnOptions.ts" />
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export interface IPickUpInStoreViewControllerOptions {
        isForPickUp: boolean;
        callerPage: string;
        storeSelectionCallback: (store: Model.Entities.OrgUnit) => IAsyncResult<any>; // Function called when the store selection occurs.
        cartLines?: Model.Entities.CartLine[];
        selectedStore?: Model.Entities.StoreLocationWrapper;
    }

    export class PickUpInStoreViewController extends ViewControllerBase {

        private static STORE_ID_KEY: string = "STORE_ID";
        private static DEFAULT_DISTANCE_UNIT: Model.Entities.DistanceUnit = Model.Entities.DistanceUnit.Miles;
        private static EARTH_AREA: Model.Entities.SearchArea = new Model.Entities.SearchAreaClass({
            Longitude: 0,
            Latitude: 0,
            Radius: 0, // 0 means no radius constraint
            DistanceUnitValue: PickUpInStoreViewController.DEFAULT_DISTANCE_UNIT
        });

        private _locations: ObservableArray<Model.Entities.StoreLocationWrapper>;
        private _map: Commerce.Controls.Bing.MapController;
        private _selectedLocation: Observable<Model.Entities.StoreLocationWrapper>;
        private _searchText: Observable<string>;
        private _showAllResults: Observable<string>;
        private _currentStoreLocation: Model.Entities.StoreLocationWrapper;
        private _cartLines: Model.Entities.CartLine[];
        private _isMapLoaded: boolean = false;
        private _requestedPickupDate: Observable<Date>;
        private _currentStore: Model.Entities.OrgUnit;
        private _eventListenerRegistered: boolean = false;

        private _isShippingDateInvalid: Observable<boolean>;
        private _minYear: number;
        private _messageEventListener: (event: any) => any = null;

        private _deliveryViewModel: ViewModels.ShippingViewModel;
        private _cartViewModel: ViewModels.CartViewModel;
        private _storeViewModel: ViewModels.StoreViewModel;

        private _options: IPickUpInStoreViewControllerOptions;

        public errorMessageSearchResultVisible: Observable<boolean>;
        public errorMessageSearchResult: Observable<string>;
        public allLocations: Model.Entities.OrgUnit[];
        public commonHeaderData: Controls.CommonHeaderData;
        public _indeterminateWaitVisible: Observable<boolean>;
        public toggleShowHideMenu: Observable<any>;

        // Dialog values
        public disablePreferredShipping: Computed<boolean>;

        constructor(options: IPickUpInStoreViewControllerOptions) {
            super(false);

            if (ObjectExtensions.isNullOrUndefined(options)) {
                throw "The options parameter was not provided and is required for PickUpInStoreView to function correctly.";
            }

            this._options = options;

            // initialize view models
            this._deliveryViewModel = new ViewModels.ShippingViewModel();
            this._cartViewModel = new ViewModels.CartViewModel();
            this._storeViewModel = new ViewModels.StoreViewModel();

            // initialize members
            this._cartLines = ObjectExtensions.isNullOrUndefined(options.cartLines) ? [] : options.cartLines;
            this._selectedLocation = ko.observable(null);
            this._locations = ko.observableArray<Model.Entities.StoreLocationWrapper>([]);
            this.allLocations = [];
            this._searchText = ko.observable(StringExtensions.EMPTY);
            this._showAllResults = ko.observable(StringExtensions.EMPTY);
            this._requestedPickupDate = ko.observable(DateExtensions.getDate());
            this.toggleShowHideMenu = ko.observable(() => { });
            this.errorMessageSearchResult = ko.observable(StringExtensions.EMPTY);
            this.errorMessageSearchResultVisible = ko.observable(false);

            var cart = this._cartViewModel.cart();

            if ((DateExtensions.isValidDate(cart.RequestedDeliveryDate))
                && (CartHelper.GetNonVoidedCartLines(cart.CartLines).length == this._cartLines.length)) { // If we are doing Pick Up All
                this._requestedPickupDate(this._cartViewModel.cart().RequestedDeliveryDate);
            } else if (ArrayExtensions.hasElements(this._cartLines) && DateExtensions.isValidDate(this._cartLines[0].RequestedDeliveryDate)) {
                this._requestedPickupDate(this._cartLines[0].RequestedDeliveryDate);
            }

            //Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            if (options.isForPickUp) {
                this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_4330")); //CUSTOMER ORDER
                this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_2508")); //Pick up in store
            } else {
                this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_2541")); //STORES
                this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_2542")); //Store locations
            }

            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);

            this._indeterminateWaitVisible = ko.observable(true);

            // Dialog values
            this.disablePreferredShipping = ko.computed(this.isPreferredShippingDisabled, this);

            this._isShippingDateInvalid = ko.observable(false);
            this._minYear = DateExtensions.getDate().getFullYear();
        }

        private isPreferredShippingDisabled(): boolean {
            var isCartViewModelNull: boolean = ObjectExtensions.isNullOrUndefined(this._cartViewModel);
            var isCustomerAddressNull: boolean = ObjectExtensions.isNullOrUndefined(this._cartViewModel.customerAddress());
            return isCartViewModelNull || isCustomerAddressNull;
        }

        /**
         * Methods executed when user navigates to this page.
         * When user clicks the back button to go to this page, this method won't be executed.
         */
        public load(): void {
            var mapElementId: string = "pickupInStoreMap";
            var mapElement: HTMLElement = document.getElementById(mapElementId);

            // This is to overcome a specific issue with the BingMaps control in the iFrame
            // when coming back to the page after it is cached.
            if (!ObjectExtensions.isNullOrUndefined(mapElement)) {
                mapElement.parentElement.removeChild(mapElement);
            }

            // Initialize map element, if not yet created.
            mapElement = Controls.Bing.MapController.createBingMapsElement("pickupInStoreMapContent", mapElementId,
                "grow positionRelative", "resx: { ariaLabel: 'string_2552' }");

            // initialize map
            var mapHandlers = new Dictionary<Function>();
            mapHandlers.setItem(Controls.Bing.MapEvents.LOADED, this.onMapLoaded);
            mapHandlers.setItem(Controls.Bing.MapEvents.ERROR, this.mapError);
            mapHandlers.setItem(Controls.Bing.MapEvents.SEARCH_SUCCESS, this.mapSearchSuccess);
            mapHandlers.setItem(Controls.Bing.MapEvents.UPDATE_LOCATIONS, this.updateLocations);
            mapHandlers.setItem(Controls.Bing.MapEvents.INITIALIZATION_ERROR, this.mapInitializationError);
            mapHandlers.setItem(Controls.Bing.MapEvents.INFOBOX_HYPERLINK_CLICKED, (data: any) => {
                ViewModelAdapter.navigate("StoreDetailsView", { StoreId: data.elementId });
            });

            this._map = new Commerce.Controls.Bing.MapController(this, <HTMLIFrameElement>mapElement, mapHandlers);
            this.addMessageEventListener();
            this._eventListenerRegistered = true;

            // Show url 'View all stores' that navigates to AllStoresView
            if (!ArrayExtensions.hasElements(this.allLocations)) {
                this._storeViewModel.getAvailableStoresForPickup()
                    .done((availableStores: Model.Entities.OrgUnit[]) => {
                        this.allLocations = availableStores;
                        this._showAllResults(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_2511"), this.allLocations.length));

                        this._cartViewModel.load()
                            .fail((errors: Model.Entities.Error[]) => this.displayErrors(errors));
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        this.displayErrors(errors);
                    });
            }

            // It is possible that we were navigated back from all stores view page, so we need to react appropriatelly
            if (!ObjectExtensions.isNullOrUndefined(this._options.selectedStore) && this._options.isForPickUp) {
                this.showPickUpDialog(this._options.selectedStore);
            }
        }

        private dateChangeHandler(event: CustomEvent): void {
            var datePickerDiv: HTMLDivElement = <HTMLDivElement>event.currentTarget;
            var datePickerControl: any = datePickerDiv.winControl;
            this._requestedPickupDate(DateExtensions.getDate(datePickerControl.current));
            var currentDate: Date = DateExtensions.getDate();

            this._isShippingDateInvalid(this._requestedPickupDate() < currentDate);
        }

        private displayErrors(errors: Model.Entities.Error[]): void {
            this._indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        public showPickUpDialog(eventInfo: Model.Entities.StoreLocationWrapper): void {
            this._indeterminateWaitVisible(false);

            if (this._options.isForPickUp) {
                this._selectedLocation(eventInfo);
            } else if (ObjectExtensions.isFunction(this._options.storeSelectionCallback)){
                var asyncResult: IAsyncResult<any> = this._options.storeSelectionCallback(eventInfo.orgUnit);

                if (ObjectExtensions.isNullOrUndefined(asyncResult)) {
                    throw "storeSelectionCallback returned a null or undefined AsyncResult.";
                }

                asyncResult.done((result: any): void => {
                    if (!ObjectExtensions.isNullOrUndefined(result)) {
                        ViewModelAdapter.navigate(this._options.callerPage, result);
                    } else {
                        throw "storeSelectionCallback returned a null or undefined result.";
                    }
                })
                .fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });
            } else {
                throw "Invalid storeSelectionCallback provided to PickUpInStoreView.";
            }
        }

        private getCurrentStoreSuccess(store: Model.Entities.OrgUnit): void {
            this._currentStore = store;
            this._map.searchByAddress(this.currentStoreAddress);
        }

        private get currentStoreAddress(): string {
            var currentStoreAddress: string = StringExtensions.EMPTY;
            if (!ObjectExtensions.isNullOrUndefined(this._currentStore) && !ObjectExtensions.isNullOrUndefined(this._currentStore.OrgUnitAddress)) {
                var addressLines: string[] = AddressHelper.getFormattedAddress(this._currentStore.OrgUnitAddress);
                currentStoreAddress = addressLines.join();
            }
            return currentStoreAddress;
        }

        private get currentCustomerAddress(): string {
            var currentCustomerAddress: string = "";
            if (!ObjectExtensions.isNullOrUndefined(this._cartViewModel.customerAddress())) {
                var addressLines: string[] = AddressHelper.getFormattedAddress(this._currentStore.OrgUnitAddress);
                currentCustomerAddress = addressLines.join();
            }
            return currentCustomerAddress;
        }

        private onMapLoaded(): void {
            // get current store location
            this.getCurrentStoreSuccess(ApplicationContext.Instance.storeInformation);
        }

        private pickUpDialogButtonClick(buttonId: string): void {
            switch (buttonId) {
                case Commerce.Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    if (this._isShippingDateInvalid()) {
                        NotificationHandler.displayErrorMessage("string_2545"); // The pick up date is not valid. Enter a valid date for pick up.
                        break;
                    }

                    this.pickUpOkButtonHandler();
                    this._selectedLocation(null);
                    break;
                case Commerce.Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this._selectedLocation(null);
                    break;
                default:
                    this.displayErrors(<Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                    this._selectedLocation(null);
                    break;
            }
        }

        private pickUpOkButtonHandler(): void {
            var location: Model.Entities.StoreLocationWrapper = this._selectedLocation();
            if (ObjectExtensions.isNullOrUndefined(location)) {
                return;
            }

            this._indeterminateWaitVisible(true);

            var selectedStore: Model.Entities.OrgUnit = ApplicationContext.Instance.availableStores.getItem(location.store.OrgUnitNumber);
            var shippingAddress: Model.Entities.Address = ObjectExtensions.isNullOrUndefined(selectedStore)
                ? new Model.Entities.AddressClass()
                : selectedStore.OrgUnitAddress;

            this._deliveryViewModel.setPickupInStoreDeliveryAsync(
                this._cartViewModel.cart(),
                this._cartLines,
                location.store.OrgUnitNumber,
                this._requestedPickupDate(),
                shippingAddress)
                .done(() => {
                    this._indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CartView", <ICartViewControllerOptions>{ transactionDetailViewMode: Commerce.ViewControllers.CartViewTransactionDetailViewMode.Delivery });
                })
                .fail((error: Model.Entities.Error[]) => {
                    this._selectedLocation(null);
                    this.displayErrors(error);
                });
        }

        private mapError(msg: ErrorEvent): void {
            this._indeterminateWaitVisible(false);
            RetailLogger.viewsCustomerPickUpInStoreViewBingMapsFaild(msg.message);
            this.errorMessageSearchResult(ViewModelAdapter.getResourceString("string_29015"));
            this.errorMessageSearchResultVisible(true);
        }

        private mapInitializationError(msg: ErrorEvent) {
            RetailLogger.viewsCustomerPickUpInStoreViewBingMapsFailedToInitialize(msg.message);
            // When map failed to initialize we unload all locations
            this._storeViewModel.getStoreLocationByArea(PickUpInStoreViewController.EARTH_AREA)
                .done((storeLocations: Model.Entities.OrgUnitLocation[]) => {
                    this._locations([]);
                    this._indeterminateWaitVisible(false);
                }).done(() => {
                    var errors: Model.Entities.Error[] = [new Model.Entities.Error("string_29015")]; //Sorry, the Bing Maps server has encountered an error. Please try again or contact your system administrator.
                    this.displayErrors(errors);
                });
        }

        private initMap(): void {
            this._isMapLoaded = true;
            this._locations.subscribe(this.onLocationsChanged, this);

            // default address is the current store’s address
            this._map.setMapView(ApplicationContext.Instance.storeInformation, Commerce.Controls.Bing.MapMethods.DEFAULT_ZOOM_LEVEL);
        }

        private searchClick(): void {
            this.showIndeterminateWait();
            this._map.removePushpins();
            this._map.removeInfobox();
            this._locations([]);
            this.errorMessageSearchResultVisible(false);
            this._map.searchByAddress(this._searchText());
        }

        private mapSearchSuccess(data: any): void {
            this._indeterminateWaitVisible(false);
            if (ObjectExtensions.isNullOrUndefined(data) || ObjectExtensions.isNullOrUndefined(data.searchResult)) {

                //search was success, but no addresses were found.
                this.errorMessageSearchResult(StringExtensions.format(ViewModelAdapter.getResourceString("string_29014"), this._searchText()));
                this.errorMessageSearchResultVisible(true);
                return;
            }

            this.errorMessageSearchResultVisible(false);
            var searchArea: Model.Entities.SearchArea = new Model.Entities.SearchAreaClass({
                Longitude: data.searchResult.location.longitude,
                Latitude: data.searchResult.location.latitude,
                Radius: data.radius,
                DistanceUnitValue: PickUpInStoreViewController.DEFAULT_DISTANCE_UNIT
            });

            if (this._isMapLoaded && !StringExtensions.isEmptyOrWhitespace(this._searchText())) {
                this.searchStoresByArea(searchArea);
            } else {
                this._storeViewModel.getStoreLocationByArea(searchArea)
                    .done((storeLocations: Model.Entities.OrgUnitLocation[]) => {

                        var currentStoreLocation: Model.Entities.OrgUnitLocation = ArrayExtensions.firstOrUndefined(
                            storeLocations,
                            (storeLocation: Model.Entities.OrgUnitLocation) => { return storeLocation.OrgUnitNumber == ApplicationContext.Instance.storeNumber; });
                        this._currentStoreLocation = !ObjectExtensions.isNullOrUndefined(currentStoreLocation) ? new Model.Entities.StoreLocationWrapper(currentStoreLocation) : null;
                        this.initMap();
                    }).fail((errors: Model.Entities.Error[]) => { this.displayErrors(errors); });
            }
        }

        private onLocationsChanged(newLocations: Model.Entities.StoreLocationWrapper[]): void {

            newLocations.forEach((location: Model.Entities.StoreLocationWrapper, index: number) => {
                var itemNumber: string = (index + 1).toString();
                this._map.addMapPin(location.store.Latitude, location.store.Longitude, itemNumber);

                location.Number = itemNumber;
            });
        }

        private searchStoresByArea(searchArea: Model.Entities.SearchArea): void {
            this._storeViewModel.getStoreLocationByArea(searchArea).done((stores: Array<Model.Entities.OrgUnitLocation>) => {

                var storesWrapper: Model.Entities.StoreLocationWrapper[] = [];
                var self = this;
                stores.forEach((storeLocation: Model.Entities.OrgUnitLocation) => {
                    var wrapper: Model.Entities.StoreLocationWrapper = new Model.Entities.StoreLocationWrapper(storeLocation);

                    wrapper.storeUrlSelected = () => {
                        this._map.addStoreInfobox(wrapper, true);
                    };

                    wrapper.storeSelected = () => {
                        self.showPickUpDialog(wrapper);
                    };

                    WinJS.Utilities.markSupportedForProcessing(wrapper.storeUrlSelected);
                    WinJS.Utilities.markSupportedForProcessing(wrapper.storeSelected);

                    storesWrapper.push(wrapper);
                });

                this._locations(storesWrapper);
                this._indeterminateWaitVisible(false);
            }).fail(this.displayErrors.bind(this));
        }

        private showAllClick(): void {

            var options: IAllStoresViewControllerOptions = null;
            var self: PickUpInStoreViewController = this;
            if (this._options.isForPickUp) {
                options = {
                    callerPage: "PickUpInStoreView",
                    locations: this.allLocations,
                    storeSelectionCallback: (store: Model.Entities.OrgUnit): IAsyncResult<IPickUpInStoreViewControllerOptions> => {
                        var storeDetailsViewModel: ViewModels.StoreDetailsViewModel = new ViewModels.StoreDetailsViewModel({ StoreId: store.OrgUnitNumber });
                        var orgUnitLocation: Model.Entities.OrgUnitLocation = Model.Entities.StoreLocationWrapper.convertToOrgUnitLocation(store);

                        return storeDetailsViewModel.getStoreDistance()
                            .map((): IPickUpInStoreViewControllerOptions => {
                                orgUnitLocation.Distance = ApplicationContext.Instance.storeDistancesMap.getItem(store.OrgUnitNumber);
                                var storeWrapper: Model.Entities.StoreLocationWrapper = new Model.Entities.StoreLocationWrapper(orgUnitLocation);
                                var pickupParameters: ViewControllers.IPickUpInStoreViewControllerOptions = self._options;
                                pickupParameters.selectedStore = storeWrapper;
                                return pickupParameters;
                            });
                    }
                };

            } else {
                options = {
                    callerPage: this._options.callerPage,
                    locations: this.allLocations,
                    storeSelectionCallback: this._options.storeSelectionCallback
                };
            }

            ViewModelAdapter.navigate("AllStoresView", options);
        }

        private updateLocations(data: any) {
            this.showIndeterminateWait();
            var searchArea: Model.Entities.SearchArea = {
                Longitude: data.longitude,
                Latitude: data.latitude,
                Radius: data.radius,
                DistanceUnitValue: PickUpInStoreViewController.DEFAULT_DISTANCE_UNIT,
            };

            this.searchStoresByArea(searchArea);
        }

        // #region Bottom app bar

        private searchCurrentStore(): void {
            this.searchStoresByAddress(this.currentStoreAddress);
        }

        private searchPreferredShipping(): void {
            this.searchStoresByAddress(this.currentCustomerAddress);
        }

        private searchStoresByAddress(address: string): void {
            this.showIndeterminateWait();
            this._searchText(address);
            this._map.searchByAddress(address);
        }

        private useCurrentStore(): void {
            this.showPickUpDialog(this._currentStoreLocation);
        }

        /**
         * Show indeterminate wait.
         */
        private showIndeterminateWait() {
            //show indeterminate wait only if dialog is not opened / not visible.
            if (ObjectExtensions.isNullOrUndefined(this._selectedLocation)) {
                this._indeterminateWaitVisible(true);
            }
        }

        private addMessageEventListener(): void {
            // add handler for communication with map iframe
            if (ObjectExtensions.isNullOrUndefined(this._messageEventListener)) {
                this._messageEventListener = this._map.processMessage.bind(this._map);
            }

            this._map.addMessageEventListener(this._messageEventListener);
        }

        /**
         * Call executed when user navigates back or navigates to this page.
         */
        public onShown(): void {
            if (this._eventListenerRegistered) {
                // event listener already registered at load() method, no need to add listener.
                this._eventListenerRegistered = false;
                return;
            }

            // We need to register event listener every time onShown is executed,
            // to ensure any map interaction will give callbacks only to this page controller.
            this.addMessageEventListener();
        }

        /*
        *   Shows center map menu
        */
        public showCenterMapMenu(): void {
            this.toggleShowHideMenu()();
        }

        // endregion

        public unload(): void {
            this._map.dispose();
            super.unload();
        }
    }
}