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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// SQL server data request handler for India tax.
        /// </summary>
        public class IndiaTaxSqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Indicates the maximum number of entries of a collection which can be cached.
            /// </summary>
            private const int MaxCachedCollectionSize = 500;

            private const string GetReceiptHeaderTaxInfoIndiaName = "GETRECEIPTHEADERTAXINFOINDIA";
            private const string GetWarehouseAddressIndiaName = "GETWAREHOUSEADDRESSINDIA";
            private const string GetTaxCodeIntervalsIndiaSprocName = "GETTAXCODEINTERVALSINDIA";
            private const string GetTaxRegimeIndiaName = "GETTAXREGIMEINDIA";
            private const string GetApplyInterStateTaxIndiaName = "GETAPPLYINTERSTATETAXINDIA";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetReceiptHeaderTaxInfoIndiaDataRequest),
                        typeof(GetWarehouseAddressIndiaDataRequest),
                        typeof(GetTaxRegimeIndiaDataRequest),
                        typeof(GetTaxCodeIntervalsIndiaDataRequest),
                        typeof(GetApplyInterstateTaxIndiaDataRequest),
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

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(GetReceiptHeaderTaxInfoIndiaDataRequest))
                {
                    response = this.GetReceiptHeaderTaxInfoIndia((GetReceiptHeaderTaxInfoIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetWarehouseAddressIndiaDataRequest))
                {
                    response = this.GetWarehouseAddressIndia((GetWarehouseAddressIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetTaxRegimeIndiaDataRequest))
                {
                    response = this.GetTaxRegimeIndia((GetTaxRegimeIndiaDataRequest)request);
                }
                else if (request is GetTaxCodeIntervalsIndiaDataRequest)
                {
                    response = this.GetTaxCodeIntervalsIndia((GetTaxCodeIntervalsIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetApplyInterstateTaxIndiaDataRequest))
                {
                    response = this.GetApplyInterstateTaxIndia((GetApplyInterstateTaxIndiaDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets the setting that shows whether to apply inter-state tax for India or not.
            /// </summary>
            /// <param name="request">The get apply interstate tax India data request.</param>
            /// <returns>The setting that shows whether to apply inter-state tax for India or not.</returns>
            private SingleEntityDataServiceResponse<ApplyInterStateTaxIndia> GetApplyInterstateTaxIndia(GetApplyInterstateTaxIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");

                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                ApplyInterStateTaxIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetApplyInterStateTaxIndia(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);

                if (!found)
                {
                    // Fetches it from database if no setting was cached.
                    // Input parameters
                    ParameterSet parameters = new ParameterSet();
                    parameters["@bi_ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;

                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = sqlServerDatabaseContext.ExecuteStoredProcedure<ApplyInterStateTaxIndia>(GetApplyInterStateTaxIndiaName, parameters).SingleOrDefault();
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    // Caches the result if it is fetched from database.
                    level2CacheDataAccessor.PutApplyInterStateTaxIndia(request.QueryResultSettings.ColumnSet, result);
                }

                return new SingleEntityDataServiceResponse<ApplyInterStateTaxIndia>(result);
            }

            /// <summary>
            /// Gets inter-state tax group for India.
            /// </summary>
            /// <param name="request">The get tax regime India data request.</param>
            /// <returns>The inter-state tax group.</returns>
            private SingleEntityDataServiceResponse<string> GetTaxRegimeIndia(GetTaxRegimeIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");

                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                string result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetTaxRegimeIndia(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);

                if (!found)
                {
                    // Input parameters
                    ParameterSet parameters = new ParameterSet();
                    parameters["@bi_ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;

                    SalesTaxGroup group;
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        group = sqlServerDatabaseContext.ExecuteStoredProcedure<SalesTaxGroup>(GetTaxRegimeIndiaName, parameters).SingleOrDefault();
                    }

                    if (group != null)
                    {
                        result = group.TaxGroupName;
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutTaxRegimeIndia(request.QueryResultSettings.ColumnSet, result);
                }

                return new SingleEntityDataServiceResponse<string>(result);
            }
        
            /// <summary>
            /// The data service method to get tax code intervals for India.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<TaxCodeInterval> GetTaxCodeIntervalsIndia(GetTaxCodeIntervalsIndiaDataRequest request)
            {
                ThrowIf.Null(request.ItemTaxGroupId, "itemTaxGroupId");

                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<TaxCodeInterval> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetTaxCodeIntervalsIndia(request.SalesTaxGroupId, request.ItemTaxGroupId, request.TransactionDate), out found, out updateL2Cache);

                if (!found)
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                    parameters["@nvc_SalesTaxGroup"] = request.SalesTaxGroupId ?? string.Empty;
                    parameters["@nvc_ItemTaxGroup"] = request.ItemTaxGroupId ?? string.Empty;
                    parameters["@dt_TransactionDate"] = request.TransactionDate.DateTime;

                    ReadOnlyCollection<TaxCodeIntervalIndia> taxCodeIntervalsIndia;
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        taxCodeIntervalsIndia = databaseContext.ExecuteStoredProcedure<TaxCodeIntervalIndia>(GetTaxCodeIntervalsIndiaSprocName, parameters).Results;
                    }

                    result = taxCodeIntervalsIndia.Cast<TaxCodeInterval>().AsReadOnly();

                    updateL2Cache &= result != null
                                     && result.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutTaxCodeIntervalsIndia(request.SalesTaxGroupId, request.ItemTaxGroupId, request.TransactionDate, result);
                }

                return new EntityDataServiceResponse<TaxCodeInterval>(result.AsPagedResult());
            }

            /// <summary>
            /// Gets a default logistics postal address for India.
            /// </summary>
            /// <param name="request">The get warehouse address India data request.</param>
            /// <returns>Matching address.</returns>
            /// <remarks>
            /// Search address as warehouse -> site -> legal entity.
            /// </remarks>
            private SingleEntityDataServiceResponse<Address> GetWarehouseAddressIndia(GetWarehouseAddressIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.WarehouseId, "request.WarehouseId");

                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);

                bool found;
                bool updateL2Cache;
                Address result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetWarehouseAddressIndia(request.WarehouseId), out found, out updateL2Cache);

                if (!found)
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@nv_InventLocationId"] = request.WarehouseId;
                    parameters["@nv_InventLocationDataAreaId"] = request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId;

                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = sqlServerDatabaseContext.ExecuteStoredProcedure<Address>(GetWarehouseAddressIndiaName, parameters).SingleOrDefault();
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutWarehouseAddressIndia(request.WarehouseId, result);
                }

                return new SingleEntityDataServiceResponse<Address>(result);
            }

            /// <summary>
            /// Gets Receipt Header tax information for India.
            /// </summary>
            /// <param name="request">The get receipt header tax info India data request.</param>
            /// <returns>Receipt Header tax info.</returns>
            private SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia> GetReceiptHeaderTaxInfoIndia(GetReceiptHeaderTaxInfoIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");

                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);
                bool found;
                bool updateL2Cache;
                ReceiptHeaderTaxInfoIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReceiptHeaderTaxInfoIndia(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);

                if (!found)
                {
                    ParameterSet parameters = new ParameterSet();

                    parameters["@bi_ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;

                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = sqlServerDatabaseContext.ExecuteStoredProcedure<ReceiptHeaderTaxInfoIndia>(GetReceiptHeaderTaxInfoIndiaName, parameters).SingleOrDefault();
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReceiptHeaderTaxInfoIndia(request.QueryResultSettings.ColumnSet, result);
                }

                return new SingleEntityDataServiceResponse<ReceiptHeaderTaxInfoIndia>(result);
            }

            /// <summary>
            /// Gets the cache accessor for the India tax data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="ChannelL2CacheDataStoreAccessor"/> class.</returns>
            private IndiaTaxL2CacheDataStoreAccessor GetIndiaTaxL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new IndiaTaxL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
