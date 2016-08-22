/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='ProxyError.ts'/>

module Commerce.Proxy {
    "use strict";

    /**
     * Error type enum.
     */
    export class ErrorTypeEnum {
        static Cash: string = "1";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDDEVICETOKEN: string = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDDEVICE";

        // Client error codes mapped to locale strings
        static APPLICATION_ERROR: string = "string_29000";
        static APPLICATION_STORE_INITIALIZATION_DATA_FAILED_TO_LOAD = "string_29007";
        static PRICE_CHECK_INITIALIZATION_DATA_FAILED_TO_LOAD = "string_29022";
        static CART_LINE_MISSING_PRODUCT_NAME_ERROR: string = "string_1242";
        static OPERATOR_ID_PASSWORD_NOT_SPECIFIED: string = "string_29001";
        static OPERATOR_PASSWORD_NOT_SPECIFIED: string = "string_29019";
        static SERVER_ERROR: string = "string_29002";
        static INVALID_URL: string = "string_1330";
        static INVALID_EMAIL: string = "string_1331";
        static INVALID_PHONE: string = "string_1332";
        static INVALID_NAME: string = "string_1360";
        static INVALID_NAME_FORMAT: string = "string_1361";
        static EMPTY_STREET: string = "string_1333";
        static EMPTY_CITY: string = "string_1334";
        static EMPTY_STATE: string = "string_1335";
        static EMPTY_COUNTRY: string = "string_1336";
        static EMPTY_ZIPCODE: string = "string_1337";
        static EMPTY_NAME: string = "string_1338";
        static LINE_ITEM_MISSING_PRODUCT_NAME_ERROR: string = "string_3204";
        static LOGOFF_ERROR: string = "string_1353";
        static AAD_AUTHENTICATION_FAILED: string = "string_1442";
        static DEVICE_ACTIVATION_DETAILS_NOT_SPECIFIED: string = "string_1407";
        static DEVICE_DEACTIVATION_INCOMPLETE_TRANSACTION_ERROR: string = "string_1421";
        static CHANGE_PASSWORD_DETAILS_NOT_SPECIFIED: string = "string_6805";
        static NEW_PASSWORD_AND_CONFIRMATION_NOT_MATCHING_ERROR: string = "string_6806";
        static RESET_PASSWORD_DETAILS_NOT_SPECIFIED: string = "string_6810";
        static OLD_AND_NEW_PASSWORD_MATCHING_ERROR: string = "string_6607";
        static OPERATION_ISSUE_CREDIT_MEMO_NOT_AVAILABLE: string = "string_29801";
        static PAYMENT_INFORMATION_INCOMPLETE: string = "string_1137";
        static PAYMENT_INVALID_NUMBER: string = "string_1138";
        static PAYMENT_CARD_NOT_SUPPORTED: string = "string_1139";
        static PAYMENT_CASH_PAYMENT_NOT_AVAILABLE: string = "string_1142";
        static PAYMENT_CARD_PAYMENT_NOT_AVAILABLE: string = "string_1158";
        static PAYMENT_UNABLE_TO_LOAD_CURRENCY_AMOUNTS: string = "string_1143";
        static PAYMENT_CUSTOMER_ACCOUNT_NOT_SET: string = "string_1154";
        static PAYMENT_AMOUNT_CANNOT_BE_EMPTY: string = "string_1159";
        static PAYMENT_CARD_TRACK_DATA_EMPTY: string = "string_1166";
        static PAYMENT_CARD_NUMBER_EMPTY: string = "string_1167";
        static PAYMENT_CARD_SECURITY_CODE_EMPTY: string = "string_1168";
        static PAYMENT_CREDIT_MEMO_NUMBER_EMPTY: string = "string_1169";
        static PAYMENT_LOYALTY_CARD_NUMBER_EMPTY: string = "string_1170";
        static PAYMENT_GIFT_CARD_NUMBER_EMPTY: string = "string_1171";
        static PAYMENT_CUSTOMER_ACCOUNT_EMPTY: string = "string_1172";
        static PAYMENT_INVALID_CARD_NUMBER: string = "string_1175";
        static PAYMENT_INVALID_SECURITY_CODE: string = "string_1176";
        static PAYMENT_INVALID_ZIP_CODE: string = "string_1177";
        static CART_IS_EMPTY: string = "string_29008";
        static RETAIL_SERVER_REDIRECT_ERROR = "RETAIL_SERVER_REDIRECT_ERROR";
        static OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM = "string_29824";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MANUALCARDNUMBERNOTALLOWED = "string_29825";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENDERLINECANNOTBEVOIDED = "string_29826";
        static INVALID_INCOME_EXPENSE_LINE_COLLECTION = "string_4122";
        static CARTLINE_DISCOUNTINUED = "string_4452";
        static SCALE_UNSPECIFIED_WITHOUT_MANUAL_ENTRY = "string_5316";
        static SCALE_RETURNED_ZERO_WITHOUT_MANUAL_ENTRY = "string_5317";
        static APPLICATION_CONFIGURATION_LOADING_ERROR: string = "string_29951";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEMOMODEOPERATIONNOTSUPPORTED: string = "string_29952";
        static MANAGER_OVERRIDE_CANCELED_ERROR: string = "string_29029";
        static SELECTED_CARTLINES_CONTAINS_VOIDED_PRODUCTS: string = "string_29832";
        static CUSTOMERORDER_MANUAL_DEPOSIT_REQUIRED: string = "string_29833";

