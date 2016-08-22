var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var ErrorHelper = (function () {
                            function ErrorHelper() {
                            }
                            ErrorHelper.getErrorMessages = function (errors) {
                                var messages = [];
                                var limitOfErrorLines = 5;
                                var previousMessageCodes = [];
                                if (Ecommerce.Utils.hasElements(errors)) {
                                    for (var i = 0; i < errors.length && i < limitOfErrorLines; i++) {
                                        var error = errors[i];
                                        if (errors.length > 1 && ErrorHelper.isAggregatedErrorResourceId(error.ErrorCode)) {
                                            continue;
                                        }
                                        if (previousMessageCodes.indexOf(error.ErrorCode) != -1) {
                                            continue;
                                        }
                                        previousMessageCodes.push(error.ErrorCode);
                                        messages.push(ErrorHelper.clientError(error));
                                    }
                                }
                                return messages;
                            };
                            ErrorHelper.clientError = function (proxyError) {
                                var localizedErrorMessage = Controls.Resources[Controls.ErrorTypeEnum.GENERICERRORMESSAGE];
                                var errorCode;
                                if (proxyError.ErrorCode) {
                                    errorCode = proxyError.ErrorCode.toUpperCase();
                                    var clientResourceId = Controls.ErrorTypeEnum[errorCode];
                                    if (!Ecommerce.Utils.isNullOrWhiteSpace(clientResourceId)) {
                                        localizedErrorMessage = Controls.Resources[clientResourceId];
                                    }
                                    else if (!Ecommerce.Utils.isNullOrWhiteSpace(proxyError.LocalizedErrorMessage)) {
                                        localizedErrorMessage = proxyError.LocalizedErrorMessage;
                                    }
                                }
                                return localizedErrorMessage;
                            };
                            ErrorHelper.isAggregatedErrorResourceId = function (errorResourceId) {
                                return ErrorHelper.AGGREGATED_ERROR_RESOUCEIDS.indexOf(errorResourceId) != -1;
                            };
                            ErrorHelper.AGGREGATED_ERROR_RESOUCEIDS = [
                                "Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError",
                                "Microsoft_Dynamics_Commerce_Runtime_AggregateCommunicationError",
                                "Microsoft_Dynamics_Commerce_Runtime_InvalidCartLinesAggregateError"];
                            return ErrorHelper;
                        })();
                        Controls.ErrorHelper = ErrorHelper;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var PaymentErrorTypeEnum = (function () {
                            function PaymentErrorTypeEnum() {
                            }
                            PaymentErrorTypeEnum[20001] = "InvalidOperation";
                            PaymentErrorTypeEnum[20002] = "ApplicationError";
                            PaymentErrorTypeEnum[20003] = "GenericCheckDetailsForError";
                            PaymentErrorTypeEnum[20004] = "DONotAuthorized";
                            PaymentErrorTypeEnum[20005] = "UserAborted";
                            PaymentErrorTypeEnum[20119] = "InvalidArgumentTenderAccountNumber";
                            PaymentErrorTypeEnum[21001] = "LocaleNotSupported";
                            PaymentErrorTypeEnum[21002] = "InvalidMerchantProperty";
                            PaymentErrorTypeEnum[22001] = "CommunicationError";
                            PaymentErrorTypeEnum[22010] = "InvalidArgumentCardTypeNotSupported";
                            PaymentErrorTypeEnum[22011] = "VoiceAuthorizationNotSupported";
                            PaymentErrorTypeEnum[22012] = "ReauthorizationNotSupported";
                            PaymentErrorTypeEnum[22013] = "MultipleCaptureNotSupported";
                            PaymentErrorTypeEnum[22014] = "BatchCaptureNotSupported";
                            PaymentErrorTypeEnum[22015] = "UnsupportedCurrency";
                            PaymentErrorTypeEnum[22016] = "UnsupportedCountry";
                            PaymentErrorTypeEnum[22017] = "CannotReauthorizePostCapture";
                            PaymentErrorTypeEnum[22018] = "CannotReauthorizePostVoid";
                            PaymentErrorTypeEnum[22019] = "ImmediateCaptureNotSupported";
                            PaymentErrorTypeEnum[22050] = "CardExpired";
                            PaymentErrorTypeEnum[22051] = "ReferToIssuer";
                            PaymentErrorTypeEnum[22052] = "NoReply";
                            PaymentErrorTypeEnum[22053] = "HoldCallOrPickupCard";
                            PaymentErrorTypeEnum[22054] = "InvalidAmount";
                            PaymentErrorTypeEnum[22055] = "AccountLengthError";
                            PaymentErrorTypeEnum[22056] = "AlreadyReversed";
                            PaymentErrorTypeEnum[22057] = "CannotVerifyPin";
                            PaymentErrorTypeEnum[22058] = "InvalidCardNumber";
                            PaymentErrorTypeEnum[22059] = "InvalidCVV2";
                            PaymentErrorTypeEnum[22060] = "CashBackNotAvailable";
                            PaymentErrorTypeEnum[22061] = "CardTypeVerificationError";
                            PaymentErrorTypeEnum[22062] = "Decline";
                            PaymentErrorTypeEnum[22063] = "EncryptionError";
                            PaymentErrorTypeEnum[22065] = "NoActionTaken";
                            PaymentErrorTypeEnum[22066] = "NoSuchIssuer";
                            PaymentErrorTypeEnum[22067] = "PinTriesExceeded";
                            PaymentErrorTypeEnum[22068] = "SecurityViolation";
                            PaymentErrorTypeEnum[22069] = "ServiceNotAllowed";
                            PaymentErrorTypeEnum[22070] = "StopRecurring";
                            PaymentErrorTypeEnum[22071] = "WrongPin";
                            PaymentErrorTypeEnum[22072] = "CVV2Mismatch";
                            PaymentErrorTypeEnum[22073] = "DuplicateTransaction";
                            PaymentErrorTypeEnum[22074] = "Reenter";
                            PaymentErrorTypeEnum[22075] = "AmountExceedLimit";
                            PaymentErrorTypeEnum[22076] = "AuthorizationExpired";
                            PaymentErrorTypeEnum[22077] = "AuthorizationAlreadyCompleted";
                            PaymentErrorTypeEnum[22078] = "AuthorizationIsVoided";
                            PaymentErrorTypeEnum[22090] = "ProcessorDuplicateBatch";
                            PaymentErrorTypeEnum[22100] = "AuthorizationFailure";
                            PaymentErrorTypeEnum[22102] = "InvalidMerchantConfiguration";
                            PaymentErrorTypeEnum[22103] = "InvalidExpirationDate";
                            PaymentErrorTypeEnum[22104] = "InvalidCardholderNameFirstNameRequired";
                            PaymentErrorTypeEnum[22105] = "InvalidCardholderNameLastNameRequired";
                            PaymentErrorTypeEnum[22106] = "FilterDecline";
                            PaymentErrorTypeEnum[22107] = "InvalidAddress";
                            PaymentErrorTypeEnum[22108] = "CVV2Required";
                            PaymentErrorTypeEnum[22109] = "CardTypeNotSupported";
                            PaymentErrorTypeEnum[22110] = "UniqueInvoiceNumberRequired";
                            PaymentErrorTypeEnum[22111] = "PossibleDuplicate";
                            PaymentErrorTypeEnum[22112] = "ProcessorRequiresLinkedRefund";
                            PaymentErrorTypeEnum[22113] = "CryptoBoxUnavailable";
                            PaymentErrorTypeEnum[22114] = "CVV2Declined";
                            PaymentErrorTypeEnum[22115] = "MerchantIdInvalid";
                            PaymentErrorTypeEnum[22116] = "TranNotAllowed";
                            PaymentErrorTypeEnum[22117] = "TerminalNotFound";
                            PaymentErrorTypeEnum[22118] = "InvalidEffectiveDate";
                            PaymentErrorTypeEnum[22119] = "InsufficientFunds";
                            PaymentErrorTypeEnum[22120] = "ReauthorizationMaxReached";
                            PaymentErrorTypeEnum[22121] = "ReauthorizationNotAllowed";
                            PaymentErrorTypeEnum[22122] = "DateOfBirthError";
                            PaymentErrorTypeEnum[22123] = "EnterLesserAmount";
                            PaymentErrorTypeEnum[22124] = "HostKeyError";
                            PaymentErrorTypeEnum[22125] = "InvalidCashBackAmount";
                            PaymentErrorTypeEnum[22126] = "InvalidTransaction";
                            PaymentErrorTypeEnum[22127] = "ImmediateCaptureRequired";
                            PaymentErrorTypeEnum[22128] = "ImmediateCaptureRequiredMAC";
                            PaymentErrorTypeEnum[22129] = "MACRequired";
                            PaymentErrorTypeEnum[22130] = "BankcardNotSet";
                            PaymentErrorTypeEnum[22131] = "InvalidRequest";
                            PaymentErrorTypeEnum[22132] = "InvalidTransactionFee";
                            PaymentErrorTypeEnum[22133] = "NoCheckingAccount";
                            PaymentErrorTypeEnum[22134] = "NoSavingsAccount";
                            PaymentErrorTypeEnum[22135] = "RestrictedCardTemporarilyDisallowedFromInterchange";
                            PaymentErrorTypeEnum[22136] = "MACSecurityFailure";
                            PaymentErrorTypeEnum[22137] = "ExceedsWithdrawalFrequencyLimit";
                            PaymentErrorTypeEnum[22138] = "InvalidCaptureDate";
                            PaymentErrorTypeEnum[22139] = "NoKeysAvailable";
                            PaymentErrorTypeEnum[22140] = "KMESyncError";
                            PaymentErrorTypeEnum[22141] = "KPESyncError";
                            PaymentErrorTypeEnum[22142] = "KMACSyncError";
                            PaymentErrorTypeEnum[22143] = "ResubmitExceedsLimit";
                            PaymentErrorTypeEnum[22144] = "SystemProblemError";
                            PaymentErrorTypeEnum[22145] = "AccountNumberNotFoundForRow";
                            PaymentErrorTypeEnum[22146] = "InvalidTokenInfoParameterForRow";
                            PaymentErrorTypeEnum[22147] = "ExceptionThrownForRow";
                            PaymentErrorTypeEnum[22148] = "TransactionAmountExceedsRemaining";
                            PaymentErrorTypeEnum[22149] = "GeneralException";
                            PaymentErrorTypeEnum[22150] = "InvalidCardTrackData";
                            PaymentErrorTypeEnum[22151] = "InvalidResultAccessCode";
                            return PaymentErrorTypeEnum;
                        })();
                        Controls.PaymentErrorTypeEnum = PaymentErrorTypeEnum;
                        var PaymentErrorHelper = (function () {
                            function PaymentErrorHelper() {
                            }
                            PaymentErrorHelper.ConvertToClientError = function (errors) {
                                var paymentErrors = [];
                                var paymentSdkErrors = [];
                                for (var i = 0; i < errors.length; i++) {
                                    var paymentException = errors[i].commerceException;
                                    if (paymentException != null && Ecommerce.Utils.hasElements(paymentException.PaymentSdkErrors)) {
                                        paymentSdkErrors = PaymentErrorHelper.ConvertPaymentSdkErrorsToClientErrors(paymentException.PaymentSdkErrors);
                                    }
                                    if (Ecommerce.Utils.hasElements(paymentSdkErrors)) {
                                        paymentErrors = paymentErrors.concat(paymentSdkErrors);
                                    }
                                    else {
                                        paymentErrors.push(PaymentErrorHelper.MapPaymentSdkErrorToClientError(errors[i]));
                                    }
                                }
                                return Controls.ErrorHelper.getErrorMessages(paymentErrors);
                            };
                            PaymentErrorHelper.ConvertPaymentSdkErrorsToClientErrors = function (errors) {
                                var paymentErrors = [];
                                for (var i = 0; i < errors.length; i++) {
                                    var code = Ecommerce.Utils.isNullOrWhiteSpace(errors[i].Code) ? PaymentErrorTypeEnum[PaymentErrorHelper.GeneralExceptionErrorCode]
                                        : errors[i].Code;
                                    paymentErrors.push(new CommerceProxy.ProxyError(PaymentErrorHelper.PaymentExceptionNamespace + code.toUpperCase(), errors[i].Message));
                                }
                                return paymentErrors;
                            };
                            PaymentErrorHelper.MapPaymentSdkErrorToClientError = function (error) {
                                var result = PaymentErrorTypeEnum[error.ErrorCode];
                                var paymentError = Ecommerce.Utils.isNullOrUndefined(result) ? error
                                    : new CommerceProxy.ProxyError(PaymentErrorHelper.PaymentExceptionNamespace + result.toUpperCase(), error.ErrorMessage);
                                return paymentError;
                            };
                            PaymentErrorHelper.PaymentExceptionNamespace = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_";
                            PaymentErrorHelper.GeneralExceptionErrorCode = "22149";
                            return PaymentErrorHelper;
                        })();
                        Controls.PaymentErrorHelper = PaymentErrorHelper;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
///<reference path="../../JQuery.d.ts" />
///<reference path="../../KnockoutJS.d.ts" />
///<reference path="../../Libraries.Proxies.Retail.TypeScript.d.ts" />
var CommerceProxy = Commerce.Proxy;
$(document).ready(function () {
    Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.ResourcesHandler.selectUICulture();
    $('.msax-Control').each(function (index, element) {
        var viewModelName = $(element.firstElementChild).attr("data-model");
        var pathNames = viewModelName.split('.');
        var viewModel = window[pathNames[0]];
        for (var i = 1; i < pathNames.length; i++) {
            viewModel = viewModel[pathNames[i]];
        }
        ko.applyBindings(new viewModel(element.firstElementChild), element);
    });
});
var msaxError = {
    Show: function (level, message, errorCodes) {
        console.error(message);
    }
};
var msaxValues;
(function (msaxValues) {
})(msaxValues || (msaxValues = {}));
var Microsoft;
(function (Microsoft) {
    var Maps;
    (function (Maps) {
    })(Maps = Microsoft.Maps || (Microsoft.Maps = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var AjaxProxy = (function () {
                            function AjaxProxy(relativeUrl) {
                                this.SubmitRequest = function (webMethod, data, successCallback, errorCallback) {
                                    var webServiceUrl = this.relativeUrl + webMethod;
                                    var requestDigestHeader = ($(document).find('#__REQUESTDIGEST'))[0];
                                    var retailRequestDigestHeader = ($(document).find('#__RETAILREQUESTDIGEST'))[0];
                                    var requestDigestHeaderValue;
                                    var retailRequestDigestHeaderValue;
                                    if (Ecommerce.Utils.isNullOrUndefined(requestDigestHeader) || Ecommerce.Utils.isNullOrUndefined(retailRequestDigestHeader)) {
                                        requestDigestHeaderValue = null;
                                        retailRequestDigestHeaderValue = null;
                                    }
                                    else {
                                        requestDigestHeaderValue = requestDigestHeader.value;
                                        retailRequestDigestHeaderValue = retailRequestDigestHeader.value;
                                    }
                                    $.ajax({
                                        url: webServiceUrl,
                                        data: JSON.stringify(data),
                                        type: "POST",
                                        contentType: "application/json; charset=utf-8",
                                        dataType: "json",
                                        success: function (data) {
                                            successCallback(data);
                                        },
                                        error: function (jqXHR) {
                                            if (jqXHR.status == 310) {
                                                var redirectUrl = jqXHR.getResponseHeader("Location");
                                                if (Ecommerce.Utils.isNullOrWhiteSpace(redirectUrl)) {
                                                    throw "The redirect url to sign in page should be provided for HTTP status code 310";
                                                }
                                                else {
                                                    window.location.replace(redirectUrl);
                                                }
                                            }
                                            errorCallback(jqXHR);
                                        },
                                        headers: {
                                            "X-RequestDigest": requestDigestHeaderValue,
                                            "X-RetailRequestDigest": retailRequestDigestHeaderValue
                                        }
                                    });
                                };
                                this.relativeUrl = relativeUrl;
                                $(document).ajaxError(this.ajaxErrorHandler);
                            }
                            AjaxProxy.prototype.ajaxErrorHandler = function (e, xhr, settings) {
                                var errorMessage = 'Url:\n' + settings.url +
                                    '\n\n' +
                                    'Response code:\n' + xhr.status +
                                    '\n\n' +
                                    'Status Text:\n' + xhr.statusText +
                                    '\n\n' +
                                    'Response Text: \n' + xhr.responseText;
                                msaxError.Show('error', 'The web service call was unsuccessful.  Details: ' + errorMessage);
                            };
                            return AjaxProxy;
                        })();
                        Controls.AjaxProxy = AjaxProxy;
                        var LoadingOverlay = (function () {
                            function LoadingOverlay() {
                            }
                            LoadingOverlay.CreateLoadingDialog = function (loadingDialog, loadingText, width, height) {
                                if (Ecommerce.Utils.isNullOrUndefined(LoadingOverlay.loadingDialog) && Ecommerce.Utils.isNullOrUndefined(LoadingOverlay.loadingText)) {
                                    LoadingOverlay.loadingDialog = loadingDialog;
                                    LoadingOverlay.loadingText = loadingText;
                                    LoadingOverlay.loadingDialog.dialog({
                                        modal: true,
                                        autoOpen: false,
                                        draggable: true,
                                        resizable: false,
                                        closeOnEscape: true,
                                        show: { effect: "fadeIn", duration: 500 },
                                        hide: { effect: "fadeOut", duration: 500 },
                                        open: function (event, ui) {
                                            setTimeout(function () {
                                                LoadingOverlay.loadingText.text(Controls.Resources.String_221);
                                            }, 60000);
                                        },
                                        width: width,
                                        height: height,
                                        dialogClass: 'msax-Control msax-LoadingOverlay msax-NoTitle'
                                    });
                                }
                            };
                            LoadingOverlay.ShowLoadingDialog = function (text) {
                                if (Ecommerce.Utils.isNullOrWhiteSpace(text)) {
                                    LoadingOverlay.loadingText.text(Controls.Resources.String_176);
                                }
                                else {
                                    LoadingOverlay.loadingText.text(text);
                                }
                                if (LoadingOverlay.pendingCallsCount == 0) {
                                    LoadingOverlay.loadingDialog.dialog('open');
                                    $('.ui-widget-overlay').addClass('msax-LoadingOverlay');
                                }
                                LoadingOverlay.pendingCallsCount = LoadingOverlay.pendingCallsCount + 1;
                            };
                            LoadingOverlay.CloseLoadingDialog = function () {
                                LoadingOverlay.pendingCallsCount = LoadingOverlay.pendingCallsCount - 1;
                                if (LoadingOverlay.pendingCallsCount == 0) {
                                    if (LoadingOverlay.loadingDialog.dialog('isOpen') == true) {
                                        LoadingOverlay.loadingDialog.dialog('close');
                                        $('.ui-widget-overlay').removeClass('msax-LoadingOverlay');
                                    }
                                }
                            };
                            LoadingOverlay.pendingCallsCount = 0;
                            LoadingOverlay.loadingDialog = null;
                            LoadingOverlay.loadingText = null;
                            return LoadingOverlay;
                        })();
                        Controls.LoadingOverlay = LoadingOverlay;
                        var Core = (function () {
                            function Core() {
                            }
                            Core.BuildImageMarkup50x50 = function (imageUrl, imageAltText) {
                                return this.BuildImageMarkup(imageUrl, imageAltText, 50, 50);
                            };
                            Core.BuildImageMarkup180x180 = function (imageUrl, imageAltText) {
                                return this.BuildImageMarkup(imageUrl, imageAltText, 180, 180);
                            };
                            Core.BuildImageMarkup = function (imageUrl, imageAltText, width, height) {
                                var imageClassName = "msax-Image";
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(imageUrl)) {
                                    var errorScript = Ecommerce.Utils.format('onerror=\"this.parentNode.innerHTML=Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.CartWebApi.GetNoImageMarkup();\"');
                                    return Ecommerce.Utils.format('<img src=\"{0}\" class=\"{1}\" alt=\"{2}\" width=\"{3}\" height=\"{4}\" {5} />', imageUrl, imageClassName, imageAltText, width, height, errorScript);
                                }
                                else {
                                    return this.GetNoImageMarkup();
                                }
                            };
                            Core.GetNoImageMarkup = function () {
                                return Ecommerce.Utils.format('<span class=\"msax-NoImageContainer\"></span>');
                            };
                            Core.GetDimensionValuesStringFromDimensions = function (dimensionValues) {
                                var color = null;
                                var size = null;
                                var style = null;
                                var configuration = null;
                                if (Ecommerce.Utils.hasElements(dimensionValues)) {
                                    for (var i = 0; i < dimensionValues.length; i++) {
                                        var dimension = dimensionValues[i];
                                        if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Color) {
                                            color = dimension.DimensionValue.Value;
                                        }
                                        else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Size) {
                                            size = dimension.DimensionValue.Value;
                                        }
                                        else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Style) {
                                            style = dimension.DimensionValue.Value;
                                        }
                                        else if (dimension.DimensionTypeValue == CommerceProxy.Entities.ProductDimensionType.Configuration) {
                                            configuration = dimension.DimensionValue.Value;
                                        }
                                    }
                                }
                                var dimensionValuesString = Core.GetDimensionValues(color, size, style, configuration);
                                return dimensionValuesString;
                            };
                            Core.GetDimensionValues = function (color, size, style, configuration) {
                                var hasColor = !Ecommerce.Utils.isNullOrWhiteSpace(color);
                                var hasSize = !Ecommerce.Utils.isNullOrWhiteSpace(size);
                                var hasStyle = !Ecommerce.Utils.isNullOrWhiteSpace(style);
                                var hasConfiguration = !Ecommerce.Utils.isNullOrWhiteSpace(configuration);
                                var dimensionValues = null;
                                if (hasColor || hasSize || hasStyle || hasConfiguration) {
                                    dimensionValues = ''
                                        + (!hasColor ? '' : color)
                                        + (hasColor && (hasSize || hasStyle || hasConfiguration) ? ', ' : '')
                                        + (!hasSize ? '' : size)
                                        + (hasSize && (hasStyle || hasConfiguration) ? ', ' : '')
                                        + (!hasStyle ? '' : style)
                                        + (hasStyle && (hasConfiguration) ? ', ' : '')
                                        + (!hasConfiguration ? '' : configuration)
                                        + '';
                                }
                                return dimensionValues;
                            };
                            Core.getDefaultQueryResultSettings = function () {
                                var queryResultSettings = new CommerceProxy.Entities.QueryResultSettingsClass();
                                queryResultSettings.Paging = new CommerceProxy.Entities.PagingInfoClass();
                                queryResultSettings.Paging.Skip = 0;
                                queryResultSettings.Paging.Top = 1000;
                                queryResultSettings.Sorting = {};
                                return queryResultSettings;
                            };
                            Core.getQueryResultSettings = function (skip, top) {
                                var queryResultSettings = new CommerceProxy.Entities.QueryResultSettingsClass();
                                queryResultSettings.Paging = new CommerceProxy.Entities.PagingInfoClass();
                                queryResultSettings.Paging.Skip = skip;
                                queryResultSettings.Paging.Top = top;
                                queryResultSettings.Sorting = {};
                                return queryResultSettings;
                            };
                            Core.getOrderSearchCriteria = function (channelReferenceId, salesId, receiptId, includeDetails) {
                                var orderSearchCriteria = new CommerceProxy.Entities.SalesOrderSearchCriteriaClass();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(channelReferenceId)) {
                                    orderSearchCriteria.ChannelReferenceId = channelReferenceId;
                                }
                                else if (!Ecommerce.Utils.isNullOrWhiteSpace(salesId)) {
                                    orderSearchCriteria.SalesId = salesId;
                                }
                                else {
                                    orderSearchCriteria.ReceiptId = receiptId;
                                }
                                orderSearchCriteria.IncludeDetails = includeDetails;
                                return orderSearchCriteria;
                            };
                            Core.getOrderNumber = function (salesOrder) {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(salesOrder.ChannelReferenceId)) {
                                    return salesOrder.ChannelReferenceId;
                                }
                                else if (!Ecommerce.Utils.isNullOrWhiteSpace(salesOrder.SalesId)) {
                                    return salesOrder.SalesId;
                                }
                                else {
                                    return salesOrder.ReceiptId;
                                }
                            };
                            Core.getProductSearchCriteria = function (productIds) {
                                var productSearchCriteria = new CommerceProxy.Entities.ProductSearchCriteriaClass();
                                productSearchCriteria.Ids = productIds;
                                productSearchCriteria.DataLevelValue = 4;
                                productSearchCriteria.SkipVariantExpansion = false;
                                return productSearchCriteria;
                            };
                            Core.getProductIdLookUpMap = function (products) {
                                var productIdLookupMap = [];
                                if (products != null) {
                                    var variants = [];
                                    for (var i = 0; i < products.length; i++) {
                                        var product = products[i];
                                        var tempVariants = this.getProductVariants(product);
                                        if (tempVariants.length > 0) {
                                            variants = variants.concat(tempVariants);
                                        }
                                        if (productIdLookupMap[product.RecordId] == null) {
                                            productIdLookupMap[product.RecordId] = product.RecordId;
                                        }
                                    }
                                    if (variants.length > 0) {
                                        for (var j = 0; j < variants.length; j++) {
                                            var variant = variants[j];
                                            if (productIdLookupMap[variant.DistinctProductVariantId] == null) {
                                                productIdLookupMap[variant.DistinctProductVariantId] = variant.MasterProductId;
                                            }
                                        }
                                    }
                                }
                                return productIdLookupMap;
                            };
                            Core.getProductVariants = function (product) {
                                var variants = [];
                                if (product.IsMasterProduct && product.CompositionInformation != null && product.CompositionInformation.VariantInformation != null) {
                                    variants = product.CompositionInformation.VariantInformation.Variants;
                                }
                                return variants;
                            };
                            Core.getExtensionPropertyValue = function (commerceProperties, propertyName) {
                                var commerceProperty = null;
                                var value = null;
                                for (var i = 0; i < commerceProperties.length; i++) {
                                    if (commerceProperties[i].Key == propertyName) {
                                        commerceProperty = commerceProperties[i];
                                        break;
                                    }
                                }
                                if (commerceProperty != null) {
                                    value = commerceProperty.Value.StringValue;
                                }
                                return value;
                            };
                            Core.populateProductDetailsForCartLine = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                Core.populateProductDetailsForLine(line, simpleProductsByIdMap, currencyStringTemplate);
                            };
                            Core.populateProductDetailsForSalesLine = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                Core.populateProductDetailsForLine(line, simpleProductsByIdMap, currencyStringTemplate);
                            };
                            Core.populateProductDetailsForLine = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                var simpleProduct = simpleProductsByIdMap[line.ProductId];
                                if (Ecommerce.Utils.isNullOrUndefined(simpleProduct)) {
                                    line[Controls.Constants.ProductNameProperty] = Ecommerce.Utils.format("Product info [{0}] unavailable", line.ProductId);
                                    line[Controls.Constants.ProductDescriptionProperty] = "This product is not available in the current store.";
                                    line[Controls.Constants.ProductDimensionProperty] = '';
                                    line[Controls.Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50('', '');
                                    ;
                                    line[Controls.Constants.ImageMarkup180pxProperty] = Core.BuildImageMarkup180x180('', '');
                                    ;
                                    line[Controls.Constants.KitComponentsProperty] = '';
                                    line[Controls.Constants.ProductTypeProperty] = '';
                                    line[Controls.Constants.ProductUrlProperty] = '#';
                                    line[Controls.Constants.KitComponentCountProperty] = '';
                                    line[Controls.Constants.KitComponentPriceProperty] = '';
                                    return;
                                }
                                line[Controls.Constants.ProductNameProperty] = simpleProduct.Name;
                                line[Controls.Constants.ProductDescriptionProperty] = simpleProduct.Description;
                                line[Controls.Constants.ProductDimensionProperty] = Core.GetDimensionValuesStringFromDimensions(simpleProduct.Dimensions);
                                line[Controls.Constants.ProductUrlProperty] = Ecommerce.Utils.format(msaxValues.msax_ProductDetailsUrlTemplate, line.ProductId);
                                var imageUrl = Controls.Constants.ProductUrlString + Core.getExtensionPropertyValue(simpleProduct.ExtensionProperties, "PrimaryImageUri");
                                imageUrl = (imageUrl != null) ? imageUrl : '';
                                var imageAltText = Core.getExtensionPropertyValue(line.ExtensionProperties, "PrimaryImageAltText");
                                imageAltText = (imageAltText != null) ? imageAltText : '';
                                line[Controls.Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50(imageUrl, imageAltText);
                                line[Controls.Constants.ImageMarkup180pxProperty] = Core.BuildImageMarkup180x180(imageUrl, imageAltText);
                                line[Controls.Constants.ProductTypeProperty] = '';
                                line[Controls.Constants.KitComponentsProperty] = [];
                                line[Controls.Constants.KitComponentCountProperty] = 0;
                            };
                            Core.populateKitItemDetailsForCartLine = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                Core.populateKitItemDetails(line, simpleProductsByIdMap, currencyStringTemplate);
                            };
                            Core.populateKitItemDetailsForSalesLine = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                Core.populateKitItemDetails(line, simpleProductsByIdMap, currencyStringTemplate);
                            };
                            Core.populateKitItemDetails = function (line, simpleProductsByIdMap, currencyStringTemplate) {
                                var simpleProduct = simpleProductsByIdMap[line.ProductId];
                                if (Ecommerce.Utils.isNullOrUndefined(simpleProduct) || simpleProduct.ProductTypeValue != CommerceProxy.Entities.ProductType.KitVariant || !Ecommerce.Utils.hasElements(simpleProduct.Components)) {
                                    return;
                                }
                                var kitComponents = simpleProduct.Components;
                                for (var j = 0; j < simpleProduct.Components.length; j++) {
                                    var kitComponent = kitComponents[j];
                                    kitComponent[Controls.Constants.ProductDimensionProperty] = Core.GetDimensionValuesStringFromDimensions(kitComponent.Dimensions);
                                    kitComponent[Controls.Constants.ProductUrlProperty] = Ecommerce.Utils.format(msaxValues.msax_ProductDetailsUrlTemplate, kitComponent.ProductId);
                                    kitComponent[Controls.Constants.ProductNameProperty] = kitComponent.Name;
                                    var imageUrl = Controls.Constants.ProductUrlString + Core.getExtensionPropertyValue(kitComponent.ExtensionProperties, "PrimaryImageUri");
                                    imageUrl = (imageUrl != null) ? imageUrl : '';
                                    var imageAltText = Core.getExtensionPropertyValue(kitComponent.ExtensionProperties, "PrimaryImageAltText");
                                    imageAltText = (imageAltText != null) ? imageAltText : '';
                                    kitComponent[Controls.Constants.ImageMarkup50pxProperty] = Core.BuildImageMarkup50x50(imageUrl, imageAltText);
                                    kitComponent[Controls.Constants.KitComponentPriceProperty] = Core.getKitComponentPriceCurrencyString(kitComponent.AdditionalChargeForComponent, currencyStringTemplate);
                                }
                                line[Controls.Constants.ProductTypeProperty] = CommerceProxy.Entities.ProductType.KitVariant;
                                line[Controls.Constants.KitComponentCountProperty] = Ecommerce.Utils.format(Controls.Resources.String_88, kitComponents.length);
                                line[Controls.Constants.KitComponentsProperty] = kitComponents;
                            };
                            Core.getKitComponentPriceCurrencyString = function (amount, currencyStringTemplate) {
                                var formattedKitComponentPrice = Controls.Resources.String_208;
                                if (amount != 0) {
                                    if (Ecommerce.Utils.isNullOrUndefined(currencyStringTemplate)) {
                                        formattedKitComponentPrice = amount.toString();
                                    }
                                    else {
                                        formattedKitComponentPrice = Ecommerce.Utils.format(currencyStringTemplate, Ecommerce.Utils.formatNumber(amount));
                                    }
                                }
                                return formattedKitComponentPrice;
                            };
                            Core.getSalesStatusString = function (statusValue) {
                                var salesStatus = "";
                                if (statusValue == CommerceProxy.Entities.SalesStatus.Unknown) {
                                    salesStatus = Controls.Resources.String_240;
                                }
                                else {
                                    salesStatus = CommerceProxy.Entities.SalesStatus[statusValue];
                                }
                                return salesStatus;
                            };
                            Core.LogEvent = function (eventName, errors, alternateMessage) {
                                var logErrorMessage = (Ecommerce.Utils.hasElements(errors) && !Ecommerce.Utils.isNullOrWhiteSpace(errors[0].LocalizedErrorMessage)) ? errors[0].LocalizedErrorMessage : alternateMessage;
                                CommerceProxy.RetailLogger.LogEvent(eventName, logErrorMessage);
                            };
                            return Core;
                        })();
                        Controls.Core = Core;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
