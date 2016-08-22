/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

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
    namespace Commerce.Runtime.Workflow
    {
        using System;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        
        /// <summary>
        /// Encapsulates the workflow required to do user authentication renewal.
        /// </summary>
        public sealed class UserAuthenticationRenewalRequestHandler : SingleRequestHandler<UserAuthenticationRenewalRequest, UserAuthenticationRenewalResponse>
        {
            /// <summary>
            /// Executes the workflow to do user authentication renewal.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override UserAuthenticationRenewalResponse Process(UserAuthenticationRenewalRequest request)
            {
                ThrowIf.Null(request, "request");
                Device device = null;
    
                // If device Id is present, authenticate the device to check if the device is active. 
                if (!string.IsNullOrWhiteSpace(this.Context.GetPrincipal().DeviceNumber))
                {
                    device = AuthenticationHelper.AuthenticateDevice(this.Context, this.Context.GetPrincipal().DeviceToken);
                }
    
                // Send authentication renewal request to the service. 
                Employee employee = AuthenticationHelper.AuthenticateRenewalUser(this.Context, device);

                CommerceIdentity identity = new CommerceIdentity(employee, device)
                {
                    // Add the LogOn Configuration to the claim.
                    LogOnConfiguration = this.Context.GetPrincipal().LogOnConfiguration
                };

                return new UserAuthenticationRenewalResponse(employee, device, identity);
            }
        }
    }
}
