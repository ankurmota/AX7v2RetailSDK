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
    namespace Commerce.Runtime.Services.CustomerOrder
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.IO;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Discount info, for use in transmitting via TS call.
        /// </summary>
        [Serializable]
        [XmlType("Discount")]
        public class DiscountInfo
        {
            /// <summary>
            /// Gets or sets the effective amount.
            /// </summary>
            [XmlAttribute("Amount")]
            public decimal Amount { get; set; }
    
            /// <summary>
            /// Gets or sets the customer discount type.
            /// Maps to the enumeration RetailCustomerDiscountType in AX.
            /// </summary>
            [XmlAttribute("CustomerDiscountType")]
            public int CustomerDiscountType { get; set; }
    
            /// <summary>
            /// Gets or sets the discount code.
            /// </summary>
            [XmlAttribute("DiscountCode")]
            public string DiscountCode { get; set; }
    
            /// <summary>
            /// Gets or sets the discount origin type.
            /// Maps to enumeration RetailDiscountOriginType in AX.
            /// </summary>
            [XmlAttribute("DiscountOriginType")]
            public int DiscountOriginType { get; set; }
    
            /// <summary>
            /// Gets or sets the manual discount type.
            /// Maps to enumeration RetailManualDiscountType in AX.
            /// </summary>
            [XmlAttribute("ManualDiscountType")]
            public int ManualDiscountType { get; set; }
    
            /// <summary>
            /// Gets or sets the periodic discount offer identifier.
            /// </summary>
            [XmlAttribute("PeriodicDiscountOfferId")]
            public string PeriodicDiscountOfferId { get; set; }
    
            /// <summary>
            /// Gets or sets the periodic discount offer name.
            /// </summary>
            [XmlAttribute("OfferName")]
            public string OfferName { get; set; }
    
            /// <summary>
            /// Gets or sets the deal price.
            /// </summary>
            [XmlAttribute("DealPrice")]
            public decimal DealPrice { get; set; }
    
            /// <summary>
            /// Gets or sets the discount amount.
            /// </summary>
            [XmlAttribute("DiscountAmount")]
            public decimal DiscountAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the discount percentage.
            /// </summary>
            [XmlAttribute("Percentage")]
            public decimal Percentage { get; set; }
        }
    }
}
