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

[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.CustomerAddress.CustomerAddress.css", "text/css", PerformSubstitution = true)]

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Controls
    {
        /// <summary>
        /// Customer address control.
        /// </summary>
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
        [ToolboxData("<{0}:CustomerAddress runat=server></{0}:CustomerAddress>")]
        [ComVisible(false)]
        public class CustomerAddress : RetailWebControl
        {
            /// <summary>
            /// Gets the markup to include control scripts, styles, startup scripts in the page.
            /// </summary>
            /// <returns>The header markup.</returns>
            internal new string GetHeaderMarkup()
            {
                Collection<string> cssUrls = this.GetCssUrls();
                Collection<string> scriptUrls = this.GetScriptUrls();

                string existingHeaderMarkup = this.GetExistingHeaderMarkup();
                string output = this.GetCssAndScriptMarkup(existingHeaderMarkup, cssUrls, scriptUrls);
                output += this.GetStartupMarkup(existingHeaderMarkup);

                return output;
            }

            /// <summary>
            /// Gets the control html markup with the control class wrapper added.
            /// </summary>
            /// <returns>The control markup.</returns>
            internal string GetControlMarkup()
            {
                return base.GetControlMarkup(CustomerAddress.CssClassName);
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
                return this.GetHtmlFragment("CustomerAddress.CustomerAddress.html");
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
                cssUrls.Add(this.GetFileUrl("CustomerAddress.CustomerAddress.css"));

                return cssUrls;
            }

            /// <summary>
            /// Gets the startup markup.
            /// </summary>
            /// <param name="existingHeaderMarkup">Existing header markup to determine if startup script registration is required.</param>
            /// <returns>The startup markup.</returns>
            private new string GetStartupMarkup(string existingHeaderMarkup)
            {
                return string.Empty;
            }
        }
    }
}