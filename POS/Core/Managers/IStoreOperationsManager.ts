/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../Entities/StoreOperations.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IStoreOperationsManagerName: string = "IStoreOperationsManager";
    

    export interface IStoreOperationsManager {
        /**
         * Create a non sales transaction.
         * @param {Model.Entities.NonSalesTransaction} nonSalesTransaction The non sales transaction object.
         * @return {IAsyncResult<Entities.NonSalesTransaction>} The async result.
         */
        createNonSalesTransaction(nonSalesTransaction: Model.Entities.NonSalesTransaction): IAsyncResult<Entities.NonSalesTransaction>;

        /**
         * Create drop and declare transaction. (Bank and safe drops).
         * @param {Model.Entities.DropAndDeclareTransaction} dropAndDeclareTransaction The drop and declare transaction object.
         * @return {IAsyncResult<Entities.DropAndDeclareTransaction>} The async result.
         */
        createDropAndDeclareTransaction(dropAndDeclareTransaction: Model.Entities.DropAndDeclareTransaction): IAsyncResult<Entities.DropAndDeclareTransaction>;

        /**
         * Gets all performed Declare starting amount/Tender removal/Float entry operations for a given shift and operation.
         * @param {number} nonSalesTenderOperationId The transaction type of the cash management operation.
         * @param {string} shiftId The shift to filter on.
         * @param {string} shiftTerminalId The identifier of the terminal that creates the shift.
         * @return {IAsyncResult<Entities.NonSalesTransaction[]>} The async result.
         */
        getNonSalesTenderOperationTransactions(
            nonSalesTenderType: number,
            shiftId: string,
            shiftTerminalId: string): IAsyncResult<Entities.NonSalesTransaction[]>;

        /**
         * Opens a new shift with the given cash drawer
         * @param {string} cashDrawer The cash drawer identifier.
         * @param {boolean} isShared The flag indicating whether the shift is shared or not.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        openShiftAsync(cashDrawer: string, isShared: boolean): IAsyncResult<Entities.Shift>;

        /**
         * Closes the given shift.
         * @param {string} terminalId The terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @param {boolean} [forceClose] Whether should force close the shift.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        closeShiftAsync(terminalId: string, shiftId: number, forceClose?: boolean): IAsyncResult<Entities.Shift>;

        /**
         * Blind closes the given shift.
         * @param {string} terminalId The terminal of the shift
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        blindCloseShiftAsync(terminalId: string, shiftId: number): IAsyncResult<Entities.Shift>;

        /**
         * Suspends the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        suspendShiftAsync(terminalId: string, shiftId: number): IAsyncResult<Entities.Shift>;

        /**
         * Resumes the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @param {string} cashDrawer The shift cash drawer.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        resumeShiftAsync(terminalId: string, shiftId: number, cashDrawer: string): IAsyncResult<Entities.Shift>;

        /**
         * Uses the given shift.
         * @param {string} terminalId The shift terminal identifier.
         * @param {number} shiftId The shift identifier.
         * @return {IAsyncResult<Entities.Shift>} The async result.
         */
        useShiftAsync(terminalId: string, shiftId: number): IAsyncResult<Entities.Shift>;

        /**
         * Get all shifts of a given status.
         * @param {number} shiftStatus The shift status to filter on.
         * @return {IAsyncResult<Entities.Shift[]>} The async result.
         */
        getShiftsAsync(shiftStatus: number): IAsyncResult<Entities.Shift[]>;

        /**
         * Gets the time registration status of the current employee.
         * @return {IAsyncResult<any>} The async result.
         */
        getCurrentTimeRegistrationStatus(): IAsyncResult<any>;

        /**
         * Creates a time registration for the given employee.
         * @param {string} staffId The staff identifier of the operator performing the action.
         * @param {Entities.EmployeeActivityType} activityType The type of registration being done.
         * @return {IAsyncResult<Date>} The async result.
         */
        performTimeRegistration(staffId: string, activityType: Entities.EmployeeActivityType): IAsyncResult<Date>;

        /**
         * Gets all time registrations matching a given criteria.
         * @param {Entities.EmployeeActivitySearchCriteria} searchCriteria The criteria to filter on.
         * @return {IAsyncResult<Entities.EmployeeActivity[]>} The async result.
         */
        getTimeRegistrations(searchCriteria: Entities.EmployeeActivitySearchCriteria): IAsyncResult<Entities.EmployeeActivity[]>;

        /**
         * Gets all time registrations matching a given criteria.
         * @param {Entities.EmployeeActivitySearchCriteria} searchCriteria The criteria to filter on.
         * @return {IAsyncResult<Entities.EmployeeActivity[]>} The async result.
         */
        getTimeRegistrationsForManager(searchCriteria: Entities.EmployeeActivitySearchCriteria): IAsyncResult<Entities.EmployeeActivity[]>;

        /**
         * Gets all stores an employee has access to.
         * @return {IAsyncResult<Entities.OrgUnit[]>} The async result.
         */
        getEmployeeStores(): IAsyncResult<Entities.OrgUnit[]>;

        /**
         * Gets all income or expense accounts for a store.
         * @param {number} accountType The account type to find.
         * @return {IAsyncResult<Entities.IncomeExpenseAccount[]>} The async result.
         */
        getIncomeExpenseAccounts(accountType: number): IAsyncResult<Entities.IncomeExpenseAccount[]>;

        /**
         * Gets an X report for the given shift.
         * @param {number} shiftId The identifier of the shift to use.
         * @param {string} terminalId The shift terminal identifier.
         * @param {string} [hardwareProfileId] Hardware profile identifier for the hardware station if enabled.
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        getXReport(shiftId: number, terminalId: string, hardwareProfileId: string): IAsyncResult<Entities.Receipt>;

        /**
         * Gets the Z report for the last closed shift.
         * @param {string} [hardwareProfileId] Hardware profile identifier for the hardware station if enabled.
         * @return {IAsyncResult<Entities.Receipt>} The async result.
         */
        getZReport(hardwareProfileId: string): IAsyncResult<Entities.Receipt>;

        /**
         * Gets the offline sync status.
         * @return {IAsyncResult<Entities.OfflineSyncStatsLine[]>} The async result.
         */
        getOfflineSyncStatus(): IAsyncResult<Entities.OfflineSyncStatsLine[]>;

        /**
         * Gets the offline pending transaction count.
         * @return {IAsyncResult<number>} The async result.
         */
        getOfflinePendingTransactionCount(): IAsyncResult<number>;

        /**
         * Updates the application version.
         * @param {string} appVersion The app version to update to.
         * @return {IVoidAsyncResult} The async result.
         */
        updateApplicationVersion(appVersion: string): IVoidAsyncResult;

        /**
         * Gets the devices available for activation.
         * @param {number} deviceType The device type value.
         * @return {IAsyncResult<Entities.Device[]} The async result.
         */
        getAvailableDevices(deviceType: number): IAsyncResult<Entities.Device[]>;
    }
}
