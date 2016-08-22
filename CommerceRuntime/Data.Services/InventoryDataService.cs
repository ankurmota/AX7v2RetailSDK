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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Inventory data services that contains methods to retrieve the inventory information from underlying data storage.
        /// </summary>
        public class InventoryDataService : IRequestHandler
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
                        typeof(GetPurchaseOrderDataRequest),
                        typeof(GetPickingListDataRequest),
                        typeof(GetTransferOrderDataRequest),
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
    
                Type requestedType = request.GetType();
                Response response;
    
                if (requestedType == typeof(GetPurchaseOrderDataRequest))
                {
                    response = GetPurchaseOrder((GetPurchaseOrderDataRequest)request);
                }
                else if (requestedType == typeof(GetPickingListDataRequest))
                {
                    response = GetPickingList((GetPickingListDataRequest)request);
                }
                else if (requestedType == typeof(GetTransferOrderDataRequest))
                {
                    response = GetTransferOrder((GetTransferOrderDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the inventory data manager instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of <see cref="CustomerDataManager"/></returns>
            private static InventoryDataManager GetDataManagerInstance(RequestContext context)
            {
                return new InventoryDataManager(context);
            }
    
            /// <summary>
            /// Gets all purchase order lines that meet the supplied criteria.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<PurchaseOrderLine> GetPurchaseOrder(GetPurchaseOrderDataRequest request)
            {
                InventoryDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                PagedResult<PurchaseOrderLine> orders = dataManager.GetPurchaseOrder(request.OrderId, request.QueryResultSettings);
    
                return new EntityDataServiceResponse<PurchaseOrderLine>(orders);
            }
    
            /// <summary>
            /// Gets all picking list lines that meet the supplied criteria.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<PickingListLine> GetPickingList(GetPickingListDataRequest request)
            {
                InventoryDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                PagedResult<PickingListLine> lines = dataManager.GetPickingList(request.OrderId, request.QueryResultSettings);
    
                return new EntityDataServiceResponse<PickingListLine>(lines);
            }
    
            /// <summary>
            /// Gets all transfer order lines that meet the supplied criteria.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<TransferOrderLine> GetTransferOrder(GetTransferOrderDataRequest request)
            {
                InventoryDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                PagedResult<TransferOrderLine> lines = dataManager.GetTransferOrder(request.OrderId, request.QueryResultSettings);
    
                return new EntityDataServiceResponse<TransferOrderLine>(lines);
            }
        }
    }
}
