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
        public sealed class ProductDataService : IRequestHandler
        {
            private const string ListingPublishStatusViewName = "LISTINGPUBLISHSTATUSVIEW";
            private const string ProductColumnName = "PRODUCT";
    
            private static readonly Type[] SupportedRequestTypesArray = new Type[]
            {
                typeof(GetListingPublishStatusesDataRequest)
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
    
                if (request is GetListingPublishStatusesDataRequest)
                {
                    response = GetListingPublishStatuses((GetListingPublishStatusesDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            private static EntityDataServiceResponse<ListingPublishStatus> GetListingPublishStatuses(GetListingPublishStatusesDataRequest request)
            {
                var productIds = request.ListingIds;
                ThrowIf.Null(productIds, "productIds");
    
                var settings = QueryResultSettings.AllRecords;
    
                var query = new SqlPagedQuery(settings)
                {
                    Select = request.QueryResultSettings.ColumnSet,
                    From = ListingPublishStatusViewName,
                };
    
                PagedResult<ListingPublishStatus> listingPublishStatuses;
                using (RecordIdTableType type = new RecordIdTableType(productIds, ProductColumnName))
                {
                    query.Parameters["@TVP_RECORDIDTABLETYPE"] = type;
    
                    using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                    {
                        listingPublishStatuses = databaseContext.ReadEntity<ListingPublishStatus>(query);
                    }
                }
    
                return new EntityDataServiceResponse<ListingPublishStatus>(listingPublishStatuses);
            }
        }
    }
}
