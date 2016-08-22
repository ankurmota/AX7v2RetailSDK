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
        using System;
        using System.Web;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// Base class for controllers that are not web APIs.
        /// </summary>
        public abstract class ActionControllerBase : Controller
        {
            /// <summary>
            /// Called when an exception is thrown.
            /// </summary>
            /// <param name="filterContext">The filter context.</param>
            protected override void OnException(ExceptionContext filterContext)
            {
                if (filterContext == null)
                {
                    throw new ArgumentNullException("filterContext");
                }

                base.OnException(filterContext);

                Exception currentException = null;
                if (filterContext.Exception as AggregateException != null)
                {
                    currentException = filterContext.Exception.InnerException;
                }
                else
                {
                    currentException = filterContext.Exception;
                }

                if (currentException as UserAuthorizationException != null
                    || currentException as UserAuthenticationException != null
                    || currentException as AuthenticationException != null
                    || currentException as SecurityException != null)
                {
                    ServiceUtilities.CleanUpOnSignOutOrAuthFailure(this.HttpContext);
                    var ctx = this.HttpContext.GetOwinContext();
                    ctx.Authentication.Challenge(CookieConstants.ApplicationCookieAuthenticationType);

                    filterContext.Result = new HttpUnauthorizedResult("User must sign in");
                    filterContext.ExceptionHandled = true;

                    RetailLogger.Log.OnlineStoreForceSignOutOnAuthenticatedFlowError(
                        filterContext.HttpContext.Response.StatusCode,
                        filterContext.HttpContext.Response.RedirectLocation,
                        filterContext.Exception,
                        filterContext.Exception.InnerException);
                }
                else
                {
                    RetailLogger.Log.OnlineStoreLogUnexpectedException(
                        filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri,
                        filterContext.Exception,
                        filterContext.Exception.InnerException);
                }
            }
        }
    }
}