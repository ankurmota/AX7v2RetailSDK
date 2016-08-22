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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handles workflow for GetProductDeliveryOptions.
        /// </summary>
        public sealed class GetProductDeliveryOptionsRequestHandler : 
            SingleRequestHandler<GetProductDeliveryOptionsRequest, GetProductDeliveryOptionsResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch delivery options for given product and address.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetProductDeliveryOptionsRequest"/>.</param>
            /// <returns>Instance of <see cref="GetProductDeliveryOptionsResponse"/>.</returns>
            protected override GetProductDeliveryOptionsResponse Process(GetProductDeliveryOptionsRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ShippingAddress, "request.ShippingAddress");
    
                // Validate and resolve address
                ShippingHelper.ValidateAndResolveAddress(this.Context, request.ShippingAddress);
    
                // Get the delivery options
                var serviceRequest = new GetProductDeliveryOptionsServiceRequest(request.ShippingAddress, request.ItemId, request.InventoryDimensionId)
                {
                    QueryResultSettings = request.QueryResultSettings
                };
                var serviceResponse = this.Context.Execute<GetProductDeliveryOptionsServiceResponse>(serviceRequest);
    
                return new GetProductDeliveryOptionsResponse(serviceResponse.DeliveryOptions);
            }
        }
    }
}
