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
        /// Activity that performs prerequisites for saving cart.
        /// </summary>
        public sealed class PerformSaveCartOperations : CodeActivity
        {
            /// <summary>
            /// Gets or sets request argument.
            /// </summary>
            public InArgument<SaveCartRequest> Request { get; set; }

            /// <summary>
            /// Gets or sets sales transaction argument.
            /// </summary>
            public InArgument<SalesTransaction> Transaction { get; set; }

            /// <summary>
            /// Gets or sets return transaction argument.
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

                SaveCartRequest request = context.GetValue<SaveCartRequest>(this.Request);
                SalesTransaction transaction = context.GetValue<SalesTransaction>(this.Transaction);
                SalesTransaction returnTransaction = context.GetValue<SalesTransaction>(this.ReturnTransaction);

                // Get products in cart
                Dictionary<long, SimpleProduct> productsByRecordId = CartWorkflowHelper.GetProductsInCartLines(request.RequestContext, request.Cart.CartLines);

                CartWorkflowHelper.PerformSaveCartOperations(request.RequestContext, request, transaction, returnTransaction, new HashSet<string>(StringComparer.OrdinalIgnoreCase), productsByRecordId);
            }
        }
    }
}