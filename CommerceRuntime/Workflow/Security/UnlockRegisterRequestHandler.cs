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
        using System.Security.Principal;
        using Composition;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
      
        /// <summary>
        /// Encapsulates the workflow required to do unlock a register.
        /// </summary>
        public sealed class UnlockRegisterRequestHandler : SingleRequestHandler<UnlockRegisterRequest, NullResponse>
        {
            /// <summary>
            /// Executes the workflow to unlock a register.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(UnlockRegisterRequest request)
            {
                ThrowIf.Null(request, "request");
                Device device = null;
    
                ICommercePrincipal principal = this.Context.GetPrincipal();
                string userId = principal.UserId;
    
                try
                {
                    if (userId != request.StaffId)
                    {
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UnlockRegisterFailed);
                    }
    
                    if (request.DeviceId != null && request.DeviceToken != null)
                    {
                        // Authenticate device only when DeviceId is specified
                        device = AuthenticationHelper.AuthenticateDevice(
                            this.Context,
                            request.DeviceToken);
                    }
                    
                    // Unlock the terminal
                    AuthenticationHelper.UnlockRegister(this.Context, device, request);
                    return new NullResponse();
                }
                catch (DeviceAuthenticationException ex)
                {
                    RetailLogger.Log.CrtWorkflowUnlockRegisterRequestHandlerFailure(request.StaffId, request.DeviceId, ex);
                    throw;
                }
                catch (UserAuthenticationException ex)
                {
                    RetailLogger.Log.CrtWorkflowUnlockRegisterRequestHandlerFailure(request.StaffId, request.DeviceId, ex);
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UnlockRegisterFailed);
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.CrtWorkflowUnlockRegisterRequestHandlerFailure(request.StaffId, request.DeviceId, ex);
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed);
                }
            }
        }
    }
}
