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
     * Options passed to the SalesPerson operation.
     */
    export interface ISalesPersonOperationOptions extends IOperationOptions {
        /**
         * The cart to add the sales person to.
         */
        cart: Model.Entities.Cart;
    }

    export class SalesPersonOperationHandler extends OperationHandlerBase {
        /**
         * Executes the SalesPerson operation.
         * @param {ISalesPersonOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ISalesPersonOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { cart: undefined };
            options.cart = options.cart || new Model.Entities.CartClass({ Id: StringExtensions.EMPTY });

            var cart: Model.Entities.Cart = options.cart;
            var salesPersonId: string;
            var currentEmployees: Model.Entities.Employee[];

            var asyncQueue: AsyncQueue = new AsyncQueue()
                .enqueue(() => {
                    // Check that the cart is in customer order or quote create or edit mode.
                    if (!CustomerOrderHelper.isCustomerOrderOrQuoteCreationOrEdition(cart)) {
                        // The operation can't be performed on income or expense items.
                        var error: Model.Entities.Error = new Model.Entities.Error(
                            ErrorTypeEnum.CHANGE_SALES_PERSON_INVALID_CART_MODE);

                        return VoidAsyncResult.createRejected([error]);
                    }
                }).enqueue(() => {
                    // Get list of employees
                    return this.operatorManager.getEmployeesAsync()
                        .done((employees: Model.Entities.Employee[]) => {
                            currentEmployees = employees;
                        });
                }).enqueue(() => {
                    // Get a sales person from list of employees
                    var activity: Activities.GetSalesPersonActivity =
                        new Activities.GetSalesPersonActivity({ originalSalesPersonId: cart.StaffId, salesPersons: currentEmployees });

                    return activity.execute().done(() => {
                        if (!activity.response) {
                            asyncQueue.cancel();
                            return;
                        }

                        salesPersonId = activity.response.salesPersonId;
                    });
                }).enqueue(() => {
                    // Send change request
                    var newCart: Model.Entities.Cart = { Id: cart.Id, StaffId: salesPersonId };
                    return this.cartManager.createOrUpdateCartAsync(newCart);
                });

            return asyncQueue.run();
        }
    }
}