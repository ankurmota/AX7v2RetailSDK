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
        using Commerce.Runtime.TransactionService.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Availability real time service.
        /// </summary>
        public class AvailabilityTransactionService : IRequestHandler
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
                        typeof(GetStoreAvailabilityRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetStoreAvailabilityRealtimeRequest))
                {
                    response = GetStoreAvailability((GetStoreAvailabilityRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Looks up for inventory in AX by item and variant ids.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The collection of <see cref="ItemAvailabilityStore"/> items.</returns>
            private static EntityDataServiceResponse<ItemAvailabilityStore> GetStoreAvailability(GetStoreAvailabilityRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                ReadOnlyCollection<InventoryInfo> inventoryInfo = transactionClient.InventoryLookup(request.ItemId, request.VariantId);
    
                PagedResult<ItemAvailabilityStore> availabilities = inventoryInfo.Select(
                    info => new ItemAvailabilityStore
                    {
                        AvailableQuantity = decimal.Parse(info.InventoryAvailable, CultureInfo.InvariantCulture),
                        ItemId = info.ItemId,
                        InventoryLocationId = info.InventoryLocationId,
                        OrgUnitName = info.StoreName
                    }).AsPagedResult();
    
                return new EntityDataServiceResponse<ItemAvailabilityStore>(availabilities);
            }
        }
    }
}
