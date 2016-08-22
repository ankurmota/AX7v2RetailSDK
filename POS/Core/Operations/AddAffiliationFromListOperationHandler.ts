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
     * Options passed to the AddAffiliationFromList operation.
     */
    export interface IAddAffiliationFromListOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the AddAffiliationFromList operation.
     */
    export class AddAffiliationFromListOperationHandler extends OperationHandlerBase {
        /**
         * Executes the AddAffiliationFromList operation.
         * @param {IAddAffiliationFromListOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IAddAffiliationFromListOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            var cart: Model.Entities.Cart = Session.instance.cart;

            // Add/remove affiliations are not allowed if order status is not create or edit.
            if (cart.CartTypeValue === Commerce.Model.Entities.CartType.CustomerOrder
                && !StringExtensions.isNullOrWhitespace(cart.SalesId)
                && cart.CustomerOrderModeValue !== Model.Entities.CustomerOrderMode.CustomerOrderCreateOrEdit) {
                NotificationHandler.displayErrorMessage("string_4451"); // This operation can't be performed at this stage of the order.
            } else {
                Commerce.ViewModelAdapter.navigate("AffiliationsView");
            }

            return VoidAsyncResult.createResolved();
        }
    }
}