/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce {
    "use strict";
    export class ProductSearchResultHelper {

        /**
         * Attaches "OfflineImageHandler" to each of the specified product search result of the specified array. 
         * (The entity in the array could be either ProductSearchResult or SimpleProduct).
         * @param {Proxy.Entities.ProductSearchResult[]} productSearchResults Array of ProductSearchResult or SimpleProduct entities.
         * @param {number} channelId The channel identifier.
         * @param {number} catalogId The catalog identifier.
         */
        public static attachOfflineImageHandler(productSearchResults: Proxy.Entities.ProductSearchResult[], channelId: number, catalogId: number): void {
            if (ArrayExtensions.hasElements(productSearchResults)) {
                var productManager: Model.Managers.IProductManager =
                    Model.Managers.Factory.getManager<Model.Managers.IProductManager>(Model.Managers.IProductManagerName);

                 // False positive.
                productSearchResults.forEach((result: Proxy.Entities.ProductSearchResult): void => {
                    var offlineResult: any = result;

                    offlineResult.OfflineImageHandler = ((productSearchResult: Proxy.Entities.ProductSearchResult): IAsyncResult<string> => {
                        var offlineImage: string = StringExtensions.EMPTY;

                        if (!ObjectExtensions.isNullOrUndefined(productSearchResult)) {
                            return productManager.getMediaBlobsAsync(
                                productSearchResult.RecordId,
                                channelId,
                                catalogId)
                                .map((mediaBlobs: Proxy.Entities.MediaBlob[]) => {
                                    var defaultMediBlob: Proxy.Entities.MediaBlob = Commerce.MediaBlobHelper.getDefaultOrFirstMediaBlob(mediaBlobs);
                                    if (!ObjectExtensions.isNullOrUndefined(defaultMediBlob)) {
                                        offlineImage = defaultMediBlob.Content;
                                    }

                                    return offlineImage;
                                });
                        }

                        return AsyncResult.createResolved(offlineImage);
                    });
                });
                
            }
        }
    }
}