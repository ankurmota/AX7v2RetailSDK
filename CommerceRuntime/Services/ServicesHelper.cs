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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Helper class for services.
        /// </summary>
        internal static class ServicesHelper
        {
            /// <summary>
            /// Validates the inbound request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <remarks>Helper method used by service operation pre-assertion.</remarks>
            public static void ValidateInboundRequest(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request == null)
                {
                    throw new ArgumentNullException("request", "request.RequestContext");
                }
    
                if (request.RequestContext.Runtime == null)
                {
                    throw new ArgumentNullException("request", "request.RequestContext.Runtime");
                }
    
                if (request.RequestContext.Runtime.Configuration == null)
                {
                    throw new ArgumentNullException("request", "request.RequestContext.Runtime.Configuration");
                }
            }
        }
    }
}
