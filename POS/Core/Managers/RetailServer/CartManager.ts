/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../Entities/Error.ts'/>
///<reference path='../../Extensions/StringExtensions.ts'/>
///<reference path='../../Utilities/CartHelper.ts'/>
///<reference path='../../Session.ts'/>
///<reference path='../Context/CommerceContext.g.ts'/>
///<reference path='../ICartManager.ts'/>

module Commerce.Model.Managers.RetailServer {
    "use strict";

    import Common = Proxy.Common;

    export class CartManager implements Commerce.Model.Managers.ICartManager {
        private static CARDPREFIXCHAR: string = "*";

        private _commerceContext: Proxy.CommerceContext = null;

        constructor(commerceContext: Proxy.CommerceContext) {
            this._commerceContext = commerceContext;
        }

        /**
         * Checks whether the specified tender lines are in the cart.
         * @param {string} cart The cart to check upon.
         * @param {string} tenderLines The tender lines to check upon.
         * @return {boolean} True: all tender lines are in cart; false: otherwise.
         */
        private static areTenderLinesInCart(cart: Entities.Cart, tenderLines: Entities.CartTenderLine[]): boolean {
            var someNotPresent: boolean = tenderLines.some((t: Entities.TenderLine) => !CartManager.isTenderLineInCart(cart, t));
            return someNotPresent === false;
        }

        /**
         * Checks whether the specified tender line is in the cart.
         * @param {Entities.Cart} cart The cart to check upon.
         * @param {Entities.CartTenderLine} tenderLine The tender line to check upon.
         * @return {boolean} True: tender line is in cart; false: otherwise.
         */
        private static isTenderLineInCart(cart: Entities.Cart, tenderLine: Entities.CartTenderLine): boolean {
            return cart.TenderLines.some((t: Entities.TenderLine) => t.TenderLineId === tenderLine.TenderLineId);
        }

        /**
         * Creates an empty cart with a session identifier.
         * @return {IVoidAsyncResult} The async result.
         */
        public createEmptyCartAsync(): IVoidAsyncResult {
            return this.createCartAsync();
        }

        /**
         * Creates or updates the cart.
         * @param {Entities.Cart} newCart The cart to be created or updated.
         * @return {IVoidAsyncResult} The async result.
         */
        public createOrUpdateCartAsync(newCart: Entities.Cart): IVoidAsyncResult {
            if (Session.instance.isCartInProgress) {
                return this.saveCartAsync(Session.instance.cart.Id, newCart);
            } else {
                return this.createCartAsync(newCart);
            }
        }

        /**
         * Gets the cart from Retail Server and sets it as the session cart.
         * @param {string} cartId The identifier of the cart to get.
         * @return {IVoidAsyncResult} The async result.
         */
        public getCartByCartIdAsync(cartId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).read();
            return request.execute<Entities.Cart>()
                .done((data: Entities.Cart) => {
                    Session.instance.cart = data;
                });
        }

        /**
         * Adds or updates the affiliation on the cart.
         * @param {Entities.AffiliationLoyaltyTier[]} affiliations The affiliations.
         * @return {IVoidAsyncResult} The async result.
         */
        public addAffiliationToCartAsync(affiliations: Entities.AffiliationLoyaltyTier[]): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;

            if (!ObjectExtensions.isNullOrUndefined(cart.AffiliationLines)) {
                // Find same reason codes of the affiliations in the cart 
                // and update the LineId and IsChanged value of the reason code line if there exist same reason code id.
                // This logic can void adding duplicate reason code line of the affiliation.

                // Create a Dictionary to keep the affiliations' reasonCodeLines.
                var affiliationReasonCodeDictionary: Dictionary<Entities.ReasonCodeLine> =
                    this.createAffiliationReasonCodeDictionary(affiliations);
                var cartAffiliationReasonCodeDictionary: Dictionary<Entities.ReasonCodeLine> =
                    this.createAffiliationReasonCodeDictionary(cart.AffiliationLines);

                cartAffiliationReasonCodeDictionary.forEach((affiliationReasonCodeKey: string, cartReasonCodeLine: Entities.ReasonCodeLine) => {
                    if (affiliationReasonCodeDictionary.hasItem(affiliationReasonCodeKey)) {
                        affiliationReasonCodeDictionary.getItem(affiliationReasonCodeKey).IsChanged = true;
                        affiliationReasonCodeDictionary.getItem(affiliationReasonCodeKey).LineId = cartReasonCodeLine.LineId;
                    }
                });
            }

