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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The shipping SQL server data service.
        /// </summary>
        public class ShippingSqlServerDataService : IRequestHandler
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
                        typeof(CreateOrUpdateShipmentStatusDataRequest),
                        typeof(GetItemDeliveryOptionsDataRequest),
                        typeof(GetLineDeliveryOptionsDataRequest),
                        typeof(GetDeliveryPreferencesDataRequest),
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
    
                if (request is CreateOrUpdateShipmentStatusDataRequest)
                {
                    response = this.CreateOrUpdateShipmentStatus((CreateOrUpdateShipmentStatusDataRequest)request);
                }
                else if (request is GetItemDeliveryOptionsDataRequest)
                {
                    response = this.GetItemDeliveryOptions((GetItemDeliveryOptionsDataRequest)request);
                }
                else if (request is GetLineDeliveryOptionsDataRequest)
                {
                    response = this.GetLineDeliveryOptions((GetLineDeliveryOptionsDataRequest)request);
                }
                else if (request is GetDeliveryPreferencesDataRequest)
                {
                    response = this.GetLineDeliveryPreferences((GetDeliveryPreferencesDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            /// <summary>
            /// Creates or updates the shipment status.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private NullResponse CreateOrUpdateShipmentStatus(CreateOrUpdateShipmentStatusDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                shippingDataManager.CreateOrUpdateShipmentStatus(request.ShipmentPublishingStatuses);
                return new NullResponse();
            }
    
            /// <summary>
            /// Gets the line delivery options.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the line delivery options.</returns>
            private EntityDataServiceResponse<SalesLineDeliveryOption> GetLineDeliveryOptions(GetLineDeliveryOptionsDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                PagedResult<SalesLineDeliveryOption> salesLineDeliveryOptions = shippingDataManager.GetLineDeliveryOptions(request.SalesLines, request.QueryResultSettings);
                return new EntityDataServiceResponse<SalesLineDeliveryOption>(salesLineDeliveryOptions);
            }
    
            /// <summary>
            /// Gets the delivery preferences for each sales line in the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response with the line delivery preferences.
            /// </returns>
            private EntityDataServiceResponse<CartLineDeliveryPreference> GetLineDeliveryPreferences(GetDeliveryPreferencesDataRequest request)
            {
                ShippingDataManager shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                ReadOnlyCollection<CartLineDeliveryPreference> salesLineDeliveryPreferences = shippingDataManager.GetLineDeliveryPreferences(request.SalesLines, request.QueryResultSettings);
                return new EntityDataServiceResponse<CartLineDeliveryPreference>(salesLineDeliveryPreferences.AsPagedResult());
            }
    
            /// <summary>
            /// Gets the item delivery options.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the delivery options.</returns>
            private EntityDataServiceResponse<DeliveryOption> GetItemDeliveryOptions(GetItemDeliveryOptionsDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var deliveryOptions = shippingDataManager.GetItemDeliveryOptions(request.ItemId, request.VariantInventoryDimensionId, request.CountryRegionId, request.StateId, request.QueryResultSettings);
                return new EntityDataServiceResponse<DeliveryOption>(deliveryOptions);
            }
    
            /// <summary>
            /// Gets the data manager instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The shipping data manager.</returns>
            private ShippingDataManager GetDataManagerInstance(RequestContext context)
            {
                return new ShippingDataManager(context);
            }
        }
    }
}
