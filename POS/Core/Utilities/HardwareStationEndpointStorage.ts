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

    interface IHardwareStationStorageItem {
        hardwareStationParingTokens: { [hardwareStationEndpointKey: string]: string };
    }

    /**
     * Helper class to store and retrieve hardware station endpoint data.
     */
    export class HardwareStationEndpointStorage {

        /**
         * Gets the active hardware station from the persistant storage.
         * @returns {Model.Entities.IHardwareStation} the hardware station or null if there's no paired one.
         */
        public static getActiveHardwareStation(): Model.Entities.IHardwareStation {
            var serializedValue: string = ApplicationStorage.getItem(ApplicationStorageIDs.ACTIVE_HARDWARE_STATION);
            var hardwareStation: Model.Entities.IHardwareStation = null;
            if (!StringExtensions.isNullOrWhitespace(serializedValue)) {
                hardwareStation = <Model.Entities.IHardwareStation>JSON.parse(serializedValue);
            }

            return hardwareStation;
        }

        /**
         * Persists the active hardware station in the storage.
         * @params {Model.Entities.IHardwareStation} hardwareStation the hardware station to persist as active.
         */
        public static setActiveHardwareStation(hardwareStation: Model.Entities.IHardwareStation): void {
            var serializedValue: string = "";
            if (!ObjectExtensions.isNullOrUndefined(hardwareStation)) {
                serializedValue = JSON.stringify(hardwareStation);
            }
            ApplicationStorage.setItem(ApplicationStorageIDs.ACTIVE_HARDWARE_STATION, serializedValue);
        }

        /**
         * Removes the active hardware station from the storage.
         */
        public static clearActiveHardwareStation(): void {
            HardwareStationEndpointStorage.setActiveHardwareStation(null);
        }

        /**
         * Gets the hardware station token.
         * @param {number} hardwareStationRecordId the hardware station identifier.
         * @param {string} hardwareStationUrl the hardware station url.
         * @returns {string} the hardware station token.
         */
        public static getHardwareStationToken(hardwareStationRecordId: number, hardwareStationUrl: string): string {
            var item: IHardwareStationStorageItem = HardwareStationEndpointStorage.getStorageItem();
            var key: string = this.getHardwareStationTokenKey(hardwareStationRecordId, hardwareStationUrl);
            return item.hardwareStationParingTokens[key];
        }

        /**
         * Gets whether there is a hardware station token for given id and url.
         * @param {number} hardwareStationRecordId the hardware station identifier.
         * @param {string} hardwareStationUrl the hardware station url.
         * @returns {boolean} True if the token exists otherwise false.
         */
        public static hasHardwareStationToken(hardwareStationRecordId: number, hardwareStationUrl: string): boolean {
            return !StringExtensions.isNullOrWhitespace(HardwareStationEndpointStorage.getHardwareStationToken(hardwareStationRecordId, hardwareStationUrl));
        }

        /**
         * Sets the hardware station token.
         * @param {number} hardwareStationRecordId the hardware station identifier.
         * @param {string} hardwareStationUrl the hardware station url.
         * @param {string} token the hardware station token.
         */
        public static setHardwareStationToken(hardwareStationRecordId: number, hardwareStationUrl: string, token: string): void {
            var item: IHardwareStationStorageItem = HardwareStationEndpointStorage.getStorageItem();
            var key: string = this.getHardwareStationTokenKey(hardwareStationRecordId, hardwareStationUrl);
            item.hardwareStationParingTokens[key] = token;
            HardwareStationEndpointStorage.setStorageItem(item);
        }

        /**
         * Gets the hardware station storage item.
         * @returns {HardwareStationStorageItem} the hardware station token.
         */
        private static getStorageItem(): IHardwareStationStorageItem {
            var storedValue: string = ApplicationStorage.getItem(ApplicationStorageIDs.HARDWARE_STATION_ENPOINT_STORAGE);
            var item: IHardwareStationStorageItem = <IHardwareStationStorageItem>JSON.parse(storedValue);

            if (ObjectExtensions.isNullOrUndefined(item)) {
                item = <IHardwareStationStorageItem>{ hardwareStationParingTokens: {} };
            }

            return item;
        }

        /**
         * Gets the hardware station storage item.
         * @returns {HardwareStationStorageItem} the hardware station token.
         */
        private static setStorageItem(item: IHardwareStationStorageItem): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.HARDWARE_STATION_ENPOINT_STORAGE, JSON.stringify(item) || "");
        }

        private static getHardwareStationTokenKey(hardwareStationRecordId: number, hardwareStationUrl: string): string {
            return hardwareStationRecordId + "/" + hardwareStationUrl;
        }
    }
}