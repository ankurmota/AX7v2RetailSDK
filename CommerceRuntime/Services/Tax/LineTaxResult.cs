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
        using System.Diagnostics;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents a tax line result.
        /// </summary>
        [DebuggerDisplay("TaxAmount: {TaxAmount}, HasExempt: {HasExempt}, ExemptAmount: {ExemptAmount}")]
        public sealed class LineTaxResult
        {
            /// <summary>
            /// Gets or sets a value indicating whether this tax line is exempt.
            /// </summary>
            /// <value>
            /// If <c>true</c> this line tax result is exempt; otherwise, <c>false</c>.
            /// </value>
            public bool HasExempt { get; set; }
    
            /// <summary>
            /// Gets or sets the tax amount.
            /// </summary>
            /// <value>
            /// The tax amount.
            /// </value>
            public decimal TaxAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the exempt amount.
            /// </summary>
            /// <value>
            /// The exempt amount.
            /// </value>
            public decimal ExemptAmount { get; set; }
    
            /// <summary>
            /// Gets or sets the tax rate percentage.
            /// </summary>
            /// <value>
            /// The tax rate percentage.
            /// </value>
            public decimal TaxRatePercent { get; set; }
        }
    }
}
