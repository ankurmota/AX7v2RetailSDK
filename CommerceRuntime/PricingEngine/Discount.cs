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
        using System.Text;
        using Commerce.Runtime.Services.PricingEngine.DiscountData;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Contains the entry points to most of the retail discount calculation. This class will handle
        ///   calculating trade agreement line discounts, multiline discounts, total discounts, as well as retail periodic discounts.
        /// </summary>
        internal class Discount
        {
            private const string ExtendedItemCacheName = "ITEMCACHE";
    
            /// <summary>
            /// Prevents a default instance of the <see cref="Discount"/> class from being created.
            /// </summary>
            private Discount()
            {
            }
    
            private DiscountParameters DiscountParameters { get; set; }
    
            /// <summary>
            /// Create instance of the Discount class.
            /// </summary>
            /// <returns>New instance of Discount class.</returns>
            public static Discount Create()
            {
                return new Discount();
            }
    
            /// <summary>
            /// Gets the periodic discount data.
            /// </summary>
            /// <param name="discounts">Collection of all periodic discounts to filter from.</param>
            /// <param name="itemId">The item id.</param>
            /// <param name="inventoryDimensionId">The variant id.</param>
            /// <returns>Collection of periodic discount configurations for the given item and variant.</returns>
            public static ReadOnlyCollection<PeriodicDiscount> GetPeriodicDiscountData(
                ReadOnlyCollection<PeriodicDiscount> discounts,
                string itemId,
                string inventoryDimensionId)
            {
                ThrowIf.Null(discounts, "discounts");
    
                var applicableDiscounts = discounts.Where(d =>
                    string.Equals(d.ItemId, itemId, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(d.InventoryDimensionId) ||
                        string.Equals(d.InventoryDimensionId, inventoryDimensionId, StringComparison.OrdinalIgnoreCase)));
    
                var offerIds = applicableDiscounts.Select(p => p.OfferId);
                NetTracer.Information(
                    "Discount.GetPeriodicDiscountData(): Found {0} periodic discounts for product '{1}' (variant '{2}'):{3}",
                        offerIds.Count(),
                        itemId,
                        inventoryDimensionId ?? string.Empty,
                        offerIds.Aggregate(new StringBuilder(), (ids, id) => ids.AppendFormat(" '{0}'", id)).ToString());
    
                return applicableDiscounts.AsReadOnly();
            }
    
            /// <summary>
            /// Initializes the specified runtime.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            public void Initialize(IPricingDataAccessor pricingDataManager)
            {
                this.DiscountParameters = DiscountParameters.CreateAndInitialize(pricingDataManager);
            }
    
            /// <summary>
            /// Calculate the customer discount.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="priceContext">Context for the pricing calculation.</param>
            public void CalcCustomerDiscount(
                IPricingDataAccessor pricingDataManager,
                SalesTransaction salesTransaction,
                PriceContext priceContext)
            {
                // Fetch all discount trade agreements if the data manager supports it
                List<TradeAgreement> lineTradeAgreements = null;
                List<TradeAgreement> multiLineTradeAgreements = null;
                List<TradeAgreement> totalTradeAgreements = null;
                HashSet<string> items = new HashSet<string>(salesTransaction.PriceCalculableSalesLines.Select(sl => sl.ItemId).Distinct(), StringComparer.OrdinalIgnoreCase);
                IEnumerable<TradeAgreement> tradeAgreements = pricingDataManager.ReadDiscountTradeAgreements(items, priceContext.CustomerAccount, priceContext.ActiveDate, priceContext.ActiveDate, priceContext.CurrencyCode, QueryResultSettings.AllRecords) as IEnumerable<TradeAgreement>;
    
                lineTradeAgreements = new List<TradeAgreement>();
                multiLineTradeAgreements = new List<TradeAgreement>();
                totalTradeAgreements = new List<TradeAgreement>();
                foreach (var agreement in tradeAgreements)
                {
                    if (agreement.Relation == PriceDiscountType.LineDiscountSales)
                    {
                        lineTradeAgreements.Add(agreement);
                    }
                    else if (agreement.Relation == PriceDiscountType.MultilineDiscountSales)
                    {
                        multiLineTradeAgreements.Add(agreement);
                    }
                    else if (agreement.Relation == PriceDiscountType.EndDiscountSales)
                    {
                        totalTradeAgreements.Add(agreement);
                    }
                }
    
                // Calculate line discount
                var lineDiscountCalculator = new LineDiscountCalculator(this.DiscountParameters, priceContext);
                salesTransaction = lineDiscountCalculator.CalcLineDiscount(lineTradeAgreements, salesTransaction);
    
                // Calculate multiline discount
                var multilineDiscountCalculator = new MultilineDiscountCalculator(this.DiscountParameters, priceContext);
                salesTransaction = multilineDiscountCalculator.CalcMultiLineDiscount(multiLineTradeAgreements, salesTransaction);
    
                // Calculate total discount
                var totalDiscountCalculator = new TotalDiscountCalculator(this.DiscountParameters, priceContext);
                salesTransaction = totalDiscountCalculator.CalcTotalCustomerDiscount(totalTradeAgreements, salesTransaction);
            }
    
            /// <summary>
            /// Update the discount items.
            /// </summary>
            /// <param name="saleItem">The item line that the discount line is added to.</param>
            /// <param name="discountItem">The new discount line to add.</param>
            internal static void UpdateDiscountLines(SalesLine saleItem, DiscountLine discountItem)
            {
                // Check if line discount is found, if so then update
                bool discountLineFound = false;
                foreach (var discLine in saleItem.DiscountLines)
                {
                    if (discLine.DiscountLineType == DiscountLineType.CustomerDiscount)
                    {
                        // If found then update
                        if ((discLine.DiscountLineType == discountItem.DiscountLineType) &&
                            (discLine.CustomerDiscountType == discountItem.CustomerDiscountType))
                        {
                            discLine.Percentage = discountItem.Percentage;
                            discLine.Amount = discountItem.Amount;
                            discountLineFound = true;
                        }
                    }
                }
    
                // If line discount is not found then add it.
                if (!discountLineFound)
                {
                    saleItem.DiscountLines.Add(discountItem);
                }
    
                if (discountItem.DiscountLineType == DiscountLineType.CustomerDiscount)
                {
                    saleItem.ResetLineMultilineDiscountOnItem();
                }
    
                saleItem.WasChanged = true;
            }
    
            internal static void ClearDiscountLinesOfType(SalesTransaction transaction, DiscountLineType? lineType)
            {
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var line in transaction.PriceCalculableSalesLines)
                {
                    ClearDiscountLinesOfType(line, lineType);
                }
            }
    
            /// <summary>
            ///  Clear manual discount of particular type from transaction.
            /// </summary>
            /// <param name="transaction">Transaction containing the discount.</param>
            /// <param name="manualDiscountType">Type of manual discount.</param>
            internal static void ClearManualDiscountLinesOfType(SalesTransaction transaction, ManualDiscountType manualDiscountType)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
    
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var line in transaction.PriceCalculableSalesLines)
                {
                    ClearManualDiscountLinesOfType(line, manualDiscountType);
                }
            }
    
            /// <summary>
            /// Clear manual discount of particular type from sales line.
            /// </summary>
            /// <param name="salesLine">Sales line containing discount line.</param>
            /// <param name="manualDiscountType">Type of manual discount.</param>
            internal static void ClearManualDiscountLinesOfType(SalesLine salesLine, ManualDiscountType manualDiscountType)
            {
                if (salesLine == null)
                {
                    throw new ArgumentNullException("salesLine");
                }
    
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                var allDiscounts = salesLine.DiscountLines.ToList();
                salesLine.DiscountLines.Clear();
    
                foreach (DiscountLine discountLine in allDiscounts)
                {
                    if (!(discountLine.DiscountLineType == DiscountLineType.ManualDiscount && discountLine.ManualDiscountType == manualDiscountType))
                    {
                        salesLine.DiscountLines.Add(discountLine);
                    }
                }
            }
    
            internal static void ClearDiscountLinesOfType(SalesLine salesLine, DiscountLineType? lineType)
            {
                if (lineType == null)
                {
                    salesLine.DiscountLines.Clear();
                    return;
                }
    
                var remainingDiscounts = salesLine.DiscountLines.Where(l => l.DiscountLineType != lineType).ToList();
                salesLine.DiscountLines.Clear();
                foreach (var discount in remainingDiscounts)
                {
                    salesLine.DiscountLines.Add(discount);
                }
    
                if (lineType == DiscountLineType.PeriodicDiscount)
                {
                    salesLine.PeriodicDiscountPossibilities.Clear();
                    salesLine.QuantityDiscounted = 0;
                }
    
                if (lineType == DiscountLineType.CustomerDiscount)
                {
                    salesLine.LineMultilineDiscOnItem = LineMultilineDiscountOnItem.None;
                }
            }
    
            internal static string GetUnitOfMeasure(SalesLine salesLine)
            {
                string unitOfMeasure = salesLine.SalesOrderUnitOfMeasure;
                if (string.IsNullOrWhiteSpace(unitOfMeasure))
                {
                    unitOfMeasure = salesLine.UnitOfMeasureSymbol ?? string.Empty;
                }
    
                return unitOfMeasure;
            }
    
            internal static PriceContext BuildPriceContext(
               IPricingDataAccessor pricingDataManager,
               ICurrencyOperations currencyAndRoundingHelper,
               Customer customer,
               SalesTransaction transaction,
               string currencyCode,
               bool doesPriceIncludeTax,
               DiscountCalculationMode discountCalculationMode,
               DateTimeOffset activeDate)
            {
                string customerAccount = string.Empty;
                string customerPriceGroup = string.Empty;
                string customerLinePriceGroup = string.Empty;
                string customerMultipleLinePriceGroup = string.Empty;
                string customerTotalPriceGroup = string.Empty;
    
                if (customer != null)
                {
                    if (!string.IsNullOrWhiteSpace(customer.AccountNumber))
                    {
                        customerAccount = customer.AccountNumber;
                    }
    
                    if (!string.IsNullOrWhiteSpace(customer.PriceGroup))
                    {
                        customerPriceGroup = customer.PriceGroup;
                    }
    
                    if (!string.IsNullOrWhiteSpace(customer.LineDiscountGroup))
                    {
                        customerLinePriceGroup = customer.LineDiscountGroup;
                    }
    
                    if (!string.IsNullOrWhiteSpace(customer.MultilineDiscountGroup))
                    {
                        customerMultipleLinePriceGroup = customer.MultilineDiscountGroup;
                    }
    
                    if (!string.IsNullOrWhiteSpace(customer.TotalDiscountGroup))
                    {
                        customerTotalPriceGroup = customer.TotalDiscountGroup;
                    }
                }
    
                ISet<string> itemIds = PriceContextHelper.GetItemIds(transaction);
                ISet<long> catalogIds = PriceContextHelper.GetCatalogIds(transaction);
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTierIds = PriceContextHelper.GetAffiliationLoyalTierIds(transaction);
    
                return PriceContextHelper.CreatePriceContext(
                        pricingDataManager,
                        currencyAndRoundingHelper,
                        PricingCalculationMode.Transaction,
                        discountCalculationMode,
                        itemIds,
                        catalogIds,
                        affiliationLoyaltyTierIds,
                        customerAccount,
                        customerPriceGroup,
                        customerLinePriceGroup,
                        customerMultipleLinePriceGroup,
                        customerTotalPriceGroup,
                        doesPriceIncludeTax,
                        currencyCode ?? string.Empty,
                        activeDate);
            }
    
            /// <summary>
            /// Calculate the periodic discounts for the transaction.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="customer">Customer for the transaction containing customer discount groups.</param>
            /// <param name="doesPriceIncludeTax">Does the channel have tax-inclusive prices.</param>
            /// <param name="currencyCode">Currency code to filter discounts by.</param>
            /// <param name="channelDateTime">Channel time.</param>
            internal static void GetAllPeriodicDisc(
                IPricingDataAccessor pricingDataManager,
                ICurrencyOperations currencyAndRoundingHelper,
                SalesTransaction transaction,
                Customer customer,
                bool doesPriceIncludeTax,
                string currencyCode,
                DateTimeOffset channelDateTime)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
    
                // Clear all periodic, customer, standard, total discounts
                ClearDiscountLinesOfType(transaction, null);
    
                PriceContext priceContext = BuildPriceContext(pricingDataManager, currencyAndRoundingHelper, customer, transaction, currencyCode, channelDateTime, doesPriceIncludeTax, DiscountCalculationMode.CalculateAll);
                PricingEngine.PopulateProductIds(pricingDataManager, priceContext, transaction);
    
                ReadOnlyCollection<PeriodicDiscount> periodicDiscountData = GetRetailDiscounts(transaction.ActiveSalesLines, priceContext, pricingDataManager, QueryResultSettings.AllRecords);
                foreach (SalesLine salesLine in transaction.PriceCalculableSalesLines)
                {
                    ReadOnlyCollection<PeriodicDiscount> applicablePeriodicDiscounts = Discount.GetPeriodicDiscountData(periodicDiscountData, salesLine.ItemId, salesLine.InventoryDimensionId);
    
                    foreach (PeriodicDiscount periodicDiscount in applicablePeriodicDiscounts)
                    {
                        var discountLine = new DiscountLine
                        {
                            OfferId = periodicDiscount.OfferId,
                            OfferName = periodicDiscount.Name,
                            Percentage = periodicDiscount.DiscountPercent,
                            Amount = periodicDiscount.DiscountAmount,
                            ConcurrencyMode = periodicDiscount.ConcurrencyMode,
                            ConcurrencyModeValue = (int)periodicDiscount.ConcurrencyMode,
                            DiscountLineType = DiscountLineType.PeriodicDiscount,
                            DiscountLineTypeValue = (int)DiscountLineType.PeriodicDiscount,
                            PeriodicDiscountType = periodicDiscount.PeriodicDiscountType,
                            PeriodicDiscountTypeValue = (int)periodicDiscount.PeriodicDiscountType,
                            DiscountApplicationGroup = salesLine.ItemId,
                            IsDiscountCodeRequired = periodicDiscount.IsDiscountCodeRequired
                        };
    
                        salesLine.DiscountLines.Add(discountLine);
                    }
                }
            }
    
            /// <summary>
            /// Gets the discount data.
            /// </summary>
            /// <param name="tradeAgreements">Trade agreement collection to calculate on.</param>
            /// <param name="relation">The relation (line, multiline, total).</param>
            /// <param name="itemRelation">The item relation.</param>
            /// <param name="accountRelation">The account relation.</param>
            /// <param name="itemCode">The item code (table, group, all).</param>
            /// <param name="accountCode">The account code (table, group, all).</param>
            /// <param name="quantityAmount">The quantity or amount that sets the minimum quantity or amount needed.</param>
            /// <param name="priceContext">The price context.</param>
            /// <param name="itemDimensions">The item dimensions.</param>
            /// <param name="includeDimensions">A value indicating whether to include item dimensions.</param>
            /// <returns>
            /// A collection of discount agreement arguments.
            /// </returns>
            internal static ReadOnlyCollection<TradeAgreement> GetPriceDiscData(
                List<TradeAgreement> tradeAgreements,
                PriceDiscountType relation,
                string itemRelation,
                string accountRelation,
                PriceDiscountItemCode itemCode,
                PriceDiscountAccountCode accountCode,
                decimal quantityAmount,
                PriceContext priceContext,
                ProductVariant itemDimensions,
                bool includeDimensions)
            {
                accountRelation = accountRelation ?? string.Empty;
                itemRelation = itemRelation ?? string.Empty;
                string targetCurrencyCode = priceContext.CurrencyCode ?? string.Empty;
                string inventColorId = (itemDimensions != null && itemDimensions.ColorId != null && includeDimensions) ? itemDimensions.ColorId : string.Empty;
                string inventSizeId = (itemDimensions != null && itemDimensions.SizeId != null && includeDimensions) ? itemDimensions.SizeId : string.Empty;
                string inventStyleId = (itemDimensions != null && itemDimensions.StyleId != null && includeDimensions) ? itemDimensions.StyleId : string.Empty;
                string inventConfigId = (itemDimensions != null && itemDimensions.ConfigId != null && includeDimensions) ? itemDimensions.ConfigId : string.Empty;
    
                DateTime today = priceContext.ActiveDate.DateTime;
                DateTime noDate = new DateTime(1900, 1, 1);
    
                ReadOnlyCollection<TradeAgreement> foundAgreements;
                foundAgreements = GetAgreementsFromCollection(tradeAgreements, relation, itemRelation, accountRelation, itemCode, accountCode, quantityAmount, targetCurrencyCode, inventColorId, inventSizeId, inventStyleId, inventConfigId, today, noDate);
    
                return foundAgreements;
            }
    
            /// <summary>
            /// Calculates all of the discounts for the transactions.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="priceContext">The price context.</param>
            internal void CalculateDiscount(
                IPricingDataAccessor pricingDataManager,
                SalesTransaction transaction,
                PriceContext priceContext)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }
    
                bool priceLock = false;
    
                PricingEngine.PopulateProductIds(pricingDataManager, priceContext, transaction);
    
                // if prices aren't locked on transaction, compute automatic discounts
                if (!priceLock)
                {
                    // Clear all the periodic discounts
                    ClearDiscountLinesOfType(transaction, null);
    
                    // Now calculate the discounts.
                    DiscountCalculator calculator = new DiscountCalculator(transaction, priceContext, pricingDataManager);
                    calculator.CalculateDiscounts(transaction);
    
                    // Calculation of customer discount
                    // should pass in "sales rounding" rule
                    if (!string.IsNullOrEmpty(transaction.CustomerId))
                    {
                        // Calculation of customer discount
                        this.CalcCustomerDiscount(pricingDataManager, transaction, priceContext); // should pass in "sales rounding" rule
                    }
                }
    
                // this is manual total discount, it should always be calculated
                // technically, this should be a different rounding rule for sales rounding
                this.CalculateLineManualDiscount(transaction, priceContext);
    
                this.CalculateTotalManualDiscount(transaction, priceContext);
    
                Discount.CalculateLoyaltyManualDiscount(transaction, priceContext);
            }
    
            private static PriceContext BuildPriceContext(
               IPricingDataAccessor pricingDataManager,
               ICurrencyOperations currencyAndRoundingHelper,
               Customer customer,
               SalesTransaction transaction,
               string currencyCode,
               DateTimeOffset channelDateTime,
               bool doesPriceIncludeTax,
               DiscountCalculationMode discountCalculationMode)
            {
                return BuildPriceContext(
                        pricingDataManager,
                        currencyAndRoundingHelper,
                        customer,
                        transaction,
                        currencyCode,
                        doesPriceIncludeTax,
                        discountCalculationMode,
                        channelDateTime);
            }
    
            private static ReadOnlyCollection<PeriodicDiscount> GetRetailDiscounts(
                IEnumerable<SalesLine> salesLines,
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
    
                var items = salesLines.Select(l => new ItemUnit
                {
                    ItemId = l.ItemId,
                    VariantInventoryDimensionId = l.InventoryDimensionId ?? string.Empty,
                    UnitOfMeasure = Discount.GetUnitOfMeasure(l),
                });
    
                ReadOnlyCollection<PeriodicDiscount> discounts =
                    pricingDataManager.ReadRetailDiscounts(items, allPriceGroups, priceContext.ActiveDate, priceContext.ActiveDate, priceContext.CurrencyCode, settings) as ReadOnlyCollection<PeriodicDiscount>;
    
                ReadOnlyCollection<PeriodicDiscount> validDiscounts =
                    discounts.Where(d => InternalValidationPeriod.ValidateDateAgainstValidationPeriod((DateValidationType)d.DateValidationType, d.ValidationPeriod, d.ValidFromDate, d.ValidToDate, priceContext.ActiveDate)).AsReadOnly();
    
                return validDiscounts;
            }
    
            private static void CalculateLoyaltyManualDiscount(SalesTransaction transaction, PriceContext priceContext)
            {
                LoyaltyDiscountCalculator loyaltyDiscountCalculator = new LoyaltyDiscountCalculator(priceContext);
                loyaltyDiscountCalculator.CalculateLoyaltyManualDiscount(transaction);
            }
    
            private static ReadOnlyCollection<TradeAgreement> GetAgreementsFromCollection(List<TradeAgreement> tradeAgreements, PriceDiscountType relation, string itemRelation, string accountRelation, PriceDiscountItemCode itemCode, PriceDiscountAccountCode accountCode, decimal quantityAmount, string targetCurrencyCode, string inventColorId, string inventSizeId, string inventStyleId, string inventConfigId, DateTime today, DateTime noDate)
            {
                ReadOnlyCollection<TradeAgreement> foundAgreements;
                foundAgreements = tradeAgreements
                    .Where(ta =>
                        ta.Relation == relation &&
                        ta.ItemCode == itemCode &&
                        string.Equals(ta.ItemRelation, itemRelation, StringComparison.OrdinalIgnoreCase) &&
                        ta.AccountCode == accountCode &&
                        string.Equals(ta.AccountRelation, accountRelation, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(ta.Currency, targetCurrencyCode, StringComparison.OrdinalIgnoreCase) &&
                        ta.QuantityAmountFrom <= Math.Abs(quantityAmount) &&
                        (ta.QuantityAmountTo >= Math.Abs(quantityAmount) || ta.QuantityAmountTo == 0) &&
                        ((ta.FromDate <= today || ta.FromDate <= noDate) &&
                            (ta.ToDate >= today || ta.ToDate <= noDate)) &&
                        (string.IsNullOrWhiteSpace(ta.ColorId) || ta.ColorId.Equals(inventColorId, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(ta.SizeId) || ta.SizeId.Equals(inventSizeId, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(ta.StyleId) || ta.StyleId.Equals(inventStyleId, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrWhiteSpace(ta.ConfigId) || ta.ConfigId.Equals(inventConfigId, StringComparison.OrdinalIgnoreCase)))
                    .AsReadOnly();
    
                 return foundAgreements;
            }
    
            private void CalculateTotalManualDiscount(SalesTransaction transaction, PriceContext priceContext)
            {
                TotalDiscountCalculator totalDiscountCalculator = new TotalDiscountCalculator(this.DiscountParameters, priceContext);
                totalDiscountCalculator.CalculateTotalManualDiscount(transaction);
            }
    
            private void CalculateLineManualDiscount(SalesTransaction transaction, PriceContext priceContext)
            {
                LineDiscountCalculator lineDiscountCalcultor = new LineDiscountCalculator(this.DiscountParameters, priceContext);
                lineDiscountCalcultor.CalculateLineManualDiscount(transaction);
            }
        }
    }
}
