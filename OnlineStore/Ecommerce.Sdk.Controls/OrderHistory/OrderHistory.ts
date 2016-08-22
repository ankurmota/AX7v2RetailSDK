/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Resources/Resources.ts" />

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    export class OrderHistory {

        public errorMessages: ObservableArray<string>;
        private salesOrders: ObservableArray<CommerceProxy.Entities.SalesOrder>;
        private isOrderHistoryEmpty;
        private _orderHistoryView;
        private _loadingDialog;
        private _loadingText;
        private errorPanel;

        private showPaging;
        private orderCount: number = parseInt(msaxValues.msax_OrderCount);
        private orderCountToSkip: number = 0;
        private currentPageNumber: number = 1;
        private pagingNav;
        private prevPage;
        private currentPage;
        private nextPage;

        constructor(element) {
            this._orderHistoryView = $(element);
            this._loadingDialog = this._orderHistoryView.find('.msax-Loading');
            this._loadingText = this._loadingDialog.find('.msax-LoadingText');
            this.errorPanel = this._orderHistoryView.find(" > .msax-ErrorPanel");
            this.pagingNav = this._orderHistoryView.find('.msax-Paging');
            this.prevPage = this._orderHistoryView.find('.msax-PrevPage');
            this.nextPage = this._orderHistoryView.find('.msax-NextPage');
            this.currentPage = this._orderHistoryView.find('.msax-CurrentPage');
            LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);

            this.errorMessages = ko.observableArray<string>([]);
            this.salesOrders = ko.observableArray<CommerceProxy.Entities.SalesOrder>([]);
            this.isOrderHistoryEmpty = ko.computed(() => {
                return (this.salesOrders().length == 0);
            });

            this.showPaging = ko.computed(() => {
                return (Utils.isNullOrUndefined(msaxValues.msax_ShowPaging) ? false : msaxValues.msax_ShowPaging.toLowerCase() == "true");
            });

            if (this.showPaging()) {
                // Request an additional result so that we can determine if there is a next page.
                this.getOrderHistory(this.orderCountToSkip, this.orderCount + 1);
            }
            else {
                this.getOrderHistory(this.orderCountToSkip, msaxValues.msax_OrderCount);
            }
        }

        private getResx(key: string) {
            // Gets the resource value.
            return Resources[key];
        }

        // Service call
        private getOrderHistory(skip: number, top: number) {
            CommerceProxy.RetailLogger.getOrderHistoryStarted();
            LoadingOverlay.ShowLoadingDialog();
            CustomerWebApi.GetOrderHistory(skip, top, this)
                .done((responseSalesOrders: CommerceProxy.Entities.SalesOrder[]) => {
                    this.salesOrders(responseSalesOrders);
                    if (this.showPaging() && responseSalesOrders.length == top) {
                        // Remove the additional result that we requested in order to determine if there is a next page.
                        this.salesOrders.splice(top - 1, 1);
                    }
                    else {
                        this.nextPage.addClass("disabled");
                    }
                    LoadingOverlay.CloseLoadingDialog();
                    CommerceProxy.RetailLogger.getOrderHistoryFinished();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.getOrderHistoryError, errors, Resources.String_230); // Sorry, something went wrong. We were unable to obtain your order history. Please refresh the page and try again.
                    this.errorMessages([Resources.String_230]); // Sorry, something went wrong. We were unable to obtain your order history. Please refresh the page and try again.
                    this.showError(true);
                    LoadingOverlay.CloseLoadingDialog();
                });
        }

        private nextPageClick() {
            if (!this.nextPage.hasClass("disabled")) {
                this.getOrderHistory(this.orderCountToSkip + this.orderCount, 1 + this.orderCount);
                this.orderCountToSkip += this.orderCount;
                this.currentPageNumber++;
                this.currentPage.text(this.currentPageNumber);
                this.prevPage.removeClass("disabled");
            }
        }

        private prevPageClick() {
            if (!this.prevPage.hasClass("disabled")) {
                this.getOrderHistory(this.orderCountToSkip - this.orderCount, 1 + this.orderCount);
                this.orderCountToSkip -= this.orderCount;
                this.currentPageNumber--;
                this.currentPage.text(this.currentPageNumber);
                if (this.orderCountToSkip == 0) {
                    this.prevPage.addClass("disabled");
                }
                this.nextPage.removeClass("disabled");
            }
        }

        private getSalesStatusString(statusValue: number): string {
            return Core.getSalesStatusString(statusValue);
        }

        private formatCreatedDate(createdDateTime: string): string {
            var dateString: string = "";
            if (!Utils.isNullOrWhiteSpace(createdDateTime)) {
                var date = new Date(parseInt(createdDateTime.substring(6, createdDateTime.length - 2)));
            
                // getMonth() function returns value between 0-11, hence the plus 1.
                dateString = (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
            }

            return dateString;
        }

        private getOrderDetailUrl(salesOrder: CommerceProxy.Entities.SalesOrder): string {
            var url: string = msaxValues.msax_OrderDetailsUrl;
            if (!Utils.isNullOrWhiteSpace(salesOrder.ChannelReferenceId)) {
                url += "?channelReferenceId=" + salesOrder.ChannelReferenceId;
            }
            else if (!Utils.isNullOrWhiteSpace(salesOrder.SalesId)) {
                url += "?salesId=" + salesOrder.SalesId;
            }
            else if (!Utils.isNullOrWhiteSpace(salesOrder.ReceiptId)) {
                url += "?receiptId=" + salesOrder.ReceiptId;
            }
            else {
                url = '#';
            }

            return url;
        }

        private getOrderNumber(salesOrder: CommerceProxy.Entities.SalesOrder) {
            return Core.getOrderNumber(salesOrder);
        }

        private showError(isError: boolean) {
            // Shows the error message on the error panel.
            if (isError) {
                this.errorPanel.addClass("msax-Error");
            }
            else if (this.errorPanel.hasClass("msax-Error")) {
                this.errorPanel.removeClass("msax-Error");
            }

            this.errorPanel.show();
            $(window).scrollTop(0);
        }
    }
}