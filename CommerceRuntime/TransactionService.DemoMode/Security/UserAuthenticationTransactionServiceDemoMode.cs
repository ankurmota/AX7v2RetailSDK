/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "This file requires multiple namespaces to support the Retail Sdk code generation.")]

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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// User authentication Service.
        /// </summary>
        public class UserAuthenticationTransactionServiceDemoMode : IRequestHandler
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
                    typeof(EnrollUserCredentialsRealtimeRequest),
                    typeof(UnenrollUserCredentialsRealtimeRequest),
                };
                }
            }

            /// <summary>
            /// Executes the specified request.
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
                else if (requestType == typeof(EnrollUserCredentialsRealtimeRequest))
                {
                    throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported);
                }
                else if (requestType == typeof(UnenrollUserCredentialsRealtimeRequest))
                {
                    throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Reset password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static Response ResetPassword(UserResetPasswordRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
                return new NullResponse();
            }

            /// <summary>
            /// Change password for the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static Response ChangePassword(UserChangePasswordRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");

                long channelId = request.RequestContext.GetPrincipal().ChannelId;
                string staffId = request.StaffId;

                // Get employee salt and password hash algorithm.
                GetEmployeePasswordCryptoInfoDataRequest employeePasswordCryptoInfoRequest = new GetEmployeePasswordCryptoInfoDataRequest(channelId, staffId);
                var passwordCryptoInfo = request.RequestContext.Execute<SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo>>(employeePasswordCryptoInfoRequest).Entity;
                string salt = passwordCryptoInfo.PasswordSalt ?? string.Empty;
                string passwordHashAlgorithm = passwordCryptoInfo.PasswordHashAlgorithm;

                if (string.IsNullOrEmpty(passwordHashAlgorithm))
                {
                    // get hash algorithm from the transaction service profile.
                    var getTransactionServiceProfileDataRequest = new GetTransactionServiceProfileDataRequest();
                    TransactionServiceProfile transactionServiceProfile = request.RequestContext.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(
                        getTransactionServiceProfileDataRequest).Entity;
                    passwordHashAlgorithm = transactionServiceProfile.StaffPasswordHash;
                }

                // hash old password
                HashDataServiceRequest hashDataServiceRequest = new HashDataServiceRequest(request.OldPassword, passwordHashAlgorithm, request.StaffId, salt);
                string oldPasswordHash = request.RequestContext.Execute<HashDataServiceResponse>(hashDataServiceRequest).Data;

                // check if old password is correct
                EmployeeLogOnStoreDataRequest employeeDataRequest = new EmployeeLogOnStoreDataRequest(
                    channelId,
                    staffId,
                    oldPasswordHash,
                    new ColumnSet());
                if (request.RequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(employeeDataRequest).Entity == null)
                {
                    // SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidPassword is used internally to identify when user provides incorrect password
                    // here we make sure this is not surfaced outside of the runtime, so there is no information disclosure
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidAuthenticationCredentials, "Incorrect user name or password.");
                }

                // hash new password
                hashDataServiceRequest = new HashDataServiceRequest(request.NewPassword, passwordHashAlgorithm, request.StaffId, salt);
                string newPasswordHash = request.RequestContext.Execute<HashDataServiceResponse>(hashDataServiceRequest).Data;

                // return new password hash + salt
                return new StaffChangePasswordRealtimeResponse(newPasswordHash, salt, passwordHashAlgorithm, DateTime.UtcNow, AuthenticationOperation.ChangePassword);
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static StaffLogOnRealtimeResponse LogOnUser(StaffLogOnRealtimeRequest request)
            {
                // Mimic the real time TS logon
                Employee employee = EmployeeLogOnStore(request, request.RequestContext.GetChannelConfiguration());
                return new StaffLogOnRealtimeResponse(employee);
            }

            /// <summary>
            /// LogOff the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static NullResponse LogOffUser(UserLogOffRealtimeRequest request)
            {
                UserAuthenticationTransactionServiceDemoMode.ThrowIfInvalidLogonConfiguration(request.LogOnConfiguration);

                ICommercePrincipal principal = request.RequestContext.GetPrincipal();

                if (string.IsNullOrEmpty(principal.UserId))
                {
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "User is not authenticated.");
                }

                string staffId = principal.UserId;

                if (!principal.IsTerminalAgnostic)
                {
                    string terminalId = request.RequestContext.GetTerminal().TerminalId;
                    UnlockUserAtLogOffDataRequest unlockUserDataRequest = new UnlockUserAtLogOffDataRequest(
                        principal.ChannelId,
                        terminalId,
                        staffId,
                        request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);

                    bool unlocked = request.RequestContext.Execute<SingleEntityDataServiceResponse<bool>>(unlockUserDataRequest).Entity;
                    if (!unlocked)
                    {
                        throw new UserAuthenticationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed,
                            string.Format("User {0} was not allowed to be unlocked from terminal {1}", principal.UserId, terminalId));
                    }
                }

                return new NullResponse();
            }

            /// <summary>
            /// Authenticate the user.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <returns>The device activation response.</returns>
            private static UserLogOnRenewalRealtimeResponse LogOnUserRenewal(UserLogOnRenewalRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "LogOnUserRenewal is not supported in demo mode.");
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

            /// <summary>
            /// Logon the user on local store.
            /// </summary>
            /// <param name="request">The device activation request.</param>
            /// <param name="channelConfiguration">The channel configuration.</param>
            /// <returns>The device activation response.</returns>
            private static Employee EmployeeLogOnStore(StaffLogOnRealtimeRequest request, ChannelConfiguration channelConfiguration)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Employee employee = null, localEmployee = null;
                string passwordHash = string.Empty;
                string staffId = request.StaffId;

                if (request.ChannelId.HasValue && request.RequestContext.GetPrincipal().ChannelId == request.ChannelId.Value)
                {
                    // Get employee salt and password hash algorithm.
                    GetEmployeePasswordCryptoInfoDataRequest employeePasswordCryptoInfoRequest = new GetEmployeePasswordCryptoInfoDataRequest(request.ChannelId.GetValueOrDefault(), staffId);
                    var passwordCryptoInfo = request.RequestContext.Execute<SingleEntityDataServiceResponse<EmployeePasswordCryptoInfo>>(employeePasswordCryptoInfoRequest).Entity;
                    string salt = passwordCryptoInfo.PasswordSalt ?? string.Empty;
                    string passwordHashAlgorithm = passwordCryptoInfo.PasswordHashAlgorithm;

                    if (!string.IsNullOrEmpty(request.StaffPassword))
                    {
                        if (string.IsNullOrEmpty(passwordHashAlgorithm))
                        {
                            // get hash algorithm from the transaction service profile.
                            var getTransactionServiceProfileDataRequest = new GetTransactionServiceProfileDataRequest();
                            TransactionServiceProfile transactionServiceProfile = request.RequestContext.Execute<SingleEntityDataServiceResponse<TransactionServiceProfile>>(
                                getTransactionServiceProfileDataRequest).Entity;
                            passwordHashAlgorithm = transactionServiceProfile.StaffPasswordHash;
                        }

                        HashDataServiceRequest hashDataServiceRequest = new HashDataServiceRequest(request.StaffPassword, passwordHashAlgorithm, staffId, salt);
                        passwordHash = request.RequestContext.Execute<HashDataServiceResponse>(hashDataServiceRequest).Data;
                    }

                    // Logon to the store
                    EmployeeLogOnStoreDataRequest dataRequest = new EmployeeLogOnStoreDataRequest(
                        request.ChannelId.Value,
                        staffId,
                        passwordHash,
                        new ColumnSet());

                    localEmployee = request.RequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                    if (localEmployee == null || localEmployee.Permissions == null)
                    {
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed);
                    }

                    // Return new object for the logged-on employee with only the required fields to ensure not returning any sensitive data.
                    employee = new Employee();
                    employee.Permissions = new EmployeePermissions();
                    employee.Name = localEmployee.Name;
                    employee.StaffId = localEmployee.StaffId;
                    employee.NameOnReceipt = localEmployee.NameOnReceipt;
                    employee.Permissions = localEmployee.Permissions;
                    employee.Images = localEmployee.Images;
                    employee.PasswordLastChangedDateTime = localEmployee.PasswordLastChangedDateTime;

                    // Lock the user if multiple logins are not allowed.
                    if (!employee.Permissions.AllowMultipleLogins && channelConfiguration != null)
                    {
                        LockUserAtLogOnDataRequest lockDataRequest = new LockUserAtLogOnDataRequest(
                            request.ChannelId.Value,
                            request.RequestContext.GetTerminal().TerminalId,
                            staffId,
                            channelConfiguration.InventLocationDataAreaId);
                        bool locked = request.RequestContext.Execute<SingleEntityDataServiceResponse<bool>>(lockDataRequest).Entity;
                        if (!locked)
                        {
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserLogonAnotherTerminal);
                        }
                    }
                }
                else
                {
                    RequestContext getEmployeeRequestContext;

                    // Employee record is associated with channel
                    // Decide what context is to be used to retrieve it            
                    if (request.RequestContext.GetPrincipal().IsChannelAgnostic)
                    {
                        // If the context is channel agnostic (no channel information), then use current (first published channel in this case)
                        GetChannelIdServiceRequest getChannelRequest = new GetChannelIdServiceRequest();
                        GetChannelIdServiceResponse getChannelResponse = request.RequestContext.Execute<GetChannelIdServiceResponse>(getChannelRequest);

                        var anonymousIdentity = new CommerceIdentity()
                        {
                            ChannelId = getChannelResponse.ChannelId
                        };

                        getEmployeeRequestContext = new RequestContext(request.RequestContext.Runtime);
                        getEmployeeRequestContext.SetPrincipal(new CommercePrincipal(anonymousIdentity));
                    }
                    else
                    {
                        // If the request has channel information, then use current context
                        getEmployeeRequestContext = request.RequestContext;
                    }

                    GetEmployeeDataRequest dataRequest = new GetEmployeeDataRequest(staffId, QueryResultSettings.SingleRecord);
                    employee = getEmployeeRequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                    if (employee == null)
                    {
                        string message = string.Format(CultureInfo.InvariantCulture, "The specified employee ({0}) was not found.", staffId);
                        throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, message);
                    }

                    ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                    RequestContext contextForOperationAuthorization;
                    if (principal.IsAnonymous)
                    {
                        // create an authenticated context that can be used for specific operation authorization
                        RequestContext authenticatedContext = new RequestContext(request.RequestContext.Runtime);
                        CommerceIdentity identity = new CommerceIdentity(
                            employee,
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
                    else
                    {
                        contextForOperationAuthorization = request.RequestContext;
                    }

                    GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(staffId, new ColumnSet());
                    employee.Permissions = contextForOperationAuthorization.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(permissionsDataRequest).Entity;
                }

                return employee;
            }

            /// <summary>
            /// Validates the staff password.
            /// </summary>
            /// <param name="request">The validate staff request.</param>
            /// <returns>The validate staff response.</returns>
            private static Response ValidateStaffPassword(ValidateStaffPasswordRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");
                throw new FeatureNotSupportedException(FeatureNotSupportedErrors.Microsoft_Dynamics_Commerce_Runtime_DemoModeOperationNotSupported, "ValidateStaffPassword is not supported in demo mode.");
            }
        }
    }
}
