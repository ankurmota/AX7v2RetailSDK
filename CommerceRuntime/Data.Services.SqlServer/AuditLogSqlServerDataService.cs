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
        /// Audit log data service class.
        /// </summary>
        public class AuditLogSqlServerDataService : IRequestHandler
        {
            // Stored procedure name
            private const string InsertAuditLogSprocName = "INSERTAUDITLOG";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(InsertAuditLogDataRequest) };
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
    
                if (requestType == typeof(InsertAuditLogDataRequest))
                {
                    response = this.InsertAuditLog((InsertAuditLogDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Inserts audit log.
            /// </summary>
            /// <param name="request">The <see cref="InsertAuditLogDataRequest"/>log request.</param>
            /// <returns>The <see cref="NullResponse"/>response.</returns>
            private NullResponse InsertAuditLog(InsertAuditLogDataRequest request)
            {
                // Insert
                ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
    
                // RetailLog is an AX table, DataAreaId field is mandatory. We need to confirm channelConfiguration is present
                // before inserting data to RetailLog, otherwise P-job will fail due to DataAreaId is missing.
                if (channelConfiguration != null)
                {
                    var parameters = new ParameterSet();
                    parameters["@RETAILLOGID"] = DateTimeOffset.UtcNow.Ticks;
                    parameters["@DATE"] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    parameters["@CODEUNIT"] = request.Source;
                    parameters["@LOGSTRING"] = request.LogEntry;
                    parameters["@LOGLEVEL"] = request.LogLevel;
                    parameters["@STOREID"] = request.StoreId;
                    parameters["@TERMINALID"] = request.TerminalId;
                    parameters["@DURATIONINMILLISEC"] = request.DurationInMilliseconds;
                    parameters["@DATAAREAID"] = channelConfiguration.InventLocationDataAreaId;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        sqlServerDatabaseContext.ExecuteStoredProcedureNonQuery(AuditLogSqlServerDataService.InsertAuditLogSprocName, parameters);
                    }
                }
    
                return new NullResponse();
            }
        }
    }
}
