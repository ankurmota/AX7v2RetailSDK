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
    namespace Retail.Channels
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Runtime.Serialization;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    
        /// <summary>
        /// Represents a catalog listing.
        /// </summary>
        /// <remarks>
        /// 1. The properties that are NOT relevant to the clients are not exposed (e.g. opt-in only).
        /// 2. The mandatory properties are defined as basic properties.
        /// 3. The optional properties are kept in the property bag.
        /// 4. Localization is handled through the locale property (e.g. we only return properties of the locale that clients care about).
        /// </remarks>
        [DataContract]
        public class Listing
        {
            #region internal and private constants
    
            private const string DefaultLocale = "en-us";
            private const string InsertString = "I";
            private const string UpdateString = "U";
            private const string DeleteString = "D";
            private const string DisplayProductNumber = "DISPLAYPRODUCTNUMBER";
    
            #endregion
    
            /// <summary>
            /// Initializes a new instance of the <see cref="Listing"/> class.
            /// </summary>
            public Listing()
                : this(DefaultLocale)
            {
                this.CategoryIds = new List<long>();
                this.RelatedProducts = new List<RelatedProduct>();
                this.ProductsRelatedToThis = new List<RelatedProduct>();
                this.Properties = new ProductPropertyDictionary();
            }
    
            /// <summary>
            /// Initializes a new instance of the <see cref="Listing" /> class.
            /// </summary>
            /// <param name="locale">The locale.</param>
            public Listing(string locale)
            {
                this.Locale = locale;
            }
    
            /// <summary>
            /// The default attributes from RetailChannelProductAttributeID base enumeration in AX.
            /// </summary>
            private enum RetailChannelProductAttributeId
            {
                ProductName,
                Description,
                ItemNumber,
                Features,
                Specification,
                Image,
                Color,
                Size,
                Style,
                Configuration,
                ReviewRating,
                New,
                CustomerFavorites,
                StaffFavorites,
                Brand
            }
    
            /// <summary>
            /// Gets a value indicating the nature of change in the database.
            /// </summary>
            [IgnoreDataMember]
            public ChangeAction ChangeAction
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the value of the ChangeAction enumeration. Used by OData only.
            /// </summary>
            [DataMember]
            public int ChangeActionValue
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the record identifier.
            /// </summary>
            [DataMember]
            public long RecordId
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the action requested by catalog publisher.
            /// </summary>
            [IgnoreDataMember]
            public PublishingAction Action
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets or sets the value of the PublishingAction enumeration. Used by OData only.
            /// </summary>
            [DataMember]
            public int ActionValue
            {
                get { return (int)this.Action; }
                set { this.Action = (PublishingAction)value; }
            }
    
            /// <summary>
            /// Gets the inventory dimension.
            /// </summary>
            [DataMember]
            public string InventoryDimension
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the item identifier.
            /// </summary>
            [DataMember]
            public string ItemId
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the unique product number.
            /// </summary>
            [DataMember]
            public string ProductNumber
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the unit of measure.
            /// </summary>
            [DataMember]
            public string SalesUnitOfMeasure
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the identifier of the parent listing for variants.
            /// </summary>
            [DataMember]
            public long ParentProductId
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the start date of listing.
            /// </summary>
            [DataMember]
            public DateTimeOffset ValidFrom
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the end date of the listing.
            /// </summary>
            [DataMember]
            public DateTimeOffset ValidTo
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the date time the listing was last modified.
            /// </summary>
            [DataMember]
            public DateTimeOffset ModifiedDateTime
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the row version (sequence identifier) of this listing record.
            /// </summary>
            /// <remarks>
            /// Underlying field RowVersion is not null, of type timestamp, equivalent to a binary(8) and <c>bigint</c>.
            /// </remarks>
            [DataMember]
            public long Sequence
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets or sets the primary category specified for the listing.
            /// </summary>
            [DataMember]
            public long PrimaryCategoryId { get; set; }
    
            /// <summary>
            /// Gets the set of category identifiers mapped to this listing.
            /// </summary>
            [DataMember]
            public ICollection<long> CategoryIds { get; internal set; }
    
            /// <summary>
            /// Gets information about the listings related to the this listing.
            /// </summary>
            [DataMember]
            public ICollection<RelatedProduct> RelatedProducts { get; internal set; }
    
            /// <summary>
            /// Gets information about the listings inversely related to this listing.
            /// </summary>
            [DataMember]
            public ICollection<RelatedProduct> ProductsRelatedToThis { get; internal set; }
    
            /// <summary>
            /// Gets a value indicating whether the current listing is a kit.
            /// </summary>
            [DataMember]
            public bool IsKitVariant { get; internal set; }
    
            /// <summary>
            /// Gets information about the kit variant contents if the current listing is a kit variant.
            /// </summary>
            [DataMember]
            public ICollection<KitComponentKey> KitVariantContents { get; internal set; }
    
            /// <summary>
            /// Gets information about the kits that contain this listing.
            /// </summary>
            [DataMember]
            public ICollection<KitComponent> ParentKits { get; internal set; }
    
            /// <summary>
            /// Gets the properties associated with this listing.
            /// </summary>
            [IgnoreDataMember]
            public ProductPropertyDictionary Properties { get; internal set; }
    
            /// <summary>
            /// Gets the base sales price for the listing.
            /// </summary>
            [DataMember]
            public decimal BasePrice
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the price after trade agreements have been applied.
            /// </summary>
            [DataMember]
            public decimal Price
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the price after trade agreements and price adjustments have been applied.
            /// </summary>
            [DataMember]
            public decimal AdjustedPrice
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the string representing the identity of this listing.
            /// </summary>
            [IgnoreDataMember]
            public string Identity
            {
                get { return string.Format("({0}, {1})", this.RecordId, this.Sequence); }
            }
    
            /// <summary>
            /// Gets the locale.
            /// </summary>
            /// <value>
            /// The locale.
            /// </value>
            [IgnoreDataMember]
            public string Locale
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the name of the product.
            /// </summary>
            /// <value>
            /// The name of the product.
            /// </value>
            [IgnoreDataMember]
            public string ProductName
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            [IgnoreDataMember]
            public string Description
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets the image.
            /// </summary>
            /// <value>
            /// The image.
            /// </value>
            [IgnoreDataMember]
            public RichMediaLocations Image
            {
                get;
                internal set;
            }
    
            /// <summary>
            /// Gets or sets the item channel identifier.
            /// </summary>
            [DataMember]
            public long ChannelId
            {
                get;
                set;
            }
    
            /// <summary>
            /// Gets or sets the item Catalog identifier.
            /// </summary>
            [DataMember]
            public long CatalogId
            {
                get;
                set;
            }
    
            #region public methods
    
            /// <summary>
            /// Creates a new <see cref="ListingPublishStatus"/> object to represent successfully deleted listing.
            /// </summary>
            /// <param name="channelId">Accepts channel id.</param>
            /// <param name="catalogId">Accepts catalog id.</param>
            /// <param name="productId">Accepts product id.</param>
            /// <param name="languageId">Accepts language id.</param>
            /// <returns>Status representing an in-progress publishing.</returns>
            public static ListingPublishStatus CreateStatusSuccessfullyDeleted(long channelId, long catalogId, long productId, string languageId)
            {
                return new ListingPublishStatus()
                {
                    ListingModifiedDateTime = DateTimeOffset.UtcNow,
                    PublishStatus = ListingPublishingActionStatus.Done,
                    ChannelListingId = productId.ToString(),
                    AppliedAction = PublishingAction.Delete,
                    ProductId = productId,
                    ChannelId = channelId,
                    CatalogId = catalogId,
                    LanguageId = languageId
                };
            }
    
            /// <summary>
            /// Creates a new <see cref="ExpandProductsToListings"/> object to expand products to Listings.
            /// </summary>
            /// <param name="products">Products to be expanded.</param>
            /// <returns>Returns collection of Listing.</returns>
            public static ReadOnlyCollection<Listing> ExpandProductsToListings(IEnumerable<Product> products)
            {
                if (products == null)
                {
                    throw new ArgumentNullException(nameof(products));
                }
    
                List<Listing> result = new List<Listing>();
    
                foreach (var product in products)
                {
                    result.AddRange(GetListings(product));
                }
    
                return result.AsReadOnly();
            }
    
            /// <summary>
            /// Enumerates the listings corresponding to this product.
            /// </summary>
            /// <param name="product">Accepts product.</param>
            /// <returns>
            /// A set of listings corresponding to this product.
            /// </returns>
            /// <remarks>
            /// A listing is a representation of a product for a given language and variant, meant
            /// to be consumed by an external system (such as SharePoint, 3rd party catalogs etc).
            /// </remarks>
            public static ReadOnlyCollection<Listing> GetListings(Product product)
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product));
                }
    
                List<long> ids = new List<long>();
                List<Listing> listings = new List<Listing>();
    
                if (product.IsMasterProduct)
                {
                    ids.AddRange(product.CompositionInformation.VariantInformation.IndexedVariants.Keys);
                }
                else
                {
                    ids.Add(product.RecordId);
                }
    
                // variants may not have translations for every language present in the master's properties.
                ISet<string> languages = new HashSet<string>(product.IndexedProductProperties.Keys, StringComparer.OrdinalIgnoreCase);
    
                // add any languages specific to variants only
                if (product.IsMasterProduct)
                {
                    foreach (long id in ids)
                    {
                        var translatedPropertySet = product.CompositionInformation.VariantInformation.IndexedVariants[id].IndexedProperties;
    
                        foreach (var languageId in translatedPropertySet.Keys)
                        {
                            if (!languages.Contains(languageId))
                            {
                                languages.Add(languageId);
                            }
                        }
                    }
                }
    
                // iterate again through all ids, creating a listing for each (id, language, catalog validity) pair
                foreach (long id in ids)
                {
                    // List of all catalogs for the current product.
                    List<ProductCatalog> catalogs;
    
                    // If "Take Snapsshot" catalog setting is set to True and
                    // master is included into the catalog but variant is not then ProductCatalogMap will have
                    // only those records which corresponds to included variants only
                    if (product.ProductCatalogMap.TryGetValue(id, out catalogs))
                    {
                        foreach (ProductCatalog catalog in catalogs)
                        {
                            foreach (string languageId in languages)
                            {
                                // The listing for the given variant should only be created if the variant contains translation for at least 1 of its attributes.
                                if (product.TranslationsExist(id, languageId))
                                {
                                    Listing listing = CreateProductListing(product, id, catalog.RecordId, catalog.ValidFrom, catalog.ValidTo);
    
                                    listing.Properties = product.GetIndexedProperties(listing.RecordId, languageId);
                                    listing.Locale = languageId;
                                    listings.Add(listing);
                                }
                            }
                        }
                    }
                }
    
                return listings.AsReadOnly();
            }
    
            /// <summary>
            /// Creates a new <see cref="ListingPublishStatus"/> object to represent a successful publishing of a listing.
            /// </summary>
            /// <returns>Status representing successful publishing.</returns>
            public ListingPublishStatus CreateStatusSuccessfullyPublished()
            {
                return new ListingPublishStatus
                {
                    ChannelListingId = this.RecordId.ToString(),
                    ListingModifiedDateTime = DateTimeOffset.UtcNow,
                    PublishStatus = ListingPublishingActionStatus.Done,
                    AppliedAction = PublishingAction.Publish,
                    ProductId = this.RecordId,
                    ChannelId = this.ChannelId,
                    CatalogId = this.CatalogId,
                    LanguageId = this.Locale
                };
            }
    
            /// <summary>
            /// Creates a new <see cref="ListingPublishStatus"/> object to represent a failed publishing of a listing.
            /// </summary>
            /// <param name="statusMessage">Publish failure message.</param>
            /// <returns>Status representing a failed publishing of a listing.</returns>
            public ListingPublishStatus CreateStatusFailedToPublish(string statusMessage)
            {
                return new ListingPublishStatus()
                {
                    ListingModifiedDateTime = DateTimeOffset.UtcNow,
                    PublishStatus = ListingPublishingActionStatus.Failed,
                    ChannelListingId = string.Empty,
                    AppliedAction = PublishingAction.Publish,
                    StatusMessage = statusMessage,
                    ProductId = this.RecordId,
                    ChannelId = this.ChannelId,
                    CatalogId = this.CatalogId,
                    LanguageId = this.Locale
                };
            }
    
            /// <summary>
            /// Creates a new <see cref="ListingPublishStatus"/> object to represent in-progress publishing of a listing.
            /// </summary>
            /// <returns>Status representing an in-progress publishing.</returns>
            public ListingPublishStatus CreateStatusPublishInProgress()
            {
                return new ListingPublishStatus()
                {
                    ListingModifiedDateTime = DateTimeOffset.UtcNow,
                    PublishStatus = ListingPublishingActionStatus.InProgress,
                    ChannelListingId = string.Empty,
                    AppliedAction = PublishingAction.Publish,
                    ProductId = this.RecordId,
                    ChannelId = this.ChannelId,
                    CatalogId = this.CatalogId,
                    LanguageId = this.Locale
                };
            }
    
            /// <summary>
            /// Given a product, create a corresponding Listing shell object.
            /// </summary>
            /// <param name="product">Product whose listings are to be created.</param>
            /// <param name="productId">Specifies the product id for which to create the listing, may be that of the product or one of its variants.</param>
            /// <param name="catalogId">The catalog identifier.</param>
            /// <param name="validFrom">The start date for the validity interval.</param>
            /// <param name="validTo">The end date for the validity interval.</param>
            /// <returns>A listing corresponding to the specified product.</returns>
            private static Listing CreateProductListing(Product product, long productId, long catalogId, DateTimeOffset validFrom, DateTimeOffset validTo)
            {
                Listing result = new Listing
                {
                    RecordId = productId,
                    Action = 0,
                    ActionValue = 0,
                    InventoryDimension = string.Empty,
                    ItemId = product.ItemId,
                    ProductNumber = product.ProductNumber,
                    SalesUnitOfMeasure = string.Empty,
                    ParentProductId = 0,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    ChangeAction = product.ChangeTrackingInformation.ChangeAction,
                    ChangeActionValue = product.ChangeTrackingInformation.ChangeActionValue,
                    ModifiedDateTime = product.ChangeTrackingInformation.ModifiedDateTime,
                    Sequence = 0,
                    PrimaryCategoryId = product.PrimaryCategoryId,
                    CategoryIds = product.CategoryIds,
                    RelatedProducts = product.RelatedProducts,
                    ProductsRelatedToThis = product.ProductsRelatedToThis,
                    ParentKits = product.ParentKits,
                    Properties = product.DefaultProductProperties,
                    BasePrice = product.BasePrice,
                    Price = product.Price,
                    AdjustedPrice = product.AdjustedPrice,
                    Locale = product.Locale,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Image = product.Image,
                    ChannelId = product.Context.ChannelId ?? 0L,
                    CatalogId = catalogId,
                    IsKitVariant = product.IsKit
                };
    
                if (product.IsMasterProduct
                    && productId != product.RecordId)
                {
                    result.InventoryDimension = product.CompositionInformation.VariantInformation.IndexedVariants[productId].InventoryDimensionId;
                    result.ParentProductId = product.RecordId;
    
                    if (product.IsKit)
                    {
                        result.KitVariantContents = product.CompositionInformation.KitDefinition.IndexedKitVariantToComponentMap[productId].KitComponentKeyList;
                    }
                }
    
                // Update parent kits information for every non-master product.
                result.ParentKits = new Collection<KitComponent>();
    
                foreach (var parentKit in product.ParentKits)
                {
                    if (parentKit.KitLineProductId == productId)
                    {
                        result.ParentKits.Add(parentKit);
                    }
                }
    
                return result;
            }
    
            #endregion
        }
    }
}
