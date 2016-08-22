/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/ProductDimensionResult.ts'/>
///<reference path='../Extensions/ObjectExtensions.ts'/>
///<reference path='../Session.ts'/>
///<reference path='Dictionary.ts'/>

module Commerce {
    "use strict";

    export enum ProductPropertyNameEnum {
        ProductNumber,
        ProductName,
        ProductDescription,
        ProductBasePrice,
        ProductPrice,
        ProductDimensionValues,
        InventoryDimensionId,
    }

    export class ProductPropertiesHelper {

        public static getVariantFromDimensionIdValues(
            product: Model.Entities.Product,
            dimensionValues: Model.Entities.ProductDimensionResult[]): Model.Entities.ProductVariant {

            if (ObjectExtensions.isNullOrUndefined(product) || !product.IsMasterProduct || !ArrayExtensions.hasElements(dimensionValues)) {
                return null;
            }

            var variants: Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants;
            var variant: Model.Entities.ProductVariant;
            var dimValuesDictionary: Dictionary<string> = new Dictionary<string>();

            if (!ArrayExtensions.hasElements(variants)) {
                return null;
            }

            for (var i: number = 0; i < dimensionValues.length; i++) {
                if (!StringExtensions.isNullOrWhitespace(dimensionValues[i].dimensionValueId)) {
                    dimValuesDictionary.setItem(dimensionValues[i].dimensionKey, dimensionValues[i].dimensionValueId);
                }
            }

            for (var j: number = 0; j < variants.length; j++) {
                variant = variants[j];

                if (dimValuesDictionary.hasItem(Model.Entities.DimensionKeys.COLOR)) {
                    if (dimValuesDictionary.getItem(Model.Entities.DimensionKeys.COLOR) !== variant.ColorId) {
                        continue;
                    }
                }

                if (dimValuesDictionary.hasItem(Model.Entities.DimensionKeys.CONFIGURATION)) {
                    if (dimValuesDictionary.getItem(Model.Entities.DimensionKeys.CONFIGURATION) !== variant.ConfigId) {
                        continue;
                    }
                }

                if (dimValuesDictionary.hasItem(Model.Entities.DimensionKeys.SIZE)) {
                    if (dimValuesDictionary.getItem(Model.Entities.DimensionKeys.SIZE) !== variant.SizeId) {
                        continue;
                    }
                }

                if (dimValuesDictionary.hasItem(Model.Entities.DimensionKeys.STYLE)) {
                    if (dimValuesDictionary.getItem(Model.Entities.DimensionKeys.STYLE) !== variant.StyleId) {
                        continue;
                    }
                }

                return variant;
            }

            return null;
        }

        public static getVariantFromDimensionTranslationValues(
            product: Model.Entities.Product,
            dimensionValues: Model.Entities.ProductDimensionResult[]): Model.Entities.ProductVariant {

            if (ObjectExtensions.isNullOrUndefined(product) || !product.IsMasterProduct || !ArrayExtensions.hasElements(dimensionValues)) {
                return null;
            }

            var variants: Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants;
            var translationProperties: Model.Entities.ProductPropertyTranslation[];
            var variantDimensionResults: Model.Entities.ProductDimensionResult[];
            var dimValuesDictionary: Dictionary<string> = new Dictionary<string>();
            var dimensionKey: string;
            var dimensionValueTranslation: string;

            for (var count: number = 0; count < dimensionValues.length; count++) {
                if (!StringExtensions.isNullOrWhitespace(dimensionValues[count].dimensionValueTranslation)) {
                    dimValuesDictionary.setItem(dimensionValues[count].dimensionKey, dimensionValues[count].dimensionValueTranslation);
                }
            }

            for (var i: number = 0; i < variants.length; i++) {
                variantDimensionResults = ProductPropertiesHelper.getDimensionTranslationValues(variants[i], product);

                translationProperties = variants[i].PropertiesAsList;

                if (variantDimensionResults.length !== dimensionValues.length) {
                    continue;
                }

                for (var j: number = 0; j < variantDimensionResults.length; j++) {
                    dimensionKey = variantDimensionResults[j].dimensionKey;
                    dimensionValueTranslation = variantDimensionResults[j].dimensionValueTranslation;

                    if (!dimValuesDictionary.hasItem(dimensionKey) ||
                        dimValuesDictionary.getItem(dimensionKey) !== dimensionValueTranslation) {
                        break;
                    }
                }

                return variants[i];
            }

            return null;
        }

