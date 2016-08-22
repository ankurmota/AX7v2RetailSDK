/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Model.Managers {
    "use strict";

    import Common = Proxy.Common;
    import Context = Proxy.Context;
    import Requests = Proxy.Requests;

    export class RetailServerManagerFactory implements Commerce.Model.Managers.IManagerFactory {

        protected _commerceContext: Proxy.CommerceContext;
        private commerceAuthenticationContext: Context.CommerceAuthenticationContext;
        private _odataEndpoint: string;
        private _locale: string;

        constructor(retailServerUrl: string, locale: string) {

            this._odataEndpoint = StringExtensions.CleanUri(retailServerUrl);
            this._locale = locale;

            this._commerceContext = new Proxy.CommerceContext(this.createDataServiceFactory(false));
            this.commerceAuthenticationContext = new Context.CommerceAuthenticationContext(this.createDataServiceFactory(true));
        }

        /**
         * Updates the server Uri of the commerce context.
         * @param {string} serverUri The new URI.
         */
        updateServerUriInCommerceContext(retailServerUri: string): void {
            this._odataEndpoint = StringExtensions.CleanUri(retailServerUri);
            this._commerceContext.dataServiceRequestFactory = this.createDataServiceFactory(false);
            this.commerceAuthenticationContext = new Context.CommerceAuthenticationContext(this.createDataServiceFactory(true));
        }

        /**
         * Updates the locale of the context.
         * @param {string} locale The new locale.
         */
        updateContextLocale(locale: string): void {
            this._locale = locale;
            this._commerceContext.dataServiceRequestFactory = this.createDataServiceFactory(false);
        }

        /**
         * Creates an instance of given entity manager.
         * @param {string} entityManagerInterface The interface name.
         */
        public getManager<T>(entityManagerInterface: string): T {
            return <T>this.GetManager(entityManagerInterface);
        }

        /**
         * Creates an instance of given entity manager.
         * @param {string} entityManagerInterface The interface name.
         * @param {any} [callerContext] The optional reference to caller object.
         */
        public GetManager(entityManagerInterface: string, callerContext?: any): any {
            var dataManager: any;

            switch (entityManagerInterface) {
                case Commerce.Model.Managers.IAsyncServiceManagerName:
                    dataManager = new RetailServer.AsyncServiceManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IAuthenticationManagerName:
                    dataManager = new RetailServer.AuthenticateManager(this._commerceContext, this.commerceAuthenticationContext);
                    break;

                case Commerce.Model.Managers.ICartManagerName:
                    dataManager = new RetailServer.CartManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IChannelManagerName:
                    dataManager = new RetailServer.ChannelManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.ICustomerManagerName:
                    dataManager = new RetailServer.CustomerManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IInventoryManagerName:
                    dataManager = new RetailServer.InventoryManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IOperatorManagerName:
                    dataManager = new RetailServer.OperatorManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IPaymentManagerName:
                    dataManager = new RetailServer.PaymentManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IProductManagerName:
                    dataManager = new RetailServer.ProductManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IRecordingManagerName:
                    dataManager = new RetailServer.RecordingManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IReportManagerName:
                    dataManager = new RetailServer.ReportManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.ISalesOrderManagerName:
                    dataManager = new RetailServer.SalesOrderManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IStockCountJournalManagerName:
                    dataManager = new RetailServer.StockCountJournalManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.IStoreOperationsManagerName:
                    dataManager = new RetailServer.StoreOperationsManager(this._commerceContext);
                    break;

                case Commerce.Model.Managers.ITillLayoutManagerName:
                    dataManager = new RetailServer.TillLayoutManager(this._commerceContext);
                    break;

                default:
                    throw entityManagerInterface + " is not supported.";
            }

            return dataManager;
        }

        /**
         * Toggles the data source connection.
         * @return {IVoidAsyncResult} The async result.
         */
        public toggleConnection(): IVoidAsyncResult {
            var chainedRequestFactory: Requests.ChainedRequestFactory = <Requests.ChainedRequestFactory>this._commerceContext.dataServiceRequestFactory;

            // Check we are using chained context.
            if (chainedRequestFactory.switchConnection) {
                var newConnectionStatus: ConnectionStatusType;

                if (Session.instance.connectionStatus === ConnectionStatusType.Online) {
                    newConnectionStatus = ConnectionStatusType.ManualOffline;
                } else {
                    newConnectionStatus = ConnectionStatusType.Online;
                }

                var asyncQueue: AsyncQueue = new AsyncQueue().enqueue((): IAsyncResult<any> => {
                    return chainedRequestFactory.switchConnection(newConnectionStatus, true);
                });

                return asyncQueue.run();

            } else {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(
                    ErrorTypeEnum.CANNOT_SWITCH_OFFLINE_NOT_AVAILABLE)]);
            }
        }

        private createDataServiceFactory(userAuthenticationFactory: boolean): Common.IDataServiceRequestFactory {
            var architectureType: Proxy.Entities.ArchitectureType = Host.instance.application.getArchitectureType();

            RetailLogger.modelManagersRetailServerManagerFactoryCreate(Proxy.Entities.ArchitectureType[architectureType]);

            var onlineConfigurationName: string = "online";
            var offlineConfigurationName: string = "offline";
            var dataServiceRequestFactory: Common.IDataServiceRequestFactory;

            switch (architectureType) {
                case Entities.ArchitectureType.X86:

                    // Offline supported in x86 app only.
                    var onlineDataServiceRequestFactory: Common.IDataServiceRequestFactory = null;
                    var offlineDataServiceRequestFactory: Common.IDataServiceRequestFactory = null;

                    // Construct data source chain for following supported scenarios.
                    // Retail Server Only
                    // Retail Server + Offline DB
                    // Online Database Only (Or Island mode)
                    // Online Database + Offline DB

                    if (!StringExtensions.isNullOrWhitespace(Config.onlineDatabase)) {
                        if (Microsoft.Dynamics.Commerce.ClientBroker.CommerceRuntimeRequest.tryAddConfiguration(
                            onlineConfigurationName,
                            Config.onlineDatabase,
                            true)) {
                            onlineDataServiceRequestFactory = new Requests.CommerceRuntimeRequestFactory("crt://" + onlineConfigurationName, this._locale);
                        }
                    }

                    if (!onlineDataServiceRequestFactory) {
                        onlineDataServiceRequestFactory = userAuthenticationFactory
                            ? new Requests.CommerceAuthenticationRequestFactory(this._odataEndpoint, this._locale)
                            : new Requests.RetailServerRequestFactory(this._odataEndpoint, this._locale);
                    }

                    if (!StringExtensions.isNullOrWhitespace(Config.offlineDatabase)) {
                        if (Microsoft.Dynamics.Commerce.ClientBroker.CommerceRuntimeRequest.tryAddConfiguration(
                            offlineConfigurationName,
                            Config.offlineDatabase,
                            false)) {
                            offlineDataServiceRequestFactory = new Requests.CommerceRuntimeRequestFactory("crt://" + offlineConfigurationName, this._locale);
                        }
                    }

                    if (onlineDataServiceRequestFactory && offlineDataServiceRequestFactory) {
                        dataServiceRequestFactory = new Requests.ChainedRequestFactory(onlineDataServiceRequestFactory, offlineDataServiceRequestFactory);
                        RetailLogger.modelManagersRetailServerManagerFactoryCreateChained();
                    } else {
                        dataServiceRequestFactory = onlineDataServiceRequestFactory;
                        RetailLogger.modelManagersRetailServerManagerFactoryCreateOnline();
                    }

                    break;

                default:
                    dataServiceRequestFactory = onlineDataServiceRequestFactory = userAuthenticationFactory
                        ? new Requests.CommerceAuthenticationRequestFactory(this._odataEndpoint, this._locale)
                        : new Requests.RetailServerRequestFactory(this._odataEndpoint, this._locale);
                    Session.instance.connectionStatus = ConnectionStatusType.Online;
                    break;

            }

            return dataServiceRequestFactory;
        }
    }
}