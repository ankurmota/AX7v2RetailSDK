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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Contains logic for calculating retail prices.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Should revisit")]
        public static class PricingEngine
        {
            private const string SalesTransactionCollectDiagnosticsFlag = "!@#COLLECTDIAGNOSTICS$%&";
            private const string SalesTransactionDiagnosticsObjectParameter = "!@#DIAGNOSTICSOBJECT$%&";

            /// <summary>
            /// Sets Collect diagnostics flag on sales transaction.
            /// </summary>
            /// <param name="salesTransaction">An instance of <c>SalesTransaction</c>.</param>
            /// <param name="collectDiagnosticsData">Collect if true; false otherwise.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This applied only to SalesTransaction.")]
            public static void SetCollectDiagnostics(
                                SalesTransaction salesTransaction,
                                bool collectDiagnosticsData)
            {   
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException(nameof(salesTransaction));
                }

                salesTransaction.SetProperty(SalesTransactionCollectDiagnosticsFlag, collectDiagnosticsData);
            }

            /// <summary>
            /// Gets Collect diagnostics flag on sales transaction.
            /// </summary>
            /// <param name="salesTransaction">An instance of <c>SalesTransaction</c>.</param>
            /// <returns>true if set; false otherwise.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This applied only to SalesTransaction.")]
            public static bool GetCollectDiagnostics(
                                SalesTransaction salesTransaction)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException(nameof(salesTransaction));
                }

                bool result = false;                
                object resultObj = salesTransaction.GetProperty(SalesTransactionCollectDiagnosticsFlag);

                if (resultObj == null || !resultObj.GetType().Equals(typeof(bool)))
                {
                    result = false;
                }
                else 
                {
                    result = (bool)resultObj;
                }

                return result;
            }

            /// <summary>
            /// Sets pricing engine diagnostics object on sales transaction.
            /// </summary>
            /// <param name="salesTransaction">An instance of <c>SalesTransaction</c>.</param>
            /// <param name="diagnosticsObject">An instance of <c>PricingEngineDiagnosticsObject</c>.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This applied only to SalesTransaction.")]
            public static void SetPricingEngineDiagnosticsObject(
                                SalesTransaction salesTransaction, 
                                PricingEngineDiagnosticsObject diagnosticsObject)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException(nameof(salesTransaction));
                }

                salesTransaction.SetProperty(SalesTransactionDiagnosticsObjectParameter, diagnosticsObject);
            }

            /// <summary>
            /// Gets pricing engine diagnostics object on sales transaction.
            /// </summary>
            /// <param name="salesTransaction">An instance of <c>SalesTransaction</c>.</param>
            /// <returns>An instance of <c>PricingEngineDiagnosticsObject</c>.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This applied only to SalesTransaction.")]
            public static PricingEngineDiagnosticsObject GetPricingEngineDiagnosticsObject(
                                SalesTransaction salesTransaction)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException(nameof(salesTransaction));
                }

                return (PricingEngineDiagnosticsObject)salesTransaction.GetProperty(SalesTransactionDiagnosticsObjectParameter);                
            }

            /// <summary>
            /// This method will calculate the prices for the whole sales transaction.
            /// </summary>
            /// <param name="salesTransaction">Sales transaction.</param>
            /// <param name="pricingDataManager">Provides access to the pricing data to the pricing calculation.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="customerPriceGroup">Customer price group.</param>
            /// <param name="currencyCode">Current code.</param>
            /// <param name="activeDate">Active date time offset for price.</param>
            /// <remarks>Parallel processing has been disabled, but we leave parameter here for backward compatibility.</remarks>
            public static void CalculatePricesForTransaction(
                                    SalesTransaction salesTransaction,
                                    IPricingDataAccessor pricingDataManager,
                                    ICurrencyOperations currencyAndRoundingHelper,
                                    string customerPriceGroup,
                                    string currencyCode,
                                    DateTimeOffset activeDate)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }

                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }

                if (currencyAndRoundingHelper == null)
                {
                    throw new ArgumentNullException("currencyAndRoundingHelper");
                }

                ISet<long> catalogIds = PriceContextHelper.GetCatalogIds(salesTransaction);
                IEnumerable<AffiliationLoyaltyTier> affiliationLoyaltyTiers = PriceContextHelper.GetAffiliationLoyalTierIds(salesTransaction);

                ISet<string> itemIds = PriceContextHelper.GetItemIds(salesTransaction);
                PriceContext priceContext = PriceContextHelper.CreatePriceContext(
                                                pricingDataManager,
                                                currencyAndRoundingHelper,
                                                PricingCalculationMode.Transaction,
                                                DiscountCalculationMode.None,
                                                itemIds,
                                                catalogIds,
                                                affiliationLoyaltyTiers,
                                                salesTransaction.CustomerId,
                                                customerPriceGroup,
                                                salesTransaction.IsTaxIncludedInPrice,
                                                currencyCode,
                                                activeDate);

                bool isDiagnosticsCollected = GetCollectDiagnostics(salesTransaction);

                if (isDiagnosticsCollected)
                {
                    priceContext.IsDiagnosticsCollected = true;
                    priceContext.PricingEngineDiagnosticsObject = new PricingEngineDiagnosticsObject();                                       
                }

                PricingEngine.CalculatePricesForSalesLines(salesTransaction.PriceCalculableSalesLines, priceContext, pricingDataManager);

                if (isDiagnosticsCollected)
                {
                    SetPricingEngineDiagnosticsObject(salesTransaction, priceContext.PricingEngineDiagnosticsObject);
                }
            }

            /// <summary>
            /// This method will calculate the prices for each of the given item lines within the given price context.
            /// </summary>
            /// <param name="salesLines">Item lines which need to have prices calculated.</param>
            /// <param name="priceContext">The configuration of the overall context for the pricing calculation. This includes channel info, currency, customer, etc.</param>
            /// <param name="pricingDataManager">Provides access to the pricing data to the pricing calculation.</param>
            public static void CalculatePricesForSalesLines(IEnumerable<SalesLine> salesLines, PriceContext priceContext, IPricingDataAccessor pricingDataManager)
            {
                if (pricingDataManager == null)
                {
                    throw new ArgumentNullException("pricingDataManager");
                }

                if (priceContext == null)
                {
                    throw new ArgumentNullException("priceContext");
                }

                if (salesLines == null)
                {
                    throw new ArgumentNullException("salesLines");
                }

                IEnumerable<SalesLine> salesLineToCalculate = salesLines;

                if (priceContext.CalculateForNewSalesLinesOnly)
                {
                    salesLineToCalculate = salesLines.Where(p => priceContext.NewSalesLineIdSet.Contains(p.LineId));
                }

                PopulateProductIds(pricingDataManager, priceContext, salesLineToCalculate);

                var calculators = GetPricingCalculators();

                Dictionary<string, IEnumerable<PriceLine>> priceLineDict = new Dictionary<string, IEnumerable<PriceLine>>(StringComparer.OrdinalIgnoreCase);
                foreach (IPricingCalculator calculator in calculators)
                {
                    Dictionary<string, IEnumerable<PriceLine>> oneDict = calculator.CalculatePriceLines(salesLineToCalculate, priceContext, pricingDataManager);

                    foreach (KeyValuePair<string, IEnumerable<PriceLine>> keyValue in oneDict)
                    {
                        IEnumerable<PriceLine> priceLines;

                        if (priceLineDict.TryGetValue(keyValue.Key, out priceLines))
                        {
                            List<PriceLine> newLines = new List<PriceLine>(priceLines);
                            newLines.AddRange(keyValue.Value);
                            priceLineDict[keyValue.Key] = newLines;
                        }
                        else
                        {
                            priceLineDict.Add(keyValue.Key, keyValue.Value);
                        }
                    }
                }

                // for each line, resolve all price lines and set the price
                PriceLineResolver.ResolveAndApplyPriceLines(salesLineToCalculate, priceLineDict, priceContext.CurrencyAndRoundingHelper);
            }

            /// <summary>
            /// Calculates all of the discount lines for the transactions.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="currencyCode">Currency code to filter discounts by.</param>
            /// <param name="lineDiscountGroup">Optional. Line discount group Id for the customer.</param>
            /// <param name="multilineDiscountGroup">Optional. Multiline discount group Id for the customer.</param>
            /// <param name="totalDiscountGroup">Optional. Total discount group Id for the customer.</param>
            /// <param name="shouldTotalLines">True if discount lines should be totaled for each line. False if they should be left as raw discount lines.</param>
            /// <param name="calculationMode">Pricing calculation mode.</param>
            /// <param name="activeDate">Optional. Active, channel date/time to apply discount for.</param>
            /// <remarks>Each sales line will have a collection of DiscountLines and a net discount total in DiscountAmount property (if totaling is enabled).</remarks>
            public static void CalculateDiscountsForLines(
                IPricingDataAccessor pricingDataManager,
                SalesTransaction transaction,
                ICurrencyOperations currencyAndRoundingHelper,
                string currencyCode,
                string lineDiscountGroup,
                string multilineDiscountGroup,
                string totalDiscountGroup,
                bool shouldTotalLines,
                DiscountCalculationMode calculationMode,
                DateTimeOffset activeDate)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                Customer customer = InitializeCustomer(transaction, lineDiscountGroup, multilineDiscountGroup, totalDiscountGroup);
                PriceContext priceContext = Discount.BuildPriceContext(pricingDataManager, currencyAndRoundingHelper, customer, transaction, currencyCode, transaction.IsTaxIncludedInPrice, calculationMode, activeDate);

                bool isDiagnosticsCollected = GetCollectDiagnostics(transaction);

                if (isDiagnosticsCollected)
                {
                    priceContext.IsDiagnosticsCollected = true;
                    priceContext.PricingEngineDiagnosticsObject = new PricingEngineDiagnosticsObject();
                }

                CalculateDiscountsForLines(
                    pricingDataManager,
                    transaction,
                    shouldTotalLines,
                    priceContext);

                if (isDiagnosticsCollected)
                {
                    SetPricingEngineDiagnosticsObject(transaction, priceContext.PricingEngineDiagnosticsObject);
                }
            }

            /// <summary>
            /// Calculates all of the discount lines for the transactions.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="shouldTotalLines">True if discount lines should be totaled for each line. False if they should be left as raw discount lines.</param>
            /// <param name="priceContext">Price context.</param>
            /// <remarks>Each sales line will have a collection of DiscountLines and a net discount total in DiscountAmount property (if totaling is enabled).</remarks>
            public static void CalculateDiscountsForLines(
                IPricingDataAccessor pricingDataManager,
                SalesTransaction transaction,
                bool shouldTotalLines,
                PriceContext priceContext)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                List<SalesLine> existingSalesLines = new List<SalesLine>();
                List<SalesLine> newSalesLines = new List<SalesLine>();

                if (priceContext.CalculateForNewSalesLinesOnly)
                {
                    foreach (SalesLine salesLine in transaction.SalesLines)
                    {
                        if (priceContext.NewSalesLineIdSet.Contains(salesLine.LineId))
                        {
                            newSalesLines.Add(salesLine);
                        }
                        else
                        {
                            existingSalesLines.Add(salesLine);
                        }
                    }

                    // Calculate for new sales lines only.
                    transaction.SalesLines.Clear();
                    transaction.SalesLines.AddRange(newSalesLines);
                }

                Discount discountEngine = InitializeDiscountEngine(pricingDataManager);

                discountEngine.CalculateDiscount(pricingDataManager, transaction, priceContext);

                if (priceContext.CalculateForNewSalesLinesOnly)
                {
                    // Add existing sales lines back after calculating for new sales lines only.
                    List<SalesLine> newSalesLinesFromCalculation = new List<SalesLine>();
                    foreach (SalesLine salesLine in transaction.SalesLines)
                    {
                        if (!priceContext.NewSalesLineIdSet.Contains(salesLine.LineId))
                        {
                            newSalesLinesFromCalculation.Add(salesLine);
                        }
                    }

                    transaction.SalesLines.Clear();
                    transaction.SalesLines.AddRange(existingSalesLines);
                    transaction.SalesLines.AddRange(newSalesLines);
                    transaction.SalesLines.AddRange(newSalesLinesFromCalculation);
                }

                if (shouldTotalLines)
                {
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    foreach (var salesLine in transaction.PriceCalculableSalesLines)
                    {
                        SalesLineTotaller.CalculateLine(transaction, salesLine, d => priceContext.CurrencyAndRoundingHelper.Round(d)); // technically rounding rule should be "sales rounding" rule
                    }
                }
            }

            /// <summary>
            /// Gets all of the periodic discount lines for the items in the transaction.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="currencyAndRoundingHelper">Currency and rounding helper.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="currencyCode">Currency code to filter discounts by.</param>
            /// <param name="activeDate">Active date in channel date time to apply discount for.</param>
            /// <param name="doesPriceIncludeTax">Does the channel have tax-inclusive prices.</param>
            /// <remarks>Each sales line will have a collection of periodic discount lines.</remarks>
            public static void GetAllPeriodicDiscountsForLines(
                IPricingDataAccessor pricingDataManager,
                ICurrencyOperations currencyAndRoundingHelper,
                SalesTransaction transaction,
                string currencyCode,
                DateTimeOffset activeDate,
                bool doesPriceIncludeTax)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                Customer customer = new Customer();
                customer.SetAccountNumber(transaction.CustomerId);
                Discount.GetAllPeriodicDisc(pricingDataManager, currencyAndRoundingHelper, transaction, customer, doesPriceIncludeTax, currencyCode, activeDate);
            }

            internal static PriceResult GetActiveTradeAgreement(IPricingDataAccessor pricingDataManager, DiscountParameters priceParameters, string currencyCode, SalesLine saleItem, decimal quantity, string customerId, string customerPriceGroup, DateTimeOffset dateToCheck)
            {
                dateToCheck = saleItem.SalesDate ?? dateToCheck;

                IEnumerable<PriceGroup> priceGroups = pricingDataManager.GetChannelPriceGroups() as IEnumerable<PriceGroup>;
                HashSet<string> priceGroupIds = new HashSet<string>(priceGroups.Select(pg => pg.GroupId).Distinct(StringComparer.OrdinalIgnoreCase));

                PriceResult result;
                ProductVariant variantLine = GetVariantFromLineOrDatabase(pricingDataManager, saleItem);
                variantLine = variantLine ?? new ProductVariant();

                Tuple<DateTimeOffset, DateTimeOffset> dateRange = GetMinAndMaxActiveDates(new SalesLine[] { saleItem }, dateToCheck);
                ReadOnlyCollection<TradeAgreement> agreements = pricingDataManager.ReadPriceTradeAgreements(
                    new HashSet<string> { saleItem.ItemId },
                    priceGroupIds,
                    customerId,
                    dateRange.Item1,
                    dateRange.Item2,
                    currencyCode,
                    QueryResultSettings.AllRecords) as ReadOnlyCollection<TradeAgreement>;

                var agreementDict = new Dictionary<string, IList<TradeAgreement>>(StringComparer.OrdinalIgnoreCase);
                agreementDict.Add(saleItem.ItemId, agreements);

                result = TradeAgreementCalculator.GetPriceResultOfActiveTradeAgreement(
                    agreementDict,
                    priceParameters,
                    currencyCode,
                    saleItem.ItemId,
                    saleItem.OriginalSalesOrderUnitOfMeasure,
                    Discount.GetUnitOfMeasure(saleItem),
                    variantLine,
                    saleItem.UnitOfMeasureConversion,
                    quantity,
                    customerId,
                    customerPriceGroup,
                    priceGroupIds,
                    new List<SalesLine> { saleItem },
                    new PriceContext(),
                    dateToCheck);

                return result;
            }

            /// <summary>
            /// Gets minimum and maximum dates from set of sales lines or default date/time.
            /// </summary>
            /// <param name="salesLines">Lines to read date range from.</param>
            /// <param name="defaultDate">Date to fall back to if lines are missing dates.</param>
            /// <returns>Truncated min and max date suitable for querying price rules.</returns>
            internal static Tuple<DateTimeOffset, DateTimeOffset> GetMinAndMaxActiveDates(IEnumerable<SalesLine> salesLines, DateTimeOffset defaultDate)
            {
                DateTimeOffset? minDate = null;
                DateTimeOffset? maxDate = null;

                // if we have sales lines, find any min/max if any dates are specified
                if (salesLines != null)
                {
                    foreach (var line in salesLines)
                    {
                        if (line.SalesDate != null)
                        {
                            if (minDate == null || line.SalesDate < minDate)
                            {
                                minDate = line.SalesDate;
                            }

                            if (maxDate == null || line.SalesDate > maxDate)
                            {
                                maxDate = line.SalesDate;
                            }
                        }
                    }
                }

                // default dates if none found
                minDate = minDate ?? defaultDate;
                maxDate = maxDate ?? defaultDate;

                // extend range to contain default date if necessary
                minDate = (minDate.Value.Date < defaultDate.Date) ? minDate : defaultDate;
                maxDate = (maxDate.Value.Date > defaultDate.Date) ? maxDate : defaultDate;

                // return discovered date range, truncated to midnight
                return new Tuple<DateTimeOffset, DateTimeOffset>(minDate.Value, maxDate.Value);
            }

            internal static void PopulateProductIds(IPricingDataAccessor pricingDataManager, PriceContext priceContext, SalesTransaction transaction)
            {
                PopulateProductIds(pricingDataManager, priceContext, transaction.PriceCalculableSalesLines);
            }

            /// <summary>
            /// Builds the set of pricing calculators to use to find price lines.
            /// </summary>
            /// <returns>Set of pricing calculators.</returns>
            private static IEnumerable<IPricingCalculator> GetPricingCalculators()
            {
                var calculators = new List<IPricingCalculator>(3);
                calculators.Add(TradeAgreementCalculator.CreateTradeAgreementCalculator());
                calculators.Add(BasePriceCalculator.CreateBasePriceCalculator());
                calculators.Add(PriceAdjustmentCalculator.CreatePriceAdjustmentCalculator());

                return calculators;
            }

            /// <summary>
            /// Returns variant from sales line (if not null), otherwise, retrieves from the database.
            /// </summary>
            /// <param name="pricingDataManager">Instance of data manager to look up the data.</param>
            /// <param name="salesLine">SalesLine to retrieve Variant from, or fetch from DB if missing.</param>
            /// <returns>
            /// Variant if found. If no variant found or the line doesn't have a variant, returns null.
            /// </returns>
            private static ProductVariant GetVariantFromLineOrDatabase(IPricingDataAccessor pricingDataManager, SalesLine salesLine)
            {
                string itemId = salesLine.ItemId;
                string inventDimId = salesLine.InventoryDimensionId;
                ProductVariant variant = null;

                if (!string.IsNullOrWhiteSpace(inventDimId))
                {
                    if (salesLine.Variant == null || salesLine.Variant.DistinctProductVariantId == 0)
                    {
                        HashSet<ItemVariantInventoryDimension> itemVariantIds = new HashSet<ItemVariantInventoryDimension>();
                        itemVariantIds.Add(new ItemVariantInventoryDimension(itemId, inventDimId));

                        ReadOnlyCollection<ProductVariant> variants = pricingDataManager.GetVariants(itemVariantIds) as ReadOnlyCollection<ProductVariant>;

                        variant = variants.Count > 0 ? variants[0] : new ProductVariant();
                    }
                    else
                    {
                        variant = salesLine.Variant;
                    }
                }

                return variant;
            }

            /// <summary>
            /// For all sales lines on the transaction, retrieve the product rec id if it's not already set.
            /// </summary>
            /// <param name="pricingDataManager">Provides data access to the calculation.</param>
            /// <param name="priceContext">Price context.</param>
            /// <param name="salesLines">Sales lines.</param>
            private static void PopulateProductIds(IPricingDataAccessor pricingDataManager, PriceContext priceContext, IEnumerable<SalesLine> salesLines)
            {
                var itemVariantIds = new HashSet<ItemVariantInventoryDimension>();
                foreach (var line in salesLines)
                {
                    if ((line.Variant == null || line.Variant.DistinctProductVariantId == 0) && !string.IsNullOrWhiteSpace(line.InventoryDimensionId))
                    {
                        var itemVariantId = new ItemVariantInventoryDimension(line.ItemId, line.InventoryDimensionId);
                        itemVariantIds.Add(itemVariantId);
                    }
                }

                // We make a single database call to retrieve all variant identifiers that we need
                // and create a map using the ItemVariantInventoryDimension as its key.
                var variantsMap = new Dictionary<ItemVariantInventoryDimension, ProductVariant>();
                if (itemVariantIds.Any())
                {
                    variantsMap = ((IEnumerable<ProductVariant>)pricingDataManager.GetVariants(itemVariantIds)).ToDictionary(key => new ItemVariantInventoryDimension(key.ItemId, key.InventoryDimensionId));
                }

                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var line in salesLines)
                {
                    if (line.MasterProductId == 0)
                    {
                        Item item = PriceContextHelper.GetItem(priceContext, line.ItemId);

                        line.MasterProductId = (item != null) ? item.Product : 0L;

                        if (item != null && string.IsNullOrWhiteSpace(line.OriginalSalesOrderUnitOfMeasure))
                        {
                            line.OriginalSalesOrderUnitOfMeasure = item.SalesUnitOfMeasure;
                        }
                    }

                    if ((line.Variant == null || line.Variant.DistinctProductVariantId == 0) && !string.IsNullOrWhiteSpace(line.InventoryDimensionId))
                    {
                        ProductVariant variant;
                        var itemVariant = new ItemVariantInventoryDimension(line.ItemId, line.InventoryDimensionId);
                        if (variantsMap.TryGetValue(itemVariant, out variant))
                        {
                            line.Variant = variant;
                        }
                    }

                    if (line.ProductId == 0)
                    {
                        line.ProductId = line.Variant != null ? line.Variant.DistinctProductVariantId : line.MasterProductId;
                    }
                }
            }

            private static Customer InitializeCustomer(SalesTransaction transaction, string lineDiscountGroup, string multilineDiscountGroup, string totalDiscountGroup)
            {
                Customer customer = new Customer();

                customer.SetAccountNumber(transaction.CustomerId);
                customer.LineDiscountGroup = lineDiscountGroup;
                customer.MultilineDiscountGroup = multilineDiscountGroup;
                customer.TotalDiscountGroup = totalDiscountGroup;

                return customer;
            }

            private static Discount InitializeDiscountEngine(IPricingDataAccessor pricingDataManager)
            {
                Discount discountEngine = Discount.Create();
                discountEngine.Initialize(pricingDataManager);

                return discountEngine;
            }
        }
    }
}
