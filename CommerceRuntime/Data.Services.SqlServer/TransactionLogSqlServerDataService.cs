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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The transaction data service to log a transactions.
        /// </summary>
        public class TransactionLogSqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return new[] { typeof(InsertTransactionLogDataRequest) }; }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>
            /// The outgoing response message.
            /// </returns>
            /// <exception cref="System.NotSupportedException">The request type is not supported.</exception>
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
                ParameterSet parameters = new ParameterSet();
    
                parameters["@bi_ChannelId"] = request.ChannelId;
                parameters["@TVP_TransactionTable"] = request.TransactionLogTable;
    
                int errorCode;
    
                using (var databaseContext = new SqlServerDatabaseContext(request))
                {
                    errorCode = databaseContext.ExecuteStoredProcedureNonQuery("INSERTTRANSACTION", parameters);
                }
    
                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save transaction log.");
                }
    
                return new NullResponse();
            }
        }
    }
}
