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
    namespace Retail.Ecommerce.Web.Storefront.ViewModels
    {
        using System.Collections.Generic;
        using Microsoft.Owin.Security;

        /// <summary>
        /// View model representing the authentication providers.
        /// </summary>
        public class AuthenticationProvidersViewModel : ViewModelBase
        {
            /// <summary>
            /// Gets or sets the authentication descriptions.
            /// </summary>
            /// <value>
            /// The authentication descriptions.
            /// </value>
            public IEnumerable<AuthenticationDescription> AuthenticationDescriptions { get; set; }

            /// <summary>
            ///  Gets or sets a value indicating whether checkout as a guest is displayed.
            /// </summary>
            public bool IsCheckoutFlow { get; set; }

            /// <summary>
            ///  Gets or sets a value indicating whether link to enter activation code is displayed.
            /// </summary>
            public bool IsActivationFlow { get; set; }
        }
    }
}