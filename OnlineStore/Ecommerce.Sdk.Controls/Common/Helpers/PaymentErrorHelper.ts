/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Contoso.Retail.Ecommerce.Sdk.Controls {
    "use strict";

    /**
     * Payment error type enum.
     */
    export class PaymentErrorTypeEnum {
        static 20001: string = "InvalidOperation";
        static 20002: string = "ApplicationError";
        static 20003: string = "GenericCheckDetailsForError";
        static 20004: string = "DONotAuthorized";
        static 20005: string = "UserAborted";
        static 20119: string = "InvalidArgumentTenderAccountNumber";
        static 21001: string = "LocaleNotSupported";
        static 21002: string = "InvalidMerchantProperty";
        static 22001: string = "CommunicationError";
        static 22010: string = "InvalidArgumentCardTypeNotSupported";
        static 22011: string = "VoiceAuthorizationNotSupported";
        static 22012: string = "ReauthorizationNotSupported";
        static 22013: string = "MultipleCaptureNotSupported";
        static 22014: string = "BatchCaptureNotSupported";
        static 22015: string = "UnsupportedCurrency";
        static 22016: string = "UnsupportedCountry";
        static 22017: string = "CannotReauthorizePostCapture";
        static 22018: string = "CannotReauthorizePostVoid";
        static 22019: string = "ImmediateCaptureNotSupported";
        static 22050: string = "CardExpired";
        static 22051: string = "ReferToIssuer";
        static 22052: string = "NoReply";
        static 22053: string = "HoldCallOrPickupCard";
        static 22054: string = "InvalidAmount";
        static 22055: string = "AccountLengthError";
        static 22056: string = "AlreadyReversed";
        static 22057: string = "CannotVerifyPin";
        static 22058: string = "InvalidCardNumber";
        static 22059: string = "InvalidCVV2";
        static 22060: string = "CashBackNotAvailable";
        static 22061: string = "CardTypeVerificationError";
        static 22062: string = "Decline";
        static 22063: string = "EncryptionError";
        static 22065: string = "NoActionTaken";
        static 22066: string = "NoSuchIssuer";
        static 22067: string = "PinTriesExceeded";
        static 22068: string = "SecurityViolation";
        static 22069: string = "ServiceNotAllowed";
        static 22070: string = "StopRecurring";
        static 22071: string = "WrongPin";
        static 22072: string = "CVV2Mismatch";
        static 22073: string = "DuplicateTransaction";
        static 22074: string = "Reenter";
        static 22075: string = "AmountExceedLimit";
        static 22076: string = "AuthorizationExpired";
        static 22077: string = "AuthorizationAlreadyCompleted";
        static 22078: string = "AuthorizationIsVoided";
        static 22090: string = "ProcessorDuplicateBatch";
        static 22100: string = "AuthorizationFailure";
        static 22102: string = "InvalidMerchantConfiguration";
        static 22103: string = "InvalidExpirationDate";
        static 22104: string = "InvalidCardholderNameFirstNameRequired";
        static 22105: string = "InvalidCardholderNameLastNameRequired";
        static 22106: string = "FilterDecline";
        static 22107: string = "InvalidAddress";
        static 22108: string = "CVV2Required";
        static 22109: string = "CardTypeNotSupported";
        static 22110: string = "UniqueInvoiceNumberRequired";
        static 22111: string = "PossibleDuplicate";
        static 22112: string = "ProcessorRequiresLinkedRefund";
        static 22113: string = "CryptoBoxUnavailable";
        static 22114: string = "CVV2Declined";
        static 22115: string = "MerchantIdInvalid";
        static 22116: string = "TranNotAllowed";
        static 22117: string = "TerminalNotFound";
        static 22118: string = "InvalidEffectiveDate";
        static 22119: string = "InsufficientFunds";
        static 22120: string = "ReauthorizationMaxReached";
        static 22121: string = "ReauthorizationNotAllowed";
        static 22122: string = "DateOfBirthError";
        static 22123: string = "EnterLesserAmount";
        static 22124: string = "HostKeyError";
        static 22125: string = "InvalidCashBackAmount";
        static 22126: string = "InvalidTransaction";
        static 22127: string = "ImmediateCaptureRequired";
        static 22128: string = "ImmediateCaptureRequiredMAC";
        static 22129: string = "MACRequired";
        static 22130: string = "BankcardNotSet";
        static 22131: string = "InvalidRequest";
        static 22132: string = "InvalidTransactionFee";
        static 22133: string = "NoCheckingAccount";
        static 22134: string = "NoSavingsAccount";
        static 22135: string = "RestrictedCardTemporarilyDisallowedFromInterchange";
        static 22136: string = "MACSecurityFailure";
        static 22137: string = "ExceedsWithdrawalFrequencyLimit";
        static 22138: string = "InvalidCaptureDate";
        static 22139: string = "NoKeysAvailable";
        static 22140: string = "KMESyncError";
        static 22141: string = "KPESyncError";
        static 22142: string = "KMACSyncError";
        static 22143: string = "ResubmitExceedsLimit";
        static 22144: string = "SystemProblemError";
        static 22145: string = "AccountNumberNotFoundForRow";
        static 22146: string = "InvalidTokenInfoParameterForRow";
        static 22147: string = "ExceptionThrownForRow";
        static 22148: string = "TransactionAmountExceedsRemaining";
        static 22149: string = "GeneralException";
        static 22150: string = "InvalidCardTrackData";
        static 22151: string = "InvalidResultAccessCode";
    }

    /**
     * Payment error helper.
     */
    export class PaymentErrorHelper {
        public static PaymentExceptionNamespace: string = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_";
        public static GeneralExceptionErrorCode: string = "22149";

        /**
         * Check for inner error messages in the given errors and convert them to client error message.
         * @param {CommerceProxy.ProxyError[]} errors The errors with inner exceptions.
         * @return {string[]} The client error messages.
         */
        public static ConvertToClientError(errors: CommerceProxy.ProxyError[]): string[] {
            var paymentErrors: CommerceProxy.ProxyError[] = [];
            var paymentSdkErrors: CommerceProxy.ProxyError[] = [];

            for (var i = 0; i < errors.length; i++) {
                // Tries to convert the inner commerceException into PaymentException and retrieves the PaymentSdkErrors list.
                var paymentException: CommerceProxy.Entities.PaymentException = <CommerceProxy.Entities.PaymentException>errors[i].commerceException;
                if (paymentException != null && Utils.hasElements(paymentException.PaymentSdkErrors)) {
                    paymentSdkErrors = PaymentErrorHelper.ConvertPaymentSdkErrorsToClientErrors(paymentException.PaymentSdkErrors);
                }
                
                if (Utils.hasElements(paymentSdkErrors)) {
                    // Appends to the error list, if there is any PaymentSdkErrors.
                    paymentErrors = paymentErrors.concat(paymentSdkErrors);
                } else {
                    // Otherwise, tries to map the numeric Payment SDK code to client error resource id and appends it to the error list. 
                    paymentErrors.push(PaymentErrorHelper.MapPaymentSdkErrorToClientError(errors[i]));
                }
            }

            return ErrorHelper.getErrorMessages(paymentErrors);
        }

        /**
         * Converts the list of Payment SDK errors to client errors.
         * @param {CommerceProxy.Entities.PaymentError[]} errors The payment errors.
         * @return {CommerceProxy.ProxyError[]} The client errors.
         */
        private static ConvertPaymentSdkErrorsToClientErrors(errors: CommerceProxy.Entities.PaymentError[]): CommerceProxy.ProxyError[]{
            var paymentErrors: CommerceProxy.ProxyError[] = [];
            for (var i = 0; i < errors.length; i++) {
                var code: string = Utils.isNullOrWhiteSpace(errors[i].Code) ? PaymentErrorTypeEnum[PaymentErrorHelper.GeneralExceptionErrorCode]
                    : errors[i].Code;
                paymentErrors.push(new CommerceProxy.ProxyError(
                    PaymentErrorHelper.PaymentExceptionNamespace + code.toUpperCase(),
                    errors[i].Message));
            }

            return paymentErrors;
        }

        /**
         * Checks for numeric payment SDK error codes in the given error and converts them to client payment exception error code.
         * @param {CommerceProxy.ProxyError} error The payment related error.
         * @return {CommerceProxy.ProxyError} The error with payment SDK error code converted to client payment exception error code.
         */
        public static MapPaymentSdkErrorToClientError(error: CommerceProxy.ProxyError): CommerceProxy.ProxyError {
            var result: string = PaymentErrorTypeEnum[error.ErrorCode];
            var paymentError: CommerceProxy.ProxyError = Utils.isNullOrUndefined(result) ? error
                : new CommerceProxy.ProxyError(
                    PaymentErrorHelper.PaymentExceptionNamespace + result.toUpperCase(),
                    error.ErrorMessage);

            return paymentError;
        }
    }
} 