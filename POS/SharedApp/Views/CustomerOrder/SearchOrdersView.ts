/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../Controls/PrintReceiptDialog.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export interface ISearchOrdersViewControllerOptions {
        searchCriteria?: Model.Entities.SalesOrderSearchCriteria;
    }

    export class SearchOrdersViewController extends ViewControllerBase {
        private _indeterminateWaitVisible: Observable<boolean>;

        private commonHeaderData: Controls.CommonHeaderData;
        private _orders: ObservableArray<Model.Entities.SalesOrderWrapper>;
        private _selectedOrder: Observable<Model.Entities.SalesOrderWrapper>;
        private _salesOrderReturnDisabled: Computed<boolean>;
        private _noSalesOrderMessageVisible: Observable<boolean>;
        private _toggleShowHidePickingMenu: Observable<any>;

        private _printPackingSlipDisabled: Computed<boolean>;
        private _createPackingSlipDisabled: Computed<boolean>;
        private _createPickingListDisabled: Computed<boolean>;
        private _pickingAndPackingDisabled: Computed<boolean>;
        private _pickUpDisabled: Computed<boolean>;
        private _cancelOrderDisabled: Computed<boolean>;
        private _editOrderDisabled: Computed<boolean>;

        private _searchOrdersViewModel: ViewModels.SearchOrdersViewModel;

        private _lastSearchCriteria: Model.Entities.SalesOrderSearchCriteria;

        private _printReceiptDialog: Controls.PrintReceiptDialog;
        private _orderIndexes: Dictionary<number>;
        public showPrintReceiptDialogControl: Observable<any>;

        constructor(options: ISearchOrdersViewControllerOptions) {
            super(true);

            options = options || {};

            this._searchOrdersViewModel = new ViewModels.SearchOrdersViewModel();
            this._searchOrdersViewModel = new ViewModels.SearchOrdersViewModel();

            this._orders = ko.observableArray(<Model.Entities.SalesOrderWrapper[]>[]);
            this._orderIndexes = new Dictionary<number>();
            this._selectedOrder = ko.observable(null);
            this._lastSearchCriteria = null;

            this._orders.subscribe((salesOrders: Model.Entities.SalesOrderWrapper[]) => {
                this.commonHeaderData.resultCount(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_1030"), salesOrders.length));
            });

            this._indeterminateWaitVisible = ko.observable(false);
            this._noSalesOrderMessageVisible = ko.observable(false);
            this.initializeCommonHeader();
            this.initializeAppBarButtons();
            this._toggleShowHidePickingMenu = ko.observable(() => { });

            if (!ObjectExtensions.isNullOrUndefined(options.searchCriteria)) {
                this.searchSalesOrders(options.searchCriteria, true);
            }

            this.addControl(this._printReceiptDialog = new Controls.PrintReceiptDialog());
        }

        private getOrderIndexKey(salesOrder: Model.Entities.SalesOrder) {
            return salesOrder.SalesId + salesOrder.CustomerOrderTypeValue
        }

        /**
         *  Display error message and refreshes grid result.
         *  Do not call this method to handle search errors.
         *  @param {Model.Entities.Error[]} errors The errors to display.
         */
        private displayError(errors: Model.Entities.Error[]): void {
            this._indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors).done(() => {
                // if error is timeout
                if (ErrorHelper.hasError(errors, ErrorTypeEnum.SERVICE_UNAVAILABLE)) {
                    // for timeout only, we should refresh the list, in case the client gave up on the request
                    // but server completed that successfully
                    this.onSearchTextChanged();
                }
            });
        }

        public pickingAndPacking(): void {
            this._toggleShowHidePickingMenu()();
        }

        private printPackingSlip(): void {
            var salesOrder: Model.Entities.SalesOrder = this._selectedOrder().salesOrder;
            this._indeterminateWaitVisible(true);
            this._searchOrdersViewModel.printPackingSlip(salesOrder.SalesId).done((receipts: Model.Entities.Receipt[]) => {
                    this.printPackingSlipSuccess(receipts);
                }).fail((error: Model.Entities.Error[]) => {
                    this.printPackingSlipFailure(error);
                });
        }

        private printPackingSlipSuccess(receipts: Model.Entities.Receipt[]): void {

            this._indeterminateWaitVisible(false);
            this._printReceiptDialog.show({ receipts: receipts, rejectOnHardwareStationErrors: true })
                .onError((errors) => { this.displayError(errors); });
        }

        private printPackingSlipFailure(errors: Model.Entities.Error[]): void {
            Commerce.NotificationHandler.displayClientErrors(errors, "string_1821");
            this._indeterminateWaitVisible(false);
        }

        private createPackingSlip(): void {
            var salesOrder: Model.Entities.SalesOrder = this._selectedOrder().salesOrder;
            this._indeterminateWaitVisible(true);

            this._searchOrdersViewModel.createPackingSlip(salesOrder)
                .done(() => {
                    this.updateSelectedOrderStatus(Model.Entities.SalesStatus.Delivered,
                        Model.Entities.DocumentStatus.PackingSlip);
                    ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString('string_4545'));
                }).fail((errors) => { this.displayError(errors); });
        }

        private createPickingList(): void {
            var salesOrder: Model.Entities.SalesOrder = this._selectedOrder().salesOrder;
            this._indeterminateWaitVisible(true);

            this._searchOrdersViewModel.createPickingList(salesOrder)
                .done(() => {
                    this.updateSelectedOrderStatus(Model.Entities.SalesStatus.Created,
                        Model.Entities.DocumentStatus.PickingList);
                    ViewModelAdapter.displayMessage(Commerce.ViewModelAdapter.getResourceString('string_4543'));
                }).fail((errors) => { this.displayError(errors); });
        }

        private pickUp(): void {
            this._indeterminateWaitVisible(true);

            this._searchOrdersViewModel.recallCustomerOrder(this._selectedOrder().salesOrder.SalesId, Model.Entities.CustomerOrderRecallType.OrderRecall)
                .done(() => {
                    this._indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("PickUpView");
                }).fail((errors) => { this.displayError(errors); });
        }

        private cancelOrder(): void {            
            this._indeterminateWaitVisible(true);
            var errors: Model.Entities.Error[];
            
            this._searchOrdersViewModel.cancelCustomerOrder(this._selectedOrder().salesOrder)
                .done((): void => {
                    this._indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CartView");
                }).fail((errors: Model.Entities.Error[]): void => {
                    this.displayError(errors);
                });
        }

        private returnOrder(): void {
            var order: Model.Entities.SalesOrder = this._selectedOrder().salesOrder;
            var viewOptions: ISalesInvoicesViewControllerOptions = { salesId: order.SalesId };
            Commerce.ViewModelAdapter.navigate("SalesInvoicesView", viewOptions);
        }

        private editOrder(): void {
            this._indeterminateWaitVisible(true);
            this._searchOrdersViewModel.recallCustomerOrderOrQuoteForEdition(this._selectedOrder().salesOrder).
                done(() => {
                    this._indeterminateWaitVisible(false);
                    Commerce.ViewModelAdapter.navigate("CartView");
                }).fail((errors) => { this.displayError(errors); });
        }

        private loadingStateChanged(event: any) {
            var winControl: any = event.currentTarget.winControl;

            // Autoselect row if result length is 1.
            if (winControl.itemDataSource.list.length == 1 && winControl.selection.count() == 0) {
                winControl.selection.add(0);
            }
        }

        private initializeAppBarButtons(): void {            
            this._printPackingSlipDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.PrintPackingSlip);
            });
            this._createPackingSlipDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.CreatePackingSlip);
            });
            this._createPickingListDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.CreatePickingList);
            });
            this._pickUpDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.PickUpFromStore);
            });
            this._cancelOrderDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.Cancel);
            });
            this._editOrderDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.Edit);
            });
            this._salesOrderReturnDisabled = ko.computed(() => {
                return ObjectExtensions.isNullOrUndefined(this._selectedOrder())
                    || !ViewModels.SearchOrdersViewModel.isOrderAvailableForOperation(this._selectedOrder().salesOrder, Model.Entities.CustomerOrderOperations.Return);
            }, this);
            this._pickingAndPackingDisabled = ko.computed(() => {
                // this button holds: pick, pack and print packing slip, it can only be disabled, if all of these things are disabled
                return this._printPackingSlipDisabled() && this._createPackingSlipDisabled() && this._createPickingListDisabled() && this._pickUpDisabled();
            });
        }

        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.searchClick = this.onSearchTextChanged.bind(this);

            //Load Common Header 
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4500"));
            this.commonHeaderData.searchText("");
            this.commonHeaderData.resultCount(StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_1030"), 0));
            this.commonHeaderData.searchValidator.validatorField = "SearchOrderText";
        }

        private prepareSearch(): void {
            this._noSalesOrderMessageVisible(false); //Hide the message first before search.
            this._orders([]);
            this._orderIndexes.clear();
            this._indeterminateWaitVisible(true);
        }

        private searchSalesOrders(searchCriteria: Model.Entities.SalesOrderSearchCriteria, isAdvancedSearch: boolean = false): void {
            if (StringExtensions.isEmptyOrWhitespace(searchCriteria.SearchIdentifiers) && !isAdvancedSearch) {
                return;
            }

            this.prepareSearch();
            this._lastSearchCriteria = searchCriteria;
            this._searchOrdersViewModel.getSalesOrderBySearchCriteria(searchCriteria).done((salesOrders: Model.Entities.SalesOrder[]) => {
                this.searchSuccess(salesOrders);
            }).fail((errors: Model.Entities.Error[]) => {
                this.searchError(errors);
            });
        }

        private searchSuccess(salesOrders: Model.Entities.SalesOrder[]): void {
            this._noSalesOrderMessageVisible(salesOrders.length <= 0);
            this._indeterminateWaitVisible(false);
            this._orders(salesOrders.map((salesOrder: Model.Entities.SalesOrder) => new Model.Entities.SalesOrderWrapper(salesOrder)));

            for (var i = 0; i < salesOrders.length; i++) {
                this._orderIndexes.setItem(this.getOrderIndexKey(salesOrders[i]), i);
            }
        }

        private searchError(errors: Model.Entities.Error[]): void {
            this._orders([]);
            this._indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        private onSearchTextChanged(): void {
            this.prepareSearch();
            this._searchOrdersViewModel.searchSimpleSalesOrders(this.commonHeaderData.searchText())
                .done((searchResults: Model.Entities.SalesOrder[]) => {
                    this.searchSuccess(searchResults);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.searchError(errors);
                });
        }

        private onSelectionChanged(salesOrder: Model.Entities.SalesOrderWrapper[]): void {
            this._selectedOrder(salesOrder[0]);
        }

        private updateSelectedOrderStatus(
            statusValue: Model.Entities.SalesStatus,
            documentStatusValue: Model.Entities.DocumentStatus): void {
            if (ObjectExtensions.isNullOrUndefined(this._selectedOrder)) {
                return;
            }

            var salesOrder = this._selectedOrder().salesOrder;

            if (ObjectExtensions.isNullOrUndefined(salesOrder)) {
                return;
            }

            if (statusValue) {
                salesOrder.StatusValue = statusValue;
            }

            if (documentStatusValue) {
                salesOrder.DocumentStatusValue = documentStatusValue;
            }

            var selectedRowIndex: number = this._orderIndexes.getItem(this.getOrderIndexKey(salesOrder));
            this._orders.splice(selectedRowIndex, 1, this._selectedOrder());
            this._indeterminateWaitVisible(false);
        }
    }
}