/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Address data service to retrieve address specific data.
        /// </summary>
        public class AddressSqlServerDataService : IRequestHandler
        {
            private const string ValidateAddressSprocName = "VALIDATEADDRESS";
            private const string GetCountryRegionInfoSprocName = "GETCOUNTRYREGIONS";
            private const string GetStateProvincesInfoSprocName = "GETSTATEPROVINCES";
            private const string GetCountiesInfoSprocName = "GETCOUNTIES";
            private const string GetCitiesInfoSprocName = "GETCITIES";
            private const string GetDistrictsInfoSprocName = "GETDISTRICTS";
            private const string GetZipCodesInfoSprocName = "GETZIPCODES";
            private const string GetFromZipCodesInfoSprocName = "GETFROMZIPCODE";
            private const string GetAddressFormattingInfoSprocName = "GETADDRESSFORMATTING";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetStateProvincesDataRequest),
                        typeof(GetCitiesDataRequest),
                        typeof(GetCountiesDataRequest),
                        typeof(GetCountryRegionDataRequest),
                        typeof(GetDistrictsDataRequest),
                        typeof(GetZipPostalCodeDataRequest),
                        typeof(GetFromZipPostalCodeDataRequest),
                        typeof(ValidateAddressDataRequest),
                        typeof(GetAddressFormattingDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetCountryRegionDataRequest))
                {
                    response = this.GetCountryRegion((GetCountryRegionDataRequest)request);
                }
                else if (requestType == typeof(GetStateProvincesDataRequest))
                {
                    response = this.GetStateProvinces((GetStateProvincesDataRequest)request);
                }
                else if (requestType == typeof(GetCountiesDataRequest))
                {
                    response = this.GetCounties((GetCountiesDataRequest)request);
                }
                else if (requestType == typeof(GetDistrictsDataRequest))
                {
                    response = this.GetDistricts((GetDistrictsDataRequest)request);
                }
                else if (requestType == typeof(GetCitiesDataRequest))
                {
                    response = this.GetCities((GetCitiesDataRequest)request);
                }
                else if (requestType == typeof(GetZipPostalCodeDataRequest))
                {
                    response = this.GetZipCodes((GetZipPostalCodeDataRequest)request);
                }
                else if (requestType == typeof(GetFromZipPostalCodeDataRequest))
                {
                    response = this.GetFromZipPostalCodes((GetFromZipPostalCodeDataRequest)request);
                }
                else if (requestType == typeof(ValidateAddressDataRequest))
                {
                    response = this.ValidateAddress((ValidateAddressDataRequest)request);
                }
                else if (requestType == typeof(GetAddressFormattingDataRequest))
                {
                    response = this.GetAddressFormatting((GetAddressFormattingDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Handles the non zero result.
            /// </summary>
            /// <param name="result">The result.</param>
            /// <returns>The fault address component.</returns>
            private static Tuple<DataValidationErrors, string> HandleNonZeroResult(int result)
            {
                Tuple<DataValidationErrors, string> faultAddressComponent;
                switch (result)
                {
                    case 1:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCountryRegion,
                    AddressServiceConstants.CountryRegionId);
                        break;
    
                    case 2:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStateProvince,
                    AddressServiceConstants.StateProvinceId);
                        break;
    
                    case 3:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCounty,
                    AddressServiceConstants.CountyId);
                        break;
    
                    case 4:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCity,
                    AddressServiceConstants.CityComponentName);
                        break;
    
                    case 5:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidDistrict,
                    AddressServiceConstants.DistrictId);
                        break;
    
                    case 6:
                        faultAddressComponent = new Tuple<DataValidationErrors, string>(
                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidZipPostalCode,
                    AddressServiceConstants.ZipPostalCodeComponentName);
                        break;
    
                    default:
                        throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, result);
                }
    
                return faultAddressComponent;
            }
    
            /// <summary>
            /// Creates failed <see cref="ValidateAddressDataResponse"/> response.
            /// </summary>
            /// <param name="errorCode">The error code associated with the failed address component.</param>
            /// <param name="faultAddressComponent">The failed address component.</param>
            /// <returns>The <see cref="ValidateAddressDataResponse"/> response.</returns>
            private static ValidateAddressDataResponse CreateFailedValidateAddressDataResponse(DataValidationErrors errorCode, string faultAddressComponent)
            {
                // If address is not valid, tell the user/client code : which component is the faulty one
                var message = string.Format(CultureInfo.InvariantCulture, @"Incorrect address provided: validate {0} property.", faultAddressComponent);
                NetTracer.Information(message);
    
                // create the response object and return
                return new ValidateAddressDataResponse(isAddressValid: false, invalidAddressComponentName: faultAddressComponent, errorCode: errorCode, errorMessage: message);
            }
    
            private GetCountryRegionDataResponse GetCountryRegion(GetCountryRegionDataRequest request)
            {
                ThrowIf.Null(request.QueryResultSettings, "settings");
    
                string languageId = request.LanguageId;
    
                if (string.IsNullOrWhiteSpace(languageId))
                {
                    var getDefaultLanguageIdDataRequest = new GetDefaultLanguageIdDataRequest();
                    languageId = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getDefaultLanguageIdDataRequest, request.RequestContext).Entity;
                }
    
                var parameterSet = new ParameterSet();
                parameterSet["nvc_LanguageId"] = languageId;
    
                Tuple<PagedResult<CountryRegionInfo>, ReadOnlyCollection<AddressFormattingInfo>> countryRegions;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    countryRegions = sqlServerDatabaseContext.ExecuteStoredProcedure<CountryRegionInfo, AddressFormattingInfo>(GetCountryRegionInfoSprocName, parameterSet);
                }
    
                return new GetCountryRegionDataResponse(countryRegions.Item1, countryRegions.Item2);
            }
    
            private EntityDataServiceResponse<StateProvinceInfo> GetStateProvinces(GetStateProvincesDataRequest request)
            {
                ThrowIf.Null(request.QueryResultSettings, "settings");
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
    
                PagedResult<StateProvinceInfo> stateProvinces;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    stateProvinces = sqlServerDatabaseContext.ExecuteStoredProcedure<StateProvinceInfo>(GetStateProvincesInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<StateProvinceInfo>(stateProvinces);
            }
    
            private EntityDataServiceResponse<CountyInfo> GetCounties(GetCountiesDataRequest request)
            {
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
                ThrowIf.Null(request.StateId, "stateId");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
                filters[AddressServiceConstants.StateProvinceId] = request.StateId;
    
                PagedResult<CountyInfo> counties;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    counties = sqlServerDatabaseContext.ExecuteStoredProcedure<CountyInfo>(GetCountiesInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<CountyInfo>(counties);
            }
    
            private EntityDataServiceResponse<CityInfo> GetCities(GetCitiesDataRequest request)
            {
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
                ThrowIf.Null(request.StateId, "stateId");
                ThrowIf.Null(request.CountyId, "countyId");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
                filters[AddressServiceConstants.StateProvinceId] = request.StateId;
                filters[AddressServiceConstants.CountyId] = request.CountyId;
    
                PagedResult<CityInfo> cities;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    cities = sqlServerDatabaseContext.ExecuteStoredProcedure<CityInfo>(GetCitiesInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<CityInfo>(cities);
            }
    
            private EntityDataServiceResponse<DistrictInfo> GetDistricts(GetDistrictsDataRequest request)
            {
                ThrowIf.Null(request.QueryResultSettings, "settings");
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
                ThrowIf.Null(request.StateId, "stateId");
                ThrowIf.Null(request.CountyId, "countyId");
                ThrowIf.Null(request.City, "city");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
                filters[AddressServiceConstants.StateProvinceId] = request.StateId;
                filters[AddressServiceConstants.CountyId] = request.CountyId;
                filters[AddressServiceConstants.CityComponentName] = request.City;
    
                PagedResult<DistrictInfo> districts;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    districts = sqlServerDatabaseContext.ExecuteStoredProcedure<DistrictInfo>(GetDistrictsInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<DistrictInfo>(districts);
            }
    
            private EntityDataServiceResponse<ZipCodeInfo> GetZipCodes(GetZipPostalCodeDataRequest request)
            {
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
                ThrowIf.Null(request.StateId, "stateId");
                ThrowIf.Null(request.CountyId, "countyId");
                ThrowIf.Null(request.City, "city");
                ThrowIf.Null(request.District, "districtId");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
                filters[AddressServiceConstants.StateProvinceId] = request.StateId;
                filters[AddressServiceConstants.CountyId] = request.CountyId;
                filters[AddressServiceConstants.CityComponentName] = request.City;
                filters[AddressServiceConstants.DistrictId] = request.District;
    
                PagedResult<ZipCodeInfo> zipCodes;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    zipCodes = sqlServerDatabaseContext.ExecuteStoredProcedure<ZipCodeInfo>(GetZipCodesInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<ZipCodeInfo>(zipCodes);
            }
    
            private EntityDataServiceResponse<ZipCodeInfo> GetFromZipPostalCodes(GetFromZipPostalCodeDataRequest request)
            {
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
                ThrowIf.Null(request.ZipPostalCode, "zipPostalCode");
    
                ParameterSet filters = new ParameterSet();
                filters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
                filters[AddressServiceConstants.ZipCode] = request.ZipPostalCode;
    
                PagedResult<ZipCodeInfo> fromZipCodes;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    fromZipCodes = sqlServerDatabaseContext.ExecuteStoredProcedure<ZipCodeInfo>(GetFromZipCodesInfoSprocName, filters);
                }
    
                return new EntityDataServiceResponse<ZipCodeInfo>(fromZipCodes);
            }
    
            private ValidateAddressDataResponse ValidateAddress(ValidateAddressDataRequest request)
            {
                ThrowIf.Null(request.Address, "address");
    
                if (string.IsNullOrWhiteSpace(request.Address.ThreeLetterISORegionName))
                {
                    return CreateFailedValidateAddressDataResponse(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCountryRegion, AddressServiceConstants.ThreeLetterISORegionName);
                }
    
                Address address = request.Address;
    
                ParameterSet parameters = new ParameterSet();
    
                parameters[AddressServiceConstants.CountryRegionId] = address.ThreeLetterISORegionName;
    
                if (!string.IsNullOrWhiteSpace(address.State))
                {
                    parameters[AddressServiceConstants.StateProvinceId] = address.State;
                }
    
                if (!string.IsNullOrWhiteSpace(address.County))
                {
                    parameters[AddressServiceConstants.CountyId] = address.County;
                }
    
                if (!string.IsNullOrWhiteSpace(address.City))
                {
                    parameters[AddressServiceConstants.CityComponentName] = address.City;
                }
    
                if (!string.IsNullOrWhiteSpace(address.DistrictName))
                {
                    parameters[AddressServiceConstants.DistrictId] = address.DistrictName;
                }
    
                if (!string.IsNullOrWhiteSpace(address.ZipCode))
                {
                    parameters[AddressServiceConstants.ZipPostalCodeComponentName] = address.ZipCode;
                }
    
                int result;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    result = sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(ValidateAddressSprocName, parameters);
                }
    
                if (result == 0)
                {
                    return new ValidateAddressDataResponse(true);
                }
    
                Tuple<DataValidationErrors, string> faultAddressComponent = HandleNonZeroResult(result);
    
                return CreateFailedValidateAddressDataResponse(faultAddressComponent.Item1, faultAddressComponent.Item2);
            }
    
            private EntityDataServiceResponse<AddressFormattingInfo> GetAddressFormatting(GetAddressFormattingDataRequest request)
            {
                ThrowIf.Null(request.CountryRegionCode, "countryRegionCode");
    
                ParameterSet parameters = new ParameterSet();
                parameters[AddressServiceConstants.CountryRegionId] = request.CountryRegionCode;
    
                PagedResult<AddressFormattingInfo> results;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    results = sqlServerDatabaseContext.ExecuteStoredProcedure<AddressFormattingInfo>(GetAddressFormattingInfoSprocName, parameters);
                }
    
                return new EntityDataServiceResponse<AddressFormattingInfo>(results);
            }
        }
    }
}
