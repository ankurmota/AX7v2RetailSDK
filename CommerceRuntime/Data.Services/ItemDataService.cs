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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Product data service class.
        /// </summary>
        public sealed class ItemDataService : IRequestHandler
        {
            private const string BarcodeSetupIdColumnName = "BARCODESETUPID";
            private const string DataAreaIdColumnName = "DATAAREAID";
            private const string ItemBarcodeColumnName = "ITEMBARCODE";
            private const string InventDimIdColumnName = "INVENTDIMID";
            private const string ItemIdColumnName = "ITEMID";
            private const string RetailVariantIdColumn = "RETAILVARIANTID";
            private const string DisplayColumn = "RETAILSHOWFORITEM";
    
            private const string DataAreaIdVariableName = "@DataAreaId";
            private const string ItemIdTableTypeVariableName = "@TVP_ITEMIDTABLETYPE";
            private const string ItemBarcodeVariableName = "@ItemBarCode";
            private const string RetailVariantIdVariableName = "@RetailVariantId";
            private const string DisplayVariableName = "@RetailShowForItem";
            private const string ItemIdVariableName = "@ItemId";
            private const string RecordIdTableTypeVariableName = "@TVP_RECORDIDTABLETYPE";
    
            private const string BarcodesViewName = "INVENTITEMBARCODESVIEW";
            private const string InventDimViewName = "INVENTDIMVIEW";
            private const string ItemsMaxRetailPricesIndiaViewName = "ITEMSMAXRETAILPRICESINDIAVIEW";
            private const string RetailCategoryMembersForItemsViewName = "RETAILPRODUCTORVARIANTCATEGORYANCESTORSVIEW";
    
            /// <summary>
            /// Indicates the maximum number of entries of a collection which can be cached.
            /// </summary>
            private const int MaxCachedCollectionSize = 500;
    
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetProductBarcodeDataRequest),
                typeof(GetVariantsByDimensionIdsDataRequest),
                typeof(GetItemMaxRetailPriceDataRequest),
                typeof(GetRetailCategoryMembersForItemsDataRequest)
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
    
                Response response;
    
                if (request is GetProductBarcodeDataRequest)
                {
                    response = GetProductBarcodeData((GetProductBarcodeDataRequest)request);
                }
                else if (request is GetVariantsByDimensionIdsDataRequest)
                {
                    response = GetVariantsByDimensionIds((GetVariantsByDimensionIdsDataRequest)request);
                }
                else if (request is GetItemMaxRetailPriceDataRequest)
                {
                    response = GetItemMaxRetailPrice((GetItemMaxRetailPriceDataRequest)request);
                }
                else if (request is GetRetailCategoryMembersForItemsDataRequest)
                {
                    response = GetRetailCategoryMembersForItems((GetRetailCategoryMembersForItemsDataRequest)request);
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
    
            private static GetProductBarcodeDataResponse GetProductBarcodeData(GetProductBarcodeDataRequest request)
            {
                string barcode = request.Barcode;
                RequestContext context = request.RequestContext;
                ThrowIf.Null(barcode, "barcode");
                ThrowIf.Null(context, "context");
                ColumnSet columns = request.QueryResultSettings != null ? request.QueryResultSettings.ColumnSet : new ColumnSet();
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                ItemBarcode result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetItemsByBarcode(barcode, columns), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        Select = columns,
                        From = BarcodesViewName,
                        Where = string.Format("{0} LIKE {1} AND {2} = {3}", ItemBarcodeColumnName, ItemBarcodeVariableName, DataAreaIdColumnName, DataAreaIdVariableName),
                    };
    
                    query.Parameters[ItemBarcodeVariableName] = barcode;
                    query.Parameters[DataAreaIdVariableName] = context.GetChannelConfiguration().InventLocationDataAreaId;

                    IEnumerable<ItemBarcode> itemBarcodes;
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        itemBarcodes = databaseContext.ReadEntity<ItemBarcode>(query).Results;
                    }
    
                    // If the barcode matches with more than one variant, then we should set the variant details to null/ empty.
                    // When this scenario occurs, the client should pop up new dialog to get the variant details.
                    if (itemBarcodes.Count() > 1)
                    {
                        ItemBarcode itemBarcode = itemBarcodes.FirstOrDefault();
    
                        if (itemBarcode != null)
                        {
                            itemBarcode.SetVariantDetailsToEmpty();
    
                            result = itemBarcode;
                        }
                    }
                    else
                    {
                        using (DatabaseContext databaseContext = new DatabaseContext(context))
                        {
                            result = databaseContext.ReadEntity<ItemBarcode>(query).SingleOrDefault();
                        }
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutItemsByBarcode(barcode, columns, result);
                }
    
                return new GetProductBarcodeDataResponse(result);
            }
    
            private static EntityDataServiceResponse<ProductVariant> GetVariantsByDimensionIds(GetVariantsByDimensionIdsDataRequest request)
            {
                IEnumerable<string> inventoryDimensionIds = request.InventoryDimensionIds;
                RequestContext context = request.RequestContext;
                ThrowIf.Null(inventoryDimensionIds, "inventoryDimensionIds");
                ThrowIf.Null(context, "context");
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<ProductVariant> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetVariantsByDimensionIds(inventoryDimensionIds), out found, out updateL2Cache);
    
                if (!found)
                {
                    if (inventoryDimensionIds.Any())
                    {
                        var settings = new QueryResultSettings(PagingInfo.CreateWithExactCount(inventoryDimensionIds.Count(), 0));
    
                        var query = new SqlPagedQuery(settings)
                        {
                            Select = new ColumnSet(),
                            From = InventDimViewName,
                            Where = string.Format("{0} = {1}", DataAreaIdColumnName, DataAreaIdVariableName)
                        };
    
                        query.Parameters[DataAreaIdVariableName] = context.GetChannelConfiguration().InventLocationDataAreaId;
    
                        using (StringIdTableType type = new StringIdTableType(inventoryDimensionIds, InventDimIdColumnName))
                        {
                            query.Parameters[ItemIdTableTypeVariableName] = type;
    
                            using (DatabaseContext databaseContext = new DatabaseContext(context))
                            {
                                result = databaseContext.ReadEntity<ProductVariant>(query).Results;
                            }
                        }
                    }
                    else
                    {
                        result = new ReadOnlyCollection<ProductVariant>(new ProductVariant[0]);
                    }
    
                    updateL2Cache &= result != null
                        && result.Count < MaxCachedCollectionSize;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutVariantDimensionsByItemIds(inventoryDimensionIds, result);
                }
    
                return new EntityDataServiceResponse<ProductVariant>(result.AsPagedResult());
            }
    
            private static SingleEntityDataServiceResponse<ItemMaxRetailPriceIndia> GetItemMaxRetailPrice(GetItemMaxRetailPriceDataRequest request)
            {
                string itemId = request.ItemId;
                RequestContext context = request.RequestContext;
                ThrowIf.Null(itemId, "itemId");
                ThrowIf.Null(context, "context");
                ColumnSet columnSet = request.QueryResultSettings != null ? request.QueryResultSettings.ColumnSet : new ColumnSet();
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ItemMaxRetailPriceIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetItemMaxRetailPrice(itemId, columnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = columnSet,
                        From = ItemsMaxRetailPricesIndiaViewName,
                        Where = string.Format("{0} = {1} AND {2} = {3}", ItemIdColumnName, ItemIdVariableName, DataAreaIdColumnName, DataAreaIdVariableName),
                    };
    
                    query.Parameters[ItemIdVariableName] = itemId;
                    query.Parameters[DataAreaIdVariableName] = context.GetChannelConfiguration().InventLocationDataAreaId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(context))
                    {
                        result = databaseContext.ReadEntity<ItemMaxRetailPriceIndia>(query).SingleOrDefault();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutItemMaxRetailPrice(itemId, columnSet, result);
                }
    
                return new SingleEntityDataServiceResponse<ItemMaxRetailPriceIndia>(result);
            }
    
            private static EntityDataServiceResponse<RetailCategoryMember> GetRetailCategoryMembersForItems(GetRetailCategoryMembersForItemsDataRequest request)
            {
                RequestContext context = request.RequestContext;
                ISet<long> productOrVariantIds = request.ProductOrVariantIds;
                ThrowIf.Null(productOrVariantIds, "productOrVariantIds");
                ThrowIf.Null(context, "context");
    
                ItemL2CacheDataStoreAccessor level2CacheDataAccessor = GetCacheAccessor(context);
    
                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<RetailCategoryMember> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetRetailCategoryMembersForItems(productOrVariantIds), out found, out updateL2Cache);
    
                if (!found)
                {
                    SqlPagedQuery query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        Select = new ColumnSet(),
                        From = RetailCategoryMembersForItemsViewName
                    };
    
                    using (RecordIdTableType type = new RecordIdTableType(productOrVariantIds, CommerceEntityExtensions.GetColumnName<RetailCategoryMember>(e => e.ProductOrVariantId)))
                    {
                        query.Parameters[RecordIdTableTypeVariableName] = type;
    
                        using (DatabaseContext databaseContext = new DatabaseContext(context))
                        {
                            result = databaseContext.ReadEntity<RetailCategoryMember>(query).Results;
                        }
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutRetailCategoryMembersForItems(productOrVariantIds, result);
                }
    
                return new EntityDataServiceResponse<RetailCategoryMember>(result.AsPagedResult());
            }
        }
    }
}
