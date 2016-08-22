/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/CommerceTypes.g.ts'/>
///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Extensions/ObjectExtensions.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../RegularExpressionValidations.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../IPaymentManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class PaymentManager implements Commerce.Model.Managers.IPaymentManager {
        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Given a set of amounts in the same/different currencies, calculate and return the total of the amounts in the store currency.
         * @param {Entities.CurrencyRequest[]} currencyRequests The set of amounts.
         * @return {IAsyncResult<Entities.CurrencyAmount>} The async result.
         */
        public calculateTotalCurrencyAmount(currencyRequests: Entities.CurrencyRequest[]): IAsyncResult<Entities.CurrencyAmount> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().calculateTotalCurrencyAmount(currencyRequests);
            return request.execute<Entities.CurrencyAmount>();
        }

        /**
         * Gets the amount, currency information, and denominations in the target currency.
         * @param {string} currencyCode The target currency.
         * @param {number} amount The amount to get in the target currency.
         * @return {IAsyncResult<Entities.CurrencyAmount[]>} The async result.
         */
        public getCurrenciesAmount(currencyCode: string, amount: number): IAsyncResult<Entities.CurrencyAmount[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCurrenciesAmount(currencyCode, amount);
            return request.execute<Entities.CurrencyAmount[]>();
        }

        /**
         * Gets the credit memo by the credit memo identifier.
         * @param {string} creditMemoId The credit memo identifier.
         * @return {IAsyncResult<Entities.CreditMemo>} The async result.
         */
        public getCreditMemoById(creditMemoId: string): IAsyncResult<Entities.CreditMemo> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getCreditMemoById(creditMemoId);
            return request.execute<Entities.CreditMemo>();
        }

        /**
         * Gets the gift card by the gift card identifier.
         * @param {string} giftCardId The gift card identifier.
         * @return {IAsyncResult<Entities.GiftCard>} The async result.
         */
        public getGiftCardById(giftCardId: string): IAsyncResult<Entities.GiftCard> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getGiftCard(giftCardId);
            return request.execute<Entities.GiftCard>();
        }

        /**
         * Round payment amount by tender type.
         * @param {number} amount The amount to round.
         * @param {string} tenderTypeId The tender type identifier.
         * @return {IAsyncResult<number>} The async result.
         */
        public roundAmountByTenderType(amount: number, tenderTypeId: string): IAsyncResult<number> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().roundAmountByTenderType(amount, tenderTypeId);
            return request.execute<string>().map((result: string): number => parseFloat(result));
        }
    }
}
