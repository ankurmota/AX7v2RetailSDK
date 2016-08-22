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
    namespace Retail.Ecommerce.Sdk.Controls
    {
        using System.Web.Mvc;

        /// <summary>
        /// MVC helper extensions for ecommerce controls.
        /// </summary>
        public static class MvcHelperExtension
        {
            /// <summary>
            /// Gets the header shared by all the controls.
            /// </summary>
            /// <returns>
            /// Base control header markup.
            /// </returns>
            public static MvcHtmlString GetBaseControlHeader()
            {
                string htmlContent;

                using (RetailWebControl control = new RetailWebControl())
                {
                    htmlContent = control.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the Checkout control.
            /// </summary>
            /// <returns>
            /// Checkout control markup.
            /// </returns>
            public static MvcHtmlString GetCheckoutControl()
            {
                string htmlContent;

                using (Checkout checkout = new Checkout())
                {
                    htmlContent = checkout.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for Checkout control header.
            /// </summary>
            /// <param name="orderConfirmationUrl">The redirection url for order confirmation page.</param>
            /// <param name="hasInventoryCheck">Boolean value indicating whether inventory check is added during pick up in store scenario.</param>
            /// <returns>
            /// Checkout control header markup.
            /// </returns>
            public static MvcHtmlString GetCheckoutControlHeader(string orderConfirmationUrl, bool hasInventoryCheck)
            {
                string htmlContent;

                using (Checkout checkout = new Checkout())
                {
                    checkout.OrderConfirmationUrl = orderConfirmationUrl;
                    checkout.HasInventoryCheck = hasInventoryCheck;

                    htmlContent = checkout.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the shopping cart control.
            /// </summary>
            /// <returns>
            /// Shopping cart control markup.
            /// </returns>
            public static MvcHtmlString GetShoppingCartControl()
            {
                string htmlContent;

                using (ShoppingCart cart = new ShoppingCart())
                {
                    htmlContent = cart.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for shopping cart control header.
            /// </summary>
            /// <param name="checkoutUrl">The redirection url for checkout page.</param>
            /// <param name="continueShoppingUrl">The redirection url for the continue shopping page.</param>
            /// <returns>
            /// Shopping cart control header markup.
            /// </returns>
            public static MvcHtmlString GetShoppingCartControlHeader(string checkoutUrl, string continueShoppingUrl)
            {
                string htmlContent;

                using (ShoppingCart cart = new ShoppingCart())
                {
                    cart.CheckoutUrl = checkoutUrl;
                    cart.ContinueShoppingUrl = continueShoppingUrl;

                    htmlContent = cart.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the mini cart control.
            /// </summary>
            /// <returns>
            /// Mini cart control markup.
            /// </returns>
            public static MvcHtmlString GetMiniCartControl()
            {
                string htmlContent;

                using (MiniCart cart = new MiniCart())
                {
                    htmlContent = cart.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for Mini cart control header.
            /// </summary>
            /// <param name="checkoutUrl">The redirection url for checkout page.</param>
            /// <param name="shoppingCartUrl">The redirection url for shopping cart page.</param>
            /// <param name="isCheckoutCart">Boolean value indicating whether the cart is checkout cart.</param>
            /// <returns>
            /// Mini cart control header markup.
            /// </returns>
            public static MvcHtmlString GetMiniCartControlHeader(string checkoutUrl, string shoppingCartUrl, bool isCheckoutCart)
            {
                string htmlContent;

                using (MiniCart cart = new MiniCart())
                {
                    cart.CheckoutUrl = checkoutUrl;
                    cart.ShoppingCartUrl = shoppingCartUrl;
                    cart.IsCheckoutCart = isCheckoutCart;

                    htmlContent = cart.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the Order history control.
            /// </summary>
            /// <returns>
            /// Order history control markup.
            /// </returns>
            public static MvcHtmlString GetOrderHistoryControl()
            {
                string htmlContent;

                using (OrderHistory orderHistory = new OrderHistory())
                {
                    htmlContent = orderHistory.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for Order history control header.
            /// </summary>
            /// <param name="orderCount">Indicates the amount of orders shown (per page or total depending on the value of ShowPaging parameter).</param>
            /// <param name="showPaging">Indicates whether paging should be displayed.</param>
            /// <param name="orderDetailsUrl">Indicates the Url of the order details.</param>
            /// <returns>
            /// Order history control header markup.
            /// </returns>
            public static MvcHtmlString GetOrderHistoryControlHeader(int orderCount, bool showPaging, string orderDetailsUrl)
            {
                string htmlContent;

                using (OrderHistory orderHistory = new OrderHistory())
                {
                    orderHistory.OrderCount = orderCount;
                    orderHistory.ShowPaging = showPaging;
                    orderHistory.OrderDetailsUrl = orderDetailsUrl;

                    htmlContent = orderHistory.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the customer address control.
            /// </summary>
            /// <returns>
            /// Customer address control markup.
            /// </returns>
            public static MvcHtmlString GetCustomerAddressControl()
            {
                string htmlContent;

                using (CustomerAddress customerAddress = new CustomerAddress())
                {
                    htmlContent = customerAddress.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for customer address control header.
            /// </summary>
            /// <returns>
            /// Customer address control header markup.
            /// </returns>
            public static MvcHtmlString GetCustomerAddressControlHeader()
            {
                string htmlContent;

                using (CustomerAddress customerAddress = new CustomerAddress())
                {
                    htmlContent = customerAddress.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the Order details control.
            /// </summary>
            /// <returns>
            /// Order details control markup.
            /// </returns>
            public static MvcHtmlString GetOrderDetailsControl()
            {
                string htmlContent;

                using (OrderDetails orderDetails = new OrderDetails())
                {
                    htmlContent = orderDetails.GetControlMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }

            /// <summary>
            /// Gets the header for Order details control header.
            /// </summary>
            /// <returns>
            /// Order details control header markup.
            /// </returns>
            public static MvcHtmlString GetOrderDetailsControlHeader()
            {
                string htmlContent;

                using (OrderDetails orderDetails = new OrderDetails())
                {
                    htmlContent = orderDetails.GetHeaderMarkup();
                }

                return MvcHtmlString.Create(htmlContent);
            }
        }
    }
}