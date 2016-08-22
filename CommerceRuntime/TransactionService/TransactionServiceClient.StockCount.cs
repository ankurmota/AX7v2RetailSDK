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
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Transaction Service Commerce Runtime Client API.
        /// </summary>
        public sealed partial class TransactionServiceClient
        {
            // Transaction service method names.
            private const string GetInventJournalsMethodName = "GetInventJournals";
            private const string GetInventJournalTransactionMethodName = "GetInventJournal";
            private const string UpdateInventJournalsMethodName = "UpdateInventoryJournal";
            private const string CreateInventoryJournalMethodName = "CreateInventoryJournal";
            private const string DeleteInventJournalMethodName = "DeleteInventoryJournal";
    
            /// <summary>
            /// Commits the Stock count journals and transactions to AX.
            /// </summary>
            /// <param name="stockCountJournal">Pass in stock count journal.</param>
            /// <returns>Returns the stock count journal created.</returns>
            public StockCountJournal CommitStockCounts(StockCountJournal stockCountJournal)
            {
                ThrowIf.Null<StockCountJournal>(stockCountJournal, "stockCountJournal");
    
                // Filter stock count line that has status pending updates.
                stockCountJournal.StockCountTransactionLines = stockCountJournal.StockCountTransactionLines
                    .Where(stockCountLine => stockCountLine.StatusEnum == StockCountStatus.PendingUpdate).ToList();

                string importValue = TransactionServiceClient.ToXml(stockCountJournal);
    
                var data = this.InvokeMethod(UpdateInventJournalsMethodName, importValue);
    
                var journalTransactions = new List<StockCountJournalTransaction>();
    
                if (!string.IsNullOrWhiteSpace(data[0].ToString()))
                {
                    XDocument doc = XDocument.Parse(data[0].ToString());
                    XElement root = doc.Elements("InventJournalTable").FirstOrDefault();
                    if (root != null)
                    {
                        journalTransactions = root.Elements("InventJournalTrans").Select<XElement, StockCountJournalTransaction>(
                            (scLine) =>
                            {
                                var scJournalTransaction = TransactionServiceClient.ParseTransactions(scLine);
    
                                return scJournalTransaction;
                            }).ToList<StockCountJournalTransaction>();
                    }
                }
    
                stockCountJournal.StockCountTransactionLines.Clear();
                stockCountJournal.StockCountTransactionLines.AddRange(journalTransactions);
    
                return stockCountJournal;
            }
    
            /// <summary>
            /// Retrieves the Stock Count Journals from AX.
            /// </summary>
            /// <param name="inventLocationId">Pass Invent Location Id.</param>
            /// <returns>Returns the StockCountJournal list from AX.</returns>
            public PagedResult<StockCountJournal> GetStockCountJournals(string inventLocationId)
            {
                ThrowIf.Null<string>(inventLocationId, "inventoryLocationId");
    
                var data = this.InvokeMethod(GetInventJournalsMethodName, inventLocationId);
    
                IEnumerable<StockCountJournal> journals = new List<StockCountJournal>();
    
                if (!string.IsNullOrEmpty(data[0].ToString()))
                {
                    XDocument doc = XDocument.Parse(data[0].ToString());
                    XElement root = doc.Elements("InventJournalTables").FirstOrDefault();
    
                    if (root != null)
                    {
                        journals = root.Elements("InventJournalTable").Select<XElement, StockCountJournal>(
                            (sc) =>
                            {
                                StockCountJournal scJournal = TransactionServiceClient.Parse(sc);
    
                                return scJournal;
                            });
                    }
                }
    
                return journals.AsPagedResult();
            }
    
            /// <summary>
            /// Retrieves Stock Count journal Transactions from AX.
            /// </summary>
            /// <param name="journalId">Pass JournalId.</param>
            /// <param name="inventLocationId">Pass Invent Location Id.</param>
            /// <returns>Returns the StockCount journal Transactions.</returns>
            public PagedResult<StockCountJournalTransaction> GetStockCountJournalsTransaction(string journalId, string inventLocationId)
            {
                ThrowIf.Null<string>(journalId, "journalId");
                ThrowIf.Null<string>(inventLocationId, "inventoryLocationId");
    
                var data = this.InvokeMethod(GetInventJournalTransactionMethodName, journalId, inventLocationId);
    
                IEnumerable<StockCountJournalTransaction> journalTransactions = new List<StockCountJournalTransaction>();
    
                if (!string.IsNullOrWhiteSpace(data[0].ToString()))
                {
                    XDocument doc = XDocument.Parse(data[0].ToString());
                    XElement root = doc.Elements("InventJournalTable").FirstOrDefault();
    
                    if (root != null)
                    {
                        journalTransactions = root.Elements("InventJournalTrans").Select<XElement, StockCountJournalTransaction>(
                                (scLine) =>
                                {
                                    StockCountJournalTransaction scJournalTransaction = TransactionServiceClient.ParseTransactions(scLine);
    
                                    return scJournalTransaction;
                                }).ToList<StockCountJournalTransaction>();
                    }
                }
    
                return journalTransactions.AsPagedResult();
            }
    
            /// <summary>
            /// Create a stock count journal.
            /// </summary>
            /// <param name="inventoryLocationId">Pass Invent Location Id.</param>
            /// <param name="description">Pass journal's description.</param>
            /// <returns>Returns the StockCountJournal list from AX.</returns>
            public PagedResult<StockCountJournal> CreateStockCountJournal(string inventoryLocationId, string description)
            {
                var data = this.InvokeMethod(CreateInventoryJournalMethodName, inventoryLocationId, description);
    
                IEnumerable<StockCountJournal> journals = new List<StockCountJournal>();
    
                if (!string.IsNullOrEmpty(data[0].ToString()))
                {
                    XDocument doc = XDocument.Parse(data[0].ToString());
    
                    if (doc != null)
                    {
                        journals = doc.Elements("InventJournalTable").Select<XElement, StockCountJournal>(
                            (sc) =>
                            {
                                StockCountJournal scJournal = TransactionServiceClient.Parse(sc);
    
                                return scJournal;
                            });
                    }
                }
    
                return journals.AsPagedResult();
            }
    
            /// <summary>
            /// Deletes stock count journal record and transactions from AX of the specified journalId.
            /// </summary>
            /// <param name="journalId">The stock count journal record identifier.</param>
            public void DeleteStockJournal(string journalId)
            {
                ThrowIf.Null(journalId, "journalId");
    
                this.InvokeMethodNoDataReturn(DeleteInventJournalMethodName, journalId);
            }
    
            /// <summary>
            /// Parse StockCount journal xml data into object.
            /// </summary>
            /// <param name="xmlJournal">Xml format of a stock count journal.</param>
            /// <returns>Returns the StockCountJournal object.</returns>
            private static StockCountJournal Parse(XElement xmlJournal)
            {
                var journal = new StockCountJournal();
    
                journal.RecordId = TransactionServiceClient.GetAttributeValue(xmlJournal, "RecId");
                journal.JournalId = TransactionServiceClient.GetAttributeValue(xmlJournal, "JournalId");
                journal.Description = TransactionServiceClient.GetAttributeValue(xmlJournal, "Description");
    
                return journal;
            }
    
            /// <summary>
            /// Parse xml StockCount journal transaction data into object.
            /// </summary>
            /// <param name="xmlJournalTransaction">Xml format of a stock count journal Transaction.</param>
            /// <returns>Returns the StockCountJournalTransactions object.</returns>
            private static StockCountJournalTransaction ParseTransactions(XElement xmlJournalTransaction)
            {
                var journalTransaction = new StockCountJournalTransaction();
                string recId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "RecId");
    
                // Set default values for those not existing in AX
                journalTransaction.RecordId = string.IsNullOrEmpty(recId) ? 0 : long.Parse(recId);
                journalTransaction.OperationType = 0;
                journalTransaction.Quantity = 0;
                journalTransaction.CountedDate = DateTime.UtcNow;
                journalTransaction.StatusEnum = StockCountStatus.Unchanged;
                journalTransaction.ItemId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "ItemId");
                journalTransaction.ItemName = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "EcoResProductName");
                journalTransaction.InventDimId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventDimId");
                journalTransaction.Counted = Convert.ToDecimal(TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "Counted"));
                journalTransaction.InventBatchId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventBatchId");
                journalTransaction.WarehouseLocationId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "WmsLocationId");
                journalTransaction.WarehousePalletId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "WmsPalletId");
                journalTransaction.InventSiteId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventSiteId");
                journalTransaction.InventLocationId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventLocationId");
                journalTransaction.ConfigId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "ConfigId");
                journalTransaction.InventSizeId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventSizeId");
                journalTransaction.InventColorId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventColorId");
                journalTransaction.InventStyleId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventStyleId");
                journalTransaction.InventSerialId = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "InventSerialId");
                journalTransaction.TrackingGuid = new Guid(TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "Guid"));
                journalTransaction.UpdatedInAx = Convert.ToBoolean(TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "UpdatedInAx"));
                journalTransaction.Message = TransactionServiceClient.GetAttributeValue(xmlJournalTransaction, "Message");
    
                return journalTransaction;
            }
    
            /// <summary>
            /// Get attribute value for the given Xml string and attribute name.
            /// </summary>
            /// <param name="xmlElement">Xml string element.</param>
            /// <param name="attributeName">Name of the attribute to find the value.</param>
            /// <returns>Returns the Attribute value.</returns>
            private static string GetAttributeValue(XElement xmlElement, string attributeName)
            {
                ThrowIf.Null(xmlElement, "xmlElement");
    
                string result = string.Empty;
    
                XAttribute attribute = xmlElement.Attribute(attributeName);
                if (attribute != null)
                {
                    result = attribute.Value;
                }
    
                return result;
            }
    
            /// <summary>
            /// Serialize to xml format of a stock count journal.
            /// </summary>
            /// <param name="journal">Enter the StockCountJournal object.</param>
            /// <returns>Xml format of a stock count journal.</returns>
            private static string ToXml(StockCountJournal journal)
            {
                ThrowIf.Null(journal, "journal");
    
                StringBuilder strOutput = new StringBuilder();
    
                strOutput.Append("<InventJournalTable");
                strOutput.Append(" JournalId=\"" + journal.JournalId + "\"");
                strOutput.Append(" RecId=\"" + journal.RecordId + "\"");
                strOutput.Append(" Worker=\"" + journal.Worker + "\"");
                strOutput.Append(">");
    
                foreach (var line in journal.StockCountTransactionLines)
                {
                    strOutput.Append(TransactionServiceClient.ToXml(line));
                }
    
                strOutput.Append("</InventJournalTable>");
    
                return strOutput.ToString();
            }
    
            /// <summary>
            /// Serialize to xml format of a SC journal transaction.
            /// </summary>
            /// <param name="journalTransaction">Enter the StockCountJournal transaction object.</param>
            /// <returns>Xml format of a SC journal transaction.</returns>
            private static string ToXml(StockCountJournalTransaction journalTransaction)
            {
                ThrowIf.Null(journalTransaction, "journalTransaction");
    
                XElement inventJournalTrans = new XElement(
                        "InventJournalTrans",
                        new XAttribute("RecId", journalTransaction.RecordId),
                        new XAttribute("ItemId", journalTransaction.ItemId ?? string.Empty),
                        new XAttribute("EcoResProductName", journalTransaction.ItemName ?? string.Empty),
                        new XAttribute("InventDimId", journalTransaction.InventDimId ?? string.Empty),
                        new XAttribute("Counted", journalTransaction.Quantity),
                        new XAttribute("InventBatchId", journalTransaction.InventBatchId ?? string.Empty),
                        new XAttribute("WmsLocationId", journalTransaction.WarehouseLocationId ?? string.Empty),
                        new XAttribute("WmsPalletId", journalTransaction.WarehousePalletId ?? string.Empty),
                        new XAttribute("InventSiteId", journalTransaction.InventSiteId ?? string.Empty),
                        new XAttribute("InventLocationId", journalTransaction.InventLocationId ?? string.Empty),
                        new XAttribute("ConfigId", journalTransaction.ConfigId ?? string.Empty),
                        new XAttribute("InventSizeId", journalTransaction.InventSizeId ?? string.Empty),
                        new XAttribute("InventColorId", journalTransaction.InventColorId ?? string.Empty),
                        new XAttribute("InventStyleId", journalTransaction.InventStyleId ?? string.Empty),
                        new XAttribute("InventSerialId", journalTransaction.InventSerialId ?? string.Empty),
                        new XAttribute("Guid", journalTransaction.TrackingGuid));
    
                return inventJournalTrans.ToString();
            }
        }
    }
}
