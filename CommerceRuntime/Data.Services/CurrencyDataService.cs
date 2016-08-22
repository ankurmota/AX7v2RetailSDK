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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Currency data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class CurrencyDataService : IRequestHandler
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
                        typeof(GetChannelCurrenciesDataRequest),
                        typeof(GetCurrencyByCodeDataRequest)
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
    
                if (requestType == typeof(GetChannelCurrenciesDataRequest))
                {
                    response = this.GetChannelCurrencies((GetChannelCurrenciesDataRequest)request);
                }
                else if (requestType == typeof(GetCurrencyByCodeDataRequest))
                {
                    response = this.GetCurrencyByCode((GetCurrencyByCodeDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the data manager instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of <see cref="CurrencyDataManager"/></returns>
            private CurrencyDataManager GetDataManagerInstance(RequestContext context)
            {
                return new CurrencyDataManager(context);
            }
    
            private EntityDataServiceResponse<CurrencyAmount> GetChannelCurrencies(GetChannelCurrenciesDataRequest request)
            {
                CurrencyDataManager manager = this.GetDataManagerInstance(request.RequestContext);
    
                PagedResult<CurrencyAmount> currencies = manager.GetChannelCurrencies(request.QueryResultSettings);
    
                return new EntityDataServiceResponse<CurrencyAmount>(currencies);
            }
    
            private SingleEntityDataServiceResponse<Currency> GetCurrencyByCode(GetCurrencyByCodeDataRequest request)
            {
                CurrencyDataManager manager = this.GetDataManagerInstance(request.RequestContext);
    
                Currency currency = manager.GetCurrencyByCode(request.CurrencyCode, request.Columns);
    
                return new SingleEntityDataServiceResponse<Currency>(currency);
            }
        }
    }
}
