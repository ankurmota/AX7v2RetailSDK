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
        using Commerce.Runtime.Services.PricingEngine;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Channel implementation of currency conversion and rounding.
        /// </summary>
        public sealed class ChannelCurrencyOperations : ICurrencyOperations
        {
            private RequestContext context;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelCurrencyOperations" /> class.
            /// </summary>
            /// <param name="context">Commerce runtime request context.</param>
            public ChannelCurrencyOperations(RequestContext context)
            {
                this.context = context;
            }
    
            /// <summary>
            /// Round an amount.
            /// </summary>
            /// <param name="amountToRound">Amount to round.</param>
            /// <returns>Rounded amount.</returns>
            public decimal Round(decimal amountToRound)
            {
                string currency = null;
                ChannelConfiguration channelConfiguration = this.context.GetChannelConfiguration();
                if (channelConfiguration != null)
                {
                    currency = channelConfiguration.Currency;
                }
    
                var roundingRequest = new GetRoundedValueServiceRequest(amountToRound, currency, 0, useSalesRounding: false);
                var roundingResponse = this.context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
    
                return roundingResponse.RoundedValue;
            }
    
            /// <summary>
            /// Convert amount in one currency to another.
            /// </summary>
            /// <param name="fromCurrencyCode">From currency code.</param>
            /// <param name="toCurrencyCode">To currency code.</param>
            /// <param name="amountToConvert">Amount to convert.</param>
            /// <returns>Converted amount in new currency.</returns>
            public decimal ConvertCurrency(string fromCurrencyCode, string toCurrencyCode, decimal amountToConvert)
            {
                GetCurrencyValueServiceRequest currencyRequest = new GetCurrencyValueServiceRequest(fromCurrencyCode, toCurrencyCode, amountToConvert);
                GetCurrencyValueServiceResponse currencyResponse = this.context.Execute<GetCurrencyValueServiceResponse>(currencyRequest);
    
                return currencyResponse.ConvertedAmount;
            }
        }
    }
}
