/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/PlatformType.ts'/>
///<reference path='../IAsyncResult.ts'/>
///<reference path='../WinJS.d.ts'/>
///<reference path='IApplicationIdentity.ts'/>

module Commerce.Host {
    "use strict";

    /**
     * Application related API.
     */
    export interface IApplication {

        /**
         * Determines whether the activation kind is an application launch kind.
         * @param {any} activationKind Activation kind value.
         * @return {boolean} True if launching otherwise false.
         */
        isApplicationLaunching(activationKind: any): boolean;

        /**
         * Gets the current application identity.
         * @return {IApplicationIdentity} Application identity.
         */
        getApplicationIdentity(): IApplicationIdentity;

        /**
         * Gets the current platform type.
         * @return {Commerce.Model.Entities.ArchitectureType} x86, ARM, x64, etc.
         */
        getArchitectureType(): Commerce.Proxy.Entities.ArchitectureType;

        /**
         * Gets the current application type.
         * @returns {Commerce.Proxy.Entities.ApplicationTypeEnum} The application type.
         */
        getApplicationType(): Commerce.Proxy.Entities.ApplicationTypeEnum;

        /**
         * Registers the binding to select on swipe for winJS grid elements.
         * @param {HTMLElement} [grid] html element used for winJS grid.
         */
        registerSwipeBinding(grid: HTMLElement): void;

        /**
         * Gets an app specific hardware identifier.
         * @return {string} The app specific hardware identifier as a string.
         */
        getAppSpecificHardwareId(): string;

        /**
         * Gets the current browser
         * @return {Commerce.Host.BrowserType} IE, Chrome, Firefox, etc.
         */
        getBrowserType(): Commerce.Host.BrowserType;
    }
}