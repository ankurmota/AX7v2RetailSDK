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
    namespace Commerce.Runtime.DataServices.Common
    {
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using SqlServer.DataServices;

        /// <summary>
        /// Customer data services that contains methods to retrieve the customer information from underlying data storage.
        /// </summary>
        public class CustomerDataService : IRequestHandler
        {
            private const string CustomerAccountNumberVariableName = "@nvc_CustAccount";
            private const string StartDateVariableName = "@startDateTime";
            private const string OrderHistoryView = "OrderHistoryView";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(GetAddressDataRequest),
                        typeof(GetAddressesDataRequest),
                        typeof(GetCustomerGroupsDataRequest),
                        typeof(GetCustomerWithPartyNumberDataRequest),
                        typeof(ProcessCustomerImagesDataRequest),
                        typeof(GetOrderHistoryDataRequest),
                    };
                }
            }
    
            /// <summary>
            /// Represents the entry point of the request handler.
            /// </summary>
            /// <param name="request">The incoming request message.</param>
            /// <returns>The outgoing response message.</returns>
            public Response Execute(Request request)
            {
                if (request == null)
                {
                    throw new ArgumentNullException("request");
                }
    
                Type requestedType = request.GetType();
                Response response;
    
                if (requestedType == typeof(GetAddressDataRequest))
                {
                    response = GetAddress((GetAddressDataRequest)request);
                }
                else if (requestedType == typeof(GetAddressesDataRequest))
                {
                    response = GetAddresses((GetAddressesDataRequest)request);
                }
                else if (requestedType == typeof(GetCustomerGroupsDataRequest))
                {
                    response = GetCustomerGroups((GetCustomerGroupsDataRequest)request);
                }
                else if (requestedType == typeof(GetCustomerWithPartyNumberDataRequest))
                {
                    response = GetInitializedCustomerFromGlobalCustomer((GetCustomerWithPartyNumberDataRequest)request);
                }
                else if (requestedType == typeof(ProcessCustomerImagesDataRequest))
                {
                    response = ProcessCustomerImages((ProcessCustomerImagesDataRequest)request);
                }
                else if (requestedType == typeof(GetOrderHistoryDataRequest))
                {
                    response = GetOrderHistory((GetOrderHistoryDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the customer data manager instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of <see cref="CustomerDataManager"/></returns>
            private static CustomerDataManager GetDataManagerInstance(RequestContext context)
            {
                return new CustomerDataManager(context);
            }
    
            /// <summary>
            /// Gets all addresses that meet the supplied criteria.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<Address> GetAddresses(GetAddressesDataRequest request)
            {
                CustomerDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                PagedResult<Address> addresses = dataManager.GetAddresses(request.AddressRecordIds);
    
                return new EntityDataServiceResponse<Address>(addresses);
            }
    
            /// <summary>
            /// Gets a single address that meets the supplied criteria.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static SingleEntityDataServiceResponse<Address> GetAddress(GetAddressDataRequest request)
            {
                CustomerDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                Address address = dataManager.GetAddress(request.RecordId, request.CustomerRecordId, request.ColumnSet);
    
                return new SingleEntityDataServiceResponse<Address>(address);
            }
    
            /// <summary>
            /// Gets the customer groups from retail store.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<CustomerGroup> GetCustomerGroups(GetCustomerGroupsDataRequest request)
            {
                CustomerDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                PagedResult<CustomerGroup> customerGroups = dataManager.GetCustomerGroups(request.QueryResultSettings);
    
                return new EntityDataServiceResponse<CustomerGroup>(customerGroups);
            }
    
            /// <summary>
            /// Gets an customer initialized with party information.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static SingleEntityDataServiceResponse<Customer> GetInitializedCustomerFromGlobalCustomer(GetCustomerWithPartyNumberDataRequest request)
            {
                CustomerDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                Customer customer = dataManager.GetInitializedCustomerFromGlobalCustomer(request.PartyNumber);
    
                return new SingleEntityDataServiceResponse<Customer>(customer);
            }
    
            /// <summary>
            /// Processes images for customers.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static NullResponse ProcessCustomerImages(ProcessCustomerImagesDataRequest request)
            {
                CustomerDataManager dataManager = GetDataManagerInstance(request.RequestContext);
                dataManager.ProcessCustomerImages(request.Customers);
    
                return new NullResponse();
            }

            /// <summary>
            /// Gets order history for customer.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<SalesOrder> GetOrderHistory(GetOrderHistoryDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.NullOrWhiteSpace(request.CustomerAccountNumber, "request.CustomerAccountNumber");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                // Build the where clause
                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = OrderHistoryView,
                    Where = RetailTransactionTableSchema.CustomerIdColumn + " = " + CustomerAccountNumberVariableName + " AND " +
                            RetailTransactionTableSchema.CreatedDateTimeColumn + " >= " + StartDateVariableName,
                    OrderBy = RetailTransactionTableSchema.CreatedDateTimeColumn + " DESC"
                };

                query.Parameters[CustomerAccountNumberVariableName] = request.CustomerAccountNumber;
                query.Parameters[StartDateVariableName] = request.StartDateTime;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    PagedResult<SalesOrder> results = databaseContext.ReadEntity<SalesOrder>(query);
                    SalesTransactionDataService.FillSalesOrderMembers(results.Results, true, request.RequestContext);
                    return new EntityDataServiceResponse<SalesOrder>(results);
                }
            }
        }
    }
}
