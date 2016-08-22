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

    export interface GetSalesPersonDialogOptions {
        salesPersons?: Model.Entities.Employee[];
        originalSalesPersonId?: string;
    }

    export class GetSalesPersonDialog extends ModalDialog<GetSalesPersonDialogOptions, string> {

        // Set Sales Person  objects
        private _isOKButtonDisabled: Computed<boolean>;
        private _employees: Observable<Model.Entities.Employee[]>;
        private _staffId: Observable<string>;

        /**
         * Initializes a new instance of the GetSalesPersonDialog class.
         *
         * @param {GetSalesPersonDialogOptions} options Setup data for the dialog.
         */
        constructor() {
            super();

            this._staffId = ko.observable(StringExtensions.EMPTY);
            this._employees = ko.observable([]);

            this._isOKButtonDisabled = ko.computed(() => {
                return this._employees().length <= 0;
            });
        }

        /**
         * Shows the dialog.
         *
         * @param {Commerce.Model.Entities.Cart} cart The cart to change sales person.
         */
        public onShowing(options: GetSalesPersonDialogOptions) {
            //sanitize input
            options = options || {};
            options.salesPersons = options.salesPersons || [];
            options.originalSalesPersonId = options.originalSalesPersonId || StringExtensions.EMPTY;

            this._employees(options.salesPersons);
            var currentEmployees: Model.Entities.Employee[] = this._employees();
            var staff: Model.Entities.Employee;
            for (var i = 0; i < currentEmployees.length; i++) {
                staff = currentEmployees[i];
                if (!ObjectExtensions.isNullOrUndefined(staff) && staff.StaffId === options.originalSalesPersonId) {
                    this._staffId(options.originalSalesPersonId);
                    break;
                }
            }

            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Dialog button handler
         */
        public dialogButtonClick(operationId: string) {
            switch (operationId) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.dialogResult.resolve(DialogResult.OK, this._staffId());
                    break;
                default:
                    this.dialogResult.resolve(DialogResult.Cancel);
                    break;
            }
        }
    }
}