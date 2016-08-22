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
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Encapsulates methods to format number, currency and date.
        /// </summary>
        public class FormattingService : IRequestHandler
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
                    typeof(GetFormattedCurrencyServiceRequest),
                    typeof(GetFormattedNumberServiceRequest),
                    typeof(GetFormattedDateServiceRequest),
                    typeof(GetFormattedTimeServiceRequest)
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
                if (requestType == typeof(GetFormattedCurrencyServiceRequest))
                {
                    response = this.FormatCurrency((GetFormattedCurrencyServiceRequest)request);
                }
                else if (requestType == typeof(GetFormattedNumberServiceRequest))
                {
                    response = this.FormatNumber((GetFormattedNumberServiceRequest)request);
                }
                else if (requestType == typeof(GetFormattedDateServiceRequest))
                {
                    response = this.FormatDate((GetFormattedDateServiceRequest)request);
                }
                else if (requestType == typeof(GetFormattedTimeServiceRequest))
                {
                    response = this.FormatTime((GetFormattedTimeServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Formats the currency.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>A GetFormattedContentServiceResponse object containing the formatted value.</returns>
            /// <remarks>
            /// This method formats the currency according to the culture info which is determined by the channel locale or company language.
            /// The currency symbol contained in the request has nothing to do with the format, but it will be put into the formatted value.
            /// e.g.
            /// [$, 123.46] (en-US) -> $123.46.
            /// [€, 123.46] (fr-FR) -> 123,46 €.
            /// [$, 123.46] (fr-FR) -> 123,46 $.
            /// </remarks>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1628:DocumentationTextMustBeginWithACapitalLetter", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1631:DocumentationMustMeetCharacterPercentage", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "These are examples for the users.")]
            private GetFormattedContentServiceResponse FormatCurrency(GetFormattedCurrencyServiceRequest request)
            {
                CultureInfo cultureInfo = this.GetCultureInfo(request.RequestContext);
                string originalCurrencySymbol = request.CurrencySymbol;
                string cultureCurrencySymbol = cultureInfo.NumberFormat.CurrencySymbol;

                // e.g. 123.45 -> $123.45 or 123,45€
                string result = request.Amount.ToString("c", cultureInfo);

                // The result is now having culture currency symbol, but it should stick to the original one.
                result = result.Replace(cultureCurrencySymbol, originalCurrencySymbol);
                return new GetFormattedContentServiceResponse(result);
            }

            /// <summary>
            /// Formats the number according to the channel locale or company language.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>Formatted value.</returns>
            /// <remarks>
            /// e.g.
            /// 1234.567 (en-US) -> 1234.57
            /// 1234.567 (ru-RU) -> 1234,57 .
            /// </remarks>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1628:DocumentationTextMustBeginWithACapitalLetter", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1631:DocumentationMustMeetCharacterPercentage", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "These are examples for the users.")]
            private GetFormattedContentServiceResponse FormatNumber(GetFormattedNumberServiceRequest request)
            {
                CultureInfo cultureInfo = this.GetCultureInfo(request.RequestContext);
                decimal number = request.Number;

                // G29 means the general number format.
                string result = number.ToString("G29", cultureInfo);
                return new GetFormattedContentServiceResponse(result);
            }

            /// <summary>
            /// Formats the date according to the channel locale or company language.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>Formatted value.</returns>
            /// <remarks>
            /// This method uses short date pattern.
            /// 2009-06-15T13:45:30 -> 6/15/2009 (en-US)
            /// 2009-06-15T13:45:30 -> 15/06/2009 (fr-FR)
            /// 2009-06-15T13:45:30 -> 2009/06/15 (ja-JP).
            /// </remarks>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1628:DocumentationTextMustBeginWithACapitalLetter", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1631:DocumentationMustMeetCharacterPercentage", Justification = "These are examples for the users.")]
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "These are examples for the users.")]
            private GetFormattedContentServiceResponse FormatDate(GetFormattedDateServiceRequest request)
            {
                CultureInfo cultureInfo = this.GetCultureInfo(request.RequestContext);
                DateTimeOffset value = request.DateTimeOffsetValue;

                string result = request.RequestContext.ConvertDateTimeToChannelDate(value).ToString("d", cultureInfo);
                return new GetFormattedContentServiceResponse(result);
            }

            /// <summary>
            /// Formats the time according to the channel locale or company language.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>Formatted value.</returns>
            private GetFormattedContentServiceResponse FormatTime(GetFormattedTimeServiceRequest request)
            {
                CultureInfo cultureInfo = this.GetCultureInfo(request.RequestContext);
                DateTimeOffset value = request.DateTimeOffsetValue;

                string result;
                switch (request.FormatType)
                {
                    case TimeFormattingType.Hour12:
                        result = request.RequestContext.ConvertDateTimeToChannelDate(value).ToString("hh:mm tt", cultureInfo);
                        break;
                    case TimeFormattingType.Hour24:
                        result = request.RequestContext.ConvertDateTimeToChannelDate(value).ToString("HH:mm", cultureInfo);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The formatting service does not support this time format: {0}", request.FormatType.ToString()));
                }

                return new GetFormattedContentServiceResponse(result);
            }

            /// <summary>
            /// Gets the culture information which is used to format the content.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>A <see cref="CultureInfo"/> object generated according to the channel locale, company language or OS culture.</returns>
            /// <remarks>
            /// Channel language has the highest priority to determine which culture info to return,
            /// then company language.
            /// </remarks>
            private CultureInfo GetCultureInfo(RequestContext context)
            {
                ChannelConfiguration channelConfig = context.GetChannelConfiguration();
                string cultureName = string.IsNullOrEmpty(channelConfig.DefaultLanguageId) ? channelConfig.CompanyLanguageId : channelConfig.DefaultLanguageId;

                if (!string.IsNullOrEmpty(cultureName))
                {
                    try
                    {
                        return new CultureInfo(cultureName);
                    }
                    catch (CultureNotFoundException)
                    {
                        return CultureInfo.InvariantCulture;
                    }
                }
                else
                {
                    return CultureInfo.InvariantCulture;
                }
            }
        }
    }
}