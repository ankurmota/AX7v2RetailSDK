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
    namespace Commerce.Runtime.Services.Security
    {
        using System;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        internal class StaffRealTimeSecurityValidationHelper
        {
            private const string StaffSecurityHelperContextCacheKey = "StaffSecurityHelper_CacheKey";

            private readonly RequestContext context;
            private readonly SecurityVerificationType verificationType;
            private readonly string password;
            private Employee realTimeStaffVerificationEmployee;

            /// <summary>
            /// Initializes a new instance of the <see cref="StaffRealTimeSecurityValidationHelper"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="verificationType">The verification type to be performed.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="channelId">The channel identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            /// <param name="password">The employee password.</param>
            private StaffRealTimeSecurityValidationHelper(RequestContext context, SecurityVerificationType verificationType, string staffId, long? channelId, long? terminalRecordId, string password)
            {
                this.ChannelId = channelId;
                this.TerminalRecordId = terminalRecordId;
                this.verificationType = verificationType;
                this.password = password;
                this.context = context;
                this.StaffId = staffId;
            }

            /// <summary>
            /// Gets the channel identifier.
            /// </summary>
            public long? ChannelId { get; private set; }

            /// <summary>
            /// Gets the terminal record identifier.
            /// </summary>
            public long? TerminalRecordId { get; private set; }

            /// <summary>
            /// Gets the staff identifier.
            /// </summary>
            public string StaffId { get; private set; }

            /// <summary>
            /// Gets a value indicating whether it is possible to use the database.
            /// </summary>
            private bool CanUseLocalDatabase
            {
                get
                {
                    // we can only make database calls if we are targeting the same channel as the one in the principal
                    // as every database call will be against the principal's channel database
                    return this.ChannelId.HasValue && this.ChannelId.Value == this.context.GetPrincipal().ChannelId;
                }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="StaffRealTimeSecurityValidationHelper"/> class if not already available in the context.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="verificationType">The verification type to be performed.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <param name="channelId">The channel identifier.</param>
            /// <param name="terminalRecordId">The terminal record identifier.</param>
            /// <param name="password">The employee password.</param>
            /// <returns>The staff security helper instance created.</returns>
            public static StaffRealTimeSecurityValidationHelper Create(RequestContext context, SecurityVerificationType verificationType, string staffId, long? channelId, long? terminalRecordId, string password)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");

                if (verificationType.HasFlag(SecurityVerificationType.Authentication) && string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password must be provided if authentication is required.", "password");
                }

                if (channelId.HasValue && context.GetPrincipal() != null && !context.GetPrincipal().IsTerminalAgnostic && !terminalRecordId.HasValue)
                {
                    throw new ArgumentException("Terminal record identifier must be provided whenever channel identifier is provider.", "terminalRecordId");
                }

                StaffRealTimeSecurityValidationHelper helper = new StaffRealTimeSecurityValidationHelper(context, verificationType, staffId, channelId, terminalRecordId, password);
                StaffRealTimeSecurityValidationHelper helperFromContext = context.GetProperty(StaffSecurityHelperContextCacheKey) as StaffRealTimeSecurityValidationHelper;

                // within the same context, authentication and authorization validations might occur separatelly
                // to avoid the penalty of two transaction service calls, we store the result for the context
                // and check if we can use the stored result for subsequent calls within the same context
                if (helperFromContext != null)
                {
                    // if helper is available from context and is equivalent to this one (based on parameters)
                    if (AreEquivalent(helperFromContext, helper))
                    {
                        // then use cached results from the one from context
                        helper.realTimeStaffVerificationEmployee = helperFromContext.realTimeStaffVerificationEmployee;
                    }
                }
                else
                {
                    // first time staff helper is used, keep it on context
                    context.SetProperty(StaffSecurityHelperContextCacheKey, helper);
                }

                return helper;
            }

            /// <summary>
            /// Retrieves the security verification configuration.
            /// </summary>
            /// <returns>The security verification configuration.</returns>
            public LogOnConfiguration GetSecurityVerificationConfiguration()
            {
                LogOnConfiguration verificationConfiguration;

                if (this.CanUseLocalDatabase)
                {
                    GetChannelConfigurationDataRequest getChannelConfiguration = new GetChannelConfigurationDataRequest(this.ChannelId.Value);
                    ChannelConfiguration channelConfiguration = this.context.Execute<SingleEntityDataServiceResponse<ChannelConfiguration>>(getChannelConfiguration).Entity;

                    // make sure channel configuration is valid
                    if (channelConfiguration == null)
                    {
                        string errorMessage = string.Format(
                            CultureInfo.InvariantCulture,
                            "The channel {0} does not exist.",
                            this.ChannelId.Value);
                        throw new SecurityException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannel, errorMessage);
                    }

                    verificationConfiguration = channelConfiguration.TransactionServiceProfile.StaffLogOnConfiguration;
                }
                else
                {
                    // no channel, we can only use headquarters
                    verificationConfiguration = LogOnConfiguration.RealTimeService;
                }

                return verificationConfiguration;
            }

            /// <summary>
            /// Authorizes the staff using the real time service.
            /// </summary>
            /// <param name="verifyEmployeeLocalDatabaseFallbackDelegate">A delegate that performs the verification checks against the local database that is used in case local fallback is required.</param>
            /// <returns>The verified employee.</returns>
            /// <exception cref="InvalidOperationException">Due to invalid verification type.</exception>
            /// <exception cref="HeadquarterTransactionServiceException">When the real time service process the request but returns an error.</exception>
            /// <exception cref="CommunicationException">When it was not possible to communicate with the real time service.</exception>
            public Employee VerifyEmployeeRealTimeService(Func<Employee> verifyEmployeeLocalDatabaseFallbackDelegate)
            {
                ThrowIf.Null(verifyEmployeeLocalDatabaseFallbackDelegate, "verifyEmployeeLocalDatabaseFallbackDelegate");

                Employee employee = null;

                if (this.verificationType == SecurityVerificationType.Authentication)
                {
                    throw new InvalidOperationException("Real time service does not support authentication only. Please use Authentication and Authorization for real time calls.");
                }

                // if we don't have a stored result, or the stored result is for a call with different paramters, then call TS
                if (this.realTimeStaffVerificationEmployee == null)
                {
                    // retail operation, logon key, extra data and all values from device object but channelid and terminal are not used by transaction service
                    StaffLogOnRealtimeRequest request = new StaffLogOnRealtimeRequest(
                        this.ChannelId,
                        this.TerminalRecordId,
                        this.StaffId,
                        this.password);

                    bool mustFallbackToLocal = false;

                    try
                    {
                        employee = this.realTimeStaffVerificationEmployee = this.context.Execute<StaffLogOnRealtimeResponse>(request).Employee;
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
                        // if we could not make the real time call, check if we can validate user locally
                        if (this.MustFallbackToLocalAuthorization())
                        {
                            RetailLogger.Instance.CrtServicesStaffAuthorizationServiceRetalTimeServiceFailure(exception);

                            // and fallback to local verification
                            mustFallbackToLocal = true;
                        }
                        else
                        {
                            RetailLogger.Log.CrtServicesStaffAuthorizationServiceUserNotAuthorizedToFallbackToLocalDatabase(this.StaffId, exception);

                            // because user does not have permission to perform a local authentication/authorization flow
                            // we need to fail authorization
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, exception, "User not authorized");
                        }
                    }

                    // if we fallback, do not cache result since it does not come from transaction service
                    if (mustFallbackToLocal)
                    {
                        employee = verifyEmployeeLocalDatabaseFallbackDelegate();
                    }
                }
                else
                {
                    employee = this.realTimeStaffVerificationEmployee;
                }

                return employee;
            }

            /// <summary>
            /// Determines whether two helper instances are equivalent.
            /// </summary>
            /// <param name="helperFromContext">The helper available from the context.</param>
            /// <param name="newHelper">The new helper.</param>
            /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
            private static bool AreEquivalent(StaffRealTimeSecurityValidationHelper helperFromContext, StaffRealTimeSecurityValidationHelper newHelper)
            {
                // all parameters must be same
                // passwords are same or newHelper does not care about authentication
                // verification type provided in the helper from context must contain the one required in the newHelper
                return helperFromContext.StaffId.Equals(newHelper.StaffId, StringComparison.OrdinalIgnoreCase)
                    && helperFromContext.ChannelId == newHelper.ChannelId
                    && helperFromContext.TerminalRecordId == newHelper.TerminalRecordId
                    && helperFromContext.verificationType.HasFlag(newHelper.verificationType)
                    && (newHelper.verificationType == SecurityVerificationType.Authorization || helperFromContext.password == newHelper.password);
            }

            /// <summary>
            /// Verifies whether local authorization must be used as a fallback mode in case of error on remote authorization.
            /// </summary>
            /// <returns>A value indicating whether local authorization must be used as a fallback mode in case of error on remote authorization.</returns>
            private bool MustFallbackToLocalAuthorization()
            {
                // we can only fallback to local database if we have a channel id provided
                if (this.CanUseLocalDatabase)
                {
                    EmployeePermissions employeePermissions = null;

                    try
                    {
                        // Create a temporary context with the employee for getting employee permissions.
                        RequestContext tempContext = new RequestContext(this.context.Runtime);
                        var employee = new Employee() { StaffId = this.StaffId };
                        ICommercePrincipal principal = this.context.GetPrincipal();
                        CommerceIdentity identity = new CommerceIdentity(
                            employee,
                            new Device()
                            {
                                DeviceNumber = principal.DeviceNumber,
                                Token = principal.DeviceToken,
                                ChannelId = principal.ChannelId,
                                TerminalRecordId = principal.TerminalId
                            });
                        tempContext.SetPrincipal(new CommercePrincipal(identity));

                        // Get employee permissions
                        GetEmployeePermissionsDataRequest getEmployeePermissionsDataRequest = new GetEmployeePermissionsDataRequest(
                            this.StaffId,
                            new ColumnSet());

                        employeePermissions = tempContext.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(
                            getEmployeePermissionsDataRequest).Entity;
                    }
                    catch (Exception exception)
                    {
                        // this method occurs in an error handling scenario
                        // if this fails, we do not want to break the flow
                        // so we just log the exception internally
                        RetailLogger.Instance.CrtServicesStaffAuthorizationServiceGetEmployeePermissionsFailure(exception);
                    }

                    // we can fallback if it is allowed by user permissions
                    return employeePermissions != null && employeePermissions.ContinueOnTSErrors;
                }

                return false;
            }
        }
    }
}