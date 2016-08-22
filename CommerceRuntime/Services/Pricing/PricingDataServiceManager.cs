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
        using Commerce.Runtime.Services.PricingEngine;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Channel pricing data accessor.
        /// </summary>
        public class PricingDataServiceManager : IPricingDataAccessor
        {
            private RequestContext requestContext;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="PricingDataServiceManager" /> class.
            /// </summary>
            /// <param name="requestContext">Commerce runtime request context.</param>
            public PricingDataServiceManager(RequestContext requestContext)
            {
                this.requestContext = requestContext;
            }
    
            /// <summary>
            /// Gets a value indicating whether the cache is disabled.
            /// </summary>
            public bool IsCacheDisabled
            {
                get { return this.requestContext.Runtime.Configuration.CacheControl.IsCachingDisabled; }
            }
    
            /// <summary>
            /// Gets the channel price configuration.
            /// </summary>
            /// <returns>Channel price configuration.</returns>
            public ChannelPriceConfiguration GetChannelPriceConfiguration()
            {
                using (SimpleProfiler profiler = new SimpleProfiler("GetChannelPriceConfiguration", 1))
                {
                    ChannelPriceConfiguration channelPriceConfiguration = null;

                    var response = this.ExecuteDataService<ChannelPriceConfiguration>(new EntityDataServiceRequest<ChannelPriceConfiguration>(QueryResultSettings.FirstRecord));

                    channelPriceConfiguration = response.SingleOrDefault();

                    if (channelPriceConfiguration == null)
                    {
                        channelPriceConfiguration = new ChannelPriceConfiguration()
                        {
                            Company = this.requestContext.GetChannelConfiguration().InventLocationDataAreaId,
                            CompanyCurrency = this.requestContext.GetChannelConfiguration().CompanyCurrency,
                            ChannelTimeZoneId = this.requestContext.GetChannelConfiguration().TimeZoneInfoId,
                        };
                    }

                    return channelPriceConfiguration;
                }
            }
    
            /// <summary>
            /// Get the discount codes (aka 'promo codes') associated with the given discount offer identifiers.
            /// </summary>
            /// <param name="offerIds">The offer Ids whose discount codes are being found.</param>
            /// <returns>Discounts codes found for given offers of type ReadOnlyCollection&lt;DiscountCode&gt;.</returns>
            public object GetDiscountCodesByOfferIds(object offerIds)
            {
                IEnumerable<string> offerIdSet = offerIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetDiscountCodesByOfferId", 1))
                {
                    return this.ExecuteDataService<DiscountCode>(new EntityDataServiceRequest<IEnumerable<string>, DiscountCode>(offerIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Gets the discount codes.
            /// </summary>
            /// <param name="offerId">The offer id (optional, exact match with OfferId).</param>
            /// <param name="discountCode">The discount code (optional, exact match with DiscountCode).</param>
            /// <param name="keyword">The keyword (optional, partial match with OfferId or discount name).</param>
            /// <param name="activeDate">The active date.</param>
            /// <returns>
            /// The discount codes that matches the given conditions.
            /// </returns>
            public ReadOnlyCollection<DiscountCode> GetDiscountCodes(
                string offerId,
                string discountCode,
                string keyword,
                DateTimeOffset activeDate)
            {
                return this.ExecuteDataService<DiscountCode>(new GetDiscountCodesDataRequest(
                                                                        offerId,
                                                                        discountCode,
                                                                        keyword,
                                                                        activeDate,
                                                                        QueryResultSettings.AllRecords));
            }
    
            /// <summary>
            /// Gets the channel price groups.
            /// </summary>
            /// <returns>
            /// A collection of channel price groups of type ReadOnlyCollection&lt;PriceGroup&gt;.
            /// </returns>
            public object GetChannelPriceGroups()
            {
                using (SimpleProfiler profiler = new SimpleProfiler("GetPriceGroups", 1))
                {
                    return this.ExecuteDataService<PriceGroup>(new EntityDataServiceRequest<PriceGroup>(QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Retrieves PriceParameters from the database. This indicates which types of
            /// trade agreements are active for various combinations of customer and item types.
            /// </summary>
            /// <returns>The first (and only) row in PriceParameters in the database.</returns>
            public PriceParameters GetPriceParameters()
            {
                using (SimpleProfiler profiler = new SimpleProfiler("GetPriceParameters", 1))
                {
                    var response = this.ExecuteDataService<PriceParameters>(new EntityDataServiceRequest<PriceParameters>(QueryResultSettings.SingleRecord));
    
                    return response.Single();
                }
            }
    
            /// <summary>
            /// Gets the items using the specified item identifiers.
            /// </summary>
            /// <param name="itemIds">The collection of item identifiers.</param>
            /// <returns>The collection of items of type ReadOnlyCollection&lt;Item&gt;.</returns>
            public object GetItems(object itemIds)
            {
                IEnumerable<string> itemIdSet = itemIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetItems", 1))
                {
                    return this.ExecuteDataService<Item>(new EntityDataServiceRequest<IEnumerable<string>, Item>(itemIdSet, QueryResultSettings.AllRecords));
                }
            }
    
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
            public object ReadDiscountTradeAgreements(
                object itemIds,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                QueryResultSettings settings)
            {
                ISet<string> itemIdSet = itemIds as ISet<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("ReadDiscountTradeAgreements", 1))
                {
                    var dataRequest = new ReadDiscountTradeAgreementsDataRequest(
                                                                            itemIdSet,
                                                                            customerAccount,
                                                                            minActiveDate,
                                                                            maxActiveDate,
                                                                            currencyCode);
                    dataRequest.QueryResultSettings = settings;
                    return this.ExecuteDataService<TradeAgreement>(dataRequest);
                }
            }
    
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
            public object ReadPriceTradeAgreements(
                object itemIds,
                object priceGroups,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                QueryResultSettings settings)
            {
                ISet<string> itemIdSet = itemIds as ISet<string>;
                ISet<string> priceGroupSet = priceGroups as ISet<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("ReadPriceTradeAgreements", 1))
                {
                    var dataRequest = new ReadPriceTradeAgreementsDataRequest(
                                                                            itemIdSet,
                                                                            priceGroupSet,
                                                                            customerAccount,
                                                                            minActiveDate,
                                                                            maxActiveDate,
                                                                            currencyCode);
                    dataRequest.QueryResultSettings = settings;
                    return this.ExecuteDataService<TradeAgreement>(dataRequest);
                }
            }
    
            /// <summary>
            /// Fetch all price adjustments for the given items, striped by item Id and dimension Id.
            /// </summary>
            /// <param name="items">The set of items to search by. Set of pairs of item Id and variant dimension Id. Ignores the unit.</param>
            /// <param name="priceGroups">Set of price groups to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>Collection of price adjustments striped by item Id and variant dimension Id (if any) of type ReadOnlyCollection&lt;PriceAdjustment&gt;.</returns>
            public object ReadPriceAdjustments(
                object items,
                object priceGroups,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                QueryResultSettings settings)
            {
                IEnumerable<ItemUnit> itemSet = items as IEnumerable<ItemUnit>;
                ISet<string> priceGroupSet = priceGroups as ISet<string>;
                ReadOnlyCollection<ValidationPeriod> validationPeriods;
                ReadOnlyCollection<PriceAdjustment> priceAdjustments;
                using (SimpleProfiler profiler = new SimpleProfiler("ReadPriceAdjustments", 1))
                {
                    var dataRequest = new ReadPriceAdjustmentsDataRequest(
                                itemSet,
                                priceGroupSet,
                                minActiveDate,
                                maxActiveDate);
                    dataRequest.QueryResultSettings = settings;
                    priceAdjustments = this.ExecuteDataService<PriceAdjustment, ValidationPeriod>(dataRequest, out validationPeriods);
                }
    
                IDictionary<string, ValidationPeriod> validationPeriodLookup = validationPeriods.ToDictionary(p => p.PeriodId, p => p, StringComparer.OrdinalIgnoreCase);
    
                foreach (PriceAdjustment adjustment in priceAdjustments)
                {
                    if (!string.IsNullOrWhiteSpace(adjustment.ValidationPeriodId))
                    {
                        ValidationPeriod validationPeriod = null;
                        if (validationPeriodLookup.TryGetValue(adjustment.ValidationPeriodId, out validationPeriod))
                        {
                            adjustment.ValidationPeriod = validationPeriod;
                        }
                    }
                }
    
                return priceAdjustments;
            }
    
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
            public object ReadRetailDiscounts(object items, object priceGroups, DateTimeOffset minActiveDate, DateTimeOffset maxActiveDate, string currencyCode, QueryResultSettings settings)
            {
                IEnumerable<ItemUnit> itemSet = items as IEnumerable<ItemUnit>;
                ISet<string> priceGroupSet = priceGroups as ISet<string>;
                ReadOnlyCollection<ValidationPeriod> validationPeriods;
    
                ReadOnlyCollection<PeriodicDiscount> periodicDiscounts;
                using (SimpleProfiler profiler = new SimpleProfiler("ReadRetailDiscounts", 1))
                {
                    var dataRequest = new ReadRetailDiscountsDataRequest(
                                itemSet,
                                priceGroupSet,
                                minActiveDate,
                                maxActiveDate,
                                currencyCode);
                    dataRequest.QueryResultSettings = settings;
                    periodicDiscounts = this.ExecuteDataService<PeriodicDiscount, ValidationPeriod>(dataRequest, out validationPeriods);
                }
    
                IDictionary<string, ValidationPeriod> validationPeriodLookup = validationPeriods.ToDictionary(p => p.PeriodId, p => p, StringComparer.OrdinalIgnoreCase);
    
                foreach (PeriodicDiscount discount in periodicDiscounts)
                {
                    if (!string.IsNullOrWhiteSpace(discount.ValidationPeriodId))
                    {
                        ValidationPeriod validationPeriod = null;
                        if (validationPeriodLookup.TryGetValue(discount.ValidationPeriodId, out validationPeriod))
                        {
                            discount.ValidationPeriod = validationPeriod;
                        }
                    }
                }
    
                return periodicDiscounts;
            }
    
            /// <summary>
            /// Get the variant dimensions populated for the given dimension Ids. This is lightweight and
            ///  only returns the dimension Ids, not translations.
            /// </summary>
            /// <param name="inventoryDimensionIds">The dimension Ids which need dimension values fetched.</param>
            /// <returns>Collection of dimension values of type ReadOnlyCollection&lt;ProductVariant&gt;.</returns>
            public object GetVariantDimensionsByInventDimensionIds(object inventoryDimensionIds)
            {
                IEnumerable<string> inventoryDimensionIdSet = inventoryDimensionIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetVariantDimensionsByItemIds", 1))
                {
                    return this.ExecuteDataService<ProductVariant>(new EntityDataServiceRequest<IEnumerable<string>, ProductVariant>(inventoryDimensionIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Get all the threshold tiers associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch tiers by.</param>
            /// <returns>Collection of tiers (if any) associated with the given offer IdsCollection of dimension values of type ReadOnlyCollection&lt;ThresholdDiscountTier&gt;.</returns>
            public object GetThresholdTiersByOfferIds(object offerIds)
            {
                IEnumerable<string> offerIdSet = offerIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetThresholdTiersByOfferIds", 1))
                {
                    return this.ExecuteDataService<ThresholdDiscountTier>(new EntityDataServiceRequest<IEnumerable<string>, ThresholdDiscountTier>(offerIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Get all the multi buy discount lines associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch discount lines by.</param>
            /// <returns>Collection of multi buy discount lines associated with the given offer Ids of type ReadOnlyCollection&lt;QuantityDiscountLevel&gt;.</returns>
            public object GetMultipleBuyDiscountLinesByOfferIds(object offerIds)
            {
                IEnumerable<string> offerIdSet = offerIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetMultipleBuyDiscountLinesByOfferIds", 1))
                {
                    return this.ExecuteDataService<QuantityDiscountLevel>(new EntityDataServiceRequest<IEnumerable<string>, QuantityDiscountLevel>(offerIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Get all the mix and match line groups associated with the given offers.
            /// </summary>
            /// <param name="offerIds">Offer Ids to fetch mix and match line groups by.</param>
            /// <returns>Collection of mix and match line groups associated with the given offer Ids of type ReadOnlyCollection&lt;MixAndMatchLineGroup&gt;.</returns>
            public object GetMixAndMatchLineGroupsByOfferIds(object offerIds)
            {
                IEnumerable<string> offerIdSet = offerIds as IEnumerable<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetMixAndMatchLineGroupsByOfferIds", 1))
                {
                    return this.ExecuteDataService<MixAndMatchLineGroup>(new EntityDataServiceRequest<IEnumerable<string>, MixAndMatchLineGroup>(offerIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Gets the customer price group.
            /// </summary>
            /// <param name="customerPriceGroupId">Customer price group Id.</param>
            /// <returns>Customer price group.</returns>
            public PriceGroup GetCustomerPriceGroup(string customerPriceGroupId)
            {
                using (SimpleProfiler profiler = new SimpleProfiler("GetCustomerPriceGroup", 1))
                {
                    var response = this.ExecuteDataService<PriceGroup>(new GetCustomerPriceGroupDataRequest(customerPriceGroupId));
    
                    return response.Single();
                }
            }
    
            /// <summary>
            /// Gets the catalog price groups.
            /// </summary>
            /// <param name="catalogIds">Catalog recId's.</param>
            /// <returns>
            /// A collection of catalog price groups of type ReadOnlyCollection&lt;CatalogPriceGroup&gt;.
            /// </returns>
            public object GetCatalogPriceGroups(object catalogIds)
            {
                ISet<long> catalogIdSet = catalogIds as ISet<long>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetCatalogPriceGroups", 1))
                {
                    return this.ExecuteDataService<CatalogPriceGroup>(new EntityDataServiceRequest<IEnumerable<long>, CatalogPriceGroup>(catalogIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Gets the affiliation price groups.
            /// </summary>
            /// <param name="affiliationLoyaltyTiers">A collection of affiliation Id or loyalty tier Id.</param>
            /// <returns>
            /// A collection of affiliation price groups of type ReadOnlyCollection&lt;PriceGroup&gt;.
            /// </returns>
            public object GetAffiliationPriceGroups(object affiliationLoyaltyTiers)
            {
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTierSet = affiliationLoyaltyTiers as IEnumerable<AffiliationLoyaltyTier>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetAffiliationPriceGroups", 1))
                {
                    return this.ExecuteDataService<PriceGroup>(new EntityDataServiceRequest<IEnumerable<AffiliationLoyaltyTier>, PriceGroup>(affiliationLoyaltyTierSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Gets retail discount price groups.
            /// </summary>
            /// <param name="offerIds">Offer identifiers.</param>
            /// <returns>
            /// A collection of retail discount price groups of type ReadOnlyCollection&lt;RetailDiscountPriceGroup&gt;.
            /// </returns>
            public object GetRetailDiscountPriceGroups(object offerIds)
            {
                ISet<string> offerIdSet = offerIds as ISet<string>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetRetailDiscountPriceGroups", 1))
                {
                    return this.ExecuteDataService<RetailDiscountPriceGroup>(new EntityDataServiceRequest<IEnumerable<string>, RetailDiscountPriceGroup>(offerIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Get all of the discounts configured in the system.
            /// </summary>
            /// <returns>The collection of discounts of type ReadOnlyCollection&lt;RetailDiscount&gt;.</returns>
            public object GetAllRetailDiscounts()
            {
                return this.ExecuteDataService<RetailDiscount>(new EntityDataServiceRequest<RetailDiscount>(QueryResultSettings.AllRecords));
            }
    
            /// <summary>
            /// Get the category membership information for the product or variant identifiers passed in.
            /// </summary>
            /// <param name="productOrVariantIds">A set of product or variant identifiers.</param>
            /// <returns>The collection of mappings between the product or variant identifier and the category identifier of type ReadOnlyCollection&lt;RetailCategoryMember&gt;.</returns>
            public object GetRetailCategoryMembersForItems(object productOrVariantIds)
            {
                ISet<long> productOrVariantIdSet = productOrVariantIds as ISet<long>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetRetailCategoryMembersForItems", 1))
                {
                    return this.ExecuteDataService<RetailCategoryMember>(new EntityDataServiceRequest<IEnumerable<long>, RetailCategoryMember>(productOrVariantIdSet, QueryResultSettings.AllRecords));
                }
            }
    
            /// <summary>
            /// Gets the variants for the specified collection of item variant inventory dimension identifiers.
            /// </summary>
            /// <param name="itemVariants">The collection of item variant inventory dimension.</param>
            /// <returns>The variant with specified columns populated. Null if variant not found of type ReadOnlyCollection&lt;ProductVariant&gt;.</returns>
            public object GetVariants(object itemVariants)
            {
                IEnumerable<ItemVariantInventoryDimension> itemVariantSet = itemVariants as IEnumerable<ItemVariantInventoryDimension>;
                using (SimpleProfiler profiler = new SimpleProfiler("GetVariants", 1))
                {
                    GetProductVariantsDataRequest getVariantsRequest = new GetProductVariantsDataRequest(itemVariantSet);
                    getVariantsRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                    return this.ExecuteDataService<ProductVariant>(getVariantsRequest);
                }
            }
    
            private ReadOnlyCollection<T> ExecuteDataService<T>(Request request) where T : CommerceEntity
            {
                var response = this.requestContext.Runtime.Execute<EntityDataServiceResponse<T>>(request, this.requestContext);
    
                return response.PagedEntityCollection.Results;
            }
    
            private ReadOnlyCollection<TResponse> ExecuteDataService<TResponse, TOutParam>(Request request, out ReadOnlyCollection<TOutParam> outputParams)
                where TResponse : CommerceEntity
                where TOutParam : CommerceEntity
            {
                var response = this.requestContext.Runtime.Execute<EntityDataServiceResponse<TResponse, TOutParam>>(request, this.requestContext);
    
                outputParams = response.OutputParams.AsReadOnly();
    
                return response.EntityCollection;
            }
        }
    }
}
