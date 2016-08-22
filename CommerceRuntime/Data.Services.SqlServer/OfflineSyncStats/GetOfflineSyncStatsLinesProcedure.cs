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
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// The SQL server implementation of getting OfflineSyncStatsLine data entity.
        /// </summary>
        internal sealed class GetOfflineSyncStatsLinesProcedure
        {
            private const string RetailOfflineSyncStatsView = "RETAILOFFLINESYNCSTATSVIEW";
            private const string RetailOfflineSyncStatsSorting = "LASTSYNCDATETIME DESC";
            private GetOfflineSyncStatsDataRequest request;
            private SqlServerDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetOfflineSyncStatsLinesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetOfflineSyncStatsLinesProcedure(GetOfflineSyncStatsDataRequest request, SqlServerDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            /// <summary>
            /// Retrieves the OfflineSyncStatsLine entity data service response.
            /// </summary>
            /// <returns>The entity data service response.</returns>
            public EntityDataServiceResponse<OfflineSyncStatsLine> Execute()
            {
                var query = new SqlPagedQuery(this.request.QueryResultSettings)
                {
                    From = RetailOfflineSyncStatsView,
                    OrderBy = RetailOfflineSyncStatsSorting
                };
    
                PagedResult<OfflineSyncStatsLine> results = this.databaseContext.ReadEntity<OfflineSyncStatsLine>(query);
                return new EntityDataServiceResponse<OfflineSyncStatsLine>(results);
            }
        }
    }
}
