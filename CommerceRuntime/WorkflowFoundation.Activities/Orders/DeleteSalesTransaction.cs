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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Activity that deletes sales transactions.
        /// </summary>
        public sealed class DeleteSalesTransaction : CodeActivity
        {
            /// <summary>
            /// Gets or sets request context argument.
            /// </summary>
            public InArgument<RequestContext> RequestContext { get; set; }
    
            /// <summary>
            /// Gets or sets transaction id argument.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "InArgument generic type is WorkflowFoundation requirement.")]
            public InArgument<IEnumerable<string>> TransactionIds { get; set; }
    
            /// <summary>
            /// Performs the execution of the activity.
            /// </summary>
            /// <param name="context">The execution context under which the activity executes.</param>
            protected override void Execute(CodeActivityContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                RequestContext requestContext = context.GetValue<RequestContext>(this.RequestContext);
                IEnumerable<string> transactionIds = context.GetValue<IEnumerable<string>>(this.TransactionIds);
    
                if (!transactionIds.Any())
                {
                    return;
                }
    
                DeleteCartDataRequest deleteCartDataRequest = new DeleteCartDataRequest(transactionIds);
                requestContext.Runtime.Execute<NullResponse>(deleteCartDataRequest, requestContext);
            }
        }
    }
}
