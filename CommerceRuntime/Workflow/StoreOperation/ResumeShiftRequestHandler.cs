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
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow required to create a shift record.
        /// </summary>
        public sealed class ResumeShiftRequestHandler : SingleRequestHandler<ResumeShiftRequest, ResumeShiftResponse>
        {
            private EmployeePermissions permissions;
    
            /// <summary>
            /// Gets employee permissions.
            /// </summary>
            private EmployeePermissions Permissions
            {
                get
                {
                    return this.permissions ?? (this.permissions = EmployeePermissionHelper.GetEmployeePermissions(this.Context, this.Context.GetPrincipal().UserId));
                }
            }
    
            /// <summary>
            /// Executes the resume shift workflow.
            /// </summary>
            /// <param name="request">The new shift request.</param>
            /// <returns>The resume shift response.</returns>
            protected override ResumeShiftResponse Process(ResumeShiftRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ShiftTerminalId, "request.ShiftTerminalId");
    
                if (this.Context.GetTerminal() != null)
                {
                    request.TerminalId = this.Context.GetTerminal().TerminalId;
                }
    
                bool includeSharedShifts = this.Permissions.HasManagerPrivileges || this.Permissions.AllowManageSharedShift || this.Permissions.AllowUseSharedShift;
                var staffId = this.Context.GetPrincipal().UserId;
                var terminalId = request.ShiftTerminalId;
                var shifts = ShiftDataDataServiceHelper.GetShifts(
                    this.Context,
                    this.Context.GetPrincipal().ChannelId,
                    terminalId,
                    request.ShiftId,
                    includeSharedShifts);
    
                Shift shift = ShiftDataDataServiceHelper.FilterShifts(shifts, terminalId, staffId).FirstOrDefault();
                this.ValidateCanResumeShift(request, shift);
    
                shift.CashDrawer = request.CashDrawer;
                shift.CurrentStaffId = this.Context.GetPrincipal().UserId;
                shift.CurrentTerminalId = request.TerminalId;
                shift.Status = ShiftStatus.Open;
                shift.StatusDateTime = this.Context.GetNowInChannelTimeZone();
    
                UpdateShiftStagingTableDataRequest dataServiceRequest = new UpdateShiftStagingTableDataRequest(shift);
                request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, this.Context);
    
                return new ResumeShiftResponse(shift);
            }
    
            /// <summary>
            /// Validates whether the shift can be resumed.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="shift">The shift.</param>
            private void ValidateCanResumeShift(ResumeShiftRequest request, Shift shift)
            {
                if (shift == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "There is no shift with the given identifier.");
                }
    
                if (shift.Status != ShiftStatus.Suspended)
                {
                    // we cannot resume an opened shift, only use it
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Cannot resume a non-suspended shift. If the shift is open, try using it instead.");
                }
    
                if (!this.Permissions.HasManagerPrivileges)
                {
                    if (shift.IsShared && !this.Permissions.AllowManageSharedShift)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeNotAllowedManageSharedShift, "Employee not allowed to manage shared shift.");
                    }
    
                    // if it is not shared shift and if the user is not a manager, the user can only resume a shift that she/he owns or have permissions to
                    if (!shift.IsShared
                        && !string.Equals(shift.StaffId, this.Context.GetPrincipal().UserId, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(shift.CurrentStaffId, this.Context.GetPrincipal().UserId, StringComparison.OrdinalIgnoreCase)
                        && !this.Permissions.AllowMultipleShiftLogOn)
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "The user does not have permission to resume shift.");
                    }
                }
    
                IList<Shift> openShiftsOnStore = ShiftDataDataServiceHelper.GetAllStoreShiftsWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, ShiftStatus.Open, request.QueryResultSettings, true);
                bool cannotOpenShift = openShiftsOnStore.Any(openShift =>
                {
                    bool cashDrawerHasOpenShift = string.Equals(openShift.CashDrawer, request.CashDrawer, StringComparison.OrdinalIgnoreCase);
                    return (openShift.IsShared || string.Equals(openShift.CurrentTerminalId, request.TerminalId, StringComparison.OrdinalIgnoreCase)) && cashDrawerHasOpenShift;
                });
    
                if (cannotOpenShift)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CashDrawerHasAnOpenShift, "There is an open shift on the current cash drawer.");
                }
            }
        }
    }
}
