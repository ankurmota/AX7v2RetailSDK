/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Operations {
    "use strict";

    /**
     * Helper for override tax operations.
     */
    export class OverrideTaxOperationsHelper {

        /**
         * Creates the queue for getting the list of tax overrides and selecting one among them.
         * Then the override is added to the context, which can be a cart or cart line.
         *
         * @param {Model.Entities.TaxOverrideBy} overrideType The override type.
         * @param {{ TaxOverrideCode?: string }} overrideTaxContext The override tax context.
         * @return {AsyncQueue} The async queue.
         */
        public static createOverrideTaxFromListQueue(overrideType: Model.Entities.TaxOverrideBy, overrideTaxContext: { TaxOverrideCode?: string }): AsyncQueue {
            var taxOverrides: Model.Entities.TaxOverride[] = [];
            var taxOverride: Model.Entities.TaxOverride = null;

            var asyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // gets the list of tax overrides
                    var cartManager = <Model.Managers.ICartManager>Model.Managers.Factory.GetManager(Model.Managers.ICartManagerName, null);
                    return cartManager.getTaxOverrides(overrideType)
                        .done((result) => { result.forEach(t => taxOverrides.push(t)); });
                }).enqueue(() => {
                    // checks if the list has elements
                    if (!ArrayExtensions.hasElements(taxOverrides)) {
                        var error = new Commerce.Model.Entities.Error(ErrorTypeEnum.NO_TAX_OVERRIDE_REASON_CODES_CONFIGURED);
                        return VoidAsyncResult.createRejected([error]);
                    }

                    return VoidAsyncResult.createResolved();
                }).enqueue(() => {
                    // selects one from the list
                    var activity = new Activities.SelectTaxOverrideActivity({ overrideType: overrideType, taxOverrides: taxOverrides });
                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        taxOverride = activity.response.taxOverride;
                    });
                }).enqueue(() => {
                    var operationId;
                    var options;

                    if (overrideType === Model.Entities.TaxOverrideBy.Line) {
                        operationId = RetailOperation.OverrideTaxLine;
                        options = <IOverrideLineProductTaxOperationOptions>{ cartLine: overrideTaxContext, taxOverride: taxOverride };
                    } else {
                        operationId = RetailOperation.OverrideTaxTransaction;
                        options = <IOverrideTransactionTaxOperationOptions>{ cart: overrideTaxContext, taxOverride: taxOverride };
                    }

                    // overrides the tax for either line or transaction
                    return OperationsManager.instance.runOperationWithoutPermissionsCheck(operationId, options);
                });

            return asyncQueue;
        }
    }
}