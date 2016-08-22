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
        using System.Collections.Generic;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Retail.Ecommerce.Sdk.Core;

        /// <summary>
        /// Base class for controllers that represent Web APIs.
        /// </summary>
        public abstract class WebApiControllerBase : Controller
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
                    || currentException as AuthenticationException != null)
                {
                    ServiceUtilities.CleanUpOnSignOutOrAuthFailure(this.HttpContext);

                    RetailProxyException retailProxyException = currentException as RetailProxyException;

                    ResponseError responseError = new ResponseError()
                    {
                        ErrorCode = retailProxyException.ErrorResourceId,
                        LocalizedErrorMessage = retailProxyException.LocalizedMessage,
                    };

                    filterContext.Result = this.Json(responseError);
                    filterContext.HttpContext.Response.StatusCode = 310;
                    filterContext.HttpContext.Response.RedirectLocation = "/SignIn";
                    filterContext.ExceptionHandled = true;
                    RetailLogger.Log.OnlineStoreForceSignOutOnAuthenticatedFlowError(
                        filterContext.HttpContext.Response.StatusCode,
                        filterContext.HttpContext.Response.RedirectLocation,
                        filterContext.Exception,
                        filterContext.Exception.InnerException);
                }
                else
                {
                    if (currentException as CartValidationException != null)
                    {
                        CartValidationException cartValidationException = (CartValidationException)currentException;
                        if (string.Equals(cartValidationException.ErrorResourceId, DataValidationErrors.Microsoft_Dynamics_Commerce_Runtime_CartNotFound.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            ServiceUtilities.ClearCartCookies(this.HttpContext);
                        }
                    }

                    IEnumerable<ResponseError> responseErrors = Utilities.GetResponseErrorsFromException(currentException);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = this.Json(responseErrors);
                    filterContext.HttpContext.Response.StatusCode = 400;
                    RetailLogger.Log.OnlineStoreLogUnexpectedException(
                        filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri,
                        filterContext.Exception,
                        filterContext.Exception.InnerException);
                }
            }
        }
    }
}