        // Client error codes - store information and details
        static STORE_NOT_FOUND = "string_29016";

        // Client error codes - return - mapped to local strings
        static DIMENSION_SELECTION_NOT_COMPLETED: string = "string_821";
        static RETURN_MULTIPLE_REASON_CODE_SETS_ARE_DEFINED: string = "RETURN_MULTIPLE_REASON_CODE_SETS_ARE_DEFINED";
        static RETURN_NO_ORDERS_FOUND: string = "string_1218";
        static RETURN_NO_SALES_LINES_IN_ORDER: string = "string_1220";
        static RETURN_ALL_SALES_LINES_IN_ORDER_RETURN: string = "string_1237";
        static RETURN_NO_REASON_CODES_ARE_DEFINED: string = "string_1244";
        static RETURN_NO_ITEM_SELECTED: string = "string_1246";
        static RETURN_MAX_RETURN_LINE_AMOUNT_EXCEEDED: string = "string_29370";
        static RETURN_MAX_RETURN_TOTAL_AMOUNT_EXCEEDED: string = "string_29371";

        // Client error codes - payment
        static CREDIT_MEMO_INVALID_AMOUNT: string = "string_29800";

        //Client error codes - inventory
        static NO_PRICECHECK_ON_PRODUCTS: string = "string_3523";
        static NO_PRODUCT_INFORMATION: string = "string_3873";

        // Client error codes - salesOrders
        static CART_UNAVAILABLE_FOR_PICK_UP: string = "string_4539";
        static CART_LINES_UNAVAILABLE_FOR_PICK_UP: string = "string_4540";
        static PICK_LIST_CAN_NOT_BE_CREATED: string = "string_4544";
        static PACK_SLIP_CAN_NOT_BE_CREATED: string = "string_4546";
        static CUSTOMER_ORDER_OPERATION_INVOICE_NOT_SUPPORTED = "string_29028";
        static CREATE_OR_EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY = "string_29032";
        static EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY = "string_29033";
        static NO_STORE_SELECTED_FOR_PICKUP = "string_29034";
        static ALL_PRODUCTS_SELECTED_PICKUP_OR_SHIP_SELECTED = "string_29035";
        static INVALID_SHIPPING_CHARGES = "string_2543";
        static NO_ADDRESSES_SELECTED_FOR_SHIP = "string_29036";
        static NO_SHIPPING_METHODS_SELECTED_FOR_SHIP = "string_29037";
        static CREATE_OR_EDIT_QUOTATION_ONLY = "string_29042";
        static EDIT_CUSTOMER_ORDER_ONLY = "string_29043";

        // Client error codes - customer order cancellation
        static CANCELLATION_CHARGE_IS_NOT_VALID: string = "string_4542";
        static CANCELLATION_CHARGE_INVALID_NEGATIVE_AMOUNT: string = "string_29026";
        static CANCELLATION_CHARGE_INVALID_OPERATION: string = "string_29027";
        static CART_UNAVAILABLE_FOR_CANCEL: string = "string_4541";

        // Client error codes - discounts
        static UNSUPPORTED_APPLY_DISCOUNT_OPERATION: string = "string_5600";
        static MISSING_CARTLINE_ON_APPLY_DISCOUNT: string = "string_5601";
        static MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED: string = "string_5602";
        static MAXIMUM_LINE_DISCOUNT_PERCENT_EXCEEDED: string = "string_5603";
        static MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED: string = "string_5604";
        static MAXIMUM_TOTAL_DISCOUNT_PERCENT_EXCEEDED: string = "string_5605";
        static MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED_PRICE: string = "string_5617";
        static MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED_SUBTOTAL: string = "string_5618";

