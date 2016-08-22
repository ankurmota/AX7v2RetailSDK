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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Terminal data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class TerminalDataService : IRequestHandler
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
                        typeof(GetTerminalDataRequest),
                        typeof(GetPaymentConnectorDataRequest),
                        typeof(GetCurrentTerminalIdDataRequest),
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
    
                if (requestType == typeof(GetTerminalDataRequest))
                {
                    response = this.GetTerminalByCriteria((GetTerminalDataRequest)request);
                }
                else if (requestType == typeof(GetPaymentConnectorDataRequest))
                {
                    response = this.GetPaymentConnector((GetPaymentConnectorDataRequest)request);
                }
                else if (requestType == typeof(GetCurrentTerminalIdDataRequest))
                {
                    response = this.GetCurrentTerminalId((GetCurrentTerminalIdDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            private TerminalDataManager GetDataManagerInstance(RequestContext context)
            {
                return new TerminalDataManager(context);
            }
    
            /// <summary>
            /// Gets terminal entity by record identifier or terminal identifier.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Terminal> GetTerminalByCriteria(GetTerminalDataRequest request)
            {
                TerminalDataManager terminalDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                Terminal terminal = null;
                if (string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    // Retrieves the terminal by record identifier
                    terminal = terminalDataManager.GetTerminalByRecordId(request.TerminalRecordId, request.QueryResultSettings);
                }
                else
                {
                    terminal = this.GetTerminalById(request.RequestContext, request.TerminalId, request.QueryResultSettings);
                }
    
                return new SingleEntityDataServiceResponse<Terminal>(terminal);
            }
    
            /// <summary>
            /// Gets the payment connector setting for the terminal.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<PaymentConnectorConfiguration> GetPaymentConnector(GetPaymentConnectorDataRequest request)
            {
                TerminalDataManager terminalDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                PaymentConnectorConfiguration configuration = terminalDataManager.GetPaymentConnector(request.TerminalId);
    
                return new SingleEntityDataServiceResponse<PaymentConnectorConfiguration>(configuration);
            }
    
            /// <summary>
            /// Gets the current working terminal identifier.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<string> GetCurrentTerminalId(GetCurrentTerminalIdDataRequest request)
            {
                TerminalDataManager terminalDataManager = this.GetDataManagerInstance(request.RequestContext);
    
                string terminalId = terminalDataManager.GetCurrentTerminalId();
    
                return new SingleEntityDataServiceResponse<string>(terminalId);
            }
    
            /// <summary>
            /// Gets terminal entity by identifier.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="settings">The query result settings to retrieve the terminal.</param>
            /// <returns>The terminal object.</returns>
            private Terminal GetTerminalById(RequestContext context, string terminalId, QueryResultSettings settings)
            {
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");
                ThrowIf.Null(settings, "settings");
    
                Terminal terminal;
                var query = new SqlPagedQuery(settings)
                {
                    From = TerminalDatabaseAccessor.TerminalsViewName,
                    Where = "TERMINALID = @id"
                };
    
                query.Parameters["@id"] = terminalId;
    
                using (var databaseContext = new DatabaseContext(context))
                {
                    terminal = databaseContext.ReadEntity<Terminal>(query).Results.SingleOrDefault();
                }
    
                return terminal;
            }
        }
    }
}
