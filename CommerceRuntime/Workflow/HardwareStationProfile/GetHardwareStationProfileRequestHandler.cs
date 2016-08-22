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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Request handler to get the hardware profiles.
        /// </summary>
        public sealed class GetHardwareStationProfileRequestHandler :
            SingleRequestHandler<GetHardwareStationProfileRequest, GetHardwareStationProfileResponse>
        {
            /// <summary>
            /// Gets the hardware station profiles from the data service.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            /// <exception cref="Microsoft.Dynamics.Commerce.Runtime.ConfigurationException">Required Service missing.</exception>
            protected override GetHardwareStationProfileResponse Process(GetHardwareStationProfileRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var getHardwareStationProfileDataRequest = new GetHardwareStationDataRequest(QueryResultSettings.AllRecords);
    
                var hardwareStationProfiles = request.RequestContext.Runtime.Execute<EntityDataServiceResponse<HardwareStationProfile>>(
                                                getHardwareStationProfileDataRequest, request.RequestContext).PagedEntityCollection;
    
                return new GetHardwareStationProfileResponse(hardwareStationProfiles);
            }
        }
    }
}
