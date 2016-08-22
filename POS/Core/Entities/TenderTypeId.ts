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
    * Enum to represent the tender type id.
    *
    * @enum {number}
    */
    export enum TenderTypeId {

        /**
         * Default value, should not be used.
         */
        None = 0,
        Cash = 1,
        Check = 2,
        Cards = 3,
        CustomerAccount = 4,
        Other = 5,
        Currency = 6,
        Voucher = 7,
        GiftCard = 8,
        TenderRemoveFloat = 9,
        LoyaltyCards = 10
    }
}


