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
    namespace Commerce.HardwareStation.CashDispenserSample
    {
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.HardwareStation.Models;

        /// <summary>
        /// The cash dispenser controller request class.
        /// </summary>
        [DataContract]
        public class CashDispenserRequest : PeripheralRequest
        {
            /// <summary>
            /// Gets or sets the value for change in decimal. 
            /// </summary>
            [DataMember]
            public decimal Change { get; set; }

            /// <summary>
            /// Gets or sets the value of the Currency.
            /// </summary>
            [DataMember]
            public string Currency { get; set; }
        }
    }
}