        public static getActiveVariant(product: Model.Entities.Product): Model.Entities.ProductVariant {
            if (ObjectExtensions.isNullOrUndefined(product) || ObjectExtensions.isNullOrUndefined(product.CompositionInformation) ||
                ObjectExtensions.isNullOrUndefined(product.CompositionInformation.VariantInformation) ||
                ObjectExtensions.isNullOrUndefined(product.CompositionInformation.VariantInformation.ActiveVariantProductId)) {

                return null;
            }

            var activeVariantId: number = product.CompositionInformation.VariantInformation.ActiveVariantProductId;

            return ProductPropertiesHelper.getVariant(activeVariantId, product);
        }

        public static getVariant(variantId: number, product: Model.Entities.Product): Model.Entities.ProductVariant {
            if (ObjectExtensions.isNullOrUndefined(product) || ObjectExtensions.isNullOrUndefined(variantId) || variantId <= 0 || !product.IsMasterProduct) {
                return null;
            }

            var variants: Model.Entities.ProductVariant[] = product.CompositionInformation.VariantInformation.Variants;

            for (var i: number = 0; i < variants.length; i++) {
                if (variants[i].DistinctProductVariantId === variantId) {
                    return variants[i];
                }
            }

            RetailLogger.coreHelpersProductPropertiesVariantNotFound(variantId, product.RecordId);
            return null;
        }

        /**
         * Flattens the products to listing.
         * @param {Model.Entities.Product[]} products Products list.
         * @return {Dictionary<Model.Entities.Product>} The flattened dictionary.
         */
        public static getflattenedProducts(products: Model.Entities.Product[]): Dictionary<Model.Entities.Product> {
            var flattenedProducts: Dictionary<Model.Entities.Product> = new Dictionary<Model.Entities.Product>();

            for (var i: number = 0; i < products.length; i++) {
                if (products[i].IsMasterProduct) {
                    var variants: Model.Entities.ProductVariant[] = products[i].CompositionInformation.VariantInformation.Variants;
                    for (var j: number = 0; j < variants.length; j++) {
                        flattenedProducts.setItem(variants[j].DistinctProductVariantId, products[i]);
                    }
                } else {
                    flattenedProducts.setItem(products[i].RecordId, products[i]);
                }
            }

            return flattenedProducts;
        }

        public static getProperty(variantRecordId: number, product: Model.Entities.Product, propertyName: ProductPropertyNameEnum): any {

            if (variantRecordId !== product.RecordId && variantRecordId > 0) {
                // product is variant
                return ProductPropertiesHelper.getVariantProperty(variantRecordId, product, propertyName);
            }

            var propertyValue: any = null;
            switch (propertyName) {
                case ProductPropertyNameEnum.ProductNumber:
                    propertyValue = product.ItemId;
                    break;
                case ProductPropertyNameEnum.ProductName:
                    propertyValue = product.ProductName;
                    break;
                case ProductPropertyNameEnum.ProductDescription:
                    propertyValue = product.Description;
                    break;
                case ProductPropertyNameEnum.ProductBasePrice:
                    propertyValue = product.BasePrice;
                    break;
                case ProductPropertyNameEnum.ProductPrice:
                    propertyValue = product.Price;
                    break;
                case ProductPropertyNameEnum.InventoryDimensionId:
                    propertyValue = "";
                    break;
                case ProductPropertyNameEnum.ProductDimensionValues:
                    propertyValue = [];
                    break;
                default:
                    RetailLogger.coreHelpersProductPropertiesPropertyNotExist(ProductPropertyNameEnum[propertyName]);
                    break;
            }

            return propertyValue;
        }

