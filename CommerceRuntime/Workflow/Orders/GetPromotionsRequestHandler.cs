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
        using System.Globalization;
        using System.Linq;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Handles workflow for GetPromotionsForCart.
        /// </summary>
        public sealed class GetPromotionsRequestHandler : SingleRequestHandler<GetPromotionsRequest, GetPromotionsResponse>
        {
            /// <summary>
            /// Executes the workflow to fetch the promotions. 
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetPromotionsResponse Process(GetPromotionsRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CartId, "request.CartId");           
    
                // Get the current instance of the transaction from the database.
                SalesTransaction transaction = CartWorkflowHelper.LoadSalesTransaction(this.Context, request.CartId);
                
                if (transaction == null)
                {
                    return new GetPromotionsResponse(null);
                }
    
                ThrowIf.Null(transaction, "transaction");
    
                // Calculate totals on the current instance of transaction.
                CartWorkflowHelper.Calculate(this.Context, transaction, CalculationModes.All);
    
                // The discount lines on this transaction are the discount lines that have been applied.           
                SalesTransaction currentSalesTransaction = transaction.Clone<SalesTransaction>();
    
                Cart cart = CartWorkflowHelper.ConvertToCart(this.Context, currentSalesTransaction);
                CartWorkflowHelper.RemoveHistoricalTenderLines(cart);
    
                // The discount lines on the transaction are all available discount lines for the items.
                CartWorkflowHelper.LoadAllPeriodicDiscounts(this.Context, currentSalesTransaction);
                SalesTransaction tempSalesTransaction = transaction.Clone<SalesTransaction>();
    
                transaction = currentSalesTransaction.Clone<SalesTransaction>();
                
                Collection<string> cartPromotionLines = new Collection<string>();
                Collection<CartLinePromotion> cartLinePromotions = new Collection<CartLinePromotion>();
    
                for (int i = 0; i < currentSalesTransaction.SalesLines.Count; i++)
                {
                    // Removing the applied discount lines, except multiple buy because a different discount level of the already applied multi buy discount can be promoted.                
                    foreach (DiscountLine discountLine in currentSalesTransaction.SalesLines[i].DiscountLines)
                    {
                        tempSalesTransaction.SalesLines[i].DiscountLines.Remove(tempSalesTransaction.SalesLines[i].DiscountLines.Where(j => j.OfferId == discountLine.OfferId).SingleOrDefault());
                    }
    
                    // Removing the discounts that require coupon code.
                    // Removing the discount offers those were not applied (because of concurrency rules). 
                    // Removing mix and match discounts (mix and match discounts are not shown as promotions).
                    List<DiscountLine> offerDiscountLines = tempSalesTransaction.SalesLines[i].DiscountLines.Where(j => (j.PeriodicDiscountType == PeriodicDiscountOfferType.Offer) || j.IsDiscountCodeRequired || (j.PeriodicDiscountType == PeriodicDiscountOfferType.MixAndMatch)).ToList();
                    foreach (DiscountLine discountLine in offerDiscountLines)
                    {
                        tempSalesTransaction.SalesLines[i].DiscountLines.Remove(discountLine);
                    }
    
                    PricingDataManager pricingDataManager = new PricingDataManager(this.Context);
    
                    // Quantity discounts.
                    // Finding all the quantity discounts that will be applied to the cart.
                    List<DiscountLine> quantityDiscountLines = tempSalesTransaction.SalesLines[i].DiscountLines.Where(j => j.PeriodicDiscountType == PeriodicDiscountOfferType.MultipleBuy).ToList();
    
                    // Get the multibuy discount lines for this multi buy discounts.
                    IEnumerable<QuantityDiscountLevel> multiBuyDiscountLines = pricingDataManager.GetMultipleBuyDiscountLinesByOfferIds(quantityDiscountLines.Select(j => j.OfferId));
    
                    foreach (DiscountLine discountLine in quantityDiscountLines)
                    {
                        GetQuantityPromotions(transaction, tempSalesTransaction, this.Context, i, discountLine, multiBuyDiscountLines);
                    }
    
                    // Threshhold Discounts.
                    // Finding all the threshold discounts that will be applied to the cart.
                    List<DiscountLine> thresholdDiscountLines = tempSalesTransaction.SalesLines[i].DiscountLines.Where(j => j.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold).ToList();
    
                    // Get the tiers for this threshold discounts
                    IEnumerable<ThresholdDiscountTier> tiers = pricingDataManager.GetThresholdTiersByOfferIds(thresholdDiscountLines.Select(j => j.OfferId));
    
                    foreach (DiscountLine thresholdDiscount in thresholdDiscountLines)
                    {
                        GetThresholdDiscounts(transaction, tempSalesTransaction, this.Context, i, cartPromotionLines, thresholdDiscount, tiers);
                    }
    
                     IEnumerable<string> promotionsForCurrentLine = tempSalesTransaction.SalesLines[i].DiscountLines.Select(j => j.OfferName);
                    cartLinePromotions.Add(new CartLinePromotion(cart.CartLines[i].LineId, promotionsForCurrentLine));
                }
    
                CartPromotions cartPromotions = new CartPromotions(cartPromotionLines, cartLinePromotions);
    
                return new GetPromotionsResponse(cartPromotions);
            }
    
            /// <summary>
            /// Updates the sales transaction with the quantity promotion if applicable.
            /// </summary>
            /// <param name="existingTransaction">Existing transaction.</param>
            /// <param name="tempSalesTransaction">Copy of existing transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="salesLineIndex">The sales line under consideration.</param>
            /// <param name="discountLine">The quantity discount under consideration.</param>
            /// <param name="multiBuyDiscountLines">The multi buy discount lines.</param>
            private static void GetQuantityPromotions(SalesTransaction existingTransaction, SalesTransaction tempSalesTransaction, RequestContext context, int salesLineIndex, DiscountLine discountLine, IEnumerable<QuantityDiscountLevel> multiBuyDiscountLines)
            {
                // Get the multi buy discount lines for the current multi buy discount.
                IEnumerable<QuantityDiscountLevel> multiBuyLinesForCurrentOffer = multiBuyDiscountLines.Where(j => j.OfferId.Equals(discountLine.OfferId)).OrderBy(l => l.MinimumQuantity);
    
                List<SalesLine> salesLinesWithSameProduct = tempSalesTransaction.SalesLines.Where(j => j.ItemId == tempSalesTransaction.SalesLines[salesLineIndex].ItemId && j.InventoryDimensionId == tempSalesTransaction.SalesLines[salesLineIndex].InventoryDimensionId).ToList();
                decimal totalQuantity = salesLinesWithSameProduct.Select(j => j.Quantity).Sum();
                decimal currentQuantity = tempSalesTransaction.SalesLines[salesLineIndex].Quantity;
                salesLinesWithSameProduct.Remove(tempSalesTransaction.SalesLines[salesLineIndex]);
                bool neverApplied = true;
                
                foreach (QuantityDiscountLevel multiBuyLine in multiBuyLinesForCurrentOffer)
                {
                    // removing the quantity discounts that were not applied (because of concurrency rules).
                    if (multiBuyLine.MinimumQuantity <= totalQuantity)
                    {
                        continue;
                    }

                    // Temporarily update the current transaction with the new quantity to see if the quantity discount will be applied.                    
                    existingTransaction.SalesLines[salesLineIndex].Quantity = multiBuyLine.MinimumQuantity - totalQuantity + currentQuantity;
    
                    CartWorkflowHelper.Calculate(context, existingTransaction, CalculationModes.All);
                    DiscountLine isApplied = existingTransaction.SalesLines[salesLineIndex].DiscountLines.Where(j => j.OfferId == discountLine.OfferId).SingleOrDefault();
    
                    // If the quantity discount will be applied then remove the discount line from the lines with same product and get the min quantity to buy for discount.
                    if (isApplied != null && (isApplied.Amount != 0 || isApplied.Percentage != 0))
                    {
                        int toBuy = (int)(multiBuyLine.MinimumQuantity - totalQuantity);
                        if (isApplied.Amount != 0)
                        {
                            discountLine.OfferName = string.Format(CultureInfo.CurrentUICulture, Resources.MultiBuyDiscountPricePromotion, toBuy, Math.Round(isApplied.Amount, 2));
                        }
                        else
                        {
                            discountLine.OfferName = string.Format(CultureInfo.CurrentUICulture, Resources.MultiBuyDiscountPercentagePromotion, toBuy, Math.Round(isApplied.Percentage, 2));
                        }
    
                        neverApplied = false;
                        break;
                    }
                }
    
                if (neverApplied)
                {
                    tempSalesTransaction.SalesLines[salesLineIndex].DiscountLines.Remove(discountLine);
                }
    
                existingTransaction.SalesLines[salesLineIndex].Quantity = currentQuantity;
                CartWorkflowHelper.Calculate(context, existingTransaction, CalculationModes.All);            
    
                foreach (SalesLine sameproductCartLine in salesLinesWithSameProduct)
                {
                    sameproductCartLine.DiscountLines.Remove(sameproductCartLine.DiscountLines.Where(k => k.OfferId == discountLine.OfferId).SingleOrDefault());
                }
            }
    
            /// <summary>
            /// Updates the sales transaction with the threshold promotion if applicable.
            /// </summary>
            /// <param name="existingTransaction">Existing transaction.</param>
            /// <param name="tempSalesTransaction">Copy of existing transaction.</param>
            /// <param name="context">The request context.</param>
            /// <param name="salesLineIndex">The sales line under consideration.</param>
            /// <param name="cartPromotionLines">The object with the cart promotion lines.</param>
            /// <param name="thresholdDiscount">The threshold discount line under consideration.</param>
            /// <param name="tiers">The tiers for the threshold discount.</param>
            private static void GetThresholdDiscounts(
                SalesTransaction existingTransaction,
                SalesTransaction tempSalesTransaction, 
                RequestContext context,
                int salesLineIndex, 
                Collection<string> cartPromotionLines, 
                DiscountLine thresholdDiscount, 
                IEnumerable<ThresholdDiscountTier> tiers)
            {
                // Find all the sales lines with the same offer.
                List<SalesLine> salesLinesWithOffer = tempSalesTransaction.SalesLines.Where(j => j.DiscountLines.Any(k => k.OfferId.Equals(thresholdDiscount.OfferId))).ToList();
                decimal totalAmount = salesLinesWithOffer.Select(j => j.GrossAmount).Sum();
                decimal currentQuantity = tempSalesTransaction.SalesLines[salesLineIndex].Quantity;
    
                // Find the minimum threshold amount required to hit a discount among all the tiers for this offer.
                IEnumerable<ThresholdDiscountTier> tiersForCurrentAmtOffer = tiers.Where(j => j.OfferId.Equals(thresholdDiscount.OfferId) && j.AmountThreshold > totalAmount).OrderBy(l => l.AmountThreshold);
                ThresholdDiscountTier tier = tiersForCurrentAmtOffer.Any() ? tiersForCurrentAmtOffer.First() : null;
    
                if (tier != null)
                {
                    // Add that amount difference to the first item that has this offer in the cart by increasing its quantity and see if this discount applies after applying concurrency rules.
                    existingTransaction.SalesLines[salesLineIndex].Quantity = 
                        Math.Ceiling(tempSalesTransaction.SalesLines[salesLineIndex].Quantity *
                        (tier.AmountThreshold - totalAmount + tempSalesTransaction.SalesLines[salesLineIndex].GrossAmount) / (tempSalesTransaction.SalesLines[salesLineIndex].GrossAmount / tempSalesTransaction.SalesLines[salesLineIndex].Quantity));
    
                    CartWorkflowHelper.Calculate(context, existingTransaction, CalculationModes.All);
                    DiscountLine isApplied = existingTransaction.SalesLines[salesLineIndex].DiscountLines.Where(j => j.OfferId.Equals(thresholdDiscount.OfferId)).SingleOrDefault();
    
                    if (isApplied != null)
                    {
                        var getItemsRequest = new GetItemsDataRequest(salesLinesWithOffer.Select(j => j.ItemId))
                        {
                            QueryResultSettings = new QueryResultSettings(new ColumnSet("NAME"), PagingInfo.AllRecords)
                        };
                        var getItemsResponse = context.Runtime.Execute<GetItemsDataResponse>(getItemsRequest, context);
    
                        ReadOnlyCollection<Item> items = getItemsResponse.Items;
                        StringBuilder buffer = new StringBuilder();
                        
                        foreach (Item item in items.ToList())
                        {
                            buffer.Append(item.Name).Append(", ");
                        }
    
                        buffer.Remove(buffer.Length - 2, 1);
    
                        if (tier.DiscountMethod == ThresholdDiscountMethod.AmountOff)
                        {
                            thresholdDiscount.OfferName = string.Format(CultureInfo.CurrentUICulture, Resources.ThresholdDiscountPricePromotion, buffer, Math.Round(tier.AmountThreshold, 2), Math.Round(tier.DiscountValue, 2));
                        }
                        else
                        {
                            thresholdDiscount.OfferName = string.Format(CultureInfo.CurrentUICulture, Resources.ThresholdDiscountPercentagePromotion, buffer, Math.Round(tier.AmountThreshold, 2), Math.Round(tier.DiscountValue, 2));
                        }
    
                        cartPromotionLines.Add(thresholdDiscount.OfferName);
                    }
                }
    
                existingTransaction.SalesLines[salesLineIndex].Quantity = currentQuantity;
                CartWorkflowHelper.Calculate(context, existingTransaction, CalculationModes.All);   
    
                foreach (SalesLine salesLineWithOffer in salesLinesWithOffer)
                {
                    salesLineWithOffer.DiscountLines.Remove(salesLineWithOffer.DiscountLines.Where(k => k.OfferId == thresholdDiscount.OfferId).SingleOrDefault());
                }            
            }
        }    
    }
}
