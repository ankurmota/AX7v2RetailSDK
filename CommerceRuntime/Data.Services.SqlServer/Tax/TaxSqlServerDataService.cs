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
        /// The tax SQL server data service.
        /// </summary>
        public class TaxSqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Indicates the maximum number of entries of a collection which can be cached.
            /// </summary>
            private const int MaxCachedCollectionSize = 500;

            private const string GetTaxRegimeSprocName = "GETTAXREGIME";
            private const string GetTaxCodeIntervalsSprocName = "GETTAXCODEINTERVALS";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetTaxCodeIntervalsDataRequest),
                        typeof(GetSalesTaxGroupDataRequest),
                    };
                }
            }

            /// <summary>
            /// Entry point to tax data service of the request execution.
            /// </summary>
            /// <param name="request">The data service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");

                Response response;

                if (request is GetTaxCodeIntervalsDataRequest)
                {
                    response = this.GetTaxCodeIntervals((GetTaxCodeIntervalsDataRequest)request);
                }
                else if (request is GetSalesTaxGroupDataRequest)
                {
                    response = this.GetSalesTaxGroup((GetSalesTaxGroupDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }

                return response;
            }
        
            /// <summary>
            /// The data service method to get tax code intervals.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<TaxCodeInterval> GetTaxCodeIntervals(GetTaxCodeIntervalsDataRequest request)
            {
                ThrowIf.Null(request.ItemTaxGroupId, "itemTaxGroupId");

                var taxDataManager = this.GetDataManagerInstance(request.RequestContext);
                TaxL2CacheDataStoreAccessor level2CacheDataAccessor = (TaxL2CacheDataStoreAccessor)taxDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];

                bool found;
                bool updateL2Cache;
                ReadOnlyCollection<TaxCodeInterval> result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetTaxCodeIntervals(request.SalesTaxGroupId, request.ItemTaxGroupId), out found, out updateL2Cache);

                if (!found)
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                    parameters["@nvc_SalesTaxGroup"] = request.SalesTaxGroupId ?? string.Empty;
                    parameters["@nvc_ItemTaxGroup"] = request.ItemTaxGroupId ?? string.Empty;
                    parameters["@dt_TransactionDate"] = request.TransactionDate.DateTime;

                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = databaseContext.ExecuteStoredProcedure<TaxCodeInterval>(GetTaxCodeIntervalsSprocName, parameters).Results;
                    }

                    updateL2Cache &= result != null
                                     && result.Count < MaxCachedCollectionSize;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutTaxCodeIntervals(request.SalesTaxGroupId, request.ItemTaxGroupId, request.TransactionDate, result);
                }

                return new EntityDataServiceResponse<TaxCodeInterval>(result.AsPagedResult());
            }

            /// <summary>
            /// The data service method to get the sales tax group for the given predicates.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<string> GetSalesTaxGroup(GetSalesTaxGroupDataRequest request)
            {
                ThrowIf.NullOrEmpty(request.Predicates, "predicates");

                var taxDataManager = this.GetDataManagerInstance(request.RequestContext);
                TaxL2CacheDataStoreAccessor level2CacheDataAccessor = (TaxL2CacheDataStoreAccessor)taxDataManager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];

                bool found;
                bool updateL2Cache;
                string result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetSalesTaxGroup(request.Predicates), out found, out updateL2Cache);

                if (!found)
                {
                    result = string.Empty;

                    ParameterSet parameters = new ParameterSet();

                    foreach (var predicate in request.Predicates)
                    {
                        parameters["nvc_" + predicate.Key] = predicate.Value;
                    }

                    SalesTaxGroup group;
                    using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                    {
                        group = databaseContext.ExecuteStoredProcedure<SalesTaxGroup>(GetTaxRegimeSprocName, parameters).FirstOrDefault();
                    }

                    if (group != null)
                    {
                        result = group.TaxGroupName;
                    }

                    updateL2Cache &= result != null;
                }

                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutSalesTaxGroup(request.Predicates, result);
                }

                return new SingleEntityDataServiceResponse<string>(result);
            }

            private TaxDataManager GetDataManagerInstance(RequestContext context)
            {
                return new TaxDataManager(context);
            }
        }
    }
}
