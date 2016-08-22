/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
///<reference path='../Controls/CommonHeader.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class StoreDetailsViewController extends ViewControllerBase {

        private _map: Commerce.Controls.Bing.MapController;
        private _storeLocation: Model.Entities.OrgUnitLocation;
        private _storeNumber: number = 1; //Since this is store details views, number is always 1.
        private _isMapLoaded: boolean;
        private _storeDetailsVisible: Observable<boolean>;
        private _messageEventListener: (event: any) => any = null;
        private _bingMapsElementId: string = "storeDetailsMap";
        private _mapHandlers: Dictionary<Function>;
        private _eventListenerRegistered: boolean = false;

        public storeDetailsViewModel: ViewModels.StoreDetailsViewModel;
        public commonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;

        constructor(options: any) {
            super(true);

            // initialize view models
            this.storeDetailsViewModel = new ViewModels.StoreDetailsViewModel(options);
            this._isMapLoaded = false;

            //Load Common Header 
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_5400"));
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);

            this.indeterminateWaitVisible = ko.observable(true);
            this._storeDetailsVisible = ko.observable(false);
        }

        /**
         * Called when the page is loaded on the DOM.
         */
        public load() {

            var mapElement: HTMLElement = document.getElementById(this._bingMapsElementId);
            if (ObjectExtensions.isNullOrUndefined(mapElement)) {

                mapElement = Controls.Bing.MapController.createBingMapsElement("storeDetailsMapContent", this._bingMapsElementId,
                    "height40 grow positionRelative", "resx: { ariaLabel: 'string_2552' }");

                // initialize map
                this._mapHandlers = new Dictionary<Function>();
                this._mapHandlers.setItem(Controls.Bing.MapEvents.LOADED, this.onMapLoaded);
                this._mapHandlers.setItem(Controls.Bing.MapEvents.ERROR, this.mapError);
                this._mapHandlers.setItem(Controls.Bing.MapEvents.INITIALIZATION_ERROR, this.mapError);
                this._mapHandlers.setItem(Controls.Bing.MapEvents.SEARCH_SUCCESS, this.mapSearchSuccess);

                this._map = new Commerce.Controls.Bing.MapController(this, <HTMLIFrameElement>mapElement, this._mapHandlers);

                this.addBingMapsListener();
                this._eventListenerRegistered = true;
            }
        }

        public unload(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._map)) {
                this._map.dispose();
            }
            super.unload();
        }

        private mapSearchSuccess(data: any): void {
            var storeAddress: Model.Entities.Address = this.storeDetailsViewModel.storeDetails().OrgUnitAddress;
            this._storeLocation = {
                Latitude: data.searchResult.location.latitude,
                Longitude: data.searchResult.location.longitude,
            };

            this._map.setMapView(this._storeLocation, Commerce.Controls.Bing.MapMethods.DEFAULT_ZOOM_LEVEL);
            this._map.addMapPin(this._storeLocation.Latitude, this._storeLocation.Longitude, this._storeNumber.toString());
        }

        private mapError(msg: ErrorEvent): void {
            this.indeterminateWaitVisible(false);
            console.log(msg.message);
            var errors: Model.Entities.Error[] = [new Model.Entities.Error("string_29015")]; //Sorry, the Bing Maps server has encountered an error. Please try again or contact your system administrator.
            NotificationHandler.displayClientErrors(errors);
        }

        private get currentStoreAddress(): string {
            var currentStoreAddress: string = StringExtensions.EMPTY;
            var orgUnit: Model.Entities.OrgUnit = this.storeDetailsViewModel.storeDetails();

            if (!ObjectExtensions.isNullOrUndefined(orgUnit) && !ObjectExtensions.isNullOrUndefined(orgUnit.OrgUnitAddress)) {
                var addressLines: string[] = AddressHelper.getFormattedAddress(orgUnit.OrgUnitAddress);
                currentStoreAddress = addressLines.join();
            }

            return currentStoreAddress;
        }

        private onMapLoaded(): void {
            if (this._isMapLoaded) {
                return;
            }

            this.storeDetailsViewModel.getStoreDetails()
                .done(() => {

                    this._isMapLoaded = true;
                    this._map.searchByAddress(this.currentStoreAddress);
                    var storeName: string = this.storeDetailsViewModel.storeDetails().OrgUnitName;
                    var storeNumber: string = this.storeDetailsViewModel.storeDetails().OrgUnitNumber;
                    this.commonHeaderData.categoryName(
                        StringExtensions.format(
                            Commerce.ViewModelAdapter.getResourceString("string_610"),
                            storeNumber,
                            storeName
                            ));

                    this.storeDetailsViewModel.getStoreDistance()
                        .done(() => {
                            this._storeDetailsVisible(true);
                            this.indeterminateWaitVisible(false);
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            this.indeterminateWaitVisible(false);
                            NotificationHandler.displayClientErrors(errors);
                        });

                    /* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
                    // Note: The getStoreDistance() and getStoreDayHours() calls could also be run with help of an AsyncQueue.
                    this.storeDetailsViewModel.getStoreDayHours()
                        .done(() => {
                            this._storeDetailsVisible(true);
                            this.indeterminateWaitVisible(false);
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            this.indeterminateWaitVisible(false);
                            NotificationHandler.displayClientErrors(errors);
                        });
                    // END SDKSAMPLE_STOREHOURS (do not remove this) */

                })
                .fail((errors: Model.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                });
        }

        private addBingMapsListener(): void {
            // add handler for communication with map iframe
            if (ObjectExtensions.isNullOrUndefined(this._messageEventListener)) {
                this._messageEventListener = this._map.processMessage.bind(this._map);
            }

            this._map.addMessageEventListener(this._messageEventListener);
        }

        public popupMapInfobox(): void {
            var orgUnitLocation: Model.Entities.OrgUnitLocation = Model.Entities.StoreLocationWrapper.convertToOrgUnitLocation(this.storeDetailsViewModel.storeDetails());
            orgUnitLocation.Longitude = this._storeLocation.Longitude;
            orgUnitLocation.Latitude = this._storeLocation.Latitude;
            var storeWrapper: Model.Entities.StoreLocationWrapper = new Model.Entities.StoreLocationWrapper(orgUnitLocation);
            this._map.addStoreInfobox(storeWrapper, false);
        }

        /**
         * Call executed when user navigates back or navigates to this page.
         */
        public onShown(): void {
            if (this._eventListenerRegistered) {
                // event listener already registered at load() method, no need to add listener again.
                this._eventListenerRegistered = false;
                return;
            }

            // We need to register event listener every time onShown is executed,
            // to ensure any map interaction will give callbacks only to this page controller.
            this.addBingMapsListener();
        }
    }
}