        // Client error codes - price override
        static PRICE_OVERRIDE_NOT_VALID_ONE_OR_MORE_ITEMS: string = "string_5705";
        static PRICE_OVERRIDE_PRICE_EXCEEDS_MAXIMUM_DEVICE_PRICE: string = "string_5715";
        static PRICE_OVERRIDE_NO_ITEM_SELECTED: string = "string_5716";
        static PRICE_OVERRIDE_PRODUCT_IS_VOIDED: string = "string_29803";
        static PRICE_OVERRIDE_PRICE_CANNOT_BE_NEGATIVE: string = "string_29009";
        static PRICE_OVERRIDE_INVALID_PRICE: string = "string_29010";
        static PRICE_OVERRIDE_PRICE_NOT_A_NUMBER: string = "string_29011";
        static PRICE_OVERRIDE_PRODUCT_IS_FOR_A_RECEIPT: string = "string_29804";
        static PRICE_OVERRIDE_PRODUCT_IS_FOR_A_GIFT_CERTIFICATE: string = "string_29805";
        static PRICE_OVERRIDE_PRICE_CANNOT_BE_ZERO: string = "string_5717";
        static PRICE_OVERRIDE_PRICE_MUST_BE_POSITIVE: string = "string_29020";
        static PRICE_OVERRIDE_ONLY_LOWER_AMOUNTS_ALLOWED: string = "string_5718";
        static PRICE_OVERRIDE_ONLY_HIGHER_AMOUNTS_ALLOWED: string = "string_5719";
        static PRICE_OVERRIDE_ONLY_LOWER_OR_EQUAL_AMOUNTS_ALLOWED: string = "string_5720";
        static PRICE_OVERRIDE_ONLY_HIGHER_OR_EQUAL_AMOUNTS_ALLOWED: string = "string_5721";
        static PRICE_OVERRIDE_NOT_ALLOWED_FOR_PRODUCT: string = "string_5722";
        static PRICE_OVERRIDE_NONE_ALLOWED: string = "string_5723";

        // Client error codes - change sales person
        static CHANGE_SALES_PERSON_INVALID_CART_MODE: string = "string_5733";

        // Client error codes - set quantity
        static SET_QUANTITY_NOT_VALID_ONE_OR_MORE_ITEMS: string = "string_5305";
        static SET_QUANTITY_NOT_GREATER_THAN_ZERO: string = "string_5308";
        static SET_QUANTITY_NOT_IN_RANGE: string = "string_5309";
        static SET_QUANTITY_NOT_VALID_NO_ITEM_SELECTED: string = "string_5310";
        static SET_QUANTITY_NOT_A_NUMBER: string = "string_5311";
        static SET_QUANTITY_NOT_ZERO: string = "string_5312";
        static SET_QUANTITY_QUANTITY_EXCEEDS_MAXIMUM_DEVICE_QUANTITY: string = "string_5313";
        static SET_QUANTITY_NOT_VALID_FOR_UNIT_OF_MEASURE: string = "string_5314";
        static SET_QUANTITY_NOT_VALID_FOR_SERIALIZED_ITEM: string = "string_5315";

        // Client error codes - unit of measure
        static UNIT_OF_MEASURE_NOT_VALID_ONE_OR_MORE_ITEMS: string = "string_3205";
        static UNIT_OF_MEASURE_NOT_VALID_NO_ITEM_SELECTED: string = "string_3206";
        static UNIT_OF_MEASURE_NOT_VALID_NO_UNIT_OF_MEASURE: string = "string_3207";
        static UNIT_OF_MEASURE_NOT_VALID_NO_UNIT_OF_MEASURE_CONVERSIONS: string = "string_3208";

        static NOT_IMPLEMENTED: string = "string_29003";

        // Client error codes - kit disassembly
        static KIT_BLOCKED_FOR_DISASSEMBLY_AT_REGISTER: string = "string_420";

        // Client error codes - peripherals
        static PERIPHERALS_HARDWARESTATION_NOTCONFIGURED = "string_4908";
        static PERIPHERALS_BARCODE_SCANNER_NOTFOUND: string = "string_4900";
        static PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED: string = "string_4901";
        static PERIPHERALS_MSR_NOTFOUND: string = "string_4902";
        static PERIPHERALS_MSR_ENABLE_FAILED: string = "string_4903";
        static PERIPHERALS_PRINTER_FAILED: string = "string_4904";
        static PERIPHERAL_PAYMENT_UNKNOWN_ERROR: string = "string_4919";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PRINTER_ERROR = "string_4904";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CASHDRAWER_ERROR = "string_4905";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_SCALE_ERROR = "string_4906";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PAYMENTTERMINAL_ERROR = "string_4907";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_DUALDISPLAY_ERROR = "string_4918";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PERIPHERALNOTFOUND = "string_4917";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PAIRINGERROR = "string_6011";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_TOKENVALIDATIONFAILED = "string_6011";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PINPAD_ERROR = "string_4923";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_SIGNATURECAPTURE_ERROR = "string_4924";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_LINEDISPLAY_ERROR = "string_4925";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_LINEDISPLAY_CHARACTERSETNOTSUPPORTED = "string_4926";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PERIPHERALISLOCKED = "string_4927";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTCONNECTORNOTFOUND = "string_4929";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTPAYMENTISNOTALLOWEDFORCUSTOMERORDERDEPOSITANDCANCELLATION = "string_29021";

