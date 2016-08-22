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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Extension property related utilities.
        /// </summary>
        public partial class Utilities
        {
            /// <summary>
            /// Sets the extension property.
            /// </summary>
            /// <param name="extensionProperties">The extension properties.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="extensionPropertyType">Type of the extension property.</param>
            /// <param name="propertyValue">The property value.</param>
            public static void SetPropertyValue(this ObservableCollection<CommerceProperty> extensionProperties, string propertyName, ExtensionPropertyTypes extensionPropertyType, object propertyValue)
            {
                if (extensionProperties == null)
                {
                    extensionProperties = new ObservableCollection<CommerceProperty>();
                }

                CommerceProperty extensionProperty = extensionProperties.Where(e => string.Equals(e.Key, propertyName, StringComparison.Ordinal)).FirstOrDefault();

                if (extensionProperty == null)
                {
                    extensionProperty = new CommerceProperty()
                    {
                        Key = propertyName,
                        Value = new CommercePropertyValue()
                    };

                    extensionProperties.Add(extensionProperty);
                }

                switch (extensionPropertyType)
                {
                    case ExtensionPropertyTypes.Boolean:
                        extensionProperty.Value.BooleanValue = (bool)propertyValue;
                        break;

                    case ExtensionPropertyTypes.Byte:
                        extensionProperty.Value.ByteValue = (byte)propertyValue;
                        break;

                    case ExtensionPropertyTypes.DateTimeOffset:
                        extensionProperty.Value.DateTimeOffsetValue = (DateTimeOffset)propertyValue;
                        break;

                    case ExtensionPropertyTypes.Decimal:
                        extensionProperty.Value.DecimalValue = (decimal)propertyValue;
                        break;

                    case ExtensionPropertyTypes.Integer:
                        extensionProperty.Value.IntegerValue = (int)propertyValue;
                        break;

                    case ExtensionPropertyTypes.Long:
                        extensionProperty.Value.LongValue = (long)propertyValue;
                        break;

                    case ExtensionPropertyTypes.String:
                        extensionProperty.Value.StringValue = (string)propertyValue;
                        break;

                    default:
                        RetailLogger.Log.OnlineStoreInvalidExtensionPropertyTypeSpecified(extensionPropertyType.ToString());
                        var message = string.Format("Invalid extension propery type value specified: {0}", extensionPropertyType);
                        var exception = new NotSupportedException(message);
                        throw exception;
                }
            }

            /// <summary>
            /// Gets the extension property.
            /// </summary>
            /// <param name="extensionProperties">The extension properties.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="extensionPropertyType">Type of the extension property.</param>
            /// <returns>The property value of the specified type.</returns>
            public static object GetPropertyValue(this ObservableCollection<CommerceProperty> extensionProperties, string propertyName, ExtensionPropertyTypes extensionPropertyType)
            {
                if (extensionProperties == null)
                {
                    throw new ArgumentNullException(nameof(extensionProperties));
                }

                CommerceProperty extensionProperty = extensionProperties.Where(e => string.Equals(e.Key, propertyName, StringComparison.Ordinal)).FirstOrDefault();

                if (extensionProperty == null)
                {
                    throw new KeyNotFoundException(string.Format("The specified property value was not found: {0}", propertyName));
                }

                if (extensionProperty.Value == null)
                {
                    RetailLogger.Log.OnlineStoreSpecifiedExtensionPropertyHasNullCommercePropertyValue(extensionPropertyType.ToString());
                    var exception = new NotSupportedException("The commerce property value object to be read cannot be null");
                    throw exception;
                }

                object propertyValue = null;
                switch (extensionPropertyType)
                {
                    case ExtensionPropertyTypes.Boolean:
                        propertyValue = extensionProperty.Value.BooleanValue;
                        break;

                    case ExtensionPropertyTypes.Byte:
                        propertyValue = extensionProperty.Value.ByteValue;
                        break;

                    case ExtensionPropertyTypes.DateTimeOffset:
                        propertyValue = extensionProperty.Value.DateTimeOffsetValue;
                        break;

                    case ExtensionPropertyTypes.Decimal:
                        propertyValue = extensionProperty.Value.DecimalValue;
                        break;

                    case ExtensionPropertyTypes.Integer:
                        propertyValue = extensionProperty.Value.IntegerValue;
                        break;

                    case ExtensionPropertyTypes.Long:
                        propertyValue = extensionProperty.Value.LongValue;
                        break;

                    case ExtensionPropertyTypes.String:
                        propertyValue = extensionProperty.Value.StringValue;
                        break;

                    default:
                        RetailLogger.Log.OnlineStoreInvalidExtensionPropertyTypeSpecified(extensionPropertyType.ToString());
                        var exception = new NotSupportedException(string.Format("Invalid extension propery type value specified: {0}", extensionPropertyType));
                        throw exception;
                }

                return propertyValue;
            }
        }
    }
}