        public static GetUnitOfMeasures(product: Model.Entities.Product): Commerce.Model.Entities.UnitOfMeasure[] {
            var unitOfMeasures: Commerce.Model.Entities.UnitOfMeasure[] = [];
            if (product && product.UnitsOfMeasureSymbol) {
                product.UnitsOfMeasureSymbol.forEach((unitOfMeasureSymbol: string) => {
                    if (Commerce.ApplicationContext.Instance.unitsOfMeasureMap.hasItem(unitOfMeasureSymbol.toLowerCase())) {
                        unitOfMeasures.push(Commerce.ApplicationContext.Instance.unitsOfMeasureMap.getItem(unitOfMeasureSymbol.toLowerCase()));
                    } else {
                        RetailLogger.coreHelpersProductPropertiesUnitOfMeasureNotExist(unitOfMeasureSymbol);
                    }
                });
            }

            return unitOfMeasures;
        }

        public static ArrangeProducts(productIds: number[], products: Model.Entities.Product[]): Model.Entities.Product[] {
            if (ObjectExtensions.isNullOrUndefined(products)) {
                return;
            }

            var arrangedProducts: Model.Entities.Product[] = [];
            productIds.forEach((productId: number) => {
                var filteredProducts: Model.Entities.Product[] = products.filter(function (product: Model.Entities.Product): boolean {
                    if (product.RecordId === productId) {
                        return true;
                    } else if (product.IsMasterProduct) {
                        var productVariant: Model.Entities.ProductVariant[] =
                            product.CompositionInformation.VariantInformation.Variants.filter(function (variant: Model.Entities.ProductVariant): boolean {
                                return variant.DistinctProductVariantId === productId;
                            });
                        if (ArrayExtensions.hasElements(productVariant)) {
                            return true;
                        }
                    }
                });
                if (ArrayExtensions.hasElements(filteredProducts)) {
                    arrangedProducts.push(<Model.Entities.Product>$.extend(true, {}, filteredProducts[0]));
                }
            });
            return arrangedProducts;
        }

