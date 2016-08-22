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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services;
    
        /// <summary>
        /// Request handler to get the device configurations.
        /// </summary>
        public sealed class GetDeviceConfigurationRequestHandler :
            SingleRequestHandler<GetDeviceConfigurationRequest, GetDeviceConfigurationResponse>
        {
            /// <summary>
            /// Gets the device configuration.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.ConfigurationException">Required Service missing.</exception>
            protected override GetDeviceConfigurationResponse Process(GetDeviceConfigurationRequest request)
            {
                ThrowIf.Null(request, "request");
    
                GetDeviceConfigurationDataRequest dataServiceRequest = new GetDeviceConfigurationDataRequest(includeImages: true);
                var response = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<DeviceConfiguration>>(dataServiceRequest, this.Context);
    
                if (response.Entity == null)
                {
                    throw new ConfigurationException(
                        ConfigurationErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceConfigurationNotFound,
                        string.Format("There is no device configuration for the current channel '{0}' and terminal {1}", this.Context.GetPrincipal().ChannelId, this.Context.GetTerminal().TerminalId));
                }
    
                return new GetDeviceConfigurationResponse(response.Entity);
            }
        }
    }
}
