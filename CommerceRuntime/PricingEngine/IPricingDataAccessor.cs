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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Interface for accessing pricing data.
        /// </summary>
        /// <remarks>Both channel and AX have their own implementations. AX can't implement an interface with generics, so we have to resort to System.Object.</remarks>
        public interface IPricingDataAccessor
        {
            /// <summary>
            /// Gets the channel price configuration.
            /// </summary>
            /// <returns>Channel price configuration.</returns>
            ChannelPriceConfiguration GetChannelPriceConfiguration();
    
            /// <summary>
            /// Get the discount codes (aka 'promo codes') associated with the given discount offer identifiers.
            /// </summary>
            /// <param name="offerIds">The offer Ids whose discount codes are being found.</param>
            /// <returns>Discounts codes found for given offers of type ReadOnlyCollection&lt;DiscountCode&gt;.</returns>
            object GetDiscountCodesByOfferIds(object offerIds);
    
            /// <summary>
            /// Gets the channel price groups.
            /// </summary>
            /// <returns>
            /// A collection of channel price groups of type ReadOnlyCollection&lt;PriceGroup&gt;.
            /// </returns>
            object GetChannelPriceGroups();
    
            /// <summary>
            /// Retrieves PriceParameters from the database. This indicates which types of
            /// trade agreements are active for various combinations of customer and item types.
            /// </summary>
            /// <returns>The first (and only) row in PriceParameters in the database.</returns>
            PriceParameters GetPriceParameters();
    
            /// <summary>
            /// Gets the items using the specified item identifiers.
            /// </summary>
            /// <param name="itemIds">The collection of item identifiers.</param>
            /// <returns>The collection of items of type ReadOnlyCollection&lt;Item&gt;.</returns>
            object GetItems(object itemIds);
    
            /// <summary>
            /// Fetch the superset of discount trade agreements which could apply to all of these items and customer for the given dates.
            /// </summary>
            /// <param name="itemIds">The item Ids to fetch for agreements for.</param>
            /// <param name="customerAccount">Optional. Customer account number to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="currencyCode">Currency code to filter by.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of trade agreements which may be applied to the given items of type ReadOnlyCollection&lt;TradeAgreement&gt;.</returns>
            object ReadDiscountTradeAgreements(
                object itemIds,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                QueryResultSettings settings);
    
            /// <summary>
            /// Fetch the superset of trade agreements which could apply to all of these items and customer for the given date.
            /// </summary>
            /// <param name="itemIds">The item Ids to fetch for agreements for.</param>
            /// <param name="priceGroups">The price groups (probably channel) to query by.</param>
            /// <param name="customerAccount">Optional. Customer account number to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="currencyCode">Currency code to filter by.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of trade agreements which may be applied to the given items of type ReadOnlyCollection&lt;TradeAgreement&gt;.</returns>
            object ReadPriceTradeAgreements(
                object itemIds,
                object priceGroups,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                QueryResultSettings settings);
    
            /// <summary>
            /// Fetch all price adjustments for the given items, striped by item Id and dimension Id.
            /// </summary>
            /// <param name="items">The set of items to search by. Set of pairs of item Id and variant dimension Id. Ignores the unit.</param>
            /// <param name="priceGroups">Set of price groups to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of price adjustments striped by item Id and variant dimension Id (if any) of type ReadOnlyCollection&lt;PriceAdjustment&gt;.</returns>
            object ReadPriceAdjustments(
                object items,
                object priceGroups,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                QueryResultSettings settings);
    
            /// <summary>
            /// Fetch all retail discounts for the given items, striped by item Id and dimension Id.
            /// </summary>
            /// <param name="items">The set of items to search by. Set of pairs of item Id and variant dimension Id. Ignores the unit.</param>
            /// <param name="priceGroups">Set of price groups to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="currencyCode">Currency code to filter by.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of price adjustments striped by item Id and variant dimension Id (if any) of type ReadOnlyCollection&lt;PeriodicDiscount&gt;.</returns>
            object ReadRetailDiscounts(
                object items,
                object priceGroups,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                QueryResultSettings settings);
    
            /// <summary>
            /// Get the variant dimensions populated for the given dimension Ids. This is lightweight and
            ///  only returns the dimension Ids, not translations.
            /// </summary>
            /// <param name="inventoryDimensionIds">The dimension Ids which need dimension values fetched.</param>
            /// <returns>Collection of dimension values of type ReadOnlyCollection&lt;ProductVariant&gt;.</returns>
            object GetVariantDimensionsByInventDimensionIds(object inventoryDimensionIds);
    
            /// <summary>
            /// Get all the threshold tiers associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch tiers by.</param>
            /// <returns>Collection of tiers (if any) associated with the given offer IdsCollection of dimension values of type ReadOnlyCollection&lt;ThresholdDiscountTier&gt;.</returns>
            object GetThresholdTiersByOfferIds(object offerIds);
    
            /// <summary>
            /// Get all the multi buy discount lines associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch discount lines by.</param>
            /// <returns>Collection of multi buy discount lines associated with the given offer Ids of type ReadOnlyCollection&lt;QuantityDiscountLevel&gt;.</returns>
            object GetMultipleBuyDiscountLinesByOfferIds(object offerIds);
    
            /// <summary>
            /// Get all the mix and match line groups associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch mix and match line groups by.</param>
            /// <returns>Collection of mix and match line groups associated with the given offer Ids of type ReadOnlyCollection&lt;MixAndMatchLineGroup&gt;.</returns>
            object GetMixAndMatchLineGroupsByOfferIds(object offerIds);
    
            /// <summary>
            /// Gets the customer price group.
            /// </summary>
            /// <param name="customerPriceGroupId">Customer price group Id.</param>
            /// <returns>Customer price group.</returns>
            PriceGroup GetCustomerPriceGroup(string customerPriceGroupId);
    
            /// <summary>
            /// Gets the catalog price groups.
            /// </summary>
            /// <param name="catalogIds">Catalog recId's.</param>
            /// <returns>
            /// A collection of catalog price groups of type ReadOnlyCollection&lt;CatalogPriceGroup&gt;.
            /// </returns>
            object GetCatalogPriceGroups(object catalogIds);
    
            /// <summary>
            /// Gets the affiliation price groups.
            /// </summary>
            /// <param name="affiliationLoyaltyTiers">A collection of affiliation Id or loyalty tier Id.</param>
            /// <returns>
            /// A collection of affiliation price groups of type ReadOnlyCollection&lt;PriceGroup&gt;.
            /// </returns>
            object GetAffiliationPriceGroups(object affiliationLoyaltyTiers);
    
            /// <summary>
            /// Gets retail discount price groups.
            /// </summary>
            /// <param name="offerIds">Offer identifiers.</param>
            /// <returns>
            /// A collection of retail discount price groups of type ReadOnlyCollection&lt;RetailDiscountPriceGroup&gt;.
            /// </returns>
            object GetRetailDiscountPriceGroups(object offerIds);
    
            /// <summary>
            /// Get all of the discounts configured in the system.
            /// </summary>
            /// <returns>The collection of discounts of type ReadOnlyCollection&lt;RetailDiscount&gt;.</returns>
            object GetAllRetailDiscounts();
    
            /// <summary>
            /// Get the category membership information for the product or variant identifiers passed in.
            /// </summary>
            /// <param name="productOrVariantIds">A set of product or variant identifiers.</param>
            /// <returns>The collection of mappings between the product or variant identifier and the category identifier of type ReadOnlyCollection&lt;RetailCategoryMember&gt;.</returns>
            object GetRetailCategoryMembersForItems(object productOrVariantIds);
    
            /// <summary>
            /// Gets the variants for the specified collection of item variant inventory dimension identifiers.
            /// </summary>
            /// <param name="itemVariants">The collection of item variant inventory dimension.</param>
            /// <returns>The variant with specified columns populated. Null if variant not found of type ReadOnlyCollection&lt;ProductVariant&gt;.</returns>
            object GetVariants(object itemVariants);
        }
    }
}