        // Client error codes - store operations
        static AMOUNT_IS_NOT_VALID: string = "string_4102";
        static SHIFT_IS_NOT_VALID: string = "string_4103";
        static OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED: string = "string_29803";
        static OPERATION_NOT_ALLOWED_PRODUCT_IS_FOR_A_RECEIPT: string = "string_29804";
        static OPERATION_NOT_ALLOWED_PRODUCT_IS_FOR_A_GIFT_CERTIFICATE: string = "string_29805";
        static OPERATION_NOT_ALLOWED_LINKED_PRODUCT: string = "string_29806";
        static OPERATION_NOT_ALLOWED_MULTIPLE_CART_LINES: string = "string_29807";
        static OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION: string = "string_29808";
        static OPERATION_NOT_ALLOWED_TIME_CLOCK_DISABLED: string = "string_29809";
        static OPERATION_NOT_ALLOWED_FINISH_CURRENT_TRANSACTION: string = "string_4125";
        static RECEIPT_EMAIL_IS_EMPTY: string = "string_4126";
        static OPERATION_NOT_ALLOWED_PERMISSION_DENIED: string = "string_511";
        static OPERATION_VALIDATION_INVALID_ARGUMENTS: string = "string_29018";
        static OPERATION_NOT_ALLOWED_NO_CART_LINE_SELECTED: string = "string_29822";
        static OPERATION_NOT_ALLOWED_IN_NONDRAWER_MODE: string = "string_4141";
        static OPERATION_NOT_ALLOWED_NO_PAYMENT_LINE_SELECTED: string = "string_29828";
        static OPERATION_NOT_ALLOWED_MULTIPLE_PAYMENT_LINES: string = "string_29829";
        static OPERATION_NOT_ALLOWED_IN_OFFLINE_STATE: string = "string_29831";

        // Client error codes - affiliation
        static INVALID_AFFILIATION_COLLECTION: string = "string_5205";

        // Client error codes - sales tax override
        static MISSING_CARTLINE_ON_APPLY_TAX_OVERRDE: string = "string_4423";
        static NO_TAX_OVERRIDE_REASON_CODES_CONFIGURED: string = "string_4422";

        // Client error code - Offline
        static CANNOT_SWITCH_ONLINE_CART_IN_PROGRESS = "string_6607";
        static CANNOT_SWITCH_OFFLINE_NOT_AVAILABLE = "string_6608";
        static CANNOT_SWITCH_TRANSFER_FAILED = "string_6609";

        // Client error codes - Signature
        static SIGNATURE_INVALID_FORMAT: string = "string_6906";

        // Async Client codes
        static ASYNC_CLIENT_ZERO_DOWNLOAD_SESSION: string = "string_29375";
        static ASYNC_CLIENT_EMPTY_UPLOAD_JOB_DEFINITION: string = "string_29376";
        static ASYNC_CLIENT_NO_TRANSACTION_DATA: string = "string_29377";
        static ASYNC_CLIENT_CANNOT_LOAD_OFFLINE_TRANSACTION_DATA: string = "string_29378";
        static ASYNC_CLIENT_FAIL_PURGE_OFFLINE_TRANSACTION_DATA: string = "string_29379";
        static ASYNC_CLIENT_FAIL_DOWNLOAD_FILE: string = "string_29380";
        static ASYNC_CLIENT_FAIL_APPLY_FILE_TO_OFFLINE_DATABASE: string = "string_29381";
        static ASYNC_CLIENT_FAIL_UPDATE_DOWNLOAD_SESSION_STATUS: string = "string_29382";
        static ASYNC_CLIENT_FAIL_RETRIEVE_INITIAL_DATA_SYNC_INDICATOR: string = "string_29383";
        static ASYNC_CLIENT_OFFLINE_NOT_ENABLED_ON_TERMINAL: string = "string_29384";

        // Transaction service errors comes already localized from Retail Server.
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE: string = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE";

        // Employee errors
        static EMPLOYEE_NOT_FOUND: string = "string_29320";

