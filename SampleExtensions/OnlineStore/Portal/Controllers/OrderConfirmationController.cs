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
        using System.Web.Mvc;

        /// <summary>
        /// Order Confirmation Controller.
        /// </summary>
        public class OrderConfirmationController : ActionControllerBase
        {
            private const string OrderConfirmationViewName = "OrderConfirmation";

            /// <summary>
            /// Return Order Confirmation View.
            /// </summary>
            /// <returns>Order Confirmation View.</returns>
            [HttpGet]
            public ActionResult Index()
            {
                return this.View(OrderConfirmationController.OrderConfirmationViewName);
            }
        }
    }
}
