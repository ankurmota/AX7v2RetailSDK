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
        using Commerce.Runtime.Workflow;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Activity that validates cart request.
        /// </summary>
        public sealed class ValidateUpdateCartRequest : CodeActivity
        {
            /// <summary>
            /// Gets or sets request context argument.
            /// </summary>
            public InArgument<RequestContext> RequestContext { get; set; }
    
            /// <summary>
            /// Gets or sets request argument.
            /// </summary>
            public InArgument<SaveCartRequest> Request { get; set; }
    
            /// <summary>
            /// Gets or sets request context argument.
            /// </summary>
            public InArgument<SalesTransaction> Transaction { get; set; }
    
            /// <summary>
            /// Gets or sets request context argument.
            /// </summary>
            public InArgument<SalesTransaction> ReturnTransaction { get; set; }
    
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
                SaveCartRequest request = context.GetValue<SaveCartRequest>(this.Request);
                SalesTransaction transaction = context.GetValue<SalesTransaction>(this.Transaction);
                SalesTransaction returnTransaction = context.GetValue<SalesTransaction>(this.ReturnTransaction);

                // Get the products in the cart lines
                IDictionary<long, SimpleProduct> productsByRecordId = CartWorkflowHelper.GetProductsInCartLines(requestContext, request.Cart.CartLines);

                CartWorkflowHelper.ValidateUpdateCartRequest(requestContext, transaction, returnTransaction, request.Cart, false, productsByRecordId);
            }
        }
    }
}
