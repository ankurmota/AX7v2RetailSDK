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
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Represents the overall settings and configuration to use when calculating prices for set of lines.
        /// </summary>
        public sealed class PriceContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PriceContext"/> class.
            /// </summary>
            public PriceContext()
            {
                this.ChannelPriceGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                this.CatalogPriceGroups = new Dictionary<long, ISet<string>>();
                this.AffiliationPriceGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                this.PriceGroupIdToPriorityDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                this.PriceGroupIdsToRecordIdsDictionary = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                this.RecordIdsToPriceGroupIdsDictionary = new Dictionary<long, string>();
                this.ItemCache = new Dictionary<string, Item>(StringComparer.OrdinalIgnoreCase);
                this.NewSalesLineIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Gets the collection of channel price groups to search by.
            /// </summary>
            public ISet<string> ChannelPriceGroups { get; private set; }

            /// <summary>
            /// Gets the collection of catalog price groups to search by.
            /// </summary>
            /// <remarks>
            /// For catalog specific discounts, we need to ensure discounted items are in the catalogs.
            /// Hence, given discount price groups, we need to figure out whether an item is qualified if the discount is catalog only.
            /// More details in PriceContextHelper in price engine.
            /// </remarks>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "this is the type of the class which this type is modeling.")]
            public IDictionary<long, ISet<string>> CatalogPriceGroups { get; private set; }

            /// <summary>
            /// Gets the collection of catalog price groups to search by.
            /// </summary>
            public ISet<string> AffiliationPriceGroups { get; private set; }

            /// <summary>
            /// Gets the translation between price group identifiers and priorities.
            /// </summary>
            public IDictionary<string, int> PriceGroupIdToPriorityDictionary { get; private set; }

            /// <summary>
            /// Gets the translation between price group identifiers and record ids.
            /// </summary>
            public IDictionary<string, long> PriceGroupIdsToRecordIdsDictionary { get; private set; }

            /// <summary>
            /// Gets the translation between record ids and price group identifiers.
            /// </summary>
            public IDictionary<long, string> RecordIdsToPriceGroupIdsDictionary { get; private set; }

            /// <summary>
            /// Gets or sets the customer account number for customer-specific prices. Optional.
            /// </summary>
            public string CustomerAccount { get; set; }

            /// <summary>
            /// Gets or sets the customer price group Id for customer-specific prices. Optional.
            /// </summary>
            public string CustomerPriceGroup { get; set; }

            /// <summary>
            /// Gets or sets the customer line discount price group Id for customer-specific prices. Optional.
            /// </summary>
            public string CustomerLinePriceGroup { get; set; }

            /// <summary>
            /// Gets or sets the customer multiple line discount price group Id for customer-specific prices. Optional.
            /// </summary>
            public string CustomerMultipleLinePriceGroup { get; set; }

            /// <summary>
            /// Gets or sets the customer multiple line discount price group Id for customer-specific prices. Optional.
            /// </summary>
            public string CustomerTotalPriceGroup { get; set; }

            /// <summary>
            /// Gets or sets the date and time on which to calculate the prices.
            /// </summary>
            public DateTimeOffset ActiveDate { get; set; }

            /// <summary>
            /// Gets or sets the currency code to search by when pricing. Usually the channel's currency.
            /// </summary>
            public string CurrencyCode { get; set; }

            /// <summary>
            /// Gets or sets the configuration of which trade agreement combinations are allowed.
            /// </summary>
            public PriceParameters PriceParameters { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether prices are fetched in tax-inclusive (e.g. VAT) channel or tax-exclusive (e.g. US taxes).
            /// </summary>
            public bool IsTaxInclusive { get; set; }

            /// <summary>
            /// Gets or sets the price calculation mode for the lines being calculated.
            /// </summary>
            public PricingCalculationMode PriceCalculationMode { get; set; }

            /// <summary>
            /// Gets or sets the discount calculation mode for the lines being calculated.
            /// </summary>
            public DiscountCalculationMode DiscountCalculationMode { get; set; }

            /// <summary>
            /// Gets or sets the currency and rounding helper.
            /// </summary>
            public ICurrencyOperations CurrencyAndRoundingHelper { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to calculate for new sales lines only.
            /// </summary>
            public bool CalculateForNewSalesLinesOnly { get; set; }

            /// <summary>
            /// Gets the new sales line id set.
            /// </summary>
            public HashSet<string> NewSalesLineIdSet { get; private set; }

            /// <summary>
            /// Gets or sets the discount algorithm mode for the lines being calculated.
            /// </summary>
            public DiscountAlgorithmMode DiscountAlgorithmMode { get; set; }

            /// <summary>
            /// Gets or sets the max best deal step count for the lines being calculated.
            /// </summary>
            public int MaxBestDealAlgorithmStepCount { get; set; }

            /// <summary>
            /// Gets or sets the pricing engine diagnostics object.
            /// </summary>
            /// <remarks>This object is used to capture analytics data during pricing engine execution. This is not involved in the calculation logic.</remarks>
            public PricingEngineDiagnosticsObject PricingEngineDiagnosticsObject { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether diagnostics is collected.
            /// </summary>
            /// <remarks>Set to true to collect diagnostics data; false otherwise.</remarks>
            public bool IsDiagnosticsCollected { get; set; }

            /// <summary>
            /// Gets the item identifier to item object cache.
            /// </summary>
            internal Dictionary<string, Item> ItemCache { get; private set; }

            internal bool ExceedsMaxBestDealAlgorithmStepCount(int stepCount)
            {
                bool isExceeded = false;

                if (this.DiscountAlgorithmMode != DiscountAlgorithmMode.Exhaustive)
                {
                    isExceeded = stepCount > this.MaxBestDealAlgorithmStepCount;
                }

                return isExceeded;
            }
        }
    }
}
