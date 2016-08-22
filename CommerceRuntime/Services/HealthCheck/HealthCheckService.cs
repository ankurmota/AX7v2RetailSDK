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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Customer Service class.
        /// </summary>
        public class HealthCheckService : IRequestHandler
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
                        typeof(RunHealthCheckServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Executes the service request.
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
    
                Type requestedType = request.GetType();
    
                if (requestedType == typeof(RunHealthCheckServiceRequest))
                {
                    switch (((RunHealthCheckServiceRequest)request).HealthCheckType)
                    {
                        case HealthCheckType.DatabaseHealthCheck:
                            return this.RunDatabaseHealthCheck((RunHealthCheckServiceRequest)request);
    
                        case HealthCheckType.RealtimeServiceHealthCheck:
                            return this.RunRealtimeServiceHealthCheck((RunHealthCheckServiceRequest)request);
    
                        default:
                            throw new NotSupportedException(string.Format("Health check type '{0}' is not supported.", ((RunHealthCheckServiceRequest)request).HealthCheckType));
                    }
                }
    
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
    
            /// <summary>
            /// Performs realtime service health check.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Service response.</returns>
            private Response RunRealtimeServiceHealthCheck(RunHealthCheckServiceRequest request)
            {
                request.RequestContext.Execute<RunHealthCheckRealtimeResponse>(new RunHealthCheckRealtimeRequest());
                return new RunHealthCheckServiceResponse();
            }
    
            /// <summary>
            /// Performs database health check.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Service response.</returns>
            private Response RunDatabaseHealthCheck(RunHealthCheckServiceRequest request)
            {
                var context = request.RequestContext;
                var response = context.Runtime.Execute<SingleEntityDataServiceResponse<bool>>(new RunDBHealthCheckDataRequest(), context);
    
                return new RunHealthCheckServiceResponse(response.Entity);
            }
        }
    }
}
