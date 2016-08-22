/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ApplicationStorage.ts'/>
///<reference path='Entities/CommerceTypes.g.ts'/>
///<reference path='Entities/Store.ts'/>
///<reference path='Entities/StoreOperations.ts'/>
///<reference path='Extensions/ObjectExtensions.ts'/>
///<reference path='Extensions/StringExtensions.ts'/>
///<reference path='Peripherals/IPeripherals.ts'/>

module Commerce {
    "use strict";

    /**
     * Class for representing cart update handlers
     */
    export interface ICartUpdateHandler {
        isAlivePage?: boolean;
        isGlobal?: boolean;
        updateCartHandler: (cartStateType?: CartStateType) => void;
    }

    /**
     * Connection status type.
     */
    export enum ConnectionStatusType {
        Online = 0,
        SeamlessOffline = 1,
        ManualOffline = 2
    }

    /**
     * Offline availability status.
     */
    export enum ConnectionAvailabilityStatus {
        Unknown = 0,
        NotEnabled = 1,
        EnabledNotAvailable = 2,
        Available = 3
    }

    /**
     * Cart state type.
     */
    export enum CartStateType {
        None = 0,
        Started = 1,
        Updated = 2,
        Completed = 3,
        Reloaded = 4
    }

    /**
     * Offline sync parameters.
     */
    export class OfflineParameters {
        public syncDownloadOfflineData: number; // The timer of download offline sync.
        public syncUploadOfflineData: number; // The timer of upload offline sync.
        public offlineModeDisabled: boolean; // The indication that if offline mode is disabled for this terminal.

        constructor(syncDownloadOfflineData: number, syncUploadOfflineData: number, offlineModeDisabled: boolean) {
            this.syncDownloadOfflineData = syncDownloadOfflineData;
            this.syncUploadOfflineData = syncUploadOfflineData;
            this.offlineModeDisabled = offlineModeDisabled;
        }
    }

    /**
     * The list of errors that can be shown/not shown per session
     */
    export enum ErrorsDisplayedPerSession {
        PaymentTerminalBeginTransaction,  // The error that occurs on each call to payment terminal begin transaction.
        CardPaymentBeginTransaction, // The error that occurs on each call to card payment begin transaction.
        HardwareStationGeneralError // The general error that occurs on hardware station issues.
    }

    /**
     * Class for storing data that is shared across pages (e.g. Cart).
     */
    export class Session {
        private static _instance: Commerce.Session = null;
        private static _cartStateUpdateEvent = "CartStateUpdateEvent";

        private _cart: Proxy.Entities.Cart;
        private _productsInCart: Dictionary<Proxy.Entities.SimpleProduct>;
        private _offlineParameters: OfflineParameters;
        private _connectionStatus: ConnectionStatusType;
        private _connectionAvailabilityStatus: ConnectionAvailabilityStatus = ConnectionAvailabilityStatus.Unknown;
        private _isOfflineAvailable: boolean = false;
        private _shift: Proxy.Entities.Shift;
        private _customer: Proxy.Entities.Customer = { AccountNumber: "" };
        private _customerPrimaryAddress: Proxy.Entities.Address = {};
        private _productCatalogStore: Proxy.Entities.ProductCatalogStore = {};
        private _currentCategories: Proxy.Entities.Category[];

        public categoryTree: ObservableArray<any>;
        public userName: Observable<string>;
        public picture: Observable<string>;
        public shiftId: Observable<number>;
        public shiftTerminalId: Observable<string>;
        public terminalId: Observable<string>;
        public connectionStatusAsString: Observable<string>;
        public offlineSyncing: Observable<boolean>;
        public pendingDownloadSessionCount: Observable<number>;
        public isSessionStateValid: boolean;
        public messageDialogAsyncResult: Array<IAsyncResult<DialogResult>>;
        public catalogName: string;
        public defaultCatalogImageFormat: string = "";
        public employee: Observable<Proxy.Entities.Employee>;
        public cartReloadedNotificationPending: boolean;

        private _errorDisplayStatesPerSession: Dictionary<boolean> = new Dictionary<boolean>();

