/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Helpers {
    "use strict";

    export class DeviceActivationHelper {

        public static AAD_ACQUIRE_TOKEN_OPERATION_NAME: string = "AADAcquireTokenOperation";
        public static AAD_LOGON_OPERATION_NAME: string = "AADLogonOperation";
        public static ANONYMOUS_LOGON_OPERATION_NAME: string = "AnonymousLogonOperation";
        public static CHECK_SERVER_CONNECTIVITY_OPERATION_NAME: string = "CheckServerConnectivityOperation";
        public static DEVICE_ACTIVATION_OPERATION_NAME: string = "DeviceActivationOperation";
        public static ENCRYPT_DATA_OPERATION_NAME: string = "EncryptDataOperation";
        public static GET_ENVIRONMENT_CONFIGURATION_OPERATION_NAME: string = "GetEnvironmentConfigurationName";
        public static GET_DEVICE_CONFIGURATION_OPERATION_NAME: string = "GetDeviceConfigurationOperation";
        public static GET_EMPLOYEES_OPERATION_NAME: string = "GetEmployeesOperation";
        public static GET_HARDWARE_PROFILE_OPERATION_NAME: string = "GetHardwareProfileOperation";
        public static GET_PAYMENT_MERCHANT_INFORMATION_OPERATION_NAME: string = "GetPaymentMerchantInformationOperation";
        public static GET_LATEST_NUMBER_SEQUENCE_OPERATION_NAME: string = "GetLatestNumberSequenceOperation";
        public static GET_LOCALIZATION_OPERATION_NAME: string = "GetLocalizationOperation";
        public static LOGOFF_OPERATION_NAME: string = "LogoffOperation";
        public static LOGON_WITH_DEVICE_TOKEN_OPERATION_NAME: string = "LogonWithDeviceTokenOperation";
        public static UPDATE_SERVER_URL_OPERATION_NAME: string = "UpdateServerUrlOperation";
        public static DIAGNOSTICS_OPERATION_NAME: string = "DiagnosticsOperation";
        public static DEVICE_ACTIVATION_OPERATIONBASE_NAME: string = "DeviceActivationOperationBase";

        public static DEVICE_ACTIVATION_VIEW_NAME: string = "DeviceActivationView";
        public static GUIDED_ACTIVATION_VIEW_NAME: string = "GuidedActivationView";
        public static DEVICE_ACTIVATION_GET_STARTED_VIEW_NAME: string = "GetStartedView";

        public static DEVICE_ACTIVATION_PROCESS_VIEW_NAME: string = "DeviceActivationProcessView";

        private static DEVICE_ACTIVATION_VIEWS: string[] = [
            DeviceActivationHelper.DEVICE_ACTIVATION_VIEW_NAME,
            DeviceActivationHelper.DEVICE_ACTIVATION_GET_STARTED_VIEW_NAME,
            DeviceActivationHelper.DEVICE_ACTIVATION_PROCESS_VIEW_NAME,
            DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME
        ];

        public static isInDeviceActivationProcess(): boolean {
            for (var i: number = 0; i < DeviceActivationHelper.DEVICE_ACTIVATION_VIEWS.length; i++) {
                if (Commerce.ViewModelAdapter.isInView(DeviceActivationHelper.DEVICE_ACTIVATION_VIEWS[i])) {
                    return true;
                }
            }

            return false;
        }

        public static isActivationConfigProvided(): boolean {
            // Returns true if:
            // 1) Device ID is provided via config file, URI arguments or application storage, and
            // 2) Register ID is provided via config file, URI arguments or application storage
            return (!StringExtensions.isNullOrWhitespace(DeviceActivationHelper.argumentDeviceNumber)
                || !StringExtensions.isNullOrWhitespace(DeviceActivationHelper.storedDeviceNumber)
                && (!StringExtensions.isNullOrWhitespace(DeviceActivationHelper.argumentRegisterNumber)
                || !StringExtensions.isNullOrWhitespace(DeviceActivationHelper.storedRegisterNumber)));
        }

        public static get storedDeviceNumber(): string {
            return ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ID_KEY);
        }

        public static get argumentDeviceNumber(): string {
            return Host.instance.configurationProvider.getArgumentValue(ApplicationArgumentId.DEVICE_NUMBER);
        }

        public static get storedRegisterNumber(): string {
            return ApplicationStorage.getItem(ApplicationStorageIDs.REGISTER_ID_KEY);
        }

        public static get argumentRegisterNumber(): string {
            return Host.instance.configurationProvider.getArgumentValue(ApplicationArgumentId.TERMINAL_NUMBER);
        }

        public static areStoredDeviceTerminalDifferentFromArguments(): boolean {
            return DeviceActivationHelper.isStoredValueDifferentFromArgument(DeviceActivationHelper.storedDeviceNumber,
                                                                                DeviceActivationHelper.argumentDeviceNumber)
                || DeviceActivationHelper.isStoredValueDifferentFromArgument(DeviceActivationHelper.storedRegisterNumber,
                                                                                DeviceActivationHelper.argumentRegisterNumber);
        }

        public static navigateToActivationPage(activationParameters?: string): void {
            if (DeviceActivationHelper.isActivationConfigProvided()) {
                // If server Url, device, and register number are already provided, navigate to manual entry
                ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.DEVICE_ACTIVATION_VIEW_NAME, activationParameters);
            } else {
                ViewModelAdapter.navigate(Helpers.DeviceActivationHelper.GUIDED_ACTIVATION_VIEW_NAME, activationParameters);
            }
        }

        public static startDeviceActivation(): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_ACTIVATION_COMPLETED, "");
        }

        public static completeDeviceActivation(): void {
            ApplicationStorage.setItem(ApplicationStorageIDs.DEVICE_ACTIVATION_COMPLETED, "true");
        }

        public static isDeviceActivationCompleted(): boolean {
            return ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_ACTIVATION_COMPLETED) === "true";
        }

        private static isStoredValueDifferentFromArgument(storedValue: string, argumentValue: string): boolean {

            if (StringExtensions.isNullOrWhitespace(storedValue)) {
                return false;
            }

            if (StringExtensions.isNullOrWhitespace(argumentValue)) {
                return false;
            }
            return StringExtensions.compare(storedValue, argumentValue, /*ignoreCase:*/true) !== 0;
        }
    }
}