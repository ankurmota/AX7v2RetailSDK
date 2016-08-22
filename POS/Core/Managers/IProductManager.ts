/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../Entities/Product.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IProductManagerName: string = "IProductManager";
    

    export interface IProductManager {
        /**
         * Get a page of records.
         * @param {Entities.ProductSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getRecordsPage(searchCriteria: Entities.ProductSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.Product[]>;

        /**
         * Get all categories.
         * @param {number} channelId The channel identifier.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Category[]>} The async result.
         */
        getCategoriesAsync(channelId: number, skip?: number, take?: number): IAsyncResult<Entities.Category[]>;

        /**
         * Get all categories under the given category identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} parentCategoryId The parent category identifier.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Category[]>} The async result.
         */
        getChildCategoriesAsync(channelId: number, parentCategoryId: number, skip?: number, take?: number): IAsyncResult<Entities.Category[]>;

        /**
         * Get refiner values for a product search criteria.
         * @param {productSearchCriteria} Product search criteria .
         * @return {Entities.ProductRefiner[]>} The async result.
         */
        getRefinersAsync(productSearchCriteria: Entities.ProductSearchCriteria): IAsyncResult<Entities.ProductRefiner[]>;

        /**
         * Get products by record identifiers.
         * @param {number[]} productRecordIds The product record identifiers.
         * @param {boolean} [arrangeProducts] Specifies if the product should be arranged in the same order as the input identifiers.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getProductDetailsAsync(productRecordIds: number[], arrangeProducts?: boolean): IAsyncResult<Entities.Product[]>;

        /**
         * Get products product search criteria.
         * @param {Entities.ProductSearchCriteria} productSearchCriteria The product search criteria.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getProductDetailsBySearchCriteriaAsync(productSearchCriteria: Entities.ProductSearchCriteria): IAsyncResult<Entities.Product[]>;

        /**
         * Get products by record identifiers taking into account supplied DataLevel..
         * @param {number[]} productRecordIds The product record identifiers.
         * @param {number} dataLevel The data level.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getProductDetailsByDataLevelAsync(productRecordIds: number[], dataLevel: number, skipVariantExpansion: boolean): IAsyncResult<Entities.Product[]>;

        /**
         * Get products by record identifiers.
         * @param {number} channelId The channel identifier.
         * @return {IAsyncResult<Entities.ProductCatalog[]>} The async result.
         */
        getCatalogsAsync(channelId: number): IAsyncResult<Entities.ProductCatalog[]>;

        /**
         * Get products in a category.
         * @param {Entities.ProjectionDomain} context The channel and catalog context.
         * @param {number} categoryId The products category identifier.
         * @param {boolean} includeDescendantCategories Set true to include descendant categories.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getProductsByCategoryAsync(
            context: Entities.ProjectionDomain,
            categoryId: number,
            includeDescendantCategories: boolean,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]>;

        /**
         * Search and get all products matching the given string.
         * @param {Entities.ProjectionDomain} context The channel and catalog context.
         * @param {string} searchString The string to be searched.
         * @param {number} dataLevel The data level.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @param {number} [skip] Number of records to be skipped.
         * @param {number} [take] Number of records per page.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        searchProductsAsync(
            context: Entities.ProjectionDomain,
            searchString: string,
            dataLevel: number,
            skipVariantExpansion: boolean,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]>;

        /**
         * Search products by item identifiers
         * @param {string[]} item identifiers to be searched.
         * @param {number} [skip] Number of records to be skipped.
         * @param {number} [take] Number of records per page.
         * @param {boolean} [downloadProductData] True, if product data should be downloaded, or false otherwise.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        searchProductsByItemIdsAsync(itemIds: string[], skip?: number, take?: number, downloadProductData?: boolean): IAsyncResult<Entities.Product[]>;

        /**
         * Search products by item barcode
         * @param {string} barcode The barcode to be searched.
         * @param {number} dataLevel The data level.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @param {number} [skip] Number of records to be skipped.
         * @param {number} [take] Number of records per page.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        searchProductsByBarcodeAsync(
            barcode: string,
            dataLevel: number,
            skipVariantExpansion: boolean,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]>;

        /**
         * Get listings referenced by barcode.
         * @param {string[]} collection of barcodes to search for.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        getProductsByBarcodeAsync(barcodes: string[]): IAsyncResult<Entities.Product[]>;

        /**
         * Get inventories available from a keyword.
         * @param {string} keyword The keyword to be searched.
         * @return {IAsyncResult<Entities.OrgUnitAvailability[]>} The async result.
         */
        getAvailableInventoriesFromKeywordAsync(keyword: string): IAsyncResult<Entities.OrgUnitAvailability[]>;

        /**
         * Get inventories available from a product.
         * @param {number} variantId The variant identifier if looking for inventories for variant. 
         * @param {Entities.Product} product The listing product to be found for inventories.
         * @return {IAsyncResult<Entities.Inventory>} The async result consists of inventories.
         */
        getAvailableInventoriesByProductListingAsync(variantId: number, product: Entities.Product): IAsyncResult<Entities.OrgUnitAvailability[]>;

        /**
         * Search product(s) by barcode, product identifier, or product name.
         * @param {string} keyword Keyword to be searched by barcode, product identifier, or product name.
         * @param {number} dataLevel The DataLevel for Product Search.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        searchBarcodeOrProductsAsync(keyword: string, dataLevel: number, skipVariantExpansion: boolean): IAsyncResult<Entities.Product[]>;

        /**
         * Searches and returns product search results matching the specified searchText.
         * @param {string} searchText The string to be searched.
         * @param {number} channelId The string to be searched.
         * @param {number} catalogId The string to be searched.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        searchByTextAsync(
            searchText: string,
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]>;

        /**
         * Searches and returns product search results matching the specified category.
         * @param {number} categoryId The text to be searched.
         * @param {number} channelId The channel id.
         * @param {number} catalogId The catalog id.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        searchByCategoryAsync(
            categoryId: number,
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]>;

        /**
         * Gets refiners by search text. 
         * @param {string} searchText The search text.
         * @param {number} catalogId The catalog id.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        getRefinersByTextAsync(searchText: string, catalogId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductRefiner[]>;

        /**
         * Gets refiner values by search text for the specified refiner. 
         * @param {string} searchText The search text.
         * @param {number} refinerId The refiner identifier.
         * @param {number} refinerSourceValue The refiner source value.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        getRefinerValuesByTextAsync(
            searchText: string,
            refinerId: number,
            refinerSourceValue: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.ProductRefinerValue[]>;

        /**
         * Refines product search by search text using the specified refiner values.
         * @param {string} searchText The text to be searched.
         * @param {Entities.ProductRefinerValue[]} refinerValues The refiner values.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        refineSearchByTextAsync(
            searchText: string,
            refinerValues: Entities.ProductRefinerValue[],
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]>;

        /**
         * Gets refiners by category. 
         * @param {number} categoryId The category identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        getRefinersByCategoryAsync(categoryId: number, catalogId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductRefiner[]>;

        /**
         * Gets refiner values by category for the specified refiner. 
         * @param {number} searchText The category identifier.
         * @param {number} searchText The refiner identifier.
         * @param {number} searchText The refiner source value.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        getRefinerValuesByCategoryAsync(
            categoryId: number,
            refinerId: number,
            refinerSourceValue: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.ProductRefinerValue[]>;

        /**
         * Refines product search by category using the specified refiner values.
         * @param {number} categoryId The category identifier.
         * @param {Entities.ProductRefinerValue[]} refinerValues The refiner values.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        refineSearchByCategoryAsync(
            categoryId: number,
            refinerValues: Entities.ProductRefinerValue[],
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]>;

        /**
         * Gets media locations for a given product identifier.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.MediaLocation[]>} The async result.
         */
        getMediaLocationsAsync(productId: number,
            channelId: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.MediaLocation[]>;

        /**
         * Gets media blobs for a given product identifier. 
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.MediaBlob[]>} The async result.
         */
        getMediaBlobsAsync(productId: number, channelId: number, catalogId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.MediaBlob[]>;

        /**
         * Gets product for a given product identifier. 
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @return {IAsyncResult<Entities.SimpleProduct>} The async result.
         */
        getByIdAsync(productId: number, channelId: number): IAsyncResult<Entities.SimpleProduct>;

        /**
         * Gets products for given product identifiers. 
         * @param {number[]} productIds The product identifiers.
         * @param {number} channelId The channel identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct>} The async result.
         */
        getByIdsAsync(productIds: number[], channelId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.SimpleProduct[]>;

        /**
         * Gets the dimension values for a given product dimension based on the previously selected dimension values, if any.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {Proxy.Entities.ProductDimensionType} dimensionType The dimension type for which the values should be retrieved.
         * @param {Proxy.Entities.ProductDimension[]} selectedDimensions The values of the already selected dimensions.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductDimensionValue[]>} The async result containing the available dimension values.
         */
        getDimensionValuesAsync(
            productId: number,
            channelId: number,
            dimensionType: Proxy.Entities.ProductDimensionType,
            selectedDimensions: Proxy.Entities.ProductDimension[],
            pageSize?: number,
            skip?: number): IAsyncResult<Proxy.Entities.ProductDimensionValue[]>;


        /**
         * Gets the variant products for a given master product based on the specified dimension values.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {Proxy.Entities.ProductDimension[]} selectedDimensions The values of the already selected dimensions.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result containing the variant products.
         */
        getVariantsByDimensionValuesAsync(
            productId: number,
            channelId: number,
            selectedDimensions: Proxy.Entities.ProductDimension[],
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.SimpleProduct[]>;

        /**
         * Gets the units of measure that are applicable based on the specified product identifier.
         * @param {number} productId The product identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.UnitOfMeasure[]>} The async result containing the units of measure.
         */
        getUnitsOfMeasureAsync(productId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.UnitOfMeasure[]>;

        /**
         * Gets the default kit components based on the specified product identifier.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductComponent[]>} The async result containing the product components.
         */
        getDefaultComponentsAsync(productId: number, channelId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductComponent[]>;

        /**
         * Gets the kit variant products for a given kit master product based on the specified components slot relations.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {Entities.ComponentInSlotRelation[]} componentSlotRelations The component slot relations of the kit variant.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result containing the kit variant products.
         */
        getVariantsByComponentsInSlotsAsync(
            productId: number,
            channelId: number,
            componentSlotRelations: Entities.ComponentInSlotRelation[],
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.SimpleProduct[]>;

        /**
         * Get product active prices.
         * @param {Entities.ProjectionDomain} projectionDomain The projection domain.
         * @param {string[]} productIds The list of product identifier.
         * @param {Date} activeDate The active date.
         * @param {string} customerId The customer identifier.
         * @param {Entities.AffiliationLoyaltyTier[]} affiliationLoyaltyTiers The list of affiliation loyalty tiers.
         * @return {IAsyncResult<Entities.ProductPrice>} The async result.
         */
        getActivePricesAsync(
            projectionDomain: Entities.ProjectionDomain,
            productIds: number[],
            activeDate: Date,
            customerId: string,
            affiliationLoyaltyTiers: Entities.AffiliationLoyaltyTier[]): IAsyncResult<Entities.ProductPrice[]>;

    }
}