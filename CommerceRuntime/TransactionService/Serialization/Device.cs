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
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Represents device entity used for deserializing transaction service return result.
        /// </summary>
        /// <remarks>The <see cref="Order"/> property of <see cref="DataMemberAttribute"/> must match the Xml elements sequence.</remarks>
        [DataContract]
        public class Device
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Device"/> class.
            /// </summary>
            public Device()
            {
                this.DeviceNumber = string.Empty;
                this.ActivationStatusValue = (int)DeviceActivationStatus.None;
            }

            /// <summary>
            /// Gets or sets the device number.
            /// </summary>
            [DataMember(Order = 1)]
            public string DeviceNumber { get; set; }

            /// <summary>
            /// Gets or sets the device type value.
            /// </summary>
            [DataMember(Order = 2)]
            public int DeviceType { get; set; }

            /// <summary>
            /// Gets or sets the device activation status.
            /// </summary>
            [DataMember(Order = 3)]
            public int ActivationStatusValue { get; set; }
        }
    }
}