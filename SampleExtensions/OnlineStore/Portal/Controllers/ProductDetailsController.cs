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
        using Microsoft.Dynamics.Retail.Diagnostics;
        using Retail.Ecommerce.Sdk.Core;
        using Retail.Ecommerce.Sdk.Core.OperationsHandlers;
        using Retail.Ecommerce.Web.Storefront.Models;

        /// <summary>
        /// Product Details Controller.
        /// </summary>
        public class ProductDetailsController : ActionControllerBase
        {
            private const string ProductDetailsViewName = "ProductDetails";

            /// <summary>
            /// Populates the view specific product information.
            /// </summary>
            /// <param name="products">The products.</param>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            /// <returns>Update collection of products.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public static async Task<IEnumerable<Product>> PopulateViewSpecificProductInfo(IEnumerable<Product> products, EcommerceContext ecommerceContext)
            {
                if (products == null)
                {
                    return products;
                }

                foreach (Product product in products)
                {
                    string extensionPropertyValue = await Utilities.ToCurrencyString((decimal)product.AdjustedPrice, ecommerceContext);
                    product.ExtensionProperties.SetPropertyValue("FormattedAdjustedPrice", ExtensionPropertyTypes.String, extensionPropertyValue);

                    extensionPropertyValue = await Utilities.ToCurrencyString((decimal)product.BasePrice, ecommerceContext);
                    product.ExtensionProperties.SetPropertyValue("FormattedBasePrice", ExtensionPropertyTypes.String, extensionPropertyValue);

                    decimal savingsPercent = 0M;
                    if (product.BasePrice > 0)
                    {
                        savingsPercent = 100 - ((decimal)product.AdjustedPrice / (decimal)product.BasePrice * 100);
                    }

                    extensionPropertyValue = await Utilities.ToCurrencyString(savingsPercent, ecommerceContext);
                    product.ExtensionProperties.SetPropertyValue("SavingsPercent", ExtensionPropertyTypes.String, extensionPropertyValue);
                }

                return products;
            }

            /// <summary>
            /// Get the Details for a particular product.
            /// </summary>
            /// <param name="productId">The product id you need the details for.</param>
            /// <returns>The View for that product.</returns>
            public async Task<ActionResult> Index(string productId = "")
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);

                PagedResult<Category> categories = await orgUnitOperationsHandler.GetNavigationalHierarchyCategories(Utilities.DefaultQuerySettings);
                IEnumerable<long> rawCategoryIds = categories.Select(c => c.RecordId);

                ObservableCollection<long> productIds = null;
                Product prod = null;
                Collection<CustomLink> breadcrumbNavLinks = new Collection<CustomLink>();
                long productIdentifier;
                if (string.IsNullOrEmpty(productId) || !long.TryParse(productId, out productIdentifier))
                {
                    RetailLogger.Log.OnlineStoreInvalidProductIdProvided(productId);
                    return this.RedirectToAction(HomeController.DefaultActionName, HomeController.ControllerName);
                }
                else
                {
                    // add productId to an ObservableCollection
                    productIds = new ObservableCollection<long>();
                    productIds.Add(productIdentifier);

                    ProductSearchCriteria searchCriteria = new ProductSearchCriteria
                    {
                        DataLevelValue = 4,
                        Ids = productIds
                    };

                    // try and get product information
                    ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);
                    PagedResult<ProductCatalog> productCatalogs = await productOperationsHandler.GetProductCatalogs(Utilities.DefaultQuerySettings);
                    IEnumerable<long> activeCatalogIds = productCatalogs.Results.Select(pc => pc.RecordId);

                    PagedResult<Product> products = await productOperationsHandler.SearchProducts(searchCriteria, activeCatalogIds, Utilities.DefaultQuerySettings);
                    if (!products.Results.Any())
                    {
                        var message = string.Format("ProductIds: {0}.", string.Join(",", productIds));
                        RetailLogger.Log.OnlineStoreNoProductsFound(message);
                        return this.RedirectToAction(HomeController.DefaultActionName, HomeController.ControllerName);
                    }

                    prod = products.Results.First<Product>();

                    // Breadcrumb Navigation Links
                    // add current item
                    breadcrumbNavLinks.Add(new CustomLink("/ProductDetails?productId=" + prod.RecordId, prod.ProductName));

                    Category currentCategory = this.GetCategoryById(prod.CategoryIds.First(), categories);

                    while (currentCategory.ParentCategory != 0)
                    {
                        breadcrumbNavLinks.Add(new CustomLink("/ProductGallery?categoryId=" + currentCategory.RecordId, currentCategory.Name));
                        currentCategory = this.GetCategoryById(currentCategory.ParentCategory, categories);
                    }

                    breadcrumbNavLinks.Add(new CustomLink("/", "Home"));
                }

                prod = (await ProductDetailsController.PopulateViewSpecificProductInfo(new Product[] { prod }, ecommerceContext)).FirstOrDefault();

                return this.View(ProductDetailsController.ProductDetailsViewName, new ProductDetailsModel(prod, breadcrumbNavLinks));
            }

            /// <summary>
            /// Get Category by Id.
            /// </summary>
            /// <param name="id">The Id you need categories for.</param>
            /// <param name="categories">The List of all Categories.</param>
            /// <returns>A Specific Category.</returns>
            private Category GetCategoryById(long? id, PagedResult<Category> categories)
            {
                return categories.Single(c => c.RecordId == id);
            }
        }
    }
}