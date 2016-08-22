/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='InventoryLookupViewModel.ts'/>
///<reference path='PriceCheckViewModel.ts'/>
///<reference path='PickingAndReceivingDetailsViewModel.ts'/>
///<reference path='SearchPickingAndReceivingViewModel.ts'/>
///<reference path='SearchStockCountViewModel.ts'/>
///<reference path='StockCountDetailsViewModel.ts'/>

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the PriceCheck operation.
     */
    export interface IPriceCheckOperationOptions extends IOperationOptions {
        priceCheckViewModel: Commerce.ViewModels.PriceCheckViewModel;
    }

    /**
     * Handler for the PriceCheck operation.
     */
    export class PriceCheckOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PriceCheck operation.
         *
         * @param {IPriceCheckOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPriceCheckOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { priceCheckViewModel: undefined };

            var asyncResult = new VoidAsyncResult();

            var priceCheckViewModel = options.priceCheckViewModel;
            var customerId: string = Commerce.Session.instance.cart.CustomerId;
            var product: Model.Entities.Product = priceCheckViewModel.product;
            var variantId: number = priceCheckViewModel.variantId();
            var inventDimId: string = "";
            var storeId: string = priceCheckViewModel.store().OrgUnitNumber;
            priceCheckViewModel.unitOfMeasure = product.Rules.DefaultUnitOfMeasure;

            if (ObjectExtensions.isNullOrUndefined(customerId)) {
                customerId = "";
            }

            if (product.IsMasterProduct) {
                inventDimId = ProductPropertiesHelper.getProperty(variantId, product, ProductPropertyNameEnum.InventoryDimensionId);
            }

            this.inventoryManager.getProductPricesAsync(priceCheckViewModel.productId(), inventDimId, priceCheckViewModel.barcodeId, customerId, priceCheckViewModel.unitOfMeasure, priceCheckViewModel.quantity)
                .done((productPrices: Model.Entities.ProductPrice[]) => {
                    if (productPrices.length === 0) {
                        asyncResult.reject(<Model.Entities.Error[]>[new Model.Entities.Error(Commerce.ErrorTypeEnum.NO_PRICECHECK_ON_PRODUCTS)]);
                    } else {
                        priceCheckViewModel.productPrice(productPrices[0].CustomerContextualPrice);
                        asyncResult.resolve();
                    }
                }).fail((error: Model.Entities.Error[]) => {
                    asyncResult.reject(error);
                });

            return asyncResult;
        }
    }

    /**
     * Options passed to the PickingAndReceiving operation.
     */
    export interface IPickingAndReceivingOperationOptions extends IOperationOptions {
        operationType: Model.Entities.PickingAndReceivingOperationType;
        viewModel: ViewModels.ViewModelBase;
        journalType: Model.Entities.PurchaseTransferOrderType;
        journalEntity: Model.Entities.PickingAndReceivingOrder;
    }

    /**
     * Handler for the PickingAndReceiving operation.
     */
    export class PickingAndReceivingOperationHandler extends OperationHandlerBase {
        /**
         * Executes the PickingAndReceiving operation.
         *
         * @param {IPickingAndReceivingOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IPickingAndReceivingOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { journalEntity: undefined, journalType: undefined, operationType: undefined, viewModel: undefined };

            var asyncResult = new VoidAsyncResult();

            var searchViewModel: ViewModels.SearchPickingAndReceivingViewModel = null;
            var detailsViewModel: ViewModels.PickingAndReceivingDetailsViewModel = null;

            if (options.operationType === Model.Entities.PickingAndReceivingOperationType.GetAllJournals) {

                searchViewModel = <ViewModels.SearchPickingAndReceivingViewModel>options.viewModel;

                this.inventoryManager.getPickAndReceiveOrdersAsync().done((orders: Commerce.Model.Entities.InventoryOrders) => {

                    var pickingAndReceivingOrders: Model.Entities.PickingAndReceivingOrder[];

                    var createJournal = Model.Entities.PickingAndReceivingOrderHelper.createJournal;

                    pickingAndReceivingOrders = orders.pickingLists.map((x) => createJournal(x.OrderTypeValue, x));
                    pickingAndReceivingOrders = pickingAndReceivingOrders.concat(orders.transferOrders.map((x) => createJournal(x.OrderTypeValue, x)));
                    pickingAndReceivingOrders = pickingAndReceivingOrders.concat(orders.purchaseOrders.map((x) => createJournal(x.OrderTypeValue, x)));

                    searchViewModel.allOrders = pickingAndReceivingOrders;
                    asyncResult.resolve();

                }).fail((errors: Model.Entities.Error[]) => {
                        RetailLogger.operationPickingAndReceivingGetAllOrdersFailed();
                        asyncResult.reject(errors);
                    });

            } else {
                detailsViewModel = <ViewModels.PickingAndReceivingDetailsViewModel>options.viewModel;
            }

            if (options.journalType === Model.Entities.PurchaseTransferOrderType.PurchaseOrder) {
                switch (options.operationType) {
                    case Model.Entities.PickingAndReceivingOperationType.Save:
                        var purchaseOrderJournal: Model.Entities.PurchaseOrder = Model.Entities.PickingAndReceivingOrderHelper.convertToCommerceTypes(options.journalEntity);
                        this.inventoryManager.updatePurchaseOrderAsync(purchaseOrderJournal)
                            .done((updatedPurchaseOrder: Model.Entities.PurchaseOrder) => {
                                detailsViewModel.updateJournal(asyncResult, Model.Entities.PickingAndReceivingOrderHelper.createJournal(options.journalType, updatedPurchaseOrder), false);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingUpdatePurchaseOrderFailed(purchaseOrderJournal.OrderId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.Commit:
                        this.inventoryManager.commitPurchaseOrderAsync(detailsViewModel.journalId)
                            .done(() => {
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingCommitPurchaseOrderFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetAllJournals: //this case is executed on the beginning of method.
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetJournalDetails:
                        this.inventoryManager.getPurchaseOrderDetailsAsync(detailsViewModel.journalId)
                            .done((purchaseOrder: Model.Entities.PurchaseOrder) => {
                                var pickAndReceiveOrder: Model.Entities.PickingAndReceivingPurchaseOrder = new Model.Entities.PickingAndReceivingPurchaseOrder(purchaseOrder);
                                detailsViewModel.updateJournal(asyncResult, pickAndReceiveOrder, true);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingGetPurchaseOrderFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                }
            } else if (options.journalType === Model.Entities.PurchaseTransferOrderType.TransferIn ||
                options.journalType === Model.Entities.PurchaseTransferOrderType.TransferOut) {

                switch (options.operationType) {
                    case Model.Entities.PickingAndReceivingOperationType.Save:
                        var transferOrderJournal: Model.Entities.TransferOrder = Model.Entities.PickingAndReceivingOrderHelper.convertToCommerceTypes(options.journalEntity);
                        this.inventoryManager.updateTransferOrderAsync(transferOrderJournal)
                            .done((updatedTransferOrder: Model.Entities.TransferOrder) => {
                                detailsViewModel.updateJournal(asyncResult, Model.Entities.PickingAndReceivingOrderHelper.createJournal(options.journalType, updatedTransferOrder), false);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingUpdateTransderOrderFailed(transferOrderJournal.OrderId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.Commit:
                        this.inventoryManager.commitTransferOrderAsync(detailsViewModel.journalId)
                            .done(() => {
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingCommitTransferOrderFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetAllJournals: //this case is executed on the beginning of method.
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetJournalDetails:
                        this.inventoryManager.getTransferOrderDetailsAsync(detailsViewModel.journalId)
                            .done((transferOrder: Model.Entities.TransferOrder) => {
                                detailsViewModel.updateJournal(asyncResult, Model.Entities.PickingAndReceivingOrderHelper.createJournal(options.journalType, transferOrder), true);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingGetTransferOrderFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                }
            } else if (options.journalType === Model.Entities.PurchaseTransferOrderType.PickingList) {

                switch (options.operationType) {
                    case Model.Entities.PickingAndReceivingOperationType.Save:
                        var pickingListJournal: Model.Entities.PickingList = Model.Entities.PickingAndReceivingOrderHelper.convertToCommerceTypes(options.journalEntity);
                        this.inventoryManager.updatePickingListAsync(pickingListJournal)
                            .done((updatedPickingList: Model.Entities.PickingList) => {
                                detailsViewModel.updateJournal(asyncResult, Model.Entities.PickingAndReceivingOrderHelper.createJournal(options.journalType, updatedPickingList), false);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingUpdatePickingListFailed(pickingListJournal.OrderId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.Commit:
                        this.inventoryManager.commitPickingListAsync(detailsViewModel.journalId)
                            .done(() => {
                                asyncResult.resolve();
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingCommitPickingListFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetAllJournals: //this case is executed on the beginning of method.
                        break;
                    case Model.Entities.PickingAndReceivingOperationType.GetJournalDetails:
                        detailsViewModel = <ViewModels.PickingAndReceivingDetailsViewModel>options.viewModel;

                        this.inventoryManager.getPickingListDetailsAsync(detailsViewModel.journalId)
                            .done((pickingList: Model.Entities.PickingList) => {
                                var pickAndReceiveOrder: Model.Entities.PickingAndReceivingPickingList = new Model.Entities.PickingAndReceivingPickingList(pickingList);
                                detailsViewModel.updateJournal(asyncResult, pickAndReceiveOrder, true);
                            })
                            .fail((errors: Model.Entities.Error[]) => {
                                RetailLogger.operationPickingAndReceivingGetPickingListFailed(detailsViewModel.journalId);
                                asyncResult.reject(errors);
                            });
                        break;
                }
            } else if (options.operationType != Model.Entities.PickingAndReceivingOperationType.GetAllJournals) {
                throw "Unknown journal type: " + options.journalType;
            }

            return asyncResult;
        }
    }

    /**
     * Options passed to the InventoryLookup operation.
     */
    export interface IInventoryLookupOperationOptions extends IOperationOptions {
        inventoryLookupViewModel: Commerce.ViewModels.InventoryLookupViewModel;
        variantId: number;
    }

    /**
     * Handler for the InventoryLookup operation.
     */
    export class InventoryLookupOperationHandler extends OperationHandlerBase {
        /**
         * Executes the InventoryLookup operation.
         *
         * @param {IInventoryLookupOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IInventoryLookupOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { inventoryLookupViewModel: undefined, variantId: undefined };

            return this.productManager.getAvailableInventoriesByProductListingAsync(options.variantId, options.inventoryLookupViewModel.product())
                .map((stores: Model.Entities.OrgUnitAvailability[]) => {
                    options.inventoryLookupViewModel.stores(stores);
                    return <IOperationResult>{ canceled: false, data: stores };
                });
        }
    }

    /**
     * Options passed to the StockCount operation.
     */
    export interface IStockCountOperationOptions extends IOperationOptions {
        operationType: Model.Entities.StockCountOperationType;
        viewModel: ViewModels.ViewModelBase;
        stockCountJournal: Model.Entities.StockCountJournal;
    }

    /**
     * Handler for the StockCount operation.
     */
    export class StockCountOperationHandler extends OperationHandlerBase {
        /**
         * Executes the StockCount operation.
         *
         * @param {IStockCountOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IStockCountOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { operationType: undefined, stockCountJournal: undefined, viewModel: undefined };

            var asyncResult = new VoidAsyncResult();
            var searchViewModel: ViewModels.SearchStockCountViewModel = <ViewModels.SearchStockCountViewModel>options.viewModel;
            var detailsViewModel: ViewModels.StockCountDetailsViewModel = <ViewModels.StockCountDetailsViewModel>options.viewModel;

            switch (options.operationType) {
                case Model.Entities.StockCountOperationType.Create:
                    this.stockCountJournalManager.createStockCountJournalAsync(options.stockCountJournal)
                        .done((journalCreated: Model.Entities.StockCountJournal) => {
                            detailsViewModel.updateJournal(asyncResult, journalCreated, true);
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        }
                        );
                    break;
                case Model.Entities.StockCountOperationType.Update:
                    this.stockCountJournalManager.updateStockCountJournalAsync(options.stockCountJournal)
                        .done((journalUpdated: Model.Entities.StockCountJournal) => {
                            detailsViewModel.updateJournal(asyncResult, journalUpdated, true);
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.Commit:
                    this.stockCountJournalManager.commitStockCountJournalAsync(options.stockCountJournal.JournalId)
                        .done(() => {
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.GetAll:
                    this.stockCountJournalManager.getStockCountJournalsAsync()
                        .done((journals: Model.Entities.StockCountJournal[]) => {
                            searchViewModel.allJournals = journals;
                            searchViewModel.stockCountJournals(searchViewModel.allJournals);
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.GetDetails:
                    this.stockCountJournalManager.getStockCountJournalDetailsAsync(detailsViewModel.journalId())
                        .done((journal: Model.Entities.StockCountJournal) => {
                            detailsViewModel.updateJournal(asyncResult, journal, true);
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.Delete:
                    this.stockCountJournalManager.deleteStockCountJournalsAsync(searchViewModel.selectedJournals.map((value, index, array) => { return value.JournalId }))
                        .done(() => {
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.SyncAll:
                    this.stockCountJournalManager.syncAllStockCountJournalsAsync()
                        .done((journals: Model.Entities.StockCountJournal[]) => {
                            searchViewModel.allJournals = journals;
                            searchViewModel.stockCountJournals(searchViewModel.allJournals);
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.SyncOne:
                    this.stockCountJournalManager.syncStockCountJournalAsync(options.stockCountJournal.JournalId)
                        .done((updatedJournalLines: Model.Entities.StockCountJournalTransaction[]) => {
                            var stockCountJournal: Model.Entities.StockCountJournal = new Model.Entities.StockCountJournalClass();
                            stockCountJournal.StockCountTransactionLines = updatedJournalLines;
                            detailsViewModel.updateJournal(asyncResult, stockCountJournal, false);
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                case Model.Entities.StockCountOperationType.RemoveProductLine:
                    var journalId: string = detailsViewModel.journalId();

                    if (StringExtensions.isNullOrWhitespace(journalId)) {
                        asyncResult.resolve();
                        break;
                    }

                    var index: number = detailsViewModel.currentProductIndex;
                    var productLine: Model.Entities.StockCountLine = detailsViewModel.productLines()[index];
                    this.stockCountJournalManager.deleteStockCountJournalTransactionAsync(
                        journalId, productLine.itemId, productLine.inventColorId, productLine.configId, productLine.inventSizeId, productLine.inventStyleId)
                        .done(() => {
                            asyncResult.resolve();
                        }).fail((errors: Model.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                    break;
                default:
                    throw "Unknown operation type: " + options.operationType + " on stock count";
                    break;
            }

            return asyncResult;
        }
    }
}