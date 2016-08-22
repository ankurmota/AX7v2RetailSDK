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
    namespace Commerce.Runtime.Workflow.Composition
    {
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Reflection;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Framework.Exceptions;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// The request trigger that is executed for every request.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Dynamics.Retail.StyleCop.Rules.FileNameAnalyzer", "SR1704:FileNameDoesNotMatchElementInside", Justification = "Will be removed once file is renamed.")]
        public class PipelineRequestTrigger : IRequestTrigger
        {
            internal static readonly string IsTimeZoneAdjustedPropertyName = "IsTimeZoneAdjusted";

            /// <summary>
            /// Gets the collection of request types supported by this trigger.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    // This trigger will be executed for all request types.
                    return Enumerable.Empty<Type>();
                }
            }

            /// <summary>
            /// Populates the request context.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The populated request context.</returns>
            public static RequestContext PopulateRequestContext(RequestContext context)
            {
                ThrowIf.Null(context, "context");
                ICommercePrincipal principal = context.GetPrincipal();

                if (principal != null && !principal.IsChannelAgnostic)
                {
                    var getChannelConfigurationDataServiceRequest = new GetChannelConfigurationDataRequest(context.GetPrincipal().ChannelId);
                    ChannelConfiguration channelConfiguration = context.Runtime.Execute<SingleEntityDataServiceResponse<ChannelConfiguration>>(getChannelConfigurationDataServiceRequest, context, skipRequestTriggers: true).Entity;

                    context.SetChannelConfiguration(channelConfiguration);

                    if (context.GetChannelConfiguration().ChannelType == RetailChannelType.RetailStore)
                    {
                        PopulateContextWithOrgUnit(context);
                    }

                    if (!principal.IsTerminalAgnostic)
                    {
                        PopulateContextWithTerminal(context);

                        if (!string.IsNullOrWhiteSpace(principal.DeviceNumber))
                        {
                            PopulateContextWithDeviceConfiguration(context);
                        }
                    }

                    // Use the channel's default language if no language was set on the request.
                    if (string.IsNullOrWhiteSpace(context.LanguageId))
                    {
                        if (!string.IsNullOrWhiteSpace(context.Runtime.Locale))
                        {
                            context.LanguageId = context.Runtime.Locale;
                        }
                        else
                        {
                            context.LanguageId = context.GetChannelConfiguration().DefaultLanguageId;
                        }
                    }

                    if (context.GetTerminal() != null && !string.IsNullOrWhiteSpace(principal.UserId) && principal.ShiftId == 0)
                    {
                        PopulateContextWithShiftInformation(context);
                    }
                }

                return context;
            }

            /// <summary>
            /// Invoked before request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            public void OnExecuting(Request request)
            {
                ThrowIf.Null(request, "request");
                Type requestType = request.GetType();
                ICommercePrincipal principal = request.RequestContext.GetPrincipal();
                if (request.NeedChannelIdFromPrincipal && principal.IsChannelAgnostic)
                {
                    throw new CommerceException(
                        SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidChannel.ToString(),
                        ExceptionSeverity.Warning,
                        null,
                        string.Format("Invalid channel id {0} in current principal. Request type is {1}.", principal.ChannelId, requestType));
                }

                AdjustRequestTimeZone(request);
                InitializeRequestContext(request, request.RequestContext);
            }

            /// <summary>
            /// Invoked after request has been processed by <see cref="IRequestHandler"/>.
            /// </summary>
            /// <param name="request">The request message processed by handler.</param>
            /// <param name="response">The response message generated by handler.</param>
            public void OnExecuted(Request request, Response response)
            {
            }

            internal static void AdjustRequestTimeZone(Request request)
            {
                bool isTimeZoneAdjusted = (bool)(request.GetProperty(IsTimeZoneAdjustedPropertyName) ?? false);
                if (isTimeZoneAdjusted)
                {
                    return;
                }

                var channelConfiguration = request.RequestContext.GetChannelConfiguration();
                bool convertToChannelTimeZone = channelConfiguration != null;
                RequestTypeCache requestTypeCache = RequestTypeCache.GetInstance(request.GetType());
                foreach (PropertyInfo property in requestTypeCache.PublicNonStaticProperties)
                {
                    RequestTypeCache typeCache = RequestTypeCache.GetInstance(request.GetType());

                    // if channel is set then adjust date/time properties to channel time zone.
                    if (convertToChannelTimeZone)
                    {
                        property.AdjustToChannelTimeZone(request, channelConfiguration.TimeZoneRecords, typeCache);
                    }
                    else
                    {
                        // conversion to UTC will be made.
                        property.AdjustToUniversalTimeZone(request, typeCache);
                    }
                }

                request.SetProperty(IsTimeZoneAdjustedPropertyName, true);
            }

            /// <summary>
            /// Initializes the request context.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="context">The request context.</param>
            private static void InitializeRequestContext(Request request, RequestContext context)
            {
                ThrowIf.Null(context, "context");
                ThrowIf.Null(request, "request");

                if (context.IsInitialized)
                {
                    return;
                }

                context.SetInitialized();
                PopulateRequestContext(context);

                // We only want to authorize once per context initialization
                // Since principal cannot be changed on the context, any calls performed using same context
                // will be under the same user's principal
                ICommercePrincipal principal = context.GetPrincipal();
                if (principal != null && principal.IsInRole(CommerceRoles.Employee))
                {
                    // Some requests must not be verified for an already opened session because they are required to
                    // establish a session on the first place
                    bool enforceSessionOpened = request.NeedSessionOpened;

                    // Authorizes the staff request
                    context.Runtime.Execute<StaffAuthorizationServiceResponse>(new StaffAuthorizationServiceRequest(enforceSessionOpened), context, skipRequestTriggers: true);
                }
            }

            /// <summary>
            /// Gets the store by identifier and sets it on the context.
            /// </summary>
            /// <param name="context">The context.</param>
            private static void PopulateContextWithOrgUnit(RequestContext context)
            {
                SearchOrgUnitDataRequest request = new SearchOrgUnitDataRequest(context.GetPrincipal().ChannelId);
                OrgUnit orgUnit = context.Runtime.Execute<EntityDataServiceResponse<OrgUnit>>(request, context, skipRequestTriggers: true).PagedEntityCollection.SingleOrDefault();
                context.SetOrgUnit(orgUnit);
            }

            /// <summary>
            /// Gets the terminal by identifier and sets it on the context.
            /// </summary>
            /// <param name="context">The context.</param>
            private static void PopulateContextWithTerminal(RequestContext context)
            {
                long terminalId = context.GetPrincipal().TerminalId;

                GetTerminalDataRequest getTerminalDataRequest = new GetTerminalDataRequest(terminalId, QueryResultSettings.AllRecords);
                Terminal terminal = context.Runtime.Execute<SingleEntityDataServiceResponse<Terminal>>(getTerminalDataRequest, context, skipRequestTriggers: true).Entity;

                if (terminal == null)
                {
                    RetailLogger.Instance.CrtWorkflowPipelineRequestHandlerPopulateTerminalOnContextTerminalNotFound(terminalId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_TerminalNotFound, "Could not find the terminal associated with the request context.");
                }

                if (terminal.ChannelId != context.GetPrincipal().ChannelId)
                {
                    RetailLogger.Instance.CrtWorkflowPipelineRequestHandlerPopulateTerminalOnContextChannelMismatch(terminalId, terminal.ChannelId, context.GetPrincipal().ChannelId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_TerminalNotFound, "The provided terminal record identifier does not belong to the channel associated to the principal.");
                }

                context.SetTerminal(terminal);
            }

            /// <summary>
            /// Gets the device configuration and sets it on the context.
            /// </summary>
            /// <param name="context">The context.</param>
            private static void PopulateContextWithDeviceConfiguration(RequestContext context)
            {
                GetDeviceConfigurationDataRequest dataRequest = new GetDeviceConfigurationDataRequest(includeImages: false);
                DeviceConfiguration deviceConfiguration = context.Runtime.Execute<SingleEntityDataServiceResponse<DeviceConfiguration>>(dataRequest, context, skipRequestTriggers: true).Entity;
                context.SetDeviceConfiguration(deviceConfiguration);
            }

            /// <summary>
            /// Gets the current shift and sets it on the context.
            /// </summary>
            /// <param name="context">The context.</param>
            private static void PopulateContextWithShiftInformation(RequestContext context)
            {
                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = context.GetPrincipal().ChannelId,
                    TerminalId = context.GetTerminal().TerminalId,
                    StaffId =
                        string.IsNullOrWhiteSpace(context.GetPrincipal().OriginalUserId)
                            ? context.GetPrincipal().UserId
                            : context.GetPrincipal().OriginalUserId,
                    Status = (int)ShiftStatus.Open,
                    SearchByStaffId = true,
                    SearchByCurrentStaffId = true,
                    SearchByTerminalId = true,
                    SearchByCurrentTerminalId = true
                };

                // Get original staff id (not a manager's) during elevated permission operations.
                GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(criteria.StaffId, new ColumnSet());
                EmployeePermissions employeePermissions = context.Runtime.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(
                    permissionsDataRequest, context, skipRequestTriggers: true).Entity;
                if (employeePermissions != null)
                {
                    criteria.IncludeSharedShifts = employeePermissions.HasManagerPrivileges
                        || employeePermissions.AllowManageSharedShift
                        || employeePermissions.AllowUseSharedShift
                        || employeePermissions.AllowMultipleShiftLogOn;
                }

                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, QueryResultSettings.SingleRecord);
                Shift shift = context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context, skipRequestTriggers: true).PagedEntityCollection.Results.FirstOrDefault();

                if (shift != null)
                {
                    // TerminalId is the identifier of the terminal which creates the shift
                    context.GetPrincipal().ShiftId = shift.ShiftId;
                    context.GetPrincipal().ShiftTerminalId = shift.TerminalId;
                }
            }
        }
    }
}