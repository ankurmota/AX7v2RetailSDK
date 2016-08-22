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
        /// Represents the configuration element for the checkout control.
        /// </summary>
        [ComVisible(false)]
        public sealed class CheckoutControl : ConfigurationElement
        {
            private const string IsDemoModeKeyName = "isDemoMode";
            private const string DemoDataPathKeyName = "demoDataPath";
    
            /// <summary>
            /// Gets a value indicating whether isDemoMode property for Control is set.
            /// </summary>
            /// <remarks>
            /// This property indicates whether the checkout control is in auto fill demo mode or not.
            /// </remarks>
            [ConfigurationProperty(IsDemoModeKeyName)]
            public bool IsDemoMode
            {
                get { return (bool)this[IsDemoModeKeyName]; }
            }
    
            /// <summary>
            /// Gets the path to the demo data XML file.
            /// </summary>
            /// <remarks>
            /// This property contains the relative to the page path to the demoData.xml file
            /// that is used by autoFillCheckout in <c>Checkout.ts</c>.
            /// </remarks>
            [ConfigurationProperty(DemoDataPathKeyName)]
            public string DemoDataPath
            {
                get { return (string)this[DemoDataPathKeyName]; }
            }
        }
    }
}
