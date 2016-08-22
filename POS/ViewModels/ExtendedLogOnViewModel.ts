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
    "use strict";

    /**
     * Represents the extended logon view model.
     */
    export class ExtendedLogOnViewModel {

        public foundEmployees: ObservableArray<Proxy.Entities.Employee>;
        public selectedEmployee: Observable<Proxy.Entities.Employee>;
        public isEmployeeSelected: Computed<boolean>;
        public busy: Observable<boolean>;
        public assignInProgress: Observable<boolean>;
        public unassignInProgress: Observable<boolean>;
        public selectedEmployeeText: Computed<string>;
        public extendedLogonTokenUnavailable: Computed<boolean>;
        private _extendedLogonToken: Observable<string>;
        private _extendedLogonGrantType: string = null;
        private _peripherals: Peripherals.IPeripherals;
        private _managerFactory: Model.Managers.IManagerFactory;
        private _displayClientErrors: (errors: Model.Entities.Error[]) => void;
        private _viewModelAdapter: IViewModelAdapter;

        /**
         * Constructs an extended logon view model.
         * @param {Peripherals.IPeripherals} peripherals The peripherals.
         * @param {Model.Managers.IManagerFactory} managerFactory The manager facto.
         * @param {(errors: Proxy.Entities.Error[]) => void} displayClientErrors The caller context.
         * @param {IViewModelAdapter} viewModelAdapter The view model adapter.
         */
        constructor(peripherals: Peripherals.IPeripherals,
                        managerFactory: Model.Managers.IManagerFactory,
                        displayClientErrors: (errors: Proxy.Entities.Error[]) => void,
                        viewModelAdapter: IViewModelAdapter) {
            this._peripherals = peripherals;
            this._managerFactory = managerFactory;
            this._displayClientErrors = displayClientErrors;
            this._viewModelAdapter = viewModelAdapter;

            this.busy = ko.observable(false);
            this.foundEmployees = ko.observableArray([]);
            this.selectedEmployee = ko.observable(null);
            this.assignInProgress = ko.observable(false);
            this.unassignInProgress = ko.observable(false);
            this._extendedLogonToken = ko.observable(null);

            this.selectedEmployeeText = ko.computed(() => this.computeSelectedEmployeeText());
            this.isEmployeeSelected = ko.computed(() => this.computeEmployeeSelected());
            this.extendedLogonTokenUnavailable = ko.computed(() => this.computeExtendedLogonTokenUnavailable());
        }

        /**
         * Search for an employee.
         * @param {string} query The query term.
         */
        public search(query: string): void {
            this.runAsyncRequest(
                this._managerFactory.getManager<Model.Managers.IOperatorManager>(Model.Managers.IOperatorManagerName)
                    .getEmployeesAsync()
                    .done((result: Proxy.Entities.Employee[]) => {
                        this.findEmployee(query, result);
                    })
                );
        }

        /**
         * Start disenrollment process.
         */
        public unassignStart(): void {
            this.unassignInProgress(true);
        }

        /**
         * Start enrollment process.
         */
        public assignStart(): void {
            this._peripherals.barcodeScanner.enableAsync((barcode: string) => {
                this._extendedLogonToken(barcode);
                this._extendedLogonGrantType = Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_BARCODE_GRANT_TYPE;
            });

            this._peripherals.magneticStripeReader.enableAsync((cardInfo: Proxy.Entities.CardInfo) => {
                this._extendedLogonToken(cardInfo.CardNumber);
                this._extendedLogonGrantType = Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_MSR_GRANT_TYPE;
            });
            this.assignInProgress(true);
        }

        /**
         * Finish enrollment process.
         */
        public assignConfirm(): void {
            this.assignExtendedLogon(this._extendedLogonToken(), this._extendedLogonGrantType);
            this.assignComplete();
        }

        /**
         * Cancel enrollment process.
         */
        public assignCancel(): void {
            this.assignComplete();
        }

        /**
         * Finish disenrollment process.
         */
        public unassignConfirm(): void {
            this.unAssignExtendedLogon();
            this.unassignComplete();
        }

        /**
         * Cancel disenrollment process.
         */
        public unassignCancel(): void {
            this.unassignComplete();
        }

        private findEmployee(query: string, employees: Proxy.Entities.Employee[]): void {
            query = (query || "").toLowerCase();
            employees = employees.filter((value: Proxy.Entities.Employee) => {
                return !ObjectExtensions.isNullOrUndefined(value)
                    && (((value.StaffId || "").toLowerCase().indexOf(query) !== -1)
                        || ((value.Name || "").toLowerCase().indexOf(query) !== -1));
            });
            this.foundEmployees(employees);
        }

        private assignComplete(): void {
            this.assignInProgress(false);
            this._extendedLogonToken(null);
            this._extendedLogonGrantType = null;
            this._peripherals.barcodeScanner.disableAsync();
            this._peripherals.magneticStripeReader.disableAsync();
        }

        private unassignComplete(): void {
            this.unassignInProgress(false);
        }

        private computeSelectedEmployeeText(): string {
            var selectedEmployee: Proxy.Entities.Employee = this.selectedEmployee();
            if (ObjectExtensions.isNullOrUndefined(selectedEmployee)) {
                return null;
            }
            return StringExtensions.format("{0} : {1}",
                selectedEmployee.StaffId,
                selectedEmployee.Name);
        }

        private computeEmployeeSelected(): boolean {
            return !ObjectExtensions.isNullOrUndefined(this.selectedEmployee());
        }

        private assignExtendedLogon(token: string, grantType: string): void {
            this.runAsyncRequest(
                this._managerFactory.getManager<Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName)
                    .enrollUserCredentials({
                        userId: this.selectedEmployee().StaffId,
                        grantType: grantType,
                        credential: token
                    }).done(() => {
                        this.showMessage("string_11011"); // action is completed confirmation.
                    })
                );
        }

        private unAssignExtendedLogon(): void {
            this.runAsyncRequest(
                this._managerFactory.getManager<Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName)
                .disenrollUserCredentials({
                    userId: this.selectedEmployee().StaffId,
                    grantType: Authentication.Providers.CommerceUserAuthenticationProvider.EXTENDEDLOGON_ALL_GRANT_TYPE
                    }).done(() => {
                        this.showMessage("string_11010"); // action is completed confirmation.
                    })
                );
        }

        private showMessage(resourceId: string): void {
            var message: string = this._viewModelAdapter.getResourceString(resourceId);
            this._viewModelAdapter.displayMessage(message);
        }

        private runAsyncRequest(result: IVoidAsyncResult): IVoidAsyncResult {
            this.busy(true);
            return result.fail((errors: Proxy.Entities.Error[]) => {
                this._displayClientErrors(errors);
            }).always(() => {
                this.busy(false);
            });
        }

        private computeExtendedLogonTokenUnavailable(): boolean {
            return StringExtensions.isNullOrWhitespace(this._extendedLogonToken());
        }
    }
}