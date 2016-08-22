/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class PaymentHistoryViewController extends ViewControllerBase {

        public historicalTenderLines: ObservableArray<Model.Entities.TenderLine>;
        public paymentHistoryViewModel: Commerce.ViewModels.PaymentHistoryViewModel;
        private commonHeaderData: Controls.CommonHeaderData;
        private cart: Proxy.Entities.Cart;

        constructor() {
            super(false);

            this.historicalTenderLines = ko.observableArray([]);
        }

        /**
         * When loading view.
         */
        public load(): void {
            this.cart = Session.instance.cart;
            this.paymentHistoryViewModel = new Commerce.ViewModels.PaymentHistoryViewModel();
            this.paymentHistoryViewModel.getPaymentsHistory(this.cart)
                .done((tenderLines: Commerce.Proxy.Entities.TenderLine[]) => {
                    this.historicalTenderLines(tenderLines);
                }).fail((errors: Proxy.Entities.Error[]) => {
                    NotificationHandler.displayClientErrors(errors);
                });

            this.initializeCommonHeader();
        }

        // Load Common Header 
        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            var salesId: string = StringExtensions.isNullOrWhitespace(this.cart.SalesId) ?
                StringExtensions.EMPTY : this.cart.SalesId;
            this.commonHeaderData.sectionTitle(
                StringExtensions.format(Commerce.ViewModelAdapter.getResourceString("string_4529"),
                    salesId));

            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_6501"));
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSearchBox(false);
        }
    }
}