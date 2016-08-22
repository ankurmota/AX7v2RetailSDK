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
        using System.Diagnostics;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// The helper class for the loyalty services.
        /// </summary>
        internal sealed class LoyaltyServiceHelper
        {
            private static readonly string NoneSalesLineSalesId = string.Empty;
    
            /// <summary>
            /// Calculates earn reward points based on sales lines and fills in the reward point lines into the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="earnSchemeLines">The earn scheme line that apply to the sales transaction.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <returns>The result sales transaction.</returns>
            public static SalesTransaction FillInLoyaltyRewardPointLinesForSales(
                RequestContext context,
                SalesTransaction salesTransaction,
                ICollection<LoyaltySchemeLineEarn> earnSchemeLines,
                string loyaltyCardNumber)
            {
                // Find the count of the loyalty tender lines.
                int loyaltyTenderCount = 0;
                if (salesTransaction.TenderLines != null)
                {
                    loyaltyTenderCount = salesTransaction.TenderLines.Where(line => line.Status != TenderLineStatus.Voided && !string.IsNullOrWhiteSpace(line.LoyaltyCardId)).Count();
                }
    
                // Fnd the count of the redeem reward lines.
                int redeemCount = 0;
                if (salesTransaction.LoyaltyRewardPointLines != null)
                {
                    redeemCount = salesTransaction.LoyaltyRewardPointLines.Where(line => line.EntryType == LoyaltyRewardPointEntryType.Redeem).Count();
                }
    
                // Calculate earned points only if the transaction is not paid by loyalty.
                if (loyaltyTenderCount == 0 && redeemCount == 0)
                {
                    return FillInLoyaltyRewardPointLinesForEarnOrDeduct(
                        context,
                        salesTransaction,
                        earnSchemeLines,
                        loyaltyCardNumber,
                        LoyaltyRewardPointEntryType.Earn);
                }
                else
                {
                    return salesTransaction;
                }
            }
    
            /// <summary>
            /// Calculates return reward points based on return lines and fills in the reward point lines into the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="earnSchemeLines">The earn scheme line that apply to the sales transaction.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <returns>The result sales transaction.</returns>
            public static SalesTransaction FillInLoyaltyRewardPointLinesForReturn(
                RequestContext context,
                SalesTransaction salesTransaction,
                ICollection<LoyaltySchemeLineEarn> earnSchemeLines,
                string loyaltyCardNumber)
            {
                // Do not calculate return points if the original transaction is paid by loyalty
                if (!salesTransaction.IsReturnByReceipt
                    || (salesTransaction.IsReturnByReceipt && !salesTransaction.ReturnTransactionHasLoyaltyPayment))
                {
                    return FillInLoyaltyRewardPointLinesForEarnOrDeduct(
                    context,
                    salesTransaction,
                    earnSchemeLines,
                    loyaltyCardNumber,
                    LoyaltyRewardPointEntryType.ReturnEarned);
                }
                else
                {
                    return salesTransaction;
                }
            }
    
            /// <summary>
            /// Fills in the loyalty reward point lines for payment to the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to fill in.</param>
            /// <param name="loyaltyCard">The loyalty card.</param>
            /// <param name="redeemAmount">The redeem amount.</param>
            /// <param name="redeemCurrency">The redeem amount currency.</param>
            /// <returns>The result sales transaction.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Payment is complex by nature.")]
            public static SalesTransaction FillInLoyaltyRewardPointLinesForPayment(
                RequestContext context,
                SalesTransaction salesTransaction,
                LoyaltyCard loyaltyCard,
                decimal redeemAmount,
                string redeemCurrency)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Get reward points status.
                DateTime channelLocalDateTime = context.GetNowInChannelTimeZone().DateTime;
                var activePointList = GetLoyaltyCardActivePoints(context, loyaltyCard, channelLocalDateTime);
    
                if (activePointList != null && activePointList.Count > 0)
                {
                    // Prepare the loyalty payable amount per sales line
                    Dictionary<string, decimal> payablePerSalesLine = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (var salesLine in salesTransaction.ActiveSalesLines.Where(l => l.TotalAmount > 0))
                    {
                        decimal salesLineTotalCharges = (from cl in salesLine.ChargeLines select cl.CalculatedAmount).Sum();
                        payablePerSalesLine.Add(salesLine.LineId, salesLine.TotalAmount + salesLineTotalCharges);
                    }
    
                    // Add transaction-level charges to the list
                    if (salesTransaction.ChargeLines.Any())
                    {
                        decimal transactionLevelTotalCharges = (from cl in salesTransaction.ChargeLines select cl.CalculatedAmount).Sum();
                        payablePerSalesLine.Add(NoneSalesLineSalesId, transactionLevelTotalCharges);
                    }
    
                    // Get applicable redeem scheme lines per card, per reward point, until we find enough points for the tender amount.
                    decimal redeemAmountLeft = redeemAmount;
                    var rewardPointLines = new List<LoyaltyRewardPointLine>();
    
                    foreach (var activePoint in activePointList)
                    {
                        var getLoyaltySchemeLineRedeemDataRequest = new GetLoyaltySchemeLineRedeemDataRequest(
                            context.GetChannelConfiguration().RecordId,
                            loyaltyCard.CardNumber,
                            activePoint.RewardPointId);
                        getLoyaltySchemeLineRedeemDataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
    
                        ReadOnlyCollection<LoyaltySchemeLineRedeem> redeemLines = context.Execute<EntityDataServiceResponse<LoyaltySchemeLineRedeem>>(getLoyaltySchemeLineRedeemDataRequest).PagedEntityCollection.Results;
    
                        // Filter redeem lines to PyamentByAmount and PaymentByQuantity
                        redeemLines = redeemLines.Where(rsl => (rsl.ToRewardType == LoyaltyRewardType.PaymentByAmount || rsl.ToRewardType == LoyaltyRewardType.PaymentByQuantity)).AsReadOnly();
    
                        redeemAmountLeft = GenerateRewardPointLineForPayment(
                                            context,
                                            salesTransaction,
                                            payablePerSalesLine,
                                            activePoint.ActivePoints,
                                            redeemLines,
                                            redeemAmountLeft,
                                            redeemCurrency,
                                            loyaltyCard.CardNumber,
                                            salesTransaction.CustomerId,
                                            channelLocalDateTime,
                                            rewardPointLines);
    
                        if (redeemAmountLeft <= 0m)
                        {
                            break;
                        }
                    }
    
                    // Fill in the reward point lines.
                    if (redeemAmountLeft > 0m)
                    {
                        throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_NotEnoughRewardPoints, "The loyalty payment amount exceeds what is allowed for this loyalty card in this transaction.");
                    }
                    else
                    {
                        if (salesTransaction.LoyaltyRewardPointLines == null)
                        {
                            salesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>();
                        }
                        else
                        {
                            // Remove old redeem reward point trans
                            IEnumerable<LoyaltyRewardPointLine> otherLines = salesTransaction.LoyaltyRewardPointLines.Where(
                                l => l.EntryType != LoyaltyRewardPointEntryType.Redeem);
                            salesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>(otherLines.ToList());
                        }
    
                        // Add new redeem reward point trans
                        foreach (var rewardPointLine in rewardPointLines)
                        {
                            if (salesTransaction.LoyaltyRewardPointLines.Count > 0)
                            {
                                rewardPointLine.LineNumber = (from line in salesTransaction.LoyaltyRewardPointLines
                                                              select line.LineNumber).Max() + 1;
                            }
                            else
                            {
                                rewardPointLine.LineNumber = 1m;
                            }
    
                            salesTransaction.LoyaltyRewardPointLines.Add(rewardPointLine);
                        }
                    }
                }
                else
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_NotEnoughRewardPoints, "The loyalty payment amount exceeds what is allowed for this loyalty card in this transaction.");
                }
    
                return salesTransaction;
            }
    
            /// <summary>
            /// Fills in the loyalty reward point lines for refund to the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction to fill in.</param>
            /// <param name="loyaltyCard">The loyalty card.</param>
            /// <param name="refundAmount">The refund amount.</param>
            /// <param name="refundCurrency">The refund amount currency.</param>
            /// <returns>The result sales transaction.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Payment is complex by nature.")]
            public static SalesTransaction FillInLoyaltyRewardPointLinesForRefund(
                RequestContext context,
                SalesTransaction salesTransaction,
                LoyaltyCard loyaltyCard,
                decimal refundAmount,
                string refundCurrency)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                // Get reward points status.
                DateTime channelLocalDateTime = context.GetNowInChannelTimeZone().DateTime;
                var refundPointList = GetLoyaltyCardActivePoints(context, loyaltyCard, channelLocalDateTime);
    
                if (refundPointList != null && refundPointList.Count > 0)
                {
                    // Prepare the loyalty payable amount per sales line
                    Dictionary<string, decimal> payablePerSalesLine = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    SalesOrder salesOrder = salesTransaction as SalesOrder;
    
                    if (salesOrder != null && salesOrder.CustomerOrderMode == CustomerOrderMode.Cancellation)
                    {
                        payablePerSalesLine = salesTransaction.ActiveSalesLines.Where(line => line.TotalAmount > decimal.Zero).ToDictionary(line => line.LineId, line => line.TotalAmount);
                    }
                    else
                    {
                        payablePerSalesLine = salesTransaction.ActiveSalesLines.Where(line => line.TotalAmount < decimal.Zero).ToDictionary(line => line.LineId, line => decimal.Negate(line.TotalAmount));
                    }
    
                    // Get applicable redeem scheme lines per card, per reward point, until we find enough points for the tender amount.
                    decimal refundAmountLeft = decimal.Negate(refundAmount);
                    var rewardPointLines = new List<LoyaltyRewardPointLine>();
    
                    foreach (var refundPoint in refundPointList)
                    {
                        var getLoyaltySchemeLineRedeemDataRequest = new GetLoyaltySchemeLineRedeemDataRequest(
                            context.GetChannelConfiguration().RecordId,
                            loyaltyCard.CardNumber,
                            refundPoint.RewardPointId);
                        getLoyaltySchemeLineRedeemDataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
    
                        ReadOnlyCollection<LoyaltySchemeLineRedeem> redeemLines = context.Execute<EntityDataServiceResponse<LoyaltySchemeLineRedeem>>(getLoyaltySchemeLineRedeemDataRequest).PagedEntityCollection.Results;
    
                        // Filter redeem lines to PyamentByAmount and PaymentByQuantity
                        redeemLines = redeemLines.Where(rsl => (rsl.ToRewardType == LoyaltyRewardType.PaymentByAmount || rsl.ToRewardType == LoyaltyRewardType.PaymentByQuantity)).AsReadOnly();
    
                        refundAmountLeft = GenerateRewardPointLineForRefund(
                                            context,
                                            salesTransaction,
                                            payablePerSalesLine,
                                            redeemLines,
                                            refundAmountLeft,
                                            refundCurrency,
                                            loyaltyCard.CardNumber,
                                            salesTransaction.CustomerId,
                                            channelLocalDateTime,
                                            rewardPointLines);
    
                        if (refundAmountLeft <= 0m)
                        {
                            break;
                        }
                    }
    
                    // Fill in the reward point lines.
                    if (refundAmountLeft > 0m)
                    {
                        throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_RefundAmountMoreThanAllowed, "The loyalty refund amount exceeds what is allowed for this loyalty card in this transaction.");
                    }
                    else
                    {
                        if (salesTransaction.LoyaltyRewardPointLines == null)
                        {
                            salesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>();
                        }
                        else
                        {
                            // Remove old refund reward point trans
                            IEnumerable<LoyaltyRewardPointLine> otherLines = salesTransaction.LoyaltyRewardPointLines.Where(
                                l => l.EntryType != LoyaltyRewardPointEntryType.Refund);
                            salesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>(otherLines.ToList());
                        }
    
                        // Add new refund reward point trans
                        foreach (var rewardPointLine in rewardPointLines)
                        {
                            if (salesTransaction.LoyaltyRewardPointLines.Count > 0)
                            {
                                rewardPointLine.LineNumber = (from line in salesTransaction.LoyaltyRewardPointLines
                                                              select line.LineNumber).Max() + 1;
                            }
                            else
                            {
                                rewardPointLine.LineNumber = 1m;
                            }
    
                            salesTransaction.LoyaltyRewardPointLines.Add(rewardPointLine);
                        }
                    }
                }
                else
                {
                    throw new PaymentException(PaymentErrors.Microsoft_Dynamics_Commerce_Runtime_RefundAmountMoreThanAllowed, "The loyalty refund amount exceeds what is allowed for this loyalty card in this transaction.");
                }
    
                return salesTransaction;
            }
    
            /// <summary>
            /// Generates a reward point line for payment.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="payablePerSalesLine">The payable amount per sales line.</param>
            /// <param name="activePoints">The amount or quantity of the active points.</param>
            /// <param name="availableRedeemSchemeLines">The available redeem scheme lines for the active points.</param>
            /// <param name="redeemAmountLeft">The amount left to pay.</param>
            /// <param name="redeemCurrency">The payment currency.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <param name="channelLocalDateTime">The local date time of the channel.</param>
            /// <param name="rewardPointLines">The list of reward point lines to collect the generated line.</param>
            /// <returns>The amount left to pay after the new reward point line is generated.</returns>
            /// <remarks>
            /// Given the active points of a certain reward points, and given the available redeem scheme lines,
            /// this method try to find the best deal for the customer, and redeem points for payment. As a result,
            /// one new reward point line should be generated and added to the collection.
            /// </remarks>
            public static decimal GenerateRewardPointLineForPayment(
                RequestContext context,
                SalesTransaction salesTransaction,
                Dictionary<string, decimal> payablePerSalesLine,
                decimal activePoints,
                ReadOnlyCollection<LoyaltySchemeLineRedeem> availableRedeemSchemeLines,
                decimal redeemAmountLeft,
                string redeemCurrency,
                string loyaltyCardNumber,
                string customerAccountNumber,
                DateTime channelLocalDateTime,
                List<LoyaltyRewardPointLine> rewardPointLines)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(payablePerSalesLine, "payablePerSalesLine");
                ThrowIf.NullOrWhiteSpace(redeemCurrency, "redeemCurrency is null or white space.");
                ThrowIf.Null(rewardPointLines, "rewardPointLines");
    
                if (activePoints <= 0m
                    || availableRedeemSchemeLines.IsNullOrEmpty()
                    || redeemAmountLeft <= 0m)
                {
                    // No points or no scheme lines for redeem
                    return redeemAmountLeft;
                }
    
                decimal redeemPointsTotal = 0m;
                LoyaltySchemeLineRedeem firstBestRedeemLine = null;
    
                // Use the reward points to pay towards the transaction, product by product (because points may have restrictions on products)
                // For each sales line, we can only redeem at most the min of redeemAmountLeft and payablePerSalesLine.
                foreach (var salesLine in salesTransaction.ActiveSalesLines)
                {
                    if (payablePerSalesLine.ContainsKey(salesLine.LineId) && payablePerSalesLine[salesLine.LineId] > 0m)
                    {
                        decimal salesLineMaxRedeemAmount = Math.Min(payablePerSalesLine[salesLine.LineId], redeemAmountLeft);
    
                        // Find the best payment deal for the customer.
                        LoyaltySchemeLineRedeem bestRedeemLine = FindBestRedeemLine(context, salesLine, redeemCurrency, availableRedeemSchemeLines);
    
                        if (bestRedeemLine != null && bestRedeemLine.FromRewardPointAmountQuantity > 0 && bestRedeemLine.ToRewardAmountQuantity > 0)
                        {
                            if (firstBestRedeemLine == null)
                            {
                                firstBestRedeemLine = bestRedeemLine;
                            }
    
                            decimal redeemPoints = CalculateRedeemPointsPerSalesLine(ref redeemAmountLeft, context, payablePerSalesLine, activePoints, redeemCurrency, salesLine, salesLineMaxRedeemAmount, bestRedeemLine);
                            activePoints -= redeemPoints;
    
                            if (redeemPoints > 0m)
                            {
                                redeemPointsTotal += redeemPoints;
                            }
                        }
                    }
    
                    if (redeemAmountLeft <= 0m || activePoints <= 0m)
                    {
                        break;
                    }
                }
    
                // Use the reward points to pay towards the transaction-level charge lines
                // We can only redeem at most the min of redeemAmountLeft and transactionLevelTotalCharges.
                if (redeemAmountLeft > 0m && activePoints > 0m && payablePerSalesLine.ContainsKey(NoneSalesLineSalesId) && payablePerSalesLine[NoneSalesLineSalesId] > 0m)
                {
                    decimal maxRedeemAmount = Math.Min(payablePerSalesLine[NoneSalesLineSalesId], redeemAmountLeft);
    
                    // Find the best payment deal for the customer.
                    LoyaltySchemeLineRedeem bestRedeemLine = FindBestRedeemLine(context, null, redeemCurrency, availableRedeemSchemeLines);
    
                    if (bestRedeemLine != null && bestRedeemLine.FromRewardPointAmountQuantity > 0 && bestRedeemLine.ToRewardAmountQuantity > 0)
                    {
                        if (firstBestRedeemLine == null)
                        {
                            firstBestRedeemLine = bestRedeemLine;
                        }
    
                        decimal redeemPoints = CalculateRedeemPointsPerSalesLine(ref redeemAmountLeft, context, payablePerSalesLine, activePoints, redeemCurrency, null, maxRedeemAmount, bestRedeemLine);
                        activePoints -= redeemPoints;
    
                        if (redeemPoints > 0m)
                        {
                            redeemPointsTotal += redeemPoints;
                        }
                    }
                }
    
                if (redeemPointsTotal > 0m)
                {
                    // Round points
                    LoyaltySchemeLineRedeem firstRedeemLine = availableRedeemSchemeLines[0];
                    redeemPointsTotal = redeemPointsTotal * -1;
                    redeemPointsTotal = RoundRewardPointsForPayment(redeemPointsTotal, firstRedeemLine.FromRewardPointType, firstRedeemLine.FromRewardPointCurrency, context);
    
                    // Generate a reward point line
                    var rewardPointLine = new LoyaltyRewardPointLine
                    {
                        CustomerAccount = customerAccountNumber,
                        EntryDate = channelLocalDateTime.Date,
                        EntryTime = (int)channelLocalDateTime.TimeOfDay.TotalSeconds,
                        EntryType = LoyaltyRewardPointEntryType.Redeem,
                        LoyaltyCardNumber = loyaltyCardNumber,
                        LoyaltyGroupRecordId = firstBestRedeemLine.LoyaltyGroupRecordId,
                        LoyaltyTierRecordId = firstBestRedeemLine.LoyaltyTierRecordId,
                        RewardPointRecordId = firstRedeemLine.FromRewardPointRecordId,
                        RewardPointId = firstRedeemLine.FromRewardPointId,
                        RewardPointIsRedeemable = firstRedeemLine.FromRewardPointIsRedeemable,
                        RewardPointType = firstRedeemLine.FromRewardPointType,
                        RewardPointAmountQuantity = redeemPointsTotal,
                        RewardPointCurrency = firstRedeemLine.FromRewardPointCurrency
                    };
    
                    rewardPointLines.Add(rewardPointLine);
                }
    
                return redeemAmountLeft;
            }
    
            /// <summary>
            /// Calculates redeem points per sales line.
            /// </summary>
            /// <param name="redeemAmountLeft">The amount left to pay.</param>
            /// <param name="context">The request context.</param>
            /// <param name="payablePerSalesLine">The payable amount per sales line.</param>
            /// <param name="activePoints">The amount or quantity of the active points.</param>
            /// <param name="redeemCurrency">The payment currency.</param>
            /// <param name="salesLine">The sales line to pay towards. The value is null when the transaction-level charges are being paid.</param>
            /// <param name="maxRedeemAmount">The maximum redeem amount.</param>
            /// <param name="redeemLine">The redemption rule.</param>
            /// <returns>The redeem points.</returns>
            private static decimal CalculateRedeemPointsPerSalesLine(ref decimal redeemAmountLeft, RequestContext context, Dictionary<string, decimal> payablePerSalesLine, decimal activePoints, string redeemCurrency, SalesLine salesLine, decimal maxRedeemAmount, LoyaltySchemeLineRedeem redeemLine)
            {
                // Calculate needed points
                decimal redeemPoints;
                decimal neededPoints;
                decimal unitAmount = 0m;
                decimal unitPoints = 0m;
                switch (redeemLine.ToRewardType)
                {
                    case LoyaltyRewardType.PaymentByAmount:
                        neededPoints = (maxRedeemAmount / redeemLine.ToRewardAmountQuantity) * redeemLine.FromRewardPointAmountQuantity;
                        break;
    
                    case LoyaltyRewardType.PaymentByQuantity:
                        unitAmount = salesLine.TotalAmount / salesLine.Quantity;
                        unitPoints = redeemLine.FromRewardPointAmountQuantity / redeemLine.ToRewardAmountQuantity;
                        neededPoints = (maxRedeemAmount / unitAmount) * unitPoints;
                        break;
    
                    default:
                        throw new InvalidOperationException(string.Format("Loyalty reward point type '{0}' is not supported for payment.", redeemLine.ToRewardType));
                }
    
                // Compare needed points with available points
                if (activePoints < neededPoints)
                {
                    // Spend all active points of the current point
                    redeemPoints = activePoints;
    
                    // Calculate covered amount
                    decimal coveredAmount = 0m;
                    switch (redeemLine.ToRewardType)
                    {
                        case LoyaltyRewardType.PaymentByAmount:
                            coveredAmount = (redeemPoints / redeemLine.FromRewardPointAmountQuantity) * redeemLine.ToRewardAmountQuantity;
                            break;
    
                        case LoyaltyRewardType.PaymentByQuantity:
                            coveredAmount = (redeemPoints / unitPoints) * unitAmount;
                            break;
    
                        default:
                            throw new InvalidOperationException(string.Format("Loyalty redemption type '{0}' is not supported for payment.", redeemLine.ToRewardType));
                    }
    
                    // Amount rounding based on currency
                    var roundingRequest = new GetRoundedValueServiceRequest(coveredAmount, redeemCurrency);
                    var response = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
                    coveredAmount = response.RoundedValue;
    
                    redeemAmountLeft -= coveredAmount;
                    payablePerSalesLine[salesLine.LineId] -= coveredAmount;
                }
                else
                {
                    redeemPoints = neededPoints;
                    redeemAmountLeft -= maxRedeemAmount;
    
                    if (salesLine != null)
                    {
                        payablePerSalesLine[salesLine.LineId] -= maxRedeemAmount;
                    }
                    else
                    {
                        payablePerSalesLine[NoneSalesLineSalesId] -= maxRedeemAmount;
                    }
                }
    
                return redeemPoints;
            }
    
            /// <summary>
            /// Generates a reward point line for refund.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="payablePerSalesLine">The payable amount per sales line.</param>
            /// <param name="availableRedeemSchemeLines">The available redeem scheme lines for the refund points.</param>
            /// <param name="refundAmountLeft">The amount left to refund.</param>
            /// <param name="refundCurrency">The payment currency.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <param name="channelLocalDateTime">The local date time of the channel.</param>
            /// <param name="rewardPointLines">The list of reward point lines to collect the generated line.</param>
            /// <returns>The amount left to refund after the new reward point line is generated.</returns>
            /// <remarks>
            /// Given a certain reward point, and given the available redeem scheme lines,
            /// this method tries to find the best deal for the customer, and calculates points for refund. As a result,
            /// one new reward point line should be generated and added to the collection.
            /// </remarks>
            private static decimal GenerateRewardPointLineForRefund(
                RequestContext context,
                SalesTransaction salesTransaction,
                Dictionary<string, decimal> payablePerSalesLine,
                ReadOnlyCollection<LoyaltySchemeLineRedeem> availableRedeemSchemeLines,
                decimal refundAmountLeft,
                string refundCurrency,
                string loyaltyCardNumber,
                string customerAccountNumber,
                DateTime channelLocalDateTime,
                List<LoyaltyRewardPointLine> rewardPointLines)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(salesTransaction, "salesTransaction");
                ThrowIf.Null(payablePerSalesLine, "payablePerSalesLine");
                ThrowIf.NullOrWhiteSpace(refundCurrency, "redeemCurrency is null or white space.");
                ThrowIf.Null(rewardPointLines, "rewardPointLines");
    
                if (availableRedeemSchemeLines.IsNullOrEmpty()
                    || refundAmountLeft <= 0m)
                {
                    // No points or no scheme lines for refund
                    return refundAmountLeft;
                }
    
                decimal refundPointsTotal = 0m;
                LoyaltySchemeLineRedeem firstBestRedeemLine = null;
    
                // Use the reward points to refund towards the transaction, product by product (because points may have restrictions on products)
                // For each sales line, we can only refund at most the min of redeemAmountLeft and payablePerSalesLine.
                foreach (var salesLine in salesTransaction.ActiveSalesLines)
                {
                    if (payablePerSalesLine.ContainsKey(salesLine.LineId) && payablePerSalesLine[salesLine.LineId] > 0m)
                    {
                        decimal salesLineMaxRefundAmount = Math.Min(payablePerSalesLine[salesLine.LineId], refundAmountLeft);
    
                        // Find the best payment deal for the customer.
                        LoyaltySchemeLineRedeem bestRedeemLine = FindBestRedeemLine(context, salesLine, refundCurrency, availableRedeemSchemeLines);
    
                        if (bestRedeemLine != null && bestRedeemLine.FromRewardPointAmountQuantity > 0 && bestRedeemLine.ToRewardAmountQuantity > 0)
                        {
                            if (firstBestRedeemLine == null)
                            {
                                firstBestRedeemLine = bestRedeemLine;
                            }
    
                            // Calculate needed points
                            decimal refundPoints;
                            switch (bestRedeemLine.ToRewardType)
                            {
                                case LoyaltyRewardType.PaymentByAmount:
                                    refundPoints = (salesLineMaxRefundAmount / bestRedeemLine.ToRewardAmountQuantity) * bestRedeemLine.FromRewardPointAmountQuantity;
                                    break;
    
                                case LoyaltyRewardType.PaymentByQuantity:
                                    decimal unitAmount = salesLine.TotalAmount / salesLine.Quantity;
                                    decimal unitPoints = bestRedeemLine.FromRewardPointAmountQuantity / bestRedeemLine.ToRewardAmountQuantity;
                                    refundPoints = (salesLineMaxRefundAmount / unitAmount) * unitPoints;
                                    break;
    
                                default:
                                    throw new InvalidOperationException(string.Format("Loyalty reward point type '{0}' is not supported for payment.", bestRedeemLine.ToRewardType));
                            }
    
                            refundAmountLeft -= salesLineMaxRefundAmount;
                            payablePerSalesLine[salesLine.LineId] -= salesLineMaxRefundAmount;
    
                            // Remember redeem points per loyalty group
                            if (refundPoints > 0m)
                            {
                                refundPointsTotal += refundPoints;
                            }
                        }
                    }
    
                    if (refundAmountLeft <= 0m)
                    {
                        break;
                    }
                }
    
                if (refundPointsTotal > 0m)
                {
                    // Round points
                    LoyaltySchemeLineRedeem firstRedeemLine = availableRedeemSchemeLines[0];
                    refundPointsTotal = RoundRewardPointsForPayment(refundPointsTotal, firstRedeemLine.FromRewardPointType, firstRedeemLine.FromRewardPointCurrency, context);
    
                    // Generate a reward point line
                    var rewardPointLine = new LoyaltyRewardPointLine
                    {
                        CustomerAccount = customerAccountNumber,
                        EntryDate = channelLocalDateTime.Date,
                        EntryTime = (int)channelLocalDateTime.TimeOfDay.TotalSeconds,
                        EntryType = LoyaltyRewardPointEntryType.Refund,
                        LoyaltyCardNumber = loyaltyCardNumber,
                        LoyaltyGroupRecordId = firstBestRedeemLine.LoyaltyGroupRecordId,
                        LoyaltyTierRecordId = firstBestRedeemLine.LoyaltyTierRecordId,
                        RewardPointRecordId = firstRedeemLine.FromRewardPointRecordId,
                        RewardPointId = firstRedeemLine.FromRewardPointId,
                        RewardPointIsRedeemable = firstRedeemLine.FromRewardPointIsRedeemable,
                        RewardPointType = firstRedeemLine.FromRewardPointType,
                        RewardPointAmountQuantity = refundPointsTotal,
                        RewardPointCurrency = firstRedeemLine.FromRewardPointCurrency
                    };
                    rewardPointLine.ExpirationDate = GetExpirationDate(rewardPointLine.EntryDate, firstRedeemLine.FromRewardPointExpirationTimeValue, firstRedeemLine.FromRewardPointExpirationTimeUnit);
    
                    rewardPointLines.Add(rewardPointLine);
                }
    
                return refundAmountLeft;
            }
    
            /// <summary>
            /// Calculates earn or return reward points and fills in the reward point lines into the sales transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="earnSchemeLines">The earn scheme line that apply to the sales transaction.</param>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="entryType">The entry type of the reward point.</param>
            /// <returns>The result sales transaction.</returns>
            private static SalesTransaction FillInLoyaltyRewardPointLinesForEarnOrDeduct(
                RequestContext context,
                SalesTransaction salesTransaction,
                ICollection<LoyaltySchemeLineEarn> earnSchemeLines,
                string loyaltyCardNumber,
                LoyaltyRewardPointEntryType entryType)
            {
                ThrowIf.Null(salesTransaction, "salesTransaction");
    
                if (salesTransaction.ActiveSalesLines != null && salesTransaction.ActiveSalesLines.Count > 0
                    && earnSchemeLines != null && earnSchemeLines.Count >= 0)
                {
                    string channelCurrency = context.GetChannelConfiguration().Currency;
    
                    // For each earn scheme line, calculate the earned reward points.
                    foreach (var earnSchemeLine in earnSchemeLines)
                    {
                        decimal earnedPoints = CalculateEarnedPointsBySchemeLine(context, earnSchemeLine, salesTransaction, channelCurrency, entryType);
    
                        // Add reward point line to the transaction.
                        if (earnedPoints != 0m)
                        {
                            if (salesTransaction.LoyaltyRewardPointLines == null)
                            {
                                salesTransaction.LoyaltyRewardPointLines = new Collection<LoyaltyRewardPointLine>();
                            }
    
                            DateTime channelDateTime = context.GetNowInChannelTimeZone().DateTime;
                            var rewardPointLine = new LoyaltyRewardPointLine
                            {
                                LoyaltyGroupRecordId = earnSchemeLine.LoyaltyGroupRecordId,
                                LoyaltyCardNumber = loyaltyCardNumber,
                                CustomerAccount = salesTransaction.CustomerId,
                                EntryDate = channelDateTime.Date,
                                EntryTime = (int)channelDateTime.TimeOfDay.TotalSeconds,
                                EntryType = entryType
                            };
    
                            if (salesTransaction.LoyaltyRewardPointLines.Count > 0)
                            {
                                rewardPointLine.LineNumber = (from line in salesTransaction.LoyaltyRewardPointLines
                                                              select line.LineNumber).Max() + 1;
                            }
                            else
                            {
                                rewardPointLine.LineNumber = 1m;
                            }
    
                            // Calculate reward point expiration date if earned points is positive.
                            if (earnedPoints > 0m)
                            {
                                rewardPointLine.ExpirationDate = GetExpirationDate(rewardPointLine.EntryDate, earnSchemeLine.ToRewardPointExpirationTimeValue, earnSchemeLine.ToRewardPointExpirationTimeUnit);
                            }
    
                            rewardPointLine.LoyaltyTierRecordId = earnSchemeLine.LoyaltyTierRecordId;
                            rewardPointLine.RewardPointRecordId = earnSchemeLine.ToRewardPointRecordId;
                            rewardPointLine.RewardPointId = earnSchemeLine.ToRewardPointId;
                            rewardPointLine.RewardPointIsRedeemable = earnSchemeLine.ToRewardPointIsRedeemable;
                            rewardPointLine.RewardPointType = earnSchemeLine.ToRewardPointType;
                            rewardPointLine.RewardPointAmountQuantity = earnedPoints;
                            rewardPointLine.RewardPointCurrency = earnSchemeLine.ToRewardPointCurrency;
    
                            salesTransaction.LoyaltyRewardPointLines.Add(rewardPointLine);
                        }
                    }
                }
    
    #if DEBUG
                DebugRewardPointLines(salesTransaction.LoyaltyRewardPointLines);
    #endif
                return salesTransaction;
            }
    
    #if DEBUG
            private static void DebugRewardPointLines(IEnumerable<LoyaltyRewardPointLine> rewardPointLines)
            {
                Debug.WriteLine("LoyaltyRewardPointLines(#{0})", rewardPointLines.Count());
                foreach (var loyaltyRewardPointLine in rewardPointLines)
                {
                    Debug.WriteLine("   LoyaltyRewardPointLine:{0}", loyaltyRewardPointLine.LineNumber);
                    Debug.WriteLine("       RewardPointId:{0}", loyaltyRewardPointLine.RewardPointId);
                    Debug.WriteLine("       RewardPointAmountQuantity:{0} {1}", loyaltyRewardPointLine.RewardPointAmountQuantity, loyaltyRewardPointLine.RewardPointCurrency);
                    Debug.WriteLine("       LoyaltyCardNumber:{0}", loyaltyRewardPointLine.LoyaltyCardNumber);
                    Debug.WriteLine("       CustomerAccount:{0}", loyaltyRewardPointLine.CustomerAccount);
                    Debug.WriteLine("       EntryDate:{0}", loyaltyRewardPointLine.EntryDate);
                    Debug.WriteLine("       EntryType:{0}", loyaltyRewardPointLine.EntryType);
                    Debug.WriteLine("       ExpirationDate:{0}", loyaltyRewardPointLine.ExpirationDate);
                    Debug.WriteLine("       RewardPointIsRedeemable:{0} RewardPointType:{1}", loyaltyRewardPointLine.RewardPointIsRedeemable, loyaltyRewardPointLine.RewardPointType);
                }
            }
    #endif
    
            /// <summary>
            /// Calculates the expiration date.
            /// </summary>
            /// <param name="entryDate">The entry date of the points.</param>
            /// <param name="expirationTimeValue">The expiration value of the reward point.</param>
            /// <param name="expirationTimeUnit">The expiration unit of the reward point.</param>
            /// <returns>The expiration date.</returns>
            private static DateTimeOffset GetExpirationDate(DateTimeOffset entryDate, int expirationTimeValue, DayMonthYear expirationTimeUnit)
            {
                DateTimeOffset expirationDate;
    
                if (expirationTimeValue == 0)
                {
                    // AX "Never" value.
                    expirationDate = DateTimeOffsetExtensions.AxMaxDateValue;
                }
                else
                {
                    switch (expirationTimeUnit)
                    {
                        case DayMonthYear.Day:
                            expirationDate = entryDate.AddDays(expirationTimeValue);
                            break;
    
                        case DayMonthYear.Month:
                            expirationDate = entryDate.AddMonths(expirationTimeValue);
                            break;
    
                        case DayMonthYear.Year:
                            expirationDate = entryDate.AddYears(expirationTimeValue);
                            break;
    
                        default:
                            expirationDate = DateTime.MaxValue;
                            break;
                    }
                }
    
                return expirationDate;
            }
    
            /// <summary>
            /// Calculates the earned points based on the scheme line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="earnSchemeLine">The scheme line.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="channelCurrency">The channel currency.</param>
            /// <param name="entryType">The entry type of the reward point.</param>
            /// <returns>The amount or the quantity of the earned reward points.</returns>
            /// <remarks>
            /// This method handles calculation of either earned points based on sales lines or returned points based on return lines.
            /// </remarks>
            private static decimal CalculateEarnedPointsBySchemeLine(
                RequestContext context,
                LoyaltySchemeLineEarn earnSchemeLine,
                SalesTransaction salesTransaction,
                string channelCurrency,
                LoyaltyRewardPointEntryType entryType)
            {
                decimal earnedPoints = 0;
                decimal totalQuantity = 0;
                decimal totalAmount = 0;
    
                // Get the purchase amount or quantity that applies to the scheme line
                if (earnSchemeLine.FromActivityType == LoyaltyActivityType.PurchaseProductByAmount
                    || earnSchemeLine.FromActivityType == LoyaltyActivityType.PurchaseProductByQuantity
                    || earnSchemeLine.FromActivityType == LoyaltyActivityType.SalesTransactionCount)
                {
                    GetPurchaseAmountAndQuantity(context, earnSchemeLine, salesTransaction, entryType, ref totalQuantity, ref totalAmount);
                }
    
                // Calculate points
                switch (earnSchemeLine.FromActivityType)
                {
                    case LoyaltyActivityType.PurchaseProductByAmount:
                        // Convert the transaction amount from the channel currency to the activity currency
                        if (!channelCurrency.Equals(earnSchemeLine.FromActivityAmountCurrency, StringComparison.OrdinalIgnoreCase))
                        {
                            var currencyValueRequest =
                                new GetCurrencyValueServiceRequest(channelCurrency, earnSchemeLine.FromActivityAmountCurrency, totalAmount);
                            GetCurrencyValueServiceResponse currencyValueResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyValueRequest);
                            totalAmount = currencyValueResponse.ConvertedAmount;
                        }
    
                        earnedPoints = CheckThresholdAndCalculateEarnedPoints(earnSchemeLine, totalAmount);
                        break;
    
                    case LoyaltyActivityType.PurchaseProductByQuantity:
                        earnedPoints = CheckThresholdAndCalculateEarnedPoints(earnSchemeLine, totalQuantity);
                        break;
    
                    case LoyaltyActivityType.SalesTransactionCount:
                        // Only earn points for SalesTransactionCount if it's calculating earned points for sales lines.
                        if (entryType == LoyaltyRewardPointEntryType.Earn && totalAmount > decimal.Zero)
                        {
                            // The activity amount/quantity can only be 1.0 for SalesTransactionCount.
                            earnedPoints = CheckThresholdAndCalculateEarnedPoints(earnSchemeLine, 1.0m);
                        }
    
                        break;
                }
    
                // Rounding points
                earnedPoints = RoundRewardPointsForEarnOrDeduct(earnedPoints, earnSchemeLine.ToRewardPointType, earnSchemeLine.ToRewardPointCurrency, context);
    
                return earnedPoints;
            }
    
            /// <summary>
            /// Rounds the reward points based on its type and currency when applies.
            /// </summary>
            /// <param name="points">The quantity or amount of the points. Points are positive for sales, negative for return.</param>
            /// <param name="rewardPointType">The reward point type.</param>
            /// <param name="rewardPointCurrency">The reward point currency for Amount type.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The points after rounding.</returns>
            private static decimal RoundRewardPointsForEarnOrDeduct(decimal points, LoyaltyRewardPointType rewardPointType, string rewardPointCurrency, RequestContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                decimal roundedPoints = points;
    
                switch (rewardPointType)
                {
                    case LoyaltyRewardPointType.Amount:
    
                        // Rounding based on currency
                        var roundingRequest = new GetRoundedValueServiceRequest(roundedPoints, rewardPointCurrency);
                        var response = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
                        roundedPoints = response.RoundedValue;
                        break;
    
                    case LoyaltyRewardPointType.Quantity:
    
                        // Rounding by cutting the decimal points
                        roundedPoints = roundedPoints > 0m ? Math.Floor(points) : Math.Ceiling(points);
    
                        break;
                }
    
                return roundedPoints;
            }
    
            /// <summary>
            /// Rounds the reward points based on its type and currency when applies.
            /// </summary>
            /// <param name="points">The quantity or amount of the points. Points are negative for payment, but positive for refund.</param>
            /// <param name="rewardPointType">The reward point type.</param>
            /// <param name="rewardPointCurrency">The reward point currency for Amount type.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The points after rounding.</returns>
            private static decimal RoundRewardPointsForPayment(decimal points, LoyaltyRewardPointType rewardPointType, string rewardPointCurrency, RequestContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                decimal roundedPoints = points;
    
                switch (rewardPointType)
                {
                    case LoyaltyRewardPointType.Amount:
    
                        // Rounding based on currency
                        var roundingRequest = new GetRoundedValueServiceRequest(roundedPoints, rewardPointCurrency);
                        var response = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
                        roundedPoints = response.RoundedValue;
                        break;
    
                    case LoyaltyRewardPointType.Quantity:
    
                        // Rounding by raising to whole points
                        roundedPoints = roundedPoints < 0m ? Math.Floor(points) : Math.Ceiling(points);
    
                        break;
                }
    
                return roundedPoints;
            }
    
            /// <summary>
            /// Calculates the earned points based on the scheme line and earning activity amount or quantity.
            /// </summary>
            /// <param name="earnSchemeLine">The earn scheme line.</param>
            /// <param name="activityAmountQuantity">The amount or quantity of the earning activity.</param>
            /// <returns>The earned points.</returns>
            private static decimal CheckThresholdAndCalculateEarnedPoints(LoyaltySchemeLineEarn earnSchemeLine, decimal activityAmountQuantity)
            {
                decimal earnedPoints = 0;
    
                if (Math.Abs(activityAmountQuantity) >= Math.Abs(earnSchemeLine.FromActivityAmountQuantity))
                {
                    if (earnSchemeLine.ToRewardPointType == LoyaltyRewardPointType.Quantity)
                    {
                        decimal div = activityAmountQuantity / earnSchemeLine.FromActivityAmountQuantity;
                        div = div > 0m ? Math.Floor(div) : Math.Ceiling(div);
    
                        earnedPoints = div * earnSchemeLine.ToRewardPointAmountQuantity;
                    }
                    else if (earnSchemeLine.ToRewardPointType == LoyaltyRewardPointType.Amount)
                    {
                        earnedPoints = (activityAmountQuantity / earnSchemeLine.FromActivityAmountQuantity) * earnSchemeLine.ToRewardPointAmountQuantity;
                    }
                }
    
                return earnedPoints;
            }
    
            /// <summary>
            /// Gets the purchase amount and quantity that applies to the earn scheme line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="earnSchemeLine">The earn scheme line.</param>
            /// <param name="salesTransaction">The sales transaction.</param>
            /// <param name="entryType">The entry type.</param>
            /// <param name="totalQuantity">The purchase quantity.</param>
            /// <param name="totalAmount">The purchase amount.</param>
            private static void GetPurchaseAmountAndQuantity(RequestContext context, LoyaltySchemeLineEarn earnSchemeLine, SalesTransaction salesTransaction, LoyaltyRewardPointEntryType entryType, ref decimal totalQuantity, ref decimal totalAmount)
            {
                bool isProductIndependent = earnSchemeLine.FromCategoryRecordId == 0
                                            && earnSchemeLine.FromProductRecordId == 0
                                            && earnSchemeLine.FromVariantRecordId == 0;
    
                // Find the sales lines or the return lines based on entry type.
                List<SalesLine> salesOrReturnLines = null;
                switch (entryType)
                {
                    case LoyaltyRewardPointEntryType.Earn:
                        salesOrReturnLines = salesTransaction.ActiveSalesLines.Where(sl => sl.Quantity > 0).ToList();
                        break;
    
                    case LoyaltyRewardPointEntryType.ReturnEarned:
                        // We only calculate returned points for return lines by receipt.
                        // Ad-hoc return lines won't deduct any points.
                        salesOrReturnLines = salesTransaction.ActiveSalesLines.Where(sl => sl.Quantity < 0).ToList();
                        break;
                }
    
                // Iterate through lines, and sum up total quantity and total amount.
                foreach (var salesOrReturnLine in salesOrReturnLines)
                {
                    if (!salesOrReturnLine.IsVoided)
                    {
                        bool accept = false;
    
                        if (isProductIndependent)
                        {
                            accept = true;
                        }
                        else if (earnSchemeLine.FromVariantRecordId != 0)
                        {
                            if (salesOrReturnLine.Variant != null)
                            {
                                accept = earnSchemeLine.FromVariantRecordId == salesOrReturnLine.Variant.DistinctProductVariantId;
                            }
                        }
                        else if (earnSchemeLine.FromProductRecordId != 0)
                        {
                            long productId;
    
                            productId = salesOrReturnLine.MasterProductId != 0 ? salesOrReturnLine.MasterProductId : salesOrReturnLine.ProductId;
    
                            accept = earnSchemeLine.FromProductRecordId == productId;
                        }
                        else if (earnSchemeLine.FromCategoryRecordId != 0)
                        {
                            // Check whether purchased product or variant belongs the category
                            long productId;
                            if (salesOrReturnLine.Variant != null && salesOrReturnLine.Variant.DistinctProductVariantId != 0)
                            {
                                productId = salesOrReturnLine.Variant.DistinctProductVariantId;
                            }
                            else
                            {
                                productId = salesOrReturnLine.ProductId;
                            }
    
                            CheckIfProductOrVariantAreInCategoryDataRequest dataRequest = new CheckIfProductOrVariantAreInCategoryDataRequest(productId, earnSchemeLine.FromCategoryRecordId);
                            accept = context.Runtime.Execute<SingleEntityDataServiceResponse<bool>>(dataRequest, context).Entity;
                        }
    
                        if (accept)
                        {
                            totalQuantity += salesOrReturnLine.Quantity;
                            totalAmount += salesOrReturnLine.NetAmount;
                        }
                    }
                }
            }
    
            /// <summary>
            /// Gets the reward point status of the given loyalty card.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="loyaltyCard">The loyalty card.</param>
            /// <param name="channelLocalDateTime">The local date time of the channel.</param>
            /// <returns>
            /// The list of active reward points which is sorted by redeem ranking (higher ranking first).
            /// </returns>
            private static List<LoyaltyRewardPoint> GetLoyaltyCardActivePoints(RequestContext context, LoyaltyCard loyaltyCard, DateTime channelLocalDateTime)
            {
                var serviceRequest = new GetLoyaltyCardRewardPointsStatusRealtimeRequest(
                    channelLocalDateTime.Date,
                    loyaltyCard.CardNumber,
                    excludeBlocked: true,
                    excludeNoTender: true,
                    includeRelatedCardsForContactTender: true,
                    includeNonRedeemablePoints: false,
                    includeActivePointsOnly: true);
    
                EntityDataServiceResponse<LoyaltyCard> serviceResponse = context.Execute<EntityDataServiceResponse<LoyaltyCard>>(serviceRequest);
                ReadOnlyCollection<LoyaltyCard> cardsStatus = serviceResponse.PagedEntityCollection.Results;
    
                List<LoyaltyRewardPoint> activePointsList = null;
                if (cardsStatus != null && cardsStatus.Count > 0)
                {
                    // Add all active points into a list.
                    var activePointDict = new Dictionary<string, LoyaltyRewardPoint>();
                    foreach (var cardStatus in cardsStatus)
                    {
                        if (cardStatus != null && cardStatus.RewardPoints != null && cardStatus.RewardPoints.Count > 0)
                        {
                            foreach (var rewardPointStatus in cardStatus.RewardPoints)
                            {
                                if (rewardPointStatus.IsRedeemable && rewardPointStatus.ActivePoints > 0)
                                {
                                    if (activePointDict.ContainsKey(rewardPointStatus.RewardPointId))
                                    {
                                        activePointDict[rewardPointStatus.RewardPointId].ActivePoints += rewardPointStatus.ActivePoints;
                                    }
                                    else
                                    {
                                        activePointDict.Add(rewardPointStatus.RewardPointId, rewardPointStatus);
                                    }
                                }
                            }
                        }
                    }
    
                    activePointsList = activePointDict.Values.ToList();
    
                    // Sort reward points by redeem ranking (lower number means higher ranking).
                    activePointsList.Sort((a, b) => a.RedeemRanking.CompareTo(b.RedeemRanking));
                }
    
                return activePointsList;
            }
    
            /// <summary>
            /// Finds the best redeem line eligible to the sales line from the given redeem lines.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="redeemCurrency">The redeem currency.</param>
            /// <param name="redeemLines">The available redeem lines.</param>
            /// <returns>The best redeem line.</returns>
            private static LoyaltySchemeLineRedeem FindBestRedeemLine(RequestContext context, SalesLine salesLine, string redeemCurrency, ReadOnlyCollection<LoyaltySchemeLineRedeem> redeemLines)
            {
                decimal bestRedeemRate = 0;
                LoyaltySchemeLineRedeem bestRedeemLine = null;
                foreach (var redeemLine in redeemLines)
                {
                    if (redeemLine.FromRewardPointAmountQuantity > 0m
                        && redeemLine.ToRewardAmountQuantity > 0
                        && ((redeemLine.ToRewardType == LoyaltyRewardType.PaymentByAmount && !string.IsNullOrWhiteSpace(redeemLine.ToRewardAmountCurrency))
                            || redeemLine.ToRewardType == LoyaltyRewardType.PaymentByQuantity))
                    {
                        // Check if the redeem line is eligible to the sales line.
                        if (IsSalesLineEligibleForRedeemLine(context, salesLine, redeemLine))
                        {
                            // Calculates the redeem rate e.g. x points for 1 USD.
                            decimal redeemRateInRedeemCurrency = 0m;
                            switch (redeemLine.ToRewardType)
                            {
                                case LoyaltyRewardType.PaymentByAmount:
                                    // Convert the reward amount to redeem currency if it's different.
                                    if (!redeemLine.ToRewardAmountCurrency.Equals(redeemCurrency, StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Currency conversion
                                        var currencyValueRequest =
                                            new GetCurrencyValueServiceRequest(redeemLine.ToRewardAmountCurrency, redeemCurrency, redeemLine.ToRewardAmountQuantity);
                                        GetCurrencyValueServiceResponse currencyValueResponse = context.Execute<GetCurrencyValueServiceResponse>(currencyValueRequest);
                                        redeemLine.ToRewardAmountQuantity = currencyValueResponse.ConvertedAmount;
                                        redeemLine.ToRewardAmountCurrency = redeemCurrency;
                                    }
    
                                    redeemRateInRedeemCurrency = redeemLine.FromRewardPointAmountQuantity / redeemLine.ToRewardAmountQuantity;
                                    break;
    
                                case LoyaltyRewardType.PaymentByQuantity:
                                    decimal amountPerUnit = salesLine.TotalAmount / salesLine.Quantity;
                                    redeemRateInRedeemCurrency = redeemLine.FromRewardPointAmountQuantity / (redeemLine.ToRewardAmountQuantity * amountPerUnit);
                                    break;
                            }
    
                            if (bestRedeemRate == 0 || redeemRateInRedeemCurrency < bestRedeemRate)
                            {
                                bestRedeemRate = redeemRateInRedeemCurrency;
                                bestRedeemLine = redeemLine;
                            }
                        }
                    }
                }
    
                return bestRedeemLine;
            }
    
            /// <summary>
            /// Checks whether the sales line is eligible for the provided loyalty redeem line.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="redeemLine">The redeem line.</param>
            /// <returns>The result.</returns>
            private static bool IsSalesLineEligibleForRedeemLine(RequestContext context, SalesLine salesLine, LoyaltySchemeLineRedeem redeemLine)
            {
                if (salesLine == null)
                {
                    return redeemLine.ToRewardType == LoyaltyRewardType.PaymentByAmount;
                }
    
                bool isProductIndependent = redeemLine.ToCategoryRecordId == 0
                                                    && redeemLine.ToProductRecordId == 0
                                                    && redeemLine.ToVariantRecordId == 0;
    
                bool accept = false;
    
                if (isProductIndependent)
                {
                    accept = true;
                }
                else if (redeemLine.ToVariantRecordId != 0)
                {
                    if (salesLine.Variant != null)
                    {
                        accept = redeemLine.ToVariantRecordId == salesLine.Variant.DistinctProductVariantId;
                    }
                }
                else if (redeemLine.ToProductRecordId != 0)
                {
                    long productId = salesLine.MasterProductId != 0 ? salesLine.MasterProductId : salesLine.ProductId;
    
                    accept = redeemLine.ToProductRecordId == productId;
                }
                else if (redeemLine.ToCategoryRecordId != 0)
                {
                    // Check whether purchased product or variant belongs the category
                    long productId;
                    if (salesLine.Variant != null && salesLine.Variant.DistinctProductVariantId != 0)
                    {
                        productId = salesLine.Variant.DistinctProductVariantId;
                    }
                    else
                    {
                        productId = salesLine.ProductId;
                    }
    
                    var dataRequest = new CheckIfProductOrVariantAreInCategoryDataRequest(productId, redeemLine.ToCategoryRecordId);
                    accept = context.Runtime.Execute<SingleEntityDataServiceResponse<bool>>(dataRequest, context).Entity;
                }
    
                return accept;
            }
        }
    }
}
