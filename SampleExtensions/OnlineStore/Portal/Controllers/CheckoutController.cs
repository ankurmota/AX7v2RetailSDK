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
        /// Controller for checkout.
        /// </summary>
        public class CheckoutController : ActionControllerBase
        {
            private const string CheckoutViewName = "Checkout";

            /// <summary>
            /// Index for Checkout.
            /// </summary>
            /// <returns>View for Checkout.</returns>
            [HttpGet]
            public ActionResult Index()
            {
                if ((Request.UrlReferrer != null && Request.UrlReferrer.AbsolutePath.Contains("SignIn")) || this.HttpContext.Request.IsAuthenticated)
                {
                    // If navigation if from SignIn view or if user is authenticated
                    return this.View(CheckoutController.CheckoutViewName);
                }
                else
                {
                    // Otherwise navigate user to SignIn view 
                    this.TempData["IsCheckoutFlow"] = true;
                    return this.RedirectToAction(SignInController.DefaultActionName, SignInController.ControllerName);
                }
            }
        }
    }
}