            var newCart: Entities.Cart = {
                Id: cart.Id,
                CustomerId: cart.CustomerId,
                AffiliationLines: affiliations
            };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Add cart lines to the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be added.
         * @return {IVoidAsyncResult} The async result.
         */
        public addCartLinesToCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(cartLines)) {
                RetailLogger.genericError("The cart line collection does not have any elements.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!Session.instance.isCartInProgress) {
                asyncQueue.enqueue((): IAsyncResult<any> => { return this.createCartAsync(); });
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                return this.addCartLinesToCartAsyncImpl(Session.instance.cart.Id, cartLines);
            }).run();
        }

        /**
         * Update cart lines on the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be updated.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateCartLinesOnCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(cartLines)) {
                RetailLogger.genericError("The cart line collection does not have any elements.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Session.instance.cart.Id);
            return query.updateCartLines(cartLines).execute<Entities.Cart>()
                .done((updatedCart: Entities.Cart) => {
                    Commerce.Session.instance.cart = updatedCart;
                });
        }

        /**
         * Void cart lines on the cart.
         * @param {Entities.CartLine[]} cartLines The cart lines to be voided.
         * @return {IVoidAsyncResult} The async result.
         */
        public voidCartLinesOnCartAsync(cartLines: Entities.CartLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(cartLines)) {
                RetailLogger.genericError("The cart line collection does not have any elements.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);
            return query.voidCartLines(cartLines).execute<Entities.Cart>()
                .done((updatedCart: Entities.Cart) => {
                    Commerce.Session.instance.cart = updatedCart;
                });
        }

        /**
         * Add reason code lines to the cart.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The reason code lines to add to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addReasonCodeLinesToCartAsync(reasonCodeLines: Entities.ReasonCodeLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(reasonCodeLines)) {
                RetailLogger.genericError("The cart line collection does not have any elements.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = { Id: cart.Id, ReasonCodeLines: reasonCodeLines };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Associates the given customer identifier to the session cart.
         * @param {string} customerId The customer identifier to be associated with the session cart.
         * @param {Entities.AffiliationLoyaltyTier[]} cartAffiliations The cart affiliations.
         * @return {IVoidAsyncResult} The async result.
         */
        public addCustomerToCartAsync(customerId: string, cartAffiliations: Model.Entities.AffiliationLoyaltyTier[]): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = { Id: cart.Id, CustomerId: customerId, AffiliationLines: cartAffiliations };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Add customer account deposit lines to the cart
         * @param {Entities.CustomerAccountDeposit[]} customerAccountDepositLines The customer account deposit lines to be added to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addCustomerAccountDepositLinesToCartAsync(customerAccountDepositLines: Entities.CustomerAccountDepositLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(customerAccountDepositLines)) {
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.INVALID_CUSTOMER_ACCOUNT_DEPOSIT_LINE_COLLECTION)]);
            }

            var cart: Entities.Cart = Session.instance.cart;
            var updatedCart: Entities.Cart = {
                Id: cart.Id,
                CartTypeValue: Commerce.Model.Entities.CartType.AccountDeposit,
                CustomerAccountDepositLines: customerAccountDepositLines
            };

            return this.createOrUpdateCartAsync(updatedCart);
        }

        /**
         * Add income/expense account lines to the cart
         * @param {Entities.IncomeExpenseLine[]} incomeExpenseLines The income/expense account lines to be added to the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addIncomeExpenseLinesToCartAsync(incomeExpenseLines: Entities.IncomeExpenseLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(incomeExpenseLines)) {
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.INVALID_INCOME_EXPENSE_LINE_COLLECTION)]);
            }

            var cart: Entities.Cart = Session.instance.cart;
            var updatedCart: Entities.Cart = {
                Id: cart.Id,
                CartTypeValue: Commerce.Model.Entities.CartType.IncomeExpense,
                IncomeExpenseLines: incomeExpenseLines,
                ReasonCodeLines: cart.ReasonCodeLines
            };

            return this.createOrUpdateCartAsync(updatedCart);
        }

        /**
         * Adds a tender line to the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to add to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addTenderLineToCartAsync(tenderLine: Entities.CartTenderLine): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(tenderLine) || !StringExtensions.isNullOrWhitespace(tenderLine.TenderLineId)) {
                RetailLogger.genericError("The tender is invalid or not a new one.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!Session.instance.isCartInProgress) {
                asyncQueue.enqueue((): IAsyncResult<any> => { return this.createCartAsync(); });
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                return this.addTenderLineToCartAsyncImpl(Session.instance.cart.Id, tenderLine);
            }).run();
        }

        /**
         * Adds a preprocessed tender line to the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to add to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addPreprocessedTenderLineToCartAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(tenderLine) || !StringExtensions.isNullOrWhitespace(tenderLine.TenderLineId)) {
                RetailLogger.genericError("The tender is invalid or not a new one.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();
            if (!Session.instance.isCartInProgress) {
                asyncQueue.enqueue((): IAsyncResult<any> => { return this.createCartAsync(); });
            }

            return asyncQueue.enqueue((): IAsyncResult<any> => {
                return this.addUpdatePreprocessedTenderLineToCartAsyncImpl(tenderLine);
            }).run();
        }

        /**
         * Updates a preprocessed tender line in the session cart.
         * @param {Entities.TenderLine} tenderLine The tender line to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public updatePreprocessedTenderLineInCartAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(tenderLine) || StringExtensions.isNullOrWhitespace(tenderLine.TenderLineId)) {
                RetailLogger.genericError("The tender is invalid or not a new one.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            // Check that the tender line is in the cart
            var tenderLineInCart: boolean = false;
            var cartTenderLines: Entities.TenderLine[] = Session.instance.cart.TenderLines;
            if (ArrayExtensions.hasElements(cartTenderLines)) {
                tenderLineInCart = cartTenderLines.some((t: Entities.TenderLine) => tenderLine.TenderLineId === t.TenderLineId);
            }

            if (!tenderLineInCart) {
                RetailLogger.genericError("The tender is invalid or not a new one.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            return this.addUpdatePreprocessedTenderLineToCartAsyncImpl(tenderLine);
        }

        /**
         * Updates tender lines in the session cart.
         * @param {Entities.TenderLine[]} tenderLines The array of tender lines to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateTenderLinesInCartAsync(tenderLines: Entities.CartTenderLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(tenderLines)) {
                RetailLogger.genericError("The cart line collection does not have any elements.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var cart: Entities.Cart = Session.instance.cart;
            if (!CartManager.areTenderLinesInCart(cart, tenderLines)) {
                RetailLogger.genericError("The tender line collection to update does not all exist in card.");
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            var newCart: Entities.Cart = { Id: cart.Id, TenderLines: tenderLines };
            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Validates whether a tender line is valid for add.
         * Can be used for any tender line, but targeted to validate tender lines that go to a different source
         * than retail server for processing before retail server validation.
         * @param {Entities.TenderLine} tenderLine The  tender line to validate before adding the cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public validateTenderLineForAddAsync(tenderLine: Entities.TenderLine): IVoidAsyncResult {
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);

            return query.validateTenderLineForAdd(tenderLine).execute();
        }

        /**
         * Adds a shipping address and delivery mode to the session cart.
         * @param {Entities.Address} shippingAddress The shipping address to be associated with the session cart.
         * @param {string} deliveryMode The delivery mode to be associated with the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addShippingAddressToCartAsync(shippingAddress: Entities.Address, deliveryMode: string): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = {
                Id: cart.Id,
                ShippingAddress: shippingAddress,
                DeliveryMode: deliveryMode
            };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Update OverriddenDepositAmount for the session cart.
         * @param {number} overriddenDepositAmount The new deposit amount.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateOverriddenDepositAmountForCartAsync(overriddenDepositAmount: number): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = {
                Id: cart.Id,
                OverriddenDepositAmount: overriddenDepositAmount
            };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Adds or updates the loyalty card on the cart.
         * @param {string} loyaltyCardId The identifier of the loyalty card.
         * @param {Entities.AffiliationLoyaltyTier[]} cartAffiliations The cart affiliations.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The reason code lines.
         * @return {IVoidAsyncResult} The async result.
         */
        public addLoyaltyCardToCartAsync(
            loyaltyCardId: string,
            cartAffiliations: Model.Entities.AffiliationLoyaltyTier[],
            reasonCodeLines?: Entities.ReasonCodeLine[]): IVoidAsyncResult {
            if (!ArrayExtensions.hasElements(reasonCodeLines)) {
                reasonCodeLines = [];
            }
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = {
                Id: cart.Id,
                LoyaltyCardId: loyaltyCardId,
                AffiliationLines: cartAffiliations,
                ReasonCodeLines: reasonCodeLines
            };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Sets customer order mode for cart.
         * @param {Model.Entities.CustomerOrderMode} mode Customer order mode.
         * @return {IVoidAsyncResult} The async result.
         */
        public setCustomerOrderModeAsync(mode: Model.Entities.CustomerOrderMode): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = {
                Id: cart.Id,
                CustomerOrderModeValue: mode,
                CartTypeValue: Entities.CartType.CustomerOrder
            };

            if (mode === Entities.CustomerOrderMode.QuoteCreateOrEdit) {
                newCart.QuotationExpiryDate = cart.QuotationExpiryDate;
            }

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Does the checkout of the session cart.
         * @param {string} recipientEmailAddress The email to send a receipt to.
         * @param {Entities.TokenizedPaymentCard} [tokenizedPaymentCard] The tokenized card to charge.
         * @return {IAsyncResult<Entities.SalesOrder>} The async result.
         */
        public checkoutCartAsync(
            recipientEmailAddress: string,
            tokenizedPaymentCard?: Entities.TokenizedPaymentCard): IAsyncResult<Entities.SalesOrder> {

            // Make sure masked card number is really masked.
            if (tokenizedPaymentCard != null &&
                tokenizedPaymentCard.CardTokenInfo != null &&
                !StringExtensions.isEmpty(tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber) &&
                tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber.length > 4) {
                var cardNumberPrefixLength: number = tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber.length - 4;
                var last4Digits: string = tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber.substr(cardNumberPrefixLength);
                var cardNumberPrefix: string = StringExtensions.padLeft(StringExtensions.EMPTY, CartManager.CARDPREFIXCHAR, cardNumberPrefixLength);
                tokenizedPaymentCard.CardTokenInfo.MaskedCardNumber = cardNumberPrefix + last4Digits;
            }

            var emailAddress: string = recipientEmailAddress ? recipientEmailAddress : "";
            var request: Common.IDataServiceRequest = this._commerceContext.carts(Session.instance.cart.Id).checkout(
                emailAddress,
                tokenizedPaymentCard,
                NumberSequence.GetNextReceiptId(Session.instance.cart),
                null);

            return request.execute<Entities.SalesOrder>()
                .done((salesOrder: Entities.SalesOrder): void => { Session.instance.clearCart(); });
        }

        /**
         * Voids the cart associated with the given identifier.
         * @param {string} cartId The identifier of the cart to be voided.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The array of reason code lines to update in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public voidCartAsync(cartId: string, reasonCodeLines: Entities.ReasonCodeLine[]): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).void(reasonCodeLines);
            return request.execute<Entities.Cart>();
        }

        /**
         * Suspends the cart associated with the given identifier.
         * @param {string} cartId The identifier of the cart to be suspended.
         * @return {IVoidAsyncResult} The async result.
         */
        public suspendCartAsync(cartId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).suspend();
            return request.execute<Entities.Cart>();
        }

        /**
         * Recalls the cart.
         * @param {string} cartId The identifier of the cart to be resumed.
         * @param {string} customerId The customer identifier of the cart to be resumed.
         * @return {IVoidAsyncResult} The async result.
         */
        public resumeCartAsync(cartId: string, customerId: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).resume();
            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Gets all suspended carts.
         * @return {IAsyncResult<Entities.Cart[]>} The async result.
         */
        public getSuspendedCarts(): IAsyncResult<Entities.Cart[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.carts().getSuspended();
            return request.execute<Entities.Cart[]>();
        }

        /**
         * Get all affiliations.
         * @return {IAsyncResult<Entities.Affiliation[]>} The async result.
         */
        public getAffiliationsAsync(): IAsyncResult<Entities.Affiliation[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getAffiliations();
            return request.execute<Entities.Affiliation[]>();
        }

        /**
         * Voids the cart tender line.
         * @param {string} tenderLineId Tender line to void.
         * @param {Entities.ReasonCodeLine[]} reasonCodeLines The array of reason code lines in the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public voidTenderLineAsync(tenderLineId: string, reasonCodeLines: Entities.ReasonCodeLine[],
            isPreprocessed: boolean): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(Session.instance.cart.Id)
                .voidTenderLine(tenderLineId, reasonCodeLines, isPreprocessed);
            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Updates/sets the signature on a cart tender line.
         * @param {string} tenderLineId Tender line identifier to add the signature.
         * @param {string} signatureData The signature.
         * @return {IVoidAsyncResult} The async result.
         */
        public updateTenderLineSignature(tenderLineId: string, signatureData: string): IVoidAsyncResult {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(Session.instance.cart.Id)
                .updateTenderLineSignature(tenderLineId, signatureData);
            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Adds the comment to cart.
         * @param {string} comment The comment string.
         * @return {IVoidAsyncResult} The async result.
         */
        public addCartCommentAsync(comment: string): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = { Id: cart.Id, Comment: comment };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Adds the invoice comment to cart.
         * @param {string} comment The comment string.
         * @return {IVoidAsyncResult} The async result.
         */
        public addInvoiceCommentAsync(comment: string): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = { Id: cart.Id, InvoiceComment: comment };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Adds the discount code to cart.
         * @param {string} discountCode The discount code that will be added to cart.
         * @return {IVoidAsyncResult} The async result.
         */
        public addDiscountCodeToCartAsync(discountCode: string): IVoidAsyncResult {
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);

            return query.addDiscountCode(discountCode).execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Recalls customer order.
         * @param {string} salesId The identifier of the sale.
         * @return {IVoidAsyncResult} The async result.
         */
        public recallCustomerOrder(salesId: string): IVoidAsyncResult {
            var transactionId: string = NumberSequence.GetNextTransactionId();
            var request: Common.IDataServiceRequest = this._commerceContext.carts().recallOrder(transactionId, salesId);

            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Recalls customer quote.
         * @param {string} quoteId The identifier of the quote.
         * @return {IVoidAsyncResult} The async result.
         */
        public recallCustomerQuote(quoteId: string): IVoidAsyncResult {
            var transactionId: string = NumberSequence.GetNextTransactionId();
            var request: Common.IDataServiceRequest = this._commerceContext.carts().recallQuote(transactionId, quoteId);

            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Recalculates the order.
         * @param {string} orderId The identifier of the order to recalculate.
         * @return {IAsyncResult<Entities.Cart>} The async result.
         */
        public recalculateOrderAsync(orderId: string): IAsyncResult<Entities.Cart> {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(orderId).recalculateOrder();
            return request.execute<Entities.Cart>()
                .done((cart: Entities.Cart): void => { Session.instance.cart = cart; });
        }

        /**
         * Recalls a cart with all products in sales invoice (for return operation).
         * @param {string} salesId The identifier of the order to recalculate.
         * @return {IAsyncResult<Entities.Cart>} The async result.
         */
        public recallSalesInvoice(salesId: string): IAsyncResult<Entities.Cart> {
            var transactionId: string = NumberSequence.GetNextTransactionId();
            var request: Common.IDataServiceRequest = this._commerceContext.carts().recallSalesInvoice(transactionId, salesId);
            return request.execute<Entities.Cart>();
        }

        /**
         * Gets the payments history.
         * @param {string} cartId The cart identifier.
         * @returns {IAsyncResult<Entities.TenderLine[]>} The async result.
         */
        public getPaymentsHistoryAsync(cartId: string): IAsyncResult<Entities.TenderLine[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).getPaymentsHistory();
            return request.execute<Entities.TenderLine[]>();
        }

        /**
         * Gets order delivery modes.
         * @param {string} shippingAddress Shipping address.
         * @param {string[]} [cartLineIds] The cart line identifiers.
         * @return {IAsyncResult<Entities.SalesLineDeliveryOption[]>} The async result.
         */
        public getDeliveryModes(shippingAddress: Entities.Address, cartLineIds?: string[]): IAsyncResult<Entities.SalesLineDeliveryOption[]> {
            var lineShippingAddresses: Entities.LineShippingAddress[] = [];

            for (var i: number = 0; i < cartLineIds.length; i++) {
                var lineShippingAddress: Entities.LineShippingAddress = new Entities.LineShippingAddressClass();
                lineShippingAddress.LineId = cartLineIds[i];
                lineShippingAddress.ShippingAddress = shippingAddress;

                lineShippingAddresses.push(lineShippingAddress);
            }

            var request: Common.IDataServiceRequest = this._commerceContext.carts(Session.instance.cart.Id)
                .getLineDeliveryOptions(lineShippingAddresses);
            return request.execute<Entities.SalesLineDeliveryOption[]>();
        }

        /**
         * Gets sales tax overrides for the current transaction/line.
         * @param {number} overrideBy Whether we are overriding by transaction (0) or by line items (1).
         * @return {IAsyncResult<Array<Entities.TaxOverride>>} The async result.
         */
        public getTaxOverrides(overrideBy: number): IAsyncResult<Entities.TaxOverride[]> {
            var request: Common.IDataServiceRequest = this._commerceContext.storeOperations().getTaxOverrides(overrideBy.toString());
            return request.execute<Entities.TaxOverride[]>();
        }

        /**
         * Updates cancellation charge to the cart.
         * @param {number} chargeAmount cancellation charge
         * @return {IVoidAsyncResult} The async result.
         */
        public updateCancellationChargeAsync(chargeAmount: number): IVoidAsyncResult {
            var cart: Entities.Cart = Session.instance.cart;
            var newCart: Entities.Cart = {
                Id: cart.Id,
                CancellationChargeAmount: chargeAmount
            };

            return this.createOrUpdateCartAsync(newCart);
        }

        /**
         * Issue a new gift card and add it to cart.
         * @param {string} giftCardId The gift card identifier.
         * @param {number} amount The amount to be issued on gift card.
         * @param {string} currency The currency of amount to be issued.
         * @param {string} lineDescription The line description.
         * @returns {IVoidAsyncResult} The async result.
         */
        public issueGiftCardToCartAsync(giftCardId: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult {
            // This code assuming that Cart has session Id. This call should be made from SalesOperationManager, IssueGiftCard.
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);
            return query.issueGiftCard(giftCardId, amount, currency, lineDescription).execute<Entities.Cart>()
                .done((updatedCart: Entities.Cart): void => {
                    Commerce.Session.instance.cart = updatedCart;
                });
        }

        /**
         * Add a gift card to cart.
         * @param {string} giftCardId The gift card identifier.
         * @param {number} amount The amount to be added on gift card.
         * @param {string} currency The currency of amount to be added.
         * @param {string} lineDescription The cart line description.
         * @returns {IVoidAsyncResult} The async result.
         */
        public addGiftCardToCartAsync(giftCardId: string, amount: number, currency: string, lineDescription: string): IVoidAsyncResult {
            // This code assuming that Cart has session Id. This call should be made from SalesOperationManager, AddToGiftCard.
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);
            return query.refillGiftCard(giftCardId, amount, currency, lineDescription).execute<Entities.Cart>()
                .done((updatedCart: Entities.Cart): void => {
                    Commerce.Session.instance.cart = updatedCart;
                });
        }

        /**
         * Price override.
         * @param {Commerce.Model.Entities.CartLine} cartLine The cart line to update price with the updated price in the Price field.
         * @return {IVoidAsyncResult} The async result.
         */
        public priceOverrideAsync(cartLine: Commerce.Model.Entities.CartLine): IVoidAsyncResult {
            if (ObjectExtensions.isNullOrUndefined(cartLine)) {
                RetailLogger.modelManagersCartManagerFailedToOverridePriceNoCartLinesProvided();
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            // Check that the cart line have a LineId and a Price value
            if (ObjectExtensions.isNullOrUndefined(cartLine)
                || ObjectExtensions.isNullOrUndefined(cartLine.LineId)
                || ObjectExtensions.isNullOrUndefined(cartLine.Price)) {
                RetailLogger.modelManagersCartManagerFailedToOverridePriceNoCartLineOrPriceProvided();
                return VoidAsyncResult.createRejected([new Entities.Error(ErrorTypeEnum.APPLICATION_ERROR)]);
            }

            // Override the price
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);
            return query.overrideCartLinePrice(cartLine.LineId, cartLine.Price).execute<Entities.Cart>()
                .done((updatedCart: Entities.Cart): void => {
                    Commerce.Session.instance.cart = updatedCart;
                });
        }

        /**
         * Gets the card payment accept page url.
         * @param {boolean} cardPaymentEnabled Indicates whether card payment is enabled.
         * @param {boolean} cardTokenizationEnabled Indicates whether card tokenization is enabled.
         * @return {IAsyncResult<Entities.CardPaymentAcceptPoint>} The async result.
         */
        public getCardPaymentAcceptPoint(cardPaymentEnabled: boolean, cardTokenizationEnabled: boolean): IAsyncResult<Entities.CardPaymentAcceptPoint> {

            var cardPaymentAcceptPoint: Entities.CardPaymentAcceptPoint;

            return new AsyncQueue()
                // Get the merchant properties
                .enqueue(() => {
                    var cart: Entities.Cart = Session.instance.cart;
                    var hostPageOrigin: string = window.location.protocol + "//" + window.location.host;
                    var adaptorPath: string = undefined;
                    var applicationType: Proxy.Entities.ApplicationTypeEnum = Commerce.Host.instance.application.getApplicationType();
                    if (applicationType === Proxy.Entities.ApplicationTypeEnum.CloudPos) {
                        // The adaptor files (HTML/CSS) must be hosted on a HTTPS web server.
                        // They won't be access if hosted inside native apps.
                        adaptorPath = hostPageOrigin + "/Connectors/";
                    }

                    var cardPaymentAcceptSettings: Entities.CardPaymentAcceptSettings = {
                        HostPageOrigin: hostPageOrigin,
                        AdaptorPath: adaptorPath,
                        CardPaymentEnabled: cardPaymentEnabled,
                        PaymentAmount: (cardPaymentEnabled ? cart.AmountDue : undefined),
                        CardTokenizationEnabled: cardTokenizationEnabled
                    };

                    var request: Common.IDataServiceRequest = this._commerceContext.carts(cart.Id).getCardPaymentAcceptPoint(cardPaymentAcceptSettings);
                    return request.execute<Entities.CardPaymentAcceptPoint>()
                        .done((result: Entities.CardPaymentAcceptPoint) => {
                         cardPaymentAcceptPoint = result;
                    });
                }).run().map((result: ICancelableResult): Entities.CardPaymentAcceptPoint => {
                    return cardPaymentAcceptPoint;
                });
        }

        /**
         * Gets the card payment accept result.
         * @param {string} resultAccessCode The card payment result access code.
         * @return {IAsyncResult<Entities.CardPaymentAcceptResult>} The async result.
         */
        public retrieveCardPaymentAcceptResult(resultAccessCode: string)
            : IAsyncResult<Entities.CardPaymentAcceptResult> {
            var request: Common.IDataServiceRequest =
                this._commerceContext.carts().retrieveCardPaymentAcceptResult(resultAccessCode);
            return request.execute<Entities.CardPaymentAcceptResult>();
        }

        /**
         * Gets the scan result.
         * @param {string} scanDataLabel The text that was scanned or manually entered.
         * @return {IAsyncResult<Entities.ScanResult>} The async result.
         */
        public getScanResult(scanDataLabel: string): IAsyncResult<Entities.ScanResult> {
            var query: Common.DataServiceQuery<Entities.ScanResult> = this._commerceContext.scanResults(scanDataLabel);
            query.expand(Model.Entities.ScanResultClass.customerPropertyName);
            return query.read().execute<Entities.ScanResult>();
        }

        /**
         * Create cart implementation details.
         * @param {Entities.Cart} [cart] The cart to be created.
         * @return {IVoidAsyncResult} The async result.
         */
        private createCartAsync(cart?: Entities.Cart): IVoidAsyncResult {
            if (!cart) {
                cart = new Model.Entities.CartClass();
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var triggerOptions: Triggers.IBeginTransactionTriggerOptions = { cart: cart };
                return Triggers.TriggerManager.instance.execute(Triggers.NonCancelableTriggerType.BeginTransaction, triggerOptions);
            }).enqueue((): IVoidAsyncResult => {
                if (StringExtensions.isNullOrWhitespace(cart.Id)) {
                    cart.Id = NumberSequence.GetNextTransactionId();
                }

                var request: Common.IDataServiceRequest = this._commerceContext.carts().create(cart);
                return request.execute<Entities.Cart>()
                    .done((data: Entities.Cart): void => { Session.instance.cart = data; });
            });

            return asyncQueue.run();
        }

        /**
         * Updates the cart on the retail server.
         * @param {string} cartId The identifier of the cart to be updated.
         * @param {Entities.Cart} newCart The new cart containing the new updates.
         * @return {IVoidAsyncResult} The async result.
         */
        private saveCartAsync(cartId: string, newCart: Entities.Cart): IVoidAsyncResult {
            newCart.Id = cartId;

            var request: Common.IDataServiceRequest = this._commerceContext.carts(cartId).update(newCart);
            return request.execute<Entities.Cart>()
                .done((data: Entities.Cart): void => { Session.instance.cart = data; });
        }

        /**
         * Add cart lines to cart implementation.
         */
        private addCartLinesToCartAsyncImpl(cartId: string, cartLines: Entities.CartLine[]): IAsyncResult<Entities.Cart> {
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(cartId);

            return query.addCartLines(cartLines).execute<Entities.Cart>().done((updatedCart: Entities.Cart) => {
                Commerce.Session.instance.cart = updatedCart;
            });
        }


        /**
         * Add tender line to cart implementation.
         */
            private addTenderLineToCartAsyncImpl(cartId: string, tenderLine: Entities.CartTenderLine): IVoidAsyncResult {
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(cartId);
            return query.addTenderLine(tenderLine).execute<Entities.Cart>().done((updatedCart: Entities.Cart) => {
                Commerce.Session.instance.cart = updatedCart;

                // Update line display
                Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayBalance(updatedCart.TotalAmount, updatedCart.AmountDue);
            }).recoverOnFailure((errors: Model.Entities.Error[]): IAsyncResult<Entities.Cart> => {
                RetailLogger.modelManagersCartManagerAddTenderLineToCartFailed(errors[0].ErrorCode, ErrorHelper.formatErrorMessage(errors[0]));

                var originalTenderLineCollectionLength: number = Commerce.Session.instance.cart.TenderLines.length;
                if (ErrorHelper.hasError(errors, ErrorTypeEnum.SERVER_TIMEOUT)
                    || ErrorHelper.hasError(errors, ErrorTypeEnum.SERVICE_UNAVAILABLE)) {

                    var asyncResult: AsyncResult<Entities.Cart> = new AsyncResult<Entities.Cart>();

                    // server timed out, it's possible that it completed the operation but we never got the response back
                    this.getCartByCartIdAsync(cartId).always((): void => {
                        // check if the tender line was actually added
                        var newCart: Model.Entities.Cart = Commerce.Session.instance.cart;
                        if (originalTenderLineCollectionLength < newCart.TenderLines.length) {
                            // we have more tender lines than originally, our call succeeded
                            // report success to caller
                            asyncResult.resolve(newCart);
                        } else {
                            asyncResult.reject(errors);
                        }
                    });

                    return asyncResult;
                }

                // failure is not related to timeout, so nothing we can do, return failure to caller
                return AsyncResult.createRejected<Entities.Cart>(errors);
            });
        }

        /**
         * Add preprocessed tender line to cart implementation or updates a tender line in the cart.
         * @param {Entities.TenderLine} tenderLine The pre-processed tender line to add or update to the session cart.
         * @return {IVoidAsyncResult} The async result.
         */
        private addUpdatePreprocessedTenderLineToCartAsyncImpl(tenderLine: Entities.TenderLine): IVoidAsyncResult {
            var query: Proxy.CartsDataServiceQuery = this._commerceContext.carts(Commerce.Session.instance.cart.Id);

            if (!StringExtensions.isEmpty(tenderLine.MaskedCardNumber) &&
                tenderLine.MaskedCardNumber.length > 4) {
                var cardNumberPrefixLength: number = tenderLine.MaskedCardNumber.length - 4;
                var last4Digits: string = tenderLine.MaskedCardNumber.substr(cardNumberPrefixLength);
                var cardNumberPrefix: string = StringExtensions.padLeft(StringExtensions.EMPTY, CartManager.CARDPREFIXCHAR, cardNumberPrefixLength);
                tenderLine.MaskedCardNumber = cardNumberPrefix + last4Digits;
            }

            // This calls a method that is named add, but will add a tenderLine to the cart or update the tenderLine in the cart
            return query.addPreprocessedTenderLine(tenderLine).execute<Entities.Cart>().done((updatedCart: Entities.Cart) => {
                Commerce.Session.instance.cart = updatedCart;

                // Update line display
                Commerce.Peripherals.HardwareStation.LineDisplayHelper.displayBalance(updatedCart.TotalAmount, updatedCart.AmountDue);
            });
        }

        /**
         * Creates a dictionary to keep the affiliation reason code lines by a dictionary key(affiliationId_reaoncodeId).
         * @param {Entities.AffiliationLoyaltyTier[]} the affiliations.
         */
        private createAffiliationReasonCodeDictionary(affiliations: Entities.AffiliationLoyaltyTier[]): Dictionary<Entities.ReasonCodeLine> {
            var affiliationReasonCodeDictionary: Dictionary<Entities.ReasonCodeLine> = new Dictionary<Entities.ReasonCodeLine>();
            affiliations.forEach((affiliationLoyaltyTier: Entities.AffiliationLoyaltyTier) => {
                if (!ObjectExtensions.isNullOrUndefined(affiliationLoyaltyTier.ReasonCodeLines)) {
                    affiliationLoyaltyTier.ReasonCodeLines.forEach((affiliationReasonCodeLine: Entities.ReasonCodeLine) => {
                        affiliationReasonCodeDictionary.setItem(
                            affiliationLoyaltyTier.AffiliationId + "_" + affiliationReasonCodeLine.ReasonCodeId, affiliationReasonCodeLine);
                    });
                }
            });

            return affiliationReasonCodeDictionary;
        }
    }
}
