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
        /// This class finds all possible price trade agreement lines for items and creates a set
        ///  of price trade agreement price lines for each item line, keyed by item line line Id.
        /// </summary>
        internal class TradeAgreementCalculator : IPricingCalculator
        {
            private static readonly DateTime NoDate = new DateTime(1900, 1, 1);
    
            /// <summary>
            /// Prevents a default instance of the TradeAgreementCalculator class from being created.
            /// </summary>
            private TradeAgreementCalculator()
            {
            }
    
            /// <summary>
            /// Factory method to get an instance of the price trade agreement calculator.
            /// </summary>
            /// <returns>Instance of a price trade agreement calculator.</returns>
            public static TradeAgreementCalculator CreateTradeAgreementCalculator()
            {
                return new TradeAgreementCalculator();
            }
    
            /// <summary>
            /// Implements the IPricingCalculator interface to calculate item price trade agreement prices.
            /// </summary>
            /// <param name="salesLines">The item lines which need prices.</param>
            /// <param name="priceContext">The configuration of the overall pricing context for the calculation.</param>
            /// <param name="pricingDataManager">Instance of pricing data manager to access pricing data.</param>
            /// <returns>Sets of possible price lines keyed by item line Id.</returns>
            public Dictionary<string, IEnumerable<PriceLine>> CalculatePriceLines(
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                IPricingDataAccessor pricingDataManager)
            {
                Tuple<DateTimeOffset, DateTimeOffset> dateRange = PricingEngine.GetMinAndMaxActiveDates(salesLines, priceContext.ActiveDate);
    
                // look up all trade agreements for given items and context
                HashSet<string> itemIds = new HashSet<string>(salesLines.Select(s => s.ItemId).Distinct(), StringComparer.OrdinalIgnoreCase);
                ReadOnlyCollection<TradeAgreement> tradeAgreements = pricingDataManager.ReadPriceTradeAgreements(
                    itemIds,
                    PriceContextHelper.GetAllPriceGroupsForPrice(priceContext),
                    priceContext.CustomerAccount,
                    dateRange.Item1,
                    dateRange.Item2,
                    priceContext.CurrencyCode,
                    QueryResultSettings.AllRecords) as ReadOnlyCollection<TradeAgreement>;

                if (priceContext.IsDiagnosticsCollected && tradeAgreements.Any())
                {
                    priceContext.PricingEngineDiagnosticsObject.AddTradeAgreementsConsidered(tradeAgreements.ToList());
                }                
    
                var agreementsByItemId = IndexAgreementsByItemId(tradeAgreements);
    
                var discountParameters = DiscountParameters.CreateAndInitialize(priceContext.PriceParameters);
    
                Dictionary<string, IEnumerable<PriceLine>> itemPriceLines;
                Dictionary<string, decimal> itemQuantites = null;
                if (priceContext.PriceCalculationMode == PricingCalculationMode.Transaction)
                {
                    itemQuantites = GetItemQuantities(salesLines);
                }
    
                itemPriceLines = new Dictionary<string, IEnumerable<PriceLine>>(StringComparer.OrdinalIgnoreCase);
    
                foreach (SalesLine salesLine in salesLines)
                {
                    Tuple<decimal, string> priceCustPriceGroup = CalculateAgreementPriceLine(salesLines, salesLine, priceContext, agreementsByItemId, discountParameters, itemQuantites);
                    if (priceCustPriceGroup.Item1 != decimal.Zero)
                    {
                        itemPriceLines.Add(salesLine.LineId, new List<PriceLine>(1) { ConstructTradeAgreementPriceLine(priceCustPriceGroup) });
                    }
                }
    
                return itemPriceLines;
            }
    
            /// <summary>
            /// Get struct PriceResult from active trade agreement.
            /// Struct PriceResult contains India MaxRetailPrice. Currently there is a field �Max. retail price� in the form price/discount agreement journal
            /// (Navigation path: Main Menu > Sales and marketing > Journal > price/discount agreement journal).
            /// The field will be visible only when the logged on company is an India company. And it is optional.
            /// User can use this field to specify different MRP values on different sites and warehouses for the same item. And when the trade agreement applies to a transaction,
            /// the MRP value should flow to the MRP field of the transaction as the default value.
            /// So current change is when fetching the superset of trade agreements which could apply to all of these items and customer for the given date,
            /// also takes field MAXIMUMRETAILPRICE_IN through the stored procedures GETALLDISCOUNTTRADEAGREEMENTS/ GETALLTRADEAGREEMENTS/ GETTRADEAGREEMENTS.
            /// Then return the whole struct PriceResult  rather than PriceResult.Price.
            /// </summary>
            /// <param name="tradeAgreementRules">The trade agreement rules.</param>
            /// <param name="priceParameters">The price parameters.</param>
            /// <param name="currencyCode">The currency code.</param>
            /// <param name="itemId">The item Id.</param>
            /// <param name="defaultSalesUnit">The default sales unit.</param>
            /// <param name="salesUnit">The sales unit.</param>
            /// <param name="variantLine">The variant line.</param>
            /// <param name="unitOfMeasureConversion">The UnitOfMeasure Conversion.</param>
            /// <param name="quantity">The quantity.</param>
            /// <param name="customerId">The customer Id.</param>
            /// <param name="customerPriceGroup">The customer price group.</param>
            /// <param name="channelPriceGroupIds">The channel price group Ids.</param>
            /// <param name="salesLines">Optional sales lines.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="activeDate">The active date.</param>
            /// <returns>The PriceResult of active trade agreement.</returns>
            internal static PriceResult GetPriceResultOfActiveTradeAgreement(
                IDictionary<string, IList<TradeAgreement>> tradeAgreementRules,
                DiscountParameters priceParameters,
                string currencyCode,
                string itemId,
                string defaultSalesUnit,
                string salesUnit,
                ProductVariant variantLine,
                UnitOfMeasureConversion unitOfMeasureConversion,
                decimal quantity,
                string customerId,
                string customerPriceGroup,
                IEnumerable<string> channelPriceGroupIds,
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                DateTimeOffset activeDate)
            {
                PriceResult result;
                variantLine = variantLine ?? new ProductVariant();
    
                // Get basic arguments for Price evaluation
                RetailPriceArgs args = new RetailPriceArgs()
                {
                    Barcode = string.Empty,
                    CurrencyCode = currencyCode,
                    CustomerId = customerId,
                    Dimensions = variantLine,
                    DefaultSalesUnitOfMeasure = defaultSalesUnit,
                    ItemId = itemId,
                    PriceGroups = channelPriceGroupIds.AsReadOnly(),
                    Quantity = quantity,
                    SalesUOM = salesUnit,
                    UnitOfMeasureConversion = unitOfMeasureConversion,
                };
    
                // Get the active retail price - checks following prices brackets in order: Customer TAs, Store price group TAs, 'All' TAs.
                // First bracket to return a price 'wins'. Each bracket returns the lowest price it can find.
                result = FindPriceAgreement(tradeAgreementRules, priceParameters, args, salesLines, priceContext, activeDate);
    
                // Direct customer TA price would have been caught above.
                // Compare against customer price group TAs now and override if lower than previously found price (or if previously found price was 0).
                if (!string.IsNullOrEmpty(customerId)
                    && !string.IsNullOrEmpty(customerPriceGroup)
                    && !channelPriceGroupIds.Contains(customerPriceGroup))
                {
                    // Customer price group
                    args.PriceGroups = new ReadOnlyCollection<string>(new[] { customerPriceGroup });
                    PriceResult customerResult = FindPriceAgreement(tradeAgreementRules, priceParameters, args, salesLines, priceContext, activeDate);
    
                    // Pick the Customer price if either the Retail price is ZERO, or the Customer Price is non-zero AND lower
                    if ((result.Price == decimal.Zero)
                        || ((customerResult.Price > decimal.Zero) && (customerResult.Price <= result.Price)))
                    {
                        result = customerResult;
                    }
                }
    
                return result;
            }
    
            private static Tuple<decimal, string> GetActiveTradeAgreementPriceAndGroup(
                IDictionary<string, IList<TradeAgreement>> tradeAgreementRules,
                DiscountParameters priceParameters,
                string currencyCode,
                string itemId,
                string defaultSalesUnit,
                string salesUnit,
                ProductVariant variantLine,
                UnitOfMeasureConversion unitOfMeasureConversion,
                decimal quantity,
                string customerId,
                string customerPriceGroup,
                IEnumerable<string> channelPriceGroupIds,
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                DateTimeOffset activeDate)
            {
                PriceResult result = GetPriceResultOfActiveTradeAgreement(
                    tradeAgreementRules,
                    priceParameters,
                    currencyCode,
                    itemId,
                    defaultSalesUnit,
                    salesUnit,
                    variantLine,
                    unitOfMeasureConversion,
                    quantity,
                    customerId,
                    customerPriceGroup,
                    channelPriceGroupIds,
                    salesLines,
                    priceContext,
                    activeDate);
    
                return new Tuple<decimal, string>(result.Price, result.CustPriceGroup);
            }
    
            /// <summary>
            /// This function takes arguments (customer, item, currency, etc.) related to price (trade) agreement
            /// as well as the set of currently enabled trade agreement types. It returns the best trade agreement
            /// price for the given constraints.
            /// As in AX, the method searches for a price on the given item which has been given to a
            /// customer, price group, or anyone (in given precedence order). If a price is found and marked as
            /// SearchAgain=False, the search will terminate. Otherwise, search for lowest price will continue.
            /// To recap, the logic is that three searches are done for customer, price group, and all, each bracket
            /// will return the lowest price it has for the constraints. If it has SearchAgain=True, then the search
            /// for lowest price continues to the next bracket.
            /// </summary>
            /// <param name="tradeAgreementRules">Trade agreements applicable to each item, keyed by item relation (i.e. item Id).</param>
            /// <param name="args">Arguments for price agreement search.</param>
            /// <param name="priceParameters">Set of enabled price agreement types.</param>
            /// <param name="salesLines">Sales lines.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="activeDate">Date to use for querying trade agreement rules.</param>
            /// <returns>
            /// Most applicable price for the given price agreement constraints.
            /// </returns>
            private static PriceResult ApplyPriceTradeAgreements(
                IDictionary<string, IList<TradeAgreement>> tradeAgreementRules,
                PriceAgreementArgs args,
                DiscountParameters priceParameters,
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                DateTimeOffset activeDate)
            {
                PriceResult priceResult = new PriceResult(0M, PriceGroupIncludesTax.NotSpecified);
    
                var itemCodes = new PriceDiscountItemCode[] { PriceDiscountItemCode.Item, PriceDiscountItemCode.ItemGroup, PriceDiscountItemCode.AllItems };
                var accountCodes = new PriceDiscountAccountCode[] { PriceDiscountAccountCode.Customer, PriceDiscountAccountCode.CustomerGroup, PriceDiscountAccountCode.AllCustomers };
    
                // Search through combinations of item/account codes from most to least specific.
                // This needs to match the behavior of AX code PriceDisc.findPriceAgreement().
                foreach (var accountCode in accountCodes)
                {
                    foreach (var itemCode in itemCodes)
                    {
                        if (priceParameters.Activation(PriceDiscountType.PriceSales, accountCode, itemCode))
                        {
                            IList<string> accountRelations = args.GetAccountRelations(accountCode);
                            string itemRelation = args.GetItemRelation(itemCode);
    
                            if (accountRelations.All(a => ValidRelation(accountCode, a)) &&
                                ValidRelation(itemCode, itemRelation))
                            {
                                bool searchAgain;
                                IEnumerable<TradeAgreement> tradeAgreements = FindPriceAgreements(tradeAgreementRules, args, itemCode, accountCode, salesLines, priceContext, activeDate);
                                PriceResult currentPriceResult = GetBestPriceAgreement(tradeAgreements, out searchAgain);
    
                                if (priceResult.Price == 0M ||
                                    (currentPriceResult.Price > 0M && currentPriceResult.Price < priceResult.Price))
                                {
                                    priceResult = currentPriceResult;
                                }
    
                                if (!searchAgain)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
    
                return priceResult;
            }
    
            private static IEnumerable<TradeAgreement> FindPriceAgreements(
                IDictionary<string, IList<TradeAgreement>> tradeAgreementRules,
                PriceAgreementArgs args,
                PriceDiscountItemCode itemCode,
                PriceDiscountAccountCode accountCode,
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                DateTimeOffset activeDate)
            {
                string itemRelation = args.GetItemRelation(itemCode);
                IList<string> accountRelations = args.GetAccountRelations(accountCode);
                string unitId = args.GetUnitId(itemCode);
    
                // price trade agreements are always item-specific, so first filter by itemId.
                IList<TradeAgreement> rulesForItem;
                if (!tradeAgreementRules.TryGetValue(itemRelation, out rulesForItem))
                {
                    return new List<TradeAgreement>(0);
                }
    
                List<TradeAgreement> tradeAgreementsOfVariantUnFilteredByQuantity = new List<TradeAgreement>();
                List<TradeAgreement> tradeAgreementsOfMasterOrProduct = new List<TradeAgreement>();
    
                // Get the initial filtered trade agreements, not checking quantity.
                for (int i = 0; i < rulesForItem.Count; i++)
                {
                    var t = rulesForItem[i];
                    if (t.ItemRelation.Equals(itemRelation, StringComparison.OrdinalIgnoreCase)
                        && t.ItemCode == itemCode
                        && t.AccountCode == accountCode
                        && accountRelations.Contains(t.AccountRelation)
                        && t.Currency.Equals(args.CurrencyCode, StringComparison.OrdinalIgnoreCase)
                        && (t.FromDate.DateTime <= activeDate.Date || t.FromDate.DateTime <= NoDate)
                        && (t.ToDate.DateTime >= activeDate.Date || t.ToDate.DateTime <= NoDate)
                        && (string.IsNullOrWhiteSpace(unitId)
                            || t.UnitOfMeasureSymbol.Equals(unitId, StringComparison.OrdinalIgnoreCase))
                        && t.IsVariantMatch(args.Dimensions))
                    {
                        if (t.IsVariant)
                        {
                            tradeAgreementsOfVariantUnFilteredByQuantity.Add(t);
                        }
                        else
                        {
                            if (t.IsQuantityMatch(args.Quantity))
                            {
                                tradeAgreementsOfMasterOrProduct.Add(t);
                            }
                        }
                    }
                }
    
                // For variants
                if (args.IsVariant)
                {
                    List<TradeAgreement> tradeAgreementsOfVariant = new List<TradeAgreement>();
                    List<TradeAgreement> tradeAgreementsOfVariantExactMatch = new List<TradeAgreement>();
    
                    foreach (TradeAgreement t in tradeAgreementsOfVariantUnFilteredByQuantity)
                    {
                        if (t.IsVariant)
                        {
                            decimal aggregatedQuantityByAgreementVariant = decimal.Zero;
                            foreach (SalesLine salesLine in salesLines)
                            {
                                if (string.Equals(args.ItemId, salesLine.ItemId, StringComparison.OrdinalIgnoreCase) && t.IsVariantMatch(salesLine.Variant))
                                {
                                    aggregatedQuantityByAgreementVariant += salesLine.Quantity;
                                }
                            }
    
                            if (aggregatedQuantityByAgreementVariant == decimal.Zero)
                            {
                                aggregatedQuantityByAgreementVariant = 1m;
                            }
    
                            if (t.IsQuantityMatch(aggregatedQuantityByAgreementVariant))
                            {
                                if (t.IsVariantExactMatch(args.Dimensions))
                                {
                                    tradeAgreementsOfVariantExactMatch.Add(t);
                                }
    
                                tradeAgreementsOfVariant.Add(t);
                            }
                        }
                    }
    
                    // 1. Return exact matches if any
                    if (tradeAgreementsOfVariantExactMatch != null && tradeAgreementsOfVariantExactMatch.Any())
                    {
                        if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                        {
                            RetainTopPriorityTradeAgreements(tradeAgreementsOfVariantExactMatch, priceContext);
                        }
    
                        tradeAgreementsOfVariantExactMatch.Sort(AgreementSortMethod);
                        return tradeAgreementsOfVariantExactMatch;
                    }
    
                    // 2. Return (partial) variant matches if any.
                    if (tradeAgreementsOfVariant.Count > 0)
                    {
                        if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                        {
                            RetainTopPriorityTradeAgreements(tradeAgreementsOfVariant, priceContext);
                        }
    
                        TradeAgreementComparer tradeAgreementComparator = new TradeAgreementComparer(tradeAgreementsOfVariant, args.Dimensions);
                        tradeAgreementComparator.SortTradeAgreement();
                        return tradeAgreementsOfVariant;
                    }
                }
    
                // 3. Return non-variant matches.
                if (accountCode == PriceDiscountAccountCode.CustomerGroup)
                {
                    RetainTopPriorityTradeAgreements(tradeAgreementsOfMasterOrProduct, priceContext);
                }
    
                tradeAgreementsOfMasterOrProduct.Sort(AgreementSortMethod);
    
                return tradeAgreementsOfMasterOrProduct;
            }
    
            /// <summary>
            /// Sort the agreements in ascending order by QuantityAmountFrom then RecordId then FromDate.
            /// </summary>
            /// <param name="left">Left side of the comparison.</param>
            /// <param name="right">Right side of the comparison.</param>
            /// <returns>Returns -1 if left is less, 0 if equal, 1 if left is more.</returns>
            private static int AgreementSortMethod(TradeAgreement left, TradeAgreement right)
            {
                if (left.QuantityAmountFrom < right.QuantityAmountFrom)
                {
                    return -1;
                }
                else if (left.QuantityAmountFrom > right.QuantityAmountFrom)
                {
                    return 1;
                }
                else
                {
                    if (left.RecordId < right.RecordId)
                    {
                        return -1;
                    }
                    else if (left.RecordId > right.RecordId)
                    {
                        return 1;
                    }
                    else
                    {
                        if (left.FromDate < right.FromDate)
                        {
                            return -1;
                        }
                        else if (left.FromDate > right.FromDate)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
    
            /// <summary>
            /// This function searches a list of trade agreements (assumed to be sorted with lowest prices first).
            ///   It calculates the price for each trade agreement, returning the lowest amount, and optionally stopping
            ///   early if it encounters a trade agreement with SearchAgain=False.
            /// </summary>
            /// <param name="tradeAgreements">List of price agreements, sorted by Amount ascending.</param>
            /// <param name="searchAgain">Out parameter indicating whether SearchAgain=False was hit.</param>
            /// <returns>Best price agreement price for the given list of trade agreements.</returns>
            private static PriceResult GetBestPriceAgreement(IEnumerable<TradeAgreement> tradeAgreements, out bool searchAgain)
            {
                decimal price = 0;
                decimal maxRetailPrice = 0M;
                string custPriceGroup = null;
                searchAgain = true;
                foreach (var ta in tradeAgreements)
                {
                    decimal priceUnit = (ta.PriceUnit != 0) ? ta.PriceUnit : 1;
                    decimal markup = ta.ShouldIncludeMarkup ? ta.MarkupAmount : 0;
                    decimal currentPrice = (ta.Amount / priceUnit) + markup;
    
                    if ((price == 0M) || (currentPrice != 0M && price > currentPrice))
                    {
                        price = currentPrice;
                        maxRetailPrice = ta.MaximumRetailPriceIndia;
                        if (ta.AccountCode == PriceDiscountAccountCode.CustomerGroup)
                        {
                            custPriceGroup = ta.AccountRelation;
                        }
                    }
    
                    if (!ta.ShouldSearchAgain)
                    {
                        searchAgain = false;
                        break;
                    }
                }
    
                return new PriceResult(price, PriceGroupIncludesTax.NotSpecified, maxRetailPrice, custPriceGroup);
            }
    
            /// <summary>
            /// True if there is a valid relation between the item code and relation.
            /// </summary>
            /// <param name="itemCode">The item code to validate against (item/item group/all).</param>
            /// <param name="relation">The relation to validate.</param>
            /// <returns>True if the relation is compatible with the item code.</returns>
            private static bool ValidRelation(PriceDiscountItemCode itemCode, string relation)
            {
                bool ok = true;
    
                if (!string.IsNullOrEmpty(relation) && (itemCode == PriceDiscountItemCode.AllItems))
                {
                    ok = false;
                }
    
                if (string.IsNullOrEmpty(relation) && (itemCode != PriceDiscountItemCode.AllItems))
                {
                    ok = false;
                }
    
                return ok;
            }
    
            /// <summary>
            /// True if there is a valid relation between the account code and relation.
            /// </summary>
            /// <param name="accountCode">The customer account code to validate against (customer/customer group/all).</param>
            /// <param name="relation">The relation to validate.</param>
            /// <returns>True if the relation is compatible with the account code.</returns>
            private static bool ValidRelation(PriceDiscountAccountCode accountCode, string relation)
            {
                bool ok = true;
    
                if (!string.IsNullOrEmpty(relation) && (accountCode == PriceDiscountAccountCode.AllCustomers))
                {
                    ok = false;
                }
    
                if (string.IsNullOrEmpty(relation) && (accountCode != PriceDiscountAccountCode.AllCustomers))
                {
                    ok = false;
                }
    
                return ok;
            }
    
            private static TradeAgreementPriceLine ConstructTradeAgreementPriceLine(Tuple<decimal, string> priceCustPriceGroupPair)
            {
                return new TradeAgreementPriceLine
                {
                    PriceMethod = PriceMethod.Fixed,
                    Value = priceCustPriceGroupPair.Item1,
                    OriginId = string.Empty,
                    CustPriceGroup = priceCustPriceGroupPair.Item2
                };
            }
    
            private static Tuple<decimal, string> CalculateAgreementPriceLine(
                IEnumerable<SalesLine> salesLines,
                SalesLine salesLine,
                PriceContext priceContext,
                Dictionary<string, IList<TradeAgreement>> agreementsByItemId,
                DiscountParameters discountParameters,
                Dictionary<string, decimal> itemQuantites)
            {
                var quantity = salesLine.Quantity;
    
                // count all occurrences for this item if this is a transaction
                if (priceContext.PriceCalculationMode == PricingCalculationMode.Transaction)
                {
                    itemQuantites.TryGetValue(salesLine.ItemId, out quantity);
    
                    if (quantity == decimal.Zero)
                    {
                        quantity = 1m;
                    }
                }
    
                var activeDate = (salesLine.SalesDate != null) ? salesLine.SalesDate.Value : priceContext.ActiveDate;
    
                return GetActiveTradeAgreementPriceAndGroup(
                    agreementsByItemId,
                    discountParameters,
                    priceContext.CurrencyCode,
                    salesLine.ItemId,
                    salesLine.OriginalSalesOrderUnitOfMeasure,
                    Discount.GetUnitOfMeasure(salesLine),
                    salesLine.Variant,
                    salesLine.UnitOfMeasureConversion,
                    quantity,
                    priceContext.CustomerAccount,
                    priceContext.CustomerPriceGroup,
                    PriceContextHelper.GetApplicablePriceGroupsForPrice(priceContext, salesLine.CatalogIds),
                    salesLines,
                    priceContext,
                    activeDate);
            }
    
            private static Dictionary<string, IList<TradeAgreement>> IndexAgreementsByItemId(ReadOnlyCollection<TradeAgreement> tradeAgreements)
            {
                var agreementsByItemId = new Dictionary<string, IList<TradeAgreement>>(StringComparer.OrdinalIgnoreCase);
                foreach (var ta in tradeAgreements)
                {
                    var itemId = ta.ItemRelation;
    
                    if (!agreementsByItemId.ContainsKey(itemId))
                    {
                        agreementsByItemId.Add(itemId, new List<TradeAgreement>());
                    }
    
                    IList<TradeAgreement> agreements;
                    if (!agreementsByItemId.TryGetValue(itemId, out agreements))
                    {
                        agreements = new TradeAgreement[0];
                    }
    
                    agreements.Add(ta);
                }
    
                return agreementsByItemId;
            }
    
            private static PriceResult FindPriceAgreement(
                IDictionary<string, IList<TradeAgreement>> tradeAgreementRules,
                DiscountParameters priceParameters,
                RetailPriceArgs args,
                IEnumerable<SalesLine> salesLines,
                PriceContext priceContext,
                DateTimeOffset activeDate)
            {
                // First we get the price according to the base UOM
                PriceAgreementArgs p = args.AgreementArgsForDefaultSales();
                PriceResult result = ApplyPriceTradeAgreements(tradeAgreementRules, p, priceParameters, salesLines, priceContext, activeDate);
    
                // Is the current UOM something different than the base UOM?
                if (args.SalesUOM != args.DefaultSalesUnitOfMeasure)
                {
                    // Then let's see if we find some price agreement for that UOM
                    p = args.ArgreementArgsForSale();
                    PriceResult salesUOMResult = ApplyPriceTradeAgreements(tradeAgreementRules, p, priceParameters, salesLines, priceContext, activeDate);
    
                    // If there is a price found then we return that as the price
                    if (salesUOMResult.Price > decimal.Zero)
                    {
                        return salesUOMResult;
                    }
                    else
                    {
                        return new PriceResult(result.Price * args.UnitOfMeasureConversion.GetFactorForQuantity(args.Quantity), result.IncludesTax, custPriceGroup: result.CustPriceGroup);
                    }
                }
    
                // else we return baseUOM price mulitplied with the unit qty factor.
                return result;
            }
    
            private static Dictionary<string, decimal> GetItemQuantities(IEnumerable<SalesLine> salesLines)
            {
                // ItemId => Quantity lookup
                Dictionary<string, decimal> itemQuantites = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                foreach (SalesLine saleLine in salesLines)
                {
                    decimal existingQuantity = decimal.Zero;
                    if (!saleLine.IsVoided)
                    {
                        itemQuantites.TryGetValue(saleLine.ItemId, out existingQuantity);
    
                        itemQuantites[saleLine.ItemId] = existingQuantity + saleLine.Quantity;
                    }
                }
    
                return itemQuantites;
            }
    
            private static void RetainTopPriorityTradeAgreements(
                List<TradeAgreement> agreements,
                PriceContext priceContext)
            {
                if (agreements != null && agreements.Count > 1)
                {
                    // Makes sure it's all customer group trade agreements.
                    if (!agreements.Where(p => p.AccountCode != PriceDiscountAccountCode.CustomerGroup || string.IsNullOrEmpty(p.AccountRelation)).Any())
                    {
                        int priority = 0;
    
                        int highestPriority = agreements.Max(p => priceContext.PriceGroupIdToPriorityDictionary.TryGetValue(p.AccountRelation, out priority) ? priority : 0);
    
                        agreements.RemoveAll(p => priceContext.PriceGroupIdToPriorityDictionary.TryGetValue(p.AccountRelation, out priority) && priority < highestPriority);
                    }
                }
            }
    
            /// <summary>
            /// This class sorts the trade agreement based on sorting method used.
            /// </summary>
            private class TradeAgreementComparer : IComparer<TradeAgreement>
            {
                private readonly List<TradeAgreement> tradeAgreements;
                private readonly ProductVariant variant;
                private readonly Dictionary<long, int> tradeAgreementsRank;
    
                internal TradeAgreementComparer(List<TradeAgreement> tradeAgreements, ProductVariant productVariant)
                {
                    this.tradeAgreements = tradeAgreements;
                    this.variant = productVariant;
                    this.tradeAgreementsRank = new Dictionary<long, int>();
                }
    
                /// <summary>
                /// Sort the agreements in ascending order by rank then QuantityAmountFrom then RecordId then FromDate.
                /// Higher rank refers to more specific variant.
                /// </summary>
                /// <param name="left">Left side of the comparison.</param>
                /// <param name="right">Right side of the comparison.</param>
                /// <returns>Returns -1 if left is less, 0 if equal, 1 if left is more.</returns>
                public int Compare(TradeAgreement left, TradeAgreement right)
                {
                    int leftRank = this.CalculateRank(left, this.variant);
                    int rightRank = this.CalculateRank(right, this.variant);
    
                    if (leftRank > rightRank)
                    {
                        return -1;
                    }
                    else if (leftRank < rightRank)
                    {
                        return 1;
                    }
    
                    return AgreementSortMethod(left, right);
                }
    
                /// <summary>
                /// Sort trade agreements.
                /// </summary>
                internal void SortTradeAgreement()
                {
                    this.tradeAgreements.Sort(this.Compare);
                }
    
                /// <summary>
                /// Calculates the rank of trade agreement based on matching dimensions.
                /// </summary>
                /// <param name="tradeAgreement">Trade agreement.</param>
                /// <param name="variant">Product variant.</param>
                /// <returns>The rank.</returns>
                private int CalculateRank(TradeAgreement tradeAgreement, ProductVariant variant)
                {
                    int rank = 0;
    
                    if (variant == null)
                    {
                        return rank;
                    }
    
                    if (this.tradeAgreementsRank.ContainsKey(tradeAgreement.RecordId))
                    {
                        return this.tradeAgreementsRank[tradeAgreement.RecordId];
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(tradeAgreement.ConfigId) && tradeAgreement.ConfigId.Equals(variant.ConfigId, StringComparison.OrdinalIgnoreCase))
                        {
                            rank++;
                        }
    
                        if (!string.IsNullOrWhiteSpace(tradeAgreement.ColorId) && tradeAgreement.ColorId.Equals(variant.ColorId, StringComparison.OrdinalIgnoreCase))
                        {
                            rank++;
                        }
    
                        if (!string.IsNullOrWhiteSpace(tradeAgreement.SizeId) && tradeAgreement.SizeId.Equals(variant.SizeId, StringComparison.OrdinalIgnoreCase))
                        {
                            rank++;
                        }
    
                        if (!string.IsNullOrWhiteSpace(tradeAgreement.StyleId) && tradeAgreement.StyleId.Equals(variant.StyleId, StringComparison.OrdinalIgnoreCase))
                        {
                            rank++;
                        }
    
                        this.tradeAgreementsRank.Add(tradeAgreement.RecordId, rank);
                    }
    
                    return rank;
                }
            }
        }
    }
}
