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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
    
        /// <summary>
        /// Channel real time service.
        /// </summary>
        public class ChannelManagementTransactionService : IRequestHandler
        {
            private PaymentL2CacheDataStoreAccessor cacheAccessor;
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(UpdateChannelPublishingStatusRealtimeRequest),
                        typeof(GetTerminalMerchantPaymentProviderDataRealtimeRequest),
                        typeof(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest)
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
    
                this.InitailizeCache(request.RequestContext);
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(UpdateChannelPublishingStatusRealtimeRequest))
                {
                    response = this.UpdateChannelPublishingStatus((UpdateChannelPublishingStatusRealtimeRequest)request);
                }
                else if (requestType == typeof(GetTerminalMerchantPaymentProviderDataRealtimeRequest))
                {
                    response = this.GetTerminalMerchantPaymentProviderData((GetTerminalMerchantPaymentProviderDataRealtimeRequest)request);
                }
                else if (requestType == typeof(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest))
                {
                    response = this.GetChannelMerchantPaymentProviderData((GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Initialize the cache and instantiate the cache accessor.
            /// </summary>
            /// <param name="context">RequestContext instance <see cref="RequestContext"/></param>
            private void InitailizeCache(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                this.cacheAccessor = new PaymentL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
    
            /// <summary>
            /// Updates the publishing status and message for the given channel in AX.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The null response.</returns>
            private NullResponse UpdateChannelPublishingStatus(UpdateChannelPublishingStatusRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                transactionClient.UpdateChannelPublishingStatus(request.ChannelId, request.PublishStatus, request.PublishStatusMessage);
    
                return new NullResponse();
            }
    
            /// <summary>
            /// Get terminal merchant payment provider data.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private GetTerminalMerchantPaymentProviderDataRealtimeResponse GetTerminalMerchantPaymentProviderData(GetTerminalMerchantPaymentProviderDataRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                string paymentMerchantInformation = string.Empty;
    
                if (!this.cacheAccessor.GetMerchantPaymentProviderDataForTerminal(request.HardwareProfileId, out paymentMerchantInformation))
                {
                    paymentMerchantInformation = transactionClient.GetMerchantPaymentProviderDataForTerminal(request.HardwareProfileId);
    
                    this.cacheAccessor.CacheMerchantPaymentProviderDataForTerminal(request.HardwareProfileId, paymentMerchantInformation);
                }
    
                var paymentInformation = new PaymentMerchantInformation(paymentMerchantInformation);
                var response = new GetTerminalMerchantPaymentProviderDataRealtimeResponse(paymentInformation);
    
                return response;
            }
    
            /// <summary>
            /// Get channel merchant payment provider data.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private GetOnlineChannelMerchantPaymentProviderDataRealtimeResponse GetChannelMerchantPaymentProviderData(GetOnlineChannelMerchantPaymentProviderDataRealtimeRequest request)
            {
                var transactionClient = new TransactionService.TransactionServiceClient(request.RequestContext);
                var merchantData = transactionClient.GetMerchantPaymentProviderDataForOnlineStore(request.ChannelId);
                var response = new GetOnlineChannelMerchantPaymentProviderDataRealtimeResponse(merchantData);
                return response;
            }
        }
    }
}
