/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */



///<reference path='Entities/CommerceTypes.g.ts'/>
///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>
///<reference path='Host/IHost.ts'/>
///<reference path='Managers/IManagerFactory.ts'/>
///<reference path='Managers/IChannelManager.ts'/>
///<reference path='Managers/ICustomerManager.ts'/>
///<reference path='Managers/IOperatorManager.ts'/>
///<reference path='Utilities/Dictionary.ts'/>
///<reference path='Utilities/ProductCatalogStoreHelper.ts'/>
///<reference path='IAsyncResult.ts'/>
///<reference path='TenderTypeMap.ts'/>
///<reference path='WinRT.d.ts'/>

module Commerce {
    "use strict";

    /**
     * List of async methods that are called during ApplicationContext initialization.
     * There is a limit of 31 methods that can be supported
     */
    export enum ApplicationContextEntitySet {
        None = 0,
        DeviceConfigurations = (1 << 2),
        CountryRegions = (1 << 3),
        Currencies = (1 << 4),
        Operations = (1 << 8),
        TenderTypes = (1 << 9),
        ChannelConfigurations = (1 << 10),
        HardwareProfile = (1 << 11),
        AvailableStores = (1 << 12),
        TillLayout = (1 << 13),
        UnitsOfMeasure = (1 << 14),
        DeliveryOptions = (1 << 15),
        EmployeeList = (1 << 17),
        PaymentMerchantInformation = (1 << 18),
        RetailTrialPlanOffer = (1 << 19),
        All = None | DeviceConfigurations | CountryRegions | Currencies
        | Operations | TenderTypes | ChannelConfigurations | HardwareProfile | AvailableStores
        | TillLayout | UnitsOfMeasure | DeliveryOptions | EmployeeList | PaymentMerchantInformation
        | RetailTrialPlanOffer
    }

    /**
     * Channel profile properties.
     */
    export enum ChannelProfileProperties {
        None = 0,
        RetailServerURL = 1,
        RichMediaBaseURL = 2,
        PrinterURL = 3,
        DrawerURL = 4,
        PaymentTerminalURL = 5,
        HardwareStationURL = 6
    }

    /**
     * Class for storing data that is shared across pages.
     */
    export class ApplicationContext {
        private static _instance: Commerce.ApplicationContext = null;

        public customerGroupsAsync: IAsyncResult<Model.Entities.CustomerGroup[]>;
        public customerTypesAsync: IAsyncResult<Model.Entities.ICustomerType[]>;
        public debitCashbackLimitAsync: IAsyncResult<number>;
        public cardTypesAsync: IAsyncResult<Model.Entities.CardTypeInfo[]>;
        public returnOrderReasonCodesAsCompositeSubcodesAsync: IAsyncResult<Model.Entities.ReasonCode>;
        public languagesAsync: IAsyncResult<Model.Entities.LanguagesInfo[]>;
        public receiptOptionsAsync: IAsyncResult<Model.Entities.ReceiptOption[]>;
        public cashDeclarationsMapAsync: IAsyncResult<Dictionary<Model.Entities.CashDeclaration[]>>;
        public hardwareStationProfileAsync: IAsyncResult<Model.Entities.HardwareStationProfile[]>;

        public availableStores: Dictionary<Model.Entities.OrgUnit>;
        public tillLayoutProxy: Model.Entities.TillLayoutProxy;
        public countriesIndexMap: Dictionary<number>;
        public channelConfiguration: Model.Entities.ChannelConfiguration;
        public channelRichMediaBaseURL: string;
        public currenciesMap: Dictionary<Model.Entities.Currency>;
        public deliveryOptionsMap: Dictionary<Model.Entities.DeliveryOption>;
        public storeInformation: Model.Entities.OrgUnit;
        public tenderTypesMap: TenderTypeMap;
        public retailTrialPlanOfferFlag: boolean;
        public unitsOfMeasureMap: Dictionary<Model.Entities.UnitOfMeasure>;
        public employeeList: Model.Entities.Employee[];
        public customUIStringsMap: Dictionary<Model.Entities.LocalizedString>;
        public operationPermissions: Model.Entities.OperationPermission[];
        public storeNumber: string;

        private _countries: Model.Entities.CountryRegionInfo[];
        private _currencies: Model.Entities.Currency[];
        private _deliveryOptions: Model.Entities.DeliveryOption[];
        private _tenderTypes: Model.Entities.TenderType[];
        private _unitsOfMeasure: Model.Entities.UnitOfMeasure[];
        private _storeDistancesMap: Dictionary<number>;
        private _customUIStrings: Model.Entities.LocalizedString[];
        private _retailTrialPlanOffer: boolean;
        private _deviceConfiguration: Model.Entities.DeviceConfiguration;
        private _hardwareProfile: Model.Entities.HardwareProfile;

        public get Countries(): Model.Entities.CountryRegionInfo[] {
            return this._countries;
        }

        public set Countries(newValue: Model.Entities.CountryRegionInfo[]) {
            this._countries = newValue.sort(
                (firstCountry: Model.Entities.CountryRegionInfo, secondCountry: Model.Entities.CountryRegionInfo) => {
                    return StringExtensions.compare(firstCountry.ShortName, secondCountry.ShortName, true);
                });

            this.countriesIndexMap.clear();
            for (var i: number = 0; i < this._countries.length; i++) {
                this.countriesIndexMap.setItem(this._countries[i].CountryRegionId, i);

                this._countries[i].AddressFormatLines = this._countries[i].AddressFormatLines.sort(
                    (a: Model.Entities.AddressFormattingInfo, b: Model.Entities.AddressFormattingInfo) => {
                        return a.LineNumber - b.LineNumber;
                    });
            }
        }