        public showPaymentDeviceBeginTransactionError: boolean = true; // Indicates whether the show the payment device error on begin transaction. The user can turn off the error display in a session.

        constructor() {
            this.createSession();
            this.resetSession();
        }

        /**
         * Get the instance of session.
         */
        public static get instance(): Session {
            if (ObjectExtensions.isNullOrUndefined(Session._instance)) {
                Session._instance = new Session();
            }

            return Session._instance;
        }

        /**
         * Resets all the values in the Session to their default value.
         */
        public resetSession(): void {
            this.userName("");
            this.picture("");
            this.shiftId(0);
            this.shiftTerminalId("");
            this.terminalId("");
            this.employee(null);
            this.isSessionStateValid = false;
            this._productsInCart.clear();
            this._offlineParameters = new OfflineParameters(0, 0, false);
            this.catalogName = StringExtensions.EMPTY;
            this._currentCategories = [];
            this.categoryTree([]);
            this.connectionStatus = Number(ApplicationStorage.getItem(ApplicationStorageIDs.CONNECTION_STATUS));
            this.offlineSyncing(false);
            this.pendingDownloadSessionCount(0);

            // Set all errors to be displayed by default
            for (var errorDisplayKey in ErrorsDisplayedPerSession) {
                this._errorDisplayStatesPerSession.setItem(errorDisplayKey, true);
            }

            // Load serialized shift and cart.
            var serializedShift: string = ApplicationStorage.getItem(ApplicationStorageIDs.SHIFT_KEY);
            this.Shift = serializedShift ? new Proxy.Entities.ShiftClass(JSON.parse(serializedShift)) : null;

            var serializedCart: string = ApplicationStorage.getItem(ApplicationStorageIDs.CART_KEY);
            this.cart = serializedCart ? new Proxy.Entities.CartClass(JSON.parse(serializedCart)) : null;
            this.cartReloadedNotificationPending = !StringExtensions.isNullOrWhitespace(this.cart.Id);

            this.refreshConnectionAvailabilityStatus();
        }

        /**
         * Method to add handler to be executed when the cart is updated.
         *
         * @param {Element} element DOM element that will host event handler and serve as trigger for event disposal.
         * @param {Function} updateCartStateHandler Cart update handler.
         */
        public AddCartStateUpdateHandler(element: Element, updateCartStateHandler: (cartStateType?: CartStateType, oldCart?: Proxy.Entities.Cart) => void): void {
            Commerce.ViewModelAdapter.addViewEvent(element, Session._cartStateUpdateEvent, updateCartStateHandler);
        }

        /**
         * Method removes all cart handlers except global ones.
         *
         * @param {Element} element DOM element that will host event handler and serve as trigger for event disposal.
         * @param {Function} updateCartStateHandler Cart update handler.
         */
        public RemoveCartStateUpdateHandler(element: Element, updateCartStateHandler: (cartStateType?: CartStateType) => void): void {
            Commerce.ViewModelAdapter.removeViewEvent(element, Session._cartStateUpdateEvent, updateCartStateHandler);
        }

        /**
         * Get whether employee is logged on from the session.
         */
        public get isLoggedOn(): boolean {
            return !(ObjectExtensions.isNullOrUndefined(Session.instance.employee()));
        }

        /**
         * Get whether a cart is in progress.
         */
        public get isCartInProgress(): boolean {
            return !(StringExtensions.isNullOrWhitespace(this.cart.Id)
                || ObjectExtensions.isNullOrUndefined(this.cart.CartStatusValue)
                || this.cart.CartStatusValue === CartStateType.None);
        }

