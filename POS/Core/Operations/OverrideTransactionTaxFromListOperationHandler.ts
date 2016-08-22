/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='OperationHandlerBase.ts' />
///<reference path='OverrideTaxOperationsHelper.ts'/>

module Commerce.Operations {
    "use strict";

    /**
     * Options passed to the OverrideTransactionTaxFromListOperationHandler operation.
     * It contains the cart where the tax override will be applied to.
     */
    export interface IOverrideTransactionTaxFromListOperationOptions extends IOperationOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Handler for the OverrideTransactionTaxFromList operation.
     */
    export class OverrideTransactionTaxFromListOperationHandler extends OperationHandlerBase {
        /**
         * Executes the OverrideTransactionTaxFromList operation.
         *
         * @param {IOverrideTransactionTaxFromListOperationOptions} options The options containing the cart to apply the tax override to.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IOverrideTransactionTaxFromListOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: Session.instance.cart };

            return OverrideTaxOperationsHelper.createOverrideTaxFromListQueue(
                Model.Entities.TaxOverrideBy.Cart, options.cart).run();
        }
    }
}