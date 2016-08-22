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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handles the request for issuing a loyalty card.
        /// </summary>
        public sealed class IssueLoyaltyCardRequestHandler : SingleRequestHandler<IssueLoyaltyCardRequest, IssueLoyaltyCardResponse>
        {
            /// <summary>
            /// Executes the workflow to issue a loyalty card.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override IssueLoyaltyCardResponse Process(IssueLoyaltyCardRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.CustomerAccountNumber, throwOnValidationFailure: true);
                request.RequestContext.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
    
                // If the request contains a customer account number, find the party recid of the customer.
                long partyRecordId = 0;
                if (!string.IsNullOrWhiteSpace(request.CustomerAccountNumber))
                {
                    var getCustomerServiceRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, request.CustomerAccountNumber);
                    var getCustomerServiceResponse = this.Context.Execute<GetCustomersServiceResponse>(getCustomerServiceRequest);
                    var customer = getCustomerServiceResponse.Customers.SingleOrDefault();
    
                    if (customer == null || customer.DirectoryPartyRecordId == 0)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerOrDirectoryPartyNotFound);
                    }
    
                    partyRecordId = customer.DirectoryPartyRecordId;
                }
    
                var issueLoyaltyCardRequest = new IssueLoyaltyCardServiceRequest(
                    request.LoyaltyCardNumber, 
                    request.CardTenderType,
                    request.CustomerAccountNumber,
                    partyRecordId,
                    this.Context.GetPrincipal().ChannelId);
    
                // Issue the loyalty card in HQ
                var serviceResponse = this.Context.Execute<IssueLoyaltyCardServiceResponse>(issueLoyaltyCardRequest);
                return new IssueLoyaltyCardResponse(serviceResponse.LoyaltyCard);
            }
        }
    }
}
