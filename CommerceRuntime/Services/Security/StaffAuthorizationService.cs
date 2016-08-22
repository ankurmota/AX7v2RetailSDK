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
        using Commerce.Runtime.Services.Security;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Authorization service class for retail staff.
        /// </summary>
        public class StaffAuthorizationService : IRequestHandler
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
                    typeof(StaffAuthorizationServiceRequest),
                    typeof(CreateStaffSessionServiceRequest)
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
                ThrowIf.Null(request, "request");

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(StaffAuthorizationServiceRequest))
                {
                    response = AuthorizeStaff((StaffAuthorizationServiceRequest)request);
                }
                else if (requestType == typeof(CreateStaffSessionServiceRequest))
                {
                    CreateStaffSession(request.RequestContext);
                    response = new NullResponse();
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request '{0}' is not supported.", requestType));
                }

                return response;
            }

            /// <summary>
            /// Authorizes the retail staff.
            /// </summary>
            /// <param name="staffAuthorizationRequest">The retail staff authorization request.</param>
            /// <returns>The service response.</returns>
            private static Response AuthorizeStaff(StaffAuthorizationServiceRequest staffAuthorizationRequest)
            {
                RequestContext context = staffAuthorizationRequest.RequestContext;
                ICommercePrincipal principal = context.GetPrincipal();
                string staffId = string.IsNullOrWhiteSpace(staffAuthorizationRequest.StaffId) ? principal.UserId : staffAuthorizationRequest.StaffId;
                long? channelId = principal.IsChannelAgnostic ? null : (long?)principal.ChannelId;
                long? terminalRecordId = principal.IsTerminalAgnostic ? null : (long?)principal.TerminalId;
                RetailOperation operation = staffAuthorizationRequest.RetailOperation;
                Employee employee;

                if (string.IsNullOrWhiteSpace(staffId))
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "UserId is missing from principal.");
                }

                if (channelId.HasValue && !principal.IsTerminalAgnostic && !terminalRecordId.HasValue)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "When channel identififer is provided on principal, terminal record identfier must also be.");
                }

                StaffRealTimeSecurityValidationHelper staffSecurityHelper = StaffRealTimeSecurityValidationHelper.Create(
                    context,
                    SecurityVerificationType.Authorization,
                    staffId,
                    channelId,
                    terminalRecordId,
                    password: string.Empty);

                // we can only check values against database if the principal is bound to a channel
                if (!principal.IsChannelAgnostic)
                {
                    VerifyThatOrgUnitIsPublished(context, channelId.Value);
                }

                // for authorization, we always want to go to local DB, we only go to headquarters if we don't have a channel
                // to access the DB
                LogOnConfiguration logOnConfiguration = principal.IsChannelAgnostic
                    ? LogOnConfiguration.RealTimeService
                    : LogOnConfiguration.LocalDatabase;

                // authorize employee based on configuration
                employee = ExecuteWorkWithLocalFallback(
                    logOnConfiguration,
                    staffSecurityHelper,
                    () =>
                    {
                        return AuthorizeEmployeeLocalDatabase(context, staffSecurityHelper.StaffId, staffAuthorizationRequest.EnforceSessionToBeOpened);
                    });

                // Validates whether the staff can perform the requested operation
                ValidateEmployeePermissionForOperation(context, operation, employee);

                return new StaffAuthorizationServiceResponse(employee);
            }

            /// <summary>
            /// Verifies that the organization unit is published.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="channelId">The channel identifier.</param>
            private static void VerifyThatOrgUnitIsPublished(RequestContext context, long channelId)
            {
                // retrieve organization unit (store) information
                OrgUnit orgUnit;
                SearchOrgUnitDataRequest getStoreDataRequest = new SearchOrgUnitDataRequest(channelId);
                orgUnit = context.Execute<EntityDataServiceResponse<OrgUnit>>(getStoreDataRequest).PagedEntityCollection.SingleOrDefault();

                // make sure store is published before moving further
                if (orgUnit == null || !orgUnit.IsPublished)
                {
                    string errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "The channel {0} does not exist or was not published.",
                        channelId);
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannel, errorMessage);
                }
            }

            /// <summary>
            /// Executes work based against RTS or Local Database based on <paramref name="authorizationConfiguration"/>.
            /// If <paramref name="authorizationConfiguration"/> is <see cref="LogOnConfiguration.RealTimeService"/> then
            /// business logic is performed using RealTime service. If RealTime is not available and <see cref="Employee"/> is allowed to fallback to local database checks, then
            /// the delegate <paramref name="localDatabaseAction"/> will be executed. In case <paramref name="authorizationConfiguration"/> is <see cref="LogOnConfiguration.LocalDatabase"/>,
            /// then authorization is performed solely using local database.
            /// </summary>
            /// <param name="authorizationConfiguration">Indicates whether the logic should run, against local database or against the real time service.</param>
            /// <param name="staffSecurityHelper">The staff security validation helper instance.</param>
            /// <param name="localDatabaseAction">A delegate for the local execution of the authorization logic.</param>
            /// <returns>The employee.</returns>
            private static Employee ExecuteWorkWithLocalFallback(LogOnConfiguration authorizationConfiguration, StaffRealTimeSecurityValidationHelper staffSecurityHelper, Func<Employee> localDatabaseAction)
            {
                Employee employee = null;
                string errorMessage;

                switch (authorizationConfiguration)
                {
                    case LogOnConfiguration.RealTimeService:
                        try
                        {
                            employee = staffSecurityHelper.VerifyEmployeeRealTimeService(() =>
                            {
                                return localDatabaseAction();
                            });
                        }
                        catch (HeadquarterTransactionServiceException exception)
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterTransactionServiceMethodCallFailure, exception, exception.Message)
                            {
                                LocalizedMessage = exception.LocalizedMessage
                            };
                        }
                        catch (CommerceException exception)
                        {
                            // The error code to be persisted
                            throw new UserAuthorizationException(
                                SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_HeadquarterCommunicationFailure,
                                exception,
                                exception.Message);
                        }
                        catch (Exception exception)
                        {
                            // any exceptions that might happen will cause the authorization to fail
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed,
                                exception,
                                exception.Message);
                        }

                        break;

                    case LogOnConfiguration.LocalDatabase:
                        employee = localDatabaseAction();
                        break;

                    default:
                        errorMessage = string.Format(
                            CultureInfo.InvariantCulture,
                            "The authorization configuration value '{0}' is not supported.",
                            authorizationConfiguration);
                        throw new NotSupportedException(errorMessage);
                }

                return employee;
            }

            /// <summary>
            /// Authorizes the staff using the local database.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="staffId">The staff identifier to be verified.</param>
            /// <param name="enforceSessionOpened">A value indicating whether the authorization service will verify and fail if the user does not have a session opened.</param>
            /// <returns>The employee object.</returns>
            private static Employee AuthorizeEmployeeLocalDatabase(RequestContext context, string staffId, bool enforceSessionOpened)
            {
                if (context.GetPrincipal().IsChannelAgnostic)
                {
                    throw new InvalidOperationException("Local authorization can only be performed if principal is not channel agnostic.");
                }

                // Check if user is blocked
                GetEmployeeAuthorizedOnStoreDataRequest employeeDataRequest = new GetEmployeeAuthorizedOnStoreDataRequest(staffId);
                Employee employee = context.Execute<SingleEntityDataServiceResponse<Employee>>(employeeDataRequest).Entity;

                if (employee == null || employee.Permissions == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_LocalLogonFailed, "User could not be authorized. The user is blocked, not exists, or permission is not found.");
                }

                if (employee.IsBlocked)
                {
                    RetailLogger.Log.CrtServicesStaffAuthorizationServiceBlockedUserAccessAttempt(employee.StaffId);
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "The user is not authorized.");
                }

                bool isRequestWithElevatedPrivileges = context.GetPrincipal().ElevatedOperation != (int)RetailOperation.None;

                // session only needs to be enforces if requested by caller (enforceSessionOpened) and request is bound to a terminal
                // and it is not a manager override
                // also, session requirement does not apply for managers
                if (!employee.Permissions.HasManagerPrivileges && enforceSessionOpened && !context.GetPrincipal().IsTerminalAgnostic && !isRequestWithElevatedPrivileges)
                {
                    // validates that employee has open session
                    CheckEmployeeHasOpenSessionDataRequest checkEmployeeSessionRequest = new CheckEmployeeHasOpenSessionDataRequest();
                    bool isStaffSessionOpenOnTerminal = context.Execute<SingleEntityDataServiceResponse<bool>>(checkEmployeeSessionRequest).Entity;

                    if (!isStaffSessionOpenOnTerminal)
                    {
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserSessionNotOpened, "User must open a session before executing this request.");
                    }
                }

                return employee;
            }

            /// <summary>
            /// Validates whether the employee has enough permission to execute the requested operation.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="retailOperation">The retail operation.</param>
            /// <param name="employee">The employee.</param>
            private static void ValidateEmployeePermissionForOperation(RequestContext context, RetailOperation retailOperation, Employee employee)
            {
                // If the request is for logon with specific operation, check if the employee can execute requested operation. 
                if (retailOperation != RetailOperation.None && !employee.Permissions.HasManagerPrivileges)
                {
                    if (context.GetPrincipal().IsChannelAgnostic)
                    {
                        throw new UserAuthorizationException(
                            SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed,
                            "Operation are only allowed when CommercePrincipal has a channel defined.");
                    }

                    GetOperationPermissionsDataRequest dataRequest = new GetOperationPermissionsDataRequest(
                        retailOperation,
                        QueryResultSettings.SingleRecord);

                    OperationPermission operationPermission = context.Execute<EntityDataServiceResponse<OperationPermission>>(dataRequest)
                        .PagedEntityCollection.FirstOrDefault();

                    if (operationPermission == null)
                    {
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "The user does not have permissions to execute the operation requested.");
                    }

                    // Check if principal has all required permissions for the Operation or have manager privilege.
                    foreach (string permission in operationPermission.Permissions)
                    {
                        if (!employee.Permissions.Roles.Contains(permission) && !employee.Permissions.HasManagerPrivileges)
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "The user does not have permissions to execute the operation requested.");
                        }
                    }
                }
            }

            /// <summary>
            /// Associates the current employee and terminal in <paramref name="context"/>.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            /// <remarks>If employee cannot hold multiple sessions on different terminals <see cref="UserAuthorizationException"/> is thrown.</remarks>
            private static void CreateStaffSession(RequestContext requestContext)
            {
                ICommercePrincipal principal = requestContext.GetPrincipal();
                string staffId = principal.UserId;

                // when no terminal is present, there is no need to enforce single terminal use
                if (principal.IsTerminalAgnostic)
                {
                    throw new InvalidOperationException("A new session can only be created when channel and terminal are present in the request context.");
                }

                if (string.IsNullOrWhiteSpace(staffId))
                {
                    throw new InvalidOperationException("AuthorizeMultipleTerminalUse can only be performed if user is known. Missing UserId from CommercePrincipal.");
                }

                if (!principal.IsEmployee)
                {
                    throw new InvalidOperationException("AuthorizeMultipleTerminalUse can only be performed if user is employee.");
                }

                GetEmployeeAuthorizedOnStoreDataRequest employeeDataRequest = new GetEmployeeAuthorizedOnStoreDataRequest(staffId);
                Employee employee = requestContext.Execute<SingleEntityDataServiceResponse<Employee>>(employeeDataRequest).Entity;

                // managers are not required to keep track of sessions
                if (employee.Permissions.HasManagerPrivileges)
                {
                    return;
                }

                // helper does most of the hard lifting regarding fallback logic and real time communication
                StaffRealTimeSecurityValidationHelper staffSecurityHelper = StaffRealTimeSecurityValidationHelper.Create(
                    requestContext,
                    SecurityVerificationType.Authorization,
                    staffId,
                    principal.ChannelId,
                    principal.TerminalId,
                    password: string.Empty);

                // executes the common set of steps that performs the session creation flow in either HQ, local DB, or HQ with local DB fallback
                ExecuteWorkWithLocalFallback(
                    staffSecurityHelper.GetSecurityVerificationConfiguration(),
                    staffSecurityHelper,
                    () =>
                    {
                        return CreateEmployeeSessionLocalDatabase(requestContext, employee);
                    });

                // we always need to create the session on local DB so we can enforce session is open,
                // so if the configuration is set to use RealTime service, also creates the session on local DB
                if (staffSecurityHelper.GetSecurityVerificationConfiguration() == LogOnConfiguration.RealTimeService)
                {
                    CreateEmployeeSessionLocalDatabase(requestContext, employee);
                }

                RetailLogger.Log.CrtServicesStaffAuthorizationServiceUserSessionStarted(staffId, requestContext.GetTerminal().TerminalId);
            }

            /// <summary>
            /// Associates the employee <paramref name="staffId"/> with the current terminal in <paramref name="context"/> on the local database.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="employee">The employee.</param>
            /// <returns>The employee object associated to <paramref name="staffId"/>.</returns>
            /// <remarks>If employee cannot hold multiple sessions on different terminals <see cref="UserAuthorizationException"/> is thrown.</remarks>
            private static Employee CreateEmployeeSessionLocalDatabase(RequestContext context, Employee employee)
            {
                // tries to creates a new entry on the DB that associates the user to the terminal
                CreateEmployeeSessionDataRequest createSessionRequest = new CreateEmployeeSessionDataRequest(employee.Permissions.AllowMultipleLogins);
                string existingSessionTerminalId = context.Execute<CreateEmployeeSessionDataResponse>(createSessionRequest).ExistingSessionTerminalId;
                bool employeeHasSessionOpenOnAnotherTerminal = !string.IsNullOrWhiteSpace(existingSessionTerminalId);

                // check if the session was created for the employee
                if (employeeHasSessionOpenOnAnotherTerminal && !employee.Permissions.AllowMultipleLogins)
                {
                    string message = string.Format(
                        "Employee has an open session on terminal {0} and is not allowed to have open sessions on multiple terminals at once.",
                        existingSessionTerminalId);
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UserLogonAnotherTerminal, message);
                }

                return employee;
            }
        }
    }
}