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

    export class ResumeCartViewController extends ViewControllerBase {
        public commonHeaderData;
        public indeterminateWaitVisible: Observable<boolean>;
        public suspendedCarts: ObservableArray<Model.Entities.Cart>;
        public isRecallTransactionButtonDisabled: Observable<boolean>;

        private _selectedCart: Model.Entities.Cart;
        private _viewModel: ViewModels.ResumeCartViewModel;

        constructor() {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this._viewModel = new Commerce.ViewModels.ResumeCartViewModel();
            this.indeterminateWaitVisible = ko.observable(false);
            this.suspendedCarts = ko.observableArray(<Model.Entities.Cart[]>[]);
            this.isRecallTransactionButtonDisabled = ko.observable(true);

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4156"));
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4079"));
        }

        public load(): void {
            this._viewModel.getSuspendedTransactions()
                .done((carts: Model.Entities.Cart[]) => { this.suspendedCarts(carts); })
                .fail((errors: Model.Entities.Error[]) => { NotificationHandler.displayClientErrors(errors); });
        }

        public cartSelectionChangedHandler(carts: Model.Entities.Cart[]): void {
            this._selectedCart = carts[0];
            this.isRecallTransactionButtonDisabled(ObjectExtensions.isNullOrUndefined(this._selectedCart));
        }

        public processOperation(): void {
            this.indeterminateWaitVisible(true);
            this._viewModel.recallTransaction(this._selectedCart)
                .always((): void => {
                    this.indeterminateWaitVisible(false);
                }).done((result: ICancelableResult) => {
                    if (result && !result.canceled) {
                        ViewModelAdapter.navigate("CartView");
                    }
                }).fail((errors: Model.Entities.Error[]) => {
                    if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMDISCONTINUEDFROMCHANNEL)) {                        
                        NotificationHandler.displayClientErrors(errors).always(() => {
                            ViewModelAdapter.navigate("CartView");
                        });
                    } else {
                        NotificationHandler.displayClientErrors(errors);
                    }
                });
        }
    }
}
