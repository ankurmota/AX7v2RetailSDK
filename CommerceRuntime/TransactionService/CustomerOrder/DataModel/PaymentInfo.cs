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
    namespace Commerce.Runtime.Services.CustomerOrder
    {
        using System;
        using System.Collections.ObjectModel;
        using System.Diagnostics.CodeAnalysis;
        using System.IO;
        using System.Xml.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime;
    
        /// <summary>
        /// Payment info, for use in transmitting via TS call.
        /// </summary>
        [Serializable]
        [XmlType("Payment")]
        public class PaymentInfo
        {
            /// <summary>
            /// Gets or sets the payment type.
            /// </summary>
            [XmlAttribute("PaymentType")]
            public string PaymentType { get; set; }
    
            /// <summary>
            /// Gets or sets the Card type.
            /// </summary>
            [XmlAttribute("CardType")]
            public string CardType { get; set; }
    
            /// <summary>
            /// Gets or sets the payment amount collected.
            /// </summary>
            [XmlAttribute("Amount")]
            public decimal Amount { get; set; }
    
            /// <summary>
            /// Gets or sets the currency code of the payment.
            /// </summary>
            [XmlAttribute("Currency")]
            public string Currency { get; set; }
    
            /// <summary>
            /// Gets or sets the date as string.
            /// </summary>
            [XmlAttribute("Date")]
            public string DateString { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether prepayment was made.
            /// </summary>
            [XmlAttribute("Prepayment")]
            public bool Prepayment { get; set; }
    
            /// <summary>
            /// Gets or sets the credit card token for the order.
            /// </summary>
            [XmlElement("CreditCardToken")]
            public string CreditCardToken { get; set; }
    
            /// <summary>
            /// Gets or sets the credit card authorization for the order.
            /// </summary>
            [XmlElement("CreditCardAuthorization")]
            public string CreditCardAuthorization { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the payment was captured.
            /// </summary>
            [XmlAttribute("PaymentCaptured")]
            public bool PaymentCaptured { get; set; }
        }
    }
}
