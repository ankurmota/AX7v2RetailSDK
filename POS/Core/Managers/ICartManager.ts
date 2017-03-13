/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/CommerceTypes.g.ts'/>
///<reference path='../Entities/Error.ts'/>
///<reference path='../IAsyncResult.ts'/>

module Commerce.Model.Managers {
    "use strict";

    
    export var ICartManagerName: string = "ICartManager";
    

    export interface ICartManager {
        /**
         * Creates an empty cart with a session identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        createEmptyCartAsync(): IVoidAsyncResult;

        /**
         * Creates or updates the cart.
         * @param {Entities.Cart} newCart The cart to be created or updated.
         * @return {IVoidAsyncResult} The async result.
         */
        createOrUpdateCartAsync(newCart: Entities.Cart): IVoidAsyncResult;

        /**
         * Gets the cart from Retail Server and sets it as the session cart.
         * @param {string} cartId The identifier of the cart to get.
         * @return {IVoidAsyncResult} The async result.
         */
        getCartByCartIdAsync(cartId: string): IVoidAsyncResult;

        /**
         * Adds or updates the affiliation on the cart.
         * @param {Entities.AffiliationLoyaltyTier[]} affiliations The affiliations.
         * @return {IVoidAsyncResult} The async result.
         */
        addAffiliationToCartAsync(affiliations: Entities.AffiliationLoyaltyTier[]): IVoidAsyncResult;

        /**
         * Add cart lines to the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be added.
         * @return {IVoidAsyncResult} The async result.
         */
        addCartLinesToCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult;

        /**
         * Adds a preprocessed tender line to the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to add to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addPreprocessedTenderLineToCartAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult;

        /**
         * Updates a preprocessed tender line in the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        updatePreprocessedTenderLineInCartAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult;

        /**
         * Update cart lines on the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be updated.
         * @return {IVoidAsyncResult} The async result.
         */
        updateCartLinesOnCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult;

        /**
         * Validates whether a tender line is valid for add.
         * Can be used for any tender line, but targeted to validate tender lines that go to a different source
         * than retail server for processing before retail server validation.
         * @param {Entities.TenderLine} tenderLine The  tender line to validate before adding the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        validateTenderLineForAddAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult;

        /**
         * Void cart lines on the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be voided.
         * @return {IVoidAsyncResult} The async result.
         */
        voidCartLinesOnCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult;

        /**
         * Add reason code lines to the cart.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The reason code lines to add to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addReasonCodeLinesToCartAsync(reasonCodeLines: Entities.ReasonCodeLine[]): IVoidAsyncResult;

        /**
         * Associates the given customer identifier to the session cart.
         * @param {string} customerId The customer identifier to be associated with the session cart.
         * @param {Entities.AffiliationLoyaltyTier[]} cartAffiliations The cart affiliations.
         * @return {IVoidAsyncResult} The async result.
         */
        addCustomerToCartAsync(customerId: string, cartAffiliations: Entities.AffiliationLoyaltyTier[]): IVoidAsyncResult;

        /**
         * Adds the discount code to cart.
         * @param {string} discountCode The discount code that will be added to cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addDiscountCodeToCartAsync(discountCode: string): IVoidAsyncResult;

        /**
         * Add customer account deposit lines to the cart
         * @param {Entities.CustomerAccountDeposit[]} customerAccountDepositLines The customer account deposit lines to be added to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addCustomerAccountDepositLinesToCartAsync(customerAccountDepositLines: Entities.CustomerAccountDepositLine[]): IVoidAsyncResult;

        /**
         * Add income/expense account lines to the cart
         * @param {Entities.IncomeExpenseLine[]} incomeExpenseLines The income/expense account lines to be added to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addIncomeExpenseLinesToCartAsync(incomeExpenseLines: Entities.IncomeExpenseLine[]): IVoidAsyncResult;

        /**
         * Adds a tender line to the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to add to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addTenderLineToCartAsync(tenderLine: Entities.CartTenderLine): IVoidAsyncResult;

        /**
         * Updates tender lines in the session cart.
         * @param {Entities.TenderLine[]} tenderLines The array of tender lines to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        updateTenderLinesInCartAsync(tenderLines: Entities.TenderLine[]): IVoidAsyncResult;

        /**
         * Adds a shipping address and delivery mode to the session cart.
         * @param {Entities.Address} shippingAddress The shipping address to be associated with the session cart.
         * @param {string} deliveryMode The delivery mode to be associated with the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        addShippingAddressToCartAsync(shippingAddress: Entities.Address, deliveryMode: string): IVoidAsyncResult;

        /**
         * Adds or updates the loyalty card on the cart.
         * @param {string} loyaltyCardId The identifier of the loyalty card.
         * @param {Entities.AffiliationLoyaltyTier[]} cartAffiliations The cart affiliations.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The reason code lines.
         * @return {IVoidAsyncResult} The async result.
         */
        addLoyaltyCardToCartAsync(loyaltyCardId: string,
            cartAffiliations: Entities.AffiliationLoyaltyTier[],
            reasonCodeLines?: Entities.ReasonCodeLine[]): IVoidAsyncResult;

