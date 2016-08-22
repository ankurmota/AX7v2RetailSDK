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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Data service for managing offline sync statistics from SQL server database.
        /// </summary>
        public class OfflineSyncStatsSqlServerDataService : IRequestHandler
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
                        typeof(GetOfflineSyncStatsDataRequest),
                        typeof(GetOfflinePendingTransactionCountDataRequest),
                    };
                }
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
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetOfflineSyncStatsDataRequest))
                {
                    response = this.GetOfflineSyncStatsLines((GetOfflineSyncStatsDataRequest)request);
                }
                else if (requestType == typeof(GetOfflinePendingTransactionCountDataRequest))
                {
                    response = this.GetOfflinePendingTransactionCount((GetOfflinePendingTransactionCountDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private EntityDataServiceResponse<OfflineSyncStatsLine> GetOfflineSyncStatsLines(GetOfflineSyncStatsDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetOfflineSyncStatsLinesProcedure getOfflineSyncStatsLinesProcedure = new GetOfflineSyncStatsLinesProcedure(request, databaseContext);
                    return getOfflineSyncStatsLinesProcedure.Execute();
                }
            }
    
            private GetOfflinePendingTransactionCountDataResponse GetOfflinePendingTransactionCount(GetOfflinePendingTransactionCountDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    GetOfflinePendingTransactionCountProcedure getOfflinePendingTransactionCountProcedure = new GetOfflinePendingTransactionCountProcedure(request, databaseContext);
                    return getOfflinePendingTransactionCountProcedure.GetPendingTransactionCount();
                }
            }
        }
    }
}