        // Retail Server Error codes mapped to locale strings
        static GENERICCOMMERCEERROR: string = "string_29200";
        static GENERICERRORMESSAGE: string = "string_29201";
        static GENERICVALIDATIONERROR: string = "string_29202";
        static ISASSOCIATEDVALIDATIONERROR: string = "string_29203";
        static ISNOTASSOCIATEDVALIDATIONERROR: string = "string_29204";
        static ISNOTAUTHENTICATEDVALIDATIONERROR: string = "string_29205";
        static ITEMSVALIDATIONERROR: string = "string_29206";
        static LINEIDSVALIDATIONERROR: string = "string_29207";
        static LISTINGSVALIDATIONERROR: string = "string_29208";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DUPLICATEOBJECT: string = "string_29209";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INSUFFICIENTQUANTITYONHAND: string = "string_29210";
        static INVALID_CURRENCY_AMOUNT: string = "string_29012";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTSALESLINEADD: string = "string_29211";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDFORMAT: string = "string_29212";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LASTCHANGEVERSIONMISMATCH: string = "string_29213";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OBJECTNOTFOUND: string = "string_29214";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REQUIREDVALUENOTFOUND: string = "string_29215";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNKNOWNREQUEST: string = "string_29216";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNSUPPORTEDLANGUAGE: string = "string_29217";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_VALUEOUTOFRANGE: string = "string_29218";
        static NAMEVALIDATIONERROR: string = "string_29219";
        static PAYMENTSVALIDATIONERROR: string = "string_29220";
        static PROMOTIONCODEVALIDATIONERROR: string = "string_29221";
        static SAVEDSHOPPINGCARTIDVALIDATIONERROR: string = "string_29222";
        static SHIPPINGOPTIONSLINEITEMSELECTION: string = "string_29223";
        static SHIPPINGOPTIONSPICKUP: string = "string_29224";
        static SHIPPINGOPTIONSSHIPTONEWADDRESS: string = "string_29225";
        static SHIPPINGOPTIONSVALIDATIONERROR: string = "string_29226";
        static SHOPPINGCARTIDSVALIDATIONERROR: string = "string_29227";
        static SHOPPINGCARTIDVALIDATIONERROR: string = "string_29228";
        static STORELOCATORBINGMAPSTOKENEMPTY: string = "string_29229";
        static STORELOCATORINVALIDDISTANCE: string = "string_29230";
        static STORELOCATORINVALIDLOCATION: string = "string_29231";
        static STORELOCATORUNABLETOGETCOORDINATES: string = "string_29232";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTVERSION: string = "string_29233";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATECOMMUNICATIONERROR: string = "string_29234";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_APPLICATIONCOMPOSITIONFAILED: string = "string_29235";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTRETURNMORETHANPURCHASED: string = "string_5303";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFIGURATIONSETTINGNOTFOUND: string = "string_29236";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DUPLICATEDEFAULTNOTIFICATIONHANDLERENCOUNTERED: string = "string_29237";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_EMPTYINVENTORYUNITOFMEASUREFORITEM: string = "string_29238";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_EXTERNALPROVIDERERROR: string = "string_29239";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERCOMMUNICATIONFAILURE: string = "string_29240";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERRESPONSEPARSINGERROR: string = "string_29241";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARTSTATE: string = "string_29008";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANNELCONFIGURATION: string = "string_29242";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCONFIGURATIONKEYFORMAT: string = "string_29243";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCONNECTIONSTRING: string = "string_29244";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPIPELINECONFIGURATION: string = "string_29245";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPROVIDERCONFIGURATION: string = "string_29246";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDRUNTIMECONTEXT: string = "string_29247";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSERVERRESPONSE: string = "string_29248";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SERVICEINITIALIZATIONFAILED: string = "string_29249";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SERVICENOTFOUND: string = "string_29250";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCOMPUTESALESTAXGROUPFORADDRESS: string = "string_29251";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDDEFAULTHANDLER: string = "string_29252";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDDELIVERYOPTIONS: string = "string_29253";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOFINDINVENTORYFORITEM: string = "string_29254";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATEVALIDATIONERROR: string = "string_29255";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHENTICATIONFAILED: string = "string_29256";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHORIZATIONFAILED: string = "string_29257";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_COUPONISVALIDFORCURRENTSESSION: string = "string_29258";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CRITICALSTORAGEERROR: string = "string_29259";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCOUNTAMOUNTINVALIDATED: string = "string_29260";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCOUNTISALLOWEDONLYFORCREATIONANDEDITION: string = "string_5613";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_IDMISMATCH: string = "string_29261";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INSUFFICIENTQUANTITYAVAILABLE: string = "string_29262";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANGETRACKINGCONFIGURATION: string = "string_29263";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPRICEENCOUNTERED: string = "string_29264";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDREQUEST: string = "string_29265";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSHIPPINGADDRESS: string = "string_29266";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSQLCOMMAND: string = "string_29267";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMDISCONTINUEDFROMCHANNEL: string = "string_29268";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OBJECTVERSIONMISMATCHERROR: string = "string_29269";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PROVIDERCOMMUNICATIONFAILURE: string = "string_29270";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REQUESTEDITEMISOUTOFSTOCK: string = "string_29271";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNITOFMEASURECONVERSIONNOTFOUND: string = "string_29272";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEFAULTCUSTOMERNOTFOUND: string = "string_29273";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BARCODENOTFOUND: string = "string_29368";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LINKEDITEMSEARCHBYBARCODENOTSUPPORTED: string = "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LINKEDITEMSEARCHBYBARCODENOTSUPPORTED";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_QUOTEMUSTNOTHAVEDEPOSITOVERRIDE: string = "string_29282";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_QUOTEMUSTHAVEVALIDQUOTATIONEXPIRYDATE: string = "string_4321";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPICKUPDEPOSITOVERRIDEAMOUNT: string = "string_29283";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SALESLINEMUSTHAVEPICKUPDELIVERYMODE: string = "string_29820";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOPICKUPMORETHANQTYREMAINING: string = "string_29821";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ORDERWASNOTCREATEDWITHDEPOSITOVERRIDE: string = "string_29299";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MULTIPLEEMPLOYEETOTALDISCOUNTSNOTALLOWED: string = "string_29294";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MULTIPLEEMPLOYEELINEDISCOUNTSNOTALLOWED: string = "string_29295";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTSUSPENDCARTWITHACTIVETENDERLINES: string = "string_29304";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TERMINALHASANOPENSHIFT: string = "string_29338";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CASHDRAWERHASANOPENSHIFT: string = "string_29306";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SHIFTVALIDATIONERROR = "string_29307";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SHIFTALREADYOPENONDIFFERENTTERMINAL = "string_29334";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SHIFTSTARTINGAMOUNTNOTENTERED = "string_29308";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SHIFTTENDERDECLARATIONAMOUNTNOTENTERED = "string_29309";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTCHANGECUSTOMERIDWHENEDITINGCUSTOMERORDER: string = "string_4420";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDSHIPPINGDATE: string = "string_29810";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CURRENCYCHANNELORDERMISMATCH: string = "string_29319";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CURRENCYNOTFOUND: string = "string_29374";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEPOSITMUSTBEGREATERTHANZERO: string = "string_29325";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEPOSITOVERRIDEMUSTNOTBEGREATERTHANTOTALAMOUNT: string = "string_29324";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEPOSITOVERRIDEMAYNOTBECHANGED: string = "string_29326";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEPOSITOVERRIDEMAYNOTBECLEARED: string = "string_29327";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTUSINGUNAUTHORIZEDACCOUNT: string = "string_29351";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_EMPLOYEEDISCOUNTEXCEEDED: string = "string_29346";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MAXCOUNTINGDIFFERENCEEXCEEDED: string = "string_29367";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGETENDERTYPENOTSUPPORTED: string = "string_29369";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ELEVATEDUSERSAMEASLOGGEDONUSER: string = "string_29256";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGENERATETOKEN: string = "string_1175";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTKEYNOTFOUND: string = "string_1195";

