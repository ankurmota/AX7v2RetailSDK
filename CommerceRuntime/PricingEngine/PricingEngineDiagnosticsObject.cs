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
        using System.Collections.Generic;        
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Analytics object to hold diagnostic information generated during pricing and discount calculation.
        /// </summary>    
        [DataContract]
        public class PricingEngineDiagnosticsObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PricingEngineDiagnosticsObject"/> class.
            /// </summary>
            public PricingEngineDiagnosticsObject()
            {
                this.DiscountsConsidered = new HashSet<string>();
                this.PriceAdjustmentsConsidered = new HashSet<string>();
                this.TradeAgreementsConsidered = new List<TradeAgreement>();
            }

            /// <summary>
            /// Gets or sets discounts considered during discount calculation.
            /// </summary>       
            [DataMember]
            public HashSet<string> DiscountsConsidered { get; set; }

            /// <summary>
            /// Gets or sets trade agreements considered for pricing calculation.
            /// </summary>
            [DataMember]
            public List<TradeAgreement> TradeAgreementsConsidered { get; set; }

            /// <summary>
            /// Gets or sets price adjustments considered.
            /// </summary>
            [DataMember]
            public HashSet<string> PriceAdjustmentsConsidered { get; set; }

            /// <summary>
            /// Adds discounts considered to the existing list.
            /// </summary>
            /// <param name="discountConsidered">A list of <c>string</c>.</param>
            public void AddDiscountsConsidered(List<string> discountConsidered)
            {
                this.DiscountsConsidered.UnionWith(discountConsidered);
            }

            /// <summary>
            /// Adds price adjustments considered to the existing list.
            /// </summary>
            /// <param name="priceAdjustmentConsidered">A list of <c>string</c>.</param>
            public void AddPriceAdjustmentsConsidered(List<string> priceAdjustmentConsidered)
            {
                this.PriceAdjustmentsConsidered.UnionWith(priceAdjustmentConsidered);
            }

            /// <summary>
            /// Adds trade agreements considered to the existing list.
            /// </summary>
            /// <param name="tradeAgreementConsidered">A list of <c>TradeAgreement</c>.</param>
            public void AddTradeAgreementsConsidered(List<TradeAgreement> tradeAgreementConsidered)
            {
                this.TradeAgreementsConsidered.AddRange(tradeAgreementConsidered);
            }
        }
    }
}