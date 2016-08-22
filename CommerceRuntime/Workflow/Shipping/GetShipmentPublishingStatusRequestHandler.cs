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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handler for reading shipments.
        /// </summary>
        public sealed class GetShipmentPublishingStatusRequestHandler : SingleRequestHandler<GetShipmentPublishingStatusRequest, GetShipmentPublishingStatusResponse>
        {
            /// <summary>
            /// Executes the workflow to read shipment publishing status.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetShipmentPublishingStatusResponse Process(GetShipmentPublishingStatusRequest request)
            {
                ThrowIf.Null(request, "request");
    
                ////var shippingDataManager = new ShippingDataManager(this.Context);
                ////var shipmentStatusCollection = shippingDataManager.GetShipmentsStatus(request.QueryResultSettings);
                var shipmentStatusCollection = new System.Collections.Generic.List<Microsoft.Dynamics.Commerce.Runtime.DataModel.ShipmentPublishingStatus>(0).AsPagedResult();
    
                return new GetShipmentPublishingStatusResponse(shipmentStatusCollection);
            }
        }
    }
}
