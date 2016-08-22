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
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handles the request for getting the loyalty card transactions.
        /// </summary>
        public sealed class GetLoyaltyCardTransactionsRequestHandler : SingleRequestHandler<GetLoyaltyCardTransactionsRequest, GetLoyaltyCardTransactionsResponse>
        {
            /// <summary>
            /// Executes the workflow to get the loyalty card transactions.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetLoyaltyCardTransactionsResponse Process(GetLoyaltyCardTransactionsRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.LoyaltyCardNumber, "request.LoyaltyCardNumber");
                ThrowIf.NullOrWhiteSpace(request.RewardPointId, "request.RewardPointId");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
                ThrowIf.Null(request.QueryResultSettings.Paging, "request.QueryResultSettings.Paging");
    
                var getLoyaltyCardStatusRequest = new GetLoyaltyCardStatusServiceRequest(request.LoyaltyCardNumber);
                GetLoyaltyCardStatusServiceResponse getLoyaltyCardStatusResponse = this.Context.Execute<GetLoyaltyCardStatusServiceResponse>(getLoyaltyCardStatusRequest);
    
                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(getLoyaltyCardStatusResponse.LoyaltyCard.CustomerAccount, throwOnValidationFailure: true);
                request.RequestContext.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
    
                var realtimeRequest = new GetLoyaltyCardTransactionsRealtimeRequest(
                    request.LoyaltyCardNumber,
                    request.RewardPointId,
                    request.QueryResultSettings);
    
                GetLoyaltyCardTransactionsRealtimeResponse realtimeResponse = request.RequestContext.Execute<GetLoyaltyCardTransactionsRealtimeResponse>(realtimeRequest);
    
                return new GetLoyaltyCardTransactionsResponse(realtimeResponse.LoyaltyCardTransactions);
            }
        }
    }
}
