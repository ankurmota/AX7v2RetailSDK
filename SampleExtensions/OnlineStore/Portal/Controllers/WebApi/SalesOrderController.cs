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
        /// Sales Order Controller.
        /// </summary>
        public class SalesOrderController : WebApiControllerBase
        {
            /// <summary>
            /// Gets the sales order.
            /// </summary>
            /// <param name="salesOrderSearchCriteria">The sales order search criteria.</param>
            /// <param name="queryResultSettings">The query result settings.</param>
            /// <returns>A response containing the sales orders inquired for.</returns>
            public async Task<ActionResult> GetSalesOrder(SalesOrderSearchCriteria salesOrderSearchCriteria, QueryResultSettings queryResultSettings)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                SalesOrderOperationsHandler salesOrderOperationHandler = new SalesOrderOperationsHandler(ecommerceContext);
                PagedResult<SalesOrder> salesOrders = await salesOrderOperationHandler.GetSalesOrder(salesOrderSearchCriteria, queryResultSettings);

                return this.Json(salesOrders.Results);
            }
        }
    }
}