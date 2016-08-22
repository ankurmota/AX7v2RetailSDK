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
        /// Data Contract for item return parameter serializations.
        /// </summary>
        [Serializable]
        public sealed class ItemReturn : SerializableObject
        {
            private const string TransactionIdKey = "TransactionIdKey";
            private const string StoreIdKey = "StoreIdKey";
            private const string ChannelIdKey = "ChannelIdKey";
            private const string TerminalIdKey = "TerminalId";
            private const string LineNumberKey = "LineNumber";
            private const string QuantityKey = "Quantity";
    
            /// <summary>
            /// Gets or sets the transaction identifier.
            /// </summary>
            public string TransactionId
            {
                get { return (string)this[TransactionIdKey]; }
                set { this[TransactionIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the store identifier.
            /// </summary>
            public string StoreId
            {
                get { return (string)this[StoreIdKey]; }
                set { this[StoreIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the channel identifier.
            /// </summary>
            public long ChannelId
            {
                get { return (long)(this[ChannelIdKey] ?? 0L); }
                set { this[ChannelIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the terminal identifier.
            /// </summary>
            public string TerminalId
            {
                get { return (string)this[TerminalIdKey]; }
                set { this[TerminalIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the line number.
            /// </summary>
            public decimal LineNumber
            {
                get { return (decimal)(this[LineNumberKey] ?? 0M); }
                set { this[LineNumberKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the quantity.
            /// </summary>
            public decimal Quantity
            {
                get { return (decimal)(this[QuantityKey] ?? 0M); }
                set { this[QuantityKey] = value; }
            }
    
            /// <summary>
            /// Deserializes the specified source XML to the <see cref="ItemReturn"/> object.
            /// </summary>
            /// <param name="source">The XML source of deserialization.</param>
            /// <returns>The item return object.</returns>
            public static ItemReturn Deserialize(string source)
            {
                return SerializationHelper.DeserializeObjectFromXml<ItemReturn>(source);
            }
    
            /// <summary>
            /// Serializes the specified item to an XML string.
            /// </summary>
            /// <param name="itemReturn">The object to serialize.</param>
            /// <returns>An XML string.</returns>
            public static string Serialize(ItemReturn itemReturn)
            {
                if (itemReturn == null)
                {
                    return null;
                }
    
                return SerializationHelper.SerializeObjectToXml<ItemReturn>(itemReturn);
            }
        }
    }
}
