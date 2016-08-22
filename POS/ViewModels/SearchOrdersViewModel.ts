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

    import Entities = Model.Entities;

    /**
     * Represents the search orders view model.
     */
    export class SearchOrdersViewModel extends ViewModelBase {

        public static defaultSearchPeriodInDays: number = 90;

        constructor() {
            super();
        }

        /**
         * Get empty search criteria for SalesOrderSearch
         * @return {Entities.SalesOrderSearchCriteria} Returns empty search criteria
         */
        public static get emptySalesOrderSearchCriteria(): Entities.SalesOrderSearchCriteria {
            return new Entities.SalesOrderSearchCriteriaClass({
                TransactionIds: [],
                SalesTransactionTypeValues: [],
                IncludeDetails: false
            });
        }

        /**
         * Get default search criteria for transaction search
         * @return {Entities.TransactionSearchCriteria} Returns default search criteria
         */
        public static get defaultTransactionSearchCriteria(): Entities.TransactionSearchCriteria {
            var searchCriteria: Entities.TransactionSearchCriteria = new Entities.TransactionSearchCriteriaClass({
                TransactionIds: []
            });

            return searchCriteria;
        }

        /**
         * Get default search criteria for SalesOrderSearch
         * @return {Entities.SalesOrderSearchCriteria} Returns default search criteria
         */
        public static get defaultCustomerOrderSearchCriteria(): Entities.SalesOrderSearchCriteria {
            var searchCriteria: Entities.SalesOrderSearchCriteria = SearchOrdersViewModel.emptySalesOrderSearchCriteria;
            var startDate: Date = DateExtensions.getDate(DateExtensions.addDays(DateExtensions.now, -SearchOrdersViewModel.defaultSearchPeriodInDays));
            searchCriteria.StartDateTime = startDate;

            searchCriteria.SalesTransactionTypeValues.push(Entities.SalesTransactionType.CustomerOrder);
            searchCriteria.SalesTransactionTypeValues.push(Entities.SalesTransactionType.PendingSalesOrder);
            searchCriteria.SalesTransactionTypeValues.push(Entities.SalesTransactionType.AsyncCustomerOrder);
            searchCriteria.SalesTransactionTypeValues.push(Entities.SalesTransactionType.AsyncCustomerQuote);

            searchCriteria.SearchTypeValue = Entities.OrderSearchType.SalesOrder;
            searchCriteria.SearchLocationTypeValue = Entities.SearchLocation.Remote;
            return searchCriteria;
        }

        /**
         * Checks if the provided sales order available for provided operation
         * @param {Entities.SalesOrder} salesOrder The sales order
         * @param {Entities.CustomerOrderOperations} operationId Id of an operation
         * @return {boolean} Returns if the sales order available for an operation or not
         */
        public static isOrderAvailableForOperation(salesOrder: Entities.SalesOrder, operationId: Entities.CustomerOrderOperations): boolean {
            if (ObjectExtensions.isNullOrUndefined(salesOrder) || StringExtensions.isEmptyOrWhitespace(salesOrder.SalesId)) {
                return false;
            }

            // Quote is only available for editing.
            if (SearchOrdersViewModel.isQuote(salesOrder)) {
                return operationId === Entities.CustomerOrderOperations.Edit;
            }

            // invoiced (shipped/picked up at store) > delivered (packed) > processing (picked) > created
            // document status -> highest line status
            // sales status -> lowest line status

            var documentStatus: Entities.SalesStatus = Entities.SalesOrderWrapper.convertDocumentToSalesStatus(salesOrder.DocumentStatusValue);
            var salesStatus: Entities.SalesStatus = salesOrder.StatusValue;

            // consider order to be have items to be shipped if header delivery mode is not pickup at store
            var isShipping: boolean =
                StringExtensions.compare(salesOrder.DeliveryMode, ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode, true) !== 0;

            var isOrderCanceled: boolean = salesStatus === Entities.SalesStatus.Canceled;

            // when order is canceled, no operation can be done
            if (isOrderCanceled) {
                return false;
            }

            switch (operationId) {
                // only pack shipped orders - there must be at least one line created or picked
                case Entities.CustomerOrderOperations.CreatePackingSlip:
                    return isShipping &&
                        (salesStatus !== Entities.SalesStatus.Invoiced && salesStatus !== Entities.SalesStatus.Delivered);

                // can return if at least one line is invoiced
                case Entities.CustomerOrderOperations.Return:
                    return documentStatus === Entities.SalesStatus.Invoiced;

                // there must be at least one line not picked
                case Entities.CustomerOrderOperations.CreatePickingList:
                    return salesStatus === Entities.SalesStatus.Created;

                // can edit/cancel if no line is more than created (order cannot have any changes)
                case Entities.CustomerOrderOperations.Cancel:
                case Entities.CustomerOrderOperations.Edit:
                    return documentStatus === Entities.SalesStatus.Created;

                // can pick if at least one line is not fully invoiced or not fully delivered.
                case Entities.CustomerOrderOperations.PickUpFromStore:
                    return salesStatus !== Entities.SalesStatus.Invoiced;

                case Entities.CustomerOrderOperations.PrintPackingSlip:
                    return isShipping &&
                        (documentStatus === Entities.SalesStatus.Delivered || documentStatus === Entities.SalesStatus.Invoiced);

                default:
                    return false;
            }
        }

        /**
         * Checks whether the order is a quote or not.
         * @param {Entities.SalesOrder} salesOrder The sales order
         * @return {boolean} Returs true if sales order for quote
         */
        public static isQuote(salesOrder: Entities.SalesOrder): boolean {
            if (ObjectExtensions.isNullOrUndefined(salesOrder)) {
                return false;
            }

            return salesOrder.CustomerOrderTypeValue === Entities.CustomerOrderType.Quote;
        }

        /**
         * Gets return types for salesOrder
         * @param {Entities.SalesOrder} salesOrder The sales order
         * @return {boolean} Returs true if sales order for quote
         */
        public static getRecallTypeForSalesOrder(salesOrder: Entities.SalesOrder): Entities.CustomerOrderRecallType {
            return SearchOrdersViewModel.isQuote(salesOrder)
                ? Entities.CustomerOrderRecallType.QuoteRecall
                : Entities.CustomerOrderRecallType.OrderRecall;
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
         * Creates picking list.
         * @param {Entities.SalesOrder} salesOrder The sales order.
         * @return {IVoidAsyncResult} The async result.
         */
        public createPickingList(salesOrder: Entities.SalesOrder): IVoidAsyncResult {
            var options: Operations.IUpdateCustomerOrderOperationOptions = {
                operationType: Entities.CustomerOrderOperations.CreatePickingList,
                parameters: { CreatePickingListParameter: { SalesId: salesOrder.SalesId } }
            };

            return this.operationsManager.runOperation(Commerce.Operations.RetailOperation.EditCustomerOrder, options);
        }

        /**
         * Creates packing slip.
         * @param {Entities.SalesOrder} salesOrder The sales order.
         * @return {IVoidAsyncResult} The async result.
         */
        public createPackingSlip(salesOrder: Entities.SalesOrder): IVoidAsyncResult {
            var options: Operations.IUpdateCustomerOrderOperationOptions = {
                operationType: Entities.CustomerOrderOperations.CreatePackingSlip,
                parameters: { CreatePackingSlipParameter: { SalesId: salesOrder.SalesId } }
            };

            return this.operationsManager.runOperation(Commerce.Operations.RetailOperation.EditCustomerOrder, options);
        }

        /**
         * Print packing slip receipt
         * @param {string} salesOrder The sales order id
         * @return {IAsyncResult<Entities.Receipt[]>} Returs receipts to print
         */
        public printPackingSlip(salesId: string): IAsyncResult<Entities.Receipt[]> {
            return this.salesOrderManager.getReceiptsForPrintAsync(
                salesId, false, Entities.ReceiptType.PackingSlip, true, null, null, false, true, ApplicationContext.Instance.hardwareProfile.ProfileId);
        }

        /**
         * Recalls customer order
         * @param {string} salesId sales order Id
         * @param {Entities.CustomerOrderRecallType} recallType recall type
         * @param {IVoidAsyncResult} The async result.
         */
        public recallCustomerOrder(salesId: string, recallType: Entities.CustomerOrderRecallType): IVoidAsyncResult {
            switch (recallType) {
                case Entities.CustomerOrderRecallType.OrderRecall:
                    return this.cartManager.recallCustomerOrder(salesId);
                case Entities.CustomerOrderRecallType.QuoteRecall:
                    return this.cartManager.recallCustomerQuote(salesId);
                default:
                    break;
            }
            RetailLogger.genericError(StringExtensions.format("Invalid recallType {0}", recallType));
            var error: Entities.Error = new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR);
            return VoidAsyncResult.createRejected([error]);
        }

        /**
         * Recalls a customer order for cancellation
         * @param {Entities.SalesOrder} salesOrder The sales order
         * @param {IVoidAsyncResult} The async result.
         */
        public cancelCustomerOrder(salesOrder: Entities.SalesOrder): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(salesOrder)) {
                return this.createSalesOrderNotProvidedResult();
            }

            if (salesOrder.StatusValue !== Entities.SalesStatus.Created) {
                var error: Entities.Error = new Entities.Error(ErrorTypeEnum.ORDER_CANNOT_BE_CANCELED);
                return VoidAsyncResult.createRejected([error]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue().enqueue(() => {
                return this.recallCustomerOrder(salesOrder.SalesId, SearchOrdersViewModel.getRecallTypeForSalesOrder(salesOrder));
            }).enqueue(() => {
                return this.setCustomerOrderMode(Entities.CustomerOrderMode.Cancellation, Entities.CustomerOrderOperations.Cancel);
            }).enqueue((): IVoidAsyncResult => {
                return this.editCancellationCharge();
            });

            return asyncQueue.run();
        }

        /**
         * Recalls customer order or quote for edit
         * @param {Entities.SalesOrder} salesOrder The sales order
         * @param {IVoidAsyncResult} The async result.
         */
        public recallCustomerOrderOrQuoteForEdition(salesOrder: Entities.SalesOrder): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(salesOrder)) {
                return this.createSalesOrderNotProvidedResult();
            }

            var documentStatus: Entities.SalesStatus = Entities.SalesOrderWrapper.convertDocumentToSalesStatus(salesOrder.DocumentStatusValue);
            if ((documentStatus !== Entities.SalesStatus.Created && salesOrder.StatusValue !== Entities.SalesStatus.Created) ||
                salesOrder.TransactionTypeValue !== Entities.TransactionType.CustomerOrder) {

                var error: Entities.Error = new Entities.Error(ErrorTypeEnum.ORDER_CANNOT_BE_EDITED);
                return VoidAsyncResult.createRejected([error]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.recallCustomerOrder(salesOrder.SalesId, SearchOrdersViewModel.getRecallTypeForSalesOrder(salesOrder));
                }).enqueue(() => {
                    var editMode: Entities.CustomerOrderMode = SearchOrdersViewModel.isQuote(salesOrder)
                        ? Entities.CustomerOrderMode.QuoteCreateOrEdit
                        : Entities.CustomerOrderMode.CustomerOrderCreateOrEdit;

                    return this.setCustomerOrderMode(editMode, Entities.CustomerOrderOperations.Edit);
                });

            return asyncQueue.run();
        }

        /**
         * Sets customer order mode for cart.
         * @param {Entities.CustomerOrderMode} customerOrderMode Customer order mode value.
         * @param {Entities.CustomerOrderOperations} operatonId The operation identifier.
         */
        public setCustomerOrderMode(customerOrderMode: Entities.CustomerOrderMode, operationId: Entities.CustomerOrderOperations): IVoidAsyncResult {
            var options: Operations.IUpdateCustomerOrderOperationOptions = {
                operationType: operationId,
                parameters: { UpdateParameter: { CustomerOrderModeValue: customerOrderMode } }
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.EditCustomerOrder, options);
        }

        public searchSimpleSalesOrders(searchText: string): IAsyncResult<Entities.SalesOrder[]> {
            var asyncResult: AsyncResult<Entities.SalesOrder[]> = new AsyncResult<Entities.SalesOrder[]>();

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                asyncResult.resolve([]);
                return asyncResult;
            }

            var identifierSearchCriteria: Entities.SalesOrderSearchCriteria =
                SearchOrdersViewModel.defaultCustomerOrderSearchCriteria;
            identifierSearchCriteria.SearchIdentifiers = searchText;

            var customerNameSearchCriteria: Entities.SalesOrderSearchCriteria =
                SearchOrdersViewModel.defaultCustomerOrderSearchCriteria;
            customerNameSearchCriteria.CustomerFirstName = "*" + searchText;

            var searchResults: Entities.SalesOrder[] = null;

            var salesIdDictionary: Dictionary<boolean> = new Dictionary<boolean>();
            var channelReferenceIdDictionary: Dictionary<boolean> = new Dictionary<boolean>();
            var transactionIdDictionary: Dictionary<boolean> = new Dictionary<boolean>();
            var receiptIdDictionary: Dictionary<boolean> = new Dictionary<boolean>();

            this.getSalesOrderBySearchCriteria(identifierSearchCriteria)
                .done((salesOrders: Entities.SalesOrder[]) => {
                    searchResults = salesOrders;

                    salesOrders.forEach((salesOrder: Entities.SalesOrder) => {

                        if (!StringExtensions.isNullOrWhitespace(salesOrder.SalesId) &&
                            !salesIdDictionary.hasItem(salesOrder.SalesId)) {

                            salesIdDictionary.setItem(salesOrder.SalesId, true);
                        }

                        if (!StringExtensions.isNullOrWhitespace(salesOrder.ChannelReferenceId) &&
                            !channelReferenceIdDictionary.hasItem(salesOrder.ChannelReferenceId)) {

                            channelReferenceIdDictionary.setItem(salesOrder.ChannelReferenceId, true);
                        }

                        if (!StringExtensions.isNullOrWhitespace(salesOrder.Id) &&
                            !transactionIdDictionary.hasItem(salesOrder.Id)) {

                            transactionIdDictionary.setItem(salesOrder.Id, true);
                        }

                        if (!StringExtensions.isNullOrWhitespace(salesOrder.ReceiptId) &&
                            !receiptIdDictionary.hasItem(salesOrder.ReceiptId)) {

                            receiptIdDictionary.setItem(salesOrder.ReceiptId, true);
                        }
                    });

                    if (ArrayExtensions.hasElements(searchResults)) {
                        asyncResult.resolve(searchResults);
                    } else {
                        this.getSalesOrderBySearchCriteria(customerNameSearchCriteria)
                            .done((salesOrders: Entities.SalesOrder[]) => {

                                salesOrders.forEach((salesOrder: Entities.SalesOrder) => {

                                    if ((StringExtensions.isNullOrWhitespace(salesOrder.SalesId) ||
                                        !salesIdDictionary.hasItem(salesOrder.SalesId)) &&
                                        (StringExtensions.isNullOrWhitespace(salesOrder.ChannelReferenceId) ||
                                            !channelReferenceIdDictionary.hasItem(salesOrder.ChannelReferenceId)) &&
                                        (StringExtensions.isNullOrWhitespace(salesOrder.Id) ||
                                            !transactionIdDictionary.hasItem(salesOrder.Id)) &&
                                        (StringExtensions.isNullOrWhitespace(salesOrder.ReceiptId) ||
                                            !receiptIdDictionary.hasItem(salesOrder.ReceiptId))) {

                                        searchResults.push(salesOrder);
                                    }
                                });
                                asyncResult.resolve(searchResults);
                            }).fail((errors: Entities.Error[]) => {
                                asyncResult.reject(errors);
                            });
                    }
                }).fail((errors: Entities.Error[]) => {
                    asyncResult.reject(errors);
            });

            return asyncResult;
        }

        /**
         * Updates cancellation charge to the cart.
         * @param {Entities.Cart} cart The cart entity.
         * @return {IVoidAsyncResult} The async result.
         */
        private editCancellationCharge(): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;

            var cancellationChargeAmount: number = cart.CancellationChargeAmount;
            var asyncQueue: AsyncQueue = new AsyncQueue();

            var errors: Entities.Error[] = [];
            asyncQueue.enqueue(() => {
                // Check that the cart type is for customer order for cancellation
                if (!CustomerOrderHelper.isCustomerOrderCancellation(cart)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.CANCELLATION_CHARGE_INVALID_OPERATION));
                }

                if (ArrayExtensions.hasElements(errors)) {
                    return VoidAsyncResult.createRejected(errors);
                }
            }).enqueue(() => {
                // Gets cancellation charge amount
                var activity: Activities.GetCancellationChargeActivity =
                    new Activities.GetCancellationChargeActivity({ originalCancellationCharge: cancellationChargeAmount });

                return activity.execute().done(() => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    cancellationChargeAmount = activity.response.cancellationChargeAmount;
                });
            }).enqueue(() => {
                // Operation validation

                // The number is not a valid price for the store currency.
                if (!Helpers.CurrencyHelper.isValidAmount(cancellationChargeAmount)) {
                    errors.push(new Entities.Error(ErrorTypeEnum.CANCELLATION_CHARGE_IS_NOT_VALID));
                    return VoidAsyncResult.createRejected(errors);
                }

                // Check that the cancellation charge amount is not negative 0
                if (cancellationChargeAmount < 0) {
                    errors.push(new Entities.Error(ErrorTypeEnum.CANCELLATION_CHARGE_INVALID_NEGATIVE_AMOUNT));
                    return VoidAsyncResult.createRejected(errors);
                }

                // Data manager call to update cancellation charge
                return this.cartManager.updateCancellationChargeAsync(cancellationChargeAmount);
            });

            return asyncQueue.run();
        }

        /**
         * Creates a rejected void async result that the sales order was not provided.
         * @return {IVoidAsyncResult} The rejected async result.
         */
        private createSalesOrderNotProvidedResult(): IVoidAsyncResult {
            RetailLogger.genericError("Sales order should be provided.");
            return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
        }
    }
}
