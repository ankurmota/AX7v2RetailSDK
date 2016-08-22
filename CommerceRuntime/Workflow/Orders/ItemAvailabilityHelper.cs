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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Helper class for item availability related workflows.
        /// </summary>
        public static class ItemAvailabilityHelper
        {
            /// <summary>
            /// Sets the sales line inventory for each sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Current transaction.</param>
            public static void SetSalesLineInventory(RequestContext context, SalesTransaction transaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(transaction, "transaction");
    
                NetTracer.Information("ItemAvailabilityHelper.SetSalesLineInventory(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);
    
                // Get channel configuration.
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
    
                if (!NeedInventoryChecking(channelConfiguration, transaction))
                {
                    return;
                }
    
                // Get unit of measure conversions from sales to inventory unit of measure.
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                IEnumerable<ItemUnitConversion> itemUnitConversions = transaction.InventorySalesLines.Select(salesLine => salesLine.GetItemUnitConversion());
                Dictionary<ItemUnitConversion, UnitOfMeasureConversion> unitOfMeasureConversions = ChannelAvailabilityHelper.GetUnitOfMeasureConversions(context, itemUnitConversions);
    
                // Convert quantities from sales to inventory unit of measure.
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                Dictionary<string, decimal> salesLineInventoryQuantities = GetSalesLineInventoryQuantities(context, transaction.InventorySalesLines, unitOfMeasureConversions);
    
                // Set sales line inventory for pickup or for return
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                IEnumerable<SalesLine> linesForPickup = transaction.InventorySalesLines.Where(s => IsPickupOrReturn(s, channelConfiguration));
                if (linesForPickup.Any())
                {
                    SetSalesLineInventoryForPickup(context, transaction, linesForPickup, salesLineInventoryQuantities);
                }
    
                // Set sales line inventory for shipping
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                IEnumerable<SalesLine> linesForShipping = transaction.InventorySalesLines.Where(s => !IsPickupOrReturn(s, channelConfiguration));
                if (linesForShipping.Any())
                {
                    SetSalesLineInventoryForShipping(context, transaction, linesForShipping, salesLineInventoryQuantities);
                }
            }
    
            /// <summary>
            /// Verifies if the sales line is a pickup or a return.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <returns><c>True</c> if the sales line is a pickup or a return, <c>false</c> otherwise.</returns>
            private static bool IsPickupOrReturn(SalesLine salesLine, ChannelConfiguration channelConfiguration)
            {
                return IsPickup(salesLine, channelConfiguration) || salesLine.IsReturnByReceipt;
            }
    
            /// <summary>
            /// Needs the inventory checking.
            /// </summary>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="transaction">The transaction.</param>
            /// <returns><c>True</c> if the inventory checking is required, <c>false</c> otherwise.</returns>
            private static bool NeedInventoryChecking(ChannelConfiguration channelConfiguration, SalesTransaction transaction)
            {
                return (channelConfiguration.ChannelType != RetailChannelType.RetailStore) && (transaction.CartType != CartType.AccountDeposit);
            }
    
            /// <summary>
            /// Verifies if the sales line is a pickup.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <returns><c>True</c> if the sales line is a pickup, <c>false</c> otherwise.</returns>
            private static bool IsPickup(SalesLine salesLine, ChannelConfiguration channelConfiguration)
            {
                return IsPickup(salesLine.DeliveryMode, channelConfiguration);
            }
    
            /// <summary>
            /// Verifies if the delivery mode is a pickup.
            /// </summary>
            /// <param name="deliveryMode">The delivery mode.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <returns><c>True</c> if the delivery mode is a pickup, <c>false</c> otherwise.</returns>
            private static bool IsPickup(string deliveryMode, ChannelConfiguration channelConfiguration)
            {
                return string.Equals(deliveryMode, channelConfiguration.PickupDeliveryModeCode, StringComparison.OrdinalIgnoreCase);
            }
    
            private static void SetSalesLineInventoryForPickup(RequestContext context, SalesTransaction transaction, IEnumerable<SalesLine> salesLines, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesLines, "salesLines");
    
                NetTracer.Information("ItemAvailabilityHelper.SetSalesLineInventoryForPickup(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);
    
                // Get item availablities by item warehouses.
                IEnumerable<ItemAvailability> itemAvailabilities = ChannelAvailabilityHelper.GetItemAvailabilitiesByItemWarehouses(context, salesLines.Select(salesLine => salesLine.GetItemWarehouse()));
    
                // Set inventory for sales lines.
                SetSalesLineInventoryForPickup(context, salesLines, itemAvailabilities, salesLineInventoryQuantities);
            }
    
            private static void SetSalesLineInventoryForShipping(RequestContext context, SalesTransaction transaction, IEnumerable<SalesLine> salesLines, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesLines, "salesLines");
    
                NetTracer.Information("ItemAvailabilityHelper.SetSalesLineInventoryForShipping(): TransactionId = {0}, CustomerId = {1}", transaction.Id, transaction.CustomerId);
    
                // Calculate total converted quantities.
                Dictionary<ItemVariantInventoryDimension, ItemQuantity> itemQuantities = GetSalesLineItemQuantities(salesLines, salesLineInventoryQuantities);
    
                // Get item availablities by total item quantities.
                IEnumerable<ItemAvailability> itemAvailabilities = ChannelAvailabilityHelper.GetAllItemAvailablitiesByItemQuantities(context, transaction.CustomerId, itemQuantities.Values, GetMaxLinesPerItem(salesLines));
    
                // Set inventory for sales lines.
                SetSalesLineInventoryForShipping(context, salesLines, itemAvailabilities, salesLineInventoryQuantities);
            }
    
            private static Dictionary<string, decimal> GetSalesLineInventoryQuantities(RequestContext context, IEnumerable<SalesLine> salesLines, Dictionary<ItemUnitConversion, UnitOfMeasureConversion> unitOfMeasureConversions)
            {
                // Convert quantities from sales to inventory unit of measure and calculate total converted quantities.
                Dictionary<string, decimal> salesLineInventoryQuantities = new Dictionary<string, decimal>();
                foreach (SalesLine salesLine in salesLines)
                {
                    decimal salesLineInventoryQuantity = 0;
                    UnitOfMeasureConversion unitOfMeasureConversion;
                    ItemUnitConversion itemUnitConversion = salesLine.GetItemUnitConversion();
    
                    if (string.IsNullOrWhiteSpace(salesLine.SalesOrderUnitOfMeasure))
                    {
                        var notification = new EmptySalesUnitOfMeasureNotification(salesLine.LineId);
                        context.Notify(notification);
                    }
                    else if (string.IsNullOrWhiteSpace(salesLine.InventOrderUnitOfMeasure))
                    {
                        var notification = new EmptyInventoryUnitOfMeasureNotification(salesLine.ItemId);
                        context.Notify(notification);
                    }
                    else if (string.Equals(salesLine.SalesOrderUnitOfMeasure, salesLine.InventOrderUnitOfMeasure, StringComparison.OrdinalIgnoreCase))
                    {
                        salesLineInventoryQuantity = salesLine.Quantity;
                    }
                    else if (unitOfMeasureConversions.TryGetValue(itemUnitConversion, out unitOfMeasureConversion))
                    {
                        salesLineInventoryQuantity = unitOfMeasureConversion.Convert(salesLine.Quantity);
                    }
                    else
                    {
                        var notification = new UnableToConvertUnitOfMeasureNotification(itemUnitConversion);
                        context.Notify(notification);
                    }
    
                    salesLineInventoryQuantities[salesLine.LineId] = salesLineInventoryQuantity;
                }
    
                return salesLineInventoryQuantities;
            }
    
            private static Dictionary<ItemVariantInventoryDimension, ItemQuantity> GetSalesLineItemQuantities(IEnumerable<SalesLine> salesLines, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                Dictionary<ItemVariantInventoryDimension, ItemQuantity> itemQuantities = new Dictionary<ItemVariantInventoryDimension, ItemQuantity>();
                foreach (SalesLine salesLine in salesLines)
                {
                    ItemQuantity itemQuantity;
                    ItemVariantInventoryDimension item = salesLine.GetItemVariantInventoryDimension();
                    if (!itemQuantities.TryGetValue(item, out itemQuantity))
                    {
                        itemQuantity = new ItemQuantity()
                        {
                            ItemId = item.ItemId,
                            VariantInventoryDimensionId = item.VariantInventoryDimensionId,
                        };
    
                        itemQuantities.Add(item, itemQuantity);
                    }
    
                    itemQuantity.Quantity += salesLineInventoryQuantities[salesLine.LineId];
                }
    
                return itemQuantities;
            }
    
            private static int GetMaxLinesPerItem(IEnumerable<SalesLine> salesLines)
            {
                Dictionary<ItemVariantInventoryDimension, int> linesByItem = new Dictionary<ItemVariantInventoryDimension, int>();
                foreach (SalesLine salesLine in salesLines)
                {
                    int linesPerItem;
                    ItemVariantInventoryDimension item = salesLine.GetItemVariantInventoryDimension();
                    if (!linesByItem.TryGetValue(item, out linesPerItem))
                    {
                        linesByItem.Add(item, linesPerItem);
                    }
    
                    linesByItem[item] = linesPerItem + 1;
                }
    
                return linesByItem.Values.Max();
            }
    
            private static void SetSalesLineInventoryForPickup(RequestContext context, IEnumerable<SalesLine> salesLines, IEnumerable<ItemAvailability> itemAvailabilities, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                Dictionary<ItemWarehouse, ItemAvailability> itemAvailabilityByItemWarehouse = itemAvailabilities.ToDictionary(itemAvailability => itemAvailability.GetItemWarehouse());
                foreach (SalesLine salesLine in salesLines)
                {
                    ItemAvailability itemAvailability;
                    itemAvailabilityByItemWarehouse.TryGetValue(salesLine.GetItemWarehouse(), out itemAvailability);
                    SetSalesInventory(context, salesLine, itemAvailability, salesLineInventoryQuantities);
                }
            }
    
            private static void SetSalesLineInventoryForShipping(RequestContext context, IEnumerable<SalesLine> salesLines, IEnumerable<ItemAvailability> itemAvailabilities, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                ILookup<ItemVariantInventoryDimension, ItemAvailability> itemAvailabilityLookup = itemAvailabilities.ToLookup(itemAvailability => itemAvailability.GetItem());
                ILookup<ItemVariantInventoryDimension, SalesLine> salesLineLookup = salesLines.ToLookup(salesLine => salesLine.GetItemVariantInventoryDimension());
    
                foreach (IGrouping<ItemVariantInventoryDimension, SalesLine> grouping in salesLineLookup)
                {
                    ItemVariantInventoryDimension item = grouping.Key;
                    SortedSet<ItemAvailability> itemAvailabilitySortedSet = new SortedSet<ItemAvailability>(itemAvailabilityLookup[item], new ItemAvailabilityByQuantityDescendingComparer());
    
                    foreach (SalesLine salesLine in grouping.OrderByDescending(salesLine => salesLineInventoryQuantities[salesLine.LineId]))
                    {
                        ItemAvailability itemAvailability = itemAvailabilitySortedSet.FirstOrDefault();
                        if (SetSalesInventory(context, salesLine, itemAvailability, salesLineInventoryQuantities))
                        {
                            // Remove and add to force sort the set if available quantity is reduced.
                            itemAvailabilitySortedSet.Remove(itemAvailability);
                            itemAvailabilitySortedSet.Add(itemAvailability);
                        }
                    }
                }
            }
    
            private static bool SetSalesInventory(RequestContext context, SalesLine salesLine, ItemAvailability itemAvailability, Dictionary<string, decimal> salesLineInventoryQuantities)
            {
                decimal salesLineInventoryQuantity = salesLineInventoryQuantities[salesLine.LineId];
                if (itemAvailability == null)
                {
                    var notification = new InventoryNotFoundNotification(salesLine.LineId);
                    context.Notify(notification);
                }
                else 
                {
                    if (itemAvailability.AvailableQuantity < salesLineInventoryQuantity)
                    {
                        var notification = new InsufficientQuantityAvailableNotification(salesLine.LineId, salesLine.ItemId);
                        context.Notify(notification);
                    }
    
                    salesLine.InventoryLocationId = itemAvailability.InventoryLocationId;
    
                    itemAvailability.AvailableQuantity -= salesLineInventoryQuantity;
    
                    return true;
                }
    
                return false;
            }
        }
    }
}
