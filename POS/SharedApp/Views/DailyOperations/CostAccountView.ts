/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    //enum for transaction detail view mode
    export class CostAccountViewMode {
        static AccountSelection: string = "accountSelection";
        static AmountEntry: string = "amountEntry";
    }

    export class CostAccountViewController extends ViewControllerBase {
        public commonHeaderData;
        public cartViewModel: Commerce.ViewModels.CartViewModel;
        public storeOperationsViewModel: Commerce.ViewModels.StoreOperationsViewModel;
        public indeterminateWaitVisible: Observable<boolean>;
        public isOKDisabled: Observable<boolean>;
        public viewMode: Observable<CostAccountViewMode>;

        public accounts: ObservableArray<Model.Entities.IncomeExpenseAccount>;
        public accountType: Model.Entities.IncomeExpenseAccountType;
        public amountInput: Observable<string>;
        private _selectedAccount: Model.Entities.IncomeExpenseAccount;

        constructor(options: any) {
            super(true);

            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.cartViewModel = new Commerce.ViewModels.CartViewModel();
            this.storeOperationsViewModel = new Commerce.ViewModels.StoreOperationsViewModel();
            this.indeterminateWaitVisible = ko.observable(false);
            this.isOKDisabled = ko.observable(true);
            this.viewMode = ko.observable(CostAccountViewMode.AccountSelection);

            this.accounts = ko.observableArray(<Model.Entities.IncomeExpenseAccount[]>[]);
            this.accountType = options.accountType;
            this.amountInput = ko.observable("");
            this._selectedAccount = null;

            //Load Common Header 
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4054"));
            if (this.accountType == Model.Entities.IncomeExpenseAccountType.Income) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4055"));
            }
            else if (this.accountType == Model.Entities.IncomeExpenseAccountType.Expense) {
                this.commonHeaderData.sectionTitle(Commerce.ViewModelAdapter.getResourceString("string_4056"));
            }
        }

        public load() {
            this.storeOperationsViewModel.getIncomeExpenseAccountsAsync(this.accountType)
                .done((accounts) => { this.accounts(accounts); })
                .fail((errors) => { Commerce.NotificationHandler.displayClientErrors(errors); });
        }

        public selectionChangedHandler(activities: Model.Entities.IncomeExpenseAccount[]): void {
            this._selectedAccount = activities[0];

            if (!ObjectExtensions.isNullOrUndefined(this._selectedAccount)) {
                this.isOKDisabled(false);
            }
            else {
                this.isOKDisabled(true);
            }
        }

        public selectAccount() {
            if (!ObjectExtensions.isNullOrUndefined(this._selectedAccount)) {
                this.viewMode(CostAccountViewMode.AmountEntry);
                this.isOKDisabled(true);
            }
        }

        public saveAccount(): void {

            var numberEntered: number = NumberExtensions.parseNumber(this.amountInput());
            this.amountInput("");

            if (isNaN(numberEntered)) {
                Commerce.NotificationHandler.displayClientErrors([new Commerce.Model.Entities.Error(Commerce.ErrorTypeEnum.AMOUNT_IS_NOT_VALID)]);
                return;
            }

            var incomeExpenseLine: Model.Entities.IncomeExpenseLine = {};
            incomeExpenseLine.AccountTypeValue = this.accountType;
            incomeExpenseLine.IncomeExpenseAccount = this._selectedAccount.AccountNumber;

            var operationId: number;
            if (this.accountType === Model.Entities.IncomeExpenseAccountType.Expense) {
                operationId = Operations.RetailOperation.ExpenseAccounts
                incomeExpenseLine.Amount = (-1) * numberEntered;    // Expense line amount should be negated.
            } else {
                operationId = Operations.RetailOperation.IncomeAccounts;
                incomeExpenseLine.Amount = numberEntered;
            }

            var options: Operations.IOperationOptions = { incomeExpenseLine: incomeExpenseLine };
            Commerce.Operations.OperationsManager.instance.runOperation(operationId, options)
            .done(() => {
                Commerce.ViewModelAdapter.navigate("CartView");
            }).fail((errors: Model.Entities.Error[]) => {
                Commerce.NotificationHandler.displayClientErrors(errors);
            });
        }

        public cancelOperation() {
            Commerce.ViewModelAdapter.navigate("CartView");
        }
    }
}
