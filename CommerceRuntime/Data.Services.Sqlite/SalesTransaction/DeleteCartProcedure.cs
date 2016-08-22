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
    namespace Commerce.Runtime.DataServices.Sqlite.DataServices.SalesTransaction
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Helper class for delete cart procedure implementation.
        /// </summary>
        internal sealed class DeleteCartProcedure
        {
            private DeleteCartDataRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DeleteCartProcedure"/> class.
            /// </summary>
            /// <param name="request">The delete cart data request.</param>
            public DeleteCartProcedure(DeleteCartDataRequest request)
            {
                this.request = request;
            }
    
            /// <summary>
            /// Executes the procedure.
            /// </summary>
            public void Execute()
            {
                const string UpdateDeleteDateQueryCommand = @"
                    UPDATE crt.SALESTRANSACTION SET
                        DELETEDDATETIME = @deteleDateTime
                    WHERE
                        TRANSACTIONID = @transactionId;";
    
                var query = new SqlQuery(UpdateDeleteDateQueryCommand);
                query.Parameters["@deteleDateTime"] = DateTime.UtcNow;
    
                using (var databaseContext = new SqliteDatabaseContext(this.request.RequestContext))
                using (var transaction = databaseContext.BeginTransaction())
                {
                    foreach (string salesTransactionId in this.request.SalesTransactionIds)
                    {
                        query.Parameters["@transactionId"] = salesTransactionId;
                        databaseContext.ExecuteNonQuery(query);
                    }
    
                    transaction.Commit();
                }
            }
        }
    }
}
