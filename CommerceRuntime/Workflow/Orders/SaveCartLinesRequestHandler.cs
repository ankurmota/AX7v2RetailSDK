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
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Saves a cart line.
        /// </summary>
        public sealed class SaveCartLinesRequestHandler : SingleRequestHandler<SaveCartLinesRequest, SaveCartLinesResponse>
        {
            /// <summary>
            /// Saves the cart lines based on the request operation type.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The save cart line response.</returns>
            protected override SaveCartLinesResponse Process(SaveCartLinesRequest request)
            {
                ThrowIf.Null(request, "request");

                // Load sales transaction.
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(request.RequestContext, request.Cart.Id);

                if (transaction == null)
                {
                    throw new CartValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound, request.Cart.Id);
                }

                // Load return sales transaction.
                // Get the returned sales transaction if the cart contains a return line.
                SalesTransaction returnTransaction = CartWorkflowHelper.LoadSalesTransactionForReturn(request.RequestContext, request.Cart, transaction, request.OperationType);
                transaction.IsReturnByReceipt = returnTransaction != null;

                // Get the products in the cart lines
                IDictionary<long, SimpleProduct> productsByRecordId = CartWorkflowHelper.GetProductsInCartLines(this.Context, request.Cart.CartLines);

                // Validate the save cart lines request against the sales transaction.
                CartWorkflowHelper.ValidateSaveCartLinesRequest(this.Context, request, transaction, returnTransaction, productsByRecordId);

                // Process the request.
                switch (request.OperationType)
                {
                    case TransactionOperationType.Create:
                        ProcessCreateCartLinesRequest(this.Context, request, transaction, returnTransaction, productsByRecordId);
                        break;

                    case TransactionOperationType.Update:
                        ProcessUpdateCartLinesRequest(this.Context, request, transaction);
                        break;

                    case TransactionOperationType.Delete:
                        ProcessDeleteCartLinesRequest(request, transaction);
                        break;

                    case TransactionOperationType.Void:
                        ProcessVoidCartLinesRequest(this.Context, request, transaction);
                        break;

                    default:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                            string.Format("Operation {0} is not supported on cart lines.", request.OperationType));
                }

                // Recalculates the sales transaction after processing the request.
                RecalculateSalesTransaction(this.Context, request, transaction);

                // Validate price on sales line after calculations
                CartWorkflowHelper.ValidateSalesLinePrice(this.Context, transaction, productsByRecordId);

                // Save the sales transaction.
                CartWorkflowHelper.SaveSalesTransaction(this.Context, transaction);

                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, transaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);

                return new SaveCartLinesResponse(cart);
            }

            /// <summary>
            /// Recalculates the sales transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current transaction.</param>
            private static void RecalculateSalesTransaction(RequestContext context, SaveCartLinesRequest request, SalesTransaction transaction)
            {
                // Sets the wharehouse id and invent location id for each line
                ItemAvailabilityHelper.SetSalesLineInventory(context, transaction);

                // Calculate totals and saves the sales transaction
                CartWorkflowHelper.Calculate(context, transaction, request.CalculationModes);

                // Calculate the required reason codes after the price calculation
                ReasonCodesWorkflowHelper.CalculateRequiredReasonCodes(context, transaction, ReasonCodeSourceType.None);
            }

            /// <summary>
            /// Processes the create cart lines request.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="returnTransaction">Return transaction.</param>
            /// <param name="productByRecordId">The mapping of products by record identifier.</param>
            private static void ProcessCreateCartLinesRequest(RequestContext context, SaveCartLinesRequest request, SalesTransaction transaction, SalesTransaction returnTransaction, IDictionary<long, SimpleProduct> productByRecordId)
            {
                // Create the new cart lines.
                var salesLines = new List<SalesLine>();
                foreach (CartLine cartLine in request.CartLines)
                {
                    var salesLine = new SalesLine();

                    if (!cartLine.LineData.IsReturnByReceipt)
                    {
                        // Creates a sales line base on the cart line
                        salesLine.CopyPropertiesFrom(cartLine.LineData);

                        // Set ItemId and VariantInventDimId of the sales line, if the cart line is constructed from listing.
                        if (cartLine.LineData.IsProductLine)
                        {
                            long id = cartLine.LineData.ProductId;
                            SimpleProduct product = productByRecordId[id];
                            salesLine.ItemId = product.ItemId;
                            salesLine.ProductId = id;
                            salesLine.InventoryDimensionId = product.InventoryDimensionId;
                            salesLine.Variant = ProductVariant.ConvertFrom(product);
                        }
                    }
                    else
                    {
                        // Creates a sales line base on the retuned sales line
                        var returnedSalesLine = CartWorkflowHelper.GetSalesLineByNumber(returnTransaction, cartLine.LineData.ReturnLineNumber);
                        CartWorkflowHelper.SetSalesLineBasedOnReturnedSalesLine(salesLine, returnedSalesLine, returnTransaction, cartLine.LineData.Quantity);

                        // Calculate required reason code lines for return transaction.
                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, transaction, salesLine, ReasonCodeSourceType.ReturnItem);
                    }

                    // Assign sales line Id. Using format 'N' to remove dashes from the GUID.
                    salesLine.LineId = Guid.NewGuid().ToString("N");

                    // Add sales lines to collection.
                    salesLines.Add(salesLine);
                }

                // Set default attributes from order header.
                CartWorkflowHelper.SetDefaultDataOnSalesLines(context, transaction, salesLines);

                // Add sales lines to transation.
                transaction.SalesLines.AddRange(salesLines);
            }

            /// <summary>
            /// Processes the update cart lines request.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current transaction.</param>
            private static void ProcessUpdateCartLinesRequest(RequestContext context, SaveCartLinesRequest request, SalesTransaction transaction)
            {
                Dictionary<string, SalesLine> salesLinesById = transaction.SalesLines.ToDictionary(sl => sl.LineId, sl => sl);

                // Keep track of updated sales lines.
                var updatedSalesLines = new List<SalesLine>();

                // Update sales lines.
                foreach (CartLine cartLine in request.CartLines)
                {
                    var salesLine = salesLinesById[cartLine.LineId];

                    if (salesLine.Quantity != cartLine.Quantity)
                    {
                        // Validate permissions.
                        context.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.SetQuantity));
                    }

                    if (!salesLine.IsReturnByReceipt)
                    {
                        // Copy the properties from the cart line
                        salesLine.CopyPropertiesFrom(cartLine.LineData);

                        // we have to preserve the LineId, regardless what is set on line data
                        salesLine.LineId = cartLine.LineId;
                    }
                    else
                    {
                        // For return
                        // Keep the properties on the sales line and only copy the quantity from the cart line
                        salesLine.Quantity = cartLine.LineData.Quantity;

                        // Calculate required reason code lines for return item.
                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, transaction, salesLine, ReasonCodeSourceType.ReturnItem);
                    }

                    updatedSalesLines.Add(salesLine);
                }

                // Set default attributes for the updated sales lines.
                if (updatedSalesLines.Any())
                {
                    CartWorkflowHelper.SetDefaultDataOnSalesLines(context, transaction, updatedSalesLines);
                }
            }

            /// <summary>
            /// Processes the delete cart lines request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current transaction.</param>
            private static void ProcessDeleteCartLinesRequest(SaveCartLinesRequest request, SalesTransaction transaction)
            {
                Dictionary<string, SalesLine> salesLinesById = transaction.SalesLines.ToDictionary(sl => sl.LineId, sl => sl);

                foreach (CartLine cartLine in request.CartLines)
                {
                    var salesLine = salesLinesById[cartLine.LineId];

                    // Removing the linked products' sales lines if any.
                    if (salesLine.LineIdsLinkedProductMap.Any())
                    {
                        foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                        {
                            transaction.SalesLines.Remove(salesLinesById[lineId]);
                        }
                    }

                    // Removing the reference to the linked product from the parent product sales line if the linked product is removed from cart.
                    if (!string.IsNullOrWhiteSpace(salesLine.LinkedParentLineId))
                    {
                        var parentLine = transaction.SalesLines.Single(i => i.LineId.Equals(salesLinesById[salesLine.LinkedParentLineId].LineId));
                        parentLine.LineIdsLinkedProductMap.Remove(salesLine.LineId);
                    }

                    transaction.SalesLines.Remove(salesLine);
                }
            }

            /// <summary>
            /// Processes the void cart lines request.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="request">The request.</param>
            /// <param name="salesTransaction">Sales transaction.</param>
            private static void ProcessVoidCartLinesRequest(RequestContext context, SaveCartLinesRequest request, SalesTransaction salesTransaction)
            {
                Dictionary<string, SalesLine> salesLinesById = salesTransaction.SalesLines.ToDictionary(sl => sl.LineId, sl => sl);

                // Keeps track of the enabled (unvoided) sales lines.
                var enabledSalesLines = new List<SalesLine>();

                foreach (CartLine cartLine in request.CartLines)
                {
                    var salesLine = salesLinesById[cartLine.LineId];

                    if (salesTransaction.CartType == CartType.CustomerOrder &&
                        salesTransaction.CustomerOrderMode != CustomerOrderMode.CustomerOrderCreateOrEdit &&
                        salesTransaction.CustomerOrderMode != CustomerOrderMode.QuoteCreateOrEdit &&
                        cartLine.IsVoided)
                    {
                        string errorMessage = "Cart line can be voided only at the time of CustomerOrderCreateOrEdit or QuoteCreateOrEdit.";
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidCustomerOrderModeForVoidProducts, errorMessage);
                    }

                    if ((cartLine.IsCustomerAccountDeposit || salesTransaction.CartType == CartType.AccountDeposit) && cartLine.IsVoided)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerAccountDepositCannotBeVoided, "Cart line cannot be voided for customer account deposit transaction.");
                    }

                    if (!cartLine.IsVoided && salesLine.IsVoided)
                    {
                        // Unvoid
                        if (cartLine.IsGiftCardLine)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_GiftCardLineVoidReversalNotSupported, "Gift card line cannot be unvoided.");
                        }

                        // Unvoid the sales line.
                        salesLine.IsVoided = false;

                        // Unvoid the linked products' sales lines if any.
                        if (salesLine.LineIdsLinkedProductMap.Any())
                        {
                            foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                            {
                                if (salesLinesById[lineId] != null)
                                {
                                    salesLinesById[lineId].IsVoided = false;
                                    enabledSalesLines.Add(salesLinesById[lineId]);
                                }
                                else
                                {
                                    throw new DataValidationException(
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                                        string.Format("Sales line of the linked product with id : {0} was not found.", lineId));
                                }
                            }
                        }

                        // Add the new line to the collection for attribute updates.
                        enabledSalesLines.Add(salesLine);

                        // Perform additional side-effect logic here (i.e. issue gift cart etc.)
                    }
                    else
                    {
                        // Process reason code lines on the cart line.
                        ReasonCodesWorkflowHelper.AddOrUpdateReasonCodeLinesOnSalesLine(salesLine, cartLine, salesTransaction.Id);

                        // Calculate the required reason codes for voiding sales lines.
                        ReasonCodesWorkflowHelper.CalculateRequiredReasonCodesOnSalesLine(context, salesTransaction, salesLine, ReasonCodeSourceType.VoidItem);

                        // Void the sales line.
                        salesLine.IsVoided = true;

                        // Void the linked products' sales lines if any.
                        if (salesLine.LineIdsLinkedProductMap.Any())
                        {
                            foreach (string lineId in salesLine.LineIdsLinkedProductMap.Keys)
                            {
                                if (salesLinesById[lineId] != null)
                                {
                                    salesLinesById[lineId].IsVoided = true;
                                }
                                else
                                {
                                    throw new DataValidationException(
                                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, 
                                        string.Format("Sales line of the linked product with id : {0} was not found.", lineId));
                                }
                            }
                        }

                        // Void gift card lines.
                        if (salesLine.IsGiftCardLine)
                        {
                            GiftCardWorkflowHelper.VoidGiftCardOperation(context, salesTransaction, salesLine.GiftCardId, salesLine.GiftCardCurrencyCode, salesLine.GiftCardOperation, salesLine.TotalAmount);
                        }

                        CartWorkflowHelper.LogAuditEntry(
                            context,
                            "SaveCartLinesRequestHandler.ProcessVoidCartLinesRequest",
                            string.Format("Line item voided: {0}, #: {1}", salesLine.Description, salesLine.LineNumber));
                    }
                }

                // Set default attributes from order header if there are any enabled sales lines.
                if (enabledSalesLines.Any())
                {
                    CartWorkflowHelper.SetDefaultDataOnSalesLines(context, salesTransaction, enabledSalesLines);
                }
            }
        }
    }
}
