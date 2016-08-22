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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages; 
    
        /// <summary>
        /// Handles the request for getting the loyalty cards of a customer.
        /// </summary>
        public sealed class GetCustomerLoyaltyCardsRequestHandler : SingleRequestHandler<GetCustomerLoyaltyCardsRequest, GetCustomerLoyaltyCardsResponse>
        {
            /// <summary>
            /// Executes the workflow to get the loyalty cards of a customer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetCustomerLoyaltyCardsResponse Process(GetCustomerLoyaltyCardsRequest request)
            {
                ThrowIf.Null(request, "request");
                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.CustomerAccountNumber, throwOnValidationFailure: true);
                var validateCustomerAccountResponse = this.Context.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
                if (validateCustomerAccountResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.CustomerAccountNumber = validateCustomerAccountResponse.ValidatedAccountNumber;
                }
    
                // Call data service
                var loyaltyDataServiceRequest = new GetCustomerLoyaltyCardsDataRequest(request.CustomerAccountNumber)
                {
                    QueryResultSettings = request.QueryResultSettings
                };
                var loyaltyDataServiceResponse = this.Context.Runtime.Execute<EntityDataServiceResponse<LoyaltyCard>>(loyaltyDataServiceRequest, this.Context);
    
                return new GetCustomerLoyaltyCardsResponse(loyaltyDataServiceResponse.PagedEntityCollection);
            }
        }
    }
}
