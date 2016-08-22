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
        using System.Collections.Generic;
        using System.Security.Claims;
        using System.Threading.Tasks;
        using System.Web;
        using System.Web.Mvc;
        using Microsoft.Owin;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;
        using Retail.Ecommerce.Web.Storefront.ViewModels;

        /// <summary>
        /// MyAccount Controller.
        /// </summary>
        public class MyAccountController : ActionControllerBase
        {
            /// <summary>
            /// The controller name.
            /// </summary>
            public const string ControllerName = "MyAccount";

            /// <summary>
            /// The default action name.
            /// </summary>
            public const string DefaultActionName = "Index";

            /// <summary>
            /// The order history action name.
            /// </summary>
            public const string OrderHistoryActionName = "OrderHistory";

            /// <summary>
            /// The unlink account action name.
            /// </summary>
            public const string UnlinkAccountActionName = "UnlinkAccount";

            /// <summary>
            /// The unlink account confirm action name.
            /// </summary>
            public const string UnlinkAccountConfirmActionName = "UnlinkAccountConfirm";

            private const string MyAccountViewName = "MyAccount";
            private const string OrderHistoryViewName = "OrderHistory";
            private const string UnlinkAccountViewName = "UnlinkAccount";

            /// <summary>
            /// The default action for the MyAccount Controller.
            /// </summary>
            /// <returns>Default MyAccount view.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributeDefaultMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            public ActionResult Index()
            {
                if (!this.HttpContext.Request.IsAuthenticated)
                {
                    var ctx = this.HttpContext.GetOwinContext();
                    ctx.Authentication.Challenge(CookieConstants.ApplicationCookieAuthenticationType);
                    return new HttpUnauthorizedResult("User must sign in first");
                }

                return this.View(MyAccountController.MyAccountViewName);
            }

            /// <summary>
            /// Action for fetching order history.
            /// </summary>
            /// <returns>View showing the order history.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributeDefaultMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            public ActionResult OrderHistory()
            {
                if (!this.HttpContext.Request.IsAuthenticated)
                {
                    return new HttpUnauthorizedResult("User must sign in first");
                }

                return this.View(MyAccountController.OrderHistoryViewName);
            }

            /// <summary>
            /// Unlinks the account.
            /// </summary>
            /// <returns>The unlink account view.</returns>
            [HttpGet]
            public ActionResult UnlinkAccount()
            {
                if (!this.HttpContext.Request.IsAuthenticated)
                {
                    return new HttpUnauthorizedResult("User must sign in first");
                }

                IOwinContext ctx = this.HttpContext.Request.GetOwinContext();
                ClaimsPrincipal user = ctx.Authentication.User;
                IEnumerable<Claim> claims = user.Claims;
                SignedInUserViewModel signedInUserViewModel = new SignedInUserViewModel(claims);

                return this.View(MyAccountController.UnlinkAccountViewName, null, signedInUserViewModel.AccountNumber);
            }

            /// <summary>
            /// Unlinks the account confirm.
            /// </summary>
            /// <returns>A redirect action to the sign out action.</returns>
            public async Task<ActionResult> UnlinkAccountConfirm()
            {
                if (!this.HttpContext.Request.IsAuthenticated)
                {
                    return new HttpUnauthorizedResult("User must sign in first");
                }

                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                await customerOperationsHandler.UnlinkExternalIdFromExistingCustomer();

                return this.RedirectToAction(SignInController.SignOutActionName, SignInController.ControllerName);
            }
        }
    }
}