/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Commerce.Core.d.ts'/>

module Custom.Operations {
    "use strict";

    /**
     * Options passed to the AddCrossLoyaltyCard operation.
     */
    export interface IAddCrossLoyaltyCardOperationOptions extends Commerce.Operations.IOperationOptions {
        cardNumber: string;
    }

    export class AddCrossLoyaltyCardOperationHandler extends Commerce.Operations.OperationHandlerBase {
        /**
         * Executes the add cross loyalty card operation.
         * @param {IAddCrossLoyaltyCardOperationOptions} options The options containing the cross loyalty card number to be added.
         * @return {IAsyncResult<IOperationResult>} The async operation result.
         */
        public execute(options: IAddCrossLoyaltyCardOperationOptions): Commerce.IAsyncResult<Commerce.Operations.IOperationResult> {
            // sanitize options
            options = options || { cardNumber: undefined };

            var discountValue: number = 0;
            var queue: Commerce.AsyncQueue = new Commerce.AsyncQueue();

            queue.enqueue(() => {
                // asks for card number, activity to get the card number, this will prompt the UI
                var activity: Custom.Activities.GetCrossLoyaltyCardNumberActivity = new Custom.Activities.GetCrossLoyaltyCardNumberActivity(
                    { cart: Commerce.Session.instance.cart });

                // This is a response handler to handdle the response for retail server on context on acivity
                // It won't complete the acitivty until the user cancels or gets a response from retail server
                activity.responseHandler = (response: Custom.Activities.IGetCrossLoyaltyCardNumberActivityResponse) => {
                    var customerManager: Custom.Managers.IExtendedCustomerManager = <Custom.Managers.IExtendedCustomerManager>this.customerManager;
                    return customerManager.getCrossLoyaltyCardAction(response.cardNumber)
                        .done((result: number) => {
                            discountValue = result;
                        });
                };

                // if activity has response we complete, if not than we cancel the async queue
                return activity.execute().done(() => {
                    if (!activity.response) {
                        queue.cancel();
                        return;
                    }
                });

                // After getting discount from server you ask if discount needs to be applied show how much discount
                // will a customer get based on their loyalty points
            }).enqueue(() => {
                return Commerce.ViewModelAdapter.displayMessage(
                    Commerce.StringExtensions.format("Apply discount? ${0} amount", discountValue),
                    Commerce.MessageType.Info, Commerce.MessageBoxButtons.OKCancel, "Apply discount?", 0).done((result: Commerce.DialogResult) => {
                        if (result === Commerce.DialogResult.No) {
                            queue.cancel();
                        }
                    });

                // If customer agree to apply the discount (press Yes), apply the discount on the cart
            }).enqueue(() => {
                var discountOptions: Commerce.Operations.ITransactionDiscountOperationOptions = { cart: Commerce.Session.instance.cart, discountValue: discountValue };
                // run the total discount amount operation to apply the total discount on the cart

                var discountResult: any = this.operationsManager.runOperation(Commerce.Operations.RetailOperation.TotalDiscountAmount, discountOptions);

                return queue.cancelOn(discountResult);
            });

            // run the queue, at each point you cancel the queue
            return queue.run();
        }
    }
} 