///<reference path="../../Libraries.Proxies.Retail.TypeScript.d.ts" />
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/EcommerceTypes.ts" />
/// <reference path="../../Libraries.Proxies.Retail.TypeScript.d.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var Constants = (function () {
                            function Constants() {
                            }
                            Constants.ProductUrlString = '/';
                            Constants.NoOpUrlString = '#';
                            Constants.OfferNamesProperty = 'OfferNames';
                            Constants.ProductNameProperty = 'ProductName';
                            Constants.ProductDescriptionProperty = 'ProductDescription';
                            Constants.ProductDimensionProperty = 'ProductDimension';
                            Constants.ProductDimensionArrayProperty = 'ProductDimensionValues';
                            Constants.ProductUrlProperty = 'ProductUrl';
                            Constants.ImageMarkup50pxProperty = 'ImageMarkup50px';
                            Constants.ImageMarkup180pxProperty = 'ImageMarkup180px';
                            Constants.ProductTypeProperty = 'ProductType';
                            Constants.KitComponentsProperty = 'KitComponents';
                            Constants.KitComponentCountProperty = 'KitComponentCount';
                            Constants.KitComponentPriceProperty = 'KitComponentPrice';
                            return Constants;
                        })();
                        Controls.Constants = Constants;
                        var CartWebApi = (function () {
                            function CartWebApi() {
                            }
                            CartWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_CartWebApiUrl + '/');
                            };
                            CartWebApi.GetCart = function (cartType, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetCart", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.RemoveFromCart = function (cartType, lineIds, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession,
                                    "lineIds": lineIds
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("RemoveItems", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.UpdateQuantity = function (cartType, cartLines, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession,
                                    "cartLines": cartLines
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("UpdateItems", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetPromotions = function (cartType, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetPromotions", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.AddOrRemovePromotion = function (cartType, promotionCode, isAdd, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession,
                                    "promotionCode": promotionCode,
                                    "isAdd": isAdd
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("AddOrRemovePromotionCode", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.CommenceCheckout = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {};
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("CommenceCheckout", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetCartUpdatedWithPromotions = function (cart, cartPromotions) {
                                if (Ecommerce.Utils.hasElements(cartPromotions.HeaderPromotions)) {
                                    cart.PromotionLines = cartPromotions.HeaderPromotions;
                                }
                                if (Ecommerce.Utils.hasElements(cartPromotions.CartLinePromotions)) {
                                    for (var i = 0; i < cartPromotions.CartLinePromotions.length; i++) {
                                        var currentLinePromotion = cartPromotions.CartLinePromotions[i];
                                        for (var j = 0; j < cart.CartLines.length; j++) {
                                            var currentCartLine = cart.CartLines[j];
                                            if (currentLinePromotion.LineId == currentCartLine.LineId) {
                                                currentCartLine.PromotionLines = currentLinePromotion.Promotions;
                                            }
                                        }
                                    }
                                }
                                return cart;
                            };
                            CartWebApi.OnUpdateShoppingCart = function (callerContext, handler) {
                                $(document).on('UpdateShoppingCart', $.proxy(handler, callerContext));
                            };
                            CartWebApi.OnUpdateCheckoutCart = function (callerContext, handler) {
                                $(document).on('UpdateCheckoutCart', $.proxy(handler, callerContext));
                            };
                            CartWebApi.GetNoImageMarkup = function () {
                                return Ecommerce.Utils.format('<span class=\"msax-NoImageContainer\"></span>');
                            };
                            CartWebApi.BuildImageMarkup = function (imageUrl, imageAltText, width, height) {
                                var imageClassName = "msax-Image";
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(imageUrl)) {
                                    var errorScript = Ecommerce.Utils.format('onerror=\"this.parentNode.innerHTML=Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.CartWebApi.GetNoImageMarkup();\"');
                                    return Ecommerce.Utils.format('<img src=\"{0}\" class=\"{1}\" alt=\"{2}\" width=\"{3}\" height=\"{4}\" {5} />', imageUrl, imageClassName, imageAltText, width, height, errorScript);
                                }
                                else {
                                    return CartWebApi.GetNoImageMarkup();
                                }
                            };
                            CartWebApi.UpdateShoppingCartOnResponse = function (cart, cartType, fetchPromotions) {
                                var _this = this;
                                var asyncResult = new CommerceProxy.AsyncResult();
                                if (!Ecommerce.Utils.isNullOrUndefined(cart) && Ecommerce.Utils.hasElements(cart.CartLines)) {
                                    this.initializeDynamicCartLineProperties(cart);
                                    var currencyStringTemplate = Controls.Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                                    var productIds = [];
                                    for (var j = 0; j < cart.CartLines.length; j++) {
                                        productIds.push(cart.CartLines[j].ProductId);
                                    }
                                    CommerceProxy.RetailLogger.getSimpleProductsByIdStarted();
                                    Controls.ProductWebApi.GetSimpleProducts(productIds, this)
                                        .done(function (simpleProducts) {
                                        CommerceProxy.RetailLogger.getSimpleProductsByIdFinished();
                                        var simpleProductsByIdMap = [];
                                        for (var i = 0; i < simpleProducts.length; i++) {
                                            var key = simpleProducts[i].RecordId;
                                            simpleProductsByIdMap[key] = simpleProducts[i];
                                        }
                                        for (var j = 0; j < cart.CartLines.length; j++) {
                                            var cartLine = cart.CartLines[j];
                                            Controls.Core.populateProductDetailsForCartLine(cartLine, simpleProductsByIdMap, currencyStringTemplate);
                                            Controls.Core.populateKitItemDetailsForCartLine(cartLine, simpleProductsByIdMap, currencyStringTemplate);
                                        }
                                        for (var i = 0; i < cart.CartLines.length; i++) {
                                            var cartLine = cart.CartLines[i];
                                            cartLine[Constants.OfferNamesProperty] = "";
                                            if (Ecommerce.Utils.hasElements(cartLine.DiscountLines)) {
                                                for (var j = 0; j < cartLine.DiscountLines.length; j++) {
                                                    cartLine[Constants.OfferNamesProperty] = cartLine[Constants.OfferNamesProperty] + cartLine.DiscountLines[j].OfferName + " ";
                                                }
                                            }
                                        }
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            var resetOrderTotal = false;
                                            if (Ecommerce.Utils.isNullOrUndefined(cart.DeliveryMode) &&
                                                !Ecommerce.Utils.isNullOrUndefined(cart.CartLines)) {
                                                var cartLines = cart.CartLines;
                                                for (var i = 0; i < cartLines.length; i++) {
                                                    if (Ecommerce.Utils.isNullOrUndefined(cartLines[i].DeliveryMode)) {
                                                        resetOrderTotal = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (resetOrderTotal) {
                                                cart.TotalAmount = cart.SubtotalAmountWithoutTax;
                                            }
                                        }
                                        if (fetchPromotions) {
                                            CommerceProxy.RetailLogger.shoppingCartGetPromotionsStarted;
                                            _this.GetPromotions(cartType, _this)
                                                .done(function (cartPromotions) {
                                                CommerceProxy.RetailLogger.shoppingCartGetPromotionsFinished();
                                                if (!Ecommerce.Utils.isNullOrUndefined(cartPromotions)) {
                                                    cart = CartWebApi.GetCartUpdatedWithPromotions(cart, cartPromotions);
                                                    asyncResult.resolve(cart);
                                                }
                                            })
                                                .fail(function (errors) {
                                                CommerceProxy.RetailLogger.shoppingCartGetPromotionsError(errors[0].LocalizedErrorMessage);
                                                asyncResult.resolve(cart);
                                            });
                                        }
                                        else {
                                            asyncResult.resolve(cart);
                                        }
                                    })
                                        .fail(function (errors) {
                                        CommerceProxy.RetailLogger.getSimpleProductsByIdError(errors[0].LocalizedErrorMessage);
                                        asyncResult.resolve(cart);
                                    });
                                }
                                else {
                                    asyncResult.resolve(cart);
                                }
                                return asyncResult;
                            };
                            CartWebApi.TriggerCartUpdateEvent = function (cartType, updatedCart) {
                                if (cartType == CommerceProxy.Entities.CartType.Checkout) {
                                    $(document).trigger('UpdateCheckoutCart', [updatedCart]);
                                }
                                else {
                                    $(document).trigger('UpdateShoppingCart', [updatedCart]);
                                }
                            };
                            CartWebApi.prepareCartLinesForServer = function (cartLines) {
                                if (Ecommerce.Utils.isNullOrUndefined(cartLines)) {
                                    return null;
                                }
                                for (var i = 0; i < cartLines.length; i++) {
                                    var cartLine = cartLines[i];
                                    if (!Ecommerce.Utils.isNullOrUndefined(cartLine)) {
                                        delete cartLine[Constants.OfferNamesProperty];
                                        delete cartLine[Constants.ProductNameProperty];
                                        delete cartLine[Constants.ProductDescriptionProperty];
                                        delete cartLine[Constants.ProductDimensionProperty];
                                        delete cartLine[Constants.ImageMarkup50pxProperty];
                                        delete cartLine[Constants.ImageMarkup180pxProperty];
                                        delete cartLine[Constants.KitComponentsProperty];
                                        delete cartLine[Constants.ProductTypeProperty];
                                        delete cartLine[Constants.ProductUrlProperty];
                                        delete cartLine[Constants.KitComponentCountProperty];
                                        delete cartLine[Constants.KitComponentPriceProperty];
                                    }
                                }
                                return cartLines;
                            };
                            CartWebApi.initializeDynamicCartLineProperties = function (cart) {
                                for (var i = 0; i < cart.CartLines.length; i++) {
                                    var cartLine = cart.CartLines[i];
                                    cartLine[Constants.OfferNamesProperty] = '';
                                    cartLine[Constants.ProductNameProperty] = '';
                                    cartLine[Constants.ProductDescriptionProperty] = '';
                                    cartLine[Constants.ProductDimensionProperty] = '';
                                    cartLine[Constants.ImageMarkup50pxProperty] = '';
                                    cartLine[Constants.ImageMarkup180pxProperty] = '';
                                    cartLine[Constants.KitComponentsProperty] = '';
                                    cartLine[Constants.ProductTypeProperty] = '';
                                    cartLine[Constants.ProductUrlProperty] = '';
                                    cartLine[Constants.KitComponentCountProperty] = '';
                                    cartLine[Constants.KitComponentPriceProperty] = '';
                                }
                            };
                            CartWebApi.UpdateLoyaltyCardId = function (cartType, loyaltyCardId, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var isCheckoutSession = (cartType == CommerceProxy.Entities.CartType.Checkout);
                                var data = {
                                    "isCheckoutSession": isCheckoutSession,
                                    "loyaltyCardId": loyaltyCardId
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("UpdateLoyaltyCardId", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.SubmitOrder = function (cartTenderLines, emailAddress, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "cartTenderLines": cartTenderLines,
                                    "emailAddress": emailAddress
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("CreateOrder", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetDeliveryPreferences = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {};
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetDeliveryPreferences", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetOrderDeliveryOptionsForShipping = function (shipToAddress, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "shipToAddress": shipToAddress,
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetOrderDeliveryOptionsForShipping", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetLineDeliveryOptionsForShipping = function (lineShippingAddresses, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "lineShippingAddresses": lineShippingAddresses,
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetLineDeliveryOptionsForShipping", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.UpdateDeliverySpecification = function (headerLevelDeliveryOption, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "deliverySpecification": headerLevelDeliveryOption
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("UpdateDeliverySpecification", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.UpdateLineDeliverySpecifications = function (lineLevelDeliveryOptions, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "lineDeliverySpecifications": lineLevelDeliveryOptions
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("UpdateLineDeliverySpecifications", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.GetCardPaymentAcceptPoint = function (cardPaymentAcceptSettings, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "cardPaymentAcceptSettings": cardPaymentAcceptSettings
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetCardPaymentAcceptPoint", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CartWebApi.CleanUpAfterSuccessfulOrder = function (linesIdsToRemoveFromShoppingCart, callerContext) {
                                return CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Shopping, linesIdsToRemoveFromShoppingCart, callerContext);
                            };
                            return CartWebApi;
                        })();
                        Controls.CartWebApi = CartWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        Controls.ResourceStrings = {};
                        Controls.Resources = {};
                        var ResourcesHandler = (function () {
                            function ResourcesHandler() {
                            }
                            ResourcesHandler.selectUICulture = function () {
                                var uiCultureFromCookie = Ecommerce.Utils.getCurrentUiCulture();
                                if (Controls.ResourceStrings[uiCultureFromCookie]) {
                                    Controls.Resources = Controls.ResourceStrings[uiCultureFromCookie];
                                }
                                else {
                                    Controls.Resources = Controls.ResourceStrings["en-us"];
                                }
                            };
                            return ResourcesHandler;
                        })();
                        Controls.ResourcesHandler = ResourcesHandler;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Utils = (function () {
                    function Utils() {
                    }
                    Utils.isNullOrUndefined = function (o) {
                        return (o === undefined || o === null);
                    };
                    Utils.isNullOrEmpty = function (o) {
                        return (Utils.isNullOrUndefined(o) || o === '');
                    };
                    Utils.isNullOrWhiteSpace = function (o) {
                        return (Utils.isNullOrEmpty(o) || (typeof o === 'string' && o.replace(/\s/g, '').length < 1));
                    };
                    Utils.hasElements = function (o) {
                        return !Utils.isNullOrUndefined(o) && o.length > 0;
                    };
                    Utils.getValueOrDefault = function (o, defaultValue) {
                        return Utils.isNullOrWhiteSpace(o) ? defaultValue : o;
                    };
                    Utils.hasErrors = function (o) {
                        return (!Utils.isNullOrUndefined(o) && !this.hasElements(o.Errors));
                    };
                    Utils.format = function (object) {
                        var params = [];
                        for (var _i = 1; _i < arguments.length; _i++) {
                            params[_i - 1] = arguments[_i];
                        }
                        if (Utils.isNullOrWhiteSpace(object)) {
                            return object;
                        }
                        if (params == null) {
                            throw Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources.String_70;
                        }
                        for (var index = 0; index < params.length; index++) {
                            if (params[index] == null) {
                                throw Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources.String_70;
                            }
                            var regexp = new RegExp('\\{' + index + '\\}', 'gi');
                            object = object.replace(regexp, params[index]);
                        }
                        return object;
                    };
                    Utils.appendString = function (originalStr, appendStr, appendChar) {
                        return appendStr ? (originalStr ? originalStr + appendChar + appendStr : appendStr) : originalStr;
                    };
                    Utils.parseNumberFromLocaleString = function (localizedNumberString) {
                        var currDecimalOperator = this.getDecimalOperatorForUiCulture();
                        var numberTokens = localizedNumberString.split(currDecimalOperator);
                        if (numberTokens.length > 2) {
                            throw Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources.String_204;
                        }
                        var regexp = new RegExp("[^0-9]", "gi");
                        var integerDigits = numberTokens[0].replace(regexp, "");
                        var fractionalDigits = "";
                        if (numberTokens.length == 2) {
                            fractionalDigits = numberTokens[1].replace(regexp, "");
                        }
                        var numberString = integerDigits + '.' + fractionalDigits;
                        var parsedNumber = parsedNumber = Number(numberString);
                        if (isNaN(parsedNumber)) {
                            parsedNumber = 0;
                        }
                        return parsedNumber;
                    };
                    Utils.getDecimalOperatorForUiCulture = function () {
                        // Intl is currently not supported in all browsers. Hence this method is currently being hardcoded to work against "en-us" only.
                        //var uiCulture = this.getCurrentUiCulture();
                        //var nf: any;
                        //nf = new Intl.NumberFormat(uiCulture);
                        //var localizedNumString: string;
                        //localizedNumString = nf.format(1.1); //Eg: 1.1 will become 1,1 in fr-ca. 1.1 wil become 1.1 in en-us.
                        return '.';
                    };
                    Utils.getQueryStringValue = function (key) {
                        var url = window.location.href;
                        var keysValues = url.split(/[\?&]+/);
                        for (var i = 0; i < keysValues.length; i++) {
                            var keyValue = keysValues[i].split("=");
                            if (keyValue[0] == key) {
                                return keyValue[1];
                            }
                        }
                    };
                    Utils.formatNumber = function (numberValue) {
                        // Intl is currently not supported in all browsers. Hence, this method is currently being hardcoded to work against "en-us" only.
                        //var uiCulture = this.getCurrentUiCulture();
                        var formattedNumber = numberValue.toFixed(2);
                        return formattedNumber;
                    };
                    Utils.getCurrentUiCulture = function () {
                        if (!Utils.isNullOrWhiteSpace(this.currentUiCulture)) {
                            return this.currentUiCulture;
                        }
                        var uiCulture = Utils.getCookieValue(this.uiCultureCookieName);
                        if (Utils.isNullOrWhiteSpace(uiCulture)) {
                            uiCulture = this.defaultUiCulture;
                        }
                        return uiCulture;
                    };
                    Utils.getCookieValue = function (cookieName) {
                        var nameWithEqSign = cookieName + "=";
                        var allCookies = document.cookie.split(';');
                        for (var i = 0; i < allCookies.length; i++) {
                            var singleCookie = allCookies[i];
                            while (singleCookie.charAt(0) == ' ') {
                                singleCookie = singleCookie.substring(1, singleCookie.length);
                            }
                            if (singleCookie.indexOf(nameWithEqSign) == 0) {
                                return singleCookie.substring(nameWithEqSign.length, singleCookie.length);
                            }
                        }
                        return null;
                    };
                    Utils.setCookieValue = function (cookieName, cookieValue) {
                        if (!Utils.isNullOrWhiteSpace(cookieName)) {
                            document.cookie = cookieName + "=" + cookieValue;
                        }
                    };
                    Utils.clone = function (origObject) {
                        return Utils.safeClone(origObject, []);
                    };
                    Utils.safeClone = function (origObject, cloneMap) {
                        if (Utils.isNullOrUndefined(origObject)) {
                            return origObject;
                        }
                        var newObj;
                        if (origObject instanceof Array) {
                            if (!cloneMap.some(function (val) {
                                if (val.id === origObject) {
                                    newObj = val.value;
                                    return true;
                                }
                                return false;
                            })) {
                                newObj = [];
                                cloneMap.push({ id: origObject, value: newObj });
                                for (var i = 0; i < origObject.length; i++) {
                                    if (typeof origObject[i] == "object") {
                                        newObj.push(Utils.safeClone(origObject[i], cloneMap));
                                    }
                                    else {
                                        newObj.push(origObject[i]);
                                    }
                                }
                            }
                        }
                        else if (origObject instanceof Date) {
                            newObj = new Date(origObject.valueOf());
                        }
                        else if (origObject instanceof Object) {
                            if (!cloneMap.some(function (val) {
                                if (val.id === origObject) {
                                    newObj = val.value;
                                    return true;
                                }
                                return false;
                            })) {
                                newObj = $.extend(false, {}, origObject);
                                cloneMap.push({ id: origObject, value: newObj });
                                for (var property in newObj) {
                                    if (newObj.hasOwnProperty(property)) {
                                        if (typeof newObj[property] == "object") {
                                            if (property === "__metadata") {
                                                newObj[property] = $.extend(false, {}, origObject[property]);
                                            }
                                            else {
                                                newObj[property] = Utils.safeClone(origObject[property], cloneMap);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            newObj = origObject;
                        }
                        return newObj;
                    };
                    Utils.roundToNDigits = function (value, numOfDigits) {
                        if (this.isNullOrUndefined(numOfDigits) || (numOfDigits <= 0)) {
                            numOfDigits = 0;
                        }
                        else {
                            numOfDigits = Math.round(numOfDigits);
                        }
                        if (numOfDigits === 0) {
                            return Math.round(value);
                        }
                        return Math.round(value * Math.pow(10, numOfDigits)) / Math.pow(10, numOfDigits);
                    };
                    Utils.currentUiCulture = "";
                    Utils.defaultUiCulture = "en-US";
                    Utils.uiCultureCookieName = "cuid";
                    return Utils;
                })();
                Ecommerce.Utils = Utils;
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="Utils.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var AsyncResult = (function () {
                            function AsyncResult() {
                                this._callerContext = this;
                                this._succeded = false;
                                this._failed = false;
                                this._successCallbacks = [];
                                this._errorCallbacks = [];
                            }
                            AsyncResult.prototype.resolve = function (result) {
                                this._succeded = true;
                                this._result = result;
                                FunctionQueueHelper.callFunctions(this._successCallbacks, this._callerContext, this._result);
                            };
                            AsyncResult.prototype.reject = function (errors) {
                                this._failed = true;
                                this._errors = errors;
                                FunctionQueueHelper.callFunctions(this._errorCallbacks, this._callerContext, this._errors);
                            };
                            AsyncResult.prototype.done = function (callback) {
                                if (this._succeded && callback) {
                                    callback.call(this._callerContext, this._result);
                                }
                                else {
                                    FunctionQueueHelper.queueFunction(this._successCallbacks, callback);
                                }
                                return this;
                            };
                            AsyncResult.prototype.fail = function (callback) {
                                if (this._failed && callback) {
                                    callback.call(this._callerContext, this._errors);
                                }
                                else {
                                    FunctionQueueHelper.queueFunction(this._errorCallbacks, callback);
                                }
                                return this;
                            };
                            return AsyncResult;
                        })();
                        Controls.AsyncResult = AsyncResult;
                        var FunctionQueueHelper = (function () {
                            function FunctionQueueHelper() {
                            }
                            FunctionQueueHelper.callFunctions = function (functionQueue, callerContext, data) {
                                if (!Microsoft.Dynamics.Retail.Ecommerce.Utils.hasElements(functionQueue)) {
                                    return;
                                }
                                for (var i = 0; i < functionQueue.length; i++) {
                                    functionQueue[i].call(callerContext, data);
                                }
                                functionQueue = [];
                            };
                            FunctionQueueHelper.queueFunction = function (functionQueue, callback) {
                                if (!Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrUndefined(callback)) {
                                    functionQueue.push(callback);
                                }
                            };
                            return FunctionQueueHelper;
                        })();
                        Controls.FunctionQueueHelper = FunctionQueueHelper;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/AsyncResult.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var CustomerWebApi = (function () {
                            function CustomerWebApi() {
                            }
                            CustomerWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_CustomerWebApiUrl + '/');
                            };
                            CustomerWebApi.GetCustomer = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult();
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetCustomer", null, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CustomerWebApi.IsAuthenticatedSession = function () {
                                var asyncResult = new CommerceProxy.AsyncResult();
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("IsAuthenticatedSession", null, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CustomerWebApi.GetOrderHistory = function (orderCountToBeSkipped, orderCountToBeRetrieved, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "queryResultSettings": Controls.Core.getQueryResultSettings(orderCountToBeSkipped, orderCountToBeRetrieved)
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetOrderHistory", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            CustomerWebApi.GetLoyaltyCards = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetLoyaltyCards", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            return CustomerWebApi;
                        })();
                        Controls.CustomerWebApi = CustomerWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/EcommerceTypes.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var OrgUnitWebApi = (function () {
                            function OrgUnitWebApi() {
                            }
                            OrgUnitWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_OrgUnitWebApiUrl + '/');
                            };
                            OrgUnitWebApi.GetChannelConfiguration = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetChannelConfiguration", null, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            OrgUnitWebApi.GetNearbyStoresWithAvailability = function (latitude, longitude, distance, itemUnits, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "latitude": latitude,
                                    "longitude": longitude,
                                    "searchRadius": distance,
                                    "itemUnits": itemUnits,
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetNearbyStoresWithAvailability", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            OrgUnitWebApi.GetNearbyStores = function (latitude, longitude, distance, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "latitude": latitude,
                                    "longitude": longitude,
                                    "distance": distance,
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetNearbyStores", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            OrgUnitWebApi.GetDeliveryOptionsInfo = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetDeliveryOptionsInfo", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            OrgUnitWebApi.GetTenderTypes = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                var data = {
                                    'queryResultSettings': Controls.Core.getDefaultQueryResultSettings()
                                };
                                this.proxy.SubmitRequest("GetChannelTenderTypes", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            OrgUnitWebApi.GetCardTypes = function (callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "queryResultSettings": Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetCardTypes", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            return OrgUnitWebApi;
                        })();
                        Controls.OrgUnitWebApi = OrgUnitWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/AsyncResult.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var ProductWebApi = (function () {
                            function ProductWebApi() {
                            }
                            ProductWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_ProductWebApiUrl + '/');
                            };
                            ProductWebApi.GetSimpleProducts = function (productIds, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    'productIds': productIds,
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetSimpleProducts", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return (asyncResult);
                            };
                            return ProductWebApi;
                        })();
                        Controls.ProductWebApi = ProductWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/AsyncResult.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var StoreOperationsWebApi = (function () {
                            function StoreOperationsWebApi() {
                            }
                            StoreOperationsWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_RetailOperationsWebApiUrl + '/');
                            };
                            StoreOperationsWebApi.GetCountryRegionInfo = function (languageId, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                var data = {
                                    'languageId': languageId,
                                    'queryResultSettings': Controls.Core.getDefaultQueryResultSettings()
                                };
                                this.proxy.SubmitRequest("GetCountryRegionInfo", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            StoreOperationsWebApi.GetStateProvinceInfo = function (countryCode, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    'countryCode': countryCode,
                                    'queryResultSettings': Controls.Core.getDefaultQueryResultSettings()
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetStateProvinceInfo", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            StoreOperationsWebApi.GetGiftCardBalance = function (giftCardNumber, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                var data = {
                                    "giftCardId": giftCardNumber
                                };
                                this.proxy.SubmitRequest("GetGiftCardInformation", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            StoreOperationsWebApi.RetrieveCardPaymentAcceptResult = function (cardPaymentResultAccessCode, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    "cardPaymentResultAccessCode": cardPaymentResultAccessCode
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("RetrieveCardPaymentAcceptResult", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            return StoreOperationsWebApi;
                        })();
                        Controls.StoreOperationsWebApi = StoreOperationsWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Helpers/Core.ts" />
/// <reference path="../Helpers/AsyncResult.ts" />
"use strict";
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var SalesOrderWebApi = (function () {
                            function SalesOrderWebApi() {
                            }
                            SalesOrderWebApi.GetProxy = function () {
                                this.proxy = new Controls.AjaxProxy(msaxValues.msax_SalesOrderWebApiUrl + '/');
                            };
                            SalesOrderWebApi.GetSalesOrderByCriteria = function (orderSearchCriteria, callerContext) {
                                var asyncResult = new CommerceProxy.AsyncResult(callerContext);
                                var data = {
                                    'queryResultSettings': Controls.Core.getDefaultQueryResultSettings(),
                                    'salesOrderSearchCriteria': orderSearchCriteria
                                };
                                if (Ecommerce.Utils.isNullOrUndefined(this.proxy)) {
                                    this.GetProxy();
                                }
                                this.proxy.SubmitRequest("GetSalesOrder", data, function (response) {
                                    asyncResult.resolve(response);
                                }, function (errors) {
                                    asyncResult.reject(errors.responseJSON);
                                });
                                return asyncResult;
                            };
                            return SalesOrderWebApi;
                        })();
                        Controls.SalesOrderWebApi = SalesOrderWebApi;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Common/Helpers/EcommerceTypes.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var CustomerAddress = (function () {
                            function CustomerAddress(element) {
                                var _this = this;
                                this._addressView = $(element);
                                this._errorPanel = this._addressView.find(" > .msax-ErrorPanel");
                                this._loadingDialog = this._addressView.find('.msax-Loading');
                                this._loadingText = this._loadingDialog.find('.msax-LoadingText');
                                Controls.LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
                                this.errorMessages = ko.observableArray([]);
                                this.addresses = ko.observableArray(null);
                                this.areAddressesLoaded = ko.observable(false);
                                this.addressDisplayEnabled = ko.computed(function () {
                                    return Ecommerce.Utils.hasElements(_this.addresses());
                                });
                                this.getCustomer();
                            }
                            CustomerAddress.prototype.closeDialogAndDisplayError = function (errorMessages, isError) {
                                Controls.LoadingOverlay.CloseLoadingDialog();
                                this.showError(errorMessages, isError);
                            };
                            CustomerAddress.prototype.showError = function (errorMessages, isError) {
                                this.errorMessages(errorMessages);
                                if (isError) {
                                    this._errorPanel.addClass("msax-Error");
                                }
                                else if (this._errorPanel.hasClass("msax-Error")) {
                                    this._errorPanel.removeClass("msax-Error");
                                }
                                this._errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            CustomerAddress.prototype.getCustomer = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.customerServiceGetCustomerStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.CustomerWebApi.GetCustomer(this)
                                    .done(function (customerResponse) {
                                    if (Ecommerce.Utils.isNullOrUndefined(customerResponse) || Ecommerce.Utils.isNullOrUndefined(customerResponse)) {
                                        _this.showError([Controls.Resources.String_209], true);
                                    }
                                    else {
                                        _this.addresses(customerResponse.Addresses);
                                        _this.areAddressesLoaded(true);
                                    }
                                    CommerceProxy.RetailLogger.customerServiceGetCustomerFinished();
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Controls.Resources.String_209);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            return CustomerAddress;
                        })();
                        Controls.CustomerAddress = CustomerAddress;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Common/Helpers/EcommerceTypes.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var MiniCart = (function () {
                            function MiniCart(element) {
                                var _this = this;
                                this._cartView = $(element);
                                this.errorMessages = ko.observableArray([]);
                                this.errorPanel = this._cartView.find(" .msax-ErrorPanel");
                                this._miniCart = this._cartView.find(" > .msax-MiniCart");
                                this.isCheckoutCart = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_IsCheckoutCart) ? false : msaxValues.msax_IsCheckoutCart.toLowerCase() == "true");
                                if (!this.isCheckoutCart()) {
                                    this.cartType = CommerceProxy.Entities.CartType.Shopping;
                                    this.getShoppingCart();
                                }
                                else {
                                    this.cartType = CommerceProxy.Entities.CartType.Checkout;
                                }
                                var cart = new CommerceProxy.Entities.CartClass(null);
                                cart.CartLines = [];
                                cart.DiscountCodes = [];
                                this.cart = ko.observable(cart);
                                if (this.isCheckoutCart()) {
                                    Controls.CartWebApi.OnUpdateCheckoutCart(this, this.updateCart);
                                }
                                else {
                                    Controls.CartWebApi.OnUpdateShoppingCart(this, this.updateCart);
                                }
                                this._cartView.keypress(function (event) {
                                    if (event.keyCode == 13 || event.keyCode == 8 || event.keyCode == 27) {
                                        event.preventDefault();
                                        return false;
                                    }
                                    return true;
                                });
                                this.isShoppingCartEnabled = ko.computed(function () {
                                    return !Ecommerce.Utils.isNullOrUndefined(_this.cart()) && Ecommerce.Utils.hasElements(_this.cart().CartLines);
                                });
                                $(window).resize($.proxy(this.repositionMiniCart, this));
                            }
                            MiniCart.prototype.getResx = function (key) {
                                return Controls.Resources[key];
                            };
                            MiniCart.prototype.formatCurrencyString = function (amount) {
                                if (isNaN(amount)) {
                                    return amount;
                                }
                                var formattedCurrencyString = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(amount)) {
                                    if (Ecommerce.Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                                        formattedCurrencyString = amount.toString();
                                    }
                                    else {
                                        formattedCurrencyString = Ecommerce.Utils.format(this.currencyStringTemplate, Ecommerce.Utils.formatNumber(amount));
                                    }
                                }
                                return formattedCurrencyString;
                            };
                            MiniCart.prototype.shoppingCartNextClick = function (viewModel, event) {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(msaxValues.msax_CheckoutUrl)) {
                                    window.location.href = msaxValues.msax_CheckoutUrl;
                                }
                            };
                            MiniCart.prototype.disableUserActions = function () {
                                this._cartView.find('*').disabled = true;
                            };
                            MiniCart.prototype.enableUserActions = function () {
                                this._cartView.find('*').disabled = false;
                            };
                            MiniCart.prototype.showError = function (errorMessages, isError) {
                                this.errorMessages(errorMessages);
                                if (isError) {
                                    this.errorPanel.addClass("msax-Error");
                                }
                                else if (this.errorPanel.hasClass("msax-Error")) {
                                    this.errorPanel.removeClass("msax-Error");
                                }
                                this.errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            MiniCart.prototype.viewCartClick = function () {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(msaxValues.msax_ShoppingCartUrl)) {
                                    window.location.href = msaxValues.msax_ShoppingCartUrl;
                                }
                            };
                            MiniCart.prototype.showMiniCart = function () {
                                this.toggleCartDisplay(true);
                            };
                            MiniCart.prototype.hideMiniCart = function () {
                                this.toggleCartDisplay(false);
                            };
                            MiniCart.prototype.toggleCartDisplay = function (show) {
                                if (!Ecommerce.Utils.isNullOrUndefined(this._miniCartButton)) {
                                    var locationOffScreen = -1 * this._miniCart.height() - 200;
                                    this.miniCartWidth = this._miniCart.width() + 3;
                                    this.miniCartButtonLocation = this._miniCartButton.offset();
                                    this.miniCartButtonWidth = this._miniCartButton.width();
                                    var miniCartButtonHeight = this._miniCartButton.height() + 3;
                                    if (show) {
                                        this.isMiniCartVisible = false;
                                        setTimeout($.proxy(function () {
                                            if (!this.isMiniCartVisible) {
                                                this._miniCart.animate({ top: this.miniCartButtonLocation.top + miniCartButtonHeight, left: this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth }, 300, 'linear');
                                            }
                                        }, this), 500);
                                    }
                                    else {
                                        this.isMiniCartVisible = true;
                                        setTimeout($.proxy(function () {
                                            if (this.isMiniCartVisible) {
                                                this._miniCart.animate({ top: locationOffScreen, left: this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth }, 300);
                                            }
                                        }, this), 500);
                                    }
                                }
                            };
                            MiniCart.prototype.repositionMiniCart = function () {
                                if (Ecommerce.Utils.isNullOrUndefined(this._miniCartButton)) {
                                    this._miniCartButton = this._cartView.find("#MiniCartButton");
                                }
                                this.miniCartButtonLocation = this._miniCartButton.offset();
                                this.miniCartButtonWidth = this._miniCartButton.width();
                                this.miniCartWidth = this._miniCart.width();
                                this._miniCart[0].style.left = this.miniCartButtonLocation.left - this.miniCartWidth + this.miniCartButtonWidth + "px";
                            };
                            MiniCart.prototype.updateCart = function (event, data) {
                                var _this = this;
                                Controls.CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Shopping, false)
                                    .done(function (cart) {
                                    _this.currencyStringTemplate = Controls.Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                                    _this.cart(cart);
                                    _this._miniCartButton = _this._cartView.find("#MiniCartButton");
                                    _this.repositionMiniCart();
                                    _this.hideMiniCart();
                                });
                            };
                            MiniCart.prototype.getShoppingCart = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
                                this.disableUserActions();
                                Controls.CartWebApi.GetCart(CommerceProxy.Entities.CartType.Shopping, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_63], true);
                                    }
                                    _this.enableUserActions();
                                    _this.errorPanel.hide();
                                    CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Controls.Resources.String_63);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.showError(errorMessages, true);
                                    _this.enableUserActions();
                                });
                            };
                            MiniCart.prototype.removeFromCartClick = function (cartLine) {
                                var _this = this;
                                this.disableUserActions();
                                CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();
                                Controls.CartWebApi.RemoveFromCart(this.cartType, [cartLine.LineId], this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_64], true);
                                    }
                                    _this.enableUserActions();
                                    _this.errorPanel.hide();
                                    CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartError, errors, Controls.Resources.String_64);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.showError(errorMessages, true);
                                    _this.enableUserActions();
                                });
                            };
                            return MiniCart;
                        })();
                        Controls.MiniCart = MiniCart;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var OrderDetails = (function () {
                            function OrderDetails(element) {
                                this.channelReferenceIdString = "channelReferenceId";
                                this.salesIdString = "salesId";
                                this.receiptIdString = "receiptId";
                                this._orderDetailsView = $(element);
                                this._loadingDialog = this._orderDetailsView.find('.msax-Loading');
                                this._loadingText = this._loadingDialog.find('.msax-LoadingText');
                                this.errorPanel = this._orderDetailsView.find(" > .msax-ErrorPanel");
                                Controls.LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
                                this.kitVariantProductType = ko.observable(CommerceProxy.Entities.ProductType.KitVariant);
                                this.isSalesOrderLoaded = ko.observable(false);
                                this.errorMessages = ko.observableArray([]);
                                this.salesOrders = null;
                                this.salesOrder = ko.observable(null);
                                this.channelReferenceId = Ecommerce.Utils.getQueryStringValue(this.channelReferenceIdString);
                                this.salesId = Ecommerce.Utils.getQueryStringValue(this.salesIdString);
                                this.receiptId = Ecommerce.Utils.getQueryStringValue(this.receiptIdString);
                                this.getAllDeliveryOptionDescriptions();
                                var orderSearchCriteria = Controls.Core.getOrderSearchCriteria(this.channelReferenceId, this.salesId, this.receiptId, true);
                                this.getOrderDetails(orderSearchCriteria);
                            }
                            OrderDetails.prototype.getResx = function (key) {
                                return Controls.Resources[key];
                            };
                            OrderDetails.prototype.formatCurrencyString = function (amount) {
                                if (isNaN(amount)) {
                                    return amount;
                                }
                                var formattedCurrencyString = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(amount)) {
                                    if (Ecommerce.Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                                        formattedCurrencyString = amount.toString();
                                    }
                                    else {
                                        formattedCurrencyString = Ecommerce.Utils.format(this.currencyStringTemplate, Ecommerce.Utils.formatNumber(amount));
                                    }
                                }
                                return formattedCurrencyString;
                            };
                            OrderDetails.prototype.getDeliveryModeText = function (deliveryModeId) {
                                var deliveryModeText = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(this.allDeliveryOptionDescriptions)) {
                                    for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                                        if (this.allDeliveryOptionDescriptions[i].Code == deliveryModeId) {
                                            deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                                            break;
                                        }
                                    }
                                }
                                return deliveryModeText;
                            };
                            OrderDetails.prototype.closeDialogAndDisplayError = function (errorMessages, isError) {
                                Controls.LoadingOverlay.CloseLoadingDialog();
                                this.showError(errorMessages, isError);
                            };
                            OrderDetails.prototype.showError = function (errorMessages, isError) {
                                this.errorMessages(errorMessages);
                                if (isError) {
                                    this.errorPanel.addClass("msax-Error");
                                }
                                else if (this.errorPanel.hasClass("msax-Error")) {
                                    this.errorPanel.removeClass("msax-Error");
                                }
                                this.errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            OrderDetails.prototype.getSalesOrderStatusString = function (statusValue) {
                                return Controls.Resources.String_242 + Controls.Core.getSalesStatusString(statusValue);
                            };
                            OrderDetails.prototype.getOrderDetails = function (orderSearchCriteria) {
                                var _this = this;
                                CommerceProxy.RetailLogger.getOrderDetailsStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.SalesOrderWebApi.GetSalesOrderByCriteria(orderSearchCriteria, this)
                                    .done(function (data) {
                                    var salesOrderResponse = data[0];
                                    _this.currencyStringTemplate = Controls.Core.getExtensionPropertyValue(salesOrderResponse.ExtensionProperties, "CurrencyStringTemplate");
                                    salesOrderResponse["OrderNumber"] = Controls.Core.getOrderNumber(salesOrderResponse);
                                    var productIds = [];
                                    for (var j = 0; j < salesOrderResponse.SalesLines.length; j++) {
                                        productIds.push(salesOrderResponse.SalesLines[j].ProductId);
                                    }
                                    CommerceProxy.RetailLogger.getSimpleProductsByIdStarted();
                                    Controls.ProductWebApi.GetSimpleProducts(productIds, _this)
                                        .done(function (simpleProducts) {
                                        CommerceProxy.RetailLogger.getSimpleProductsByIdFinished();
                                        var simpleProductsByIdMap = [];
                                        for (var i = 0; i < simpleProducts.length; i++) {
                                            var key = simpleProducts[i].RecordId;
                                            simpleProductsByIdMap[key] = simpleProducts[i];
                                        }
                                        for (var i = 0; i < salesOrderResponse.SalesLines.length; i++) {
                                            var salesLine = salesOrderResponse.SalesLines[i];
                                            Controls.Core.populateProductDetailsForSalesLine(salesLine, simpleProductsByIdMap, _this.currencyStringTemplate);
                                            Controls.Core.populateKitItemDetailsForSalesLine(salesLine, simpleProductsByIdMap, _this.currencyStringTemplate);
                                        }
                                        _this.salesOrder(salesOrderResponse);
                                        _this.isSalesOrderLoaded(true);
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.getOrderDetailsFinished();
                                    })
                                        .fail(function (errors) {
                                        CommerceProxy.RetailLogger.getSimpleProductsByIdError(errors[0].LocalizedErrorMessage);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.getOrderDetailsError, errors, Controls.Resources.String_237);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            OrderDetails.prototype.getAllDeliveryOptionDescriptions = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.OrgUnitWebApi.GetDeliveryOptionsInfo(this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.allDeliveryOptionDescriptions = data;
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_237], true);
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsFinished();
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsError, errors, Controls.Resources.String_237);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            return OrderDetails;
                        })();
                        Controls.OrderDetails = OrderDetails;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var OrderHistory = (function () {
                            function OrderHistory(element) {
                                var _this = this;
                                this.orderCount = parseInt(msaxValues.msax_OrderCount);
                                this.orderCountToSkip = 0;
                                this.currentPageNumber = 1;
                                this._orderHistoryView = $(element);
                                this._loadingDialog = this._orderHistoryView.find('.msax-Loading');
                                this._loadingText = this._loadingDialog.find('.msax-LoadingText');
                                this.errorPanel = this._orderHistoryView.find(" > .msax-ErrorPanel");
                                this.pagingNav = this._orderHistoryView.find('.msax-Paging');
                                this.prevPage = this._orderHistoryView.find('.msax-PrevPage');
                                this.nextPage = this._orderHistoryView.find('.msax-NextPage');
                                this.currentPage = this._orderHistoryView.find('.msax-CurrentPage');
                                Controls.LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
                                this.errorMessages = ko.observableArray([]);
                                this.salesOrders = ko.observableArray([]);
                                this.isOrderHistoryEmpty = ko.computed(function () {
                                    return (_this.salesOrders().length == 0);
                                });
                                this.showPaging = ko.computed(function () {
                                    return (Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_ShowPaging) ? false : msaxValues.msax_ShowPaging.toLowerCase() == "true");
                                });
                                if (this.showPaging()) {
                                    this.getOrderHistory(this.orderCountToSkip, this.orderCount + 1);
                                }
                                else {
                                    this.getOrderHistory(this.orderCountToSkip, msaxValues.msax_OrderCount);
                                }
                            }
                            OrderHistory.prototype.getResx = function (key) {
                                return Controls.Resources[key];
                            };
                            OrderHistory.prototype.getOrderHistory = function (skip, top) {
                                var _this = this;
                                CommerceProxy.RetailLogger.getOrderHistoryStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.CustomerWebApi.GetOrderHistory(skip, top, this)
                                    .done(function (responseSalesOrders) {
                                    _this.salesOrders(responseSalesOrders);
                                    if (_this.showPaging() && responseSalesOrders.length == top) {
                                        _this.salesOrders.splice(top - 1, 1);
                                    }
                                    else {
                                        _this.nextPage.addClass("disabled");
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.getOrderHistoryFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.getOrderHistoryError, errors, Controls.Resources.String_230);
                                    _this.errorMessages([Controls.Resources.String_230]);
                                    _this.showError(true);
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                });
                            };
                            OrderHistory.prototype.nextPageClick = function () {
                                if (!this.nextPage.hasClass("disabled")) {
                                    this.getOrderHistory(this.orderCountToSkip + this.orderCount, 1 + this.orderCount);
                                    this.orderCountToSkip += this.orderCount;
                                    this.currentPageNumber++;
                                    this.currentPage.text(this.currentPageNumber);
                                    this.prevPage.removeClass("disabled");
                                }
                            };
                            OrderHistory.prototype.prevPageClick = function () {
                                if (!this.prevPage.hasClass("disabled")) {
                                    this.getOrderHistory(this.orderCountToSkip - this.orderCount, 1 + this.orderCount);
                                    this.orderCountToSkip -= this.orderCount;
                                    this.currentPageNumber--;
                                    this.currentPage.text(this.currentPageNumber);
                                    if (this.orderCountToSkip == 0) {
                                        this.prevPage.addClass("disabled");
                                    }
                                    this.nextPage.removeClass("disabled");
                                }
                            };
                            OrderHistory.prototype.getSalesStatusString = function (statusValue) {
                                return Controls.Core.getSalesStatusString(statusValue);
                            };
                            OrderHistory.prototype.formatCreatedDate = function (createdDateTime) {
                                var dateString = "";
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(createdDateTime)) {
                                    var date = new Date(parseInt(createdDateTime.substring(6, createdDateTime.length - 2)));
                                    dateString = (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
                                }
                                return dateString;
                            };
                            OrderHistory.prototype.getOrderDetailUrl = function (salesOrder) {
                                var url = msaxValues.msax_OrderDetailsUrl;
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(salesOrder.ChannelReferenceId)) {
                                    url += "?channelReferenceId=" + salesOrder.ChannelReferenceId;
                                }
                                else if (!Ecommerce.Utils.isNullOrWhiteSpace(salesOrder.SalesId)) {
                                    url += "?salesId=" + salesOrder.SalesId;
                                }
                                else if (!Ecommerce.Utils.isNullOrWhiteSpace(salesOrder.ReceiptId)) {
                                    url += "?receiptId=" + salesOrder.ReceiptId;
                                }
                                else {
                                    url = '#';
                                }
                                return url;
                            };
                            OrderHistory.prototype.getOrderNumber = function (salesOrder) {
                                return Controls.Core.getOrderNumber(salesOrder);
                            };
                            OrderHistory.prototype.showError = function (isError) {
                                if (isError) {
                                    this.errorPanel.addClass("msax-Error");
                                }
                                else if (this.errorPanel.hasClass("msax-Error")) {
                                    this.errorPanel.removeClass("msax-Error");
                                }
                                this.errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            return OrderHistory;
                        })();
                        Controls.OrderHistory = OrderHistory;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        var Cart = (function () {
                            function Cart(element) {
                                var _this = this;
                                this._cartView = $(element);
                                this.errorMessages = ko.observableArray([]);
                                this.errorPanel = this._cartView.find(" > .msax-ErrorPanel");
                                this.kitVariantProductType = ko.observable(CommerceProxy.Entities.ProductType.KitVariant);
                                this._editRewardCardDialog = this._cartView.find('.msax-EditRewardCard');
                                this._loadingDialog = this._cartView.find('.msax-Loading');
                                this._loadingText = this._loadingDialog.find('.msax-LoadingText');
                                Controls.LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
                                this.isShoppingCartLoaded = ko.observable(false);
                                this.supportDiscountCodes = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_CartDiscountCodes) ? true : msaxValues.msax_CartDiscountCodes.toLowerCase() == "true");
                                this.supportLoyaltyReward = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_CartLoyaltyReward) ? true : msaxValues.msax_CartLoyaltyReward.toLowerCase() == "true");
                                this.displayPromotionBanner = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_CartDisplayPromotionBanner) ? true : msaxValues.msax_CartDisplayPromotionBanner.toLowerCase() == "true");
                                this.getShoppingCart();
                                var cart = new CommerceProxy.Entities.CartClass(null);
                                cart.CartLines = [];
                                cart.DiscountCodes = [];
                                this.cart = ko.observable(cart);
                                Controls.CartWebApi.OnUpdateShoppingCart(this, this.updateShoppingCart);
                                this._cartView.keypress(function (event) {
                                    if (event.keyCode == 13 || event.keyCode == 8 || event.keyCode == 27) {
                                        event.preventDefault();
                                        return false;
                                    }
                                    return true;
                                });
                                this.isShoppingCartEnabled = ko.computed(function () {
                                    return !Ecommerce.Utils.isNullOrUndefined(_this.cart()) && Ecommerce.Utils.hasElements(_this.cart().CartLines);
                                });
                                this.isPromotionCodesEnabled = ko.computed(function () {
                                    return !Ecommerce.Utils.isNullOrUndefined(_this.cart()) && Ecommerce.Utils.hasElements(_this.cart().DiscountCodes);
                                });
                            }
                            Cart.prototype.getResx = function (key) {
                                return Controls.Resources[key];
                            };
                            Cart.prototype.formatCurrencyString = function (amount) {
                                if (isNaN(amount)) {
                                    return amount;
                                }
                                var formattedCurrencyString = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(amount)) {
                                    if (Ecommerce.Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                                        formattedCurrencyString = amount.toString();
                                    }
                                    else {
                                        formattedCurrencyString = Ecommerce.Utils.format(this.currencyStringTemplate, Ecommerce.Utils.formatNumber(amount));
                                    }
                                }
                                return formattedCurrencyString;
                            };
                            Cart.prototype.shoppingCartNextClick = function (viewModel, event) {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(msaxValues.msax_CheckoutUrl)) {
                                    window.location.href = msaxValues.msax_CheckoutUrl;
                                }
                            };
                            Cart.prototype.quantityMinusClick = function (cartLine) {
                                if (cartLine.Quantity == 1) {
                                    this.removeFromCartClick(cartLine);
                                }
                                else {
                                    cartLine.Quantity = cartLine.Quantity - 1;
                                    this.updateQuantity([cartLine]);
                                }
                            };
                            Cart.prototype.quantityPlusClick = function (cartLine) {
                                cartLine.Quantity = cartLine.Quantity + 1;
                                this.updateQuantity([cartLine]);
                            };
                            Cart.prototype.quantityTextBoxChanged = function (cartLine, valueAccesor) {
                                var srcElement = valueAccesor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    if (Ecommerce.Utils.isNullOrWhiteSpace(srcElement.value)) {
                                        srcElement.value = cartLine.Quantity;
                                        return;
                                    }
                                    var enteredNumber = Number(srcElement.value);
                                    if (isNaN(enteredNumber)) {
                                        srcElement.value = cartLine.Quantity;
                                        return;
                                    }
                                    if (enteredNumber != cartLine.Quantity) {
                                        cartLine.Quantity = enteredNumber;
                                        if (cartLine.Quantity < 0) {
                                            cartLine.Quantity = 1;
                                        }
                                        if (cartLine.Quantity == 0) {
                                            this.removeFromCartClick(cartLine);
                                        }
                                        else {
                                            this.updateQuantity([cartLine]);
                                        }
                                    }
                                }
                            };
                            Cart.prototype.closeDialogAndDisplayError = function (errorMessages, isError) {
                                Controls.LoadingOverlay.CloseLoadingDialog();
                                this.showError(errorMessages, isError);
                            };
                            Cart.prototype.showError = function (errorMessages, isError) {
                                this.errorMessages(errorMessages);
                                if (isError) {
                                    this.errorPanel.addClass("msax-Error");
                                }
                                else if (this.errorPanel.hasClass("msax-Error")) {
                                    this.errorPanel.removeClass("msax-Error");
                                }
                                this.errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            Cart.prototype.editRewardCardOverlayClick = function () {
                                this.dialogOverlay = $('.ui-widget-overlay');
                                this.dialogOverlay.on('click', $.proxy(this.closeEditRewardCardDialog, this));
                            };
                            Cart.prototype.createEditRewardCardDialog = function () {
                                this._editRewardCardDialog.dialog({
                                    modal: true,
                                    title: Controls.Resources.String_186,
                                    autoOpen: false,
                                    draggable: true,
                                    resizable: false,
                                    closeOnEscape: true,
                                    show: { effect: "fadeIn", duration: 500 },
                                    hide: { effect: "fadeOut", duration: 500 },
                                    width: 500,
                                    height: 300,
                                    dialogClass: 'msax-Control'
                                });
                            };
                            Cart.prototype.showEditRewardCardDialog = function () {
                                $('.ui-dialog-titlebar-close').on('click', $.proxy(this.closeEditRewardCardDialog, this));
                                this._editRewardCardDialog.dialog('open');
                                this.editRewardCardOverlayClick();
                            };
                            Cart.prototype.closeEditRewardCardDialog = function () {
                                this._editRewardCardDialog.dialog('close');
                            };
                            Cart.prototype.continueShoppingClick = function () {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(msaxValues.msax_ContinueShoppingUrl)) {
                                    window.location.href = msaxValues.msax_ContinueShoppingUrl;
                                }
                            };
                            Cart.prototype.updateShoppingCart = function (event, data) {
                                var _this = this;
                                Controls.CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Shopping, this.displayPromotionBanner())
                                    .done(function (cart) {
                                    _this.currencyStringTemplate = Controls.Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                                    _this.cart(data);
                                    _this.errorPanel.hide();
                                    _this.isShoppingCartLoaded(true);
                                    _this._cartView.find('.msax-ChargeAmount .msax-FooterValue').tooltip();
                                    _this._cartView.find('.msax-TaxAmount .msax-FooterValue').tooltip();
                                });
                            };
                            Cart.prototype.getShoppingCart = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.CartWebApi.GetCart(CommerceProxy.Entities.CartType.Shopping, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_63], true);
                                    }
                                    _this.createEditRewardCardDialog();
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Controls.Resources.String_63);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Cart.prototype.removeFromCartClick = function (cartLine) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                Controls.CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Shopping, [cartLine.LineId], this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_64], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartError, errors, Controls.Resources.String_64);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Cart.prototype.updateQuantity = function (cartLines) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartUpdateQuantityStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                Controls.CartWebApi.UpdateQuantity(CommerceProxy.Entities.CartType.Shopping, cartLines, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_65], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartUpdateQuantityFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateQuantityError, errors, Controls.Resources.String_65);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Cart.prototype.applyPromotionCode = function (cart, valueAccesor) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var srcElement = valueAccesor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement.firstElementChild)) {
                                    if (!Ecommerce.Utils.isNullOrWhiteSpace(srcElement.parentElement.firstElementChild.value)) {
                                        var promoCode = srcElement.parentElement.firstElementChild.value;
                                        Controls.CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Shopping, promoCode, true, this)
                                            .done(function (cart) {
                                            if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                                Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                            }
                                            else {
                                                _this.showError([Controls.Resources.String_93], true);
                                            }
                                            Controls.LoadingOverlay.CloseLoadingDialog();
                                            CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeFinished();
                                        })
                                            .fail(function (errors) {
                                            Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeError, errors, Controls.Resources.String_93);
                                            var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                            _this.closeDialogAndDisplayError(errorMessages, true);
                                        });
                                    }
                                    else {
                                        this.showError([Controls.Resources.String_97], true);
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                    }
                                }
                                else {
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                }
                            };
                            Cart.prototype.removePromotionCode = function (cart, valueAccesor) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var srcElement = valueAccesor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement.lastElementChild)
                                    && !Ecommerce.Utils.isNullOrWhiteSpace(srcElement.parentElement.lastElementChild.textContent)) {
                                    var promoCode = srcElement.parentElement.lastElementChild.textContent;
                                    Controls.CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Shopping, promoCode, false, this)
                                        .done(function (cart) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Shopping, cart);
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_94], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeError, errors, Controls.Resources.String_94);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                                else {
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                }
                            };
                            Cart.prototype.updateLoyaltyCardId = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var loyaltyCardId = this._editRewardCardDialog.find('#RewardCardTextBox').val();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(loyaltyCardId)) {
                                    Controls.CartWebApi.UpdateLoyaltyCardId(CommerceProxy.Entities.CartType.Shopping, loyaltyCardId, this)
                                        .done(function (cart) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            _this.closeEditRewardCardDialog();
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_232], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdError, errors, Controls.Resources.String_232);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeEditRewardCardDialog();
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                            };
                            return Cart;
                        })();
                        Controls.Cart = Cart;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../Common/Helpers/Core.ts" />
