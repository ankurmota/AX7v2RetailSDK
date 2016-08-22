/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.ShoppingCart.ShoppingCart.css", "text/css", PerformSubstitution = true)]

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Controls
    {
        /// <summary>
        /// Shopping cart control.
        /// </summary>
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
        [ToolboxData("<{0}:ShoppingCart runat=server></{0}:ShoppingCart>")]
        [ComVisible(false)]
        public class ShoppingCart : RetailWebControl
        {
            private bool supportDiscountCodes = true;
            private bool supportLoyaltyReward = true;
            private bool cartDisplayPromotionBanner = true;

            /// <summary>
            /// Gets or sets the value indicating the checkout redirect url.
            /// </summary>
            public string CheckoutUrl { get; set; }

            /// <summary>
            /// Gets or sets the value indicating the continue shopping redirect url.
            /// </summary>
            public string ContinueShoppingUrl { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether cart supports adding or removing discount codes.
            /// </summary>
            public bool SupportDiscountCodes
            {
                get { return this.supportDiscountCodes; }
                set { this.supportDiscountCodes = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether cart supports adding loyalty for earning reward points.
            /// </summary>
            public bool SupportLoyaltyReward
            {
                get { return this.supportLoyaltyReward; }
                set { this.supportLoyaltyReward = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether cart displays promotion banners.
            /// </summary>
            public bool CartDisplayPromotionBanner
            {
                get { return this.cartDisplayPromotionBanner; }
                set { this.cartDisplayPromotionBanner = value; }
            }

            /// <summary>
            /// Gets the markup to include control scripts, CSS, startup scripts in the page.
            /// </summary>
            /// <returns>The header markup.</returns>
            internal new string GetHeaderMarkup()
            {
                Collection<string> cssUrls = this.GetCssUrls();
                Collection<string> scriptUrls = this.GetScriptUrls();

                string existingHeaderMarkup = GetExistingHeaderMarkup();
                string output = GetCssAndScriptMarkup(existingHeaderMarkup, cssUrls, scriptUrls);
                output += this.GetStartupMarkup(existingHeaderMarkup);

                return output;
            }

            /// <summary>
            /// Gets the control html markup with the control class wrapper added.
            /// </summary>
            /// <returns>The control markup.</returns>
            internal string GetControlMarkup()
            {
                return base.GetControlMarkup(ShoppingCart.CssClassName);
            }

            /// <summary>
            /// Raises the PreRender event.
            /// </summary>
            /// <param name="e">An <see cref="System.EventArgs"/> object that contains the event data.</param>
            protected override void OnPreRender(EventArgs e)
            {
                base.OnPreRender(e);

                string headerMarkup = base.GetHeaderMarkup();
                headerMarkup += this.GetHeaderMarkup();
                this.RegisterHeaderMarkup(headerMarkup);
            }

            /// <summary>
            /// Call base control RegisterHeaderMarkup event.
            /// </summary>
            /// <param name="headerMarkup"> Object that contains header markup.</param>
            protected override void RegisterHeaderMarkup(string headerMarkup)
            {
                base.RegisterHeaderMarkup(headerMarkup);
            }

            /// <summary>
            /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
            /// </summary>
            /// <param name="writer">A <see cref="System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
            protected override void RenderContents(HtmlTextWriter writer)
            {
                base.RenderContents(writer);
            }

            /// <summary>
            /// Gets the control html markup.
            /// </summary>
            /// <returns>The control markup.</returns>
            protected override string GetHtml()
            {
                return this.GetHtmlFragment("ShoppingCart.ShoppingCart.html");
            }

            /// <summary>
            /// Gets the script URLs.
            /// </summary>
            /// <returns>The script URLs.</returns>
            private new Collection<string> GetScriptUrls()
            {
                return new Collection<string>();
            }

            /// <summary>
            /// Gets the CSS URLs.
            /// </summary>
            /// <returns>The CSS URLs.</returns>
            private new Collection<string> GetCssUrls()
            {
                Collection<string> cssUrls = new Collection<string>();
                cssUrls.Add(this.GetFileUrl("ShoppingCart.ShoppingCart.css"));
                
                return cssUrls;
            }

            /// <summary>
            /// Gets the startup markup.
            /// </summary>
            /// <param name="existingHeaderMarkup">Existing header markup to determine if startup script registration is required.</param>
            /// <returns>The startup markup.</returns>
            private new string GetStartupMarkup(string existingHeaderMarkup)
            {
                string startupScript = string.Empty;

                string cartStartupScript = string.Format(
                    @"<script type='text/javascript'>
                    msaxValues['msax_CheckoutUrl'] = '{0}';
                    msaxValues['msax_ContinueShoppingUrl'] = '{1}';
                    msaxValues['msax_CartDiscountCodes'] = '{2}';
                    msaxValues['msax_CartLoyaltyReward'] = '{3}';
                    msaxValues['msax_CartDisplayPromotionBanner'] = '{4}';
                </script>",
                    this.CheckoutUrl,
                    this.ContinueShoppingUrl,
                    this.SupportDiscountCodes,
                    this.SupportLoyaltyReward,
                    this.CartDisplayPromotionBanner);

                // If page header is visible here, check if the markup is present in the header already.
                if (this.Page == null || this.Page.Header == null || (existingHeaderMarkup != null && !existingHeaderMarkup.Contains(cartStartupScript)))
                {
                    startupScript += cartStartupScript;
                }

                return startupScript;
            }
        }
    }
}