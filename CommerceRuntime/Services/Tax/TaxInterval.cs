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
        /// Represents a tax interval.
        /// </summary>
        /// <remarks>For tiered tax.</remarks>
        internal struct TaxInterval
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaxInterval"/> struct.
            /// </summary>
            /// <param name="min">The minimum.</param>
            /// <param name="max">The maximum.</param>
            /// <param name="value">The value.</param>
            public TaxInterval(decimal min, decimal max, decimal value)
                : this()
            {
                this.TaxLimitMax = max;
                this.TaxLimitMin = min;
                this.Value = value;
            }
    
            /// <summary>
            /// Gets the tax limit minimum.
            /// </summary>
            public decimal TaxLimitMin { get; private set; }
    
            /// <summary>
            /// Gets the tax limit maximum.
            /// </summary>
            public decimal TaxLimitMax { get; private set; }
    
            /// <summary>
            /// Gets the tax value.
            /// </summary>
            public decimal Value { get; private set; }
        }
    }
}
