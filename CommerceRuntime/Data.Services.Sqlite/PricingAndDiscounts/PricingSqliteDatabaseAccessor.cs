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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Pricing SQLite database accessor class.
        /// </summary>
        public class PricingSqliteDatabaseAccessor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PricingSqliteDatabaseAccessor"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public PricingSqliteDatabaseAccessor(RequestContext context)
            {
                this.Context = context;
            }
    
            /// <summary>
            /// Gets or sets the request context.
            /// </summary>
            private RequestContext Context { get; set; }
    
            /// <summary>
            /// Fetch all retail discounts for the given items, striped by item Id and dimension Id.
            /// </summary>
            /// <param name="items">The set of items to search by. Set of pairs of item Id and variant dimension Id. Ignores the unit.</param>
            /// <param name="priceGroups">Set of price groups to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="currencyCode">Currency code to filter by.</param>
            /// <param name="validationPeriods">Output all the validation periods used by the discovered price adjustments.</param>
            /// <returns>Collection of price adjustments striped by item Id and variant dimension Id (if any).</returns>
            public ReadOnlyCollection<PeriodicDiscount> ReadRetailDiscounts(
                IEnumerable<ItemUnit> items,
                IEnumerable<string> priceGroups,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode,
                out ReadOnlyCollection<ValidationPeriod> validationPeriods)
            {
                ThrowIf.Null(items, "items");
                ThrowIf.Null(priceGroups, "priceGroups");
                ThrowIf.Null(currencyCode, "currencyCode");
    
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    var priceGroupCollection = PricingProcedure.GetPriceGroup(context, priceGroups);
                    var itemUnits = PricingProcedure.GetItemsUnit(context, items);
    
                    IEnumerable<long> priceGroupRecIds = priceGroupCollection.Select(price => price.PriceGroupId);
    
                    ReadOnlyCollection<PeriodicDiscount> periodicDiscounts = DiscountProcedure.GetRetailDiscount(
                        context,
                        itemUnits.Results,
                        priceGroupRecIds,
                        minActiveDate.DateTime,
                        maxActiveDate.DateTime,
                        currencyCode);
    
                    // fetch any validation periods in use by the price adjustments
                    var periodIds = periodicDiscounts
                        .Where(p => p.DateValidationType == (int)DateValidationType.Advanced && !string.IsNullOrWhiteSpace(p.ValidationPeriodId))
                        .Select(p => p.ValidationPeriodId)
                        .Distinct();
    
                    if (periodIds.Any())
                    {
                        validationPeriods = PricingProcedure.GetValidationPeriodsByIds(context, periodIds).Results;
                    }
                    else
                    {
                        validationPeriods = new ReadOnlyCollection<ValidationPeriod>(new ValidationPeriod[0]);
                    }
    
                    return periodicDiscounts;
                }
            }
    
            /// <summary>
            /// Fetch the superset of price trade agreements which could apply to all of these items and customer for the given date.
            /// </summary>
            /// <param name="itemIds">The item Ids to fetch for agreements for.</param>
            /// <param name="priceGroups">The price groups (probably channel) to query by.</param>
            /// <param name="customerAccount">Optional. Customer account number to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="currencyCode">Currency code to filter by.</param>
            /// <returns>Collection of trade agreements which may be applied to the given items.</returns>
            public PagedResult<TradeAgreement> ReadPriceTradeAgreements(
                IEnumerable<string> itemIds,
                ISet<string> priceGroups,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode)
            {
                ThrowIf.Null(itemIds, "itemIds");
                ThrowIf.Null(priceGroups, "priceGroups");
    
                if (minActiveDate > maxActiveDate)
                {
                    throw new ArgumentException("minActiveDate must be less than or equal to maxActiveDate.");
                }
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.Context))
                {
                    return PricingProcedure.GetPriceTradeAgreements(
                        context,
                        itemIds,
                        priceGroups,
                        customerAccount,
                        minActiveDate,
                        maxActiveDate,
                        currencyCode);
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
            /// <returns>Collection of trade agreements which may be applied to the given items.</returns>
            public ReadOnlyCollection<TradeAgreement> ReadDiscountTradeAgreements(
                IEnumerable<string> itemIds,
                string customerAccount,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                string currencyCode)
            {
                ThrowIf.Null(itemIds, "itemIds");
    
                if (minActiveDate > maxActiveDate)
                {
                    throw new ArgumentException("minActiveDate must be less than or equal to maxActiveDate.");
                }
    
                DateTimeOffset minActiveDateChannelTimeZone = this.Context.ConvertDateTimeToChannelDate(minActiveDate);
                DateTimeOffset maxActiveDateChannelTimeZone = this.Context.ConvertDateTimeToChannelDate(maxActiveDate);
    
                using (var context = new SqliteDatabaseContext(this.Context))
                {
                    return DiscountProcedure.GetAllDiscountTradeAgreements(
                        context,
                        itemIds,
                        customerAccount,
                        minActiveDateChannelTimeZone,
                        maxActiveDateChannelTimeZone,
                        currencyCode);
                }
            }
    
            /// <summary>
            /// Gets the affiliation price groups.
            /// </summary>
            /// <param name="affiliationLoyaltyTiers">A collection of affiliation Id or loyalty tier Id.</param>
            /// <returns>
            /// A collection of affiliation price groups.
            /// </returns>
            public ReadOnlyCollection<PriceGroup> GetAffiliationPriceGroups(IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers)
            {
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.Context))
                {
                    var affiliationPriceGroupCollection = PricingProcedure.GetAffiliationPriceGroups(context, affiliationLoyaltyTiers);
    
                    return affiliationPriceGroupCollection.Results;
                }
            }
    
            /// <summary>
            /// Fetch all price adjustments for the given items, striped by item Id and dimension Id.
            /// </summary>
            /// <param name="items">The set of items to search by. Set of pairs of item Id and variant dimension Id. Ignores the unit.</param>
            /// <param name="priceGroups">Set of price groups to search by.</param>
            /// <param name="minActiveDate">The earliest inclusive active date to search by. Must be less than or equal to maxActiveDate.</param>
            /// <param name="maxActiveDate">The latest inclusive active date to search by. Must be greater than or equal to minActiveDate.</param>
            /// <param name="validationPeriods">Output all the validation periods used by the discovered price adjustments.</param>
            /// <returns>Collection of price adjustments striped by item Id and variant dimension Id (if any).</returns>
            public ReadOnlyCollection<PriceAdjustment> ReadPriceAdjustments(
                IEnumerable<ItemUnit> items,
                ISet<string> priceGroups,
                DateTimeOffset minActiveDate,
                DateTimeOffset maxActiveDate,
                out ReadOnlyCollection<ValidationPeriod> validationPeriods)
            {
                ThrowIf.Null(items, "items");
                ThrowIf.Null(priceGroups, "priceGroups");
    
                if (minActiveDate > maxActiveDate)
                {
                    throw new ArgumentException("minActiveDate must be less than or equal to maxActiveDate.");
                }
    
                PagedResult<PriceAdjustment> promotionLines = null;
    
                using (SqliteDatabaseContext context = new SqliteDatabaseContext(this.Context))
                {
                    var priceGroupCollection = PricingProcedure.GetPriceGroup(context, priceGroups);
    
                    promotionLines = PricingProcedure.GetRetailPriceAdjustments(context, items, priceGroupCollection, minActiveDate.DateTime, maxActiveDate.DateTime);
    
                    // fetch any validation periods in use by the price adjustments
                    var periodIds = promotionLines.Results
                        .Where(p => p.DateValidationType == (int)DateValidationType.Advanced && !string.IsNullOrWhiteSpace(p.ValidationPeriodId))
                        .Select(p => p.ValidationPeriodId)
                        .Distinct();
    
                    if (periodIds.Any())
                    {
                        validationPeriods = PricingProcedure.GetValidationPeriodsByIds(context, periodIds).Results;
                    }
                    else
                    {
                        validationPeriods = new ReadOnlyCollection<ValidationPeriod>(new ValidationPeriod[0]);
                    }
                }
    
                return promotionLines.Results;
            }
        }
    }
}
