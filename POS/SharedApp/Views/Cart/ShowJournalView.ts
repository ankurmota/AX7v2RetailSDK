/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Core/Converters.ts'/>
///<reference path='../../Commerce.Core.d.ts'/>
///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    import Entities = Proxy.Entities;
    import SalesLineForDisplay = ViewModels.SalesLineForDisplay;

    /**
     * Type interface provided to the ShowJournalViewController constructor.
     */
    export interface IShowJournalViewControllerOptions {
        
        IsShowJournalMode?: boolean;
        IsCustomerSalesOrdersMode?: boolean;
        IsSearchOrderMode?: boolean;
        CustomerAccountNumber?: string;
        SearchCriteria?: Entities.TransactionSearchCriteria;
        
    }

    export class ShowJournalViewController extends ViewControllerBase {

        // Common header variable
        public commonHeaderData: Controls.CommonHeaderData;

        // observable controlling the spinner
        public indeterminateWaitVisible: Observable<boolean>;

        // Variable controlling the mode
        public isShowJournalMode: boolean;
        public isCustomerSalesOrdersMode: boolean;

        // Sales Order specific lines
        public selectedSalesOrder: Observable<Model.Entities.SalesOrder>;
        public customerAccountDepositLines: ObservableArray<Commerce.Model.Entities.CustomerAccountDepositLine>;
        public incomeExpenseAccountLines: ObservableArray<Commerce.Model.Entities.IncomeExpenseLine>;
        public salesLines: ObservableArray<SalesLineForDisplay>;
        public tenderLines: ObservableArray<Model.Entities.TenderLine>;

        // View models
        private _showJournalViewModel: ViewModels.ShowJournalViewModel;
        private _productDetailsViewModel: ViewModels.ProductDetailsViewModel;
        private _customerViewModel: ViewModels.CustomerDetailsViewModel;
        private _customerCardViewModel: ViewModels.CustomerCardViewModel;
        private _searchReceiptViewModel: ViewModels.SearchReceiptsViewModel;
        private _receiptViewModel: ViewModels.ReceiptViewModel;

        // Network device selection dialog control
        private _printReceiptDialog: Controls.PrintReceiptDialog;

        private _receiptToPreview: Model.Entities.Receipt;
        private _receiptToPrint: Model.Entities.Receipt;

        // Appbar related observables
        private _disableGiftReceiptButton: Observable<boolean>;
        private _disablePrintReceipt: Observable<boolean>;
        private _disableReturnButton: Observable<boolean>;
        private _isReceiptPreviewVisible: Observable<boolean>;
        private _isCustomerDetailsVisible: Observable<boolean>;

        private receiptText: Observable<string>;
        private _transactionGridViewMode: Observable<CartViewTransactionDetailViewMode>;
        private _viewCategoryName: string = Commerce.ViewModelAdapter.getResourceString("string_2807"); // JOURNALS
        private _productsInSelectedOrder: { [productId: number]: Entities.Product };

        constructor(options?: IShowJournalViewControllerOptions) {
            super(true);

            // ViewModels
            this._showJournalViewModel = new ViewModels.ShowJournalViewModel();
            this._searchReceiptViewModel = new ViewModels.SearchReceiptsViewModel();
            this._customerViewModel = new ViewModels.CustomerDetailsViewModel();
            this._customerCardViewModel = new ViewModels.CustomerCardViewModel(null);
            this._productDetailsViewModel = new ViewModels.ProductDetailsViewModel();
            this._receiptViewModel = new ViewModels.ReceiptViewModel();

            this.indeterminateWaitVisible = ko.observable(false);
            this._disableGiftReceiptButton = ko.observable(true);
            this._disablePrintReceipt = ko.observable(false);
            this._disableReturnButton = ko.observable(true);
            this._isReceiptPreviewVisible = ko.observable(false);
            this._isCustomerDetailsVisible = ko.observable(false);
            this._transactionGridViewMode = ko.observable(CartViewTransactionDetailViewMode.Items);
            this.receiptText = ko.observable(StringExtensions.EMPTY);

            this.selectedSalesOrder = ko.observable(null);
            this.customerAccountDepositLines = ko.observableArray(<Model.Entities.CustomerAccountDepositLine[]>[]);
            this.incomeExpenseAccountLines = ko.observableArray(<Model.Entities.IncomeExpenseLine[]>[]);
            this.salesLines = ko.observableArray(<SalesLineForDisplay[]>[]);
            this.tenderLines = ko.observableArray(<Model.Entities.TenderLine[]>[]);

            // Load Common Header
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCategoryName(true);
            this.isShowJournalMode = true;
            this.isCustomerSalesOrdersMode = false;
            this._productsInSelectedOrder = Object.create(null);

            if (!ObjectExtensions.isNullOrUndefined(options)) {
                if (options.IsShowJournalMode) {
                    this.filterSuccess(options.SearchCriteria);
                } else if (options.IsCustomerSalesOrdersMode) {
                    this.isShowJournalMode = false;
                    this.isCustomerSalesOrdersMode = true;
                    this.setCustomerSalesOrdersMode(options.CustomerAccountNumber);
                } else {
                    this.isShowJournalMode = false;
                    this.setOrderSearchMode(options.SearchCriteria);
                }
            } else {
                this.filterSuccess(Commerce.ViewModels.SearchOrdersViewModel.defaultTransactionSearchCriteria);
            }
        }

        /**
         * Loads the select network device dialog.
         */
        public load(): void {
            this.addControl(this._printReceiptDialog = new Controls.PrintReceiptDialog());
        }

        /**
         * Transaction list item selection change handler. Retrieves product details for the invoked
         * transaction as well as customer details - IF - the view is not customer details mode.
         * @param {{ Id: string; TransactionTypeValue: number }[]} selectedTransactions The selected journal transactions.
         */
        
        public journalSelectionChangedHandler(selectedTransactions: { Id: string; TransactionTypeValue: number }[]): void {
            this._disableGiftReceiptButton(true);
            this._disablePrintReceipt(false);
            this._disableReturnButton(true);
            this._isCustomerDetailsVisible(false);

            this.customerAccountDepositLines([]);
            this.incomeExpenseAccountLines([]);
            this.salesLines([]);
            this.tenderLines([]);
            this.selectedSalesOrder(null);

            if (ArrayExtensions.hasElements(selectedTransactions)) {
                var selectedTransaction: { Id: string; TransactionTypeValue: number } = selectedTransactions[0];
                this.indeterminateWaitVisible(true);

                var searchCriteria: Model.Entities.SalesOrderSearchCriteria = new Model.Entities.SalesOrderSearchCriteriaClass({
                    TransactionIds: [selectedTransaction.Id],
                    SearchLocationTypeValue: Model.Entities.SearchLocation.All,
                    IncludeDetails: true
                });

                

                // Casting is necessary to process CustomerOrder entities.
                if (!this.isShowJournalMode) {
                    searchCriteria.SalesId = (<Model.Entities.SalesOrder>selectedTransaction).SalesId;
                }

                searchCriteria.SalesTransactionTypeValues = [selectedTransaction.TransactionTypeValue];

                this._showJournalViewModel.getSalesOrderBySearchCriteria(searchCriteria).done((salesOrders: Model.Entities.SalesOrder[]) => {
                    this.indeterminateWaitVisible(false);

                    if (ArrayExtensions.hasElements(salesOrders)) {
                        this.selectedSalesOrder(salesOrders[0]);
                        this.populateProductInfo(salesOrders[0]);
                        this.loadCustomer(salesOrders[0].CustomerId);
                        this.tenderLines(salesOrders[0].TenderLines);

                        if (ReceiptHelper.canSalesOrderContainGiftReceipt(salesOrders[0])) {
                            this._disableGiftReceiptButton(false);
                        }

                        if (selectedTransaction.TransactionTypeValue === Model.Entities.TransactionType.Sales) {
                            this._disableReturnButton(false);
                        }
                    }
                }).fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
            }
        }

        /**
         * Navigates to the AdvancedSearchOrdersView in order to allow the user to filter the
         * search results. This functionality is disabled when in customer details mode.
         */
        public advanceSearch(): void {
            var viewOptions: IAdvancedSearchOrdersViewControllerOptions = {
                searchCriteria: this._showJournalViewModel.TransactionSearchCriteria
            };

            Commerce.ViewModelAdapter.navigate("AdvancedSearchOrdersView", viewOptions);
        }

        /**
         * Handler for the return operation in the appbar. Attempts to return the selected order,
         * otherwise throws an error if there is no order selected.
         */
        public returnOperation(): void {

            if (this.selectedSalesOrder().TransactionTypeValue === Model.Entities.TransactionType.CustomerOrder) {
                this.indeterminateWaitVisible(true);

                // There is a chance that Sales order is actually sales transactions that has been invoiced.
                // Search receipt with transaction type sales and see if it's return anything.
                this._searchReceiptViewModel.searchReceipts(this.selectedSalesOrder().ReceiptId, StringExtensions.EMPTY, StringExtensions.EMPTY)
                    .done(() => {
                        this.indeterminateWaitVisible(false);

                        if (ArrayExtensions.hasElements(this._searchReceiptViewModel.salesOrders)) {
                            var options: Operations.IReturnTransactionOperationOptions = {
                                salesOrder: this.selectedSalesOrder()
                            };

                            // The sales order is a sales transaction. Execute return operation.
                            Commerce.Operations.OperationsManager.instance.runOperation(
                                Commerce.Operations.RetailOperation.ReturnTransaction,
                                options)
                                .done((result: ICancelableDataResult<{}>) => {
                                    if (result && !result.canceled) {
                                        ViewModelAdapter.navigate("CartView");
                                    }
                                }
                                    );
                        } else {
                            // We don't support return transaction if transaction type customer order.
                            // Give message to user they should navigate to recall order page to return this type of transaction.
                            NotificationHandler.displayErrorMessage("string_4151");
                        }
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        this.indeterminateWaitVisible(false);
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    });
            } else {
                var options: Operations.IReturnTransactionOperationOptions = {
                    salesOrder: this.selectedSalesOrder()
                };

                Commerce.Operations.OperationsManager.instance.runOperation(
                    Commerce.Operations.RetailOperation.ReturnTransaction, options)
                    .done((result: ICancelableDataResult<{}>) => {
                        if (result && !result.canceled) {
                            ViewModelAdapter.navigate("CartView");
                        }
                    }
                        );
            }
        }

        /**
         * Handler for the show receipt appbar command. Attempts to retrieve the receipts for a
         * selected transaction. If no transaction is selected, throws an error.
         */
        public showReceiptOperation(): void {
            this.indeterminateWaitVisible(true);
            this._showJournalViewModel.getReceiptsForSalesOrder(this.selectedSalesOrder(), Entities.ReceiptType.SalesReceipt, true)
                .done((receipts: Model.Entities.Receipt[]) => {
                    this.indeterminateWaitVisible(false);
                    this.showReceipt(receipts);
                }).fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                });
        }

        /**
         * Handler for the show gift receipt appbar command. Attempts to retrieve the gift receipts for a
         * selected transaction. If no transaction is selected, throws an error.
         */
        public showGiftReceiptOperation(): void {
            this.indeterminateWaitVisible(true);
            this._showJournalViewModel.getReceiptsForSalesOrder(this.selectedSalesOrder(), Entities.ReceiptType.GiftReceipt, true)
                .done((receipts: Model.Entities.Receipt[]) => {
                    this.indeterminateWaitVisible(false);
                    this.showReceipt(receipts);
                }).fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                    this.indeterminateWaitVisible(false);
                });
        }

        /**
         * Retrieves a customer by customer account number and initializes a new customer for this view
         * @param {string} customerAccountNumber the customer's account number
         */
        private loadCustomer(customerAccountNumber: string): void {
            if (StringExtensions.isNullOrWhitespace(customerAccountNumber)) {
                this._isCustomerDetailsVisible(false);
            } else if (!(!ObjectExtensions.isNullOrUndefined(this._customerCardViewModel.customer())
                && StringExtensions.compare(customerAccountNumber, this._customerCardViewModel.customer().AccountNumber, true) === 0)) {

                this._customerViewModel.loadCustomer(customerAccountNumber)
                    .done(() => {
                        var newCustomer: Model.Entities.Customer = this._customerViewModel.Customer();
                        this._customerCardViewModel.customer(newCustomer);
                        this._customerViewModel.getLoyaltyCards().done(() => {
                            this._customerCardViewModel.customerLoyaltyCards(this._customerViewModel.loyaltyCards());
                        });

                        this._customerViewModel.getBalanceAsync(customerAccountNumber, newCustomer.InvoiceAccount)
                            .done((customerBalances: Commerce.Model.Entities.CustomerBalances) => {
                                newCustomer.Balance = CustomerHelper.getCustomerBalance(newCustomer, customerBalances);
                                newCustomer.CreditLimit = CustomerHelper.getCustomerCreditLimit(newCustomer, customerBalances);
                                this._customerCardViewModel.customer(newCustomer);
                                this._isCustomerDetailsVisible(true);
                                RetailLogger.viewsCartShowJournalViewLoaded();
                            });
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        this._isCustomerDetailsVisible(false);
                    });
            } else {
                this._isCustomerDetailsVisible(true);
            }
        }

        /**
         * Populate product information for the sales lines
         * @param {Model.Entities.SalesOrder} order the order for which to retrieve product details
         */
        private populateProductInfo(order: Model.Entities.SalesOrder): void {

            this.indeterminateWaitVisible(true);
            this._productsInSelectedOrder = Object.create(null);

            if (ArrayExtensions.hasElements(order.IncomeExpenseLines)) {
                this.incomeExpenseAccountLines(order.IncomeExpenseLines);
                this.indeterminateWaitVisible(false);
            } else if (ArrayExtensions.hasElements(order.CustomerAccountDepositLines)) {
                this.customerAccountDepositLines(order.CustomerAccountDepositLines);
                this.indeterminateWaitVisible(false);
            } else if (ArrayExtensions.hasElements(order.SalesLines)) {
                this.populateProductInformationQueue(order).run().fail((errors: Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }).always((): void => {
                    var salesLinesForDisplay: SalesLineForDisplay[] = order.SalesLines.map((salesLine: Entities.SalesLine): SalesLineForDisplay => {
                        var salesLineForDisplay: ViewModels.SalesLineForDisplay = new ViewModels.SalesLineForDisplay(salesLine);
                        if (salesLine.IsGiftCardLine) {
                            salesLineForDisplay.ProductName = Commerce.ViewModelAdapter.getResourceString("string_5152"); // 'Gift card'
                        } else {
                            var product: Entities.Product = this._productsInSelectedOrder[salesLine.ProductId];
                            if (!ObjectExtensions.isNullOrUndefined(product)) {
                                salesLineForDisplay.Product = product;
                                salesLineForDisplay.ProductName = this._productsInSelectedOrder[salesLine.ProductId].ProductName;
                            }
                        }

                        return salesLineForDisplay;
                    });

                    this.salesLines(salesLinesForDisplay);
                    this.indeterminateWaitVisible(false);
                });
            } else {
                this.indeterminateWaitVisible(false);
            }
        }

        /**
         * Creates the async queue to populate the product information for the provided sales order.
         * @param {Entities.SalesOrder} order The sales order for which to get the product information.
         * @return {AsyncQueue} The async queue that populates the product information.
         */
        private populateProductInformationQueue(order: Entities.SalesOrder): AsyncQueue {
            var idLookUp: number[] = [];
            order.SalesLines.forEach((value: Commerce.Model.Entities.SalesLine) => {
                if (value.ProductId !== 0) {
                    idLookUp.push(value.ProductId);
                }
            });

            var remoteProductItemIdLookup: Model.Entities.ProductLookupClause[] = [];
            var populateInfoQueue: AsyncQueue = new AsyncQueue();
            populateInfoQueue.enqueue((): IVoidAsyncResult => {
                var localProductSearchCriteria: Model.Entities.ProductSearchCriteria = {
                    Context: new Model.Entities.ProjectionDomainClass({ ChannelId: ApplicationContext.Instance.storeInformation.RecordId, CatalogId: 0 }),
                    Ids: idLookUp,
                    DataLevelValue: 4,
                    SkipVariantExpansion: true
                };

                return this._productDetailsViewModel.getProductDetailsBySearchCriteria(localProductSearchCriteria)
                    .done((products: Commerce.Model.Entities.Product[]) => {
                        order.SalesLines.forEach((salesLine: Commerce.Model.Entities.SalesLine) => {
                            if (salesLine.ProductId !== 0) {
                                var filteredProduct: Proxy.Entities.Product = this.searchProductByItemId(salesLine.ItemId, products);
                                if (!ObjectExtensions.isNullOrUndefined(filteredProduct)) {
                                    this._productsInSelectedOrder[salesLine.ProductId] = filteredProduct;
                                } else {
                                    var result: Model.Entities.ProductLookupClause[]
                                        = $.grep(remoteProductItemIdLookup, (e: Model.Entities.ProductLookupClause): boolean => {
                                            return e.ItemId === salesLine.ItemId;
                                        }, false);

                                    if (result.length === 0) {
                                        remoteProductItemIdLookup.push({ ItemId: salesLine.ItemId });
                                    }
                                }
                            }
                        });
                    });
            }).enqueue((): IVoidAsyncResult => {
                if (!ArrayExtensions.hasElements(remoteProductItemIdLookup)) {
                    return VoidAsyncResult.createResolved();
                }

                // When retrieving products in the order - the only needed information is that of the variant used in the order, hence use skipVariantExpansion
                // to force the search to exclude all the other remaining variants of the product - this is done for performance optimization.
                var remoteProductSearchCriteria: Model.Entities.ProductSearchCriteria = {
                    Context: new Model.Entities.ProjectionDomainClass({ ChannelId: ApplicationContext.Instance.storeInformation.RecordId, CatalogId: 0 }),
                    Ids: [],
                    DataLevelValue: 4,
                    SkipVariantExpansion: true,
                    ItemIds: remoteProductItemIdLookup
                };

                remoteProductSearchCriteria.DownloadProductData = true;
                this._productDetailsViewModel.getProductDetailsBySearchCriteria(remoteProductSearchCriteria)
                    .done((remoteProducts: Commerce.Model.Entities.Product[]) => {
                        order.SalesLines.forEach((salesLine: Commerce.Model.Entities.SalesLine) => {
                            if (salesLine.ProductId !== 0) {
                                if (ObjectExtensions.isNullOrUndefined(this._productsInSelectedOrder[salesLine.ProductId])) {
                                    var filteredProduct: Proxy.Entities.Product =
                                        this.searchProductByItemId(salesLine.ItemId, remoteProducts);

                                    if (!ObjectExtensions.isNullOrUndefined(filteredProduct)) {
                                        this._productsInSelectedOrder[salesLine.ProductId] = filteredProduct;
                                    } else {
                                        RetailLogger.viewsCartShowJournalViewRetrieveProductFailed(salesLine.ProductId);
                                    }
                                }
                            }
                        });
                    });
            });

            return populateInfoQueue;
        }

        /**
         * Sets the visibility for payment lines to visible
         */
        
        private TogglePaymentLineItemGrid(): void {
            this._transactionGridViewMode() === Commerce.ViewControllers.CartViewTransactionDetailViewMode.Payments ?
                this._transactionGridViewMode(Commerce.ViewControllers.CartViewTransactionDetailViewMode.Items) :
                this._transactionGridViewMode(Commerce.ViewControllers.CartViewTransactionDetailViewMode.Payments);
        }

        /**
         * Displays the print receipt dialog and prints the receipt
         */
        private printReceipts(): void {
            if (!ObjectExtensions.isNullOrUndefined(this._receiptToPrint)
                && ArrayExtensions.hasElements(this._receiptToPrint.Printers)) {
                // If the user chooses to print, we override the print behavior (except if it can never be printed)
                this._receiptToPrint.Printers.forEach((printer: Entities.Printer) => {
                    if (printer.PrintBehaviorValue !== Entities.PrintBehavior.Never) {
                        printer.PrintBehaviorValue = Entities.PrintBehavior.Always;
                    }
                });
            }

            var dialogState: Controls.IPrintReceiptDialogState = {
                receipts: [this._receiptToPrint],
                rejectOnHardwareStationErrors: true,
                ignoreShouldPrompt: true,
                notifyOnNoPrintableReceipts: true,
                isCopyOfReceipt: true,
                associatedOrder: this.selectedSalesOrder()
            };

            this._printReceiptDialog.show(dialogState).onError((errors: Model.Entities.Error[]) => {
                NotificationHandler.displayClientErrors(errors);
            });
        }

        /**
         * Cancels the show receipt option
         */
        private cancelShowReceiptOperation(): void {
            this._isReceiptPreviewVisible(false);
            this.commonHeaderData.backButtonVisible(true);
            this._receiptToPreview = null;
            this._receiptToPrint = null;
            this.receiptText(StringExtensions.EMPTY);
            this.commonHeaderData.categoryName(this._viewCategoryName); // revert view category name
        }

        private isPaymentVoided(tenderLine: Model.Entities.CartTenderLine): boolean {
            return tenderLine.StatusValue === Model.Entities.TenderLineStatus.Voided;
        }

        private getPriceOverrideText(cartLine: Model.Entities.SalesLine): string {
            var priceOverrideText: string = StringExtensions.EMPTY;
            if (cartLine && cartLine.IsPriceOverridden) {
                var originalFormattedPriceText: string = NumberExtensions.formatCurrency(cartLine.OriginalPrice);
                priceOverrideText = StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4368"), originalFormattedPriceText);
            }

            return priceOverrideText;
        }

        private onLineItemExpand(eventInfo: Commerce.ListView.IItemExpand): void {
            ko.applyBindingsToNode(eventInfo.colspanRow, {
                template: {
                    name: "ShowJournallineItemColspan",
                    data: eventInfo.data,
                    as: "originalCartLine"
                }
            }, this);
        }

        /**
         * Bind the line values to the row to expand the Customer Account Deposit
         * @param {Commerce.ListView.IItemExpand} eventInfo The event infomation that contains the row to expand and the data to bind.
         */
        private onCustomerAccountDepositLineItemExpand(eventInfo: Commerce.ListView.IItemExpand): void {
            ko.applyBindingsToNode(eventInfo.colspanRow, {
                template: {
                    name: "ShowJournalCustomerAccountDepositLineItemColspan",
                    data: eventInfo.data,
                    as: "line"
                }
            }, this);
        }

        /**
         * Checks if cart line has information to display in expandable section.
         * @param {Proxy.Entities.SalesLine} cartLine The cart line.
         * @return {boolean} The flag to enable or disable row expand.
         */
        private isTransactionGridRowExpandable(salesLine: Proxy.Entities.SalesLine): boolean {
            var product: Entities.Product = this._productsInSelectedOrder[salesLine.ProductId];
            var variantName: string = this.getVariantName(salesLine);
            var isExpandable: boolean = (variantName.length > 0) ||
                salesLine.IsPriceOverridden ||
                salesLine.Comment.length > 0 ||
                salesLine.SerialNumber.length > 0 ||
                ArrayExtensions.hasElements(salesLine.DiscountLines);

            return isExpandable;
        }

        /**
         * Checks if a customer account deposit line has information to display in expandable section.
         *
         * @param {Model.Entities.CartLine} cartLine The cart line.
         * @return {boolean} The flag to enable or disable row expand.
         */
        private isCustomerAccountDepositGridRowExpandable(customerAccountDepositCartLine: Model.Entities.CustomerAccountDepositLine): boolean {
            var isExpandable: boolean = !ObjectExtensions.isNullOrUndefined(customerAccountDepositCartLine)
                && (customerAccountDepositCartLine.Comment && (customerAccountDepositCartLine.Comment.length > 0));

            return isExpandable;
        }

        

        /**
         * Previews the first element at the given collection of receipts.
         * @param {Model.Entities.Receipt} receipts The collection of receipts.
         */
        private showReceipt(receipts: Model.Entities.Receipt[]): void {
            if (ArrayExtensions.hasElements(receipts)) {
                this._receiptToPreview = receipts[0];
                this._receiptToPrint = receipts.length > 1 ? receipts[1] : null;

                this.commonHeaderData.backButtonVisible(false);
                this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4127")); // Receipt preview
                this.receiptText(Commerce.ReceiptHelper.convertToHtml(
                    this._receiptToPreview.Header + this._receiptToPreview.Body + this._receiptToPreview.Footer));
                this._disablePrintReceipt(!this._receiptViewModel.canReceiptBePrinted(this._receiptToPrint));
                this._isReceiptPreviewVisible(true);
            } else {
                var errors: Model.Entities.Error[] = [new Model.Entities.Error(ErrorTypeEnum.RECEIPT_NOT_AVAILABlE_FOR_ORDER)];
                NotificationHandler.displayClientErrors(errors, "string_4127");
            }
        }

        /**
         * Handler for retrieving transaction search criteria that was passed in as options to the constructor.
         * @param {Model.Entities.SalesOrderSearchCriteria} searchCriteria specifies the criteria to search transactions.
         */
        private setOrderSearchMode(salesOrderSearchCriteria: Model.Entities.SalesOrderSearchCriteria): void {
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4428")); // "Customer"
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_204")); // "Order history"
            this._viewCategoryName = Commerce.ViewModelAdapter.getResourceString("string_204");

            this.indeterminateWaitVisible(true);
            this._showJournalViewModel.OrderSearchCriteria = salesOrderSearchCriteria;
        }

        /**
         * Handler for retrieving transaction search criteria that was passed in as options to the constructor.
         * @param {Model.Entities.SalesOrderSearchCriteria} searchCriteria specifies the criteria to search transactions.
         */
        private setCustomerSalesOrdersMode(customerId: string): void {
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4428")); // "Customer"
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_204")); // "Order history"
            this._viewCategoryName = Commerce.ViewModelAdapter.getResourceString("string_204"); // "Order history"

            this.indeterminateWaitVisible(true);
            this._showJournalViewModel.CustomerId = customerId;
        }

        /**
         * Handler for retrieving transaction search criteria that was passed in as options to the constructor.
         * @param {Model.Entities.TransactionSearchCriteria} searchCriteria specifies the criteria to search transactions.
         */
        private filterSuccess(searchCriteria: Model.Entities.TransactionSearchCriteria): void {
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4143")); // "SHOW JOURNAL"
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_2807")); // "Journals"
            this._viewCategoryName = Commerce.ViewModelAdapter.getResourceString("string_2807"); // "Journals"

            if (ObjectExtensions.isNullOrUndefined(searchCriteria.SearchLocationTypeValue)) {
                searchCriteria.SearchLocationTypeValue = Model.Entities.SearchLocation.Local;
                searchCriteria.StoreId = ApplicationContext.Instance.storeNumber;
            }

            if (!ObjectExtensions.isNullOrUndefined(searchCriteria) && !ObjectExtensions.isNullOrUndefined(searchCriteria.TransactionIds)) {
                searchCriteria.TransactionIds = searchCriteria.TransactionIds.filter((value: string) => {
                    return !StringExtensions.isEmptyOrWhitespace(value);
                });
            }

            this.indeterminateWaitVisible(true);
            this._showJournalViewModel.TransactionSearchCriteria = searchCriteria;
        }

        private getVariantName(salesLine: Entities.SalesLine): string {
            if (salesLine.IsGiftCardLine) {
                return salesLine.Comment;
            }

            var product: Entities.Product = this._productsInSelectedOrder[salesLine.ProductId];
            return ObjectExtensions.isNullOrUndefined(product) ?
                StringExtensions.EMPTY :
                ProductPropertiesHelper.ProductPropertyFormatter(product, "VariantName", salesLine.ProductId);
        }

        private searchProductByItemId(itemId: string, products: Model.Entities.Product[]): Model.Entities.Product {
            if (ObjectExtensions.isNullOrUndefined(products)) {
                return null;
            }

            var filteredProducts: Model.Entities.Product[] = products.filter(function (product: Model.Entities.Product): boolean {
                return product.ItemId === itemId;
            });

            if (ArrayExtensions.hasElements(filteredProducts)) {
                return <Model.Entities.Product>$.extend(true, {}, filteredProducts[0]);
            } else {
                return null;
            }
        }
    }
}