/// <reference path="../Common/Helpers/EcommerceTypes.ts" />
/// <reference path="../Resources/Resources.ts" />
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        "use strict";
                        (function (InitEntitySet) {
                            InitEntitySet[InitEntitySet["None"] = 0] = "None";
                            InitEntitySet[InitEntitySet["CheckoutCart"] = 1] = "CheckoutCart";
                            InitEntitySet[InitEntitySet["DeliveryDescriptions"] = 2] = "DeliveryDescriptions";
                            InitEntitySet[InitEntitySet["IsAuthSession"] = 4] = "IsAuthSession";
                            InitEntitySet[InitEntitySet["Customer"] = 8] = "Customer";
                            InitEntitySet[InitEntitySet["ChannelConfigurations"] = 16] = "ChannelConfigurations";
                            InitEntitySet[InitEntitySet["TenderTypes"] = 32] = "TenderTypes";
                            InitEntitySet[InitEntitySet["CountryRegion"] = 64] = "CountryRegion";
                            InitEntitySet[InitEntitySet["DeliveryPreferences"] = 128] = "DeliveryPreferences";
                            InitEntitySet[InitEntitySet["CardTypes"] = 256] = "CardTypes";
                            InitEntitySet[InitEntitySet["All"] = 511] = "All";
                        })(Controls.InitEntitySet || (Controls.InitEntitySet = {}));
                        var InitEntitySet = Controls.InitEntitySet;
                        (function (InitPaymentEntitySet) {
                            InitPaymentEntitySet[InitPaymentEntitySet["None"] = 0] = "None";
                            InitPaymentEntitySet[InitPaymentEntitySet["LoyaltyCards"] = 1] = "LoyaltyCards";
                            InitPaymentEntitySet[InitPaymentEntitySet["SetDeliveryPreferences"] = 2] = "SetDeliveryPreferences";
                            InitPaymentEntitySet[InitPaymentEntitySet["All"] = 3] = "All";
                        })(Controls.InitPaymentEntitySet || (Controls.InitPaymentEntitySet = {}));
                        var InitPaymentEntitySet = Controls.InitPaymentEntitySet;
                        var Checkout = (function () {
                            function Checkout(element) {
                                var _this = this;
                                this._checkoutFragments = {
                                    DeliveryPreferences: "msax-DeliveryPreferences",
                                    PaymentInformation: "msax-PaymentInformation",
                                    Review: "msax-Review",
                                    Confirmation: "msax-Confirmation"
                                };
                                this.DisableTouchInputOnMap = false;
                                this._deliveryPreferencesFragments = {
                                    ShipItemsOrderLevel: "msax-ShipItemsOrderLevel",
                                    PickUpInStoreOrderLevel: "msax-PickUpInStoreOrderLevel",
                                    EmailOrderLevel: "msax-EmailOrderLevel",
                                    ItemLevelPreference: "msax-ItemLevelPreference"
                                };
                                this._itemDeliveryPreferencesFragments = {
                                    ShipItemsItemLevel: "msax-ShipItemsItemLevel",
                                    PickUpInStoreItemLevel: "msax-PickUpInStoreItemLevel",
                                    EmailItemLevel: "msax-EmailItemLevel"
                                };
                                this.tenderLines = [];
                                this.checkGiftCardAmountValidity = false;
                                this.supportedTenderTypes = [];
                                this.cardTypes = [];
                                this.expirationYear = [
                                    "2014",
                                    "2015",
                                    "2016",
                                    "2017",
                                    "2018",
                                    "2019"
                                ];
                                this._checkoutView = $(element);
                                this.errorMessages = ko.observableArray([]);
                                this.errorPanel = this._checkoutView.find(" > .msax-ErrorPanel");
                                this.nextButton = this._checkoutView.find('.msax-Next');
                                this.kitVariantProductType = ko.observable(CommerceProxy.Entities.ProductType.KitVariant);
                                this._loadingDialog = this._checkoutView.find('.msax-Loading');
                                this._loadingText = this._loadingDialog.find('.msax-LoadingText');
                                Controls.LoadingOverlay.CreateLoadingDialog(this._loadingDialog, this._loadingText, 200, 200);
                                this.countries = ko.observableArray([{ CountryCode: "NoSelection", CountryName: "Select a country:" }]);
                                this.states = ko.observableArray(null);
                                var cart = new CommerceProxy.Entities.CartClass(null);
                                cart.CartLines = [];
                                cart.DiscountCodes = [];
                                cart.ShippingAddress = new CommerceProxy.Entities.AddressClass(null);
                                this.cart = ko.observable(cart);
                                var selectedHeaderLevelDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass();
                                selectedHeaderLevelDeliveryOption.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
                                this.latestHeaderLevelDeliverySpecification = ko.observable(selectedHeaderLevelDeliveryOption);
                                this.isAuthenticated = ko.observable(false);
                                this._initEntitySetCompleted = 0;
                                this._initEntitySetFailed = 0;
                                this._initEntityErrors = [];
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                this.isAuthenticatedSession();
                                this.commenceCheckout();
                                this.getAllDeliveryOptionDescriptions();
                                Controls.CartWebApi.OnUpdateCheckoutCart(this, this.updateCheckoutCart);
                                this.allowedHeaderLevelDeliveryPreferences = ko.observableArray([{ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Controls.Resources.String_159 }]);
                                ({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Controls.Resources.String_159 });
                                this.selectedOrderDeliveryPreference = ko.observable(null);
                                this._deliveryPreferencesView = this._checkoutView.find(" > ." + this._checkoutFragments.DeliveryPreferences);
                                this.deliveryPreferenceToValidate = ko.observable(null);
                                this.tempShippingAddress = ko.observable(null);
                                this.selectedDeliveryOptionByLineIdMap = [];
                                this.storedCustomerAddresses = ko.observableArray(null);
                                this.orderLevelSelectedAddress = ko.observable(null);
                                this.lineLevelSelectedAddress = ko.observable(null);
                                this.availableDeliveryOptions = ko.observableArray(null);
                                var selectedOrderDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                selectedOrderDeliveryOption.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                this.isBillingAddressSameAsShippingAddress = ko.observable(false);
                                this.sendEmailToMe = ko.observable(false);
                                this.displayLocations = ko.observableArray(null);
                                this.hasInventoryCheck = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_HasInventoryCheck) ? true : msaxValues.msax_HasInventoryCheck.toLowerCase() == "true");
                                this.currentCartLine = ko.observable(null);
                                var selectedLineDeliveryOption = new CommerceProxy.Entities.LineDeliverySpecificationClass(null);
                                selectedLineDeliveryOption.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                selectedLineDeliveryOption.DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
                                selectedLineDeliveryOption.DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                this.currentLineDeliverySpecification = ko.observable(selectedLineDeliveryOption);
                                this.currentLineLevelSelectedDeliveryPreference = ko.observable(CommerceProxy.Entities.DeliveryPreferenceType.None);
                                this.itemDeliveryPreferenceToValidate = ko.observable(null);
                                this.showItemDeliveryPreferenceDialog = ko.observable(false);
                                this._itemLevelDeliveryPreferenceSelection = this._deliveryPreferencesView.find('.msax-ItemLevelPreference .msax-ItemLevelPreferenceSelection');
                                this._paymentView = this._checkoutView.find(" > ." + this._checkoutFragments.PaymentInformation);
                                this._addDiscountCodeDialog = this._paymentView.find('.msax-PayPromotionCode .msax-AddDiscountCodeDialog');
                                this.useShippingAddressForBilling = ko.observable(false);
                                this.paymentCardTypes = ko.observableArray(null);
                                this.confirmEmailValue = ko.observable('');
                                this.paymentCard = ko.observable(null);
                                var paymentCard = new CommerceProxy.Entities.PaymentCardClass(null);
                                paymentCard.ExpirationYear = 2016;
                                paymentCard.ExpirationMonth = 1;
                                this.paymentCard(paymentCard);
                                this.paymentCardAddress = ko.observable(new CommerceProxy.Entities.AddressClass());
                                this.formattedCreditCardAmount = ko.observable('');
                                this.giftCardNumber = ko.observable('');
                                this.formattedGiftCardAmount = ko.observable('');
                                this.isGiftCardInfoAvailable = ko.observable(false);
                                this.giftCardBalance = ko.observable('');
                                this._paymentView.find('.msax-GiftCardBalance').hide();
                                this.formattedPaymentTotal = ko.observable(Ecommerce.Utils.formatNumber(0));
                                this._creditCardPanel = this._paymentView.find('.msax-PayCreditCard .msax-CreditCardDetails');
                                this._giftCardPanel = this._paymentView.find('.msax-PayGiftCard .msax-GiftCardDetails');
                                this._loyaltyCardPanel = this._paymentView.find('.msax-PayLoyaltyCard .msax-LoyaltyCardDetails');
                                this.loyaltyCards = ko.observableArray(null);
                                this.loyaltyCardNumber = ko.observable('');
                                this.formattedLoyaltyCardAmount = ko.observable('');
                                this.payCreditCard = ko.observable(false);
                                this.payGiftCard = ko.observable(false);
                                this.payLoyaltyCard = ko.observable(false);
                                this.expirationMonths = ko.observableArray([
                                    { key: 1, value: Controls.Resources.String_192 },
                                    { key: 2, value: Controls.Resources.String_193 },
                                    { key: 3, value: Controls.Resources.String_194 },
                                    { key: 4, value: Controls.Resources.String_195 },
                                    { key: 5, value: Controls.Resources.String_196 },
                                    { key: 6, value: Controls.Resources.String_197 },
                                    { key: 7, value: Controls.Resources.String_198 },
                                    { key: 8, value: Controls.Resources.String_199 },
                                    { key: 9, value: Controls.Resources.String_200 },
                                    { key: 10, value: Controls.Resources.String_201 },
                                    { key: 11, value: Controls.Resources.String_202 },
                                    { key: 12, value: Controls.Resources.String_203 }
                                ]);
                                this.isCardPaymentAcceptPage = ko.observable(false);
                                this.cardPaymentAcceptPageUrl = ko.observable('');
                                this.cardPaymentAcceptMessageHandlerProxied = $.proxy(this.cardPaymentAcceptMessageHandler, this);
                                this.tokenizedCartTenderLine = null;
                                this.isEmailDeliverySet = ko.observable(false);
                                this.isOrderLevelDeliverySet = ko.observable(false);
                                this._editRewardCardDialog = this._checkoutView.find('.msax-EditRewardCard');
                                this.displayPromotionBanner = ko.observable(Ecommerce.Utils.isNullOrUndefined(msaxValues.msax_ReviewDisplayPromotionBanner) ? true : msaxValues.msax_ReviewDisplayPromotionBanner.toLowerCase() == "true");
                                this.orderNumber = ko.observable(null);
                                this._checkoutView.keypress(function (event) {
                                    if (event.keyCode == 13 || event.keyCode == 27) {
                                        event.preventDefault();
                                        return false;
                                    }
                                    return true;
                                });
                                this.isShoppingCartEnabled = ko.computed(function () {
                                    return !Ecommerce.Utils.isNullOrUndefined(_this.cart()) && Ecommerce.Utils.hasElements(_this.cart().CartLines);
                                });
                                this.isPromotionCodesEnabled = ko.computed(function () {
                                    return !Ecommerce.Utils.isNullOrUndefined(_this.cart()) && Ecommerce.Utils.hasElements(_this.cart().DiscountCodes);
                                });
                                this.selectedOrderDeliveryPreference.subscribe(function (newValue) {
                                    _this.resetSelectedOrderShippingOptions();
                                    _this.hideError();
                                    _this.isEmailDeliverySet(false);
                                    _this.isOrderLevelDeliverySet(true);
                                    if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                        var headerLevelDeliverySpecification = _this.latestHeaderLevelDeliverySpecification();
                                        if (Ecommerce.Utils.isNullOrUndefined(headerLevelDeliverySpecification.DeliveryAddress)) {
                                            headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                            _this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                                        }
                                        headerLevelDeliverySpecification = _this.latestHeaderLevelDeliverySpecification();
                                        if ((msaxValues.msax_IsDemoMode.toLowerCase() == "true")
                                            && (Ecommerce.Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.City)
                                                || Ecommerce.Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.Street)
                                                || Ecommerce.Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.State)
                                                || Ecommerce.Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.ZipCode)
                                                || Ecommerce.Utils.isNullOrWhiteSpace(headerLevelDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName))) {
                                            _this.autoFillCheckout();
                                        }
                                        var tempAddress = Ecommerce.Utils.clone(_this.latestHeaderLevelDeliverySpecification().DeliveryAddress);
                                        _this.tempShippingAddress(tempAddress);
                                        _this.deliveryPreferenceToValidate(' .' + _this._deliveryPreferencesFragments.ShipItemsOrderLevel);
                                        _this.showDeliveryPreferenceFragment(_this._deliveryPreferencesFragments.ShipItemsOrderLevel);
                                    }
                                    else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                        _this.deliveryPreferenceToValidate(' .' + _this._deliveryPreferencesFragments.PickUpInStoreOrderLevel);
                                        _this.showDeliveryPreferenceFragment(_this._deliveryPreferencesFragments.PickUpInStoreOrderLevel);
                                        _this._availableStoresView = _this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel .msax-AvailableStores");
                                        _this._location = _this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel input.msax-Location");
                                        _this._availableStoresView.hide();
                                        _this.map = _this._deliveryPreferencesView.find(".msax-PickUpInStoreOrderLevel .msax-Map");
                                        _this.getMap();
                                    }
                                    else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                                        _this.deliveryPreferenceToValidate(' .' + _this._deliveryPreferencesFragments.EmailOrderLevel);
                                        _this.showDeliveryPreferenceFragment(_this._deliveryPreferencesFragments.EmailOrderLevel);
                                        var _sendEmailToMeCheckBox = _this._checkoutView.find('.msax-EmailOrderLevel .msax-SendEmailToMe');
                                        _this._emailAddressTextBox = _this._deliveryPreferencesView.find('.msax-EmailOrderLevel .msax-EmailTextBox');
                                        if (_this._emailAddressTextBox.val() == _this.recepientEmailAddress) {
                                            _this.sendEmailToMe(true);
                                        }
                                        else {
                                            _this.sendEmailToMe(false);
                                        }
                                        var headerLevelDeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                        headerLevelDeliverySpecification.DeliveryModeId = _this.emailDeliveryModeCode;
                                        headerLevelDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                                        headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                        headerLevelDeliverySpecification.ElectronicDeliveryEmailAddress = _this._emailAddressTextBox.val();
                                        _this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                                        _this.isEmailDeliverySet(true);
                                    }
                                    else if (newValue == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                                        _this.deliveryPreferenceToValidate(' .' + _this._deliveryPreferencesFragments.ItemLevelPreference);
                                        _this.showDeliveryPreferenceFragment(_this._deliveryPreferencesFragments.ItemLevelPreference);
                                    }
                                    else {
                                        _this.deliveryPreferenceToValidate('');
                                        _this.showDeliveryPreferenceFragment('');
                                    }
                                }, this);
                                this.currentLineLevelSelectedDeliveryPreference.subscribe(function (deliveryPreferenceType) {
                                    _this.resetSelectedOrderShippingOptions();
                                    _this.hideError();
                                    _this.currentLineDeliverySpecification().LineId = _this.currentCartLine().LineId;
                                    _this.isEmailDeliverySet(false);
                                    _this.isOrderLevelDeliverySet(false);
                                    if (Ecommerce.Utils.isNullOrUndefined(deliveryPreferenceType)) {
                                        var currentDeliverySpecification = _this.currentLineDeliverySpecification().DeliverySpecification;
                                        if (Ecommerce.Utils.isNullOrUndefined(currentDeliverySpecification)) {
                                            _this.currentLineLevelSelectedDeliveryPreference(CommerceProxy.Entities.DeliveryPreferenceType.None);
                                        }
                                        else {
                                            if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                                deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                                            }
                                            else if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                                                deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                                            }
                                            else if (currentDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                                deliveryPreferenceType = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                                            }
                                            else {
                                                _this.currentLineLevelSelectedDeliveryPreference(CommerceProxy.Entities.DeliveryPreferenceType.None);
                                            }
                                        }
                                    }
                                    if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                        var currentDeliverySpecification = _this.currentLineDeliverySpecification().DeliverySpecification;
                                        currentDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                                        var tempAddress = Ecommerce.Utils.clone(currentDeliverySpecification.DeliveryAddress);
                                        _this.tempShippingAddress(tempAddress);
                                        var addressDropDownInitialized = false;
                                        if (_this.isAuthenticated() && !Ecommerce.Utils.isNullOrUndefined(currentDeliverySpecification.DeliveryAddress)) {
                                            for (var index in _this.storedCustomerAddresses()) {
                                                if (currentDeliverySpecification.DeliveryAddress.Name == _this.storedCustomerAddresses()[index].Value.Name &&
                                                    currentDeliverySpecification.DeliveryAddress.Street == _this.storedCustomerAddresses()[index].Value.Street &&
                                                    currentDeliverySpecification.DeliveryAddress.City == _this.storedCustomerAddresses()[index].Value.City &&
                                                    currentDeliverySpecification.DeliveryAddress.State == _this.storedCustomerAddresses()[index].Value.State &&
                                                    currentDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName == _this.storedCustomerAddresses()[index].Value.ThreeLetterISORegionName &&
                                                    currentDeliverySpecification.DeliveryAddress.ZipCode == _this.storedCustomerAddresses()[index].Value.ZipCode) {
                                                    _this.lineLevelSelectedAddress(_this.storedCustomerAddresses()[index].Value);
                                                    addressDropDownInitialized = true;
                                                }
                                            }
                                        }
                                        if (!addressDropDownInitialized) {
                                            _this.lineLevelSelectedAddress(null);
                                        }
                                        _this.itemDeliveryPreferenceToValidate(' .' + _this._itemDeliveryPreferencesFragments.ShipItemsItemLevel);
                                        _this.showItemDeliveryPreferenceFragment(_this._itemDeliveryPreferencesFragments.ShipItemsItemLevel);
                                    }
                                    else if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                        _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                                        _this.itemDeliveryPreferenceToValidate(' .' + _this._itemDeliveryPreferencesFragments.PickUpInStoreItemLevel);
                                        _this.showItemDeliveryPreferenceFragment(_this._itemDeliveryPreferencesFragments.PickUpInStoreItemLevel);
                                        _this._availableStoresView = _this._itemLevelDeliveryPreferenceSelection.find(" .msax-PickUpInStoreItemLevel .msax-AvailableStores");
                                        _this._location = _this._itemLevelDeliveryPreferenceSelection.find(" .msax-PickUpInStoreItemLevel input.msax-Location");
                                        _this._availableStoresView.hide();
                                        _this.map = _this._itemLevelDeliveryPreferenceSelection.find(" .msax-Map");
                                        _this.getMap();
                                    }
                                    else if (deliveryPreferenceType == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                                        _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery;
                                        _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                        _this.itemDeliveryPreferenceToValidate(' .' + _this._itemDeliveryPreferencesFragments.EmailItemLevel);
                                        _this.showItemDeliveryPreferenceFragment(_this._itemDeliveryPreferencesFragments.EmailItemLevel);
                                        var _sendEmailToMeCheckBox = _this._itemLevelDeliveryPreferenceSelection.find('.msax-SendEmailToMe');
                                        _this._emailAddressTextBox = _this._itemLevelDeliveryPreferenceSelection.find('.msax-EmailItemLevel .msax-EmailTextBox');
                                        if (_this._emailAddressTextBox.val() == _this.recepientEmailAddress) {
                                            _this.sendEmailToMe(true);
                                        }
                                        else {
                                            _this.sendEmailToMe(false);
                                        }
                                        _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = _this.emailDeliveryModeCode;
                                    }
                                    else {
                                        _this.itemDeliveryPreferenceToValidate('');
                                        _this.showItemDeliveryPreferenceFragment('');
                                    }
                                }, this);
                                this.orderLevelSelectedAddress.subscribe(function (newValue) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(newValue)) {
                                        _this.tempShippingAddress(newValue);
                                        var element = {};
                                        element.id = "OrderAddressStreet";
                                        element.value = newValue.Street;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                        element.id = "OrderAddressCity";
                                        element.value = newValue.City;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                        element.id = "OrderAddressZipCode";
                                        element.value = newValue.ZipCode;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                        element.id = "OrderAddressState";
                                        element.value = newValue.State;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                        element.id = "OrderAddressCountry";
                                        element.value = newValue.ThreeLetterISORegionName;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                        element.id = "OrderAddressName";
                                        element.value = newValue.Name;
                                        _this.resetOrderAvailableDeliveryMethods(element);
                                    }
                                }, this);
                                this.lineLevelSelectedAddress.subscribe(function (newValue) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(newValue)) {
                                        _this.tempShippingAddress(newValue);
                                        var element = {};
                                        element.id = "ItemAddressStreet";
                                        element.value = newValue.Street;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                        element.id = "ItemAddressCity";
                                        element.value = newValue.City;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                        element.id = "ItemAddressZipCode";
                                        element.value = newValue.ZipCode;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                        element.id = "ItemAddressState";
                                        element.value = newValue.State;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                        element.id = "ItemAddressCountry";
                                        element.value = newValue.ThreeLetterISORegionName;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                        element.id = "ItemAddressName";
                                        element.value = newValue.Name;
                                        _this.resetItemAvailableDeliveryMethods(element);
                                    }
                                }, this);
                                this.isBillingAddressSameAsShippingAddress.subscribe(function (isValueSet) {
                                    var paymentCardAddress = _this.paymentCardAddress();
                                    var email = paymentCardAddress.Email;
                                    if (isValueSet && !Ecommerce.Utils.isNullOrUndefined(_this.cart().ShippingAddress)) {
                                        paymentCardAddress = _this.cart().ShippingAddress;
                                        _this.getStateProvinceInfoService(paymentCardAddress.ThreeLetterISORegionName);
                                    }
                                    else {
                                        paymentCardAddress = new CommerceProxy.Entities.AddressClass(null);
                                        _this.states(null);
                                    }
                                    _this.paymentCardAddress(paymentCardAddress);
                                    return isValueSet;
                                }, this);
                                this.sendEmailToMe.subscribe(function (isSendEmailToMeSet) {
                                    if (isSendEmailToMeSet) {
                                        if (Ecommerce.Utils.isNullOrWhiteSpace(_this.recepientEmailAddress)) {
                                            _this.showError([Controls.Resources.String_119], true);
                                        }
                                        if (_this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                                            _this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailAddress = _this.recepientEmailAddress;
                                            _this.currentLineDeliverySpecification(_this.currentLineDeliverySpecification());
                                        }
                                        else if (_this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                                            _this.latestHeaderLevelDeliverySpecification().ElectronicDeliveryEmailAddress = _this.recepientEmailAddress;
                                            _this.latestHeaderLevelDeliverySpecification(_this.latestHeaderLevelDeliverySpecification());
                                        }
                                    }
                                    return isSendEmailToMeSet;
                                }, this);
                                this.maskedCreditCard = ko.computed(function () {
                                    var ccNumber = '';
                                    if (!Ecommerce.Utils.isNullOrUndefined(_this.paymentCard())) {
                                        var cardNumber = _this.paymentCard().CardNumber;
                                        if (!Ecommerce.Utils.isNullOrUndefined(cardNumber)) {
                                            var ccLength = cardNumber.length;
                                            if (ccLength > 4) {
                                                for (var i = 0; i < ccLength - 4; i++) {
                                                    ccNumber += '*';
                                                    if ((i + 1) % 4 == 0) {
                                                        ccNumber += '-';
                                                    }
                                                }
                                                ccNumber += cardNumber.substring(ccLength - 4, ccLength);
                                            }
                                        }
                                    }
                                    return ccNumber;
                                });
                            }
                            Checkout.prototype.loadXMLDoc = function (filename) {
                                var xhttp;
                                if (XMLHttpRequest) {
                                    xhttp = new XMLHttpRequest();
                                }
                                else {
                                    xhttp = new ActiveXObject("Microsoft.XMLHTTP");
                                }
                                xhttp.open("GET", filename, false);
                                xhttp.send();
                                return xhttp.responseXML;
                            };
                            Checkout.prototype.checkIfCurrentLineLevelDeliveryMode = function (selectedLineDeliveryOption, valueToCheck) {
                                var result = false;
                                if (!Ecommerce.Utils.isNullOrUndefined(selectedLineDeliveryOption) && !Ecommerce.Utils.isNullOrUndefined(selectedLineDeliveryOption.DeliverySpecification)) {
                                    if (selectedLineDeliveryOption.DeliverySpecification.DeliveryModeId == valueToCheck) {
                                        result = true;
                                    }
                                }
                                return result;
                            };
                            Checkout.prototype.lineLevelDeliveryOptionClick = function (selectedDeliveryModeId) {
                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = selectedDeliveryModeId;
                                return true;
                            };
                            Checkout.prototype.autoFillCheckout = function () {
                                if (Ecommerce.Utils.isNullOrEmpty(msaxValues.msax_DemoDataPath)) {
                                    return;
                                }
                                var xmlDoc = this.loadXMLDoc(msaxValues.msax_DemoDataPath);
                                var address = xmlDoc.getElementsByTagName("Address");
                                var country = address[0].getElementsByTagName("Country");
                                var name = address[0].getElementsByTagName("Name");
                                var street = address[0].getElementsByTagName("Street");
                                var city = address[0].getElementsByTagName("City");
                                var state = address[0].getElementsByTagName("State");
                                var zipcode = address[0].getElementsByTagName("Zipcode");
                                var email = xmlDoc.getElementsByTagName("Email");
                                var payment = xmlDoc.getElementsByTagName("Payment");
                                var cardNumber = payment[0].getElementsByTagName("CardNumber");
                                var ccid = payment[0].getElementsByTagName("CCID");
                                var tempAddress = new CommerceProxy.Entities.AddressClass(null);
                                tempAddress.Name = name[0].textContent;
                                tempAddress.ThreeLetterISORegionName = country[0].textContent;
                                tempAddress.Street = street[0].textContent;
                                tempAddress.City = city[0].textContent;
                                tempAddress.State = state[0].textContent;
                                tempAddress.ZipCode = zipcode[0].textContent;
                                tempAddress.Email = email[0].textContent;
                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress = tempAddress;
                                this.latestHeaderLevelDeliverySpecification(this.latestHeaderLevelDeliverySpecification());
                                var paymentCard = this.paymentCard();
                                this.confirmEmailValue(email[0].textContent);
                                paymentCard.NameOnCard = name[0].textContent;
                                paymentCard.CardNumber = cardNumber[0].textContent;
                                paymentCard.CCID = ccid[0].textContent;
                                this.paymentCard(paymentCard);
                                this.paymentCardAddress(tempAddress);
                            };
                            Checkout.prototype.showCheckoutFragment = function (fragmentCssClass) {
                                var allFragments = this._checkoutView.find("> div:not(' .msax-ProgressBar, .msax-Loading')");
                                allFragments.hide();
                                var fragmentToShow = this._checkoutView.find(" > ." + fragmentCssClass);
                                fragmentToShow.show();
                                var _progressBar = this._checkoutView.find(" > .msax-ProgressBar");
                                var _delivery = _progressBar.find(" > .msax-DeliveryProgress");
                                var _payment = _progressBar.find(" > .msax-PaymentProgress");
                                var _review = _progressBar.find(" > .msax-ReviewProgress");
                                var _progressBarEnd = _progressBar.find(" > .msax-ProgressBarEnd");
                                switch (fragmentCssClass) {
                                    case this._checkoutFragments.DeliveryPreferences:
                                        _delivery.addClass("msax-Active");
                                        if (_payment.hasClass("msax-Active")) {
                                            _payment.removeClass("msax-Active");
                                        }
                                        if (_review.hasClass("msax-Active")) {
                                            _review.removeClass("msax-Active");
                                        }
                                        if (_progressBarEnd.hasClass("msax-Active")) {
                                            _progressBarEnd.removeClass("msax-Active");
                                        }
                                        break;
                                    case this._checkoutFragments.PaymentInformation:
                                        _delivery.addClass("msax-Active");
                                        _payment.addClass("msax-Active");
                                        if (_review.hasClass("msax-Active")) {
                                            _review.removeClass("msax-Active");
                                        }
                                        if (_progressBarEnd.hasClass("msax-Active")) {
                                            _progressBarEnd.removeClass("msax-Active");
                                        }
                                        break;
                                    case this._checkoutFragments.Review:
                                        _delivery.addClass("msax-Active");
                                        _payment.addClass("msax-Active");
                                        _review.addClass("msax-Active");
                                        if (_progressBarEnd.hasClass("msax-Active")) {
                                            _progressBarEnd.removeClass("msax-Active");
                                        }
                                        break;
                                    case this._checkoutFragments.Confirmation:
                                        _delivery.addClass("msax-Active");
                                        _payment.addClass("msax-Active");
                                        _review.addClass("msax-Active");
                                        _progressBarEnd.addClass("msax-Active");
                                        break;
                                }
                            };
                            Checkout.prototype.showDeliveryPreferenceFragment = function (fragmentCssClass) {
                                var allFragments = this._deliveryPreferencesView.find(" .msax-DeliveryPreferenceOption");
                                allFragments.hide();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(fragmentCssClass)) {
                                    var fragmentToShow = this._deliveryPreferencesView.find(" ." + fragmentCssClass);
                                    fragmentToShow.show();
                                }
                            };
                            Checkout.prototype.showItemDeliveryPreferenceFragment = function (fragmentCssClass) {
                                var allFragments = this._itemLevelDeliveryPreferenceSelection.find(" .msax-DeliveryPreferenceOption");
                                allFragments.hide();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(fragmentCssClass)) {
                                    var fragmentToShow = this._itemLevelDeliveryPreferenceSelection.find(" ." + fragmentCssClass);
                                    fragmentToShow.show();
                                }
                            };
                            Checkout.prototype.validateItemDeliveryInformation = function () {
                                if (Ecommerce.Utils.isNullOrWhiteSpace(this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId)) {
                                    if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                        this.showError([Ecommerce.Utils.format(Controls.Resources.String_114)], false);
                                    }
                                    else if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                        this.showError([Ecommerce.Utils.format(Controls.Resources.String_61)], false);
                                    }
                                    else if (this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.None) {
                                        this.showError([Controls.Resources.String_158], false);
                                    }
                                    return false;
                                }
                                this.hideError();
                                return true;
                            };
                            Checkout.prototype.validateDeliveryInformation = function ($shippingOptions) {
                                if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore && Ecommerce.Utils.isNullOrWhiteSpace(this.latestHeaderLevelDeliverySpecification().DeliveryModeId)) {
                                    this.showError([Controls.Resources.String_114], false);
                                    return false;
                                }
                                else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress && Ecommerce.Utils.isNullOrWhiteSpace(this.latestHeaderLevelDeliverySpecification().DeliveryModeId)) {
                                    this.showError([Controls.Resources.String_61], false);
                                    return false;
                                }
                                else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                                    for (var i = 0; i < this.cart().CartLines.length; i++) {
                                        var cartLine = this.cart().CartLines[i];
                                        var currentLineDeliverySpecification = this.selectedDeliveryOptionByLineIdMap[cartLine.LineId];
                                        if (Ecommerce.Utils.isNullOrWhiteSpace(currentLineDeliverySpecification.DeliveryModeId)) {
                                            if (currentLineDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                                this.showError([Ecommerce.Utils.format(Controls.Resources.String_114 + Controls.Resources.String_125, cartLine.ProductId)], false);
                                            }
                                            else if (currentLineDeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                                this.showError([Ecommerce.Utils.format(Controls.Resources.String_61 + Controls.Resources.String_125, cartLine.ProductId)], false);
                                            }
                                            else {
                                                this.showError([Ecommerce.Utils.format(Controls.Resources.String_126, cartLine.ProductId)], false);
                                            }
                                            return false;
                                        }
                                    }
                                }
                                else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.None) {
                                    this.showError([Controls.Resources.String_158], false);
                                    return false;
                                }
                                this.hideError();
                                return true;
                            };
                            Checkout.prototype.deliveryPreferencesNextClick = function () {
                                this.states(null);
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                this._initPaymentEntitySetCompleted = 0;
                                this._initPaymentEntitySetFailed = 0;
                                this._initPaymentEntityErrors = [];
                                switch (this.selectedOrderDeliveryPreference()) {
                                    case CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually:
                                        var selectedLineDeliveryOptions;
                                        selectedLineDeliveryOptions = this.getLatestLineLevelDeliverySpecifications();
                                        this.setLineLevelDeliveryOptions(selectedLineDeliveryOptions);
                                        this.useShippingAddressForBilling(false);
                                        break;
                                    case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress:
                                        this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                                        this.useShippingAddressForBilling(true);
                                        break;
                                    case CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery:
                                        this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                                        this.useShippingAddressForBilling(false);
                                        break;
                                    default:
                                        this.setHeaderLevelDeliveryOptions(this.latestHeaderLevelDeliverySpecification());
                                        this.useShippingAddressForBilling(false);
                                        break;
                                }
                                if (this.isAuthenticated()) {
                                    this.getLoyaltyCards();
                                    this.paymentCardAddress().Email = this.recepientEmailAddress;
                                    this.paymentCardAddress(this.paymentCardAddress());
                                    this.confirmEmailValue(this.recepientEmailAddress);
                                }
                                else {
                                    var _customLoyaltyRadio = this._paymentView.find("#CustomLoyaltyRadio");
                                    _customLoyaltyRadio.hide();
                                    this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.LoyaltyCards);
                                }
                                this.updatePaymentValidations();
                            };
                            Checkout.prototype.paymentInformationPreviousClick = function () {
                                this.showCheckoutFragment(this._checkoutFragments.DeliveryPreferences);
                            };
                            Checkout.prototype.getLatestLineLevelDeliverySpecifications = function () {
                                var latestLineLevelDeliverySpecifications = [];
                                for (var index in this.selectedDeliveryOptionByLineIdMap) {
                                    if (!(Ecommerce.Utils.isNullOrUndefined(this.selectedDeliveryOptionByLineIdMap[index]))) {
                                        var selectedLineDeliveryOption = new CommerceProxy.Entities.LineDeliverySpecificationClass();
                                        selectedLineDeliveryOption.LineId = index;
                                        selectedLineDeliveryOption.DeliverySpecification = this.selectedDeliveryOptionByLineIdMap[index];
                                        latestLineLevelDeliverySpecifications.push(selectedLineDeliveryOption);
                                    }
                                }
                                return latestLineLevelDeliverySpecifications;
                            };
                            Checkout.prototype.getOrderLevelDeliveryAddressHeaderText = function () {
                                var headerText = null;
                                var selectedOrderLevelDeliveryPreference = this.selectedOrderDeliveryPreference();
                                switch (selectedOrderLevelDeliveryPreference) {
                                    case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress:
                                        headerText = Controls.Resources.String_18;
                                        break;
                                    case CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore:
                                        headerText = Controls.Resources.String_115;
                                        break;
                                    default:
                                }
                                return headerText;
                            };
                            Checkout.prototype.validateConfirmEmailTextBox = function (srcElement) {
                                var $element = $(srcElement);
                                var value = $element.val();
                                if (value !== this.paymentCardAddress().Email) {
                                    this.showError([Controls.Resources.String_62], false);
                                    return false;
                                }
                                this.hideError();
                                return true;
                            };
                            Checkout.prototype.updatePayments = function () {
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                this.tenderLines = [];
                                this.validatePayments();
                            };
                            Checkout.prototype.reviewPreviousClick = function () {
                                if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                    this.useShippingAddressForBilling(true);
                                }
                                else {
                                    this.useShippingAddressForBilling(false);
                                }
                                this.showCheckoutFragment(this._checkoutFragments.PaymentInformation);
                                this.updatePaymentValidations();
                            };
                            Checkout.prototype.updatePaymentValidations = function () {
                                if (!this.isCardPaymentAcceptPage() && this.payCreditCard()) {
                                    this.addValidation(this._creditCardPanel);
                                }
                                else {
                                    this.removeValidation(this._creditCardPanel);
                                }
                                if (this.payGiftCard()) {
                                    this.addValidation(this._giftCardPanel);
                                }
                                else {
                                    this.removeValidation(this._giftCardPanel);
                                }
                                if (this.payLoyaltyCard()) {
                                    this.addValidation(this._loyaltyCardPanel);
                                    if (this.isAuthenticated()) {
                                        this.removeValidation(this._paymentView.find('#LoyaltyCustomCard'));
                                    }
                                }
                                else {
                                    this.removeValidation(this._loyaltyCardPanel);
                                }
                            };
                            Checkout.prototype.quantityMinusClick = function (cartLine) {
                                if (cartLine.Quantity == 1) {
                                    this.removeFromCartClick(cartLine);
                                }
                                else {
                                    cartLine.Quantity = cartLine.Quantity - 1;
                                    this.updateQuantity([cartLine]);
                                }
                            };
                            Checkout.prototype.quantityPlusClick = function (cartLine) {
                                cartLine.Quantity = cartLine.Quantity + 1;
                                this.updateQuantity([cartLine]);
                            };
                            Checkout.prototype.quantityTextBoxChanged = function (cartLine, valueAccesor) {
                                var srcElement = valueAccesor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    if (Ecommerce.Utils.isNullOrWhiteSpace(srcElement.value)) {
                                        srcElement.value = cartLine.Quantity;
                                        return;
                                    }
                                    var enteredNumber = Number(srcElement.value);
                                    if (isNaN(enteredNumber)) {
                                        srcElement.value = cartLine.Quantity;
                                        return;
                                    }
                                    if (enteredNumber != cartLine.Quantity) {
                                        cartLine.Quantity = enteredNumber;
                                        if (cartLine.Quantity < 0) {
                                            cartLine.Quantity = 1;
                                        }
                                        if (cartLine.Quantity == 0) {
                                            this.removeFromCartClick(cartLine);
                                        }
                                        else {
                                            this.updateQuantity([cartLine]);
                                        }
                                    }
                                }
                            };
                            Checkout.prototype.resetSelectedOrderShippingOptions = function () {
                                this.availableDeliveryOptions(null);
                                this.latestHeaderLevelDeliverySpecification().DeliveryModeId = "";
                            };
                            Checkout.prototype.initEntitySetCallSuccessful = function (entity) {
                                this.actionOnInitEntitySetCompletion(entity);
                            };
                            Checkout.prototype.initEntitySetCallFailed = function (entity, errors) {
                                this._initEntitySetFailed = this._initEntitySetFailed | entity;
                                this._initEntityErrors = this._initEntityErrors.concat(errors);
                                this.actionOnInitEntitySetCompletion(entity);
                            };
                            Checkout.prototype.actionOnInitEntitySetCompletion = function (entity) {
                                if (entity === InitEntitySet.None) {
                                    CommerceProxy.RetailLogger.initEntitySetInvalidError(InitEntitySet[entity]);
                                }
                                else if ((this._initEntitySetCompleted & entity) === entity) {
                                    CommerceProxy.RetailLogger.initEntitySetMultipleTimesError(InitEntitySet[entity]);
                                }
                                this._initEntitySetCompleted = this._initEntitySetCompleted | entity;
                                if (this._initEntitySetCompleted === InitEntitySet.All) {
                                    if (this._initEntitySetFailed !== InitEntitySet.None) {
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(this._initEntityErrors);
                                        this.closeDialogAndDisplayError(errorMessages, true);
                                    }
                                    else {
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                    }
                                }
                                else if (this._initEntitySetCompleted > InitEntitySet.All) {
                                    CommerceProxy.RetailLogger.initEntitySetNoMethodNumberError();
                                }
                            };
                            Checkout.prototype.initPaymentEntitySetCallSuccessful = function (entity) {
                                this.actionOnInitPaymentEntitySetCompletion(entity);
                            };
                            Checkout.prototype.initPaymentEntitySetCallFailed = function (entity, errors) {
                                this._initPaymentEntitySetFailed = this._initPaymentEntitySetFailed | entity;
                                this._initPaymentEntityErrors = this._initPaymentEntityErrors.concat(errors);
                                this.actionOnInitPaymentEntitySetCompletion(entity);
                            };
                            Checkout.prototype.actionOnInitPaymentEntitySetCompletion = function (entity) {
                                if (entity === InitPaymentEntitySet.None) {
                                    CommerceProxy.RetailLogger.initPaymentEntitySetInvalidError(InitPaymentEntitySet[entity]);
                                }
                                else if ((this._initPaymentEntitySetCompleted & entity) === entity) {
                                    CommerceProxy.RetailLogger.initPaymentEntitySetMultipleTimesError(InitPaymentEntitySet[entity]);
                                }
                                this._initPaymentEntitySetCompleted = this._initPaymentEntitySetCompleted | entity;
                                if (this._initPaymentEntitySetCompleted === InitPaymentEntitySet.All) {
                                    if (this._initPaymentEntitySetFailed !== InitPaymentEntitySet.None) {
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(this._initPaymentEntityErrors);
                                        this.closeDialogAndDisplayError(errorMessages, true);
                                    }
                                    else {
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                    }
                                }
                                else if (this._initPaymentEntitySetCompleted > InitPaymentEntitySet.All) {
                                    CommerceProxy.RetailLogger.initPaymentEntitySetNoMethodNumberError();
                                }
                            };
                            Checkout.prototype.closeDialogAndDisplayError = function (errorMessages, isError) {
                                Controls.LoadingOverlay.CloseLoadingDialog();
                                this.showError(errorMessages, isError);
                            };
                            Checkout.prototype.showError = function (errorMessages, isError) {
                                this.errorMessages(errorMessages);
                                if (isError) {
                                    this.errorPanel.addClass("msax-Error");
                                }
                                else if (this.errorPanel.hasClass("msax-Error")) {
                                    this.errorPanel.removeClass("msax-Error");
                                }
                                this.errorPanel.show();
                                $(window).scrollTop(0);
                            };
                            Checkout.prototype.hideError = function () {
                                this.errorPanel.hide();
                            };
                            Checkout.prototype.formatCurrencyString = function (amount) {
                                if (isNaN(amount)) {
                                    return amount;
                                }
                                var formattedCurrencyString = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(amount)) {
                                    if (Ecommerce.Utils.isNullOrUndefined(this.currencyStringTemplate)) {
                                        formattedCurrencyString = amount.toString();
                                    }
                                    else {
                                        formattedCurrencyString = Ecommerce.Utils.format(this.currencyStringTemplate, Ecommerce.Utils.formatNumber(amount));
                                    }
                                }
                                return formattedCurrencyString;
                            };
                            Checkout.prototype.formatProductAvailabilityString = function (availableCount) {
                                if (Ecommerce.Utils.isNullOrUndefined(availableCount) || isNaN(availableCount)) {
                                    availableCount = 0;
                                }
                                var formattedProductAvailabilityString = '[' + availableCount + ']';
                                return formattedProductAvailabilityString;
                            };
                            Checkout.prototype.formatDistance = function (distance) {
                                return Ecommerce.Utils.formatNumber(distance);
                            };
                            Checkout.prototype.getResx = function (key) {
                                return Controls.Resources[key];
                            };
                            Checkout.prototype.getDeliveryModeText = function (deliveryModeId) {
                                var deliveryModeText = "";
                                if (!Ecommerce.Utils.isNullOrUndefined(this.allDeliveryOptionDescriptions)) {
                                    for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                                        if (this.allDeliveryOptionDescriptions[i].Code == deliveryModeId) {
                                            deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                                            break;
                                        }
                                    }
                                }
                                return deliveryModeText;
                            };
                            Checkout.prototype.getDeliverySpecificationForCartLine = function (cartLine) {
                                var selectedDeliveryOptionForLine = this.selectedDeliveryOptionByLineIdMap[cartLine.LineId];
                                if (Ecommerce.Utils.isNullOrUndefined(selectedDeliveryOptionForLine)) {
                                    selectedDeliveryOptionForLine = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                    selectedDeliveryOptionForLine.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
                                    selectedDeliveryOptionForLine.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                }
                                return selectedDeliveryOptionForLine;
                            };
                            Checkout.prototype.getLineLevelDeliveryModeDescription = function (cartLine) {
                                var deliveryModeText = null;
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(cartLine.DeliveryMode)) {
                                    for (var i = 0; i < this.allDeliveryOptionDescriptions.length; i++) {
                                        if (this.allDeliveryOptionDescriptions[i].Code == cartLine.DeliveryMode) {
                                            deliveryModeText = this.allDeliveryOptionDescriptions[i].Description;
                                        }
                                    }
                                }
                                return deliveryModeText;
                            };
                            Checkout.prototype.getMap = function () {
                                if (this.mapStoreLocator) {
                                    this.mapStoreLocator.dispose();
                                }
                                this.mapStoreLocator = new Microsoft.Maps.Map(this.map[0], { credentials: this.bingMapsToken, zoom: 1, disableTouchInput: this.DisableTouchInputOnMap });
                                Microsoft.Maps.loadModule('Microsoft.Maps.Search');
                            };
                            Checkout.prototype.getNearbyStoresWithAvailability = function () {
                                if (!Ecommerce.Utils.isNullOrUndefined(this._location) && !Ecommerce.Utils.isNullOrWhiteSpace(this._location.val())) {
                                    this.resetSelectedOrderShippingOptions();
                                    this.getMap();
                                    var searchManager = new Microsoft.Maps.Search.SearchManager(this.mapStoreLocator);
                                    var geocodeRequest = { where: this._location.val(), count: 1, callback: this.geocodeCallback.bind(this), errorCallback: this.geocodeErrorCallback.bind(this) };
                                    searchManager.geocode(geocodeRequest);
                                }
                            };
                            Checkout.prototype.geocodeCallback = function (geocodeResult, userData) {
                                if (!geocodeResult.results[0]) {
                                    this.showError([Controls.Resources.String_109], false);
                                    return;
                                }
                                this.searchLocation = geocodeResult.results[0].location;
                                this.mapStoreLocator.setView({ zoom: 11, center: this.searchLocation });
                                Microsoft.Maps.Events.addHandler(this.mapStoreLocator, 'viewchanged', this.renderAvailableStores.bind(this));
                                if (this.hasInventoryCheck()) {
                                    this.getNearbyStoresWithAvailabilityService();
                                }
                                else {
                                    this.getNearbyStoresService();
                                }
                            };
                            Checkout.prototype.geocodeErrorCallback = function (geocodeRequest) {
                                this.showError([Controls.Resources.String_110], true);
                            };
                            Checkout.prototype.renderAvailableStores = function () {
                                this.mapStoreLocator.entities.clear();
                                this._availableStoresView.hide();
                                this.displayLocations(null);
                                var storeCount = 0;
                                var pin;
                                var pinInfoBox;
                                var mapBounds = this.mapStoreLocator.getBounds();
                                var displayLocations = [];
                                if (!Ecommerce.Utils.isNullOrUndefined(this.searchLocation) && mapBounds.contains(this.searchLocation)) {
                                    pin = new Microsoft.Maps.Pushpin(this.searchLocation, { draggable: false, text: "X" });
                                    this.mapStoreLocator.entities.push(pin);
                                }
                                if (!Ecommerce.Utils.isNullOrEmpty(this.orgUnitLocations)) {
                                    for (var i = 0; i < this.orgUnitLocations.length; i++) {
                                        var currentStoreLocation = this.orgUnitLocations[i];
                                        var locationObj = { latitude: currentStoreLocation.Latitude, longitude: currentStoreLocation.Longitude };
                                        if (mapBounds.contains(locationObj)) {
                                            this._availableStoresView.show();
                                            storeCount++;
                                            currentStoreLocation['LocationCount'] = storeCount;
                                            displayLocations.push(currentStoreLocation);
                                            var storeAddressText = '<div style="width:80%;height:100%;"><p style="background-color:gray;color:black;margin-bottom:5px;"><span style="padding-right:45px;">Store</span><span style="font-weight:bold;">Distance</span><p><p style="margin-bottom:0px;margin-top:0px;"><span style="color:black;padding-right:35px;">' + currentStoreLocation.OrgUnitName + '</span><span style="color:black;">' + currentStoreLocation.Distance + ' miles</span></p><p style="margin-bottom:0px;margin-top:0px;">' + currentStoreLocation.Street + ' </p><p style="margin-bottom:0px;margin-top:0px;">' + currentStoreLocation.City + ', ' + currentStoreLocation.State + ' ' + currentStoreLocation.Zip + '</p></div>';
                                            pin = new Microsoft.Maps.Pushpin(locationObj, { draggable: false, text: "" + storeCount + "" });
                                            pinInfoBox = new Microsoft.Maps.Infobox(locationObj, { width: 225, offset: new Microsoft.Maps.Point(0, 10), showPointer: true, visible: false, description: storeAddressText });
                                            Microsoft.Maps.Events.addHandler(pin, 'click', (function (pinInfoBox) {
                                                return function () {
                                                    pinInfoBox.setOptions({ visible: true });
                                                };
                                            })(pinInfoBox));
                                            this.mapStoreLocator.entities.push(pin);
                                            this.mapStoreLocator.entities.push(pinInfoBox);
                                        }
                                    }
                                }
                                this.displayLocations(displayLocations);
                                if (displayLocations.length > 0) {
                                    this.selectStore(displayLocations[0]);
                                }
                            };
                            Checkout.prototype.selectStore = function (location) {
                                if (this.hasInventoryCheck() && !this.areAllReqProductsAvailableInOrgUnit(location.OrgUnitNumber)) {
                                    this.resetSelectedOrderShippingOptions();
                                    this.showError([Controls.Resources.String_113], false);
                                    this.nextButton.addClass("msax-Grey");
                                }
                                else {
                                    this.hideError();
                                    if (this.nextButton.hasClass("msax-Grey")) {
                                        this.nextButton.removeClass("msax-Grey");
                                    }
                                    if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                        var headerLevelDeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass();
                                        headerLevelDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                                        headerLevelDeliverySpecification.PickUpStoreId = location.OrgUnitNumber;
                                        headerLevelDeliverySpecification.DeliveryModeId = this.pickUpInStoreDeliveryModeCode;
                                        headerLevelDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
                                        headerLevelDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = location.Country;
                                        headerLevelDeliverySpecification.DeliveryAddress.ZipCode = location.Zip;
                                        headerLevelDeliverySpecification.DeliveryAddress.State = location.State;
                                        headerLevelDeliverySpecification.DeliveryAddress.City = location.City;
                                        headerLevelDeliverySpecification.DeliveryAddress.Street = location.Street;
                                        this.latestHeaderLevelDeliverySpecification(headerLevelDeliverySpecification);
                                    }
                                    else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                                        var currentLineDeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass();
                                        currentLineDeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore;
                                        currentLineDeliverySpecification.PickUpStoreId = location.OrgUnitNumber;
                                        currentLineDeliverySpecification.DeliveryModeId = this.pickUpInStoreDeliveryModeCode;
                                        currentLineDeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass();
                                        currentLineDeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = location.Country;
                                        currentLineDeliverySpecification.DeliveryAddress.ZipCode = location.Zip;
                                        currentLineDeliverySpecification.DeliveryAddress.State = location.State;
                                        currentLineDeliverySpecification.DeliveryAddress.City = location.City;
                                        currentLineDeliverySpecification.DeliveryAddress.Street = location.Street;
                                        this.currentLineDeliverySpecification().DeliverySpecification = currentLineDeliverySpecification;
                                        this.currentLineDeliverySpecification(this.currentLineDeliverySpecification());
                                    }
                                }
                                var _stores = this._availableStoresView.find(".msax-AvailableStore");
                                var selectedChannelId = location.ChannelId;
                                _stores.each(function (index, element) {
                                    if ($(element).hasClass("msax-Selected")) {
                                        $(element).removeClass("msax-Selected");
                                    }
                                    if (selectedChannelId == parseInt($(element).attr("channelId"))) {
                                        $(element).addClass("msax-Selected");
                                    }
                                });
                            };
                            Checkout.prototype.editRewardCardOverlayClick = function () {
                                this.dialogOverlay = $('.ui-widget-overlay');
                                this.dialogOverlay.on('click', $.proxy(this.closeEditRewardCardDialog, this));
                            };
                            Checkout.prototype.createEditRewardCardDialog = function () {
                                this._editRewardCardDialog.dialog({
                                    modal: true,
                                    title: Controls.Resources.String_186,
                                    autoOpen: false,
                                    draggable: true,
                                    resizable: false,
                                    closeOnEscape: true,
                                    show: { effect: "fadeIn", duration: 500 },
                                    hide: { effect: "fadeOut", duration: 500 },
                                    width: 500,
                                    height: 300,
                                    dialogClass: 'msax-Control'
                                });
                            };
                            Checkout.prototype.showEditRewardCardDialog = function () {
                                $('.ui-dialog-titlebar-close').on('click', $.proxy(this.closeEditRewardCardDialog, this));
                                this._editRewardCardDialog.dialog('open');
                                this.editRewardCardOverlayClick();
                            };
                            Checkout.prototype.closeEditRewardCardDialog = function () {
                                this._editRewardCardDialog.dialog('close');
                            };
                            Checkout.prototype.discountCodeOverlayClick = function () {
                                this.dialogOverlay = $('.ui-widget-overlay');
                                this.dialogOverlay.on('click', $.proxy(this.closeDiscountCodeDialog, this));
                            };
                            Checkout.prototype.createDiscountCodeDialog = function () {
                                this._addDiscountCodeDialog.dialog({
                                    modal: true,
                                    title: Controls.Resources.String_188,
                                    autoOpen: false,
                                    draggable: true,
                                    resizable: false,
                                    closeOnEscape: true,
                                    show: { effect: "fadeIn", duration: 500 },
                                    hide: { effect: "fadeOut", duration: 500 },
                                    width: 500,
                                    height: 300,
                                    dialogClass: 'msax-Control'
                                });
                            };
                            Checkout.prototype.showDiscountCodeDialog = function () {
                                $('.ui-dialog-titlebar-close').on('click', $.proxy(this.closeDiscountCodeDialog, this));
                                this._addDiscountCodeDialog.dialog('open');
                                this.discountCodeOverlayClick();
                            };
                            Checkout.prototype.closeDiscountCodeDialog = function () {
                                this._addDiscountCodeDialog.dialog('close');
                            };
                            Checkout.prototype.itemDeliveryPreferenceSelectionOverlayClick = function () {
                                this.dialogOverlay = $('.ui-widget-overlay');
                                this.dialogOverlay.on('click', $.proxy(this.closeItemDeliveryPreferenceSelection, this));
                            };
                            Checkout.prototype.createItemDeliveryPreferenceDialog = function () {
                                this._itemLevelDeliveryPreferenceSelection.dialog({
                                    modal: true,
                                    autoOpen: false,
                                    draggable: true,
                                    resizable: false,
                                    closeOnEscape: true,
                                    show: { effect: "fadeIn", duration: 500 },
                                    hide: { effect: "fadeOut", duration: 500 },
                                    width: 980,
                                    height: 700,
                                    dialogClass: 'msax-Control msax-NoTitle'
                                });
                            };
                            Checkout.prototype.getApplicableDeliveryPreferencesForCartLine = function (cartLine) {
                                var lineLevelDeliveryPreferences = [];
                                lineLevelDeliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Controls.Resources.String_159 });
                                if (!Ecommerce.Utils.isNullOrUndefined(this.cartDeliveryPreferences) && !Ecommerce.Utils.isNullOrUndefined(this.cartDeliveryPreferences.CartLineDeliveryPreferences)) {
                                    var deliveryPreferencesForAllLines = this.cartDeliveryPreferences.CartLineDeliveryPreferences;
                                    for (var i = 0; i < deliveryPreferencesForAllLines.length; i++) {
                                        if (deliveryPreferencesForAllLines[i].LineId == cartLine.LineId) {
                                            for (var j = 0; j < deliveryPreferencesForAllLines[i].DeliveryPreferenceTypeValues.length; j++) {
                                                var preferenceText = "";
                                                var currentDeliveryPreference = deliveryPreferencesForAllLines[i].DeliveryPreferenceTypeValues[j];
                                                switch (currentDeliveryPreference) {
                                                    case CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress:
                                                        preferenceText = Controls.Resources.String_99;
                                                        break;
                                                    case CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore:
                                                        preferenceText = Controls.Resources.String_100;
                                                        break;
                                                    case CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery:
                                                        preferenceText = Controls.Resources.String_58;
                                                        break;
                                                    default: throw "Not supported delivery preference type.";
                                                }
                                                lineLevelDeliveryPreferences.push({ Value: currentDeliveryPreference, Text: preferenceText });
                                            }
                                        }
                                    }
                                }
                                return lineLevelDeliveryPreferences;
                            };
                            Checkout.prototype.showLineLevelDeliveryPreferenceSelection = function (cartLine) {
                                var temp = new CommerceProxy.Entities.LineDeliverySpecificationClass(null);
                                temp.DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                temp.DeliverySpecification.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.None;
                                temp.DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                if (!(Ecommerce.Utils.isNullOrWhiteSpace(cartLine.DeliveryMode))) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cartLine.ShippingAddress)) {
                                        temp.DeliverySpecification.DeliveryAddress.Name = cartLine.ShippingAddress.Name;
                                        temp.DeliverySpecification.DeliveryAddress.Street = cartLine.ShippingAddress.Street;
                                        temp.DeliverySpecification.DeliveryAddress.City = cartLine.ShippingAddress.City;
                                        temp.DeliverySpecification.DeliveryAddress.State = cartLine.ShippingAddress.State;
                                        temp.DeliverySpecification.DeliveryAddress.ZipCode = cartLine.ShippingAddress.ZipCode;
                                        temp.DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = cartLine.ShippingAddress.ThreeLetterISORegionName;
                                    }
                                    temp.DeliverySpecification.ElectronicDeliveryEmailAddress = cartLine.ElectronicDeliveryEmail;
                                    temp.DeliverySpecification.ElectronicDeliveryEmailContent = cartLine.ElectronicDeliveryEmailContent;
                                }
                                this.currentLineDeliverySpecification(temp);
                                this.currentCartLine(cartLine);
                                this.hideError();
                                this.errorPanel = this._itemLevelDeliveryPreferenceSelection.find(" .msax-ErrorPanel");
                                this.currentLineLevelSelectedDeliveryPreference(temp.DeliverySpecification.DeliveryPreferenceTypeValue);
                                this._itemLevelDeliveryPreferenceSelection.dialog('open');
                                this.itemDeliveryPreferenceSelectionOverlayClick();
                            };
                            Checkout.getDeliveryPreferencesForLine = function (lineId, cartDeliveryPreferences) {
                                var lineDeliveryPreferences = cartDeliveryPreferences.CartLineDeliveryPreferences;
                                for (var i = 0; i < lineDeliveryPreferences.length; i++) {
                                    if (lineDeliveryPreferences[i].LineId == lineId) {
                                        return lineDeliveryPreferences[i].DeliveryPreferenceTypeValues;
                                    }
                                }
                                var msg = "No delivery preferences were found for line id" + lineId;
                                throw new Error(msg);
                            };
                            Checkout.prototype.paymentCountryUpdate = function (srcElement) {
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    this.paymentCardAddress().ThreeLetterISORegionName = srcElement.value;
                                    this.getStateProvinceInfoService(srcElement.value);
                                }
                                return true;
                            };
                            Checkout.prototype.resetOrderAvailableDeliveryMethods = function (srcElement) {
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    var id = srcElement.id;
                                    switch (id) {
                                        case "OrderAddressStreet":
                                            if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Street != srcElement.value) {
                                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Street = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                            }
                                            break;
                                        case "OrderAddressCity":
                                            if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.City != srcElement.value) {
                                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress.City = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                            }
                                            break;
                                        case "OrderAddressZipCode":
                                            if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ZipCode != srcElement.value) {
                                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ZipCode = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                            }
                                            break;
                                        case "OrderAddressState":
                                            if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State != srcElement.value) {
                                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                            }
                                            break;
                                        case "OrderAddressCountry":
                                            if (this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ThreeLetterISORegionName != srcElement.value) {
                                                this.latestHeaderLevelDeliverySpecification().DeliveryAddress.ThreeLetterISORegionName = srcElement.value;
                                                this.getStateProvinceInfoService(srcElement.value);
                                                this.resetSelectedOrderShippingOptions();
                                            }
                                            break;
                                        case "OrderAddressName":
                                            this.latestHeaderLevelDeliverySpecification().DeliveryAddress.Name = srcElement.value;
                                            break;
                                    }
                                }
                                return true;
                            };
                            Checkout.prototype.resetItemAvailableDeliveryMethods = function (srcElement) {
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    var id = srcElement.id;
                                    switch (id) {
                                        case "ItemAddressStreet":
                                            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Street != srcElement.value) {
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Street = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                                            }
                                            break;
                                        case "ItemAddressCity":
                                            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.City != srcElement.value) {
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.City = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                                            }
                                            break;
                                        case "ItemAddressZipCode":
                                            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ZipCode != srcElement.value) {
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ZipCode = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                                            }
                                            break;
                                        case "ItemAddressState":
                                            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State != srcElement.value) {
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State = srcElement.value;
                                                this.resetSelectedOrderShippingOptions();
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                                            }
                                            break;
                                        case "ItemAddressCountry":
                                            if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName != srcElement.value) {
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.ThreeLetterISORegionName = srcElement.value;
                                                this.getStateProvinceInfoService(srcElement.value);
                                                this.resetSelectedOrderShippingOptions();
                                                this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = null;
                                            }
                                            break;
                                        case "ItemAddressName":
                                            this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.Name = srcElement.value;
                                            break;
                                    }
                                }
                                return true;
                            };
                            Checkout.prototype.closeItemDeliveryPreferenceSelection = function () {
                                this.errorPanel = this._checkoutView.find(" > .msax-ErrorPanel");
                                this._itemLevelDeliveryPreferenceSelection.dialog('close');
                                this.cart(this.cart());
                            };
                            Checkout.prototype.setItemDeliveryPreferenceSelection = function () {
                                if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                    this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailAddress = null;
                                    this.currentLineDeliverySpecification().DeliverySpecification.ElectronicDeliveryEmailContent = null;
                                }
                                else if (this.currentLineDeliverySpecification().DeliverySpecification.DeliveryPreferenceTypeValue == CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery) {
                                    this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress = new CommerceProxy.Entities.AddressClass(null);
                                }
                                var latestLineDeliveryOption = this.currentLineDeliverySpecification();
                                var currentDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                currentDeliveryOption.DeliveryAddress = latestLineDeliveryOption.DeliverySpecification.DeliveryAddress;
                                currentDeliveryOption.DeliveryModeId = latestLineDeliveryOption.DeliverySpecification.DeliveryModeId;
                                ;
                                currentDeliveryOption.DeliveryPreferenceTypeValue = latestLineDeliveryOption.DeliverySpecification.DeliveryPreferenceTypeValue;
                                ;
                                currentDeliveryOption.ElectronicDeliveryEmailAddress = latestLineDeliveryOption.DeliverySpecification.ElectronicDeliveryEmailAddress;
                                currentDeliveryOption.ElectronicDeliveryEmailContent = latestLineDeliveryOption.DeliverySpecification.ElectronicDeliveryEmailContent;
                                currentDeliveryOption.PickUpStoreId = latestLineDeliveryOption.DeliverySpecification.PickUpStoreId;
                                this.selectedDeliveryOptionByLineIdMap[latestLineDeliveryOption.LineId] = currentDeliveryOption;
                                this.closeItemDeliveryPreferenceSelection();
                            };
                            Checkout.prototype.findLocationKeyPress = function (data, event) {
                                if (event.keyCode == 8 || event.keyCode == 27) {
                                    event.preventDefault();
                                    return false;
                                }
                                else if (event.keyCode == 13) {
                                    this.getNearbyStoresWithAvailability();
                                    return false;
                                }
                                return true;
                            };
                            Checkout.prototype.removeValidation = function (element) {
                                $(element).find(":input").each(function (idx, element) {
                                    $(element).removeAttr('required');
                                });
                            };
                            Checkout.prototype.addValidation = function (element) {
                                $(element).find(":input").each(function (idx, element) {
                                    $(element).attr('required', true);
                                });
                            };
                            Checkout.prototype.updateCustomLoyaltyValidation = function () {
                                if (this._paymentView.find('#CustomLoyaltyRadio').is(':checked')) {
                                    this.addValidation(this._paymentView.find('#LoyaltyCustomCard'));
                                }
                                return true;
                            };
                            Checkout.prototype.checkForGiftCardInCart = function (cart) {
                                var isGiftCardPresent = false;
                                var cartLines = cart.CartLines;
                                for (var i = 0; i < cartLines.length; i++) {
                                    if (cartLines[i].ItemId == this.giftCardItemId) {
                                        isGiftCardPresent = true;
                                    }
                                }
                                return isGiftCardPresent;
                            };
                            Checkout.prototype.updateHeaderLevelDeliveryPreferences = function (deliveryPreferenceTypeValues) {
                                var headerLevelDeliveryPreferenceTypes = this.cartDeliveryPreferences.HeaderDeliveryPreferenceTypeValues;
                                var hasShipToAddress = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress);
                                var hasPickUpInStore = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore);
                                var hasEmail = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery);
                                var hasMultiDeliveryPreference = Checkout.isRequestedDeliveryPreferenceApplicable(headerLevelDeliveryPreferenceTypes, CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually);
                                var deliveryPreferences = [];
                                deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.None, Text: Controls.Resources.String_159 });
                                if (hasShipToAddress) {
                                    deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress, Text: Controls.Resources.String_99 });
                                }
                                if (hasPickUpInStore) {
                                    deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore, Text: Controls.Resources.String_100 });
                                }
                                if (hasEmail) {
                                    deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.ElectronicDelivery, Text: Controls.Resources.String_58 });
                                }
                                if (hasMultiDeliveryPreference) {
                                    deliveryPreferences.push({ Value: CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually, Text: Controls.Resources.String_101 });
                                }
                                this.allowedHeaderLevelDeliveryPreferences(deliveryPreferences);
                            };
                            Checkout.prototype.showPaymentPanel = function (data, valueAccessor) {
                                var srcElement = valueAccessor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    if ($(srcElement).hasClass('msax-PayCreditCardLink')) {
                                        this.payCreditCard(true);
                                        this.getCardPaymentAcceptUrl();
                                    }
                                    else if ($(srcElement).hasClass('msax-PayGiftCardLink')) {
                                        this._giftCardPanel.show();
                                        this.addValidation(this._giftCardPanel);
                                        this.payGiftCard(true);
                                    }
                                    else if ($(srcElement).hasClass('msax-PayLoyaltyCardLink')) {
                                        this._loyaltyCardPanel.show();
                                        this.addValidation(this._loyaltyCardPanel);
                                        if (this.isAuthenticated()) {
                                            this.removeValidation(this._paymentView.find('#LoyaltyCustomCard'));
                                        }
                                        this.payLoyaltyCard(true);
                                    }
                                    $(srcElement).hide();
                                    this.updatePaymentTotal();
                                }
                            };
                            Checkout.prototype.hidePaymentPanel = function (data, valueAccessor) {
                                var srcElement = valueAccessor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)) {
                                    if ($(srcElement.parentElement).hasClass('msax-CreditCardDetails')) {
                                        this._creditCardPanel.hide();
                                        this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                                        this.removeValidation(this._creditCardPanel);
                                        this.payCreditCard(false);
                                        this.tokenizedCartTenderLine = null;
                                    }
                                    else if ($(srcElement.parentElement).hasClass('msax-GiftCard')) {
                                        this._giftCardPanel.hide();
                                        this._paymentView.find('.msax-PayGiftCard .msax-PayGiftCardLink').show();
                                        this.removeValidation(this._giftCardPanel);
                                        this.payGiftCard(false);
                                    }
                                    else if ($(srcElement.parentElement).hasClass('msax-LoyaltyCard')) {
                                        this._loyaltyCardPanel.hide();
                                        this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                                        this.removeValidation(this._loyaltyCardPanel);
                                        this.payLoyaltyCard(false);
                                    }
                                    this.updatePaymentTotal();
                                }
                            };
                            Checkout.prototype.updatePaymentTotal = function () {
                                this.creditCardAmount = 0;
                                this.giftCardAmount = 0;
                                this.loyaltyCardAmount = 0;
                                if (this.payGiftCard()) {
                                    this.giftCardAmount = Ecommerce.Utils.parseNumberFromLocaleString(this.formattedGiftCardAmount());
                                    this.formattedGiftCardAmount(this.formatCurrencyString(this.giftCardAmount));
                                }
                                if (this.payLoyaltyCard()) {
                                    this.loyaltyCardAmount = Ecommerce.Utils.parseNumberFromLocaleString(this.formattedLoyaltyCardAmount());
                                    this.formattedLoyaltyCardAmount(this.formatCurrencyString(this.loyaltyCardAmount));
                                }
                                if (this.payCreditCard()) {
                                    this.creditCardAmount = Ecommerce.Utils.roundToNDigits(this.cart().TotalAmount - this.giftCardAmount - this.loyaltyCardAmount, 3);
                                    if (isNaN(this.creditCardAmount) || (this.creditCardAmount < 0)) {
                                        this.creditCardAmount = 0;
                                    }
                                    this.formattedCreditCardAmount(this.formatCurrencyString(this.creditCardAmount));
                                }
                                this.totalAmount = Number(this.creditCardAmount + this.giftCardAmount + this.loyaltyCardAmount);
                                if (isNaN(this.totalAmount)) {
                                    this.totalAmount = 0;
                                }
                                this.formattedPaymentTotal(this.formatCurrencyString(this.totalAmount));
                                return true;
                            };
                            Checkout.prototype.validatePayments = function () {
                                this.updatePaymentTotal();
                                if (!this.payCreditCard() && !this.payGiftCard() && !this.payLoyaltyCard()) {
                                    this.closeDialogAndDisplayError([Controls.Resources.String_139], false);
                                    return;
                                }
                                if (!this.isCardPaymentAcceptPage() && this.payCreditCard()) {
                                    var selectedYear = this.paymentCard().ExpirationYear;
                                    var selectedMonth = this.paymentCard().ExpirationMonth;
                                    var currentTime = new Date();
                                    var currentMonth = currentTime.getMonth() + 1;
                                    var currentYear = currentTime.getFullYear();
                                    if (selectedYear < currentYear || selectedYear == currentYear && selectedMonth < currentMonth) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_140], false);
                                        return;
                                    }
                                }
                                if (this.payLoyaltyCard()) {
                                    if (this.loyaltyCardAmount == 0) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_152], false);
                                        return;
                                    }
                                    if (this.loyaltyCardAmount > this.cart().TotalAmount) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_153], false);
                                        return;
                                    }
                                }
                                if (this.payGiftCard()) {
                                    if (Ecommerce.Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_144], false);
                                        return;
                                    }
                                    if (this.giftCardAmount == 0) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_146], false);
                                        return;
                                    }
                                    if (this.giftCardAmount > this.cart().TotalAmount) {
                                        this.closeDialogAndDisplayError([Controls.Resources.String_147], false);
                                        return;
                                    }
                                    this.checkGiftCardAmountValidity = true;
                                    this.getGiftCardBalance();
                                }
                                else {
                                    this.createPaymentCardTenderLine();
                                }
                            };
                            Checkout.prototype.createPaymentCardTenderLine = function () {
                                this.paymentCard(this.paymentCard());
                                if (this.totalAmount != this.cart().TotalAmount) {
                                    this.closeDialogAndDisplayError([Controls.Resources.String_149], false);
                                    return;
                                }
                                if (this.payCreditCard()) {
                                    if (this.isCardPaymentAcceptPage()) {
                                        if (Ecommerce.Utils.isNullOrUndefined(this.tokenizedCartTenderLine)) {
                                            this.submitCardPaymentAcceptPayment(this.creditCardAmount, this.cardPaymentAcceptMessageOrigin, this.cardPaymentAcceptPageSubmitUrl);
                                        }
                                        else {
                                            this.tokenizedCartTenderLine.Amount = this.creditCardAmount;
                                            this.tenderLines.push(this.tokenizedCartTenderLine);
                                        }
                                    }
                                    else {
                                        var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                                        tenderLine.Currency = this.channelCurrencyCode;
                                        tenderLine.Amount = this.creditCardAmount;
                                        tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayCard);
                                        tenderLine.PaymentCard = new CommerceProxy.Entities.PaymentCardClass(this.paymentCard());
                                        this.formattedCreditCardAmount(this.formatCurrencyString(this.creditCardAmount));
                                        tenderLine.PaymentCard.Address1 = this.paymentCardAddress().Street;
                                        tenderLine.PaymentCard.City = this.paymentCardAddress().City;
                                        tenderLine.PaymentCard.State = this.paymentCardAddress().State;
                                        tenderLine.PaymentCard.Zip = this.paymentCardAddress().ZipCode;
                                        tenderLine.PaymentCard.Country = this.paymentCardAddress().ThreeLetterISORegionName;
                                        this.tenderLines.push(tenderLine);
                                    }
                                }
                                if (this.payLoyaltyCard()) {
                                    var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                                    if (!this.isAuthenticated() || (this.loyaltyCards().length == 0 || this._paymentView.find('#CustomLoyaltyRadio').is(':checked'))) {
                                        this.loyaltyCardNumber(this._paymentView.find('#CustomLoyaltyCardNumber').val());
                                    }
                                    tenderLine.LoyaltyCardId = this.loyaltyCardNumber();
                                    tenderLine.Currency = this.channelCurrencyCode;
                                    tenderLine.Amount = this.loyaltyCardAmount;
                                    tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayLoyalty);
                                    this.formattedLoyaltyCardAmount(this.formatCurrencyString(this.loyaltyCardAmount));
                                    this.tenderLines.push(tenderLine);
                                }
                                if (this.payGiftCard()) {
                                    var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                                    tenderLine.Currency = this.channelCurrencyCode;
                                    tenderLine.Amount = this.giftCardAmount;
                                    tenderLine.TenderTypeId = this.getTenderTypeIdForOperationId(this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayGiftCertificate);
                                    tenderLine.GiftCardId = this.giftCardNumber();
                                    this.formattedGiftCardAmount(this.formatCurrencyString(this.giftCardAmount));
                                    this.tenderLines.push(tenderLine);
                                }
                                if (!this.payCreditCard() || !this.isCardPaymentAcceptPage() || !Ecommerce.Utils.isNullOrUndefined(this.tokenizedCartTenderLine)) {
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    this.showCheckoutFragment(this._checkoutFragments.Review);
                                }
                            };
                            Checkout.prototype.updateCheckoutCart = function (event, data) {
                                var _this = this;
                                Controls.CartWebApi.UpdateShoppingCartOnResponse(data, CommerceProxy.Entities.CartType.Checkout, this.displayPromotionBanner())
                                    .done(function (cart) {
                                    _this.currencyStringTemplate = Controls.Core.getExtensionPropertyValue(cart.ExtensionProperties, "CurrencyStringTemplate");
                                    _this.cart(data);
                                    _this.productNameByItemVariantIdMap = _this.createProductNameByItemVariantMap(_this.cart().CartLines);
                                    if (_this._checkoutView.find(" ." + _this._checkoutFragments.Review).is(":visible")) {
                                        _this.updatePayments();
                                    }
                                });
                            };
                            Checkout.isRequestedDeliveryPreferenceApplicable = function (deliveryPreferenceTypeValues, reqDeliveryPreferenceType) {
                                for (var i = 0; i < deliveryPreferenceTypeValues.length; i++) {
                                    if (deliveryPreferenceTypeValues[i] == reqDeliveryPreferenceType) {
                                        return true;
                                    }
                                }
                                return false;
                            };
                            Checkout.prototype.submitCardPaymentAcceptPayment = function (paymentAmount, messageOrigin, submitURL) {
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(submitURL)) {
                                    var d = new Date();
                                    submitURL = submitURL + "#" + d.getTime();
                                    this.cardPaymentAcceptPageUrl(submitURL);
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                }
                                else if (!Ecommerce.Utils.isNullOrWhiteSpace(messageOrigin)) {
                                    var cardPaymentAcceptIframe = document.getElementById("cardPaymentAcceptFrame");
                                    var cardPaymentAcceptMessage = {
                                        type: Checkout.CARDPAYMENTACCEPTPAGESUBMIT,
                                        value: "true"
                                    };
                                    cardPaymentAcceptIframe.contentWindow.postMessage(JSON.stringify(cardPaymentAcceptMessage), messageOrigin);
                                }
                            };
                            Checkout.prototype.setCardPaymentAcceptCardType = function (filteredCreditCardTypes) {
                                if (filteredCreditCardTypes.length === 0) {
                                    this.errorMessages([Controls.Resources.String_309]);
                                    this.showError(this.errorMessages(), true);
                                    return false;
                                }
                                else {
                                    this.cardPaymentAcceptCardType = filteredCreditCardTypes[0].TypeId;
                                    return true;
                                }
                            };
                            Checkout.prototype.filterCreditCardTypes = function (cardPrefix) {
                                var filteredCardTypes = [];
                                for (var i = 0; i < this.cardTypes.length; i++) {
                                    var cardType = this.cardTypes[i];
                                    if (cardType.CardTypeValue !== CommerceProxy.Entities.CardType.InternationalCreditCard &&
                                        cardType.CardTypeValue !== CommerceProxy.Entities.CardType.CorporateCard) {
                                        continue;
                                    }
                                    if (this.isAssociatedCardType(cardType, cardPrefix)) {
                                        filteredCardTypes.push(cardType);
                                    }
                                }
                                return filteredCardTypes;
                            };
                            Checkout.prototype.isAssociatedCardType = function (cardType, cardNumber) {
                                if (cardNumber) {
                                    var maskNumFrom = parseInt(cardType.NumberFrom);
                                    var maskNumTo = parseInt(cardType.NumberTo);
                                    var maskLength = cardType.NumberFrom.length;
                                    var cardSubStr;
                                    cardSubStr = (cardNumber.length > maskLength) ? parseInt(cardNumber.substr(0, maskLength)) : parseInt(cardNumber);
                                    if ((maskNumFrom <= cardSubStr) && (cardSubStr <= maskNumTo)) {
                                        return true;
                                    }
                                }
                                return false;
                            };
                            Checkout.prototype.cardPaymentAcceptMessageHandler = function (eventInfo) {
                                if (!(this.cardPaymentAcceptMessageOrigin.indexOf(eventInfo.origin) === 0)) {
                                    return;
                                }
                                var message = eventInfo.data;
                                if (typeof (message) === "string" && message.length > 0) {
                                    var messageObject = JSON.parse(message);
                                    switch (messageObject.type) {
                                        case Checkout.CARDPAYMENTACCEPTPAGEHEIGHT:
                                            Controls.LoadingOverlay.CloseLoadingDialog();
                                            var cardPaymentAcceptIframe = document.getElementById("cardPaymentAcceptFrame");
                                            cardPaymentAcceptIframe.height = messageObject.value;
                                            break;
                                        case Checkout.CARDPAYMENTACCEPTCARDPREFIX:
                                            this.cardPaymentAcceptCardPrefix = messageObject.value;
                                            break;
                                        case Checkout.CARDPAYMENTACCEPTPAGEERROR:
                                            var paymentErrors = messageObject.value;
                                            var errors = [];
                                            for (var i = 0; i < paymentErrors.length; i++) {
                                                errors.push(new CommerceProxy.ProxyError(paymentErrors[i].Code.toString(), paymentErrors[i].Message));
                                            }
                                            this.closeDialogAndDisplayError(Controls.PaymentErrorHelper.ConvertToClientError(errors), true);
                                            break;
                                        case Checkout.CARDPAYMENTACCEPTPAGERESULT:
                                            var cardPaymentResultAccessCode = messageObject.value;
                                            this.retrieveCardPaymentAcceptResult(cardPaymentResultAccessCode);
                                            break;
                                        default:
                                    }
                                }
                            };
                            Checkout.prototype.addCardPaymentAcceptListener = function () {
                                window.addEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
                            };
                            Checkout.prototype.removeCardPaymentAcceptListener = function () {
                                window.removeEventListener("message", this.cardPaymentAcceptMessageHandlerProxied, false);
                            };
                            Checkout.prototype.handleGetCardPaymentAcceptUrlFailure = function (errors) {
                                if (!Ecommerce.Utils.isNullOrUndefined(errors) && Ecommerce.Utils.hasElements(errors)) {
                                    this.closeDialogAndDisplayError(Controls.PaymentErrorHelper.ConvertToClientError(errors), true);
                                }
                                else {
                                    this.closeDialogAndDisplayError([Controls.Resources.String_211], true);
                                }
                                this.payCreditCard(false);
                                this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                            };
                            Checkout.prototype.getShoppingCart = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.CartWebApi.GetCart(CommerceProxy.Entities.CartType.Checkout, this)
                                    .done(function (cart) {
                                    Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceGetShoppingCartError, errors, Controls.Resources.String_63);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.commenceCheckout = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutStarted();
                                Controls.CartWebApi.CommenceCheckout(this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                        _this.showCheckoutFragment(_this._checkoutFragments.DeliveryPreferences);
                                        _this.initEntitySetCallSuccessful(InitEntitySet.CheckoutCart);
                                        _this.getChannelConfigurationAndTenderTypes(cart);
                                        _this.getDeliveryPreferences();
                                        _this.getCardTypes();
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.CheckoutCart, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_63)]);
                                    }
                                    _this.createEditRewardCardDialog();
                                    _this.createDiscountCodeDialog();
                                    CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartCommenceCheckoutError, errors, Controls.Resources.String_63);
                                    _this.initEntitySetCallFailed(InitEntitySet.CheckoutCart, errors);
                                });
                            };
                            Checkout.prototype.getAllDeliveryOptionDescriptions = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsStarted();
                                Controls.OrgUnitWebApi.GetDeliveryOptionsInfo(this)
                                    .done(function (deliveryOptions) {
                                    if (Ecommerce.Utils.hasElements(deliveryOptions)) {
                                        _this.allDeliveryOptionDescriptions = deliveryOptions;
                                        _this.initEntitySetCallSuccessful(InitEntitySet.DeliveryDescriptions);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.DeliveryDescriptions, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_160)]);
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetAllDeliveryOptionDescriptionsError, errors, Controls.Resources.String_160);
                                    _this.initEntitySetCallFailed(InitEntitySet.DeliveryDescriptions, errors);
                                });
                                ;
                            };
                            Checkout.prototype.getChannelConfigurationAndTenderTypes = function (cart) {
                                var _this = this;
                                CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationStarted();
                                Controls.OrgUnitWebApi.GetChannelConfiguration(this)
                                    .done(function (channelConfiguration) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(channelConfiguration)) {
                                        _this.bingMapsToken = channelConfiguration.BingMapsApiKey;
                                        _this.pickUpInStoreDeliveryModeCode = channelConfiguration.PickupDeliveryModeCode;
                                        _this.emailDeliveryModeCode = channelConfiguration.EmailDeliveryModeCode;
                                        _this.giftCardItemId = channelConfiguration.GiftCardItemId;
                                        _this.channelCurrencyCode = channelConfiguration.Currency;
                                        _this.initEntitySetCallSuccessful(InitEntitySet.ChannelConfigurations);
                                        _this.getCountryRegionInfo(channelConfiguration.DefaultLanguageId);
                                        _this.getTenderTypes(cart);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.ChannelConfigurations, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_98)]);
                                    }
                                    CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetChannelConfigurationError, errors, Controls.Resources.String_98);
                                    _this.initEntitySetCallFailed(InitEntitySet.ChannelConfigurations, errors);
                                });
                            };
                            Checkout.prototype.getDeliveryPreferences = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesStarted();
                                Controls.CartWebApi.GetDeliveryPreferences(this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.cartDeliveryPreferences = data;
                                        _this.updateHeaderLevelDeliveryPreferences(_this.cartDeliveryPreferences.HeaderDeliveryPreferenceTypeValues);
                                        _this.createItemDeliveryPreferenceDialog();
                                        _this.initEntitySetCallSuccessful(InitEntitySet.DeliveryPreferences);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.DeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_98)]);
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetDeliveryPreferencesError, errors, Controls.Resources.String_98);
                                    _this.initEntitySetCallFailed(InitEntitySet.DeliveryPreferences, errors);
                                });
                            };
                            Checkout.prototype.getCardTypes = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.channelServiceGetCardTypesStarted();
                                Controls.OrgUnitWebApi.GetCardTypes(this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.cardTypes = data;
                                        _this.initEntitySetCallSuccessful(InitEntitySet.CardTypes);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.CardTypes, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_68)]);
                                    }
                                    CommerceProxy.RetailLogger.channelServiceGetCardTypesFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetCardTypesError, errors, Controls.Resources.String_68);
                                    _this.initEntitySetCallFailed(InitEntitySet.CardTypes, errors);
                                });
                            };
                            Checkout.prototype.removeFromCartClick = function (cartLine) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                Controls.CartWebApi.RemoveFromCart(CommerceProxy.Entities.CartType.Checkout, [cartLine.LineId], this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                        _this.getDeliveryPreferences();
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_64], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartServiceRemoveFromCartError, errors, Controls.Resources.String_64);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.updateQuantity = function (cartLines) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartUpdateQuantityStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                Controls.CartWebApi.UpdateQuantity(CommerceProxy.Entities.CartType.Checkout, cartLines, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_65], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.shoppingCartUpdateQuantityFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateQuantityError, errors, Controls.Resources.String_65);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.applyPromotionCode = function (cart, valueAccesor) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var discountCode = this._addDiscountCodeDialog.find('#DiscountCodeTextBox').val();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(discountCode)) {
                                    Controls.CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Checkout, discountCode, true, this)
                                        .done(function (cart) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                            _this.closeDiscountCodeDialog();
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_93], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartApplyPromotionCodeError, errors, Controls.Resources.String_93);
                                        _this.closeDiscountCodeDialog();
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                            };
                            Checkout.prototype.removePromotionCode = function (cart, valueAccesor) {
                                var _this = this;
                                CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var srcElement = valueAccesor.target;
                                if (!Ecommerce.Utils.isNullOrUndefined(srcElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement)
                                    && !Ecommerce.Utils.isNullOrUndefined(srcElement.parentElement.lastElementChild)
                                    && !Ecommerce.Utils.isNullOrWhiteSpace(srcElement.parentElement.lastElementChild.textContent)) {
                                    var promoCode = srcElement.parentElement.lastElementChild.textContent;
                                    Controls.CartWebApi.AddOrRemovePromotion(CommerceProxy.Entities.CartType.Checkout, promoCode, false, this)
                                        .done(function (cart) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_94], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartRemovePromotionCodeError, errors, Controls.Resources.String_94);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                                else {
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                }
                            };
                            Checkout.prototype.getOrderDeliveryOptions = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsStarted();
                                this.resetSelectedOrderShippingOptions();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                var shipToAddress = this.latestHeaderLevelDeliverySpecification().DeliveryAddress;
                                Controls.CartWebApi.GetOrderDeliveryOptionsForShipping(shipToAddress, this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.availableDeliveryOptions(data);
                                        var selectedOrderDeliveryOption = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                        selectedOrderDeliveryOption.DeliveryPreferenceTypeValue = CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress;
                                        selectedOrderDeliveryOption.DeliveryAddress = shipToAddress;
                                        if (_this.availableDeliveryOptions().length == 1) {
                                            selectedOrderDeliveryOption.DeliveryModeId = _this.availableDeliveryOptions()[0].Code;
                                        }
                                        _this.latestHeaderLevelDeliverySpecification(selectedOrderDeliveryOption);
                                        _this.hideError();
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_66], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetOrderDeliveryOptionsError, errors, Controls.Resources.String_66);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                                ;
                            };
                            Checkout.prototype.getItemDeliveryOptions = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                var currentLineDeliveryOption = this.currentLineDeliverySpecification();
                                var lineShippingAddress = new CommerceProxy.Entities.LineShippingAddressClass();
                                lineShippingAddress.LineId = currentLineDeliveryOption.LineId;
                                lineShippingAddress.ShippingAddress = currentLineDeliveryOption.DeliverySpecification.DeliveryAddress;
                                Controls.CartWebApi.GetLineDeliveryOptionsForShipping([lineShippingAddress], this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                        var salesLineDeliveryOptionsInResponse = data;
                                        for (var i = 0; i < salesLineDeliveryOptionsInResponse.length; i++) {
                                            if (salesLineDeliveryOptionsInResponse[i].SalesLineId == _this.currentLineDeliverySpecification().LineId) {
                                                _this.availableDeliveryOptions(salesLineDeliveryOptionsInResponse[i].DeliveryOptions);
                                            }
                                        }
                                    }
                                    else {
                                        _this.closeDialogAndDisplayError([Controls.Resources.String_66], true);
                                        return;
                                    }
                                    if (_this.availableDeliveryOptions().length == 1) {
                                        if (Ecommerce.Utils.isNullOrUndefined(_this.currentLineDeliverySpecification().DeliverySpecification)) {
                                            _this.currentLineDeliverySpecification().DeliverySpecification = new CommerceProxy.Entities.DeliverySpecificationClass(null);
                                        }
                                        _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryModeId = _this.availableDeliveryOptions()[0].Code;
                                        _this.currentLineDeliverySpecification(_this.currentLineDeliverySpecification());
                                    }
                                    _this.hideError();
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetItemDeliveryOptionsError, errors, Controls.Resources.String_66);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.getItemUnitFromCartLine = function (cartLine) {
                                var itemUnit = new CommerceProxy.Entities.ItemUnitClass();
                                itemUnit.ItemId = cartLine.ItemId;
                                itemUnit.VariantInventoryDimensionId = cartLine.InventoryDimensionId;
                                return itemUnit;
                            };
                            Checkout.prototype.getNearbyStoresWithAvailabilityService = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                var itemUnits;
                                if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.DeliverItemsIndividually) {
                                    var itemUnit = this.getItemUnitFromCartLine(this.currentCartLine());
                                    itemUnits = [itemUnit];
                                }
                                else if (this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.PickupFromStore) {
                                    itemUnits = this.cart().CartLines.map(function (cl) { return _this.getItemUnitFromCartLine(cl); });
                                }
                                Controls.OrgUnitWebApi.GetNearbyStoresWithAvailability(this.searchLocation.latitude, this.searchLocation.longitude, 0, itemUnits, this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.hasElements(data)) {
                                        _this.resetSelectedOrderShippingOptions();
                                        _this.displayLocations(null);
                                        _this._availableStoresView.hide();
                                        _this.showError([Controls.Resources.String_107], true);
                                    }
                                    else {
                                        _this.orgUnitLocations = data.map(function (oua) { return oua.OrgUnitLocation; });
                                        _this.availabilityByOrgUnitMap = _this.createAvailabilitiesByOrgUnitMap(data);
                                        _this.availabilityFlagByOrgUnitMap = _this.createAvailabilityFlagByOrgUnitMap(data, itemUnits);
                                        _this.renderAvailableStores();
                                        _this.hideError();
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresWithAvailabilityError, errors, Controls.Resources.String_107);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.createProductNameByItemVariantMap = function (cartLines) {
                                var productNameByItemVariantMap = [];
                                if (!Ecommerce.Utils.isNullOrUndefined(cartLines)) {
                                    for (var i = 0; i < cartLines.length; i++) {
                                        var key = cartLines[i].ItemId + '|' + cartLines[i].InventoryDimensionId;
                                        productNameByItemVariantMap[key] = cartLines[i][Controls.Constants.ProductNameProperty];
                                    }
                                }
                                return productNameByItemVariantMap;
                            };
                            Checkout.prototype.getProductName = function (itemId, inventoryDimensionId) {
                                var key = itemId + '|' + inventoryDimensionId;
                                var productName = key;
                                if (this.productNameByItemVariantIdMap != null && !Ecommerce.Utils.isNullOrUndefined(this.productNameByItemVariantIdMap[key])) {
                                    productName = this.productNameByItemVariantIdMap[key];
                                }
                                return productName;
                            };
                            Checkout.prototype.createAvailabilitiesByOrgUnitMap = function (orgUnitAvailabilities) {
                                var availabilitiesByOrgUnitMap = [];
                                if (!Ecommerce.Utils.isNullOrUndefined(orgUnitAvailabilities)) {
                                    for (var i = 0; i < orgUnitAvailabilities.length; i++) {
                                        var currentOrgUnitAvailablities = orgUnitAvailabilities[i];
                                        availabilitiesByOrgUnitMap[currentOrgUnitAvailablities.OrgUnitLocation.OrgUnitNumber] = currentOrgUnitAvailablities.ItemAvailabilities;
                                    }
                                }
                                return availabilitiesByOrgUnitMap;
                            };
                            Checkout.prototype.createAvailabilityFlagByOrgUnitMap = function (orgUnitAvailabilities, itemUnits) {
                                var availabilityFlagByOrgUnitMap = [];
                                if (!Ecommerce.Utils.isNullOrUndefined(orgUnitAvailabilities)) {
                                    for (var i = 0; i < orgUnitAvailabilities.length; i++) {
                                        var itemAvailablities = orgUnitAvailabilities[i].ItemAvailabilities;
                                        var key;
                                        var availableQuantityIndexByItemVariantId = [];
                                        for (var j = 0; j < itemAvailablities.length; j++) {
                                            key = itemAvailablities[j].ItemId + '|' + itemAvailablities[j].VariantInventoryDimensionId;
                                            availableQuantityIndexByItemVariantId[key] = itemAvailablities[j].AvailableQuantity;
                                        }
                                        var areAllItemsAvailableInCurentOrgUnit = true;
                                        for (var j = 0; j < itemUnits.length; j++) {
                                            key = itemUnits[j].ItemId + '|' + itemUnits[j].VariantInventoryDimensionId;
                                            var tempBool = !Ecommerce.Utils.isNullOrUndefined(availableQuantityIndexByItemVariantId[key]) && availableQuantityIndexByItemVariantId[key] > 0;
                                            areAllItemsAvailableInCurentOrgUnit = areAllItemsAvailableInCurentOrgUnit && tempBool;
                                        }
                                        availabilityFlagByOrgUnitMap[orgUnitAvailabilities[i].OrgUnitLocation.OrgUnitNumber] = areAllItemsAvailableInCurentOrgUnit;
                                    }
                                }
                                return availabilityFlagByOrgUnitMap;
                            };
                            Checkout.prototype.areAllReqProductsAvailableInOrgUnit = function (orgUnitNumber) {
                                var areAllSpecifiedProductsAvailable = false;
                                if (!Ecommerce.Utils.isNullOrUndefined(this.availabilityFlagByOrgUnitMap) && !Ecommerce.Utils.isNullOrUndefined(this.availabilityFlagByOrgUnitMap[orgUnitNumber])) {
                                    areAllSpecifiedProductsAvailable = this.availabilityFlagByOrgUnitMap[orgUnitNumber];
                                }
                                return areAllSpecifiedProductsAvailable;
                            };
                            Checkout.prototype.getAvailabilitesforOrgUnitNumber = function (orgUnitNumber) {
                                var availabilities = null;
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(orgUnitNumber) && !Ecommerce.Utils.isNullOrUndefined(this.availabilityByOrgUnitMap)) {
                                    availabilities = this.availabilityByOrgUnitMap[orgUnitNumber];
                                }
                                return availabilities;
                            };
                            Checkout.prototype.getNearbyStoresService = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                Controls.OrgUnitWebApi.GetNearbyStores(this.searchLocation.latitude, this.searchLocation.longitude, 0, this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.hasElements(data)) {
                                        _this.resetSelectedOrderShippingOptions();
                                        _this.displayLocations(null);
                                        _this._availableStoresView.hide();
                                        _this.showError([Controls.Resources.String_107], true);
                                    }
                                    else {
                                        _this.orgUnitLocations = data;
                                        _this.renderAvailableStores();
                                        _this.hideError();
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.storeProductAvailabilityServiceGetNearbyStoresError, errors, Controls.Resources.String_107);
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.prototype.getCountryRegionInfo = function (languageId) {
                                var _this = this;
                                CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoStarted();
                                Controls.StoreOperationsWebApi.GetCountryRegionInfo(languageId, this)
                                    .done(function (data) {
                                    if (Ecommerce.Utils.hasElements(data)) {
                                        _this.countries(_this.getSortedCountries(data));
                                        _this.initEntitySetCallSuccessful(InitEntitySet.CountryRegion);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.CountryRegion, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_165)]);
                                    }
                                    CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetCountryRegionInfoError, errors, Controls.Resources.String_165);
                                    _this.initEntitySetCallFailed(InitEntitySet.CountryRegion, errors);
                                });
                            };
                            Checkout.prototype.getSortedCountries = function (countryRegionInfoArray) {
                                var sortedCountries = [];
                                if (!Ecommerce.Utils.isNullOrUndefined(countryRegionInfoArray)) {
                                    for (var i = 0; i < countryRegionInfoArray.length; i++) {
                                        sortedCountries.push({ CountryCode: countryRegionInfoArray[i].CountryRegionId, CountryName: countryRegionInfoArray[i].ShortName });
                                    }
                                }
                                sortedCountries.sort(function (a, b) { return a.CountryName.localeCompare(b.CountryName); });
                                return sortedCountries;
                            };
                            Checkout.prototype.getStateProvinceInfoService = function (countryCode) {
                                var _this = this;
                                CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoStarted();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(countryCode)) {
                                    Controls.LoadingOverlay.ShowLoadingDialog();
                                    Controls.StoreOperationsWebApi.GetStateProvinceInfo(countryCode, this)
                                        .done(function (data) {
                                        _this.states(data);
                                        if (_this._checkoutView.find(" ." + _this._checkoutFragments.DeliveryPreferences).is(":visible")) {
                                            var tempAddress = _this.tempShippingAddress();
                                            if (_this.selectedOrderDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                                if (!Ecommerce.Utils.isNullOrUndefined(_this.latestHeaderLevelDeliverySpecification().DeliveryAddress) &&
                                                    !Ecommerce.Utils.isNullOrUndefined(_this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State) &&
                                                    _this.countryContainsState(data, _this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State)) {
                                                    tempAddress.State = _this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State;
                                                }
                                                else {
                                                    tempAddress.State = '';
                                                }
                                            }
                                            else if (_this.currentLineLevelSelectedDeliveryPreference() == CommerceProxy.Entities.DeliveryPreferenceType.ShipToAddress) {
                                                if (!Ecommerce.Utils.isNullOrUndefined(_this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress) &&
                                                    !Ecommerce.Utils.isNullOrUndefined(_this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State) &&
                                                    _this.countryContainsState(data, _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State)) {
                                                    tempAddress.State = _this.currentLineDeliverySpecification().DeliverySpecification.DeliveryAddress.State;
                                                }
                                                else {
                                                    tempAddress.State = '';
                                                }
                                            }
                                            _this.tempShippingAddress(tempAddress);
                                        }
                                        else if (!Ecommerce.Utils.isNullOrUndefined(_this.paymentCardAddress())) {
                                            var tempPaymentCardAddress = _this.paymentCardAddress();
                                            if (!Ecommerce.Utils.isNullOrUndefined(_this.paymentCardAddress().State) &&
                                                _this.countryContainsState(data, _this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State)) {
                                                tempPaymentCardAddress.State = _this.latestHeaderLevelDeliverySpecification().DeliveryAddress.State;
                                            }
                                            else {
                                                tempPaymentCardAddress.State = '';
                                            }
                                            _this.paymentCardAddress(tempPaymentCardAddress);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetStateProvinceInfoError, errors, Controls.Resources.String_185);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                            };
                            Checkout.prototype.countryContainsState = function (stateProvinces, stateId) {
                                for (var i = 0; i < stateProvinces.length; i++) {
                                    if (stateId == stateProvinces[i].StateId) {
                                        return true;
                                    }
                                }
                                return false;
                            };
                            Checkout.prototype.isAuthenticatedSession = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.customerServiceIsAuthenticationSessionStarted();
                                Controls.CustomerWebApi.IsAuthenticatedSession()
                                    .done(function (data) {
                                    _this.isAuthenticated(data);
                                    if (_this.isAuthenticated()) {
                                        _this.getUserEmailAndAddresses();
                                    }
                                    else {
                                        _this.initEntitySetCallSuccessful(InitEntitySet.Customer);
                                    }
                                    _this.initEntitySetCallSuccessful(InitEntitySet.IsAuthSession);
                                    CommerceProxy.RetailLogger.customerServiceIsAuthenticationSessionFinished();
                                })
                                    .fail(function (errors) {
                                    _this.isAuthenticated(false);
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Controls.Resources.String_233);
                                    _this.initEntitySetCallFailed(InitEntitySet.IsAuthSession, errors);
                                });
                            };
                            Checkout.prototype.getUserEmailAndAddresses = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.customerServiceGetCustomerStarted();
                                Controls.CustomerWebApi.GetCustomer(this)
                                    .done(function (data) {
                                    if (Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.initEntitySetCallFailed(InitEntitySet.Customer, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_209)]);
                                    }
                                    else {
                                        var addresses = [];
                                        if (data.Addresses) {
                                            for (var i = 0; i < data.Addresses.length; i++) {
                                                var address = data.Addresses[i];
                                                if (Ecommerce.Utils.isNullOrWhiteSpace(address.Name) &&
                                                    Ecommerce.Utils.isNullOrWhiteSpace(address.Street) &&
                                                    Ecommerce.Utils.isNullOrWhiteSpace(address.City) &&
                                                    Ecommerce.Utils.isNullOrWhiteSpace(address.State) &&
                                                    Ecommerce.Utils.isNullOrWhiteSpace(address.ZipCode)) {
                                                    continue;
                                                }
                                                var delimiter = Ecommerce.Utils.isNullOrWhiteSpace(address.State) && Ecommerce.Utils.isNullOrWhiteSpace(address.ZipCode) ? "" : ", ";
                                                var addressString = Ecommerce.Utils.format("({0}) {1} {2}{3}{4} {5}", address.Name, address.Street, address.City, delimiter, address.State, address.ZipCode);
                                                addresses.push({ Value: address, Text: addressString });
                                            }
                                            if (addresses.length > 0) {
                                                _this.storedCustomerAddresses(addresses);
                                            }
                                        }
                                        _this.recepientEmailAddress = data.Email;
                                        _this.initEntitySetCallSuccessful(InitEntitySet.Customer);
                                    }
                                    CommerceProxy.RetailLogger.customerServiceGetCustomerFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.customerServiceGetCustomerError, errors, Controls.Resources.String_209);
                                    _this.initEntitySetCallFailed(InitEntitySet.Customer, errors);
                                });
                            };
                            Checkout.prototype.setHeaderLevelDeliveryOptions = function (headerLevelDeliveryOption) {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsStarted();
                                Controls.CartWebApi.UpdateDeliverySpecification(headerLevelDeliveryOption, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                        _this.showCheckoutFragment(_this._checkoutFragments.PaymentInformation);
                                        _this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.SetDeliveryPreferences);
                                    }
                                    else {
                                        _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_67)]);
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceUpdateDeliverySpecificationsError, errors, Controls.Resources.String_67);
                                    _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, errors);
                                });
                            };
                            Checkout.prototype.setLineLevelDeliveryOptions = function (selectedLineLevelDeliveryOptions) {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsStarted();
                                Controls.CartWebApi.UpdateLineDeliverySpecifications(selectedLineLevelDeliveryOptions, this)
                                    .done(function (cart) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                        Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                        _this.showCheckoutFragment(_this._checkoutFragments.PaymentInformation);
                                        _this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.SetDeliveryPreferences);
                                    }
                                    else {
                                        _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_67)]);
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceUpdateLineDeliverySpecificationsError, errors, Controls.Resources.String_67);
                                    _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.SetDeliveryPreferences, errors);
                                });
                            };
                            Checkout.prototype.getLoyaltyCards = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsStarted();
                                Controls.CustomerWebApi.GetLoyaltyCards(this)
                                    .done(function (data) {
                                    if (Ecommerce.Utils.isNullOrUndefined(data)) {
                                        _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.LoyaltyCards, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_150)]);
                                    }
                                    else {
                                        var loyaltyCards = [];
                                        var _customLoyaltyRadio = _this._paymentView.find("#CustomLoyaltyRadio");
                                        var containsValidLoyaltyCard = false;
                                        for (var i = 0; i < data.length; i++) {
                                            if (data[i].CardTenderTypeValue == CommerceProxy.Entities.LoyaltyCardTenderType.AsCardTender ||
                                                data[i].CardTenderTypeValue == CommerceProxy.Entities.LoyaltyCardTenderType.AsContactTender) {
                                                loyaltyCards.push(data[i].CardNumber);
                                                containsValidLoyaltyCard = true;
                                            }
                                        }
                                        if (!containsValidLoyaltyCard) {
                                            _customLoyaltyRadio.hide();
                                        }
                                        else {
                                            _customLoyaltyRadio.show();
                                            _this.loyaltyCardNumber(loyaltyCards[0]);
                                        }
                                        _this.loyaltyCards(loyaltyCards);
                                        _this.hideError();
                                        _this.initPaymentEntitySetCallSuccessful(InitPaymentEntitySet.LoyaltyCards);
                                    }
                                    CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.loyaltyServiceGetLoyaltyCardsError, errors, Controls.Resources.String_150);
                                    _this.initPaymentEntitySetCallFailed(InitPaymentEntitySet.LoyaltyCards, errors);
                                });
                            };
                            Checkout.prototype.updateLoyaltyCardId = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.loyaltyServiceUpdateLoyaltyCardIdStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_179);
                                var loyaltyCardId = this._editRewardCardDialog.find('#RewardCardTextBox').val();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(loyaltyCardId)) {
                                    Controls.CartWebApi.UpdateLoyaltyCardId(CommerceProxy.Entities.CartType.Checkout, loyaltyCardId, this)
                                        .done(function (cart) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(cart)) {
                                            Controls.CartWebApi.TriggerCartUpdateEvent(CommerceProxy.Entities.CartType.Checkout, cart);
                                            _this.closeEditRewardCardDialog();
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_232], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.loyaltyServiceUpdateLoyaltyCardIdFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.shoppingCartUpdateLoyaltyCardIdError, errors, Controls.Resources.String_232);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeEditRewardCardDialog();
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                            };
                            Checkout.prototype.getTenderTypes = function (cart) {
                                var _this = this;
                                CommerceProxy.RetailLogger.channelServiceGetTenderTypesStarted();
                                Controls.OrgUnitWebApi.GetTenderTypes(this)
                                    .done(function (data) {
                                    if (Ecommerce.Utils.hasElements(data)) {
                                        _this.supportedTenderTypes = data;
                                        _this.calculateSupportedPaymentTypes(data);
                                        if (_this.isCreditCardPaymentAllowed) {
                                            _this._paymentView.find('.msax-PayCreditCard').show();
                                            _this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                                            _this._creditCardPanel.hide();
                                        }
                                        else {
                                            _this._paymentView.find('.msax-PayCreditCard').hide();
                                        }
                                        if (!_this.checkForGiftCardInCart(cart) && _this.isGiftCardPaymentAllowed) {
                                            _this._paymentView.find('.msax-PayGiftCard').show();
                                            _this._giftCardPanel.hide();
                                            _this._paymentView.find('.msax-PayGiftCard .msax-PayGiftCardLink').show();
                                        }
                                        else {
                                            _this._paymentView.find('.msax-PayGiftCard').hide();
                                        }
                                        if (_this.isLoyaltyCardPaymentAllowed) {
                                            _this._paymentView.find('.msax-PayLoyaltyCard').show();
                                            _this._loyaltyCardPanel.hide();
                                            _this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                                        }
                                        else {
                                            _this._paymentView.find('.msax-PayLoyaltyCard').hide();
                                        }
                                        _this.removeValidation(_this._creditCardPanel);
                                        _this.removeValidation(_this._giftCardPanel);
                                        _this.removeValidation(_this._loyaltyCardPanel);
                                        _this.initEntitySetCallSuccessful(InitEntitySet.TenderTypes);
                                    }
                                    else {
                                        _this.initEntitySetCallFailed(InitEntitySet.TenderTypes, [new CommerceProxy.ProxyError(null, null, Controls.Resources.String_138)]);
                                    }
                                    CommerceProxy.RetailLogger.channelServiceGetTenderTypesFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.channelServiceGetTenderTypesError, errors, Controls.Resources.String_138);
                                    _this.initEntitySetCallFailed(InitEntitySet.TenderTypes, errors);
                                });
                            };
                            Checkout.prototype.getCardPaymentAcceptUrl = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                var hostPageOrigin = window.location.protocol + "//" + window.location.host;
                                var adaptorPath = hostPageOrigin + "/Connectors/";
                                var cardPaymentAcceptSettings = {
                                    HostPageOrigin: hostPageOrigin,
                                    AdaptorPath: adaptorPath,
                                    CardPaymentEnabled: false,
                                    CardTokenizationEnabled: true
                                };
                                Controls.CartWebApi.GetCardPaymentAcceptPoint(cardPaymentAcceptSettings, this)
                                    .done(function (data) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(data)
                                        && !Ecommerce.Utils.isNullOrWhiteSpace(data.AcceptPageUrl)
                                        && !Ecommerce.Utils.isNullOrWhiteSpace(data.MessageOrigin)) {
                                        var cardPaymentAcceptUrl = data.AcceptPageUrl;
                                        _this.cardPaymentAcceptPageUrl(cardPaymentAcceptUrl);
                                        _this.cardPaymentAcceptPageSubmitUrl = data.AcceptPageSubmitUrl;
                                        _this.cardPaymentAcceptMessageOrigin = data.MessageOrigin;
                                        _this.isCardPaymentAcceptPage(true);
                                        _this.removeCardPaymentAcceptListener();
                                        _this.addCardPaymentAcceptListener();
                                        _this._creditCardPanel.show();
                                        if (!(cardPaymentAcceptUrl.indexOf(data.MessageOrigin) === 0)) {
                                            var cardPaymentAcceptIframe = document.getElementById("cardPaymentAcceptFrame");
                                            cardPaymentAcceptIframe.height = "600px";
                                        }
                                        _this.updatePaymentTotal();
                                        _this.hideError();
                                    }
                                    else {
                                        _this.handleGetCardPaymentAcceptUrlFailure();
                                    }
                                    CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetCardPaymentAcceptUrlError, errors, Controls.Resources.String_211);
                                    _this.handleGetCardPaymentAcceptUrlFailure(errors);
                                });
                            };
                            Checkout.prototype.retrieveCardPaymentAcceptResult = function (cardPaymentResultAccessCode) {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultStarted();
                                Controls.StoreOperationsWebApi.RetrieveCardPaymentAcceptResult(cardPaymentResultAccessCode, this)
                                    .done(function (cardPaymentAcceptResult) {
                                    if (!Ecommerce.Utils.isNullOrUndefined(cardPaymentAcceptResult) && !Ecommerce.Utils.isNullOrUndefined(cardPaymentAcceptResult.TokenizedPaymentCard)) {
                                        if (Ecommerce.Utils.isNullOrUndefined(_this.cardPaymentAcceptCardPrefix)) {
                                            _this.cardPaymentAcceptCardPrefix = cardPaymentAcceptResult.TokenizedPaymentCard.CardTokenInfo.MaskedCardNumber;
                                        }
                                        var types = _this.filterCreditCardTypes(_this.cardPaymentAcceptCardPrefix);
                                        if (_this.setCardPaymentAcceptCardType(types)) {
                                            cardPaymentAcceptResult.TokenizedPaymentCard.CardTypeId = _this.cardPaymentAcceptCardType;
                                            var tenderLine = new CommerceProxy.Entities.CartTenderLineClass(null);
                                            tenderLine.Currency = _this.channelCurrencyCode;
                                            tenderLine.Amount = _this.creditCardAmount;
                                            tenderLine.TenderTypeId = _this.getTenderTypeIdForOperationId(_this.supportedTenderTypes, CommerceProxy.Entities.RetailOperation.PayCard);
                                            tenderLine.TokenizedPaymentCard = cardPaymentAcceptResult.TokenizedPaymentCard;
                                            tenderLine.CardTypeId = _this.cardPaymentAcceptCardType;
                                            _this.formattedCreditCardAmount(_this.formatCurrencyString(_this.creditCardAmount));
                                            var tokenizedPaymentCardAddress = new CommerceProxy.Entities.AddressClass();
                                            tokenizedPaymentCardAddress.Street = tenderLine.TokenizedPaymentCard.Address1;
                                            tokenizedPaymentCardAddress.City = tenderLine.TokenizedPaymentCard.City;
                                            tokenizedPaymentCardAddress.State = tenderLine.TokenizedPaymentCard.State;
                                            tokenizedPaymentCardAddress.ZipCode = tenderLine.TokenizedPaymentCard.Zip;
                                            tokenizedPaymentCardAddress.ThreeLetterISORegionName = tenderLine.TokenizedPaymentCard.Country;
                                            tokenizedPaymentCardAddress.Email = _this.paymentCardAddress().Email;
                                            _this.paymentCardAddress(tokenizedPaymentCardAddress);
                                            _this.tokenizedCartTenderLine = tenderLine;
                                            _this.tenderLines.push(_this.tokenizedCartTenderLine);
                                            _this.hideError();
                                            _this.showCheckoutFragment(_this._checkoutFragments.Review);
                                        }
                                    }
                                    else {
                                        _this.showError([Controls.Resources.String_210], true);
                                    }
                                    Controls.LoadingOverlay.CloseLoadingDialog();
                                    CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultFinished();
                                }).fail(function (errors) {
                                    _this.closeDialogAndDisplayError(Controls.PaymentErrorHelper.ConvertToClientError(errors), true);
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceRetrieveCardPaymentAcceptResultError, errors, Controls.Resources.String_210);
                                });
                            };
                            Checkout.prototype.calculateSupportedPaymentTypes = function (tenderTypes) {
                                for (var i = 0; i < tenderTypes.length; i++) {
                                    switch (tenderTypes[i].OperationId) {
                                        case CommerceProxy.Entities.RetailOperation.PayCard:
                                            this.isCreditCardPaymentAllowed = true;
                                            break;
                                        case CommerceProxy.Entities.RetailOperation.PayLoyalty:
                                            this.isLoyaltyCardPaymentAllowed = true;
                                            break;
                                        case CommerceProxy.Entities.RetailOperation.PayGiftCertificate:
                                            this.isGiftCardPaymentAllowed = this.isAuthenticated();
                                            break;
                                    }
                                }
                            };
                            Checkout.prototype.getTenderTypeIdForOperationId = function (tenderTypes, operationId) {
                                var tenderTypeId = "";
                                for (var i = 0; i < tenderTypes.length; i++) {
                                    if (tenderTypes[i].OperationId == operationId) {
                                        tenderTypeId = tenderTypes[i].TenderTypeId;
                                        break;
                                    }
                                }
                                return tenderTypeId;
                            };
                            Checkout.prototype.getGiftCardBalance = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                this._paymentView.find('.msax-GiftCardBalance').hide();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                                    Controls.StoreOperationsWebApi.GetGiftCardBalance(this.giftCardNumber(), this)
                                        .done(function (data) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                            var giftCardInResponse = data;
                                            if (Ecommerce.Utils.isNullOrEmpty(giftCardInResponse.Id)) {
                                                _this.isGiftCardInfoAvailable(false);
                                            }
                                            else {
                                                if (_this.checkGiftCardAmountValidity) {
                                                    if (Number(giftCardInResponse.Balance) < Number(_this.giftCardAmount)) {
                                                        _this.closeDialogAndDisplayError([Controls.Resources.String_148], false);
                                                    }
                                                }
                                                _this.isGiftCardInfoAvailable(true);
                                                _this.giftCardBalance(giftCardInResponse.BalanceCurrencyCode + giftCardInResponse.Balance);
                                            }
                                            _this._paymentView.find('.msax-GiftCardBalance').show();
                                            _this.hideError();
                                            if (_this.isGiftCardInfoAvailable() && _this.checkGiftCardAmountValidity) {
                                                _this.createPaymentCardTenderLine();
                                            }
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_145], true);
                                        }
                                        _this.checkGiftCardAmountValidity = false;
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceError, errors, Controls.Resources.String_145);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                        _this.checkGiftCardAmountValidity = false;
                                    });
                                }
                                else {
                                    this.closeDialogAndDisplayError([Controls.Resources.String_144], false);
                                    this.checkGiftCardAmountValidity = false;
                                }
                            };
                            Checkout.prototype.applyFullGiftCardAmount = function () {
                                var _this = this;
                                Controls.LoadingOverlay.ShowLoadingDialog();
                                if (!Ecommerce.Utils.isNullOrWhiteSpace(this.giftCardNumber())) {
                                    CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceStarted();
                                    Controls.StoreOperationsWebApi.GetGiftCardBalance(this.giftCardNumber(), this)
                                        .done(function (data) {
                                        if (!Ecommerce.Utils.isNullOrUndefined(data)) {
                                            var giftCardInResponse = data;
                                            var totalAmount = _this.cart().TotalAmount;
                                            var giftCardBalance = giftCardInResponse.Balance;
                                            var giftCardBalanceWithCurrency = giftCardInResponse.BalanceCurrencyCode + giftCardInResponse.Balance;
                                            var _giftCardTextBox = _this._paymentView.find('#GiftCardAmount');
                                            if (Ecommerce.Utils.isNullOrEmpty(giftCardInResponse.Id)) {
                                                _this.isGiftCardInfoAvailable(false);
                                            }
                                            else {
                                                _this.isGiftCardInfoAvailable(true);
                                                _this.giftCardBalance(giftCardBalance.toString());
                                                if (Number(giftCardBalance) <= Number(totalAmount)) {
                                                    _giftCardTextBox.val(giftCardBalanceWithCurrency);
                                                    _this.updatePaymentTotal();
                                                }
                                                else {
                                                    _giftCardTextBox.val(totalAmount);
                                                    _this._creditCardPanel.hide();
                                                    _this._paymentView.find('.msax-PayCreditCard .msax-PayCreditCardLink').show();
                                                    _this.payCreditCard(false);
                                                    _this._loyaltyCardPanel.hide();
                                                    _this._paymentView.find('.msax-PayLoyaltyCard .msax-PayLoyaltyCardLink').show();
                                                    _this.payLoyaltyCard(false);
                                                }
                                            }
                                            _this._paymentView.find('.msax-GiftCardBalance').show();
                                            _this.hideError();
                                        }
                                        else {
                                            _this.showError([Controls.Resources.String_145], true);
                                        }
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceFinished();
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceGetGiftCardBalanceError, errors, Controls.Resources.String_145);
                                        var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                        _this.closeDialogAndDisplayError(errorMessages, true);
                                    });
                                }
                                else {
                                    this.closeDialogAndDisplayError([Controls.Resources.String_144], false);
                                }
                            };
                            Checkout.prototype.redirectOnOrderCreation = function (channelReferenceId, cleanUpErrors) {
                                this.orderNumber(channelReferenceId);
                                this.hideError();
                                if (Ecommerce.Utils.isNullOrWhiteSpace(msaxValues.msax_OrderConfirmationUrl)) {
                                    this.showCheckoutFragment(this._checkoutFragments.Confirmation);
                                }
                                else {
                                    window.location.href = msaxValues.msax_OrderConfirmationUrl += '?confirmationId=' + channelReferenceId;
                                }
                            };
                            Checkout.prototype.submitOrder = function () {
                                var _this = this;
                                CommerceProxy.RetailLogger.checkoutServiceSubmitOrderStarted();
                                Controls.LoadingOverlay.ShowLoadingDialog(Controls.Resources.String_180);
                                var linesIdsToRemoveFromShoppingCart = this.cart().CartLines.map(function (cartLine) { return cartLine.LineId; });
                                Controls.CartWebApi.SubmitOrder(this.tenderLines, this.paymentCardAddress().Email, this)
                                    .done(function (salesOrder) {
                                    CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderStarted();
                                    Controls.CartWebApi.CleanUpAfterSuccessfulOrder(linesIdsToRemoveFromShoppingCart, _this)
                                        .done(function () {
                                        CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderFinished();
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        _this.redirectOnOrderCreation(salesOrder.ChannelReferenceId);
                                    })
                                        .fail(function (errors) {
                                        Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceCleanUpAfterSuccessfulOrderError, errors, "There was an error on order cleanup");
                                        Controls.LoadingOverlay.CloseLoadingDialog();
                                        _this.redirectOnOrderCreation(salesOrder.ChannelReferenceId, errors);
                                    });
                                    CommerceProxy.RetailLogger.checkoutServiceSubmitOrderFinished();
                                })
                                    .fail(function (errors) {
                                    Controls.Core.LogEvent(CommerceProxy.RetailLogger.checkoutServiceSubmitOrderError, errors, "There was an error submitting the order");
                                    var errorMessages = Controls.ErrorHelper.getErrorMessages(errors);
                                    _this.closeDialogAndDisplayError(errorMessages, true);
                                });
                            };
                            Checkout.CARDPAYMENTACCEPTPAGEHEIGHT = "msax-cc-height";
                            Checkout.CARDPAYMENTACCEPTPAGEERROR = "msax-cc-error";
                            Checkout.CARDPAYMENTACCEPTPAGERESULT = "msax-cc-result";
                            Checkout.CARDPAYMENTACCEPTPAGESUBMIT = "msax-cc-submit";
                            Checkout.CARDPAYMENTACCEPTCARDPREFIX = "msax-cc-cardprefix";
                            return Checkout;
                        })();
                        Controls.Checkout = Checkout;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var ErrorTypeEnum = (function () {
                            function ErrorTypeEnum() {
                            }
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDZIPCODE = "String_212";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSTATE = "String_213";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCITY = "String_214";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSHIPPINGADDRESS = "String_215";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDLOYALTYCARDNUMBER = "String_216";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BLOCKEDLOYALTYCARD = "String_217";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFLICTLOYALTYCARDCUSTOMERANDTRANSACTIONCUSTOMER = "String_218";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENDERLOYALTYCARD = "String_219";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENOUGHREWARDPOINTS = "String_220";
                            ErrorTypeEnum.GENERICERRORMESSAGE = "String_501";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DUPLICATEOBJECT = "String_502";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INSUFFICIENTQUANTITYONHAND = "String_503";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTSALESLINEADD = "String_504";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDFORMAT = "String_505";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LASTCHANGEVERSIONMISMATCH = "String_506";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OBJECTNOTFOUND = "String_507";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REQUIREDVALUENOTFOUND = "String_508";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNKNOWNREQUEST = "String_509";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNSUPPORTEDLANGUAGE = "String_510";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_VALUEOUTOFRANGE = "String_511";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTVERSION = "String_512";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATECOMMUNICATIONERROR = "String_513";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_APPLICATIONCOMPOSITIONFAILED = "String_514";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFIGURATIONSETTINGNOTFOUND = "String_515";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DUPLICATEDEFAULTNOTIFICATIONHANDLERENCOUNTERED = "String_516";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_EMPTYINVENTORYUNITOFMEASUREFORITEM = "String_517";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_EXTERNALPROVIDERERROR = "String_518";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERRESPONSEPARSINGERROR = "String_519";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTSTATE = "String_520";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANNELCONFIGURATION = "String_521";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCONFIGURATIONKEYFORMAT = "String_522";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCONNECTIONSTRING = "String_523";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPIPELINECONFIGURATION = "String_524";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPROVIDERCONFIGURATION = "String_525";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDRUNTIMECONTEXT = "String_526";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSERVERRESPONSE = "String_527";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SERVICEINITIALIZATIONFAILED = "String_528";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SERVICENOTFOUND = "String_529";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCOMPUTESALESTAXGROUPFORADDRESS = "String_530";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDDEFAULTHANDLER = "String_531";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDDELIVERYOPTIONS = "String_532";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDINVENTORYFORITEM = "String_533";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATEVALIDATIONERROR = "String_534";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_COUPONISVALIDFORCURRENTSESSION = "String_535";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CRITICALSTORAGEERROR = "String_536";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCOUNTAMOUNTINVALIDATED = "String_537";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_IDMISMATCH = "String_538";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INSUFFICIENTQUANTITYAVAILABLE = "String_539";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPRICEENCOUNTERED = "String_540";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDREQUEST = "String_541";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSQLCOMMAND = "String_542";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMDISCONTINUEDFROMCHANNEL = "String_543";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OBJECTVERSIONMISMATCHERROR = "String_544";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PROVIDERCOMMUNICATIONFAILURE = "String_545";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REQUESTEDITEMISOUTOFSTOCK = "String_546";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNITOFMEASURECONVERSIONNOTFOUND = "String_547";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEFAULTCUSTOMERNOTFOUND = "String_548";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CURRENCYNOTFOUND = "String_549";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGENERATETOKEN = "String_550";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANNEL = "String_551";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RETAILSERVERAPIVERSIONNOTSUPPORTED = "String_552";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOAUTHORIZEPAYMENT = "String_553";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOYALTYCARDALREADYISSUED = "String_554";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERNOTFOUND = "String_555";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CDXREALTIMESERVICEFAILURE = "String_556";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOMORETHANONELOYALTYTENDER = "String_557";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AMOUNTDUEMUSTBEPAIDBEFORECHECKOUT = "String_558";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPAYMENTREQUEST = "String_559";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDADDRESS = "String_560";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTAMOUNTEXCEEDSGIFTBALANCE = "String_561";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOMORETHANONEOPERATIONWITHAGIFTCARD = "String_562";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDQUANTITY = "String_563";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMQUANTITYEXCEEDED = "String_564";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDUNITOFMEASURE = "String_565";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_INTERNAL_SERVER_ERROR = "String_501";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTFOUND = "String_566";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_GIFTCARDUNLOCKFAILED = "String_567";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGEPASSWORDFAILED = "String_568";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RESETPASSWORDFAILED = "String_569";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICECONNECTIONFAILED = "String_570";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SALESMUSTHAVEQUANTITYGREATERTHANZERO = "String_571";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDUSERTOKEN = "String_572";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAMOUNT = "String_573";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPASSWORD = "String_574";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFIGURATIONERROR = "String_583";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DATAVALIDATIONERROR = "String_584";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_STORAGEERROR = "String_585";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERCOMMUNICATIONFAILURE = "String_586";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICETIMEOUT = "String_588";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEEXCEPTION = "String_589";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEAUTHENTICATIONFAILEDFAULT = "String_590";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEFORBIDDENFAULT = "String_591";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICESENDERFAULT = "String_592";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDDELIVERYPREFERENCES = "String_593";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDCONFIGFORTENDERTYPE = "String_594";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTCONNECTORNOTFOUND = "String_595";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SALESLINENOTALLOWED = "String_596";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDDELIVERYMODE = "String_597";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDEMAILADDRESS = "String_598";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDDELIVERYPREFERENCETYPE = "String_599";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSTATUS = "String_600";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTACTIVE = "String_601";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MULTIPLECREDITCARDPAYMENTNOTSUPPORTED = "String_602";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTTYPECANNOTBENONE = "String_603";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTSALESLINEUPDATE = "String_604";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPRODUCT = "String_605";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTINVENTORYLOCATIONID = "String_606";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTLINESAGGREGATEERROR = "String_607";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFLICTINGCARTLINEOPERATION = "String_608";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGEBACKISNOTALLOWED = "String_575";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERLINE = "String_576";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERTRANSACTION = "String_577";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERLINE = "String_578";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERTRANSACTION = "String_579";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTMUSTBEUSEDTOFINALIZETRANSACTION = "String_580";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PRODUCTISNOTACTIVE = "String_581";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PRODUCTISBLOCKED = "String_582";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGETCARDPAYMENTACCEPTPOINT = "String_211";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETORETRIEVECARDPAYMENTACCEPTRESULT = "String_210";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDOPERATION = "String_301";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_APPLICATIONERROR = "String_302";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_GENERICCHECKDETAILSFORERROR = "String_303";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DONOTAUTHORIZED = "String_304";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_USERABORTED = "String_305";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_LOCALENOTSUPPORTED = "String_306";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDMERCHANTPROPERTY = "String_307";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_COMMUNICATIONERROR = "String_308";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDARGUMENTCARDTYPENOTSUPPORTED = "String_309";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_VOICEAUTHORIZATIONNOTSUPPORTED = "String_310";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONNOTSUPPORTED = "String_311";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MULTIPLECAPTURENOTSUPPORTED = "String_312";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_BATCHCAPTURENOTSUPPORTED = "String_313";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNSUPPORTEDCURRENCY = "String_314";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNSUPPORTEDCOUNTRY = "String_315";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTREAUTHORIZEPOSTCAPTURE = "String_316";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTREAUTHORIZEPOSTVOID = "String_317";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTURENOTSUPPORTED = "String_318";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDEXPIRED = "String_319";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REFERTOISSUER = "String_320";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOREPLY = "String_321";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_HOLDCALLORPICKUPCARD = "String_322";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDAMOUNT = "String_323";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ACCOUNTLENGTHERROR = "String_324";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ALREADYREVERSED = "String_325";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTVERIFYPIN = "String_326";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDNUMBER = "String_327";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCVV2 = "String_328";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CASHBACKNOTAVAILABLE = "String_329";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDTYPEVERIFICATIONERROR = "String_330";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DECLINE = "String_331";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ENCRYPTIONERROR = "String_332";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOACTIONTAKEN = "String_333";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOSUCHISSUER = "String_334";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PINTRIESEXCEEDED = "String_335";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SECURITYVIOLATION = "String_336";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SERVICENOTALLOWED = "String_337";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_STOPRECURRING = "String_338";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_WRONGPIN = "String_339";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2MISMATCH = "String_340";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DUPLICATETRANSACTION = "String_341";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REENTER = "String_342";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AMOUNTEXCEEDLIMIT = "String_343";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONEXPIRED = "String_344";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONALREADYCOMPLETED = "String_345";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONISVOIDED = "String_346";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PROCESSORDUPLICATEBATCH = "String_347";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONFAILURE = "String_348";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDMERCHANTCONFIGURATION = "String_349";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDEXPIRATIONDATE = "String_350";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDHOLDERNAMEFIRSTNAMEREQUIRED = "String_351";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDHOLDERNAMELASTNAMEREQUIRED = "String_352";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_FILTERDECLINE = "String_353";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDADDRESS = "String_354";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2REQUIRED = "String_355";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDTYPENOTSUPPORTED = "String_356";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNIQUEINVOICENUMBERREQUIRED = "String_357";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_POSSIBLEDUPLICATE = "String_358";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PROCESSORREQUIRESLINKEDREFUND = "String_359";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CRYPTOBOXUNAVAILABLE = "String_360";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2DECLINED = "String_361";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MERCHANTIDINVALID = "String_362";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TRANNOTALLOWED = "String_363";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TERMINALNOTFOUND = "String_364";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDEFFECTIVEDATE = "String_365";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INSUFFICIENTFUNDS = "String_366";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONMAXREACHED = "String_367";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONNOTALLOWED = "String_368";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DATEOFBIRTHERROR = "String_369";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ENTERLESSERAMOUNT = "String_370";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_HOSTKEYERROR = "String_371";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCASHBACKAMOUNT = "String_372";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTRANSACTION = "String_373";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTUREREQUIRED = "String_374";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTUREREQUIREDMAC = "String_375";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MACREQUIRED = "String_376";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_BANKCARDNOTSET = "String_377";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDREQUEST = "String_378";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTRANSACTIONFEE = "String_379";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOCHECKINGACCOUNT = "String_380";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOSAVINGSACCOUNT = "String_381";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_RESTRICTEDCARDTEMPORARILYDISALLOWEDFROMINTERCHANGE = "String_382";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MACSECURITYFAILURE = "String_383";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_EXCEEDSWITHDRAWALFREQUENCYLIMIT = "String_384";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCAPTUREDATE = "String_385";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOKEYSAVAILABLE = "String_386";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KMESYNCERROR = "String_387";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KPESYNCERROR = "String_388";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KMACSYNCERROR = "String_389";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_RESUBMITEXCEEDSLIMIT = "String_390";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SYSTEMPROBLEMERROR = "String_391";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ACCOUNTNUMBERNOTFOUNDFORROW = "String_392";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTOKENINFOPARAMETERFORROW = "String_393";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_EXCEPTIONTHROWNFORROW = "String_394";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TRANSACTIONAMOUNTEXCEEDSREMAINING = "String_395";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDARGUMENTTENDERACCOUNTNUMBER = "String_396";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDTRACKDATA = "String_397";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDRESULTACCESSCODE = "String_398";
                            ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_GENERALEXCEPTION = "String_399";
                            return ErrorTypeEnum;
                        })();
                        Controls.ErrorTypeEnum = ErrorTypeEnum;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        Controls.ResourceStrings["fr"] = {
                            String_1: "(FR) Shopping cart",
                            String_2: "(FR) Product details",
                            String_3: "(FR) Each",
                            String_4: "(FR) Quantity",
                            String_5: "(FR) Line total",
                            String_6: "(FR) Remove",
                            String_7: "(FR) Savings:",
                            String_8: "(FR) Update quantity",
                            String_9: "(FR) Order summary",
                            String_10: "(FR) Subtotal:",
                            String_11: "(FR) Shipping and handling:",
                            String_12: "(FR) Order total:",
                            String_13: "(FR) Total savings:",
                            String_14: "(FR) Next step",
                            String_15: "(FR) There are no items in your shopping cart. Please add items to the cart.",
                            String_16: "(FR) Delivery information",
                            String_17: "(FR) Delivery preference:",
                            String_18: "(FR) Shipping address",
                            String_19: "(FR) Shipping name",
                            String_20: "(FR) Country/region",
                            String_21: "(FR) United States",
                            String_22: "(FR) Address",
                            String_23: "(FR) City",
                            String_24: "(FR) State/province",
                            String_25: "(FR) ZIP/postal code",
                            String_26: "(FR) Shipping method",
                            String_27: "(FR) Get shipping options",
                            String_28: "(FR) Previous step",
                            String_29: "(FR) Name",
                            String_30: "(FR) Billing information",
                            String_31: "(FR) Contact information",
                            String_32: "(FR) Email address",
                            String_33: "(FR) Confirm email address",
                            String_34: "(FR) Payment method",
                            String_35: "(FR) Card number",
                            String_36: "(FR) Card type",
                            String_37: "(FR) Expiration month",
                            String_38: "(FR) Expiration year",
                            String_39: "(FR) CCID",
                            String_40: "(FR) What is this?",
                            String_41: "(FR) Payment amount",
                            String_42: "(FR) Billing address",
                            String_43: "(FR) Same as shipping address",
                            String_44: "(FR) Address2",
                            String_45: "(FR) Review and confirm",
                            String_46: "(FR) Order information",
                            String_47: "(FR) Edit",
                            String_48: "(FR) Credit card",
                            String_49: "(FR) Checkout",
                            String_50: "(FR) You have not added any promotion code to your order",
                            String_51: "(FR) Tax:",
                            String_52: "(FR) Submit order",
                            String_53: "(FR) Thank you for your order",
                            String_54: "(FR) Your order confirmation number is ",
                            String_55: "(FR) Street",
                            String_56: "(FR) State",
                            String_57: "(FR) Zipcode",
                            String_58: "(FR) Email",
                            String_59: "(FR) Payment",
                            String_60: "(FR) CardNumber",
                            String_61: "(FR) Please select shipping method",
                            String_62: "(FR) The confirm email address must match the email address.",
                            String_63: "(FR) Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.",
                            String_64: "(FR) Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.",
                            String_65: "(FR) Sorry, something went wrong. The product quantity couldn't be updated. Please refresh the page and try again.",
                            String_66: "(FR) Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.",
                            String_67: "(FR) Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.",
                            String_68: "(FR) Sorry, something went wrong. The payment card type information was not retrieved successfully. Please refresh the page and try again.",
                            String_69: "(FR) Sorry, something went wrong. The order submission was not successful. Please refresh the page and try again.",
                            String_70: "(FR) Invalid parameter",
                            String_71: "(FR) validatorType attribute is not provided for validator binding.",
                            String_72: "(FR) Use text characters only. Numbers, spaces, and special characters are not allowed.",
                            String_73: "(FR) Use text characters only. Numbers, spaces, and special characters are not allowed.",
                            String_74: "(FR) The quantity field cannot be empty",
                            String_75: "(FR) Select delivery method.",
                            String_76: "(FR) The email address is invalid.",
                            String_77: "(FR) Please enter the name.",
                            String_78: "(FR) Please enter the street number.",
                            String_79: "(FR) Please enter the address.",
                            String_80: "(FR) Please enter the city.",
                            String_81: "(FR) Please enter the zip/postal code.",
                            String_82: "(FR) Please enter the state.",
                            String_83: "(FR) Please enter the country.",
                            String_84: "(FR) Please specify a payment card name.",
                            String_85: "(FR) Please enter a valid card number.",
                            String_86: "(FR) Please enter a valid CCID.",
                            String_87: "(FR) Please specify a valid amount.",
                            String_88: "(FR) {0} PRODUCT(S)",
                            String_89: "(FR) Included",
                            String_90: "(FR) Color: {0}",
                            String_91: "(FR) Size: {0}",
                            String_92: "(FR) Style: {0}",
                            String_93: "(FR) Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.",
                            String_94: "(FR) Sorry, something went wrong. The promotion code could not be removed successfully. Please refresh the page and try again.",
                            String_95: "(FR) Apply",
                            String_96: "(FR) Promotion Codes",
                            String_97: "(FR) Please enter a promotion code",
                            String_98: "(FR) Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.",
                            String_99: "(FR) Ship items",
                            String_100: "(FR) Pick up in store",
                            String_101: "(FR) Select delivery options by item",
                            String_102: "(FR) Find a store",
                            String_103: "(FR) miles",
                            String_104: "(FR) Available stores",
                            String_105: "(FR) Store",
                            String_106: "(FR) Distance",
                            String_107: "(FR) Products are not available for pick up in the stores around the location you searched. Please update your delivery preferences and try again.",
                            String_108: "(FR) Sorry, something went wrong. An error occurred while trying to get stores. Please refresh the page and try again.",
                            String_109: "(FR) Sorry, we were not able to decipher the address you gave us.  Please enter a valid Address",
                            String_110: "(FR) Sorry, something went wrong. An error has occured while looking up the address you provided. Please refresh the page and try again.",
                            String_111: "(FR) Products are not available in this store",
                            String_112: "(FR) Product availability:",
                            String_113: "(FR) Products are not available in the selected store, Please select a different store",
                            String_114: "(FR) Please select a store for pick up",
                            String_115: "(FR) Store address",
                            String_116: "(FR) Send to me",
                            String_117: "(FR) Optional note",
                            String_118: "(FR) Please enter email address for gift card delivery",
                            String_119: "(FR) Sorry, something went wrong. An error occurred while trying to get the email address. Please enter the email address in the text box below",
                            String_120: "(FR) Enter the shipping address and then click get shipping options to view the shipping options that are available for your area.",
                            String_121: "(FR) Delivery method",
                            String_122: "(FR) Select your delivery preference",
                            String_123: "(FR) Cancel",
                            String_124: "(FR) Done",
                            String_125: "(FR) for product: {0}",
                            String_126: "(FR) Please select delivery preference for product {0}",
                            String_127: "(FR) Add credit card",
                            String_128: "(FR) Gift card",
                            String_129: "(FR) Add gift card",
                            String_130: "(FR) Loyalty card",
                            String_131: "(FR) Add loyalty card",
                            String_132: "(FR) Payment information",
                            String_133: "(FR) Payment total:",
                            String_134: "(FR) Order total:",
                            String_135: "(FR) Gift card does not exist",
                            String_136: "(FR) Gift card balance",
                            String_137: "(FR) Card details",
                            String_138: "(FR) Sorry, something went wrong. An error occurred while trying to get payment methods supported by the store. Please refresh the page and try again.",
                            String_139: "(FR) Please select payment method",
                            String_140: "(FR) The expiration date is not valid. Please select valid expiration month and year and then try again",
                            String_141: "(FR) Please enter a valid gift card number",
                            String_142: "(FR) Get gift card balance",
                            String_143: "(FR) Apply full amount",
                            String_144: "(FR) Please enter a gift card number",
                            String_145: "(FR) Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.",
                            String_146: "(FR) Gift card payment amount cannot be zero",
                            String_147: "(FR) Gift card payment amount is more than order total",
                            String_148: "(FR) Gift card does not have sufficient balance",
                            String_149: "(FR) Payment amount is different from the order total",
                            String_150: "(FR) Sorry, something went wrong. An error occurred while trying to get loyalty card information. Please refresh the page and try again.",
                            String_151: "(FR) Please enter a valid loyalty card number",
                            String_152: "(FR) Loyalty card payment amount cannot be zero",
                            String_153: "(FR) Loyalty card payment amount is more than order total",
                            String_154: "(FR) The loyalty card is blocked",
                            String_155: "(FR) The loyalty card is not allowed for payment",
                            String_156: "(FR) The loyalty payment amount exceeds what is allowed for this loyalty card in this transaction",
                            String_157: "(FR) The loyalty card number does not exist",
                            String_158: "(FR) Please select delivery preference",
                            String_159: "(FR) Please select a delivery preference...",
                            String_160: "(FR) Sorry, something went wrong. An error occurred while trying to get delivery methods information. Please refresh the page and try again.",
                            String_161: "(FR) Select address...",
                            String_162: "(FR) You have not added loyalty card number to your order",
                            String_163: "(FR) Enter a reward card for the current order. You can include only one reward card per order",
                            String_164: "(FR) Sorry, something went wrong. An error occurred while trying to update reward card id in cart. Please refresh the page and try again.",
                            String_165: "(FR) Sorry, something went wrong. An error occurred while retrieving the country region information. Please refresh the page and try again.",
                            String_166: "(FR) TBD",
                            String_167: "(FR) Mini Cart",
                            String_168: "(FR) Ordering FAQ",
                            String_169: "(FR) Return policy",
                            String_170: "(FR) Store locator tool",
                            String_171: "(FR) Continue shopping",
                            String_172: "(FR) Cart Order Total",
                            String_173: "(FR) View full cart contents",
                            String_174: "(FR) Quantity:",
                            String_175: "(FR) Added to your cart:",
                            String_176: "(FR) Loading ...",
                            String_177: "(FR) Sorry, something went wrong. The cart's promotion information couldn't be retrieved. Please refresh the page and try again.",
                            String_178: "(FR) Delivery method",
                            String_179: "(FR) Updating shopping cart ...",
                            String_180: "(FR) Submitting order ...",
                            String_181: "(FR) Discount code",
                            String_182: "(FR) Add coupon code",
                            String_183: "(FR) Enter a discount code",
                            String_184: "(FR) Please enter a valid discount code",
                            String_185: "(FR) Sorry, something went wrong. An error occurred while retrieving the state/province information. Please refresh the page and try again.",
                            String_186: "(FR) Edit reward card",
                            String_187: "(FR) Reward card",
                            String_188: "(FR) Add discount code",
                            String_189: "(FR) Select country/region",
                            String_190: "(FR) Select state/province",
                            String_191: "(FR) You have selected multiple delivery methods for this order",
                            String_192: "(FR) 01-January",
                            String_193: "(FR) 02-February",
                            String_194: "(FR) 03-March",
                            String_195: "(FR) 04-April",
                            String_196: "(FR) 05-May",
                            String_197: "(FR) 06-June",
                            String_198: "(FR) 07-July",
                            String_199: "(FR) 08-August",
                            String_200: "(FR) 09-September",
                            String_201: "(FR) 10-October",
                            String_202: "(FR) 11-November",
                            String_203: "(FR) 12-December",
                            String_204: "(FR) The number string has more than one decimal operator.",
                            String_205: "(FR) Estimated total: ",
                            String_206: "(FR) The actual tax amount will be calculated based on the applicable state and local sales taxes when your delivery preference is selected.",
                            String_207: "(FR) The shipping and handling charges will be calculated based on delivery preference and recipient address(es) provided.",
                            String_208: "(FR) Included",
                            String_209: "(FR) Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.",
                            String_210: "(FR) Sorry, something went wrong. We were unable to obtain the card payment accept result. Please refresh the page and try again.",
                            String_211: "(FR) Sorry, something went wrong. We were unable to obtain the card payment accept page url. Please refresh the page and try again.",
                            String_212: "(FR) The specified zip code is invalid.",
                            String_213: "(FR) The specified state is invalid.",
                            String_214: "(FR) The specified city is invalid.",
                            String_215: "(FR) The shipping address is not valid.",
                            String_216: "(FR) The loyalty card number that you entered is invalid.",
                            String_217: "(FR) The loyalty card is blocked.",
                            String_218: "(FR) The loyalty card can't be added to the transaction because the customer on the loyalty card is different than the customer on the transaction. Either change the customer on the transaction or select a different loyalty card.",
                            String_219: "(FR) This loyalty card is not eligible to redeem loyalty points for this transaction.",
                            String_220: "(FR) There are not enough loyalty reward points available on the loyalty card.",
                            String_221: "(FR) The service call is taking longer than expected. Please wait for it to respond or refresh the page and try again.",
                            String_222: "(FR) IMPORTANT NOTICE:",
                            String_223: "(FR) This form is provided by your designated third-party payment provider.  By clicking \"Next\" you acknowledge your information will be transmitted directly to your payment provider, and will be handled in accordance with the terms and conditions and privacy statement that you agreed to with your payment provider.  Microsoft does not collect, store, or transmit any of your payment card information, or provide rights for third-party products or services.",
                            String_224: "(FR) Order date",
                            String_225: "(FR) Order number",
                            String_226: "(FR) Order status",
                            String_227: "(FR) Recent orders",
                            String_228: "(FR) My account",
                            String_229: "(FR) Order history",
                            String_230: "(FR) Sorry, something went wrong. We were unable to obtain your order history. Please refresh the page and try again.",
                            String_231: "(FR) There are no orders to display.",
                            String_232: "(FR) Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.",
                            String_233: "(FR) Sorry, something went wrong. An error occurred while trying to get user login information. Please refresh the page and try again.",
                            String_234: "(FR) Address book",
                            String_235: "(FR) There are no addresses.",
                            String_236: "(FR) Order detail: ",
                            String_237: "(FR) Sorry, something went wrong. An error occurred while trying to get the order details information. Please refresh the page and try again.",
                            String_238: "(FR) Line total:",
                            String_239: "(FR) Delivery method: ",
                            String_240: "Processing",
                            String_241: "Order number: ",
                            String_242: "Order status: ",
                            String_301: "(FR) Invalid Operation",
                            String_302: "(FR) The application has encountered an unknown error.",
                            String_303: "(FR) Errors found, check the detailed error results for additional information.",
                            String_304: "(FR) Errors found, payment not authorized. Check the detailed error results for additional information.",
                            String_305: "(FR) User aborted",
                            String_306: "(FR) Locale not supported",
                            String_307: "(FR) Invalid merchant property",
                            String_308: "(FR) There was an error communicating with the payment provider. Retry your request.",
                            String_309: "(FR) The specified card type is not supported",
                            String_310: "(FR) Voice authorization not supported.",
                            String_311: "(FR) Re-authorizations are not supported by this payment provider.",
                            String_312: "(FR) The payment provider assigned does not support multiple captures against a single authorization.",
                            String_313: "(FR) Batch capture is not supported.",
                            String_314: "(FR) Invalid CurrencyCode",
                            String_315: "(FR) Invalid CountryOrRegion",
                            String_316: "(FR) Cannot reauthorize a transaction against which funds have already been captured.",
                            String_317: "(FR) Cannot reauthorize a transaction that has been voided.",
                            String_318: "(FR) The payment provider does not support immediate capture.",
                            String_319: "(FR) Credit card expired",
                            String_320: "(FR) Refer to card issuer",
                            String_321: "(FR) There was no reply from the payment provider for this transaction. Retry the operation.",
                            String_322: "(FR) The transaction was declined by the payment provider. The payment provider requests that you hold the card and call the card issuer.",
                            String_323: "(FR) Invalid Amount",
                            String_324: "(FR) AccountNumber has an invalid length",
                            String_325: "(FR) The payment provider rejected the transaction with the following message: The transaction has already been reversed",
                            String_326: "(FR) Invalid PIN",
                            String_327: "(FR) Invalid AccountNumber",
                            String_328: "(FR) Invalid CVV",
                            String_329: "(FR) Cashback is not supported for this transaction.",
                            String_330: "(FR) Invalid CardType",
                            String_331: "(FR) The transaction was declined by the payment provider.",
                            String_332: "(FR) Encryption error, check the detailed error results for additional information.",
                            String_333: "(FR) The payment provider rejected the transaction with this message: No action taken",
                            String_334: "(FR) The payment provider rejected the transaction with this message: No such issuer",
                            String_335: "(FR) Invalid PIN. You have exceeded the allowed number of attempts to authenticate and this account has been locked.",
                            String_336: "(FR) The payment provider rejected the transaction with this message: Security violation at the processor",
                            String_337: "(FR) The payment provider rejected the transaction with this message: Service not allowed",
                            String_338: "(FR) The payment provider rejected the transaction with this message: Customer stopped recurring",
                            String_339: "(FR) The transaction was rejected due to an invalid PIN.",
                            String_340: "(FR) The transaction was rejected due to an invalid card verification value (CVV/CVV2).",
                            String_341: "(FR) The payment transaction duplicates a previous payment transaction. The duplicate transaction was not processed.",
                            String_342: "(FR) The payment provider rejected the transaction with this message: Re-Enter",
                            String_343: "(FR) Amount exceeds the maximum allowed amount for a payment",
                            String_344: "(FR) Authorization for this payment transaction has expired. You will need to re-authorize before capturing funds.",
                            String_345: "(FR) The payment provider rejected the capture because the entire amount of the original authorization has already been captured.",
                            String_346: "(FR) Cannot process payments against a transaction that has been voided.",
                            String_347: "(FR) The payment provider rejected the batch as a duplicate.",
                            String_348: "(FR) Authorization failed",
                            String_349: "(FR) The merchant configuration is invalid.",
                            String_350: "(FR) Invalid Expiration Date",
                            String_351: "(FR) Invalid Firstname",
                            String_352: "(FR) Invalid LastName",
                            String_353: "(FR) The payment provider declined this transaction.",
                            String_354: "(FR) Invalid Address",
                            String_355: "(FR) The transaction was rejected because a Card Verification Value (CVV/CVV2) is required and was not provided.",
                            String_356: "(FR) CardType is an unsupported type",
                            String_357: "(FR) The payment provider rejected the transaction because the invoice number is a duplicate of a previous transaction.",
                            String_358: "(FR) Payment provider indicated that this transaction appears to be a duplicate. The transaction was not processed.",
                            String_359: "(FR) The payment provider selected for this payment type requires refunds to link to an existing payment capture. Reference a capture transaction using the CaptureTransactionGuid field.",
                            String_360: "(FR) Encryption unavailable",
                            String_361: "(FR) The transaction was declined due to an invalid card verification value (CVV/CVV2).",
                            String_362: "(FR) The Merchant Id is invalid.",
                            String_363: "(FR) The transaction was not allowed.",
                            String_364: "(FR) The terminal has not been registered. Check your terminal Id setting.",
                            String_365: "(FR) Invalid effective date",
                            String_366: "(FR) Insufficient funds for the transaction.",
                            String_367: "(FR) The payment provider rejected the transaction with this message: Reauthorization maximum reached",
                            String_368: "(FR) The payment provider rejected the transaction with this message: Reauthorization Not Allowed",
                            String_369: "(FR) Invalid Date of Birth",
                            String_370: "(FR) The transaction requires a smaller amount.",
                            String_371: "(FR) Host key error",
                            String_372: "(FR) Invalid cashback amount",
                            String_373: "(FR) Invalid transaction",
                            String_374: "(FR) Immediate transaction type required",
                            String_375: "(FR) Immediate transaction type required with MAC",
                            String_376: "(FR) MAC Required for this transaction",
                            String_377: "(FR) The bank card property in the request has not been set.",
                            String_378: "(FR) Invalid request message sent.",
                            String_379: "(FR) Invalid transaction fee",
                            String_380: "(FR) No checking account",
                            String_381: "(FR) No savings account",
                            String_382: "(FR) Restricted card temporarily disallowed from interchange.",
                            String_383: "(FR) MAC security failure",
                            String_384: "(FR) Payment transaction exceeds withdrawal frequency limit.",
                            String_385: "(FR) Invalid capture date",
                            String_386: "(FR) No keys available",
                            String_387: "(FR) KME sync error",
                            String_388: "(FR) KPE sync error",
                            String_389: "(FR) KMAC sync error",
                            String_390: "(FR) Payment transaction has exceeded the limit of resubmits.",
                            String_391: "(FR) System problem error",
                            String_392: "(FR) Account number not found for row {0}.",
                            String_393: "(FR) Invalid TokenInfo parameter for row {0}.",
                            String_394: "(FR) Exception {0} occurred for row {1}.",
                            String_395: "(FR) The transaction amount exceeds the remaining authorized amount.",
                            String_396: "(FR) Invalid TenderAccountNumber",
                            String_397: "(FR) The card track data is invalid.",
                            String_398: "(FR) The payment accept page result access code is invalid.",
                            String_399: "(FR) Sorry, something went wrong. Payment exception has occured. Please refresh the page and try again or try another payment method.",
                            String_501: "(FR) Sorry something went wrong, we cannot process your request at this time. Please try again later.",
                            String_502: "(FR) The specified ID already exists.",
                            String_503: "(FR) There is an insufficient quantity of the product on-hand.",
                            String_504: "(FR) The cart line add is not valid.",
                            String_505: "(FR) The format of the specified data is not valid.",
                            String_506: "(FR) The change version requested and stored do not match.",
                            String_507: "(FR) The specified ID was not found.",
                            String_508: "(FR) Required value is missing.",
                            String_509: "(FR) The specified request type is unknown.",
                            String_510: "(FR) The language specified is not supported.",
                            String_511: "(FR) The specified value is out of range.",
                            String_512: "(FR) The cart was updated by another session. Please refresh and retry.",
                            String_513: "(FR) A communication error occurred.",
                            String_514: "(FR) Runtime components for the application are missing.",
                            String_515: "(FR) Configuration settings are missing.",
                            String_516: "(FR) Duplicate notification handlers have been encountered.",
                            String_517: "(FR) The unit of measure is not set for the product.",
                            String_518: "(FR) An error occurred communicating with an external provider.",
                            String_519: "(FR) Data returned from headquarters could not be parsed.",
                            String_520: "(FR) There is nothing in the cart. Add a product to the cart, and then try again.",
                            String_521: "(FR) The channel configuration is not properly configured.",
                            String_522: "(FR) The configuration key is not valid.",
                            String_523: "(FR) The database connection string is not valid.",
                            String_524: "(FR) The runtime pipeline has not been correctly configured.",
                            String_525: "(FR) One or more providers are not correctly configured.",
                            String_526: "(FR) The runtime context is not valid.  This is most likely a coding error.",
                            String_527: "(FR) The response returned from the server is not valid.",
                            String_528: "(FR) One or more runtime components failed to initialize.",
                            String_529: "(FR) The service cannot be found.",
                            String_530: "(FR) The address provided does not match any sales tax group. This is likely a sales tax group configuration issue.",
                            String_531: "(FR) No default notification handler has been configured.",
                            String_532: "(FR) The delivery options could not be found.",
                            String_533: "(FR) No inventory can be found for this product.",
                            String_534: "(FR) A validation error occurred.",
                            String_535: "(FR) The coupon is only valid for the current session.",
                            String_536: "(FR) A database error has occurred.",
                            String_537: "(FR) The discount amount for a line item has changed.",
                            String_538: "(FR) An ID mismatch has occurred.",
                            String_539: "(FR) There is an insufficient quantity of the product available.",
                            String_540: "(FR) The price cannot be found for the product.",
                            String_541: "(FR) The request passed to the service is not valid.",
                            String_542: "(FR) A SQL command is not valid. This is most likely a coding error.",
                            String_543: "(FR) One or more products are discontinued for the given channel.",
                            String_544: "(FR) There is a mismatch between the object to be saved and the object in the database. Please try again.",
                            String_545: "(FR) There is an error communicating with the provider.",
                            String_546: "(FR) The requested product is out of stock.",
                            String_547: "(FR) The unit of measure conversion cannot be found.",
                            String_548: "(FR) Default customer cannot be found.",
                            String_549: "(FR) The given currency is not supported.",
                            String_550: "(FR) The number that was entered for the credit card number isn't valid. Enter a valid card number.",
                            String_551: "(FR) The channel is invalid.",
                            String_552: "(FR) The server api version is not supported",
                            String_553: "(FR) Declined. The payment couldn't be authorized.",
                            String_554: "(FR) That loyalty card number is not available. Please try a different card number.",
                            String_555: "(FR) The customer was not found. Please try again.",
                            String_556: "(FR) There was an error processing your request. Please try again later.",
                            String_557: "(FR) The transaction can't contain more than one loyalty payment line.",
                            String_558: "(FR) Amount due must be paid before checkout.",
                            String_559: "(FR) The payment information is either missing information or it is incorrect. Verify the payment information and then try again.",
                            String_560: "(FR) The address is either missing information or it is incorrect. Verify the address and then try again.",
                            String_561: "(FR) The amount exceeds the balance on the gift card. Enter a different amount and then try again.",
                            String_562: "(FR) You cannot perform two operations with the same gift card in a single transaction.",
                            String_563: "(FR) The quantity of item is not valid for the unit of measure. Please enter a different value and try again.",
                            String_564: "(FR) An item cannot be added to a sales transaction. The maximum quantity has been reached.",
                            String_565: "(FR) The unit of measure symbol is invalid.",
                            String_566: "(FR) The shopping cart is not found.",
                            String_567: "(FR) An error occurred while trying to unlock the gift card.",
                            String_568: "(FR) There was an error when changing the password.",
                            String_569: "(FR) There was an error when resetting the password.",
                            String_570: "(FR) There was a connectivity error. Please try again later.",
                            String_571: "(FR) You can't add a line item to the sale with a quantity that is less than or equal to zero. Enter a quantity that is greater than zero and then try again.",
                            String_572: "(FR) Sorry, the user session has expired or is invalid. Please logon again.",
                            String_573: "(FR) The amount cannot be zero or negative.",
                            String_574: "(FR) The password is invalid. Please try again.",
                            String_575: "(FR) Overpayment is not allowed for this payment method. Enter an amount that is either less than or equal to the amount due or change the payment method, and then try again.",
                            String_576: "(FR) The payment amount exceeds the maximum amount that is allowed per line.  Enter a different payment amount and then try again.",
                            String_577: "(FR) The payment amount exceeds the maximum amount that is allowed for the transaction.  Enter a different payment amount and then try again.",
                            String_578: "(FR) The payment amount is less than the minimum amount that is allowed per line. Enter a different payment amount and then try again.",
                            String_579: "(FR) The payment amount is less than the minimum amount that is allowed for the transaction. Enter a different payment amount and then try again.",
                            String_580: "(FR) The payment amount must be equal to or greater than the remaining balance. Enter a different payment amount and then try again.",
                            String_581: "(FR) Selected product has not been activated for sale.",
                            String_582: "(FR) Selected product is blocked and cannot be sold.",
                            String_583: "(FR) Invalid configuration, please try again.",
                            String_584: "(FR) A data validation error has occurred, please try again.",
                            String_585: "(FR) A storage error has occurred, please try again.",
                            String_586: "(FR) A communication error has occurred, please try again.",
                            String_587: "(FR) A real time service call failure has occurred, please try again.",
                            String_588: "(FR) Real time service call has timed out. Please try again.",
                            String_589: "(FR) Real time service exception. Please try again.",
                            String_590: "(FR) Real time service authentication failure. Please try again.",
                            String_591: "(FR) Real time service call is not supported. Please try again.",
                            String_592: "(FR) Real time service call is incorrect. Please try again.",
                            String_593: "(FR) Unable to find delivery preferences. Please try again.",
                            String_594: "(FR) The tender type configuration is not found. Please try again.",
                            String_595: "(FR) Payment connector is not found.",
                            String_596: "(FR) The given sales line is invalid. Required value is missing.",
                            String_597: "(FR) The delivery mode is invalid.",
                            String_598: "(FR) The email address is invalid.",
                            String_599: "(FR) The delivery preference type is invalid.",
                            String_600: "(FR) The transaction status is invalid.",
                            String_601: "(FR) The cart is not active. Please refresh the page and try again.",
                            String_602: "(FR) Multiple credit card payment is not supported.",
                            String_603: "(FR) The cart type is invalid. Please try again.",
                            String_604: "(FR) The sales line update is invalid. Please try again.",
                            String_605: "(FR) The product is invalid. Please try again.",
                            String_606: "(FR) Invalid cart inventory location id. Please try again.",
                            String_607: "(FR) Invalid cart lines aggregation error. Please try again.",
                            String_608: "(FR) Conflicting cart line operation. Please try again.",
                        };
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        Controls.ResourceStrings["en-us"] = {
                            String_1: "Shopping cart",
                            String_2: "Product details",
                            String_3: "Each",
                            String_4: "Quantity",
                            String_5: "Line total",
                            String_6: "Remove",
                            String_7: "Savings:",
                            String_8: "Update quantity",
                            String_9: "Order summary",
                            String_10: "Subtotal:",
                            String_11: "Shipping and handling:",
                            String_12: "Order total:",
                            String_13: "Total savings:",
                            String_14: "Next step",
                            String_15: "There are no items in your shopping cart. Please add items to the cart.",
                            String_16: "Delivery information",
                            String_17: "Delivery preference:",
                            String_18: "Shipping address",
                            String_19: "Shipping name",
                            String_20: "Country/region",
                            String_21: "United States",
                            String_22: "Address",
                            String_23: "City",
                            String_24: "State/province",
                            String_25: "ZIP/postal code",
                            String_26: "Shipping method",
                            String_27: "Get shipping options",
                            String_28: "Previous step",
                            String_29: "Name",
                            String_30: "Billing information",
                            String_31: "Contact information",
                            String_32: "Email address",
                            String_33: "Confirm email address",
                            String_34: "Payment method",
                            String_35: "Card number",
                            String_36: "Card type",
                            String_37: "Expiration month",
                            String_38: "Expiration year",
                            String_39: "CCID",
                            String_40: "What is this?",
                            String_41: "Payment amount",
                            String_42: "Billing address",
                            String_43: "Same as shipping address",
                            String_44: "Address2",
                            String_45: "Review and confirm",
                            String_46: "Order information",
                            String_47: "Edit",
                            String_48: "Credit card",
                            String_49: "Checkout",
                            String_50: "You have not added any promotion code to your order",
                            String_51: "Tax:",
                            String_52: "Submit order",
                            String_53: "Thank you for your order",
                            String_54: "Your order confirmation number is ",
                            String_55: "Street",
                            String_56: "State",
                            String_57: "Zipcode",
                            String_58: "Email",
                            String_59: "Payment",
                            String_60: "CardNumber",
                            String_61: "Please select shipping method",
                            String_62: "The confirm email address must match the email address.",
                            String_63: "Sorry, something went wrong. The shopping cart information couldn't be retrieved. Please refresh the page and try again.",
                            String_64: "Sorry, something went wrong. The product was not removed from the cart successfully. Please refresh the page and try again.",
                            String_65: "Sorry, something went wrong. The product quantity couldn't be updated. Please refresh the page and try again.",
                            String_66: "Sorry, something went wrong. Delivery methods could not be retrieved. Please refresh the page and try again.",
                            String_67: "Sorry, something went wrong. The shipping information was not stored successfully. Please refresh the page and try again.",
                            String_68: "Sorry, something went wrong. The payment card type information was not retrieved successfully. Please refresh the page and try again.",
                            String_69: "Sorry, something went wrong. The order submission was not successful. Please refresh the page and try again.",
                            String_70: "Invalid parameter",
                            String_71: "validatorType attribute is not provided for validator binding.",
                            String_72: "Use text characters only. Numbers, spaces, and special characters are not allowed.",
                            String_73: "Use text characters only. Numbers, spaces, and special characters are not allowed.",
                            String_74: "The quantity field cannot be empty",
                            String_75: "Select delivery method.",
                            String_76: "The email address is invalid.",
                            String_77: "Please enter the name.",
                            String_78: "Please enter the street number.",
                            String_79: "Please enter the address.",
                            String_80: "Please enter the city.",
                            String_81: "Please enter the zip/postal code.",
                            String_82: "Please enter the state.",
                            String_83: "Please enter the country.",
                            String_84: "Please specify a payment card name.",
                            String_85: "Please enter a valid card number.",
                            String_86: "Please enter a valid CCID.",
                            String_87: "Please specify a valid amount.",
                            String_88: "{0} PRODUCT(S)",
                            String_89: "Included",
                            String_90: "Color: {0}",
                            String_91: "Size: {0}",
                            String_92: "Style: {0}",
                            String_93: "Sorry, something went wrong. The promotion code could not be added successfully. Please refresh the page and try again.",
                            String_94: "Sorry, something went wrong. The promotion code could not be removed successfully. Please refresh the page and try again.",
                            String_95: "Apply",
                            String_96: "Promotion Codes",
                            String_97: "Please enter a promotion code",
                            String_98: "Sorry, something went wrong. The channel configuration could not be retrieved successfully. Please refresh the page and try again.",
                            String_99: "Ship items",
                            String_100: "Pick up in store",
                            String_101: "Select delivery options by item",
                            String_102: "Find a store",
                            String_103: "miles",
                            String_104: "Available stores",
                            String_105: "Store",
                            String_106: "Distance",
                            String_107: "Products are not available for pick up in the stores around the location you searched. Please update your delivery preferences and try again.",
                            String_108: "Sorry, something went wrong. An error occurred while trying to get stores. Please refresh the page and try again.",
                            String_109: "Sorry, we were not able to decipher the address you gave us.  Please enter a valid Address",
                            String_110: "Sorry, something went wrong. An error has occured while looking up the address you provided. Please refresh the page and try again.",
                            String_111: "Products are not available in this store",
                            String_112: "Product availability:",
                            String_113: "Products are not available in the selected store, Please select a different store",
                            String_114: "Please select a store for pick up",
                            String_115: "Store address",
                            String_116: "Send to me",
                            String_117: "Optional note",
                            String_118: "Please enter email address for gift card delivery",
                            String_119: "Sorry, something went wrong. An error occurred while trying to get the email address. Please enter the email address in the text box below",
                            String_120: "Enter the shipping address and then click get shipping options to view the shipping options that are available for your area.",
                            String_121: "Delivery method",
                            String_122: "Select your delivery preference",
                            String_123: "Cancel",
                            String_124: "Done",
                            String_125: "for product: {0}",
                            String_126: "Please select delivery preference for product {0}",
                            String_127: "Add credit card",
                            String_128: "Gift card",
                            String_129: "Add gift card",
                            String_130: "Loyalty card",
                            String_131: "Add loyalty card",
                            String_132: "Payment information",
                            String_133: "Payment total:",
                            String_134: "Order total:",
                            String_135: "Gift card does not exist",
                            String_136: "Gift card balance",
                            String_137: "Card details",
                            String_138: "Sorry, something went wrong. An error occurred while trying to get payment methods supported by the store. Please refresh the page and try again.",
                            String_139: "Please select payment method",
                            String_140: "The expiration date is not valid. Please select valid expiration month and year and then try again",
                            String_141: "Please enter a valid gift card number",
                            String_142: "Get gift card balance",
                            String_143: "Apply full amount",
                            String_144: "Please enter a gift card number",
                            String_145: "Sorry, something went wrong. An error occurred while trying to get gift card balance. Please refresh the page and try again.",
                            String_146: "Gift card payment amount cannot be zero",
                            String_147: "Gift card payment amount is more than order total",
                            String_148: "Gift card does not have sufficient balance",
                            String_149: "Payment amount is different from the order total",
                            String_150: "Sorry, something went wrong. An error occurred while trying to get loyalty card information. Please refresh the page and try again.",
                            String_151: "Please enter a valid loyalty card number",
                            String_152: "Loyalty card payment amount cannot be zero",
                            String_153: "Loyalty card payment amount is more than order total",
                            String_154: "The loyalty card is blocked",
                            String_155: "The loyalty card is not allowed for payment",
                            String_156: "The loyalty payment amount exceeds what is allowed for this loyalty card in this transaction",
                            String_157: "The loyalty card number does not exist",
                            String_158: "Please select delivery preference",
                            String_159: "Please select a delivery preference...",
                            String_160: "Sorry, something went wrong. An error occurred while trying to get delivery methods information. Please refresh the page and try again.",
                            String_161: "Select address...",
                            String_162: "You have not added loyalty card number to your order",
                            String_163: "Enter a reward card for the current order. You can include only one reward card per order",
                            String_164: "Sorry, something went wrong. An error occurred while trying to update reward card id in cart. Please refresh the page and try again.",
                            String_165: "Sorry, something went wrong. An error occurred while retrieving the country region information. Please refresh the page and try again.",
                            String_166: "TBD",
                            String_167: "Mini Cart",
                            String_168: "Ordering FAQ",
                            String_169: "Return policy",
                            String_170: "Store locator tool",
                            String_171: "Continue shopping",
                            String_172: "Cart Order Total",
                            String_173: "View full cart contents",
                            String_174: "Quantity:",
                            String_175: "Added to your cart:",
                            String_176: "Loading ...",
                            String_177: "Sorry, something went wrong. The cart's promotion information couldn't be retrieved. Please refresh the page and try again.",
                            String_178: "Delivery method",
                            String_179: "Updating shopping cart ...",
                            String_180: "Submitting order ...",
                            String_181: "Discount code",
                            String_182: "Add coupon code",
                            String_183: "Enter a discount code",
                            String_184: "Please enter a valid discount code",
                            String_185: "Sorry, something went wrong. An error occurred while retrieving the state/province information. Please refresh the page and try again.",
                            String_186: "Edit reward card",
                            String_187: "Reward card",
                            String_188: "Add discount code",
                            String_189: "Select country/region",
                            String_190: "Select state/province",
                            String_191: "You have selected multiple delivery methods for this order",
                            String_192: "01-January",
                            String_193: "02-February",
                            String_194: "03-March",
                            String_195: "04-April",
                            String_196: "05-May",
                            String_197: "06-June",
                            String_198: "07-July",
                            String_199: "08-August",
                            String_200: "09-September",
                            String_201: "10-October",
                            String_202: "11-November",
                            String_203: "12-December",
                            String_204: "The number string has more than one decimal operator.",
                            String_205: "Estimated total: ",
                            String_206: "The actual tax amount will be calculated based on the applicable state and local sales taxes when your delivery preference is selected.",
                            String_207: "The shipping and handling charges will be calculated based on delivery preference and recipient address(es) provided.",
                            String_208: "Included",
                            String_209: "Sorry, something went wrong. An error occurred while retrieving signed-in customer's information. Please refresh the page and try again.",
                            String_210: "Sorry, something went wrong. We were unable to obtain the card payment accept result. Please refresh the page and try again.",
                            String_211: "Sorry, something went wrong. We were unable to obtain the card payment accept page url. Please refresh the page and try again.",
                            String_212: "The specified zip code is invalid.",
                            String_213: "The specified state is invalid.",
                            String_214: "The specified city is invalid.",
                            String_215: "The shipping address is not valid.",
                            String_216: "The loyalty card number that you entered is invalid.",
                            String_217: "The loyalty card is blocked.",
                            String_218: "The loyalty card can't be added to the transaction because the customer on the loyalty card is different than the customer on the transaction. Either change the customer on the transaction or select a different loyalty card.",
                            String_219: "This loyalty card is not eligible to redeem loyalty points for this transaction.",
                            String_220: "There are not enough loyalty reward points available on the loyalty card.",
                            String_221: "The service call is taking longer than expected. Please wait for it to respond or refresh the page and try again.",
                            String_222: "IMPORTANT NOTICE:",
                            String_223: "This form is provided by your designated third-party payment provider.  By clicking \"Next\" you acknowledge your information will be transmitted directly to your payment provider, and will be handled in accordance with the terms and conditions and privacy statement that you agreed to with your payment provider.  Microsoft does not collect, store, or transmit any of your payment card information, or provide rights for third-party products or services.",
                            String_224: "Order date",
                            String_225: "Order number",
                            String_226: "Order status",
                            String_227: "Recent orders",
                            String_228: "My account",
                            String_229: "Order history",
                            String_230: "Sorry, something went wrong. We were unable to obtain your order history. Please refresh the page and try again.",
                            String_231: "There are no orders to display.",
                            String_232: "Sorry, something went wrong. An error occurred while trying to update loyalty card information. Please refresh the page and try again.",
                            String_233: "Sorry, something went wrong. An error occurred while trying to get user login information. Please refresh the page and try again.",
                            String_234: "Address book",
                            String_235: "There are no addresses.",
                            String_236: "Order detail: ",
                            String_237: "Sorry, something went wrong. An error occurred while trying to get the order details information. Please refresh the page and try again.",
                            String_238: "Line total:",
                            String_239: "Delivery method: ",
                            String_240: "Processing",
                            String_241: "Order number: ",
                            String_242: "Order status: ",
                            String_301: "Invalid Operation",
                            String_302: "The application has encountered an unknown error.",
                            String_303: "Errors found, check the detailed error results for additional information.",
                            String_304: "Errors found, payment not authorized. Check the detailed error results for additional information.",
                            String_305: "User aborted",
                            String_306: "Locale not supported",
                            String_307: "Invalid merchant property",
                            String_308: "There was an error communicating with the payment provider. Retry your request.",
                            String_309: "The specified card type is not supported",
                            String_310: "Voice authorization not supported.",
                            String_311: "Re-authorizations are not supported by this payment provider.",
                            String_312: "The payment provider assigned does not support multiple captures against a single authorization.",
                            String_313: "Batch capture is not supported.",
                            String_314: "Invalid CurrencyCode",
                            String_315: "Invalid CountryOrRegion",
                            String_316: "Cannot reauthorize a transaction against which funds have already been captured.",
                            String_317: "Cannot reauthorize a transaction that has been voided.",
                            String_318: "The payment provider does not support immediate capture.",
                            String_319: "Credit card expired",
                            String_320: "Refer to card issuer",
                            String_321: "There was no reply from the payment provider for this transaction. Retry the operation.",
                            String_322: "The transaction was declined by the payment provider. The payment provider requests that you hold the card and call the card issuer.",
                            String_323: "Invalid Amount",
                            String_324: "AccountNumber has an invalid length",
                            String_325: "The payment provider rejected the transaction with the following message: The transaction has already been reversed",
                            String_326: "Invalid PIN",
                            String_327: "Invalid AccountNumber",
                            String_328: "Invalid CVV",
                            String_329: "Cashback is not supported for this transaction.",
                            String_330: "Invalid CardType",
                            String_331: "The transaction was declined by the payment provider.",
                            String_332: "Encryption error, check the detailed error results for additional information.",
                            String_333: "The payment provider rejected the transaction with this message: No action taken",
                            String_334: "The payment provider rejected the transaction with this message: No such issuer",
                            String_335: "Invalid PIN. You have exceeded the allowed number of attempts to authenticate and this account has been locked.",
                            String_336: "The payment provider rejected the transaction with this message: Security violation at the processor",
                            String_337: "The payment provider rejected the transaction with this message: Service not allowed",
                            String_338: "The payment provider rejected the transaction with this message: Customer stopped recurring",
                            String_339: "The transaction was rejected due to an invalid PIN.",
                            String_340: "The transaction was rejected due to an invalid card verification value (CVV/CVV2).",
                            String_341: "The payment transaction duplicates a previous payment transaction. The duplicate transaction was not processed.",
                            String_342: "The payment provider rejected the transaction with this message: Re-Enter",
                            String_343: "Amount exceeds the maximum allowed amount for a payment",
                            String_344: "Authorization for this payment transaction has expired. You will need to re-authorize before capturing funds.",
                            String_345: "The payment provider rejected the capture because the entire amount of the original authorization has already been captured.",
                            String_346: "Cannot process payments against a transaction that has been voided.",
                            String_347: "The payment provider rejected the batch as a duplicate.",
                            String_348: "Authorization failed",
                            String_349: "The merchant configuration is invalid.",
                            String_350: "Invalid Expiration Date",
                            String_351: "Invalid Firstname",
                            String_352: "Invalid LastName",
                            String_353: "The payment provider declined this transaction.",
                            String_354: "Invalid Address",
                            String_355: "The transaction was rejected because a Card Verification Value (CVV/CVV2) is required and was not provided.",
                            String_356: "CardType is an unsupported type",
                            String_357: "The payment provider rejected the transaction because the invoice number is a duplicate of a previous transaction.",
                            String_358: "Payment provider indicated that this transaction appears to be a duplicate. The transaction was not processed.",
                            String_359: "The payment provider selected for this payment type requires refunds to link to an existing payment capture. Reference a capture transaction using the CaptureTransactionGuid field.",
                            String_360: "Encryption unavailable",
                            String_361: "The transaction was declined due to an invalid card verification value (CVV/CVV2).",
                            String_362: "The Merchant Id is invalid.",
                            String_363: "The transaction was not allowed.",
                            String_364: "The terminal has not been registered. Check your terminal Id setting.",
                            String_365: "Invalid effective date",
                            String_366: "Insufficient funds for the transaction.",
                            String_367: "The payment provider rejected the transaction with this message: Reauthorization maximum reached",
                            String_368: "The payment provider rejected the transaction with this message: Reauthorization Not Allowed",
                            String_369: "Invalid Date of Birth",
                            String_370: "The transaction requires a smaller amount.",
                            String_371: "Host key error",
                            String_372: "Invalid cashback amount",
                            String_373: "Invalid transaction",
                            String_374: "Immediate transaction type required",
                            String_375: "Immediate transaction type required with MAC",
                            String_376: "MAC Required for this transaction",
                            String_377: "The bank card property in the request has not been set.",
                            String_378: "Invalid request message sent.",
                            String_379: "Invalid transaction fee",
                            String_380: "No checking account",
                            String_381: "No savings account",
                            String_382: "Restricted card temporarily disallowed from interchange.",
                            String_383: "MAC security failure",
                            String_384: "Payment transaction exceeds withdrawal frequency limit.",
                            String_385: "Invalid capture date",
                            String_386: "No keys available",
                            String_387: "KME sync error",
                            String_388: "KPE sync error",
                            String_389: "KMAC sync error",
                            String_390: "Payment transaction has exceeded the limit of resubmits.",
                            String_391: "System problem error",
                            String_392: "Account number not found for row {0}.",
                            String_393: "Invalid TokenInfo parameter for row {0}.",
                            String_394: "Exception {0} occurred for row {1}.",
                            String_395: "The transaction amount exceeds the remaining authorized amount.",
                            String_396: "Invalid TenderAccountNumber",
                            String_397: "The card track data is invalid.",
                            String_398: "The payment accept page result access code is invalid.",
                            String_399: "Sorry, something went wrong. Payment exception has occured. Please refresh the page and try again or try another payment method.",
                            String_501: "Sorry something went wrong, we cannot process your request at this time. Please try again later.",
                            String_502: "The specified ID already exists.",
                            String_503: "There is an insufficient quantity of the product on-hand.",
                            String_504: "The cart line add is not valid.",
                            String_505: "The format of the specified data is not valid.",
                            String_506: "The change version requested and stored do not match.",
                            String_507: "The specified ID was not found.",
                            String_508: "Required value is missing.",
                            String_509: "The specified request type is unknown.",
                            String_510: "The language specified is not supported.",
                            String_511: "The specified value is out of range.",
                            String_512: "The cart was updated by another session. Please refresh and retry.",
                            String_513: "A communication error occurred.",
                            String_514: "Runtime components for the application are missing.",
                            String_515: "Configuration settings are missing.",
                            String_516: "Duplicate notification handlers have been encountered.",
                            String_517: "The unit of measure is not set for the product.",
                            String_518: "An error occurred communicating with an external provider.",
                            String_519: "Data returned from headquarters could not be parsed.",
                            String_520: "There is nothing in the cart. Add a product to the cart, and then try again.",
                            String_521: "The channel configuration is not properly configured.",
                            String_522: "The configuration key is not valid.",
                            String_523: "The database connection string is not valid.",
                            String_524: "The runtime pipeline has not been correctly configured.",
                            String_525: "One or more providers are not correctly configured.",
                            String_526: "The runtime context is not valid.  This is most likely a coding error.",
                            String_527: "The response returned from the server is not valid.",
                            String_528: "One or more runtime components failed to initialize.",
                            String_529: "The service cannot be found.",
                            String_530: "The address provided does not match any sales tax group. This is likely a sales tax group configuration issue.",
                            String_531: "No default notification handler has been configured.",
                            String_532: "The delivery options could not be found.",
                            String_533: "No inventory can be found for this product.",
                            String_534: "A validation error occurred.",
                            String_535: "The coupon is only valid for the current session.",
                            String_536: "A database error has occurred.",
                            String_537: "The discount amount for a line item has changed.",
                            String_538: "An ID mismatch has occurred.",
                            String_539: "There is an insufficient quantity of the product available.",
                            String_540: "The price cannot be found for the product.",
                            String_541: "The request passed to the service is not valid.",
                            String_542: "A SQL command is not valid. This is most likely a coding error.",
                            String_543: "One or more products are discontinued for the given channel.",
                            String_544: "There is a mismatch between the object to be saved and the object in the database. Please try again.",
                            String_545: "There is an error communicating with the provider.",
                            String_546: "The requested product is out of stock.",
                            String_547: "The unit of measure conversion cannot be found.",
                            String_548: "Default customer cannot be found.",
                            String_549: "The given currency is not supported.",
                            String_550: "The number that was entered for the credit card number isn't valid. Enter a valid card number.",
                            String_551: "The channel is invalid.",
                            String_552: "The server api version is not supported",
                            String_553: "Declined. The payment couldn't be authorized.",
                            String_554: "That loyalty card number is not available. Please try a different card number.",
                            String_555: "The customer was not found. Please try again.",
                            String_556: "There was an error processing your request. Please try again later.",
                            String_557: "The transaction can't contain more than one loyalty payment line.",
                            String_558: "Amount due must be paid before checkout.",
                            String_559: "The payment information is either missing information or it is incorrect. Verify the payment information and then try again.",
                            String_560: "The address is either missing information or it is incorrect. Verify the address and then try again.",
                            String_561: "The amount exceeds the balance on the gift card. Enter a different amount and then try again.",
                            String_562: "You cannot perform two operations with the same gift card in a single transaction.",
                            String_563: "The quantity of item is not valid for the unit of measure. Please enter a different value and try again.",
                            String_564: "An item cannot be added to a sales transaction. The maximum quantity has been reached.",
                            String_565: "The unit of measure symbol is invalid.",
                            String_566: "The shopping cart is not found.",
                            String_567: "An error occurred while trying to unlock the gift card.",
                            String_568: "There was an error when changing the password.",
                            String_569: "There was an error when resetting the password.",
                            String_570: "There was a connectivity error. Please try again later.",
                            String_571: "You can't add a line item to the sale with a quantity that is less than or equal to zero. Enter a quantity that is greater than zero and then try again.",
                            String_572: "Sorry, the user session has expired or is invalid. Please logon again.",
                            String_573: "The amount cannot be zero or negative.",
                            String_574: "The password is invalid. Please try again.",
                            String_575: "Overpayment is not allowed for this payment method. Enter an amount that is either less than or equal to the amount due or change the payment method, and then try again.",
                            String_576: "The payment amount exceeds the maximum amount that is allowed per line.  Enter a different payment amount and then try again.",
                            String_577: "The payment amount exceeds the maximum amount that is allowed for the transaction.  Enter a different payment amount and then try again.",
                            String_578: "The payment amount is less than the minimum amount that is allowed per line. Enter a different payment amount and then try again.",
                            String_579: "The payment amount is less than the minimum amount that is allowed for the transaction. Enter a different payment amount and then try again.",
                            String_580: "The payment amount must be equal to or greater than the remaining balance. Enter a different payment amount and then try again.",
                            String_581: "Selected product has not been activated for sale.",
                            String_582: "Selected product is blocked and cannot be sold.",
                            String_583: "Invalid configuration, please try again.",
                            String_584: "A data validation error has occurred, please try again.",
                            String_585: "A storage error has occurred, please try again.",
                            String_586: "A communication error has occurred, please try again.",
                            String_587: "A real time service call failure has occurred, please try again.",
                            String_589: "Real time service exception. Please try again.",
                            String_590: "Real time service authentication failure. Please try again.",
                            String_591: "Real time service call is not supported. Please try again.",
                            String_592: "Real time service call is incorrect. Please try again.",
                            String_593: "Unable to find delivery preferences. Please try again.",
                            String_594: "The tender type configuration is not found. Please try again.",
                            String_595: "Payment connector is not found.",
                            String_596: "The given sales line is invalid. Required value is missing.",
                            String_597: "The delivery mode is invalid.",
                            String_598: "The email address is invalid.",
                            String_599: "The delivery preference type is invalid.",
                            String_600: "The transaction status is invalid.",
                            String_601: "The cart is not active. Please refresh the page and try again.",
                            String_602: "Multiple credit card payment is not supported.",
                            String_603: "The cart type is invalid. Please try again.",
                            String_604: "The sales line update is invalid. Please try again.",
                            String_605: "The product is invalid. Please try again.",
                            String_606: "Invalid cart inventory location id. Please try again.",
                            String_607: "Invalid cart lines aggregation error. Please try again.",
                            String_608: "Conflicting cart line operation. Please try again.",
                            String_609: "The gift card amount cannot be zero.",
                        };
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../../Resources/Resources.ts" />
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Microsoft;
(function (Microsoft) {
    var Dynamics;
    (function (Dynamics) {
        var Retail;
        (function (Retail) {
            var Ecommerce;
            (function (Ecommerce) {
                var Sdk;
                (function (Sdk) {
                    var Controls;
                    (function (Controls) {
                        var stringValidationErrorMessage = Controls.Resources.String_72;
                        var stringValidationRegex = Controls.Resources.String_73;
                        ;
                        var FieldValidator = (function () {
                            function FieldValidator(params) {
                                this._validationAttributes = params;
                            }
                            FieldValidator.prototype.setValidationAttributes = function (element) {
                                for (var attrName in this._validationAttributes) {
                                    if (attrName != "title") {
                                        var value = this._validationAttributes[attrName];
                                        if (value) {
                                            element.setAttribute(attrName, value);
                                        }
                                        if (this._validationAttributes.required !== true) {
                                            element.removeAttribute("required");
                                        }
                                    }
                                }
                            };
                            FieldValidator.prototype.setTitleAttributeIfInvalid = function (element) {
                                var value = this._validationAttributes["title"];
                                if (value && element.getAttribute("msax-isValid") == "false") {
                                    element.setAttribute("title", value);
                                }
                                else {
                                    element.removeAttribute("title");
                                }
                            };
                            return FieldValidator;
                        })();
                        Controls.FieldValidator = FieldValidator;
                        var EntityValidatorBase = (function () {
                            function EntityValidatorBase() {
                            }
                            EntityValidatorBase.prototype.setValidationAttributes = function (element, fieldName) {
                                var fieldValidator = this[fieldName];
                                if (fieldValidator) {
                                    fieldValidator.setValidationAttributes(element);
                                }
                            };
                            return EntityValidatorBase;
                        })();
                        Controls.EntityValidatorBase = EntityValidatorBase;
                        var ShoppingCartItemValidator = (function (_super) {
                            __extends(ShoppingCartItemValidator, _super);
                            function ShoppingCartItemValidator() {
                                _super.call(this);
                                this.Quantity = new FieldValidator({
                                    maxLength: 3, required: true, title: Controls.Resources.String_74 });
                            }
                            return ShoppingCartItemValidator;
                        })(EntityValidatorBase);
                        Controls.ShoppingCartItemValidator = ShoppingCartItemValidator;
                        var SelectedOrderDeliveryOptionValidator = (function (_super) {
                            __extends(SelectedOrderDeliveryOptionValidator, _super);
                            function SelectedOrderDeliveryOptionValidator() {
                                _super.call(this);
                                this.DeliveryModeId = new FieldValidator({
                                    required: true, title: Controls.Resources.String_75
                                });
                            }
                            return SelectedOrderDeliveryOptionValidator;
                        })(EntityValidatorBase);
                        Controls.SelectedOrderDeliveryOptionValidator = SelectedOrderDeliveryOptionValidator;
                        var CustomerValidator = (function (_super) {
                            __extends(CustomerValidator, _super);
                            function CustomerValidator() {
                                _super.call(this);
                                this.FirstName = new FieldValidator({ maxLength: 25, required: true, title: stringValidationErrorMessage, pattern: stringValidationRegex });
                                this.MiddleName = new FieldValidator({ maxLength: 25, title: stringValidationErrorMessage, pattern: stringValidationRegex });
                                this.LastName = new FieldValidator({ maxLength: 25, required: true, title: stringValidationErrorMessage, pattern: stringValidationRegex });
                                this.Name = new FieldValidator({ maxLength: 100, required: true });
                            }
                            return CustomerValidator;
                        })(EntityValidatorBase);
                        Controls.CustomerValidator = CustomerValidator;
                        var AddressValidator = (function (_super) {
                            __extends(AddressValidator, _super);
                            function AddressValidator() {
                                _super.call(this);
                                this.Phone = new FieldValidator({ maxLength: 20 });
                                this.Url = new FieldValidator({ maxLength: 255 });
                                this.Email = new FieldValidator({ maxLength: 80, required: true, title: Controls.Resources.String_76, pattern: "^[-0-9a-zA-Z.+_]+@[-0-9a-zA-Z.+_]+.[a-zA-Z]{2,4}$" });
                                this.Name = new FieldValidator({ maxLength: 60, required: true, title: Controls.Resources.String_77 });
                                this.StreetNumber = new FieldValidator({ maxLength: 20, title: Controls.Resources.String_78 });
                                this.Street = new FieldValidator({ maxLength: 250, required: true, title: Controls.Resources.String_79 });
                                this.City = new FieldValidator({ maxLength: 60, required: true, title: Controls.Resources.String_80 });
                                this.ZipCode = new FieldValidator({ maxLength: 10, required: true, title: Controls.Resources.String_81 });
                                this.State = new FieldValidator({ maxLength: 10, required: true, title: Controls.Resources.String_82 });
                                this.Country = new FieldValidator({ required: true, title: Controls.Resources.String_83 });
                            }
                            return AddressValidator;
                        })(EntityValidatorBase);
                        Controls.AddressValidator = AddressValidator;
                        var PaymentCardTypeValidator = (function (_super) {
                            __extends(PaymentCardTypeValidator, _super);
                            function PaymentCardTypeValidator() {
                                _super.call(this);
                                this.NameOnCard = new FieldValidator({ maxLength: 100, required: true, title: Controls.Resources.String_84 });
                                this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Controls.Resources.String_85 });
                                this.CCID = new FieldValidator({ maxLength: 50, required: true, title: Controls.Resources.String_86, pattern: "^[0-9]{3,4}$" });
                                this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Controls.Resources.String_87, pattern: "\w+([0123456789.]\w+)*" });
                            }
                            return PaymentCardTypeValidator;
                        })(EntityValidatorBase);
                        Controls.PaymentCardTypeValidator = PaymentCardTypeValidator;
                        var GiftCardTypeValidator = (function (_super) {
                            __extends(GiftCardTypeValidator, _super);
                            function GiftCardTypeValidator() {
                                _super.call(this);
                                this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Controls.Resources.String_141 });
                                this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Controls.Resources.String_87 });
                            }
                            return GiftCardTypeValidator;
                        })(EntityValidatorBase);
                        Controls.GiftCardTypeValidator = GiftCardTypeValidator;
                        var LoyaltyCardTypeValidator = (function (_super) {
                            __extends(LoyaltyCardTypeValidator, _super);
                            function LoyaltyCardTypeValidator() {
                                _super.call(this);
                                this.CardNumber = new FieldValidator({ maxLength: 30, required: true, title: Controls.Resources.String_151 });
                                this.PaymentAmount = new FieldValidator({ maxLength: 100, required: true, title: Controls.Resources.String_87 });
                            }
                            return LoyaltyCardTypeValidator;
                        })(EntityValidatorBase);
                        Controls.LoyaltyCardTypeValidator = LoyaltyCardTypeValidator;
                        var DiscountCardTypeValidator = (function (_super) {
                            __extends(DiscountCardTypeValidator, _super);
                            function DiscountCardTypeValidator() {
                                _super.call(this);
                                this.CardNumber = new FieldValidator({ maxLength: 100, required: true, title: Controls.Resources.String_184 });
                            }
                            return DiscountCardTypeValidator;
                        })(EntityValidatorBase);
                        Controls.DiscountCardTypeValidator = DiscountCardTypeValidator;
                    })(Controls = Sdk.Controls || (Sdk.Controls = {}));
                })(Sdk = Ecommerce.Sdk || (Ecommerce.Sdk = {}));
            })(Ecommerce = Retail.Ecommerce || (Retail.Ecommerce = {}));
        })(Retail = Dynamics.Retail || (Dynamics.Retail = {}));
    })(Dynamics = Microsoft.Dynamics || (Microsoft.Dynamics = {}));
})(Microsoft || (Microsoft = {}));
/// <reference path="../../KnockoutJS.d.ts" />
/// <reference path="../Helpers/Utils.ts" />
/// <reference path="../../Resources/Resources.ts" />
ko.bindingHandlers.validator = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here
        var binding = ko.utils.unwrapObservable(valueAccessor()) || {};
        if (!Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrWhiteSpace(binding.field)) {
            var valueObject = binding.data ? binding.data[binding.field] : bindingContext.$data[binding.field];
            var observableValueObject;
            if (ko.isObservable(valueObject)) {
                observableValueObject = valueObject;
            }
            else {
                observableValueObject = ko.observable(valueObject);
                observableValueObject.subscribe(function (newValue) {
                    if (Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrUndefined(binding.data)) {
                        bindingContext.$data[binding.field] = newValue;
                    }
                    else {
                        binding.data[binding.field] = newValue;
                    }
                });
            }
            ko.applyBindingsToNode(element, { value: observableValueObject });
        }
        if (Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrUndefined(binding.validatorType)) {
            throw Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources.String_71;
        }
        var validator = Object.create(Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls[binding.validatorType].prototype);
        validator.constructor.apply(validator);
        var field = binding.validatorField ? binding.validatorField : binding.field;
        validator.setValidationAttributes(element, field);
        var $element = $(element);
        $element.attr("msax-isValid", true);
        $element.change(function (eventObject) {
            if (!('checkValidity' in eventObject.currentTarget)) {
                eventObject.currentTarget.checkValidity = function () {
                    var valid = true, required = eventObject.currentTarget.getAttribute("required"), minLength = eventObject.currentTarget.getAttribute("minlength"), maxLength = eventObject.currentTarget.getAttribute("maxlength"), pattern = eventObject.currentTarget.getAttribute("pattern"), value = eventObject.currentTarget.value, type = eventObject.currentTarget.getAttribute("type"), option = (type === "checkbox" || type === "radio");
                    if (eventObject.currentTarget.disabled) {
                        return valid;
                    }
                    valid = valid && (!required ||
                        (option && eventObject.currentTarget.checked) ||
                        (!option && value !== ""));
                    valid = valid && (option ||
                        ((!minLength || value.length >= minLength) &&
                            (!maxLength || value.length <= maxLength)));
                    if (valid && pattern) {
                        pattern = new RegExp(pattern);
                        valid = pattern.test(value);
                    }
                    return valid;
                };
            }
            var isValid = eventObject.currentTarget.checkValidity();
            if (eventObject.currentTarget.type === "select-one" && eventObject.currentTarget.selectedIndex != 0) {
                isValid = true;
            }
            if (isValid && binding.validate) {
                try {
                    isValid = binding.validate.call(viewModel, eventObject.currentTarget);
                }
                catch (ex) {
                    isValid = false;
                }
            }
            $element.attr("msax-isValid", isValid);
            if (eventObject.currentTarget.type === "radio") {
                var $label = $element.parent().find("[for=" + eventObject.currentTarget.id + "]");
                $label.attr("msax-isValid", isValid);
            }
            if (!Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrWhiteSpace(validator[field])) {
                validator[field].setTitleAttributeIfInvalid(element);
            }
            return isValid;
        });
    }
};
ko.bindingHandlers.submitIfValid = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var binding = ko.utils.unwrapObservable(valueAccessor()) || {};
        var $element = $(element);
        $element.click(function (eventObject) {
            eventObject.preventDefault();
            var container;
            if (Microsoft.Dynamics.Retail.Ecommerce.Utils.isNullOrWhiteSpace(binding.containerSelector) || binding.containerSelector.length == 0) {
                var containerObservable = binding.containerSelector;
                container = containerObservable();
            }
            else {
                container = binding.containerSelector;
            }
            var $wrapper = $element.closest(container);
            if ($wrapper.length === 0) {
                $wrapper = $(container);
            }
            $wrapper.find("input,select").each(function (index, elem) {
                $(elem).change();
            });
            var $invalidFields = $wrapper.find("[msax-isValid=false]");
            $invalidFields.first().focus();
            $invalidFields.first().select();
            if ($invalidFields.length === 0) {
                var isValid = true;
                if (binding.validate) {
                    isValid = binding.validate.call(viewModel, $wrapper);
                }
                if (isValid) {
                    binding.submit.call(viewModel, eventObject);
                }
            }
        });
    }
};
/// <reference path="../../KnockoutJS.d.ts" />
ko.bindingHandlers.resx = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here
        var binding = ko.utils.unwrapObservable(valueAccessor()) || {};
        for (var memberName in binding) {
            switch (memberName) {
                case "textContent":
                    element.textContent = Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources[binding[memberName]];
                    break;
                case "label":
                    element.label = Microsoft.Dynamics.Retail.Ecommerce.Sdk.Controls.Resources[binding[memberName]];
                    break;
            }
        }
    }
};
//# sourceMappingURL=e:/bt/129158/source/frameworks/retailrain/components/apps/web/platform/ecommerce.sdk.controls/Scripts.js.map
// SIG // Begin signature block
// SIG // MIIdnAYJKoZIhvcNAQcCoIIdjTCCHYkCAQExCzAJBgUr
// SIG // DgMCGgUAMGcGCisGAQQBgjcCAQSgWTBXMDIGCisGAQQB
// SIG // gjcCAR4wJAIBAQQQEODJBs441BGiowAQS9NQkAIBAAIB
// SIG // AAIBAAIBAAIBADAhMAkGBSsOAwIaBQAEFFdBcb0qzdmn
// SIG // fYzZZV7QXrgrDoERoIIYZDCCBMMwggOroAMCAQICEzMA
// SIG // AACampsWwoPa1cIAAAAAAJowDQYJKoZIhvcNAQEFBQAw
// SIG // dzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWlj
// SIG // cm9zb2Z0IFRpbWUtU3RhbXAgUENBMB4XDTE2MDMzMDE5
// SIG // MjEyOVoXDTE3MDYzMDE5MjEyOVowgbMxCzAJBgNVBAYT
// SIG // AlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAwDgYDVQQH
// SIG // EwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3NvZnQgQ29y
// SIG // cG9yYXRpb24xDTALBgNVBAsTBE1PUFIxJzAlBgNVBAsT
// SIG // Hm5DaXBoZXIgRFNFIEVTTjpCMUI3LUY2N0YtRkVDMjEl
// SIG // MCMGA1UEAxMcTWljcm9zb2Z0IFRpbWUtU3RhbXAgU2Vy
// SIG // dmljZTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoC
// SIG // ggEBAKZGcyHKAK1+KMPlE5szicc4CJIlJq0R/J8UFYJz
// SIG // YmMl8u5Me1+ZqDys5iCAV+aHEnUP3noHraQ8R7DXhYSg
// SIG // Tpdd35govgBRMWpxghNHe/vJe/YXSUkkzhe2TXlHhE1j
// SIG // j+O0JQyknC4q9qi2dcccePDGAKm0jt9MuccG/XAq+I7Q
// SIG // IR6DgWUMnECilK4qJilajEqeW2FMnFSesDzqkidwXk7j
// SIG // J2Li4DZKnPXh/Vs33s9dAcsKdcz83tvYtINUy3uDKYZR
// SIG // ECNHwStxzK+Wzlx8yprFXADBj2rK1JKn2K/rvhWbtKgd
// SIG // xGuEfFh0sDZkj9KCLPgMuSwKVnof6AmHqQbfHNUCAwEA
// SIG // AaOCAQkwggEFMB0GA1UdDgQWBBQmmgbvkXTwOgin21sU
// SIG // 7d0HCiAvCTAfBgNVHSMEGDAWgBQjNPjZUkZwCu1A+3b7
// SIG // syuwwzWzDzBUBgNVHR8ETTBLMEmgR6BFhkNodHRwOi8v
// SIG // Y3JsLm1pY3Jvc29mdC5jb20vcGtpL2NybC9wcm9kdWN0
// SIG // cy9NaWNyb3NvZnRUaW1lU3RhbXBQQ0EuY3JsMFgGCCsG
// SIG // AQUFBwEBBEwwSjBIBggrBgEFBQcwAoY8aHR0cDovL3d3
// SIG // dy5taWNyb3NvZnQuY29tL3BraS9jZXJ0cy9NaWNyb3Nv
// SIG // ZnRUaW1lU3RhbXBQQ0EuY3J0MBMGA1UdJQQMMAoGCCsG
// SIG // AQUFBwMIMA0GCSqGSIb3DQEBBQUAA4IBAQCJehwGFIbD
// SIG // v+5TfA//GKMWAGxUw9KZZvNqxbNTH3/VgV9R8/z6Lqiv
// SIG // 0Y0RH9q3RKNwAhBNsIT2njVXk4PeJqyb4884skOIK8vl
// SIG // V0vWUmtcbTARAu+pUZbB4oK/Z6uaECCEFKny/OromIJS
// SIG // dXwD3txRJK1umXshuqEqLPVjxAE01+WgDEnUCt1uAQux
// SIG // L2lxU/GPEcPl2w0LfSyUhk1nF3nYKHrloO5UvDdy8ZqL
// SIG // 1Hc4YFOvg2ScMl6+Vy6dpeZ78el6NHeRHnRMqsdL59xq
// SIG // 4XlayVog0TOb5ffjo7l67nWYUo/ViOKrtyqsfoqBKRvR
// SIG // cKkPD7NmpVq1jr1cvPdVvPkQMIIGBzCCA++gAwIBAgIK
// SIG // YRZoNAAAAAAAHDANBgkqhkiG9w0BAQUFADBfMRMwEQYK
// SIG // CZImiZPyLGQBGRYDY29tMRkwFwYKCZImiZPyLGQBGRYJ
// SIG // bWljcm9zb2Z0MS0wKwYDVQQDEyRNaWNyb3NvZnQgUm9v
// SIG // dCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkwHhcNMDcwNDAz
// SIG // MTI1MzA5WhcNMjEwNDAzMTMwMzA5WjB3MQswCQYDVQQG
// SIG // EwJVUzETMBEGA1UECBMKV2FzaGluZ3RvbjEQMA4GA1UE
// SIG // BxMHUmVkbW9uZDEeMBwGA1UEChMVTWljcm9zb2Z0IENv
// SIG // cnBvcmF0aW9uMSEwHwYDVQQDExhNaWNyb3NvZnQgVGlt
// SIG // ZS1TdGFtcCBQQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IB
// SIG // DwAwggEKAoIBAQCfoWyx39tIkip8ay4Z4b3i48WZUSNQ
// SIG // rc7dGE4kD+7Rp9FMrXQwIBHrB9VUlRVJlBtCkq6YXDAm
// SIG // 2gBr6Hu97IkHD/cOBJjwicwfyzMkh53y9GccLPx754gd
// SIG // 6udOo6HBI1PKjfpFzwnQXq/QsEIEovmmbJNn1yjcRlOw
// SIG // htDlKEYuJ6yGT1VSDOQDLPtqkJAwbofzWTCd+n7Wl7Po
// SIG // IZd++NIT8wi3U21StEWQn0gASkdmEScpZqiX5NMGgUqi
// SIG // +YSnEUcUCYKfhO1VeP4Bmh1QCIUAEDBG7bfeI0a7xC1U
// SIG // n68eeEExd8yb3zuDk6FhArUdDbH895uyAc4iS1T/+QXD
// SIG // wiALAgMBAAGjggGrMIIBpzAPBgNVHRMBAf8EBTADAQH/
// SIG // MB0GA1UdDgQWBBQjNPjZUkZwCu1A+3b7syuwwzWzDzAL
// SIG // BgNVHQ8EBAMCAYYwEAYJKwYBBAGCNxUBBAMCAQAwgZgG
// SIG // A1UdIwSBkDCBjYAUDqyCYEBWJ5flJRP8KuEKU5VZ5KSh
// SIG // Y6RhMF8xEzARBgoJkiaJk/IsZAEZFgNjb20xGTAXBgoJ
// SIG // kiaJk/IsZAEZFgltaWNyb3NvZnQxLTArBgNVBAMTJE1p
// SIG // Y3Jvc29mdCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0
// SIG // eYIQea0WoUqgpa1Mc1j0BxMuZTBQBgNVHR8ESTBHMEWg
// SIG // Q6BBhj9odHRwOi8vY3JsLm1pY3Jvc29mdC5jb20vcGtp
// SIG // L2NybC9wcm9kdWN0cy9taWNyb3NvZnRyb290Y2VydC5j
// SIG // cmwwVAYIKwYBBQUHAQEESDBGMEQGCCsGAQUFBzAChjho
// SIG // dHRwOi8vd3d3Lm1pY3Jvc29mdC5jb20vcGtpL2NlcnRz
// SIG // L01pY3Jvc29mdFJvb3RDZXJ0LmNydDATBgNVHSUEDDAK
// SIG // BggrBgEFBQcDCDANBgkqhkiG9w0BAQUFAAOCAgEAEJeK
// SIG // w1wDRDbd6bStd9vOeVFNAbEudHFbbQwTq86+e4+4LtQS
// SIG // ooxtYrhXAstOIBNQmd16QOJXu69YmhzhHQGGrLt48ovQ
// SIG // 7DsB7uK+jwoFyI1I4vBTFd1Pq5Lk541q1YDB5pTyBi+F
// SIG // A+mRKiQicPv2/OR4mS4N9wficLwYTp2OawpylbihOZxn
// SIG // LcVRDupiXD8WmIsgP+IHGjL5zDFKdjE9K3ILyOpwPf+F
// SIG // ChPfwgphjvDXuBfrTot/xTUrXqO/67x9C0J71FNyIe4w
// SIG // yrt4ZVxbARcKFA7S2hSY9Ty5ZlizLS/n+YWGzFFW6J1w
// SIG // lGysOUzU9nm/qhh6YinvopspNAZ3GmLJPR5tH4LwC8cs
// SIG // u89Ds+X57H2146SodDW4TsVxIxImdgs8UoxxWkZDFLyz
// SIG // s7BNZ8ifQv+AeSGAnhUwZuhCEl4ayJ4iIdBD6Svpu/RI
// SIG // zCzU2DKATCYqSCRfWupW76bemZ3KOm+9gSd0BhHudiG/
// SIG // m4LBJ1S2sWo9iaF2YbRuoROmv6pH8BJv/YoybLL+31HI
// SIG // jCPJZr2dHYcSZAI9La9Zj7jkIeW1sMpjtHhUBdRBLlCs
// SIG // lLCleKuzoJZ1GtmShxN1Ii8yqAhuoFuMJb+g74TKIdbr
// SIG // Hk/Jmu5J4PcBZW+JC33Iacjmbuqnl84xKf8OxVtc2E0b
// SIG // odj6L54/LlUWa8kTo/0wggYQMIID+KADAgECAhMzAAAA
// SIG // ZEeElIbbQRk4AAAAAABkMA0GCSqGSIb3DQEBCwUAMH4x
// SIG // CzAJBgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9u
// SIG // MRAwDgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNy
// SIG // b3NvZnQgQ29ycG9yYXRpb24xKDAmBgNVBAMTH01pY3Jv
// SIG // c29mdCBDb2RlIFNpZ25pbmcgUENBIDIwMTEwHhcNMTUx
// SIG // MDI4MjAzMTQ2WhcNMTcwMTI4MjAzMTQ2WjCBgzELMAkG
// SIG // A1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0b24xEDAO
// SIG // BgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1pY3Jvc29m
// SIG // dCBDb3Jwb3JhdGlvbjENMAsGA1UECxMETU9QUjEeMBwG
// SIG // A1UEAxMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMIIBIjAN
// SIG // BgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAky7a2OY+
// SIG // mNkbD2RfTahYTRQ793qE/DwRMTrvicJKLUGlSF3dEp7v
// SIG // q2YoNNV9KlV7TE2K8sDxstNSFYu2swi4i1AL3X/7agmg
// SIG // 3GcExPHfvHUYIEC+eCyZVt3u9S7dPkL5Wh8wrgEUirCC
// SIG // tVGg4m1l/vcYCo0wbU06p8XzNi3uXyygkgCxHEziy/f/
// SIG // JCV/14/A3ZduzrIXtsccRKckyn6B5uYxuRbZXT7RaO6+
// SIG // zUjQhiyu3A4hwcCKw+4bk1kT9sY7gHIYiFP7q78wPqB3
// SIG // vVKIv3rY6LCTraEbjNR+phBQEL7hyBxk+ocu+8RHZhbA
// SIG // hHs2r1+6hURsAg8t4LAOG6I+JQIDAQABo4IBfzCCAXsw
// SIG // HwYDVR0lBBgwFgYIKwYBBQUHAwMGCisGAQQBgjdMCAEw
// SIG // HQYDVR0OBBYEFFhWcQTwvbsz9YNozOeARvdXr9IiMFEG
// SIG // A1UdEQRKMEikRjBEMQ0wCwYDVQQLEwRNT1BSMTMwMQYD
// SIG // VQQFEyozMTY0Mis0OWU4YzNmMy0yMzU5LTQ3ZjYtYTNi
// SIG // ZS02YzhjNDc1MWM0YjYwHwYDVR0jBBgwFoAUSG5k5VAF
// SIG // 04KqFzc3IrVtqMp1ApUwVAYDVR0fBE0wSzBJoEegRYZD
// SIG // aHR0cDovL3d3dy5taWNyb3NvZnQuY29tL3BraW9wcy9j
// SIG // cmwvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // bDBhBggrBgEFBQcBAQRVMFMwUQYIKwYBBQUHMAKGRWh0
// SIG // dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMvY2Vy
// SIG // dHMvTWljQ29kU2lnUENBMjAxMV8yMDExLTA3LTA4LmNy
// SIG // dDAMBgNVHRMBAf8EAjAAMA0GCSqGSIb3DQEBCwUAA4IC
// SIG // AQCI4gxkQx3dXK6MO4UktZ1A1r1mrFtXNdn06DrARZkQ
// SIG // Tdu0kOTLdlGBCfCzk0309RLkvUgnFKpvLddrg9TGp3n8
// SIG // 0yUbRsp2AogyrlBU+gP5ggHFi7NjGEpj5bH+FDsMw9Py
// SIG // gLg8JelgsvBVudw1SgUt625nY7w1vrwk+cDd58TvAyJQ
// SIG // FAW1zJ+0ySgB9lu2vwg0NKetOyL7dxe3KoRLaztUcqXo
// SIG // YW5CkI+Mv3m8HOeqlhyfFTYxPB5YXyQJPKQJYh8zC9b9
// SIG // 0JXLT7raM7mQ94ygDuFmlaiZ+QSUR3XVupdEngrmZgUB
// SIG // 5jX13M+Pl2Vv7PPFU3xlo3Uhj1wtupNC81epoxGhJ0tR
// SIG // uLdEajD/dCZ0xIniesRXCKSC4HCL3BMnSwVXtIoj/QFy
// SIG // mFYwD5+sAZuvRSgkKyD1rDA7MPcEI2i/Bh5OMAo9App4
// SIG // sR0Gp049oSkXNhvRi/au7QG6NJBTSBbNBGJG8Qp+5QTh
// SIG // KoQUk8mj0ugr4yWRsA9JTbmqVw7u9suB5OKYBMUN4hL/
// SIG // yI+aFVsE/KJInvnxSzXJ1YHka45ADYMKAMl+fLdIqm3n
// SIG // x6rIN0RkoDAbvTAAXGehUCsIod049A1T3IJyUJXt3OsT
// SIG // d3WabhIBXICYfxMg10naaWcyUePgW3+VwP0XLKu4O1+8
// SIG // ZeGyaDSi33GnzmmyYacX3BTqMDCCB3owggVioAMCAQIC
// SIG // CmEOkNIAAAAAAAMwDQYJKoZIhvcNAQELBQAwgYgxCzAJ
// SIG // BgNVBAYTAlVTMRMwEQYDVQQIEwpXYXNoaW5ndG9uMRAw
// SIG // DgYDVQQHEwdSZWRtb25kMR4wHAYDVQQKExVNaWNyb3Nv
// SIG // ZnQgQ29ycG9yYXRpb24xMjAwBgNVBAMTKU1pY3Jvc29m
// SIG // dCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eSAyMDEx
// SIG // MB4XDTExMDcwODIwNTkwOVoXDTI2MDcwODIxMDkwOVow
// SIG // fjELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEoMCYGA1UEAxMfTWlj
// SIG // cm9zb2Z0IENvZGUgU2lnbmluZyBQQ0EgMjAxMTCCAiIw
// SIG // DQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKvw+nIQ
// SIG // HC6t2G6qghBNNLrytlghn0IbKmvpWlCquAY4GgRJun/D
// SIG // DB7dN2vGEtgL8DjCmQawyDnVARQxQtOJDXlkh36UYCRs
// SIG // r55JnOloXtLfm1OyCizDr9mpK656Ca/XllnKYBoF6WZ2
// SIG // 6DJSJhIv56sIUM+zRLdd2MQuA3WraPPLbfM6XKEW9Ea6
// SIG // 4DhkrG5kNXimoGMPLdNAk/jj3gcN1Vx5pUkp5w2+oBN3
// SIG // vpQ97/vjK1oQH01WKKJ6cuASOrdJXtjt7UORg9l7snuG
// SIG // G9k+sYxd6IlPhBryoS9Z5JA7La4zWMW3Pv4y07MDPbGy
// SIG // r5I4ftKdgCz1TlaRITUlwzluZH9TupwPrRkjhMv0ugOG
// SIG // jfdf8NBSv4yUh7zAIXQlXxgotswnKDglmDlKNs98sZKu
// SIG // HCOnqWbsYR9q4ShJnV+I4iVd0yFLPlLEtVc/JAPw0Xpb
// SIG // L9Uj43BdD1FGd7P4AOG8rAKCX9vAFbO9G9RVS+c5oQ/p
// SIG // I0m8GLhEfEXkwcNyeuBy5yTfv0aZxe/CHFfbg43sTUkw
// SIG // p6uO3+xbn6/83bBm4sGXgXvt1u1L50kppxMopqd9Z4Dm
// SIG // imJ4X7IvhNdXnFy/dygo8e1twyiPLI9AN0/B4YVEicQJ
// SIG // TMXUpUMvdJX3bvh4IFgsE11glZo+TzOE2rCIF96eTvSW
// SIG // sLxGoGyY0uDWiIwLAgMBAAGjggHtMIIB6TAQBgkrBgEE
// SIG // AYI3FQEEAwIBADAdBgNVHQ4EFgQUSG5k5VAF04KqFzc3
// SIG // IrVtqMp1ApUwGQYJKwYBBAGCNxQCBAweCgBTAHUAYgBD
// SIG // AEEwCwYDVR0PBAQDAgGGMA8GA1UdEwEB/wQFMAMBAf8w
// SIG // HwYDVR0jBBgwFoAUci06AjGQQ7kUBU7h6qfHMdEjiTQw
// SIG // WgYDVR0fBFMwUTBPoE2gS4ZJaHR0cDovL2NybC5taWNy
// SIG // b3NvZnQuY29tL3BraS9jcmwvcHJvZHVjdHMvTWljUm9v
// SIG // Q2VyQXV0MjAxMV8yMDExXzAzXzIyLmNybDBeBggrBgEF
// SIG // BQcBAQRSMFAwTgYIKwYBBQUHMAKGQmh0dHA6Ly93d3cu
// SIG // bWljcm9zb2Z0LmNvbS9wa2kvY2VydHMvTWljUm9vQ2Vy
// SIG // QXV0MjAxMV8yMDExXzAzXzIyLmNydDCBnwYDVR0gBIGX
// SIG // MIGUMIGRBgkrBgEEAYI3LgMwgYMwPwYIKwYBBQUHAgEW
// SIG // M2h0dHA6Ly93d3cubWljcm9zb2Z0LmNvbS9wa2lvcHMv
// SIG // ZG9jcy9wcmltYXJ5Y3BzLmh0bTBABggrBgEFBQcCAjA0
// SIG // HjIgHQBMAGUAZwBhAGwAXwBwAG8AbABpAGMAeQBfAHMA
// SIG // dABhAHQAZQBtAGUAbgB0AC4gHTANBgkqhkiG9w0BAQsF
// SIG // AAOCAgEAZ/KGpZjgVHkaLtPYdGcimwuWEeFjkplCln3S
// SIG // eQyQwWVfLiw++MNy0W2D/r4/6ArKO79HqaPzadtjvyI1
// SIG // pZddZYSQfYtGUFXYDJJ80hpLHPM8QotS0LD9a+M+By4p
// SIG // m+Y9G6XUtR13lDni6WTJRD14eiPzE32mkHSDjfTLJgJG
// SIG // KsKKELukqQUMm+1o+mgulaAqPyprWEljHwlpblqYluSD
// SIG // 9MCP80Yr3vw70L01724lruWvJ+3Q3fMOr5kol5hNDj0L
// SIG // 8giJ1h/DMhji8MUtzluetEk5CsYKwsatruWy2dsViFFF
// SIG // WDgycScaf7H0J/jeLDogaZiyWYlobm+nt3TDQAUGpgEq
// SIG // KD6CPxNNZgvAs0314Y9/HG8VfUWnduVAKmWjw11SYobD
// SIG // HWM2l4bf2vP48hahmifhzaWX0O5dY0HjWwechz4GdwbR
// SIG // BrF1HxS+YWG18NzGGwS+30HHDiju3mUv7Jf2oVyW2ADW
// SIG // oUa9WfOXpQlLSBCZgB/QACnFsZulP0V3HjXG0qKin3p6
// SIG // IvpIlR+r+0cjgPWe+L9rt0uX4ut1eBrs6jeZeRhL/9az
// SIG // I2h15q/6/IvrC4DqaTuv/DDtBEyO3991bWORPdGdVk5P
// SIG // v4BXIqF4ETIheu9BCrE/+6jMpF3BoYibV3FWTkhFwELJ
// SIG // m3ZbCoBIa/15n8G9bW1qyVJzEw16UM0xggSkMIIEoAIB
// SIG // ATCBlTB+MQswCQYDVQQGEwJVUzETMBEGA1UECBMKV2Fz
// SIG // aGluZ3RvbjEQMA4GA1UEBxMHUmVkbW9uZDEeMBwGA1UE
// SIG // ChMVTWljcm9zb2Z0IENvcnBvcmF0aW9uMSgwJgYDVQQD
// SIG // Ex9NaWNyb3NvZnQgQ29kZSBTaWduaW5nIFBDQSAyMDEx
// SIG // AhMzAAAAZEeElIbbQRk4AAAAAABkMAkGBSsOAwIaBQCg
// SIG // gbgwGQYJKoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYK
// SIG // KwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJKoZI
// SIG // hvcNAQkEMRYEFLG5lQixgS7uRISPh4vRyEKIVHdoMFgG
// SIG // CisGAQQBgjcCAQwxSjBIoBaAFABTAGMAcgBpAHAAdABz
// SIG // AC4AagBzoS6ALGh0dHA6Ly93d3cuTWljcm9zb2Z0LmNv
// SIG // bS9NaWNyb3NvZnREeW5hbWljcy8gMA0GCSqGSIb3DQEB
// SIG // AQUABIIBAGEfRibGaNl+PL9W2OEVc2rb1PwPOUnHljdB
// SIG // A+iEcURrLRwctph+MALTb7X4QDqapgrIVmjowBsry8IA
// SIG // xr+TP3GOQzacSmrfq10jq+C6yRXwgq7dsXbusrCenq2M
// SIG // euSkenXXQ3FoVvKv4cRq0lPsW5Ngt0EjxOBcbLvRBcwr
// SIG // qgxta/b0RwE8mVoQGRkxf3l/WKKSONHd+1dRYO8bFas2
// SIG // n52VwhUpV9NCjA8coNc2vCfXyhj9Uhgbho9+vSLtw+a+
// SIG // rcIogQ4bWgSAGZt/eEoQBJWGbqrPXGv8Bpagebikq9hl
// SIG // NxX+3jhEV333MJgNddtZ3HpsRm0y8RE+R2LQr2NNtVGh
// SIG // ggIoMIICJAYJKoZIhvcNAQkGMYICFTCCAhECAQEwgY4w
// SIG // dzELMAkGA1UEBhMCVVMxEzARBgNVBAgTCldhc2hpbmd0
// SIG // b24xEDAOBgNVBAcTB1JlZG1vbmQxHjAcBgNVBAoTFU1p
// SIG // Y3Jvc29mdCBDb3Jwb3JhdGlvbjEhMB8GA1UEAxMYTWlj
// SIG // cm9zb2Z0IFRpbWUtU3RhbXAgUENBAhMzAAAAmpqbFsKD
// SIG // 2tXCAAAAAACaMAkGBSsOAwIaBQCgXTAYBgkqhkiG9w0B
// SIG // CQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0x
// SIG // NjA3MjEyMTA3MjBaMCMGCSqGSIb3DQEJBDEWBBTOL5U0
// SIG // 8t9kkWxNm+LH78yp7WHgOzANBgkqhkiG9w0BAQUFAASC
// SIG // AQCjz6psS88AMFIthwY77QvDewXeTEKKJhTZ8WhwMv9W
// SIG // UbtxI4CyjbQT8LugMSpor+V/oG6RPu8Yxz0tSIHWuDVc
// SIG // 9aI8vRA26rHFmZYo7gWH2869aujp3ivqrKcnPYyk+uz1
// SIG // fg1pgpj0lHSGQhRei8PYL3cSdkMAtM+QpLtcNeLDub5v
// SIG // t7eENfbcD3kGWUxjQfrokOk4Lt0L8tg5fd8TwjgclcDa
// SIG // uOpNonXnuSf2n/3O6PvdSNlpJYZnvQG1e2GiNfdCOB8g
// SIG // LIBqMqg2Ia7nhvPkIhGAbk5XyV+jM0FO8W7tsF1KbHIv
// SIG // yCDL+j9TInhbG9Rzr8D3IELBlUbpmY0N/X9X
// SIG // End signature block
