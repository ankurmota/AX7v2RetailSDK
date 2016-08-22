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
        using System.Runtime.Serialization;

        /// <summary>
        /// Response error class.
        /// </summary>
        [DataContract]
        public sealed class ResponseError
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ResponseError"/> class.
            /// </summary>
            public ResponseError()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ResponseError"/> class.
            /// </summary>
            /// <param name="errorCode">Error code value.</param>
            /// <param name="localizedErrorMessage">Localized error message value.</param>
            public ResponseError(string errorCode, string localizedErrorMessage)
            {
                this.LocalizedErrorMessage = localizedErrorMessage;
                this.ErrorCode = errorCode;
            }

            /// <summary>
            /// Gets or sets the value of the error code.
            /// </summary>
            [DataMember]
            public string ErrorCode { get; set; }

            /// <summary>
            /// Gets or sets the value of the localized error message.
            /// </summary>
            [DataMember]
            public string LocalizedErrorMessage { get; set; }
        }
    }
}
