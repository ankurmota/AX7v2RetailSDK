/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='DiscountOperationsHelper.ts'/>
///<reference path='OperationHandlerBase.ts' />

module Commerce.Operations {
    "use strict";

    /**
     * Operation handler for applying a line discount amount.
     */
    export class LineDiscountAmountOperationHandler extends OperationHandlerBase {
        /**
         * Executes the LineDiscountAmount operation.
         *
         * @param {ILineDiscountOperationOptions} options The options containing the cart lines to get discounts for and/or discounts to add to the cart lines.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: ILineDiscountOperationOptions): IAsyncResult<IOperationResult> {
            return DiscountOperationsHelper.createLineDiscountQueue(options, /* isPercent */ false).run();
        }
    }
}