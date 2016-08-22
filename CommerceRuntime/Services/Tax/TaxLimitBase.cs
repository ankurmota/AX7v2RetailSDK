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
        /// Represents a tax limit basis.
        /// </summary>
        /// <remarks>This is tax code marginal base in AX (TaxTable.TaxLimitBase).</remarks>
        public enum TaxLimitBase
        {
            /// <summary>
            /// A line without value-added tax.
            /// </summary>
            LineWithoutVat = 0,
    
            /// <summary>
            /// A unit without value-added tax.
            /// </summary>
            UnitWithoutVat = 1,
    
            /// <summary>
            /// An invoice without value-added tax.
            /// </summary>
            InvoiceWithoutVat = 2,
    
            /// <summary>
            /// A line with value-added tax.
            /// </summary>
            LineWithVat = 3,
    
            /// <summary>
            /// A unit with value-added tax.
            /// </summary>
            UnitWithVat = 4,
    
            /// <summary>
            /// An invoice with value-added tax.
            /// </summary>
            InvoiceWithVat = 5
        }
    }
}
