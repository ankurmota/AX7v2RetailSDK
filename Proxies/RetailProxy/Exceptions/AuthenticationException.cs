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
    namespace Commerce.RetailProxy
    {
        using System;
        using System.Diagnostics.CodeAnalysis;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Represents the authentication exceptions.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "By design.")]
    
        public class AuthenticationException : RetailProxyException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            public AuthenticationException(string errorResourceId)
                : base(errorResourceId)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            /// <param name="message">The message containing format strings.</param>
            public AuthenticationException(string errorResourceId, string message)
                : base(errorResourceId, message)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="AuthenticationException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            /// <param name="message">The message.</param>
            /// <param name="innerException">The inner exception.</param>
            public AuthenticationException(string errorResourceId, string message, Exception innerException)
                : base(errorResourceId, message, innerException)
            {
            }
        }
    }
}
