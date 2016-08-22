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
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
        using Microsoft.Dynamics.Commerce.Runtime.Messages;

        /// <summary>
        /// Customer data requests handler that retrieves the customer information from underlying data storage.
        /// </summary>
        public class CustomerSqlServerDataService : IRequestHandler
        {
            // Stored procedure names
            private const string GetCustomerByExternalIdentitySprocName = "GETCUSTOMERBYEXTERNALIDENTITY";

            // Parameter names
            private const string DataAreaIdVariableName = "@nvc_DataAreaId";
            private const string LocaleVariableName = "@nvc_Locale";
            private const string SearchText = "@nvc_SearchText";
            private const string CustomerAccountNumberVariableName = "@nvc_CustAccount";
            private const string StartDateVariableName = "@startDateTime";

            // Function names
            private const string GetPurchaseHistoryFunctionName = "GetPurchaseHistory(@nvc_CustAccount, @nvc_Locale, @startDateTime)";

            /// <summary>
            /// Gets the collection of supported request types by this handler.
            /// </summary>
            public IEnumerable<Type> SupportedRequestTypes
            {
                get
                {
                    return new[]
                    {
                        typeof(FinalizeLinkToExistingCustomerDataRequest),
                        typeof(CreateOrUpdateCustomerDataRequest),
                        typeof(CreateOrUpdateAsyncCustomerAddressDataRequest),
                        typeof(GetCustomerDataRequest),
                        typeof(SearchCustomersDataRequest),
                        typeof(InitiateLinkToExistingCustomerDataRequest),
                        typeof(FinalizeLinkToExistingCustomerDataRequest),
                        typeof(UnlinkFromExistingCustomerDataRequest),
                        typeof(ValidateAccountActivationDataRequest),
                        typeof(GetCustomerAccountByExternalIdentityDataRequest),
                        typeof(GetPurchaseHistoryDataRequest),
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

                if (requestedType == typeof(FinalizeLinkToExistingCustomerDataRequest))
                {
                    response = this.FinalizeLinkToExistingCustomer((FinalizeLinkToExistingCustomerDataRequest)request);
                }
                else if (requestedType == typeof(CreateOrUpdateCustomerDataRequest))
                {
                    response = this.CreateOrUpdateCustomer((CreateOrUpdateCustomerDataRequest)request);
                }
                else if (requestedType == typeof(CreateOrUpdateAsyncCustomerAddressDataRequest))
                {
                    response = this.CreateOrUpdateAsyncCustomerAddress((CreateOrUpdateAsyncCustomerAddressDataRequest)request);
                }
                else if (requestedType == typeof(GetCustomerDataRequest))
                {
                    response = this.GetCustomerByAccountNumber((GetCustomerDataRequest)request);
                }
                else if (requestedType == typeof(SearchCustomersDataRequest))
                {
                    response = this.SearchCustomers((SearchCustomersDataRequest)request);
                }
                else if (requestedType == typeof(InitiateLinkToExistingCustomerDataRequest))
                {
                    response = this.InitiateLinkToExistingCustomer((InitiateLinkToExistingCustomerDataRequest)request);
                }
                else if (requestedType == typeof(UnlinkFromExistingCustomerDataRequest))
                {
                    response = this.UnlinkFromExistingCustomerDataRequest((UnlinkFromExistingCustomerDataRequest)request);
                }
                else if (requestedType == typeof(ValidateAccountActivationDataRequest))
                {
                    response = this.ValidateAccountActivation((ValidateAccountActivationDataRequest)request);
                }
                else if (requestedType == typeof(GetCustomerAccountByExternalIdentityDataRequest))
                {
                    response = this.GetCustomerAccountByExternalIdentity((GetCustomerAccountByExternalIdentityDataRequest)request);
                }
                else if (request is GetPurchaseHistoryDataRequest)
                {
                    response = this.GetPurchaseHistory((GetPurchaseHistoryDataRequest)request);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Request '{0}' is not supported.", request.GetType()));
                }

                return response;
            }

            private EntityDataServiceResponse<PurchaseHistory> GetPurchaseHistory(GetPurchaseHistoryDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.RequestContext, "request.RequestContext");
                ThrowIf.NullOrWhiteSpace(request.CustomerAccountNumber, "request.CustomerAccountNumber");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = GetPurchaseHistoryFunctionName,
                };

                query.Parameters[CustomerAccountNumberVariableName] = request.CustomerAccountNumber;
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[StartDateVariableName] = request.StartDate.UtcDateTime;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    PagedResult<PurchaseHistory> results = databaseContext.ReadEntity<PurchaseHistory>(query);
                    return new EntityDataServiceResponse<PurchaseHistory>(results);
                }
            }

            /// <summary>
            /// Gets the customer SQLServer database accessor instance.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of <see cref="CustomerSqlServerDatabaseAccessor"/></returns>
            private CustomerSqlServerDatabaseAccessor GetSqlDatabaseAccessorInstance(RequestContext context)
            {
                return new CustomerSqlServerDatabaseAccessor(context);
            }

            /// <summary>
            /// Gets the customer by account number.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Customer> GetCustomerByAccountNumber(GetCustomerDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                Customer customer = databaseAccessor.GetCustomerByAccountNumber(request.AccountNumber);

                return new SingleEntityDataServiceResponse<Customer>(customer);
            }

            /// <summary>
            /// Searches the customers with given search conditions.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private EntityDataServiceResponse<GlobalCustomer> SearchCustomers(SearchCustomersDataRequest request)
            {
                ThrowIf.Null(request, "request");
                ThrowIf.Null(request.QueryResultSettings, "request.QueryResultSettings");

                var query = new SqlPagedQuery(request.QueryResultSettings)
                {
                    From = "GETCUSTOMERSEARCHRESULTS(@bi_ChannelId, @dt_ChannelDate, @nvc_SearchText)"
                };

                query.Parameters[DatabaseAccessor.ChannelIdVariableName] = request.RequestContext.GetPrincipal().ChannelId;
                query.Parameters[SearchText] = '"' + request.Keyword.Replace(" ", "* ").Replace("\"", "\"\"") + '*' + '"';
                query.Parameters[LocaleVariableName] = request.RequestContext.LanguageId;
                query.Parameters[DatabaseAccessor.ChannelDateVariableName] = request.RequestContext.GetNowInChannelTimeZone().Date;

                using (DatabaseContext databaseContext = new DatabaseContext(request.RequestContext))
                {
                    return new EntityDataServiceResponse<GlobalCustomer>(databaseContext.ReadEntity<GlobalCustomer>(query));
                }
            }

            /// <summary>
            /// Saves customer account activation request to channel DB.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<LinkToExistingCustomerResult> FinalizeLinkToExistingCustomer(FinalizeLinkToExistingCustomerDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                LinkToExistingCustomerResult result = databaseAccessor.FinalizeLinkToExistingCustomer(request.EmailAddress, request.ActivationToken);
                this.UpdateCustomerExternalIdentityMapCache(request.RequestContext, result.ExternalIdentityId, result.ExternalIdentityProvider);

                return new SingleEntityDataServiceResponse<LinkToExistingCustomerResult>(result);
            }

            /// <summary>
            /// Creates or updates a customer.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Customer> CreateOrUpdateCustomer(CreateOrUpdateCustomerDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                Customer customer;

                if (request.Customer.IsAsyncCustomer)
                {   // Create the async customer (for offline)
                    customer = databaseAccessor.CreateOrUpdateAsyncCustomer(request.Customer);
                }
                else
                {   // Create the customer
                    customer = databaseAccessor.CreateOrUpdateCustomer(request.Customer);
                }

                return new SingleEntityDataServiceResponse<Customer>(customer);
            }

            /// <summary>
            /// Creates or updates a customer address.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<Address> CreateOrUpdateAsyncCustomerAddress(CreateOrUpdateAsyncCustomerAddressDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                Address address = null;

                if (request.Customer.IsAsyncCustomer)
                {   // Create the async customer (for offline)
                    address = databaseAccessor.CreateOrUpdateAsyncCustomerAddress(request.Customer, request.Address);
                }
                else
                {
                    // Should never happen...
                    throw new InvalidOperationException("Only addresses of async customers can be updated with this method.");
                }

                return new SingleEntityDataServiceResponse<Address>(address);
            }

            /// <summary>
            /// Saves the customer account activation request to channel DB.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<LinkToExistingCustomerResult> InitiateLinkToExistingCustomer(InitiateLinkToExistingCustomerDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                LinkToExistingCustomerResult result = databaseAccessor.InitiateLinkToExistingCustomer(request.EmailAddress, request.ActivationToken, request.ExternalIdentityId, request.ExternalIdentityIssuer, request.CustomerId);
                this.UpdateCustomerExternalIdentityMapCache(request.RequestContext, result.ExternalIdentityId, result.ExternalIdentityProvider);

                return new SingleEntityDataServiceResponse<LinkToExistingCustomerResult>(result);
            }

            private NullResponse UnlinkFromExistingCustomerDataRequest(UnlinkFromExistingCustomerDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                databaseAccessor.UnlinkExternalIdentityFromCustomer(request.ExternalIdentityId, request.ExternalIdentityIssuer, request.CustomerId);
                this.UpdateCustomerExternalIdentityMapCache(request.RequestContext, request.ExternalIdentityId, request.ExternalIdentityIssuer);

                return new NullResponse();
            }

            /// <summary>
            /// Validates the account activation request.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private ValidateAccountActivationDataResponse ValidateAccountActivation(ValidateAccountActivationDataRequest request)
            {
                CustomerSqlServerDatabaseAccessor databaseAccessor = this.GetSqlDatabaseAccessorInstance(request.RequestContext);
                bool isValid = databaseAccessor.ValidateAccountActivationRequest(request.Email, request.ActivationToken);

                return new ValidateAccountActivationDataResponse(isValid);
            }

            /// <summary>
            /// Gets the customer account by external identity.
            /// </summary>
            /// <param name="request">The data service request.</param>
            /// <returns>The data service response.</returns>
            private SingleEntityDataServiceResponse<CustomerExternalIdentityMap> GetCustomerAccountByExternalIdentity(GetCustomerAccountByExternalIdentityDataRequest request)
            {
                CustomerL2CacheDataStoreAccessor levelL2CacheDataAccessor = this.GetCustomerL2CacheDataStoreAccessor(request.RequestContext);
                bool foundInCache;
                bool updateCache;

                CustomerExternalIdentityMap customerExternalIdentityMap = DataManager.GetDataFromCache(() => levelL2CacheDataAccessor.GetCustomerAccountByExternalIdentity(request.ExternalIdentityId, request.ExternalIdentityIssuer), out foundInCache, out updateCache);

                if (!foundInCache)
                {
                    customerExternalIdentityMap = this.GetCustomerAccountByExternalIdentityFromDb(request.RequestContext, request.ExternalIdentityId, request.ExternalIdentityIssuer);
                }

                if (updateCache && (customerExternalIdentityMap != null))
                {
                    levelL2CacheDataAccessor.CacheCustomerAccountByExternalIdentity(request.ExternalIdentityId, request.ExternalIdentityIssuer, customerExternalIdentityMap);
                }

                return new SingleEntityDataServiceResponse<CustomerExternalIdentityMap>(customerExternalIdentityMap);
            }

            private CustomerExternalIdentityMap GetCustomerAccountByExternalIdentityFromDb(RequestContext requestContext, string externalIdentityId, string externalIdentityIssuer)
            {
                CustomerExternalIdentityMap customerExternalIdentityMap;
                ParameterSet parameters = new ParameterSet();
                parameters["@nvc_ExternalIdentityId"] = externalIdentityId;
                parameters["@nvc_Issuer"] = externalIdentityIssuer;
                parameters[DataAreaIdVariableName] = requestContext.GetChannelConfiguration().InventLocationDataAreaId;
                using (SqlServerDatabaseContext sqlServerDatabaseContext = new SqlServerDatabaseContext(requestContext))
                {
                    customerExternalIdentityMap = sqlServerDatabaseContext.ExecuteStoredProcedure<CustomerExternalIdentityMap>(GetCustomerByExternalIdentitySprocName, parameters).Results.FirstOrDefault();
                }

                return customerExternalIdentityMap;
            }

            private void UpdateCustomerExternalIdentityMapCache(RequestContext requestContext, string externalIdentityId, string externalIdentityIssuer)
            {
                CustomerExternalIdentityMap customerExternalIdentityMap = this.GetCustomerAccountByExternalIdentityFromDb(requestContext, externalIdentityId, externalIdentityIssuer);
                CustomerL2CacheDataStoreAccessor levelL2CacheDataAccessor = this.GetCustomerL2CacheDataStoreAccessor(requestContext);
                levelL2CacheDataAccessor.CacheCustomerAccountByExternalIdentity(externalIdentityId, externalIdentityIssuer, customerExternalIdentityMap);
            }

            /// <summary>
            /// Gets the cache accessor for the customer data service requests.
            /// </summary>
            /// <param name="context">The request context.</param>
            /// <returns>An instance of the <see cref="CustomerL2CacheDataStoreAccessor"/> class.</returns>
            private CustomerL2CacheDataStoreAccessor GetCustomerL2CacheDataStoreAccessor(RequestContext context)
            {
                DataStoreManager.InstantiateDataStoreManager(context);
                return new CustomerL2CacheDataStoreAccessor(DataStoreManager.DataStores[DataStoreType.L2Cache], context);
            }
        }
    }
}
