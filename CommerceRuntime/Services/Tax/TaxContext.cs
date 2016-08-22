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
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        internal class TaxContext
        {
            internal TaxContext()
            {
                this.TaxCodeInternalsLookup = new Dictionary<TaxDateAndGroups, ReadOnlyCollection<TaxCodeInterval>>(new TaxDateAndGroups.TaxDateAndGroupsComparer());
            }

            internal TaxContext(RequestContext context)
                : this()
            {
                this.TaxCurrencyOperations = new CurrencyOperations(context);
                this.ChannelCurrency = context.GetChannelConfiguration().Currency;
                this.NowInChannelTimeZone = context.GetNowInChannelTimeZone();
            }

            internal interface ITaxCurrencyOperations
            {
                /// <summary>
                /// Gets request context for internal use in unit tests only.
                /// </summary>
                RequestContext RequestContext { get; }

                decimal Round(decimal amountToRound, decimal roundingOff, RoundingMethod roundingMethod);

                decimal ConvertCurrency(
                    string fromCurrencyCode,
                    string toCurrencyCode,
                    decimal amountToConvert);
            }

            internal DateTimeOffset NowInChannelTimeZone { get; set; }

            internal string ChannelCurrency { get; set; }

            /// <summary>
            /// Gets or sets the tax currency operations.
            /// </summary>
            /// <remarks>Interface for test.</remarks>
            internal ITaxCurrencyOperations TaxCurrencyOperations { get; set; }

            internal Dictionary<TaxDateAndGroups, ReadOnlyCollection<TaxCodeInterval>> TaxCodeInternalsLookup { get; private set; }

            private class CurrencyOperations : ITaxCurrencyOperations
            {
                private RequestContext context;

                /// <summary>
                /// Initializes a new instance of the <see cref="CurrencyOperations" /> class.
                /// </summary>
                /// <param name="context">Commerce runtime request context.</param>
                internal CurrencyOperations(RequestContext context)
                {
                    this.context = context;
                }

                public RequestContext RequestContext
                {
                    get { return this.context; }
                }

                /// <summary>
                /// Round an amount.
                /// </summary>
                /// <param name="amountToRound">Amount to round.</param>
                /// <param name="roundingOff">Precision of rounding.</param>
                /// <param name="roundingMethod">Nearest, up or down.</param>
                /// <returns>Rounded amount.</returns>
                public decimal Round(decimal amountToRound, decimal roundingOff, RoundingMethod roundingMethod)
                {
                    return Rounding.RoundToUnit(amountToRound, roundingOff, roundingMethod);
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
}