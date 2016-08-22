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
    namespace Retail.Ecommerce.Sdk.Core.OperationsHandlers
    {
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Handler for customer operations.
        /// </summary>
        public class CustomerOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public CustomerOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Creates the specified customer.
            /// </summary>
            /// <param name="customer">The customer.</param>
            /// <returns>The created customer.</returns>
            public virtual async Task<Customer> Create(Customer customer)
            {
                // An empty ecommerce context is explicitly passed in because this needs to be an anonymous call.
                IOrgUnitManager orgUnitManager = Utilities.GetManagerFactory(this.EcommerceContext.GetAnonymousContext()).GetManager<IOrgUnitManager>();
                ChannelConfiguration channelConfiguration = await orgUnitManager.GetOrgUnitConfiguration();

                customer.Language = channelConfiguration.DefaultLanguageId;
                customer.CustomerTypeValue = 1; // To denote this is a CustomerType.Person.

                ManagerFactory factory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICustomerManager customerManager = factory.GetManager<ICustomerManager>();

                Customer createdCustomer = null;
                try
                {
                    createdCustomer = await customerManager.Create(customer);
                }
                catch (UserAuthorizationException ex)
                {
                    if (ex.ErrorResourceId == AuthenticationErrors.UserNotActivated)
                    {
                        var message = "There is already an inactive account associated with the current external id. Need to unlink first.";
                        RetailLogger.Log.OnlineStoreCreatingNewCustomerForExternalIdWithInactiveLinkToExistingCustomer(
                            Utilities.GetMaskedEmailAddress(customer.Email),
                            this.EcommerceContext.IdentityProviderType.ToString(),
                            message);

                        IStoreOperationsManager storeOperationsManager = factory.GetManager<IStoreOperationsManager>();
                        await storeOperationsManager.UnlinkFromExistingCustomer();

                        createdCustomer = await customerManager.Create(customer);
                    }
                    else
                    {
                        throw ex;
                    }
                }

                return createdCustomer;
            }

            /// <summary>
            /// Gets the order history of a customer.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The order history of the current customer.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Async method - The nesting naturally fits into the business purpose of this method.")]
            public virtual async Task<PagedResult<SalesOrder>> GetOrderHistory(QueryResultSettings queryResultSettings)
            {
                ManagerFactory factory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICustomerManager customerManager = factory.GetManager<ICustomerManager>();
                PagedResult<SalesOrder> salesOrders = await customerManager.GetOrderHistory(string.Empty, queryResultSettings);
                salesOrders = await DataAugmenter.GetAugmentedSalesOrders(this.EcommerceContext, salesOrders);
                return salesOrders;
            }

            /// <summary>
            /// Get a customer by account number.
            /// </summary>
            /// <returns>The customer entity.</returns>
            public virtual async Task<Customer> GetCustomer()
            {
                ManagerFactory factory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICustomerManager customerManager = factory.GetManager<ICustomerManager>();
                return await customerManager.Read(string.Empty);
            }

            /// <summary>
            /// Update a customer entity based on provided entity.
            /// </summary>
            /// <param name="customer">The customer.</param>
            /// <returns>
            /// The updated customer entity.
            /// </returns>
            public virtual async Task<Customer> UpdateCustomer(Customer customer)
            {
                ManagerFactory factory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICustomerManager customerManager = factory.GetManager<ICustomerManager>();
                Customer updatedCustomer = await customerManager.Update(customer);

                return customer;
            }

            /// <summary>
            /// Generates a loyalty card identifier.
            /// </summary>
            /// <returns>
            /// A loyalty card.
            /// </returns>
            public virtual async Task<LoyaltyCard> GenerateLoyaltyCardId()
            {
                LoyaltyCard loyaltyCard = new LoyaltyCard();
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();

                loyaltyCard = await storeOperationsManager.IssueLoyaltyCard(loyaltyCard);

                return loyaltyCard;
            }

            /// <summary>
            /// Gets a read only collection with all loyalty card objects.
            /// </summary>
            /// <param name="queryResultSettings">Accepts queryResultSettings.</param>
            /// <returns>
            /// Read only collection of all loyalty card numbers.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<LoyaltyCard>> GetLoyaltyCards(QueryResultSettings queryResultSettings)
            {
                PagedResult<LoyaltyCard> loyaltyCards = new PagedResult<LoyaltyCard>();
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                loyaltyCards = await storeOperationsManager.GetCustomerLoyaltyCards(accountNumber: null, queryResultSettings: queryResultSettings);

                return loyaltyCards;
            }

            /// <summary>
            /// Gets the wish lists corresponding to customer.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// The wish lists.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<CommerceList>> GetWishLists(QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);

                ICustomerManager customerManager = managerFactory.GetManager<ICustomerManager>();
                Customer customer = await customerManager.Read(null);

                ICommerceListManager commerceListManager = managerFactory.GetManager<ICommerceListManager>();
                PagedResult<CommerceList> wishLists = await commerceListManager.GetByCustomer(customer.AccountNumber, queryResultSettings);

                return wishLists;
            }

            /// <summary>
            /// Unlinks the external identifier from existing customer.
            /// </summary>
            /// <returns>A task.</returns>
            public virtual async Task UnlinkExternalIdFromExistingCustomer()
            {
                IStoreOperationsManager storeOperationsManager = Utilities.GetManagerFactory(this.EcommerceContext).GetManager<IStoreOperationsManager>();
                await storeOperationsManager.UnlinkFromExistingCustomer();
            }

            /// <summary>
            /// Initiates a link between an external identity and an existing customer.
            /// </summary>
            /// <param name="emailAddressOfExistingCustomer">The email address of existing customer.</param>
            /// <param name="emailTemplateId">The email template identifier.</param>
            /// <param name="emailTemplateProperties">The email template properties.</param>
            /// <returns>
            /// A task.
            /// </returns>
            public virtual async Task<LinkToExistingCustomerResult> InitiateLinkExternalIdToExistingCustomer(string emailAddressOfExistingCustomer, string emailTemplateId, IEnumerable<NameValuePair> emailTemplateProperties)
            {
                string activationToken = System.Guid.NewGuid().ToString();
                IStoreOperationsManager storeOperationsManager = Utilities.GetManagerFactory(this.EcommerceContext).GetManager<IStoreOperationsManager>();
                LinkToExistingCustomerResult linkResult = await storeOperationsManager.InitiateLinkToExistingCustomer(emailAddressOfExistingCustomer, activationToken, emailTemplateId, emailTemplateProperties);

                return linkResult;
            }
        }
    }
}