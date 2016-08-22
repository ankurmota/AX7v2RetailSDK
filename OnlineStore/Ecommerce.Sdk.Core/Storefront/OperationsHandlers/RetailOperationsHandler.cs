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

        /// <summary>
        /// Handler for all miscellaneous retail operations.
        /// </summary>
        public class RetailOperationsHandler : OperationsHandlerBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RetailOperationsHandler"/> class.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            public RetailOperationsHandler(EcommerceContext ecommerceContext) : base(ecommerceContext)
            {
            }

            /// <summary>
            /// Get status of Loyalty card.
            /// </summary>
            /// <param name="loyaltyCardNumbers">List of loyalty card numbers.</param>
            /// <returns>Read only collection of all loyalty card numbers.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<LoyaltyCard>> GetLoyaltyCardStatus(IEnumerable<string> loyaltyCardNumbers)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                List<LoyaltyCard> loyaltyCards = new List<LoyaltyCard>();

                foreach (string loyaltyCardNumber in loyaltyCardNumbers)
                {
                    loyaltyCards.Add(await storeOperationsManager.GetLoyaltyCard(loyaltyCardNumber));
                }

                PagedResult<LoyaltyCard> loyaltyCardPagedResult = new PagedResult<LoyaltyCard>(loyaltyCards);
                return loyaltyCardPagedResult;
            }

            /// <summary>
            /// Gets all the transaction data specific to a a loyalty card number for a given points category.
            /// </summary>
            /// <param name="loyaltyCardNumber">The loyalty card Id.</param>
            /// <param name="rewardPointId">The reward points Id.</param>
            /// <param name="queryResultSettings">The queryResultSettings.</param>
            /// <returns>A PagedResult object that includes all transactions.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<LoyaltyCardTransaction>> GetLoyaltyCardTransactions(string loyaltyCardNumber, string rewardPointId, QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();

                PagedResult<LoyaltyCardTransaction> loyaltyCardTransactions = await storeOperationsManager.GetLoyaltyCardTransactions(loyaltyCardNumber, rewardPointId, queryResultSettings);

                return loyaltyCardTransactions;
            }

            /// <summary>
            /// Get the gift card balance.
            /// </summary>
            /// <param name="giftCardNumber">The gift card number.</param>
            /// <returns>A response containing gift card balance.</returns>
            public virtual async Task<GiftCard> GetGiftCardInformation(string giftCardNumber)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();
                GiftCard giftCard = await storeOperationsManager.GetGiftCard(giftCardNumber);

                return giftCard;
            }

            /// <summary>
            /// Gets the region info for all countries.
            /// </summary>
            /// <param name="languageId">The language identifier.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Paged result of CountryRegionInfo.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<CountryRegionInfo>> GetCountryRegionInfo(string languageId, QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();

                PagedResult<CountryRegionInfo> countryRegionInfoCollection =
                    await storeOperationsManager.GetCountryRegionsByLanguageId(languageId, queryResultSettings);

                return countryRegionInfoCollection;
            }

            /// <summary>
            /// Gets the states/provinces data for the given country.
            /// </summary>
            /// <param name="countryCode">Country code.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The states/provinces for the given country.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public virtual async Task<PagedResult<StateProvinceInfo>> GetStateProvinces(string countryCode, QueryResultSettings queryResultSettings)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                IStoreOperationsManager storeOperationsManager = managerFactory.GetManager<IStoreOperationsManager>();

                PagedResult<StateProvinceInfo> stateProvinceInfoCollection = await storeOperationsManager.GetStateProvinces(countryCode, queryResultSettings);

                return stateProvinceInfoCollection;
            }

            /// <summary>
            /// Retrieves card payment accept result.
            /// </summary>
            /// <param name="cardPaymentResultAccessCode">The card payment accept result code.</param>
            /// <returns>Card payment accept result.</returns>
            public virtual async Task<CardPaymentAcceptResult> RetrieveCardPaymentAcceptResult(string cardPaymentResultAccessCode)
            {
                ManagerFactory managerFactory = Utilities.GetManagerFactory(this.EcommerceContext);
                ICartManager cartManager = managerFactory.GetManager<ICartManager>();
                CardPaymentAcceptResult cardPaymentAcceptResult = await cartManager.RetrieveCardPaymentAcceptResult(cardPaymentResultAccessCode);

                return cardPaymentAcceptResult;
            }

            /// <summary>
            /// Finalizes the link to existing customer.
            /// </summary>
            /// <param name="emailAddressOfExistingCustomer">The email address of existing customer.</param>
            /// <param name="activationCode">The activation code.</param>
            /// <returns>The link result.</returns>
            public virtual async Task<LinkToExistingCustomerResult> FinalizeLinkToExistingCustomer(string emailAddressOfExistingCustomer, string activationCode)
            {
                IStoreOperationsManager storeOperationsManager = Utilities.GetManagerFactory(this.EcommerceContext.GetAnonymousContext()).GetManager<IStoreOperationsManager>();
                LinkToExistingCustomerResult result = await storeOperationsManager.FinalizeLinkToExistingCustomer(emailAddressOfExistingCustomer, activationCode);
                return result;
            }
        }
    }
}