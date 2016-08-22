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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        internal class StockCountJournalManager : IStockCountJournalManager
        {
            public Task<StockCountJournal> Create(StockCountJournal entity)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).CreateOrUpdateStockCountJournal(entity));
            }
    
            public Task<StockCountJournal> Read(string journalId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetStockCountJournalById(journalId));
            }
    
            public Task<PagedResult<StockCountJournal>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetStockCountJournals(queryResultSettings));
            }
    
            public Task<StockCountJournal> Update(StockCountJournal entity)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).CreateOrUpdateStockCountJournal(entity));
            }
    
            public Task Delete(StockCountJournal entity)
            {
                // Should use RemoveJournal() call rather than this call.
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<StockCountJournal>> Sync(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).SyncStockCountJournal(queryResultSettings));
            }
    
            public Task<PagedResult<StockCountJournalTransaction>> SyncTransactions(string journalId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).SyncStockCountJournalTransactions(journalId, queryResultSettings));
            }
    
            public Task RemoveJournal(string journalId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).DeleteStockCountJournal(journalId));
            }
    
            public Task RemoveTransaction(string journalId, string itemId, string inventSizeId, string inventColorId, string inventStyleId, string configId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).DeleteStockCountJournalTransactions(journalId, itemId, inventSizeId, inventColorId, inventStyleId, configId));
            }
    
            public Task Commit(string journalId)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).CommitStockCountJournalTransactions(journalId));
            }
        }
    }
}
