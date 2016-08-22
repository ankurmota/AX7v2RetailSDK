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
        /// Shipping Service Implementation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
        public sealed class ShippingService : IRequestHandler
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
                        typeof(GetOrderDeliveryOptionsServiceRequest),
                        typeof(GetLineDeliveryOptionsServiceRequest),
                        typeof(GetProductDeliveryOptionsServiceRequest),
                        typeof(GetDeliveryPreferencesServiceRequest),
                        typeof(GetExternalShippingRateServiceRequest),
                        typeof(ValidateShippingAddressServiceRequest),
                        typeof(GetShipmentsServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>
            /// The response object.
            /// </returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetOrderDeliveryOptionsServiceRequest))
                {
                    response = GetOrderDeliveryOptions((GetOrderDeliveryOptionsServiceRequest)request);
                }
                else if (requestType == typeof(GetLineDeliveryOptionsServiceRequest))
                {
                    response = GetLineDeliveryOptions((GetLineDeliveryOptionsServiceRequest)request);
                }
                else if (requestType == typeof(GetProductDeliveryOptionsServiceRequest))
                {
                    response = GetProductDeliveryOptions((GetProductDeliveryOptionsServiceRequest)request);
                }
                else if (requestType == typeof(GetDeliveryPreferencesServiceRequest))
                {
                    response = GetDeliveryPreferences((GetDeliveryPreferencesServiceRequest)request);
                }
                else if (requestType == typeof(GetExternalShippingRateServiceRequest))
                {
                    response = GetExternalShippingRate((GetExternalShippingRateServiceRequest)request);
                }
                else if (requestType == typeof(ValidateShippingAddressServiceRequest))
                {
                    response = ValidateShippingAddress((ValidateShippingAddressServiceRequest)request);
                }
                else if (requestType == typeof(GetShipmentsServiceRequest))
                {
                    response = GetShipments((GetShipmentsServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request));
                }
    
                return response;
            }
    
            /// <summary>
            /// Calculates the shipping rates for each group of sales lines.
            /// </summary>
            /// <param name="groupedSalesLines">The grouped sales lines.</param>
            /// <param name="warehouseAddresses">The warehouse addresses.</param>
            /// <param name="itemGrossWeights">The item gross weights.</param>
            /// <param name="shippingAdapterConfigRecords">The shipping adapter configuration records.</param>
            /// <param name="requestContext">The request context.</param>
            /// <returns>
            /// List of sales line ids with their corresponding shipping rates.
            /// </returns>
            internal static IEnumerable<SalesLineShippingRate> CalculateShippingRates(
                Dictionary<int, List<SalesLine>> groupedSalesLines,
                Dictionary<string, Address> warehouseAddresses,
                Dictionary<string, decimal> itemGrossWeights,
                IEnumerable<ShippingAdapterConfig> shippingAdapterConfigRecords,
                RequestContext requestContext)
            {
                List<SalesLineShippingRate> finalSalesLineShippingRatesList = new List<SalesLineShippingRate>();
    
                // Call shipping carrier once for each set of saleslines with common delivery mode, origin and destination addresses.
                foreach (KeyValuePair<int, List<SalesLine>> salesLineGroup in groupedSalesLines)
                {
                    var tempSalesLineShippingRateList = GetSalesLineShippingRatesPerGroup(
                        salesLineGroup.Value, warehouseAddresses, itemGrossWeights, shippingAdapterConfigRecords, requestContext);
    
                    finalSalesLineShippingRatesList.AddRange(tempSalesLineShippingRateList);
                }
    
                return finalSalesLineShippingRatesList;
            }

            /// <summary>
            /// Gets the delivery options that are applicable to entire SalesTransaction i.e., common for all the sales lines.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The matching delivery options.</returns>
            private static GetOrderDeliveryOptionsServiceResponse GetOrderDeliveryOptions(GetOrderDeliveryOptionsServiceRequest request)
            {
                if (request.SalesTransaction == null)
                {
                    throw new NotSupportedException("Sales transaction on the request cannot be null.");
                }

                if (request.SalesTransaction.ShippingAddress == null)
                {
                    throw new NotSupportedException("The shipping address on the sales transaction cannot be null if order level delivery options are being fetched.");
                }

                // Consider active lines only. Ignore voided lines.
                var salesLines = request.SalesTransaction.ActiveSalesLines;
                foreach (SalesLine salesLine in salesLines)
                {
                    salesLine.ShippingAddress = request.SalesTransaction.ShippingAddress;
                }

                var dataServiceRequest = new GetLineDeliveryOptionsDataRequest(salesLines);
                dataServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                var dataServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<SalesLineDeliveryOption>>(dataServiceRequest);

                IEnumerable<DeliveryOption> deliveryOptions = null;
                if (dataServiceResponse != null)
                {
                    var salesLineDeliveryOptions = dataServiceResponse.PagedEntityCollection.Results;
                    deliveryOptions = GetCommonDeliveryOptions(salesLineDeliveryOptions);
                }

                // Raise notification if no common delivery options were found.
                if (deliveryOptions == null || !deliveryOptions.Any())
                {
                    var notification = new EmptyOrderDeliveryOptionSetNotification(request.SalesTransaction.Id);
                    request.RequestContext.Notify(notification);
                }

                return new GetOrderDeliveryOptionsServiceResponse(deliveryOptions.AsPagedResult());
            }
    
            /// <summary>
            /// Gets the delivery options applicable for each sales line level.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The matching delivery options.</returns>
            private static GetLineDeliveryOptionsServiceResponse GetLineDeliveryOptions(GetLineDeliveryOptionsServiceRequest request)
            {
                if (request.SalesLines.IsNullOrEmpty())
                {
                    throw new NotSupportedException("A non-empty set of sales lines must be provided for computing line level delivery options");
                }

                if (request.SalesLines.Where(sl => sl.ShippingAddress == null).Any())
                {
                    throw new NotSupportedException("The shipping address should be set on each requested line when fetching line level delivery options.");
                }

                var dataServiceRequest = new GetLineDeliveryOptionsDataRequest(request.SalesLines);
                dataServiceRequest.QueryResultSettings = request.QueryResultSettings;
                var dataServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<SalesLineDeliveryOption>>(dataServiceRequest);
    
                var deliveryOptions = dataServiceResponse.PagedEntityCollection;
    
                // Group all lines identifiers without an associated delivery option.
                var salesLinesWithoutDeliveryOption = new Collection<string>();
                foreach (var saleLineDeliveryOption in deliveryOptions.Results)
                {
                    if (!saleLineDeliveryOption.DeliveryOptions.Any())
                    {
                        salesLinesWithoutDeliveryOption.Add(saleLineDeliveryOption.SalesLineId);
                    }
                }
    
                if (salesLinesWithoutDeliveryOption.Any())
                {
                    // Raise notification of an anomaly.
                    EmptyLineDeliveryOptionSetNotification notification = new EmptyLineDeliveryOptionSetNotification(salesLinesWithoutDeliveryOption);
                    request.RequestContext.Notify(notification);
                }
    
                return new GetLineDeliveryOptionsServiceResponse(deliveryOptions);
            }
    
            /// <summary>
            /// Gets the delivery preferences applicable for each sales line individually and combined.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The delivery preferences applicable to the request.</returns>
            private static GetDeliveryPreferencesServiceResponse GetDeliveryPreferences(GetDeliveryPreferencesServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.Null(request.CartId, "request.CartId");
    
                // Try to load the transaction
                GetCartsDataRequest getCartDataRequest = new GetCartsDataRequest(new CartSearchCriteria { CartId = request.CartId }, QueryResultSettings.SingleRecord);
                SalesTransaction salesTransaction = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<SalesTransaction>>(getCartDataRequest, request.RequestContext).PagedEntityCollection.SingleOrDefault();
    
                if (salesTransaction.ActiveSalesLines == null || !salesTransaction.ActiveSalesLines.Any())
                {
                    return new GetDeliveryPreferencesServiceResponse(new CartDeliveryPreferences());
                }
    
                var dataServiceRequest = new GetDeliveryPreferencesDataRequest(salesTransaction.ActiveSalesLines);
                dataServiceRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                EntityDataServiceResponse<CartLineDeliveryPreference> dataServiceResponse = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<CartLineDeliveryPreference>>(dataServiceRequest, request.RequestContext);
    
                ReadOnlyCollection<CartLineDeliveryPreference> salesLineDeliveryPreferences = dataServiceResponse.PagedEntityCollection.Results;
    
                IEnumerable<string> salesLineIdsWithoutDeliveryPreferences = salesLineDeliveryPreferences.Where(sl => (sl.DeliveryPreferenceTypes == null || !sl.DeliveryPreferenceTypes.Any())).Select(sl => sl.LineId);
                if (salesLineIdsWithoutDeliveryPreferences.Any())
                {
                    string lineIds = string.Join(" ", salesLineIdsWithoutDeliveryPreferences);
                    var message = string.Format("No delivery preferences could be retrieved for the sales line ids : {0}.", lineIds);
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindDeliveryPreferences, message);
                }
    
                IEnumerable<DeliveryPreferenceType> headerLevelDeliveryPreferences = GetHeaderLevelDeliveryPreferences(salesLineDeliveryPreferences);
                CartDeliveryPreferences cartDeliveryPreferences = new CartDeliveryPreferences(headerLevelDeliveryPreferences, salesLineDeliveryPreferences);
    
                return new GetDeliveryPreferencesServiceResponse(cartDeliveryPreferences);
            }
    
            /// <summary>
            /// Gets the header level delivery preferences.
            /// </summary>
            /// <param name="salesLineDeliveryPreferences">The sales line delivery preferences.</param>
            /// <returns>Delivery preferences that are applicable at the cart header level.</returns>
            private static IEnumerable<DeliveryPreferenceType> GetHeaderLevelDeliveryPreferences(IEnumerable<CartLineDeliveryPreference> salesLineDeliveryPreferences)
            {
                int salesLineCount = 0;
    
                // Extract the delivery preferences common across all the lines.
                Dictionary<DeliveryPreferenceType, int> deliveryPreferenceApplicabilityCount = new Dictionary<DeliveryPreferenceType, int>();
                foreach (CartLineDeliveryPreference salesLineDeliveryInfo in salesLineDeliveryPreferences)
                {
                    salesLineCount++;
    
                    foreach (var deliveryPreferenceType in salesLineDeliveryInfo.DeliveryPreferenceTypes.Distinct())
                    {
                        int deliveryPreferenceCount = 0;
                        if (deliveryPreferenceApplicabilityCount.TryGetValue(deliveryPreferenceType, out deliveryPreferenceCount))
                        {
                            deliveryPreferenceApplicabilityCount[deliveryPreferenceType] = deliveryPreferenceCount + 1;
                        }
                        else
                        {
                            deliveryPreferenceApplicabilityCount.Add(deliveryPreferenceType, 1);
                        }
                    }
                }
    
                List<DeliveryPreferenceType> commonDeliveryPreferences = deliveryPreferenceApplicabilityCount
                    .Where(kv => kv.Value == salesLineCount)
                    .Select(kv => kv.Key).ToList();
    
                if (salesLineCount > 1)
                {
                    commonDeliveryPreferences.Add(DeliveryPreferenceType.DeliverItemsIndividually);
                }
    
                return commonDeliveryPreferences;
            }
    
            /// <summary>
            /// Fetches the delivery options applicable for the item/inventDimId and Address.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Delivery options applicable.</returns>
            /// <remarks>This API is typically used to display the delivery options in Item details page.</remarks>
            private static GetProductDeliveryOptionsServiceResponse GetProductDeliveryOptions(GetProductDeliveryOptionsServiceRequest request)
            {
                if (request.ShippingAddress == null)
                {
                    throw new ArgumentNullException("request", "request.ShippingAddress");
                }
    
                var dataServiceRequest = new GetItemDeliveryOptionsDataRequest(request.ItemId, request.InventoryDimensionId, request.ShippingAddress.ThreeLetterISORegionName, request.ShippingAddress.State);
                dataServiceRequest.QueryResultSettings = request.QueryResultSettings;
                var dataServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<DeliveryOption>>(dataServiceRequest);
    
                var deliveryOptions = dataServiceResponse.PagedEntityCollection;
    
                // Raise notification if no delivery options are found.
                if (!deliveryOptions.Results.Any())
                {
                    EmptyProductDeliveryOptionSetNotification notification = new EmptyProductDeliveryOptionSetNotification(request.ShippingAddress, request.ItemId, request.InventoryDimensionId);
                    request.RequestContext.Notify(notification);
                }
    
                return new GetProductDeliveryOptionsServiceResponse(deliveryOptions);
            }
    
            /// <summary>
            /// Gets the shipping rate from the external carriers.
            /// </summary>
            /// <param name="request">Contains the sales line item details for which the shipping rate needs to be computed.</param>
            /// <returns>The response containing the shipping response.</returns>
            /// <remarks>
            /// No exception is thrown if the delivery mode identifier is empty for a sales line.
            /// </remarks>
            private static GetExternalShippingRateServiceResponse GetExternalShippingRate(GetExternalShippingRateServiceRequest request)
            {
                // Check each salesLine for:
                //  1) an item id (used for getting dimensions,
                //  2) delivery mode id (used for determining shipping adapter to use)
                //  3) InventoryLocationId (used to determine ShippFromAddress)
                //  4) ShippingAddress(use to determine ShipToAddress)
                // Create a list of item ids for contacting database for item dimensions
                List<string> itemIds = new List<string>();
    
                // Create a list of warehouse ids for contacting database for warehouse addresses
                List<string> warehouseIds = new List<string>();
    
                // Create a list of delivery mode ids for contacting database for adapter configurations
                List<string> deliverModeIds = new List<string>();
    
                // Divide sales lines into groups that have the same delivery mode id and origin and destination shipping addresses.
                Dictionary<int, List<SalesLine>> groupedSalesLines = new Dictionary<int, List<SalesLine>>();
    
                bool isValidSalesLine;
                Collection<string> linesWithoutDeliveryModeId = new Collection<string>();
                Collection<string> linesWithoutShippingAddress = new Collection<string>();
                Collection<string> linesWithoutInventoryLocationId = new Collection<string>();
    
                foreach (var salesLine in request.SalesLines)
                {
                    // Assume that a sales line is valid until found otherwise.
                    isValidSalesLine = true;
    
                    if (salesLine == null)
                    {
                        throw new ArgumentNullException("request", "salesLine is not set in sales lines");
                    }
    
                    if (string.IsNullOrWhiteSpace(salesLine.ItemId))
                    {
                        throw new ArgumentNullException("request", "salesLine.ItemId");
                    }
    
                    if (string.IsNullOrWhiteSpace(salesLine.InventoryLocationId))
                    {
                        linesWithoutInventoryLocationId.Add(salesLine.LineId);
                        isValidSalesLine = false;
                    }
    
                    if (string.IsNullOrWhiteSpace(salesLine.DeliveryMode))
                    {
                        linesWithoutDeliveryModeId.Add(salesLine.LineId);
                        isValidSalesLine = false;
                    }
    
                    if (salesLine.ShippingAddress == null)
                    {
                        linesWithoutShippingAddress.Add(salesLine.LineId);
                        isValidSalesLine = false;
                    }
    
                    if (isValidSalesLine)
                    {
                        itemIds.Add(salesLine.ItemId);
                        warehouseIds.Add(salesLine.InventoryLocationId);
                        deliverModeIds.Add(salesLine.DeliveryMode);
    
                        // Group all sales line with the same delivery mode, warehouse id and destination shipping address together
                        int key = (salesLine.DeliveryMode + salesLine.InventoryLocationId + salesLine.ShippingAddress.GetHashCode()).GetHashCode();
                        if (!groupedSalesLines.ContainsKey(key))
                        {
                            groupedSalesLines.Add(key, new List<SalesLine>());
                        }
    
                        groupedSalesLines[key].Add(salesLine);
                    }
                }
    
                RaiseNotificationForInvalidLines(request.RequestContext, linesWithoutDeliveryModeId, linesWithoutInventoryLocationId, linesWithoutShippingAddress);
    
                var getWarehouseDataRequest = new GetWarehouseDetailsDataRequest(warehouseIds, QueryResultSettings.AllRecords);
                var getWarehouseDataResponse = request.RequestContext.Execute<EntityDataServiceResponse<WarehouseDetails>>(getWarehouseDataRequest);
    
                var warehouseDetailsRecords = getWarehouseDataResponse.PagedEntityCollection.Results;
    
                Dictionary<string, Address> warehouseAddresses = warehouseDetailsRecords.ToDictionary(key => key.InventoryLocationId.ToUpperInvariant(), value => (Address)value);
    
                // Validate that a warehouse address was retrieved for each inventory location id.
                foreach (var warehouseId in warehouseIds)
                {
                    if (!warehouseAddresses.ContainsKey(warehouseId.ToUpperInvariant()))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidShippingAddress, string.Format(CultureInfo.InvariantCulture, "Address for inventory location id {0} could not be found", warehouseId));
                    }
                }
    
                var getItemDimensionsDataRequest = new GetItemDimensionsDataRequest(itemIds, QueryResultSettings.AllRecords);
                var getItemDimensionsDataResponse = request.RequestContext.Execute<EntityDataServiceResponse<ItemDimensions>>(getItemDimensionsDataRequest);
    
                var itemDimensionsRecords = getItemDimensionsDataResponse.PagedEntityCollection.Results;
                Dictionary<string, decimal> itemGrossWeights = itemDimensionsRecords.ToDictionary(key => key.ItemId.ToUpperInvariant(), value => value.GrossWeight);
    
                // Validate that a weight value was retrieved for each item id.
                foreach (var itemId in itemIds)
                {
                    if (!itemGrossWeights.ContainsKey(itemId.ToUpperInvariant()))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_GrossWeightForItemNotFound, string.Format(CultureInfo.InvariantCulture, "Gross Weight for item id {0} could not be found", itemId));
                    }
                }
    
                var shippingAdapterConfigDataRequest = new GetShippingAdapterConfigurationDataRequest(deliverModeIds, QueryResultSettings.AllRecords);
                var shippingAdapterConfigDataResponse = request.RequestContext.Execute<EntityDataServiceResponse<ShippingAdapterConfig>>(shippingAdapterConfigDataRequest);
    
                var shippingAdapterConfigRecords = shippingAdapterConfigDataResponse.PagedEntityCollection.Results;
    
                var salesLineShippingRates = CalculateShippingRates(
                    groupedSalesLines, warehouseAddresses, itemGrossWeights, shippingAdapterConfigRecords, request.RequestContext);
    
                return new GetExternalShippingRateServiceResponse(salesLineShippingRates.AsPagedResult());
            }
    
            /// <summary>
            /// Raises the notification for invalid lines.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="linesWithoutDeliveryModeId">The lines without a delivery mode identifier.</param>
            /// <param name="linesWithoutInventoryLocationId">The lines without an inventory location identifier.</param>
            /// <param name="linesWithoutShippingAddress">The lines without shipping address.</param>
            private static void RaiseNotificationForInvalidLines(RequestContext requestContext, IEnumerable<string> linesWithoutDeliveryModeId, IEnumerable<string> linesWithoutInventoryLocationId, IEnumerable<string> linesWithoutShippingAddress)
            {
                bool raiseNotification = false;
    
                if (linesWithoutDeliveryModeId.Any())
                {
                    raiseNotification = true;
                }
    
                if (linesWithoutInventoryLocationId.Any())
                {
                    raiseNotification = true;
                }
    
                if (linesWithoutShippingAddress.Any())
                {
                    raiseNotification = true;
                }
    
                if (raiseNotification)
                {
                    var notification = new MissingLineShippingInfoNotification(linesWithoutDeliveryModeId, linesWithoutInventoryLocationId, linesWithoutShippingAddress);
                    requestContext.Notify(notification);
                }
            }
    
            /// <summary>
            /// Gets the sales line shipping rates for sales line group.
            /// </summary>
            /// <param name="salesLineGroup">The sales line group.</param>
            /// <param name="warehouseAddresses">The warehouse addresses.</param>
            /// <param name="itemGrossWeights">The item gross weights.</param>
            /// <param name="shippingAdapterConfigRecords">The shipping adapter configuration records.</param>
            /// <param name="requestContext">The request context.</param>
            /// <returns>
            /// An enumeration of sales line ids with corresponding shipping rates and weights.
            /// </returns>
            private static IEnumerable<SalesLineShippingRate> GetSalesLineShippingRatesPerGroup(
                IEnumerable<SalesLine> salesLineGroup,
                Dictionary<string, Address> warehouseAddresses,
                Dictionary<string, decimal> itemGrossWeights,
                IEnumerable<ShippingAdapterConfig> shippingAdapterConfigRecords,
                RequestContext requestContext)
            {
                // We package all the items with identical dlv mode, origin and destination addresses into one package.
                // Total weight of all items in current saleslines group
                decimal totalWeight;
    
                Address shippingDestination = null;
                Address shippingOrigin = null;
                string deliveryModeId = null;
    
                // Since destination, origin and delivery mode are identical for all lines within a group, we can grab values from any member of the list.
                // Does not matter if it is the first element, or the last, or anywhere in the middle.
                if (salesLineGroup != null && salesLineGroup.Any())
                {
                    shippingDestination = salesLineGroup.First().ShippingAddress;
                    shippingOrigin = warehouseAddresses[salesLineGroup.First().InventoryLocationId.ToUpperInvariant()];
                    deliveryModeId = salesLineGroup.First().DeliveryMode;
                }
    
                var salesLineShippingRateList = GetSalesLineShippingRateListAndTotalWeight(salesLineGroup, itemGrossWeights, out totalWeight);
    
                ShippingRateInfo shippingRateInfo = new ShippingRateInfo { FromAddress = shippingOrigin, ToAddress = shippingDestination, GrossWeight = totalWeight };
    
                // Get Carrier info
                ParameterSet adapterConfig = ExtractAdapterConfigForSpecificDeliveryMode(deliveryModeId, shippingAdapterConfigRecords);
                var carrierServiceRequest = new GetShippingRateFromCarrierServiceRequest(adapterConfig, shippingRateInfo);
                IRequestHandler carrierAdapterHandler = GetCarrierAdapterService(requestContext, carrierServiceRequest.GetType(), adapterConfig);
    
                if (carrierAdapterHandler != null)
                {
                    // Contact carrier
                    var carrierServiceResponse = requestContext.Execute<GetShippingRateFromCarrierServiceResponse>(carrierServiceRequest, carrierAdapterHandler);
                    var totalShippingCharge = carrierServiceResponse.ShippingRate;
    
                    // Redistribute rates across the salesline based on their weights
                    foreach (var lineShippingRate in salesLineShippingRateList)
                    {
                        lineShippingRate.ShippingCharge = (totalShippingCharge * lineShippingRate.NetWeight) / totalWeight;
                    }
                }
                else
                {
                    NetTracer.Warning("No registered carrier adapter is found.");
                }
    
                return salesLineShippingRateList;
            }
    
            /// <summary>
            /// Calculates the total weight of all items in sales lines.
            /// </summary>
            /// <param name="salesLines">The sales lines.</param>
            /// <param name="itemGrossWeights">The item gross weights.</param>
            /// <param name="totalWeightofAllItems">The total weight of all items.</param>
            /// <returns>List of SalesLineShippingRate initialized with line ids and weights of items per line.</returns>
            private static IEnumerable<SalesLineShippingRate> GetSalesLineShippingRateListAndTotalWeight(IEnumerable<SalesLine> salesLines, Dictionary<string, decimal> itemGrossWeights, out decimal totalWeightofAllItems)
            {
                List<SalesLineShippingRate> salesLineShippingRates = new List<SalesLineShippingRate>();
    
                totalWeightofAllItems = 0M;
    
                // Iterate through list of salesline in a given group
                foreach (var salesLine in salesLines)
                {
                    decimal weightOfItemsInLine = itemGrossWeights[salesLine.ItemId.ToUpperInvariant()] * salesLine.Quantity;
                    var lineShippingRate = new SalesLineShippingRate(salesLine.LineId, 0M, weightOfItemsInLine);
                    totalWeightofAllItems += weightOfItemsInLine;
    
                    salesLineShippingRates.Add(lineShippingRate);
                }
    
                return salesLineShippingRates;
            }
    
            /// <summary>
            /// Computes the delivery options that are common to all the Sales lines.
            /// </summary>
            /// <param name="salesLineDeliveryOptions">Delivery options at each sales line level.</param>
            /// <returns>The delivery options that are common to all the sales lines.</returns>
            private static ReadOnlyCollection<DeliveryOption> GetCommonDeliveryOptions(IEnumerable<SalesLineDeliveryOption> salesLineDeliveryOptions)
            {
                if (salesLineDeliveryOptions == null)
                {
                    throw new ArgumentNullException("salesLineDeliveryOptions");
                }
    
                Collection<DeliveryOption> commonDeliveryOptions = new Collection<DeliveryOption>();
    
                // Keeps track of the number of times a delivery option occurs across all the saleslines.
                Dictionary<DeliveryOption, int> deliveryOptionsApplicabilityCount = new Dictionary<DeliveryOption, int>();
    
                // Parse once through all the delivery options of every salesline to calculate the number of saleslines that each delivery option
                // is applicable to.
                var nonNullSalesLineDeliveryOptions = salesLineDeliveryOptions.Where(s => s != null);
                foreach (var salesLineDeliveryOption in nonNullSalesLineDeliveryOptions)
                {
                    var nonNullUniqueDeliveryOptions = salesLineDeliveryOption.DeliveryOptions.Where(d => d != null).Distinct();
                    foreach (var deliveryOption in nonNullUniqueDeliveryOptions)
                    {
                        if (!deliveryOptionsApplicabilityCount.ContainsKey(deliveryOption))
                        {
                            deliveryOptionsApplicabilityCount.Add(deliveryOption, 0);
                        }
    
                        deliveryOptionsApplicabilityCount[deliveryOption]++;
                    }
                }
    
                int salesLineDeliveryOptionsCount = salesLineDeliveryOptions.Count();
    
                // Pull out delivery options that appeared at least once across all the saleslines.
                foreach (var deliveryOption in deliveryOptionsApplicabilityCount.Keys)
                {
                    if (deliveryOptionsApplicabilityCount[deliveryOption] == salesLineDeliveryOptionsCount)
                    {
                        commonDeliveryOptions.Add(deliveryOption);
                    }
                }
    
                return commonDeliveryOptions.AsReadOnly();
            }
    
            /// <summary>
            /// Validates the address.
            /// </summary>
            /// <param name="shippingRequest">The shipping request.</param>
            /// <returns>The response.</returns>
            /// <remarks>Throws an exception if the address field in the request is null.</remarks>
            private static ValidateShippingAddressServiceResponse ValidateShippingAddress(ValidateShippingAddressServiceRequest shippingRequest)
            {
                if (shippingRequest.AddressToValidate == null)
                {
                    throw new ArgumentNullException("shippingRequest", "shippingRequest.AddressToValidate");
                }
    
                if (shippingRequest.DeliveryModeId == null)
                {
                    throw new ArgumentNullException("shippingRequest", "shippingRequest.DeliveryModeId");
                }
    
                var dataServiceRequest = new GetShippingAdapterConfigurationDataRequest(new[] { shippingRequest.DeliveryModeId }, QueryResultSettings.AllRecords);
                var dataServiceResponse = shippingRequest.RequestContext.Execute<EntityDataServiceResponse<ShippingAdapterConfig>>(dataServiceRequest);
    
                var shippingAdapterConfigLines = dataServiceResponse.PagedEntityCollection.Results;
    
                ParameterSet adapterConfig = ExtractAdapterConfigForSpecificDeliveryMode(shippingRequest.DeliveryModeId, shippingAdapterConfigLines);
    
                var carrierServiceRequest = new ValidateShippingAddressCarrierServiceRequest(adapterConfig, shippingRequest.AddressToValidate, shippingRequest.SuggestAddress);
                IRequestHandler carrierAdapterHandler = GetCarrierAdapterService(shippingRequest.RequestContext, carrierServiceRequest.GetType(), adapterConfig);
    
                bool isAddressValid = true;
                IEnumerable<Address> addressSuggestions = null;
    
                if (carrierAdapterHandler != null)
                {
                    // Call the adapter's validation method
                    var carrierServiceResponse = shippingRequest.RequestContext.Runtime.Execute<ValidateShippingAddressCarrierServiceResponse>(carrierServiceRequest, shippingRequest.RequestContext, carrierAdapterHandler);
                    isAddressValid = carrierServiceResponse.IsAddressValid;
                    addressSuggestions = carrierServiceResponse.RecommendedAddresses;
                }
                else
                {
                    NetTracer.Warning("No registered shipping carrier is found.");
                }
    
                return new ValidateShippingAddressServiceResponse(isAddressValid, addressSuggestions);
            }
    
            /// <summary>
            /// Gets handle to instance of carrier adapter based on passed in carrier name.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="requestType">The request type.</param>
            /// <param name="adapterConfig">The adapter configuration.</param>
            /// <returns>
            /// Instance of the adapter.
            /// </returns>
            private static IRequestHandler GetCarrierAdapterService(RequestContext context, Type requestType, ParameterSet adapterConfig)
            {
                if (adapterConfig == null)
                {
                    throw new ArgumentNullException("adapterConfig");
                }
    
                // Name of key that identifies name of adapter in the property bag.
                const string AdapterIdentifierField = "Name";
                string carrierAdapterName = adapterConfig[AdapterIdentifierField] as string;
                if (string.IsNullOrWhiteSpace(carrierAdapterName))
                {
                    return null;
                }
    
                IRequestHandler carrierAdapter = context.Runtime.GetRequestHandler(requestType, carrierAdapterName);
                if (carrierAdapter == null)
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidProviderConfiguration,
                        string.Format("Unable to retrieve adapter {0}.", carrierAdapterName));
                }
    
                return carrierAdapter;
            }
    
            /// <summary>
            /// Populates the tracking info for each shipment.
            /// </summary>
            /// <param name="shipments">The shipments.</param>
            /// <param name="requestContext">The request context.</param>
            private static void PopulateTrackingInfo(ICollection<Shipment> shipments, RequestContext requestContext)
            {
                if (shipments == null)
                {
                    throw new ArgumentNullException("shipments");
                }
    
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }
    
                // Dictionary to keep track of non-duplicate delivery mode ids alongwith associated adapterconfigs and tracking numbers.
                Dictionary<string, TrackingConfig> deliveryModeIdMapper = new Dictionary<string, TrackingConfig>(StringComparer.OrdinalIgnoreCase);
    
                // Map a list of non-duplicate delivery mode Ids to corresponding tracking numbers containers across all shipments.
                foreach (Shipment shipment in shipments)
                {
                    // If deliveryModeId not encountered so far, then add entry to dictionary.
                    if (!deliveryModeIdMapper.ContainsKey(shipment.DeliveryMode))
                    {
                        deliveryModeIdMapper.Add(shipment.DeliveryMode, new TrackingConfig());
                    }
    
                    var trackingConfig = deliveryModeIdMapper[shipment.DeliveryMode];
    
                    // Consolidate all tracking numbers that have the same delivery mode.
                    if (!string.IsNullOrWhiteSpace(shipment.TrackingNumber))
                    {
                        trackingConfig.TrackingNumbers.Add(shipment.TrackingNumber);
                    }
                }
    
                PopulateAdapterConfig(requestContext, deliveryModeIdMapper);
    
                // For each unique delivery mode identifier, fetch and contact the associated adapter.
                foreach (var trackingConfig in deliveryModeIdMapper.Values)
                {
                    var carrierServiceRequest = new GetTrackingInformationFromCarrierServiceRequest(trackingConfig.AdapterConfig, trackingConfig.TrackingNumbers);
                    IRequestHandler carrierAdapterHandler = GetCarrierAdapterService(requestContext, carrierServiceRequest.GetType(), trackingConfig.AdapterConfig);
                    var carrierServiceResponse = requestContext.Runtime.Execute<GetTrackingInformationFromCarrierServiceResponse>(carrierServiceRequest, requestContext, carrierAdapterHandler);
    
                    // Update shipments based on response.
                    foreach (TrackingInfo trackingDetail in carrierServiceResponse.TrackingDetails.Results)
                    {
                        var matchingShipments = shipments.Where(shipment => string.Equals(shipment.TrackingNumber, trackingDetail.TrackingNumber, StringComparison.OrdinalIgnoreCase));
                        foreach (var shipment in matchingShipments)
                        {
                            shipment.LatestCarrierTrackingInfo = trackingDetail;
                        }
                    }
                }
            }
    
            /// <summary>
            /// Populates the adapter configuration for each delivery mode identifier key in the passed in dictionary.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="deliveryModeIdMapper">The delivery mode identifier mapper.</param>
            private static void PopulateAdapterConfig(RequestContext context, Dictionary<string, TrackingConfig> deliveryModeIdMapper)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                if (deliveryModeIdMapper == null)
                {
                    throw new ArgumentNullException("deliveryModeIdMapper");
                }
    
                List<string> nonDuplicateDeliveryIds = new List<string>(deliveryModeIdMapper.Keys);
    
                // Fetch adapter configuration table for all the delivery mode ids.
                var dataServiceRequest = new GetShippingAdapterConfigurationDataRequest(nonDuplicateDeliveryIds, QueryResultSettings.AllRecords);
                var dataServiceResponse = context.Execute<EntityDataServiceResponse<ShippingAdapterConfig>>(dataServiceRequest);
    
                var shippingAdapterConfigTable = dataServiceResponse.PagedEntityCollection.Results;
    
                // Consolidate adapter config entries by having separate property bags per deliveryModeId.
                foreach (var adapterConfigLine in shippingAdapterConfigTable)
                {
                    var trackingConfig = deliveryModeIdMapper[adapterConfigLine.DeliveryModeId];
                    string keyName = adapterConfigLine.KeyName;
                    string keyValue = adapterConfigLine.KeyValue;
    
                    trackingConfig.AdapterConfig[keyName] = keyValue;
                }
            }
    
            /// <summary>
            /// Reads the shipping information from AX database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The shipment data.</returns>
            private static GetShipmentsServiceResponse GetShipments(GetShipmentsServiceRequest request)
            {
                if (request.SalesId == null)
                {
                    throw new ArgumentNullException("request", "request.SalesId");
                }
    
                var getShipmentsRealtimeRequest = new GetShipmentsRealtimeRequest(request.SalesId, request.ShipmentId);
                var shipments = request.RequestContext.Execute<EntityDataServiceResponse<Shipment>>(getShipmentsRealtimeRequest).PagedEntityCollection;
    
                // Populate latest tracking info from carrier for all the shipments if the flag was set.
                if (request.GetTrackingInfo && shipments.Results.Any())
                {
                    PopulateTrackingInfo(shipments.Results, request.RequestContext);
                }
    
                return new GetShipmentsServiceResponse(shipments);
            }
    
            /// <summary>
            /// Extracts the adapter configuration for single delivery mode.
            /// </summary>
            /// <param name="deliveryMode">The delivery mode.</param>
            /// <param name="shippingAdapterConfigRecords">The shipping adapter configuration records.</param>
            /// <returns>Property bag of adapter configuration key names and values.</returns>
            /// <exception cref="ArgumentNullException">The <paramref name="deliveryMode"/> parameter is null or empty. </exception>
            /// <exception cref="ArgumentNullException">The <paramref name="shippingAdapterConfigRecords"/> parameter is null or empty. </exception>
            private static ParameterSet ExtractAdapterConfigForSpecificDeliveryMode(string deliveryMode, IEnumerable<ShippingAdapterConfig> shippingAdapterConfigRecords)
            {
                if (string.IsNullOrWhiteSpace(deliveryMode))
                {
                    throw new ArgumentNullException("deliveryMode");
                }
    
                if (shippingAdapterConfigRecords == null)
                {
                    throw new ArgumentNullException("shippingAdapterConfigRecords");
                }
    
                ParameterSet adapterConfig = new ParameterSet();
    
                foreach (ShippingAdapterConfig shippingAdapterConfigLine in shippingAdapterConfigRecords)
                {
                    if (string.Equals(deliveryMode, shippingAdapterConfigLine.DeliveryModeId, StringComparison.OrdinalIgnoreCase))
                    {
                        adapterConfig[shippingAdapterConfigLine.KeyName] = shippingAdapterConfigLine.KeyValue;
                    }
                }
    
                return adapterConfig;
            }
        }
    }
}
