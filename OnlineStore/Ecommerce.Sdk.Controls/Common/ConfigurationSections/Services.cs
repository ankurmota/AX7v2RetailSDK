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
        /// Represents the configuration element for the services.
        /// </summary>
        [ComVisible(false)]
        public sealed class Services : ConfigurationElement
        {
            private const string CartWebApiUrlKeyName = "cartWebApiUrl";
            private const string OrgUnitWebApiUrlKeyName = "orgUnitWebApiUrl";
            private const string RetailOperationsWebApiUrlKeyName = "retailOperationsWebApiUrl";
            private const string CustomerWebApiUrlKeyName = "customerWebApiUrl";
            private const string SalesOrderWebApiUrlKeyName = "salesOrderWebApiUrl";
            private const string ProductWebApiUrlKeyName = "productWebApiUrl";

            /// <summary>
            /// Gets the cart web API url.
            /// </summary>
            [ConfigurationProperty(CartWebApiUrlKeyName)]
            public string CartWebApiUrl
            {
                get { return (string)this[CartWebApiUrlKeyName]; }
            }

            /// <summary>
            /// Gets the org unit web API url.
            /// </summary>
            [ConfigurationProperty(OrgUnitWebApiUrlKeyName)]
            public string OrgUnitWebApiUrl
            {
                get { return (string)this[OrgUnitWebApiUrlKeyName]; }
            }

            /// <summary>
            /// Gets the retail operations web API url.
            /// </summary>
            [ConfigurationProperty(RetailOperationsWebApiUrlKeyName)]
            public string RetailOperationsWebApiUrl
            {
                get { return (string)this[RetailOperationsWebApiUrlKeyName]; }
            }

            /// <summary>
            /// Gets the customer web API url.
            /// </summary>
            [ConfigurationProperty(CustomerWebApiUrlKeyName)]
            public string CustomerWebApiUrl
            {
                get { return (string)this[CustomerWebApiUrlKeyName]; }
            }

            /// <summary>
            /// Gets the sales order web API url.
            /// </summary>
            [ConfigurationProperty(SalesOrderWebApiUrlKeyName)]
            public string SalesOrderWebApiUrl
            {
                get { return (string)this[SalesOrderWebApiUrlKeyName]; }
            }

            /// <summary>
            /// Gets the product web API url.
            /// </summary>
            [ConfigurationProperty(ProductWebApiUrlKeyName)]
            public string ProductWebApiUrl
            {
                get { return (string)this[ProductWebApiUrlKeyName]; }
            }
        }
    }
}