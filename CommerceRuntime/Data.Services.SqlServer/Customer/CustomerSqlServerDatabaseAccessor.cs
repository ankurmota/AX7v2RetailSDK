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
    namespace Commerce.Runtime.DataServices.SqlServer
    {
        using System.Collections.ObjectModel;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Customer SQL database accessor class.
        /// </summary>
        public sealed class CustomerSqlServerDatabaseAccessor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerSqlServerDatabaseAccessor"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public CustomerSqlServerDatabaseAccessor(RequestContext context)
            {
                this.Context = context;
            }
    
            private RequestContext Context { get; set; }
    
            /// <summary>
            /// Gets the customer by account number.
            /// </summary>
            /// <param name="accountNumber">The account number of the customer to retrieved.</param>
            /// <returns>
            /// The customer.
            /// </returns>
            public Customer GetCustomerByAccountNumber(string accountNumber)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.GetCustomerByAccountNumber(accountNumber);
            }
    
            /// <summary>
            /// Save customer account activation request to channel DB.
            /// </summary>
            /// <param name="email">E-mail address.</param>
            /// <param name="activationToken">Activation token (a GUID).</param>
            /// <returns>Result for linking external identity to an existing customer.</returns>
            public LinkToExistingCustomerResult FinalizeLinkToExistingCustomer(string email, string activationToken)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.FinalizeLinkToExistingCustomer(email, activationToken);
            }
    
            /// <summary>
            /// Creates or updates a customer.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <returns>Updated customer.</returns>
            public Customer CreateOrUpdateCustomer(Customer customer)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.CreateOrUpdateCustomer(customer);
            }
    
            /// <summary>
            /// Creates or updates an asynchronous customer.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <returns>Updated customer.</returns>
            public Customer CreateOrUpdateAsyncCustomer(Customer customer)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.CreateOrUpdateAsyncCustomer(customer);
            }
    
            /// <summary>
            /// Creates or updates an asynchronous customer address.
            /// </summary>
            /// <param name="customer">The customer data.</param>
            /// <param name="address">The customer address.</param>
            /// <returns>Updated address.</returns>
            public Address CreateOrUpdateAsyncCustomerAddress(Customer customer, Address address)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.CreateOrUpdateAsyncCustomerAddress(customer, address);
            }

            /// <summary>
            /// Initiates link up of an external user identifier with an existing customer.
            /// </summary>
            /// <param name="emailAddress">The email address where the activation token has been sent.</param>
            /// <param name="activationToken">Activation token (a GUID).</param>
            /// <param name="externalIdentityId">The external identity identifier.</param>
            /// <param name="externalIdentityIssuer">The external identity issuer.</param>
            /// <param name="customerId">The identifier for existing customer.</param>
            /// <returns>Result for linking external identity to an existing customer.</returns>
            public LinkToExistingCustomerResult InitiateLinkToExistingCustomer(string emailAddress, string activationToken, string externalIdentityId, string externalIdentityIssuer, string customerId)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.InitiateLinkToExistingCustomer(emailAddress, activationToken, externalIdentityId, externalIdentityIssuer, customerId);
            }

            /// <summary>
            /// Unlinks the external identity from the customer account.
            /// </summary>
            /// <param name="externalIdentityId">The external identity identifier.</param>
            /// <param name="externalIdentityIssuer">The external identity issuer.</param>
            /// <param name="customerId">The customer identifier.</param>
            public void UnlinkExternalIdentityFromCustomer(string externalIdentityId, string externalIdentityIssuer, string customerId)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                dataManager.UnlinkExternalIdentityFromCustomer(externalIdentityId, externalIdentityIssuer, customerId);
            }

            /// <summary>
            /// Validate the account activation request.
            /// </summary>
            /// <param name="email">E-mail address.</param>
            /// <param name="activationToken">Activation token (a GUID).</param>
            /// <returns>True if the request is valid; otherwise return false.</returns>
            public bool ValidateAccountActivationRequest(string email, string activationToken)
            {
                CustomerDataManager dataManager = this.GetDataManagerInstance();
                return dataManager.ValidateAccountActivationRequest(email, activationToken);
            }
    
            /// <summary>
            /// Gets the customer data manager instance.
            /// </summary>
            /// <returns>An instance of <see cref="CustomerDataManager"/></returns>
            private CustomerDataManager GetDataManagerInstance()
            {
                return new CustomerDataManager(this.Context);
            }
        }
    }
}
