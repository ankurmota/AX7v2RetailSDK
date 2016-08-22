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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates the implementation of the rounding service.
        /// </summary>
        public class RoundingService : IRequestHandler
        {
            private const int DefaultRoundingPrecision = 2;
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetRoundedValueServiceRequest),
                        typeof(GetPaymentRoundedValueServiceRequest),
                        typeof(GetRoundedStringServiceRequest),
                        typeof(GetRoundQuantityServiceRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(GetRoundedValueServiceRequest))
                {
                    response = this.GetRoundedValue((GetRoundedValueServiceRequest)request);
                }
                else if (requestType == typeof(GetPaymentRoundedValueServiceRequest))
                {
                    response = this.GetPaymentRoundedValue((GetPaymentRoundedValueServiceRequest)request);
                }
                else if (requestType == typeof(GetRoundedStringServiceRequest))
                {
                    response = this.GetRoundedString((GetRoundedStringServiceRequest)request);
                }
                else if (requestType == typeof(GetRoundQuantityServiceRequest))
                {
                    response = this.RoundQuantity((GetRoundQuantityServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private static RoundingUnitAndMethod GetRoundingUnitAndMethod(RequestContext context, string currencyCode, bool useSalesRounding)
            {
                GetCurrencyByCodeDataRequest dataRequest = new GetCurrencyByCodeDataRequest(currencyCode, new ColumnSet());
                var currency = context.Execute<SingleEntityDataServiceResponse<Currency>>(dataRequest).Entity;
    
                decimal roundingUnit = Rounding.DefaultRoundingValue;
                RoundingMethod roundingMethod = RoundingMethod.Nearest;
    
                if (currency != null)
                {
                    roundingUnit = useSalesRounding ? currency.RoundOffSales : currency.RoundOffPrice;
                    if (roundingUnit == decimal.Zero)
                    {
                        roundingUnit = Rounding.DefaultRoundingValue;
                    }
    
                    int roundoffType = useSalesRounding ? currency.RoundOffTypeSales : currency.RoundOffTypePrice;
                    roundingMethod = Rounding.ConvertRoundOffTypeToRoundingMethod(roundoffType);
                }
                else
                {
                    NetTracer.Warning("No currency found for currency code {0}. Falling back to default rounding value ({1}).", currencyCode, Rounding.DefaultRoundingValue);
                }
    
                return new RoundingUnitAndMethod(roundingUnit, roundingMethod);
            }
    
            /// <summary>
            /// Returns a string to give the correct number number of decimals depending on the currency unit.
            /// 1 will give return N0, 0,1 returns N1, 0,01 returns N2 etc.
            /// </summary>
            /// <param name="currencyUnit">The smallest currency unit.</param>
            /// <returns>Returns the format string N0,N1, etc for the ToString function.</returns>
            private static string NumberFormat(decimal currencyUnit)
            {
                string result = "N";
                if (Math.Round(currencyUnit) > 0)
                {
                    result += "0";
                }
                else
                {
                    for (int i = 1; i < 9; i++)
                    {
                        decimal factor = (decimal)Math.Pow(10, i);
                        decimal multipl = currencyUnit * factor;
                        if (multipl == Math.Round(multipl))
                        {
                            result += i.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                    }
                }
    
                return result;
            }
    
            /// <summary>
            /// Standard round to minimal coin/amount.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Rounded value.</returns>
            private GetRoundedStringServiceResponse GetRoundedString(GetRoundedStringServiceRequest request)
            {
                string currencyCode = request.CurrencyCode;
                decimal currencyUnit = 0.0m;
    
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    var channelConfiguration = request.RequestContext.GetChannelConfiguration();
    
                    currencyCode = channelConfiguration.Currency;
                }
    
                RoundingUnitAndMethod unitAndMethod = GetRoundingUnitAndMethod(request.RequestContext, currencyCode, request.UseSalesRounding);
                if (request.NumberOfDecimals != 0)
                {
                    currencyUnit = 1.0M / (decimal)Math.Pow(10, request.NumberOfDecimals);
                }
                else
                {
                    currencyUnit = unitAndMethod.RoundingUnit;
                }
    
                string format = NumberFormat(currencyUnit);
                decimal value;
    
                value = !request.IsRounded
                    ? Rounding.RoundToUnit(request.Value, currencyUnit, unitAndMethod.RoundingMethod)
                    : request.Value;
    
                string roundedString = value.ToString(format, CultureInfo.InvariantCulture);
                return new GetRoundedStringServiceResponse(roundedString);
            }
    
            /// <summary>
            /// Round quantity for the given unit of measure.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Rounded value.</returns>
            private GetRoundQuantityServiceResponse RoundQuantity(GetRoundQuantityServiceRequest request)
            {
                var unitsOfMeasureIds = new List<string> { request.UnitOfMeasure };
                var getUnitsOfMeasureByUnitIdDataRequest = new GetUnitsOfMeasureDataRequest(unitsOfMeasureIds, QueryResultSettings.SingleRecord);
                UnitOfMeasure uom = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<UnitOfMeasure>>(getUnitsOfMeasureByUnitIdDataRequest, request.RequestContext).PagedEntityCollection.FirstOrDefault();
    
                int decimalPrecision = DefaultRoundingPrecision;
    
                if (uom != null)
                {
                    decimalPrecision = uom.DecimalPrecision;
                }
    
                string roundedString = request.Value.ToString(string.Format("N{0}", decimalPrecision));
    
                return new GetRoundQuantityServiceResponse(Convert.ToDecimal(roundedString));
            }
    
            /// <summary>
            /// Standard round to minimal coin/amount.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Rounded value.</returns>
            private GetRoundedValueServiceResponse GetRoundedValue(GetRoundedValueServiceRequest request)
            {
                string currencyCode = request.CurrencyCode;
                decimal currencyUnit = 0.0m;
    
                if (string.IsNullOrWhiteSpace(currencyCode))
                {
                    var channelConfiguration = request.RequestContext.GetChannelConfiguration();
    
                    currencyCode = channelConfiguration.Currency;
                }
    
                RoundingUnitAndMethod unitAndMethod = GetRoundingUnitAndMethod(request.RequestContext, currencyCode, request.UseSalesRounding);
                if (request.NumberOfDecimals != 0)
                {
                    currencyUnit = 1.0M / (decimal)Math.Pow(10, request.NumberOfDecimals);
                }
                else
                {
                    currencyUnit = unitAndMethod.RoundingUnit;
                }
    
                decimal value = Rounding.RoundToUnit(request.Value, currencyUnit, unitAndMethod.RoundingMethod);
                return new GetRoundedValueServiceResponse(value);
            }
    
            /// <summary>
            /// Round for channel payment method.
            /// </summary>
            /// <param name="request">Service request.</param>
            /// <returns>Rounded value.</returns>
            private GetRoundedValueServiceResponse GetPaymentRoundedValue(GetPaymentRoundedValueServiceRequest request)
            {
                GetChannelTenderTypesDataRequest dataServiceRequest = new GetChannelTenderTypesDataRequest(request.RequestContext.GetPrincipal().ChannelId, QueryResultSettings.AllRecords);
                EntityDataServiceResponse<TenderType> response = request.RequestContext.Execute<EntityDataServiceResponse<TenderType>>(dataServiceRequest);
    
                TenderType tenderType = response.PagedEntityCollection.Results.SingleOrDefault(channelTenderType => string.Equals(channelTenderType.TenderTypeId, request.TenderTypeId, StringComparison.OrdinalIgnoreCase));
    
                RoundingMethod roundingMethod = tenderType.RoundingMethod;
                if (roundingMethod == RoundingMethod.None)
                {
                    return new GetRoundedValueServiceResponse(request.Value);
                }
    
                decimal currencyUnit = tenderType.RoundOff;
                if (currencyUnit == decimal.Zero)
                {
                    currencyUnit = Rounding.DefaultRoundingValue;
                }
    
                decimal roundedValue;
    
                if (request.IsChange)
                {
                    // For change rounding up/down should be applied in opposite direction.
                    if (roundingMethod == RoundingMethod.Down)
                    {
                        roundingMethod = RoundingMethod.Up;
                    }
                    else if (roundingMethod == RoundingMethod.Up)
                    {
                        roundingMethod = RoundingMethod.Down;
                    }
                }
    
                // Using absolute value so payment and refund is rounded same way when rounding up or down.
                decimal absoluteAmount = Math.Abs(request.Value);
                roundedValue = Rounding.RoundToUnit(absoluteAmount, currencyUnit, roundingMethod);
                if (request.Value < 0)
                {
                    // Revert sign back to original.
                    roundedValue = decimal.Negate(roundedValue);
                }
    
                return new GetRoundedValueServiceResponse(roundedValue);
            }
    
            private class RoundingUnitAndMethod
            {
                internal RoundingUnitAndMethod(decimal roundingUnit, RoundingMethod roundingMethod)
                {
                    this.RoundingUnit = roundingUnit;
                    this.RoundingMethod = roundingMethod;
                }
    
                internal decimal RoundingUnit { get; private set; }
    
                internal RoundingMethod RoundingMethod { get; private set; }
            }
        }
    }
}
