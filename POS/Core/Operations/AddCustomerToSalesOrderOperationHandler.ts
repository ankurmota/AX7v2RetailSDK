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
     * Options passed to the AddCustomerToSalesOrder operation.
     */
    export interface IAddCustomerToSalesOrderOperationOptions extends IOperationOptions {
        /**
         * The customer identifier.
         */
        customerId: string;

        /**
         * The cart affiliations.
         */
        cartAffiliations: Proxy.Entities.AffiliationLoyaltyTier[];

        /**
         * The customer.
         */
        customer?: Proxy.Entities.Customer;
    }

    /**
     * Handler for the AddCustomerToSalesOrder operation.
     */
    export class AddCustomerToSalesOrderOperationHandler extends OperationHandlerBase {
        /**
         * Executes the AddCustomerToSalesOrder operation.
         * @param {IAddCustomerToSalesOrderOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddCustomerToSalesOrderOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cartAffiliations: undefined, customerId: undefined, customer: undefined };
            options.cartAffiliations = options.cartAffiliations || [];

            var cart: Proxy.Entities.Cart = Session.instance.cart;
            var currentCustomerAccount: string = cart.CustomerId;

            var customerAccount: string = options.customerId;
            if (currentCustomerAccount === customerAccount) {
                return VoidAsyncResult.createResolved();
            }

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (!StringExtensions.isNullOrWhitespace(customerAccount)) {
                asyncQueue.enqueue((): IAsyncResult<ICancelableResult> => {
                    var preTriggerOptions: Triggers.IPreCustomerSetTriggerOptions = { cart: Session.instance.cart, customerId: customerAccount };
                    var preTriggerResult: IAsyncResult<ICancelableResult> =
                        Triggers.TriggerManager.instance.execute(Triggers.CancelableTriggerType.PreCustomerSet, preTriggerOptions);

                    return asyncQueue.cancelOn(preTriggerResult);
                });
            }

            // If valid customer account and no affiliations passed in.
            if (!StringExtensions.isNullOrWhitespace(customerAccount)
                && !ArrayExtensions.hasElements(options.cartAffiliations)) {

                // If the customer was not provided get the customer details.
                if (ObjectExtensions.isNullOrUndefined(options.customer)) {
                    asyncQueue.enqueue((): IVoidAsyncResult => {
                        return this.customerManager.getCustomerDetailsAsync(customerAccount)
                            .done((customerDetails: Proxy.Entities.ICustomerDetails) => {
                                options.customer = customerDetails.customer;
                            });
                    });
                }

                asyncQueue.enqueue((): IVoidAsyncResult => {
                    options.customer.CustomerAffiliations.forEach((customerAffiliation: Proxy.Entities.CustomerAffiliation): void => {
                        // convert the customer affiliation to cart affiliation
                        var affiliationLoyaltyTier: Proxy.Entities.AffiliationLoyaltyTier = new Proxy.Entities.AffiliationLoyaltyTierClass();
                        affiliationLoyaltyTier.AffiliationId = customerAffiliation.RetailAffiliationId;
                        affiliationLoyaltyTier.CustomerId = customerAccount;
                        affiliationLoyaltyTier.LoyaltyTierId = 0;

                        options.cartAffiliations.push(affiliationLoyaltyTier);
                    });

                    // Merge the customer affiliations with the ones already existing.
                    options.cartAffiliations = AddCustomerOperationHelper.getUpdatedCartAffiliations(cart, customerAccount, options.cartAffiliations);
                    return VoidAsyncResult.createResolved();
                });
            }

            asyncQueue.enqueue((): IVoidAsyncResult => {
                var reasonCodeQueue: AsyncQueue = ActivityHelper.getStartOfTransactionReasonCodesAsyncQueue(Session.instance.cart);
                return asyncQueue.cancelOn(reasonCodeQueue.run());
            }).enqueue((): IVoidAsyncResult => {
                var newCart: Proxy.Entities.Cart = {
                    Id: Session.instance.cart.Id,
                    CustomerId: customerAccount,
                    AffiliationLines: options.cartAffiliations
                };

                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: newCart, affiliationLines: newCart.AffiliationLines },
                    (c: ReasonCodesContext) => { return this.cartManager.createOrUpdateCartAsync(c.cart); }).run();

                return asyncQueue.cancelOn(result);
            }).enqueue((): IVoidAsyncResult => {
                return this.postExecuteStepsAsync();
            });

            if (StringExtensions.isNullOrWhitespace(customerAccount)
                && !ArrayExtensions.hasElements(Session.instance.cart.CartLines)) {

                asyncQueue.enqueue((): IVoidAsyncResult => {
                    var options: IVoidTransactionOperationOptions = { cart: Session.instance.cart };
                    var operationResult: IAsyncResult<ICancelableResult> = this.operationsManager.runOperation(RetailOperation.VoidTransaction, options);

                    return asyncQueue.cancelOn(operationResult);
                });
            }

            return asyncQueue.run();
        }

        /**
         * Executes necessary steps after main 'execute' operation finished, to complete the operation.
         * @return {IVoidAsyncResult} The async result.
         */
        private postExecuteStepsAsync(): IVoidAsyncResult {
            var cart: Proxy.Entities.Cart = Session.instance.cart;
            var newCart: Proxy.Entities.Cart = new Proxy.Entities.CartClass();
            newCart.Id = cart.Id;

            // When updating customer, we need to clear delivery information
            var nonVoidedShippingCartLines: Proxy.Entities.CartLine[] = cart.CartLines
                .filter((cartLine: Proxy.Entities.CartLine) => {
                    return !cartLine.IsVoided
                        && cartLine.DeliveryMode !== this.applicationContext.channelConfiguration.PickupDeliveryModeCode;
                });

            var mustClearHeaderDeliveryInformation: boolean = cart.CartTypeValue === Proxy.Entities.CartType.CustomerOrder
                && (cart.CustomerOrderModeValue === Proxy.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit
                    || cart.CustomerOrderModeValue === Proxy.Entities.CustomerOrderMode.QuoteCreateOrEdit)
                && cart.DeliveryMode !== this.applicationContext.channelConfiguration.PickupDeliveryModeCode;

            var mustClearCartLinesDeliveryInformation: boolean = mustClearHeaderDeliveryInformation
                && ArrayExtensions.hasElements(nonVoidedShippingCartLines);

            if (mustClearHeaderDeliveryInformation) {
                DeliveryHelper.clearHeaderDeliveryInfo(newCart);
            }

            if (mustClearCartLinesDeliveryInformation) {
                DeliveryHelper.clearLinesDeliveryInformation(nonVoidedShippingCartLines);
            }

            if (!mustClearHeaderDeliveryInformation && !mustClearCartLinesDeliveryInformation) {
                return VoidAsyncResult.createResolved();
            }

            return new AsyncQueue().enqueue((): IVoidAsyncResult => {
                return this.cartManager.createOrUpdateCartAsync(newCart);
            }).enqueue((): IVoidAsyncResult => {
                if (mustClearCartLinesDeliveryInformation) {
                    return this.cartManager.updateCartLinesOnCartAsync(nonVoidedShippingCartLines);
                }

                return VoidAsyncResult.createResolved();
            }).run();
        }
    }
}