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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The request handler for GetReceiptRequest class.
        /// </summary>
        public sealed class GetReceiptRequestHandler : SingleRequestHandler<GetReceiptRequest, GetReceiptResponse>
        {
            private const string LogoMessage = "<L>";
            private const string OPOSEscapeMarker = "&#x1B;";
            private const string OPOSOneSpaceMarker = "&#x1B;|1C";
            private const string OPOSTwoSpacesMarker = "&#x1B;|2C";
            private const string OPOSNewLineMarker = "\r\n";
            private const string AXNewLineMarker = "\n";
            private const char CharEmptySpace = ' ';
            private readonly string receiptEmailTemplate = "EmailRecpt";
            private readonly string receiptEmailTemplateParameter = "message";

            /// <summary>
            /// Processes the GetReceiptRequest to return the set of receipts. The request should not be null.
            /// </summary>
            /// <param name="request">The request parameter.</param>
            /// <returns>The GetReceiptResponse.</returns>
            protected override GetReceiptResponse Process(GetReceiptRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ReceiptRetrievalCriteria, "request.ReceiptRetrievalCriteria");

                ReceiptRetrievalCriteria criteria = request.ReceiptRetrievalCriteria;
                if (string.IsNullOrWhiteSpace(request.TransactionId))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "Transaction identifier must be provided.");
                }

                switch (criteria.ReceiptType)
                {
                    case ReceiptType.BankDrop:
                    case ReceiptType.SafeDrop:
                    case ReceiptType.TenderDeclaration:
                        {
                            return this.GetReceiptForDropAndDeclarationTransaction(request.TransactionId, criteria);
                        }

                    case ReceiptType.FloatEntry:
                    case ReceiptType.StartingAmount:
                    case ReceiptType.RemoveTender:
                        {
                            return this.GetReceiptForNonSalesTransaction(request.TransactionId, criteria);
                        }

                    case ReceiptType.PackingSlip:
                        {
                            SalesOrder salesOrder = this.GetSalesOrder(request.TransactionId, criteria);
                            var receiptTypes = new HashSet<ReceiptType> { ReceiptType.PackingSlip };

                            return this.GetReceiptsForSalesTransaction(salesOrder, receiptTypes, criteria);
                        }

                    case ReceiptType.PickupReceipt:
                        {
                            SalesOrder salesOrder = this.GetSalesOrder(request.TransactionId, criteria);
                            HashSet<ReceiptType> receiptTypes = ReceiptWorkflowHelper.GetSalesTransactionReceiptTypes(salesOrder, this.Context);

                            // The server does not have enought information to tell if a customer is picking up products
                            // so client must explicitly tell the server that pickup receipt needs to be printed.
                            receiptTypes.Add(ReceiptType.PickupReceipt);

                            // If we are printing PickupReceipt then do not print SalesOrderReceipt
                            if (receiptTypes.Contains(ReceiptType.SalesOrderReceipt))
                            {
                                receiptTypes.Remove(ReceiptType.SalesOrderReceipt);
                            }

                            return this.GetReceiptsForSalesTransaction(salesOrder, receiptTypes, criteria);
                        }

                    // SalesOrderReceipt is for N-1.
                    case ReceiptType.SalesOrderReceipt:
                    case ReceiptType.Unknown:
                        {
                            SalesOrder salesOrder = this.GetSalesOrder(request.TransactionId, criteria);
                            HashSet<ReceiptType> receiptTypes = ReceiptWorkflowHelper.GetSalesTransactionReceiptTypes(salesOrder, this.Context);

                            return this.GetReceiptsForSalesTransaction(salesOrder, receiptTypes, criteria);
                        }

                    // Receipts types requested from ShowJournal should return only the given receipt type
                    case ReceiptType.SalesReceipt:
                    case ReceiptType.GiftReceipt:
                        {
                            SalesOrder salesOrder = this.GetSalesOrder(request.TransactionId, criteria);
                            var receiptTypes = new HashSet<ReceiptType> { criteria.ReceiptType };

                            return this.GetReceiptsForSalesTransaction(salesOrder, receiptTypes, criteria);
                        }

                    case ReceiptType.CustomReceipt1:
                    case ReceiptType.CustomReceipt2:
                    case ReceiptType.CustomReceipt3:
                    case ReceiptType.CustomReceipt4:
                    case ReceiptType.CustomReceipt5:
                        {
                            SalesOrder salesOrder = this.GetTransactionForCustomReceipt(request);
                            var receiptTypes = new HashSet<ReceiptType> { criteria.ReceiptType };

                            var getReceiptServiceRequest = new GetReceiptServiceRequest(
                                salesOrder,
                                receiptTypes,
                                salesOrder.TenderLines,
                                criteria.IsCopy,
                                criteria.IsPreview,
                                criteria.HardwareProfileId);

                            var getReceiptServiceResponse = this.Context.Execute<GetReceiptServiceResponse>(getReceiptServiceRequest);
                            return new GetReceiptResponse(getReceiptServiceResponse.Receipts);
                        }

                    default:
                        throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_ReceiptTypeNotSupported, string.Format("The following receipt type is not supported: {0}", criteria.ReceiptType.ToString()));
                }
            }

            /// <summary>
            /// Make the transaction service call to email the receipt.
            /// </summary>
            /// <param name="salesOrder">The sales order.</param>
            private void SendReceiptMail(SalesOrder salesOrder)
            {
                List<ReceiptType> receiptTypes = new List<ReceiptType>();
                receiptTypes.Add(ReceiptType.SalesReceipt);

                var emailReceipt = new GetEmailReceiptServiceRequest(
                    salesOrder,
                    receiptTypes,
                    salesOrder.TenderLines,
                    false);

                var emailResponse = this.Context.Execute<GetEmailReceiptServiceResponse>(emailReceipt);

                if (emailResponse.Receipts == null || emailResponse.Receipts.Results.Count == 0)
                {
                    return;
                }

                string emailMessage = emailResponse.Receipts.Results[0].Header + emailResponse.Receipts.Results[0].Body + emailResponse.Receipts.Results[0].Footer;
                emailMessage = this.ConvertToHTML(emailMessage);

                string language = string.Empty;

                if (!string.IsNullOrEmpty(salesOrder.CustomerId))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(salesOrder.CustomerId);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, this.Context);
                    Customer customer = getCustomerDataResponse.Entity;

                    if (customer == null)
                    {
                        language = this.Context.GetChannelConfiguration().DefaultLanguageId;
                    }
                    else
                    {
                        language = customer.Language;
                    }
                }

                if (string.IsNullOrEmpty(language))
                {
                    language = CultureInfo.CurrentUICulture.ToString();
                }

                NameValuePair mapping = new NameValuePair
                {
                    Name = this.receiptEmailTemplateParameter,
                    Value = emailMessage
                };

                Collection<NameValuePair> mappings = new Collection<NameValuePair>();
                mappings.Add(mapping);

                var emailServiceRequest = new SendEmailRealtimeRequest(
                    salesOrder.ReceiptEmail,
                    mappings,
                    language,
                    string.Empty,
                    this.receiptEmailTemplate);

                this.Context.Execute<NullResponse>(emailServiceRequest);
            }

            /// <summary>
            /// Get the non sale tender transaction for receipt printing.
            /// </summary>
            /// <param name="transactionId">The transaction identifier.</param>
            /// <param name="tenderType">The non sale tender type.</param>
            /// <param name="shiftId">The identifier of the shift associated with the receipt.</param>
            /// <param name="shiftTerminalId">The identifier of the terminal that creates the shift.</param>
            /// <returns>The non sale tender transaction.</returns>
            private NonSalesTransaction GetNonSaleTransaction(string transactionId, TransactionType tenderType, long shiftId, string shiftTerminalId)
            {
                var serviceRequest = new GetNonSaleTenderServiceRequest { TransactionType = tenderType, TransactionId = transactionId, ShiftId = shiftId.ToString(), ShiftTerminalId = shiftTerminalId };
                var serviceResponse = this.Context.Execute<GetNonSaleTenderServiceResponse>(serviceRequest);
                return serviceResponse.NonSalesTenderOperation.SingleOrDefault();
            }

            /// <summary>
            /// Get the drop and declare tender transaction.
            /// </summary>
            /// <param name="transactionId">The transaction identifier.</param>
            /// <returns>The drop and declare tender transaction.</returns>
            private DropAndDeclareTransaction GetDropAndDeclareTransaction(string transactionId)
            {
                var getDropAndDeclareTransactionDataRequest = new GetDropAndDeclareTransactionDataRequest(transactionId, QueryResultSettings.SingleRecord);
                DropAndDeclareTransaction transaction = this.Context.Runtime.Execute<EntityDataServiceResponse<DropAndDeclareTransaction>>(getDropAndDeclareTransactionDataRequest, this.Context).PagedEntityCollection.FirstOrDefault();

                var getDropAndDeclareTransactionTenderDetailsDataRequest = new GetDropAndDeclareTransactionTenderDetailsDataRequest(transactionId, QueryResultSettings.AllRecords);
                PagedResult<TenderDetail> tenderDetails = this.Context.Runtime.Execute<EntityDataServiceResponse<TenderDetail>>(getDropAndDeclareTransactionTenderDetailsDataRequest, this.Context).PagedEntityCollection;

                transaction.TenderDetails = tenderDetails.Results;
                return transaction;
            }

            /// <summary>
            /// Returns Sales Order by sales id and terminal id. Used to get remote orders from AX which does not have transaction id.
            /// </summary>
            /// <param name="salesId">The sales id parameter.</param>
            /// <returns>The  SalesOrder.</returns>
            private SalesOrder GetTransactionBySalesId(string salesId)
            {
                // Recall the customer order
                var realtimeRequest = new RecallCustomerOrderRealtimeRequest(salesId, isQuote: false);

                var serviceResponse = this.Context.Execute<RecallCustomerOrderRealtimeResponse>(realtimeRequest);
                SalesOrder salesOrder = serviceResponse.SalesOrder;

                // Channel and terminal don't come from ax
                salesOrder.ChannelId = this.Context.GetPrincipal().ChannelId;
                salesOrder.TerminalId = this.Context.GetTerminal().TerminalId;

                // Perform order calculations (deposit, amount due, etc)
                CartWorkflowHelper.Calculate(this.Context, salesOrder, requestedMode: null);

                return salesOrder;
            }

            /// <summary>
            /// Returns Sales Order by transaction id and terminal id. Used to get local orders.
            /// </summary>
            /// <param name="transactionId">The transaction id parameter.</param>
            /// <param name="isRemoteTransaction">Client sends if this is local or remote transaction.</param>
            /// <returns>The SalesOrder.</returns>
            private SalesOrder GetTransactionByTransactionId(string transactionId, bool isRemoteTransaction)
            {
                // Based on order type we decide the Search location to be efficient
                Microsoft.Dynamics.Commerce.Runtime.DataModel.SearchLocation searchLocationType = isRemoteTransaction ? SearchLocation.All : SearchLocation.Local;
                var criteria = new SalesOrderSearchCriteria
                {
                    TransactionIds = new[] { transactionId },
                    SearchLocationType = searchLocationType,
                    IncludeDetails = true,
                    SearchType = OrderSearchType.SalesTransaction
                };

                // Get the order. If order sales id is provided then it should be remote search mode.
                var getOrdersServiceRequest = new GetOrdersServiceRequest(criteria, QueryResultSettings.SingleRecord);
                var getOrdersServiceResponse = this.Context.Execute<GetOrdersServiceResponse>(getOrdersServiceRequest);

                return getOrdersServiceResponse.Orders.Results.SingleOrDefault();
            }

            private GetReceiptResponse GetReceiptForNonSalesTransaction(string transactionId, ReceiptRetrievalCriteria criteria)
            {
                NonSalesTransaction nonSalesTransaction = null;
                long shiftId = criteria.ShiftId.GetValueOrDefault(0);

                switch (criteria.ReceiptType)
                {
                    case ReceiptType.FloatEntry:
                        nonSalesTransaction = this.GetNonSaleTransaction(transactionId, TransactionType.FloatEntry, shiftId, criteria.ShiftTerminalId);
                        break;
                    case ReceiptType.StartingAmount:
                        nonSalesTransaction = this.GetNonSaleTransaction(transactionId, TransactionType.StartingAmount, shiftId, criteria.ShiftTerminalId);
                        break;
                    case ReceiptType.RemoveTender:
                        nonSalesTransaction = this.GetNonSaleTransaction(transactionId, TransactionType.RemoveTender, shiftId, criteria.ShiftTerminalId);
                        break;
                }

                var getReceiptServiceRequest = new GetReceiptServiceRequest(nonSalesTransaction, criteria.ReceiptType, criteria.IsCopy, criteria.IsPreview, criteria.HardwareProfileId);
                var getReceiptServiceResponse = this.Context.Execute<GetReceiptServiceResponse>(getReceiptServiceRequest);
                return new GetReceiptResponse(getReceiptServiceResponse.Receipts);
            }

            private GetReceiptResponse GetReceiptForDropAndDeclarationTransaction(string transactionId, ReceiptRetrievalCriteria criteria)
            {
                DropAndDeclareTransaction dropAndDeclareTransaction = this.GetDropAndDeclareTransaction(transactionId);

                var getReceiptServiceRequest = new GetReceiptServiceRequest(dropAndDeclareTransaction, criteria.ReceiptType, criteria.IsCopy, criteria.IsPreview, criteria.HardwareProfileId);
                var getReceiptServiceResponse = this.Context.Execute<GetReceiptServiceResponse>(getReceiptServiceRequest);
                return new GetReceiptResponse(getReceiptServiceResponse.Receipts);
            }

            private GetReceiptResponse GetReceiptsForSalesTransaction(SalesOrder salesOrder, IEnumerable<ReceiptType> receiptTypes, ReceiptRetrievalCriteria criteria)
            {
                if (!string.IsNullOrEmpty(salesOrder.ReceiptEmail))
                {
                    try
                    {
                        this.SendReceiptMail(salesOrder);
                    }
                    catch (CommunicationException)
                    {
                        // If failed to send email receipt, we still need to print paper receipt.
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // Sending email receipt is not supported in offline mode.
                    }
                }

                var getReceiptServiceRequest = new GetReceiptServiceRequest(
                    salesOrder,
                    receiptTypes,
                    salesOrder.TenderLines,
                    criteria.IsCopy,
                    criteria.IsPreview,
                    criteria.HardwareProfileId);

                var getReceiptServiceResponse = this.Context.Execute<GetReceiptServiceResponse>(getReceiptServiceRequest);
                return new GetReceiptResponse(getReceiptServiceResponse.Receipts);
            }

            private SalesOrder GetSalesOrder(string transactionId, ReceiptRetrievalCriteria criteria)
            {
                SalesOrder salesOrder;
                if (!criteria.QueryBySalesId)
                {
                    // Changed to fix the following bug
                    // [GetReceipts() API requires UI to feed isRemoteOrder parameter, which is not possible in certain scenarios]
                    salesOrder = this.GetTransactionByTransactionId(transactionId, false) ?? this.GetTransactionByTransactionId(transactionId, true);
                }
                else
                {
                    salesOrder = this.GetTransactionBySalesId(transactionId);
                }

                if (salesOrder == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                        string.Format("Unable to get the sales order created. ID: {0}", transactionId));
                }

                return salesOrder;
            }

            /// <summary>
            /// Converts the receipt of OPOS style to HTML style by replacing OPOS characters.
            /// </summary>
            /// <param name="receipt">The receipt string.</param>
            /// <returns>The receipt string in HTML style.</returns>
            private string ConvertToHTML(string receipt)
            {
                if (string.IsNullOrEmpty(receipt))
                {
                    return receipt;
                }

                // Convert bold text markers.
                while (true)
                {
                    var boldMarkerIndex = receipt.IndexOf(OPOSTwoSpacesMarker, StringComparison.OrdinalIgnoreCase);

                    if (boldMarkerIndex < 0)
                    {
                        break;
                    }

                    var endOfLineMarkerIndex = receipt.IndexOf(OPOSNewLineMarker, boldMarkerIndex, StringComparison.OrdinalIgnoreCase);
                    var nextStyleMarkerIndex = receipt.IndexOf(OPOSEscapeMarker, boldMarkerIndex + 1, StringComparison.OrdinalIgnoreCase);
                    int endOfBoldMarkerIndex = Math.Min(endOfLineMarkerIndex, nextStyleMarkerIndex);

                    // The entire bold item, e.g: "012532    ". 
                    string entireBoldItem = receipt.Substring(boldMarkerIndex + OPOSTwoSpacesMarker.Length, endOfBoldMarkerIndex - (boldMarkerIndex + OPOSTwoSpacesMarker.Length));
                    string emptySpaces = new string(CharEmptySpace, entireBoldItem.Length);

                    receipt = receipt.Substring(0, boldMarkerIndex) + entireBoldItem + emptySpaces + receipt.Substring(endOfBoldMarkerIndex);
                }

                receipt = receipt.Replace(OPOSOneSpaceMarker, string.Empty)
                .Replace(OPOSTwoSpacesMarker, string.Empty)
                .Replace(OPOSNewLineMarker, AXNewLineMarker);

                // remove the logo
                receipt = receipt.Replace(LogoMessage, string.Empty);
                return receipt;
            }

            /// <summary>
            /// Gets the transaction for custom receipts.
            /// </summary>
            /// <param name="request">The get receipt request.</param>
            /// <returns>The sales order.</returns>
            /// <remarks>
            /// This method is used to handle custom receipt. Since only sales order related receipt
            /// is using receipt designer while others are hardcoded, so we cannot support customizing receipts for
            /// NonSalesTransaction or DropAndDeclareTransaction.
            /// Here we first try to load the SalesOrder, if we cannot find it, then try to load 
            /// a cart and convert it to sales order.
            /// </remarks>
            private SalesOrder GetTransactionForCustomReceipt(GetReceiptRequest request)
            {
                ReceiptRetrievalCriteria criteria = request.ReceiptRetrievalCriteria;
                string transactionId = request.TransactionId;

                SalesOrder salesOrder = null;

                if (!criteria.QueryBySalesId)
                {
                    salesOrder = this.GetTransactionByTransactionId(transactionId, false) ?? this.GetTransactionByTransactionId(transactionId, true);
                }
                else
                {
                    try
                    {
                        salesOrder = this.GetTransactionBySalesId(transactionId);
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // Not able to get sales order in offline mode.
                    }
                }

                // If cannot find a sales order, then try to find a cart (suspended transaction).
                if (salesOrder == null)
                {
                    SalesTransaction salesTransaction = CartWorkflowHelper.LoadSalesTransaction(request.RequestContext, request.TransactionId);
                    if (salesTransaction != null)
                    {
                        salesOrder = new SalesOrder();

                        // Now ReceiptService only accept SalesOrder. So in order to reuse the code in ReceiptService, we need to convert
                        // SalesTransaction to SalesOrder. SalesOrder is extended from SalesTransaction, so in this case we are good. But
                        // in the future we should refactor ReceiptService to make it accept a common interface so that we can extend to 
                        // other objects.
                        salesOrder.CopyFrom(salesTransaction);
                    }
                    else
                    {
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                            string.Format("Unable to get the transaction created. ID: {0}", transactionId));
                    }
                }

                return salesOrder;
            }
        }
    }
}
