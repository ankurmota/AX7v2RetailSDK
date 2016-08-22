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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates the implementation of the logging service.
        /// </summary>
        public class LoggingService : IRequestHandler
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
                        typeof(InsertAuditLogServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Execute the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(InsertAuditLogServiceRequest))
                {
                    response = InsertAuditLog((InsertAuditLogServiceRequest)request);
                }
                else
                {
                    RetailLogger.Log.CrtServicesUnsupportedRequestType(request.GetType(), "LoggingService");
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Inserts Audit Log.
            /// </summary>
            /// <param name="request">The audit log request.</param>
            /// <returns>The response.</returns>
            private static NullResponse InsertAuditLog(InsertAuditLogServiceRequest request)
            {
                string storeId = string.Empty;
                string terminalId = string.Empty;
                bool auditEnabled = true;
    
                try
                {
                    var context = request.RequestContext;
    
                    if (context.GetPrincipal() != null)
                    {
                        Terminal terminal = context.GetTerminal();
                        if (terminal != null)
                        {
                            terminalId = terminal.TerminalId;
    
                            // Get device information
                            if (!context.GetPrincipal().IsChannelAgnostic)
                            {
                                try
                                {
                                    DeviceConfiguration deviceConfiguration = context.GetDeviceConfiguration();
                                    if (deviceConfiguration != null)
                                    {
                                        auditEnabled = deviceConfiguration.AuditEnabled;
                                    }
    
                                    // Try to get store information
                                    if (request.RequestContext.GetOrgUnit() != null)
                                    {
                                        storeId = request.RequestContext.GetOrgUnit().OrgUnitNumber ?? string.Empty;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // If anything bad happens, log an event and continue.
                                    RetailLogger.Log.CrtServicesLoggingServiceWriteEntryFailure(ex);
                                }
                            }
                        }
                    }
    
                    if (auditEnabled)
                    {
                        if (string.IsNullOrEmpty(storeId))
                        {
                            storeId = @"<not set>";
                        }
    
                        if (string.IsNullOrEmpty(terminalId))
                        {
                            terminalId = @"<not set>";
                        }
    
                        var dataRequest = new InsertAuditLogDataRequest(
                            request.Source,
                            request.LogEntry,
                            request.LogLevel,
                            storeId,
                            terminalId,
                            request.DurationInMilliseconds);
    
                        context.Execute<NullResponse>(dataRequest);
                    }
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.CrtServicesLoggingServiceWriteEntryFailure(ex);
                }
    
                return new NullResponse();
            }
        }
    }
}
