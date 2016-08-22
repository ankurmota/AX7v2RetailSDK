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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Data service for products.
        /// </summary>
        public sealed class ItemSqlServerDataService : IRequestHandler
        {
            private const string ProductIdsVariableName = "@tvp_ProductIds";
            private const string ItemIdsVariableName = "@tvp_ItemIds";
            private const string GetItemsByProductIdsSprocName = "GETITEMSBYPRODUCTIDS";
            private const string GetItemsByItemIdsSprocName = "GETITEMSBYITEMIDS";
            private const string VariantIdsVariableName = "@tvp_VariantIds";
            private const string IsProductInCategorySprocName = "ISPRODUCTINCATEGORY";
            private const string ProductRecIdColumnName = "PRODUCTRECID";
            private const string CategoryRecIdColumnName = "CATEGORYRECID";
            private const string ItemVariantIdsVariableName = "@tvp_ItemVariantIds";
            private const string GetVariantsByItemIdAndInventDimIdFunctionName = "GETVARIANTSBYITEMIDANDINVENTDIMIDS(@bi_ChannelId, @dt_ChannelDate, @tvp_ItemVariantIds)";
    
            /// <summary>
            /// Indicates the maximum number of entries of a collection which can be cached.
            /// </summary>
            private const int MaxCachedCollectionSize = 500;
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetItemsDataRequest),
                        typeof(CheckIfProductOrVariantAreInCategoryDataRequest),
                        typeof(GetProductVariantsDataRequest),
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
    
                if (request is GetItemsDataRequest)
                {
                    response = GetItems((GetItemsDataRequest)request);
                }
                else if (request is CheckIfProductOrVariantAreInCategoryDataRequest)
                {
                    response = IsProductOrVariantInCategory((CheckIfProductOrVariantAreInCategoryDataRequest)request);
                }
                else if (request is GetProductVariantsDataRequest)
                {
                    response = GetProductVariants((GetProductVariantsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the cache accessor for the item data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ItemL2CacheDataStoreAccessor"/> class.</returns>
            private static ItemL2CacheDataStoreAccessor GetCacheAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new ItemL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
    
            private static GetItemsDataResponse GetItems(GetItemsDataRequest request)
            {
                QueryResultSettings settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
                ReadOnlyCollection<Item> items;
    
                if (!request.ItemIds.IsNullOrEmpty())
                {
                    items = GetItems(request.RequestContext, request.ItemIds, settings);
                }
                else if (!request.ProductIds.IsNullOrEmpty())
                {
                    items = GetItems(request.RequestContext, request.ProductIds, settings);
                }
                else
                {
                    items = new ReadOnlyCollection<Item>(new List<Item>());
                }
    
                return new GetItemsDataResponse(items);
            }
    
            private static EntityDataServiceResponse<ProductVariant> GetProductVariants(GetProductVariantsDataRequest request)
            {
                IEnumerable<ItemVariantInventoryDimension> itemVariants = request.ItemAndInventoryDimensionIds;
                RequestContext context = request.RequestContext;
                ThrowIf.Null(itemVariants, "itemVariants");
                ThrowIf.Null(context, "context");
    
                ColumnSet columnSet = new ColumnSet();
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<ProductVariant> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetVariants(itemVariants, columnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        Select = columnSet,
                        From = GetVariantsByItemIdAndInventDimIdFunctionName
                    };
    
                    using (var type = new ItemVariantInventoryDimensionTableType(itemVariants))
                    {
                        query.Parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                        query.Parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().Date;
                        query.Parameters[ItemVariantIdsVariableName] = type.DataTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context))
                        {
                            result = databaseContext.ReadEntity<ProductVariant>(query).Results;
                        }
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutVariants(itemVariants, columnSet, result);
                }
    
                return new EntityDataServiceResponse<ProductVariant>(result.AsPagedResult());
            }
    
            private static SingleEntityDataServiceResponse<bool> IsProductOrVariantInCategory(CheckIfProductOrVariantAreInCategoryDataRequest request)
            {
                RequestContext context = request.RequestContext;
                ThrowIf.Null(context, "context");
    
                ParameterSet parameters = new ParameterSet();
                parameters[ProductRecIdColumnName] = request.ProductRecordId;
                parameters[CategoryRecIdColumnName] = request.CategoryRecordId;
    
                int result;
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    result = databaseContext.ExecuteStoredProcedureNonQuery(IsProductInCategorySprocName, parameters);
                }
    
                return new SingleEntityDataServiceResponse<bool>(result == 1);
            }
    
            /// <summary>
            /// Gets the items available to the current channel by their product identifiers.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="productIds">The product identifiers.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The collection of items or an empty list if no matching record could be found.</returns>
            private static ReadOnlyCollection<Item> GetItems(RequestContext context, IEnumerable<long> productIds, QueryResultSettings settings)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(productIds, "productIds");
                ThrowIf.Null(settings, "settings");
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<Item> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetItems(productIds, settings), out found, out updateL2Cache);
    
                if (!found)
                {
                    using (RecordIdTableType type = new RecordIdTableType(productIds, "PRODUCT"))
                    {
                        var parameters = new ParameterSet();
                        parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                        parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                        parameters[ProductIdsVariableName] = type.DataTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, settings))
                        {
                            result = databaseContext.ExecuteStoredProcedure<Item>(GetItemsByProductIdsSprocName, parameters).Results;
                        }
                    }
    
                    updateL2Cache &= result != null
                                     && result.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutItems(productIds, settings, result);
                }
    
                return result;
            }
    
            /// <summary>
            /// Gets the items using the specified item identifiers.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="itemIds">The list of item identifiers.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>The collection of items.</returns>
            private static ReadOnlyCollection<Item> GetItems(RequestContext context, IEnumerable<string> itemIds, QueryResultSettings settings)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(itemIds, "itemIds");
                ThrowIf.Null(settings, "settings");
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<Item> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetItems(itemIds, settings), out found, out updateL2Cache);
    
                if (!found)
                {
                    using (StringIdTableType type = new StringIdTableType(itemIds, "ITEMID"))
                    {
                        var parameters = new ParameterSet();
                        parameters[DatabaseAccessor.ChannelIdVariableName] = context.GetPrincipal().ChannelId;
                        parameters[DatabaseAccessor.ChannelDateVariableName] = context.GetNowInChannelTimeZone().DateTime;
                        parameters[ItemIdsVariableName] = type.DataTable;
    
                        using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context, settings))
                        {
                            result = databaseContext.ExecuteStoredProcedure<Item>(GetItemsByItemIdsSprocName, parameters).Results;
                        }
                    }
    
                    updateL2Cache &= result != null
                                     && result.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutItems(itemIds, settings, result);
                }
    
                return result;
            }
        }
    }
}
