/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {
    "use strict";

    export interface ProductCatalogStore {
        Store?: Model.Entities.OrgUnit;
        Context?: Model.Entities.ProjectionDomain
        StoreType?: StoreButtonControlType;
    }

    export enum StoreButtonControlType {
        Unknown = 0,
        CurrentStore = 1,
        FindStore = 2,
        AllStores = 3,
        Warehouse = 4
    }

    export interface StoreButtonControl {
        ControlType: StoreButtonControlType;
        StoreNumber: string;
        Description: string;
    }

    export class StoreLocationWrapper {
        public Number: string;
        public distanceString: string;
        public distanceUnit: string;

        public store: Model.Entities.OrgUnitLocation;
        public orgUnit: Model.Entities.OrgUnit;
        public storeUrlSelected: () => void; // Click handlers when store url is selected.
        public storeSelected: () => void; // Click handlers when the store is selected.

        constructor(storeLocation: Model.Entities.OrgUnitLocation) {
            this.store = storeLocation;

            this.Number = '1';
            this.distanceUnit = StoreLocationWrapper.constructDistanceUnit(this.store);
            this.distanceString = StoreLocationWrapper.constructDistanceString(this.store);
            this.orgUnit = StoreLocationWrapper.convertToOrgUnit(this.store);
        }

        /**
         * Construct a string contains distance value.
         *
         * @param {Model.Entities.OrgUnitLocation} storeLocation the store entity contains distance value.
         * @returns {string} Formatted string for distance value.
         */
        public static constructDistanceString(storeLocation: Model.Entities.OrgUnitLocation): string {
            if (ObjectExtensions.isNullOrUndefined(storeLocation.Distance)) {
                return StringExtensions.EMPTY;
            }

            return NumberExtensions.roundToNDigits(storeLocation.Distance, 1) + StringExtensions.EMPTY;
        }

        /**
         * Construct a string contains distance unit.
         *
         * @param {Model.Entities.OrgUnitLocation} storeLocation the store entity contains distance value.
         * @returns {string} Formatted string for distance unit.
         */
        public static constructDistanceUnit(storeLocation: Model.Entities.OrgUnitLocation): string {
            return ViewModelAdapter.getResourceString("string_2527"); // miles
        }

        /**
         * Convert OrgUnit entity type to become OrgUnitLocation entity type.
         *
         * @param {Model.Entities.OrgUnit} orgUnit The OrgUnit entity type.
         * @return {Model.Entities.OrgUnitLocation} the entity type of OrgUnitLocation converted from OrgUnit entity type.
         */
        public static convertToOrgUnitLocation(orgUnit: Model.Entities.OrgUnit): Model.Entities.OrgUnitLocation {
            var storeUnitLocation: Model.Entities.OrgUnitLocation = new Model.Entities.OrgUnitLocationClass();
            storeUnitLocation.OrgUnitName = orgUnit.OrgUnitName;
            storeUnitLocation.OrgUnitNumber = orgUnit.OrgUnitNumber;
            storeUnitLocation.ChannelId = orgUnit.RecordId;

            storeUnitLocation.Address = orgUnit.OrgUnitFullAddress;
            storeUnitLocation.BuildingCompliment = orgUnit.OrgUnitAddress.BuildingCompliment;
            storeUnitLocation.City = orgUnit.OrgUnitAddress.City;
            storeUnitLocation.Country = orgUnit.OrgUnitAddress.ThreeLetterISORegionName;
            storeUnitLocation.County = orgUnit.OrgUnitAddress.County;
            storeUnitLocation.CountyName = orgUnit.OrgUnitAddress.CountyName;
            storeUnitLocation.DistrictName = orgUnit.OrgUnitAddress.DistrictName;
            storeUnitLocation.Postbox = orgUnit.OrgUnitAddress.Postbox;
            storeUnitLocation.State = orgUnit.OrgUnitAddress.State;
            storeUnitLocation.StateName = orgUnit.OrgUnitAddress.StateName;
            storeUnitLocation.Street = orgUnit.OrgUnitAddress.Street;
            storeUnitLocation.StreetNumber = orgUnit.OrgUnitAddress.StreetNumber;
            storeUnitLocation.Zip = orgUnit.OrgUnitAddress.ZipCode;

            return storeUnitLocation;
        }

        /**
         * Convert OrgUnitLocation entity type to become OrgUnit entity type.
         *
         * @param {Model.Entities.OrgUnitLocation} orgUnitLocation The OrgUnitLocation entity type.
         * @return {Model.Entities.OrgUnit} the entity type of OrgUnit converted from OrgUnitLocation entity type.
         */
        public static convertToOrgUnit(orgUnitLocation: Model.Entities.OrgUnitLocation): Model.Entities.OrgUnit {
            var orgUnit: Model.Entities.OrgUnit = new Model.Entities.OrgUnitClass();
            orgUnit.OrgUnitName = orgUnitLocation.OrgUnitName;
            orgUnit.OrgUnitNumber = orgUnitLocation.OrgUnitNumber;
            orgUnit.RecordId = orgUnitLocation.ChannelId;

            orgUnit.OrgUnitAddress = new Model.Entities.AddressClass();
            orgUnit.OrgUnitFullAddress = orgUnitLocation.Address;
            orgUnit.OrgUnitAddress.BuildingCompliment = orgUnitLocation.BuildingCompliment;
            orgUnit.OrgUnitAddress.City = orgUnitLocation.City;
            orgUnit.OrgUnitAddress.ThreeLetterISORegionName = orgUnitLocation.Country;
            orgUnit.OrgUnitAddress.County = orgUnitLocation.County;
            orgUnit.OrgUnitAddress.CountyName = orgUnitLocation.CountyName;
            orgUnit.OrgUnitAddress.DistrictName = orgUnitLocation.DistrictName;
            orgUnit.OrgUnitAddress.Postbox = orgUnitLocation.Postbox;
            orgUnit.OrgUnitAddress.State = orgUnitLocation.State;
            orgUnit.OrgUnitAddress.StateName = orgUnitLocation.StateName;
            orgUnit.OrgUnitAddress.Street = orgUnitLocation.Street;
            orgUnit.OrgUnitAddress.StreetNumber = orgUnitLocation.StreetNumber;
            orgUnit.OrgUnitAddress.ZipCode = orgUnitLocation.Zip;

            return orgUnit;
        }
    }
}