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
    namespace Commerce.Runtime.Services.PricingEngine
    {
        /// <summary>
        /// Interface for currency operations: rounding and currency conversion.
        /// </summary>
        public interface ICurrencyOperations
        {
            /// <summary>
            /// Round an amount.
            /// </summary>
            /// <param name="amountToRound">Amount to round.</param>
            /// <returns>Rounded amount.</returns>
            decimal Round(decimal amountToRound);
    
            /// <summary>
            /// Convert amount from one currency to another.
            /// </summary>
            /// <param name="fromCurrencyCode">From currency code.</param>
            /// <param name="toCurrencyCode">To currency code.</param>
            /// <param name="amountToConvert">Amount to convert.</param>
            /// <returns>Converted amount in new currency.</returns>
            decimal ConvertCurrency(
                string fromCurrencyCode,
                string toCurrencyCode,
                decimal amountToConvert);
        }
    }
}
