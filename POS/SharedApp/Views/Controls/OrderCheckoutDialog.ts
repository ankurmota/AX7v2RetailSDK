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

    export interface OrderCheckoutDialogState {
        salesOrder: Model.Entities.SalesOrder;
        salesPersonName: string;
        changeAmount: number;
        customerOrderMode: Model.Entities.CustomerOrderMode;
    }

    export class OrderCheckoutDialog extends ModalDialog<OrderCheckoutDialogState, any> {

        public salesOrder: Observable<Model.Entities.SalesOrder>;
        public salesPersonName: Observable<String>;
        public salesLinesCount: Computed<number>;
        public controlHeader:  Observable<string>;
        public customerOrderMode: Model.Entities.CustomerOrderMode;

        /**
         * Initializes a new instance of the TaxOverrideDialog class.
         */
        constructor() {
            super();

            this.salesOrder = ko.observable(new Model.Entities.SalesOrderClass());
            this.salesPersonName = ko.observable(StringExtensions.EMPTY);
            this.controlHeader = ko.observable(StringExtensions.EMPTY);
            this.customerOrderMode = Model.Entities.CustomerOrderMode.None;
            this.salesLinesCount = ko.computed((): number => {
                return ArrayExtensions.where(this.salesOrder().SalesLines, (salesLine: Model.Entities.SalesLine): boolean => {
                    return !salesLine.IsVoided && salesLine.Quantity !== 0;
                }).length;
            });
        }

        /**
         * Shows the dialog.
         *
         * @param {OrderCheckoutDialogState} dialogState The dialog state.
         */
        public onShowing(controlState: OrderCheckoutDialogState) {

            if (ObjectExtensions.isNullOrUndefined(controlState) || ObjectExtensions.isNullOrUndefined(controlState.salesOrder)) {
                RetailLogger.viewsControlsOrderCheckoutDialogOnShowingParametersUndefined();
                var error: Model.Entities.Error = new Model.Entities.Error("string_1622");
                this.dialogResult.reject([error]);
                return;
            }

            this.salesOrder(controlState.salesOrder);
            this.salesPersonName(controlState.salesPersonName);
            this.customerOrderMode = controlState.customerOrderMode;
            this.controlHeader(this.getDialogHeader());

            this.visible(true);
        }

        private dialogCloseHandler() {
            this.dialogResult.resolve(DialogResult.OK);
        }

        private getDialogHeader(): string {
            var orderTypeName: string;
            var orderOperationName: string;

            var orderHeaderFormatter: Function = (type: string , operation: string) => {
                return StringExtensions.format(ViewModelAdapter.getResourceString("string_1899"), type, operation);  // "{0} {1}"
            };

            switch (this.salesOrder().CustomerOrderTypeValue) {
                    case Model.Entities.CustomerOrderType.SalesOrder:
                        orderTypeName = Commerce.ViewModelAdapter.getResourceString("string_1871");  // Customer order
                    break;
                    case Model.Entities.CustomerOrderType.Quote:
                        orderTypeName = Commerce.ViewModelAdapter.getResourceString("string_1870");   // Quote
                    break;
            }

            switch (this.customerOrderMode) {
                case Model.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit:
                case Model.Entities.CustomerOrderMode.QuoteCreateOrEdit:
                    orderOperationName = Commerce.ViewModelAdapter.getResourceString("string_1880"); // created
                    break;
                case Model.Entities.CustomerOrderMode.Pickup:
                    orderOperationName = Commerce.ViewModelAdapter.getResourceString("string_1881"); // picked up
                    break;
                case Model.Entities.CustomerOrderMode.Return:
                    orderOperationName = Commerce.ViewModelAdapter.getResourceString("string_1882"); // returned
                    break;

                case Model.Entities.CustomerOrderMode.Cancellation:
                    orderOperationName = Commerce.ViewModelAdapter.getResourceString("string_1883"); // cancelled
                    break;
            }

            return (!StringExtensions.isNullOrWhitespace(orderTypeName) && !StringExtensions.isNullOrWhitespace(orderOperationName))
                ? orderHeaderFormatter(orderTypeName, orderOperationName)
                : StringExtensions.EMPTY;
        }
    }
}