        /**
         * Gets a value indicating whether the device has is activated or not.
         */
        public get isDeviceActivated(): boolean {
            return !StringExtensions.isNullOrWhitespace(ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY));
        }

        public get Currencies(): Model.Entities.Currency[] {
            return this._currencies;
        }

        public set Currencies(newValue: Model.Entities.Currency[]) {
            this._currencies = newValue;
            this._currencies.forEach((value: Model.Entities.Currency) => {
                this.currenciesMap.setItem(value.CurrencyCode, value);
            });
        }

        public get deliveryOptions(): Model.Entities.DeliveryOption[] {
            return this._deliveryOptions;
        }

        public set deliveryOptions(newValue: Model.Entities.DeliveryOption[]) {
            this._deliveryOptions = newValue;
            this.deliveryOptionsMap.clear();
            this.deliveryOptionsMap.setItems(newValue, (deliveryOption: Model.Entities.DeliveryOption) => deliveryOption.Code);
        }

        public get customUIStrings(): Model.Entities.LocalizedString[] {
            return this._customUIStrings;
        }

        public set customUIStrings(newValue: Model.Entities.LocalizedString[]) {
            this._customUIStrings = newValue;
            this.customUIStringsMap.clear();
            this.customUIStringsMap.setItems(newValue, (customString: Model.Entities.LocalizedString) => "string_" +
                customString.TextId.toString());
        }

        public get tenderTypes(): Model.Entities.TenderType[] {
            return this._tenderTypes;
        }

        public set tenderTypes(newValue: Model.Entities.TenderType[]) {
            this._tenderTypes = newValue;
            this.tenderTypesMap.clear();
            this._tenderTypes.forEach((tenderType: Model.Entities.TenderType) => {
                if (!this.tenderTypesMap.hasItem(tenderType.OperationId)) {
                    this.tenderTypesMap.setItem(tenderType.OperationId, []);
                }

                this.tenderTypesMap.getItem(tenderType.OperationId).push(tenderType);
            });
        }

        public get unitsOfMeasure(): Model.Entities.UnitOfMeasure[] {
            return this._unitsOfMeasure;
        }

        public set unitsOfMeasure(newValue: Model.Entities.UnitOfMeasure[]) {
            this._unitsOfMeasure = newValue;
            this.unitsOfMeasureMap.clear();
            this._unitsOfMeasure.forEach((value: Model.Entities.UnitOfMeasure) => {
                this.unitsOfMeasureMap.setItem(value.Symbol.toLowerCase(), value);
            });
        }

        public get retailTrialPlanOffer(): boolean {
            return this._retailTrialPlanOffer;
        }

        public set retailTrialPlanOffer(newValue: boolean) {
            this._retailTrialPlanOffer = newValue;

            this.retailTrialPlanOfferFlag = this._retailTrialPlanOffer;
        }

        /**
         * Gets the dictionary that stores the distances between current store and other stores within the store locator group.
         * @returns {Dictionary<number>} The dictionary that stores the distances between current store and other stores
         * within the store locator group.
         */
        public get storeDistancesMap(): Dictionary<number> {
            return this._storeDistancesMap;
        }

        /**
         * Sets the dictionary that stores the distances between current store and other stores within the store locator group.
         * @param {Dictionary<number>} newValue The dictionary that stores the distances between current store and other stores
         * within the store locator group.
         */
        public set storeDistancesMap(newValue: Dictionary<number>) {
            this._storeDistancesMap = newValue;
        }

        /*
         * Gets the active eft terminal id. It can belong to a harware station profile if any is active and set.
         * Otherwise it's defaulted to register's configuration.
         */
        public get activeEftTerminalId(): string {

            var activeHardwareStation: Model.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();
            var activeEftTerminalId: string = null;
            if (!ObjectExtensions.isNullOrUndefined(activeHardwareStation)) {
                activeEftTerminalId = activeHardwareStation.EftTerminalId;
            }

            if (StringExtensions.isNullOrWhitespace(activeEftTerminalId)) {
                activeEftTerminalId = this.deviceConfiguration.EFTTerminalId;
            }
            return activeEftTerminalId;
        }

        /**
         * Constructor.
         */
        constructor() {
            this.availableStores = new Dictionary<Model.Entities.OrgUnit>();
            this.countriesIndexMap = new Dictionary<number>();
            this.currenciesMap = new Dictionary<Model.Entities.Currency>();
            this.deliveryOptionsMap = new Dictionary<Model.Entities.DeliveryOption>();
            this.tenderTypesMap = new TenderTypeMap();
            this.unitsOfMeasureMap = new Dictionary<Model.Entities.UnitOfMeasure>();
            this._storeDistancesMap = new Dictionary<number>();
            this.channelRichMediaBaseURL = StringExtensions.EMPTY;
            this.customUIStringsMap = new Dictionary<Model.Entities.LocalizedString>();
        }

        /**
         * Sets the device configuration.
         */
        public set deviceConfiguration(deviceConfiguration: Model.Entities.DeviceConfiguration) {
            this._deviceConfiguration = deviceConfiguration;
            ApplicationStorage.updateStorageConfiguration(deviceConfiguration);
        }

        /**
         * Sets the hardware profile
         */
        public set hardwareProfile(hardwareProfile: Model.Entities.HardwareProfile) {
            this._hardwareProfile = hardwareProfile;
        }

