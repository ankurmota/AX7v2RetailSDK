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
    namespace Commerce.Runtime.Services.CustomerOrder
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;    

        /// <summary>
        /// Helper class to serialize a sales transaction to be send to transaction server APIs.
        /// </summary>
        internal static class Utilities
        {
            // Date time format based on ISO8601 standard
            // AX DateTimeUtl::ToStr(aDateTime) method also outputs the same format
            // e.g. 2006-07-14T19:05:49
            private const string FixedDateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
            private const string FixedDateFormat = "yyyy-MM-dd";
            private const long EmptyCustomerRecId = 0;
            private const long EmptyPartyRecId = 0;

            /// <summary>
            /// Get UTC string version of the date.
            /// </summary>
            /// <param name="date">Date to be converted.</param>
            /// <returns>A string representing the date in AX format.</returns>
            internal static string ConvertDateToAxString(DateTimeOffset? date)
            {
                return IsDateNullOrDefaultValue(date) ? string.Empty : date.Value.DateTime.ToString(FixedDateFormat, CultureInfo.InvariantCulture);
            }
    
            /// <summary>
            /// Returns whether a date is null or has the default value set to it.
            /// </summary>
            /// <param name="dateTime">The date to be validated.</param>
            /// <returns>Whether a date is null or has the default value set to it.</returns>
            internal static bool IsDateNullOrDefaultValue(DateTimeOffset? dateTime)
            {
                // SqlDateTime.MinValue.Value is the value used when the property bag of a sales transaction for a date is not set
                return !dateTime.HasValue || dateTime.Value.DateTime == System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            }
    
            /// <summary>
            /// Gets the SalesStatus from SalesOrderStatus and DocumentStatus.
            /// </summary>
            /// <param name="salesOrderStatus">The sales order status.</param>
            /// <param name="docStatus">The document status.</param>
            /// <returns>The SalesStatus.</returns>
            internal static SalesStatus GetSalesStatus(SalesOrderStatus salesOrderStatus, DocumentStatus docStatus)
            {
                SalesStatus salesStatus = GetSalesStatus(salesOrderStatus);
    
                switch (salesStatus)
                {
                    case SalesStatus.Unknown:
                    case SalesStatus.Created:
                        SalesStatus documentStatus = GetSalesStatus(docStatus);
                        if (documentStatus == SalesStatus.Unknown)
                        {
                            return salesStatus;
                        }
    
                        return documentStatus;
                    default:
                        return salesStatus;
                }
            }

            /// <summary>
            /// Convert SalesOrderStatus to SalesStatus.
            /// </summary>
            /// <param name="salesOrderStatus">The sales order status.</param>
            /// <returns>The converted sales status.</returns>
            internal static SalesStatus GetSalesStatus(SalesOrderStatus salesOrderStatus)
            {
                switch (salesOrderStatus)
                {
                    case SalesOrderStatus.Backorder: return SalesStatus.Created;
                    case SalesOrderStatus.Delivered: return SalesStatus.Delivered;
                    case SalesOrderStatus.Invoiced: return SalesStatus.Invoiced;
                    case SalesOrderStatus.Canceled: return SalesStatus.Canceled;
                    default: return SalesStatus.Unknown;
                }
            }
    
            /// <summary>
            /// Convert DocumentStatus to SalesStatus.
            /// </summary>
            /// <param name="docStatus">The document status.</param>
            /// <returns>The returned sales status.</returns>
            internal static SalesStatus GetSalesStatus(DocumentStatus docStatus)
            {
                switch (docStatus)
                {
                    case DocumentStatus.None: return SalesStatus.Created;
                    case DocumentStatus.PickingList: return SalesStatus.Processing;
                    case DocumentStatus.PackingSlip: return SalesStatus.Delivered;
                    case DocumentStatus.Invoice: return SalesStatus.Invoiced;
                    case DocumentStatus.Canceled: return SalesStatus.Canceled;
                    case DocumentStatus.Lost: return SalesStatus.Lost;
                    default: return SalesStatus.Unknown;
                }
            }

            /// <summary>
            /// Convert SalesQuotationStatus to SalesStatus.
            /// </summary>
            /// <param name="quoteStatus">The sales quotation status.</param>
            /// <returns>The returned sales status.</returns>
            internal static SalesStatus GetSalesStatus(SalesQuotationStatus quoteStatus)
            {
                switch (quoteStatus)
                {
                    case SalesQuotationStatus.Created: return SalesStatus.Created;
                    case SalesQuotationStatus.Confirmed: return SalesStatus.Confirmed;
                    case SalesQuotationStatus.Sent: return SalesStatus.Canceled;
                    case SalesQuotationStatus.Canceled: return SalesStatus.Canceled;
                    case SalesQuotationStatus.Lost: return SalesStatus.Lost;
                    default: return SalesStatus.Unknown;
                }
            }

            /// <summary>
            /// Parse the date from an AX-sent date string.
            /// </summary>
            /// <param name="valueToParse">The date string to be parsed.</param>
            /// <param name="defaultDate">The default date format.</param>
            /// <param name="dateTimeStyle">The date time style.</param>
            /// <returns>The converted date time.</returns>
            internal static DateTime ParseDateString(string valueToParse, DateTime defaultDate, DateTimeStyles dateTimeStyle = DateTimeStyles.AssumeLocal)
            {
                DateTimeFormatInfo info = new DateTimeFormatInfo();
                DateTime result;

                if (string.IsNullOrWhiteSpace(valueToParse) ||
                    (!DateTime.TryParseExact(valueToParse, FixedDateFormat, info, dateTimeStyle, out result)
                     && !DateTime.TryParseExact(valueToParse, FixedDateTimeFormat, info, dateTimeStyle, out result)))
                {
                    return defaultDate;
                }

                return result;
            }

            /// <summary>
            /// Parse the <see cref="DateTimeOffset"/> from an AX-sent date string.
            /// </summary>
            /// <param name="valueToParse">The date string to be parsed.</param>
            /// <param name="defaultDate">The default date.</param>
            /// <param name="defaultTimezoneOffset">The time zone.</param>
            /// <returns>The converted date time offset.</returns>
            internal static DateTimeOffset ParseDateStringAsDateTimeOffset(string valueToParse, DateTime defaultDate, TimeSpan defaultTimezoneOffset)
            {
                DateTime date = Utilities.ParseDateString(valueToParse, defaultDate.Date, DateTimeStyles.None);
    
                // because headquarters do not store date time, we take dates as representing the last second of the day
                return new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, defaultTimezoneOffset);
            }

            /// <summary>
            /// Setup the variant data for the sale line.
            /// </summary>
            /// <param name="context">The CRT context.</param>
            /// <param name="inventDimensionId">The invent dimension identifier.</param>
            /// <param name="itemId">The item identifier.</param>
            /// <param name="lineItem">The sales line to update.</param>
            internal static void SetUpVariantAndProduct(RequestContext context, string inventDimensionId, string itemId, SalesLine lineItem)
            {
                // Get the product corresponding to this item.
                // It is expected to get product details
                // even if the product is not assorted, as long as
                // respected product exists on headquarters.                
                long channelId = context.GetPrincipal().ChannelId;

                // Search product with itemId and InventDimensionId.          
                var itemAndInventDimIdCombination = new List<ProductLookupClause> { new ProductLookupClause(itemId, inventDimensionId) };
                var productsRequest = new GetProductsServiceRequest(channelId, itemAndInventDimIdCombination, QueryResultSettings.AllRecords)
                {
                    SearchLocation = SearchLocation.All
                };             
                
                var product = context.Execute<GetProductsServiceResponse>(productsRequest).Products.Results.OrderBy(p => p.IsRemote).FirstOrDefault();

                if (product == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindListing,
                        string.Format("Unable to find listing for item {0}, variant {1}, channel {2}.", lineItem.ItemId, lineItem.InventoryDimensionId, context.GetPrincipal().ChannelId));
                }
    
                // Populate variant information
                // Processing variants.
                if (!string.IsNullOrWhiteSpace(inventDimensionId))
                {
                    lineItem.Variant = null;
    
                    if (product.IsDistinct)
                    {
                        lineItem.Variant = ProductVariant.ConvertFrom(product);
                    }
    
                    if (lineItem.Variant == null)
                    {
                        throw new DataValidationException(
                             DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnableToFindVariant,
                             string.Format("Unable to find variant for invent Dimension id {0}, channel {1}.", inventDimensionId, context.GetPrincipal().ChannelId));
                    }
    
                    lineItem.InventoryDimensionId = lineItem.Variant.InventoryDimensionId;
                    lineItem.ProductId = lineItem.Variant.DistinctProductVariantId;
                }
                else
                {
                    lineItem.InventoryDimensionId = string.Empty;
                    lineItem.ProductId = product.RecordId;
                }
            }

            /// <summary>
            /// Download customer from AX.
            /// </summary>
            /// <param name="requestContext">The CRT context.</param>
            /// <param name="customerAccountNumber">The customer account number.</param>
            internal static void DownloadCustomerData(RequestContext requestContext, string customerAccountNumber)
            {
                // Download customer from AX.
                var customerDownloadRequest = new DownloadCustomerRealtimeRequest(customerAccountNumber);
                requestContext.Execute<NullResponse>(customerDownloadRequest);
            }
        }
    }
}
