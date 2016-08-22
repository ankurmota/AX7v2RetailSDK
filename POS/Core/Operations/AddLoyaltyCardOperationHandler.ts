/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the LoyaltyRequest operation.
     */
    export interface IAddLoyaltyCardOperationOptions extends IOperationOptions {
        /**
         * The loyalty card identifier to be added to the cart.
         */
        loyaltyCardId?: string;

        /**
         * The loyalty card.
         */
        loyaltyCard?: Proxy.Entities.LoyaltyCard;

        /**
         * The customer associated with the loyalty card.
         */
        customer?: Proxy.Entities.Customer;
    }

    /**
     * Handler for the LoyaltyRequest operation.
     */
    export class AddLoyaltyCardOperationHandler extends OperationHandlerBase {
        /**
         * Executes the LoyaltyRequest operation.
         * @param {IAddLoyaltyCardOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddLoyaltyCardOperationOptions): IAsyncResult<IOperationResult> {
            options = options || { loyaltyCardId: undefined, loyaltyCard: undefined, customer: undefined };

            var operationCanceled: boolean = false;
            var asyncQueue: AsyncQueue = new AsyncQueue();
            var shouldGetLoyaltyCard: boolean =
                ObjectExtensions.isNullOrUndefined(options.loyaltyCard) && StringExtensions.isNullOrWhitespace(options.loyaltyCardId);

            if (shouldGetLoyaltyCard) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var activity: Activities.GetLoyaltyCardActivity = new Activities.GetLoyaltyCardActivity(
                        { defaultLoyaltyCardId: Session.instance.cart.LoyaltyCardId });
                    activity.responseHandler = (response: Activities.GetLoyaltyCardActivityResponse): IVoidAsyncResult => {
                        var loyaltyCardId: string = response ? response.loyaltyCardId : null;
                        if (StringExtensions.isNullOrWhitespace(loyaltyCardId)) {
                            operationCanceled = true;
                            return VoidAsyncResult.createResolved();
                        }

                        return this.addLoyaltyCardToCartAsyncQueue(loyaltyCardId).run()
                            .done((result: ICancelableResult): void => { operationCanceled = result.canceled; });
                    };

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }
                    });
                });
            } else {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    return asyncQueue.cancelOn(this.addLoyaltyCardToCartAsyncQueue(options.loyaltyCardId, options.loyaltyCard, options.customer).run());
                });
            }

            return asyncQueue.run().map((result: ICancelableResult): IOperationResult => {
                return { canceled: result.canceled || operationCanceled };
            });
        }

        /**
         * Creates an async queue that adds the loyalty card identifier to the cart.
         * The queue handles customer affiliations and reason codes for both customer and affiliations.
         * @param {string} loyaltyCardId The loyalty card identifier.
         * @return {AsyncQueue} The async queue.
         */
        private addLoyaltyCardToCartAsyncQueue(
            loyaltyCardId: string,
            loyaltyCard?: Proxy.Entities.LoyaltyCard,
            customer?: Proxy.Entities.Customer): AsyncQueue {

            var addLoyaltyCardQueue: AsyncQueue = new AsyncQueue();
            var notInCartAffiliations: Proxy.Entities.AffiliationLoyaltyTier[] = [];

            if (ObjectExtensions.isNullOrUndefined(loyaltyCard)) {
                addLoyaltyCardQueue.enqueue((): IVoidAsyncResult => {
                    return this.customerManager.getLoyaltyCardAsync(loyaltyCardId)
                        .done((result: Proxy.Entities.LoyaltyCard) => {
                            if (ObjectExtensions.isNullOrUndefined(result)) {
                                NotificationHandler.displayErrorMessage(ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDLOYALTYCARDNUMBER);
                                 addLoyaltyCardQueue.cancel();
                                 return;
                            }

                            loyaltyCard = result;
                        });
                });
            }

            addLoyaltyCardQueue.enqueue((): IVoidAsyncResult => {
                // Check that the loyalty card exists and that there is an associated customer
                if (ObjectExtensions.isNullOrUndefined(loyaltyCard) || StringExtensions.isNullOrWhitespace(loyaltyCard.CustomerAccount)) {
                    return VoidAsyncResult.createResolved();
                } else if (!ObjectExtensions.isNullOrUndefined(customer)
                    && StringExtensions.compare(customer.AccountNumber, loyaltyCard.CustomerAccount) === 0) {
                    // If the customer is provided and the one associated with the loyalty card no need to get the customer details.
                    return VoidAsyncResult.createResolved();
                }

                // Get the customer details
                return this.customerManager.getCustomerDetailsAsync(loyaltyCard.CustomerAccount)
                    .done((result: Proxy.Entities.ICustomerDetails): void => { customer = result.customer; });
            }).enqueue((): IVoidAsyncResult => {
                // Get the customer required affiliations not in the cart and add them to the cart
                var notInCartCustomerAffiliations: Proxy.Entities.CustomerAffiliation[] = [];
                if (!ObjectExtensions.isNullOrUndefined(customer)) {
                    notInCartCustomerAffiliations = AddCustomerOperationHelper.getNotInCartCustomerAffiliations(
                        customer.AccountNumber, customer.CustomerAffiliations);
                    notInCartCustomerAffiliations.forEach((customerAffiliation: Proxy.Entities.CustomerAffiliation): void => {
                        // convert the customer affiliation to cart affiliations.
                        var affiliationLoyaltyTier: Proxy.Entities.AffiliationLoyaltyTier = new Proxy.Entities.AffiliationLoyaltyTierClass();
                        affiliationLoyaltyTier.AffiliationId = customerAffiliation.RetailAffiliationId;
                        affiliationLoyaltyTier.CustomerId = loyaltyCard.CustomerAccount;
                        affiliationLoyaltyTier.LoyaltyTierId = 0;
                        notInCartAffiliations.push(affiliationLoyaltyTier);
                    });
                }

                // Add loyalty card to the cart and handle the required reason codes.
                var retryQueue: AsyncQueue = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: Session.instance.cart, affiliationLines: notInCartAffiliations },
                    (context: ReasonCodesContext): IAsyncResult<any> => {
                        return this.cartManager.addLoyaltyCardToCartAsync(loyaltyCard.CardNumber, context.affiliationLines, context.cart.ReasonCodeLines);
                    });

                return addLoyaltyCardQueue.cancelOn(retryQueue.run());
            });

            return addLoyaltyCardQueue;
        }
    }
}