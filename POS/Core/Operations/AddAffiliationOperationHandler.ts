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
     * Options passed to the AddAffiliation operation.
     */
    export interface IAddAffiliationOperationOptions extends IOperationOptions {
        affiliationNames: string[];
        affiliations: Proxy.Entities.Affiliation[];
    }

    /**
     * Handler for the AddAffiliation operation.
     */
    export class AddAffiliationOperationHandler extends OperationHandlerBase {
        /**
         * Executes the AddAffiliation operation.
         * @param {IAddAffiliationOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddAffiliationOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { affiliationNames: undefined, affiliations: undefined };
            options.affiliationNames = options.affiliationNames || [];
            options.affiliations = options.affiliations || [];

            var cart: Proxy.Entities.Cart = Session.instance.cart;

            // Add/Remove affiliations are not allowed if order status is not create or edit.
            if (cart.CartTypeValue === Proxy.Entities.CartType.CustomerOrder
                && !StringExtensions.isNullOrWhitespace(cart.SalesId)
                && cart.CustomerOrderModeValue != Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit) {

                // This operation can't be performed at this stage of the order.
                var error: Model.Entities.Error = new Model.Entities.Error(ErrorTypeEnum.CUSTOMER_ORDER_CANNOT_PERFORM_OPERATION); 
                return VoidAsyncResult.createRejected([error]);
            }

            var affiliationsToAdd: Proxy.Entities.AffiliationLoyaltyTier[] = options.affiliations.map(
                (a: Proxy.Entities.Affiliation) => this.convertToAffiliationLoyaltyTier(a));
            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (ArrayExtensions.hasElements(options.affiliationNames)) {
                asyncQueue.enqueue((): IVoidAsyncResult => {
                    return this.cartManager.getAffiliationsAsync()
                        .done((result: Proxy.Entities.Affiliation[]) => {
                            result.forEach((a: Proxy.Entities.Affiliation) => {
                                if (options.affiliationNames.some((n: string) => a.Name === n)) {
                                    affiliationsToAdd.push(this.convertToAffiliationLoyaltyTier(a));
                                }
                            });
                        });
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                if (!ArrayExtensions.hasElements(affiliationsToAdd)) {
                    return VoidAsyncResult.createResolved();
                }

                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { affiliationLines: affiliationsToAdd },
                    (context: ReasonCodesContext) => { return this.cartManager.addAffiliationToCartAsync(context.affiliationLines); }).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }

        private convertToAffiliationLoyaltyTier(affiliation: Proxy.Entities.Affiliation): Proxy.Entities.AffiliationLoyaltyTier {
            var affiliationLoyaltyTier: Proxy.Entities.AffiliationLoyaltyTier = new Proxy.Entities.AffiliationLoyaltyTierClass();
            affiliationLoyaltyTier.AffiliationId = affiliation.RecordId;
            affiliationLoyaltyTier.LoyaltyTierId = 0;
            affiliationLoyaltyTier.ReasonCodeLines = [];
            return affiliationLoyaltyTier;
        }
    }
}