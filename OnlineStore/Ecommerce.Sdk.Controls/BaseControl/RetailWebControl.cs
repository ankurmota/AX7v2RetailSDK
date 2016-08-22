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
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Scripts.js", "application/x-javascript")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.BaseControl.RetailWebControl.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_i_forward_lrg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_i_back_lrg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_bl_bg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_bl_left.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_bl_right.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_gr_bg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_gr_left.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_lrg_gr_right.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Arrow_Left_16_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Arrow_Right_16_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Delete_16_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Minus_16_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Plus_16_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.input_bg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.product_sm_cart_placeholder.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_search.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Edit_19_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Update_24_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Arrow_Left_24_on.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.btn_Checkout_sm.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.update_bg.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.loading_animation_lg.gif", "image/gif")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.banner_burst.png", "image/png")]
[assembly: WebResource("Contoso.Retail.Ecommerce.Sdk.Controls.Common.Images.i_Close_48_on.png", "image/png")]

namespace Contoso
{
    namespace Retail.Ecommerce.Sdk.Controls
    {
        /// <summary>
        /// RetailWebControl base class.
        /// </summary>
        [ComVisible(false)]
        public class RetailWebControl : WebControl
        {
            /// <summary>
            /// Base CSS class for control.
            /// </summary>
            protected const string CssClassName = "msax-Control";

            private const string FilePathFormat = "{0}.{1}";
            private ControlsSection controlsSection;

            /// <summary>
            /// Gets or sets the ControlsSection of control from section "ecommerceControls" in web.config.
            /// </summary>
            protected ControlsSection ControlsSection
            {
                get
                {
                    return this.controlsSection;
                }

                set
                {
                    this.controlsSection = value;
                }
            }

            /// <summary>
            /// Gets the markup to include base control scripts, CSS, startup scripts in the page.
            /// </summary>
            /// <returns>The header markup.</returns>
            internal virtual string GetHeaderMarkup()
            {
                Collection<string> cssUrls = this.GetCssUrls();
                Collection<string> scriptUrls = this.GetScriptUrls();

                string existingHeaderMarkup = this.GetExistingHeaderMarkup();
                string output = this.GetCssAndScriptMarkup(existingHeaderMarkup, cssUrls, scriptUrls);

                output += this.GetStartupMarkup(existingHeaderMarkup);

                return output;
            }

            /// <summary>
            /// Gets all the script URLs required for the control.
            /// </summary>
            /// <returns>Collection of script URLs.</returns>
            protected virtual Collection<string> GetScriptUrls()
            {
                return new Collection<string> { this.GetFileUrl("Common.Scripts.js") };
            }

            /// <summary>
            /// Gets all the CSS URLs required for the control.
            /// </summary>
            /// <returns>Collection of CSS URLs.</returns>
            protected virtual Collection<string> GetCssUrls()
            {
                return new Collection<string> { this.GetFileUrl("BaseControl.RetailWebControl.css") };
            }

            /// <summary>
            /// Gets the startup markup required for the control.
            /// </summary>
            /// <param name="existingHeaderMarkup">Existing header markup to determine if startup script registration is required.</param>
            /// <returns>Startup markup.</returns>
            protected virtual string GetStartupMarkup(string existingHeaderMarkup)
            {
                ControlsSection = (ControlsSection)WebConfigurationManager.GetSection("ecommerceControls");
                bool configExists = ControlsSection != null
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.CartWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.CustomerWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.OrgUnitWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.ProductWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.SalesOrderWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.Services.RetailOperationsWebApiUrl)
                    && !string.IsNullOrWhiteSpace(ControlsSection.ProductDetailsUrlTemplate);

                if (configExists)
                {
                    string cartWebApiUrl = ControlsSection.Services.CartWebApiUrl;
                    string customerWebApiUrl = ControlsSection.Services.CustomerWebApiUrl;
                    string orgUnitWebApiUrl = ControlsSection.Services.OrgUnitWebApiUrl;
                    string productWebApiUrl = ControlsSection.Services.ProductWebApiUrl;
                    string salesOrderWebApiUrl = ControlsSection.Services.SalesOrderWebApiUrl;
                    string retailOperationsWebApiUrl = ControlsSection.Services.RetailOperationsWebApiUrl;
                    string productDetailsUrlTemplate = ControlsSection.ProductDetailsUrlTemplate;

                    string startupScript = string.Empty;
                    string baseStartupScript = string.Format(
                        @"<script type='text/javascript'> var msaxValues = {{
                            msax_ProductDetailsUrlTemplate:'{0}',
                            msax_CartWebApiUrl:'{1}',
                            msax_CustomerWebApiUrl:'{2}',
                            msax_OrgUnitWebApiUrl: '{3}',
                            msax_ProductWebApiUrl: '{4}',
                            msax_SalesOrderWebApiUrl: '{5}',
                            msax_RetailOperationsWebApiUrl: '{6}'
                        }};

                    </script>",
                        productDetailsUrlTemplate,
                        cartWebApiUrl,
                        customerWebApiUrl,
                        orgUnitWebApiUrl,
                        productWebApiUrl,
                        salesOrderWebApiUrl,
                        retailOperationsWebApiUrl);

                    // If page header is visible here, check if the startup script markup is present in the header already.
                    if (this.Page == null || this.Page.Header == null || (existingHeaderMarkup != null && !existingHeaderMarkup.Contains(baseStartupScript)))
                    {
                        startupScript += baseStartupScript;
                    }

