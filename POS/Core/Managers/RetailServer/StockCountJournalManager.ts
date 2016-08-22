/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IStockCountJournalManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class StockCountJournalManager implements Commerce.Model.Managers.IStockCountJournalManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Create a stock count journal.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be created.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        public createStockCountJournalAsync(stockCountJournal: Entities.StockCountJournal): IAsyncResult<Entities.StockCountJournal> {
            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals().create(stockCountJournal);
            return request.execute<Entities.StockCountJournal>();
        }

        /**
         * Update a stock count journal.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be updated.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        public updateStockCountJournalAsync(stockCountJournal: Entities.StockCountJournal): IAsyncResult<Entities.StockCountJournal> {
            if (ObjectExtensions.isNullOrUndefined(stockCountJournal) || StringExtensions.isNullOrWhitespace(stockCountJournal.JournalId)) {
                RetailLogger.genericError("Journal entity or journal identifier is null or undefined.");
                return AsyncResult.createRejected<Entities.StockCountJournal>([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals(stockCountJournal.JournalId).update(stockCountJournal);
            return request.execute<Entities.StockCountJournal>();
        }

        /**
         * Commit a stock count journal. This action will make a journal not searchable until we do operation sync all stock count journals.
         * @param {Entities.StockCountJournal} stockCountJournal The journal to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        public commitStockCountJournalAsync(journalId: string): IVoidAsyncResult {
            if (StringExtensions.isNullOrWhitespace(journalId)) {
                RetailLogger.genericError("Journal identifier is null or empty.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals(journalId).commit();
            return request.execute<Entities.StockCountJournal>();
        }

        /**
         * Sync back committed stock count journal and its product lines (StockCountJournalTransactions).
         * This method is being used when synchronizing StockCountJournalTransactions.
         * @param {string} journalId The journal identifier to be synch-ed with.
         * @return {IAsyncResult<Entities.StockCountJournalTransaction[]>} The async result.
         */
        public syncStockCountJournalAsync(journalId: string): IAsyncResult<Entities.StockCountJournalTransaction[]> {
            if (StringExtensions.isNullOrWhitespace(journalId)) {
                RetailLogger.genericError("Journal identifier is null or empty.");
                return AsyncResult.createRejected<Entities.StockCountJournalTransaction[]>([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals(journalId).syncTransactions();
            return request.execute<Entities.StockCountJournalTransaction[]>();
        }

        /**
         * Get stock count journal details.
         * @param {string} journalId The journal identifier.
         * @return {IAsyncResult<Entities.StockCountJournal>} The async result.
         */
        public getStockCountJournalDetailsAsync(journalId: string): IAsyncResult<Entities.StockCountJournal> {
            if (StringExtensions.isNullOrWhitespace(journalId)) {
                RetailLogger.genericError("Journal identifier is null or empty.");
                return AsyncResult.createRejected<Entities.StockCountJournal>([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals(journalId).read();
            return request.execute<Entities.StockCountJournal>();
        }

        /**
         * Get the list of all stock count journals.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of journals to skip.
         * @return {IAsyncResult<Entities.StockCountJournal[]>} The async result.
         */
        public getStockCountJournalsAsync(top?: number, skip?: number): IAsyncResult<Entities.StockCountJournal[]> {
            var query: Proxy.StockCountJournalsDataServiceQuery = this._commerceContext.stockCountJournals();

            if (top && skip) {
                query.top(top).skip(skip).inlineCount();
            }

            return query.read().execute<Entities.StockCountJournal[]>();
        }

        /**
         * Sync all committed journals. All committed journals will be sync-ed, except StockCountJournalTransactions property.
         * Synchronize stock count journal individually will bring back StockCountJournalTransactions property.
         * @return {IAsyncResult<Entities.StockCountJournal[]>} The async result.
         */
        public syncAllStockCountJournalsAsync(): IAsyncResult<Entities.StockCountJournal[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals().sync();
            return request.execute<Entities.StockCountJournal[]>();
        }

        /**
         * Delete stock count journals.
         * @param {string[]} journalIds The stock count journal identifiers to be removed.
         * @return {IVoidAsyncResult} The async result.
         */
        public deleteStockCountJournalsAsync(journalIds: string[]): IVoidAsyncResult {
            var requests: Common.IDataServiceRequest[] = [];
            var deleteRequest: Common.IDataServiceRequest;

            for (var i: number = 0; i < journalIds.length; i++) {
                deleteRequest = this._commerceContext.stockCountJournals(journalIds[i]).removeJournal();
                requests.push(deleteRequest);
            }

            return deleteRequest.executeBatch(requests);
        }

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
        public deleteStockCountJournalTransactionAsync(
            journalId: string,
            itemId: string,
            colorId: string,
            configurationId: string,
            sizeId: string,
            styleId: string): IVoidAsyncResult {
            if (StringExtensions.isNullOrWhitespace(colorId)) {
                colorId = StringExtensions.EMPTY;
            }

            if (StringExtensions.isNullOrWhitespace(configurationId)) {
                configurationId = StringExtensions.EMPTY;
            }

            if (StringExtensions.isNullOrWhitespace(sizeId)) {
                sizeId = StringExtensions.EMPTY;
            }

            if (StringExtensions.isNullOrWhitespace(styleId)) {
                styleId = StringExtensions.EMPTY;
            }

            var request: Common.IDataServiceRequest = this._commerceContext.stockCountJournals(journalId)
                .removeTransaction(itemId, sizeId, colorId, styleId, configurationId);
            return request.execute();
        }
    }
}
