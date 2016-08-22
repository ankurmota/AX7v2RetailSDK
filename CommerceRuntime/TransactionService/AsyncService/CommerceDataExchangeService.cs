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
        using System.IdentityModel.Tokens;
        using System.ServiceModel;
        using Commerce.Runtime.TransactionService;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Commerce data exchange service.
        /// </summary>
        public class CommerceDataExchangeService : IRequestHandler
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
                        typeof(GetDownloadIntervalRealtimeRequest),
                        typeof(GetDownloadLinkRealtimeRequest),
                        typeof(GetDownloadSessionsRealtimeRequest),
                        typeof(GetTerminalDataStoreNameRealtimeRequest),
                        typeof(GetUploadIntervalRealtimeRequest),
                        typeof(GetUploadJobDefinitionsRealtimeRequest),
                        typeof(UpdateDownloadSessionStatusRealtimeRequest),
                        typeof(ValidateDataStoreRealtimeRequest)
                    };
                }
            }
    
            /// <summary>
            /// Processes the requests.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                Response response = null;
                string methodName = string.Empty;
                Exception exception = null;
                GetDownloadSessionsRealtimeRequest getDownloadSessionRealtimeRequest;
                GetDownloadLinkRealtimeRequest getDownloadLinkRealtimeRequest;
                GetDownloadIntervalRealtimeRequest getDownloadIntervalRealtimeRequest;
                GetTerminalDataStoreNameRealtimeRequest getTerminalDataStoreNameRealtimeRequest;
                GetUploadIntervalRealtimeRequest getUploadIntervalRealtimeRequest;
                GetUploadJobDefinitionsRealtimeRequest getUploadJobDefinitionsRealtimeRequest;
                UpdateDownloadSessionStatusRealtimeRequest updateDownloadSessionStatusRealtimeRequest;
                ValidateDataStoreRealtimeRequest validateDataStoreRealtimeRequest;

                Guid correlationId = Guid.NewGuid();
                Guid relatedActivityId = Guid.NewGuid();

                try
                {
                    if ((getDownloadSessionRealtimeRequest = request as GetDownloadSessionsRealtimeRequest) != null)
                    {
                        methodName = "GetDownloadSessions";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, getDownloadSessionRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.GetDownloadSessions(getDownloadSessionRealtimeRequest, relatedActivityId);
                    }
                    else if ((getDownloadLinkRealtimeRequest = request as GetDownloadLinkRealtimeRequest) != null)
                    {
                        methodName = "GetDownloadLink";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, getDownloadLinkRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.GetDownloadLink(getDownloadLinkRealtimeRequest, relatedActivityId);
                    }
                    else if ((getDownloadIntervalRealtimeRequest = request as GetDownloadIntervalRealtimeRequest) != null)
                    {
                        methodName = "GetDownloadInterval";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, getDownloadIntervalRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.GetDownloadInterval(getDownloadIntervalRealtimeRequest, relatedActivityId);
                    }
                    else if ((getTerminalDataStoreNameRealtimeRequest = request as GetTerminalDataStoreNameRealtimeRequest) != null)
                    {
                        methodName = "GetTerminalDataStoreName";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, string.Empty, relatedActivityId);
                        response = CommerceDataExchangeService.GetTerminalDataStoreName(getTerminalDataStoreNameRealtimeRequest, relatedActivityId);
                    }
                    else if ((getUploadIntervalRealtimeRequest = request as GetUploadIntervalRealtimeRequest) != null)
                    {
                        methodName = "GetUploadInterval";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, getUploadIntervalRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.GetUploadInterval(getUploadIntervalRealtimeRequest, relatedActivityId);
                    }
                    else if ((updateDownloadSessionStatusRealtimeRequest = request as UpdateDownloadSessionStatusRealtimeRequest) != null)
                    {
                        methodName = "UpdateDownloadSessionStatus";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, string.Empty, relatedActivityId);
                        response = CommerceDataExchangeService.UpdateDownloadSessionStatus(updateDownloadSessionStatusRealtimeRequest, relatedActivityId);
                    }
                    else if ((validateDataStoreRealtimeRequest = request as ValidateDataStoreRealtimeRequest) != null)
                    {
                        methodName = "ValidateDataStoreService";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, validateDataStoreRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.ValidateDataStore(validateDataStoreRealtimeRequest, relatedActivityId);
                    }
                    else if ((getUploadJobDefinitionsRealtimeRequest = request as GetUploadJobDefinitionsRealtimeRequest) != null)
                    {
                        methodName = "GetUploadJobDefinitions";
                        RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallStarted(correlationId, methodName, getUploadJobDefinitionsRealtimeRequest.DataStoreName, relatedActivityId);
                        response = CommerceDataExchangeService.GetUploadJobDefinitions(getUploadJobDefinitionsRealtimeRequest, relatedActivityId);
                    }
                    else
                    {
                        RetailLogger.Log.CrtTransactionServiceClientCommerceDataExchangeServiceExecuteRequestInformation("Unknown Request");
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request));
                    }
                }
                catch (Microsoft.Dynamics.Commerce.Runtime.CommunicationException ex)
                {
                    exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure);
                }
                catch (SecurityTokenException ex)
                {
                    exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure);
                }
                catch (TimeoutException ex)
                {
                    exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceTimeOut);
                }
                catch (Exception ex)
                {
                    exception = TransactionServiceClient.CreateCommunicationException(methodName, ex, CommunicationErrors.Microsoft_Dynamics_Commerce_Runtime_TransactionServiceException);
                }
    
                if (exception != null)
                {
                    RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallError(correlationId, methodName, relatedActivityId, exception.GetType().ToString(), exception);
                    throw exception;
                }
                else
                {
                    RetailLogger.Log.CrtTransactionServiceClientRtsAsyncServiceCallSuccessful(correlationId, methodName, relatedActivityId);
                }
    
                return response;
            }

            /// <summary>
            /// Gets download sessions.
            /// </summary>
            /// <param name="request">The request for getting download sessions.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetDownloadSessionsRealtimeResponse GetDownloadSessions(GetDownloadSessionsRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetDownloadSessionsRealtimeResponse(client.GetDownloadSessions(activityId, request.DataStoreName, request.QueryResultSettings));
            }

            /// <summary>
            /// Gets upload job Definitions.
            /// </summary>
            /// <param name="request">The request for getting upload job Definitions.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetUploadJobDefinitionsRealtimeResponse GetUploadJobDefinitions(GetUploadJobDefinitionsRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetUploadJobDefinitionsRealtimeResponse(client.GetUploadJobDefinitions(activityId, request.DataStoreName));
            }

            /// <summary>
            /// Gets the download link.
            /// </summary>
            /// <param name="request">The request for getting download session link.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetDownloadLinkRealtimeResponse GetDownloadLink(GetDownloadLinkRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }

                if (request.DownloadSessionId <= 0)
                {
                    throw new ArgumentException("DownloadSessionId should be greater than 0.");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetDownloadLinkRealtimeResponse(client.GetDownloadLink(activityId, request.DataStoreName, request.DownloadSessionId));
            }

            /// <summary>
            /// Gets data store name from terminal Id.
            /// </summary>
            /// <param name="request">The request for getting download interval.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetTerminalDataStoreNameRealtimeResponse GetTerminalDataStoreName(GetTerminalDataStoreNameRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    throw new ArgumentException("TerminalId must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetTerminalDataStoreNameRealtimeResponse(client.GetTerminalDataStoreName(activityId, request.TerminalId));
            }

            /// <summary>
            /// Gets download interval.
            /// </summary>
            /// <param name="request">The request for getting download interval.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetDownloadIntervalRealtimeResponse GetDownloadInterval(GetDownloadIntervalRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetDownloadIntervalRealtimeResponse(client.GetDownloadInterval(activityId, request.DataStoreName));
            }

            /// <summary>
            /// Gets upload interval.
            /// </summary>
            /// <param name="request">The request for getting upload interval.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static GetUploadIntervalRealtimeResponse GetUploadInterval(GetUploadIntervalRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new GetUploadIntervalRealtimeResponse(client.GetUploadInterval(activityId, request.DataStoreName));
            }

            /// <summary>
            /// Updates the download session status.
            /// </summary>
            /// <param name="request">The request for uploading the download session status.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static UpdateDownloadSessionStatusRealtimeResponse UpdateDownloadSessionStatus(UpdateDownloadSessionStatusRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (request.DownloadSession == null)
                {
                    throw new ArgumentException("DownloadSession is not set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                client.UpdateDownloadSessionStatus(activityId, request.DownloadSession);
                return new UpdateDownloadSessionStatusRealtimeResponse(true);
            }

            /// <summary>
            /// Validates data store.
            /// </summary>
            /// <param name="request">The request for uploading the download session status.</param>
            /// <param name="activityId">The activity identifier associated with the request.</param>
            /// <returns>The operation result.</returns>
            private static ValidateDataStoreRealtimeResponse ValidateDataStore(ValidateDataStoreRealtimeRequest request, Guid activityId)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                if (string.IsNullOrWhiteSpace(request.DataStoreName))
                {
                    throw new ArgumentException("DataStoreName must be set in request", "request");
                }
    
                if (string.IsNullOrWhiteSpace(request.UserName))
                {
                    throw new ArgumentException("UserName must be set in request", "request");
                }
    
                var client = new TransactionServiceClient(request.RequestContext);
                return new ValidateDataStoreRealtimeResponse(client.ValidateDataStore(activityId, request.DataStoreName, request.UserName));
            }
        }
    }
}
