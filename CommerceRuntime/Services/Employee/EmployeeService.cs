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
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Employee service class.
        /// </summary>
        public class EmployeeService : IRequestHandler
        {
            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                    typeof(GetStoresByEmployeeServiceRequest),
                    typeof(GetEmployeesServiceRequest)
                };
                }
            }

            /// <summary>
            /// Executes the service request.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>
            /// The response.
            /// </returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }

                Type requestedType = request.GetType();

                if (requestedType == typeof(GetEmployeesServiceRequest))
                {
                    return GetEmployees((GetEmployeesServiceRequest)request);
                }
                else if (requestedType == typeof(GetStoresByEmployeeServiceRequest))
                {
                    return GetStoresByEmployee((GetStoresByEmployeeServiceRequest)request);
                }

                throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
            }

            /// <summary>
            /// Get employees using the request criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria to retrieve employees for.</param>
            /// <returns>GetEmployeesServiceResponse object.</returns>
            private static GetEmployeesServiceResponse GetEmployees(GetEmployeesServiceRequest request)
            {
                if (!string.IsNullOrEmpty(request.StaffId))
                {
                    GetEmployeeDataRequest dataRequest = new GetEmployeeDataRequest(request.StaffId, QueryResultSettings.SingleRecord);
                    Employee employee = request.RequestContext.Execute<SingleEntityDataServiceResponse<Employee>>(dataRequest).Entity;

                    if (employee == null)
                    {
                        return new GetEmployeesServiceResponse();
                    }

                    return new GetEmployeesServiceResponse(employee);
                }
                else
                {
                    EntityDataServiceRequest<Employee> dataRequest = new EntityDataServiceRequest<Employee>(request.QueryResultSettings);
                    var employees = request.RequestContext.Execute<EntityDataServiceResponse<Employee>>(dataRequest).PagedEntityCollection;
                    return new GetEmployeesServiceResponse(employees);
                }
            }

            /// <summary>
            /// Gets the accessible stores of the current employee.
            /// </summary>
            /// <param name="request">Instance of <see cref="GetStoresByEmployeeServiceRequest"/>.</param>
            /// <returns>Instance of <see cref="EntityDataServiceResponse{OrgUnit}"/>.</returns>
            private static EntityDataServiceResponse<OrgUnit> GetStoresByEmployee(GetStoresByEmployeeServiceRequest request)
            {
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                EntityDataServiceResponse<OrgUnit> response = null;

                if (request.RequestContext.Runtime.Configuration.IsMasterDatabaseConnectionString)
                {
                    // If connected to online, we make a RTS call for retrieving all accessible org units.
                    var serviceRequest = new GetEmployeeStoresFromAddressBookRealtimeRequest(request.QueryResultSettings);
                    response = request.RequestContext.Execute<EntityDataServiceResponse<OrgUnit>>(serviceRequest);
                }
                else
                {
                    var dataRequest = new GetEmployeeStoresFromAddressBookDataRequest(request.QueryResultSettings);
                    response = request.RequestContext.Execute<EntityDataServiceResponse<OrgUnit>>(dataRequest);
                }

                return response;
            }
        }
    }
}
