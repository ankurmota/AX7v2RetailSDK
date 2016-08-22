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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using PE = Commerce.Runtime.Services.PricingEngine;

        /// <summary>
        /// Encapsulates the implementation of the pricing service.
        /// </summary>
        public class PricingService : IRequestHandler
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
                    typeof(GetPriceServiceRequest),
                    typeof(GetPricesServiceRequest),
                    typeof(UpdatePriceServiceRequest),
                    typeof(CalculateDiscountsServiceRequest),
                    typeof(GetAllPeriodicDiscountsServiceRequest),
                    typeof(GetProductPricesServiceRequest),
                    typeof(GetDiscountCodesServiceRequest),
                    typeof(GetIndependentPriceDiscountServiceRequest)
                };
                }
            }

            /// <summary>
            /// Implements the ExecuteImplementation method for the IPricing service interface.
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
                using (var profiler = new PE.SimpleProfiler(requestType.Name, true, 0))
                {
                    Response response;
                    if (requestType == typeof(GetPriceServiceRequest))
                    {
                        response = GetPrice((GetPriceServiceRequest)request);
                    }
                    else if (requestType == typeof(UpdatePriceServiceRequest))
                    {
                        response = UpdateAllPrices((UpdatePriceServiceRequest)request);
                    }
                    else if (requestType == typeof(CalculateDiscountsServiceRequest))
                    {
                        response = CalculateDiscount((CalculateDiscountsServiceRequest)request);
                    }
                    else if (requestType == typeof(GetAllPeriodicDiscountsServiceRequest))
                    {
                        response = GetAllPeriodicDiscounts((GetAllPeriodicDiscountsServiceRequest)request);
                    }
                    else if (requestType == typeof(GetProductPricesServiceRequest))
                    {
                        response = CalculateProductPrices((GetProductPricesServiceRequest)request);
                    }
                    else if (requestType == typeof(GetPricesServiceRequest))
                    {
                        response = GetLinePrices((GetPricesServiceRequest)request);
                    }
                    else if (requestType == typeof(GetDiscountCodesServiceRequest))
                    {
                        response = GetDiscountCodes((GetDiscountCodesServiceRequest)request);
                    }
                    else if (requestType == typeof(GetIndependentPriceDiscountServiceRequest))
                    {
                        response = CalculateIndependentPriceAndDiscount((GetIndependentPriceDiscountServiceRequest)request);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                    }

                    return response;
                }
            }

            #region wrappers to former IApplication calls

            /// <summary>
            /// Converts amount from one currency to another.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="companyCurrency">The company currency.</param>
            /// <param name="storeCurrency">The store currency.</param>
            /// <param name="amount">The amount.</param>
            /// <returns>Returns the converted amount in the new currency.</returns>
            internal static decimal CurrencyToCurrency(RequestContext context, string companyCurrency, string storeCurrency, decimal amount)
            {
                var currencyRequest = new GetCurrencyValueServiceRequest(companyCurrency, storeCurrency, amount);
                var currencyResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyRequest);

                return currencyResponse.ConvertedAmount;
            }

            /// <summary>
            /// Rounds the specified amount.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="amount">The amount.</param>
            /// <returns>The rounded off amount.</returns>
            internal static decimal RoundWithPricesRounding(RequestContext context, decimal amount)
            {
                string currency = null;
                if (context != null && context.GetChannelConfiguration() != null)
                {
                    currency = context.GetChannelConfiguration().Currency;
                }

                var roundingRequest = new GetRoundedValueServiceRequest(amount, currency, 0, useSalesRounding: false);
                var roundingResponse = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);

                return roundingResponse.RoundedValue;
            }

            #endregion

            /// <summary>
            /// Calculates the product prices.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Response object for product prices request.</returns>
            private static GetProductPricesServiceResponse CalculateProductPrices(GetProductPricesServiceRequest request)
            {
                var service = GetPriceService(request.RequestContext);
                return service.CalculatePricesForProducts(request);
            }

            /// <summary>
            /// Gets the price.
            /// </summary>
            /// <param name="request">The pricing request.</param>
            /// <returns>Response to pricing request.</returns>
            private static GetPriceServiceResponse GetPrice(GetPriceServiceRequest request)
            {
                var channelDateTime = request.RequestContext.GetNowInChannelTimeZone().DateTime;

                var service = GetPriceService(request.RequestContext);
                service.UpdateAllPrices(request.RequestContext, request.Transaction, channelDateTime);

                return new GetPriceServiceResponse(request.Transaction);
            }

            /// <summary>
            /// Updates all prices.
            /// </summary>
            /// <param name="request">The pricing request.</param>
            /// <returns>The response object.</returns>
            private static GetPriceServiceResponse UpdateAllPrices(UpdatePriceServiceRequest request)
            {
                var channelDateTime = request.RequestContext.GetNowInChannelTimeZone().DateTime;

                var service = GetPriceService(request.RequestContext);
                service.UpdateAllPrices(request.RequestContext, request.Transaction, channelDateTime);

                return new GetPriceServiceResponse(request.Transaction);
            }

            private static GetPricesServiceResponse GetLinePrices(GetPricesServiceRequest pricingRequest)
            {
                return Price.GetLinePrices(pricingRequest);
            }

            /// <summary>
            /// Calculates the discount.
            /// </summary>
            /// <param name="request">The pricing request.</param>
            /// <returns>The response object.</returns>
            private static GetPriceServiceResponse CalculateDiscount(CalculateDiscountsServiceRequest request)
            {
                var service = Discount.Create(request.RequestContext);
                service.CalculateDiscount(request.RequestContext, request.Transaction, request.DiscountCalculationMode);

                return new GetPriceServiceResponse(request.Transaction);
            }

            /// <summary>
            /// Calculates price and discount independently for each item.
            /// </summary>
            /// <param name="request">The pricing request.</param>
            /// <returns>The response object.</returns>
            private static GetPriceServiceResponse CalculateIndependentPriceAndDiscount(GetIndependentPriceDiscountServiceRequest request)
            {
                PricingDataServiceManager pricingDataManager = new PricingDataServiceManager(request.RequestContext);
                DateTimeOffset today = request.RequestContext.GetNowInChannelTimeZone();
                Customer customer = Discount.GetCustomer(request.RequestContext, request.Transaction.CustomerId);
                PE.PriceContext priceContext = PE.PriceContextHelper.CreatePriceContext(
                    request.RequestContext,
                    pricingDataManager,
                    request.Transaction,
                    new ChannelCurrencyOperations(request.RequestContext),
                    today,
                    request.Transaction.CustomerId,
                    customer != null ? customer.PriceGroup : string.Empty,
                    customer != null ? customer.LineDiscountGroup : string.Empty,
                    customer != null ? customer.MultilineDiscountGroup : string.Empty,
                    customer != null ? customer.TotalDiscountGroup : string.Empty,
                    request.Transaction.IsTaxIncludedInPrice,
                    PricingCalculationMode.Independent,
                    DiscountCalculationMode.CalculateOffer,
                    request.CalculateForNewSalesLinesOnly,
                    request.NewSalesLineIdSet);

                PE.PricingEngine.CalculatePricesForSalesLines(request.Transaction.PriceCalculableSalesLines, priceContext, pricingDataManager);

                PE.PricingEngine.CalculateDiscountsForLines(pricingDataManager, request.Transaction, true, priceContext);

                return new GetPriceServiceResponse(request.Transaction);
            }

            /// <summary>
            /// Gets all the periodic discounts for the current transaction.
            /// </summary>
            /// <param name="request">The pricing request.</param>
            /// <returns>The response object.</returns>
            private static GetPriceServiceResponse GetAllPeriodicDiscounts(GetAllPeriodicDiscountsServiceRequest request)
            {
                var service = Discount.Create(request.RequestContext);
                service.GetAllPeriodicDiscounts(request.RequestContext, request.Transaction, request.RequestContext.GetNowInChannelTimeZone());
                return new GetPriceServiceResponse(request.Transaction);
            }

            /// <summary>
            /// Gets the discount codes.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response object.</returns>
            private static GetDiscountCodesServiceResponse GetDiscountCodes(GetDiscountCodesServiceRequest request)
            {
                var service = Discount.Create(request.RequestContext);
                return service.GetDiscountCodes(request);
            }

            private static Price GetPriceService(RequestContext context)
            {
                var pricingDataManager = new PricingDataServiceManager(context);
                var salesParameters = pricingDataManager.GetPriceParameters();

                return Price.Create(salesParameters);
            }
        }
    }
}