        /**
         * Set the employee in the session.
         *
         * @param {Proxy.Entities.Employee} newEmployee New employee to be added.
         */
        public set CurrentEmployee(newEmployee: Proxy.Entities.Employee) {
            var oldEmployee: Proxy.Entities.Employee = this.employee();
            // Check whether the setting the current employee
            var isCurrentEmployee = !ObjectExtensions.isNullOrUndefined(oldEmployee)
                && !ObjectExtensions.isNullOrUndefined(newEmployee)
                && (oldEmployee.StaffId === newEmployee.StaffId);

            // Set the employee
            this.employee(newEmployee);

            if (!ObjectExtensions.isNullOrUndefined(newEmployee)) {
                this.userName(newEmployee.Name);
                if (Session.instance.connectionStatus == ConnectionStatusType.Online) {
                    if (ArrayExtensions.hasElements(newEmployee.Images) &&
                        !StringExtensions.isNullOrWhitespace(newEmployee.Images[0].Uri)) {
                        this.picture(newEmployee.Images[0].Uri);
                    }
                } else {
                    this.picture(newEmployee.DefaultImage);
                }

            } else {
                this.userName("");
                this.picture("");
            }

            // Set the user/login tracking state in the session for the employee if not already set
            if (!isCurrentEmployee) {
                this.setDefaultEmployeeSessionState();
            }
        }

        /**
         * Get the currently logged on employee from the session.
         */
        public get CurrentEmployee(): Proxy.Entities.Employee {
            return this.employee();
        }

        /**
         * Get current category list in context of the store and the catalog
         */
        public get CurrentCategoryList(): Proxy.Entities.Category[] {
            return Session.instance._currentCategories;
        }

        /**
         * Sets current category list in context of the store and the catalog
         */
        public set CurrentCategoryList(categories: Proxy.Entities.Category[]) {
            Session.instance._currentCategories = categories;
            this.arrangeCategories(Session.instance._currentCategories);
        }

        /**
         * Create a category list to be displayed in the navigation bar
         */
        public arrangeCategories(arry: any): void {
            var roots = [], children = {};

            for (var i = 0, len = arry.length; i < len; ++i) {
                var item = arry[i];
                var p = item.ParentCategory;
                var target = !p ? roots : (children[p] || (children[p] = []));
                target.push({ value: item });
            }

            var findChildren = function (parentCategory) {
                if (children[parentCategory.value.RecordId]) {
                    parentCategory.children = children[parentCategory.value.RecordId];
                    for (var i = 0, len = parentCategory.children.length; i < len; ++i) {
                        findChildren(parentCategory.children[i]);
                    }
                } else {
                    parentCategory.children = [];
                }
            };

            for (var i = 0, ltd = roots.length; i < ltd; ++i) {
                findChildren(roots[i]);
            }

            this.categoryTree(roots);
        }

        /**
         * Set the shift for the session.
         *
         * @param {Proxy.Entities.Shift} newShift The current shift for the session.
         */
        public set Shift(newShift: Proxy.Entities.Shift) {
            if (ObjectExtensions.isNullOrUndefined(newShift)) {
                newShift = new Proxy.Entities.ShiftClass({ ShiftId: 0, TerminalId: Commerce.ViewModelAdapter.getResourceString("string_4038") });
            }

            this._shift = newShift;
            this.shiftId(this._shift.ShiftId);
            this.shiftTerminalId(this._shift.TerminalId);
            this.terminalId(this._shift.CurrentTerminalId || this._shift.TerminalId);

            ApplicationStorage.setItem(ApplicationStorageIDs.SHIFT_KEY, JSON.stringify(newShift));
        }

        /**
         * Get the current shift from the session.
         */
        public get Shift(): Proxy.Entities.Shift {
            return Session.instance._shift;
        }

        /**
         * Set the customer in the session.
         *
         * @param {Proxy.Entities.Customer} newCustomer New customer to be added.
         */
        public set Customer(newCustomer: Proxy.Entities.Customer) {
            Session.instance._customer = newCustomer;
        }

        /**
         * Get the customer in the session.
         */
        public get Customer(): Proxy.Entities.Customer {
            return Session.instance._customer;
        }

        /**
         * Set the customer primary address in the session.
         *
         * @param {Proxy.Entities.Address} newCustomerAddress New customeraddress to be added.
         */
        public set CustomerPrimaryAddress(newAddress: Proxy.Entities.Address) {
            Session.instance._customerPrimaryAddress = newAddress;
        }

        /**
         * Get the customer primary address in the session.
         */
        public get CustomerPrimaryAddress(): Proxy.Entities.Address {
            return Session.instance._customerPrimaryAddress;
        }

