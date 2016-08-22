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
        using Microsoft.Dynamics.Commerce.HardwareStation.Peripherals;

        /// <summary>
        /// The interface to the cash dispenser.
        /// </summary>
        public interface ICashDispenser : IPeripheral
        {
            /// <summary>
            /// Collect the change dispensed to the cash dispenser.
            /// </summary>
            /// <param name="changeValue">The change value to be dispensed.</param>
            /// <param name="currency">The currency value.</param>
            /// <returns>Returns success true/ false once the cash is dispensed.</returns>
            bool CollectChange(decimal changeValue, string currency);
        }
    }
}
