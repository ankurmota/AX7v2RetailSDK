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
        using System.Collections.ObjectModel;
        using System.Collections.Specialized;
        using System.Globalization;
        using System.IO;
        using System.Xml;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Represents the customer order information. Used in serializing and transmitting the order to AX.
        /// </summary>
        [Serializable]
        [XmlRoot("CustomerOrder")]
        public class CustomerOrderInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerOrderInfo"/> class.
            /// </summary>
            public CustomerOrderInfo()
            {
                this.Items = new ItemInfoCollection();
                this.Charges = new ChargeInfoCollection();
                this.Payments = new PaymentInfoCollection();
                this.Affiliations = new AffiliationInfoCollection();
                this.DiscountCodes = new StringCollection();
                this.Taxes = new TaxInfoCollection();
                this.ExtensionProperties = new Collection<CommerceProperty>();
            }
    
            /// <summary>
            /// Gets or sets the identifier of the order/quote used during edition flow.
            /// </summary>
            [XmlElement("Id")]
            public string Id { get; set; }
    
            /// <summary>
            /// Gets or sets the quotation identifier used when converting a customer quote to a sales order.
            /// </summary>
            [XmlElement("QuotationId")]
            public string QuotationId { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether or not to Automatically create a pick list for the order.
            /// </summary>
            [XmlElement("AutoPickOrder")]
            public bool AutoPickOrder { get; set; }
    
            /// <summary>
            /// Gets or sets the customer order type.
            /// </summary>
            [XmlElement("OrderType")]
            public CustomerOrderType OrderType { get; set; }
    
            /// <summary>
            /// Gets or sets the status of the order.
            /// </summary>
            [XmlElement("Status")]
            public int Status { get; set; }
    
            /// <summary>
            /// Gets or sets the document status.
            /// </summary>
            [XmlElement("DocumentStatus")]
            public int DocumentStatus { get; set; }
    
            /// <summary>
            /// Gets or sets the customer account number.
            /// </summary>
            [XmlElement("CustomerAccount")]
            public string CustomerAccount { get; set; }
    
            /// <summary>
            /// Gets or sets the channel record id.
            /// </summary>
            [XmlElement("ChannelRecordId")]
            public string ChannelRecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the address record identifier.
            /// </summary>
            [XmlElement("AddressRecord")]
            public string AddressRecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the site identifier.
            /// </summary>
            [XmlElement("InventSiteId")]
            public string SiteId { get; set; }
    
            /// <summary>
            /// Gets or sets the warehouse identifier.
            /// </summary>
            [XmlElement("InventLocationId")]
            public string WarehouseId { get; set; }
    
            /// <summary>
            /// Gets or sets the identifier of the current store.
            /// </summary>
            [XmlElement("StoreId")]
            public string StoreId { get; set; }
    
            /// <summary>
            /// Gets or sets the terminal identifier.
            /// </summary>
            [XmlElement("TerminalId")]
            public string TerminalId { get; set; }
    
            /// <summary>
            /// Gets or sets the transaction id.
            /// </summary>
            [XmlElement("TransactionId")]
            public string TransactionId { get; set; }

            /// <summary>
            /// Gets or sets the value for tax included in price.
            /// </summary>
            [XmlElement("IsTaxIncludedInPrice")]
            public string IsTaxIncludedInPrice { get; set; }

            /// <summary>
            /// Gets or sets rounding difference amount.
            /// </summary>
            [XmlElement("RoundingDifference")]
            public decimal RoundingDifference { get; set; }

            /// <summary>
            /// Gets or sets total manual discount amount.
            /// </summary>
            [XmlElement("TotalManualDiscountAmount")]
            public decimal TotalManualDiscountAmount { get; set; }
    
            /// <summary>
            /// Gets or sets total manual discount amount.
            /// </summary>
            [XmlElement("TotalManualDiscountPercentage")]
            public decimal TotalManualDiscountPercentage { get; set; }
    
            /// <summary>
            /// Gets or sets the expiry date in string format.
            /// </summary>
            [XmlElement("ExpiryDate")]
            public string ExpiryDateString { get; set; }
    
            /// <summary>
            /// Gets or sets the creation date in string format.
            /// </summary>
            [XmlElement("CreationDate")]
            public string CreationDateString { get; set; }
    
            /// <summary>
            /// Gets or sets the local hour of day when order is created.
            /// </summary>
            [XmlElement("HourOfDay")]
            public int LocalHourOfDay { get; set; }
    
            /// <summary>
            /// Gets or sets the delivery mode.
            /// </summary>
            [XmlElement("DeliveryMode")]
            public string DeliveryMode { get; set; }
    
            /// <summary>
            /// Gets or sets the expiry Date in string format.
            /// </summary>
            [XmlElement("RequestedDeliveryDate")]
            public string RequestedDeliveryDateString { get; set; }
    
            /// <summary>
            /// Gets or sets the transaction comment.
            /// </summary>
            [XmlElement("Comment")]
            public string Comment { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the prepayment (deposit) amount was overridden.
            /// </summary>
            [XmlElement("PrepaymentAmountOverridden")]
            public bool PrepaymentAmountOverridden { get; set; }
    
            /// <summary>
            /// Gets or sets the amount of prepayment that is currently applied to this order.
            /// </summary>
            [XmlElement("PrepaymentAmountApplied")]
            public decimal PrepaymentAmountApplied { get; set; }
    
            /// <summary>
            /// Gets or sets the amount that has been previously invoiced (picked-up).
            /// </summary>
            [XmlElement("PreviouslyInvoicedAmount")]
            public decimal PreviouslyInvoicedAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the information to void a prior authorization.
            /// </summary>
            [XmlElement("PreAuthorization")]
            public Preauthorization Preauthorization { get; set; }
    
            /// <summary>
            /// Gets or sets the staff identifier for the sales person.
            /// </summary>
            /// <remarks>
            /// This is not the operator identifier.
            /// </remarks>
            [XmlElement("SalespersonStaffId")]
            public string SalespersonStaffId { get; set; }
    
            /// <summary>
            /// Gets or sets the name of the sales person.
            /// </summary>
            [XmlElement("SalespersonName")]
            public string SalespersonName { get; set; }
    
            /// <summary>
            /// Gets or sets the currency code for the order.
            /// </summary>
            /// <remarks>
            /// This is usually the store's currency code.
            /// </remarks>
            [XmlElement("CurrencyCode")]
            public string CurrencyCode { get; set; }
    
            /// <summary>
            /// Gets or sets the return reason code identifier.
            /// </summary>
            [XmlElement("ReturnReasonCodeId")]
            public string ReturnReasonCodeId { get; set; }
    
            /// <summary>
            /// Gets or sets the loyalty card identifier.
            /// </summary>
            [XmlElement("LoyaltyCardId")]
            public string LoyaltyCardId { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the customer order has any posted loyalty tender line.
            /// </summary>
            [XmlElement("HasLoyaltyPayment")]
            public bool HasLoyaltyPayment { get; set; }
    
            /// <summary>
            /// Gets or sets the channel reference identifier.
            /// </summary>
            [XmlElement("ChannelReferenceId")]
            public string ChannelReferenceId { get; set; }
    
            /// <summary>
            /// Gets or sets the credit card token for the order.
            /// </summary>
            [XmlElement("CreditCardToken")]
            public string CreditCardToken { get; set; }
    
            /// <summary>
            /// Gets or sets the e-mail address of the order.
            /// </summary>
            [XmlElement("Email")]
            public string Email { get; set; }
    
            /// <summary>
            /// Gets or sets original transaction time.
            /// </summary>
            [XmlElement("OriginalTransactionTime")]
            public DateTime OriginalTransactionTime { get; set; }
    
            /// <summary>
            /// Gets the collection of item information.
            /// </summary>
            public ItemInfoCollection Items { get; private set; }
    
            /// <summary>
            /// Gets the collection of charge information.
            /// </summary>
            public ChargeInfoCollection Charges { get; private set; }
    
            /// <summary>
            /// Gets the collection of payment information.
            /// </summary>
            public PaymentInfoCollection Payments { get; private set; }
    
            /// <summary>
            /// Gets the affiliation information collection.
            /// </summary>
            [XmlArray("Affiliations")]
            public AffiliationInfoCollection Affiliations { get; private set; }
    
            /// <summary>
            /// Gets the collection of discount codes on the transaction.
            /// </summary>
            [XmlElement("DiscountCodes")]
            public StringCollection DiscountCodes { get; private set; }
    
            /// <summary>
            /// Gets the extension properties collection.
            /// </summary>
            [XmlArray("ExtensionProperties")]
            public Collection<CommerceProperty> ExtensionProperties { get; private set; }
    
            /// <summary>
            /// Gets the the collection of tax information.
            /// </summary>
            public TaxInfoCollection Taxes { get; private set; }

            //DEMO4 new //AM : Added HasReturns property
            /// <summary>
            /// Describes if the SalesOrder has a Return against it
            /// </summary>
            [XmlElement("HasReturns")]
            public string HasReturns { get; set; }
    
            /// <summary>
            /// Deserializes the customer order information from the specified XML blob.
            /// </summary>
            /// <param name="orderXml">The order XML blob.</param>
            /// <returns>The customer order information.</returns>
            public static CustomerOrderInfo FromXml(string orderXml)
            {
                CustomerOrderInfo orderInfo;
                XmlSerializer serializer = new XmlSerializer(typeof(CustomerOrderInfo));
                StringReader orderXmlReader = null;
                System.Xml.XmlReader reader = null;
                try
                {
                    orderXmlReader = new StringReader(orderXml);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.XmlResolver = null;
                    reader = System.Xml.XmlReader.Create(orderXmlReader, settings);
                    orderXmlReader = null;
                    serializer = new XmlSerializer(typeof(CustomerOrderInfo));
                    orderInfo = (CustomerOrderInfo)serializer.Deserialize(reader);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Dispose();
                    }
    
                    if (orderXmlReader != null)
                    {
                        orderXmlReader.Dispose();
                    }
                }
    
                return orderInfo;
            }
    
            /// <summary>
            /// Serializes the current customer order information to XML.
            /// </summary>
            /// <returns>The object in XML format.</returns>
            public string ToXml()
            {
                string xmlString = string.Empty;
                XmlSerializer serializer = new XmlSerializer(typeof(CustomerOrderInfo));
                using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
                {
                    serializer.Serialize(writer, this);
                    writer.Flush();
                    xmlString = writer.ToString();
                }
    
                return xmlString;
            }
        }
    }
}
