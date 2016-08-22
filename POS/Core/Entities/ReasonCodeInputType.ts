/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {

    /**
     * The reason code input type enum. Maps to RetailInfocodeInputType enum in AX.
     */
    export enum ReasonCodeInputTypeEnum {

        /**
         * The default value.
         */
        None = 0,

        /**
         * The sub code.
         */
        SubCode = 1,

        /**
         * The date type.
         */
        Date = 2,

        /**
         * The numeric.
         */
        Numeric = 3,

        /**
         * The item type.
         */
        Item = 4,

        /**
         * The customer type.
         */
        Customer = 5,

        /**
         *  The staff type.
         */
        Staff = 6,

        /**
         * The text type.
         */
        Text = 9,

        /**
         * The sub code buttons.
         */
        SubCodeButtons = 10,

        /**
         * The age limit.
         */
        AgeLimit = 11,

        /**
         * Composite reason codes as subcodes.
         */
        CompositeSubCodes = 12
    }
}


