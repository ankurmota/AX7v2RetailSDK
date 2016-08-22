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
    

    // Contains the SalesLine with Product and ProductName
    export class SalesLineForDisplay extends Model.Entities.SalesLineClass {

        // Values not implemented by Model.Entities.SalesLine
        public ProductName: string;
        public Product: Model.Entities.Product;

        constructor(salesLine: Model.Entities.SalesLine) {
            super(salesLine);

            this.ProductName = StringExtensions.EMPTY;
            this.Product = null;
        }
    }

    

    export class SearchReceiptsViewModel extends ViewModelBase {
        // Receipt objects
        public salesOrders: Model.Entities.SalesOrder[];
        public salesLinesForDisplay: ObservableArray<SalesLineForDisplay>;
        public selectedSalesOrder: Observable<Model.Entities.SalesOrder>;
        public selectedSalesLines: ObservableArray<Model.Entities.SalesLine>;

        constructor() {
            super();
            // Receipt objects
            this.salesOrders = [];
            this.selectedSalesOrder = ko.observable(null);
            this.salesLinesForDisplay = ko.observableArray<SalesLineForDisplay>([]);
            this.selectedSalesLines = ko.observableArray<Model.Entities.SalesLine>([]);
        }

        /**
         * Searches sales orders given the receipt identifier, store number and terminal identifier.
         * where the sales order was created.
         * @param {string} receiptId The receipt identifier.
         * @param {string} orderStoreNumber The store number where the sales order was created.
         * @param {string} orderTerminalId The terminal identifier where the sales order was created.
         * @param {number[]} [transactionTypeValues] The type of transactions to be searched.
         * @return {IVoidAsyncResult} The async result.
         */
        public searchReceipts(
            receiptId: string,
            orderStoreNumber: string,
            orderTerminalId: string,
            transactionTypeValues?: number[]): IVoidAsyncResult {
            this.salesOrders = [];
            this.salesLinesForDisplay([]);

            if (StringExtensions.isNullOrWhitespace(receiptId)) {
                return VoidAsyncResult.createResolved();
            }

            // Set the transaction type if not yet defined.
            // By default it includes: Sales
            // and excludes: CustomerOrder
            if (ObjectExtensions.isNullOrUndefined(transactionTypeValues)) {
                transactionTypeValues = [
                    Model.Entities.TransactionType.Sales
                ];
            }

            return this.salesOrderManager.getOrdersByReceiptIdAsync(receiptId, orderStoreNumber, orderTerminalId, transactionTypeValues)
                .done((salesOrders: Model.Entities.SalesOrder[]) => { this.salesOrders = salesOrders; });
        }

        /**
         * Gets the list of orders, where each order is listed by store and register.
         * @return { Proxy.Entities.ISearchReceiptStore[] } The list of orders.
         */
        public getStoreList(): Model.Entities.ISearchReceiptStore[] {
            var storeList: Model.Entities.ISearchReceiptStore[] = [];
            var storesRegisterList: any = {};
            var storeNameList: string[] = [];

            if (ObjectExtensions.isNullOrUndefined(this.salesOrders)) {
                return storeList;
            }

            // Build the list of stores and registers from the SalesOrders list
            for (var i: number = 0; i < this.salesOrders.length; i++) {
                var storeName: string = this.salesOrders[i].StoreId;
                if (ObjectExtensions.isNullOrUndefined(storesRegisterList[storeName])) {
                    storesRegisterList[storeName] = [];
                    storeNameList.push(storeName);
                }

                storesRegisterList[storeName].push(
                    <Model.Entities.ISearchReceiptRegister> { terminalId: this.salesOrders[i].TerminalId, salesOrder: this.salesOrders[i] });
            }

            // Sort the stores and registers to display in alphabetical order
            storeNameList.sort((x: string, y: string) => StringExtensions.compare(x, y, true));
            for (var n: number = 0; n < storeNameList.length; n++) {
                var store: Model.Entities.ISearchReceiptStore = <Model.Entities.ISearchReceiptStore> { storeName: storeNameList[n], registers: [] };
                store.registers = storesRegisterList[storeNameList[n]].sort(
                    (x: Model.Entities.ISearchReceiptRegister, y: Model.Entities.ISearchReceiptRegister) => {
                        return StringExtensions.compare(x.terminalId, y.terminalId, true);
                    });

                storeList.push(store);
            }

            return storeList;
        }

        /**
         * Prepares the sales lines based on the provided sales order.
         * @param { Proxy.Entities.SalesOrder } salesOrder The sales order to prepare the lines from.
         * @return { IAsyncResult<ICancelableResult> } The async result of the preparation.
         */
        public prepareAndSetSalesLineDataFromOrder(salesOrder: Proxy.Entities.SalesOrder): IAsyncResult<ICancelableResult> {
            return Triggers.TriggerHelper.executeTriggerWorkflowAsync(
                (): IAsyncResult<ICancelableResult> => {
                    var preTriggerOptions: Triggers.IPreConfirmReturnTransactionTriggerOptions = {
                        cart: Session.instance.cart,
                        originalTransaction: salesOrder,
                        shift: Session.instance.Shift,
                        employee: Session.instance.CurrentEmployee
                    };

                    return Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreConfirmReturnTransaction, preTriggerOptions);
                },
                (): IVoidAsyncResult => {
                    return this.prepareAndSetSalesLineDataFromOrderInternal(salesOrder);
                },
                null);
        }

        private prepareAndSetSalesLineDataFromOrderInternal(salesOrder: Proxy.Entities.SalesOrder): IVoidAsyncResult {
            var allSalesLinesInOrderReturned: boolean = true;
            var salesLines: Model.Entities.SalesLine[] = salesOrder.SalesLines;

            var cart: Model.Entities.Cart = Session.instance.cart;

            // Filter the sales lines to only include sales lines not in the cart
            var filteredSalesLines: Model.Entities.SalesLine[] = [];
            for (var i: number = 0; i < salesLines.length; i++) {
                var salesLine: Model.Entities.SalesLine = salesLines[i];
                var addToFilteredSalesLines: boolean = false;
                // Add the line item if the cart is empty
                if (!Session.instance.isCartInProgress) {
                    addToFilteredSalesLines = true;
                    // Check that the line item is not in the cart
                } else {
                    addToFilteredSalesLines = true;
                    for (var j: number = 0; j < cart.CartLines.length; j++) {
                        var cartLine: Model.Entities.CartLine = cart.CartLines[j];
                        if ((salesOrder.Id === cartLine.ReturnTransactionId) && (salesLine.LineNumber === cartLine.ReturnLineNumber)) {
                            addToFilteredSalesLines = false;
                            allSalesLinesInOrderReturned = false;
                            break;
                        }
                    }
                }

                // Check that the items are not voided or a gift card
                if (addToFilteredSalesLines) {
                    if (salesLine.IsVoided || salesLine.IsGiftCardLine) {
                        addToFilteredSalesLines = false;
                    }
                }

                // Check that the items have not been returned
                if (addToFilteredSalesLines) {
                    var quantityThatCanBeReturned: number = salesLine.Quantity - salesLine.ReturnQuantity;
                    if (quantityThatCanBeReturned <= 0) {
                        addToFilteredSalesLines = false;
                    } else {
                        salesLine.Quantity = quantityThatCanBeReturned;
                    }
                }

                if (addToFilteredSalesLines) {
                    filteredSalesLines.push(salesLine);
                }
            }

            // Get the product names for the sales lines
            if (filteredSalesLines.length > 0) {
                // Get the products asynchronously
                var productIds: number[] = [];
                filteredSalesLines.forEach((x: Model.Entities.SalesLine) => productIds.push(x.ProductId));

                var searchProductCriteria: Commerce.Model.Entities.ProductSearchCriteria = {
                    Context: Commerce.Session.instance.productCatalogStore.Context,
                    Ids: productIds,
                    DataLevelValue: 4,
                    SkipVariantExpansion: true,
                    DownloadProductData: true
                };

                return this.productManager.getProductDetailsBySearchCriteriaAsync(searchProductCriteria)
                    .done((products: Model.Entities.Product[]) => this.setProductNamesInSalesLines(filteredSalesLines, products));
            } else if (allSalesLinesInOrderReturned) {
                return AsyncResult.createRejected(
                    [new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.RETURN_ALL_SALES_LINES_IN_ORDER_RETURN, null)]);
            }

            return AsyncResult.createResolved();
        }



        private setProductNamesInSalesLines(salesLines: Model.Entities.SalesLine[], products: Model.Entities.Product[]): void {
            // Add the sales line to display
            var newList: SalesLineForDisplay[] = [];
            salesLines.forEach((x: Model.Entities.SalesLine) => newList.push(new SalesLineForDisplay(x)));

            // Set the product name in each sales line
            var flattenedProducts: Dictionary<Model.Entities.Product> = ProductPropertiesHelper.getflattenedProducts(products);
            for (var i: number = 0; i < newList.length; i++) {
                var product: Model.Entities.Product = flattenedProducts.getItem(newList[i].ListingId);
                if (!ObjectExtensions.isNullOrUndefined(product)) {
                    newList[i].ProductName = product.ProductName;
                    newList[i].Product = product;
                }
            }

            this.salesLinesForDisplay(newList);
        }
    }
}