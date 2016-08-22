/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Commerce.Core.d.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Managers = Commerce.Model.Managers;

    /**
     * Provide common properties and functions for the view models.
     */
    export class ViewModelBase implements IDisposable {
        // Private members
        private _asyncServiceManager: Lazy<Managers.IAsyncServiceManager>;
        private _authenticationManager: Lazy<Managers.IAuthenticationManager>;
        private _cartManager: Lazy<Managers.ICartManager>;
        private _channelManager: Lazy<Managers.IChannelManager>;
        private _customerManager: Lazy<Managers.ICustomerManager>;
        private _storeOperationsManager: Lazy<Managers.IStoreOperationsManager>;
        private _inventoryManager: Lazy<Managers.IInventoryManager>;
        private _operatorManager: Lazy<Managers.IOperatorManager>;
        private _paymentManager: Lazy<Managers.IPaymentManager>;
        private _productManager: Lazy<Managers.IProductManager>;
        private _reportManager: Lazy<Managers.IReportManager>;
        private _salesOrderManager: Lazy<Managers.ISalesOrderManager>;
        private _stockCountJournalManager: Lazy<Managers.IStockCountJournalManager>;
        private _tillLayoutManager: Lazy<Managers.ITillLayoutManager>;

        /**
         * Instantiates an instance of the ViewModelBase class.
         */
        constructor() {
            // Initialize the managers' lazy initializers
            this._asyncServiceManager = new Lazy<Managers.IAsyncServiceManager>(() =>
                Managers.Factory.getManager<Managers.IAsyncServiceManager>(Managers.IAsyncServiceManagerName));

            this._authenticationManager = new Lazy<Managers.IAuthenticationManager>(() =>
                Managers.Factory.getManager<Managers.IAuthenticationManager>(Managers.IAuthenticationManagerName));

            this._cartManager = new Lazy<Managers.ICartManager>(() =>
                Managers.Factory.getManager<Managers.ICartManager>(Managers.ICartManagerName));

            this._channelManager = new Lazy<Managers.IChannelManager>(() =>
                Managers.Factory.getManager<Managers.IChannelManager>(Managers.IChannelManagerName));

            this._customerManager = new Lazy<Managers.ICustomerManager>(() =>
                Managers.Factory.getManager<Managers.ICustomerManager>(Managers.ICustomerManagerName));

            this._inventoryManager = new Lazy<Managers.IInventoryManager>(() =>
                Managers.Factory.getManager<Managers.IInventoryManager>(Managers.IInventoryManagerName));

            this._operatorManager = new Lazy<Managers.IOperatorManager>(() =>
                Managers.Factory.getManager<Managers.IOperatorManager>(Managers.IOperatorManagerName));

            this._paymentManager = new Lazy<Managers.IPaymentManager>(() =>
                Managers.Factory.getManager<Managers.IPaymentManager>(Managers.IPaymentManagerName));

            this._productManager = new Lazy<Managers.IProductManager>(() =>
                Managers.Factory.getManager<Managers.IProductManager>(Managers.IProductManagerName));

            this._reportManager = new Lazy<Managers.IReportManager>(() =>
                Managers.Factory.getManager<Managers.IReportManager>(Managers.IReportManagerName));

            this._salesOrderManager = new Lazy<Managers.ISalesOrderManager>(() =>
                Managers.Factory.getManager<Managers.ISalesOrderManager>(Managers.ISalesOrderManagerName));

            this._stockCountJournalManager = new Lazy<Managers.IStockCountJournalManager>(() =>
                Managers.Factory.getManager<Managers.IStockCountJournalManager>(Managers.IStockCountJournalManagerName));

            this._storeOperationsManager = new Lazy<Managers.IStoreOperationsManager>(() =>
                Managers.Factory.getManager<Managers.IStoreOperationsManager>(Managers.IStoreOperationsManagerName));

            this._tillLayoutManager = new Lazy<Managers.ITillLayoutManager>(() =>
                Managers.Factory.getManager<Managers.ITillLayoutManager>(Managers.ITillLayoutManagerName));
        }

        /**
         * Gets the application context instance.
         */
        public get applicationContext(): ApplicationContext {
            return ApplicationContext.Instance;
        }

        /**
         * Gets the async service manager.
         */
        public get asyncServiceManager(): Managers.IAsyncServiceManager {
            return this._asyncServiceManager.value;
        }

        /**
         * Gets the authentication manager.
         */
        public get authenticationManager(): Managers.IAuthenticationManager {
            return this._authenticationManager.value;
        }

        /**
         * Gets the cart manager.
         */
        public get cartManager(): Managers.ICartManager {
            return this._cartManager.value;
        }

        /**
         * Gets the channel manager.
         */
        public get channelManager(): Managers.IChannelManager {
            return this._channelManager.value;
        }

        /**
         * Gets the customer manager.
         */
        public get customerManager(): Managers.ICustomerManager {
            return this._customerManager.value;
        }

        /**
         * Gets the inventory manager.
         */
        public get inventoryManager(): Managers.IInventoryManager {
            return this._inventoryManager.value;
        }

        /**
         * Gets the operator manager.
         */
        public get operatorManager(): Managers.IOperatorManager {
            return this._operatorManager.value;
        }

        /**
         * Gets the operations manager.
         * @return {Operations.OperationsManager} The operations manager instance.
         */
        public get operationsManager(): Operations.OperationsManager {
            return Operations.OperationsManager.instance;
        }

        /**
         * Gets the payment manager.
         */
        public get paymentManager(): Managers.IPaymentManager {
            return this._paymentManager.value;
        }

        /**
         * Gets the product manager.
         */
        public get productManager(): Managers.IProductManager {
            return this._productManager.value;
        }

        /**
         * Gets the report manager.
         */
        public get reportManager(): Managers.IReportManager {
            return this._reportManager.value;
        }

        /**
         * Gets the sales order manager.
         */
        public get salesOrderManager(): Managers.ISalesOrderManager {
            return this._salesOrderManager.value;
        }

        /**
         * Gets the stock count journal manager.
         */
        public get stockCountJournalManager(): Managers.IStockCountJournalManager {
            return this._stockCountJournalManager.value;
        }

        /**
         * Gets the store operations manager.
         */
        public get storeOperationsManager(): Managers.IStoreOperationsManager {
            return this._storeOperationsManager.value;
        }

        /**
         * Gets the TillLayout manager.
         */
        public get tillLayoutManager(): Managers.ITillLayoutManager {
            return this._tillLayoutManager.value;
        }

        /**
         * Called when the resources need to be disposed.
         */
        public dispose(): void {
            ObjectExtensions.disposeAllProperties(this);
        }
    }
}