/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Model.Managers {
    "use strict";

    
    export var IStockCountJournalManagerName: string = "IStockCountJournalManager";
    

    export interface IStockCountJournalManager {
        /**
         * Create a stock count journal.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be created.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        createStockCountJournalAsync(stockCountJournal: Entities.StockCountJournal): IAsyncResult<Entities.StockCountJournal>;

        /**
         * Update a stock count journal.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be updated.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        updateStockCountJournalAsync(stockCountJournal: Entities.StockCountJournal): IAsyncResult<Entities.StockCountJournal>;

        /**
         * Commit a stock count journal. This action will make a journal not searchable until we do operation sync all stock count journals.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        commitStockCountJournalAsync(journalId: string): IVoidAsyncResult;

        /**
         * Sync back committed stock count journal and its product lines (StockCountJournalTransactions).
         * This method is being used when synchronizing StockCountJournalTransactions.
         * @param {string} journalId The journal identifier to be synch-ed with.
         * @return {IAsyncResult<Entities.StockCountJournalTransaction[]>} The async result.
         */
        syncStockCountJournalAsync(journalId: string): IAsyncResult<Entities.StockCountJournalTransaction[]>;

        /**
         * Get stock count journal details.
         * @param {string} journalId The journal identifier.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        getStockCountJournalDetailsAsync(journalId: string): IAsyncResult<Entities.StockCountJournal>;

        /**
         * Get the list of all stock count journals.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of journals to skip.
         * @return {IAsyncResult<Entities.StockCountJournal[]>} The async result.
         */
        getStockCountJournalsAsync(pageSize?: number, skip?: number): IAsyncResult<Entities.StockCountJournal[]>;

        /**
         * Sync all committed journals. All committed journals will be sync-ed, except StockCountJournalTransactions property.
         * Synchronize stock count journal individually will bring back StockCountJournalTransactions property.
         * @return {IAsyncResult<Entities.StockCountJournal[]>} The async result.
         */
        syncAllStockCountJournalsAsync(): IAsyncResult<Entities.StockCountJournal[]>;

        /**
         * Delete stock count journals.
         * @param {string[]} journalIds The stock count journal identifiers to be removed.
         * @return {IVoidAsyncResult} The async result.
         */
        deleteStockCountJournalsAsync(journalIds: string[]): IVoidAsyncResult;

        /**
         * Delete a stock count journal transaction (product line).
         * @param {string} journalId The stock count journal identifier.
         * @param {string} itemId The item identifier to be deleted.
         * @param {string} colorId The color identifier of the item to be deleted.
         * @param {string} configurationId The configuration identifier of the item to be deleted.
         * @param {string} sizeId The size identifier of the item to be deleted.
         * @param {string} styleId The style identifier of the item to be deleted.
         * @return {IVoidAsyncResult} The async result.
         */
        deleteStockCountJournalTransactionAsync(
            journalId: string,
            itemId: string,
            colorId: string,
            configurationId: string,
            sizeId: string,
            styleId: string): IVoidAsyncResult;
    }
}