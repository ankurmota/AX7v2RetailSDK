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
    namespace Commerce.Runtime.CrossLoyaltySample.Messages
    {
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Represents the response object for the <see cref="GetCrossLoyaltyCardRequest"/> class.
        /// </summary>
        [DataContract]
        public sealed class GetCrossLoyaltyCardResponse : Response
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GetCrossLoyaltyCardResponse"/> class.
            /// </summary>
            /// <param name="discount">The discount calculated.</param>
            public GetCrossLoyaltyCardResponse(decimal discount)
            {
                this.Discount = discount;
            }

            /// <summary>
            /// Gets the calculated discount value.
            /// </summary>
            [DataMember]
            public decimal Discount { get; private set; }
        }
    }
}