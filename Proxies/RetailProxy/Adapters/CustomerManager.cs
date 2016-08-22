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
        using System.Linq;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Runtime = Microsoft.Dynamics.Commerce.Runtime;
    
        internal class CustomerManager : ICustomerManager
        {
            public Task<Customer> Create(Customer entity)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).CreateCustomer(entity));
            }
    
            public Task<Customer> Read(string accountNumber)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).GetCustomer(accountNumber));
            }
    
            public Task<PagedResult<Customer>> ReadAll(QueryResultSettings queryResultSettings)
            {
                // Reading all customers without any filter is a performance concern.
                throw new NotSupportedException();
            }
    
            public Task<Customer> Update(Customer entity)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).UpdateCustomer(entity));
            }
    
            public Task Delete(Customer entity)
            {
                throw new NotSupportedException();
            }
    
            public Task<PagedResult<GlobalCustomer>> Search(CustomerSearchCriteria customerSearchCriteria, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).SearchCustomers(customerSearchCriteria, queryResultSettings));
            }
    
            public Task<PagedResult<SalesOrder>> GetOrderHistory(string accountNumber, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).GetOrderHistory(accountNumber, queryResultSettings));
            }
    
            public Task<PagedResult<LoyaltyCard>> GetLoyaltyCards(string customerId, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.LoyaltyManager.Create(CommerceRuntimeManager.Runtime).GetCustomerLoyaltyCards(customerId, queryResultSettings));
            }

            public Task<PagedResult<PurchaseHistory>> GetPurchaseHistory(string accountNumber, QueryResultSettings queryResultSettings)
            {
                return Task.Run(() => Runtime.Client.CustomerManager.Create(CommerceRuntimeManager.Runtime).GetPurchaseHistory(accountNumber, queryResultSettings));
            }
        }
    }
}
