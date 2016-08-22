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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow to process the employee time clock registration.
        /// </summary>
        public class EmployeeTimeRegistrationRequestHandler : SingleRequestHandler<EmployeeTimeRegistrationRequest, EmployeeTimeRegistrationResponse>
        {
            /// <summary>
            /// Workflow to process employee time clock activities.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override EmployeeTimeRegistrationResponse Process(EmployeeTimeRegistrationRequest request)
            {
                ThrowIf.Null(request, "request");
                EmployeeTimeRegistrationResponse response;
    
                if (request.IsLatestActivity && request.IsSelectStore)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Both latest activity and selecting from stores is not supported");
                }
    
                bool enableTimeRegistration = EmployeeTimeRegistrationWorkflowHelper.ValidateTimeRegistrationFunctionalityProfile(this.Context);
    
                if (!enableTimeRegistration)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_TimeClockNotEnabled,
                        string.Format("Time Clock should be enabled before performing employee activities. EmployeeActivityType: {0}", request.EmployeeActivityType));
                }
                
                // This flag is set to true if client needs to retrieve the latest activity of the employee.
                if (request.IsLatestActivity)
                {
                    EmployeeActivity employeeActivity = this.GetLatestEmployeeActivity();
    
                    response = new EmployeeTimeRegistrationResponse(new[] { employeeActivity }.AsPagedResult());
    
                    return response;
                }
    
                // The workflow follows any one of the activity chosen from client.
                switch (request.EmployeeActivityType)
                {
                    case EmployeeActivityType.ClockIn:
                    {
                        var currentActivityDateTimeOffset = this.ProcessClockIn();
                        response = new EmployeeTimeRegistrationResponse(currentActivityDateTimeOffset);
                        break;
                    }
    
                    case EmployeeActivityType.ClockOut:
                    {
                        var currentActivityDateTimeOffset = this.ProcessClockOut();
                        response = new EmployeeTimeRegistrationResponse(currentActivityDateTimeOffset);
                        break;
                    }
    
                    case EmployeeActivityType.BreakFromWork:
                    {
                        var currentActivityDateTimeOffset = this.ProcessBreakFlow(EmployeeTimeRegistrationWorkflowHelper.BreakFromWork);
                        response = new EmployeeTimeRegistrationResponse(currentActivityDateTimeOffset);
                        break;
                    }
    
                    case EmployeeActivityType.BreakForLunch:
                    {
                        var currentActivityDateTimeOffset = this.ProcessBreakFlow(EmployeeTimeRegistrationWorkflowHelper.BreakForLunch);
                        response = new EmployeeTimeRegistrationResponse(currentActivityDateTimeOffset);
                        break;
                    }
    
                    case EmployeeActivityType.Logbook:
                    {
                        if (request.IsManagerLogbook)
                        {
                            EmployeePermissions employeePermisssion = EmployeePermissionHelper.GetEmployeePermissions(this.Context, this.Context.GetPrincipal().UserId);
    
                            if (employeePermisssion == null || !employeePermisssion.AllowViewTimeClockEntries)
                            {
                                throw new DataValidationException(
                                    DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ViewTimeClockNotEnabled,
                                    string.Format("View Time Clock Entries should be enabled to view other employee activities. EmployeeActivityType: {0}", request.EmployeeActivityType));
                            }
                                
                            var employeeActivities = this.ProcessManagerLogBook(request.EmployeeActivitySearchCriteria, request.QueryResultSettings.Paging, request.QueryResultSettings.Sorting);
                            response = new EmployeeTimeRegistrationResponse(employeeActivities.AsPagedResult());
                        }
                        else
                        {
                            var employeeActivities = this.ProcessEmployeeLogBook(request.EmployeeActivitySearchCriteria, request.QueryResultSettings.Paging, request.QueryResultSettings.Sorting);
                            response = new EmployeeTimeRegistrationResponse(employeeActivities.AsPagedResult());    
                        }
                        
                        break;
                    }
    
                    default:
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_UnSupportedType, 
                            string.Format("Unsupported type for Employee Activity {0}", request.EmployeeActivityType));
                }
    
                return response;
            }
    
            private DateTimeOffset ProcessClockIn()
            {
                return EmployeeTimeRegistrationWorkflowHelper.RegisterClockIn(this.Context);
            }
    
            private DateTimeOffset ProcessClockOut()
            {
                return EmployeeTimeRegistrationWorkflowHelper.RegisterClockOut(this.Context);
            }
    
            private EmployeeActivity GetLatestEmployeeActivity()
            {
                EmployeeActivity lastActivity = EmployeeTimeRegistrationWorkflowHelper.GetLatestActivity(this.Context);
    
                // Gets the details of activity and job names if the activity is break.
                if (lastActivity != null && lastActivity.EmployeeActivityType == EmployeeActivityType.BreakFlowStart)
                {
                    lastActivity.BreakCategory = EmployeeTimeRegistrationWorkflowHelper.GetBreakCategoryByJob(this.Context, lastActivity.JobId);
                }
    
                return lastActivity ?? new EmployeeActivity();
            }
    
            private DateTimeOffset ProcessBreakFlow(string breakCategory)
            {
                string jobId = EmployeeTimeRegistrationWorkflowHelper.GetBreakCategoryJobIdByActivity(this.Context, breakCategory);
    
                return EmployeeTimeRegistrationWorkflowHelper.RegisterEmployeeBreak(this.Context, jobId);
            }
    
            private IEnumerable<EmployeeActivity> ProcessEmployeeLogBook(EmployeeActivitySearchCriteria criteria, PagingInfo pagingInfo, SortingInfo sortingInfo)
            {
                return EmployeeTimeRegistrationWorkflowHelper.GetEmployeeLogbookDetails(this.Context, criteria.StoreNumber, criteria.FromDateTimeOffset, criteria.ToDateTimeOffset, pagingInfo, sortingInfo);
            }
    
            private IEnumerable<EmployeeActivity> ProcessManagerLogBook(EmployeeActivitySearchCriteria criteria, PagingInfo pagingInfo, SortingInfo sortingInfo)
            {
                return EmployeeTimeRegistrationWorkflowHelper.GetManagerLogbookView(this.Context, criteria.StoreNumber, criteria.EmployeeActivityTypes.ToArray(), criteria.FromDateTimeOffset, criteria.ToDateTimeOffset, pagingInfo, sortingInfo);
            }
        }
    }
}
