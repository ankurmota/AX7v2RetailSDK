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
    namespace Commerce.RetailProxy.Authentication
    {
        using Microsoft.Dynamics.Commerce.Runtime;
        using Newtonsoft.Json;
    
        /// <summary>
        /// The reset password request.
        /// </summary>
        internal class ResetPasswordRequest
        {
            /// <summary>
            /// Gets or sets the targeted user identifier.
            /// </summary>
            [JsonProperty("userId")]
            public string UserId { get; set; }
    
            /// <summary>
            /// Gets or sets the new password.
            /// </summary>
            [JsonProperty("newPassword")]
            public string NewPassword { get; set; }
    
            /// <summary>
            /// Gets or sets a value indicating whether the password must be changed at the next logon.
            /// </summary>
            [JsonProperty("mustChangePasswordAtNextLogOn")]
            public bool MustChangePasswordAtNextLogOn { get; set; }
        }
    }
}
