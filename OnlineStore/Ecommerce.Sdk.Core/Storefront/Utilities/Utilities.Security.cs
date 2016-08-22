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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Security.Claims;

        /// <summary>
        /// Security related utilities.
        /// </summary>
        public partial class Utilities
        {
            /// <summary>
            /// Gets the claim value.
            /// </summary>
            /// <param name="claims">The claims.</param>
            /// <param name="claimType">Type of the claim.</param>
            /// <returns>The claim value.</returns>
            public static string GetClaimValue(IEnumerable<Claim> claims, string claimType)
            {
                string claimValue = string.Empty;

                if (claims != null)
                {
                    Claim claim = claims.Where(cl => string.Equals(cl.Type, claimType, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                    if (claim != null)
                    {
                        claimValue = claim.Value;
                    }
                }

                return claimValue;
            }

            /// <summary>
            /// Returns a masked form of the provided email address.
            /// </summary>
            /// <param name="emailAddress">The email address.</param>
            /// <returns>The masked email address.</returns>
            public static string GetMaskedEmailAddress(string emailAddress)
            {
                string maskedEmailAddress = string.Empty;
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    string[] emailAddressSegments = emailAddress.Split('@');
                    if (emailAddressSegments.Length == 2)
                    {
                        // AbcdD@outlook.com will become Ab*****@outlook.com
                        string maskedUserName = string.Empty;
                        if (emailAddressSegments[0].Length > 1)
                        {
                            maskedUserName = emailAddressSegments[0].Substring(0, 2) + "*****";
                        }
                        else
                        {
                            maskedUserName = emailAddressSegments[0] + "*****";
                        }

                        maskedEmailAddress = maskedUserName + "@" + emailAddressSegments[1];
                    }
                }

                return maskedEmailAddress;
            }
        }
    }
}