/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='CommerceTypes.g.ts'/>

module Commerce.Proxy.Entities {

    /**
     * Class for displaying SalesOrder in search
     */
    export class SalesOrderWrapper {
        public salesOrder: SalesOrder;

        constructor(salesOrder: SalesOrder) {
            this.salesOrder = salesOrder;
        }

        private get customerOrderTypeString(): string {
            switch (this.salesOrder.CustomerOrderTypeValue) {
                case Model.Entities.CustomerOrderType.Quote:
                    return Commerce.ViewModelAdapter.getResourceString("string_4569"); // Quote
                case Model.Entities.CustomerOrderType.SalesOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4568"); // Customer order
                default:
                    return '';
            }
        }

        private get transactionTypeString(): string {
            switch (this.salesOrder.TransactionTypeValue) {
                case SalesTransactionType.CustomerOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4515"); // Customer order
                case SalesTransactionType.IncomeExpense:
                    return Commerce.ViewModelAdapter.getResourceString("string_4516"); // Income expense
                case SalesTransactionType.None:
                    return Commerce.ViewModelAdapter.getResourceString("string_4517"); // None
                case SalesTransactionType.PendingSalesOrder:
                    return Commerce.ViewModelAdapter.getResourceString("string_4518"); // Pending sales order
                case SalesTransactionType.Sales:
                    return Commerce.ViewModelAdapter.getResourceString("string_4519"); // Sales
                default:
                    return '';
            }
        }

        private get orderStatusString(): string {
            return SalesOrderWrapper.getOrderStatusString(this.salesOrder.StatusValue);
        }

        private get documentStatusString(): string {
            return this.getDocumentStatusString(this.salesOrder.DocumentStatusValue);
        }
        
        /**
         * Gets the order status string based on the SalesStatus value.
         * @param {SalesStatus} status The sales status.
         * @return {string} The mapped description string based on SalesStatus value.
         */
        public static getOrderStatusString(status: SalesStatus): string {

            if (ObjectExtensions.isNullOrUndefined(status)) {
                status = SalesStatus.Unknown;
            }

            switch (status) {
                case SalesStatus.Unknown:
                case SalesStatus.Created:
                    return Commerce.ViewModelAdapter.getResourceString("string_4521"); // Created
                case SalesStatus.Processing:
                    return Commerce.ViewModelAdapter.getResourceString("string_4522"); // Processing
                case SalesStatus.Delivered:
                    return Commerce.ViewModelAdapter.getResourceString("string_4523"); // Delivered
                case SalesStatus.Invoiced:
                    return Commerce.ViewModelAdapter.getResourceString("string_4524"); // Invoiced
                case SalesStatus.Confirmed:
                    return Commerce.ViewModelAdapter.getResourceString("string_4525"); // Confirmed
                case SalesStatus.Sent:
                    return Commerce.ViewModelAdapter.getResourceString("string_4526"); // Sent
                case SalesStatus.Canceled:
                    return Commerce.ViewModelAdapter.getResourceString("string_4527"); // Canceled
                case SalesStatus.Lost:
                    return Commerce.ViewModelAdapter.getResourceString("string_4528"); // Lost
                default:
                    return Commerce.ViewModelAdapter.getResourceString("string_4520"); // Unknown;
            }
        }

        private getDocumentStatusString(documentStatus: DocumentStatus): string {
            var documentToSalesStatus: SalesStatus = SalesOrderWrapper.convertDocumentToSalesStatus(documentStatus);
            return SalesOrderWrapper.getOrderStatusString(documentToSalesStatus);
        }

        /**
         * Converts document status to sales status.
         *
         * @param {Model.Entities.DocumentStatus} documentStatus The document status.
         * @return {Model.Entities.SalesStatus} The sales status.
         */
        public static convertDocumentToSalesStatus(documentStatus: Model.Entities.DocumentStatus): Model.Entities.SalesStatus {
            switch (documentStatus) {
                case Model.Entities.DocumentStatus.None:
                    return Model.Entities.SalesStatus.Created;
                case Model.Entities.DocumentStatus.PickingList:
                    return Model.Entities.SalesStatus.Processing;
                case Model.Entities.DocumentStatus.PackingSlip:
                    return Model.Entities.SalesStatus.Delivered;
                case Model.Entities.DocumentStatus.Invoice:
                    return Model.Entities.SalesStatus.Invoiced;
                case Model.Entities.DocumentStatus.Confirmation:
                    return Model.Entities.SalesStatus.Confirmed;
                case Model.Entities.DocumentStatus.Canceled:
                    return Model.Entities.SalesStatus.Canceled;
                case Model.Entities.DocumentStatus.Lost:
                    return Model.Entities.SalesStatus.Lost;
                default:
                    return Model.Entities.SalesStatus.Unknown;
            }
        }
    }
}