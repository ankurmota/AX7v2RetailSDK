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
        using DataServices.ReasonCodes;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Reason code data request handler for SQLite.
        /// </summary>
        public sealed class ReasonCodeSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetReasonCodesDataRequest) };
                }
            }
    
            /// <summary>
            /// Gets the sales transaction to be saved.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
    
                if (request is GetReasonCodesDataRequest)
                {
                    response = GetReasonCodes((GetReasonCodesDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<ReasonCode> GetReasonCodes(GetReasonCodesDataRequest request)
            {
                string defaultLanguageId = GetDefaultLanguageId(request.RequestContext);
                string employeeLanguageId = GetEmployeeLanguageId(request.RequestContext);
    
                GetReasonCodesProcedure getReasonCodesProcedure = new GetReasonCodesProcedure(request, defaultLanguageId, employeeLanguageId);
                PagedResult<ReasonCode> reasonCodes = getReasonCodesProcedure.Execute();
    
                return new EntityDataServiceResponse<ReasonCode>(reasonCodes);
            }
    
            /// <summary>
            /// Gets the default language id for the channel.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Language identifier.</returns>
            private static string GetDefaultLanguageId(RequestContext context)
            {
                var getDefaultLanguageIdDataRequest = new GetDefaultLanguageIdDataRequest();
                string defaultLanguageId = context.Runtime.Execute<SingleEntityDataServiceResponse<string>>(getDefaultLanguageIdDataRequest, context).Entity;
    
                return defaultLanguageId;
            }
    
            /// <summary>
            /// Gets the language identifier for current employee.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Language identifier.</returns>
            private static string GetEmployeeLanguageId(RequestContext context)
            {
                GetEmployeeDataRequest dataRequest = new GetEmployeeDataRequest(context.GetPrincipal().UserId, QueryResultSettings.SingleRecord);
                var currentEmployee = context.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                return currentEmployee.CultureName;
            }
        }
    }
}
