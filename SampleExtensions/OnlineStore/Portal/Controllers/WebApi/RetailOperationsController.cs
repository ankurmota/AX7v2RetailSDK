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
    namespace Retail.Ecommerce.Web.Storefront.Controllers
    {
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Store Operations Controller.
        /// </summary>
        public class RetailOperationsController : WebApiControllerBase
        {
            /// <summary>
            /// Gets the loyalty card status.
            /// </summary>
            /// <param name="loyaltyCardNumbers">The loyalty card numbers.</param>
            /// <returns>Response containing the statuses of the specified loyalty card numbers.</returns>
            public async Task<ActionResult> GetLoyaltyCardStatus(IEnumerable<string> loyaltyCardNumbers)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                PagedResult<LoyaltyCard> loyaltyCards = await retailOperationsHandler.GetLoyaltyCardStatus(loyaltyCardNumbers);

                return this.Json(loyaltyCards.Results);
            }

            /// <summary>
            /// Gets the loyalty card transactions.
            /// </summary>
            /// <param name="loyaltyCardNumber">The loyalty card number.</param>
            /// <param name="rewardPointId">The reward point identifier.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>A response containing transactions for the specified loyalty card.</returns>
            public async Task<ActionResult> GetLoyaltyCardTransactions(string loyaltyCardNumber, string rewardPointId, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                PagedResult<LoyaltyCardTransaction> loyaltyCardTransactions = await retailOperationsHandler.GetLoyaltyCardTransactions(
                    loyaltyCardNumber,
                    rewardPointId,
                    queryResultSettings);

                return this.Json(loyaltyCardTransactions.Results);
            }

            /// <summary>
            /// Gets the country region information.
            /// </summary>
            /// <param name="languageId">The language identifier.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Country info response.</returns>
            public async Task<ActionResult> GetCountryRegionInfo(string languageId, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                PagedResult<CountryRegionInfo> countryRegionInfoCollection = await retailOperationsHandler.GetCountryRegionInfo(languageId, queryResultSettings);

                return this.Json(countryRegionInfoCollection.Results);
            }

            /// <summary>
            /// Gets the state province information.
            /// </summary>
            /// <param name="countryCode">The country code.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The states/provinces for the given country.</returns>
            public async Task<ActionResult> GetStateProvinceInfo(string countryCode, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                PagedResult<StateProvinceInfo> stateProvinceInfoCollection = await retailOperationsHandler.GetStateProvinces(countryCode, queryResultSettings);

                return this.Json(stateProvinceInfoCollection.Results);
            }

            /// <summary>
            /// Gets the gift card information.
            /// </summary>
            /// <param name="giftCardId">The gift card identifier.</param>
            /// <returns>A response containing gift card information.</returns>
            public async Task<ActionResult> GetGiftCardInformation(string giftCardId)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                GiftCard giftCard = await retailOperationsHandler.GetGiftCardInformation(giftCardId);

                return this.Json(giftCard);
            }

            /// <summary>
            /// Retrieves the card payment accept result.
            /// </summary>
            /// <param name="cardPaymentResultAccessCode">The card payment result access code.</param>
            /// <returns>Returns payment response.</returns>
            public async Task<ActionResult> RetrieveCardPaymentAcceptResult(string cardPaymentResultAccessCode)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                RetailOperationsHandler retailOperationsHandler = new RetailOperationsHandler(ecommerceContext);
                CardPaymentAcceptResult cardPaymentAcceptResult = await retailOperationsHandler.RetrieveCardPaymentAcceptResult(cardPaymentResultAccessCode);

                return this.Json(cardPaymentAcceptResult);
            }
        }
    }
}