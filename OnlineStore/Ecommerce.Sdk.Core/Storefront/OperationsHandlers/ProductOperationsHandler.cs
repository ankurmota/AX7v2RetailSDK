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
    namespace Retail.Ecommerce.Sdk.Core.OperationsHandlers
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;

        /// <summary>
        /// Handler for product operations.
        /// </summary>
        public class ProductOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProductOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public ProductOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Get available quantities of specified listings.
            /// </summary>
            /// <param name="productIds">Listing identifiers.</param>
            /// <param name="channelId">The Channel Id.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// A collection of available quantities for the listings inquired.
            /// </returns>
            /// <exception cref="System.ArgumentNullException">Throws when productIds is null.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<ProductAvailableQuantity>> GetProductAvailability(IEnumerable<long> productIds, long channelId, QueryResultSettings queryResultSettings)
            {
                if (productIds == null)
                {
                    throw new ArgumentNullException(nameof(productIds));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IProductManager productManager = managerFactory.GetManager<IProductManager>();

                PagedResult<ProductAvailableQuantity> productAvailableQuantities =
                    await productManager.GetProductAvailabilities(productIds, 0, queryResultSettings);

                return productAvailableQuantities;
            }

            /// <summary>
            /// Gets a collection of products based on some search criteria.
            /// </summary>
            /// <param name="productSearchCriteria">The search criteria.</param>
            /// <param name="catalogIds">The catalog ids.</param>
            /// <param name="queryResultSettings">The queryResultSettings.</param>
            /// <returns>
            /// Return products as paged results.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<Product>> SearchProducts(ProductSearchCriteria productSearchCriteria, IEnumerable<long> catalogIds, QueryResultSettings queryResultSettings)
            {
                if (productSearchCriteria == null)
                {
                    throw new ArgumentNullException(nameof(productSearchCriteria));
                }

                if (catalogIds == null || !catalogIds.Any())
                {
                    throw new ArgumentNullException(nameof(catalogIds));
                }

                if (Utilities.AreNonCataloguedProductsIncludedByDefault() || catalogIds.Contains(0))
                {
                    // Override the list of catalog Ids to zero, because query for zero will automatically included products from all catalogs.
                    catalogIds = new long[] { 0 };
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IOrgUnitManager orgUnitManager = managerFactory.GetManager<IOrgUnitManager>();
                long channelId = await Utilities.GetChannelId(this.EcommerceContext);

                List<Product> productsFromAllCatalogs = new List<Product>();

                IProductManager productManager = managerFactory.GetManager<IProductManager>();
                foreach (long catalogId in catalogIds.Distinct())
                {
                    productSearchCriteria.Context = new ProjectionDomain()
                    {
                        ChannelId = channelId,
                        CatalogId = catalogId
                    };

                    PagedResult<Product> products = await productManager.Search(productSearchCriteria, queryResultSettings);

                    productsFromAllCatalogs.AddRange(products.Results);
                }

                PagedResult<Product> allProducts = new PagedResult<Product>(productsFromAllCatalogs);

                return allProducts;
            }

            /// <summary>
            /// Gets Active Prices.
            /// </summary>
            /// <param name="productIds">Products' record IDs.</param>
            /// <param name="catalogId">The catalog ID.</param>
            /// <param name="affiliationLoyaltyTierIds">Affiliation Loyalty Tier IDs.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The product prices.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async methods.")]
            public async Task<PagedResult<ProductPrice>> GetActivePrices(IEnumerable<long> productIds, long? catalogId, IEnumerable<long> affiliationLoyaltyTierIds, QueryResultSettings queryResultSettings)
            {
                IProductManager manager = Utilities.GetManagerFactory(this.EcommerceContext).GetManager<IProductManager>();
                long channelId = await Utilities.GetChannelId(this.EcommerceContext);

                ProjectionDomain projectionDomain = new ProjectionDomain { ChannelId = channelId, CatalogId = catalogId };
                IList<AffiliationLoyaltyTier> tiers = null;
                if (affiliationLoyaltyTierIds != null)
                {
                    tiers = new List<AffiliationLoyaltyTier>();
                    foreach (long affiliationLoyaltyTierId in affiliationLoyaltyTierIds)
                    {
                        tiers.Add(new AffiliationLoyaltyTier { AffiliationId = affiliationLoyaltyTierId });
                    }
                }

                PagedResult<ProductPrice> productPrices = await manager.GetActivePrices(projectionDomain, productIds, DateTimeOffset.UtcNow, null, tiers, queryResultSettings);

                return productPrices;
            }

            /// <summary>
            /// Gets the products.
            /// </summary>
            /// <param name="productIds">The product ids.</param>
            /// <returns>
            /// The simple products.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async methods.")]
            public async Task<IEnumerable<SimpleProduct>> GetSimpleProducts(IEnumerable<long> productIds)
            {
                if (productIds == null)
                {
                    throw new ArgumentNullException(nameof(productIds));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IProductManager productManager = managerFactory.GetManager<IProductManager>();
                long channelId = await Utilities.GetChannelId(this.EcommerceContext);
                var distinctProductIds = productIds.Distinct();

                Task<SimpleProduct>[] simpleProductTasks = new Task<SimpleProduct>[distinctProductIds.Count()];

                // Enable batching when batching bug is fixed. Currently, batching fails if the server returns a no-content response.
                //// managerFactory.Context.BeginBatch();

                int index = 0;
                foreach (long productId in distinctProductIds)
                {
                    simpleProductTasks[index++] = productManager.GetById(productId, channelId);
                }

                //// await managerFactory.Context.ExecuteBatchAsync();

                Collection<SimpleProduct> simpleProducts = new Collection<SimpleProduct>();

                foreach (Task<SimpleProduct> simpleProductTask in simpleProductTasks)
                {
                    SimpleProduct simpleProduct = await simpleProductTask;
                    if (simpleProduct != null)
                    {
                        simpleProducts.Add(simpleProduct);
                    }
                }

                IEnumerable<ProductCatalog> productCatalogs = await this.GetProductCatalogs(Utilities.DefaultQuerySettings);
                simpleProducts = await DataAugmenter.GetAugmentedSimpleProducts(this.EcommerceContext, simpleProducts, productCatalogs);
                return simpleProducts;
            }

            /// <summary>
            /// Gets products that match the search text.
            /// </summary>
            /// <param name="searchText">The search text.</param>
            /// <param name="catalogId">The catalog identifier.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// The product search results.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async methods.")]
            public async Task<PagedResult<ProductSearchResult>> SearchByText(string searchText, long catalogId, QueryResultSettings queryResultSettings)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    throw new ArgumentNullException(nameof(searchText));
                }

                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IProductManager productManager = managerFactory.GetManager<IProductManager>();
                long channelId = await Utilities.GetChannelId(this.EcommerceContext);

                PagedResult<ProductSearchResult> productSearchResults = await productManager.SearchByText(channelId, catalogId, searchText, queryResultSettings);

                return productSearchResults;
            }

            /// <summary>
            /// Gets the product catalogs.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The product catalogs.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async methods.")]
            public async Task<PagedResult<ProductCatalog>> GetProductCatalogs(QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IProductCatalogManager productCatalogManager = managerFactory.GetManager<IProductCatalogManager>();

                long channelId = await Utilities.GetChannelId(this.EcommerceContext);

                PagedResult<ProductCatalog> productCatalogs = await productCatalogManager.GetCatalogs(channelId, activeOnly: true, queryResultSettings: queryResultSettings);

                return productCatalogs;
            }
        }
    }
}