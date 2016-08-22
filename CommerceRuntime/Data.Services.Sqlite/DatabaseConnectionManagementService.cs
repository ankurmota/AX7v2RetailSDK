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
        using Data.Sqlite;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages.ReleaseConnection;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Database connection management service which is responsible to release SQLite connections.
        /// </summary>
        public class DatabaseConnectionManagementService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types for this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return new[] { typeof(ReleaseConnectionServiceRequest) }; }
            }
    
            /// <summary>
            /// Entry point to the database connection management service of the request execution.
            /// </summary>
            /// <param name="request">The incoming request type.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(ReleaseConnectionServiceRequest))
                {
                    response = ReleaseConnection((ReleaseConnectionServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Get a database provider.
            /// </summary>
            /// <param name="context">The current request context.</param>
            /// <returns>Database provider.</returns>
            private static SqliteDatabaseProvider GetDatabaseProvider(RequestContext context)
            {
                return (SqliteDatabaseProvider)context.Runtime.Configuration.DatabaseProvider;
            }
    
            /// <summary>
            /// Releases the connection.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static ReleaseConnectionServiceResponse ReleaseConnection(ReleaseConnectionServiceRequest request)
            {
                SqliteDatabaseProvider databaseProvider = DatabaseConnectionManagementService.GetDatabaseProvider(request.RequestContext);
                bool released = databaseProvider.ReleaseConnection(request.ConnectionString);
                return new ReleaseConnectionServiceResponse(released);
            }
        }
    }
}
