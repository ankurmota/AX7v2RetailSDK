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
    namespace Commerce.Runtime.TransactionService
    {
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Data Contract for GetOrderHistoryListSearch request parameter.
        /// </summary>
        [DataContract]
        public class AxOrderHistorySearchCriteria
        {
            // AX Xml Schema definition.
            // <Arguments>
            //  <CustomerAccountNumber>"1234"</CustomerAccountNumber>
            //  <IncludeDetails>"true"</IncludeDetails>
            //  <PagingInfo>
            //   <Skip>1</Skip>
            //   <Top>100</Top>
            //  </PagingInfo>
            // </Arguments>
    
            /// <summary>
            /// Initializes a new instance of the <see cref="AxOrderHistorySearchCriteria"/> class.
            /// </summary>
            public AxOrderHistorySearchCriteria()
            {
                this.Initialize();
            }
    
            /// <summary>
            /// Gets or sets current customer number.
            /// </summary>
            [DataMember]
            public string CustomerAccountNumber { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether to include order details (line information, order attributes etc).
            /// </summary>
            [DataMember]
            public bool IncludeDetails { get; set; }
    
            /// <summary>
            /// Gets or sets a PagingInfo object that represents a paged request.
            /// </summary>
            [DataMember]
            public PagingInfo PagingInfo { get; set; }

            /// <summary>
            /// Gets or sets the start date to fetch order history.
            /// </summary>
            [DataMember]
            public string StartDateTime { get; set; }

            /// <summary>
            /// Called when deserializing.
            /// </summary>
            /// <param name="context">The context.</param>
            [OnDeserializing]
            private void OnDeserializing(StreamingContext context)
            {
                this.Initialize();
            }
    
            /// <summary>
            /// Initializes default property values.
            /// </summary>
            /// <remarks>Being called from constructor and on deserialize.</remarks>
            private void Initialize()
            {
                // Default to search all if not specified by client.
                this.PagingInfo = PagingInfo.AllRecords;
            }
        }
    }
}
