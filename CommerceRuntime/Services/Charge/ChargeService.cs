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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// This service implements logic for calculating auto-charges, price charges,
        /// and shipping charges for transactions.
        /// </summary>
        public class ChargeService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetChargesServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Executes the specified request.
            /// </summary>
            /// <param name="request">The request object.</param>
            /// <returns>The response object.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetChargesServiceRequest))
                {
                    response = CalculateCharges((GetChargesServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Given a delivery mode code, retrieve the charge group assigned to that delivery mode.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="deliveryMode">The delivery mode code.</param>
            /// <returns>
            /// The delivery mode charge group identifier.
            /// </returns>
            private static string GetDeliveryModeGroupFromCode(RequestContext context, string deliveryMode)
            {
                DeliveryOption deliveryOption;
                string deliveryModeGroup = string.Empty;
    
                if (!string.IsNullOrWhiteSpace(deliveryMode))
                {
                    var deliveryOptionDataRequest = new GetDeliveryOptionDataRequest(deliveryMode, new QueryResultSettings(new ColumnSet("CODE", "MARKUPGROUP"), PagingInfo.AllRecords));
                    var deliveryOptionDataResponse = context.Execute<EntityDataServiceResponse<DeliveryOption>>(deliveryOptionDataRequest);
                    deliveryOption = deliveryOptionDataResponse.PagedEntityCollection.FirstOrDefault();
    
                    if (deliveryOption != null && deliveryOption.ChargeGroup != null)
                    {
                        deliveryModeGroup = deliveryOption.ChargeGroup;
                    }
                }
    
                return deliveryModeGroup;
            }
    
            /// <summary>
            /// Calculates the price charges.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The miscellaneous charge line.
            /// </returns>
            private static Microsoft.Dynamics.Commerce.Runtime.DataModel.ChargeLine CalculatePriceCharges(SalesLine salesLine, GetChargesServiceRequest request)
            {
                // hook up with caching when it is in.
                var getItemsRequest = new GetItemsDataRequest(new string[] { salesLine.ItemId });
                getItemsRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet("ItemId", "AllocateMarkup", "PriceQty", "Markup"), PagingInfo.AllRecords);
                var getItemsResponse = request.RequestContext.Execute<GetItemsDataResponse>(getItemsRequest);
    
                Item itemDetails = getItemsResponse.Items.SingleOrDefault();
    
                if (itemDetails != null && itemDetails.Markup != 0)
                {
                    // there is a price charge associated with the item.
                    decimal amount;
    
                    // check type price charge
                    if (itemDetails.AllocateMarkup)
                    {
                        // Per Unit markup
                        if (!itemDetails.PriceQuantity.HasValue || itemDetails.PriceQuantity.Value == 0M)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange,
                                string.Format("Price quantity is set to 0 for item {0}", itemDetails.ItemId));
                        }
    
                        amount = itemDetails.Markup / itemDetails.PriceQuantity.Value;
                    }
                    else
                    {
                        // per line markup
                        amount = itemDetails.Markup;
                    }
    
                    // there is a price charge associated with this item. need to update the price accordingly.
                    var priceCharge = new Microsoft.Dynamics.Commerce.Runtime.DataModel.ChargeLine
                    {
                        CalculatedAmount = amount,
                        ChargeCode = string.Empty,
                        ItemTaxGroupId = salesLine.ItemTaxGroupId,
                        SalesTaxGroupId = salesLine.SalesTaxGroupId,
                        ChargeType = ChargeType.PriceCharge,
                        ModuleType = ChargeModule.Sales,
                    };
    
                    return priceCharge;
                }
    
                return null;
            }
    
            /// <summary>
            /// If any of the charge lines aren't expressed in the channel currency,
            ///     convert their calculated amount to the channel currency.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="chargeLines">The charge lines which will have amounts calculated.</param>
            /// <param name="channelCurrency">The currency of the channel, compared against charge line to convert.</param>
            private static void ChargeAmountsToStoreCurrency(RequestContext context, IEnumerable<ChargeLine> chargeLines, string channelCurrency)
            {
                foreach (var chargeLine in chargeLines)
                {
                    // if charge and channel don't have same currency, convert to channel currency
                    if (!chargeLine.CurrencyCode.Equals(channelCurrency, StringComparison.OrdinalIgnoreCase))
                    {
                        var currencyRequest = new GetCurrencyValueServiceRequest(chargeLine.CurrencyCode, channelCurrency, chargeLine.CalculatedAmount);
    
                        var currencyResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyRequest);
                        chargeLine.CalculatedAmount = currencyResponse.ConvertedAmount;
                    }
                }
            }
    
            /// <summary>
            /// Given all relevant transaction/line info, this method builds a charge configuration header
            /// containing the appropriate relations to be consumed by the ChargeDataManager to find
            /// charge configurations in the database.
            /// </summary>
            /// <param name="accountType">The customer relation type on the header.</param>
            /// <param name="itemType">The item relation type on the header.</param>
            /// <param name="deliveryType">The delivery relation type on the header.</param>
            /// <param name="customerId">The account number of the customer on the header.</param>
            /// <param name="customerGroup">The customer charge account group on the header.</param>
            /// <param name="itemId">The item id.</param>
            /// <param name="itemGroup">The item charge group id.</param>
            /// <param name="deliveryMode">The delivery mode code.</param>
            /// <param name="deliveryModeGroup">The delivery mode charge group id.</param>
            /// <returns>
            /// Charge configuration header built from the parameters to use to query for configurations.
            /// </returns>
            private static ChargeConfigurationHeader BuildConfigurationHeader(
                ChargeAccountType accountType,
                ChargeItemType itemType,
                ChargeDeliveryType deliveryType,
                string customerId,
                string customerGroup,
                string itemId,
                string itemGroup,
                string deliveryMode,
                string deliveryModeGroup)
            {
                // extract appropriate account relation string
                string accountRelation = string.Empty;
                if (accountType == ChargeAccountType.Customer)
                {
                    accountRelation = customerId;
                }
                else if (accountType == ChargeAccountType.CustomerGroup)
                {
                    accountRelation = customerGroup;
                }
    
                // extract appropriate item relation string
                string itemRelation = string.Empty;
                if (itemType == ChargeItemType.Item)
                {
                    itemRelation = itemId;
                }
                else if (itemType == ChargeItemType.ItemGroup)
                {
                    itemRelation = itemGroup;
                }
    
                // extract appropriate delivery mode relation string
                string deliveryRelation = string.Empty;
                if (deliveryType == ChargeDeliveryType.DeliveryMode)
                {
                    deliveryRelation = deliveryMode;
                }
                else if (deliveryType == ChargeDeliveryType.DeliveryModeGroup)
                {
                    deliveryRelation = deliveryModeGroup;
                }
    
                var header = new ChargeConfigurationHeader
                {
                    AccountType = accountType,
                    AccountRelation = accountRelation ?? string.Empty,
                    ItemType = itemType,
                    ItemRelation = itemRelation ?? string.Empty,
                    DeliveryType = deliveryType,
                    DeliveryRelation = deliveryRelation ?? string.Empty,
                };
    
                return header;
            }
    
            /// <summary>
            /// This method will find all auto-charge configurations which match the combinations
            /// and info given in the ChargeProcessorArguments parameters.
            /// Then it will apply the charge calculation rules and return a collection of
            /// ChargeLines which should be applied to the transaction or line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">Transaction we will populate with charges.</param>
            /// <param name="args">Defines which combinations to look for, whether header/line charges, and transaction/line info.</param>
            /// <returns>The collection of charge lines.</returns>
            private static IEnumerable<ChargeLine> ApplyAutoCharges(RequestContext context, SalesTransaction transaction, ChargeProcessorArguments args)
            {
                // for all combinations to try, search for any matches in the data base
                var chargeConfigurations = new List<ChargeConfiguration>();
                foreach (var combo in args.CombinationsToTry)
                {
                    var header = BuildConfigurationHeader(
                        combo.Item1,
                        combo.Item2,
                        combo.Item3,
                        args.CustomerId,
                        args.CustomerChargeGroup,
                        args.ItemId,
                        args.ItemChargeGroup,
                        args.DeliveryModeId,
                        args.DeliveryModeChargeGroup);
    
                    GetChargeConfigurationsByHeaderDataRequest getChargeConfigurationsByHeaderDataRequest = new GetChargeConfigurationsByHeaderDataRequest(args.ChargeType, header, QueryResultSettings.AllRecords);
                    var configs = context.Execute<EntityDataServiceResponse<ChargeConfiguration>>(getChargeConfigurationsByHeaderDataRequest).PagedEntityCollection;
                    chargeConfigurations.AddRange(GetValidChargeConfigurations(configs.Results));
                }
    
                // try to apply all the charge configurations found above
                var orderAmount = transaction.NetAmountWithNoTax;
                var appliedCharges = ApplyChargeConfigurations(chargeConfigurations, orderAmount);
    
                return appliedCharges;
            }
    
            /// <summary>
            /// This filters out any unusable charge configurations in case they are in the data.
            /// </summary>
            /// <param name="configs">Configurations to filter for validity.</param>
            /// <returns>The valid subset of the given configurations.</returns>
            private static IEnumerable<ChargeConfiguration> GetValidChargeConfigurations(IEnumerable<ChargeConfiguration> configs)
            {
                foreach (var config in configs)
                {
                    // customer or customer charge group specific charges must specify a customer or customer charge group
                    if ((config.AccountCode == ChargeAccountType.Customer && string.IsNullOrWhiteSpace(config.AccountRelation)) ||
                        (config.AccountCode == ChargeAccountType.CustomerGroup && string.IsNullOrWhiteSpace(config.AccountRelation)))
                    {
                        continue;
                    }
    
                    yield return config;
                }
            }
    
            /// <summary>
            /// Returns a list of 3-tuples of all possible account, item, and delivery types which can apply to line-level auto charges.
            /// </summary>
            /// <returns>
            /// The collection of applicable line-level auto charges.
            /// </returns>
            private static IEnumerable<Tuple<ChargeAccountType, ChargeItemType, ChargeDeliveryType>> GetAllLineChargeCombinations()
            {
                var allCombinations = CartesianProduct(
                        new ChargeAccountType[] { ChargeAccountType.Customer, ChargeAccountType.CustomerGroup, ChargeAccountType.All },
                        new ChargeItemType[] { ChargeItemType.Item, ChargeItemType.ItemGroup, ChargeItemType.All },
                        new ChargeDeliveryType[] { ChargeDeliveryType.None })
                    .Concat(CartesianProduct(
                        new ChargeAccountType[] { ChargeAccountType.Customer, ChargeAccountType.CustomerGroup, ChargeAccountType.All },
                        new ChargeItemType[] { ChargeItemType.None },
                        new ChargeDeliveryType[] { ChargeDeliveryType.DeliveryMode, ChargeDeliveryType.DeliveryModeGroup, ChargeDeliveryType.All }));
    
                return allCombinations;
            }
    
            /// <summary>
            /// Returns a list of 3-tuples of all possible account, item, and delivery types which can apply to transaction-level auto charges.
            /// </summary>
            /// <returns>
            /// The collection of applicable transaction-level auto charges.
            /// </returns>
            private static IEnumerable<Tuple<ChargeAccountType, ChargeItemType, ChargeDeliveryType>> GetAllHeaderChargeCombinations()
            {
                var allCombinations = CartesianProduct(
                        new ChargeAccountType[] { ChargeAccountType.Customer, ChargeAccountType.CustomerGroup, ChargeAccountType.All },
                        new ChargeItemType[] { ChargeItemType.All },
                        new ChargeDeliveryType[] { ChargeDeliveryType.None })
                    .Concat(CartesianProduct(
                        new ChargeAccountType[] { ChargeAccountType.Customer, ChargeAccountType.CustomerGroup, ChargeAccountType.All },
                        new ChargeItemType[] { ChargeItemType.None },
                        new ChargeDeliveryType[] { ChargeDeliveryType.DeliveryMode, ChargeDeliveryType.DeliveryModeGroup, ChargeDeliveryType.All }));
    
                return allCombinations;
            }
    
            /// <summary>
            /// Given charge configurations and transaction/item info, this method will create charge lines
            ///     based on the charge configurations and transaction info passed into it. These auto charge lines
            ///     will not have any final calculated amounts on them.
            /// </summary>
            /// <param name="configurations">All charge configurations to attempt to compute.</param>
            /// <param name="transactionAmount">Total order amount (minus tax). Used for Header charges.</param>
            /// <returns>
            /// Collection of ChargeLines created from the charge configurations.
            /// </returns>
            private static IEnumerable<ChargeLine> ApplyChargeConfigurations(IEnumerable<ChargeConfiguration> configurations, decimal transactionAmount)
            {
                var appliedCharges = new List<ChargeLine>();
    
                foreach (var config in configurations)
                {
                    if (config.ChargeMethod == ChargeMethod.Fixed)
                    {
                        if (config.ChargeLevel == ChargeLevel.Line ||
                            (config.ChargeLevel == ChargeLevel.Header &&
                             config.FromAmount <= transactionAmount &&
                             (transactionAmount < config.ToAmount || 0 == config.ToAmount)))
                        {
                            appliedCharges.Add(new ChargeLine
                                {
                                    Value = config.Value,
                                    ChargeCode = config.ChargeCode,
                                    ModuleType = config.ConfigurationModule,
                                    CurrencyCode = config.CurrencyCode,
                                    ItemTaxGroupId = config.ItemTaxGroup,
                                    SalesTaxGroupId = config.SalesTaxGroup,
                                    ChargeType = ChargeType.AutoCharge,
                                    ChargeMethod = ChargeMethod.Fixed,
                                });
                        }
                    }
                    else if (config.ChargeMethod == ChargeMethod.Pieces && config.ChargeLevel == ChargeLevel.Line)
                    {
                        appliedCharges.Add(new ChargeLine
                            {
                                Value = config.Value,
                                ChargeCode = config.ChargeCode,
                                ModuleType = config.ConfigurationModule,
                                CurrencyCode = config.CurrencyCode,
                                ItemTaxGroupId = config.ItemTaxGroup,
                                SalesTaxGroupId = config.SalesTaxGroup,
                                ChargeType = ChargeType.AutoCharge,
                                ChargeMethod = ChargeMethod.Pieces,
                            });
                    }
                    else if (config.ChargeMethod == ChargeMethod.Percent)
                    {
                        appliedCharges.Add(new ChargeLine
                            {
                                Value = config.Value,
                                ChargeCode = config.ChargeCode,
                                ModuleType = config.ConfigurationModule,
                                CurrencyCode = config.CurrencyCode,
                                ItemTaxGroupId = config.ItemTaxGroup,
                                SalesTaxGroupId = config.SalesTaxGroup,
                                ChargeType = ChargeType.AutoCharge,
                                ChargeMethod = ChargeMethod.Percent,
                            });
                    }
                    else if (config.ChargeMethod == ChargeMethod.External)
                    {
                        appliedCharges.Add(new ChargeLine
                            {
                                Value = config.Value,
                                ChargeCode = config.ChargeCode,
                                ModuleType = config.ConfigurationModule,
                                CurrencyCode = config.CurrencyCode,
                                ItemTaxGroupId = config.ItemTaxGroup,
                                SalesTaxGroupId = config.SalesTaxGroup,
                                ChargeType = ChargeType.AutoCharge,
                                ChargeMethod = ChargeMethod.External,
                            });
                    }
                }
    
                return appliedCharges;
            }
    
            /// <summary>
            /// This will take a transaction with existing auto charge lines and calculate their
            /// actual amount based on the values and methods defined on them.
            /// </summary>
            /// <param name="context">The current request context with access to the shipping service.</param>
            /// <param name="salesTransaction">The sales transaction which will have its charge amounts calculated.</param>
            private static void CalculateAutoChargeAmounts(RequestContext context, SalesTransaction salesTransaction)
            {
                CalculateTransactionAutoChargeAmounts(context, salesTransaction);
                CalculateLineAutoChargeAmounts(context, salesTransaction);
            }
    
            /// <summary>
            /// This will take a transaction with existing transaction-level auto charge lines and calculate their
            /// actual amount based on the values and methods defined on them.
            /// </summary>
            /// <param name="context">The current request context with access to the shipping service.</param>
            /// <param name="salesTransaction">The sales transaction which will have its charge amounts calculated.</param>
            private static void CalculateTransactionAutoChargeAmounts(RequestContext context, SalesTransaction salesTransaction)
            {
                // fetch shipping rates up front for all lines in the transaction if the transaction has an external charge
                var shippingRates = new Dictionary<string, decimal>();
                if (salesTransaction.ChargeLines.Any(cl => cl.ChargeMethod == ChargeMethod.External))
                {
                    // Consider calculable lines only. Ignore voided and return lines.
                    shippingRates = CalculateExternalCharge(context, salesTransaction.ChargeCalculableSalesLines);
                }
    
                // calculate charge amounts for auto charges on the transaction
                foreach (var charge in salesTransaction.ChargeLines.Where(cl => cl.ChargeType == ChargeType.AutoCharge))
                {
                    if (charge.ChargeMethod == ChargeMethod.Fixed)
                    {
                        // fixed charges are just a constant amount
                        charge.CalculatedAmount = charge.Value;
                    }
                    else if (charge.ChargeMethod == ChargeMethod.Percent)
                    {
                        // percent charges are a specified percentage of the transaction's net amount
                        decimal orderTotal = salesTransaction.NetAmountWithNoTax;
                        charge.CalculatedAmount = (charge.Value / 100M) * orderTotal;
                    }
                    else if (charge.ChargeMethod == ChargeMethod.External)
                    {
                        // sum all the shipping rates for lines on the transaction
                        decimal rate = 0M;
                        foreach (var key in shippingRates.Keys)
                        {
                            rate += shippingRates[key];
                        }
    
                        // external shipping charges are a specified percentage markup over the externally calculated shipping rate
                        charge.CalculatedAmount = rate + (rate * charge.Value / 100M);
                    }
                }
            }
    
            /// <summary>
            /// This will take a transaction with existing line-level auto charge lines and calculate their
            /// actual amount based on the values and methods defined on them.
            /// </summary>
            /// <param name="context">The current request context with access to the shipping service.</param>
            /// <param name="salesTransaction">The sales transaction which will have its charge amounts calculated.</param>
            private static void CalculateLineAutoChargeAmounts(RequestContext context, SalesTransaction salesTransaction)
            {
                // fetch shipping rates up front for all lines in the transaction with external charges
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                var linesWithExternalCharge = salesTransaction.ChargeCalculableSalesLines.Where(sl => sl.ChargeLines.Any(cl => cl.ChargeMethod == ChargeMethod.External));
                var shippingRates = CalculateExternalCharge(context, linesWithExternalCharge);
    
                // for each sales line, calculate the actual charge amount for the auto charges on the line
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var salesLine in salesTransaction.ChargeCalculableSalesLines)
                {
                    foreach (var charge in salesLine.ChargeLines.Where(cl => cl.ChargeType == ChargeType.AutoCharge))
                    {
                        if (charge.ChargeMethod == ChargeMethod.Fixed)
                        {
                            // fixed charges are just a constant amount
                            charge.CalculatedAmount = charge.Value;
                        }
                        else if (charge.ChargeMethod == ChargeMethod.Pieces)
                        {
                            // unit charges are a fixed amount per unit sold
                            charge.CalculatedAmount = charge.Value * salesLine.Quantity;
                        }
                        else if (charge.ChargeMethod == ChargeMethod.Percent)
                        {
                            // percent charges are a specified percentage of the sales line's net amount
                            charge.CalculatedAmount = (charge.Value / 100M) * salesLine.NetAmountWithNoTax();
                        }
                        else if (charge.ChargeMethod == ChargeMethod.External)
                        {
                            // try to get the rate from the pre-fetched shipping rates
                            decimal rate;
                            if (shippingRates.TryGetValue(salesLine.LineId, out rate))
                            {
                                // external shipping charges are a specified percentage markup over the externally calculated shipping rate
                                charge.CalculatedAmount = rate + (rate * charge.Value / 100M);
                            }
                            else
                            {
                                NetTracer.Warning(
                                    "Charge::CalculateAutoChargeLineAmounts(): Sales line '{0}' with external shipping method doesn't have a shipping rate", salesLine.LineId);
                            }
                        }
                    }
                }
            }
    
            /// <summary>
            /// This method will encapsulate the call out to shipping service to get external charges.
            /// </summary>
            /// <param name="context">The request context passed into the service.</param>
            /// <param name="salesLines">Set of sales lines which have external charges. Given to shipping service as a unit to find shipping prices.</param>
            /// <returns>Amounts of external charges by line Id. Empty if no charges found.</returns>
            private static Dictionary<string, decimal> CalculateExternalCharge(RequestContext context, IEnumerable<SalesLine> salesLines)
            {
                // Shipping service will return a collection of salesline ids along with their corresponding shipping rates.
                var shippingRates = new Dictionary<string, decimal>();
    
                // return immediately if no sales lines
                if (!salesLines.Any())
                {
                    return shippingRates;
                }
    
                // call shipping service with given sales lines
                var request = new GetExternalShippingRateServiceRequest(salesLines);
                var response = context.Execute<GetExternalShippingRateServiceResponse>(request);
                if (response != null)
                {
                    foreach (var salesLineShippingRate in response.SalesLineShippingRates.Results)
                    {
                        var lineId = salesLineShippingRate.SalesLineId;
                        shippingRates[lineId] = salesLineShippingRate.ShippingCharge;
                    }
    
                    return shippingRates;
                }
                else
                {
                    NetTracer.Warning("Charge::CalculateExternalCharge(): Received null response from shipping service, using 0 as external charge");
                    foreach (var line in salesLines)
                    {
                        shippingRates[line.LineId] = 0M;
                    }
    
                    return shippingRates;
                }
            }
    
            /// <summary>
            /// Returns a cartesian product or "cross-join" of the three collections passed in.
            /// Used to find all possible combinations of given collections.
            /// </summary>
            /// <typeparam name="T">Type of the first collection.</typeparam>
            /// <typeparam name="U">Type of the second collection.</typeparam>
            /// <typeparam name="V">Type of the third collection.</typeparam>
            /// <param name="first">First collection of elements for the product.</param>
            /// <param name="second">Second collection of elements for the product.</param>
            /// <param name="third">Third collection of elements for the product.</param>
            /// <returns>
            /// Collection of 3-tuples containing all possible combinations.
            /// </returns>
            private static IEnumerable<Tuple<T, U, V>> CartesianProduct<T, U, V>(IEnumerable<T> first, IEnumerable<U> second, IEnumerable<V> third)
            {
                var product = from f in first
                              from s in second
                              from t in third
                              select new Tuple<T, U, V>(f, s, t);
    
                return product;
            }
    
            /// <summary>
            /// Clears all the non-manual charges on the transaction and on the lines.
            /// </summary>
            /// <param name="transaction">The sales transaction.</param>
            private static void ClearNonManualCharges(SalesTransaction transaction)
            {
                ClearNonManualCharges(transaction.ChargeLines);
    
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (SalesLine salesLine in transaction.ChargeCalculableSalesLines)
                {
                    ClearNonManualCharges(salesLine.ChargeLines);
                }
            }
    
            /// <summary>
            /// Clears all the non-manual charges.
            /// </summary>
            /// <param name="chargeLines">The charge lines collection.</param>
            private static void ClearNonManualCharges(ICollection<ChargeLine> chargeLines)
            {
                var manualCharges = chargeLines.Where(c => c.ChargeType == ChargeType.ManualCharge).ToArray();
                chargeLines.Clear();
                chargeLines.AddRange(manualCharges);
            }
    
            /// <summary>
            /// High-level calculation method to get charges from the calculation logic
            /// and attach them to the transaction and it's lines.
            /// </summary>
            /// <param name="request">The request containing context and transaction to update.</param>
            /// <returns>
            /// Response with updated transaction.
            /// </returns>
            private static GetChargesServiceResponse CalculateCharges(GetChargesServiceRequest request)
            {
                // extract transaction we'll be populating
                var transaction = request.Transaction;
    
                if (request.RequestContext.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                    request.RequestContext.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore)
                {
                    // clear all the non-manual charges
                    ClearNonManualCharges(transaction);
                }
    
                // total transaction so that we have order totals and line net amounts for percentage charges and tiered fixed charges
                SalesTransactionTotaler.CalculateTotals(request.RequestContext, transaction);
    
                // put charges on the transaction lines
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var salesLine in transaction.ChargeCalculableSalesLines)
                {
                    var priceChargesPerLine = CalculatePriceCharges(salesLine, request);
                    if (priceChargesPerLine != null)
                    {
                        salesLine.ChargeLines.Add(priceChargesPerLine);
                    }
                }
    
                // Auto-Charges are only supported for Online stores.
                if (request.RequestContext.GetChannelConfiguration().ChannelType == RetailChannelType.OnlineStore ||
                    request.RequestContext.GetChannelConfiguration().ChannelType == RetailChannelType.SharePointOnlineStore)
                {
                    CalculateAutoCharges(request, transaction);
                }
    
                return new GetChargesServiceResponse(transaction);
            }
    
            /// <summary>
            /// Calculates the auto charges for the transaction / sales lines.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="transaction">Current sales transaction.</param>
            private static void CalculateAutoCharges(GetChargesServiceRequest request, SalesTransaction transaction)
            {
                // get customer to see their markup group
                string customerAccount = transaction.CustomerId ?? string.Empty;
                string customerChargeGroup = string.Empty;
                if (!string.IsNullOrWhiteSpace(customerAccount))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(customerAccount);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                    var customer = getCustomerDataResponse.Entity;
                    customerChargeGroup = (customer != null) ? customer.ChargeGroup : string.Empty;
                }
    
                // get delivery mode information off the transaction
                string deliveryMode = transaction.DeliveryMode ?? string.Empty;
                string deliveryModeGroup = GetDeliveryModeGroupFromCode(request.RequestContext, deliveryMode);
    
                var channelConfiguration = request.RequestContext.GetChannelConfiguration();
                string currencyCode = (channelConfiguration != null) ? channelConfiguration.Currency : string.Empty;
    
                // put charges on the transaction
                var transactionCharges = CalculateTransactionCharges(request.RequestContext, customerAccount, customerChargeGroup, deliveryMode, deliveryModeGroup, transaction);
    
                foreach (var charge in transactionCharges)
                {
                    transaction.ChargeLines.Add(charge);
                }
    
                // put charges on the transaction lines
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                foreach (var salesLine in transaction.ChargeCalculableSalesLines)
                {
                    // get delivery mode information off the sales line
                    deliveryMode = string.IsNullOrEmpty(salesLine.DeliveryMode) ? (transaction.DeliveryMode ?? string.Empty) : salesLine.DeliveryMode;
                    deliveryModeGroup = GetDeliveryModeGroupFromCode(request.RequestContext, deliveryMode);
    
                    var lineCharges = CalculateLineCharges(request.RequestContext, customerAccount, customerChargeGroup, deliveryMode, deliveryModeGroup, salesLine, transaction);
    
                    foreach (var charge in lineCharges)
                    {
                        salesLine.ChargeLines.Add(charge);
                    }
                }
    
                // now that all auto charges are on the transaction, calculate their amounts
                CalculateAutoChargeAmounts(request.RequestContext, transaction);
    
                // convert any charge amounts not in the channel currency
                var transactionAutoChargeLines = transaction.ChargeLines.Where(cl => cl.ChargeType == ChargeType.AutoCharge);
    
                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                var lineAutoChargeLines = transaction.ChargeCalculableSalesLines.SelectMany(sl => sl.ChargeLines).Where(cl => cl.ChargeType == ChargeType.AutoCharge);
    
                var allAutoChargeLines = transactionAutoChargeLines.Concat(lineAutoChargeLines);
    
                ChargeAmountsToStoreCurrency(request.RequestContext, allAutoChargeLines, currencyCode);
            }
    
            /// <summary>
            /// Given all relevant transaction/line info, this will calculate the charges
            /// which should be put on the given sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="customerId">The customer account number to use for searching.</param>
            /// <param name="customerGroup">The customer charge group id to use for searching.</param>
            /// <param name="deliveryMode">The delivery mode code to use for searching.</param>
            /// <param name="deliveryModeGroup">The delivery mode charge group id to use for searching.</param>
            /// <param name="line">The sales line that would get the charge. Null if not applying to sales line.</param>
            /// <param name="transaction">The sales transaction that will have this charge.</param>
            /// <returns>Collection of charges which apply to this line.</returns>
            private static IEnumerable<ChargeLine> CalculateLineCharges(RequestContext context, string customerId, string customerGroup, string deliveryMode, string deliveryModeGroup, SalesLine line, SalesTransaction transaction)
            {
                GetSalesParametersDataRequest getSalesParametersDataRequest = new GetSalesParametersDataRequest(QueryResultSettings.SingleRecord);
                var salesParameters = context.Execute<SingleEntityDataServiceResponse<SalesParameters>>(getSalesParametersDataRequest).Entity;
    
                // return empty if we're not calculating line charges
                if (!salesParameters.UseLineCharges)
                {
                    return new Collection<ChargeLine>();
                }
    
                var getItemsRequest = new GetItemsDataRequest(new string[] { line.ItemId });
                getItemsRequest.QueryResultSettings = new QueryResultSettings(new ColumnSet("ITEMID", "MARKUPGROUPID"), PagingInfo.AllRecords);
                var getItemsResponse = context.Execute<GetItemsDataResponse>(getItemsRequest);
    
                Item item = getItemsResponse.Items.SingleOrDefault();
                if (item == null)
                {
                    return new Collection<ChargeLine>();
                }
    
                // generate all applicable combinations of account, item, and delivery type for line auto-charges
                //   we'll iterate through these below to create the header filters for finding auto-charge configurations
                var allCombinations = GetAllLineChargeCombinations();
                var processorArgs = new ChargeProcessorArguments
                {
                    CombinationsToTry = allCombinations,
                    ChargeType = ChargeLevel.Line,
                    CustomerId = customerId,
                    CustomerChargeGroup = customerGroup,
                    ItemId = item.ItemId,
                    ItemChargeGroup = item.ChargeGroup,
                    DeliveryModeId = deliveryMode,
                    DeliveryModeChargeGroup = deliveryModeGroup,
                };
    
                return ApplyAutoCharges(context, transaction, processorArgs);
            }
    
            /// <summary>
            /// Given all relevant transaction info, this method will calculate the charges
            /// which should be put on the given transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="customerId">The customer account number to search for transaction charges by.</param>
            /// <param name="customerGroup">The customer charge group id to search for transaction charges by.</param>
            /// <param name="deliveryMode">The delivery mode code to search transaction charges by.</param>
            /// <param name="deliveryModeGroup">The delivery mode charge group id to search transactions charges by.</param>
            /// <param name="transaction">The transaction which will potentially have charges.</param>
            /// <returns>Collection of charges which apply to this transaction.</returns>
            private static IEnumerable<ChargeLine> CalculateTransactionCharges(RequestContext context, string customerId, string customerGroup, string deliveryMode, string deliveryModeGroup, SalesTransaction transaction)
            {
                GetSalesParametersDataRequest getSalesParametersDataRequest = new GetSalesParametersDataRequest(QueryResultSettings.SingleRecord);
                var salesParameters = context.Execute<SingleEntityDataServiceResponse<SalesParameters>>(getSalesParametersDataRequest).Entity;
    
                // return empty if we're not calculating transaction charges
                if (!salesParameters.UseHeaderCharges)
                {
                    return new Collection<ChargeLine>();
                }
    
                // generate all applicable combinations of account, item, and delivery type for header auto-charges
                //   we'll iterate through these to create the header filters for finding auto-charge configurations
                var allCombinations = GetAllHeaderChargeCombinations();
    
                var processorArgs = new ChargeProcessorArguments
                {
                    CombinationsToTry = allCombinations,
                    ChargeType = ChargeLevel.Header,
                    CustomerId = customerId,
                    CustomerChargeGroup = customerGroup,
                    ItemId = string.Empty,
                    ItemChargeGroup = string.Empty,
                    DeliveryModeId = deliveryMode,
                    DeliveryModeChargeGroup = deliveryModeGroup,
                };
    
                return ApplyAutoCharges(context, transaction, processorArgs);
            }
    
            /// <summary>
            /// This class encapsulates the input to ApplyAutoCharges function.
            /// It contains all combinations of charge header types we want to try to apply.
            /// It uses the rest of its info to build the proper queries to search for and apply the charges
            /// which match the given combinations.
            /// </summary>
            private class ChargeProcessorArguments
            {
                /// <summary>
                /// Gets or sets the combinations to try.
                /// </summary>
                /// <value>
                /// The combinations to try.
                /// </value>
                public IEnumerable<Tuple<ChargeAccountType, ChargeItemType, ChargeDeliveryType>> CombinationsToTry { get; set; }
    
                /// <summary>
                /// Gets or sets the type of the charge.
                /// </summary>
                /// <value>
                /// The type of the charge.
                /// </value>
                public ChargeLevel ChargeType { get; set; }
    
                /// <summary>
                /// Gets or sets the customer id.
                /// </summary>
                /// <value>
                /// The customer id.
                /// </value>
                public string CustomerId { get; set; }
    
                /// <summary>
                /// Gets or sets the customer charge group.
                /// </summary>
                /// <value>
                /// The customer charge group.
                /// </value>
                public string CustomerChargeGroup { get; set; }
    
                /// <summary>
                /// Gets or sets the item id.
                /// </summary>
                /// <value>
                /// The item id.
                /// </value>
                public string ItemId { get; set; }
    
                /// <summary>
                /// Gets or sets the item charge group.
                /// </summary>
                /// <value>
                /// The item charge group.
                /// </value>
                public string ItemChargeGroup { get; set; }
    
                /// <summary>
                /// Gets or sets the delivery mode id.
                /// </summary>
                /// <value>
                /// The delivery mode id.
                /// </value>
                public string DeliveryModeId { get; set; }
    
                /// <summary>
                /// Gets or sets the delivery mode charge group.
                /// </summary>
                /// <value>
                /// The delivery mode charge group.
                /// </value>
                public string DeliveryModeChargeGroup { get; set; }
            }
        }
    }
}
