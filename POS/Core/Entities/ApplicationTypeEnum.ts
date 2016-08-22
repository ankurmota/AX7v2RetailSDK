/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    "use strict";

    /**
     * Represents the application type enumerable.
     */
    export enum ApplicationTypeEnum {

        /**
         * Modern POS for Windows
         */
        MposForWindows = 2,

        /**
         * Modern POS for Non-Windows
         */
        MposForNonWindows = 3,

        /**
         * Modern POS for Windows Phone
         */
        MposForWindowsPhone = 1,

        /**
         * Cloud Pos
         */
        CloudPos = 4
    }
}


