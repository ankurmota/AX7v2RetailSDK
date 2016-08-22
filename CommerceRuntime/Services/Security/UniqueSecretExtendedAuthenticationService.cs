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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Handlers;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;        

        /// <summary>
        /// The service for extended authentication using a simple unique secret.
        /// </summary>
        public abstract class UniqueSecretExtendedAuthenticationService : INamedRequestHandler
        {
            private const int IdentifierLength = 5;
            private const string PasswordGrantType = "password";

            /// <summary>
            /// Gets the unique name for this request handler.
            /// </summary>
            public abstract string HandlerName { get; }

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetUserEnrollmentDetailsServiceRequest),
                        typeof(GetUserAuthenticationCredentialIdServiceRequest),
                        typeof(ConfirmUserAuthenticationServiceRequest)
                    };
                }
            }

            /// <summary>
            /// Executes the request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Response response;
                Type requestType = request.GetType();

                if (requestType == typeof(GetUserEnrollmentDetailsServiceRequest))
                {
                    response = this.GetUserEnrollmentDetails((GetUserEnrollmentDetailsServiceRequest)request);
                }
                else if (requestType == typeof(GetUserAuthenticationCredentialIdServiceRequest))
                {
                    response = this.GetUserAuthenticationCredentialId((GetUserAuthenticationCredentialIdServiceRequest)request);
                }
                else if (requestType == typeof(ConfirmUserAuthenticationServiceRequest))
                {
                    response = this.ConfirmUserAuthentication((ConfirmUserAuthenticationServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request));
                }

                return response;
            }

            /// <summary>
            /// Gets a value indicating whether the service is enabled.
            /// </summary>
            /// <param name="deviceConfiguration">The device configuration.</param>
            /// <returns>A value indicating whether the service is enabled.</returns>
            protected abstract bool IsServiceEnabled(DeviceConfiguration deviceConfiguration);

            /// <summary>
            /// Gets a value indicating whether the service is requires the user password as a second factor authentication.
            /// </summary>
            /// <param name="deviceConfiguration">The device configuration.</param>
            /// <returns>A value indicating whether the service is requires the user password as a second factor authentication.</returns>
            protected abstract bool IsPasswordRequired(DeviceConfiguration deviceConfiguration);

            /// <summary>
            /// Confirms whether the user authentication can complete successfully.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private Response ConfirmUserAuthentication(ConfirmUserAuthenticationServiceRequest request)
            {
                DeviceConfiguration deviceConfiguration = request.RequestContext.GetDeviceConfiguration();

                if (!this.IsServiceEnabled(deviceConfiguration))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationMethodDisabled, "Authentication service is disabled.");
                }

                if (this.IsPasswordRequired(deviceConfiguration))
                {
                    if (string.IsNullOrWhiteSpace(request.Password))
                    {
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordRequired);
                    }

                    // call auth service for password check
                    UserLogOnServiceRequest passwordAuthenticationRequest = new UserLogOnServiceRequest(
                        request.UserId, 
                        request.Password, 
                        request.Credential,
                        PasswordGrantType,
                        request.ExtraAuthenticationParameters);
                    request.RequestContext.Execute<Response>(passwordAuthenticationRequest);
                }

                return new NullResponse();
            }

            /// <summary>
            /// Gets the user credential identifier.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private GetUserAuthenticationCredentialIdServiceResponse GetUserAuthenticationCredentialId(GetUserAuthenticationCredentialIdServiceRequest request)
            {
                return this.GetUserAuthenticationCredentialId(request.Credential, request.RequestContext);
            }

            /// <summary>
            /// Gets the user credential identifier.
            /// </summary>
            /// <param name="credential">The credential.</param>
            /// <param name="requestContext">The request context.</param>
            /// <returns>The response.</returns>
            private GetUserAuthenticationCredentialIdServiceResponse GetUserAuthenticationCredentialId(string credential, RequestContext requestContext)
            {
                DeviceConfiguration deviceConfiguration = requestContext.GetDeviceConfiguration();

                if (!this.IsServiceEnabled(deviceConfiguration))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationMethodDisabled, "Authentication service is disabled.");
                }

                if (string.IsNullOrWhiteSpace(credential))
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_MissingParameter, "credential");
                }

                if (credential.Length <= IdentifierLength)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidFormat, 
                        string.Format("credential is too small. It should be at least '{0}' characters.", IdentifierLength + 1));
                }

                string credentialId = credential.Substring(0, IdentifierLength);
                return new GetUserAuthenticationCredentialIdServiceResponse(credentialId);
            }

            /// <summary>
            /// Gets the user enrollment details.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private GetUserEnrollmentDetailsServiceResponse GetUserEnrollmentDetails(GetUserEnrollmentDetailsServiceRequest request)
            {
                string credentialId = this.GetUserAuthenticationCredentialId(request.Credential, request.RequestContext).CredentialId;
                return new GetUserEnrollmentDetailsServiceResponse(credentialId, string.Empty);
            }
        }
    }
}