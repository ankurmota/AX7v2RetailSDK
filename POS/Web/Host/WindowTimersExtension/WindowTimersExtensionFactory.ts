/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../../../SharedApp/Commerce.Core.d.ts'/>
///<reference path='MessageChannelWindowTimersExtension.ts'/>
///<reference path='SetTimeoutWindowTimersExtension.ts'/>

module Commerce.Host.CrossBrowser {
    "use strict";

    /**
     * Factory that creates an instance of WindowTimersExtension implementation, 
     * That is used add setImmediate functionality to all browsers.
     */
    export class WindowTimersExtensionFactory {

        private static _instance: WindowTimersExtension;

        /**
         * Creates an instance of WindowTimersExtension interface implementation.
         * 
         * @return The instance of WindowTimersExtension implementation created by the factory.
         * 
         * @remarks If window.setImmediate function is defined, just return reference to the window object, 
         * otherwise the fastest environment supported implementation.
         */
        public static get(): WindowTimersExtension {
            if (!ObjectExtensions.isNullOrUndefined(WindowTimersExtensionFactory._instance)) {
                return WindowTimersExtensionFactory._instance;
            } else {
                WindowTimersExtensionFactory._instance = WindowTimersExtensionFactory.createInternal();
                return WindowTimersExtensionFactory._instance;
            }
        }

        private static createInternal(): WindowTimersExtension {
            if (Commerce.ObjectExtensions.isFunction(window.setImmediate) && ObjectExtensions.isFunction(window.clearImmediate)) {
                return <WindowTimersExtension>window;
            } else if (Commerce.ObjectExtensions.isFunction((<any>window).MessageChannel)) {
                // MessageChannel based WindowTimersExtension implementation, where supported.
                return new MessageChannelWindowTimersExtension();
            } else {
                // Older browsers.
                return new SetTimeoutWindowTimersExtension();
            }
        }
    }
}