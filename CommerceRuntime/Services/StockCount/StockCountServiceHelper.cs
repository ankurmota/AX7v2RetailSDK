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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the stock count functionality for StockCountService class.
        /// </summary>
        internal sealed class StockCountServiceHelper
        {
            /// <summary>
            /// Prevents a default instance of the <see cref="StockCountServiceHelper" /> class from being created.
            /// </summary>
            private StockCountServiceHelper()
            {
            }
    
            /// <summary>
            /// Creates the specified context.
            /// </summary>
            /// <returns>The service instance.</returns>
            public static StockCountServiceHelper Create()
            {
                return new StockCountServiceHelper();
            }
    
            /// <summary>
            /// Creates the StockCount Journal in AX
            /// If the StockCount creation is successful, it syncs the changes from AX to RetailServer tables.
            /// It deletes all the journals that are saved locally.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="description">The journal description.</param>
            /// <returns>
            /// The stock count journal response.
            /// </returns>
            public static CreateStockCountJournalServiceResponse CreateStockCountJournal(RequestContext context, string description)
            {
                var channelConfiguration = context.GetChannelConfiguration();
    
                var createStockCountJournalRealtimeRequest = new CreateStockCountJournalRealtimeRequest(channelConfiguration.InventLocation, description);
                var createdStockCountJournal = context.Execute<EntityDataServiceResponse<StockCountJournal>>(createStockCountJournalRealtimeRequest).PagedEntityCollection.Results;
    
                var stockCountJournalsRequest = new GetStockCountJournalsRealtimeRequest(channelConfiguration.InventLocation);
                var stockCountJournalList = context.Execute<EntityDataServiceResponse<StockCountJournal>>(stockCountJournalsRequest).PagedEntityCollection.Results;
    
                AddUpdateStockCountJournal(context, stockCountJournalList);
    
                var stockCountListToDelete = GetUnusedJournals(context, stockCountJournalList);
    
                // Deletes the unused journals that are saved locally in RetailServer
                if (stockCountListToDelete != null && stockCountListToDelete.Count > 0)
                {
                    var deleteRequest = new DeleteStockCountJournalsDataRequest(stockCountListToDelete.Select(journal => journal.JournalId));
                    context.Runtime.Execute<NullResponse>(deleteRequest, context);
                }
    
                return new CreateStockCountJournalServiceResponse(createdStockCountJournal.FirstOrDefault());
            }
    
            /// <summary>
            /// Deletes the stock count journal in channel and AX databases.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <returns>
            /// An empty response object.
            /// </returns>
            public static DeleteStockCountServiceResponse DeleteStockCountJournal(RequestContext context, string journalId)
            {
                // Delete the specified stock count journal as well as its journal transation(s) in AX database
                var deleteJournalRealtimeRequest = new DeleteStockCountJournalRealtimeRequest(journalId);
                context.Execute<NullResponse>(deleteJournalRealtimeRequest);
    
                // Delete the specified stock count journal as well as its journal transation(s) in channel database
                var deleteDataRequest = new DeleteStockCountJournalsDataRequest(journalId);
                context.Execute<NullResponse>(deleteDataRequest);
    
                return new DeleteStockCountServiceResponse();
            }
    
            /// <summary>
            /// Deletes the Stock count journal Transaction for the given journal and ItemID.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <param name="itemId">The item identifier.</param>
            /// <param name="inventSizeId">Enter the inventory size identifier.</param>
            /// <param name="inventColorId">Enter the inventory color identifier.</param>
            /// <param name="inventStyleId">Enter the inventory style identifier.</param>
            /// <param name="configId">Enter the inventory configuration.</param>
            /// <returns>
            /// An empty response object.
            /// </returns>
            public static DeleteStockCountServiceResponse DeleteStockCountJournalTransaction(RequestContext context, string journalId, string itemId, string inventSizeId, string inventColorId, string inventStyleId, string configId)
            {
                var deleteRequest = new DeleteStockCountTransactionDataRequest(journalId, itemId, inventSizeId, inventColorId, inventStyleId, configId);
                context.Runtime.Execute<NullResponse>(deleteRequest, context);
    
                return new DeleteStockCountServiceResponse();
            }
    
            /// <summary>
            /// Saves the stock count journal transactions.
            /// </summary>
            /// <param name="journalId">The stock count journal identifier.</param>
            /// <param name="context">The request context.</param>
            /// <param name="transactions">A collection of stock count journal transactions.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public static SaveStockCountJournalTransactionServiceResponse SaveStockCountJournalTransactions(string journalId, RequestContext context, IEnumerable<StockCountJournalTransaction> transactions)
            {
                AddUpdateStockCount(journalId, context, transactions);
    
                // Refresh the stock count lines from database.
                var stockCountJournalTransactionResponse = GetStockCountJournalTransactions(context, journalId);
    
                var stockCountJournalResponse = new StockCountJournal { JournalId = journalId, StockCountTransactionLines = stockCountJournalTransactionResponse.StockCountJournalTransactions.Results };
    
                return new SaveStockCountJournalTransactionServiceResponse(stockCountJournalResponse);
            }
    
            /// <summary>
            /// Retrieves the stock count journal.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// A collection of stock count journals.
            /// </returns>
            public static GetStockCountJournalServiceResponse GetStockCountJournal(GetStockCountJournalServiceRequest request)
            {
                var dataRequest = new GetStockCountDataRequest
                {
                    QueryResultSettings = request.QueryResultSettings
                };
    
                if (!string.IsNullOrWhiteSpace(request.JournalId))
                {
                    dataRequest.JournalId = request.JournalId;
                }
    
                PagedResult<StockCountJournal> journals = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<StockCountJournal>>(dataRequest, request.RequestContext).PagedEntityCollection;
    
                return new GetStockCountJournalServiceResponse(journals);
            }
    
            /// <summary>
            /// Retrieves the stock count journal transactions.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public static GetStockCountJournalTransactionServiceResponse GetStockCountJournalTransactions(RequestContext context, string journalId)
            {
                var dataRequest = new GetStockCountDataRequest
                {
                    JournalId = journalId,
                    TransactionRecordsOnly = true,
                    QueryResultSettings = QueryResultSettings.AllRecords
                };
    
                var journalTransactions = context.Runtime.Execute<EntityDataServiceResponse<StockCountJournalTransaction>>(dataRequest, context).PagedEntityCollection;
    
                return new GetStockCountJournalTransactionServiceResponse(journalTransactions);
            }
    
            /// <summary>
            /// Syncs StockCount journal from AX.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The response.</returns>
            public static SyncStockCountJournalsFromAxServiceResponse SyncStockCountJournalFromAx(RequestContext context)
            {
                var stockCountJournals = GetStockCountJournalsFromAx(context);
    
                // Remove stock count journals that are not available on AX anymore.
                var journalsToRemoved = GetUnusedJournals(context, stockCountJournals);
                var dataRequest = new DeleteStockCountJournalsDataRequest(journalsToRemoved.Select(journal => journal.JournalId));
                context.Runtime.Execute<NullResponse>(dataRequest, context);
    
                AddUpdateStockCountJournal(context, stockCountJournals);
    
                // Get all the stock count journals from RetailServer database and send the dataset to client.
                var stockDataRequest = new GetStockCountDataRequest { QueryResultSettings = QueryResultSettings.AllRecords };
                var currentJournals = context.Runtime.Execute<EntityDataServiceResponse<StockCountJournal>>(stockDataRequest, context).PagedEntityCollection;
    
                return new SyncStockCountJournalsFromAxServiceResponse(currentJournals);
            }
    
            /// <summary>
            /// Sync Stock count journal transactions from AX.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <returns>The response.</returns>
            public static SyncStockCountTransactionsFromAxServiceResponse SyncStockCountJournalTransactions(RequestContext context, string journalId)
            {
                var currJournalTrans = GetStockCountJournalTransFromAx(context, journalId);
    
                AddUpdateStockCount(journalId, context, currJournalTrans);
    
                var dataRequest = new GetStockCountDataRequest
                {
                    TransactionRecordsOnly = true,
                    JournalId = journalId,
                    QueryResultSettings = QueryResultSettings.AllRecords
                };
    
                var currentScDbTransactions = context.Runtime.Execute<EntityDataServiceResponse<StockCountJournalTransaction>>(dataRequest, context).PagedEntityCollection;
    
                return new SyncStockCountTransactionsFromAxServiceResponse(currentScDbTransactions);
            }
    
            /// <summary>
            /// Commits the StockCount transactions to AX.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <param name="transactions">A collection of stock count transactions to commit to AX.</param>
            /// <returns>The response object.</returns>
            public static CommitStockCountTransactionsServiceResponse CommitStockCountTransactions(RequestContext context, string journalId, IEnumerable<StockCountJournalTransaction> transactions)
            {
                // Save the StockCount transactions into DB before commiting to AX.
                AddUpdateStockCount(journalId, context, transactions);
    
                var stockCountJournal = ConstructAxStockJournal(context, journalId);
                var commitStockCountsRequest = new CommitStockCountJournalRealtimeRequest(stockCountJournal);
                StockCountJournal commitedStockCounts = context.Execute<SingleEntityDataServiceResponse<StockCountJournal>>(commitStockCountsRequest).Entity;
    
                var removeStockCountTrans = new List<StockCountJournalTransaction>();
    
                if (commitedStockCounts != null)
                {
                    foreach (var transactionLine in stockCountJournal.StockCountTransactionLines)
                    {
                        StockCountJournalTransaction updatedLine = commitedStockCounts.StockCountTransactionLines
                            .FirstOrDefault(
                                line => string.Equals(
                                    line.TrackingGuid.ToString(),
                                    transactionLine.TrackingGuid.ToString(),
                                    StringComparison.OrdinalIgnoreCase)
                                    && line.UpdatedInAx);
    
                        if (updatedLine != null)
                        {
                            removeStockCountTrans.Add(updatedLine);
                        }
                    }
                }
    
                // This deletes both the journals and transactions.
                var deleteDataRequest = new DeleteStockCountJournalsDataRequest(commitedStockCounts.JournalId);
                context.Runtime.Execute<NullResponse>(deleteDataRequest, context);
    
                return new CommitStockCountTransactionsServiceResponse();
            }
    
            /// <summary>
            /// Gets the unused journals.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="activeList">The active list.</param>
            /// <returns>The stock count journals.</returns>
            private static ReadOnlyCollection<StockCountJournal> GetUnusedJournals(RequestContext context, IEnumerable<StockCountJournal> activeList)
            {
                var dataRequest = new GetStockCountDataRequest { QueryResultSettings = QueryResultSettings.AllRecords };
                var currentJournals = context.Runtime.Execute<EntityDataServiceResponse<StockCountJournal>>(dataRequest, context).PagedEntityCollection.Results;
    
                IList<StockCountJournal> unusedList = new List<StockCountJournal>();
    
                foreach (var currScj in currentJournals)
                {
                    // check whether the current journal is present in active journal
                    var stockCountJournalExists = activeList.Where(active => string.Equals(active.JournalId, currScj.JournalId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    
                    // if the current journal does not exist in active journal delete them
                    if (stockCountJournalExists == null)
                    {
                        unusedList.Add(currScj);
                    }
                }
    
                return new ReadOnlyCollection<StockCountJournal>(unusedList);
            }
    
            /// <summary>
            /// Creates the <see cref="StockCountJournal"/> object to commit to AX.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journalId">The journal identifier.</param>
            /// <returns>
            /// The stock count journal with transaction lines to be committed to AX.
            /// </returns>
            private static StockCountJournal ConstructAxStockJournal(RequestContext context, string journalId)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
    
                var dataRequest = new GetStockCountDataRequest
                {
                    TransactionRecordsOnly = true,
                    JournalId = journalId,
                    QueryResultSettings = QueryResultSettings.AllRecords
                };
    
                var currentScDbTransactions = context.Runtime.Execute<EntityDataServiceResponse<StockCountJournalTransaction>>(dataRequest, context).PagedEntityCollection.Results;
    
                StockCountJournal stockCountJournal = new StockCountJournal();
                stockCountJournal.JournalId = journalId;
    
                foreach (var currScDbTran in currentScDbTransactions)
                {
                    StockCountJournalTransaction journalTransaction = new StockCountJournalTransaction();
    
                    journalTransaction.RecordId = currScDbTran.RecordId;
                    journalTransaction.ItemId = currScDbTran.ItemId;
                    journalTransaction.ItemName = currScDbTran.ItemName;
                    journalTransaction.Counted = currScDbTran.Counted;
                    journalTransaction.Quantity = currScDbTran.Quantity;
                    journalTransaction.ConfigId = currScDbTran.ConfigId;
                    journalTransaction.TrackingGuid = currScDbTran.TrackingGuid;
                    journalTransaction.InventColorId = currScDbTran.InventColorId;
                    journalTransaction.InventSizeId = currScDbTran.InventSizeId;
                    journalTransaction.InventStyleId = currScDbTran.InventStyleId;
                    journalTransaction.InventDimId = currScDbTran.InventDimId;
                    journalTransaction.InventSiteId = currScDbTran.InventSiteId;
                    journalTransaction.InventLocationId = channelConfiguration.InventLocation;
                    journalTransaction.Status = currScDbTran.Status;
    
                    stockCountJournal.StockCountTransactionLines.Add(journalTransaction);
                }
    
                return stockCountJournal;
            }
    
            private static ReadOnlyCollection<StockCountJournalTransaction> GetStockCountJournalTransFromAx(RequestContext context, string journalId)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
    
                var journalTransactionsRequest = new GetStockCountJournalTransactionsRealtimeRequest(journalId, channelConfiguration.InventLocation);
                ReadOnlyCollection<StockCountJournalTransaction> journalTransactions = context.Execute<EntityDataServiceResponse<StockCountJournalTransaction>>(journalTransactionsRequest).PagedEntityCollection.Results;
    
                return journalTransactions;
            }
    
            /// <summary>
            /// Adds or updates the stock count.
            /// </summary>
            /// <param name="journalId">The stock count journal identifier.</param>
            /// <param name="context">The request context.</param>
            /// <param name="transactions">A collection of stock count journal transactions.</param>
            private static void AddUpdateStockCount(string journalId, RequestContext context, IEnumerable<StockCountJournalTransaction> transactions)
            {
                foreach (var stockCountJournalTransaction in transactions)
                {
                    stockCountJournalTransaction.UpdateJournalId(journalId);
                }
    
                var dataRequest = new CreateUpdateStockCountJournalTransactionDataRequest(transactions);
                context.Runtime.Execute<NullResponse>(dataRequest, context);
            }
    
            /// <summary>
            /// Add or updates the stock journal.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="journals">A collection of stock count journals.</param>
            private static void AddUpdateStockCountJournal(RequestContext context, IEnumerable<StockCountJournal> journals)
            {
                var dataRequest = new CreateUpdateStockCountJournalDataRequest(journals);
                context.Runtime.Execute<NullResponse>(dataRequest, context);
            }
    
            private static ReadOnlyCollection<StockCountJournal> GetStockCountJournalsFromAx(RequestContext context)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
    
                var stockCountJournalsRequest = new GetStockCountJournalsRealtimeRequest(channelConfiguration.InventLocation);
    
                return context.Execute<EntityDataServiceResponse<StockCountJournal>>(stockCountJournalsRequest).PagedEntityCollection.Results;
            }
        }
    }
}
