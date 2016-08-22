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
     * Provides the base type interface for discount trigger options.
     */
    export interface IDiscountTriggerOptions extends ITriggerOptions {
        cart: Model.Entities.Cart;
    }

    /**
     * Provides the base type interface for line discount trigger options.
     */
    export interface ILineDiscountTriggerOptions extends IDiscountTriggerOptions {
        cartLines: Model.Entities.CartLine[];
    }

    /**
     * Provides the type interface for pre line discount trigger options.
     */
    export interface IPreLineDiscountTriggerOptions extends ILineDiscountTriggerOptions {
    }

    /**
     * Provides the type interface for post line discount trigger options.
     */
    export interface IPostLineDiscountTriggerOptions extends ILineDiscountTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the line discount amount operation.
     */
    export interface IPreLineDiscountAmountTrigger extends ICancelableTrigger {
        execute(options: IPreLineDiscountTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the line discount amount operation.
     */
    export interface IPostLineDiscountAmountTrigger extends ITrigger {
        execute(options: IPostLineDiscountTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the line discount percent operation.
     */
    export interface IPreLineDiscountPercentTrigger extends ICancelableTrigger {
        execute(options: IPreLineDiscountTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the line discount percent operation.
     */
    export interface IPostLineDiscountPercentTrigger extends ITrigger {
        execute(options: IPostLineDiscountTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the total discount operation.
     */
    export interface IPreTotalDiscountTriggerOptions extends IDiscountTriggerOptions {
    }

    /**
     * Provides the type interface for post total discount trigger options.
     */
    export interface IPostTotalDiscountTriggerOptions extends IDiscountTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the total discount amount operation.
     */
    export interface IPreTotalDiscountAmountTrigger extends ICancelableTrigger {
        execute(options: IPreTotalDiscountTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the total discount amount operation.
     */
    export interface IPostTotalDiscountAmountTrigger extends ITrigger {
        execute(options: IPostTotalDiscountTriggerOptions): IVoidAsyncResult;
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the total discount percent operation.
     */
    export interface IPreTotalDiscountPercentTrigger extends ICancelableTrigger {
        execute(options: IPreTotalDiscountTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the total discount percent operation.
     */
    export interface IPostTotalDiscountPercentTrigger extends ITrigger {
        execute(options: IPostTotalDiscountTriggerOptions): IVoidAsyncResult;
    }
} 