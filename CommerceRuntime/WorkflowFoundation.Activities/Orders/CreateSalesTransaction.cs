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
        using Commerce.Runtime.Workflow;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Activity that creates sales transaction.
        /// </summary>
        public sealed class CreateSalesTransaction : CodeActivity<SalesTransaction>
        {
            /// <summary>
            /// Gets or sets request context argument.
            /// </summary>
            public InArgument<RequestContext> RequestContext { get; set; }
    
            /// <summary>
            /// Gets or sets transaction id argument.
            /// </summary>
            public InArgument<string> TransactionId { get; set; }
    
            /// <summary>
            /// Gets or sets customer id argument.
            /// </summary>
            public InArgument<string> CustomerId { get; set; }
    
            /// <summary>
            /// Performs the execution of the activity.
            /// </summary>
            /// <param name="context">The execution context under which the activity executes.</param>
            /// <returns>Created sales transaction.</returns>
            protected override SalesTransaction Execute(CodeActivityContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException("context");
                }
    
                RequestContext requestContext = context.GetValue<RequestContext>(this.RequestContext);
                string transactionId = context.GetValue<string>(this.TransactionId);
                string customerId = context.GetValue<string>(this.CustomerId);
                SalesTransaction transaction = CartWorkflowHelper.CreateSalesTransaction(requestContext, transactionId, customerId);
                return transaction;
            }
        }
    }
}
