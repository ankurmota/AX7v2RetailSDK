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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        ///  Product / Item availability SQL server data service class.
        /// </summary>
        public class ProductAvailabilitySqlServerDataService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetItemAvailabilitiesDataRequest),
                        typeof(ReleaseItemsDataRequest),
                        typeof(ReserveItemsDataRequest),
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
    
                if (requestType == typeof(GetItemAvailabilitiesDataRequest))
                {
                    response = this.GetItemAvailabilities((GetItemAvailabilitiesDataRequest)request);
                }
                else if (requestType == typeof(ReleaseItemsDataRequest))
                {
                    response = this.ReleaseItems((ReleaseItemsDataRequest)request);
                }
                else if (requestType == typeof(ReserveItemsDataRequest))
                {
                    response = this.ReserveItems((ReserveItemsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Get item availabilities by requested item quantities.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="Response"/> object.</returns>
            private Response GetItemAvailabilities(GetItemAvailabilitiesDataRequest request)
            {
                var dataManager = new ItemAvailabilityDataManager(request.RequestContext);
                PagedResult<ItemAvailability> itemAvailabilities = null;
    
                ItemAvailabilitiesQueryCriteria criteria = request.QueryCriteria;
                ThrowIf.Null(criteria, "request.QueryCriteria");
    
                if (criteria.MaxWarehousesPerItem.HasValue)
                {
                    // Get item availabilities by requested item quantities.
                    itemAvailabilities = dataManager.GetItemAvailabilitiesByItemQuantities(criteria.ItemQuantities, criteria.CustomerAccountNumber, criteria.MaxWarehousesPerItem.Value, request.QueryResultSettings);
                    return new EntityDataServiceResponse<ItemAvailability>(itemAvailabilities);
                }
    
                if (criteria.ItemWarehouses != null)
                {
                    // Get item availabilities by item and warehouse combinations.
                    itemAvailabilities = dataManager.GetItemAvailabilitiesByItemWarehouses(criteria.ItemWarehouses, request.QueryResultSettings);
                    return new EntityDataServiceResponse<ItemAvailability>(itemAvailabilities);
                }
    
                if (criteria.IncludeQuantities)
                {
                    // Get item available quantities by items.
                    PagedResult<ItemAvailableQuantity> itemAvailableQuantities = dataManager.GetItemAvailableQuantitiesByItems(criteria.Items, criteria.CustomerAccountNumber, request.QueryResultSettings);
                    return new EntityDataServiceResponse<ItemAvailableQuantity>(itemAvailableQuantities);
                }
    
                // Get item availabilities by items.
                itemAvailabilities = dataManager.GetItemAvailabilitiesByItems(criteria.Items, criteria.CustomerAccountNumber, request.QueryResultSettings);
                return new EntityDataServiceResponse<ItemAvailability>(itemAvailabilities);
            }
    
            /// <summary>
            /// Release items.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="NullResponse"/> object.</returns>
            private NullResponse ReleaseItems(ReleaseItemsDataRequest request)
            {
                var dataManager = new ItemAvailabilityDataManager(request.RequestContext);
                dataManager.ReleaseItems(request.ReservationIds);
                return new NullResponse();
            }
    
            /// <summary>
            /// Reserve items.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns><see cref="NullResponse"/> object.</returns>
            private NullResponse ReserveItems(ReserveItemsDataRequest request)
            {
                var dataManager = new ItemAvailabilityDataManager(request.RequestContext);
                dataManager.ReserveItems(request.ItemReservations);
                return new NullResponse();
            }
        }
    }
}
