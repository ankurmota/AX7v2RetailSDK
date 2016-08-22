/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Host/IHost.ts'/>

module Commerce {
    "use strict";

    type NonEssentialDataFilter = (value: string) => string;

    export enum ApplicationStorageIDs {
        CART_KEY,
        CASH_DRAWER_NAME,
        CASH_DRAWER_TYPE,
        CUSTOM_UI_STRINGS_KEY,
        CONNECTION_STATUS,
        ENVIRONMENT_CONFIGURATION_KEY,
        DEVICE_CONFIGURATION_KEY,
        DEVICE_ID_KEY,
        DEVICE_TOKEN_KEY,
        EMPLOYEE_LIST_KEY,
        HARDWARE_PROFILE_KEY,
        ACTIVE_HARDWARE_STATION,
        HARDWARE_STATION_ENPOINT_STORAGE,
        INITIAL_SYNC_COMPLETED_KEY,
        NUMBER_SEQUENCES_KEY,
        CART_RECEIPT_NUMBER_SEQUENCE_KEY,
        REGISTER_ID_KEY,
        RETAIL_SERVER_URL,
        SHIFT_KEY,
        STORE_ID_KEY,
        FIRST_TIME_USE,
        BUBBLE_TOUR_DISABLED,
        VIDEO_TUTORIAL_DISABLED,
        AAD_LOGON_IN_PROCESS_KEY,
        ACTIVATION_PAGE_PARAMETERS_KEY,
        CLOUD_SESSION_ID,
        NAVIGATION_LOGGING_ENABLED,
        NAVIGATION_LOG_VISIBLE,
        CSS_DEVMODE,
        CSS_THEME_COLOR,
        CSS_BODY_DIRECTION,
        CSS_DEV_GRID,
        CSS_DEV_COLORS,
        APP_BAR_ALWAYS_VISIBLE,
        APPLICATION_VERSION,
        TENANT_ID,
        DEVICE_ACTIVATION_COMPLETED,
        IGNORE_UNSUPPORTED_BROWSER_ERROR
    }

    /**
     * Class for storing data that is persisted past application close.
     */
    export class ApplicationStorage {
        private static nonEssentialDataFilterMap: Dictionary<NonEssentialDataFilter> = new Dictionary<NonEssentialDataFilter>();
        private static useInMemoryStorage: boolean = false;

        /*
         * Keys to be saved / read from local storage even when the device is configured to use session storage.
         */
        private static LOCALSTORAGE_BOUNDKEYS: ApplicationStorageIDs[] = [
            ApplicationStorageIDs.RETAIL_SERVER_URL,
            ApplicationStorageIDs.CART_KEY,
            ApplicationStorageIDs.FIRST_TIME_USE,
            ApplicationStorageIDs.BUBBLE_TOUR_DISABLED,
            ApplicationStorageIDs.VIDEO_TUTORIAL_DISABLED,
            ApplicationStorageIDs.CLOUD_SESSION_ID,
            ApplicationStorageIDs.DEVICE_ID_KEY,
            ApplicationStorageIDs.REGISTER_ID_KEY,
            ApplicationStorageIDs.TENANT_ID,
            ApplicationStorageIDs.ENVIRONMENT_CONFIGURATION_KEY,
            ApplicationStorageIDs.DEVICE_ACTIVATION_COMPLETED
        ];

        /*
         * Keys to be saved / read from session storage even when the device is configured to use local storage.
         */
        private static SESSIONSTORAGE_BOUNDKEYS: ApplicationStorageIDs[] = [
            ApplicationStorageIDs.IGNORE_UNSUPPORTED_BROWSER_ERROR
        ];

        /**
         * Method to register filters for non-essential data by storage id.
         * @param {ApplicationStorageIDs} key They application storage id that the filter is associated with.
         * @param {(value: string) => string)} dataFilter The filter to use on the data.
         * @remarks These filters are used to reduce the WebStorage consumption of the application if the WebStorage quota is surpassed.
         */
        public static registerNonEssentialDataFilter(key: ApplicationStorageIDs, dataFilter: (value: string) => string): void {
            if (!ObjectExtensions.isFunction(dataFilter)) {
                throw "ApplicationStorage::StorageQuotaReachedRecoveryHandler must be a function.";
            }

            var applicationStorageId: string = ApplicationStorage.getStorageKeyFromId(key);
            ApplicationStorage.nonEssentialDataFilterMap.setItem(applicationStorageId, dataFilter);
        }

