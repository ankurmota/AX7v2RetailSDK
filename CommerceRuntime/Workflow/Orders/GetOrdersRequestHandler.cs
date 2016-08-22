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
        /// Handles workflow for GetSalesOrders.
        /// </summary>
        public sealed class GetOrdersRequestHandler : SingleRequestHandler<GetOrdersRequest, GetOrdersResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch the sales orders.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetOrdersRequest"/>.</param>
            /// <returns>Instance of <see cref="GetOrdersResponse"/>.</returns>
            protected override GetOrdersResponse Process(GetOrdersRequest request)
            {
                ThrowIf.Null(request, "request");

                if (request.Criteria == null || request.Criteria.IsEmpty())
                {
                    throw new ArgumentException("Must pass at least one search criteria");
                }

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.Criteria.CustomerAccountNumber, throwOnValidationFailure: false);
                var validateCustomerAccountResponse = this.Context.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
                if (validateCustomerAccountResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.Criteria.CustomerAccountNumber = validateCustomerAccountResponse.ValidatedAccountNumber;
                }

                this.ValidateSearchCriteria(request.Criteria, request.RequestContext.GetPrincipal());

                if (request.Criteria.SearchLocationType == SearchLocation.None)
                {
                    request.Criteria.SearchLocationType = SearchLocation.All;
                }
    
                // Get the orders
                var serviceRequest = new GetOrdersServiceRequest(
                    request.Criteria,
                    request.QueryResultSettings);
    
                var serviceResponse = this.Context.Execute<GetOrdersServiceResponse>(serviceRequest);
                return new GetOrdersResponse(serviceResponse.Orders);
            }

            /// <summary>
            /// Validates the search criteria.
            /// </summary>
            /// <param name="criteria">The sales order search criteria.</param>
            /// <param name="principal">The commerce principal.</param>
            private void ValidateSearchCriteria(SalesOrderSearchCriteria criteria, ICommercePrincipal principal)
            {
                // In ecommerce scenario, we do not allow customers to search the orders by staff id.
                if (principal.IsCustomer || principal.IsAnonymous)
                {
                    if (criteria.StaffId != null)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Searching orders by staff id is not allowed in ecommerce scenario.");
                    }
                }
            }
        }
    }
}
