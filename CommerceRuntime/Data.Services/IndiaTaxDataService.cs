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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Common data request handler for India tax.
        /// </summary>
        public class IndiaTaxDataService : IRequestHandler
        {
            private const string TaxInformationLegalEntitiesIndiaViewName = "TAXINFORMATIONLEGALENTITIESINDIAVIEW";
            private const string RetailStoreTableIndiaViewName = "RETAILSTORETABLEINDIAVIEW";
            private const string TaxComponentTableIndiaViewName = "TAXCOMPONENTTABLEINDIAVIEW";
    
            private const string RecIdColumn = "RECID";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetReceiptHeaderInfoIndiaDataRequest),
                        typeof(GetTaxSummarySettingIndiaDataRequest),
                        typeof(GetTaxComponentIndiaDataRequest),
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
    
                if (requestType == typeof(GetReceiptHeaderInfoIndiaDataRequest))
                {
                    response = this.GetReceiptHeaderInfoIndia((GetReceiptHeaderInfoIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetTaxSummarySettingIndiaDataRequest))
                {
                    response = this.GetTaxSummarySettingIndia((GetTaxSummarySettingIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetTaxComponentIndiaDataRequest))
                {
                    response = this.GetTaxComponentIndia((GetTaxComponentIndiaDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets Tax component.
            /// </summary>
            /// <param name="request">The get tax component India data request.</param>
            /// <returns>Tax component.</returns>
            private SingleEntityDataServiceResponse<TaxComponentIndia> GetTaxComponentIndia(GetTaxComponentIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.TaxCode, "request.TaxCode");
    
                bool found;
                bool updateL2Cache;
                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);
    
                TaxComponentIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetTaxComponentIndia(request.TaxCode), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = TaxComponentTableIndiaViewName,
                        Where = "TAXCODE = @id AND CHANNELID = @channelId",
                    };
    
                    query.Parameters["@id"] = request.TaxCode;
                    query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<TaxComponentIndia>(query).SingleOrDefault();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutTaxComponentIndia(request.TaxCode, result);
                }
    
                return new SingleEntityDataServiceResponse<TaxComponentIndia>(result);
            }
    
            /// <summary>
            /// Gets tax summary setting.
            /// </summary>
            /// <param name="request">The get tax summary setting India data request.</param>
            /// <returns>Tax summary setting.</returns>
            private SingleEntityDataServiceResponse<TaxSummarySettingIndia> GetTaxSummarySettingIndia(GetTaxSummarySettingIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");
    
                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                TaxSummarySettingIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetTaxSummarySettingIndia(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(QueryResultSettings.SingleRecord)
                    {
                        Select = request.QueryResultSettings.ColumnSet,
                        From = RetailStoreTableIndiaViewName,
                        Where = "CHANNELID = @channelId",
                    };
    
                    query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<TaxSummarySettingIndia>(query).SingleOrDefault();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutTaxSummarySettingIndia(request.QueryResultSettings.ColumnSet, result);
                }
    
                return new SingleEntityDataServiceResponse<TaxSummarySettingIndia>(result);
            }
    
            /// <summary>
            /// Gets Receipt Header information for India.
            /// </summary>
            /// <param name="request">The get receipt header info India data request.</param>
            /// <returns>Receipt Header information.</returns>
            private SingleEntityDataServiceResponse<ReceiptHeaderInfoIndia> GetReceiptHeaderInfoIndia(GetReceiptHeaderInfoIndiaDataRequest request)
            {
                ThrowIf.Null(request, "request");
    
                IndiaTaxL2CacheDataStoreAccessor level2CacheDataAccessor = this.GetIndiaTaxL2CacheDataStoreAccessor(request.RequestContext);
    
                bool found;
                bool updateL2Cache;
                ReceiptHeaderInfoIndia result = DataManager.GetDataFromCache(() => level2CacheDataAccessor.GetReceiptHeaderInfoIndia(request.QueryResultSettings.ColumnSet), out found, out updateL2Cache);
    
                if (!found)
                {
                    var query = new SqlPagedQuery(request.QueryResultSettings)
                    {
                        From = TaxInformationLegalEntitiesIndiaViewName,
                        Where = "CHANNELID = @channelId",
                    };
    
                    query.Parameters["@channelId"] = request.RequestContext.GetPrincipal().ChannelId;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        result = databaseContext.ReadEntity<ReceiptHeaderInfoIndia>(query).SingleOrDefault();
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutReceiptHeaderInfoIndia(request.QueryResultSettings.ColumnSet, result);
                }
    
                return new SingleEntityDataServiceResponse<ReceiptHeaderInfoIndia>(result);
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
