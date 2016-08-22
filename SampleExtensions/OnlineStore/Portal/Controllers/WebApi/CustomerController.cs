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
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Customer Controller.
        /// </summary>
        public class CustomerController : WebApiControllerBase
        {
            /// <summary>
            /// Determines whether [is authenticated session].
            /// </summary>
            /// <returns>A response indicating if the current session is authenticated.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Web.Configuration", "CA3147:MarkVerbHandlersWithValidateAntiforgeryToken", MessageId = "#ValidateAntiForgeryTokenAttributeDefaultMissing", Justification = "Support for anti-forgery token will be added once the controls are redesigned to follow MVC pattern.")]
            public ActionResult IsAuthenticatedSession()
            {
                bool isAuthenticated = this.HttpContext.Request.IsAuthenticated;
                return this.Json(isAuthenticated);
            }

            /// <summary>
            /// Gets the customer.
            /// </summary>
            /// <returns>A customer response.</returns>
            public async Task<ActionResult> GetCustomer()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                Customer customer = await customerOperationsHandler.GetCustomer();

                return this.Json(customer);
            }

            /// <summary>
            /// Updates the customer.
            /// </summary>
            /// <param name="customer">The customer.</param>
            /// <returns>
            /// The updated customer entity.
            /// </returns>
            public async Task<ActionResult> UpdateCustomer(Customer customer)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                Customer updatedCustomer = await customerOperationsHandler.UpdateCustomer(customer);

                return this.Json(updatedCustomer);
            }

            /// <summary>
            /// Gets the order history of a customer.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>A sakes order response.</returns>
            public async Task<ActionResult> GetOrderHistory(QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                PagedResult<SalesOrder> salesOrders = await customerOperationsHandler.GetOrderHistory(queryResultSettings);

                return this.Json(salesOrders);
            }

            /// <summary>
            /// Generates the loyalty card identifier.
            /// </summary>
            /// <returns>
            /// Response containing a newly generated loyalty card.
            /// </returns>
            public async Task<ActionResult> GenerateLoyaltyCardId()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                LoyaltyCard loyaltyCard = await customerOperationsHandler.GenerateLoyaltyCardId();

                return this.Json(loyaltyCard);
            }

            /// <summary>
            /// Gets the loyalty cards.
            /// </summary>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>Response containing the loyalty cards associated with the current user.</returns>
            public async Task<ActionResult> GetLoyaltyCards(QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                CustomerOperationsHandler customerOperationsHandler = new CustomerOperationsHandler(ecommerceContext);
                PagedResult<LoyaltyCard> loyaltyCards = await customerOperationsHandler.GetLoyaltyCards(queryResultSettings);

                return this.Json(loyaltyCards.Results);
            }
        }
    }
}