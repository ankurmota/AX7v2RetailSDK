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
     * Provides the base type interface for payment trigger options.
     */
    export interface IPaymentTriggerOptions extends ITriggerOptions {
        cart: Proxy.Entities.Cart;
    }

    /**
     * Provides the type interface for pre add tender line trigger options.
     */
    export interface IPreAddTenderLineTriggerOptions extends IPaymentTriggerOptions {
        tenderLine: Proxy.Entities.TenderLine;
    }

    /**
     * Provides the type interfact to be implemented by add tender line triggers.
     */
    export interface IPreAddTenderLineTrigger extends ICancelableTrigger {
        execute(options: IPreAddTenderLineTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for pre payment trigger options.
     */
    export interface IPrePaymentTriggerOptions extends IPaymentTriggerOptions {
        tenderType: Proxy.Entities.TenderType;
    }

    /**
     * Provides the type interfact to be implemented by pre payment triggers.
     */
    export interface IPrePaymentTrigger extends ICancelableTrigger {
        execute(options: IPrePaymentTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post payment trigger options.
     */
    export interface IPostPaymentTriggerOptions extends IPaymentTriggerOptions {
        tenderLine: Proxy.Entities.TenderLine;
    }

    /**
     * Provides the type interfact to be implemented by post payment triggers.
     */
    export interface IPostPaymentTrigger extends ITrigger {
        execute(options: IPostPaymentTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface for pre void payment trigger options.
     */
    export interface IPreVoidPaymentTriggerOptions extends IPaymentTriggerOptions {
        tenderLines: Proxy.Entities.TenderLine[];
    }

    /**
     * Provides the type interfact to be implemented by pre void payment triggers.
     */
    export interface IPreVoidPaymentTrigger extends ICancelableTrigger {
        execute(options: IPreVoidPaymentTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post void payment trigger options.
     */
    export interface IPostVoidPaymentTriggerOptions extends IPaymentTriggerOptions {
        tenderLines: Proxy.Entities.TenderLine[];
    }

    /**
     * Provides the type interfact to be implemented by post void payment triggers.
     */
    export interface IPostVoidPaymentTrigger extends ITrigger {
        execute(options: IPostVoidPaymentTriggerOptions): IVoidAsyncResult;
    }
}