        /**
         * Gets the instance of ApplicationContext.
         */
        public static get Instance(): ApplicationContext {
            if (ObjectExtensions.isNullOrUndefined(ApplicationContext._instance)) {
                ApplicationContext._instance = new ApplicationContext();
            }

            return ApplicationContext._instance;
        }

        /**
         * Sets the instance of ApplicationContext.
         */
        public static set Instance(newContext: ApplicationContext) {
            ApplicationContext._instance = newContext;
        }

        /**
         * Gets the device configuration.
         */
        public get deviceConfiguration(): Model.Entities.DeviceConfiguration {
            if (this._deviceConfiguration == null) {
                var storedData: string = ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY);
                this._deviceConfiguration = new Model.Entities.DeviceConfigurationClass(JSON.parse(storedData));
            }

            return this._deviceConfiguration;
        }

        /**
         * Gets the hardware profile.
         */
        public get hardwareProfile(): Model.Entities.HardwareProfile {
            if (this._hardwareProfile == null) {
                var storedData: string = ApplicationStorage.getItem(ApplicationStorageIDs.HARDWARE_PROFILE_KEY);
                this._hardwareProfile = new Model.Entities.HardwareProfileClass(JSON.parse(storedData));
            }

            return this._hardwareProfile;
        }

        /**
         * Updates server URL in storage.
         * @param {string} newUrl The server URL.
         */
        public static updateServerUrl(newUrl: string): void {
            if (StringExtensions.compare(ApplicationStorage.getItem(ApplicationStorageIDs.RETAIL_SERVER_URL), newUrl) !== 0) {
                var managerFactory: Model.Managers.RetailServerManagerFactory =
                    <Model.Managers.RetailServerManagerFactory>Model.Managers.Factory;
                var serverUri: string = newUrl;

                ApplicationStorage.setItem(ApplicationStorageIDs.RETAIL_SERVER_URL, serverUri);
                Config.retailServerUrl = serverUri;

                managerFactory.updateServerUriInCommerceContext(serverUri);
            }
        }
    }

    /**
     * Class for loading the data that is shared across pages.
     */
    export class ApplicationContextLoader {
        public static _categories: Model.Entities.Category[] = [];
        private _tillLayoutManager: Commerce.Model.Managers.ITillLayoutManager;
        private _channelManager: Commerce.Model.Managers.IChannelManager;
        private _productManager: Commerce.Model.Managers.IProductManager;
        private _customerManager: Commerce.Model.Managers.ICustomerManager;
        private _operatorManager: Commerce.Model.Managers.IOperatorManager;

        // The result object to report the results for the load channel method
        private _loadChannelConfigurationAsyncResult: VoidAsyncResult;

        private _applicationContextEntitySetCompleted: ApplicationContextEntitySet;
        private _applicationContextEntitySetFailed: ApplicationContextEntitySet;
        private _applicationContextEntityErrors: Dictionary<Model.Entities.Error[]>; // Information on failed ApplicationContext methods

        /**
         * Constructor.
         */
        constructor() {
            this._tillLayoutManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.ITillLayoutManagerName, this);
            this._channelManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.IChannelManagerName, this);
            this._productManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.IProductManagerName, this);
            this._customerManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.ICustomerManagerName, this);
            this._operatorManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.IOperatorManagerName, this);
            this._applicationContextEntitySetCompleted = null;
            this._applicationContextEntitySetFailed = null;
            this._applicationContextEntityErrors = new Dictionary<Model.Entities.Error[]>();
        }

        /**
         * Incrementally loads all categories for the current store context
         */
        public static loadCategories(isRefresh: boolean): void {
            if (isRefresh) {
                Commerce.ApplicationContextLoader._categories = [];
            }

            var productManager: Commerce.Model.Managers.IProductManager = Commerce.Model.Managers.Factory.GetManager(
                Model.Managers.IProductManagerName, this);
            productManager.getCategoriesAsync(Session.instance.productCatalogStore.Context.ChannelId,
                ApplicationContextLoader._categories.length, Config.defaultPageSize)
                .done((result: Model.Entities.Category[]) => {
                    result.forEach((value: Model.Entities.Category) => {
                        Commerce.ApplicationContextLoader._categories.push(value);
                    });

                    if (result.length === 0) {
                        Commerce.Session.instance.CurrentCategoryList = Commerce.ApplicationContextLoader._categories;
                    } else {
                        this.loadCategories(false);
                    }
                }).fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextLoadCategoriesFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                });
        }

        /**
         * Gets the error for the action for the specified entity
         * @param {ApplicationContextEntitySet} entity The name of the entity that the action has failed.
         * @return {Model.Entities.Error[]} The error collection or null if the error does not exist,
         * the entity action passed, or it was not run.
         */
        public getErrorForEntitySet(entity: ApplicationContextEntitySet): Model.Entities.Error[] {
            if (!this.hasEntitySetFailed(entity) || !this._applicationContextEntityErrors.hasItem(entity)) {
                return null;
            }

            return this._applicationContextEntityErrors.getItem(entity);
        }

        /**
         * Gets the enumerated value indicating the sets that have failed
         * @return {ApplicationContextEntitySet} The enumerated value of the actions that have failed,
         * null if the actions have not been run.
         */
        public getFailedEntitySets(): ApplicationContextEntitySet {
            return this._applicationContextEntitySetFailed;
        }

        /**
         * Gets whether the actions specified entity has failed
         * @param {ApplicationContextEntitySet} entity The name of the entity that the action has failed on.
         * @return {boolean} True if the action on the entity has failed, false if the action has not been run or it succeeded.
         */
        public hasEntitySetFailed(entity: ApplicationContextEntitySet): boolean {
            return (this._applicationContextEntitySetFailed != null) && ((this._applicationContextEntitySetFailed & entity) === entity);
        }

        /**
         * Gets whether the actions specified entity has passed
         * @param {ApplicationContextEntitySet} entity The name of the entity.
         * @return {boolean} True if the action on the entity has passed, false if the action has not been run or it failed.
         */
        public hasEntitySetSucceeded(entity: ApplicationContextEntitySet): boolean {
            return (this._applicationContextEntitySetFailed != null) && ((this._applicationContextEntitySetFailed & entity) === 0);
        }

        /**
         * Loads default channel configuration.
         * @return {IVoidAsyncResult} The async result.
         */
        public loadChannelConfiguration(): IVoidAsyncResult {
            this._loadChannelConfigurationAsyncResult = new VoidAsyncResult();

            // Initialize the array tracking the methods that succeeded and failed
            this._applicationContextEntitySetCompleted = 0;
            this._applicationContextEntitySetFailed = 0;
            this._applicationContextEntityErrors.clear();
            this.setupAsyncEntities(ApplicationContext.Instance);

            new AsyncQueue().enqueue(() => {

                var result: VoidAsyncResult = new VoidAsyncResult();

                if (Host.instance.application.getApplicationType() !== Proxy.Entities.ApplicationTypeEnum.CloudPos) {
                    // the result of the call should not stop the queue
                    // from running that's why we always resolve the result.
                    this._channelManager.getEnvironmentConfiguration()
                        .done((environmentConfiguration: Model.Entities.EnvironmentConfiguration) => {
                            var config: string = JSON.stringify(environmentConfiguration);
                            TsLogging.LoggerBase.setInstrumentationKey(environmentConfiguration.ClientAppInsightsInstrumentationKey);
                            ApplicationStorage.setItem(ApplicationStorageIDs.ENVIRONMENT_CONFIGURATION_KEY, config);
                            RetailLogger.applicationLoadEnvironmentConfigurationServerLoadSucceeded(config);
                        }).fail((errors: Model.Entities.Error[]) => {
                            RetailLogger.applicationLoadEnvironmentConfigurationServerLoadFailed(ErrorHelper.formatErrorMessage(errors[0]));
                        }).always(() => {
                            result.resolve();
                        });
                } else {
                    result.resolve();
                }

                return result;
            }).enqueue(() => {
                // device configuration needs to be the first element to be downloaded
                // as it controls how the remaining elements are stored in local storage or in memory
                var getDeviceConfigurationResult: VoidAsyncResult = new VoidAsyncResult();
                var previousDeviceConfiguration: Proxy.Entities.DeviceConfiguration = ApplicationContext.Instance.deviceConfiguration;
                this._channelManager.getDeviceConfigurationAsync()
                    .done((deviceConfiguration: Proxy.Entities.DeviceConfiguration) => {
                        // Store the latest device configuration in local storage for before logon access.
                        try {
                            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_CONFIGURATION_KEY, JSON.stringify(deviceConfiguration));
                            ApplicationContext.Instance.deviceConfiguration = deviceConfiguration;
                            getDeviceConfigurationResult.resolve();
                        } catch (exception) {
                            var storageError: Proxy.Entities.Error =
                                new Proxy.Entities.Error(ErrorTypeEnum.APPLICATION_STORE_FAILED_TO_SAVE_DEVICE_CONFIGURATION);
                            getDeviceConfigurationResult.reject([storageError]);
                        }
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("DeviceConfiguration", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.CountryRegions, errors);
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.DeviceConfigurations, errors);
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.AvailableStores, errors);
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.HardwareProfile, errors);
                        getDeviceConfigurationResult.reject(errors);
                    });

                return getDeviceConfigurationResult.done((): void => {
                    // Initialize culture if no culture is set in deviceConfiguration.
                    if (StringExtensions.isNullOrWhitespace(ApplicationContext.Instance.deviceConfiguration.CultureName)) {
                        ApplicationContext.Instance.deviceConfiguration.CultureName = Commerce.ViewModelAdapter.getDefaultUILanguage();
                    }

                    ApplicationContext.Instance.storeNumber = ApplicationContext.Instance.deviceConfiguration.StoreNumber;
                    this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.DeviceConfigurations);

                    CSSHelpers.applyThemeAsync(ApplicationContext.Instance.deviceConfiguration, previousDeviceConfiguration);

                    // over-ride device configuration theme with developer mode preset, if applicable
                    CSSHelpers.setDeveloperModeThemeDefault();

                    this._tillLayoutManager.getTillLayoutAsync()
                        .done((tillLayout: Model.Entities.TillLayoutProxy) => {
                            ApplicationContext.Instance.tillLayoutProxy = tillLayout;
                            this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.TillLayout);
                        }).fail((errors: Model.Entities.Error[]) => {
                            RetailLogger.applicationLoadChannelConfigurationFailed("TillLayout", errors[0].ErrorCode,
                                ErrorHelper.formatErrorMessage(errors[0]));
                            this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.TillLayout, errors);
                        });

                    this._channelManager.getCountryRegionsAsync(ApplicationContext.Instance.deviceConfiguration.CultureName)
                        .done((countries: Model.Entities.CountryRegionInfo[]) => {
                            ApplicationContext.Instance.Countries = countries;
                            this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.CountryRegions);
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            RetailLogger.applicationLoadChannelConfigurationFailed("CountryRegions", errors[0].ErrorCode,
                                ErrorHelper.formatErrorMessage(errors[0]));
                            this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.CountryRegions, errors);
                        });

                    this.loadActiveHardwareStationProfileAsync().done(() => {
                        new Activities.SaveMerchantInformationActivity(ApplicationContext.Instance.hardwareProfile).execute()
                            .done(() => this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.PaymentMerchantInformation))
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.applicationLoadChannelConfigurationFailed("PaymentMerchantInformation",
                                    errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                                this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.PaymentMerchantInformation, errors);
                            });
                    });

                    this._channelManager.getAvailableStoresAsync()
                        .done((stores: Model.Entities.OrgUnit[]) => {
                            ApplicationContext.Instance.availableStores.setItems(
                                stores, (store: Model.Entities.OrgUnit) => store.OrgUnitNumber);
                            ApplicationContext.Instance.storeInformation =
                            ApplicationContext.Instance.availableStores.getItem(
                                ApplicationContext.Instance.deviceConfiguration.StoreNumber);
                            Commerce.Session.instance.productCatalogStore.Store =
                            <Model.Entities.OrgUnit>ObjectExtensions.clone(ApplicationContext.Instance.storeInformation);
                            Commerce.Session.instance.productCatalogStore.Context =
                            new Model.Entities.ProjectionDomainClass(
                                { ChannelId: ApplicationContext.Instance.storeInformation.RecordId, CatalogId: 0 });
                            Commerce.Session.instance.productCatalogStore.StoreType = Model.Entities.StoreButtonControlType.CurrentStore;
                            this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.AvailableStores);
                            Commerce.ApplicationContextLoader.loadCategories(true);
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.AvailableStores, errors);
                        });
                });
            }).enqueue(() => {
                this._channelManager.getDeliveryOptionsAsync()
                    .done((deliveryOptions: Model.Entities.DeliveryOption[]) => {
                        ApplicationContext.Instance.deliveryOptions = deliveryOptions;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.DeliveryOptions);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("DeliveryOptions", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.DeliveryOptions, errors);
                    });

                this._channelManager.getCurrenciesAsync()
                    .done((currencies: Model.Entities.Currency[]) => {
                        ApplicationContext.Instance.Currencies = currencies;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.Currencies);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("Currencies", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.Currencies, errors);
                    });

                this._channelManager.getOperationsAsync()
                    .done((availableOperations: Model.Entities.OperationPermission[]) => {
                        ApplicationContext.Instance.operationPermissions = availableOperations;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.Operations);
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("Operations", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.Operations, errors);
                    });

                this._channelManager.getTenderTypesAsync()
                    .done((tenderTypes: Model.Entities.TenderType[]) => {
                        ApplicationContext.Instance.tenderTypes = tenderTypes;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.TenderTypes);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("TenderTypes", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.TenderTypes, errors);
                    });

                this._channelManager.getUnitsOfMeasureAsync()
                    .done((unitsOfMeasure: Model.Entities.UnitOfMeasure[]) => {
                        ApplicationContext.Instance.unitsOfMeasure = unitsOfMeasure;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.UnitsOfMeasure);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("UnitsOfMeasure", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.UnitsOfMeasure, errors);
                    });

                this._channelManager.getRetailTrialPlanOfferAsync()
                    .done((retailtrialPlanOfferFlag: boolean) => {
                        ApplicationContext.Instance.retailTrialPlanOffer = retailtrialPlanOfferFlag;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.RetailTrialPlanOffer);
                    }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("RetailTrialPlanOffer", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.RetailTrialPlanOffer, errors);
                    });

                this._channelManager.getChannelConfigurationAsync()
                    .done((channelConfiguration: Model.Entities.ChannelConfiguration) => {
                        ApplicationContext.Instance.channelConfiguration = channelConfiguration;
                        var channelRichMediaBaseURLOption: Model.Entities.ChannelProfileProperty = ArrayExtensions.firstOrUndefined(
                            channelConfiguration.ProfileProperties,
                            (value: Model.Entities.ChannelProfileProperty) => value.Key === ChannelProfileProperties.RichMediaBaseURL);

                        if (!Commerce.StringExtensions.isEmptyOrWhitespace(channelConfiguration.CatalogDefaultImageTemplate)) {
                            try {
                                var xmlURL: any = $.parseXML(channelConfiguration.CatalogDefaultImageTemplate);
                                Commerce.Session.instance.defaultCatalogImageFormat = $(xmlURL).find("Url")[0].textContent;
                            } catch (err) {
                                RetailLogger.applicationContextInvalidCatalogImageFormat();
                            }
                        }

                        if (!ObjectExtensions.isNullOrUndefined(channelRichMediaBaseURLOption)) {
                            ApplicationContext.Instance.channelRichMediaBaseURL = (Commerce.Config.isDemoMode) ?
                                "ms-appx:///DemoMode/" : UrlHelper.formatBaseUrl(channelRichMediaBaseURLOption.Value);

                            if (Session.instance.connectionStatus === ConnectionStatusType.Online) {
                                if (!Core.RegularExpressionValidations.validateUrl(Commerce.Session.instance.picture())) {
                                    Commerce.Session.instance.picture(ApplicationContext.Instance.channelRichMediaBaseURL +
                                        Commerce.Session.instance.picture());
                                }
                            }
                        }

                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.ChannelConfigurations);
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("ChannelConfiguration", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.ChannelConfigurations, errors);
                    });

                this._operatorManager.getEmployeesAsync()
                    .done((employeeList: Model.Entities.Employee[]) => {
                        // Refresh the list of employees after logon
                        ApplicationStorage.setItem(ApplicationStorageIDs.EMPLOYEE_LIST_KEY, JSON.stringify(employeeList));
                        ApplicationContext.Instance.employeeList = employeeList;
                        this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.EmployeeList);
                    })
                    .fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.applicationLoadChannelConfigurationFailed("Employees", errors[0].ErrorCode,
                            ErrorHelper.formatErrorMessage(errors[0]));
                    });

                return VoidAsyncResult.createResolved();
            }).run();

            return this._loadChannelConfigurationAsyncResult;
        }

        /*
         * Loads an active hardware station profile into ApplicationContext.
         *
         * @param {Model.Entities.HardwareProfile} [hardwareProfile] The operation options.
         * @return {IVoidAsyncResult} The async void result returns to caller.
         */
        public loadActiveHardwareStationProfileAsync(hardwareProfile?: Proxy.Entities.HardwareProfile): IVoidAsyncResult {

            var hardwareAsyncQueue: AsyncQueue = new AsyncQueue();
            var currentHardwareProfile: Proxy.Entities.HardwareProfile = null;
            var hardwareStation: Proxy.Entities.IHardwareStation = HardwareStationEndpointStorage.getActiveHardwareStation();

            if (!ObjectExtensions.isNullOrUndefined(hardwareStation)) {
                hardwareAsyncQueue.enqueue(() => {
                    return Commerce.ApplicationContext.Instance.hardwareStationProfileAsync.done(
                        (hardwareStationProfiles: Proxy.Entities.HardwareStationProfile[]) => {
                            var foundHardwareStations: Proxy.Entities.HardwareStationProfile[] = null;
                            foundHardwareStations = hardwareStationProfiles.filter((profile: Proxy.Entities.HardwareStationProfile) => {
                                return profile.RecordId === hardwareStation.RecordId;
                            });

                            if (ObjectExtensions.isNullOrUndefined(foundHardwareStations)
                                || foundHardwareStations.length === 0
                                || StringExtensions.compare(
                                    hardwareStation.Url,
                                    Peripherals.HardwareStation.HardwareStationContext.getHardwareStationUrlFromProfile(foundHardwareStations[0]),
                                    true) !== 0) {
                                // 1. Didn't find activated hardware station in profiles from channel database.
                                // 2. Hardware station URL has changed. 
                                // Either case, remove the active hardware station. The user has to select and pair again.
                                HardwareStationEndpointStorage.clearActiveHardwareStation();
                                hardwareStation = null;
                            } else {
                                // Find a matching hardware station, update the active hardware station with the latest settings.
                                hardwareStation.ProfileId = foundHardwareStations[0].HardwareProfileId;
                                hardwareStation.EftTerminalId = foundHardwareStations[0].HardwareStationEftTerminalId;
                                hardwareStation.HardwareConfigurations = foundHardwareStations[0].HardwareConfigurations;
                                HardwareStationEndpointStorage.setActiveHardwareStation(hardwareStation);
                            }
                        });
                });
            }

            hardwareAsyncQueue.enqueue(() => {
                if (ObjectExtensions.isNullOrUndefined(hardwareProfile)) {
                    return this.getActiveHardwareProfileAsync(hardwareStation)
                        .done((profile: Proxy.Entities.HardwareProfile) => { currentHardwareProfile = profile; });
                } else {
                    currentHardwareProfile = hardwareProfile;
                    return VoidAsyncResult.createResolved();
                }
            }).enqueue(() => {
                ApplicationStorage.setItem(ApplicationStorageIDs.HARDWARE_PROFILE_KEY, JSON.stringify(currentHardwareProfile));
                ApplicationContext.Instance.hardwareProfile = currentHardwareProfile;
                this.applicationContextEntitySetCallSuccessful(ApplicationContextEntitySet.HardwareProfile);
                return VoidAsyncResult.createResolved();
            });

            return hardwareAsyncQueue.run();
        }


        /*
         * Gets active hardware profile into ApplicationContext. 
         * The profile can be either a profile of an active hardware station (if exists) or a register's profile.
         *
         * @param {Proxy.Entities.HardwareProfile} [hardwareProfile] The operation options.
         * @return {IVoidAsyncResult} The async void result returns to caller.
         */
        public getActiveHardwareProfileAsync(hardwareStation: Proxy.Entities.IHardwareStation): IAsyncResult<Proxy.Entities.HardwareProfile> {
            var hardwareProfileId: string = this.getHardwareProfileId(hardwareStation);
            var currentHardwareProfile: Proxy.Entities.HardwareProfile = null;

            return this._channelManager.getHardwareProfileAsync(hardwareProfileId)
                .done((hardwareProfile: Proxy.Entities.HardwareProfile) => {
                    currentHardwareProfile = hardwareProfile;
                }).recoverOnFailure((errors: Proxy.Entities.Error[]) => {
                    // Try and use the application storage value, before throwing exception
                    currentHardwareProfile = JSON.parse(ApplicationStorage.getItem(ApplicationStorageIDs.HARDWARE_PROFILE_KEY));
                    if (currentHardwareProfile == null) {
                        this.applicationContextEntitySetCallFailed(ApplicationContextEntitySet.HardwareProfile, errors);
                        return AsyncResult.createRejected(errors);
                    }
                    return AsyncResult.createResolved(currentHardwareProfile);
                });
        }

        private getHardwareProfileId(hardwareStation: Proxy.Entities.IHardwareStation): string {

            var profileId: string = null;
            if (!ObjectExtensions.isNullOrUndefined(hardwareStation)) {
                if (!StringExtensions.isNullOrWhitespace(hardwareStation.ProfileId)) {
                    profileId = hardwareStation.ProfileId;
                }
            }
            return profileId || ApplicationContext.Instance.deviceConfiguration.HardwareProfile;
        }

        private setupAsyncEntities(context: ApplicationContext): void {
            var result: IAsyncResult<Model.Entities.CardTypeInfo[]> = this._channelManager.getCardTypesAsync();
            context.cardTypesAsync = this.setupCardTypesAsync(result);
            context.debitCashbackLimitAsync = this.setupDebitCashbackLimitAsync(result);
            context.hardwareStationProfileAsync = this.setupHardwareStationProfileAsync();
            context.customerGroupsAsync = this.setupCustomerGroupsAsync();
            context.customerTypesAsync = this.setupCustomerTypesAsync();
            context.returnOrderReasonCodesAsCompositeSubcodesAsync = this.setupReturnOrderReasonCodesAsCompositeSubcodesAsync();
            context.languagesAsync = this.setupLanguagesAsync();
            context.receiptOptionsAsync = this.setupReceiptOptionsAsync();
            context.cashDeclarationsMapAsync = this.setupCashDeclarationsMapAsync();
        }

        private setupDebitCashbackLimitAsync(result: IAsyncResult<Model.Entities.CardTypeInfo[]>): IAsyncResult<number> {
            var debitCashbackLimitAsync: AsyncResult<number> = new AsyncResult<number>();
            result
                .done((cardTypes: Model.Entities.CardTypeInfo[]) => {
                    var debitCashBackLimit: number = Infinity;

                    for (var i: number = 0; i < cardTypes.length; i++) {
                        // Debit cashback limit is set to the minimum of cashback limits of all debit card types.
                        if ((cardTypes[i].CardTypeValue === Model.Entities.CardType.InternationalDebitCard)
                            && (cardTypes[i].CashBackLimit < debitCashBackLimit)) {
                            debitCashBackLimit = cardTypes[i].CashBackLimit;
                        }
                    }

                    // Set the debit cashback limit.
                    debitCashbackLimitAsync.resolve(debitCashBackLimit === Infinity ? 0 : debitCashBackLimit);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupDebitCashbackLimitFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    debitCashbackLimitAsync.reject(errors);
                });
            return debitCashbackLimitAsync;
        }

        private setupCardTypesAsync(result: IAsyncResult<Model.Entities.CardTypeInfo[]>): IAsyncResult<Model.Entities.CardTypeInfo[]> {
            var cardTypesAsync: AsyncResult<Model.Entities.CardTypeInfo[]> = new AsyncResult<Model.Entities.CardTypeInfo[]>();
            result
                .done((cardTypes: Model.Entities.CardTypeInfo[]) => {
                    // Set the cards types configured for the channel.
                    cardTypesAsync.resolve(cardTypes || []);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupCardTypesFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    cardTypesAsync.reject(errors);
                });
            return cardTypesAsync;
        }

        private setupReturnOrderReasonCodesAsCompositeSubcodesAsync(): IAsyncResult<Model.Entities.ReasonCode> {
            var returnOrderReasonCodesAsCompositeSubcodesAsync: AsyncResult<Model.Entities.ReasonCode> = new AsyncResult<Model.Entities.ReasonCode>();

            this._channelManager.getReturnOrderReasonCodesAsync()
                .done((availableReasonCodes: Model.Entities.ReasonCode[]) => {
                    var compositeCode: Commerce.Model.Entities.ReasonCode = null;
                    if (ArrayExtensions.hasElements(availableReasonCodes)) {
                        compositeCode = new Model.Entities.ReasonCodeClass();
                        compositeCode.ReasonCodeId = availableReasonCodes[0].ReasonCodeId;
                        compositeCode.Description = availableReasonCodes[0].Description;
                        compositeCode.InputTypeValue = Model.Entities.ReasonCodeInputTypeEnum.CompositeSubCodes;
                        compositeCode.Prompt = Commerce.ViewModelAdapter.getResourceString("string_1203");
                        compositeCode.ReasonSubCodes = [];

                        availableReasonCodes.forEach((reasonCode: Model.Entities.ReasonCode) => {
                            var subCode: Model.Entities.ReasonSubCode = new Model.Entities.ReasonSubCodeClass();
                            subCode.SubCodeId = reasonCode.ReasonCodeId;
                            subCode.Description = reasonCode.Description;
                            compositeCode.ReasonSubCodes.push(subCode);
                        });
                    }
                    returnOrderReasonCodesAsCompositeSubcodesAsync.resolve(compositeCode);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupReturnOrderReasonCodesFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    returnOrderReasonCodesAsCompositeSubcodesAsync.reject(errors);
                });
            return returnOrderReasonCodesAsCompositeSubcodesAsync;
        }

        private setupCustomerTypesAsync(): IAsyncResult<Model.Entities.ICustomerType[]> {
            return this._customerManager.getCustomerTypesAsync()
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupCustomerTypesFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                });
        }

        private setupCustomerGroupsAsync(): IAsyncResult<Model.Entities.CustomerGroup> {
            return this._customerManager.getCustomerGroupsAsync()
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupCustomerGroupsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                });
        }

        private setupHardwareStationProfileAsync(): IAsyncResult<Model.Entities.HardwareStationProfile[]> {
            return this._channelManager.getHardwareStationProfileAsync()
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupHardwareStationProfileFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                });
        }

        private setupLanguagesAsync(): IAsyncResult<Model.Entities.LanguagesInfo[]> {
            var languagesAsync: AsyncResult<Model.Entities.LanguagesInfo[]> = new AsyncResult<Model.Entities.LanguagesInfo[]>();
            this._channelManager.getLanguagesAsync()
                .done((availableLanguages: Model.Entities.SupportedLanguage[]) => {
                    var languages: Model.Entities.LanguagesInfo[] = [];
                    availableLanguages.forEach((value: Model.Entities.SupportedLanguage) => {
                        var isLanguageFound: boolean;
                        var language: Commerce.Host.ILanguage;

                        try {
                            language = Commerce.Host.instance.globalization.getLanguageByTag(value.LanguageId);
                            isLanguageFound = !StringExtensions.isNullOrWhitespace(language.displayName);
                        } catch (err) {
                            isLanguageFound = false;
                        }

                        if (isLanguageFound) {
                            languages.push({
                                LanguageName: language.displayName,
                                LanguageId: value.LanguageId
                            });
                        } else {
                            RetailLogger.applicationContextSetupLanguagesInvalidLanguage(value.LanguageId);
                            return false;
                        }
                    });

                    languagesAsync.resolve(languages.sort((left: Model.Entities.LanguagesInfo, right: Model.Entities.LanguagesInfo) => {
                        return StringExtensions.compare(left.LanguageName, right.LanguageName);
                    }));
                }).fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupLanguagesFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    languagesAsync.reject(errors);
                });
            return languagesAsync;
        }

        private setupReceiptOptionsAsync(): IAsyncResult<Model.Entities.ReceiptOption[]> {
            var result: AsyncResult<Model.Entities.ReceiptOption[]> = new AsyncResult<Model.Entities.ReceiptOption[]>();
            this._customerManager.getAllReceiptOptionsAsync()
                .done((options: Model.Entities.ReceiptOption[]) => {
                    options.forEach((receiptOption: Model.Entities.ReceiptOption) => {
                        receiptOption.Description = Commerce.ViewModelAdapter.getResourceString(receiptOption.ResourceString);
                    });
                    result.resolve(options);
                })
                .fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupReceiptOptionsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    result.reject(errors);
                });
            return result;
        }

        private setupCashDeclarationsMapAsync(): IAsyncResult<Dictionary<Model.Entities.CashDeclaration[]>> {

            var result: AsyncResult<Dictionary<Model.Entities.CashDeclaration[]>> = new AsyncResult<Dictionary<Model.Entities.CashDeclaration[]>>();

            this._channelManager.getCashDeclarationAsync()
                .done((cashDeclarations: Model.Entities.CashDeclaration[]) => {
                    var cashDeclarationsMap: Dictionary<Model.Entities.CashDeclaration[]> = new Dictionary<Model.Entities.CashDeclaration[]>();
                    cashDeclarations.forEach((value: Model.Entities.CashDeclaration) => {
                        if (!cashDeclarationsMap.hasItem(value.Currency)) {
                            cashDeclarationsMap.setItem(value.Currency, []);
                        }
                        cashDeclarationsMap.getItem(value.Currency).push(value);
                    });
                    result.resolve(cashDeclarationsMap);
                }).fail((errors: Model.Entities.Error[]) => {
                    RetailLogger.applicationContextSetupCashDeclarationsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    result.reject(errors);
                });
            return result;
        }

        /**
         * Tracks the tasks that succeeds and does any callbacks if all tasks have completed
         * @param {ApplicationContextEntitySet} entity The name of the entity that the action has failed on.
         */
        private applicationContextEntitySetCallSuccessful(entity: ApplicationContextEntitySet): void {
            this.actionOnContextEntitySetCompletion(entity);
        }

        /**
         * Tracks the tasks that failed and does any callbacks if all tasks have completed
         * @param {ApplicationContextEntitySet} entity The name of the entity that the action has failed on.
         * @param {Model.Entities.Error[]} [errors] The error.
         */
        private applicationContextEntitySetCallFailed(entity: ApplicationContextEntitySet, errors?: Model.Entities.Error[]): void {
            this._applicationContextEntitySetFailed = this._applicationContextEntitySetFailed | entity;
            this._applicationContextEntityErrors.setItem(entity, errors);
            this.actionOnContextEntitySetCompletion(entity);
        }

        /**
         * Actions to take if all the methods have completed. Will not do any actions if there are methods still running.
         * @param {ApplicationContextEntitySet} entity The name of the entity that the action has failed on.
         */
        private actionOnContextEntitySetCompletion(entity: ApplicationContextEntitySet): void {
            if (entity === ApplicationContextEntitySet.None) {
                RetailLogger.applicationContextApplicationContextEntitySetInvalid(ApplicationContextEntitySet[entity]);
            } else if ((this._applicationContextEntitySetCompleted & entity) === entity) {
                RetailLogger.applicationContextApplicationContextEntitySetMultipleTimes(ApplicationContextEntitySet[entity]);
            }

            this._applicationContextEntitySetCompleted = this._applicationContextEntitySetCompleted | entity;

            if (this._applicationContextEntitySetCompleted === ApplicationContextEntitySet.All) {
                if (this._applicationContextEntitySetFailed !== ApplicationContextEntitySet.None) {
                    this._loadChannelConfigurationAsyncResult.reject(
                        [new Model.Entities.Error(ErrorTypeEnum.APPLICATION_STORE_INITIALIZATION_DATA_FAILED_TO_LOAD)]);
                } else {
                    this._loadChannelConfigurationAsyncResult.resolve();
                }
            } else if (this._applicationContextEntitySetCompleted > ApplicationContextEntitySet.All) {
                RetailLogger.applicationContextApplicationContextEntitySetNoMethodNumber();
            }
        }
    }
}
