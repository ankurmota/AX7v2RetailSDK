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
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to deactivate the device.
        /// </summary>
        public sealed class DeactivateDeviceRequestHandler : SingleRequestHandler<DeactivateDeviceRequest, NullResponse>
        {
            /// <summary>
            /// Executes the workflow to deactivate a device.
            /// </summary>
            /// <param name="request">The deactivate device request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(DeactivateDeviceRequest request)
            {
                ThrowIf.Null(request, "request");
    
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                string terminalId = this.Context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;
    
                // Slect all shifts.
                IList<Shift> shifts = ShiftDataDataServiceHelper.GetAllOpenedShiftsOnTerminal(this.Context, this.Context.GetPrincipal().ChannelId, terminalId, true);
    
                if (shifts.HasMultiple())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_TerminalHasAnOpenShift);
                }
    
                var serviceRequest = new DeactivateDeviceServiceRequest(this.Context.GetPrincipal().DeviceNumber, terminalId, this.Context.GetPrincipal().UserId, this.Context.GetPrincipal().DeviceToken);
    
                DeactivateDeviceServiceResponse deactivationResponse = this.Context.Execute<DeactivateDeviceServiceResponse>(serviceRequest);
    
                if (!string.IsNullOrWhiteSpace(deactivationResponse.DeactivationResult.ErrorMessage))
                {
                    throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceDeactivationFailed, deactivationResponse.DeactivationResult.ErrorMessage);
                }
    
                // Log off the user.
                var userLogOffRequest = new UserLogOffRequest
                {
                    StaffId = this.Context.GetPrincipal().UserId, 
                    LogOnConfiguration = this.Context.GetPrincipal().LogOnConfiguration
                };
    
                AuthenticationHelper.LogOff(this.Context, userLogOffRequest);
    
                return new NullResponse();
            }
        }
    }
}
