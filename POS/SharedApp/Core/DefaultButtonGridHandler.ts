/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {

    export class DefaultButtonGridHandler {

        /**
         * Default operation handler for button grid buttons.
         *
         * @param {number} operationId The view options.
         * @param {string} actionProperty The extra parameters for operation.
         * @param {Observable<boolean>} [indeterminateWaitVisible] The indeterminate wait observable.
         */
        public static handleOperation(operationId: number, actionProperty: string, indeterminateWaitVisible?: Observable<boolean>): boolean {
            var operationsManager: OperationsManager = OperationsManager.instance;

            switch (operationId) {
                // WelcomeScreen1
                case RetailOperation.BlankOperation:
                    if (StringExtensions.isNullOrWhitespace(actionProperty)) {
                        return false;
                    }

                    var blankOperationParameters = actionProperty.split(";");
                    var blankOperationOptions: IBlankOperationOptions = {
                        operationId: blankOperationParameters.shift(), operationData: blankOperationParameters.shift()
                    };

                    operationsManager.runOperation(operationId, blankOperationOptions)
                        .fail((errors) => { NotificationHandler.displayClientErrors(errors); });
                    return true;
                   // break;
                case RetailOperation.ItemSale:
                    var options = {
                        itemToAddOrSearch: actionProperty
                    };
                    Commerce.ViewModelAdapter.navigate("CartView", options);
                    return true;
                case RetailOperation.PriceCheck:
                    Commerce.ViewModelAdapter.navigate("PriceCheckView");
                    return true;
                case RetailOperation.InventoryLookup:
                    Commerce.ViewModelAdapter.navigate("InventoryLookupView");
                    return true;
                case RetailOperation.PickingAndReceiving:
                    Commerce.ViewModelAdapter.navigate("SearchPickingAndReceivingView");
                    return true;
                case RetailOperation.StockCount:
                    Commerce.ViewModelAdapter.navigate("SearchStockCountView");
                    return true;
                case RetailOperation.ViewReport:
                    Commerce.ViewModelAdapter.navigate("ReportsView");
                    return true;
                case RetailOperation.ChangePassword:
                    // Check to make sure there is no active transaction.
                    if (Session.instance.isCartInProgress) {
                        var errors: Model.Entities.Error[] = <Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FINISH_CURRENT_TRANSACTION)];
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    } else {
                        Commerce.ViewModelAdapter.navigate("ChangePasswordView");
                    }
                    return true;
                case RetailOperation.ResetPassword:
                    // Check to make sure there is no active transaction.
                    if (Session.instance.isCartInProgress) {
                        var errors: Model.Entities.Error[] = <Model.Entities.Error[]>[new Model.Entities.Error(ErrorTypeEnum.OPERATION_NOT_ALLOWED_FINISH_CURRENT_TRANSACTION)];
                        Commerce.NotificationHandler.displayClientErrors(errors);
                    } else {
                        Commerce.ViewModelAdapter.navigate("ResetPasswordView");
                    }
                    return true;
                case RetailOperation.KitDisassembly:
                    Commerce.ViewModelAdapter.navigate("KitDisassemblyView");
                    return true;
                case RetailOperation.ChangeHardwareStation:
                    ViewModelAdapter.navigate("HardwareStationView");
                    return true;
                case RetailOperation.CustomerSearch:
                    ViewModelAdapter.navigate("SearchView", { searchEntity: "Customers" });
                    return true;
                case RetailOperation.ItemSearch:
                    Commerce.ViewModelAdapter.navigate("SearchView", { searchEntity: "Products" });
                    return true;
                case RetailOperation.Search:
                    var searchParameters: string[] = actionProperty.split(";");
                    // default parameters for search
                    var params: any = {
                        searchEntity: "Products",
                        searchText: ""
                    };

                    // Map the button grid search action property to SearchView Parameters.
                    if (!StringExtensions.isNullOrWhitespace(searchParameters[0])) {

                        switch (searchParameters[0]) {
                            case "Products":
                                params.searchEntity = searchParameters[0];
                                params.searchText = !ObjectExtensions.isNullOrUndefined(searchParameters[1]) ? searchParameters[1] : "";
                                Commerce.ViewModelAdapter.navigate("SearchView", params);
                                break;
                            case "Customers":
                                params.searchEntity = searchParameters[0];
                                params.searchText = !ObjectExtensions.isNullOrUndefined(searchParameters[1]) ? searchParameters[1] : "";
                                Commerce.ViewModelAdapter.navigate("SearchView", params);
                                break;
                            case "Category":
                                if (!ObjectExtensions.isNullOrUndefined(searchParameters[1])) {
                                    Commerce.ViewModelAdapter.navigate("ProductsView", <ViewControllers.IProductsViewOptions>{ category: { RecordId: parseInt(searchParameters[1]) }, activeMode: Commerce.ViewModels.ProductsViewModelActiveMode.Products });
                                } else {
                                    Commerce.ViewModelAdapter.navigate("CategoriesView");
                                }
                                break;
                        }

                    } else {
                        Commerce.ViewModelAdapter.navigate("SearchView", params);
                    }

                    return true;
                case RetailOperation.BankDrop:
                case RetailOperation.DeclareStartAmount:
                case RetailOperation.FloatEntry:
                case RetailOperation.SafeDrop:
                case RetailOperation.TenderDeclaration:
                case RetailOperation.TenderRemoval:
                    DefaultButtonGridHandler.runTenderOperation(operationId, indeterminateWaitVisible);
                    return true;
                case Operations.RetailOperation.PrintX:
                    DefaultButtonGridHandler.runOperation(
                        operationId,
                        <Operations.IPrintXOperationOptions>{ shift: Session.instance.Shift, notifyOnNoPrintableReceipts: true },
                        indeterminateWaitVisible);
                    return true;
                case Operations.RetailOperation.PrintZ:
                    DefaultButtonGridHandler.runOperation(
                        operationId,
                        <Operations.IPrintZOperationOptions>{ notifyOnNoPrintableReceipts: true },
                        indeterminateWaitVisible);
                    return true;
                case RetailOperation.OpenDrawer:
                case RetailOperation.CloseShift:
                    DefaultButtonGridHandler.runOperation(operationId, null, indeterminateWaitVisible);
                    return true;
                case RetailOperation.DisplayTotal:
                case RetailOperation.ExtendedLogOn:
                    operationsManager.runOperation(operationId, null).fail((errors: Model.Entities.Error[]) => {
                        NotificationHandler.displayClientErrors(errors, "string_4159"); // The operation can't be performed.
                    });
                    return true;
                // run the operations passing null options and displaying default error messages
                case RetailOperation.AddAffiliationFromList:
                case RetailOperation.BlindCloseShift:
                case RetailOperation.CustomerAccountDeposit:
                case RetailOperation.CustomerAdd:
                case RetailOperation.DatabaseConnectionStatus:
                case RetailOperation.ExpenseAccounts:
                case RetailOperation.IncomeAccounts:
                case RetailOperation.LoyaltyIssueCard:
                case RetailOperation.ReturnTransaction:
                case RetailOperation.ShowBlindClosedShifts:
                case RetailOperation.ShowJournal:
                case RetailOperation.SuspendShift:
                case RetailOperation.TimeRegistration:
                case RetailOperation.ViewTimeClockEntries:
                case RetailOperation.RecallSalesOrder:
                    if (!ObjectExtensions.isNullOrUndefined(indeterminateWaitVisible)) {
                        indeterminateWaitVisible(true);
                    }

                    operationsManager.runOperation(operationId, null)
                        .done((result: Operations.IOperationResult) => {
                            if (operationId === RetailOperation.ReturnTransaction) {
                                if (result && !result.canceled) {
                                    ViewModelAdapter.navigate("CartView");
                                }
                            }
                        })
                        .fail((errors: Model.Entities.Error[]) => {
                            NotificationHandler.displayClientErrors(errors);
                        })
                        .always((): void => {
                            if (!ObjectExtensions.isNullOrUndefined(indeterminateWaitVisible)) {
                                indeterminateWaitVisible(false);
                            }
                        });
                    return true;
            }

            return false;
        }

        /**
         * Runs the the operation with blocking the UI. Displays the errors if any occur.
         *
         * @param {RetailOperation} operationType The type of operation to run.
         * @param {IOperationOptions} parameters The operation parameters.
         * @param {Observable<boolean>} [indeterminateWaitVisible] The indeterminate wait observable.
         */
        private static runOperation(operationType: RetailOperation, parameters: IOperationOptions, indeterminateWaitVisible?: Observable<boolean>): void {
            DefaultButtonGridHandler.runOperationWithIndeterminateWait(operationType, parameters, indeterminateWaitVisible)
                .fail((errors: Model.Entities.Error[]) => {
                    Commerce.NotificationHandler.displayClientErrors(errors);
                });
        }

        /**
         * Runs the tender counting operations.
         *
         * @param {RetailOperation} operationType The type of tender counting operation to run.
         * @param {Observable<boolean>} [indeterminateWaitVisible] The indeterminate wait observable.
         */
        private static runTenderOperation(operationType: RetailOperation, indeterminateWaitVisible?: Observable<boolean>): void {
            DefaultButtonGridHandler.runOperationWithIndeterminateWait(operationType, null, indeterminateWaitVisible)
                .fail((errors: Model.Entities.Error[]) => {
                    DefaultButtonGridHandler.handleTenderOperationErrors(errors, operationType);
                });
        }

        /**
         * Runs the the operation with blocking the UI.
         *
         * @param {RetailOperation} operationType The type of operation to run.
         * @param {IOperationOptions} parameters The operation parameters.
         * @param {Observable<boolean>} [indeterminateWaitVisible] The indeterminate wait observable.
         */
        private static runOperationWithIndeterminateWait(operationType: RetailOperation, parameters: IOperationOptions, indeterminateWaitVisible?: Observable<boolean>): IAsyncResult<ICancelableDataResult<{}>> {
            if (!ObjectExtensions.isNullOrUndefined(indeterminateWaitVisible)) {
                indeterminateWaitVisible(true);
            }
            return Commerce.Operations.OperationsManager.instance.runOperation(operationType, parameters)
                .always((): void => {
                    if (!ObjectExtensions.isNullOrUndefined(indeterminateWaitVisible)) {
                        indeterminateWaitVisible(false);
                    }
                });
        }

        /**
         * Handles the errors for TenderCounting Operations. Specifically, if the cash drawer is not found.
         *
         * @param {Model.Entities.Error[]} errors The array of errors.
         * @param {RetailOperation} operationType The operation that was being performed.
         */
        private static handleTenderOperationErrors(errors: Model.Entities.Error[], operationType: RetailOperation): void {
            if (errors.length == 1) {
                var errorCode = errors[0].ErrorCode;
                var errorMessage: string = NotificationHandler.getErrorMessage(errors[0]);
                var cashDrawerNotFoundErrorMessage: string = ViewModelAdapter.getResourceString(Commerce.ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CASHDRAWER_ERROR);

                // If the error is because the cash drawer is not found or there is no active hardware station, ask the user if they want to continue.
                if (/**POSHackF hide hardware station error
                    errorCode === Commerce.ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_NOTCONFIGURED
                    ||
                    */
                    errorCode === Commerce.ErrorTypeEnum.PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED
                    || errorMessage === cashDrawerNotFoundErrorMessage) {
                    errorMessage += "\r\n\r\n";
                    errorMessage += ViewModelAdapter.getResourceString("string_421"); // Do you wish to continue the operation anyway?

                    Commerce.ViewModelAdapter.displayMessage(errorMessage, MessageType.Error, MessageBoxButtons.YesNo)
                        .done(continueMessageResult => {
                            if (continueMessageResult == DialogResult.Yes) {
                                // Create View Options and Navigate to the Corresponding View
                                DefaultButtonGridHandler.createOptionsAndNavigateToView(operationType);
                            }
                        });

                    return;
                }
            }

            Commerce.NotificationHandler.displayClientErrors(errors);
        }

        /**
         * Creates the navigation options and navigates to the appropriate view for the operation type.
         *
         * @param {RetailOperation} operationType The Operation type.
         */
        private static createOptionsAndNavigateToView(operationType: RetailOperation): void {
            var options: any;
            switch (operationType) {
                case RetailOperation.TenderDeclaration:
                    options = { tenderDropAndDeclareType: Model.Entities.TransactionType.TenderDeclaration };
                    ViewModelAdapter.navigate("TenderCountingView", options);
                    break;
                case RetailOperation.BankDrop:
                    options = { tenderDropAndDeclareType: Model.Entities.TransactionType.BankDrop };
                    ViewModelAdapter.navigate("TenderCountingView", options);
                    break;
                case RetailOperation.SafeDrop:
                    options = { tenderDropAndDeclareType: Model.Entities.TransactionType.SafeDrop };
                    ViewModelAdapter.navigate("TenderCountingView", options);
                    break;
                case RetailOperation.DeclareStartAmount:
                    options = { nonSalesTenderType: Commerce.Model.Entities.TransactionType.StartingAmount };
                    Commerce.ViewModelAdapter.navigate("CashManagementView", options);
                    break;
                case RetailOperation.TenderRemoval:
                    options = { nonSalesTenderType: Commerce.Model.Entities.TransactionType.RemoveTender };
                    Commerce.ViewModelAdapter.navigate("CashManagementView", options);
                    break;
                case RetailOperation.FloatEntry:
                    options = { nonSalesTenderType: Commerce.Model.Entities.TransactionType.FloatEntry };
                    Commerce.ViewModelAdapter.navigate("CashManagementView", options);
                    break;
            }
        }
    }
}