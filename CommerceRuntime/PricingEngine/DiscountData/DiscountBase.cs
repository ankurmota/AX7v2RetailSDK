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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Abstract class containing all of the standard properties that are shared across discount types.  For specific discount types, see one of the implementations of this class.
        /// </summary>
        public abstract class DiscountBase
        {
            /// <summary>
            /// The value for the NumberOfTimesApplicable property that indicates that there is no limit.
            /// </summary>
            public static readonly int UnlimitedNumberOfTimesApplicable = 0;
    
            /// <summary>
            /// Constant used to determine if a product ID or variant ID was not set.
            /// </summary>
            public static readonly long AnyProductOrVariant = 0;
    
            /// <summary>
            /// Constant used to determine if an index value is invalid or not found.
            /// </summary>
            protected const int InvalidIndex = -1;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="DiscountBase" /> class.
            /// </summary>
            /// <param name="validationPeriod">Validation period.</param>
            protected DiscountBase(ValidationPeriod validationPeriod)
            {
                this.ValidationPeriod = validationPeriod;
                this.DiscountLines = new Dictionary<decimal, RetailDiscountLine>();
                this.DiscountCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                this.PriceDiscountGroupIds = new HashSet<long>();
    
                //// DiscountBase list is loaded in 2 ways:
                //// 1. load all retail discounts and then filter by sales lines
                ////    product to category map is needed to filter discount lines and IsProductOrVariantIdToDiscountLinesMapSet is false initially.
                //// 2. load retail discounts by sales lines
                ////    filtering is done already and no more addition processing. IsProductOrVariantIdToDiscountLinesMapSet is true from the beginning.
                this.ProductOfVariantToDiscountLinesMap = new Dictionary<long, IList<RetailDiscountLine>>();
                this.CategoryToProductOrVariantIdsMap = new Dictionary<long, IList<RetailCategoryMember>>();
                this.IsCategoryToProductOrVariantIdsMapSet = false;
    
                this.DiscountLineNumberToItemGroupIndexSetMap = new Dictionary<decimal, HashSet<int>>();
                this.ItemGroupIndexToDiscountLineNumberSetMap = new Dictionary<int, HashSet<decimal>>();
            }
    
            /// <summary>
            /// Gets the validation period.
            /// </summary>
            public ValidationPeriod ValidationPeriod { get; private set; }
    
            /// <summary>
            /// Gets or sets the offer ID for the discount.
            /// </summary>
            public string OfferId { get; set; }
    
            /// <summary>
            /// Gets or sets the currency code required for this discount.
            /// </summary>
            public string CurrencyCode { get; set; }
    
            /// <summary>
            /// Gets the pricing group identifiers for this discount.
            /// </summary>
            public ISet<long> PriceDiscountGroupIds { get; private set; }
    
            /// <summary>
            /// Gets or sets the name of this discount offer.
            /// </summary>
            public string OfferName { get; set; }
    
            /// <summary>
            /// Gets or sets the type of discount (mix and match, quantity, threshold, etc.).
            /// </summary>
            public PeriodicDiscountOfferType PeriodicDiscountType { get; set; }
    
            /// <summary>
            /// Gets or sets the concurrency mode for this discount (exclusive, best-price, compound).
            /// </summary>
            public ConcurrencyMode ConcurrencyMode { get; set; }
    
            /// <summary>
            /// Gets or sets the pricing priority number.
            /// </summary>
            public int PricingPriorityNumber { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether or not a discount code is required to trigger this discount.
            /// </summary>
            public bool IsDiscountCodeRequired { get; set; }
    
            /// <summary>
            /// Gets the collection containing all of the discount codes that can trigger this discount, if one is required.
            /// </summary>
            public ISet<string> DiscountCodes { get; private set; }
    
            /// <summary>
            /// Gets or sets the validation period ID for testing if this offer is valid.
            /// </summary>
            public string DateValidationPeriodId { get; set; }
    
            /// <summary>
            /// Gets or sets the validation type to use for date validation for this offer.
            /// </summary>
            public DateValidationType DateValidationType { get; set; }
    
            /// <summary>
            /// Gets or sets the starting date for this offer.
            /// </summary>
            public DateTimeOffset ValidFrom { get; set; }
    
            /// <summary>
            /// Gets or sets the expiration date for this offer.
            /// </summary>
            public DateTimeOffset ValidTo { get; set; }
    
            /// <summary>
            /// Gets or sets the discount method type for this offer.
            /// </summary>
            public DiscountMethodType DiscountType { get; set; }
    
            /// <summary>
            /// Gets or sets the deal-price value of this offer.
            /// </summary>
            public decimal DealPriceValue { get; set; }
    
            /// <summary>
            /// Gets or sets the discount percentage value for this offer.
            /// </summary>
            public decimal DiscountPercentValue { get; set; }
    
            /// <summary>
            /// Gets or sets the discount amount value for this offer.
            /// </summary>
            public decimal DiscountAmountValue { get; set; }
    
            /// <summary>
            /// Gets or sets the number of times this offer can be applied to a transaction.
            /// </summary>
            public int NumberOfTimesApplicable { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether non-discount items contribute to threshold amount trigger.
            /// </summary>
            public bool ShouldCountNonDiscountItems { get; set; }
    
            /// <summary>
            /// Gets the retail discount line number to retail discount line map.
            /// </summary>
            internal IDictionary<decimal, RetailDiscountLine> DiscountLines { get; private set; }
    
            /// <summary>
            /// Gets or sets product or variant identifiers in transaction.
            /// </summary>
            internal ISet<long> ProductOrVariantIdsInTransaction { get; set; }
    
            /// <summary>
            /// Gets product or variant id to retail discount lines map that are relevant for the transaction.
            /// </summary>
            internal IDictionary<long, IList<RetailDiscountLine>> ProductOfVariantToDiscountLinesMap { get; private set; }
    
            /// <summary>
            /// Gets discount line number to item group index set lookup.
            /// </summary>
            /// <remarks>It gets initialized twice, along with ItemGroupIndexToDiscountLineNumberSetMap, one for sales, one for return.</remarks>
            internal Dictionary<decimal, HashSet<int>> DiscountLineNumberToItemGroupIndexSetMap { get; private set; }
    
            /// <summary>
            /// Gets item group index to discount line number set lookup.
            /// </summary>
            /// <remarks>It gets initialized twice, along with DiscountLineNumberToItemGroupIndexSetMap, one for sales, one for return.</remarks>
            internal Dictionary<int, HashSet<decimal>> ItemGroupIndexToDiscountLineNumberSetMap { get; private set; }
    
            /// <summary>
            /// Gets or sets product or variant to categories map that is relevant for the transaction.
            /// </summary>
            internal Dictionary<long, IList<RetailCategoryMember>> CategoryToProductOrVariantIdsMap { get; set; }
    
            internal bool IsCategoryToProductOrVariantIdsMapSet { get; set; }
    
            internal bool IsFinished { get; set; }
    
            internal bool CanCompound
            {
                get { return this.ConcurrencyMode == Microsoft.Dynamics.Commerce.Runtime.DataModel.ConcurrencyMode.Compounded; }
            }

            /// <summary>
            /// Gets all of the possible applications of this discount to the specified transaction and line items.
            /// </summary>
            /// <param name="transaction">The transaction to consider for discounts.</param>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <param name="appliedDiscounts">Applied discount.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="isInterrupted">A flag indicating whether it's interrupted for too many discount applications.</param>
            /// <returns>The possible permutations of line items that this discount can apply to, or an empty collection if this discount cannot apply.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            public abstract IEnumerable<DiscountApplication> GetDiscountApplications(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                PriceContext priceContext,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                out bool isInterrupted);

            /// <summary>
            /// Gets non-overlapped best-deal discount applications that can be applied right away.
            /// </summary>
            /// <param name="transaction">The transaction to consider for discounts.</param>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <param name="appliedDiscounts">Applied discount application.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>Non-overlapped discount applications that can be applied right away.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            public abstract IEnumerable<DiscountApplication> GetDiscountApplicationsNonOverlappedWithBestDeal(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                PriceContext priceContext,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly);

            /// <summary>
            /// Applies the discount application and gets the value, taking into account previously applied discounts.
            /// </summary>
            /// <param name="discountableItemGroups">The transaction line items.</param>
            /// <param name="remainingQuantities">The quantities remaining for each item.</param>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="discountApplication">The specific application of the discount to use.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>The value of the discount application.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            public abstract AppliedDiscountApplication GetAppliedDiscountApplication(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountApplication discountApplication,
                PriceContext priceContext);
    
            /// <summary>
            /// Generate discount lines for the applied discount application.
            /// </summary>
            /// <param name="appliedDiscountApplication">The applied discount application.</param>
            /// <param name="discountableItemGroups">The discountable item groups.</param>
            /// <param name="priceContext">The price context.</param>
            public abstract void GenerateDiscountLines(
                AppliedDiscountApplication appliedDiscountApplication,
                DiscountableItemGroup[] discountableItemGroups,
                PriceContext priceContext);
    
            /// <summary>
            /// Gets product or variant Id to retail discount lines map.
            /// </summary>
            /// <returns>Product or variant Id to retail discount lines map.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Backward compatibility.")]
            public IDictionary<long, IList<RetailDiscountLine>> GetProductOrVariantIdToRetailDiscountLinesMap()
            {
                //// DiscountBase list is loaded in 2 ways:
                //// 1. load all retail discounts and then filter by sales lines
                ////    product to category map is needed to filter discount lines and IsProductOrVariantIdToDiscountLinesMapSet is false initially.
                //// 2. load retail discounts by sales lines
                ////    filtering is done already and no more addition processing. IsProductOrVariantIdToDiscountLinesMapSet is true from the beginning.
                if (!this.IsCategoryToProductOrVariantIdsMapSet && this.ProductOrVariantIdsInTransaction != null)
                {
                    foreach (KeyValuePair<decimal, RetailDiscountLine> pair in this.DiscountLines)
                    {
                        RetailDiscountLine line = pair.Value;
    
                        if (line.DistinctProductVariantId != AnyProductOrVariant)
                        {
                            if (this.ProductOrVariantIdsInTransaction.Contains(line.DistinctProductVariantId))
                            {
                                this.AddDiscountLineToDictionary(line.DistinctProductVariantId, line);
                            }
                        }
                        else if (line.ProductId != AnyProductOrVariant)
                        {
                            if (this.ProductOrVariantIdsInTransaction.Contains(line.ProductId))
                            {
                                this.AddDiscountLineToDictionary(line.ProductId, line);
                            }
                        }
                        else if (this.CategoryToProductOrVariantIdsMap != null)
                        {
                            if (this.CategoryToProductOrVariantIdsMap.ContainsKey(line.CategoryId))
                            {
                                foreach (RetailCategoryMember member in this.CategoryToProductOrVariantIdsMap[line.CategoryId])
                                {
                                    this.AddDiscountLineToDictionary(member.ProductOrVariantId, line);
                                }
                            }
                        }
                    }
    
                    this.IsCategoryToProductOrVariantIdsMapSet = true;
                }
    
                return this.ProductOfVariantToDiscountLinesMap;
            }
    
            internal static decimal GetDiscountAmountForDealUnitPrice(decimal itemPrice, decimal dealUnitPrice)
            {
                decimal discountAmount = itemPrice - Math.Max(dealUnitPrice, decimal.Zero);
    
                return Math.Max(discountAmount, decimal.Zero);
            }
    
            internal static decimal GetDiscountAmountForPercentageOff(decimal itemPrice, decimal percentageOff)
            {
                percentageOff = Math.Min(percentageOff, 100m);
                percentageOff = Math.Max(percentageOff, decimal.Zero);
    
                decimal discountAmount = (itemPrice * percentageOff) / 100m;
    
                return discountAmount;
            }
    
            internal static void AddToItemQuantities(Dictionary<int, decimal> itemQuantities, int itemGroupIndex, decimal quantity)
            {
                decimal existingQuantity = decimal.Zero;
                if (itemQuantities.TryGetValue(itemGroupIndex, out existingQuantity))
                {
                    itemQuantities[itemGroupIndex] = quantity + existingQuantity;
                }
                else
                {
                    itemQuantities[itemGroupIndex] = quantity;
                }
            }
    
    #if DEBUG
            internal static void DebugDiscounts(string header, IEnumerable<DiscountBase> discounts)
            {
                System.Diagnostics.Debug.WriteLine("[{0}] possible discounts", header);
    
                foreach (DiscountBase discount in discounts)
                {
                    discount.DebugDiscount();
                }
            }
    #endif
    
            internal void InitializeAndPrepareDiscountLineNumberAndItemGroupIndexLookups(DiscountableItemGroup[] discountableItemGroups, PriceContext priceContext)
            {
                this.InitializeLookups();
    
                HashSet<string> discountPriceGroups = new HashSet<string>(ConvertPriceDiscountGroupIdsToGroups(this.PriceDiscountGroupIds, priceContext));
                IDictionary<long, IList<RetailDiscountLine>> productOrVariantToDiscountLinesLookup = this.GetProductOrVariantIdToRetailDiscountLinesMap();
    
                for (int i = 0; i < discountableItemGroups.Length; i++)
                {
                    DiscountableItemGroup discountableItemGroup = discountableItemGroups[i];
                    if (discountableItemGroup.Quantity > decimal.Zero)
                    {
                        if ((this.PeriodicDiscountType == PeriodicDiscountOfferType.Threshold || DiscountBase.IsDiscountAllowedForDiscountableItemGroup(discountableItemGroup)) &&
                            DiscountBase.IsDiscountAllowedForCatalogIds(priceContext, discountPriceGroups, discountableItemGroup.CatalogIds))
                        {
                            IList<RetailDiscountLine> discountLines = null;
                            if (productOrVariantToDiscountLinesLookup.TryGetValue(discountableItemGroup.ProductId, out discountLines))
                            {
                                this.PopulateDiscountLineNumberAndItemGroupIndexLookups(discountLines, discountableItemGroup, i);
                            }
    
                            if (discountableItemGroup.MasterProductId != discountableItemGroup.ProductId && productOrVariantToDiscountLinesLookup.TryGetValue(discountableItemGroup.MasterProductId, out discountLines))
                            {
                                this.PopulateDiscountLineNumberAndItemGroupIndexLookups(discountLines, discountableItemGroup, i);
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// Determines if this discount can possibly apply to the specified transaction by examining all of the triggering rules not related to the actual line items on the transaction.
            /// </summary>
            /// <param name="transaction">The transaction to use for checking the triggering rules.</param>
            /// <param name="storePriceGroups">The collection of price groups that the store belongs to.</param>
            /// <param name="currencyCode">The currency code for the current transaction.</param>
            /// <param name="isReturn">The flag indicating whether or not it's for return.</param>
            /// <param name="priceContext">Price context object.</param>
            /// <returns>True if the discount could apply if the correct line items exist, false otherwise.</returns>
            internal bool CanDiscountApply(SalesTransaction transaction, IEnumerable<long> storePriceGroups, string currencyCode, bool isReturn, PriceContext priceContext)
            {
                ThrowIf.Null(priceContext, "priceContext");
    
                bool canApply = false;
    
                if (transaction == null || storePriceGroups == null || priceContext == null)
                {
                    return canApply;
                }
    
                // Check to see if a discount code is required first.
                if (this.IsDiscountCodeRequired && isReturn == false)
                {
                    foreach (var discountCode in transaction.DiscountCodes)
                    {
                        if (this.DiscountCodes.Contains(discountCode))
                        {
                            canApply = true;
                            break;
                        }
                    }
                }
                else
                {
                    canApply = true;
                }
    
                if (!canApply)
                {
                    return false;
                }
    
                // Now examine the price/discount groups
                canApply = false;
    
                foreach (var priceDiscountGroup in storePriceGroups)
                {
                    if (this.PriceDiscountGroupIds.Contains(priceDiscountGroup))
                    {
                        canApply = true;
                        break;
                    }
                }
    
                if (!canApply)
                {
                    return false;
                }
    
                // Check the valid dates and validation type
                // User active date from price context object
                canApply = false;
    
                if ((this.ValidFrom.Date <= priceContext.ActiveDate.Date || this.ValidFrom.Date <= InternalValidationPeriod.NoDate)
                    && (this.ValidTo.Date >= priceContext.ActiveDate.Date || this.ValidTo.Date <= InternalValidationPeriod.NoDate))
                {
                    canApply = InternalValidationPeriod.ValidateDateAgainstValidationPeriod(this.DateValidationType, this.ValidationPeriod, this.ValidFrom, this.ValidTo, priceContext.ActiveDate);
                }
    
                if (!canApply)
                {
                    return false;
                }
    
                // Check the currency code
                canApply = false;
                if (string.IsNullOrWhiteSpace(this.CurrencyCode) || this.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    canApply = true;
                }
    
                return canApply;
            }
    
            /// <summary>
            /// Gets discount applications in fast mode.
            /// </summary>
            /// <param name="transaction">The transaction to consider for discounts.</param>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <returns>Discount applications to apply standalone.</returns>
            protected internal abstract IEnumerable<DiscountApplication> GetDiscountApplicationsFastMode(
                SalesTransaction transaction,
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities);
    
            /// <summary>
            /// Determines whether the discount can be applied standalone, i.e. not competing with other discounts.
            /// </summary>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>true if it can be applied standalone, otherwise false.</returns>
            protected internal virtual bool CanApplyStandalone(
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                return false;
            }
    
            /// <summary>
            /// Get single item non-overlapped discount result.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <param name="price">Item price.</param>
            /// <param name="quantity">Item quantity.</param>
            /// <returns>Single item non-overlapped discount result.</returns>
            /// <remarks>To compare offer discounts and quantity discounts.</remarks>
            protected internal virtual SingleItemNonOverlappedDiscountResult GetSingleItemNonOverlappedDiscountResult(
                int itemGroupIndex,
                decimal price,
                decimal quantity)
            {
                return SingleItemNonOverlappedDiscountResult.NotApplicable;
            }
    
            /// <summary>
            /// Determines if the item is likely evaluated with other items.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <returns>true if it's likely evaluated with other items; false otherwise.</returns>
            protected internal virtual bool IsItemLikelyEvaluatedWithOtherItems(int itemGroupIndex)
            {
                return true;
            }
    
            /// <summary>
            /// Pre optimization.
            /// </summary>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            protected internal abstract void PreOptimization(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly);
    
            /// <summary>
            /// Gets the discount deal estimate.
            /// </summary>
            /// <param name="discountableItemGroups">The valid sales line items on the transaction to consider.</param>
            /// <param name="remainingQuantities">The remaining quantities of each of the sales lines to consider.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>Discount deal estimate.</returns>
            protected internal abstract DiscountDealEstimate GetDiscountDealEstimate(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly);
    
    #if DEBUG
            /// <summary>
            /// Debug discount.
            /// </summary>
            protected internal virtual void DebugDiscount()
            {
                if (DiscountCalculator.LogDiscountDetails)
                {
                    System.Diagnostics.Debug.WriteLine("  Offer [{0}] Concurrency [{1}]", this.OfferId, this.ConcurrencyMode);
    
                    System.Diagnostics.Debug.WriteLine("  Discount line number to item group index set lookup.");
                    foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                    {
                        System.Text.StringBuilder itemGroupIndices = new System.Text.StringBuilder();
                        itemGroupIndices.AppendFormat("    Discount line number [{0}] Indices:", pair.Key);
                        foreach (int index in pair.Value)
                        {
                            itemGroupIndices.AppendFormat(" [{0}]", index);
                        }
    
                        System.Diagnostics.Debug.WriteLine(itemGroupIndices.ToString());
                    }
    
                    System.Diagnostics.Debug.WriteLine("  Item group index to discount line number set lookup.");
                    foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                    {
                        System.Text.StringBuilder discountLineNumbers = new System.Text.StringBuilder();
                        discountLineNumbers.AppendFormat("    Item group index [{0}] Discount line numbers:", pair.Key);
                        foreach (decimal discountLineNumber in pair.Value)
                        {
                            discountLineNumbers.AppendFormat(" [{0}]", discountLineNumber);
                        }
    
                        System.Diagnostics.Debug.WriteLine(discountLineNumbers.ToString());
                    }
                }
            }
    #endif
    
            /// <summary>
            /// Clean up lookups.
            /// </summary>
            /// <remarks>Remove discount line numbers with empty item group index sets.</remarks>
            protected internal virtual void CleanupLookups()
            {
                HashSet<decimal> discountLineNumberSetToRemove = new HashSet<decimal>();
                foreach (KeyValuePair<decimal, HashSet<int>> pair in this.DiscountLineNumberToItemGroupIndexSetMap)
                {
                    if (!pair.Value.Any())
                    {
                        discountLineNumberSetToRemove.Add(pair.Key);
                    }
                }
    
                foreach (decimal discountLineNumber in discountLineNumberSetToRemove)
                {
                    this.DiscountLineNumberToItemGroupIndexSetMap.Remove(discountLineNumber);
                }
    
                if (!this.IsFinished && !this.ItemGroupIndexToDiscountLineNumberSetMap.Any())
                {
                    this.IsFinished = true;
                }
            }
    
            /// <summary>
            /// Remove item group index from lookups.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            protected internal virtual void RemoveItemIndexGroupFromLookups(int itemGroupIndex)
            {
                HashSet<decimal> discountLineNumberSet = null;
    
                if (this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemGroupIndex, out discountLineNumberSet))
                {
                    foreach (decimal discountLineNumber in discountLineNumberSet)
                    {
                        HashSet<int> itemGroupIndexSetForDiscountLineNumber = null;
                        if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumber, out itemGroupIndexSetForDiscountLineNumber))
                        {
                            itemGroupIndexSetForDiscountLineNumber.Remove(itemGroupIndex);
                        }
                    }
    
                    this.ItemGroupIndexToDiscountLineNumberSetMap.Remove(itemGroupIndex);
                }
            }
    
            /// <summary>
            /// Removes discount line numbers from lookups.
            /// </summary>
            /// <param name="discountLineNumbersToRemove">Discount line numbers to remove.</param>
            protected internal virtual void RemoveDiscountLineNumbersFromLookups(HashSet<decimal> discountLineNumbersToRemove)
            {
                if (discountLineNumbersToRemove != null)
                {
                    foreach (decimal discountLineNumberToRemove in discountLineNumbersToRemove)
                    {
                        HashSet<int> itemGroupIndexSet = this.DiscountLineNumberToItemGroupIndexSetMap[discountLineNumberToRemove];
                        foreach (int itemGroupIndex in itemGroupIndexSet)
                        {
                            HashSet<decimal> discountLineNumberSetForItem = this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex];
                            discountLineNumberSetForItem.Remove(discountLineNumberToRemove);
                            if (discountLineNumberSetForItem.Count == 0)
                            {
                                this.ItemGroupIndexToDiscountLineNumberSetMap.Remove(itemGroupIndex);
                            }
                        }
    
                        this.DiscountLineNumberToItemGroupIndexSetMap.Remove(discountLineNumberToRemove);
                    }
                }
            }
    
            /// <summary>
            /// Creates a new DiscountLine that is a copy of the specified line.
            /// </summary>
            /// <param name="original">The original discount line.</param>
            /// <returns>The new discount line.</returns>
            protected static DiscountLine CloneDiscountItem(DiscountLine original)
            {
                DiscountLine newLine = new DiscountLine();
                newLine.CopyFrom(original);
    
                return newLine;
            }
    
            /// <summary>
            /// Determines whether the specified item group is eligible for discounts (based on the NoDiscount flag).
            /// </summary>
            /// <param name="discountableItemGroup">The item group to examine.</param>
            /// <returns>True if the item may be discounted, false otherwise.</returns>
            protected static bool IsDiscountAllowedForDiscountableItemGroup(DiscountableItemGroup discountableItemGroup)
            {
                if (discountableItemGroup != null && discountableItemGroup.ExtendedProperties != null)
                {
                    return !discountableItemGroup.ExtendedProperties.NoDiscountAllowed;
                }
                else
                {
                    return false;
                }
            }
    
            /// <summary>
            /// Translates price discount groups from record identifiers into the text identifiers for the groups.
            /// </summary>
            /// <param name="priceDiscountGroupIds">The record identifiers for the discount groups.</param>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <returns>A collection of text identifiers for the price discount groups.</returns>
            protected static IEnumerable<string> ConvertPriceDiscountGroupIdsToGroups(ISet<long> priceDiscountGroupIds, PriceContext priceContext)
            {
                foreach (long id in priceDiscountGroupIds)
                {
                    if (priceContext.RecordIdsToPriceGroupIdsDictionary.ContainsKey(id))
                    {
                        yield return priceContext.RecordIdsToPriceGroupIdsDictionary[id];
                    }
                }
            }
    
            /// <summary>
            /// Determines if the discount is allowed for the specified catalog identifiers, based on the discount price groups for this discount.
            /// </summary>
            /// <param name="priceContext">The pricing context to use.</param>
            /// <param name="discountPriceGroups">The set of discount price group text identifiers.</param>
            /// <param name="itemCatalogIds">The catalog IDs for the item.</param>
            /// <returns>True if the discount is allowed for this catalog, false otherwise.</returns>
            protected static bool IsDiscountAllowedForCatalogIds(PriceContext priceContext, ISet<string> discountPriceGroups, ISet<long> itemCatalogIds)
            {
                return PriceContextHelper.IsApplicableForDiscount(priceContext, discountPriceGroups, itemCatalogIds);
            }
    
            /// <summary>
            /// Get discount amount from deal price, taking into account existing deal price discounts.
            /// </summary>
            /// <param name="price">The price.</param>
            /// <param name="hasExistingDealPrice">A flag indicating whether or not it has existing deal price discounts.</param>
            /// <param name="bestExistingDealPrice">Best existing deal price.</param>
            /// <param name="dealPrice">Deal price.</param>
            /// <returns>The discount amount.</returns>
            protected static decimal GetDiscountAmountFromDealPrice(decimal price, bool hasExistingDealPrice, decimal bestExistingDealPrice, decimal dealPrice)
            {
                decimal discountAmount = decimal.Zero;
    
                if (hasExistingDealPrice)
                {
                    discountAmount = bestExistingDealPrice - dealPrice;
                }
                else
                {
                    discountAmount = price - dealPrice;
                }
    
                return discountAmount > decimal.Zero ? discountAmount : decimal.Zero;
            }
    
            /// <summary>
            /// Try getting the best existing deal price.
            /// </summary>
            /// <param name="discountDictionary">Discount dictionary of existing discount lines.</param>
            /// <param name="itemIndex">Item index.</param>
            /// <param name="bestExistingDealPrice">Bet existing deal price.</param>
            /// <returns>true if it has best exiting deal price; false otherwise.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design.")]
            protected static bool TryGetBestExistingDealPrice(
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary,
                int itemIndex,
                out decimal bestExistingDealPrice)
            {
                bool hasExistingDealPrice = false;
                bestExistingDealPrice = 0m;
                IList<DiscountLineQuantity> existingLines = null;
                if (discountDictionary != null && discountDictionary.TryGetValue(itemIndex, out existingLines))
                {
                    foreach (DiscountLineQuantity existingLine in existingLines)
                    {
                        if (existingLine.DiscountLine.DealPrice > 0m && (!hasExistingDealPrice || existingLine.DiscountLine.DealPrice < bestExistingDealPrice))
                        {
                            bestExistingDealPrice = existingLine.DiscountLine.DealPrice;
                            hasExistingDealPrice = true;
                        }
                    }
                }
    
                return hasExistingDealPrice;
            }
    
            /// <summary>
            /// Determines if item group index is overlapped with non-compounded discounts.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with external overlapping discounts.</param>
            /// <param name="itemsWithOverlappingDiscountsCompoundedOnly">Hast set of overlapped item group indices, compounded only.</param>
            /// <returns>true if item group index is overlapped with non-compounded discounts, otherwise false.</returns>
            protected bool IsItemIndexGroupOverlappedWithNonCompoundedDiscounts(
                int itemGroupIndex,
                HashSet<int> itemsWithOverlappingDiscounts,
                HashSet<int> itemsWithOverlappingDiscountsCompoundedOnly)
            {
                if (itemsWithOverlappingDiscounts == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscounts");
                }
    
                if (itemsWithOverlappingDiscountsCompoundedOnly == null)
                {
                    throw new ArgumentNullException("itemsWithOverlappingDiscountsCompoundedOnly");
                }
    
                bool isItemGroupIndexOverlappedWithNonCompoundedDiscount =
                    itemsWithOverlappingDiscounts.Contains(itemGroupIndex) &&
                    !(this.CanCompound && itemsWithOverlappingDiscountsCompoundedOnly.Contains(itemGroupIndex));
    
                return isItemGroupIndexOverlappedWithNonCompoundedDiscount;
            }
    
            /// <summary>
            /// Determines whether the item (by item group index) has overlapping discounts.
            /// </summary>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <param name="itemsWithOverlappingDiscounts">Items with overlapping discounts.</param>
            /// <returns>A value indicating whether the item has overlapping discounts.</returns>
            protected bool HasOverlap(int itemGroupIndex, HashSet<int> itemsWithOverlappingDiscounts)
            {
                bool hasOverlap = false;
    
                if (itemsWithOverlappingDiscounts != null && itemsWithOverlappingDiscounts.Contains(itemGroupIndex))
                {
                    // Overlap with other discounts.
                    hasOverlap = true;
                }
                else if (this.ItemGroupIndexToDiscountLineNumberSetMap[itemGroupIndex].Count > 1)
                {
                    // Overlap within the same discount.
                    hasOverlap = true;
                }
    
                return hasOverlap;
            }
    
            /// <summary>
            /// Try getting retail discount lines by variant Id or product identifier.
            /// </summary>
            /// <param name="productOrVariantId">Product or variant identifier.</param>
            /// <param name="masterProductId">Master product identifier.</param>
            /// <param name="unitOfMeasure">Unit of measure.</param>
            /// <param name="lines">Retail discount lines.</param>
            /// <returns>True if found.</returns>
            /// <remarks>The key of triggering product dictionary is one of variant Id, product identifier or master product identifier.</remarks>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3", Justification = "False positive for out parameters.")]
            protected bool TryGetRetailDiscountLines(long productOrVariantId, long masterProductId, string unitOfMeasure, out IList<RetailDiscountLine> lines)
            {
                IList<RetailDiscountLine> discountLines = null;
                IDictionary<long, IList<RetailDiscountLine>> productOrVariantIdToRetailDiscountLinesMap = this.GetProductOrVariantIdToRetailDiscountLinesMap();
    
                productOrVariantIdToRetailDiscountLinesMap.TryGetValue(productOrVariantId, out discountLines);
    
                if (productOrVariantId != masterProductId)
                {
                    IList<RetailDiscountLine> moreLines = null;
                    productOrVariantIdToRetailDiscountLinesMap.TryGetValue(masterProductId, out moreLines);
    
                    if (moreLines != null && moreLines.Count > 0)
                    {
                        if (discountLines == null || discountLines.Count == 0)
                        {
                            discountLines = moreLines;
                        }
                        else
                        {
                            foreach (RetailDiscountLine discountLine in moreLines)
                            {
                                if (!discountLines.Where(p => p.DiscountLineNumber == discountLine.DiscountLineNumber).Any())
                                {
                                    discountLines.Add(discountLine);
                                }
                            }
                        }
                    }
                }
    
                lines = new List<RetailDiscountLine>();
                if (discountLines != null)
                {
                    lines.AddRange(discountLines.Where(p => string.IsNullOrWhiteSpace(p.UnitOfMeasureSymbol) || string.Equals(p.UnitOfMeasureSymbol, unitOfMeasure, StringComparison.OrdinalIgnoreCase)));
                }
    
                return lines.Count > 0;
            }
    
            /// <summary>
            /// Determines if the discount line covers the item by item group index.
            /// </summary>
            /// <param name="discountLineNumber">Discount line number.</param>
            /// <param name="itemGroupIndex">Item group index.</param>
            /// <returns>True if discount line covers the item; false otherwise.</returns>
            protected bool IsDiscountLineCoveringItem(decimal discountLineNumber, int itemGroupIndex)
            {
                bool ret = false;
                HashSet<int> itemIndexGroupSet = null;
    
                if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLineNumber, out itemIndexGroupSet))
                {
                    ret = itemIndexGroupSet.Contains(itemGroupIndex);
                }
    
                return ret;
            }
    
            /// <summary>
            /// Initializes lookups, etc.
            /// </summary>
            protected virtual void InitializeLookups()
            {
                this.DiscountLineNumberToItemGroupIndexSetMap.Clear();
                this.ItemGroupIndexToDiscountLineNumberSetMap.Clear();
                this.IsFinished = false;
            }
    
            /// <summary>
            /// Gets the sort index to use for a discount application using the specified discount line.
            /// </summary>
            /// <param name="line">The discount line to determine the sort index on.</param>
            /// <returns>The sort index to use.</returns>
            protected virtual int GetSortIndexForRetailDiscountLine(RetailDiscountLine line)
            {
                if (line == null)
                {
                    return InvalidIndex;
                }
    
                // The discount offer type enum has the values in the proper order, discount method type does not.
                switch (this.DiscountType)
                {
                    case DiscountMethodType.LeastExpensive:
                        return (int)DiscountOfferMethod.DiscountPercent;
                    case DiscountMethodType.DealPrice:
                    case DiscountMethodType.MultiplyDealPrice:
                        return (int)DiscountOfferMethod.OfferPrice;
                    case DiscountMethodType.DiscountAmount:
                        return (int)DiscountOfferMethod.DiscountAmount;
                    case DiscountMethodType.DiscountPercent:
                    case DiscountMethodType.MultiplyDiscountPercent:
                        return (int)DiscountOfferMethod.DiscountPercent;
                    case DiscountMethodType.LineSpecific:
                        return line.DiscountMethod;
                    default:
                        return -1;
                }
            }
    
            /// <summary>
            /// Gets the (first) discount code from the transaction that triggered the discount.
            /// </summary>
            /// <param name="transaction">The transaction that the discount will be applied to.</param>
            /// <returns>The first matching discount code from the transaction that is contained in the collection of required discount codes for this discount.</returns>
            protected string GetDiscountCodeForDiscount(SalesTransaction transaction)
            {
                string discountCodeUsed = string.Empty;
    
                if (transaction != null && transaction.DiscountCodes != null && this.IsDiscountCodeRequired)
                {
                    foreach (var discountCode in transaction.DiscountCodes)
                    {
                        if (this.DiscountCodes.Contains(discountCode))
                        {
                            discountCodeUsed = discountCode;
                            break;
                        }
                    }
                }
    
                return discountCodeUsed;
            }

            /// <summary>
            /// Initializes the dictionary of previously applied discounts for calculating threshold discount amounts.
            /// </summary>
            /// <param name="appliedDiscounts">The previously applied discounts.</param>
            /// <param name="itemGroupIndexToDiscountLineQuantitiesLookup">The dictionary to hold discount line quantities applied to each relevant line.</param>
            /// <param name="countThreshold">A flag indicating whether to count threshold discounts.</param>
            /// <returns>True if the dictionary has been initialized, false otherwise.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Parameter is correct for this usage.")]
            protected bool InitializeDiscountDictionary(
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                Dictionary<int, IList<DiscountLineQuantity>> itemGroupIndexToDiscountLineQuantitiesLookup,
                bool countThreshold)
            {
                if (itemGroupIndexToDiscountLineQuantitiesLookup == null)
                {
                    return false;
                }

                var discountApplications = appliedDiscounts.Where(p => this.CanCompoundOnTopOf(p.DiscountApplication.Discount) && p.DiscountApplication.Discount.OfferId != this.OfferId).Reverse();

                foreach (var app in discountApplications)
                {
                    if (!countThreshold && app.DiscountApplication.Discount is ThresholdDiscount)
                    {
                        continue;
                    }

                    var applicationItemGroupIndexToDiscountLineQuantitiesLookup = app.ItemGroupIndexToDiscountLineQuantitiesLookup;

                    foreach (int itemGroupIndex in applicationItemGroupIndexToDiscountLineQuantitiesLookup.Keys)
                    {
                        IList<DiscountLineQuantity> discountLineQuantities;
                        if (!itemGroupIndexToDiscountLineQuantitiesLookup.TryGetValue(itemGroupIndex, out discountLineQuantities))
                        {
                            discountLineQuantities = new List<DiscountLineQuantity>();
                            itemGroupIndexToDiscountLineQuantitiesLookup.Add(itemGroupIndex, discountLineQuantities);
                        }

                        discountLineQuantities.AddRange(applicationItemGroupIndexToDiscountLineQuantitiesLookup[itemGroupIndex]);
                    }
                }

                return true;
            }

            /// <summary>
            /// Create a new discount line.
            /// </summary>
            /// <param name="discountCode">Discount code.</param>
            /// <param name="itemId">Item identifier.</param>
            /// <returns>A new discount line.</returns>
            protected DiscountLine NewDiscountLine(string discountCode, string itemId)
            {
                DiscountLine discountItem = new DiscountLine()
                {
                    DiscountLineType = DiscountLineType.PeriodicDiscount,
                    PeriodicDiscountType = this.PeriodicDiscountType,
                    OfferId = this.OfferId,
                    OfferName = this.OfferName,
                    DiscountCode = discountCode,
                    ConcurrencyMode = this.ConcurrencyMode,
                    PricingPriorityNumber = this.PricingPriorityNumber,
                    IsCompoundable = this.CanCompound,
                    DiscountApplicationGroup = itemId,
                };
    
                return discountItem;
            }

            /// <summary>
            /// Get existing discount dictionary and discounted prices, related to currency discount application.
            /// </summary>
            /// <param name="discountableItemGroups">Discountable item groups.</param>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            /// <param name="appliedDiscounts">Applied discounts.</param>
            /// <param name="discountApplication">Currency discount application.</param>
            /// <param name="includeAmountOff">A flag indicates whether to include amount off in discounted price.</param>
            /// <param name="includePercentageOff">A flag indicates whether to include percentage off in discounted price.</param>
            /// <param name="discountedPrices">Discounted prices.</param>
            /// <returns>Dictionary of existing discount lines, indexed by item index.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4", Justification = "False positive for out parameters.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design.")]
            protected Dictionary<int, IList<DiscountLineQuantity>> GetExistingDiscountDictionaryAndDiscountedPrices(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                DiscountApplication discountApplication,
                bool includeAmountOff,
                bool includePercentageOff,
                decimal[] discountedPrices)
            {
                if (discountApplication == null)
                {
                    throw new ArgumentNullException("discountApplication");
                }
    
                HashSet<int> itemGroupIndexSet = new HashSet<int>(discountApplication.RetailDiscountLines.Select(p => p.ItemIndex).Distinct());
    
                return this.GetExistingDiscountDictionaryAndDiscountedPrices(
                    discountableItemGroups,
                    remainingQuantities,
                    appliedDiscounts,
                    itemGroupIndexSet,
                    includeAmountOff,
                    includePercentageOff,
                    discountedPrices);
            }

            /// <summary>
            /// Get existing discount dictionary and discounted prices, related to currency discount application.
            /// </summary>
            /// <param name="discountableItemGroups">Discountable item groups.</param>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            /// <param name="appliedDiscounts">Applied discounts.</param>
            /// <param name="itemGroupIndexSet">Item group index set.</param>
            /// <param name="includeAmountOff">A flag indicates whether to include amount off in discounted price.</param>
            /// <param name="includePercentageOff">A flag indicates whether to include percentage off in discounted price.</param>
            /// <param name="discountedPrices">Discounted prices.</param>
            /// <returns>Dictionary of existing discount lines, indexed by item index.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4", Justification = "False positive for out parameters.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design.")]
            protected Dictionary<int, IList<DiscountLineQuantity>> GetExistingDiscountDictionaryAndDiscountedPrices(
                DiscountableItemGroup[] discountableItemGroups,
                decimal[] remainingQuantities,
                IEnumerable<AppliedDiscountApplication> appliedDiscounts,
                HashSet<int> itemGroupIndexSet,
                bool includeAmountOff,
                bool includePercentageOff,
                decimal[] discountedPrices)
            {
                if (discountableItemGroups == null)
                {
                    throw new ArgumentNullException("discountableItemGroups");
                }
    
                if (remainingQuantities == null)
                {
                    throw new ArgumentNullException("remainingQuantities");
                }
    
                if (itemGroupIndexSet == null)
                {
                    throw new ArgumentNullException("itemGroupIndexSet");
                }
    
                if (discountedPrices == null)
                {
                    throw new ArgumentNullException("discountedPrices");
                }
    
                Dictionary<int, IList<DiscountLineQuantity>> discountDictionary = new Dictionary<int, IList<DiscountLineQuantity>>();
    
                if (this.CanCompound)
                {
                    bool discountDictionaryInitialized = false;
                    foreach (int x in itemGroupIndexSet)
                    {
                        discountedPrices[x] = discountableItemGroups[x].Price;
    
                        if (this.CanCompound && this.DiscountType != DiscountMethodType.DiscountAmount)
                        {
                            if (!discountDictionaryInitialized)
                            {
                                discountDictionaryInitialized = this.InitializeDiscountDictionary(appliedDiscounts, discountDictionary, true);
                            }
    
                            // Determine the value of the previously applied concurrent discounts so that we can get the proper value here.
                            // We group them together here to get the correct amount when there is a quantity greater than one and a percentage discount.
                            if (discountDictionary.ContainsKey(x))
                            {
                                List<List<DiscountLineQuantity>> sortedConcurrentDiscounts =
                                    discountDictionary[x].GroupBy(p => p.DiscountLine.OfferId)
                                                         .OrderByDescending(p => p.First().DiscountLine.DealPrice)
                                                         .ThenByDescending(p => p.First().DiscountLine.Amount)
                                                         .Select(concurrentDiscountGroup => concurrentDiscountGroup.ToList())
                                                         .ToList();
    
                                foreach (List<DiscountLineQuantity> concurrentDiscount in sortedConcurrentDiscounts)
                                {
                                    decimal startingPrice = discountedPrices[x];
    
                                    foreach (DiscountLineQuantity discLine in concurrentDiscount)
                                    {
                                        if (remainingQuantities[x] != 0m &&
                                            (discLine.DiscountLine.DealPrice > decimal.Zero ||
                                             (discLine.DiscountLine.Amount > decimal.Zero && includeAmountOff) ||
                                             (discLine.DiscountLine.Percentage > decimal.Zero && includePercentageOff)))
                                        {
                                            discountedPrices[x] -= (((startingPrice * discLine.DiscountLine.Percentage / 100M) + discLine.DiscountLine.Amount) * discLine.Quantity) / Math.Abs(remainingQuantities[x]);
                                        }
                                    }
                                }
                            }
                        }
    
                        discountedPrices[x] = Math.Max(decimal.Zero, discountedPrices[x]);
                    }
                }
                else
                {
                    foreach (int x in itemGroupIndexSet)
                    {
                        discountedPrices[x] = discountableItemGroups[x].Price;
                    }
                }
    
                return discountDictionary;
            }
    
            /// <summary>
            /// Removes item group indexes with zero quantity from lookups.
            /// </summary>
            /// <param name="remainingQuantities">Remaining quantities.</param>
            protected void RemoveItemGroupIndexesWithZeroQuanttiyFromLookups(decimal[] remainingQuantities)
            {
                if (remainingQuantities != null)
                {
                    HashSet<int> itemGroupIndexSetToRemove = new HashSet<int>();
    
                    foreach (KeyValuePair<int, HashSet<decimal>> pair in this.ItemGroupIndexToDiscountLineNumberSetMap)
                    {
                        int itemGroupIndex = pair.Key;
                        if (remainingQuantities[itemGroupIndex] == decimal.Zero)
                        {
                            itemGroupIndexSetToRemove.Add(itemGroupIndex);
                        }
                    }
    
                    foreach (int itemGroupIndex in itemGroupIndexSetToRemove)
                    {
                        this.RemoveItemIndexGroupFromLookups(itemGroupIndex);
                    }
                }
            }
    
            /// <summary>
            /// Adds the specified product or variant identifier and discount line to the dictionary of lines available for the product or variant.
            /// </summary>
            /// <param name="productOrVariantId">The product or variant identifier.</param>
            /// <param name="line">The RetailDiscountLine object that the product or variant can apply to.</param>
            private void AddDiscountLineToDictionary(long productOrVariantId, RetailDiscountLine line)
            {
                if (this.ProductOfVariantToDiscountLinesMap.ContainsKey(productOrVariantId))
                {
                    this.ProductOfVariantToDiscountLinesMap[productOrVariantId].Add(line);
                }
                else
                {
                    this.ProductOfVariantToDiscountLinesMap.Add(productOrVariantId, new List<RetailDiscountLine>() { line });
                }
            }
    
            private void PopulateDiscountLineNumberAndItemGroupIndexLookups(
                IList<RetailDiscountLine> discountLines,
                DiscountableItemGroup item,
                int itemIndex)
            {
                if (item.Quantity > decimal.Zero)
                {
                    HashSet<decimal> discountLineNumberSet = null;
    
                    foreach (RetailDiscountLine discountLine in discountLines)
                    {
                        if (string.IsNullOrWhiteSpace(discountLine.UnitOfMeasureSymbol) ||
                            string.Equals(discountLine.UnitOfMeasureSymbol, item.SalesOrderUnitOfMeasure, StringComparison.OrdinalIgnoreCase))
                        {
                            if (discountLineNumberSet == null)
                            {
                                if (!this.ItemGroupIndexToDiscountLineNumberSetMap.TryGetValue(itemIndex, out discountLineNumberSet))
                                {
                                    discountLineNumberSet = new HashSet<decimal>();
                                    this.ItemGroupIndexToDiscountLineNumberSetMap[itemIndex] = discountLineNumberSet;
                                }
                            }
    
                            discountLineNumberSet.Add(discountLine.DiscountLineNumber);
    
                            HashSet<int> itemIndexSet = null;
                            if (this.DiscountLineNumberToItemGroupIndexSetMap.TryGetValue(discountLine.DiscountLineNumber, out itemIndexSet))
                            {
                                itemIndexSet.Add(itemIndex);
                            }
                            else
                            {
                                this.DiscountLineNumberToItemGroupIndexSetMap[discountLine.DiscountLineNumber] = new HashSet<int>() { itemIndex };
                            }
                        }
                    }
                }
            }
    
            private bool CanCompoundOnTopOf(DiscountBase theOther)
            {
                bool canCompound = true;
    
                if (theOther.ConcurrencyMode != ConcurrencyMode.Compounded)
                {
                    canCompound = false;
                }
    
                if (canCompound)
                {
                    bool isThisThreshold = this is ThresholdDiscount;
                    bool isTheOtherThreshold = theOther is ThresholdDiscount;
    
                    if (isThisThreshold == isTheOtherThreshold)
                    {
                        // We don't compound across priorities.
                        if (this.PricingPriorityNumber != theOther.PricingPriorityNumber)
                        {
                            canCompound = false;
                        }
                    }
                }
    
                return canCompound;
            }
        }
    }
}
