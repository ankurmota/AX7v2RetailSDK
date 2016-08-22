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
        using System.Diagnostics.CodeAnalysis;
        using System.IO;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Item info, for use in transmitting via TS call.
        /// </summary>
        [Serializable]
        [XmlType("Item")]
        public class ItemInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ItemInfo"/> class.
            /// </summary>
            public ItemInfo()
            {
                this.Charges = new ChargeInfoCollection();
                this.Discounts = new DiscountInfoCollection();
                this.Taxes = new TaxInfoCollection();
            }
    
            /// <summary>
            /// Gets or sets the item identifier.
            /// </summary>
            [XmlAttribute]
            public string ItemId { get; set; }

            /// <summary>
            /// Gets or sets the the variant's Inventory dimension identifier.
            /// </summary>
            [XmlAttribute]
            public string InventDimensionId { get; set; }

            /// <summary>
            /// Gets or sets the item comment.
            /// </summary>
            [XmlAttribute]
            public string Comment { get; set; }
    
            /// <summary>
            /// Gets or sets the record identifier.
            /// </summary>
            [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rec", Justification = "Follows naming in AX.")]
            [XmlAttribute("RecId")]
            public long RecId { get; set; }
    
            /// <summary>
            /// Gets or sets the quantity.
            /// </summary>
            [XmlAttribute]
            public decimal Quantity { get; set; }
    
            /// <summary>
            /// Gets or sets the quantity picked.
            /// </summary>
            [XmlAttribute]
            public decimal QuantityPicked { get; set; }
    
            /// <summary>
            /// Gets or sets the unit of measure.
            /// </summary>
            [XmlAttribute]
            public string Unit { get; set; }
    
            /// <summary>
            /// Gets or sets the price.
            /// </summary>
            [XmlAttribute]
            public decimal Price { get; set; }
    
            /// <summary>
            /// Gets or sets the discount amount.
            /// </summary>
            [XmlAttribute]
            public decimal Discount { get; set; }
    
            /// <summary>
            /// Gets or sets the discount percent.
            /// </summary>
            [XmlAttribute]
            public decimal DiscountPercent { get; set; }
    
            /// <summary>
            /// Gets or sets the net amount.
            /// </summary>
            [XmlAttribute]
            public decimal NetAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the sales tax group identifier.
            /// </summary>
            [XmlAttribute("TaxGroup")]
            public string SalesTaxGroup { get; set; }
    
            /// <summary>
            /// Gets or sets the item tax group identifier.
            /// </summary>
            [XmlAttribute("TaxItemGroup")]
            public string ItemTaxGroup { get; set; }
    
            /// <summary>
            /// Gets or sets the sales markup used for price charges.
            /// </summary>
            [XmlAttribute("SalesMarkup")]
            public decimal SalesMarkup { get; set; }
    
            /// <summary>
            /// Gets or sets the site identifier.
            /// </summary>
            [XmlAttribute]
            public string SiteId { get; set; }
    
            /// <summary>
            /// Gets or sets the identifier of store from where the order for item is fulfilled.
            /// </summary>
            [XmlAttribute("FulfillmentStoreId")]
            public string FulfillmentStoreId { get; set; }
    
            /// <summary>
            /// Gets or sets the sales status.
            /// </summary>
            [XmlAttribute]
            public int Status { get; set; }
    
            /// <summary>
            /// Gets or sets the warehouse identifier.
            /// </summary>
            [XmlAttribute("InventLocationId")]
            public string WarehouseId { get; set; }
    
            /// <summary>
            /// Gets or sets the color identifier.
            /// </summary>
            [XmlAttribute("InventColorId")]
            public string ColorId { get; set; }
    
            /// <summary>
            /// Gets or sets the name of the color.
            /// </summary>
            [XmlAttribute("InventColorName")]
            public string ColorName { get; set; }
    
            /// <summary>
            /// Gets or sets the size identifier.
            /// </summary>
            [XmlAttribute("InventSizeId")]
            public string SizeId { get; set; }
    
            /// <summary>
            /// Gets or sets the name of the size.
            /// </summary>
            [XmlAttribute("InventSizeName")]
            public string SizeName { get; set; }
    
            /// <summary>
            /// Gets or sets the style identifier.
            /// </summary>
            [XmlAttribute("InventStyleId")]
            public string StyleId { get; set; }
    
            /// <summary>
            /// Gets or sets the name of the style.
            /// </summary>
            [XmlAttribute("InventStyleName")]
            public string StyleName { get; set; }
    
            /// <summary>
            /// Gets or sets the configuration identifier.
            /// </summary>
            [XmlAttribute("ConfigId")]
            public string ConfigId { get; set; }
    
            /// <summary>
            /// Gets or sets the name of the configuration.
            /// </summary>
            [XmlAttribute("ConfigName")]
            public string ConfigName { get; set; }
    
            /// <summary>
            /// Gets or sets the delivery mode.
            /// </summary>
            [XmlAttribute("DeliveryMode")]
            public string DeliveryMode { get; set; }
    
            /// <summary>
            /// Gets or sets the expiry Date in string format.
            /// </summary>
            [XmlAttribute("RequestedDeliveryDate")]
            public string RequestedDeliveryDateString { get; set; }
    
            /// <summary>
            /// Gets or sets the address identifier.
            /// </summary>
            [XmlAttribute("AddressRecord")]
            public string AddressRecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the batch identifier.
            /// </summary>
            [XmlAttribute]
            public string BatchId { get; set; }
    
            /// <summary>
            /// Gets or sets the serial identifier.
            /// </summary>
            [XmlAttribute]
            public string SerialId { get; set; }
    
            /// <summary>
            /// Gets or sets the variant identifier.
            /// </summary>
            [XmlAttribute]
            public string VariantId { get; set; }

            /// <summary>
            /// Gets or sets the inventory reservation transaction identifier.
            /// </summary>
            [XmlAttribute]
            public string InventTransId { get; set; }

            /// <summary>
            /// Gets or sets the inventory transaction identifier.
            /// </summary>
            [XmlAttribute("ReturnInventTransId")]
            public string ReturnInventTransId { get; set; }
    
            /// <summary>
            /// Gets or sets the invoice identifier.
            /// </summary>
            [XmlAttribute("InvoiceId")]
            public string InvoiceId { get; set; }
    
            /// <summary>
            /// Gets or sets the line discount amount.
            /// </summary>
            [XmlAttribute("LineDscAmount")]
            public decimal LineDscAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the periodic discount.
            /// </summary>
            [XmlAttribute("PeriodicDiscount")]
            public decimal PeriodicDiscount { get; set; }
    
            /// <summary>
            /// Gets or sets the periodic percentage discount.
            /// </summary>
            [XmlAttribute("PeriodicPercentageDiscount")]
            public decimal PeriodicPercentageDiscount { get; set; }
    
            /// <summary>
            /// Gets or sets the line manual discount amount.
            /// </summary>
            [XmlAttribute("LineManualDiscountAmount")]
            public decimal LineManualDiscountAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the line manual discount percentage.
            /// </summary>
            [XmlAttribute("LineManualDiscountPercentage")]
            public decimal LineManualDiscountPercentage { get; set; }
    
            /// <summary>
            /// Gets or sets the total discount.
            /// </summary>
            [XmlAttribute("TotalDiscount")]
            public decimal TotalDiscount { get; set; }
    
            /// <summary>
            /// Gets or sets the total percentage discount.
            /// </summary>
            [XmlAttribute("TotalPctDiscount")]
            public decimal TotalPctDiscount { get; set; }
    
            /// <summary>
            /// Gets or sets the listing id.
            /// </summary>
            [XmlAttribute("ListingId")]
            public string ListingId { get; set; }
    
            /// <summary>
            /// Gets or sets the catalog rec id.
            /// </summary>
            [XmlAttribute("CatalogRecId")]
            public long Catalog { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether this is a gift card item.
            /// </summary>
            [XmlAttribute("Giftcard")]
            public bool Giftcard { get; set; }
    
            /// <summary>
            /// Gets or sets the gift card delivery email.
            /// </summary>
            [XmlAttribute("GiftcardDeliveryEmail")]
            public string GiftcardDeliveryEmail { get; set; }
    
            /// <summary>
            /// Gets or sets the gift card delivery message.
            /// </summary>
            [XmlAttribute("GiftcardDeliveryMessage")]
            public string GiftcardDeliveryMessage { get; set; }
    
            /// <summary>
            /// Gets the the collection of line level charges for mixed delivery.
            /// </summary>
            public ChargeInfoCollection Charges { get; private set; }
    
            /// <summary>
            /// Gets the the collection of line level discounts.
            /// </summary>
            public DiscountInfoCollection Discounts { get; private set; }
    
            /// <summary>
            /// Gets the the collection of tax information.
            /// </summary>
            public TaxInfoCollection Taxes { get; private set; }
        }
    }
}