        static BAD_REQUEST: string = "string_29274";
        static NOT_AUTHORIZIED: string = "string_29275";
        static FORBIDDEN: string = "string_29276";
        static PRECONDITION_FAILED: string = "string_29277";
        static SERVICE_UNAVAILABLE: string = "string_29278";
        static SERVER_TIMEOUT: string = "string_29279";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCARDEXPIRATIONDATE: string = "string_1180";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOAUTHORIZEPAYMENT: string = "string_29280";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_VOIDTRANSACTIONCONTAINSTENDEREDLINES: string = "string_29281";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOYALTYCARDALREADYISSUED: string = "string_29284";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERNOTFOUND: string = "string_29285";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDLOYALTYCARDNUMBER: string = "string_29286";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BLOCKEDLOYALTYCARD: string = "string_29287";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENDERLOYALTYCARD: string = "string_29322";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFLICTLOYALTYCARDCUSTOMERANDTRANSACTIONCUSTOMER: string = "string_29288";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDDEVICE: string = "string_507";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CDXREALTIMESERVICEFAILURE = "string_29289";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENOUGHREWARDPOINTS = "string_29290";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOMORETHANONELOYALTYTENDER = "string_29291";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AMOUNTDUEMUSTBEPAIDBEFORECHECKOUT = "string_29292";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPAYMENTREQUEST = "string_29293";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTALREADYVOIDED = "string_29293";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDADDRESS: string = "string_29296";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOPRICEOVERRIDEFORRETURNS = "string_29297";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOPRICEOVERRIDEFORGIFTCARDS = "string_29298";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTAMOUNTEXCEEDSGIFTBALANCE = "string_29301";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOMORETHANONEOPERATIONWITHAGIFTCARD = "string_29302";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTADDNONPRODUCTITEMTOCUSTOMERORDER = "string_29811";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USEEXISTINGSHIFTPERMISSIONDENIED = "string_29303";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SHIFTNOTFOUND = "string_29336";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTDEPOSITMULTIPLECARTLINESNOTALLOWED = "string_29340";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTNUMBERISNOTSET = "string_29341";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTPAYFORCUSTOMERACCOUNTDEPOSITWITHCUSTOMERACCOUNTPAYMENTMETHOD = "string_29342";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTDEPOSITCANNOTBENEGATIVE = "string_29343";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTDEPOSITCANNOTBEVOIDED = "string_29344";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTDEPOSITCARTTYPEMISMATCH = "string_29345";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NONDRAWEROPERATIONSONLY = "string_2123";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTSUSPENDCARTWITHACTIVEGIFTCARDSALESLINES = "string_29305";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RETURNITEMPRICEEXCEEDED = "string_29310";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RETURNTRANSACTIONTOTALEXCEEDED = "string_29311";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PROPERTYUPDATENOTALLOWED = "string_29312";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDQUANTITY = "string_29313";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCUSTOMERORDERMODEFORADDCARTLINE: string = "string_29314";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCUSTOMERORDERMODEFORVOIDPRODUCTS: string = "string_29335";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REMOTEPRODUCTSNOTSUPPORTEDWITHCURRENTTRANSACTIONTYPE = "string_29315";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REFUNDAMOUNTMORETHANALLOWED = "string_29316";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_VIEWTIMECLOCKNOTENABLED = "string_29317";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMQUANTITYEXCEEDED: string = "string_29318";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TIMECLOCKNOTENABLED: string = "string_29321";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDUNITOFMEASURE: string = "string_29323";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SETTLEINVOICEFAILED = "string_29330";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOPRICEOVERRIDEFORINVOICELINES = "string_29331";
        static MICROSOFT_DYNAMICS_SERVER_INTERNAL_ERROR: string = ErrorTypeEnum.GENERICERRORMESSAGE;
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTFOUND: string = "string_29314";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTACTIVE: string = "string_29834";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCOMEEXPENSECARTDOESNOTALLOWSALESLINE: string = "string_29337";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCOMEEXPENSECARTDOESNOTALLOWCUSTOMER: string = "string_29348";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_GIFTCARDUNLOCKFAILED: string = "string_29339";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNLOCKREGISTERFAILED: string = "string_29352";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OFFLINEDATABASECHUNKFILENOTFOUND: string = "string_29830";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGEPASSWORDFAILED = "string_6613";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RESETPASSWORDFAILED = "string_6614";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANNELDATABASECONNECTIONFAILED: string = "string_1429";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICECONNECTIONFAILED: string = "string_1429";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SALESMUSTHAVEQUANTITYGREATERTHANZERO: string = "string_29353";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCAPTUREPAYMENT: string = "string_29354";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_SESSIONEXPIRED: string = "string_29256";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAMOUNT: string = "string_29372";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDBAGNUMBER: string = "string_29373";

