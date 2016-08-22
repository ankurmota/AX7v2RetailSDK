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
        /// The data request handler for audit log in SQLite.
        /// </summary>
        public sealed class AuditLogSqliteDataService : IRequestHandler
        {
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
            /// Processes the request.
            /// </summary>
            /// <param name="request">The request message.</param>
            /// <returns>The response message.</returns>
            public Response Execute(Request request)
            {
                Response response;
    
                ThrowIf.Null(request, "request");
    
                if (request is InsertAuditLogDataRequest)
                {
                    response = new NullResponse();
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
        }
    }
}
