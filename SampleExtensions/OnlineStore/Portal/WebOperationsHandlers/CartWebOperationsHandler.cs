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
    namespace Retail.Ecommerce.Web.Storefront.WebOperationsHandlers
    {
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;
        using System.Web;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Handler for cart operations that require access to web context.
        /// </summary>
        public class CartWebOperationsHandler
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CartWebOperationsHandler"/> class.
            /// </summary>
            /// <param name="httpContextBase">The HTTP context base.</param>
            public CartWebOperationsHandler(HttpContextBase httpContextBase)
            {
                if (httpContextBase == null)
                {
                    throw new ArgumentNullException(nameof(httpContextBase));
                }

                this.HttpContextBase = httpContextBase;
            }

            /// <summary>
            /// Gets the HTTP context base.
            /// </summary>
            /// <value>
            /// The HTTP context base.
            /// </value>
            public HttpContextBase HttpContextBase { get;  private set; }

            /// <summary>
            /// Adds the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>The updated cart.</returns>
            public async Task<Cart> AddItems(bool isCheckoutSession, IEnumerable<CartLine> cartLines)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContextBase);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContextBase, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, sessionType);

                foreach (CartLine cartLine in cartLines)
                {
                    if (cartLine.Price != null && cartLine.Price != 0)
                    {
                        cartLine.IsPriceKeyedIn = true;
                    }
                }

                Cart cart = await cartOperationsHandler.AddItems(cartId, cartLines, sessionType);
                if (cart != null)
                {
                    ServiceUtilities.SetCartIdInResponseCookie(this.HttpContextBase, sessionType, cart.Id);
                }

                cart = await DataAugmenter.GetAugmentedCart(ecommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Removes the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="lineIds">The line ids.</param>
            /// <returns>The updated cart.</returns>
            public async Task<Cart> RemoveItems(bool isCheckoutSession, IEnumerable<string> lineIds)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContextBase);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContextBase, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, sessionType);

                Cart cart = await cartOperationsHandler.RemoveItems(cartId, lineIds);

                // For checkout sessions, reflect the changes in shopping cart as well.
                if (sessionType == SessionType.AnonymousCheckout || sessionType == SessionType.SignedInCheckout)
                {
                    string shoppingCartId = null;
                    if (sessionType == SessionType.AnonymousCheckout)
                    {
                        shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.AnonymousShopping);
                    }
                    else if (sessionType == SessionType.SignedInCheckout)
                    {
                        shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.SignedInShopping);
                    }

                    await cartOperationsHandler.RemoveItems(shoppingCartId, lineIds);
                }

                cart = await DataAugmenter.GetAugmentedCart(ecommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Updates the items.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <param name="cartLines">The cart lines.</param>
            /// <returns>The updated cart.</returns>
            public async Task<Cart> UpdateItems(bool isCheckoutSession, IEnumerable<CartLine> cartLines)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContextBase);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContextBase, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, sessionType);

                Cart cart = await cartOperationsHandler.UpdateItems(cartId, cartLines);

                // For checkout sessions, reflect the changes in shopping cart as well.
                if (sessionType == SessionType.AnonymousCheckout || sessionType == SessionType.SignedInCheckout)
                {
                    string shoppingCartId = null;
                    if (sessionType == SessionType.AnonymousCheckout)
                    {
                        shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.AnonymousShopping);
                    }
                    else if (sessionType == SessionType.SignedInCheckout)
                    {
                        shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.SignedInShopping);
                    }

                    await cartOperationsHandler.UpdateItems(cartId, cartLines);
                }

                cart = await DataAugmenter.GetAugmentedCart(ecommerceContext, cart);
                return cart;
            }

            /// <summary>
            /// Commences the checkout.
            /// </summary>
            /// <returns>Checkout cart.</returns>
            public async Task<Cart> CommenceCheckout()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContextBase);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                string shoppingCartId;
                string previousCheckoutCartId;
                bool isSignedIn = this.HttpContextBase.Request.IsAuthenticated;
                if (isSignedIn)
                {
                    shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.SignedInShopping);
                    previousCheckoutCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.SignedInCheckout);
                }
                else
                {
                    shoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.AnonymousShopping);
                    previousCheckoutCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.AnonymousCheckout);
                }

                // Shopping cart would be null if the user lands on the checkout page immediately after signing in.
                // In this case we need to claim the anonymous shopping cart and assign it to the signed in user,
                // because there is no explicit GetShoppingCart call, which implicitly does the claiming, in the checkout page.
                if (string.IsNullOrWhiteSpace(shoppingCartId) && isSignedIn)
                {
                    Cart claimedShoppingCart = await this.GetCart(isCheckoutSession: true);
                    shoppingCartId = claimedShoppingCart.Id;
                    ServiceUtilities.SetCartIdInResponseCookie(this.HttpContextBase, SessionType.SignedInShopping, shoppingCartId);
                }

                Cart checkoutCart = await cartOperationsHandler.CommenceCheckout(shoppingCartId, previousCheckoutCartId);

                if (checkoutCart == null)
                {
                    string message = string.Format("Unable to create a checkout cart from shopping cart id: {0}", shoppingCartId);
                    throw new DataValidationException(DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), message);
                }

                // Update the checkout cart id cookie.
                SessionType sessionType = isSignedIn ? SessionType.SignedInCheckout : SessionType.AnonymousCheckout;
                ServiceUtilities.SetCartIdInResponseCookie(this.HttpContextBase, sessionType, checkoutCart.Id);

                checkoutCart = await DataAugmenter.GetAugmentedCart(ecommerceContext, checkoutCart);
                return checkoutCart;
            }

            /// <summary>
            /// Gets the cart.
            /// </summary>
            /// <param name="isCheckoutSession">If set to <c>true</c> [is checkout session].</param>
            /// <returns>The cart object.</returns>
            public async Task<Cart> GetCart(bool isCheckoutSession)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContextBase);
                CartOperationsHandler cartOperationsHandler = new CartOperationsHandler(ecommerceContext);

                SessionType sessionType = ServiceUtilities.GetSessionType(this.HttpContextBase, isCheckoutSession);
                string cartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, sessionType);

                Cart cart = null;
                if (sessionType == SessionType.AnonymousCheckout || sessionType == SessionType.SignedInCheckout)
                {
                    cart = await cartOperationsHandler.GetCart(cartId);
                }
                else
                {
                    if (sessionType == SessionType.AnonymousShopping)
                    {
                        if (!string.IsNullOrEmpty(cartId))
                        {
                            cart = await cartOperationsHandler.GetCart(cartId);
                        }
                    }
                    else if (sessionType == SessionType.SignedInShopping)
                    {
                        // Get the latest cart associated with the user.
                        Cart activeAuthenticatedShoppingCart = await cartOperationsHandler.GetActiveShoppingCart();
                        string anonymousShoppingCartId = ServiceUtilities.GetCartIdFromRequestCookie(this.HttpContextBase, SessionType.AnonymousShopping);
                        bool isAnonymousShoppingCartIdSet = !string.IsNullOrWhiteSpace(anonymousShoppingCartId);

                        if ((activeAuthenticatedShoppingCart == null) && isAnonymousShoppingCartIdSet)
                        {
                            // Claim the shopping cart id present in the cookie.
                            activeAuthenticatedShoppingCart = await cartOperationsHandler.ClaimAnonymousCart(anonymousShoppingCartId);
                        }
                        else if ((activeAuthenticatedShoppingCart != null) && isAnonymousShoppingCartIdSet)
                        {
                            // Move items from the anonymous shopping cart to the authenticated shopping cart.
                            activeAuthenticatedShoppingCart = await cartOperationsHandler.MoveItemsBetweenCarts(anonymousShoppingCartId, activeAuthenticatedShoppingCart.Id, sessionType);
                        }

                        // Clear anonymous shopping cart identifier.
                        ServiceUtilities.SetCartIdInResponseCookie(this.HttpContextBase, SessionType.AnonymousShopping, string.Empty);

                        cart = activeAuthenticatedShoppingCart;
                    }
                    else
                    {
                        string message = string.Format("Invalid session type encountered: {0}.", sessionType);
                        throw new NotSupportedException(message);
                    }

                    if (cart == null)
                    {
                        cart = await cartOperationsHandler.CreateEmptyCart();
                    }
                }

                ServiceUtilities.SetCartIdInResponseCookie(this.HttpContextBase, sessionType, cart.Id);

                cart = await DataAugmenter.GetAugmentedCart(ecommerceContext, cart);
                return cart;
            }
        }
    }
}