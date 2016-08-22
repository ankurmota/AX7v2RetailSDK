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
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// The request handler for deleting user credentials.
        /// </summary>
        public class UnenrollUserCredentialsRequestHandler : SingleRequestHandler<UnenrollUserCredentialRequest, NullResponse>
        {
            /// <summary>
            /// Executes the workflow.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(UnenrollUserCredentialRequest request)
            {
                ThrowIf.Null(request, "request");

                if (string.IsNullOrWhiteSpace(request.GrantType))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MissingParameter, "grantType is missing.");
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MissingParameter, "userId is missing.");
                }

                // Disenroll user in headquarters
                UnenrollUserCredentialsRealtimeRequest disenrollUserRealtimeRequest = new UnenrollUserCredentialsRealtimeRequest(
                    request.UserId,
                    request.GrantType);
                this.Context.Runtime.Execute<NullResponse>(disenrollUserRealtimeRequest, this.Context);

                // Delete data on local database
                DeleteUserCredentialsDataRequest deleteUserCredentialsDataRequest = new DeleteUserCredentialsDataRequest(
                    request.UserId,
                    request.GrantType);
                this.Context.Runtime.Execute<NullResponse>(deleteUserCredentialsDataRequest, this.Context);

                // Create auth log
                AuthenticationHelper.LogAuthenticationRequest(request.RequestContext, request.UserId, AuthenticationStatus.Success, AuthenticationOperation.EnrollUserCredentials);

                return new NullResponse();
            }
        }
    }    
}