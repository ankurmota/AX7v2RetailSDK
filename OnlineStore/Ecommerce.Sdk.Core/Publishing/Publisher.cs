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
    namespace Retail.Ecommerce.Sdk.Core.Publishing
    {
        using System;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Configuration;
        using System.Diagnostics;
        using System.Globalization;
        using System.IO;
        using System.Linq;
        using Channels;
        using Microsoft.Dynamics.Commerce.Runtime;
        using Microsoft.Dynamics.Commerce.Runtime.Client;
        using Microsoft.Dynamics.Commerce.Runtime.Data;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Microsoft.Dynamics.Retail.Diagnostics;

        /// <summary>
        /// Encapsulates logic to handle Channel and Catalog publishing.
        /// </summary>
        public class Publisher
        {
            private const string KeySyncAnchor = "SyncAnchor";

            private readonly CommerceRuntime runtime;
            private readonly ChannelManager channelManager;
            private readonly ProductManager productManager;
            private readonly OnlineChannel onlineChannel;
            private readonly PublishingConfiguration publishingConfig;

            /// <summary>
            /// Initializes a new instance of the <see cref="Publisher" /> class.
            /// </summary>
            /// <param name="appConfig">Application configuration which contains CRT initialization information.</param>
            /// <param name="publishingConfig">Publishing configuration.</param>
            public Publisher(Configuration appConfig, PublishingConfiguration publishingConfig)
            {
                this.runtime = CrtUtilities.GetCommerceRuntime(appConfig);
                this.channelManager = ChannelManager.Create(this.runtime);
                this.productManager = ProductManager.Create(this.runtime);
                this.onlineChannel = this.channelManager.GetOnlineChannel(this.channelManager.GetCurrentChannelId());
                this.publishingConfig = publishingConfig;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Publisher"/> class.
            /// </summary>
            /// <param name="publishingConfig">Publishing configuration.</param>
            /// <param name="channelId">The channel identifier.</param>
            public Publisher(PublishingConfiguration publishingConfig, long channelId)
            {
                CommerceRuntimeConfiguration configuration = CrtUtilities.GetCrtConfiguration();
                this.runtime = CommerceRuntime.Create(configuration, new CommercePrincipal(new CommerceIdentity(channelId, new string[] { CommerceRoles.Storefront })));

                this.channelManager = ChannelManager.Create(this.runtime);
                this.productManager = ProductManager.Create(this.runtime);
                this.onlineChannel = this.channelManager.GetOnlineChannel(channelId);
                this.publishingConfig = publishingConfig;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Publisher"/> class.
            /// </summary>
            /// <param name="publishingConfig">Publishing configuration.</param>
            /// <param name="operatingUnitNumber">The channel's operating unit number.</param>
            public Publisher(PublishingConfiguration publishingConfig, string operatingUnitNumber)
            {
                CommerceRuntimeConfiguration configuration = CrtUtilities.GetCrtConfiguration();

                // First creating a runtime without a channel association, this is needed to resolve the operating unit number.
                this.runtime = CommerceRuntime.Create(configuration, new CommercePrincipal(new CommerceIdentity(0, new string[] { CommerceRoles.Storefront })));

                ChannelManager manager = ChannelManager.Create(this.runtime);
                long channelId = manager.ResolveOperatingUnitNumber(operatingUnitNumber);

                // Now creating a runtime with the resolved channel Id.
                this.runtime = CommerceRuntime.Create(configuration, new CommercePrincipal(new CommerceIdentity(channelId, new string[] { CommerceRoles.Storefront })));

                this.channelManager = ChannelManager.Create(this.runtime);
                this.productManager = ProductManager.Create(this.runtime);
                this.onlineChannel = this.channelManager.GetOnlineChannel(channelId);
                this.publishingConfig = publishingConfig;
            }

            /// <summary>
            /// Gets the CRT connection string.
            /// </summary>
            public string CrtConnectionString
            {
                get
                {
                    return this.runtime.Configuration.ConnectionString;
                }
            }

            /// <summary>
            /// Gets the publishing configuration.
            /// </summary>
            public PublishingConfiguration PublishingConfiguration
            {
                get
                {
                    return this.publishingConfig;
                }
            }

            /// <summary>
            /// Gets Channel languages.
            /// </summary>
            public IReadOnlyCollection<ChannelLanguage> ChannelLanguages
            {
                get
                {
                    return this.onlineChannel.ChannelLanguages;
                }
            }

            /// <summary>
            /// Gets the channel Id.
            /// </summary>
            public long ChannelId
            {
                get
                {
                    return this.onlineChannel.RecordId;
                }
            }

            /// <summary>
            /// Initiates a channel publishing process.
            /// </summary>
            /// <param name="channelPublisher">Instance of the object which implements IChannelPublisher.</param>
            /// <returns>Return publishing parameters.</returns>
            /// <remarks>Retrieves the channel info from the CRT, then executes callbacks for the supplied IChannelPublisher and finally updates the channel publishing status in CRT/AX.</remarks>
            public PublishingParameters PublishChannel(IChannelPublisher channelPublisher)
            {
                if (channelPublisher == null)
                {
                    throw new ArgumentNullException(nameof(channelPublisher));
                }

                if (this.onlineChannel.PublishStatus != OnlineChannelPublishStatusType.Published
                    && this.onlineChannel.PublishStatus != OnlineChannelPublishStatusType.InProgress)
                {
                    throw new ChannelNotPublishedException(Resources.ErrorChannelNotInPublishedState, this.onlineChannel.PublishStatus, this.onlineChannel.PublishStatusMessage);
                }

                IEnumerable<Category> categories;
                Dictionary<long, IEnumerable<AttributeCategory>> categoriesAttributes;

                // always load the categories but process them only if the channel is not published yet.
                try
                {
                    this.LoadCategories(out categories, out categoriesAttributes);
                    int categoriesCount = categories.Count();
                    NetTracer.Information(Resources.NumberOfReadCategoriesAndTheirAttributes, categoriesCount, categoriesAttributes.Count());
                    if (categoriesCount == 0)
                    {
                        throw new InvalidDataException(string.Format(
                            "Navigation categories count returned is '{0}'. Error details {1}",
                            categoriesCount,
                            Resources.ErrorNoNavigationCategories));
                    }

                    // Loading product attributes schema from CRT
                    IEnumerable<AttributeProduct> productAttributes = this.LoadProductAttributes();
                    channelPublisher.OnValidateProductAttributes(productAttributes);

                    int listingAttributesCount = productAttributes.Count();
                    NetTracer.Information(Resources.NumberOfReadAttributes, listingAttributesCount);
                    if (listingAttributesCount == 0)
                    {
                        throw new InvalidDataException(string.Format(
                            "Listing Attributes Count returned is '{0}'. Error details '{1}'",
                            listingAttributesCount,
                            Resources.ErrorNoSchemaAttributes));
                    }

                    ChannelLanguage language = this.onlineChannel.ChannelLanguages.Single(l => l.IsDefault);
                    CultureInfo culture = new CultureInfo(language.LanguageId);

                    PublishingParameters parameters = new PublishingParameters
                    {
                        Categories = categories,
                        CategoriesAttributes = categoriesAttributes,
                        ProductsAttributes = productAttributes,
                        ChannelDefaultCulture = culture,
                        GiftCartItemId = this.channelManager.GetChannelConfiguration().GiftCardItemId
                    };

                    if (this.onlineChannel.PublishStatus == OnlineChannelPublishStatusType.InProgress)
                    {
                        channelPublisher.OnChannelInformationAvailable(parameters, true);
                        this.channelManager.UpdateOnlineChannelPublishStatus(OnlineChannelPublishStatusType.Published, null);
                    }
                    else
                    {
                        channelPublisher.OnChannelInformationAvailable(parameters, false);
                    }

                    return parameters;
                }
                catch (Exception ex)
                {
                    RetailLogger.Log.EcommercePlatformChannelPublishFailure(ex);
                    string error = string.Format(CultureInfo.InvariantCulture, Resources.ErrorChannelPublishingFailed, ex.Message, DateTime.Now);
                    this.channelManager.UpdateOnlineChannelPublishStatus(OnlineChannelPublishStatusType.Failed, error);
                    throw;
                }
            }

            /// <summary>
            /// Updates listing's publishing status in CRT.
            /// </summary>
            /// <param name="publishingStatuses">Set of publishing statuses to be sent to CRT.</param>
            public void UpdateListingPublishingStatus(IEnumerable<ListingPublishStatus> publishingStatuses)
            {
                if (publishingStatuses == null)
                {
                    throw new ArgumentNullException(nameof(publishingStatuses));
                }

                // Due to CDX restriction CRT doesn't have Language ID as part of the primary key for the Listing Publishing Status
                // Therefore, before passing the data to CRT we need to make sure each record for identical Channel-Catalog-Product (which could happen
                // if we have more than one translation for the same product) combination has unique StatusDateTime, the purpose of the code
                // below is to achieve that.
                // Below we build a map where key is Channel-Catalog-Product and value is a StatusDateTime. While processing each listing
                // we see whether this key already exist in the map, if it does then we read the value for its StatusDateTime, increase it by 5 milliseoncs
                // and then store back into the status object. This code has an assumption that all statuses for the same Channel-Catalog-Product
                // come at approximatelly (we are talking about seconds) the same time because we in fact don't respect Status Date Time for each listing
                // but instead we use first encoutnered listing (for the given composite key) as a base and then we just add 2 seconds to it.
                Dictionary<ListingStatusKey, DateTimeOffset> map = new Dictionary<ListingStatusKey, DateTimeOffset>();

                foreach (ListingPublishStatus status in publishingStatuses)
                {
                    ListingStatusKey currentKey = new ListingStatusKey
                    {
                        ChannelId = status.ChannelId,
                        CatalogId = status.CatalogId,
                        ProductId = status.ProductId,
                    };

                    DateTimeOffset currentMaxValue;
                    if (map.TryGetValue(currentKey, out currentMaxValue))
                    {
                        // AX utcdatetime precision is one second. Adding 2 seconds to be safe from any potential rounding issue.
                        currentMaxValue = currentMaxValue.AddSeconds(2);
                        status.ListingModifiedDateTime = currentMaxValue;
                    }
                    else
                    {
                        currentMaxValue = status.ListingModifiedDateTime;
                    }

                    map[currentKey] = currentMaxValue;
                }

                this.productManager.UpdateListingPublishingStatus(publishingStatuses);
            }

            /// <summary>
            /// Initiates a catalog publishing.
            /// </summary>
            /// <param name="catalogPublisher">Instance of the object which implements ICatalogPublisher.</param>
            /// <returns>True if changed products were found in CRT, False otherwise.</returns>
            /// <remarks>Retrieves the channel's catalogs from CRT and then checks whether CRT contains changed products for each of the catalogs. If changed products are found then
            /// ICatalogPublisher's callbacks are executed to let the caller's code process changed products.</remarks>
            public bool PublishCatalog(ICatalogPublisher catalogPublisher)
            {
                if (catalogPublisher == null)
                {
                    throw new ArgumentNullException(nameof(catalogPublisher));
                }

                List<long> productCatalogIds = new List<long>(1);

                // If catalogs were published to this channel, a given product will be published into SP for each catalog
                // in which it appears, so catalogless publishing would not yield different results for those products.
                // If, however, a product was published directly from the assortment, that product will only be detected
                // and published to SP if the ForceCataloglessPublishing flag is set to 'true' (1) in the job configuration file.
                // The semantics of forcing catalogless publishing as strict, in that catalog-less products will be published
                // if and only if the flag is set. That means, for instance, that if the flag is not set and there are no
                // catalogs published to this channel, the SP job will not detect/publish any products to SP.
                if (this.publishingConfig.ForceNoCatalogPublishing)
                {
                    NetTracer.Information(Resources.ProductCatalogToPublish, 0, "unspecified", "(not a proper catalog)");
                    productCatalogIds.Add(0);
                }

                IReadOnlyCollection<ProductCatalog> productCatalogs = this.GetCatalogs();

                bool deletesFound = this.DeleteProducts(productCatalogs, catalogPublisher);

                foreach (ProductCatalog productCatalog in productCatalogs)
                {
                    productCatalogIds.Add(productCatalog.RecordId);
                }

                ChangedProductsSearchCriteria searchCriteria = new ChangedProductsSearchCriteria
                {
                    DataLevel = CommerceEntityDataLevel.Complete
                };

                searchCriteria.Context.ChannelId = this.onlineChannel.RecordId;

                bool isInitialSync;

                QueryResultSettings productsQuerySettings = this.CreateGetListingsCriteria(
                    this.onlineChannel.ChannelProperties,
                    searchCriteria,
                    out isInitialSync);

                bool changesFound = false;

                try
                {
                    Stopwatch readChangedProductsWatch = Stopwatch.StartNew();
                    searchCriteria.Session = this.productManager.BeginReadChangedProducts(searchCriteria);
                    readChangedProductsWatch.Stop();
                    this.LogTimingMessage(Resources.Duration_ReadChangedProducts, readChangedProductsWatch.Elapsed, searchCriteria.Session.TotalNumberOfProducts);

                    if (searchCriteria.Session.TotalNumberOfProducts > 0)
                    {
                        changesFound = true;
                        int totalProductsCount = 0;

                        Stopwatch timerCummulativeListingRetrieval = new Stopwatch();

                        // loop through the product catalogs, retrieving products.
                        foreach (long productCatalogId in productCatalogIds)
                        {
                            NetTracer.Information(Resources.StartReadProductsFromCatalog, productCatalogId);

                            // set the catalog id on the search criteria
                            searchCriteria.Context.CatalogId = productCatalogId;
                            searchCriteria.Session.ResetNumberOfProductsRead();

                            int pageNumberForCatalog = 0;
                            int catalogProductsCount = 0;

                            // inner loop: load changes, page by page, up to catalog max size
                            do
                            {
                                timerCummulativeListingRetrieval.Start();
                                ChangedProductsSearchResult getProductsResults = this.LoadChangedProducts(searchCriteria, productsQuerySettings);
                                timerCummulativeListingRetrieval.Stop();

                                int numberOfReadProducts = getProductsResults.Results.Count;
                                totalProductsCount += numberOfReadProducts;
                                catalogProductsCount += numberOfReadProducts;
                                this.LogTimingMessage(Resources.NumberOfReadProductsInPageSummary, productCatalogId, catalogProductsCount, totalProductsCount, timerCummulativeListingRetrieval.Elapsed);

                                catalogPublisher.OnChangedProductsFound(getProductsResults, pageNumberForCatalog, productCatalogId);
                                pageNumberForCatalog++;
                            }
                            while (searchCriteria.Session.NumberOfProductsRead < searchCriteria.Session.TotalNumberOfProducts);

                            this.LogTimingMessage(Resources.CatalogReadCompleted, productCatalogId, catalogProductsCount, totalProductsCount, timerCummulativeListingRetrieval.Elapsed);

                            catalogPublisher.OnCatalogReadCompleted(productCatalogId, this);
                        }   // for each product catalog

                        this.LogTimingMessage(Resources.AllCatalogsReadCompleted, totalProductsCount, timerCummulativeListingRetrieval.Elapsed);
                    } // if changed products were found
                }
                finally
                {
                    this.productManager.EndReadChangedProducts(searchCriteria.Session);
                }

                ChannelProperty channelProperty = new ChannelProperty
                {
                    Name = KeySyncAnchor,
                    Value = new string(searchCriteria.Session.NextSynchronizationToken)
                };

                this.channelManager.UpdateChannelProperties(new ChannelProperty[] { channelProperty });

                return changesFound || deletesFound;
            }

            /// <summary>
            /// Gets Store published Ids.
            /// </summary>
            /// <param name="ids">Returns ids.</param>
            public void StorePublishedIds(IEnumerable<ListingIdentity> ids)
            {
                DataAccessor accessor = new DataAccessor(this.onlineChannel.RecordId, this.runtime.Configuration.ConnectionString, this.publishingConfig.CRTListingPageSize);
                accessor.StorePublishedIds(ids);
            }

            /// <summary>
            /// Logs a timing message, overriding the default level if so specified in the configuration.
            /// </summary>
            /// <param name="format">Message format.</param>
            /// <param name="args">Message parameters.</param>
            public void LogTimingMessage(string format, params object[] args)
            {
                if (this.publishingConfig.ForceTimingInfoLogging)
                {
                    RetailLogger.Log.EcommercePlatformTimingErrorLevelMessage(format, args);
                }
                else
                {
                    RetailLogger.Log.EcommercePlatformTimingInformationalMessage(format, args);
                }
            }

            /// <summary>
            /// Retrieves set of catalogs published to the channel.
            /// </summary>
            /// <returns>Collection of catalogs published to the channel.</returns>
            internal IReadOnlyCollection<ProductCatalog> GetCatalogs()
            {
                QueryResultSettings catalogsQuerySettings = new QueryResultSettings(new PagingInfo(this.publishingConfig.CRTListingPageSize, 0));
                IReadOnlyCollection<ProductCatalog> productCatalogs = this.productManager.GetProductCatalogs(this.onlineChannel.RecordId, true, catalogsQuerySettings).Results;
                return productCatalogs;
            }

            /// <summary>
            /// Verifies products existence in CRT.
            /// </summary>
            /// <param name="catalogId">The catalog ID.</param>
            /// <param name="ids">ProductExistence IDs.</param>
            /// <returns>ProductExistence IDs found in CRT.</returns>
            /// <remarks>This method can be used to figure out which products should be removed from the target channel if they no longer exist in CRT.</remarks>
            internal ReadOnlyCollection<ProductExistenceId> VerifyProductExistence(long catalogId, IEnumerable<ProductExistenceId> ids)
            {
                ProductExistenceCriteria criteria = new ProductExistenceCriteria
                {
                    ChannelId = this.onlineChannel.RecordId,
                    CatalogId = catalogId,
                    Ids = ids
                };

                ProductManager manager = ProductManager.Create(this.runtime);
                ReadOnlyCollection<ProductExistenceId> crtIds = manager.VerifyProductExistence(criteria, QueryResultSettings.AllRecords).Results;
                return crtIds;
            }

            private static IEnumerable<ProductExistenceId> ConvertToProductExistenceId(List<ListingIdentity> ids)
            {
                List<ProductExistenceId> result = new List<ProductExistenceId>(ids.Count);
                foreach (ListingIdentity id in ids)
                {
                    result.Add(new ProductExistenceId
                    {
                        LanguageId = id.LanguageId,
                        ProductId = id.ProductId
                    });
                }

                return result;
            }

            private static IList<ListingIdentity> GetIdsToBeRemoved(IEnumerable<ProductExistenceId> crtIds, IEnumerable<ListingIdentity> publishedIds)
            {
                // Adding CRT results to a dictionary whether the key is a product ID and value is a hashset of languages available for that Id.
                // This is to speedup the check whether the product should be deleted or not.
                Dictionary<long, HashSet<string>> crtProductIds = new Dictionary<long, HashSet<string>>();
                foreach (ProductExistenceId crtId in crtIds)
                {
                    HashSet<string> currentLanguages;
                    if (!crtProductIds.TryGetValue(crtId.ProductId, out currentLanguages))
                    {
                        currentLanguages = new HashSet<string>();
                        crtProductIds.Add(crtId.ProductId, currentLanguages);
                    }

                    currentLanguages.Add(crtId.LanguageId);
                }

                // Creating a list of items which were publsied but don't exist in CRT.
                List<ListingIdentity> result = new List<ListingIdentity>();
                foreach (ListingIdentity publishedId in publishedIds)
                {
                    bool addToDeletions = false;

                    HashSet<string> currenLanguages;
                    if (!crtProductIds.TryGetValue(publishedId.ProductId, out currenLanguages))
                    {
                        addToDeletions = true;
                    }
                    else
                    {
                        if (!currenLanguages.Contains(publishedId.LanguageId))
                        {
                            addToDeletions = true;
                        }
                    }

                    if (addToDeletions)
                    {
                        result.Add(publishedId);
                    }
                }

                return result;
            }

            /// <summary>
            /// Loads AX categories and their attributes.
            /// </summary>
            /// <param name="categories">Upon function return contains loaded AX categories.</param>
            /// <param name="categoryAttributesMap">Upon function return contains loaded AX categories' attributes.</param>
            private void LoadCategories(out IEnumerable<Category> categories, out Dictionary<long, IEnumerable<AttributeCategory>> categoryAttributesMap)
            {
                ////******** Reading categories *****************
                QueryResultSettings getCategoriesCriteria = new QueryResultSettings(new PagingInfo(this.publishingConfig.CategoriesPageSize, 0));

                List<Category> resultCategories = new List<Category>();
                categories = resultCategories;

                IEnumerable<Category> currentPageCategories;
                do
                {
                    currentPageCategories = this.channelManager.GetChannelCategoryHierarchy(getCategoriesCriteria).Results;
                    resultCategories.AddRange(currentPageCategories);
                    getCategoriesCriteria.Paging.Skip = getCategoriesCriteria.Paging.Skip + this.publishingConfig.CategoriesPageSize;
                }
                while (currentPageCategories.Count() == getCategoriesCriteria.Paging.Top);

                // ******* Reading categories' attributes
                QueryResultSettings getCategoryAttributesCriteria = new QueryResultSettings(new PagingInfo(this.publishingConfig.CategoriesPageSize, 0));
                categoryAttributesMap = new Dictionary<long, IEnumerable<AttributeCategory>>();
                foreach (Category category in categories)
                {
                    getCategoryAttributesCriteria.Paging.Skip = 0;
                    List<AttributeCategory> allCategoryAttributes = new List<AttributeCategory>();
                    IEnumerable<AttributeCategory> categoryAttributes;
                    do
                    {
                        categoryAttributes = this.channelManager.GetChannelCategoryAttributes(getCategoryAttributesCriteria, category.RecordId).Results;
                        allCategoryAttributes.AddRange(categoryAttributes);
                        getCategoryAttributesCriteria.Paging.Skip = getCategoryAttributesCriteria.Paging.Skip + this.publishingConfig.CategoriesPageSize;

                        categoryAttributesMap.Add(category.RecordId, allCategoryAttributes);
                    }
                    while (categoryAttributes.Count() == getCategoryAttributesCriteria.Paging.Top);
                }
            }

            /// <summary>
            /// Loads AX product attributes.
            /// </summary>
            /// <returns>Returns product attributes.</returns>
            private IEnumerable<AttributeProduct> LoadProductAttributes()
            {
                QueryResultSettings getProductAttributesCriteria = new QueryResultSettings(new PagingInfo(this.publishingConfig.ProductAttributesPageSize, 0));

                List<AttributeProduct> attributes = new List<AttributeProduct>();
                IEnumerable<AttributeProduct> currentAttributePage;
                do
                {
                    currentAttributePage = this.productManager.GetChannelProductAttributes(getProductAttributesCriteria).Results;
                    attributes.AddRange(currentAttributePage);
                    getProductAttributesCriteria.Paging.Skip = getProductAttributesCriteria.Paging.Skip + getProductAttributesCriteria.Paging.Top;
                }
                while (currentAttributePage.Count() == getProductAttributesCriteria.Paging.Top);

                return attributes;
            }

            /// <summary>
            /// Creates query criteria to read listings.
            /// </summary>
            /// <param name="channelProperties">Properties of the channel.</param>
            /// <param name="searchCriteria">Search criteria.</param>
            /// <param name="isInitialSync">Is initial sync.</param>
            /// <returns>New instance of filter criteria.</returns>
            private QueryResultSettings CreateGetListingsCriteria(
                IEnumerable<ChannelProperty> channelProperties,
                ChangedProductsSearchCriteria searchCriteria,
                out bool isInitialSync)
            {
                char[] syncToken;
                ChannelProperty channelProperty = channelProperties.SingleOrDefault(p => p.Name == KeySyncAnchor);
                if (channelProperty == null)
                {
                    syncToken = ProductChangeTrackingAnchorSet.GetSynchronizationTokenFromAnchorSet(new ProductChangeTrackingAnchorSet(), 0);
                    isInitialSync = true;
                }
                else
                {
                    syncToken = channelProperty.Value.ToCharArray();
                    isInitialSync = false;
                }

                // Reading listings.
                QueryResultSettings getListingsCriteria = new QueryResultSettings(new PagingInfo(this.publishingConfig.CRTListingPageSize, 0));

                searchCriteria.SynchronizationToken = syncToken;

                return getListingsCriteria;
            }

            /// <summary>
            /// Loads the specified listing page, and returns true if a full page was loaded.
            /// </summary>
            /// <param name="productsSearchCriteria">The search criteria.</param>
            /// <param name="querySettings">The query settings.</param>
            /// <returns>Returns changed products.</returns>
            private ChangedProductsSearchResult LoadChangedProducts(ChangedProductsSearchCriteria productsSearchCriteria, QueryResultSettings querySettings)
            {
                Stopwatch watch = Stopwatch.StartNew();

                ChangedProductsSearchResult results;

                results = this.productManager.GetChangedProducts(productsSearchCriteria, querySettings);

                watch.Stop();
                int numberOfReadProducts = results.Results.Count;
                this.LogTimingMessage(Resources.NumberOfReadProductsInPage, numberOfReadProducts, watch.Elapsed);

                return results;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Refactoring will be costly at this stage. Should be taken up in next cycle.")]
            private bool DeleteProducts(
              IReadOnlyCollection<ProductCatalog> productCatalogs,
              ICatalogPublisher catalogPublisher)
            {
                Stopwatch watchTotalDelete = Stopwatch.StartNew();
                bool changesDetected = false;

                //// 1: delete listings for the catalogs which are no longer exist in AX (were retracted for instance).
                //// This is the one of 2 fastest steps (because we don't need to query CRT for each product to figure out whether it is still there or not)

                DataAccessor dataAccessor = new DataAccessor(this.onlineChannel.RecordId, this.runtime.Configuration.ConnectionString, this.publishingConfig.CRTListingPageSize);
                Stopwatch watch = Stopwatch.StartNew();
                Dictionary<string, List<long>> catalogsToBeDeleted = dataAccessor.GetNotExistingCatalogs(productCatalogs);
                watch.Stop();
                this.LogTimingMessage(Resources.Duration_GetNotExistingCatalogs, watch.Elapsed, catalogsToBeDeleted.Count, catalogsToBeDeleted.Values.Count);

                if (catalogsToBeDeleted.Count > 0)
                {
                    watch.Restart();
                    catalogPublisher.OnDeleteProductsByCatalogIdRequested(catalogsToBeDeleted);
                    watch.Stop();
                    this.LogTimingMessage(Resources.Duration_Processor_DeleteListingsByCatalogs, watch.Elapsed);
                }

                watch.Restart();
                dataAccessor.DeleteListingsByCatalogs(catalogsToBeDeleted.SelectMany(c => c.Value));
                watch.Stop();
                this.LogTimingMessage(Resources.Duration_Manager_DeleteListingsByCatalogs, watch.Elapsed);

                // 2. delete listings for languages which are no longer exist on channel, this is another fast operation (in terms of querying CRT).
                watch.Restart();
                Dictionary<string, List<string>> languagesToBeDeleted = dataAccessor.GetNotExistingLanguages(this.ChannelLanguages);
                watch.Stop();
                this.LogTimingMessage(Resources.Duration_GetNotExistingLanguages, watch.Elapsed, languagesToBeDeleted.Keys.Count, languagesToBeDeleted.Values.Count);

                if (languagesToBeDeleted.Count > 0)
                {
                    watch.Restart();
                    catalogPublisher.OnDeleteProductsByLanguageIdRequested(languagesToBeDeleted);
                    watch.Stop();
                    this.LogTimingMessage(Resources.Duration_Processor_DeleteListingsByLanguage, watch.Elapsed);
                }

                if (languagesToBeDeleted.Count > 0)
                {
                    watch.Restart();
                    dataAccessor.DeleteListingsByLanguages(languagesToBeDeleted.SelectMany(c => c.Value));
                    watch.Stop();
                    this.LogTimingMessage(Resources.Duration_Processor_DeleteListingsByLanguages, watch.Elapsed);
                }

                changesDetected |= (catalogsToBeDeleted.Count > 0) || (languagesToBeDeleted.Count > 0);

                if (this.publishingConfig.CheckEveryListingForRemoval)
                {
                    // 3: Finally read all listings left from published listings table and ask CRT whehter the product still available there or not
                    watch.Restart();
                    Dictionary<long, List<ListingIdentity>> catalogs = dataAccessor.LoadAllListingsMap();
                    watch.Stop();
                    int listingsCount = 0;
                    foreach (List<ListingIdentity> list in catalogs.Values)
                    {
                        listingsCount += list.Count;
                    }

                    this.LogTimingMessage(Resources.Duration_LoadListingsMap, watch.Elapsed, listingsCount, catalogs.Keys.Count);

                    // Loop over published listings which are grouped by a catalog
                    foreach (KeyValuePair<long, List<ListingIdentity>> catalog in catalogs)
                    {
                        int bottomIndex = 0;
                        List<ListingIdentity> publishedIds = catalog.Value;

                        // Calling CRT, in a separate pages, to find out whether the products are still available or not.
                        while (bottomIndex < publishedIds.Count)
                        {
                            int topIndex = bottomIndex + this.publishingConfig.CRTListingPageSize - 1;
                            if (topIndex + 1 >= publishedIds.Count)
                            {
                                topIndex = publishedIds.Count - 1;
                            }

                            List<ListingIdentity> currentPagePublishedIds = publishedIds.GetRange(bottomIndex, topIndex + 1 - bottomIndex);
                            int previousBottomIndex = bottomIndex;
                            bottomIndex = topIndex + 1;

                            watch.Restart();
                            System.Collections.ObjectModel.ReadOnlyCollection<ProductExistenceId> crtIds = this.VerifyProductExistence(catalog.Key, ConvertToProductExistenceId(currentPagePublishedIds));
                            watch.Stop();
                            this.LogTimingMessage(Resources.Duration_VerifyProductExistence, watch.Elapsed, currentPagePublishedIds.Count, previousBottomIndex, publishedIds.Count, crtIds.Count);

                            IList<ListingIdentity> idsToBeRemoved = GetIdsToBeRemoved(crtIds, currentPagePublishedIds);
                            changesDetected |= idsToBeRemoved.Any();

                            if (idsToBeRemoved.Count > 0)
                            {
                                watch.Restart();
                                catalogPublisher.OnDeleteIndividualProductsRequested(idsToBeRemoved);
                                watch.Stop();
                                this.LogTimingMessage(Resources.Duration_Processor_DeleteListingsByCompositeIds, watch.Elapsed, idsToBeRemoved.Count);

                                watch.Restart();
                                dataAccessor.DeleteListingsByCompositeIds(catalog.Key, idsToBeRemoved);
                                watch.Stop();
                                this.LogTimingMessage(Resources.Duration_Manager_DeleteListingsByCompositeIds, watch.Elapsed, idsToBeRemoved.Count);
                            }

                            List<ListingPublishStatus> statuses = new List<ListingPublishStatus>();
                            foreach (ListingIdentity id in idsToBeRemoved)
                            {
                                statuses.Add(Listing.CreateStatusSuccessfullyDeleted(this.onlineChannel.RecordId, id.CatalogId, id.ProductId, id.LanguageId));
                            }

                            if (statuses.Count > 0)
                            {
                                this.UpdateListingPublishingStatus(statuses);
                            }
                        }
                    }

                    watchTotalDelete.Stop();
                    this.LogTimingMessage(Resources.Duration_DeleteProducts, watchTotalDelete.Elapsed, changesDetected);
                }

                return changesDetected;
            }

            /// <summary>
            /// Describes a primary key of the AX.RETAILLISTINGSTATUSLOG table.
            /// </summary>
            private struct ListingStatusKey
            {
                /// <summary>
                /// Gets or sets the Channel ID.
                /// </summary>
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Legacy code.")]
                public long ChannelId
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the Catalog ID.
                /// </summary>
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Legacy code.")]
                public long CatalogId
                {
                    get;
                    set;
                }

                /// <summary>
                /// Gets or sets the Product ID.
                /// </summary>
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Legacy code.")]
                public long ProductId
                {
                    get;
                    set;
                }
            }
        }
    }
}
