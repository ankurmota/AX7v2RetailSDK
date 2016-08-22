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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates the implementation of the currency service.
        /// </summary>
        public class CurrencyService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetCurrencyValueServiceRequest),
                        typeof(GetChannelCurrencyServiceRequest),
                        typeof(CalculateTotalAmountServiceRequest),
                        typeof(GetExchangeRateServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Entry point to Currency service. Takes a Currency service request and returns the result
            /// of the request execution.
            /// </summary>
            /// <param name="request">The Currency service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetCurrencyValueServiceRequest))
                {
                    response = CurrencyToCurrency((GetCurrencyValueServiceRequest)request);
                }
                else if (requestType == typeof(GetChannelCurrencyServiceRequest))
                {
                    response = GetSupportedChannelCurrencies((GetChannelCurrencyServiceRequest)request);
                }
                else if (requestType == typeof(CalculateTotalAmountServiceRequest))
                {
                    response = CalculateTotalCurrencyAmount((CalculateTotalAmountServiceRequest)request);
                }
                else if (requestType == typeof(GetExchangeRateServiceRequest))
                {
                    response = GetExchangeRate((GetExchangeRateServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Returns the valid exchange rate for a given pair of currencies.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="fromCurrency">The from currency.</param>
            /// <param name="toCurrency">The to currency.</param>
            /// <returns>The unrounded exchange rate.</returns>
            private static GetExchangeRateServiceResponse GetExchangeRate(RequestContext context, string fromCurrency, string toCurrency)
            {
                if (fromCurrency.Trim().ToLower() == toCurrency.Trim().ToLower())
                {
                    return new GetExchangeRateServiceResponse(fromCurrency, toCurrency, 1M);
                }
    
                decimal exchangeRate = 0.00M;
    
                DateTime channelDateTime = context.GetNowInChannelTimeZone().DateTime;
    
                GetExchangeRatesDataRequest dataRequest = new GetExchangeRatesDataRequest(fromCurrency, toCurrency, channelDateTime);
                dataRequest.QueryResultSettings = QueryResultSettings.AllRecords;
                var exchangeRates = context.Execute<EntityDataServiceResponse<ExchangeRate>>(dataRequest).PagedEntityCollection.Results;
    
                // try to find forward exchange rate
                var rate = exchangeRates.SingleOrDefault(er => (er.FromCurrency == fromCurrency) && (er.ToCurrency == toCurrency));
                if (rate != null)
                {
                    exchangeRate = rate.Rate / 100M;
                }
                else
                {
                    // otherwise try to find backward exchange rate and reciprocate it
                    rate = exchangeRates.SingleOrDefault(er => (er.FromCurrency == toCurrency) && (er.ToCurrency == fromCurrency));
                    if (rate != null)
                    {
                        exchangeRate = 100M / rate.Rate;
                    }
                }
    
                return new GetExchangeRateServiceResponse(fromCurrency, toCurrency, exchangeRate);
            }
    
            /// <summary>
            /// Internal wrapper for GetExchangeRate. This derives the parameters from a currency request instance.
            /// </summary>
            /// <param name="request">Currency request with retail runtime and currencies specified.</param>
            /// <returns>The unrounded exchange rate.</returns>
            private static GetExchangeRateServiceResponse GetExchangeRate(GetExchangeRateServiceRequest request)
            {
                return GetExchangeRate(request.RequestContext, request.FromCurrencyCode, request.ToCurrencyCode);
            }
    
            /// <summary>
            /// Calculate the total currency amount after converting to channel currency for the given list of currencies.
            /// </summary>
            /// <param name="request">The list of currencies that requires to be summed.</param>
            /// <returns>Returns the response that contains the sum of currencies amount.</returns>
            private static CalculateTotalAmountServiceResponse CalculateTotalCurrencyAmount(CalculateTotalAmountServiceRequest request)
            {
                var channelConfiguration = request.RequestContext.GetChannelConfiguration();
                decimal totalAmount = 0m;
    
                foreach (var currency in request.CurrenciesToConvert)
                {
                    var getCurrencyValueRequest = new GetCurrencyValueServiceRequest(currency.CurrencyCode, channelConfiguration.Currency, currency.AmountToConvert);
                    GetCurrencyValueServiceResponse response = request.RequestContext.Execute<GetCurrencyValueServiceResponse>(getCurrencyValueRequest);
                    totalAmount += response.RoundedConvertedAmount;
                }
    
                return new CalculateTotalAmountServiceResponse(new CurrencyAmount { RoundedConvertedAmount = totalAmount, CurrencyCode = channelConfiguration.Currency });
            }
    
            /// <summary>
            /// Gets all supported channel currency value and exchange rate for the given amount.
            /// </summary>
            /// <param name="request">Request contains the amount to be converted.</param>
            /// <returns>The converted amount with exchange rates.</returns>
            private static GetChannelCurrencyServiceResponse GetSupportedChannelCurrencies(GetChannelCurrencyServiceRequest request)
            {
                string fromCurrencyCode = request.CurrencyCode;
    
                GetChannelCurrenciesDataRequest dataRequest = new GetChannelCurrenciesDataRequest(request.QueryResultSettings ?? QueryResultSettings.AllRecords);
                PagedResult<CurrencyAmount> pagedChannelCurrencies = request.RequestContext.Execute<EntityDataServiceResponse<CurrencyAmount>>(dataRequest).PagedEntityCollection;
                ReadOnlyCollection<CurrencyAmount> channelCurrencies = pagedChannelCurrencies.Results;
    
                if (channelCurrencies == null || !channelCurrencies.Any())
                {
                    NetTracer.Warning("Cannot find channel currencies");
                    return new GetChannelCurrencyServiceResponse();
                }
    
                var currencyList = channelCurrencies.ToList();
    
                foreach (var toCurrency in currencyList)
                {
                    var getCurrencyValueRequest = new GetCurrencyValueServiceRequest(fromCurrencyCode, toCurrency.CurrencyCode, request.Amount);
                    GetCurrencyValueServiceResponse serviceResponse = GetCurrencyConversion(request.RequestContext, getCurrencyValueRequest);
                    toCurrency.ExchangeRate = serviceResponse.ExchangeRate;
                    toCurrency.ConvertedAmount = serviceResponse.ConvertedAmount;
                    toCurrency.RoundedConvertedAmount = serviceResponse.RoundedConvertedAmount;
                }
    
                var storeCurrencyList = currencyList.Where(currency => currency.ExchangeRate > 0M).ToList();
    
                // If the from currency does not exists add to the list.
                if (storeCurrencyList.All(currency => string.CompareOrdinal(currency.CurrencyCode, fromCurrencyCode) != 0))
                {
                    CurrencyAmount conversionResult = GetFromCurrencyAmount(request.RequestContext, request.Amount, fromCurrencyCode);
                    storeCurrencyList.Add(conversionResult);
                }
    
                pagedChannelCurrencies.Results = storeCurrencyList.AsReadOnly();
    
                return new GetChannelCurrencyServiceResponse(pagedChannelCurrencies);
            }
    
            /// <summary>
            /// Gets the currency amount object for the given currency code.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="amount">The currency amount.</param>
            /// <param name="currency">The currency code.</param>
            /// <returns>The currency amount object.</returns>
            private static CurrencyAmount GetFromCurrencyAmount(RequestContext context, decimal amount, string currency)
            {
                var getCurrenciesDataRequest = new GetCurrenciesDataRequest(currency, QueryResultSettings.SingleRecord);
                Currency currencyValue = context.Runtime.Execute<EntityDataServiceResponse<Currency>>(getCurrenciesDataRequest, context).PagedEntityCollection.FirstOrDefault();
    
                if (currencyValue == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyNotFound,
                        string.Format("The given currency: {0} is not found.", currency));
                }
    
                decimal roundedAmount = RoundAmount(context, amount, currency);
    
                return new CurrencyAmount
                    {
                        ConvertedAmount = amount,
                        RoundedConvertedAmount = roundedAmount,
                        CurrencyCode = currency,
                        ExchangeRate = 1,   // from currency always defaults to 1.
                        CurrencySymbol = currencyValue.CurrencySymbol,
                    };
            }
    
            /// <summary>
            /// Converts a value from one currency to another.
            /// </summary>
            /// <param name="request">Currency request specifying retail runtime, source and destination currencies, and amount to convert.</param>
            /// <returns>
            /// The value as it is after conversion in the destination currency, rounded according to the destination currency's rounding setup.
            /// </returns>
            private static GetCurrencyValueServiceResponse CurrencyToCurrency(GetCurrencyValueServiceRequest request)
            {
                GetCurrencyValueServiceResponse response = GetCurrencyConversion(request.RequestContext, request);
    
                if (response.ExchangeRate <= 0)
                {
                    throw new DataValidationException(
                            DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyConversionFailed,
                            string.Format("Exchange rate from currency '{0}' to currency '{1}' is not supported or misconfigured. Calculated exchange rate: {2}", response.FromCurrencyCode, response.ToCurrencyCode, response.ExchangeRate));
                }
    
                return response;
            }
    
            /// <summary>
            /// Converts a value from one currency to another.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="request">Currency request specifying retail runtime, source and destination currencies, and amount to convert.</param>
            /// <returns>
            /// The value as it is after conversion in the destination currency, rounded according to the destination currency's rounding setup.
            /// </returns>
            private static GetCurrencyValueServiceResponse GetCurrencyConversion(RequestContext context, GetCurrencyValueServiceRequest request)
            {
                string fromCurrencyCode = request.FromCurrencyCode;
                string toCurrencyCode = request.ToCurrencyCode;
                decimal amountToConvert = request.AmountToConvert;
    
                // If from and to currency is the same one return the original value
                if (fromCurrencyCode.Trim() == toCurrencyCode.Trim())
                {
                    decimal roundedAmount = RoundAmount(context, amountToConvert, toCurrencyCode);
                    return new GetCurrencyValueServiceResponse(fromCurrencyCode, toCurrencyCode, amountToConvert, amountToConvert, roundedAmount);
                }
    
                // If the value to be converted is 0 then just return 0
                if (amountToConvert == 0M)
                {
                    return new GetCurrencyValueServiceResponse(fromCurrencyCode, toCurrencyCode, amountToConvert, amountToConvert, amountToConvert);
                }
    
                decimal exchangeRate = CalculateExchangeRate(context, request.FromCurrencyCode, request.ToCurrencyCode);
                decimal convertedAmount = 0m;
                decimal roundedConvertedAmount = 0m;
    
                if (exchangeRate > 0)
                {
                    convertedAmount = amountToConvert * exchangeRate;
                    roundedConvertedAmount = RoundAmount(context, convertedAmount, toCurrencyCode);
                }
    
                return new GetCurrencyValueServiceResponse(fromCurrencyCode, toCurrencyCode, amountToConvert, convertedAmount, roundedConvertedAmount, exchangeRate);
            }
    
            /// <summary>
            /// Calculates the exchange rates.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="fromCurrencyCode">The from currency.</param>
            /// <param name="toCurrencyCode">The to currency.</param>
            /// <returns>The unrounded exchange rate.</returns>
            private static decimal CalculateExchangeRate(RequestContext context, string fromCurrencyCode, string toCurrencyCode)
            {
                decimal exchangeRate = GetExchangeRate(context, fromCurrencyCode, toCurrencyCode).ExchangeRate;
    
                // try a cross exchange rate
                if (exchangeRate == 0M)
                {
                    var channelConfiguration = context.GetChannelConfiguration();
    
                    // From -> Store
                    decimal currencyExchangeRate = GetExchangeRate(context, fromCurrencyCode, channelConfiguration.Currency).ExchangeRate;
    
                    // Store -> To
                    decimal storeExchangeRate = GetExchangeRate(context, channelConfiguration.Currency, toCurrencyCode).ExchangeRate;
    
                    // From -> Store -> To
                    exchangeRate = currencyExchangeRate * storeExchangeRate;
                }
    
                return exchangeRate;
            }
    
            /// <summary>
            /// Round amount using rounding service.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="amountToRound">Amount to round.</param>
            /// <param name="currencyCode">Currency code of amount.</param>
            /// <returns>Rounded amount.</returns>
            private static decimal RoundAmount(RequestContext context, decimal amountToRound, string currencyCode)
            {
                var roundingRequest = new GetRoundedValueServiceRequest(amountToRound, currencyCode);
                GetRoundedValueServiceResponse roundingResponse = context.Execute<GetRoundedValueServiceResponse>(roundingRequest);
                return roundingResponse.RoundedValue;
            }
        }
    }
}
