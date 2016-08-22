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
    namespace Retail.Ecommerce.Publishing
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using Microsoft.Dynamics.Commerce.Runtime.DataModel;
        using Retail.Channels;
        using Retail.Ecommerce.Sdk.Core.Publishing;
    
        internal class CatalogPublisher : ICatalogPublisher
        {
            /// <summary>
            /// Stores all listings accumulated for the current catalog.
            /// </summary>
            private List<Listing> globalListings = new List<Listing>();
    
            /// <summary>
            /// Indicates the all changed products for the given, passed via the catalogId parameter, catalog were retrieved (in form of multiple previous calls by meanings of OnChangedProductsFound).
            /// </summary>
            /// <param name="catalogId">The catalog identifier.</param>
            /// <param name="publisher">Instance of the publisher.</param>
            /// <remarks>This method is called once for every channel's catalog once all changed products were already read. Implementation of this method could, for instance, initiate a publishing to the target channel.</remarks>
            public void OnCatalogReadCompleted(long catalogId, Publisher publisher)
            {
                if (publisher == null)
                {
                    throw new ArgumentNullException("publisher");
                }
    
                Trace.TraceInformation("Catalog read completed. CatalogID={0}.", catalogId);
    
                // Your code to handle received products should be here, alternatively it could also be placed in OnChangedProductsFound callback
                // in case you want more granular handling, the decision is up to you and depends on your specific implementation details.
                // Once your code completes inserting the products into the target channel (like Search Index for instance) we need to:
                // 1. store published products' IDs in the cache (DB table which is logical part of CRT DB) which is used by the Publisher to detect deleted products (is not used for any other purposes).
                // 2. update publishing status for the listings so it is available in AX for review
                // Below is the code which demonstrated how those 2 steps could be acheived
                List<ListingIdentity> ids = new List<ListingIdentity>();
                List<ListingPublishStatus> statuses = new List<ListingPublishStatus>();
    
                // Traversing all listings accumulated for the given catalog.
                foreach (Listing listing in this.globalListings)
                {
                    // 1. Preparing IDs for those listings which were successfully published into the target channel
                    // Do not add IDs corresponding to those listings which failed to publish.
                    ListingIdentity id = new ListingIdentity
                    {
                        CatalogId = catalogId,
    
                        // Replace with any value specific to your published listing
                        Tag = listing.RecordId.ToString(),
                        LanguageId = listing.Locale,
                        ProductId = listing.RecordId
                    };
    
                    ids.Add(id);
    
                    // 2. Preparing the publishing status for each listing.
                    // If your listing failed to publish (it could be rejected by your Search Index by any reason which is specific to your needs) then use method CreateStatusFailedToPublish instead.
                    statuses.Add(listing.CreateStatusSuccessfullyPublished());
                }
    
                // Storing successfully published IDs.
                Trace.TraceInformation("Initiating storing published IDs...");
                publisher.StorePublishedIds(ids);
    
                // Storing publishing statuses.
                Trace.TraceInformation("Initiating storing publishing status...");
                publisher.UpdateListingPublishingStatus(statuses);
    
                // Finally cleaning up global listings' list because there could be more than 1 catalog for the current channel
                this.globalListings.Clear();
            }
    
            /// <summary>
            /// Indicates that changed (new or modified) products were found in CRT.
            /// </summary>
            /// <param name="products">The products which were changed.</param>
            /// <param name="pageNumberInCatalog">Page number used while retrieving products from CRT. Can be used by clients for diagnostics purposes.</param>
            /// <param name="catalogId">The catalog ID which contains the changed products.</param>
            /// <remarks>The class which implements this method should expect this method to be called multiple times. Number of times it is called depends on page size used while initializing the Publisher.</remarks>
            public void OnChangedProductsFound(ChangedProductsSearchResult products, int pageNumberInCatalog, long catalogId)
            {
                if (products == null)
                {
                    throw new ArgumentNullException("products");
                }
    
                Trace.TraceInformation("Page read completed. Products read in this page={0}, The page number={1}, CatalogID={2}", products.Results.Count, pageNumberInCatalog, catalogId);
    
                // Flattening products into Listings. The flattening is done by using Language dimension, so, if a product contains more than 1 language in its attributes there will be
                // one listing per each language. This call is optional and is only needed if target channel really needs these flattened form of the product.
                IEnumerable<Listing> listings = products.Results.SelectMany(p => Listing.GetListings(p));
    
                Trace.TraceInformation("{0} products were flattened into {1} listings", products.Results.Count, listings.Count());
    
                // Accumulating listings because this callback could be called multiple times per each catalog based on the page size.
                this.globalListings.AddRange(listings);
    
                Trace.TraceInformation("Total saved listings so far: {0}", this.globalListings.Count);
            }
    
            /// <summary>
            /// Indicates that deleted catalogs were found in CRT.
            /// </summary>
            /// <param name="catalogs">Deleted catalogs.</param>
            public void OnDeleteProductsByCatalogIdRequested(Dictionary<string, List<long>> catalogs)
            {
                throw new NotImplementedException();
            }
    
            /// <summary>
            /// Indicates that deleted languages, belonging to the channel, were found in CRT.
            /// </summary>
            /// <param name="languageIds">Deleted languages.</param>
            public void OnDeleteProductsByLanguageIdRequested(Dictionary<string, List<string>> languageIds)
            {
                throw new NotImplementedException();
            }
    
            /// <summary>
            /// Indicates that individually deleted products were found in CRT.
            /// </summary>
            /// <param name="ids">Deleted products' IDs.</param>
            public void OnDeleteIndividualProductsRequested(IList<ListingIdentity> ids)
            {
                throw new NotImplementedException();
            }
        }
    }
}
