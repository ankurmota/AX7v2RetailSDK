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
        /// The SQL server implementation of getting sales lines.
        /// </summary>
        internal sealed class GetSalesLinesProcedure
        {
            private const string RetailTransactionSalesTransView = "RETAILTRANSACTIONSALESTRANSVIEW";
    
            private GetSalesLinesDataRequest request;
            private SqlServerDatabaseContext databaseContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetSalesLinesProcedure"/> class.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <param name="databaseContext">The database context object.</param>
            public GetSalesLinesProcedure(GetSalesLinesDataRequest request, SqlServerDatabaseContext databaseContext)
            {
                this.request = request;
                this.databaseContext = databaseContext;
            }
    
            public EntityDataServiceResponse<SalesLine> Execute()
            {
                using (StringIdTableType transactionIdTableType = new StringIdTableType(this.request.Criteria.TransactionIds, RetailTransactionTableSchema.TransactionIdColumn))
                {
                    var query = new SqlPagedQuery(this.request.QueryResultSettings)
                    {
                        From = RetailTransactionSalesTransView,
                    };
                    query.Parameters["@TVP_TRANSACTIONIDTABLETYPE"] = transactionIdTableType;
    
                    PagedResult<SalesLine> results = this.databaseContext.ReadEntity<SalesLine>(query);
                    return new EntityDataServiceResponse<SalesLine>(results);
                }
            }
        }
    }
}