        // Retail Server Payment Error codes mapped to locale strings
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BALANCEAMOUNTEXCEEDSMAXIMUMALLOWEDVALUE: string = "string_29355";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGEBACKISNOTALLOWED: string = "string_29356";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCORRECTPAYMENTAMOUNTSIGN: string = "string_29357";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OVERTENDERAMOUNTEXCEEDSMAXIMUMALLOWEDVALUE: string = "string_29358";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERLINE: string = "string_29359";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERTRANSACTION: string = "string_29360";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERLINE: string = "string_29361";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERTRANSACTION: string = "string_29362";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTMUSTBEUSEDTOFINALIZETRANSACTION: string = "string_29363";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PRODUCTISNOTACTIVE: string = "string_29364";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PRODUCTISBLOCKED: string = "string_29365";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTRETURNMULTIPLETRANSACTIONS: string = "string_29366";
    }

    /**
     * Forward links for device activation errors.
     */
    export class DeviceActivationErrorsForwardLinks {
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANNELDATABASECONNECTIONFAILED: string = "http://go.microsoft.com/fwlink/?LinkId=403590";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICECONNECTIONFAILED: string = "http://go.microsoft.com/fwlink/?LinkId=403591";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TERMINALNOTASSIGNEDTOSTORE: string = "http://go.microsoft.com/fwlink/?LinkId=403592";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERNOTASSIGNEDTOSTORE: string = "http://go.microsoft.com/fwlink/?LinkId=403593";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCORRECTSTAFFIDORPASSWORD: string = "http://go.microsoft.com/fwlink/?LinkId=403594";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICEALREADYACTIVATED: string = "http://go.microsoft.com/fwlink/?LinkId=519136";
        static SERVER_ERROR: string = "http://go.microsoft.com/fwlink/?LinkId=519137";
    }

    export class ErrorHelper {
        private static AGGREGATED_ERROR_RESOUCEIDS: string[] = [
            "Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError",
            "Microsoft_Dynamics_Commerce_Runtime_AggregateCommunicationError",
            "Microsoft_Dynamics_Commerce_Runtime_InvalidCartLinesAggregateError"];

        public static MICROSOFT_DYNAMICS_SERVER_INTERNAL_ERROR: string = 'Microsoft_Dynamics_Server_Internal_Error';

