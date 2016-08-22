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
        public sealed class TransactionHeader : SerializableObject
        {
            #region Fields
    
            private const string TransactionIdKey = "TransactionId";
            private const string ReceiptIdKey = "ReceiptId";
            private const string StoreKey = "Store";
            private const string TerminalKey = "Terminal";
            private const string StaffKey = "Staff";
            private const string TransactionDateKey = "TransactionDate";
            private const string ShiftKey = "Shift";
            private const string ShiftDateKey = "ShiftDate";
            private const string CustomerAccountKey = "CustomerAccount";
            private const string EntryStatusKey = "EntryStatus";
            private const string CurrencyKey = "Currency";
            private const string BatchIdKey = "BatchId";
            private const string PaymentAmountKey = "PaymentAmount";
            private const string LoyaltyCardIdKey = "LoyaltyCardId";
            private const string HasLoyaltyPaymentKey = "HasLoyaltyPayment";
    
            #endregion
    
            #region Proerties
    
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
            /// Gets or sets the store.
            /// </summary>
            /// <value>
            /// The store.
            /// </value>
            public string Store
            {
                get { return (string)this[StoreKey]; }
                set { this[StoreKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the terminal.
            /// </summary>
            /// <value>
            /// The terminal.
            /// </value>
            public string Terminal
            {
                get { return (string)this[TerminalKey]; }
                set { this[TerminalKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the staff.
            /// </summary>
            /// <value>
            /// The staff.
            /// </value>
            public string Staff
            {
                get { return (string)this[StaffKey]; }
                set { this[StaffKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the transaction date.
            /// </summary>
            /// <value>
            /// The transaction date.
            /// </value>
            public string TransactionDate
            {
                get { return (string)this[TransactionDateKey]; }
                set { this[TransactionDateKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the shift.
            /// </summary>
            /// <value>
            /// The shift.
            /// </value>
            public string Shift
            {
                get { return (string)this[ShiftKey]; }
                set { this[ShiftKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the shift date.
            /// </summary>
            /// <value>
            /// The shift date.
            /// </value>
            public string ShiftDate
            {
                get { return (string)this[ShiftDateKey]; }
                set { this[ShiftDateKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the customer account.
            /// </summary>
            /// <value>
            /// The customer account.
            /// </value>
            public string CustomerAccount
            {
                get { return (string)this[CustomerAccountKey]; }
                set { this[CustomerAccountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the entry status.
            /// </summary>
            /// <value>
            /// The entry status.
            /// </value>
            public string EntryStatus
            {
                get { return (string)this[EntryStatusKey]; }
                set { this[EntryStatusKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the currency.
            /// </summary>
            /// <value>
            /// The currency.
            /// </value>
            public string Currency
            {
                get { return (string)this[CurrencyKey]; }
                set { this[CurrencyKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the BatchId.
            /// </summary>
            /// <value>
            /// The BatchId.
            /// </value>
            public string BatchId
            {
                get { return (string)this[BatchIdKey]; }
                set { this[BatchIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the payment amount.
            /// </summary>
            /// <value>
            /// The payment amount.
            /// </value>
            public decimal PaymentAmount
            {
                get { return (decimal)(this[PaymentAmountKey] ?? 0M); }
                set { this[PaymentAmountKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the loyalty card number.
            /// </summary>
            /// <value>
            /// The loyalty card number.
            /// </value>
            public string LoyaltyCardId
            {
                get { return (string)this[LoyaltyCardIdKey]; }
                set { this[LoyaltyCardIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets a value indicating whether the transaction has any posted loyalty tender.
            /// </summary>
            /// <value>
            /// The value indicating whether the transaction has any posted loyalty tender.
            /// </value>
            public bool HasLoyaltyPayment
            {
                get { return Convert.ToBoolean(this[HasLoyaltyPaymentKey] ?? 0); }
                set { this[HasLoyaltyPaymentKey] = value; }
            }
    
            #endregion
    
            #region Serialization Methods
    
            /// <summary>
            /// Deserializes the specified serialized XML string.
            /// </summary>
            /// <param name="serializedXml">The serialized XML string.</param>
            /// <returns>TransactionHeader deserialized from the input.</returns>
            public static TransactionHeader Deserialize(string serializedXml)
            {
                return SerializationHelper.DeserializeObjectFromXml<TransactionHeader>(serializedXml);
            }
    
            /// <summary>
            /// Serializes the specified transaction header.
            /// </summary>
            /// <param name="transactionHeader">The transaction header.</param>
            /// <returns>TransactionHeader serialized into a string.</returns>
            public static string Serialize(TransactionHeader transactionHeader)
            {
                if (transactionHeader == null)
                {
                    return null;
                }
    
                return SerializationHelper.SerializeObjectToXml<TransactionHeader>(transactionHeader);
            }
    
            #endregion
        }
    }
}
