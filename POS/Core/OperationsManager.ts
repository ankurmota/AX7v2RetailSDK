/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    /**
     * Class for managing operations that are performed across pages.
     * This class is implemented as a singleton
     */
    export class OperationsManager {
        private static _instance: Commerce.Operations.OperationsManager = null;
        private _managerOverrideHandler: any = null;
        private _managerOverrideRequestingOperation: number = null;
        private _managerInformation: Commerce.Model.Entities.Employee;
        private _employeeToken: Authentication.IAuthenticationToken;
        private _authenticationManager: Commerce.Model.Managers.IAuthenticationManager = null;
        private _managerPrivileges: string = "MANAGERPRIVILEGES";
        private _operationsMap: { [operationId: number]: IOperation };

        private _operationStackCount: number;
        private _operationStackCountDuringElevation: number;

        /**
         * Creates an instance of the OperationsManager class.
         */
        constructor() {
            this._operationsMap = {};
            this._operationStackCount = 0;
            this._operationStackCountDuringElevation = -1;
        }

        /**
         * Get the instance of operations manager.
         */
        public static get instance(): OperationsManager {
            if (ObjectExtensions.isNullOrUndefined(OperationsManager._instance)) {
                OperationsManager._instance = new OperationsManager();
            }

            return OperationsManager._instance;
        }

        /**
         * Get the value of the manager override handler.
         *
         * @return {any} The handler for manager override execution.
         */
        public get managerOverrideHandler(): any {
            return OperationsManager.instance._managerOverrideHandler;
        }

        /**
         * Set the value of the manager override handler.
         * 
         * @param {any} handler The new value of the manager override handler.
         */
        public set managerOverrideHandler(handler: any) {
            OperationsManager.instance._managerOverrideHandler = handler;
        }

        /**
         * Get the employee token used for operation execution.
         *
         * @return {Authentication.IAuthenticationToken} The token used for operation execution.
         */
        public get employeeToken(): Authentication.IAuthenticationToken {
            return OperationsManager.instance._employeeToken;
        }

        /**
         * Set the employee token to be used for operation execution.
         * 
         * @param {Authentication.IAuthenticationToken} token The employee token to be used for operation execution.
         */
        public set employeeToken(token: Authentication.IAuthenticationToken) {
            OperationsManager.instance._employeeToken = token;
        }

        /**
         * Get the manager information used for override.
         *
         * @return {Commerce.Model.Entities.Employee} The manager employee object.
         */
        public get managerInformation(): Commerce.Model.Entities.Employee {
            return OperationsManager.instance._managerInformation;
        }

        /**
         * Set the manager information used for override.
         * 
         * @param {Commerce.Model.Entities.Employee} The manager employee object.
         */
        public set managerInformation(manager: Commerce.Model.Entities.Employee) {
            OperationsManager.instance._managerInformation = manager;
        }

        /**
         * Get the employee information used for current operation execution.
         *
         * @return {Commerce.Model.Entities.Employee} The employee object.
         */
        public get currentOperationEmployee(): Commerce.Model.Entities.Employee {
            return OperationsManager.instance._managerInformation || Commerce.Session.instance.CurrentEmployee;
        }

        /**
         * Register an operation pre-handler, handler, post-handler and validators for the specified operation identifier.
         * 
         * @param {IOperation} operation The operation to register the pre-handler, handler, post-handler and validators for.
         */
        public registerOperationHandler(operation: IOperation) {
            if (ObjectExtensions.isNullOrUndefined(operation)
                || ObjectExtensions.isNullOrUndefined(operation.id)
                || ObjectExtensions.isNullOrUndefined(operation.handler)) {
                return;
            }

            //override the default handler
            this._operationsMap[operation.id] = operation;
        }

        /**
         * Run the specified operation identifier without performing any permissions check.
         * 
         * @param {number} id The operation identifier to run.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableDataResult<T>>} The async result.
         */
        public runOperationWithoutPermissionsCheck<T>(id: number, options: IOperationOptions): IAsyncResult<ICancelableDataResult<T>> {
            return this.runOperationInternal(id, false, options);
        }

        /**
         * Run the specified operation identifier.
         * 
         * @param {number} id The operation identifier to run.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableDataResult<T>>} The async result.
         */
        public runOperation<T>(id: number, options: IOperationOptions): IAsyncResult<ICancelableDataResult<T>> {
            return this.runOperationInternal(id, true, options);
        }

        /**
         * Checks whether the operation will pass client side non-permission execution validation tests.
         * 
         * @param {number} id The operation identifier to run.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableDataResult<T>>} The async result.
         */
        public canExecuteAsync(id: number, options: IOperationOptions): IVoidAsyncResult {
            var operationImpl = this._operationsMap[id];

            if (operationImpl == null) {
                RetailLogger.genericError(StringExtensions.format("Operation '{0}' doesn't have any implementation handler", id));
            }

            return this.preValidatorsQueue(operationImpl, options).run();
        }

        /**
         * Run the specified operation identifier.
         * 
         * @param {number} id The operation identifier to run.
         * @param {boolean} checkPermissions Flag indicating if permissions are to be checked.
         * @param {IOperationOptions} options The operation options.
         * @return {IAsyncResult<ICancelableDataResult<T>>} The async result.
         */
        private runOperationInternal<T>(id: number, checkPermissions: boolean, options: IOperationOptions): IAsyncResult<ICancelableDataResult<T>> {
            var asyncResult = new AsyncResult<{ canceled: boolean; data: any }>();
            var dataContainer = { data: undefined };
            var asyncQueue = new AsyncQueue();
            var operationImpl = this._operationsMap[id];
            var correlationId = TsLogging.Utils.generateGuid();

            if (operationImpl == null) {
                throw 'Operation ' + id + ' does not have any implementation handler.';
            }

            RetailLogger.coreRetailOperationStarted(
                correlationId,
                RetailOperation[operationImpl.id],
                operationImpl.id);

            asyncQueue
                .enqueue(() => {
                    var drawerCheckQueue: AsyncQueue = this.validateDrawerStatus(operationImpl);
                    return asyncQueue.cancelOn(drawerCheckQueue.run());
                }).enqueue(() => {
                    this._operationStackCount++;

                    var queue: AsyncQueue = this.revertToSelf();
                    return asyncQueue.cancelOn(queue.run());
                }).enqueue(() => {
                    var queue: AsyncQueue = this.preTriggerQueue(operationImpl, options);
                    return asyncQueue.cancelOn(queue.run());
                }).enqueue(() => {
                    var queue: AsyncQueue = this.preHandlerQueue(operationImpl, options);
                    return asyncQueue.cancelOn(queue.run());
                }).enqueue(() => {
                    var queue = this.preValidatorsQueue(operationImpl, options);
                    return asyncQueue.cancelOn(queue.run());
                });

            if (checkPermissions) {
                asyncQueue
                    .enqueue(() => {
                        var queue = this.managerOverrideQueue(operationImpl);
                        return asyncQueue.cancelOn(queue.run())
                            .fail((errors) => {
                                this._managerOverrideRequestingOperation = null;
                                this.managerInformation = null;
                                this.employeeToken = null;
                            });
                    });
            }

            asyncQueue
                .enqueue(() => {
                    var queue = this.handlerQueue(operationImpl, dataContainer, options, correlationId);
                    return asyncQueue.cancelOn(queue.run());
                }).enqueue(() => {
                    // we cannot cancel the queue after the operation handler executed, so we just run it
                    return this.postHandlerQueue(operationImpl, dataContainer.data).run();
                }).enqueue(() => {
                    return this.postTriggerQueue(operationImpl, options, dataContainer.data).run();
                });

            // resolves the async result with results from the queue and data from operation
            asyncQueue.run()
                .always((): void => {
                    this._operationStackCount--;
                }).done((result) => {
                    if (result.canceled) {
                        RetailLogger.coreRetailOperationCanceled(
                            correlationId,
                            RetailOperation[operationImpl.id],
                            operationImpl.id);
                    } else {
                        RetailLogger.coreRetailOperationCompleted(
                            correlationId,
                            RetailOperation[operationImpl.id],
                            operationImpl.id);
                    }

                    asyncResult.resolve({ canceled: result.canceled, data: dataContainer.data });
                }).fail((errors) => {
                    this.failureTriggerQueue(operationImpl, options, errors).run()
                        .always((): void => {
                        RetailLogger.coreRetailOperationFailed(
                            correlationId,
                            RetailOperation[operationImpl.id],
                            operationImpl.id,
                            ErrorHelper.getErrorMessages(errors));
                            asyncResult.reject(errors);
                        });
                });

            return asyncResult;
        }

        /**
         * Validate if the cash drawer is open before performing any operation other than CloseShift, TenderDeclaration, BankDrop, SafeDrop, PrintZ and Logoff.
         * 
         * @param {IOperation} operationImpl The operation implementation.
         * @return {AsyncQueue} The async queue.
         */
        public validateDrawerStatus(operationImpl: IOperation): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            var operationId: number = operationImpl.id;

            if (operationId !== RetailOperation.CloseShift
                && operationId !== RetailOperation.TenderDeclaration
                && operationId !== RetailOperation.BankDrop
                && operationId !== RetailOperation.SafeDrop
                && operationId !== RetailOperation.PrintZ
                && Peripherals.HardwareStation.HardwareStationContext.instance.isActive()
                && !Session.instance.Shift.IsShared) {

                asyncQueue.enqueue((): IVoidAsyncResult => {
                    RetailLogger.coreRetailCheckOpenDrawerStatusExecutionStart();

                    var isDrawerOpen: boolean = false;
                    var drawerCheckResult: VoidAsyncResult = new VoidAsyncResult();

                    Peripherals.instance.cashDrawer.isOpenAsync()
                        .done((result: boolean) => {
                            isDrawerOpen = result;

                            if (isDrawerOpen) {
                                var errors = [new Model.Entities.Error(ErrorTypeEnum.PERIPHERALS_CASHDRAWER_ALREADY_OPENED)];
                                drawerCheckResult.reject(errors);
                            }

                            drawerCheckResult.resolve();
                            RetailLogger.coreRetailCheckOpenDrawerStatusExecutionCompleted();
                        }).fail((error): void => {

                        drawerCheckResult.resolve();
                        RetailLogger.coreRetailCheckOpenDrawerStatusExecutionFailed(error[0].ErrorCode, ErrorHelper.formatErrorMessage(error[0]));
                    });

                    return drawerCheckResult;
                });
            }

            return asyncQueue;
        }

        /**
         * Revert to Self if elevated before.
         * 
         * @return {AsyncQueue} The async queue.
         */
        public revertToSelf(): AsyncQueue {
            var asyncQueue = new AsyncQueue();

            // manager override is only supported on IResourceOwnerPasswordGrantAuthenticationProvider
            var authenticationProvider: Authentication.IResourceOwnerPasswordGrantAuthenticationProvider;
            authenticationProvider = Authentication.AuthenticationProviderManager.instance.getResourceOwnerPasswordGrantProvider(
                Authentication.AuthenticationProviderResourceType.USER);

            // revert to self if needed
            if (this._operationStackCount <= this._operationStackCountDuringElevation
                && !ObjectExtensions.isNullOrUndefined(this._managerOverrideRequestingOperation)) {

                asyncQueue.enqueue((): IVoidAsyncResult => {
                    if (ObjectExtensions.isNullOrUndefined(this._authenticationManager)) {
                        this._authenticationManager = Commerce.Model.Managers.Factory.GetManager(Commerce.Model.Managers.IAuthenticationManagerName, this);
                    }

                    RetailLogger.coreOperationManagerRevertToSelf();

                    return authenticationProvider.restoreToken(this.employeeToken).done(() => {
                        this._operationStackCountDuringElevation = -1;
                        this._managerOverrideRequestingOperation = null;
                        this.managerInformation = null;
                        this.employeeToken = null;
                    });
                });
            }
            return asyncQueue;
        }

        /**
         * Creates an async queue executing the pre operation trigger.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @return {AsyncQueue} The async queue.
         */
        private preTriggerQueue(operationImpl: IOperation, options: IOperationOptions): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                var triggerOptions: Triggers.IOperationTriggerOptions = { operationId: operationImpl.id, operationOptions: options };
                return asyncQueue.cancelOn(Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreOperation, triggerOptions));
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the pre-handler function, if any provided.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @return {AsyncQueue} The async queue.
         */
        private preHandlerQueue(operationImpl: IOperation, options: IOperationOptions): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            if (operationImpl.preHandler) {
                asyncQueue.enqueue(() => {
                    return asyncQueue.cancelOn(operationImpl.preHandler(options));
                });
            }

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the pre-validators, if any provided.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @return {AsyncQueue} The async queue.
         */
        private preValidatorsQueue(operationImpl: IOperation, options: IOperationOptions): AsyncQueue {
            var asyncQueue = new AsyncQueue();

            asyncQueue.enqueue(() => {
                // Run pre-operation validators, if any.
                if (!ObjectExtensions.isNullOrUndefined(operationImpl) && ArrayExtensions.hasElements(operationImpl.validators)) {
                    var validationErrors: Model.Entities.Error[] = [];

                    operationImpl.validators.forEach(current => {
                        if (!current.validatorFunctions) {
                            return;
                        }

                        var data = current.dataAccessor ? current.dataAccessor(options) : null;
                        current.validatorFunctions.forEach((validatorFunction: (data?: any) => Model.Entities.Error[]) => {
                            var errors: Model.Entities.Error[] = validatorFunction(data);
                            if (ArrayExtensions.hasElements(errors)) {
                                validationErrors.push.apply(validationErrors, errors);
                            }
                        });
                    });

                    if (ArrayExtensions.hasElements(validationErrors)) {
                        return AsyncResult.createRejected(validationErrors);
                    }
                }
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue to check whether a manager override is required for the operation.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @return {AsyncQueue} The async queue.
         */
        private managerOverrideQueue(operationImpl: IOperation): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            var errors: Proxy.Entities.Error[];

            asyncQueue.enqueue(() => {
                // Run the operation with permissions check.
                if (!this.isCurrentEmployeeAllowed(operationImpl.id)) {
                    if (!this.isManagerOverrideAllowed(operationImpl.id, errors = [])) {
                        return AsyncResult.createRejected(errors);
                    } else if (ObjectExtensions.isNullOrUndefined(this.managerOverrideHandler)) {
                        errors = [new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PERMISSION_DENIED)];
                        return AsyncResult.createRejected(errors);
                    } else {
                        this._operationStackCountDuringElevation = this._operationStackCount;
                        this._managerOverrideRequestingOperation = operationImpl.id;
                        var managerOverrideResult = new VoidAsyncResult(this);
                        var managerOverrideArguments: any[] = [this, operationImpl.id, managerOverrideResult];

                        RetailLogger.coreRetailOperationManagerOverride(RetailOperation[operationImpl.id], operationImpl.id);

                        // invoke manager override
                        (<Function>this.managerOverrideHandler.execute).apply(this, managerOverrideArguments);

                        return managerOverrideResult;
                    }
                }

                return null;
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the handler function.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @return {AsyncQueue} The async queue.
         */
        private handlerQueue(operationImpl: IOperation, dataContainer: { data: any }, options: IOperationOptions, correlationId: string): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            asyncQueue.enqueue(() => {
                var handler = operationImpl.handler;

                if (ObjectExtensions.isNullOrUndefined(handler)) {
                    RetailLogger.coreRetailOperationHandlerNotFound(RetailOperation[operationImpl.id], operationImpl.id);
                }

                // execute the handler for the corresponding operation identifier
                var asyncResult: IAsyncResult<IOperationResult>;
                try {
                    asyncResult = <IAsyncResult<{ canceled: boolean; data?: any }>>handler.execute(options);
                    if (asyncResult) {
                        asyncResult.done((result) => {
                            if (result && !result.canceled) {
                                dataContainer.data = result.data || result;
                            }
                        });
                    }
                } catch (error) {
                    var errorMessage: string = 'Operation ' + operationImpl.id + ' terminated unexpectedly. ' + (error || '');
                    RetailLogger.coreRetailOperationFailed(
                        correlationId,
                        RetailOperation[operationImpl.id],
                        operationImpl.id,
                        errorMessage);
                    asyncResult = AsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
                }

                return asyncQueue.cancelOn(asyncResult);
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the post-handler function, if any provided.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {any} handlerResult The result provided by the handler function, if any.
         * @return {AsyncQueue} The async queue.
         */
        private postHandlerQueue(operationImpl: IOperation, handlerResult: any): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            if (operationImpl.postHandler) {
                asyncQueue.enqueue(() => {
                    return operationImpl.postHandler(handlerResult);
                });
            }

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the post operation triggers.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @param {any} handlerResult The result provided by the handler function.
         * @return {AsyncQueue} The async queue.
         */
        private postTriggerQueue(operationImpl: IOperation, options: IOperationOptions, handlerResult: any): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            asyncQueue.enqueue(() => {
                var triggerOptions: Triggers.IPostOperationTriggerOptions = { operationId: operationImpl.id, operationOptions: options, data: handlerResult };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.PostOperation, triggerOptions);
            });

            return asyncQueue;
        }

        /**
         * Creates an async queue executing the operation failure triggers.
         *
         * @param {IOperation} operationImpl The operation implementation.
         * @param {IOperationOptions} options The operation options.
         * @param {Entities.Error[]} errors The list of errors that occurred when the operation was run.
         * @return {AsyncQueue} The async queue.
         */
        private failureTriggerQueue(operationImpl: IOperation, options: IOperationOptions, errors: Model.Entities.Error[]): AsyncQueue {
            var asyncQueue = new AsyncQueue();
            asyncQueue.enqueue(() => {
                var triggerOptions: Triggers.IOperationFailureTriggerOptions = { operationId: operationImpl.id, operationOptions: options, errors: errors };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.OperationFailure, triggerOptions);
            });

            return asyncQueue;
        }

        /**
         * Check if the current operator has permissions for the specified operation identifier.
         * 
         * @param {number} id The operation identifier to run.
         * @return {boolean} True if the current logged in employee has permissions to perform the operation, else false.
         */
        private isCurrentEmployeeAllowed(operationId: number): boolean {
            // check if employee is logged on
            var isLoggedOn: boolean = !ObjectExtensions.isNullOrUndefined(Commerce.Session.instance.CurrentEmployee);
            if (!isLoggedOn) {
                // if employee is not logged on, server request will fail with unauthenticated user and error handling will redirect
                // user back to the logon page - returning "false" here would redirect the user to manager override page instead
                return true;
            }

            // first check if employee is manager
            var employeePermissions: Model.Entities.EmployeePermissions = Commerce.Session.instance.CurrentEmployee.Permissions;
            if (employeePermissions.Roles.some(role => role === this._managerPrivileges)) {
                return true;
            }

            // if permissions are not set for operation, assume secure by default and deny access
            var operationPermissions: Commerce.Model.Entities.OperationPermission = this.getOperationPermissions(operationId);
            if (ObjectExtensions.isNullOrUndefined(operationPermissions)) {
                return false;
            }

            // if there is no need to check user access or no special permissions are required, grant access
            if (!operationPermissions.CheckUserAccess || !ArrayExtensions.hasElements(operationPermissions.Permissions)) {
                return true;
            }

            // special case for price override
            if (operationId === RetailOperation.PriceOverride) {
                return employeePermissions.AllowPriceOverride != Model.Entities.EmployeePriceOverrideType.NotAllowed;
            }

            // check if all the permissions specified by the operation are present on the employee roles
            return operationPermissions.Permissions.reduce<boolean>((isAllowed: boolean, operationPermission: string) => {
                return isAllowed && employeePermissions.Roles.some(role => role === operationPermission);
            }, true /* initial value: assume employee is allowed */);
        }

        /**
         * Check if manager override is allowed for the operation.
         * 
         * @param {number} id The operation identifier to run.
         * @param {Entities.Error[]} errors The list of errors to add the errors.
         * @return {boolean} True if manager override is allowed for the operation, else false.
         */
        private isManagerOverrideAllowed(operationId: number, errors: Model.Entities.Error[]): boolean {
            var isAllowed: boolean = true;

            if (ObjectExtensions.isNullOrUndefined(errors)) {
                errors = null;
            }

            // Method to add the error to the error list
            var addError = ((error: Model.Entities.Error) => {
                if (errors != null) {
                    errors.push(error);
                }
            });

            // Special case for change password
            if (operationId === RetailOperation.ChangePassword) {
                addError(new Model.Entities.Error(ErrorTypeEnum.CHANGE_PASSWORD_NOT_ALLOWED_PERMISSION_DENIED_MANAGER_OVERRIDE_NOT_ALLOWED));
                isAllowed = false;
            }

            // Set the default error message if one was not set
            if (!isAllowed && (errors != null) && (errors.length === 0)) {
                errors.push(new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_PERMISSION_DENIED_MANAGER_OVERRIDE_NOT_ALLOWED));
            }

            return isAllowed;
        }

        /**
         * Check if the currently logged on employee has manager privileges.
         * 
         * @return {boolean} True if the employee has manager privileges, otherwise false.
         */
        private hasManagerPrivileges(): boolean {
            for (var index = 0; index < Commerce.Session.instance.CurrentEmployee.Permissions.Roles.length; index++) {
                // If operator has manager permissions, allow the operation.
                if (this._managerPrivileges === Commerce.Session.instance.CurrentEmployee.Permissions.Roles[index]) {
                    return true;
                }
            }

            return false;
        }

        /**
         * Get the permission name for the specified operation identifier.
         * 
         * @param {number} operationId The operation identifier to run.
         * @return {Commerce.Model.Entities.OperationPermissions} The OperationPermissions object for an operation with given identifier.
         * If no object exists, it returns null.
         */
        private getOperationPermissions(operationId: number): Commerce.Model.Entities.OperationPermission {
            var operationPermissions: Model.Entities.OperationPermission[] = ApplicationContext.Instance.operationPermissions;
            for (var index = 0; index < operationPermissions.length; index++) {
                if (operationId == operationPermissions[index].OperationId) {
                    return operationPermissions[index];
                }
            }

            return null;
        }
    }
}
