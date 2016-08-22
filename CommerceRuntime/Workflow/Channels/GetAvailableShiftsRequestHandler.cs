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
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow required to get available stores for device.
        /// </summary>
        public sealed class GetAvailableShiftsRequestHandler :
            SingleRequestHandler<GetAvailableShiftsRequest, GetAvailableShiftsResponse>
        {
            /// <summary>
            /// Executes the workflow to get available Shifts.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetAvailableShiftsResponse Process(GetAvailableShiftsRequest request)
            {
                ThrowIf.Null(request, "request");

                IEnumerable<Shift> shifts;
                EmployeePermissions employeePermission = EmployeePermissionHelper.GetEmployeePermissions(this.Context, this.Context.GetPrincipal().UserId);
    
                var staffId = this.Context.GetPrincipal().UserId;
    
                GetCurrentTerminalIdDataRequest dataRequest = new GetCurrentTerminalIdDataRequest();
                string terminalId = this.Context.Execute<SingleEntityDataServiceResponse<string>>(dataRequest).Entity;
    
                bool isManager = this.Context.GetPrincipal().IsInRole(AuthenticationHelper.ManagerPrivilegies);
                bool readAllShifts;
                bool includeSharedShifts;
    
                switch (request.Status)
                {
                    case ShiftStatus.Suspended:
                    case ShiftStatus.BlindClosed:
                        includeSharedShifts = employeePermission.AllowManageSharedShift;
    
                        // If user is manager or has permission to logon multiple shifts or allowed to manage shared shifts)
                        // Read all shifts including shared
                        // Else read only shifts that belong to the user and are not shared shifts.
                        if (isManager || (employeePermission.AllowMultipleShiftLogOn && includeSharedShifts))
                        {
                            // Read all shifts
                            shifts = ShiftDataDataServiceHelper.GetAllStoreShiftsWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, request.Status, request.QueryResultSettings, true);
                        }
                        else if (employeePermission.AllowMultipleShiftLogOn && !includeSharedShifts)
                        {
                            shifts = ShiftDataDataServiceHelper.GetShiftsForStaffWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, request.Status, request.QueryResultSettings, false);
                        }
                        else if (includeSharedShifts && !employeePermission.AllowMultipleShiftLogOn)
                        {
                            var allShifts = ShiftDataDataServiceHelper.GetShiftsForStaffWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, request.Status, request.QueryResultSettings, true);

                            // Exclude non shared shifts and shift not opened or used by the current staff id.
                            shifts = allShifts.Where(s => (s.IsShared == true || s.CurrentStaffId == staffId || s.StaffId == staffId));
                        }
                        else
                        {
                            shifts = ShiftDataDataServiceHelper.GetShiftsForStaffWithStatus(this.Context, this.Context.GetPrincipal().ChannelId, staffId, request.Status, request.QueryResultSettings, false);
                        }

                        break;
                    case ShiftStatus.Open:
    
                        includeSharedShifts = employeePermission.AllowManageSharedShift || employeePermission.AllowUseSharedShift;
                        readAllShifts = isManager || (includeSharedShifts && employeePermission.AllowMultipleShiftLogOn);
    
                        // If user is manager or (has permission to logon multiple shifts and allowed to manage or use shared shifts)
                        // Read all terminal shifts including shared
                        // Else read only shifts that belong to the user and are not shared shifts.
                        if (readAllShifts)
                        {
                            var openShiftsOnTerminal = ShiftDataDataServiceHelper.GetAllOpenedShiftsOnTerminal(this.Context, this.Context.GetPrincipal().ChannelId, terminalId, false);
                            var openSharedShiftsOnStore = ShiftDataDataServiceHelper.GetAllOpenedSharedShiftsOnStore(this.Context, this.Context.GetPrincipal().ChannelId);
                            shifts = openShiftsOnTerminal.Union(openSharedShiftsOnStore);
                        }
                        else
                        {
                            IEnumerable<Shift> usableShifts;
                            if (employeePermission.AllowMultipleShiftLogOn)
                            {
                                usableShifts = ShiftDataDataServiceHelper.GetAllOpenedShiftsOnTerminal(this.Context, this.Context.GetPrincipal().ChannelId, terminalId, false);
                            }
                            else
                            {
                                usableShifts = ShiftDataDataServiceHelper.GetOpenedShiftsOnTerminalForStaff(this.Context, this.Context.GetPrincipal().ChannelId, staffId, terminalId, false);
                            }
    
                            if (includeSharedShifts)
                            {
                                var openSharedShiftsOnStore = ShiftDataDataServiceHelper.GetAllOpenedSharedShiftsOnStore(this.Context, this.Context.GetPrincipal().ChannelId);
                                shifts = usableShifts.Union(openSharedShiftsOnStore);
                            }
                            else
                            {
                                shifts = usableShifts;
                            }
                        }
    
                        break;
                    default:
                        shifts = new Shift[] { };
                        break;
                }

                if (shifts != null || shifts.Count() != 0)
                {
                    shifts = ShiftDataDataServiceHelper.FilterShifts(shifts, terminalId, staffId);
                }

                return new GetAvailableShiftsResponse(shifts.AsPagedResult());
            }
        }
    }
}
