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
     * Provides the base type interface for tender declaration trigger options.
     */
    export interface ITenderDeclarationTriggerOptions extends ITriggerOptions {
        shift: Proxy.Entities.Shift;
    }

    /**
     * Provides the type interface for pre tender declaration trigger options.
     */
    export interface IPreTenderDeclarationTriggerOptions extends ITenderDeclarationTriggerOptions {
    }

    /**
     * Provides the type interface to be implemented by pre-triggers for the TenderDeclaration operation. 
     */
    export interface IPreTenderDeclarationTrigger extends ICancelableTrigger {
        execute(options: IPreTenderDeclarationTriggerOptions): IAsyncResult<ICancelableResult>;
    }

    /**
     * Provides the type interface for post tender declaration trigger options.
     */
    export interface IPostTenderDeclarationTriggerOptions extends ITenderDeclarationTriggerOptions {
        transaction: Proxy.Entities.DropAndDeclareTransaction;
    }

    /**
     * Provides the type interface to be implemented by post-triggers for the TenderDeclaration operation. 
     */
    export interface IPostTenderDeclarationTrigger extends ITrigger {
        execute(options: IPostTenderDeclarationTriggerOptions): IVoidAsyncResult;
    }
}