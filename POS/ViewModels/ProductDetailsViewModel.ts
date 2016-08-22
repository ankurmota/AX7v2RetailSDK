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

    export class ProductDetailsViewModel extends ViewModelBase {

        // Common variables for both product details and kit details
        public product: Observable<Commerce.Model.Entities.Product>;
        public productDetails: Observable<any>;
        public images: ObservableArray<any>;
        public productProperty: ObservableArray<Commerce.Model.Entities.ProductPropertyTranslation>;
        public dimensionValue: ObservableArray<any>;
        public selectedVariant: Observable<Commerce.Model.Entities.ProductVariant>;
        // Variables for product details
        public relatedProducts: ObservableArray<Commerce.Model.Entities.Product>;
        public relatedProductsForDisplay: ObservableArray<Commerce.Model.Entities.Product>;

        // Variables for kit details
        public kitComponents: ObservableArray<Commerce.Model.Entities.Product>;

        // Observables used for display
        public kitComponentsForDisplay: ObservableArray<any>;
        public kitDimensions: ObservableArray<any>;

        public static MaxRelatedProductsForDisplay: number = 4;
        public static ProductVideoPropertyTypeValue: number = 40;

        constructor() {
            super();
            // Common variables for both product details and kit details
            this.product = ko.observable(null);
            this.images = ko.observableArray([]);

            // Variables for product details
            this.relatedProducts = ko.observableArray<Commerce.Model.Entities.Product>([]);
            this.relatedProductsForDisplay = ko.observableArray<Commerce.Model.Entities.Product>([]);
            this.productProperty = ko.observableArray<Commerce.Model.Entities.ProductPropertyTranslation>([]);
            this.dimensionValue = ko.observableArray([]);
            this.productDetails = ko.observable([]);

            this.selectedVariant = ko.observable(null);

            // Variables for kit details
            this.kitComponents = ko.observableArray([new Commerce.Model.Entities.ProductClass()]);

            // Observables used for display
            this.kitComponentsForDisplay = ko.observableArray([]);
            this.kitDimensions = ko.observableArray([]);
        }

        public Load() {
        }

        public getProductDetails(productIds: number[], arrangeProducts?: boolean): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.getProductDetailsAsync(productIds, arrangeProducts);
        }

        public getProductDetailsByDataLevel(productIds: number[], dataLevel: number, skipVariantExpansion: boolean): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.getProductDetailsByDataLevelAsync(productIds, dataLevel, skipVariantExpansion);
        }

        public getProductDetailsBySearchCriteria(productSearchCriteria: Model.Entities.ProductSearchCriteria): IAsyncResult<Model.Entities.Product[]> {
            return this.productManager.getProductDetailsBySearchCriteriaAsync(productSearchCriteria);
        }
        
        /**
        * Substitute Kit Component
        */
        public SubstituteKitComponent(productId: number, kitLineIdentifier: number, callerContext: any, callerCallback: any, errorCallerCallback: any) {
            if (ArrayExtensions.hasElements(this.product().CompositionInformation.KitDefinition.KitLineDefinitions)) {

                this.getProductDetails([productId])
                    .done((products: Model.Entities.Product[]) => {
                        var componentNumber = this.product().CompositionInformation.KitDefinition.KitLineDefinitions.map(function (x) { return x.KitLineIdentifier; }).indexOf(kitLineIdentifier);
                        this.kitComponents()[componentNumber] = this.ProcessKitComponents(products[0], componentNumber);
                        this.kitComponents(this.kitComponents());
                        callerCallback.call(callerContext, componentNumber);
                    }).fail((error: Model.Entities.Error[]) => {
                        errorCallerCallback.call(callerContext);
                    });
            } else {
                RetailLogger.viewModelProductDetailsComponentsNotInKit();
                errorCallerCallback.call(callerContext);
            }
        }

        /**
        * Loads components of the Kits
        */
        public LoadKitComponents(callerContext: any, variantId: number, callerCallback: any, errorCallerCallback: any) {
            if (ArrayExtensions.hasElements(this.product().CompositionInformation.KitDefinition.KitLineDefinitions)) {

                var ProductIds: number[] = [];

                if (this.product().RecordId === variantId) {
                    for (var componentNumber = 0; componentNumber < this.product().CompositionInformation.KitDefinition.KitLineDefinitions.length; componentNumber++) {
                        ProductIds.push(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentNumber].ComponentProductId);
                    }
                } else {
                    // Get the variant content of the cart product, which will give all the components of the kit variant
                    var kitVariantContent = this.product().CompositionInformation.KitDefinition.KitVariantToComponentMap.filter((line) => {
                        return line.KitVariantId == variantId;
                    })[0];
                    ProductIds = ProductIds.concat(kitVariantContent.KitComponentKeyList.map((line) => { return line.DistinctProductId; }));
                }

                this.getProductDetails(ProductIds)
                    .done((products: Model.Entities.Product[]) => {

                        var kitProducts: Model.Entities.Product[] = [];

                        this.product().CompositionInformation.KitDefinition.KitLineDefinitions.forEach((kitLine) => {

                            var KitLineProductIds: number[] = [kitLine.ComponentProductId];
                            KitLineProductIds = KitLineProductIds.concat(kitLine.SubstituteProductIds);

                            var filteredProducts = products.filter(function (product) {

                                if (KitLineProductIds.indexOf(product.RecordId) > -1) {
                                    return true;
                                } else if (product.IsMasterProduct) {
                                    var productVariant: Commerce.Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants.filter(function (variant) {
                                        if (KitLineProductIds.indexOf(variant.DistinctProductVariantId) > -1) {
                                            return true;
                                        }
                                    });
                                    if (ArrayExtensions.hasElements(productVariant)) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                }
                            });
                            kitProducts.push(<Model.Entities.Product>$.extend(true, {}, filteredProducts[0]));

                        });

                        for (var i = 0; i < kitProducts.length; i++) {
                            this.ProcessKitComponents(kitProducts[i], i);
                        }
                        this.kitComponents(kitProducts);
                        callerCallback.call(callerContext);
                    }).fail((error: Model.Entities.Error[]) => {
                        errorCallerCallback.call(callerContext);
                    });
            } else {
                RetailLogger.viewModelProductDetailsComponentsNotInKit();
                errorCallerCallback.call(callerContext);
            }
        }

        public LoadRelatedProducts(skip?: number, take?: number): IAsyncResult<Model.Entities.Product[]> {
            if (this.product().RelatedProducts.length > 0) {
                var relatedProductIds: number[] = [];
                this.product().RelatedProducts.forEach((relatedProduct) => { relatedProductIds.push(relatedProduct.RelatedProductRecordId); });

                return this.productManager.getProductDetailsByDataLevelAsync(relatedProductIds, 1, true) // 1 corresponds to data level
                    .done((products) => {
                        this.relatedProducts(products);
                        this.relatedProductsForDisplay(products.slice(0, Math.min(products.length, Commerce.ViewModels.ProductDetailsViewModel.MaxRelatedProductsForDisplay)));
                    });
            }

            return AsyncResult.createResolved<Model.Entities.Product[]>([]);
        }

        public SetActiveVariant(variantId: number) {
            if (this.product().IsMasterProduct) {
                this.selectedVariant(this.product().CompositionInformation.VariantInformation.Variants.filter((v) => v.DistinctProductVariantId == variantId)[0]);
            }
        }

        /**
         * Parse the dimension values.
         *
         * @param {Commerce.Model.Entities.Product} product - Product to be parsed for dimension
         */
        public getProductDimensions(callerContext: any, product: Commerce.Model.Entities.Product, productId: number, dimensionsChangeCallback: any): any {
            if (product.IsMasterProduct) {
                var Dimensions: Commerce.Model.Entities.ProductDimensionSet[] = product.CompositionInformation.VariantInformation.Dimensions;
                var DimensionValue: any = [];

                for (var dimensionCount = 0; dimensionCount < Dimensions.length; dimensionCount++) {
                    var dimensionValues: ObservableArray<any> = ko.observableArray([]);
                    var OptionCaption: string = "";
                    var selectedOptions: Observable<any> = ko.observable(undefined);
                    var isSelectionEnabled: Observable<boolean> = ko.observable(dimensionCount === 0);

                    // Put default selection here
                    for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                        dimensionValues.push(
                            {
                                value: {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    value: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()
                                },
                                text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()
                            });
                    }

                    OptionCaption = ProductPropertiesHelper.getDimensionTypeString(Dimensions[dimensionCount].DimensionKey.toUpperCase());

                    if (productId != product.RecordId) {
                        // Assign default dimensions
                        var componentVariantInfo: Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants.filter(
                            (variant) => {
                                return variant.DistinctProductVariantId == productId;
                            });

                        var defaultDimensionValue = dimensionValues().filter((dimension) => {
                            return componentVariantInfo[0][dimension.value.dimension] == dimension.value.value;
                        });
                        selectedOptions(defaultDimensionValue[0].value);
                    }

                    selectedOptions.subscribe(function (item) {
                        dimensionsChangeCallback.call(callerContext, item);
                    }, this);

                    DimensionValue.push({
                        Optionstext: Dimensions[dimensionCount].DimensionKey,
                        OptionCaption: ProductPropertiesHelper.getDimensionTypeString(Dimensions[dimensionCount].DimensionKey.toUpperCase()),
                        AvailableOptions: dimensionValues,
                        SelectedOption: selectedOptions,
                        IsSelectionEnabled: isSelectionEnabled,
                    });

                }
                return DimensionValue;
            };
        }

        /**
         * Parse the dimension values.
         *
         * @param {Commerce.Model.Entities.Product} product - Product to be parsed for dimension
         */
        public ParseProductDimensions(callerContext: any, product: Commerce.Model.Entities.Product, dimensionsChangeCallback: any, componentId: number, kitVariantId: number, isSubstituteProduct: boolean = false): any {
            if (product.IsMasterProduct) {
                var Dimensions: Commerce.Model.Entities.ProductDimensionSet[] = product.CompositionInformation.VariantInformation.Dimensions;
                var DimensionValue: any = [];
                var isPreviosDimensionValueEnabled: boolean = true;

                for (var dimensionCount = 0; dimensionCount < Dimensions.length; dimensionCount++) {
                    let dimensionIndex: number = dimensionCount;
                    var dimensionValues: ObservableArray<any> = ko.observableArray([]);
                    var OptionCaption: string = "";
                    var selectedOptions: Observable<any> = ko.observable(null);
                    var isSelectionEnabled: Observable<boolean> = ko.observable(isPreviosDimensionValueEnabled);

                    // Put default selection here
                    for (var i = 0; i < Dimensions[dimensionCount].DimensionValues.length; i++) {
                        dimensionValues.push(
                            {
                                value: {
                                    dimension: Dimensions[dimensionCount].DimensionKey,
                                    value: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()
                                },
                                text: Dimensions[dimensionCount].DimensionValues[i].DimensionValue.toString()
                            });
                    }

                    OptionCaption = ProductPropertiesHelper.getDimensionTypeString(Dimensions[dimensionCount].DimensionKey.toUpperCase());

                    // Assign default dimensions
                    if (!isSubstituteProduct) {
                        var defaultVariantId: number;
                        if (this.product().RecordId == kitVariantId) {
                            defaultVariantId = this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentId].ComponentProductId;
                        } else {

                            var kitVariantContent: Model.Entities.KitVariantContent[] = this.product().CompositionInformation.KitDefinition.KitVariantToComponentMap.filter((value) => { return value.KitVariantId == kitVariantId });
                            if (ArrayExtensions.hasElements(kitVariantContent)) {
                                kitVariantContent[0].KitComponentKeyList.forEach((kitComponentKey: Model.Entities.KitComponentKey) => {
                                    if (this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentId].KitLineIdentifier == kitComponentKey.KitLineIdentifier) {
                                        defaultVariantId = kitComponentKey.DistinctProductId;
                                    }
                                });
                            } else {
                                RetailLogger.viewModelProductDetailsKitVariantNotFound(kitVariantId, this.product().RecordId);
                                return [];
                            }
                        }

                        var componentVariantInfo: Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants.filter(
                            (variant) => {
                                return variant.DistinctProductVariantId === defaultVariantId;
                            });

                        var defaultDimensionValue = dimensionValues().filter((dimension) => {
                            return componentVariantInfo[0][dimension.value.dimension] === dimension.value.value;
                        });
                        selectedOptions(defaultDimensionValue[0].value);
                        isPreviosDimensionValueEnabled = !ObjectExtensions.isNullOrUndefined(defaultDimensionValue[0].value);
                    }

                    ProductDetailsViewModel.createDimensionChangeCallback(callerContext, dimensionsChangeCallback, selectedOptions, componentId, dimensionIndex);

                    DimensionValue.push({
                        Optionstext: Dimensions[dimensionCount].DimensionKey,
                        OptionCaption: ProductPropertiesHelper.getDimensionTypeString(Dimensions[dimensionCount].DimensionKey.toUpperCase()),
                        AvailableOptions: dimensionValues,
                        SelectedOption: selectedOptions,
                        IsSelectionEnabled: isSelectionEnabled,
                    });

                }
                return DimensionValue;
            };
        }
        
        /**
         * Creates callback for dimension change. 
         * @param callerContext current context
         * @param callback callback
         * @param options selected options.
         * @param id component id
         * @param index dimension index
         */
        private static createDimensionChangeCallback(callerContext: any, callback: any, options: Observable<any>, id: number, index: number): void {
            options.subscribe((item) => {
                callback.call(callerContext, item, id, index);
            });
        }

        /**
         * Get localized dimension string.
         *
         * @param {Commerce.Model.Entities.Product[]} kitComponentProduct - Kit component products
         * @param {number} kitComponentIndex - Kit component index
         * @return {Commerce.Model.Entities.Product[]} Return processed products.
         */
        private ProcessKitComponents(kitComponentProduct: Commerce.Model.Entities.Product, kitComponentIndex: number): Commerce.Model.Entities.Product {

            var product: Commerce.Model.Entities.Product = kitComponentProduct;
            if (product.IsMasterProduct) {
                var dimensions: Commerce.Model.Entities.ProductDimensionSet[] = product.CompositionInformation.VariantInformation.Dimensions;
                var substituteProductIds: number[] = [];

                substituteProductIds = substituteProductIds.concat(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[kitComponentIndex].SubstituteProductIds);

                if ($.inArray(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[kitComponentIndex].ComponentProductId, substituteProductIds) !== -1) {
                    substituteProductIds = substituteProductIds.concat(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[kitComponentIndex].ComponentProductId);
                }

                for (var i = 0; i < substituteProductIds.length; i++) {
                    if (substituteProductIds[i] == product.RecordId) {
                        return product;
                    }
                }

                for (var j = 0; j < dimensions.length; j++) {
                    var dimensionValues: Commerce.Model.Entities.ProductDimensionValueSet[] = dimensions[j].DimensionValues;
                    for (var k = 0; k < dimensionValues.length; k++) {
                        var variantSet: number[] = dimensionValues[k].VariantSet;
                        dimensionValues[k].VariantSet = $.grep(variantSet, function (element) {
                            return $.inArray(element, substituteProductIds) !== -1;
                        }, false);
                    }

                    var tempDimensionValues: Commerce.Model.Entities.ProductDimensionValueSet[] = [];
                    dimensionValues.forEach(function (dimensionValue) {
                        if (dimensionValue.VariantSet.length > 0) {
                            tempDimensionValues.push(dimensionValue);
                        }
                    });

                    dimensions[j].DimensionValues = tempDimensionValues;
                }
            }

            return product;
        }

        public getVariantId(product: Model.Entities.Product, dimension: any): number {
            var variantId: number = -1;
            var variantCollection: Commerce.Model.Entities.ProductVariant[] = [];

            if (product.IsMasterProduct && !product.IsKit) {
                variantCollection = product.CompositionInformation.VariantInformation.Variants;

                // check if all dimensions are selected
                for (var dimensionCount = 0; dimensionCount < product.CompositionInformation.VariantInformation.Dimensions.length; dimensionCount++) {
                    if (!ObjectExtensions.isNullOrUndefined(dimension[dimensionCount].SelectedOption())) {
                        var tempVariantCollection: Commerce.Model.Entities.ProductVariant[] = [];
                        variantCollection.forEach((variant) => {
                            if (variant[dimension[dimensionCount].SelectedOption().dimension] == dimension[dimensionCount].SelectedOption().value) {
                                tempVariantCollection.push(variant);
                            }
                        });

                        variantCollection = tempVariantCollection;
                    }
                    else {
                        break;
                    }
                }
                if (ArrayExtensions.hasElements(variantCollection) && variantCollection.length == 1) {
                    variantId = variantCollection[0].DistinctProductVariantId;
                }
            } else {
                variantId = product.RecordId;
            }

            return variantId;
        }

        public getComponentProperty(KitLineIdentifier: number, ProductId: number, ComponentProduct: Commerce.Model.Entities.Product): any {
            var componentProperty = { Charge: "", Quantity: "" };
            var property: Commerce.Model.Entities.KitLineProductProperty[];
            var kitLineDefinition: Commerce.Model.Entities.KitLineDefinition;
            if (!ObjectExtensions.isNullOrUndefined(KitLineIdentifier) &&
                !ObjectExtensions.isNullOrUndefined(ProductId) &&
                !ObjectExtensions.isNullOrUndefined(ComponentProduct)) {
                kitLineDefinition = this.product().CompositionInformation.KitDefinition.KitLineDefinitions.filter((line) => line.KitLineIdentifier == KitLineIdentifier)[0];
                property = kitLineDefinition.ComponentProperties.filter((property) => property.ProductId == ProductId);

                // if 'ProductId' is not the master product identifier and the property is not set at a distinct product level 
                // then search using the master product id
                if ((ObjectExtensions.isNullOrUndefined(property) || ObjectExtensions.isNullOrUndefined(property[0]))) {
                    if (ProductId != ComponentProduct.RecordId) {
                        property = kitLineDefinition.ComponentProperties.filter((property) => property.ProductId == ComponentProduct.RecordId);
                    }
                    else {
                        // ProductId is the master product identifier, but none of the substitutes are the master product
                        // Find the corresponding substitute variant id to get the substitute information
                        // If there are multiple variants, we only display the master product so we just need the information from one, the proper information will still be showed when using the dimension picker
                        var variant = ComponentProduct.CompositionInformation.VariantInformation.Variants.filter((variant) =>
                            ArrayExtensions.hasElement(kitLineDefinition.SubstituteProductIds, variant.DistinctProductVariantId));
                        var variantId = variant[0].DistinctProductVariantId;

                        property = kitLineDefinition.ComponentProperties.filter((property) => property.ProductId == variantId);
                    }
                }

                if (!ObjectExtensions.isNullOrUndefined(property) && !ObjectExtensions.isNullOrUndefined(property[0])) {
                    if (property[0].Charge == 0) {
                        componentProperty.Charge = "Included";
                    }
                    else {
                        componentProperty.Charge = NumberExtensions.formatCurrency(property[0].Charge);
                    }

                    componentProperty.Quantity = Commerce.StringExtensions.format("{0} {1}", property[0].Quantity, property[0].Unit);
                }
            }
            return componentProperty;
        }

        public IsSubstitutionApplicable(componentId: number): boolean {

            // Get all substitute component ids 
            var substituteProductIds: number[] = [];
            this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentId].SubstituteProductIds.forEach((value) => { substituteProductIds.push(value); });
            if ($.inArray(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentId].ComponentProductId, substituteProductIds) !== -1) {
                substituteProductIds.concat(this.product().CompositionInformation.KitDefinition.KitLineDefinitions[componentId].ComponentProductId);
            }

            // Get all Variant ids of the default component product
            var productVariantIds: number[] = [];
            if (this.kitComponents()[componentId].IsMasterProduct) {
                this.kitComponents()[componentId].CompositionInformation.VariantInformation.Variants.forEach((variant) => { productVariantIds.push(variant.DistinctProductVariantId); });
            }
            productVariantIds.push(this.kitComponents()[componentId].RecordId);


            var substituteComponentIds: number[] = [];
            substituteComponentIds = $.grep(substituteProductIds, function (element) {
                return $.inArray(element, productVariantIds) == -1;
            }, false);

            return (substituteComponentIds.length > 0);
        }

        /*
         * Determines the current kit variant by looking at the selected components and modifies the price of the kit to be that of the selected kit configuration price.
         */
        public setActiveKitVariant(): IVoidAsyncResult {
            var selectedComponentvariantIds: any = [];
            for (var i = 0; i < this.kitComponents().length; i++) {
                // Note: this.getVariantId returns the variant identifier if the product is a variant OR the standalone products record identifier
                // if the product is not a standard product
                var variantId = this.getVariantId(this.kitComponents()[i], this.kitComponentsForDisplay()[i].Dimensionvalue);
                if (variantId > -1) {
                    selectedComponentvariantIds.push(variantId);
                } else {
                    break;
                }
            }

            if (selectedComponentvariantIds.length == this.product().CompositionInformation.KitDefinition.KitLineDefinitions.length) {
                var kitVariantIds: number[] = ProductPropertiesHelper.getKitVariant(this.product(), selectedComponentvariantIds);
                var selectedKitVariantId = kitVariantIds[0];
                var tempVariants;
                var variantCollection = this.product().CompositionInformation.VariantInformation.Variants;
                if (!Commerce.ObjectExtensions.isNullOrUndefined(variantCollection)) {
                    tempVariants = variantCollection.filter((v) => v.DistinctProductVariantId == selectedKitVariantId);
                }

                if (Commerce.ObjectExtensions.isNullOrUndefined(tempVariants) || tempVariants.length == 0) {  // only one variant is expected
                    // get variant from server as the kit product object is being retrieved with DataLevel= 1(minimal) and does not contain all available variants
                    var MINIMALDATALEVEL: number = 1;
                    var SKIPVARIANTEXPANSION: boolean = true;

                    return this.productManager.getProductDetailsByDataLevelAsync([selectedKitVariantId], MINIMALDATALEVEL, SKIPVARIANTEXPANSION)
                        .done((products) => {
                            // add the variant to the kit product object on the view model
                            variantCollection.push(products[0].CompositionInformation.VariantInformation.Variants.filter((v) => v.DistinctProductVariantId == selectedKitVariantId)[0]);
                            this.SetActiveVariant(selectedKitVariantId);
                        });
                } else {
                    this.SetActiveVariant(selectedKitVariantId);
                }
            }

            return VoidAsyncResult.createResolved();
        }
    }
}
