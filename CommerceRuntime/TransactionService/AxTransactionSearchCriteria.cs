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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Serialization;
    
        /// <summary>
        /// Data Contract for GetJournalListSearch request parameter.
        /// </summary>
        [DataContract]
        public class AxTransactionSearchCriteria
        {
            // AX Xml Schema definition.
            // <Arguments>
            //  <CurrentChannelId/>
            //  <TransactionIds/>
            //  <ReceiptId/>
            //  <ChannelReferenceId/>
            //  <CustomerAccountNumber/>
            //  <CustomerFirstName/>
            //  <CustomerLastName/>
            //  <StoreId/>
            //  <TerminalId/>
            //  <ItemId/>
            //  <Barcode/>
            //  <StaffId/>
            //  <StartDateTime/>
            //  <EndDateTime/>
            //  <ReceiptEmailAddress/>
            //  <SerialNumber/>
            //  <IncludeDetails/>
            //  <PagingInfo>
            //    <Skip/>
            //    <Top/>
            //  </PagingInfo>
            // </Arguments>
    
            /// <summary>
            /// Initializes a new instance of the <see cref="AxTransactionSearchCriteria"/> class.
            /// </summary>
            public AxTransactionSearchCriteria()
            {
                this.Initialize();
            }
    
            /// <summary>
            /// Gets or sets current channel identifier.
            /// </summary>
            [DataMember]
            public long CurrentChannelId { get; set; }
    
            /// <summary>
            /// Gets a collection transaction identifier.
            /// </summary>
            [DataMember]
            public Collection<string> TransactionIds { get; private set; }
    
            /// <summary>
            /// Gets or sets transaction receipt identifier.
            /// </summary>
            [DataMember]
            public string ReceiptId { get; set; }
    
            /// <summary>
            /// Gets or sets channel reference identifier.
            /// </summary>
            [DataMember]
            public string ChannelReferenceId { get; set; }
    
            /// <summary>
            /// Gets or sets customer account number.
            /// </summary>
            [DataMember]
            public string CustomerAccountNumber { get; set; }
    
            /// <summary>
            /// Gets or sets customer first name.
            /// </summary>
            [DataMember]
            public string CustomerFirstName { get; set; }
    
            /// <summary>
            /// Gets or sets customer last name.
            /// </summary>
            [DataMember]
            public string CustomerLastName { get; set; }
    
            /// <summary>
            /// Gets or sets the retail store identifier for the sales order.
            /// </summary>
            [DataMember]
            public string StoreId { get; set; }
    
            /// <summary>
            ///  Gets or sets the terminal identifier.
            /// </summary>
            [DataMember]
            public string TerminalId { get; set; }
    
            /// <summary>
            ///  Gets or sets the item identifier.
            /// </summary>
            [DataMember]
            public string ItemId { get; set; }
    
            /// <summary>
            ///  Gets or sets the item barcode.
            /// </summary>
            [DataMember]
            public string BarCode { get; set; }
    
            /// <summary>
            ///  Gets or sets the staff identifier.
            /// </summary>
            [DataMember]
            public string StaffId { get; set; }
    
            /// <summary>
            ///  Gets or sets the sales identifier.
            /// </summary>
            [DataMember]
            public string SalesId { get; set; }
    
            /// <summary>
            ///  Gets or sets the transaction start date time.
            /// </summary>
            [DataMember]
            public string StartDateTime { get; set; }
    
            /// <summary>
            ///  Gets or sets the transaction end date time.
            /// </summary>
            [DataMember]
            public string EndDateTime { get; set; }
    
            /// <summary>
            ///  Gets or sets the receipt email address.
            /// </summary>
            [DataMember]
            public string ReceiptEmailAddress { get; set; }
    
            /// <summary>
            ///  Gets or sets the item serial number.
            /// </summary>
            [DataMember]
            public string SerialNumber { get; set; }
    
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
                this.TransactionIds = new Collection<string>();
                this.PagingInfo = PagingInfo.AllRecords;
            }
        }
    }
}
