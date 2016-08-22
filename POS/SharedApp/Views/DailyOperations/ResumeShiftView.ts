/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Options passed to the ResumeShiftViewController constructor.
     */
    export interface IResumeShiftViewControllerOptions {
        /**
         * Async result to be resolved whenever a shift is selected.
         */
        onShiftSelected: (shift: Proxy.Entities.Shift) => IVoidAsyncResult;
        availableShiftActions: Proxy.Entities.AvailableShiftActions;
    }

    export class ResumeShiftViewController extends ViewControllerBase {
        public commonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;
        public availableShifts: ObservableArray<Proxy.Entities.Shift>;
        public isShiftSelected: Observable<boolean>;

        private _selectedShift: Proxy.Entities.Shift;
        private _options: IResumeShiftViewControllerOptions;

        constructor(options?: any) {
            super(true);

            this._options = options || { onShiftSelected: null, availableShiftActions: null };
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.indeterminateWaitVisible = ko.observable(false);
            this.availableShifts = ko.observableArray(<Proxy.Entities.Shift[]>[]);

            this.isShiftSelected = ko.observable(false);

            // Load Common Header
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4042"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4053"));
        }

        public load(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._options.availableShiftActions)) {
                if (ArrayExtensions.hasElements(this._options.availableShiftActions.reusableShifts)) {
                    this._options.availableShiftActions.reusableShifts.forEach((shiftValue, shiftIndex, shiftArray) => {
                        this.availableShifts.push(shiftValue);
                    });
                }

                if (ArrayExtensions.hasElements(this._options.availableShiftActions.suspendedShifts)) {
                    this._options.availableShiftActions.suspendedShifts.forEach((shiftValue, shiftIndex, shiftArray) => {
                        this.availableShifts.push(shiftValue);
                    });
                }
            }
        }

        public shiftSelectionChangedHandler(shifts: Proxy.Entities.Shift[]): void {
            this._selectedShift = shifts[0];
                
            // Enable the OK button
            this.isShiftSelected(!ObjectExtensions.isNullOrUndefined(this._selectedShift));
        }

        public useExistingShift(): void {
            if (this._selectedShift && this._options.onShiftSelected) {
                this.indeterminateWaitVisible(true);
                this._options.onShiftSelected(this._selectedShift)
                    .always(() => {
                        this.indeterminateWaitVisible(false);
                    });
            }
        }

        public cancelUseExistingShift(): void {
            if (this._options.onShiftSelected) {
                this._options.onShiftSelected(null);
            }
        }

        private onHidden() {
            if (this._options.onShiftSelected) {
                this._options.onShiftSelected(null);
            }
        }
    }
}
