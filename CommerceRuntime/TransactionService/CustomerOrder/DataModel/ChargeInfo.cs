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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Miscellaneous charge info for use in transmitting via TS call.
        /// </summary>
        [Serializable]
        [XmlType("Charge")]
        public class ChargeInfo
        {
            /// <summary>
            /// Gets or sets the charge code.
            /// </summary>
            [XmlAttribute("Code")]
            public string Code { get; set; }
    
            /// <summary>
            /// Gets or sets the charge amount.
            /// </summary>
            [XmlAttribute("Amount")]
            public decimal Amount { get; set; }
    
            /// <summary>
            /// Gets or sets the sales tax group identifier.
            /// </summary>
            [XmlAttribute("TaxGroup")]
            public string SalesTaxGroup { get; set; }
    
            /// <summary>
            /// Gets or sets the item tax group identifier.
            /// </summary>
            [XmlAttribute("TaxItemGroup")]
            public string TaxGroup { get; set; }
    
            /// <summary>
            /// Gets or sets the method.
            /// </summary>
            [XmlAttribute("Method")]
            public ChargeMethod Method { get; set; }
        }
    }
}
