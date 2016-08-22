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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Gets the shopping cart specified by cart id and optionally calculates the totals on the cart.
        /// </summary>
        /// <remarks>Upon calculating the totals, the cart is saved to the database.</remarks>
        public sealed class GetCartRequestHandler : SingleRequestHandler<GetCartRequest, GetCartResponse>
        {
            /// <summary>
            /// Gets the shopping cart specified by cart identifier and optionally calculates the totals on the cart.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="GetCartResponse"/> object containing the shopping cart or a new one if the flag to create is set and no cart was found.</returns>
            protected override GetCartResponse Process(GetCartRequest request)
            {
                ThrowIf.Null(request, "request");

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.SearchCriteria.CustomerAccountNumber, throwOnValidationFailure: false);
                var validateCustomerAccountResponse = this.Context.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
                if (validateCustomerAccountResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.SearchCriteria.CustomerAccountNumber = validateCustomerAccountResponse.ValidatedAccountNumber;
                    request.SearchCriteria.IncludeAnonymous = true;
                }

                if (!request.SearchCriteria.SuspendedOnly && string.IsNullOrEmpty(request.SearchCriteria.CartId) && string.IsNullOrEmpty(request.SearchCriteria.CustomerAccountNumber))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "SearchCriteria requires one of the following fields to be set: SuspendedOnly, CartId, CustomerAccountNumber.");
                }

                if (!string.IsNullOrEmpty(request.SearchCriteria.CustomerAccountNumber) && string.IsNullOrWhiteSpace(request.SearchCriteria.CartId) && !request.SearchCriteria.SuspendedOnly)
                {
                    // If the only search criteria set is that of customer account number, then anonymous carts should not be fetched.
                    request.SearchCriteria.IncludeAnonymous = false;
                }

                // User query result settings provided by caller. If not available and cart identifier is set retrieve one row, otherwise do not apply paging.
                QueryResultSettings queryResultSettings = request.QueryResultSettings ?? (string.IsNullOrEmpty(request.SearchCriteria.CartId) ? QueryResultSettings.AllRecords : QueryResultSettings.SingleRecord);
                bool removeUnassortedProducts = !request.SearchCriteria.SuspendedOnly;

                var getCartsServiceRequest = new GetSalesTransactionsServiceRequest(request.SearchCriteria, queryResultSettings, removeUnassortedProducts);
                var getCartsServiceResponse = this.Context.Execute<GetSalesTransactionsServiceResponse>(getCartsServiceRequest);

                PagedResult<SalesTransaction> salesTransactions = getCartsServiceResponse.SalesTransactions;
                IDictionary<string, IList<SalesLine>> linesWithUnavailableProducts = getCartsServiceResponse.LinesWithUnavailableProducts;

                IEnumerable<SalesTransaction> transactionWithUnassortedProducts = salesTransactions.Results.Where(t => linesWithUnavailableProducts.Keys.Contains(t.Id));

                if (removeUnassortedProducts && linesWithUnavailableProducts.Any())
                {
                    foreach (SalesTransaction transaction in transactionWithUnassortedProducts)
                    {
                        // Recalculate totals (w/o unassorted products and save cart).
                        CartWorkflowHelper.Calculate(this.Context, transaction, requestedMode: null);
                        CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);
                    }

                    if (!request.IgnoreProductDiscontinuedNotification)
                    {
                        // Send notification to decide if caller should be notified about discontinued products.
                        var notification = new ProductDiscontinuedFromChannelNotification(linesWithUnavailableProducts);
                        this.Context.Notify(notification);
                    }

                    // Reload to cart to avoid version mismatch.
                    var reloadCartsServiceRequest = new GetSalesTransactionsServiceRequest(request.SearchCriteria, queryResultSettings, mustRemoveUnavailableProductLines: false);
                    salesTransactions = this.Context.Execute<GetSalesTransactionsServiceResponse>(reloadCartsServiceRequest).SalesTransactions;
                }

                PagedResult<Cart> carts = salesTransactions.ConvertTo(transaction => ConvertTransactionToCart(this.Context, transaction, request.IncludeHistoricalTenderLines));
                return new GetCartResponse(carts, salesTransactions);
            }

            private static Cart ConvertTransactionToCart(RequestContext context, SalesTransaction transaction, bool includeHistoricalTenderLines)
            {
                Cart cart = CartWorkflowHelper.ConvertToCart(context, transaction);

                if (cart.TenderLines.Any(t => t.IsHistorical))
                {
                    if (includeHistoricalTenderLines)
                    {
                        // Check access rights for tender line historical lines.
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.PaymentsHistory));
                    }
                    else
                    {
                        CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
                    }
                }

                return cart;
            }
        }
    }
}