        public static ProductPropertyFormatter(product: Model.Entities.Product, propertyName: string, variantId?: number): string {

            var productPropertyValue: string = "";

            if (ObjectExtensions.isNullOrUndefined(product)) {
                RetailLogger.coreHelpersProductPropertiesInputParameterProductIsUndefined(propertyName);
            } else if (StringExtensions.isNullOrWhitespace(propertyName)) {
                RetailLogger.coreHelpersProductPropertiesInputParameterPropertyNameIsInvalid(product.ItemId, propertyName);
            } else {
                var variant: Model.Entities.ProductVariant;
                var dimensions: Model.Entities.ProductDimensionResult[];
                var dimensionValue: Model.Entities.ProductDimensionResult;
                switch (propertyName) {
                    case "VariantName":
                        if (product.IsMasterProduct) {
                            // select the variant
                            var productVariant: Model.Entities.ProductVariant[] =
                                product.CompositionInformation.VariantInformation.Variants.filter((variant: Model.Entities.ProductVariant) => {
                                    return variant.DistinctProductVariantId === variantId;
                                });
                            if (ArrayExtensions.hasElements(productVariant)) {
                                var productVariantProperties: Model.Entities.ProductProperty[] = productVariant[0].PropertiesAsList[0].TranslatedProperties;
                                var productDimensions: Model.Entities.ProductDimensionSet[] = product.CompositionInformation.VariantInformation.Dimensions;
                                var dimensionValues: string[] = [];
                                productDimensions.forEach((productDimension: Model.Entities.ProductDimensionSet): void => {
                                    productVariantProperties.forEach((productVariantProperty: Model.Entities.ProductProperty): void => {
                                        if (productVariantProperty.KeyName.toUpperCase() === productDimension.DimensionKey.toUpperCase()) {
                                            dimensionValues.push(productVariantProperty.ValueString);
                                        }
                                    });
                                });

                                if (!ArrayExtensions.hasElements(dimensionValues)) {
                                    return StringExtensions.EMPTY;
                                } else if (dimensionValues.length === 1) {
                                    return dimensionValues[0];
                                }

                                var formatString: string = ViewModelAdapter.getResourceString("string_4385"); // "{0}, {1}"
                                productPropertyValue = StringExtensions.format(formatString, dimensionValues[0], dimensionValues[1]);
                                for (var i: number = 2; i < dimensionValues.length; ++i) {
                                    productPropertyValue = StringExtensions.format(formatString, productPropertyValue, dimensionValues[i]);
                                }
                            } else {
                                RetailLogger.coreHelpersProductPropertiesProductNotHaveVariant(product.ItemId, variantId);
                            }
                        }
                        break;
                    case "SizeName":
                        if (product.IsMasterProduct) {
                            variant = ProductPropertiesHelper.getVariant(variantId, product);
                            dimensions = ProductPropertiesHelper.getDimensionTranslationValues(variant, product);
                            dimensionValue = ArrayExtensions.firstOrUndefined(dimensions,
                                (d: Model.Entities.ProductDimensionResult): boolean => {
                                    return d.dimensionKey.toUpperCase() === Model.Entities.DimensionKeys.SIZE;
                                });

                            productPropertyValue = dimensionValue ? dimensionValue.dimensionValueTranslation : productPropertyValue;
                        }
                        break;
                    case "ColorName":
                        if (product.IsMasterProduct) {
                            variant = ProductPropertiesHelper.getVariant(variantId, product);
                            dimensions = ProductPropertiesHelper.getDimensionTranslationValues(variant, product);
                            dimensionValue = ArrayExtensions.firstOrUndefined(dimensions,
                                (d: Model.Entities.ProductDimensionResult): boolean => {
                                    return d.dimensionKey.toUpperCase() === Model.Entities.DimensionKeys.COLOR;
                                });

                            productPropertyValue = dimensionValue ? dimensionValue.dimensionValueTranslation : productPropertyValue;
                        }
                        break;
                    case "StyleName":
                        if (product.IsMasterProduct) {
                            variant = ProductPropertiesHelper.getVariant(variantId, product);
                            dimensions = ProductPropertiesHelper.getDimensionTranslationValues(variant, product);
                            dimensionValue = ArrayExtensions.firstOrUndefined(dimensions,
                                (d: Model.Entities.ProductDimensionResult): boolean => {
                                    return d.dimensionKey.toUpperCase() === Model.Entities.DimensionKeys.STYLE;
                                });

                            productPropertyValue = dimensionValue ? dimensionValue.dimensionValueTranslation : productPropertyValue;
                        }
                        break;
                    case "ConfigurationName":
                        if (product.IsMasterProduct) {
                            variant = ProductPropertiesHelper.getVariant(variantId, product);
                            dimensions = ProductPropertiesHelper.getDimensionTranslationValues(variant, product);
                            dimensionValue = ArrayExtensions.firstOrUndefined(dimensions,
                                (d: Model.Entities.ProductDimensionResult): boolean => {
                                    return d.dimensionKey.toUpperCase() === Model.Entities.DimensionKeys.CONFIGURATION;
                                });

                            productPropertyValue = dimensionValue ? dimensionValue.dimensionValueTranslation : productPropertyValue;
                        }
                        break;
                    default:

                        // Check if translation for specific locale exists and all check if the specifed property is translated.
                        // Otherwise we need to fall back to the first existing langauge.
                        var productPropertyTranslations: Model.Entities.ProductPropertyTranslation[] =
                            product.ProductProperties.filter((property: Model.Entities.ProductPropertyTranslation) => {
                                return property.TranslationLanguage === Commerce.ApplicationContext.Instance.deviceConfiguration.CultureName &&
                                    property.TranslatedProperties.filter((productProperty: Model.Entities.ProductProperty): boolean => {
                                        return productProperty.KeyName === propertyName;
                                    }).length > 0;
                            });

                        if (!ArrayExtensions.hasElements(productPropertyTranslations) &&
                            ArrayExtensions.hasElements(product.ProductProperties)) {
                            productPropertyTranslations = [product.ProductProperties[0]];
                        }

                        var productProperties: Model.Entities.ProductProperty[] = [];

                        if (ArrayExtensions.hasElements(productPropertyTranslations) &&
                            ArrayExtensions.hasElements(productPropertyTranslations[0].TranslatedProperties)) {
                            productProperties =
                                productPropertyTranslations[0].TranslatedProperties.filter((property: Model.Entities.ProductProperty): boolean => {
                                    return property.KeyName === propertyName;
                                });
                        }

                        if (!ArrayExtensions.hasElements(productProperties)) {
                            RetailLogger.coreHelpersProductPropertiesProductNotHaveProperty(product.ItemId, propertyName);
                        } else {
                            productPropertyValue = productProperties[0].ValueString;
                        }
                        break;
                }
            }
            return productPropertyValue;
        }