        /**
         * Set the cart to the session.
         *
         * @param {Proxy.Entities.Cart} newCart New cart to be added.
         */
        public set cart(newCart: Proxy.Entities.Cart) {
            // we should never let the cart be null or undefined.
            if (ObjectExtensions.isNullOrUndefined(newCart) || StringExtensions.isNullOrWhitespace(newCart.Id)) {
                newCart = new Proxy.Entities.CartClass({ Id: StringExtensions.EMPTY, CartLines: [] });
            }

            var currentCart = this._cart;

            this._cart = newCart;
            // Store the cart in persisted store for reload if the application crashes
            ApplicationStorage.setItem(ApplicationStorageIDs.CART_KEY, JSON.stringify(newCart));

            var cartStateType: CartStateType = CartStateType.None;

            if ((!currentCart || StringExtensions.isNullOrWhitespace(currentCart.Id)) && !StringExtensions.isNullOrWhitespace(newCart.Id)) {
                cartStateType = CartStateType.Started;
            }
            else if (currentCart && !StringExtensions.isNullOrWhitespace(currentCart.Id) && StringExtensions.isNullOrWhitespace(newCart.Id)) {
                cartStateType = CartStateType.Completed;
            }
            else if (!StringExtensions.isNullOrWhitespace(newCart.Id)) {
                cartStateType = CartStateType.Updated;
            }

            Commerce.ViewModelAdapter.raiseViewEvent(Session._cartStateUpdateEvent, cartStateType, currentCart);
        }

        /**
         * Get the cart from session.
         */
        public get cart(): Proxy.Entities.Cart {
            return this._cart;
        }

        /**
         * Get the offline parameters used for offline data sync.
         */
        public get offlineParameters(): OfflineParameters {
            return Session.instance._offlineParameters;
        }

        /**
         * Stores the product object in the cache of products in the cart.
         * @remarks The product should only be stored using this method if it is a part of the current transaction.
         * @param {Proxy.Entities.SimpleProduct} product The product to store in the cache.
         */
        public addToProductsInCartCache(product: Proxy.Entities.SimpleProduct): void {
            this._productsInCart.setItem(product.RecordId, product);
        }

        /**
         * Gets the product object from the cache of products.
         * @remarks Only the products that are a part of the current transaction are available in the cart.
         * @param {number} productId The product identifier of the product to get.
         * @return {Proxy.Entities.SimpleProduct} The product, if it is found. Null otherwise.
         */
        public getFromProductsInCartCache(productId: number): Proxy.Entities.SimpleProduct {
            return this._productsInCart.getItem(productId);
        }

        /**
         * Clears cart from the session and associated elements.
         */
        public clearCart(): void {
            // clear the cart upon checkout
            Session.instance.cart = null;
            // clear products in cart dictionary
            this._productsInCart.clear();
            // clear the customer
            Session.instance.Customer = null;
            //clear the customer address
            Session.instance.CustomerPrimaryAddress = null;

        }

        /**
         * Notifies card state listeners that the card is reloaded.
         */
        public cartReloaded(): void {
            Session.instance.cartReloadedNotificationPending = false;
            Commerce.ViewModelAdapter.raiseViewEvent(Session._cartStateUpdateEvent, CartStateType.Reloaded);
        }

        /**
         * Set the product catalog store to session.
         *
         * @param {string} newStoreId Store identifier to be set to session.
         */
        public set productCatalogStore(newProductCatalogStore: Proxy.Entities.ProductCatalogStore) {
            Session.instance._productCatalogStore = newProductCatalogStore;
        }

        /**
         * Gets the product catalog store information.
         */
        public get productCatalogStore(): Proxy.Entities.ProductCatalogStore {
            return Session.instance._productCatalogStore;
        }

        /**
         * Gets a boolean value if catalog from store is virtual
         *
         * @return true if store catalog is virtual, false otherwise.
         */
        public get isStoreCatalogVirtual(): boolean {
            var storeType: Proxy.Entities.StoreButtonControlType = Session.instance._productCatalogStore.StoreType;
            return storeType === Proxy.Entities.StoreButtonControlType.AllStores ||
                storeType === Proxy.Entities.StoreButtonControlType.Warehouse;
        }

