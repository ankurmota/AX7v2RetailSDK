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
    namespace Commerce.Runtime.Services.PricingEngine.DiscountData
    {
        using System;
        using System.Collections.Generic;
    
        internal class ItemPriceComparer
        {
            private Dictionary<int, decimal> itemPricesForSorting;
    
            internal ItemPriceComparer(Dictionary<int, decimal> itemPricesForSorting)
            {
                this.itemPricesForSorting = itemPricesForSorting;
            }
    
            internal int CompareItemPriceByItemGroupIndexDescending(int left, int right)
            {
                // This is for sort, and it requires that itemPricesIndicedByItemGroupIndex be set up first.
                // It results in descending order by price.
                return Math.Sign(this.itemPricesForSorting[right] - this.itemPricesForSorting[left]);
            }
    
            internal Comparison<int> GetComparison()
            {
                return new Comparison<int>(this.CompareItemPriceByItemGroupIndexDescending);
            }
        }
    }
}
