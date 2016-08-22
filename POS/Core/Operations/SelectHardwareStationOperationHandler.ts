/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the SelectHardwareStation operation.
     */
    export interface ISelectHardwareStationOperationOptions {
        hardwareStation: Proxy.Entities.IHardwareStation;
        isInactivate: boolean;
    }

    /**
     * Handler for the SelectHardwareStation operation.
     */
    export class SelectHardwareStationOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SelectHardwareStation operation.
         *
         * @param {ISelectHardwareStationOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ISelectHardwareStationOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { hardwareStation: undefined, isInactivate: undefined };

            if (ArrayExtensions.hasElements(Session.instance.cart.TenderLines)) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.CANNOT_CHANGE_HARDWARE_STATION_WHEN_PAYMENT_DONE)]);
            }

            // If line display is currently active, don't allow the hardware station to be changed.
            if (Peripherals.instance && Peripherals.instance.lineDisplay && Peripherals.instance.lineDisplay.isActive) {
                return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.HARDWARESTATION_CHANGE_ERROR_LINE_DISPLAY_ACTIVE)]);
            }

            var asyncQueue = new AsyncQueue();
            var appContextLoader: Commerce.ApplicationContextLoader;
            var sharedDrawer: Proxy.Entities.HardwareProfileCashDrawer;
            var hardwareProfile: Proxy.Entities.HardwareProfile;

            // select hardware station if none provided
            if (!options.hardwareStation) {
                asyncQueue.enqueue(() => {
                    // skip it on inactivate
                    if (options.isInactivate) {
                        return VoidAsyncResult.createResolved();
                    }

                    var activity = new Activities.SelectHardwareStationActivity({ activeOnly: false });
                    activity.responseHandler = (response: Activities.SelectHardwareStationActivityResponse): IVoidAsyncResult => {
                        if (!Peripherals.HardwareStation.HardwareStationContext.isLocalStation(response.hardwareStation)
                            && !HardwareStationEndpointStorage.hasHardwareStationToken(response.hardwareStation.RecordId, response.hardwareStation.Url)) {
                            return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.HARDWARESTATION_MUST_BE_PAIRED_BEFORE_ACTIVATE)]);
                        }

                        return VoidAsyncResult.createResolved();
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        options = activity.response;
                    });
                });
            }

            asyncQueue
                .enqueue(() => {
                    // skip it on inactivate
                    if (options.isInactivate) {
                        return VoidAsyncResult.createResolved();
                    }

                    var hardwareStation: Proxy.Entities.IHardwareStation = options.hardwareStation;

                    if (!Peripherals.HardwareStation.HardwareStationContext.isLocalStation(hardwareStation)
                        && !HardwareStationEndpointStorage.getHardwareStationToken(hardwareStation.RecordId, hardwareStation.Url)) {
                        return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.HARDWARESTATION_MUST_BE_PAIRED_BEFORE_ACTIVATE)]);
                    }

                    return VoidAsyncResult.createResolved();
                }).enqueue(() => {
                    appContextLoader = new Commerce.ApplicationContextLoader();

                    // skip it on inactivate, non-drawer mode or non-shared shift.
                    if (options.isInactivate
                        || Session.instance.Shift.ShiftId === 0
                        || !Session.instance.Shift.IsShared) {
                        return VoidAsyncResult.createResolved();
                    }

                    // If current shift is shared then only allow switching when selected hardware station has a shared shift drawer.
                    return appContextLoader.getActiveHardwareProfileAsync(options.hardwareStation)
                        .done((profile: Proxy.Entities.HardwareProfile) => {
                            hardwareProfile = profile;
                            sharedDrawer = ArrayExtensions.firstOrUndefined(
                                profile.CashDrawers,
                                (cashDrawer: Proxy.Entities.HardwareProfileCashDrawer) => { return cashDrawer.IsSharedShiftDrawer; });
                        });
                }).enqueue(() => {
                    var nextStepAllowed: boolean = false;

                    if (options.isInactivate  // Inactivate operation
                        || Session.instance.Shift.ShiftId === 0 // Non-Drawer mode
                        || (Session.instance.Shift.IsShared && !ObjectExtensions.isNullOrUndefined(sharedDrawer)) // Shared shift with hardware station with Shared shift profile
                        || (!Peripherals.HardwareStation.HardwareStationContext.instance.isActive() // Standard shift with hardware station that has no profile and none currently active.
                            && !Session.instance.Shift.IsShared
                            && StringExtensions.isNullOrWhitespace(options.hardwareStation.ProfileId))) {
                        nextStepAllowed = true;
                    }

                    if (nextStepAllowed) {
                        return VoidAsyncResult.createResolved();
                    }

                    return VoidAsyncResult.createRejected([new Proxy.Entities.Error(ErrorTypeEnum.HARDWARESTATION_SWITCH_NOT_ALLOWED_TO_NONSHARED)]);
                }).enqueue(() => {
                    if (options.isInactivate) {
                        HardwareStationEndpointStorage.clearActiveHardwareStation();
                    } else {
                        HardwareStationEndpointStorage.setActiveHardwareStation(options.hardwareStation);
                    }
                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.HardwareStationGeneralError, true);
                    // Reload current hardware profile in application context.
                    return appContextLoader.loadActiveHardwareStationProfileAsync(hardwareProfile);
                }).enqueue(() => {
                    // skip it on inactivate or non-shared shift.
                    if (!options.isInactivate && !ObjectExtensions.isNullOrUndefined(sharedDrawer)) {
                        ApplicationStorage.setItem(ApplicationStorageIDs.CASH_DRAWER_NAME, sharedDrawer.DeviceName);
                        ApplicationStorage.setItem(ApplicationStorageIDs.CASH_DRAWER_TYPE, sharedDrawer.DeviceTypeValue.toString());
                    }

                    return VoidAsyncResult.createResolved();
                });

            return asyncQueue.run();
        }
    }
}