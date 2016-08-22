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
    namespace Commerce.Runtime.TransactionService.Serialization
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Customer affiliation information.
        /// </summary>
        [Serializable]
        [XmlType("RetailCustAffiliation")]
        public class CustomerAffiliationInfo : SerializableObject
        {
            /// <summary>
            /// Gets or sets the record identifier.
            /// </summary>
            [XmlAttribute("RecId")]
            public long RecordId { get; set; }
    
            /// <summary>
            /// Gets or sets the customer account number.
            /// </summary>
            [XmlAttribute("CustAccountNum")]
            public string CustAccountNum { get; set; }
    
            /// <summary>
            /// Gets or sets the retail affiliation id.
            /// </summary>
            [XmlAttribute("RetailAffiliationId")]
            public long RetailAffiliationId { get; set; }
        }
    }
}
