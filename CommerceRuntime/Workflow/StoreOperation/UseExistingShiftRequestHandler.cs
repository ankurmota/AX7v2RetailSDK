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
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow required to create a shift record.
        /// </summary>
        public sealed class UseExistingShiftRequestHandler : SingleRequestHandler<UseShiftRequest, UseShiftResponse>
        {
            /// <summary>
            /// Executes the resume shift workflow.
            /// </summary>
            /// <param name="request">The new shift request.</param>
            /// <returns>The resume shift response.</returns>
            protected override UseShiftResponse Process(UseShiftRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ShiftTerminalId, "request.ShiftTerminalId");
    
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                request.TerminalId = this.Context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;
    
                GetShiftDataRequest getShiftDataRequest = new GetShiftDataRequest(request.ShiftTerminalId, request.ShiftId);
                Shift shift = this.Context.Execute<SingleEntityDataServiceResponse<Shift>>(getShiftDataRequest).Entity;
    
                if (shift == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "There is no shift with the given identifier.");
                }
    
                this.ValidateCanUseShift(request, shift);
    
                shift.CurrentStaffId = this.Context.GetPrincipal().UserId;
                shift.CurrentTerminalId = request.TerminalId;
                shift.StatusDateTime = this.Context.GetNowInChannelTimeZone();
    
                UpdateShiftStagingTableDataRequest dataServiceRequest = new UpdateShiftStagingTableDataRequest(shift);
                request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, this.Context);
    
                return new UseShiftResponse(shift);
            }
    
            /// <summary>
            /// Validates whether the shift can be resumed.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="shift">The shift.</param>
            private void ValidateCanUseShift(UseShiftRequest request, Shift shift)
            {
                if (shift == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "There is no shift with the given identifier.");
                }

                var staffId = this.Context.GetPrincipal().UserId;

                if (!this.Context.GetPrincipal().IsInRole(AuthenticationHelper.ManagerPrivilegies))
                {
                    EmployeePermissions employeePermission = EmployeePermissionHelper.GetEmployeePermissions(this.Context, staffId);

                    if (employeePermission != null && (!(string.Equals(shift.StaffId, staffId) || string.Equals(shift.CurrentStaffId, staffId)) && ((shift.IsShared && !employeePermission.AllowUseSharedShift) || (!shift.IsShared && !employeePermission.AllowMultipleShiftLogOn))))
                    {
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_UseExistingShiftPermissionDenied, string.Format(CultureInfo.CurrentUICulture, "Permission denied to use the existing shift: {0}", this.Context.GetPrincipal().UserId));
                    }
                }

                if (shift.Status != ShiftStatus.Open)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "Only open shifts can be used. If the shift is suspended, try resuming instead.");
                }
                else if (!string.Equals(shift.CurrentTerminalId, request.TerminalId, StringComparison.OrdinalIgnoreCase) && !shift.IsShared)
                    {
                    // we can only use an open shift on a different terminal if it is a shared shift
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftAlreadyOpenOnDifferentTerminal, "The shift is open on a different terminal.");
                    }
                }
            }
        }
}
