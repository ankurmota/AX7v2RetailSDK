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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;

        /// <summary>
        /// User authentication Service.
        /// </summary>
        public class UserAuthenticationTransactionService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(UserResetPasswordRealtimeRequest),
                        typeof(UserChangePasswordRealtimeRequest),
                        typeof(StaffLogOnRealtimeRequest),
                        typeof(UserLogOffRealtimeRequest),
                        typeof(UserLogOnRenewalRealtimeRequest),
                        typeof(ValidateStaffPasswordRealtimeRequest),
                        typeof(GetEmployeeIdentityByExternalIdentityRealtimeRequest),
                        typeof(GetUserCredentialsRealtimeRequest),
                        typeof(EnrollUserCredentialsRealtimeRequest),
                        typeof(UnenrollUserCredentialsRealtimeRequest),
                        typeof(GetUserCredentialsRealtimeRequest),
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

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(UserResetPasswordRealtimeRequest))
                {
                    response = ResetPassword((UserResetPasswordRealtimeRequest)request);
                }
                else if (requestType == typeof(UserChangePasswordRealtimeRequest))
                {
                    response = ChangePassword((UserChangePasswordRealtimeRequest)request);
                }
                else if (requestType == typeof(StaffLogOnRealtimeRequest))
                {
                    response = LogOnUser((StaffLogOnRealtimeRequest)request);
                }
                else if (requestType == typeof(UserLogOffRealtimeRequest))
                {
                    response = LogOffUser((UserLogOffRealtimeRequest)request);
                }
                else if (requestType == typeof(UserLogOnRenewalRealtimeRequest))
                {
                    response = LogOnUserRenewal((UserLogOnRenewalRealtimeRequest)request);
                }
                else if (requestType == typeof(ValidateStaffPasswordRealtimeRequest))
                {
                    response = ValidateStaffPassword((ValidateStaffPasswordRealtimeRequest)request);
                }
                else if (requestType == typeof(GetEmployeeIdentityByExternalIdentityRealtimeRequest))
                {
                    response = GetEmployeeIdentityByExternalIdentity((GetEmployeeIdentityByExternalIdentityRealtimeRequest)request);
                }
                else if (requestType == typeof(UnenrollUserCredentialsRealtimeRequest))
                {
                    response = UnenrollUserCredentials((UnenrollUserCredentialsRealtimeRequest)request);
                }
                else if (requestType == typeof(EnrollUserCredentialsRealtimeRequest))
                {
                    response = EnrollUserCredentials((EnrollUserCredentialsRealtimeRequest)request);
                }
                else if (requestType == typeof(GetUserCredentialsRealtimeRequest))
                {
                    response = GetUserCredentials((GetUserCredentialsRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets the user credentials associated to the <paramref name="request"/>.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetUserCredentialsRealtimeResponse GetUserCredentials(GetUserCredentialsRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                UserCredential userCredential;

                try
                {
                    userCredential = transactionService.GetUserCredentials(
                        request.CredentialId,
                        request.GrantType);
                }
                catch (HeadquarterTransactionServiceException)
                {
                    // transaction service fails when record was not found
                    userCredential = null;
                }
                
                return new GetUserCredentialsRealtimeResponse(userCredential);
            }

            /// <summary>
            /// Enrolls user credentials.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static EnrollUserCredentialsRealtimeResponse EnrollUserCredentials(EnrollUserCredentialsRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                return new EnrollUserCredentialsRealtimeResponse(
                    transactionService.EnrollUserCredentials(
                        request.UserId, 
                        request.GrantType, 
                        request.CredentialId, 
                        request.Credential, 
                        request.AdditionalAuticationData));
            }

            /// <summary>
            /// Removes the association between user and grant type regarding enrollment credentials.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static NullResponse UnenrollUserCredentials(UnenrollUserCredentialsRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                transactionService.UnenrollUserCredentials(request.UserId, request.GrantType);
                return new NullResponse();
            }

            /// <summary>
            /// Reset password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static StaffChangePasswordRealtimeResponse ResetPassword(UserResetPasswordRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                string newPasswordHash;
                string newPasswordSalt;
                string newPasswordHashAlgorithm;
                DateTimeOffset newPasswordLastChangedDateTime;
                AuthenticationOperation newPasswordLastUpdatedOperation;

                transactionService.StaffResetPassword(
                    request.TargetUserId,
                    request.NewPassword,
                    request.ChangePassword,
                    out newPasswordHash,
                    out newPasswordSalt,
                    out newPasswordHashAlgorithm,
                    out newPasswordLastChangedDateTime,
                    out newPasswordLastUpdatedOperation);

                return new StaffChangePasswordRealtimeResponse(newPasswordHash, newPasswordSalt, newPasswordHashAlgorithm, newPasswordLastChangedDateTime, newPasswordLastUpdatedOperation);
            }

            /// <summary>
            /// Gets the employee identity given the external identity.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static Response GetEmployeeIdentityByExternalIdentity(GetEmployeeIdentityByExternalIdentityRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                Employee employee = transactionService.GetRetailServerStaffByExternalIdentity(request.ExternalIdentityId, request.ExternalIdentitySubId);

                var employeeIdentity = new CommerceIdentity(employee, null);
                return new GetEmployeeIdentityByExternalIdentityRealtimeResponse(employeeIdentity);
            }

            /// <summary>
            /// Validates the staff password.
            /// </summary>
            /// <param name="request">The validate staff request.</param>
            /// <returns>The validate staff response.</returns>
            private static NullResponse ValidateStaffPassword(ValidateStaffPasswordRealtimeRequest request)
            {
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                transactionService.RetailServerStaffIsPasswordValidForStaff(request.StaffId, request.Password);

                return new NullResponse();
            }

            /// <summary>
            /// Change password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static StaffChangePasswordRealtimeResponse ChangePassword(UserChangePasswordRealtimeRequest request)
            {
                string staffId = request.StaffId;

                if (string.IsNullOrWhiteSpace(staffId))
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_ChangePasswordFailed, "Staff Id is not valid.");
                }

                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                string newPasswordHash;
                string newPasswordSalt;
                string newPasswordHashAlgorithm;
                DateTimeOffset newPasswordLastChangedDateTime;
                AuthenticationOperation newPasswordLastUpdatedOperation;

                transactionService.StaffChangePassword(
                    staffId,
                    request.OldPassword,
                    request.NewPassword,
                    changePassword: false,
                    newPasswordHash: out newPasswordHash,
                    newPasswordSalt: out newPasswordSalt,
                    newPasswordHashAlgorithm: out newPasswordHashAlgorithm,
                    newPasswordLastChangedDateTime: out newPasswordLastChangedDateTime,
                    newPasswordLastUpdatedOperation: out newPasswordLastUpdatedOperation);

                return new StaffChangePasswordRealtimeResponse(newPasswordHash, newPasswordSalt, newPasswordHashAlgorithm, newPasswordLastChangedDateTime, newPasswordLastUpdatedOperation);
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static StaffLogOnRealtimeResponse LogOnUser(StaffLogOnRealtimeRequest request)
            {
                Employee employee;

                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                long channelId = request.ChannelId.GetValueOrDefault(0);
                long terminalId = request.TerminalRecordId.GetValueOrDefault(0);
                bool logOntoStore = request.ChannelId.HasValue;
                bool skipPasswordVerification = string.IsNullOrWhiteSpace(request.StaffPassword);

                employee = transactionService.RetailServerStaffLogOn(request.StaffId, channelId, terminalId, request.StaffPassword, logOntoStore, skipPasswordVerification);
                return new StaffLogOnRealtimeResponse(employee);
            }

            /// <summary>
            /// LogOff the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static NullResponse LogOffUser(UserLogOffRealtimeRequest request)
            {
                UserAuthenticationTransactionService.ThrowIfInvalidLogonConfiguration(request.LogOnConfiguration);
                TransactionService.TransactionServiceClient transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);

                ICommercePrincipal principal = request.RequestContext.GetPrincipal();

                if (string.IsNullOrEmpty(principal.UserId))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "User is not authenticated.");
                }

                transactionService.RetailServerStaffLogOff(principal.UserId, principal.ChannelId, principal.TerminalId, !principal.IsTerminalAgnostic);
                return new NullResponse();
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static UserLogOnRenewalRealtimeResponse LogOnUserRenewal(UserLogOnRenewalRealtimeRequest request)
            {
                var transactionService = new TransactionService.TransactionServiceClient(request.RequestContext);
                Employee employee = transactionService.RetailServerStaffLogOnRenewal(request.StaffId);
                return new UserLogOnRenewalRealtimeResponse(employee);
            }

            /// <summary>
            /// Throws if invalid logon configuration.
            /// </summary>
            /// <param name="logOnConfiguration">The log on configuration.</param>
            /// <exception cref="System.InvalidOperationException">Invalid LogOnConfiguration of a request.</exception>
            private static void ThrowIfInvalidLogonConfiguration(LogOnConfiguration logOnConfiguration)
            {
                if (logOnConfiguration != LogOnConfiguration.RealTimeService)
                {
                    throw new InvalidOperationException("Invalid LogOnConfiguration of a request.");
                }
            }
        }
    }
}
