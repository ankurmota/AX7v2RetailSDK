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
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Implementation for channel service.
        /// </summary>
        public class ChannelService : IRequestHandler
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
                        typeof(UpdateChannelPublishStatusServiceRequest),
                        typeof(GetChannelIdServiceRequest),
                    };
                }
            }
    
            /// <summary>
            /// Entry point to Channel service.
            /// </summary>
            /// <param name="request">The service request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestType = request.GetType();
                Response response;
                if (requestType == typeof(UpdateChannelPublishStatusServiceRequest))
                {
                    response = UpdateChannelPublishStatus((UpdateChannelPublishStatusServiceRequest)request);
                }
                else if (requestType == typeof(GetChannelIdServiceRequest))
                {
                    response = GetChannelId((GetChannelIdServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Updates the channel publish status.
            /// </summary>
            /// <param name="request">The service request.</param>
            /// <returns>The service response.</returns>
            private static NullResponse UpdateChannelPublishStatus(UpdateChannelPublishStatusServiceRequest request)
            {
                var getOnlineChannelByIdDataRequest = new GetOnlineChannelByIdDataRequest(request.ChannelId, new ColumnSet());
                OnlineChannel channel = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<OnlineChannel>>(getOnlineChannelByIdDataRequest, request.RequestContext).Entity;
    
                var updateOnlineChannelPublishStatusDataRequest = new UpdateOnlineChannelPublishStatusDataRequest(request.ChannelId, request.PublishStatus, request.PublishStatusMessage);
                request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<bool>>(updateOnlineChannelPublishStatusDataRequest, request.RequestContext);
    
                try
                {
                    var updateChannelPublishingStatusRequest = new UpdateChannelPublishingStatusRealtimeRequest(request.ChannelId, request.PublishStatus, request.PublishStatusMessage);
                    request.RequestContext.Execute<NullResponse>(updateChannelPublishingStatusRequest);
                }
                catch (CommerceException)
                {
                    // Revert the link status if it fails to update AX via transaction service.
                    updateOnlineChannelPublishStatusDataRequest = new UpdateOnlineChannelPublishStatusDataRequest(request.ChannelId, channel.PublishStatus, channel.PublishStatusMessage);
                    request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<bool>>(updateOnlineChannelPublishStatusDataRequest, request.RequestContext);
                    throw;
                }
    
                return new NullResponse();
            }
    
            private static GetChannelIdServiceResponse GetChannelId(GetChannelIdServiceRequest request)
            {
                long channelId = request.IsDefault ? GetDefaultChannelId(request.RequestContext) : GetCurrentChannelId(request);
                return new GetChannelIdServiceResponse(channelId);
            }
    
            /// <summary>
            /// Gets the current channel identifier.
            /// </summary>
            /// <param name="request">Request object.</param>
            /// <returns>The channel identifier.</returns>
            private static long GetCurrentChannelId(Request request)
            {
                RequestContext context = request.RequestContext;
    
                // If we have a valid channel ID set on the Principal, we should use it.
                if (context.GetPrincipal() != null && context.GetPrincipal().ChannelId != 0)
                {
                    return context.GetPrincipal().ChannelId;
                }
    
                // Try to get the default channel identifier based on the default operating unit number in the commerce runtime configuration file.
                long defaultChannelId;
                if (TryGetDefaultChannelId(context, out defaultChannelId))
                {
                    return defaultChannelId;
                }
    
                // Gets the fist published channel if request does not need to get channel identifier from the Principal.
                if (request != null && !request.NeedChannelIdFromPrincipal)
                {
                    return GetFirstPublishedChannelId(context);
                }
    
                string errorMessage = string.Format(
                    "Can't find proper channel identifier for this request. Request type: {0}, NeedChannelIdFromPrincipal: {1}, Principal.ChannelId : {2}.",
                    request == null ? "NULL" : request.GetType().ToString(),
                    request != null && request.NeedChannelIdFromPrincipal,
                    context.GetPrincipal() == null ? default(long) : context.GetPrincipal().ChannelId);
    
                throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ConfigurationSettingNotFound, errorMessage);
            }
    
            /// <summary>
            /// Gets the default channel identifier based on the default operating unit number specified in the commerce runtime configuration file.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <returns>The default channel identifier.</returns>
            /// <exception cref="ConfigurationException">No default channel identifier was found.</exception>
            private static long GetDefaultChannelId(RequestContext context)
            {
                long defaultChannelId;
    
                if (!TryGetDefaultChannelId(context, out defaultChannelId))
                {
                    string message = string.Format("The default channel identifier could not be found. Please ensure that a default operating unit number has been specified as part of the <commerceRuntime> configuration section.");
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration, ExceptionSeverity.Warning, message);
                }
    
                return defaultChannelId;
            }
    
            /// <summary>
            /// Tries to get default channel identifier.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <param name="defaultChannelId">The default channel id.</param>
            /// <returns>True if the default channel identifier could be fetched, else false.</returns>
            private static bool TryGetDefaultChannelId(RequestContext context, out long defaultChannelId)
            {
                if (string.IsNullOrWhiteSpace(GetDefaultOperatingUnitNumber(context)))
                {
                    defaultChannelId = 0;
                    return false;
                }
    
                defaultChannelId = GetChannelIdByOperatingUnitNumber(context, GetDefaultOperatingUnitNumber(context));
                return true;
            }
    
            /// <summary>
            /// Gets the channel identifier of the first published channel in local database.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <returns>The channel identifier.</returns>
            /// <remarks>
            /// This is designed for Retail Server scenarios of Device Activation and Restricted Logon.
            /// In order to make the system become more robust, ONLY published channel is allowed to be used.
            /// </remarks>
            private static long GetFirstPublishedChannelId(RequestContext context)
            {
                GetStorageLookupDataRequest request = new GetStorageLookupDataRequest(context.Runtime.Configuration.ConnectionString);
                IEnumerable<StorageLookup> lookups = context.Execute<GetStorageLookupDataResponse>(request).LookupValues.Values;
    
                IEnumerable<StorageLookup> publishedChannels = lookups.Where(lookup => lookup.IsPublished).ToList();
    
                if (!publishedChannels.Any())
                {
                    string errorMessage = "The published channel can not be found in local database. Please make sure at least 1 retail channel is published to this DB through AX.";
                    throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ConfigurationSettingNotFound, errorMessage);
                }
    
                // Favors channel(s) published to local, in order to avoid database connection cross different machines.
                var firstPublishedChannelOnLocal =
                    publishedChannels.FirstOrDefault(lookup => lookup.IsLocal);
    
                if (firstPublishedChannelOnLocal != null)
                {
                    return firstPublishedChannelOnLocal.ChannelId;
                }
    
                return publishedChannels.FirstOrDefault().ChannelId;
            }
    
            /// <summary>
            /// Gets the configured default operating unit number.
            /// </summary>
            /// <param name="context">Request context.</param>
            /// <returns>Operating unit number.</returns>
            private static string GetDefaultOperatingUnitNumber(RequestContext context)
            {
                ICommerceRuntimeConfiguration cachedConfiguration = context.Runtime.Configuration;
                if (cachedConfiguration != null && cachedConfiguration.Storage != null)
                {
                    return cachedConfiguration.Storage.DefaultOperatingUnitNumber;
                }
    
                return null;
            }
    
            /// <summary>
            /// Gets the channel identifier by operating unit number.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="operatingUnitNumber">The operating unit number.</param>
            /// <returns>The channel identifier.</returns>
            private static long GetChannelIdByOperatingUnitNumber(RequestContext context, string operatingUnitNumber)
            {
                GetStorageLookupDataRequest request = new GetStorageLookupDataRequest(context.Runtime.Configuration.ConnectionString);
                IEnumerable<StorageLookup> lookups = context.Execute<GetStorageLookupDataResponse>(request).LookupValues.Values;
    
                foreach (StorageLookup lookup in lookups)
                {
                    if (string.Equals(operatingUnitNumber, lookup.OperatingUnitNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        return lookup.ChannelId;
                    }
                }
    
                string message = string.Format("The specified default operating unit ({0}) was not found.", operatingUnitNumber);
                throw new ConfigurationException(ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_ConfigurationSettingNotFound, message);
            }
        }
    }
}
