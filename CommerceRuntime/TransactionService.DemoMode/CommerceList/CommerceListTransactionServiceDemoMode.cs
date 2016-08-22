/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

namespace Contoso
{
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Represents an implementation of the commerce list service.
        /// </summary>
        public sealed class CommerceListTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetCommerceListRealtimeRequest),
                        typeof(SaveCommerceListRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetCommerceListRealtimeRequest))
                {
                    response = GetCommerceLists();
                }
                else if (requestType == typeof(SaveCommerceListRealtimeRequest))
                {
                    response = SaveCommerceList();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Invokes the transaction service to get commerce lists from AX.
            /// </summary>
            /// <returns>Returns the GetCommerceListServiceResponse that contains the result.</returns>
            private static GetCommerceListRealtimeResponse GetCommerceLists()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetCommerceLists is not supported in demo mode.");
            }
    
            /// <summary>
            /// Invokes the transaction service to save a commerce list on AX.
            /// </summary>
            /// <returns>Returns the SaveCommerceListServiceResponse that contains the result.</returns>
            private static GetCommerceListRealtimeResponse SaveCommerceList()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SaveCommerceList is not supported in demo mode.");
            }
        }
    }
}
