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
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents the customer order tax information. Used in serializing and transmitting the order to AX.
        /// </summary>
        [Serializable]
        [XmlRoot("TaxTrans")]
        public class TaxInfo
        {
            /// <summary>
            /// Gets or sets the tax code.
            /// </summary>
            [XmlElement("TaxCode")]
            public string TaxCode { get; set; }
    
            /// <summary>
            /// Gets or sets the tax amount.
            /// </summary>
            [XmlElement("TaxAmount")]
            public decimal Amount { get; set; }
        }
    }
}
