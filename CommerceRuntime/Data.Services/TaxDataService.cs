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
        using Microsoft.Dynamics.Commerce.Runtime.Data.Types;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Channel data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class TaxDataService : IRequestHandler
        {
            private const string RetailTransactionTaxTransView = "RETAILTRANSACTIONTAXTRANSVIEW";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof(GetTaxOverridesDataRequest),
                        typeof(GetTaxOverrideDetailsDataRequest),
                        typeof(GetSalesTaxGroupsDataRequest),
                        typeof(GetTaxCodeFormulaIndiaDataRequest),
                        typeof(GetTaxParameterDataRequest),
                        typeof(GetTaxLinesDataRequest),
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
    
                if (requestType == typeof(GetTaxOverridesDataRequest))
                {
                    response = this.GetTaxOverrides((GetTaxOverridesDataRequest)request);
                }
                else if (requestType == typeof(GetTaxOverrideDetailsDataRequest))
                {
                    response = this.GetTaxOverrideDetails((GetTaxOverrideDetailsDataRequest)request);
                }
                else if (requestType == typeof(GetSalesTaxGroupsDataRequest))
                {
                    response = this.GetSalesTaxGroups((GetSalesTaxGroupsDataRequest)request);
                }
                else if (requestType == typeof(GetTaxLinesDataRequest))
                {
                    response = this.GetTaxLines((GetTaxLinesDataRequest)request);
                }
                else if (requestType == typeof(GetTaxCodeFormulaIndiaDataRequest))
                {
                    response = this.GetTaxCodeFormulaIndia((GetTaxCodeFormulaIndiaDataRequest)request);
                }
                else if (requestType == typeof(GetTaxParameterDataRequest))
                {
                    response = this.GetTaxParameter((GetTaxParameterDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// The data service method to execute the data manager to get tax override details.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<TaxOverride> GetTaxOverrideDetails(GetTaxOverrideDetailsDataRequest request)
            {
                var taxOverride = new TaxDataManager(request.RequestContext).GetTaxOverrideDetails(request.TaxOverrideCode, request.QueryResultSettings.ColumnSet);
                return new SingleEntityDataServiceResponse<TaxOverride>(taxOverride);
            }
    
            /// <summary>
            /// The data service method to execute the data manager to get tax overrides.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<TaxOverride> GetTaxOverrides(GetTaxOverridesDataRequest request)
            {
                var taxOverrides = new TaxDataManager(request.RequestContext).GetTaxOverrides(request.OverrideBy, request.ChannelId, request.QueryResultSettings.ColumnSet);
                return new EntityDataServiceResponse<TaxOverride>(taxOverrides.AsPagedResult());
            }
    
            /// <summary>
            /// The data service method to execute the data manager to get the sales tax groups.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<SalesTaxGroup> GetSalesTaxGroups(GetSalesTaxGroupsDataRequest request)
            {
                var salesTaxGroups = new TaxDataManager(request.RequestContext).GetSalesTaxGroups(request.QueryResultSettings);
                return new EntityDataServiceResponse<SalesTaxGroup>(salesTaxGroups);
            }
    
            /// <summary>
            /// The data service method to execute the data manager to get the tax code formula for India.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<FormulaIndia> GetTaxCodeFormulaIndia(GetTaxCodeFormulaIndiaDataRequest request)
            {
                FormulaIndia taxCodeFormula = new TaxDataManager(request.RequestContext).GetTaxFormulaIndia(request.ItemTaxGroupId, request.TaxCode);
                return new SingleEntityDataServiceResponse<FormulaIndia>(taxCodeFormula);
            }
    
            /// <summary>
            /// The data service method to execute the data manager to get the tax parameter.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<TaxParameters> GetTaxParameter(GetTaxParameterDataRequest request)
            {
                TaxParameters taxParameter = new TaxDataManager(request.RequestContext).GetTaxParameter(request.QueryResultSettings);
                return new SingleEntityDataServiceResponse<TaxParameters>(taxParameter);
            }

            private EntityDataServiceResponse<TaxLine> GetTaxLines(GetTaxLinesDataRequest request)
            {
                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                using (StringIdTableType transactionIdsTableType = new StringIdTableType(request.TransactionIds, TaxLine.TransactionIdColumn))
                {
                    var query = new SqlPagedQuery(QueryResultSettings.AllRecords)
                    {
                        From = RetailTransactionTaxTransView
                    };

                    query.Parameters["@TVP_TABLETYPE"] = transactionIdsTableType;

                    PagedResult<TaxLine> results = databaseContext.ReadEntity<TaxLine>(query);
                    return new EntityDataServiceResponse<TaxLine>(results);
                }
            }
        }
    }
}
