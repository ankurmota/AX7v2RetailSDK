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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Implementation for products service.
        /// </summary>
        public class ProductsService : IRequestHandler
        {
            // Error messages.
            private const string InvalidChannelIdErrorMessage = "Channel identifier cannot be less than zero.";
    
            /// <summary>
            /// The types of service requests that this service can process.
            /// </summary>
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetProductsServiceRequest),
                typeof(GetProductAttributeValuesServiceRequest),
                typeof(GetVariantProductsServiceRequest)
            };
    
            /// <summary>
            /// Gets the collection of service request types supported by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get { return SupportedRequestTypesArray; }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
                Response response;
    
                if (request is GetProductsServiceRequest)
                {
                    response = GetProducts((GetProductsServiceRequest)request);
                }
                else if (request is GetProductAttributeValuesServiceRequest)
                {
                    response = ProcessGetProductAttributeValuesServiceRequest((GetProductAttributeValuesServiceRequest)request);
                }
                else if (request is GetVariantProductsServiceRequest)
                {
                    response = GetVariants((GetVariantProductsServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the products requested based on their record identifiers.
            /// </summary>
            /// <param name="request">The request to retrieve <see cref="SimpleProduct"/> objects.</param>
            /// <returns>The response containing the collection of products requested.</returns>
            private static GetProductsServiceResponse GetProducts(GetProductsServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                if (request.ProductIds.IsNullOrEmpty() && request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    return new GetProductsServiceResponse(PagedResult<SimpleProduct>.Empty());
                }
    
                if (request.ProductIds != null && request.ProductIds.Any() && request.ItemAndInventDimIdCombinations != null && request.ItemAndInventDimIdCombinations.Any())
                {
                    throw new ArgumentOutOfRangeException("request", "The GetProductsServiceRequest cannot be processed when both product ids and item-inventdim ids are specified. Please specify only one.");
                }

                if (request.SearchLocation == SearchLocation.Remote)
                {
                    throw new NotSupportedException(string.Format("SearchLocation {0} is not supported.", request.SearchLocation));
                }

                bool? downloadedProductsFilter = null;

                switch (request.SearchLocation)
                {
                    case SearchLocation.Local:
                        downloadedProductsFilter = false;
                        break;
                    case SearchLocation.Remote:
                        downloadedProductsFilter = true;
                        break;
                    case SearchLocation.All:
                        downloadedProductsFilter = null;
                        break;
                    default:
                        throw new InvalidOperationException(string.Format("SearchLocation '{0}' is not supported.", request.SearchLocation));
                }
                
                if (request.QueryResultSettings.Sorting.IsSpecified)
                {
                    // We have to enforce paging by "IsRemote" flag and retrieve local products first. Otherwise result set can be shifted once remote product is inserted to local database.
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "When retrieving products by identifiers only default sort order is supported.");
                }

                request.QueryResultSettings.Sorting.Add(new SortColumn(SimpleProduct.IsRemoteColumnName, isDescending: false));

                GetProductsDataRequest productsDataRequest;
                if (!request.ProductIds.IsNullOrEmpty())
                {
                    productsDataRequest = new GetProductsDataRequest(request.ProductIds, request.QueryResultSettings, downloadedProductsFilter);
                }
                else
                {
                    productsDataRequest = new GetProductsDataRequest(request.ItemAndInventDimIdCombinations, request.QueryResultSettings, downloadedProductsFilter);
                }

                PagedResult<SimpleProduct> products = request.RequestContext.Execute<EntityDataServiceResponse<SimpleProduct>>(productsDataRequest).PagedEntityCollection;

                // If requested products are not found in the current store, download it to local database as assorted to current channel and then retrieve them.
                int numberOfProductsRequested = request.ProductIds.IsNullOrEmpty() ? request.ItemAndInventDimIdCombinations.Count : request.ProductIds.Count;
                int numberOfProductsExpected = request.QueryResultSettings.Paging.NoPageSizeLimit ? numberOfProductsRequested : Math.Min(numberOfProductsRequested, (int)request.QueryResultSettings.Paging.Top);
                if (request.SearchLocation.HasFlag(SearchLocation.Remote) && (products == null || products.Results.IsNullOrEmpty() || products.TotalCount < numberOfProductsExpected))
                {
                    try
                    {
                        UpsertRemoteProductData(request);
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // Suppress FeatureNotSupportException for offline scenario.
                        // Exception will be logged by ExceptionNotificationHandler.
                    }

                    products = request.RequestContext.Execute<EntityDataServiceResponse<SimpleProduct>>(productsDataRequest).PagedEntityCollection;
                }

                products = PopulateComplexProductProperties(request.ChannelId, products, request.RequestContext, request.SearchLocation, downloadedProductsFilter, request.CalculatePrice);

                // If a product is assorted locally and downloaded from virtual catalog, we only return the locally assorted version.
                // Ideally, collapsing these should happen from the underlying product data service.
                products.Results = products.Results.GroupBy(p => p.RecordId).Select(group => group.OrderBy(p => p.IsRemote).First()).AsReadOnly();

                return new GetProductsServiceResponse(products);
            }

            private static PagedResult<SimpleProduct> PopulateComplexProductProperties(long channelId, PagedResult<SimpleProduct> products, RequestContext context, SearchLocation searchLocation, bool? downloadedProductsFilter, bool calculatePrices)
            {
                // Retrieving all id collections needed to query the Data Service for complex properties.
                var productIds = products.Results.Select(p => p.RecordId);
                var masterTypeProductIds = products.Results.Where(p => p.ProductType == ProductType.Master).Select(p => p.RecordId);
                var kitVariantTypeProductIds = products.Results.Where(p => p.ProductType == ProductType.KitVariant).Select(v => v.RecordId);
                var kitMasterTypeProductIds = products.Results.Where(p => p.ProductType == ProductType.KitMaster).Select(v => v.RecordId);
                var variantTypeProductIds = products.Results.Where(p => p.ProductType == ProductType.Variant).Select(v => v.RecordId);

                // Products of types Master and KitMaster have dimensions that need to be retrieved.
                var idsOfProductsContainingDimensions = masterTypeProductIds.Concat(kitMasterTypeProductIds);

                IEnumerable<ProductComponent> components = new List<ProductComponent>();
                IEnumerable<ProductDimension> productDimensions = new List<ProductDimension>();
                IEnumerable<ProductDimensionValue> dimensionValues = new List<ProductDimensionValue>();

                // Products of type KitVariant have components that need to be retrieved.
                if (kitVariantTypeProductIds.Any())
                {
                    var getProductComponentsDataRequestColummnSet = ProductComponent.DefaultColumnSet;
                    getProductComponentsDataRequestColummnSet.Add("VARIANTPRODUCTID");
                    var getProductComponentsDataRequestSettings = new QueryResultSettings(getProductComponentsDataRequestColummnSet, PagingInfo.AllRecords);
                    var getProductComponentsDataRequest = new GetProductComponentsForVariantProductsDataRequest(kitVariantTypeProductIds, getProductComponentsDataRequestSettings, downloadedProductsFilter);
                    components = context.Execute<EntityDataServiceResponse<ProductComponent>>(getProductComponentsDataRequest).PagedEntityCollection.Results;
                }

                if (idsOfProductsContainingDimensions.Any())
                {
                    var getDimensionsDataRequest = new GetProductDimensionsDataRequest(idsOfProductsContainingDimensions, QueryResultSettings.AllRecords);
                    productDimensions = context.Execute<EntityDataServiceResponse<ProductDimension>>(getDimensionsDataRequest).PagedEntityCollection.Results;
                }

                // Products of types Variant and KitVariant have dimension values that need to be retrieved.
                // This collection is populated after retrieving components so that dimension values of Variant type products can also be retrieved in the same transaction.
                var idsOfProductsContainingDimensionValues = variantTypeProductIds.Concat(kitVariantTypeProductIds).Concat(components.Where(c => c.ProductType == ProductType.Variant).Select(c => c.ProductId).Distinct());

                if (idsOfProductsContainingDimensionValues.Any())
                {
                    var getDimensionValuesDataRequest = new GetProductDimensionValuesForVariantProductsDataRequest(idsOfProductsContainingDimensionValues, QueryResultSettings.AllRecords, downloadedProductsFilter);
                    dimensionValues = context.Execute<EntityDataServiceResponse<ProductDimensionValue>>(getDimensionValuesDataRequest).PagedEntityCollection.Results;
                }

                var productIdsToFetchBehavior = productIds.Concat(components.Select(c => c.ProductId).Distinct());
                var productBehaviorSettings = new QueryResultSettings(ProductBehavior.DefaultColumnSet, PagingInfo.CreateWithExactCount(productIdsToFetchBehavior.Count(), skip: 0));
                var productsBehaviorDataRequest = new GetProductBehaviorDataRequest(productIdsToFetchBehavior, productBehaviorSettings, downloadedProductsFilter);
                PagedResult<ProductBehavior> productsBehavior = context.Execute<EntityDataServiceResponse<ProductBehavior>>(productsBehaviorDataRequest).PagedEntityCollection;

                var getLinkedProductRelationsDataRequest = new GetLinkedProductRelationsDataRequest(productIds, QueryResultSettings.AllRecords, downloadedProductsFilter);
                PagedResult<LinkedProductRelation> linkedProductRelations = context.Execute<EntityDataServiceResponse<LinkedProductRelation>>(getLinkedProductRelationsDataRequest).PagedEntityCollection;

                var linkedProductIds = linkedProductRelations.Results.Select(r => r.LinkedProductId).Distinct();
                var getLinkedProductsDataRequest = new GetProductsServiceRequest(channelId, linkedProductIds, QueryResultSettings.AllRecords);
                getLinkedProductsDataRequest.SearchLocation = searchLocation;
                PagedResult<SimpleProduct> linkedProducts = context.Execute<GetProductsServiceResponse>(getLinkedProductsDataRequest).Products;

                PagedResult<ProductPrice> productPrices = PagedResult<ProductPrice>.Empty();

                if (calculatePrices)
                {
                    // Pricing APIs currently take Product instead of SimpleProduct. We manually build a Product object
                    // and populate the required field in the interim until uptake of SimpleProduct in pricing service.
                    List<Product> productsTransformedForPricing = ConvertSimpleProductsToProducts(products.Results).ToList();

                    var priceRequest = new GetProductPricesServiceRequest(productsTransformedForPricing);
                    productPrices = context.Execute<GetProductPricesServiceResponse>(priceRequest).ProductPrices;
                }

                return InsertProductPropertiesIntoProduct(products, productsBehavior, productPrices, linkedProductRelations, linkedProducts, components, productDimensions, dimensionValues);
            }

            private static PagedResult<SimpleProduct> InsertProductPropertiesIntoProduct(PagedResult<SimpleProduct> products, PagedResult<ProductBehavior> productsBehavior, PagedResult<ProductPrice> productPrices, PagedResult<LinkedProductRelation> linkedProductRelations, PagedResult<SimpleProduct> linkedProducts, IEnumerable<ProductComponent> components, IEnumerable<ProductDimension> productDimensions, IEnumerable<ProductDimensionValue> dimensionValues)
            {
                // Creating a dictionary for product prices to avoid having to loop over all product prices while looping over products to populate values.
                Dictionary<long, ProductPrice> productPriceDictionary = productPrices.Results.ToDictionary(price => price.ProductId, price => price);

                foreach (var product in products.Results)
                {
                    var matchingBehaviors = productsBehavior.Results.Where(r => r.ProductId == product.RecordId);
                    var matchingLinkedProductRelations = linkedProductRelations.Results.Where(r => r.ProductId == product.RecordId).Distinct();

                    if (matchingLinkedProductRelations.Any())
                    {
                        foreach (var linkedProductRelation in matchingLinkedProductRelations)
                        {
                            var matchingProduct = linkedProducts.Results.Where(l => l.RecordId == linkedProductRelation.LinkedProductId && l.IsRemote == product.IsRemote).SingleOrDefault();

                            if (matchingProduct != null)
                            {
                                // Merge product with relation rule to generate linked product.
                                SimpleLinkedProduct linkedProduct = new SimpleLinkedProduct();
                                linkedProduct.AdjustedPrice = matchingProduct.AdjustedPrice;
                                linkedProduct.BasePrice = matchingProduct.BasePrice;
                                linkedProduct.Behavior = matchingProduct.Behavior;
                                linkedProduct.DefaultUnitOfMeasure = linkedProductRelation.UnitOfMeasure;
                                linkedProduct.Description = matchingProduct.Description;
                                linkedProduct.ExtensionProperties = matchingProduct.ExtensionProperties;
                                linkedProduct.ItemId = matchingProduct.ItemId;
                                linkedProduct.Name = matchingProduct.Name;
                                linkedProduct.Price = matchingProduct.Price;
                                linkedProduct.ProductType = matchingProduct.ProductType;
                                linkedProduct.Quantity = linkedProductRelation.Quantity;
                                linkedProduct.RecordId = matchingProduct.RecordId;
                                linkedProduct.Dimensions = matchingProduct.Dimensions;

                                product.LinkedProducts.Add(linkedProduct);
                            }
                            else
                            {
                                RetailLogger.Log.CrtServicesLinkedProductNotFound(linkedProductRelation.LinkedProductId, product.RecordId, product.IsRemote);
                            }
                        }
                    }

                    if (matchingBehaviors.Count() == 1)
                    {
                        product.Behavior = matchingBehaviors.Single();
                    }
                    else
                    {
                        var ex = new InvalidOperationException(string.Format("Exactly one behavior should be found for a product. But, '{0}' behavior(s) were found for product with identifier '{1}'.", matchingBehaviors.Count(), product.RecordId));
                        RetailLogger.Log.CrtServicesInvalidNumberOfProductBehaviorsFound(product.RecordId, matchingBehaviors.Count(), ex);

                        throw ex;
                    }

                    if (product.ProductType == ProductType.KitVariant)
                    {
                        product.Components = components.Where(c => c.VariantProductId == product.RecordId).Distinct().ToList();

                        foreach (var component in product.Components)
                        {
                            var matchingBehaviorsForComponent = productsBehavior.Results.Where(b => b.ProductId == component.ProductId).Distinct();
                            var matchingDimensionValues = dimensionValues.Where(p => p.ProductId == component.ProductId).ToList();
                            component.Dimensions = new List<ProductDimension>();

                            foreach (var dimensionValue in matchingDimensionValues)
                            {
                                var dimension = new ProductDimension()
                                {
                                    DimensionType = dimensionValue.DimensionType,
                                    DimensionValue = dimensionValue
                                };

                                component.Dimensions.Add(dimension);
                            }

                            if (matchingBehaviorsForComponent.Count() == 1)
                            {
                                component.Behavior = matchingBehaviorsForComponent.Single();
                            }
                            else
                            {
                                var ex = new InvalidOperationException(string.Format("Exactly one behavior should be found for a product. But '{0}' behavior(s) were found for product component with identifier '{1}'.", matchingBehaviorsForComponent.Count(), product.RecordId));
                                RetailLogger.Log.CrtServicesInvalidNumberOfProductBehaviorsFound(product.RecordId, matchingBehaviorsForComponent.Count(), ex);

                                throw ex;
                            }
                        }
                    }

                    if (product.ProductType == ProductType.Master)
                    {
                        product.Dimensions = productDimensions.Where(p => p.ProductId == product.RecordId).ToList();
                    }

                    if (product.ProductType == ProductType.Variant || product.ProductType == ProductType.KitVariant)
                    {
                        var matchingDimensionValues = dimensionValues.Where(p => p.ProductId == product.RecordId).ToList();
                        product.Dimensions = new List<ProductDimension>();

                        foreach (var dimensionValue in matchingDimensionValues)
                        {
                            var dimension = new ProductDimension()
                            {
                                DimensionType = dimensionValue.DimensionType,
                                DimensionValue = dimensionValue
                            };

                            product.Dimensions.Add(dimension);
                        }
                    }

                    ProductPrice productPrice;
                    if (productPriceDictionary.TryGetValue(product.RecordId, out productPrice))
                    {
                        product.SetPrice(productPrice);
                    }
                }

                return products;
            }

            private static IEnumerable<Product> ConvertSimpleProductsToProducts(IEnumerable<SimpleProduct> simpleProducts)
            {
                List<Product> productsTransformedForPricing = new List<Product>(simpleProducts.Count());

                foreach (SimpleProduct simpleProduct in simpleProducts)
                {
                    Product product = null;

                    if (simpleProduct.MasterProductId.HasValue)
                    {
                        product = (productsTransformedForPricing.Where(p => p.RecordId == simpleProduct.MasterProductId.Value).Count() == 1) ? productsTransformedForPricing.Single(p => p.RecordId == simpleProduct.MasterProductId.Value) : null;
                    }

                    if (product == null)
                    {
                        product = new Product
                        {
                            RecordId = simpleProduct.MasterProductId.HasValue ? simpleProduct.MasterProductId.Value : simpleProduct.RecordId,
                            ItemId = simpleProduct.ItemId,
                            DefaultUnitOfMeasure = simpleProduct.DefaultUnitOfMeasure,
                            IsRemote = simpleProduct.IsRemote,
                            IsMasterProduct = simpleProduct.ProductType == ProductType.Master || simpleProduct.ProductType == ProductType.KitMaster || simpleProduct.ProductType == ProductType.Variant || simpleProduct.ProductType == ProductType.KitVariant,
                        };
                    }

                    if (product.CompositionInformation == null)
                    {
                        product.CompositionInformation = new ProductCompositionInformation
                        {
                            VariantInformation = new ProductVariantInformation()
                        };
                    }

                    if (simpleProduct.ProductType == ProductType.Variant || simpleProduct.ProductType == ProductType.KitVariant)
                    {
                        product.CompositionInformation.VariantInformation.IndexedVariants.Add(
                            simpleProduct.RecordId,
                            ProductVariant.ConvertFrom(simpleProduct));
                    }

                    if (!productsTransformedForPricing.Exists(p => p.RecordId == (simpleProduct.MasterProductId.HasValue ? simpleProduct.MasterProductId.Value : simpleProduct.RecordId)))
                    {
                        productsTransformedForPricing.Add(product);
                    }
                }

                return productsTransformedForPricing;
            }

            private static GetProductAttributeValuesServiceResponse ProcessGetProductAttributeValuesServiceRequest(GetProductAttributeValuesServiceRequest request)
            {
                var getProductAttributeValuesDataRequest = new GetProductAttributeValuesDataRequest(request.ChannelId, request.CatalogId, request.ProductId, QueryResultSettings.AllRecords);
                var attributeValueCombinations = request.RequestContext.Execute<EntityDataServiceResponse<AttributeValue>>(getProductAttributeValuesDataRequest).PagedEntityCollection.Results;

                var attributeValues = new List<AttributeValue>();
                var attributeIdToValuesMap = attributeValueCombinations.GroupBy(a => a.RecordId).ToDictionary(v => v.Key, v => v.ToList());

                // Retrieving catalogs associated to product to select most refined attribute values based on latest catalog attribute value.
                var columnsToBeRetrieved = new string[] { "CATALOG", "PRODUCT" };
                var settings = new QueryResultSettings(new ColumnSet(columnsToBeRetrieved), PagingInfo.AllRecords, new SortingInfo("VALIDFROM"));
                var getCatalogsAssociatedToProductsDataRequest = new GetCatalogsAssociatedToProductsDataRequest(request.ChannelId, new List<long>() { request.ProductId }, settings);
                var catalogsAssociatedToProduct = request.RequestContext.Execute<EntityDataServiceResponse<ProductCatalog>>(getCatalogsAssociatedToProductsDataRequest).PagedEntityCollection.Results;
                Dictionary<long, Tuple<ProductCatalog, int>> activeCatalogMap;
                Dictionary<long, List<long>> activeProductCatalogMap;

                ExtractActiveCatalogMap(catalogsAssociatedToProduct, out activeCatalogMap, out activeProductCatalogMap);

                foreach (var attributeValuesForOneAttribute in attributeIdToValuesMap)
                {
                    if (attributeValuesForOneAttribute.Value.Count == 1)
                    {
                        attributeValues.Add(attributeValuesForOneAttribute.Value.Single());
                    }
                    else
                    {
                        AttributeValue mostRefinedAttributeValue = null;
    
                        foreach (var potentialAttributeValue in attributeValuesForOneAttribute.Value)
                        {
                            if (mostRefinedAttributeValue != null)
                            {
                                mostRefinedAttributeValue = SelectMostRefinedProperty(activeCatalogMap, mostRefinedAttributeValue, potentialAttributeValue);
                            }
                            else
                            {
                                mostRefinedAttributeValue = potentialAttributeValue;
                            }
                        }
    
                        attributeValues.Add(mostRefinedAttributeValue);
                    }
                }
    
                return new GetProductAttributeValuesServiceResponse(new PagedResult<AttributeValue>(attributeValues.AsReadOnly(), request.QueryResultSettings.Paging));
            }
    
            // If changing this function, please change a copy of this function in Runtime.Data.Managers.ProductBuilder.
            private static AttributeValue SelectMostRefinedProperty(Dictionary<long, Tuple<ProductCatalog, int>> activeCatalogMap, AttributeValue leftAttributeValue, AttributeValue rightAttributeValue)
            {
                if (object.ReferenceEquals(leftAttributeValue, rightAttributeValue))
                {
                    // eeny meeny
                    return rightAttributeValue;
                }
    
                bool leftWins, rightWins;
    
                // order of evaluation:
                // - catalog: most specific/recent wins
                // - source: bigger value wins
                // - reference: false > true
                // - distance: smaller value wins
                // - product category: smaller value wins (and that's a bug; should be primary, then whatever)
                // - value: bigger value wins
                do
                {
                    // catalog comparison
                    var leftCatIdx = activeCatalogMap.ContainsKey(leftAttributeValue.CatalogId) ? activeCatalogMap[leftAttributeValue.CatalogId].Item2 : short.MaxValue;
                    var rightCatIdx = activeCatalogMap.ContainsKey(rightAttributeValue.CatalogId) ? activeCatalogMap[rightAttributeValue.CatalogId].Item2 : short.MaxValue;
                    if (leftCatIdx != rightCatIdx)
                    {
                        leftWins = leftCatIdx < rightCatIdx;
                        break;
                    }
    
                    // source comparison
                    if (leftAttributeValue.Source != rightAttributeValue.Source)
                    {
                        leftWins = leftAttributeValue.Source > rightAttributeValue.Source;
                        break;
                    }
    
                    // reference comparison
                    if (leftAttributeValue.IsReference ^ rightAttributeValue.IsReference)
                    {
                        leftWins = !leftAttributeValue.IsReference;
                        break;
                    }
    
                    // distance to ancestor
                    if (leftAttributeValue.Distance != rightAttributeValue.Distance)
                    {
                        leftWins = leftAttributeValue.Distance < rightAttributeValue.Distance;
                        break;
                    }
    
                    // source product category
                    if (leftAttributeValue.CategoryId != rightAttributeValue.CategoryId)
                    {
                        // this is a bit of a contentious point - what this means is that the attribute has been assigned to this
                        // product via 2 categories. If either is 0, the non-0 wins. If neither is 0, that which corresponds to the
                        // primary category wins - but we don't know that, and both can be non-primary. Other than that, the most
                        // derived category wins, if they're on the same path in the tree. (No, we don't know that, either.) Lastly,
                        // if they're completely unrelated, the newest wins, indicated by the greater id. We'll pick that heuristic.
                        leftWins = leftAttributeValue.CategoryId != 0
                                || leftAttributeValue.CategoryId < rightAttributeValue.CategoryId;
                        break;
                    }
    
                    // lastly, value
                    leftWins = leftAttributeValue.AttributeValueId > rightAttributeValue.AttributeValueId;
                    rightWins = rightAttributeValue.AttributeValueId > leftAttributeValue.AttributeValueId;
    
                    if (leftWins ^ rightWins)
                    {
                        // we were not able to break the tie; could pick any and be on our merry, but that is an unexpected situation
                        // from a data and representation standpoint.
                        string message = string.Format(
                            "unexpected property value conflict for property {0}: left product id {1} conflicts with right product id {2}.",
                            rightAttributeValue.Name,
                            leftAttributeValue.ProductId,
                            rightAttributeValue.ProductId);
    
                        throw new ArgumentException(message);
                    }
                }
                while (false);
    
                return leftWins ? leftAttributeValue : rightAttributeValue;
            }
    
            // If changing this function, please change a copy of this function in Runtime.Data.Managers.ProductBuilder.
            private static void ExtractActiveCatalogMap(
                ReadOnlyCollection<ProductCatalog> dataSet,
                out Dictionary<long, Tuple<ProductCatalog, int>> activeCatalogMap,
                out Dictionary<long, List<long>> activeProductCatalogMap)
            {
                var catalogMap = new Dictionary<long, Tuple<ProductCatalog, int>>();
                var productCatalogMap = new Dictionary<long, List<long>>();
    
                int idx = 0;
                foreach (var productCatalogAssociation in dataSet)
                {
                    if (!catalogMap.ContainsKey(productCatalogAssociation.RecordId))
                    {
                        catalogMap.Add(productCatalogAssociation.RecordId, new Tuple<ProductCatalog, int>(productCatalogAssociation, idx++));
                    }
    
                    if (!productCatalogMap.ContainsKey(productCatalogAssociation.ProductId))
                    {
                        productCatalogMap[productCatalogAssociation.ProductId] = new List<long>();
                    }
    
                    productCatalogMap[productCatalogAssociation.ProductId].Add(productCatalogAssociation.RecordId);
                }
    
                activeCatalogMap = catalogMap;
                activeProductCatalogMap = productCatalogMap;
            }
    
            private static GetProductsServiceResponse GetVariants(GetVariantProductsServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.ChannelId, InvalidChannelIdErrorMessage);
                }
    
                long variantId;
    
                if (!request.MatchingDimensionValues.IsNullOrEmpty())
                {
                    var getVariantProductIdsDataRequest = new GetVariantProductIdsDataRequest(request.ChannelId, request.MasterProductId, request.MatchingDimensionValues, request.QueryResultSettings);
                    variantId = request.RequestContext.Execute<SingleEntityDataServiceResponse<long>>(getVariantProductIdsDataRequest).Entity;
                }
                else if (!request.MatchingSlotToComponentRelations.IsNullOrEmpty())
                {
                    var getVariantProductIdsDataRequest = new GetVariantProductIdsDataRequest(request.ChannelId, request.MasterProductId, request.MatchingSlotToComponentRelations, request.QueryResultSettings);
                    variantId = request.RequestContext.Execute<SingleEntityDataServiceResponse<long>>(getVariantProductIdsDataRequest).Entity;
                }
                else
                {
                    throw new NotSupportedException("Please specify exactly one of dimension value(s) or component(s) to retrieve variant products.");
                }
    
                var getVariantProductsDataRequest = new GetProductsServiceRequest(request.ChannelId, new List<long>() { variantId }, QueryResultSettings.AllRecords);
                var variantProducts = request.RequestContext.Execute<GetProductsServiceResponse>(getVariantProductsDataRequest).Products;
    
                return new GetProductsServiceResponse(variantProducts);
            }
    
            private static void UpsertRemoteProductData(GetProductsServiceRequest request)
            {
                GetProductDataRealtimeRequest getProductDataServiceRequest = null;

                if (!request.ProductIds.IsNullOrEmpty())
                {
                    getProductDataServiceRequest = new GetProductDataRealtimeRequest(request.ProductIds);
                }
                else if (!request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    var itemIds = request.ItemAndInventDimIdCombinations.Select(i => i.ItemId);
                    getProductDataServiceRequest = new GetProductDataRealtimeRequest(itemIds);
                }
                else
                {
                    return;
                }

                var getProductDataResponse = request.RequestContext.Execute<GetProductDataRealtimeResponse>(getProductDataServiceRequest);
                var productsXml = getProductDataResponse.ProductDataXml;
    
                var manager = new ProductDataManager(request.RequestContext);
                manager.SaveProductData(productsXml);
            }
        }
    }
}