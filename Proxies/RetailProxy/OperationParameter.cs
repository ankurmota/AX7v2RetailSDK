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
    namespace Commerce.RetailProxy
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates parameter of an action or a function.
        /// </summary>
        public class OperationParameter
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }
    
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public object Value { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether this instance is key.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is key; otherwise, <c>false</c>.
            /// </value>
            public bool IsKey { get; set; }
    
            /// <summary>
            /// Creates the specified name.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            /// <returns>An Instance of action or function parameter.</returns>
            public static OperationParameter Create(string name, object value)
            {
                return OperationParameter.Create(name, value, false);
            }
    
            /// <summary>
            /// Creates the specified name.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            /// <param name="isKey">If set to <c>true</c> [is key].</param>
            /// <returns>An Instance of action or function parameter.</returns>
            public static OperationParameter Create(string name, object value, bool isKey)
            {
                return new OperationParameter() { Name = name, Value = value, IsKey = isKey };
            }
        }
    }
}
