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
        /// The SQL server implementation of getting offline pending transactions count.
        /// </summary>
        internal sealed class GetOfflinePendingTransactionCountProcedure
        {
            private const string RetailOfflinePendingTransactionCountView = "RETAILOFFLINEPENDINGTRANSACTIONCOUNTVIEW";
            private const string RetailOfflinePendingTransactionCountColumn = "PENDINGTRANSACTIONCOUNT";
            private GetOfflinePendingTransactionCountDataRequest request;
            private SqlServerDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetOfflinePendingTransactionCountProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetOfflinePendingTransactionCountProcedure(GetOfflinePendingTransactionCountDataRequest request, SqlServerDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            /// <summary>
            /// Gets pending transactions count.
            /// </summary>
            /// <returns>The GetOfflinePendingTransactionCount response message.</returns>
            public GetOfflinePendingTransactionCountDataResponse GetPendingTransactionCount()
            {
                var query = new SqlPagedQuery(this.request.QueryResultSettings)
                {
                    From = RetailOfflinePendingTransactionCountView,
                    OrderBy = RetailOfflinePendingTransactionCountColumn
                };
    
                long numberOfTransactions = (long)this.databaseContext.ExecuteScalar<int>(query);
                return new GetOfflinePendingTransactionCountDataResponse(numberOfTransactions);
            }
        }
    }
}
