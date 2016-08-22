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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Tax data service class.
        /// </summary>
        public sealed class TaxSqliteDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetTaxCodeIntervalsDataRequest) };
                }
            }
    
            /// <summary>
            /// Entry point to tax data service.
            /// of the request execution.
            /// </summary>
            /// <param name="request">The shifts data service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                ThrowIf.Null(request, "request");
    
                Response response;
    
                if (request is GetTaxCodeIntervalsDataRequest)
                {
                    response = this.GetTaxCodeIntervals((GetTaxCodeIntervalsDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private EntityDataServiceResponse<TaxCodeInterval> GetTaxCodeIntervals(GetTaxCodeIntervalsDataRequest request)
            {
                GetTaxCodeIntervalsProcedure databaseAccessor = new GetTaxCodeIntervalsProcedure(request.RequestContext);
                PagedResult<TaxCodeInterval> taxCodeIntervals = databaseAccessor.GetTaxCodeIntervals(request.SalesTaxGroupId, request.ItemTaxGroupId, request.TransactionDate);
                return new EntityDataServiceResponse<TaxCodeInterval>(taxCodeIntervals);
            }
        }
    }
}