        /**
         * Validate if a Product object is a kit.
         * @param {Model.Entities.Product} product The product object to be validated.
         * @returns {boolean} True if product is kit, false otherwise.
         */
        public static isKit(product: Model.Entities.Product): boolean {
            return !ObjectExtensions.isNullOrUndefined(product) && product.IsKit;
        }

        public static getKitVariant(product: Model.Entities.Product, componentIds: number[]): number[] {
            var kitComponentVariantSet: any = [];
            product.CompositionInformation.KitDefinition.KitLineProductToVariantMap.forEach(
                function (kitLineProductToVariant: Model.Entities.ComponentKitVariantSet): void {
                    for (var i: number = 0; i < componentIds.length; i++) {
                        if (kitLineProductToVariant.KitLineProductId === componentIds[i]) {
                            kitComponentVariantSet.push(kitLineProductToVariant.KitVariantIdList);
                            return;
                        }
                    }
            });

            if (kitComponentVariantSet.length === 0) {
                return [];
            } else if (kitComponentVariantSet.length === 1) {
                return kitComponentVariantSet[0];
            } else {
                var partialVariantList: any = kitComponentVariantSet[0];
                for (var i: number = 1; i < kitComponentVariantSet.length; i++) {
                    partialVariantList = $.grep(partialVariantList, function (element: any): boolean {
                        return $.inArray(element, kitComponentVariantSet[i]) !== -1;
                    }, false);
                }
                return partialVariantList;
            }
        }

        /**
         * Gets the localized label for the specified dimension type.
         * @param {Proxy.Entities.ProductDimensionType} dimensionType The dimension type.
         * @return {string} The localized label for the dimension type.
         */
        public static getDimensionTypeLabel(dimensionType: Proxy.Entities.ProductDimensionType): string {
            var dimensionLabel: string = StringExtensions.EMPTY;
            if (!ObjectExtensions.isNullOrUndefined(dimensionType)) {
                switch (dimensionType) {
                    case Proxy.Entities.ProductDimensionType.Color:
                        dimensionLabel = ViewModelAdapter.getResourceString("string_915");
                        break;
                    case Proxy.Entities.ProductDimensionType.Size:
                        dimensionLabel = ViewModelAdapter.getResourceString("string_917");
                        break;
                    case Proxy.Entities.ProductDimensionType.Style:
                        dimensionLabel = ViewModelAdapter.getResourceString("string_916");
                        break;
                    case Proxy.Entities.ProductDimensionType.Configuration:
                        dimensionLabel = ViewModelAdapter.getResourceString("string_918");
                        break;
                    default:
                        break;
                }
            }

            return dimensionLabel;
        }

        /**
         * Get localized dimension string.
         * @param {string} dimensionType Type of dimension.
         * @return {string} The localized dimension string.
         */
        public static getDimensionTypeString(dimensionType: string): string {
            var optionCaption: string = "";
            if (!StringExtensions.isNullOrWhitespace(dimensionType)) {
                switch (dimensionType) {
                    case Model.Entities.DimensionKeys.COLOR:
                        optionCaption = Commerce.ViewModelAdapter.getResourceString("string_915");
                        break;
                    case "SIZE":
                        optionCaption = Commerce.ViewModelAdapter.getResourceString("string_917");
                        break;
                    case "STYLE":
                        optionCaption = Commerce.ViewModelAdapter.getResourceString("string_916");
                        break;
                    case "CONFIGURATION":
                        optionCaption = Commerce.ViewModelAdapter.getResourceString("string_918");
                        break;
                    default:
                        break;
                }
            }
            return optionCaption;
        }