        /**
         * Does the checkout of the session cart.
         * @param {string} recipientEmailAddress The email to send a receipt to.
         * @param {Entities.TokenizedPaymentCard} [tokenizedPaymentCard] The tokenized card to charge.
         * @return {IAsyncResult<Entities.SalesOrder>} The async result.
         */
        checkoutCartAsync(
            recipientEmailAddress: string,
            tokenizedPaymentCard?: Entities.TokenizedPaymentCard): IAsyncResult<Entities.SalesOrder>;

        /**
         * Voids the cart associated with the given identifier.
         * @param {string} cartId The identifier of the cart to be voided.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The array of reason code lines to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        voidCartAsync(cartId: string, reasonCodeLines: Entities.ReasonCodeLine[]): IVoidAsyncResult;

        /**
         * Suspends the cart associated with the given identifier.
         * @param {string} cartId The identifier of the cart to be suspended.
         * @return {IVoidAsyncResult} The async result.
         */
        suspendCartAsync(cartId: string): IVoidAsyncResult;

        /**
         * Gets all suspended carts.
         * @return {IAsyncResult<Entities.Cart[]>} The async result.
         */
        getSuspendedCarts(): IAsyncResult<Entities.Cart[]>;

        /**
         * Recalls the cart.
         * @param {string} cartId The identifier of the cart to be resumed.
         * @param {string} customerId The customer identifier of the cart to be resumed.
         * @return {IVoidAsyncResult} The async result.
         */
        resumeCartAsync(cartId: string, customerId: string): IVoidAsyncResult;

        /**
         * Voids the cart tender line.
         * @param {string} tenderLineId Tender line to void.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The array of reason code lines in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        voidTenderLineAsync(tenderLineId: string, reasonCodeLines: Entities.ReasonCodeLine[],
            isPreprocessed: boolean): IVoidAsyncResult;

        /**
         * Updates/sets the signature on a cart tender line.
         * @param {string} tenderLineId Tender line identifier to add the signature.
         * @param {string} signatureData The signature.
         * @return {IVoidAsyncResult} The async result.
         */
        updateTenderLineSignature(tenderLineId: string, signatureData: string): IVoidAsyncResult;

        /**
         * Adds the comment to cart.
         * @param {string} comment The comment string.
         * @return {IVoidAsyncResult} The async result.
         */
        addCartCommentAsync(comment: string): IVoidAsyncResult;

        /**
         * Adds the invoice comment to cart.
         * @param {string} comment The comment string.
         * @return {IVoidAsyncResult} The async result.
         */
        addInvoiceCommentAsync(comment: string): IVoidAsyncResult;

        /**
         * Recalls customer order.
         * @param {string} salesId The identifier of the sale.
         * @return {IVoidAsyncResult} The async result.
         */
        recallCustomerOrder(salesId: string): IVoidAsyncResult;

        /**
         * Recalls customer quote.
         * @param {string} quoteId The identifier of the quote.
         * @return {IVoidAsyncResult} The async result.
         */
        recallCustomerQuote(quoteId: string): IVoidAsyncResult;

        /**
         * Recalculates the order.
         * @param {string} orderId The identifier of the order to recalculate.
         * @return {IAsyncResult<Entities.Cart>} The async result.
         */
        recalculateOrderAsync(orderId: string): IAsyncResult<Entities.Cart>;

        /**
         * Get all affiliations.
         * @return {IAsyncResult<Entities.Affiliation[]>} The async result.
         */
        getAffiliationsAsync(): IAsyncResult<Entities.Affiliation[]>;

        /**
         * Gets the payments history.
         * @param {string} cartId The cart identifier.
         * @returns {IAsyncResult<Entities.TenderLine[]>} The async result.
         */
        getPaymentsHistoryAsync(cartId: string): IAsyncResult<Entities.TenderLine[]>;

