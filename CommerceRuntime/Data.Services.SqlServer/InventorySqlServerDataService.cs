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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Inventory (picking/receiving) SQL server data service class.
        /// </summary>
        public class InventorySqlServerDataService : IRequestHandler
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
                        typeof(SavePurchaseOrderLinesDataRequest),
                        typeof(SavePickingListDataRequest),
                        typeof(SaveTransferOrderDataRequest),
                        typeof(DeletePurchaseOrderLinesDataRequest),
                        typeof(DeletePickingListLinesDataRequest),
                        typeof(DeleteTransferOrderLinesDataRequest),
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
    
                if (requestType == typeof(SavePurchaseOrderLinesDataRequest))
                {
                    response = this.SavePurchaseOrderLine((SavePurchaseOrderLinesDataRequest)request);
                }
                else if (requestType == typeof(SavePickingListDataRequest))
                {
                    response = this.SavePickingList((SavePickingListDataRequest)request);
                }
                else if (requestType == typeof(SaveTransferOrderDataRequest))
                {
                    response = this.SaveTransferOrder((SaveTransferOrderDataRequest)request);
                }
                else if (requestType == typeof(DeletePurchaseOrderLinesDataRequest))
                {
                    response = this.DeletePurchaseOrder((DeletePurchaseOrderLinesDataRequest)request);
                }
                else if (requestType == typeof(DeletePickingListLinesDataRequest))
                {
                    response = this.DeletePickingListLines((DeletePickingListLinesDataRequest)request);
                }
                else if (requestType == typeof(DeleteTransferOrderLinesDataRequest))
                {
                    response = this.DeleteTransferOrderLines((DeleteTransferOrderLinesDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Saves the purchase order lines.
            /// </summary>
            /// <param name="savePurchaseOrderLinesDataRequest">The <see cref="SavePurchaseOrderLinesDataRequest" /> request.</param>
            /// <returns><see cref="NullResponse"/></returns>
            private NullResponse SavePurchaseOrderLine(SavePurchaseOrderLinesDataRequest savePurchaseOrderLinesDataRequest)
            {
                this.GetDataManagerInstance(savePurchaseOrderLinesDataRequest.RequestContext).SavePurchaseOrderLines(savePurchaseOrderLinesDataRequest.PurchaseOrder);
                return new NullResponse();
            }
    
            /// <summary>
            /// Deletes the purchase order lines.
            /// </summary>
            /// <param name="request">The <see cref="DeletePurchaseOrderLinesDataRequest" /> request.</param>
            /// <returns><see cref="NullResponse"/></returns>
            private NullResponse DeletePurchaseOrder(DeletePurchaseOrderLinesDataRequest request)
            {
                this.GetDataManagerInstance(request.RequestContext).DeletePurchaseOrderLines(request.OrderId);
                return new NullResponse();
            }
    
            /// <summary>
            /// Deletes the picking list lines.
            /// </summary>
            /// <param name="request">The <see cref="DeletePickingListLinesDataRequest" /> request.</param>
            /// <returns><see cref="NullResponse"/></returns>
            private NullResponse DeletePickingListLines(DeletePickingListLinesDataRequest request)
            {
                this.GetDataManagerInstance(request.RequestContext).DeletePickingListLines(request.OrderId);
                return new NullResponse();
            }
    
            /// <summary>
            /// Deletes the transfer order lines.
            /// </summary>
            /// <param name="request">The <see cref="DeleteTransferOrderLinesDataRequest" /> request.</param>
            /// <returns><see cref="NullResponse"/></returns>
            private NullResponse DeleteTransferOrderLines(DeleteTransferOrderLinesDataRequest request)
            {
                this.GetDataManagerInstance(request.RequestContext).DeleteTransferOrderLines(request.OrderId);
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets Inventory Data Manager.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>Inventory data manager.</returns>
            private InventoryDataManager GetDataManagerInstance(RequestContext context)
            {
                return new InventoryDataManager(context);
            }
    
            /// <summary>
            /// Saves the picking list lines.
            /// </summary>
            /// <param name="request">The request with picking list.</param>
            /// <returns>Empty response.</returns>
            private NullResponse SavePickingList(SavePickingListDataRequest request)
            {
                this.GetDataManagerInstance(request.RequestContext).SavePickingListLines(request.PickingList);
                return new NullResponse();
            }
    
            /// <summary>
            /// Saves the transfer order.
            /// </summary>
            /// <param name="request">The request with transfer order.</param>
            /// <returns>Empty response.</returns>
            private NullResponse SaveTransferOrder(SaveTransferOrderDataRequest request)
            {
                this.GetDataManagerInstance(request.RequestContext).SaveTransferOrderLines(request.TransferOrder);
                return new NullResponse();
            }
        }
    }
}
