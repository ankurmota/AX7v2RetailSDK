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
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// View model for the sign up view.
        /// </summary>
        public class SignUpViewModel : ViewModelBase
        {
            /// <summary>
            /// Gets or sets the authentication token.
            /// </summary>
            /// <value>
            /// The authentication token.
            /// </value>
            public string AuthenticationToken { get; set; }

            /// <summary>
            /// Gets or sets the type of the external identity provider.
            /// </summary>
            /// <value>
            /// The type of the external identity provider.
            /// </value>
            public IdentityProviderType ExternalIdentityProviderType { get; set; }

            /// <summary>
            /// Gets or sets the email address used for logging on.
            /// </summary>
            /// <value>
            /// The log-on email address.
            /// </value>
            public string LogOnEmailAddress { get; set; }
        }
    }
}