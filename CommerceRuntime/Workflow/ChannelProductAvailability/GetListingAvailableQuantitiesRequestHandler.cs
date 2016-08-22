/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Retrieves quantities of provided listings from the default warehouse associated with the customer.
        /// </summary>
        public sealed class GetListingAvailableQuantitiesRequestHandler : SingleRequestHandler<GetListingAvailableQuantitiesRequest, GetListingAvailableQuantitiesResponse>
        {
            /// <summary>
            /// Processes the specified request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// Available quantities of specified listings at the requested warehouse.
            /// </returns>
            protected override GetListingAvailableQuantitiesResponse Process(GetListingAvailableQuantitiesRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrEmpty(request.ProductIds, "No Ids have been provided");
    
                QueryResultSettings settings = QueryResultSettings.AllRecords;
                var productIds = request.ProductIds.Distinct().ToList();
                ProductSearchCriteria queryCriteria = new ProductSearchCriteria(this.Context.GetPrincipal().ChannelId)
                {
                    Ids = productIds,
                    DataLevel = CommerceEntityDataLevel.Identity,
                };
    
                var productSearchResult = request.RequestContext.Runtime.Execute<ProductSearchServiceResponse>(
                        new ProductSearchServiceRequest(queryCriteria, settings), request.RequestContext).ProductSearchResult;
    
                if (productSearchResult.Results.IsNullOrEmpty())
                {
                    string nonResolvedProductIdsInfo = string.Join(" ", productIds);
                    //// This is a valid situtation for cross channel scenarios, wish lists for example.
                    NetTracer.Warning("None of the specified product ids were found on the current channel {0}. ProductIds = {1}", this.Context.GetPrincipal().ChannelId, nonResolvedProductIdsInfo);
                    return new GetListingAvailableQuantitiesResponse();
                }
    
                var productMap = productSearchResult.Results.ToDictionary(p => p.RecordId, p => p);
                var items = this.GetItemAndInventDimId(productIds, productSearchResult, productMap);
                settings = new QueryResultSettings(new PagingInfo(items.Count(), 0));
                var itemAvailabilities = new HashSet<ItemAvailability>();
                var itemUnits = new HashSet<ItemUnit>();
                if (request.ChannelId == 0)
                {
                    var itemVariantInventoryDimensions = new HashSet<ItemVariantInventoryDimension>();
                    foreach (var item in items)
                    {
                        itemVariantInventoryDimensions.Add(new ItemVariantInventoryDimension(item.Item1, item.Item2));
                    }
    
                    var itemAvailableQuantitiesRequest = new GetItemAvailableQuantitiesByItemsServiceRequest(settings, itemVariantInventoryDimensions, string.Empty);
                    var itemAvailableQuantitiesResponse = this.Context.Execute<GetItemAvailableQuantitiesByItemsServiceResponse>(itemAvailableQuantitiesRequest);
                    foreach (var quantity in itemAvailableQuantitiesResponse.ItemAvailableQuantities.Results)
                    {
                        if (quantity != null)
                        {
                            var productAvailableQuantity = new ItemAvailability
                            {
                                ItemId = quantity.ItemId,
                                VariantInventoryDimensionId = quantity.VariantInventoryDimensionId,
                                AvailableQuantity = quantity.AvailableQuantity,
                                UnitOfMeasure = quantity.UnitOfMeasure
                            };
    
                            itemAvailabilities.Add(productAvailableQuantity);
    
                            var itemUnit = new ItemUnit
                            {
                                ItemId = quantity.ItemId,
                                VariantInventoryDimensionId = quantity.VariantInventoryDimensionId,
                                UnitOfMeasure = items.Where(i => i.Item1.Equals(quantity.ItemId) && i.Item2.Equals(quantity.VariantInventoryDimensionId)).SingleOrDefault().Item3
                            };
    
                            itemUnits.Add(itemUnit);
                        }
                    }
                }
                else
                {
                    var itemWarehouses = new HashSet<ItemWarehouse>();
                    foreach (var item in items)
                    {
                        var itemWarehouse = new ItemWarehouse()
                        {
                            ItemId = item.Item1,
                            VariantInventoryDimensionId = item.Item2,
                            InventoryLocationId = this.Context.GetChannelConfiguration().InventLocation
                        };
                        itemWarehouses.Add(itemWarehouse);
                    }
    
                    var warehouseRequest = new GetItemAvailabilitiesByItemWarehousesServiceRequest(settings, itemWarehouses);
                    var warehouseResponse = this.Context.Execute<GetItemAvailabilitiesByItemWarehousesServiceResponse>(warehouseRequest);
                    foreach (var quantity in warehouseResponse.ItemAvailabilities.Results)
                    {
                        if (quantity != null)
                        {
                            itemAvailabilities.Add(quantity);
    
                            var itemUnit = new ItemUnit
                            {
                                ItemId = quantity.ItemId,
                                VariantInventoryDimensionId = quantity.VariantInventoryDimensionId,
                                UnitOfMeasure = items.Where(i => i.Item1.Equals(quantity.ItemId) && i.Item2.Equals(quantity.VariantInventoryDimensionId)).SingleOrDefault().Item3
                            };
    
                            itemUnits.Add(itemUnit);
                        }
                    }
                }
    
                var itemAvailabilitiesList = ChannelAvailabilityHelper.ConvertUnitOfMeasure(this.Context, itemAvailabilities.ToList(), itemUnits.ToList());
                var processedAvailabilities = this.ProcessItemAvailabilities(itemAvailabilitiesList, productIds, productSearchResult, productMap);
    
                return new GetListingAvailableQuantitiesResponse(processedAvailabilities.AsPagedResult());
            }
    
            /// <summary>
            /// Compiles results by preventing duplicates and calculating sum of quantity for master products.
            /// </summary>
            /// <param name="quantities">List of ProductAvailableQuantity.</param>
            /// <param name="productIds">List of product identifiers.</param>
            /// <param name="productSearchResult">The search result from performing a SearchProducts on the product ids.</param>
            /// <param name="productMap">A mapping of product identifiers to the respective product object.</param>
            /// <returns>List of ProductAvailableQuantity mapped to the appropriate product identifier.</returns>
            private IEnumerable<ProductAvailableQuantity> ProcessItemAvailabilities(IEnumerable<ItemAvailability> quantities, IEnumerable<long> productIds, ProductSearchResultContainer productSearchResult, Dictionary<long, Product> productMap)
            {
                var productAvailableQuantities = new HashSet<ProductAvailableQuantity>();
    
                foreach (var requestedProductId in productIds)
                {
                    long productId;
                    Product product;
                    ProductVariant variant;
                    ProductAvailableQuantity productAvailableQuantity;
    
                    if (!productSearchResult.ProductIdLookupMap.TryGetValue(requestedProductId, out productId))
                    {
                        continue;
                    }
    
                    if (!productMap.TryGetValue(productId, out product))
                    {
                        continue;
                    }
    
                    if (product.IsMasterProduct
                        && product.RecordId != requestedProductId
                        && product.TryGetVariant(requestedProductId, out variant))
                    {
                        // Product is a variant.
                        var quantity = quantities.Where(i => i.ItemId.Equals(variant.ItemId, StringComparison.OrdinalIgnoreCase) && i.VariantInventoryDimensionId.Equals(variant.InventoryDimensionId, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                        productAvailableQuantity = new ProductAvailableQuantity()
                        {
                            ProductId = requestedProductId,
                            AvailableQuantity = quantity != null ? quantity.AvailableQuantity : 0m,
                            UnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                        };
                    }
                    else if (product.IsMasterProduct)
                    {
                        // Product is a master, so quantities of all it's variants will be retrieved.
                        var variants = quantities.Where(i => i.ItemId.Equals(product.ItemId, StringComparison.OrdinalIgnoreCase));
                        decimal totalQuantity = variants.Sum(i => i.AvailableQuantity);
                        productAvailableQuantity = new ProductAvailableQuantity
                        {
                            ProductId = product.RecordId,
                            AvailableQuantity = totalQuantity,
                            UnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                        };
                    }
                    else
                    {
                        // Product is a standalone. Hence, InventDimId is set to empty.
                        var quantity = quantities.Where(i => i.ItemId.Equals(product.ItemId, StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(i.VariantInventoryDimensionId)).SingleOrDefault();
                        productAvailableQuantity = new ProductAvailableQuantity
                        {
                            ProductId = product.RecordId,
                            AvailableQuantity = quantity != null ? quantity.AvailableQuantity : 0m,
                            UnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                        };
                    }

                    productAvailableQuantities.Add(productAvailableQuantity);
                }
    
                return productAvailableQuantities;
            }
    
            /// <summary>
            /// Get a tuple of item identifiers and inventory dimension identifiers for every product identifier.
            /// </summary>
            /// <param name="productIds">List of product identifiers.</param>
            /// <param name="productSearchResult">The search result from performing a SearchProducts on the product identifiers.</param>
            /// <param name="productMap">A mapping of product identifiers to the respective product object.</param>
            /// <returns>List of item identifiers, inventory dimension identifiers and unit of measures.</returns>
            private IEnumerable<Tuple<string, string, string>> GetItemAndInventDimId(IEnumerable<long> productIds, ProductSearchResultContainer productSearchResult, Dictionary<long, Product> productMap)
            {
                var items = new HashSet<Tuple<string, string, string>>();
    
                foreach (var requestedProductId in productIds)
                {
                    long productId;
                    Product product;
                    ProductVariant variant;
    
                    if (!productSearchResult.ProductIdLookupMap.TryGetValue(requestedProductId, out productId))
                    {
                        continue;
                    }
    
                    if (!productMap.TryGetValue(productId, out product))
                    {
                        continue;
                    }
    
                    if (product.IsMasterProduct
                        && product.RecordId != requestedProductId
                        && product.TryGetVariant(requestedProductId, out variant))
                    {
                        // Product is a variant.
                        items.Add(new Tuple<string, string, string>(variant.ItemId, variant.InventoryDimensionId, product.Rules.DefaultUnitOfMeasure));
                    }
                    else if (product.IsMasterProduct)
                    {
                        // Product is a master, so quantities of all it's variants will be retrieved.
                        var variants = product.GetVariants();
                        foreach (var item in variants)
                        {
                            items.Add(new Tuple<string, string, string>(item.ItemId, item.InventoryDimensionId, product.Rules.DefaultUnitOfMeasure));
                        }
                    }
                    else
                    {
                        // Product is a standalone. Hence, InventDimId is set to empty.
                        items.Add(new Tuple<string, string, string>(product.ItemId, string.Empty, product.Rules.DefaultUnitOfMeasure));
                    }
                }
    
                return items;
            }
        }
    }
}
