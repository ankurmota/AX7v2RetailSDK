/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to retrieve supported channel currency amounts.
        /// </summary>
        public class GetChannelCurrencyRequestHandler : SingleRequestHandler<GetChannelCurrencyAmountRequest, GetChannelCurrencyAmountResponse>
        {
            /// <summary>
            /// Executes the workflow associated with retrieving list of supported channel currencies.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetChannelCurrencyAmountResponse Process(GetChannelCurrencyAmountRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.CurrenciesToConvert, "request.CurrencyToConvert");
                
                if (!request.CurrenciesToConvert.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CurrencyNotFound);
                }

                GetChannelCurrencyAmountResponse response;

                if (!request.IsTotalToBeCalculated)
                {
                    CurrencyRequest currencyToConvert = request.CurrenciesToConvert.Single();
    
                    // Set default store currency if the currency Iso code is not set.
                    if (string.IsNullOrWhiteSpace(currencyToConvert.CurrencyCode))
                    {
                        var channelConfiguration = this.Context.GetChannelConfiguration();
    
                        currencyToConvert.CurrencyCode = channelConfiguration.Currency;
                    }
    
                    var serviceRequest = new GetChannelCurrencyServiceRequest(currencyToConvert.CurrencyCode, currencyToConvert.AmountToConvert, request.QueryResultSettings);
    
                    var serviceResponse = this.Context.Execute<GetChannelCurrencyServiceResponse>(serviceRequest);
    
                    response = new GetChannelCurrencyAmountResponse(serviceResponse.ChannelCurrencies);
                }
                else
                {
                    var serviceRequest = new CalculateTotalAmountServiceRequest(request.CurrenciesToConvert);
    
                    var serviceResponse = this.Context.Execute<CalculateTotalAmountServiceResponse>(serviceRequest);
    
                    response = new GetChannelCurrencyAmountResponse(new ReadOnlyCollection<CurrencyAmount>(new[] { serviceResponse.TotalCurrencyAmount }).AsPagedResult());
                }
    
                return response;
            }
        }
    }
}
