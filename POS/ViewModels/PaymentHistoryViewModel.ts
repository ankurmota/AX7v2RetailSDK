/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ViewModelBase.ts'/>

module Commerce.ViewModels {
    "use strict";

    /**
     * Represents the payment view model.
     */
    export class PaymentHistoryViewModel extends ViewModelBase {

        /**
         * Instantiates the payments history view model.
         */
        constructor() {
            super();
        }

        /**
         * Get payments history from the designated cart.
         * @returns {Proxy.Entities.TenderLine[]} List of paid tender lines.
         */
        public getPaymentsHistory(cart: Proxy.Entities.Cart): IAsyncResult<Proxy.Entities.TenderLine[]> {
            var operationOptions: Operations.IGetPaymentsHistoryOperationOptions = { cart: cart };

            return this.operationsManager.runOperation<Proxy.Entities.TenderLine[]>(Operations.RetailOperation.PaymentsHistory, operationOptions)
                .map((result: Operations.IOperationResult) => {
                    return result.data;
                });
        }
    }
}