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

    export class BlindCloseViewController extends ViewControllerBase {
        public commonHeaderData: Controls.CommonHeaderData;
        public shiftViewModel: Commerce.ViewModels.ShiftViewModel;
        public indeterminateWaitVisible: Observable<boolean>;
        public blindClosedShifts: ObservableArray<Proxy.Entities.Shift>;

        private _toggleShowHideDeclareMenu: Observable<Function>;
        private _selectedShift: Observable<Proxy.Entities.Shift>;
        private _appBarVisible: Observable<boolean>;
        private _isShiftCommandDisabled: Computed<boolean>;

        constructor() {
            super(true);
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.shiftViewModel = new Commerce.ViewModels.ShiftViewModel();
            this.indeterminateWaitVisible = ko.observable(false);
            this.blindClosedShifts = ko.observableArray(<Proxy.Entities.Shift[]>[]);
            this._appBarVisible = ko.observable(false);
            this._selectedShift = ko.observable(null);

            // Load Common Header
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_2005"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_2114")); // taskview in resources.rejson
            this._toggleShowHideDeclareMenu = ko.observable(() => { return; });
            this._isShiftCommandDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedShift());
            }, this);
        }

        public load(): void {
            this.shiftViewModel.getBlindClosedShiftsAsync()
                .done((bcShifts: Proxy.Entities.Shift[]) => {
                    for (var n: number = 0; n < bcShifts.length; n++) {
                        this.blindClosedShifts.push(bcShifts[n]);
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        public declareClick(): void {
            this._toggleShowHideDeclareMenu()();
        }

        public shiftSelectionChangedHandler(shifts: Proxy.Entities.Shift[]): void {
            if (ArrayExtensions.hasElements(shifts)) {
                this._selectedShift(shifts[0]);
            } else {
                this._selectedShift(null);
            }

            this._appBarVisible(!ObjectExtensions.isNullOrUndefined(this._selectedShift()));
        }

        public declareStartAmount(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._selectedShift())) {
                var options: any = { nonSalesTenderType: Proxy.Entities.TransactionType.StartingAmount, shift: this._selectedShift() };
                Commerce.ViewModelAdapter.navigate("CashManagementView", options);
            }
        }

        public tenderDeclaration(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._selectedShift())) {
                this.indeterminateWaitVisible(true);

                var options: Operations.ITenderDeclarationOperationOptions = { shift: this._selectedShift() };

                Commerce.Operations.OperationsManager.instance.runOperation(Commerce.Operations.RetailOperation.TenderDeclaration, options)
                    .fail((errors: Model.Entities.Error[]) => {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    }).always((): void => {
                        this.indeterminateWaitVisible(false);
                    });
            }
        }

        public printX(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._selectedShift())) {
                this.indeterminateWaitVisible(true);

                var options: Operations.IPrintXOperationOptions = { shift: this._selectedShift() };

                Commerce.Operations.OperationsManager.instance.runOperation(Commerce.Operations.RetailOperation.PrintX, options)
                    .done(() => { this.indeterminateWaitVisible(false); })
                    .fail((errors: Proxy.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
            }
        }

        public closeShift(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._selectedShift())) {
                this.indeterminateWaitVisible(true);

                var options: Operations.ICloseShiftOperationOptions = { shift: this._selectedShift() };
                Commerce.Operations.OperationsManager.instance.runOperation(Operations.RetailOperation.CloseShift, options)
                    .done((result: ICancelableResult) => {
                        if (result.canceled) {
                            return;
                        }

                        // Remove the shift from the array client side
                        var _index: number = this.blindClosedShifts().indexOf(this._selectedShift());
                        this.blindClosedShifts().splice(_index, 1);
                        this.blindClosedShifts(this.blindClosedShifts());
                        this._selectedShift(null);
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    }).always((): void => { this.indeterminateWaitVisible(false); });
            }
        }
    }
}