        /**
         * Get product active prices.
         * @param {number[]} productIds The list of product IDs which prices should be retrieved.
         * @return {IAsyncResult<Entities.ProductPrice[]>} The async result containing the list of active prices.
         */
        public static getActivePricesUsingCart(productIds: number[]): IAsyncResult<Model.Entities.ProductPrice[]> {
            if (!ArrayExtensions.hasElements(productIds)) {
                return AsyncResult.createResolved([]);
            }

            var productManager: Model.Managers.IProductManager =
                Model.Managers.Factory.getManager<Model.Managers.IProductManager>(Model.Managers.IProductManagerName);

            var projectionDomain: Model.Entities.ProjectionDomain = Commerce.Session.instance.productCatalogStore.Context;
            var activeDate: Date = DateExtensions.now;
            var customerId: string;
            var affiliationLoyaltyTiers: Model.Entities.AffiliationLoyaltyTier[];

            if (Session.instance.isCartInProgress) {
                customerId = Session.instance.cart.CustomerId;
                affiliationLoyaltyTiers = Session.instance.cart.AffiliationLines;
            }

            return productManager.getActivePricesAsync(projectionDomain, productIds, activeDate, customerId, affiliationLoyaltyTiers);
        }

        /**
         * Callback method from the grid to retrieve active prices and update on the product search result.
         * @param {Entities.ProductSearchResult[]} gridData The whole grid data.
         * @return {IAsyncResult<Proxy.Entities.ProductSearchResult[]>} The async result containing the updated data.
         */
        public static getActivePricesAndUpdateProductSearchResults(gridData: Proxy.Entities.ProductSearchResult[])
            : IAsyncResult<Proxy.Entities.ProductSearchResult[]> {
            var newProductIds: number[] = gridData.map((value: Proxy.Entities.ProductSearchResult) => { return value.RecordId; });

            return ProductPropertiesHelper.getActivePricesUsingCart(newProductIds)
                .done((productPrices: Proxy.Entities.ProductPrice[]): void => {
                    ProductPropertiesHelper.updatePricesOnProductSearchResults(gridData, productPrices);
                }).map(() => {
                    return gridData;
                });
        }

        /**
         * Updates the prices in a list of products.
         *
         * @param {Proxy.Entities.Product[]} productsToUpdate The list of products that will have the price updated.
         * @param {Proxy.Entities.ProductPrice[]} productPrices The list of prices.
         */
        public static updatePricesOnProducts(
            productsToUpdate: Proxy.Entities.Product[],
            productPrices: Proxy.Entities.ProductPrice[]): void {

            // Constructs the map (ItemId, ProductPrice)
            var productPricesByItemId: { [itemId: string]: Proxy.Entities.ProductPrice } = Object.create(null);
            productPrices.forEach((productPrice: Proxy.Entities.ProductPrice): void => {
                productPricesByItemId[productPrice.ItemId] = productPrice;
            });

            productsToUpdate.forEach((productToUpdate: Proxy.Entities.Product) => {
                // Finds the corresponding price and update it
                var matchedPrice: Proxy.Entities.ProductPrice = productPricesByItemId[productToUpdate.ItemId];
                if (!ObjectExtensions.isNullOrUndefined(matchedPrice)) {
                    productToUpdate.Price = matchedPrice.AdjustedPrice;
                }
            });
        }

        /**
         * Updates the prices in a list of products from a search.
         *
         * @param {Proxy.Entities.ProductSearchResult[]} productsToUpdate The list of products that will have the price updated.
         * @param {Model.Entities.ProductPrice[]} productPrices The list of prices.
         */
        private static updatePricesOnProductSearchResults(
            productsToUpdate: Proxy.Entities.ProductSearchResult[],
            productPrices: Proxy.Entities.ProductPrice[]): void {

            // Constructs the map (ItemId, ProductPrice)
            var productPricesByItemId: { [itemId: string]: Proxy.Entities.ProductPrice } = Object.create(null);
            productPrices.forEach((productPrice: Proxy.Entities.ProductPrice): void => {
                productPricesByItemId[productPrice.ItemId] = productPrice;
            });

            productsToUpdate.forEach((productToUpdate: Proxy.Entities.ProductSearchResult) => {
                // Finds the corresponding price and update it
                var matchedPrice: Proxy.Entities.ProductPrice = productPricesByItemId[productToUpdate.ItemId];
                if (!ObjectExtensions.isNullOrUndefined(matchedPrice)) {
                    productToUpdate.Price = matchedPrice.AdjustedPrice;
                }
            });
        }

