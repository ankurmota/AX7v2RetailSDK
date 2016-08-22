/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Triggers {
    "use strict";

    /**
     * Provides a typed interface for trigger options.
     */
    export interface ITriggerOptions {
    }

    /**
     * This interface enforces a type on the requirements for a trigger implementation.
     * A trigger must always implement an execute method.
     */
    export interface ITrigger {
        execute(options: ITriggerOptions): IVoidAsyncResult;
    }

    /**
     * This interface enforces a type on the requirements for a cancelable trigger implementation.
     * A trigger must always implement an execute method, and for cancelable triggers the signature should match this more specific definition.
     */
    export interface ICancelableTrigger extends ITrigger {
        execute(options: ITriggerOptions): IAsyncResult<ICancelableResult>;
    }
} 