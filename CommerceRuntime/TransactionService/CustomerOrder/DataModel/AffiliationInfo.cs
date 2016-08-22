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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Affiliation information of the customer order, encapsulating the transmitting data via Real-time Service invocation.
        /// </summary>
        [Serializable]
        [XmlType("Affiliation")]
        public class AffiliationInfo
        {
            /// <summary>
            /// Gets or sets the record identifier of the affiliation.
            /// </summary>
            [XmlAttribute("RetailAffiliationId")]
            public long AffiliationRecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the record identifier of the loyalty tier.
            /// </summary>
            [XmlAttribute("RetailLoyaltyTierId")]
            public long LoyaltyTierRecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the affiliation type.
            /// </summary>
            [XmlAttribute("RetailAffiliationType")]
            public RetailAffiliationType AffiliationType { get; set; }
        }
    }
}
