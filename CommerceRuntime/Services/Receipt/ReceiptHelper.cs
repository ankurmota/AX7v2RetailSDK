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
        using System.Globalization;
        using System.Text;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Helper class for receipt service. Used mainly to handle formatting of hardcoded receipt templates.
        /// </summary>
        internal static class ReceiptHelper
        {
            /// <summary>
            /// The line format.
            /// </summary>
            private const string LineFormat = "{0}:{1}";
    
            /// <summary>
            /// The dotted padding.
            /// </summary>
            private const char DottedPadding = '.';
    
            /// <summary>
            /// The paper width.
            /// </summary>
            private const int PaperWidth = 55;
    
            /// <summary>
            /// The single line for receipt.
            /// </summary>
            private static readonly string SingleLine = string.Empty.PadLeft(55, '-');

            /// <summary>
            /// Prepare the header for a receipt.
            /// </summary>
            /// <param name="title">The title of the header.</param>
            /// <param name="reportLayout">The receipt string.</param>
            /// <param name="transaction">The transaction.</param>
            /// <param name="channelDateTimeOffset">The current time of the channel.</param>
            public static void PrepareReceiptHeader(string title, StringBuilder reportLayout, Transaction transaction, DateTimeOffset channelDateTimeOffset)
            {
                ThrowIf.NullOrWhiteSpace(title, "title");
                ThrowIf.Null(reportLayout, "reportLayout");
                ThrowIf.Null(transaction, "transaction");
    
                string channelDate = channelDateTimeOffset.ToString("d");
                string channelTime = channelDateTimeOffset.ToString("T");
    
                reportLayout.AppendLine();
    
                reportLayout.AppendLine(title); // Report Title
                reportLayout.AppendLine();
                reportLayout.AppendLine(string.Empty.PadLeft(55, '='));
    
                reportLayout.Append(FormatHeaderLine("<T:string_6149>", transaction.StaffId, true));
                reportLayout.Append("\t\t");
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6139>", channelDate, false));
                reportLayout.Append(FormatHeaderLine("<T:string_6138>", transaction.StoreId, true));
                reportLayout.Append("\t\t");
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6141>", channelTime, false));
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6140>", transaction.TerminalId, true));
                reportLayout.AppendLine(FormatHeaderLine("<F:string_6143>", string.Format(LineFormat, transaction.ShiftTerminalId, transaction.ShiftId), true));
            }

            /// <summary>
            /// Prepare the header for a receipt.
            /// </summary>
            /// <param name="reportLayout">The receipt string.</param>
            /// <param name="salesOrder">The sales order.</param>
            /// <param name="channelDateTimeOffset">The current time of the channel.</param>
            public static void PrepareGiftCardHeader(StringBuilder reportLayout, SalesOrder salesOrder, DateTimeOffset channelDateTimeOffset)
            {
                ThrowIf.Null(reportLayout, "reportLayout");
                ThrowIf.Null(salesOrder, "salesOrder");
    
                string channelDate = channelDateTimeOffset.ToString("d");
                string channelTime = channelDateTimeOffset.ToString("T");
    
                reportLayout.AppendLine();
    
                reportLayout.AppendLine("<T:string_6150>"); // Report Title
                reportLayout.AppendLine();
                reportLayout.AppendLine(string.Empty.PadLeft(55, '='));
    
                reportLayout.Append(FormatHeaderLine("<T:string_6149>", salesOrder.StaffId, true));
                reportLayout.Append("\t\t");
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6139>", channelDate, false));
                reportLayout.Append(FormatHeaderLine("<T:string_6138>", salesOrder.StoreId, true));
                reportLayout.Append("\t\t");
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6141>", channelTime, false));
                reportLayout.AppendLine(FormatHeaderLine("<T:string_6140>", salesOrder.TerminalId, true));
            }
    
            /// <summary>
            /// Format the tender line of Receipt.
            /// </summary>
            /// <param name="title">Title of tender item.</param>
            /// <param name="value">Value of tender item.</param>
            /// <returns>The formatted tender line string.</returns>
            public static string FormatTenderLine(string title, string value)
            {
                return string.Format(CultureInfo.CurrentCulture, LineFormat, title, value.PadLeft(35 - title.Length, '.'));
            }
    
            /// <summary>
            /// Format the header line of Receipt.
            /// </summary>
            /// <param name="titleResourceId">The title resource identifier.</param>
            /// <param name="value">Value part of line.</param>
            /// <param name="firstPart">True for first part of header, false for second.</param>
            /// <returns>The formatted header.</returns>
            public static string FormatHeaderLine(string titleResourceId, string value, bool firstPart)
            {
                string title = titleResourceId;
    
                if (firstPart)
                {
                    return string.Format(CultureInfo.CurrentCulture, LineFormat, title.PadRight(15, DottedPadding), value.PadLeft(8));
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture, LineFormat, title.PadRight(7, DottedPadding), value.PadLeft(10)).PadLeft(22);
                }
            }
    
            /// <summary>
            /// Append report line.
            /// </summary>
            /// <param name="receiptString">The receipt string.</param>
            /// <param name="title">Title string.</param>
            /// <param name="value">Value of tender item.</param>
            public static void AppendReportLine(StringBuilder receiptString, string title, object value)
            {
                receiptString.AppendLine(string.Format(CultureInfo.CurrentCulture, LineFormat, title, value.ToString().PadLeft(PaperWidth - title.Length - 2)));
            }
    
            /// <summary>
            /// Append a report title.
            /// </summary>
            /// <param name="receiptString">The receipt string.</param>
            /// <param name="titleResourceId">Resource Id.</param>
            public static void AppendReportLine(StringBuilder receiptString, string titleResourceId)
            {
                receiptString.AppendLine(titleResourceId);
                receiptString.AppendLine(SingleLine);
            }
    
            /// <summary>
            /// Append Report header line.
            /// </summary>
            /// <param name="receiptString">The receipt string.</param>
            /// <param name="titleResourceId">Resource identifier of the title part of the line.</param>
            /// <param name="value">Value part of the line.</param>
            /// <param name="firstPart">True for first part of header, false for second.</param>
            public static void AppendReportHeaderLine(StringBuilder receiptString, string titleResourceId, string value, bool firstPart)
            {
                int partWidth = PaperWidth / 2;
                int titleWidth = (int)(partWidth * 0.5);
                int valueWidth = (int)(partWidth * 0.4);
                string title = titleResourceId;
                string line = string.Format(CultureInfo.CurrentCulture, LineFormat, title.PadRight(titleWidth), value.PadLeft(valueWidth));
    
                if (firstPart)
                {
                    receiptString.Append(line.PadRight(partWidth));
                }
                else
                {
                    receiptString.AppendLine(line.PadLeft(partWidth));
                }
            }
        }
    }
}
