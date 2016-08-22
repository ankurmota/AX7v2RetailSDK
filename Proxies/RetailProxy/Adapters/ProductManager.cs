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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class ProductManager : IProductManager
        {
            public Task<Product> Create(Product entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<Product> Read(long recordId)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<Product>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetProducts(queryResultSettings));
            }
    
            public Task<Product> Update(Product entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(Product entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<MediaLocation>> GetMediaLocations(long recordId, long channelId, long catalogId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetMediaLocations(channelId, catalogId, recordId, null, queryResultSettings));
            }
    
            public Task<PagedResult<MediaBlob>> GetMediaBlobs(long recordId, long channelId, long catalogId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetMediaBlobs(channelId, catalogId, recordId, null, queryResultSettings));
            }
    
            public Task<PagedResult<Product>> Search(ProductSearchCriteria productSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).SearchProducts(productSearchCriteria, queryResultSettings).Results.ToCRTPagedResult());
            }
    
            public Task<PagedResult<ProductSearchResult>> SearchByCategory(long channelId, long catalogId, long categoryId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).SearchByCategory(channelId, catalogId, categoryId, null, queryResultSettings));
            }
    
            public Task<PagedResult<ProductSearchResult>> SearchByText(long channelId, long catalogId, string searchText, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).SearchByText(channelId, catalogId, searchText, null, queryResultSettings));
            }
    
            public Task<PagedResult<ProductRefiner>> GetRefinersByCategory(long catalogId, long categoryId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRefinersByCategory(catalogId, categoryId, null, queryResultSettings));
            }
    
            public Task<PagedResult<ProductRefiner>> GetRefinersByText(long catalogId, string searchText, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRefinersByText(catalogId, searchText, null, queryResultSettings));
            }
    
            public Task<PagedResult<ProductRefinerValue>> GetRefinerValuesByCategory(long catalogId, long categoryId, long refinerId, int refinerSourceValue, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRefinerValuesByCategory(catalogId, categoryId, null, refinerId, (ProductRefinerSource)refinerSourceValue, queryResultSettings));
            }
    
            public Task<PagedResult<ProductRefinerValue>> GetRefinerValuesByText(long catalogId, string searchText, long refinerId, int refinerSourceValue, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRefinerValuesByText(catalogId, searchText, null, refinerId, (ProductRefinerSource)refinerSourceValue, queryResultSettings));
            }

            public Task<PagedResult<ProductSearchResult>> RefineSearchByCategory(long channelId, long catalogId, long categoryId, IEnumerable<ProductRefinerValue> refinementCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).RefineSearchByCategory(channelId, catalogId, categoryId, refinementCriteria, queryResultSettings));
            }

            public Task<PagedResult<ProductSearchResult>> RefineSearchByText(long channelId, long catalogId, string searchText, IEnumerable<ProductRefinerValue> refinementCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).RefineSearchByText(channelId, catalogId, searchText, refinementCriteria, queryResultSettings));
            }

            public Task<SimpleProduct> GetById(long recordId, long channelId)
            {
                var productIds = new List<long>() { recordId };
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetByIds(channelId, productIds, QueryResultSettings.SingleRecord).SingleOrDefault());
            }

            public Task<PagedResult<SimpleProduct>> GetByIds(long channelId, IEnumerable<long> productIds, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetByIds(channelId, productIds, queryResultSettings));
            }

            public Task<PagedResult<SimpleProduct>> GetVariantsByDimensionValues(long recordId, long channelId, IEnumerable<ProductDimension> matchingDimensionValues, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetVariants(channelId, recordId, null /* locale */, matchingDimensionValues.AsReadOnly(), queryResultSettings));
            }
    
            public Task<PagedResult<SimpleProduct>> GetVariantsByComponentsInSlots(long recordId, long channelId, IEnumerable<ComponentInSlotRelation> matchingSlotToComponentRelationship, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetVariants(channelId, recordId, null /* locale */, matchingSlotToComponentRelationship.AsReadOnly(), queryResultSettings));
            }
    
            public Task<PagedResult<ProductComponent>> GetDefaultComponents(long recordId, long channelId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetDefaultComponents(channelId, recordId, null /* locale */, queryResultSettings));
            }
    
            public Task<PagedResult<ProductComponent>> GetSlotComponents(long recordId, long channelId, long slotId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetSlotComponents(channelId, recordId, slotId, null /* locale */, queryResultSettings));
            }
    
            public Task<PagedResult<ProductDimensionValue>> GetDimensionValues(long recordId, long channelId, int dimension, IEnumerable<ProductDimension> matchingDimensionValues, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetDimensionValues(channelId, recordId, null /* locale */, (ProductDimensionType)dimension, matchingDimensionValues.AsReadOnly(), queryResultSettings));
            }
    
            public Task<PagedResult<AttributeValue>> GetAttributeValues(long recordId, long channelId, long catalogId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetAttributeValues(channelId, catalogId, recordId, null /* locale */, queryResultSettings));
            }
    
            public Task<PagedResult<ProductRelationType>> GetRelationTypes(long recordId, long channelId, long catalogId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRelationTypes(channelId, catalogId, recordId, null /* locale */, queryResultSettings));
            }
    
            public Task<PagedResult<ProductSearchResult>> GetRelatedProducts(long recordId, long channelId, long catalogId, long relationTypeId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRelatedProducts(channelId, catalogId, recordId, null /* locale */, relationTypeId, queryResultSettings));
            }

            public Task<PagedResult<ProductRefiner>> GetRefiners(ProductSearchCriteria productSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetRefiners(productSearchCriteria));
            }

            public Task<PagedResult<UnitOfMeasure>> GetUnitsOfMeasure(long recordId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetUnitsOfMeasure(recordId, queryResultSettings));
            }

            public Task<PagedResult<Product>> Changes(ChangedProductsSearchCriteria productSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetChangedProducts(productSearchCriteria, queryResultSettings).Results.ToCRTPagedResult());
            }
    
            public Task<PagedResult<ProductPrice>> GetPrices(string itemId, string inventoryDimensionId, string barcode, string customerAccountNumber, string unitOfMeasureSymbol, decimal quantity, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => InventoryManager.Create(CommerceRuntimeManager.Runtime).GetItemPrice(itemId, inventoryDimensionId, barcode, customerAccountNumber, unitOfMeasureSymbol, quantity));
            }
    
            public Task<PagedResult<AttributeProduct>> GetProductAttributes(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetChannelProductAttributes(queryResultSettings));
            }
    
            public Task<ReadChangedProductsSession> BeginReadChanges(ChangedProductsSearchCriteria searchCriteria)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).BeginReadChangedProducts(searchCriteria));
            }
    
            public Task EndReadChanges(ReadChangedProductsSession session)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).EndReadChangedProducts(session));
            }
    
            public Task<PagedResult<ProductPrice>> GetActivePrices(
                ProjectionDomain projectDomain,
                IEnumerable<long> productIds,
                DateTimeOffset activeDate,
                string customerId,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers,
                QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetActiveProductPrice(
                    projectDomain,
                    productIds,
                    activeDate,
                    customerId,
                    queryResultSettings,
                    affiliationLoyaltyTiers));
            }
    
            public Task<PagedResult<ProductPrice>> GetIndependentProductPriceDiscount(
                ProjectionDomain projectDomain,
                IEnumerable<long> productIds,
                string customerId,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers,
                QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).GetIndependentProductPriceDiscount(
                    projectDomain,
                    productIds,
                    customerId,
                    queryResultSettings,
                    affiliationLoyaltyTiers));
            }
    
            public Task<PagedResult<ProductExistenceId>> VerifyExistence(ProductExistenceCriteria searchCriteria, QueryResultSettings settings)
            {
                return Task.Run(() => Runtime.Client.ProductManager.Create(CommerceRuntimeManager.Runtime).VerifyProductExistence(searchCriteria, settings));
            }
    
            public Task<PagedResult<ProductAvailableQuantity>> GetProductAvailabilities(IEnumerable<long> itemIds, long channelId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.InventoryManager.Create(CommerceRuntimeManager.Runtime).GetProductAvailabilities(
                    queryResultSettings,
                    itemIds,
                    channelId));
            }
        }
    }
}
