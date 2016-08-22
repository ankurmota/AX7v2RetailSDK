/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../Utilities/ProductPropertiesHelper.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IProductManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class ProductManager implements IProductManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Get a page of records.
         * @param {Entities.ProductSearchCriteria} searchCriteria The criteria to use when searching.
         * @param {number} pageSize Number of records per page to return.
         * @param {number} skip Number of records to skip.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getRecordsPage(searchCriteria: Entities.ProductSearchCriteria, pageSize: number, skip: number): IAsyncResult<Entities.Product[]> {
            if (searchCriteria) {
                var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
                query.top(pageSize).skip(skip);

                return query.search(searchCriteria).execute<Entities.Product[]>();
            }

            return AsyncResult.createResolved<Entities.Product[]>([]);
        }

        /**
         * Get all categories.
         * @param {number} channelId The channel identifier.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Category[]>} The async result.
         */
        public getCategoriesAsync(channelId: number, skip?: number, take?: number): IAsyncResult<Entities.Category[]> {
            var query: Proxy.CategoriesDataServiceQuery = this._commerceContext.categories();

            if (!ObjectExtensions.isNullOrUndefined(take) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(take).skip(skip);
            }

            return query.getCategories(channelId).execute<Entities.Category[]>();
        }

        /**
         * Get refiner values for a product search criteria.
         * @param {productSearchCriteria} Product search criteria .
         * @return {Entities.ProductRefiner[]>} The async result.
         */
        public getRefinersAsync(productSearchCriteria: Entities.ProductSearchCriteria): IAsyncResult<Entities.ProductRefiner[]> {
            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            return query.getRefiners(productSearchCriteria).execute<Entities.ProductRefiner[]>();
        }

        /**
         * Get all categories under the given category identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} parentCategoryId The parent category identifier.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Category[]>} The async result.
         */
        public getChildCategoriesAsync(channelId: number, parentCategoryId: number, skip?: number, take?: number): IAsyncResult<Entities.Category[]> {
            var query: Proxy.CategoriesDataServiceQuery = this._commerceContext.categories();

            if (!ObjectExtensions.isNullOrUndefined(take) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(take).skip(skip).inlineCount();
            }

            return query.getChildren(channelId, parentCategoryId).execute<Entities.Category[]>();
        }

        /**
         * Get products in a category.
         * @param {Entities.ProjectionDomain} context The channel and catalog context.
         * @param {number} categoryId The products category identifier.
         * @param {boolean} includeDescendantCategories Set true to include descendant categories.
         * @param {number} [skip] Number of records to skip.
         * @param {number} [take] Number of records to take.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getProductsByCategoryAsync(
            context: Entities.ProjectionDomain,
            categoryId: number,
            includeDescendantCategories: boolean,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]> {
            if (ObjectExtensions.isNullOrUndefined(categoryId)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(take) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(take).skip(skip).inlineCount();
            }

            var searchCriteriaByCategoryId: Entities.ProductSearchCriteria = {
                Context: context,
                CategoryIds: [categoryId],
                DataLevelValue: 1
            };

            return query.search(searchCriteriaByCategoryId).execute<Entities.Product[]>();
        }

        /**
         * Search products by item barcode
         * @param {string} barcode The barcode to be searched.
         * @param {number} dataLevel The data level.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @param {number} [skip] Number of records to be skipped.
         * @param {number} [take] Number of records per page.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public searchProductsByBarcodeAsync(
            barcode: string,
            dataLevel: number = 4,
            skipVariantExpansion: boolean = false,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]> {
            if (StringExtensions.isNullOrWhitespace(barcode)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            var productLookupClause: Entities.ProductLookupClause = { ItemId: barcode };
            var searchProductCriteria: Entities.ProductSearchCriteria = {
                Context: Commerce.Session.instance.productCatalogStore.Context,
                ItemIds: [productLookupClause],
                Barcodes: [barcode],
                DataLevelValue: dataLevel,
                SkipVariantExpansion: skipVariantExpansion
            };

            if (take && skip) {
                query.top(take).skip(skip).inlineCount();
            }

            return query.search(searchProductCriteria).execute<Entities.Product[]>();
        }

        /**
         * Search products by item identifiers
         * @param {string[]} item identifiers to be searched.
         * @param {number} [skip] Number of records to be skipped.
         * @param {number} [take] Number of records per page.
         * @param {boolean} [downloadProductData] True, if product data should be downloaded, or false otherwise.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public searchProductsByItemIdsAsync(itemIds: string[], skip?: number, take?: number, downloadProductData?: boolean): IAsyncResult<Entities.Product[]> {
            if (!ArrayExtensions.hasElements(itemIds)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            var productLookupClauses: Entities.ProductLookupClause[] = [];
            var itemIdsDictionary: Dictionary<string> = new Dictionary<string>();
            var itemId: string;

            for (var i: number = 0; i < itemIds.length; i++) {
                itemId = itemIds[i];
                if (!itemIdsDictionary.hasItem(itemId)) {
                    itemIdsDictionary.setItem(itemId, itemId);
                    productLookupClauses.push({ ItemId: itemId });
                }
            }

            var searchProductCriteria: Entities.ProductSearchCriteria = null;
            if (!ObjectExtensions.isNullOrUndefined(downloadProductData)) {
                searchProductCriteria = {
                    Context: Commerce.Session.instance.productCatalogStore.Context,
                    ItemIds: productLookupClauses,
                    DataLevelValue: 4,
                    DownloadProductData: true
                };
            } else {
                searchProductCriteria = {
                    Context: Commerce.Session.instance.productCatalogStore.Context,
                    ItemIds: productLookupClauses,
                    DataLevelValue: 4
                };
            }

            if (take && skip) {
                query.top(take).skip(skip).inlineCount();
            }

            return query.search(searchProductCriteria).execute<Entities.Product[]>();
        }

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
        public searchProductsAsync(
            context: Entities.ProjectionDomain,
            searchString: string,
            dataLevel: number,
            skipVariantExpansion: boolean,
            skip?: number,
            take?: number): IAsyncResult<Entities.Product[]> {
            if (StringExtensions.isNullOrWhitespace(searchString)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            var searchProductCriteria: Entities.ProductSearchCriteria = {
                Context: context,
                SearchCondition: searchString,
                DataLevelValue: dataLevel,
                SkipVariantExpansion: skipVariantExpansion
            };

            if (!ObjectExtensions.isNullOrUndefined(take) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(take).skip(skip).inlineCount();
            }

            return query.search(searchProductCriteria).execute<Entities.Product[]>();
        }

        /**
         * Get products by record identifiers.
         * @param {number[]} productRecordIds The product record identifiers.
         * @param {boolean} [arrangeProducts] Specifies if the product should be arranged in the same order as the input identifiers.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getProductDetailsAsync(productRecordIds: number[], arrangeProducts?: boolean): IAsyncResult<Entities.Product[]> {
            return this.getProductDetailsByDataLevelAsync(productRecordIds, 4, false);
        }

        /**
         * Get products product search criteria.
         * @param {Entities.ProductSearchCriteria} productSearchCriteria The product search criteria.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getProductDetailsBySearchCriteriaAsync(productSearchCriteria: Entities.ProductSearchCriteria): IAsyncResult<Entities.Product[]> {
            if (ObjectExtensions.isNullOrUndefined(productSearchCriteria)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.products().search(productSearchCriteria);
            return request.execute<Entities.Product[]>();
        }

        /**
         * Get products by record identifiers taking into account supplied DataLevel..
         * @param {number[]} productRecordIds The product record identifiers.
         * @param {number} dataLevel The data level.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getProductDetailsByDataLevelAsync(
            productRecordIds: number[],
            dataLevel: number,
            skipVariantExpansion: boolean): IAsyncResult<Entities.Product[]> {
            if (!ArrayExtensions.hasElements(productRecordIds)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var searchProductCriteria: Entities.ProductSearchCriteria = {
                Context: Commerce.Session.instance.productCatalogStore.Context,
                Ids: productRecordIds,
                DataLevelValue: dataLevel,
                SkipVariantExpansion: skipVariantExpansion
            };

            var request: Common.IDataServiceRequest = this._commerceContext.products().search(searchProductCriteria);
            return request.execute<Entities.Product[]>();
        }

        /**
         * Get products by record identifiers.
         * @param {number} channelId The channel identifier.
         * @return {IAsyncResult<Entities.ProductCatalog[]>} The async result.
         */
        public getCatalogsAsync(channelId: number): IAsyncResult<Entities.ProductCatalog[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.catalogs().getCatalogs(channelId, true);
            return request.execute<Entities.ProductCatalog[]>();
        }

        /**
         * Get listings referenced by barcode.
         * @param {string[]} collection of barcodes to search for.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public getProductsByBarcodeAsync(barcodes: string[]): IAsyncResult<Entities.Product[]> {
            if (!ArrayExtensions.hasElements(barcodes)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            var searchProductCriteria: Entities.ProductSearchCriteria = {
                Context: Commerce.Session.instance.productCatalogStore.Context,
                Barcodes: barcodes,
                DataLevelValue: 4
            };

            return query.search(searchProductCriteria).execute<Entities.Product[]>();
        }

        /**
         * Get inventories available from a keyword.
         * @param {string} keyword The keyword to be searched.
         * @return {IAsyncResult<Entities.OrgUnitAvailability[]>} The async result.
         */
        public getAvailableInventoriesFromKeywordAsync(keyword: string): IAsyncResult<Entities.OrgUnitAvailability[]> {
            return this._commerceContext.orgUnits().getAvailableInventory("", "", keyword).execute<Entities.OrgUnitAvailability[]>();
        }

        /**
         * Get inventories available from a product.
         * @param {number} variantId The variant identifier if looking for inventories for variant. 
         * @param {Entities.Product} product The listing product to be found for inventories.
         * @return {IAsyncResult<Entities.Inventory>} The async result consists of inventories.
         */
        public getAvailableInventoriesByProductListingAsync(variantId: number, product: Entities.Product): IAsyncResult<Entities.OrgUnitAvailability[]> {
            var itemId: string;
            var distinctVariantId: string = "";

            variantId = variantId || 0;
            itemId = ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.ProductNumber);

            if (product.IsMasterProduct) {
                var productVariant: Entities.ProductVariant = ProductPropertiesHelper.getVariant(variantId, product);
                distinctVariantId = productVariant.VariantId.toString();
            }

            var query: Proxy.OrgUnitsDataServiceQuery = this._commerceContext.orgUnits();
            return query.getAvailableInventory(itemId, distinctVariantId, "").execute<Entities.OrgUnitAvailability[]>();
        }

        /**
         * Search product(s) by barcode, product identifier, or product name.
         * @param {string} keyword Keyword to be searched by barcode, product identifier, or product name.
         * @param {number} dataLevel The DataLevel for Product Search.
         * @param {boolean} skipVariantExpansion Specifies whether all variants for the product's variant should be returned.
         * @return {IAsyncResult<Entities.Product[]>} The async result.
         */
        public searchBarcodeOrProductsAsync(keyword: string, dataLevel: number = 4, skipVariantExpansion: boolean = false): IAsyncResult<Entities.Product[]> {
            if (StringExtensions.isNullOrWhitespace(keyword)) {
                return AsyncResult.createResolved<Entities.Product[]>([]);
            }

            var productsResult: Entities.Product[] = [];
            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // Search by barcode and item identifier first.
                    var barcodeItemIdQuery: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
                    var searchProductCriteria: Entities.ProductSearchCriteria = {
                        Context: Commerce.Session.instance.productCatalogStore.Context,
                        Barcodes: [keyword],
                        DataLevelValue: dataLevel,
                        SkipVariantExpansion: skipVariantExpansion,
                        ItemIds: [{ ItemId: keyword }]
                    };

                    return barcodeItemIdQuery.search(searchProductCriteria).execute<Entities.Product[]>()
                        .done((products: Entities.Product[]) => {
                            productsResult = products;

                            if (ArrayExtensions.hasElements(products)) {
                                asyncQueue.cancel();
                            }
                        });
                }).enqueue(() => {
                    // If no results from previous search, search by keyword.
                    return this.searchProductsAsync(Session.instance.productCatalogStore.Context, keyword, dataLevel, skipVariantExpansion)
                        .done((products: Entities.Product[]) => {
                            productsResult = products;
                        });
                });

            return asyncQueue.run().map((result: ICancelableResult): Entities.Product[] => { return productsResult; });
        }

        /**
         * Searches and returns product search results matching the specified searchText.
         * @param {string} searchText The text to be searched.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        public searchByTextAsync(
            searchText: string,
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]> {

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return AsyncResult.createResolved<Entities.ProductSearchResult[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.searchByText(channelId, catalogId, searchText).execute<Entities.ProductSearchResult[]>();
        }

        /**
         * Searches and returns product search results matching the specified category.
         * @param {number} categoryId The text to be searched.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} pageSize Number of records per page.
         * @param {number} skip Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductSearchResult[]>} The async result.
         */
        public searchByCategoryAsync(
            categoryId: number,
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.searchByCategory(channelId, catalogId, categoryId).execute<Entities.ProductSearchResult[]>();
        }

        /**
         * Gets refiners by search text. 
         * @param {string} searchText The search text.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        public getRefinersByTextAsync(searchText: string, catalogId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductRefiner[]> {

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return AsyncResult.createResolved<Entities.ProductRefiner[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getRefinersByText(catalogId, searchText).execute<Entities.ProductRefiner[]>();
        }

        /**
         * Gets refiner values by search text for the specified refiner. 
         * @param {string} searchText The search text.
         * @param {number} searchText The refiner identifier.
         * @param {number} searchText The refiner source value.
         * @param {number} catalogId The catalog id.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        public getRefinerValuesByTextAsync(
            searchText: string,
            refinerId: number,
            refinerSourceValue: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.ProductRefinerValue[]> {

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return AsyncResult.createResolved<Entities.ProductRefinerValue[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getRefinerValuesByText(catalogId, searchText, refinerId, refinerSourceValue).execute<Entities.ProductRefinerValue[]>();
        }

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
        public refineSearchByTextAsync(
            searchText: string,
            refinerValues: Entities.ProductRefinerValue[],
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]> {

            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return AsyncResult.createResolved<Entities.ProductSearchResult[]>([]);
            }

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            query.top(pageSize).skip(skip);

            return query.refineSearchByText(channelId, catalogId, searchText, refinerValues).execute<Entities.ProductSearchResult[]>();
        }

        /**
         * Gets refiners by category. 
         * @param {number} categoryId The category identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefiner[]>} The async result.
         */
        public getRefinersByCategoryAsync(categoryId: number, catalogId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductRefiner[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getRefinersByCategory(catalogId, categoryId).execute<Entities.ProductRefiner[]>();
        }

        /**
         * Gets refiner values by category for the specified refiner. 
         * @param {number} categoryId The category identifier.
         * @param {number} refinerId The refiner identifier.
         * @param {number} refinerSourceValue The refiner source value.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductRefinerValue[]>} The async result.
         */
        public getRefinerValuesByCategoryAsync(
            categoryId: number,
            refinerId: number,
            refinerSourceValue: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.ProductRefinerValue[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getRefinerValuesByCategory(catalogId, categoryId, refinerId, refinerSourceValue).execute<Entities.ProductRefinerValue[]>();
        }

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
        public refineSearchByCategoryAsync(
            categoryId: number,
            refinerValues: Entities.ProductRefinerValue[],
            channelId: number,
            catalogId: number,
            pageSize: number,
            skip: number): IAsyncResult<Entities.ProductSearchResult[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();
            query.top(pageSize).skip(skip);

            return query.refineSearchByCategory(channelId, catalogId, categoryId, refinerValues).execute<Entities.ProductSearchResult[]>();
        }

        /**
         * Gets media locations for a given product identifier. 
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.MediaLocation[]>} The async result.
         */
        public getMediaLocationsAsync(productId: number,
            channelId: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.MediaLocation[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getMediaLocations(channelId, catalogId).execute<Entities.MediaLocation[]>();
        }

        /**
         * Gets media blobs for a given product identifier. 
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.MediaBlob[]>} The async result.
         */
        public getMediaBlobsAsync(
            productId: number,
            channelId: number,
            catalogId: number,
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.MediaBlob[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getMediaBlobs(channelId, catalogId).execute<Entities.MediaBlob[]>();
        }

        /**
         * Gets product for a given product identifier. 
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @return {IAsyncResult<Entities.SimpleProduct>} The async result.
         */
        public getByIdAsync(productId: number, channelId: number): IAsyncResult<Entities.SimpleProduct> {
            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            return query.getById(channelId).execute<Entities.SimpleProduct>();
        }

        /**
         * Gets products for given product identifiers. 
         * @param {number[]} productIds The product identifiers.
         * @param {number} channelId The channel identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct>} The async result.
         */
        public getByIdsAsync(productIds: number[], channelId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.SimpleProduct[]> {
            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products();

            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getByIds(channelId, productIds).execute<Entities.SimpleProduct[]>();
        }

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
        public getDimensionValuesAsync(
            productId: number,
            channelId: number,
            dimensionType: Proxy.Entities.ProductDimensionType,
            selectedDimensions: Proxy.Entities.ProductDimension[],
            pageSize?: number,
            skip?: number): IAsyncResult<Proxy.Entities.ProductDimensionValue[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getDimensionValues(channelId, dimensionType, selectedDimensions).execute<Entities.ProductDimensionValue[]>();
        }

        /**
         * Gets the variant products for a given master product based on the specified dimension values.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {Proxy.Entities.ProductDimension[]} selectedDimensions The values of the already selected dimensions.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result containing the variant products.
         */
        public getVariantsByDimensionValuesAsync(
            productId: number,
            channelId: number,
            selectedDimensions: Proxy.Entities.ProductDimension[],
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.SimpleProduct[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getVariantsByDimensionValues(channelId, selectedDimensions).execute<Entities.SimpleProduct[]>();
        }

        /**
         * Gets the units of measure that are applicable based on the specified product identifier.
         * @param {number} productId The product identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.UnitOfMeasure[]>} The async result containing the units of measure.
         */
        public getUnitsOfMeasureAsync(productId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.UnitOfMeasure[]> {
            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getUnitsOfMeasure().execute<Entities.UnitOfMeasure[]>();
        }

        /**
         * Gets the default kit components based on the specified product identifier.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.ProductComponent[]>} The async result containing the product components.
         */
        public getDefaultComponentsAsync(productId: number, channelId: number, pageSize?: number, skip?: number): IAsyncResult<Entities.ProductComponent[]> {
            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getDefaultComponents(channelId).execute<Entities.ProductComponent[]>();
        }

        /**
         * Gets the kit variant products for a given kit master product based on the specified components slot relations.
         * @param {number} productId The product identifier.
         * @param {number} channelId The channel identifier.
         * @param {Entities.ComponentInSlotRelation[]} componentSlotRelations The component slot relations of the kit variant.
         * @param {number} [pageSize] Number of records per page.
         * @param {number} [skip] Number of records to be skipped.
         * @return {IAsyncResult<Entities.SimpleProduct[]>} The async result containing the kit variant products.
         */
        public getVariantsByComponentsInSlotsAsync(
            productId: number,
            channelId: number,
            componentSlotRelations: Entities.ComponentInSlotRelation[],
            pageSize?: number,
            skip?: number): IAsyncResult<Entities.SimpleProduct[]> {

            var query: Proxy.ProductsDataServiceQuery = this._commerceContext.products(productId);
            if (!ObjectExtensions.isNullOrUndefined(pageSize) && !ObjectExtensions.isNullOrUndefined(skip)) {
                query.top(pageSize).skip(skip);
            }

            return query.getVariantsByComponentsInSlots(channelId, componentSlotRelations).execute<Entities.SimpleProduct[]>();
        }

        /**
         * Get product active prices.
         * @param {Entities.ProjectionDomain} projectionDomain The product identifier.
         * @param {number[]} productIds The inventory dimension identifier.
         * @param {Date} activeDate The barcode identifier.
         * @param {string} customerId The customer identifier.
         * @param {Entities.AffiliationLoyaltyTier[]} affiliationLoyaltyTiers The unit of measure.
         * @return {IAsyncResult<Entities.ProductPrice>} The async result.
         */
        public getActivePricesAsync(
            projectionDomain: Entities.ProjectionDomain,
            productIds: number[],
            activeDate: Date,
            customerId: string,
            affiliationLoyaltyTiers: Entities.AffiliationLoyaltyTier[]): IAsyncResult<Entities.ProductPrice[]> {

            var query: Common.IDataServiceRequest = this._commerceContext.products().getActivePrices(
                projectionDomain, productIds, activeDate, customerId, affiliationLoyaltyTiers);
            return query.execute<Entities.ProductPrice[]>();
        }
    }
}