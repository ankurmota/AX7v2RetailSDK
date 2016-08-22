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
    export class MediaBlobHelper {

        /**
         * Returns the default media blob from the specified media blobs. If there is no default, then it will simply return the first media blob.
         * This is used in cases where you need to display one media blob (offline image) out of the media blobs available.
         * @param {Proxy.Entities.MediaBlob[]} mediaBlobs The media blobs.
         * @return {Proxy.Entities.MediaBlob} The default of first media blob (null if the specified blob array is empty).
         */
        public static getDefaultOrFirstMediaBlob(mediaBlobs: Proxy.Entities.MediaBlob[]): Proxy.Entities.MediaBlob {
            var matchedMediaBlob: Proxy.Entities.MediaBlob = null;

            if (ArrayExtensions.hasElements(mediaBlobs)) {
                matchedMediaBlob = ArrayExtensions.firstOrUndefined(mediaBlobs, (mediaBlob: Proxy.Entities.MediaBlob) => mediaBlob.IsDefault);
                if (ObjectExtensions.isNullOrUndefined(matchedMediaBlob)) {
                    matchedMediaBlob = mediaBlobs[0];
                }
            }

            return matchedMediaBlob;
        }
    }
}