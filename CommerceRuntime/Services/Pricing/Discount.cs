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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using PE = Commerce.Runtime.Services.PricingEngine;

        /// <summary>
        /// Encapsulates the business logic for calculating discounts.
        /// </summary>
        internal sealed class Discount
        {
            private readonly PricingDataServiceManager pricingDataManager;

            /// <summary>
            /// Initializes a new instance of the <see cref="Discount"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            private Discount(RequestContext context)
            {
                this.pricingDataManager = new PricingDataServiceManager(context);
            }

            /// <summary>
            /// Creates the discount using the specified context.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The discount.</returns>
            public static Discount Create(RequestContext context)
            {
                return new Discount(context);
            }

            /// <summary>
            /// Gets all of the periodic discounts for the items in the transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="discountTime">The applicable discounts for this time stamp in channel date/time.</param>
            public void GetAllPeriodicDiscounts(RequestContext context, SalesTransaction transaction, DateTimeOffset discountTime)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                var channelConfiguration = context.GetChannelConfiguration();

                PE.PricingEngine.GetAllPeriodicDiscountsForLines(
                    this.pricingDataManager,
                    new ChannelCurrencyOperations(context),
                    transaction,
                    channelConfiguration.Currency,
                    discountTime,
                    false);
            }

            /// <summary>
            /// Calculates all of the discounts for the transactions.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            /// <param name="discountCalculationMode">Discount calculation mode.</param>
            public void CalculateDiscount(RequestContext context, SalesTransaction transaction, DiscountCalculationMode discountCalculationMode)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                // take a snapshot of discount amount coming from previous computations, if there is any
                Dictionary<string, decimal> previousLineDiscounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                if (transaction.PriceCalculableSalesLines.Any())
                {
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    foreach (var line in transaction.PriceCalculableSalesLines)
                    {
                        // if it is zero, means this is the first time we are calculating, there is no previous state
                        if (line.DiscountAmount != 0)
                        {
                            previousLineDiscounts.Add(line.LineId, line.DiscountAmount);
                        }
                    }
                }

                var channelConfiguration = context.GetChannelConfiguration();

                var customer = GetCustomer(context, transaction.CustomerId);

                PE.PricingEngine.CalculateDiscountsForLines(
                    this.pricingDataManager,
                    transaction,
                    new ChannelCurrencyOperations(context),
                    channelConfiguration.Currency,
                    customer.LineDiscountGroup,
                    customer.MultilineDiscountGroup,
                    customer.TotalDiscountGroup,
                    false,
                    discountCalculationMode,
                    context.GetNowInChannelTimeZone());

                // check whether any discount discrepancy occurred after calculation
                if (transaction.PriceCalculableSalesLines.Any())
                {
                    // Consider calculable lines only. Ignore voided or return-by-receipt lines.
                    foreach (var line in transaction.PriceCalculableSalesLines)
                    {
                        // calculate on a clone so that we don't modify the actual sales line
                        var clonedLine = line.Clone<SalesLine>();
                        SalesTransactionTotaler.CalculateLine(context, transaction, clonedLine);

                        Discount.RaiseNotificationIfDiscountWasInvalidated(context, previousLineDiscounts, clonedLine);
                    }
                }

                transaction.IsDiscountFullyCalculated = discountCalculationMode.HasFlag(DiscountCalculationMode.CalculateAll);
            }

            /// <summary>
            /// Calculates price and discount independently for each item.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transaction">The sales transaction.</param>
            public void CalculateIndependentDiscount(RequestContext context, SalesTransaction transaction)
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException("transaction");
                }

                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();

                Customer customer = GetCustomer(context, transaction.CustomerId);

                PE.PricingEngine.CalculateDiscountsForLines(
                    this.pricingDataManager,
                    transaction,
                    new ChannelCurrencyOperations(context),
                    channelConfiguration.Currency,
                    customer.LineDiscountGroup,
                    customer.MultilineDiscountGroup,
                    customer.TotalDiscountGroup,
                    true,
                    DiscountCalculationMode.CalculateOffer,
                    context.GetNowInChannelTimeZone());
            }

            /// <summary>
            /// Gets the discount codes with offer id, or keyword, or get all if none specified.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The discount codes.</returns>
            public GetDiscountCodesServiceResponse GetDiscountCodes(GetDiscountCodesServiceRequest request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (request.RequestContext == null)
                {
                    throw new ArgumentException("request.RequestContext is not set", "request");
                }

                IEnumerable<DiscountCode> discountCodes =
                    this.pricingDataManager.GetDiscountCodes(request.OfferId, request.DiscountCode, request.Keyword, request.ActiveDate);

                return new GetDiscountCodesServiceResponse(discountCodes.AsPagedResult());
            }

            internal static void ClearDiscountLinesOfType(SalesLine salesLine, DiscountLineType lineType)
            {
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
            }

            internal static Customer GetCustomer(RequestContext context, string customerAccount)
            {
                Customer customer = null;
                if (!string.IsNullOrWhiteSpace(customerAccount))
                {
                    var getCustomerDataRequest = new GetCustomerDataRequest(customerAccount);
                    SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest);
                    customer = getCustomerDataResponse.Entity;
                }

                return customer ?? (new Customer());
            }

            /// <summary>
            /// Verifies whether a previous discount is found and different from current discount, causing a <see cref="DiscountInvalidatedNotification"/>.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="previousLineDiscounts">The dictionary of previous line discounts.</param>
            /// <param name="clonedLine">The cloned sales line.</param>
            private static void RaiseNotificationIfDiscountWasInvalidated(
                RequestContext context,
                Dictionary<string, decimal> previousLineDiscounts,
                SalesLine clonedLine)
            {
                decimal oldDiscountValue = 0;

                if (previousLineDiscounts.TryGetValue(clonedLine.LineId, out oldDiscountValue)
                    && oldDiscountValue != 0
                    && oldDiscountValue != clonedLine.DiscountAmount)
                {
                    // raise Discount mismatch notification
                    DiscountInvalidatedNotification notification = new DiscountInvalidatedNotification(clonedLine, oldDiscountValue);
                    context.Notify(notification);
                }
            }
        }
    }
}
