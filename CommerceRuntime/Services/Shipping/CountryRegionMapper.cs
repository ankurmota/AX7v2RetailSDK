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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;

        /// <summary>
        /// Encapsulates the functionality required to convert between two-letter
        /// and three-letter country codes.
        /// </summary>
        internal sealed class CountryRegionMapper
        {
            /// <summary>
            /// Three digit to two digit lookup.
            /// </summary>
            private readonly IDictionary<string, string> mapCountryRegionToIsoCode;
    
            /// <summary>
            /// Two digit to three digit lookup.
            /// </summary>
            private readonly IDictionary<string, string> mapIsoCodeToCountryRegion;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CountryRegionMapper"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public CountryRegionMapper(RequestContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                GetDefaultLanguageIdDataRequest languageIdRequest = new GetDefaultLanguageIdDataRequest();
                string languageId = context.Execute<SingleEntityDataServiceResponse<string>>(languageIdRequest).Entity;
                GetCountryRegionDataRequest countryRequest = new GetCountryRegionDataRequest(languageId)
                {
                    QueryResultSettings = QueryResultSettings.AllRecords
                };
    
                var response = context.Execute<GetCountryRegionDataResponse>(countryRequest);
    
                IEnumerable<CountryRegionInfo> countryRegions = response.CountryRegionInfo.Results;
    
                this.mapCountryRegionToIsoCode = countryRegions.ToDictionary(key => key.CountryRegionId, value => value.ISOCode);
                this.mapIsoCodeToCountryRegion = countryRegions.ToDictionary(key => key.ISOCode, value => value.CountryRegionId);
            }
    
            /// <summary>
            /// Converts a two letter country code to its three letter equivalent.
            /// </summary>
            /// <param name="twoLetterCountryCode">The two letter country code.</param>
            /// <returns>
            /// The three letter country code.
            /// </returns>
            public string ConvertToThreeLetterCountryCode(string twoLetterCountryCode)
            {
                if (string.IsNullOrWhiteSpace(twoLetterCountryCode))
                {
                    throw new ArgumentNullException("twoLetterCountryCode");
                }
    
                return this.mapIsoCodeToCountryRegion[twoLetterCountryCode];
            }
    
            /// <summary>
            /// Converts a two letter country code to its three letter equivalent.
            /// </summary>
            /// <param name="threeLetterCountryCode">The three letter country code.</param>
            /// <returns>
            /// The two letter country code.
            /// </returns>
            public string ConvertToTwoLetterCountryCode(string threeLetterCountryCode)
            {
                if (string.IsNullOrWhiteSpace(threeLetterCountryCode))
                {
                    throw new ArgumentNullException("threeLetterCountryCode");
                }
    
                return this.mapCountryRegionToIsoCode[threeLetterCountryCode];
            }
        }
    }
}
