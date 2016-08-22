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
    namespace Commerce.Runtime.Sample.ExtensionProperties
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Globalization;
        using ExtensionProperties.Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Sample service to demonstrate extension properties.
        /// </summary>
        public class ExtensionPropertiesService : IRequestHandler
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
                        typeof(ExtensionPropertiesRequest),
                    };
                }
            }

            /// <summary>
            /// Entry point to ExtensionPropertiesService service.
            /// </summary>
            /// <param name="request">The request to execute.</param>
            /// <returns>Result of executing request, or null object for void operations.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type reqType = request.GetType();
                if (reqType == typeof(ExtensionPropertiesRequest))
                {
                    var entity = new DataModel.ExtensionPropertyEntity();
                    bool? requestExtended = (bool?)request.GetProperty("EXTENSION_PROPERTY_ADDED");
                    if (requestExtended.HasValue && requestExtended == true)
                    {
                        // only if request was extended, we will also extend the entity. Therefore we can check on the caller if all extensions worked
                        entity.SetProperty("EXTENSION_PROPERTY_ADDED", true);
                    }

                    var response = new ExtensionPropertiesResponse(entity);
                    response.SetProperty("EXTENSION_PROPERTY_ADDED", true);

                    // notifying, to whoever is listening...
                    request.RequestContext.Runtime.Notify(request.RequestContext, new CustomNotification("data"));

                    return response;
                }
                else
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", reqType);
                    RetailLogger.Log.ExtendedErrorEvent(message);
                    throw new NotSupportedException(message);
                }
            }
        }
    }
}
