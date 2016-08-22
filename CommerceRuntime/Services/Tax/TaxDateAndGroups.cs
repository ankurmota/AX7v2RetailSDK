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
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Tax data, sales tax group and item tax group.
        /// </summary>
        public class TaxDateAndGroups
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaxDateAndGroups"/> class.
            /// </summary>
            /// <param name="taxDate">Tax date.</param>
            /// <param name="salesTaxGroup">Sales tax group.</param>
            /// <param name="itemTaxGroup">Item tax group.</param>
            public TaxDateAndGroups(DateTimeOffset taxDate, string salesTaxGroup, string itemTaxGroup)
            {
                this.TaxDate = taxDate;
                this.SalesTaxGroup = string.IsNullOrWhiteSpace(salesTaxGroup) ? string.Empty : salesTaxGroup.Trim().ToUpper();
                this.ItemTaxGroup = string.IsNullOrWhiteSpace(itemTaxGroup) ? string.Empty : itemTaxGroup.Trim().ToUpper();
            }
    
            internal DateTimeOffset TaxDate { get; private set; }
    
            internal string SalesTaxGroup { get; private set; }
    
            internal string ItemTaxGroup { get; private set; }
    
            internal bool IsNoTax
            {
                get
                {
                    return string.IsNullOrWhiteSpace(this.SalesTaxGroup) || string.IsNullOrWhiteSpace(this.ItemTaxGroup);
                }
            }
    
            internal class TaxDateAndGroupsComparer : IEqualityComparer<TaxDateAndGroups>
            {
                /// <summary>
                /// Checks whether two values are equal.
                /// </summary>
                /// <param name="x">One of <see cref="TaxDateAndGroups"/>.</param>
                /// <param name="y">Another one of <see cref="TaxDateAndGroups"/>.</param>
                /// <returns>true if equal; false otherwise.</returns>
                public bool Equals(TaxDateAndGroups x, TaxDateAndGroups y)
                {
                    bool areEqual = false;
    
                    if (x != null && y != null)
                    {
                        areEqual = x.TaxDate == y.TaxDate && string.Equals(x.SalesTaxGroup, y.SalesTaxGroup, StringComparison.OrdinalIgnoreCase) && string.Equals(x.ItemTaxGroup, y.ItemTaxGroup, StringComparison.OrdinalIgnoreCase);
                    }
    
                    return areEqual;
                }
    
                /// <summary>
                /// Gets the hash code of <see cref="TaxDateAndGroups"/>.
                /// </summary>
                /// <param name="taxDateAndGroups">Tax date and groups.</param>
                /// <returns>The hash code.</returns>
                public int GetHashCode(TaxDateAndGroups taxDateAndGroups)
                {
                    int hashCode = 0;
                    if (taxDateAndGroups != null)
                    {
                        hashCode = taxDateAndGroups.TaxDate.GetHashCode() + taxDateAndGroups.SalesTaxGroup.GetHashCode() + taxDateAndGroups.ItemTaxGroup.GetHashCode();
                    }
    
                    return hashCode;
                }
            }
        }
    }
}
