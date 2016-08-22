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

    /**
     * Type interface provided to the SalesInvoiceDetailsViewController constructor.
     */
    export interface ISalesInvoiceDetailsViewControllerOptions {
        /**
         * The identifier for the invoice.
         */
        invoiceId: string;
    }

    export class SalesInvoiceDetailsViewController extends ViewControllerBase {
        public commonHeaderData: Controls.CommonHeaderData;
        private _indeterminateWaitVisible: Observable<boolean>;
        private _winControl: any;
        
        private _invoiceId: string;

        private _viewModel: ViewModels.SalesInvoiceDetailsViewModel;

        constructor(options: ISalesInvoiceDetailsViewControllerOptions) {
            super(true);

            // Validate the options provided to the view include the invoice id.
            if (ObjectExtensions.isNullOrUndefined(options)) {
                throw "SalesInvoiceDetailsViewController::ctor options are a required parameter.";
            } else if (StringExtensions.isNullOrWhitespace(options.invoiceId)) {
                throw "SalesInvoiceDetailsViewController::ctor The options did not contain a value for the invoiceId, which is a required field.";
            }

            this._invoiceId = options.invoiceId;
            this._indeterminateWaitVisible = ko.observable(false);

            this._viewModel = new ViewModels.SalesInvoiceDetailsViewModel();

            this.initializeCommonHeader();
        }

        private loadingStateChanged(event: any) {
            this._winControl = event.currentTarget.winControl;

            // Autoselect row if result length is 1.
            if (this._winControl.itemDataSource.list.length == 1 && this._winControl.selection.count() == 0) {
                this._winControl.selection.add(0);
            }
        }

        private returnSalesInvoice(): void {
            this.handleAsyncResult(this._viewModel.returnCartLines())
                .done((result) => {
                    if (!result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                });
        }

        private selectAllClick(): void {
            if (this._winControl.selection.isEverything()) {
                this._winControl.selection.clear();
            } else {
                this._winControl.selection.selectAll();
            }
        }

        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.viewSectionInfo(true);
            this.commonHeaderData.viewSearchBox(false);

            this.commonHeaderData.sectionTitle(ViewModelAdapter.getResourceString("string_5011"));
            this.commonHeaderData.sectionInfo(this._invoiceId);
            this.commonHeaderData.categoryName(ViewModelAdapter.getResourceString("string_5012"));
        }

        public load(): void {
            this.handleAsyncResult(this._viewModel.recallCartByInvoiceId(this._invoiceId));
        }

        private handleAsyncResult<T>(asyncResult: IAsyncResult<T>): IAsyncResult<T> {
            this._indeterminateWaitVisible(true);
            return asyncResult
                .done(() => {
                    this._indeterminateWaitVisible(false);
                }).fail((errors) => {
                    this._indeterminateWaitVisible(false);
                    NotificationHandler.displayClientErrors(errors);
                });
        }
    }
}