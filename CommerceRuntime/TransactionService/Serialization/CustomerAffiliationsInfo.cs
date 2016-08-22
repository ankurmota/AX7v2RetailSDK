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
        using System.Collections.ObjectModel;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Customer affiliations.
        /// </summary>
        [Serializable]
        [XmlRoot("RetailCustAffiliations")]
        public class CustomerAffiliationsInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerAffiliationsInfo"/> class.
            /// </summary>
            public CustomerAffiliationsInfo()
            {
                this.CustomerAffiliationItems = new Collection<CustomerAffiliationInfo>();
            }
    
            /// <summary>
            /// Gets or sets Collection of affiliation elements.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Non-read-only collection required for serialization.")]
            [XmlElement("RetailCustAffiliation")]
            public Collection<CustomerAffiliationInfo> CustomerAffiliationItems { get; set; }
        }
    }
}
