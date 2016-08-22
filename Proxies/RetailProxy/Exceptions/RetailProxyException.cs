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
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;

        /// <summary>
        /// Represents the base class for exceptions originating from the client.
        /// </summary>
        public class RetailProxyException : Exception
        {
            /// <summary>
            /// The name of the error code property for serialization purposes.
            /// </summary>
            private const string ErrorResourcePropertyName = "ErrorResourceId";

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailProxyException"/> class.
            /// </summary>
            [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Required for serialization.")]
            public RetailProxyException()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailProxyException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            public RetailProxyException(string errorResourceId)
                : this(errorResourceId, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailProxyException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            /// <param name="message">The message.</param>
            public RetailProxyException(string errorResourceId, string message)
                : this(errorResourceId, message, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="RetailProxyException"/> class.
            /// </summary>
            /// <param name="errorResourceId">The error resource identifier.</param>
            /// <param name="message">The message.</param>
            /// <param name="innerException">The inner exception.</param>
            public RetailProxyException(string errorResourceId, string message, Exception innerException)
                : base(message, innerException)
            {
                this.ErrorResourceId = errorResourceId;
                this.InnerException = innerException;
            }

            /// <summary>
            /// Gets or sets the error resource identifier associated with this exception.
            /// </summary>
            /// <value>
            /// The error code.
            /// </value>
            public string ErrorResourceId { get; set; }

            /// <summary>
            /// Gets or sets the localized user error message associated with this exception.
            /// </summary>
            /// <remarks>The setter is required by deserialization.</remarks>
            public string LocalizedMessage { get; set; }

            /// <summary>
            /// Gets or sets the instance of Exception that describes the error that caused the current exception.
            /// </summary>
            /// <remarks>The setter is required by deserialization.</remarks>
            public new Exception InnerException { get; set; }

            /// <summary>
            /// Gets the exception message.
            /// </summary>
            public override string Message
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.LocalizedMessage))
                    {
                        return base.Message + this.LocalizedMessage;
                    }

                    return base.Message;
                }
            }
        }
    }
}
