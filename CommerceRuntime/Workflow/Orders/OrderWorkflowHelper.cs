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
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Helper for orders related workflows.
        /// </summary>
        internal static class OrderWorkflowHelper
        {
            /// <summary>
            /// Creates a sales order with the provided payment properties.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>The sales order created.</returns>
            public static SalesOrder CreateSalesOrder(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
    
                NetTracer.Information("OrderWorkflowHelper.CreateSalesOrder(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);
    
                CreateSalesOrderServiceRequest createSalesOrderRequest = new CreateSalesOrderServiceRequest(transaction);
    
                var response = context.Execute<CreateSalesOrderServiceResponse>(createSalesOrderRequest);
    
                if (string.IsNullOrWhiteSpace(transaction.Id))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound,
                        "Sales transaction id is empty");
                }
    
                return response.SalesOrder;
            }
    
            /// <summary>
            /// Validates the request context for sales order creation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <exception cref="DataValidationException">If order is not valid.</exception>
            public static void ValidateContextForCreateOrder(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
    
                switch (transaction.TransactionType)
                {
                    case SalesTransactionType.CustomerOrder:
                    case SalesTransactionType.Sales:
                        {
                            ValidateSalesLinesForCreateOrder(context, transaction);
                            CalculateAndValidateAmountPaidForCheckout(context, transaction);
                            break;
                        }
    
                    case SalesTransactionType.IncomeExpense:
                    case SalesTransactionType.CustomerAccountDeposit:
                        {
                            break;
                        }
    
                    case SalesTransactionType.PendingSalesOrder:
                        {
                            // If we don't have the inventory location identifier set, we cannot create the order
                            if (string.IsNullOrWhiteSpace(transaction.InventoryLocationId))
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "It is not possible to create an order without the inventory location identifier.");
                            }
    
                            // no additional validation required. Payment amount is validated in ProcessPendingOrderPayments() method.
                            ValidateSalesLinesForCreateOrder(context, transaction);
                            break;
                        }
    
                    default:
                        throw new InvalidOperationException(string.Format("Transaction type '{0}' is not supported.", transaction.TransactionType));
                }
            }
    
            /// <summary>
            /// Calculate due and paid amounts, validate they fulfill checkout requirements.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="salesTransaction">Sales transaction.</param>
            public static void CalculateAndValidateAmountPaidForCheckout(RequestContext context, SalesTransaction salesTransaction)
            {
                CartWorkflowHelper.CalculateAmountsPaidAndDue(context, salesTransaction);
    
                if (!salesTransaction.IsRequiredAmountPaid)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AmountDueMustBePaidBeforeCheckout, "Amount due must be paid before checkout.");
                }
            }
    
            /// <summary>
            /// Handle payments for the order.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="cartTenderLines">The tender lines containing authorization request.</param>
            /// <returns>The tender line created after processing payment.</returns>
            public static List<TenderLine> ProcessPendingOrderPayments(RequestContext context, SalesTransaction transaction,  IEnumerable<CartTenderLine> cartTenderLines)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
                ThrowIf.Null(cartTenderLines, "cartTenderLines");
    
                // Assign id to each cart tender lines.
                foreach (CartTenderLine cartTenderLine in cartTenderLines)
                {
                    cartTenderLine.TenderLineId = Guid.NewGuid().ToString();
                }
    
                decimal totalTenderedAmount = GetPaymentsSum(cartTenderLines);
    
                // If the total of tender line amounts do not match cart total we cannot create order.
                if (transaction.TotalAmount != totalTenderedAmount)
                {
                    var exception = new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AmountDueMustBePaidBeforeCheckout, 
                        string.Format("Tender line totals do not match cart total. Transaction = {0}, Tender Total = {1}, Cart Total = {2}", transaction.Id, totalTenderedAmount, transaction.TotalAmount));
    
                    throw exception;
                }
    
                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var channelTenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection.Results;
    
                // There can be only one credit card tenderline, raw or tokenized. If more than one credit card is defined, then we cannot create order.
                OrderWorkflowHelper.ThrowIfMultipleCreditCardTenderLines(cartTenderLines, channelTenderTypes);
    
                NetTracer.Information("OrderWorkflowHelper.AuthorizePayments(): Transaction = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);
    
                List<TenderLine> tenderLines = new List<TenderLine>();
    
                try
                {
                    foreach (CartTenderLine cartTenderLine in cartTenderLines)
                    {
                        TenderType tenderType = channelTenderTypes.SingleOrDefault(channelTenderType => string.Equals(channelTenderType.TenderTypeId, cartTenderLine.TenderTypeId, StringComparison.OrdinalIgnoreCase));
                        if (tenderType == null)
                        {
                            var message = string.Format("The tender type id '{0}' specified in cart tenderline '{1}' does not match any of the tender types supported by channel.", cartTenderLine.TenderTypeId, cartTenderLine.TenderLineId);
                            throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindConfigForTenderType, message);
                        }
    
                        TenderLine tenderLine;
                        if (tenderType.OperationType == RetailOperation.PayCard)
                        {
                            tenderLine = GenerateCardTokenAndGetAuthorization(context, cartTenderLine);
                        }
                        else
                        {
                            tenderLine = AuthorizeAndCapturePayment(context, transaction, cartTenderLine, skipLimitValidation: false);
                        }
    
                        tenderLines.Add(tenderLine);
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        // Cancel the payment authorizations
                        if (tenderLines.Any())
                        {
                            CancelPayments(context, transaction, tenderLines, cartTenderLines);
                        }
                    }
                    catch (Exception cancelPaymentsEx)
                    {
                        RetailLogger.Log.CrtWorkflowCancelingPaymentFailure(ex, cancelPaymentsEx);
                        throw;
                    }
    
                    throw;
                }
    
                // Setting tender lines on the salesTransaction here so they can be used for saving order in the database.
                transaction.TenderLines.Clear();
                transaction.TenderLines.AddRange(tenderLines);
    
                return tenderLines;
            }
    
            /// <summary>
            /// Cancels the card authorized payments.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="tenderLines">The tender lines containing authorization responses.</param>
            /// <param name="cartTenderLines">The cart tender lines containing authorization.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "AggregateException is rethrown at the end of method call. We like to recast any exception encountered to a PaymentException type.")]
            public static void CancelPayments(RequestContext context, SalesTransaction transaction, IEnumerable<TenderLine> tenderLines, IEnumerable<CartTenderLine> cartTenderLines)
            {
                if (tenderLines == null || !tenderLines.Any())
                {
                    // Nothing to do as there are no authorization responses.
                    return;
                }
    
                foreach (TenderLine tenderLine in tenderLines)
                {
                    // For each authorization response, cancel/void the payment.
                    if (tenderLine.Status == TenderLineStatus.Committed || tenderLine.Status == TenderLineStatus.PendingCommit)
                    {
                        try
                        {
                            var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                            var channelTenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection.Results;
    
                            TenderType tenderType = channelTenderTypes.SingleOrDefault(channelTenderType => string.Equals(channelTenderType.TenderTypeId, tenderLine.TenderTypeId, StringComparison.OrdinalIgnoreCase));
                            if (tenderType.OperationId == (int)RetailOperation.PayCard)
                            {
                                VoidCardAuthorization(context, tenderLine, transaction);
                            }
                            else
                            {
                                CartTenderLine refundCartTenderLine = cartTenderLines.SingleOrDefault(t => t.TenderLineId == tenderLine.TenderLineId);
    
                                if (refundCartTenderLine == null)
                                {
                                    var message = string.Format(CultureInfo.InvariantCulture, "Voiding payment failed due to not being able find original tender line that represents credit card authorization. Line id: {0}.", tenderLine.TenderLineId);
                                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCancelPayment, message);
                                }
    
                                RefundPayment(context, transaction, refundCartTenderLine);
                            }
                        }
                        catch (PaymentException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.InvariantCulture, "Payment cancellation failed for tender line ({0}) with tender type id ({1}).", tenderLine.TenderLineId, tenderLine.TenderTypeId);
                            throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCancelPayment, ex, message);
                        }
                    }
                }
            }
    
            /// <summary>
            /// Cancels the authorized payments for only authorized card type.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="tenderLine">The tender lines containing authorization responses.</param>
            /// <param name="transaction">Transaction that line belongs to.</param>
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "PaymentException is rethrown at the end of method call.")]
            public static void VoidCardAuthorization(RequestContext context, TenderLine tenderLine, SalesTransaction transaction)
            {
                try
                {
                    VoidPaymentServiceRequest voidRequest = new VoidPaymentServiceRequest(transaction, tenderLine);
                    IRequestHandler cardPaymentHandler = context.Runtime.GetRequestHandler(voidRequest.GetType(), (int)RetailOperation.PayCard);
                    context.Execute<VoidPaymentServiceResponse>(voidRequest, cardPaymentHandler);
                }
                catch (PaymentException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "Payment cancellation failed for tender line ({0}) with tender type id ({1}).", tenderLine.TenderLineId, tenderLine.TenderTypeId);
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToCancelPayment, ex, message);
                }
            }
    
            /// <summary>
            /// Refunds payment for captured card type.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="refundTenderLine">The tender lines containing authorization responses.</param>
            /// <returns>The Tender line created after processing payment for return.</returns>
            public static TenderLine RefundPayment(RequestContext context, SalesTransaction transaction, CartTenderLine refundTenderLine)
            {
                refundTenderLine.Amount = decimal.Negate(refundTenderLine.Amount);
                return AuthorizeAndCapturePayment(context, transaction, refundTenderLine, skipLimitValidation: true);
            }
    
            /// <summary>
            /// Fills the missing requirements for order creation.
            /// In this case, defaults the CustomerId to the Default Customer set on the channel if none specified.
            /// Creates an empty shipping address on null address at the order level.
            /// Populate inventory location based on the store number.
            /// Populate the channel reference identifier if none is specified for a pending sales order.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to be updated.</param>
            public static void FillMissingRequirementsForOrder(RequestContext context, SalesTransaction salesTransaction)
            {
                if (string.IsNullOrWhiteSpace(salesTransaction.CustomerId))
                {
                    var getChannelByIdDataRequest = new GetChannelByIdDataRequest(context.GetPrincipal().ChannelId);
                    Channel channel = context.Runtime.Execute<SingleEntityDataServiceResponse<Channel>>(getChannelByIdDataRequest, context).Entity;
                    salesTransaction.CustomerId = channel.DefaultCustomerAccount;
                }
    
                if (salesTransaction.ShippingAddress == null)
                {
                    salesTransaction.ShippingAddress = new Address();
                }
    
                if (!salesTransaction.RequestedDeliveryDate.HasValue)
                {
                    DateTimeOffset channelDateTime = context.GetNowInChannelTimeZone();
                    salesTransaction.RequestedDeliveryDate = channelDateTime;
                }
    
                // Need to fill fields for all non-voided lines.
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    if (!salesLine.RequestedDeliveryDate.HasValue)
                    {
                        salesLine.RequestedDeliveryDate = salesTransaction.RequestedDeliveryDate;
                    }
                }
    
                if (string.IsNullOrWhiteSpace(salesTransaction.ChannelReferenceId) && salesTransaction.TransactionType == SalesTransactionType.PendingSalesOrder)
                {
                    GenerateOrderNumberServiceRequest generateOrderNumberServiceRequest = new GenerateOrderNumberServiceRequest(salesTransaction.ReceiptEmail);
                    GenerateOrderNumberServiceResponse generateOrderNumberServiceResponse = context.Execute<GenerateOrderNumberServiceResponse>(generateOrderNumberServiceRequest);
                    salesTransaction.ChannelReferenceId = generateOrderNumberServiceResponse.OrderNumber;
                }
    
                // Fill store information
                OrderWorkflowHelper.FillDeliveryStoreInformation(context, salesTransaction);
            }
            
            /// <summary>
            /// Fill sales transaction with data from request context. 
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction object.</param>
            internal static void FillTransactionWithContextData(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(transaction, "transaction");
    
                ICommercePrincipal principal = context.GetPrincipal();
                if (principal == null)
                {
                    return; // do nothing
                }
    
                // Only retail stores having terminals / store identifiers
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                {
                    if (context.GetTerminal() != null)
                    {
                        transaction.TerminalId = context.GetTerminal().TerminalId;
                    }                   
    
                    if (context.GetOrgUnit() != null)
                    {
                        transaction.StoreId = context.GetOrgUnit().OrgUnitNumber;
                    }

                    if (transaction.ShiftId == 0)
                    {
                        transaction.ShiftId = principal.ShiftId;
                    }

                    if (!string.IsNullOrEmpty(principal.ShiftTerminalId))
                    {
                        transaction.ShiftTerminalId = principal.ShiftTerminalId;
                    }
                }
    
                if (principal.IsEmployee)
                {
                    // For Customer orders we allow to change Sales person
                    transaction.StaffId = transaction.CartType == CartType.CustomerOrder && transaction.StaffId != null
                        ? transaction.StaffId
                        : context.GetPrincipal().UserId;
                }
    
                transaction.ChannelId = context.GetPrincipal().ChannelId;
            }
    
            /// <summary>
            /// Fills in the receipt identifier into the request context.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="receiptNumberSequence">The receipt number sequence.</param>
            internal static void FillInReceiptId(RequestContext context, SalesTransaction salesTransaction, string receiptNumberSequence)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                if (context.GetPrincipal() == null)
                {
                    return; // do nothing
                }
    
                // Only fill in receipt Id for store (not online channel)
                if (context.GetOrgUnit() != null)
                {
                    GetNextReceiptIdServiceRequest getNextReceiptIdServiceRequest = new GetNextReceiptIdServiceRequest(
                        salesTransaction.TransactionType,
                        salesTransaction.NetAmountWithNoTax,
                        receiptNumberSequence,
                        salesTransaction.CustomerOrderMode);
    
                    GetNextReceiptIdServiceResponse getNextReceiptIdServiceResponse = context.Execute<GetNextReceiptIdServiceResponse>(getNextReceiptIdServiceRequest);
    
                    salesTransaction.ReceiptId = getNextReceiptIdServiceResponse.NextReceiptId;
                }
            }
    
            /// <summary>
            /// Sets the variant information on the sales lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The transaction to update.</param>
            internal static void FillVariantInformation(RequestContext context, SalesTransaction salesTransaction)
            {
                // Retrieve all sales lines that have variant information specified and construct the
                // item variant inventory dimension identifier for each line.
                var salesLinesWithVariants = from line in salesTransaction.SalesLines
                                             where !string.IsNullOrWhiteSpace(line.InventoryDimensionId)
                                                && (line.Variant == null || line.Variant.DistinctProductVariantId == 0)
                                             select new
                                             {
                                                 SalesLine = line,
                                                 ItemVariantId = new ItemVariantInventoryDimension(line.ItemId, line.InventoryDimensionId),
                                             };
    
                if (!salesLinesWithVariants.Any())
                {
                    return;
                }

                // Retrieve all of the variants in a single database roundtrip and create a map for lookups.
                Dictionary<ItemVariantInventoryDimension, ProductVariant> variantsMap;
                var itemVariantIds = salesLinesWithVariants.Select(key => key.ItemVariantId);
    
                var getVariantsRequest = new GetProductVariantsDataRequest(itemVariantIds);
                ReadOnlyCollection<ProductVariant> variants = context.Runtime.Execute<EntityDataServiceResponse<ProductVariant>>(getVariantsRequest, context).PagedEntityCollection.Results;
                variantsMap = variants.ToDictionary(key => new ItemVariantInventoryDimension(key.ItemId, key.InventoryDimensionId));
    
                // For all sales lines that had variants, we update the variant information.
                foreach (var result in salesLinesWithVariants)
                {
                    ProductVariant variant;
                    if (variantsMap.TryGetValue(result.ItemVariantId, out variant))
                    {
                        result.SalesLine.Variant = variant;
                    }
                }
            }
    
            /// <summary>
            /// Gets the payments sum.
            /// </summary>
            /// <param name="tenderLines">The tender lines.</param>
            /// <returns>The total payment made across all the tender lines.</returns>
            internal static decimal GetPaymentsSum(IEnumerable<TenderLineBase> tenderLines)
            {
                ThrowIf.Null(tenderLines, nameof(tenderLines));
    
                var notVoidedTenderLines = tenderLines.Where(t => t.Status != TenderLineStatus.Voided && t.Status != TenderLineStatus.Historical);
                decimal paymentsSum = notVoidedTenderLines.Sum(t => t.Amount);
    
                return paymentsSum;
            }
    
            /// <summary>
            /// Updates/adds tender lines such that the payments total becomes equal to order total.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <returns>Change tender line; null if change amount is zero.</returns>
            internal static TenderLine GetChangeTenderLine(RequestContext context, SalesTransaction salesTransaction)
            {
                if (salesTransaction.AmountDue > 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AmountDueMustBePaidBeforeCheckout, "Amount due must be paid before checkout.");
                }
    
                if (salesTransaction.AmountDue == 0
                    || !salesTransaction.TenderLines.Any())
                {
                    // Exact payment or exchange scenario.
                    return null;
                }
    
                decimal changeBackAmount = decimal.Negate(salesTransaction.AmountDue);
    
                // Change should be given in store currency
                string changeCurrencyCode = context.GetChannelConfiguration().Currency;
    
                // We will always use the very latest method of payment for providing change.
                var lastTenderLine = salesTransaction.TenderLines.Last();
    
                var getChangeRequest = new GetChangePaymentServiceRequest(salesTransaction, changeBackAmount, changeCurrencyCode, lastTenderLine.TenderTypeId);
                IRequestHandler paymentManagerHandler = context.Runtime.GetRequestHandler(getChangeRequest.GetType(), CartWorkflowHelper.PaymentManagerServiceName);
                GetChangePaymentServiceResponse getChangeResponse = context.Execute<GetChangePaymentServiceResponse>(getChangeRequest, paymentManagerHandler);
    
                return getChangeResponse.TenderLine;
            }
    
            /// <summary>
            /// Process payments for sales transaction checkout.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <returns>Updated sales transaction.</returns>
            internal static SalesTransaction ProcessCheckoutPayments(RequestContext context, SalesTransaction salesTransaction)
            {
                // Add tender lines for over payment, if needed.
                TenderLine changeBackTenderLine = GetChangeTenderLine(context, salesTransaction);
    
                // Capture payments (will save/reload transaction).
                salesTransaction = CapturePayments(context, salesTransaction, salesTransaction.TenderLines, changeBackTenderLine);
    
                return salesTransaction;
            }
    
            /// <summary>
            /// Captures the payments and save sales transaction.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="tenderLines">The tender lines that represents payments.</param>
            /// <param name="changeBackTenderLine">The tender line that represents change back.</param>
            /// <returns>Returns the sales transaction updated after captures.</returns>
            internal static SalesTransaction CapturePayments(RequestContext context, SalesTransaction transaction, IList<TenderLine> tenderLines, TenderLine changeBackTenderLine)
            {
                try
                {
                    // Capture payments
                    for (int i = 0; i < tenderLines.Count; i++)
                    {
                        var captureTenderRequest = new CapturePaymentServiceRequest(tenderLines[i], transaction);
                        IRequestHandler paymentManagerService = context.Runtime.GetRequestHandler(captureTenderRequest.GetType(), CartWorkflowHelper.PaymentManagerServiceName);
                        var captureTenderResponse = context.Execute<CapturePaymentServiceResponse>(captureTenderRequest, paymentManagerService);
                        tenderLines[i] = captureTenderResponse.TenderLine;
                    }
    
                    if (changeBackTenderLine != null)
                    {
                        // Authorize and capture change.
                        TenderLine authorizedChangeBackTenderLine = AuthorizePayment(context, transaction, changeBackTenderLine, skipLimitValidation: true);
                        transaction.TenderLines.Add(authorizedChangeBackTenderLine);
                        TenderLine capturedChangeBackTenderLine = CapturePayment(context, changeBackTenderLine, transaction);
                        transaction.TenderLines.Remove(authorizedChangeBackTenderLine);
                        transaction.TenderLines.Add(capturedChangeBackTenderLine);
                    }
                }
                finally
                {
                    // we need to save the transaction to keep tender line state up to date even in case of failures, but not suppress exception.
                    CartWorkflowHelper.Calculate(context, transaction, CalculationModes.AmountDue);
                    CartWorkflowHelper.SaveSalesTransaction(context, transaction);
                }
    
                // Loads transaction from database so future saves don't hit a version mismatch exception
                transaction = CartWorkflowHelper.LoadSalesTransaction(context, transaction.Id);
    
                return transaction;
            }
    
            /// <summary>
            /// Settle any invoice sales lines.
            /// </summary>
            /// <param name="requestContext">Request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            internal static void SettleInvoiceSalesLines(RequestContext requestContext, SalesTransaction salesTransaction)
            {
                if (salesTransaction != null)
                {
                    foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                    {
                        // Consder only active lines - ignore voided lines
                        if ((salesLine != null)
                            && salesLine.IsInvoiceLine
                            && !salesLine.IsInvoiceSettled)
                        {
                            try
                            {
                                // Settle the invoice lines
                                var settleInvoiceRequest = new SettleInvoiceRealtimeRequest(salesLine.InvoiceId, salesLine.NetAmount, salesTransaction.Id);
                                requestContext.Execute<NullResponse>(settleInvoiceRequest);
                                salesLine.IsInvoiceSettled = true;
                            }
                            catch (Exception ex)
                            {
                                // Inform the cashier that the invoice could not be settled. 
                                throw new DataValidationException(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_SettleInvoiceFailed,
                                    ex,
                                    string.Format(CultureInfo.InvariantCulture, "Exception while trying to settle payment of invoice: {0}", salesLine.InvoiceId));
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Fills transaction with store information for pickup and shipping lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            private static void FillDeliveryStoreInformation(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Get all non-empty stores numbers for all lines plus header
                var storeNumbers = salesTransaction.ActiveSalesLines.Select(s => s.FulfillmentStoreId).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    
                // This check is required for online channel ship to address/multi-mode shipping options scanario, where the header will not have a store id set.
                if (!string.IsNullOrWhiteSpace(salesTransaction.StoreId))
                {
                    storeNumbers.Add(salesTransaction.StoreId);
                }
    
                // In case of online channel if the entire transaction is shipped to a specific address or 
                // If multi-mode shipping option is chosen where each item is shipped to specific address then this condition will be false.
                if (storeNumbers.Any())
                {
                    // Fill store information using header's for lines that don't have it
                    foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                    {
                        if (string.IsNullOrEmpty(salesLine.FulfillmentStoreId))
                        {
                            salesLine.FulfillmentStoreId = salesTransaction.StoreId;
                        }
                    }
    
                    // Get stores
                    var getStoresByStoreNumbersDataRequest = new SearchOrgUnitDataRequest(storeNumbers, QueryResultSettings.AllRecords);
                    var stores = context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(getStoresByStoreNumbersDataRequest, context).PagedEntityCollection.Results;
                    
                    var storeByNumber = stores.ToDictionary(s => s.OrgUnitNumber);
    
                    // for each line, populate store information
                    foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                    {
                        string storeNumber = string.IsNullOrEmpty(salesLine.FulfillmentStoreId) ? salesTransaction.StoreId : salesLine.FulfillmentStoreId;
    
                        // In case of online channel if multi-mode shipping option is chosen this condition will be false for items which are being shipped to an address.
                        if (!string.IsNullOrWhiteSpace(storeNumber))
                        {
                            OrgUnit orgUnit;
                            if (!storeByNumber.TryGetValue(storeNumber, out orgUnit))
                            {
                                throw new DataValidationException(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStoreNumber,
                                    string.Format("Store with number {0} was not found for line {1}.", storeNumber, salesLine.LineId));
                            }
    
                            string lineDeliveryMode = string.IsNullOrWhiteSpace(salesLine.DeliveryMode)
                                                          ? salesTransaction.DeliveryMode
                                                          : salesLine.DeliveryMode;
    
                            salesLine.InventoryLocationId = OrderWorkflowHelper.GetInventoryLocationId(context, orgUnit, lineDeliveryMode);
                        }
                    }
    
                    // populate store info on header if set
                    if (!string.IsNullOrEmpty(salesTransaction.StoreId))
                    {
                        OrgUnit orgUnit;
    
                        if (!storeByNumber.TryGetValue(salesTransaction.StoreId, out orgUnit))
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStoreNumber, 
                                string.Format("Store with number {0} was not found for transaction header.", salesTransaction.StoreId));
                        }
    
                        salesTransaction.InventoryLocationId = OrderWorkflowHelper.GetInventoryLocationId(context, orgUnit, salesTransaction.DeliveryMode);
                    }
                }
            }
    
            /// <summary>
            /// Gets the inventory location id for a specific store based on delivery mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="store">The store related to the location.</param>
            /// <param name="deliveryMode">Delivery mode used.</param>
            /// <returns>The warehouse id (invent location id) for the store based on the delivery mode.</returns>
            private static string GetInventoryLocationId(RequestContext context, OrgUnit store, string deliveryMode)
            {
                string pickupDeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode ?? string.Empty;
                bool isPickup = pickupDeliveryMode.Equals(deliveryMode, StringComparison.OrdinalIgnoreCase);
    
                if (isPickup)
                {
                    // for pickup, we use default store inventory location
                    return store.InventoryLocationId ?? string.Empty;
                }
                else
                {
                    // for shipping, we use the shipping warehouse, if configure, otherwise we use the default warehouse
                    return string.IsNullOrWhiteSpace(store.ShippingInventLocationId)
                        ? (store.InventoryLocationId ?? string.Empty)
                        : store.ShippingInventLocationId;
                }
            }
    
            /// <summary>
            /// Validates the sales line collection for sales order creation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The collection of sales lines.</param>
            private static void ValidateSalesLinesForCreateOrder(RequestContext context, SalesTransaction salesTransaction)
            {
                // Verify whether items were added
                // Consider only active lines. Ignore voided lines.
                if (!salesTransaction.ActiveSalesLines.Any())
                {
                    var exception = new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "It is not possible to create an order without items.");
    
                    throw exception;
                }
    
                bool hasShippingAddressAtOrderLevel = salesTransaction.ShippingAddress != null;
                bool hasDeliveryModeAtOrderLevel = !string.IsNullOrWhiteSpace(salesTransaction.DeliveryMode);
                string emailDeliveryModeCode = context.GetChannelConfiguration().EmailDeliveryModeCode;
    
                // Consider only active lines. Ignore voided lines.
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    if (salesLine == null)
                    {
                        var exception = new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "It is not possible to create an order with a null sales line.");
    
                        throw exception;
                    }
    
                    if (salesLine.Quantity == 0 && salesTransaction.CustomerOrderMode != CustomerOrderMode.Return)
                    {
                        var exception = new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, "It is not possible to create an order with no quantities set on a sales line.");
    
                        throw exception;
                    }
    
                    if (salesLine.IsProductLine && string.IsNullOrWhiteSpace(salesLine.ItemId))
                    {
                        var exception = new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "It is not possible to create an order with an empty sales line item identifier.");
    
                        throw exception;
                    }
    
                    if (!salesTransaction.IsSales && !hasDeliveryModeAtOrderLevel && string.IsNullOrWhiteSpace(salesLine.DeliveryMode))
                    {
                        var exception = new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PickupModeOfDeliveryNotConfiguredOnHeadquarters, "It is not possible to create an order without a delivery mode code.");
    
                        throw exception;
                    }
    
                    bool isEmailDelivery = string.IsNullOrWhiteSpace(salesLine.DeliveryMode) ? string.Equals(salesTransaction.DeliveryMode, emailDeliveryModeCode, StringComparison.OrdinalIgnoreCase) : string.Equals(salesLine.DeliveryMode, emailDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
                    if (!salesTransaction.IsSales && !isEmailDelivery && !hasShippingAddressAtOrderLevel && salesLine.ShippingAddress == null)
                    {
                        var exception = new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_RequiredValueNotFound, "It is not possible to create an order without a shipping address.");
    
                        throw exception;
                    }
                }
            }
    
            /// <summary>
            /// Generates token and gets authorization.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="cartTenderLine">The authorization cart tender line.</param>
            /// <returns>The tender line created after processing payment.</returns>
            private static TenderLine GenerateCardTokenAndGetAuthorization(RequestContext context, CartTenderLine cartTenderLine)
            {
                ThrowIf.Null(cartTenderLine, "tenderLine");
                TenderLine tenderLine = new TenderLine { TenderLineId = cartTenderLine.TenderLineId };
    
                tenderLine.CopyPropertiesFrom(cartTenderLine);
    
                TokenizedPaymentCard tokenizedPaymentCard;
                if (cartTenderLine.TokenizedPaymentCard != null)
                {
                    // Token response properties blob will remain empty since this token was generated externally.
                    tokenizedPaymentCard = cartTenderLine.TokenizedPaymentCard;
                }
                else
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToAuthorizePayment, "Either one of PaymentCard or TokenizedPaymentCard must be set on cartTenderLine.");
                }
    
                tenderLine.CardToken = tokenizedPaymentCard.CardTokenInfo.CardToken;
    
                AuthorizeTokenizedCardPaymentServiceRequest authorizeRequest = new AuthorizeTokenizedCardPaymentServiceRequest(
                    tenderLine,
                    tokenizedPaymentCard);
    
                IRequestHandler cardPaymentHandler = context.Runtime.GetRequestHandler(authorizeRequest.GetType(), (int)RetailOperation.PayCard);
                AuthorizePaymentServiceResponse authorizeResponse = context.Execute<AuthorizePaymentServiceResponse>(authorizeRequest, cardPaymentHandler);
    
                tenderLine = authorizeResponse.TenderLine;
    
                return tenderLine;
            }
    
            /// <summary>
            /// Authorizes and captures the payment.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="cartTenderLine">The authorization tender line.</param>
            /// <param name="skipLimitValidation">If set to 'true' limits validation (over tender, under tender etc.) will be skipped.</param>
            /// <returns>The tender line created after payment processing.</returns>
            private static TenderLine AuthorizeAndCapturePayment(RequestContext context, SalesTransaction transaction, CartTenderLine cartTenderLine, bool skipLimitValidation)
            {
                ThrowIf.Null(cartTenderLine, "cartTenderLine");
                TenderLine tenderLine = new TenderLine { TenderLineId = cartTenderLine.TenderLineId };
                tenderLine.CopyPropertiesFrom(cartTenderLine);
                var authorizedTenderLine = AuthorizePayment(context, transaction, tenderLine, skipLimitValidation);
                return CapturePayment(context, authorizedTenderLine, transaction);
            }
    
            /// <summary>
            /// Authorizes the payment.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="tenderLine">Tender line to authorize.</param>
            /// <param name="skipLimitValidation">If set to 'true' limits validation (over-tender, under-tender etc.) will be skipped.</param>
            /// <returns>Tender line that represents authorized payment.</returns>
            private static TenderLine AuthorizePayment(RequestContext context, SalesTransaction transaction, TenderLine tenderLine, bool skipLimitValidation)
            {
                AuthorizePaymentServiceRequest authorizeRequest = new AuthorizePaymentServiceRequest(transaction, tenderLine, skipLimitValidation: skipLimitValidation);
    
                IRequestHandler paymentManagerHandler = context.Runtime.GetRequestHandler(authorizeRequest.GetType(), CartWorkflowHelper.PaymentManagerServiceName);
    
                var authorizeResponse = context.Execute<AuthorizePaymentServiceResponse>(authorizeRequest, paymentManagerHandler);
                if (authorizeResponse.TenderLine == null)
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToAuthorizePayment, "Payment service did not return tender line.");
                }
    
                return authorizeResponse.TenderLine;
            }
    
            /// <summary>
            /// Captures the payment.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="tenderLine">Tender line to capture.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Tender line that represents captured payment.</returns>
            private static TenderLine CapturePayment(RequestContext context, TenderLine tenderLine, SalesTransaction transaction)
            {
                switch (tenderLine.Status)
                {
                    case TenderLineStatus.PendingCommit:
                        {
                            CapturePaymentServiceRequest captureRequest = new CapturePaymentServiceRequest(tenderLine, transaction);
                            IRequestHandler paymentManagerHandler = context.Runtime.GetRequestHandler(captureRequest.GetType(), CartWorkflowHelper.PaymentManagerServiceName);
                            CapturePaymentServiceResponse captureResponse = context.Execute<CapturePaymentServiceResponse>(captureRequest, paymentManagerHandler);
                            return captureResponse.TenderLine;
                        }
    
                    case TenderLineStatus.Committed:
                        {
                            return tenderLine;
                        }
    
                    default:
                        {
                            throw new InvalidOperationException(string.Format("Payment authorization returned unexpected tender line status: {0}", tenderLine.Status));
                        }
                }
            }
    
            /// <summary>
            /// Ensures that only a single credit card tender line is added to the cart.
            /// </summary>
            /// <param name="cartTenderLines">The cart tender lines.</param>
            /// <param name="tenderTypes">The tender types.</param>
            private static void ThrowIfMultipleCreditCardTenderLines(IEnumerable<CartTenderLine> cartTenderLines, ReadOnlyCollection<TenderType> tenderTypes)
            {
                IEnumerable<string> paymentCardTenderTypeIds = tenderTypes.Where(t => t.OperationType == RetailOperation.PayCard).Select(t => t.TenderTypeId);
                IEnumerable<CartTenderLine> paymentCardCartTenderLines = cartTenderLines.Where(ctl => paymentCardTenderTypeIds.Contains(ctl.TenderTypeId));
    
                if (paymentCardCartTenderLines.HasMultiple())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MultipleCreditCardPaymentNotSupported, "Tender lines have more than one credit card.");
                }
            }
        }
    }
}