        /**
         * Gets the connection availability status.
         */
        public get connectionAvailabilityStatus(): ConnectionAvailabilityStatus {
            return this._connectionAvailabilityStatus;
        }

        /**
         * Sets the connection status.
         */
        public set connectionStatus(value: ConnectionStatusType) {
            this._connectionStatus = value;
            ApplicationStorage.setItem(ApplicationStorageIDs.CONNECTION_STATUS, value.toString());
            TsLogging.LoggerBase.setDeviceOfflineInfo(ConnectionAvailabilityStatus[this.connectionAvailabilityStatus], ConnectionStatusType[this.connectionStatus]);

            if (value == Commerce.ConnectionStatusType.Online) {
                this.connectionStatusAsString(Commerce.ViewModelAdapter.getResourceString("string_6610"));
            }
            else {
                this.connectionStatusAsString(Commerce.ViewModelAdapter.getResourceString("string_6611"));
            }
        }

        /**
         * Gets the connection status.
         */
        public get connectionStatus(): ConnectionStatusType {
            return this._connectionStatus;
        }

        /**
         * Gets the is offline available.
         */
        public get isOfflineAvailable(): boolean {
            return this._isOfflineAvailable;
        }

        /**
         * Sets the is offline available.
         * @param {boolean} value New value.
         */
        public set isOfflineAvailable(value: boolean) {
            this._isOfflineAvailable = value;
            this.refreshConnectionAvailabilityStatus();
        }

        /**
         * Gets the display state of the error.
         *
         * @param {ErrorsDisplayedPerSession} errorKey The error identifier.
         * @return {boolean} True if the error should be displayed or error identifier does not exist, false otherwise.
         */
        public getErrorDisplayState(errorKey: ErrorsDisplayedPerSession): boolean {
            if (!this._errorDisplayStatesPerSession.hasItem(errorKey)) {
                return true;
            }

            return this._errorDisplayStatesPerSession.getItem(errorKey);
        }

        /**
         * Set the display state of the error.
         *
         * @param {ErrorsDisplayedPerSession} errorKey The error identifier.
         * @param {boolean} shouldDisplay True if the error should be displayed, false otherwise.
         */
        public setErrorDisplayState(errorKey: ErrorsDisplayedPerSession, shouldDisplay: boolean = false): void {
            this._errorDisplayStatesPerSession.setItem(errorKey, shouldDisplay);
        }

        /**
         * Creates the instance variables for the Session object.
         */
        private createSession(): void {
            this.userName = ko.observable("");
            this.picture = ko.observable("");
            this.shiftId = ko.observable(0);
            this.shiftTerminalId = ko.observable("");
            this.terminalId = ko.observable("");
            this.connectionStatusAsString = ko.observable("");
            this.employee = ko.observable(null);
            this._productsInCart = new Dictionary<Proxy.Entities.SimpleProduct>();
            this._offlineParameters = new OfflineParameters(0, 0, false);
            this.categoryTree = ko.observableArray([]);
            this.offlineSyncing = ko.observable(false);
            this.pendingDownloadSessionCount = ko.observable(0);
        }

        /**
         * Set the default session state for an employee.
         */
        private setDefaultEmployeeSessionState(): void {
            this.setErrorDisplayState(ErrorsDisplayedPerSession.PaymentTerminalBeginTransaction, true);
        }

        /**
         * Refreshes the connection availability status.
         */
        public refreshConnectionAvailabilityStatus(): void {
            if (!Commerce.Utilities.OfflineHelper.isOfflineEnabled()) {
                this._connectionAvailabilityStatus = ConnectionAvailabilityStatus.NotEnabled;
            } else if (this.isOfflineAvailable) {
                this._connectionAvailabilityStatus = ConnectionAvailabilityStatus.Available;
            } else {
                this._connectionAvailabilityStatus = ConnectionAvailabilityStatus.EnabledNotAvailable;
            }

            TsLogging.LoggerBase.setDeviceOfflineInfo(ConnectionAvailabilityStatus[this.connectionAvailabilityStatus], ConnectionStatusType[this.connectionStatus]);
        }
    }
}