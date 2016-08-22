/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path="../Managers/IAsyncServiceManager.ts" />
/// <reference path="../Managers/IAuthenticationManager.ts" />
/// <reference path="../Managers/ICartManager.ts" />
/// <reference path="../Managers/IChannelManager.ts" />
/// <reference path="../Managers/ICustomerManager.ts" />
/// <reference path="../Managers/IInventoryManager.ts" />
/// <reference path="../Managers/IManagerFactory.ts" />
/// <reference path="../Managers/IOperatorManager.ts" />
/// <reference path="../Managers/IPaymentManager.ts" />
/// <reference path="../Managers/IProductManager.ts" />
/// <reference path="../Managers/IReportManager.ts" />
/// <reference path="../Managers/ISalesOrderManager.ts" />
/// <reference path="../Managers/IStockCountJournalManager.ts" />
/// <reference path="../Managers/IStoreoperationsManager.ts" />
/// <reference path="../Managers/ITillLayoutManager.ts" />

module Commerce.Operations {
    "use strict";

    /**
     * Provide common properties and functions for the operation handlers.
     */
    export class OperationHandlerBase implements IOperationHandler {
        /**
         * Initializes a new instance of the OperationHandlerBase class.
         */
        constructor() {
            // this means that no new properties can be added to the operation handler
            Object.freeze(this);
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
        public get asyncServiceManager(): Model.Managers.IAsyncServiceManager {
            return Model.Managers.Factory.getManager<Model.Managers.IAsyncServiceManager>(Model.Managers.IAsyncServiceManagerName);
        }

        /**
         * Gets the authentication manager.
         */
        public get authenticationManager(): Model.Managers.IAuthenticationManager {
            return Model.Managers.Factory.getManager<Model.Managers.IAuthenticationManager>(Model.Managers.IAuthenticationManagerName);
        }

        /**
         * Gets the cart manager.
         */
        public get cartManager(): Model.Managers.ICartManager {
            return Model.Managers.Factory.getManager<Model.Managers.ICartManager>(Model.Managers.ICartManagerName);
        }

        /**
         * Gets the channel manager.
         */
        public get channelManager(): Model.Managers.IChannelManager {
            return Model.Managers.Factory.getManager<Model.Managers.IChannelManager>(Model.Managers.IChannelManagerName);
        }

        /**
         * Gets the customer manager.
         */
        public get customerManager(): Model.Managers.ICustomerManager {
            return Model.Managers.Factory.getManager<Model.Managers.ICustomerManager>(Model.Managers.ICustomerManagerName);
        }

        /**
         * Gets the inventory manager.
         */
        public get inventoryManager(): Model.Managers.IInventoryManager {
            return Model.Managers.Factory.getManager<Model.Managers.IInventoryManager>(Model.Managers.IInventoryManagerName);
        }

        /**
         * Gets the operator manager.
         */
        public get operatorManager(): Model.Managers.IOperatorManager {
            return Model.Managers.Factory.getManager<Model.Managers.IOperatorManager>(Model.Managers.IOperatorManagerName);
        }

        /**
         * Gets the operations manager.
         */
        public get operationsManager(): OperationsManager {
            return Operations.OperationsManager.instance;
        }

        /**
         * Gets the payment manager.
         */
        public get paymentManager(): Model.Managers.IPaymentManager {
            return Model.Managers.Factory.getManager<Model.Managers.IPaymentManager>(Model.Managers.IPaymentManagerName);
        }

        /**
         * Gets the product manager.
         */
        public get productManager(): Model.Managers.IProductManager {
            return Model.Managers.Factory.getManager<Model.Managers.IProductManager>(Model.Managers.IProductManagerName);
        }

        /**
         * Gets the report manager.
         */
        public get reportManager(): Model.Managers.IReportManager {
            return Model.Managers.Factory.getManager<Model.Managers.IReportManager>(Model.Managers.IReportManagerName);
        }

        /**
         * Gets the sales order manager.
         */
        public get salesOrderManager(): Model.Managers.ISalesOrderManager {
            return Model.Managers.Factory.getManager<Model.Managers.ISalesOrderManager>(Model.Managers.ISalesOrderManagerName);
        }

        /**
         * Gets the stock count journal manager.
         */
        public get stockCountJournalManager(): Model.Managers.IStockCountJournalManager {
            return Model.Managers.Factory.getManager<Model.Managers.IStockCountJournalManager>(Model.Managers.IStockCountJournalManagerName);
        }

        /**
         * Gets the store operations manager.
         */
        public get storeOperationsManager(): Model.Managers.IStoreOperationsManager {
            return Model.Managers.Factory.getManager<Model.Managers.IStoreOperationsManager>(Model.Managers.IStoreOperationsManagerName);
        }

        /**
         * Gets the TillLayout manager.
         */
        public get tillLayoutManager(): Model.Managers.ITillLayoutManager {
            return Model.Managers.Factory.getManager<Model.Managers.ITillLayoutManager>(Model.Managers.ITillLayoutManagerName);
        }

        /**
         * Executes the operation.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         * @remarks The default implementation throws an exception.
         */
        public execute(options: IOperationOptions): IAsyncResult<IOperationResult> {
            throw "Operation handler not implemented.";
        }
    }
}