        /**
         * Gets the specified item from local storage specified by storageId.
         * @param {ApplicationStorageIDs} itemKey The storage Id of the value to be retrieved.
         * @return {string} The value of the stored item.
         */
        public static getItem(itemKey: ApplicationStorageIDs): string {
            var storageKey: string = ApplicationStorage.getStorageKeyFromId(itemKey);
            return ApplicationStorage.getStorage(itemKey).getItem(storageKey);
        }

        /**
         * Sets the specified item value in local storage.
         * @param {ApplicationStorageIDs} itemKey The storage Id of the value to be stored.
         * @param {string} itemValue The value to be stored.
         */
        public static setItem(itemKey: ApplicationStorageIDs, itemValue: string): void {
            var storageKey: string = ApplicationStorage.getStorageKeyFromId(itemKey);
            var storage: Storage = ApplicationStorage.getStorage(itemKey);
            ApplicationStorage.setItemOnStorage(storage, storageKey, itemValue);
        }

        /**
         * Clears the local storage.
         */
        public static clear(): void {
            window.localStorage.clear();
            window.sessionStorage.clear();

            // default configuration
            ApplicationStorage.updateStorageConfiguration(null);
        }

        /**
         * Updates the storage configuration based on configuration.         
         * @param {Proxy.Entities.DeviceConfiguration} newDeviceConfiguration a value indicating what is the new configuration.
         */
        public static updateStorageConfiguration(newDeviceConfiguration: Proxy.Entities.DeviceConfiguration): void {
            var newUseInMemoryStorage: boolean = (newDeviceConfiguration === null) || newDeviceConfiguration.UseInMemoryDeviceDataStorage;
            var currentUseInMemoryStorage: boolean = ApplicationStorage.useInMemoryStorage;

            // when configuration changes, we need to copy the values from the old storage to the new one
            var currentStorage: Storage;
            var newStorage: Storage;

            // nothing to do
            if (currentUseInMemoryStorage === newUseInMemoryStorage) {
                return;
            }

            if (newUseInMemoryStorage) {
                currentStorage = window.localStorage;
                newStorage = window.sessionStorage;
            } else {
                currentStorage = window.sessionStorage;
                newStorage = window.localStorage;
            }

            for (var keyEntry in ApplicationStorageIDs) {
                if (ApplicationStorageIDs[keyEntry] && typeof ApplicationStorageIDs[keyEntry] === "number") {
                    var keyId: ApplicationStorageIDs = <ApplicationStorageIDs>Number(ApplicationStorageIDs[keyEntry]);
                    var storageKey: string = ApplicationStorage.getStorageKeyFromId(keyId);

                    var currentStoredValue: string = currentStorage.getItem(storageKey);

                    // skip keys bound to local storage and those not set on currentStorage
                    if (!ApplicationStorage.isKeyBoundToLocalStorage(keyId) && !ApplicationStorage.isKeyBoundToSessionStorage(keyId)
                        && !ObjectExtensions.isNullOrUndefined(currentStoredValue)) {
                        currentStorage.removeItem(storageKey);
                        ApplicationStorage.setItemOnStorage(newStorage, storageKey, currentStoredValue);
                    }
                }
            }

            ApplicationStorage.useInMemoryStorage = newUseInMemoryStorage;
        }

        /**
         * Gets a value indicating whether is local storage supported.
         * @return {boolean} True if local storage supported, false otherwise.
         */
        public static isLocalStorageSupported(): boolean {
            var TEST_MARKER: string = "_test_value_0cacf36c-d73a-48bb-8d4d-0d611d83b1ef";
            try {
                window.localStorage.setItem(TEST_MARKER, TEST_MARKER);
            } catch (error) {
                return false;
            }

            try {
                window.localStorage.removeItem(TEST_MARKER);
            } catch (error) {
                return false;
            }

            return true;
        }

        /**
         * Gets the storage used for a specific key.
         * @param {ApplicationStorageIDs} itemKey the item key used to storage the item.
         * @returns {Storage} the storage provider to store the specific key.
         */
        private static getStorage(itemKey: ApplicationStorageIDs): Storage {
            ApplicationStorage.loadConfiguration();
            // by default, use current configured value - if configuration is not present, default to stricter setting (session storage)
            var mustUseSessionStorage: boolean = ObjectExtensions.isNullOrUndefined(ApplicationStorage.useInMemoryStorage)
                ? true
                : ApplicationStorage.useInMemoryStorage;

            // some keys must always go to local storage
            if (mustUseSessionStorage && ApplicationStorage.isKeyBoundToLocalStorage(itemKey)) {
                mustUseSessionStorage = false;
            }

            if (!mustUseSessionStorage && ApplicationStorage.isKeyBoundToSessionStorage(itemKey)) {
                mustUseSessionStorage = true;
            }

            return mustUseSessionStorage
                ? window.sessionStorage
                : window.localStorage;
        }

