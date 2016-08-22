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
    
        /// <summary>
        /// Helper activity to validate arguments before using them.
        /// </summary>
        public sealed class ThrowIfNull : CodeActivity
        {
            /// <summary>
            /// Gets or sets  object to validate.
            /// </summary>
            public InArgument<object> ObjectToValidate { get; set; }
    
            /// <summary>
            /// Gets or sets argument name to provide in exception.
            /// </summary>
            public InArgument<string> ArgumentName { get; set; }
    
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
    
                object value = context.GetValue(this.ObjectToValidate);
                if (value == null)
                {
                    string argumentName = context.GetValue<string>(this.ArgumentName);
                    throw new ArgumentNullException(argumentName);
                }
            }
        }
    }
}
