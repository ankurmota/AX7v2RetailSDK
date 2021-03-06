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
    namespace Retail.Ecommerce.Web.Storefront
    {
        using System;
        using System.Web.Mvc;
        using System.Web.Routing;
    
        /// <summary>
        /// The core MVC application class.
        /// </summary>
        public class MvcApplication : System.Web.HttpApplication
        {
            /// <summary>
            /// Application start.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event arguments.</param>
            protected void Application_Start(object sender, EventArgs e)
            {
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
            }
        }
    }
}
