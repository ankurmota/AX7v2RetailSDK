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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// The SQL server data request handler for products.
        /// </summary>
        public class ProductsSqlServerDataService : IRequestHandler
        {
            // Function names.
            private const string GetCatalogIdsAssociatedToProductsFunctionName = "GETCATALOGIDSASSOCIATEDTOPRODUCTS(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + ProductIdsVariableName + ")";
            private const string GetProductAttributeValueCombinationsFunctionName = "GETPRODUCTATTRIBUTEVALUECOMBINATIONS(" + DatabaseAccessor.ChannelIdVariableName + ", " + CatalogIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + LocaleVariableName + ", " + ProductIdsVariableName + ", " + DummyIdVariableName + ")";
            private const string GetLinkedProductRelationsFunctionName = "GETLINKEDPRODUCTRELATIONS_V2(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + ProductIdsVariableName + ", " + DataAreaIdVariableName + ")";
            private const string GetProductBehaviorByProductIdsFunctionName = "GETPRODUCTBEHAVIORBYPRODUCTIDS_V2(" + DataAreaIdVariableName + ", " + ProductIdsVariableName + ")";
            private const string GetProductComponentsFunctionName = "GETPRODUCTCOMPONENTS(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + LocaleVariableName + ")";
            private const string GetProductDimensionValuesByVariantProductIdsFunctionName = "GETPRODUCTDIMENSIONVALUESBYVARIANTPRODUCTIDS_V2(" + DataAreaIdVariableName + ", " + LocaleVariableName + ", " + ProductIdsVariableName + ")";
            private const string GetProductsByIdsFunctionName = "GETPRODUCTSBYIDS_V2(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + LocaleVariableName + ", " + ProductIdsVariableName + ", " + DefaultLocaleVariableName + ", " + DataAreaIdVariableName + ")";
            private const string GetProductMediaBlobsFunctionName = "GETPRODUCTMEDIABLOBS(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.CatalogIdVariableName + ", " + ProductIdVariableName + ", " + LocaleVariableName + ")";
            private const string GetProductMediaLocationsFunctionName = "GETPRODUCTMEDIALOCATIONS(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.CatalogIdVariableName + ", " + ProductIdVariableName + ", " + LocaleVariableName + ")";
            private const string ConvertItemAndInventDimIdsFunctionName = "CONVERTITEMANDINVENTDIMIDSTOPRODUCTIDS(" + DatabaseAccessor.DataAreaIdVariableName + ", " + ItemAndInventDimIdsVariableName + ")";
            private const string GetVariantsWithDimensionValuesFunctionName = "GETVARIANTSWITHDIMENSIONVALUES(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + LocaleVariableName + ", " + MasterProductIdVariableName + ")";
            private const string GetVariantsWithComponentValuesFunctionName = "GETVARIANTSWITHCOMPONENTVALUES(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + SlotToComponentRelationshipVariableName + ")";
            private const string GetProductRelationships = "GETPRODUCTRELATIONSHIPS(" + DatabaseAccessor.ChannelIdVariableName + ", " + DatabaseAccessor.ChannelDateVariableName + ", " + LocaleVariableName + ", " + ProductIdVariableName + ")";

            // Stored procedure names.
            private const string GetRefinersByCategoryIdSprocName = "GETREFINERSBYCATEGORYID";
            private const string GetRefinersByTextSprocName = "GETREFINERSBYTEXT";
            private const string GetRefinerValuesByCategoryIdSprocName = "GETREFINERVALUESBYCATEGORYID";
            private const string GetRefinerValuesByTextSprocName = "GETREFINERVALUESBYTEXT";

            // Variable names.
            private const string CatalogIdVariableName = "@bi_CatalogId";
            private const string CategoryIdVariableName = "@bi_CategoryId";
            private const string DummyIdVariableName = "@tvp_DummyId";
            private const string ItemAndInventDimIdsVariableName = "@tvp_ItemAndInventDimIds";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string MasterProductIdVariableName = "@bi_MasterProductId";
            private const string ProductIdVariableName = "@bi_ProductId";
            private const string ProductIdsVariableName = "@tvp_ProductIds";
            private const string SlotToComponentRelationshipVariableName = "@tvp_SlotToComponentRelationship";
            private const string RefinerIdVariableName = "@bi_RefinerId";
            private const string RefinerSourceVariableName = "@i_RefinerSource";
            private const string SearchTextVariableName = "@nvc_SearchText";
            private const string DefaultLocaleVariableName = "@nvc_DefaultLocale";
            private const string DataAreaIdVariableName = "@nvc_DataAreaId";

            // Data Request Types supported.
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetCatalogsAssociatedToProductsDataRequest),
                typeof(GetProductAttributeValuesDataRequest),
                typeof(GetLinkedProductRelationsDataRequest),
                typeof(GetProductsDataRequest),
                typeof(GetProductBehaviorDataRequest),
                typeof(GetProductComponentsDataRequest),
                typeof(GetProductComponentsForVariantProductsDataRequest),
                typeof(GetProductDimensionValuesDataRequest),
                typeof(GetProductDimensionValuesForVariantProductsDataRequest),
                typeof(GetProductRefinersByCategoryDataRequest),
                typeof(GetProductRefinersByTextDataRequest),
                typeof(GetProductRefinerValuesByCategoryDataRequest),
                typeof(GetProductRefinerValuesByTextDataRequest),
                typeof(GetVariantProductIdsDataRequest),
                typeof(GetProductRelationTypesDataRequest),
                typeof(GetRelatedProductsDataRequest),
                typeof(GetProductMediaBlobsDataRequest),
                typeof(GetProductMediaLocationsDataRequest),
            };
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
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
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetCatalogsAssociatedToProductsDataRequest))
                {
                    response = ProcessGetCatalogsAssociatedToProductsDataRequest((GetCatalogsAssociatedToProductsDataRequest)request);
                }
                else if (requestType == typeof(GetLinkedProductRelationsDataRequest))
                {
                    response = ProcessGetLinkedProductRelationsDataRequest((GetLinkedProductRelationsDataRequest)request);
                }
                else if (requestType == typeof(GetProductAttributeValuesDataRequest))
                {
                    response = ProcessGetProductAttributeValuesDataRequest((GetProductAttributeValuesDataRequest)request);
                }
                else if (requestType == typeof(GetProductsDataRequest))
                {
                    response = ProcessGetProductsDataRequest((GetProductsDataRequest)request);
                }
                else if (requestType == typeof(GetProductBehaviorDataRequest))
                {
                    response = ProcessGetProductBehaviorDataRequest((GetProductBehaviorDataRequest)request);
                }
                else if (requestType == typeof(GetProductComponentsDataRequest))
                {
                    response = ProcessGetProductComponentsDataRequest((GetProductComponentsDataRequest)request);
                }
                else if (requestType == typeof(GetProductComponentsForVariantProductsDataRequest))
                {
                    response = ProcessGetProductComponentsForVariantProductsDataRequest((GetProductComponentsForVariantProductsDataRequest)request);
                }
                else if (requestType == typeof(GetProductDimensionValuesDataRequest))
                {
                    response = ProcessGetProductDimensionValuesDataRequest((GetProductDimensionValuesDataRequest)request);
                }
                else if (requestType == typeof(GetProductDimensionValuesForVariantProductsDataRequest))
                {
                    response = ProcessGetProductDimensionValuesForVariantProductsDataRequest((GetProductDimensionValuesForVariantProductsDataRequest)request);
                }
                else if (requestType == typeof(GetProductRefinersByCategoryDataRequest))
                {
                    response = ProcessGetProductRefinersByCategoryDataRequest((GetProductRefinersByCategoryDataRequest)request);
                }
                else if (requestType == typeof(GetProductRefinersByTextDataRequest))
                {
                    response = ProcessGetProductRefinersByTextDataRequest((GetProductRefinersByTextDataRequest)request);
                }
                else if (requestType == typeof(GetProductRefinerValuesByCategoryDataRequest))
                {
                    response = ProcessGetProductRefinerValuesByCategoryDataRequest((GetProductRefinerValuesByCategoryDataRequest)request);
                }
                else if (requestType == typeof(GetProductRefinerValuesByTextDataRequest))
                {
                    response = ProcessGetProductRefinerValuesByTextDataRequest((GetProductRefinerValuesByTextDataRequest)request);
                }
                else if (requestType == typeof(GetVariantProductIdsDataRequest))
                {
                    response = ProcessGetVariantProductIdsDataRequest((GetVariantProductIdsDataRequest)request);
                }
                else if (requestType == typeof(GetProductRelationTypesDataRequest))
                {
                    response = ProcessGetProductRelationshipsDataRequest((GetProductRelationTypesDataRequest)request);
                }
                else if (requestType == typeof(GetRelatedProductsDataRequest))
                {
                    response = ProcessGetRelatedProductsDataRequest((GetRelatedProductsDataRequest)request);
                }
                else if (requestType == typeof(GetProductMediaLocationsDataRequest))
                {
                    response = ProcessGetProductMediaLocationsRequest((GetProductMediaLocationsDataRequest)request);
                }
                else if (requestType == typeof(GetProductMediaBlobsDataRequest))
                {
                    response = ProcessGetProductMediaBlobsRequest((GetProductMediaBlobsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<ProductCatalog> ProcessGetCatalogsAssociatedToProductsDataRequest(GetCatalogsAssociatedToProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (!request.ProductIds.Any())
                {
                    return new EntityDataServiceResponse<ProductCatalog>(new List<ProductCatalog>().AsPagedResult());
                }

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetCatalogIdsAssociatedToProductsFunctionName,
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (RecordIdTableType productIds = new RecordIdTableType(request.ProductIds, ProductIdsVariableName))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[ProductIdsVariableName] = productIds.DataTable;
    
                    return new EntityDataServiceResponse<ProductCatalog>(databaseContext.ReadEntity<ProductCatalog>(query));
                }
            }

            private static EntityDataServiceResponse<AttributeValue> ProcessGetProductAttributeValuesDataRequest(GetProductAttributeValuesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductAttributeValueCombinationsFunctionName,
                    OrderBy = "NAME",
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };

                using (RecordIdTableType productIds = new RecordIdTableType(new List<long>() { request.ProductId }, ProductIdsVariableName))
                using (RecordIdTableType dummyId = new RecordIdTableType(new List<long>() { 0 }, DummyIdVariableName))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[CatalogIdVariableName] = request.CatalogId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdsVariableName] = productIds.DataTable;
                    query.Parameters[DummyIdVariableName] = dummyId.DataTable;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        return new EntityDataServiceResponse<AttributeValue>(databaseContext.ReadEntity<AttributeValue>(query));
                    }
                }
            }

            private static EntityDataServiceResponse<LinkedProductRelation> ProcessGetLinkedProductRelationsDataRequest(GetLinkedProductRelationsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (!request.ProductIds.Any())
                {
                    return new EntityDataServiceResponse<LinkedProductRelation>(new PagedResult<LinkedProductRelation>(new List<LinkedProductRelation>().AsReadOnly()));
                }

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetLinkedProductRelationsFunctionName,
                    OrderBy = "PRODUCTID"
                };

                if (request.DownloadedProductsFilter.HasValue)
                {
                    query.Where += "ISREMOTE = " + (request.DownloadedProductsFilter.Value ? "1" : "0");
                }
    
                using (RecordIdTableType type = new RecordIdTableType(request.ProductIds, ProductIdsVariableName))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[ProductIdsVariableName] = type.DataTable;
                    query.Parameters[DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        return new EntityDataServiceResponse<LinkedProductRelation>(databaseContext.ReadEntity<LinkedProductRelation>(query));
                    }
                }
            }
    
            private static EntityDataServiceResponse<SimpleProduct> ProcessGetProductsDataRequest(GetProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ProductIds != null && request.ProductIds.Any() && request.ItemAndInventDimIdCombinations != null && request.ItemAndInventDimIdCombinations.Any())
                {
                    throw new ArgumentOutOfRangeException("request", "The GetProductsDataRequest cannot be processed when both product ids and item-inventdim ids are specified. Please specify only one.");
                }
    
                if (request.ProductIds.IsNullOrEmpty() && request.ItemAndInventDimIdCombinations.IsNullOrEmpty())
                {
                    return new EntityDataServiceResponse<SimpleProduct>(new List<SimpleProduct>().AsPagedResult());
                }
    
                if (!request.ProductIds.IsNullOrEmpty())
                {
                    return new EntityDataServiceResponse<SimpleProduct>(GetProductsByProductIds(request));
                }
    
                return new EntityDataServiceResponse<SimpleProduct>(GetProductsByItemAndInventDimIds(request));
            }
    
            private static EntityDataServiceResponse<ProductBehavior> ProcessGetProductBehaviorDataRequest(GetProductBehaviorDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                if (!request.ProductIds.Any())
                {
                    return new EntityDataServiceResponse<ProductBehavior>(new PagedResult<ProductBehavior>(new List<ProductBehavior>().AsReadOnly()));
                }
                
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductBehaviorByProductIdsFunctionName
                };

                using (RecordIdTableType type = new RecordIdTableType(request.ProductIds, ProductIdsVariableName))
                {
                    query.Parameters[DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    query.Parameters[ProductIdsVariableName] = type.DataTable;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        return new EntityDataServiceResponse<ProductBehavior>(databaseContext.ReadEntity<ProductBehavior>(query));
                    }
                }
            }
    
            private static EntityDataServiceResponse<ProductDimensionValue> ProcessGetProductDimensionValuesDataRequest(GetProductDimensionValuesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Channel identifier cannot be less than zero.");
                }
    
                if (request.RequestedDimension == ProductDimensionType.None)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Requested dimension to retrieve cannot be none. Please select a valid dimension to retrieve.");
                }
    
                var requiredColumns = GetColumnSetForDimension(request.RequestedDimension);
                requiredColumns.Add(((int)request.RequestedDimension).ToString() + " AS DIMENSION");
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Select = requiredColumns,
                    Distinct = true,
                    From = GetVariantsWithDimensionValuesFunctionName,
                    OrderBy = "DISPLAYORDER",
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[MasterProductIdVariableName] = request.MasterProductId;
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new EntityDataServiceResponse<ProductDimensionValue>(databaseContext.ReadEntity<ProductDimensionValue>(query));
                }
            }
    
            private static EntityDataServiceResponse<ProductDimensionValue> ProcessGetProductDimensionValuesForVariantProductsDataRequest(GetProductDimensionValuesForVariantProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Distinct = true,
                    From = GetProductDimensionValuesByVariantProductIdsFunctionName
                };

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (RecordIdTableType type = new RecordIdTableType(request.ProductIds, ProductIdsVariableName))
                {
                    query.Parameters[DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdsVariableName] = type.DataTable;
    
                    return new EntityDataServiceResponse<ProductDimensionValue>(databaseContext.ReadEntity<ProductDimensionValue>(query));
                }
            }
    
            private static SingleEntityDataServiceResponse<long> ProcessGetVariantProductIdsDataRequest(GetVariantProductIdsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Channel identifier cannot be less than zero.");
                }
    
                long variantId;
    
                if (!request.MatchingDimensionValues.IsNullOrEmpty())
                {
                    variantId = GetVariantProductIdsUsingDimensionValues(request);
                }
                else if (!request.MatchingSlotToComponentRelations.IsNullOrEmpty())
                {
                    variantId = GetVariantProductIdsUsingSlotToComponentRelationship(request);
                }
                else if (!request.MatchingDimensionValues.IsNullOrEmpty() && !request.MatchingSlotToComponentRelations.IsNullOrEmpty())
                {
                    throw new NotSupportedException("Retrieving variant id(s) when both dimension value(s) and component(s) are specified is not supported.");
                }
                else
                {
                    throw new InvalidOperationException("Please specify either of dimension value(s) or component(s) to retrieve variant product ids.");
                }
    
                return new SingleEntityDataServiceResponse<long>(variantId);
            }
    
            private static EntityDataServiceResponse<ProductComponent> ProcessGetProductComponentsDataRequest(GetProductComponentsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new ArgumentOutOfRangeException("request", request.ChannelId, "Channel identifier cannot be less than zero.");
                }
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Distinct = true,
                    From = GetProductComponentsFunctionName
                };
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                query.Where = "ISREMOTE = " + ((currentChannelId != request.ChannelId) ? "1" : "0");
                query.Where += " AND MASTERPRODUCTID = @bi_ProductId";
                query.Where += request.ShouldRetrieveOnlyDefaultComponents ? " AND ISDEFAULTCOMPONENT = 1" : string.Empty;
                query.Where += request.SlotIds.IsNullOrEmpty() ? string.Empty : " AND SLOTID = @bi_SlotId";

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdVariableName] = request.ProductId;
    
                    if (!request.SlotIds.IsNullOrEmpty())
                    {
                        query.Parameters["@bi_SlotId"] = request.SlotIds.First();
                    }
    
                    return new EntityDataServiceResponse<ProductComponent>(databaseContext.ReadEntity<ProductComponent>(query));
                }
            }
    
            private static EntityDataServiceResponse<ProductComponent> ProcessGetProductComponentsForVariantProductsDataRequest(GetProductComponentsForVariantProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Distinct = true,
                    Aliased = true,
                    From = GetProductComponentsFunctionName
                };

                if (request.DownloadedProductsFilter.HasValue)
                {
                    query.Where += "ISREMOTE = " + (request.DownloadedProductsFilter.Value ? "1" : "0");
                }

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (RecordIdTableType productIds = new RecordIdTableType(request.ProductIds, "VARIANTPRODUCTID"))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdsVariableName] = productIds;
    
                    return new EntityDataServiceResponse<ProductComponent>(databaseContext.ReadEntity<ProductComponent>(query));
                }
            }
    
            private static EntityDataServiceResponse<ProductRelationType> ProcessGetProductRelationshipsDataRequest(GetProductRelationTypesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Channel identifier cannot be less than zero.");
                }

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                var requiredColumns = new string[]
                {
                    "RELATIONNAME",
                    "RELATIONTYPEID"
                };
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Select = new ColumnSet(requiredColumns),
                    Distinct = true,
                    From = GetProductRelationships,
                    OrderBy = "RELATIONNAME",
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[ProductIdVariableName] = request.ProductId;
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new EntityDataServiceResponse<ProductRelationType>(databaseContext.ReadEntity<ProductRelationType>(query));
                }
            }
    
            private static EntityDataServiceResponse<ProductRefiner> ProcessGetProductRefinersByCategoryDataRequest(GetProductRefinersByCategoryDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                parameters[CategoryIdVariableName] = request.CategoryId;
    
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    var refiners = sqlServerDatabaseContext.ExecuteStoredProcedure<ProductRefiner>(GetRefinersByCategoryIdSprocName, parameters);
    
                    return new EntityDataServiceResponse<ProductRefiner>(refiners);
                }
            }
    
            private static EntityDataServiceResponse<ProductRefiner> ProcessGetProductRefinersByTextDataRequest(GetProductRefinersByTextDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                parameters[SearchTextVariableName] = FreeTextSearchFormatter.FormatFuzzySearch(request.SearchText);
    
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    var refiners = sqlServerDatabaseContext.ExecuteStoredProcedure<ProductRefiner>(GetRefinersByTextSprocName, parameters);
    
                    return new EntityDataServiceResponse<ProductRefiner>(refiners);
                }
            }
    
            private static EntityDataServiceResponse<ProductRefinerValue> ProcessGetProductRefinerValuesByCategoryDataRequest(GetProductRefinerValuesByCategoryDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                parameters[CategoryIdVariableName] = request.CategoryId;
                parameters[RefinerIdVariableName] = request.RefinerId;
                parameters[RefinerSourceVariableName] = request.RefinerSource;
    
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    var refiners = sqlServerDatabaseContext.ExecuteStoredProcedure<ProductRefinerValue>(GetRefinerValuesByCategoryIdSprocName, parameters);
    
                    return new EntityDataServiceResponse<ProductRefinerValue>(refiners);
                }
            }
    
            private static EntityDataServiceResponse<ProductRefinerValue> ProcessGetProductRefinerValuesByTextDataRequest(GetProductRefinerValuesByTextDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                parameters[SearchTextVariableName] = FreeTextSearchFormatter.FormatFuzzySearch(request.SearchText);
                parameters[RefinerIdVariableName] = request.RefinerId;
                parameters[RefinerSourceVariableName] = request.RefinerSource;
    
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                {
                    var refiners = sqlServerDatabaseContext.ExecuteStoredProcedure<ProductRefinerValue>(GetRefinerValuesByTextSprocName, parameters);
    
                    return new EntityDataServiceResponse<ProductRefinerValue>(refiners);
                }
            }
    
            private static EntityDataServiceResponse<ProductSearchResult> ProcessGetRelatedProductsDataRequest(GetRelatedProductsDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");
    
                if (request.ChannelId < 0)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Channel identifier cannot be less than zero.");
                }

                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductRelationships,
                    OrderBy = "NAME",
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };

                query.Where += string.Format(" AND RELATIONTYPEID = '{0}'", request.RelationTypeId);
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[ProductIdVariableName] = request.ProductId;
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new EntityDataServiceResponse<ProductSearchResult>(databaseContext.ReadEntity<ProductSearchResult>(query));
                }
            }
    
            private static EntityDataServiceResponse<MediaLocation> ProcessGetProductMediaLocationsRequest(GetProductMediaLocationsDataRequest request)
            {
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductMediaLocationsFunctionName
                };
    
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = request.ChannelId;
                query.Parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                query.Parameters[ProductIdVariableName] = request.ProductId;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var results = databaseContext.ReadEntity<MediaLocation>(query);
                    return new EntityDataServiceResponse<MediaLocation>(results);
                }
            }
    
            private static EntityDataServiceResponse<MediaBlob> ProcessGetProductMediaBlobsRequest(GetProductMediaBlobsDataRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductMediaBlobsFunctionName,
                    OrderBy = "ISDEFAULT DESC, CONTENT"
                };
    
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = request.ChannelId;
                query.Parameters[DatabaseAccessor.CatalogIdVariableName] = request.CatalogId;
                query.Parameters[ProductIdVariableName] = request.ProductId;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
    
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    var results = databaseContext.ReadEntity<MediaBlob>(query);
                    return new EntityDataServiceResponse<MediaBlob>(results);
                }
            }
    
            private static PagedResult<SimpleProduct> GetProductsByProductIds(GetProductsDataRequest request)
            {
                ChannelConfiguration channelConfiguration = request.RequestContext.GetChannelConfiguration();
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;
                string dataAreaId = channelConfiguration.InventLocationDataAreaId;

                // this is the default language configured in the channel and is used as a fallback
                // language for translations, in case product translations are not available in the
                // language referenced by request.RequestContext.LanguageId, used below
                string defaultLocale = channelConfiguration.DefaultLanguageId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetProductsByIdsFunctionName
                };

                if (request.DownloadedProductsFilter.HasValue)
                {
                    query.Where += "ISREMOTE = " + (request.DownloadedProductsFilter.Value ? "1" : "0");
                }

                using (RecordIdTableType type = new RecordIdTableType(request.ProductIds, ProductIdsVariableName))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdsVariableName] = type.DataTable;
                    query.Parameters[DataAreaIdVariableName] = dataAreaId;
                    query.Parameters[DefaultLocaleVariableName] = defaultLocale;

                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        return databaseContext.ReadEntity<SimpleProduct>(query);
                    }
                }
            }
    
            private static PagedResult<SimpleProduct> GetProductsByItemAndInventDimIds(GetProductsDataRequest request)
            {
                IEnumerable<long> productIds;
    
                // Do not page this function call because it does not adhere to assortment rules
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = ConvertItemAndInventDimIdsFunctionName
                };
    
                using (ItemIdSearchTableType type = new ItemIdSearchTableType(request.ItemAndInventDimIdCombinations))
                {
                    query.Parameters[DatabaseAccessor.DataAreaIdVariableName] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;
                    query.Parameters[ItemAndInventDimIdsVariableName] = type.DataTable;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        productIds = databaseContext.ExecuteScalarCollection<long>(query);
                    }
                }
    
                var getProductsByRecordIdsRequest = new GetProductsDataRequest(productIds, request.QueryResultSettings, request.DownloadedProductsFilter);
                var getProductsByRecordIdsResponse = request.RequestContext.Execute<EntityDataServiceResponse<SimpleProduct>>(getProductsByRecordIdsRequest);
    
                return getProductsByRecordIdsResponse.PagedEntityCollection;
            }
    
            private static long GetVariantProductIdsUsingDimensionValues(GetVariantProductIdsDataRequest request)
            {
                var requiredColumns = new List<string>();
                requiredColumns.Add("RECORDID");
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Select = new ColumnSet(requiredColumns.ToArray()),
                    Distinct = true,
                    From = GetVariantsWithDimensionValuesFunctionName,
                    OrderBy = "RECORDID",
                    Where = "ISREMOTE = " + (currentChannelId != request.ChannelId ? "1" : "0")
                };

                query.Where += GetWhereClauseFromDimensionValues(request.MatchingDimensionValues);
                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[MasterProductIdVariableName] = request.MasterProductId;
    
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    return databaseContext.ExecuteScalar<long>(query);
                }
            }

            private static long GetVariantProductIdsUsingSlotToComponentRelationship(GetVariantProductIdsDataRequest request)
            {
                long currentChannelId = request.RequestContext.GetPrincipal().ChannelId;

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    Select = new ColumnSet(new[] { "PRODUCTID" }),
                    From = GetVariantsWithComponentValuesFunctionName,
                    OrderBy = "PRODUCTID"
                };

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                using (RecordIdsTableType matchingSlotToComponentRelations = new RecordIdsTableType(request.MatchingSlotToComponentRelations))
                {
                    query.Parameters[DatabaseAccessor.ChannelIdVariableName] = currentChannelId;
                    query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().DateTime;
                    query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                    query.Parameters[ProductIdVariableName] = request.MasterProductId;
                    query.Parameters[SlotToComponentRelationshipVariableName] = matchingSlotToComponentRelations.DataTable;
    
                    return databaseContext.ExecuteScalar<long>(query);
                }
            }
    
            private static ColumnSet GetColumnSetForDimension(ProductDimensionType dimension)
            {
                var requiredColumns = new List<string>();
    
                if (dimension == ProductDimensionType.Color)
                {
                    requiredColumns.Add("COLOR AS VALUE");
                    requiredColumns.Add("COLORDISPLAYORDER AS DISPLAYORDER");
                }
                else if (dimension == ProductDimensionType.Configuration)
                {
                    requiredColumns.Add("CONFIGURATION AS VALUE");
                    requiredColumns.Add("CONFIGURATIONDISPLAYORDER AS DISPLAYORDER");
                }
                else if (dimension == ProductDimensionType.Size)
                {
                    requiredColumns.Add("SIZE AS VALUE");
                    requiredColumns.Add("SIZEDISPLAYORDER AS DISPLAYORDER");
                }
                else if (dimension == ProductDimensionType.Style)
                {
                    requiredColumns.Add("STYLE AS VALUE");
                    requiredColumns.Add("STYLEDISPLAYORDER AS DISPLAYORDER");
                }
    
                return new ColumnSet(requiredColumns.ToArray());
            }
    
            private static string GetWhereClauseFromDimensionValues(IEnumerable<ProductDimension> dimensionValues)
            {
                string whereClause = string.Empty;
    
                foreach (var dimensionValue in dimensionValues)
                {
                    whereClause += " AND";
                    if (dimensionValue.DimensionType == ProductDimensionType.Color)
                    {
                        whereClause += " COLOR = ";
                    }
                    else if (dimensionValue.DimensionType == ProductDimensionType.Configuration)
                    {
                        whereClause += " CONFIGURATION = ";
                    }
                    else if (dimensionValue.DimensionType == ProductDimensionType.Size)
                    {
                        whereClause += " SIZE = ";
                    }
                    else if (dimensionValue.DimensionType == ProductDimensionType.Style)
                    {
                        whereClause += " STYLE = ";
                    }
                    else if (dimensionValue.DimensionType == ProductDimensionType.None)
                    {
                        throw new ArgumentOutOfRangeException("dimensionValues", dimensionValue.DimensionType, "The type of dimension value to be matched cannot be null.");
                    }
    
                    whereClause += "'" + dimensionValue.DimensionValue.Value + "'";
                }
    
                return whereClause;
            }
        }
    }
}
