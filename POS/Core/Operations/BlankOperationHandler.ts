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
     * Options passed to the BlankOperation operation.
     */
    export interface IBlankOperationOptions extends IOperationOptions {
        operationId: string;
        operationData: IOperationOptions;
    }

    /**
     * Handler for the BlankOperation operation.
     */
    export class BlankOperationHandler extends OperationHandlerBase {
        private static _blankOperationsMap: { [operationId: string]: IOperationHandler } = {};

        /**
         * Registers or overrides a blank operation handler, given its identifier.
         * @param {string} blankOperationId The blank operation identifier.
         * @param {IOperationHandler} handler The blank operation handler.
         */
        public static registerBlankOperationHandler(blankOperationId: string, handler: IOperationHandler): void {
            if (StringExtensions.isNullOrWhitespace(blankOperationId)) {
                throw "Invalid blank operation identifier.";
            }

            BlankOperationHandler._blankOperationsMap[blankOperationId] = handler;
        }

        /**
         * Gets a blank operation handler given its identifier, or undefined if none found.
         * @param {string} blankOperationId The blank operation identifier.
         * @return {IOperationHandler} The blank operation handler.
         */
        public static getBlankOperationHandler(blankOperationId: string): IOperationHandler {
            return BlankOperationHandler._blankOperationsMap[blankOperationId];
        }

        /**
         * Executes the blank operation.
         * @param {IBlankOperationOptions} options The blank operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IBlankOperationOptions): IAsyncResult<IOperationResult> {
            // sanitize options
            options = options || { operationId: undefined, operationData: undefined };

            if (StringExtensions.isNullOrWhitespace(options.operationId)) {
                return VoidAsyncResult.createRejected([
                    new Model.Entities.Error(ErrorTypeEnum.INVALID_BLANK_OPERATION)
                ]);
            }

            // find blank operation by its identifier.
            var handler: IOperationHandler = BlankOperationHandler.getBlankOperationHandler(options.operationId);
            if (ObjectExtensions.isNullOrUndefined(handler) || ObjectExtensions.isNullOrUndefined(handler.execute)) {
                return VoidAsyncResult.createRejected([
                    new Model.Entities.Error(ErrorTypeEnum.INVALID_BLANK_OPERATION)
                ]);
            }

            // executes the blank operation handler
            return handler.execute(options.operationData);
        }
    }
}