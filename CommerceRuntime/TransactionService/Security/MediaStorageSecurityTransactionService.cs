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
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Media storage security service.
        /// </summary>
        public class MediaStorageSecurityTransactionService : IRequestHandler
        {
            private MediaStorageSasKeyCacheAccessor cacheAccessor;
    
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(AssociateSasKeyRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request <see cref="Request"/>.</param>
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
                if (requestType == typeof(AssociateSasKeyRealtimeRequest))
                {
                    response = this.AssociateSasKey((AssociateSasKeyRealtimeRequest)request);
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
                this.cacheAccessor = new MediaStorageSasKeyCacheAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
    
            /// <summary>
            /// This routine integrate security with the images by obtaining the security information from the transaction service.
            /// </summary>
            /// <param name="request">The <see cref="AssociateSasKeyRealtimeRequest"/> class instance.</param>
            /// <returns><see cref="AssociateSasKeyRealtimeResponse"/> class instance.</returns>
            private AssociateSasKeyRealtimeResponse AssociateSasKey(AssociateSasKeyRealtimeRequest request)
            {
                Dictionary<string, RichMediaLocations> media = request.MediaDictionary;
                if (media == null || !media.Keys.Any())
                {
                    return new AssociateSasKeyRealtimeResponse(media);
                }
    
                string sasKey = string.Empty;
                if (!this.cacheAccessor.GetMediaStorageSasKey(out sasKey))
                {
                    sasKey = this.GetMediaStorageSasKey(request.RequestContext);
                    this.cacheAccessor.PutMediaStorageSasKey(sasKey);
                }
    
                if (string.IsNullOrEmpty(sasKey))
                {
                    RetailLogger.Log.GenericWarningEvent("No media sas key found.");
                }
    
                foreach (string key in media.Keys)
                {
                    if (media[key].Items != null)
                    {
                        foreach (RichMediaLocationsRichMediaLocation item in media[key].Items)
                        {
                            if (item.IsSelfHosted)
                            {
                                item.Url = item.Url + sasKey;
                            }
                        }
                    }
                }
    
                return new AssociateSasKeyRealtimeResponse(media);
            }
    
            /// <summary>
            /// Invokes the transaction service to get the media storage SAS key.
            /// </summary>
            /// <param name="context">Request context <see cref="RequestContext" />.</param>
            /// <returns>The media storage SAS key.</returns>
            private string GetMediaStorageSasKey(RequestContext context)
            {
                ThrowIf.Null(context, "context");
    
                var transactionService = new TransactionService.TransactionServiceClient(context);
                MediaStorageSasDetails sasKey = transactionService.GetMediaStorageSasKeyDetails();
                return sasKey.SasKey;
            }
        }
    }
}
