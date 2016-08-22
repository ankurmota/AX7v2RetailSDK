/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

module Commerce.Proxy.Entities {
    "use strict";

    export class AuthenticationProviderEnum {
        public static USERAUTHENTICATION: string = "UserAuthentication";
    }

    /**
     * Enum for describing the activation state on the controller.
     */
    export enum DeviceActivationControllerState {
        PreMessage = 1,
        Deactivated = 2,
        Processing = 3,
        Error = 4,
        Succeeded = 5
    }

    /**
     * Interface for passing activation parameters between controllers.
     */
    export interface IActivationParameters {
        serverUrl?: string;
        deviceId?: string;
        registerId?: string;
        operatorId?: string;
        password?: string;
        errors?: Model.Entities.Error[];  /* Used for showing errors after force log off. */
        skipConnectivityOperation?: boolean;
    }

    /**
     * Interface for holding state between ativation operations.
     */
    export interface IDeviceActivationState {
        serviceUrl?: string;
        operatorId?: string;
        password?: string;
        deviceId?: string;
        registerId?: string;
        hardwareProfile?: Model.Entities.HardwareProfile;
        paymentMerchantInformation?: Model.Entities.PaymentMerchantInformation;
        currentOperation: Observable<Operations.IDeviceActivationOperation>;
        currentOperationStep: Observable<number>;
        skipEncryptionOperation?: boolean;
        forceActivate?: boolean;
        skipConnectivityOperation?: boolean;
    }
}