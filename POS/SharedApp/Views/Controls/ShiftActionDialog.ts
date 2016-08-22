/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>
///<reference path='../../Controls/tileList/knockout.tileList.ts'/>

module Commerce.Controls {
    "use strict";

    type ShiftActionLine = { value: string, Action: Activities.ShiftActionType };

    export class ShiftActionDialog extends ModalDialog<Proxy.Entities.AvailableShiftActions, Activities.ShiftActionType> {

        private shiftDialogMethods: ObservableArray<ShiftActionLine>;
        private shiftDialogTitle: Observable<string>;

        constructor() {
            super();

            this.shiftDialogMethods = ko.observableArray(<ShiftActionLine[]>[]);
            this.shiftDialogTitle = ko.observable(StringExtensions.EMPTY);
        }

        /**
         * Shows the dialog.
         *
         * @param {Proxy.Entities.AvailableShiftActions} shiftActions The available shift actions.
         */
        public onShowing(shiftActions: Proxy.Entities.AvailableShiftActions): void {
            if (ObjectExtensions.isNullOrUndefined(shiftActions)) {
                this.dialogResult.resolve(DialogResult.Cancel);
                return;
            }

            this.shiftDialogTitle(shiftActions.dialogTitle);
            this.shiftDialogMethods(this.getShiftDialogMethodsForDisplay(shiftActions));
            this.visible(true);
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        public buttonClickHandler(buttonId: string): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Item click handler
         *
         * @param {Commerce.TileList.IItemInvokedArgs} eventArgs The event arg.
         */
        public shiftDialogClickHandler(eventArgs: Commerce.TileList.IItemInvokedArgs): void {
            var action: Activities.ShiftActionType = eventArgs.data.Action;
            this.dialogResult.resolve(DialogResult.OK, action);
        }

        /**
         * Get the shift dialog methods in button grid format for display.
         *
         * @param {Proxy.Entities.HardwareProfileCashDrawer[]} availableCashDrawers The available cash drawers.
         * @return {Commerce.Proxy.Entities.ButtonGrid} The button grid containing an array of
         * shift methods index data in button grid button format.
         */
        private getShiftDialogMethodsForDisplay(shiftActions: Proxy.Entities.AvailableShiftActions): Array<ShiftActionLine> {
            // Get the shift methods to display
            var tileFields: ShiftActionLine[] = [];
            var tileField: ShiftActionLine;

            // Open new shift button
            if (this.getCanOpenShift(shiftActions)) {
                tileField = {
                    Action: Activities.ShiftActionType.NewShift,
                    value: ViewModelAdapter.getResourceString("string_4002")
                };
                tileFields.push(tileField);
            }

            // Use existing shift button
            if (ArrayExtensions.hasElements(shiftActions.reusableShifts)
                || ArrayExtensions.hasElements(shiftActions.suspendedShifts)) {
                tileField = {
                    Action: Activities.ShiftActionType.ExistingShift,
                    value: ViewModelAdapter.getResourceString("string_4035")
                };
                tileFields.push(tileField);
            }

            // Non-drawer mode button
            tileField = {
                Action: Activities.ShiftActionType.NonDrawer,
                value: ViewModelAdapter.getResourceString("string_4004")
            };
            tileFields.push(tileField);
            return tileFields;
        }

        private getCanOpenShift(shiftActions: Proxy.Entities.AvailableShiftActions): boolean {
            var result: boolean = false;
            var drawer: Proxy.Entities.HardwareProfileCashDrawer = ArrayExtensions.firstOrUndefined(shiftActions.availableCashDrawers);

            // Can open shift if there is any drawer available.
            // And If it is a shared drawer then no reusable shift should be available.
            if (!ObjectExtensions.isNullOrUndefined(drawer)
                && (!drawer.IsSharedShiftDrawer || !shiftActions.reusableShifts.some((shift: Proxy.Entities.Shift) => shift.IsShared))) {

                result = true;
            }

            return result;
        }
    }
}