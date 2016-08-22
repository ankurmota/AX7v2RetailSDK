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
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Authentication logic helper.
        /// </summary>
        internal static class AuthenticationHelper
        {
            internal const string ManagerPrivilegies = "MANAGERPRIVILEGES";

            /// <summary>
            /// Makes a TS call to do device authentication.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="deviceToken">The device token.</param>
            /// <returns>Authenticated device.</returns>
            internal static Device AuthenticateDevice(RequestContext context, string deviceToken)
            {
                var serviceRequest = new AuthenticateDeviceServiceRequest(deviceToken);
                AuthenticateDeviceServiceResponse serviceResponse = context.Execute<AuthenticateDeviceServiceResponse>(serviceRequest);
                return serviceResponse.Device;
            }

            /// <summary>
            /// Authenticates and authorizes the user.
            /// </summary>            
            /// <param name="request">UserAuthenticationRequest request object.</param>
            /// <param name="device">The device.</param>
            /// <returns>Employee object.</returns>
            internal static Employee AuthenticateAndAuthorizeUser(UserAuthenticationRequest request, Device device)
            {
                // Authenticate
                Employee employee;
                string staffId; // in extended authentication cases, this value might be empty
                RequestContext executionContext = request.RequestContext;

                if (device != null && request.RequestContext.GetPrincipal().IsChannelAgnostic)
                {
                    executionContext = CreateRequestContext(string.Empty, device, request.RequestContext.Runtime);
                }

                UserLogOnServiceRequest authenticateServiceRequest = new UserLogOnServiceRequest(
                        request.StaffId,
                        request.Password,
                        request.Credential,
                        request.GrantType,
                        request.AdditionalAuthenticationData);
                UserLogOnServiceResponse response = executionContext.Execute<UserLogOnServiceResponse>(authenticateServiceRequest);
                staffId = response.StaffId;

                // Authorize only if this was for elevate user
                if (request.RetailOperation != RetailOperation.None)
                {
                    var authorizeStaffRequest = new StaffAuthorizationServiceRequest(
                        request.RequestContext.GetPrincipal().UserId,
                        request.RetailOperation);

                    bool currentUserAlreadyAuthorizedForOperation = true;

                    try
                    {
                        executionContext.Execute<StaffAuthorizationServiceResponse>(authorizeStaffRequest);
                    }
                    catch (UserAuthorizationException authorizationException)
                    {
                        if (string.Equals(authorizationException.ErrorResourceId, SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed.ToString(), StringComparison.Ordinal))
                        {
                            currentUserAlreadyAuthorizedForOperation = false;
                        }
                    }

                    if (currentUserAlreadyAuthorizedForOperation)
                    {
                        RetailLogger.Log.CrtWorkflowUserAuthenticationUserAlreadyHasAccessToTheTargetedOperation(request.RequestContext.GetPrincipal().UserId, request.RetailOperation.ToString());
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "The current user is already authorized to perform the targeted operation.");
                    }

                    authorizeStaffRequest = new StaffAuthorizationServiceRequest(
                        staffId,
                        request.RetailOperation);

                    StaffAuthorizationServiceResponse authorizationResponse =
                        executionContext.Execute<StaffAuthorizationServiceResponse>(authorizeStaffRequest);
                    employee = authorizationResponse.Employee;
                }
                else
                {
                    // The employee has already been authenticated above.
                    employee = new Employee() { StaffId = staffId };

                    // if we created a new context, then we need to update it here with the staff id
                    executionContext = CreateRequestContext(staffId, device, request.RequestContext.Runtime);

                    // This is needed for offline scenarios/logon because as opposed to online/RS in CRT there is no user look up when the identity is presented on API calls.
                    GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(staffId, new ColumnSet());
                    employee.Permissions = executionContext.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(permissionsDataRequest).Entity;
                }
   
                LogAuthenticationRequest(executionContext, employee.StaffId, AuthenticationStatus.Success, AuthenticationOperation.CreateToken);
    
                return employee;
            }

            /// <summary>
            /// User authentication renewal.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="device">The device object.</param>
            /// <returns>Employee object.</returns>
            internal static Employee AuthenticateRenewalUser(RequestContext context, Device device)
            {
                var serviceRequest = new UserLogOnRenewalServiceRequest(device, context.GetPrincipal().UserId);
                UserLogOnRenewalServiceResponse serviceResponse = context.Execute<UserLogOnRenewalServiceResponse>(serviceRequest);
                return serviceResponse.Employee;
            }

            /// <summary>
            /// Performs a user logs off.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="request">The request object.</param>
            internal static void LogOff(RequestContext context, UserLogOffRequest request)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(request, "request");

                var serviceRequest = new UserLogOffServiceRequest(request.LogOnConfiguration);
                context.Execute<Response>(serviceRequest);
            }

            /// <summary>
            /// Performs unlock register.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="device">The device object.</param>
            /// <param name="request">The request object.</param>
            internal static void UnlockRegister(RequestContext context, Device device, UnlockRegisterRequest request)
            {
                var serviceRequest = new UnlockRegisterServiceRequest(device, request.RetailOperation, request.StaffId, request.Password);
                context.Execute<Response>(serviceRequest);
            }

            /// <summary>
            /// Logs a retail transaction.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="transactionType">The transaction type.</param>
            /// <param name="transactionId">The transaction identifier.</param>
            internal static void LogTransaction(RequestContext context, TransactionType transactionType, string transactionId)
            {
                var serviceRequest = new SaveTransactionLogServiceRequest(transactionType, transactionId);
                context.Execute<Response>(serviceRequest);

                if (transactionType == TransactionType.LogOn)
                {
                    string auditMessage = string.Format("User has successfully logged on. OperatorID: {0}; DeviceNumber: {1}", context.GetPrincipal().UserId, context.GetPrincipal().DeviceNumber);
                    LogAuditEntry(context, "AuthenticationHelper.LogTransaction", auditMessage);
                }
            }

            /// <summary>
            /// Logs an authentication request.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="logOnStatus">The log status.</param>
            /// <param name="authenticationOperation">The authentication operation.</param>
            internal static void LogAuthenticationRequest(RequestContext context, string staffId, AuthenticationStatus logOnStatus, AuthenticationOperation authenticationOperation)
            {
                LogAuthenticationDataRequest dataRequest = new LogAuthenticationDataRequest(context.GetPrincipal().ChannelId, staffId, logOnStatus, authenticationOperation);
                context.Runtime.Execute<NullResponse>(dataRequest, context);
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

            private static RequestContext CreateRequestContext(string staffId, Device device, ICommerceRuntime runtime)
            {
                CommerceIdentity identity;
                if (string.IsNullOrWhiteSpace(staffId))
                {
                    identity = new CommerceIdentity(device);
                    identity.Roles.Add(CommerceRoles.Anonymous);
                }
                else
                {
                    var employee = new Employee() { StaffId = staffId };
                    identity = new CommerceIdentity(employee, device);
                }
                
                CommercePrincipal principal = new CommercePrincipal(identity);
                RequestContext context = new RequestContext(runtime);
                context.SetPrincipal(principal);

                return context;
            }
        }
    }
}
