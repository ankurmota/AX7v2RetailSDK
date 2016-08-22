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
        using System.Diagnostics;
        using System.Globalization;
        using Commerce.Runtime.Services.Security;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// User authentication Service.
        /// </summary>
        public class UserAuthenticationService : IRequestHandler
        {
            private const string PasswordAuthenticationGrantType = "password";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(UserLogOnServiceRequest),
                        typeof(UserLogOffServiceRequest),
                        typeof(CheckAccessServiceRequest),
                        typeof(CheckAccessIsManagerServiceRequest),
                        typeof(CheckAccessHasShiftServiceRequest),
                        typeof(CheckAccessToCartServiceRequest),
                        typeof(CheckAccessToCustomerAccountServiceRequest),
                        typeof(UserLogOnRenewalServiceRequest),
                        typeof(UserResetPasswordServiceRequest),
                        typeof(UserChangePasswordServiceRequest),
                        typeof(UnlockRegisterServiceRequest),
                        typeof(GetCommerceIdentityByExternalIdentityServiceRequest)
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
                if (requestType == typeof(UserLogOnServiceRequest))
                {
                    response = AuthenticateUser((UserLogOnServiceRequest)request);
                }
                else if (requestType == typeof(UserLogOffServiceRequest))
                {
                    response = LogOffUser((UserLogOffServiceRequest)request);
                }
                else if (requestType == typeof(CheckAccessServiceRequest))
                {
                    response = CheckAccess((CheckAccessServiceRequest)request);
                }
                else if (requestType == typeof(CheckAccessIsManagerServiceRequest))
                {
                    response = CheckManagerAccess((CheckAccessIsManagerServiceRequest)request);
                }
                else if (requestType == typeof(CheckAccessHasShiftServiceRequest))
                {
                    response = CheckHasShift((CheckAccessHasShiftServiceRequest)request);
                }
                else if (requestType == typeof(UserLogOnRenewalServiceRequest))
                {
                    response = LogOnUserRenewal((UserLogOnRenewalServiceRequest)request);
                }
                else if (requestType == typeof(UserResetPasswordServiceRequest))
                {
                    response = ResetPassword((UserResetPasswordServiceRequest)request);
                }
                else if (requestType == typeof(UserChangePasswordServiceRequest))
                {
                    response = ChangePassword((UserChangePasswordServiceRequest)request);
                }
                else if (requestType == typeof(UnlockRegisterServiceRequest))
                {
                    response = UnlockRegister((UnlockRegisterServiceRequest)request);
                }
                else if (requestType == typeof(GetCommerceIdentityByExternalIdentityServiceRequest))
                {
                    response = GetCommerceIdentityByExternalIdentity((GetCommerceIdentityByExternalIdentityServiceRequest)request);
                }
                else if (requestType == typeof(CheckAccessToCartServiceRequest))
                {
                    response = CheckAccessToCart((CheckAccessToCartServiceRequest)request);
                }
                else if (requestType == typeof(CheckAccessToCustomerAccountServiceRequest))
                {
                    response = CheckAccessToCustomerAccount((CheckAccessToCustomerAccountServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The authentication request.</param>
            /// <returns>The authentication response.</returns>
            private static UserLogOnServiceResponse AuthenticateUser(UserLogOnServiceRequest request)
            {
                string staffId;

                // default to password authentication method if grant type is not defined
                if (string.IsNullOrWhiteSpace(request.GrantType) || PasswordAuthenticationGrantType.Equals(request.GrantType, StringComparison.OrdinalIgnoreCase))
                {
                    AuthenticateUserWithPassword(request.RequestContext, request.StaffId, request.Password);
                    staffId = request.StaffId;
                }
                else
                {
                    // extended authentication
                    staffId = AuthenticateUserExtended(request);
                }

                return new UserLogOnServiceResponse(staffId);
            }

            /// <summary>
            /// Authenticate the user using the extension mechanism.
            /// </summary>
            /// <param name="request">The authentication request.</param>
            /// <returns>The staff identifier that was authenticated.</returns>
            private static string AuthenticateUserExtended(UserLogOnServiceRequest request)
            {
                IRequestHandler authenticationHandler = request.RequestContext.Runtime.GetRequestHandler(typeof(GetUserAuthenticationCredentialIdServiceRequest), request.GrantType);

                // get service responsible for this grant type
                if (authenticationHandler == null)
                {
                    RetailLogger.Log.CrtServicesAuthenticationHandlerNotFound(request.GrantType, typeof(GetUserAuthenticationCredentialIdServiceRequest));
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_GrantTypeNotSupported);
                }

                // make a call to the authenticationHandler to get the credential id
                GetUserAuthenticationCredentialIdServiceRequest getCredentialIdRequest = new GetUserAuthenticationCredentialIdServiceRequest(request.Credential, request.AdditionalAuthenticationParameters);
                string credentialId = request.RequestContext.Execute<GetUserAuthenticationCredentialIdServiceResponse>(getCredentialIdRequest, authenticationHandler).CredentialId;

                // lookup on DB or transaction service as appropriated
                UserCredential userCredential;

                // perform realtime lookup if request is channel agnostic (as we cannot use the DB)
                // or the system is configured to perform realtime checks
                bool performRealTimeLookup = request.RequestContext.GetPrincipal().IsChannelAgnostic
                    || request.RequestContext.GetChannelConfiguration().TransactionServiceProfile.StaffLogOnConfiguration == LogOnConfiguration.RealTimeService;

                if (performRealTimeLookup)
                {
                    GetUserCredentialsRealtimeRequest getUserCredentialsRealTimeRequest = new GetUserCredentialsRealtimeRequest(credentialId, request.GrantType);
                    userCredential = request.RequestContext.Execute<GetUserCredentialsRealtimeResponse>(getUserCredentialsRealTimeRequest).UserCredential;
                }
                else
                {
                    GetUserCredentialsDataRequest getUserCredentialRequest = new GetUserCredentialsDataRequest(credentialId, request.GrantType);
                    userCredential = request.RequestContext.Execute<SingleEntityDataServiceResponse<UserCredential>>(getUserCredentialRequest).Entity;
                }

                // if no matching user credential was found, fail as no valid credential was provided
                if (userCredential == null)
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Invalid credentials.");
                }

                // make sure that the credential provided match the hashed value
                string hashedInputCredential = HashPassword(request.RequestContext, request.Credential, userCredential.HashAlgorithm, credentialId, userCredential.Salt);
                if (!string.Equals(userCredential.HashedCredential, hashedInputCredential, StringComparison.Ordinal))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Invalid credentials.");
                }                

                // confirm authentication - give the extended authentication service implementation to execute futher authentication validations
                // before stating that the user is authenticated
                ConfirmUserAuthenticationServiceRequest confirmUserAuthenticationRequest = new ConfirmUserAuthenticationServiceRequest(
                    userCredential.StaffId,
                    request.Password,
                    request.Credential,
                    userCredential.AdditionalAuthenticationData, 
                    request.AdditionalAuthenticationParameters);
                request.RequestContext.Execute<NullResponse>(confirmUserAuthenticationRequest, authenticationHandler);

                return userCredential.StaffId;
            }

            /// <summary>
            /// Authenticate the user using the password mechanism.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="password">The staff password.</param>
            private static void AuthenticateUserWithPassword(RequestContext requestContext, string staffId, string password)
            {
                ICommercePrincipal principal = requestContext.GetPrincipal();
                LogOnConfiguration logOnConfiguration;

                long? channelId = null;
                long? terminalRecordId = null;

                if (!principal.IsTerminalAgnostic)
                {
                    channelId = principal.ChannelId;
                    terminalRecordId = principal.TerminalId;
                }

                // Validate the logon parameters.
                if (string.IsNullOrWhiteSpace(staffId) || string.IsNullOrWhiteSpace(password))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_IncorrectLogonTypeUserAccountOrPassword, "Staff identifier or password is missing.");
                }

                // For our custom login, we do both authentication and authorization
                // this will save a transaction service call latter on
                StaffRealTimeSecurityValidationHelper staffSecurityHelper = StaffRealTimeSecurityValidationHelper.Create(
                    requestContext,
                    SecurityVerificationType.Authentication | SecurityVerificationType.Authorization,
                    staffId,
                    channelId,
                    terminalRecordId,
                    password: password);

                logOnConfiguration = staffSecurityHelper.GetSecurityVerificationConfiguration();

                VerifyUserLockoutPolicy(staffId, requestContext);

                Employee employee;

                try
                {
                    // Authenticate the user using real time service if device is not specified or if terminal configuration is set with RealTime Service.
                    switch (logOnConfiguration)
                    {
                        case LogOnConfiguration.RealTimeService:
                            employee = EmployeeLogOnRealTimeService(requestContext, password, staffSecurityHelper);
                            break;

                        case LogOnConfiguration.LocalDatabase:
                            employee = EmployeeLogOnStore(requestContext, staffId, password, channelId.GetValueOrDefault());
                            break;

                        default:
                            string errorMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                "The authentication configuration value '{0}' is not supported.",
                                logOnConfiguration);
                            throw new NotSupportedException(errorMessage);
                    }
                }
                catch (UserAuthenticationException ex)
                {
                    if (ex.ErrorResourceId == SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword.ToString())
                    {
                        LogAuthenticationRequest(requestContext, staffId, AuthenticationStatus.InvalidPasswordFailure, AuthenticationOperation.CreateToken);

                        // SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword is used internally to identify when user provides incorrect password
                        // here we make sure this is not surfaced outside of the runtime, so there is no information disclosure
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Incorrect user name or password.");
                    }

                    throw;
                }

                int passwordExpiryIntervalInDays = 0;
                ChannelConfiguration channelConfiguration = null;

                if (channelId.HasValue)
                {
                    GetChannelConfigurationDataRequest getChannelConfiguration = new GetChannelConfigurationDataRequest(channelId.GetValueOrDefault());
                    channelConfiguration = requestContext.Execute<SingleEntityDataServiceResponse<ChannelConfiguration>>(getChannelConfiguration).Entity;

                    passwordExpiryIntervalInDays = channelConfiguration.PasswordExpiryIntervalInDays;
                }

                if (employee.IsPasswordExpired || IsEmployeePasswordExpired(passwordExpiryIntervalInDays, employee.PasswordLastChangedDateTime))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserPasswordExpired);
                }
            }

            /// <summary>
            /// Writes an entry into the audit table.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="source">The log source.</param>
            /// <param name="value">The log entry.</param>
            /// <param name="traceLevel">The trace level.</param>
            private static void LogAuditEntry(RequestContext context, string source, string value, AuditLogTraceLevel traceLevel = AuditLogTraceLevel.Trace)
            {
                var auditLogServiceRequest = new InsertAuditLogServiceRequest(source, value, traceLevel, unchecked((int)context.RequestTimer.ElapsedMilliseconds));
                context.Execute<NullResponse>(auditLogServiceRequest);
            }

            /// <summary>
            /// LogOff the user.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="password">The staff password.</param>
            /// <param name="staffSecurityHelper">The staff security helper.</param>
            /// <returns>The employee object.</returns>
            private static Employee EmployeeLogOnRealTimeService(RequestContext requestContext, string password, StaffRealTimeSecurityValidationHelper staffSecurityHelper)
            {
                try
                {
                    return staffSecurityHelper.VerifyEmployeeRealTimeService(() =>
                    {
                        return EmployeeLogOnStore(requestContext, staffSecurityHelper.StaffId, password, staffSecurityHelper.ChannelId.GetValueOrDefault());
                    });
                }
                catch (UserAuthenticationException)
                {
                    throw;
                }
                catch (UserAuthorizationException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    // any exceptions that might happen will cause the authentication to fail
                    throw new UserAuthenticationException(
                        SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed,
                        exception,
                        exception.Message);
                }
            }

            /// <summary>
            /// Logs on the user.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="password">The staff password.</param>
            /// <param name="channelId">The channel identifier.</param>
            /// <returns>The employee object.</returns>
            private static Employee EmployeeLogOnStore(RequestContext requestContext, string staffId, string password, long channelId)
            {
                Employee localEmployee;

                string passwordHash = string.Empty;
                string salt = string.Empty;
                string passwordHashAlgorithm = string.Empty;

                GetEmployeePasswordCryptoInfoDataRequest employeePasswordCryptoInfoRequest = new GetEmployeePasswordCryptoInfoDataRequest(channelId, staffId);
                EmployeePasswordCryptoInfo result = requestContext.Execute<SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo>>(employeePasswordCryptoInfoRequest).Entity;

                if (result != null)
                {
                    salt = result.PasswordSalt;
                    passwordHashAlgorithm = result.PasswordHashAlgorithm;
                }

                if (string.IsNullOrEmpty(passwordHashAlgorithm))
                {
                    passwordHashAlgorithm = UserAuthenticationService.GetTransactionServiceProfile(requestContext).StaffPasswordHash;
                }

                if (string.IsNullOrEmpty(salt))
                {
                    var exception = new UserAuthenticationException(
                                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials,
                                            "Log on to the store locally failed due to the employee password not configured.");
                    RetailLogger.Log.CrtServicesStaffAuthorizationServiceUserPasswordNotConfigured(staffId);
                    throw exception;
                }

                if (!string.IsNullOrWhiteSpace(password))
                {
                    passwordHash = UserAuthenticationService.HashPassword(requestContext, password, passwordHashAlgorithm, staffId, salt);
                }

                // Logon to the store
                EmployeeLogOnStoreDataRequest employeeDataRequest = new EmployeeLogOnStoreDataRequest(channelId, staffId, passwordHash, new ColumnSet());

                localEmployee = requestContext.Execute<SingleEntityDataServiceResponse<Employee>>(employeeDataRequest).Entity;

                if (localEmployee == null || localEmployee.Permissions == null)
                {
                    // SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword is used internally to identify when user provides incorrect password
                    // here we make sure this is not surfaced outside of the runtime, so there is no information disclosure
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword, "Incorrect user name or password.");
                }

                return localEmployee;
            }

            /// <summary>
            /// Logs off the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static NullResponse LogOffUser(UserLogOffServiceRequest request)
            {
                ICommercePrincipal principal = request.RequestContext.GetPrincipal();

                if (string.IsNullOrEmpty(principal.UserId))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "User is not authenticated.");
                }

                string staffId = principal.UserId;
                string terminalId = request.RequestContext.GetPrincipal().IsTerminalAgnostic ? string.Empty : request.RequestContext.GetTerminal().TerminalId;

                UnlockUserAtLogOffDataRequest unlockUserDataRequest = new UnlockUserAtLogOffDataRequest(
                            principal.ChannelId,
                            terminalId,
                            staffId,
                            request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);

                switch (request.LogOnConfiguration)
                {
                    case LogOnConfiguration.RealTimeService:
                        try
                        {
                            UserLogOffRealtimeRequest realtimeRequest = new UserLogOffRealtimeRequest(request.LogOnConfiguration);
                            request.RequestContext.Execute<NullResponse>(realtimeRequest);
                        }
                        catch (Exception exception)
                        {
                            // Allow logoff during internal error
                            NetTracer.Warning("Transaction service error during logoff", exception.Message);
                            return new NullResponse();
                        }

                        // unlock / clear session from local database - this is required as sessions are created on local db independently
                        // from RTS configuration
                        request.RequestContext.Execute<SingleEntityDataServiceResponse<bool>>(unlockUserDataRequest);
                        break;

                    case LogOnConfiguration.LocalDatabase:
                        if (principal.IsTerminalAgnostic)
                        {
                            throw new NotSupportedException("LogOffUser can only be performed for a request that is not terminal agnostic.");
                        }

                        // unlock / clear session from local database
                        request.RequestContext.Execute<SingleEntityDataServiceResponse<bool>>(unlockUserDataRequest);
                        break;

                    default:
                        string errorMessage = string.Format("LogOffUser is not supported when LogOnConfiguration is {0}", request.LogOnConfiguration);
                        throw new NotSupportedException(errorMessage);
                }

                RetailLogger.Log.CrtServicesStaffAuthorizationServiceUserSessionEnded(staffId, terminalId);

                // Audit Log logoff request
                var auditMessage = string.Format("User has successfully logged off. OperatorID: {0}", staffId);
                LogAuditEntry(request.RequestContext, "UserAuthenticationService.LogOffUser", auditMessage);

                return new NullResponse();
            }

            /// <summary>
            /// Reset password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static NullResponse ResetPassword(UserResetPasswordServiceRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.TargetUserId))
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_ResetPasswordFailed, "Staff identifier must not be null or empty.");
                }

                // check permissions for operation
                request.RequestContext.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ResetPassword));

                UserResetPasswordRealtimeRequest realtimeRequest = new UserResetPasswordRealtimeRequest(request.TargetUserId, request.NewPassword, request.ChangePassword);
                StaffChangePasswordRealtimeResponse changePasswordResponse = null;

                try
                {
                    changePasswordResponse = request.RequestContext.Execute<StaffChangePasswordRealtimeResponse>(realtimeRequest);
                    LogAuthenticationRequest(request.RequestContext, request.TargetUserId, AuthenticationStatus.Success, AuthenticationOperation.ResetPassword);
                }
                catch (SecurityException exception)
                {
                    if (SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordComplexityRequirementsNotMet.ToString().Equals(exception.ErrorResourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        LogAuthenticationRequest(request.RequestContext, request.TargetUserId, AuthenticationStatus.PasswordComplexityRequirementsNotMetFailure, AuthenticationOperation.ResetPassword);

                        // Throwing the inner exception (headquarter transaction excpetion) to be handled by the client.
                        throw exception.InnerException;
                    }
                    else if (SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordHistoryRequirementsNotMet.ToString().Equals(exception.ErrorResourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        LogAuthenticationRequest(request.RequestContext, request.TargetUserId, AuthenticationStatus.PasswordHistoryRequirementsNotMetFailure, AuthenticationOperation.ResetPassword);

                        // Throwing the inner exception (headquarter transaction excpetion) to be handled by the client.
                        throw exception.InnerException;
                    }

                    throw;
                }

                // updates password on local database to support local database logins
                UpdatePasswordLocalDatabase(
                  request.RequestContext,
                  request.TargetUserId,
                  changePasswordResponse.PasswordHash,
                  changePasswordResponse.PasswordSalt,
                  changePasswordResponse.PasswordHashAlgorithm,
                  changePasswordResponse.PasswordLastChangedDateTime,
                  changePasswordResponse.PasswordLastUpdatedOperation,
                  request.ChangePassword);

                return new NullResponse();
            }

            /// <summary>
            /// Change password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static NullResponse ChangePassword(UserChangePasswordServiceRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.StaffId))
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_ChangePasswordFailed, "Staff identifier must not be null or empty.");
                }

                VerifyUserLockoutPolicy(request.StaffId, request.RequestContext);

                ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                RequestContext contextForOperationAuthorization;
                if (!principal.IsAnonymous)
                {
                    // make sure user authenticated is the same as the one in the request
                    if (!string.Equals(request.StaffId, principal.UserId, StringComparison.OrdinalIgnoreCase))
                    {
                        RetailLogger.Log.CrtServicesStaffAuthenticationServiceAttemptToChangePasswordForDifferentUser(principal.UserId, request.StaffId);
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "User is not allowed to change the password of another user.");
                    }

                    contextForOperationAuthorization = request.RequestContext;
                }
                else
                {
                    // general authorization for user
                    StaffAuthorizationServiceRequest authorizationRequest = new StaffAuthorizationServiceRequest(request.StaffId, RetailOperation.None);
                    StaffAuthorizationServiceResponse authorizationResponse = request.RequestContext.Execute<StaffAuthorizationServiceResponse>(authorizationRequest);

                    // create an authenticated context that can be used for specific operation authorization
                    RequestContext authenticatedContext = new RequestContext(request.RequestContext.Runtime);
                    CommerceIdentity identity = new CommerceIdentity(
                        authorizationResponse.Employee,
                        new Device()
                        {
                            DeviceNumber = principal.DeviceNumber,
                            Token = principal.DeviceToken,
                            ChannelId = principal.ChannelId,
                            TerminalRecordId = principal.TerminalId
                        });
                    authenticatedContext.SetPrincipal(new CommercePrincipal(identity));

                    contextForOperationAuthorization = authenticatedContext;
                }

                // check permissions for operation
                contextForOperationAuthorization.Execute<NullResponse>(new CheckAccessServiceRequest(RetailOperation.ChangePassword));

                StaffChangePasswordRealtimeResponse changePasswordResponse;

                try
                {
                    // make RTS call using 'contextForOperationAuthorization' as it will perform operation checks before going to RTS
                    UserChangePasswordRealtimeRequest realtimeRequest = new UserChangePasswordRealtimeRequest(
                        request.StaffId,
                        request.OldPassword,
                        request.NewPassword);
                    changePasswordResponse = contextForOperationAuthorization.Execute<StaffChangePasswordRealtimeResponse>(realtimeRequest);
                    LogAuthenticationRequest(request.RequestContext, request.StaffId, AuthenticationStatus.Success, AuthenticationOperation.ChangePassword);
                }
                catch (UserAuthenticationException exception)
                {
                    if (SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword.ToString().Equals(exception.ErrorResourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        LogAuthenticationRequest(request.RequestContext, request.StaffId, AuthenticationStatus.InvalidPasswordFailure, AuthenticationOperation.ChangePassword);

                        // SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword is used internally to identify when user provides incorrect password
                        // here we make sure this is not surfaced outside of the runtime, so there is no information disclosure
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Incorrect user name or password.");
                    }

                    throw;
                }
                catch (SecurityException exception)
                {
                    if (SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordComplexityRequirementsNotMet.ToString().Equals(exception.ErrorResourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        LogAuthenticationRequest(request.RequestContext, request.StaffId, AuthenticationStatus.PasswordComplexityRequirementsNotMetFailure, AuthenticationOperation.ChangePassword);

                        // Throwing the inner exception (headquarter transaction excpetion) to be handled by the client.
                        throw exception.InnerException;
                    }
                    else if (SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_PasswordHistoryRequirementsNotMet.ToString().Equals(exception.ErrorResourceId, StringComparison.OrdinalIgnoreCase))
                    {
                        LogAuthenticationRequest(request.RequestContext, request.StaffId, AuthenticationStatus.PasswordHistoryRequirementsNotMetFailure, AuthenticationOperation.ChangePassword);

                        // Throwing the inner exception (headquarter transaction excpetion) to be handled by the client.
                        throw exception.InnerException;
                    }

                    throw;
                }

                // updates password on local database to support local database logins
                UpdatePasswordLocalDatabase(
                    request.RequestContext,
                    request.StaffId,
                    changePasswordResponse.PasswordHash,
                    changePasswordResponse.PasswordSalt,
                    changePasswordResponse.PasswordHashAlgorithm,
                    changePasswordResponse.PasswordLastChangedDateTime,
                    changePasswordResponse.PasswordLastUpdatedOperation,
                    changePasswordAtNextLogOn: false);

                return new NullResponse();
            }

            /// <summary>
            /// Verifies whether the employee password has expired.
            /// </summary>
            /// <param name="passwordExpiryIntervalInDays">The password expiry interval in days.</param>
            /// <param name="passwordLastChangedDateTime">The date time at which the password was last changed.</param>
            /// <returns><c>True</c> if the employee password has expired, <c>false</c> otherwise.</returns>
            private static bool IsEmployeePasswordExpired(int passwordExpiryIntervalInDays, DateTimeOffset passwordLastChangedDateTime)
            {
                return passwordExpiryIntervalInDays != 0 && passwordLastChangedDateTime.AddDays(passwordExpiryIntervalInDays) <= DateTime.UtcNow;
            }

            /// <summary>
            /// Unlock the register.
            /// </summary>
            /// <param name="request">The unlock register request.</param>
            /// <returns>The unlock register response.</returns>
            private static NullResponse UnlockRegister(UnlockRegisterServiceRequest request)
            {
                AuthenticateUserWithPassword(request.RequestContext, request.StaffId, request.Password);
                return new NullResponse();
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static UserLogOnRenewalServiceResponse LogOnUserRenewal(UserLogOnRenewalServiceRequest request)
            {
                Employee employee;

                try
                {
                    // Authenticate the user using real time service if device is not specified or if terminal configuration is set with RealTime Service.
                    if (request.RequestContext.GetPrincipal().LogOnConfiguration == LogOnConfiguration.RealTimeService)
                    {
                        UserLogOnRenewalRealtimeRequest realtimeRequest = new UserLogOnRenewalRealtimeRequest(request.Device, request.StaffId);
                        UserLogOnRenewalRealtimeResponse realtimeResponse = request.RequestContext.Execute<UserLogOnRenewalRealtimeResponse>(realtimeRequest);
                        employee = realtimeResponse.Employee;
                    }
                    else if (request.RequestContext.GetPrincipal().LogOnConfiguration == LogOnConfiguration.LocalDatabase)
                    {
                        EmployeeLogOnStoreDataRequest dataRequest = new EmployeeLogOnStoreDataRequest(request.Device.ChannelId, request.StaffId, null, new ColumnSet());
                        Employee localEmployee = request.RequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                        if (localEmployee == null)
                        {
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed);
                        }

                        // Return new object for the logged-on employee with only the required fields.
                        employee = new Employee();
                        employee.Name = localEmployee.Name;
                        employee.StaffId = localEmployee.StaffId;
                        employee.NameOnReceipt = localEmployee.NameOnReceipt;

                        GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(request.StaffId, new ColumnSet());
                        employee.Permissions = request.RequestContext.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(permissionsDataRequest).Entity;
                    }
                    else
                    {
                        Debug.Assert(false, "Invalid Login configuration");
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed);
                    }
                }
                catch (Exception ex)
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, ex, ex.Message);
                }

                return new UserLogOnRenewalServiceResponse(employee);
            }

            /// <summary>
            /// Gets the external identity.
            /// </summary>
            /// <param name="request">Get commerce identity by external identity service request.</param>
            /// <returns>The response containing the commerce identity.</returns>
            private static GetCommerceIdentityByExternalIdentityServiceResponse GetCommerceIdentityByExternalIdentity(GetCommerceIdentityByExternalIdentityServiceRequest request)
            {
                switch (request.CommerceIdentityType)
                {
                    case CommerceIdentityType.Employee:

                        // When there is no ExternalIdentitySubId we consider issuer to use ExternalIdentityId as StaffId
                        if (string.IsNullOrWhiteSpace(request.ExternalIdentitySubId))
                        {
                            CommerceIdentity employeeIdentity;
                            if (request.RequestContext.GetPrincipal().IsChannelAgnostic)
                            {
                                // If the channel is not present look up the employee object in AX.
                                StaffLogOnRealtimeRequest staffLogOnRealtime = new StaffLogOnRealtimeRequest(null, 0, request.ExternalIdentityId, null);
                                var response = request.RequestContext.Execute<StaffLogOnRealtimeResponse>(staffLogOnRealtime);
                                employeeIdentity = new CommerceIdentity(response.Employee, null);
                            }
                            else
                            {
                                var staffAuthorizationRequest = new GetEmployeeAuthorizedOnStoreDataRequest(request.ExternalIdentityId);
                                var response = request.RequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(staffAuthorizationRequest);
                                employeeIdentity = new CommerceIdentity(response.Entity, null);
                            }

                            return new GetCommerceIdentityByExternalIdentityServiceResponse(employeeIdentity);
                        }
                        else
                        {
                            var employeeIdentityRequest = new GetEmployeeIdentityByExternalIdentityRealtimeRequest(request.ExternalIdentityId, request.ExternalIdentitySubId);
                            var employeeIdentityResponse = request.RequestContext.Execute<GetEmployeeIdentityByExternalIdentityRealtimeResponse>(employeeIdentityRequest);
                            return new GetCommerceIdentityByExternalIdentityServiceResponse(employeeIdentityResponse.EmployeeIdentity);
                        }

                    case CommerceIdentityType.Customer:
                        var customerAccountRequest = new GetCustomerAccountByExternalIdentityDataRequest(request.ExternalIdentityId, request.ExternalIdentityIssuer);
                        var customerAccountResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<CustomerExternalIdentityMap>>(customerAccountRequest);

                        CommerceIdentity commerceIdentity = null;
                        if (customerAccountResponse != null && customerAccountResponse.Entity != null && !string.IsNullOrWhiteSpace(customerAccountResponse.Entity.CustomerAccountNumber))
                        {
                            commerceIdentity = new CommerceIdentity();
                            commerceIdentity.Roles.Add(CommerceRoles.Customer);
                            commerceIdentity.Roles.AddRange(customerAccountResponse.Entity.CustomerPermissions);
                            commerceIdentity.UserId = customerAccountResponse.Entity.CustomerAccountNumber;
                            commerceIdentity.IsActivated = customerAccountResponse.Entity.IsActivated;
                        }

                        return new GetCommerceIdentityByExternalIdentityServiceResponse(commerceIdentity);

                    default:
                        throw new NotSupportedException(string.Format("The provided commerce identity type '{0}' is not supported.", request.CommerceIdentityType));
                }
            }

            /// <summary>
            /// Check Access.
            /// </summary>
            /// <param name="request">The Check access request.</param>
            /// <returns>Service response.</returns>
            private static NullResponse CheckAccess(CheckAccessServiceRequest request)
            {
                CommerceAuthorization.CheckAccess(request.RequestContext.GetPrincipal(), request.RetailOperation, request.RequestContext, request.AllowedRoles, request.DeviceTokenRequired, request.NonDrawerOperationCheckRequired);
                return new NullResponse();
            }

            /// <summary>
            /// Check manager access.
            /// </summary>
            /// <param name="request">The Check access request.</param>
            /// <returns>Service response.</returns>
            private static NullResponse CheckManagerAccess(CheckAccessIsManagerServiceRequest request)
            {
                CommerceAuthorization.CheckAccessManager(request.RequestContext.GetPrincipal());
                return new NullResponse();
            }

            /// <summary>
            /// Check if principal has shift.
            /// </summary>
            /// <param name="request">The check has shift request.</param>
            /// <returns>Service response.</returns>
            private static NullResponse CheckHasShift(CheckAccessHasShiftServiceRequest request)
            {
                CommerceAuthorization.CheckAccessHasShift(request.RequestContext.GetPrincipal());
                return new NullResponse();
            }

            /// <summary>
            /// Check if principal has access to the cart.
            /// </summary>
            /// <param name="request">Check Access to cart request.</param>
            /// <returns>Service response.</returns>
            private static NullResponse CheckAccessToCart(CheckAccessToCartServiceRequest request)
            {
                CommerceAuthorization.CheckAccessToCarts(request.RequestContext.GetPrincipal(), request.Transactions);
                return new NullResponse();
            }

            /// <summary>
            /// Check if principal has access to the customer Account.
            /// </summary>
            /// <param name="request">Check Access to customer Account.</param>
            /// <returns>Service response.</returns>
            private static NullResponse CheckAccessToCustomerAccount(CheckAccessToCustomerAccountServiceRequest request)
            {
                CommerceAuthorization.CheckAccessToCustomerAccount(request.RequestContext.GetPrincipal(), request.CustomerAccountNumber);
                return new NullResponse();
            }

            /// <summary>
            /// Gets the transaction service profile.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <returns>The transaction service profile.</returns>
            private static TransactionServiceProfile GetTransactionServiceProfile(RequestContext context)
            {
                var getTransactionServiceProfileDataRequest = new GetTransactionServiceProfileDataRequest();
                TransactionServiceProfile transactionServiceProfile = context.Runtime.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(getTransactionServiceProfileDataRequest, context).Entity;
                return transactionServiceProfile;
            }

            /// <summary>
            /// Updates the employee password on the local database.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="staffId">The employee staff identifier.</param>
            /// <param name="newPasswordHash">The new password hash.</param>
            /// <param name="newPasswordSalt">The new password salt.</param>
            /// <param name="newPasswordHashAlgorithm">The new password hash algorithm.</param>
            /// <param name="newPasswordLastChangedDateTime">The new  UTC date and time at which the password was changed.</param>
            /// <param name="newPasswordLastUpdatedOperation">The authentication operation for the last password update.</param>
            /// <param name="changePasswordAtNextLogOn">Change Password at next logon.</param>
            private static void UpdatePasswordLocalDatabase(RequestContext context, string staffId, string newPasswordHash, string newPasswordSalt, string newPasswordHashAlgorithm, DateTimeOffset newPasswordLastChangedDateTime, AuthenticationOperation newPasswordLastUpdatedOperation, bool changePasswordAtNextLogOn)
            {
                UpdateEmployeePasswordDataRequest updatePasswordRequest = new UpdateEmployeePasswordDataRequest(
                    staffId,
                    newPasswordHash,
                    newPasswordSalt,
                    newPasswordHashAlgorithm,
                    newPasswordLastChangedDateTime,
                    newPasswordLastUpdatedOperation,
                    changePasswordAtNextLogOn);

                try
                {
                    context.Execute<NullResponse>(updatePasswordRequest);
                }
                catch (StorageException exception)
                {
                    RetailLogger.Instance.CrtServicesUserAuthenticationServicePasswordChangeFailedOnLocalDatabase(staffId, exception);

                    // if channel is configured to logon against local database, then it means that the user will not be able to login
                    // we need to return a failure message
                    // for any other configuration, we can ignore this error, given that is logged above, as it is not blocking
                    if (context.GetChannelConfiguration().TransactionServiceProfile.StaffLogOnConfiguration == LogOnConfiguration.LocalDatabase)
                    {
                        throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UpdatePasswordFailed, "Updating the password failed. Password reset might be necessary.");
                    }
                }
            }

            /// <summary>
            /// Verifies the user account lockout policy.
            /// </summary>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="context">The request context.</param>
            private static void VerifyUserLockoutPolicy(string staffId, RequestContext context)
            {
                // Check user account policy.
                VerifyUserLockoutPolicyDataRequest verifyUserLockoutPolicyDataRequest = new VerifyUserLockoutPolicyDataRequest(staffId);
                int timeToAccountUnlockInSeconds = context.Execute<SingleEntityDataServiceResponse<int>>(verifyUserLockoutPolicyDataRequest).Entity;

                if (timeToAccountUnlockInSeconds != 0)
                {
                    throw new UserAuthenticationException(
                        SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserBlockedDueToTooManyFailedLogonAttempts,
                        string.Format("Too many failed logon attempts. User will be blocked for {0} seconds.", timeToAccountUnlockInSeconds));
                }
            }

            /// <summary>
            /// Logs an authentication attempt.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="authenticationStatus">The authentication status.</param>
            /// <param name="authenticationOperation">The authentication operation.</param>
            private static void LogAuthenticationRequest(RequestContext context, string staffId, AuthenticationStatus authenticationStatus, AuthenticationOperation authenticationOperation)
            {
                LogAuthenticationDataRequest dataRequest = new LogAuthenticationDataRequest(context.GetPrincipal().ChannelId, staffId, authenticationStatus, authenticationOperation);
                context.Runtime.Execute<NullResponse>(dataRequest, context);
            }

            /// <summary>
            /// Hashes the password.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="password">The password.</param>
            /// <param name="algorithm">The algorithm.</param>
            /// <param name="invariant">The invariant.</param>
            /// <param name="salt">Salt for password.</param>
            /// <returns>The hash of the password.</returns>
            private static string HashPassword(RequestContext context, string password, string algorithm, string invariant, string salt)
            {
                HashDataServiceRequest hashDataServiceRequest = new HashDataServiceRequest(password, algorithm, invariant, salt);

                string passwordHash = context.Execute<HashDataServiceResponse>(hashDataServiceRequest).Data;

                return passwordHash;
            }
        }
    }
}