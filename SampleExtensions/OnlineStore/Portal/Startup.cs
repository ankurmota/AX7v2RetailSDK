/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

[assembly: Microsoft.Owin.OwinStartup(typeof(Contoso.Retail.Ecommerce.Web.Storefront.Startup))]

namespace Contoso
{
    namespace Retail.Ecommerce.Web.Storefront
    {
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Microsoft.Dynamics.Retail.Diagnostics.Core.Desktop;
        using global::Owin;

        /// <summary>
        /// Entry point for OWIN.
        /// </summary>
        public partial class Startup
        {
            /// <summary>
            /// Configurations the specified application.
            /// </summary>
            /// <param name="app">The application.</param>
            public void Configuration(IAppBuilder app)
            {
                DiagnosticsConfigManager.Instance.Initialize(DiagnosticsConfigSection.Instance);
                this.ConfigureAuth(app);
            }
        }
    }
}