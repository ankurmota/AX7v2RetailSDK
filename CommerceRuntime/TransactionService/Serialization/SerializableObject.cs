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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents a serializable object.
        /// </summary>
        [Serializable]
        public abstract class SerializableObject
        {
            private readonly IDictionary<string, object> properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    
            /// <summary>
            /// Gets or sets the value associated with the specified key.
            /// </summary>
            /// <param name="key">The specified key.</param>
            /// <returns>
            /// The value associated with the specified key or NULL if the specified key does not exist.
            /// </returns>
            public object this[string key]
            {
                get
                {
                    object value;
                    if (this.properties.TryGetValue(key, out value))
                    {
                        return value;
                    }
    
                    return null;
                }
    
                set
                {
                    if (this.properties.ContainsKey(key) == false)
                    {
                        this.properties.Add(key, value);
                    }
                    else
                    {
                        this.properties[key] = value;
                    }
                }
            }
        }
    }
}
