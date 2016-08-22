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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulate helpers for tax-related functionality.
        /// </summary>
        internal static class TaxHelpers
        {
            /// <summary>
            /// Creates the rounding service request.
            /// </summary>
            /// <param name="storeCurrency">The store currency.</param>
            /// <param name="amount">The amount.</param>
            /// <returns>
            /// The rounded amount.
            /// </returns>
            public static GetRoundedValueServiceRequest CreateRoundingServiceRequest(string storeCurrency, decimal amount)
            {
                return new GetRoundedValueServiceRequest(amount, storeCurrency, 0, useSalesRounding: false);
            }
    
            /// <summary>
            /// Creates the currency service request.
            /// </summary>
            /// <param name="taxAmount">The tax amount.</param>
            /// <param name="fromCurrency">From currency.</param>
            /// <param name="toCurrency">To currency.</param>
            /// <returns>
            /// The currency request object.
            /// </returns>
            public static GetCurrencyValueServiceRequest CreateCurrencyServiceRequest(decimal taxAmount, string fromCurrency, string toCurrency)
            {
                return new GetCurrencyValueServiceRequest(fromCurrency, toCurrency, taxAmount);
            }
    
            /// <summary>
            /// Sets the missing sales tax group identifier.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            public static void SetSalesTaxGroup(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Do not update sales tax group during customer order recall, since sales transaction will be saved on channel database.
                // This way the original sales tax group will be preserved for customer order scenarios tax calculations
                // on pick up order or return order.
                // On customer order pick up, we have to honor the original sales tax group from headquarters.
                // On customer order returns, we don't change tax groups as they come populated from the headquarters
                // Tax groups have been set during order creation and could be from a different store than the one the return is being made
                if (salesTransaction.CartType == CartType.CustomerOrder
                    && (salesTransaction.CustomerOrderMode == CustomerOrderMode.Return
                        || salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup
                        || salesTransaction.CustomerOrderMode == CustomerOrderMode.OrderRecalled))
                {
                    return;
                }
    
                SetSalesTaxGroupOnNonReturn(context, salesTransaction);
            }
    
            /// <summary>
            /// Sets default Sales Tax Group (STG) for the cart based on a channel tax configuration for non-return transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">Current transaction.</param>
            private static void SetSalesTaxGroupOnNonReturn(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                Channel channel;
                long currentChannelId = context.GetPrincipal().ChannelId;
                string channelTaxGroup;
    
                if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                {
                    OrgUnit store = context.GetOrgUnit();
                    channel = store;
                    channelTaxGroup = store.TaxGroup;
                }
                else
                {
                    var getChannelByIdDataRequest = new GetChannelByIdDataRequest(currentChannelId);
                    channel = context.Runtime.Execute<SingleEntityDataServiceResponse<Channel>>(getChannelByIdDataRequest, context).Entity;
    
                    channelTaxGroup = string.Empty;
                }
    
                Address headerAddress = Address.IsNullOrEmpty(salesTransaction.ShippingAddress)
                    ? null
                    : salesTransaction.ShippingAddress;
    
                // Header charges follows header when taxed
                SalesTaxGroupPicker headerPicker = SalesTaxGroupPicker.Create(
                    channel,
                    context,
                    headerAddress,
                    salesTransaction.DeliveryMode,
                    salesTransaction.StoreId ?? string.Empty,
                    salesTransaction.InventoryLocationId,
                    salesTransaction.CustomerId);
                FillChargeLinesSalesTaxGroup(context, salesTransaction.ChargeLines, headerPicker.SalesTaxGroup, channelTaxGroup);
    
                // items needed to retrieve ITG information from Product repo for each sales line
                IEnumerable<Item> items = GetItemsForSalesLines(context, salesTransaction.ActiveSalesLines);
                Dictionary<string, Item> itemsByItemId = items.ToDictionary(i => i.ItemId, i => i, StringComparer.OrdinalIgnoreCase);
    
                // Consider active lines for taxation purpose only.
                foreach (SalesLine salesLine in salesTransaction.ActiveSalesLines)
                {
                    // On Return By Receipt carts, we don't change tax groups as they come populated from the headquarters
                    // Tax groups have been set during sales transaction creation and could be from a different store than the one the return is being made.
                    if (!salesLine.IsReturnByReceipt)
                    {
                        Address shippingAddress = Address.IsNullOrEmpty(salesLine.ShippingAddress)
                            ? headerAddress
                            : salesLine.ShippingAddress;
    
                        SalesTaxGroupPicker linePicker = SalesTaxGroupPicker.Create(
                            channel,
                            context,
                            shippingAddress,
                            salesLine.DeliveryMode,
                            salesLine.FulfillmentStoreId ?? string.Empty,
                            salesTransaction.InventoryLocationId,
                            salesTransaction.CustomerId);
                        salesLine.SalesTaxGroupId = linePicker.SalesTaxGroup;
                        salesLine.OriginalSalesTaxGroupId = salesLine.SalesTaxGroupId;

                        Item cartItem;
    
                        // case 1: item without item tax group (set to null), regular cash and carry or sales order
                        //          -> set ITG to Product ITG
                        // case 2: customer order recall with tax exempt, sales line ITG
                        //         is set to "" (empty string) or carries any other value
                        //          -> keep ITG already set on sales line
                        if (salesLine.ItemId != null
                            && salesLine.ItemTaxGroupId == null
                            && itemsByItemId.TryGetValue(salesLine.ItemId, out cartItem))
                        {
                            salesLine.ItemTaxGroupId = cartItem.ItemTaxGroupId;
                            salesLine.OriginalItemTaxGroupId = salesLine.ItemTaxGroupId;
                        }
    
                        FillChargeLinesSalesTaxGroup(context, salesLine.ChargeLines, linePicker.SalesTaxGroup, channelTaxGroup);
                    }
                }
            }
    
            /// <summary>
            /// Retrieves the item data for the items in the cart.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLines">The sales lines for which to retrieve item information.</param>
            /// <returns>The cart items.</returns>
            private static IEnumerable<Item> GetItemsForSalesLines(RequestContext context, IEnumerable<SalesLine> salesLines)
            {
                IEnumerable<string> itemIds = salesLines.Where(l => l.IsProductLine).Select(l => l.ItemId);
    
                IEnumerable<Item> items = null;
                if (itemIds.Any())
                {
                    var getItemsRequest = new GetItemsDataRequest(itemIds);
                    getItemsRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet("ITEMID", "ITEMTAXGROUPID", "PRODUCT"), PagingInfo.AllRecords);
                    var getItemsResponse = context.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, context);
                    items = getItemsResponse.Items;
                }
                else
                {
                    items = new Collection<Item>();
                }
    
                return items;
            }
    
            /// <summary>
            /// Fill in charge group for charge lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="chargeLines">The charge lines to be updated.</param>
            /// <param name="deliverableTaxGroup">The deliverable (sales line or sales transaction) that contains the charge lines.</param>
            /// <param name="channelTaxGroup">The channel tax group.</param>
            private static void FillChargeLinesSalesTaxGroup(RequestContext context, IEnumerable<ChargeLine> chargeLines, string deliverableTaxGroup, string channelTaxGroup)
            {
                string shippingChargeCode = context.GetChannelConfiguration().ShippingChargeCode ?? string.Empty;
                string cancellationChargeCode = context.GetChannelConfiguration().CancellationChargeCode ?? string.Empty;
    
                // Charges follow sales line charge group
                foreach (ChargeLine chargeLine in chargeLines)
                {
                    if (shippingChargeCode.Equals(chargeLine.ChargeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        // Shipping charge follows sales line tax group
                        chargeLine.SalesTaxGroupId = deliverableTaxGroup;
                    }
                    else if (cancellationChargeCode.Equals(chargeLine.ChargeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        // Cancellation charges must always follow current channel's tax group
                        chargeLine.SalesTaxGroupId = channelTaxGroup;
                    }
                    else
                    {
                        // All other charges follows sales line tax group
                        chargeLine.SalesTaxGroupId = deliverableTaxGroup;
                    }
                }
            }
        }
    }
}
