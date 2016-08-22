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
        using System.Globalization;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow required to create a shift record.
        /// </summary>
        public sealed class CreateShiftRequestHandler : SingleRequestHandler<CreateShiftRequest, CreateShiftResponse>
        {
            /// <summary>
            /// Executes the create shift staging workflow.
            /// </summary>
            /// <param name="request">The new Shift request.</param>
            /// <returns>The new Shift response.</returns>
            protected override CreateShiftResponse Process(CreateShiftRequest request)
            {
                ThrowIf.Null(request, "request");
                
                // Validate Shift id.
                if (request.ShiftId == null || request.ShiftId <= 0)
                {
                    throw new ArgumentException("Shift identifier should not be null and should be greater than 0 to open a shift.");
                }
    
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                request.TerminalId = this.Context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;
                this.ValidateCanOpenShift(request);
    
                Shift shift = this.CreateNewShift(request);
    
                var createShiftRequest = new CreateShiftDataRequest(shift);
                request.RequestContext.Runtime.Execute<NullResponse>(createShiftRequest, this.Context);
    
                return new CreateShiftResponse(shift);
            }
    
            /// <summary>
            /// Validates whether employee can open shifts.
            /// </summary>
            /// <param name="request">The create shift request.</param>
            private void ValidateCanOpenShift(CreateShiftRequest request)
            {
                IList<Shift> shifts = ShiftDataDataServiceHelper.GetAllStoreShiftsWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, ShiftStatus.Open, new QueryResultSettings(PagingInfo.AllRecords), true);

                if (!this.Context.GetPrincipal().IsInRole(AuthenticationHelper.ManagerPrivilegies))
                {
                    EmployeePermissions employeePermission = EmployeePermissionHelper.GetEmployeePermissions(this.Context, this.Context.GetPrincipal().UserId);
                    if (employeePermission != null && employeePermission.AllowMultipleLogins == false && shifts.Any(shift => (string.Equals(shift.StaffId, this.Context.GetPrincipal().UserId, StringComparison.OrdinalIgnoreCase) || string.Equals(shift.CurrentStaffId, this.Context.GetPrincipal().UserId, StringComparison.OrdinalIgnoreCase))))
                    {
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_OpenMultipleShiftsNotAllowed, string.Format(CultureInfo.CurrentUICulture, "Permission denied to open multiple shifts: {0}", this.Context.GetPrincipal().UserId));
                    }

                    if (request.IsShared)
                    {
                        if (!employeePermission.AllowManageSharedShift)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeNotAllowedManageSharedShift, "Employee not allowed to manage shared shift.");
                        }
                    }
                }
    
                // Validate if shift is already open with given shift identifier.
                if (request.ShiftId != null && shifts.Any(shift => shift.ShiftId == request.ShiftId && string.Equals(shift.TerminalId, request.TerminalId, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_TerminalHasAnOpenShift, 
                        string.Format("There is already an open shift with shift id {0} on the current terminal.", request.ShiftId));
                }
    
                // Validate if any shift is open for the cash drawer specified in request.
                bool cannotOpenShift = shifts.Any(shift =>
                {
                    bool cashDrawerHasOpenShift = string.Equals(shift.CashDrawer, request.CashDrawer, StringComparison.OrdinalIgnoreCase);
                    return cashDrawerHasOpenShift && (shift.IsShared || string.Equals(shift.CurrentTerminalId, request.TerminalId, StringComparison.OrdinalIgnoreCase));
                });
    
                if (cannotOpenShift)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CashDrawerHasAnOpenShift, "There is an open shift on the current cash drawer.");
                }
            }

            /// <summary>
            /// Creates a new shift given the request.
            /// </summary>
            /// <param name="request">The create shift request.</param>
            /// <returns>The created shift.</returns>
            private Shift CreateNewShift(CreateShiftRequest request)
            {
                Shift shift = new Shift
                {
                    StartDateTime = this.Context.GetNowInChannelTimeZone(),
                    StatusDateTime = this.Context.GetNowInChannelTimeZone(),
                    StaffId = this.Context.GetPrincipal().UserId,
                    CurrentStaffId = this.Context.GetPrincipal().UserId,
                    StoreRecordId = this.Context.GetPrincipal().ChannelId,
                    Status = ShiftStatus.Open,
                    CashDrawer = request.CashDrawer,
                    IsShared = request.IsShared,
                    StoreId = this.Context.GetOrgUnit().OrgUnitNumber,
                    TerminalId = request.TerminalId,
                    CurrentTerminalId = request.TerminalId,
                    ShiftId = request.ShiftId.GetValueOrDefault(),
                };

                return shift;
            }
        }
    }
}
