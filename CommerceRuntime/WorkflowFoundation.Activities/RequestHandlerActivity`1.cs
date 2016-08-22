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
    namespace Commerce.Runtime.WorkflowFoundation.Activities
    {
        using System;
        using System.Activities;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Generic activity adapter for CRT request handlers.
        /// </summary>
        /// <typeparam name="TResponse">Type of workflow request handler.</typeparam>
        public sealed class RequestHandlerActivity<TResponse> : CodeActivity<TResponse> where TResponse : Response
        {
            /// <summary>
            /// Gets or sets request object.
            /// </summary>
            public InArgument<Request> Request { get; set; }
    
            /// <summary>
            /// Gets or sets request context.
            /// </summary>
            public InOutArgument<RequestContext> RequestContext { get; set; }
    
            /// <summary>
            /// Performs the execution of the activity.
            /// </summary>
            /// <param name="context">The execution context under which the activity executes.</param>
            /// <returns>The result of the activity?s execution which is instance of <see cref="Response"/>.</returns>
            protected override TResponse Execute(CodeActivityContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                var requestContext = context.GetValue(this.RequestContext);
                var request = context.GetValue(this.Request);
                return requestContext.Execute<TResponse>(request);
            }
        }
    }
}
