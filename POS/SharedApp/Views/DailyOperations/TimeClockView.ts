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

    export class TimeClockViewController extends ViewControllerBase {
        public commonHeaderData;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;
        public indeterminateWaitVisible: Observable<boolean>;

        public timeRegistrations: ObservableArray<Model.Entities.EmployeeActivity>;
        public availableStores: ObservableArray<string>;
        private _selectedRegistration: Model.Entities.EmployeeActivity;
        private _logbookDayWindow: number;
        private _logbookStoreFilter: string;

        constructor() {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.indeterminateWaitVisible = ko.observable(false);

            this.timeRegistrations = ko.observableArray(<Model.Entities.EmployeeActivity[]>[]);
            this.availableStores = ko.observableArray(<string[]>[]);
            this._logbookDayWindow = 1;
            this._logbookStoreFilter = ApplicationContext.Instance.storeNumber;

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4082"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4088"));
        }

        private cancelOperation() { }
        private processOperation() { }

        public selectionChangedHandler(activities: Model.Entities.EmployeeActivity[]): void {
            this._selectedRegistration = activities[0];
        }

        public load(): void {
            this.indeterminateWaitVisible(true);
            this.storeOperationsViewModel.getEmployeeStoresAsync()
                .done((stores: Model.Entities.OrgUnit[]) => {
                    this.availableStores(stores.map(s => s.OrgUnitNumber));
                    this.getLogbook();
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public setFilterLast24Hours() {
            this._logbookDayWindow = 1;
            this.getLogbook();
        }

        public setFilterLastWeek() {
            this._logbookDayWindow = 7;
            this.getLogbook();
        }

        public setFilterLastMonth() {
            this._logbookDayWindow = 31;
            this.getLogbook();
        }

        public setStoreFilter(store: string) {
            this._logbookStoreFilter = store;
            this.getLogbook();
        }

        private getLogbook() {
            this.indeterminateWaitVisible(true);

            var myDate = new Date();
            myDate.setDate(myDate.getDate() - this._logbookDayWindow);
            var searchCriteria: Model.Entities.EmployeeActivitySearchCriteria = {
                FromDateTimeOffset:myDate,
                ToDateTimeOffset: new Date(),
                StoreNumber: this._logbookStoreFilter,
                DataLevelValue: 5,
                EmployeeActivityTypeValues: [
                    Model.Entities.EmployeeActivityType.ClockIn,
                    Model.Entities.EmployeeActivityType.ClockOut,
                    Model.Entities.EmployeeActivityType.BreakFromWork,
                    Model.Entities.EmployeeActivityType.BreakForLunch]
            };

            this.storeOperationsViewModel.getTimeRegistrationsAsync(searchCriteria)
                .done((registrations: Model.Entities.EmployeeActivity[]) => {
                    this.indeterminateWaitVisible(false);
                    this.timeRegistrations(registrations);
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }
    }
}
