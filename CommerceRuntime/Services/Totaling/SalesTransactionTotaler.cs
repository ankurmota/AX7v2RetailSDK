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
        using System.Globalization;
        using System.Linq;
        using Commerce.Runtime.Services.PricingEngine;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// This class encapsulates all logic for totaling a transaction once all the
        /// prices, discounts, charges, taxes, etc. have been set on it.
        /// </summary>
        internal class SalesTransactionTotaler
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SalesTransactionTotaler"/> class.
            /// </summary>
            /// <param name="salesTransaction">The sales transaction.</param>
            public SalesTransactionTotaler(SalesTransaction salesTransaction)
            {
                this.SalesTransaction = salesTransaction;
            }

            /// <summary>
            /// Gets the transaction which this class is operating on.
            /// </summary>
            public SalesTransaction SalesTransaction { get; private set; }

            /// <summary>
            /// Static entry point to call the totaling logic directly.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void CalculateTotals(RequestContext context, SalesTransaction salesTransaction)
            {
                CalculateAmountDue(context, salesTransaction);
            }

            /// <summary>
            /// Static entry point to calculate amount paid and due.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void CalculateAmountPaidAndDue(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                decimal paymentRequiredAmount;
                salesTransaction.AmountPaid = SalesTransactionTotaler.GetPaymentsSum(salesTransaction.TenderLines);

                // decides what is expected to be paid for this transaction
                switch (salesTransaction.CartType)
                {
                    case CartType.CustomerOrder:
                        paymentRequiredAmount = SalesTransactionTotaler.CalculateRequiredPaymentAmount(context, salesTransaction);
                        break;

                    case CartType.Shopping:
                    case CartType.Checkout:
                    case CartType.AccountDeposit:
                        paymentRequiredAmount = salesTransaction.TotalAmount;
                        break;

                    case CartType.IncomeExpense:
                        paymentRequiredAmount = salesTransaction.IncomeExpenseTotalAmount;
                        break;

                    default:
                        throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                            string.Format("SalesTransactionTotaler::CalculateAmountPaidAndDue: CartType '{0}' not supported.", salesTransaction.CartType));
                }

                salesTransaction.SalesPaymentDifference = paymentRequiredAmount - salesTransaction.AmountPaid;
                salesTransaction.AmountDue = paymentRequiredAmount - salesTransaction.AmountPaid;

                TenderLine lastTenderLine = null;
                if (!salesTransaction.TenderLines.IsNullOrEmpty())
                {
                    lastTenderLine = salesTransaction.ActiveTenderLines.LastOrDefault();
                }

                if (lastTenderLine != null)
                {
                    // Calculate the expected (rounded) amount due for last payment.
                    decimal amountDueBeforeLastPayment = paymentRequiredAmount - salesTransaction.ActiveTenderLines.Take(salesTransaction.ActiveTenderLines.Count - 1).Sum(t => t.Amount);
                    GetPaymentRoundedValueServiceRequest roundAmountDueBeforeLastPaymentRequest = new GetPaymentRoundedValueServiceRequest(amountDueBeforeLastPayment, lastTenderLine.TenderTypeId, isChange: false);
                    GetRoundedValueServiceResponse roundAmountDueBeforeLastPaymentResponse = context.Execute<GetRoundedValueServiceResponse>(roundAmountDueBeforeLastPaymentRequest);

                    // Set amont due to zero if payment amount equals to expected rounded payment amount. Otherwise another payment should be required (that could use different rounding settings).
                    if (roundAmountDueBeforeLastPaymentResponse.RoundedValue == lastTenderLine.Amount)
                    {
                        salesTransaction.AmountDue = decimal.Zero;
                    }
                }

                // When required amount is positive, amount due must be zero or negative (overtender), otherwise (e.g. for refunds or exchanges) exact amount has to refunded (zero balance).
                salesTransaction.IsRequiredAmountPaid = (paymentRequiredAmount > 0 && salesTransaction.AmountDue <= 0)
                    || (paymentRequiredAmount <= 0 && salesTransaction.AmountDue == 0);
            }

            /// <summary>
            /// Calculates the deposit and updates the sales transaction deposit amount.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            public static void CalculateDeposit(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                salesTransaction.CalculatedDepositAmount = SalesTransactionTotaler.CalculateDepositAmount(context, salesTransaction);

                salesTransaction.RequiredDepositAmount = salesTransaction.IsDepositOverridden ?
                    RoundCurrencyAmount(context, salesTransaction.OverriddenDepositAmount.Value) :
                    salesTransaction.CalculatedDepositAmount;

                // the deposit available on the pickup operation is the DepositAvailableAmount
                // but we cannot use more deposit than the DepositRequiredAmount for the transaction
                if (salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup)
                {
                    bool hasAnyQuantityRemainingForPickup = salesTransaction.SalesLines.Any(line => (line.QuantityInvoiced + line.Quantity) < line.QuantityOrdered);
                    if (hasAnyQuantityRemainingForPickup)
                    {
                        salesTransaction.PrepaymentAmountAppliedOnPickup = Math.Min(salesTransaction.AvailableDepositAmount, salesTransaction.RequiredDepositAmount);
                    }
                    else
                    {
                        salesTransaction.PrepaymentAmountAppliedOnPickup = salesTransaction.AvailableDepositAmount;
                    }
                }
                else
                {
                    salesTransaction.PrepaymentAmountAppliedOnPickup = decimal.Zero;
                }
            }

            /// <summary>
            /// Populates the fields for total amount, total discount, and total taxes on the sales line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="salesLine">The sales line.</param>
            public static void CalculateLine(RequestContext context, SalesTransaction salesTransaction, SalesLine salesLine)
            {
                RoundingRule roundingRule = SalesTransactionTotaler.GetRoundingRule(context);

                SalesLineTotaller.CalculateLine(salesTransaction, salesLine, roundingRule);
            }

            /// <summary>
            /// Calculates the required amount that must be paid for a customer order transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The transaction that must be used for the calculation.</param>
            /// <returns>The amount that must be paid for this transaction.</returns>
            public static decimal CalculateRequiredPaymentAmount(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                if (salesTransaction.CartType != CartType.CustomerOrder)
                {
                    throw new InvalidOperationException("Transaction must be a customer order.");
                }

                switch (salesTransaction.CustomerOrderMode)
                {
                    case CustomerOrderMode.CustomerOrderCreateOrEdit:
                        return salesTransaction.RequiredDepositAmount - salesTransaction.PrepaymentAmountPaid;

                    case CustomerOrderMode.QuoteCreateOrEdit:
                        return decimal.Zero;

                    case CustomerOrderMode.Cancellation:
                        decimal cancellationChargeAmount = SalesTransactionTotaler.GetCancellationChargeAmount(
                            context,
                            salesTransaction,
                            context.GetChannelConfiguration());

                        // We need to refund the amount paid for the order minus cancellation charges
                        // For refunding, we use a negative amount due
                        return cancellationChargeAmount - salesTransaction.PrepaymentAmountPaid;

                    case CustomerOrderMode.Return:
                        return salesTransaction.TotalAmount;

                    case CustomerOrderMode.Pickup:
                        // customer needs to pay the total for the transaction minus the credit he has due to prepayments
                        // in case he has more credit than what the transaction is worth, we need to refund him
                        return salesTransaction.TotalAmount - salesTransaction.PrepaymentAmountAppliedOnPickup;

                    case CustomerOrderMode.None:
                    case CustomerOrderMode.OrderRecalled:
                        return decimal.Zero;

                    default:
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Customer order mode '{0}' not supported.", salesTransaction.CustomerOrderMode));
                }
            }

            /// <summary>
            /// Calculates all total sum in the transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            public void CalculateTotals(RequestContext context)
            {
                CalculateAmountDue(context, this.SalesTransaction);
            }

            internal static void AddToTaxItems(SalesTransaction salesTransaction, TaxableItem taxableItem)
            {
                // If a taxgroup has been assigned to the saleItem
                if (taxableItem.ItemTaxGroupId != null)
                {
                    AddToTaxItems(salesTransaction, taxableItem.TaxLines);
                }
            }

            /// <summary>
            /// Gets the calculated deposit amount.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <returns>Calculated deposit amount.</returns>
            private static decimal CalculateDepositAmount(RequestContext context, SalesTransaction salesTransaction)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");

                decimal calculatedDepositAmount = decimal.Zero;

                if (salesTransaction.CartType == CartType.CustomerOrder &&
                    (salesTransaction.CustomerOrderMode == CustomerOrderMode.CustomerOrderCreateOrEdit ||
                     salesTransaction.CustomerOrderMode == CustomerOrderMode.Pickup))
                {
                    decimal minimumDepositMultiplier = context.GetChannelConfiguration().MinimumDepositPercentage / 100M;

                    if (minimumDepositMultiplier > decimal.Zero)
                    {
                        // get cancellation charge total amount
                        decimal cancellationChargeAmount = SalesTransactionTotaler.GetCancellationChargeAmount(context, salesTransaction, context.GetChannelConfiguration());

                        // take the transaction total minus the cancellation charges
                        decimal transactionTotalAmountWithoutCancellationCharges = salesTransaction.TotalAmount - cancellationChargeAmount;

                        // charges are added before deposit calculation - we need to make sure we do not calculate the deposit over cancellation charges
                        // deposit required = (order total - any cancellation charges) * minimum deposit multiplier
                        calculatedDepositAmount = transactionTotalAmountWithoutCancellationCharges * minimumDepositMultiplier;
                    }
                }

                return RoundCurrencyAmount(context, calculatedDepositAmount);
            }

            private static decimal GetPaymentsSum(IEnumerable<TenderLineBase> tenderLines)
            {
                var notVoidedTenderLines = tenderLines.Where(t => t.Status != TenderLineStatus.Voided && t.Status != TenderLineStatus.Historical);
                decimal paymentsSum = notVoidedTenderLines.Sum(t => t.Amount);

                return paymentsSum;
            }

            private static decimal RoundWithPricesRounding(RequestContext context, decimal amountToRound, string currencyCode)
            {
                GetRoundedValueServiceRequest request = new GetRoundedValueServiceRequest(
                    amountToRound,
                    currencyCode,
                    numberOfDecimals: 0,
                    useSalesRounding: false);

                GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);

                return response.RoundedValue;
            }

            /// <summary>
            /// Calculates the amount due for this transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design.")]
            private static void CalculateAmountDue(RequestContext context, SalesTransaction salesTransaction)
            {
                if (salesTransaction == null)
                {
                    throw new ArgumentNullException("salesTransaction");
                }

                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string cancellationcode = channelConfiguration.CancellationChargeCode;
                string currencyCode = channelConfiguration.Currency;

                RoundingRule roundingRule = amountToRound => RoundWithPricesRounding(context, amountToRound, currencyCode);

                SalesTransactionTotaler.ClearTotalAmounts(salesTransaction);
                salesTransaction.NumberOfItems = 0m;

                // initialize with header-level charges list
                var charges = new List<ChargeLine>();
                if (salesTransaction.ChargeLines.Any())
                {
                    charges.AddRange(salesTransaction.ChargeLines);
                }

                // Calculate totals for sale items , which might also include line-level misc charge in it.
                foreach (SalesLine saleLineItem in salesTransaction.SalesLines)
                {
                    if (saleLineItem.IsVoided == false)
                    {
                        // Calculate the sum of items
                        salesTransaction.NumberOfItems += Math.Abs(saleLineItem.Quantity);

                        // calculate the line item cost excluding charges and tax on charges.
                        SalesLineTotaller.CalculateLine(salesTransaction, saleLineItem, roundingRule);

                        UpdateTotalAmounts(salesTransaction, saleLineItem);
                    }
                    else
                    {
                        saleLineItem.PeriodicDiscountPossibilities.Clear();
                        SalesLineTotaller.CalculateLine(salesTransaction, saleLineItem, roundingRule);
                    }

                    saleLineItem.WasChanged = false;
                }

                // Add eligible charges on sales lines
                foreach (SalesLine salesLine in salesTransaction.ChargeCalculableSalesLines)
                {
                    charges.AddRange(salesLine.ChargeLines);
                }

                decimal incomeExpenseTotalAmount = 0m;

                foreach (IncomeExpenseLine incomeExpense in salesTransaction.IncomeExpenseLines)
                {
                    if (incomeExpense.AccountType != IncomeExpenseAccountType.None)
                    {
                        salesTransaction.NetAmountWithTax += incomeExpense.Amount;
                        salesTransaction.NetAmountWithNoTax += incomeExpense.Amount;
                        salesTransaction.GrossAmount += incomeExpense.Amount;
                        salesTransaction.IncomeExpenseTotalAmount += incomeExpense.Amount;

                        // The total is done to calculate the payment amount.
                        incomeExpenseTotalAmount += incomeExpense.Amount;
                    }
                }

                foreach (ChargeLine charge in charges)
                {
                    AddToTaxItems(salesTransaction, charge);

                    // Calculate tax on the charge item.
                    CalculateTaxForCharge(charge);

                    if (charge.ChargeCode.Equals(cancellationcode, StringComparison.OrdinalIgnoreCase) && IsSeparateTaxInCancellationCharge(context))
                    {
                        salesTransaction.TaxOnCancellationCharge += charge.TaxAmount;
                    }
                    else
                    {
                        salesTransaction.TaxAmount += charge.TaxAmount;
                    }

                    // Later there is "TotalAmount = NetAmountWithTax + ChargeAmount", so we should add TaxAmountExclusive here
                    salesTransaction.NetAmountWithTax += charge.TaxAmountExclusive;
                }

                salesTransaction.DiscountAmount = salesTransaction.PeriodicDiscountAmount + salesTransaction.LineDiscount + salesTransaction.TotalDiscount;
                salesTransaction.ChargeAmount = salesTransaction.ChargesTotal();

                // Subtotal is the net amount for the transaction (which includes the discounts) and optionally the tax amount if tax inclusive
                salesTransaction.SubtotalAmount = roundingRule(salesTransaction.NetAmountWithNoTax + salesTransaction.TaxAmountInclusive);

                salesTransaction.SubtotalAmountWithoutTax = context.GetChannelConfiguration().PriceIncludesSalesTax
                                                                ? salesTransaction.SubtotalAmount - salesTransaction.TaxAmount
                                                                : salesTransaction.SubtotalAmount;

                // Net amount when saved should include charges, it should be done after Subtotal amount calc because Subtotal does not include charge amount.
                salesTransaction.NetAmountWithNoTax = roundingRule(salesTransaction.NetAmountWithNoTax + salesTransaction.ChargeAmount);

                if (salesTransaction.IncomeExpenseLines.Any())
                {
                    // Setting the total amount sames as Payment amount for Income/ expense accounts.
                    salesTransaction.TotalAmount = incomeExpenseTotalAmount;
                }
                else if (salesTransaction.TransactionType == SalesTransactionType.CustomerAccountDeposit && salesTransaction.CustomerAccountDepositLines.Any())
                {
                    CustomerAccountDepositLine customerAccountDepositLine = salesTransaction.CustomerAccountDepositLines.SingleOrDefault();
                    salesTransaction.SubtotalAmountWithoutTax = customerAccountDepositLine.Amount;
                    salesTransaction.TotalAmount = customerAccountDepositLine.Amount;
                }
                else
                {
                    // NetAmountWithTax already includes the discounts
                    salesTransaction.TotalAmount = roundingRule(salesTransaction.NetAmountWithTax + salesTransaction.ChargeAmount);
                }
            }

            private static void CalculateTaxForCharge(ChargeLine charge)
            {
                ChargeLineTotaler.CalculateTax(charge);
            }

            private static RoundingRule GetRoundingRule(RequestContext context)
            {
                ChannelConfiguration channelConfiguration = context.GetChannelConfiguration();
                string currencyCode = channelConfiguration.Currency;

                return amountToRound => RoundWithPricesRounding(context, amountToRound, currencyCode);
            }

            private static void ClearTotalAmounts(SalesTransaction transaction)
            {
                transaction.NetAmountWithNoTax = 0;
                transaction.NetAmountWithTax = 0;
                transaction.GrossAmount = 0;
                transaction.LineDiscount = 0;
                transaction.PeriodicDiscountAmount = 0;
                transaction.TotalDiscount = 0;
                transaction.TaxAmount = 0;
                transaction.TaxAmountExclusive = 0;
                transaction.TaxAmountInclusive = 0;
                transaction.IncomeExpenseTotalAmount = 0;
                transaction.TaxOnCancellationCharge = 0;

                transaction.TaxLines.Clear();
            }

            private static void UpdateTotalAmounts(SalesTransaction salesTransaction, SalesLine salesLine)
            {
                // Total amounts
                salesTransaction.NetAmountWithNoTax += salesLine.NetAmountWithNoTax();
                salesTransaction.NetAmountWithTax += salesLine.NetAmountWithTax();
                salesTransaction.GrossAmount += salesLine.GrossAmount;
                salesTransaction.LineDiscount += salesLine.LineDiscount;
                salesTransaction.PeriodicDiscountAmount += salesLine.PeriodicDiscount;
                salesTransaction.TotalDiscount += salesLine.TotalDiscount;
                salesTransaction.TaxAmount += salesLine.TaxAmount;
                salesTransaction.TaxAmountExclusive += salesLine.TaxAmountExclusive;
                salesTransaction.TaxAmountInclusive += salesLine.TaxAmountInclusive;

                AddToTaxItems(salesTransaction, salesLine);
            }

            private static void AddToTaxItems(SalesTransaction transaction, IEnumerable<TaxLine> taxLines)
            {
                // Looping through each of the tax lines currently set to the ietm
                foreach (TaxLine saleLineTaxItem in taxLines)
                {
                    // For every taxLine it is checked whether it has been added before to the transaction
                    // If found, add the amount to the existing tax item.
                    // If not a new taxItem is added to the transaction
                    bool found = false;

                    // Creating a new Tax item in case it needs to be added.
                    TaxLine taxItem = new TaxLine(saleLineTaxItem);

                    // Looping to see if tax group already exists
                    foreach (TaxLine transTaxItem in transaction.TaxLines)
                    {
                        if ((transTaxItem.TaxCode != null) && (transTaxItem.TaxCode == saleLineTaxItem.TaxCode))
                        {
                            transTaxItem.Amount += saleLineTaxItem.Amount;
                            transTaxItem.TaxBasis += saleLineTaxItem.TaxBasis;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        transaction.TaxLines.Add(taxItem);
                    }
                }
            }

            /// <summary>
            /// This method indicate whether calculate tax for cancellation charge on Customer order separately.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The flag indicating whether to calculate tax for cancellation separately or not.</returns>
            private static bool IsSeparateTaxInCancellationCharge(RequestContext context)
            {
                return context.GetChannelConfiguration().CountryRegionISOCode == CountryRegionISOCode.IN;
            }

            /// <summary>
            /// Gets the cancellation total charge amount (include taxes) on a sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to get the cancellation charge amount from.</param>
            /// <param name="channelConfiguration">The channel configuration object.</param>
            /// <returns>The cancellation total charge amount.</returns>
            private static decimal GetCancellationChargeAmount(RequestContext context, SalesTransaction salesTransaction, ChannelConfiguration channelConfiguration)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(channelConfiguration, "channelConfiguration");

                decimal cancellationChargeAmount = decimal.Zero;

                // sum up all cancellation charges in the order
                foreach (ChargeLine chargeLine in salesTransaction.ChargeLines)
                {
                    if (chargeLine.ChargeCode.Equals(channelConfiguration.CancellationChargeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        cancellationChargeAmount += chargeLine.CalculatedAmount + chargeLine.TaxAmountExclusive;
                    }
                }

                return SalesTransactionTotaler.RoundCurrencyAmount(context, cancellationChargeAmount);
            }

            /// <summary>
            /// Rounding helper method for rounding currency amounts.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="amount">The amount to be rounded.</param>
            /// <returns>The rounded amount.</returns>
            private static decimal RoundCurrencyAmount(RequestContext context, decimal amount)
            {
                GetRoundedValueServiceRequest request = new GetRoundedValueServiceRequest(
                    amount,
                    context.GetChannelConfiguration().Currency,
                    numberOfDecimals: 0,
                    useSalesRounding: false);

                GetRoundedValueServiceResponse response = context.Execute<GetRoundedValueServiceResponse>(request);

                return response.RoundedValue;
            }
        }
    }
}
