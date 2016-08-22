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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;

        /// <summary>
        /// Helper of India retail price.
        /// </summary>
        public sealed class IndiaPriceHelper
        {
            private decimal maxRetailPrice;
            private IPricingDataAccessor pricingDataManager;
            private ChannelConfiguration channelConfiguration;
            private string customerId;
            private string priceGroup;
            private SalesTransaction salesTransaction;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="IndiaPriceHelper"/> class.
            /// </summary>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <param name="pricingDataManager">Pricing data manager.</param>
            /// <param name="transaction">Current transaction.</param>
            /// /// <param name="priceGroup">Customer price group.</param>
            /// <returns>The instance of IndiaPriceHelper.</returns>
            public IndiaPriceHelper(ChannelConfiguration channelConfiguration, IPricingDataAccessor pricingDataManager, SalesTransaction transaction, string priceGroup) 
            {
                ThrowIf.Null(channelConfiguration, "channelConfiguration");
                ThrowIf.Null(pricingDataManager, "pricingDataManager");
                ThrowIf.Null(transaction, "transaction");
    
                this.channelConfiguration = channelConfiguration;
                this.pricingDataManager = pricingDataManager;
                this.salesTransaction = transaction;
                this.customerId = transaction.CustomerId;
                this.priceGroup = priceGroup;
            }
    
            /// <summary>
            /// Gets maximum retail price from trade agreement.
            /// </summary>
            /// <param name="salesLine">The sales line.</param>
            /// <param name="context">The request context.</param>
            /// <returns>Maximum retail price from trade agreement.</returns>
            public decimal GetMaximumRetailPriceFromTradeAgreement(SalesLine salesLine, RequestContext context)
            {
                if (salesLine == null)
                {
                    throw new ArgumentNullException("salesLine");
                }
    
                decimal quantity = this.salesTransaction.ActiveSalesLines.Where(x => (x.ItemId == salesLine.ItemId) && (x.InventoryDimensionId == salesLine.InventoryDimensionId)).Sum(x => x.Quantity);
                if (quantity == decimal.Zero)
                {
                    quantity = 1;
                }
    
                if (!salesLine.BeginDateTime.IsValidAxDateTime())
                {
                    salesLine.BeginDateTime = context.GetNowInChannelTimeZone();
                }
    
                PriceResult priceResult = PricingEngine.GetActiveTradeAgreement(
                    this.pricingDataManager,
                    DiscountParameters.CreateAndInitialize(this.pricingDataManager),
                    this.channelConfiguration.Currency,
                    salesLine,
                    quantity,
                    this.customerId,
                    this.priceGroup,
                    salesLine.BeginDateTime);
    
                this.maxRetailPrice = priceResult.MaximumRetailPriceIndia;
    
                return this.maxRetailPrice;
            }
        }
    }
}
