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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.Runtime.Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Address SQLite database accessor class.
        /// </summary>
        public class AddressSqliteDatabaseAccessor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AddressSqliteDatabaseAccessor"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public AddressSqliteDatabaseAccessor(RequestContext context)
            {
                this.Context = context;
            }
    
            /// <summary>
            /// Gets or sets the request context.
            /// </summary>
            private RequestContext Context { get; set; }
    
            /// <summary>
            /// Gets the country region details with address formatting info.
            /// </summary>
            /// <param name="languageId">The language identifier.</param>
            /// <returns>Returns the tuple collection of country region info and address formatting info.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Backward compatibility.")]
            public Tuple<ReadOnlyCollection<CountryRegionInfo>, ReadOnlyCollection<AddressFormattingInfo>> GetCountryRegions(string languageId)
            {
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    var countryRegions = AddressQueries.GetCountryRegionIds(context, languageId);
    
                    IEnumerable<string> countryRegionIds = countryRegions.Results.Select(countryRegionId => countryRegionId.CountryRegionId);
    
                    using (TempTable countryRegionIdsTempTable = TempTableHelper.CreateScalarTempTable(context, "RECID", countryRegionIds))
                    {
                        ReadOnlyCollection<CountryRegionInfo> countryRegionInfo = AddressQueries.GetCountryRegionInfo(context, languageId, countryRegionIdsTempTable).Results;
    
                        ReadOnlyCollection<AddressFormattingInfo> formattingInfos = AddressQueries.GetAddressFormatInfo(context, countryRegionIdsTempTable).Results;
    
                        return new Tuple<ReadOnlyCollection<CountryRegionInfo>, ReadOnlyCollection<AddressFormattingInfo>>(countryRegionInfo, formattingInfos);
                    }
                }
            }
        }
    }
}
