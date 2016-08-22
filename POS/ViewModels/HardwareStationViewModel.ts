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
     * Represents the hardware station view model.
     */
    export class HardwareStationViewModel extends ViewModelBase {
        public hardwareStations: ObservableArray<Model.Entities.HardwareStation>;
        public selectedHardwareStation: Observable<Model.Entities.HardwareStation>;

        /**
         * Constructs a hardware station view model.
         */
        constructor() {
            super();

            this.hardwareStations = ko.observableArray(<Model.Entities.HardwareStation[]>[]);
            this.selectedHardwareStation = ko.observable(null);
        }

        /**
         * Loads the view model.
         */
        public load(): void {
            this.updateDataSet();
        }

        /**
         * Sets the selected hardware station as active.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public setActive(): IAsyncResult<ICancelableResult> {
            var selectedHardwareStation: Model.Entities.HardwareStation = this.selectedHardwareStation();

            var options: Operations.ISelectHardwareStationOperationOptions = {
                hardwareStation: selectedHardwareStation, isInactivate: false
            };

            // Running without permissions check because this page will NOT show otherwise.
            // A manager override, if needed, has already been performed at this stage.
            return this.operationsManager.runOperationWithoutPermissionsCheck(Operations.RetailOperation.ChangeHardwareStation, options)
                .done(() => {
                    this.updateDataSet();

                    // On user executed change of hardware station connection state,
                    // set the toggle to allow display of the error message when the payment terminal cannot be opened
                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction, true);
                });
        }

        /**
         * Sets the selected hardware station as inactive.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public setInactive(): IAsyncResult<ICancelableResult> {
            var options: Operations.ISelectHardwareStationOperationOptions = {
                hardwareStation: this.selectedHardwareStation(), isInactivate: true
            };

            // Running without permissions check because this page will NOT show otherwise.
            // A manager override, if needed, has already been performed at this stage.
            return this.operationsManager.runOperationWithoutPermissionsCheck(Operations.RetailOperation.ChangeHardwareStation, options)
                .done(() => {
                    this.updateDataSet();

                    // On user executed change of hardware station connection state, 
                    // set the toggle to allow display of the error message when the payment terminal cannot be opened
                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction, true);
                });
        }

        /**
         * Initiate the pairing with the selected hardware station.
         * @return {IVoidAsyncResult} The async result.
         */
        public pairStation(): IVoidAsyncResult {
            var options: Operations.IPairHardwareStationOperationOptions = {
                hardwareStation: this.selectedHardwareStation(), pair: true
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.PairHardwareStation, options)
                .done(() => {
                    this.updateDataSet();

                    // On user executed change of hardware station connection state,
                    // set the toggle to allow display of the error message when the payment terminal cannot be opened
                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction, true);
                });
        }

        /**
         * Unpair with the selected hardware station.
         * @return {IVoidAsyncResult} The async result.
         */
        public unpairStation(): IVoidAsyncResult {
            var options: Operations.IPairHardwareStationOperationOptions = {
                hardwareStation: this.selectedHardwareStation(), pair: false
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.PairHardwareStation, options)
                .done(() => {
                    this.updateDataSet();

                    // On user executed change of hardware station connection state,
                    // set the toggle to allow display of the error message when the payment terminal cannot be opened
                    Session.instance.setErrorDisplayState(Commerce.ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction, true);
                });
        }

        private updateDataSet(): void {
            this.hardwareStations.removeAll();

            Commerce.ApplicationContext.Instance.hardwareStationProfileAsync.done((hardwareStationProfiles: Model.Entities.HardwareStationProfile[]) => {
                var activeHardwareStation: Model.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();
                var hasActiveHardwareStation: boolean = !ObjectExtensions.isNullOrUndefined(activeHardwareStation);

                for (var i: number = 0; i < hardwareStationProfiles.length; i++) {
                    var profile: Model.Entities.HardwareStationProfile = hardwareStationProfiles[i];

                    var hardwareStationUrl: string = Peripherals.HardwareStation.HardwareStationContext.getHardwareStationUrlFromProfile(profile);

                    var hardwareStation: Model.Entities.HardwareStation = {
                        RecordId: profile.RecordId,
                        HostName: profile.HardwareStationHostName,
                        Description: profile.HardwareStationDescription,
                        Url: hardwareStationUrl,
                        IsActive: false,
                        IsPaired: false,
                        ProfileId: profile.HardwareProfileId,
                        EftTerminalId: profile.HardwareStationEftTerminalId,
                        HardwareConfigurations: profile.HardwareConfigurations
                    };

                    if (Peripherals.HardwareStation.HardwareStationContext.isLocalStation(hardwareStation)) {
                        hardwareStation.IsPaired = true;
                    } else {
                        hardwareStation.IsPaired = HardwareStationEndpointStorage.hasHardwareStationToken(hardwareStation.RecordId, hardwareStation.Url);
                    }

                    hardwareStation.IsActive = hardwareStation.IsPaired
                                                && hasActiveHardwareStation
                                                && (hardwareStation.RecordId === activeHardwareStation.RecordId);

                    this.hardwareStations.push(hardwareStation);
                }
            });
        }
    }
}
