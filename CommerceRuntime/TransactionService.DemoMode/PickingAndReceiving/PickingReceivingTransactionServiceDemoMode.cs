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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// The picking and receiving demo mode transaction service.
        /// </summary>
        public class PickingReceivingTransactionServiceDemoMode : IRequestHandler
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
                        typeof(GetPurchaseOrderRealtimeRequest),
                        typeof(GetTransferOrderRealtimeRequest),
                        typeof(SavePurchaseOrderRealtimeRequest),
                        typeof(SaveTransferOrderRealtimeRequest),
                        typeof(GetPickingListRealtimeRequest),
                        typeof(SavePickingListRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The service response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestedType = request.GetType();
    
                if (requestedType == typeof(GetPurchaseOrderRealtimeRequest))
                {
                    return GetPurchaseOrders();
                }
    
                if (requestedType == typeof(GetTransferOrderRealtimeRequest))
                {
                    return GetTransferOrders();
                }
    
                if (requestedType == typeof(SavePurchaseOrderRealtimeRequest))
                {
                    return SavePurchaseOrder();
                }
    
                if (requestedType == typeof(SaveTransferOrderRealtimeRequest))
                {
                    return SaveTransferOrder();
                }
    
                if (requestedType == typeof(GetPickingListRealtimeRequest))
                {
                    return GetPickingLists();
                }
    
                if (requestedType == typeof(SavePickingListRealtimeRequest))
                {
                    return SavePickingList();
                }
    
                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }
    
            /// <summary>
            /// Saves a purchase order in either local database or Ax.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SavePurchaseOrder()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SavePurchaseOrder is not supported in demo mode.");
            }
    
            /// <summary>
            /// Saves a transfer order in either local database or Ax.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SaveTransferOrder()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SaveTransferOrder is not supported in demo mode.");
            }
    
            /// <summary>
            /// Saves a picking list in either local database or Ax.
            /// </summary>
            /// <returns>The service response.</returns>
            private static SavePurchaseTransferOrderRealtimeResponse SavePickingList()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "SavePickingList is not supported in demo mode.");
            }
    
            /// <summary>
            /// Get all open picking lists.
            /// </summary>
            /// <returns>The service response.</returns>
            private static Response GetPickingLists()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetPickingLists is not supported in demo mode.");
            }
    
            /// <summary>
            /// Get all open purchase and/or transfer orders.
            /// </summary>
            /// <returns>The service response.</returns>
            private static Response GetPurchaseOrders()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetPurchaseOrders is not supported in demo mode.");
            }
    
            /// <summary>
            /// Get all open transfer orders.
            /// </summary>
            /// <returns>The service response.</returns>
            private static Response GetTransferOrders()
            {
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "GetTransferOrders is not supported in demo mode.");
            }
        }
    }
}
