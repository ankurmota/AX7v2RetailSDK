/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/// <reference path='AADLogonOperation.ts' />
/// <reference path='AnonymousLogonOperation.ts' />
/// <reference path='CheckServerConnectivityOperation.ts' />
/// <reference path='DeviceActivationOperation.ts' />
/// <reference path='DiagnosticsOperation.ts' />
/// <reference path='GetDeviceConfigurationOperation.ts' />
/// <reference path='GetEmployeesOperation.ts' />
/// <reference path='GetHardwareProfileOperation.ts' />
/// <reference path='GetLatestNumberSequenceOperation.ts' />
/// <reference path='GetLocalizationOperation.ts' />
/// <reference path='LogoffOperation.ts' />
/// <reference path='LogonWithDeviceTokenOperation.ts' />
/// <reference path='UpdateServerUrlOperation.ts' />

module Commerce.Operations {
    "use strict";

    export class DeviceActivationSequence {
        private isDeviceActivated: boolean;
        private isInitialDataSynced: boolean;
        private operationState: Model.Entities.IDeviceActivationState;
        private lastOperationName: string;

        constructor(operationState: Model.Entities.IDeviceActivationState, lastOperationName?: string) {
            this.operationState = operationState;
            this.lastOperationName = lastOperationName;
            this.isDeviceActivated = !StringExtensions.isNullOrWhitespace(
                ApplicationStorage.getItem(ApplicationStorageIDs.DEVICE_TOKEN_KEY));
            this.isInitialDataSynced = !StringExtensions.isNullOrWhitespace(
                ApplicationStorage.getItem(ApplicationStorageIDs.INITIAL_SYNC_COMPLETED_KEY));
        }

        public constructActivationSequence(): IDeviceActivationOperation[] {
            var activationSequence: IDeviceActivationOperation[] = [];

            if (Config.aadEnabled) {
                activationSequence = activationSequence.concat(this.constructAADActivationSequence());
            } else {
                activationSequence = activationSequence.concat(this.constructNonAADActivationSequence());
            }

            activationSequence = activationSequence.concat(this.constructInitialSyncDataSequence());
            activationSequence = activationSequence.concat(this.constructLogoffSequence());

            return activationSequence;
        }

        private constructAADActivationSequence(): IDeviceActivationOperation[] {
            var logonSequence: IDeviceActivationOperation[] = [];

            if (this.isDeviceActivated && this.isInitialDataSynced) {
                return logonSequence;
            }

            var checkConnectivityOperation: CheckServerConnectivityOperation =
                new CheckServerConnectivityOperation(this.operationState);

            if (this.isConnectivityCheckRequired()) {
                logonSequence.push(checkConnectivityOperation);
            }

            logonSequence.push(new AADLogonOperation(this.operationState));
            logonSequence.push(new DiagnosticsOperation(this.operationState));

            if (Config.locatorServiceEnabled) {
                // If Locator Service enabled, then CheckServerConnectivityOperation
                // should be executed after UpdateServerUrlOperation since
                // Server Url from the UpdateServerUrlOperation
                logonSequence.splice(0, 1);
                logonSequence.push(new UpdateServerUrlOperation(this.operationState));
                logonSequence.push(checkConnectivityOperation);
            }

            if (!this.isDeviceActivated) {
                logonSequence.push(new DeviceActivationOperation(this.operationState));
            }

            return logonSequence;
        }

        private constructNonAADActivationSequence(): IDeviceActivationOperation[] {
            var logonSequence: IDeviceActivationOperation[] = [];

            if (this.isDeviceActivated && this.isInitialDataSynced) {
                return logonSequence;
            }

            if (this.isConnectivityCheckRequired()) {
                logonSequence.push(new CheckServerConnectivityOperation(this.operationState));
            }

            if (!this.isDeviceActivated) {
                logonSequence.push(new AnonymousLogonOperation(this.operationState));
                logonSequence.push(new DeviceActivationOperation(this.operationState));
            }

            logonSequence.push(new LogonWithDeviceTokenOperation(this.operationState));

            return logonSequence;
        }

        private constructInitialSyncDataSequence(): IDeviceActivationOperation[] {
            var syncDataSequence: IDeviceActivationOperation[] = [];

            if (this.isDeviceActivated && this.isInitialDataSynced) {
                return syncDataSequence;
            } else if (!this.isDeviceActivated) {
                this.lastOperationName = StringExtensions.EMPTY;
            }

            syncDataSequence.splice(0, 0, new GetEmployeesOperation(this.operationState));

            if (this.lastOperationName === Helpers.DeviceActivationHelper.GET_EMPLOYEES_OPERATION_NAME) {
                return syncDataSequence;
            }

            syncDataSequence.splice(0, 0,
                new GetHardwareProfileOperation(this.operationState),
                new GetPaymentMerchantInformationOperation(this.operationState));

            if (this.lastOperationName === Helpers.DeviceActivationHelper.GET_HARDWARE_PROFILE_OPERATION_NAME ||
                this.lastOperationName === Helpers.DeviceActivationHelper.GET_PAYMENT_MERCHANT_INFORMATION_OPERATION_NAME) {
                return syncDataSequence;
            }

            syncDataSequence.splice(0, 0, new GetLocalizationOperation(this.operationState));

            if (this.lastOperationName === Helpers.DeviceActivationHelper.GET_LOCALIZATION_OPERATION_NAME) {
                return syncDataSequence;
            }

            syncDataSequence.splice(0, 0, new GetLatestNumberSequenceOperation(this.operationState));

            if (this.lastOperationName === Helpers.DeviceActivationHelper.GET_LATEST_NUMBER_SEQUENCE_OPERATION_NAME) {
                return syncDataSequence;
            }

            syncDataSequence.splice(0, 0, new GetDeviceConfigurationOperation(this.operationState));

            if (this.lastOperationName === Helpers.DeviceActivationHelper.GET_DEVICE_CONFIGURATION_OPERATION_NAME) {
                return syncDataSequence;
            }

            if (Commerce.Host.instance.application.getApplicationType() !== Commerce.Proxy.Entities.ApplicationTypeEnum.CloudPos) {
                syncDataSequence.splice(0, 0, new GetEnvironmentConfiguration(this.operationState));
            }

            return syncDataSequence;
        }

        private constructLogoffSequence(): IDeviceActivationOperation[] {
            var logoffSequence: IDeviceActivationOperation[] = [];

            if (this.isDeviceActivated && this.isInitialDataSynced) {
                return logoffSequence;
            }

            logoffSequence.push(new LogoffOperation(this.operationState));
            return logoffSequence;
        }

        private isConnectivityCheckRequired(): boolean {
            if ((!Config.aadEnabled && !StringExtensions.isNullOrWhitespace(Config.onlineDatabase)) ||
                this.operationState.skipConnectivityOperation) {
                return false;
            }

            return true;
        }
    }
}