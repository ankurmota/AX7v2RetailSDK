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
    namespace Retail.Ecommerce.Sdk.Controls
    {
        using System.Configuration;
        using System.Runtime.InteropServices;
    
        /// <summary>
        /// Represents the configuration section for the Ecommerce controls.
        /// </summary>
        [ComVisible(false)]
        public sealed class ControlsSection : ConfigurationSection
        {
            private const string ServicesKeyName = "services";
            private const string CheckoutControlKeyName = "checkout";
            private const string ProductDetailsUrlTemplateKeyName = "productDetailsUrlTemplate";
    
            /// <summary>
            /// Gets the services.
            /// </summary>
            [ConfigurationProperty(ServicesKeyName)]
            public Services Services
            {
                get { return (Services)this[ServicesKeyName]; }
            }
    
            /// <summary>
            /// Gets the checkout control.
            /// </summary>
            [ConfigurationProperty(CheckoutControlKeyName)]
            public CheckoutControl Checkout
            {
                get { return (CheckoutControl)this[CheckoutControlKeyName]; }
            }
    
            /// <summary>
            /// Gets the product url format.
            /// </summary>
            [ConfigurationProperty(ProductDetailsUrlTemplateKeyName)]
            public string ProductDetailsUrlTemplate
            {
                get { return (string)this[ProductDetailsUrlTemplateKeyName]; }
            }
        }
    }
}
