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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using DataServices;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Product;
    
        /// <summary>
        /// Product SQLite data service class.
        /// </summary>
        public sealed class ProductSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetProductPartsDataRequest),
                        typeof(GetProductUnitOfMeasureOptionsDataRequest),
                        typeof(GetParentKitDataRequest),
                        typeof(ProductSearchDataRequest),
                        typeof(GetItemsDataRequest),
                        typeof(GetProductCatalogAssociationsDataRequest),
                        typeof(GetLinkedProductsDataRequest),
                        typeof(GetProductVariantsDataRequest),
                        typeof(GetProductKitDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;
    
                if (request is GetProductPartsDataRequest)
                {
                    response = GetProductsParts((GetProductPartsDataRequest)request);
                }
                else if (request is GetProductUnitOfMeasureOptionsDataRequest)
                {
                    response = GetProductUnitOfMeasureOptions((GetProductUnitOfMeasureOptionsDataRequest)request);
                }
                else if (request is GetParentKitDataRequest)
                {
                    response = GetParentProductKit((GetParentKitDataRequest)request);
                }
                else if (request is ProductSearchDataRequest)
                {
                    response = SearchProducts((ProductSearchDataRequest)request);
                }
                else if (request is GetItemsDataRequest)
                {
                    response = GetItems((GetItemsDataRequest)request);
                }
                else if (request is GetProductCatalogAssociationsDataRequest)
                {
                    response = GetProductCatalogAssociations((GetProductCatalogAssociationsDataRequest)request);
                }
                else if (request is GetLinkedProductsDataRequest)
                {
                    response = GetLinkedProducts((GetLinkedProductsDataRequest)request);
                }
                else if (request is GetProductVariantsDataRequest)
                {
                    response = GetProductVariants((GetProductVariantsDataRequest)request);
                }
                else if (request is GetProductKitDataRequest)
                {
                    response = GetProductKit((GetProductKitDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<ProductVariant> GetProductVariants(GetProductVariantsDataRequest request)
            {
                PagedResult<ProductVariant> variants;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(request.RequestContext))
                {
                    string languageId = request.RequestContext.GetChannelConfiguration().DefaultLanguageId;
                    GetProductVariantsProcedure getVariantsProcedure = new GetProductVariantsProcedure(context, languageId, request.QueryResultSettings);
                    variants = getVariantsProcedure.Execute(request.ItemAndInventoryDimensionIds);
                }
    
                return new EntityDataServiceResponse<ProductVariant>(variants);
            }
    
            private static ProductSearchDataResponse SearchProducts(ProductSearchDataRequest request)
            {
                var searchProductsProcedure = new SearchProductsProcedure(request);
                return searchProductsProcedure.Execute();
            }
    
            private static GetParentKitDataResponse GetParentProductKit(GetParentKitDataRequest request)
            {
                GetKitComponentsProcedure getKitComponentsProcedure = new GetKitComponentsProcedure(request);
                PagedResult<KitComponent> kitComponents = getKitComponentsProcedure.Execute();
    
                return new GetParentKitDataResponse(kitComponents);
            }
    
            private static GetProductKitDataResponse GetProductKit(GetProductKitDataRequest request)
            {
                GetKitDefinitionProcedure getKitDefinitionProcedure = new GetKitDefinitionProcedure(request);
                ReadOnlyCollection<KitDefinition> kitDefinitions = getKitDefinitionProcedure.Execute();
    
                GetKitComponentAndSubstituteProcedure getKitComponentAndSubstituteProcedure = new GetKitComponentAndSubstituteProcedure(request);
                ReadOnlyCollection<KitComponent> componentAndSubstituteList = getKitComponentAndSubstituteProcedure.Execute();
    
                GetKitVariantMapProcedure getKitVariantMapProcedure = new GetKitVariantMapProcedure(request);
                ReadOnlyCollection<KitConfigToComponentAssociation> configToComponentAssociations = getKitVariantMapProcedure.Execute();
    
                return new GetProductKitDataResponse(kitDefinitions, componentAndSubstituteList, configToComponentAssociations);
            }
    
            private static GetProductUnitOfMeasureOptionsDataResponse GetProductUnitOfMeasureOptions(GetProductUnitOfMeasureOptionsDataRequest request)
            {
                var getUnitOfMeasureOptionsProcedure = new GetUnitOfMeasureOptionsProcedure(request);
                return getUnitOfMeasureOptionsProcedure.Execute();
            }
    
            private static GetLinkedProductsDataResponse GetLinkedProducts(GetLinkedProductsDataRequest request)
            {
                ReadOnlyCollection<LinkedProduct> linkedProducts;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(request.RequestContext))
                {
                    GetLinkedProductsProcedure getLinkedProductsProcedure = new GetLinkedProductsProcedure(context, context.ChannelId, request.ProductIds);
                    linkedProducts = getLinkedProductsProcedure.Execute();
                }
    
                return new GetLinkedProductsDataResponse(linkedProducts);
            }
    
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "[TODO] Bug #3312134")]
            private static GetProductCatalogAssociationsDataResponse GetProductCatalogAssociations(GetProductCatalogAssociationsDataRequest request)
            {
                return new GetProductCatalogAssociationsDataResponse(new ReadOnlyCollection<ProductCatalogAssociation>(new List<ProductCatalogAssociation>()));
            }
    
            private static GetProductPartsDataResponse GetProductsParts(GetProductPartsDataRequest request)
            {
                // quick eval of the search criteria; if by id only, we'll jump straight to the main sproc
                // if none, return an empty collection, rather than retrieving the whole space.
                bool useCategoryIds = !request.Criteria.CategoryIds.IsNullOrEmpty();
                bool useKeywords = !string.IsNullOrWhiteSpace(request.Criteria.SearchCondition);
                bool useBarcodes = !request.Criteria.Barcodes.IsNullOrEmpty();
                bool useItemIds = !request.Criteria.ItemIds.IsNullOrEmpty();
                bool useRefiners = !request.Criteria.Refinement.IsNullOrEmpty();
    
                var doSearch = useCategoryIds || useItemIds || useKeywords || useBarcodes;
                if (doSearch)
                {
                    ProductSearchDataRequest searchDataServiceRequest = new ProductSearchDataRequest(request.Criteria, request.QueryResultSettings);
                    var searchDataServiceResponse = request.RequestContext.Runtime.Execute<ProductSearchDataResponse>(searchDataServiceRequest, request.RequestContext);
    
                    request.Criteria.Ids = searchDataServiceResponse.ProductIds.ToList();
                }
    
                if (useRefiners)
                {
                    // Refinement requires a minimum of DataLevel 2 and can not skip variants
                    request.Criteria.SkipVariantExpansion = false;
                    if (request.Criteria.DataLevel < CommerceEntityDataLevel.Extended)
                    {
                        request.Criteria.DataLevel = CommerceEntityDataLevel.Extended;
                    }
                }
    
                // due to SqlServer implementation being changed regarding SkipVariantExpansion, Sqlite cannot support this flag as false
                // and needs to run the variation expansion code
                request.Criteria.SkipVariantExpansion = false;
    
                var getProductsProcedure = new GetProductsProcedure(request);
                return getProductsProcedure.Execute();
            }
    
            private static GetItemsDataResponse GetItems(GetItemsDataRequest request)
            {
                var getItemsByItemIdsProcedure = new GetItemsProcedure(request);
                return getItemsByItemIdsProcedure.Execute();
            }
        }
    }
}
