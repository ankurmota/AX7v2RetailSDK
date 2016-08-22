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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Currency data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class CurrencySqlServerDataService : IRequestHandler
        {
            private const string GetExchangeRateProcedureName = "GETACTIVEEXCHANGERATE";
    
            private const string FromCurrencyVariable = "@nvc_FromCurrency";
            private const string ToCurrencyVariable = "@nvc_ToCurrency";
            private const string ActiveDateVariable = "@dt_ActiveDate";
            private const string ChannelIdVariable = "@bi_channelId";
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(GetExchangeRatesDataRequest) };
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
    
                if (requestType == typeof(GetExchangeRatesDataRequest))
                {
                    response = this.GetExchangeRates((GetExchangeRatesDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the exchange rate active on given date between these currencies on this channel.
            /// </summary>
            /// <param name="request">The data service request with the to and from currencies and active date.</param>
            /// <returns>The data service response with up to two exchange rates, which are forward and backward rates between the currencies.</returns>
            public EntityDataServiceResponse<ExchangeRate> GetExchangeRates(GetExchangeRatesDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.NullOrWhiteSpace(request.FromCurrency, "fromCurrency");
                ThrowIf.NullOrWhiteSpace(request.ToCurrency, "toCurrency");
    
                ReadOnlyCollection<ExchangeRate> result;
    
                CurrencyDataManager manager = new CurrencyDataManager(request.RequestContext);
    
                CurrencyL2CacheDataStoreAccessor level2CacheDataAccessor = (CurrencyL2CacheDataStoreAccessor)manager.DataStoreManagerInstance.RegisteredAccessors[DataStoreType.L2Cache];
    
                bool updateL2Cache = DataStoreManager.DataStores[DataStoreType.L2Cache].Policy.MustUpdateOnMiss;
    
                result = level2CacheDataAccessor.GetExchangeRates(request.FromCurrency, request.ToCurrency, request.ActiveDate);
                updateL2Cache &= result == null;
    
                if (result == null)
                {
                    ParameterSet parameters = new ParameterSet();
    
                    parameters[FromCurrencyVariable] = request.FromCurrency;
                    parameters[ToCurrencyVariable] = request.ToCurrency;
                    parameters[ActiveDateVariable] = request.ActiveDate;
    
                    using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(request))
                    {
                        result = sqlServerDatabaseContext.ExecuteNonPagedStoredProcedure<ExchangeRate>(GetExchangeRateProcedureName, parameters);
                    }
    
                    updateL2Cache &= result != null;
                }
    
                if (updateL2Cache)
                {
                    level2CacheDataAccessor.PutExchangeRates(request.FromCurrency, request.ToCurrency, request.ActiveDate, result);
                }
    
                return new EntityDataServiceResponse<ExchangeRate>(result.AsPagedResult());
            }
        }
    }
}
