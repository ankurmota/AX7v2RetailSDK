/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Activities/GetCustomerAccountDepositAmountActivity.ts'/>
///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    import CustomerAccountDepositLine = Model.Entities.CustomerAccountDepositLine;
    import GetCustomerAccountDepositAmountActivity = Activities.GetCustomerAccountDepositAmountActivity;

    /**
     * The options passed to the CustomerAccountDeposit operation.
     */
    export interface ICustomerAccountDepositOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the CustomerAccountDeposit operation.
     */
    export class CustomerAccountDepositOperationHandler extends OperationHandlerBase {
        /**
         * Executes the CustomerAccountDeposit operation.
         * @param {ICustomerAccountDepositOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ICustomerAccountDepositOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || {};

            var amountDeposit: number;
            var customerAccountDepositLineCustomerAccount: string;

            var asyncQueue: AsyncQueue = new AsyncQueue().enqueue((): IAsyncResult<any> => {
                var activity: GetCustomerAccountDepositAmountActivity = new GetCustomerAccountDepositAmountActivity();
                return activity.execute().done((): void => {
                    if (!activity.response) {
                        asyncQueue.cancel();
                        return;
                    }

                    amountDeposit = activity.response;
                });
            }).enqueue((): IAsyncResult<any> => {
                customerAccountDepositLineCustomerAccount = Commerce.Session.instance.cart.CustomerId;
                return this.customerManager.getCustomerDetailsAsync(customerAccountDepositLineCustomerAccount)
                    .done((customerDetails: Proxy.Entities.ICustomerDetails) => {
                        if (!StringExtensions.isNullOrWhitespace(customerDetails.customer.InvoiceAccount)) {
                            customerAccountDepositLineCustomerAccount = customerDetails.customer.InvoiceAccount;
                        }
                    });
            }).enqueue((): IAsyncResult<any> => {
                    var customerAccountDepositLine: CustomerAccountDepositLine = {
                        Amount: amountDeposit,
                        CustomerAccount: customerAccountDepositLineCustomerAccount
                    };

                    return this.cartManager.addCustomerAccountDepositLinesToCartAsync([customerAccountDepositLine]);
            });

            return asyncQueue.run();
        }
    }
}