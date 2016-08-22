/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Workflow for picking list and packing slip creation.
        /// </summary>
        public sealed class PickAndPackOrderRequestHandler : SingleRequestHandler<PickAndPackOrderRequest, NullResponse>
        {
            /// <summary>
            /// Process the request.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>The response object.</returns>
            protected override NullResponse Process(PickAndPackOrderRequest request)
            {
                ThrowIf.Null(request, "request");
    
                if (string.IsNullOrEmpty(request.SalesId))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Order identifier is required.");
                }
    
                // Get current channel
                OrgUnit currentStore = PickAndPackOrderRequestHandler.GetStoreFromContext(this.Context);
    
                string shippingWarehouseId = string.IsNullOrWhiteSpace(currentStore.ShippingInventLocationId)
                    ? currentStore.InventoryLocationId
                    : currentStore.ShippingInventLocationId;
    
                // Execute request
                this.Context.Execute<Response>(
                    new PickAndPackOrderRealtimeRequest(
                        request.SalesId,
                        request.CreatePickingList,
                        request.CreatePackingSlip,
                        shippingWarehouseId));
    
                // Retrive customer order
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets the store by identifier.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The store.</returns>
            private static OrgUnit GetStoreFromContext(RequestContext context)
            {
                SearchOrgUnitDataRequest request = new SearchOrgUnitDataRequest(context.GetPrincipal().ChannelId);
                return context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(request, context).PagedEntityCollection.SingleOrDefault();
            }
        }
    }
}
