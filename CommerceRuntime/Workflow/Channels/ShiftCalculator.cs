/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
    
        /// <summary>
        /// Shift calculator helps to perform end of day calculation.
        /// </summary>
        internal static class ShiftCalculator
        {
            /// <summary>
            /// Calculate the totals, number of transactions happened in the current shift.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="currentShift">Current shift object.</param>
            /// <param name="shiftTerminalId">Shift terminal Identifier.</param>
            /// <param name="shiftId">Shift identifier.</param>
            public static void Calculate(RequestContext context, Shift currentShift, string shiftTerminalId, long shiftId)
            {
                GetEndOfDayShiftDetailsDataRequest getEndOfDayShiftDetailsDataRequest = new GetEndOfDayShiftDetailsDataRequest(shiftTerminalId, shiftId, context.GetChannelConfiguration().PriceIncludesSalesTax);
                Shift endOfDayShiftDetails = context.Runtime.Execute<SingleEntityDataServiceResponse<Shift>>(getEndOfDayShiftDetailsDataRequest, context).Entity;
                
                // Sets the sales totals.
                SetShiftSalesTotals(currentShift, endOfDayShiftDetails);
    
                // Set the tender lines.
                SetShiftTenderLine(currentShift, endOfDayShiftDetails);
    
                // Set the account lines.
                SetShiftAccountLines(currentShift, endOfDayShiftDetails);
    
                // Calculates the shift counts.
                GetShiftTransactionsCountDataRequest getShiftTransactionCountsDataRequest = new GetShiftTransactionsCountDataRequest(shiftTerminalId, shiftId);
                Shift shiftCounts = context.Runtime.Execute<SingleEntityDataServiceResponse<Shift>>(getShiftTransactionCountsDataRequest, context).Entity;
    
                // Set the retail transaction counts.
                SetRetailTransactionCount(currentShift, shiftCounts);
    
                // Calculates the tender line tender amounts.
                GetShiftTenderedAmountDataRequest getShiftTenderedAmountDataRequest = new GetShiftTenderedAmountDataRequest(shiftTerminalId, shiftId, QueryResultSettings.AllRecords);
                var shiftTenderAmount = context.Runtime.Execute<EntityDataServiceResponse<ShiftTenderLine>>(getShiftTenderedAmountDataRequest, context).PagedEntityCollection.Results;
    
                // Set the tender line tender amounts.
                SetShiftTenderLineTenderAmounts(currentShift, shiftTenderAmount);
            }
    
            /// <summary>
            /// Sets the totals and counts of the sales transaction.
            /// </summary>
            /// <param name="currentShift">Current shift data.</param>
            /// <param name="shiftTotals">Calculated shift totals from end of day.</param>
            private static void SetShiftSalesTotals(Shift currentShift, Shift shiftTotals)
            {
                currentShift.SalesTotal = shiftTotals.SalesTotal;
                currentShift.ReturnsTotal = shiftTotals.ReturnsTotal;
                currentShift.DiscountTotal = shiftTotals.DiscountTotal;
                currentShift.TaxTotal = shiftTotals.TaxTotal;
                currentShift.SaleTransactionCount = shiftTotals.SaleTransactionCount;
                currentShift.PaidToAccountTotal = shiftTotals.PaidToAccountTotal;
                currentShift.SuspendedTransactionCount = shiftTotals.SuspendedTransactionCount;
            }
    
            /// <summary>
            /// Sets the current shift tender line details by calculating the totals from daily operation transactions.
            /// </summary>
            /// <param name="currentShift">Current shift data.</param>
            /// <param name="endOfDayShiftDetails">Calculated shift details from end of day.</param>
            private static void SetShiftTenderLine(Shift currentShift, Shift endOfDayShiftDetails)
            {
                currentShift.TenderLines.AddRange(endOfDayShiftDetails.TenderLines);
            }
    
            /// <summary>
            /// Sets the current shift account line details by calculating the totals from income expense transactions.
            /// </summary>
            /// <param name="currentShift">Current shift data.</param>
            /// <param name="endOfDayShiftDetails">Calculated shift details from end of day.</param>
            private static void SetShiftAccountLines(Shift currentShift, Shift endOfDayShiftDetails)
            {
                currentShift.AccountLines.AddRange(endOfDayShiftDetails.AccountLines);
            }
    
            /// <summary>
            /// Sets the tender line tender amounts.
            /// </summary>
            /// <param name="currentShift">Current shift data.</param>
            /// <param name="shiftTenderLines">Shift tender lines that contains the end of day amount.</param>
            private static void SetShiftTenderLineTenderAmounts(Shift currentShift, IEnumerable<ShiftTenderLine> shiftTenderLines)
            {
                foreach (var shiftTenderLine in shiftTenderLines)
                {
                    ShiftTenderLine tenderLine = currentShift.TenderLines.FindOrCreate(shiftTenderLine);
    
                    tenderLine.CountingRequired = shiftTenderLine.CountingRequired;
                    tenderLine.TenderCurrency = shiftTenderLine.TenderCurrency;
                    tenderLine.ChangeLine = shiftTenderLine.ChangeLine;
    
                    if (tenderLine.ChangeLine)
                    {
                        tenderLine.ChangeAmountOfTenderCurrency = decimal.Negate(shiftTenderLine.TenderedAmountOfTenderCurrency);
                        tenderLine.ChangeAmountOfStoreCurrency = decimal.Negate(shiftTenderLine.TenderedAmountOfStoreCurrency);
                    }
                    else
                    {
                        tenderLine.TenderedAmountOfTenderCurrency = shiftTenderLine.TenderedAmountOfTenderCurrency;
                        tenderLine.TenderedAmountOfStoreCurrency = shiftTenderLine.TenderedAmountOfStoreCurrency;
                        tenderLine.Count = shiftTenderLine.Count;
                    }
                }
            }
    
            /// <summary>
            /// Find or create a shift tender line.
            /// </summary>
            /// <param name="shiftTenderLines">Shift tender lines.</param>
            /// <param name="tenderLine">Tender line.</param>
            /// <returns>Returns the shift tender line.</returns>
            private static ShiftTenderLine FindOrCreate(this IList<ShiftTenderLine> shiftTenderLines, ShiftTenderLine tenderLine)
            {
                string tenderTypeId = tenderLine.TenderTypeId;
                string tenderCurrency = tenderLine.TenderCurrency;
                string tenderTypeName = tenderLine.TenderTypeName;
    
                ShiftTenderLine shiftTenderLine = shiftTenderLines.FirstOrDefault(p => p.TenderTypeId == tenderTypeId && p.TenderCurrency == tenderCurrency);
    
                if (shiftTenderLine == null)
                {
                    shiftTenderLine = new ShiftTenderLine
                    {
                        TenderTypeId = tenderTypeId,
                        CardTypeId = string.Empty,
                        TenderCurrency = tenderCurrency,
                        TenderTypeName = tenderTypeName
                    };

                    shiftTenderLines.Add(shiftTenderLine);
                }
    
                return shiftTenderLine;
            }
    
            /// <summary>
            /// Sets the number of retail transactions happened in the current shift.
            /// Sets the rounding totals of the current shift.
            /// Sets the first created date/time of the transaction.
            /// </summary>
            /// <param name="currentShift">Current Shift data.</param>
            /// <param name="endOfDayShift">End of day shifts.</param>
            private static void SetRetailTransactionCount(Shift currentShift, Shift endOfDayShift)
            {
                currentShift.VoidTransactionCount = endOfDayShift.VoidTransactionCount;
                currentShift.NoSaleTransactionCount = endOfDayShift.NoSaleTransactionCount;
                currentShift.RoundedAmountTotal = endOfDayShift.RoundedAmountTotal;
                currentShift.TransactionCount = endOfDayShift.TransactionCount;
                currentShift.CustomerCount = endOfDayShift.CustomerCount;
            }
        }
    }
}
