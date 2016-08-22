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
        using System;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// This class encapsulates all logic for totaling a charge line once all the
        /// prices, discounts, charges, taxes, etc. have been set on it.
        /// </summary>
        public static class ChargeLineTotaler
        {
            /// <summary>
            /// Calculates the tax amount properties on this taxable item.
            /// </summary>
            /// <param name="taxableItem">The taxable item.</param>
            public static void CalculateTax(TaxableItem taxableItem)
            {
                if (taxableItem == null)
                {
                    throw new ArgumentNullException("taxableItem");
                }
    
                taxableItem.TaxAmount = taxableItem.TaxLines.Sum(t => t.IsExempt ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountExclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt || t.IsIncludedInPrice) ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountInclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt || !t.IsIncludedInPrice) ? decimal.Zero : t.Amount);
                taxableItem.TaxAmountExemptInclusive = taxableItem.TaxLines.Sum(t => (t.IsExempt && t.IsIncludedInPrice) ? t.Amount : decimal.Zero);
            }
        }
    }
}