        /**
         * Gets a value indicating whether the itemKey must be stored on localStorage.
         * @param {ApplicationStorageIDs} itemKey the item key being checked.
         * @returns {boolean} a value indicating whether the itemKey must be storage on localStorage.
         */
        private static isKeyBoundToLocalStorage(itemKey: ApplicationStorageIDs): boolean {
            return ArrayExtensions.hasElement(ApplicationStorage.LOCALSTORAGE_BOUNDKEYS, itemKey);
        }

        private static isKeyBoundToSessionStorage(itemKey: ApplicationStorageIDs): boolean {
            return ArrayExtensions.hasElement(ApplicationStorage.SESSIONSTORAGE_BOUNDKEYS, itemKey);
        }

        /**
         * Gets a string representing the storage id key.
         * @param {ApplicationStorageIDs} itemKey the item key being checked.
         * @returns {string} a string representing the storage id key.
         */
        private static getStorageKeyFromId(storageKeyId: ApplicationStorageIDs): string {
            return <string>ApplicationStorageIDs[storageKeyId];
        }

        private static loadConfiguration(): void {
            // if this is the first time we try to access storage
            if (ApplicationStorage.useInMemoryStorage == null) {
                var deviceConfigurationStorageKey: string = ApplicationStorage.getStorageKeyFromId(ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY);
                // check for previous configuration stored on local storage
                var deviceConfiguration: Proxy.Entities.DeviceConfiguration =
                    JSON.parse(window.localStorage.getItem(deviceConfigurationStorageKey));

                if (deviceConfiguration != null && deviceConfiguration.UseInMemoryDeviceDataStorage != null) {
                    ApplicationStorage.useInMemoryStorage = deviceConfiguration.UseInMemoryDeviceDataStorage;
                }
            }
        }

        /**
         * Attempts to set the item on the given storage, and if the quota is reached attempt to free space using the registered non-essential data filters.
         * @param {Storage} storage The storage to set the item on.
         * @param {string} storageKey The key to use when setting the item.
         * @param {string} itemValue The value of the item to set.
         */
        private static setItemOnStorage(storage: Storage, storageKey: string, itemValue: string): void {
            try {
                storage.setItem(storageKey, itemValue);
            } catch (exception) {
                var itemSavedSuccessfully: boolean = false;
                if (exception.name === "QuotaExceededError" ||
                    exception.name === "NS_ERROR_DOM_QUOTA_REACHED") {
                    // Attempt to recover from storage quota exception by filtering out the non-essential data, and attempting to set the item again.
                    RetailLogger.coreApplicationStorageSetItemFailure(storageKey, exception.message);
                    try {
                        ApplicationStorage.nonEssentialDataFilterMap.forEach((key: string, dataFilter: NonEssentialDataFilter): void => {
                            var originalData: string;
                            var isKeyOfItemToSet: boolean = storageKey === key;
                            if (isKeyOfItemToSet) {
                                originalData = itemValue;
                            } else {
                                originalData = storage.getItem(key);
                            }

                            if (!ObjectExtensions.isNullOrUndefined(originalData)) {
                                var essentialData: string = dataFilter(originalData);
                                storage.setItem(key, essentialData);
                                // If this is the key of the item to set, mark that we have successfully set the item.
                                itemSavedSuccessfully = itemSavedSuccessfully || isKeyOfItemToSet;
                            }
                        });

                        // If the item wasn't saved during data filtering, then attempt to set the item after filtering the non-essential data.
                        if (!itemSavedSuccessfully) {
                            storage.setItem(storageKey, itemValue);
                            itemSavedSuccessfully = true;
                        }
                    } catch (recoveryException) {
                        RetailLogger.coreApplicationStorageSetItemFailureRecoveryUnsuccessful(storageKey, recoveryException.message);
                    }
                }

                // If we were not able to recover and save the item, propogate the exception.
                if (!itemSavedSuccessfully) {
                    throw exception;
                }
            }
        }
    }
}