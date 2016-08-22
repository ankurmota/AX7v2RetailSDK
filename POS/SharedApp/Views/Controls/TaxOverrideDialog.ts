/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ModalDialog.ts'/>

module Commerce.Controls {
    "use strict";

    export interface ITaxOverrideDialogState {
        overrideType: Model.Entities.TaxOverrideBy;
        taxOverrides: Model.Entities.TaxOverride[];
    }

    export class TaxOverrideDialog extends ModalDialog<ITaxOverrideDialogState, Model.Entities.TaxOverride> {

        public taxOverrides: ObservableArray<Model.Entities.TaxOverride>;
        public overrideType: Model.Entities.TaxOverrideBy;

        /**
         * Initializes a new instance of the TaxOverrideDialog class.
         */
        constructor() {
            super();

            this.title(Commerce.ViewModelAdapter.getResourceString("string_4130"));
            this.taxOverrides = ko.observableArray([]);
        }

        /**
         * Shows the dialog.
         *
         * @param {TaxOverrideDialogState} dialogState The dialog state.
         */
        public onShowing(dialogState: ITaxOverrideDialogState): void {
            this.overrideType = dialogState.overrideType;
            this.taxOverrides(dialogState.taxOverrides);
            this.visible(true);
        }

        /**
         * Applies the selected tax code override to the transaction/line
         *
         * @param {Commerce.TileList.IItemInvokedArgs} eventArgs The event data and selected tax code
         */
        public itemInvokedHandler(eventArgs: Commerce.TileList.IItemInvokedArgs): void {
            var override: Model.Entities.TaxOverride = eventArgs.data;

            this.dialogResult.resolve(DialogResult.OK, override);
        }

        /**
         * Cancels the dialog without applying a tax override
         */
        public cancelDialog(): void {
            this.dialogResult.resolve(DialogResult.Cancel);
        }
    }
}