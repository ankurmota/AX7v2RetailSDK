/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IChannelManagerName: string = "IChannelManager";
    

    export interface IChannelManager {
        /**
         * Get the channel configuration of a particular channel.
         * @return {IAsyncResult<Entities.ChannelConfiguration>} The async result.
         */
        getChannelConfigurationAsync(): IAsyncResult<Entities.ChannelConfiguration>;

        /**
         * Get the environment configuration.
         * @return {IAsyncResult<Entities.EnvironmentConfiguration>} The async result.
         */
        getEnvironmentConfiguration(): IAsyncResult<Entities.EnvironmentConfiguration>;

        /**
         * Get the device configuration.
         * @return {IAsyncResult<Entities.DeviceConfiguration>} The async result.
         */
        getDeviceConfigurationAsync(): IAsyncResult<Entities.DeviceConfiguration>;

        /**
         * Get the hardware profile.
         * @param {string} profileId The profile identifier.
         * @return {IAsyncResult<Entities.HardwareProfile>} The async result.
         */
        getHardwareProfileAsync(profileId: string): IAsyncResult<Entities.HardwareProfile>;

        /**
         * Get the payment merchant information.
         * @param {string} profileId The profile identifier.
         * @return {IAsyncResult<Entities.PaymentMerchantInformation>} The async result.
         */
        getPaymentMerchantInformationAsync(profileId: string): IAsyncResult<Entities.PaymentMerchantInformation>;

        /**
         * Gets the card types.
         * @return {IAsyncResult<Entities.CardTypeInfo[]>} The async result.
         */
        getCardTypesAsync(): IAsyncResult<Entities.CardTypeInfo[]>;

        /**
         * Get the list of country / region based on language identifier.
         * It will return object with values according to language identifier given from parameter.
         * @param {string} languageId The language identifier.
         * @return {IAsyncResult<Entities.CountryRegionInfo[]>} The async result.
         */
        getCountryRegionsAsync(languageId: string): IAsyncResult<Entities.CountryRegionInfo[]>;

        /**
         * Get the list of hardware station profile.
         * @return {IAsyncResult<Entities.HardwareStationProfile[]>} The async result.
         */
        getHardwareStationProfileAsync(): IAsyncResult<Entities.HardwareStationProfile[]>;

        /**
         * Get the list of state / provinces based on country / region Id.
         * @param {string} countryId The country / region identifier.
         * @return {IAsyncResult<Entities.StateProvinceInfo[]>} The async result.
         */
        getStateProvincesAsync(countryId: string): IAsyncResult<Entities.StateProvinceInfo[]>;

        /**
         * Get the list of languages available.
         * @return {IAsyncResult<Entities.SupportedLanguage[]>} The async result.
         */
        getLanguagesAsync(): IAsyncResult<Entities.SupportedLanguage[]>;

        /**
         * Get the list of currencies available.
         * @return {IAsyncResult<Entities.Currency[]>} The async result.
         */
        getCurrenciesAsync(): IAsyncResult<Entities.Currency[]>;

        /**
         * Get the list of denominations available for all store currencies.
         * @return {IAsyncResult<Entities.CashDeclaration[]>} The async result.
         */
        getCashDeclarationAsync(): IAsyncResult<Entities.CashDeclaration[]>;

        /**
         * Get the list of available delivery options.
         * @return {IAsyncResult<Entities.DeliveryOption[]>} The async result.
         */
        getDeliveryOptionsAsync(): IAsyncResult<Entities.DeliveryOption[]>;

        /**
         * Get the list of operations available.
         * @return {IAsyncResult<Entities.OperationPermission[]>} The async result.
         */
        getOperationsAsync(): IAsyncResult<Entities.OperationPermission[]>;

        /**
         * Gets the customized UI strings for the given language identifier.
         * @param {string} the language identifier for which to get customized strings.
         * @return {IAsyncResult<Entities.LocalizedString[]>} The async result.
         */
        getCustomUIStrings(languageId: string): IAsyncResult<Entities.LocalizedString[]>;

        /**
         * Get the list of tender types available.
         * @return {IAsyncResult<Entities.TenderType[]>} The async result.
         */
        getTenderTypesAsync(): IAsyncResult<Entities.TenderType[]>;

        /**
         * Get the list of units of measure available.
         * @return {IAsyncResult<Entities.UnitOfMeasure[]>} The async result.
         */
        getUnitsOfMeasureAsync(): IAsyncResult<Entities.UnitOfMeasure[]>;

        /**
         * Gets all available stores.
         * @return {IAsyncResult<Entities.OrgUnit[]>} The async result.
         */
        getAvailableStoresAsync(): IAsyncResult<Entities.OrgUnit[]>;

        /**
         * Gets stores within area.
         * @param {Entities.SearchArea} searchArea: Area to search within.
         * @return {IAsyncResult<Entities.OrgUnitLocation[]>} The async result.
         */
        getStoreLocationByArea(searchArea: Entities.SearchArea): IAsyncResult<Entities.OrgUnitLocation[]>;

        /**
         * Gets the store information from a given store number.
         * @param {string} storeId The store number.
         * @return {IAsyncResult<Entities.OrgUnit>} The async result.
         */
        getStoreDetailsAsync(storeId: string): IAsyncResult<Entities.OrgUnit>;

        /**
         * Get sales tax groups.
         * @return {IAsyncResult<Entities.SalesTaxGroup[]>} The async result.
         */
        getSalesTaxGroups(): IAsyncResult<Entities.SalesTaxGroup[]>;

        /**
         * Get reason codes for return order operation.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result.
         */
        getReturnOrderReasonCodesAsync(): IAsyncResult<Entities.ReasonCode[]>;

        /**
         * Get latest number sequence.
         * @return {IAsyncResult<Entities.NumberSequenceSeedData>} The async result.
         */
        getLatestNumberSequence(): IAsyncResult<Entities.NumberSequenceSeedData[]>;

        /**
         * Get the list of counties based on country / region identifier and state.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @return {IAsyncResult<Entities.CountyInfo[]>} The async result.
         */
        getCountiesAsync(countryId: string, stateId: string): IAsyncResult<Entities.CountyInfo[]>;

        /**
         * Get the list of cities based on country / region identifier, state and county.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @param {string} countyId The county identifier.
         * @return {IAsyncResult<Entities.CityInfo[]>} The async result.
         */
        getCitiesAsync(countryId: string, stateId: string, countyId: string): IAsyncResult<Entities.CityInfo[]>;

        /**
         * Get the list of districts based on country / region identifier, stateId, countyId and name of city.
         * @param {string} countryId The country / region identifier.
         * @param {string} stateId The state identifier.
         * @param {string} countyId The county identifier.
         * @param {string} cityName The name of city.
         * @return {IAsyncResult<Entities.DistrictInfo[]>} The async result.
         */
        getDistrictsAsync(countryId: string, stateId: string, countyId: string, cityName: string): IAsyncResult<Entities.DistrictInfo[]>;

        /**
         * Get the list of addresses associated with ZIP/Postal code.
         * @param {string} countryId The country / region identifier.
         * @param {string} zipCode ZIP/Postal code.
         * @return {IAsyncResult<Entities.ZipCodeInfo[]>} The async result.
         */
        getAddressFromZipCodeAsync(countryId: string, zipCode: string): IAsyncResult<Entities.ZipCodeInfo[]>;

        /**
         * Gets the retail plan trial offer flag.
         * @return {IAsyncResult<boolean>} The async result.
         */
        getRetailTrialPlanOfferAsync(): IAsyncResult<boolean>;

        /**
         * Gets the list of terminal and device association information for a particular store.
         * @param {string} orgUnitNumber The store number.
         * @param {number} deviceType The device type value.
         * @return {IAsyncResult<Entities.TerminalInfo[]>} The async result.
         */
        getTerminalInfoAsync(orgUnitNumber: string, deviceType: number): IAsyncResult<Entities.TerminalInfo[]>;
    }
}