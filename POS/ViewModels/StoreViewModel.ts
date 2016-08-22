/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    /**
     * Represents the delivery view model.
     */
    export class StoreViewModel extends ViewModelBase {

        constructor() {
            super();
        }

        /**
         * Gets stores within area.
         *
         * @param {Entities.SearchArea} searchArea: Area to search within.
         * @return {IAsyncResult<Entities.OrgUnitLocation[]>} The async result.
         */
        public getStoreLocationByArea(searchArea: Model.Entities.SearchArea): IAsyncResult<Model.Entities.OrgUnitLocation[]> {
            return this.channelManager.getStoreLocationByArea(searchArea);
        }

        /**
         * Gets the current store.
         *
         * @return {IAsyncResult<Entities.OrgUnitLocation[]>} The async result.
         */
        public getCurrentStore(): IAsyncResult<Model.Entities.OrgUnit> {
            return this.channelManager.getStoreDetailsAsync(ApplicationContext.Instance.storeNumber);
        }

        /**
         * Get the list of all available stores.
         *
         * @return {Model.Entities.OrgUnit[]} List of available stores.
         */
        public getAvailableStores(): IAsyncResult<Model.Entities.OrgUnit[]> {
            return this.channelManager.getAvailableStoresAsync();
        }

        /**
         * Get the list of available stores within current store locator group.
         *
         * @return {Model.Entities.OrgUnit[]} List of available stores.
         */
        public getAvailableStoresForPickup(): IAsyncResult<Model.Entities.OrgUnit[]> {

            var asyncResult: AsyncResult<Model.Entities.OrgUnit[]> = new AsyncResult<Model.Entities.OrgUnit[]>();

            // Find all stores available within store locator group.
            var searchArea: Model.Entities.SearchArea = {
                Radius: 0, // unlimited radius
                DistanceUnitValue: Model.Entities.DistanceUnit.Miles
            };

            var pickupStores: Model.Entities.OrgUnit[] = [];

            this.channelManager.getStoreLocationByArea(searchArea)
                .done((storeLocations: Model.Entities.OrgUnitLocation[]) => {
                    pickupStores = storeLocations.map((storeLocation: Model.Entities.OrgUnitLocation) => {
                        return ApplicationContext.Instance.availableStores.getItem(storeLocation.OrgUnitNumber);
                    });

                    asyncResult.resolve(pickupStores);
                }).fail((errors: Model.Entities.Error[]) => { asyncResult.reject(errors); });

            return asyncResult;
        }
    }
}