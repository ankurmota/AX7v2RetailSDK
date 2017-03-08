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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Saves a shopping cart.
        /// </summary>
        public sealed class SaveCartRequestHandler : SingleRequestHandler<SaveCartRequest, SaveCartResponse>
        {
            /// <summary>
            /// Saves (updating if it exists and creating a new one if it does not) the shopping cart on the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="SaveCartResponse"/> object containing the cart with updated item quantities.</returns>
            protected override SaveCartResponse Process(SaveCartRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Cart, "request.Cart");

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.Cart.CustomerId, throwOnValidationFailure: false);
                var validateCustomerAccountResponse = this.Context.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);
                if (validateCustomerAccountResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.Cart.CustomerId = validateCustomerAccountResponse.ValidatedAccountNumber;
                }

                bool isItemSale = request.Cart.CartLines.Any(l => string.IsNullOrWhiteSpace(l.LineId) && !l.IsGiftCardLine && !l.IsVoided && l.Quantity >= 0m);

                if (string.IsNullOrWhiteSpace(request.Cart.Id))
                {
                    request.Cart.Id = CartWorkflowHelper.GenerateRandomTransactionId(this.Context);
                }

                // Copy the logic from CartService.CreateCart().
                foreach (CartLine line in request.Cart.CartLines)
                {
                    // Sets the IsReturn flag to true, when ReturnTransactionId is specified.
                    // The reason of doing so is that the IsReturn is not currently exposed on CartLine entity.
                    if (!string.IsNullOrEmpty(line.ReturnTransactionId))
                    {
                        //DEMO4
                        //line.LineData.IsReturnByReceipt = true;  //TODO:AM Uncomment for real implementation
                    }
                }

                // Get the Sales Transaction
                SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.Cart.Id);

                if (salesTransaction == null)
                {
                    // New transaction - set the default cart type to shopping if none
                    if (request.Cart.CartType == CartType.None)
                    {
                        request.Cart.CartType = CartType.Shopping;
                    }
                }

                CartWorkflowHelper.ValidateCartPermissions(salesTransaction, request.Cart, this.Context);

                if (salesTransaction == null)
                {
                    // New transaction - set the default cart type to shopping if none
                    if (request.Cart.CartType == CartType.None)
                    {
                        request.Cart.CartType = CartType.Shopping;
                    }

                    // Do not allow new transaction for blocked customer.
                    CartWorkflowHelper.ValidateCustomerAccount(this.Context, request.Cart, null);

                    // Set loyalty card from the customer number
                    CartWorkflowHelper.SetLoyaltyCardFromCustomer(this.Context, request.Cart);

                    // Set affiliations from the customer number
                    CartWorkflowHelper.AddOrUpdateAffiliationLinesFromCustomer(this.Context, null, request.Cart);

                    // If cannot find the transaction, create a new transaction.
                    salesTransaction = CartWorkflowHelper.CreateSalesTransaction(this.Context, request.Cart.Id, request.Cart.CustomerId);

                    // Set initial values on cart to be same as on transaction.
                    request.Cart.CopyPropertiesFrom(salesTransaction);

                    // Update transaction level reason code lines.
                    ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnTransaction(salesTransaction, request.Cart);

                    // Calculate required reason code lines for start of transaction.
                    ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnTransaction(this.Context, salesTransaction, ReasonCodeSourceType.StartOfTransaction);
                }

                // If cart or the sales transaction is suspended then update is not permitted
                if (salesTransaction.IsSuspended)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotActive, request.Cart.Id);
                }

                // If the terminal id of the cart is not same as the context then it means that the cart is active on another terminal.
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                if (!(salesTransaction.TerminalId ?? string.Empty).Equals(this.Context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_LoadingActiveCartFromAnotherTerminalNotAllowed, request.Cart.Id);
                }

                // At this point, the sales transaction is either newly created with no sales lines or just loaded from DB.
                // We are yet to add new sales lines or update existing sales lines.
                // Get the returned sales transaction if the cart contains a return line
                SalesTransaction returnTransaction = CartWorkflowHelper.LoadSalesTransactionForReturn(this.Context, request.Cart, salesTransaction, request.OperationType);

                // If customer account number is not specified on the request it should not be overriden.
                if (request.Cart.CustomerId == null)
                {
                    request.Cart.CustomerId = salesTransaction.CustomerId;
                }

                // Get the products in the cart lines
                IDictionary<long, SimpleProduct> productsByRecordId = CartWorkflowHelper.GetProductsInCartLines(this.Context, request.Cart.CartLines);

                // Validate update cart request
                CartWorkflowHelper.ValidateUpdateCartRequest(this.Context, salesTransaction, returnTransaction, request.Cart, request.IsGiftCardOperation, productsByRecordId);

                request.Cart.IsReturnByReceipt = returnTransaction != null;
                request.Cart.ReturnTransactionHasLoyaltyPayment = returnTransaction != null && returnTransaction.HasLoyaltyPayment;

                if (returnTransaction != null
                    && !string.IsNullOrWhiteSpace(returnTransaction.LoyaltyCardId)
                    && string.IsNullOrWhiteSpace(salesTransaction.LoyaltyCardId))
                {
                    // Set the loyalty card of the returned transaction to the current transaction
                    request.Cart.LoyaltyCardId = returnTransaction.LoyaltyCardId;
                }

                HashSet<string> newSalesLineIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Perform update cart operations
                CartWorkflowHelper.PerformSaveCartOperations(this.Context, request, salesTransaction, returnTransaction, newSalesLineIdSet, productsByRecordId);

                // Sets the wharehouse id and invent location id for each line
                ItemAvailabilityHelper.SetSalesLineInventory(this.Context, salesTransaction);

                // Calculate totals and saves the sales transaction
                CartWorkflowHelper.Calculate(this.Context, salesTransaction, request.CalculationModes, isItemSale, newSalesLineIdSet);

                // Validate price on sales line after calculations
                CartWorkflowHelper.ValidateSalesLinePrice(this.Context, salesTransaction, productsByRecordId);

                // Validate the customer account deposit transaction.
                AccountDepositHelper.ValidateCustomerAccountDepositTransaction(this.Context, salesTransaction);

                // Validate return item and return transaction permissions
                CartWorkflowHelper.ValidateReturnPermission(this.Context, salesTransaction, request.Cart.CartType);

                // Calculate the required reason codes after the price calculation
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodes(this.Context, salesTransaction, ReasonCodeSourceType.None);

                CartWorkflowHelper.SaveSalesTransaction(this.Context, salesTransaction);

                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, salesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);

                return new SaveCartResponse(cart);
            }
        }
    }
}