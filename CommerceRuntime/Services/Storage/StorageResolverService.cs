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
        using Microsoft.Dynamics.Commerce.Runtime.Data.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Service that does storage resolution.
        /// </summary>
        public class StorageResolverService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new Type[]
                    {
                        typeof(GetConnectionStringRequest)
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
    
                if (requestType == typeof(GetConnectionStringRequest))
                {
                    response = this.GetConnectionString((GetConnectionStringRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            private GetConnectionStringResponse GetConnectionString(GetConnectionStringRequest request)
            {
                // Get current channel identifier.
                GetChannelIdServiceRequest channelIdRequest = new GetChannelIdServiceRequest();
                ICommerceRuntime runtime = request.RequestContext.Runtime;
                long channelId = runtime.Execute<GetChannelIdServiceResponse>(channelIdRequest, request.RequestContext, skipRequestTriggers: true).ChannelId;
    
                // If connection string is overriden simply return connection string from configuration and skip lookup.
                string connectionStringFromConfig = runtime.Configuration.ConnectionString;
                if (runtime.Configuration.IsConnectionStringOverridden)
                {
                    if (string.IsNullOrWhiteSpace(connectionStringFromConfig))
                    {
                        throw new ConfigurationException(
                            ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration,
                            string.Format("No connection string found for channel ({0}).", channelId));
                    }
    
                    return new GetConnectionStringResponse(connectionStringFromConfig);
                }
    
                // For current channel identifier query storage lookup data.
                GetStorageLookupDataRequest storageLookupRequest = new GetStorageLookupDataRequest(connectionStringFromConfig);
                GetStorageLookupDataResponse storageLookupResponse = request.RequestContext.Runtime.Execute<GetStorageLookupDataResponse>(storageLookupRequest, request.RequestContext, skipRequestTriggers: true);
                if (!storageLookupResponse.LookupValues.ContainsKey(channelId))
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration,
                        ExceptionSeverity.Warning,
                        string.Format("The specified channel ({0}) was not found.", channelId));
                }
    
                // Using storage identifier from storage lookup view lookup connection string in configuration.
                StorageLookup lookup = storageLookupResponse.LookupValues[channelId];
                string connectionString;
                if (!runtime.Configuration.StorageLookupConnectionStrings.TryGetValue(lookup.StorageId, out connectionString))
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannelConfiguration,
                        ExceptionSeverity.Warning,
                        string.Format("The connection string is not found. StorageId: {0}. ChannelId: {1}.", lookup.StorageId, channelId));
                }
    
                return new GetConnectionStringResponse(connectionString);
            }
        }
    }
}
