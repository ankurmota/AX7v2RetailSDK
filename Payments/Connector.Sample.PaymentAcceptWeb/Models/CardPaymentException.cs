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
    /*
    SAMPLE CODE NOTICE
    
    THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
    OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
    THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
    NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
    */
    namespace Retail.SampleConnector.PaymentAcceptWeb.Models
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Retail.PaymentSDK.Portable;
    
        /// <summary>
        /// Exception class for card payment errors.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Do not need serialization.")]
        public sealed class CardPaymentException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CardPaymentException" /> class.
            /// </summary>
            public CardPaymentException()
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CardPaymentException" /> class.
            /// </summary>
            /// <param name="message">The message describing the error that occurred.</param>
            public CardPaymentException(string message)
                : base(message)
            {
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="CardPaymentException" /> class.
            /// </summary>
            /// <param name="message">The message describing the error that occurred.</param>
            /// <param name="paymentErrors">The payment errors causing the error.</param>
            public CardPaymentException(string message, IEnumerable<PaymentError> paymentErrors)
                : base(message)
            {
                this.PaymentErrors = paymentErrors;
            }
    
            /// <summary>
            /// Gets or sets payment errors.
            /// </summary>
            public IEnumerable<PaymentError> PaymentErrors { get; set; }
        }
    }
}
