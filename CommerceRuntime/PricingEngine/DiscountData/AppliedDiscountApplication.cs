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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Applied discount application.
        /// </summary>
        public class AppliedDiscountApplication
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AppliedDiscountApplication" /> class.
            /// </summary>
            /// <param name="discountApplication">Discount application.</param>
            /// <param name="value">The value of the discount application.</param>
            /// <param name="appliedQuantities">The applied quantities.</param>
            /// <param name="isDiscountLineGenerated">A value indicating whether the discount lines have been generated.</param>
            internal AppliedDiscountApplication(
                DiscountApplication discountApplication,
                decimal value,
                Dictionary<int, decimal> appliedQuantities,
                bool isDiscountLineGenerated)
                : this(
                    discountApplication,
                    value,
                    appliedQuantities,
                    decimal.Zero,
                    null,
                    isDiscountLineGenerated)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AppliedDiscountApplication" /> class.
            /// </summary>
            /// <param name="discountApplication">Discount application.</param>
            /// <param name="value">The value of the discount application.</param>
            /// <param name="appliedQuantities">The applied quantities.</param>
            /// <param name="totalAmountForCoveredLines">Total amount for the covered lines.</param>
            /// <param name="itemPrices">Item prices.</param>
            /// <param name="isDiscountLineGenerated">A value indicating whether the discount lines have been generated.</param>
            internal AppliedDiscountApplication(
                DiscountApplication discountApplication,
                decimal value,
                Dictionary<int, decimal> appliedQuantities,
                decimal totalAmountForCoveredLines,
                decimal[] itemPrices,
                bool isDiscountLineGenerated)
            {
                this.DiscountApplication = discountApplication;

                this.Value = value;
                this.TotalAmountForCoveredLines = totalAmountForCoveredLines;
                this.ItemPrices = itemPrices;

                this.ItemGroupIndexToDiscountLineQuantitiesLookup = new Dictionary<int, IList<DiscountLineQuantity>>();
                this.ItemQuantities = new Dictionary<int, decimal>();
                this.ItemQuantities.AddRange(appliedQuantities);

                this.IsDiscountLineGenerated = isDiscountLineGenerated;
            }

            internal DiscountApplication DiscountApplication { get; private set; }

            internal Dictionary<int, decimal> ItemQuantities { get; private set; }

            internal decimal Value { get; private set; }

            internal decimal TotalAmountForCoveredLines { get; private set; }

            internal decimal[] ItemPrices { get; private set; }

            internal IDictionary<int, IList<DiscountLineQuantity>> ItemGroupIndexToDiscountLineQuantitiesLookup { get; set; }

            internal bool IsDiscountLineGenerated { get; private set; }

#if DEBUG
            internal static void DebugAppliedDiscountApplicationList(IEnumerable<AppliedDiscountApplication> appliedDiscountApplicationList)
            {
                foreach (AppliedDiscountApplication appliedDiscountApplication in appliedDiscountApplicationList)
                {
                    appliedDiscountApplication.DebugAppliedDiscountApplication();
                }
            }
#endif

            internal void AddDiscountLine(int itemGroupIndex, DiscountLineQuantity discountLine)
            {
                if (this.ItemGroupIndexToDiscountLineQuantitiesLookup.ContainsKey(itemGroupIndex))
                {
                    this.ItemGroupIndexToDiscountLineQuantitiesLookup[itemGroupIndex].Add(discountLine);
                }
                else
                {
                    this.ItemGroupIndexToDiscountLineQuantitiesLookup.Add(itemGroupIndex, new List<DiscountLineQuantity>() { discountLine });
                }
            }

            internal void Apply(DiscountableItemGroup[] discountableItemGroups)
            {
                if (discountableItemGroups == null || discountableItemGroups.Length == 0)
                {
                    return;
                }

                foreach (var item in this.ItemGroupIndexToDiscountLineQuantitiesLookup)
                {
                    foreach (DiscountLineQuantity line in item.Value)
                    {
                        discountableItemGroups[item.Key].AddDiscountLine(line);
                    }
                }
            }

            internal void GenerateDiscountLines(DiscountableItemGroup[] discountableItemGroups, PriceContext priceContext)
            {
                if (!this.IsDiscountLineGenerated)
                {
                    this.DiscountApplication.Discount.GenerateDiscountLines(this, discountableItemGroups, priceContext);
                    this.IsDiscountLineGenerated = true;
                }
            }

#if DEBUG
            internal void DebugAppliedDiscountApplication()
            {
                string header = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0} P [{6}] deal [{1:0.##}] $ off [{2:0.##}] % off [{3:0.##}] sort [{4}] v [{5:0.##}] on ",
                    this.DiscountApplication.Discount.OfferId,
                    this.DiscountApplication.DealPriceValue,
                    this.DiscountApplication.DiscountAmountValue,
                    this.DiscountApplication.DiscountPercentValue,
                    this.DiscountApplication.SortIndex,
                    this.DiscountApplication.SortValue,
                    this.DiscountApplication.Discount.PricingPriorityNumber);

                System.Diagnostics.Debug.WriteLine(header);

                System.Text.StringBuilder quantitiesInString = new System.Text.StringBuilder();

                foreach (KeyValuePair<int, decimal> pair in this.ItemQuantities)
                {
                    int itemGroupIndex = pair.Key;
                    decimal quantity = pair.Value;

                    quantitiesInString.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " {0}x{1}", quantity, itemGroupIndex);
                }

                System.Diagnostics.Debug.WriteLine(quantitiesInString.ToString());
            }
#endif
        }
    }
}
