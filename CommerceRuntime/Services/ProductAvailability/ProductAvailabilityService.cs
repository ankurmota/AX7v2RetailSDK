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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Implementation for product availability service.
        /// </summary>
        public class ProductAvailabilityService : IRequestHandler
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
                        typeof(GetStoreAvailabilityServiceRequest),
                        typeof(GetItemAvailabilitiesByItemsServiceRequest),
                        typeof(GetItemAvailabilitiesByItemQuantitiesServiceRequest),
                        typeof(GetItemAvailabilitiesByItemWarehousesServiceRequest),
                        typeof(GetItemAvailableQuantitiesByItemsServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Entry point to Availability service. Takes a Availability service request and returns the result of the request execution.
            /// </summary>
            /// <param name="request">The Availability service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetItemAvailabilitiesByItemsServiceRequest))
                {
                    response = GetItemAvailabilitiesByItems((GetItemAvailabilitiesByItemsServiceRequest)request);
                }
                else if (requestType == typeof(GetItemAvailabilitiesByItemQuantitiesServiceRequest))
                {
                    response = GetItemAvailabilitiesByItemQuantities((GetItemAvailabilitiesByItemQuantitiesServiceRequest)request);
                }
                else if (requestType == typeof(GetItemAvailabilitiesByItemWarehousesServiceRequest))
                {
                    response = GetItemAvailabilitiesByItemWarehouses((GetItemAvailabilitiesByItemWarehousesServiceRequest)request);
                }
                else if (requestType == typeof(GetItemAvailableQuantitiesByItemsServiceRequest))
                {
                    response = GetItemAvailableQuantitiesByItems((GetItemAvailableQuantitiesByItemsServiceRequest)request);
                }
                else if (requestType == typeof(GetStoreAvailabilityServiceRequest))
                {
                    response = GetStoreAvailability((GetStoreAvailabilityServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static GetStoreAvailabilityServiceResponse GetStoreAvailability(GetStoreAvailabilityServiceRequest serviceRequest)
            {
                var getStoreAvailabilityRealtimeRequest = new GetStoreAvailabilityRealtimeRequest(serviceRequest.ItemId, serviceRequest.VariantId);
                ReadOnlyCollection<ItemAvailabilityStore> availabilities = serviceRequest.RequestContext.Execute<EntityDataServiceResponse<ItemAvailabilityStore>>(getStoreAvailabilityRealtimeRequest).PagedEntityCollection.Results;
                return new GetStoreAvailabilityServiceResponse(availabilities);
            }
    
            private static GetItemAvailabilitiesByItemsServiceResponse GetItemAvailabilitiesByItems(GetItemAvailabilitiesByItemsServiceRequest request)
            {
                var settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
                PagedResult<ItemAvailability> itemAvailabilities = null;
    
                try
                {
                    var searchCriteria = new ItemAvailabilitiesQueryCriteria(request.CustomerAccountNumber, request.Items, includeQuantities: false);
                    var dataRequest = new GetItemAvailabilitiesDataRequest(searchCriteria, settings);
                    itemAvailabilities = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ItemAvailability>>(dataRequest, request.RequestContext).PagedEntityCollection;
                }
                catch (StorageException)
                {
                    // supress data service exception in order not to block the order
                    // raise the notification instead
                    UnableToDetermineQuantityNotification notification = new UnableToDetermineQuantityNotification(request.Items);
                    request.RequestContext.Notify(notification);
                }
    
                return new GetItemAvailabilitiesByItemsServiceResponse(itemAvailabilities);
            }
    
            private static GetItemAvailabilitiesByItemQuantitiesServiceResponse GetItemAvailabilitiesByItemQuantities(GetItemAvailabilitiesByItemQuantitiesServiceRequest request)
            {
                var settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
                PagedResult<ItemAvailability> itemAvailabilities = null;
    
                try
                {
                    var searchCriteria = new ItemAvailabilitiesQueryCriteria(request.ItemQuantities, request.CustomerAccountNumber, request.MaxWarehousesPerItem);
                    var dataRequest = new GetItemAvailabilitiesDataRequest(searchCriteria, settings);
                    itemAvailabilities = request.RequestContext.Execute<EntityDataServiceResponse<ItemAvailability>>(dataRequest).PagedEntityCollection;
                }
                catch (StorageException)
                {
                    // supress exception in order not to block the order
                    // raise the notification instead
                    UnableToDetermineQuantityNotification notification = new UnableToDetermineQuantityNotification(request.ItemQuantities.Select(itemQuantity => itemQuantity.GetItem()));
                    request.RequestContext.Notify(notification);
                }
    
                return new GetItemAvailabilitiesByItemQuantitiesServiceResponse(itemAvailabilities);
            }
    
            private static GetItemAvailableQuantitiesByItemsServiceResponse GetItemAvailableQuantitiesByItems(GetItemAvailableQuantitiesByItemsServiceRequest request)
            {
                var settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
                PagedResult<ItemAvailableQuantity> itemAvailableQuantities = null;
    
                try
                {
                    var searchCriteria = new ItemAvailabilitiesQueryCriteria(request.CustomerAccountNumber, request.Items, includeQuantities: true);
                    var dataRequest = new GetItemAvailabilitiesDataRequest(searchCriteria, settings);
                    itemAvailableQuantities = request.RequestContext.Execute<EntityDataServiceResponse<ItemAvailableQuantity>>(dataRequest).PagedEntityCollection;
                }
                catch (StorageException)
                {
                    // supress exception in order to not block the order
                    // raise the notification instead
                    UnableToDetermineQuantityNotification notification = new UnableToDetermineQuantityNotification(request.Items);
                    request.RequestContext.Notify(notification);
                }
    
                return new GetItemAvailableQuantitiesByItemsServiceResponse(itemAvailableQuantities);
            }
    
            private static GetItemAvailabilitiesByItemWarehousesServiceResponse GetItemAvailabilitiesByItemWarehouses(GetItemAvailabilitiesByItemWarehousesServiceRequest request)
            {
                QueryResultSettings settings = request.QueryResultSettings ?? QueryResultSettings.AllRecords;
                PagedResult<ItemAvailability> itemAvailabilities = null;
    
                try
                {
                    var searchCriteria = new ItemAvailabilitiesQueryCriteria(request.ItemWarehouses);
                    var dataRequest = new GetItemAvailabilitiesDataRequest(searchCriteria, settings);
                    itemAvailabilities = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<ItemAvailability>>(dataRequest, request.RequestContext).PagedEntityCollection;
                }
                catch (StorageException)
                {
                    // supress exception in order not to block the order
                    // raise the notification instead
                    UnableToDetermineQuantityForStoresNotification notification = new UnableToDetermineQuantityForStoresNotification(request.ItemWarehouses);
                    request.RequestContext.Notify(notification);
                }
    
                return new GetItemAvailabilitiesByItemWarehousesServiceResponse(itemAvailabilities);
            }
        }
    }
}
