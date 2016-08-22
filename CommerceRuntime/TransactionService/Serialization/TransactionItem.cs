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
    namespace Commerce.Runtime.TransactionService.Serialization
    {
        using System;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Data Contract for GetTransaction result serialization.
        /// </summary>
        [Serializable]
        public sealed class TransactionItem : SerializableObject
        {
            #region Fields
    
            /// <summary>
            /// Periodic percentage discount key.
            /// </summary>
            public static readonly string PeriodicPercentageDiscountKey = "PeriodicPercentageDiscount";
    
            /// <summary>
            /// Line manual discount amount key.
            /// </summary>
            public static readonly string LineManualDiscountAmountKey = "LineManualDiscountAmount";
    
            /// <summary>
            /// Line manual discount percentage key.
            /// </summary>
            public static readonly string LineManualDiscountPercentageKey = "LineManualDiscountPercentage";
    
            private const string TransactionIdKey = "TransactionId";
            private const string ReceiptIdKey = "ReceiptId";
            private const string LineNumberKey = "LineNumber";
            private const string ListingIdKey = "ListingId";
            private const string BarcodeKey = "Barcode";
            private const string ItemIdKey = "ItemId";
            private const string VariantIdKey = "VariantId";
            private const string StatusKey = "Status";
            private const string CommentKey = "Comment";
            private const string InventoryBatchIdKey = "InventoryBatchId";
            private const string ReturnQuantityKey = "ReturnQuantity";
            private const string PriceKey = "Price";
            private const string NetPriceKey = "NetPrice";
            private const string NetAmountKey = "NetAmount";
            private const string NetAmountInclusiveTaxKey = "NetAmountInclusiveTax";
            private const string QuantityKey = "Quantity";
            private const string TaxGroupKey = "TaxGroup";
            private const string TaxAmountKey = "TaxAmount";
            private const string TotalDiscountAmountKey = "TotalDiscountAmount";
            private const string TotalDiscountPercentageKey = "TotalDiscountPercentage";
            private const string LineDiscountAmountKey = "LineDiscountAmount";
            private const string UnitKey = "Unit";
            private const string UnitQuantityKey = "UnitQuantity";
            private const string InventSerialIdKey = "InventSerialId";
            private const string RFIDTagIdKey = "RFIDTagId";
            private const string OriginalTaxGroupKey = "OriginalTaxGroup";
            private const string TaxItemGroupKey = "TaxItemGroup";
            private const string OriginalTaxItemGroupKey = "OriginalTaxItemGroup";
            private const string PeriodicDiscountTypeKey = "PeriodicDiscountType";
            private const string PeriodicDiscountAmountKey = "PeriodicDiscountAmount";
            private const string DiscountAmountKey = "DiscountAmount";
            private const string LoyaltyDiscountPercentageKey = "LoyaltyDiscountPercentage";
            private const string OriginalPriceKey = "OriginalPrice";
            private const string PriceChangeKey = "PriceChange";
            private const string FulfillmentStoreIdKey = "FulfillmentStoreId";
    
            #endregion
    
            #region Properties
    
            /// <summary>
            /// Gets or sets the transaction id.
            /// </summary>
            /// <value>
            /// The transaction id.
            /// </value>
            public string TransactionId
            {
                get { return (string)this[TransactionIdKey]; }
                set { this[TransactionIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the fulfillment store.
            /// </summary>
            /// <value>
            /// The store.
            /// </value>
            public string FulfillmentStoreId
            {
                get { return (string)this[FulfillmentStoreIdKey]; }
                set { this[FulfillmentStoreIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the receipt id.
            /// </summary>
            /// <value>
            /// The receipt id.
            /// </value>
            public string ReceiptId
            {
                get { return (string)this[ReceiptIdKey]; }
                set { this[ReceiptIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the line number.
            /// </summary>
            /// <value>
            /// The line number.
            /// </value>
            public decimal LineNumber
            {
                get { return (decimal)(this[LineNumberKey] ?? 0M); }
                set { this[LineNumberKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the product identifier.
            /// </summary>
            /// <value>
            /// The product identifier.
            /// </value>
            public long ListingId
            {
                get { return (long)(this[ListingIdKey] ?? 0L); }
                set { this[ListingIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the barcode.
            /// </summary>
            /// <value>
            /// The barcode.
            /// </value>
            public string Barcode
            {
                get { return (string)this[BarcodeKey]; }
                set { this[BarcodeKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the item id.
            /// </summary>
            /// <value>
            /// The item id.
            /// </value>
            public string ItemId
            {
                get { return (string)this[ItemIdKey]; }
                set { this[ItemIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the variant id.
            /// </summary>
            /// <value>
            /// The variant id.
            /// </value>
            public string VariantId
            {
                get { return (string)this[VariantIdKey]; }
                set { this[VariantIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            /// <value>
            /// The status.
            /// </value>
            public int Status
            {
                get { return (int)this[StatusKey]; }
                set { this[StatusKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the comment.
            /// </summary>
            /// <value>
            /// The comment.
            /// </value>
            public string Comment
            {
                get { return (string)this[CommentKey]; }
                set { this[CommentKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the inventory batch id.
            /// </summary>
            /// <value>
            /// The inventory batch id.
            /// </value>
            public string InventoryBatchId
            {
                get { return (string)this[InventoryBatchIdKey]; }
                set { this[InventoryBatchIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the return quantity.
            /// </summary>
            /// <value>
            /// The return quantity.
            /// </value>
            public decimal ReturnQuantity
            {
                get { return (decimal)(this[ReturnQuantityKey] ?? 0M); }
                set { this[ReturnQuantityKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the price.
            /// </summary>
            /// <value>
            /// The price.
            /// </value>
            public decimal Price
            {
                get { return (decimal)(this[PriceKey] ?? 0M); }
                set { this[PriceKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the net price.
            /// </summary>
            /// <value>
            /// The net price.
            /// </value>
            public decimal NetPrice
            {
                get { return (decimal)(this[NetPriceKey] ?? 0M); }
                set { this[NetPriceKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the net amount.
            /// </summary>
            /// <value>
            /// The net amount.
            /// </value>
            public decimal NetAmount
            {
                get { return (decimal)this[NetAmountKey]; }
                set { this[NetAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the net amount with all inclusive tax.
            /// </summary>
            /// <value>
            /// The net amount with all inclusive tax.
            /// </value>
            public decimal NetAmountInclusiveTax
            {
                get { return (decimal)(this[NetAmountInclusiveTaxKey] ?? 0M); }
                set { this[NetAmountInclusiveTaxKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the quantity.
            /// </summary>
            /// <value>
            /// The quantity.
            /// </value>
            public decimal Quantity
            {
                get { return (decimal)this[QuantityKey]; }
                set { this[QuantityKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the tax group.
            /// </summary>
            /// <value>
            /// The tax group.
            /// </value>
            public string TaxGroup
            {
                get { return (string)this[TaxGroupKey]; }
                set { this[TaxGroupKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the tax amount.
            /// </summary>
            /// <value>
            /// The tax amount.
            /// </value>
            public decimal TaxAmount
            {
                get { return (decimal)(this[TaxAmountKey] ?? 0M); }
                set { this[TaxAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the total discount amount.
            /// </summary>
            /// <value>
            /// The total discount amount.
            /// </value>
            public decimal TotalDiscountAmount
            {
                get { return (decimal)(this[TotalDiscountAmountKey] ?? 0M); }
                set { this[TotalDiscountAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the total discount percentage.
            /// </summary>
            /// <value>
            /// The total discount percentage.
            /// </value>
            public decimal TotalDiscountPercentage
            {
                get { return (decimal)(this[TotalDiscountPercentageKey] ?? 0M); }
                set { this[TotalDiscountPercentageKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the line discount amount.
            /// </summary>
            /// <value>
            /// The line discount amount.
            /// </value>
            public decimal LineDiscountAmount
            {
                get { return (decimal)(this[LineDiscountAmountKey] ?? 0M); }
                set { this[LineDiscountAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the unit of measure.
            /// </summary>
            /// <value>
            /// The unit of measure.
            /// </value>
            public string Unit
            {
                get { return (string)this[UnitKey]; }
                set { this[UnitKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the unit quantity.
            /// </summary>
            /// <value>
            /// The unit quantity.
            /// </value>
            public decimal UnitQuantity
            {
                get { return (decimal)(this[UnitQuantityKey] ?? 0M); }
                set { this[UnitQuantityKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the invent serial id.
            /// </summary>
            /// <value>
            /// The invent serial id.
            /// </value>
            public string InventSerialId
            {
                get { return (string)this[InventSerialIdKey]; }
                set { this[InventSerialIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the RFID tag id.
            /// </summary>
            /// <value>
            /// The RFID tag id.
            /// </value>
            public string RFIDTagId
            {
                get { return (string)this[RFIDTagIdKey]; }
                set { this[RFIDTagIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the original tax group.
            /// </summary>
            /// <value>
            /// The original tax group.
            /// </value>
            public string OriginalTaxGroup
            {
                get { return (string)this[OriginalTaxGroupKey]; }
                set { this[OriginalTaxGroupKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the tax item group.
            /// </summary>
            /// <value>
            /// The tax item group.
            /// </value>
            public string TaxItemGroup
            {
                get { return (string)this[TaxItemGroupKey]; }
                set { this[TaxItemGroupKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the original tax item group.
            /// </summary>
            /// <value>
            /// The original tax item group.
            /// </value>
            public string OriginalTaxItemGroup
            {
                get { return (string)this[OriginalTaxItemGroupKey]; }
                set { this[OriginalTaxItemGroupKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the type of the periodic discount.
            /// </summary>
            /// <value>
            /// The type of the periodic discount.
            /// </value>
            public int PeriodicDiscountType
            {
                get { return (int)(this[PeriodicDiscountTypeKey] ?? 0); }
                set { this[PeriodicDiscountTypeKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the periodic discount amount.
            /// </summary>
            /// <value>
            /// The periodic discount amount.
            /// </value>
            public decimal PeriodicDiscountAmount
            {
                get { return (decimal)(this[PeriodicDiscountAmountKey] ?? 0M); }
                set { this[PeriodicDiscountAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the periodic discount percentage.
            /// </summary>
            public decimal PeriodicPercentageDiscount
            {
                get { return (decimal)this[PeriodicPercentageDiscountKey]; }
                set { this[PeriodicPercentageDiscountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the line manual discount amount.
            /// </summary>
            public decimal LineManualDiscountAmount
            {
                get { return (decimal)(this[LineManualDiscountAmountKey] ?? 0M); }
                set { this[LineManualDiscountAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the line manual discount percentage.
            /// </summary>
            public decimal LineManualDiscountPercentage
            {
                get { return (decimal)(this[LineManualDiscountPercentageKey] ?? 0M); }
                set { this[LineManualDiscountPercentageKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the discount amount.
            /// </summary>
            /// <value>
            /// The discount amount.
            /// </value>
            public decimal DiscountAmount
            {
                get { return (decimal)(this[DiscountAmountKey] ?? 0M); }
                set { this[DiscountAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the loyalty discount percentage.
            /// </summary>
            /// <value>
            /// The loyalty discount percentage.
            /// </value>
            public decimal LoyaltyDiscountPercentage
            {
                get { return (decimal)(this[LoyaltyDiscountPercentageKey] ?? decimal.Zero); }
                set { this[LoyaltyDiscountPercentageKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the original price.
            /// </summary>
            public decimal OriginalPrice
            {
                get { return (decimal)(this[OriginalPriceKey] ?? decimal.Zero); }
                set { this[OriginalPriceKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets a value indicating whether the original price has been overridden.
            /// </summary>
            public bool PriceChange
            {
                get { return Convert.ToBoolean(this[PriceChangeKey] ?? false); }
                set { this[PriceChangeKey] = value; }
            }
    
            #endregion
    
            #region Serialization Methods
    
            /// <summary>
            /// Deserializes the specified serialized XML string.
            /// </summary>
            /// <param name="serializedXml">The serialized XML string.</param>
            /// <returns>TransactionItem instance deserialized from input.</returns>
            public static TransactionItem Deserialize(string serializedXml)
            {
                return SerializationHelper.DeserializeObjectFromXml<TransactionItem>(serializedXml);
            }
    
            /// <summary>
            /// Serializes the specified transaction item.
            /// </summary>
            /// <param name="transactionItem">The transaction item.</param>
            /// <returns>The serialized string of the transaction item.</returns>
            public static string Serialize(TransactionItem transactionItem)
            {
                if (transactionItem == null)
                {
                    return null;
                }
    
                return SerializationHelper.SerializeObjectToXml<TransactionItem>(transactionItem);
            }
    
            #endregion
        }
    }
}
