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
    
        /// <summary>
        /// Encapsulates helper functions for getting shift data.
        /// </summary>
        internal static class ShiftDataDataServiceHelper
        {
            internal static IList<Shift> GetAllOpenedSharedShiftsOnStore(RequestContext context, long channelId)
            {
                return ShiftDataDataServiceHelper.GetAllStoreShiftsWithStatus(context, channelId, ShiftStatus.Open, QueryResultSettings.AllRecords, true)
                    .Where(s => s.IsShared).AsReadOnly();
            }
    
            internal static IList<Shift> GetAllOpenedShiftsOnTerminal(RequestContext context, long channelId, string terminalId, bool includeSharedShifts)
            {
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");

                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = channelId,
                    TerminalId = terminalId,
                    Status = (int)ShiftStatus.Open,
                    SearchByCurrentTerminalId = true,
                    IncludeSharedShifts = includeSharedShifts
                };

                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, QueryResultSettings.AllRecords);
                return context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context).PagedEntityCollection.Results;
            }
    
            internal static IList<Shift> GetOpenedShiftsOnTerminalForStaff(RequestContext context, long channelId, string staffId, string terminalId, bool includeSharedShifts)
            {
                ThrowIf.NullOrWhiteSpace(staffId, "staffId");
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");

                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = channelId,
                    StaffId = staffId,
                    TerminalId = terminalId,
                    Status = (int)ShiftStatus.Open,
                    SearchByCurrentTerminalId = true,
                    SearchByStaffId = true,
                    IncludeSharedShifts = includeSharedShifts
                };

                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, QueryResultSettings.AllRecords);
                return context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context).PagedEntityCollection.Results;
            }
    
            /// <summary>
            /// Filter shifts based on terminal identifier and staff identifier.
            /// </summary>
            /// <param name="shifts">The shifts to filter for.</param>
            /// <param name="terminalId">The terminal identifier.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <returns>The collection of filtered shifts.</returns>
            /// <remarks>Shared shifts can appear multiple times on the same register (one for each staff using the shift at a specific terminal)
            /// in order to avoid unnecessary duplication for using, resuming, or showing blind closed shifts,
            /// shifts are grouped by terminal and shift identifiers, then for each group selects the one matching both current terminal
            /// and current staff identifier, if none can be found with this criteria, default to current staff identifier,
            /// if still none is found, gets the first one in the group.</remarks>
            internal static IEnumerable<Shift> FilterShifts(IEnumerable<Shift> shifts, string terminalId, string staffId)
            {
                if (shifts.Count() == 1)
                {
                    return shifts;
                }
    
                return shifts.GroupBy(s => string.Format("{0}.{1}", s.TerminalId, s.ShiftId)).Select(
                    shiftsById =>
                    {
                        var shift = shiftsById.FirstOrDefault(s =>
                            string.Equals(staffId, s.CurrentStaffId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(terminalId, s.CurrentTerminalId, StringComparison.OrdinalIgnoreCase));
    
                        if (shift == null)
                        {
                            shift = shiftsById.FirstOrDefault(s => string.Equals(staffId, s.CurrentStaffId, StringComparison.OrdinalIgnoreCase));
                        }
    
                        return shift ?? shiftsById.First();
                    });
            }
    
            internal static IList<Shift> GetShifts(RequestContext context, long channelId, string terminalId, long shiftId, bool includeSharedShifts)
            {
                return GetShifts(context, channelId, string.Empty, terminalId, shiftId, includeSharedShifts);
            }
    
            internal static IList<Shift> GetShifts(RequestContext context, long channelId, string staffId, string terminalId, long shiftId, bool includeSharedShifts)
            {
                ThrowIf.NullOrWhiteSpace(terminalId, "terminalId");

                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = channelId,
                    TerminalId = terminalId,
                    ShiftId = shiftId,
                    SearchByTerminalId = true,
                    IncludeSharedShifts = includeSharedShifts
                };

                if (!string.IsNullOrWhiteSpace(staffId))
                {
                    criteria.StaffId = staffId;
                    criteria.SearchByStaffId = true;
                    criteria.SearchByCurrentStaffId = true;
                }
    
                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, QueryResultSettings.SingleRecord);
                return context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context).PagedEntityCollection.Results;
            }

            internal static IList<Shift> GetShiftsForStaffWithStatus(RequestContext context, long channelId, ShiftStatus status, QueryResultSettings settings, bool includeSharedShifts)
            {
                return GetShiftsForStaffWithStatus(context, channelId, null, status, settings, includeSharedShifts);
            }

            internal static IList<Shift> GetShiftsForStaffWithStatus(RequestContext context, long channelId, string staffId, ShiftStatus status, QueryResultSettings settings, bool includeSharedShifts)
            {
                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = channelId,
                    Status = (int)status,
                    SearchByStaffId = true,
                    IncludeSharedShifts = includeSharedShifts
                };

                if (!string.IsNullOrWhiteSpace(staffId))
                {
                    criteria.StaffId = staffId;
                }

                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, settings);
                return context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context).PagedEntityCollection.Results;
            }
    
            internal static IList<Shift> GetAllStoreShiftsWithStatus(RequestContext context, long channelId, ShiftStatus status, QueryResultSettings settings, bool includeSharedShifts)
            {
                ShiftDataQueryCriteria criteria = new ShiftDataQueryCriteria
                {
                    ChannelId = channelId,
                    Status = (int)status,
                    IncludeSharedShifts = includeSharedShifts
                };

                GetShiftDataDataRequest dataServiceRequest = new GetShiftDataDataRequest(criteria, settings);
                return context.Runtime.Execute<EntityDataServiceResponse<Shift>>(dataServiceRequest, context).PagedEntityCollection.Results;
            }
        }
    }
}
