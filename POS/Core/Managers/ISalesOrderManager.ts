/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var ISalesOrderManagerName: string = "ISalesOrderManager";
    

    export interface ISalesOrderManager {

        /**
         * Get a page of records.
         * @param {Entities.TransactionSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.Transaction[]>} The async result.
         */
        getPagedJournalTransactions(searchCriteria: Entities.TransactionSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.Transaction[]>;

        /**
         * Get a page of records.
         * @param {Entities.SalesOrderSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        getPagedSalesOrders(searchCriteria: Entities.SalesOrderSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]>;

        /**
         * Get all orders for a customer.
         * @param {string} customerId The id of the customer to have orders returned.
         * @param {number} pageSize The desired number of search results.
         * @param {number} skip The The number of records to skip.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        getPagedSalesOrdersByCustomerIdAsync(customerId: string, pageSize: number, skip: number): IAsyncResult<Entities.SalesOrder[]>;

        /**
         * Get all orders for a customer.
         * @param {string} customerId The identifier of the customer to have orders returned.
         * @param {number} [desiredResultCount] The desired number of search results.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        getSalesOrdersByCustomerIdAsync(customerId: string, desiredResultCount?: number): IAsyncResult<Entities.SalesOrder[]>;

        /**
         * Get orders by receipt identifier.
         * @param {string} receiptId The receipt identifier of the orders to be retrieved.
         * @param {string} orderStoreNumber The store number where the order was created. Optional parameter, set to null or empty.
         * @param {string} orderTerminalId The terminal where the order was created. Optional parameter, set to null or empty.
         * @param {number[]} [transactionTypeValues] The type of transactions to be searched.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        getOrdersByReceiptIdAsync(
            receiptId: string,
            orderStoreNumber: string,
            orderTerminalId: string,
            transactionTypeValues?: number[]): IAsyncResult<Entities.SalesOrder[]>;

        /**
         * Get reason code by reason code identifier.
         * @param {string} reasonCodeId The reason code identifier of the reason code to be retrieved.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result.
         */
        getReasonCodeAsync(reasonCodeId: string): IAsyncResult<Entities.ReasonCode>;

        /**
         * Get reason codes by reason code or reason code group identifier.
         * @param {string} reasonCodeId The reason code or reason code group identifier.
         * @return {IAsyncResult<Entities.ReasonCode>} The async result containing a collection (in case of a group) of reason codes.
         */
        getReasonCodesByIdAsync(reasonCodeId: string): IAsyncResult<Entities.ReasonCode[]>;

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
        getReceiptsForPrintAsync(
            id: string,
            isCopy: boolean,
            receiptTypeId: number,
            isRemoteOrder: boolean,
            shiftId?: number,
            shiftTerminalId?: string,
            isPreview?: boolean,
            queryBySalesId?: boolean,
            hardwareProfileId?: string): IAsyncResult<Entities.Receipt[]>;

        /**
         * Get sales orders by searchCriteria.
         * @param {Entities.SalesOrderSearchCriteria} searchCriteria The search criteria.
         * @return {IAsyncResult<Entities.SalesOrder[]>} The async result.
         */
        getSalesOrderBySearchCriteriaAsync(searchCriteria: Entities.SalesOrderSearchCriteria): IAsyncResult<Entities.SalesOrder[]>;

        /**
         * Creates picking list.
         * @param {string} salesId The sales identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        createPickingList(salesId: string): IVoidAsyncResult;

        /**
         * Creates packing slip.
         * @param {string} salesId The sales identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        createPackingSlip(salesId: string): IVoidAsyncResult;

        /**
         * Get sales invoices' header by sales order identifier.
         * @param {string} salesId The sales order identifier.
         * @return {IAsyncResult<Entities.SalesInvoice[]>} The async result.
         */
        getSalesInvoicesBySalesIdAsync(salesId: string): IAsyncResult<Entities.SalesInvoice[]>;
    }
}
