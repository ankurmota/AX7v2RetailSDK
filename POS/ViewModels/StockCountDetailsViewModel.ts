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

    export enum StockCountDetailsMode {
        Add,
        Edit
    }

    export interface StockCountDetailsViewModelParameters {
        JournalId?: string;
        Product?: Model.Entities.Product;
        VariantId?: number;
        ProductDetailsDictionary?: Dictionary<Model.Entities.Product>;
        AllJournals?: ObservableArray<Model.Entities.StockCountJournal>;
    }

    export class StockCountDetailsViewModel extends ViewModelBase {

        private _mode: StockCountDetailsMode;
        private _dimensionDelimiter: string = "|";
        private _productIndexes: number[][]; //keys to search product faster within lines
        private _deletedIndexes: number[];
        private _counter: number; //counter that will increase each time product is added.
        public defaultJournalDescription: string;
        public journalId: Observable<string>;
        public stockCountJournal: Observable<Model.Entities.StockCountJournal>;
        public productLines: ObservableArray<Model.Entities.StockCountLine>;
        public productDetailsDictionary: Dictionary<Model.Entities.Product>;
        public currentProductIndex: number;
        public totalProducts: Observable<number>;
        public totalCounted: Observable<number>;
        public totalQuantity: Observable<number>;

        constructor(options?: StockCountDetailsViewModelParameters) {
            super();
            this.defaultJournalDescription = "";
            this.stockCountJournal = ko.observable({ JournalId: "", StockCountTransactionLines: [] });
            this.productLines = ko.observableArray<Model.Entities.StockCountLine>([]);
            this.totalProducts = ko.observable(this.productLines().length);
            this.totalCounted = ko.observable(0);
            this.totalQuantity = ko.observable(0);
            this._counter = 0;
            this._productIndexes = [[]];
            this._deletedIndexes = [];

            options = options || {};
            if (options.JournalId) {
                this._mode = StockCountDetailsMode.Edit;
                this.journalId = ko.observable(options.JournalId);
            } else {
                this._mode = StockCountDetailsMode.Add;
                this.journalId = ko.observable("");
            }

            if (options.ProductDetailsDictionary) {
                this.productDetailsDictionary = options.ProductDetailsDictionary;
            } else {
                this.productDetailsDictionary = new Dictionary<Model.Entities.Product>();
            }
        }

        public updateJournal(asyncResult: VoidAsyncResult, journal: Model.Entities.StockCountJournal, updateHeader: boolean): void {

            var journalLines: Model.Entities.StockCountJournalTransaction[] = journal.StockCountTransactionLines;

            //get item ids for each journal line that are not exist on product dictionary.
            var itemIds: string[] = [];
            for (var i = 0; i < journalLines.length; i++) {
                if (!this.productDetailsDictionary.hasItem(journalLines[i].ItemId)) {
                    itemIds.push(journalLines[i].ItemId);
                }
            }

            if (ArrayExtensions.hasElements(itemIds)) {

                //get product details for item ids to be updated on the dictionary.
                this.productManager.searchProductsByItemIdsAsync(itemIds)
                    .done((products: Model.Entities.Product[]) => {
                        products.forEach((value: Model.Entities.Product) => {

                            //add product to the dictionary.
                            this.productDetailsDictionary.setItem(value.ItemId, value);
                        });

                        //update product lines and journal header
                        if (updateHeader) {
                            this.getJournalHeaderDetailsSuccess(journal);
                        }

                        this.getJournalLineDetailsSuccess(journalLines);
                        asyncResult.resolve();
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.viewModelStockCountDetailsSearchProductsByItemsFailed();
                        asyncResult.reject(errors);
                    });
            } else {

                //update journal header
                if (updateHeader) {
                    this.getJournalHeaderDetailsSuccess(journal);
                }

                this.getJournalLineDetailsSuccess(journal.StockCountTransactionLines);

                asyncResult.resolve();
            }
        }

        private getJournalLineDetailsSuccess(newProductLines: Model.Entities.StockCountJournalTransaction[]) {

            this.stockCountJournal().StockCountTransactionLines = newProductLines;

            this.productLines.removeAll();
            this._productIndexes = [[]]; //reset product indexes
            this._deletedIndexes = []; //reset deleted indexes
            this._counter = newProductLines.length;
            var totalCountedCurrent: number = 0;
            var totalQuantityCurrent: number = 0;

            var productLine: Proxy.Entities.StockCountLine;
            var stockCountProductLine: Proxy.Entities.StockCountJournalTransaction;
            var productFound: Proxy.Entities.Product;
            var productVariant: Proxy.Entities.ProductVariant = new Proxy.Entities.ProductVariantClass();
            var productLineCollection: Proxy.Entities.StockCountLine[] = [];

            for (var i: number = 0; i < newProductLines.length; i++) {
                stockCountProductLine = newProductLines[i];
                productFound = this.productDetailsDictionary.getItem(stockCountProductLine.ItemId);

                if (!ObjectExtensions.isNullOrUndefined(productFound) && productFound.IsMasterProduct) {
                    var dimensionIdValues: Model.Entities.ProductDimensionResult[] = [];
                    dimensionIdValues.push({ dimensionKey: Model.Entities.DimensionKeys.COLOR, dimensionValueId: stockCountProductLine.InventColorId });
                    dimensionIdValues.push({ dimensionKey: Model.Entities.DimensionKeys.CONFIGURATION, dimensionValueId: stockCountProductLine.ConfigId });
                    dimensionIdValues.push({ dimensionKey: Model.Entities.DimensionKeys.SIZE, dimensionValueId: stockCountProductLine.InventSizeId });
                    dimensionIdValues.push({ dimensionKey: Model.Entities.DimensionKeys.STYLE, dimensionValueId: stockCountProductLine.InventStyleId });

                    productVariant = ProductPropertiesHelper.getVariantFromDimensionIdValues(productFound, dimensionIdValues);
                } else if (ObjectExtensions.isNullOrUndefined(productFound)) {

                    // Handle unassorted products.
                    var isMasterProduct: boolean = !StringExtensions.isNullOrWhitespace(stockCountProductLine.ConfigId) ||
                        !StringExtensions.isNullOrWhitespace(stockCountProductLine.InventColorId) ||
                        !StringExtensions.isNullOrWhitespace(stockCountProductLine.InventSizeId) ||
                        !StringExtensions.isNullOrWhitespace(stockCountProductLine.InventStyleId);

                    if (isMasterProduct) {
                        productVariant.ColorId = stockCountProductLine.InventColorId;
                        productVariant.ConfigId = stockCountProductLine.ConfigId;
                        productVariant.SizeId = stockCountProductLine.InventSizeId;
                        productVariant.StyleId = stockCountProductLine.InventStyleId;
                    }
                }

                productLine = new Model.Entities.StockCountLineClass(stockCountProductLine, productVariant);

                productLine.counted = parseFloat(productLine.counted.toString());
                productLine.quantity = parseFloat(productLine.quantity.toString());

                totalCountedCurrent += productLine.counted;
                totalQuantityCurrent += productLine.quantity;

                productLineCollection.push(productLine);
                this.setProductIndex(productLine.recordId, productLine.itemId, productLine.inventColorId, productLine.configId, productLine.inventSizeId, productLine.inventStyleId, i);

            }

            this.productLines(productLineCollection);

            this.totalCounted(totalCountedCurrent);
            this.totalQuantity(totalQuantityCurrent);
            this.totalProducts(newProductLines.length);
        }

        private getJournalHeaderDetailsSuccess(newJournal: Model.Entities.StockCountJournal) {
            this.journalId(newJournal.JournalId);
            this.stockCountJournal(newJournal);
        }

        private setProductIndex(recordId: number, itemId: string, colorId: string, configurationId: string, sizeId: string, styleId: string, value: number) {

            // Set key by record identifier in case user is
            // selecting a product on UI.
            if (!NumberExtensions.isNullOrZero(recordId)) {
                this._productIndexes[recordId] = [value];
            }

            // We also need to set key by product number and dimensions
            // In case user is searching existing product by typing on the numpad
            // or do search product.
            if (StringExtensions.isNullOrWhitespace(colorId)) { colorId = StringExtensions.EMPTY; }
            if (StringExtensions.isNullOrWhitespace(configurationId)) { configurationId = StringExtensions.EMPTY; }
            if (StringExtensions.isNullOrWhitespace(sizeId)) { sizeId = StringExtensions.EMPTY; }
            if (StringExtensions.isNullOrWhitespace(styleId)) { styleId = StringExtensions.EMPTY; }

            var productKey: string = this._dimensionDelimiter + itemId + this._dimensionDelimiter + colorId + this._dimensionDelimiter + configurationId + this._dimensionDelimiter + sizeId + this._dimensionDelimiter + styleId + this._dimensionDelimiter;

            // Assuming there are more than one stock count line with the same product,
            // give precedence to the first line
            // when user search product on numpad or through product search.
            if (ObjectExtensions.isNullOrUndefined(this._productIndexes[productKey])) {
                this._productIndexes[productKey] = [value];
            } else {
                this._productIndexes[productKey].push(value);
            }
        }

        private countStepBack(indexResult: number): number {
            var stepBack: number = 0;
            for (var i = 0; i < this._deletedIndexes.length; i++) {
                if (indexResult - stepBack == 0) {
                    return 0;
                }

                if (this._deletedIndexes[i] < indexResult) {
                    stepBack++;
                }
            }

            return stepBack;
        }

        private getOriginalIndex(recordId: number, itemId: string, colorId: string, configurationId: string, sizeId: string, styleId: string): number {
            var result: number = -1;

            if (!NumberExtensions.isNullOrZero(recordId)) {
                result = this._productIndexes[recordId][0];
            } else {
                if (StringExtensions.isNullOrWhitespace(colorId)) { colorId = StringExtensions.EMPTY; }
                if (StringExtensions.isNullOrWhitespace(configurationId)) { configurationId = StringExtensions.EMPTY; }
                if (StringExtensions.isNullOrWhitespace(sizeId)) { sizeId = StringExtensions.EMPTY; }
                if (StringExtensions.isNullOrWhitespace(styleId)) { styleId = StringExtensions.EMPTY; }

                var productKey: string = this._dimensionDelimiter + itemId + this._dimensionDelimiter + colorId + this._dimensionDelimiter + configurationId + this._dimensionDelimiter + sizeId + this._dimensionDelimiter + styleId + this._dimensionDelimiter;

                if (ArrayExtensions.hasElements(this._productIndexes[productKey])) {
                    result = this._productIndexes[productKey][0];
                }
            }

            return result;
        }

        public getProductIndex(recordId: number, itemId: string, colorId: string, configurationId: string, sizeId: string, styleId: string): number {
            var result: number = this.getOriginalIndex(recordId, itemId, colorId, configurationId, sizeId, styleId);

            if (result >= 0) {

                //since _productIndexes might contain products that already removed, need to check with _deletedIndexes here.
                //each time a deleted index is smaller / same as the result, decrease the result index. How many moves to be decreased is the stepBack counter.
                var stepBack: number = this.countStepBack(result);
                return result - stepBack;
            } else {
                return -1;
            }
        }

        /**
         * Load the journal, when mode is edit.
         *
         * @param {IVoidAsyncResult} The async result.
         */
        public loadJournal(): IVoidAsyncResult {
            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (this._mode === StockCountDetailsMode.Edit) {
                asyncQueue.enqueue((): IAsyncResult<any> => {
                    var options: Operations.IStockCountOperationOptions = {
                        operationType: Model.Entities.StockCountOperationType.GetDetails,
                        stockCountJournal: undefined,
                        viewModel: this
                    };

                    return this.operationsManager.runOperation(Commerce.Operations.RetailOperation.StockCount, options);
                }).enqueue((): IAsyncResult<any> => {
                    // try to load journal from AX if product is empty
                    if (!ArrayExtensions.hasElements(this.productLines())) {
                        return this.refreshProductLines();
                    }

                    return VoidAsyncResult.createResolved();
                });
            }

            return asyncQueue.run();
        }

        public searchProduct(searchText: string): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.searchBarcodeOrProductsAsync(searchText, 4, true);
        }

        /**
         * Saves the current journal.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public saveJournal(): IVoidAsyncResult {
            var journal = this.stockCountJournal();
            var stockCountOperationType: Model.Entities.StockCountOperationType;

            if (!StringExtensions.isNullOrWhitespace(journal.JournalId)) {
                //journal is already created. Do update operation.
                stockCountOperationType = Model.Entities.StockCountOperationType.Update;
            } else {
                //journal is not yet created. Do create operation.
                stockCountOperationType = Model.Entities.StockCountOperationType.Create;
                journal.Description = this.defaultJournalDescription;
            }

            var options: Operations.IStockCountOperationOptions = {
                operationType: stockCountOperationType,
                stockCountJournal: journal,
                viewModel: this
            };

            var result = this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options);

            return result.done(() => { this._mode = StockCountDetailsMode.Edit; });
        }

        /**
         * Commits the current journal.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public commitJournal(): IVoidAsyncResult {
            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.Commit,
                stockCountJournal: this.stockCountJournal(),
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options)
        }

        /**
         * Deletes the current product line.
         * @param {number} recordId The stock count line product identifier to be deleted.
         * @param {string} productId The product identifier to be deleted.
         * @param {string} productVariant The product variant to be deleted.
         * @return {IVoidAsyncResult} The async result.
         */
        public deleteProductLine(recordId: number, productId: string, productVariant: Model.Entities.ProductVariant): IVoidAsyncResult {
            this.currentProductIndex = this.getProductIndex(recordId, productId, productVariant.ColorId, productVariant.ConfigId, productVariant.SizeId, productVariant.StyleId);
            var productLinesArray: Model.Entities.StockCountLine[] = this.productLines();
            var productCounted: number = productLinesArray[this.currentProductIndex].counted;
            var productQuantity: number = productLinesArray[this.currentProductIndex].quantity;

            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.RemoveProductLine,
                stockCountJournal: this.stockCountJournal(),
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options)
                .done(() => {
                    this._deletedIndexes.push(this.currentProductIndex); //add deleted indexes

                    //because product is deleted, set the respective product index to -1.
                    //we are not deleting product indexes so we don't have to reset all the indexes to enhance performance.
                    if (!NumberExtensions.isNullOrZero(recordId)) {
                        this._productIndexes[recordId][0] = -1;
                    }
                    
                    var originalIndex: number = this.getOriginalIndex(recordId, productId, productVariant.ColorId, productVariant.ConfigId, productVariant.SizeId, productVariant.StyleId);
                    var productKey: string = this._dimensionDelimiter + productId +
                        this._dimensionDelimiter + (StringExtensions.isNullOrWhitespace(productVariant.ColorId) ? StringExtensions.EMPTY : productVariant.ColorId) +
                        this._dimensionDelimiter + (StringExtensions.isNullOrWhitespace(productVariant.ConfigId) ? StringExtensions.EMPTY : productVariant.ConfigId) +
                        this._dimensionDelimiter + (StringExtensions.isNullOrWhitespace(productVariant.SizeId) ? StringExtensions.EMPTY : productVariant.SizeId) +
                        this._dimensionDelimiter + (StringExtensions.isNullOrWhitespace(productVariant.StyleId) ? StringExtensions.EMPTY : productVariant.StyleId) +
                        this._dimensionDelimiter;
                    var index: number = this._productIndexes[productKey].indexOf(originalIndex);
                    this._productIndexes[productKey].splice(index, 1);
                    
                    this.totalQuantity(this.totalQuantity() - productQuantity);
                    this.totalCounted(this.totalCounted() - productCounted);
                    this.totalProducts(this.totalProducts() - 1);

                    this.stockCountJournal().StockCountTransactionLines.splice(this.currentProductIndex, 1);
                    this.productLines.splice(this.currentProductIndex, 1);
                });
        }

        /**
         * Add new product line to the journal. If product line is already exists, then just update the quantity.
         *
         * @param {number} variantId The variant identifier associated with the product.
         * @param {Model.Entities.Product} product The product entity to be added in journal product lines
         * @param {number} newQuantity The quantity to be added / updated.
         * @return {number} The product line index.
         */
        public addNewProductLine(variantId: number, product: Model.Entities.Product, newQuantity: number): number {
            var itemId: string = ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductNumber);
            var inventoryDimId: string = ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.InventoryDimensionId);
            var productName: string = ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductName);

            var productVariant: Model.Entities.ProductVariant = new Model.Entities.ProductVariantClass();

            if (product.IsMasterProduct) {
                productVariant = ProductPropertiesHelper.getVariant(variantId, product);
            }

            //this is a new product, add the respected product on list of product lines
            var newProductLine: Model.Entities.StockCountJournalTransaction = {
                JournalId: this.journalId(),
                ItemId: itemId,
                InventDimId: inventoryDimId,
                InventColorId: productVariant.ColorId,
                ConfigId: productVariant.ConfigId,
                InventSizeId: productVariant.SizeId,
                InventStyleId: productVariant.StyleId,
                ItemName: productName,
                Quantity: newQuantity,
                Counted: 0,
                Status: Model.Entities.StockCountStatus.PendingUpdate
            };

            this.totalQuantity(this.totalQuantity() + newQuantity);
            this.totalProducts(this.totalProducts() + 1);

            this.stockCountJournal().StockCountTransactionLines.push(newProductLine);
            this.productLines.push(new Model.Entities.StockCountLineClass(newProductLine, productVariant));
            var index = this._counter++; //insert new product line on the last index;
            this.setProductIndex(0, itemId, productVariant.ColorId, productVariant.ConfigId, productVariant.SizeId, productVariant.StyleId, index);

            return index;
        }

        /*
         * Increases the quantity of the product line.
         *
         * @param {number} index The index of the line that will be edited
         * @param {number} newQuantity The amount that the quantity increases by
         *
         * @returns {Model.Entities.Error[]} The list of error validations, if found.
         */
        public addExistingProductLine(index: number, newQuantity: number): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = []
            var lines: Model.Entities.StockCountJournalTransaction[] = this.productLines();

            if (newQuantity < 0) {
                errors.push(new Model.Entities.Error("string_3388"));
            }

            if (index >= lines.length || index < 0) {
                errors.push(new Model.Entities.Error("string_3389"));
            }

            if (!ArrayExtensions.hasElements(errors)) {
                var productLine = this.productLines()[index];
                productLine.quantity += newQuantity;
                productLine.status = Model.Entities.StockCountStatus.PendingUpdate;
                this.totalQuantity(this.totalQuantity() + newQuantity); // update total quantities
                this.productLines.splice(index, 1, productLine);
            }

            return errors;
        }

        /**
         * Overwrite the stock count product line
         *
         * @param {number} index The index number.
         * @param {number} newQuantity The quantity that overrides the old quantity.
         *
         * @returns {Model.Entities.Error[]} The list of error validations, if found.
         */
        public overwriteExistingProductLine(index: number, newQuantity: number): Model.Entities.Error[] {
            var errors: Model.Entities.Error[] = []
            var lines: Model.Entities.StockCountJournalTransaction[] = this.productLines();
            
            if (newQuantity < 0) {
                errors.push(new Model.Entities.Error("string_3388"));
            }

            if (index >= lines.length || index < 0) {
                errors.push(new Model.Entities.Error("string_3389"));
            }

            if (!ArrayExtensions.hasElements(errors)) {
                var productLine = this.productLines()[index];
                productLine.quantity = newQuantity;
                productLine.status = Model.Entities.StockCountStatus.PendingUpdate;
                this.totalQuantity(newQuantity); // overwrite total quantities
                this.productLines.splice(index, 1, productLine);
            }

            return errors;
        }

        /**
         * Refresh the commited product lines in the current journal to be edited.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public refreshProductLines(): IVoidAsyncResult {
            var options: Operations.IStockCountOperationOptions = {
                operationType: Model.Entities.StockCountOperationType.SyncOne,
                stockCountJournal: this.stockCountJournal(),
                viewModel: this
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.StockCount, options)
        }
    }
}
