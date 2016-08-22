/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IChannelManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class ChannelManager implements Commerce.Model.Managers.IChannelManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get the channel configuration of a particular channel.
         * @return {IAsyncResult<Entities.ChannelConfiguration>} The async result.
         */
        public getChannelConfigurationAsync(): IAsyncResult<Entities.ChannelConfiguration> {
            var request: Common.IDataServiceRequest = this._commerceContext.orgUnits().getOrgUnitConfiguration();
            return request.execute<Entities.ChannelConfiguration>();
        }

        /**
         * Get the environment configuration.
         * @return {IAsyncResult<Entities.EnvironmentConfiguration>} The async result.
         */
        public getEnvironmentConfiguration(): IAsyncResult<Entities.EnvironmentConfiguration> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getEnvironmentConfiguration();
            return request.execute<Entities.EnvironmentConfiguration>();
        }

        /**
         * Get the device configuration.
         * @return {IAsyncResult<Entities.DeviceConfiguration>} The async result.
         */
        public getDeviceConfigurationAsync(): IAsyncResult<Entities.DeviceConfiguration> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getDeviceConfiguration();
            return request.execute<Entities.DeviceConfiguration>();
        }

        /**
         * Get the hardware profile.
         * @param {string} profileId The profile identifier.
         * @return {IAsyncResult<Entities.HardwareProfile>} The async result.
         */
        public getHardwareProfileAsync(profileId: string): IAsyncResult<Entities.HardwareProfile> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getHardwareProfileById(profileId);
            return request.execute<Entities.HardwareProfile>();
        }

        /**
         * Get the payment merchant information.
         * @param {string} profileId The profile identifier.
         * @return {IAsyncResult<Entities.PaymentMerchantInformation>} The async result.
         */
        public getPaymentMerchantInformationAsync(profileId: string): IAsyncResult<Entities.PaymentMerchantInformation> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getPaymentMerchantInformation(profileId);
            return request.execute<Entities.PaymentMerchantInformation>();
        }

        /**
         * Get the list of country / region based on language identifier.
         * It will return object with values according to language identifier given from parameter.
         * @param {string} languageId The language identifier.
         * @return {IAsyncResult<Entities.CountryRegionInfo[]>} The async result.
         */
        public getCountryRegionsAsync(languageId: string): IAsyncResult<Entities.CountryRegionInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCountryRegionsByLanguageId(languageId);
            return request.execute<Entities.CountryRegionInfo[]>();
        }

        /**
         * Get the list of state / provinces based on country / region Id.
         * @param {string} countryId The country / region identifier.
         * @return {IAsyncResult<Entities.StateProvinceInfo[]>} The async result.
         */
        public getStateProvincesAsync(countryId: string): IAsyncResult<Entities.StateProvinceInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getStateProvinces(countryId);
            return request.execute<Entities.StateProvinceInfo[]>();
        }

        /**
         * Get the list of currencies available.
         * @return {IAsyncResult<Entities.Currency[]>} The async result.
         */
        public getCurrenciesAsync(): IAsyncResult<Entities.Currency[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCurrencies();
            return request.execute<Entities.Currency[]>();
        }

        /**
         * Get the list of denominations available for all store currencies.
         * @return {IAsyncResult<Entities.CashDeclaration[]>} The async result.
         */
        public getCashDeclarationAsync(): IAsyncResult<Entities.CashDeclaration[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCashDeclarations();
            return request.execute<Entities.CashDeclaration[]>();
        }

        /**
         * Get the list of available delivery options.
         * @return {IAsyncResult<Entities.DeliveryOption[]>} The async result.
         */
        public getDeliveryOptionsAsync(): IAsyncResult<Entities.DeliveryOption[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getDeliveryOptions();
            return request.execute<Entities.DeliveryOption[]>();
        }

        /**
         * Get the list of languages available.
         * @return {IAsyncResult<Entities.SupportedLanguage[]>} The async result.
         */
        public getLanguagesAsync(): IAsyncResult<Entities.SupportedLanguage[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getLanguages();
            return request.execute<Entities.SupportedLanguage[]>();
        }

        /**
         * Get the list of operations available.
         * @return {IAsyncResult<Entities.OperationPermission[]>} The async result.
         */
        public getOperationsAsync(): IAsyncResult<Entities.OperationPermission[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getOperationPermissions();
            return request.execute<Entities.OperationPermission[]>();
        }

        /**
         * Get the list of tender types available.
         * @return {IAsyncResult<Entities.TenderType[]>} The async result.
         */
        public getTenderTypesAsync(): IAsyncResult<Entities.TenderType[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getTenderTypes();
            return request.execute<Entities.TenderType[]>();
        }

        /**
         * Get the list of units of measure available.
         * @return {IAsyncResult<Entities.UnitOfMeasure[]>} The async result.
         */
        public getUnitsOfMeasureAsync(): IAsyncResult<Entities.UnitOfMeasure[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getUnitsOfMeasure();
            return request.execute<Entities.UnitOfMeasure[]>();
        }

        /**
         * Gets the card types.
         * @return {IAsyncResult<Entities.CardTypeInfo[]>} The async result.
         */
        public getCardTypesAsync(): IAsyncResult<Entities.CardTypeInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCardTypes();
            return request.execute<Entities.CardTypeInfo[]>();
        }

        /**
         * Gets the hardware station profiles.
         * @return {IAsyncResult<Entities.HardwareStationProfile[]>} The async result.
         */
        public getHardwareStationProfileAsync(): IAsyncResult<Entities.HardwareStationProfile[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getHardwareStationProfiles();
            return request.execute<Entities.HardwareStationProfile[]>();
        }

        /**
         * Gets the customized UI strings for the given language identifier.
         * @param {string} the language identifier for which to get customized strings.
         * @return {IAsyncResult<Entities.LocalizedString[]>} The async result.
         */
        public getCustomUIStrings(languageId: string): IAsyncResult<Entities.LocalizedString[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getLocalizedStrings(languageId, null);
            return request.execute<Entities.LocalizedString[]>();
        }

        /**
         * Gets all available stores.
         * @return {IAsyncResult<Entities.OrgUnit[]>} The async result.
         */
        public getAvailableStoresAsync(): IAsyncResult<Entities.OrgUnit[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.orgUnits().read();
            return request.execute<Entities.OrgUnitLocation[]>();
        }

        /**
         * Gets stores within area.
         * @param {Entities.SearchArea} searchArea: Area to search within.
         * @return {IAsyncResult<Entities.OrgUnitLocation[]>} The async result.
         */
        public getStoreLocationByArea(searchArea: Entities.SearchArea): IAsyncResult<Entities.OrgUnitLocation[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.orgUnits().getOrgUnitLocationsByArea(searchArea);
            return request.execute<Entities.OrgUnitLocation[]>();
        }

        /**
         * Gets the store information from a given store number.
         * @param {string} storeId The store number.
         * @return {IAsyncResult<Entities.OrgUnit>} The async result.
         */
        public getStoreDetailsAsync(storeId: string): IAsyncResult<Entities.OrgUnit> {
            var request: Common.IDataServiceRequest = this._commerceContext.orgUnits(storeId).read();
            return request.execute<Entities.OrgUnit>();
        }

        /**
         * Get sales tax groups.
         * @return {IAsyncResult<Entities.SalesTaxGroup[]>} The async result.
         */
        public getSalesTaxGroups(): IAsyncResult<Entities.SalesTaxGroup[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getSalesTaxGroups();
            return request.execute<Entities.SalesTaxGroup[]>();
        }

        /**
         * Get reason codes for return order operation.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result.
         */
        public getReturnOrderReasonCodesAsync(): IAsyncResult<Entities.ReasonCode[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getReturnOrderReasonCodes();
            return request.execute<Entities.ReasonCode[]>();
        }

        /**
         * Get latest number sequence.
         * @return {IAsyncResult<Entities.NumberSequenceSeedData>} The async result.
         */
        public getLatestNumberSequence(): IAsyncResult<Entities.NumberSequenceSeedData[]> {
            return this._commerceContext.storeOperations().getLatestNumberSequence().execute<Entities.NumberSequenceSeedData[]>();
        }

        /**
         * validate retail plan trial plan offer.
         * @return {IAsyncResult<boolean>} The async result.
         */
        public getRetailTrialPlanOfferAsync(): IAsyncResult<boolean> {
            return this._commerceContext.storeOperations().getRetailTrialPlanOffer().execute<boolean>();
        }

        /**
         * Get the list of counties based on country / region identifier and state.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @return {IAsyncResult<Entities.CountyInfo[]>} The async result.
         */
        public getCountiesAsync(countryId: string, stateId: string): IAsyncResult<Entities.CountyInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCounties(countryId, stateId);
            return request.execute<Entities.CountyInfo[]>();
        }

        /**
         * Get the list of cities based on country / region identifier, state and county.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @param {string} countyId The county identifier.
         * @return {IAsyncResult<Entities.CityInfo[]>} The async result.
         */
        public getCitiesAsync(countryId: string, stateId: string, countyId: string): IAsyncResult<Entities.CityInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCities(countryId, stateId, countyId);
            return request.execute<Entities.CityInfo[]>();
        }

        /**
         * Get the list of districts based on country / region identifier, stateId, countyId and name of city.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @param {string} countyId The county identifier.
         * @param {string} cityName The name of city.
         * @return {IAsyncResult<Entities.DistrictInfo[]>} The async result.
         */
        public getDistrictsAsync(countryId: string, stateId: string, countyId: string, cityName: string): IAsyncResult<Entities.DistrictInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getDistricts(countryId, stateId, countyId, cityName);
            return request.execute<Entities.DistrictInfo[]>();
        }

        /**
         * Get the list of addresses associated with ZIP/Postal code.
         * @param {string} countryId The country / region identifier.
         * @param {string} zipCode ZIP/Postal code.
         * @return {IAsyncResult<Entities.ZipCodeInfo[]>} The async result.
         */
        public getAddressFromZipCodeAsync(countryId: string, zipCode: string): IAsyncResult<Entities.ZipCodeInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getAddressFromZipCode(countryId, zipCode);
            return request.execute<Entities.ZipCodeInfo[]>();
        }

        /**
         * Gets the list of terminal and device association information for a particular store.
         * @param {string} orgUnitNumber The store number.
         * @param {number} deviceType The device type value.
         * @return {IAsyncResult<Entities.TerminalInfo[]>} The async result.
         */
        public getTerminalInfoAsync(orgUnitNumber: string, deviceType: number): IAsyncResult<Entities.TerminalInfo[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.orgUnits(orgUnitNumber).getTerminalInfo(deviceType);
            return request.execute<Entities.TerminalInfo[]>();
        }
    }
}