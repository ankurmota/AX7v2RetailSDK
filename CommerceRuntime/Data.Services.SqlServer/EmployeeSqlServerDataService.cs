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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Employee data services that contains methods to retrieve the information by calling views.
        /// </summary>
        public class EmployeeSqlServerDataService : IRequestHandler
        {
            private const string CreateEmployeeSessionSprocName = "INSERTRETAILSTAFFLOGINLOG";
            private const string UnlockUserSprocName = "DELETERETAILSTAFFLOGINLOG";
            private const string UpdateEmployeePasswordSprocName = "UPDATEEMPLOYEEPASSWORD_V2";
            private const string VerifyUserLockoutSprocName = "VERIFYUSERLOCKOUTPOLICY";
            private const string LogAuthenticationRequestSprocName = "LOGAUTHENTICATIONREQUEST";
            private const string ChannelIdParamName = "@CHANNELID";
            private const string StaffIdParamName = "@STAFFID";
            private const string DataAreaIdParamName = "@DATAAREAID";
            private const string ReturnValueParamName = "@RETURN_VALUE";
            private const string CreateEmployeeSessionOpenSessionTerminalIdReturnParamName = "@OPENSESSIONOTHERTERMINALID";
            private const string CreateEmployeeSessionReturnValueParamName = "@RETURN_VALUE";
            private const string OnlyCreateSessionIfNoOtherSessionsArePresentParameterName = "@CREATEONLYIFNOSESSION";
            private const string TerminalIdParamName = "@TERMINALID";
            private const string TimeToAccountUnlockInSecondsParamName = "@i_TimeToAccountUnlockInSeconds";
            private const string AuthenticationStatusParamName = "@i_AuthenticationStatus";
            private const string SaveUserCredentialsSprocName = "SAVEUSERCREDENTIALS";
            private const string DeleteUserCredentialsSprocName = "DELETEUSERCREDENTIALS";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(LockUserAtLogOnDataRequest),
                    typeof(CreateEmployeeSessionDataRequest),
                    typeof(UnlockUserAtLogOffDataRequest),
                    typeof(UpdateEmployeePasswordDataRequest),
                    typeof(LogAuthenticationDataRequest),
                    typeof(VerifyUserLockoutPolicyDataRequest),
                    typeof(CreateEmployeeSessionDataRequest),
                    typeof(SaveUserCredentialsDataRequest),
                    typeof(DeleteUserCredentialsDataRequest),
                };
                }
            }

            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestType = request.GetType();
                Response response;

                if (requestType == typeof(LockUserAtLogOnDataRequest))
                {
                    response = this.LockUserAtLogOn((LockUserAtLogOnDataRequest)request);
                }
                else if (requestType == typeof(CreateEmployeeSessionDataRequest))
                {
                    response = this.CreateEmployeeSession((CreateEmployeeSessionDataRequest)request);
                }
                else if (requestType == typeof(UnlockUserAtLogOffDataRequest))
                {
                    response = this.UnLockUserAtLogOff((UnlockUserAtLogOffDataRequest)request);
                }
                else if (requestType == typeof(UpdateEmployeePasswordDataRequest))
                {
                    response = this.UpdateEmployeePassword((UpdateEmployeePasswordDataRequest)request);
                }
                else if (requestType == typeof(LogAuthenticationDataRequest))
                {
                    response = this.LogAuthenticationRequest((LogAuthenticationDataRequest)request);
                }
                else if (requestType == typeof(VerifyUserLockoutPolicyDataRequest))
                {
                    response = this.VerifyUserLockoutPolicy((VerifyUserLockoutPolicyDataRequest)request);
                }
                else if (requestType == typeof(SaveUserCredentialsDataRequest))
                {
                    response = this.SaveUserCredentials((SaveUserCredentialsDataRequest)request);
                }
                else if (requestType == typeof(DeleteUserCredentialsDataRequest))
                {
                    response = this.DeleteUserCredentials((DeleteUserCredentialsDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets the cache accessor for the employee data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="EmployeeL2CacheDataStoreAccessor"/> class.</returns>
            private static EmployeeL2CacheDataStoreAccessor GetCacheAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new EmployeeL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }

            /// <summary>
            /// Delete user credentials associated to the <paramref name="request"/>.
            /// </summary>
            /// <param name="request">The data request.</param>
            /// <returns>A void service response.</returns>
            private NullResponse DeleteUserCredentials(DeleteUserCredentialsDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@nvc_staffId"] = request.UserId;
                    parameters["@nvc_grantType"] = request.GrantType;

                    databaseContext.ExecuteStoredProcedureNonQuery(DeleteUserCredentialsSprocName, parameters);
                }

                return new NullResponse();
            }

            /// <summary>
            /// Persists the user credential on the database.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private NullResponse SaveUserCredentials(SaveUserCredentialsDataRequest request)
            {
                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@bi_recId"] = request.UserCredential.RecId;
                    parameters["@nvc_StaffId"] = request.UserCredential.StaffId;
                    parameters["@nvc_HashedCredential"] = request.UserCredential.HashedCredential;
                    parameters["@nvc_Salt"] = request.UserCredential.Salt;
                    parameters["@nvc_HashAlgorithm"] = request.UserCredential.HashAlgorithm;
                    parameters["@nvc_GrantType"] = request.UserCredential.GrantType;
                    parameters["@nvc_CredentialId"] = request.UserCredential.CredentialId;
                    parameters["@nvc_AdditionalAuthenticationData"] = request.UserCredential.AdditionalAuthenticationData;

                    databaseContext.ExecuteStoredProcedureNonQuery(SaveUserCredentialsSprocName, parameters);
                }

                return new NullResponse();
            }

            /// <summary>
            /// Updates the current user's password.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private NullResponse UpdateEmployeePassword(UpdateEmployeePasswordDataRequest request)
            {
                ThrowIf.NullOrWhiteSpace(request.StaffId, "StaffId");

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request.RequestContext))
                {
                    ParameterSet parameters = new ParameterSet();
                    parameters["@nvc_StaffId"] = request.StaffId;
                    parameters["@nvc_PasswordHash"] = request.PasswordHash;
                    parameters["@nvc_PasswordSalt"] = request.PasswordSalt;
                    parameters["@nvc_PasswordHashAlgorithm"] = request.PasswordHashAlgorithm;
                    parameters["@dt_PasswordLastChangedDateTime"] = request.PasswordLastChangedDateTime.UtcDateTime;
                    parameters["i_PasswordLastUpdatedOperation"] = (int)request.PasswordLastUpdatedOperation;
                    parameters["@nvc_ChangePasswordAtNextLogOn"] = request.ChangePasswordAtNextLogOn;

                    databaseContext.ExecuteStoredProcedureNonQuery(UpdateEmployeePasswordSprocName, parameters);

                    // Clear cache for the employee object after updating password.
                    GetCacheAccessor(request.RequestContext).ClearCacheAuthorizedEmployeeOnStore(request.RequestContext.GetPrincipal().ChannelId, request.StaffId);
                }

                return new NullResponse();
            }

            /// <summary>
            /// Unlock the current user.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> UnLockUserAtLogOff(UnlockUserAtLogOffDataRequest request)
            {
                ParameterSet parameters = new ParameterSet();
                parameters[ChannelIdParamName] = request.ChannelId;
                parameters[StaffIdParamName] = request.StaffId;
                parameters[DataAreaIdParamName] = request.DataAreaId;
                ParameterSet outputParameters = new ParameterSet();
                outputParameters[ReturnValueParamName] = 0;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    databaseContext.ExecuteStoredProcedureScalar(UnlockUserSprocName, parameters, outputParameters);
                }

                bool result = false;
                if ((int)outputParameters[ReturnValueParamName] == 1)
                {
                    result = true;
                }

                EmployeeL2CacheDataStoreAccessor cacheAccessor = GetCacheAccessor(request.RequestContext);
                cacheAccessor.ClearCacheLockUserAtLogOn(request.TerminalId, request.StaffId);

                // unlocking the user has the same meaning as deleting the user session on the terminal
                cacheAccessor.ClearCacheIsEmployeeSessionOpenOnTerminal(request.TerminalId, request.StaffId);

                return new SingleEntityDataServiceResponse<bool>(result);
            }

            /// <summary>
            /// Lock the current user, so that same user can't log into another terminal until log off from the current terminal.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<bool> LockUserAtLogOn(LockUserAtLogOnDataRequest request)
            {
                const bool AllowMultipleTerminalSessions = false;

                bool isAccessAllowed;
                EmployeeL2CacheDataStoreAccessor cacheAccessor = GetCacheAccessor(request.RequestContext);
                if (!cacheAccessor.LockUserAtLogOn(request.TerminalId, request.StaffId, out isAccessAllowed))
                {
                    CreateEmployeeSessionDataResponse response = this.CreateEmployeeSession(
                        request.RequestContext,
                        AllowMultipleTerminalSessions,
                        request.ChannelId,
                        request.TerminalId,
                        request.StaffId,
                        request.DataAreaId);

                    // access is allowed if there is no previous existing session
                    isAccessAllowed = string.IsNullOrWhiteSpace(response.ExistingSessionTerminalId);

                    if (isAccessAllowed)
                    {
                        // only cache if user is allowed
                        // if user is blocked, we want to always keep performing the check to avoid
                        // blocking the user during the cache expiration period
                        cacheAccessor.CacheLockUserAtLogOn(request.TerminalId, request.StaffId, isAccessAllowed);
                    }
                }

                return new SingleEntityDataServiceResponse<bool>(isAccessAllowed);
            }

            /// <summary>
            /// Creates an employee session.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response for creating the new session.</returns>
            private CreateEmployeeSessionDataResponse CreateEmployeeSession(CreateEmployeeSessionDataRequest request)
            {
                return this.CreateEmployeeSession(
                    request.RequestContext,
                    request.AlllowMultipleTerminalSessions,
                    request.RequestContext.GetPrincipal().ChannelId,
                    request.RequestContext.GetTerminal().TerminalId,
                    request.RequestContext.GetPrincipal().UserId,
                    request.RequestContext.GetChannelConfiguration().InventLocationDataAreaId);
            }

            /// <summary>
            /// Creates an employee session.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="allowMultipleTerminalSessions">A value indicating whether a session can be created if the employee already holds a session on another terminal.</param>
            /// <param name="channelId">The channel identifier.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="dataAreaId">The data area identifier.</param>
            /// <returns>The response for creating the new session.</returns>
            private CreateEmployeeSessionDataResponse CreateEmployeeSession(
                RequestContext context,
                bool allowMultipleTerminalSessions,
                long channelId,
                string terminalId,
                string staffId,
                string dataAreaId)
            {
                ParameterSet parameters = new ParameterSet();
                parameters[TerminalIdParamName] = terminalId;
                parameters[ChannelIdParamName] = channelId;
                parameters[StaffIdParamName] = staffId;
                parameters[DataAreaIdParamName] = dataAreaId;
                
                // if user is not allowed multiple sessions, then we can only create the session if no other sessions are present for the user
                parameters[OnlyCreateSessionIfNoOtherSessionsArePresentParameterName] = !allowMultipleTerminalSessions;

                ParameterSet outputParameters = new ParameterSet();
                outputParameters[CreateEmployeeSessionOpenSessionTerminalIdReturnParamName] = string.Empty;
                outputParameters[CreateEmployeeSessionReturnValueParamName] = 0;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(context))
                {
                    databaseContext.ExecuteStoredProcedureScalar(CreateEmployeeSessionSprocName, parameters, outputParameters);
                }

                string existingSessionTerminalId = (string)outputParameters[CreateEmployeeSessionOpenSessionTerminalIdReturnParamName];

                // session was created if multiple terminal sessions were allowed; or if it wasn't, then session was created only if no previous session was found
                bool wasSessionCreated = allowMultipleTerminalSessions || string.IsNullOrWhiteSpace(existingSessionTerminalId);

                GetCacheAccessor(context).CacheIsEmployeeSessionOpenOnTerminal(terminalId, staffId, wasSessionCreated);

                return new CreateEmployeeSessionDataResponse(existingSessionTerminalId);
            }

            /// <summary>
            /// Logs the user authentication request.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The empty response.</returns>
            private NullResponse LogAuthenticationRequest(LogAuthenticationDataRequest request)
            {
                ParameterSet parameters = new ParameterSet();
                parameters[AuthenticationStatusParamName] = (int)request.AuthenticationStatus;
                parameters["@ui_LogId"] = Guid.NewGuid();
                parameters["@bi_ChannelId"] = request.ChannelId;
                parameters["@nvc_StaffId"] = request.StaffId;
                parameters["@i_AuthenticationOperation"] = request.AuthenticationOperation;

                int errorCode;

                using (var databaseContext = new SqlServerDatabaseContext(request))
                {
                    errorCode = databaseContext.ExecuteStoredProcedureNonQuery(LogAuthenticationRequestSprocName, parameters);
                }

                if (errorCode != (int)DatabaseErrorCodes.Success)
                {
                    throw new StorageException(StorageErrors.Microsoft_Dynamics_Commerce_Runtime_CriticalStorageError, errorCode, "Unable to save logon attempt.");
                }

                return new NullResponse();
            }

            /// <summary>
            /// Verifies if the current user is locked out from the system, and if yes, returns the time duration in which the user will be unlocked.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response containing the number of minutes for which the user will be blocked.</returns>
            private SingleEntityDataServiceResponse<int> VerifyUserLockoutPolicy(VerifyUserLockoutPolicyDataRequest request)
            {
                ParameterSet parameters = new ParameterSet();
                parameters["@bi_ChannelId"] = request.RequestContext.GetPrincipal().ChannelId;
                parameters["@nvc_StaffId"] = request.StaffId;
                ParameterSet outputParameters = new ParameterSet();
                outputParameters[TimeToAccountUnlockInSecondsParamName] = 0;
                int timeToAccountUnlockInSeconds;

                using (SqlServerDatabaseContext databaseContext = new SqlServerDatabaseContext(request))
                {
                    databaseContext.ExecuteStoredProcedureScalar(VerifyUserLockoutSprocName, parameters, outputParameters);
                }

                timeToAccountUnlockInSeconds = (int)outputParameters[TimeToAccountUnlockInSecondsParamName];

                return new SingleEntityDataServiceResponse<int>(timeToAccountUnlockInSeconds);
            }
        }
    }
}