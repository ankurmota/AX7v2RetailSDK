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
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Threading.Tasks;
        using System.Web.Mvc;
        using Commerce.RetailProxy;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;

        /// <summary>
        /// Home Controller.
        /// </summary>
        public class HomeController : ActionControllerBase
        {
            /// <summary>
            /// The controller name.
            /// </summary>
            public const string ControllerName = "Home";

            /// <summary>
            /// The default action name.
            /// </summary>
            public const string DefaultActionName = "Index";

            private const string HomeViewName = "Home";

            /// <summary>
            /// Return View.
            /// </summary>
            /// <returns>The View.</returns>
            public async Task<ActionResult> Index()
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);

                PagedResult<Category> categories = await orgUnitOperationsHandler.GetNavigationalHierarchyCategories(Utilities.DefaultQuerySettings);
                IEnumerable<long> rawCategoryIds = categories.Select(c => c.RecordId);

                Collection<Product> productList = null;

                // add productId to an ObservableCollection
                ObservableCollection<long> productIds = new ObservableCollection<long>() { 22565423115, 22565423455, 22565423885, 22565423933 };

                ProductSearchCriteria searchCriteria = new ProductSearchCriteria
                {
                    DataLevelValue = 4,
                    Ids = productIds
                };

                // try and get product information
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);

                // This will fetch even uncatalogued products that match the criteria.
                PagedResult<Product> products = await productOperationsHandler.SearchProducts(searchCriteria, new long[] { 0 }, Utilities.DefaultQuerySettings);
                if (products.Results.Any())
                {
                    productList = new Collection<Product>(products.Results.ToList());
                }

                return this.View(HomeController.HomeViewName, productList);
            }
        }
    }
}