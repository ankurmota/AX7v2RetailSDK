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
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;
    
        /// <summary>
        /// Encapsulates the workflow required to do user authentication.
        /// </summary>
        public sealed class UserAuthenticationRequestHandler : SingleRequestHandler<UserAuthenticationRequest, UserAuthenticationResponse>
        {
            /// <summary>
            /// Executes the workflow to do user authentication.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override UserAuthenticationResponse Process(UserAuthenticationRequest request)
            {   
                ThrowIf.Null(request, "request");
                Device device = null;
                CommerceIdentity identity;
                Employee employee;
                string deviceId = string.IsNullOrWhiteSpace(request.DeviceId)
                    ? request.RequestContext.GetPrincipal().DeviceNumber
                    : request.DeviceId;

                string deviceToken = string.IsNullOrWhiteSpace(request.DeviceToken)
                    ? request.RequestContext.GetPrincipal().DeviceToken
                    : request.DeviceToken;

                try
                {
                    // Authenticate device only when the device token is specified
                    if (!string.IsNullOrWhiteSpace(deviceToken))
                    {
                        device = AuthenticationHelper.AuthenticateDevice(
                            this.Context,
                            deviceToken);
                    }

                    // User logs on.
                    employee = AuthenticationHelper.AuthenticateAndAuthorizeUser(request, device);

                    identity = new CommerceIdentity(employee, device);

                    // If the request is for elevate operation
                    if (request.RetailOperation != RetailOperation.None)
                    {
                        // Add the Elevation properties to the claim.
                        identity.OriginalUserId = this.Context.GetPrincipal().UserId;
                        identity.ElevatedRetailOperation = request.RetailOperation;

                        // successful manager override for operation with id and operator with id
                        var message = string.Format(
                            "Manager with id '{0}' has approved override for operation with id '{1}' to the operator with id '{2}'.",
                            request.StaffId,
                            identity.ElevatedRetailOperation,
                            identity.OriginalUserId);
                        LogAuditEntry(request.RequestContext, "ElevateUser", message);
                    }

                    return new UserAuthenticationResponse(employee, device, identity);
                }
                catch (Exception exception)
                {
                    RetailLogger.Log.CrtWorkflowUserAuthenticationRequestHandlerFailure(request.StaffId, deviceId, exception);
                    throw;
                }                   
            }
    
            /// <summary>
            /// Writes an entry into the audit table.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="source">The log source.</param>
            /// <param name="value">The log entry.</param>
            /// <param name="logTraceLevel">The log trace level.</param>
            private static void LogAuditEntry(RequestContext context, string source, string value, AuditLogTraceLevel logTraceLevel = AuditLogTraceLevel.Trace)
            {
                var auditLogDataRequest = new InsertAuditLogServiceRequest(source, value, logTraceLevel, unchecked((int)context.RequestTimer.ElapsedMilliseconds));
                context.Execute<NullResponse>(auditLogDataRequest);
            }
        }
    }
}
