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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Handles workflow for GetDeliveryOptions.
        /// </summary>
        public sealed class GetDeliveryOptionsRequestHandler : SingleRequestHandler<GetDeliveryOptionsRequest, GetDeliveryOptionsResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch line level delivery options for given cart.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetDeliveryOptionsRequest"/>.</param>
            /// <returns>Instance of <see cref="GetDeliveryOptionsResponse"/>.</returns>
            protected override GetDeliveryOptionsResponse Process(GetDeliveryOptionsRequest request)
            {
                ThrowIf.Null(request, "request");

                ValidateRequest(request);

                // Get the Sales Transaction
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                if (salesTransaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.CartId);
                }

                GetDeliveryOptionsRequestHandler.ResetPreviousShippingAddressOnTransaction(salesTransaction);

                Collection<SalesLine> requestedSalesLines = null; // Used if line level delivery options were requested.
                if (!request.FetchDeliveryOptionsForLines)
                {
                    salesTransaction.ShippingAddress = request.HeaderShippingAddress;
                }
                else
                {
                    requestedSalesLines = new Collection<SalesLine>();
                    Dictionary<string, LineShippingAddress> shippingAddressByLineId = request.LineShippingAddresses.ToDictionary(lsa => lsa.LineId);

                    foreach (var salesLine in salesTransaction.ActiveSalesLines)
                    {
                        LineShippingAddress lineShippingAddress;
                        if (shippingAddressByLineId.TryGetValue(salesLine.LineId, out lineShippingAddress))
                        {
                            salesLine.ShippingAddress = lineShippingAddress.ShippingAddress;
                            requestedSalesLines.Add(salesLine);
                        }
                    }
                }

                // Validate and resolve addresses.
                ShippingHelper.ValidateAndResolveAddresses(this.Context, salesTransaction);

                // Get the delivery options.
                GetDeliveryOptionsResponse response;
                if (!request.FetchDeliveryOptionsForLines)
                {
                    // Get the delivery options that are common to all the cart lines.
                    var serviceRequest = new GetOrderDeliveryOptionsServiceRequest(salesTransaction);
                    serviceRequest.QueryResultSettings = request.QueryResultSettings;
                    var serviceResponse = this.Context.Execute<GetOrderDeliveryOptionsServiceResponse>(serviceRequest);

                    response = new GetDeliveryOptionsResponse(serviceResponse.DeliveryOptions);
                }
                else
                {
                    // Get the delivery options for each line.
                    var serviceRequest = new GetLineDeliveryOptionsServiceRequest(requestedSalesLines);
                    serviceRequest.QueryResultSettings = request.QueryResultSettings;
                    var serviceResponse = this.Context.Execute<GetLineDeliveryOptionsServiceResponse>(serviceRequest);

                    response = new GetDeliveryOptionsResponse(serviceResponse.LineDeliveryOptions);
                }

                return response;
            }

            private static void ResetPreviousShippingAddressOnTransaction(SalesTransaction salesTransaction)
            {
                salesTransaction.ShippingAddress = null;
                foreach (var salesLine in salesTransaction.ActiveSalesLines)
                {
                    salesLine.ShippingAddress = null;
                }
            }

            /// <summary>
            /// Validates the request to get delivery options.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <exception cref="DataValidationException">
            /// The header level shipping address cannot be null if fetching delivery options for the entire order
            /// or
            /// Both the cart line identifier and the corresponding non-null shipping address must be specified when fetching delivery options per line.
            /// </exception>
            private static void ValidateRequest(GetDeliveryOptionsRequest request)
            {
                ThrowIf.Null(request.CartId, "request.CartId");

                if (!request.FetchDeliveryOptionsForLines)
                {
                    if (request.HeaderShippingAddress == null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingAddress, "The header level shipping address cannot be null if fetching delivery options for the entire order");
                    }
                }
                else
                {
                    if (request.LineShippingAddresses == null
                        || request.LineShippingAddresses.Where(lsa => string.IsNullOrEmpty(lsa.LineId)).Any()
                        || request.LineShippingAddresses.Where(lsa => lsa.ShippingAddress == null).Any())
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "Both the cart line identifier and non-null shipping address must be specified when fetching delivery options per line.");
                    }
                }
            }
        }
    }
}
