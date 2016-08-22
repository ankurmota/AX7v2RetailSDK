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
        using Retail.Ecommerce.Web.Storefront.Models;

        /// <summary>
        /// Product Gallery Controller.
        /// </summary>
        public class ProductGalleryController : ActionControllerBase
        {
            private const string ProductGalleryViewName = "ProductGallery";

            /// <summary>
            /// Return View with optional search criteria added.
            /// </summary>
            /// <param name="categoryId">Required: Category id to show products for.</param>
            /// <param name="filterBrands">List of brands to show (comma separated).</param>
            /// <param name="filterCategories">List of categories to show (comma separated).</param>
            /// <returns>View of Products.</returns>
            public async Task<ActionResult> Index(string categoryId = "", string[] filterBrands = null, string[] filterCategories = null)
            {
                EcommerceContext ecommerceContext = ServiceUtilities.GetEcommerceContext(this.HttpContext);
                OrgUnitOperationsHandler orgUnitOperationsHandler = new OrgUnitOperationsHandler(ecommerceContext);

                PagedResult<Category> categories = await orgUnitOperationsHandler.GetNavigationalHierarchyCategories(Utilities.DefaultQuerySettings);
                IEnumerable<long> rawCategoryIds = categories.Select(c => c.RecordId);

                // determine what category to load products for, if null, load all products
                ObservableCollection<long> categoryIds;
                if (string.IsNullOrEmpty(categoryId))
                {
                    categoryIds = new ObservableCollection<long>(rawCategoryIds);
                }
                else
                {
                    categoryIds = new ObservableCollection<long>();
                    categoryIds.Add(long.Parse(categoryId));
                }

                // Category Id to Name Mapping
                Dictionary<long, string> mapping = new Dictionary<long, string>();
                foreach (Category category in categories)
                {
                    mapping.Add(category.RecordId, category.Name);
                }

                // Retrieving Products - make sure we include products from descendant categories too 
                ProductSearchCriteria searchCriteria = new ProductSearchCriteria
                {
                    DataLevelValue = 4,
                    CategoryIds = categoryIds,
                    IncludeProductsFromDescendantCategories = true
                };

                // try and get product information
                ProductOperationsHandler productOperationsHandler = new ProductOperationsHandler(ecommerceContext);

                PagedResult<ProductCatalog> productCatalogs = await productOperationsHandler.GetProductCatalogs(Utilities.DefaultQuerySettings);
                IEnumerable<long> activeCatalogIds = productCatalogs.Results.Select(pc => pc.RecordId);

                PagedResult<Product> products = await productOperationsHandler.SearchProducts(searchCriteria, activeCatalogIds, Utilities.DefaultQuerySettings);

                // Breadcrumb Navigation Links
                Collection<CustomLink> breadcrumbNavLinks = new Collection<CustomLink>();
                Category currentCategory = this.GetCategoryById(long.Parse(categoryId), categories);

                while (!currentCategory.ParentCategory.Equals((long?)0))
                {
                    breadcrumbNavLinks.Add(new CustomLink("/ProductGallery?categoryId=" + currentCategory.RecordId, currentCategory.Name));
                    currentCategory = this.GetCategoryById(currentCategory.ParentCategory, categories);
                }

                breadcrumbNavLinks.Add(new CustomLink("/", "Home"));

                // Filter Mapping
                Dictionary<string, string[]> filters = new Dictionary<string, string[]>();
                filters.Add("brand", filterBrands);
                filters.Add("categories", filterCategories);

                IEnumerable<Product> productList = await ProductDetailsController.PopulateViewSpecificProductInfo(products.Results, ecommerceContext);

                // create a new product gallery model for the view
                ProductGalleryModel productGalleryModel = new ProductGalleryModel(long.Parse(categoryId), productList, breadcrumbNavLinks, mapping, filters);

                return this.View(ProductGalleryController.ProductGalleryViewName, productGalleryModel);
            }

            /// <summary>
            /// Get the category from the product identifier.
            /// </summary>
            /// <param name="id">Id of particular property.</param>
            /// <param name="categories">List of all categories.</param>
            /// <returns>Category belonging to that product.</returns>
            private Category GetCategoryById(long? id, PagedResult<Category> categories)
            {
                return categories.First(c => c.RecordId == id);
            }
        }
    }
}