        public static isAggregatedErrorResourceId(errorResourceId: string): boolean {
            return ErrorHelper.AGGREGATED_ERROR_RESOUCEIDS.indexOf(errorResourceId) != -1;
        }

        /**
         * Map response status code to error.
         *
         * @param {any} Error returned by retail server call.
         */
        public static MapResponseStatusCodeToError(errorMessage: string, statusCode: number, err?: any): ProxyError {
            var errorCode = "";
            var canRetry = false;

            switch (statusCode) {
                case 400:
                    errorCode = ErrorTypeEnum.BAD_REQUEST;
                    break;
                case 401:
                    errorCode = ErrorTypeEnum.NOT_AUTHORIZIED;
                    break;
                case 403:
                    errorCode = ErrorTypeEnum.FORBIDDEN;
                    break;
                case 412:
                    errorCode = ErrorTypeEnum.PRECONDITION_FAILED;
                    break;
                case 503:
                    errorCode = ErrorTypeEnum.SERVICE_UNAVAILABLE;
                    canRetry = true;
                    break;
                case 500:
                default:
                    errorCode = ErrorTypeEnum.SERVER_ERROR;
                    errorMessage = "The server error format is not supported or it does not have enough information.";

                    // Temporary until the fix for Bug 681427 goes in
                    // If no orders were found, than an error did not occur
                    if (err && err.response && err.response.body && err.response.requestUri) {
                        if (err.response.requestUri.indexOf("GetOrderByReceiptId") > -1) {
                            if (err.response.body.indexOf("No orders were found") > -1) {
                                errorMessage = Commerce.Proxy.ErrorTypeEnum.RETURN_NO_ORDERS_FOUND;
                            }
                        }
                    }

                    break;
            }

            if (errorMessage && errorMessage.toUpperCase() == "TIMEOUT") {
                errorCode = ErrorTypeEnum.SERVER_TIMEOUT;
            }

            return new ProxyError(errorCode, errorMessage || StringExtensions.EMPTY, StringExtensions.EMPTY, canRetry);
        }

        /**
         * Gets whether an error code is present in the error collection or not.
         * @param {string} errorType The error resource identifier.
         * @return {boolean} Whether an error code is present in the error collection or not.
         */
        public static hasError(errors: ProxyError[], errorType: string): boolean {

            if (ArrayExtensions.hasElements(errors)) {
                for (var i = 0; i < errors.length; i++) {

                    // get errorType from errorResourceId
                    var error: ProxyError = errors[i];
                    var errorTypeValue: string = error.ErrorCode != null
                        ? ErrorTypeEnum[error.ErrorCode.toUpperCase()]
                        : null;

                    // compares both error resource id as well as label resource id
                    if (!StringExtensions.compare(errorType, error.ErrorCode, true) || !StringExtensions.compare(errorType, errorTypeValue, true)) {
                        // error found
                        return true;
                    }
                }
            }

            return false;
        }

        /**
        * Checks whether all the provided errors are retryable. If no error code is provided or an error is undefined,
        * then the errors will be treated as not retryable.
        *
        * @return {boolean} True if the error codes are retryable, false if the error codes are not retryable or not defined
        */
        public static isRetryable(errors: ProxyError[]): boolean {
            var numErrors: number = ObjectExtensions.isNullOrUndefined(errors) ? 0 : errors.length;
            var isRetryable: boolean = numErrors > 0;
            for (var i: number = 0; i < numErrors; i++) {
                if (!ObjectExtensions.isNullOrUndefined(errors[i])) {
                    isRetryable = isRetryable && errors[i].CanRetry;
                } else {
                    isRetryable = false;
                }
            }

            return isRetryable;
        }

        /**
         * Gets a string with all error codes.
         * @param {ProxyError[]} errors the error collection.
         * @return {string} a formated string containing all error codes.
         */
        public static getErrorResourceIds(errors: ProxyError[]): string {
            var result: string = "";

            if (ArrayExtensions.hasElements(errors)) {
                var errorResourceIds: string[] = [];
                for (var i = 0; i < errors.length; i++) {
                    errorResourceIds.push(errors[i].ErrorCode);
                }

                result = errorResourceIds.join(", ");
            }

            return result;
        }

        /**
         * Gets a string with all error messages.
         * @param {ProxyError[]} errors the error collection.
         * @return {string} a formated string containing all error messages.
         */
        public static getErrorMessages(errors: ProxyError[]): string {
            var result: string = "";

            if (ArrayExtensions.hasElements(errors)) {
                var errorResourceIds: string[] = [];
                for (var i = 0; i < errors.length; i++) {
                    errorResourceIds.push(errors[i].LocalizedErrorMessage);
                }

                result = errorResourceIds.join(", ");
            }

            return result;
        }
    }
}