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

///<reference path='ViewModelBase.ts'/>
/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
///<reference path='Custom.Extension.d.ts'/>
   END SDKSAMPLE_STOREHOURS (do not remove this) */

module Commerce.ViewModels {

    export class StoreDetailsViewModel extends ViewModelBase {

        private _storeId: string;
        public storeDetails: Observable<Model.Entities.OrgUnit>;
        public distance: Observable<string>;
        public distanceUnit: Observable<string>;
/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
        public storeDayHours: ObservableArray<Model.Entities.StoreDayHours>;
        public isStoreDayHoursVisible: Computed<boolean>;
   END SDKSAMPLE_STOREHOURS (do not remove this) */

        /**
         * Initialize new instance of StoreDetailsViewModel.
         * 
         * @param {any} callerContext The caller context.
         * @param {any} options 
         */
        constructor(options: any) {
            super();
            this._storeId = options.StoreId;
            this.distance = ko.observable(StringExtensions.EMPTY);
            this.distanceUnit = ko.observable(StringExtensions.EMPTY);
            this.storeDetails = ko.observable(new Model.Entities.OrgUnitClass({
                RecordId: 0,
                OrgUnitName: StringExtensions.EMPTY,
                OrgUnitNumber: StringExtensions.EMPTY,
                OrgUnitAddress: {
                    Street: StringExtensions.EMPTY,
                    City: StringExtensions.EMPTY,
                    State: StringExtensions.EMPTY,
                    County: StringExtensions.EMPTY,
                    ZipCode: StringExtensions.EMPTY,
                    Phone: StringExtensions.EMPTY,
                    ThreeLetterISORegionName: StringExtensions.EMPTY,
                    FullAddress: StringExtensions.EMPTY
                }
            }));
/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
            this.storeDayHours = ko.observableArray([]);
            this.isStoreDayHoursVisible = ko.computed(() => {
                return ArrayExtensions.hasElements(this.storeDayHours());
            });
   END SDKSAMPLE_STOREHOURS (do not remove this) */

        }

        /**
         * Reject the asynchronous call with error couldn't find store number.
         *
         * @param {VoidAsyncResult} asyncResult The asynchronous result instance to reject the call.
         */
        private rejectStoreIdNotFound(asyncResult: VoidAsyncResult): void {
            asyncResult.reject([new Model.Entities.Error(ErrorTypeEnum.STORE_NOT_FOUND, false, null, null, [this._storeId])]);
        }

        /**
         * Get the store distance between current store and this store to be viewed.
         *
         * @return {IVoidAsyncResult} Asynchronous result instance.
         */
        public getStoreDistance(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            var storeDistances: Dictionary<number> = ApplicationContext.Instance.storeDistancesMap;

            if (storeDistances.length() === 0) {
                //Find all stores available within store locator group.
                var searchArea: Model.Entities.SearchArea = {
                    Radius: 0, //no radius or distance constraint
                    DistanceUnitValue: Model.Entities.DistanceUnit.Miles
                };
                this.channelManager.getStoreLocationByArea(searchArea)
                    .done((stores: Model.Entities.OrgUnitLocation[]) => {

                        var storeFound: boolean = false;
                        for (var i: number = 0; i < stores.length; i++) {
                            storeDistances.setItem(stores[i].OrgUnitNumber, stores[i].Distance);

                            if (stores[i].OrgUnitNumber === this._storeId) {
                                this.distance(Model.Entities.StoreLocationWrapper.constructDistanceString(stores[i]));
                                this.distanceUnit(Model.Entities.StoreLocationWrapper.constructDistanceUnit(stores[i]));
                                storeFound = true;
                            }

                            ApplicationContext.Instance.storeDistancesMap = storeDistances;
                        }

                        if (storeFound) {
                            asyncResult.resolve();
                        } else {
                            //No stores found for this store Id
                            this.rejectStoreIdNotFound(asyncResult);
                        }

                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        asyncResult.reject(errors);
                    });
            }
            else if (storeDistances.hasItem(this._storeId)) {
                var storeLocation: Model.Entities.OrgUnitLocation = new Model.Entities.OrgUnitLocationClass({
                    Distance: storeDistances.getItem(this._storeId)
                });
                this.distance(Model.Entities.StoreLocationWrapper.constructDistanceString(storeLocation));
                this.distanceUnit(Model.Entities.StoreLocationWrapper.constructDistanceUnit(storeLocation));

                asyncResult.resolve();
            }
            else {
                //No stores found from this store Id
                this.rejectStoreIdNotFound(asyncResult);
            }

            return asyncResult;
        }

/* BEGIN SDKSAMPLE_STOREHOURS (do not remove this)
        public getStoreDayHours(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();
            Commerce.RetailLogger.extendedInformational("StoreDetailsViewModel.getStoreDayHours()");

            var extendedChannelManager = <Custom.Managers.IExtendedChannelManager>this.channelManager;

            extendedChannelManager.getStoreDayHoursAsync(this._storeId)
                .done((foundStoreDayHours: Model.Entities.StoreDayHours[]) => {
                    this.storeDayHours(foundStoreDayHours);
                    Commerce.RetailLogger.extendedInformational("StoreDetailsViewModel.getStoreDayHours() Success");
                    asyncResult.resolve();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }
   END SDKSAMPLE_STOREHOURS (do not remove this) */

        /**
         * Gets the store information.
         *
         * @return {IVoidAsyncResult} Asynchronous result instance.
         */
        public getStoreDetails(): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            this.channelManager.getStoreDetailsAsync(this._storeId)
                .done((storeFound: Model.Entities.OrgUnit) => {
                    this.storeDetails(storeFound);
                    asyncResult.resolve();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    asyncResult.reject(errors);
                });

            return asyncResult;
        }
    }
}