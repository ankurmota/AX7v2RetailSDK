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

        /// <summary>
        /// The operating unit configuration.
        /// </summary>
        public class OperatingUnitConfiguration
        {
            /// <summary>
            /// Gets or sets the operating unit identifier.
            /// </summary>
            public string OperatingUnitId { get; set; }

            /// <summary>
            /// Gets or sets the channel identifier.
            /// </summary>
            public long ChannelId { get; set; }

            /// <summary>
            /// Gets or sets the retail server url.
            /// </summary>
            public Uri RetailServerUri { get; set; }

            /// <summary>
            /// Gets or sets the currency symbol (e.g. $, Â£, â‚¬, etc.).
            /// </summary>
            public string CurrencySymbol { get; set; }

            /// <summary>
            /// Gets or sets the currency string template.
            /// </summary>
            public string CurrencyStringTemplate { get; set; }
        }
    }
}