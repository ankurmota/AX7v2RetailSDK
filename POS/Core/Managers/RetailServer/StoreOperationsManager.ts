/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Entities/StoreOperations.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IStoreOperationsManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class StoreOperationsManager implements Commerce.Model.Managers.IStoreOperationsManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Create a non sales transaction.
         * @param {Model.Entities.NonSalesTransaction} nonSalesTransaction The non sales transaction object.
         * @return {IAsyncResult<Entities.NonSalesTransaction>} The async result.
         */
        public createNonSalesTransaction(nonSalesTransaction: Model.Entities.NonSalesTransaction): IAsyncResult<Entities.NonSalesTransaction> {
            if (StringExtensions.isNullOrWhitespace(nonSalesTransaction.Id)) {
                nonSalesTransaction.Id = NumberSequence.GetNextTransactionId();
            }

            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().createNonSalesTransaction(nonSalesTransaction);
            return request.execute<Entities.NonSalesTransaction>();
        }

        /**
         * Create drop and declare transaction. (Bank and safe drops)
         * @param {Model.Entities.DropAndDeclareTransaction} dropAndDeclareTransaction The drop and declare transaction object.
         * @return {IAsyncResult<Entities.DropAndDeclareTransaction>} The async result.
         */
        public createDropAndDeclareTransaction(dropAndDeclareTransaction: Model.Entities.DropAndDeclareTransaction)
            : IAsyncResult<Entities.DropAndDeclareTransaction> {
            if (StringExtensions.isNullOrWhitespace(dropAndDeclareTransaction.Id)) {
                dropAndDeclareTransaction.Id = NumberSequence.GetNextTransactionId();
            }

            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().createDropAndDeclareTransaction(dropAndDeclareTransaction);
            return request.execute<Entities.DropAndDeclareTransaction>();
        }

        /**
         * Gets all performed Declare starting amount/Tender removal/Float entry operations for a given shift and operation.
         * @param {number} nonSalesTenderOperationId The transaction type of the cash management operation.
         * @param {string} shiftId The shift to filter on.
         * @param {string} shiftTerminalId The identifier of the terminal that creates the shift.
         * @return {IAsyncResult<Entities.NonSalesTransaction[]>} The async result.
         */
        public getNonSalesTenderOperationTransactions(
            nonSalesTenderType: number,
            shiftId: string,
            shiftTerminalId: string): IAsyncResult<Entities.NonSalesTransaction[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations()
                .getNonSalesTransactions(shiftId, shiftTerminalId, nonSalesTenderType);
            return request.execute<Entities.NonSalesTransaction[]>();
        }

        /**
         * Opens a new shift with the given cash drawer
         * @param {string} cashDrawer The cash drawer identifier.
         * @param {boolean} isShared The flag indicating whether the shift is shared or not.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public openShiftAsync(cashDrawer: string, isShared: boolean): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts()
                .open(NumberSequence.GetNextValue(Entities.NumberSequenceSeedType.BatchId), cashDrawer, isShared);
            return request.execute<Entities.Shift>();
        }

        /**
         * Closes the given shift.
         * @param {string} terminalId The terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @param {boolean} [forceClose] Whether should force close the shift.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public closeShiftAsync(terminalId: string, shiftId: number, forceClose: boolean = false): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId)
                .close(NumberSequence.GetNextTransactionId(), forceClose);
            return request.execute<Entities.Shift>();
        }

        /**
         * Blind closes the given shift.
         * @param {string} terminalId The terminal of the shift
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public blindCloseShiftAsync(terminalId: string, shiftId: number, forceClose: boolean = true): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId)
                .blindClose(NumberSequence.GetNextTransactionId(), forceClose);
            return request.execute<Entities.Shift>();
        }

        /**
         * Suspends the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public suspendShiftAsync(terminalId: string, shiftId: number): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId)
                .suspend(NumberSequence.GetNextTransactionId());
            return request.execute<Entities.Shift>();
        }

        /**
         * Resumes the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @param {string} cashDrawer The shift cash drawer.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public resumeShiftAsync(terminalId: string, shiftId: number, cashDrawer: string): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId).resume(cashDrawer);
            return request.execute<Entities.Shift>();
        }

        /**
         * Uses the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        public useShiftAsync(terminalId: string, shiftId: number): IAsyncResult<Entities.Shift> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId).use();
            return request.execute<Entities.Shift>();
        }

        /**
         * Get all shifts of a given status.
         * @param {number} shiftStatus The shift status to filter on.
         * @return {IAsyncResult<Entities.Shift[]>} The async result.
         */
        public getShiftsAsync(shiftStatus: number): IAsyncResult<Entities.Shift[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.shifts().getByStatus(shiftStatus);
            return request.execute<Entities.Shift[]>();
        }

        /**
         * Gets the time registration status of the current employee.
         * @return {IAsyncResult<any>} The async result.
         */
        public getCurrentTimeRegistrationStatus(): IAsyncResult<Entities.EmployeeActivity> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().getLatestActivity();
            return request.execute<Entities.EmployeeActivity>();
        }

        /**
         * Creates a time registration for the given employee.
         * @param {string} staffId The staff identifier of the operator performing the action.
         * @param {Entities.EmployeeActivityType} activityType The type of registration being done.
         * @return {IAsyncResult<any>} The async result.
         */
        public performTimeRegistration(staffId: string, activityType: Entities.EmployeeActivityType): IAsyncResult<Date> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees(staffId).registerActivity(activityType);
            return request.execute<string>()
                .map((result: string) => {
                    return OData.jsonLightReadStringPropertyValue(result, "Edm.DateTimeOffset", false);
                });
        }

        /**
         * Gets all time registrations matching a given criteria.
         * @param {Entities.EmployeeActivitySearchCriteria} searchCriteria The criteria to filter on.
         * @return {IAsyncResult<Entities.EmployeeActivity[]>} The async result.
         */
        public getTimeRegistrations(searchCriteria: Entities.EmployeeActivitySearchCriteria): IAsyncResult<Entities.EmployeeActivity[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().getActivities(searchCriteria);
            return request.execute<Entities.EmployeeActivity[]>();
        }

        /**
         * Gets all time registrations matching a given criteria.
         * @param {Entities.EmployeeActivitySearchCriteria} searchCriteria The criteria to filter on.
         * @return {IAsyncResult<Entities.EmployeeActivity[]>} The async result.
         */
        public getTimeRegistrationsForManager(searchCriteria: Entities.EmployeeActivitySearchCriteria): IAsyncResult<Entities.EmployeeActivity[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().getManagerActivityView(searchCriteria);
            return request.execute<Entities.EmployeeActivity[]>();
        }

        /**
         * Gets all stores an employee has access to.
         * @return {IAsyncResult<Entities.OrgUnit[]>} The async result.
         */
        public getEmployeeStores(): IAsyncResult<Entities.OrgUnit[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.employees().getAccessibleOrgUnits();
            return request.execute<Entities.OrgUnit[]>();
        }

        /**
         * Gets all income or expense accounts for a store.
         * @param {number} accountType The account type to find.
         * @return {IAsyncResult<Entities.IncomeExpenseAccount[]>} The async result.
         */
        public getIncomeExpenseAccounts(accountType: number): IAsyncResult<Entities.IncomeExpenseAccount[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getIncomeExpenseAccounts(accountType);
            return request.execute<Entities.IncomeExpenseAccount[]>();
        }

        /**
         * Gets an X report for the given shift.
         * @param {number} shiftId The identifier of the shift to use.
         * @param {string} terminalId The shift terminal identifier.
         * @param {string} [hardwareProfileId] Hardware profile identifier for the hardware station if enabled.
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        public getXReport(shiftId: number, terminalId: string, hardwareProfileId: string): IAsyncResult<Entities.Receipt> {
            var transactionId: string = NumberSequence.GetNextTransactionId();
            var request: Common.IDataServiceRequest = this._commerceContext.shifts(shiftId, terminalId).getXReport(transactionId, hardwareProfileId);
            return request.execute<Entities.Receipt>();
        }

        /**
         * Gets the Z report for the last closed shift.
         * @param {string} [hardwareProfileId] Hardware profile identifier for the hardware station if enabled.
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        public getZReport(hardwareProfileId: string): IAsyncResult<Entities.Receipt> {
            var transactionId: string = NumberSequence.GetNextTransactionId();
            var request: Common.IDataServiceRequest = this._commerceContext.shifts().getZReport(transactionId, hardwareProfileId);
            return request.execute<Entities.Receipt>();
        }

        /**
         * Gets the offline sync status.
         * @return {IAsyncResult<Entities.OfflineSyncStatsLine[]>} The async result.
         */
        public getOfflineSyncStatus(): IAsyncResult<Entities.OfflineSyncStatsLine[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getOfflineSyncStatus();
            return request.execute<Entities.OfflineSyncStatsLine[]>();
        }

        /**
         * Gets the offline pending transaction count.
         * @return {IAsyncResult<number>} The async result.
         */
        public getOfflinePendingTransactionCount(): IAsyncResult<number> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getOfflinePendingTransactionCount();
            return request.execute<number>();
        }

        /**
         * Updates the application version.
         * @param {string} appVersion The app version to update to.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateApplicationVersion(appVersion: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().updateApplicationVersion(appVersion);
            return request.execute();
        }

        /**
         * Gets the devices available for activation.
         * @param {number} deviceType The device type value.
         * @return {IAsyncResult<Entities.Device[]} The async result.
         */
        public getAvailableDevices(deviceType: number): IAsyncResult<Entities.Device[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getAvailableDevices(deviceType);
            return request.execute<Entities.Device[]>();
        }
    }
}
