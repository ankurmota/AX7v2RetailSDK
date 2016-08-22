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
    namespace Commerce.Runtime.Services
    {
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Represents the mode of tax calculation.
        /// </summary>
        public enum TaxCalculationMode
        {
            /// <summary>
            /// A mode where tax is calculated based on full amounts.
            /// </summary>
            FullAmounts = 0,
    
            /// <summary>
            /// A mode where tax is calculated for each interval.
            /// </summary>
            Interval = 1
        }
    }
}
