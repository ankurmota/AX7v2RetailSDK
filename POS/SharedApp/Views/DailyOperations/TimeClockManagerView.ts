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

module Commerce.ViewControllers {
    "use strict";

    /**
    * Options for time clock manager view.
    */
    export interface ITimeClockManagerViewControllerOptions {

        /**
         * The store id to filter the time clock entries by.
         */
        storeId?: string;
    }

    export class TimeClockManagerViewController extends ViewControllerBase {
        public commonHeaderData;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;
        public indeterminateWaitVisible: Observable<boolean>;

        public timeRegistrations: ObservableArray<Model.Entities.EmployeeActivity>;
        private _selectedRegistration: Model.Entities.EmployeeActivity;
        private _storeFilter: string;
        private _options: ITimeClockManagerViewControllerOptions;
        private _activityFilters: Model.Entities.EmployeeActivityType[];
        private _registrationsPeriodInDays: number = 90;

         /**
         * constructor
         *
         * @param {ITimeClockManagerViewControllerOptions} options Setup data for the view
         * 
         * Supported option properties:
         * {string} [storeId] The store id to filter the time clock entries by.
         */
        constructor(options?: ITimeClockManagerViewControllerOptions) {
            super(true);

            this._options = options || <ITimeClockManagerViewControllerOptions>{};
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.indeterminateWaitVisible = ko.observable(false);

            this.timeRegistrations = ko.observableArray(<Model.Entities.EmployeeActivity[]>[]);

            if (!Commerce.ObjectExtensions.isNullOrUndefined(options) && !Commerce.StringExtensions.isNullOrWhitespace(this._options.storeId)) {
                this._storeFilter = this._options.storeId;
            }
            else {
                this._storeFilter = ApplicationContext.Instance.storeNumber;
            }

            this._activityFilters = [Model.Entities.EmployeeActivityType.ClockIn, Model.Entities.EmployeeActivityType.BreakFlowStop];

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4124"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4158"));
        }

        public selectionChangedHandler(activities: Model.Entities.EmployeeActivity[]): void {
            this._selectedRegistration = activities[0];
        }

        public load(): void {
            this.loadRegistrations();
        }

        private loadRegistrations() {
            var startDate: Date = DateExtensions.now;
            startDate = DateExtensions.addDays(startDate, -this._registrationsPeriodInDays);
            var activitiesFilterValues: number[] = [];
            this._activityFilters.forEach(f => { activitiesFilterValues.push(f); });
            
            var searchCriteria: Model.Entities.EmployeeActivitySearchCriteria = {
                FromDateTimeOffset: startDate,
                ToDateTimeOffset: DateExtensions.now,
                StoreNumber: this._storeFilter,
                DataLevelValue: 4,
                EmployeeActivityTypeValues: activitiesFilterValues
            };

            this.indeterminateWaitVisible(true);
            this.storeOperationsViewModel.getRegistrationsForManagerAsync(searchCriteria)
                .done((registrations: Model.Entities.EmployeeActivity[]) => {
                    for (var i: number = 0; i < registrations.length; i++) {
                        switch (this._activityFilters[0]) {
                            case Model.Entities.EmployeeActivityType.BreakForLunch:
                                registrations[i].Activity = Commerce.ViewModelAdapter.getResourceString("string_4087");
                                break;
                            case Model.Entities.EmployeeActivityType.BreakFromWork:
                                registrations[i].Activity = Commerce.ViewModelAdapter.getResourceString("string_4086");
                                break;
                            case Model.Entities.EmployeeActivityType.ClockIn:
                                registrations[i].Activity = Commerce.ViewModelAdapter.getResourceString("string_4084");
                                break;
                            case Model.Entities.EmployeeActivityType.ClockOut:
                                registrations[i].Activity = Commerce.ViewModelAdapter.getResourceString("string_4085");
                                break;
                        }
                    }

                    this.indeterminateWaitVisible(false);
                    this.timeRegistrations(registrations);
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public setClockIn() {
            this._activityFilters = [Model.Entities.EmployeeActivityType.ClockIn, Model.Entities.EmployeeActivityType.BreakFlowStop];
            this.loadRegistrations();
        }

        public setClockOut() {
            this._activityFilters = [Model.Entities.EmployeeActivityType.ClockOut];
            this.loadRegistrations();
        }

        public setBreakForLunch() {
            this._activityFilters = [Model.Entities.EmployeeActivityType.BreakForLunch];
            this.loadRegistrations();
        }

        public setBreakFromWork() {
            this._activityFilters = [Model.Entities.EmployeeActivityType.BreakFromWork];
            this.loadRegistrations();
        }

        public setStoreFilter() {
            var parameters: IPickUpInStoreViewControllerOptions = {
                isForPickUp: false,
                callerPage: "TimeClockManagerView",
                storeSelectionCallback: (store: Model.Entities.OrgUnit): IAsyncResult<ITimeClockManagerViewControllerOptions> => {
                    return AsyncResult.createResolved({ storeId: store.OrgUnitNumber });
                }
            };

            Commerce.ViewModelAdapter.navigate("PickUpInStoreView", parameters);
        }

        public setStoreFilterAll() {
            this._storeFilter = null;
            this.loadRegistrations();
        }
    }
}
