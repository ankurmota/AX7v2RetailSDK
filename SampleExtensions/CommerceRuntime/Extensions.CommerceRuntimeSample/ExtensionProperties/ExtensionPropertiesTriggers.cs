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
        using System.Diagnostics;
        using Commerce.Runtime.Sample.ExtensionProperties.Messages;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Class that implements a simple pre and post trigger for the ExtensionPropertiesRequest request type.
        /// </summary>
        public class ExtensionPropertiesTriggers : IRequestTrigger
        {
            /// <summary>
            /// Gets the supported requests for this trigger.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[] { typeof(ExtensionPropertiesRequest) };
                }
            }

            /// <summary>
            /// Post trigger code.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="response">The response.</param>
            public void OnExecuted(Request request, Response response)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                if (response == null)
                {
                    throw new ArgumentNullException("response");
                }

                Debug.WriteLine("Post trigger called.");
                request.SetProperty("POST_TRIGGER_CALLED", true);
                response.SetProperty("POST_TRIGGER_CALLED", true);
            }

            /// <summary>
            /// Pre trigger code.
            /// </summary>
            /// <param name="request">The request.</param>
            public void OnExecuting(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Debug.WriteLine("Pre trigger called.");
                request.SetProperty("PRE_TRIGGER_CALLED", true);
            }
        }
    }
}
