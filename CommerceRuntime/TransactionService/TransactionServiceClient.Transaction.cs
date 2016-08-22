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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.IO;
        using System.Linq;
        using System.Xml;
        using System.Xml.Serialization;
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using CRT = Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string GetTransactionMethodName = "GetTransaction";
            private const string GetTransactionByTransactionIdMethodName = "GetTransactionByTransactionId";
            private const string SearchOrdersMethodName = "SearchOrderList";
            private const string SearchJournalTransactionsMethodName = "GetJournalListSearch";
    
            /// <summary>
            /// Gets the transaction by receipt identifier.
            /// </summary>
            /// <param name="receiptId">The receipt identifier.</param>
            /// <param name="storeNumber">The store number.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="foundTransCount">The the number of found transactions.</param>
            /// <param name="transHeader">The transaction header data.</param>
            /// <param name="transItems">The transaction items data.</param>
            /// <param name="transLoyaltyCardNumber">The transaction loyalty card number.</param>
            public void GetTransaction(string receiptId, string storeNumber, string terminalId, ref int foundTransCount, ref TransactionHeader transHeader, ref TransactionItem[] transItems, ref string transLoyaltyCardNumber)
            {
                ThrowIf.NullOrWhiteSpace(receiptId, "receiptId");
                ThrowIf.Null(transHeader, "transHeader");
                ThrowIf.Null(transItems, "transItems");
    
                // Invoke the transaction service
                var data = this.InvokeMethod(
                    GetTransactionMethodName,
                    new object[] { receiptId, storeNumber, terminalId });
    
                GetTransactionDataFromResponse(data, ref foundTransCount, ref transHeader, ref transItems, ref transLoyaltyCardNumber);
            }
    
            /// <summary>
            /// Gets the transaction by transaction identifier.
            /// </summary>
            /// <param name="transactionId">The transaction identifier.</param>
            /// <param name="foundTransCount">The the number of found transactions.</param>
            /// <param name="transHeader">The transaction header data.</param>
            /// <param name="transItems">The transaction items data.</param>
            /// <param name="transLoyaltyCardNumber">The transaction loyalty card number.</param>
            public void GetTransactionByTransactionId(string transactionId, ref int foundTransCount, ref TransactionHeader transHeader, ref TransactionItem[] transItems, ref string transLoyaltyCardNumber)
            {
                ThrowIf.NullOrWhiteSpace(transactionId, "transactionId");
                ThrowIf.Null(transHeader, "transHeader");
                ThrowIf.Null(transItems, "transItems");
    
                // Invoke the transaction service
                var data = this.InvokeMethod(
                    GetTransactionByTransactionIdMethodName,
                    new object[] { transactionId });
    
                GetTransactionDataFromResponse(data, ref foundTransCount, ref transHeader, ref transItems, ref transLoyaltyCardNumber);
            }
    
            /// <summary>
            ///  Searches for transactions that match the given criteria in AX.
            /// </summary>
            /// <param name="currentChannelId">The current channel identifier.</param>
            /// <param name="criteria">Search criteria.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>A collection of transactions.</returns>
            public IEnumerable<Transaction> SearchJournalTransactions(long currentChannelId, TransactionSearchCriteria criteria, QueryResultSettings settings)
            {
                ThrowIf.Null(criteria, "criteria");
                ThrowIf.Null(settings, "settings");
    
                var headQuarterCritera = new AxTransactionSearchCriteria();
                headQuarterCritera.BarCode = criteria.Barcode;
                headQuarterCritera.ChannelReferenceId = criteria.ChannelReferenceId;
                headQuarterCritera.CurrentChannelId = currentChannelId;
                headQuarterCritera.CustomerAccountNumber = criteria.CustomerAccountNumber;
                headQuarterCritera.CustomerFirstName = criteria.CustomerFirstName;
                headQuarterCritera.CustomerLastName = criteria.CustomerLastName;
                headQuarterCritera.IncludeDetails = false;
                headQuarterCritera.ItemId = criteria.ItemId;
                headQuarterCritera.PagingInfo = settings.Paging;
                headQuarterCritera.ReceiptId = criteria.ReceiptId;
                headQuarterCritera.ReceiptEmailAddress = criteria.ReceiptEmailAddress;
                headQuarterCritera.SerialNumber = criteria.SerialNumber;
                headQuarterCritera.SalesId = criteria.SalesId;
                headQuarterCritera.StaffId = criteria.StaffId;
                headQuarterCritera.StoreId = criteria.StoreId;
                headQuarterCritera.TerminalId = criteria.TerminalId;
                headQuarterCritera.TransactionIds.AddRange(criteria.TransactionIds);
                headQuarterCritera.StartDateTime = criteria.StartDateTime.HasValue
                    ? criteria.StartDateTime.Value.UtcDateTime.ToString("s")
                    : string.Empty;
                headQuarterCritera.EndDateTime = criteria.EndDateTime.HasValue
                    ? criteria.EndDateTime.Value.UtcDateTime.ToString("s")
                    : string.Empty;
    
                try
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(SearchJournalTransactionsMethodName))
                    {
                        ReadOnlyCollection<object> transactionData = null;
    
                        transactionData = this.InvokeMethod(
                            SearchJournalTransactionsMethodName,
                            SerializationHelper.SerializeObjectToXml(headQuarterCritera));
    
                        // No matching orders were found.
                        if (transactionData == null)
                        {
                            return Enumerable.Empty<Transaction>();
                        }
    
                        if (transactionData.Count != 1)
                        {
                            throw new InvalidOperationException(
                                "TransactionServiceClient.SearchJournalTransactions returned an invalid result.");
                        }
    
                        // Parse transactions from results, the last value is the items for all transactions
                        string transactionsXml = (string)transactionData[0];
                        var salesTransactions =
                            SerializationHelper.DeserializeObjectDataContractFromXml<SalesTransaction[]>(transactionsXml);
    
                        var transactions = ConvertToTransactions(salesTransactions);
                        return transactions;
                    }
                }
                catch (Exception)
                {
                    if (!MethodsNotFoundInAx.Value.ContainsKey(SearchJournalTransactionsMethodName))
                    {
                        throw;
                    }
    
                    // Need to trigger fallback logic.
                }
    
                // Fallback to existing SearchOrders() call to meet SE deployment requirement.
                var orderSearchCriteria = new SalesOrderSearchCriteria();
                orderSearchCriteria.Barcode = criteria.Barcode;
                orderSearchCriteria.ChannelReferenceId = criteria.ChannelReferenceId;
                orderSearchCriteria.CustomerAccountNumber = criteria.CustomerAccountNumber;
                orderSearchCriteria.CustomerFirstName = criteria.CustomerFirstName;
                orderSearchCriteria.CustomerLastName = criteria.CustomerLastName;
                orderSearchCriteria.IncludeDetails = false;
                orderSearchCriteria.ItemId = criteria.ItemId;
                orderSearchCriteria.ReceiptId = criteria.ReceiptId;
                orderSearchCriteria.ReceiptEmailAddress = criteria.ReceiptEmailAddress;
                orderSearchCriteria.SerialNumber = criteria.SerialNumber;
                orderSearchCriteria.StaffId = criteria.StaffId;
                orderSearchCriteria.StoreId = criteria.StoreId;
                orderSearchCriteria.TerminalId = criteria.TerminalId;
                orderSearchCriteria.SalesTransactionTypeValues = new[]
                {
                    (int)TransactionType.BankDrop,
                    (int)TransactionType.Payment,
                    (int)TransactionType.TenderDeclaration,
                    (int)TransactionType.SalesOrder,
                    (int)TransactionType.SalesInvoice,
                    (int)TransactionType.BankDrop,
                    (int)TransactionType.SafeDrop,
                    (int)TransactionType.IncomeExpense,
                    (int)TransactionType.CustomerOrder
                };
    
                orderSearchCriteria.SearchType = OrderSearchType.SalesTransaction;
                orderSearchCriteria.TransactionStatusTypes = new[]
                {
                    TransactionStatus.Normal,
                    TransactionStatus.Posted
                };
                orderSearchCriteria.StartDateTime = criteria.StartDateTime;
                orderSearchCriteria.EndDateTime = criteria.EndDateTime;
                orderSearchCriteria.SalesId = criteria.SalesId;
    
                var maxNumberOfResults = settings.Paging.Top + settings.Paging.Skip;
                var orders = this.SearchOrders(orderSearchCriteria, maxNumberOfResults);
    
                return ConvertToTransactions(orders.Results);
            }
    
            /// <summary>
            ///  Searches for orders that match the given criteria in AX.
            /// </summary>
            /// <param name="criteria">Search criteria.</param>
            /// <param name="maxTransactionSearchResults">The max transaction search result count.</param>
            /// <returns>A sales transaction.</returns>
            public PagedResult<SalesOrder> SearchOrders(SalesOrderSearchCriteria criteria, long maxTransactionSearchResults)
            {
                ThrowIf.Null(criteria, "criteria");
    
                if (criteria.TransactionIds.HasMultiple())
                {
                    throw new NotSupportedException("Transaction service does not support retrieving order by multiple identifiers.");
                }
    
                // Trim parameters
                TrimParameters(criteria);

                // For cash & carry transaction we should exclude the sales order records.
                bool includeNonTransactions = (criteria.SearchType == OrderSearchType.ConsolidateOrder) // True, if search type is ConsolidateOrder.
                    || (criteria.SearchType == OrderSearchType.SalesOrder) // True, if search type is SalesOrder.
                    || (!criteria.TransactionIds.Any() && (!string.IsNullOrWhiteSpace(criteria.SalesId) || !string.IsNullOrWhiteSpace(criteria.ChannelReferenceId) || !string.IsNullOrWhiteSpace(criteria.ReceiptId))); // True, if only SalesId or ChannelReferenceId or ReceiptId is specified.

                // Results are returned back as (TransactionDetails1, TransactionDetails2, TransactionItems)
                ReadOnlyCollection<object> transactionData = this.InvokeMethod(
                    SearchOrdersMethodName,
                    criteria.TransactionIds.SingleOrDefault(),
                    criteria.SalesId,
                    criteria.ReceiptId,
                    criteria.ChannelReferenceId,
                    criteria.CustomerAccountNumber,
                    criteria.CustomerFirstName,
                    criteria.CustomerLastName,
                    criteria.StoreId,
                    criteria.TerminalId,
                    criteria.ItemId,
                    criteria.Barcode,
                    criteria.StaffId,
                    criteria.StartDateTime.HasValue ? criteria.StartDateTime.Value.UtcDateTime.ToString("s") : string.Empty,
                    criteria.EndDateTime.HasValue ? criteria.EndDateTime.Value.UtcDateTime.ToString("s") : string.Empty,
                    criteria.IncludeDetails,
                    criteria.ReceiptEmailAddress,
                    criteria.SearchIdentifiers,
                    maxTransactionSearchResults,
                    string.Join(",", criteria.SalesTransactionTypeValues.Select(p => (int)p)),
                    criteria.SerialNumber,
                    string.Join(",", criteria.TransactionStatusTypeValues),
                    includeNonTransactions);
    
                // No matching orders were found.
                if (transactionData == null)
                {
                    return Enumerable.Empty<SalesOrder>().AsPagedResult();
                }
    
                if (transactionData.Count != 1)
                {
                    throw new InvalidOperationException("TransactionServiceClient.SearchOrders returned an invalid result.");
                }
    
                // Parse transactions from results, the last value is the items for all transactions
                string transactionsXml = (string)transactionData[0];
                SalesOrder[] salesOrders = SerializationHelper.DeserializeObjectDataContractFromXml<SalesOrder[]>(transactionsXml);
    
                return salesOrders.OrderByDescending(x => x.CreatedDateTime).AsPagedResult();
            }
    
            /// <summary>
            /// Gets the shipments.
            /// </summary>
            /// <param name="salesId">The sales identifier.</param>
            /// <param name="shipmentId">The shipment identifier.</param>
            /// <returns>A collection of shipment.</returns>
            public PagedResult<Shipment> GetShipments(string salesId, string shipmentId)
            {
                ReadOnlyCollection<object> responseContainer = this.InvokeMethod(
                    "GetShipments", new string[] { salesId, shipmentId });
    
                // Deserialize.
                string xmlResponse = responseContainer[0] as string;
    
                if (string.IsNullOrWhiteSpace(xmlResponse))
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError, "Empty response data returned from service");
                }
    
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ShipmentsContainer));
                    using (StringReader stringReader = new StringReader(xmlResponse))
                    {
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.XmlResolver = null;
                        System.Xml.XmlReader reader = System.Xml.XmlReader.Create(stringReader, settings);
                        ShipmentsContainer shipmentsContainer =
                            (ShipmentsContainer)xmlSerializer.Deserialize(reader);
    
                        return new PagedResult<Shipment>(shipmentsContainer.Shipments.AsReadOnly());
                    }
                }
                catch (Exception ex)
                {
                    throw new CRT.CommunicationException(
                        CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterResponseParsingError,
                        ex,
                        string.Format("Unable to parse service response data: {0}", xmlResponse));
                }
            }
    
            /// <summary>
            /// Parses the service response into transaction data.
            /// </summary>
            /// <param name="data">The collection of transaction data.</param>
            /// <param name="foundTransCount">The the number of found transactions.</param>
            /// <param name="transHeader">The transaction header data.</param>
            /// <param name="transItems">The transaction items data.</param>
            /// <param name="transLoyaltyCardNumber">The transaction loyalty card number.</param>
            private static void GetTransactionDataFromResponse(ReadOnlyCollection<object> data, ref int foundTransCount, ref TransactionHeader transHeader, ref TransactionItem[] transItems, ref string transLoyaltyCardNumber)
            {
                // Parse response data
                foundTransCount = Convert.ToInt32(data[0]);
    
                // The response contract on success case:
                // Data[0] - number of transaction found,
                // Data[1] - transaction header
                // Data[2] - transaction items
                // Data[3] - loyalty card number ("" if not available)
                if (foundTransCount == 1 &&
                    data.Count == 4)
                {
                    // Parse transaction header
                    transHeader = TransactionHeader.Deserialize((string)data[1]);
    
                    // Parse transaction items
                    transItems = SerializationHelper.DeserializeObjectFromXml<TransactionItem[]>((string)data[2]);
    
                    // Convert amount and quantity signs in the transaction items
                    foreach (var transItem in transItems)
                    {
                        transItem.NetPrice = transItem.NetPrice * -1;
                        transItem.NetAmount = transItem.NetAmount * -1;
                        transItem.NetAmountInclusiveTax = transItem.NetAmountInclusiveTax * -1;
                        transItem.TaxAmount = transItem.TaxAmount * -1;
                        transItem.Quantity = transItem.Quantity * -1;
                        transItem.ReturnQuantity = transItem.ReturnQuantity * -1;
                    }
    
                    // Parse loyalty card number
                    transLoyaltyCardNumber = (string)data[3];
                }
            }
    
            /// <summary>
            /// Trims the sales order criteria if requires.
            /// </summary>
            /// <param name="criteria">The sales order criteria to trim.</param>
            private static void TrimParameters(SalesOrderSearchCriteria criteria)
            {
                if (!string.IsNullOrWhiteSpace(criteria.SalesId))
                {
                    criteria.SalesId = criteria.SalesId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.ReceiptId))
                {
                    criteria.ReceiptId = criteria.ReceiptId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.ChannelReferenceId))
                {
                    criteria.ChannelReferenceId = criteria.ChannelReferenceId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.CustomerAccountNumber))
                {
                    criteria.CustomerAccountNumber = criteria.CustomerAccountNumber.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.CustomerFirstName))
                {
                    criteria.CustomerFirstName = criteria.CustomerFirstName.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.CustomerLastName))
                {
                    criteria.CustomerLastName = criteria.CustomerLastName.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.StoreId))
                {
                    criteria.StoreId = criteria.StoreId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.TerminalId))
                {
                    criteria.TerminalId = criteria.TerminalId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.ItemId))
                {
                    criteria.ItemId = criteria.ItemId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.Barcode))
                {
                    criteria.Barcode = criteria.Barcode.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.StaffId))
                {
                    criteria.StaffId = criteria.StaffId.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.ReceiptEmailAddress))
                {
                    criteria.ReceiptEmailAddress = criteria.ReceiptEmailAddress.Trim();
                }
    
                if (!string.IsNullOrWhiteSpace(criteria.SearchIdentifiers))
                {
                    criteria.SearchIdentifiers = criteria.SearchIdentifiers.Trim();
                }
            }
    
            private static List<Transaction> ConvertToTransactions(IEnumerable<SalesTransaction> salesTransactions)
            {
                var transactions = new List<Transaction>();
                foreach (var salesTransaction in salesTransactions)
                {
                    var transaction = new Transaction();
                    transaction.ChannelCurrencyExchangeRate = salesTransaction.ChannelCurrencyExchangeRate;
                    transaction.CreatedDateTime = salesTransaction.CreatedDateTime;
                    transaction.Description = salesTransaction.Comment;
                    transaction.Id = salesTransaction.Id;
                    transaction.GrossAmount = salesTransaction.GrossAmount;
                    transaction.ReceiptId = salesTransaction.ReceiptId;
                    transaction.ShiftId = salesTransaction.ShiftId.ToString();
                    transaction.ShiftTerminalId = salesTransaction.ShiftTerminalId;
                    transaction.StaffId = salesTransaction.StaffId;
                    transaction.StoreId = salesTransaction.StoreId;
                    transaction.TerminalId = salesTransaction.TerminalId;
                    transaction.TotalAmount = salesTransaction.TotalAmount;
                    transaction.TransactionTypeValue = salesTransaction.TransactionTypeValue;
    
                    transactions.Add(transaction);
                }
    
                return transactions;
            }
        }
    }
}
