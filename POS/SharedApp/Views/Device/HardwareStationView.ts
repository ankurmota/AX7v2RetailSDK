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

    /**
     * Options passed to the HardwareStationViewController constructor.
     */
    export interface HardwareStationViewControllerOptions {
        /**
         * Async result to be resolved whenever a hardware station is active.
         */
        onActiveInactive: AsyncResult<Operations.ISelectHardwareStationOperationOptions>;
        activeOnly: boolean;
    }

    export class HardwareStationViewController extends ViewControllerBase {
        public viewModel: Commerce.ViewModels.HardwareStationViewModel;
        public shiftViewModel: Commerce.ViewModels.ShiftViewModel;
        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;
        public isCommandEnable: Observable<boolean>;
        public isPairingEnable: Observable<boolean>;
        public isUnpairingEnable: Observable<boolean>;
        public isActiveOnly: Observable<boolean>;

        private _options: HardwareStationViewControllerOptions;

        constructor(options?: HardwareStationViewControllerOptions) {
            super(false);

            this._options = options || { onActiveInactive: undefined, activeOnly: false };
            this.viewModel = new Commerce.ViewModels.HardwareStationViewModel();
            this.shiftViewModel = new Commerce.ViewModels.ShiftViewModel();

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.indeterminateWaitVisible = ko.observable(false);
            this.isCommandEnable = ko.observable(false);
            this.isPairingEnable = ko.observable(false);
            this.isUnpairingEnable = ko.observable(false);
            this.isActiveOnly = ko.observable(this._options.activeOnly);

            //Load Common Header
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_6000"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_6001"));
        }

        public load(): void {
            this.viewModel.load();
        }

        public selectionChangedHandler(hardwareStations: Model.Entities.HardwareStation[]): void {
            var selectedHardwareStation = hardwareStations[0];

            this.viewModel.selectedHardwareStation(selectedHardwareStation);
            this.isCommandEnable(!ObjectExtensions.isNullOrUndefined(selectedHardwareStation));
            this.isPairingEnable(this.isCommandEnable() && (!Peripherals.HardwareStation.HardwareStationContext.isLocalStation(selectedHardwareStation)));
            this.isUnpairingEnable(this.isPairingEnable() && selectedHardwareStation.IsPaired);
        }

        public setActive(): void {
            this.handleAsyncResult(this.viewModel.setActive())
                .done((result) => {
                    if (!result.canceled) {
                        if (this._options.onActiveInactive) {
                            this._options.onActiveInactive.resolve({
                                hardwareStation: this.viewModel.selectedHardwareStation(), isInactivate: false
                            });
                        }

                        // In non-drawer mode, start the start shift workflow.
                        if (!Session.instance.Shift.ShiftId) {
                            this.startShift();
                        }
                    }
                }).fail((errors) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public setInactive(): void {
            this.handleAsyncResult(this.viewModel.setInactive())
                .done((result) => {
                    if (!result.canceled && this._options.onActiveInactive) {
                        this._options.onActiveInactive.resolve({
                            hardwareStation: this.viewModel.selectedHardwareStation(), isInactivate: true
                        });
                    }
                }).fail((errors) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        public pair(): void {
            this.handleAsyncResult(this.viewModel.pairStation())
                .fail((errors) => {
                    NotificationHandler.displayClientErrors(errors, 'string_6008');
                });
        }

        public unpair(): void {
            this.handleAsyncResult(this.viewModel.unpairStation())
                .fail((errors) => {
                    NotificationHandler.displayClientErrors(errors, 'string_6008');
                });
        }

        public skipSelection(): void {
            if (this._options.onActiveInactive) {
                this._options.onActiveInactive.resolve({
                    hardwareStation: null, isInactivate: false
                });
            }
        }

        private startShift(): void {
            this.handleAsyncResult(this.shiftViewModel.openOrResumeShift(Session.instance.CurrentEmployee.StaffId))
                .fail((errors) => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private handleAsyncResult<T>(asyncResult: IAsyncResult<T>): IAsyncResult<T> {
            this.updateUI(true);
            return asyncResult.always(() => { this.updateUI(false); });
        }

        private updateUI(enable: boolean) {
            this.indeterminateWaitVisible(enable);
            this.isCommandEnable(!enable);
        }

        private onHidden() {
            if (this._options.onActiveInactive) {
                this._options.onActiveInactive.resolve(null);
            }
        }
    }
}
