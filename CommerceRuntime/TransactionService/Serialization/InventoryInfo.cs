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
        /// Data Contract for inventory information result serialization.
        /// </summary>
        [Serializable]
        public class InventoryInfo : SerializableObject
        {
            private const string ItemIdKey = "ItemId";
            private const string InventoryLocationIdKey = "InventoryLocationId";
            private const string StoreNameKey = "StoreName";
            private const string InventoryAvailableKey = "InventoryAvailable";
    
            /// <summary>
            /// Gets or sets the item identifier.
            /// </summary>
            /// <value>
            /// The item identifier.
            /// </value>
            public string ItemId
            {
                get { return (string)this[ItemIdKey]; }
                set { this[ItemIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the inventory location identifier.
            /// </summary>
            /// <value>
            /// The inventory location identifier.
            /// </value>
            public string InventoryLocationId
            {
                get { return (string)this[InventoryLocationIdKey]; }
                set { this[InventoryLocationIdKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the name of the store.
            /// </summary>
            /// <value>
            /// The name of the store.
            /// </value>
            public string StoreName
            {
                get { return (string)this[StoreNameKey]; }
                set { this[StoreNameKey] = value; }
            }
    
            /// <summary>
            /// Gets or sets the inventory available.
            /// </summary>
            /// <value>
            /// The inventory available.
            /// </value>
            public string InventoryAvailable
            {
                get { return (string)this[InventoryAvailableKey]; }
                set { this[InventoryAvailableKey] = value; }
            }
    
            /// <summary>
            /// Deserializes the specified source.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <returns>Deserialized InventoryInfo instance from source.</returns>
            public static InventoryInfo Deserialize(string source)
            {
                return SerializationHelper.DeserializeObjectFromXml<InventoryInfo>(source);
            }
    
            /// <summary>
            /// Serializes the specified inventory info.
            /// </summary>
            /// <param name="inventoryInfo">The inventory info.</param>
            /// <returns>Serialized string for inventoryInfo.</returns>
            public static string Serialize(InventoryInfo inventoryInfo)
            {
                if (inventoryInfo == null)
                {
                    return null;
                }
    
                return SerializationHelper.SerializeObjectToXml<InventoryInfo>(inventoryInfo);
            }
        }
    }
}
