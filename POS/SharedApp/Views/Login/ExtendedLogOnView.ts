/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * Represents the extended logon view controller.
     */
    export class ExtendedLogonViewController extends ViewControllerBase {

        public viewModel: ViewModels.ExtendedLogOnViewModel;
        public commonHeaderData: Controls.CommonHeaderData;

        constructor() {
            super(true);
            this.viewModel = new ViewModels.ExtendedLogOnViewModel(Peripherals.instance,
                Model.Managers.Factory,
                NotificationHandler.displayClientErrors,
                Commerce.ViewModelAdapter);
            this.initCommonHeaderData();
        }

        /**
         * Handler for a list view selections.
         * @param {Proxy.Entities.Employee[]} employees Selected employees.
         */
        public employeeSelectionChanged(employees: Proxy.Entities.Employee[]): void {
            this.viewModel.selectedEmployee(employees[0]);
        }

        /**
         * Handler for an assign dialog button clicks.
         * @param {string} operationId The operation id associated with the clicked button.
         */
        public assignDialogButtonClick(operationId: string): void {
            switch (operationId) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                    this.viewModel.assignConfirm();
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                    this.viewModel.assignCancel();
                    break;
            }
        }

        /**
         * Handler for an unassign dialog button clicks.
         * @param {string} operationId The operation id associated with the clicked button.
         */
        public unAssignDialogButtonClick(operationId: string): void {
              switch (operationId) {
                case Controls.Dialog.OperationIds.OK_BUTTON_CLICK:
                      this.viewModel.unassignConfirm();
                    break;
                case Controls.Dialog.OperationIds.CANCEL_BUTTON_CLICK:
                      this.viewModel.unassignCancel();
                    break;
            }
        }

        private initCommonHeaderData(): void {
            this.commonHeaderData = new Controls.CommonHeaderData();
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewCategoryName(false);
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.backButtonVisible(true);
            this.commonHeaderData.viewHeader(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_11000"));
            this.commonHeaderData.searchClick = () => {
                this.viewModel.search(this.commonHeaderData.searchText());
            };
        }
    }
}