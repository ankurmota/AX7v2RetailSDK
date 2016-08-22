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
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// The tax SQL server data service.
        /// </summary>
        public class ShippingDataService : IRequestHandler
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
                        typeof(GetAllDeliveryOptionsDataRequest),
                        typeof(GetShippingAdapterConfigurationDataRequest),
                        typeof(GetWarehouseDetailsDataRequest),
                        typeof(GetItemDimensionsDataRequest),
                        typeof(GetDeliveryOptionDataRequest),
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
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Response response;

                if (request is GetAllDeliveryOptionsDataRequest)
                {
                    response = this.GetAllDeliveryOptions((GetAllDeliveryOptionsDataRequest)request);
                }
                else if (request is GetShippingAdapterConfigurationDataRequest)
                {
                    response = this.GetShippingAdapterConfiguration((GetShippingAdapterConfigurationDataRequest)request);
                }
                else if (request is GetWarehouseDetailsDataRequest)
                {
                    response = this.GetWarehouseDetails((GetWarehouseDetailsDataRequest)request);
                }
                else if (request is GetItemDimensionsDataRequest)
                {
                    response = this.GetItemDimensions((GetItemDimensionsDataRequest)request);
                }
                else if (request is GetDeliveryOptionDataRequest)
                {
                    response = this.GetDeliveryOption((GetDeliveryOptionDataRequest)request);
                }
                else
                {
                    string message = string.Format("Request type '{0}' is not supported", request.GetType().FullName);
                    throw new NotSupportedException(message);
                }
    
                return response;
            }
    
            /// <summary>
            /// Get all delivery options.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the delivery options.</returns>
            private EntityDataServiceResponse<DeliveryOption> GetAllDeliveryOptions(GetAllDeliveryOptionsDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var deliveryOptions = shippingDataManager.GetAllDeliveryOptions();
                return new EntityDataServiceResponse<DeliveryOption>(deliveryOptions);
            }
    
            /// <summary>
            /// Gets the delivery option.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the delivery option.</returns>
            private EntityDataServiceResponse<DeliveryOption> GetDeliveryOption(GetDeliveryOptionDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var deliveryOption = shippingDataManager.GetDeliveryOption(request.Code, request.QueryResultSettings.ColumnSet);
                var deliveryOptionList = new List<DeliveryOption>() { deliveryOption };
                var deliveryOptions = new ReadOnlyCollection<DeliveryOption>(deliveryOptionList).AsPagedResult();
                return new EntityDataServiceResponse<DeliveryOption>(deliveryOptions);
            }
    
            /// <summary>
            /// Gets the shipping adapter configuration.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the shipping adapter configuration.</returns>
            private EntityDataServiceResponse<ShippingAdapterConfig> GetShippingAdapterConfiguration(GetShippingAdapterConfigurationDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var adapterConfiguration = shippingDataManager.GetShippingAdapterConfiguration(request.DeliveryModeIds, request.QueryResultSettings.ColumnSet);
                return new EntityDataServiceResponse<ShippingAdapterConfig>(adapterConfiguration);
            }
    
            /// <summary>
            /// Gets the warehouse details.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the warehouse details.</returns>
            private EntityDataServiceResponse<WarehouseDetails> GetWarehouseDetails(GetWarehouseDetailsDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var warehouseDetails = shippingDataManager.GetWarehouseDetails(request.WarehouseIds, request.QueryResultSettings.ColumnSet);
                return new EntityDataServiceResponse<WarehouseDetails>(warehouseDetails);
            }
    
            /// <summary>
            /// Gets the item dimensions.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response with the item dimensions.</returns>
            private EntityDataServiceResponse<ItemDimensions> GetItemDimensions(GetItemDimensionsDataRequest request)
            {
                var shippingDataManager = this.GetDataManagerInstance(request.RequestContext);
                var itemDimensions = shippingDataManager.GetItemDimensions(request.ItemIds, request.QueryResultSettings.ColumnSet);
                return new EntityDataServiceResponse<ItemDimensions>(itemDimensions);
            }
    
            /// <summary>
            /// Gets the data manager instance.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The shipping data manager instance.</returns>
            private ShippingDataManager GetDataManagerInstance(RequestContext context)
            {
                return new ShippingDataManager(context);
            }
        }
    }
}
