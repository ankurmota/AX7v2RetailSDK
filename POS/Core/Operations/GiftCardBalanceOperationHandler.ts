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
     * Options passed to the GiftCardBalance operation.
     */
    export interface IGiftCardBalanceOperationOptions extends IOperationOptions {
        callerContext: any;

        /**
         * The function that handles retrieving and showing the gift card balance.
         */
        giftCardFunction: () => void;
    }

    /**
     * Handler for the GiftCardBalance operation.
     */
    export class GiftCardBalanceOperationHandler extends OperationHandlerBase {
        /**
         * Executes the GiftCardBalance operation.
         *
         * @param {IGiftCardBalanceOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IGiftCardBalanceOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { callerContext: undefined, giftCardFunction: undefined };

            if (ObjectExtensions.isNullOrUndefined(options.giftCardFunction)) {
                return VoidAsyncResult.createRejected([new Model.Entities.Error(ErrorTypeEnum.NOT_IMPLEMENTED)]);
            }

            options.giftCardFunction.call(options.callerContext);
            return VoidAsyncResult.createResolved();
        }
    }
}