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

    export class SearchPickingAndReceivingViewController extends ViewControllerBase {

        private _selectedPurchaseTransferOrder: Model.Entities.PickingAndReceivingOrder[];
        public indeterminateWaitVisible: Observable<boolean>;
        public searchViewModel: Commerce.ViewModels.SearchPickingAndReceivingViewModel;
        public commonHeaderData: Controls.CommonHeaderData;
        public editPurchaseTransferOrderDisabled: Observable<boolean>;

        constructor(options?: any) {
            super(true);

            this.indeterminateWaitVisible = ko.observable(false);
            this.searchViewModel = new Commerce.ViewModels.SearchPickingAndReceivingViewModel(options);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.searchClick = () => {
                this.searchPurchaseTransferOrders();
            };

            //Load Common Header 
            this.commonHeaderData.viewSearchBox(true);
            this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_3700"));
            this.commonHeaderData.resultCount("");
            this.commonHeaderData.searchText("");

            this.editPurchaseTransferOrderDisabled = ko.observable(true);
            this.getPurchaseTransferOrders();
        }

        private getPurchaseTransferOrders(): void {
            this.indeterminateWaitVisible(true);
            this.searchViewModel.getPurchaseTransferOrders()
                .done(() => {
                    this.indeterminateWaitVisible(false);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    this.indeterminateWaitVisible(false);
                    Commerce.NotificationHandler.displayClientErrors(errors);
                }
                );
        }

        /**
         * Search the purchase order from a textbox input.
         */
        public searchPurchaseTransferOrders() {
            this.searchViewModel.searchPurchaseTransferOrders(this.commonHeaderData.searchText());
            this.commonHeaderData.resultCount(this.searchViewModel.resultCount() + " " + Commerce.ViewModelAdapter.getResourceString("string_1001"));
        }

        /**
         * Event raised after user select / unselect purchase order
         */
        public purchaseTransferOrderSelectionChanged(items): void {
            this._selectedPurchaseTransferOrder = items;
            this.editPurchaseTransferOrderDisabled(items.length !== 1);
        }

        /**
         * Edit selected purchase order.
         */
        public editPurchaseTransferOrder(): void {
            var parameters = {
                JournalId: this._selectedPurchaseTransferOrder[0].orderId,
                JournalType: this._selectedPurchaseTransferOrder[0].orderType
            };

            Commerce.ViewModelAdapter.navigate("PickingAndReceivingDetailsView", parameters);
        }
    }
}