/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ITrigger.ts' />

module Commerce.Triggers {
    "use strict";

    /**
     * Provides the type interface for pre print receipt copy trigger options.
     */
    export interface IPrePrintReceiptCopyTriggerOptions extends ITriggerOptions {
        salesOrder: Proxy.Entities.SalesOrder;
        receipts: Proxy.Entities.PrintableReceipt[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for printing a copy of a receipt.
     */
    export interface IPrePrintReceiptCopyTrigger extends ICancelableTrigger {
        execute(options: Operations.IItemSaleOperationOptions): IAsyncResult<ICancelableResult>;
    }
}