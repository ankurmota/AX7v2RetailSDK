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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to retrieve the online channel.
        /// </summary>
        public sealed class GetOnlineChannelRequestHandler : SingleRequestHandler<GetOnlineChannelRequest, GetOnlineChannelResponse>
        {
            /// <summary>
            /// Executes the workflow associated with retrieving online channel by channel identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetOnlineChannelResponse Process(GetOnlineChannelRequest request)
            {
                ThrowIf.Null(request, "request");
    
                var getOnlineChannelByIdDataRequest = new GetOnlineChannelByIdDataRequest(request.ChannelId, new ColumnSet());
                OnlineChannel channel = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<OnlineChannel>>(getOnlineChannelByIdDataRequest, this.Context).Entity;
    
                if (channel == null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "Cannot load channel for ID '{0}'.", request.ChannelId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ValueOutOfRange, message);
                }
    
                var settings = QueryResultSettings.AllRecords;
    
                var getChannelProfileByChannelIdDataRequest = new GetChannelProfileByChannelIdDataRequest(request.ChannelId, QueryResultSettings.SingleRecord);
                channel.ChannelProfile = this.Context.Runtime.Execute<SingleEntityDataServiceResponse<ChannelProfile>>(getChannelProfileByChannelIdDataRequest, this.Context).Entity;
    
                var getChannelPropertiesByChannelIdDataRequest = new GetChannelPropertiesByChannelIdDataRequest(request.ChannelId, settings);
                channel.ChannelProperties = this.Context.Runtime.Execute<EntityDataServiceResponse<ChannelProperty>>(getChannelPropertiesByChannelIdDataRequest, this.Context).PagedEntityCollection.Results;
    
                var getChannelLanguagesByChannelIdDataRequest = new GetChannelLanguagesByChannelIdDataRequest(request.ChannelId, settings);
                channel.ChannelLanguages = this.Context.Runtime.Execute<EntityDataServiceResponse<ChannelLanguage>>(getChannelLanguagesByChannelIdDataRequest, this.Context).PagedEntityCollection.Results;

                var getOrgunitContactsDataRequest = new GetOrgUnitContactsDataRequest(new long[] { request.ChannelId }, settings);
                channel.Contacts = this.Context.Runtime.Execute<EntityDataServiceResponse<OrgUnitContact>>(getOrgunitContactsDataRequest, this.Context).PagedEntityCollection.Results;

                var response = new GetOnlineChannelResponse(channel);
                return response;
            }
        }
    }
}
