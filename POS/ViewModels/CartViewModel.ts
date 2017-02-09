/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    import Diagnostics = Microsoft.Dynamics.Diagnostics;
    import Entities = Proxy.Entities;
    import RetailOperation = Operations.RetailOperation;

    /**
     * Provides the typed interface for a process text result.
     */
    export interface IProcessTextResult {
        /**
         * Field indicating whether or not the cart was updated.
         */
        cartUpdated: boolean;

        /**
         * Field containing the product that was found, if any.
         * @remarks Found by matching a barcode or item id.
         */
        product?: Entities.SimpleProduct;

        /**
         * Field containing the product search results, if any.
         * @remarks Found by product text search.
         */
        productSearchResults?: Entities.ProductSearchResult[];

        /**
         * Field containing the customers that were found, if any.
         */
        customers?: Entities.GlobalCustomer[];
    }

    /**
     * Represents the cart view model.
     */
    export class CartViewModel extends ViewModelBase {
        public customer: Observable<Proxy.Entities.Customer>;
        public customerAddress: Observable<Proxy.Entities.Address>;
        public customerInvoiceAddress: Observable<Proxy.Entities.Address>;
        public customerBalance: Observable<Proxy.Entities.CustomerBalances>;
        public customerLoyaltyCards: Observable<Proxy.Entities.LoyaltyCard[]>;
        public cart: Observable<Proxy.Entities.Cart>;
        public cartItemsTotalCountString: Computed<string>;

        public customerFullName: Computed<string>;
        public depositPaid: Computed<number>;

        public originalCartLines: ObservableArray<Proxy.Entities.CartLine>;
        public cartLinesTotalCount: Computed<number>;
        public incomeExpenseAccountLines: ObservableArray<Proxy.Entities.IncomeExpenseLine>;
        public customerAccountDepositLines: ObservableArray<Proxy.Entities.CustomerAccountDepositLine>;

        public lastSalesTransaction: Observable<Proxy.Entities.SalesOrder>;

        public lastChangeAmount: Observable<number>;
        public lastTotalAmountPaid: Observable<number>;
        public lastAmountDue: Observable<number>;
        public salesPaymentDifference: Observable<number>;
        public changeTenderTypeName: Observable<string>;
        public lastCustomerOrderMode: Proxy.Entities.CustomerOrderMode;

        public tenderLines: ObservableArray<Proxy.Entities.CartTenderLineTenderType>;
        public tenderLinesTotalCount: Computed<number>;
        private tenderTypes: ObservableArray<Proxy.Entities.TenderType>;

        private _reasonCodesInCartByReasonCodeId: Dictionary<Entities.ReasonCode>;
        private _isTenderTypesLoaded: boolean;

        /**
         *  A value indicating whether change must be given manually.
         *  In case it's not valid, the checkout cannot happen before the user manually give change using a valid tender method.
         */
        private _isManualChangeRequired: boolean;

        public selectedCartLines: ObservableArray<Proxy.Entities.CartLine>;
        public selectedTenderLines: ObservableArray<Proxy.Entities.CartTenderLine>;

        public buttonGrid1: Observable<Proxy.Entities.ButtonGrid>;
        public buttonGrid2: Observable<Proxy.Entities.ButtonGrid>;
        public buttonGrid3: Observable<Proxy.Entities.ButtonGrid>;
        public buttonGrid4: Observable<Proxy.Entities.ButtonGrid>;
        public buttonGrid5: Observable<Proxy.Entities.ButtonGrid>;

        public customControls: ObservableArray<Proxy.Entities.CustomControl>;

        private _processTextWorkerQueue: AsyncWorkerQueue;

        constructor() {
            super();
            this._isManualChangeRequired = false;
            this.customer = ko.observable(new Proxy.Entities.CustomerClass());
            this.customerAddress = ko.observable(null);
            this.customerInvoiceAddress = ko.observable(null);
            this.customerBalance = ko.observable(new Proxy.Entities.CustomerBalancesClass());
            this.customerLoyaltyCards = ko.observable(null);

            this.customerAccountDepositLines = ko.observableArray([]);

            this.cart = ko.observable(Session.instance.cart);

            this.selectedCartLines = ko.observableArray([]);
            this.selectedTenderLines = ko.observableArray([]);

            this.lastSalesTransaction = ko.observable(new Proxy.Entities.SalesOrderClass());
            this.lastTotalAmountPaid = ko.observable(0);
            this.lastChangeAmount = ko.observable(0);
            this.lastAmountDue = ko.observable(0);
            this.salesPaymentDifference = ko.observable(0);
            this.changeTenderTypeName = ko.observable("");
            this.lastCustomerOrderMode = Proxy.Entities.CustomerOrderMode.None;

            this.originalCartLines = ko.observableArray(<Proxy.Entities.CartLine[]>[]);
            this.cartLinesTotalCount = ko.computed(this.computedCartLinesTotalCount, this);
            this.incomeExpenseAccountLines = ko.observableArray(this.cart().IncomeExpenseLines);

            this.tenderLines = ko.observableArray<Proxy.Entities.CartTenderLineTenderType>([]);
            this.tenderLinesTotalCount = ko.computed(this.computedTenderLinesTotalCount, this);
            this.tenderTypes = ko.observableArray(<Proxy.Entities.TenderType[]>[]);
            this._isTenderTypesLoaded = false;
            this.cartItemsTotalCountString = ko.computed<string>(() => {
                return StringExtensions.format(ViewModelAdapter.getResourceString("string_128"), this.cartLinesTotalCount());
            }, this);
            this.depositPaid = ko.computed(() => {
                return CustomerOrderHelper.calculateDepositPaid(this.cart());
            }, this);
            this.customerFullName = ko.computed(this.getCustomerFullName, this);

            this._reasonCodesInCartByReasonCodeId = new Dictionary<Entities.ReasonCode>();

            this.cart.subscribe((cart: Proxy.Entities.Cart) => {

                if (ArrayExtensions.hasElements(this.selectedCartLines())) {
                    this.selectedCartLines([]);
                    // Clear originalCartLines so that the selected lines styling is cleared from the list view.
                    // They will be updated again in cartUpdateHandlerAsync
                    this.originalCartLines([]);
                }

                if (ArrayExtensions.hasElements(this.selectedTenderLines())) {
                    this.selectedTenderLines([]);
                }

                this.cartUpdateHandlerAsync(cart);
            }, this);

            this.buttonGrid1 = ko.observable(<Proxy.Entities.ButtonGrid>null);
            this.buttonGrid2 = ko.observable(<Proxy.Entities.ButtonGrid>null);
            this.buttonGrid3 = ko.observable(<Proxy.Entities.ButtonGrid>null);
            this.buttonGrid4 = ko.observable(<Proxy.Entities.ButtonGrid>null);
            this.buttonGrid5 = ko.observable(<Proxy.Entities.ButtonGrid>null);
            this.customControls = ko.observableArray([]);

            this._processTextWorkerQueue = new AsyncWorkerQueue();
        }

        public setCartAsync(cart: Proxy.Entities.Cart): IVoidAsyncResult {
            this.cart(cart);
            return this.cartUpdateHandlerAsync(this.cart());
        }

        /**
         * Loads the view model properties given the session cart.
         */
        public load(): IVoidAsyncResult {
            var transactionViewButtonGridZones: string[] = ['TransactionScreen1', 'TransactionScreen2', 'TransactionScreen3', 'TransactionScreen4', 'TransactionScreen5'];
            var buttonGridDictionary: any = this.applicationContext.tillLayoutProxy.getButtonGridByZoneIds(transactionViewButtonGridZones);
            this.buttonGrid1(buttonGridDictionary.getItem(transactionViewButtonGridZones[0]));
            this.buttonGrid2(buttonGridDictionary.getItem(transactionViewButtonGridZones[1]));
            this.buttonGrid3(buttonGridDictionary.getItem(transactionViewButtonGridZones[2]));
            this.buttonGrid4(buttonGridDictionary.getItem(transactionViewButtonGridZones[3]));
            this.buttonGrid5(buttonGridDictionary.getItem(transactionViewButtonGridZones[4]));

            var customControls = this.applicationContext.tillLayoutProxy.getCustomControls('transactionScreenLayout');
            this.customControls(customControls);

            return this.cartUpdateHandlerAsync(this.cart());
        }

        public getTenderTypes(): Proxy.Entities.TenderType[] {
            return this.tenderTypes();
        }

        /**
         * Gets a reason code by its identifier.
         *
         * @param {string} reasonCodeId The reason code identifier.
         * @return {IAsyncResult<Proxy.Entities.ReasonCode>} The async result.
         */
        private getReasonCodeById(reasonCodeId: string): IAsyncResult<Proxy.Entities.ReasonCode> {
            return this.salesOrderManager.getReasonCodeAsync(reasonCodeId).done((reasonCode: Proxy.Entities.ReasonCode): void => {
                if (!ObjectExtensions.isNullOrUndefined(reasonCode) && !StringExtensions.isNullOrWhitespace(reasonCode.ReasonCodeId)) {
                    this._reasonCodesInCartByReasonCodeId.setItem(reasonCode.ReasonCodeId, reasonCode);
                }
            });
        }

        /**
         * Adds products to the cart by barcode.
         *
         * @param {string} itemId The barcode identifier.
         */
        public searchProductsByItemId(itemId: string): IAsyncResult<Proxy.Entities.Product[]> {
            return this.productManager.searchProductsByBarcodeAsync(itemId, 4, true);
        }

        /**
         * Processes the provided text and updates the cart accordingly.
         * @param {string} searchText The text to use to update the cart.
         * @param {number?} quantity The quantity to be used when updating the cart.
         * @return {IAsyncResult<ICancelableDataResult<IProcessTextResult>>} The async result. 
         */
        public processText(searchText: string, quantity?: number): IAsyncResult<ICancelableDataResult<IProcessTextResult>> {
            return this._processTextWorkerQueue.enqueue((): IAsyncResult<ICancelableDataResult<IProcessTextResult>> => {
                var processTextResult: IProcessTextResult = { cartUpdated: false };
                var scanResult: Proxy.Entities.ScanResult;
                var processTextInstanceQueue: AsyncQueue = new AsyncQueue();

                processTextInstanceQueue.enqueue((): IVoidAsyncResult => {
                    return this.cartManager.getScanResult(searchText)
                        .done((scanData: Proxy.Entities.ScanResult): void => {
                            scanResult = scanData;
                        });
                }).enqueue((): IAsyncResult<ICancelableResult> => {
                    var barcodeMaskType: Proxy.Entities.BarcodeMaskType = scanResult.MaskTypeValue;
                    var processingResult: IAsyncResult<ICancelableResult>;

                    var correlationId: string = Diagnostics.TypeScriptCore.Utils.generateGuid();
                    RetailLogger.viewModelCartProcessScanResultStarted(correlationId, Entities.BarcodeMaskType[barcodeMaskType]);

                    switch (barcodeMaskType) {
                        case Proxy.Entities.BarcodeMaskType.Item:
                            processingResult = this.processItemScan(scanResult, processTextResult, quantity);
                            break;
                        case Proxy.Entities.BarcodeMaskType.Customer:
                            // If the customer is null attempt to search for customer.
                            if (ObjectExtensions.isNullOrUndefined(scanResult.Customer)) {
                                var customerNotFoundError: Proxy.Entities.Error = new Proxy.Entities.Error(ErrorTypeEnum.CUSTOMER_ASSOCIATED_WITH_BARCODE_NOT_FOUND);
                                return VoidAsyncResult.createRejected([customerNotFoundError]);
                            }

                            var customerOptions: Operations.IAddCustomerToSalesOrderOperationOptions = {
                                cartAffiliations: undefined,
                                customerId: scanResult.Customer.AccountNumber,
                                customer: scanResult.Customer
                            };

                            processingResult =
                            this.handleCartUpdateScanResult(this.operationsManager.runOperation(RetailOperation.SetCustomer, customerOptions), processTextResult);

                            break;
                        case Proxy.Entities.BarcodeMaskType.LoyaltyCard:
                            var loyaltyOptions: Operations.IAddLoyaltyCardOperationOptions = { loyaltyCard: scanResult.LoyaltyCard, customer: scanResult.Customer };
                            processingResult =
                            this.handleCartUpdateScanResult(this.operationsManager.runOperation(RetailOperation.LoyaltyRequest, loyaltyOptions), processTextResult);

                            break;
                        case Proxy.Entities.BarcodeMaskType.DiscountCode:
                            var discountOptions: Operations.IAddDiscountCodeBarcodeOperationOptions = {
                                cart: Session.instance.cart,
                                discountCode: scanResult.Barcode.DiscountCode
                            };

                            processingResult =
                            this.handleCartUpdateScanResult(this.operationsManager.runOperation(RetailOperation.DiscountCodeBarcode, discountOptions), processTextResult);

                            break;
                        case Proxy.Entities.BarcodeMaskType.None:
                            return this.searchProductsAndCustomers(searchText, processTextResult);
                            //break;
                        default:
                            RetailLogger.viewModelUnsupportedBarcodeMaskType(Proxy.Entities.BarcodeMaskType[barcodeMaskType]);
                            var barcodeTypeNotSupportedError: Proxy.Entities.Error = new Proxy.Entities.Error(ErrorTypeEnum.BARCODE_TYPE_NOT_SUPPORTED);
                            return VoidAsyncResult.createRejected([barcodeTypeNotSupportedError]);
                    }

                    return processTextInstanceQueue.cancelOn(processingResult).done((result: ICancelableResult): void => {
                        RetailLogger.viewModelCartProcessScanResultFinished(correlationId, true);
                    }).fail((errors: Entities.Error[]): void => {
                        RetailLogger.viewModelCartProcessScanResultFinished(correlationId, false);
                    });
                });

                return processTextInstanceQueue.run().map((queueResult: ICancelableResult): ICancelableDataResult<IProcessTextResult> => {
                    return { canceled: queueResult.canceled, data: processTextResult };
                });
            });
        }

        /**
         * Processes the item scan result.
         * @param {Proxy.Entities.ScanResult} scanResult The scan result to process.
         * @param {IProcessTextResult} processTextResult The process text result to update based on the scan result.
         * @param {number} [quantity] The manual quantity entered, if any.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        private processItemScan(scanResult: Proxy.Entities.ScanResult, processTextResult: IProcessTextResult, quantity?: number): IAsyncResult<ICancelableResult> {
            var product: Proxy.Entities.SimpleProduct = scanResult.Product;

            // If the product is null, trigger the item not found reason code.
            if (ObjectExtensions.isNullOrUndefined(scanResult.Product)) {
                var itemNotOnFileQueue: AsyncQueue = new AsyncQueue();

                itemNotOnFileQueue.enqueue((): IVoidAsyncResult => {
                    var productNotFoundError: Proxy.Entities.Error = new Proxy.Entities.Error(ErrorTypeEnum.PRODUCT_ASSOCIATED_WITH_BARCODE_NOT_FOUND);
                    return NotificationHandler.displayClientErrors([productNotFoundError]);
                }).enqueue((): IAsyncResult<ICancelableResult> => {

                    var previousNumberOfReasonCodeLines: number =
                        ArrayExtensions.hasElements(Session.instance.cart.ReasonCodeLines) ? Session.instance.cart.ReasonCodeLines.length : 0;

                    return ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cart: Session.instance.cart },
                        (context: ReasonCodesContext) => {
                            // If a reason code line was added to the cart make a call to create or update the cart.
                            if (ArrayExtensions.hasElements(context.cart.ReasonCodeLines) && context.cart.ReasonCodeLines.length > previousNumberOfReasonCodeLines) {
                                return this.cartManager.createOrUpdateCartAsync(context.cart);
                            }

                            return VoidAsyncResult.createResolved();
                        },
                        Proxy.Entities.ReasonCodeSourceType.ItemNotOnFile).run()
                });

                return itemNotOnFileQueue.run().done((reasonCodeResult: ICancelableResult): void => {
                    if (reasonCodeResult && !reasonCodeResult.canceled) {
                        processTextResult.cartUpdated = true;
                    }
                });
            }

            // If it is a kit master product return the product since the operation doesn't support adding kit master products to cart.
            if (product.ProductTypeValue === Entities.ProductType.KitMaster) {
                processTextResult.product = product;
                return VoidAsyncResult.createResolved();
            }

            var barcode: Proxy.Entities.Barcode = scanResult.Barcode;
            if (!ObjectExtensions.isNullOrUndefined(barcode)) {
                if (!NumberExtensions.isNullOrZero(quantity)
                    && (!NumberExtensions.isNullOrZero(barcode.Quantity) && !NumberExtensions.isNullOrZero(barcode.BarcodePrice))) {
                    // If the barcode contains price embedded information manual entry of quantity is not allowed.
                    var noManualEntryError: Proxy.Entities.Error =
                        new Proxy.Entities.Error(ErrorTypeEnum.MANUAL_QUANTITY_NOT_ALLOWED_ON_PRICE_EMBEDDED_BARCODE);

                    return VoidAsyncResult.createRejected([noManualEntryError]);
                } else if (!NumberExtensions.isNullOrZero(quantity) && !NumberExtensions.isNullOrZero(barcode.Quantity)) {
                    quantity *= barcode.Quantity;
                } else if (!NumberExtensions.isNullOrZero(barcode.Quantity)) {
                    quantity = barcode.Quantity;
                }
            }

            if (ObjectExtensions.isNullOrUndefined(quantity)) {
                quantity = 0;
            }

            var productSaleReturnDetail: Proxy.Entities.ProductSaleReturnDetails = {
                product: product,
                quantity: quantity,
                barcode: barcode
            };

            var productSaleReturnDetails: Proxy.Entities.ProductSaleReturnDetails[] = [productSaleReturnDetail];

            var addToSaleResult: IAsyncResult<ICancelableResult>;
            if (quantity >= 0) {
                addToSaleResult = this.addProductsToCart(productSaleReturnDetails);
            } else {
                addToSaleResult = this.returnProducts(productSaleReturnDetails);
            }

            return addToSaleResult.done((result: ICancelableResult): void => {
                if (result && !result.canceled) {
                    processTextResult.cartUpdated = true;
                }
            });
        }

        /**
         * Handles the result of scan update methods, updating the process text result accordingly.
         * @param {IAsyncResult<ICancelableResult>} asyncResult The async result.
         * @param {IProcessTextResult} processTextResult The process text result.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        private handleCartUpdateScanResult(asyncResult: IAsyncResult<ICancelableResult>, processTextResult: IProcessTextResult): IAsyncResult<ICancelableResult> {
            return asyncResult.done((result: ICancelableResult): void => {
                if (result && !result.canceled) {
                    this.cart(Session.instance.cart);
                    processTextResult.cartUpdated = true;
                }
            });
        }

        /**
         * Searches for products and customers and updates the process text result.
         * @param {string} searchText The search value.
         * @param {IProcessTextResult} The process text result to update.
         * @return {IVoidAsyncResult} The async result.
         */
        private searchProductsAndCustomers(searchText: string, processTextResult: IProcessTextResult): IVoidAsyncResult {
            var catalogId: number = 0;
            var skip: number = 0;
            var pageSize: number = 2;
            var productResult: IAsyncResult<Proxy.Entities.ProductSearchResult[]> =
                this.productManager.searchByTextAsync(searchText, Session.instance.productCatalogStore.Context.ChannelId, catalogId, pageSize, skip)
                    .done((productSearchResults: Proxy.Entities.ProductSearchResult[]): void => {
                        processTextResult.productSearchResults = productSearchResults;
                    });

            var customerSearchResult: IAsyncResult<Proxy.Entities.GlobalCustomer> =
                this.searchCustomers(searchText, pageSize, skip).done((customerResult: Proxy.Entities.GlobalCustomer[]): void => {
                    processTextResult.customers = customerResult;
                });

            return VoidAsyncResult.join([productResult, customerSearchResult]);
        }

        private searchCustomers(searchText: string, pageSize?: number, skip?: number): IAsyncResult<Proxy.Entities.GlobalCustomer[]> {
            if (StringExtensions.isNullOrWhitespace(searchText)) {
                return AsyncResult.createResolved([]);
            }

            return this.customerManager.searchCustomersAsync(searchText, pageSize, skip);
        }

        /**
         * Adds loyalty card to the transaction.
         * @param {string} [loyaltyCardId] The optional loyalty card identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        public addLoyaltyCardToCartAsync(loyaltyCardId?: string): IAsyncResult<ICancelableResult> {
            var options: Operations.IAddLoyaltyCardOperationOptions = { loyaltyCardId: loyaltyCardId };
            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.LoyaltyRequest, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add cart tender line to the cart.
         *
         * @param {Proxy.Entities.CartTenderLine} cartTenderLine The cart tender line to add.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addCartTenderLineToCart(cartTenderLine: Proxy.Entities.CartTenderLine): IAsyncResult<ICancelableResult> {
            var tenderLineQueue: AsyncQueue = new AsyncQueue();
            tenderLineQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreAddTenderLineTriggerOptions = { cart: Session.instance.cart, tenderLine: cartTenderLine };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreAddTenderLine, preTriggerOptions);

                return tenderLineQueue.cancelOn(preTriggerResult);
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                return this.cartManager.addTenderLineToCartAsync(cartTenderLine);
            });

            return tenderLineQueue.run().done((result) => {
                if (!result.canceled) {
                    this.addOrUpdateTenderLinesSuccess();
                }
            });
        }

        /**
         * Add tender line to the cart.
         *
         * @param {Proxy.Entities.TenderLine} tenderLine tender line to add.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addTenderLineToCart(tenderLine: Proxy.Entities.TenderLine): IAsyncResult<ICancelableResult> {
            var tenderLineQueue: AsyncQueue = new AsyncQueue();
            tenderLineQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreAddTenderLineTriggerOptions = { cart: Session.instance.cart, tenderLine: tenderLine };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreAddTenderLine, preTriggerOptions);

                return tenderLineQueue.cancelOn(preTriggerResult);
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var reasonCodeResult: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { tenderLines: [tenderLine] },
                    (c: ReasonCodesContext) => { return this.cartManager.addTenderLineToCartAsync(c.tenderLines[0]); }).run();

                return tenderLineQueue.cancelOn(reasonCodeResult);
            });

            return tenderLineQueue.run().done((result) => {
                if (!result.canceled) {
                    this.addOrUpdateTenderLinesSuccess();
                }
            });
        }

        /**
         * Add preprocessed tender line to the cart.
         *
         * @param {Proxy.Entities.TenderLine} tenderLine tender line to add.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addPreprocessedTenderLineToCart(tenderLine: Proxy.Entities.TenderLine): IAsyncResult<ICancelableResult> {
            var tenderLineQueue: AsyncQueue = new AsyncQueue();
            tenderLineQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var preTriggerOptions: Triggers.IPreAddTenderLineTriggerOptions = { cart: Session.instance.cart, tenderLine: tenderLine };
                var preTriggerResult: IAsyncResult<ICancelableResult> =
                    Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreAddTenderLine, preTriggerOptions);

                return tenderLineQueue.cancelOn(preTriggerResult);
            }).enqueue((): IAsyncResult<ICancelableResult> => {
                var reasonCodeResult: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { tenderLines: [tenderLine] },
                    (c: ReasonCodesContext) => { return this.cartManager.addPreprocessedTenderLineToCartAsync(c.tenderLines[0]); }).run()

                return tenderLineQueue.cancelOn(reasonCodeResult);
            });

            return tenderLineQueue.run().done((result) => {
                if (!result.canceled) {
                    this.addOrUpdateTenderLinesSuccess();
                }
            });
        }

        /**
         * Updates tender line in the cart.
         *
         * @param {Proxy.Entities.TenderLine} tenderLine tender line to add.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public updatePreprocessedTenderLineInCart(tenderLine: Proxy.Entities.TenderLine): IAsyncResult<ICancelableResult> {
            return ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                { tenderLines: [tenderLine] },
                c => { return this.cartManager.updatePreprocessedTenderLineInCartAsync(c.tenderLines[0]); })
                .run()
                .done((result) => {
                    if (!result.canceled) {
                        this.addOrUpdateTenderLinesSuccess();
                    }
                });
        }

        /**
         * Updates the signature of a tender line in the cart.
         *
         * @param {Proxy.Entities.TenderLine} tenderLine tender line to add.
         */
        public updateTenderLineSignatureInCart(tenderLine: Proxy.Entities.TenderLine): IVoidAsyncResult {
            return this.cartManager.updateTenderLineSignature(tenderLine.TenderLineId, tenderLine.SignatureData)
                .done(() => { this.addOrUpdateTenderLinesSuccess(); });
        }

        /**
         * Updates tenderLines property used in view bindings.
         */
        private addOrUpdateTenderLinesSuccess(): void {
            this.cart(Session.instance.cart);
            var cart = this.cart();
            this.refreshCartTenderTypes(cart);

            // Clear sensitive data on success
            if (cart.TenderLines.length > 0) {
                var tenderLine = cart.TenderLines[cart.TenderLines.length - 1];
                CartViewModel.clearSensitiveDataOnTenderLine(tenderLine);
            }
        }

        /**
         * Validates whether a tender line is valid for add.
         * Can be used for any tender line, but targeted to validate tender lines that go to a different source
         * than retail server for processing before retail server validation.
         *
         * @param {Entities.TenderLine} tenderLine The tender line to validate before adding it to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public validateTenderLineForAdd(tenderLine: Proxy.Entities.TenderLine): IVoidAsyncResult {
            return this.cartManager.validateTenderLineForAddAsync(tenderLine);
        }

        /**
         * Whether an income/expense line can be added to the transaction
         *
         * @return {boolean} Returns true if an income/expense line can be added, otherwise false
         */
        public canAddIncomeExpenseLine(): boolean {
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            if (StringExtensions.isNullOrWhitespace(cart.Id)
                || !(ArrayExtensions.hasElements(cart.CartLines)
                    || ArrayExtensions.hasElements(cart.CustomerAccountDepositLines)
                    || StringExtensions.isNullOrWhitespace(cart.CustomerId)
                    || ArrayExtensions.hasElements(cart.IncomeExpenseLines))) {
                return true;
            } else {
                return false;
            }
        }

        /**
         * Checkout cart.
         *
         * @param {string} receiptEmail The email URL to send the receipt
         * @param {Entities.TokenizedPaymentCard} tokenizedPaymentCard The tokenized card to charge.
         * @return {IAsyncResult<{ canceled: boolean; data: Proxy.Entities.Receipt[] }>} The async result which can contain the receipts or be canceled.
         */
        public checkOutCart(
            receiptEmail: string,
            tokenizedPaymentCard?: Proxy.Entities.TokenizedPaymentCard,
            isPickingUpProducts?: boolean): IAsyncResult<{ canceled: boolean; data: Proxy.Entities.Receipt[] }> {
            var salesOrder: Proxy.Entities.SalesOrder;
            var receipts: Proxy.Entities.Receipt[];

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // always revert to self to avoid creating a transaction under manager's credentials
                    return asyncQueue.cancelOn(this.operationsManager.revertToSelf().run());
                }).enqueue(() => {
                    // Checkout the cart
                    var updateCartBeforeCheckout: boolean = false;
                    var retryQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                        { cart: Session.instance.cart },
                        (context) => {
                            var updateAsyncQueue = new AsyncQueue()
                            if (updateCartBeforeCheckout) {
                                updateAsyncQueue.enqueue(() => {
                                    // updates the cart with the added reason codes
                                    return this.cartManager.createOrUpdateCartAsync(context.cart);
                                });
                            }

                            updateCartBeforeCheckout = true;

                            updateAsyncQueue.enqueue(() => {
                                return this.cartManager.checkoutCartAsync(receiptEmail, tokenizedPaymentCard)
                                    .done((salesOrderResult: Proxy.Entities.SalesOrder) => {
                                        salesOrder = salesOrderResult;
                                        this._isManualChangeRequired = false;
                                    });
                            });

                            return updateAsyncQueue.run();
                        });

                    return asyncQueue.cancelOn(retryQueue.run());
                }).enqueue(() => {
                    return this.checkOutCartSuccess(salesOrder, isPickingUpProducts).done((data) => {
                        receipts = data;
                    });
                });

            return asyncQueue.run()
                .map((result) => {
                    return { canceled: result.canceled, data: receipts };
                });
        }

        private handleCheckoutFailure(errors: Proxy.Entities.Error[]) {
            // if checkout failed because of invalid change tender method
            if (ErrorHelper.hasError(errors, ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGETENDERTYPENOTSUPPORTED)) {
                this._isManualChangeRequired = true;
            }
        }

        /**
         * Voids the current cart.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public voidTransaction(): IAsyncResult<ICancelableResult> {
            var options: Operations.IVoidTransactionOperationOptions = { cart: this.cart() };
            var operationResult = this.operationsManager.runOperation(Operations.RetailOperation.VoidTransaction, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                    this.originalCartLines(<Proxy.Entities.CartLine[]>[]);
                    this.incomeExpenseAccountLines(<Proxy.Entities.IncomeExpenseAccount[]>[]);
                    this.customerAccountDepositLines(<Proxy.Entities.CustomerAccountDepositLine[]>[]);
                    this.tenderLines(<Proxy.Entities.CartTenderLineTenderType[]>[]);
                    this.customer(null);
                    this.customerAddress(null);
                    this.customerInvoiceAddress(null);
                }
            });
        }

        /**
         * Calculates order total.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public calculateTotalAsync(): IVoidAsyncResult {
            return this.operationsManager.runOperation(Operations.RetailOperation.CalculateFullDiscounts, null)
                .done((): void => {
                    this.cart(Session.instance.cart);
                });
        }

        /**
         * Recalculates customer order.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public recalculateCustomerOrder(): IVoidAsyncResult {
            var recalculateOperationParameters: Operations.IRecalculateCustomerOrderOperationOptions = {
                cart: this.cart()
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.RecalculateCustomerOrder, recalculateOperationParameters)
                .done(() => {
                    this.cart(Session.instance.cart);
                });
        }

        /**
         *  Gets whether a payment can be added to the transaction.
         *
         *  @return {boolean} Returns whether a payment can be added to the transaction.
         */
        public get canAddPayment(): boolean {
            var cart: Proxy.Entities.Cart = this.cart();
            var balanceDue: number = cart.AmountDue;

            // a payment can be added at any moment when the cart has lines (cart or income expense lines) present and the transaction balance is not 0 or a customer account deposit
            return ((ArrayExtensions.hasElements(cart.CartLines)
                || ArrayExtensions.hasElements(cart.IncomeExpenseLines)
                || ArrayExtensions.hasElements(cart.CustomerAccountDepositLines))
                && balanceDue != 0);
        }

        /**
         * Whether the cart is ready for checkout (no more payments are allowed).
         *
         * @return {boolean} Returns whether the cart is ready for checkout (no more payments are allowed).
         */
        public get canCheckout(): boolean {
            var cart: Proxy.Entities.Cart = this.cart();
            var hasLines: boolean = ArrayExtensions.hasElements(cart.CartLines)
                || ArrayExtensions.hasElements(cart.IncomeExpenseLines)
                || ArrayExtensions.hasElements(cart.CustomerAccountDepositLines);

            // checkout happens when cart has at least one line and card is tendered
            return hasLines && cart.IsRequiredAmountPaid && !this._isManualChangeRequired;
        }

        /**
         * Checks whether cart is in customer order creation process
         *
         * @return {boolean} Returns true if cart in customer order creation process
         */
        public get isCustomerOrderCreationOrEdition(): boolean {
            return CustomerOrderHelper.isCustomerOrderCreationOrEdition(this.cart());
        }

        /**
         * Checks whether cart is in customer order cancellation process
         *
         * @return {boolean} Returns true if cart in customer order cancellation process
         */
        public get isCustomerOrderCancellation(): boolean {
            return CustomerOrderHelper.isCustomerOrderCancellation(this.cart());
        }

        /**
         * Checks whether cart is in customer order pickup process
         * @return {boolean} Returns true if cart in customer order pickup process
         */
        public get isCustomerOrderPickup(): boolean {
            return CustomerOrderHelper.isCustomerOrderPickup(this.cart());
        }

        /**
         * Suspends the current cart.
         * @param {IVoidAsyncResult} The async result.
         */
        public suspendTransaction(): IVoidAsyncResult {
            var options: Operations.ISuspendTransactionOperationOptions = { cart: this.cart() };
            var operationResult = this.operationsManager.runOperation(Operations.RetailOperation.SuspendTransaction, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                    this.originalCartLines(<Proxy.Entities.CartLine[]>[]);
                    this.tenderLines(<Proxy.Entities.CartTenderLineTenderType[]>[]);
                }
            });
        }

        /**
         * Voids the product lines.
         * @param {Proxy.Entities.CartLine[]} cartLines The cart lines to be voided.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public voidProducts(cartLines: Proxy.Entities.CartLine[]): IAsyncResult<ICancelableResult> {
            var options: Operations.IVoidProductsOperationOptions = { cartLines: cartLines };
            return this.operationsManager.runOperation(Operations.RetailOperation.VoidItem, options)
                .done((result: Operations.IOperationResult) => {
                    if (!result.canceled) {
                        this.cart(Session.instance.cart);
                        RetailLogger.viewModelCartVoidProductsFinished(true);
                    }
                }).fail((errors: Proxy.Entities.Error[]) => {
                    RetailLogger.viewModelCartVoidProductsFinished(false);
                });
        }

        /**
         * Update OverriddenDepositAmount for the session cart.
         *
         * @param {number} overriddenDepositAmount The new deposit amount.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateOverriddenDepositAmountAsync(overridenDepositAmount: number): IVoidAsyncResult {
            return this.operationsManager.runOperation(
                Operations.RetailOperation.DepositOverride,
                <Operations.IDepositOverrideOperationOptions>{ depositOverrideAmount: overridenDepositAmount })
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Overrides transaction tax for the session cart from the store's tax override group.
         *
         * @return {IVoidAsyncResult} The async result.
         */
        public overrideTransactionTaxFromList(): IVoidAsyncResult {
            var options: Operations.IOverrideTransactionTaxFromListOperationOptions = { cart: this.cart() };
            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.OverrideTaxTransactionList, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Overrides transaction tax for the session cart with the given tax override.
         *
         * @param {Proxy.Entities.TaxOverride} taxOverride The tax override.
         * @return {IVoidAsyncResult} The async result.
         */
        public overrideTransactionTax(taxOverride: Proxy.Entities.TaxOverride): IVoidAsyncResult {
            var options: Operations.IOverrideTransactionTaxOperationOptions = {
                cart: this.cart(),
                taxOverride: taxOverride
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.OverrideTaxTransaction, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Overrides the selected line tax with the given tax override.
         *
         * @param {Proxy.Entities.TaxOverride} taxOverride The tax override.
         * @return {IVoidAsyncResult} The async result.
         */
        public overrideLineTax(taxOverride: Proxy.Entities.TaxOverride): IVoidAsyncResult {
            var options: Operations.IOverrideLineProductTaxOperationOptions = {
                cartLine: this.selectedCartLines()[0],
                taxOverride: taxOverride
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.OverrideTaxLine, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Overrides the selected line tax with a tax override from a list.
         *
         * @param {Proxy.Entities.TaxOverride} taxOverride The tax override.
         * @return {IVoidAsyncResult} The async result.
         */
        public overrideLineTaxFromList(): IVoidAsyncResult {
            var options: Operations.IOverrideLineProductTaxFromListOperationOptions = {
                cartLine: this.selectedCartLines()[0]
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.OverrideTaxLineList, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Voids the payment line.
         * @param {Proxy.Entities.CartTenderLine[]} tenderLines Tender lines.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public voidPayment(tenderLines: Proxy.Entities.CartTenderLine[]): IAsyncResult<ICancelableResult> {
            var options: Operations.IVoidPaymentOperationOptions = { tenderLines: tenderLines };
            return this.operationsManager.runOperation(Operations.RetailOperation.VoidPayment, options)
                .done((result: Operations.IOperationResult) => {
                    if (!result.canceled) {
                        this.cart(Session.instance.cart);
                    }
                });
        }

        /**
         * Add a comment to the transaction.
         *
         * @param {string} [comment] The optional comment to be added to the transaction.
         * @return {IvoidAsyncResult} The async result.
         * @remarks If no comment is passed, it is up to the operation to handle this.
         */
        public addTransactionComment(comment?: string): IVoidAsyncResult {
            var options: Operations.ITransactionCommentOperationOptions = { cart: this.cart(), comment: comment };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.TransactionComment, options)
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Adds a comment to the customer account deposit line.
         *
         * @param {string} [comment] The optional comment to be added to the customer account deposit line.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public addCustomerAccountDepositComment(comment?: string): IAsyncResult<ICancelableResult> {
            var cart: Proxy.Entities.Cart = this.cart();
            var options: Operations.IProductCommentOperationOptions = {
                cart: cart,
                cartLineComments: null,
                customerAccountDepositLineComment: { line: cart.CustomerAccountDepositLines[0], comment: comment }
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ItemComment, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add comments to the selected cart lines.
         *
         * @param {string[]} [comments] The optional comments to be added to the selected cart lines.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         * @remarks The comment collection is mapped one to one to the selected cart lines. If less comments than cart lines are provided,
         * the extra cart lines will have an undefined comment, leaving the operation to handle this.
         */
        public addProductComments(comments?: string[]): IAsyncResult<ICancelableResult> {
            comments = comments || [];
            var options: Operations.IProductCommentOperationOptions = {
                cart: this.cart(),
                cartLineComments: this.selectedCartLines().map((cartLine, index) => {
                    return { cartLine: cartLine, comment: comments[index] };
                }),
                customerAccountDepositLineComment: null
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ItemComment, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add an income account line comment to the transaction.
         *
         * @param {string[]} [comment] The optional income account line comment to be added to the transaction.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         * @remarks If no comment is passed, it is up to the operation to handle this.
         */
        public addIncomeAccountComment(comments?: string[]): IAsyncResult<ICancelableResult> {
            // line comments for income account transaction is not allowed and hence comments are not passed
            var options: Operations.IProductCommentOperationOptions = {
                cart: this.cart(),
                cartLineComments: null,
                customerAccountDepositLineComment: null
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ItemComment, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add an invoice comment to the transaction.
         *
         * @param {string} [comment] The optional invoice comment to be added to the transaction.
         * @return {IvoidAsyncResult} The async result.
         * @remarks If no comment is passed, it is up to the operation to handle this.
         */
        public addInvoiceComment(comment?: string): IVoidAsyncResult {
            var options: Operations.IInvoiceCommentOperationOptions = { cart: this.cart(), comment: comment };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.InvoiceComment, options)
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Adds the discount amount to the selected lines.
         * @param {number[]} [discounts] The optional discount collection, containing the discount amounts.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         * @remarks The comment collection is mapped one to one to the selected cart lines. If less discounts than cart lines are provided,
         * the extra cart lines will have an undefined discount, leaving the operation to handle this.
         */
        public addLineDiscountAmount(discounts?: number[]): IAsyncResult<ICancelableResult> {
            discounts = discounts || [];

            var options: Operations.ILineDiscountOperationOptions = {
                cartLineDiscounts: this.selectedCartLines().map((cartLine, index) => {
                    return { cartLine: cartLine, discountValue: discounts[index] };
                })
            };

            var result: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                Operations.RetailOperation.LineDiscountAmount, options);

            return result.done((result: Operations.IOperationResult) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Adds the discount percentage to the selected lines.
         * @param {number[]} [discounts] The optional discount collection, containing the discount percentages.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         * @remarks The comment collection is mapped one to one to the selected cart lines. If less discounts than cart lines are provided,
         * the extra cart lines will have an undefined discount, leaving the operation to handle this.
         */
        public addLineDiscountPercent(discounts?: number[]): IAsyncResult<ICancelableResult> {
            discounts = discounts || [];

            var options: Operations.ILineDiscountOperationOptions = {
                cartLineDiscounts: this.selectedCartLines().map((cartLine, index) => {
                    return { cartLine: cartLine, discountValue: discounts[index] };
                })
            };

            var result: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                Operations.RetailOperation.LineDiscountPercent, options);

            return result.done((result: Operations.IOperationResult) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add transaction discount amount.
         * @param {number} [discount] The optional discount amount.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addTransactionDiscountAmount(discount?: number): IAsyncResult<ICancelableResult> {
            var options: Operations.ITransactionDiscountOperationOptions = {
                cart: this.cart(),
                discountValue: discount
            };

            var result: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                Operations.RetailOperation.TotalDiscountAmount, options);

            return result.done((result: Operations.IOperationResult) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add transaction discount percent.
         * @param {number} [discount] The optional discount amount.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addTransactionDiscountPercent(discount?: number): IAsyncResult<ICancelableResult> {
            var options: Operations.ITransactionDiscountOperationOptions = {
                cart: this.cart(),
                discountValue: discount
            };

            var result: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                Operations.RetailOperation.TotalDiscountPercent, options);

            return result.done((result: Operations.IOperationResult) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Add discount code to the transaction.
         *
         * @param {string} discountCode The discount code.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public addDiscountCode(discountCode?: string): IAsyncResult<ICancelableResult> {
            var options: Operations.IAddDiscountCodeBarcodeOperationOptions = {
                cart: this.cart(),
                discountCode: discountCode
            };

            var result = this.operationsManager.runOperation(
                Operations.RetailOperation.DiscountCodeBarcode, options);

            return result.done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Set quantities on the selected cart lines.
         *
         * @param {number[]} [quantities] The optional quantities to be set on the selected cart lines.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         * @remarks The quantity collection is mapped one to one to the selected cart lines. If less quantities than cart lines are provided,
         * the extra cart lines will have an undefined quantity, leaving the operation to handle this.
         */
        public setQuantities(quantities?: number[]): IAsyncResult<ICancelableResult> {
            quantities = quantities || [];
            var options: Operations.ISetQuantityOperationOptions = {
                cartLineQuantities: this.selectedCartLines().map((cartLine, index) => {
                    return { cartLine: cartLine, quantity: quantities[index] };
                })
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.SetQuantity, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Clear quantities on the selected cart lines.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         */
        public clearQuantities(): IAsyncResult<ICancelableResult> {
            var options: Operations.IClearQuantityOperationOptions = {
                cartLines: this.selectedCartLines().map((cartLine: Proxy.Entities.CartLine) => {
                    return cartLine;
                })
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ClearQuantity, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Change the unit of measures on the selected cart lines.
         * @param {Proxy.Entities.UnitOfMeasure[]} [unitOfMeasures] The optional unit of measures to be set on the selected cart lines.
         * @return {IAsyncResult<ICancelableResult>} The async result.
         * @remarks The unit of measure collection is mapped one to one to the selected cart lines. If less unit of measures than cart lines are
         * provided, the extra cart lines will have an undefined unit of measure, leaving the operation to handle this.
         */
        public changeUnitOfMeasures(unitOfMeasures?: Proxy.Entities.UnitOfMeasure[]): IAsyncResult<ICancelableResult> {
            unitOfMeasures = unitOfMeasures || [];
            var options: Operations.IChangeUnitOfMeasureOperationOptions = {
                cartLineUnitOfMeasures: this.selectedCartLines().map((cartLine, index) => {
                    return { cartLine: cartLine, unitOfMeasure: unitOfMeasures[index] };
                })
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ChangeUnitOfMeasure, options);

            return operationResult.done((result) => {
                if (!result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Gets the tender type for the specified tender operation identifier.
         *
         * @param {number} tenderOperationId The tender operation identifier.
         * @return The tender type, null if not found.
         */
        public getTenderType(tenderOperationId: number): Proxy.Entities.TenderType {
            var tenderTypes = this.tenderTypes().filter((value: Proxy.Entities.TenderType) => {
                return value.OperationId == tenderOperationId;
            });

            var tenderType: Proxy.Entities.TenderType = null;
            if (tenderTypes.length > 0) {
                tenderType = tenderTypes[0];
            }

            return tenderType;
        }

        private refreshCartTenderTypes(cart: Proxy.Entities.Cart) {
            if (!ObjectExtensions.isNullOrUndefined(cart)
                && ArrayExtensions.hasElements(cart.TenderLines)) {
                var updatedTenderLines: Proxy.Entities.CartTenderLineTenderType[] = [];

                cart.TenderLines.forEach((tenderLine: Proxy.Entities.CartTenderLine) => {
                    var tenderType: Proxy.Entities.TenderType = null;

                    // ignores all historical tender lines, they are shown on a separate view
                    if (!tenderLine.IsHistorical) {
                        tenderType = ApplicationContext.Instance.tenderTypesMap.getTenderByTypeId(tenderLine.TenderTypeId);

                        var cartTenderLineTenderType: Proxy.Entities.CartTenderLineTenderType = <Proxy.Entities.CartTenderLineTenderType>$.extend({
                            TenderType: tenderType,
                        }, tenderLine);

                        updatedTenderLines.push(cartTenderLineTenderType);
                    }
                });
                this.tenderLines(updatedTenderLines);
            }

            if (!this._isTenderTypesLoaded) {
                ApplicationContext.Instance.tenderTypesMap.getItems().forEach((operationsTenderTypes: Proxy.Entities.TenderType[]) => {
                    operationsTenderTypes.forEach((tenderType: Proxy.Entities.TenderType) => {
                        if (tenderType.OperationId != 0 &&
                            tenderType.Function != TenderFunctionEnum.TenderRemoveFloat) {
                            this.tenderTypes().push(tenderType);
                        }
                    });
                });

                this.tenderTypes(this.tenderTypes());
                this._isTenderTypesLoaded = true;
            }
        }

        /**
         * Gets executable tender types.
         *
         * @param {Proxy.Entities.TenderType[]} [tenderTypes] The tender types to check.
         * @return {IAsyncResult<<Proxy.Entities.TenderType[]>} The list of executable tender types.
         */
        public getExecutableTenderTypesAsync(tenderTypes?: Proxy.Entities.TenderType[]): IAsyncResult<Proxy.Entities.TenderType[]> {
            var asyncResult = new AsyncResult<Proxy.Entities.TenderType[]>();
            if (ObjectExtensions.isNullOrUndefined(tenderTypes)) {
                tenderTypes = this.getTenderTypes();
            }

            // Filter for the tender types that can be executed
            var executableTenderTypes: Proxy.Entities.TenderType[] = new Array();
            var executableTenderTypeResults: IAsyncResult<any>[] = new Array();
            if (ArrayExtensions.hasElements(tenderTypes)) {
                tenderTypes.forEach((tenderType: Proxy.Entities.TenderType) => {
                    var paymentControllerOptions: Operations.IPaymentOperationOptions = <Operations.IPaymentOperationOptions>{
                        tenderType: tenderType
                    };

                    executableTenderTypeResults.push(Operations.OperationsManager.instance.canExecuteAsync(tenderType.OperationId, paymentControllerOptions)
                        .done(() => {
                            executableTenderTypes.push(tenderType);
                        }));
                });
            }

            // Wait for the tender types to be validated and display to the user
            VoidAsyncResult.join(executableTenderTypeResults).always(() => {
                asyncResult.resolve(executableTenderTypes);
            });

            return asyncResult;
        }

        /**
         * Clears the sensitive data on tender line.
         */
        private static clearSensitiveDataOnTenderLine(tenderLine: Proxy.Entities.CartTenderLine): void {
            if (!ObjectExtensions.isNullOrUndefined(tenderLine.PaymentCard)) {
                if (tenderLine.PaymentCard.IsSwipe) {
                    tenderLine.PaymentCard.Track1 = StringExtensions.EMPTY;
                    tenderLine.PaymentCard.Track2 = StringExtensions.EMPTY;
                    tenderLine.PaymentCard.Track3 = StringExtensions.EMPTY;
                } else {
                    tenderLine.PaymentCard.CardNumber = StringExtensions.EMPTY;
                    tenderLine.PaymentCard.CCID = StringExtensions.EMPTY;
                    tenderLine.PaymentCard.Zip = StringExtensions.EMPTY;
                    tenderLine.PaymentCard.Country = PaymentViewModel.getDefaultCountryRegionISOCode();
                }
            }
        }

        /**
         * Loads the listing objects from the cart lines.
         *
         * @param {Cart} cart The cart.
         * @return {IVoidAsyncResult} The async result.
         */
        private loadListingsFromCartLinesAsync(cart: Proxy.Entities.Cart): IVoidAsyncResult {

            var asyncResult = new VoidAsyncResult();

            if (ObjectExtensions.isNullOrUndefined(cart) || ObjectExtensions.isNullOrUndefined(cart.CartTypeValue)) {
                asyncResult.resolve();
                return asyncResult;
            }

            if (cart.CartTypeValue === Proxy.Entities.CartType.IncomeExpense && ArrayExtensions.hasElements(cart.IncomeExpenseLines)) {
                this.incomeExpenseAccountLines(this.cart().IncomeExpenseLines);
                asyncResult.resolve();
            } else if (CartHelper.isCartType(cart, Proxy.Entities.CartType.AccountDeposit)) {
                this.customerAccountDepositLines(this.cart().CustomerAccountDepositLines);
                asyncResult.resolve();
            } else if (ArrayExtensions.hasElements(cart.CartLines)) {
                //filter cart listing ids
                var missingProductIds: number[] = [];
                $.each(cart.CartLines, function (index, cartLine) {
                    var productId = cartLine.ProductId;
                    //also check if line is a gift card since they have listing Id = 0
                    if (ObjectExtensions.isNullOrUndefined(Session.instance.getFromProductsInCartCache(productId)) && CartLineHelper.IsProduct(cartLine)) {
                        missingProductIds.push(productId);
                    }
                });

                if (ArrayExtensions.hasElements(missingProductIds)) {
                    var channelId: number = Session.instance.productCatalogStore.Context.ChannelId;
                    this.productManager.getByIdsAsync(missingProductIds, channelId)
                        .done((products: Proxy.Entities.SimpleProduct[]) => {
                            products.forEach((product: Entities.SimpleProduct): void => {
                                Session.instance.addToProductsInCartCache(product);
                            });
                            this.originalCartLines(this.cart().CartLines);
                            asyncResult.resolve();
                        }).fail((errors: Proxy.Entities.Error[]) => {
                            asyncResult.reject(errors);
                        });
                } else {
                    this.originalCartLines(this.cart().CartLines);
                    asyncResult.resolve();
                }
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Gets the customer details and updates the customer releated observables.
         * @param {string} customerAccount The customer account number for which to retrieve the customer details.
         * @return {IVoidAsyncResult} The async result.
         */
        public getCustomerDetails(customerAccount: string): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            if (StringExtensions.isNullOrWhitespace(customerAccount)) {
                this.updateCustomerObservables(null, null).done(() => {
                    asyncResult.resolve();
                });
            } else if (ObjectExtensions.isNullOrUndefined(this.customer()) || this.customer().AccountNumber !== customerAccount) {
                this.customer(new Proxy.Entities.CustomerClass({ AccountNumber: customerAccount }));

                this.customerManager.getCustomerDetailsAsync(customerAccount)
                    .done((customerDetails) => {
                        Session.instance.Customer = customerDetails.customer;
                        this.updateCustomerObservables(customerDetails.customer, customerDetails.primaryAddress).done(() => {
                            asyncResult.resolve();
                        });
                    }).fail((errors) => {
                        asyncResult.resolve();
                    });
            } else {
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Gets name of the employee by identifier.
         *
         * @param {string} staffId Identifier of the employee.
         * @return {IAsyncResult<string>} The async result.
         */
        public getEmployeeNameAsync(staffId: string): IAsyncResult<string> {
            return this.operatorManager.getEmployeeAsync(staffId)
                .map((employee: Proxy.Entities.Employee): string => {
                    var name: string = StringExtensions.EMPTY;

                    if (!ObjectExtensions.isNullOrUndefined(employee)) {
                        name = employee.Name;
                    }

                    return name;
                });
        }

        /**
         * Issues a new gift card and add it to the session cart.
         *
         * @param {string} cardNumber The  gift card number.
         * @param {number} amount The gift card amount.
         * @param {string} currency The gift card amount currency.
         * @param {string} lineDescription The cart line description
         * @return {IVoidAsyncResult} The async result.
         */
        public issueGiftCardAsync(cardNumber: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult {
            var options: Operations.IIssueGiftCardOperationOptions = {
                amount: amount, currency: currency, giftCardId: cardNumber, lineDescription: lineDescription
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.IssueGiftCertificate, options)
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Adds the amount to the gift card.
         *
         * @param {string} cardNumber The gift card number.
         * @param {number} amount The gift card amount.
         * @param {string} currency The gift card amount currency.
         * @param {string} lineDescription The cart line description
         * @return {IVoidAsyncResult} The async result.
         */
        public addToGiftCardAsync(cardNumber: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult {
            var options: Operations.IAddGiftCardOperationOptions = {
                amount: amount, currency: currency, giftCardId: cardNumber, lineDescription: lineDescription
            };
            return this.operationsManager.runOperation(Operations.RetailOperation.AddToGiftCard, options)
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Gets the gift card by its number.
         *
         * @param {string} cardNumber The gift card number.
         * @return {IAsyncResult<Proxy.Entities.GiftCard>} The async result.
         */
        public getGiftCardByIdAsync(cardNumber: string): IAsyncResult<Proxy.Entities.GiftCard> {
            return this.paymentManager.getGiftCardById(cardNumber);
        }

        /**
         * Issues credit memo.
         *
         * @param {string} recipientEmailAddress The receipt email.
         * @return {IAsyncResult<Proxy.Entities.Cart[]>} The async result containing the receipts.
         */
        public issueCreditMemoAsync(recipientEmail: string): IAsyncResult<Proxy.Entities.Receipt[]> {
            var salesOrder: Proxy.Entities.SalesOrder;
            var receipts: Proxy.Entities.Receipt[];

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    var options: Operations.IIssueCreditMemoOperationOptions = { recipientEmailAddress: recipientEmail };
                    var result = this.operationsManager.runOperation<Proxy.Entities.SalesOrder>(
                        Operations.RetailOperation.IssueCreditMemo, options);

                    return result.done((operationResult) => { salesOrder = operationResult.data; });
                }).enqueue(() => {
                    return this.checkOutCartSuccess(salesOrder).done((data) => { receipts = data; });
                });

            return asyncQueue.run().map(() => { return receipts; });
        }

        /**
         * Updates the customer loyalty card observable in the view model with the loyalty card information for the customer.
         *
         * @param {string} customerId The customer.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateCustomerLoyaltyCards(
            customerId: string): IVoidAsyncResult {
            var asyncResult = new VoidAsyncResult();

            // Get the customer loyalty card information and set it in the view model
            if (!StringExtensions.isNullOrWhitespace(customerId)) {
                asyncResult.resolveOrRejectOn(this.customerManager.getCustomerLoyaltyCardsAsync(customerId)
                    .done((customerLoyaltyCards: Proxy.Entities.LoyaltyCard[]) => {
                        this.customerLoyaltyCards(customerLoyaltyCards);
                    })
                    .fail((errors: Proxy.Entities.Error[]) => {
                        RetailLogger.viewModelGetCustomerLoyaltyCardsFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                    }));
            } else {
                // If customer is null or undefined then update it anyway to trigger UI change.
                this.customerLoyaltyCards(null);
                asyncResult.resolve();
            }

            return asyncResult;
        }

        /**
         * Updates the customer observables in the view model with the information for the customer.
         * Includes customer balance, credit limit, loyalty cards, invoice addresses, and primary address.
         *
         * @param {Proxy.Entities.Customer} customerObject The customer.
         * @param {Proxy.Entities.Address} primaryAddress The primary address of the customer.
         * @return {IVoidAsyncResult} The async result.
         */
        private updateCustomerObservables(
            customerObject: Proxy.Entities.Customer,
            primaryAddress: Proxy.Entities.Address): IVoidAsyncResult {
            var invoiceAddress: Proxy.Entities.Address[] = [];
            var asyncResult = new VoidAsyncResult();

            this.customer(null); // Reset customer to prevent actions on stale data.

            if (!ObjectExtensions.isNullOrUndefined(customerObject)) {
                // Clear the customer balance and credit limit from the customer object to prevent displaying incorrect/out of date information.
                customerObject.Balance = null;
                customerObject.CreditLimit = null;
                this.customer(customerObject);

                // Get and update the customer balances
                var getCustomerBalanceAsyncResult: IAsyncResult<Entities.CustomerBalances> = this.customerManager.getCustomerBalanceAsync(customerObject.AccountNumber, customerObject.InvoiceAccount)
                    .done((customerBalances: Proxy.Entities.CustomerBalances) => {
                        this.customerBalance(customerBalances);
                        customerObject.Balance = CustomerHelper.getCustomerBalance(customerObject, customerBalances);
                        customerObject.CreditLimit = CustomerHelper.getCustomerCreditLimit(customerObject, customerBalances);
                    }).fail((errors: Proxy.Entities.Error[]) => {
                        if (errors[0].ErrorCode.toUpperCase() !==
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED.serverErrorCode) {
                            RetailLogger.viewModelGetCustomerBalanceFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));
                        }
                    });

                // Get and update the customer loyalty cards
                var getCustomerLoyaltyCardsAsyncResult: IVoidAsyncResult = this.updateCustomerLoyaltyCards(customerObject.AccountNumber);

                // Update customer only after async results return to avoid race condition on UI binding which is tracking customer update.
                VoidAsyncResult.join([getCustomerBalanceAsyncResult, getCustomerLoyaltyCardsAsyncResult])
                    .always(() => {
                        this.customer(customerObject);
                        asyncResult.resolve();
                    });
            } else {
                // If customer is null or undefined then update it anyway to trigger UI change.
                this.customer(customerObject);
                asyncResult.resolve();
            }

            if (!ObjectExtensions.isNullOrUndefined(customerObject)
                && ArrayExtensions.hasElements(customerObject.Addresses)) {
                invoiceAddress = customerObject.Addresses.filter((address: Proxy.Entities.Address) => {
                    return address.AddressTypeValue === Proxy.Entities.AddressType.Invoice;
                });
            }

            this.customerInvoiceAddress(invoiceAddress.length > 0 ? invoiceAddress[0] : null);
            this.customerAddress(primaryAddress);

            return asyncResult;
        }

        /**
         * Gets the descriptions for the kit components that are a part of the cart line.
         * @param {Proxy.Entities.CartLine} The cart line.
         * @return {ObservableArray<string>} The component descriptions.
         */
        private getKitComponentDescriptions(cartLine: Proxy.Entities.CartLine): ObservableArray<string> {
            var product: Proxy.Entities.SimpleProduct = Session.instance.getFromProductsInCartCache(cartLine.ProductId);
            var kitComponentDescriptions: string[] = [];
            var isKit: boolean =
                !ObjectExtensions.isNullOrUndefined(product)
                && (product.ProductTypeValue === Entities.ProductType.KitMaster || product.ProductTypeValue === Entities.ProductType.KitVariant);

            // check if the cart line product is a kit
            if (isKit) {
                // {0} {1} × {2} {3}
                var componentDescriptionFormatString: string = ViewModelAdapter.getResourceString("string_4386");
                var componentVariantDescriptionFormatString: string = ViewModelAdapter.getResourceString("string_4387");
                // for each of the component of the kit variant, we should get the name and the quantity associated
                product.Components.forEach((component: Proxy.Entities.ProductComponent) => {
                    var variantInformation: string = SimpleProductHelper.getVariantDescription(component);
                    // If there is variant information, then format the variant description.
                    if (!StringExtensions.isEmptyOrWhitespace(variantInformation)) {
                        variantInformation = StringExtensions.format(componentVariantDescriptionFormatString, variantInformation);
                    }

                    var componentDescription: string =
                        StringExtensions.format(
                            componentDescriptionFormatString,
                            component.Quantity.toString(),
                            component.UnitOfMeasure,
                            component.Name,
                            variantInformation);

                    kitComponentDescriptions.push(componentDescription);
                });
            }

            return ko.observableArray(kitComponentDescriptions);
        }

        private checkOutCartSuccess(salesOrder: Proxy.Entities.SalesOrder, isPickingUpProducts?: boolean): IAsyncResult<Proxy.Entities.Receipt[]> {
            this.lastAmountDue(this.cart().AmountDue + this.cart().AmountPaid);
            this.lastCustomerOrderMode = this.cart().CustomerOrderModeValue;

            this.cart(Session.instance.cart);

            this.originalCartLines(<Proxy.Entities.CartLine[]>[]);
            this.incomeExpenseAccountLines(<Proxy.Entities.IncomeExpenseAccount[]>[]);
            this.tenderLines(<Proxy.Entities.CartTenderLineTenderType[]>[]);

            this.lastSalesTransaction(salesOrder);

            var tenderLines = salesOrder.TenderLines;

            var changeAmount: number = ArrayExtensions.sum(
                tenderLines,
                (tenderLine: Proxy.Entities.TenderLine) => -1 * (tenderLine.Amount || 0),
                (tenderLine: Proxy.Entities.TenderLine) => tenderLine.IsChangeLine);
            this.lastChangeAmount(changeAmount);

            // Get all the change tender lines and use the last one to get the change tender type name.
            var changeTenderLines: Proxy.Entities.TenderLine[] = tenderLines.filter((tenderLine: Proxy.Entities.TenderLine) => tenderLine.IsChangeLine);
            if (ArrayExtensions.hasElements(changeTenderLines)) {
                this.changeTenderTypeName(ApplicationContext.Instance.tenderTypesMap.getTenderByTypeId(changeTenderLines[changeTenderLines.length - 1].TenderTypeId).Name);
            }

            this.lastTotalAmountPaid(NumberExtensions.roundToNDigits(salesOrder.AmountPaid + changeAmount, NumberExtensions.getDecimalPrecision()));
            this.salesPaymentDifference(salesOrder.SalesPaymentDifference);

            if (!isPickingUpProducts) {
                return this.salesOrderManager.getReceiptsForPrintAsync(
                    salesOrder.Id, false, Proxy.Entities.ReceiptType.Unknown, false, null, null, null, null, ApplicationContext.Instance.hardwareProfile.ProfileId);
            } else {
                return this.salesOrderManager.getReceiptsForPrintAsync(
                    salesOrder.Id, false, Proxy.Entities.ReceiptType.PickupReceipt, false, null, null, null, null, ApplicationContext.Instance.hardwareProfile.ProfileId);
            }
        }

        /**
         * Gets customer full name.
         *
         * @returns {string} customer full name.
         */
        private getCustomerFullName(): string {
            var customer = this.customer();
            var fullName: string = '';

            if (customer) {
                fullName = customer.Name;
            }

            return fullName;
        }

        private computedCartLinesTotalCount(): number {
            var cart: Proxy.Entities.Cart = this.cart();
            var count: number = 0;

            if (CartHelper.isCartType(cart, Proxy.Entities.CartType.IncomeExpense)) {
                count = ArrayExtensions.hasElements(cart.IncomeExpenseLines) ? cart.IncomeExpenseLines.length : 0;
            } else if (CartHelper.isCartType(cart, Proxy.Entities.CartType.AccountDeposit)) {
                count = ArrayExtensions.hasElements(cart.CustomerAccountDepositLines) ? cart.CustomerAccountDepositLines.length : 0;
            } else {
                count = ArrayExtensions.hasElements(cart.CartLines) ? CartHelper.GetNonVoidedCartLines(cart.CartLines).length : 0;
            }

            return count;
        }

        /**
         * Computes the tender lines total count.
         *
         * @return {number} The total count.
         */
        private computedTenderLinesTotalCount(): number {
            if (ArrayExtensions.hasElements(this.cart().TenderLines)) {
                return this.cart().TenderLines.length;
            }

            return 0;
        }

        /**
         * Adds products to the cart.
         */
        public addProductsToCart(productSaleDetails: Proxy.Entities.ProductSaleReturnDetails[]): IAsyncResult<ICancelableResult> {
            productSaleDetails = productSaleDetails || [];

            var options: Operations.IItemSaleOperationOptions = {
                productSaleDetails: productSaleDetails
            };

            var operationResult: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(Operations.RetailOperation.ItemSale, options);

            return operationResult.done((result: ICancelableResult) => {
                if (result && !result.canceled) {
                    this.cart(Session.instance.cart);
                }
            });
        }

        /**
         * Manually return products.
         *
         * @param {Proxy.Entities.ProductSaleReturnDetails[]} productReturnDetails The products to return.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        private returnProducts(productReturnDetails: Proxy.Entities.ProductSaleReturnDetails[]): IAsyncResult<ICancelableResult> {
            productReturnDetails = productReturnDetails || [];

            var options: Operations.IReturnProductOperationOptions = {
                customerId: this.cart().CustomerId,
                productReturnDetails: productReturnDetails.map(p => {
                    return <Proxy.Entities.ProductReturnDetails>{ manualReturn: p }
                })
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ReturnItem, options);

            return operationResult.done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Returns the selected cart lines.
         *
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public returnCartLines(): IAsyncResult<ICancelableResult> {
            var options: Operations.IReturnProductOperationOptions = {
                customerId: this.cart().CustomerId,
                productReturnDetails: this.selectedCartLines().map(cartLine => {
                    return <Proxy.Entities.ProductReturnDetails>{ cartLine: cartLine };
                })
            };

            var operationResult = this.operationsManager.runOperation(
                Operations.RetailOperation.ReturnItem, options);

            return operationResult.done(() => { this.cart(Session.instance.cart); });
        }        

        /**
         * Performs a customer account deposit.
         * @returns {IVoidAsyncResult} The async result.
         */
        public customerAccountDeposit(): IVoidAsyncResult {
            return this.operationsManager.runOperation(
                Operations.RetailOperation.CustomerAccountDeposit, {})
                .done((result: ICancelableResult) => {
                    if (result && !result.canceled) {
                        this.cart(Session.instance.cart)
                    }
                });
        }

        /**
         * Overrides the prices on the selected cart lines.
         * @param {number[]} [newPrices] The optional collection of prices to be set on the cart lines.
         * @return {IAsyncResult<ICancelableResult>} The cancelable async result.
         */
        public priceOverride(newPrices?: number[]): IAsyncResult<ICancelableResult> {
            newPrices = newPrices || [];

            var options: Operations.IPriceOverrideOperationOptions = {
                cartLinePrices: this.selectedCartLines().map((c: Proxy.Entities.CartLine, index: number) => {
                    return { cartLine: c, price: newPrices[index] };
                })
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.PriceOverride, options)
                .done((result: ICancelableResult) => {
                    if (!result.canceled) {
                        this.cart(Session.instance.cart);
                    }
                });
        }

        /**
         * Changes the sales person on the current sales order
         *
         * @param {IVoidAsyncResult} The async result.
         */
        public changeSalesPerson(): IVoidAsyncResult {
            var changeSalesPersonOperationParameters: Operations.ISalesPersonOperationOptions = {
                cart: this.cart()
            }

            return this.operationsManager.runOperation(
                Operations.RetailOperation.SalesPerson,
                changeSalesPersonOperationParameters)
                .done(() => { this.cart(Session.instance.cart); });
        }

        

        /**
         * Checks if cart or sales lines available for pick up
         *
         * @param {string} deliveryMode Line delivery mode
         * @param {string} storeNumber Line store number
         * @return {boolean} Returns is line available to pick up in current store or not
         */
        public isLineAvailableForPickUp(deliveryMode: string, storeNumber: string): boolean {
            return deliveryMode === ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode && storeNumber === ApplicationContext.Instance.storeNumber;
        }

        /**
         * Gets cart or sales lines available for pick up
         *
         * @param {Proxy.Entities.CartLine[]} cartLines Cart lines
         * @return {Proxy.Entities.CartLine[} Returns lines available for pick up in current store
         */
        public getLinesAvailableForPickUp(cartLines: Proxy.Entities.CartLine[]): Proxy.Entities.CartLine[] {
            var lines: Proxy.Entities.CartLine[] = cartLines.filter((line: Proxy.Entities.CartLine) => {
                return this.isLineAvailableForPickUp(line.DeliveryMode, line.FulfillmentStoreId);
            });

            // set max available quantity for pick up
            lines.forEach((line: Proxy.Entities.CartLine) => {
                line.Quantity = line.QuantityOrdered - line.QuantityInvoiced;
            });

            return lines;
        }

        /**
         * Picks up cart lines
         *
         * @param {Proxy.Entities.CartLine[]} cartLines cart lines to pick up
         * @param {IVoidAsyncResult} The async result.
         */
        public pickUpCartLines(cartLines: Proxy.Entities.CartLine[]): IAsyncResult<ICancelableResult> {

            // validate quantity to pick
            cartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
                cartLine.Quantity = Math.min(cartLine.Quantity, cartLine.QuantityOrdered - cartLine.QuantityInvoiced);
            });
            
            var nonSelectedCartLines: Proxy.Entities.CartLine[] =
                ArrayExtensions.difference<Proxy.Entities.CartLine>(
                    this.cart().CartLines,
                    cartLines,
                    (left: Proxy.Entities.CartLine, right: Proxy.Entities.CartLine) => left.LineId == right.LineId);

            nonSelectedCartLines.forEach((cartLine: Proxy.Entities.CartLine) => cartLine.Quantity = 0);

            if (cartLines.length == 0) {
                var error = new Proxy.Entities.Error(ErrorTypeEnum.CART_LINES_UNAVAILABLE_FOR_PICK_UP);
                return VoidAsyncResult.createRejected([error]);
            }

            ////DEMO4 //TODO:AM
            ////Apply discounts only for Last line(s)
            //if (!this.isLastLine(cartLines)) {
            //    cartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
            //        let discountAmount: number = cartLine.DiscountAmount;
            //        cartLine.LineManualDiscountAmount = (discountAmount - 0.01) * -1;
            //        //cartLine.DiscountAmount = -1 * discountAmount;
            //        cartLine.LineDiscount = (discountAmount - 0.01) * -1;
            //    });
            //} else {
            //    this.applyDiscountsToLastLine(cartLines);
            //}

            var options: Operations.IUpdateCustomerOrderOperationOptions = {
                operationType: Proxy.Entities.CustomerOrderOperations.PickUpFromStore,
                parameters: { PickUpInStoreParameter: { CartLines: this.cart().CartLines } }
            };

            var result: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                Operations.RetailOperation.EditCustomerOrder, options);

            return result.done((result: ICancelableResult) => { this.cart(Session.instance.cart); });
        }

        //DEMO 4 //TODO:AM

        public applyDiscountsToLastLine(cartLines: Proxy.Entities.CartLine[]) {
            let totalDiscountsToApply: number = 0;

            //Find All non selected lines
            let nonSelectedCartlines: Proxy.Entities.CartLine[] =
                ArrayExtensions.difference<Proxy.Entities.CartLine>(
                    this.cart().CartLines,
                    cartLines,
                    (left: Proxy.Entities.CartLine, right: Proxy.Entities.CartLine) => left.LineId == right.LineId);

            
            cartLines.forEach((cartLine: Proxy.Entities.CartLine) => {
                totalDiscountsToApply += cartLine.DiscountAmount;
            });

            nonSelectedCartlines.forEach((cartLine: Proxy.Entities.CartLine) => {
                totalDiscountsToApply += cartLine.DiscountAmount;
            });

            //Apply discount to the last line
            let lastCartLine: Proxy.Entities.CartLine = cartLines[0];
            lastCartLine.LineManualDiscountAmount = totalDiscountsToApply;
        }

        //Find if a last item(s) is to be picked up
        public isLastLine(cartLines: Proxy.Entities.CartLine[]): boolean {
            let returnValue: boolean = false;
            let counter: number = 0;

            let nonSelectedCartlines: Proxy.Entities.CartLine[] =
                ArrayExtensions.difference<Proxy.Entities.CartLine>(
                    this.cart().CartLines,
                    cartLines,
                    (left: Proxy.Entities.CartLine, right: Proxy.Entities.CartLine) => left.LineId == right.LineId);

            //If all other lines are invoiced, then it is the last line(s) Pickup
            nonSelectedCartlines.forEach((cartLine: Proxy.Entities.CartLine) => {
                if (cartLine.QuantityInvoiced > 0)
                    counter++;
            });

            if (nonSelectedCartlines.length === counter)
                returnValue = true;

            return returnValue;
        }

        // APPLY ZERO DISCOUNTS TO PICKUP LINES FOR LAYAWAY
        public applyZeroDiscountsToPickupLinesForLayaway() : IVoidAsyncResult { 
            // Remove line discount for pickup
            var newcart: Proxy.Entities.Cart = Session.instance.cart;
            if (newcart.DeliveryMode === ApplicationContext.Instance.channelConfiguration.PickupDeliveryModeCode) {

                //get cart lines for pickup
                var cartLinesToUpdate: Proxy.Entities.CartLine[] = [];
                newcart.CartLines.forEach((cartLine1: Proxy.Entities.CartLine) => {
                    if (cartLine1.Quantity > 0) {
                        cartLine1.LineManualDiscountAmount = 0;
                        cartLinesToUpdate.push(cartLine1);
                    }
                });

                return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                   
                    return this.cartManager.updateCartLinesOnCartAsync(cartLinesToUpdate);
               

                    //return VoidAsyncResult.createResolved();
                }).run();

                //apply zero discounts on line
                //var options1: Operations.ILineDiscountOperationOptions = {
                //    cartLineDiscounts: cartLinesToUpdate.map((cartLine, index) => {
                //        return { cartLine: cartLine, discountValue: 0 };
                //    })
                //};

                
                //var result1: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(
                //    Operations.RetailOperation.LineDiscountAmount, options1);

                //result1.done((result1: Operations.IOperationResult) => {
                //    if (!result1.canceled) {
                //        this.cart(Session.instance.cart);
                //    }
                //});
            }
        }
        //DEMO4 END

        /**
         * Validates properties given before executing customer order related operations.
         *
         * @param {Proxy.Entities.CartLine[]} cartLines The selected cart lines.
         * @param {Operations.RetailOperation} retailOperationId Retail operations identifier, should be related with customer orders.
         * @param {IAsyncResult<ICancelable>} The async result.
         */
        public customerOrderPreExecuteOperation(cartLines: Proxy.Entities.CartLine[],
            retailOperationId: Operations.RetailOperation): IAsyncResult<ICancelableResult> {

            var cart: Proxy.Entities.Cart = this.cart();
            var errors: Proxy.Entities.Error[] = Operations.CreateCustomerOrderOperationHandler.preOperationValidation(cart);
            if (ArrayExtensions.hasElements(errors)) {
                return AsyncResult.createRejected(errors);
            }

            var asyncQueue = new AsyncQueue();
            var purposeOperationId: Proxy.Entities.CustomerOrderMode;
            var isOperationsForShipOrPickup = retailOperationId === Operations.RetailOperation.PickupAllProducts
                || retailOperationId === Operations.RetailOperation.PickupSelectedProducts
                || retailOperationId === Operations.RetailOperation.ShipAllProducts
                || retailOperationId === Operations.RetailOperation.ShipSelectedProducts;

            var isShipOrPickupSelected: boolean = retailOperationId === Operations.RetailOperation.PickupSelectedProducts
                || retailOperationId === Operations.RetailOperation.ShipSelectedProducts;

            if (isOperationsForShipOrPickup) {
                asyncQueue.enqueue(() => {
                    return DeliveryHelper.validateCartForShippingOrPickup(cart, false);
                });
            }

            if (isShipOrPickupSelected) {
                asyncQueue.enqueue(() => {
                    return DeliveryHelper.validateCartLinesForShippingOrPickup(cart, cartLines);
                });
            }

            if (isShipOrPickupSelected && DeliveryHelper.mustClearHeaderDeliveryInfo(cart, cartLines)) {

                asyncQueue.enqueue(() => {
                    // A dialog that asks user that will clear delivery line header
                    // if he / she clicks OK button.
                    return ViewModelAdapter.displayMessage(ViewModelAdapter.getResourceString("string_4450"),
                        MessageType.Info, MessageBoxButtons.YesNo)
                        .done((result: DialogResult) => {
                            if (result === DialogResult.No) {
                                asyncQueue.cancel();
                                return;
                            }
                        });
                });
            }

            // Add more async queue operations when cart type is not customer order.
            // When the cart being shipped / picked is not a customer order, automatically
            // ask the user to make it a customer order / quote
            if (!CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {

                asyncQueue
                    .enqueue(() => {
                        // A dialog that asks user to convert cart to customer order or quote.
                        var activity: Activities.GetOrderTypeActivity = new Activities.GetOrderTypeActivity({ operationId: retailOperationId });
                        return activity.execute().done(() => {
                            if (!activity.response) {
                                asyncQueue.cancel();
                                return;
                            }

                            purposeOperationId = activity.response.customerOrderMode;
                        });
                    }).enqueue(() => {
                        // Run operation to convert cart to become customer order / quotation.
                        switch (purposeOperationId) {
                            case Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit:
                                return this.createCustomerOrder();
                            case Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit:
                                var quotationAsyncQueue = this.createQuotationAndSetExpirationDateAsyncQueue();
                                return asyncQueue.cancelOn(quotationAsyncQueue.run());
                            default:
                                asyncQueue.cancel();
                                return;
                        }
                    });
            }

            return asyncQueue.run();
        }

        /**
         *  Converts the current cart to a customer order. If current cart represents a quote, it converts the customer quote into a customer order.
         *  return {IVoidAsyncResult} The void async result.
         */
        public createCustomerOrder(): IVoidAsyncResult {

            var options: Operations.ICreateCustomerOrderOperationOptions = {
                cart: this.cart()
            };

            return this.operationsManager.runOperation(Operations.RetailOperation.CreateCustomerOrder, options)
                .done((results: Operations.IOperationResult) => { this.cart(results.data); });
        }

        /**
         * Converts the current cart to a quotation.
         * @param {Date} [expirationDate] The quotation expiration date.
         * @returns {IVoidAsyncResult} The async result.
         */
        public createQuotation(expirationDate?: Date): IVoidAsyncResult {
            var quoteCreationOperationParameters: Operations.ICreateCustomerQuoteOperationOptions = {
                cart: this.cart(),
                quotationExpirationDate: expirationDate
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.CreateQuotation, quoteCreationOperationParameters)
                .done(() => {
                    this.cart(Session.instance.cart);
                });
        }

        /**
         * Sets the quotation expiration date.
         *
         * @param {Date} [requestedExpirationDate] The requested quotation expiration date.
         * @returns {IVoidAsyncResult} The async result.
         */
        public setQuotationExpirationDate(requestedExpirationDate?: Date): IVoidAsyncResult {
            var options: Operations.ISetQuotationExpirationDateOperationOptions = {
                cart: this.cart(),
                requestedExpirationDate: requestedExpirationDate
            };

            return this.operationsManager.runOperation(
                Operations.RetailOperation.SetQuotationExpirationDate, options)
                .done(() => {
                    this.cart(Session.instance.cart);
                });
        }

        /**
         * Create quotation and set expiration date.
         * The sequence is as follows:
         * - Get quotation expiration date dialog
         * - Run operation create quotation
         * - Run operation set quotation expiration date
         * This method is executed when cart type is Shopping.
         * When cart is going to be converted to become quotation, client must be shown
         * the expiration date dialog since expiration date is needed to create quotation
         * successfully in the headquarters.
         *
         * @returns {AsyncQueue} The async queue to be executed.
         */
        public createQuotationAndSetExpirationDateAsyncQueue(): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            var expirationDate: Date = null;

            asyncQueue
                .enqueue(() => {
                    // Get quotation expiration date dialog.
                    // This dialog will be first shown, even though the expiration date
                    // will be later be used by Set Quotation Expiration Date operation.
                    // This way, when user clicks 'Cancel' on the dialog,
                    // The Async Queue will be canceled and cart won't be converted to become quotation.
                    return CustomerOrderHelper.getQuotationExpirationDate()
                        .done((quotationExpirationDate: Date) => {
                            if (ObjectExtensions.isNullOrUndefined(quotationExpirationDate)) {
                                asyncQueue.cancel();
                                return;
                            }

                            expirationDate = quotationExpirationDate;
                        });
                }).enqueue(() => {
                    return this.createQuotation(expirationDate);
                });

            return asyncQueue;
        }

        /**
         * Checks whether cart has a customer
         *
         * @return {boolean} Returns true if cart has a customer
         */
        public get cartHasCustomer(): boolean {
            var cart: Proxy.Entities.Cart = this.cart();
            var customer: Proxy.Entities.Customer = this.customer();

            var isCartCreated: boolean = !ObjectExtensions.isNullOrUndefined(cart) && !StringExtensions.isNullOrWhitespace(cart.Id);
            var cartHasCustomer: boolean = !ObjectExtensions.isNullOrUndefined(customer) && !StringExtensions.isNullOrWhitespace(customer.AccountNumber);
            return isCartCreated && cartHasCustomer;
        }

        /**
         * Clear customer from the cart
         *
         * @return {IVoidAsyncResult } async result.
         */
        public removeCustomerFromCart(): IVoidAsyncResult {
            return this.operationsManager.runOperation(Operations.RetailOperation.CustomerClear, this)
                .done(() => { this.cart(Session.instance.cart); });
        }

        /**
         * Clears a cart in session.
         */
        public removeCartFromSession(): void {
            Session.instance.clearCart();
            this.cart(Session.instance.cart);
        }

        private cartUpdateHandlerAsync(newCart: Proxy.Entities.Cart): IVoidAsyncResult {
            var asyncResults: IVoidAsyncResult[] = [];
            var customer: Proxy.Entities.Customer = this.customer();

            if (ObjectExtensions.isNullOrUndefined(customer) || customer.AccountNumber != newCart.CustomerId) {
                asyncResults.push(this.getCustomerDetails(newCart.CustomerId));
            } else if (!ObjectExtensions.isNullOrUndefined(Session.instance.Customer) &&
                Session.instance.Customer.AccountNumber === newCart.CustomerId) {
                this.customer(Session.instance.Customer);
            }

            // If there is no cart in progress then clear the reason codes in cart cache.
            if (!Session.instance.isCartInProgress) {
                this._reasonCodesInCartByReasonCodeId.clear();
            }

            asyncResults.push(this.loadListingsFromCartLinesAsync(newCart));

            this.refreshCartTenderTypes(newCart);

            // whenever the cart is updated, we assume manual change is not required anymore, if it is, server will send the appropriate resourceId
            // this is required to allow checkout after the manual refund has happened
            this._isManualChangeRequired = false;

            return VoidAsyncResult.join(asyncResults);
        }

        /**
         * Determines whether the drawer should open for this cart when it is checked out.
         *
         * @return {boolean} True if the drawer should open. False otherwise.
         */
        public shouldOpenDrawer(): boolean {
            if (!Peripherals.HardwareStation.HardwareStationContext.instance.isActive()) {
                return false;
            } else if (this.lastChangeAmount() > 0) {
                return true;
            }

            var salesOrder: Proxy.Entities.SalesOrder = this.lastSalesTransaction();
            if (!ObjectExtensions.isNullOrUndefined(salesOrder)
                && ArrayExtensions.hasElements(salesOrder.TenderLines)) {

                return salesOrder.TenderLines.some((tenderLine: Proxy.Entities.CartTenderLine) => {
                    var tenderType: Proxy.Entities.TenderType = null;

                    // ignores all historical tender lines, they are shown on a separate view
                    if (!tenderLine.IsHistorical) {
                        tenderType = ApplicationContext.Instance.tenderTypesMap.getTenderByTypeId(tenderLine.TenderTypeId);

                        if (!ObjectExtensions.isNullOrUndefined(tenderType) && tenderType.OpenDrawer) {
                            return true;
                        }
                    }

                    return false;
                });
            }

            return false;
        }

        /**
         * Formats the reason code line description for each of the info code lines provided.
         *
         * @param {Entities.ReasonCodeLine[]} reasonCodeLine The collection of reasoncodelines to get reason code out of.
         * @return {Observable<string>[]} The array of reason code description strings.
         */
        private formatReasonCodeLineDescriptions(reasonCodeLines: Entities.ReasonCodeLine[]): Observable<string>[] {
            var reasonCodeLineDescriptions: Observable<string>[] = [];
            var separatorFormat: string = ViewModelAdapter.getResourceString("string_198"); // "{0} - {1}"
            var formatInfoCodeDescription: (reasonCode: Entities.ReasonCode, reasonCodeLine: Entities.ReasonCodeLine) => string =
                (reasonCode: Entities.ReasonCode, reasonCodeLine: Entities.ReasonCodeLine): string => {
                    if (!ObjectExtensions.isNullOrUndefined(reasonCode)) {
                        var reasonCodeInfo: string[] = [];

                        if (!StringExtensions.isEmptyOrWhitespace(reasonCode.Prompt)) {
                            reasonCodeInfo.push(reasonCode.Prompt);
                        }

                        if (!StringExtensions.isEmptyOrWhitespace(reasonCodeLine.SubReasonCodeId)) {
                            reasonCodeInfo.push(reasonCodeLine.SubReasonCodeId);
                        }

                        if (!StringExtensions.isEmptyOrWhitespace(reasonCodeLine.Information)) {
                            reasonCodeInfo.push(reasonCodeLine.Information);
                        }

                        if (ArrayExtensions.hasElements(reasonCodeInfo)) {
                            var displayString: string = reasonCodeInfo[0];
                            for (var i = 1; i < reasonCodeInfo.length; i++) {
                                displayString = StringExtensions.format(separatorFormat, displayString, reasonCodeInfo[i]);
                            }

                            return displayString;
                        }
                    }

                    return StringExtensions.EMPTY;
                };

            reasonCodeLines.forEach((reasonCodeLine: Proxy.Entities.ReasonCodeLine) => {
                var reasonCodeDescription: Observable<string> = ko.observable<string>("");
                var cachedReasonCode: Entities.ReasonCode = this._reasonCodesInCartByReasonCodeId.getItem(reasonCodeLine.ReasonCodeId);

                // If a reason code was not found in the cache make a call to retrieve the reason code. 
                if (ObjectExtensions.isNullOrUndefined(cachedReasonCode)) {
                    this.getReasonCodeById(reasonCodeLine.ReasonCodeId).done((reasonCode: Entities.ReasonCode) => {
                        reasonCodeDescription(formatInfoCodeDescription(reasonCode, reasonCodeLine));
                    });
                } else {
                    reasonCodeDescription(formatInfoCodeDescription(cachedReasonCode, reasonCodeLine));
                }

                reasonCodeLineDescriptions.push(reasonCodeDescription);
            });

            return reasonCodeLineDescriptions;
        }
    }
}
