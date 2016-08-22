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
     * Options passed to the ShowJournal operation.
     */
    export interface IShowJournalOperationOptions extends IOperationOptions {
    }

    /**
     * Handler for the ShowJournal operation.
     */
    export class ShowJournalOperationHandler extends OperationHandlerBase {
        /**
         * Executes the ShowJournal operation.
         *
         * @param {IShowJournalOperationOptions} options The operation options.
         * @return {IAsyncResult<IOperationResult>} The async result containing the operation result, if any.
         */
        public execute(options: IShowJournalOperationOptions): IAsyncResult<IOperationResult> {
            // do not sanitize the options as ShowJournalView expects null/undefined for some flows

            Commerce.ViewModelAdapter.navigate("ShowJournalView", options);
            return VoidAsyncResult.createResolved();
        }
    }
}