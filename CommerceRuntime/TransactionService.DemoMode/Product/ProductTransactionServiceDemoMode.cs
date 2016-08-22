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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// The product demo mode transaction service implementation.
        /// </summary>
        public class ProductTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetProductDataRealtimeRequest),
                        typeof(GetRemoteProductsByCategoryRealtimeRequest),
                        typeof(GetRemoteProductsByKeywordRealtimeRequest),
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
                if (requestType == typeof(GetProductDataRealtimeRequest))
                {
                    response = GetProductData();
                }
                else if (requestType == typeof(GetRemoteProductsByCategoryRealtimeRequest))
                {
                    response = GetRemoteProductsByCategory();
                }
                else if (requestType == typeof(GetRemoteProductsByKeywordRealtimeRequest))
                {
                    response = GetRemoteProductsByKeyword();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static ProductSearchRealtimeResponse GetRemoteProductsByKeyword()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetRemoteProductsByKeyword is not supported in demo mode.");
            }
    
            private static ProductSearchRealtimeResponse GetRemoteProductsByCategory()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetRemoteProductsByCategory is not supported in demo mode.");
            }
    
            private static GetProductDataRealtimeResponse GetProductData()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetProductData is not supported in demo mode.");
            }
        }
    }
}
