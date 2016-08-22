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

    export class OrderDetails {

        public errorMessages: ObservableArray<string>;
        private salesOrders: CommerceProxy.Entities.SalesOrder[];
        private salesOrder: Observable<CommerceProxy.Entities.SalesOrder>;
        private channelReferenceId: string;
        private salesId: string;
        private receiptId: string;
        private isSalesOrderLoaded;
        private _orderDetailsView;
        private _loadingDialog;
        private _loadingText;
        private errorPanel;
        private kitVariantProductType;
        private allDeliveryOptionDescriptions: CommerceProxy.Entities.DeliveryOption[];
        private channelReferenceIdString: string = "channelReferenceId";
        private salesIdString: string = "salesId";
        private receiptIdString: string = "receiptId";
        private currencyStringTemplate: string;

        constructor(element) {
            this._orderDetailsView = $(element);
            this._loadingDialog = this._orderDetailsView.find('.msax-Loading');
            this._loadingText = this._loadingDialog.find('.msax-LoadingText');
            this.errorPanel = this._orderDetailsView.find(" > .msax-ErrorPanel");
            LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);

            this.kitVariantProductType = ko.observable<CommerceProxy.Entities.ProductType>(CommerceProxy.Entities.ProductType.KitVariant);
            this.isSalesOrderLoaded = ko.observable<boolean>(false);
            this.errorMessages = ko.observableArray<string>([]);
            this.salesOrders = null;
            this.salesOrder = ko.observable<CommerceProxy.Entities.SalesOrder>(null);
            this.channelReferenceId = Utils.getQueryStringValue(this.channelReferenceIdString);
            this.salesId = Utils.getQueryStringValue(this.salesIdString);
            this.receiptId = Utils.getQueryStringValue(this.receiptIdString);
            this.getAllDeliveryOptionDescriptions();

            var orderSearchCriteria: CommerceProxy.Entities.SalesOrderSearchCriteria = Core.getOrderSearchCriteria(this.channelReferenceId, this.salesId, this.receiptId, true /*includeDetails*/);
            this.getOrderDetails(orderSearchCriteria);
        }

        private getResx(key: string) {
            // Gets the resource value.
            return Resources[key];
        }

        private formatCurrencyString(amount: number): any {
            if (isNaN(amount)) {
                return amount;
            }
            var formattedCurrencyString: string = "";

            if (!Utils.isNullOrUndefined(amount)) {
                if (Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                    formattedCurrencyString = amount.toString();
                }
                else {
                    formattedCurrencyString = Utils.format(this.currencyStringTemplate, Utils.formatNumber(amount));
                }
            }

            return formattedCurrencyString;
        }

        private getDeliveryModeText(deliveryModeId: string): string {
            var deliveryModeText: string = "";
            if (!Utils.isNullOrUndefined(this.allDeliveryOptionDescriptions)) {
                for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                    if (this.allDeliveryOptionDescriptions[i].Code == deliveryModeId) {
                        deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                        break;
                    }
                }
            }

            return deliveryModeText;
        }

        private closeDialogAndDisplayError(errorMessages: string[], isError: boolean) {
            LoadingOverlay.CloseLoadingDialog();
            this.showError(errorMessages, isError);
        }

        private showError(errorMessages: string[], isError: boolean) {
            this.errorMessages(errorMessages);

            if (isError) {
                this.errorPanel.addClass("msax-Error");
            }
            else if (this.errorPanel.hasClass("msax-Error")) {
                this.errorPanel.removeClass("msax-Error");
            }

            this.errorPanel.show();
            $(window).scrollTop(0);
        }

        private getSalesOrderStatusString(statusValue: number): string {
            return Resources.String_242 + Core.getSalesStatusString(statusValue); /* Status: XYZ */
        }

        // Service calls
        private getOrderDetails(orderSearchCriteria: CommerceProxy.Entities.SalesOrderSearchCriteria) {
            CommerceProxy.RetailLogger.getOrderDetailsStarted();
            LoadingOverlay.ShowLoadingDialog();
            SalesOrderWebApi.GetSalesOrderByCriteria(orderSearchCriteria, this)
                .done((data: CommerceProxy.Entities.SalesOrder[]) => {
                    var salesOrderResponse: CommerceProxy.Entities.SalesOrder = data[0];
                    this.currencyStringTemplate = Core.getExtensionPropertyValue(salesOrderResponse.ExtensionProperties, "CurrencyStringTemplate");
                    salesOrderResponse["OrderNumber"] = Core.getOrderNumber(salesOrderResponse);

                    var productIds: number[] = [];
                    for (var j = 0; j < salesOrderResponse.SalesLines.length; j++) {
                        productIds.push(salesOrderResponse.SalesLines[j].ProductId);
                    }

                    CommerceProxy.RetailLogger.getSimpleProductsByIdStarted();
                    ProductWebApi.GetSimpleProducts(productIds, this)
                        .done((simpleProducts: CommerceProxy.Entities.SimpleProduct[]) => {
                            CommerceProxy.RetailLogger.getSimpleProductsByIdFinished();

                            //Create a dictionary
                            var simpleProductsByIdMap: CommerceProxy.Entities.SimpleProduct[] = [];
                            for (var i = 0; i < simpleProducts.length; i++) {
                                var key: number = simpleProducts[i].RecordId;
                                simpleProductsByIdMap[key] = simpleProducts[i];
                            }

                            for (var i = 0; i < salesOrderResponse.SalesLines.length; i++) {
                                var salesLine: CommerceProxy.Entities.SalesLine = salesOrderResponse.SalesLines[i];
                                Core.populateProductDetailsForSalesLine(salesLine, simpleProductsByIdMap, this.currencyStringTemplate);
                                Core.populateKitItemDetailsForSalesLine(salesLine, simpleProductsByIdMap, this.currencyStringTemplate)
                            }

                            this.salesOrder(salesOrderResponse);
                            this.isSalesOrderLoaded(true);
                            LoadingOverlay.CloseLoadingDialog();
                            CommerceProxy.RetailLogger.getOrderDetailsFinished();
                        })
                        .fail((errors: CommerceProxy.ProxyError[]) => {
                            CommerceProxy.RetailLogger.getSimpleProductsByIdError(errors[0].LocalizedErrorMessage);
                            var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                            this.closeDialogAndDisplayError(errorMessages, true);
                        });
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.getOrderDetailsError, errors, Resources.String_237); // Sorry, something went wrong. An error occurred while trying to get the order details information. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }

        private getAllDeliveryOptionDescriptions() {
            CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsStarted();
            LoadingOverlay.ShowLoadingDialog();
            OrgUnitWebApi.GetDeliveryOptionsInfo(this)
                .done((data: CommerceProxy.Entities.DeliveryOption[]) => {
                    if (!Utils.isNullOrUndefined(data)) {
                        this.allDeliveryOptionDescriptions = data;
                    }
                    else {
                        this.showError([Resources.String_237], true); // Sorry, something went wrong. An error occurred while trying to get delivery methods information. Please refresh the page and try again.
                    }
                    CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsFinished();
                    LoadingOverlay.CloseLoadingDialog();
                })
                .fail((errors: CommerceProxy.ProxyError[]) => {
                    Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsError, errors, Resources.String_237); // Sorry, something went wrong. An error occurred while trying to get the order details information. Please refresh the page and try again.
                    var errorMessages: string[] = ErrorHelper.getErrorMessages(errors);
                    this.closeDialogAndDisplayError(errorMessages, true);
                });
        }
    }
}