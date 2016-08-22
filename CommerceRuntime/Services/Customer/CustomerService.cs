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
        using Microsoft.Dynamics.Commerce.Runtime.Framework;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.RealtimeServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Services.Messages;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Customer Service class.
        /// </summary>
        public class CustomerService : IRequestHandler
        {
            /// <summary>
            /// The separator used when generating the key.
            /// </summary>
            private const string Separator = "@";

            /// <summary>
            /// Enumeration of Customer operations used in the Customer Service.
            /// </summary>
            private enum CustomerOperations
            {
                UpdateCustomer = 0,
                CreateCustomerInAX,
                CreateCustomerInCRT,
                CreateAddressInAX,
                UpdateCustomerInAX,
                UpdateCustomerInCRT,
                UpdateAddressInAX,
                DeactivateAddressInAX,
                InitiateLinkToExistingCustomer,
                FinalizeLinkToExistingCustomer,
                CreateAsyncCustomerAddress,
                UnlinkFromExistingCustomer
            }

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(SaveCustomerServiceRequest),
                        typeof(GetCustomersServiceRequest),
                        typeof(CustomersSearchServiceRequest),
                        typeof(GetCustomerGroupsServiceRequest),
                        typeof(InitiateLinkToExistingCustomerServiceRequest),
                        typeof(FinalizeLinkToExistingCustomerServiceRequest),
                        typeof(UnlinkFromExistingCustomerServiceRequest),
                        typeof(GetCustomerBalanceServiceRequest),
                        typeof(GetOrderHistoryServiceRequest),
                        typeof(GetValidatedCustomerAccountNumberServiceRequest),
                        typeof(GetPurchaseHistoryServiceRequest)
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

                Response response;
                Type requestedType = request.GetType();

                if (requestedType == typeof(SaveCustomerServiceRequest))
                {
                    response = SaveCustomer((SaveCustomerServiceRequest)request);
                }
                else if (requestedType == typeof(GetCustomersServiceRequest))
                {
                    response = GetCustomers((GetCustomersServiceRequest)request);
                }
                else if (requestedType == typeof(CustomersSearchServiceRequest))
                {
                    response = SearchCustomers((CustomersSearchServiceRequest)request);
                }
                else if (requestedType == typeof(GetCustomerGroupsServiceRequest))
                {
                    response = GetCustomerGroups((GetCustomerGroupsServiceRequest)request);
                }
                else if (requestedType == typeof(InitiateLinkToExistingCustomerServiceRequest))
                {
                    response = InitiateLinkToExistingCustomer((InitiateLinkToExistingCustomerServiceRequest)request);
                }
                else if (requestedType == typeof(FinalizeLinkToExistingCustomerServiceRequest))
                {
                    response = FinalizeLinkToExistingCustomer((FinalizeLinkToExistingCustomerServiceRequest)request);
                }
                else if (requestedType == typeof(UnlinkFromExistingCustomerServiceRequest))
                {
                    response = UnlinkFromExistingCustomer((UnlinkFromExistingCustomerServiceRequest)request);
                }
                else if (requestedType == typeof(GetCustomerBalanceServiceRequest))
                {
                    response = GetBalance((GetCustomerBalanceServiceRequest)request);
                }
                else if (requestedType == typeof(GetOrderHistoryServiceRequest))
                {
                    response = GetOrderHistory((GetOrderHistoryServiceRequest)request);
                }
                else if (requestedType == typeof(GetValidatedCustomerAccountNumberServiceRequest))
                {
                    response = ValidateCustomerAccountNumber((GetValidatedCustomerAccountNumberServiceRequest)request);
                }
                else if (requestedType == typeof(GetPurchaseHistoryServiceRequest))
                {
                    response = GetPurchaseHistory((GetPurchaseHistoryServiceRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            /// <summary>
            /// Merges two collections of order records.
            /// </summary>
            /// <param name="remoteResults">The remote order collection.</param>
            /// <param name="localResults">The local order collection.</param>
            /// <returns>A merged order collection.</returns>
            internal static IEnumerable<SalesOrder> MergeOrderRecords(IEnumerable<SalesOrder> remoteResults, IEnumerable<SalesOrder> localResults)
            {
                IEnumerable<SalesOrder> mergedResults = MultiDataSourcesPagingHelper.MergeResults<SalesOrder, SalesOrder>(
                    remoteResults,
                    localResults,
                    GetOrderMergingKey,
                    new OrderComparer());

                return mergedResults;
            }

            /// <summary>
            /// Merges two collections of purchase history records.
            /// </summary>
            /// <param name="remoteResults">The remote purchase history collection.</param>
            /// <param name="localResults">The local purchase history collection.</param>
            /// <returns>A merged purchase history collection.</returns>
            internal static IEnumerable<PurchaseHistory> MergePurchaseHistoryRecords(IEnumerable<PurchaseHistory> remoteResults, IEnumerable<PurchaseHistory> localResults)
            {
                IEnumerable<PurchaseHistory> mergedResults = MultiDataSourcesPagingHelper.MergeResults<PurchaseHistory, string>(
                    remoteResults,
                    localResults,
                    GetPurchaseHistoryMergingKey);
                return mergedResults;
            }

            private static GetPurchaseHistoryServiceResponse GetPurchaseHistory(GetPurchaseHistoryServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");

                // Async customers do not have purchase histories.
                if (CustomerService.IsAsyncCustomer(request.RequestContext, request.CustomerAccountNumber))
                {
                    return new GetPurchaseHistoryServiceResponse(PagedResult<PurchaseHistory>.Empty());
                }

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.CustomerAccountNumber, throwOnValidationFailure: false);
                var validatedCustomerAccountNumberResponse = request.RequestContext.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);

                if (validatedCustomerAccountNumberResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.CustomerAccountNumber = validatedCustomerAccountNumberResponse.ValidatedAccountNumber;
                }

                // Adjust the paging.
                QueryResultSettings settings = request.QueryResultSettings;
                PagingInfo adjustedPaging = MultiDataSourcesPagingHelper.GetAdjustedPaging(settings.Paging);
                QueryResultSettings adjustedQueryResultSettings = new QueryResultSettings(settings.ColumnSet, adjustedPaging, settings.Sorting, settings.ChangeTracking);

                if (!settings.Sorting.IsSpecified)
                {
                    settings.Sorting.Add(new SortColumn(PurchaseHistory.DatePurchasedColumn, isDescending: true));
                    settings.Sorting.Add(new SortColumn(PurchaseHistory.ItemIdColumn));
                }

                // Set the start date time.
                DateTimeOffset startDateTime = CalculateStarteDateTime(request.RequestContext);

                // Get the local data.
                var dataRequest = new GetPurchaseHistoryDataRequest(request.CustomerAccountNumber, startDateTime, adjustedQueryResultSettings);
                PagedResult<PurchaseHistory> localResults = request.RequestContext.Execute<EntityDataServiceResponse<PurchaseHistory>>(dataRequest).PagedEntityCollection;

                // Get the data from HQ.
                var realtimeRequest = new GetPurchaseHistoryRealtimeRequest(request.CustomerAccountNumber, startDateTime, adjustedQueryResultSettings);
                PagedResult<PurchaseHistory> remoteResults = request.RequestContext.Execute<GetPurchaseHistoryRealtimeResponse>(realtimeRequest).Results;

                // For the HQ product images, we have to process them to ensure the urls are expanded as required.
                foreach (PurchaseHistory purchaseHistory in remoteResults.Results)
                {
                    IEnumerable<MediaLocation> processedImage = RichMediaHelper.PopulateProductMediaLocation(purchaseHistory.ItemId, purchaseHistory.ImageUrl, request.RequestContext.LanguageId);
                    if (processedImage != null && processedImage.Any())
                    {
                        purchaseHistory.ImageUrl = processedImage.FirstOrDefault().Uri;
                    }
                }

                // Get the paged results from both of the result sets.
                IEnumerable<PurchaseHistory> mergedResults = CustomerService.MergePurchaseHistoryRecords(remoteResults.Results, localResults.Results);
                PagedResult<PurchaseHistory> pagedResults = MultiDataSourcesPagingHelper.GetPagedResult(mergedResults, settings.Paging, settings.Sorting);
                return new GetPurchaseHistoryServiceResponse(pagedResults);
            }

            /// <summary>
            /// Ensures that customer account number is valid.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static GetValidatedCustomerAccountNumberServiceResponse ValidateCustomerAccountNumber(GetValidatedCustomerAccountNumberServiceRequest request)
            {
                if (!request.RequestContext.GetPrincipal().IsCustomer && !request.RequestContext.GetPrincipal().IsAnonymous)
                {
                    return new GetValidatedCustomerAccountNumberServiceResponse(request.AccountNumber, isCustomerAccountNumberInContextDifferent: false);
                }

                string accountOnPrincipal = request.RequestContext.GetPrincipal().UserId ?? string.Empty;
                string accountOnRequest = request.AccountNumber ?? accountOnPrincipal;

                bool isChanged = accountOnRequest != accountOnPrincipal;

                if (request.ThrowOnValidationFailure && isChanged)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Customer account number on the request was wrong.");
                }

                return new GetValidatedCustomerAccountNumberServiceResponse(accountOnPrincipal, isCustomerAccountNumberInContextDifferent: request.AccountNumber != accountOnPrincipal);
            }

            /// <summary>
            /// Get customers using the request criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria to retrieve customers for.</param>
            /// <returns>GetCustomersServiceResponse object.</returns>
            /// <remarks>
            /// Calling this method with empty criteria (e.g. empty account number or no record id) will lead to performance issues.
            /// Please use instead SearchCustomers method.
            /// </remarks>
            private static GetCustomersServiceResponse GetCustomers(GetCustomersServiceRequest request)
            {
                ThrowIf.Null(request, "request");

                var customersDataRequest = new GetCustomerDataRequest(request.CustomerAccountNumber);
                SingleEntityDataServiceResponse<Customer> customersResponse = null;

                if (request.SearchLocation.HasFlag(SearchLocation.Local))
                {
                    customersResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(customersDataRequest);
                }

                // If no customer was found locally and the request permits it, attempt to download and return the customer from AX
                if (request.SearchLocation.HasFlag(SearchLocation.Remote) &&
                    (customersResponse == null || customersResponse.Entity == null))
                {
                    try
                    {
                        var customerDownloadRequest = new DownloadCustomerRealtimeRequest(request.CustomerAccountNumber);
                        request.RequestContext.Execute<NullResponse>(customerDownloadRequest);
                        customersResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(customersDataRequest);
                    }
                    catch (FeatureNotSupportedException)
                    {
                        // If we are in offline mode, a feature not supported exception will be thrown.
                        // If this is the case, continue with the empty response from the local database.
                    }
                }

                IList<Customer> customers;
                if (customersResponse.Entity != null)
                {
                    customers = new Customer[] { customersResponse.Entity };
                }
                else
                {
                    customers = new Customer[0];
                }

                ReadOnlyCollection<Customer> outputCustomers = new ReadOnlyCollection<Customer>(customers);
                return new GetCustomersServiceResponse(new PagedResult<Customer>(outputCustomers));
            }

            /// <summary>
            /// Get customers using the request criteria.
            /// </summary>
            /// <param name="request">Request containing the criteria to retrieve customers for.</param>
            /// <returns>CustomersSearchServiceResponse object.</returns>
            private static CustomersSearchServiceResponse SearchCustomers(CustomersSearchServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.Criteria, "request.Criteria");
                ThrowIf.NullOrWhiteSpace(request.Criteria.Keyword, "request.Criteria.Keyword");
                if (request.Criteria.SearchLocation.Equals(SearchLocation.None) || request.Criteria.SearchLocation.Equals(SearchLocation.All))
                {
                    throw new ArgumentException("request.Criteria.SearchLocation must be set to either Local or Remote.");
                }

                var customers = PagedResult<GlobalCustomer>.Empty();

                if (request.Criteria.SearchLocation.HasFlag(SearchLocation.Remote))
                {
                    var customerSearchRealtimeRequest = new SearchCustomersRealtimeRequest(
                                                            request.Criteria.Keyword,
                                                            request.QueryResultSettings);

                    var realtimeServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<GlobalCustomer>>(customerSearchRealtimeRequest);
                    customers = realtimeServiceResponse.PagedEntityCollection;
                }
                else
                {
                    var dataServiceRequest = new SearchCustomersDataRequest(
                        request.Criteria.Keyword,
                        request.Criteria.SearchOnlyCurrentCompany,
                        request.QueryResultSettings);

                    var dataServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<GlobalCustomer>>(dataServiceRequest);
                    customers = dataServiceResponse.PagedEntityCollection;
                }

                string customerImage = null;
                foreach (GlobalCustomer customer in customers.Results)
                {
                    if (customer.Images != null && customer.Images.Any())
                    {
                        customerImage = customer.Images.FirstOrDefault().Uri;
                    }

                    if (!string.IsNullOrEmpty(customer.AccountNumber))
                    {
                        customer.Images = RichMediaHelper.PopulateCustomerMediaInformation(customer.AccountNumber, customerImage, request.RequestContext.GetChannelConfiguration().CustomerDefaultImageTemplate);
                    }
                }

                var response = new CustomersSearchServiceResponse(customers);
                return response;
            }

            /// <summary>
            /// Gets the collection of customer group from customer group table.
            /// </summary>
            /// <param name="request">Request containing the service context.</param>
            /// <returns>Returns the customer groups.</returns>
            private static GetCustomerGroupsServiceResponse GetCustomerGroups(GetCustomerGroupsServiceRequest request)
            {
                var dataServiceRequest = new GetCustomerGroupsDataRequest(request.QueryResultSettings);
                EntityDataServiceResponse<CustomerGroup> dataServiceResponse = request.RequestContext.Execute<EntityDataServiceResponse<CustomerGroup>>(dataServiceRequest);
                return new GetCustomerGroupsServiceResponse(dataServiceResponse.PagedEntityCollection);
            }

            /// <summary>
            /// Save the requested customer.
            /// </summary>
            /// <param name="request">Request contains the customer to save.</param>
            /// <returns>The response.</returns>
            private static SaveCustomerServiceResponse SaveCustomer(SaveCustomerServiceRequest request)
            {
                Customer customer;

                if (request.CustomerToSave.CustomerExists)
                {
                    customer = UpdateCustomer(request);
                }
                else
                {
                    customer = CreateCustomer(request, request.RequestContext);
                }

                var response = new SaveCustomerServiceResponse(customer);

                return response;
            }

            /// <summary>
            /// Create a new customer record.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="context">The request context.</param>
            /// <returns>The customer record created.</returns>
            private static Customer CreateCustomer(SaveCustomerServiceRequest request, RequestContext context)
            {
                ThrowIf.Null(request, "request");

                long channelId = context.GetPrincipal().ChannelId;

                ICommercePrincipal principal = context.GetPrincipal();

                // An anonymous principal is allowed to create a customer account but the external identity information must be provided (customer sign up flow).
                if (principal.IsAnonymous && (string.IsNullOrWhiteSpace(principal.ExternalIdentityId) || string.IsNullOrWhiteSpace(principal.ExternalIdentityIssuer)))
                {
                    UserAuthenticationException exception = new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "External user identity not provided.");
                    RetailLogger.Log.CrtServicesCustomerExternalIdentityNotProvidedDuringCustomerSignUp(exception);
                    throw exception;
                }

                // If the user is a customer and there is already a user id set in the principal it means this user already has a customer account.
                if (principal.IsCustomer && !string.IsNullOrWhiteSpace(principal.UserId))
                {
                    RetailLogger.Log.CrtServicesCustomerExternalIdentityAlreadyHasCustomerAccountFailure(principal.ExternalIdentityId, principal.ExternalIdentityIssuer, principal.UserId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ExistingCustomerAlreadyMappedToExternalIdentity);
                }

                Customer customer;
                if (string.IsNullOrWhiteSpace(request.CustomerToSave.NewCustomerPartyNumber))
                {
                    customer = request.CustomerToSave;

                    var getCustomerdataServiceRequest = new GetCustomerDataRequest(GetDefaultCustomerAccountNumberFromChannelProperties(context));
                    SingleEntityDataServiceResponse<Customer> getCustomerdataServiceResponse = context.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerdataServiceRequest);
                    Customer defaultAxCustomer = getCustomerdataServiceResponse.Entity;

                    // if there is no default customer specified we can't set the defaults
                    if (defaultAxCustomer != null)
                    {
                        customer.CustomerGroup = string.IsNullOrWhiteSpace(customer.CustomerGroup) ? defaultAxCustomer.CustomerGroup : customer.CustomerGroup;
                        customer.CurrencyCode = string.IsNullOrWhiteSpace(customer.CurrencyCode) ? defaultAxCustomer.CurrencyCode : customer.CurrencyCode;
                    }

                    // If the user creating the customer is the customer himself then add the external identity information.
                    if (principal.IsAnonymous)
                    {
                        customer.ExternalIdentityId = principal.ExternalIdentityId;
                        customer.ExternalIdentityIssuer = principal.ExternalIdentityIssuer;
                    }

                    if (customer.IsAsyncCustomer)
                    {   // Create the Async customer
                        customer.AccountNumber = Guid.NewGuid().ToString();

                        if (customer.CustomerTypeValue == (int)CustomerType.Person)
                        {   // We must set the customer name when it is a person (First, Middle, Last)
                            string[] names = { customer.FirstName, customer.MiddleName, customer.LastName };
                            customer.Name = string.Join(" ", names.Where(x => !string.IsNullOrWhiteSpace(x)));
                        }
                    }
                    else
                    {
                        // save customer in AX
                        ExecutionHandler(
                             delegate
                             {
                                 var newCustomerRequest = new NewCustomerRealtimeRequest(customer, channelId);
                                 SaveCustomerRealtimeResponse newCustomerResponse = context.Execute<SaveCustomerRealtimeResponse>(newCustomerRequest);
                                 customer = newCustomerResponse.UpdatedCustomer;
                             },
                             CustomerOperations.CreateCustomerInAX.ToString());

                        // No need to save the address in AX since it was created already by the newCustomer call.
                    }

                    // save customer and addresses in CRT
                    ExecutionHandler(
                        delegate
                        {
                            var saveCustomerDataServiceRequest = new CreateOrUpdateCustomerDataRequest(customer);
                            SingleEntityDataServiceResponse<Customer> saveCustomerDataServiceResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(saveCustomerDataServiceRequest, request.RequestContext);
                            customer = saveCustomerDataServiceResponse.Entity;
                        },
                        CustomerOperations.CreateCustomerInCRT.ToString());
                }
                else
                {
                    // Refresh customerData so that the party specific fields are filled
                    var getInitCustomerDataServiceRequest = new GetCustomerWithPartyNumberDataRequest(request.CustomerToSave.NewCustomerPartyNumber);
                    SingleEntityDataServiceResponse<Customer> getInitCustomerDataServiceResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getInitCustomerDataServiceRequest, request.RequestContext);
                    customer = getInitCustomerDataServiceResponse.Entity;

                    // If the underlying party could not be found, download it from AX
                    if (customer == null)
                    {
                        var partyDownloadRequest = new DownloadPartyRealtimeRequest(request.CustomerToSave.NewCustomerPartyNumber);
                        context.Execute<NullResponse>(partyDownloadRequest);
                        customer = context.Execute<SingleEntityDataServiceResponse<Customer>>(getInitCustomerDataServiceRequest).Entity;
                    }

                    // save customer in AX
                    ExecutionHandler(
                         delegate
                         {
                             var newCustomerFromDirectoryPartyServiceRequest = new NewCustomerFromDirectoryPartyRealtimeRequest(customer, channelId);
                             SaveCustomerRealtimeResponse newCustomerFromDirectoryPartyServiceResponse = context.Execute<SaveCustomerRealtimeResponse>(newCustomerFromDirectoryPartyServiceRequest);
                             customer = newCustomerFromDirectoryPartyServiceResponse.UpdatedCustomer;
                         },
                         CustomerOperations.CreateCustomerInAX.ToString());

                    // save customer and addresses in CRT
                    ExecutionHandler(
                        delegate
                        {
                            var saveCustomerDataServiceRequest = new CreateOrUpdateCustomerDataRequest(customer);
                            SingleEntityDataServiceResponse<Customer> saveCustomerDataServiceResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(saveCustomerDataServiceRequest, request.RequestContext);
                            customer = saveCustomerDataServiceResponse.Entity;
                        },
                        CustomerOperations.CreateCustomerInCRT.ToString());
                }

                var getCustomerDataRequest = new GetCustomerDataRequest(customer.AccountNumber);
                SingleEntityDataServiceResponse<Customer> getCustomerDataResponse = context.Runtime.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerDataRequest, request.RequestContext);
                customer = getCustomerDataResponse.Entity;

                return customer;
            }

            /// <summary>
            /// Updates the customer.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>Updated customer record.</returns>
            private static Customer UpdateCustomer(SaveCustomerServiceRequest request)
            {
                ThrowIf.Null(request, "request");

                Customer customer;

                // Create customer in current company if the customer is from 3rd party company
                if (string.IsNullOrEmpty(request.CustomerToSave.AccountNumber) && !string.IsNullOrEmpty(request.CustomerToSave.NewCustomerPartyNumber))
                {
                    customer = CustomerService.CreateCustomer(request, request.RequestContext);
                }

                // get original copy of customer
                var getCustomerdataServiceRequest = new GetCustomerDataRequest(request.CustomerToSave.AccountNumber);
                SingleEntityDataServiceResponse<Customer> getCustomerdataServiceResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerdataServiceRequest);
                Customer databaseStoredCustomer = getCustomerdataServiceResponse.Entity;

                if (databaseStoredCustomer == null)
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound,
                        string.Format("Customer with RecordId {0} and AccountNumber {1} not found", request.CustomerToSave.RecordId, request.CustomerToSave.AccountNumber));
                }

                // make sure that the the recordid of the customer retrieved matches the customer record id in the request.
                // this prevents a request from updating the wrong record
                if (!request.CustomerToSave.IsAsyncCustomer && (databaseStoredCustomer.RecordId != request.CustomerToSave.RecordId))
                {
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_IdMismatch,
                        string.Format("Customer RecordId {0} in request does not match the Customer RecordId retrieved from the Channel Db {1} using AccountNumber {2} as the lookup", request.CustomerToSave.RecordId, databaseStoredCustomer.RecordId, request.CustomerToSave.AccountNumber));
                }

                // merge with CRT customer
                customer = DataModelExtensions.MergeDatabaseCustomerIntoInputCustomer(request.CustomerToSave, databaseStoredCustomer);

                if (customer.IsAsyncCustomer)
                {
                    // Additional processing may go here...
                }
                else
                {   // Process HQ (non-Async) customer...
                    ExecutionHandler(
                        delegate
                        {
                            // update customer in AX
                            var updateCustomerRealtimeRequest = new SaveCustomerRealtimeRequest(customer);
                            SaveCustomerRealtimeResponse updateCustomerRealtimeResponse = request.RequestContext.Execute<SaveCustomerRealtimeResponse>(updateCustomerRealtimeRequest);
                            customer = updateCustomerRealtimeResponse.UpdatedCustomer;
                        },
                        CustomerOperations.UpdateCustomerInAX.ToString());
                }

                // save address in AX
                customer = CustomerService.SaveAddresses(request, customer);

                if (!customer.IsAsyncCustomer)
                {
                    // save customer and addresses in CRT
                    ExecutionHandler(
                        delegate
                        {
                            var saveCustomerDataServiceRequest = new CreateOrUpdateCustomerDataRequest(customer);
                            SingleEntityDataServiceResponse<Customer> saveCustomerDataServiceResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(saveCustomerDataServiceRequest);
                            customer = saveCustomerDataServiceResponse.Entity;
                        },
                        CustomerOperations.UpdateCustomerInCRT.ToString());
                }

                // retrieve customer from database after update
                // client might update customer addresses / affiliations by recid and returning the current customer in memory will not have the full detail for those objects
                getCustomerdataServiceResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<Customer>>(getCustomerdataServiceRequest);
                return getCustomerdataServiceResponse.Entity;
            }

            /// <summary>
            /// Save customer account activation request to channel database.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static Response InitiateLinkToExistingCustomer(InitiateLinkToExistingCustomerServiceRequest request)
            {
                ThrowIf.Null(request, "request");

                LinkToExistingCustomerResult linkToExistingCustomerResult = null;
                ExecutionHandler(
                    delegate
                    {
                        var dataServiceRequest = new InitiateLinkToExistingCustomerDataRequest(
                            request.EmailAddress,
                            request.ActivationToken,
                            request.ExternalIdentityId,
                            request.ExternalIdentityIssuer,
                            request.CustomerId);

                        SingleEntityDataServiceResponse<LinkToExistingCustomerResult> initiateLinkToExistingCustomerDataResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<LinkToExistingCustomerResult>>(dataServiceRequest);
                        linkToExistingCustomerResult = initiateLinkToExistingCustomerDataResponse.Entity;
                    },
                    CustomerOperations.InitiateLinkToExistingCustomer.ToString());

                return new LinkToExistingCustomerServiceResponse(linkToExistingCustomerResult);
            }

            /// <summary>
            /// Create customer account and update account activation request status to completed.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>The response.</returns>
            private static Response FinalizeLinkToExistingCustomer(FinalizeLinkToExistingCustomerServiceRequest request)
            {
                if (request.RequestContext.GetPrincipal().IsEmployee)
                {
                    throw new UserAuthorizationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthorizationFailed, "Employee user is not authorized to perform this operation.");
                }

                LinkToExistingCustomerResult linkToExistingCustomerResult = null;
                ExecutionHandler(
                    delegate
                    {
                        var dataServiceRequest = new ValidateAccountActivationDataRequest(request.EmailAddress, request.ActivationToken);
                        ValidateAccountActivationDataResponse dataServiceResponse = request.RequestContext.Execute<ValidateAccountActivationDataResponse>(dataServiceRequest);
                        if (!dataServiceResponse.IsRequestValid)
                        {
                            throw new DataValidationException(
                                DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_ObjectNotFound,
                                string.Format("Invalid request. EmailAddress='{0}'; Activation token='{1}'", request.EmailAddress, request.ActivationToken));
                        }

                        // Update the request status in channel db
                        var finalizeLinkToExistingCustomerDataRequest = new FinalizeLinkToExistingCustomerDataRequest(request.EmailAddress, request.ActivationToken);
                        SingleEntityDataServiceResponse<LinkToExistingCustomerResult> finalizeLinkToExistingCustomerDataResponse = request.RequestContext.Execute<SingleEntityDataServiceResponse<LinkToExistingCustomerResult>>(finalizeLinkToExistingCustomerDataRequest);
                        linkToExistingCustomerResult = finalizeLinkToExistingCustomerDataResponse.Entity;
                    },
                    CustomerOperations.FinalizeLinkToExistingCustomer.ToString());

                return new LinkToExistingCustomerServiceResponse(linkToExistingCustomerResult);
            }

            private static Response UnlinkFromExistingCustomer(UnlinkFromExistingCustomerServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ExecutionHandler(
                    delegate
                    {
                        ICommercePrincipal principal = request.RequestContext.GetPrincipal();

                        if (!principal.IsCustomer || string.IsNullOrEmpty(principal.ExternalIdentityId) || string.IsNullOrEmpty(principal.ExternalIdentityIssuer))
                        {
                            UserAuthenticationException exception = new UserAuthenticationException(SecurityErrors.Microsoft_Dynamics_Commerce_Runtime_AuthenticationFailed, "The current role must be of a customer, and the external identity must be provided.");
                            RetailLogger.Log.CrtServicesCustomerExternalIdentityNotProvidedDuringCustomerUnlinking(exception);
                            throw exception;
                        }

                        string customerId = principal.UserId;
                        if (string.IsNullOrEmpty(customerId))
                        {
                            throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CustomerNotFound, "The customer account number is not set on the principal.");
                        }

                        var unlinkFromExistingCustomerServiceRequest = new UnlinkFromExistingCustomerDataRequest(principal.ExternalIdentityId, principal.ExternalIdentityIssuer, customerId);
                        request.RequestContext.Execute<NullResponse>(unlinkFromExistingCustomerServiceRequest);

                        // This is where a call to Ax would be made to update the table in HQ as well.
                    },
                    CustomerOperations.UnlinkFromExistingCustomer.ToString());

                return new NullResponse();
            }

            /// <summary>
            /// Save the customer addresses.
            /// </summary>
            /// <param name="request">This service request.</param>
            /// <param name="customerData">The customer data.</param>
            /// <returns>The updated customer. </returns>
            private static Customer SaveAddresses(Request request, Customer customerData)
            {
                Customer result;
                if (customerData.IsAsyncCustomer)
                {
                    result = SaveAsyncCustomerAddresses(request, customerData);
                }
                else
                {
                    result = SaveCustomerAddresses(request, customerData);
                }

                return result;
            }

            /// <summary>
            /// Save the async customer addresses (locally) in ax.RETAILASYNCADDRESS.
            /// </summary>
            /// <param name="request">This service request.</param>
            /// <param name="customerData">The customer data.</param>
            /// <returns>The updated customer. </returns>
            private static Customer SaveAsyncCustomerAddresses(Request request, Customer customerData)
            {
                List<Address> savedAddresses = new List<Address>();

                foreach (Address address in customerData.Addresses)
                {
                    Address savedAddress = address;
                    if (savedAddress == null)
                    {
                        continue;
                    }
                    else if (
                        string.IsNullOrWhiteSpace(savedAddress.Street) &&
                        string.IsNullOrWhiteSpace(savedAddress.City) &&
                        string.IsNullOrWhiteSpace(savedAddress.ZipCode) &&
                        string.IsNullOrWhiteSpace(savedAddress.County) &&
                        string.IsNullOrWhiteSpace(savedAddress.TwoLetterISORegionName) &&
                        string.IsNullOrWhiteSpace(savedAddress.Email) &&
                        string.IsNullOrWhiteSpace(savedAddress.Phone) &&
                        string.IsNullOrWhiteSpace(savedAddress.Url) &&
                        string.IsNullOrWhiteSpace(savedAddress.StreetNumber) &&
                        string.IsNullOrWhiteSpace(savedAddress.DistrictName) &&
                        string.IsNullOrWhiteSpace(savedAddress.BuildingCompliment))
                    {
                        // Pre-check using SAME criteria as in AX Transaction Service call to RetailTransactionServiceCustomer.createAddress() failed...
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PreValidationForAsyncAddressFailed, string.Format("Pre-validation of the data required by AX transaction service to create the address failed."));
                    }
                    else if (savedAddress.RecordId != 0)
                    {
                        // Get existing address and validate readonly properties before updating the address.
                        GetAddressDataRequest dataServiceRequest = new GetAddressDataRequest(savedAddress.RecordId, customerData.RecordId, new ColumnSet());
                        Address existingAddress = request.RequestContext.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest).Entity;

                        if (existingAddress != null)
                        {
                            Collection<DataValidationFailure> dataValidationFailures = ReadOnlyAttribute.CheckReadOnlyProperties(existingAddress, savedAddress);

                            if (dataValidationFailures.Count > 0)
                            {
                                throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_PreValidationForAsyncAddressFailed, dataValidationFailures, string.Format("Validation failures occurred when verifying address for customer number {0} for update.", customerData.AccountNumber));
                            }
                        }
                    }

                    // save or create async addresses
                    ExecutionHandler(
                        delegate
                        {
                            var saveAsyncCustomerAddressServiceRequest = new CreateOrUpdateAsyncCustomerAddressDataRequest(customerData, address);
                            SingleEntityDataServiceResponse<Address> saveAsyncCustomerAddressServiceResponse = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<Address>>(saveAsyncCustomerAddressServiceRequest, request.RequestContext);
                            savedAddress = saveAsyncCustomerAddressServiceResponse.Entity;
                        },
                        CustomerOperations.CreateAsyncCustomerAddress.ToString());

                    if (savedAddress.RecordId == 0)
                    {
                        // The record was not saved for some reason...
                        throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidOperation, "The savedAddress did not return a RecordId.");
                    }

                    savedAddresses.Add(savedAddress);
                }

                customerData.Addresses = savedAddresses;

                return customerData;
            }

            /// <summary>
            /// Save the customer addresses in AX.
            /// </summary>
            /// <param name="request">This service request.</param>
            /// <param name="customerData">The customer data.</param>
            /// <returns>The updated customer. </returns>
            private static Customer SaveCustomerAddresses(Request request, Customer customerData)
            {
                List<Address> savedAddresses = new List<Address>();

                foreach (Address address in customerData.Addresses)
                {
                    Address savedAddress = address;
                    if (savedAddress == null)
                    {
                        continue;
                    }

                    if (savedAddress.RecordId == 0)
                    {
                        ExecutionHandler(
                            delegate
                            {
                                var createAddressRequest = new CreateAddressRealtimeRequest(customerData, savedAddress);
                                CreateAddressRealtimeResponse createAddressResponse = request.RequestContext.Execute<CreateAddressRealtimeResponse>(createAddressRequest);
                                savedAddress = createAddressResponse.Address;
                            },
                            CustomerOperations.CreateAddressInAX.ToString());
                    }
                    else
                    {
                        if (address.Deactivate)
                        {
                            long addressId = savedAddress.RecordId;
                            long customerId = customerData.RecordId;

                            ExecutionHandler(
                                delegate
                                {
                                    var deactivateAddressRequest = new DeactivateAddressRealtimeRequest(addressId, customerId);
                                    request.RequestContext.Execute<NullResponse>(deactivateAddressRequest);
                                },
                                CustomerOperations.DeactivateAddressInAX.ToString());
                        }
                        else
                        {
                            // Get existing address and validate readonly properties before updating the address.
                            GetAddressDataRequest dataServiceRequest = new GetAddressDataRequest(savedAddress.RecordId, customerData.RecordId, new ColumnSet());
                            Address existingAddress = request.RequestContext.Execute<SingleEntityDataServiceResponse<Address>>(dataServiceRequest).Entity;

                            if (existingAddress != null)
                            {
                                Collection<DataValidationFailure> dataValidationFailures = ReadOnlyAttribute.CheckReadOnlyProperties(existingAddress, savedAddress);

                                if (dataValidationFailures.Count > 0)
                                {
                                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError, dataValidationFailures, string.Format("Validation failures occurred when verifying address for customer number {0} for update.", customerData.AccountNumber));
                                }
                            }

                            ExecutionHandler(
                                delegate
                                {
                                    var updateAddressReqeust = new UpdateAddressRealtimeRequest(customerData, savedAddress);
                                    UpdateAddressRealtimeResponse updateAddressResponse = request.RequestContext.Execute<UpdateAddressRealtimeResponse>(updateAddressReqeust);
                                    customerData = updateAddressResponse.Customer;
                                    savedAddress = updateAddressResponse.Address;
                                },
                                CustomerOperations.UpdateAddressInAX.ToString());
                        }
                    }

                    savedAddresses.Add(savedAddress);
                }

                customerData.Addresses = savedAddresses;

                return customerData;
            }

            /// <summary>
            /// Gets the balance.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <returns>A customer balance service response.</returns>
            private static GetCustomerBalanceServiceResponse GetBalance(GetCustomerBalanceServiceRequest request)
            {
                if (!request.SearchLocation.HasFlag(SearchLocation.Remote))
                {
                    // Since last transaction that was in sync after P-job (anchor) can be retrieved from TS only,
                    // the required search location has to be either Remote or All.
                    throw new DataValidationException(
                        DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_InvalidRequest,
                        "Search location requires Remote flag in order to retrive the PendingTransactionsAnchor.");
                }

                // Get balance from HQ
                var realtimeRequest = new GetCustomerBalanceRealtimeRequest(request.AccountNumber);
                GetCustomerBalanceRealtimeResponse getCustomerBalanceResponse = request.RequestContext.Execute<GetCustomerBalanceRealtimeResponse>(realtimeRequest);
                CustomerBalances customerBalance = getCustomerBalanceResponse.Balance;

                if (request.SearchLocation.HasFlag(SearchLocation.Local))
                {
                    // Fetch pending local balance too
                    GetCustomerAccountLocalPendingBalanceDataRequest getCustomerAccountLocalPendingBalanceDataRequest = new GetCustomerAccountLocalPendingBalanceDataRequest(request.AccountNumber, customerBalance.PendingTransactionsAnchor, QueryResultSettings.SingleRecord);
                    decimal localNonPostedTransactionAmount = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<decimal>>(getCustomerAccountLocalPendingBalanceDataRequest, request.RequestContext).Entity;

                    customerBalance.PendingBalance = localNonPostedTransactionAmount;

                    if (!string.IsNullOrWhiteSpace(request.InvoiceAccountNumber))
                    {
                        getCustomerAccountLocalPendingBalanceDataRequest = new GetCustomerAccountLocalPendingBalanceDataRequest(request.InvoiceAccountNumber, customerBalance.PendingTransactionsAnchor, QueryResultSettings.SingleRecord);
                        decimal localInvoiceNonPostedTransactionAmount = request.RequestContext.Runtime.Execute<SingleEntityDataServiceResponse<decimal>>(getCustomerAccountLocalPendingBalanceDataRequest, request.RequestContext).Entity;

                        customerBalance.InvoiceAccountPendingBalance = localInvoiceNonPostedTransactionAmount;
                    }
                }

                return new GetCustomerBalanceServiceResponse(customerBalance);
            }

            /// <summary>
            /// Get order history for the customer.
            /// </summary>
            /// <param name="request">Request containing the criteria used to retrieve order history.</param>
            /// <returns>GetOrderHistoryServiceResponse object.</returns>
            private static GetOrderHistoryServiceResponse GetOrderHistory(GetOrderHistoryServiceRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");

                // Async customers do not have order histories.
                if (CustomerService.IsAsyncCustomer(request.RequestContext, request.CustomerAccountNumber))
                {
                    return new GetOrderHistoryServiceResponse(PagedResult<SalesOrder>.Empty());
                }

                var validateCustomerAccountRequest = new GetValidatedCustomerAccountNumberServiceRequest(request.CustomerAccountNumber, throwOnValidationFailure: false);
                var validatedCustomerAccountNumberResponse = request.RequestContext.Execute<GetValidatedCustomerAccountNumberServiceResponse>(validateCustomerAccountRequest);

                if (validatedCustomerAccountNumberResponse.IsCustomerAccountNumberInContextDifferent)
                {
                    request.CustomerAccountNumber = validatedCustomerAccountNumberResponse.ValidatedAccountNumber;
                }

                // Adjust the paging.
                QueryResultSettings settings = request.QueryResultSettings;
                PagingInfo adjustedPaging = MultiDataSourcesPagingHelper.GetAdjustedPaging(settings.Paging);
                QueryResultSettings adjustedQueryResultSettings = new QueryResultSettings(settings.ColumnSet, adjustedPaging, settings.Sorting, settings.ChangeTracking);

                // Check sorting information.
                if (settings.Sorting == null)
                {
                    settings.Sorting = new SortingInfo();
                }

                if (!settings.Sorting.IsSpecified)
                {
                    settings.Sorting.Add(new SortColumn(RetailTransactionTableSchema.CreatedDateTimeColumn, isDescending: true));
                }

                DateTimeOffset startDateTime = CalculateStarteDateTime(request.RequestContext);
                IEnumerable<SalesOrder> localOrders, remoteOrders;

                var dataRequest = new GetOrderHistoryDataRequest(request.CustomerAccountNumber, startDateTime, adjustedQueryResultSettings);
                localOrders = request.RequestContext.Execute<EntityDataServiceResponse<SalesOrder>>(dataRequest).PagedEntityCollection.Results;

                var remoteRequest = new GetOrderHistoryRealtimeRequest(request.CustomerAccountNumber, startDateTime, adjustedQueryResultSettings);
                GetOrderHistoryRealtimeResponse getOrderHistoryServiceResponse = request.RequestContext.Execute<GetOrderHistoryRealtimeResponse>(remoteRequest);
                remoteOrders = getOrderHistoryServiceResponse.Orders.Results.ToList();

                IEnumerable<SalesOrder> mergedResults = CustomerService.MergeOrderRecords(remoteOrders, localOrders);
                PagedResult<SalesOrder> pagedResults = MultiDataSourcesPagingHelper.GetPagedResult(mergedResults, settings.Paging, settings.Sorting);

                var response = new GetOrderHistoryServiceResponse(pagedResults);
                return response;
            }

            #region Helpers

            /// <summary>
            /// Gets the key used when merging two orders.
            /// </summary>
            /// <param name="salesOrder">The sales order to merge.</param>
            /// <returns>The key.</returns>
            private static SalesOrder GetOrderMergingKey(SalesOrder salesOrder)
            {
                return salesOrder;
            }

            /// <summary>
            /// Gets the key used when merging two records.
            /// </summary>
            /// <param name="purchaseHistory">The purchase history to merge.</param>
            /// <returns>The key.</returns>
            private static string GetPurchaseHistoryMergingKey(PurchaseHistory purchaseHistory)
            {
                string result = purchaseHistory.ReceiptId + Separator
                    + purchaseHistory.SalesId + Separator
                    + purchaseHistory.ItemId + Separator
                    + purchaseHistory.ProductId;

                // filter out invalid data.
                if (result.Equals(Separator + Separator + Separator))
                {
                    return null;
                }
                else
                {
                    return result;
                }
            }

            /// <summary>
            /// Gets the default customer account number from the channel properties.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The default customer account number.</returns>
            private static string GetDefaultCustomerAccountNumberFromChannelProperties(RequestContext context)
            {
                ChannelConfiguration channelConfig = context.GetChannelConfiguration();

                if (channelConfig.ChannelType == RetailChannelType.RetailStore)
                {
                    return context.GetOrgUnit().DefaultCustomerAccount;
                }

                var getOnlineChannelByIdDataRequest = new GetOnlineChannelByIdDataRequest(channelConfig.RecordId, new ColumnSet());
                OnlineChannel channel = context.Runtime.Execute<SingleEntityDataServiceResponse<OnlineChannel>>(getOnlineChannelByIdDataRequest, context).Entity;

                return channel.DefaultCustomerAccount;
            }

            /// <summary>
            /// The execution handler.
            /// </summary>
            /// <param name="action">The action.</param>
            /// <param name="operationType">Type of the operation.</param>
            private static void ExecutionHandler(Action action, string operationType)
            {
                try
                {
                    action();
                    NetTracer.Information("Operation {0} succeeded", operationType);
                }
                catch (Exception e)
                {
                    RetailLogger.Log.CrtServicesCustomerServiceHandlerExecutionFailure(operationType, e);
                    throw;
                }
            }

            /// <summary>
            /// Calculate the starting date time to fetch data.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>The starting date time.</returns>
            private static DateTimeOffset CalculateStarteDateTime(RequestContext context)
            {
                DateTimeOffset startDateTime = DateTimeOffset.UtcNow;
                ChannelConfiguration channelConfig = context.GetChannelConfiguration();
                switch (channelConfig.DaysCustomerHistory)
                {
                    case DaysCustomerHistoryType.All:
                        startDateTime = DateTimeOffset.MinValue;
                        break;

                    case DaysCustomerHistoryType.Days90:
                        startDateTime = startDateTime.AddDays(-90d);
                        break;

                    case DaysCustomerHistoryType.Days60:
                        startDateTime = startDateTime.AddDays(-60d);
                        break;

                    default:
                        startDateTime = startDateTime.AddDays(-30d);
                        break;
                }

                return startDateTime;
            }

            /// <summary>
            /// Check if an account number maps to an async customer.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <param name="customerAccountNumber">The customer account number.</param>
            /// <returns>A boolean indicating whether or not the customer is an async customer.</returns>
            private static bool IsAsyncCustomer(RequestContext context, string customerAccountNumber)
            {
                if (!string.IsNullOrWhiteSpace(customerAccountNumber))
                {
                    var getCustomersServiceRequest = new GetCustomersServiceRequest(QueryResultSettings.SingleRecord, customerAccountNumber);
                    var getCustomersServiceResponse = context.Execute<GetCustomersServiceResponse>(getCustomersServiceRequest);

                    if (getCustomersServiceResponse.Customers.FirstOrDefault().IsAsyncCustomer)
                    {
                        return true;
                    }
                }

                return false;
            }
            #endregion
        }
    }
}
