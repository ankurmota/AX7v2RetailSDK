/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/ReturnMultipleTransactionDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    /**
     * The return data for the SearchReceiptsViewController.
     */
    export interface ISearchReceiptsReturnLineData {
        /**
         * The sales order to return.
         */
        salesOrder: Model.Entities.SalesOrder;

        /**
         * The lines of the sales order to return.
         */
        salesLines: Model.Entities.SalesLine[];
    }

    /**
     * Options passed to the SearchReceiptsViewController constructor.
     */
    export interface ISearchReceiptsViewControllerOptions {
        /**
         * The default sales order to return.
         */
        salesOrderToReturn: Model.Entities.SalesOrder;

        /**
         * Async result containing the sales lines to return.
         */
        onReturnSalesOrderSalesLines: AsyncResult<ISearchReceiptsReturnLineData>;

        /**
         * Observable indicating whether the view should go into processing mode.
         */
        processing: Observable<boolean>;
    }

    export class SearchReceiptsViewController extends ViewControllerBase {
        public commonHeaderData: Commerce.Controls.CommonHeaderData;
        public forceGridLayout: Observable<() => void>;
        private _viewModel: Commerce.ViewModels.SearchReceiptsViewModel;
        // Receipt objects
        private _searchText: Observable<string>;
        private _searchTextEntered: boolean;
        private _storeNumber: Observable<string>;
        private _terminalId: Observable<string>;

        private _appBarVisible: Observable<boolean>;
        private _isNoItemSelected: Observable<boolean>;
        private _disableSelectAllSalesOrderLines: Observable<boolean>;

        // Message objects
        private _searchReceiptMessageText: Observable<string>;
        private _searchReceiptMessageTextVisible: Observable<boolean>;
        private _gridVisible: Computed<boolean>;

        // Multiple transaction dialog
        private _returnMultipleTransactionDialog: Controls.ReturnMultipleTransactionDialog;

        // Action handlers
        private _onReturnSalesOrderSalesLines: AsyncResult<ISearchReceiptsReturnLineData>;

        // Indeterminate wait objects
        private _indeterminateWaitVisible: Observable<boolean>;

        /**
         * Creates an instance of the SearchReceiptsViewController object.
         * @param {SearchReceiptsViewControllerOptions} The SearchReceiptsViewController options.
         */
        constructor(options: ISearchReceiptsViewControllerOptions) {
            super(true);
            var self: Commerce.ViewControllers.SearchReceiptsViewController = this;
            this._viewModel = new Commerce.ViewModels.SearchReceiptsViewModel();

            // Receipt objects
            this._searchText = ko.observable("");
            this._searchTextEntered = false;
            this._storeNumber = ko.observable("");
            this._terminalId = ko.observable("");
            this._appBarVisible = ko.observable(false);
            this._isNoItemSelected = ko.observable(true);
            this._disableSelectAllSalesOrderLines = ko.observable(true);
            this.forceGridLayout = ko.observable(null);
            // Message objects
            this._searchReceiptMessageText = ko.observable(Commerce.ViewModelAdapter.getResourceString("string_1032"));
            this._searchReceiptMessageTextVisible = ko.observable(true);
            this._gridVisible = ko.computed(() => {
                return !self._searchReceiptMessageTextVisible();
            });

            this.addControl(this._returnMultipleTransactionDialog = new Controls.ReturnMultipleTransactionDialog());

            // Indeterminate wait objects
            this._indeterminateWaitVisible = ko.observable(false);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.searchClick = () => {
                this.searchReceipts();
            };

            // Load Common Header
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewCommonHeader(true);
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_1205")); // SEARCH RECEIPTS
            this.commonHeaderData.searchValidator.validatorField = "SearchReceiptText";

            // Set the option values
            if (ObjectExtensions.isNullOrUndefined(options) || ObjectExtensions.isNullOrUndefined(options.salesOrderToReturn)) {
                this.commonHeaderData.searchText();
            } else {
                this._viewModel.selectedSalesOrder(options.salesOrderToReturn);
                this.commonHeaderData.searchText(options.salesOrderToReturn.ReceiptId);
            }

            this._onReturnSalesOrderSalesLines = null;
            if (!ObjectExtensions.isNullOrUndefined(options)) {
                this._onReturnSalesOrderSalesLines = options.onReturnSalesOrderSalesLines;
                if (!ObjectExtensions.isNullOrUndefined(options.processing)) {
                    this._indeterminateWaitVisible(options.processing());
                    options.processing.subscribe((newValue: boolean) => {
                        if (newValue) {
                            this.showControlSetup();
                        } else {
                            this.showControlClose();
                        }

                        this._indeterminateWaitVisible(newValue);
                    });
                }
            }
        }

        /**
         * Called when view is shown.
         */
        public onShown(): void {
            // Auto search when page is shown if the search text is set
            if (!StringExtensions.isEmptyOrWhitespace(this.commonHeaderData.searchText())) {
                this.setSalesLineDataFromOrder();
            }

            this.enablePageEvents();
        }

        /**
         * Called when view is hidden.
         */
        public onHidden(): void {
            this.disablePageEvents();

            // If the view is hidden cancel the current action
            if (this._onReturnSalesOrderSalesLines) {
                this._onReturnSalesOrderSalesLines.resolve(null);
            }
        }

        /**
         * Disable the page events
         */
        public disablePageEvents(): void {
            Commerce.Peripherals.instance.barcodeScanner.disableAsync();
        }

        /**
         * Enable the page events
         */
        public enablePageEvents(): void {
            Commerce.Peripherals.instance.barcodeScanner.enableAsync(
                (barcode: string) => {
                    if (!ObjectExtensions.isNullOrUndefined(barcode)) {
                        this.commonHeaderData.searchText(barcode);
                        this.searchReceipts();
                    }
                });
        }

        private setSalesLineDataFromOrder(): void {
            this._viewModel.prepareAndSetSalesLineDataFromOrder(this._viewModel.selectedSalesOrder()).done((result: ICancelableResult): void => {
                if (result && !result.canceled) {
                    var numberSalesLinesToDisplay: number = this._viewModel.salesLinesForDisplay().length;
                    if (numberSalesLinesToDisplay === 0) {
                        this._searchReceiptMessageTextVisible(true);
                        this._searchReceiptMessageText(Commerce.ViewModelAdapter.getResourceString("string_1219"));
                        return;
                    }

                    this.setControlStatesOnReceiptSearch(numberSalesLinesToDisplay);
                }
            }).fail((errors: Commerce.Model.Entities.Error[]): void => {
                // If no orders were found, than an error did not occur
                this._searchReceiptMessageTextVisible(true);

                if (ErrorHelper.hasError(errors, Commerce.ErrorTypeEnum.RETURN_NO_SALES_LINES_IN_ORDER)) {
                    this._searchReceiptMessageText(Commerce.ViewModelAdapter.getResourceString("string_1220"));
                } else if (ErrorHelper.hasError(errors, Commerce.ErrorTypeEnum.RETURN_ALL_SALES_LINES_IN_ORDER_RETURN)) {
                    this._searchReceiptMessageText(Commerce.ViewModelAdapter.getResourceString("string_1237"));
                } else {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
            }).always((): void => {
                this._indeterminateWaitVisible(false);
            });
        }

        

        private salesOrderLineListSelectionChangedEventHandler(items: Commerce.ViewModels.SalesLineForDisplay[]): void {
            this._viewModel.selectedSalesLines(items);
            var numItemsSelected: number = items.length;

            // Enable or disable available commands that are bound to the following members.
            this._isNoItemSelected(numItemsSelected < 1);
        }

        // Reason code dialog and submit to cart methods
        private returnSalesOrderLines(eventInfo: any): void {
            if (this._onReturnSalesOrderSalesLines) {
                var searchReceiptsReturnLineData: ISearchReceiptsReturnLineData = {
                    salesOrder: this._viewModel.selectedSalesOrder() || new Model.Entities.SalesOrderClass(),
                    salesLines: this._viewModel.selectedSalesLines() || []
                };
                this._onReturnSalesOrderSalesLines.resolve(searchReceiptsReturnLineData);
            }
        }

        

        private searchReceipts(): void {
            var searchText: string = this.commonHeaderData.searchText();
            var orderStoreNumber: string = this._storeNumber();
            var orderTerminalId: string = this._terminalId();

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return;
            }

            this.setControlStatesOnReceiptSearch(0);
            this._indeterminateWaitVisible(true);
            this._viewModel.searchReceipts(searchText, orderStoreNumber, orderTerminalId)
                .done(() => { this.searchReceiptsSuccess(); })
                .fail((errors: Model.Entities.Error[]) => {
                    this._indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private searchReceiptsSuccess(): void {
            this._indeterminateWaitVisible(false);

            if (!ArrayExtensions.hasElements(this._viewModel.salesOrders)) {
                this._searchReceiptMessageTextVisible(true);
                this._searchReceiptMessageText(Commerce.ViewModelAdapter.getResourceString("string_1218"));
                return;
            }

            // Handle the behavior when multiple items are returned
            if (this._viewModel.salesOrders.length > 1) {
                var stores: Model.Entities.ISearchReceiptStore[] = this._viewModel.getStoreList();

                this.showControlSetup();
                this._viewModel.selectedSalesOrder(null);

                this._returnMultipleTransactionDialog.show({ storeList: stores })
                    .on(DialogResult.OK, (result: Controls.IReturnMultipleTransactionDialogOutput) => { this.multipleTransactionDialogSuccess(result); })
                    .on(DialogResult.Cancel, (result: Controls.IReturnMultipleTransactionDialogOutput) => { this.multipleTransactionDialogCancelCallback(); })
                    .onError((errors: Model.Entities.Error[]) => { this.multipleTransactionDialogCancelCallback(); });

                return;
            }

            this._viewModel.selectedSalesOrder(this._viewModel.salesOrders[0]);
            this.setSalesLineDataFromOrder();
        }

        private setControlStatesOnReceiptSearch(numSalesOrderLines: number): void {
            this._searchReceiptMessageTextVisible(false);
            this.forceGridLayout()();
            this._appBarVisible(numSalesOrderLines > 0);
            this._disableSelectAllSalesOrderLines(numSalesOrderLines < 1);
            this._isNoItemSelected(true);
        }

        // Multiple transaction/order dialog
        private multipleTransactionDialogSuccess(result: Controls.IReturnMultipleTransactionDialogOutput): void {
            this._viewModel.selectedSalesOrder(result.salesOrder);
            this._indeterminateWaitVisible(true);
            this.setSalesLineDataFromOrder();
        }

        private multipleTransactionDialogCancelCallback(): void {
            this._viewModel.salesOrders = [];
            this._viewModel.selectedSalesOrder(null);
            this.showControlClose(false);
        }

        /**
         * Setup steps to take when showing a control.
         */
        private showControlSetup(): void {
            this._appBarVisible(false);
            this.disablePageEvents();
        }

        /**
         * Steps to take when a control is closed.
         * @param {boolean} show True to show application bar, false to hide the application bar
         */
        private showControlClose(showAppBar?: boolean): void {
            this._appBarVisible(showAppBar ? true : false);
            this.enablePageEvents();
        }
    }
}
