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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Price context builder.
        /// </summary>
        public static class PriceContextHelper
        {
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class.
            /// </summary>
            /// <param name="requestContext">Request context.</param>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="priceParameters">Price parameters.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="itemIds">Item Ids.</param>
            /// <param name="catalogIds">Catalog identifiers.</param>
            /// <param name="activeDate">Active date.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                RequestContext requestContext,
                IPricingDataAccessor pricingDataManager,
                PriceParameters priceParameters,
                ICurrencyOperations currencyAndRoundingHelper,
                ISet<string> itemIds,
                ISet<long> catalogIds,
                DateTimeOffset activeDate,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode)
            {
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }
    
                PriceContext priceContext = new PriceContext
                {
                    CurrencyAndRoundingHelper = currencyAndRoundingHelper,
                    ActiveDate = activeDate,
                    PriceParameters = priceParameters,
                    PriceCalculationMode = priceCalculationMode,
                    DiscountCalculationMode = discountCalculationMode,
                };
    
                PriceContextHelper.InitializePriceContextOfInferredProperties(priceContext, pricingDataManager, requestContext, itemIds, catalogIds, null);
    
                return priceContext;
            }
    
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class.
            /// </summary>
            /// <param name="requestContext">Request context.</param>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="itemIds">Item Ids.</param>
            /// <param name="catalogIds">Catalog identifiers.</param>
            /// <param name="affiliationLoyaltyTiers">Affiliation loyalty tiers.</param>
            /// <param name="activeDate">Active date.</param>
            /// <param name="customerId">Customer Id.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                RequestContext requestContext,
                IPricingDataAccessor pricingDataManager,
                ICurrencyOperations currencyAndRoundingHelper,
                ISet<string> itemIds,
                ISet<long> catalogIds,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers,
                DateTimeOffset activeDate,
                string customerId,
                string customerPriceGroup,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode)
            {
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }
    
                if (affiliationLoyaltyTiers == null)
                {
                    throw new ArgumentNullException("affiliationLoyaltyTiers");
                }
    
                PriceContext priceContext = new PriceContext
                {
                    CurrencyAndRoundingHelper = currencyAndRoundingHelper,
                    ActiveDate = activeDate,
                    CustomerAccount = customerId,
                    CustomerPriceGroup = customerPriceGroup,
                    PriceCalculationMode = priceCalculationMode,
                    DiscountCalculationMode = discountCalculationMode,
                };
    
                PriceContextHelper.InitializePriceContextOfInferredProperties(priceContext, pricingDataManager, requestContext, itemIds, catalogIds, affiliationLoyaltyTiers);
    
                return priceContext;
            }
    
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class.
            /// </summary>
            /// <param name="requestContext">Request context.</param>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="priceParameters">Price parameters.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="activeDate">Active date.</param>
            /// <param name="customerId">Customer Id.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="priceIncludesTax">Price includes tax.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                RequestContext requestContext,
                IPricingDataAccessor pricingDataManager,
                SalesTransaction transaction,
                PriceParameters priceParameters,
                ICurrencyOperations currencyAndRoundingHelper,
                DateTimeOffset activeDate,
                string customerId,
                string customerPriceGroup,
                bool priceIncludesTax,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode)
            {
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }
    
                PriceContext priceContext = new PriceContext
                {
                    CurrencyAndRoundingHelper = currencyAndRoundingHelper,
                    ActiveDate = activeDate,
                    CustomerAccount = customerId,
                    CustomerPriceGroup = customerPriceGroup,
                    IsTaxInclusive = priceIncludesTax,
                    PriceParameters = priceParameters,
                    PriceCalculationMode = priceCalculationMode,
                    DiscountCalculationMode = discountCalculationMode,
                };
    
                ISet<long> catalogIds = GetCatalogIds(transaction);
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers = GetAffiliationLoyalTierIds(transaction);
                ISet<string> itemIds = GetItemIds(transaction);
    
                PriceContextHelper.InitializePriceContextOfInferredProperties(priceContext, pricingDataManager, requestContext, itemIds, catalogIds, affiliationLoyaltyTiers);
    
                return priceContext;
            }
    
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class.
            /// </summary>
            /// <param name="requestContext">Request context.</param>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="transaction">Current transaction.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="activeDate">Active date.</param>
            /// <param name="customerId">Customer Id.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="customerLinePriceGroup">Customer line discount price group.</param>
            /// <param name="customerMultipleLinePriceGroup">Customer multiple line discount price group.</param>
            /// <param name="customerTotalPriceGroup">Customer total discount price group.</param>
            /// <param name="priceIncludesTax">Price includes tax.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <param name="calculateForNewSalesLinesOnly">A flag indicating whether to calculate for new sales lines only.</param>
            /// <param name="newSalesLineIdSet">New sales line id set.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                RequestContext requestContext,
                IPricingDataAccessor pricingDataManager,
                SalesTransaction transaction,
                ICurrencyOperations currencyAndRoundingHelper,
                DateTimeOffset activeDate,
                string customerId,
                string customerPriceGroup,
                string customerLinePriceGroup,
                string customerMultipleLinePriceGroup,
                string customerTotalPriceGroup,
                bool priceIncludesTax,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode,
                bool calculateForNewSalesLinesOnly,
                HashSet<string> newSalesLineIdSet)
            {
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }
    
                PriceContext priceContext = new PriceContext
                {
                    CurrencyAndRoundingHelper = currencyAndRoundingHelper,
                    ActiveDate = activeDate,
                    CustomerAccount = customerId,
                    CustomerPriceGroup = customerPriceGroup,
                    CustomerLinePriceGroup = customerLinePriceGroup,
                    CustomerMultipleLinePriceGroup = customerMultipleLinePriceGroup,
                    CustomerTotalPriceGroup = customerTotalPriceGroup,
                    IsTaxInclusive = priceIncludesTax,
                    PriceCalculationMode = priceCalculationMode,
                    DiscountCalculationMode = discountCalculationMode,
                    CalculateForNewSalesLinesOnly = calculateForNewSalesLinesOnly,
                };
    
                if (newSalesLineIdSet != null && newSalesLineIdSet.Count > 0)
                {
                    priceContext.NewSalesLineIdSet.AddRange(newSalesLineIdSet);
                }
    
                ISet<long> catalogIds = GetCatalogIds(transaction);
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers = GetAffiliationLoyalTierIds(transaction);
                ISet<string> itemIds = GetItemIds(transaction);
    
                PriceContextHelper.InitializePriceContextOfInferredProperties(priceContext, pricingDataManager, requestContext, itemIds, catalogIds, affiliationLoyaltyTiers);
    
                return priceContext;
            }
    
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class for price calculation.
            /// </summary>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <param name="itemIds">Item Ids.</param>
            /// <param name="catalogIds">Catalog identifiers.</param>
            /// <param name="affiliationLoyaltyTiers">Affiliation or loyalty tier identifiers.</param>
            /// <param name="customerId">Customer Id.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="priceIncludesTax">Price includes tax.</param>
            /// <param name="currencyCode">Currency code.</param>
            /// <param name="activeDate">Active date.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                IPricingDataAccessor pricingDataManager,
                ICurrencyOperations currencyAndRoundingHelper,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode,
                ISet<string> itemIds,
                ISet<long> catalogIds,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers,
                string customerId,
                string customerPriceGroup,
                bool priceIncludesTax,
                string currencyCode,
                DateTimeOffset activeDate)
            {
                PriceContext priceContext = CreatePriceContext(
                    pricingDataManager,
                    currencyAndRoundingHelper,
                    priceCalculationMode,
                    discountCalculationMode,
                    itemIds,
                    catalogIds,
                    affiliationLoyaltyTiers,
                    customerId,
                    customerPriceGroup,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    priceIncludesTax,
                    currencyCode,
                    activeDate);
    
                return priceContext;
            }
    
            /// <summary>
            /// Create a new instance of the <see cref="PriceContext"/> class for price or discount calculation.
            /// </summary>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="priceCalculationMode">Price calculation mode.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            /// <param name="itemIds">Item Ids.</param>
            /// <param name="catalogIds">Catalog identifiers.</param>
            /// <param name="affiliationLoyaltyTiers">Affiliation or loyalty tier identifiers.</param>
            /// <param name="customerId">Customer Id.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="customerLinePriceGroup">Customer line discount price group.</param>
            /// <param name="customerMultipleLinePriceGroup">Customer multiple line discount price group.</param>
            /// <param name="customerTotalPriceGroup">Customer total discount price group.</param>
            /// <param name="priceIncludesTax">Price includes tax.</param>
            /// <param name="currencyCode">Currency code.</param>
            /// <param name="activeDate">Active date.</param>
            /// <returns>A new instance of the <see cref="PriceContext"/> class.</returns>
            public static PriceContext CreatePriceContext(
                IPricingDataAccessor pricingDataManager,
                ICurrencyOperations currencyAndRoundingHelper,
                PricingCalculationMode priceCalculationMode,
                DiscountCalculationMode discountCalculationMode,
                ISet<string> itemIds,
                ISet<long> catalogIds,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers,
                string customerId,
                string customerPriceGroup,
                string customerLinePriceGroup,
                string customerMultipleLinePriceGroup,
                string customerTotalPriceGroup,
                bool priceIncludesTax,
                string currencyCode,
                DateTimeOffset activeDate)
            {
                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }
    
                PriceContext priceContext = new PriceContext
                {
                    CurrencyAndRoundingHelper = currencyAndRoundingHelper,
                    ActiveDate = activeDate,
                    CurrencyCode = currencyCode,
                    CustomerAccount = customerId,
                    CustomerPriceGroup = customerPriceGroup,
                    CustomerLinePriceGroup = customerLinePriceGroup,
                    CustomerMultipleLinePriceGroup = customerMultipleLinePriceGroup,
                    CustomerTotalPriceGroup = customerTotalPriceGroup,
                    PriceParameters = pricingDataManager.GetPriceParameters(),
                    IsTaxInclusive = priceIncludesTax,
                    PriceCalculationMode = priceCalculationMode,
                    DiscountCalculationMode = discountCalculationMode,
                };

                PriceContextHelper.InitializePriceContexOfAlgorithmMode(priceContext, pricingDataManager);
                PriceContextHelper.InitializePriceContexOfPriceGroups(priceContext, pricingDataManager, catalogIds, affiliationLoyaltyTiers);
                PriceContextHelper.InitializeItemCache(priceContext, pricingDataManager, itemIds);
    
                return priceContext;
            }
    
            /// <summary>
            /// Get all price groups from price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <returns>All price groups.</returns>
            /// <remarks>We could have made it an C# extension. Leave it here for all price context logic.</remarks>
            public static HashSet<string> GetAllPriceGroupsForDiscount(PriceContext priceContext)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return GetAllPriceGroups(priceContext, GetAllPriceGroupsExceptCatalogsForDiscount(priceContext));
            }
    
            /// <summary>
            /// Get item by item identifier.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="itemId">Item identifier.</param>
            /// <returns>The item.</returns>
            public static Item GetItem(PriceContext priceContext, string itemId)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                Item item = null;
    
                priceContext.ItemCache.TryGetValue(itemId, out item);
    
                return item;
            }
    
            /// <summary>
            /// Check whether the discount is allowed for the item.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="itemId">Item identifier.</param>
            /// <returns>True if  the discount is allowed for the item, otherwise false.</returns>
            public static bool IsDiscountAllowed(PriceContext priceContext, string itemId)
            {
                Item item = PriceContextHelper.GetItem(priceContext, itemId);
                bool isDiscountAllowed = item != null ? !item.NoDiscountAllowed : true;
    
                return isDiscountAllowed;
            }
    
            /// <summary>
            /// Get all price group identifiers from price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <returns>All price groups.</returns>
            public static IEnumerable<long> GetAllPriceGroupIdsForDiscount(PriceContext priceContext)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                HashSet<string> groups = GetAllPriceGroupsForDiscount(priceContext);
    
                return priceContext.PriceGroupIdsToRecordIdsDictionary.Where(p => groups.Contains(p.Key)).Select(p => p.Value);
            }
    
            /// <summary>
            /// Get all price groups for price from price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <returns>All price groups for price.</returns>
            /// <remarks>We could have made it an C# extension. Leave it here for all price context logic.</remarks>
            public static ISet<string> GetAllPriceGroupsForPrice(PriceContext priceContext)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return GetAllPriceGroups(priceContext, GetAllPriceGroupsExceptCatalogsForPrice(priceContext));
            }
    
            /// <summary>
            /// Get all applicable discount price groups from price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="itemCatalogIds">Item catalog identifiers.</param>
            /// <returns>All applicable discount price groups.</returns>
            /// <remarks>We could have made it an C# extension. Leave it here for all price context logic.</remarks>
            public static ISet<string> GetApplicablePriceGroupsForDiscount(PriceContext priceContext, ISet<long> itemCatalogIds)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return GetApplicablePriceGroups(priceContext, GetAllPriceGroupsExceptCatalogsForDiscount(priceContext), itemCatalogIds);
            }
    
            /// <summary>
            /// Get all applicable price price groups from price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="itemCatalogIds">Item catalog identifiers.</param>
            /// <returns>All applicable price price groups.</returns>
            /// <remarks>We could have made it an C# extension. Leave it here for all price context logic.</remarks>
            public static ISet<string> GetApplicablePriceGroupsForPrice(PriceContext priceContext, ISet<long> itemCatalogIds)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return GetApplicablePriceGroups(priceContext, GetAllPriceGroupsExceptCatalogsForPrice(priceContext), itemCatalogIds);
            }
    
            /// <summary>
            /// Check whether it is applicable for retail discount.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="discountPriceGroups">Discount price groups.</param>
            /// <param name="itemCatalogIds">Item catalog identifiers.</param>
            /// <returns>True if it is applicable.</returns>
            public static bool IsApplicableForDiscount(PriceContext priceContext, ISet<string> discountPriceGroups, ISet<long> itemCatalogIds)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return IsApplicable(discountPriceGroups, GetApplicablePriceGroupsForDiscount(priceContext, itemCatalogIds));
            }
    
            /// <summary>
            /// Check whether it is applicable for price.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="pricePriceGroups">Price price groups.</param>
            /// <param name="itemCatalogIds">Item catalog identifiers.</param>
            /// <returns>True if it is applicable.</returns>
            public static bool IsApplicableForPrice(PriceContext priceContext, ISet<string> pricePriceGroups, ISet<long> itemCatalogIds)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                return IsApplicable(pricePriceGroups, GetApplicablePriceGroupsForPrice(priceContext, itemCatalogIds));
            }
    
            /// <summary>
            /// Get catalog identifiers.
            /// </summary>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Set of catalog Ids.</returns>
            public static ISet<long> GetCatalogIds(SalesTransaction transaction)
            {
                ISet<long> catalogIds = transaction != null ? GetCatalogIds(transaction.PriceCalculableSalesLines) : new HashSet<long>();
    
                return catalogIds;
            }
    
            /// <summary>
            /// Get catalog identifiers.
            /// </summary>
            /// <param name="salesLines">List of sales lines.</param>
            /// <returns>Set of catalog Ids.</returns>
            public static ISet<long> GetCatalogIds(IEnumerable<SalesLine> salesLines)
            {
                HashSet<long> catalogIds = new HashSet<long>();
    
                if (salesLines != null)
                {
                    foreach (SalesLine salesLine in salesLines)
                    {
                        if (!salesLine.CatalogIds.IsNullOrEmpty())
                        {
                            catalogIds.UnionWith(salesLine.CatalogIds);
                        }
                    }
                }
    
                return catalogIds;
            }
    
            /// <summary>
            /// Get item identifiers.
            /// </summary>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>Set of item Ids.</returns>
            public static ISet<string> GetItemIds(SalesTransaction transaction)
            {
                return transaction != null ? GetItemIds(transaction.PriceCalculableSalesLines) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
    
            /// <summary>
            /// Get item identifiers.
            /// </summary>
            /// <param name="salesLines">List of sales lines.</param>
            /// <returns>Set of item Ids.</returns>
            public static ISet<string> GetItemIds(IEnumerable<SalesLine> salesLines)
            {
                HashSet<string> itemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    
                if (salesLines != null)
                {
                    foreach (SalesLine salesLine in salesLines)
                    {
                        if (!string.IsNullOrWhiteSpace(salesLine.ItemId))
                        {
                            itemIds.Add(salesLine.ItemId);
                        }
                    }
                }
    
                return itemIds;
            }
    
            /// <summary>
            /// Get list of affiliation or loyalty tier identifiers.
            /// </summary>
            /// <param name="transaction">Current transaction.</param>
            /// <returns>List of affiliation or loyalty tier identifiers.</returns>
            public static IEnumerable<AffiliationLoyaltyTier> GetAffiliationLoyalTierIds(SalesTransaction transaction)
            {
                List<AffiliationLoyaltyTier> affiliationLoyaltyTiers = new List<AffiliationLoyaltyTier>();
    
                if (transaction != null)
                {
                    IList<SalesAffiliationLoyaltyTier> salesAffiliationLoyaltyTiers = transaction.AffiliationLoyaltyTierLines;
                    if (salesAffiliationLoyaltyTiers != null)
                    {
                        foreach (SalesAffiliationLoyaltyTier salesAffiliationLoyaltyTier in salesAffiliationLoyaltyTiers)
                        {
                            AffiliationLoyaltyTier oneAffiliationLoyaltyTier = new AffiliationLoyaltyTier()
                            {
                                AffiliationId = salesAffiliationLoyaltyTier.AffiliationId,
                                LoyaltyTierId = salesAffiliationLoyaltyTier.LoyaltyTierId
                            };
    
                            affiliationLoyaltyTiers.Add(oneAffiliationLoyaltyTier);
                        }
                    }
                }
    
                return affiliationLoyaltyTiers;
            }
    
            /// <summary>
            /// Get offer identifier to retail discount price groups dictionary, filter by transaction price groups.
            /// </summary>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="offerIds">Offer identifiers.</param>
            /// <param name="transactionPriceGroups">Price groups from transaction.</param>
            /// <returns>A dictionary of offer identifier to retail discount price groups map filtered by price groups from transaction.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "A simple map from offerId to price groups.")]
            public static IDictionary<string, IList<PriceGroup>> GetRetailDiscountPriceGroupDictionaryFilteredByTransaction(
                IPricingDataAccessor pricingDataManager,
                ISet<string> offerIds,
                ISet<string> transactionPriceGroups)
            {
                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }
    
                Dictionary<string, IList<PriceGroup>> ret = new Dictionary<string, IList<PriceGroup>>(StringComparer.OrdinalIgnoreCase);
    
                if (offerIds != null && offerIds.Count > 0 && transactionPriceGroups != null && transactionPriceGroups.Any())
                {
                    IEnumerable<RetailDiscountPriceGroup> discountPriceGroups = pricingDataManager.GetRetailDiscountPriceGroups(offerIds) as IEnumerable<RetailDiscountPriceGroup>;
    
                    if (discountPriceGroups != null)
                    {
                        foreach (RetailDiscountPriceGroup discountPriceGroup in discountPriceGroups)
                        {
                            if (transactionPriceGroups.Contains(discountPriceGroup.GroupId))
                            {
                                IList<PriceGroup> priceGroups = null;
    
                                if (!ret.TryGetValue(discountPriceGroup.OfferId, out priceGroups))
                                {
                                    priceGroups = new List<PriceGroup>();
                                    ret.Add(discountPriceGroup.OfferId, priceGroups);
                                }
    
                                priceGroups.Add(discountPriceGroup);
                            }
                        }
                    }
                }
    
                return ret;
            }
    
            /// <summary>
            /// Check whether discount type matches the discount calculation mode in price context.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="discountType">Discount type.</param>
            /// <returns>True if discount type matches the discount calculation mode.</returns>
            public static bool MatchCalculationMode(PriceContext priceContext, PeriodicDiscountOfferType discountType)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                bool match = false;
                DiscountCalculationMode filterFlag = DiscountCalculationMode.None;
    
                switch (discountType)
                {
                    case PeriodicDiscountOfferType.Offer:
                        filterFlag = DiscountCalculationMode.CalculateOffer;
                        break;
                    case PeriodicDiscountOfferType.MultipleBuy:
                        filterFlag = DiscountCalculationMode.CalculateMultipleBuy;
                        break;
                    case PeriodicDiscountOfferType.MixAndMatch:
                        filterFlag = DiscountCalculationMode.CalculateMixAndMatch;
                        break;
                    case PeriodicDiscountOfferType.Threshold:
                        filterFlag = DiscountCalculationMode.CalculateThreshold;
                        break;
                    default:
                        break;
                }
    
                if (filterFlag != DiscountCalculationMode.None)
                {
                    match = (priceContext.DiscountCalculationMode & filterFlag) != DiscountCalculationMode.None;
                }
    
                return match;
            }
    
            /// <summary>
            /// Add channel price groups.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="priceGroups">Price groups.</param>
            /// <remarks>This is private. Exposed as internal for test.</remarks>
            internal static void AddChannelPriceGroups(PriceContext priceContext, IEnumerable<PriceGroup> priceGroups)
            {
                AddPriceGroupsToCollections(
                    priceGroups,
                    priceContext.ChannelPriceGroups,
                    priceContext.PriceGroupIdsToRecordIdsDictionary,
                    priceContext.RecordIdsToPriceGroupIdsDictionary,
                    priceContext.PriceGroupIdToPriorityDictionary);
            }
    
            /// <summary>
            /// Get smallest non negative amount.
            /// </summary>
            /// <param name="priceContext">Price context.</param>
            /// <param name="amount">Reference amount.</param>
            /// <returns>Smallest non negative amount.</returns>
            /// <remarks>We could have rounding service return it, but we also need to fix AX which doesn't use rounding service.</remarks>
            internal static decimal GetSmallestNonNegativeAmount(PriceContext priceContext, decimal amount)
            {
                decimal smallestAmount = priceContext.CurrencyAndRoundingHelper.Round(Math.Abs(amount));
                bool stop = false;
    
                while (!stop && smallestAmount > decimal.Zero)
                {
                    decimal half = priceContext.CurrencyAndRoundingHelper.Round(smallestAmount / 2m);
                    stop = half == decimal.Zero || half == smallestAmount;
    
                    if (!stop)
                    {
                        smallestAmount = half;
                    }
                }
    
                return smallestAmount;
            }

            private static void InitializePriceContexOfAlgorithmMode(PriceContext priceContext, IPricingDataAccessor pricingDataManager)
            {
                ChannelPriceConfiguration channelPriceConfig = pricingDataManager.GetChannelPriceConfiguration();
                priceContext.DiscountAlgorithmMode = channelPriceConfig.DiscountAlgorithmMode;
                priceContext.MaxBestDealAlgorithmStepCount = channelPriceConfig.MaxBestDealStepCount;
            }

            private static void InitializePriceContexOfPriceGroups(PriceContext priceContext, IPricingDataAccessor pricingDataManager, ISet<long> catalogIds, IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }
    
                IEnumerable<PriceGroup> channelPriceGroups = pricingDataManager.GetChannelPriceGroups() as IEnumerable<PriceGroup>;
                AddPriceGroupsToCollections(
                    channelPriceGroups,
                    priceContext.ChannelPriceGroups,
                    priceContext.PriceGroupIdsToRecordIdsDictionary,
                    priceContext.RecordIdsToPriceGroupIdsDictionary,
                    priceContext.PriceGroupIdToPriorityDictionary);
                IEnumerable<CatalogPriceGroup> catalogPriceGroups = catalogIds != null && catalogIds.Count > 0 ? pricingDataManager.GetCatalogPriceGroups(catalogIds) as IEnumerable<CatalogPriceGroup> : new List<CatalogPriceGroup>().AsReadOnly();
                IEnumerable<PriceGroup> affiliationPriceGroups = affiliationLoyaltyTiers != null && affiliationLoyaltyTiers.Any() ? pricingDataManager.GetAffiliationPriceGroups(affiliationLoyaltyTiers) as IEnumerable<PriceGroup> : new List<PriceGroup>().AsReadOnly();
    
                foreach (CatalogPriceGroup catalogPriceGroup in catalogPriceGroups)
                {
                    if (!priceContext.CatalogPriceGroups.ContainsKey(catalogPriceGroup.CatalogId))
                    {
                        priceContext.CatalogPriceGroups.Add(catalogPriceGroup.CatalogId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    }
    
                    priceContext.CatalogPriceGroups[catalogPriceGroup.CatalogId].Add(catalogPriceGroup.GroupId);
                }
    
                priceContext.PriceGroupIdsToRecordIdsDictionary.Merge(catalogPriceGroups.Select(p => new KeyValuePair<string, long>(p.GroupId, p.PriceGroupId)));
                priceContext.RecordIdsToPriceGroupIdsDictionary.Merge(catalogPriceGroups.Select(p => new KeyValuePair<long, string>(p.PriceGroupId, p.GroupId)));
                priceContext.PriceGroupIdToPriorityDictionary.Merge(catalogPriceGroups.Select(p => new KeyValuePair<string, int>(p.GroupId, p.PricingPriorityNumber)));
    
                AddPriceGroupsToCollections(
                    affiliationPriceGroups,
                    priceContext.AffiliationPriceGroups,
                    priceContext.PriceGroupIdsToRecordIdsDictionary,
                    priceContext.RecordIdsToPriceGroupIdsDictionary,
                    priceContext.PriceGroupIdToPriorityDictionary);
    
                FixCustomerPriceGroupPriority(pricingDataManager, priceContext);
            }
    
            private static void AddPriceGroupsToCollections(
                IEnumerable<PriceGroup> priceGroups,
                ISet<string> priceGroupSet,
                IDictionary<string, long> priceGroupIdsToRecordIdsDictionary,
                IDictionary<long, string> recordIdsToPriceGroupIdsDictionary,
                IDictionary<string, int> priceGroupIdToPriorityDictionary)
            {
                priceGroupSet.AddRange(priceGroups.Select(p => p.GroupId));
                priceGroupIdsToRecordIdsDictionary.Merge(priceGroups.Select(p => new KeyValuePair<string, long>(p.GroupId, p.PriceGroupId)));
                recordIdsToPriceGroupIdsDictionary.Merge(priceGroups.Select(p => new KeyValuePair<long, string>(p.PriceGroupId, p.GroupId)));
                priceGroupIdToPriorityDictionary.Merge(priceGroups.Select(p => new KeyValuePair<string, int>(p.GroupId, p.PricingPriorityNumber)));
            }
    
            private static void FixCustomerPriceGroupPriority(IPricingDataAccessor pricingDataManager, PriceContext priceContext)
            {
                if (!string.IsNullOrEmpty(priceContext.CustomerPriceGroup))
                {
                    // Customer price group may have already been covered by price groups from channel, affiliation, loyalty or catalog.
                    if (!priceContext.PriceGroupIdToPriorityDictionary.ContainsKey(priceContext.CustomerPriceGroup))
                    {
                        PriceGroup customerPriceGroup = pricingDataManager.GetCustomerPriceGroup(priceContext.CustomerPriceGroup);
    
                        if (customerPriceGroup != null && customerPriceGroup.PriceGroupId > 0)
                        {
                            priceContext.PriceGroupIdToPriorityDictionary.Add(priceContext.CustomerPriceGroup, customerPriceGroup.PricingPriorityNumber);
                        }
                    }
                }
            }
    
            private static HashSet<string> GetAllPriceGroupsExceptCatalogsForDiscount(PriceContext priceContext)
            {
                HashSet<string> allPriceGroupsExceptCatalogs = new HashSet<string>(priceContext.ChannelPriceGroups, StringComparer.OrdinalIgnoreCase);
                allPriceGroupsExceptCatalogs.UnionWith(priceContext.AffiliationPriceGroups);
    
                return allPriceGroupsExceptCatalogs;
            }
    
            private static HashSet<string> GetAllPriceGroupsExceptCatalogsForPrice(PriceContext priceContext)
            {
                HashSet<string> allPriceGroupsExceptCatalogsForPrice = GetAllPriceGroupsExceptCatalogsForDiscount(priceContext);
                if (!string.IsNullOrWhiteSpace(priceContext.CustomerPriceGroup))
                {
                    allPriceGroupsExceptCatalogsForPrice.Add(priceContext.CustomerPriceGroup);
                }
    
                return allPriceGroupsExceptCatalogsForPrice;
            }
    
            private static HashSet<string> GetAllPriceGroups(PriceContext priceContext, HashSet<string> allPriceGroupsExceptCatalogs)
            {
                HashSet<string> allPriceGroups = new HashSet<string>(allPriceGroupsExceptCatalogs);
    
                foreach (KeyValuePair<long, ISet<string>> priceGroups in priceContext.CatalogPriceGroups)
                {
                    allPriceGroups.UnionWith(priceGroups.Value);
                }
    
                return allPriceGroups;
            }
    
            private static bool IsApplicable(ISet<string> discountPriceGroups, ISet<string> applicablePriceGroups)
            {
                bool isApplicable = false;
    
                if (discountPriceGroups != null && discountPriceGroups.Count > 0 && applicablePriceGroups != null && applicablePriceGroups.Count > 0)
                {
                    applicablePriceGroups.IntersectWith(discountPriceGroups);
    
                    isApplicable = applicablePriceGroups.Count > 0;
                }
    
                return isApplicable;
            }
    
            private static ISet<string> GetApplicablePriceGroups(PriceContext priceContext, ISet<string> allPriceGroupsExceptCatalogs, ISet<long> itemCatalogIds)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                HashSet<string> applicablePriceGroups = new HashSet<string>(allPriceGroupsExceptCatalogs);
    
                if (itemCatalogIds != null)
                {
                    foreach (var itemCatalogId in itemCatalogIds)
                    {
                        ISet<string> catalogPriceGroups = null;
                        if (priceContext.CatalogPriceGroups.TryGetValue(itemCatalogId, out catalogPriceGroups))
                        {
                            if (catalogPriceGroups != null)
                            {
                                applicablePriceGroups.UnionWith(catalogPriceGroups);
                            }
                        }
                    }
                }
    
                return applicablePriceGroups;
            }
    
            private static void InitializePriceContextOfInferredProperties(
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager,
                RequestContext requestContext,
                ISet<string> itemIds,
                ISet<long> catalogIds,
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers)
            {
                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }
    
                if (requestContext == null)
                {
                    throw new ArgumentNullException("requestContext");
                }

                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }

                if (string.IsNullOrWhiteSpace(priceContext.CurrencyCode))
                {
                    string currencyCode = requestContext.GetChannelConfiguration().Currency;
                    priceContext.CurrencyCode = currencyCode;
                }
    
                if (priceContext.PriceParameters == null)
                {
                    priceContext.PriceParameters = pricingDataManager.GetPriceParameters();
                }

                InitializePriceContexOfAlgorithmMode(priceContext, pricingDataManager);
                InitializePriceContexOfPriceGroups(priceContext, pricingDataManager, catalogIds, affiliationLoyaltyTiers);
                InitializeItemCache(priceContext, pricingDataManager, itemIds);
            }
    
            private static void InitializeItemCache(
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager,
                ISet<string> itemIds)
            {
                if (itemIds != null && itemIds.Count > 0)
                {
                    ReadOnlyCollection<Item> items = pricingDataManager.GetItems(itemIds) as ReadOnlyCollection<Item>;
                    foreach (Item item in items)
                    {
                        priceContext.ItemCache.Add(item.ItemId, item);
                    }
                }
            }
        }
    }
}
