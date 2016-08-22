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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Helper class for receipt workflow. Used to fetch the type of receipts need to be printed.
        /// </summary>
        internal static class ReceiptWorkflowHelper
        {
            /// <summary>
            /// Gets the types of receipts that are to be printed for a sales transaction.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="context">The request context.</param>
            /// <returns>A hash set of receipt types.</returns>
            internal static HashSet<ReceiptType> GetSalesTransactionReceiptTypes(SalesOrder salesOrder, RequestContext context)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesOrder, "salesOrder");

                HashSet<ReceiptType> receiptTypes = new HashSet<ReceiptType>();

                // for combination of any receipts
                if (ShouldIncludeSalesReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.SalesReceipt);
                }

                if (ShouldIncludeSalesOrderReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.SalesOrderReceipt);
                }

                if (ShouldIncludeReturnLabel(salesOrder, context))
                {
                    receiptTypes.Add(ReceiptType.ReturnLabel);
                }

                if (ShouldIncludeQuotationReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.QuotationReceipt);
                }

                if (ShouldIncludeGiftReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.GiftReceipt);
                }

                if (ShouldIncludeCustomerAccountDepositReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.CustomerAccountDeposit);
                }

                Dictionary<RetailOperation, List<TenderLine>> tenderLineMap = MapRetailOperationToTenderLine(context, salesOrder.TenderLines);

                if (ShouldIncludeCreditCardReceipt(tenderLineMap))
                {
                    receiptTypes.Add(ReceiptType.CardReceiptForShop);
                    receiptTypes.Add(ReceiptType.CardReceiptForCustomer);
                }

                if (ShouldIncludeCreditCardReceiptForReturn(tenderLineMap))
                {
                    receiptTypes.Add(ReceiptType.CardReceiptForCustomerReturn);
                    receiptTypes.Add(ReceiptType.CardReceiptForShopReturn);
                }

                if (ShouldIncludeCreditMemo(tenderLineMap))
                {
                    receiptTypes.Add(ReceiptType.CreditMemo);
                }

                if (ShouldIncludeCustomerAccountReceiptReturn(tenderLineMap))
                {
                    receiptTypes.Add(ReceiptType.CustomerAccountReceiptForCustomerReturn);
                    receiptTypes.Add(ReceiptType.CustomerAccountReceiptForShopReturn);
                }

                if (ShouldIncludeCustomerAccountReceipt(tenderLineMap))
                {
                    receiptTypes.Add(ReceiptType.CustomerAccountReceiptForCustomer);
                    receiptTypes.Add(ReceiptType.CustomerAccountReceiptForShop);
                }

                if (ShouldIncludeGiftCardReceipt(salesOrder))
                {
                    receiptTypes.Add(ReceiptType.GiftCertificate);
                }

                return receiptTypes;
            }

            /// <summary>
            /// Determines whether or not includes QuotationReceipt when printing receipts.
            /// Conditions:
            /// 1. The sales order has to be either CustomerQuote or AsyncCustomerQuote.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeQuotationReceipt(SalesOrder salesOrder)
            {
                if (salesOrder.TransactionType == SalesTransactionType.AsyncCustomerQuote)
                {
                    return true;
                }

                if (salesOrder.CustomerOrderType == CustomerOrderType.Quote)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes SalesOrderReceipt when printing receipts.
            /// Conditions:
            /// 1. The sales order has to be either CustomerOrder or AsyncCustomerOrder.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeSalesOrderReceipt(SalesOrder salesOrder)
            {
                if (salesOrder.TransactionType == SalesTransactionType.CustomerOrder ||
                    salesOrder.TransactionType == SalesTransactionType.AsyncCustomerOrder)
                {
                    return salesOrder.CustomerOrderType == CustomerOrderType.SalesOrder;
                }

                return false;
            }

            private static bool ShouldIncludeSalesReceipt(SalesOrder salesOrder)
            {
                if (salesOrder.TransactionType == SalesTransactionType.Sales ||
                    salesOrder.TransactionType == SalesTransactionType.IncomeExpense ||
                    salesOrder.TransactionType == SalesTransactionType.CustomerAccountDeposit)
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes GiftReceipt when printing receipts.
            /// Conditions:
            /// 1. The transaction must be cash and carry transaction or customer order.
            /// AND
            /// 2. At least have one sales line that is not gift card, nor return, nor voided.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeGiftReceipt(SalesOrder salesOrder)
            {
                if (salesOrder.TransactionType == SalesTransactionType.Sales ||
                    salesOrder.TransactionType == SalesTransactionType.CustomerOrder ||
                    salesOrder.TransactionType == SalesTransactionType.AsyncCustomerOrder)
                {
                    // Add gift receipt only if it's a sale and it contains at least one line which: is not gift card, nor return, nor voided.
                    foreach (SalesLine salesLine in salesOrder.SalesLines)
                    {
                        if (!salesLine.IsVoided &&
                            salesLine.Quantity > 0 &&
                            !salesLine.IsGiftCardLine &&
                            !salesLine.IsReturnByReceipt)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes GiftCard when printing receipts.
            /// Conditions:
            /// 1. At least have one sales line that is not voided and is gift card line.
            /// OR
            /// 2. Paying with gift card.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeGiftCardReceipt(SalesOrder salesOrder)
            {
                // gift card line
                foreach (SalesLine salesLine in salesOrder.SalesLines)
                {
                    if (salesLine.IsGiftCardLine &&
                        !salesLine.IsVoided)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CreditCardReceipt for both customer and store when printing receipts.
            /// Conditions:
            /// 1. At least have one tender line that is pay card.
            /// AND
            /// 2. The amount of this tender line is greater than or equal to 0.
            /// </summary>
            /// <param name="tenderLineMap">The dictionary telling the tender line type.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCreditCardReceipt(Dictionary<RetailOperation, List<TenderLine>> tenderLineMap)
            {
                List<TenderLine> tenderLines = null;
                if (tenderLineMap.TryGetValue(RetailOperation.PayCard, out tenderLines))
                {
                    return tenderLines.Any(tenderLine => tenderLine.Amount >= 0);
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CreditCardReceipt for return for both customer and store when printing receipts.
            /// Conditions:
            /// 1. At least have one tender line that is pay card.
            /// AND
            /// 2. The amount of this tender line is smaller than 0.
            /// </summary>
            /// <param name="tenderLineMap">The dictionary telling the tender line type.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCreditCardReceiptForReturn(Dictionary<RetailOperation, List<TenderLine>> tenderLineMap)
            {
                List<TenderLine> tenderLines = null;
                if (tenderLineMap.TryGetValue(RetailOperation.PayCard, out tenderLines))
                {
                    return tenderLines.Any(tenderLine => tenderLine.Amount < 0);
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CustomerAccountDepositReceipt when printing receipts.
            /// Conditions:
            /// 1. The transaction type must be CustomerAccountDeposit.
            /// AND
            /// 2. At least have one customer account deposit line.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCustomerAccountDepositReceipt(SalesOrder salesOrder)
            {
                if (salesOrder.TransactionType == SalesTransactionType.CustomerAccountDeposit &&
                    salesOrder.CustomerAccountDepositLines != null &&
                    salesOrder.CustomerAccountDepositLines.Any())
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CustomerAccountReceipt for return for both customer and store when printing receipts.
            /// Conditions:
            /// 1. At least have one tender line that is pay customer account.
            /// AND
            /// 2. The amount of this tender line is smaller than 0.
            /// </summary>
            /// <param name="tenderLineMap">The dictionary telling the tender line type.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCustomerAccountReceiptReturn(Dictionary<RetailOperation, List<TenderLine>> tenderLineMap)
            {
                List<TenderLine> tenderLines = null;
                if (tenderLineMap.TryGetValue(RetailOperation.PayCustomerAccount, out tenderLines))
                {
                    return tenderLines.Any(tenderLine => tenderLine.Amount < 0);
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CustomerAccountReceipt for both customer and store when printing receipts.
            /// Conditions:
            /// 1. At least have one tender line that is pay customer account.
            /// AND
            /// 2. The amount of this tender line is greater than or equal to 0.
            /// </summary>
            /// <param name="tenderLineMap">The dictionary telling the tender line type.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCustomerAccountReceipt(Dictionary<RetailOperation, List<TenderLine>> tenderLineMap)
            {
                List<TenderLine> tenderLines = null;
                if (tenderLineMap.TryGetValue(RetailOperation.PayCustomerAccount, out tenderLines))
                {
                    return tenderLines.Any(tenderLine => tenderLine.Amount >= 0);
                }

                return false;
            }

            /// <summary>
            /// Determines whether or not includes CustomerAccountDepositReceipt when printing receipts.
            /// Conditions:
            /// 1. Issue credit memo while returning something.
            /// OR
            /// 2. Pay with credit memo.
            /// </summary>
            /// <param name="tenderLineMap">The dictionary telling the tender line type.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeCreditMemo(Dictionary<RetailOperation, List<TenderLine>> tenderLineMap)
            {
                return tenderLineMap.ContainsKey(RetailOperation.IssueCreditMemo) || tenderLineMap.ContainsKey(RetailOperation.PayCreditMemo);
            }

            /// <summary>
            /// Determines whether or not includes ReturnLabel when printing receipts.
            /// Conditions:
            /// 1. At least have one sales line that is not voided and is return by receipt or direct return.
            /// AND
            /// 2. For this sales line the GetReturnLocationRealtimeResponse returns true.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="context">The request context.</param>
            /// <returns>True if the conditions match, otherwise false.</returns>
            private static bool ShouldIncludeReturnLabel(SalesOrder salesOrder, RequestContext context)
            {
                bool result = false;
                foreach (SalesLine salesLine in salesOrder.SalesLines)
                {
                    if (!salesLine.IsVoided &&
                        (salesLine.IsReturnByReceipt ||
                        salesLine.Quantity < 0))
                    {
                        if (IsLabelPrintingRequired(salesOrder, salesLine, context))
                        {
                            result = true;
                        }
                    }
                }

                return result;
            }

            private static Dictionary<RetailOperation, List<TenderLine>> MapRetailOperationToTenderLine(RequestContext context, Collection<TenderLine> tenderLines)
            {
                var getChannelTenderTypesDataRequest = new GetChannelTenderTypesDataRequest(context.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                var channelTenderTypes = context.Runtime.Execute<EntityDataServiceResponse<TenderType>>(getChannelTenderTypesDataRequest, context).PagedEntityCollection.Results;

                ThrowIf.Null(channelTenderTypes, "channelTenderTypes");

                Dictionary<RetailOperation, List<TenderLine>> results = new Dictionary<RetailOperation, List<TenderLine>>();

                if (tenderLines == null)
                {
                    return results;
                }

                foreach (TenderLine tenderLine in tenderLines)
                {
                    if (tenderLine.Status == TenderLineStatus.Committed)
                    {
                        foreach (TenderType tenderType in channelTenderTypes)
                        {
                            if (tenderLine.TenderTypeId == tenderType.TenderTypeId)
                            {
                                if (results.ContainsKey(tenderType.OperationType))
                                {
                                    results[tenderType.OperationType].Add(tenderLine);
                                }
                                else
                                {
                                    var list = new List<TenderLine>();
                                    list.Add(tenderLine);
                                    results[tenderType.OperationType] = list;
                                }
                            }
                        }
                    }
                }

                return results;
            }

            private static bool IsLabelPrintingRequired(SalesTransaction salesTransaction, SalesLine salesLine, RequestContext context)
            {
                salesLine.ReturnLabelProperties = new ReturnLabelContent();
                GetReturnLocationRealtimeResponse response;

                try
                {
                    // If this is customer order transaction, than we obtain return label fields by reason code; otherwise by info codes.
                    response = salesTransaction.TransactionType == SalesTransactionType.CustomerOrder 
                        ? ObtainByReasonCode(salesTransaction, salesLine, null, context)
                        : ObtainByInfoCode(salesTransaction, salesLine, context);
                }
                catch (FeatureNotSupportedException)
                {
                    // Realtime service is not supported in current configuration.
                    // It should not block checkout workflow hence suppress exception (it is already logged as warning by CommerceRuntime).
                    return false;
                }
                catch (HeadquarterTransactionServiceException ex)
                {
                    // Realtime service call failed. It should not block checkout workflow hence log and suppress exception.
                    RetailLogger.Log.CrtServicesSalesOrderTransactionServiceMarkReturnedItemsFailure(ex);
                    return false;
                }

                if (salesLine.ReasonCodeLines != null &&
                    salesLine.ReasonCodeLines.Any())
                {
                    salesLine.ReturnLabelProperties.ReturnReasonText = salesLine.ReasonCodeLines.First().Information ?? string.Empty;
                }

                salesLine.ReturnLabelProperties.ReturnWarehouseText = response.ReturnWarehouseText;
                salesLine.ReturnLabelProperties.ReturnLocationText = response.ReturnLocationText;
                salesLine.ReturnLabelProperties.ReturnPalleteText = response.ReturnPalleteText;
                return response.PrintReturnLabel;
            }

            private static GetReturnLocationRealtimeResponse ObtainByReasonCode(SalesTransaction order, SalesLine line, string reasonCodeId, RequestContext context)
            {
                GetReturnLocationRealtimeRequest request = new GetReturnLocationRealtimeRequest(order, line, reasonCodeId, false);

                GetReturnLocationRealtimeResponse response = context.Execute<GetReturnLocationRealtimeResponse>(request);
                return response;
            }

            private static GetReturnLocationRealtimeResponse ObtainByInfoCode(SalesTransaction order, SalesLine line, RequestContext context)
            {
                GetReturnLocationRealtimeRequest request = new GetReturnLocationRealtimeRequest(order, line, null, true);

                GetReturnLocationRealtimeResponse response = context.Execute<GetReturnLocationRealtimeResponse>(request);
                return response;
            }
        }
    }
}
