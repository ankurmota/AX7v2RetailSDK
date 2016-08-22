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
    namespace Commerce.Runtime.TransactionService
    {
        using System;
        using System.Collections.Generic;
        using System.Globalization;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;

        /// <summary>
        /// The customer transaction service implementation.
        /// </summary>
        public class CustomerTransactionServiceRequestHandler : IRequestHandler
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
                    typeof(GetCustomerBalanceRealtimeRequest),
                    typeof(ValidateCustomerAccountPaymentRealtimeRequest),
                    typeof(SearchCustomersRealtimeRequest),
                    typeof(DownloadCustomerRealtimeRequest),
                    typeof(DownloadPartyRealtimeRequest),
                    typeof(NewCustomerRealtimeRequest),
                    typeof(NewCustomerFromDirectoryPartyRealtimeRequest),
                    typeof(SaveCustomerRealtimeRequest),
                    typeof(DeactivateAddressRealtimeRequest),
                    typeof(CreateAddressRealtimeRequest),
                    typeof(UpdateAddressRealtimeRequest),
                    typeof(GetOrderHistoryRealtimeRequest),
                    typeof(GetPurchaseHistoryRealtimeRequest),
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

                Type requestType = request.GetType();

                Response response;
                if (requestType == typeof(GetCustomerBalanceRealtimeRequest))
                {
                    response = GetCustomerBalance((GetCustomerBalanceRealtimeRequest)request);
                }
                else if (requestType == typeof(ValidateCustomerAccountPaymentRealtimeRequest))
                {
                    response = ValidateCustomerAccountPayment((ValidateCustomerAccountPaymentRealtimeRequest)request);
                }
                else if (requestType == typeof(SearchCustomersRealtimeRequest))
                {
                    response = SearchCustomers((SearchCustomersRealtimeRequest)request);
                }
                else if (requestType == typeof(DownloadCustomerRealtimeRequest))
                {
                    response = DownloadCustomer((DownloadCustomerRealtimeRequest)request);
                }
                else if (requestType == typeof(DownloadPartyRealtimeRequest))
                {
                    response = DownloadParty((DownloadPartyRealtimeRequest)request);
                }
                else if (requestType == typeof(NewCustomerRealtimeRequest))
                {
                    response = NewCustomer((NewCustomerRealtimeRequest)request);
                }
                else if (requestType == typeof(NewCustomerFromDirectoryPartyRealtimeRequest))
                {
                    response = NewCustomerFromDirParty((NewCustomerFromDirectoryPartyRealtimeRequest)request);
                }
                else if (requestType == typeof(SaveCustomerRealtimeRequest))
                {
                    response = UpdateCustomer((SaveCustomerRealtimeRequest)request);
                }
                else if (requestType == typeof(DeactivateAddressRealtimeRequest))
                {
                    response = DeactivateAddress((DeactivateAddressRealtimeRequest)request);
                }
                else if (requestType == typeof(CreateAddressRealtimeRequest))
                {
                    response = CreateAddress((CreateAddressRealtimeRequest)request);
                }
                else if (requestType == typeof(UpdateAddressRealtimeRequest))
                {
                    response = UpdateAddress((UpdateAddressRealtimeRequest)request);
                }
                else if (requestType == typeof(GetOrderHistoryRealtimeRequest))
                {
                    response = GetOrderHistory((GetOrderHistoryRealtimeRequest)request);
                }
                else if (requestType == typeof(GetPurchaseHistoryRealtimeRequest))
                {
                    response = GetPurchaseHistory((GetPurchaseHistoryRealtimeRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Request type '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Gets the customer purchase history data.
            /// </summary>
            /// <param name="request">The real time request to get customer purchase history.</param>
            /// <returns>A collection of purchase history.</returns>
            private static GetPurchaseHistoryRealtimeResponse GetPurchaseHistory(GetPurchaseHistoryRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
                PagedResult<PurchaseHistory> results = client.GetPurchaseHistory(request.CustomerAccountNumber, request.RequestContext.LanguageId, request.StartDateTime, request.QueryResultSettings);

                return new GetPurchaseHistoryRealtimeResponse(results);
            }

            /// <summary>
            /// Gets the customer balance from AX.
            /// </summary>
            /// <param name="request">The service request to get customer balance.</param>
            /// <returns>The service response.</returns>
            private static GetCustomerBalanceRealtimeResponse GetCustomerBalance(GetCustomerBalanceRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);
                string orgUnitNumber = request.RequestContext.GetOrgUnit().OrgUnitNumber;
                string currency = request.RequestContext.GetOrgUnit().Currency;

                CustomerBalances customerBalance = client.GetCustomerBalance(request.AccountNumber, currency, orgUnitNumber);

                return new GetCustomerBalanceRealtimeResponse(customerBalance);
            }

            /// <summary>
            /// Validates Customer Account Payment from AX.
            /// </summary>
            /// <param name="request">The service request to validate customer account payment.</param>
            /// <returns>The service response.</returns>
            private static NullResponse ValidateCustomerAccountPayment(ValidateCustomerAccountPaymentRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                client.ValidateCustomerAccountPayment(request.AccountNumber, request.Amount, request.CurrencyCode);

                return new NullResponse();
            }

            /// <summary>
            /// Perform a keyword search in AX for customers.
            /// </summary>
            /// <param name="request">A service request specifying the keywords and query settings search using.</param>
            /// <returns>A service response containing the query results.</returns>
            private static EntityDataServiceResponse<GlobalCustomer> SearchCustomers(SearchCustomersRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                var keywords = request.Keywords;
                var paging = request.QueryResultSettings.Paging;
                var searchResults = client.SearchCustomers(keywords, paging);

                return new EntityDataServiceResponse<GlobalCustomer>(searchResults);
            }

            /// <summary>
            /// Find and retrieve a customer customer from AX and add them to the DB.
            /// </summary>
            /// <param name="request">A service request specifying the customer to retrieve.</param>
            /// <returns>An empty response.</returns>
            private static NullResponse DownloadCustomer(DownloadCustomerRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                string packageData = client.GetCustomerDataPackage(
                                        request.RecordId,
                                        request.AccountNumber,
                                        request.DirectoryPartyRecordId,
                                        request.RequestContext.GetPrincipal().ChannelId);

                // Apply the retreived data package
                var packageWriteRequest = new ProcessDataPackageServiceRequest(packageData);
                request.RequestContext.Execute<NullResponse>(packageWriteRequest);

                return new NullResponse();
            }

            /// <summary>
            /// Find and retrieve a customer customer from AX and add them to the DB.
            /// </summary>
            /// <param name="request">A service request specifying the customer to retrieve.</param>
            /// <returns>An empty response.</returns>
            private static NullResponse DownloadParty(DownloadPartyRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                string packageData = client.GetPartyDataPackage(
                                        request.PartyNumber,
                                        request.RequestContext.GetPrincipal().ChannelId);

                // Apply the retreived data package
                var packageWriteRequest = new ProcessDataPackageServiceRequest(packageData);
                request.RequestContext.Execute<NullResponse>(packageWriteRequest);

                return new NullResponse();
            }

            /// <summary>
            /// Creates a new customer in AX.
            /// </summary>
            /// <param name="request">The service request to create a new customer.</param>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse NewCustomer(NewCustomerRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                Customer customer = request.CustomerToSave;
                client.NewCustomer(ref customer, request.ChannelId);

                return new SaveCustomerRealtimeResponse(customer);
            }

            /// <summary>
            /// Creates a new customer from directory in AX.
            /// </summary>
            /// <param name="request">The service request to create a new customer from directory.</param>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse NewCustomerFromDirParty(NewCustomerFromDirectoryPartyRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                Customer customer = request.CustomerToSave;
                client.NewCustomerFromDirParty(ref customer, request.ChannelId);

                return new SaveCustomerRealtimeResponse(customer);
            }

            /// <summary>
            /// Updates a customer in AX.
            /// </summary>
            /// <param name="request">The service request to update a customer.</param>
            /// <returns>The service response.</returns>
            private static SaveCustomerRealtimeResponse UpdateCustomer(SaveCustomerRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                Customer customer = client.UpdateCustomer(request.CustomerToSave);

                return new SaveCustomerRealtimeResponse(customer);
            }

            /// <summary>
            /// Deactivates an address in AX.
            /// </summary>
            /// <param name="request">The service request to deactivate an address.</param>
            /// <returns>The service response.</returns>
            private static NullResponse DeactivateAddress(DeactivateAddressRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                client.DeactivateAddress(request.AddressId, request.CustomerId);

                return new NullResponse();
            }

            /// <summary>
            /// Creates a new address in AX.
            /// </summary>
            /// <param name="request">The service request to create a new address.</param>
            /// <returns>The service response.</returns>
            private static CreateAddressRealtimeResponse CreateAddress(CreateAddressRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                Address address = request.Address;
                client.CreateAddress(request.Customer, ref address);

                return new CreateAddressRealtimeResponse(address);
            }

            /// <summary>
            /// Updates an address in AX.
            /// </summary>
            /// <param name="request">The service request to update an address.</param>
            /// <returns>The service response.</returns>
            private static UpdateAddressRealtimeResponse UpdateAddress(UpdateAddressRealtimeRequest request)
            {
                var client = new TransactionServiceClient(request.RequestContext);

                Customer customer = request.Customer;
                Address address = request.Address;
                client.UpdateAddress(ref customer, ref address);

                return new UpdateAddressRealtimeResponse(customer, address);
            }

            private static GetOrderHistoryRealtimeResponse GetOrderHistory(GetOrderHistoryRealtimeRequest request)
            {
                ThrowIf.Null(request, "request");

                var transactionServiceClient = new TransactionServiceClient(request.RequestContext);
                var orders = transactionServiceClient.GetOrderHistory(request.CustomerAccountNumber, request.StartDateTime, request.QueryResultSettings);

                return new GetOrderHistoryRealtimeResponse(orders);
            }
        }
    }
}