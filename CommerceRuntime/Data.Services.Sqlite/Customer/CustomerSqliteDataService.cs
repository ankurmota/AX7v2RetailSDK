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
        using System;
        using System.Collections.Generic;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
    
        /// <summary>
        /// Customer data requests handler that retrieves the customer information from underlying data storage.
        /// </summary>
        public class CustomerSqliteDataService : IRequestHandler
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
                        typeof(GetCustomerDataRequest),
                        typeof(SearchCustomersDataRequest),
                        typeof(GetCustomerLoyaltyCardsDataRequest),
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
    
                if (requestedType == typeof(GetCustomerDataRequest))
                {
                    response = GetCustomerByAccountNumber((GetCustomerDataRequest)request);
                }
                else if (requestedType == typeof(SearchCustomersDataRequest))
                {
                    response = SearchCustomers((SearchCustomersDataRequest)request);
                }
                else if (requestedType == typeof(GetCustomerLoyaltyCardsDataRequest))
                {
                    response = GetCustomerLoyaltyCards();
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType().ToString()));
                }
    
                return response;
            }
    
            /// <summary>
            /// Gets the customer by account number.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static SingleEntityDataServiceResponse<Customer> GetCustomerByAccountNumber(GetCustomerDataRequest request)
            {
                var procedure = new GetCustomersProcedure(request.RequestContext);
                Customer customer = procedure.GetCustomerByAccountNumber(request.AccountNumber);
    
                return new SingleEntityDataServiceResponse<Customer>(customer);
            }
    
            /// <summary>
            /// Searches the customers with given search conditions.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<GlobalCustomer> SearchCustomers(SearchCustomersDataRequest request)
            {
                var procedure = new SearchCustomersProcedure(request.RequestContext);
                PagedResult<GlobalCustomer> globalCustomers = procedure.SearchCustomers(request.Keyword, request.SearchCurrentCompanyOnly, request.QueryResultSettings);
    
                return new EntityDataServiceResponse<GlobalCustomer>(globalCustomers);
            }
    
            /// <summary>
            /// Gets customer loyalty cards.
            /// </summary>
            /// <returns>The data service response.</returns>
            private static EntityDataServiceResponse<LoyaltyCard> GetCustomerLoyaltyCards()
            {
                // SQLLite does not support loyalty yet. Return an empty list.
                return new EntityDataServiceResponse<LoyaltyCard>(new List<LoyaltyCard>().AsPagedResult());
            }
        }
    }
}
