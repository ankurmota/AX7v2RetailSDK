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

    export class CashDrawerInputDialog extends ModalDialog<Proxy.Entities.HardwareProfileCashDrawer[], Proxy.Entities.HardwareProfileCashDrawer> {

        private _cashDrawers: ObservableArray<Proxy.Entities.HardwareProfileCashDrawer>;

        constructor() {
            super();

            this._cashDrawers = ko.observableArray(<Proxy.Entities.HardwareProfileCashDrawer[]>[]);
        }

        /**
         * Shows the dialog.
         *
         * @param {Proxy.Entities.HardwareProfileCashDrawer[]} availableCashDrawers The available cash drawers.
         */
        public onShowing(availableCashDrawers: Proxy.Entities.HardwareProfileCashDrawer[]) {
            if (!ArrayExtensions.hasElements(availableCashDrawers)) {
                this.dialogResult.resolve(DialogResult.Cancel);
                return;
            }

            if (availableCashDrawers.length === 1) {
                this.dialogResult.resolve(DialogResult.OK, availableCashDrawers[0]);
                return;
            }

            this._cashDrawers(availableCashDrawers);
            this.visible(true);
        }

        /**
         * Button click handler
         *
         * @param {string} buttonId The identifier of the button.
         */
        private buttonClickHandler(buttonId: string) {
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        private selectedCashDrawerHandler(eventArgs: Commerce.TileList.IItemInvokedArgs) {
            var drawer: Proxy.Entities.HardwareProfileCashDrawer = eventArgs.data;
            this.dialogResult.resolve(DialogResult.OK, drawer);
        }
    }
}