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
        using System.Collections.ObjectModel;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
    
        /// <summary>
        /// Encapsulates the workflow required to get employee permission.
        /// </summary>
        public sealed class GetEmployeePermissionRequestHandler : SingleRequestHandler<GetEmployeePermissionsRequest, GetEmployeePermissionsResponse>
        {
            /// <summary>
            /// Executes the workflow to do user authentication.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            protected override GetEmployeePermissionsResponse Process(GetEmployeePermissionsRequest request)
            {
                ThrowIf.Null(request, "request");
                GetEmployeesServiceRequest getEmployeeRequest = new GetEmployeesServiceRequest(request.StaffId, QueryResultSettings.SingleRecord);
                GetEmployeesServiceResponse employeeResponse = this.Context.Execute<GetEmployeesServiceResponse>(getEmployeeRequest);
                Employee employee = employeeResponse.Employees.SingleOrDefault();
    
                if (employee == null)
                {
                    return new GetEmployeePermissionsResponse(employee);
                }
    
                // Check if the requested Employee object is same as logged-on user. 
                // If not, check staff have manager permission.
                // If the staff is not manager, do not return permissions
                if (!string.Equals(request.StaffId, this.Context.GetPrincipal().UserId))
                {
                    try
                    {
                        this.Context.Execute<Response>(new CheckAccessIsManagerServiceRequest());
                    }
                    catch (UserAuthorizationException)
                    {
                        return new GetEmployeePermissionsResponse(employee);
                    }
                }
    
                GetEmployeePermissionsDataRequest permissionsDataRequest = new GetEmployeePermissionsDataRequest(request.StaffId, new ColumnSet());
                employee.Permissions = this.Context.Execute<SingleEntityDataServiceResponse<EmployeePermissions>>(permissionsDataRequest).Entity;
                return new GetEmployeePermissionsResponse(employee);
            }
        }
    }
}
