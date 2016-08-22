/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Entities = Proxy.Entities;

    /**
     * Represents the show journal view model.
     */
    export class ShowJournalViewModel extends ViewModelBase {

        private _transactionSearchCriteria: Entities.TransactionSearchCriteria;
        private _salesOrderSearchCriteria: Entities.SalesOrderSearchCriteria;
        private _customerId: string;

        constructor() {
            super();
        }

        /**
         * Returns the search transaction criteria.
         * @return {Entities.TransactionSearchCriteria} The search order criteria.
         */
        public get TransactionSearchCriteria(): Entities.TransactionSearchCriteria {
            return this._transactionSearchCriteria;
        }

        /**
         * Set search transaction criteria for paging.
         * @param {Entities.TransactionSearchCriteria} transactionSearchCriteria The search order criteria.
         */
        public set TransactionSearchCriteria(transactionSearchCriteria: Entities.TransactionSearchCriteria) {
            this._transactionSearchCriteria = transactionSearchCriteria;
        }

        /**
         * Set search order criteria for paging.
         * @param {Entities.SalesOrderSearchCriteria} The search order criteria.
         */
        public set OrderSearchCriteria(orderSearchCriteria: Entities.SalesOrderSearchCriteria) {
            this._salesOrderSearchCriteria = orderSearchCriteria;
        }

        /**
         * Set customer search order criteria for paging.
         * @param {{ customerId: string }} orderSearchCriteria The search order criteria.
         */
        public set CustomerId(customerId: string) {
            this._customerId = customerId;
        }

        /**
         * Get a page of records.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.Transaction[]>} The async result.
         */
        public getJournalTransactions(pageSize: number, skip: number): IAsyncResult<Entities.Transaction[]> {
            return this.salesOrderManager.getPagedJournalTransactions(this._transactionSearchCriteria, pageSize, skip)
                .fail((errors: Entities.Error[]): void => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Get a page of records.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip The number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getSalesOrders(pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]> {
            return this.salesOrderManager.getPagedSalesOrders(this._salesOrderSearchCriteria, pageSize, skip)
                .fail((errors: Entities.Error[]): void => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Get all orders for a customer.
         * @param {number} pageSize The desired number of search results.
         * @param {number} skip The The number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getSalesOrdersByCustomerIdAsync(pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]> {
            return this.salesOrderManager.getPagedSalesOrdersByCustomerIdAsync(this._customerId, pageSize, skip)
                .fail((errors: Entities.Error[]): void => {
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Get sales orders by searchCriteria
         * @param {Entities.SalesOrderSearchCriteria} searchCriteria The search criteria.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result containing the sales orders.
         */
        public getSalesOrderBySearchCriteria(searchCriteria: Entities.SalesOrderSearchCriteria): IAsyncResult<Entities.SalesOrder[]> {
            return this.salesOrderManager.getSalesOrderBySearchCriteriaAsync(searchCriteria);
        }

        /**
         * Gets the receipts that were generated for a sales order.
         * @param {Entities.SalesOrder} salesOrder: The sales order whose receipt is to be printed.
         * @param {Entities.ReceiptType} receiptType: The type of the receipt.
         * @param {boolean} isPreview: True: if the preview of the receipts is begin requested. False: otherwise.
         * @return {IAsyncResult<Entities.Receipt[]>} The async result with the receipts.
         */
        public getReceiptsForSalesOrder(salesOrder: Entities.SalesOrder, receiptType: Entities.ReceiptType, isPreview: boolean)
            : IAsyncResult<Entities.Receipt[]> {
            // deciding if this is local or remote order by matching transaction store id and client context store id
            var isRemoteOrder: boolean = salesOrder.StoreId !== ApplicationContext.Instance.storeNumber;

            // if TransactionId equals to SalesId, then we are actually querying the sales order by sales id.
            var queryBySalesId: boolean = StringExtensions.compare(salesOrder.Id, salesOrder.SalesId, true) === 0;
            return this.salesOrderManager.getReceiptsForPrintAsync(
                salesOrder.Id,
                true,
                receiptType,
                isRemoteOrder,
                null,
                null,
                isPreview,
                queryBySalesId,
                ApplicationContext.Instance.hardwareProfile.ProfileId);
        }
    }
}
