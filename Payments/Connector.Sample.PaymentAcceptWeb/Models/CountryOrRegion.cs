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
        /// Represents a country or region.
        /// </summary>
        public class CountryOrRegion
        {
            /// <summary>
            /// Gets or sets the two-letter ISO code, e.g. US.
            /// </summary>
            public string TwoLetterCode { get; set; }
    
            /// <summary>
            /// Gets or sets the locale in which the names are, e.g. en-US.
            /// </summary>
            public string Locale { get; set; }
    
            /// <summary>
            /// Gets or sets the short name.
            /// </summary>
            public string ShortName { get; set; }
    
            /// <summary>
            /// Gets or sets the long name.
            /// </summary>
            public string LongName { get; set; }
        }
    }
}
