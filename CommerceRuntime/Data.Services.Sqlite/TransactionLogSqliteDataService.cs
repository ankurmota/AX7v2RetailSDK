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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// The transaction data service to log a transactions.
        /// </summary>
        public class TransactionLogSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(InsertTransactionLogDataRequest) };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>
            /// The outgoing response message.
            /// </returns>
            /// <exception cref="System.NotSupportedException">Thrown if the current request is not supported.</exception>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(InsertTransactionLogDataRequest))
                {
                    response = Save((InsertTransactionLogDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Saves the transaction log.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The empty response.</returns>
            private static NullResponse Save(InsertTransactionLogDataRequest request)
            {
                var allTables = new DataTable[] { request.TransactionLogTable };
    
                request.TransactionLogTable.TableName = "ax.RETAILTRANSACTIONTABLE";
                request.TransactionLogTable.Columns.Add("CREATEDOFFLINE", typeof(int));
    
                foreach (DataRow row in request.TransactionLogTable.Rows)
                {
                    row["CREATEDOFFLINE"] = 1; // set offline flag to true.
                }
    
                using (var databaseContext = new SqliteDatabaseContext(request.RequestContext))
                using (var transaction = databaseContext.BeginTransaction())
                {
                    foreach (DataTable dataTable in allTables)
                    {
                        databaseContext.SaveTable(dataTable);
                    }
    
                    transaction.Commit();
                }
    
                return new NullResponse();
            }
        }
    }
}
