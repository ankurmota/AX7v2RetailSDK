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
        using System.Collections.ObjectModel;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Data service for products.
        /// </summary>
        public sealed class ProductSqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Indicates the maximum number of entries of a collection which can be cached.
            /// </summary>
            private const int MaxCachedCollectionSize = 500;
    
            private const string VerifyProductsExistenceSProcName = "VERIFYPRODUCTSEXISTENCE";
            private const string GetProductsSprocName = "GETPRODUCTS";
            private const string GetProductsByCategorySprocName = "GETPRODUCTSBYCATEGORY";
            private const string GetProductsByKeywordSprocName = "GETPRODUCTSBYKEYWORD";
            private const string GetProductsByItemIdSprocName = "GETPRODUCTSBYITEMID";
            private const string GetLinkedProductsSprocName = "GETLINKEDPRODUCTS";
            private const string InsertPublishStatusForCurrentListingPageSprocName = "INSERTPUBLISHSTATUSFORCURRENTLISTINGPAGE";
            private const string ProductCatalogAssociationsSprocName = "GETPRODUCTCATALOGS";
            private const string ProductChangeTrackingAnchorSetTableTypeName = "PRODUCTCHANGETRACKINGANCHORSET";
            private const string CreateMissingProductPricesSprocName = "CREATEMISSINGPRODUCTPRICES";
            private const string GetChangedProductIdsSprocName = "GETCHANGEDPRODUCTIDS";
            private const string GetUnitOfMeasureOptionsSprocName = "GETUNITOFMEASUREOPTIONS";
    
            private const string GetKitListingFuncName = "GETRETAILKITLISTING(@bi_ChannelId, @dt_ChannelDate, @tvp_KitProductMasterIds)";
            private const string GetKitComponentAndSubstituteFuncName = "GETRETAILKITLINEDEFINITION(@bi_ChannelId, @dt_ChannelDate, @tvp_KitProductMasterIds)";
            private const string GetKitConfigurationToProductMapFuncName = "GETRETAILKITVARIANTMAP(@bi_ChannelId, @dt_ChannelDate, @tvp_ProductIds)";
            private const string GetKitComponentsInfoSprocName = "GETKITCOMPONENTSINFO";
            private const string KitProductMasterColumnName = "KITPRODUCTMASTERLISTING";
    
            private const string GetNextBatchListingPublishStatusesFunctionName = "GETNEXTBATCHLISTINGPUBLISHSTATUSES(@i_ActionStatus, @nvc_LastChannelBatchId)";
            private const string GetPublishedCatalogsToActiveChannelFunctionName = "GETCATALOGSPUBLISHEDTOACTIVECHANNEL(@bi_ChannelId, @b_LoadMediaServerImage)";
    
            private const string GetRefinerValuesForKeywordSearchSproc = "GETREFINERVALUESFORKEYWORDSEARCH";
            private const string GetRefinerValuesForCategorySearchSproc = "GETREFINERVALUESFORCATEGORYSEARCH";
    
            private const string LoadMediaServerImageParameterName = "@b_LoadMediaServerImage";
            private const string CatalogIdVariableName = "@bi_CatalogId";
            private const string DataLevelVariableName = "@i_DataLevel";
            private const string IsForwardLookingVariableName = "@b_IsForwardLooking";
            private const string SkipVariantExpansionVariableName = "@b_SkipVariantExpansion";
            private const string ProductIdsVariableName = "@tvp_ProductIds";
            private const string RefinerValuesVariableName = "@tvp_RefinerValues";
            private const string IdsVariableName = "@tvp_Ids";
            private const string IncludeProductsFromDescendantCategoriesVariableName = "@b_IncludeProductsFromDescendantCategories";
            private const string CategoryIdsVariableName = "@tvp_CategoryIds";
            private const string SearchConditionVariableName = "@nvc_SearchCondition";
            private const string LanguageIdVariableName = "@nvc_LanguageId";
            private const string ItemIdsVariableName = "@tvp_ItemIds";
            private const string WatermarkVariableName = "@tvp_Watermark";
            private const string KitComponentProductIdsVariableName = "@tvp_KitComponentProductListings";
            private const string IsOnlineSearchName = "@b_IsOnlineSearch";
            private const string DataAreaIdVariableName = "@nvc_DataAreaId";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string SearchConditionParametrizationString = @"""{0}*""";
    
            private const string RefinementByProductIdsExceptionMessage = "Refine products by product ids is not supported.";
            private const string RefinementByItemIdsExceptionMessage = "Refine products by item ids is not supported.";
    
            private const string ListingPublishStatusTableTypeName = "LISTINGPUBLISHSTATUSTABLETYPE";
            private const string ActionStatusColumnName = "ACTIONSTATUS";
            private const string ListingModifiedDateTimeColumnName = "LISTINGMODIFIEDDATETIME";
            private const string ListingModifiedDateTimeZoneIdColumnName = "LISTINGMODIFIEDDATETIMETZID";
            private const string ChannelColumnName = "CHANNEL";
            private const string ChannelListingIdColumnName = "CHANNELLISTINGID";
            private const string ChannelBatchIdColumnName = "CHANNELBATCHID";
            private const string ChannelStateColumnName = "CHANNELSTATE";
            private const string AppliedActionColumnName = "APPLIEDACTION";
            private const string StatusDateTimeColumnName = "STATUSDATETIME";
            private const string StatusDateTimeZoneIdColumnName = "STATUSDATETIMETZID";
            private const string StatusMessageColumnName = "STATUSMESSAGE";
            private const string ProcessedColumnName = "PROCESSED";
            private const string CatalogColumnName = "CATALOG";
            private const string ProductColumnName = "PRODUCT";
            private const string LanguageIdColumnName = "LANGUAGEID";
            private const string ChannelColumn = "CHANNELID";
            private const string ERPColumn = "ECORESPRODUCT";
            private const string ERPIVColumn = "ECORESPRODUCTINSTANCEVALUE";
            private const string ERIVColumn = "ECORESINSTANCEVALUE";
            private const string ERAColumn = "ECORESATTRIBUTE";
            private const string ERAVColumn = "ECORESATTRIBUTEVALUE";
            private const string ERVColumn = "ECORESVALUE";
            private const string ERBVColumn = "ECORESBOOLEANVALUE";
            private const string ERDTVColumn = "ECORESDATETIMEVALUE";
            private const string ERCVColumn = "ECORESCURRENCYVALUE";
            private const string ERFVColumn = "ECORESFLOATVALUE";
            private const string ERNVColumn = "ECORESINTVALUE";
            private const string ERRVColumn = "ECORESREFERENCEVALUE";
            private const string ERTVColumn = "ECORESTEXTVALUE";
            private const string ERTVTColumn = "ECORESTEXTVALUETRANSLATION";
            private const string ERPTColumn = "ECORESPRODUCTTRANSLATION";
            private const string ERPVColColumn = "ECORESPRODUCTVARIANTCOLOR";
            private const string ERPVConColumn = "ECORESPRODUCTVARIANTCONFIGURATION";
            private const string ERPVDVColumn = "ECORESPRODUCTVARIANTDIMENSIONVALUE";
            private const string ERPVSzColumn = "ECORESPRODUCTVARIANTSIZE";
            private const string ERPVStColumn = "ECORESPRODUCTVARIANTSTYLE";
            private const string ERColColumn = "ECORESCOLOR";
            private const string ERConColumn = "ECORESCONFIGURATION";
            private const string ERSzColumn = "ECORESSIZE";
            private const string ERStColumn = "ECORESSTYLE";
            private const string ERAGAColumn = "ECORESATTRIBUTEGROUPATTRIBUTE";
            private const string ERCAGColumn = "ECORESCATEGORYATTRIBUTEGROUP";
            private const string ERCALColumn = "ECORESCATEGORYATTRIBUTELOOKUP";
            private const string ERDPVColumn = "ECORESDISTINCTPRODUCTVARIANT";
            private const string ERPCColumn = "ECORESPRODUCTCATEGORY";
            private const string RCCLColumn = "RETAILCATEGORYCONTAINMENTLOOKUP";
            private const string RPCPColumn = "RETAILPUBCATALOGPRODUCT";
            private const string RPCPCColumn = "RETAILPUBCATALOGPRODUCTCATEGORY";
            private const string RPCPRColumn = "RETAILPUBCATALOGPRODUCTRELATION";
            private const string RPCPREColumn = "RETAILPUBCATALOGPRODUCTRELATIONEXCLUSION";
            private const string RPIOAGColumn = "RETAILPUBINTERNALORGATTRIBUTEGROUP";
            private const string RPIOIEColumn = "RETAILPUBINTORGINHERITANCEEXPLODED";
            private const string RPPACMDColumn = "RETAILPUBPRODUCTATTRIBUTECHANNELMETADATA";
            private const string RPPAVColumn = "RETAILPUBPRODUCTATTRIBUTEVALUE";
            private const string RPRCTColumn = "RETAILPUBRETAILCHANNELTABLE";
            private const string RSPColumn = "RETAILSHAREDPARAMETERS";
            private const string RSAColumn = "RETAILSTANDARDATTRIBUTE";
            private const string UOMColumn = "UNITOFMEASURE";
    
            [Flags]
            private enum ProductSearchCriteriaInputs
            {
                None = 0,
                ProductIds = 1,
                CategoryIds = 2,
                ItemIds = 4,
                Keyword = 8
            }
    
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
                        typeof(GetProductCatalogAssociationsDataRequest),
                        typeof(GetLinkedProductsDataRequest),
                        typeof(GetProductKitDataRequest),
                        typeof(GetChangedProductsDataRequest),
                        typeof(BeginReadChangedProductsDataRequest),
                        typeof(VerifyProductExistenceDataRequest),
                        typeof(GetProductRefinersDataRequest),
                        typeof(GetNextBatchListingPublishStatusesDataRequest),
                        typeof(UpdateListingStatusesDataRequest),
                        typeof(GetProductCatalogsDataRequest),
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
                ThrowIf.Null(request, "request");
    
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
                else if (request is GetProductCatalogAssociationsDataRequest)
                {
                    response = GetProductCatalogAssociations((GetProductCatalogAssociationsDataRequest)request);
                }
                else if (request is GetLinkedProductsDataRequest)
                {
                    response = GetLinkedProducts((GetLinkedProductsDataRequest)request);
                }
                else if (request is GetProductKitDataRequest)
                {
                    response = GetProductKits((GetProductKitDataRequest)request);
                }
                else if (request is GetChangedProductsDataRequest)
                {
                    response = GetChangedProducts((GetChangedProductsDataRequest)request);
                }
                else if (request is BeginReadChangedProductsDataRequest)
                {
                    response = BeginReadChangedProducts((BeginReadChangedProductsDataRequest)request);
                }
                else if (request is VerifyProductExistenceDataRequest)
                {
                    response = VerifyProductExistence((VerifyProductExistenceDataRequest)request);
                }
                else if (request is GetProductRefinersDataRequest)
                {
                    response = GetProductRefinersData((GetProductRefinersDataRequest)request);
                }
                else if (request is GetNextBatchListingPublishStatusesDataRequest)
                {
                    response = GetNextBatchListingPublishStatuses((GetNextBatchListingPublishStatusesDataRequest)request);
                }
                else if (request is UpdateListingStatusesDataRequest)
                {
                    response = UpdateListingStatuses((UpdateListingStatusesDataRequest)request);
                }
                else if (request is GetProductCatalogsDataRequest)
                {
                    response = GetProductCatalogs((GetProductCatalogsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static GetLinkedProductsDataResponse GetLinkedProducts(GetLinkedProductsDataRequest request)
            {
                ThrowIf.Null(request.ProductIds, "request.ProductIds");
    
                var productDataManager = GetProductDataManager(request.RequestContext);
                var productIds = request.ProductIds;
    
                if (!productIds.Any())
                {
                    return new GetLinkedProductsDataResponse(new ReadOnlyCollection<LinkedProduct>(new List<LinkedProduct>()));
                }
    
                ProductL2CacheDataStoreAccessor level2CacheDataAccessor = (ProductL2CacheDataStoreAccessor)productDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<LinkedProduct> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetLinkedProducts(productIds), out found, out updateL2Cache);
    
                if (!found)
                {
                    ParameterSet parameters = new ParameterSet();
    
                    using (RecordIdTableType type = new RecordIdTableType(productIds))
                    {
                        parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                        parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().Date;
                        parameters[ProductIdsVariableName] = type.DataTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                        {
                            result = databaseContext.ExecuteStoredProcedure<LinkedProduct>(GetLinkedProductsSprocName, parameters).Results;
                        }
                    }
    
                    updateL2Cache &= result != null
                                     && result.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutLinkedProducts(productIds, result.AsReadOnly());
                }
    
                return new GetLinkedProductsDataResponse(result);
            }
    
            private static GetProductCatalogAssociationsDataResponse GetProductCatalogAssociations(GetProductCatalogAssociationsDataRequest request)
            {
                RequestContext context = request.RequestContext;
                var productIds = request.ProductIds;
                QueryResultSettings settings = request.QueryResultSettings;
                ThrowIf.Null(context, "context");
                ThrowIf.Null(productIds, "productIds");
                ThrowIf.Null(settings, "settings");
    
                if (!productIds.Any())
                {
                    return new GetProductCatalogAssociationsDataResponse(new List<ProductCatalogAssociation>(0).AsReadOnly());
                }
    
                ProductDataManager productDataManager = GetProductDataManager(context);
                ProductL2CacheDataStoreAccessor level2CacheDataAccessor = (ProductL2CacheDataStoreAccessor)productDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<ProductCatalogAssociation> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetProductCatalogAssociations(productIds, settings), out found, out updateL2Cache);
    
                if (!found)
                {
                    using (RecordIdTableType type = new RecordIdTableType(productIds))
                    {
                        DataTable listingIdTable = type.DataTable;
    
                        // Important: the underlying SQL objects assume that the date passed in is, indeed,
                        // the channel's current date time. Therefore if changing the meaning of this parameter,
                        // reevaluate the SQL objects (fn or views) called here and update any references to
                        // GetUTCDate() to match the new meaning of the parameter. Ideally, when the CRT will
                        // expose this parameter as a client API parameter, the db accessor will pass in 2
                        // date parameters: one with the date translated into the channel's time zone, and the
                        // corresponding UTC date.
                        ParameterSet parameters = new ParameterSet();
                        parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                        parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                        parameters[ProductIdsVariableName] = listingIdTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                        {
                            result = databaseContext.ExecuteStoredProcedure<ProductCatalogAssociation>(ProductCatalogAssociationsSprocName, parameters).Results;
                        }
    
                        updateL2Cache &= result != null
                                         && result.Count < MaxCachedCollectionSize;
                    }
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutProductCatalogAssociations(productIds, settings, result);
                }
    
                return new GetProductCatalogAssociationsDataResponse(result);
            }
    
            private static GetParentKitDataResponse GetParentProductKit(GetParentKitDataRequest request)
            {
                var kitComponentProductIds = request.ProductIds;
                RequestContext context = request.RequestContext;
                ThrowIf.Null(kitComponentProductIds, "kitComponentProductIds");
                ThrowIf.Null(context, "context");
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                PagedResult<KitComponent> result;
                using (RecordIdTableType kitComponentProductIdsTable = new RecordIdTableType(kitComponentProductIds))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                    parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                    parameters[KitComponentProductIdsVariableName] = kitComponentProductIdsTable.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = databaseContext.ExecuteStoredProcedure<KitComponent>(GetKitComponentsInfoSprocName, parameters);
                    }
                }
    
                processTimer.Stop();
                NetTracer.Information("** timer info: GetKitComponentsInfo completed in {0} ms.", processTimer.ElapsedMilliseconds);
    
                return new GetParentKitDataResponse(result);
            }
    
            private static GetProductUnitOfMeasureOptionsDataResponse GetProductUnitOfMeasureOptions(GetProductUnitOfMeasureOptionsDataRequest request)
            {
                var productIds = request.ProductIds;
                ThrowIf.Null(productIds, "productIds");
                if (!productIds.Any())
                {
                    return new GetProductUnitOfMeasureOptionsDataResponse(new List<UnitOfMeasureConversion>(0).AsPagedResult());
                }
    
                PagedResult<UnitOfMeasureConversion> convertibleUoMList;
                ParameterSet parameters = new ParameterSet();
    
                using (RecordIdTableType type = new RecordIdTableType(productIds))
                {
                    parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                    parameters[ProductIdsVariableName] = type.DataTable;
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext, request.QueryResultSettings))
                    {
                        convertibleUoMList = databaseContext.ExecuteStoredProcedure<UnitOfMeasureConversion>(GetUnitOfMeasureOptionsSprocName, parameters);
                    }
                }
    
                return new GetProductUnitOfMeasureOptionsDataResponse(convertibleUoMList);
            }
    
            private static GetProductPartsDataResponse GetProductsParts(GetProductPartsDataRequest request)
            {
                var result = RetrieveProductParts(request.RequestContext, request.Criteria, request.FetchProductsOnFutureDate, request.QueryResultSettings, request.IsOnline);
    
                ReadOnlyCollection<LinkedProduct> linkedProducts;
    
                if (request.Criteria.DataLevel >= CommerceEntityDataLevel.Extended)
                {
                    var productIdentities = (ReadOnlyCollection<ProductIdentity>)result.Item1;
                    var dataRequest = new GetLinkedProductsDataRequest(productIdentities.Select(i => i.LookupId));
                    dataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    linkedProducts = request.RequestContext.Runtime.Execute<GetLinkedProductsDataResponse>(dataRequest, request.RequestContext).LinkedProducts;
                }
                else
                {
                    linkedProducts = new ReadOnlyCollection<LinkedProduct>(new List<LinkedProduct>());
                }
    
                return new GetProductPartsDataResponse(
                    result.Item1,
                    result.Item2,
                    result.Item5,
                    result.Item4,
                    result.Item3,
                    result.Item6,
                    result.Item7,
                    result.Rest.Item1,
                    linkedProducts.AsReadOnly());
            }
    
            private static GetProductKitDataResponse GetProductKits(GetProductKitDataRequest request)
            {
                var kitMasterProductIds = request.KitMasterProductIds;
                ThrowIf.Null(kitMasterProductIds, "kitMasterProductIds");
    
                ReadOnlyCollection<KitDefinition> kitDefinitions;
                ReadOnlyCollection<KitComponent> componentAndSubstituteList;
                ReadOnlyCollection<KitConfigToComponentAssociation> configToComponentAssociations;
    
                if (kitMasterProductIds.Any())
                {
                    kitDefinitions = GetProductKitDefinition(request);
                    IList<long> kitProductIds = kitDefinitions.Select<KitDefinition, long>(kd => kd.ProductId).ToList();
                    componentAndSubstituteList = GetProductKitLineDefinitions(request, kitProductIds);
                    configToComponentAssociations = GetKitConfigToComponentAssociation(request, kitProductIds);
                }
                else
                {
                    kitDefinitions = new ReadOnlyCollection<KitDefinition>(new List<KitDefinition>());
                    componentAndSubstituteList = new ReadOnlyCollection<KitComponent>(new List<KitComponent>());
                    configToComponentAssociations = new ReadOnlyCollection<KitConfigToComponentAssociation>(new List<KitConfigToComponentAssociation>());
                }
    
                return new GetProductKitDataResponse(kitDefinitions, componentAndSubstituteList, configToComponentAssociations);
            }
    
            private static SingleEntityDataServiceResponse<ChangedProductsSearchResult> GetChangedProducts(GetChangedProductsDataRequest request)
            {
                ChangedProductsSearchCriteria queryCriteria = request.Criteria;
                QueryResultSettings querySettings = request.QueryResultSettings;
                ThrowIf.Null(queryCriteria, "queryCriteria");
                ThrowIf.Null(querySettings, "querySettings");
                ThrowIf.Null(queryCriteria.Session, "queryCriteria.Session");
    
                ProductDataManager productManager = GetProductDataManager(request.RequestContext);
    
                ProductL2CacheDataStoreAccessor cache = (ProductL2CacheDataStoreAccessor)productManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
                ReadOnlyCollection<CommerceEntityChangeTrackingInformation> changedProductsIds = cache.RetrieveIdsOfChangedProducts(queryCriteria.Session.Id);
    
                HashSet<long> productIds = new HashSet<long>();
                long pageSize = querySettings.Paging.Top;
    
                int startIndex = queryCriteria.Session.NumberOfProductsRead;
                long endIndex = startIndex + pageSize - 1;
                if (endIndex >= changedProductsIds.Count)
                {
                    endIndex = changedProductsIds.Count - 1;
                }
    
                for (int currentIndex = startIndex; currentIndex <= endIndex; currentIndex++)
                {
                    productIds.Add(changedProductsIds[currentIndex].RecordId);
                }
    
                queryCriteria.Session.NumberOfProductsReadInCurrentPage = productIds.Count;
                queryCriteria.Session.NumberOfProductsRead += productIds.Count;
    
                // Paging has already been performed on the input ids.
                // Hence, disabling it for the subsequent data request call.
                var resultSettings = QueryResultSettings.AllRecords;
    
                // retrieve and update the changed products
                ProductSearchResultContainer result = new ProductSearchResultContainer();
                result.DataLevel = CommerceEntityDataLevel.Complete;
                result = RetrieveAndAssembleProducts(productIds.AsReadOnly(), queryCriteria.Context, result.DataLevel, request.RequestContext, result, resultSettings, isForwardLooking: true, skipVariantExpansion: false);
    
                return new SingleEntityDataServiceResponse<ChangedProductsSearchResult>(new ChangedProductsSearchResult(result.Results));
            }
    
            private static SingleEntityDataServiceResponse<ReadChangedProductsSession> BeginReadChangedProducts(BeginReadChangedProductsDataRequest request)
            {
                RequestContext context = request.RequestContext;
                ChangedProductsSearchCriteria queryCriteria = request.Criteria;
                ThrowIf.Null(context, "context");
                ThrowIf.Null(queryCriteria, "queryCriteria");
    
                long currentMarker;
                ProductChangeTrackingAnchorSet currentWatermark;
                ProductChangeTrackingAnchorSet nextWatermark;
                ReadOnlyCollection<CommerceEntityChangeTrackingInformation> changedIds = RetrieveIdsOfChangedProducts(
                    request.RequestContext,
                    queryCriteria,
                    out currentMarker,
                    out currentWatermark,
                    out nextWatermark).Results;
    
                Guid sessionId = Guid.NewGuid();
    
                ProductDataManager productDataManager = GetProductDataManager(request.RequestContext);
                ProductL2CacheDataStoreAccessor cache = (ProductL2CacheDataStoreAccessor)productDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
                cache.PutIdsOfChangedProducts(sessionId, changedIds);
    
                char[] nextSyncToken = ProductChangeTrackingAnchorSet.GetSynchronizationTokenFromAnchorSet(nextWatermark, 0);
                ReadChangedProductsSession session = new ReadChangedProductsSession(sessionId, changedIds.Count, nextSyncToken);
                return new SingleEntityDataServiceResponse<ReadChangedProductsSession>(session);
            }
    
            private static EntityDataServiceResponse<ProductExistenceId> VerifyProductExistence(VerifyProductExistenceDataRequest request)
            {
                RequestContext context = request.RequestContext;
                ProductExistenceCriteria criteria = request.Criteria;
                ThrowIf.Null(context, "context");
                ThrowIf.Null(criteria, "criteria");
    
                ParameterSet parameters = new ParameterSet();
                parameters[DatabaseAccessor.ChannelIdVariableName] = criteria.ChannelId;
                parameters[CatalogIdVariableName] = criteria.CatalogId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
    
                using (RecordIdLanguageTableType ids = new RecordIdLanguageTableType(criteria.Ids))
                {
                    parameters[IdsVariableName] = ids.DataTable;
    
                    PagedResult<ProductExistenceId> productExistenceIds;
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, request.QueryResultSettings))
                    {
                        productExistenceIds = databaseContext.ExecuteStoredProcedure<ProductExistenceId>(VerifyProductsExistenceSProcName, parameters);
                    }
    
                    return new EntityDataServiceResponse<ProductExistenceId>(productExistenceIds);
                }
            }
    
            private static GetProductRefinersDataResponse GetProductRefinersData(GetProductRefinersDataRequest request)
            {
                RequestContext context = request.RequestContext;
                ProductSearchCriteria searchCriteria = request.SearchCriteria;
                ThrowIf.Null(context, "context");
                ThrowIf.Null(searchCriteria, "searchCriteria");
    
                ReadOnlyCollection<ProductRefiner> refiners;
                ReadOnlyCollection<ProductRefinerValue> refinerValues;
    
                ParameterSet parameters = new ParameterSet();
                var applicableCriteria = GetApplicableSearchCriteria(searchCriteria);
                string sprocName = string.Empty;
                var results = new Tuple<PagedResult<ProductRefiner>, ReadOnlyCollection<ProductRefinerValue>>(
                    new PagedResult<ProductRefiner>(new List<ProductRefiner>().AsReadOnly()),
                    new List<ProductRefinerValue>().AsReadOnly());
    
                if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.Keyword))
                {
                    sprocName = GetRefinerValuesForKeywordSearchSproc;
    
                    parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                    parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                    parameters[SearchConditionVariableName] = string.Format(CultureInfo.InvariantCulture, SearchConditionParametrizationString, searchCriteria.SearchCondition);
                    parameters[LanguageIdVariableName] = context.GetChannelConfiguration().DefaultLanguageId;
                    parameters[LocaleVariableName] = request.RequestContext.LanguageId;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        results = databaseContext.ExecuteStoredProcedure<ProductRefiner, ProductRefinerValue>(sprocName, parameters);
                    }
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.CategoryIds))
                {
                    using (var categoryIdTable = new RecordIdTableType(request.SearchCriteria.CategoryIds))
                    {
                        sprocName = GetRefinerValuesForCategorySearchSproc;
    
                        parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                        parameters[CatalogIdVariableName] = searchCriteria.Context.CatalogId.GetValueOrDefault();
                        parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                        parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                        parameters[IncludeProductsFromDescendantCategoriesVariableName] = searchCriteria.IncludeProductsFromDescendantCategories;
                        parameters[CategoryIdsVariableName] = categoryIdTable.DataTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                        {
                            results = databaseContext.ExecuteStoredProcedure<ProductRefiner, ProductRefinerValue>(sprocName, parameters);
                        }
                    }
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.ProductIds))
                {
                    throw new NotSupportedException(RefinementByProductIdsExceptionMessage);
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.ItemIds))
                {
                    throw new NotSupportedException(RefinementByItemIdsExceptionMessage);
                }
    
                refiners = results.Item1.Results;
                refinerValues = results.Item2;
    
                return new GetProductRefinersDataResponse(refiners, refinerValues);
            }
    
            private static EntityDataServiceResponse<ListingPublishStatus> GetNextBatchListingPublishStatuses(GetNextBatchListingPublishStatusesDataRequest request)
            {
                ListingPublishingActionStatus actionStatus = request.ActionStatus;
                string lastChannelBatchId = request.LastChannelBatchId;
                QueryResultSettings queryResultSettings = request.QueryResultSettings;
                ThrowIf.NullOrWhiteSpace(lastChannelBatchId, "lastChannelBatchId");
                ThrowIf.Null(queryResultSettings, "queryResultSettings");
    
                var query = new SqlPagedQuery(queryResultSettings)
                {
                    From = GetNextBatchListingPublishStatusesFunctionName,
                    OrderBy = "RETAILLISTING"
                };
    
                query.Parameters["@i_ActionStatus"] = (int)actionStatus;
                query.Parameters["@nvc_LastChannelBatchId"] = lastChannelBatchId ?? string.Empty;
    
                PagedResult<ListingPublishStatus> listingPublishStatus;
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    listingPublishStatus = databaseContext.ReadEntity<ListingPublishStatus>(query);
                }
    
                return new EntityDataServiceResponse<ListingPublishStatus>(listingPublishStatus);
            }
    
            private static NullResponse UpdateListingStatuses(UpdateListingStatusesDataRequest request)
            {
                IEnumerable<ListingPublishStatus> statusList = request.StatusList;
                ThrowIf.Null(statusList, "statusList");
    
                if (statusList.Any())
                {
                    Stopwatch processTimer = Stopwatch.StartNew();
    
                    using (DataTable listingStatusTable = new DataTable(ListingPublishStatusTableTypeName))
                    {
                        // First copy data from StatusList into the respective DataTable objects.
                        listingStatusTable.Columns.Add(new DataColumn(ActionStatusColumnName, typeof(int)));
                        listingStatusTable.Columns.Add(new DataColumn(AppliedActionColumnName, typeof(int)));
                        listingStatusTable.Columns.Add(new DataColumn(CatalogColumnName, typeof(long)));
                        listingStatusTable.Columns.Add(new DataColumn(ChannelColumnName, typeof(long)));
                        listingStatusTable.Columns.Add(new DataColumn(ChannelListingIdColumnName, typeof(string)));
                        listingStatusTable.Columns.Add(new DataColumn(ListingModifiedDateTimeColumnName, typeof(DateTime)));
                        listingStatusTable.Columns.Add(new DataColumn(ListingModifiedDateTimeZoneIdColumnName, typeof(int)));
                        listingStatusTable.Columns.Add(new DataColumn(ProductColumnName, typeof(long)));
                        listingStatusTable.Columns.Add(new DataColumn(ProcessedColumnName, typeof(int)));
                        listingStatusTable.Columns.Add(new DataColumn(StatusDateTimeColumnName, typeof(DateTime)));
                        listingStatusTable.Columns.Add(new DataColumn(StatusDateTimeZoneIdColumnName, typeof(int)));
                        listingStatusTable.Columns.Add(new DataColumn(StatusMessageColumnName, typeof(string)));
                        listingStatusTable.Columns.Add(new DataColumn(ChannelBatchIdColumnName, typeof(string)));
                        listingStatusTable.Columns.Add(new DataColumn(ChannelStateColumnName, typeof(string)));
                        listingStatusTable.Columns.Add(new DataColumn(LanguageIdColumnName, typeof(string)));
    
                        foreach (var status in statusList)
                        {
                            DataRow listingStatusLogRow = listingStatusTable.NewRow();
                            listingStatusLogRow[ActionStatusColumnName] = status.PublishStatus;
                            listingStatusLogRow[AppliedActionColumnName] = status.AppliedAction;
                            listingStatusLogRow[CatalogColumnName] = status.CatalogId;
                            listingStatusLogRow[ChannelColumnName] = status.ChannelId;
                            listingStatusLogRow[ChannelListingIdColumnName] = status.ChannelListingId ?? string.Empty;
                            listingStatusLogRow[ListingModifiedDateTimeColumnName] = status.ListingModifiedDateTime.DateTime;
                            listingStatusLogRow[ListingModifiedDateTimeZoneIdColumnName] = 0;
                            listingStatusLogRow[ProductColumnName] = status.ProductId;
                            listingStatusLogRow[ProcessedColumnName] = 0;
                            listingStatusLogRow[StatusDateTimeColumnName] = DateTime.UtcNow;
                            listingStatusLogRow[StatusDateTimeZoneIdColumnName] = 0;
                            listingStatusLogRow[StatusMessageColumnName] = status.StatusMessage;
                            listingStatusLogRow[ChannelBatchIdColumnName] = status.ChannelBatchId ?? string.Empty;
                            listingStatusLogRow[ChannelStateColumnName] = status.ChannelState ?? string.Empty;
                            listingStatusLogRow[LanguageIdColumnName] = status.LanguageId ?? string.Empty;
    
                            listingStatusTable.Rows.Add(listingStatusLogRow);
                        }
    
                        // Insert the new records in their destination tables. Using tvps seems to be more efficient
                        // than bulk inserts up to 1000 rows;
                        // This update does not invalidate the cache content.
                        ParameterSet parameters = new ParameterSet();
                        parameters["@tvp_ListingPublishStatus"] = listingStatusTable;
    
                        int errorCode;
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                        {
                            errorCode = databaseContext.ExecuteStoredProcedureScalar(InsertPublishStatusForCurrentListingPageSprocName, parameters);
                        }
    
                        if (errorCode != (int)DatabaseErrorCodes.Success)
                        {
                            throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to update listing statuses.");
                        }
                    }
    
                    processTimer.Stop();
                    NetTracer.Information("** timer info: ApplyListingStatuses completed in {0} ms.", processTimer.ElapsedMilliseconds);
                }
    
                return new NullResponse();
            }

            private static EntityDataServiceResponse<ProductCatalog> GetProductCatalogs(GetProductCatalogsDataRequest request)
            {
                RequestContext context = request.RequestContext;
                CatalogSearchCriteria queryCriteria = request.Criteria;
                QueryResultSettings queryResultSettings = request.QueryResultSettings;
                ThrowIf.Null(context, "context");
                ThrowIf.Null(queryCriteria, "queryCriteria");
                ThrowIf.Null(queryResultSettings, "queryResultSettings");

                ProductDataManager productDataManager = GetProductDataManager(context);
                ProductL2CacheDataStoreAccessor level2CacheDataAccessor = (ProductL2CacheDataStoreAccessor)productDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<ProductCatalog> catalogs = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetProductCatalogs(queryCriteria, queryResultSettings), out found, out updateL2Cache);

                if (!found)
                {
                    Stopwatch processTimer = Stopwatch.StartNew();

                    catalogs = RetrieveProductCatalogs(context, queryCriteria, queryResultSettings).Results;

                    processTimer.Stop();
                    NetTracer.Information("** timer info: GetProductCatalogs completed in {0} ms", processTimer.ElapsedMilliseconds);

                    var getChannelByIdDataRequest = new GetChannelByIdDataRequest(queryCriteria.ChannelId);
                    string channelName = context.Runtime.Execute<SingleEntityDataServiceResponse<Channel>>(getChannelByIdDataRequest, context).Entity.Name;

                    // if image, process
                    ProcessCatalogImages(catalogs, channelName, context.GetChannelConfiguration());

                    updateL2Cache &= catalogs != null
                                     && catalogs.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutProductCatalogs(queryCriteria, queryResultSettings, catalogs);
                }

                // Filter to only catalogs for the requested language. If no catlaogs for the requested language are found, then fall back on the catalogs for the channel's default language.
                PagedResult<ProductCatalog> filteredCatalogs = catalogs.Where(catalog => string.Equals(context.LanguageId, catalog.Language, StringComparison.OrdinalIgnoreCase)).AsPagedResult();
                if (!filteredCatalogs.Results.Any())
                {
                    string defaultLanguageId = context.GetChannelConfiguration().DefaultLanguageId;
                    filteredCatalogs = catalogs.Where(catalog => string.Equals(defaultLanguageId, catalog.Language, StringComparison.OrdinalIgnoreCase)).AsPagedResult();
                    RetailLogger.Log.CrtServicesUsingChannelDefaultLanguageToFilterCatalogs(context.GetChannelConfiguration().RecordId, context.LanguageId, defaultLanguageId);
                }

                return new EntityDataServiceResponse<ProductCatalog>(filteredCatalogs);
            }
    
            /// <summary>
            /// Given a set of product identifiers, gets the kit definition, for the products that are kits.
            /// </summary>
            /// <param name="request">The request object with record Ids of the products for which kit property is being retrieved.</param>
            /// <returns>Collection of Kit property.</returns>
            private static ReadOnlyCollection<KitDefinition> GetProductKitDefinition(GetProductKitDataRequest request)
            {
                ThrowIf.Null(request, "request");
                RequestContext context = request.RequestContext;
                var productIds = request.KitMasterProductIds;
                ThrowIf.Null(productIds, "productIds");
                ThrowIf.Null(context, "context");
    
                if (!productIds.Any())
                {
                    return new List<KitDefinition>(0).AsReadOnly();
                }
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = GetKitListingFuncName
                };
    
                using (RecordIdTableType type = new RecordIdTableType(productIds, KitDefinition.KitProductMasterColumnName))
                {
                    query.Parameters["@bi_ChannelId"] = context.GetPrincipal().ChannelId;
                    query.Parameters["@dt_ChannelDate"] = context.GetNowInChannelTimeZone().DateTime;
                    query.Parameters["@tvp_KitProductMasterIds"] = type.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        return databaseContext.ReadEntity<KitDefinition>(query).Results;
                    }
                }
            }
    
            /// <summary>
            /// Given a set of product identifiers, gets the kit line definitions, for the products that are kits.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="productIds">Record Ids of the products for which kit line definitions is being retrieved.</param>
            /// <returns>Collection of Kit line definitions.</returns>
            private static ReadOnlyCollection<KitComponent> GetProductKitLineDefinitions(GetProductKitDataRequest request, IList<long> productIds)
            {
                ThrowIf.Null(productIds, "productIds");
                ThrowIf.Null(request, "request");
                RequestContext context = request.RequestContext;
                ThrowIf.Null(context, "context");
    
                if (!productIds.Any())
                {
                    return new List<KitComponent>(0).AsReadOnly();
                }
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = GetKitComponentAndSubstituteFuncName
                };
    
                using (RecordIdTableType type = new RecordIdTableType(productIds, KitProductMasterColumnName))
                {
                    query.Parameters["@bi_ChannelId"] = context.GetPrincipal().ChannelId;
                    query.Parameters["@dt_ChannelDate"] = context.GetNowInChannelTimeZone().DateTime;
                    query.Parameters["@tvp_KitProductMasterIds"] = type.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        return databaseContext.ReadEntity<KitComponent>(query).Results;
                    }
                }
            }
    
            /// <summary>
            /// Given a set of product identifiers, gets the associations between its kit variants and the component products used in the kit variant, as a flat list.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="productIds">Record identifiers of the products for which kit component definition and property is being retrieve.</param>
            /// <returns>Collection of kit variant line products(component product) to variant association.</returns>
            private static ReadOnlyCollection<KitConfigToComponentAssociation> GetKitConfigToComponentAssociation(GetProductKitDataRequest request, IList<long> productIds)
            {
                ThrowIf.Null(productIds, "productIds");
                ThrowIf.Null(request, "request");
                RequestContext context = request.RequestContext;
                ThrowIf.Null(context, "context");
    
                if (!productIds.Any())
                {
                    return new List<KitConfigToComponentAssociation>(0).AsReadOnly();
                }
    
                var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = GetKitConfigurationToProductMapFuncName
                };
    
                using (RecordIdTableType type = new RecordIdTableType(productIds, KitConfigToComponentAssociation.KitProductMasterColumnName))
                {
                    query.Parameters["@bi_ChannelId"] = context.GetPrincipal().ChannelId;
                    query.Parameters["@dt_ChannelDate"] = context.GetNowInChannelTimeZone().DateTime;
                    query.Parameters["@tvp_ProductIds"] = type.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        return databaseContext.ReadEntity<KitConfigToComponentAssociation>(query).Results;
                    }
                }
            }
    
            /// <summary>
            /// Get product parts.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="queryCriteria">The product search criteria.</param>
            /// <param name="isForwardLooking">Whether it should retrieve products assorted in a future date.</param>
            /// <param name="settings">The query result settings to apply to the returned result set.</param>
            /// <param name="isOnline">Whether the current search is online or offline.</param>
            /// <returns>The collections of product parts.</returns>
            [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Legacy code.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Legacy code.")]
            private static Tuple<ReadOnlyCollection<ProductIdentity>,
                ReadOnlyCollection<ProductVariant>,
                ReadOnlyCollection<ProductRules>,
                ReadOnlyCollection<ProductAttributeSchemaEntry>,
                ReadOnlyCollection<ProductProperty>,
                ReadOnlyCollection<ProductCatalog>,
                ReadOnlyCollection<ProductCategoryAssociation>,
                Tuple<ReadOnlyCollection<RelatedProduct>>> RetrieveProductParts(
                RequestContext context,
                ProductSearchCriteria queryCriteria,
                bool isForwardLooking,
                QueryResultSettings settings,
                bool isOnline)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(queryCriteria, "queryCriteria");
                ThrowIf.Null(settings, "settings");
    
                ParameterSet parameters = new ParameterSet();
                Tuple<ReadOnlyCollection<ProductIdentity>,
                        ReadOnlyCollection<ProductVariant>,
                        ReadOnlyCollection<ProductRules>,
                        ReadOnlyCollection<ProductAttributeSchemaEntry>,
                        ReadOnlyCollection<ProductProperty>,
                        ReadOnlyCollection<ProductCatalog>,
                        ReadOnlyCollection<ProductCategoryAssociation>,
                        Tuple<ReadOnlyCollection<RelatedProduct>>> dataSets = null;
    
                var applicableCriteria = GetApplicableSearchCriteria(queryCriteria);
    
                parameters[DatabaseAccessor.ChannelIdVariableName] = queryCriteria.Context.ChannelId;
                parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                parameters[DataLevelVariableName] = queryCriteria.DataLevel;
                parameters[IsForwardLookingVariableName] = isForwardLooking;
                parameters[SkipVariantExpansionVariableName] = queryCriteria.SkipVariantExpansion;
    
                ProductDataManager productDataManager = GetProductDataManager(context);
                ProductDatabaseAccessor databaseAccessor = (ProductDatabaseAccessor)productDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.Database];
    
                if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.CategoryIds))
                {
                    using (var categoryIdTable = new RecordIdTableType(queryCriteria.CategoryIds))
                    using (var refinerValuesTable = new ProductRefinerValueTableType(queryCriteria.Refinement))
                    {
                        parameters[CatalogIdVariableName] = queryCriteria.Context.CatalogId.GetValueOrDefault();
                        parameters[IncludeProductsFromDescendantCategoriesVariableName] = queryCriteria.IncludeProductsFromDescendantCategories;
                        parameters[LanguageIdVariableName] = context.GetChannelConfiguration().DefaultLanguageId;
                        parameters[CategoryIdsVariableName] = categoryIdTable.DataTable;
                        parameters[RefinerValuesVariableName] = refinerValuesTable.DataTable;
                        parameters[IsOnlineSearchName] = isOnline;
    
                        dataSets = databaseAccessor.ExecuteStoredProcedure<ProductIdentity, ProductVariant, ProductRules, ProductAttributeSchemaEntry, ProductProperty, ProductCatalog, ProductCategoryAssociation, RelatedProduct>(GetProductsByCategorySprocName, parameters, settings);
                    }
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.Keyword))
                {
                    using (var refinerValuesTable = new ProductRefinerValueTableType(queryCriteria.Refinement))
                    {
                        parameters[SearchConditionVariableName] = queryCriteria.IncludeProductsFromDescendantCategories;
                        parameters[SearchConditionVariableName] = string.Format(CultureInfo.InvariantCulture, SearchConditionParametrizationString, queryCriteria.SearchCondition.Replace("\"", "\"\""));
                        parameters[LanguageIdVariableName] = context.GetChannelConfiguration().DefaultLanguageId;
                        parameters[RefinerValuesVariableName] = refinerValuesTable.DataTable;
                        parameters[IsOnlineSearchName] = isOnline;
    
                        dataSets = databaseAccessor.ExecuteStoredProcedure<ProductIdentity, ProductVariant, ProductRules, ProductAttributeSchemaEntry, ProductProperty, ProductCatalog, ProductCategoryAssociation, RelatedProduct>(GetProductsByKeywordSprocName, parameters, settings);
                    }
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.ItemIds))
                {
                    using (var itemIdTable = new ItemIdSearchTableType(queryCriteria.ItemIds))
                    {
                        parameters[DataAreaIdVariableName] = context.GetChannelConfiguration().InventLocationDataAreaId;
                        parameters[ItemIdsVariableName] = itemIdTable.DataTable;
    
                            dataSets = databaseAccessor.ExecuteStoredProcedure<ProductIdentity, ProductVariant, ProductRules, ProductAttributeSchemaEntry, ProductProperty, ProductCatalog, ProductCategoryAssociation, RelatedProduct>(GetProductsByItemIdSprocName, parameters, settings);
                    }
                }
                else if (applicableCriteria.HasFlag(ProductSearchCriteriaInputs.None) || applicableCriteria.HasFlag(ProductSearchCriteriaInputs.ProductIds))
                {
                    using (var productIdTable = new RecordIdTableType(queryCriteria.Ids))
                    {
                        parameters[CatalogIdVariableName] = queryCriteria.Context.CatalogId.GetValueOrDefault();
                        parameters[ProductIdsVariableName] = productIdTable.DataTable;
                        parameters[IsOnlineSearchName] = isOnline;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, settings))
                        {
                            dataSets = databaseAccessor.ExecuteStoredProcedure<ProductIdentity, ProductVariant, ProductRules, ProductAttributeSchemaEntry, ProductProperty, ProductCatalog, ProductCategoryAssociation, RelatedProduct>(GetProductsSprocName, parameters, settings);
                        }
                    }
                }
    
                return dataSets;
            }
    
            private static ProductSearchCriteriaInputs GetApplicableSearchCriteria(ProductSearchCriteria searchCriteria)
            {
                ProductSearchCriteriaInputs inputs = ProductSearchCriteriaInputs.None;
                int criteriaCounter = 0;
                if (!searchCriteria.Ids.IsNullOrEmpty())
                {
                    inputs |= ProductSearchCriteriaInputs.ProductIds;
                    criteriaCounter++;
                }
    
                if (!searchCriteria.CategoryIds.IsNullOrEmpty())
                {
                    inputs |= ProductSearchCriteriaInputs.CategoryIds;
                    criteriaCounter++;
                }
    
                if (!searchCriteria.ItemIds.IsNullOrEmpty())
                {
                    inputs |= ProductSearchCriteriaInputs.ItemIds;
                    criteriaCounter++;
                }
    
                if (!string.IsNullOrWhiteSpace(searchCriteria.SearchCondition))
                {
                    inputs |= ProductSearchCriteriaInputs.Keyword;
                    criteriaCounter++;
                }
    
                if (criteriaCounter > 1)
                {
                    string error = string.Format(
                        "This criteria combination is not suppoted: useProductIds={0}; useCategoryIds={1}; useItemIds={2}; useKeywords={3}. Only one search criteria type can be specified.",
                        inputs.HasFlag(ProductSearchCriteriaInputs.ProductIds),
                        inputs.HasFlag(ProductSearchCriteriaInputs.CategoryIds),
                        inputs.HasFlag(ProductSearchCriteriaInputs.ItemIds),
                        inputs.HasFlag(ProductSearchCriteriaInputs.Keyword));
                    throw new NotSupportedException(error);
                }
    
                return inputs;
            }
    
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "defaultLanguageId", Justification = "Will be eventually used by the assembly function.")]
            [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "The product converges in this function. And we're off by 1 only.")]
            private static ProductSearchResultContainer RetrieveAndAssembleProducts(IList<long> productIds, ProjectionDomain context, CommerceEntityDataLevel dataLevel, RequestContext requestContext, ProductSearchResultContainer result, QueryResultSettings settings, bool isForwardLooking, bool skipVariantExpansion)
            {
                Stopwatch processTimer = Stopwatch.StartNew();
                Stopwatch queryTimer = Stopwatch.StartNew();
    
                Tuple<ReadOnlyCollection<ProductIdentity>,
                        ReadOnlyCollection<ProductVariant>,
                        ReadOnlyCollection<ProductRules>,
                        ReadOnlyCollection<ProductAttributeSchemaEntry>,
                        ReadOnlyCollection<ProductProperty>,
                        ReadOnlyCollection<ProductCatalog>,
                        ReadOnlyCollection<ProductCategoryAssociation>,
                        Tuple<ReadOnlyCollection<RelatedProduct>>> dataSets;
    
                // nop?
                if (!productIds.Any())
                {
                    result.Results = Enumerable.Empty<Product>().AsReadOnly();
                    return result;
                }
    
                ProductSearchCriteria criteria = new ProductSearchCriteria(requestContext.GetPrincipal().ChannelId);
                criteria.Ids = productIds;
                criteria.Context = context;
                criteria.DataLevel = dataLevel;
                criteria.SkipVariantExpansion = skipVariantExpansion;
    
                dataSets = RetrieveProductParts(requestContext, criteria, isForwardLooking, settings, true);
    
                ReadOnlyCollection<UnitOfMeasureConversion> unitOfMeasureOptionsDataSet = null;
                ReadOnlyCollection<LinkedProduct> linkedProductsDataSet = null;
                ReadOnlyCollection<KitDefinition> kitDefinitions = null;
                ReadOnlyCollection<KitComponent> componentAndSubstituteList = null;
                ReadOnlyCollection<KitConfigToComponentAssociation> configToComponentAssociations = null;
                ReadOnlyCollection<KitComponent> parentKitsComponentInfo = null;
    
                if (result.DataLevel >= CommerceEntityDataLevel.Extended)
                {
                    GetLinkedProductsDataRequest dataRequest = new GetLinkedProductsDataRequest(productIds);
                    dataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    linkedProductsDataSet = requestContext.Runtime.Execute<GetLinkedProductsDataResponse>(dataRequest, requestContext).LinkedProducts;
    
                    IList<long> masterIds = dataSets.Item1.Where(i => i.IsMasterProduct).Select(t => t.LookupId).ToList();
                    IList<long> allIds = productIds.Union(masterIds).ToList();      // Get master, variant and standalone product Ids
    
                    unitOfMeasureOptionsDataSet = GetUnitOfMeasureOptions(requestContext, allIds, QueryResultSettings.AllRecords).Results;
    
                    IList<long> kitMasterIds = dataSets.Item1.Where(i => i.IsKitProduct).Select(t => t.LookupId).ToList();
    
                    // We can skip the kit assembly code path if we have no products that are kits.
                    if (kitMasterIds.Any())
                    {
                        GetProductKitDataRequest getProductKitsDataRequest = new GetProductKitDataRequest(kitMasterIds);
                        var response = requestContext.Runtime.Execute<GetProductKitDataResponse>(getProductKitsDataRequest, requestContext);
                        kitDefinitions = response.KitDefinitions;
                        componentAndSubstituteList = response.KitComponentAndSubstituteList;
                        configToComponentAssociations = response.KitConfigToComponentAssociations;
                    }
    
                    if (result.DataLevel >= CommerceEntityDataLevel.Complete)
                    {
                        // This call used to check if a product (non-kit) participates in any kit configuration as a kit component.
                        // This call is necessary to create an inverse mapping between a product and a kit.
                        parentKitsComponentInfo = GetParentKitComponentInfo(requestContext, allIds, settings).Results;
                    }
                }
    
                queryTimer.Stop();
    
                Stopwatch assemblyTimer = Stopwatch.StartNew();
    
                ProductBuilder.AssembleProductsFromDataSets(
                    context,
                    dataLevel,
                    dataSets,
                    linkedProductsDataSet,
                    kitDefinitions,
                    componentAndSubstituteList,
                    configToComponentAssociations,
                    parentKitsComponentInfo,
                    unitOfMeasureOptionsDataSet,
                    result, 
                    requestContext.GetChannelConfiguration().ProductDefaultImageTemplate);

                assemblyTimer.Stop();
                processTimer.Stop();
    
                return result;
            }
    
            /// <summary>
            /// Gets the component level details for all the kits whose product master identifiers are provided.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="kitComponentProductIds">The product identifiers of all products that could be potentially product masters/standalone product of any kit component line.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The collection of parent kit's component information. An empty list is returned if no matching record could be found.</returns>
            private static PagedResult<KitComponent> GetParentKitComponentInfo(RequestContext context, IEnumerable<long> kitComponentProductIds, QueryResultSettings settings)
            {
                ThrowIf.Null(kitComponentProductIds, "kitComponentProductIds");
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                PagedResult<KitComponent> result;
                using (RecordIdTableType kitComponentProductIdsTable = new RecordIdTableType(kitComponentProductIds))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                    parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                    parameters[KitComponentProductIdsVariableName] = kitComponentProductIdsTable.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, settings))
                    {
                        result = databaseContext.ExecuteStoredProcedure<KitComponent>(GetKitComponentsInfoSprocName, parameters);
                    }
                }
    
                processTimer.Stop();
                NetTracer.Information("** timer info: GetKitComponentsInfo completed in {0} ms.", processTimer.ElapsedMilliseconds);
    
                return result;
            }
    
            /// <summary>
            /// Retrieves the list of convertible units of measure for the products.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="productIds">Record Ids of the products (master and standalone Ids are supported, variant Ids will be ignored).</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of convertible units of measure.</returns>
            private static PagedResult<UnitOfMeasureConversion> GetUnitOfMeasureOptions(RequestContext context, IEnumerable<long> productIds, QueryResultSettings settings)
            {
                ThrowIf.Null(productIds, "productIds");
                if (!productIds.Any())
                {
                    return new List<UnitOfMeasureConversion>(0).AsPagedResult();
                }
    
                PagedResult<UnitOfMeasureConversion> convertibleUoMList;
                ParameterSet parameters = new ParameterSet();
    
                using (RecordIdTableType type = new RecordIdTableType(productIds))
                {
                    parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                    parameters[ProductIdsVariableName] = type.DataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, settings))
                    {
                        convertibleUoMList = databaseContext.ExecuteStoredProcedure<UnitOfMeasureConversion>(GetUnitOfMeasureOptionsSprocName, parameters);
                    }
                }
    
                return convertibleUoMList;
            }
    
            /// <summary>
            /// Retrieves the ids of products that satisfy the specified change-tracking criteria.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="queryCriteria">The query criteria.</param>
            /// <param name="currentMarker">Current marker.</param>
            /// <param name="currentWatermark">The current watermark.</param>
            /// <param name="nextWatermark">The next watermark.</param>
            /// <returns>
            /// The collection of products ids.
            /// </returns>
            private static PagedResult<CommerceEntityChangeTrackingInformation> RetrieveIdsOfChangedProducts(
                RequestContext context,
                ChangedProductsSearchCriteria queryCriteria,
                out long currentMarker,
                out ProductChangeTrackingAnchorSet currentWatermark,
                out ProductChangeTrackingAnchorSet nextWatermark)
            {
                ThrowIf.Null(queryCriteria, "queryCriteria");
                ThrowIf.Null(context, "context");
    
                if (!queryCriteria.Context.ChannelId.HasValue)
                {
                    queryCriteria.Context.ChannelId = context.GetPrincipal().ChannelId;
                }
    
                // extract the watermark from the synchronization anchor byte array. We may want to change this into an XML doc,
                // or something else less prone to misinterpretation. The sync anchor is the byte representation of a serialized SynchronizationToken token.
                currentMarker = 0;
    
                // Extract the sync anchor set and marker from the synchronization token.
                currentWatermark = ProductChangeTrackingAnchorSet.GetAnchorSetFromSynchronizationToken(queryCriteria.SynchronizationToken, out currentMarker);
                if (currentWatermark.ChannelId == 0 && queryCriteria.Context.ChannelId.HasValue)
                {
                    currentWatermark.ChannelId = queryCriteria.Context.ChannelId.Value;
                }
    
                ThrowIf.Null(currentWatermark, "currentWatermark");
    
                Stopwatch processTimer = Stopwatch.StartNew();
    
                int storedProcReturnValue = 0;
                ParameterSet parameters = new ParameterSet();
                Tuple<PagedResult<CommerceEntityChangeTrackingInformation>,
                    ReadOnlyCollection<ProductChangeTrackingAnchorSet>> dataSets;
    
                // translate the current watermark into a data table to be passed to the sproc.
                using (var watermarkDataTable = BuildProductAnchorSetTableFromWatermark(currentWatermark))
                {
                    parameters[DatabaseAccessor.ChannelIdVariableName] = currentWatermark.ChannelId;
                    parameters[WatermarkVariableName] = watermarkDataTable;
    
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, QueryResultSettings.AllRecords))
                    {
                        dataSets = databaseContext.ExecuteStoredProcedure<CommerceEntityChangeTrackingInformation, ProductChangeTrackingAnchorSet>(GetChangedProductIdsSprocName, parameters, null, out storedProcReturnValue);
                    }
                }
    
                processTimer.Stop();
                nextWatermark = dataSets.Item2.First();
    
                return dataSets.Item1;
            }
    
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller disposes")]
            private static DataTable BuildProductAnchorSetTableFromWatermark(ProductChangeTrackingAnchorSet watermark)
            {
                // create a data table object and update with the specified anchors
                DataTable anchorSet = new DataTable(ProductChangeTrackingAnchorSetTableTypeName);
    
                anchorSet.Columns.Add(ChannelColumn, typeof(long)).DefaultValue = watermark.ChannelId;
                anchorSet.Columns.Add(ERPColumn, typeof(long)).DefaultValue = watermark.EcoResProduct;
                anchorSet.Columns.Add(ERPIVColumn, typeof(long)).DefaultValue = watermark.EcoResProductInstanceValue;
                anchorSet.Columns.Add(ERIVColumn, typeof(long)).DefaultValue = watermark.EcoResInstanceValue;
                anchorSet.Columns.Add(ERAColumn, typeof(long)).DefaultValue = watermark.EcoResAttribute;
                anchorSet.Columns.Add(ERAVColumn, typeof(long)).DefaultValue = watermark.EcoResAttributeValue;
                anchorSet.Columns.Add(ERVColumn, typeof(long)).DefaultValue = watermark.EcoResValue;
                anchorSet.Columns.Add(ERBVColumn, typeof(long)).DefaultValue = watermark.EcoResBooleanValue;
                anchorSet.Columns.Add(ERDTVColumn, typeof(long)).DefaultValue = watermark.EcoResDateTimeValue;
                anchorSet.Columns.Add(ERCVColumn, typeof(long)).DefaultValue = watermark.EcoResCurrencyValue;
                anchorSet.Columns.Add(ERFVColumn, typeof(long)).DefaultValue = watermark.EcoResFloatValue;
                anchorSet.Columns.Add(ERNVColumn, typeof(long)).DefaultValue = watermark.EcoResIntValue;
                anchorSet.Columns.Add(ERRVColumn, typeof(long)).DefaultValue = watermark.EcoResReferenceValue;
                anchorSet.Columns.Add(ERTVColumn, typeof(long)).DefaultValue = watermark.EcoResTextValue;
                anchorSet.Columns.Add(ERTVTColumn, typeof(long)).DefaultValue = watermark.EcoResTextValueTranslation;
                anchorSet.Columns.Add(ERPTColumn, typeof(long)).DefaultValue = watermark.EcoResProductTranslation;
                anchorSet.Columns.Add(ERPVColColumn, typeof(long)).DefaultValue = watermark.EcoResProductVariantColor;
                anchorSet.Columns.Add(ERPVConColumn, typeof(long)).DefaultValue = watermark.EcoResProductVariantConfiguration;
                anchorSet.Columns.Add(ERPVDVColumn, typeof(long)).DefaultValue = watermark.EcoResProductVariantDimensionValue;
                anchorSet.Columns.Add(ERPVSzColumn, typeof(long)).DefaultValue = watermark.EcoResProductVariantSize;
                anchorSet.Columns.Add(ERPVStColumn, typeof(long)).DefaultValue = watermark.EcoResProductVariantStyle;
                anchorSet.Columns.Add(ERColColumn, typeof(long)).DefaultValue = watermark.EcoResColor;
                anchorSet.Columns.Add(ERConColumn, typeof(long)).DefaultValue = watermark.EcoResConfiguration;
                anchorSet.Columns.Add(ERSzColumn, typeof(long)).DefaultValue = watermark.EcoResSize;
                anchorSet.Columns.Add(ERStColumn, typeof(long)).DefaultValue = watermark.EcoResStyle;
                anchorSet.Columns.Add(ERAGAColumn, typeof(long)).DefaultValue = watermark.EcoResAttributeGroupAttribute;
                anchorSet.Columns.Add(ERCAGColumn, typeof(long)).DefaultValue = watermark.EcoResCategoryAttributeGroup;
                anchorSet.Columns.Add(ERCALColumn, typeof(long)).DefaultValue = watermark.EcoResCategoryAttributeLookup;
                anchorSet.Columns.Add(ERDPVColumn, typeof(long)).DefaultValue = watermark.EcoResDistinctProductVariant;
                anchorSet.Columns.Add(ERPCColumn, typeof(long)).DefaultValue = watermark.EcoResProductCategory;
                anchorSet.Columns.Add(RCCLColumn, typeof(long)).DefaultValue = watermark.RetailCategoryContainmentLookup;
                anchorSet.Columns.Add(RPCPColumn, typeof(long)).DefaultValue = watermark.RetailPubCatalogProduct;
                anchorSet.Columns.Add(RPCPCColumn, typeof(long)).DefaultValue = watermark.RetailPubCatalogProductCategory;
                anchorSet.Columns.Add(RPCPRColumn, typeof(long)).DefaultValue = watermark.RetailPubCatalogProductRelation;
                anchorSet.Columns.Add(RPCPREColumn, typeof(long)).DefaultValue = watermark.RetailPubCatalogProductRelationExclusion;
                anchorSet.Columns.Add(RPIOAGColumn, typeof(long)).DefaultValue = watermark.RetailPubInternalOrgAttributeGroup;
                anchorSet.Columns.Add(RPIOIEColumn, typeof(long)).DefaultValue = watermark.RetailPubIntOrgInheritanceExploded;
                anchorSet.Columns.Add(RPPACMDColumn, typeof(long)).DefaultValue = watermark.RetailPubProductAttributeChannelMetadata;
                anchorSet.Columns.Add(RPPAVColumn, typeof(long)).DefaultValue = watermark.RetailPubProductAttributeValue;
                anchorSet.Columns.Add(RPRCTColumn, typeof(long)).DefaultValue = watermark.RetailPubRetailChannelTable;
                anchorSet.Columns.Add(RSPColumn, typeof(long)).DefaultValue = watermark.RetailSharedParameters;
                anchorSet.Columns.Add(RSAColumn, typeof(long)).DefaultValue = watermark.RetailStandardAttribute;
                anchorSet.Columns.Add(UOMColumn, typeof(long)).DefaultValue = watermark.UnitOfMeasure;
    
                anchorSet.Rows.Add(anchorSet.NewRow());
    
                return anchorSet;
            }
    
            /// <summary>
            /// Retrieves the ids of products that satisfy the specified criteria.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="queryCriteria">The query criteria.</param>
            /// <param name="querySettings">The query result settings.</param>
            /// <returns>
            /// The collection of products.
            /// </returns>
            [SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "Mandated by the functional requirements of this type.")]
            private static PagedResult<ProductCatalog> RetrieveProductCatalogs(RequestContext context, CatalogSearchCriteria queryCriteria, QueryResultSettings querySettings)
            {
                ThrowIf.Null(queryCriteria, "queryCriteria");
                ThrowIf.Null(querySettings, "querySettings");
    
                ParameterSet parameters = new ParameterSet();
    
                parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime.Date;
                parameters[DatabaseAccessor.ChannelIdVariableName] = queryCriteria.ChannelId;
                parameters[LoadMediaServerImageParameterName] = context.Runtime.Configuration.IsMasterDatabaseConnectionString;
    
                var pagedQuery = new SqlPagedQuery(querySettings, parameters, null)
                {
                    Select = new ColumnSet(
                                "getCatalogsFn.[CATALOG]",
                                "getCatalogsFn.[NAME]",
                                "getCatalogsFn.[DESCRIPTION]",
                                "getCatalogsFn.[IMAGE]",
                                "getCatalogsFn.[DEFAULTIMAGE]",
                                "getCatalogsFn.[LANGUAGE]",
                                "getCatalogsFn.[ENABLESNAPSHOT]",
                                "getCatalogsFn.[VALIDFROM]",
                                "getCatalogsFn.[VALIDTO]",
                                "getCatalogsFn.[CREATEDDATETIME]",
                                "getCatalogsFn.[MODIFIEDDATETIME]",
                                "getCatalogsFn.[PUBLISHEDDATETIME]"),
                    From = string.Format("{0} getCatalogsFn", GetPublishedCatalogsToActiveChannelFunctionName),
                    OrderBy = "VALIDFROM DESC, MODIFIEDDATETIME DESC, PUBLISHEDDATETIME DESC, CREATEDDATETIME DESC "
                };
    
                // if search for active catalogs only, append the WHERE clause
                if (queryCriteria.ActiveOnly)
                {
                    pagedQuery.Where = string.Format(
                        "{0} BETWEEN getCatalogsFn.[VALIDFROM] AND getCatalogsFn.[VALIDTO]",
                        DatabaseAccessor.ChannelDateVariableName);
                }
    
                PagedResult<ProductCatalog> catalogs;
    
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, querySettings))
                {
                    catalogs = databaseContext.ReadEntity<ProductCatalog>(pagedQuery);
                }
    
                return catalogs;
            }
    
            /// <summary>
            /// Processes the catalog images.
            /// </summary>
            /// <param name="catalogs">The catalogs.</param>
            /// <param name="channelName">The channel name.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            private static void ProcessCatalogImages(IEnumerable<ProductCatalog> catalogs, string channelName, ChannelConfiguration channelConfiguration)
            {
                if (catalogs != null &&
                    catalogs.Any())
                {
                    // process catalogs with image
                    foreach (ProductCatalog productCatalog in catalogs.ToList())
                    {
                        productCatalog.Images = RichMediaHelper.PopulateCatalogMediaInformation(
                            productCatalog,
                            channelConfiguration.DefaultLanguageId,
                            channelName,
                            channelConfiguration.CatalogDefaultImageTemplate);
                    }
                }
            }
    
            private static ProductDataManager GetProductDataManager(RequestContext context)
            {
                return new ProductDataManager(context);
            }
        }
    }
}
