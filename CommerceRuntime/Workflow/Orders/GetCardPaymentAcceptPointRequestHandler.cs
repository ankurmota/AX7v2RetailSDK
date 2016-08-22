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
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handles workflow for getting the accepting point of card payment.
        /// </summary>
        public sealed class GetCardPaymentAcceptPointRequestHandler : SingleRequestHandler<GetCardPaymentAcceptPointRequest, GetCardPaymentAcceptPointResponse>
        {
            /// <summary>
            /// Executes the workflow to get the accepting point of card payment.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetCardPaymentAcceptPointRequest"/>.</param>
            /// <returns>Instance of <see cref="GetCardPaymentAcceptPointResponse"/>.</returns>
            protected override GetCardPaymentAcceptPointResponse Process(GetCardPaymentAcceptPointRequest request)
            {
                ThrowIf.Null(request, "request");
    
                if (request.CartId == null)
                {
                    throw new ArgumentException("request.CartId cannot be null.");
                }
    
                // Find the first shipping address from the cart which is not store pickup
                // Look at the line level, if not found, then the header level.
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                Address defaultAddress = null;
                ChannelConfiguration channelConfiguration = this.Context.GetChannelConfiguration();
    
                foreach (var salesline in transaction.SalesLines)
                {
                    if (!string.IsNullOrWhiteSpace(salesline.DeliveryMode)
                        && !salesline.DeliveryMode.Equals(channelConfiguration.PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase)
                        && salesline.ShippingAddress != null)
                    {
                        defaultAddress = salesline.ShippingAddress;
                        break;
                    }
                }
    
                if (defaultAddress == null
                    && !string.IsNullOrWhiteSpace(transaction.DeliveryMode)
                    && !transaction.DeliveryMode.Equals(channelConfiguration.PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase)
                    && transaction.ShippingAddress != null)
                {
                    defaultAddress = transaction.ShippingAddress;
                }
    
                // Call service to get accept point
                var serviceRequest = new GetCardPaymentAcceptPointServiceRequest(request.CardPaymentAcceptSettings, defaultAddress, defaultAddress != null);
                var serviceResponse = this.Context.Execute<GetCardPaymentAcceptPointServiceResponse>(serviceRequest);
                return new GetCardPaymentAcceptPointResponse(serviceResponse.CardPaymentAcceptPoint);
            }
        }
    }
}
