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
        using Retail.Ecommerce.Web.Storefront.WebOperationsHandlers;

        /// <summary>
        /// Cart Controller.
        /// </summary>
        public class CartController : WebApiControllerBase
        {
            /// <summary>
            /// Gets the cart.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c>, then the current session is of checkout type.</param>
            /// <returns>The response containing the shopping cart.</returns>
            public async Task<ActionResult> GetCart(bool isCheckoutSession)
            {
                CartWebOperationsHandler cartWebOperationsHandler = new CartWebOperationsHandler(this.HttpContext);
                Cart cart = await cartWebOperationsHandler.GetCart(isCheckoutSession);
                return this.Json(cart);
            }

            /// <summary>
            /// Adds the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c>, then the current session is of checkout type.</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>The service response containing the updated shopping cart.</returns>
            public async Task<ActionResult> AddItems(bool isCheckoutSession, IEnumerable<CartLine> cartLines)
            {
                CartWebOperationsHandler cartWebOperationsHandler = new CartWebOperationsHandler(this.HttpContext);
                Cart cart = await cartWebOperationsHandler.AddItems(isCheckoutSession, cartLines);
                return this.Json(cart);
            }

            /// <summary>
            /// Removes the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="lineIds">The line ids.</param>
            /// <returns>The service response containing the updated shopping cart.</returns>
            public async Task<ActionResult> RemoveItems(bool isCheckoutSession, IEnumerable<string> lineIds)
            {
                CartWebOperationsHandler cartWebOperationsHandler = new CartWebOperationsHandler(this.HttpContext);
                Cart cart = await cartWebOperationsHandler.RemoveItems(isCheckoutSession, lineIds);
                return this.Json(cart);
            }

            /// <summary>
            /// Updates the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>The service response containing the updated shopping cart.</returns>
            public async Task<ActionResult> UpdateItems(bool isCheckoutSession, IEnumerable<CartLine> cartLines)
            {
                CartWebOperationsHandler cartWebOperationsHandler = new CartWebOperationsHandler(this.HttpContext);
                Cart cart = await cartWebOperationsHandler.UpdateItems(isCheckoutSession, cartLines);
                return this.Json(cart);
            }

            /// <summary>
            /// Gets the promotions.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <returns>The service response containing the applicable promotions.</returns>
            public async Task<ActionResult> GetPromotions(bool isCheckoutSession)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                CartPromotions cartPromotions = await cartOperationsHandler.GetPromotions(cartId);

                return this.Json(cartPromotions);
            }

            /// <summary>
            /// Adds the or remove promotion code.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="promotionCode">The promotion code.</param>
            /// <param name="isAdd">If set to <c>true</c> [is add].</param>
            /// <returns>The service response containing the updated shopping cart.</returns>
            public async Task<ActionResult> AddOrRemovePromotionCode(bool isCheckoutSession, string promotionCode, bool isAdd)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                Cart cart = await cartOperationsHandler.AddOrRemovePromotionCode(cartId, promotionCode, isAdd);

                return this.Json(cart);
            }

            /// <summary>
            /// Commences the checkout.
            /// </summary>
            /// <returns>A new cart with a random cart id that should be used during the secure checkout process.</returns>
            public async Task<ActionResult> CommenceCheckout()
            {
                CartWebOperationsHandler cartWebOperationsHandler = new CartWebOperationsHandler(this.HttpContext);
                Cart cart = await cartWebOperationsHandler.CommenceCheckout();
                return this.Json(cart);
            }

            /// <summary>
            /// Gets the cart affiliation loyalty tiers.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <returns>Returns response containing cart affiliation loyalty tiers.</returns>
            public async Task<ActionResult> GetCartAffiliationLoyaltyTiers(bool isCheckoutSession)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                IEnumerable<long> cartAffiliationLoyaltyTiers = await cartOperationsHandler.GetCartAffiliationLoyaltyTiers(cartId);

                return this.Json(cartAffiliationLoyaltyTiers);
            }

            /// <summary>
            /// Sets the cart affiliation loyalty tiers.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="tiers">The tiers.</param>
            /// <returns>Returns a null response.</returns>
            public async Task<ActionResult> SetCartAffiliationLoyaltyTiers(bool isCheckoutSession, IEnumerable<long> tiers)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                await cartOperationsHandler.SetCartAffiliationLoyaltyTiers(cartId, tiers);

                return this.Json(null);
            }

            /// <summary>
            /// Gets the delivery preferences.
            /// </summary>
            /// <returns>The delivery preference response object.</returns>
            public async Task<ActionResult> GetDeliveryPreferences()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                CartDeliveryPreferences cartDeliveryPreferences = await cartOperationsHandler.GetDeliveryPreferences(cartId);

                return this.Json(cartDeliveryPreferences);
            }

            /// <summary>
            /// Gets the order delivery options for shipping.
            /// </summary>
            /// <param name="shipToAddress">The ship to address.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>The service response containing the available delivery options.</returns>
            public async Task<ActionResult> GetOrderDeliveryOptionsForShipping(Address shipToAddress, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                PagedResult<DeliveryOption> deliveryOptions = await cartOperationsHandler.GetOrderDeliveryOptionsForShipping(cartId, shipToAddress, queryResultSettings);

                return this.Json(deliveryOptions);
            }

            /// <summary>
            /// Gets the line delivery options for shipping.
            /// </summary>
            /// <param name="lineShippingAddresses">The line shipping addresses.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>
            /// The service response containing the available delivery options for the specified lines.
            /// </returns>
            public async Task<ActionResult> GetLineDeliveryOptionsForShipping(IEnumerable<LineShippingAddress> lineShippingAddresses, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                PagedResult<SalesLineDeliveryOption> lineDeliveryOptions = await cartOperationsHandler.GetLineDeliveryOptionsForShipping(cartId, lineShippingAddresses, queryResultSettings);

                return this.Json(lineDeliveryOptions.Results);
            }

            /// <summary>
            /// Updates the delivery specification.
            /// </summary>
            /// <param name="deliverySpecification">The delivery specification.</param>
            /// <returns>The updated shopping cart.</returns>
            public async Task<ActionResult> UpdateDeliverySpecification(DeliverySpecification deliverySpecification)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                Cart cart = await cartOperationsHandler.UpdateDeliverySpecification(cartId, deliverySpecification);

                return this.Json(cart);
            }

            /// <summary>
            /// Updates the line delivery specifications.
            /// </summary>
            /// <param name="lineDeliverySpecifications">The line delivery specifications.</param>
            /// <returns>The updated shopping cart.</returns>
            public async Task<ActionResult> UpdateLineDeliverySpecifications(IEnumerable<LineDeliverySpecification> lineDeliverySpecifications)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                Cart cart = await cartOperationsHandler.UpdateLineDeliverySpecifications(cartId, lineDeliverySpecifications);

                return this.Json(cart);
            }

            /// <summary>
            /// Creates the order.
            /// </summary>
            /// <param name="cartTenderLines">The cart tender lines.</param>
            /// <param name="emailAddress">The email address.</param>
            /// <returns>The updated shopping cart response.</returns>
            public async Task<ActionResult> CreateOrder(IEnumerable<CartTenderLine> cartTenderLines, string emailAddress)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                SalesOrder salesOrder = await cartOperationsHandler.CreateOrder(cartId, cartTenderLines, emailAddress);

                ServiceUtilities.SetCartIdInResponseCookie(this.HttpContext, sessionType, null);

                return this.Json(salesOrder);
            }

            /// <summary>
            /// Gets the card payment accept point.
            /// </summary>
            /// <param name="cardPaymentAcceptSettings">The card payment accept settings.</param>
            /// <returns>Returns payment response.</returns>
            public async Task<ActionResult> GetCardPaymentAcceptPoint(CardPaymentAcceptSettings cardPaymentAcceptSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession: true);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                CardPaymentAcceptPoint cardPaymentAcceptPoint = await cartOperationsHandler.GetCardPaymentAcceptPoint(cartId, cardPaymentAcceptSettings);

                return this.Json(cardPaymentAcceptPoint);
            }

            /// <summary>
            /// Updates the loyalty card identifier.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="loyaltyCardId">The loyalty card identifier.</param>
            /// <returns>A response containing the updated shopping cart.</returns>
            public async Task<ActionResult> UpdateLoyaltyCardId(bool isCheckoutSession, string loyaltyCardId)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContext, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContext, sessionType);

                Cart cart = await cartOperationsHandler.UpdateLoyaltyCardId(cartId, loyaltyCardId);

                return this.Json(cart);
            }
        }
    }
}