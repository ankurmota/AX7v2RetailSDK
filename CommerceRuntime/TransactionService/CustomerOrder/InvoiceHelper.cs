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
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using System.Xml.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Helper class to convert the getSalesInvoicesBySalesId and getSalesInvoiceDetail transaction server APIs.
        /// </summary>
        internal static class InvoiceHelper
        {
            /// <summary>
            /// Converts the invoice xml into SaleInvoice object.
            /// </summary>
            /// <param name="xmlInvoice">The xml that represents the invoices.</param>
            /// <returns>An array of invoices.</returns>
            /// <exception cref="XmlException">Thrown when xmlInvoice does not represent a valid XML text.</exception>
            public static SalesInvoice[] GetInvoicesFromXml(string xmlInvoice)
            {
                List<SalesInvoice> invoices = InvoiceHelper.ConvertXmlToSaleInvoice(xmlInvoice);
                return invoices.ToArray();
            }
    
            /// <summary>
            /// Converts the invoice xml into SaleOrder object.
            /// </summary>
            /// <param name="xmlInvoice">The xml that represents the invoices.</param>
            /// <param name="context">The CRT context.</param>
            /// <returns>The Sales Orders.</returns>
            /// <exception cref="XmlException">Thrown when xmlInvoice does not represent a valid XML text.</exception>
            public static SalesOrder GetSalesOrderFromXml(string xmlInvoice, RequestContext context)
            {
                SalesOrder order = InvoiceHelper.ConvertXmlToSaleOrder(xmlInvoice, context);
                return order;
            }
    
            /// <summary>
            /// Parse Invoices from Transaction Service response data.
            /// </summary>
            /// <param name="responseData">The transaction service response data.</param>
            /// <returns>Array of SalesInvoice objects parsed.</returns>
            internal static SalesInvoice[] GetInvoicesFromArray(ReadOnlyCollection<object> responseData)
            {
                List<SalesInvoice> invoices = new List<SalesInvoice>();
    
                if (responseData != null)
                {
                    SalesInvoice invoice;
    
                    // This array parsing is required to support the legacy implementation of the PaySalesInvoice transaction service API
    
                    // Each sales invoice consists of 8 properties in returned data bags
                    const int InvoiceLenth = 8;
                    for (int currentInvoiceStart = 0; currentInvoiceStart + InvoiceLenth <= responseData.Count; currentInvoiceStart += InvoiceLenth)
                    {
                        bool valid = Convert.ToBoolean(responseData[currentInvoiceStart]);
    
                        // the 2nd one is comment, which we don't care for one item
                        if (valid)
                        {
                            invoice = new SalesInvoice()
                            {
                                Id = ConvertToStringAtIndex(responseData, currentInvoiceStart + 2),
                                AmountPaid = ConvertToDecimalAtIndex(responseData, currentInvoiceStart + 3),
                                Amount = ConvertToDecimalAtIndex(responseData, currentInvoiceStart + 4),
                                Account = ConvertToStringAtIndex(responseData, currentInvoiceStart + 5),
                                Name = ConvertToStringAtIndex(responseData, currentInvoiceStart + 6),
                                InvoiceDate = ConvertToDateTimeAtIndex(responseData, currentInvoiceStart + 7),
                            };
    
                            invoices.Add(invoice);
                        }
                    }
                }
    
                return invoices.ToArray();
            }
    
            /// <summary>
            /// Converts the transaction service result XML into the SaleInvoice object.
            /// </summary>
            /// <param name="xmlInvoice">The XML representing the SaleInvoice.</param>
            /// <returns>SaleInvoice array.</returns>
            /// <exception cref="XmlException">Thrown when xmlInvoice does not represent a valid XML text.</exception>
            private static List<SalesInvoice> ConvertXmlToSaleInvoice(string xmlInvoice)
            {
                List<SalesInvoice> invoices = new List<SalesInvoice>();
                SalesInvoice invoice = null;
                SalesInvoiceLine invoiceLine = null;
                XDocument doc = null;
    
                if (!string.IsNullOrWhiteSpace(xmlInvoice))
                {
                    doc = XDocument.Parse(xmlInvoice);
    
                    if (doc != null && doc.Root != null)
                    {
                        foreach (XElement header in doc.Descendants("CustInvoiceJour"))
                        {
                            invoice = new SalesInvoice();
                            invoice.RecordId = long.Parse(header.Attribute("RecId").Value);
                            invoice.Id = header.Attribute("InvoiceId").Value;
                            invoice.SalesId = header.Attribute("SalesId").Value;
                            invoice.SalesType = (SalesInvoiceType)int.Parse(header.Attribute("SalesType").Value);
                            invoice.InvoiceDate = Utilities.ParseDateString(header.Attribute("InvoiceDate").Value, DateTime.UtcNow, DateTimeStyles.AssumeUniversal);
                            invoice.CurrencyCode = header.Attribute("CurrencyCode").Value;
                            invoice.Amount = decimal.Parse(header.Attribute("InvoiceAmount").Value);
                            invoice.Account = header.Attribute("InvoiceAccount").Value;
                            invoice.Name = header.Attribute("InvoicingName").Value;
                            invoice.SalesInvoiceLine = new List<SalesInvoiceLine>();
                            foreach (XElement detail in header.Elements("CustInvoiceTrans"))
                            {
                                invoiceLine = new SalesInvoiceLine();
                                invoiceLine.RecordId = long.Parse(detail.Attribute("RecId").Value);
                                invoiceLine.ItemId = detail.Attribute("ItemId").Value;
                                invoiceLine.ProductName = detail.Attribute("EcoResProductName").Value;
                                invoiceLine.InventDimensionId = detail.Attribute("InventDimId").Value;
                                invoiceLine.InventTransactionId = detail.Attribute("InventTransId").Value;
                                invoiceLine.Quantity = decimal.Parse(detail.Attribute("Qty").Value);
                                invoiceLine.Price = decimal.Parse(detail.Attribute("SalesPrice").Value);
                                invoiceLine.DiscountPercent = decimal.Parse(detail.Attribute("DiscPercent").Value);
                                invoiceLine.DiscountAmount = decimal.Parse(detail.Attribute("DiscAmount").Value);
                                invoiceLine.NetAmount = decimal.Parse(detail.Attribute("LineAmount").Value);
                                invoiceLine.SalesMarkup = decimal.Parse(detail.Attribute("SalesMarkup").Value);
                                invoiceLine.SalesTaxGroup = detail.Attribute("TaxGroup").Value;
                                invoiceLine.ItemTaxGroup = detail.Attribute("TaxItemGroup").Value;
                                invoiceLine.BatchId = detail.Attribute("InventBatchId").Value;
                                invoiceLine.Site = detail.Attribute("InventSiteId").Value;
                                invoiceLine.Warehouse = detail.Attribute("InventLocationId").Value;
                                invoiceLine.SerialId = detail.Attribute("InventSerialId").Value;
    
                                // Dimension details
                                invoiceLine.ColorId = InvoiceHelper.GetAttribute(detail, "InventColorId");
                                invoiceLine.ColorName = InvoiceHelper.GetAttribute(detail, "InventColorName");
                                invoiceLine.SizeId = InvoiceHelper.GetAttribute(detail, "InventSizeId");
                                invoiceLine.SizeName = InvoiceHelper.GetAttribute(detail, "InventSizeName");
                                invoiceLine.StyleId = InvoiceHelper.GetAttribute(detail, "InventStyleId");
                                invoiceLine.StyleName = InvoiceHelper.GetAttribute(detail, "InventStyleName");
                                invoiceLine.ConfigId = InvoiceHelper.GetAttribute(detail, "ConfigId");
                                invoiceLine.ConfigName = InvoiceHelper.GetAttribute(detail, "ConfigName");
                                invoice.SalesInvoiceLine.Add(invoiceLine);
                            }
    
                            invoices.Add(invoice);
                        }
                    }
                }
    
                return invoices;
            }
    
            /// <summary>
            /// Converts the transaction service result XML into the SaleOrder object.
            /// </summary>
            /// <param name="xmlInvoice">The XML representing the SaleInvoice.</param>
            /// <param name="context">The CRT context.</param>
            /// <returns>The SalesOrder.</returns>
            /// <exception cref="XmlException">Thrown when xmlInvoice does not represent a valid XML text.</exception>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Required by deserialization logic.")]
            private static SalesOrder ConvertXmlToSaleOrder(string xmlInvoice, RequestContext context)
            {
                SalesOrder order = null;
                XDocument doc = null;

                DateTimeOffset currentChannelDate = context.GetNowInChannelTimeZone();                
    
                if (!string.IsNullOrWhiteSpace(xmlInvoice))
                {
                    doc = XDocument.Parse(xmlInvoice);
    
                    if (doc != null && doc.Root != null)
                    {
                        foreach (XElement header in doc.Descendants("CustInvoiceJour"))
                        {
                            string invoiceId = header.Attribute("InvoiceId").Value;
                            order = new SalesOrder();
    
                            DateTimeOffset invoiceDate = Utilities.ParseDateString(header.Attribute("CreatedDateTime").Value, currentChannelDate.ToUniversalTime().DateTime, DateTimeStyles.AssumeUniversal);
    
                            // General header properties
                            order.SalesId = header.Attribute("SalesId").Value;
                            order.TransactionType = SalesTransactionType.CustomerOrder;
                            order.CustomerOrderMode = CustomerOrderMode.Return;
                            order.CartType = CartType.CustomerOrder;
                            order.CustomerOrderType = CustomerOrderType.SalesOrder;
                            order.Status = SalesStatus.Invoiced;
    
                            var currentStore = context.GetOrgUnit();
                            if (currentStore != null)
                            {
                                order.StoreId = currentStore.OrgUnitNumber;
                                order.InventoryLocationId = currentStore.InventoryLocationId;
                                order.ShippingAddress = currentStore.OrgUnitAddress != null ? currentStore.OrgUnitAddress : new Address();
                            }
    
                            order.CustomerId = header.Attribute("InvoiceAccount").Value;
    
                            order.DeliveryMode = context.GetChannelConfiguration().PickupDeliveryModeCode;
                            order.BeginDateTime = invoiceDate;
                            order.IsTaxIncludedInPrice = context.GetChannelConfiguration().PriceIncludesSalesTax;
                            order.RequestedDeliveryDate = currentChannelDate;
                            order.QuotationExpiryDate = currentChannelDate;
                            order.Comment = string.Empty;
                            order.TotalManualDiscountAmount = Convert.ToDecimal(header.Attribute("TotalManualDiscountAmount").Value);
                            order.TotalManualDiscountPercentage = Convert.ToDecimal(header.Attribute("TotalManualDiscountPercentage").Value);
                            order.LoyaltyCardId = header.Attribute("LoyaltyCardId").Value;
                            
                            //Ankur DEMO4 : Add extension property HasReturns
                            order.SetProperty("HasReturns", header.Attribute("HasReturns").Value == "true");
                            //DEMO4 END

                            order.SalesLines = new Collection<SalesLine>();
    
                            // Items
                            int lineId = 0;
                            foreach (XElement detail in header.Elements("CustInvoiceTrans"))
                            {
                                ParseAndCreateSalesLine(detail, order, lineId, invoiceId, invoiceDate, context);
                                lineId++;
                            }
    
                            // All taxable items must use invoice date for tax calculation purposes
                            foreach (TaxableItem taxableItem in order.ChargeLines)
                            {
                                taxableItem.BeginDateTime = invoiceDate;
                            }
                        }
                    }
                }
    
                return order;
            }
    
            /// <summary>
            /// Creates a sales line from the xml element.
            /// </summary>
            /// <param name="xmlItemElement">The sales line line xml element.</param>
            /// <param name="order">The sales order.</param>
            /// <param name="lineId">The sales line identifier.</param>
            /// <param name="invoiceId">The invoice identifier.</param>
            /// <param name="invoiceBeginDate">The invoice begin date.</param>
            /// <param name="context">The request context.</param>
            private static void ParseAndCreateSalesLine(XElement xmlItemElement, SalesOrder order, int lineId, string invoiceId, DateTimeOffset invoiceBeginDate, RequestContext context)
            {
                SalesLine lineItem = new SalesLine();
                lineItem.LineId = lineId.ToString();
                lineItem.RecordId = long.Parse(xmlItemElement.Attribute("RecId").Value);
                lineItem.ItemId = xmlItemElement.Attribute("ItemId").Value;
    
                lineItem.Description = xmlItemElement.Attribute("EcoResProductName").Value;
                lineItem.InventoryDimensionId = xmlItemElement.Attribute("InventDimId").Value;
                lineItem.ReturnInventTransId = xmlItemElement.Attribute("InventTransId").Value;
                lineItem.ReturnTransactionId = invoiceId;
    
                decimal quantityInvoiced = decimal.Parse(xmlItemElement.Attribute("Qty").Value);
                lineItem.Quantity = decimal.Negate(quantityInvoiced);
                lineItem.UnitOfMeasureSymbol = xmlItemElement.Attribute("SalesUnit").Value;
                lineItem.ReturnQuantity = lineItem.Quantity;
                lineItem.QuantityInvoiced = quantityInvoiced;
                lineItem.Price = decimal.Parse(xmlItemElement.Attribute("SalesPrice").Value);
                lineItem.LineManualDiscountPercentage = decimal.Parse(xmlItemElement.Attribute("DiscPercent").Value);
                lineItem.DiscountAmount = decimal.Parse(xmlItemElement.Attribute("DiscAmount").Value);
                lineItem.NetAmount = decimal.Parse(xmlItemElement.Attribute("LineAmount").Value);
                lineItem.TaxAmount = decimal.Parse(xmlItemElement.Attribute("LineAmountTax").Value);
    
                lineItem.PeriodicDiscount = Convert.ToDecimal(xmlItemElement.Attribute("PeriodicDiscount").Value);
                lineItem.PeriodicPercentageDiscount = Convert.ToDecimal(xmlItemElement.Attribute("PeriodicPercentageDiscount").Value);
                lineItem.LineDiscount = Convert.ToDecimal(xmlItemElement.Attribute("LineDscAmount").Value);
                lineItem.TotalDiscount = Convert.ToDecimal(xmlItemElement.Attribute("TotalDiscount").Value);
                lineItem.TotalPercentageDiscount = Convert.ToDecimal(xmlItemElement.Attribute("TotalPctDiscount").Value);
                lineItem.LineManualDiscountAmount = Convert.ToDecimal(xmlItemElement.Attribute("LineManualDiscountAmount").Value);
                lineItem.LineManualDiscountPercentage = Convert.ToDecimal(xmlItemElement.Attribute("LineManualDiscountPercentage").Value);
    
                // lineItem.SalesMarkup = decimal.Parse(detail.Attribute("SalesMarkup").Value);
                lineItem.SalesTaxGroupId = xmlItemElement.Attribute("TaxGroup").Value;
                lineItem.ItemTaxGroupId = xmlItemElement.Attribute("TaxItemGroup").Value;
                lineItem.BatchId = xmlItemElement.Attribute("InventBatchId").Value;
                lineItem.SerialNumber = xmlItemElement.Attribute("InventSerialId").Value;
                lineItem.BeginDateTime = invoiceBeginDate;
    
                lineItem.DeliveryMode = order.DeliveryMode;
                lineItem.RequestedDeliveryDate = order.RequestedDeliveryDate;
                lineItem.ShippingAddress = order.ShippingAddress.Clone<Address>();
                lineItem.InventoryDimensionId = xmlItemElement.Attribute("InventDimensionId").Value;               
    
                Utilities.SetUpVariantAndProduct(context, lineItem.InventoryDimensionId, lineItem.ItemId, lineItem);
    
                foreach (XElement discountDetail in xmlItemElement.Elements("Discounts").Elements("Discount"))
                {
                    ParseAndCreateDiscountLine(discountDetail, lineItem);
                }
    
                // All taxable items need to use invoice date to have taxes calculated properly
                foreach (TaxableItem taxableItem in lineItem.ChargeLines)
                {
                    taxableItem.BeginDateTime = invoiceBeginDate;
                }
    
                order.SalesLines.Add(lineItem);
            }
    
            private static void ParseAndCreateDiscountLine(XElement discountDetail, SalesLine lineItem)
            {
                DiscountLine lineDiscount = new DiscountLine();
    
                lineDiscount.EffectiveAmount = Convert.ToDecimal(discountDetail.Attribute("Amount").Value);
                lineDiscount.Amount = Convert.ToDecimal(discountDetail.Attribute("DiscountAmount").Value);
    
                lineDiscount.DiscountLineType = (DiscountLineType)Convert.ToInt32(discountDetail.Attribute("DiscountOriginType").Value);
                lineDiscount.DiscountCode = discountDetail.Attribute("DiscountCode").Value;
                lineDiscount.CustomerDiscountType = (CustomerDiscountType)Convert.ToInt32(discountDetail.Attribute("CustomerDiscountType").Value);
                lineDiscount.ManualDiscountType = (ManualDiscountType)Convert.ToInt32(discountDetail.Attribute("ManualDiscountType").Value);
                lineDiscount.OfferId = discountDetail.Attribute("PeriodicDiscountOfferId").Value;
                lineDiscount.Percentage = Convert.ToDecimal(discountDetail.Attribute("Percentage").Value);
                lineDiscount.DealPrice = Convert.ToDecimal(discountDetail.Attribute("DealPrice").Value);
    
                lineItem.DiscountLines.Add(lineDiscount);
            }
    
            /// <summary>
            /// Gets the attribute value if exists.
            /// </summary>
            /// <param name="element">The XML element.</param>
            /// <param name="attributeName">The required attribute.</param>
            /// <returns>The value of the attribute.</returns>
            private static string GetAttribute(XElement element, string attributeName)
            {
                string attributeValue = string.Empty;
                XAttribute attribute = element.Attribute(attributeName);
                if (attribute != null)
                {
                    attributeValue = attribute.Value;
                }
    
                return attributeValue;
            }
    
            /// <summary>
            /// Converts item of index in list to string.
            /// </summary>
            /// <param name="list">The list.</param>
            /// <param name="index">The index.</param>
            /// <returns>The string result.</returns>
            private static string ConvertToStringAtIndex(IList list, int index)
            {
                try
                {
                    return Convert.ToString(list[index]);
                }
                catch
                {
                    return string.Empty;
                }
            }
    
            /// <summary>
            /// Converts item of index in list to decimal.
            /// </summary>
            /// <param name="list">The list.</param>
            /// <param name="index">The index.</param>
            /// <returns>The decimal results.</returns>
            private static decimal ConvertToDecimalAtIndex(IList list, int index)
            {
                try
                {
                    return Convert.ToDecimal(list[index]);
                }
                catch
                {
                    return 0.00M;
                }
            }
    
            /// <summary>
            /// Converts item of index in list to DateTime.
            /// </summary>
            /// <param name="list">The list.</param>
            /// <param name="index">The index.</param>
            /// <returns>The DateTime results.</returns>
            private static DateTime ConvertToDateTimeAtIndex(IList list, int index)
            {
                try
                {
                    return Convert.ToDateTime(list[index]);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }
    }
}
