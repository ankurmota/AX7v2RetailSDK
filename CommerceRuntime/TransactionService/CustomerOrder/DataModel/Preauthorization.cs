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
        /// Represents payment pre-authorization.
        /// </summary>
        [Serializable]
        [XmlType("PreAuthorization")]
        public class Preauthorization
        {
            /// <summary>
            /// Gets or sets the Payment SDK properties XML.
            /// </summary>
            [XmlAttribute("PaymentPropertiesBlob")]
            public string PaymentPropertiesBlob { get; set; }
        }
    }
}
