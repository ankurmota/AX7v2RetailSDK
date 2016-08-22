/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    export class SearchStockCountViewModel extends ViewModelBase {

        public allJournals: Model.Entities.StockCountJournal[];
        public selectedJournals: Model.Entities.StockCountJournal[];
        public resultCount: Computed<number>;
        public stockCountJournals: ObservableArray<Model.Entities.StockCountJournal>;

        constructor() {
            super();
            this.allJournals = [];

            this.stockCountJournals = ko.observableArray<Model.Entities.StockCountJournal>([]);
            this.resultCount = ko.computed(this.countJournals, this);
        }

        private countJournals(): number {
            return this.stockCountJournals().length;
        }

        /**
         * Get the list of all uncommitted journals.
         *
         * @return {IVoidAsyncResult} The async void result.
         */
        public getStockCountJournals(): IVoidAsyncResult {
            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.GetAll,
                stockCountJournal: undefined,
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options);
        }

        /**
         * Search stock count journals from a given search text.
         */
        public searchStockCountJournals(searchText: string): void {
            var journalFilterResults: Model.Entities.StockCountJournal[] = [];
            for (var i = 0; i < this.allJournals.length; i++) {
                if (this.allJournals[i].JournalId.indexOf(searchText) >= 0) {
                    journalFilterResults.push(this.allJournals[i]);
                }
            }

            this.stockCountJournals(journalFilterResults);
        }

        /**
         * Synchronize all committed journals so they can be edited again.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public syncAllJournals(): IVoidAsyncResult {
            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.SyncAll,
                stockCountJournal: undefined,
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options);
        }

        /**
         * Delete the selected stock count journals.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public deleteJournals(): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(this.selectedJournals)) {
                return VoidAsyncResult.createResolved();
            }

            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.Delete,
                stockCountJournal: undefined,
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options);
        }
    }
}