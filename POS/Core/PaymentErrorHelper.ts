/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='Entities/Error.ts'/>

module Commerce {
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
        static 21033: string = "InvalidVoiceAuthorizationCode";
        static 21035: string = "CashBackAmountExceedsTotalAmount";
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
        
        public static PAYMENT_EXCEPTION_NAMESPACE: string = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_";
        public static GENERAL_EXCEPTION_ERROR_CODE: string = "22149";
        

        /**
         * Converts and aggregates the payment errors, payment SDK errors, and payment error codes into client errors.
         * @param {Model.Entities.Error[]} errors The errors with inner exceptions.
         * @return {Model.Entities.Error[]} The errors with inner exceptions expanded.
         */
        public static ConvertToClientErrors(errors: Model.Entities.Error[]): Model.Entities.Error[] {
            var paymentErrors: Model.Entities.Error[] = [];
            var paymentSdkErrors: Model.Entities.Error[] = [];

            for (var i: number = 0; i < errors.length; i++) {
                // Tries to convert the inner commerceException into PaymentException and retrieves the PaymentSdkErrors list.
                var paymentException: Model.Entities.PaymentException = <Model.Entities.PaymentException>errors[i].commerceException;
                if (paymentException != null && ArrayExtensions.hasElements(paymentException.PaymentSdkErrors)) {
                    paymentSdkErrors = PaymentErrorHelper.ConvertPaymentSdkErrorsToClientErrors(paymentException.PaymentSdkErrors);
                }

                if (ArrayExtensions.hasElements(paymentSdkErrors)) {
                    // Appends to the error list, if there is any PaymentSdkErrors.
                    paymentErrors = paymentErrors.concat(paymentSdkErrors);
                } else {
                    // Otherwise, tries to map the numeric Payment SDK code to client error resource id and appends it to the error list. 
                    paymentErrors.push(PaymentErrorHelper.MapPaymentSdkErrorToClientError(errors[i]));
                }
            }

            return paymentErrors;
        }

        /**
         * Converts the list of Payment SDK errors to client errors.
         * @param {Model.Entities.PaymentError[]} errors The payment errors.
         * @return {Model.Entities.Error[]} The client errors.
         */
        private static ConvertPaymentSdkErrorsToClientErrors(errors: Model.Entities.PaymentError[]): Model.Entities.Error[] {
            var paymentErrors: Model.Entities.Error[] = [];
            for (var i: number = 0; i < errors.length; i++) {
                var code: string = StringExtensions.isNullOrWhitespace(errors[i].Code) ? PaymentErrorTypeEnum[PaymentErrorHelper.GENERAL_EXCEPTION_ERROR_CODE]
                    : errors[i].Code;
                paymentErrors.push(new Model.Entities.Error(
                    PaymentErrorHelper.PAYMENT_EXCEPTION_NAMESPACE + code.toUpperCase(),
                    false,
                    errors[i].Message));
            }

            return paymentErrors;
        }

        /**
         * Checks for numeric payment SDK error codes in the given error and converts them to client payment exception error code.
         * @param {Model.Entities.Error} error The payment related error.
         * @return {Model.Entities.Error} The error with payment SDK error code converted to client payment exception error code.
         */
        private static MapPaymentSdkErrorToClientError(error: Model.Entities.Error): Model.Entities.Error {
            var result: string = PaymentErrorTypeEnum[error.ErrorCode];
            var paymentError: Model.Entities.Error = ObjectExtensions.isNullOrUndefined(result) ? error
                : new Commerce.Model.Entities.Error(
                    PaymentErrorHelper.PAYMENT_EXCEPTION_NAMESPACE + result.toUpperCase(),
                    false,
                    error.ExternalLocalizedErrorMessage);

            return paymentError;
        }
    }
}
