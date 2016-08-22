/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../IAsyncResult.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../ISalesOrderManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class SalesOrderManager implements ISalesOrderManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get a page of records.
         * @param {Entities.TransactionSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.Transaction[]>} The async result.
         */
        public getPagedJournalTransactions(
            searchCriteria: Entities.TransactionSearchCriteria,
            pageSize: number,
            skip: number): IAsyncResult<Entities.Transaction[]> {
            if (!ObjectExtensions.isNullOrUndefined(searchCriteria)) {
                var query: Proxy.StoreOperationsDataServiceQuery = this._commerceContext.storeOperations();
                query.top(pageSize).skip(skip);

                return query.searchJournalTransactions(searchCriteria).execute<Entities.Transaction[]>();
            }

            return AsyncResult.createResolved<Entities.Transaction[]>([]);
        }

        /**
         * Get a page of records.
         * @param {Entities.SalesOrderSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip The number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getPagedSalesOrders(searchCriteria: Entities.SalesOrderSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]> {

            if (!ObjectExtensions.isNullOrUndefined(searchCriteria)) {
                var query: Proxy.SalesOrdersDataServiceQuery = this._commerceContext.salesOrders();
                query.top(pageSize).skip(skip);
                return query.search(searchCriteria).execute<Entities.SalesOrder[]>();
            }
            return AsyncResult.createResolved<Entities.SalesOrder[]>([]);
        }


        /**
         * Get all orders for a customer.
         * @param {string} customerId The id of the customer to have orders returned.
         * @param {number} pageSize The desired number of search results.
         * @param {number} skip The The number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getPagedSalesOrdersByCustomerIdAsync(customerId: string, pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]> {

            if (!StringExtensions.isNullOrWhitespace(customerId)) {
                var query: Proxy.CustomersDataServiceQuery = this._commerceContext.customers(customerId);
                query.top(pageSize).skip(skip);

                return query.getOrderHistory().execute<Entities.SalesOrder[]>();
            }

            return AsyncResult.createResolved<Entities.SalesOrder[]>([]);
        }

        /**
         * Get all orders for a customer.
         * @param {string} customerId The identifier of the customer to have orders returned.
         * @param {number} [desiredResultCount] The desired number of search results.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getSalesOrdersByCustomerIdAsync(customerId: string, desiredResultCount?: number): IAsyncResult<Entities.SalesOrder[]> {
            var query: Proxy.CustomersDataServiceQuery = this._commerceContext.customers(customerId);
            if (!ObjectExtensions.isNullOrUndefined(desiredResultCount)) {
                query.top(desiredResultCount);
            }

            return query.getOrderHistory().execute<Entities.SalesOrder[]>();
        }

        /**
         * Get orders by receipt identifier.
         * @param {string} receiptId The receipt identifier of the orders to be retrieved.
         * @param {string} orderStoreNumber The store number where the order was created. Optional parameter, set to null or empty.
         * @param {string} orderTerminalId The terminal where the order was created. Optional parameter, set to null or empty.
         * @param {number[]} [transactionTypeValues] The type of transactions to be searched.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getOrdersByReceiptIdAsync(
            receiptId: string,
            orderStoreNumber: string,
            orderTerminalId: string,
            transactionTypeValues?: number[]): IAsyncResult<Entities.SalesOrder[]> {

            var searchCriteria: Model.Entities.SalesOrderSearchCriteria = {
                StoreId: orderStoreNumber,
                TerminalId: orderTerminalId,
                ReceiptId: receiptId,
                IncludeDetails: true
            };

            if (!ObjectExtensions.isNullOrUndefined(transactionTypeValues)) {
                searchCriteria.SalesTransactionTypeValues = transactionTypeValues;
            }

            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders().search(searchCriteria);
            return request.execute<Entities.SalesOrder[]>();
        }

        /**
         * Get reason code by reason code identifier.
         * @param {string} reasonCodeId The reason code identifier of the reason code to be retrieved.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result.
         */
        public getReasonCodeAsync(reasonCodeId: string): IAsyncResult<Entities.ReasonCode> {
            return this.getReasonCodesByIdAsync(reasonCodeId).map((reasonCodes: Entities.ReasonCode[]): Entities.ReasonCode => {
                var reasonCode: Entities.ReasonCode = null;
                if (ArrayExtensions.hasElements(reasonCodes)) {
                    reasonCode = reasonCodes[0];
                }

                return reasonCode;
            });
        }

        /**
         * Get reason codes by reason code or reason code group identifier.
         * @param {string} reasonCodeOrGroupId The reason code or reason code group identifier.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result containing a collection (in case of a group) of reason codes.
         */
        public getReasonCodesByIdAsync(reasonCodeId: string): IAsyncResult<Entities.ReasonCode[]> {
            return this._commerceContext.storeOperations().getReasonCodesById(reasonCodeId).execute<Entities.ReasonCode[]>();
        }

        /**
         * Get the print receipt buffer for the sales order.
         * @param {string} identifier The order identifier.
         * @param {boolean} isCopy Is receipt is duplicate.
         * @param {number} receiptType The receipt type.
         * @param {boolean} isRemoteOrder Is remote order.
         * @param {number} [shiftId] The shift identifier associated with the receipt.
         * @param {string} [shiftTerminalId] The identifier of the terminal that creates the shift.
         * @param {boolean} [isPreview] Whether the receipt is for preview.
         * @param {boolean} [queryBySalesId] Whether the identifier passed in is a sales identifier.
         * @param {string} [hardwareProfileId] Hardware profile identifier for the hardware station if enabled.
         * @return {IAsyncResult<Entities.Receipt[]>} The async result.
         */
        public getReceiptsForPrintAsync(
            id: string,
            isCopy: boolean,
            receiptTypeId: number,
            isRemoteOrder: boolean,
            shiftId?: number,
            shiftTerminalId?: string,
            isPreview?: boolean,
            queryBySalesId?: boolean,
            hardwareProfileId?: string): IAsyncResult<Entities.Receipt[]> {

            var receiptShiftId: number = shiftId ? shiftId : 0;
            var receiptShiftTerminalId: string = StringExtensions.isNullOrWhitespace(shiftTerminalId) ? StringExtensions.EMPTY : shiftTerminalId;
            var receiptIsPreview: boolean = isPreview ? isPreview : false;
            var receiptQueryBySalesId: boolean = queryBySalesId ? queryBySalesId : false;

            var criteria: Entities.ReceiptRetrievalCriteria = new Entities.ReceiptRetrievalCriteriaClass({
                IsCopy: isCopy,
                ReceiptTypeValue: receiptTypeId,
                IsRemoteTransaction: isRemoteOrder,
                ShiftId: receiptShiftId,
                ShiftTerminalId: receiptShiftTerminalId,
                IsPreview: receiptIsPreview,
                QueryBySalesId: receiptQueryBySalesId,
                HardwareProfileId: hardwareProfileId
            });

            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders(id).getReceipts(criteria);

            return request.execute<Entities.Receipt[]>();
        }

        /**
         * Get sales orders by searchCriteria.
         * @param {Entities.SalesOrderSearchCriteria} searchCriteria The search criteria.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        public getSalesOrderBySearchCriteriaAsync(searchCriteria: Entities.SalesOrderSearchCriteria): IAsyncResult<Entities.SalesOrder[]> {
            var originalTransactionIds: string[] = searchCriteria.TransactionIds;
            if (ArrayExtensions.hasElements(originalTransactionIds)) {
                searchCriteria.TransactionIds = originalTransactionIds.filter((id: string) => !StringExtensions.isNullOrWhitespace(id));
            }

            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders().search(searchCriteria);
            return request.execute<Entities.SalesOrder[]>();
        }

        /**
         * Creates picking list.
         * @param {string} salesId The sales identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        public createPickingList(salesId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders().createPickingList(salesId);
            return request.execute();
        }

        /**
         * Creates packing slip.
         * @param {string} salesId The sales identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        public createPackingSlip(salesId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders().createPackingSlip(salesId);
            return request.execute();
        }

        /**
         * Get sales invoices' header by sales order identifier.
         * @param {string} salesId The sales order identifier.
         * @return {IAsyncResult<Entities.SalesInvoice[]>} The async result.
         */
        public getSalesInvoicesBySalesIdAsync(salesId: string): IAsyncResult<Entities.SalesInvoice[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.salesOrders().getInvoicesBySalesId(salesId);
            return request.execute<Entities.SalesInvoice[]>();
        }
    }
}
