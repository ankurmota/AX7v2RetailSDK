/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class SearchStockCountViewController extends ViewControllerBase {

        private _selectedJournals: Model.Entities.StockCountJournal[];
        public indeterminateWaitVisible: Observable<boolean>;
        public searchViewModel: Commerce.ViewModels.SearchStockCountViewModel;
        public deleteJournalsDisabled: Observable<boolean>;
        public editJournalDisabled: Observable<boolean>;
        public createJournalDisabled: Observable<boolean>;
        public commonHeaderData: Controls.CommonHeaderData;

        constructor(options?: any) {
            super(true);

            this.indeterminateWaitVisible = ko.observable(false);
            this.searchViewModel = new Commerce.ViewModels.SearchStockCountViewModel();
            this.deleteJournalsDisabled = ko.observable(true);
            this.editJournalDisabled = ko.observable(true);
            this.createJournalDisabled = ko.observable(false);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.searchClick = () => {
                this.searchJournals();
            };

            //Load Common Header 
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_3300"));
            this.commonHeaderData.resultCount("");
            this.commonHeaderData.searchText("");

            this.getAndSyncAllJournals();
        }

        /**
         * Get all local journals and sync all journals from HQ.
         */
        public getAndSyncAllJournals(): void {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            this.indeterminateWaitVisible(true);

            asyncQueue.enqueue(() => {
                return this.searchViewModel.getStockCountJournals();
            }).enqueue(() => {
                return this.searchViewModel.syncAllJournals();
            });

            asyncQueue.run().always(() => {
                this.indeterminateWaitVisible(false);
            })
            .done(() => {
                this.commonHeaderData.resultCount(this.searchViewModel.resultCount() + " " + Commerce.ViewModelAdapter.getResourceString("string_1001"));
            }).
            fail((errors: Proxy.Entities.Error[]) => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        /**
         * Get the list of uncommited journals.
         */
        public getJournals() {
            this.indeterminateWaitVisible(true);

            this.searchViewModel.getStockCountJournals()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    this.commonHeaderData.resultCount(this.searchViewModel.resultCount() + " " + Commerce.ViewModelAdapter.getResourceString("string_1001"));
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            );
        }

        /**
         * Search the journals from a textbox input.
         */
        public searchJournals() {
            this.searchViewModel.searchStockCountJournals(this.commonHeaderData.searchText());
            this.commonHeaderData.resultCount(this.searchViewModel.resultCount() + " " + Commerce.ViewModelAdapter.getResourceString("string_1001"));
        }

        /**
         * Delete selected journal(s).
         */
        public deleteJournals() {
            this.searchViewModel.selectedJournals = this._selectedJournals;
            this.indeterminateWaitVisible(true);

            this.searchViewModel.deleteJournals()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                    this.getJournals();
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            );
        }

        /**
         * Create a new journal.
         */
        public createJournal() {
            Commerce.ViewModelAdapter.navigate("StockCountDetailsView");
        }

        /**
         * Edit a journal.
         */
        public editJournal() {

            var parameters = {
                JournalId: this._selectedJournals[0].JournalId
            };

            Commerce.ViewModelAdapter.navigate("StockCountDetailsView", parameters);
        }

        /**
         * Synchronized all commited journals to become uncommited.
         */
        public syncAllJournals() {
            this.indeterminateWaitVisible(true);
            this.searchViewModel.syncAllJournals()
                .done(() => {
                    this.commonHeaderData.resultCount(this.searchViewModel.resultCount() + " " + Commerce.ViewModelAdapter.getResourceString("string_1001"));
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                }
            );
        }

        /**
         * Event raised after user select / unselect journal(s)
         */
        public stockCountListSelectionChanged(items: Model.Entities.StockCountJournal[]) {
            this._selectedJournals = items;
            var numJournalsSelected: number = items.length;
            this.deleteJournalsDisabled(numJournalsSelected < 1);
            this.editJournalDisabled(numJournalsSelected !== 1);
        }
    }
}