                    return startupScript;
                }
                else
                {
                    throw new System.Configuration.ConfigurationErrorsException("The required configuration for the ecommerce controls does not exist.");
                }
            }

            /// <summary>
            /// Raises the PreRender event.
            /// </summary>
            /// <param name="e">An <see cref="System.EventArgs"/> object that contains the event data.</param>
            protected override void OnPreRender(EventArgs e)
            {
                base.OnPreRender(e);

                this.CssClass = CssClassName;
            }

            /// <summary>
            /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
            /// </summary>
            /// <param name="writer">A <see cref="System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
            protected override void RenderContents(HtmlTextWriter writer)
            {
                if (writer != null)
                {
                    string htmlContent = this.GetHtml();
                    writer.Write(htmlContent);
                }
            }

            /// <summary>
            /// Gets the control html markup.
            /// </summary>
            /// <returns>The control markup.</returns>
            protected virtual string GetHtml()
            {
                return string.Empty;
            }

            /// <summary>
            /// Gets the control html markup with the control class wrapper added.
            /// </summary>
            /// <param name="cssClassName">The CSS class name.</param>
            /// <returns>The control markup.</returns>
            protected string GetControlMarkup(string cssClassName)
            {
                string htmlContent = this.GetHtml();

                // Adding wrapper with the control's cssClass.
                return "<div class=\"" + cssClassName + "\">" + htmlContent + "</div>";
            }

            /// <summary>
            /// Gets the existing header markup.
            /// </summary>
            /// <returns>The existing header markup.</returns>
            /// <remarks>Preventing markup getting registered multiple times in a page when using multiple controls.</remarks>
            protected string GetExistingHeaderMarkup()
            {
                string existingHeaderMarkup = string.Empty;

                if (this.Page != null && this.Page.Header != null)
                {
                    foreach (Control control in this.Page.Header.Controls)
                    {
                        if (control.GetType() == typeof(LiteralControl))
                        {
                            LiteralControl literalControl = (LiteralControl)control;
                            existingHeaderMarkup += literalControl.Text;
                        }
                    }
                }

                return existingHeaderMarkup;
            }

            /// <summary>
            /// Gets the markup to include control scripts, CSS in the page.
            /// </summary>
            /// <param name="existingHeaderMarkup">The existing header markup.</param>
            /// <param name="cssUrls">The CSS URLs.</param>
            /// <param name="scriptUrls">The script URLs.</param>
            /// <returns>The part of the header markup that contains control scripts, CSS.</returns>
            protected string GetCssAndScriptMarkup(string existingHeaderMarkup, Collection<string> cssUrls, Collection<string> scriptUrls)
            {
                string output = string.Empty;

                foreach (string cssUrl in cssUrls)
                {
                    string cssLink = string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />", cssUrl);

                    // If page header is visible here, check if the markup is present in the header already.
                    if (this.Page == null || this.Page.Header == null || !existingHeaderMarkup.Contains(cssLink))
                    {
                        output += cssLink;
                    }
                }

                foreach (string scriptUrl in scriptUrls)
                {
                    string scriptLink = string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", scriptUrl);

                    // If page header is visible here, check if the markup is present in the header already.
                    if (this.Page == null || this.Page.Header == null || !existingHeaderMarkup.Contains(scriptLink))
                    {
                        output += scriptLink;
                    }
                }

                return output;
            }

            /// <summary>
            /// Adds the given header markup to the page header.
            /// </summary>
            /// <param name="headerMarkup">The header markup.</param>
            protected virtual void RegisterHeaderMarkup(string headerMarkup)
            {
                // Checking if page header is visible here before registering markup.
                // This way if the client does not use runat='server' on the page header, we will not be throwing any error.
                if (!string.IsNullOrWhiteSpace(headerMarkup) && this.Page.Header != null)
                {
                    this.Page.Header.Controls.Add(new LiteralControl(headerMarkup));
                }
            }

            /// <summary>
            /// Get html content of the control.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <returns>Html content of the file.</returns>
            protected string GetHtmlFragment(string fileName)
            {
                string resxHtml = this.GetResourceText(fileName);
                string result = string.Empty;

                // Exctract content inside of the placeholders.
                RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
                Regex regx = new Regex("<!--CONTENT_START-->(?<controlHtmlContent>.*)<!--CONTENT_END-->", options);
                Match match = regx.Match(resxHtml);

                if (match.Success)
                {
                    result = match.Groups["controlHtmlContent"].Value;
                }

                return result;
            }

            /// <summary>
            /// Gets the url corresponding to the given file.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <returns>The web resource url.</returns>
            protected string GetFileUrl(string fileName)
            {
                // This check and initialization is for MVC support.
                if (this.Page == null)
                {
                    this.Page = new Page();
                }

                Type type = typeof(RetailWebControl);
                return this.Page.ClientScript.GetWebResourceUrl(type, string.Format(FilePathFormat, type.Namespace, fileName));
            }

            /// <summary>
            /// Get text from embedded resource.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <returns>Text from embedded resource file.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Objects disposed properly.")]
            private string GetResourceText(string fileName)
            {
                string result = string.Empty;
                Type type = this.GetType();

                using (Stream stream = type.Assembly.GetManifestResourceStream(string.Format(FilePathFormat, type.Namespace, fileName)))
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                return result;
            }
        }
    }
}