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
        using System.Security.Claims;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// View model representing the signed-in user.
        /// </summary>
        public class SignedInUserViewModel : ViewModelBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SignedInUserViewModel"/> class.
            /// </summary>
            /// <param name="claims">The claims.</param>
            public SignedInUserViewModel(IEnumerable<Claim> claims)
            {
                this.EmailAddress = Utilities.GetClaimValue(claims, CookieConstants.Email);
                this.AccountNumber = Utilities.GetClaimValue(claims, CookieConstants.CustomerAccountNumber);
                this.FirstName = Utilities.GetClaimValue(claims, CookieConstants.FirstName);
                this.LastName = Utilities.GetClaimValue(claims, CookieConstants.LastName);
            }

            /// <summary>
            /// Gets or sets the email address.
            /// </summary>
            /// <value>
            /// The email address.
            /// </value>
            public string EmailAddress { get; set; }

            /// <summary>
            /// Gets or sets the account number.
            /// </summary>
            /// <value>
            /// The account number.
            /// </value>
            public string AccountNumber { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get
                {
                    return SignedInUserViewModel.GetFullName(this.FirstName, this.LastName);
                }
            }

            private static string GetFullName(string firstName, string lastName)
            {
                firstName = (firstName ?? string.Empty).Trim();
                lastName = (lastName ?? string.Empty).Trim();
                if (lastName.Length > 1)
                {
                    lastName = lastName.Substring(0, 1);
                }

                string fullName = string.Format("{0} {1}", firstName, lastName).Trim();

                return fullName;
            }
        }
    }
}
