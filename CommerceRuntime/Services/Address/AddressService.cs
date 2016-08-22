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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Address service reference implementation.
        /// </summary>
        public class AddressService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetCountryRegionsServiceRequest),
                        typeof(GetStateProvincesServiceRequest),
                        typeof(GetCountiesServiceRequest),
                        typeof(GetCitiesServiceRequest),
                        typeof(GetDistrictServiceRequest),
                        typeof(GetZipCodesServiceRequest),
                        typeof(GetFromZipPostalCodeServiceRequest),
                        typeof(GetAddressFormattingServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
                Type requestType = request.GetType();
    
                if (requestType == typeof(GetCountryRegionsServiceRequest))
                {
                    response = GetCountryRegions((GetCountryRegionsServiceRequest)request);
                }
                else if (requestType == typeof(GetStateProvincesServiceRequest))
                {
                    response = GetStateProvinces((GetStateProvincesServiceRequest)request);
                }
                else if (requestType == typeof(GetCountiesServiceRequest))
                {
                    response = GetCounties((GetCountiesServiceRequest)request);
                }
                else if (requestType == typeof(GetCitiesServiceRequest))
                {
                    response = GetCities((GetCitiesServiceRequest)request);
                }
                else if (requestType == typeof(GetDistrictServiceRequest))
                {
                    response = GetDistricts((GetDistrictServiceRequest)request);
                }
                else if (requestType == typeof(GetZipCodesServiceRequest))
                {
                    response = GetZipCodes((GetZipCodesServiceRequest)request);
                }
                else if (requestType == typeof(GetFromZipPostalCodeServiceRequest))
                {
                    response = GetFromZipPostalCode((GetFromZipPostalCodeServiceRequest)request);
                }
                else if (requestType == typeof(GetAddressFormattingServiceRequest))
                {
                    response = GetAddressFormatting((GetAddressFormattingServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the mandatory filter names.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// List of mandatory filter names.
            /// </returns>
            private static IEnumerable<string> GetMandatoryFilterNames(GetAddressInfoServiceRequest request)
            {
                Collection<string> filterNames = new Collection<string>();
    
                if (request is GetCountryRegionsServiceRequest)
                {
                    // languageId is required
                    filterNames.Add(AddressServiceConstants.LanguageIdSqlParameter);
                }
                else
                {
                    // countryregionId is mandatory
                    filterNames.Add(AddressServiceConstants.CountryRegionId);
    
                    // for GetFromZipPostalCodeRequesty both ZipCode and countryRegionId is required.
                    if (request is GetFromZipPostalCodeServiceRequest)
                    {
                        filterNames.Add(AddressServiceConstants.ZipCode);
                    }
                }
    
                return filterNames;
            }
    
            /// <summary>
            /// Validates the get address info requests.
            /// </summary>
            /// <param name="request">The request.</param>
            private static void ValidateGetAddressInfoRequests(GetAddressInfoServiceRequest request)
            {
                ServicesHelper.ValidateInboundRequest(request);
    
                // ensure all required filters are given.
                foreach (var filterName in GetMandatoryFilterNames(request))
                {
                    if (request.Filters[filterName] == null || string.IsNullOrWhiteSpace(request.Filters[filterName].ToString()))
                    {
                        string errorMessage = string.Format(CultureInfo.CurrentCulture, "Empty filter specified: {0} is required to call the API.", filterName);
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, errorMessage);
                    }
                }
            }
    
            /// <summary>
            /// Gets the address formatting.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>List of address component and associated metadata that prescribes how to format/render in UI.</returns>
            /// <remarks>
            /// AX only supports the following address components: street, city, county, state, zip code, country.
            /// </remarks>
            private static GetAddressFormattingServiceResponse GetAddressFormatting(GetAddressFormattingServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                GetAddressFormattingDataRequest getAddressFormattingDataRequest = new GetAddressFormattingDataRequest(countryRegionId);
                PagedResult<AddressFormattingInfo> results = request.RequestContext.Execute<EntityDataServiceResponse<AddressFormattingInfo>>(
                                                                        getAddressFormattingDataRequest).PagedEntityCollection;
    
                if (!results.Results.Any())
                {
                    NetTracer.Warning(
                        "No formatting information found for country {0}. Ensure you run job A/N-1010 properly.",
                        request.Filters[AddressServiceConstants.CountryRegionId] ?? string.Empty);
                }
    
                return new GetAddressFormattingServiceResponse(results);
            }
    
            /// <summary>
            /// Gets from zip postal code.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response..</returns>
            private static GetFromZipPostalCodeServiceResponse GetFromZipPostalCode(GetFromZipPostalCodeServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                string zipCode = string.Empty;
    
                if (request.Filters[AddressServiceConstants.ZipCode] != null)
                {
                    zipCode = request.Filters[AddressServiceConstants.ZipCode].ToString();
                }
    
                var getZipcodeDataRequest = new GetFromZipPostalCodeDataRequest(countryRegionId, zipCode);
                getZipcodeDataRequest.QueryResultSettings = request.QueryResultSettings;
                var zipcodeDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ZipCodeInfo>>(getZipcodeDataRequest, request.RequestContext);
    
                return new GetFromZipPostalCodeServiceResponse(zipcodeDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the zip codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetZipCodesServiceResponse GetZipCodes(GetZipCodesServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                string stateId = string.Empty;
                string city = string.Empty;
                string countyId = string.Empty;
                string district = string.Empty;
    
                if (request.Filters[AddressServiceConstants.StateProvinceId] != null)
                {
                    stateId = request.Filters[AddressServiceConstants.StateProvinceId].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.CountyId] != null)
                {
                    countyId = request.Filters[AddressServiceConstants.CountyId].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.CityComponentName] != null)
                {
                    city = request.Filters[AddressServiceConstants.CityComponentName].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.DistrictId] != null)
                {
                    district = request.Filters[AddressServiceConstants.DistrictId].ToString();
                }
    
                var getZipcodeDataRequest = new GetZipPostalCodeDataRequest(countryRegionId, stateId, countyId, city, district);
                getZipcodeDataRequest.QueryResultSettings = request.QueryResultSettings;
                var zipcodeDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ZipCodeInfo>>(getZipcodeDataRequest, request.RequestContext);
    
                return new GetZipCodesServiceResponse(zipcodeDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the districts.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetDistrictServiceResponse GetDistricts(GetDistrictServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                string stateId = string.Empty;
                string city = string.Empty;
                string countyId = string.Empty;
    
                if (request.Filters[AddressServiceConstants.StateProvinceId] != null)
                {
                    stateId = request.Filters[AddressServiceConstants.StateProvinceId].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.CountyId] != null)
                {
                    countyId = request.Filters[AddressServiceConstants.CountyId].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.CityComponentName] != null)
                {
                    city = request.Filters[AddressServiceConstants.CityComponentName].ToString();
                }
    
                var getDistrictDataRequest = new GetDistrictsDataRequest(countryRegionId, stateId, countyId, city, request.QueryResultSettings);
                var districtDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<DistrictInfo>>(getDistrictDataRequest, request.RequestContext);
    
                return new GetDistrictServiceResponse(districtDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the cities.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetCitiesServiceResponse GetCities(GetCitiesServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                string stateId = string.Empty;
                string countyId = string.Empty;
    
                if (request.Filters[AddressServiceConstants.StateProvinceId] != null)
                {
                    stateId = request.Filters[AddressServiceConstants.StateProvinceId].ToString();
                }
    
                if (request.Filters[AddressServiceConstants.CountyId] != null)
                {
                    countyId = request.Filters[AddressServiceConstants.CountyId].ToString();
                }
    
                var getCityDataRequest = new GetCitiesDataRequest(countryRegionId, stateId, countyId, request.QueryResultSettings);
                var cityDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<CityInfo>>(getCityDataRequest, request.RequestContext);
    
                return new GetCitiesServiceResponse(cityDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the counties.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetCountiesServiceResponse GetCounties(GetCountiesServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                string stateId = string.Empty;
    
                if (request.Filters[AddressServiceConstants.StateProvinceId] != null)
                {
                    stateId = request.Filters[AddressServiceConstants.StateProvinceId].ToString();
                }
    
                var getCountiesDataRequest = new GetCountiesDataRequest(countryRegionId, stateId, request.QueryResultSettings);
                var countyInfoDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<CountyInfo>>(getCountiesDataRequest, request.RequestContext);
    
                return new GetCountiesServiceResponse(countyInfoDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the state provinces.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetStateProvincesServiceResponse GetStateProvinces(GetStateProvincesServiceRequest request)
            {
                ValidateGetAddressInfoRequests(request);
    
                string countryRegionId = request.Filters[AddressServiceConstants.CountryRegionId].ToString();
    
                var stateProvinceRequest = new GetStateProvincesDataRequest(countryRegionId, request.QueryResultSettings);
                var stateProvinceDataSet = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<StateProvinceInfo>>(stateProvinceRequest, request.RequestContext);
    
                return new GetStateProvincesServiceResponse(stateProvinceDataSet.PagedEntityCollection);
            }
    
            /// <summary>
            /// Gets the county/region information for the given locale/languageId.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetCountryRegionsServiceResponse GetCountryRegions(GetCountryRegionsServiceRequest request)
            {
                string languageId = (string)request.Filters[AddressServiceConstants.LanguageIdSqlParameter];
    
                if (!string.IsNullOrWhiteSpace(languageId))
                {
                    // Do not specify the query settings as country request query setting does not apply for languages.
                    GetSupportedLanguagesDataRequest getLanguagesDataRequest = new GetSupportedLanguagesDataRequest();
                    var languages = request.RequestContext.Execute<EntityDataServiceResponse<SupportedLanguage>>(getLanguagesDataRequest).PagedEntityCollection.Results;
    
                    var languageIds = new HashSet<string>(languages.Select(l => l.LanguageId).ToList());
    
                    if (!languageIds.Contains(languageId, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnsupportedLanguage,
                            string.Format("Unsupported Language Id specified {0}", languageId));
                    }
                }
    
                var countryRequest = new GetCountryRegionDataRequest(languageId) { QueryResultSettings = request.QueryResultSettings };
                GetCountryRegionDataResponse countryDataSets = request.RequestContext.Execute<GetCountryRegionDataResponse>(countryRequest);
    
                ReadOnlyCollection<CountryRegionInfo> results = countryDataSets.CountryRegionInfo.Results;
                if (results.Any())
                {
                    var addressFormattingResults = countryDataSets.AddressFormattingInfo;
                    if (addressFormattingResults != null)
                    {
                        // Construct the lists of addressFormattingInfo per country
                        var addressFormattingDictionary = new Dictionary<string, IList<AddressFormattingInfo>>();
    
                        foreach (var addressFormattingInfo in addressFormattingResults)
                        {
                            if (!addressFormattingDictionary.ContainsKey(addressFormattingInfo.CountryRegionId))
                            {
                                addressFormattingDictionary[addressFormattingInfo.CountryRegionId] = new List<AddressFormattingInfo>();
                            }
    
                            addressFormattingDictionary[addressFormattingInfo.CountryRegionId].Add(addressFormattingInfo);
                        }
    
                        // Now populate the address formatting info to the countries.
                        foreach (CountryRegionInfo country in results)
                        {
                            if (addressFormattingDictionary.ContainsKey(country.CountryRegionId))
                            {
                                country.AddressFormatLines = addressFormattingDictionary[country.CountryRegionId];
                            }
                        }
                    }
                }
                else
                {
                    // If not country is found, that is certainly a data syncronization issue.
                    NetTracer.Warning("No country information could be found. Ensure you run job A/N-1010 properly.");
                }
    
                return new GetCountryRegionsServiceResponse(countryDataSets.CountryRegionInfo);
            }
        }
    }
}
