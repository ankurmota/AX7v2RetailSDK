/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../DataJS.d.ts'/>
///<reference path='../Tracer.ts'/>
///<reference path='../CommerceContext.g.ts'/>
///<reference path='../Interfaces/IDataServiceRequestFactory.ts'/>
///<reference path='../RetailServerRequestFactory.ts'/>
///<reference path='../XhrHelper.ts'/>

module Commerce.Proxy {
    "use strict";

    export class ManagerFactory implements Commerce.Proxy.IManagerFactory {

        private _commerceContext: Commerce.Proxy.CommerceContext;
        private _odataEndpoint: string;
        private _operatingUnitNumber: string;
        private _authToken: string;
        private _locale: string;

        constructor(retailServerUrl: string, operatingUnitNumber?: string, authToken?: string, locale: string = "") {

            this._odataEndpoint = StringExtensions.CleanUri(retailServerUrl);
            this._operatingUnitNumber = operatingUnitNumber;
            this._authToken = authToken;
            this._locale = locale;

            XhrHelper.SetupAjaxParameters();
            XhrHelper.SetupODataParameters();

            this._commerceContext = new Commerce.Proxy.CommerceContext(this.createDataServiceFactory());
        }

        /**
         * Updates the server Uri of the commerce context.
         *
         * @param {string} serverUri The new URI.
         */
        updateServerUriInCommerceContext(retailServerUri: string) {
            this._odataEndpoint = StringExtensions.CleanUri(retailServerUri);
            this._commerceContext.dataServiceRequestFactory = this.createDataServiceFactory();
        }

        /**
         * Updates the locale of the commerce context.
         * @param {string} locale The new locale.
         */
        updateLocaleInCommerceContext(locale: string): void {
            this._locale = locale;
            this._commerceContext.dataServiceRequestFactory = this.createDataServiceFactory();
        }

        /**
         * Creates an instance of given entity manager.
         *
         * @param {string} entityManagerInterface The interface name.
         */
        public getManager<T>(entityManagerInterface: string): T {
            return <T>this.GetManager(entityManagerInterface);
        }

        /**
         * Creates an instance of given entity manager.
         *
         * @param {string} entityManagerInterface The interface name.
         * @param {any} [callerContext] The optional reference to caller object.
         */
        public GetManager(entityManagerInterface: string, callerContext?: any): any {
            var dataManager: any;

            switch (entityManagerInterface) {

                case Commerce.Proxy.ICartManagerName:
                    dataManager = new Commerce.Proxy.CartManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.ICustomerManagerName:
                    dataManager = new Commerce.Proxy.CustomerManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.IProductManagerName:
                    dataManager = new Commerce.Proxy.ProductManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.ISalesOrderManagerName:
                    dataManager = new Commerce.Proxy.SalesOrderManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.IStockCountJournalManagerName:
                    dataManager = new Commerce.Proxy.StockCountJournalManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.IStoreOperationsManagerName:
                    dataManager = new Commerce.Proxy.StoreOperationsManager(this._commerceContext, callerContext);
                    break;

                case Commerce.Proxy.IOrgUnitManagerName:
                    dataManager = new Commerce.Proxy.OrgUnitManager(this._commerceContext, callerContext);
                    break;

                default:
                    throw entityManagerInterface + " is not supported.";
            }

            return dataManager;
        }

        private createDataServiceFactory(): Common.IDataServiceRequestFactory {
            var onlineConfigurationName = "online";
            var offlineConfigurationName = "offline";
            var dataServiceRequestFactory: Common.IDataServiceRequestFactory;

            dataServiceRequestFactory = new Commerce.Proxy.RetailServerRequestFactory(this._odataEndpoint, this._operatingUnitNumber, this._authToken);
            dataServiceRequestFactory.locale = this._locale;

            return dataServiceRequestFactory;
        }
    }
}