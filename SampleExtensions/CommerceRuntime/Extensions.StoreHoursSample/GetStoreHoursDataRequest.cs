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
    namespace Commerce.Runtime.StoreHoursSample.Messages
    {
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// A simple request class to get a list of store hours for a store.
        /// </summary>
        [DataContract]
        public sealed class GetStoreHoursDataRequest : Request
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetStoreHoursDataRequest"/> class.
            /// </summary>
            /// <param name="storeNumber">The store number.</param>
            public GetStoreHoursDataRequest(string storeNumber)
            {
                this.StoreNumber = storeNumber;
            }

            /// <summary>
            /// Gets the store number related to the request.
            /// </summary>
            [DataMember]
            public string StoreNumber { get; private set; }
        }
    }
}