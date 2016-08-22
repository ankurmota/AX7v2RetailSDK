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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Encapsulates the workflow required to update the status of a shift record.
        /// </summary>
        public sealed class ChangeShiftStatusRequestHandler : SingleRequestHandler<ChangeShiftStatusRequest, ChangeShiftStatusResponse>
        {
            /// <summary>
            /// Executes the create shift staging workflow.
            /// </summary>
            /// <param name="request">The new Shift request.</param>
            /// <returns>The new Shift response.</returns>
            protected override ChangeShiftStatusResponse Process(ChangeShiftStatusRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.ShiftTerminalId, "request.ShiftTerminalId");
    
                if (this.Context.GetTerminal() != null)
                {
                    request.TerminalId = this.Context.GetTerminal().TerminalId;
                }
    
                EmployeePermissions permissions = EmployeePermissionHelper.GetEmployeePermissions(this.Context, this.Context.GetPrincipal().UserId);
                bool includeSharedShifts = permissions.HasManagerPrivileges || permissions.AllowManageSharedShift || permissions.AllowUseSharedShift;
                var staffId = this.Context.GetPrincipal().UserId;
                var terminalId = request.ShiftTerminalId;
                var shifts = ShiftDataDataServiceHelper.GetShifts(
                    this.Context,
                    this.Context.GetPrincipal().ChannelId,
                    terminalId,
                    request.ShiftId,
                    includeSharedShifts);
    
                Shift shift = ShiftDataDataServiceHelper.FilterShifts(shifts, terminalId, staffId).FirstOrDefault();
                if (shift == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "There is no shift with the given identifier.");
                }
    
                ShiftTransitionHelper shiftTransitionHelper = new ShiftTransitionHelper(this.Context, request);
    
                // Validate if the change of shift status can be performed
                shiftTransitionHelper.TransitShiftStatus(shift);
    
                shift.StatusDateTime = this.Context.GetNowInChannelTimeZone();
    
                UpdateShiftStagingTableDataRequest dataServiceRequest = new UpdateShiftStagingTableDataRequest(shift);
                request.RequestContext.Runtime.Execute<NullResponse>(dataServiceRequest, this.Context);
    
                this.SaveTransactionLog(shift, request.TransactionId);
    
                if (request.ToStatus == ShiftStatus.Closed)
                {
                    this.PurgeSalesTransactionData(request.RequestContext);
                }
    
                return new ChangeShiftStatusResponse(shift);
            }
    
            private static TransactionType? GetTransactionType(Shift shift)
            {
                switch (shift.Status)
                {
                    case ShiftStatus.BlindClosed:
                        return TransactionType.BlindCloseShift;
                    case ShiftStatus.Closed:
                        return TransactionType.CloseShift;
                    case ShiftStatus.Suspended:
                        return TransactionType.SuspendShift;
                    default:
                        return null;
                }
            }
    
            /// <summary>
            /// Simple, non-critical maintenance task assigned to the shift closing event.
            /// </summary>
            /// <param name="requestContext">The request context.</param>
            private void PurgeSalesTransactionData(RequestContext requestContext)
            {
                if (requestContext == null || requestContext.Runtime == null)
                {
                    return;
                }
    
                // Get retention period in days from device configuration
                Terminal terminal = requestContext.GetTerminal();
                DeviceConfiguration deviceConfiguration = requestContext.GetDeviceConfiguration();
    
                // Purge transactions
                PurgeSalesTransactionsDataRequest dataServiceRequest = new PurgeSalesTransactionsDataRequest(terminal.ChannelId, terminal.TerminalId, deviceConfiguration.RetentionDays);
                requestContext.Runtime.Execute<NullResponse>(dataServiceRequest, this.Context);
            }
    
            private void SaveTransactionLog(Shift shift, string transactionId)
            {
                TransactionType? transactionType = GetTransactionType(shift);
    
                if (transactionType == null)
                {
                    return; // Do nothing
                }

                TransactionLog transactionLog = new TransactionLog
                {
                    TransactionType = transactionType.Value,
                    StaffId = shift.StaffId,
                    Id = transactionId,
                    TerminalId = shift.CurrentTerminalId,
                    StoreId = shift.StoreId
                };

                // Need to persist the current terminal id here since the Shift could be floated.
                SaveTransactionLogDataRequest saveTransactionLogDataServiceRequest = new SaveTransactionLogDataRequest(transactionLog);
                this.Context.Runtime.Execute<NullResponse>(saveTransactionLogDataServiceRequest, this.Context);
            }
        }
    }
}
