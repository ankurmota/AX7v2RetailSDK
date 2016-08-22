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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Shifts data service class.
        /// </summary>
        public class ShiftSqliteDataService : IRequestHandler
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
                        typeof(UpdateShiftStagingTableDataRequest),
                        typeof(CreateShiftDataRequest),
                        typeof(GetShiftDataDataRequest),
                        typeof(DeleteShiftDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(UpdateShiftStagingTableDataRequest))
                {
                    response = this.UpdateShiftsStagingTable((UpdateShiftStagingTableDataRequest)request);
                }
                else if (requestType == typeof(CreateShiftDataRequest))
                {
                    response = this.InsertShiftsStagingTable((CreateShiftDataRequest)request);
                }
                else if (requestType == typeof(GetShiftDataDataRequest))
                {
                    response = this.GetShiftData((GetShiftDataDataRequest)request);
                }
                else if (requestType == typeof(DeleteShiftDataRequest))
                {
                    response = this.DeleteShiftStagingTable((DeleteShiftDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private Response GetShiftData(GetShiftDataDataRequest request)
            {
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(request.RequestContext))
                {
                    GetShiftDataProcedure procedure = new GetShiftDataProcedure(request.Criteria, databaseContext);
                    return procedure.Execute();
                }
            }
    
            private NullResponse InsertShiftsStagingTable(CreateShiftDataRequest request)
            {
                var databaseAccessor = new ShiftsSqliteDatabaseAccessor(request.RequestContext);
                databaseAccessor.InsertShiftStaging(request.Shift);
    
                return new NullResponse();
            }
    
            private NullResponse UpdateShiftsStagingTable(UpdateShiftStagingTableDataRequest request)
            {
                var databaseAccessor = new ShiftsSqliteDatabaseAccessor(request.RequestContext);
                databaseAccessor.UpdateShiftStaging(request.Shift);
    
                return new NullResponse();
            }
    
            private NullResponse DeleteShiftStagingTable(DeleteShiftDataRequest request)
            {
                var databaseAccessor = new ShiftsSqliteDatabaseAccessor(request.RequestContext);
                databaseAccessor.DeleteShiftStaging(request.Shift);
    
                return new NullResponse();
            }
        }
    }
}
