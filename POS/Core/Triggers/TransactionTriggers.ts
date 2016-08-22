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
     * Provides the base options type interface for transaction triggers.
     */
    export interface ITransactionTriggerOptions extends ITriggerOptions {
        cart: Proxy.Entities.Cart;
    }

    /**
     * Provides the options type interface for begin transaction triggers.
     */
    export interface IBeginTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by begin transaction triggers.
     */
    export interface IBeginTransactionTrigger extends ITrigger {
        execute(options: IBeginTransactionTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the base options type interface for return transaction triggers.
     */
    export interface IReturnTransactionTriggerOptions extends ITransactionTriggerOptions {
        originalTransaction: Proxy.Entities.SalesOrder;
    }

    /**
     * Provides the options type interface for pre confirm return transaction triggers.
     */
    export interface IPreConfirmReturnTransactionTriggerOptions extends IReturnTransactionTriggerOptions {
        employee: Proxy.Entities.Employee;
        shift: Proxy.Entities.Shift;
    }

    /**
     * Provides the type interface to be implemented by pre confirm return transaction triggers.
     */
    export interface IPreConfirmReturnTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreConfirmReturnTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface for post return transaction triggers.
     */
    export interface IPostReturnTransactionTriggerOptions extends IReturnTransactionTriggerOptions {
        cartLinesForReturn: Proxy.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the return transaction operation.
     */
    export interface IPostReturnTransactionTrigger extends ITrigger {
        execute(options: IPostReturnTransactionTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the options type interface for pre return transaction triggers.
     */
    export interface IPreReturnTransactionTriggerOptions extends IReturnTransactionTriggerOptions {
        cartLinesForReturn: Proxy.Entities.CartLine[];
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the return transaction operation.
     */
    export interface IPreReturnTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreReturnTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface for pre end transaction triggers.
     */
    export interface IPreEndTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre end transaction triggers.
     */
    export interface IPreEndTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreEndTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface for post end transaction triggers.
     */
    export interface IPostEndTransactionTriggerOptions extends ITriggerOptions {
        receipts: Proxy.Entities.Receipt[];
    }

    /**
     * Provides the type interface to be implemented by post end transaction triggers.
     */
    export interface IPostEndTransactionTrigger extends ITrigger {
        execute(options: IPostEndTransactionTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the options type interface for pre void transaction triggers.
     */
    export interface IPreVoidTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the void transaction operation.
     * @remarks These triggers will be executed after the user confirms they want to void the transaction, but prior to voiding the transaction.
     */
    export interface IPreVoidTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreVoidTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface for post void transaction triggers.
     */
    export interface IPostVoidTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the void transaction operation.
     */
    export interface IPostVoidTransactionTrigger extends ITrigger {
        execute(options: IPostVoidTransactionTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the options type interface for pre suspend transaction triggers.
     */
    export interface IPreSuspendTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the suspend transaction operation.
     */
    export interface IPreSuspendTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreSuspendTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface post suspend transaction triggers.
     */
    export interface IPostSuspendTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the suspend transaction operation.
     */
    export interface IPostSuspendTransactionTrigger extends ITrigger {
        execute(options: IPostSuspendTransactionTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the options type interface for pre recall transaction triggers.
     */
    export interface IPreRecallTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the recall transaction operation.
     */
    export interface IPreRecallTransactionTrigger extends ICancelableTrigger {
        execute(options: IPreRecallTransactionTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the options type interface post recall transaction triggers.
     */
    export interface IPostRecallTransactionTriggerOptions extends ITransactionTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the recall transaction operation.
     */
    export interface IPostRecallTransactionTrigger extends ITrigger {
        execute(options: IPostRecallTransactionTriggerOptions): IVoidAsyncResult;
    }
} 