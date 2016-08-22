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

    export interface OrderType {
        orderMode: Model.Entities.CustomerOrderMode;
        operationName: string;
    }

    export interface GetOrderTypeDialogOptions {
        operationId: Operations.RetailOperation;
    }

    export class GetOrderTypeDialog extends ModalDialog<GetOrderTypeDialogOptions, Model.Entities.CustomerOrderMode> {

        // Set Sales Person  objects
        private _orderTypeList: ObservableArray<OrderType>;
        private _customerOrderTypeButton: OrderType;
        private _quotationOrderTypeButton: OrderType;

        /**
         * Initializes a new instance of the GetOrderTypeDialog class.
         *
         */
        constructor() {
            super();

            this._customerOrderTypeButton = {
                orderMode: Model.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit,
                operationName: ViewModelAdapter.getResourceString("string_7131")
            };

            this._quotationOrderTypeButton = {
                orderMode: Model.Entities.CustomerOrderMode.QuoteCreateOrEdit,
                operationName: ViewModelAdapter.getResourceString("string_7132")
            };

            this._orderTypeList = ko.observableArray([]);
        }

        /**
         * Shows the dialog.
         *
         * @param {GetOrderTypeDialogOptions} options The order type options.
         */
        public onShowing(options: GetOrderTypeDialogOptions) {
            //sanitize input
            options = options || { operationId: 0 };
            
            switch (options.operationId) {
                case Operations.RetailOperation.PickupAllProducts:
                case Operations.RetailOperation.PickupSelectedProducts:
                case Operations.RetailOperation.ShipAllProducts:
                case Operations.RetailOperation.ShipSelectedProducts:
                case Operations.RetailOperation.RecalculateCustomerOrder:
                case Operations.RetailOperation.SalesPerson:
                    this._orderTypeList([this._customerOrderTypeButton, this._quotationOrderTypeButton]);
                    break;
                case Operations.RetailOperation.SetQuotationExpirationDate:
                    this._orderTypeList([this._quotationOrderTypeButton]);
                    break;
                default:
                    throw "Operation id not supported: " + options.operationId;
            }

            this.indeterminateWaitVisible(false);
            this.visible(true);
        }

        /**
         * Dialog button handler
         */
        public dialogButtonClick(operationId: string) {
            //Only cancel button available on this button section.
            this.dialogResult.resolve(DialogResult.Cancel);
        }

        /**
         * Handle after user clicks any of the dialog buttons.
         * 
         * @param {TileList.IItemInvokedArgs} eventArgs The button event that user clicked from the dialog.
         */
        public orderTypeListInvokedHandler(eventArgs: Commerce.TileList.IItemInvokedArgs): void {
            var buttonControl: OrderType = eventArgs.data;
            var customerOrderMode: Model.Entities.CustomerOrderMode = buttonControl.orderMode;

            this.dialogResult.resolve(DialogResult.OK, customerOrderMode);
        }
    }
}