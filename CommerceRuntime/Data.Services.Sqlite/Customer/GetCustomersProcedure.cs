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
    namespace Commerce.Runtime.DataServices.Sqlite
    {
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Helper procedure class to get customers.
        /// </summary>
        public sealed class GetCustomersProcedure
        {
            /// <summary>
            /// The empty record identifier value.
            /// </summary>
            private const long EmptyRecordId = 0;
    
            private const string CustomerPostalAddressesView = "CUSTOMERPOSTALADDRESSESVIEW";
            private const string CustomersView = "CUSTOMERSVIEW";
    
            /// <summary>
            /// Initializes a new instance of the <see cref="GetCustomersProcedure"/> class.
            /// </summary>
            /// <param name="context">The request context.</param>
            public GetCustomersProcedure(RequestContext context)
            {
                this.Context = context;
            }
    
            private RequestContext Context { get; set; }
    
            /// <summary>
            /// Gets the customer by account number.
            /// </summary>
            /// <param name="accountNumber">The account number of the customer to retrieve.</param>
            /// <returns>
            /// The customer.
            /// </returns>
            public Customer GetCustomerByAccountNumber(string accountNumber)
            {
                ThrowIf.NullOrWhiteSpace(accountNumber, "accountNumber");
    
                return this.GetCustomers(accountNumber, QueryResultSettings.SingleRecord).SingleOrDefault();
            }
    
            /// <summary>
            /// Gets the customers.
            /// </summary>
            /// <param name="accountNumber">Optional account number of the customer to retrieve.</param>
            /// <param name="settings">The query result settings.</param>
            /// <returns>
            /// A collection of customers.
            /// </returns>
            public PagedResult<Customer> GetCustomers(string accountNumber, QueryResultSettings settings)
            {
                ThrowIf.Null(settings, "settings");
    
                PagedResult<Customer> customers = this.GetCustomersView(accountNumber, settings);
    
                return customers;
            }
    
            /// <summary>
            /// Get addresses for the customer that meets the supplied criteria.
            /// </summary>
            /// <param name="databaseContext">The SQLite database context.</param>
            /// <param name="customer">Customer to retrieve addresses for.</param>
            /// <returns>
            /// A collection of addresses for the specified customer.
            /// </returns>
            private static ReadOnlyCollection<Address> GetAddresses(SqliteDatabaseContext databaseContext, Customer customer)
            {
                ThrowIf.Null(customer, "customer");
    
                // don't fetch addresses if the customer doesn't have a party id
                if (customer.DirectoryPartyRecordId == 0)
                {
                    return new ReadOnlyCollection<Address>(new Collection<Address>());
                }
    
                var addressesQuery = new SqlPagedQuery(QueryResultSettings.AllRecords)
                {
                    From = CustomerPostalAddressesView,
                    Where = "DIRPARTYTABLERECID = @party",
                };
    
                addressesQuery.Parameters["@party"] = customer.DirectoryPartyRecordId;
    
                ReadOnlyCollection<Address> addresses = databaseContext.ReadEntity<Address>(addressesQuery).Results;
                return addresses;
            }
    
            private PagedResult<Customer> GetCustomersView(string accountNumber, QueryResultSettings settings)
            {
                PagedResult<Customer> customers = new List<Customer>().AsPagedResult();
                using (SqliteDatabaseContext databaseContext = new SqliteDatabaseContext(this.Context))
                {
                    var customersQuery = new SqlPagedQuery(settings)
                    {
                        From = CustomersView,
                    };
    
                    string operand = string.Empty;
    
                    if (!string.IsNullOrWhiteSpace(accountNumber))
                    {
                        customersQuery.Where = string.Format("{0}{1}ACCOUNTNUMBER = @accountNumber", customersQuery.Where, operand);
                        customersQuery.Parameters["@accountNumber"] = accountNumber;
                        operand = " and ";
                    }
    
                    customersQuery.Where = string.Format("{0}{1}DATAAREAID = @dataAreaId", customersQuery.Where, operand);
                    customersQuery.Parameters["@dataAreaId"] = databaseContext.DataAreaId;
    
                    customers = databaseContext.ReadEntity<Customer>(customersQuery);
    
                    foreach (Customer customer in customers.Results)
                    {
                        // we don't need to use any pagination for this call.
                        customer.Addresses = GetAddresses(databaseContext, customer);
                    }
                }
    
                // process images if any
                var dataServiceRequest = new ProcessCustomerImagesDataRequest(customers.Results);
                this.Context.Runtime.Execute<NullResponse>(dataServiceRequest, this.Context);
    
                return customers;
            }
        }
    }
}
