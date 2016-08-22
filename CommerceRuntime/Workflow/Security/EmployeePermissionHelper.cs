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
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Authentication logic helper.
        /// </summary>
        internal static class EmployeePermissionHelper
        {
            /// <summary>
            /// Gets the employee permission details.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="staffId">The staff identifier.</param>
            /// <returns>
            /// The employee permission request.
            /// </returns>
            /// <exception cref="UserAuthenticationException">When the employee does not exists.</exception>
            public static EmployeePermissions GetEmployeePermissions(RequestContext context, string staffId)
            {
                ThrowIf.Null(context, "context");
                GetEmployeeDataRequest dataRequest = new GetEmployeeDataRequest(staffId, QueryResultSettings.SingleRecord);
                Employee employee = context.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;
                if (employee == null)
                {
                    string message = string.Format(CultureInfo.InvariantCulture, "The specified employee ({0}) was not found.", staffId);
                    throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, message);
                }

                // Check if the requested Employee object is same as logged-on user. 
                // If not, check staff have manager permission.
                if (!string.Equals(staffId, context.GetPrincipal().UserId))
                {
                    var checkAccessRequest = new CheckAccessIsManagerServiceRequest();
                    context.Execute<NullResponse>(checkAccessRequest);                    
                }

                GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(staffId, new ColumnSet());
                employee.Permissions = context.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(permissionsDataRequest).Entity;

                return employee.Permissions;
            }
    
            /// <summary>
            /// Helper method to get consolidated permission.
            /// </summary>
            /// <param name="employeePermissionsCollection">List of permission groups.</param>
            /// <param name="overriddenPermission">Overridden Permission.</param>
            /// <returns>The employee permissions.</returns>
            public static EmployeePermissions GetConsolidatedPermission(IEnumerable<EmployeePermissions> employeePermissionsCollection, EmployeePermissions overriddenPermission)
            {
                ThrowIf.Null(employeePermissionsCollection, "employeePermissionsCollection");
    
                EmployeePermissions employeePermissions = new EmployeePermissions();
    
                // First check if the employee has overridden permission
                // If override is  present return that permission.
                if (overriddenPermission != null)
                {
                    return overriddenPermission;
                }
    
                // If only position is found, return the permission for the position.
                // For multiple positions, enumerate the permission for all associated positions and find consolidated permission.
                if (employeePermissionsCollection.Count() == 1)
                {
                    return employeePermissionsCollection.First();
                }
                else
                {
                    foreach (EmployeePermissions settings in employeePermissionsCollection)
                    {
                        employeePermissions.AllowKitDisassembly |= settings.AllowKitDisassembly;
                        employeePermissions.AllowBlindClose |= settings.AllowBlindClose;
                        employeePermissions.AllowChangeNoVoid |= settings.AllowChangeNoVoid;
                        employeePermissions.HasManagerPrivileges |= settings.HasManagerPrivileges;
                        employeePermissions.AllowCreateOrder |= settings.AllowCreateOrder;
                        employeePermissions.AllowEditOrder |= settings.AllowEditOrder;
                        employeePermissions.AllowFloatingTenderDeclaration |= settings.AllowFloatingTenderDeclaration;
                        employeePermissions.AllowMultipleLogins |= settings.AllowMultipleLogins;
                        employeePermissions.AllowMultipleShiftLogOn |= settings.AllowMultipleShiftLogOn;
                        employeePermissions.AllowOpenDrawer |= settings.AllowOpenDrawer;
    
                        // Allow cumulative permissions for priceoverride from all positions
                        if ((employeePermissions.AllowPriceOverride == (int)EmployeePriceOverrideType.HigherOnly && settings.AllowPriceOverride == (int)EmployeePriceOverrideType.LowerOnly) ||
                            (employeePermissions.AllowPriceOverride == (int)EmployeePriceOverrideType.LowerOnly && settings.AllowPriceOverride == (int)EmployeePriceOverrideType.HigherOnly))
                        {
                            employeePermissions.AllowPriceOverride = (int)EmployeePriceOverrideType.HigherAndLower;
                        }
                        else
                        {
                            employeePermissions.AllowPriceOverride = Math.Min(employeePermissions.AllowPriceOverride, settings.AllowPriceOverride);
                        }
    
                        employeePermissions.AllowRetrieveOrder |= settings.AllowRetrieveOrder;
                        employeePermissions.AllowSalesTaxChange |= settings.AllowSalesTaxChange;
                        employeePermissions.AllowTenderDeclaration |= settings.AllowTenderDeclaration;
                        employeePermissions.AllowTransactionSuspension |= settings.AllowTransactionSuspension;
                        employeePermissions.AllowTransactionVoiding |= settings.AllowTransactionVoiding;
                        employeePermissions.AllowXReportPrinting |= settings.AllowXReportPrinting;
                        employeePermissions.AllowZReportPrinting |= settings.AllowZReportPrinting;
                        employeePermissions.AllowUseHandheld |= settings.AllowUseHandheld;
                        employeePermissions.AllowViewTimeClockEntries |= settings.AllowViewTimeClockEntries;
                        employeePermissions.AllowChangePeripheralStation |= settings.AllowChangePeripheralStation;
                        employeePermissions.ManageDevice |= settings.ManageDevice;
                        employeePermissions.AllowPasswordChange |= settings.AllowPasswordChange;
                        employeePermissions.AllowResetPassword |= settings.AllowResetPassword;
                        employeePermissions.MaximumDiscountPercentage = Math.Max(employeePermissions.MaximumDiscountPercentage, settings.MaximumDiscountPercentage);
                        employeePermissions.MaximumLineDiscountAmount = Math.Max(employeePermissions.MaximumLineDiscountAmount, settings.MaximumLineDiscountAmount);
                        employeePermissions.MaximumLineReturnAmount = Math.Max(employeePermissions.MaximumLineReturnAmount, settings.MaximumLineReturnAmount);
                        employeePermissions.MaximumTotalDiscountAmount = Math.Max(employeePermissions.MaximumTotalDiscountAmount, settings.MaximumTotalDiscountAmount);
                        employeePermissions.MaximumTotalDiscountPercentage = Math.Max(employeePermissions.MaximumTotalDiscountPercentage, settings.MaximumTotalDiscountPercentage);
                        employeePermissions.MaxTotalReturnAmount = Math.Max(employeePermissions.MaxTotalReturnAmount, settings.MaxTotalReturnAmount);
                    }
                }
    
                return employeePermissions;
            }
        }
    }
}
