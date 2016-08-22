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

    export class KitDisassemblyViewModel extends ViewModelBase {
        private _options: any;

        public kiproduct: Commerce.Model.Entities.Product;
        public kitId: Observable<string>;
        public kitName: Observable<string>;
        public kitQuantity: Observable<number>;
        public componentLines: ObservableArray<Commerce.ViewModels.ComponentLine>;
        public kitVariantId: number;
        public inventDimId: string;
        public selectedComponentIndex: number;
        public disassembleAtRegister: boolean;

        constructor(options?: any) {
            super();

            this._options = options;
            this.kitId = ko.observable("");
            this.kitName = ko.observable("");
            this.kitQuantity = ko.observable(1);
            this.componentLines = ko.observableArray<Commerce.ViewModels.ComponentLine>([]);
            this.inventDimId = "";
            this.selectedComponentIndex = -1;
        }

        /**
         * Loads the view model.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public load(): IVoidAsyncResult {
            if (!ObjectExtensions.isNullOrUndefined(this._options)
                && !ObjectExtensions.isNullOrUndefined(this._options.VariantId)) {

                if (!ObjectExtensions.isNullOrUndefined(this._options.Product)) {
                    return this.populateViewModel(
                        this._options.Product, this._options.VariantId, this._options.kitComponentProducts, this._options.quantityToDisassemble);
                } else {
                    return this.productManager.getProductDetailsAsync(this._options.kitProductVariantId)
                        .done((products: Model.Entities.Product[]) => {
                            RetailLogger.viewModelKitDisassemblyRetrievedKitProduct();
                            this.populateViewModel(products[0], this._options.kitProductVariantId, this._options.kitComponentProducts);
                        });
                }
            }

            return VoidAsyncResult.createResolved();
        }

        private loadComponents(kitProduct: Model.Entities.Product, kitComponentKeys: Commerce.Model.Entities.KitComponentKey[], componentProducts?: Model.Entities.Product[]): IVoidAsyncResult {
            var componentProductIds: number[] = kitComponentKeys.map((line) => {return line.DistinctProductId });
            if (ObjectExtensions.isNullOrUndefined(componentProducts)) {
                return this.productManager.getProductDetailsAsync(componentProductIds)
                    .done((componentProducts: Model.Entities.Product[]) => {
                        this.createComponentLines(kitProduct, kitComponentKeys, componentProducts);
                    });
            } else {
                this.createComponentLines(kitProduct, kitComponentKeys, componentProducts);
            }

            return VoidAsyncResult.createResolved();
        }

        private createComponentLines(kitProduct: Model.Entities.Product, kitComponentKeys: Commerce.Model.Entities.KitComponentKey[], componentProducts: Model.Entities.Product[]) {
            this.componentLines.removeAll();
            for (var i: number = 0; i < kitComponentKeys.length; i++) {

                var componentProduct: Model.Entities.Product = componentProducts.filter(
                    line => {
                        if (line.RecordId == kitComponentKeys[i].DistinctProductId) {
                            return true;
                        } else if (line.IsMasterProduct &&
                            line.CompositionInformation.VariantInformation.Variants.filter(variant =>
                                variant.DistinctProductVariantId === kitComponentKeys[i].DistinctProductId).length == 1) {
                            return true;
                        } else {
                            return false;
                        }
                    })[0];

                var newComponentLine: Commerce.ViewModels.ComponentLine = new ComponentLine(kitProduct, componentProduct, kitComponentKeys[i], this.kitQuantity());
                newComponentLine.index = i;
                this.componentLines.push(newComponentLine);
            }
        }

        /**
         * Search and get product kits matching the given string.
         *
         * @param {string} string to be searched.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public searchProduct(searchText: string): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.searchBarcodeOrProductsAsync(searchText, 4, true);
        }

        /**
         * Populates the view model with the currently selected kit information.
         *
         * @param {Model.Entities.Product} selected kit product.
         * @param {number} currently selected kit variant Id.
         */
        public populateViewModel(newKitProduct: Model.Entities.Product, selectedKitVariantId: number, componentProducts?: Model.Entities.Product[], quantityToDisassemble?: number): IVoidAsyncResult {
            this.kiproduct = newKitProduct;
            this.kitId(ProductPropertiesHelper.getProperty(newKitProduct.RecordId, newKitProduct, ProductPropertyNameEnum.ProductNumber));
            this.kitName(ProductPropertiesHelper.getProperty(newKitProduct.RecordId, newKitProduct, ProductPropertyNameEnum.ProductName));
            this.kitVariantId = selectedKitVariantId;
            this.disassembleAtRegister = newKitProduct.CompositionInformation.KitDefinition.DisassembleAtRegister;

            if (Commerce.ObjectExtensions.isNullOrUndefined(quantityToDisassemble)) {
                quantityToDisassemble = 1;
            }

            this.kitQuantity(quantityToDisassemble);

            var variantProduct: Model.Entities.ProductVariant = newKitProduct.CompositionInformation.VariantInformation.Variants
                .filter(line => line.DistinctProductVariantId === selectedKitVariantId)[0];
            this.inventDimId = variantProduct.InventoryDimensionId;

            var x = newKitProduct.CompositionInformation.KitDefinition.KitVariantToComponentMap
                .filter(line => line.KitVariantId === selectedKitVariantId)[0];

            return this.loadComponents(newKitProduct, x.KitComponentKeyList, componentProducts);
        }

        /**
         * Sets the kit quantity to be disassembled.
         */
        public setQuantityToDisassemble(quantityToDisassemble: number) {
            this.kitQuantity(quantityToDisassemble);
            this.resetAddToSaleQuantities();
        }

        /**
         * Resets the add to sale quantity of all included components back to zero.
         */
        public resetAddToSaleQuantities() {
            var resetAddToSaleQtyVal: number = 0;
            for (var i: number = 0; i < this.componentLines().length; i++) {
                var selectedComponent: ComponentLine = this.componentLines()[i];
                var updatedComponentLine: ComponentLine = selectedComponent.getUpdatedComponentLine(resetAddToSaleQtyVal, this.kitQuantity());
                this.componentLines.splice(selectedComponent.index, 1, updatedComponentLine);
            }
        }

        /**
         * Clears content of the view model.
         */
        public clearViewModel() {
            this.kitName("");
            this.kitId("");
            this.kitQuantity(1);
            this.componentLines.removeAll();
            this.inventDimId = "";
            this.selectedComponentIndex = -1;
        }

        /**
         * Creates the kit (disassembly) transaction from the view model data and saves the transaction to the server.
         *
         * @return {IVoidAsyncResult>} The async result.
         */
        public commit(): IVoidAsyncResult {
            if (this.disassembleAtRegister == false) {
                RetailLogger.viewModelKitDisassemblyKitDisassemblyBlocked();

                var error = new Model.Entities.Error(ErrorTypeEnum.KIT_BLOCKED_FOR_DISASSEMBLY_AT_REGISTER);
                return VoidAsyncResult.createRejected([error]);
            }

            // Creates the kit (disassembly) transaction
            var kitTransLines: Commerce.Model.Entities.KitTransactionLine[] = [
                {
                    ItemId: this.kitId(),
                    Quantity: this.kitQuantity(),
                    InventoryDimensionId: this.inventDimId
                }
            ];

            var kitTrans: Commerce.Model.Entities.KitTransaction = {
                Id: "",
                TransactionTypeValue: 28,
                KitTransactionLines: kitTransLines
            };

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    return this.inventoryManager.saveKitTransactionAsync(kitTrans);
                }).enqueue(() => {
                    // Creates sales line for disassembled components.
                    var saleLines: Commerce.Model.Entities.ProductSaleReturnDetails[] = new Array();

                    for (var i: number = 0; i < this.componentLines().length; i++) {
                        var line: Model.Entities.ProductSaleReturnDetails = this.componentLines()[i].getSaleLineObj();
                        if (line != null) {
                            saleLines.push(line);
                        }
                    }

                    //// Adds disassembled components to the cart
                    if (saleLines.length > 0) {
                        var options: Operations.IItemSaleOperationOptions = {
                            productSaleDetails: saleLines
                        };

                        var operationResult = Operations.OperationsManager.instance.runOperation(
                            Operations.RetailOperation.ItemSale, options);

                        return asyncQueue.cancelOn(operationResult);
                    }

                    return VoidAsyncResult.createResolved();
                });

            return asyncQueue.run();
        }
    }

    export class ComponentLine {
        private component: Model.Entities.Product;
        public itemName: string;
        public itemId: string;
        public totalQuantity: number;
        public quantity: number;
        public unit: string;
        public addToCartQty: number;
        public returnToInventoryQty: number;
        public distinctProductId: number; // This could be product recordId if the component is standard product or it could be a variantId if the component is a productvariant.
                                          // since components are always saleable products (i.e. standalone or variant products) - this can never represent recordId of a master product.
        public index: number;
        public variantSummaryString: string;

        /**
         * constructor
         *
         * @param {Model.Entities.Product} The product object representing the kit that contains the current component.
         * @param {Model.Entities.Product} The product object representing the current component 
         * @param {number} ListingId of the current component product, If the component is a variant this value identifying the specific variant Id of the component
         *                 (if the component is a standalone product the value is same as the product's record identifier) 
         *
         */
        constructor(kitProduct?: Model.Entities.Product, component?: Model.Entities.Product, kitComponentKey?: Commerce.Model.Entities.KitComponentKey, kitQuantityToDisassemble?: number) {
            // Initialize the view model properties
            if (kitComponentKey) {
                var productId = kitComponentKey.DistinctProductId;

                if (component) {
                    this.itemId = ProductPropertiesHelper.getProperty(productId, component, ProductPropertyNameEnum.ProductNumber);
                    this.itemName = ProductPropertiesHelper.getProperty(productId, component, ProductPropertyNameEnum.ProductName);

                    this.distinctProductId = productId;
                    this.component = component;

                    // Creates and initializes the variant summary string
                    var dimensionValues: Model.Entities.ProductDimensionResult[];
                    this.variantSummaryString = "";

                    dimensionValues = ProductPropertiesHelper.getProperty(productId, component, ProductPropertyNameEnum.ProductDimensionValues);
                    dimensionValues.forEach(val => {
                        this.variantSummaryString = this.variantSummaryString + val.dimensionValueTranslation + ' : ';
                    });
                }

                if (kitProduct) {
                    var kitLineDefinition: Model.Entities.KitLineDefinition;

                    // Get the kit line definition that contains the current component product

                    kitLineDefinition = kitProduct.CompositionInformation.KitDefinition.KitLineDefinitions
                        .filter(line => line.KitLineIdentifier == kitComponentKey.KitLineIdentifier)[0];

                    // get the component property i.e. unit quantity and charge information of the current component product, 
                    // from the ComponentProperties collection.  
                    var selectedProperties = kitLineDefinition.ComponentProperties.filter(property => property.ProductId === productId);

                    // if the component property is not found using the listingId, try the product's recordId as its property is stored on the master product level 
                    if (selectedProperties.length == 0) {
                        selectedProperties = kitLineDefinition.ComponentProperties.filter(property => property.ProductId === component.RecordId);
                    }

                    if (Commerce.ObjectExtensions.isNullOrUndefined(kitQuantityToDisassemble)) {
                        kitQuantityToDisassemble = 1;
                    }

                    this.unit = selectedProperties[0].Unit;
                    this.quantity = selectedProperties[0].Quantity;
                    this.totalQuantity = this.quantity * kitQuantityToDisassemble;
                    this.returnToInventoryQty = this.totalQuantity;
                    this.addToCartQty = 0;
                }
            }
        }

        /**
         * creates an updated ComponentLine object whenever the view model changes the add to cart quantity property of the ComponentLine
         *
         * @param {number} The new add to cart quantity.
         * @return {Commerce.ViewModels.ComponentLine} The updated component line object.
         */
        public getUpdatedComponentLine(newAddToCartQty: number, quantityToDisassemble?: number): Commerce.ViewModels.ComponentLine {
            var updatedObj: Commerce.ViewModels.ComponentLine = new Commerce.ViewModels.ComponentLine();
            updatedObj.itemId = this.itemId;
            updatedObj.itemName = this.itemName;
            updatedObj.quantity = this.quantity;
            updatedObj.unit = this.unit;

            updatedObj.index = this.index;
            updatedObj.component = this.component;
            updatedObj.distinctProductId = this.distinctProductId;
            updatedObj.variantSummaryString = this.variantSummaryString;

            if (Commerce.ObjectExtensions.isNullOrUndefined(quantityToDisassemble)) {
                quantityToDisassemble = 1;
            }

            // set the updated add to cart quantity and update the return inventory quantity
            updatedObj.totalQuantity = quantityToDisassemble * updatedObj.quantity;
            updatedObj.addToCartQty = newAddToCartQty;
            updatedObj.returnToInventoryQty = updatedObj.totalQuantity - updatedObj.addToCartQty;

            return updatedObj;
        }

        public getSaleLineObj(): Proxy.Entities.ProductSaleReturnDetails {

            var saleLine: Proxy.Entities.ProductSaleReturnDetails = null;

            // If the component is a productvariant then set distinctProductId of the component line as variantId of the saleLine object.
            var productId: number = this.component.IsMasterProduct ? this.distinctProductId : this.component.RecordId;

            if (this.addToCartQty != 0) {
                saleLine = {
                    productId: productId,
                    quantity: this.addToCartQty,
                    unitOfMeasureSymbol: this.unit
                }
            }

            return saleLine;
        }

        public validateAddToCartQty(newAddToCartQty: number): boolean {
            return this.totalQuantity >= newAddToCartQty && newAddToCartQty >= 0;
        }
    }
}