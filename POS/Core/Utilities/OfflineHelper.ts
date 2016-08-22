/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Utilities {
    "use strict";

    /**
     * Helper class for offline operations.
     */
    export class OfflineHelper {
        /**
         * Stop the current offline sync and brings back to online.
         */
        public static stopOffline(): void {
            if (OfflineHelper.isOfflineEnabled()) {
                // Clear offline sync timer.
                clearTimeout(Commerce.Session.instance.offlineParameters.syncDownloadOfflineData);
                Commerce.Session.instance.offlineParameters.syncDownloadOfflineData = 0;
                clearTimeout(Commerce.Session.instance.offlineParameters.syncUploadOfflineData);
                Commerce.Session.instance.offlineParameters.syncUploadOfflineData = 0;
            }
            Session.instance.connectionStatus = ConnectionStatusType.Online;
        }

        /**
         * Helper method to determine whether offline is enabled or not.
         */
        public static isOfflineEnabled(): boolean {
            var offlineEnabled: boolean = !StringExtensions.isNullOrWhitespace(Commerce.Config.offlineDatabase) &&
                !StringExtensions.isEmpty(Commerce.Config.offlineDatabase);

            return offlineEnabled;
        }
    }
}