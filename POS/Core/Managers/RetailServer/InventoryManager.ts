/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/PickingAndReceivingJournal.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IInventoryManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class InventoryManager implements IInventoryManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get product prices.
         * @param {string} productId The product identifier.
         * @param {string} inventoryDimId The inventory dimension identifier.
         * @param {string} barcode The barcode identifier.
         * @param {string} customerId The customer identifier.
         * @param {string} unitOfMeasure The unit of measure.
         * @param {number} quantity The quantity for checking price.
         * @return {IAsyncResult<Entities.ProductPrice>} The async result.
         */
        public getProductPricesAsync(
            productId: string,
            inventoryDimId: string,
            barcode: string,
            customerId: string,
            unitOfMeasure: string,
            quantity: number): IAsyncResult<Entities.ProductPrice[]> {

            var query: Common.IDataServiceRequest = this._commerceContext.products().getPrices(
                productId, inventoryDimId, barcode, customerId, unitOfMeasure, quantity);
            return query.execute<Entities.ProductPrice[]>();
        }

        /**
         * Get all purchase orders.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder[]>} The async result.
         */
        public getPurchaseOrdersAsync(): IAsyncResult<Model.Entities.PurchaseOrder[]> {
            var query: Common.IDataServiceRequest = this._commerceContext.purchaseOrders().read();
            return query.execute<Model.Entities.PurchaseOrder[]>();
        }

        /**
         * Get purchase order details.
         * @param {string} orderId The purchase order identifier.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder>} The async result.
         */
        public getPurchaseOrderDetailsAsync(orderId: string): IAsyncResult<Model.Entities.PurchaseOrder> {
            var query: Common.IDataServiceRequest = this._commerceContext.purchaseOrders(orderId).read();
            return query.execute<Model.Entities.PurchaseOrder>();
        }

        /**
         * Update a purchase order.
         * @param {Model.Entities.PurchaseOrder} purchaseOrder Purchase Order to be updated.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder>} The async result.
         */
        public updatePurchaseOrderAsync(purchaseOrder: Model.Entities.PurchaseOrder): IAsyncResult<Model.Entities.PurchaseOrder> {
            var request: Common.IDataServiceRequest = this._commerceContext.purchaseOrders(purchaseOrder.OrderId)
                .update(purchaseOrder);
            return request.execute();
        }

        /**
         * Commit a purchase order.
         * @param {string} purchaseOrderId Purchase Order identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        public commitPurchaseOrderAsync(purchaseOrderId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.purchaseOrders(purchaseOrderId).commit();
            return request.execute();
        }

        /**
         * Get all transfer orders.
         * @return {IAsyncResult<Model.Entities.TransferOrder[]>} The async result.
         */
        public getTransferOrdersAsync(): IAsyncResult<Model.Entities.TransferOrder[]> {
            var query: Common.IDataServiceRequest = this._commerceContext.transferOrders().read();
            return query.execute<Model.Entities.TransferOrder[]>();
        }

        /**
         * Get all picking lists.
         * @return {IAsyncResult<Model.Entities.PickingList[]>} The async result.
         */
        public getPickingListsAsync(): IAsyncResult<Model.Entities.PickingList[]> {
            var query: Common.IDataServiceRequest = this._commerceContext.pickingLists().read();
            return query.execute<Model.Entities.PickingList[]>();
        }

        /**
         * Get transfer order details.
         * @param {string} orderId The transfer order identifier.
         * @return {IAsyncResult<Model.Entities.TransferOrder>} The async result.
         */
        public getTransferOrderDetailsAsync(orderId: string): IAsyncResult<Model.Entities.TransferOrder> {
            var query: Common.IDataServiceRequest = this._commerceContext.transferOrders(orderId).read();
            return query.execute<Model.Entities.TransferOrder>();
        }

        /**
         * Get picking list details.
         * @param {string} pickingListId The picking list identifier.
         * @return {IAsyncResult<Model.Entities.PickingList>} The async result.
         */
        public getPickingListDetailsAsync(pickingListId: string): IAsyncResult<Model.Entities.PickingList> {
            var query: Common.IDataServiceRequest = this._commerceContext.pickingLists(pickingListId).read();
            return query.execute<Model.Entities.PickingList>();
        }

        /**
         * Update a transfer order.
         * @param {Model.Entities.TransferOrder} transferOrder Transfer Order to be updated.
         * @return {IAsyncResult<Model.Entities.TransferOrder>} The async result.
         */
        public updateTransferOrderAsync(transferOrder: Model.Entities.TransferOrder): IAsyncResult<Model.Entities.TransferOrder> {
            var request: Common.IDataServiceRequest = this._commerceContext.transferOrders(transferOrder.OrderId)
                .update(transferOrder);
            return request.execute();
        }

        /**
         * Update a picking list.
         * @param {Model.Entities.PickingList} pickingList Picking list to update.
         * @return {IAsyncResult<Model.Entities.PickingList>} The async result.
         */
        public updatePickingListAsync(pickingList: Model.Entities.PickingList): IAsyncResult<Model.Entities.PickingList> {
            var request: Common.IDataServiceRequest = this._commerceContext.pickingLists(pickingList.OrderId).update(pickingList);
            return request.execute();
        }

        /**
         * Commit a transfer order.
         * @param {string} transferOrder Transfer Order identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        public commitTransferOrderAsync(transferOrderId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.transferOrders(transferOrderId).commit();
            return request.execute();
        }

        /**
         * Commit a picking list.
         * @param {string} pickingListId Picking list identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        public commitPickingListAsync(pickingListId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.pickingLists(pickingListId).commit();
            return request.execute();
        }

        /**
         * Save kit transaction implementation details.
         * @param {Entities.KitTransaction} trans The transaction to be created.
         * @return {IAsyncResult<Entities.KitTransaction>} The async result.
         */
        public saveKitTransactionAsync(trans: Entities.KitTransaction): IAsyncResult<Entities.KitTransaction> {
            trans.Id = NumberSequence.GetNextTransactionId();

            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().disassembleKitTransactions(trans);
            return request.execute<Entities.KitTransaction>();
        }

        /**
         * Get all picking lists, transfer and purchase orders.
         * @return {IAsyncResult<Model.Entities.InventoryOrders>} The async result.
         */
        public getPickAndReceiveOrdersAsync(): IAsyncResult<Entities.InventoryOrders> {
            var pickingsListsRequest: Common.IDataServiceRequest = this._commerceContext.pickingLists().read();
            var purchaseOrdersRequest: Common.IDataServiceRequest = this._commerceContext.purchaseOrders().read();
            var transferOrdersRequest: Common.IDataServiceRequest = this._commerceContext.transferOrders().read();

            var pickingListsResult: Entities.PickingList[];
            var transferOrdersResult: Entities.TransferOrder[];
            var purchaseOrdersResult: Entities.PurchaseOrder[];

            var pickingListAsyncResult: IAsyncResult<Entities.PickingList[]> =
                    pickingsListsRequest.execute<Entities.PickingList[]>()
                .done((result: Entities.PickingList[]) => {
                    pickingListsResult = result;
                });

            var transferOrderAsyncResult: IAsyncResult<Entities.TransferOrder[]> =
                    transferOrdersRequest.execute<Entities.TransferOrder[]>()
                .done((result: Entities.TransferOrder[]) => {
                    transferOrdersResult = result;
                });

            var purchaseOrderAsyncResult: IAsyncResult<Entities.PurchaseOrder[]> =
                    purchaseOrdersRequest.execute<Entities.PurchaseOrder[]>()
                .done((result: Entities.PurchaseOrder[]) => {
                    purchaseOrdersResult = result;
                });

            var asyncResults: IAsyncResult<any>[] = [pickingListAsyncResult, transferOrderAsyncResult, purchaseOrderAsyncResult];

            return VoidAsyncResult.join(asyncResults).map((): Entities.InventoryOrders => {
                var orders: Entities.InventoryOrders = {
                    pickingLists: pickingListsResult,
                    transferOrders: transferOrdersResult,
                    purchaseOrders: purchaseOrdersResult
                };

                return orders;
            });
        }
    }
}