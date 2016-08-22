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
        using System.Configuration;
    
        /// <summary>
        /// Represents a collection of Identity Provider Client configuration elements.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "This class has very limited use, and does not need to support generic collection interfaces")]
        public class IdentityProviderClientConfigurationElementCollection : ConfigurationElementCollection
        {
            /// <summary>
            /// Creates new element of the collection.
            /// </summary>
            /// <returns>Newly created element.</returns>
            protected override ConfigurationElement CreateNewElement()
            {
                return new IdentityProviderClientConfigurationElement();
            }
    
            /// <summary>
            /// Gets element's key.
            /// </summary>
            /// <param name="element">The element.</param>
            /// <returns>The key.</returns>
            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((IdentityProviderClientConfigurationElement)element).Name;
            }
        }
    }
}