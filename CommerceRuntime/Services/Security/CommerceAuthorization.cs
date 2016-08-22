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
    namespace Commerce.Runtime.Services
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Commerce Authorization Helper functions.
        /// </summary>
        internal static class CommerceAuthorization
        {
            /// <summary>
            /// The manager privilege.
            /// </summary>
            private const string ManagerPrivilege = "MANAGERPRIVILEGES";

            /// <summary>
            /// Checks if the principal has permission to do the operation. If not, throws UserAuthorizationException.
            /// </summary>
            /// <param name="principal">The request.</param>
            /// <param name="operationId">Operation Id.</param>
            /// <param name="context">Request Context.</param>
            /// <param name="allowedRoles">Allowed roles.</param>
            /// <param name="deviceTokenRequired">Device token required.</param>
            /// <param name="nonDrawerOperationCheckRequired">Is non-drawer mode operation check required.</param>
            public static void CheckAccess(ICommercePrincipal principal, RetailOperation operationId, RequestContext context, string[] allowedRoles, bool deviceTokenRequired, bool nonDrawerOperationCheckRequired)
            {
                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                if (context == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid context.");
                }

                // Access check not enabled for storefront operations.
                if (principal.IsInRole(CommerceRoles.Storefront))
                {
                    return;
                }

                // Check device token for Employee role.
                if (principal.IsInRole(CommerceRoles.Employee) && deviceTokenRequired)
                {
                    if (string.IsNullOrEmpty(principal.DeviceToken))
                    {
                        throw new DeviceAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_DeviceTokenNotPresent, @"Device token was expected, but not provided.");
                    }
                }

                bool isAnonymousUser = principal.IsInRole(CommerceRoles.Anonymous);

                // Check if the principal is one of the allowed roles.
                if (allowedRoles != null)
                {
                    bool allowedRole = false;
                    foreach (string role in allowedRoles)
                    {
                        if (principal.IsInRole(role))
                        {
                            allowedRole = true;
                            break;
                        }
                    }

                    // If the user is not in the allowed roles, throw unauthorized exception.
                    if (!allowedRole)
                    {
                        if (isAnonymousUser)
                        {
                            // this means that if the user authenticates with the system and retry the request
                            // s(he) might get a different result (user authentication maps to HTTP 401 whereas Authorization maps to HTTP 403)
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, @"Assigned role is not allowed to perform this operation.");
                        }
                        else
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Assigned role is not allowed to perform this operation.");
                        }
                    }
                }

                // Access check for Anonymous principal.
                if (isAnonymousUser)
                {
                    if (operationId != RetailOperation.None)
                    {
                        GetOperationPermissionsDataRequest dataRequest = new GetOperationPermissionsDataRequest(operationId, QueryResultSettings.SingleRecord);
                        OperationPermission operationPermission = context.Execute<EntityDataServiceResponse<OperationPermission>>(dataRequest).PagedEntityCollection.FirstOrDefault();
                        if (operationPermission == null)
                        {
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied for employee to perform this operation.");
                        }

                        if (!operationPermission.AllowAnonymousAccess)
                        {
                            throw new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied to perform this operation.");
                        }
                    }
                }

                // Access check for authenticated customer.
                if (principal.IsInRole(CommerceRoles.Customer))
                {
                    if (operationId != RetailOperation.None)
                    {
                        GetOperationPermissionsDataRequest dataRequest = new GetOperationPermissionsDataRequest(operationId, QueryResultSettings.SingleRecord);
                        OperationPermission operationPermission = context.Execute<EntityDataServiceResponse<OperationPermission>>(dataRequest).PagedEntityCollection.FirstOrDefault();
                        if (operationPermission == null)
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied  to perform this operation.");
                        }

                        if (!operationPermission.AllowCustomerAccess)
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied to perform this operation.");
                        }

                        if (!string.IsNullOrWhiteSpace(operationPermission.PermissionsStringV2))
                        {
                            if (!principal.IsInRole(operationPermission.PermissionsStringV2.ToUpperInvariant()))
                            {
                                throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied for employee to perform this operation (permissions).");
                            }
                        }
                    }
                }

                // Access check for Staff principal.
                if (principal.IsInRole(CommerceRoles.Employee))
                {
                    // Validates the non drawer operation permission.
                    if (nonDrawerOperationCheckRequired)
                    {
                        CheckNonDrawerOperationPermission(operationId, context);
                    }

                    // If the principal has Manager privilege, always allow operation.
                    if (IsManager(principal) && principal.ElevatedOperation == (int)RetailOperation.None)
                    {
                        return;
                    }

                    // Only employees users have access to the retail operation specified in the attribute.
                    if (operationId != RetailOperation.None)
                    {
                        GetOperationPermissionsDataRequest dataRequest = new GetOperationPermissionsDataRequest(operationId, QueryResultSettings.SingleRecord);
                        OperationPermission operationPermission = context.Execute<EntityDataServiceResponse<OperationPermission>>(dataRequest).PagedEntityCollection.FirstOrDefault();
                        if (operationPermission == null)
                        {
                            if (operationId == RetailOperation.ActivateDevice)
                            {
                                throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_NoDeviceManagementPermission, "Access denied for employee to perform device activation operation.");
                            }

                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Access denied for employee to perform this operation.");
                        }

                        // If CheckUserAccess flag is not enabled, return.
                        if (operationPermission.CheckUserAccess == false)
                        {
                            return;
                        }

                        // Enumerate the permissions for the operation and ensure user have access.
                        foreach (string permission in operationPermission.Permissions)
                        {
                            if (!principal.IsInRole(permission.ToUpperInvariant()))
                            {
                                if (operationId == RetailOperation.ActivateDevice)
                                {
                                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_NoDeviceManagementPermission, "Access denied for employee to perform device activation operation.");
                                }

                                if (string.IsNullOrWhiteSpace(principal.OriginalUserId))
                                {
                                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Access denied for employee to perform this operation (permissions).");
                                }

                                // Checkin for elevated operation only the if the user is not already in role.
                                CheckUserIsElevatedForOperation(principal, operationId);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Method to check if the principal has manager permission.
            /// </summary>
            /// <param name="principal">Commerce Principal.</param>
            public static void CheckAccessManager(ICommercePrincipal principal)
            {
                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                if (!IsManager(principal))
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied for non-manager.");
                }
            }

            /// <summary>
            /// Method to check if the principal has a shift.
            /// </summary>
            /// <param name="principal">Commerce Principal.</param>
            public static void CheckAccessHasShift(ICommercePrincipal principal)
            {
                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                // Access check not enabled for storefront operations.
                if (principal.IsInRole(CommerceRoles.Storefront))
                {
                    return;
                }

                if (principal.ShiftId == 0)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_NonDrawerOperationsOnly, "Shift is not open.");
                }
            }

            /// <summary>
            /// Method to check if the principal has access to the carts.
            /// </summary>
            /// <param name="principal">Commerce Principal.</param>
            /// <param name="transactions">Collection of transactions to check access for.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "This is false positive. If transactions is null or empty method returns.")]
            public static void CheckAccessToCarts(ICommercePrincipal principal, IEnumerable<SalesTransaction> transactions)
            {
                if (transactions.IsNullOrEmpty())
                {
                    // Skip check if transaction does not exist.
                    return;
                }

                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                if (principal.IsAnonymous || principal.IsCustomer)
                {
                    foreach (SalesTransaction transaction in transactions)
                    {
                        // For anonymous user, check the cart belongs to anonymous user.
                        // For C2 users, make sure the cart belongs to the user or an anonymous user.
                        if ((principal.IsAnonymous || (principal.IsCustomer && !string.IsNullOrWhiteSpace(transaction.CustomerId)))
                        && !string.Equals(transaction.CustomerId ?? string.Empty, principal.UserId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "User does not have access to cart");
                        }

                        if (!string.IsNullOrWhiteSpace(transaction.StaffId))
                        {
                            throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Anonymous and C2 users cannot modify C1 carts.");
                        }
                    }
                }
            }

            /// <summary>
            /// Method to check if the principal has access to the customer account.
            /// </summary>
            /// <param name="principal">Commerce Principal.</param>
            /// <param name="customerAccount">Customer Account.</param>
            public static void CheckAccessToCustomerAccount(ICommercePrincipal principal, string customerAccount)
            {
                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                if (principal.IsAnonymous)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "User does not have access to customer account");
                }
                else if (principal.IsCustomer)
                {
                    if (!string.Equals(principal.UserId, customerAccount))
                    {
                        throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "User does not have access to customer account");
                    }
                }
            }

            /// <summary>
            /// Method to check if the principal has manager permission.
            /// </summary>
            /// <param name="principal">Commerce Principal.</param>
            /// <returns>True or False.</returns>
            private static bool IsManager(ICommercePrincipal principal)
            {
                if (principal == null)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Invalid principal.");
                }

                return principal.IsInRole(ManagerPrivilege);
            }

            /// <summary>
            /// This method performs validation for non drawer operation permissions.
            /// </summary>
            /// <param name="operationId">Retail operation Id.</param>
            /// <param name="context">The request context.</param>
            private static void CheckNonDrawerOperationPermission(RetailOperation operationId, RequestContext context)
            {
                // If the shift Id is not set then it is non-drawer operation performed by user.
                // Then we validate against the white list of retail operations that can be performed by user.
                // The terminal should be always set to support non-drawer operation.
                if (context.GetPrincipal() != null && context.GetPrincipal().ShiftId == 0 && operationId != RetailOperation.None && !context.GetPrincipal().IsTerminalAgnostic)
                {
                    // Few operations are added to the white list which are not related to non-drawer operations like:
                    // ActivateDevice, Logon, DeactivateDevice, Logoff and CloseShift.
                    switch (operationId)
                    {
                        case RetailOperation.ActivateDevice:
                        case RetailOperation.ChangePassword:
                        case RetailOperation.ResetPassword:
                        case RetailOperation.LogOn:
                        case RetailOperation.DeactivateDevice:
                        case RetailOperation.LogOff:
                        case RetailOperation.CustomerClear:
                        case RetailOperation.CustomerSearch:
                        case RetailOperation.CustomerAdd:
                        case RetailOperation.CustomerEdit:
                        case RetailOperation.CustomerTransactions:
                        case RetailOperation.CustomerTransactionsReport:
                        case RetailOperation.DatabaseConnectionStatus:
                        case RetailOperation.GiftCardBalance:
                        case RetailOperation.InventoryLookup:
                        case RetailOperation.ItemSearch:
                        case RetailOperation.MinimizePOSWindow:
                        case RetailOperation.PairHardwareStation:
                        case RetailOperation.PriceCheck:
                        case RetailOperation.ShippingAddressSearch:
                        case RetailOperation.ShowJournal:
                        case RetailOperation.ShowBlindClosedShifts:
                        case RetailOperation.Search:
                        case RetailOperation.ExtendedLogOn:
                        case RetailOperation.TimeRegistration:
                        case RetailOperation.ViewReport:
                        case RetailOperation.PrintZ:
                        case RetailOperation.ChangeHardwareStation:
                            break;

                        default:
                            {
                                throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_NonDrawerOperationsOnly, string.Format(@"Operation {0} cannot be executed; Shift is not open.", operationId));
                            }
                    }
                }
            }

            /// <summary>
            /// Checks if user has been elevated for a given operation.
            /// </summary>
            /// <param name="principal">
            /// The principal.
            /// </param>
            /// <param name="operationId">
            /// The operation id.
            /// </param>
            private static void CheckUserIsElevatedForOperation(ICommercePrincipal principal, RetailOperation operationId)
            {
                if (operationId != (int)RetailOperation.None && principal.ElevatedOperation != (int)operationId)
                {
                    RetailLogger.Log.CrtServicesUserAuthenticationServiceElevatedPermissionNotApplicableToRequestedOperation(
                        ((RetailOperation)principal.ElevatedOperation).ToString(),
                        operationId.ToString());

                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, @"Access denied; elevated to perform another operation.");
                }
            }
        }
    }
}