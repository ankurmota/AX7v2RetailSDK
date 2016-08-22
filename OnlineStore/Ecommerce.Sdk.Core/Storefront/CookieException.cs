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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System;
        using System.Runtime.Serialization;
    
        /// <summary>
        /// Cookie exception.
        /// </summary>
        [Serializable]
        public class CookieException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CookieException"/> class.
            /// </summary>
            public CookieException()
                : base()
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CookieException"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public CookieException(string message)
                : base(message)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CookieException"/> class.
            /// </summary>
            /// <param name="message">The error message that explains the reason for the exception.</param>
            /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
            public CookieException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CookieException"/> class.
            /// </summary>
            /// <param name="serializationInfo">The serialization info.</param>
            /// <param name="streamingContext">The streaming context.</param>
            protected CookieException(SerializationInfo serializationInfo, StreamingContext streamingContext)
                : base(serializationInfo, streamingContext)
            {
            }
        }
    }
}