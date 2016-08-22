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
        using System.Web;
        using System.Web.Mvc;
    
        /// <summary>
        /// Filter class for default MVC Application.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors", Justification = "The class is instantiated by error handler.")]
        public sealed class FilterConfig
        {
            /// <summary>
            /// Registers all filters.
            /// </summary>
            /// <param name="filters">The parameter filters.</param>
            public static void RegisterGlobalFilters(GlobalFilterCollection filters)
            {
                if (filters == null)
                {
                    throw new ArgumentNullException("filters");
                }
    
                filters.Add(new HandleErrorAttribute());
            }
        }
    }
}
