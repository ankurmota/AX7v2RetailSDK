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
        /// <summary>
        /// The data object that maps to the CardPaymentResult table.
        /// </summary>
        public class CardPaymentResult
        {
            /// <summary>
            /// Gets or sets the service account ID.
            /// </summary>
            public string ServiceAccountId { get; set; }
    
            /// <summary>
            /// Gets or sets the entry ID of the card payment.
            /// </summary>
            public string EntryId { get; set; }
    
            /// <summary>
            /// Gets or sets the result access code.
            /// </summary>
            public string ResultAccessCode { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the token has been retrieved.
            /// </summary>
            public bool Retrieved { get; set; }
    
            /// <summary>
            /// Gets or sets the payment result XML data.
            /// </summary>
            public string ResultData { get; set; }
        }
    }
}
