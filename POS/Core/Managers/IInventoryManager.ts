/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/InventoryOrders.ts'/>
///<reference path='../Entities/PickingAndReceivingJournal.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IInventoryManagerName: string = "IInventoryManager";
    

    export interface IInventoryManager {
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
        getProductPricesAsync(
            productId: string,
            inventoryDimId: string,
            barcode: string,
            customerId: string,
            unitOfMeasure: string,
            quantity: number): IAsyncResult<Entities.ProductPrice[]>;

        /**
         * Get all purchase orders.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder[]>} The async result.
         */
        getPurchaseOrdersAsync(): IAsyncResult<Model.Entities.PurchaseOrder[]>;

        /**
         * Get purchase order details.
         * @param {string} orderId The purchase order identifier.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder>} The async result.
         */
        getPurchaseOrderDetailsAsync(orderId: string): IAsyncResult<Model.Entities.PurchaseOrder>;

        /**
         * Update a purchase order.
         * @param {Model.Entities.PurchaseOrder} purchaseOrder Purchase Order to be updated.
         * @return {IAsyncResult<Model.Entities.PurchaseOrder>} The async result.
         */
        updatePurchaseOrderAsync(purchaseOrder: Model.Entities.PurchaseOrder): IAsyncResult<Model.Entities.PurchaseOrder>;

        /**
         * Commit a purchase order.
         * @param {string} purchaseOrderId Purchase Order identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        commitPurchaseOrderAsync(purchaseOrderId: string): IVoidAsyncResult;

        /**
         * Get all transfer orders.
         * @return {IAsyncResult<Model.Entities.TransferOrder[]>} The async result.
         */
        getTransferOrdersAsync(): IAsyncResult<Model.Entities.TransferOrder[]>;

        /**
         * Get all picking lists.
         * @return {IAsyncResult<Model.Entities.PickingList[]>} The async result.
         */
        getPickingListsAsync(): IAsyncResult<Model.Entities.PickingList[]>;

        /**
         * Get transfer order details.
         * @param {string} orderId The transfer order identifier.
         * @return {IAsyncResult<Model.Entities.TransferOrder>} The async result.
         */
        getTransferOrderDetailsAsync(orderId: string): IAsyncResult<Model.Entities.TransferOrder>;

        /**
         * Get picking list details.
         * @param {string} pickingListId The picking list identifier.
         * @return {IAsyncResult<Model.Entities.PickingList>} The async result.
         */
        getPickingListDetailsAsync(pickingListId: string): IAsyncResult<Model.Entities.PickingList>;

        /**
         * Update a transfer order.
         * @param {Model.Entities.TransferOrder} transferOrder Transfer Order to be updated.
         * @return {IAsyncResult<Model.Entities.TransferOrder>} The async result.
         */
        updateTransferOrderAsync(transferOrder: Model.Entities.TransferOrder): IAsyncResult<Model.Entities.TransferOrder>;

        /**
         * Update a picking list.
         * @param {Model.Entities.PickingList} pickingList Picking list to update.
         * @return {IAsyncResult<Model.Entities.PickingList>} The async result.
         */
        updatePickingListAsync(pickingList: Model.Entities.PickingList): IAsyncResult<Model.Entities.PickingList>;

        /**
         * Commit a transfer order.
         * @param {string} transferOrderId Transfer Order identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        commitTransferOrderAsync(purchaseOrderId: string): IVoidAsyncResult;

        /**
         * Commit a picking list.
         * @param {string} pickingListId Picking list identifier to be committed.
         * @return {IVoidAsyncResult} The async result.
         */
        commitPickingListAsync(pickingListId: string): IVoidAsyncResult;

        /**
         * Save kit transaction implementation details.
         * @param {Entities.KitTransaction} trans The transaction to be created.
         * @return {IAsyncResult<Entities.KitTransaction>} The async result.
         */
        saveKitTransactionAsync(trans: Entities.KitTransaction): IAsyncResult<Entities.KitTransaction>;

        /**
         * Get all picking lists, transfer and purchase orders.
         * @return {IAsyncResult<Model.Entities.InventoryOrders>} The async result.
         */
        getPickAndReceiveOrdersAsync(): IAsyncResult<Model.Entities.InventoryOrders>;
    }
}