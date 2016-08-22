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
     * Provides the interface for application start trigger options.
     */
    export interface IApplicationStartTriggerOptions extends ITriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by application start triggers. 
     */
    export interface IApplicationStartTrigger extends ITrigger {
        execute(options: IApplicationStartTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the interface for application suspend trigger options.
     */
    export interface IApplicationSuspendTriggerOptions extends ITriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by application suspend triggers. 
     */
    export interface IApplicationSuspendTrigger extends ITrigger {
        execute(options: IApplicationSuspendTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the interface for pre-logon trigger options.
     */
    export interface IPreLogOnTriggerOptions extends ITriggerOptions {
        operatorId: string;
    }

    /**
     * Provides the type interface to be implemented by pre-logon triggers. 
     */
    export interface IPreLogOnTrigger extends ICancelableTrigger {
        execute(options: IPreLogOnTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the interface for post-logon trigger options.
     */
    export interface IPostLogOnTriggerOptions extends ITriggerOptions {
        employee: Model.Entities.Employee;
    }

    /**
     * Provides the type interface to be implemented by post-logon triggers. 
     */
    export interface IPostLogOnTrigger extends ITrigger {
        execute(options: IPostLogOnTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the interface for post-logoff trigger options.
     */
    export interface IPostLogOffTriggerOptions extends ITriggerOptions {
        employee: Model.Entities.Employee;
        wasSilent?: boolean;
    }

    /**
     * Provides the type interface to be implemented by post-logoff triggers. 
     */
    export interface IPostLogOffTrigger extends ITrigger {
        execute(options: IPostLogOffTriggerOptions): IVoidAsyncResult;
    }
}