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
    namespace Retail.Ecommerce.Sdk.Core
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Linq;
        using System.Threading.Tasks;
        using Commerce.RetailProxy;

        /// <summary>
        /// Encapsulates functionality to augment entities based on requested data level.
        /// </summary>
        public static class DataAugmenter
        {
            /// <summary>
            /// Adds the data to the cart based on data level.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            /// <param name="cart">The cart.</param>
            /// <returns> Augmented cart.</returns>
            public static async Task<Cart> GetAugmentedCart(EcommerceContext ecommerceContext, Cart cart)
            {
                if (ecommerceContext == null)
                {
                    throw new ArgumentNullException(nameof(ecommerceContext));
                }

                if (cart == null)
                {
                    return null;
                }

                string currencyStringTemplate = await Utilities.GetChannelCurrencyStringTemplate(ecommerceContext);
                cart.ExtensionProperties.SetPropertyValue("CurrencyStringTemplate", ExtensionPropertyTypes.String, currencyStringTemplate);

                return cart;
            }

            /// <summary>
            /// Adds the data to the entity.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            /// <param name="simpleProducts">The simple products.</param>
            /// <param name="catalogs">List of catalogs associated with current channel.</param>
            /// <returns>
            /// Augmented entity.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public static async Task<Collection<SimpleProduct>> GetAugmentedSimpleProducts(EcommerceContext ecommerceContext, Collection<SimpleProduct> simpleProducts, IEnumerable<ProductCatalog> catalogs)
            {
                if (ecommerceContext == null)
                {
                    throw new ArgumentNullException(nameof(ecommerceContext));
                }

                HashSet<long> distinctProductIdsIncludingKitComponents = new HashSet<long>();
                foreach (SimpleProduct simpleProduct in simpleProducts)
                {
                    distinctProductIdsIncludingKitComponents.Add((long)simpleProduct.RecordId);

                    if ((ProductType)simpleProduct.ProductTypeValue == ProductType.KitVariant)
                    {
                        foreach (ProductComponent productComponent in simpleProduct.Components)
                        {
                            distinctProductIdsIncludingKitComponents.Add((long)productComponent.ProductId);
                        }
                    }
                }

                IDictionary<long, MediaLocation> mediaLocationDictionary = await GetMediaLocationDictionary(ecommerceContext, distinctProductIdsIncludingKitComponents, catalogs);

                MediaLocation mediaLocation = null;
                string primaryImageUri = string.Empty;
                string primaryImageAltText = string.Empty;

                foreach (SimpleProduct simpleProduct in simpleProducts)
                {
                    if (mediaLocationDictionary.TryGetValue((long)simpleProduct.RecordId, out mediaLocation))
                    {
                        primaryImageUri = mediaLocation.Uri;
                        primaryImageAltText = mediaLocation.AltText;
                    }

                    simpleProduct.ExtensionProperties.SetPropertyValue("PrimaryImageUri", ExtensionPropertyTypes.String, primaryImageUri);
                    simpleProduct.ExtensionProperties.SetPropertyValue("PrimaryImageAltText", ExtensionPropertyTypes.String, primaryImageAltText);

                    if ((ProductType)simpleProduct.ProductTypeValue == ProductType.KitVariant)
                    {
                        foreach (ProductComponent productComponent in simpleProduct.Components)
                        {
                            if (mediaLocationDictionary.TryGetValue((long)productComponent.ProductId, out mediaLocation))
                            {
                                primaryImageUri = mediaLocation.Uri;
                                primaryImageAltText = mediaLocation.AltText;
                            }

                            productComponent.ExtensionProperties.SetPropertyValue("PrimaryImageUri", ExtensionPropertyTypes.String, primaryImageUri);
                            productComponent.ExtensionProperties.SetPropertyValue("PrimaryImageAltText", ExtensionPropertyTypes.String, primaryImageAltText);
                        }
                    }
                }

                return simpleProducts;
            }

            /// <summary>
            /// Adds the data to the entity based.
            /// </summary>
            /// <param name="ecommerceContext">The ecommerce context.</param>
            /// <param name="salesOrders">The sales orders.</param>
            /// <returns>
            /// Augmented entity.
            /// </returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for async calls.")]
            public static async Task<PagedResult<SalesOrder>> GetAugmentedSalesOrders(EcommerceContext ecommerceContext, PagedResult<SalesOrder> salesOrders)
            {
                if (ecommerceContext == null)
                {
                    throw new ArgumentNullException(nameof(ecommerceContext));
                }

                if (salesOrders.Any())
                {
                    string currencyStringTemplate = await Utilities.GetChannelCurrencyStringTemplate(ecommerceContext);
                    foreach (SalesOrder salesOrder in salesOrders.Results)
                    {
                        salesOrder.ExtensionProperties.SetPropertyValue("CurrencyStringTemplate", ExtensionPropertyTypes.String, currencyStringTemplate);
                    }
                }

                return salesOrders;
            }

            private static async Task<IDictionary<long, MediaLocation>> GetMediaLocationDictionary(EcommerceContext ecommerceContext, IEnumerable<long> productIds, IEnumerable<ProductCatalog> catalogs)
            {
                long channelId = await Utilities.GetChannelId(ecommerceContext);

                var distinctProductIds = productIds.Distinct();
                ManagerFactory managerFactory = Utilities.GetManagerFactory(ecommerceContext);
                IProductManager productManager = managerFactory.GetManager<IProductManager>();

                IDictionary<long, MediaLocation> mediaLocationDictionary = new Dictionary<long, MediaLocation>();
                QueryResultSettings queryResultSettings = new QueryResultSettings()
                {
                    Paging = new PagingInfo() { Skip = 0, Top = 1 }
                };

                ProductCatalog firstCatalog = catalogs.FirstOrDefault();
                long catalogId = (firstCatalog == null) ? 0 : firstCatalog.RecordId;

                foreach (var productId in distinctProductIds)
                {
                    PagedResult<MediaLocation> mediaLocations = await productManager.GetMediaLocations(productId, channelId, catalogId, queryResultSettings: queryResultSettings);
                    if (mediaLocations.Any())
                    {
                        mediaLocationDictionary.Add(productId, mediaLocations.First());
                    }
                }

                return mediaLocationDictionary;
            }
        }
    }
}