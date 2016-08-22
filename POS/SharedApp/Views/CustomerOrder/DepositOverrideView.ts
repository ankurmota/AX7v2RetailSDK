/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Commerce.ViewModels.d.ts'/>
///<reference path='../Controls/CommonHeader.ts'/>
///<reference path='../ViewControllerBase.ts'/>

module Commerce.ViewControllers {
    "use strict";

    export class DepositOverrideViewController extends ViewControllerBase {
        public indeterminateWaitVisible: Observable<boolean>;
        public depositAmount: Observable<number>;
        public commonHeaderData: Controls.CommonHeaderData;
        public overriddenDepositAmount: Observable<string>;
        public isCustomerOrderPickup: boolean;
        public availableDepositAmount: number;
        public currentDeposit: number;
        private _cartViewModel: Commerce.ViewModels.CartViewModel;

        constructor() {
            super(true);

            this.indeterminateWaitVisible = ko.observable(false);

            this._cartViewModel = new Commerce.ViewModels.CartViewModel();
            this._cartViewModel.load();

            var cart: Model.Entities.Cart = this._cartViewModel.cart();
            this.currentDeposit = ObjectExtensions.isNullOrUndefined(cart.OverriddenDepositAmount) ?
                cart.RequiredDepositAmount : cart.OverriddenDepositAmount;
            this.overriddenDepositAmount = ko.observable(
                NumberExtensions.formatNumber(cart.OverriddenDepositAmount, NumberExtensions.getDecimalPrecision()));
            this.isCustomerOrderPickup = CustomerOrderHelper.isCustomerOrderPickup(cart);
            this.availableDepositAmount = cart.AvailableDepositAmount;

            this.initializeCommonHeader();
        }

        public clearDepositOverride(): void {
            this.indeterminateWaitVisible(true);

            this._cartViewModel.updateOverriddenDepositAmountAsync(null)
                .done(this.onDepositOverrideSuccess.bind(this))
                .fail(this.onDepositOverrideFailure.bind(this));
        }

        public depositOverrideHandler(numpadResult: Controls.NumPad.INumPadResult): void {

            var depositAmount: number = NumberExtensions.parseNumber(this.overriddenDepositAmount());
            if (isNaN(depositAmount)) {
                return;
            }

            this.indeterminateWaitVisible(true);
            this._cartViewModel.updateOverriddenDepositAmountAsync(depositAmount)
                .done(this.onDepositOverrideSuccess.bind(this))
                .fail(this.onDepositOverrideFailure.bind(this));
        }

        public  setFullOverridenDepositAmount(): void {
            this.overriddenDepositAmount(NumberExtensions.formatNumber(this.currentDeposit, NumberExtensions.getDecimalPrecision()));
        }

        public  setDepositAvailableAmount(): void {
            this.overriddenDepositAmount(NumberExtensions.formatNumber(this.availableDepositAmount, NumberExtensions.getDecimalPrecision()));
        }

        private onDepositOverrideSuccess(): void {
            this.indeterminateWaitVisible(false);
            Commerce.ViewModelAdapter.navigate("CartView");
        }

        private onDepositOverrideFailure(errors: Model.Entities.Error[]): void {
            this.indeterminateWaitVisible(false);
            NotificationHandler.displayClientErrors(errors);
        }

        private initializeCommonHeader(): void {
            this.commonHeaderData = new Commerce.Controls.CommonHeaderData();
            this.commonHeaderData.viewCategoryName(true);
            this.commonHeaderData.categoryName(Commerce.ViewModelAdapter.getResourceString("string_4600"));
            this.commonHeaderData.sectionTitle(Commerce.Formatters.CartTypeName(this._cartViewModel.cart()));
            this.commonHeaderData.viewSearchBox(false);
        }
    }

}