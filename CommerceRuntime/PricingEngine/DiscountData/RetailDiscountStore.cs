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
    namespace Commerce.Runtime.Services.PricingEngine.DiscountData
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Retail discount store.
        /// </summary>
        public static class RetailDiscountStore
        {
            [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Grandfathered.")]
            internal static Dictionary<long, List<DiscountBase>> GetProductOrVariantToDiscountMapLive(
                SalesTransaction transaction,
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager)
            {
                List<ItemUnit> items = new List<ItemUnit>();
                Dictionary<string, long> itemIdInventDimIdToProductOrVariantIdMap = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    
                foreach (SalesLine salesLine in transaction.PriceCalculableSalesLines)
                {
                    // The map is to look up product or variant id, but not master id if variant id is present.
                    itemIdInventDimIdToProductOrVariantIdMap[GetItemIdInventDimIdKey(salesLine.ItemId, salesLine.InventoryDimensionId)] = salesLine.ProductId;
    
                    items.Add(new ItemUnit() { ItemId = salesLine.ItemId, VariantInventoryDimensionId = salesLine.InventoryDimensionId, Product = salesLine.MasterProductId == 0 ? salesLine.ProductId : salesLine.MasterProductId, DistinctProductVariant = salesLine.Variant != null ? salesLine.Variant.DistinctProductVariantId : 0, UnitOfMeasure = Discount.GetUnitOfMeasure(salesLine) });
                }
    
                ReadOnlyCollection<PeriodicDiscount> discountAndLines = GetRetailDiscountsAndLines(items, priceContext, pricingDataManager, QueryResultSettings.AllRecords);
                ISet<long> productVariantMasterIdsInTransaction = GetProductVariantMasterIdsForTransaction(transaction);
    
                Dictionary<long, List<DiscountBase>> productDiscountMap = new Dictionary<long, List<DiscountBase>>();
                Dictionary<string, DiscountBase> offerIdToDiscountMap = new Dictionary<string, DiscountBase>(StringComparer.OrdinalIgnoreCase);
    
                foreach (PeriodicDiscount discountAndLine in discountAndLines)
                {
                    if (!PriceContextHelper.MatchCalculationMode(priceContext, discountAndLine.PeriodicDiscountType))
                    {
                        continue;
                    }
    
                    string key = GetItemIdInventDimIdKey(discountAndLine.ItemId, discountAndLine.InventoryDimensionId);
                    long productOrVariantId = 0;
                    if (itemIdInventDimIdToProductOrVariantIdMap.TryGetValue(key, out productOrVariantId))
                    {
                        DiscountBase discount = null;
    
                        if (offerIdToDiscountMap.TryGetValue(discountAndLine.OfferId, out discount))
                        {
                            RetailDiscountLine discountLine = null;
                            if (!discount.DiscountLines.TryGetValue(discountAndLine.DiscountLineNumber, out discountLine))
                            {
                                discountLine = ConvertDiscountAndLineToDiscountLine(discountAndLine, discount);
                                discount.DiscountLines.Add(discountLine.DiscountLineNumber, discountLine);
                            }
    
                            IList<RetailDiscountLine> discountLines = null;
                            if (discount.ProductOfVariantToDiscountLinesMap.TryGetValue(productOrVariantId, out discountLines))
                            {
                                discountLines.Add(discountLine);
                            }
                            else
                            {
                                discount.ProductOfVariantToDiscountLinesMap[productOrVariantId] = new List<RetailDiscountLine> { discountLine };
                            }
                        }
                        else
                        {
                            discount = ConvertDiscountAndLineToDiscountBase(discountAndLine);
                            discount.ProductOrVariantIdsInTransaction = productVariantMasterIdsInTransaction;
                            RetailDiscountLine discountLine = ConvertDiscountAndLineToDiscountLine(discountAndLine, discount);
                            discount.DiscountLines.Add(discountLine.DiscountLineNumber, discountLine);
                            offerIdToDiscountMap.Add(discount.OfferId, discount);
                            discount.ProductOfVariantToDiscountLinesMap[productOrVariantId] = new List<RetailDiscountLine> { discountLine };
                        }
    
                        List<DiscountBase> discounts;
                        if (productDiscountMap.TryGetValue(productOrVariantId, out discounts))
                        {
                            if (!discounts.Where(p => p.OfferId == discount.OfferId).Any())
                            {
                                discounts.Add(discount);
                            }
                        }
                        else
                        {
                            productDiscountMap[productOrVariantId] = new List<DiscountBase>() { discount };
                        }
                    }
                }
    
                IEnumerable<string> offerIds = offerIdToDiscountMap.Select(p => p.Key);
    
                if (offerIds.Any())
                {
                    IEnumerable<DiscountCode> discountCodes = pricingDataManager.GetDiscountCodesByOfferIds(offerIds) as IEnumerable<DiscountCode>;
    
                    foreach (DiscountCode discountCode in discountCodes)
                    {
                        DiscountBase discountBase;
                        if (offerIdToDiscountMap.TryGetValue(discountCode.OfferId, out discountBase))
                        {
                            // Accept both discount code and barcode in retail channel.
                            discountBase.DiscountCodes.Add(discountCode.Code);
                            discountBase.DiscountCodes.Add(discountCode.Barcode);
                        }
                    }
    
                    IEnumerable<RetailDiscountPriceGroup> discountPriceGroups = pricingDataManager.GetRetailDiscountPriceGroups(new HashSet<string>(offerIds)) as IEnumerable<RetailDiscountPriceGroup>;
    
                    foreach (RetailDiscountPriceGroup discountPriceGroup in discountPriceGroups)
                    {
                        offerIdToDiscountMap[discountPriceGroup.OfferId].PriceDiscountGroupIds.Add(discountPriceGroup.PriceGroupId);
                    }
    
                    SetEffectiveDiscountPriorityFromPriceGroups(offerIdToDiscountMap, priceContext);
    
                    IEnumerable<string> quantityOfferIds = offerIdToDiscountMap.Where(p => p.Value.PeriodicDiscountType == PeriodicDiscountOfferType.MultipleBuy).Select(p => p.Key);
    
                    if (quantityOfferIds.Any())
                    {
                        IEnumerable<QuantityDiscountLevel> quantityLevels = pricingDataManager.GetMultipleBuyDiscountLinesByOfferIds(quantityOfferIds) as IEnumerable<QuantityDiscountLevel>;
    
                        foreach (QuantityDiscountLevel quantityLevel in quantityLevels)
                        {
                            DiscountBase discountBase;
                            if (offerIdToDiscountMap.TryGetValue(quantityLevel.OfferId, out discountBase))
                            {
                                MultipleBuyDiscount multipleBuy = discountBase as MultipleBuyDiscount;
    
                                if (multipleBuy != null)
                                {
                                    multipleBuy.QuantityDiscountLevels.Add(quantityLevel);
                                }
                            }
                        }
                    }
    
                    IEnumerable<string> mixMatchOfferIds = offerIdToDiscountMap.Where(p => p.Value.PeriodicDiscountType == PeriodicDiscountOfferType.MixAndMatch).Select(p => p.Key);
    
                    if (mixMatchOfferIds.Any())
                    {
                        IEnumerable<MixAndMatchLineGroup> mixMatchLineGroups = pricingDataManager.GetMixAndMatchLineGroupsByOfferIds(mixMatchOfferIds) as IEnumerable<MixAndMatchLineGroup>;
    
                        foreach (MixAndMatchLineGroup lineGroup in mixMatchLineGroups)
                        {
                            DiscountBase discountBase;
                            if (offerIdToDiscountMap.TryGetValue(lineGroup.OfferId, out discountBase))
                            {
                                MixAndMatchDiscount mixMatch = discountBase as MixAndMatchDiscount;
    
                                if (mixMatch != null)
                                {
                                    mixMatch.LineGroupToNumberOfItemsMap.Add(lineGroup.LineGroup, lineGroup.NumberOfItemsNeeded);
                                }
                            }
                        }
                    }
    
                    IEnumerable<string> thresholdOfferIds = offerIdToDiscountMap.Where(p => p.Value.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold).Select(p => p.Key);
    
                    if (thresholdOfferIds.Any())
                    {
                        IEnumerable<ThresholdDiscountTier> thresholdTiers = pricingDataManager.GetThresholdTiersByOfferIds(thresholdOfferIds) as IEnumerable<ThresholdDiscountTier>;
    
                        foreach (ThresholdDiscountTier tier in thresholdTiers)
                        {
                            DiscountBase discountBase;
                            if (offerIdToDiscountMap.TryGetValue(tier.OfferId, out discountBase))
                            {
                                ThresholdDiscount threshold = discountBase as ThresholdDiscount;
    
                                if (threshold != null)
                                {
                                    threshold.ThresholdDiscountTiers.Add(tier);
                                }
                            }
                        }
                    }
                }
    
                return productDiscountMap;
            }
    
            internal static Dictionary<long, List<DiscountBase>> GetProductOrVarintToDiscountMapFromCache(
                IPricingDataAccessor pricingDataManager,
                PriceContext priceContext,
                SalesTransaction transaction)
            {
                ISet<long> productVariantMasterIdsInTransaction = GetProductVariantMasterIdsForTransaction(transaction);
                Dictionary<long, IList<RetailCategoryMember>> categorytoProductOrVariantIdsMap = GetCategoryToProductOrVariantIdsMapForTransaction(pricingDataManager, productVariantMasterIdsInTransaction);
    
                IEnumerable<RetailDiscount> allDiscounts = pricingDataManager.GetAllRetailDiscounts() as IEnumerable<RetailDiscount>;
    
                Dictionary<long, List<DiscountBase>> allApplicableDiscounts = new Dictionary<long, List<DiscountBase>>();
    
                foreach (RetailDiscount retailDiscount in allDiscounts)
                {
                    if (!PriceContextHelper.MatchCalculationMode(priceContext, retailDiscount.PeriodicDiscountType))
                    {
                        continue;
                    }
    
                    DiscountBase discount = ConvertRetailDiscountToDiscountBase(retailDiscount, priceContext);
                    discount.ProductOrVariantIdsInTransaction = productVariantMasterIdsInTransaction;
    
                    // Product or variant id to categories map is needed to filter which discount lines are applicable for the transaction. See DiscountBase class.
                    discount.CategoryToProductOrVariantIdsMap = categorytoProductOrVariantIdsMap;
                    IDictionary<long, IList<RetailDiscountLine>> itemDiscounts = discount.GetProductOrVariantIdToRetailDiscountLinesMap();
                    foreach (long productOrVariantId in itemDiscounts.Keys)
                    {
                        if (allApplicableDiscounts.ContainsKey(productOrVariantId))
                        {
                            allApplicableDiscounts[productOrVariantId].Add(discount);
                        }
                        else
                        {
                            allApplicableDiscounts.Add(productOrVariantId, new List<DiscountBase>() { discount });
                        }
                    }
                }
    
                return allApplicableDiscounts;
            }
    
            /// <summary>
            /// Converts retail discount data from database to discount object.
            /// </summary>
            /// <param name="retailDiscount">Retail discount data from database.</param>
            /// <returns>Discount object.</returns>
            /// <remarks>This is private. Exposed as internal for test.</remarks>
            internal static DiscountBase ConvertRetailDiscountToDiscountBase(RetailDiscount retailDiscount)
            {
                DiscountBase discount = null;
                OfferDiscount offer = null;
                MixAndMatchDiscount mixAndMatch = null;
                MultipleBuyDiscount multipleBuy = null;
                ThresholdDiscount threshold = null;
    
                switch (retailDiscount.PeriodicDiscountType)
                {
                    case PeriodicDiscountOfferType.Offer:
                        offer = new OfferDiscount(retailDiscount.ValidationPeriod);
                        discount = offer;
                        break;
                    case PeriodicDiscountOfferType.MixAndMatch:
                        mixAndMatch = new MixAndMatchDiscount(retailDiscount.ValidationPeriod);
                        mixAndMatch.DealPriceValue = retailDiscount.MixAndMatchDealPrice;
                        mixAndMatch.DiscountAmountValue = retailDiscount.MixAndMatchDiscountAmount;
                        mixAndMatch.DiscountPercentValue = retailDiscount.MixAndMatchDiscountPercent;
                        mixAndMatch.NumberOfLeastExpensiveLines = retailDiscount.MixAndMatchNumberOfLeastExpensiveLines;
                        mixAndMatch.LeastExpensiveMode = retailDiscount.LeastExpensiveMode;
                        mixAndMatch.NumberOfTimesApplicable = retailDiscount.MixAndMatchNumberOfTimeApplicable;
                        foreach (RetailDiscountLine mixMatchLine in retailDiscount.DiscountLines)
                        {
                            if (!mixAndMatch.LineGroupToNumberOfItemsMap.ContainsKey(mixMatchLine.MixAndMatchLineGroup))
                            {
                                mixAndMatch.LineGroupToNumberOfItemsMap.Add(mixMatchLine.MixAndMatchLineGroup, mixMatchLine.MixAndMatchLineNumberOfItemsNeeded);
                            }
                        }
    
                        discount = mixAndMatch;
                        break;
                    case PeriodicDiscountOfferType.MultipleBuy:
                        multipleBuy = new MultipleBuyDiscount(retailDiscount.ValidationPeriod);
                        multipleBuy.QuantityDiscountLevels.AddRange(retailDiscount.MultibuyQuantityTiers);
                        discount = multipleBuy;
                        break;
                    case PeriodicDiscountOfferType.Threshold:
                        threshold = new ThresholdDiscount(retailDiscount.ValidationPeriod);
                        threshold.ShouldCountNonDiscountItems = retailDiscount.ShouldCountNonDiscountItems != 0;
                        threshold.ThresholdDiscountTiers.AddRange(retailDiscount.ThresholdDiscountTiers);
                        discount = threshold;
                        break;
                }
    
                if (discount != null)
                {
                    discount.IsCategoryToProductOrVariantIdsMapSet = false;
    
                    discount.OfferId = retailDiscount.OfferId;
                    discount.OfferName = retailDiscount.Name;
                    discount.PeriodicDiscountType = retailDiscount.PeriodicDiscountType;
                    discount.IsDiscountCodeRequired = retailDiscount.IsDiscountCodeRequired;
                    discount.ConcurrencyMode = retailDiscount.ConcurrencyMode;
                    discount.PricingPriorityNumber = retailDiscount.PricingPriorityNumber;
                    discount.CurrencyCode = retailDiscount.CurrencyCode;
                    discount.DateValidationPeriodId = retailDiscount.ValidationPeriodId;
                    discount.DateValidationType = (DateValidationType)retailDiscount.DateValidationType;
                    discount.DiscountType = GetDiscountMethodType(discount.PeriodicDiscountType, retailDiscount.DiscountType);
                    discount.ValidFrom = retailDiscount.ValidFromDate;
                    discount.ValidTo = retailDiscount.ValidToDate;
    
                    foreach (RetailDiscountLine discountLine in retailDiscount.DiscountLines)
                    {
                        discountLine.DiscountMethod = (int)GetLineDiscountOfferMethod(discount.PeriodicDiscountType, discount.DiscountType, discountLine.DiscountMethod, discountLine.MixAndMatchLineSpecificDiscountType);
                        discount.DiscountLines.Add(discountLine.DiscountLineNumber, discountLine);
                    }
    
                    foreach (RetailDiscountPriceGroup priceGroup in retailDiscount.PriceGroups)
                    {
                        discount.PriceDiscountGroupIds.Add(priceGroup.PriceGroupId);
                    }
    
                    foreach (DiscountCode discountCode in retailDiscount.DiscountCodes)
                    {
                        discount.DiscountCodes.Add(discountCode.Code);
                        discount.DiscountCodes.Add(discountCode.Barcode);
                    }
                }
    
                return discount;
            }
    
            /// <summary>
            /// Converts retail discount data from database to discount object.
            /// </summary>
            /// <param name="retailDiscount">Retail discount data from database.</param>
            /// <param name="priceContext">Price context.</param>
            /// <returns>Discount object.</returns>
            /// <remarks>This is private. Exposed as internal for test.</remarks>
            internal static DiscountBase ConvertRetailDiscountToDiscountBase(RetailDiscount retailDiscount, PriceContext priceContext)
            {
                DiscountBase discount = ConvertRetailDiscountToDiscountBase(retailDiscount);
    
                SetEffectiveDiscounDiscountPriorityFromPriceGroups(discount, priceContext);
    
                return discount;
            }
    
            private static ISet<long> GetProductVariantMasterIdsForTransaction(SalesTransaction salesTransaction)
            {
                HashSet<long> productOrVariantIds = new HashSet<long>();
                foreach (SalesLine salesLine in salesTransaction.PriceCalculableSalesLines)
                {
                    productOrVariantIds.Add(salesLine.ProductId);
    
                    if (salesLine.ProductId != salesLine.MasterProductId)
                    {
                        productOrVariantIds.Add(salesLine.MasterProductId);
                    }
                }
    
                return productOrVariantIds;
            }
    
            /// <summary>
            /// Gets category to product or variant identifiers lookup for all items applicable of price on the transaction.
            /// </summary>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="productOrVariantIdsInTransaction">Product or variant identifiers in transaction.</param>
            /// <returns>A dictionary of category to product categories.</returns>
            private static Dictionary<long, IList<RetailCategoryMember>> GetCategoryToProductOrVariantIdsMapForTransaction(
                IPricingDataAccessor pricingDataManager,
                ISet<long> productOrVariantIdsInTransaction)
            {
                Dictionary<long, IList<RetailCategoryMember>> result = new Dictionary<long, IList<RetailCategoryMember>>();
    
                IEnumerable<RetailCategoryMember> allProductCategories = pricingDataManager.GetRetailCategoryMembersForItems(productOrVariantIdsInTransaction) as IEnumerable<RetailCategoryMember>;
    
                if (allProductCategories != null)
                {
                    foreach (RetailCategoryMember productCategory in allProductCategories)
                    {
                        IList<RetailCategoryMember> productCategories = null;
                        if (result.TryGetValue(productCategory.CategoryId, out productCategories))
                        {
                            productCategories.Add(productCategory);
                        }
                        else
                        {
                            productCategories = new List<RetailCategoryMember>() { productCategory };
                            result.Add(productCategory.CategoryId, productCategories);
                        }
                    }
                }
    
                return result;
            }
    
            private static DiscountMethodType GetDiscountMethodType(PeriodicDiscountOfferType periodicDiscountType, int discountTypeFromDatabase)
            {
                DiscountMethodType discountMethod = DiscountMethodType.DealPrice;
    
                // ISNULL(pdmm.MIXANDMATCHDISCOUNTTYPE, ISNULL(pdmb.MULTIBUYDISCOUNTTYPE, pd.PERIODICDISCOUNTTYPE))
                switch (periodicDiscountType)
                {
                    case PeriodicDiscountOfferType.Offer:
                    case PeriodicDiscountOfferType.Promotion:
                        discountMethod = DiscountMethodType.LineSpecific;
                        break;
                    case PeriodicDiscountOfferType.MixAndMatch:
                    case PeriodicDiscountOfferType.MultipleBuy:
                        discountMethod = (DiscountMethodType)discountTypeFromDatabase;
                        break;
                    case PeriodicDiscountOfferType.Threshold:
                        discountMethod = DiscountMethodType.LineSpecific;
                        break;
                    default:
                        NetTracer.Warning("Unsupported discount type: {0}", discountTypeFromDatabase);
                        break;
                }
    
                return discountMethod;
            }
    
            private static DiscountOfferMethod GetLineDiscountOfferMethod(PeriodicDiscountOfferType periodicDiscountType, DiscountMethodType discountMethod, int lineDiscountMethod, int lineSpecificDiscountType)
            {
                DiscountOfferMethod offerMethod = (DiscountOfferMethod)lineDiscountMethod;
    
                if (periodicDiscountType == PeriodicDiscountOfferType.MixAndMatch && discountMethod == DiscountMethodType.LineSpecific)
                {
                    if (lineSpecificDiscountType == (int)DiscountMethodType.DealPrice)
                    {
                        offerMethod = DiscountOfferMethod.OfferPrice;
                    }
                    else if (lineSpecificDiscountType == (int)DiscountMethodType.DiscountPercent)
                    {
                        offerMethod = DiscountOfferMethod.DiscountPercent;
                    }
                }
    
                return offerMethod;
            }
    
            private static RetailDiscountLine ConvertDiscountAndLineToDiscountLine(PeriodicDiscount discountAndLine, DiscountBase discount)
            {
                RetailDiscountLine discountLine = new RetailDiscountLine();
                discountLine.OfferId = discountAndLine.OfferId;
                discountLine.DiscountLineNumber = discountAndLine.DiscountLineNumber;
                discountLine.ProductId = discountAndLine.ProductId;
                discountLine.DistinctProductVariantId = discountAndLine.DistinctProductVariantId;
                discountLine.DiscountAmount = discountAndLine.DiscountAmount;
                discountLine.DiscountLinePercentOrValue = discountAndLine.DiscountLinePercentOrValue;
                discountLine.DiscountMethod = (int)GetLineDiscountOfferMethod(discount.PeriodicDiscountType, discount.DiscountType, discountAndLine.DiscountMethod, discountAndLine.MixAndMatchLineSpecificDiscountType);
                discountLine.DiscountPercent = discountAndLine.DiscountPercent;
                discountLine.MixAndMatchLineGroup = discountAndLine.MixAndMatchLineGroup;
                discountLine.MixAndMatchLineNumberOfItemsNeeded = discountAndLine.MixAndMatchLineNumberOfItemsNeeded;
                discountLine.MixAndMatchLineSpecificDiscountType = discountAndLine.MixAndMatchLineSpecificDiscountType;
                discountLine.OfferPrice = discountAndLine.OfferPrice;
                discountLine.UnitOfMeasureSymbol = discountAndLine.UnitOfMeasureSymbol;
    
                return discountLine;
            }
    
            private static DiscountBase ConvertDiscountAndLineToDiscountBase(PeriodicDiscount discountAndLine)
            {
                DiscountBase discount = null;
                OfferDiscount offer = null;
                MixAndMatchDiscount mixAndMatch = null;
                MultipleBuyDiscount multipleBuy = null;
                ThresholdDiscount threshold = null;
    
                switch (discountAndLine.PeriodicDiscountType)
                {
                    case PeriodicDiscountOfferType.Offer:
                        offer = new OfferDiscount(discountAndLine.ValidationPeriod);
                        discount = offer;
                        break;
                    case PeriodicDiscountOfferType.MixAndMatch:
                        mixAndMatch = new MixAndMatchDiscount(discountAndLine.ValidationPeriod);
                        mixAndMatch.DealPriceValue = discountAndLine.MixAndMatchDealPrice;
                        mixAndMatch.DiscountAmountValue = discountAndLine.MixAndMatchDiscountAmount;
                        mixAndMatch.DiscountPercentValue = discountAndLine.MixAndMatchDiscountPercent;
                        mixAndMatch.NumberOfLeastExpensiveLines = discountAndLine.MixAndMatchNumberOfLeastExpensiveLines;
                        mixAndMatch.NumberOfTimesApplicable = discountAndLine.MixAndMatchNumberOfTimeApplicable;
                        mixAndMatch.LeastExpensiveMode = discountAndLine.LeastExpensiveMode;
                        discount = mixAndMatch;
                        break;
                    case PeriodicDiscountOfferType.MultipleBuy:
                        multipleBuy = new MultipleBuyDiscount(discountAndLine.ValidationPeriod);
                        discount = multipleBuy;
                        break;
                    case PeriodicDiscountOfferType.Threshold:
                        threshold = new ThresholdDiscount(discountAndLine.ValidationPeriod);
                        threshold.ShouldCountNonDiscountItems = discountAndLine.ShouldCountNonDiscountItems != 0;
                        discount = threshold;
                        break;
                }
    
                if (discount != null)
                {
                    discount.IsCategoryToProductOrVariantIdsMapSet = true;
    
                    discount.OfferId = discountAndLine.OfferId;
                    discount.OfferName = discountAndLine.Name;
                    discount.PeriodicDiscountType = discountAndLine.PeriodicDiscountType;
                    discount.IsDiscountCodeRequired = discountAndLine.IsDiscountCodeRequired;
                    discount.ConcurrencyMode = discountAndLine.ConcurrencyMode;
                    discount.PricingPriorityNumber = discountAndLine.PricingPriorityNumber;
                    discount.CurrencyCode = discountAndLine.CurrencyCode;
                    discount.DateValidationPeriodId = discountAndLine.ValidationPeriodId;
                    discount.DateValidationType = (DateValidationType)discountAndLine.DateValidationType;
                    discount.DiscountType = GetDiscountMethodType(discount.PeriodicDiscountType, discountAndLine.DiscountType);
                    discount.ValidFrom = discountAndLine.ValidFromDate;
                    discount.ValidTo = discountAndLine.ValidToDate;
                }
    
                return discount;
            }
    
            private static string GetItemIdInventDimIdKey(string itemId, string inventDimId)
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}-@-{1}", itemId, inventDimId);
            }

            /// <summary>
            /// Gets list of discount lines corresponding to products.
            /// </summary>
            /// <param name="items">The collection of products.</param>
            /// <param name="priceContext">The pricing context.</param>
            /// <param name="pricingDataManager">The Pricing data manager.</param>
            /// <param name="settings">Query Settings.</param>
            /// <returns>List of discounts.</returns>
            /// <remarks>The method returns flattened list of discounts which roughly speaking means: it returns 1 line per each possible discount for every product.</remarks>
            private static ReadOnlyCollection<PeriodicDiscount> GetRetailDiscountsAndLines(
                IEnumerable<ItemUnit> items,
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager,
                QueryResultSettings settings)
            {
                // don't do lookup if there aren't any price groups to search by
                HashSet<string> allPriceGroups = PriceContextHelper.GetAllPriceGroupsForDiscount(priceContext);
                if (allPriceGroups.Count == 0)
                {
                    return new ReadOnlyCollection<PeriodicDiscount>(new PeriodicDiscount[0]);
                }
    
                ReadOnlyCollection<PeriodicDiscount> discounts =
                    pricingDataManager.ReadRetailDiscounts(items, allPriceGroups, priceContext.ActiveDate, priceContext.ActiveDate, priceContext.CurrencyCode, settings) as ReadOnlyCollection<PeriodicDiscount>;
    
                ReadOnlyCollection<PeriodicDiscount> validDiscounts = discounts.Where(p => InternalValidationPeriod.ValidateDateAgainstValidationPeriod((DateValidationType)p.DateValidationType, p.ValidationPeriod, p.ValidFromDate, p.ValidToDate, priceContext.ActiveDate)).AsReadOnly();
    
                return validDiscounts;
            }
    
            private static void SetEffectiveDiscountPriorityFromPriceGroups(
                Dictionary<string, DiscountBase> discountsLookup,
                PriceContext priceContext)
            {
                foreach (KeyValuePair<string, DiscountBase> pair in discountsLookup)
                {
                    DiscountBase discount = pair.Value;
                    SetEffectiveDiscounDiscountPriorityFromPriceGroups(discount, priceContext);
                }
            }
    
            private static void SetEffectiveDiscounDiscountPriorityFromPriceGroups(
                DiscountBase discount,
                PriceContext priceContext)
            {
                if (discount.PricingPriorityNumber <= 0)
                {
                    foreach (long priceGroupRecordId in discount.PriceDiscountGroupIds)
                    {
                        string priceGroupId = null;
                        if (priceContext.RecordIdsToPriceGroupIdsDictionary.TryGetValue(priceGroupRecordId, out priceGroupId))
                        {
                            int priority = 0;
                            if (priceContext.PriceGroupIdToPriorityDictionary.TryGetValue(priceGroupId, out priority))
                            {
                                if (priority > discount.PricingPriorityNumber)
                                {
                                    // We could have a new property on DiscountBase that indicates effective priority number.
                                    // This is much simpler, for now, without complications.
                                    discount.PricingPriorityNumber = priority;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
