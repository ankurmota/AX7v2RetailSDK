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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Helper class for shift status transition.
        /// </summary>
        internal class ShiftTransitionHelper
        {
            private readonly RequestContext context;
            private readonly ChangeShiftStatusRequest request;
            private IDictionary<KeyValuePair<ShiftStatus, ShiftStatus>, Action<Shift>> transitions;
            private DeviceConfiguration deviceConfiguration;
    
            /// <summary>
            /// Initializes a new instance of the <see cref="ShiftTransitionHelper"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="request">The request.</param>
            internal ShiftTransitionHelper(RequestContext context, ChangeShiftStatusRequest request)
            {
                this.context = context;
                this.request = request;
                this.InitShiftTransitions();
            }
    
            /// <summary>
            /// Transits the shift object status and trigger associated operations.
            /// </summary>
            /// <param name="shift">The shift object.</param>
            public void TransitShiftStatus(Shift shift)
            {
                ThrowIf.Null(shift, "shift");
    
                ShiftStatus toStatus = this.request.ToStatus;
                Action<Shift> nextAction;
                if (!this.transitions.TryGetValue(new KeyValuePair<ShiftStatus, ShiftStatus>(shift.Status, toStatus), out nextAction))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidStatus,
                        string.Format("Invalid status change request from {0} to {1}.", shift.Status, toStatus));
                }
    
                // Validates if the shift status transition is allowable
                this.ValidateCanChangeStatus(shift);
    
                // Validate start amount and tender declaration if required
                if (toStatus == ShiftStatus.Closed)
                {
                    ShiftCalculator.Calculate(this.request.RequestContext, shift, this.request.ShiftTerminalId, this.request.ShiftId);
    
                    this.ValidateStartingAmountsAndTenderDeclarationForClose(shift);
                }
    
                ThrowIf.Null(nextAction, "nextAction");
                nextAction.Invoke(shift);
            }
    
            /// <summary>
            /// Validates if the starting amounts and tender declaration of a close shift have been set.
            /// </summary>
            /// <param name="shift">The shift object to be verified.</param>
            private void ValidateStartingAmountsAndTenderDeclarationForClose(Shift shift)
            {
                bool canForceClose = this.deviceConfiguration.RequireAmountDeclaration && this.request.CanForceClose;
                if (canForceClose)
                {
                    return;
                }
    
                var validationResults = new Collection<DataValidationFailure>();
    
                // Reuse the calculated starting amount value for a shift to be closed
                if (this.deviceConfiguration.RequireAmountDeclaration && shift.StartingAmountTotal == 0)
                {
                    validationResults.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftStartingAmountNotEntered, "Starting amounts have not been entered."));
                }
    
                // Reuse the calculated declared tender value for a shift to be closed
                if (this.deviceConfiguration.RequireAmountDeclaration && shift.DeclareTenderAmountTotal == 0)
                {
                    validationResults.Add(new DataValidationFailure(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftTenderDeclarationAmountNotEntered, "A tender declaration has not been performed."));
                }
    
                if (validationResults.Any())
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftValidationError, validationResults, "Could not close shift due to data validation failures.");
                }
            }
    
            /// <summary>
            /// Validates whether a status transition is possible on the specified shift.
            /// </summary>
            /// <param name="shift">The shift.</param>
            private void ValidateCanChangeStatus(Shift shift)
            {
                bool isManager = this.context.GetPrincipal().IsInRole(AuthenticationHelper.ManagerPrivilegies);
    
                if (shift.Status == ShiftStatus.Open
                    && !string.Equals(shift.TerminalId, this.request.TerminalId, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(shift.CurrentTerminalId, this.request.TerminalId, StringComparison.OrdinalIgnoreCase))
                {
                    if (shift.IsShared)
                    {
                        if (!isManager)
                        {
                            EmployeePermissions employeePermission = EmployeePermissionHelper.GetEmployeePermissions(this.context, this.context.GetPrincipal().UserId);
                            if (!employeePermission.AllowManageSharedShift)
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeNotAllowedManageSharedShift, "Employee not allowed to manage shared shift.");
                            }
                        }
                    }
                    else
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ShiftAlreadyOpenOnDifferentTerminal, "The shift is open on a different terminal.");
                    }
                }
    
                if (!isManager)
                {
                    if (shift.IsShared)
                    {
                        EmployeePermissions employeePermission = EmployeePermissionHelper.GetEmployeePermissions(this.context, this.context.GetPrincipal().UserId);
                        if (!employeePermission.AllowManageSharedShift)
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_EmployeeNotAllowedManageSharedShift, "Employee not allowed to manage shared shift.");
                        }
                    }

                    // Get the original user id for manager override cases.
                    string userId = (!string.IsNullOrWhiteSpace(this.context.GetPrincipal().OriginalUserId) && this.context.GetPrincipal().UserId != this.context.GetPrincipal().OriginalUserId) ? this.context.GetPrincipal().OriginalUserId : this.context.GetPrincipal().UserId;
                    if (!string.Equals(shift.StaffId, userId, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(shift.CurrentStaffId, userId, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest, "The user does not have permission to change shift status.");
                    }
                }
            }
    
            /// <summary>
            /// Blind closes the shift.
            /// </summary>
            /// <param name="shift">The shift.</param>
            private void BlindCloseShift(Shift shift)
            {
                this.CloseShift(shift, true);
            }
    
            /// <summary>
            /// Closes the shift.
            /// </summary>
            /// <param name="shift">The shift.</param>
            private void CloseShift(Shift shift)
            {
                this.CloseShift(shift, false);
            }
    
            /// <summary>
            /// Suspends the shift.
            /// </summary>
            /// <param name="shift">The shift.</param>
            private void SuspendShift(Shift shift)
            {
                shift.CashDrawer = null;
                shift.Status = ShiftStatus.Suspended;
            }
    
            /// <summary>
            /// Closes the shift.
            /// </summary>
            /// <param name="shift">The shift.</param>
            /// <param name="isBlindClose">If <c>true</c> the shift will be closed with a Closed status, otherwise with BlindClosed status.</param>
            private void CloseShift(Shift shift, bool isBlindClose)
            {
                if (isBlindClose)
                {
                    shift.Status = ShiftStatus.BlindClosed;
                }
                else
                {
                    shift.Status = ShiftStatus.Closed;
                    shift.CloseDateTime = this.context.GetNowInChannelTimeZone();
                    shift.ClosedAtTerminalId = shift.CurrentTerminalId = this.context.GetTerminal().TerminalId;
                }
            }
    
            /// <summary>
            /// Initializes the allowed shift status transitions.
            /// </summary>
            private void InitShiftTransitions()
            {
                this.transitions = new Dictionary<KeyValuePair<ShiftStatus, ShiftStatus>, Action<Shift>>();
    
                this.transitions.Add(new KeyValuePair<ShiftStatus, ShiftStatus>(ShiftStatus.Open, ShiftStatus.Closed), this.CloseShift);
                this.transitions.Add(new KeyValuePair<ShiftStatus, ShiftStatus>(ShiftStatus.Open, ShiftStatus.BlindClosed), this.BlindCloseShift);
                this.transitions.Add(new KeyValuePair<ShiftStatus, ShiftStatus>(ShiftStatus.Open, ShiftStatus.Suspended), this.SuspendShift);
                this.transitions.Add(new KeyValuePair<ShiftStatus, ShiftStatus>(ShiftStatus.BlindClosed, ShiftStatus.Closed), this.CloseShift);
    
                this.deviceConfiguration = this.context.GetDeviceConfiguration();
                if (this.deviceConfiguration == null)
                {
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound, "There is no device configuration for the current channel and terminal");
                }
            }
        }
    }
}
