/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.ErrorHandling {
    "use strict";
    export class CommerceExceptionTypes {
        public static COMMERCE_EXCEPTION_TYPE: string = "CommerceException";
        public static DATA_VALIDATION_EXCEPTION_TYPE: string = "DataValidationException";
        public static CART_VALIDATION_EXCEPTION_TYPE: string = "CartValidationException";
        public static ITEM_DISCONTINUED_EXCEPTION_TYPE: string = "ItemDiscontinuedException";
        public static STORAGE_EXCEPTION_TYPE: string = "StorageException";
        public static PAYMENT_EXCEPTION_TYPE: string = "PaymentException";
        public static USER_AUTHENTICATION_EXCEPTION_TYPE: string = "UserAuthenticationException";
        public static DEVICE_AUTHENTICATION_EXCEPTION_TYPE: string = "DeviceAuthenticationException";
        public static MISSING_REQUIRED_REASON_CODE_EXCEPTION: string = "MissingRequiredReasonCodeException";
        public static TENDER_VALIDATION_EXCEPTION: string = "TenderValidationException";
    }
}