        /**
         * Gets order delivery modes.
         * @param {string} shippingAddress Shipping address.
         * @param {string[]} [cartLineIds] The cart line identifiers.
         * @return {IAsyncResult<Entities.SalesLineDeliveryOption[]>} The async result.
         */
        getDeliveryModes(shippingAddress: Entities.Address, cartLineIds?: string[]): IAsyncResult<Entities.SalesLineDeliveryOption[]>;

        /**
         * Gets sales tax overrides for the current transaction/line.
         * @param {number} overrideBy Whether we are overriding by transaction (0) or by line items (1).
         * @return {IAsyncResult<Array<Entities.TaxOverride>>} The async result.
         */
        getTaxOverrides(overrideBy: number): IAsyncResult<Entities.TaxOverride[]>;

        /**
         * Sets customer order mode for cart.
         * @param {Model.Entities.CustomerOrderMode} mode Customer order mode.
         * @return {IVoidAsyncResult} The async result.
         */
        setCustomerOrderModeAsync(mode: Model.Entities.CustomerOrderMode): IVoidAsyncResult;

        /**
         * Update OverriddenDepositAmount for the session cart.
         * @param {number} overriddenDepositAmount The new deposit amount.
         * @return {IVoidAsyncResult} The async result.
         */
        updateOverriddenDepositAmountForCartAsync(overriddenDepositAmount: number): IVoidAsyncResult;

        /**
         * Updates cancellation charge to the cart.
         * @param {number} chargeAmount cancellation charge
         * @return {IVoidAsyncResult} The async result.
         */
        updateCancellationChargeAsync(chargeAmount: number): IVoidAsyncResult;

        /**
         * Issue a new gift card and add it to cart.
         * @param {string} giftCardId The gift card identifier.
         * @param {number} amount The amount to be issued on gift card.
         * @param {string} currency The currency of amount to be issued.
         * @param {string} lineDescription The line description.
         * @returns {IVoidAsyncResult} The async result.
         */
        issueGiftCardToCartAsync(giftCardId: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult;

        /**
         * Add a gift card to cart.
         * @param {string} giftCardId The gift card identifier.
         * @param {number} amount The amount to be added on gift card.
         * @param {string} currency The currency of amount to be added.
         * @param {string} lineDescription The cart line description.
         * @returns {IVoidAsyncResult} The async result.
         */
        addGiftCardToCartAsync(giftCardId: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult;

        /**
         * Price override.
         * @param {Commerce.Model.Entities.CartLine} cartLine The cart line to update price with the updated price in the Price field.
         * @return {IVoidAsyncResult} The async result.
         */
        priceOverrideAsync(cartLine: Commerce.Model.Entities.CartLine): IVoidAsyncResult;

        //demo4 //todo: AM
        getCartByCartIdForKitAsync(cartId: string): Commerce.Proxy.CartsDataServiceQuery;
        //demo4 end

        /**
         * Recalls a cart with all products in sales invoice (for return operation).
         * @param {string} salesId The identifier of the order to recalculate.
         * @return {IAsyncResult<Entities.Cart>} The async result.
         */
        recallSalesInvoice(salesId: string): IAsyncResult<Entities.Cart>;

        /**
         * Gets the card payment accept page url.
         * @param {boolean} cardPaymentEnabled Indicates whether card payment is enabled.
         * @param {boolean} cardTokenizationEnabled Indicates whether card tokenization is enabled.
         * @return {IAsyncResult<Entities.CardPaymentAcceptPoint>} The async result.
         */
        getCardPaymentAcceptPoint(cardPaymentEnabled: boolean, cardTokenizationEnabled: boolean): IAsyncResult<Entities.CardPaymentAcceptPoint>;

        /**
         * Gets the card payment accept result.
         * @param {string} resultAccessCode The card payment result access code.
         * @return {IAsyncResult<Entities.CardPaymentAcceptResult>} The async result.
         */
        retrieveCardPaymentAcceptResult(resultAccessCode: string): IAsyncResult<Entities.CardPaymentAcceptResult>;

        /**
         * Gets the scan result.
         * @param {string} scanDataLabel The text that was scanned or manually entered.
         * @return {IAsyncResult<Entities.ScanResult>} The async result.
         */
        getScanResult(scanDataLabel: string): IAsyncResult<Entities.ScanResult>;
    }
}
