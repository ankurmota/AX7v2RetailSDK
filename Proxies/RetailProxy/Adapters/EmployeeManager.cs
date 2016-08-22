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
    namespace Commerce.RetailProxy.Adapters
    {
        using System;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class EmployeeManager : IEmployeeManager
        {
            public Task<Employee> Create(Employee entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<Employee> Read(string staffId)
            {
                return Task.Run(() => SecurityManager.Create(CommerceRuntimeManager.Runtime).GetEmployeePermissions(staffId));
            }
    
            public Task<PagedResult<Employee>> ReadAll(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetAllStoreEmployees(queryResultSettings));
            }
    
            public Task<Employee> Update(Employee entity)
            {
                throw new NotSupportedException();
            }
    
            public Task Delete(Employee entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<EmployeeActivity>> GetActivities(EmployeeActivitySearchCriteria employeeActivitySearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetEmployeeActivities(employeeActivitySearchCriteria, queryResultSettings));
            }
    
            public Task<PagedResult<EmployeeActivity>> GetManagerActivityView(EmployeeActivitySearchCriteria employeeActivitySearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetManagerActivityView(employeeActivitySearchCriteria, queryResultSettings));
            }
    
            public Task<DateTimeOffset> RegisterActivity(string staffId, int employeeActivityType)
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).RegisterActivity((EmployeeActivityType)employeeActivityType));
            }
    
            public Task<PagedResult<OrgUnit>> GetAccessibleOrgUnits(QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetEmployeeStores(queryResultSettings));
            }
    
            public Task<EmployeeActivity> GetLatestActivity()
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetLatestEmployeeActivity());
            }
    
            public Task<Employee> GetCurrentEmployee()
            {
                return Task.Run(() => Runtime.Client.EmployeeManager.Create(CommerceRuntimeManager.Runtime).GetCurrentEmployee());
            }
        }
    }
}
