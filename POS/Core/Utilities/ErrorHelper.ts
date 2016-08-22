/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

///<reference path='../Entities/Error.ts'/>
///<reference path='../Entities/IErrorDetails.ts'/> 

module Commerce {
    "use strict";

    /**
     * Error type enum.
     */
    export class ErrorTypeEnum {
        
        // Device activation and Logon Error Codes
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCORRECTLOGONTYPEUSERACCOUNTORPASSWORD: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCORRECTLOGONTYPEUSERACCOUNTORPASSWORD",
            clientErrorCode: "DA1015",
            messageResource: "string_27210"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANNELDATABASECONNECTIONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANNELDATABASECONNECTIONFAILED",
            clientErrorCode: "DA1001",
            messageResource: "string_27000",
            messageDetailsResource: ["string_27002", "string_27003"],
            helperUrl: "http://go.microsoft.com/fwlink/?LinkId=403590",
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICECONFIGURATIONNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICECONFIGURATIONNOTFOUND",
            clientErrorCode: "DA1003",
            messageResource: "string_27020"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICETOKENVALIDATIONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEVICETOKENVALIDATIONFAILED",
            clientErrorCode: "DA1008",
            messageResource: "string_27070"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOCALDEVICEAUTHENTICATIONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOCALDEVICEAUTHENTICATIONFAILED",
            clientErrorCode: "DA1013",
            messageResource: "string_27200"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HARDWAREPROFILENOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HARDWAREPROFILENOTFOUND",
            clientErrorCode: "DA1004",
            messageResource: "string_27030"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOCALLOGONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_LOCALLOGONFAILED",
            clientErrorCode: "DA1006",
            messageResource: "string_27050",
            messageDetailsResource: ["string_27051", "string_27052", "string_27053", "string_27054", "string_27055"]
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NODEVICEMANAGEMENTPERMISSION: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NODEVICEMANAGEMENTPERMISSION",
            clientErrorCode: "DA1010",
            messageResource: "string_27090"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFIGURATIONSETTINGNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CONFIGURATIONSETTINGNOTFOUND",
            clientErrorCode: "DA1011",
            messageResource: "string_27110"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNKNOWNREQUESTRESPONSEPAIR: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNKNOWNREQUESTRESPONSEPAIR",
            clientErrorCode: "DA1013",
            messageResource: "string_27230"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEENDPOINTNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEENDPOINTNOTFOUND",
            clientErrorCode: "DA2002",
            messageResource: "string_27120"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEEXCEPTION: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEEXCEPTION",
            clientErrorCode: "DA2003",
            messageResource: "string_27130",
            messageDetailsResource: ["string_27131", "string_27132", "string_27133", "string_27134", "string_27135"]
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEMETHODNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICEMETHODNOTFOUND",
            clientErrorCode: "DA2007",
            messageResource: "string_27170"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICETIMEOUT: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TRANSACTIONSERVICETIMEOUT",
            clientErrorCode: "DA2006",
            messageResource: "string_27160"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERLOGINANOTHERTERMINAL: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERLOGONANOTHERTERMINAL",
            clientErrorCode: "DA1005",
            messageResource: "string_27040"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ATTEMPTTOACTIVATEFROMDIFFERENTPHYSICALDEVICE: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ATTEMPTTOACTIVATEFROMDIFFERENTPHYSICALDEVICE",
            clientErrorCode: "DA1016",
            messageResource: "string_27270"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAUDIENCE: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAUDIENCE",
            clientErrorCode: "DA1017",
            messageResource: "string_29385"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDISSUER: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDISSUER",
            clientErrorCode: "DA1018",
            messageResource: "string_29386"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENANTIDNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENANTIDNOTFOUND",
            clientErrorCode: "DA1019",
            messageResource: "string_29387"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AADTOKENISSUEDFORDIFFERENTENVIRONMENT: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AADTOKENISSUEDFORDIFFERENTENVIRONMENT",
            clientErrorCode: "DA1020",
            messageResource: "string_29388"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAADTENANTID: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAADTENANTID",
            clientErrorCode: "DA1021",
            messageResource: "string_29389"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RETAILSERVERCONFIGURATIONNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_RETAILSERVERCONFIGURATIONNOTFOUND",
            clientErrorCode: "DA1022",
            messageResource: "string_29390"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_COMMERCEIDENTITYNOTFOUND: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_COMMERCEIDENTITYNOTFOUND",
            clientErrorCode: "DA1023",
            messageResource: "string_29391"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE: Model.Entities.IErrorDetails = {
            // Transaction service errors comes already localized from Retail Server.
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE",
            clientErrorCode: "DA2001"
            // This type of error already has error message.
        };
        static MICROSOFT_DYNAMICS_POS_DATAENCRYPTIONERROR: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_DATAENCRYPTIONERROR",
            clientErrorCode: "DA3122",
            messageResource: "string_4930"
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_DNS_LOOKUP_FAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_DNS_LOOKUP_FAILED",
            clientErrorCode: "DA3005",
            messageResource: "string_27193"
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_ERROR: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_ERROR",
            clientErrorCode: "DA3001",
            messageResource: "string_27190",
            messageDetailsResource: ["string_27191", "string_27192", "string_27193", "string_27194", "string_27195", "string_27196"]
        };
        static MICROSOFT_DYNAMICS_POS_RETAILSERVERAPI_FAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_RETAILSERVERAPI_FAILED",
            clientErrorCode: "DA3003",
            messageResource: "string_27220",
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_FIREWALL_BLOCKED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_FIREWALL_BLOCKED",
            clientErrorCode: "DA3007",
            messageResource: "string_27196"
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_SERVER_TIMED_OUT: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_SERVER_TIMED_OUT",
            clientErrorCode: "DA3006",
            messageResource: "string_27190",
            messageDetailsResource: ["string_27191", "string_27192", "string_27194", "string_27195"]
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_FAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_FAILED",
            clientErrorCode: "DA3011",
            messageResource: "string_27240",
        };
        static MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_METADATA_FAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_SERVERCONNECTIVITYCHECK_HEALTH_CHECK_METADATA_FAILED",
            clientErrorCode: "DA3012",
            messageResource: "string_27250",
        };
        static MICROSOFT_DYNAMICS_POS_CLIENTBROKER_COMMUNICATION_ERROR: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_POS_CLIENTBROKER_COMMUNICATION_ERROR",
            clientErrorCode: "DA3014",
            messageResource: "string_29841",
            messageDetailsResource: ["string_29842", "string_29843"]
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHENTICATIONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHENTICATIONFAILED",
            clientErrorCode: "DA1002",
            messageResource: "string_27010"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHORIZATIONFAILED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AUTHORIZATIONFAILED",
            clientErrorCode: "DA1010",
            messageResource: "string_29257"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERCOMMUNICATIONFAILURE: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERCOMMUNICATIONFAILURE",
            clientErrorCode: "DA2009",
            messageResource: "string_29240"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANNELCONFIGURATION: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDCHANNELCONFIGURATION",
            clientErrorCode: "DA2010",
            messageResource: "string_29242",
            messageDetailsResource: ["string_29403", "string_29404", "string_29405"]
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERPASSWORDEXPIRED",
            messageResource: "string_512"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED: Model.Entities.IErrorDetails = {
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REALTIMESERVICENOTSUPPORTED",
            messageResource: "string_29831"
        };
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAUTHENTICATIONCREDENTIALS: Model.Entities.IErrorDetails = {
            // Error comes already localized from Retail Server.
            serverErrorCode: "MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDAUTHENTICATIONCREDENTIALS"
            // This type of error already has error message.
        };

        // Client error codes mapped to locale strings
        static APPLICATION_ERROR: string = "string_29000";
        static APPLICATION_STORE_FAILED_TO_SAVE_DEVICE_CONFIGURATION: string = "string_1475";
        static APPLICATION_STORE_INITIALIZATION_DATA_FAILED_TO_LOAD = "string_29007";
        static PRICE_CHECK_INITIALIZATION_DATA_FAILED_TO_LOAD = "string_29022";
        static ORDERS_CANNOT_INCLUDE_GIFTCARDS = "string_29023";
        static ORDERS_CANNOT_INCLUDE_RETURNS = "string_29024";
        static CART_LINE_MISSING_PRODUCT_NAME_ERROR: string = "string_1242";
        static OPERATOR_ID_PASSWORD_NOT_SPECIFIED: string = "string_29001";
        static OPERATOR_PASSWORD_NOT_SPECIFIED: string = "string_29019";
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
        static AAD_AUTHENTICATION_FAILED: string = "string_29046";
        static AAD_USER_ACCOUNT_IDENTIFIER_NOT_PROVIDED: string = "string_29053";
        static RETAILSERVER_URL_DISCOVERY_FAILED: string = "string_29047";
        static DEVICE_ACTIVATION_DETAILS_NOT_SPECIFIED: string = "string_1407";
        static DEVICE_DEACTIVATION_INCOMPLETE_TRANSACTION_ERROR: string = "string_1421";
        static CHANGE_PASSWORD_DETAILS_NOT_SPECIFIED: string = "string_6805";
        static NEW_PASSWORD_AND_CONFIRMATION_NOT_MATCHING_ERROR: string = "string_6806";
        static RESET_PASSWORD_DETAILS_NOT_SPECIFIED: string = "string_6810";
        static RESET_PASSWORD_CURRENT_EMPLOYEE: string = "string_6811";
        static OLD_AND_NEW_PASSWORD_MATCHING_ERROR: string = "string_6607";
        static OPERATION_ISSUE_CREDIT_MEMO_NOT_AVAILABLE: string = "string_29801";
        static OPERATION_ISSUE_CREDIT_MEMO_CALCULATE_TRANSACTION: string = "string_4376";
        static PAYMENT_CREDIT_MEMO_NEGATIVE_BALANCE: string = "string_29827";
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
        static PAYMENT_ONLY_ONE_CUSTOMER_ACCOUNT_PAYMENT_ALLOWED: string = "string_1188";
        static PAYMENT_AUTHORIZED_VOID_FAILED: string = "string_1189";
        static PAYMENT_CAPTURED_VOID_FAILED: string = "string_1190";
        static PAYMENT_UNABLE_AUTHORIZE_OR_REFUND: string = "string_4931";
        static PAYMENT_CARD_SECURITY_CODE_EMPTY: string = "string_1168";
        static PAYMENT_CREDIT_MEMO_NUMBER_EMPTY: string = "string_1169";
        static PAYMENT_LOYALTY_CARD_NUMBER_EMPTY: string = "string_1170";
        static PAYMENT_GIFT_CARD_NUMBER_EMPTY: string = "string_1171";
        static PAYMENT_CUSTOMER_ACCOUNT_EMPTY: string = "string_1172";
        static PAYMENT_INVALID_CARD_NUMBER: string = "string_1175";
        static PAYMENT_INVALID_SECURITY_CODE: string = "string_1176";
        static PAYMENT_INVALID_ZIP_CODE: string = "string_1177";
        static QUANTITY_MUST_BE_NUMBER: string = "string_168";
        static CART_IS_EMPTY: string = "string_29008";
        static RETAIL_SERVER_REDIRECT_ERROR = "RETAIL_SERVER_REDIRECT_ERROR";
        static OPERATION_NOT_VALID_FOR_BIG_ENDIAN_SYSTEM = "string_29824";
        static INVALID_INCOME_EXPENSE_LINE_COLLECTION = "string_4122";
        static RECEIPT_PREVIEW = "string_4127";
        static CUSTOMER_ORDER_CANNOT_PERFORM_OPERATION = "string_4451";
        static CARTLINE_DISCOUNTINUED = "string_4452";
        static CANNOT_REMOVE_CUSTOMER_PARTIAL_ORDER = "string_4453";
        static SCALE_UNSPECIFIED_WITHOUT_MANUAL_ENTRY = "string_5316";
        static SCALE_RETURNED_ZERO_WITHOUT_MANUAL_ENTRY = "string_5317";
        static APPLICATION_CONFIGURATION_LOADING_ERROR: string = "string_29951";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DEMOMODEOPERATIONNOTSUPPORTED: string = "string_29952";
        static MANAGER_OVERRIDE_CANCELED_ERROR: string = "string_29029";
        static SELECTED_CARTLINES_CONTAINS_VOIDED_PRODUCTS: string = "string_29832";
        static CUSTOMERORDER_MANUAL_DEPOSIT_REQUIRED: string = "string_29833";
        static CANNOT_CHANGE_QUANTITY_LESS_ONE: string = "string_29835";
        static CANNOT_CHANGE_QUANTITY_WHEN_SERIALIZED: string = "string_4363";
        static LOCAL_STORAGE_IS_NOT_AVAILABLE: string = "string_29836";
        static BROWSER_IS_NOT_SUPPORTED = "string_29850";
        static APPLICATION_UPDATE_REQUIRED: string = "string_29052";
        static ORDER_CANNOT_BE_EDITED: string = "string_29030";
        static INVOICE_COMMENTS_NOT_AVAILABLE: string = "string_29301";
        static REASONCODE_LENGTH_EXCEEDED: string = "string_29038";
        static REASONCODE_LENGTH_SHORT: string = "string_29039";
        static NUMBER_INPUT_VALUE_GREATER_THAN_MAXIMUM_ALLOWED: string = "string_29040";
        static NUMBER_INPUT_VALUE_LESS_THAN_MINIMUM_ALLOWED: string = "string_29041";
        static DEVICE_NOT_AUTHENTICATED: string = "string_29393";
        static FINISH_TRANSACTION_BEFORE_STARTING_ANOTHER: string = "string_4322";
        static NOT_SUPPORTED_IN_OFFLINE_MODE_WHEN_HARDWARE_STATION_NOT_ACTIVE: string = "string_29839";
        static PAYMENT_INVALID_CALCULATE_TRANSACTION_REQUIRED: string = "string_4383";
        static INVALID_CUSTOMER_ACCOUNT_DEPOSIT_LINE_COLLECTION = "string_4180";
        static ITEM_ADD_INVALID_NON_UPDATABLE_PRICE = "string_5726";
        static ACCESS_WRONG_DEVICE_TERMINAL: string = "string_29055";

        // Task Recorder codes
        static TASK_RECORDER_SESSION_INVALID_STATE = "string_10200";
        static TASK_RECORDER_SESSION_NO_ACTIVE_TASK = "string_10201";
        static TASK_RECORDER_CONFIGURATION_ERROR = "string_10202";
        static TASK_RECORDER_MANAGER_BUSY = "string_10203";
        static TASK_RECORDER_MANAGER_NO_ACTIVE_SESSION = "string_10204";
        static TASK_RECORDER_VIEWMANAGER_VIEW_NOT_FOUND = "string_10205";
        static TASK_RECORDER_VIEWMANAGER_LOAD_FAILED = "string_10206";
        static TASK_RECORDER_INVALID_DOM = "string_10207";
        static TASK_RECORDER_CONTROLLER_NOT_SUPPORTED_STATE = "string_10208";
        static TASK_RECORDER_STEP_VIEW_MODEL_NOT_FOUND = "string_10209";
        static TASK_RECORDER_COULDNT_TAKE_SCREENSHOT = "string_10210";
        static TASK_RECORDER_COULDNT_UPLOAD_SCREENSHOT = "string_10211";
        static TASK_RECORDER_TASK_VIEW_MODEL_NOT_FOUND = "string_10212";
        static TASK_RECORDER_ODATA_TYPE_NOT_FOUND = "string_10213";
        static TASK_RECORDER_ERROR_OCCURED_DURING_UPLOADING_FILE = "string_10214";
        static TASK_RECORDER_ERROR_OCCURRED_DURING_DISPLAYING_SAVE_DIALOG = "string_10215";
        static TASK_RECORDER_COULDNT_SAVE_FILE = "string_10216";
        static TASK_RECORDER_COULDNT_COMPLETE_UPDATES_FOR_FILE = "string_10217";
        static TASK_RECORDER_COULDNT_DOWNLOAD_FILE = "string_10218";
        static TASK_RECORDER_UNEXPECTED_FILE_EXTENSION = "string_10219";
        static TASK_RECORDER_XML_EXPORT_ERROR = "string_10220";
        static TASK_RECORDER_WORD_EXPORT_ERROR = "string_10221";
        static TASK_RECORDER_SAVE_FILE_ERROR = "string_10222";
        static TASK_RECORDER_BPM_PACKAGE_EXPORT_ERROR = "string_10223";
        static TASK_RECORDER_COULDNT_DOWNLOAD_RECORDING = "string_10224";
        static TASK_RECORDER_SAVE_SESSION_AS_RECORDING_BUNDLE_ERROR = "string_10225";

        // Client error codes - hardware station
        static CANNOT_CHANGE_HARDWARE_STATION_WHEN_PAYMENT_DONE = "string_6009";
        static HARDWARESTATION_CHANGE_ERROR_LINE_DISPLAY_ACTIVE = "string_6014";
        static HARDWARESTATION_BALANCE_TOKEN_ERROR = "string_7204";
        static HARDWARESTATION_MUST_BE_PAIRED_BEFORE_ACTIVATE = "string_6010";
        static HARDWARESTATION_SWITCH_NOT_ALLOWED_TO_NONSHARED = "string_6017";
        static SHIFT_NOT_ALLOWED_ON_ACTIVE_HARDWARE_PROFILE = "string_6018";

        // Client error codes - store information and details
        static STORE_NOT_FOUND = "string_29016";

        // Client error codes - customer information and details
        static CUSTOMER_NOT_FOUND: string = "string_29048";

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
        static RETURN_CANNOT_CHANGE_PRODUCT_QUANTITY: string = "string_4421";

        // Client error codes - payment
        static CREDIT_MEMO_INVALID_AMOUNT: string = "string_29800";
        static CANNOT_PAYMENT_TRANSACTION_COMPLETED: string = "string_4356";
        static CALCULATE_TOTAL_BEFORE_PAYMENT: string = "string_4373";

        //Client error codes - inventory
        static NO_PRICECHECK_ON_PRODUCTS: string = "string_3523";
        static NO_PRODUCT_INFORMATION: string = "string_3873";

        // Client error codes - salesOrders
        static CART_UNAVAILABLE_FOR_PICK_UP: string = "string_4539";
        static CART_LINES_UNAVAILABLE_FOR_PICK_UP: string = "string_4540";
        static PICK_LIST_CAN_NOT_BE_CREATED: string = "string_4544";
        static PACK_SLIP_CAN_NOT_BE_CREATED: string = "string_4546";
        static CUSTOMER_ORDER_OPERATION_PICKUP_CANCEL_RETURN_NOT_SUPPORTED = "string_29028";
        static CREATE_OR_EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY = "string_29032";
        static EDIT_CUSTOMER_ORDER_OR_QUOTATION_ONLY = "string_29033";
        static NO_STORE_SELECTED_FOR_PICKUP = "string_29034";
        static ALL_PRODUCTS_SELECTED_PICKUP_OR_SHIP_SELECTED = "string_29035";
        static INVALID_SHIPPING_CHARGES = "string_2543";
        static NO_ADDRESSES_SELECTED_FOR_SHIP = "string_29036";
        static NO_SHIPPING_METHODS_SELECTED_FOR_SHIP = "string_29037";
        static CREATE_OR_EDIT_QUOTATION_ONLY = "string_29042";
        static EDIT_CUSTOMER_ORDER_ONLY = "string_29043";
        static EDIT_OR_PICKUP_CUSTOMER_ORDER_ONLY = "string_29050";

        // Client error codes - Advanced Search Sales Orders
        static INVALID_SEARCH_CRITERIA: string = "string_4584";
        static START_DATE_NOT_IN_FUTURE: string = "string_4585";
        static START_DATE_NOT_MORE_RECENT_THAN_END_DATE: string = "string_4586";

        // Client error codes - customer order cancellation
        static CANCELLATION_CHARGE_IS_NOT_VALID: string = "string_4542";
        static CANCELLATION_CHARGE_INVALID_NEGATIVE_AMOUNT: string = "string_29026";
        static CANCELLATION_CHARGE_INVALID_OPERATION: string = "string_29027";
        static ORDER_CANNOT_BE_CANCELED: string = "string_4541";

        // Client error codes - customer account deposit
        static CUSTOMERACCOUNTDEPOSIT_MULTIPLECARTLINESNOTALLOWED = "string_29340";
        
        // Client error codes - discounts
        static UNSUPPORTED_APPLY_DISCOUNT_OPERATION: string = "string_5600";
        static MISSING_CARTLINE_ON_APPLY_DISCOUNT: string = "string_5601";
        static MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED: string = "string_5602";
        static MAXIMUM_LINE_DISCOUNT_PERCENT_EXCEEDED: string = "string_5603";
        static MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED: string = "string_5604";
        static MAXIMUM_TOTAL_DISCOUNT_PERCENT_EXCEEDED: string = "string_5605";
        static MAXIMUM_LINE_DISCOUNT_AMOUNT_EXCEEDED_PRICE: string = "string_5617";
        static MAXIMUM_TOTAL_DISCOUNT_AMOUNT_EXCEEDED_SUBTOTAL: string = "string_5618";

        static PERMISSION_DENIED_LINE_AMOUNT_DISCOUNT: string = "string_5619";
        static PERMISSION_DENIED_LINE_PERCENT_DISCOUNT: string = "string_5620";
        static PERMISSION_DENIED_TOTAL_AMOUNT_DISCOUNT: string = "string_5621";
        static PERMISSION_DENIED_TOTAL_PERCENT_DISCOUNT: string = "string_5622";
        static PERMISSION_DENIED_CANNOT_APPLY_DISCOUNT_TO_LINE_WITH_OVERRIDDEN_PRICE: string = "string_5623";

        // Client error codes - price override
        static PRICE_OVERRIDE_NOT_VALID_ONE_OR_MORE_ITEMS: string = "string_5705";
        static PRICE_OVERRIDE_PRICE_EXCEEDS_MAXIMUM_DEVICE_PRICE: string = "string_5715";
        static PRICE_OVERRIDE_PRODUCT_IS_VOIDED: string = "string_29803";
        static PRICE_OVERRIDE_PRICE_CANNOT_BE_NEGATIVE: string = "string_29009";
        static PRICE_OVERRIDE_INVALID_PRICE: string = "string_29010";
        static PRICE_OVERRIDE_PRICE_NOT_A_NUMBER: string = "string_29011";
        static PRICE_OVERRIDE_PRICE_CANNOT_BE_ZERO: string = "string_5717";
        static PRICE_OVERRIDE_PRICE_MUST_BE_POSITIVE: string = "string_29020";
        static PRICE_OVERRIDE_ONLY_LOWER_AMOUNTS_ALLOWED: string = "string_5718";
        static PRICE_OVERRIDE_ONLY_HIGHER_AMOUNTS_ALLOWED: string = "string_5719";
        static PRICE_OVERRIDE_ONLY_LOWER_OR_EQUAL_AMOUNTS_ALLOWED: string = "string_5720";
        static PRICE_OVERRIDE_ONLY_HIGHER_OR_EQUAL_AMOUNTS_ALLOWED: string = "string_5721";
        static PRICE_OVERRIDE_NOT_ALLOWED_FOR_PRODUCT: string = "string_5722";
        static PRICE_OVERRIDE_NONE_ALLOWED: string = "string_5723";

        // Client error codes - key in prices
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ENTERINGPRICENOTALLOWED: string = "string_4379";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MUSTKEYINEQUALHIGHERPRICE: string = "string_4380";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MUSTKEYINEQUALLOWERPRICE: string = "string_4381";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MUSTKEYINNEWPRICE: string = "string_4382";

        // Client error codes - barcodes
        static MANUAL_QUANTITY_NOT_ALLOWED_ON_PRICE_EMBEDDED_BARCODE: string = "string_4458";
        static BARCODE_TYPE_NOT_SUPPORTED: string = "string_4459";
        static PRODUCT_ASSOCIATED_WITH_BARCODE_NOT_FOUND: string = "string_4460";
        static CUSTOMER_ASSOCIATED_WITH_BARCODE_NOT_FOUND: string = "string_4461";

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
        static UNIT_OF_MEASURE_CANNOT_BE_CHANGED: string = "string_3207";
        static UNIT_OF_MEASURE_CONVERSION_NOT_DEFINED: string = "string_3208";
        static UNIT_OF_MEASURE_NOT_VALID_ITEM_NOT_ALLOW_QUANTITY_UPDATE: string = "string_3209";

        static NOT_IMPLEMENTED: string = "string_29003";

        // Client error codes - kit disassembly
        static KIT_BLOCKED_FOR_DISASSEMBLY_AT_REGISTER: string = "string_420";

        // Client error codes - peripherals
        static PERIPHERALS_HARDWARESTATION_NOTCONFIGURED = "string_4908";
        static PERIPHERALS_HARDWARESTATION_COMMUNICATION_FAILED = "string_4914";
        static PERIPHERALS_BARCODE_SCANNER_NOTFOUND: string = "string_4900";
        static PERIPHERALS_BARCODE_SCANNER_ENABLE_FAILED: string = "string_4901";
        static PERIPHERALS_CASHDRAWER_ALREADY_OPENED: string = "string_4936";
        static PERIPHERALS_MSR_NOTFOUND: string = "string_4902";
        static PERIPHERALS_MSR_ENABLE_FAILED: string = "string_4903";
        static PERIPHERALS_PRINTER_FAILED: string = "string_4904";
        static PERIPHERAL_PAYMENT_UNKNOWN_ERROR: string = "string_4919";
        static PERIPHERAL_UNSUPPORTED_PRINTERTYPE_ERROR: string = "string_4937";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PRINTER_ERROR = "string_4904";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CASHDRAWER_ERROR = "string_4905";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_SCALE_ERROR = "string_4906";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PAYMENTTERMINAL_ERROR = "string_4907";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_DUALDISPLAY_ERROR = "string_4918";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PERIPHERALNOTFOUND = "string_4917";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PAIRINGERROR = "string_6011";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PINPAD_ERROR = "string_4923";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_SIGNATURECAPTURE_ERROR = "string_4924";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_LINEDISPLAY_ERROR = "string_4925";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_LINEDISPLAY_CHARACTERSETNOTSUPPORTED = "string_4926";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PERIPHERALISLOCKED = "string_4927";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_ERROR_EVENT_FROM_PERIPHERAL = "string_4932";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_BARCODESCANNER_ERROR = "string_4933";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_MAGNETICSWIPEREADER_ERROR = "string_4934";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CARDPAYMENT_ERROR = "string_7202";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CARDPAYMENT_INVALIDTOKEN = "string_7203";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_AMOUNTEXCEEDSMAXIMUMLIMIT = "string_7208";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_AMOUNTLESSTHANMINIMUMLIMIT = "string_7209";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CASHBACKAMOUNTEXCEEDSLIMIT = "string_7210";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_EMPTYPAYMENTPROPERTIES = "string_7211";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_PERIPHERALLOCKNOTACQUIRED =  "string_4935";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CARDPAYMENT_MISSINGASSEMBLYNAMEINCONFIGURATION = "string_7215";
        static MICROSOFT_DYNAMICS_COMMERCE_HARDWARESTATION_CARDPAYMENT_MISSINGPAYMENTCONNECTORNAME = "string_7216";

        // Client error codes - store operations
        static AMOUNT_IS_NOT_VALID: string = "string_4102";
        static SHIFT_IS_NOT_VALID: string = "string_4103";
        static OPERATION_NOT_ALLOWED_PRODUCT_IS_VOIDED: string = "string_29803";
        static OPERATION_NOT_ALLOWED_PRODUCT_IS_FOR_A_RECEIPT: string = "string_29804";
        static OPERATION_NOT_ALLOWED_FOR_A_GIFT_CARD: string = "string_29805";
        static OPERATION_NOT_ALLOWED_LINKED_PRODUCT: string = "string_29806";
        static OPERATION_NOT_ALLOWED_MULTIPLE_CART_LINES: string = "string_29807";
        static OPERATION_NOT_ALLOWED_INCOME_EXPENSE_TRANSACTION: string = "string_29808";
        static OPERATION_NOT_ALLOWED_TIME_CLOCK_DISABLED: string = "string_29809";
        static OPERATION_NOT_ALLOWED_FINISH_CURRENT_TRANSACTION: string = "string_4125";
        static OPERATION_NOT_ALLOWED_NO_CURRENT_TRANSACTION: string = "string_4175";
        static OPERATION_NOT_ALLOWED_NO_CART_LINE_SELECTED: string = "string_29822";
        static OPERATION_NOT_ALLOWED_IN_NONDRAWER_MODE: string = "string_4141";
        static OPERATION_NOT_ALLOWED_NO_PAYMENT_LINE_SELECTED: string = "string_29828";
        static OPERATION_NOT_ALLOWED_MULTIPLE_PAYMENT_LINES: string = "string_29829";
        static OPERATION_NOT_ALLOWED_IN_OFFLINE_STATE: string = "string_29831";
        static OPERATION_NOT_ALLOWED_PRICE_IS_OVERRIDDEN: string = "string_29837";
        static OPERATION_NOT_ALLOWED_CUSTOMER_ACCOUNT_DEPOSIT: string = "string_29844";
        static OPERATION_NOT_ALLOWED_PERMISSION_DENIED_MANAGER_OVERRIDE_NOT_ALLOWED: string = "string_522";
        static OPERATION_NOT_ALLOWED_FOR_A_SERIALIZED_ITEM: string = "string_29848";
        static OPERATION_NOT_ALLOWED_KEY_IN_QUANTITY_NOT_ALLOWED_FOR_ITEM: string = "string_29849";
        static RECEIPT_NOT_AVAILABlE_FOR_ORDER: string = "string_4173";
        static RECEIPT_EMAIL_IS_EMPTY: string = "string_4126";
        static OPERATION_NOT_ALLOWED_PERMISSION_DENIED: string = "string_511";
        static OPERATION_VALIDATION_INVALID_ARGUMENTS: string = "string_29018";
        static INVALID_BLANK_OPERATION: string = "string_29838";
        static TRANSACTION_NOT_SELECTED: string = "string_4147";
        static CHANGE_PASSWORD_NOT_ALLOWED_PERMISSION_DENIED_MANAGER_OVERRIDE_NOT_ALLOWED: string = "string_523";
        static MATCHING_VARIANT_NOT_FOUND: string = "string_29846";
        static REQUIRED_DIMENSION_VALUES_MISSING: string = "string_29847";

        // Client error codes - affiliation
        static INVALID_AFFILIATION_COLLECTION: string = "string_5205";

        // Client error codes - sales tax override
        static MISSING_CARTLINE_ON_APPLY_TAX_OVERRDE: string = "string_4423";
        static NO_TAX_OVERRIDE_REASON_CODES_CONFIGURED: string = "string_4422";

        // Client error code - Offline
        static CANNOT_SWITCH_ONLINE_CART_IN_PROGRESS = "string_6607";
        static CANNOT_SWITCH_OFFLINE_NOT_AVAILABLE = "string_6608";
        static CANNOT_SWITCH_TRANSFER_FAILED = "string_6609";
        static CANNOT_SWITCH_OFFLINE_REQUIRE_RELOGIN = "string_6622";
        static CANNOT_SYNC_DATA_IN_OFFLINE = "string_6629";
        static OFFLINE_DATA_IS_SYNCING = "string_6625";
        static OFFLINE_MODE_NOT_SUPPORTED = "string_6628";
        static CANNOT_TRANSFER_SHIFT_TO_ONLINE = "string_6637";
        
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
        static ASYNC_CLIENT_FAIL_UPDATE_UPLOAD_FAILED_STATUS: string = "string_29396";
        static ASYNC_CLIENT_FAIL_UPLOAD_DATA: string = "string_29397";
        static ASYNC_CLIENT_RETAIL_SERVER_UNAVAILABLE: string = "string_29406";
        static ASYNC_CLIENT_FAILED_TO_GET_OFFLINE_SYNC_STATS: string = "string_29409";
        
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
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INSUFFICIENTQUANTITYONHAND: string = "string_29210";
        static INVALID_CURRENCY_AMOUNT: string = "string_29012";
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
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATECOMMUNICATIONERROR: string = "string_29234";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERRESPONSEPARSINGERROR: string = "string_29241";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_AGGREGATEVALIDATIONERROR: string = "string_29255";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCOUNTISALLOWEDONLYFORCREATIONANDEDITION: string = "string_5613";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ITEMDISCONTINUEDFROMCHANNEL: string = "string_29268";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_QUOTEMUSTHAVEVALIDQUOTATIONEXPIRYDATE: string = "string_4321";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOPICKUPMORETHANQTYREMAINING: string = "string_29821";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CANNOTCHANGECUSTOMERIDWHENEDITINGCUSTOMERORDER: string = "string_4420";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGETENDERTYPENOTSUPPORTED: string = "string_29369";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_ZEROPRICEISNOTALLOWED: string = "string_29399";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_DISCONTINUEDPRODUCTSREMOVEDFROMCART = "string_4457";

        static BAD_REQUEST: string = "string_29274";
        static NOT_AUTHORIZED: string = "string_29275";
        static FORBIDDEN: string = "string_29276";
        static PRECONDITION_FAILED: string = "string_29277";
        static SERVICE_UNAVAILABLE: string = "string_29278";
        static SERVER_TIMEOUT: string = "string_29279";
        static POSSIBLE_LOOPBACK_BLOCKED: string = "string_29329";
        static SERVER_INTERNAL_ERROR: string = "string_29395";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NONDRAWEROPERATIONSONLY = "string_2123";
        static MICROSOFT_DYNAMICS_INTERNAL_SERVER_ERROR: string = ErrorTypeEnum.GENERICERRORMESSAGE;
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CARTNOTACTIVE: string = "string_29834";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDUSERTOKEN: string = "string_29275";

        // Retail Server and Hardware Station shared Payment Error codes mapped to locale strings
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGENERATETOKEN: string = "string_1175";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOAUTHORIZEPAYMENT: string = "string_29280";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCAPTUREPAYMENT: string = "string_29354";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTREQUIRESMERCHANTPROPERTIES: string = "string_29400";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOCANCELPAYMENT: string = "string_29401";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BALANCEAMOUNTEXCEEDSMAXIMUMALLOWEDVALUE: string = "string_29355";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CHANGEBACKISNOTALLOWED: string = "string_29356";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INCORRECTPAYMENTAMOUNTSIGN: string = "string_29357";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_OVERTENDERAMOUNTEXCEEDSMAXIMUMALLOWEDVALUE: string = "string_29358";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERLINE: string = "string_29359";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMAXIMUMAMOUNTPERTRANSACTION: string = "string_29360";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERLINE: string = "string_29361";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEEDSMINIMUMAMOUNTPERTRANSACTION: string = "string_29362";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTMUSTBEUSEDTOFINALIZETRANSACTION: string = "string_29363";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_MANUALCARDNUMBERNOTALLOWED = "string_29825";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_TENDERLINECANNOTBEVOIDED = "string_29826";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETORETRIEVECARDPAYMENTACCEPTRESULT: string = "string_7206";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_UNABLETOGETCARDPAYMENTACCEPTPOINT: string = "string_4377";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDPAYMENTREQUEST = "string_29293";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTALREADYVOIDED = "string_29293";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_INVALIDLOYALTYCARDNUMBER: string = "string_29286";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTAMOUNTEXCEEDSGIFTBALANCE = "string_29301";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_BLOCKEDLOYALTYCARD: string = "string_29287";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENDERLOYALTYCARD: string = "string_29322";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOTENOUGHREWARDPOINTS = "string_29290";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_REFUNDAMOUNTMORETHANALLOWED = "string_29316";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_NOMORETHANONELOYALTYTENDER = "string_29291";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTUSINGUNAUTHORIZEDACCOUNT: string = "string_29351";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_CUSTOMERACCOUNTPAYMENTISNOTALLOWEDFORCUSTOMERORDERDEPOSITANDCANCELLATION = "string_29021";        
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTKEYNOTFOUND: string = "string_1195";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_USERSESSIONNOTOPENED: string = "string_29054";

        // Payment error codes mapped to locale strings
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDOPERATION = "string_29601";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_APPLICATIONERROR = "string_29602";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_GENERICCHECKDETAILSFORERROR = "string_29603";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DONOTAUTHORIZED = "string_29604";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_USERABORTED = "string_29605";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_LOCALENOTSUPPORTED = "string_29606";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDMERCHANTPROPERTY = "string_29607";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_COMMUNICATIONERROR = "string_29608";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDARGUMENTCARDTYPENOTSUPPORTED = "string_29609";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_VOICEAUTHORIZATIONNOTSUPPORTED = "string_29610";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONNOTSUPPORTED = "string_29611";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MULTIPLECAPTURENOTSUPPORTED = "string_29612";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_BATCHCAPTURENOTSUPPORTED = "string_29613";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNSUPPORTEDCURRENCY = "string_29614";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNSUPPORTEDCOUNTRY = "string_29615";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTREAUTHORIZEPOSTCAPTURE = "string_29616";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTREAUTHORIZEPOSTVOID = "string_29617";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTURENOTSUPPORTED = "string_29618";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDEXPIRED = "string_29619";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REFERTOISSUER = "string_29620";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOREPLY = "string_29621";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_HOLDCALLORPICKUPCARD = "string_29622";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDAMOUNT = "string_29623";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ACCOUNTLENGTHERROR = "string_29624";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ALREADYREVERSED = "string_29625";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CANNOTVERIFYPIN = "string_29626";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDNUMBER = "string_29627";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCVV2 = "string_29628";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CASHBACKNOTAVAILABLE = "string_29629";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDTYPEVERIFICATIONERROR = "string_29630";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DECLINE = "string_29631";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ENCRYPTIONERROR = "string_29632";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOACTIONTAKEN = "string_29633";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOSUCHISSUER = "string_29634";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PINTRIESEXCEEDED = "string_29635";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SECURITYVIOLATION = "string_29636";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SERVICENOTALLOWED = "string_29637";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_STOPRECURRING = "string_29638";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_WRONGPIN = "string_29639";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2MISMATCH = "string_29640";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DUPLICATETRANSACTION = "string_29641";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REENTER = "string_29642";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AMOUNTEXCEEDLIMIT = "string_29643";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONEXPIRED = "string_29644";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONALREADYCOMPLETED = "string_29645";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONISVOIDED = "string_29646";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PROCESSORDUPLICATEBATCH = "string_29647";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_AUTHORIZATIONFAILURE = "string_29648";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDMERCHANTCONFIGURATION = "string_29649";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDEXPIRATIONDATE = "string_29650";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDHOLDERNAMEFIRSTNAMEREQUIRED = "string_29651";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDHOLDERNAMELASTNAMEREQUIRED = "string_29652";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_FILTERDECLINE = "string_29653";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDADDRESS = "string_29654";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2REQUIRED = "string_29655";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CARDTYPENOTSUPPORTED = "string_29656";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_UNIQUEINVOICENUMBERREQUIRED = "string_29657";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_POSSIBLEDUPLICATE = "string_29658";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_PROCESSORREQUIRESLINKEDREFUND = "string_29659";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CRYPTOBOXUNAVAILABLE = "string_29660";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CVV2DECLINED = "string_29661";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MERCHANTIDINVALID = "string_29662";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TRANNOTALLOWED = "string_29663";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TERMINALNOTFOUND = "string_29664";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDEFFECTIVEDATE = "string_29665";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INSUFFICIENTFUNDS = "string_29666";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONMAXREACHED = "string_29667";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_REAUTHORIZATIONNOTALLOWED = "string_29668";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_DATEOFBIRTHERROR = "string_29669";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ENTERLESSERAMOUNT = "string_29670";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_HOSTKEYERROR = "string_29671";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCASHBACKAMOUNT = "string_29672";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTRANSACTION = "string_29673";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTUREREQUIRED = "string_29674";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_IMMEDIATECAPTUREREQUIREDMAC = "string_29675";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MACREQUIRED = "string_29676";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_BANKCARDNOTSET = "string_29677";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDREQUEST = "string_29678";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTRANSACTIONFEE = "string_29679";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOCHECKINGACCOUNT = "string_29680";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOSAVINGSACCOUNT = "string_29681";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_RESTRICTEDCARDTEMPORARILYDISALLOWEDFROMINTERCHANGE = "string_29682";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_MACSECURITYFAILURE = "string_29683";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_EXCEEDSWITHDRAWALFREQUENCYLIMIT = "string_29684";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCAPTUREDATE = "string_29685";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_NOKEYSAVAILABLE = "string_29686";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KMESYNCERROR = "string_29687";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KPESYNCERROR = "string_29688";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_KMACSYNCERROR = "string_29689";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_RESUBMITEXCEEDSLIMIT = "string_29690";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_SYSTEMPROBLEMERROR = "string_29691";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_ACCOUNTNUMBERNOTFOUNDFORROW = "string_29692";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDTOKENINFOPARAMETERFORROW = "string_29693";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_EXCEPTIONTHROWNFORROW = "string_29694";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_TRANSACTIONAMOUNTEXCEEDSREMAINING = "string_29695";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDARGUMENTTENDERACCOUNTNUMBER = "string_29696";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDCARDTRACKDATA = "string_29697";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDRESULTACCESSCODE = "string_29698";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_GENERALEXCEPTION = "string_29699";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_INVALIDVOICEAUTHORIZATIONCODE = "string_29700";
        static MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_PAYMENTEXCEPTION_CASHBACKAMOUNTEXCEEDSTOTALAMOUNT: string = "string_29701";

        static MICROSOFT_DYNAMICS_POS_SERVER_URL_NOT_HTTPS: string = "string_8085";
        static MICROSOFT_DYNAMICS_POS_NO_PRINTABLE_RECEIPTS: string = "string_1827";
    }

    export class ErrorHelper {
        private static AGGREGATED_ERROR_RESOUCEIDS: string[] = [
            "Microsoft_Dynamics_Commerce_Runtime_AggregateValidationError",
            "Microsoft_Dynamics_Commerce_Runtime_AggregateCommunicationError",
            "Microsoft_Dynamics_Commerce_Runtime_InvalidCartLinesAggregateError"];

        public static HTTPRESPONSE_DNS = "DNS";
        public static HTTPRESPONSE_TIMED = "TIMED";
        public static HTTPRESPONSE_FAILED = "FAILED";
        public static HTTPRESPONSE_OUT = "OUT";
        public static HTTPRESPONSE_FIREWALL = "FIREWALL";

        public static isAggregatedErrorResourceId(errorResourceId: string): boolean {
            return ErrorHelper.AGGREGATED_ERROR_RESOUCEIDS.indexOf(errorResourceId) != -1;
        }

        /**
         * Map response status code to error.
         *
         * @param {any} Error returned by retail server call.
         */
        public static MapResponseStatusCodeToError(errorMessage: string, statusCode: number): Model.Entities.Error {
            var errorCode = "";
            var canRetry = false;

            switch (statusCode) {
                case 400:
                    errorCode = ErrorTypeEnum.BAD_REQUEST;
                    break;
                case 401:
                    errorCode = ErrorTypeEnum.NOT_AUTHORIZED;
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
                    errorCode = ErrorTypeEnum.SERVER_INTERNAL_ERROR;
                    break;
            }

            if (errorMessage && errorMessage.toUpperCase() == "TIMEOUT") {
                errorCode = ErrorTypeEnum.SERVER_TIMEOUT;
            }

            return new Model.Entities.Error(errorCode, canRetry);
        }

        /**
         * Gets whether an error code is present in the error collection or not.
         * @param {string} errorType The error resource identifier.
         * @return {boolean} Whether an error code is present in the error collection or not.
         */
        public static hasError(errors: Model.Entities.Error[], errorType: string): boolean {

            if (ArrayExtensions.hasElements(errors)) {
                for (var i = 0; i < errors.length; i++) {

                    // get errorType from errorResourceId
                    var error: Model.Entities.Error = errors[i];
                    var errorTypeValueObj: any = error.ErrorCode != null
                        ? ErrorTypeEnum[error.ErrorCode.toUpperCase()]
                        : null;

                    var errorTypeValue: string = null;
                    if (ObjectExtensions.isString(errorTypeValueObj)) {
                        errorTypeValue = errorTypeValueObj;
                    } else if (!ObjectExtensions.isNullOrUndefined(errorTypeValueObj) && !ObjectExtensions.isNullOrUndefined(errorTypeValueObj.messageResource)) {
                        errorTypeValue = errorTypeValueObj.messageResource;
                    }

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
        public static isRetryable(errors: Model.Entities.Error[]): boolean {
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
         * @param {Model.Entities.Error[]} errors the error collection.
         * @return {string} a formated string containing all error codes.
         */
        public static getErrorResourceIds(errors: Model.Entities.Error[]): string {
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
         * @param {Model.Entities.Error[]} errors the error collection.
         * @return {string} a formated string containing all error messages.
         */
        public static getErrorMessages(errors: Model.Entities.Error[]): string {
            var result: string = "";

            if (ArrayExtensions.hasElements(errors)) {
                var errorResourceIds: string[] = [];
                for (var i = 0; i < errors.length; i++) {
                    errorResourceIds.push(ErrorHelper.formatErrorMessage(errors[i]));
                }

                result = errorResourceIds.join(", ");
            }

            return result;
        }

        public static formatErrorMessage(error: Model.Entities.Error): string {
            var errorMessage: string;

            if (error && error.ErrorCode) {
                if (error.ErrorCode.toUpperCase() !== ErrorTypeEnum.MICROSOFT_DYNAMICS_COMMERCE_RUNTIME_HEADQUARTERTRANSACTIONSERVICEMETHODCALLFAILURE.serverErrorCode) {
                    // Tries client side error localization based on the ErrorCode.
                    var errorDetails: Model.Entities.IErrorDetails = ErrorHelper.resolveError(error.ErrorCode, false);

                    if (!StringExtensions.isNullOrWhitespace(errorDetails.messageResource)) {
                        // If the error message resource is found, do the localization on the client
                        errorMessage = ViewModelAdapter.getResourceString(errorDetails.messageResource);

                        if (ArrayExtensions.hasElements(error.formatData)) {
                            errorMessage = StringExtensions.format(errorMessage, ...error.formatData);
                        }
                    }
                }

                // If localization is not performed on client, displays the localized error message from server. (No support for client side formatting)
                if ((error.ErrorCode === errorMessage || StringExtensions.isNullOrWhitespace(errorMessage))
                    && !StringExtensions.isNullOrWhitespace(error.ExternalLocalizedErrorMessage)) {
                    errorMessage = error.ExternalLocalizedErrorMessage;
                }
            }

            if (StringExtensions.isNullOrWhitespace(errorMessage)) {
                RetailLogger.coreCannotMapErrorCode(error.ErrorCode);
            }

            return errorMessage;
        }

        /**
         * Creates error details entity from an error code.
         * @param {string} The error code.
         * @param {boolean} Log error if not found. Default true.
         * @returns {Model.Entities.IErrorDetails} The error details entity.
         */
        public static resolveError(errorCode: string, logNotFoundError: boolean = true): Model.Entities.IErrorDetails {
            if (StringExtensions.isNullOrWhitespace(errorCode)) {
                return null;
            }

            var errorDetails: Model.Entities.IErrorDetails = null;
            var result: any = ErrorTypeEnum[errorCode.toUpperCase()];

            if (ObjectExtensions.isObject(result)) {
                errorDetails = <Model.Entities.IErrorDetails>result;
            } else if (ObjectExtensions.isString(result)) {
                errorDetails = {
                    messageResource: result
                };
            } else {

                if (logNotFoundError && errorCode === ViewModelAdapter.getResourceString(errorCode)) {
                    RetailLogger.coreCannotMapErrorCode(errorCode);
                }
                errorDetails = {
                    messageResource: errorCode
                };
            }

            return errorDetails;
        }

        /**
         * Serializes errors for the RetailLogger.
         * @param {Model.Entities.Error[]} errors The errors.
         * @return {string} The serialized errors.
         */
        public static serializeErrorsForRetailLogger(errors: Model.Entities.Error[]): string {
            var serializedErrorDetails: string;
            try {
                serializedErrorDetails = JSON.stringify(errors, ["ErrorCode", "ExternalLocalizedErrorMessage"]);
            } catch (exception) {
                var details: string;
                if (!ObjectExtensions.isNullOrUndefined(exception)) {
                    if (!ObjectExtensions.isNullOrUndefined(exception.message)) {
                        details = exception.message;
                    } else {
                        details = String(exception);
                    }
                } else {
                    details = "";
                }
                serializedErrorDetails = `Failed to serialize errors (${details}).`;
            }
            return serializedErrorDetails;
        }
    }
}