        private static getDimensionKeys(product: Model.Entities.Product): string[] {
            var dimensionKeys: string[] = [];

            if (ObjectExtensions.isNullOrUndefined(product.CompositionInformation) ||
                ObjectExtensions.isNullOrUndefined(product.CompositionInformation.VariantInformation)) {

                return dimensionKeys;
            }

            var dimensionSet: Model.Entities.ProductDimensionSet[] = product.CompositionInformation.VariantInformation.Dimensions;
            for (var i: number = 0; i < dimensionSet.length; i++) {
                dimensionKeys.push(dimensionSet[i].DimensionKey);
            }

            return dimensionKeys;
        }

        private static getDimensionTranslationValues(
            variant: Model.Entities.ProductVariant,
            product: Model.Entities.Product): Model.Entities.ProductDimensionResult[] {

            var dimensionValues: Model.Entities.ProductDimensionResult[] = [];
            var dimensionKeys: string[] = ProductPropertiesHelper.getDimensionKeys(product);
            var variantTranslationProperties: Model.Entities.ProductPropertyTranslation[] = variant.PropertiesAsList;

            var dimValue: string;
            for (var i: number = 0; i < dimensionKeys.length; i++) {
                dimValue = ProductPropertiesHelper.getTranslationValue(variantTranslationProperties, dimensionKeys[i]);
                dimensionValues.push({ dimensionKey: dimensionKeys[i], dimensionValueTranslation: dimValue });
            }

            return dimensionValues;
        }

        private static getVariantProperty(variantRecordId: number, product: Model.Entities.Product, propertyName: ProductPropertyNameEnum): any {
            var propertyValue: any = null;
            var variant: Model.Entities.ProductVariant = ProductPropertiesHelper.getVariant(variantRecordId, product);

            if (ObjectExtensions.isNullOrUndefined(variant)) {
                return null;
            }

            var variantTranslationProperties: Model.Entities.ProductPropertyTranslation[] = variant.PropertiesAsList;

            switch (propertyName) {
                case ProductPropertyNameEnum.ProductNumber:
                    propertyValue = ProductPropertiesHelper.getProperty(product.RecordId, product, ProductPropertyNameEnum.ProductNumber);
                    break;
                case ProductPropertyNameEnum.ProductName:
                    propertyValue = ProductPropertiesHelper.getTranslationValue(variantTranslationProperties, "ProductName");
                    break;
                case ProductPropertyNameEnum.ProductDescription:
                    propertyValue = ProductPropertiesHelper.getTranslationValue(variantTranslationProperties, "Description");
                    break;
                case ProductPropertyNameEnum.InventoryDimensionId:
                    propertyValue = variant.InventoryDimensionId;
                    break;
                case ProductPropertyNameEnum.ProductBasePrice:
                    propertyValue = variant.BasePrice;
                    break;
                case ProductPropertyNameEnum.ProductPrice:
                    propertyValue = variant.Price;
                    break;
                case ProductPropertyNameEnum.ProductDimensionValues:
                    propertyValue = ProductPropertiesHelper.getDimensionTranslationValues(variant, product);
                    break;
                default:
                    RetailLogger.coreHelpersProductPropertiesVariantPropertyNotExist(ProductPropertyNameEnum[propertyName]);
                    propertyValue = null;
            }

            return propertyValue;
        }

        private static getTranslationValue(translationProperties: Model.Entities.ProductPropertyTranslation[], keyName: string): string {
            RetailLogger.coreHelpersProductPropertiesGetTranslation(keyName);
            var productProperties: Model.Entities.ProductProperty[] = translationProperties[0].TranslatedProperties;
            for (var i: number = 0; i < productProperties.length; i++) {
                if (productProperties[i].KeyName === keyName) {
                    return productProperties[i].ValueString;
                }
            }

            RetailLogger.coreHelpersProductPropertiesTranslationPropertyNotFound(keyName);
            return "";
        }
    }
}
