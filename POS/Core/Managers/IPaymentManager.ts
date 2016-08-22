/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var IPaymentManagerName: string = "IPaymentManager";
    

    export interface IPaymentManager {
        /**
         * Given a set of amounts in the same/different currencies, calculate and return the total of the amounts in the store currency.
         * @param {Entities.CurrencyRequest[]} currencyRequests The set of amounts.
         * @return {IAsyncResult<Entities.CurrencyAmount>} The async result.
         */
        calculateTotalCurrencyAmount(currencyRequests: Entities.CurrencyRequest[]): IAsyncResult<Entities.CurrencyAmount>;

        /**
         * Gets the amount, currency information, and denominations in the target currency.
         * @param {string} currencyCode The target currency.
         * @param {number} amount The amount to get in the target currency.
         * @return {IAsyncResult<Entities.CurrencyAmount[]>} The async result.
         */
        getCurrenciesAmount(currencyCode: string, amount: number): IAsyncResult<Entities.CurrencyAmount[]>;

        /**
         * Gets the credit memo by the credit memo identifier.
         * @param {string} creditMemoId The credit memo identifier.
         * @return {IAsyncResult<Entities.CreditMemo>} The async result.
         */
        getCreditMemoById(creditMemoId: string): IAsyncResult<Entities.CreditMemo>;

        /**
         * Gets the gift card by the gift card identifier.
         * @param {string} giftCardId The gift card identifier.
         * @return {IAsyncResult<Entities.GiftCard>} The async result.
         */
        getGiftCardById(giftCardId: string): IAsyncResult<Entities.GiftCard>;

        /**
         * Round payment amount by tender type.
         * @param {number} amount The amount to round.
         * @param {string} tenderTypeId The tender type identifier.
         * @return {IAsyncResult<number>} The async result.
         */
        roundAmountByTenderType(amount: number, tenderTypeId: string): IAsyncResult<number>;
    }
}
