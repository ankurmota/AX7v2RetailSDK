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

    export interface PickingAndReceivingDetailsViewModelParameters {
        JournalId?: string;
        JournalType?: Model.Entities.PurchaseTransferOrderType;
        VariantId?: number;
        Product?: Model.Entities.Product;
        ProductDetailsDictionary?: Dictionary<Model.Entities.Product>;
    }

    export class PickingAndReceivingDetailsViewModel extends ViewModelBase {

        private _productIndexes: number[]; //keys to search product faster within lines
        private _dimensionDelimiter: string = "|";
        public journalId: string;
        public lineDetails: ObservableArray<Model.Entities.PickingAndReceivingOrderLine>;
        public journalHeader: Observable<Model.Entities.PickingAndReceivingOrder>;
        public productDetailsDictionary: Dictionary<Model.Entities.Product>;
        public totalProducts: number;

        private _journalType: Model.Entities.PurchaseTransferOrderType;

        constructor(options: PickingAndReceivingDetailsViewModelParameters) {
            super();

            this.journalId = options.JournalId;
            this._journalType = options.JournalType;
            this.journalHeader = ko.observable({
                OrderId: "",
                OrderType: Model.Entities.PurchaseTransferOrderType.Unknown
            });
            this.lineDetails = ko.observableArray<Model.Entities.PickingAndReceivingOrderLine>([]);
            this.totalProducts = this.lineDetails().length;
            this._productIndexes = [];

            if (options.ProductDetailsDictionary) {
                this.productDetailsDictionary = options.ProductDetailsDictionary;
            } else {
                this.productDetailsDictionary = new Dictionary<Model.Entities.Product>();
            }
        }

        private convertToJournalEntity(): Model.Entities.PickingAndReceivingOrder {
            var journal: Model.Entities.PickingAndReceivingOrder = this.journalHeader();
            journal.orderLines = this.lineDetails();

            return journal;
        }

        private getJournalDetailsSuccess(newOrderLines: Model.Entities.PickingAndReceivingOrderLine[]) {
            var orderLine: Model.Entities.PickingAndReceivingOrderLine;
            var productDetails: Model.Entities.Product;
            var productVariant: Model.Entities.ProductVariant;
            var dimensionIds: Model.Entities.ProductDimensionResult[];

            for (var i: number = 0; i < newOrderLines.length; i++) {
                orderLine = newOrderLines[i];
                this.setProductIndex(orderLine.recordId, orderLine.productNumber, orderLine.colorId, orderLine.configurationId, orderLine.sizeId, orderLine.styleId, i);

                if (this.productDetailsDictionary.hasItem(orderLine.productNumber)) {
                    //update dimension translation values
                    productDetails = this.productDetailsDictionary.getItem(orderLine.productNumber);

                    if (productDetails.IsMasterProduct) {
                        dimensionIds = [
                            { dimensionKey: Model.Entities.DimensionKeys.COLOR, dimensionValueId: orderLine.colorId },
                            { dimensionKey: Model.Entities.DimensionKeys.CONFIGURATION, dimensionValueId: orderLine.configurationId },
                            { dimensionKey: Model.Entities.DimensionKeys.SIZE, dimensionValueId: orderLine.sizeId },
                            { dimensionKey: Model.Entities.DimensionKeys.STYLE, dimensionValueId: orderLine.styleId },
                        ];
                        productVariant = ProductPropertiesHelper.getVariantFromDimensionIdValues(productDetails, dimensionIds);

                        orderLine.colorTranslation = productVariant.Color;
                        orderLine.configurationTranslation = productVariant.Configuration;
                        orderLine.sizeTranslation = productVariant.Size;
                        orderLine.styleTranslation = productVariant.Style;
                    }
                } else {
                    orderLine.colorTranslation = orderLine.colorId;
                    orderLine.configurationTranslation = orderLine.configurationId;
                    orderLine.sizeTranslation = orderLine.sizeId;
                    orderLine.styleTranslation = orderLine.styleId;
                }
            }

            this.lineDetails(newOrderLines);
            this.totalProducts = newOrderLines.length;
        }

        private setProductIndex(recordId: number, itemId: string, colorId: string, configId: string, sizeId: string, styleId: string, value: number) {

            // Set key by record identifier in case user is
            // selecting a product on UI.
            if (!NumberExtensions.isNullOrZero(recordId)) {
                this._productIndexes[recordId] = value;
            }

            // We also need to set key by product number and dimensions
            // In case user is searching existing product by typing on the numpad
            // or do search product.
            if (StringExtensions.isNullOrWhitespace(colorId)) { colorId = ""; }
            if (StringExtensions.isNullOrWhitespace(configId)) { configId = ""; }
            if (StringExtensions.isNullOrWhitespace(sizeId)) { sizeId = ""; }
            if (StringExtensions.isNullOrWhitespace(styleId)) { styleId = ""; }

            var productKey: string = this._dimensionDelimiter + itemId + this._dimensionDelimiter + colorId + this._dimensionDelimiter + configId + this._dimensionDelimiter + sizeId + this._dimensionDelimiter + styleId + this._dimensionDelimiter;

            // Assuming there are more than one pick and receive line with the same product,
            // give precedence to the first line
            // when user search product on numpad or through product search.
            if (ObjectExtensions.isNullOrUndefined(this._productIndexes[productKey])) {
                this._productIndexes[productKey] = value;
            }
        }

        public getProductIndex(recordId: number, itemId: string, colorId: string, configId: string, sizeId: string, styleId: string): number {
            var result: number;

            if (!NumberExtensions.isNullOrZero(recordId)) {
                result = this._productIndexes[recordId];
            } else {
                if (StringExtensions.isNullOrWhitespace(colorId)) { colorId = ""; }
                if (StringExtensions.isNullOrWhitespace(configId)) { configId = ""; }
                if (StringExtensions.isNullOrWhitespace(sizeId)) { sizeId = ""; }
                if (StringExtensions.isNullOrWhitespace(styleId)) { styleId = ""; }

                result = this._productIndexes[this._dimensionDelimiter + itemId + this._dimensionDelimiter + colorId + this._dimensionDelimiter + configId + this._dimensionDelimiter + sizeId + this._dimensionDelimiter + styleId + this._dimensionDelimiter];
            }

            if (result >= 0) {
                return result;
            } else {
                return -1;
            }
        }

        public updateJournal(asyncResult: VoidAsyncResult, journal: Model.Entities.PickingAndReceivingOrder, updateHeader: boolean): void {

            var journalLines: Model.Entities.PickingAndReceivingOrderLine[] = journal.orderLines;

            //get item ids for each journal line that are not exist on product dictionary.
            var itemIdsDictionary: Dictionary<boolean> = new Dictionary<boolean>();
            var itemId: string;
            for (var i = 0; i < journalLines.length; i++) {
                itemId = journalLines[i].productNumber;
                if (!this.productDetailsDictionary.hasItem(itemId) && !itemIdsDictionary.hasItem(itemId)) {
                    itemIdsDictionary.setItem(itemId, true);
                }
            }

            if (itemIdsDictionary.length() > 0) {

                //get product details for item ids to be updated on the dictionary.
                this.productManager.searchProductsByItemIdsAsync(itemIdsDictionary.getKeys())
                    .done((products: Model.Entities.Product[]) => {
                        products.forEach((value: Model.Entities.Product) => {

                            //add product to the dictionary.
                            this.productDetailsDictionary.setItem(value.ItemId, value);

                            itemIdsDictionary.removeItem(value.ItemId);
                        });

                        //update product lines and journal header
                        if (updateHeader) {
                            this.journalHeader(journal);
                        } 

                        this.getJournalDetailsSuccess(journal.orderLines);
                        asyncResult.resolve();
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewModelPickingAndReceivingDetailsSearchProductsByIdFailed();
                        asyncResult.reject(errors);
                    });
            } else {

                //update journal header
                if (updateHeader) {
                    this.journalHeader(journal);
                }

                this.getJournalDetailsSuccess(journal.orderLines);

                asyncResult.resolve();
            }
        }

        /**
         * Loads picking and receiving journal.
         */
        public loadJournal(): IVoidAsyncResult {
            var options: Operations.IPickingAndReceivingOperationOptions = {
                journalEntity: undefined,
                journalType: this._journalType,
                operationType: Model.Entities.PickingAndReceivingOperationType.GetJournalDetails,
                viewModel: this
            };

            return this.operationsManager.runOperation(
                Commerce.Operations.RetailOperation.PickingAndReceiving, options);
        }

        /**
         * Saves picking and receiving journal.
         */
        public saveJournal(): IVoidAsyncResult {
            var options: Operations.IPickingAndReceivingOperationOptions = {
                journalEntity: this.convertToJournalEntity(),
                journalType: this.journalHeader().orderType,
                operationType: Model.Entities.PickingAndReceivingOperationType.Save,
                viewModel: this
            };

            return this.operationsManager.runOperation(
                Commerce.Operations.RetailOperation.PickingAndReceiving, options);
        }

        /**
         * Commits picking and receiving journal.
         */
        public commitJournal(): IVoidAsyncResult {
            var options: Operations.IPickingAndReceivingOperationOptions = {
                journalEntity: this.convertToJournalEntity(),
                journalType: this.journalHeader().orderType,
                operationType: Model.Entities.PickingAndReceivingOperationType.Commit,
                viewModel: this
            };

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.operationsManager.runOperation(
                        Commerce.Operations.RetailOperation.PickingAndReceiving, options);
                }).enqueue(() => {
                    if (this.journalHeader().orderType != Model.Entities.PurchaseTransferOrderType.PickingList) {
                        return this.loadJournal();
                    }

                    return null;
                });

            return asyncQueue.run();
        }

        /**
         * Search for products given a text.
         *
         * @param {string} searchText The text to be searched.
         * @return {IAsyncResult<Model.Entities.Product[]>} The async result.
         */
        public searchProduct(searchText: string): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.searchBarcodeOrProductsAsync(searchText, 4, true);
        }

        public receiveAllLines(): void {
            var productLines: Model.Entities.PickingAndReceivingOrderLine[] = this.lineDetails();

            var receivedNow: number;
            for (var i = 0; i < productLines.length; i++) {
                //get the rest of received now = quantity ordered - purchase received
                receivedNow = productLines[i].quantityOrdered - productLines[i].quantityReceived;

                if (receivedNow >= 1) {
                    productLines[i].quantityReceivedNow = receivedNow;
                }
            }

            this.lineDetails(productLines);
        }

        /**
         * Add new product line to the journal.
         *
         * @param {number} variantId The variant identifier associated with the product.
         * @param {Model.Entities.Product} product The product entity to be added in journal product lines
         * @param {number} newQuantity The quantity to be added / updated.
         * @return {number} The product line index.
         */
        public addNewProductLine(variantId: number, product: Model.Entities.Product, newQuantity: number): number {

            //this is a new product, add the respected product on list of product lines
            var newProductLine: Model.Entities.PickingAndReceivingOrderLine = Model.Entities.PickingAndReceivingOrderHelper.createPickingAndReceivingOrderLine(
                variantId,
                product,
                0,
                0,
                newQuantity,
                this.journalHeader().orderType,
                this.journalId);

            this.lineDetails.push(newProductLine);
            this.totalProducts++;
            var index: number = this.totalProducts - 1; //insert new product line on the last index;
            this.setProductIndex(0, newProductLine.productNumber, newProductLine.colorId, newProductLine.configurationId, newProductLine.sizeId, newProductLine.styleId, index);

            return index;
        }

        /**
         * Adds a new quantity to the current line quantity.
         *
         * @param {number} index The line being changed.
         * @param {number} newQuantity The amount to increase the quantity by.
         *
         * @return {Model.Entities.Error[] A list of validation errors, if found.
         */
        public addToExistingProductLine(index: number, newQuantity: number): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = [];
            var lines: Model.Entities.PickingAndReceivingOrderLine[] = this.lineDetails();
            var productLine: Model.Entities.PickingAndReceivingOrderLine = lines[index];

            if (newQuantity < 0) {
                errors.push(new Model.Entities.Error("string_3388")); // The quantity is not valid. Select a line to directly edit the quantity.
            }

            if (index >= lines.length || index < 0) {
                errors.push(new Model.Entities.Error("string_3389")); // An error occurred while trying to select that line. Please refresh the page and try again.
            }

            if (!ArrayExtensions.hasElements(errors)) {
                productLine.quantityReceivedNow += newQuantity;
                this.lineDetails.splice(index, 1, productLine);
            }

            return errors;
        }

        /**
         * Replaces the line quantity with a new value.
         *
         * @param {number} index The line being changed.
         * @param {number} newQuantity The quantity to replace the previous one.
         *
         * @return {Model.Entities.Error[] A list of validation errors, if found.
         */
        public overwriteExistingProductLine(index: number, newQuantity: number): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = [];
            var lines: Model.Entities.PickingAndReceivingOrderLine[] = this.lineDetails();
            var productLine: Model.Entities.PickingAndReceivingOrderLine = lines[index];

            if (newQuantity < 0) {
                errors.push(new Model.Entities.Error("string_3388")); // The quantity is not valid. Select a line to directly edit the quantity.
            }

            if (index >= lines.length || index < 0) {
                errors.push(new Model.Entities.Error("string_3389")); // An error occurred while trying to select that line. Please refresh the page and try again.
            }

            if (!ArrayExtensions.hasElements(errors)) {
                productLine.quantityReceivedNow = newQuantity;
                this.lineDetails.splice(index, 1, productLine);
            }

            return errors;
        }
    }
}
