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

    type CashDrawerInputCancelableResult = ICancelableDataResult<Proxy.Entities.HardwareProfileCashDrawer>;

    export class ShiftViewModel extends ViewModelBase {
        private static MANAGER_PRIVILEGES: string = "MANAGERPRIVILEGES";
        private _availableShiftActions: Proxy.Entities.AvailableShiftActions;

        constructor() {
            super();
        }

        /**
         * Opens or Resumes a shift.
         *
         * @param {string} operatorId The operator identifier.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public openOrResumeShift(operatorId: string): IAsyncResult<ICancelableResult> {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var shiftAction: Activities.ShiftActionType;

            asyncQueue.enqueue(() => {
                return this.getShiftActions(operatorId)
                    .done((availableShiftActions: Proxy.Entities.AvailableShiftActions) => {
                        this._availableShiftActions = availableShiftActions;
                    });
            }).enqueue(() => {
                if (this._availableShiftActions.shouldSkipDialog) {
                    shiftAction = Activities.ShiftActionType.AlreadyOpen;
                    return VoidAsyncResult.createResolved();
                } else {
                    var activity: Activities.GetShiftActionActivity
                        = new Activities.GetShiftActionActivity({ shiftActions: this._availableShiftActions });

                    return activity.execute().done(() => {
                        if (ObjectExtensions.isNullOrUndefined(activity.response)) {
                            asyncQueue.cancel();
                        } else {
                            shiftAction = activity.response.shiftActionType;
                        }
                    });
                }
            }).enqueue(() => {
                switch (shiftAction) {
                    case Activities.ShiftActionType.AlreadyOpen:
                        break;

                    case Activities.ShiftActionType.NewShift:
                        return asyncQueue.cancelOn(this.startNewShift().run());

                    case Activities.ShiftActionType.ExistingShift:
                        return asyncQueue.cancelOn(this.resumeShift().run());

                    default:
                        Session.instance.Shift = null;
                }

                return VoidAsyncResult.createResolved();
            });

            return asyncQueue.run();
        }

        /**
         * Opens a shift on the given cash drawer.
         *
         * @param {string} cashDrawerName The cash drawer name.
         * @param {boolean} isShared Set to true to open shift shared.
         * @return {IAsyncResult<Proxy.Entities.Shift>} The async result.
         */
        public openShiftAsync(cashDrawerName: string, isShared: boolean): IAsyncResult<Proxy.Entities.Shift> {
            // if isShared was not passed then CashDrawers were not setup.
            // We still need to allow opening the shift.
            if (typeof isShared !== "boolean") {
                isShared = false;
            }

            // For shared shift we will open a shift with empty drawer name, as drawer name/type can change while shift is open.
            return this.storeOperationsManager.openShiftAsync(isShared ? StringExtensions.EMPTY : cashDrawerName, isShared)
                .done((shift: Proxy.Entities.Shift) => {
                    Session.instance.Shift = shift;
                    ShiftHelper.saveCashDrawerOnStorage(cashDrawerName);
                });
        }

        /**
         * Resumes a shift.
         *
         * @param {Proxy.Entities.Shift} shift The shift to resume.
         * @return {IAsyncResult<Proxy.Entities.Shift>} The async result.
         */
        public resumeShiftAsync(shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Shift> {
            return this.storeOperationsManager.resumeShiftAsync(
                shift.TerminalId,
                shift.ShiftId,
                shift.IsShared ? StringExtensions.EMPTY : shift.CashDrawer)
                .done((updatedShift: Proxy.Entities.Shift) => {
                    Session.instance.Shift = updatedShift;
                    ShiftHelper.saveCashDrawerOnStorage(shift.CashDrawer);
                });
        }

        /**
         * The current logged on operator uses the given shift, for instance, a manager can use a cashier's open shift.
         *
         * @param {Proxy.Entities.Shift} shift The shift to use.o
         * @return {IAsyncResult<Proxy.Entities.Shift>} The async result.
         */
        public useShiftAsync(shift: Proxy.Entities.Shift): IAsyncResult<Proxy.Entities.Shift> {
            return this.storeOperationsManager.useShiftAsync(shift.TerminalId, shift.ShiftId)
                .done((updatedShift: Proxy.Entities.Shift) => {
                    Session.instance.Shift = updatedShift;
                    ShiftHelper.saveCashDrawerOnStorage(shift.CashDrawer);
                });
        }

        /**
         * Get all open shifts.
         *
         * @return {IAsyncResult<Proxy.Entities.Shift[]>} The async result.
         */
        public getOpenShiftsAsync(): IAsyncResult<Proxy.Entities.Shift[]> {
            return this.storeOperationsManager.getShiftsAsync(Proxy.Entities.ShiftStatus.Open);
        }

        /**
         * Get all suspended shifts.
         *
         * @return {IAsyncResult<Proxy.Entities.Shift[]>} The async result.
         */
        public getSuspendedShiftsAsync(): IAsyncResult<Proxy.Entities.Shift[]> {
            return this.storeOperationsManager.getShiftsAsync(Proxy.Entities.ShiftStatus.Suspended);
        }

        /**
         * Get all blind closed shifts.
         *
         * @return {IAsyncResult<Proxy.Entities.Shift[]>} The async result.
         */
        public getBlindClosedShiftsAsync(): IAsyncResult<Proxy.Entities.Shift[]> {
            return this.storeOperationsManager.getShiftsAsync(Proxy.Entities.ShiftStatus.BlindClosed);
        }

        /**
         * Get the possible shift actions for the current operator and terminal
         *
         * @param {string} operatorId The current operator.
         */
        private getShiftActions(operatorId: string): IAsyncResult<Proxy.Entities.AvailableShiftActions> {
            var shiftActions: Proxy.Entities.AvailableShiftActions = new Proxy.Entities.AvailableShiftActions();
            var openShifts: Proxy.Entities.Shift[];

            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue((): IAsyncResult<any> => {
                    return this.getOpenShiftsAsync().done((result: Proxy.Entities.Shift[]) => { openShifts = result; });
                }).enqueue((): IAsyncResult<any> => {
                    // check for auto-reusable shifts
                    var autoReusableShift: Proxy.Entities.Shift = this.getAutoReusableShift(openShifts, operatorId);
                    if (!ObjectExtensions.isNullOrUndefined(autoReusableShift)) {
                        return this.useShiftAsync(autoReusableShift)
                            .done((shift: Proxy.Entities.Shift) => { shiftActions.shouldSkipDialog = true; });
                    }

                    var permissions: Proxy.Entities.EmployeePermissions = Session.instance.CurrentEmployee.Permissions;
                    var isManager: boolean = permissions.Roles.some((r: string) => r.toUpperCase() === ShiftViewModel.MANAGER_PRIVILEGES);

                    // if none, get suspended, openable cashdrawers and re-usable shifts
                    shiftActions.dialogTitle = Commerce.ViewModelAdapter.getResourceString("string_4000");
                    shiftActions.reusableShifts = openShifts;

                    var cashDrawers: Proxy.Entities.HardwareProfileCashDrawer[] = this.getCashDrawersFromProfile();
                    shiftActions.availableCashDrawers = this.getAvailableCashDrawer(cashDrawers, openShifts, isManager, permissions);

                    // if we do not have available cash drawers, it means we cannot resume a suspended shift,
                    // we might be able to at least use one
                    if (ArrayExtensions.hasElements(shiftActions.availableCashDrawers)) {
                        return this.getSuspendedShiftsAsync().done((suspendedShifts: Proxy.Entities.Shift[]) => {
                            shiftActions.suspendedShifts = suspendedShifts.filter((s: Proxy.Entities.Shift) => {
                                if (s.IsShared && !permissions.AllowManageSharedShift && !isManager) {
                                    return false;
                                }

                                return true;
                            });
                        });
                    }

                    return VoidAsyncResult.createResolved();
                });

            return asyncQueue.run().map((): Proxy.Entities.AvailableShiftActions => { return shiftActions; });
        }

        /**
         * Returns the new shift execution queue.
         *
         * @return {AsyncQueue} The execution queue.
         */
        private startNewShift(): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var cashDrawer: Proxy.Entities.HardwareProfileCashDrawer;

            asyncQueue.enqueue(() => {
                return this.getCashDrawerSelection(this._availableShiftActions.availableCashDrawers)
                    .done((drawerInputResult: CashDrawerInputCancelableResult) => {
                        if (drawerInputResult.canceled) {
                            asyncQueue.cancel();
                        } else {
                            cashDrawer = drawerInputResult.data;
                        }
                    });
            }).enqueue(() => {
                return this.openShiftAsync(cashDrawer.DeviceName, cashDrawer.IsSharedShiftDrawer);
            });

            return asyncQueue;
        }

        /**
         * Returns the resume shift execution queue.
         *
         * @return {AsyncQueue} The execution queue.
         */
        private resumeShift(): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                var activity: Activities.ResumeShiftActivity
                    = new Activities.ResumeShiftActivity({ availableShiftActions: this._availableShiftActions });

                activity.responseHandler = (shift: Proxy.Entities.Shift): IVoidAsyncResult => {
                    if (ObjectExtensions.isNullOrUndefined(shift)) {
                        asyncQueue.cancel();
                        return VoidAsyncResult.createResolved();
                    }
                    // ResponseHandler doesn't support cancelable result. Convert cancel result to failure. 
                    var asyncResult: VoidAsyncResult = new VoidAsyncResult();

                    this.useOrResumeSelectedShift(shift).run()
                        .done((result: ICancelableResult) => {
                            if (result.canceled) {
                                asyncResult.reject(null);
                            } else {
                                asyncResult.resolve();
                            }
                        })
                        .fail((errors: Proxy.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });

                    return asyncResult;
                };

                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }
                });
            });

            return asyncQueue;
        }

        /**
         * Returns an use or resumes workflow for the selected shift in Resume shift view..
         *
         * @return {AsyncQueue} The execution queue.
         */
        private useOrResumeSelectedShift(shift: Proxy.Entities.Shift): AsyncQueue {
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var shiftCashDrawer: Proxy.Entities.HardwareProfileCashDrawer = new Proxy.Entities.HardwareProfileCashDrawerClass();

            shiftCashDrawer.DeviceName = StringExtensions.EMPTY;

            asyncQueue.enqueue(() => {
                if (StringExtensions.isNullOrWhitespace(shift.CashDrawer)) {
                    var cashDrawers: Proxy.Entities.HardwareProfileCashDrawer[] = ArrayExtensions.where(
                        this._availableShiftActions.availableCashDrawers,
                        (c: Proxy.Entities.HardwareProfileCashDrawer) =>
                            (c.IsSharedShiftDrawer && shift.IsShared) || (!c.IsSharedShiftDrawer && !shift.IsShared));

                    if (!ArrayExtensions.hasElements(cashDrawers)) {
                        if (shift.IsShared || ArrayExtensions.hasElements(this._availableShiftActions.availableCashDrawers)) {
                            // If no drawer with shared shift or no local drawer with Standard shift, then error out.
                            return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.SHIFT_NOT_ALLOWED_ON_ACTIVE_HARDWARE_PROFILE)]);
                        }

                        // Otherwise we allow resuming/opening one shift per terminal without drawer.
                    } else {
                        return this.getCashDrawerSelection(cashDrawers)
                            .done((drawerSelection: CashDrawerInputCancelableResult) => {
                                if (drawerSelection.canceled) {
                                    asyncQueue.cancel();
                                } else {
                                    shiftCashDrawer = drawerSelection.data;
                                }
                            });
                    }
                } else {
                    shiftCashDrawer = ArrayExtensions.firstOrUndefined(
                        this.getCashDrawersFromProfile(),
                        (drawer: Proxy.Entities.HardwareProfileCashDrawer) => drawer.DeviceName === shift.CashDrawer);

                    if (ObjectExtensions.isNullOrUndefined(shiftCashDrawer)) {
                        VoidAsyncResult.createRejected();
                    }
                }

                return VoidAsyncResult.createResolved();
            }).enqueue(() => {
                shift.CashDrawer = shiftCashDrawer.DeviceName;

                if (shift.StatusValue === Proxy.Entities.ShiftStatus.Open) {
                    return this.useShiftAsync(shift);
                } else if (shift.StatusValue === Proxy.Entities.ShiftStatus.Suspended) {
                    return this.resumeShiftAsync(shift);
                }

                return VoidAsyncResult.createResolved();
            });

            return asyncQueue;
        }

        /**
         * Get cash drawers selection from user.
         *
         * @param {Proxy.Entities.HardwareProfileCashDrawer[]} cashDrawers Available cash drawers for selection.
         * @return {Proxy.Entities.HardwareProfileCashDrawer} The user selected cash drawer.
         */
        private getCashDrawerSelection(cashDrawers: Proxy.Entities.HardwareProfileCashDrawer[])
            : IAsyncResult<ICancelableDataResult<Proxy.Entities.HardwareProfileCashDrawer>> {

            var result: CashDrawerInputCancelableResult = { canceled: false, data: null };
            var asyncResult: AsyncResult<ICancelableDataResult<Proxy.Entities.HardwareProfileCashDrawer>>
                = new AsyncResult<{ canceled: boolean; data: any }>();
            var activity: Activities.GetCashDrawerActivity
                = new Activities.GetCashDrawerActivity({ availableCashDrawers: cashDrawers });

            activity.execute().done(() => {
                if (ObjectExtensions.isNullOrUndefined(activity.response)) {
                    result.canceled = true;
                } else {
                    result.data = activity.response.cashDrawer;
                }

                asyncResult.resolve(result);
            });

            return asyncResult;
        }

        /**
         * Gets an auto-reusable shift given the staff identifier.
         *
         * @param {Proxy.Entities.Shift[]} shifts The shifts collection to look for an auto-reusable shift.
         * @param {string} staffId The staff identifier.
         * @return {Proxy.Entities.Shift} The auto-reusable shift or null if none found.
         */
        private getAutoReusableShift(shifts: Proxy.Entities.Shift[], staffId: string): Proxy.Entities.Shift {
            var autoReusableShifts: Proxy.Entities.Shift[] = shifts.filter((shift: Proxy.Entities.Shift) => {
                var canUse: boolean = shift.StaffId === staffId || shift.CurrentStaffId === staffId;
                if (shift.IsShared) {
                    canUse = canUse && (shift.CurrentTerminalId === this.applicationContext.deviceConfiguration.TerminalId);
                }

                return canUse;
            });

            if (autoReusableShifts.length === 1) {
                return autoReusableShifts[0];
            }

            return null;
        }

        /**
         * Gets the available cash drawers so that a shift can be opened on.
         *
         * @param {Proxy.Entities.HardwareProfileCashDrawer[]} allCashDrawers All of the cash drawers of hardware profile used by current terminal.
         * @param {Proxy.Entities.Shift[]} allOpenedShifts All opened shits.
         * @param {boolean} isManager A boolean value indicating whether or not the current employee is a manager.
         * @param {Proxy.Entities.EmployeePermissions} permissions The permissions of current employee.
         */
        private getAvailableCashDrawer(
            allCashDrawers: Proxy.Entities.HardwareProfileCashDrawer[],
            allOpenedShifts: Proxy.Entities.Shift[],
            isManager: boolean,
            permissions: Proxy.Entities.EmployeePermissions): Proxy.Entities.HardwareProfileCashDrawer[] {

            // Cash drawers that do not have shifts opened on.
            var availableCashDrawers: Proxy.Entities.HardwareProfileCashDrawer[] = [];

            if (ObjectExtensions.isNullOrUndefined(allCashDrawers) ||
                !ArrayExtensions.hasElements(allCashDrawers)) {
                return availableCashDrawers;
            } else {
                availableCashDrawers = allCashDrawers.filter((drawer: Proxy.Entities.HardwareProfileCashDrawer) => {
                    if (allOpenedShifts.some((s: Proxy.Entities.Shift) => s.CashDrawer === drawer.DeviceName)) {
                        // drawer is not available for open shift (already has a shift open on it)
                        return false;
                    }

                    // Do not show drawer that would open or use shared shift if user does not have permissions.
                    if (drawer.IsSharedShiftDrawer && !isManager && !permissions.AllowManageSharedShift && !permissions.AllowUseSharedShift) {
                        return false;
                    }

                    return true;
                });

                return availableCashDrawers;
            }
        }

        /**
         * Get all cash drawers with device names from the hardware profile
         *
         * @return {Proxy.Entities.HardwareProfileCashDrawer[]} The list of valid cash drawers
         */
        private getCashDrawersFromProfile(): Proxy.Entities.HardwareProfileCashDrawer[] {
            var drawers: Proxy.Entities.HardwareProfileCashDrawer[] = [];

            this.applicationContext.hardwareProfile.CashDrawers.forEach((cashDrawer: Proxy.Entities.HardwareProfileCashDrawer) => {
                if (!StringExtensions.isNullOrWhitespace(cashDrawer.DeviceName) && cashDrawer.DeviceTypeValue !== Proxy.Entities.PeripheralType.None) {
                    drawers.push(cashDrawer);
                }
            });

            // If there is no drawer configured in HQ, then locally assume one for opening one shift.
            if (!ArrayExtensions.hasElements(drawers)) {
                drawers.push(new Proxy.Entities.HardwareProfileCashDrawerClass({
                    DeviceTypeValue: Proxy.Entities.PeripheralType.None,
                    DeviceName: StringExtensions.EMPTY
                }));
            }

            return drawers;
        }
    }
}