/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Helpers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates helper functions for channel availability.
        /// </summary>
        internal static class ChannelAvailabilityHelper
        {
            /// <summary>
            /// Get availability of items for specified stores.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="stores">The collection of store locations.</param>
            /// <param name="itemUnits">The collection of items.</param>
            /// <returns>
            /// The collection of store availability information.
            /// </returns>
            internal static Collection<OrgUnitAvailability> GetChannelAvailabiltiy(RequestContext context, IEnumerable<OrgUnitLocation> stores, IEnumerable<ItemUnit> itemUnits)
            {
                HashSet<ItemWarehouse> itemWarehouses = new HashSet<ItemWarehouse>();
    
                foreach (OrgUnitLocation storeLocation in stores)
                {
                    foreach (var itemUnit in itemUnits)
                    {
                        ItemWarehouse itemWarehouse = new ItemWarehouse
                        {
                            ItemId = itemUnit.ItemId,
                            VariantInventoryDimensionId = itemUnit.VariantInventoryDimensionId,
                            InventoryLocationId = storeLocation.InventoryLocationId
                        };
                        itemWarehouses.Add(itemWarehouse);
                    }
                }
    
                var request = new GetItemAvailabilitiesByItemWarehousesServiceRequest(QueryResultSettings.AllRecords, itemWarehouses);
                var response = context.Execute<GetItemAvailabilitiesByItemWarehousesServiceResponse>(request);
    
                ChannelAvailabilityHelper.ConvertUnitOfMeasure(context, response.ItemAvailabilities.Results, itemUnits);
    
                Collection<OrgUnitAvailability> storeAvailabilities = new Collection<OrgUnitAvailability>();
                foreach (OrgUnitLocation storeLocation in stores)
                {
                    List<ItemAvailability> itemAvailabilities = response.ItemAvailabilities.Results.Where(item => item.InventoryLocationId.Equals(storeLocation.InventoryLocationId, StringComparison.OrdinalIgnoreCase)).ToList();
                    storeAvailabilities.Add(new OrgUnitAvailability(storeLocation, itemAvailabilities));
                }
    
                return storeAvailabilities;
            }
    
            /// <summary>
            /// Gets a list of item availability given the item warehouse filters.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="itemWarehouses">The list of item warehouse filters.</param>
            /// <returns>The list of item availability.</returns>
            internal static IEnumerable<ItemAvailability> GetItemAvailabilitiesByItemWarehouses(
                RequestContext context,
                IEnumerable<ItemWarehouse> itemWarehouses)
            {
                var request = new GetItemAvailabilitiesByItemWarehousesServiceRequest(QueryResultSettings.AllRecords, itemWarehouses.Distinct());
                var response = context.Execute<GetItemAvailabilitiesByItemWarehousesServiceResponse>(request);
    
                return response.ItemAvailabilities.Results;
            }
    
            internal static IEnumerable<ItemAvailability> GetAllItemAvailablitiesByItemQuantities(RequestContext context, string customerId, ICollection<ItemQuantity> itemQuantities, int maxWarehousesPerItem)
            {
                QueryResultSettings queryResultSettings = QueryResultSettings.AllRecords;
    
                // Get item availabilities.
                var itemQuantitiesRequest = new GetItemAvailabilitiesByItemQuantitiesServiceRequest(
                    queryResultSettings,
                    itemQuantities,
                    customerId,
                    maxWarehousesPerItem);
    
                var itemQuantitiesResponse = context.Execute<GetItemAvailabilitiesByItemQuantitiesServiceResponse>(itemQuantitiesRequest);
    
                return itemQuantitiesResponse.ItemAvailabilities.Results;
            }
    
            /// <summary>
            /// Convert unit of measure for item reservations.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="itemReservations">The item reservations.</param>
            /// <param name="itemUnitConversions">The item unit conversions.</param>
            /// <exception cref="DataValidationException">Thrown if there is no conversion rules defined for any of item reservations.</exception>
            internal static void ConvertUnitOfMeasure(RequestContext context, IEnumerable<ItemReservation> itemReservations, IEnumerable<ItemUnitConversion> itemUnitConversions)
            {
                Collection<DataValidationFailure> validationFailures = new Collection<DataValidationFailure>();
    
                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> conversions = GetUnitOfMeasureConversions(context, itemUnitConversions);
                IEnumerator<ItemReservation> itemReservationEnumerator = itemReservations.GetEnumerator();
                IEnumerator<ItemUnitConversion> itemUnitConversionEnumerator = itemUnitConversions.GetEnumerator();
                while (itemReservationEnumerator.MoveNext() && itemUnitConversionEnumerator.MoveNext())
                {
                    ItemUnitConversion itemUnitConversion = itemUnitConversionEnumerator.Current;
                    if (!itemUnitConversion.IsNop)
                    {
                        UnitOfMeasureConversion unitOfMeasureConversion;
                        if (conversions.TryGetValue(itemUnitConversion, out unitOfMeasureConversion))
                        {
                            ItemReservation itemReservation = itemReservationEnumerator.Current;
                            itemReservation.Quantity = unitOfMeasureConversion.Convert(itemReservation.Quantity);
                        }
                        else
                        {
                            DataValidationFailure validationFailure = new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnitOfMeasureConversionNotFound);
                            validationFailure.ErrorContext = string.Format(CultureInfo.InvariantCulture, "No conversion rules defined for ItemUnitConversion:{0}", itemUnitConversion);
                            validationFailures.Add(validationFailure);
                        }
                    }
                }
    
                if (validationFailures.Count > 0)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError,
                        validationFailures,
                        string.Format("There are {0} item reservations whose unit of measures cannot be converted.", validationFailures.Count));
                }
            }
    
            /// <summary>
            /// Convert unit of measure for item availabilities.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="itemAvailabilities">The item availabilities.</param>
            /// <param name="itemUnits">The desired item unit of measures.</param>
            /// <returns>The converted item availabilities.</returns>
            internal static IEnumerable<ItemAvailability> ConvertUnitOfMeasure(RequestContext context, IEnumerable<ItemAvailability> itemAvailabilities, IEnumerable<ItemUnit> itemUnits)
            {
                List<ItemAvailability> convertedItemAvailabilities = new List<ItemAvailability>();
    
                IEnumerable<ItemUnit> distinctItemUnits = itemUnits.Distinct();
                IEnumerable<ItemUnitQuantity> itemUnitQuantities = itemAvailabilities.Select(itemAvailability => itemAvailability.GetItemUnitQuantity());
                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> conversions = GetUnitOfMeasureConversions(context, itemUnitQuantities, distinctItemUnits);
    
                ILookup<ItemVariantInventoryDimension, ItemUnit> itemUnitLookupByItem = distinctItemUnits.ToLookup(itemUnit => itemUnit.GetItem());
                foreach (ItemAvailability itemAvailability in itemAvailabilities)
                {
                    bool hasInventoryUnit = false;
                    foreach (ItemUnit itemUnit in itemUnitLookupByItem[itemAvailability.GetItem()])
                    {
                        ItemUnitConversion itemUnitConversion = ChannelAvailabilityHelper.GetItemUnitConversion(itemAvailability.GetItemUnitQuantity(), itemUnit);
                        UnitOfMeasureConversion unitOfMeasureConversion;
                        if (!conversions.TryGetValue(itemUnitConversion, out unitOfMeasureConversion))
                        {
                            if (!StringDataHelper.Equals(itemAvailability.UnitOfMeasure, itemUnit.UnitOfMeasure))
                            {
                                context.Notify(new UnableToConvertUnitOfMeasureNotification(itemUnitConversion));
                            }
    
                            if (!hasInventoryUnit)
                            {
                                convertedItemAvailabilities.Add(itemAvailability);
                                hasInventoryUnit = true;
                            }
                        }
                        else
                        {
                            ItemAvailability convertedItemAvailability = new ItemAvailability();
                            convertedItemAvailability.CopyPropertiesFrom(itemAvailability);
                            convertedItemAvailability.AvailableQuantity = unitOfMeasureConversion.Convert(itemAvailability.AvailableQuantity);
                            convertedItemAvailability.UnitOfMeasure = itemUnitConversion.ToUnitOfMeasure;
                            convertedItemAvailabilities.Add(convertedItemAvailability);
                        }
                    }
                }
    
                return convertedItemAvailabilities.AsEnumerable();
            }
    
            /// <summary>
            /// Convert unit of measure for item availabilities.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="itemAvailableQuantities">The item availabilities.</param>
            /// <param name="itemUnits">The desired item unit of measures.</param>
            /// <returns>The converted item available quantities.</returns>
            internal static IEnumerable<ItemAvailableQuantity> ConvertUnitOfMeasure(RequestContext context, IEnumerable<ItemAvailableQuantity> itemAvailableQuantities, IEnumerable<ItemUnit> itemUnits)
            {
                List<ItemAvailableQuantity> convertedItemAvailableQuantities = new List<ItemAvailableQuantity>();
    
                IEnumerable<ItemUnit> distinctItemUnits = itemUnits.Distinct();
                IEnumerable<ItemUnitQuantity> itemUnitQuantities = itemAvailableQuantities.Select(itemAvailableQuantity => itemAvailableQuantity.GetItemUnitQuantity());
                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> conversions = GetUnitOfMeasureConversions(context, itemUnitQuantities, distinctItemUnits);
    
                ILookup<ItemVariantInventoryDimension, ItemUnit> itemUnitLookupByItem = distinctItemUnits.ToLookup(itemUnit => itemUnit.GetItem());
                foreach (ItemAvailableQuantity itemAvailableQuantity in itemAvailableQuantities)
                {
                    bool hasInventoryUnit = false;
                    foreach (ItemUnit itemUnit in itemUnitLookupByItem[itemAvailableQuantity.GetItem()])
                    {
                        ItemUnitConversion itemUnitConversion = ChannelAvailabilityHelper.GetItemUnitConversion(itemAvailableQuantity.GetItemUnitQuantity(), itemUnit);
                        UnitOfMeasureConversion unitOfMeasureConversion;
                        if (!conversions.TryGetValue(itemUnitConversion, out unitOfMeasureConversion))
                        {
                            if (!StringDataHelper.Equals(itemAvailableQuantity.UnitOfMeasure, itemUnit.UnitOfMeasure))
                            {
                                context.Notify(new UnableToConvertUnitOfMeasureNotification(itemUnitConversion));
                            }
    
                            if (!hasInventoryUnit)
                            {
                                convertedItemAvailableQuantities.Add(itemAvailableQuantity);
                                hasInventoryUnit = true;
                            }
                        }
                        else
                        {
                            ItemAvailableQuantity convertedItemAvailableQuantity = new ItemAvailableQuantity();
                            convertedItemAvailableQuantity.CopyPropertiesFrom(itemAvailableQuantity);
                            convertedItemAvailableQuantity.AvailableQuantity = unitOfMeasureConversion.Convert(itemAvailableQuantity.AvailableQuantity);
                            convertedItemAvailableQuantity.UnitOfMeasure = itemUnitConversion.ToUnitOfMeasure;
                            convertedItemAvailableQuantities.Add(convertedItemAvailableQuantity);
                        }
                    }
                }
    
                return convertedItemAvailableQuantities.AsEnumerable();
            }
    
            internal static Dictionary<ItemUnitConversion, UnitOfMeasureConversion> GetUnitOfMeasureConversions(RequestContext context, IEnumerable<ItemUnitQuantity> itemUnitQuantities, IEnumerable<ItemUnit> itemUnits)
            {
                IEnumerable<ItemUnit> distinctItemUnits = itemUnits.Distinct();
                ILookup<string, ItemUnit> itemUnitLookupByItemId = distinctItemUnits.ToLookup(itemUnit => itemUnit.ItemId);
                IEnumerable<ItemUnitConversion> itemUnitConversions = itemUnitQuantities.SelectMany(itemUnitQuantity => ChannelAvailabilityHelper.GetItemUnitConversions(itemUnitQuantity, itemUnitLookupByItemId));
                return GetUnitOfMeasureConversions(context, itemUnitConversions);
            }
    
            internal static Dictionary<ItemUnitConversion, UnitOfMeasureConversion> GetUnitOfMeasureConversions(RequestContext context, IEnumerable<ItemUnitConversion> itemUnitConversions)
            {
                IEnumerable<ItemUnitConversion> distinctItemUnitConversions = itemUnitConversions
                    .Where(itemUnitConversion => !itemUnitConversion.IsNop)
                    .Distinct();
    
                var getUomConvertionDataRequest = new GetUnitOfMeasureConversionDataRequest(distinctItemUnitConversions, QueryResultSettings.AllRecords);
                IEnumerable<UnitOfMeasureConversion> unitOfMeasureConversions = context.Runtime
                    .Execute<GetUnitOfMeasureConversionDataResponse>(getUomConvertionDataRequest, context).UnitConversions.Results;
    
                return unitOfMeasureConversions.ToDictionary(unitOfMeasureConversion => new ItemUnitConversion()
                {
                    ItemId = unitOfMeasureConversion.ItemId,
                    FromUnitOfMeasure = unitOfMeasureConversion.FromUnitOfMeasureSymbol,
                    ToUnitOfMeasure = unitOfMeasureConversion.ToUnitOfMeasureSymbol,
                });
            }
    
            private static ItemUnitConversion GetItemUnitConversion(ItemUnitQuantity itemUnitQuantity, ItemUnit itemUnit)
            {
                return new ItemUnitConversion()
                {
                    ItemId = itemUnitQuantity.ItemId,
                    FromUnitOfMeasure = itemUnitQuantity.UnitOfMeasure,
                    ToUnitOfMeasure = itemUnit.UnitOfMeasure,
                };
            }
    
            private static IEnumerable<ItemUnitConversion> GetItemUnitConversions(ItemUnitQuantity itemUnitQuantity, ILookup<string, ItemUnit> itemUnitLookupByItemId)
            {
                return itemUnitLookupByItemId[itemUnitQuantity.ItemId].Select(itemUnit => GetItemUnitConversion(itemUnitQuantity, itemUnit));
            }
        }
    }
}
