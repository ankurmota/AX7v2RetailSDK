/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    export class SearchPickingAndReceivingViewModel extends ViewModelBase {

        public allOrders: Model.Entities.PickingAndReceivingOrder[];
        public purchaseTransferOrders: ObservableArray<Model.Entities.PickingAndReceivingOrder>;
        public resultCount: Computed<number>;

        constructor(options?: any) {
            super();
            this.allOrders = [];

            this.purchaseTransferOrders = ko.observableArray([]);
            this.resultCount = ko.computed(this.countPurchaseOrders, this);

        }

        private countPurchaseOrders(): number {
            return this.purchaseTransferOrders().length;
        }

        /**
         * Get all of purchase transfer orders.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public getPurchaseTransferOrders(): IVoidAsyncResult {
            if (ArrayExtensions.hasElements(this.allOrders)) {
                return VoidAsyncResult.createResolved();
            }

            var options: Operations.IPickingAndReceivingOperationOptions = {
                journalEntity: undefined,
                journalType: Model.Entities.PurchaseTransferOrderType.PurchaseOrder,
                operationType: Model.Entities.PickingAndReceivingOperationType.GetAllJournals,
                viewModel: this
            };

            var asyncResult = this.operationsManager.runOperation<Model.Entities.PickingAndReceivingOrder[]>(
                Operations.RetailOperation.PickingAndReceiving, options);

            return asyncResult.done((result) => { this.purchaseTransferOrders(this.allOrders); });
        }

        /**
         * Search purchase transfer orders from a given search text.
         *
         * @param {string} searchText The search text
         */
        public searchPurchaseTransferOrders(searchText: string): void {
            var purchaseTransferOrderFilterResults: Model.Entities.PickingAndReceivingOrder[] = [];
            for (var i = 0; i < this.allOrders.length; i++) {
                if (this.allOrders[i].orderId.indexOf(searchText) >= 0) {
                    purchaseTransferOrderFilterResults.push(this.allOrders[i]);
                }
            }

            this.purchaseTransferOrders(purchaseTransferOrderFilterResults);
        }
    }
}
