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
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// Encapsulates the workflow required to get the currently logged in employee for a store.
        /// </summary>
        public class GetCurrentEmployeeRequestHandler : SingleRequestHandler<GetCurrentEmployeeRequest, GetCurrentEmployeeResponse>
        {
            /// <summary>
            /// Executes the workflow to get the currently logged in employee.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetCurrentEmployeeResponse Process(GetCurrentEmployeeRequest request)
            {
                ThrowIf.Null(request, "request");

                // authorize employee will return employee and all permissions
                var staffRequest = new StaffAuthorizationServiceRequest(
                    request.RequestContext.Runtime.CurrentPrincipal.UserId,
                    RetailOperation.None);
                var response = this.Context.Execute<StaffAuthorizationServiceResponse>(staffRequest);

                // get the full employee object from the database.
                Employee employee = response.Employee;
                if (employee != null && !string.IsNullOrWhiteSpace(employee.StaffId))
                {
                    var employeePermission = employee.Permissions;

                    QueryResultSettings settings = new QueryResultSettings(new PagingInfo(top: 1));
                    GetEmployeesServiceRequest employeeRequest = new GetEmployeesServiceRequest(employee.StaffId, settings);
                    var employeeResponse = this.Context.Execute<GetEmployeesServiceResponse>(employeeRequest);

                    if (employeeResponse != null)
                    {
                        employee = employeeResponse.Employees.SingleOrDefault();

                        // Set the employee permission as persisted during staff authorization call.
                        employee.Permissions = employeePermission;
                    }

                    // Set the number of days to password expiry on the employee.
                    int passwordExpiryIntervalInDays = 0;
                    int passwordExpiryNotificationThreshold = 0;
                    ChannelConfiguration channelConfiguration = this.Context.GetChannelConfiguration();

                    if (channelConfiguration != null)
                    {
                        passwordExpiryIntervalInDays = channelConfiguration.PasswordExpiryIntervalInDays;
                        passwordExpiryNotificationThreshold = channelConfiguration.PasswordExpiryNotificationThresholdInDays;
                    }

                    employee.NumberOfDaysToPasswordExpiry = CalculateNumberOfDaysToPasswordExpiry(passwordExpiryIntervalInDays, passwordExpiryNotificationThreshold, employee.PasswordLastChangedDateTime);
                }

                return new GetCurrentEmployeeResponse(employee);
            }

            /// <summary>
            /// Calculate the number of days left to password expiry.
            /// </summary>
            /// <param name="passwordExpiryIntervalInDays">The password expiry interval in days.</param>
            /// <param name="passwordExpiryNotificationThreshold">The threshold at which notification is shown to the user.</param>
            /// <param name="passwordLastChangedDateTime">The date time at which the password was last changed.</param>
            /// <returns>The interval, in days, after which the user password will expire if it falls within the notification threshold.</returns>
            private static int CalculateNumberOfDaysToPasswordExpiry(int passwordExpiryIntervalInDays, int passwordExpiryNotificationThreshold, DateTimeOffset passwordLastChangedDateTime)
            {
                if (passwordExpiryIntervalInDays != 0 && passwordExpiryNotificationThreshold != 0)
                {
                    int daysToPasswordExpiry = (passwordLastChangedDateTime.AddDays(passwordExpiryIntervalInDays) - DateTime.UtcNow.Date).Days;
                    if (daysToPasswordExpiry >= 0 && daysToPasswordExpiry <= passwordExpiryNotificationThreshold)
                    {
                        return daysToPasswordExpiry;
                    }
                }

                return 0;
            }
        }
    }
}
