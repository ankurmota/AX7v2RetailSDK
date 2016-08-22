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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Encapsulates extension methods for the <see cref="TaxInterval"/> struct.
        /// </summary>
        internal static class TaxExtensions
        {
            /// <summary>
            /// Determine whether the given amount is wholly within the interval.
            /// </summary>
            /// <param name="interval">The interval.</param>
            /// <param name="amount">The amount.</param>
            /// <returns>A value indicating whether the amount is entirely within the tax interval.</returns>
            /// <example>
            /// Interval = $25 - $100.
            /// Amount = $10 returns FALSE.
            /// Amount = $50 returns TRUE.
            /// Amount = $150 returns FALSE.
            /// </example>
            /// <remarks>This belongs to TaxInterval.</remarks>
            public static bool WholeAmountInInterval(this TaxInterval interval, decimal amount)
            {
                // return if the amount is in within the interval or equal to either end.
                return (interval.TaxLimitMin == decimal.Zero || interval.TaxLimitMin <= amount)
                    && (interval.TaxLimitMax == decimal.Zero || interval.TaxLimitMax >= amount);
            }
    
            /// <summary>
            /// Determine whether any portion of the given amount is within the given interval.
            /// </summary>
            /// <param name="interval">The interval.</param>
            /// <param name="amount">The amount.</param>
            /// <returns>A value indicating whether the amount is in the tax interval.</returns>
            /// <example>
            /// Interval = $25 - $100.
            /// Amount = $10 returns FALSE.
            /// Amount = $50 returns TRUE.
            /// Amount = $150 returns TRUE.
            /// </example>
            /// <remarks>This belongs to TaxInterval.</remarks>
            public static bool AmountInInterval(this TaxInterval interval, decimal amount)
            {
                return (interval.TaxLimitMin == decimal.Zero) || (interval.TaxLimitMin < amount);
            }
    
            /// <summary>
            /// Determine whether an interval exists that includes the given limit base.
            /// </summary>
            /// <param name="intervals">The collection of tax intervals.</param>
            /// <param name="limitBase">The limit base.</param>
            /// <returns>A value indicating whether the tax interval exists.</returns>
            /// <remarks>This belongs to TaxInterval.</remarks>
            public static bool Exists(this IEnumerable<TaxInterval> intervals, decimal limitBase)
            {
                return intervals.Any(t => t.WholeAmountInInterval(limitBase));
            }
    
            /// <summary>
            /// Retrieve the interval which includes the limit base.
            /// </summary>
            /// <param name="intervals">The collection of tax intervals.</param>
            /// <param name="limitBase">The limit base.</param>
            /// <returns>The tax interval.</returns>
            /// <remarks>This belongs to TaxInterval.</remarks>
            public static TaxInterval Find(this IEnumerable<TaxInterval> intervals, decimal limitBase)
            {
                return intervals.FirstOrDefault(t => t.WholeAmountInInterval(limitBase));
            }
        }
    }
}
