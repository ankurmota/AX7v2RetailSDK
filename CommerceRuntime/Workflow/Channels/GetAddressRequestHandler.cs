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

namespace Contoso
{
    namespace Commerce.Runtime.Workflow
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        
        /// <summary>
        /// Gets the global address values that consists of countries, counties, state provinces, cities.
        /// </summary>
        public class GetAddressRequestHandler : SingleRequestHandler<GetAddressRequest, GetAddressResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch the Address information.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetAddressRequest"/>.</param>
            /// <returns>Instance of <see cref="GetAddressResponse"/>.</returns>
            protected override GetAddressResponse Process(GetAddressRequest request)
            {
                ThrowIf.Null(request, "request");
    
                GetAddressResponse response;
    
                switch (request.AddressFilter)
                {
                    case AddressFilter.Countries:
                        {
                            response = this.GetCountries(request);
                            break;
                        }
    
                    case AddressFilter.StateProvinces:
                        {
                            response = this.GetStateProvinces(request);
                            break;
                        }
    
                    case AddressFilter.Counties:
                        {
                            response = this.GetCounties(request);
                            break;
                        }
    
                    case AddressFilter.Cities:
                        {
                            response = this.GetCities(request);
                            break;
                        }
    
                    case AddressFilter.ZipCodes:
                        {
                            response = this.GetZipcodes(request);
                            break;
                        }
    
                    case AddressFilter.Districts:
                        {
                            response = this.GetDistricts(request);
                            break;
                        }
    
                    case AddressFilter.AddressByZipCode:
                        {
                            response = this.GetAddressFromZipCode(request);
                            break;
                        }
    
                    default:
                        {
                            response = new GetAddressResponse();
                            break;
                        }
                }
    
                return response;
            }
    
            private GetAddressResponse GetDistricts(GetAddressRequest request)
            {
                var serviceRequest = new GetDistrictServiceRequest(request.CountryRegionId, request.StateProvinceId, request.CountyId, request.City, request.QueryResultSettings)
                {
                    QueryResultSettings = request.QueryResultSettings
                };

                var serviceResponse = this.Context.Execute<GetDistrictServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            private GetAddressResponse GetZipcodes(GetAddressRequest request)
            {
                var serviceRequest = new GetZipCodesServiceRequest(request.CountryRegionId, request.StateProvinceId, request.CountyId, request.City, request.District)
                {
                    QueryResultSettings = request.QueryResultSettings
                };

                var serviceResponse = this.Context.Execute<GetZipCodesServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            private GetAddressResponse GetCities(GetAddressRequest request)
            {
                var serviceRequest = new GetCitiesServiceRequest(request.CountryRegionId, request.StateProvinceId, request.CountyId);
                serviceRequest.QueryResultSettings = request.QueryResultSettings;
    
                var serviceResponse = this.Context.Execute<GetCitiesServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            private GetAddressResponse GetCounties(GetAddressRequest request)
            {
                var serviceRequest = new GetCountiesServiceRequest(request.CountryRegionId, request.StateProvinceId)
                {
                    QueryResultSettings = request.QueryResultSettings
                };

                var serviceResponse = this.Context.Execute<GetCountiesServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            private GetAddressResponse GetStateProvinces(GetAddressRequest request)
            {
                var serviceRequest = new GetStateProvincesServiceRequest(request.CountryRegionId, request.QueryResultSettings)
                {
                    QueryResultSettings = request.QueryResultSettings
                };

                var serviceResponse = this.Context.Execute<GetStateProvincesServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            /// <summary>
            /// Executes WorkFlow associated with retrieving list of countries. 
            /// </summary>
            /// <param name="request">Instance of <see cref="GetAddressRequest"/>.</param>
            /// <returns>Instance of <see cref="GetAddressResponse"/>.</returns>
            private GetAddressResponse GetCountries(GetAddressRequest request)
            {
                // setting language Id value to null if language Id not passing
                if (string.IsNullOrWhiteSpace(request.LanguageId))
                {
                    request.LanguageId = null;
                }
    
                var serviceRequest = new GetCountryRegionsServiceRequest(request.LanguageId, request.QueryResultSettings);
    
                var serviceResponse = this.Context.Execute<GetCountryRegionsServiceResponse>(serviceRequest);
    
                return new GetAddressResponse(serviceResponse.Results);
            }
    
            /// <summary>
            /// Executes Workflow associated with retrieving list of addresses according to passed zip\postal code.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetAddressRequest"/>.</param>        
            /// <returns>Instance of <see cref="GetAddressResponse"/>.</returns>
            private GetAddressResponse GetAddressFromZipCode(GetAddressRequest request)
            {
                var serviceRequest = new GetFromZipPostalCodeServiceRequest(request.CountryRegionId, request.ZipCode)
                {
                    QueryResultSettings = request.QueryResultSettings
                };

                var serviceResponse = this.Context.Execute<GetFromZipPostalCodeServiceResponse>(serviceRequest);                
    
                return new GetAddressResponse(serviceResponse.Results);
            }
        }
    }
}
