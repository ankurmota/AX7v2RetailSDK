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
     * Options passed to the ExtendedLogOnOperation operation.
     */
    export interface IExtendedLogOnOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the ExtendedLogOn operation.
     */
    export class ExtendedLogOnOperationHandler extends OperationHandlerBase {

        /**
         * Executes the extended logon operation.
         * @param {IExtendedLogOnOperationOptions} options The extended logon options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IExtendedLogOnOperationOptions): IAsyncResult<IOperationResult> {
            ViewModelAdapter.navigate("ExtendedLogOnView");
            return VoidAsyncResult.createResolved();
        }
    }
}