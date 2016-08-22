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
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Arguments for a Price Agreement lookup operation and methods for reading.
        /// </summary>
        internal struct PriceAgreementArgs
        {
            /// <summary>
            /// The optional customer account number to consider.
            /// </summary>
            public string CustomerId;
    
            /// <summary>
            /// The price group Ids to search by.
            /// </summary>
            public ReadOnlyCollection<string> PriceGroups;
    
            /// <summary>
            /// The currency code to filter by.
            /// </summary>
            public string CurrencyCode;
    
            /// <summary>
            /// The quantity of the item or total cost to consider.
            /// </summary>
            public decimal Quantity;
    
            /// <summary>
            /// The item Id to find a price for.
            /// </summary>
            public string ItemId;
    
            /// <summary>
            /// The sales unit of measure of the item. Used to search by unit.
            /// </summary>
            public string UnitOfMeasure;
    
            /// <summary>
            /// Optional parameter which specifies the product variant dimensions to consider for price search.
            /// </summary>
            public ProductVariant Dimensions;
    
            /// <summary>
            /// Gets a value indicating whether the item is a variant.
            /// </summary>
            public bool IsVariant
            {
                get
                {
                    return !(string.IsNullOrWhiteSpace(this.Dimensions.ConfigId)
                          && string.IsNullOrWhiteSpace(this.Dimensions.ColorId)
                          && string.IsNullOrWhiteSpace(this.Dimensions.SizeId)
                          && string.IsNullOrWhiteSpace(this.Dimensions.StyleId));
                }
            }
    
            /// <summary>
            /// Gets price agreement 'item relation' based on arguments and given item relation code.
            /// </summary>
            /// <param name="itemCode">Item relation code (item/group/all).</param>
            /// <returns>
            /// Returns item if 'item' relation code given, otherwise empty string.
            /// </returns>
            public string GetItemRelation(PriceDiscountItemCode itemCode)
            {
                string itemRelation = string.Empty;
                if (itemCode == PriceDiscountItemCode.Item && !string.IsNullOrEmpty(this.ItemId))
                {
                    itemRelation = this.ItemId;
                }
    
                return itemRelation;
            }
    
            /// <summary>
            /// Gets price agreement 'account relations' based on arguments and given account relation code.
            /// </summary>
            /// <param name="accountCode">Account relation code (customer/price group/all).</param>
            /// <returns>
            /// Returns customer if 'customer' code given, price groups if 'group' code given, otherwise empty.
            /// </returns>
            public ReadOnlyCollection<string> GetAccountRelations(PriceDiscountAccountCode accountCode)
            {
                ReadOnlyCollection<string> accountRelations = new ReadOnlyCollection<string>(new List<string> { string.Empty });
                if (accountCode == PriceDiscountAccountCode.Customer && !string.IsNullOrEmpty(this.CustomerId))
                {
                    accountRelations = new ReadOnlyCollection<string>(new List<string> { this.CustomerId });
                }
                else if (accountCode == PriceDiscountAccountCode.CustomerGroup && this.PriceGroups.Count > 0)
                {
                    accountRelations = this.PriceGroups;
                }
    
                return accountRelations;
            }
    
            /// <summary>
            /// Gets price agreement unit of measure based on arguments and given item relation code.
            /// </summary>
            /// <param name="itemCode">Item relation code (item/group/all).</param>
            /// <returns>
            /// Return unit of measure id if 'item' code specified, otherwise empty.
            /// </returns>
            public string GetUnitId(PriceDiscountItemCode itemCode)
            {
                return itemCode == PriceDiscountItemCode.Item && !string.IsNullOrEmpty(this.UnitOfMeasure) ? this.UnitOfMeasure : string.Empty;
            }
        }
    }
}
