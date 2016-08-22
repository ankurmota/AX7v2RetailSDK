/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Handler class to get products.
        /// </summary>
        internal sealed class GetProductsHelper
        {
            private GetProductServiceRequest request;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetProductsHelper"/> class.
            /// </summary>
            /// <param name="request">The request for getting products.</param>
            public GetProductsHelper(GetProductServiceRequest request)
            {
                this.request = request;
                this.DataStoreManagerInstance = DataStoreManager.InstantiateDataStoreManager(request.RequestContext);
                this.DataStoreManagerInstance.RegisterDataStoreAccessor(DataStoreType.L2Cache, ProductL2CacheDataStoreAccessor.Instantiate, request.RequestContext);
            }
    
            /// <summary>
            /// Gets the data store manager instance.
            /// </summary>
            /// <value>
            /// The data store manager instance.
            /// </value>
            public DataStoreManager DataStoreManagerInstance
            {
                get;
                private set;
            }
    
            /// <summary>
            /// Get products based on the <see cref="request" />.
            /// </summary>
            /// <returns>
            /// The <see cref="ProductSearchResultContainer" /> containing the requested products.
            /// </returns>
            public ProductSearchResultContainer GetProducts()
            {
                ProductL2CacheDataStoreAccessor level2CacheDataAccessor = (ProductL2CacheDataStoreAccessor)this.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    
                List<long> missed = new List<long>(0);
    
                ProductSearchResultContainer result = new ProductSearchResultContainer();
                bool useSearchById = !this.request.Criteria.Ids.IsNullOrEmpty();
                ProductSearchCriteria criteria = this.request.Criteria;
    
                PagingInfo originalPagingInfo = this.request.QueryResultSettings.Paging;
    
                if (useSearchById)
                {
                    // Do not read from cache if SkipVariantExpansion is true
                    // This is because entry point for the cache item is a Product and not variant,
                    // therefore we would not be able to distinguish between cases when I read product by master or its variants
                    // (where all othere parameters to the search are equal) where SkipVariants is set to true.
                    if (!criteria.SkipVariantExpansion)
                    {
                        result = level2CacheDataAccessor.GetProductsByIds(criteria.Ids, criteria, out missed);
    
                        // we have partial results.
                        if (missed.Count > 0)
                        {
                            // modify the criteria to include only the missed ids.
                            criteria = this.request.Criteria.Clone();
                            criteria.Ids = missed;
    
                            // we need to return all missed from the db since the pagination was already done on the product ids above.
                            this.request.QueryResultSettings.Paging = PagingInfo.AllRecords;
                        }
                        else
                        {
                            // just return the result; all the products were found in the cache.
                            return result;
                        }
                    }
                }
    
                // regardless if the search was done
                ProductSearchResultContainer nonCachedResults = this.GetProductsFromDbStorage(criteria);
    
                if (nonCachedResults.Results != null)
                {
                    // Do not store in cache if SkipVariantExpansion is true
                    // This is because entry point for the cache item is a Product and not variant,
                    // therefore we would not be able to distinguish between cases when I read product by master or its variants
                    // (where all othere parameters to the search are equal) where SkipVariants is set to true. In this case
                    // each next entry will overwrite previous.
                    if (!criteria.SkipVariantExpansion)
                    {
                        level2CacheDataAccessor.PutProducts(nonCachedResults, criteria);
                    }
    
                    List<Product> fullList = new List<Product>(0);
                    if (result.Results != null)
                    {
                        fullList.AddRange(result.Results);
                    }
    
                    fullList.AddRange(nonCachedResults.Results);
                    result = new ProductSearchResultContainer(fullList.AsReadOnly());
                }
    
                if (useSearchById)
                {
                    this.request.QueryResultSettings.Paging = originalPagingInfo;
                }
    
                return result;
            }
    
            /// <summary>
            /// Get products based on the <see cref="request" />.
            /// </summary>
            /// <param name="criteria">The criteria.</param>
            /// <returns>
            /// The <see cref="ProductSearchResultContainer" /> containing the requested products.
            /// </returns>
            private ProductSearchResultContainer GetProductsFromDbStorage(ProductSearchCriteria criteria)
            {
                GetProductPartsDataResponse productPartsResponse = this.GetProductParts(criteria);
                IEnumerable<ProductIdentity> productIdentities = productPartsResponse.ProductIdentities;
    
                ReadOnlyCollection<UnitOfMeasureConversion> unitOfMeasureOptionsDataSet;
                ReadOnlyCollection<KitDefinition> kitDefinitions;
                ReadOnlyCollection<KitComponent> kitComponentAndSubstituteList;
                ReadOnlyCollection<KitConfigToComponentAssociation> kitConfigToComponentAssociations;
                ReadOnlyCollection<KitComponent> parentKitsComponentInfo;
    
                if (criteria.DataLevel >= CommerceEntityDataLevel.Extended)
                {
                    IList<long> masterIds = productIdentities.Where(i => i.IsMasterProduct).Select(t => t.LookupId).ToList();
                    IList<long> kitMasterIds = productIdentities.Where(i => i.IsKitProduct).Select(t => t.LookupId).ToList();
                    IList<long> allIds = productIdentities.Select(i => i.RecordId).ToList();
    
                    // unit of measure
                    unitOfMeasureOptionsDataSet = this.GetUnitOfMeasureOptions(allIds);
    
                    // kits
                    this.GetKits(
                        masterIds,
                        kitMasterIds,
                        allIds,
                        out kitDefinitions,
                        out kitComponentAndSubstituteList,
                        out kitConfigToComponentAssociations,
                        out parentKitsComponentInfo);
                }
                else
                {
                    // lower data levels don't contain this information
                    unitOfMeasureOptionsDataSet = new ReadOnlyCollection<UnitOfMeasureConversion>(new List<UnitOfMeasureConversion>());
                    kitDefinitions = new ReadOnlyCollection<KitDefinition>(new List<KitDefinition>());
                    kitComponentAndSubstituteList = new ReadOnlyCollection<KitComponent>(new List<KitComponent>());
                    kitConfigToComponentAssociations = new ReadOnlyCollection<KitConfigToComponentAssociation>(new List<KitConfigToComponentAssociation>());
                    parentKitsComponentInfo = new ReadOnlyCollection<KitComponent>(new List<KitComponent>());
                }
    
                return this.AssembleProducts(
                    productPartsResponse,
                    unitOfMeasureOptionsDataSet,
                    kitDefinitions,
                    kitComponentAndSubstituteList,
                    kitConfigToComponentAssociations,
                    parentKitsComponentInfo);
            }
    
            private ProductSearchResultContainer AssembleProducts(
                GetProductPartsDataResponse productPartsResponse,
                ReadOnlyCollection<UnitOfMeasureConversion> unitOfMeasureOptionsDataSet,
                ReadOnlyCollection<KitDefinition> kitDefinitions,
                ReadOnlyCollection<KitComponent> kitComponentAndSubstituteList,
                ReadOnlyCollection<KitConfigToComponentAssociation> kitConfigToComponentAssociations,
                ReadOnlyCollection<KitComponent> parentKitsComponentInfo)
            {
                var productSearchResult = new ProductSearchResultContainer();
    
                var productParts = new Tuple<ReadOnlyCollection<ProductIdentity>, ReadOnlyCollection<ProductVariant>, ReadOnlyCollection<ProductRules>, ReadOnlyCollection<ProductAttributeSchemaEntry>, ReadOnlyCollection<ProductProperty>, ReadOnlyCollection<ProductCatalog>, ReadOnlyCollection<ProductCategoryAssociation>, Tuple<ReadOnlyCollection<RelatedProduct>>>(
                        productPartsResponse.ProductIdentities,
                        productPartsResponse.ProductVariants,
                        productPartsResponse.ProductRules,
                        productPartsResponse.ProductAttributeSchemaEntries,
                        productPartsResponse.ProductProperties,
                        productPartsResponse.ProductCatalogs,
                        productPartsResponse.CategoryAssociations,
                        new Tuple<ReadOnlyCollection<RelatedProduct>>(productPartsResponse.RelatedProducts));
    
                ProductBuilder.AssembleProductsFromDataSets(
                   this.request.Criteria.Context,
                   this.request.Criteria.DataLevel,
                   productParts,
                   productPartsResponse.LinkedProducts,
                   kitDefinitions,
                   kitComponentAndSubstituteList,
                   kitConfigToComponentAssociations,
                   parentKitsComponentInfo,
                   unitOfMeasureOptionsDataSet,
                   productSearchResult,
                   this.request.RequestContext.GetChannelConfiguration().ProductDefaultImageTemplate);
    
                return productSearchResult;
            }
    
            private void GetKits(
                IList<long> masterIds,
                IList<long> kitMasterIds,
                IList<long> productIdsForKitComponentLookup,
                out ReadOnlyCollection<KitDefinition> kitDefinitions,
                out ReadOnlyCollection<KitComponent> kitComponentAndSubstituteList,
                out ReadOnlyCollection<KitConfigToComponentAssociation> kitConfigToComponentAssociations,
                out ReadOnlyCollection<KitComponent> parentKitsComponentInfo)
            {
                // kits
                var getKitsRequest = new GetProductKitDataRequest(kitMasterIds);
                var getKitsResponse = this.request.RequestContext.Execute<GetProductKitDataResponse>(getKitsRequest);
    
                // handle kit response
                kitDefinitions = getKitsResponse.KitDefinitions;
                kitComponentAndSubstituteList = getKitsResponse.KitComponentAndSubstituteList;
                kitConfigToComponentAssociations = getKitsResponse.KitConfigToComponentAssociations;
    
                // depending on data level, we need to retrieve more information
                if (this.request.Criteria.DataLevel >= CommerceEntityDataLevel.Complete)
                {
                    var getKitParentRequest = new GetParentKitDataRequest(productIdsForKitComponentLookup, masterIds);
                    getKitParentRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    var kitParentResponse = this.request.RequestContext.Execute<GetParentKitDataResponse>(getKitParentRequest);
                    parentKitsComponentInfo = kitParentResponse.KitComponents.Results;
                }
                else
                {
                    parentKitsComponentInfo = new ReadOnlyCollection<KitComponent>(new List<KitComponent>());
                }
            }
    
            private GetProductPartsDataResponse GetProductParts(ProductSearchCriteria criteria)
            {
                // Get product parts that composes products related to this request
                GetProductPartsDataRequest getProductPartsRequest = new GetProductPartsDataRequest(
                    criteria,
                    this.request.LanguageId,
                    this.request.FetchProductsAvailableInFuture,
                    this.request.QueryResultSettings,
                    this.request.RetrieveDefaultImage);
    
                return this.request.RequestContext.Execute<GetProductPartsDataResponse>(getProductPartsRequest);
            }
    
            private ReadOnlyCollection<UnitOfMeasureConversion> GetUnitOfMeasureOptions(IEnumerable<long> productIds)
            {
                // Pagination happens on the productIds collection, caller expects to get all unit of measure options for all products in the given collection
                var unitOfMeasureOptionsRequest = new GetProductUnitOfMeasureOptionsDataRequest(productIds, QueryResultSettings.AllRecords);
                var unitOfMeasureOptionsResponse = this.request.RequestContext.Execute<GetProductUnitOfMeasureOptionsDataResponse>(unitOfMeasureOptionsRequest);
    
                // handle unit of measure response
                return unitOfMeasureOptionsResponse.UnitOfMeasureConversions.Results;
            }
        }
    }
}
