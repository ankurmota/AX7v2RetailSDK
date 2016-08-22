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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Request handler for enrolling a user's credential.
        /// </summary>
        public class EnrollUserCredentialsRequestHandler : SingleRequestHandler<EnrollUserCredentialRequest, NullResponse>
        {
            private const string PasswordGrantType = "password";

            /// <summary>
            /// Executes the workflow.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override NullResponse Process(EnrollUserCredentialRequest request)
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

                if (request.ExtraParameters == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MissingParameter, "extraParameters is missing.");
                }

                if (request.GrantType.Equals(PasswordGrantType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_RequestParameterValueNotSupported, "grant type 'password' is not supported for enrollment.");
                }

                // get the request handler that handles this specific grant type
                IRequestHandler authenticationService = this.Context.Runtime.GetRequestHandler(typeof(GetUserEnrollmentDetailsServiceRequest), request.GrantType);

                if (authenticationService == null)
                {
                    RetailLogger.Log.CrtServicesAuthenticationHandlerNotFound(request.GrantType, typeof(GetUserEnrollmentDetailsServiceRequest));
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationGrantTypeNotSupported, 
                        string.Format("The grant type '{0}' is not supported.", request.GrantType));
                }

                // Get enrollment data from authentication service
                GetUserEnrollmentDetailsServiceRequest getEnrollmentDetailsRequest = new GetUserEnrollmentDetailsServiceRequest(request.Credentials, request.ExtraParameters);
                GetUserEnrollmentDetailsServiceResponse enrollmentDetailsResponse = this.Context.Runtime.Execute<GetUserEnrollmentDetailsServiceResponse>(
                    getEnrollmentDetailsRequest,
                    this.Context,
                    authenticationService);

                // Enroll user in headquarters
                EnrollUserCredentialsRealtimeRequest enrollUserRealtimeRequest = new EnrollUserCredentialsRealtimeRequest(
                    request.UserId,
                    request.GrantType,
                    enrollmentDetailsResponse.CredentialId,
                    request.Credentials,
                    enrollmentDetailsResponse.AdditionalAuthenticationData);
                EnrollUserCredentialsRealtimeResponse enrollUserRealtimeResponse = this.Context.Runtime.Execute<EnrollUserCredentialsRealtimeResponse>(enrollUserRealtimeRequest, this.Context);

                // Persist data on local database
                SaveUserCredentialsDataRequest saveUserCredentialsDataRequest = new SaveUserCredentialsDataRequest(enrollUserRealtimeResponse.UserCredential);
                this.Context.Runtime.Execute<NullResponse>(saveUserCredentialsDataRequest, this.Context);

                // Create auth log
                AuthenticationHelper.LogAuthenticationRequest(request.RequestContext, request.UserId, AuthenticationStatus.Success, AuthenticationOperation.EnrollUserCredentials);

                return new NullResponse();
            }
        }
    }
}