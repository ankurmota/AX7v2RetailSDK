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
     * Provides the base interface for operation trigger options.
     */
    export interface IOperationTriggerOptions extends ITriggerOptions {
        operationId: number;
        operationOptions: Operations.IOperationOptions;
    }

    /**
     * Provides the type interface to be implemented by pre operation triggers. 
     */
    export interface IPreOperationTrigger extends ICancelableTrigger {
        execute(options: IOperationTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for the post operation trigger options.
     */
    export interface IPostOperationTriggerOptions extends IOperationTriggerOptions {
        data: any;
    }

    /**
     * Provides the type interface to be implemented by post operation triggers. 
     */
    export interface IPostOperationTrigger extends ITrigger {
        execute(options: IPostOperationTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for the operation failure trigger options.
     */
    export interface IOperationFailureTriggerOptions extends IOperationTriggerOptions {
        errors: Model.Entities.Error[];
    }

    /**
     * Provides the type interface to be implemented by operation failure triggers. 
     */
    export interface IOperationFailureTrigger extends ITrigger {
        execute(options: IOperationFailureTriggerOptions): IVoidAsyncResult;
    }
} 