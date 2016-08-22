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
        using System;
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Data Contract for GetPurchaseHistory request parameter.
        /// </summary>
        [DataContract]
        public class AxPurchaseHistorySearchCriteria
        {
            // AX Xml Schema definition.
            // <Arguments>
            //  <CustomerAccountNumber/>
            //  <LanguageId/>
            //  <PagingInfo>
            //    <Skip/>
            //    <Top/>
            //  </PagingInfo>
            // </Arguments>

            /// <summary>
            /// Initializes a new instance of the <see cref="AxPurchaseHistorySearchCriteria"/> class.
            /// </summary>
            public AxPurchaseHistorySearchCriteria()
                : this(string.Empty, string.Empty, DateTimeOffset.UtcNow.UtcDateTime.ToString(), PagingInfo.AllRecords)
            {
                this.Initialize();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AxPurchaseHistorySearchCriteria"/> class.
            /// </summary>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <param name="languageId">The language to localize the data.</param>
            /// <param name="startDateTime">The start date time to fetch data.</param>
            /// <param name="pagingInfo">The paging information.</param>
            public AxPurchaseHistorySearchCriteria(string customerAccountNumber, string languageId, string startDateTime, PagingInfo pagingInfo)
            {
                this.CustomerAccountNumber = customerAccountNumber;
                this.LanguageId = languageId;
                this.StartDateTime = startDateTime;
                this.PagingInfo = pagingInfo;
            }

            /// <summary>
            /// Gets or sets customer account number.
            /// </summary>
            [DataMember]
            public string CustomerAccountNumber { get; set; }

            /// <summary>
            /// Gets or sets language to localize the data.
            /// </summary>
            [DataMember]
            public string LanguageId { get; set; }

            /// <summary>
            /// Gets or sets a PagingInfo object that represents a paged request.
            /// </summary>
            [DataMember]
            public PagingInfo PagingInfo { get; set; }

            /// <summary>
            /// Gets or sets the start date to fetch customer purchase history.
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