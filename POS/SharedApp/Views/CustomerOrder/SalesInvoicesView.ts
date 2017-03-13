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

    export interface ISalesInvoicesViewControllerOptions {
        salesId: string;
        salesOrderStatus? : number;//DEMO4 NEW //AM: Added to pass this to SalesInvoiceView
    }

    export class SalesInvoicesViewController extends ViewControllerBase {
        public commonHeaderData: Controls.CommonHeaderData;
        private _indeterminateWaitVisible: Observable<boolean>;
        private _salesInvoices: ObservableArray<Model.Entities.SalesInvoice>;
        private _selectedInvoice: Observable<Model.Entities.SalesInvoice>;
        private _returnSalesInvoiceDisabled: Computed<boolean>;
        private _salesId: string;
        private _salesOrderStatus: number;//DEMO4 NEW //AM: Added to pass this to SalesInvoiceView

        private _salesInvoicesViewModel: ViewModels.SalesInvoicesViewModel;

        constructor(options: ISalesInvoicesViewControllerOptions) {
            super(true);

            if (ObjectExtensions.isNullOrUndefined(options) || StringExtensions.isNullOrWhitespace(options.salesId)) {
                throw new Error("options is a required parameter for SalesInvoiceViewController and the salesId field must be set.");
            }

            this._salesId = options.salesId;
            this._indeterminateWaitVisible = ko.observable(false);
            this._selectedInvoice = ko.observable(null);
            this._returnSalesInvoiceDisabled = ko.computed(() => { return this._selectedInvoice() == null; }, this);
            this._salesInvoices = ko.observableArray<Model.Entities.SalesInvoice>([]);
            this._salesInvoicesViewModel = new ViewModels.SalesInvoicesViewModel();
            //DEMO4 NEW //AM: Added to pass this to SalesInvoiceView
            if(!NumberExtensions.isNullOrZero(options.salesOrderStatus))
                this._salesOrderStatus = options.salesOrderStatus;
            //DEMO4 end
            this.initializeCommonHeader();

            this.loadSalesInvoices();
        }

        private loadSalesInvoices(): void {
            this._indeterminateWaitVisible(true);

            this._salesInvoicesViewModel.getSalesInvoicesBySalesId(this._salesId)
                .done(this.loadSalesInvoicesSuccess.bind(this))
                .fail((errors: Model.Entities.Error[]) => {
                    this._indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }

        private loadSalesInvoicesSuccess(salesInvoices: Model.Entities.SalesInvoice[]) {
            this._indeterminateWaitVisible(false);
            this._salesInvoices(salesInvoices);
        }

        private onSelectionChanged(salesInvoices: Model.Entities.SalesInvoice[]): void {
            this._selectedInvoice(salesInvoices[0] || null);
        }

        private loadingStateChanged(event: any) {
            var winControl: any = event.currentTarget.winControl;

            // Autoselect row if result length is 1.
            if (winControl.itemDataSource.list.length == 1 && winControl.selection.count() == 0) {
                winControl.selection.add(0);
            }
        }

        private returnSalesInvoice(): void {
            // UI will make sure this is only called when this._selectedInvoice is set
            var invoice: Model.Entities.SalesInvoice = this._selectedInvoice();
            var options: ISalesInvoiceDetailsViewControllerOptions = {
                invoiceId: invoice.Id,
                salesOrderStatus: this._salesOrderStatus//DEMO4 NEW //Added new optional parameter
        };

            ViewModelAdapter.navigate("SalesInvoiceDetailsView", options);
        }

        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewSearchBox(false);

            this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_5001"));
            this.commonHeaderData.sectionInfo(this._salesId);
            this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_5000"));
        }
    }
}