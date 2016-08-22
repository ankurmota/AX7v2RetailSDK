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
using System.Web.Configuration;
using System.Web.UI;

[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Checkout.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.electronic_delivery_info.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.i_shipping_truck.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_end.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_end_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_bg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_bg_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_left.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_left_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_right.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.progress_step_right_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Checkout.Images.i_Info_16.png", "image/png")]

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Controls
    {
        /// <summary>
        /// Checkout control.
        /// </summary>
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
        [ToolboxData("<{0}:Checkout runat=server></{0}:Checkout>")]
        [ComVisible(false)]
        public class Checkout : RetailWebControl
        {
            private bool hasInventoryCheck = true;
            private bool reviewDisplayPromotionBanner = true;

            /// <summary>
            /// Gets or sets the value indicating the order confirmation redirect url.
            /// </summary>
            public string OrderConfirmationUrl
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the control includes inventory check in pick up in store flow.
            /// </summary>
            public bool HasInventoryCheck
            {
                get { return this.hasInventoryCheck; }
                set { this.hasInventoryCheck = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether cart in order review section displays promotion banners.
            /// </summary>
            public bool ReviewDisplayPromotionBanner
            {
                get { return this.reviewDisplayPromotionBanner; }
                set { this.reviewDisplayPromotionBanner = value; }
            }

            /// <summary>
            /// Gets the markup to include control scripts, CSS, startup scripts in the page.
            /// </summary>
            /// <returns>The header markup.</returns>
            internal new string GetHeaderMarkup()
            {
                Collection<string> cssUrls = this.GetCssUrls();
                Collection<string> scriptUrls = this.GetScriptUrls();

                string existingHeaderMarkup = this.GetExistingHeaderMarkup();
                string output = this.GetCssAndScriptMarkup(existingHeaderMarkup, cssUrls, scriptUrls);

                string bingMapsLink = string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", "https://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0&s=1");

                // If page header is visible here, check if the markup is present in the header already.
                if (this.Page == null || this.Page.Header == null || !existingHeaderMarkup.Contains(bingMapsLink))
                {
                    output += bingMapsLink;
                }

                output += this.GetStartupMarkup(existingHeaderMarkup);

                return output;
            }

            /// <summary>
            /// Gets the control html markup with the control class wrapper added.
            /// </summary>
            /// <returns>The control markup.</returns>
            internal string GetControlMarkup()
            {
                return base.GetControlMarkup(Checkout.CssClassName);
            }

            /// <summary>
            /// Raises the PreRender event.
            /// </summary>
            /// <param name="e">An <see cref="System.EventArgs"/> Object that contains the event data.</param>
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
            /// <param name="writer">A <see cref="System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client. </param>
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
                return this.GetHtmlFragment("Checkout.Checkout.html");
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
            /// <returns>The CSS URLS.</returns>
            private new Collection<string> GetCssUrls()
            {
                Collection<string> cssUrls = new Collection<string>();
                cssUrls.Add(this.GetFileUrl("Checkout.Checkout.css"));

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
                this.ControlsSection = (ControlsSection)WebConfigurationManager.GetSection("ecommerceControls");
                bool configExists = this.ControlsSection != null;

                if (configExists)
                {
                    bool isDemoMode = ControlsSection.Checkout.IsDemoMode;
                    string demoDataPath = ControlsSection.Checkout.DemoDataPath;

                    string checkoutStartupScript = string.Format(
                        @"<script type='text/javascript'>
                        msaxValues['msax_OrderConfirmationUrl'] = '{0}';
                        msaxValues['msax_IsDemoMode'] = '{1}';
                        msaxValues['msax_HasInventoryCheck'] = '{2}';
                        msaxValues['msax_DemoDataPath'] = '{3}';
                        msaxValues['msax_ReviewDisplayPromotionBanner'] = '{4}';
                    </script>",
                        this.OrderConfirmationUrl,
                        isDemoMode,
                        this.HasInventoryCheck,
                        demoDataPath,
                        this.ReviewDisplayPromotionBanner);

                    // If page header is visible here, check if the markup is present in the header already.
                    if (this.Page == null || this.Page.Header == null || (existingHeaderMarkup != null && !existingHeaderMarkup.Contains(checkoutStartupScript)))
                    {
                        startupScript += checkoutStartupScript;
                    }

                    return startupScript;
                }
                else
                {
                    throw new NotSupportedException("The required configuration for the checkout control does not exist");
                }
            }
        }
    }
}