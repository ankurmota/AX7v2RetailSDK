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
     * Options passed to the IncomeAccounts operation.
     */
    export interface IIncomeAccountsOperationOptions extends IOperationOptions {
        incomeExpenseLine: Model.Entities.IncomeExpenseLine;
    }

    /**
     * Handler for the IncomeAccounts operation.
     */
    export class IncomeAccountsOperationHandler extends OperationHandlerBase {
        /**
         * Executes the IncomeAccounts operation.
         *
         * @param {IIncomeAccountsOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IIncomeAccountsOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { incomeExpenseLine: undefined };

            var asyncQueue: AsyncQueue = new AsyncQueue();

            if (ObjectExtensions.isNullOrUndefined(options.incomeExpenseLine)) {
                asyncQueue.enqueue(() => {
                    var activity: Activities.GetIncomeExpenseLineActivity =
                        new Activities.GetIncomeExpenseLineActivity({ accountType: Proxy.Entities.IncomeExpenseAccountType.Income });

                    return activity.execute()
                        .done(() => {
                            if (!activity.response) {
                                asyncQueue.cancel();
                                return;
                            }

                            options.incomeExpenseLine = activity.response;
                        });
                });
            }

            asyncQueue.enqueue(() => {
                var result: IAsyncResult<ICancelableResult> = ReasonCodesHelper.handleRequiredReasonCodesAsyncQueue(
                    { cart: Session.instance.cart },
                    (c: ReasonCodesContext) => { return this.cartManager.addIncomeExpenseLinesToCartAsync([options.incomeExpenseLine]); }).run();

                return asyncQueue.cancelOn(result);
            });

            return asyncQueue.run();
        }
    }
}