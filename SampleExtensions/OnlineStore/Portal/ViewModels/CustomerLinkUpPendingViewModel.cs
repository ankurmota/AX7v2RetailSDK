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
        /// <summary>
        /// View model for finalizing link to existing customer.
        /// </summary>
        public class CustomerLinkUpPendingViewModel : ViewModelBase
        {
            /// <summary>
            /// Gets or sets the email address of existing customer.
            /// </summary>
            /// <value>
            /// The email address of existing customer.
            /// </value>
            public string EmailAddressOfExistingCustomer { get; set; }

            /// <summary>
            /// Gets or sets the activation code.
            /// </summary>
            /// <value>
            /// The activation code.
            /// </value>
            public string ActivationCode { get; set; }
        }
    }
}