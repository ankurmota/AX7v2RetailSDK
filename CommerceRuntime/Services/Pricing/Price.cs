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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using PE = Commerce.Runtime.Services.PricingEngine;

        /// <summary>
        /// Encapsulates the business logic for calculating prices.
        /// </summary>
        internal sealed class Price
        {
            private readonly PriceParameters priceParameters;

            /// <summary>
            /// Initializes a new instance of the <see cref="Price"/> class.
            /// </summary>
            /// <param name="salesParameters">The sales parameters.</param>
            private Price(PriceParameters salesParameters)
            {
                this.priceParameters = salesParameters;
            }

            /// <summary>
            /// Creates the specified sales parameters.
            /// </summary>
            /// <param name="salesParameters">The discount parameters.</param>
            /// <returns>The price.</returns>
            public static Price Create(PriceParameters salesParameters)
            {
                return new Price(salesParameters);
            }

            /// <summary>
            /// Puts prices on the given sales lines according to the PriceContext.
            /// </summary>
            /// <param name="pricingRequest">The request context.</param>
            /// <returns>Response containing updated lines and original context.</returns>
            public static GetPricesServiceResponse GetLinePrices(GetPricesServiceRequest pricingRequest)
            {
                PE.IPricingDataAccessor pricingDataManager = new PricingDataServiceManager(pricingRequest.RequestContext);

                PE.PriceContext priceContext = PE.PriceContextHelper.CreatePriceContext(
                    pricingRequest.RequestContext,
                    pricingDataManager,
                    new ChannelCurrencyOperations(pricingRequest.RequestContext),
                    PE.PriceContextHelper.GetItemIds(pricingRequest.SalesLines),
                    PE.PriceContextHelper.GetCatalogIds(pricingRequest.SalesLines),
                    pricingRequest.AffiliationLoyaltyTiers,
                    pricingRequest.ActiveDate,
                    pricingRequest.CustomerAccount,
                    pricingRequest.CustomerPriceGroup,
                    pricingRequest.PriceCalculationMode,
                    DiscountCalculationMode.None);

                PE.PricingEngine.CalculatePricesForSalesLines(pricingRequest.SalesLines, priceContext, pricingDataManager);

                return new GetPricesServiceResponse(pricingRequest.SalesLines.AsPagedResult(), priceContext.CurrencyCode);
            }

            /// <summary>
            /// Get independent prices.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="transaction">The transaction.</param>
            public void GetIndependentPrices(RequestContext context, SalesTransaction transaction)
            {
                PricingDataServiceManager pricingDataManager = new PricingDataServiceManager(context);
                DateTimeOffset today = context.GetNowInChannelTimeZone();
                string customerPriceGroup = GetCustomerPriceGroup(context, transaction.CustomerId);
                PE.PriceContext priceContext = PE.PriceContextHelper.CreatePriceContext(context, pricingDataManager, transaction, this.priceParameters, new ChannelCurrencyOperations(context), today, transaction.CustomerId, customerPriceGroup, transaction.IsTaxIncludedInPrice, PricingCalculationMode.Independent, DiscountCalculationMode.None);
                PE.PricingEngine.CalculatePricesForSalesLines(transaction.PriceCalculableSalesLines, priceContext, pricingDataManager);
            }

            /// <summary>
            /// Updates all prices.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="dateToCheck">The date to check.</param>
            public void UpdateAllPrices(RequestContext context, SalesTransaction transaction, DateTime dateToCheck)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                var linesToCalculate = transaction.PriceCalculableSalesLines.ToList();

                // build price calculation context
                string customerAccount = string.IsNullOrWhiteSpace(transaction.CustomerId) ? string.Empty : transaction.CustomerId;
                string customerPriceGroup = GetCustomerPriceGroup(context, customerAccount);

                PricingDataServiceManager pricingDataManager = new PricingDataServiceManager(context);
                var priceContext = PE.PriceContextHelper.CreatePriceContext(context, pricingDataManager, transaction, this.priceParameters, new ChannelCurrencyOperations(context), dateToCheck, customerAccount, customerPriceGroup, transaction.IsTaxIncludedInPrice, PricingCalculationMode.Transaction, DiscountCalculationMode.None);
                priceContext.IsTaxInclusive = transaction.IsTaxIncludedInPrice;

                // store old line prices
                var oldPrices = linesToCalculate.ToDictionary(l => l.LineId, l => l.Price, StringComparer.OrdinalIgnoreCase);

                // calculate the prices on the transaction
                PE.PricingEngine.CalculatePricesForSalesLines(linesToCalculate, priceContext, pricingDataManager);

                // Check if any prices were changed/invalid
                foreach (var salesLine in linesToCalculate)
                {
                    if (salesLine.PriceInBarcode && !salesLine.IsPriceOverridden)
                    {
                        if (salesLine.Price != 0 && salesLine.Price != salesLine.BarcodeEmbeddedPrice)
                        {
                            decimal quantity = salesLine.BarcodeEmbeddedPrice / salesLine.Price;
                            var roundingRequest = new GetRoundQuantityServiceRequest(quantity, salesLine.UnitOfMeasureSymbol);
                            var response = context.Execute<GetRoundQuantityServiceResponse>(roundingRequest);
                            quantity = response.RoundedValue;

                            salesLine.Quantity = quantity;
                            salesLine.BarcodeCalculatedQuantity = quantity;
                        }
                        else
                        {
                            salesLine.Price = salesLine.BarcodeEmbeddedPrice;
                            salesLine.BarcodeCalculatedQuantity = 1;
                        }
                    }

                    decimal oldPrice = oldPrices[salesLine.LineId];

                    if (salesLine.Price != oldPrice)
                    {
                        salesLine.WasChanged = true;
                    }

                    // if price change, raise Price changed notification
                    RaiseNotificationIfPriceIsChanged(context, oldPrice, salesLine);

                    // if price is invalid, raise Price invalid notification
                    RaiseNotificationIfPriceIsInvalid(context, salesLine);
                }
            }

            /// <summary>
            /// For all the product ids given in the request, calculate the price based on the info
            /// in the product and update the price table in database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// Response containing updated product prices for each given id.
            /// </returns>
            internal GetProductPricesServiceResponse CalculatePricesForProducts(GetProductPricesServiceRequest request)
            {
                // return empty fast if no products are given
                if (!request.Products.Any())
                {
                    return new GetProductPricesServiceResponse(PagedResult<ProductPrice>.Empty());
                }

                IList<ProductPrice> prices = this.CalculateProductPrices(request.RequestContext, request.Products);

                // add materialized updated prices to reponse
                return new GetProductPricesServiceResponse(prices.AsPagedResult());
            }

            private static string GetCustomerPriceGroup(RequestContext context, string customerAccount)
            {
                string customerPriceGroup = string.Empty;

                if (!string.IsNullOrWhiteSpace(customerAccount))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(customerAccount);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                    Customer customer = getCustomerDataResponse.Entity;

                    if (customer != null)
                    {
                        if (!string.IsNullOrWhiteSpace(customer.PriceGroup))
                        {
                            customerPriceGroup = customer.PriceGroup;
                        }
                    }
                }

                return customerPriceGroup;
            }

            /// <summary>
            /// Raises the notification if price is changed.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="oldPrice">The old price.</param>
            /// <param name="saleLineItem">The sale line item.</param>
            private static void RaiseNotificationIfPriceIsChanged(RequestContext context, decimal oldPrice, SalesLine saleLineItem)
            {
                if (oldPrice != 0 && oldPrice != saleLineItem.Price)
                {
                    var notification = new PriceChangedNotification(saleLineItem.ItemId, oldPrice, saleLineItem.Price, saleLineItem.InventoryDimensionId);
                    context.Notify(notification);
                }
            }

            /// <summary>
            /// Raises the notification if price is invalid.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="saleLineItem">The sale line item.</param>
            private static void RaiseNotificationIfPriceIsInvalid(RequestContext context, SalesLine saleLineItem)
            {
                // check for non-positive price values, and raise notification if any exists.
                if (saleLineItem.Price <= 0)
                {
                    var notification = new InvalidPriceNotification(saleLineItem.ItemId, saleLineItem.InventoryDimensionId, saleLineItem.Price);
                    context.Notify(notification);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private IList<ProductPrice> CalculateProductPrices(RequestContext requestContext, IEnumerable<Product> products)
            {
                // use truncated date for comparison and price search
                DateTimeOffset today = requestContext.GetNowInChannelTimeZone();

                // create sales lines for calculation
                var salesLines = new List<SalesLine>();
                foreach (var product in products)
                {
                    SalesLine salesLine = new SalesLine
                    {
                        ItemId = product.ItemId,
                        InventoryDimensionId = string.Empty,
                        LineId = System.Guid.NewGuid().ToString("N"),
                        SalesOrderUnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                        Quantity = 1,
                        ProductId = product.RecordId,
                        SalesDate = today,
                    };

                    salesLines.Add(salesLine);

                    if (product.IsMasterProduct)
                    {
                        foreach (var variant in product.GetVariants())
                        {
                            salesLine = new SalesLine
                            {
                                ItemId = product.ItemId,
                                InventoryDimensionId = variant.InventoryDimensionId,
                                LineId = System.Guid.NewGuid().ToString("N"),
                                SalesOrderUnitOfMeasure = product.Rules.DefaultUnitOfMeasure,
                                Quantity = 1,
                                ProductId = variant.DistinctProductVariantId,
                                SalesDate = today,
                            };

                            salesLines.Add(salesLine);
                        }
                    }
                }

                // set the catalogIds on the sales lines
                var productCatalogAssociationRequest = new GetProductCatalogAssociationsDataRequest(salesLines.Select(p => p.ProductId));
                productCatalogAssociationRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                var productCatalogs = requestContext.Runtime.Execute<GetProductCatalogAssociationsDataResponse>(
                    productCatalogAssociationRequest,
                    requestContext).CatalogAssociations;

                foreach (var sl in salesLines)
                {
                    sl.CatalogIds.UnionWith(productCatalogs.Where(pc => pc.ProductRecordId == sl.ProductId).Select(pc => pc.CatalogRecordId));
                }

                // create price context for calculation
                ISet<string> itemIds = PE.PriceContextHelper.GetItemIds(salesLines);
                var pricingDataManager = new PricingDataServiceManager(requestContext);
                var priceContext = PE.PriceContextHelper.CreatePriceContext(requestContext, pricingDataManager, this.priceParameters, new ChannelCurrencyOperations(requestContext), itemIds, PE.PriceContextHelper.GetCatalogIds(salesLines), today, PricingCalculationMode.Independent, DiscountCalculationMode.None);

                // calculate product prices
                PE.PricingEngine.CalculatePricesForSalesLines(salesLines, priceContext, pricingDataManager);

                IList<ProductPrice> prices = new List<ProductPrice>(salesLines.Count);
                foreach (var salesLine in salesLines)
                {
                    prices.Add(new ProductPrice
                    {
                        ItemId = salesLine.ItemId,
                        InventoryDimensionId = salesLine.InventoryDimensionId,
                        ProductId = salesLine.ProductId,
                        BasePrice = salesLine.BasePrice,
                        AdjustedPrice = salesLine.AdjustedPrice,
                        TradeAgreementPrice = salesLine.AgreementPrice
                    });
                }

                // return updated product prices
                return prices;
            }
        }
    }
}
