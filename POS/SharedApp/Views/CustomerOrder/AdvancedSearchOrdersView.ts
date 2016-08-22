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

    export interface IAdvancedSearchOrdersViewControllerOptions {
        searchCriteria: Model.Entities.TransactionSearchCriteria;
    }

    export class AdvancedSearchOrdersViewController extends ViewControllerBase {
        private START_DATE_ELEMENT_ID: string = 'startDate';
        private END_DATE_ELEMENT_ID: string = 'endDate';

        private _searchCriteria: Observable<any>;
        public commonHeaderData;
        private _isStartDateEnabled: Observable<boolean>;
        private _isEndDateEnabled: Observable<boolean>;

        // Required for disabling date time controls.
        private _isStartDateDisabled: Computed<boolean>;
        private _isEndDateDisabled: Computed<boolean>;

        private _previousPageData: any;

        constructor(options: IAdvancedSearchOrdersViewControllerOptions) {
            super(true); // Date pickers

            options = options || { searchCriteria: null };

            this._isStartDateEnabled = ko.observable(false);
            this._isEndDateEnabled = ko.observable(false);

            this._isStartDateDisabled = ko.computed(() => !this._isStartDateEnabled());
            this._isEndDateDisabled = ko.computed(() => !this._isEndDateEnabled());

            var transactionSearchCriteria: Model.Entities.TransactionSearchCriteria = ViewModels.SearchOrdersViewModel.defaultTransactionSearchCriteria;

            if (!ObjectExtensions.isNullOrUndefined(options.searchCriteria)) {
                transactionSearchCriteria = options.searchCriteria;
                transactionSearchCriteria.SearchLocationTypeValue = Model.Entities.SearchLocation.All;
            }

            transactionSearchCriteria.SearchIdentifiers = ''; // reset field hidden in UI
            this.checkDateTime(transactionSearchCriteria);
            this._searchCriteria = ko.observable(ObjectExtensions.convertToObservableProxyObject(transactionSearchCriteria));

            // Load Common Header 
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4143"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4567"));
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);
        }

        private checkDateTime(criteria: Model.Entities.TransactionSearchCriteria): void {
            var now: Date = DateExtensions.getDate();
            if (ObjectExtensions.isNullOrUndefined(criteria.StartDateTime)) {
                criteria.StartDateTime = now;
                this._isStartDateEnabled(false);
            } else {
                this._isStartDateEnabled(true);
            }

            if (ObjectExtensions.isNullOrUndefined(criteria.EndDateTime)) {
                criteria.EndDateTime = DateExtensions.addDays(now, 1);
                this._isEndDateEnabled(false);
            } else {
                this._isEndDateEnabled(true);
            }
        }

        private clearClicked(): void {
            var criteria: Model.Entities.TransactionSearchCriteria = ViewModels.SearchOrdersViewModel.defaultTransactionSearchCriteria;
            this.checkDateTime(criteria);
            this._searchCriteria(ObjectExtensions.convertToObservableProxyObject(criteria));
        }

        private searchClicked(): void {
            var criteria: Model.Entities.TransactionSearchCriteria = ObjectExtensions.unwrapObservableProxyObject(this._searchCriteria());

            criteria.StartDateTime = !this._isStartDateEnabled() ? null : this.getSelectedDate(this.START_DATE_ELEMENT_ID);
            if (this._isEndDateEnabled()) {
                var endDate: Date = this.getSelectedDate(this.END_DATE_ELEMENT_ID);
                DateExtensions.setTimeToLastSecondOfDay(endDate);
                criteria.EndDateTime = endDate;
            } else {
                criteria.EndDateTime = null;
            }

            var validationErrors: Proxy.Entities.Error[] = this.validateSearchCriteria(criteria);
            if (ArrayExtensions.hasElements(validationErrors)) {
                NotificationHandler.displayClientErrors(validationErrors, ErrorTypeEnum.INVALID_SEARCH_CRITERIA);
            } else {
                ViewModelAdapter.navigate("ShowJournalView", <IShowJournalViewControllerOptions>{ SearchCriteria: criteria, IsShowJournalMode: true });
            }
        }

        private validateSearchCriteria(searchCriteria: Proxy.Entities.TransactionSearchCriteria): Proxy.Entities.Error[]{
            var errors: Proxy.Entities.Error[] = [];
            
            if (!ObjectExtensions.isNullOrUndefined(searchCriteria.StartDateTime)) {
                var now: Date = new Date();
                if (searchCriteria.StartDateTime > now) {
                    var startDateNotInTheFuture: Proxy.Entities.Error = new Proxy.Entities.Error(ErrorTypeEnum.START_DATE_NOT_IN_FUTURE);
                    errors.push(startDateNotInTheFuture);
                }

                if (!ObjectExtensions.isNullOrUndefined(searchCriteria.EndDateTime)
                    && searchCriteria.StartDateTime > searchCriteria.EndDateTime) {
                    var startDateNotGreaterThanEndDate: Proxy.Entities.Error = new Proxy.Entities.Error(ErrorTypeEnum.START_DATE_NOT_MORE_RECENT_THAN_END_DATE);
                    errors.push(startDateNotGreaterThanEndDate);
                }
            }

            return errors;
        }

        private getSelectedDate(elementId: string): Date {
            return DateExtensions.getDate(<Date>document.getElementById(elementId).winControl.current);
        }
    }
}