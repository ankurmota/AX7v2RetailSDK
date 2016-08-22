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
        using SampleConnector.PaymentAcceptWeb.Utilities;
    
        /// <summary>
        /// The data object that maps to the CardPaymentEntry table.
        /// </summary>
        public class CardPaymentEntry
        {
            /// <summary>
            /// Gets or sets the service account ID.
            /// </summary>
            public string ServiceAccountId { get; set; }
    
            /// <summary>
            /// Gets or sets the unique entry ID.
            /// </summary>
            public string EntryId { get; set; }
    
            /// <summary>
            /// Gets or sets the entry data.
            /// </summary>
            public string EntryData { get; set; }
            
            /// <summary>
            /// Gets or sets the UTC date time of the payment entry.
            /// </summary>
            public DateTime EntryUtcTime { get; set; }
            
            /// <summary>
            /// Gets or sets the origin of the host page URI.
            /// </summary>
            public string HostPageOrigin { get; set; }
    
            /// <summary>
            /// Gets or sets the entry locale.
            /// </summary>
            public string EntryLocale { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the entry has been used.
            /// </summary>
            public bool Used { get; set; }
    
            /// <summary>
            /// Gets or sets the industry type, e.g. Retail, DirectMarketing, Ecommerce.
            /// </summary>
            public string IndustryType { get; set; }
    
            /// <summary>
            /// Gets or sets the transaction type, e.g. None, Authorize, Capture.
            /// </summary>
            public string TransactionType { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the transaction should support card swipe.
            /// </summary>
            public bool SupportCardSwipe { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the transaction should tokenize a card.
            /// </summary>
            public bool SupportCardTokenization { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether voice authorization is allowed.
            /// </summary>
            public bool AllowVoiceAuthorization { get; set; }
    
            /// <summary>
            /// Gets or sets the card types to display.
            /// </summary>
            public string CardTypes { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the card holder name.
            /// </summary>
            public string DefaultCardHolderName { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the street 1 of the billing address.
            /// </summary>
            public string DefaultStreet1 { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the street 2 of the billing address.
            /// </summary>
            public string DefaultStreet2 { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the city of the billing address.
            /// </summary>
            public string DefaultCity { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the state or province of the billing address.
            /// </summary>
            public string DefaultStateOrProvince { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the zip code or post code of the billing address.
            /// </summary>
            public string DefaultPostalCode { get; set; }
    
            /// <summary>
            /// Gets or sets the default value of the country code of the billing address.
            /// </summary>
            public string DefaultCountryCode { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the "Same as shipping address" option should be displayed.
            /// </summary>
            public bool ShowSameAsShippingAddress { get; set; }
    
            /// <summary>
            /// Gets a value indicating whether the request has been expired.
            /// </summary>
            public bool IsExpired
            {
                get
                {
                    return DateTime.UtcNow > this.EntryUtcTime.AddMinutes(AppSettings.PaymentEntryValidPeriodInMinutes);
                }
            }
        }
    }
}
